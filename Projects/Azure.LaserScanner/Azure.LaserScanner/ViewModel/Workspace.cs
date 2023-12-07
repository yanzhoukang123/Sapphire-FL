/************************************************************************

   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the New BSD
   License (BSD) as published at http://avalondock.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up AvalonDock in Extended WPF Toolkit Plus at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like facebook.com/datagrids

  **********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Win32.SafeHandles;
using Microsoft.Win32;
using System.IO;
//using System.IO.Ports;
using System.Windows;
//using System.Windows.Controls;  //ContentControl
using System.Windows.Media.Imaging;
using System.ComponentModel;    //BackgroundWorker
using System.Printing;          //PrintCapabilities
using System.Threading;         //Thead
using System.Text.RegularExpressions;   //Regex
using System.Windows.Controls;  //PrintDialog
using System.Windows.Media;     //DrawingVisual
using System.Globalization;     //CultureInfo
using System.Windows.Threading; //DispatcherTimer
using System.Runtime.InteropServices;
using Azure.EthernetCommLib;
using Azure.Configuration.Settings;
using Azure.Image.Processing;
using Azure.Ipp.Imaging;
using Azure.ImagingSystem;
using Azure.LaserScanner;
using Azure.LaserScanner.View;
using Azure.Utilities;
using Azure.WPF.Framework;
using DrawToolsLib;
using TaskDialogInterop;
using SimpleWifi;
using System.Net;
using System.Net.NetworkInformation;

namespace Azure.LaserScanner.ViewModel
{
    // These value must match application tab index
    public enum ApplicationTabType
    {
        Imaging = 0,
        Gallery = 1,
        Analysis = 2,
        Annotation = 3,
        Settings = 4,
        Help = 5,
        ItemCount = 6,
    }
    public enum GalleryPanelType
    {
        ContrastPanel,
        RoiPanel,
        TransformPanel,
        ResizePanel,
        AnnotationPanel,
        ImageInfoPanel,
    }


    /*public enum ImagingType
    {
        None,
        Fluorescence,
        Chemiluminescence,
        PhosphorImaging,
        Visible
    }*/

    class Workspace : ViewModelBase, IDisposable
    {
        public static readonly string[] IntensityLevels = new[] { "0", "L1", "L2", "L3", "L4", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };

        // Flag: Has Dispose already been called?
        bool disposed = false;
        // Instantiate a SafeHandle instance.
        SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);

        #region Private data...

        string _BitmapScalingMode = "HighQuality";
        bool _IsEnableMainWinContent = true;
        string _InitializationMessage = string.Empty;

        private bool _HasSecurePassword = false;

        private ApplicationTabType _SelectedApplicationTab;
        private ImagingType _SelectedImagingType;
        private GalleryPanelType _SelectedGalleryPanel = GalleryPanelType.ContrastPanel;
        private FileViewModel _ActiveDocument = null;
        private AbstractPaneViewModel activePane = null;

        private const int _MaxOpenFileAllowed = 10;
        private int _FileNameCount = 0;
        private int _FileNameSetCount = 0;

        private bool _IsLoading = false;
        private bool _Is8bpp = false;
        private bool _Is16bpp = false;
        //private bool _Is32bppImage = false;

        private LoadingAnimationManager _LoadingAnimManager = null;

        private RelayCommand _OpenCommand = null;
        private RelayCommand _NewCommand = null;
        private RelayCommand _ShowOSKBCommand = null;

        // Drawing commands
        //private RelayCommand _DrawingUndoCommand = null;
        //private RelayCommand _DrawingRedoCommand = null;
        //private RelayCommand _DrawingDeleteCommand = null;

        //private bool _IsActiveDocument = false;
        //private double _DefaultZoomFactor = 0.1;
        //private bool _IsDarkroom = false;
        //private bool _IsSettings = false;

        //private ManualContrast _ManualContrast = null;
        //private ObservableCollection<LineWidthType> _LineWidthOptions = new ObservableCollection<LineWidthType>();

        private bool _IsProcessingContrast = false;

        //private bool _IsImagingMode = true;
        //private bool _IsScanningMode = true;
        //private bool _IsCameraMode = false;
        private bool _IsPreparingToScan = false;
        private bool _IsReadyScanning = false;
        private bool _IsScanning = false;
        private bool _IsMotorAlive = false;

        private string _DoorStatus = string.Empty;

        private bool _IsAdjustmentsChecked = false;
        private bool _IsCropChecked = false;
        private bool _IsRotateChecked = false;
        private bool _IsResizeChecked = false;

        //private DispatcherTimer _CaptureDispatcherTimer = new DispatcherTimer();
        private DateTime _CaptureStartTime;
        private string _StatusTextProgress = string.Empty;
        private string _EstimatedTimeRemaining = "";
        private double _EstimatedCaptureTime = 0.0;
        private double _PercentCompleted = 0.0;

        private MotorViewModel _MotorViewModel = null;
        private ApdViewModel _ApdViewModel = null;
        private NewParameterSetupViewModel _NewParamViewModel = null;
        //private TransportLockViewModel _TransportLockViewModel = null;
        //private ZAutomaticallyFocalViewModel _ZAutomaticallyFocal = null;
        private EthernetController _EthernetController;
        private double _EthernetTransactionRate;

        private FluorescenceViewModel _FluorescenceVM = null;
        private PhosphorViewModel _PhosphorVM = null;
        private SettingsViewModel _SettingsViewModel = null;
        private ResizeViewModel _ResizeVM = null;
        private TransformViewModel _ImageTransformVm = null;
        private CopyPasteViewModel _ImageCopyPasteVm = null;

        private bool _IsImageClipboard = false;
        private ImageClipboard _ImageClipboard = new ImageClipboard();

        private bool _IsFluorescenceImagingVisible = false;
        private bool _IsPhosphorImagingVisible = false;
        //private double _AbsFocusPosition = 0.0;

        protected Logger _Logger = null;
        protected string _LogFilePath;

        private bool _IsCollapsiblePanelExpanded = false;
        private ScaleBarViewModel _ScaleBarVm = null;
        private Dictionary<LaserChannels, int> _LaserChannelTypeList;
        private ObservableCollection<LaserTypes> _LaserOptions = null;
        private LaserModule _LaserModuleL1;
        private LaserModule _LaserModuleR1;
        private LaserModule _LaserModuleR2;

        private ManualAlignViewModel _ManualAlignVm;

        #endregion

        #region Constructors...

        public Workspace()
        {
            _BitmapScalingMode = SettingsManager.ConfigSettings.BitmapScalingMode;
            if (string.IsNullOrEmpty(_BitmapScalingMode))
            {
                _BitmapScalingMode = "HighQuality";
            }

            //this.Zoom = 1.0;

            //
            // Initialize the 'Open Documents' pane view-model.
            //
            //this.OpenDocumentsPaneViewModel = new OpenDocumentsPaneViewModel();
            //
            // Add view-models for panes to the 'Panes' collection.
            //
            this.Panes = new ObservableCollection<AbstractPaneViewModel>();
            //this.Panes.Add(this.OpenDocumentsPaneViewModel);

            this.Files = new ObservableCollection<FileViewModel>();

            _EthernetController = new EthernetController();
            _EthernetController.OnDataRateChanged += EthernetController_OnDataRateChanged;

            _MotorViewModel = new MotorViewModel();
            //_CameraViewModel = new ChemiViewModel();
            //_ScannerViewModel = new ScannerViewModel();
            _ApdViewModel = new ApdViewModel();
            _NewParamViewModel = new NewParameterSetupViewModel();
            _FluorescenceVM = new FluorescenceViewModel();
            _PhosphorVM = new PhosphorViewModel();

            _SettingsViewModel = new SettingsViewModel();

            // image capturing status
            //_CaptureDispatcherTimer.Tick += new EventHandler(_DispatcherTimer_Tick);
            //_CaptureDispatcherTimer.Interval = new TimeSpan(0, 0, 1);

            // Default to Imaging tab and Fluorescence imaging.
            //_SelectedApplicationTab = ApplicationTabType.Imaging;
            //_SelectedImagingType = ImagingType.Fluorescence;

            _MultiplexVm = new MultiplexViewModel();
            _ImageCopyPasteVm = new CopyPasteViewModel();
            _ImageTransformVm = new TransformViewModel();
            _ResizeVM = new ResizeViewModel();
            _ScaleBarVm = new ScaleBarViewModel();
            _LaserChannelTypeList = new Dictionary<LaserChannels, int>();
            _LaserOptions = new ObservableCollection<LaserTypes>();

            _LaserModuleL1 = new LaserModule();
            _LaserModuleR1 = new LaserModule();
            _LaserModuleR2 = new LaserModule();

            _ManualAlignVm = new ManualAlignViewModel();
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                handle.Dispose();
                // Free any other managed objects here.
                //

            }

            // Free any unmanaged objects here.
            //

            disposed = true;
            // Call base class implementation.
            base.Dispose(disposing);
        }

        public string Title { get; set; }

        static Workspace _this = new Workspace();
        public static Workspace This
        {
            get { return _this; }
            set { _this = value; }
        }

        public string BitmapScalingMode
        {
            get { return _BitmapScalingMode; }
            set
            {
                _BitmapScalingMode = value;
                RaisePropertyChanged("BitmapScalingMode");
            }
        }

        public bool IsEnableMainWinContent
        {
            get
            {
                return _IsEnableMainWinContent;
            }
            set
            {
                if (_IsEnableMainWinContent != value)
                {
                    _IsEnableMainWinContent = value;
                    RaisePropertyChanged("IsEnableMainWinContent");
                    //RaisePropertyChanged("IsScannerReady");
                }
            }
        }

        /*public double Zoom
        {
            get { return this._Zoom; }
            set
            {
                if (value.Equals(this._Zoom)) return;
                this._Zoom = value;
                this.RaisePropertyChanged("Zoom");
            }
        }*/

        //private ObservableCollection<FileViewModel> _Files = new ObservableCollection<FileViewModel>();
        //private ReadOnlyObservableCollection<FileViewModel> _ReadOnyFiles = null;
        //public ReadOnlyObservableCollection<FileViewModel> Files
        //{
        //    get
        //    {
        //        if (_ReadOnyFiles == null)
        //        {
        //            _ReadOnyFiles = new ReadOnlyObservableCollection<FileViewModel>(_Files);
        //        }
        //        return _ReadOnyFiles;
        //    }
        //}

        /// <summary>
        /// View-models for documents.
        /// </summary>
        public ObservableCollection<FileViewModel> Files
        {
            get;
            private set;
        }

        /// <summary>
        /// View-models for panes.
        /// </summary>
        public ObservableCollection<AbstractPaneViewModel> Panes
        {
            get;
            private set;
        }

        /// <summary>
        /// View-model for the active pane.
        /// </summary>
        public AbstractPaneViewModel ActivePane
        {
            get
            {
                return activePane;
            }
            set
            {
                if (activePane == value)
                {
                    return;
                }

                activePane = value;

                RaisePropertyChanged("ActivePane");
            }
        }

        /// <summary>
        /// View-model for the 'Open Documents' pane.
        /// </summary>
        //public OpenDocumentsPaneViewModel OpenDocumentsPaneViewModel
        //{
        //    get;
        //    private set;
        //}

        public Action CloseAction { get; set; }

        public MainWindow Owner { get; set; }
        public string CompanyName { get; set; }
        public string ProductName { get; set; }
        public string AppDataPath { get; set; }
        public string ProductVersion { get; set; }
        public string CameraFirmware { get; set; }
        public string FWVersion { get; set; }
        public string HWversion { get; set; }
        public string SystemSN { get; set; }
        //Indicates the device status, while Fasle indicates that the optical module power off button has been manually pressed. At this time, the software pop-up reminds the device that it has been powered off and needs to restart the software. The software is always set to gray and is not allowed to be used
        //表示设备状态，Fasle表示手动按下了光学模块下电按钮，此时软件弹框提醒设备已经下电请重启软件，软件始终置为灰色不允许使用
        public bool DeviceStatus { get; set; } = true;
        public string DefaultHWversion { get; set; } = "1.1.0.0";
        public string LEDVersion { get; set; }
        public bool IsFluorescenceImagingVisible
        {
            get { return _IsFluorescenceImagingVisible; }
            set
            {
                _IsFluorescenceImagingVisible = value;
                RaisePropertyChanged("IsFluorescenceImagingVisible");
            }
        }
        public bool IsPhosphorImagingVisible
        {
            get { return _IsPhosphorImagingVisible; }
            set
            {
                _IsPhosphorImagingVisible = value;
                RaisePropertyChanged("IsPhosphorImagingVisible");
            }
        }

        private WindowState _curWindowState;
        public WindowState CurWindowState
        {
            get
            {
                return _curWindowState;
            }
            set
            {
                _prevWindowState = _curWindowState;

                _curWindowState = value;
                RaisePropertyChanged("CurWindowState");
            }
        }

        private WindowState _prevWindowState;
        public WindowState PrevWindowState
        {
            get
            {
                return _prevWindowState;
            }
            set
            {
                _prevWindowState = value;
                RaisePropertyChanged("PrevWindowState");
            }
        }

        private void EthernetController_OnDataRateChanged()
        {
            EthernetDataRate = Math.Round(_EthernetController.ReadingRate);
        }

        internal bool ConnectEthernetSlave()
        {
            return _EthernetController.Connect(new System.Net.IPAddress(new byte[] { 192, 168, 1, 110 }), 5000, 8000, new System.Net.IPAddress(new byte[] { 192, 168, 1, 100 }));
        }

        internal EthernetController EthernetController
        {
            get { return _EthernetController; }
        }

        public double EthernetDataRate
        {
            get { return _EthernetTransactionRate; }
            set
            {
                if (_EthernetTransactionRate != value)
                {
                    _EthernetTransactionRate = value;
                    RaisePropertyChanged(nameof(EthernetDataRate));
                }
            }
        }

        public bool IsMotorAlive
        {
            get { return _IsMotorAlive; }
            set { _IsMotorAlive = value; }
        }

        public bool IsScannerConnected
        {
            get
            {
                bool bResult = false;
                if (SettingsManager.ConfigSettings.IsSimulationMode)
                {
                    bResult = true;
                }
                else
                {
                    if (Workspace.This.EthernetController != null && Workspace.This.EthernetController.IsConnected)
                    {
                        bResult = true;
                    }
                }
                return bResult;
            }
        }

        // Detected lasers
        public int LaserL1 { get; set; }
        public int LaserR1 { get; set; }
        public int LaserR2 { get; set; }
        public IvSensorType SensorML1 { get; set; }
        public IvSensorType SensorMR1 { get; set; }
        public IvSensorType SensorMR2 { get; set; }
        public string LaserSNL1 { get; set; }
        public string LaserSNR1 { get; set; }
        public string LaserSNR2 { get; set; }
        public bool HasDeviceProperties { get; set; }
        public LaserModule LaserModuleL1
        {
            get { return _LaserModuleL1; }
            set
            {
                _LaserModuleL1 = value;
                RaisePropertyChanged("LaserModuleL1");
            }
        }
        public LaserModule LaserModuleR1
        {
            get { return _LaserModuleR1; }
            set
            {
                _LaserModuleR1 = value;
                RaisePropertyChanged("LaserModuleR1");
            }
        }
        public LaserModule LaserModuleR2
        {
            get { return _LaserModuleR2; }
            set
            {
                _LaserModuleR2 = value;
                RaisePropertyChanged("LaserModuleR2");
            }
        }
        public ManualAlignViewModel ManualAlignVm
        {
            get { return _ManualAlignVm; }
            set
            {
                _ManualAlignVm = value;
                RaisePropertyChanged("ManualAlignVm");
            }
        }

        public Dictionary<LaserChannels, int> LaserChannelTypeList
        {
            get { return _LaserChannelTypeList; }
            set { _LaserChannelTypeList = value; }
        }
        public ObservableCollection<LaserTypes> LaserOptions
        {
            get { return _LaserOptions; }
            set { _LaserOptions = value; }
        }

        #region internal void LogMessage(string msg)
        /// <summary>
        /// Log a message to the attached logger. See method <see cref="SetLogger"/>
        /// to attach a logger.
        /// </summary>
        /// <param name="msg"></param>
        public void LogMessage(string msg)
        {
            if (_Logger == null) return;

            _Logger.LogMessage(msg);
        }
        #endregion

        #region internal void LogException(string header, Exception ex)
        /// <summary>
        /// Log an exception to the attached logger. See method <see cref="SetLogger"/>
        /// to attach a logger.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="ex"></param>
        internal void LogException(string header, Exception ex)
        {
            if (_Logger == null) return;

            _Logger.LogException(header, ex);
        }
        #endregion

        public void LoggerSetup()
        {
            //
            // Create logger
            //
            _LogFilePath = Path.Combine(AppDataPath, ProductName + ".log");
            _Logger = new Logger();
            _Logger.Open(_LogFilePath);
            _Logger.SuppressLoggingExceptionEvents = true;
            //_CommandMediator.SetLogger(_Logger);
            //_CommandMediator.Exception += new ExceptionDelegate(_CommandMediator_Exception);
            //_CommandMediator.StateChanged += new StateChangedDelegate(_CommandMediator_StateChanged);
            _Logger.LogAppConfigSettings();
        }
        public Logger Logger
        {
            get { return _Logger; }
        }

        #region ActiveDocument

        //public event EventHandler ActiveDocumentChanged;

        public FileViewModel ActiveDocument
        {
            get { return _ActiveDocument; }
            set
            {
                if (_ActiveDocument != value)
                {
                    _ActiveDocument = value;

                    /*#region === Work-around ===
                    // Work-around for RGB image crop application crash
                    // For some reason this got rid of the access violation error
                    // TODO: find a better solution
                    // NOTE: cSeries error, commented out because not seeing the same error in Sapphire
                    if (_ActiveDocument != null)
                    {
                        if (_ActiveDocument.IsRgbImageCropped)
                        {
                            if (_ActiveDocument.Image.Format.BitsPerPixel == 48)
                            {
                                int iColorGradation = 3;
                                WriteableBitmap dspBitmap = _ActiveDocument.Image;
                                dspBitmap = ImageProcessing.Scale(dspBitmap,
                                                                  _ActiveDocument.ImageInfo.MixChannel.BlackValue,
                                                                  _ActiveDocument.ImageInfo.MixChannel.WhiteValue,
                                                                  _ActiveDocument.ImageInfo.MixChannel.GammaValue,
                                                                  _ActiveDocument.ImageInfo.MixChannel.IsInvertChecked,
                                                                  _ActiveDocument.ImageInfo.MixChannel.IsSaturationChecked,
                                                                  _ActiveDocument.ImageInfo.SaturationThreshold,
                                                                  iColorGradation);
                                dspBitmap = null;
                                _ActiveDocument.IsRgbImageCropped = false;
                            }
                        }
                    }
                    #endregion*/

                    RaisePropertyChanged("ActiveDocument");
                    RaisePropertyChanged("IsActiveDocument");
                    RaisePropertyChanged("IsRgbImage");
                    //RaisePropertyChanged("Is8bpp");
                    //RaisePropertyChanged("Is32bppImage");
                    RaisePropertyChanged("IsCopyAndPasteAllowed");
                    RaisePropertyChanged("IsChemiMarkerSet");

                    if (_ActiveDocument != null)
                    {
                        if (_IsResizeChecked)
                        {
                            if (_ResizeVM != null)
                            {
                                _ResizeVM.ActiveDocument = _ActiveDocument;
                            }
                        }
                        else
                        {
                            if (_ResizeVM != null)
                            {
                                _ResizeVM.ActiveDocument = null;
                            }
                        }
                        if (_ManualAlignVm != null)
                        {
                            if (_ActiveDocument.IsRgbImage)
                            {
                                _ManualAlignVm.SelectedChannel = ImageChannelType.Red;
                                _ManualAlignVm.Is4ChannelImage = _ActiveDocument.Is4ChannelImage;
                            }
                        }
                    }
                    else
                    {
                        if (IsResizeChecked)
                        {
                            _ResizeVM.ActiveDocument = null;
                        }
                    }

                    //if (ActiveDocumentChanged != null)
                    //{
                    //    ActiveDocumentChanged(this, EventArgs.Empty);
                    //}
                }
            }
        }

        #endregion

        #region public bool IsActiveDocument
        public bool IsActiveDocument
        {
            get
            {
                bool bIsActiveDocument = false;

                if (Files.Count > 0 && ActiveDocument != null)
                {
                    {
                        bIsActiveDocument = true;
                    }
                }

                return bIsActiveDocument;
            }
        }
        #endregion

        #region public bool NewDocument(WriteableBitmap image, ImageInfo imageInfo, string title, bool bIsCropped, bool bIsGetMinMax = true)
        /// <summary>
        /// Add new document.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="imageInfo"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public bool NewDocument(WriteableBitmap image, ImageInfo imageInfo, string title, bool bIsCropped, bool bIsGetMinMax = true)
        {
            bool bResult = false;

            FileViewModel fileViewModel = new FileViewModel(image, imageInfo, title, bIsCropped, bIsGetMinMax);
            if (fileViewModel != null)
            {
                if (Files.Count == 0)
                {
                    if (!IsCollapsiblePanelExpanded)
                        IsCollapsiblePanelExpanded = true;
                }

                Files.Add(fileViewModel);        // Add to the end
                ActiveDocument = fileViewModel;
                //ActiveDocument = Files.Last();
                ActiveDocument.IsDirty = true;
                //ActiveDocument.Title = ActiveDocument.FileName; // Update title with IsDirty flag set
                bResult = true;
            }
            return bResult;
        }
        #endregion

        #region public bool NewDocument(WriteableBitmap image, ImageInfo imageInfo, string title, string filePath, bool bIsGetMinMax = true)
        /// <summary>
        /// Add new document
        /// </summary>
        /// <param name="image"></param>
        /// <param name="imageInfo"></param>
        /// <param name="title"></param>
        /// <param name="filePath"></param>
        /// <param name="bIsGetMinMax"></param>
        /// <returns></returns>
        public bool NewDocument(WriteableBitmap image, ImageInfo imageInfo, string title, string filePath, bool bIsSaveAsPUB, bool bIsGetMinMax = true)
        {
            bool bResult = false;

            if (imageInfo != null)
            {
                // Enable saturation display by default (automatically check the 'Saturation' button)
                imageInfo.IsSaturationChecked = true;
                // Enable auto-contrast by default ('Auto-Contrast' each image channel)
                if (imageInfo.RedChannel.LaserIntensity > 0) { imageInfo.RedChannel.IsAutoChecked = true; }
                if (imageInfo.GreenChannel.LaserIntensity > 0) { imageInfo.GreenChannel.IsAutoChecked = true; }
                if (imageInfo.BlueChannel.LaserIntensity > 0) { imageInfo.BlueChannel.IsAutoChecked = true; }
                if (imageInfo.GrayChannel.LaserIntensity > 0) { imageInfo.GrayChannel.IsAutoChecked = true; }
                if (imageInfo.MixChannel.LaserIntensity > 0) { imageInfo.MixChannel.IsAutoChecked = true; }
            }

            FileViewModel fileViewModel = new FileViewModel(image, imageInfo, title, false, bIsGetMinMax);
            if (fileViewModel != null)
            {
                if (fileViewModel.IsRgbImage)
                {
                    fileViewModel.IsDisplayOverAll = true;
                }
                // Enable auto-contrast by default (check the 'Auto-Contrast' button)
                fileViewModel.IsAutoContrast = true;

                if (Files.Count == 0)
                {
                    if (!IsCollapsiblePanelExpanded)
                        IsCollapsiblePanelExpanded = true;
                }

                Files.Add(fileViewModel);        // Add to the end
                ActiveDocument = fileViewModel;
                ActiveDocument.FilePath = filePath;
                if (!string.IsNullOrEmpty(filePath))
                    ActiveDocument.IsDirty = false;
                else
                    ActiveDocument.IsDirty = true;
                bResult = true;
            }

            try
            {
                // Save as PUB
                if (bIsSaveAsPUB)
                {
                    SaveFileAsPub(ActiveDocument);
                }
            }
            catch
            {
                throw;
            }

            return bResult;
        }
        #endregion

        public ScaleBarViewModel ScaleBarVm
        {
            get { return _ScaleBarVm; }
            set
            {
                if (_ScaleBarVm != value)
                {
                    _ScaleBarVm = value;
                    RaisePropertyChanged("ScaleBarVm");
                }
            }
        }

        public int MaxOpenFileAllowed
        {
            get { return _MaxOpenFileAllowed; }
        }

        public int FileNameCount
        {
            get { return _FileNameCount; }
            set { _FileNameCount = value; }
        }

        public int FileNameSetCount
        {
            get { return _FileNameSetCount; }
            set { _FileNameSetCount = value; }
        }

        /// <summary>
        /// Software production version string
        /// </summary>
        /*public string ProductVersion
        {
            get
            {
                string productVersion = string.Empty;

                if (Owner != null)
                {
                    productVersion = Owner.ProductVersion;
                }
                return productVersion;
            }
        }*/

        public bool Is8bpp
        {
            get
            {
                if (ActiveDocument != null)
                {
                    int bpp = ActiveDocument.Image.Format.BitsPerPixel;
                    _Is8bpp = (bpp == 8 || bpp == 24 || bpp == 32) ? true : false;
                }
                return _Is8bpp;
            }
            set
            {
                if (_Is8bpp != value)
                {
                    _Is8bpp = value;
                    RaisePropertyChanged("Is8bpp");
                }
            }
        }

        public bool Is16bpp
        {
            get
            {
                if (ActiveDocument != null)
                {
                    int bpp = ActiveDocument.Image.Format.BitsPerPixel;
                    _Is16bpp = (bpp == 16 || bpp == 48) ? true : false;
                }
                return _Is16bpp;
            }
            set
            {
                if (_Is16bpp != value)
                {
                    _Is16bpp = value;
                    RaisePropertyChanged("Is16bpp");
                }
            }
        }

        //NO longer use 32bpp buffer to store EDR image
        /*public bool Is32bppImage
        {
            get
            {
                if (ActiveDocument != null)
                {
                    int bpp = ActiveDocument.Image.Format.BitsPerPixel;
                    _Is32bppImage = (bpp == 32) ? true : false;
                }
                return _Is32bppImage;
            }
            set
            {
                if (_Is32bppImage != value)
                {
                    _Is32bppImage = value;
                    RaisePropertyChanged("Is32bppImage");
                }
            }
        }*/

        public bool IsShowCloseAll
        {
            get
            {
                bool bIsShowCloseAll = false;
                if (this.Files != null)
                {
                    if (this.Files.Count > 1)
                    {
                        bIsShowCloseAll = true;
                    }
                }
                return bIsShowCloseAll;
            }
        }

        #region public bool IsRgbImage
        //private bool _IsRgbImage = false;
        public bool IsRgbImage
        {
            get
            {
                bool bResult = false;
                if (ActiveDocument != null)
                {
                    bResult = ActiveDocument.IsRgbImage;
                }
                return bResult;
            }
        }
        #endregion

        public bool IsProcessingContrast
        {
            get { return _IsProcessingContrast; }
            set
            {
                if (_IsProcessingContrast != value)
                {
                    _IsProcessingContrast = value;
                    RaisePropertyChanged("IsProcessingContrast");
                }
            }
        }

        public bool IsCollapsiblePanelExpanded
        {
            get { return _IsCollapsiblePanelExpanded; }
            set
            {
                _IsCollapsiblePanelExpanded = value;
                RaisePropertyChanged("IsCollapsiblePanelExpanded");
            }
        }

        #region public bool IsBusyIndicator
        private bool _IsBusyIndicator = false;
        public bool IsBusyIndicator
        {
            get { return _IsBusyIndicator; }
            set
            {
                if (_IsBusyIndicator != value)
                {
                    _IsBusyIndicator = value;
                    RaisePropertyChanged("IsBusyIndicator");
                }
            }
        }
        #endregion

        #region public string BusyIndicatorContent
        private string _BusyIndicatorContent = "Loading....";
        public string BusyIndicatorContent
        {
            get { return _BusyIndicatorContent; }
            set
            {
                if (_BusyIndicatorContent != value)
                {
                    _BusyIndicatorContent = value;
                    RaisePropertyChanged("BusyIndicatorContent");
                }
            }
        }
        #endregion

        public System.Reflection.Assembly EntryAssembly
        {
            get
            {
                // or any other assembly you want
                return System.Reflection.Assembly.GetExecutingAssembly();
            }
        }


        public void EnableBusyIndicator(string content)
        {
            BusyIndicatorContent = content;
            IsBusyIndicator = true;
        }

        public void DisableBusyIndicator()
        {
            BusyIndicatorContent = string.Empty;
            IsBusyIndicator = false;
        }

        private bool _IsBusySavingFile = false;
        public bool IsBusySavingFile
        {
            get { return _IsBusySavingFile; }
            set
            {
                _IsBusySavingFile = value;
                RaisePropertyChanged("IsBusySavingFile");
            }
        }

        #region public void StartWaitAnimation()
        /// <summary>
        /// Start animation window thread.
        /// </summary>
        /// <param name="content"></param>
        public void StartWaitAnimation(string content)
        {
            // Center the animation window in the center
            // of the dock manager grid container
            //var element = _Owner as Window;
            System.Windows.Point relativePoint = new System.Windows.Point(0, 0);
            double width = 0.0;
            double height = 0.0;
            /*
            if (SelectedApplicationTab == ApplicationTabType.Gallery)   // Gallery tab
            {
                relativePoint = Owner.DockManagerContainer.TransformToAncestor(Owner).Transform(new Point(0, 0));
                width = Owner.DockManagerContainer.ActualWidth;
                height = Owner.DockManagerContainer.ActualHeight;
            }
            else if (SelectedApplicationTab == ApplicationTabType.Fluorescence)  // Fluorescence tab
            {
                relativePoint = Owner._FluorescenceContainer.TransformToAncestor(Owner).Transform(new Point(0, 0));
                width = Owner._FluorescenceContainer.ActualWidth;
                height = Owner._FluorescenceContainer.ActualHeight;
            }
            else if (SelectedApplicationTab == ApplicationTabType.Chemiluminescence)  // Chemiluminescence tab
            {
                relativePoint = Owner._ChemiContainer.TransformToAncestor(Owner).Transform(new Point(0, 0));
                width = Owner._FluorescenceContainer.ActualWidth;
                height = Owner._FluorescenceContainer.ActualHeight;
            }
            else if (SelectedApplicationTab == ApplicationTabType.PhosphorImaging)  // PhosphorImaging tab
            {
                relativePoint = Owner._PhosphorContainer.TransformToAncestor(Owner).Transform(new Point(0, 0));
                width = Owner._FluorescenceContainer.ActualWidth;
                height = Owner._FluorescenceContainer.ActualHeight;
            }
            var location = new Point(relativePoint.X, relativePoint.Y);
            */
            width = Owner.ActualWidth;
            height = Owner.ActualHeight;
            System.Windows.Point location = Owner.PointToScreen(new System.Windows.Point(0, 0));

            //var location = element.PointToScreen(new Point(0, 0));
            //var location = new Point(element.Top, element.Left);
            //double width = element.ActualWidth;
            //double height = element.ActualHeight;
            //width = Owner.DockManagerContainer.ActualWidth;
            //eight = Owner.DockManagerContainer.ActualHeight;

            // Start animation
            _LoadingAnimManager = new LoadingAnimationManager(location, new System.Windows.Size(width, height), content);
            _LoadingAnimManager.BeginWaiting();
        }
        #endregion

        #region public void StopWaitAnimation()
        /// <summary>
        /// Close animation window thread.
        /// </summary>
        public void StopWaitAnimation()
        {
            if (_LoadingAnimManager != null)
            {
                // Close the animation window
                _LoadingAnimManager.EndWaiting();
                // Make sure the animation is closed
                while (_LoadingAnimManager.Thread.IsAlive)
                {
                    Thread.Sleep(100);
                    _LoadingAnimManager.EndWaiting();
                }
            }
        }
        #endregion


        #region NewCommand
        public ICommand NewCommand
        {
            get
            {
                if (_NewCommand == null)
                {
                    _NewCommand = new RelayCommand((p) => OnNew(p), (p) => CanNew(p));
                }

                return _NewCommand;
            }
        }

        private bool CanNew(object parameter)
        {
            return true;
        }

        private void OnNew(object parameter)
        {
            Files.Add(new FileViewModel());  // Add to the end
            ActiveDocument = Files.Last();   // Make the last item the active document
        }

        #endregion 

        #region OpenCommand
        public ICommand OpenCommand
        {
            get
            {
                if (_OpenCommand == null)
                {
                    _OpenCommand = new RelayCommand((p) => OnOpen(p), (p) => CanOpen(p));
                }

                return _OpenCommand;
            }
        }

        private bool CanOpen(object parameter)
        {
            return !_IsLoading;
        }

        private void OnOpen(object parameter)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "TIFF Files (*.tif;*.tiff)|*.tif;*.tiff|JPEG Files (*.jpg;*.jpeg)|*.jpg;*.jpeg|BMP Files (*.bmp)|*.bmp|All Files|*.*";
            dlg.Title = "Open File";
            dlg.CheckFileExists = true;
            dlg.CheckPathExists = true;
            dlg.Multiselect = true;
            if (dlg.ShowDialog().GetValueOrDefault())
            {
                BackgroundWorker worker = new BackgroundWorker();
                // Open the document in a different thread
                worker.DoWork += (o, ea) =>
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)delegate
                    {
                        foreach (var fileName in dlg.FileNames)
                        {
                            var fileViewModel = Open(fileName);
                            ActiveDocument = fileViewModel;
                        }
                    });
                };
                worker.RunWorkerCompleted += (o, ea) =>
                {
                    // Work has completed. You can now interact with the UI
                    StopWaitAnimation();
                    _IsLoading = false;
                    if (ea.Error != null)
                    {
                        string caption = "Open file...";
                        string message = "Error opening: " + dlg.FileName + "\n" + ea.Error.ToString();
                        Xceed.Wpf.Toolkit.MessageBox.Show(Owner, message, caption, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                };

                StartWaitAnimation("Loading...");
                _IsLoading = true;

                worker.RunWorkerAsync();
            }
        }

        public FileViewModel Open(string filePath)
        {
            var fileViewModel = Files.FirstOrDefault(fm => fm.FilePath == filePath);
            if (fileViewModel != null)
            {
                return fileViewModel;
            }

            if (!File.Exists(filePath))
            {
                return null;
            }

            fileViewModel = new FileViewModel(filePath);
            if (Files != null)
            {
                if (Files.Count == 0)
                {
                    if (!IsCollapsiblePanelExpanded)
                        IsCollapsiblePanelExpanded = true;
                }
                Files.Add(fileViewModel);
            }

            // Remember initial directory
            SettingsManager.ApplicationSettings.InitialDirectory = System.IO.Path.GetDirectoryName(filePath);

            return fileViewModel;
        }

        #endregion 

        #region SaveCommand
        private RelayCommand _SaveCommand = null;
        public ICommand SaveCommand
        {
            get
            {
                if (_SaveCommand == null)
                {
                    _SaveCommand = new RelayCommand((p) => OnSave(p), (p) => CanSave(p));
                }

                return _SaveCommand;
            }
        }

        private bool CanSave(object parameter)
        {
            return (_ActiveDocument != null);
        }

        private void OnSave(object parameter)
        {
            //SaveAsync(ActiveDocument);
            if (ActiveDocument.IsDirty || string.IsNullOrEmpty(ActiveDocument.FilePath))
            {
                SaveSync(ActiveDocument);
            }
        }

        #endregion 

        #region SaveAsCommand
        private RelayCommand _SaveAsCommand = null;
        public ICommand SaveAsCommand
        {
            get
            {
                if (_SaveAsCommand == null)
                {
                    _SaveAsCommand = new RelayCommand((p) => OnSaveAs(p), (p) => CanSaveAs(p));
                }

                return _SaveAsCommand;
            }
        }

        private bool CanSaveAs(object parameter)
        {
            return (ActiveDocument != null);
        }

        private void OnSaveAs(object parameter)
        {
            //SaveAsync(ActiveDocument, true);
            //SaveSync(ActiveDocument, true, showOsKb : false);
            SaveSync(ActiveDocument, saveAndOpen: true, saveAsFlag: true, showOsKb: false);
        }

        #endregion 

        #region SaveAllCommand
        private RelayCommand _SaveAllCommand = null;
        public ICommand SaveAllCommand
        {
            get
            {
                if (_SaveAllCommand == null)
                {
                    _SaveAllCommand = new RelayCommand((p) => OnSaveAll(p), (p) => CanSaveAll(p));
                }

                return _SaveAllCommand;
            }
        }

        private bool CanSaveAll(object parameter)
        {
            return (_ActiveDocument != null);
        }

        private void OnSaveAll(object parameter)
        {
            bool bIsUnsavedFile = false;
            string destinationPath = string.Empty;
            string fileType = ".tif";
            bool bIsSaveAsCompressed = false;

            if (Files.Count != 0)
            {
                // Is there an unsaved files?
                for (int index = 0; index < Files.Count; index++)
                {
                    if (string.IsNullOrEmpty(Files[index].FilePath))
                    {
                        bIsUnsavedFile = true;
                        break;
                    }
                }

                // Get the destination folder if there's an unsaved file
                if (bIsUnsavedFile)
                {
                    destinationPath = SettingsManager.ApplicationSettings.InitialDirectory;

                    if (String.IsNullOrEmpty(destinationPath))
                    {
                        string commonPictureFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                        //string appCommonPictureFolder = commonPictureFolder + "\\" + Owner.ProductName;
                        string appCommonPictureFolder = commonPictureFolder + "\\" + "";
                        destinationPath = appCommonPictureFolder;
                    }

                    SelectFolder selectDestFolder = new SelectFolder(destinationPath);
                    selectDestFolder.Owner = Application.Current.MainWindow;
                    bool? dialogResult = selectDestFolder.ShowDialog();
                    if (dialogResult == true)
                    {
                        destinationPath = selectDestFolder.DestinationFolder;
                        fileType = selectDestFolder.SelectedFileType;
                        bIsSaveAsCompressed = selectDestFolder.IsSaveAsCompressed;

                        if (!System.IO.Directory.Exists(destinationPath))
                        {
                            // Create destination folder (if it doesn't exist)
                            try
                            {
                                System.IO.Directory.CreateDirectory(destinationPath);
                            }
                            catch (Exception ex)
                            {
                                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, "ERROR: Creating the specified directory...",
                                    MessageBoxButton.OK, MessageBoxImage.Stop);
                                return;
                            }
                        }

                        // Remember initial directory
                        SettingsManager.ApplicationSettings.InitialDirectory = destinationPath;
                    }
                    else
                    {
                        return; // Cancel saving operation
                    }
                } //bIsUnsavedFile

                // Show wait window
                StartWaitAnimation("Saving...");

                try
                {
                    IsBusySavingFile = true;
                    SaveAllFiles(destinationPath, fileType, bIsSaveAsCompressed);
                }
                catch
                {
                    StopWaitAnimation();
                    // Rethrow to preserve stack details
                    // Satisfies the rule. 
                    throw;
                }
                finally
                {
                    IsBusySavingFile = false;
                    StopWaitAnimation();
                }
            }
        }

        internal void SaveAllFiles(string destinationPath, string fileType, bool bIsSaveAsCompressed)
        {
            for (int index = 0; index < Files.Count; index++)
            {
                if (Files[index].IsDirty)
                {
                    string filePath = string.Empty;
                    string savedFilePath = string.Empty;
                    bool bIsAutoSavePublish = SettingsManager.ApplicationSettings.IsAutoSavePubFileEnabled;
                    bool bIsSaveInPlace = false;

                    if (Files[index].FilePath != null)
                        savedFilePath = string.Copy(Files[index].FilePath);

                    // Unsaved file?
                    if (string.IsNullOrEmpty(Files[index].FilePath))
                    {
                        string fileName = Files[index].Title;
                        bool hasFileName = false;

                        while (!hasFileName)
                        {
                            //if (string.IsNullOrEmpty(fileName) ||
                            //    (Regex.IsMatch(fileName, @"-F\d") || Regex.IsMatch(fileName, @"-S\d")))
                            if (string.IsNullOrEmpty(fileName))
                            {
                                fileName = GenerateFileName(string.Empty, fileType);
                            }
                            else
                            {
                                // Already has file extension?
                                if (CheckSupportedFileType(fileName))
                                {
                                    string ext = Path.GetExtension(fileName);
                                    if (ext.Equals(fileType))
                                    {
                                        fileName = Path.GetFileNameWithoutExtension(fileName);
                                    }
                                }

                                if (!string.IsNullOrEmpty(fileType))
                                {
                                    if (fileType.ToLower().Contains(".jpg"))
                                    {
                                        fileName = fileName + ".jpg";
                                        bIsAutoSavePublish = false;
                                    }
                                    else if (fileType.ToLower().Contains(".bmp"))
                                    {
                                        fileName = fileName + ".bmp";
                                        bIsAutoSavePublish = false;
                                    }
                                    else
                                    {
                                        fileName = fileName + ".tif";
                                    }
                                }
                                else
                                {
                                    fileName = fileName + fileType;
                                }
                            }

                            filePath = Path.Combine(destinationPath, fileName);

                            // Make sure we don't have the duplicate file name
                            if (System.IO.File.Exists(filePath))
                            {
                                // Generate a different file name.
                                if (string.IsNullOrEmpty(fileName) ||
                                    (fileName.Contains("Frame") || fileName.Contains("Set")))
                                {
                                    System.Threading.Thread.Sleep(1000);
                                    continue;
                                }
                                else
                                {
                                    // Get a unique file name in the destination folder
                                    fileName = GetUniqueFilenameInFolder(destinationPath, fileName);
                                    filePath = Path.Combine(destinationPath, fileName);
                                    hasFileName = true;
                                }
                            }
                            else
                            {
                                hasFileName = true;
                            }
                        }
                        Files[index].FilePath = filePath;
                    }
                    else
                    {
                        bIsSaveInPlace = true;
                    }

                    // Save the image to disk
                    if (SaveFile(Files[index], bIsSaveAsCompressed, bIsSaveInPlaceAllowed: bIsSaveInPlace))
                    {
                        if (!string.IsNullOrEmpty(@Files[index].FilePath))
                        {
                            DateTime modifiedDate = File.GetLastWriteTime(@Files[index].FilePath);
                            Files[index].ModifiedDate = System.String.Format("{0:G} UTC{0:zz}", modifiedDate);
                        }

                        if ((Files[index].Image.Format.BitsPerPixel % 16) == 0 && bIsAutoSavePublish)
                        {
                            // Save the display image as .PUB file
                            SaveFileAsPub(Files[index]);
                        }

                        if ((Files[index].Image.Format.BitsPerPixel % 16) == 0 &&
                            (fileType.Equals(".jpg") || fileType.Equals(".bmp")))
                        {
                            // Don't update the image tab title

                            // Restore file path
                            Files[index].FilePath = savedFilePath;
                        }
                        else
                        {
                            // Update image title and reset the dirty flag
                            Files[index].IsDirty = false;
                            Files[index].Title = Files[index].FileName;
                        }
                    }
                }
            }
        }

        private string GetLastOpenSaveFile(string extention)
        {
            RegistryKey regKey = Registry.CurrentUser;
            string lastUsedFolder = string.Empty;
            regKey = regKey.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\ComDlg32\\OpenSavePidMRU");

            if (string.IsNullOrEmpty(extention))
                extention = "tif";

            RegistryKey myKey = regKey.OpenSubKey(extention);

            if (myKey == null && regKey.GetSubKeyNames().Length > 0)
                myKey = regKey.OpenSubKey(regKey.GetSubKeyNames()[regKey.GetSubKeyNames().Length - 2]);

            if (myKey != null)
            {
                string[] names = myKey.GetValueNames();
                if (names != null && names.Length > 0)
                {
                    lastUsedFolder = (string)myKey.GetValue(names[names.Length - 2]);
                }
            }

            return lastUsedFolder;
        }

        #endregion 


        #region CloseCommand
        private RelayCommand _CloseCommand = null;
        public ICommand CloseCommand
        {
            get
            {
                if (_CloseCommand == null)
                {
                    _CloseCommand = new RelayCommand((p) => ExecuteCloseCommand(p), (p) => CanExecuteCloseCommand(p));
                }

                return _CloseCommand;
            }
        }

        private void ExecuteCloseCommand(object parameter)
        {
            if (_ActiveDocument != null)
            {
                Close(_ActiveDocument);
            }
        }

        private bool CanExecuteCloseCommand(object parameter)
        {
            return (_ActiveDocument != null);
        }

        #endregion 

        #region CloseAllCommand
        private RelayCommand _CloseAllCommand = null;
        public ICommand CloseAllCommand
        {
            get
            {
                if (_CloseAllCommand == null)
                {
                    _CloseAllCommand = new RelayCommand((p) => ExecuteCloseAllCommand(p), (p) => CanExecuteCloseAllCommand(p));
                }

                return _CloseAllCommand;
            }
        }

        private void ExecuteCloseAllCommand(object parameter)
        {
            if (_ActiveDocument != null)
            {
                CloseAll();
            }
        }

        private bool CanExecuteCloseAllCommand(object parameter)
        {
            return (_ActiveDocument != null);
        }

        public bool CloseAll()
        {
            bool bResult = true;
            TaskDialogOptions config = new TaskDialogOptions();
            config.Owner = Owner;
            config.Title = "Save changes?";
            config.MainInstruction = "Save changes before closing?";
            //config.ExpandedInfo = "Any expanded content text for the " +
            //                      "task dialog is shown here and the text " +
            //                      "will automatically wrap as needed.";
            //config.VerificationText = "Don't show me this message again";
            //config.FooterText = "Optional footer text with an icon can be included.";
            config.MainIcon = VistaTaskDialogIcon.Shield;
            //config.FooterIcon = VistaTaskDialogIcon.Warning;

            while (Files.Count > 0)
            {
                config.Content = string.Format("Do you want save changes to '{0}'", Files[0].Title);

                if (Files.Count > 1)
                {
                    config.CustomButtons = new string[] { "&Save", "Do&n't save", "No to &All", "&Cancel" };
                }
                else
                {
                    config.CustomButtons = new string[] { "&Save", "Do&n't save", "&Cancel" };
                }

                TaskDialogResult taskDlgResult = null;

                if (Files[0].IsDirty)
                {
                    taskDlgResult = TaskDialog.Show(config);
                }
                else
                {
                    Remove(Files[0]);
                    continue;
                }

                if (taskDlgResult != null)
                {
                    if (taskDlgResult.CustomButtonResult == 0)
                    {
                        // Close the file: save before closing.
                        if (!SaveSync(Files[0], closeAfterSaved: true))
                        {
                            // Cancel is selected
                            bResult = false;
                            break;
                        }
                    }
                    else if (taskDlgResult.CustomButtonResult == 1)
                    {
                        // Close the file: don't save before closing.
                        Remove(Files[0]);
                    }
                    else if (taskDlgResult.CustomButtonResult == 2 && config.CustomButtons.Length == 4)
                    {
                        // Close all file: don't save before closing.
                        while (Files.Count > 0)
                        {
                            Remove(Files[0]);
                        }
                        break;
                    }
                    else if ((taskDlgResult.CustomButtonResult == 2 && config.CustomButtons.Length == 3) ||
                             taskDlgResult.CustomButtonResult == 3)
                    {
                        // Cancel is selected
                        bResult = false;
                        break;
                    }
                }
            } //while

            return bResult;
        }

        /// <summary>
        /// Remove file and release memory.
        /// </summary>
        /// <param name="fileToRemove"></param>
        internal void Remove(FileViewModel fileToRemove)
        {
            int nextItem = Files.IndexOf(fileToRemove) - 1;
            bool bIsRemoveActiveDoc = (fileToRemove == ActiveDocument) ? true : false;

            Files.Remove(fileToRemove);

            fileToRemove.Dispose();
            fileToRemove = null;

            // Forces a garbage collection
            //GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            //GC.WaitForPendingFinalizers();
            //GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);

            // Retrieves the number of bytes currently thought to be allocated (true: force full collection)
            //GC.GetTotalMemory(true);

            if (Files.Count == 0)
            {
                ActiveDocument = null;

                if (IsCollapsiblePanelExpanded)
                    IsCollapsiblePanelExpanded = false;
            }
            else
            {
                if (bIsRemoveActiveDoc)
                {
                    if (nextItem < 0)
                    {
                        nextItem = 0;
                    }
                    ActiveDocument = Files[nextItem];
                }
            }

            RaisePropertyChanged("IsActiveDocument");
            RaisePropertyChanged("IsShowCloseAll");

            // Force a garbage collection to free up memory as quickly as possible.
            //GC.Collect();
        }

        #endregion

        #region CloseFileCommand
        RelayCommand _CloseFileCommand = null;
        public ICommand CloseFileCommand
        {
            get
            {
                if (_CloseFileCommand == null)
                {
                    _CloseFileCommand = new RelayCommand(ExecuteCloseFileCommand, CanExecuteCloseFileCommand);
                }
                return _CloseFileCommand;
            }
        }
        private bool CanExecuteCloseFileCommand(object parameter)
        {
            var file = parameter as FileViewModel;
            return (file != null);
        }
        private void ExecuteCloseFileCommand(object parameter)
        {
            var file = parameter as FileViewModel;
            if (file != null)
            {
                Close(file);
            }
        }

        #endregion

        #region ExportToAzureSpotCommand
        private RelayCommand _ExportToAzureSpotCommand = null;
        public ICommand ExportToAzureSpotCommand
        {
            get
            {
                if (_ExportToAzureSpotCommand == null)
                {
                    _ExportToAzureSpotCommand = new RelayCommand((p) => ExecuteExportToAzureSpotCommand(p), (p) => CanExecuteExportToAzureSpotCommand(p));
                }

                return _ExportToAzureSpotCommand;
            }
        }
        private void ExecuteExportToAzureSpotCommand(object parameter)
        {
            if (ActiveDocument == null || !ActiveDocument.IsRgbImage) { return; }

            var fileSaveDlg = new SaveFileDialog();
            fileSaveDlg.Filter = "DS FILE (*.ds)|*.ds";
            fileSaveDlg.Title = "Export to AzureSpot DS File";

            string fileName = string.Empty;
            string filePath = string.Empty;
            string initialFileName = string.Empty;
            string initialDirectory = string.Empty;
            string dstFolder = string.Empty;
            string fileExtension = string.Empty;

            // Get/set initial file name in SaveFileDialog
            if (CheckSupportedFileType(ActiveDocument.Title))
                initialFileName = Path.GetFileNameWithoutExtension(ActiveDocument.Title);
            else
                initialFileName = ActiveDocument.Title;
            initialFileName = initialFileName + ".ds";

            // Get/set initial destination folder in SaveFileDialog
            if (ActiveDocument.FilePath == null || string.IsNullOrEmpty(ActiveDocument.FilePath))
            {
                initialDirectory = SettingsManager.ApplicationSettings.InitialDirectory;
                if (string.IsNullOrEmpty(initialDirectory))
                {
                    initialDirectory = "D:\\";
                }
            }
            else
            {
                initialDirectory = Path.GetDirectoryName(ActiveDocument.FilePath);
            }

            fileExtension = ".tif";     // assume .TIFF
            if (CheckSupportedFileType(ActiveDocument.Title))
                fileExtension = Path.GetExtension(ActiveDocument.Title);
            if (string.IsNullOrEmpty(fileExtension))
                fileExtension = ".tif"; // assume .TIFF

            int iFileType = ImageProcessing.TIFF_FILE;

            if (fileExtension.ToLower() == ".tif" ||
                fileExtension.ToLower() == ".tiff")
            {
                iFileType = ImageProcessing.TIFF_FILE;
            }
            else if (fileExtension.ToLower() == ".jpg" ||
                     fileExtension.ToLower() == ".jpeg")
            {
                iFileType = ImageProcessing.JPG_FILE;
            }
            else if (fileExtension.ToLower() == ".bmp")
            {
                iFileType = ImageProcessing.BMP_FILE;
            }
            else
            {
                throw new NotSupportedException("File type not supported.");
            }

            fileSaveDlg.FileName = initialFileName;
            fileSaveDlg.InitialDirectory = initialDirectory;

            if (fileSaveDlg.ShowDialog().GetValueOrDefault())
            {
                filePath = fileSaveDlg.FileName;
                //if (CheckSupportedFileType(filePath))
                //    fileName = Path.GetFileNameWithoutExtension(filePath);
                //else
                //    fileName = Path.GetFileName(filePath);
                var fileExt = Path.GetExtension(filePath);
                if (fileExt.Equals(".ds"))
                    fileName = Path.GetFileNameWithoutExtension(filePath);
                else
                    fileName = Path.GetFileName(filePath);
                dstFolder = Path.GetDirectoryName(filePath);

                if (ActiveDocument.Image != null)
                {
                    WriteableBitmap[] imageChannels = null;
                    imageChannels = ImageProcessing.GetChannel(ActiveDocument.Image);

                    if (imageChannels == null)
                    {
                        return;
                    }

                    List<string> savedImageChannels = new List<string>();

                    #region === Red channel ===
                    // red channel
                    if (imageChannels[0] != null)
                    {
                        string wavelength = string.Empty;
                        ImageInfo cpyInfo = new ImageInfo();

                        int nMaxPixel = ImageProcessing.Max(imageChannels[0], new System.Windows.Rect(0, 0, imageChannels[0].Width, imageChannels[0].Height));

                        if (ActiveDocument.ImageInfo.IsRedChannelAvail || nMaxPixel > 0)
                        {
                            cpyInfo = ActiveDocument.ImageInfo.Clone() as ImageInfo;
                            cpyInfo.MixChannel = (ImageChannel)cpyInfo.RedChannel.Clone();
                            if (cpyInfo.IsScannedImage)
                            {
                                var laserType = GetImageInfoLaserType(cpyInfo, ImageChannelType.Red);
                                ExtractAndSetLaserInfo(ref cpyInfo, laserType);
                                wavelength = GetLaserWaveLength(cpyInfo, laserType);
                            }
                        }

                        if (string.IsNullOrEmpty(wavelength))
                        {
                            wavelength = "Red";
                        }

                        cpyInfo.SelectedChannel = ImageChannelType.Mix;
                        cpyInfo.NumOfChannels = 1;

                        var imageCHFileName = fileName + "_" + wavelength + fileExtension;
                        var dstfilePath = Path.Combine(dstFolder, imageCHFileName);
                        savedImageChannels.Add(imageCHFileName);
                        // Invert the image pixels before saving (AzureSpot expected the exported image to be inverted).
                        var invertedImage = ImageProcessing.Invert(imageChannels[0]);
                        // Save the image channel file
                        ImageProcessing.Save(dstfilePath, invertedImage, cpyInfo, iFileType, false, false);

                        // Remember initial directory
                        SettingsManager.ApplicationSettings.InitialDirectory = System.IO.Path.GetDirectoryName(dstfilePath);
                    }
                    #endregion

                    #region === Green channel ===
                    // green channel
                    if (imageChannels[1] != null)
                    {

                        string wavelength = string.Empty;
                        ImageInfo cpyInfo = new ImageInfo();

                        int nMaxPixel = ImageProcessing.Max(imageChannels[1], new System.Windows.Rect(0, 0, imageChannels[1].Width, imageChannels[1].Height));

                        if (ActiveDocument.ImageInfo.IsGreenChannelAvail || nMaxPixel > 0)
                        {
                            cpyInfo = ActiveDocument.ImageInfo.Clone() as ImageInfo;
                            cpyInfo.MixChannel = (ImageChannel)cpyInfo.GreenChannel.Clone();
                            if (cpyInfo.IsScannedImage)
                            {
                                var laserType = GetImageInfoLaserType(cpyInfo, ImageChannelType.Green);
                                ExtractAndSetLaserInfo(ref cpyInfo, laserType);
                                wavelength = GetLaserWaveLength(cpyInfo, laserType);
                            }
                        }

                        cpyInfo.SelectedChannel = ImageChannelType.Mix;
                        cpyInfo.NumOfChannels = 1;

                        if (string.IsNullOrEmpty(wavelength))
                        {
                            wavelength = "Green";
                        }

                        var imageCHFileName = fileName + "_" + wavelength + fileExtension;
                        var dstfilePath = Path.Combine(dstFolder, imageCHFileName);
                        savedImageChannels.Add(imageCHFileName);
                        // Invert the image pixels before saving (AzureSpot expected the exported image to be inverted).
                        var invertedImage = ImageProcessing.Invert(imageChannels[1]);
                        // Save the image channel file
                        ImageProcessing.Save(dstfilePath, invertedImage, cpyInfo, iFileType, false, false);

                        // Remember initial directory
                        SettingsManager.ApplicationSettings.InitialDirectory = System.IO.Path.GetDirectoryName(dstfilePath);
                    }
                    #endregion

                    #region === Blue Channel ===
                    // blue channel
                    if (imageChannels[2] != null)
                    {
                        string wavelength = string.Empty;
                        ImageInfo cpyInfo = new ImageInfo();

                        int nMaxPixel = ImageProcessing.Max(imageChannels[2], new System.Windows.Rect(0, 0, imageChannels[2].Width, imageChannels[2].Height));

                        if (ActiveDocument.ImageInfo.IsBlueChannelAvail || nMaxPixel > 0)
                        {
                            cpyInfo = ActiveDocument.ImageInfo.Clone() as ImageInfo;
                            cpyInfo.MixChannel = (ImageChannel)cpyInfo.BlueChannel.Clone();
                            if (cpyInfo.IsScannedImage)
                            {
                                var laserType = GetImageInfoLaserType(cpyInfo, ImageChannelType.Blue);
                                ExtractAndSetLaserInfo(ref cpyInfo, laserType);
                                wavelength = GetLaserWaveLength(cpyInfo, laserType);
                            }
                        }

                        cpyInfo.SelectedChannel = ImageChannelType.Mix;
                        cpyInfo.NumOfChannels = 1;

                        if (string.IsNullOrEmpty(wavelength))
                        {
                            wavelength = "Blue";
                        }

                        var imageCHFileName = fileName + "_" + wavelength + fileExtension;
                        var dstfilePath = Path.Combine(dstFolder, imageCHFileName);
                        savedImageChannels.Add(imageCHFileName);
                        // Invert the image pixels before saving (AzureSpot expected the exported image to be inverted).
                        var invertedImage = ImageProcessing.Invert(imageChannels[2]);
                        // Save the image channel file
                        ImageProcessing.Save(dstfilePath, invertedImage, cpyInfo, iFileType, false, false);

                        // Remember initial directory
                        SettingsManager.ApplicationSettings.InitialDirectory = System.IO.Path.GetDirectoryName(dstfilePath);
                    }
                    #endregion

                    #region === Gray Channel ===
                    if (imageChannels.Length == 4)
                    {
                        // gray channel
                        if (imageChannels[3] != null)
                        {
                            string wavelength = string.Empty;
                            ImageInfo cpyInfo = new ImageInfo();

                            int nMaxPixel = ImageProcessing.Max(imageChannels[3], new System.Windows.Rect(0, 0, imageChannels[3].Width, imageChannels[3].Height));

                            if (ActiveDocument.ImageInfo.IsGrayChannelAvail || nMaxPixel > 0)
                            {
                                cpyInfo = ActiveDocument.ImageInfo.Clone() as ImageInfo;
                                cpyInfo.MixChannel = (ImageChannel)cpyInfo.GrayChannel.Clone();
                                if (cpyInfo.IsScannedImage)
                                {
                                    var laserType = GetImageInfoLaserType(cpyInfo, ImageChannelType.Gray);
                                    ExtractAndSetLaserInfo(ref cpyInfo, laserType);
                                    wavelength = GetLaserWaveLength(cpyInfo, laserType);
                                }
                            }

                            cpyInfo.SelectedChannel = ImageChannelType.Mix;
                            cpyInfo.NumOfChannels = 1;

                            if (string.IsNullOrEmpty(wavelength))
                            {
                                wavelength = "Gray";
                            }

                            var imageCHFileName = fileName + "_" + wavelength + fileExtension;
                            var dstfilePath = Path.Combine(dstFolder, imageCHFileName);
                            savedImageChannels.Add(imageCHFileName);
                            // Invert the image pixels before saving (AzureSpot expected the exported image to be inverted).
                            var invertedImage = ImageProcessing.Invert(imageChannels[3]);
                            // Save the image channel file
                            ImageProcessing.Save(dstfilePath, invertedImage, cpyInfo, iFileType, false, false);

                            // Remember initial directory
                            SettingsManager.ApplicationSettings.InitialDirectory = System.IO.Path.GetDirectoryName(dstfilePath);
                        }
                    }
                    #endregion

                    #region Create .DS file
                    int nNumOfChannels = savedImageChannels.Count;
                    if (nNumOfChannels > 0)
                    {
                        string fileNamePlusExt = fileName + ".ds";
                        string iniFilePath = Path.Combine(dstFolder, fileNamePlusExt);
                        IniFile iniFile = new IniFile(iniFilePath);
                        iniFile.IniWriteValue("MD_FILE", "MDDSFILE", "1");
                        iniFile.IniWriteValue("DATASET", "DSFILETYPE", "575");
                        iniFile.IniWriteValue("DATASET", "DS_UNSEP_CHANNEL_COUNT", nNumOfChannels.ToString());

                        for (int i = 0; i < nNumOfChannels; i++)
                        {
                            iniFile.IniWriteValue("DATASET", string.Format("UNSEP_CHANNEL{0}", i + 1), savedImageChannels[i]);
                        }

                        //if (_Owner.ImagingSysSettings.IsQcVersion)
                        //{
                        //    ActivityLog newActivityLog = new ActivityLog(_Owner.LoginUser.UserName);
                        //    newActivityLog.LogCreateSignalChannel("Export to AzureSpot's .DS file");
                        //    _Owner.ManageUsersVM.LogActivity(newActivityLog);
                        //}
                    }
                    #endregion

                    string caption = "Export to AzureSpot";
                    string message = "Image file exported.";
                    Xceed.Wpf.Toolkit.MessageBox.Show((MainWindow)Owner, message, caption, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);

                    //if (_Owner.ImagingSysSettings.IsQcVersion)
                    //{
                    //    ActivityLog newLog = new ActivityLog(_Owner.LoginUser.UserName);
                    //    newLog.LogFileAction("Exported to AzureSpot", ActiveDocument.FileName);
                    //    _Owner.ManageUsersVM.LogActivity(newLog);
                    //}
                }
            }
        }
        private bool CanExecuteExportToAzureSpotCommand(object parameter)
        {
            return (ActiveDocument != null && ActiveDocument.IsRgbImage);
        }

        #endregion

        #region PrintCommand
        private RelayCommand _PrintCommand = null;
        /// <summary>
        /// Get the print command.
        /// </summary>
        public ICommand PrintCommand
        {
            get
            {
                if (_PrintCommand == null)
                {
                    _PrintCommand = new RelayCommand(this.ExecutePrintCommand, null);
                }

                return _PrintCommand;
            }
        }
        #region protected void ExecutePrintCommand(object parameter)
        /// <summary>
        /// Print command implementation.
        /// </summary>
        /// <param name="parameter"></param>
        /*protected void ExecutePrintCommand(object parameter)
        {
            if (ActiveDocument == null) { return; }

            string commandParam = (string)parameter;
            bool bIsPrintInfo = false;
            if (commandParam == "PrintInfo")
            {
                bIsPrintInfo = true;
            }

            PrintDialog printDlg = new PrintDialog();
            if (printDlg.ShowDialog() != true) { return; }

            System.Windows.Controls.Image image = new System.Windows.Controls.Image();
            image.Source = ActiveDocument.DisplayImage;
            string fileName = ActiveDocument.FileName;
            string dateTime = ActiveDocument.ImageInfo.DateTime;
            //double margin = 20.0;
            Thickness marginPage = new Thickness(25);

            // Calculate rectangle for image
            //double width = printDlg.PrintableAreaWidth / 2;
            //double height = width * image.Source.Height / image.Source.Width;
            //double left = (printDlg.PrintableAreaWidth - width) / 2;
            //double top = (printDlg.PrintableAreaHeight - height) / 2;

            double width = printDlg.PrintableAreaWidth - (marginPage.Left + marginPage.Right);
            double height = width * image.Source.Height / image.Source.Width;
            double fontSize = 12;
            double left = marginPage.Left;
            double top = marginPage.Right;

            //Rect rect = new Rect(left, top, width, height);

            // Create DrawingVisual and get its drawing context
            DrawingVisual vs = new DrawingVisual();
            DrawingContext dc = vs.RenderOpen();

            // Remove unsaved file indicator
            //fileName = fileName.Replace('*', ' ').TrimEnd();
            fileName = fileName.Trim(new Char[] { ' ', '*' });

            // Create formatted text--in a particular font at a particular size
            FormattedText formtxt = new FormattedText(
                fileName,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(new FontFamily("Trebuchet MS"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                fontSize,
                Brushes.Black);

            // Draw the text at a location (file name : left justify)
            dc.DrawText(formtxt, new Point(marginPage.Left, marginPage.Top));

            // Create formatted text--in a particular font at a particular size
            formtxt = new FormattedText(
                dateTime,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(new FontFamily("Trebuchet MS"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                fontSize,
                Brushes.Black);

            // Get size of text.
            Size sizeText = new Size(formtxt.Width, formtxt.Height);
            double destX = width - sizeText.Width + marginPage.Left;

            // Draw the text at a location (date-time : right justify)
            dc.DrawText(formtxt, new Point(destX, marginPage.Top));

            top = marginPage.Top + sizeText.Height + 2;
            Rect rect = new Rect(left, top, width, height);
            
            // Draw image
            dc.DrawImage(image.Source, rect);

            //double scale = width / image.Source.Width;

            // Keep old existing actual scale and set new actual scale.
            //double oldActualScale = ActiveDocument.DrawingCanvas.ActualScale;
            //ActiveDocument.DrawingCanvas.ActualScale = scale;

            // Remove clip in the canvas - we set our own clip.
            //ActiveDocument.DrawingCanvas.RemoveClip();

            // Prepare drawing context to draw graphics
            //dc.PushClip(new RectangleGeometry(rect));
            //dc.PushTransform(new TranslateTransform(left, top));
            //dc.PushTransform(new ScaleTransform(scale, scale));

            // Ask canvas to draw overlays
            //ActiveDocument.DrawingCanvas.Draw(dc);

            // Restore old actual scale.
            //ActiveDocument.DrawingCanvas.ActualScale = oldActualScale;

            // Restore clip
            //ActiveDocument.DrawingCanvas.RefreshClip();

            //dc.Pop();
            //dc.Pop();
            //dc.Pop();

            #region === Print Image Info ===

            if (ActiveDocument.ImageInfo != null && bIsPrintInfo)
            {
                //ImageInfo imageInfo = ActiveDocument.ImageInfo;
                const int verticalSpacing = 2;
                double destY = 0.0;
                Point ptImageInfo;

                if (ActiveDocument.ImageInfo.IsScannedImage)
                {
                    #region === Scanned image info ===

                    if (!string.IsNullOrEmpty(ActiveDocument.ImageInfo.CaptureType))
                    {
                        string strCaptureType = "Capture type: " + ActiveDocument.ImageInfo.CaptureType;
                        // Create formatted text--in a particular font at a particular size
                        formtxt = new FormattedText(
                            strCaptureType,
                            CultureInfo.CurrentCulture,
                            FlowDirection.LeftToRight,
                            new Typeface(new FontFamily("Trebuchet MS"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                            fontSize,
                            Brushes.Black);

                        // Get size of text.
                        sizeText = new Size(formtxt.Width, formtxt.Height);
                        destY = rect.Bottom + 4;
                        ptImageInfo = new Point(marginPage.Left, destY);

                        // Draw the text at a location (filer position)
                        dc.DrawText(formtxt, ptImageInfo);
                    }

                    if (!string.IsNullOrEmpty(ActiveDocument.ScanRegion))
                    {
                        string strScanRegion = "Scan region: " + ActiveDocument.ScanRegion;
                        // Create formatted text--in a particular font at a particular size
                        formtxt = new FormattedText(
                            strScanRegion,
                            CultureInfo.CurrentCulture,
                            FlowDirection.LeftToRight,
                            new Typeface(new FontFamily("Trebuchet MS"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                            fontSize,
                            Brushes.Black);

                        // Get size of text.
                        sizeText = new Size(formtxt.Width, formtxt.Height);
                        destY = (destY > 0) ? destY + sizeText.Height + verticalSpacing : rect.Bottom + 4;
                        ptImageInfo = new Point(marginPage.Left, destY);

                        // Draw the text at a location (Light Source)
                        dc.DrawText(formtxt, ptImageInfo);
                    }

                    if (!string.IsNullOrEmpty(ActiveDocument.SampleType))
                    {
                        string strSampleType = "Sample type: " + ActiveDocument.SampleType;
                        // Create formatted text--in a particular font at a particular size
                        formtxt = new FormattedText(
                            strSampleType,
                            CultureInfo.CurrentCulture,
                            FlowDirection.LeftToRight,
                            new Typeface(new FontFamily("Trebuchet MS"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                            fontSize,
                            Brushes.Black);

                        // Get size of text.
                        sizeText = new Size(formtxt.Width, formtxt.Height);
                        destY = (destY > 0) ? destY + sizeText.Height + verticalSpacing : rect.Bottom + 4;
                        ptImageInfo = new Point(marginPage.Left, destY);

                        // Draw the text at a location (Light Source)
                        dc.DrawText(formtxt, ptImageInfo);
                    }

                    if (ActiveDocument.ImageInfo.ScanResolution > 0)
                    {
                        string strPixelSize = "Pixel size: " + ActiveDocument.ImageInfo.ScanResolution.ToString() + " μm";
                        // Create formatted text--in a particular font at a particular size
                        formtxt = new FormattedText(
                            strPixelSize,
                            CultureInfo.CurrentCulture,
                            FlowDirection.LeftToRight,
                            new Typeface(new FontFamily("Trebuchet MS"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                            fontSize,
                            Brushes.Black);

                        // Get size of text.
                        sizeText = new Size(formtxt.Width, formtxt.Height);
                        destY = (destY > 0) ? destY + sizeText.Height + verticalSpacing : rect.Bottom + 4;
                        ptImageInfo = new Point(marginPage.Left, destY);

                        // Draw the text at a location (Exposure Time)
                        dc.DrawText(formtxt, ptImageInfo);
                    }

                    if (!string.IsNullOrEmpty(ActiveDocument.ScanSpeed))
                    {
                        string strScanSpeed = "Scan speed: " + ActiveDocument.ScanSpeed;
                        // Create formatted text--in a particular font at a particular size
                        formtxt = new FormattedText(
                            strScanSpeed,
                            CultureInfo.CurrentCulture,
                            FlowDirection.LeftToRight,
                            new Typeface(new FontFamily("Trebuchet MS"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                            fontSize,
                            Brushes.Black);

                        // Get size of text.
                        sizeText = new Size(formtxt.Width, formtxt.Height);
                        destY = (destY > 0) ? destY + sizeText.Height + verticalSpacing : rect.Bottom + 4;
                        ptImageInfo = new Point(marginPage.Left, destY);

                        // Draw the text at a location (Exposure Time)
                        dc.DrawText(formtxt, ptImageInfo);
                    }

                    if (!string.IsNullOrEmpty(ActiveDocument.IntensityLevel))
                    {
                        string strIntensityLevel = "Intensity: " + ActiveDocument.IntensityLevel;
                        // Create formatted text--in a particular font at a particular size
                        formtxt = new FormattedText(
                            strIntensityLevel,
                            CultureInfo.CurrentCulture,
                            FlowDirection.LeftToRight,
                            new Typeface(new FontFamily("Trebuchet MS"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                            fontSize,
                            Brushes.Black);

                        // Get size of text.
                        sizeText = new Size(formtxt.Width, formtxt.Height);
                        destY = (destY > 0) ? destY + sizeText.Height + verticalSpacing : rect.Bottom + 4;
                        ptImageInfo = new Point(marginPage.Left, destY);

                        // Draw the text at a location (Exposure Time)
                        dc.DrawText(formtxt, ptImageInfo);
                    }

                    if (!string.IsNullOrEmpty(ActiveDocument.LaserChannels))
                    {
                        string strChannel = "Channel: " + ActiveDocument.LaserChannels;
                        // Create formatted text--in a particular font at a particular size
                        formtxt = new FormattedText(
                            strChannel,
                            CultureInfo.CurrentCulture,
                            FlowDirection.LeftToRight,
                            new Typeface(new FontFamily("Trebuchet MS"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                            fontSize,
                            Brushes.Black);

                        // Get size of text.
                        sizeText = new Size(formtxt.Width, formtxt.Height);
                        destY = (destY > 0) ? destY + sizeText.Height + verticalSpacing : rect.Bottom + 4;
                        ptImageInfo = new Point(marginPage.Left, destY);

                        // Draw the text at a location (Exposure Time)
                        dc.DrawText(formtxt, ptImageInfo);
                    }

                    #endregion
                }
                else
                {
                    #region === Chemi image info ===

                    if (!string.IsNullOrEmpty(ActiveDocument.ImageInfo.CaptureType))
                    {
                        string filterType = "Capture type: " + ActiveDocument.ImageInfo.CaptureType;
                        // Create formatted text--in a particular font at a particular size
                        formtxt = new FormattedText(
                            filterType,
                            CultureInfo.CurrentCulture,
                            FlowDirection.LeftToRight,
                            new Typeface(new FontFamily("Trebuchet MS"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                            fontSize,
                            Brushes.Black);

                        // Get size of text.
                        sizeText = new Size(formtxt.Width, formtxt.Height);
                        destY = rect.Bottom + 4;
                        ptImageInfo = new Point(marginPage.Left, destY);

                        // Draw the text at a location (filer position)
                        dc.DrawText(formtxt, ptImageInfo);
                    }

                    if (!string.IsNullOrEmpty(ActiveDocument.FormattedExposureTime))
                    {
                        string exposureTime = "Exposure Time: " + ActiveDocument.FormattedExposureTime;
                        // Create formatted text--in a particular font at a particular size
                        formtxt = new FormattedText(
                            exposureTime,
                            CultureInfo.CurrentCulture,
                            FlowDirection.LeftToRight,
                            new Typeface(new FontFamily("Trebuchet MS"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                            fontSize,
                            Brushes.Black);

                        // Get size of text.
                        sizeText = new Size(formtxt.Width, formtxt.Height);
                        destY = (destY > 0) ? destY + sizeText.Height + verticalSpacing : rect.Bottom + 4;
                        ptImageInfo = new Point(marginPage.Left, destY);

                        // Draw the text at a location (Exposure Time)
                        dc.DrawText(formtxt, ptImageInfo);
                    }

                    #endregion
                }

                if (!string.IsNullOrEmpty(ActiveDocument.ImageInfo.Comment))
                {
                    string comment = "Comment: " + ActiveDocument.ImageInfo.Comment;
                    // Create formatted text--in a particular font at a particular size
                    formtxt = new FormattedText(
                        comment,
                        CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        new Typeface(new FontFamily("Trebuchet MS"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                        fontSize,
                        Brushes.Black);

                    // Get size of text.
                    sizeText = new Size(formtxt.Width, formtxt.Height);
                    destY = (destY > 0) ? destY + sizeText.Height + verticalSpacing : rect.Bottom + 4;
                    ptImageInfo = new Point(marginPage.Left, destY);

                    // Draw the text at a location (Comment)
                    dc.DrawText(formtxt, ptImageInfo);
                }
            }

            #endregion

            dc.Close();

            // Print DrawVisual
            printDlg.PrintVisual(vs, fileName);
        }*/
        protected void ExecutePrintCommand(object parameter)
        {
            if (ActiveDocument == null) { return; }

            string commandParam = (string)parameter;
            bool bIsPrintInfo = false;
            if (commandParam == "PrintInfo")
            {
                bIsPrintInfo = true;
            }

            List<string> infoToPrint = new List<string>();

            //var visualSize = new Size(ActiveDocument.Image.PixelWidth, ActiveDocument.Image.PixelHeight);
            //System.Windows.Controls.Image image = new System.Windows.Controls.Image();
            //image.Source = ActiveDocument.DisplayImage;
            //var printControl = SUT.PrintEngine.Utils.PrintControlFactory.Create(visualSize, image);
            //printControl.ShowPrintPreview();

            var printDlg = new PrintDialog();
            if (printDlg.ShowDialog().GetValueOrDefault() != true)
            {
                return;
            }

            System.Windows.Controls.Image image = new System.Windows.Controls.Image();
            image.Source = ActiveDocument.DisplayImage;
            Thickness pageMargin = new Thickness(25);

            // Drawing annotations/scalebar
            if (ActiveDocument.IsShowScalebar &&
                ActiveDocument.DrawingCanvas != null && ActiveDocument.DrawingCanvas.GraphicsList.Count > 0)
            {
                WriteableBitmap wbm = ActiveDocument.DisplayImage;
                DrawAnnotations(ActiveDocument, ref wbm);
                image.Source = wbm;
            }

            //get selected printer capabilities
            PrintCapabilities capabilities = printDlg.PrintQueue.GetPrintCapabilities(printDlg.PrintTicket);

            System.Windows.Documents.FixedDocument doc = new System.Windows.Documents.FixedDocument();
            doc.DocumentPaginator.PageSize = new System.Windows.Size(capabilities.PageImageableArea.ExtentWidth, capabilities.PageImageableArea.ExtentHeight);
            //doc.DocumentPaginator.PageSize = new Size(printDlg.PrintableAreaWidth, printDlg.PrintableAreaHeight);
            System.Windows.Documents.FixedPage fp = new System.Windows.Documents.FixedPage();
            fp.Width = doc.DocumentPaginator.PageSize.Width;
            fp.Height = doc.DocumentPaginator.PageSize.Height;

            //Print header (file name and date captured)
            TextBlock headerText = new TextBlock();
            headerText.Text = ActiveDocument.Title;
            headerText.Margin = pageMargin;
            headerText.FontFamily = new FontFamily("Trebuchet MS");
            headerText.FontSize = 12;

            fp.Children.Add(headerText);

            var textSize = MeasureString(headerText.Text, headerText.FontSize);

            if (!string.IsNullOrEmpty(ActiveDocument.ImageInfo.DateTime))
            {
                headerText = new TextBlock();
                headerText.Text = ActiveDocument.ImageInfo.DateTime;
                headerText.FontFamily = new FontFamily("Trebuchet MS");
                headerText.FontSize = 12;
                textSize = MeasureString(headerText.Text, headerText.FontSize);
                headerText.Margin = new Thickness(capabilities.PageImageableArea.ExtentWidth - textSize.Width - pageMargin.Left, pageMargin.Top, pageMargin.Right, pageMargin.Bottom);

                fp.Children.Add(headerText);
            }

            #region === Image Info ===

            if (ActiveDocument.ImageInfo != null && bIsPrintInfo)
            {
                // Common fields: user name and protocol
                //if (!string.IsNullOrEmpty(ActiveDocument.ImageInfo.UserName))
                //{
                //    string infoText = "User name: " + ActiveDocument.ImageInfo.UserName;
                //    infoToPrint.Add(infoText);
                //}
                //if (!string.IsNullOrEmpty(ActiveDocument.ImageInfo.Protocol))
                //{
                //    string infoText = "Protocol: " + ActiveDocument.ImageInfo.Protocol;
                //    infoToPrint.Add(infoText);
                //}

                if (ActiveDocument.ImageInfo.IsScannedImage)
                {
                    #region === Print Scanned Image Info ===

                    if (!string.IsNullOrEmpty(ActiveDocument.ImageInfo.CaptureType))
                    {
                        string infoText = "Capture type: " + ActiveDocument.ImageInfo.CaptureType;
                        infoToPrint.Add(infoText);
                    }
                    if (!string.IsNullOrEmpty(ActiveDocument.ImageInfo.ScanRegion))
                    {
                        string infoText = "Scan region: " + ActiveDocument.ScanRegion;
                        infoToPrint.Add(infoText);
                    }
                    if (!string.IsNullOrEmpty(ActiveDocument.SampleType))
                    {
                        string infoText = "Focus type: " + ActiveDocument.SampleType;
                        infoToPrint.Add(infoText);
                    }
                    if (ActiveDocument.ImageInfo.ScanResolution > 0)
                    {
                        string infoText = "Pixel size: " + ActiveDocument.ImageInfo.ScanResolution.ToString() + " μm";
                        infoToPrint.Add(infoText);
                    }
                    if (!string.IsNullOrEmpty(ActiveDocument.ScanSpeed))
                    {
                        string infoText = "Scan speed: " + ActiveDocument.ScanSpeed;
                        infoToPrint.Add(infoText);
                    }
                    if (!string.IsNullOrEmpty(ActiveDocument.IntensityLevel))
                    {
                        string infoText = "Intensity: " + ActiveDocument.IntensityLevel;
                        infoToPrint.Add(infoText);
                    }
                    if (!string.IsNullOrEmpty(ActiveDocument.LaserWavelength))
                    {
                        string infoText = "Channel: " + ActiveDocument.LaserWavelength;
                        infoToPrint.Add(infoText);
                    }
                    if (!string.IsNullOrEmpty(ActiveDocument.ImageInfo.ScanQuality))
                    {
                        string infoText = "Quality: " + ActiveDocument.ImageInfo.ScanQuality;
                        infoToPrint.Add(infoText);
                    }

                    #endregion
                }
                else
                {
                    #region === Print CCD Image Info ===

                    if (!string.IsNullOrEmpty(ActiveDocument.ImageInfo.CaptureType))
                    {
                        string infoText = String.Format("{0}: {1}", "Capture type", ActiveDocument.ImageInfo.CaptureType);
                        infoToPrint.Add(infoText);
                    }
                    if (!string.IsNullOrEmpty(ActiveDocument.FormattedExposureTime))
                    {
                        string infoText = "Exposure Time: " + ActiveDocument.FormattedExposureTime;
                        infoToPrint.Add(infoText);
                    }
                    if (!string.IsNullOrEmpty(ActiveDocument.AutoExposureType))
                    {
                        string infoText = "Autoexposure type: " + ActiveDocument.AutoExposureType;
                        infoToPrint.Add(infoText);
                    }
                    if (!string.IsNullOrEmpty(ActiveDocument.ImageInfo.ReadoutSpeed))
                    {
                        string infoText = "Readout speed: " + ActiveDocument.ImageInfo.ReadoutSpeed;
                        infoToPrint.Add(infoText);
                    }
                    if (ActiveDocument.ImageInfo.BinFactor > 0)
                    {
                        string infoText = String.Format("{0}: {1}x{1}", "Binning", ActiveDocument.ImageInfo.BinFactor);
                        infoToPrint.Add(infoText);
                    }
                    if (ActiveDocument.ImageInfo.GainValue > 0)
                    {
                        string infoText = "Gain: " + ActiveDocument.ImageInfo.GainValue;
                        infoToPrint.Add(infoText);
                    }
                    if (!string.IsNullOrEmpty(ActiveDocument.ImageInfo.Calibration))
                    {
                        string infoText = "Calibration: " + ActiveDocument.ImageInfo.Calibration;
                        infoToPrint.Add(infoText);
                    }
                    //if (!string.IsNullOrEmpty(ActiveDocument.ImageInfo.CameraFirmware))
                    //{
                    //    string infoText = "CCD Camera Firmware: " + ActiveDocument.ImageInfo.CameraFirmware;
                    //    infoToPrint.Add(infoText);
                    //}
                    #endregion
                }

                // More common fields
                //
                //if (!string.IsNullOrEmpty(ActiveDocument.ImageInfo.FpgaFirmware))
                //{
                //    string infoText = "System Firmware: " + ActiveDocument.ImageInfo.FpgaFirmware;
                //    infoToPrint.Add(infoText);
                //}
                if (!string.IsNullOrEmpty(ActiveDocument.ImageInfo.SoftwareVersion))
                {
                    string infoText = "Software version: " + ActiveDocument.ImageInfo.SoftwareVersion;
                    infoToPrint.Add(infoText);
                }
                if (!string.IsNullOrEmpty(ActiveDocument.ImageInfo.SystemSN))
                {
                    string infoText = "System serial number: " + ActiveDocument.ImageInfo.SystemSN;
                    infoToPrint.Add(infoText);
                }
                if (!string.IsNullOrEmpty(ActiveDocument.ImageInfo.Comment))
                {
                    string comment = "Comment: " + ActiveDocument.ImageInfo.Comment;
                }
            }
            #endregion

            //Get scale of the print wrt to screen of WPF visual
            int nInfoLines = infoToPrint.Count;
            double imageHeight = image.Source.Height;
            double infoTextHeight = (nInfoLines * textSize.Height) + pageMargin.Top + pageMargin.Bottom + textSize.Height + 4;
            double sx = (capabilities.PageImageableArea.ExtentWidth - pageMargin.Left - pageMargin.Right) / image.Source.Width;
            double sy = (capabilities.PageImageableArea.ExtentHeight - infoTextHeight) / image.Source.Height;
            double scaleFactor = Math.Min(sx, sy);
            // Scale down to fit the page
            if (scaleFactor < 1)
            {
                //Transform the Visual to scale
                image.LayoutTransform = new ScaleTransform(scaleFactor, scaleFactor);
                imageHeight = imageHeight * scaleFactor;
            }

            // Set print margins
            //public Thickness(double left, double top, double right, double bottom);
            var imageMargin = new Thickness(pageMargin.Left, pageMargin.Top + textSize.Height + 4, pageMargin.Right, 0);
            image.Margin = imageMargin;

            fp.Children.Add(image);

            #region Print Image Info

            if (ActiveDocument.ImageInfo != null && bIsPrintInfo)
            {
                double topMargin = pageMargin.Top + textSize.Height + imageHeight;

                foreach (var info in infoToPrint)
                {
                    var infoTextBlock = new TextBlock();
                    infoTextBlock.Text = info;
                    infoTextBlock.FontFamily = new FontFamily("Trebuchet MS");
                    infoTextBlock.FontSize = 12;
                    infoTextBlock.TextWrapping = TextWrapping.WrapWithOverflow;
                    //textSize = MeasureString(infoText.Text, infoText.FontSize);
                    topMargin += textSize.Height;
                    infoTextBlock.Margin = new Thickness(pageMargin.Left, topMargin, pageMargin.Right, 0);

                    fp.Children.Add(infoTextBlock);
                }
            }

            #endregion

            System.Windows.Documents.PageContent pc = new System.Windows.Documents.PageContent();
            ((System.Windows.Markup.IAddChild)pc).AddChild(fp);
            doc.Pages.Add(pc);
            printDlg.PrintDocument(doc.DocumentPaginator, "Azure Imager Print Job");
        }
        private System.Windows.Size MeasureString(string toPrint, double fontSize)
        {
            FormattedText formattedText = new FormattedText(
                toPrint,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(new FontFamily("Trebuchet MS"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                fontSize,
                Brushes.Black);

            return new System.Windows.Size(formattedText.Width, formattedText.Height);
        }
        public System.Windows.Size MeasureString(string toPrint, string fontFamily, double fontSize, FontWeight fontWeight)
        {
            var formattedText = new FormattedText(
                toPrint,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(new FontFamily(fontFamily), FontStyles.Normal, fontWeight, FontStretches.Normal),
                fontSize,
                Brushes.Black);

            return new System.Windows.Size(formattedText.Width, formattedText.Height);
        }
        #endregion

        //#region protected bool CanExecutePrintCommand(object parameter)
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="parameter"></param>
        ///// <returns></returns>
        //protected bool CanExecutePrintCommand(object parameter)
        //{
        //    return (Workspace.This.ActiveDocument != null);
        //}
        //#endregion

        /// <summary>
        /// This function prints graphics with background image.
        /// </summary>
        /*void PrintImageWithAnnotations(Image image)
        {
            PrintDialog dlg = new PrintDialog();

            if (dlg.ShowDialog().GetValueOrDefault() != true)
            {
                return;
            }

            // Calculate rectangle for image
            double width = dlg.PrintableAreaWidth / 2;
            double height = width * image.Source.Height / image.Source.Width;

            double left = (dlg.PrintableAreaWidth - width) / 2;
            double top = (dlg.PrintableAreaHeight - height) / 2;

            Rect rect = new Rect(left, top, width, height);

            // Create DrawingVisual and get its drawing context
            DrawingVisual vs = new DrawingVisual();
            DrawingContext dc = vs.RenderOpen();

            // Draw image
            dc.DrawImage(image.Source, rect);

            double scale = width / image.Source.Width;

            // Keep old existing actual scale and set new actual scale.
            double oldActualScale = ActiveDocument.DrawingCanvas.ActualScale;
            ActiveDocument.DrawingCanvas.ActualScale = scale;

            // Remove clip in the canvas - we set our own clip.
            ActiveDocument.DrawingCanvas.RemoveClip();

            // Prepare drawing context to draw graphics
            dc.PushClip(new RectangleGeometry(rect));
            dc.PushTransform(new TranslateTransform(left, top));
            dc.PushTransform(new ScaleTransform(scale, scale));

            // Ask canvas to draw overlays
            ActiveDocument.DrawingCanvas.Draw(dc);

            // Restore old actual scale.
            ActiveDocument.DrawingCanvas.ActualScale = oldActualScale;

            // Restore clip
            ActiveDocument.DrawingCanvas.RefreshClip();

            dc.Pop();
            dc.Pop();
            dc.Pop();

            dc.Close();

            // Print DrawVisual
            dlg.PrintVisual(vs, "Graphics");
        }*/

        #endregion


        #region QuitCommand
        private RelayCommand _QuitCommand = null;
        public ICommand QuitCommand
        {
            get
            {
                if (_QuitCommand == null)
                {
                    _QuitCommand = new RelayCommand((p) => ExecuteQuitCommand(p), (p) => CanExecuteQuitCommand(p));
                }

                return _QuitCommand;
            }
        }

        private void ExecuteQuitCommand(object parameter)
        {
            this.CloseAction();
        }

        private bool CanExecuteQuitCommand(object parameter)
        {
            return true;
        }

        #endregion 


        #region BlackSliderContrastCommand

        private RelayCommand _BlackSliderContrastCommand = null;
        public ICommand BlackSliderContrastCommand
        {
            get
            {
                if (_BlackSliderContrastCommand == null)
                {
                    _BlackSliderContrastCommand = new RelayCommand(ExecuteBlackSliderContrastCommand, CanExecuteBlackSliderContrastCommand);
                }

                return _BlackSliderContrastCommand;
            }
        }

        protected void ExecuteBlackSliderContrastCommand(object parameter)
        {
            if (!IsActiveDocument) { return; }

            if (ActiveDocument.IsAutoContrast)
            {
                ActiveDocument.IsManualContrast = true; // manual contrast, don't restore saved B/W/G values
                ActiveDocument.IsAutoContrast = false;
            }

            if (ActiveDocument.IsRgbImage && ActiveDocument.SelectedChannel == ImageChannelType.Mix)
            {
                // composite channel manual contrast; de-select auto in the individual channel
                ActiveDocument.ImageInfo.RedChannel.IsAutoChecked = false;
                ActiveDocument.ImageInfo.GreenChannel.IsAutoChecked = false;
                ActiveDocument.ImageInfo.BlueChannel.IsAutoChecked = false;
            }

            ActiveDocument.UpdateDisplayImage();

            //WriteableBitmap newDisplayImage = ImageProcessingHelper.UpdateDisplayImage(
            //    ActiveDocument.Image,
            //    ActiveDocument.ImageInfo);

            //if (btnHistogram.IsChecked == true)
            //{
            //    int iPixelType;
            //    // create the histogram
            //    iPixelType = MVImage.GetPixelType(bitmap.Format);
            //    if (MVImage.P8uC3_1 == iPixelType)
            //    {
            //        int[,] p8uLevels = new int[3, 256];
            //        for (int i = 0; i < 256; ++i)
            //        {
            //            p8uLevels[0, i] = i;
            //            p8uLevels[1, i] = i;
            //            p8uLevels[2, i] = i;
            //        }
            //        int[,] p8uHist = new int[3, 256];
            //        int[] pn8uLevels = new int[3];
            //        for (int i = 0; i < 3; ++i)
            //        {
            //            pn8uLevels[i] = 256;
            //        }
            //        double iScaleX = canvasHistogram.ActualWidth / 256;
            //        MVImage.ImageHistogram(bitmap, p8uHist, p8uLevels, pn8uLevels);
            //        DrawHistogram(bitmap, iPixelType, p8uHist, p8uLevels, pn8uLevels, iScaleX);
            //    }
            //}

            //if (newDisplayImage != null)
            //{
            //    ActiveDocument.DisplayImage = newDisplayImage;
            //}
        }

        protected bool CanExecuteBlackSliderContrastCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region WhiteSliderContrastCommand

        private RelayCommand _WhiteSliderContrastCommand = null;
        public ICommand WhiteSliderContrastCommand
        {
            get
            {
                if (_WhiteSliderContrastCommand == null)
                {
                    _WhiteSliderContrastCommand = new RelayCommand(ExecuteWhiteSliderContrastCommand, CanExecuteWhiteSliderContrastCommand);
                }

                return _WhiteSliderContrastCommand;
            }
        }

        protected void ExecuteWhiteSliderContrastCommand(object parameter)
        {
            if (!IsActiveDocument) { return; }

            if (ActiveDocument.IsAutoContrast == true)
            {
                ActiveDocument.IsManualContrast = true; // manual contrast, don't restore saved B/W/G values
                ActiveDocument.IsAutoContrast = false;
            }

            if (ActiveDocument.IsRgbImage && ActiveDocument.SelectedChannel == ImageChannelType.Mix)
            {
                // composite channel manual contrast; de-select auto in the individual channel
                ActiveDocument.ImageInfo.RedChannel.IsAutoChecked = false;
                ActiveDocument.ImageInfo.GreenChannel.IsAutoChecked = false;
                ActiveDocument.ImageInfo.BlueChannel.IsAutoChecked = false;
            }

            ActiveDocument.UpdateDisplayImage();

            //WriteableBitmap newDisplayImage = ImageProcessingHelper.UpdateDisplayImage(
            //    ActiveDocument.Image,
            //    ActiveDocument.ImageInfo);

            //if (btnHistogram.IsChecked == true)
            //{
            //    int iPixelType;
            //    // create the histogram
            //    iPixelType = MVImage.GetPixelType(bitmap.Format);
            //
            //    //int iMax = MVImage.Max(m_MainWindow.imageTabControl.CurrentBitmapSource);
            //
            //    if (iPixelType == MVImage.P8uC1)
            //    {
            //        int[,] p8uLevels = new int[1, 256];
            //        for (int i = 0; i < 256; ++i)
            //        {
            //            p8uLevels[0, i] = i;
            //        }
            //        int[,] p8uHist = new int[1, 256];
            //        int[] pn8uLevels = new int[1];
            //        for (int i = 0; i < 1; ++i)
            //        {
            //            pn8uLevels[i] = 256;
            //        }
            //        double iScaleX = canvasHistogram.ActualWidth / 256;
            //        MVImage.ImageHistogram(_MainWindow.imageTabControl.CurrentBitmap, p8uHist, p8uLevels, pn8uLevels);
            //        DrawHistogram(_MainWindow.imageTabControl.CurrentBitmap, iPixelType, p8uHist, p8uLevels, pn8uLevels, iScaleX);
            //    }
            //    else if (MVImage.P8uC3_1 == iPixelType)
            //    {
            //        int[,] p8uLevels = new int[3, 256];
            //        for (int i = 0; i < 256; ++i)
            //        {
            //            p8uLevels[0, i] = i;
            //            p8uLevels[1, i] = i;
            //            p8uLevels[2, i] = i;
            //        }
            //        int[,] p8uHist = new int[3, 256];
            //        int[] pn8uLevels = new int[3];
            //        for (int i = 0; i < 3; ++i)
            //        {
            //            pn8uLevels[i] = 256;
            //        }
            //
            //        double iScaleX = canvasHistogram.ActualWidth / 256;
            //        MVImage.ImageHistogram(bitmap, p8uHist, p8uLevels, pn8uLevels);
            //        DrawHistogram(_MainWindow.imageTabControl.CurrentBitmap, iPixelType, p8uHist, p8uLevels, pn8uLevels, iScaleX);
            //    }
            //    else if (iPixelType == MVImage.P16uC1)
            //    {
            //        int[,] pLevels = new int[1, 65536];
            //        for (int i = 0; i < 65536; ++i)
            //        {
            //            pLevels[0, i] = i;
            //        }
            //        int[,] pHist = new int[1, 65536];
            //        int[] pnLevels = new int[1];
            //        for (int i = 0; i < 1; ++i)
            //        {
            //            pnLevels[i] = 65536;
            //        }
            //        double iScaleX = canvasHistogram.ActualWidth / 65536;
            //        MVImage.ImageHistogram(_MainWindow.imageTabControl.CurrentBitmap, pHist
            //            , pLevels, pnLevels);
            //        DrawHistogram(_MainWindow.imageTabControl.CurrentBitmap, iPixelType, pHist, pLevels, pnLevels, iScaleX);
            //    }
            //    else if (MVImage.P16uC3 == iPixelType)
            //    {
            //        double iScaleX = canvasHistogram.ActualWidth / 65536;
            //        //MVImage.ImageHistogram(m_MainWindow.imageTabControl.CurrentBitmapSource, p16uHist, p16uLevels, pn16uLevels);
            //    }
            //}

            //if (newDisplayImage != null)
            //{
            //    ActiveDocument.DisplayImage = newDisplayImage;
            //}
        }

        protected bool CanExecuteWhiteSliderContrastCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region GammaSliderContrastCommand

        private RelayCommand _GammaSliderContrastCommand = null;
        public ICommand GammaSliderContrastCommand
        {
            get
            {
                if (_GammaSliderContrastCommand == null)
                {
                    _GammaSliderContrastCommand = new RelayCommand(ExecuteGammaSliderContrastCommand, CanExecuteGammaSliderContrastCommand);
                }

                return _GammaSliderContrastCommand;
            }
        }

        protected void ExecuteGammaSliderContrastCommand(object parameter)
        {
            if (!IsActiveDocument) { return; }

            if (ActiveDocument.IsAutoContrast == true)
            {
                ActiveDocument.IsManualContrast = true; // manual contrast, don't restore the saved B/W/G values
                ActiveDocument.IsAutoContrast = false;
            }

            if (ActiveDocument.IsRgbImage && ActiveDocument.SelectedChannel == ImageChannelType.Mix)
            {
                // composite channel manual contrast; de-select auto in the individual channel
                ActiveDocument.ImageInfo.RedChannel.IsAutoChecked = false;
                ActiveDocument.ImageInfo.GreenChannel.IsAutoChecked = false;
                ActiveDocument.ImageInfo.BlueChannel.IsAutoChecked = false;
            }

            ActiveDocument.UpdateDisplayImage();
        }

        protected bool CanExecuteGammaSliderContrastCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region public ICommand UpdateBlackContrastOnEnterCommand

        private RelayCommand _UpdateBlackContrastOnEnterCommand = null;
        public ICommand UpdateBlackContrastOnEnterCommand
        {
            get
            {
                if (_UpdateBlackContrastOnEnterCommand == null)
                {
                    _UpdateBlackContrastOnEnterCommand = new RelayCommand(ExecuteUpdateBlackContrastOnEnterCommand, CanExecuteUpdateBlackContrastOnEnterCommand);
                }

                return _UpdateBlackContrastOnEnterCommand;
            }
        }

        protected void ExecuteUpdateBlackContrastOnEnterCommand(object parameter)
        {
            System.Windows.Controls.TextBox txtBox = parameter as System.Windows.Controls.TextBox;
            if (txtBox != null)
            {
                //DependencyProperty prop = TextBox.TextProperty;
                //BindingExpression binding = BindingOperations.GetBindingExpression(tBox, prop);
                //if (binding != null)
                //    binding.UpdateSource();

                int blackValue = 0;
                if (int.TryParse(txtBox.Text, out blackValue))
                {
                    // Don't allow the black value to be < 0 and > MaxWhiteValue - 1.
                    if (blackValue < 0)
                    {
                        blackValue = 0;
                    }
                    else if (blackValue > (ActiveDocument.MaxWhiteValue - 1))
                    {
                        blackValue = ActiveDocument.MaxWhiteValue - 1;
                    }

                    // Do not allow the black value to be >= to the white value.
                    if (blackValue >= ActiveDocument.WhiteValue)
                    {
                        ActiveDocument.WhiteValue = (blackValue < (ActiveDocument.MaxWhiteValue - 1)) ? (blackValue + 1) : ActiveDocument.MaxWhiteValue;
                    }
                    ActiveDocument.BlackValue = blackValue;
                    ExecuteBlackSliderContrastCommand(null);
                }
                else
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show("Please entry a valid value.");
                }
            }
        }

        protected bool CanExecuteUpdateBlackContrastOnEnterCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region public ICommand UpdateWhiteContrastOnEnterCommand

        private RelayCommand _UpdateWhiteContrastOnEnterCommand = null;
        public ICommand UpdateWhiteContrastOnEnterCommand
        {
            get
            {
                if (_UpdateWhiteContrastOnEnterCommand == null)
                {
                    _UpdateWhiteContrastOnEnterCommand = new RelayCommand(ExecuteUpdateWhiteContrastOnEnterCommand, CanExecuteUpdateWhiteContrastOnEnterCommand);
                }

                return _UpdateWhiteContrastOnEnterCommand;
            }
        }

        protected void ExecuteUpdateWhiteContrastOnEnterCommand(object parameter)
        {
            System.Windows.Controls.TextBox txtBox = parameter as System.Windows.Controls.TextBox;
            if (txtBox != null)
            {
                //DependencyProperty prop = TextBox.TextProperty;
                //BindingExpression binding = BindingOperations.GetBindingExpression(tBox, prop);
                //if (binding != null)
                //    binding.UpdateSource();

                int whiteValue = 0;
                if (int.TryParse(txtBox.Text, out whiteValue))
                {
                    // Restrict the white value to be between 1 and MaxWhiteValue.
                    if (whiteValue > ActiveDocument.MaxWhiteValue)
                    {
                        whiteValue = ActiveDocument.MaxWhiteValue;
                    }
                    else if (whiteValue < 1)
                    {
                        whiteValue = 1;
                    }
                    // Do not allow the white value to be <= to the black value.
                    if (whiteValue <= ActiveDocument.BlackValue)
                    {
                        ActiveDocument.BlackValue = (whiteValue >= 1) ? (whiteValue - 1) : 0;
                    }
                    ActiveDocument.WhiteValue = whiteValue;
                    ExecuteWhiteSliderContrastCommand(null);
                }
                else
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show("Please entry a valid value.");
                }
            }
        }

        protected bool CanExecuteUpdateWhiteContrastOnEnterCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region public ICommand UpdateGammaOnEnterCommand

        private RelayCommand _UpdateGammaOnEnterCommand = null;
        public ICommand UpdateGammaOnEnterCommand
        {
            get
            {
                if (_UpdateGammaOnEnterCommand == null)
                {
                    _UpdateGammaOnEnterCommand = new RelayCommand(ExecuteUpdateGammaOnEnterCommand, CanExecuteUpdateGammaOnEnterCommand);
                }

                return _UpdateGammaOnEnterCommand;
            }
        }

        protected void ExecuteUpdateGammaOnEnterCommand(object parameter)
        {
            //if (DisplayImage == null) { return; }
            System.Windows.Controls.TextBox txtBox = parameter as System.Windows.Controls.TextBox;
            if (txtBox != null)
            {
                //DependencyProperty prop = TextBox.TextProperty;
                //BindingExpression binding = BindingOperations.GetBindingExpression(tBox, prop);
                //if (binding != null)
                //    binding.UpdateSource();

                double gammaValue = 0.0;
                if (double.TryParse(txtBox.Text, out gammaValue))
                {
                    gammaValue = Math.Round(Math.Log10(gammaValue), 3); // convert to "real" gamma value
                    ActiveDocument.GammaValue = gammaValue;
                    ExecuteGammaSliderContrastCommand(null);
                }
                else
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show("Please entry a valid value.");
                }
            }
        }

        protected bool CanExecuteUpdateGammaOnEnterCommand(object parameter)
        {
            return true;
        }

        #endregion


        #region AutoContrastCommand
        private RelayCommand _AutoContrastCommand = null;
        /// <summary>
        /// Get the auto-contrast command.
        /// </summary>
        public ICommand AutoContrastCommand
        {
            get
            {
                if (_AutoContrastCommand == null)
                {
                    _AutoContrastCommand = new RelayCommand(ExecuteAutoContrastCommand, CanExecuteAutoContrastCommand);
                }

                return _AutoContrastCommand;
            }
        }
        #region protected void ExecuteAutoContrastCommand(object parameter)
        /// <summary>
        ///Auto-contrast command implementation.
        /// </summary>
        /// <param name="parameter"></param>
        protected void ExecuteAutoContrastCommand(object parameter)
        {
            //FileViewModel activeDocument = Workspace.This.ActiveDocument;
            if (ActiveDocument != null && !IsProcessingContrast)
            {
                try
                {
                    IsProcessingContrast = true;
                    ActiveDocument.UpdateDisplayImage();
                    IsProcessingContrast = false;
                }
                catch (Exception ex)
                {
                    IsProcessingContrast = false;
                    throw new Exception(string.Format("Error creating the display image.\n{0}", ex.Message));
                }
                //finally
                //{
                //    IsProcessingContrast = false;
                //}
            }
        }
        #endregion

        #region protected bool CanExecuteAutoContrastCommand(object parameter)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        protected bool CanExecuteAutoContrastCommand(object parameter)
        {
            return (IsProcessingContrast != true);
        }
        #endregion

        #endregion

        #region InvertCommand
        private RelayCommand _InvertCommand = null;
        /// <summary>
        /// Get the inversion command.
        /// </summary>
        public ICommand InvertCommand
        {
            get
            {
                if (_InvertCommand == null)
                {
                    _InvertCommand = new RelayCommand(ExecuteInvertCommand, CanExecuteInvertCommand);
                }

                return _InvertCommand;
            }
        }
        #region protected void ExecuteInvertCommand(object parameter)
        /// <summary>
        ///Invert command implementation.
        /// </summary>
        /// <param name="parameter"></param>
        protected void ExecuteInvertCommand(object parameter)
        {
            //FileViewModel activeDocument = Workspace.This.ActiveDocument;
            if (ActiveDocument != null && !IsProcessingContrast)
            {
                try
                {
                    IsProcessingContrast = true;
                    ActiveDocument.UpdateDisplayImage();
                    IsProcessingContrast = false;
                }
                catch (Exception ex)
                {
                    IsProcessingContrast = false;
                    throw new Exception(string.Format("Error creating the display image.\n{0}", ex.Message));
                }
                //finally
                //{
                //    IsProcessingContrast = false;
                //}
            }
        }
        #endregion

        #region protected bool CanExecuteInvertCommand(object parameter)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        protected bool CanExecuteInvertCommand(object parameter)
        {
            return (IsProcessingContrast != true);
        }
        #endregion

        #endregion

        #region SaturationCommand
        private RelayCommand _SaturationCommand = null;
        /// <summary>
        /// Get the saturation command.
        /// </summary>
        public ICommand SaturationCommand
        {
            get
            {
                if (_SaturationCommand == null)
                {
                    _SaturationCommand = new RelayCommand(this.ExecuteSaturationCommand, CanExecuteSaturationCommand);
                }

                return _SaturationCommand;
            }
        }
        #region protected void ExecuteSaturationCommand(object parameter)
        /// <summary>
        ///Saturation command implementation.
        /// </summary>
        /// <param name="parameter"></param>
        protected void ExecuteSaturationCommand(object parameter)
        {
            //FileViewModel activeDocument = Workspace.This.ActiveDocument;
            if (ActiveDocument != null && !IsProcessingContrast)
            {
                try
                {
                    IsProcessingContrast = true;
                    //if (ActiveDocument.Image.Format == PixelFormats.Gray8 || ActiveDocument.Image.Format == PixelFormats.Gray16)
                    //{
                    //    bool bIsSaturation = ActiveDocument.ImageInfo.IsSaturationChecked;
                    //    int nWidth = ActiveDocument.Width;
                    //    int nHeight = ActiveDocument.Height;
                    //    BitmapPalette palette = new BitmapPalette(ImageProcessing.GetColorTableIndexed(bIsSaturation));
                    //    ActiveDocument.DisplayImage = new WriteableBitmap(nWidth, nHeight, 96, 96, PixelFormats.Indexed8, palette);
                    //    //ActiveDocument.DisplayImage.Freeze();
                    //}
                    ActiveDocument.UpdateDisplayImage();
                    IsProcessingContrast = false;
                }
                catch (Exception ex)
                {
                    IsProcessingContrast = false;
                    throw new Exception(string.Format("Error creating the display image.\n{0}", ex.Message));
                }
                //finally
                //{
                //    IsProcessingContrast = false;
                //}
            }
        }
        #endregion

        #region protected bool CanExecuteSaturationCommand(object parameter)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        protected bool CanExecuteSaturationCommand(object parameter)
        {
            return (IsProcessingContrast != true);
        }
        #endregion

        #endregion


        #region DisplayRedChCommand

        private RelayCommand _DisplayRedChCommand = null;
        public ICommand DisplayRedChCommand
        {
            get
            {
                if (_DisplayRedChCommand == null)
                {
                    _DisplayRedChCommand = new RelayCommand(ExecuteDisplayRedChCommand, CanExecuteDisplayRedChCommand);
                }

                return _DisplayRedChCommand;
            }
        }

        protected void ExecuteDisplayRedChCommand(object parameter)
        {
            if (ActiveDocument != null && !IsProcessingContrast)
            {
                if (!IsActiveDocument || !ActiveDocument.IsDisplayRedChannel)
                {
                    return;
                }

                try
                {
                    IsProcessingContrast = true;

                    // Reset the overall contrast buttons
                    ActiveDocument.ImageInfo.MixChannel.IsAutoChecked = false;
                    ActiveDocument.ImageInfo.MixChannel.IsInvertChecked = false;
                    //ActiveDocument.ImageInfo.MixChannel.IsSaturationChecked = false;

                    ActiveDocument.UpdateDisplayImage();
                    IsProcessingContrast = false;
                }
                catch (Exception ex)
                {
                    IsProcessingContrast = false;
                    throw new Exception(string.Format("Error creating the display image.\n{0}", ex.Message));
                }
                //finally
                //{
                //    IsProcessingContrast = false;
                //}
            }
        }

        protected bool CanExecuteDisplayRedChCommand(object parameter)
        {
            return (IsProcessingContrast != true);
        }

        #endregion

        #region DisplayGreenChCommand

        private RelayCommand _DisplayGreenChCommand = null;
        public ICommand DisplayGreenChCommand
        {
            get
            {
                if (_DisplayGreenChCommand == null)
                {
                    _DisplayGreenChCommand = new RelayCommand(ExecuteDisplayGreenChCommand, CanExecuteDisplayGreenChCommand);
                }

                return _DisplayGreenChCommand;
            }
        }

        protected void ExecuteDisplayGreenChCommand(object parameter)
        {
            if (ActiveDocument != null && !IsProcessingContrast)
            {
                if (!IsActiveDocument || !ActiveDocument.IsDisplayGreenChannel)
                {
                    return;
                }

                try
                {
                    IsProcessingContrast = true;

                    // Reset the overall contrast buttons
                    ActiveDocument.ImageInfo.MixChannel.IsAutoChecked = false;
                    ActiveDocument.ImageInfo.MixChannel.IsInvertChecked = false;
                    //ActiveDocument.ImageInfo.MixChannel.IsSaturationChecked = false;

                    ActiveDocument.UpdateDisplayImage();
                    IsProcessingContrast = false;
                }
                catch (Exception ex)
                {
                    IsProcessingContrast = false;
                    throw new Exception(string.Format("Error creating the display image.\n{0}", ex.Message));
                }
                //finally
                //{
                //    IsProcessingContrast = false;
                //}
            }
        }

        protected bool CanExecuteDisplayGreenChCommand(object parameter)
        {
            return (IsProcessingContrast != true);
        }

        #endregion

        #region DisplayBlueChCommand

        private RelayCommand _DisplayBlueChCommand = null;
        public ICommand DisplayBlueChCommand
        {
            get
            {
                if (_DisplayBlueChCommand == null)
                {
                    _DisplayBlueChCommand = new RelayCommand(ExecuteDisplayBlueChCommand, CanExecuteDisplayBlueChCommand);
                }

                return _DisplayBlueChCommand;
            }
        }

        protected void ExecuteDisplayBlueChCommand(object parameter)
        {
            if (ActiveDocument != null && !IsProcessingContrast)
            {
                if (!IsActiveDocument || !ActiveDocument.IsDisplayBlueChannel)
                {
                    return;
                }

                try
                {
                    IsProcessingContrast = true;

                    // Reset the overall contrast buttons
                    ActiveDocument.ImageInfo.MixChannel.IsAutoChecked = false;
                    ActiveDocument.ImageInfo.MixChannel.IsInvertChecked = false;
                    //ActiveDocument.ImageInfo.MixChannel.IsSaturationChecked = false;

                    ActiveDocument.UpdateDisplayImage();
                    IsProcessingContrast = false;
                }
                catch (Exception ex)
                {
                    IsProcessingContrast = false;
                    throw new Exception(string.Format("Error creating the display image.\n{0}", ex.Message));
                }
                //finally
                //{
                //    IsProcessingContrast = false;
                //}
            }
        }

        protected bool CanExecuteDisplayBlueChCommand(object parameter)
        {
            return (IsProcessingContrast != true);
        }

        #endregion

        #region DisplayGrayChCommand

        private RelayCommand _DisplayGrayChCommand = null;
        public ICommand DisplayGrayChCommand
        {
            get
            {
                if (_DisplayGrayChCommand == null)
                {
                    _DisplayGrayChCommand = new RelayCommand(ExecuteDisplayGrayChCommand, CanExecuteDisplayGrayChCommand);
                }

                return _DisplayGrayChCommand;
            }
        }

        protected void ExecuteDisplayGrayChCommand(object parameter)
        {
            if (ActiveDocument != null && !IsProcessingContrast)
            {
                if (!IsActiveDocument || !ActiveDocument.IsDisplayGrayChannel)
                {
                    return;
                }

                try
                {
                    IsProcessingContrast = true;

                    // Reset the overall contrast buttons
                    ActiveDocument.ImageInfo.MixChannel.IsAutoChecked = false;
                    ActiveDocument.ImageInfo.MixChannel.IsInvertChecked = false;
                    //ActiveDocument.ImageInfo.MixChannel.IsSaturationChecked = false;

                    ActiveDocument.UpdateDisplayImage();
                    IsProcessingContrast = false;
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Error creating the display image.\n{0}", ex.Message));
                }
                //finally
                //{
                //    IsProcessingContrast = false;
                //}
            }
        }

        protected bool CanExecuteDisplayGrayChCommand(object parameter)
        {
            return (IsProcessingContrast != true);
        }

        #endregion

        #region DisplayCompositeCommand

        private RelayCommand _DisplayCompositeCommand = null;
        public ICommand DisplayCompositeCommand
        {
            get
            {
                if (_DisplayCompositeCommand == null)
                {
                    _DisplayCompositeCommand = new RelayCommand(ExecuteDisplayCompositeCommand, CanExecuteDisplayCompositeCommand);
                }

                return _DisplayCompositeCommand;
            }
        }

        protected void ExecuteDisplayCompositeCommand(object parameter)
        {
            if (ActiveDocument != null && !IsProcessingContrast)
            {
                if (!IsActiveDocument)
                {
                    return;
                }

                if (ActiveDocument.Image.Format.BitsPerPixel != 24 &&
                    ActiveDocument.Image.Format.BitsPerPixel != 48 &&
                    ActiveDocument.Image.Format.BitsPerPixel != 64)
                {
                    return;
                }

                try
                {
                    IsProcessingContrast = true;

                    /*if (ActiveDocument.ImageInfo.RedChannel.IsAutoChecked ||
                        ActiveDocument.ImageInfo.GreenChannel.IsAutoChecked ||
                        ActiveDocument.ImageInfo.BlueChannel.IsAutoChecked)
                    {
                        WriteableBitmap[] imageChannels = ImageProcessing.GetChannel(ActiveDocument.Image);
                        if (imageChannels != null)
                        {
                            // GetChannel returns RGB color order
                            //
                            int nBlackValue = 0;
                            int nWhiteValue = 0;
                            if (ActiveDocument.ImageInfo.RedChannel.IsAutoChecked)
                            {
                                ImageProcessing.GetAutoScaleValues(imageChannels[0], ref nWhiteValue, ref nBlackValue);
                                ActiveDocument.ImageInfo.RedChannel.BlackValue = nBlackValue;
                                ActiveDocument.ImageInfo.RedChannel.WhiteValue = nWhiteValue;
                                ActiveDocument.ImageInfo.RedChannel.GammaValue = 1.0;
                            }
                            if (ActiveDocument.ImageInfo.GreenChannel.IsAutoChecked)
                            {
                                ImageProcessing.GetAutoScaleValues(imageChannels[1], ref nWhiteValue, ref nBlackValue);
                                ActiveDocument.ImageInfo.GreenChannel.BlackValue = nBlackValue;
                                ActiveDocument.ImageInfo.GreenChannel.WhiteValue = nWhiteValue;
                                ActiveDocument.ImageInfo.GreenChannel.GammaValue = 1.0;
                            }
                            if (ActiveDocument.ImageInfo.BlueChannel.IsAutoChecked)
                            {
                                ImageProcessing.GetAutoScaleValues(imageChannels[2], ref nWhiteValue, ref nBlackValue);
                                ActiveDocument.ImageInfo.BlueChannel.BlackValue = nBlackValue;
                                ActiveDocument.ImageInfo.BlueChannel.WhiteValue = nWhiteValue;
                                ActiveDocument.ImageInfo.BlueChannel.GammaValue = 1.0;
                            }
                            imageChannels = null;
                        }
                    }*/

                    //int nColorGradation = ActiveDocument.ImageInfo.NumOfChannels;
                    //ActiveDocument.UpdateDisplayImage(nColorGradation, true);
                    ActiveDocument.UpdateDisplayImage();
                    IsProcessingContrast = false;
                }
                catch (Exception ex)
                {
                    IsProcessingContrast = false;
                    throw new Exception(string.Format("Error creating the display image.\n{0}", ex.Message));
                }
                //finally
                //{
                //    IsProcessingContrast = false;
                //
                //    // Forces a garbage collection
                //    //GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                //    //GC.WaitForPendingFinalizers();
                //    //GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                //}
            }
        }

        protected bool CanExecuteDisplayCompositeCommand(object parameter)
        {
            return (IsProcessingContrast != true);
        }

        #endregion

        #region ExtractGrayscaleCommand

        private RelayCommand _ExtractGrayscaleCommand = null;
        public ICommand ExtractGrayscaleCommand
        {
            get
            {
                if (_ExtractGrayscaleCommand == null)
                {
                    _ExtractGrayscaleCommand = new RelayCommand(ExecuteExtractGrayscaleCommand, CanExecuteExtractGrayscaleCommand);
                }

                return _ExtractGrayscaleCommand;
            }
        }

        protected void ExecuteExtractGrayscaleCommand(object parameter)
        {
            if (!IsActiveDocument || !ActiveDocument.IsRgbImage) { return; }

            if (ActiveDocument.Image != null)
            {
                if (ActiveDocument.Image.Format.BitsPerPixel != 24 &&
                    ActiveDocument.Image.Format.BitsPerPixel != 48 &&
                    ActiveDocument.Image.Format.BitsPerPixel != 64)
                {
                    return;
                }

                WriteableBitmap[] imageChannels = null;
                imageChannels = ImageProcessing.GetChannel(ActiveDocument.Image);

                if (imageChannels == null)
                {
                    return;
                }

                var cpyActiveImageInfo = (ImageInfo)ActiveDocument.ImageInfo.Clone();
                cpyActiveImageInfo.IsRedChannelAvail = false;
                cpyActiveImageInfo.IsGreenChannelAvail = false;
                cpyActiveImageInfo.IsBlueChannelAvail = false;
                cpyActiveImageInfo.IsGrayChannelAvail = false;

                // Check available channels
                for (int i = 0; i < imageChannels.Length; i++)
                {
                    if (imageChannels[i] != null)
                    {
                        int max = 0;
                        System.Windows.Rect roiRect = new System.Windows.Rect(0, 0, imageChannels[i].PixelWidth, imageChannels[i].PixelHeight);
                        max = ImageProcessing.Max(imageChannels[i], roiRect);
                        if (max > 0)
                        {
                            switch (i)
                            {
                                case 0:
                                    cpyActiveImageInfo.IsRedChannelAvail = true;
                                    break;
                                case 1:
                                    cpyActiveImageInfo.IsGreenChannelAvail = true;
                                    break;
                                case 2:
                                    cpyActiveImageInfo.IsBlueChannelAvail = true;
                                    break;
                                case 3:
                                    cpyActiveImageInfo.IsGrayChannelAvail = true;
                                    break;
                            }
                        }
                    }
                }

                string currentTitle = ActiveDocument.Title;
                string newTitle = string.Empty;

                #region === Red channel ===
                // red channel
                if (imageChannels[0] != null &&
                    (cpyActiveImageInfo.IsRedChannelAvail || cpyActiveImageInfo.AvailChannelFlags == ImageChannelFlag.None))
                {
                    ImageInfo cpyInfo = new ImageInfo();
                    cpyInfo = cpyActiveImageInfo.Clone() as ImageInfo;
                    var laserType = GetImageInfoLaserType(cpyActiveImageInfo, ImageChannelType.Red);
                    // reset imaging channels.
                    cpyInfo.RedChannel = new ImageChannel();
                    cpyInfo.GreenChannel = new ImageChannel();
                    cpyInfo.BlueChannel = new ImageChannel();
                    cpyInfo.GrayChannel = new ImageChannel();
                    cpyInfo.MixChannel = (ImageChannel)cpyActiveImageInfo.RedChannel.Clone();
                    cpyInfo.NumOfChannels = 1;
                    cpyInfo.AvailChannelFlags = ImageChannelFlag.None;
                    cpyInfo.SelectedChannel = ImageChannelType.Mix;

                    string wavelength = string.Empty;
                    if (cpyInfo.IsScannedImage && laserType != LaserType.None && cpyInfo.MixChannel.LaserWavelength == null)
                    {
                        ExtractAndSetLaserInfo(ref cpyInfo, laserType);
                        wavelength = GetLaserWaveLength(cpyInfo, laserType);
                    }
                    else
                    {
                        if (cpyInfo.MixChannel.LaserWavelength != null)
                            wavelength = cpyInfo.MixChannel.LaserWavelength;
                        //cpyInfo.IntensityLevel = cpyInfo.MixChannel.LaserIntensityLevel.ToString();
                        if (cpyInfo.MajorVersion >= 1 && cpyInfo.MinorVersion >= 5)
                        {
                            cpyInfo.IntensityLevel = IntensityLevels[cpyInfo.MixChannel.LaserIntensityLevel];
                        }
                        else
                        {
                            cpyInfo.IntensityLevel = cpyInfo.MixChannel.LaserIntensityLevel.ToString();
                        }
                    }

                    if (string.IsNullOrEmpty(wavelength) || wavelength.Equals("0"))
                    {
                        wavelength = "Red";
                    }

                    if (CheckSupportedFileType(currentTitle))
                        newTitle = Path.GetFileNameWithoutExtension(currentTitle) + "-" + wavelength;
                    else
                        newTitle = currentTitle + "-" + wavelength;

                    if (DocumentExists(newTitle))
                    {
                        newTitle = GetUniqueFilename(newTitle);
                    }

                    NewDocument(imageChannels[0], cpyInfo, newTitle, false, false);
                }
                #endregion

                #region === Green channel ===
                // green channel
                if (imageChannels[1] != null &&
                    (cpyActiveImageInfo.IsGreenChannelAvail || cpyActiveImageInfo.AvailChannelFlags == ImageChannelFlag.None))
                {
                    ImageInfo cpyInfo = new ImageInfo();
                    cpyInfo = cpyActiveImageInfo.Clone() as ImageInfo;
                    var laserType = GetImageInfoLaserType(cpyActiveImageInfo, ImageChannelType.Green);
                    // reset imaging channels.
                    cpyInfo.RedChannel = new ImageChannel();
                    cpyInfo.GreenChannel = new ImageChannel();
                    cpyInfo.BlueChannel = new ImageChannel();
                    cpyInfo.GrayChannel = new ImageChannel();
                    cpyInfo.MixChannel = (ImageChannel)cpyActiveImageInfo.GreenChannel.Clone();
                    cpyInfo.NumOfChannels = 1;
                    cpyInfo.AvailChannelFlags = ImageChannelFlag.None;
                    cpyInfo.SelectedChannel = ImageChannelType.Mix;

                    // Extract laser scanned image captured with version 1.2 or older
                    string wavelength = string.Empty;
                    if (cpyInfo.IsScannedImage && laserType != LaserType.None && cpyInfo.MixChannel.LaserWavelength == null)
                    {
                        ExtractAndSetLaserInfo(ref cpyInfo, laserType);
                        wavelength = GetLaserWaveLength(cpyInfo, laserType);
                    }
                    else
                    {
                        if (cpyInfo.MixChannel.LaserWavelength != null)
                            wavelength = cpyInfo.MixChannel.LaserWavelength;
                        //cpyInfo.IntensityLevel = cpyInfo.MixChannel.LaserIntensityLevel.ToString();
                        if (cpyInfo.MajorVersion >= 1 && cpyInfo.MinorVersion >= 5)
                        {
                            cpyInfo.IntensityLevel = IntensityLevels[cpyInfo.MixChannel.LaserIntensityLevel];
                        }
                        else
                        {
                            cpyInfo.IntensityLevel = cpyInfo.MixChannel.LaserIntensityLevel.ToString();
                        }
                    }

                    if (string.IsNullOrEmpty(wavelength) || wavelength.Equals("0"))
                    {
                        wavelength = "Green";
                    }

                    if (CheckSupportedFileType(currentTitle))
                        newTitle = Path.GetFileNameWithoutExtension(currentTitle) + "-" + wavelength;
                    else
                        newTitle = currentTitle + "-" + wavelength;

                    if (DocumentExists(newTitle))
                    {
                        newTitle = GetUniqueFilename(newTitle);
                    }

                    NewDocument(imageChannels[1], cpyInfo, newTitle, false, false);
                }
                #endregion

                #region === Blue Channel ===
                // blue channel
                if (imageChannels[2] != null &&
                    (cpyActiveImageInfo.IsBlueChannelAvail || cpyActiveImageInfo.AvailChannelFlags == ImageChannelFlag.None))
                {
                    ImageInfo cpyInfo = new ImageInfo();
                    cpyInfo = (ImageInfo)cpyActiveImageInfo.Clone();
                    var laserType = GetImageInfoLaserType(cpyActiveImageInfo, ImageChannelType.Blue);
                    // reset imaging channels.
                    cpyInfo.RedChannel = new ImageChannel();
                    cpyInfo.GreenChannel = new ImageChannel();
                    cpyInfo.BlueChannel = new ImageChannel();
                    cpyInfo.GrayChannel = new ImageChannel();
                    cpyInfo.MixChannel = (ImageChannel)cpyActiveImageInfo.BlueChannel.Clone();
                    cpyInfo.NumOfChannels = 1;
                    cpyInfo.AvailChannelFlags = ImageChannelFlag.None;
                    cpyInfo.SelectedChannel = ImageChannelType.Mix;

                    // Extract laser scanned image captured with version 1.2 or older
                    string wavelength = string.Empty;
                    if (cpyInfo.IsScannedImage && laserType != LaserType.None && cpyInfo.MixChannel.LaserWavelength == null)
                    {
                        ExtractAndSetLaserInfo(ref cpyInfo, laserType);
                        wavelength = GetLaserWaveLength(cpyInfo, laserType);
                    }
                    else
                    {
                        if (cpyInfo.MixChannel.LaserWavelength != null)
                            wavelength = cpyInfo.MixChannel.LaserWavelength;
                        //cpyInfo.IntensityLevel = cpyInfo.MixChannel.LaserIntensityLevel.ToString();
                        if (cpyInfo.MajorVersion >= 1 && cpyInfo.MinorVersion >= 5)
                        {
                            cpyInfo.IntensityLevel = IntensityLevels[cpyInfo.MixChannel.LaserIntensityLevel];
                        }
                        else
                        {
                            cpyInfo.IntensityLevel = cpyInfo.MixChannel.LaserIntensityLevel.ToString();
                        }
                    }

                    if (string.IsNullOrEmpty(wavelength) || wavelength.Equals("0"))
                    {
                        wavelength = "Blue";
                    }

                    if (CheckSupportedFileType(currentTitle))
                        newTitle = Path.GetFileNameWithoutExtension(currentTitle) + "-" + wavelength;
                    else
                        newTitle = currentTitle + "-" + wavelength;

                    if (DocumentExists(newTitle))
                    {
                        newTitle = GetUniqueFilename(newTitle);
                    }

                    NewDocument(imageChannels[2], cpyInfo, newTitle, false, false);
                }
                #endregion

                #region === Gray Channel ===

                if (imageChannels.Length == 4)
                {
                    // gray channel
                    if (imageChannels[3] != null &&
                        (cpyActiveImageInfo.IsGrayChannelAvail || cpyActiveImageInfo.AvailChannelFlags == ImageChannelFlag.None))
                    {
                        ImageInfo cpyInfo = new ImageInfo();
                        cpyInfo = (ImageInfo)cpyActiveImageInfo.Clone();
                        var laserType = GetImageInfoLaserType(cpyActiveImageInfo, ImageChannelType.Gray);

                        // reset imaging channels.
                        cpyInfo.RedChannel = new ImageChannel();
                        cpyInfo.GreenChannel = new ImageChannel();
                        cpyInfo.BlueChannel = new ImageChannel();
                        cpyInfo.GrayChannel = new ImageChannel();
                        cpyInfo.MixChannel = (ImageChannel)cpyActiveImageInfo.GrayChannel.Clone();
                        cpyInfo.NumOfChannels = 1;
                        cpyInfo.AvailChannelFlags = ImageChannelFlag.None;
                        cpyInfo.SelectedChannel = ImageChannelType.Mix;

                        // Extract laser scanned image captured with version 1.2 or older
                        string wavelength = string.Empty;
                        if (cpyInfo.IsScannedImage && laserType != LaserType.None && cpyInfo.MixChannel.LaserWavelength == null)
                        {
                            ExtractAndSetLaserInfo(ref cpyInfo, laserType);
                            wavelength = GetLaserWaveLength(cpyInfo, laserType);
                        }
                        else
                        {
                            if (cpyInfo.MixChannel.LaserWavelength != null)
                                wavelength = cpyInfo.MixChannel.LaserWavelength;
                            //cpyInfo.IntensityLevel = cpyInfo.MixChannel.LaserIntensityLevel.ToString();
                            if (cpyInfo.MajorVersion >= 1 && cpyInfo.MinorVersion >= 5)
                            {
                                cpyInfo.IntensityLevel = IntensityLevels[cpyInfo.MixChannel.LaserIntensityLevel];
                            }
                            else
                            {
                                cpyInfo.IntensityLevel = cpyInfo.MixChannel.LaserIntensityLevel.ToString();
                            }
                        }

                        if (string.IsNullOrEmpty(wavelength) || wavelength.Equals("0"))
                        {
                            wavelength = "Gray";
                        }

                        if (CheckSupportedFileType(currentTitle))
                            newTitle = Path.GetFileNameWithoutExtension(currentTitle) + "-" + wavelength;
                        else
                            newTitle = currentTitle + "-" + wavelength;

                        if (DocumentExists(newTitle))
                        {
                            newTitle = GetUniqueFilename(newTitle);
                        }

                        NewDocument(imageChannels[3], cpyInfo, newTitle, false, false);
                    }
                }

                #endregion

            }
        }

        protected bool CanExecuteExtractGrayscaleCommand(object parameter)
        {
            return true;
        }

        private LaserType GetImageInfoLaserType(ImageInfo imgInfo, ImageChannelType imgChannel)
        {
            LaserType result = LaserType.None;

            if (imgInfo == null)
            {
                return result;
            }

            if (imgChannel == ImageChannelType.Red)
            {
                if (imgInfo.RedChannel != null)
                    result = (LaserType)imgInfo.RedChannel.LightSource;
            }
            else if (imgChannel == ImageChannelType.Green)
            {
                if (imgInfo.GreenChannel != null)
                    result = (LaserType)imgInfo.GreenChannel.LightSource;
            }
            else if (imgChannel == ImageChannelType.Blue)
            {
                if (imgInfo.BlueChannel != null)
                    result = (LaserType)imgInfo.BlueChannel.LightSource;
            }
            if (imgChannel == ImageChannelType.Gray)
            {
                if (imgInfo.GrayChannel != null)
                    result = (LaserType)imgInfo.GrayChannel.LightSource;
            }

            return result;
        }

        //static readonly string[] IntensityLevels = new[] { "0", "L1", "L2", "L3", "L4", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };
        private void ExtractAndSetLaserInfo(ref ImageInfo imgInfo, LaserType laserType)
        {
            var intLevels = imgInfo.IntensityLevel.Split('/');
            //LaserType enumLaserType;
            //Enum.TryParse(laserType, out enumLaserType);
            if (laserType == LaserType.LaserA)
            {
                imgInfo.MixChannel.ApdGain = imgInfo.ApdAGain;
                imgInfo.MixChannel.ApdPga = imgInfo.ApdAPga;
                imgInfo.MixChannel.LaserIntensity = imgInfo.LaserAIntensity;
                //imgInfo.MixChannel.LaserIntensityLevel = int.Parse(intLevels[0]);
                imgInfo.IntensityLevel = intLevels[0];
                if (!string.IsNullOrEmpty(intLevels[0]))
                {
                    int nLevel = 0;
                    bool bIsSuccessful = Int32.TryParse(intLevels[0], out nLevel);
                    if (bIsSuccessful)
                    {
                        imgInfo.MixChannel.LaserIntensityLevel = nLevel;
                    }
                }
                //int laserASignalLevel = imgInfo.MixChannel.LaserIntensityLevel;
                //int laserBSignalLevel = 0;
                //int laserCSignalLevel = 0;
                //int laserDSignalLevel = 0;
                //imgInfo.IntensityLevel = string.Format("{0} / {1} / {2} / {3}", laserASignalLevel, laserBSignalLevel, laserCSignalLevel, laserDSignalLevel);
                //imgInfo.IntensityLevel = imgInfo.MixChannel.LaserIntensityLevel.ToString();
            }
            else if (laserType == LaserType.LaserB)
            {
                imgInfo.MixChannel.ApdGain = imgInfo.ApdBGain;
                imgInfo.MixChannel.ApdPga = imgInfo.ApdBPga;
                imgInfo.MixChannel.LaserIntensity = imgInfo.LaserBIntensity;
                //imgInfo.MixChannel.LaserIntensityLevel = int.Parse(intLevels[1]);
                imgInfo.IntensityLevel = intLevels[1];
                if (!string.IsNullOrEmpty(intLevels[1]))
                {
                    int nLevel = 0;
                    bool bIsSuccessful = Int32.TryParse(intLevels[1], out nLevel);
                    if (bIsSuccessful)
                    {
                        imgInfo.MixChannel.LaserIntensityLevel = nLevel;
                    }
                }
                //int laserASignalLevel = 0;
                //int laserBSignalLevel = imgInfo.MixChannel.LaserIntensityLevel;
                //int laserCSignalLevel = 0;
                //int laserDSignalLevel = 0;
                //imgInfo.IntensityLevel = string.Format("{0} / {1} / {2} / {3}", laserASignalLevel, laserBSignalLevel, laserCSignalLevel, laserDSignalLevel);
                //imgInfo.IntensityLevel = imgInfo.MixChannel.LaserIntensityLevel.ToString();
            }
            else if (laserType == LaserType.LaserC)
            {
                imgInfo.MixChannel.ApdGain = imgInfo.ApdCGain;
                imgInfo.MixChannel.ApdPga = imgInfo.ApdCPga;
                imgInfo.MixChannel.LaserIntensity = imgInfo.LaserCIntensity;
                //imgInfo.MixChannel.LaserIntensityLevel = int.Parse(intLevels[2]);
                imgInfo.IntensityLevel = intLevels[2];
                if (!string.IsNullOrEmpty(intLevels[2]))
                {
                    int nLevel = 0;
                    bool bIsSuccessful = Int32.TryParse(intLevels[2], out nLevel);
                    if (bIsSuccessful)
                    {
                        imgInfo.MixChannel.LaserIntensityLevel = nLevel;
                    }
                }
                //int laserASignalLevel = 0;
                //int laserBSignalLevel = 0;
                //int laserCSignalLevel = imgInfo.MixChannel.LaserIntensityLevel;
                //int laserDSignalLevel = 0;
                //imgInfo.IntensityLevel = string.Format("{0} / {1} / {2} / {3}", laserASignalLevel, laserBSignalLevel, laserCSignalLevel, laserDSignalLevel);
                //imgInfo.IntensityLevel = imgInfo.MixChannel.LaserIntensityLevel.ToString();
            }
            else if (laserType == LaserType.LaserD)
            {
                imgInfo.MixChannel.ApdGain = imgInfo.ApdDGain;
                imgInfo.MixChannel.ApdPga = imgInfo.ApdDPga;
                imgInfo.MixChannel.LaserIntensity = imgInfo.LaserDIntensity;
                //imgInfo.MixChannel.LaserIntensityLevel = int.Parse(intLevels[3]);
                imgInfo.IntensityLevel = intLevels[3];
                if (!string.IsNullOrEmpty(intLevels[3]))
                {
                    int nLevel = 0;
                    bool bIsSuccessful = Int32.TryParse(intLevels[3], out nLevel);
                    if (bIsSuccessful)
                    {
                        imgInfo.MixChannel.LaserIntensityLevel = nLevel;
                    }
                }
                //int laserASignalLevel = 0;
                //int laserBSignalLevel = 0;
                //int laserCSignalLevel = 0;
                //int laserDSignalLevel = imgInfo.MixChannel.LaserIntensityLevel;
                //imgInfo.IntensityLevel = string.Format("{0} / {1} / {2} / {3}", laserASignalLevel, laserBSignalLevel, laserCSignalLevel, laserDSignalLevel);
                //imgInfo.IntensityLevel = imgInfo.MixChannel.LaserIntensityLevel.ToString();
            }
        }

        #endregion

        #region CopyCommand
        private RelayCommand _CopyCommand = null;
        /// <summary>
        /// Get the copy command.
        /// </summary>
        public ICommand CopyCommand
        {
            get
            {
                if (_CopyCommand == null)
                {
                    _CopyCommand = new RelayCommand(this.ExecuteCopyCommand, CanExecuteCopyCommand);
                }

                return _CopyCommand;
            }
        }
        #region protected void ExecuteCopyCommand(object parameter)
        /// <summary>
        ///Region of interest copy command implementation.
        /// </summary>
        /// <param name="parameter"></param>
        protected void ExecuteCopyCommand(object parameter)
        {
            if (ActiveDocument != null)
            {
                if (ActiveDocument.IsSelectionToolChecked)
                {
                    // current only support 16-bit and 24-bit image
                    if (ActiveDocument.Image.Format.BitsPerPixel != 16 &&
                        ActiveDocument.Image.Format.BitsPerPixel != 24 &&
                        ActiveDocument.Image.Format.BitsPerPixel != 48)
                    {
                        ActiveDocument.IsSelectionToolChecked = false;
                        return;
                    }

                    System.Windows.Rect clipRect = ActiveDocument.SelectedRegion;

                    if (clipRect.Width == 0 || clipRect.Height == 0)
                    {
                        string caption = "Region selection error...";
                        string message = "Please reselect your region of interest.";
                        Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                        return;
                    }

                    // Special case: when 16-bit image is saved as .JPG or .BMP; the image is not reloaded,
                    // the loaded data is still 16-bit, make copy & paste feature use the display image.
                    bool bIsActiveJpgOrBmp = false;
                    string fileExtension = Path.GetExtension(ActiveDocument.FilePath);
                    if (!string.IsNullOrEmpty(fileExtension))
                    {
                        if (fileExtension.Contains(".jpg") || fileExtension.Contains(".bmp"))
                        {
                            bIsActiveJpgOrBmp = true;
                        }
                    }

                    if (ActiveDocument.Image.Format.BitsPerPixel == 24 || bIsActiveJpgOrBmp)
                    {
                        // Crop the selected region of the display image
                        _ImageClipboard.ClipImage = ImageProcessing.Crop(ActiveDocument.DisplayImage, clipRect);
                    }
                    else
                    {
                        try
                        {
                            StartWaitAnimation("Copying...");
                            // Crop the selected region of the active document
                            _ImageClipboard.ClipImage = ImageProcessing.Crop(ActiveDocument.Image, clipRect);
                            // Find the blobs (of the blot)
                            if (_ImageClipboard.ClipImage != null)
                            {
                                if (_ImageClipboard.ClipImage.Format.BitsPerPixel == 48)
                                {
                                    //extract rgb channels
                                    WriteableBitmap[] extractedImages = { null, null, null };
                                    extractedImages = ImageProcessing.GetChannel(ActiveDocument.Image);
                                    if (extractedImages != null)
                                    {
                                        //Get the blobs of the green channel
                                        _ImageClipboard.BlobsImage = ImageProcessing.FindBlobs(extractedImages[1], new System.Windows.Size(20, 20), _ImageClipboard.ClipImage, clipRect);
                                        if (_ImageClipboard.BlobsImage == null)
                                        {
                                            StopWaitAnimation();
                                            string caption = "Find blobs error...";
                                            string message = "Error finding the blobs on the selected region.\n" +
                                                             "Please select another region on the marker image.";
                                            Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                                        }
                                    }
                                }
                                else
                                {
                                    _ImageClipboard.BlobsImage = ImageProcessing.FindBlobs(ActiveDocument.Image, new System.Windows.Size(20, 20), _ImageClipboard.ClipImage, clipRect);
                                    if (_ImageClipboard.BlobsImage == null)
                                    {
                                        StopWaitAnimation();
                                        string caption = "Find blobs error...";
                                        string message = "Error finding the blobs on the selected region.\n" +
                                                         "Please select another region on the marker image.";
                                        Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            StopWaitAnimation();
                            System.Diagnostics.Debug.WriteLine(ex.Message);
                            throw;
                        }
                        finally
                        {
                            StopWaitAnimation();
                        }
                    }

                    _ImageClipboard.Title = ActiveDocument.Title;
                    clipRect.Width = _ImageClipboard.ClipImage.PixelWidth;
                    clipRect.Height = _ImageClipboard.ClipImage.PixelHeight;
                    _ImageClipboard.ClipRect = clipRect;
                    _ImageClipboard.OrigSize = new System.Windows.Size(ActiveDocument.Image.PixelWidth, ActiveDocument.Image.PixelHeight);
                    _IsImageClipboard = true;
                    ActiveDocument.IsSelectionToolChecked = false;

                    // Forces an immediate garbage collection.
                    //GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                    //GC.WaitForPendingFinalizers();
                    //GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                    // Force garbage collection.
                    //GC.Collect();
                    // Wait for all finalizers to complete before continuing.
                    //GC.WaitForPendingFinalizers();
                }
                else
                {
                    // Make duplicate image (copied image).
                    WriteableBitmap clonedImage = (WriteableBitmap)ActiveDocument.Image.Clone();
                    ImageInfo clonedImageInfo = (ImageInfo)ActiveDocument.ImageInfo.Clone();
                    int counter = 2;

                    string docTitle = string.Empty;
                    if (CheckSupportedFileType(ActiveDocument.Title))
                        docTitle = Path.GetFileNameWithoutExtension(ActiveDocument.Title);
                    else
                        docTitle = ActiveDocument.Title;

                    string newDocTitle = docTitle + " ( " + counter + " )";
                    // check if document title already
                    for (int i = 0; i < Files.Count; i++)
                    {
                        string tmpTitle = string.Empty;
                        if (CheckSupportedFileType(Files[i].Title))
                            tmpTitle = Path.GetFileNameWithoutExtension(Files[i].Title);
                        else
                            tmpTitle = Files[i].Title;

                        if (tmpTitle == newDocTitle)
                        {
                            counter++;
                            newDocTitle = docTitle + " ( " + counter + " )";
                            i = 0;
                            continue;
                        }
                    }
                    NewDocument(clonedImage, clonedImageInfo, newDocTitle, false, false);
                }
            }
        }
        #endregion

        #region protected bool CanExecuteCopyCommand(object parameter)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        protected bool CanExecuteCopyCommand(object parameter)
        {
            return (ActiveDocument != null);
        }
        #endregion

        #endregion

        #region PasteCommand
        private RelayCommand _PasteCommand = null;
        /// <summary>
        /// Get the paste command.
        /// </summary>
        public ICommand PasteCommand
        {
            get
            {
                if (_PasteCommand == null)
                {
                    _PasteCommand = new RelayCommand(this.ExecutePasteCommand, CanExecutePasteCommand);
                }

                return _PasteCommand;
            }
        }
        #region protected void ExecutePasteCommand(object parameter)
        /// <summary>
        ///Region of interest paste command implementation.
        /// </summary>
        /// <param name="parameter"></param>
        protected void ExecutePasteCommand(object parameter)
        {
            if (ActiveDocument != null)
            {
                if (_IsImageClipboard)
                {
                    // current only support 16-bit and 24-bit image image
                    if (ActiveDocument.Image.Format.BitsPerPixel != 16 &&
                        ActiveDocument.Image.Format.BitsPerPixel != 24)
                    {
                        return;
                    }

                    // Special case: when 16-bit image is saved as .JPG or .BMP; the image is not reloaded,
                    // the loaded data is still 16-bit, make copy & paste feature use the display image.
                    bool bIsActiveJpgOrBmp = false;
                    string fileExtension = Path.GetExtension(ActiveDocument.FilePath);
                    if (!string.IsNullOrEmpty(fileExtension))
                    {
                        if (fileExtension.Contains(".jpg") || fileExtension.Contains(".bmp"))
                        {
                            bIsActiveJpgOrBmp = true;
                        }
                    }

                    int clippboardBpp = _ImageClipboard.ClipImage.Format.BitsPerPixel;
                    int activeDocBpp = ActiveDocument.Image.Format.BitsPerPixel;
                    bool bIs24bitCopyAndPaste = false;

                    if (clippboardBpp == activeDocBpp)
                    {
                        if (clippboardBpp == 24 && activeDocBpp == 24)
                        {
                            bIs24bitCopyAndPaste = true;
                        }
                        else
                        {
                            if (bIsActiveJpgOrBmp && activeDocBpp == 16)
                            {
                                string caption = "Bit depth mismatch...";
                                string message = "The chemi image and the clipboard image are of different bit depth.";
                                Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                                return;
                            }
                        }
                    }
                    else
                    {
                        if (bIsActiveJpgOrBmp && clippboardBpp == 24)
                        {
                            bIs24bitCopyAndPaste = true;
                        }
                        else if (clippboardBpp != 48)
                        {
                            string caption = "Bit depth mismatch...";
                            string message = "The chemi image and the clipboard image are of different bit depth.";
                            Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                            return;
                        }
                    }

                    // check image size
                    if (_ImageClipboard.OrigSize.Width != ActiveDocument.Image.PixelWidth &&
                        _ImageClipboard.OrigSize.Height != ActiveDocument.Image.PixelHeight)
                    {
                        string caption = "Size mismatch";
                        string message = "The marker image and the current image are of different size.";
                        Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                        return;
                    }

                    try
                    {
                        StartWaitAnimation("Pasting...");

                        WriteableBitmap pastedImage = null;
                        ImageInfo imageInfo = null;
                        if (ActiveDocument.ImageInfo != null)
                        {
                            imageInfo = (ImageInfo)ActiveDocument.ImageInfo.Clone();
                        }
                        else
                        {
                            imageInfo = new ImageInfo();
                        }

                        if (bIs24bitCopyAndPaste)
                        {
                            try
                            {
                                pastedImage = ImageProcessingHelper.Paste24bppImage(ActiveDocument.DisplayImage, _ImageClipboard.ClipImage, _ImageClipboard.ClipRect);
                                imageInfo.MixChannel.IsInvertChecked = false;
                                imageInfo.SelectedChannel = ImageChannelType.Mix;
                                imageInfo.MixChannel.BlackValue = 0;
                                imageInfo.MixChannel.WhiteValue = 255;
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }
                        }
                        else
                        {
                            try
                            {
                                if (clippboardBpp == 48)
                                {
                                    pastedImage = ImageProcessingHelper.PasteRgbMarkerImage(ActiveDocument.Image,
                                                                                            _ImageClipboard.BlobsImage,
                                                                                            _ImageClipboard.ClipImage,
                                                                                            _ImageClipboard.ClipRect);
                                    if (pastedImage != null)
                                    {
                                        imageInfo.CaptureType = "Chemi + Marker";
                                        if (imageInfo.MixChannel.IsInvertChecked == true)
                                        {
                                            imageInfo.RedChannel.IsInvertChecked = true;
                                            imageInfo.GreenChannel.IsInvertChecked = true;
                                            imageInfo.BlueChannel.IsInvertChecked = true;
                                            imageInfo.GrayChannel.IsInvertChecked = true;
                                            imageInfo.SelectedChannel = ImageChannelType.Mix;
                                        }
                                    }
                                }
                                else
                                {
                                    pastedImage = ImageProcessingHelper.PasteMarkerImage(ActiveDocument.Image,
                                                                                         _ImageClipboard.BlobsImage,
                                                                                         _ImageClipboard.ClipImage,
                                                                                         _ImageClipboard.ClipRect);
                                }
                            }
                            catch (Exception)
                            {
                                StopWaitAnimation();
                                throw;
                            }
                        }

                        if (pastedImage != null)
                        {
                            //if (pastedImage.CanFreeze) { pastedImage.Freeze(); }
                            var fileNameWithoutExt = string.Empty;
                            if (CheckSupportedFileType(ActiveDocument.Title))
                                fileNameWithoutExt = Path.GetFileNameWithoutExtension(ActiveDocument.Title);
                            else
                                fileNameWithoutExt = ActiveDocument.Title;

                            string newTitle = fileNameWithoutExt + "_+_Marker";

                            // check if document name already exists/opened
                            int docCounter = 0;
                            int result = 0;
                            foreach (var doc in Files)
                            {
                                string title = doc.Title;
                                if (title.Contains(newTitle))
                                {
                                    char lastChar = title[title.Length - 1];
                                    if (!Char.IsDigit(lastChar))
                                    {
                                        docCounter = 1;
                                    }
                                    else
                                    {
                                        result = (int)Char.GetNumericValue(lastChar);
                                        if (result > docCounter)
                                        {
                                            docCounter = result;
                                        }
                                    }
                                }
                            }
                            if (docCounter > 0)
                            {
                                newTitle = newTitle + "_" + (docCounter + 1);
                            }

                            NewDocument(pastedImage, imageInfo, newTitle, false);
                        }
                    }
                    catch (Exception ex)
                    {
                        StopWaitAnimation();

                        string caption = "Chemi-marker paste error...";
                        string message = string.Format("Chemi-marker paste error: {0}", ex.Message);
                        Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                    }
                    finally
                    {
                        StopWaitAnimation();
                    }
                }
            }
        }
        #endregion

        #region protected bool CanExecutePasteCommand(object parameter)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        protected bool CanExecutePasteCommand(object parameter)
        {
            return (ActiveDocument != null && _IsImageClipboard);
        }
        #endregion

        #endregion

        private bool _IsAnimatedGifSelected = false;
        public bool IsAnimatedGifSelected
        {
            get { return _IsAnimatedGifSelected; }
            set
            {
                _IsAnimatedGifSelected = value;
                RaisePropertyChanged("IsAnimatedGifSelected");
            }
        }

        private bool _IsZStackingSelected = false;
        public bool IsZStackingSelected
        {
            get { return _IsZStackingSelected; }
            set
            {
                _IsZStackingSelected = value;
                RaisePropertyChanged("IsZStackingSelected");
            }
        }

        #region OpenAnimatedGifWinCommand

        private RelayCommand _OpenAnimatedGifWinCommand = null;
        public ICommand OpenAnimatedGifWinCommand
        {
            get
            {
                if (_OpenAnimatedGifWinCommand == null)
                {
                    _OpenAnimatedGifWinCommand = new RelayCommand(ExecuteOpenAnimatedGifWinCommand, CanExecuteOpenAnimatedGifWinCommand);
                }

                return _OpenAnimatedGifWinCommand;
            }
        }

        protected void ExecuteOpenAnimatedGifWinCommand(object parameter)
        {
            try
            {
                ObservableCollection<FileViewModel> sourceFiles = null;
                if (Files != null)
                    sourceFiles = new ObservableCollection<FileViewModel>(Files);

                var animatedGifVm = new AnimatedGifViewModel();
                if (sourceFiles != null && sourceFiles.Count > 0)
                {
                    animatedGifVm.SourceFiles = sourceFiles;
                    animatedGifVm.SelectedSourceFile = animatedGifVm.SourceFiles[0];
                }

                AnimatedGifWindow animatedWin = new AnimatedGifWindow();
                animatedWin.DataContext = animatedGifVm;
                animatedWin.Owner = Application.Current.MainWindow;
                animatedWin.ShowDialog();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                IsAnimatedGifSelected = false;
            }
        }

        protected bool CanExecuteOpenAnimatedGifWinCommand(object parameter)
        {
            return true;
        }
        #endregion

        #region OpenZStackWinCommand

        private RelayCommand _OpenZStackWinCommand = null;
        public ICommand OpenZStackWinCommand
        {
            get
            {
                if (_OpenZStackWinCommand == null)
                {
                    _OpenZStackWinCommand = new RelayCommand(ExecuteOpenZStackWinCommand, CanExecuteOpenZStackWinCommand);
                }

                return _OpenZStackWinCommand;
            }
        }

        protected void ExecuteOpenZStackWinCommand(object parameter)
        {
            try
            {
                ObservableCollection<FileViewModel> sourceFiles = null;
                if (Files != null)
                {
                    sourceFiles = new ObservableCollection<FileViewModel>(Files);
                }

                var zstackingVm = new ZStackingViewModel();
                if (sourceFiles != null && sourceFiles.Count > 0)
                {
                    zstackingVm.SourceFiles = sourceFiles;
                    zstackingVm.SelectedSourceFile = zstackingVm.SourceFiles.FirstOrDefault();
                }

                ZStackingWindow zstackingWin = new ZStackingWindow();
                zstackingWin.DataContext = zstackingVm;
                zstackingWin.Owner = Application.Current.MainWindow;
                zstackingVm.OnRequestClose += (s, e) => zstackingWin.Close();
                var dlgResult = zstackingWin.ShowDialog();
                //if (dlgResult == true)
                //{
                //    zstackingVm.IsCloseDialog = true;
                //}
                IsZStackingSelected = false;
            }
            catch (Exception)
            {
                IsZStackingSelected = false;
                throw;
            }
            //finally
            //{
            //    IsZStackingSelected = false;
            //}
        }

        protected bool CanExecuteOpenZStackWinCommand(object parameter)
        {
            return true;
        }
        #endregion


        #region OpenScaleBarWinCommand

        private RelayCommand _OpenScaleBarWinCommand = null;
        public ICommand OpenScaleBarWinCommand
        {
            get
            {
                if (_OpenScaleBarWinCommand == null)
                {
                    _OpenScaleBarWinCommand = new RelayCommand(ExecuteOpenScaleBarWinCommand, CanExecuteOpenScaleBarWinCommand);
                }

                return _OpenScaleBarWinCommand;
            }
        }

        protected void ExecuteOpenScaleBarWinCommand(object parameter)
        {
            try
            {
                if (_ActiveDocument == null)
                {
                    return;
                }

                if (_ScaleBarVm == null)
                {
                    _ScaleBarVm = new ScaleBarViewModel();
                    _ScaleBarVm.SelectedColor = Colors.White;
                }

                var scaleBarWin = new ScaleBarWindow();
                _ScaleBarVm.ActiveDocument = ActiveDocument;
                scaleBarWin.DataContext = _ScaleBarVm;

                if (!_ScaleBarVm.IsShowScalebar)
                {
                    _ScaleBarVm.IsShowScalebar = true;
                }

                //scaleBarWin.Owner = Application.Current.MainWindow;
                scaleBarWin.Topmost = true;
                //scaleBarWin.Show();
                bool? dlgResult = scaleBarWin.ShowDialog();
                if (dlgResult == true)
                {
                    // Do something...
                    //
                    // Save the scale bar parameters/settings
                    ActiveDocument.Scalebar = _ScaleBarVm.Scalebar;
                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        protected bool CanExecuteOpenScaleBarWinCommand(object parameter)
        {
            return true;
        }
        #endregion

        #region MergeWithMarkerCommand
        private RelayCommand _MergeWithMarkerCommand = null;
        public ICommand MergeWithMarkerCommand
        {
            get
            {
                if (_MergeWithMarkerCommand == null)
                {
                    _MergeWithMarkerCommand = new RelayCommand(this.ExecuteMergeWithMarkerCommand, CanExecuteMergeWithMarkerCommand);
                }

                return _MergeWithMarkerCommand;
            }
        }
        protected void ExecuteMergeWithMarkerCommand(object parameter)
        {
            if (ActiveDocument != null &&
                !ActiveDocument.IsRgbImage && ActiveDocument.ImageInfo.IsChemiImage)
            {
                try
                {
                    var guid = ActiveDocument.ImageInfo.GroupID;
                    // Find marker image
                    FileViewModel markerFile = null;
                    foreach (var file in Files)
                    {
                        if (file != null)
                        {
                            if (file.IsRgbImage)
                            {
                                if (file.ImageInfo.GroupID == guid)
                                {
                                    markerFile = file;
                                    break;
                                }
                            }
                        }
                    }

                    if (markerFile == null)
                    {
                        string caption = "Marker not found.";
                        string message = "Did not find the associated marker image.\nPlease make sure the marker image is opened.";
                        Xceed.Wpf.Toolkit.MessageBox.Show(Owner, message, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // check image sizes
                    int ciWidth = ActiveDocument.Image.PixelWidth;
                    int ciHeight = ActiveDocument.Image.PixelHeight;
                    int miWidth = markerFile.Image.PixelWidth;
                    int miHeight = markerFile.Image.PixelHeight;

                    if (ciWidth != miWidth && ciHeight != miHeight)
                    {
                        string caption = "Different image size";
                        string message = "The chemi image and the marker image are of different size.";
                        Xceed.Wpf.Toolkit.MessageBox.Show(Owner, message, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Merge chemi with marker
                    if (markerFile != null)
                    {
                        WriteableBitmap chemiWithMarkerImage = null;
                        var markerFrames = ImageProcessing.GetChannel(markerFile.Image);
                        if (markerFrames != null)
                        {
                            #region === chemi + marker (auto-merge into 4-channel image) ===

                            WriteableBitmap[] chemiWithMarkerFrames = new WriteableBitmap[4];
                            for (int i = 0; i < markerFrames.Length; i++)
                            {
                                chemiWithMarkerFrames[i] = markerFrames[i];
                            }
                            chemiWithMarkerFrames[3] = ActiveDocument.Image;
                            chemiWithMarkerImage = ImageProcessing.SetChannel(chemiWithMarkerFrames);
                            if (chemiWithMarkerImage != null)
                            {
                                if (chemiWithMarkerImage.CanFreeze)
                                    chemiWithMarkerImage.Freeze();

                                var imgInfoCloned = (ImageInfo)markerFile.ImageInfo.Clone();
                                imgInfoCloned.GrayChannel = (ImageChannel)ActiveDocument.ImageInfo.MixChannel.Clone();
                                // individually auto-contrast each channel
                                imgInfoCloned.RedChannel.IsAutoChecked = true;
                                imgInfoCloned.GreenChannel.IsAutoChecked = true;
                                imgInfoCloned.BlueChannel.IsAutoChecked = true;
                                imgInfoCloned.GrayChannel.IsAutoChecked = true;
                                imgInfoCloned.GrayChannel.IsInvertChecked = true;
                                imgInfoCloned.MixChannel.IsAutoChecked = false;
                                imgInfoCloned.IsChemiImage = true;
                                imgInfoCloned.IsGrayChannelAvail = true;

                                string docTitle = string.Empty;
                                if (CheckSupportedFileType(ActiveDocument.Title))
                                    docTitle = Path.GetFileNameWithoutExtension(ActiveDocument.Title) + "+Marker";
                                else
                                    docTitle = Path.GetFileName(ActiveDocument.Title) + "+Marker";

                                // Add image to Gallery
                                NewDocument(chemiWithMarkerImage, imgInfoCloned, docTitle, false, false);

                                // Switch to Gallery tab
                                //SelectedApplicationTab = ApplicationTabType.Gallery;
                            }
                            #endregion
                        }
                    }
                    //if (_Owner.ImagingSysSettings.IsQcVersion)
                    //{
                    //    ActivityLog newLog = new ActivityLog(_Owner.LoginUser.UserName);
                    //    newLog.LogFileAction("Chemi merged with marker image", ActiveDocument.FileName);
                    //    _Owner.ManageUsersVM.LogActivity(newLog);
                    //}
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Error merging chemi image with marker image.\n{0}", ex.Message));
                }
            }
        }
        protected bool CanExecuteMergeWithMarkerCommand(object parameter)
        {
            return (ActiveDocument != null);
        }
        #endregion

        /*#region ShowCropAdornerCommand
        /// <summary>
        /// Get the show crop adorner command.
        /// </summary>
        public ICommand ShowCropAdornerCommand
        {
            get
            {
                if (_ShowCropAdornerCommand == null)
                {
                    _ShowCropAdornerCommand = new RelayCommand(this.ExecuteShowCropAdornerCommand, null);
                }

                return _ShowCropAdornerCommand;
            }
        }
        #region protected void ExecuteShowCropAdornerCommand(object parameter)
        /// <summary>
        ///Show crop adorner command implementation.
        /// </summary>
        /// <param name="parameter"></param>
        protected void ExecuteShowCropAdornerCommand(object parameter)
        {
            if (ActiveDocument != null)
            {
                if (!ActiveDocument.IsSelectionToolChecked)
                {
                    ActiveDocument.IsSelectionToolChecked = true;
                }
                else
                {
                    ActiveDocument.IsSelectionToolChecked = false;
                }
            }
        }
        #endregion

        //#region protected bool CanExecuteZoomInCommand(object parameter)
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="parameter"></param>
        ///// <returns></returns>
        //protected bool CanExecuteZoomInCommand(object parameter)
        //{
        //    return (Workspace.This.ActiveDocument != null);
        //}
        //#endregion

        #endregion*/

        #region ZoomInCommand
        /// <summary>
        /// Get the ZoomIn command.
        /// </summary>
        /*public ICommand ZoomInCommand
        {
            get
            {
                if (_ZoomInCommand == null)
                {
                    _ZoomInCommand = new RelayCommand(this.ExecuteZoomInCommand, null);
                }

                return _ZoomInCommand;
            }
        }
        #region protected void ExecuteZoomInCommand(object parameter)
        /// <summary>
        ///Image zoom-in command implementation.
        /// </summary>
        /// <param name="parameter"></param>
        protected void ExecuteZoomInCommand(object parameter)
        {
            if (ActiveDocument != null)
            {
                //double currentZoom = ActiveDocument.ZoomLevel;
                //currentZoom += _DefaultZoomFactor;
                //if (currentZoom >= 0.95 && currentZoom <= 1.05)
                //{
                //    currentZoom = 1.0;
                //}
                //ActiveDocument.ZoomLevel = currentZoom;
                //ActiveDocument.ImageViewer.ZoomIn();

                ActiveDocument.ZoomIn();
            }
        }
        #endregion

        //#region protected bool CanExecuteZoomInCommand(object parameter)
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="parameter"></param>
        ///// <returns></returns>
        //protected bool CanExecuteZoomInCommand(object parameter)
        //{
        //    return (Workspace.This.ActiveDocument != null);
        //}
        //#endregion
        */
        #endregion

        #region ZoomOutCommand
        /// <summary>
        /// Get the ZoomOut command.
        /// </summary>
        /*public ICommand ZoomOutCommand
        {
            get
            {
                if (_ZoomOutCommand == null)
                {
                    _ZoomOutCommand = new RelayCommand(this.ExecuteZoomOutCommand, null);
                }

                return _ZoomOutCommand;
            }
        }

        #region protected void ExecuteZoomOutCommand(object parameter)
        /// <summary>
        ///Image zoom-in command implementation.
        /// </summary>
        /// <param name="parameter"></param>
        protected void ExecuteZoomOutCommand(object parameter)
        {
            if (ActiveDocument != null)
            {
                //if (Math.Round(ActiveDocument.ZoomLevel, 10) > Math.Round(ActiveDocument.MinimumZoom, 10))
                //{
                //    double currentZoom = ActiveDocument.ZoomLevel;
                //    currentZoom -= _DefaultZoomFactor;
                //    if (currentZoom >= 0.95 && currentZoom <= 1.05)
                //    {
                //        currentZoom = 1.0;
                //    }
                //    if (currentZoom < ActiveDocument.MinimumZoom)
                //    {
                //        currentZoom = ActiveDocument.MinimumZoom;
                //    }
                //    ActiveDocument.ZoomLevel = currentZoom;
                //}

                ActiveDocument.ZoomOut();
            }
        }
        #endregion

        //#region protected bool CanExecuteZoomOutCommand(object parameter)
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="parameter"></param>
        ///// <returns></returns>
        //protected bool CanExecuteZoomOutCommand(object parameter)
        //{
        //    return (Workspace.This.ActiveDocument != null);
        //}
        //#endregion
        */
        #endregion

        #region ZoomFitCommand
        /// <summary>
        /// Get the ZoomFit command.
        /// </summary>
        /*public ICommand ZoomFitCommand
        {
            get
            {
                if (_ZoomFitCommand == null)
                {
                    _ZoomFitCommand = new RelayCommand(this.ExecuteZoomFitCommand, this.CanExecuteZoomFitCommand);
                }

                return _ZoomFitCommand;
            }
        }
        #region protected void ExecuteZoomFitCommand(object parameter)
        /// <summary>
        ///Image zoom-in command implementation.
        /// </summary>
        /// <param name="parameter"></param>
        protected void ExecuteZoomFitCommand(object parameter)
        {
            if (ActiveDocument != null)
            {
                ActiveDocument.ZoomLevel = ActiveDocument.MinimumZoom;
            }
        }
        #endregion

        #region protected bool CanExecuteZoomFitCommand(object parameter)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        protected bool CanExecuteZoomFitCommand(object parameter)
        {
            return (ActiveDocument != null);
        }
        #endregion
        */
        #endregion


        #region ChangeLaserModuleCommand
        private ICommand _ChangeLaserModuleCommand;
        public ICommand ChangeLaserModuleCommand
        {
            get
            {
                if (_ChangeLaserModuleCommand == null)
                {
                    _ChangeLaserModuleCommand = new RelayCommand(this.ExecuteChangeLaserModuleCommand, this.CanExecuteChangeLaserModuleCommand);
                }

                return _ChangeLaserModuleCommand;
            }
        }
        public void ExecuteChangeLaserModuleCommand(object parameter)
        {
            LaserModuleChange laserModuleChangeWin = new LaserModuleChange();
            laserModuleChangeWin.Owner = Application.Current.MainWindow;
            var dlgResult = laserModuleChangeWin.ShowDialog();
            if (dlgResult == true)
            {
                if (!Workspace.This.EthernetController.IsConnected ||
                    SettingsManager.ConfigSettings.IsSimulationMode)
                {
                    return;
                }

                Workspace.This.StartWaitAnimation("Homing the scan head. Please wait...");

                if (!Workspace.This.MotorVM.HomeXYZmotor())
                {
                    Workspace.This.StopWaitAnimation();
                    string caption = "Sapphire FL Biomolecular Imager";
                    string message = "Error homing the scan head motors.\n" +
                                    "Please restart the application to home the scan head.";
                    Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                    return;
                }
                bool bIsXYZmotorHoming = true;
                bool bIsScanheadHomed = false;
                while (bIsXYZmotorHoming)
                {
                    System.Threading.Thread.Sleep(500);

                    if (Workspace.This.MotorVM.MotionController.CrntState[MotorTypes.X].AtHome &&
                        Workspace.This.MotorVM.MotionController.CrntState[MotorTypes.Y].AtHome &&
                        Workspace.This.MotorVM.MotionController.CrntState[MotorTypes.Z].AtHome)
                    {
                        bIsScanheadHomed = true;
                        Workspace.This.StopWaitAnimation();

                        bIsXYZmotorHoming = false;
                        Workspace.This.LogMessage("Homing X/Y/Z motors: Succeeded!");
                    }
                    else
                    {
                        bIsXYZmotorHoming = true;
                    }
                }

                if (bIsScanheadHomed)
                {
                    SettingsManager.ApplicationSettings.IsChangeLaserModuleInitiated = true;

                    //string caption = "Sapphire FL Biomolecular Imager";
                    //string message = "The scan head is successfully homed.\n" +
                    //                 "Do you want to close the application now";
                    //Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Information);
                    LaserModuleHomed laserModuleHomed = new LaserModuleHomed();
                    laserModuleHomed.Owner = Application.Current.MainWindow;
                    dlgResult = laserModuleHomed.ShowDialog();
                    if (dlgResult == true)
                    {
                        //Close selected...do somethinbg
                        //

                        // Close all opened files
                        if (CloseAll() == false)
                        {
                            return;
                        }

                        // Not graceful exit (more like ending the process
                        //System.Environment.Exit(0);

                        //If you call Application.Current.Shutdown() to close the application;
                        //your function will not return immediately. You need to call return; as well for this
                        System.Windows.Application.Current.Shutdown();
                        return;
                    }
                }
            }

        }
        protected bool CanExecuteChangeLaserModuleCommand(object parameter)
        {
            return true;
        }
        #endregion

        public bool IsChemiModule
        {
            get
            {
                return SettingsManager.ConfigSettings.IsChemiModule;
            }
            set
            {
                SettingsManager.ConfigSettings.IsChemiModule = value;
                RaisePropertyChanged("IsChemiModule");
            }
        }

        #region LaunchChemiModuleCommand
        private RelayCommand _LaunchChemiModuleCommand = null;
        /// <summary>
        /// Get the inversion command.
        /// </summary>
        public ICommand LaunchChemiModuleCommand
        {
            get
            {
                if (_LaunchChemiModuleCommand == null)
                {
                    _LaunchChemiModuleCommand = new RelayCommand(ExecuteLaunchChemiModuleCommand, CanExecuteLaunchChemiModuleCommand);
                }

                return _LaunchChemiModuleCommand;
            }
        }
        #region protected void ExecuteLaunchChemiModuleCommand(object parameter)
        /// <summary>
        /// Launch Chemi Module (connect and open the Chemi Module home page)
        /// </summary>
        /// <param name="parameter"></param>
        protected void ExecuteLaunchChemiModuleCommand(object parameter)
        {
            if (SettingsManager.ApplicationSettings.CMConnectionType == Common.CMConnectionType.LAN)
            {
                try
                {
                    StartWaitAnimation("Launching Chemi Module...");
                    string url = SettingsManager.ConfigSettings.ChemiModuleLANUrl;
                    Uri myUri = new Uri(url);
                    var ip = Dns.GetHostAddresses(myUri.Host)[0];
                    var ping = new Ping();
                    PingReply res = ping.Send(ip);
                    if (res.Status == IPStatus.Success)
                    {
                        // LAN connection
                        //string url = SettingsManager.ConfigSettings.ChemiModuleLANUrl;
                        Workspace.This.LogMessage(string.Format("Opening ULR: {0}", url));
                        System.Diagnostics.Process.Start(@url);
                    }
                    else
                    {
                        StopWaitAnimation();
                        string caption = "Chemi Module connection...";
                        string message = "Unable to connect to the Chemi Module.\nPlease make sure the Chemi Module is connected to your local network.";
                        Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                finally
                {
                    StopWaitAnimation();
                }
            }
            else
            {
                // Connect to the Chemi Module via Wi-Fi
                CMWiFiConnect();
            }
        }
        #endregion

        #region protected bool CanExecuteLaunchChemiModuleCommand(object parameter)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        protected bool CanExecuteLaunchChemiModuleCommand(object parameter)
        {
            return true;
        }
        #endregion
    
        private void CMWiFiConnect()
        {
            bool bIsSFLChemiModuleConnected = false;
            // Init wifi object and event handlers
            Wifi wifi = new Wifi();
            wifi.ConnectionStatusChanged += Wifi_ConnectionStatusChanged;

            StartWaitAnimation("Launching Chemi Module...");

            var availAccessPoints = wifi.GetAccessPoints();

            // for each access point from list
            foreach (var ap in availAccessPoints)
            {
                //Console.WriteLine("ap: {0}\r\n", ap.Name);
                //check if SSID is desired
                if (ap.Name.ToLower().StartsWith("sfl"))
                {
                    //verify connection to desired SSID
                    //Console.WriteLine("connected: {0}, password needed: {1}, has profile: {2}\r\n", ap.Name, ap.IsConnected, ap.HasProfile);
                    if (!ap.IsConnected)
                    {
                        //disconnect the current connection
                        if (wifi.ConnectionStatus == WifiStatus.Connected)
                        {
                            Workspace.This.LogMessage("Disconnected Wifi connection...");
                            wifi.Disconnect();
                        }
                        //connect if not connected
                        //Console.WriteLine("\r\n{0}\r\n", ap.ToString());
                        //Console.WriteLine("Trying to connect..\r\n");
                        Workspace.This.LogMessage(string.Format("Trying to connect to...{0}", ap.Name));
                        AuthRequest authRequest = new AuthRequest(ap);
                        ap.Connect(authRequest);
                        //assume requested connection is successully connected
                        bIsSFLChemiModuleConnected = true;
                        break;
                    }
                    else
                    {
                        bIsSFLChemiModuleConnected = true;
                        Workspace.This.LogMessage("SFL chemi module already connected...");
                        break;
                    }
                }
            }

            ImagingHelper.Delay(1000);
            if (wifi.ConnectionStatus == WifiStatus.Connected && bIsSFLChemiModuleConnected)
            {
                string url = SettingsManager.ConfigSettings.ChemiModuleWiFiUrl;
                Workspace.This.LogMessage(string.Format("Opening ULR: {0}", url));
                System.Diagnostics.Process.Start(@url);
                StopWaitAnimation();
            }
            else
            {
                StopWaitAnimation();
                string caption = "Chemi Module connection...";
                string message = "Please make sure the WiFi is connected to the Chemi Module";
                Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void Wifi_ConnectionStatusChanged(object sender, WifiStatusEventArgs e)
        {
            Workspace.This.LogMessage(string.Format("Chemi module wifi connection status: {0}", e.NewStatus));
        }
        #endregion

        public bool Close(FileViewModel fileToClose)
        {
            if (fileToClose.IsDirty)
            {
                //var productName = _Owner.ProductName;
                var productName = string.Empty;
                var res = Xceed.Wpf.Toolkit.MessageBox.Show(string.Format("Do you want to save changes to '{0}'?", fileToClose.Title), productName, MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                if (res == MessageBoxResult.Cancel)
                    return false;
                if (res == MessageBoxResult.Yes)
                {
                    try
                    {
                        if (!SaveSync(fileToClose, closeAfterSaved: true, showOsKb: false))
                        {
                            return false;
                        }
                    }
                    catch
                    {
                        // Rethrow to preserve stack details
                        // Satisfies the rule. 
                        throw;
                    }
                }
                else
                {
                    Remove(fileToClose);
                }
            }
            else
            {
                Remove(fileToClose);
            }
            return true;
        }

        /*internal void SaveAsync(FileViewModel fileToSave, bool saveAsFlag = false, bool closeAfterSaved = false, bool showOsKb = true)
        {
            bool bIsCompressed = false;

            if (fileToSave.FilePath == null || saveAsFlag)
            {
                var dlg = new SaveFileDialog();
                dlg.Filter = "TIF Files(.TIFF)|*.tif|TIF Files(Compressed)|*.tif|JPG Files(.JPG)|*.jpg|BMP Files(.BMP)|*.bmp";
                dlg.Title = "Save an Image File";
                if (fileToSave.FilePath == null)
                {
                    dlg.FileName = GenerateFileName(fileToSave.Title, ".tif");
                }
                else
                {
                    dlg.FileName = fileToSave.Title;
                }

                //if (showOsKb)
                //{
                //    ShowOnscreenKeyboard();
                //}

                if (dlg.ShowDialog().GetValueOrDefault())
                {
                    fileToSave.FilePath = dlg.FileName;
                    bIsCompressed = (dlg.FilterIndex == 2);
                    
                    //if (showOsKb)
                    //{
                    //    HideOnscreenKeyboard();
                    //}
                }
                else
                {
                    //if (showOsKb)
                    //{
                    //    HideOnscreenKeyboard();
                    //}

                    return;
                }
            }

            BackgroundWorker saveFileWorker = new BackgroundWorker();

            // Save the document in a different thread
            saveFileWorker.DoWork += (o, ea) =>
            {
                SaveFile(fileToSave, bIsCompressed);
            };
            saveFileWorker.RunWorkerCompleted += (o, ea) =>
            {
                // Work has completed. You can now interact with the UI
                StopWaitAnimation();

                if (ea.Cancelled)
                {
                    // operation cancelled
                }
                else if (ea.Error != null)
                {
                    // error occurred
                    throw ea.Error;
                }
                else
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)delegate
                    {
                        fileToSave.IsDirty = false;
                        fileToSave.Title = fileToSave.FileName;

                        if (closeAfterSaved)
                        {
                            Remove(fileToSave);
                        }
                    });
                }
            };

            saveFileWorker.RunWorkerAsync();
            StartWaitAnimation("Saving...");
        }*/

        public void SaveAsync(WriteableBitmap imageToBeSave, ImageInfo imageInfo, string fileFullPath, bool bIsOverrideDefaultDpi = false)
        {
            if (imageToBeSave == null)
                return;

            BackgroundWorker saveFileWorker = new BackgroundWorker();

            // Save the document in a different thread
            saveFileWorker.DoWork += (o, ea) =>
            {
                //20210419: Now keeping the old captured date/time
                //imageInfo.DateTime = System.String.Format("{0:G}", DateTime.Now);

                try
                {
                    IsBusySavingFile = true;

                    // Reserve the back buffer for updates.
                    if (!imageToBeSave.IsFrozen)
                        imageToBeSave.Lock();

                    // Don't overwrite existing file - add suffix number
                    if (System.IO.File.Exists(fileFullPath))
                    {
                        string fileName = Path.GetFileName(fileFullPath);
                        string directoryName = Path.GetDirectoryName(fileFullPath);
                        // Get a unique file name in the destination folder
                        var generatedFileName = GetUniqueFilenameInFolder(directoryName, fileName);
                        fileFullPath = Path.Combine(directoryName, generatedFileName);
                    }

                    // Save the image to disk
                    ImageProcessing.Save(fileFullPath, imageToBeSave, imageInfo, ImageProcessing.TIFF_FILE, false, bIsOverrideDefaultDpi);
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    IsBusySavingFile = false;

                    // Release the back buffer and make it available for display.
                    if (!imageToBeSave.IsFrozen)
                        imageToBeSave.Unlock();
                }
            };
            saveFileWorker.RunWorkerCompleted += (o, ea) =>
            {
                if (ea.Cancelled)
                {
                    // operation cancelled
                }
                else if (ea.Error != null)
                {
                    // error occurred
                    throw ea.Error;
                }
                else
                {
                    // Remember initial directory
                    SettingsManager.ApplicationSettings.InitialDirectory = System.IO.Path.GetDirectoryName(fileFullPath);
                }
            };

            saveFileWorker.RunWorkerAsync();
        }

        internal bool SaveSync(FileViewModel fileToSave, bool saveAndOpen = false, bool saveAsFlag = false, bool closeAfterSaved = false, bool showOsKb = false)
        {
            bool bIsCompressed = false;
            bool bIsSaveAsPub = false;
            bool bIsJpegOrBmp = false;
            bool bIsSaveInPlaceAllowed = false;
            string savedFilePath = string.Empty;
            string destFilePath = string.Empty;
            string dateTimeNow = string.Empty;
            string savedDateTime = fileToSave.ImageInfo.DateTime;

            if (string.IsNullOrEmpty(fileToSave.FilePath) || saveAsFlag)
            {
                var dlg = new SaveFileDialog();
                dlg.Filter = "TIFF (*.tif;*.tiff)|*.tif;*.tiff|TIFF (PUB)|*.tif;*.tiff|JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|BMP (*.bmp)|*.bmp";
                dlg.Title = "Save an Image File";
                //dlg.DefaultExt = ".tif";
                //dlg.AddExtension = true;
                dlg.OverwritePrompt = false;
                if (fileToSave.FilePath == null)
                {
                    if (string.IsNullOrEmpty(fileToSave.Title))
                    {
                        dlg.FileName = GenerateFileName(fileToSave.Title, ".tif");
                    }
                    else
                    {
                        string fileExtension = string.Empty;
                        if (CheckSupportedFileType(fileToSave.Title))
                            fileExtension = Path.GetExtension(fileToSave.Title);
                        dlg.DefaultExt = fileExtension;
                        if (string.IsNullOrEmpty(fileExtension))
                        {
                            dlg.FileName = fileToSave.Title + ".tif";
                        }
                        else
                            dlg.FileName = fileToSave.Title;
                    }
                }
                else
                {
                    dlg.FileName = fileToSave.Title;
                }

                if (showOsKb)
                {
                    ShowOnscreenKeyboard();
                }

                if (dlg.ShowDialog().GetValueOrDefault())
                {
                    savedFilePath = fileToSave.FilePath;
                    destFilePath = dlg.FileName;
                    bIsSaveAsPub = (dlg.FilterIndex == 2);
                    bIsJpegOrBmp = (dlg.FilterIndex == 3 || dlg.FilterIndex == 4);

                    if (showOsKb)
                    {
                        HideOnscreenKeyboard();
                    }
                }
                else
                {
                    if (showOsKb)
                    {
                        HideOnscreenKeyboard();
                    }

                    return false;
                }
                //savedDateTime = fileToSave.ImageInfo.DateTime;
                //dateTimeNow = System.String.Format("{0:G}", DateTime.Now);
            }
            else
            {
                if (!string.IsNullOrEmpty(fileToSave.FilePath))
                {
                    bIsSaveInPlaceAllowed = true;
                    destFilePath = fileToSave.FilePath;
                }
            }

            try
            {
                IsBusySavingFile = true;

                StartWaitAnimation("Saving...");

                // 20210419: now keeping the original captured date/time
                //fileToSave.ImageInfo.DateTime = dateTimeNow;
                fileToSave.FilePath = destFilePath;
                if (!SaveFile(fileToSave, bIsCompressed, bIsSaveAsPub, bIsSaveInPlaceAllowed))
                {
                    fileToSave.FilePath = savedFilePath;
                    //fileToSave.ImageInfo.DateTime = savedDateTime;
                    return false;
                }
                else
                {
                    if ((string.IsNullOrEmpty(savedFilePath) && !bIsSaveAsPub && !bIsJpegOrBmp) || bIsSaveInPlaceAllowed)
                    {
                        // Get modified date/time
                        DateTime modifiedDate = File.GetLastWriteTime(@fileToSave.FilePath);
                        fileToSave.ModifiedDate = System.String.Format("{0:G}", modifiedDate.ToString());
                    }
                }

                if ((fileToSave.Image.Format.BitsPerPixel % 16) == 0 && (bIsSaveAsPub || bIsJpegOrBmp))
                {
                    // Don't update the image tab title

                    // Restore file path
                    fileToSave.FilePath = savedFilePath;
                }
                else
                {
                    // 2020.04.01: Don't reset the flag, we're now opening the saved file
                    //
                    if ((saveAsFlag && savedFilePath == fileToSave.FileName) || string.IsNullOrEmpty(savedFilePath))
                    {
                        // Update image title and reset the dirty flag
                        fileToSave.IsDirty = false;
                        fileToSave.Title = fileToSave.FileName;
                    }
                    else
                    {
                        if (saveAsFlag && saveAndOpen)
                        {
                            var filePath = fileToSave.FilePath;
                            // Restore current file path
                            fileToSave.FilePath = savedFilePath;
                            // Open the newly saved file
                            var fileViewModel = Open(filePath);
                            if (fileViewModel != null)
                            {
                                ActiveDocument = fileViewModel;
                            }
                        }
                    }
                }

                if (closeAfterSaved)
                {
                    Remove(fileToSave);
                }
            }
            catch(Exception ex)
            {
                StopWaitAnimation();

                // 20210419: now keeping the original captured date/time
                //if (saveAsFlag)
                //{
                //    fileToSave.ImageInfo.DateTime = savedDateTime;
                //}

                fileToSave.FilePath = savedFilePath;
                // Rethrow to preserve stack details
                // Satisfies the rule. 
                //throw;
                string caption = "File Saving Error...";
                string message = "File saving error: \n" + ex.Message + "\n\nStack Trace:\n" + ex.StackTrace;
                var parent = Application.Current.MainWindow;
                Xceed.Wpf.Toolkit.MessageBox.Show(parent, message, caption, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            finally
            {
                IsBusySavingFile = false;
                StopWaitAnimation();
            }

            return true;
        }

        internal bool SaveFile(FileViewModel fileToSave, bool bIsCompressed, bool bIsSaveAsPub = false, bool bIsSaveInPlaceAllowed = false)
        {
            if (string.IsNullOrEmpty(fileToSave.FilePath)) { return false; }

            string fileExtension = ".tif";
            int iFileType = ImageProcessing.TIFF_FILE;

            if (CheckSupportedFileType(fileToSave.FilePath))
                fileExtension = Path.GetExtension(fileToSave.FilePath);

            if (fileExtension.ToLower() == ".tif" ||
                fileExtension.ToLower() == ".tiff")
            {
                iFileType = ImageProcessing.TIFF_FILE;
            }
            else if (fileExtension.ToLower() == ".jpg" ||
                     fileExtension.ToLower() == ".jpeg")
            {
                iFileType = ImageProcessing.JPG_FILE;
            }
            else if (fileExtension.ToLower() == ".bmp")
            {
                iFileType = ImageProcessing.BMP_FILE;
            }
            else
            {
                throw new NotSupportedException("File type not supported.");
            }

            try
            {
                ImageInfo imageInfo = fileToSave.ImageInfo.Clone() as ImageInfo;
                if (imageInfo == null)
                {
                    imageInfo = new ImageInfo();
                }

                WriteableBitmap imageToBeSaved = null;
                string filePath = fileToSave.FilePath;
                if (iFileType == ImageProcessing.BMP_FILE || iFileType == ImageProcessing.JPG_FILE || bIsSaveAsPub)
                {
                    // Save the display image
                    // Cloning to avoid access error.
                    imageToBeSaved = fileToSave.DisplayImage.Clone();

                    if ((fileToSave.Image.Format.BitsPerPixel % 16) == 0 || bIsSaveAsPub)
                    {
                        // Reset the contrast value to 8-bit per channel compatible
                        imageInfo.RedChannel.BlackValue = 0;
                        imageInfo.RedChannel.WhiteValue = 255;
                        imageInfo.GreenChannel.BlackValue = 0;
                        imageInfo.GreenChannel.WhiteValue = 255;
                        imageInfo.BlueChannel.BlackValue = 0;
                        imageInfo.BlueChannel.WhiteValue = 255;
                        imageInfo.MixChannel.BlackValue = 0;
                        imageInfo.MixChannel.WhiteValue = 255;
                        imageInfo.IsChemiImage = false;
                        imageInfo.MixChannel.IsInvertChecked = false;
                        imageInfo.SelectedChannel = ImageChannelType.Mix;
                        imageInfo.IsGrayChannelAvail = false;

                        if (fileToSave.Is4ChannelImage)
                        {
                            imageInfo.RedChannel.FilterPosition = 0;
                            imageInfo.GreenChannel.FilterPosition = 0;
                            imageInfo.BlueChannel.FilterPosition = 0;
                            imageInfo.GrayChannel.FilterPosition = 0;
                            imageInfo.MixChannel.FilterPosition = 0;
                            imageInfo.RedChannel.LightSource = 0;
                            imageInfo.GreenChannel.LightSource = 0;
                            imageInfo.BlueChannel.LightSource = 0;
                            imageInfo.GrayChannel.LightSource = 0;
                            imageInfo.MixChannel.LightSource = 0;
                        }

                        filePath = fileToSave.FilePath;
                        string directoryName = Path.GetDirectoryName(filePath);
                        string fileName = string.Empty;
                        string pubFileName = string.Empty;

                        if (CheckSupportedFileType(filePath))
                            fileName = Path.GetFileNameWithoutExtension(filePath);
                        else
                            fileName = Path.GetFileName(filePath);

                        // Save the display image with the same image DPI as the original source image.
                        var dpi = fileToSave.Image.DpiX;
                        if (Math.Round(dpi, 3) != Math.Round(imageToBeSaved.DpiX, 3))
                        {
                            int width = imageToBeSaved.PixelWidth;
                            int height = imageToBeSaved.PixelHeight;
                            int stride = imageToBeSaved.BackBufferStride;
                            int bufferSize = height * stride;
                            var modifiedImage = BitmapSource.Create(width, height,
                                                                    dpi, dpi,
                                                                    imageToBeSaved.Format, imageToBeSaved.Palette,
                                                                    imageToBeSaved.BackBuffer, bufferSize, stride);
                            imageToBeSaved = new WriteableBitmap(modifiedImage);
                        }
                        pubFileName = fileName + "_PUB_" + Math.Round(dpi, 2) + fileExtension;
                        filePath = Path.Combine(directoryName, pubFileName);

                        // Save .JPEG and .BMP image file with annotations
                        if (fileToSave.IsShowScalebar &&
                            fileToSave.DrawingCanvas != null && fileToSave.DrawingCanvas.GraphicsList.Count > 0)
                        {
                            WriteableBitmap wbm = imageToBeSaved;
                            DrawAnnotations(fileToSave, ref wbm);
                            imageToBeSaved = wbm;
                            // Don't set scale bar to be loaded on image open (the scale bar is already written on the image).
                            imageInfo.IsShowScalebar = false;
                            imageInfo.DynamicBit = 0;
                            if (imageInfo.DrawingGraphics != null)
                            {
                                //imageInfo.DrawingGraphics = null;
                                imageInfo.DrawingGraphics = new PropertiesGraphicsBase[0];
                            }
                        }
                    }
                }
                else
                {
                    if (bIsSaveInPlaceAllowed)
                    {
                        imageToBeSaved = fileToSave.Image;  // Save the source image
                    }
                    else
                    {
                        // Not save in-place, make a copy to avoid threading access exception
                        imageToBeSaved = fileToSave.Image.Clone();
                    }
                }

                if (File.Exists(filePath) && !bIsSaveInPlaceAllowed)
                {
                    StopWaitAnimation();

                    string caption = "Save an Image File";
                    string message = string.Format("{0} already exists.\nDo you want to replace it?", filePath);
                    var dlgResult = Xceed.Wpf.Toolkit.MessageBox.Show(Owner, message, caption, System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning);
                    if (dlgResult == MessageBoxResult.Yes)
                        StartWaitAnimation("Saving...");
                    else
                        return false;
                }

                DateTime modifiedDate = File.GetLastWriteTime(@filePath);
                imageInfo.ModifiedDate = System.String.Format("{0:G} UTC{0:zz}", modifiedDate);

                // Save the image file
                ImageProcessing.Save(filePath, imageToBeSaved, imageInfo, iFileType, bIsCompressed, fileToSave.IsOverrideDefaultDpi);

                // Save the annotation
                //if (fileToSave.DrawingCanvas.IsDirty)
                //{
                //    try
                //    {
                //        var filePath = Path.ChangeExtension(fileToSave.FilePath, ".aan");
                //        fileToSave.DrawingCanvas.Save(filePath);
                //        fileToSave.IsDirty = false;
                //    }
                //    catch (Exception ex)
                //    {
                //        string caption = "Error loading annotations";
                //        string message = string.Format("Error loading annotations:\n{0}", ex.Message);
                //        Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                //    }
                //}

                // Remember initial directory
                SettingsManager.ApplicationSettings.InitialDirectory = System.IO.Path.GetDirectoryName(fileToSave.FilePath);

                return true;
            }
            catch (Exception ex)
            {
                StopWaitAnimation();
                string caption = "File Saving Error...";
                string message = "File saving error: \n" + ex.Message + "\n\nStack Trace:\n" + ex.StackTrace;
                var parent = Application.Current.MainWindow;
                Xceed.Wpf.Toolkit.MessageBox.Show(parent, message, caption, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        internal void SaveFileAsPub(FileViewModel fileToSave)
        {
            var imgInfo = (ImageInfo)fileToSave.ImageInfo.Clone();
            var imageToSave = fileToSave.DisplayImage.Clone();

            // Save .JPEG and .BMP image file with annotations
            if (fileToSave.IsShowScalebar &&
                fileToSave.DrawingCanvas != null && fileToSave.DrawingCanvas.GraphicsList.Count > 0)
            {
                WriteableBitmap wbm = imageToSave;
                DrawAnnotations(fileToSave, ref wbm);
                imageToSave = wbm;
                // Don't set scale bar to be loaded on image open (the scale bar is already written on the image).
                imgInfo.IsShowScalebar = false;
                imgInfo.DynamicBit = 0;
                // Clear (don't save) drawing graphics list in image info
                if (fileToSave.DrawingCanvas != null && fileToSave.DrawingCanvas.GraphicsList.Count > 0)
                {
                    if (imgInfo.DrawingGraphics != null)
                    {
                        //imgInfo.DrawingGraphics = null;
                        imgInfo.DrawingGraphics = new PropertiesGraphicsBase[0];
                    }
                }
            }

            if (imageToSave.CanFreeze)
                imageToSave.Freeze();

            // Reset the contrast value to 8-bit per channel compatible
            imgInfo.RedChannel.BlackValue = 0;
            imgInfo.RedChannel.WhiteValue = 255;
            imgInfo.GreenChannel.BlackValue = 0;
            imgInfo.GreenChannel.WhiteValue = 255;
            imgInfo.BlueChannel.BlackValue = 0;
            imgInfo.BlueChannel.WhiteValue = 255;
            imgInfo.MixChannel.BlackValue = 0;
            imgInfo.MixChannel.WhiteValue = 255;
            //imgInfo.IsChemiImage = false;
            imgInfo.MixChannel.IsInvertChecked = false;
            imgInfo.SelectedChannel = ImageChannelType.Mix;
            imgInfo.IsGrayChannelAvail = false;

            if (fileToSave.Is4ChannelImage)
            {
                imgInfo.RedChannel.FilterPosition = 0;
                imgInfo.GreenChannel.FilterPosition = 0;
                imgInfo.BlueChannel.FilterPosition = 0;
                imgInfo.GrayChannel.FilterPosition = 0;
                imgInfo.MixChannel.FilterPosition = 0;
                imgInfo.RedChannel.LightSource = 0;
                imgInfo.GreenChannel.LightSource = 0;
                imgInfo.BlueChannel.LightSource = 0;
                imgInfo.GrayChannel.LightSource = 0;
                imgInfo.MixChannel.LightSource = 0;
                //imgInfo.RedChannel.DyeName = string.Empty;
                //imgInfo.GreenChannel.DyeName = string.Empty;
                //imgInfo.BlueChannel.DyeName = string.Empty;
                //imgInfo.GrayChannel.DyeName = string.Empty;
                //imgInfo.MixChannel.DyeName = string.Empty;
            }

            string filePath = fileToSave.FilePath;
            string dirPath = Path.GetDirectoryName(filePath);
            string fileName = string.Empty;

            if (CheckSupportedFileType(filePath))
                fileName = Path.GetFileNameWithoutExtension(filePath);
            else
                fileName = Path.GetFileName(filePath);

            BackgroundWorker saveFileWorker = new BackgroundWorker();

            // Save the document in a different thread
            saveFileWorker.DoWork += (o, ea) =>
            {
                if (SettingsManager.ApplicationSettings.IsAutoSavePubFileJpeg)
                {
                    string fileNameWithExt = fileName + ".jpg";
                    filePath = Path.Combine(dirPath, fileNameWithExt);
                    if (System.IO.File.Exists(filePath))
                    {
                        // Get a unique file name in the destination folder
                        var generatedFileName = GetUniqueFilenameInFolder(dirPath, fileNameWithExt);
                        filePath = Path.Combine(dirPath, generatedFileName);
                    }
                    ImageProcessing.Save(filePath, imageToSave, imgInfo, ImageProcessing.JPG_FILE, false, false);
                }
                if (SettingsManager.ApplicationSettings.IsAutoSavePubFile300dpi)
                {
                    int dpiX = 300;
                    int dpiY = 300;
                    WriteableBitmap imageToSave300 = null;
                    bool bIsOverrideDefaultDPI = false;
                    string fileNameWithExt = fileName + "_PUB_300.tif";
                    filePath = Path.Combine(dirPath, fileNameWithExt);
                    if (System.IO.File.Exists(filePath))
                    {
                        // Get a unique file name in the destination folder
                        var generatedFileName = GetUniqueFilenameInFolder(dirPath, fileNameWithExt);
                        filePath = Path.Combine(dirPath, generatedFileName);
                    }
                    if (imageToSave.DpiX != dpiX && imageToSave.DpiY != dpiY)
                    {
                        bIsOverrideDefaultDPI = true;
                        imageToSave300 = ImageProcessing.SetBitmapDpi(imageToSave, dpiX, dpiY);
                        if (imageToSave300.CanFreeze)
                            imageToSave300.Freeze();
                        // Save the image to disk (300 DPI)
                        ImageProcessing.Save(filePath, imageToSave300, imgInfo, ImageProcessing.TIFF_FILE, false, bIsOverrideDefaultDPI);
                    }
                    else
                    {
                        ImageProcessing.Save(filePath, imageToSave, imgInfo, ImageProcessing.TIFF_FILE, false, bIsOverrideDefaultDPI);
                    }
                }
                if (SettingsManager.ApplicationSettings.IsAutoSavePubFile600dpi)
                {
                    int dpiX = 600;
                    int dpiY = 600;
                    WriteableBitmap imageToSave600 = null;
                    bool bIsOverrideDefaultDPI = false;
                    string fileNameWithExt = fileName + "_PUB_600.tif";
                    filePath = Path.Combine(dirPath, fileNameWithExt);
                    if (System.IO.File.Exists(filePath))
                    {
                        // Get a unique file name in the destination folder
                        var generatedFileName = GetUniqueFilenameInFolder(dirPath, fileNameWithExt);
                        filePath = Path.Combine(dirPath, generatedFileName);
                    }
                    if (imageToSave.DpiX != dpiX && imageToSave.DpiY != dpiY)
                    {
                        bIsOverrideDefaultDPI = true;
                        imageToSave600 = ImageProcessing.SetBitmapDpi(imageToSave, dpiX, dpiY);
                        if (imageToSave600.CanFreeze)
                            imageToSave600.Freeze();
                        // Save the image to disk (600 DPI)
                        ImageProcessing.Save(filePath, imageToSave600, imgInfo, ImageProcessing.TIFF_FILE, false, bIsOverrideDefaultDPI);
                    }
                    else
                    {
                        ImageProcessing.Save(filePath, imageToSave, imgInfo, ImageProcessing.TIFF_FILE, false, bIsOverrideDefaultDPI);
                    }
                }
            };
            saveFileWorker.RunWorkerCompleted += (o, ea) =>
            {
                if (ea.Cancelled)
                {
                    // operation cancelled
                }
                else if (ea.Error != null)
                {
                    // error occurred
                    throw ea.Error;
                }
                else
                {
                    // Remember initial directory
                    SettingsManager.ApplicationSettings.InitialDirectory = dirPath;
                }
            };

            saveFileWorker.RunWorkerAsync();
        }

        /// <summary>
        /// Draw annotations on the image (when saved as .JPG or .BMP).
        /// </summary>
        /// <param name="fileToSave"></param>
        /// <param name="wbmBitmap"></param>
        internal void DrawAnnotations(FileViewModel fileToSave, ref WriteableBitmap wbmBitmap)
        {
            // Create DrawingVisual and get its drawing context
            DrawingVisual vs = new DrawingVisual();
            DrawingContext dc = vs.RenderOpen();

            System.Windows.Controls.Image image = new System.Windows.Controls.Image();
            image.Source = wbmBitmap;

            int width = wbmBitmap.PixelWidth;
            int height = wbmBitmap.PixelHeight;
            double dpiX = wbmBitmap.DpiX;
            double dpiY = wbmBitmap.DpiY;
            System.Windows.Rect rect = new System.Windows.Rect(0, 0, image.Source.Width, image.Source.Height);

            // Draw image
            dc.DrawImage(image.Source, rect);

            double scale = image.Source.Width / fileToSave.DrawingCanvas.ActualWidth;

            // Keep old existing actual scale and set new actual scale.
            double oldActualScale = ActiveDocument.DrawingCanvas.ActualScale;
            ActiveDocument.DrawingCanvas.ActualScale = scale;

            // Remove clip in the canvas - we set our own clip.
            ActiveDocument.DrawingCanvas.RemoveClip();

            // Prepare drawing context to draw graphics
            dc.PushClip(new RectangleGeometry(rect));
            dc.PushTransform(new TranslateTransform(0, 0));
            dc.PushTransform(new ScaleTransform(scale, scale));

            // Ask canvas to draw overlays
            ActiveDocument.DrawingCanvas.Draw(dc);

            // Restore old actual scale.
            ActiveDocument.DrawingCanvas.ActualScale = oldActualScale;

            // Restore clip
            ActiveDocument.DrawingCanvas.RefreshClip();

            dc.Pop();
            dc.Pop();
            dc.Pop();

            dc.Close();

            var mergedImage = new RenderTargetBitmap(width, height, dpiX, dpiY, PixelFormats.Default);
            mergedImage.Render(vs);
            BitmapSource renderedBitmap = mergedImage;
            wbmBitmap = new WriteableBitmap(renderedBitmap);
        }

        public BitmapImage ConvertWriteableBitmapToBitmapImage(WriteableBitmap wbm)
        {
            BitmapImage bmImage = new BitmapImage();
            using (MemoryStream stream = new MemoryStream())
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(wbm));
                encoder.Save(stream);
                bmImage.BeginInit();
                bmImage.CacheOption = BitmapCacheOption.OnLoad;
                bmImage.StreamSource = stream;
                bmImage.EndInit();
                bmImage.Freeze();
            }
            return bmImage;
        }

        internal bool DocumentExists(string strTitle)
        {
            bool bResult = false;

            if (Files.Count > 0)
            {
                foreach (var file in Files)
                {
                    //file.Title.Trim( new Char[] {'*'} );
                    if (file.Title.Equals(strTitle))
                    {
                        bResult = true;
                        break;
                    }
                }
            }

            return bResult;
        }

        /// <summary>
        /// Generate default file name using timestamp (default file type is: .TIFF)
        /// </summary>
        /// <param name="headerTitle"></param>
        /// <param name="fileType"></param>
        /// <returns></returns>
        internal string GenerateFileName(string headerTitle, string fileType = ".tif")
        {
            string strFileName = string.Empty;
            string strFileFullPath = string.Empty;
            int[] intArray = null;
            bool bIsFramePartOfSet = false;
            bool bIsScannedSet = false;
            string filePrefix = string.Empty;

            //
            // Get set and frame number
            //
            if (Regex.IsMatch(headerTitle, @"-F\d"))
            {
                bIsFramePartOfSet = true;

                // Split on one or more non-digit characters.
                string[] numbers = Regex.Split(headerTitle, @"\D+");
                intArray = new int[numbers.Count()];
                int setIndex = 0;
                for (int index = 0; index < numbers.Count(); index++)
                {
                    if (!string.IsNullOrEmpty(numbers[index]))
                    {
                        int number = int.Parse(numbers[index]);
                        if (number > 0)
                        {
                            intArray[setIndex++] = number;
                        }
                    }
                }
            }
            else if (Regex.IsMatch(headerTitle, @"-S\d"))
            {
                bIsScannedSet = true;
            }

            //
            // Generate default image name using timestamp
            //
            DateTime dt = DateTime.Now;

            if (bIsFramePartOfSet || bIsScannedSet)
            {
                if (bIsFramePartOfSet)
                {
                    strFileName = string.Format("{0}.{1:D2}.{2:D2}_{3:D2}.{4:D2}.{5:D2}", dt.ToString("yy"), dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                    strFileName = string.Format("{0}_S{1}_F{2}", strFileName, intArray[0], intArray[1]);
                }
                else if (bIsScannedSet)
                {
                    string timeStamp = string.Format("{0:D2}{1:D2}-{2:D2}{3:D2}{4:D2}", dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                    strFileName = string.Format("{0}_{1}", headerTitle, timeStamp);
                }
            }
            else
            {
                strFileName = string.Format("{0}.{1:D2}.{2:D2}_{3:D2}.{4:D2}.{5:D2}", dt.ToString("yy"), dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
            }

            switch (fileType.ToLower())
            {
                case ".tif":
                    strFileName = strFileName + ".tif";
                    break;
                case ".jpg":
                    strFileName = strFileName + ".jpg";
                    break;
                case ".bmp":
                    strFileName = strFileName + ".bmp";
                    break;
            }

            return strFileName;
        }

        internal string GetUniqueFilename(string fileName)
        {
            int count = 2;

            string tempFileName = string.Format("{0} ({1})", fileName, count);
            string fileNameWithoutExt = string.Empty;
            for (int i = 0; i < Workspace.This.Files.Count; i++)
            {
                if (CheckSupportedFileType(Files[i].Title))
                    fileNameWithoutExt = Path.GetFileNameWithoutExtension(Files[i].Title);
                else
                    fileNameWithoutExt = Files[i].Title;

                if (tempFileName.Equals(fileNameWithoutExt, StringComparison.InvariantCultureIgnoreCase))
                {
                    tempFileName = string.Format("{0} ({1})", fileName, count++);
                    i = -1; // Reset i to 0; setting it to -1 here because the for loop will do an increment
                }
            }

            return tempFileName;
        }

        /// <summary>
        /// Get a unique file name (so not to overwrite the existing files)
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        internal string GetUniqueFilenameInFolder(string folderPath, string fileName)
        {
            int count = 1;
            string extension;
            string fileNameWithoutExt;
            if (CheckSupportedFileType(fileName))
            {
                fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                extension = Path.GetExtension(fileName);
            }
            else
            {
                fileNameWithoutExt = fileName;
                extension = ".tif";
            }

            string result;
            string pattern = @"\(\d+\)";    //pattern: number(s) inside parenthesis
            Regex rg = new Regex(pattern);
            MatchCollection matched;
            while (true)
            {
                //string pattern = @"\(\d+\)";    //pattern: number(s) inside parenthesis
                //Regex rg = new Regex(pattern);
                //MatchCollection matched = rg.Matches(fileNameWithoutExt);
                matched = rg.Matches(fileNameWithoutExt);
                if (matched.Count > 0)
                {
                    //string[] numbers = System.Text.RegularExpressions.Regex.Split(fileNameWithoutExt, @"\D+");
                    string output = fileNameWithoutExt.Split('(', ')')[1];
                    string toBeReplaced = string.Format("({0})", output);
                    count = int.Parse(output) + 1;
                    string replaceWith = string.Format("({0})", count);
                    result = fileNameWithoutExt.Replace(toBeReplaced, replaceWith);
                }
                else
                {
                    result = string.Format("{0} ({1})", fileNameWithoutExt, count);
                }
                result = result + extension;
                string filePath = Path.Combine(folderPath, result);

                // Make sure we don't have the duplicate file name
                if (System.IO.File.Exists(filePath))
                {
                    count++;
                    continue;
                }
                else
                {
                    break;
                }
            }
            return result;
        }

        internal bool CheckSupportedFileType(string fileName)
        {
            string ext = Path.GetExtension(fileName);
            switch (ext.ToLower())
            {
                case ".gif":
                    return true;
                case ".jpg":
                    return true;
                case ".jpeg":
                    return true;
                case ".png":
                    return true;
                case ".bmp":
                    return true;
                case ".tif":
                    return true;
                //case ".ds":     // AzureSpot file type
                //    return true;
                default:
                    return false;
            }
        }

        internal bool IsGeneratedFileName(string fileName)
        {
            bool bResult = false;

            if (!string.IsNullOrEmpty(fileName))
            {
                string fileNameWithoutExt = string.Empty;
                if (CheckSupportedFileType(fileName))
                {
                    fileNameWithoutExt = System.IO.Path.GetFileNameWithoutExtension(fileName);
                }
                else
                {
                    fileNameWithoutExt = fileName;
                }
                //string[] numbers = System.Text.RegularExpressions.Regex.Split(fileName, @"\D+");
                string pattern = @"\d+\.\d+\.\d+_\d+\.\d+\.\d+";
                System.Text.RegularExpressions.Regex rg = new System.Text.RegularExpressions.Regex(pattern);
                System.Text.RegularExpressions.MatchCollection matched = rg.Matches(fileNameWithoutExt);
                // Found a matching default file name pattern
                if (matched.Count > 0)
                {
                    bResult = true;
                }
            }

            return bResult;
        }

        public WriteableBitmap LoadImage(string filePath)
        {
            WriteableBitmap wbBitmap = null;
            ImageInfo imageInfo = null;

            try
            {
                wbBitmap = ImageProcessing.LoadImageFile(filePath);

                if (wbBitmap != null)
                {
                    try
                    {
                        // Get image info from the comments section of the image metadata
                        imageInfo = ImageProcessing.ReadMetadata(filePath);
                    }
                    catch
                    {
                    }

                    if (imageInfo == null)
                    {
                        imageInfo = new ImageInfo();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return wbBitmap;
        }

        #region ShowOSKBCommand
        /// <summary>
        /// Get Onscreen keyboard command.
        /// </summary>
        public ICommand ShowOSKBCommand
        {
            get
            {
                if (_ShowOSKBCommand == null)
                {
                    _ShowOSKBCommand = new RelayCommand(this.ExecuteShowOSKBCommand, this.CanExecuteShowOSKBCommand);
                }

                return _ShowOSKBCommand;
            }
        }
        #region protected void ExecuteShowOSKBCommand(object parameter)
        /// <summary>
        ///Show onscreen keyboard command implementation.
        /// </summary>
        /// <param name="parameter"></param>
        protected void ExecuteShowOSKBCommand(object parameter)
        {
            if (ActiveDocument != null)
            {
                ShowOnscreenKeyboard();
            }
        }
        #endregion

        #region protected bool CanExecuteShowOSKBCommand(object parameter)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        protected bool CanExecuteShowOSKBCommand(object parameter)
        {
            return (ActiveDocument != null);
        }
        #endregion

        #endregion


        /*private bool _IsEnableTabControl = true;
        public bool IsEnableTabControl
        {
            get { return _IsEnableTabControl; }
            set
            {
                if (_IsEnableTabControl != value)
                {
                    _IsEnableTabControl = value;
                    RaisePropertyChanged("IsEnableTabControl");
                }
            }
        }*/

        private bool _IsSwitchingImagingMode = false;
        public bool IsSwitchingImagingMode
        {
            get { return _IsSwitchingImagingMode; }
            set
            {
                if (_IsSwitchingImagingMode != value)
                {
                    _IsSwitchingImagingMode = value;
                    RaisePropertyChanged("IsSwitchingImagingMode");
                }
            }
        }

        private MultiplexViewModel _MultiplexVm = null;
        public MultiplexViewModel MultiplexVm
        {
            get { return _MultiplexVm; }
            set
            {
                _MultiplexVm = value;
                RaisePropertyChanged("MultiplexVm");
            }
        }

        private bool _IsMultiplexChecked = false;
        public bool IsMultiplexChecked
        {
            get { return _IsMultiplexChecked; }
            set
            {
                if (value == true)
                    if (this.Files == null || this.Files.Count == 0)
                        return;

                _IsMultiplexChecked = value;
                RaisePropertyChanged("IsMultiplexChecked");
                if (_IsMultiplexChecked)
                {
                    //_MultiplexVm.Files = this.Files;
                    // Set default selection.
                    /*for (int index = 0; index < this.Files.Count; index++)
                    {
                        if (index == 0)
                            _MultiplexVm.SelectedImageC1 = this.Files[index];
                        if (index == 1)
                            _MultiplexVm.SelectedImageC2 = this.Files[index];
                        if (index == 2)
                        {
                            _MultiplexVm.SelectedImageC3 = this.Files[index];
                            break;
                        }
                    }*/

                    _MultiplexVm.FilesTitle.Clear();
                    foreach (var file in this.Files)
                    {
                        if (file != null)
                        {
                            MultiplexData mpData = new MultiplexData(file.Title, file.FileID);
                            _MultiplexVm.FilesTitle.Add(mpData);
                        }
                    }
                    MultiplexData tmpData = new MultiplexData("None", "0");
                    _MultiplexVm.FilesTitle.Add(tmpData);
                }
                else
                {
                    _MultiplexVm.ResetSelection();
                }
            }
        }

        public bool IsAdjustmentsChecked
        {
            get { return _IsAdjustmentsChecked; }
            set
            {
                if (_IsAdjustmentsChecked != value)
                {
                    _IsAdjustmentsChecked = value;
                    RaisePropertyChanged("IsAdjustmentsChecked");
                    if (_IsAdjustmentsChecked)
                    {
                        _IsCropChecked = false;
                        RaisePropertyChanged("IsCropChecked");
                        _IsResizeChecked = false;
                        RaisePropertyChanged("IsResizeChecked");
                        _IsRotateChecked = false;
                        RaisePropertyChanged("IsRotateChecked");
                    }
                }
            }
        }

        public bool IsCropChecked
        {
            get { return _IsCropChecked; }
            set
            {
                if (_IsCropChecked != value)
                {
                    _IsCropChecked = value;
                    RaisePropertyChanged("IsCropChecked");
                    if (_IsCropChecked)
                    {
                        _IsAdjustmentsChecked = false;
                        RaisePropertyChanged("IsAdjustmentsChecked");
                        _IsResizeChecked = false;
                        RaisePropertyChanged("IsResizeChecked");
                        _IsRotateChecked = false;
                        RaisePropertyChanged("IsRotateChecked");
                    }
                }
            }
        }

        public bool IsRotateChecked
        {
            get { return _IsRotateChecked; }
            set
            {
                if (_IsRotateChecked != value)
                {
                    _IsRotateChecked = value;
                    RaisePropertyChanged("IsRotateChecked");
                    if (_IsRotateChecked)
                    {
                        _IsAdjustmentsChecked = false;
                        RaisePropertyChanged("IsAdjustmentsChecked");
                        _IsCropChecked = false;
                        RaisePropertyChanged("IsCropChecked");
                        _IsResizeChecked = false;
                        RaisePropertyChanged("IsResizeChecked");
                    }
                }
            }
        }

        public bool IsResizeChecked
        {
            get { return _IsResizeChecked; }
            set
            {
                if (_IsResizeChecked != value)
                {
                    _IsResizeChecked = value;
                    RaisePropertyChanged("IsResizeChecked");
                    if (_IsResizeChecked)
                    {
                        _IsAdjustmentsChecked = false;
                        RaisePropertyChanged("IsAdjustmentsChecked");
                        _IsCropChecked = false;
                        RaisePropertyChanged("IsCropChecked");
                        _IsRotateChecked = false;
                        RaisePropertyChanged("IsRotateChecked");

                        ResizeVM.ActiveDocument = ActiveDocument;
                    }
                    else
                    {
                        ResizeVM.ActiveDocument = null;
                    }
                }
            }
        }

        /// <summary>
        /// Return true if chemi image that's part of chemi marker set.
        /// </summary>
        private Guid blankGuid = new Guid("00000000-0000-0000-0000-000000000000");
        public bool IsChemiMarkerSet
        {
            get
            {
                bool bResult = false;
                if (ActiveDocument != null)
                {
                    if (!ActiveDocument.IsRgbImage && ActiveDocument.ImageInfo.IsChemiImage)
                    {
                        if (ActiveDocument.ImageInfo.GroupID != null &&
                            ActiveDocument.ImageInfo.GroupID != blankGuid)
                        {
                            bResult = true;
                        }
                    }
                }
                return bResult;
            }
        }

        public CopyPasteViewModel ImageCopyPasteVm
        {
            get { return _ImageCopyPasteVm; }
        }

        public TransformViewModel ImageTransformVm
        {
            get { return _ImageTransformVm; }
        }

        public ResizeViewModel ResizeVM
        {
            get { return _ResizeVM; }
        }

        //private bool _IsTabSwitchAllowed = true;
        //public bool IsTabSwitchAllowed
        //{
        //    get { return _IsTabSwitchAllowed; }
        //    set { _IsTabSwitchAllowed = value; }
        //}

        public ApplicationTabType SelectedApplicationTab
        {
            get { return _SelectedApplicationTab; }
            set
            {
                if (_SelectedApplicationTab != value)
                {
                    // Close preview/contrast window
                    if (_SelectedApplicationTab == ApplicationTabType.Imaging)
                    {
                        if (_FluorescenceVM.IsPreviewChannels)
                            _FluorescenceVM.IsPreviewChannels = false;
                        if (_SelectedImagingType == ImagingType.PhosphorImaging)
                            if (_PhosphorVM.IsPreviewChannels)
                                _PhosphorVM.IsPreviewChannels = false;
                    }
                    //else if (_SelectedApplicationTab == ApplicationTabType.Settings)
                    //{
                    //    if (this.IsCapturing)
                    //    {
                    //        string caption = "Settings...";
                    //        string message = "The system is busy.\nPlease stop the current operation before switching tab.";
                    //        System.Windows.MessageBoxResult dlgResult = Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    //        return;
                    //    }
                    //}

                    if (value == ApplicationTabType.Help)
                    {
                        if (!IsAdobeAcrobatInstalled)
                        {
                            // If Adobe Reader is not installed; load the user's manual on the web browser (or default non-Adobe PDF reader)
                            //
                            //string userManualUrl = @"https://azurebiosystems.com/AISmanual";
                            //string userManualUrl21CFR = @"https://azurebiosystems.com/AISmanual/21CFR";
                            //string userManualUrl = @"https://azurebiosystems.com/wp-content/uploads/2023/07/Azure-Imaging-Systems-User-Manual.pdf";
                            //string userManualUrl21CFR = @"https://azurebiosystems.com/wp-content/uploads/2023/07/AIS-21CFR11-Capture-Software-User-Manual_2.pdf";
                            string userManualUrl = "https://azurebiosystems.com/wp-content/uploads/2023/02/UM-0014_R1_Sapphire_FL_User_Manual.pdf";
                            string userManualFileName = "Sapphire_FL_User_Manual.pdf";
                            var userManualLocalPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UserManual", userManualFileName);

                            try
                            {
                                if (File.Exists(userManualLocalPath))
                                {
                                    System.Diagnostics.Process.Start(userManualLocalPath);
                                    //OpenWithDefaultProgram(userManualLocalPath);
                                }
                                else
                                {
                                    string manualUrl = userManualUrl;
                                    //if (_Owner.ImagingSysSettings.IsQcVersion)
                                    //{
                                    //    manualUrl = userManualUrl21CFR;
                                    //}
                                    System.Diagnostics.Process.Start(manualUrl);
                                    //OpenWithDefaultProgram(manualURL);
                                }
                            }
                            catch (System.ComponentModel.Win32Exception noBrowser)
                            {
                                if (noBrowser.ErrorCode == -2147467259)
                                {
                                    Console.WriteLine(noBrowser.Message);
                                }
                            }
                            catch (System.Exception other)
                            {
                                Console.WriteLine(other.Message);
                            }
                        }
                    }
                    else if (value == ApplicationTabType.Settings)
                    {
                        #region === Settings Page ===

                        #region === Settings Page: Password prompt ===

                        if (!_HasSecurePassword &&
                            SettingsVM.CurrentPageViewModel != null &&
                            !SettingsVM.CurrentPageViewModel.Name.Equals("GENERAL", StringComparison.OrdinalIgnoreCase))
                        {
                            // Prompt for password
                            string strSecureXmlPath = System.IO.Path.Combine(Workspace.This.AppDataPath, "Secure.xml");

                            // Get the current password for the 'Settings' page
                            string passwordHash = SecureSettings.GetPassword(strSecureXmlPath);

                            // Don't prompt for a password if current password is blank/empty
                            if (!string.IsNullOrEmpty(passwordHash))
                            {
                                PasswordPrompt passwordPrompt = new PasswordPrompt(strSecureXmlPath);
                                passwordPrompt.ShowDialog();
                                if (passwordPrompt.DialogResult != true)
                                {
                                    return;
                                }
                            }

                            _HasSecurePassword = true;
                        }

                        #endregion

                        //if (!SettingsManager.ConfigSettings.IsSimulationMode)
                        //{
                        //    if (IsScanningMode)
                        //    {
                        //        if (SettingsVM != null && SettingsVM.CurrentPageViewModel != null)
                        //        {
                        //            // Switch to camera mode if the selected Settings page is Chemi/Visible
                        //            if (SettingsVM.IsCreateCalibrationPage)
                        //            {
                        //                // Switch to Camera Mode
                        //                if (!SwitchToCameraMode())
                        //                {
                        //                    return;
                        //                }
                        //                IsCameraMode = true;
                        //            }
                        //        }
                        //    }
                        //}

                        #endregion
                    }
                    else if (value == ApplicationTabType.Imaging)
                    {
                        #region === Imaging Mode ===

                        //if (SelectedImagingType == ImagingType.Fluorescence ||
                        //    SelectedImagingType == ImagingType.PhosphorImaging)
                        //{
                        //    IsScanningMode = true;
                        //}

                        #endregion
                    }

                    _SelectedApplicationTab = value;
                    RaisePropertyChanged("SelectedApplicationTab");
                    RaisePropertyChanged("IsGalleryTab");
                    RaisePropertyChanged("IsImagingTab");
                    RaisePropertyChanged("IsHelpTab");
                    RaisePropertyChanged("IsSettingsTab");
                    //RaisePropertyChanged("IsScanningMode");
                    RaisePropertyChanged("IsFluorescenceImaging");
                    RaisePropertyChanged("IsPhosphorImaging");
                }
            }
        }

        public ImagingType SelectedImagingType
        {
            get { return _SelectedImagingType; }
            set
            {
                if (_SelectedImagingType != value)
                {
                    // Close preview/contrast window
                    if (_SelectedImagingType == ImagingType.Fluorescence)
                        if (_FluorescenceVM.IsPreviewChannels)
                            _FluorescenceVM.IsPreviewChannels = false;
                    if (_SelectedImagingType == ImagingType.PhosphorImaging)
                        if (_PhosphorVM.IsPreviewChannels)
                            _PhosphorVM.IsPreviewChannels = false;

                    var defaultFileName = GenerateFileName(string.Empty, string.Empty);

                    if (value == ImagingType.Fluorescence || value == ImagingType.PhosphorImaging)
                    {
                        //IsScanningMode = true;

                        if (!Workspace.This.IsScanning)
                        {
                            if (value == ImagingType.Fluorescence)
                            {
                                if (_FluorescenceVM.SelectedAppProtocol != null && _FluorescenceVM.SelectedAppProtocol.SelectedScanRegion != null)
                                {
                                    int nScanRegions = _FluorescenceVM.SelectedAppProtocol.ScanRegions.Count;
                                    string fileName = defaultFileName;
                                    for (int i = 0; i < nScanRegions; i++)
                                    {
                                        if (nScanRegions > 1)
                                        {
                                            fileName = string.Format("{0}_SR{1}", defaultFileName, _FluorescenceVM.SelectedAppProtocol.ScanRegions[i].ScanRegionNum);
                                        }
                                        Workspace.This.FluorescenceVM.SelectedAppProtocol.ScanRegions[i].FileLocationVm.FileName = fileName;
                                    }
                                }
                            }
                            else
                            {
                                if (_PhosphorVM.SelectedAppProtocol != null && _PhosphorVM.SelectedAppProtocol.SelectedScanRegion != null)
                                {
                                    //_PhosphorVM.SelectedAppProtocol.SelectedScanRegion.FileLocationVm.FileName = defaultFileName;
                                    int nScanRegions = Workspace.This.FluorescenceVM.SelectedAppProtocol.ScanRegions.Count;
                                    //string generatedFileName = Workspace.This.GenerateFileName(string.Empty, "");
                                    string fileName = defaultFileName;
                                    for (int i = 0; i < Workspace.This.PhosphorVM.SelectedAppProtocol.ScanRegions.Count; i++)
                                    {
                                        if (nScanRegions > 1)
                                        {
                                            fileName = string.Format("{0}_SR{1}", defaultFileName, Workspace.This.PhosphorVM.SelectedAppProtocol.ScanRegions[i].ScanRegionNum);
                                        }
                                        Workspace.This.PhosphorVM.SelectedAppProtocol.ScanRegions[i].FileLocationVm.FileName = fileName;
                                    }
                                }
                            }
                        }
                    }

                    _SelectedImagingType = value;
                    RaisePropertyChanged("SelectedImagingType");
                    // Trigger UI update
                    //RaisePropertyChanged("IsScanningMode");
                    RaisePropertyChanged("IsFluorescenceImaging");
                    RaisePropertyChanged("IsPhosphorImaging");
                }
            }
        }

        public GalleryPanelType SelectedGalleryPanel
        {
            get { return _SelectedGalleryPanel; }
            set
            {
                if (_SelectedGalleryPanel == GalleryPanelType.TransformPanel)
                {
                    _ImageTransformVm.IsRotateArbitraryChecked = false;
                }
                else if (_SelectedGalleryPanel == GalleryPanelType.RoiPanel && value != GalleryPanelType.RoiPanel)
                {
                    if (ActiveDocument != null)
                    {
                        if (ActiveDocument.IsSelectionToolChecked)
                            ActiveDocument.IsSelectionToolChecked = false;
                    }
                }

                _SelectedGalleryPanel = value;
                RaisePropertyChanged("SelectedGalleryPanel");

                IsResizeChecked = (_SelectedGalleryPanel == GalleryPanelType.ResizePanel);
            }
        }

        public bool IsFluorescenceImaging
        {
            get
            {
                if (_SelectedApplicationTab == ApplicationTabType.Imaging &&
                    _SelectedImagingType == ImagingType.Fluorescence)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool IsPhosphorImaging
        {
            get
            {
                if (_SelectedApplicationTab == ApplicationTabType.Imaging &&
                    _SelectedImagingType == ImagingType.PhosphorImaging)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool IsImagingTab
        {
            get
            {
                bool bIsImagingTab = false;
                if (_SelectedApplicationTab == ApplicationTabType.Imaging)
                {
                    bIsImagingTab = true;
                }
                else
                {
                    bIsImagingTab = false;
                }
                return bIsImagingTab;
            }
        }

        public bool IsGalleryTab
        {
            get
            {
                bool bIsGalleryTab = false;
                if (_SelectedApplicationTab == ApplicationTabType.Gallery)
                {
                    bIsGalleryTab = true;

                    // Set gallery tab selected option
                    IsAdjustmentsChecked = true;
                    RaisePropertyChanged("IsAdjustmentsChecked");
                }
                else
                {
                    bIsGalleryTab = false;
                }
                return bIsGalleryTab;
            }
        }

        public bool IsAnalysisTab
        {
            get
            {
                bool bIsAnalysisTab = false;
                if (_SelectedApplicationTab == ApplicationTabType.Analysis)
                {
                    bIsAnalysisTab = true;
                }
                else
                {
                    bIsAnalysisTab = false;
                }
                return bIsAnalysisTab;
            }
        }

        public bool IsAnnotationTab
        {
            get
            {
                //bool bIsAnnotationTab = false;
                if (_SelectedApplicationTab == ApplicationTabType.Annotation)
                {
                    return true;
                }
                else
                {
                    return false;
                }
                //return bIsAnnotationTab;
            }
        }

        public bool IsSettingsTab
        {
            get
            {
                if (_SelectedApplicationTab == ApplicationTabType.Settings)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool IsHelpTab
        {
            get
            {
                bool bIsHelpTab = false;
                if (_SelectedApplicationTab == ApplicationTabType.Help)
                {
                    bIsHelpTab = true;
                }
                else
                {
                    bIsHelpTab = false;
                }
                return bIsHelpTab;
            }
        }

        /// <summary>
        /// Check the registry if Adobe reader is installed
        /// </summary>
        internal bool IsAdobeAcrobatInstalled
        {
            get
            {
                bool isAdobeAcroInstalled = false;

                var adobePath = Registry.GetValue(@"HKEY_CLASSES_ROOT\Software\Adobe\Acrobat\Exe", string.Empty, string.Empty);
                if (adobePath != null)
                {
                    var adobeExePath = (string)adobePath;
                    if (adobeExePath.Length > 0)
                    {
                        adobeExePath = adobeExePath.Replace("\"", string.Empty);
                        if (File.Exists(adobeExePath))
                            isAdobeAcroInstalled = true;
                    }
                }

                if (!isAdobeAcroInstalled)
                {
                    // First location check failed.
                    // Check the second location in the registry
                    //
                    RegistryKey software = Registry.LocalMachine.OpenSubKey("Software");
                    if (software != null)
                    {
                        RegistryKey adobe = null;
                        // Try to get 64bit versions of adobe
                        if (Environment.Is64BitOperatingSystem)
                        {
                            RegistryKey software64 = software.OpenSubKey("Wow6432Node");
                            if (software64 != null)
                                adobe = software64.OpenSubKey("Adobe");
                        }

                        // If a 64bit version is not installed, try to get a 32bit version
                        if (adobe == null)
                            adobe = software.OpenSubKey("Adobe");

                        // If no 64bit or 32bit version can be found, chances are adobe reader is not installed.
                        if (adobe != null)
                        {
                            RegistryKey acroRead = adobe.OpenSubKey("Adobe Acrobat");

                            if (acroRead != null)
                            {
                                string[] acroReadVersions = acroRead.GetSubKeyNames();
                                Console.WriteLine("The following version(s) of Acrobat Reader are installed: ");

                                foreach (string versionNumber in acroReadVersions)
                                {
                                    //Console.WriteLine(versionNumber);
                                    var acroInst = acroRead.OpenSubKey(versionNumber + "\\Installer");
                                    if (acroInst != null)
                                    {
                                        var acroExePath = acroInst.GetValue("Acrobat.exe").ToString();
                                        if (File.Exists(acroExePath))
                                        {
                                            isAdobeAcroInstalled = true;
                                            break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                //Console.WriteLine("Adobe reader is not installed!");
                                isAdobeAcroInstalled = false;
                            }
                        }
                        else
                        {
                            //Console.WriteLine("Adobe reader is not installed!");
                            isAdobeAcroInstalled = false;
                        }
                    }
                }

                return isAdobeAcroInstalled;
            }
        }

        public bool HasSecurePassword
        {
            get { return _HasSecurePassword; }
            set
            {
                _HasSecurePassword = value;
                RaisePropertyChanged("HasSecurePassword");
            }
        }

        public bool IsPreparingToScan
        {
            get { return _IsPreparingToScan; }
            set
            {
                if (_IsPreparingToScan != value)
                {
                    _IsPreparingToScan = value;
                    RaisePropertyChanged("IsPreparingToScan");
                }
            }
        }
        public bool IsReadyScanning
        {
            get { return _IsReadyScanning; }
            set
            {
                if (_IsReadyScanning != value)
                {
                    _IsReadyScanning = value;
                    RaisePropertyChanged("IsReadyScanning");
                }
            }
        }

        public bool IsScannerReady
        {
            get
            {
                bool bResult = false;
                if (_EthernetController != null && _EthernetController.IsConnected)
                {
                    if (!IsScanning)
                    {
                        bResult = true;
                    }
                }
                return bResult;
            }
        }

        public bool IsScanning
        {
            get { return _IsScanning; }
            set
            {
                if (_IsScanning != value)
                {
                    _IsScanning = value;
                    RaisePropertyChanged("IsScanning");
                    RaisePropertyChanged("IsScannerReady");
                }
            }
        }

        public string DoorStatus
        {
            get { return _DoorStatus; }
            set
            {
                if (_DoorStatus != value)
                {
                    _DoorStatus = value;
                    RaisePropertyChanged("DoorStatus");
                }
            }
        }

        public MotorViewModel MotorVM
        {
            get { return _MotorViewModel; }
        }

        public ApdViewModel ApdVM
        {
            get { return _ApdViewModel; }
        }

        public NewParameterSetupViewModel NewParameterVM
        {
            get { return _NewParamViewModel; }
        }

        public FluorescenceViewModel FluorescenceVM
        {
            get { return _FluorescenceVM; }
        }

        public PhosphorViewModel PhosphorVM
        {
            get { return _PhosphorVM; }
        }

        public SettingsViewModel SettingsVM
        {
            get { return _SettingsViewModel; }
        }

        //public double AbsFocusPosition
        //{
        //    get { return _AbsFocusPosition; }
        //    set
        //    {
        //        _AbsFocusPosition = value;
        //        RaisePropertyChanged("AbsFocusPosition");
        //    }
        //}

        public string StatusTextProgress
        {
            get { return _StatusTextProgress; }
            set
            {
                if (_StatusTextProgress != value)
                {
                    _StatusTextProgress = value;
                    RaisePropertyChanged("StatusTextProgress");
                }
            }
        }


        #region public DateTime CaptureStartTime
        /// <summary>
        /// Get/set the image capture start time.
        /// </summary>
        public DateTime CaptureStartTime
        {
            get { return _CaptureStartTime; }
            set
            {
                if (_CaptureStartTime != value)
                {
                    _CaptureStartTime = value;
                    RaisePropertyChanged("CaptureStartTime");
                }
            }
        }
        #endregion

        #region public double EstimatedCaptureTime
        /// <summary>
        /// Get/set estimate capture time.
        /// </summary>
        public double EstimatedCaptureTime
        {
            get { return _EstimatedCaptureTime; }
            set
            {
                if (_EstimatedCaptureTime != value)
                {
                    _EstimatedCaptureTime = value;
                    RaisePropertyChanged("EstimatedCaptureTime");
                }
            }
        }
        #endregion

        #region public double EstimatedRemainingTime
        private int _EstimatedCaptureRemainingTime;
        /// <summary>
        /// Get/set estimate remaining capture time.
        /// </summary>
        public int EstimatedCaptureRemainingTime
        {
            get { return _EstimatedCaptureRemainingTime; }
            set
            {
                if (_EstimatedCaptureRemainingTime != value)
                {
                    _EstimatedCaptureRemainingTime = value;
                    RaisePropertyChanged("EstimatedCaptureRemainingTime");
                }
            }
        }
        #endregion

        #region public string EstimatedTimeRemaining
        /// <summary>
        /// Get/set the Darkroom estimated capture time remaining status display string.
        /// </summary>
        public string EstimatedTimeRemaining
        {
            get { return _EstimatedTimeRemaining; }
            set
            {
                _EstimatedTimeRemaining = value;
                RaisePropertyChanged("EstimatedTimeRemaining");
            }
        }
        #endregion

        #region public double PercentCompleted
        /// <summary>
        /// get/set the percent capture/scan completed.
        /// </summary>
        public double PercentCompleted
        {
            get { return _PercentCompleted; }
            set
            {
                if (_PercentCompleted != value)
                {
                    _PercentCompleted = value;
                    RaisePropertyChanged("PercentCompleted");
                }
            }
        }
        #endregion

        //public DispatcherTimer CaptureDispatcherTimer
        //{
        //    get { return _CaptureDispatcherTimer; }
        //}

        /*#region private void dispatcherTimer_Tick(object sender, EventArgs e)
        /// <summary>
        /// Upate progress bar text
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _DispatcherTimer_Tick(object sender, EventArgs e)
        {
            double estTimeRemain = 0;
            double percentCompete = 0;
            double estimatedCaptureTimeInSec = 0;

            EstimatedTimeRemaining = string.Empty;

            if (IsCapturing || IsScanning)
            {
                //
                // We are capturing an image and NOT in "live" mode:
                //
                TimeSpan elapsedTime = DateTime.Now - CaptureStartTime;
                estimatedCaptureTimeInSec = EstimatedCaptureTime;

                if (estimatedCaptureTimeInSec > 0.0)
                {
                    estTimeRemain = Math.Max(0, estimatedCaptureTimeInSec - elapsedTime.TotalSeconds);
                    percentCompete = 100.0 * elapsedTime.TotalSeconds / estimatedCaptureTimeInSec;
                }
                else
                {
                    estTimeRemain = 0;
                    percentCompete = 0;
                }

                Owner.Dispatcher.Invoke((Action)delegate
                {
                    if (estTimeRemain > 0.0)
                    {
                        this.EstimatedTimeRemaining = "Estimated Time Remaining: " + estTimeRemain.ToString("F1") + " [sec]";
                        this.PercentCompleted = percentCompete;
                    }
                    else
                    {
                        this.EstimatedTimeRemaining = string.Empty;
                    }
                });

                // Forcing the CommandManager to raise the RequerySuggested event
                CommandManager.InvalidateRequerySuggested();
            }
        }
        #endregion*/

        #region private void dispatcherTimer_Tick(object sender, EventArgs e)
        /// <summary>
        /// Upate progress bar and percentage completed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /*private void _DispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (IsScanning && !IsPreparingToScan)
            {
                double percentCompleted = 0;
                EstimatedTimeRemaining = string.Empty;

                //RemainingTime = scanCommand.RemainingTime;
                //double timeElapsed = Time - RemainingTime;
                //Workspace.This.StatusTextProgress = ImagingSystemHelper.FormatTime(RemainingTime);
                //double percentCompleted = (timeElapsed / (double)Time) * 100.0;
                //Workspace.This.PercentCompleted = (int)percentCompleted;

                //TimeSpan elapsedTime = DateTime.Now - CaptureStartTime;
                //EstimatedCaptureRemainingTime

                if (EstimatedCaptureTime > 0.0)
                {
                    //percentCompleted = 100.0 * elapsedTime.TotalSeconds / EstimatedCaptureTime;
                    double timeElapsed = EstimatedCaptureTime - EstimatedCaptureRemainingTime;
                    percentCompleted = (int)((timeElapsed / (double)EstimatedCaptureTime) * 100.0);
                    EstimatedTimeRemaining = ImagingSystemHelper.FormatTime(EstimatedCaptureRemainingTime);
                }
                else
                {
                    PercentCompleted = 0;
                }

                Owner.Dispatcher.BeginInvoke((Action)delegate
                {
                    if (EstimatedCaptureRemainingTime > 0)
                    {
                        this.StatusTextProgress = EstimatedTimeRemaining;
                        this.PercentCompleted = percentCompleted;
                    }
                    else
                    {
                        this.EstimatedTimeRemaining = string.Empty;
                        this.PercentCompleted = 0;
                    }
                });

                // Forcing the CommandManager to raise the RequerySuggested event
                CommandManager.InvalidateRequerySuggested();
            }
        }*/
        #endregion


        /*private void Delay(int mm)
        {
            DateTime current = DateTime.Now;

            while (current.AddMilliseconds(mm) > DateTime.Now)
            {
                System.Windows.Forms.Application.DoEvents();
            }
            return;
        }*/


        #region Helper Methods

        public void TurnOffAllLasers()
        {
            if (_EthernetController != null && _EthernetController.IsConnected)
            {
                _EthernetController.SetLaserPower(LaserChannels.ChannelA, 0);
                _EthernetController.SetLaserPower(LaserChannels.ChannelB, 0);
                _EthernetController.SetLaserPower(LaserChannels.ChannelC, 0);
            }
        }

        public void SetImagingTabVisibility()
        {
            if (SettingsManager.ConfigSettings.ImagingSettings != null)
            {
                foreach (var imagingType in SettingsManager.ConfigSettings.ImagingSettings)
                {
                    if (imagingType.ImagingTabType == ImagingType.Fluorescence)
                    {
                        IsFluorescenceImagingVisible = imagingType.IsVisible;
                    }
                    else if (imagingType.ImagingTabType == ImagingType.PhosphorImaging)
                    {
                        this.IsPhosphorImagingVisible = imagingType.IsVisible;
                    }
                }

                // Set default Fluorescence tab selection
                if (!IsFluorescenceImagingVisible && IsPhosphorImagingVisible)
                {
                    if (SettingsManager.ConfigSettings.IsSimulationMode ||
                        (!SettingsManager.ConfigSettings.IsSimulationMode && Workspace.This.EthernetController.IsConnected))
                    {
                        // Phosphor ONLY - select the Phosphor Imaging tab.
                        SelectedApplicationTab = ApplicationTabType.Imaging;
                        SelectedImagingType = ImagingType.PhosphorImaging;
                    }
                    else
                    {
                        SelectedApplicationTab = ApplicationTabType.Gallery;
                        SelectedImagingType = ImagingType.None;
                    }
                }
                else if (IsFluorescenceImagingVisible)
                {
                    if (SettingsManager.ConfigSettings.IsSimulationMode ||
                        (!SettingsManager.ConfigSettings.IsSimulationMode && Workspace.This.EthernetController.IsConnected))
                    {
                        SelectedApplicationTab = ApplicationTabType.Imaging;
                        if (SelectedImagingType == ImagingType.None)
                            SelectedImagingType = ImagingType.Fluorescence;
                    }
                    else
                    {
                        SelectedApplicationTab = ApplicationTabType.Gallery;
                        SelectedImagingType = ImagingType.None;
                    }
                }
            }
        }

        public void ShowOnscreenKeyboard()
        {
            string progFiles = @"C:\Program Files\Common Files\Microsoft Shared\ink";
            string keyboardPath = System.IO.Path.Combine(progFiles, "TabTip.exe");

            System.Diagnostics.Process.Start(keyboardPath);
        }

        public void HideOnscreenKeyboard()
        {
            // retrieve the handler of the window
            int iHandle = WindowsInvoke.FindWindow("IPTIP_Main_Window", "");
            if (iHandle > 0)
            {
                // close the window using API
                WindowsInvoke.SendMessage(iHandle, WindowsInvoke.WM_SYSCOMMAND, WindowsInvoke.SC_CLOSE, 0);
            }
        }

        public string GetLaserWaveLength(ImageInfo imgInfo, ImagingSystem.LaserType laserType)
        {
            string result = string.Empty;
            if (imgInfo.LaserAWavelength != null || imgInfo.LaserBWavelength != null ||
                imgInfo.LaserCWavelength != null || imgInfo.LaserDWavelength != null)
            {
                if (laserType == ImagingSystem.LaserType.LaserA)
                {
                    if (!string.IsNullOrEmpty(imgInfo.LaserAWavelength) && !imgInfo.LaserAWavelength.Equals("0"))
                        result = imgInfo.LaserAWavelength;
                    else
                        result = ((int)laserType).ToString();
                }
                else if (laserType == ImagingSystem.LaserType.LaserB)
                {
                    if (!string.IsNullOrEmpty(imgInfo.LaserBWavelength) && !imgInfo.LaserBWavelength.Equals("0"))
                        result = imgInfo.LaserBWavelength;
                    else
                        result = ((int)laserType).ToString();
                }
                else if (laserType == ImagingSystem.LaserType.LaserC)
                {
                    if (!string.IsNullOrEmpty(imgInfo.LaserCWavelength) && !imgInfo.LaserCWavelength.Equals("0"))
                        result = imgInfo.LaserCWavelength;
                    else
                        result = ((int)laserType).ToString();
                }
                else if (laserType == ImagingSystem.LaserType.LaserD)
                {
                    if (!string.IsNullOrEmpty(imgInfo.LaserDWavelength) && !imgInfo.LaserDWavelength.Equals("0"))
                        result = imgInfo.LaserDWavelength;
                    else
                        result = ((int)laserType).ToString();
                }
            }
            else
            {
                //EL: TODO:
                //if (SettingsManager.ConfigSettings.LasersWavelength != null && SettingsManager.ConfigSettings.LasersWavelength.Count > 0)
                //{
                //    foreach (var wavelength in SettingsManager.ConfigSettings.LasersWavelength)
                //    {
                //        if (wavelength.LaserType == laserType)
                //        {
                //            string[] values = wavelength.Wavelength.Split('/');
                //            result = values[0].Trim();
                //            break;
                //        }
                //    }
                //}
                //else if (SettingsManager.ConfigSettings.DyeOptions != null && SettingsManager.ConfigSettings.DyeOptions.Count > 0)
                //{
                //    foreach (var dye in SettingsManager.ConfigSettings.DyeOptions)
                //    {
                //        if (dye.LaserType == laserType)
                //        {
                //            string[] values = dye.WaveLength.Split('/');
                //            result = values[0].Trim();
                //            break;
                //        }
                //    }
                //}
            }
            return result;
        }

        //static readonly string[] IntensityLevels = new[] { "0", "L1", "L2", "L3", "L4", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };
        static readonly string[] Rows = new[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
        public string IndexToRow(double index)
        {
            index = Math.Round(index);
            if (index > Rows.Length - 1) { index = Rows.Length - 1; }
            if (index < 0)
                throw new IndexOutOfRangeException("index must be a positive number");

            return Rows[(int)index];
        }


        public bool IsPhosphorModule(LaserTypes laserType)
        {
            bool bResult = false;
            if (laserType != null)
            {
                if (laserType.Wavelength == 638 || laserType.Wavelength == 658 || laserType.Wavelength == 685)
                {
                    if (laserType.SensorType == IvSensorType.PMT)
                    {
                        bResult = true;
                    }
                }
            }
            return bResult;
        }
        public bool IsPhosphorModule(LaserModule laserModule)
        {
            bool bResult = false;
            if (laserModule != null)
            {
                if (laserModule.LaserWavelength == 638 || laserModule.LaserWavelength == 658 || laserModule.LaserWavelength == 685)
                {
                    if (laserModule.SensorType == IvSensorType.PMT)
                    {
                        bResult = true;
                    }
                }
            }
            return bResult;
        }
        public bool ContainsPhosphorModule(ObservableCollection<LaserTypes> laserOptions)
        {
            bool bResult = false;
            if (laserOptions != null && laserOptions.Count > 0)
            {
                foreach (var laser in laserOptions)
                {
                    if (laser.Wavelength == 638 || laser.Wavelength == 658 || laser.Wavelength == 685)
                    {
                        if (laser.SensorType == IvSensorType.PMT)
                        {
                            bResult = true;
                            break;
                        }
                    }
                }
            }
            return bResult;
        }


        public ObservableCollection<LaserTypes> RemovePhosphorModule(ObservableCollection<LaserTypes> laserOptions)
        {
            ObservableCollection<LaserTypes> result = null;

            if (laserOptions != null && laserOptions.Count > 0)
            {
                var laserList = laserOptions.ToList();
                // Remove Phosphor laser module from the laser options
                if (laserList != null && laserList.Count > 0)
                {
                    for (int i = laserList.Count - 1; i >= 0; i--)
                    {
                        if (IsPhosphorModule(laserList[i]))
                        {
                            laserList.Remove(laserList[i]);
                        }
                    }
                }
                result = new ObservableCollection<LaserTypes>(laserList);
            }
            return result;
        }


        private LaserTemperature _LaserSensorTemp;
        public void LaserSensorTemperatureLogging(bool bIsEnableLogging)
        {
            if (bIsEnableLogging)
            {
                if (_LaserSensorTemp == null)
                {
                    _LaserSensorTemp = new LaserTemperature();
                    _LaserSensorTemp.AppDataPath = AppDataPath;
                    _LaserSensorTemp.IsWriteToLogFileEnabled = SettingsManager.ConfigSettings.IsLaserTempLogging;
                    if (SettingsManager.ConfigSettings.LaserTempLoggingInterval > 0)
                    {
                        _LaserSensorTemp.Interval = SettingsManager.ConfigSettings.LaserTempLoggingInterval * 1000; // time in milliseconds
                    }
                    else
                    {
                        _LaserSensorTemp.Interval = 30000;  //default to 30 seconds
                    }
                    _LaserSensorTemp.EthernetDevice = EthernetController;
                }
                _LaserSensorTemp.Start();
            }
            else
            {
                if (_LaserSensorTemp != null)
                {
                    _LaserSensorTemp.Stop();
                }
            }
        }

        //获取othe Setting中的三个仓位参数  Obtain the three bin parameters in the Other Setting 
        /*public void PreloadModuleInformation()
        {
            StartWaitAnimation("Loading...");
            This.NewParameterVM.OtherSettingBtnIsEnable = false;
            _IsLoading = true;
            BackgroundWorker worker = new BackgroundWorker();
            // Open the document in a different thread
            worker.DoWork += (o, ea) =>
            {
                This.NewParameterVM.LIVFirmwareVersionSN = "NaN";
                This.NewParameterVM.LIVFirmwareSN = "NaN";
                This.NewParameterVM.LTEControlTemperature = 0;
                This.NewParameterVM.LTECMaximumCoolingCurrent = 0;
                This.NewParameterVM.LOpticalPowerLessThanOrEqual15mWKp = 0;
                This.NewParameterVM.LOpticalPowerLessThanOrEqual15mWKi = 0;
                This.NewParameterVM.LOpticalPowerLessThanOrEqual15mWKd = 0;
                This.NewParameterVM.LOpticalPowerGreaterThan15mWKp = 0;
                This.NewParameterVM.LOpticalPowerGreaterThan15mWKi = 0;
                This.NewParameterVM.LOpticalPowerGreaterThan15mWKd = 0;
                This.NewParameterVM.LLaserMaxCurrent = 0;
                This.NewParameterVM.LLaserMinCurrent = 0;
                This.NewParameterVM.LTECControlKp = 0;
                This.NewParameterVM.LTECControlKi = 0;
                This.NewParameterVM.LTECControlKd = 0;


                This.NewParameterVM.R1IVFirmwareVersionSN = "NaN";
                This.NewParameterVM.R1IVFirmwareSN = "NaN";
                This.NewParameterVM.R1TEControlTemperature = 0;
                This.NewParameterVM.R1TECMaximumCoolingCurrent = 0;
                This.NewParameterVM.R1OpticalPowerLessThanOrEqual15mWKp = 0;
                This.NewParameterVM.R1OpticalPowerLessThanOrEqual15mWKi = 0;
                This.NewParameterVM.R1OpticalPowerLessThanOrEqual15mWKd = 0;
                This.NewParameterVM.R1OpticalPowerGreaterThan15mWKp = 0;
                This.NewParameterVM.R1OpticalPowerGreaterThan15mWKi = 0;
                This.NewParameterVM.R1OpticalPowerGreaterThan15mWKd = 0;
                This.NewParameterVM.R1LaserMaxCurrent = 0;
                This.NewParameterVM.R1LaserMinCurrent = 0;
                This.NewParameterVM.R1TECControlKp = 0;
                This.NewParameterVM.R1TECControlKi = 0;
                This.NewParameterVM.R1TECControlKd = 0;


                This.NewParameterVM.R2IVFirmwareVersionSN = "NaN";
                This.NewParameterVM.R2IVFirmwareSN = "NaN";
                This.NewParameterVM.R2TEControlTemperature = 0;
                This.NewParameterVM.R2TECMaximumCoolingCurrent = 0;
                This.NewParameterVM.R2OpticalPowerLessThanOrEqual15mWKp = 0;
                This.NewParameterVM.R2OpticalPowerLessThanOrEqual15mWKi = 0;
                This.NewParameterVM.R2OpticalPowerLessThanOrEqual15mWKd = 0;
                This.NewParameterVM.R2OpticalPowerGreaterThan15mWKp = 0;
                This.NewParameterVM.R2OpticalPowerGreaterThan15mWKi = 0;
                This.NewParameterVM.R2OpticalPowerGreaterThan15mWKd = 0;
                This.NewParameterVM.R2LaserMaxCurrent = 0;
                This.NewParameterVM.R2LaserMinCurrent = 0;
                This.NewParameterVM.R2TECControlKp = 0;
                This.NewParameterVM.R2TECControlKi = 0;
                This.NewParameterVM.R2TECControlKd = 0;

                EthernetController.IVOpticalModuleSerialNumber[IVChannels.ChannelA] = "0";
                EthernetController.IVOpticalModuleSerialNumber[IVChannels.ChannelB] = "0";
                EthernetController.IVOpticalModuleSerialNumber[IVChannels.ChannelC] = "0";

                EthernetController.IVEstimatedVersionNumberBoard[IVChannels.ChannelA] = "0";
                EthernetController.IVEstimatedVersionNumberBoard[IVChannels.ChannelB] = "0";
                EthernetController.IVEstimatedVersionNumberBoard[IVChannels.ChannelC] = "0";

                EthernetController.TECControlTemperature[LaserChannels.ChannelA] = 0;
                EthernetController.TECControlTemperature[LaserChannels.ChannelB] = 0;
                EthernetController.TECControlTemperature[LaserChannels.ChannelC] = 0;

                EthernetController.TECMaximumCurrent[LaserChannels.ChannelA] = 0;
                EthernetController.TECMaximumCurrent[LaserChannels.ChannelB] = 0;
                EthernetController.TECMaximumCurrent[LaserChannels.ChannelC] = 0;

                EthernetController.AllOpticalPowerLessThanOrEqual15mWKp[LaserChannels.ChannelA] = 0;
                EthernetController.AllOpticalPowerLessThanOrEqual15mWKp[LaserChannels.ChannelB] = 0;
                EthernetController.AllOpticalPowerLessThanOrEqual15mWKp[LaserChannels.ChannelC] = 0;

                EthernetController.AllOpticalPowerLessThanOrEqual15mWKi[LaserChannels.ChannelA] = 0;
                EthernetController.AllOpticalPowerLessThanOrEqual15mWKi[LaserChannels.ChannelB] = 0;
                EthernetController.AllOpticalPowerLessThanOrEqual15mWKi[LaserChannels.ChannelC] = 0;

                EthernetController.AllOpticalPowerLessThanOrEqual15mWKd[LaserChannels.ChannelA] = 0;
                EthernetController.AllOpticalPowerLessThanOrEqual15mWKd[LaserChannels.ChannelB] = 0;
                EthernetController.AllOpticalPowerLessThanOrEqual15mWKd[LaserChannels.ChannelC] = 0;

                EthernetController.AllOpticalPowerGreaterThan15mWKp[LaserChannels.ChannelA] = 0;
                EthernetController.AllOpticalPowerGreaterThan15mWKp[LaserChannels.ChannelB] = 0;
                EthernetController.AllOpticalPowerGreaterThan15mWKp[LaserChannels.ChannelC] = 0;

                EthernetController.AllOpticalPowerGreaterThan15mWKi[LaserChannels.ChannelA] = 0;
                EthernetController.AllOpticalPowerGreaterThan15mWKi[LaserChannels.ChannelB] = 0;
                EthernetController.AllOpticalPowerGreaterThan15mWKi[LaserChannels.ChannelC] = 0;

                EthernetController.AllOpticalPowerGreaterThan15mWKd[LaserChannels.ChannelA] = 0;
                EthernetController.AllOpticalPowerGreaterThan15mWKd[LaserChannels.ChannelB] = 0;
                EthernetController.AllOpticalPowerGreaterThan15mWKd[LaserChannels.ChannelC] = 0;

                EthernetController.AllGetLaserMaximumCurrent[LaserChannels.ChannelA] = 0;
                EthernetController.AllGetLaserMaximumCurrent[LaserChannels.ChannelB] = 0;
                EthernetController.AllGetLaserMaximumCurrent[LaserChannels.ChannelC] = 0;

                EthernetController.AllGetLaserMinimumCurrent[LaserChannels.ChannelA] = 0;
                EthernetController.AllGetLaserMinimumCurrent[LaserChannels.ChannelB] = 0;
                EthernetController.AllGetLaserMinimumCurrent[LaserChannels.ChannelC] = 0;

                EthernetController.TECRefrigerationControlParameterKp[LaserChannels.ChannelA] = 0;
                EthernetController.TECRefrigerationControlParameterKp[LaserChannels.ChannelB] = 0;
                EthernetController.TECRefrigerationControlParameterKp[LaserChannels.ChannelC] = 0;

                EthernetController.TECRefrigerationControlParameterKi[LaserChannels.ChannelA] = 0;
                EthernetController.TECRefrigerationControlParameterKi[LaserChannels.ChannelB] = 0;
                EthernetController.TECRefrigerationControlParameterKi[LaserChannels.ChannelC] = 0;

                EthernetController.TECRefrigerationControlParameterKd[LaserChannels.ChannelA] = 0;
                EthernetController.TECRefrigerationControlParameterKd[LaserChannels.ChannelB] = 0;
                EthernetController.TECRefrigerationControlParameterKd[LaserChannels.ChannelC] = 0;
                int sleeptime = 200;
                for (int i = 0; i < 2; i++)
                {
                    if (LaserModuleL1.LaserWavelength != 0 && LaserModuleL1.LaserWavelength != Workspace.This.NewParameterVM.Uint16Code)
                    {
                        //Laser 板子固件版本号
                        //Laser board firmware version number
                        if (!This.EthernetController.GetLaserSystemVersions(LaserChannels.ChannelC))
                        {
                            This.NewParameterVM.OtherSettingBtnIsEnable = true;
                            StopWaitAnimation();
                            _IsLoading = false;
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                Window window = new Window();
                                MessageBox.Show(window, "GetLaserSystemVersions failed.");
                            });
                            return;
                        }
                        Thread.Sleep(sleeptime);
                        if (!This.EthernetController.GetLaserOpticalModuleSerialNumber(LaserChannels.ChannelC))
                        {
                            This.NewParameterVM.OtherSettingBtnIsEnable = true;
                            StopWaitAnimation();
                            _IsLoading = false;
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                Window window = new Window();
                                MessageBox.Show(window, "GetLaserOpticalModuleSerialNumber failed.");
                            });
                            return;
                        }
                        Thread.Sleep(sleeptime);
                        //TEC 控制温度
                        //TEC control temperature
                        if (!This.EthernetController.GetAllTECControlTTemperatures(LaserChannels.ChannelC))
                        {
                            This.NewParameterVM.OtherSettingBtnIsEnable = true;
                            StopWaitAnimation();
                            _IsLoading = false;
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                Window window = new Window();
                                MessageBox.Show(window, "GetAllTECControlTTemperatures failed.");
                            });
                            return;
                        }
                        Thread.Sleep(sleeptime);
                        //TEC 最大制冷电流
                        //TEC maximum cooling current
                        if (!This.EthernetController.GetAllTECMaximumCoolingCurrentValue(LaserChannels.ChannelC))
                        {
                            This.NewParameterVM.OtherSettingBtnIsEnable = true;
                            StopWaitAnimation();
                            _IsLoading = false;
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                Window window = new Window();
                                MessageBox.Show(window, "GetAllTECMaximumCoolingCurrentValue failed.");
                            });
                            return;
                        }
                        Thread.Sleep(sleeptime);
                        //TEC控制kp
                        //TEC control kp
                        if (!This.EthernetController.GetAllCurrentTECRefrigerationControlParameterKp(LaserChannels.ChannelC))
                        {
                            This.NewParameterVM.OtherSettingBtnIsEnable = true;
                            StopWaitAnimation();
                            _IsLoading = false;
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                Window window = new Window();
                                MessageBox.Show(window, "GetAllCurrentTECRefrigerationControlParameterKp failed.");
                            });
                            return;
                        }
                        Thread.Sleep(sleeptime);
                        //TEC控制ki
                        //TEC control ki
                        if (!This.EthernetController.GetAllCurrentTECRefrigerationControlParameterKi(LaserChannels.ChannelC))
                        {
                            This.NewParameterVM.OtherSettingBtnIsEnable = true;
                            StopWaitAnimation();
                            _IsLoading = false;
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                Window window = new Window();
                                MessageBox.Show(window, "GetAllCurrentTECRefrigerationControlParameterKi failed.");
                            });
                            return;
                        }
                        Thread.Sleep(sleeptime);
                        //TEC控制kd
                        //TEC control kd
                        if (!This.EthernetController.GetAllCurrentTECRefrigerationControlParameterKd(LaserChannels.ChannelC))
                        {
                            This.NewParameterVM.OtherSettingBtnIsEnable = true;
                            StopWaitAnimation();
                            _IsLoading = false;
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                Window window = new Window();
                                MessageBox.Show(window, "GetAllCurrentTECRefrigerationControlParameterKd failed.");
                            });
                            return;
                        }
                        if (LaserModuleL1.LaserWavelength == 532)
                        {
                            //光功率(<=15mW)控制kp
                            ////Optical power (<=15mW) control kp
                            Thread.Sleep(sleeptime);
                            if (!This.EthernetController.GetOpticalPowerLessThanOrEqual15mWKp(LaserChannels.ChannelC))
                            {
                                This.NewParameterVM.OtherSettingBtnIsEnable = true;
                                StopWaitAnimation();
                                _IsLoading = false;
                                Application.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    Window window = new Window();
                                    MessageBox.Show(window, "GetOpticalPowerLessThanOrEqual15mWKp failed.");
                                });
                                return;
                            }
                            Thread.Sleep(sleeptime);
                            //光功率(<=15mW)控制ki
                            //Optical power (<=15mW) control ki
                            if (!This.EthernetController.GetOpticalPowerLessThanOrEqual15mWKi(LaserChannels.ChannelC))
                            {
                                This.NewParameterVM.OtherSettingBtnIsEnable = true;
                                StopWaitAnimation();
                                _IsLoading = false;
                                Application.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    Window window = new Window();
                                    MessageBox.Show(window, "GetOpticalPowerLessThanOrEqual15mWKi failed.");
                                });
                                return;
                            }
                            Thread.Sleep(sleeptime);
                            //光功率(<=15mW)控制kd
                            //Optical power (<=15mW) control kd
                            if (!This.EthernetController.GetOpticalPowerLessThanOrEqual15mWKd(LaserChannels.ChannelC))
                            {
                                This.NewParameterVM.OtherSettingBtnIsEnable = true;
                                StopWaitAnimation();
                                _IsLoading = false;
                                Application.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    Window window = new Window();
                                    MessageBox.Show(window, "GetOpticalPowerLessThanOrEqual15mWKd failed.");
                                });
                                return;
                            }
                            Thread.Sleep(sleeptime);
                            //   光功率(>15mW)控制kp
                            //Optical power (>15mW) control kp
                            if (!This.EthernetController.GetAllOpticalPowerGreaterThan15mWKp(LaserChannels.ChannelC))
                            {
                                This.NewParameterVM.OtherSettingBtnIsEnable = true;
                                StopWaitAnimation();
                                _IsLoading = false;
                                Application.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    Window window = new Window();
                                    MessageBox.Show(window, "GetAllOpticalPowerGreaterThan15mWKp failed.");
                                });
                                return;
                            }
                            Thread.Sleep(sleeptime);
                            //   光功率(>15mW)控制ki
                            //Optical power (>15mW) control ki
                            if (!This.EthernetController.GetAllOpticalPowerGreaterThan15mWKi(LaserChannels.ChannelC))
                            {
                                This.NewParameterVM.OtherSettingBtnIsEnable = true;
                                StopWaitAnimation();
                                _IsLoading = false;
                                Application.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    Window window = new Window();
                                    MessageBox.Show(window, "GetAllOpticalPowerGreaterThan15mWKi failed.");
                                });
                                return;
                            }
                            Thread.Sleep(sleeptime);
                            //   光功率(>15mW)控制kd
                            //Optical power (>15mW) control kd
                            if (!This.EthernetController.GetAllOpticalPowerGreaterThan15mWKd(LaserChannels.ChannelC))
                            {
                                This.NewParameterVM.OtherSettingBtnIsEnable = true;
                                StopWaitAnimation();
                                _IsLoading = false;
                                Application.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    Window window = new Window();
                                    MessageBox.Show(window, "GetAllOpticalPowerGreaterThan15mWKd failed.");
                                });
                                return;
                            }
                            Thread.Sleep(sleeptime);
                            //激光器最大电流
                            //Laser maximum current
                            if (!This.EthernetController.GetAllLaserMaximumCurrent(LaserChannels.ChannelC))
                            {
                                This.NewParameterVM.OtherSettingBtnIsEnable = true;
                                StopWaitAnimation();
                                _IsLoading = false;
                                Application.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    Window window = new Window();
                                    MessageBox.Show(window, "GetAllLaserMaximumCurrent failed.");
                                });
                                return;
                            }
                            Thread.Sleep(sleeptime);
                            //激光器最小电流
                            //Laser minimum current
                            if (!This.EthernetController.GetAllLaserMinimumCurrent(LaserChannels.ChannelC))
                            {
                                This.NewParameterVM.OtherSettingBtnIsEnable = true;
                                StopWaitAnimation();
                                _IsLoading = false;
                                Application.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    Window window = new Window();
                                    MessageBox.Show(window, "GetAllLaserMinimumCurrent failed.");
                                });
                                return;
                            }
                        }
                        Thread.Sleep(sleeptime);

                    }

                    if (LaserModuleR1.LaserWavelength != 0 && LaserModuleR1.LaserWavelength != Workspace.This.NewParameterVM.Uint16Code)
                    {
                        //Laser 板子固件版本号
                        //Laser board firmware version number
                        if (!This.EthernetController.GetLaserSystemVersions(LaserChannels.ChannelA))
                        {
                            This.NewParameterVM.OtherSettingBtnIsEnable = true;
                            StopWaitAnimation();
                            _IsLoading = false;
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                Window window = new Window();
                                MessageBox.Show(window, "GetLaserSystemVersions failed.");
                            });
                            return;
                        }
                        Thread.Sleep(sleeptime);
                        //Laser 板子软件版本号
                        //Software versioning of board Laser
                        if (!This.EthernetController.GetLaserOpticalModuleSerialNumber(LaserChannels.ChannelA))
                        {
                            This.NewParameterVM.OtherSettingBtnIsEnable = true;
                            StopWaitAnimation();
                            _IsLoading = false;
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                Window window = new Window();
                                MessageBox.Show(window, "GetLaserOpticalModuleSerialNumber failed.");
                            });
                            return;
                        }
                        Thread.Sleep(sleeptime);
                        //TEC 控制温度
                        //TEC control temperature
                        if (!This.EthernetController.GetAllTECControlTTemperatures(LaserChannels.ChannelA))
                        {
                            This.NewParameterVM.OtherSettingBtnIsEnable = true;
                            StopWaitAnimation();
                            _IsLoading = false;
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                Window window = new Window();
                                MessageBox.Show(window, "GetAllTECControlTTemperatures failed.");
                            });
                            return;
                        }
                        Thread.Sleep(sleeptime);
                        //TEC 最大制冷电流
                        //TEC maximum cooling current
                        if (!This.EthernetController.GetAllTECMaximumCoolingCurrentValue(LaserChannels.ChannelA))
                        {
                            This.NewParameterVM.OtherSettingBtnIsEnable = true;
                            StopWaitAnimation();
                            _IsLoading = false;
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                Window window = new Window();
                                MessageBox.Show(window, "GetAllTECMaximumCoolingCurrentValue failed.");
                            });
                            return;
                        }

                        Thread.Sleep(sleeptime);
                        //TEC控制kp
                        //TEC control kp
                        if (!This.EthernetController.GetAllCurrentTECRefrigerationControlParameterKp(LaserChannels.ChannelA))
                        {
                            This.NewParameterVM.OtherSettingBtnIsEnable = true;
                            StopWaitAnimation();
                            _IsLoading = false;
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                Window window = new Window();
                                MessageBox.Show(window, "GetAllCurrentTECRefrigerationControlParameterKp failed.");
                            });
                            return;
                        }
                        Thread.Sleep(sleeptime);
                        //TEC控制ki
                        //TEC control ki
                        if (!This.EthernetController.GetAllCurrentTECRefrigerationControlParameterKi(LaserChannels.ChannelA))
                        {
                            This.NewParameterVM.OtherSettingBtnIsEnable = true;
                            StopWaitAnimation();
                            _IsLoading = false;
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                Window window = new Window();
                                MessageBox.Show(window, "GetAllCurrentTECRefrigerationControlParameterKi failed.");
                            });
                            return;
                        }
                        Thread.Sleep(sleeptime);
                        //TEC控制kd
                        //TEC control kd
                        if (!This.EthernetController.GetAllCurrentTECRefrigerationControlParameterKd(LaserChannels.ChannelA))
                        {
                            This.NewParameterVM.OtherSettingBtnIsEnable = true;
                            StopWaitAnimation();
                            _IsLoading = false;
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                Window window = new Window();
                                MessageBox.Show(window, "GetAllCurrentTECRefrigerationControlParameterKd failed.");
                            });
                            return;
                        }
                        if (LaserModuleR1.LaserWavelength == 532)
                        {
                            //光功率(<=15mW)控制kp
                            //Optical power (<=15mW) control kp
                            Thread.Sleep(sleeptime);
                            if (!This.EthernetController.GetOpticalPowerLessThanOrEqual15mWKp(LaserChannels.ChannelA))
                            {
                                This.NewParameterVM.OtherSettingBtnIsEnable = true;
                                StopWaitAnimation();
                                _IsLoading = false;
                                Application.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    Window window = new Window();
                                    MessageBox.Show(window, "GetOpticalPowerLessThanOrEqual15mWKp failed.");
                                });
                                return;
                            }
                            Thread.Sleep(sleeptime);
                            //光功率(<=15mW)控制ki
                            //Optical power (<=15mW) control ki
                            if (!This.EthernetController.GetOpticalPowerLessThanOrEqual15mWKi(LaserChannels.ChannelA))
                            {
                                This.NewParameterVM.OtherSettingBtnIsEnable = true;
                                StopWaitAnimation();
                                _IsLoading = false;
                                Application.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    Window window = new Window();
                                    MessageBox.Show(window, "GetOpticalPowerLessThanOrEqual15mWKi failed.");
                                });
                                return;
                            }
                            Thread.Sleep(sleeptime);
                            //光功率(<=15mW)控制kd
                            //Optical power (<=15mW) control kd
                            if (!This.EthernetController.GetOpticalPowerLessThanOrEqual15mWKd(LaserChannels.ChannelA))
                            {
                                This.NewParameterVM.OtherSettingBtnIsEnable = true;
                                StopWaitAnimation();
                                _IsLoading = false;
                                Application.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    Window window = new Window();
                                    MessageBox.Show(window, "GetOpticalPowerLessThanOrEqual15mWKd failed.");
                                });
                                return;
                            }
                            Thread.Sleep(sleeptime);
                            //   光功率(>15mW)控制kp
                            //Optical power (>15mW) control kp
                            if (!This.EthernetController.GetAllOpticalPowerGreaterThan15mWKp(LaserChannels.ChannelA))
                            {
                                This.NewParameterVM.OtherSettingBtnIsEnable = true;
                                StopWaitAnimation();
                                _IsLoading = false;
                                Application.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    Window window = new Window();
                                    MessageBox.Show(window, "GetAllOpticalPowerGreaterThan15mWKp failed.");
                                });
                                return;
                            }
                            Thread.Sleep(sleeptime);
                            //   光功率(>15mW)控制ki
                            //Optical power (>15mW) control ki
                            if (!This.EthernetController.GetAllOpticalPowerGreaterThan15mWKi(LaserChannels.ChannelA))
                            {
                                This.NewParameterVM.OtherSettingBtnIsEnable = true;
                                StopWaitAnimation();
                                _IsLoading = false;
                                Application.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    Window window = new Window();
                                    MessageBox.Show(window, "GetAllOpticalPowerGreaterThan15mWKi failed.");
                                });
                                return;
                            }
                            Thread.Sleep(sleeptime);
                            //   光功率(>15mW)控制kd
                            //Optical power (>15mW) control kd
                            if (!This.EthernetController.GetAllOpticalPowerGreaterThan15mWKd(LaserChannels.ChannelA))
                            {
                                This.NewParameterVM.OtherSettingBtnIsEnable = true;
                                StopWaitAnimation();
                                _IsLoading = false;
                                Application.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    Window window = new Window();
                                    MessageBox.Show(window, "GetAllOpticalPowerGreaterThan15mWKd failed.");
                                });
                                return;
                            }
                            Thread.Sleep(sleeptime);
                            //激光器最大电流
                            //Laser maximum current
                            if (!This.EthernetController.GetAllLaserMaximumCurrent(LaserChannels.ChannelA))
                            {
                                This.NewParameterVM.OtherSettingBtnIsEnable = true;
                                StopWaitAnimation();
                                _IsLoading = false;
                                Application.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    Window window = new Window();
                                    MessageBox.Show(window, "GetAllLaserMaximumCurrent failed.");
                                });
                                return;
                            }
                            Thread.Sleep(sleeptime);
                            //激光器最小电流
                            //Laser minimum current
                            if (!This.EthernetController.GetAllLaserMinimumCurrent(LaserChannels.ChannelA))
                            {
                                This.NewParameterVM.OtherSettingBtnIsEnable = true;
                                StopWaitAnimation();
                                _IsLoading = false;
                                Application.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    Window window = new Window();
                                    MessageBox.Show(window, "GetAllLaserMinimumCurrent failed.");
                                });
                                return;
                            }
                        }
                        Thread.Sleep(sleeptime);
                    }

                    if (LaserModuleR2.LaserWavelength != 0 && Workspace.This.LaserModuleR2.LaserWavelength != Workspace.This.NewParameterVM.Uint16Code)
                    {
                        //Laser 板子固件版本号
                        //Laser board firmware version number
                        if (!This.EthernetController.GetLaserSystemVersions(LaserChannels.ChannelB))
                        {
                            This.NewParameterVM.OtherSettingBtnIsEnable = true;
                            StopWaitAnimation();
                            _IsLoading = false;
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                Window window = new Window();
                                MessageBox.Show(window, "GetLaserSystemVersions failed.");
                            });
                            return;
                        }
                        Thread.Sleep(sleeptime);
                        //Laser 板子软件版本号
                        //Software versioning of board Laser
                        if (!This.EthernetController.GetLaserOpticalModuleSerialNumber(LaserChannels.ChannelB))
                        {
                            This.NewParameterVM.OtherSettingBtnIsEnable = true;
                            StopWaitAnimation();
                            _IsLoading = false;
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                Window window = new Window();
                                MessageBox.Show(window, "GetLaserOpticalModuleSerialNumber failed.");
                            });
                            return;
                        }
                        Thread.Sleep(sleeptime);
                        //TEC 控制温度
                        //TEC control temperature
                        if (!This.EthernetController.GetAllTECControlTTemperatures(LaserChannels.ChannelB))
                        {
                            This.NewParameterVM.OtherSettingBtnIsEnable = true;
                            StopWaitAnimation();
                            _IsLoading = false;
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                Window window = new Window();
                                MessageBox.Show(window, "GetAllTECControlTTemperatures failed.");
                            });
                            return;
                        }
                        Thread.Sleep(sleeptime);
                        //TEC 最大制冷电流
                        //TEC maximum cooling current
                        if (!This.EthernetController.GetAllTECMaximumCoolingCurrentValue(LaserChannels.ChannelB))
                        {
                            This.NewParameterVM.OtherSettingBtnIsEnable = true;
                            StopWaitAnimation();
                            _IsLoading = false;
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                Window window = new Window();
                                MessageBox.Show(window, "GetAllTECMaximumCoolingCurrentValue failed.");
                            });
                            return;
                        }
                        Thread.Sleep(sleeptime);
                        //TEC控制kp
                        //TEC control kp
                        if (!This.EthernetController.GetAllCurrentTECRefrigerationControlParameterKp(LaserChannels.ChannelB))
                        {
                            This.NewParameterVM.OtherSettingBtnIsEnable = true;
                            StopWaitAnimation();
                            _IsLoading = false;
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                Window window = new Window();
                                MessageBox.Show(window, "GetAllCurrentTECRefrigerationControlParameterKp failed.");
                            });
                            return;
                        }
                        Thread.Sleep(sleeptime);
                        //TEC控制ki
                        //TEC control ki
                        if (!This.EthernetController.GetAllCurrentTECRefrigerationControlParameterKi(LaserChannels.ChannelB))
                        {
                            This.NewParameterVM.OtherSettingBtnIsEnable = true;
                            StopWaitAnimation();
                            _IsLoading = false;
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                Window window = new Window();
                                MessageBox.Show(window, "GetAllCurrentTECRefrigerationControlParameterKi failed.");
                            });
                            return;
                        }
                        Thread.Sleep(sleeptime);
                        //TEC控制kd
                        //TEC control kd
                        if (!This.EthernetController.GetAllCurrentTECRefrigerationControlParameterKd(LaserChannels.ChannelB))
                        {
                            This.NewParameterVM.OtherSettingBtnIsEnable = true;
                            StopWaitAnimation();
                            _IsLoading = false;
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                Window window = new Window();
                                MessageBox.Show(window, "GetAllCurrentTECRefrigerationControlParameterKd failed.");
                            });
                            return;
                        }
                        if (LaserModuleR2.LaserWavelength == 532)
                        {
                            Thread.Sleep(sleeptime);
                            //光功率(<=15mW)控制kp
                            //Optical power (<=15mW) control kp
                            if (!This.EthernetController.GetOpticalPowerLessThanOrEqual15mWKp(LaserChannels.ChannelB))
                            {
                                This.NewParameterVM.OtherSettingBtnIsEnable = true;
                                StopWaitAnimation();
                                _IsLoading = false;
                                Application.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    Window window = new Window();
                                    MessageBox.Show(window, "GetOpticalPowerLessThanOrEqual15mWKp failed.");
                                });
                                return;
                            }
                            Thread.Sleep(sleeptime);
                            //光功率(<=15mW)控制ki
                            //Optical power (<=15mW) control ki
                            if (!This.EthernetController.GetOpticalPowerLessThanOrEqual15mWKi(LaserChannels.ChannelB))
                            {
                                This.NewParameterVM.OtherSettingBtnIsEnable = true;
                                StopWaitAnimation();
                                _IsLoading = false;
                                Application.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    Window window = new Window();
                                    MessageBox.Show(window, "GetOpticalPowerLessThanOrEqual15mWKi failed.");
                                });
                                return;
                            }
                            Thread.Sleep(sleeptime);
                            //光功率(<=15mW)控制kd
                            //Optical power (<=15mW) control kd
                            if (!This.EthernetController.GetOpticalPowerLessThanOrEqual15mWKd(LaserChannels.ChannelB))
                            {
                                This.NewParameterVM.OtherSettingBtnIsEnable = true;
                                StopWaitAnimation();
                                _IsLoading = false;
                                Application.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    Window window = new Window();
                                    MessageBox.Show(window, "GetOpticalPowerLessThanOrEqual15mWKd failed.");
                                });
                                return;
                            }
                            Thread.Sleep(sleeptime);
                            //   光功率(>15mW)控制kp
                            //Optical power (>15mW) control kp
                            if (!This.EthernetController.GetAllOpticalPowerGreaterThan15mWKp(LaserChannels.ChannelB))
                            {
                                This.NewParameterVM.OtherSettingBtnIsEnable = true;
                                StopWaitAnimation();
                                _IsLoading = false;
                                Application.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    Window window = new Window();
                                    MessageBox.Show(window, "GetAllOpticalPowerGreaterThan15mWKp failed.");
                                });
                                return;
                            }
                            Thread.Sleep(sleeptime);
                            //   光功率(>15mW)控制ki
                            //Optical power (>15mW) control ki
                            if (!This.EthernetController.GetAllOpticalPowerGreaterThan15mWKi(LaserChannels.ChannelB))
                            {
                                This.NewParameterVM.OtherSettingBtnIsEnable = true;
                                StopWaitAnimation();
                                _IsLoading = false;
                                Application.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    Window window = new Window();
                                    MessageBox.Show(window, "GetAllOpticalPowerGreaterThan15mWKi failed.");
                                });
                                return;
                            }
                            Thread.Sleep(sleeptime);
                            //   光功率(>15mW)控制kd
                            //Optical power (>15mW) control kd
                            if (!This.EthernetController.GetAllOpticalPowerGreaterThan15mWKd(LaserChannels.ChannelB))
                            {
                                This.NewParameterVM.OtherSettingBtnIsEnable = true;
                                StopWaitAnimation();
                                _IsLoading = false;
                                Application.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    Window window = new Window();
                                    MessageBox.Show(window, "GetAllOpticalPowerGreaterThan15mWKd failed.");
                                });
                                return;
                            }
                            Thread.Sleep(sleeptime);
                            //激光器最大电流
                            //Laser maximum current
                            if (!This.EthernetController.GetAllLaserMaximumCurrent(LaserChannels.ChannelB))
                            {
                                This.NewParameterVM.OtherSettingBtnIsEnable = true;
                                StopWaitAnimation();
                                _IsLoading = false;
                                Application.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    Window window = new Window();
                                    MessageBox.Show(window, "GetAllLaserMaximumCurrent failed.");
                                });
                                return;
                            }
                            Thread.Sleep(sleeptime);
                            //激光器最小电流
                            //Laser minimum current
                            if (!This.EthernetController.GetAllLaserMinimumCurrent(LaserChannels.ChannelB))
                            {
                                This.NewParameterVM.OtherSettingBtnIsEnable = true;
                                StopWaitAnimation();
                                _IsLoading = false;
                                Application.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    Window window = new Window();
                                    MessageBox.Show(window, "GetAllLaserMinimumCurrent failed.");
                                });
                                return;
                            }

                        }
                        Thread.Sleep(sleeptime);
                    }

                }
                if (LaserModuleL1.LaserWavelength == 0 && LaserModuleR1.LaserWavelength == 0 && LaserModuleR2.LaserWavelength == 0)
                {
                    Thread.Sleep(3000);
                }
            };
            worker.RunWorkerCompleted += (o, ea) =>
            {
                // Work has completed. You can now interact with the UI
                StopWaitAnimation();
                _IsLoading = false;

                if (LaserModuleL1.LaserWavelength != 0 && LaserModuleL1.LaserWavelength != Workspace.This.NewParameterVM.Uint16Code) //  Determine if the module wavelength exists
                {
                    This.NewParameterVM.LIVFirmwareVersionSN = EthernetController.IVOpticalModuleSerialNumber[IVChannels.ChannelC];
                    This.NewParameterVM.LIVFirmwareSN = EthernetController.IVEstimatedVersionNumberBoard[IVChannels.ChannelC];
                    This.NewParameterVM.LTEControlTemperature = EthernetController.TECControlTemperature[LaserChannels.ChannelC];
                    This.NewParameterVM.LTECMaximumCoolingCurrent = EthernetController.TECMaximumCurrent[LaserChannels.ChannelC];
                    This.NewParameterVM.LTECControlKp = EthernetController.TECRefrigerationControlParameterKp[LaserChannels.ChannelC];
                    This.NewParameterVM.LTECControlKi = EthernetController.TECRefrigerationControlParameterKi[LaserChannels.ChannelC];
                    This.NewParameterVM.LTECControlKd = EthernetController.TECRefrigerationControlParameterKd[LaserChannels.ChannelC];

                    if (LaserModuleL1.LaserWavelength == 532)
                    {
                        This.NewParameterVM.LOpticalPowerLessThanOrEqual15mWKp = EthernetController.AllOpticalPowerLessThanOrEqual15mWKp[LaserChannels.ChannelC];
                        This.NewParameterVM.LOpticalPowerLessThanOrEqual15mWKi = EthernetController.AllOpticalPowerLessThanOrEqual15mWKi[LaserChannels.ChannelC];
                        This.NewParameterVM.LOpticalPowerLessThanOrEqual15mWKd = EthernetController.AllOpticalPowerLessThanOrEqual15mWKd[LaserChannels.ChannelC];

                        This.NewParameterVM.LOpticalPowerGreaterThan15mWKp = EthernetController.AllOpticalPowerGreaterThan15mWKp[LaserChannels.ChannelC];
                        This.NewParameterVM.LOpticalPowerGreaterThan15mWKi = EthernetController.AllOpticalPowerGreaterThan15mWKi[LaserChannels.ChannelC];
                        This.NewParameterVM.LOpticalPowerGreaterThan15mWKd = EthernetController.AllOpticalPowerGreaterThan15mWKd[LaserChannels.ChannelC];

                        This.NewParameterVM.LLaserMaxCurrent = EthernetController.AllGetLaserMaximumCurrent[LaserChannels.ChannelC];
                        This.NewParameterVM.LLaserMinCurrent = EthernetController.AllGetLaserMinimumCurrent[LaserChannels.ChannelC];
                    }
                }
                if (LaserModuleR1.LaserWavelength != 0 && LaserModuleR1.LaserWavelength != Workspace.This.NewParameterVM.Uint16Code)
                {
                    This.NewParameterVM.R1IVFirmwareVersionSN = EthernetController.IVOpticalModuleSerialNumber[IVChannels.ChannelA];
                    This.NewParameterVM.R1IVFirmwareSN = EthernetController.IVEstimatedVersionNumberBoard[IVChannels.ChannelA];
                    This.NewParameterVM.R1TEControlTemperature = EthernetController.TECControlTemperature[LaserChannels.ChannelA];
                    This.NewParameterVM.R1TECMaximumCoolingCurrent = EthernetController.TECMaximumCurrent[LaserChannels.ChannelA];
                    This.NewParameterVM.R1TECControlKp = EthernetController.TECRefrigerationControlParameterKp[LaserChannels.ChannelA];
                    This.NewParameterVM.R1TECControlKi = EthernetController.TECRefrigerationControlParameterKi[LaserChannels.ChannelA];
                    This.NewParameterVM.R1TECControlKd = EthernetController.TECRefrigerationControlParameterKd[LaserChannels.ChannelA];
                    if (LaserModuleR1.LaserWavelength == 532)
                    {
                        This.NewParameterVM.R1OpticalPowerLessThanOrEqual15mWKp = EthernetController.AllOpticalPowerLessThanOrEqual15mWKp[LaserChannels.ChannelA];
                        This.NewParameterVM.R1OpticalPowerLessThanOrEqual15mWKi = EthernetController.AllOpticalPowerLessThanOrEqual15mWKi[LaserChannels.ChannelA];
                        This.NewParameterVM.R1OpticalPowerLessThanOrEqual15mWKd = EthernetController.AllOpticalPowerLessThanOrEqual15mWKd[LaserChannels.ChannelA];

                        This.NewParameterVM.R1OpticalPowerGreaterThan15mWKp = EthernetController.AllOpticalPowerGreaterThan15mWKp[LaserChannels.ChannelA];
                        This.NewParameterVM.R1OpticalPowerGreaterThan15mWKi = EthernetController.AllOpticalPowerGreaterThan15mWKi[LaserChannels.ChannelA];
                        This.NewParameterVM.R1OpticalPowerGreaterThan15mWKd = EthernetController.AllOpticalPowerGreaterThan15mWKd[LaserChannels.ChannelA];

                        This.NewParameterVM.R1LaserMaxCurrent = EthernetController.AllGetLaserMaximumCurrent[LaserChannels.ChannelA];
                        This.NewParameterVM.R1LaserMinCurrent = EthernetController.AllGetLaserMinimumCurrent[LaserChannels.ChannelA];

                    }
                }

                if (LaserModuleR2.LaserWavelength != 0 && LaserModuleR2.LaserWavelength != Workspace.This.NewParameterVM.Uint16Code)
                {
                    This.NewParameterVM.R2IVFirmwareVersionSN = EthernetController.IVOpticalModuleSerialNumber[IVChannels.ChannelB];
                    This.NewParameterVM.R2IVFirmwareSN = EthernetController.IVEstimatedVersionNumberBoard[IVChannels.ChannelB];
                    This.NewParameterVM.R2TEControlTemperature = EthernetController.TECControlTemperature[LaserChannels.ChannelB];
                    This.NewParameterVM.R2TECMaximumCoolingCurrent = EthernetController.TECMaximumCurrent[LaserChannels.ChannelB];
                    This.NewParameterVM.R2TECControlKp = EthernetController.TECRefrigerationControlParameterKp[LaserChannels.ChannelB];
                    This.NewParameterVM.R2TECControlKi = EthernetController.TECRefrigerationControlParameterKi[LaserChannels.ChannelB];
                    This.NewParameterVM.R2TECControlKd = EthernetController.TECRefrigerationControlParameterKd[LaserChannels.ChannelB];

                    if (LaserModuleR2.LaserWavelength == 532)
                    {
                        This.NewParameterVM.R2OpticalPowerLessThanOrEqual15mWKp = EthernetController.AllOpticalPowerLessThanOrEqual15mWKp[LaserChannels.ChannelB];
                        This.NewParameterVM.R2OpticalPowerLessThanOrEqual15mWKi = EthernetController.AllOpticalPowerLessThanOrEqual15mWKi[LaserChannels.ChannelB];
                        This.NewParameterVM.R2OpticalPowerLessThanOrEqual15mWKd = EthernetController.AllOpticalPowerLessThanOrEqual15mWKd[LaserChannels.ChannelB];

                        This.NewParameterVM.R2OpticalPowerGreaterThan15mWKp = EthernetController.AllOpticalPowerGreaterThan15mWKp[LaserChannels.ChannelB];
                        This.NewParameterVM.R2OpticalPowerGreaterThan15mWKi = EthernetController.AllOpticalPowerGreaterThan15mWKi[LaserChannels.ChannelB];
                        This.NewParameterVM.R2OpticalPowerGreaterThan15mWKd = EthernetController.AllOpticalPowerGreaterThan15mWKd[LaserChannels.ChannelB];

                        This.NewParameterVM.R2LaserMaxCurrent = EthernetController.AllGetLaserMaximumCurrent[LaserChannels.ChannelB];
                        This.NewParameterVM.R2LaserMinCurrent = EthernetController.AllGetLaserMinimumCurrent[LaserChannels.ChannelB];

                    }
                }
                This.NewParameterVM.OtherSettingBtnIsEnable = true;
            };

            worker.RunWorkerAsync();

        }*/

        #endregion

        /*#region Version Info

        public string SoftwareVersion
        {
            get { return string.Format("Software Version: {0}", _Owner.ProductVersion); }
        }

        public string CameraFirmware
        {
            get { return string.Format("Camera Firmware: {0}", CameraFWVersion); }
        }

        public string ControllerFirmware
        {
            get { return string.Format("Control Board Firmware: {0}", ControllerFWVersion); }
        }

        #endregion*/


        #region Debugging: CodeDebuggingCommand

        private RelayCommand _CodeDebuggingCommand = null;
        public ICommand CodeDebuggingCommand
        {
            get
            {
                if (_CodeDebuggingCommand == null)
                {
                    _CodeDebuggingCommand = new RelayCommand(ExecuteCodeDebuggingCommand, CanExecuteCodeDebuggingCommand);
                }

                return _CodeDebuggingCommand;
            }
        }
        protected void ExecuteCodeDebuggingCommand(object parameter)
        {
            //if (!IsActiveDocument) { return; }

            /*var refimage1 = LoadImage(@"M:\Test_Images\Sapphire-FL\EDR\20230410-1\EDRSmart_ChannelB.tif");
            var refimage2 = LoadImage(@"M:\Test_Images\Sapphire-FL\EDR\20230410-1\EDRSaturated_ChannelB.tif");
            int dynamicBit = 16;

            double scaleFactor = GetScaleFactor(refimage1, refimage2);

            System.Diagnostics.Trace.WriteLine("scale factor: " + scaleFactor.ToString());

            if (scaleFactor > 1)
            {
                ProcessEDRPixelAndCompress(ref refimage1, ref refimage2, ref dynamicBit, scaleFactor, scaleFactor);
                string title = Workspace.This.GetUniqueFilename("scaled_and_compressed");
                ImageInfo imgInfo = new ImageInfo();
                imgInfo.DynamicBit = dynamicBit;
                NewDocument(refimage2, imgInfo, title, false, false);
            }
            else
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Error calculating the scale factor");
            }*/

            ImageAlignParam alignParam = new ImageAlignParam();
            alignParam.Resolution = 20;
            alignParam.PixelOddX = SettingsManager.ConfigSettings.XOddNumberedLine;   //X odd Line
            alignParam.PixelEvenX = SettingsManager.ConfigSettings.XEvenNumberedLine; //X even Line
            alignParam.PixelOddY = SettingsManager.ConfigSettings.YOddNumberedLine;   //Y odd Line
            alignParam.PixelEvenY = SettingsManager.ConfigSettings.YEvenNumberedLine; //Y Even Line
            alignParam.YCompOffset = SettingsManager.ConfigSettings.YCompenOffset;
            alignParam.OpticalL_R1Distance = 48;
            alignParam.Pixel_10_L_DX = -46;
            alignParam.Pixel_10_L_DY = -14;
            alignParam.OpticalR2_R1Distance = 24;
            alignParam.Pixel_10_R2_DX = 1;
            alignParam.Pixel_10_R2_DY = 2;
            alignParam.IsImageOffsetProcessing = SettingsManager.ConfigSettings.ImageOffsetProcessing;
            alignParam.IsPixelOffsetProcessing = SettingsManager.ConfigSettings.PixelOffsetProcessing;
            alignParam.PixelOffsetProcessingRes = SettingsManager.ConfigSettings.PixelOffsetProcessingRes;
            alignParam.IsYCompensationBitAt = SettingsManager.ConfigSettings.YCompenSationBitAt;
            alignParam.XMotionExtraMoveLength = SettingsManager.ConfigSettings.XMotionExtraMoveLength;
            alignParam.YMotionExtraMoveLength = SettingsManager.ConfigSettings.YMotionExtraMoveLength;
            // L1
            var alignedImageL1 = LoadImage(@"M:\Test_Images\Sapphire-FL\Alignment_Issue\23.04.27_11.53.48_638_RAW.tif");
            alignParam.LaserChannel = LaserChannels.ChannelC;
            //ImagingHelper.AlignImage(ref _ChannelCImage, alignParam);
            alignedImageL1 = ImagingHelper.SFLImageAlign(alignedImageL1, alignParam);
            NewDocument(alignedImageL1, new ImageInfo(), "23.04.27_11.53.48_638", false, false);
            // R1
            var alignedImageR1 = LoadImage(@"M:\Test_Images\Sapphire-FL\Alignment_Issue\23.04.27_11.53.48_488_RAW.tif");
            alignParam.LaserChannel = LaserChannels.ChannelA;
            //ImagingHelper.AlignImage(ref _ChannelCImage, alignParam);
            alignedImageR1 = ImagingHelper.SFLImageAlign(alignedImageR1, alignParam);
            NewDocument(alignedImageR1, new ImageInfo(), "23.04.27_11.53.48_488", false, false);
            // R2
            var alignedImageR2 = LoadImage(@"M:\Test_Images\Sapphire-FL\Alignment_Issue\23.04.27_11.53.48_532_RAW.tif");
            alignParam.LaserChannel = LaserChannels.ChannelB;
            //ImagingHelper.AlignImage(ref _ChannelCImage, alignParam);
            alignedImageR2 = ImagingHelper.SFLImageAlign(alignedImageR2, alignParam);
            NewDocument(alignedImageR2, new ImageInfo(), "23.04.27_11.53.48_532", false, false);
        }
        protected bool CanExecuteCodeDebuggingCommand(object parameter)
        {
            return true;
        }

        private double GetScaleFactor(WriteableBitmap wbmLowLevel, WriteableBitmap wbmHighLevel)
        {
            //System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();

            int width = wbmHighLevel.PixelWidth;
            int height = wbmHighLevel.PixelHeight;
            int stride = wbmHighLevel.BackBufferStride;
            int numDataPoints = 10;
            List<DataPoint>[] dataGroupHi = new List<DataPoint>[numDataPoints];

            uint nMin = 0, nMax = 0;
            int offsetX = (int)(width * 0.05);
            int offsetY = (int)(height * 0.05);
            ImageProcessing.MinMax(wbmHighLevel, new Rect(offsetX, offsetY, width - 2 * offsetX, height - 2 * offsetY), ref nMin, ref nMax);
            if (nMax > 60000) { nMax = 60000; }
            int rangeMax = (int)(nMax - nMin);
            int blockRange = (int)(rangeMax / numDataPoints);
            for (int i = 0; i < numDataPoints; i++)
            {
                //stopWatch.Start();

                dataGroupHi[i] = GetDataPoints(wbmHighLevel, (int)nMax - (i * blockRange) - blockRange, (int)nMax - (i * blockRange));

                //stopWatch.Stop();
                //Logger.LogMessage(string.Format("Group {0}: {1}", i, stopWatch.ElapsedMilliseconds));
            }

            ImageStatistics imageStats = new ImageStatistics();
            List<DataPoint>[] dataGroupLo = GetDataPoints(wbmLowLevel, dataGroupHi);
            List<RsquaredData>[] arrRsquaredData = new List<RsquaredData>[dataGroupHi.Length];
            double rsquared;
            double yintercept;
            double slopetemp;
            int dataSets = dataGroupHi.Length;
            int inclusiveStart = 0;
            int exclusiveEnd = dataSets;
            double rsquaredMax = 0;
            RsquaredData[] rsquaredDataHi = new RsquaredData[dataSets];
            RsquaredData[] rsquaredDataLo = new RsquaredData[dataSets];
            for (int i = 0; i < dataSets; i++)
            {
                var hiVals = dataGroupHi[i].Select(x => (double)x.Value).ToArray();
                var loVals = dataGroupLo[i].Select(x => (double)x.Value).ToArray();
                imageStats.LinearRegression(loVals, hiVals, 0, hiVals.Length, out rsquared, out yintercept, out slopetemp);
                rsquaredDataHi[i] = new RsquaredData(dataGroupHi[i], rsquared);
                rsquaredDataLo[i] = new RsquaredData(dataGroupLo[i], rsquared);
                if (rsquared > rsquaredMax)
                {
                    rsquaredMax = rsquared;
                }
                System.Diagnostics.Trace.WriteLine("rsquared max: " + rsquaredMax.ToString());
                //LogMessage("EDR: rsquared max: " + rsquaredMax.ToString());
            }

            try
            {
                var sortedRsquaredDataHi = rsquaredDataHi.OrderBy(o => o.RsquaredValue != null).ThenBy(o => o.RsquaredValue).ToArray();
                var sortedRsquaredDataLo = rsquaredDataLo.OrderBy(o => o.RsquaredValue != null).ThenBy(o => o.RsquaredValue).ToArray();
                dataGroupHi = sortedRsquaredDataHi.Select(x => x.DataPoints).ToArray();
                dataGroupLo = sortedRsquaredDataLo.Select(x => x.DataPoints).ToArray();
            }
            catch
            {
            }

            System.Diagnostics.Trace.WriteLine("inclusiveStart: " + inclusiveStart.ToString());
            //LogMessage("EDR: inclusiveStart: " + inclusiveStart.ToString());

            if (rsquaredMax > 0.80)
            {
                // Use the last 3 data sets
                inclusiveStart = dataGroupHi.Length - 3;
                exclusiveEnd = dataGroupHi.Length;
            }
            else
            {
                // Use the first 3 data sets
                inclusiveStart = 0;
                exclusiveEnd = 3;
            }

            System.Diagnostics.Trace.WriteLine("inclusiveStart value used: " + inclusiveStart.ToString());
            System.Diagnostics.Trace.WriteLine("exclusiveEnd value used: " + exclusiveEnd.ToString());
            //LogMessage("EDR: inclusiveStart value used: " + inclusiveStart.ToString());
            //LogMessage("EDR: exclusiveEnd value used: " + exclusiveEnd.ToString());

            List<double> hiLevelMeanList = GetMean(dataGroupHi, inclusiveStart, exclusiveEnd);
            List<double> loLevelMeanList = GetMean(dataGroupLo, inclusiveStart, exclusiveEnd);
            FilterMean(ref loLevelMeanList, ref hiLevelMeanList);
            double slope = Azure.ImagingSystem.LinearRegression.Slope(loLevelMeanList.ToArray(), hiLevelMeanList.ToArray());
            return slope;
        }
        internal unsafe List<DataPoint> GetDataPoints(WriteableBitmap img, int lower, int upper)
        {
            List<DataPoint> result = new List<DataPoint>();
            int width = img.PixelWidth;
            int height = img.PixelHeight;
            int stride = img.BackBufferStride;
            byte* pData = (byte*)img.BackBuffer.ToPointer();
            ushort* pData16;
            int offsetX = (int)(width * 0.10);
            int offsetY = (int)(height * 0.10);

            for (int iRow = offsetY; iRow < height - offsetY; iRow++)
            {
                pData16 = (ushort*)(pData + (iRow * stride));
                for (int iCol = offsetX; iCol < width - offsetX; iCol++)
                {
                    if (*(pData16 + iCol) < upper && *(pData16 + iCol) > lower)
                    {
                        result.Add(new DataPoint(new Point(iCol, iRow), *(pData16 + iCol)));
                        if (result.Count == 20)
                        {
                            break;
                        }
                    }
                }
                if (result.Count == 20)
                {
                    break;
                }
            }
            return result;
        }
        internal unsafe List<DataPoint>[] GetDataPoints(WriteableBitmap img, List<DataPoint>[] dataPoints)
        {
            int width = img.PixelWidth;
            int height = img.PixelHeight;
            int stride = img.BackBufferStride;
            byte* pData = (byte*)img.BackBuffer.ToPointer();
            ushort* pData16;
            int iRow = 0;
            int iCol = 0;
            int dataCount = 0;

            List<DataPoint>[] newDataPoints = new List<DataPoint>[dataPoints.Length];
            int groupCount = dataPoints.Length;

            for (int i = 0; i < groupCount; i++)
            {
                newDataPoints[i] = new List<DataPoint>();
                dataCount = dataPoints[i].Count;
                for (int j = 0; j < dataCount; j++)
                {
                    iRow = (int)dataPoints[i][j].Point.Y;
                    iCol = (int)dataPoints[i][j].Point.X;
                    pData16 = (ushort*)(pData + (iRow * stride));
                    newDataPoints[i].Add(new DataPoint(new Point(iCol, iRow), *(pData16 + iCol)));
                }
            }
            return newDataPoints;
        }
        internal List<double> GetMean(List<DataPoint>[] dataGroup, int inclusiveStart, int exclusiveEnd)
        {
            List<double> meanList = new List<double>();

            int groupCount = dataGroup.Length;
            for (int i = inclusiveStart; i < exclusiveEnd; i++)
            {
                double dMean = 0;
                double dTotal = 0;
                int dataCount = dataGroup[i].Count;
                for (int j = 0; j < dataCount; j++)
                {
                    dTotal += dataGroup[i][j].Value;
                }
                dMean = dTotal / dataCount;
                meanList.Add(dMean);
            }
            return meanList;
        }
        internal unsafe List<double> GetMean(WriteableBitmap img, List<DataPoint>[] dataGroup)
        {
            int width = img.PixelWidth;
            int height = img.PixelHeight;
            int stride = img.BackBufferStride;
            byte* pData = (byte*)img.BackBuffer.ToPointer();
            ushort* pData16;
            int iRow = 0;
            int iCol = 0;
            double dTotal = 0;
            double dMean = 0;
            int dataCount = 0;
            List<double> meanList = new List<double>();

            int groupCount = dataGroup.Length;
            for (int i = 0; i < groupCount; i++)
            {
                dMean = 0;
                dTotal = 0;
                dataCount = dataGroup[i].Count;
                for (int j = 0; j < dataCount; j++)
                {
                    iRow = (int)dataGroup[i][j].Point.Y;
                    iCol = (int)dataGroup[i][j].Point.X;
                    pData16 = (ushort*)(pData + (iRow * stride));
                    dTotal += *(pData16 + iCol);
                }
                dMean = dTotal / dataCount;
                meanList.Add(dMean);
            }
            return meanList;
        }
        private void FilterMean(ref List<double> meanLow, ref List<double> meanHigh)
        {
            const int threshold = 55000;
            if (meanLow != null && meanHigh != null && meanLow.Count > 0 && meanHigh.Count > 0)
            {
                for (int i = meanHigh.Count - 1; i >= 0; i--)
                {
                    if (meanHigh[i] > threshold || (Double.IsNaN(meanHigh[i]) || Double.IsInfinity(meanHigh[i])))
                    {
                        meanLow.Remove(meanLow[i]);
                        meanHigh.Remove(meanHigh[i]);
                    }
                }
            }
        }
        private unsafe void ProcessEDRPixelAndCompress(ref WriteableBitmap lowSignalFrame, ref WriteableBitmap highSignalFrame, ref int dynamicBits, double scaleFactor, double compressFactor)
        {
            int width = 0;
            int height = 0;
            int bufferWidth = 0;

            dynamicBits = (int)Math.Ceiling((Math.Log10(60000 * compressFactor) / Math.Log10(2)));
            double compressCoeff = 16.0 / (double)dynamicBits;

            if (lowSignalFrame != null && highSignalFrame != null)
            {
                width = lowSignalFrame.PixelWidth;
                height = highSignalFrame.PixelHeight;
                bufferWidth = lowSignalFrame.BackBufferStride;
                byte* pImgBufferLow = (byte*)lowSignalFrame.BackBuffer.ToPointer();
                byte* pImgBufferHigh = (byte*)highSignalFrame.BackBuffer.ToPointer();

                ushort* pLowSignal16;
                ushort* pHighSignal16;
                double dTempLoVal = 0;
                int nTempVal = 0;
                for (int iRow = 0; iRow < height; iRow++)
                {
                    pLowSignal16 = (ushort*)(pImgBufferLow + (iRow * bufferWidth));
                    pHighSignal16 = (ushort*)(pImgBufferHigh + (iRow * bufferWidth));
                    for (int iCol = 0; iCol < width; iCol++)
                    {
                        if (*(pHighSignal16 + iCol) > 60000)
                        {
                            dTempLoVal = *(pLowSignal16 + iCol);
                            dTempLoVal *= scaleFactor;
                            nTempVal = (int)Math.Pow(dTempLoVal, compressCoeff);
                            // Just in case there's an overflow
                            // If low level frame (smart-scan image) is saturated (or above 60000), we're going to have an overflow
                            if (nTempVal > 65535)
                            {
                                nTempVal = 65535;
                            }
                            *(pHighSignal16 + iCol) = (ushort)nTempVal;
                        }
                        else
                        {
                            dTempLoVal = *(pHighSignal16 + iCol);
                            *(pHighSignal16 + iCol) = (ushort)Math.Pow(dTempLoVal, compressCoeff);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Fits a line to a collection of (x,y) points.
        /// </summary>
        /// <param name="xVals">The x-axis values.</param>
        /// <param name="yVals">The y-axis values.</param>
        /// <param name="inclusiveStart">The inclusive inclusiveStart index.</param>
        /// <param name="exclusiveEnd">The exclusive exclusiveEnd index.</param>
        /// <param name="rsquared">The r^2 value of the line.</param>
        /// <param name="yintercept">The y-intercept value of the line (i.e. y = ax + b, yintercept is b).</param>
        /// <param name="slope">The slop of the line (i.e. y = ax + b, slope is a).</param>
        /*public static void LinearRegression(double[] xVals, double[] yVals,
                                      int inclusiveStart, int exclusiveEnd,
                                      out double rsquared, out double yintercept,
                                      out double slope)
        {
            System.Diagnostics.Debug.Assert(xVals.Length == yVals.Length);
            double sumOfX = 0;
            double sumOfY = 0;
            double sumOfXSq = 0;
            double sumOfYSq = 0;
            double ssX = 0;
            double ssY = 0;
            double sumCodeviates = 0;
            double sCo = 0;
            double count = exclusiveEnd - inclusiveStart;

            for (int ctr = inclusiveStart; ctr < exclusiveEnd; ctr++)
            {
                double x = xVals[ctr];
                double y = yVals[ctr];
                sumCodeviates += x * y;
                sumOfX += x;
                sumOfY += y;
                sumOfXSq += x * x;
                sumOfYSq += y * y;
            }
            ssX = sumOfXSq - ((sumOfX * sumOfX) / count);
            ssY = sumOfYSq - ((sumOfY * sumOfY) / count);
            double RNumerator = (count * sumCodeviates) - (sumOfX * sumOfY);
            double RDenom = (count * sumOfXSq - (sumOfX * sumOfX))
             * (count * sumOfYSq - (sumOfY * sumOfY));
            sCo = sumCodeviates - ((sumOfX * sumOfY) / count);

            double meanX = sumOfX / count;
            double meanY = sumOfY / count;
            double dblR = RNumerator / Math.Sqrt(RDenom);
            rsquared = dblR * dblR;
            yintercept = meanY - ((sCo / ssX) * meanX);
            slope = sCo / ssX;
        }*/


        #region Edit Dynamic Bit
        private bool _IsEditDynamicBitAllowed = false;
        public bool IsEditDynamicBitAllowed
        {
            get
            {
                return _IsEditDynamicBitAllowed;
            }
            set
            {
                if (_IsEditDynamicBitAllowed != value)
                {
                    _IsEditDynamicBitAllowed = value;
                    RaisePropertyChanged("IsEditDynamicBitAllowed");
                    RaisePropertyChanged("DynamicBit");
                }
            }
        }
        public int DynamicBit
        {
            get
            {
                int dynamicBit = 0;
                if (ActiveDocument != null && ActiveDocument.ImageInfo != null)
                {
                    dynamicBit = ActiveDocument.ImageInfo.DynamicBit;
                }
                return dynamicBit;
            }
            set
            {
                if (ActiveDocument != null && ActiveDocument.ImageInfo != null && ActiveDocument.ImageInfo.DynamicBit != value)
                {
                    ActiveDocument.ImageInfo.DynamicBit = value;
                    RaisePropertyChanged("DynamicBit");
                    ActiveDocument.IsDirty = true;
                    IsEditDynamicBitAllowed = false; // hide the textbox
                }
            }

        }

        private RelayCommand _UpdateTextBoxBindingOnEnterCommand = null;
        public ICommand UpdateTextBoxBindingOnEnterCommand
        {
            get
            {
                if (_UpdateTextBoxBindingOnEnterCommand == null)
                {
                    _UpdateTextBoxBindingOnEnterCommand = new RelayCommand(ExecuteUpdateTextBoxBindingOnEnterCommand, CanExecuteUpdateTextBoxBindingOnEnterCommand);
                }

                return _UpdateTextBoxBindingOnEnterCommand;
            }
        }
        protected void ExecuteUpdateTextBoxBindingOnEnterCommand(object parameter)
        {
            if (!IsActiveDocument) { return; }
            TextBox tBox = parameter as TextBox;
            if (tBox != null)
            {
                DependencyProperty prop = TextBox.TextProperty;
                System.Windows.Data.BindingExpression binding = System.Windows.Data.BindingOperations.GetBindingExpression(tBox, prop);
                if (binding != null)
                    binding.UpdateSource();
            }
        }
        protected bool CanExecuteUpdateTextBoxBindingOnEnterCommand(object parameter)
        {
            return (ActiveDocument != null);
        }
        #endregion

        #endregion

    }

    #region Helper Class
    #endregion

}
