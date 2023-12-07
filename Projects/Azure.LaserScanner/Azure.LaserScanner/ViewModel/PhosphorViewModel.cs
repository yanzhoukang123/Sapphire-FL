using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;   //ObservableCollection
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;                   //Rect
using System.Windows.Input;             //ICommand
using System.Windows.Media;             //PixelFormats
using System.Windows.Media.Imaging;     //WriteableBitmap
using System.Xml;
using Azure.Common;
using Azure.WPF.Framework;
using Azure.Image.Processing;
using Azure.CommandLib;
using Azure.Configuration.Settings;
using Azure.EthernetCommLib;
using Azure.ImagingSystem;
using Azure.LaserScanner.View;
using Azure.Ipp.Imaging;
using CroppingImageLibrary.Services;

namespace Azure.LaserScanner.ViewModel
{
    class PhosphorViewModel : ViewModelBase
    {
        #region Private data...

        //private FileLocationViewModel _FileLocationVm = null;
        private ImagingViewModel _ImagingVm = null;
        private ObservableCollection<ProtocolViewModel> _AppProtocolOptions = new ObservableCollection<ProtocolViewModel>();
        private ProtocolViewModel _SelectedAppProtocol = null;
        private ObservableCollection<ResolutionType> _PixelSizeOptions = new ObservableCollection<ResolutionType>();
        private ObservableCollection<ScanSpeedType> _ScanSpeedOptions = new ObservableCollection<ScanSpeedType>();
        private ObservableCollection<SignalIntensity> _SignalLevelOptions = new ObservableCollection<SignalIntensity>();
        private const string _ProtocolConfigFile = "Protocols.xml";

        private double _XMotorSubdivision = 0;
        private double _YMotorSubdivision = 0;
        private double _ZMotorSubdivision = 0;

        private const int _ScanWidthInMm = 250;    // scan width in millimeters
        private const int _ScanHeightInMm = 250;   // scan height in millimters

        private int _Time = 0;
        private double _ScanX0 = 0;
        private double _ScanY0 = 0;
        private double _ScanZ0 = 0;
        private double _ScanDeltaX = 0;
        private double _ScanDeltaY = 0;
        private double _ScanDeltaZ = 0;

        private string _ScanTime = string.Empty;

        private int _RemainingTime = 0;
        private string _EstTimeRemaining = string.Empty;
        private double _PercentCompleted = 0.0;

        private RelayCommand _StartScanCommand = null;
        private RelayCommand _StopScanCommand = null;
        private LaserScanCommand _ImageScanCommand = null;
        private bool _IsSaveScanDataOnAborted = false;

        private Nullable<ScanType> _CurrentScanType = ScanType.Normal;

        private bool _IsPreviewChannels = false;
        private ContrastSettingsWindow _PreviewContrastWindow = null;
        private bool _IsUpdatingPreviewImage = false;
        private bool _IsAligningPreviewImage = false;
        private bool _IsPrescanCompleted = false;
        private static AutoResetEvent _ContrastThreadEvent = new AutoResetEvent(false);
        private bool _IsAbortedOnLidOpened = false;
        private bool _IsPreviewSetupCompleted = false;
        private bool _IsCropPreviewImage = false;
        private bool _IsScanRegionChanged = false;  //Did scan region changed while scanning?
        private bool _IsPreviewImageCleared = false;

        private WriteableBitmap _PreviewImage = null;
        private WriteableBitmap _ChannelL1PrevImage = null;
        private WriteableBitmap _ChannelR1PrevImage = null;
        private WriteableBitmap _ChannelR2PrevImage = null;
        private WriteableBitmap _ChannelL1PrevImageUnAligned = null;
        private WriteableBitmap _ChannelR1PrevImageUnAligned = null;
        private WriteableBitmap _ChannelR2PrevImageUnAligned = null;
        private ImageChannelType _LaserL1ColorChannel = ImageChannelType.None;
        private ImageChannelType _LaserR1ColorChannel = ImageChannelType.None;
        private ImageChannelType _LaserR2ColorChannel = ImageChannelType.None;

        #endregion

        #region Constructors...

        public PhosphorViewModel()
        {
            ProgressMin = 0.0;
            ProgressMax = 100.0;

            _XMotorSubdivision = SettingsManager.ConfigSettings.XMotorSubdivision;
            _YMotorSubdivision = SettingsManager.ConfigSettings.YMotorSubdivision;
            _ZMotorSubdivision = SettingsManager.ConfigSettings.ZMotorSubdivision;

            //_FileLocationVm = new FileLocationViewModel();
            //_FileLocationVm.DestinationFolder = SettingsManager.ApplicationSettings.InitialDirectory;

            _ImagingVm = new ImagingViewModel();
            _ImagingVm.ShowPreviewChannelsClicked += new ImagingViewModel.ShowPreviewChannelsClickedHandler(_ImagingVm_ShowPreviewChannelsClicked);
            _ImagingVm.UpdateDisplayImage += new ImagingViewModel.UpdateDisplayImagedHandler(_ImagingVm_UpdateDisplayImage);
            _ImagingVm.IsContrastChannelAllowed = false;
            _ImagingVm.ScanRegionChanged += ImagingVm_ScanRectChanged;
        }

        #endregion

        private void _ImagingVm_UpdateDisplayImage(object sender, ImageInfo imageInfo)
        {
            // Contrast the preview scanned image
            if (!Workspace.This.IsScanning)
            {
                if (_ChannelL1PrevImage != null && _IsPrescanCompleted)
                {
                    // Contrast the image....
                    UpdatePreviewDisplayImage();
                }
            }
        }

        private void _ImagingVm_ShowPreviewChannelsClicked(object sender)
        {
            IsPreviewChannels = !_IsPreviewChannels;
        }

        //public FileLocationViewModel FileLocationVm
        //{
        //    get { return _FileLocationVm; }
        //    set { _FileLocationVm = value; }
        //}

        public ImagingViewModel ImagingVm
        {
            get { return _ImagingVm; }
        }

        public ObservableCollection<ProtocolViewModel> AppProtocolOptions
        {
            get { return _AppProtocolOptions; }
            set
            {
                if (_AppProtocolOptions != value)
                {
                    _AppProtocolOptions = value;
                    RaisePropertyChanged("AppProtocolOptions");
                }
            }
        }

        public ProtocolViewModel SelectedAppProtocol
        {
            get { return _SelectedAppProtocol; }
            set
            {
                if (_SelectedAppProtocol != value)
                {
                    // Restore default the current selected protocol if modified not and not saved.
                    if (_SelectedAppProtocol != null)
                    {
                        // Protocol selection changed (restore default settings)
                        if (_SelectedAppProtocol.IsModified)
                        {
                            _SelectedAppProtocol.IsModified = false;
                        }
                    }

                    _SelectedAppProtocol = value;
                    RaisePropertyChanged("SelectedAppProtocol");

                    if (_SelectedAppProtocol != null)
                    {
                        if (_SelectedAppProtocol.SelectedScanRegion != null)
                        {
                            ScanTime = ImagingSystemHelper.FormatTime(_SelectedAppProtocol.SelectedScanRegion.GetScanTime());
                            _SelectedAppProtocol.SelectedScanRegion.FileLocationVm.FileSize = GetFileSize();

                            UpdateScanRegionPreviewChannels();
                        }
                    }
                }
            }
        }

        public bool IsPreviewChannels
        {
            get { return _IsPreviewChannels; }
            set
            {
                _IsPreviewChannels = value;
                RaisePropertyChanged("IsPreviewChannels");
                if (_IsPreviewChannels)
                {
                    if (_PreviewContrastWindow == null)
                    {
                        // Automatically show and select laser C channel
                        _ImagingVm.IsLaserL1PrvSelected = true;  //EL: TODO: Is Phosphor module restricted on L1
                        _ImagingVm.IsLaserL1PrvVisible = true;

                        // Display preview channels/contrast window
                        _PreviewContrastWindow = new ContrastSettingsWindow();
                        _PreviewContrastWindow.DataContext = Workspace.This.PhosphorVM;
                        _PreviewContrastWindow.Owner = Workspace.This.Owner;
                        //_PreviewContrastWindow.Topmost = true;
                        _PreviewContrastWindow.Show();
                    }
                }
                else
                {
                    if (_PreviewContrastWindow != null)
                    {
                        if (_PreviewContrastWindow.IsLoaded)
                        {
                            // Close preview channels/contrast window
                            _PreviewContrastWindow.Close();
                            _PreviewContrastWindow = null;
                        }
                    }
                }
            }
        }

        public int Time
        {
            get { return _Time; }
            set
            {
                if (_Time != value)
                {
                    _Time = value;
                }
            }
        }

        /// <summary>
        /// Selected scan region scan time
        /// </summary>
        public string ScanTime
        {
            get { return _ScanTime; }
            set
            {
                _ScanTime = value;
                RaisePropertyChanged("ScanTime");
                RaisePropertyChanged("TotalScanTime");
            }
        }

        /// <summary>
        /// Total scan time (if there's multiple scan regions)
        /// </summary>
        public string TotalScanTime
        {
            get
            {
                string strTotalScanTime = string.Empty;
                if (SelectedAppProtocol != null)
                {
                    if (SelectedAppProtocol.ScanRegions != null && SelectedAppProtocol.ScanRegions.Count > 1)
                    {
                        double dTotalScanTime = 0;
                        foreach (var scanRegion in SelectedAppProtocol.ScanRegions)
                        {
                            dTotalScanTime += scanRegion.GetScanTime();
                        }
                        strTotalScanTime = "[Total: " + ImagingSystemHelper.TotalScanTime(dTotalScanTime) + "]";
                    }
                }

                return strTotalScanTime;
            }
        }

        public double ScanX0
        {
            get
            {
                double scanX0 = 0.0;
                if (_XMotorSubdivision != 0)
                {
                    scanX0 = (_ScanX0 - (Workspace.This.MotorVM.AbsXPos * _XMotorSubdivision)) / (double)_XMotorSubdivision;
                }
                return scanX0;
            }
            set
            {
                _ScanX0 = Math.Round(value * _XMotorSubdivision + (Workspace.This.MotorVM.AbsXPos * _XMotorSubdivision));
            }
        }

        public double ScanY0
        {
            get
            {
                double scanY0 = 0.0;
                if (_YMotorSubdivision != 0)
                {
                    scanY0 = (_ScanY0 - (Workspace.This.MotorVM.AbsYPos * _YMotorSubdivision)) / _YMotorSubdivision;
                }
                return scanY0;
            }
            set
            {
                _ScanY0 = Math.Round(value * _YMotorSubdivision + (Workspace.This.MotorVM.AbsYPos * _YMotorSubdivision)); 
            }
        }

        public double ScanZ0
        {
            get { return _ScanZ0 / _ZMotorSubdivision; }
            set
            {
                if ((ScanZ0 / _ZMotorSubdivision) != value)
                {
                    _ScanZ0 = Math.Round(value * _ZMotorSubdivision);
                }
            }
        }

        public double ScanDeltaX
        {
            get
            {
                double scanDeltaX = 0.0;
                if (_XMotorSubdivision != 0)
                {
                    scanDeltaX = _ScanDeltaX / _XMotorSubdivision;
                }
                return scanDeltaX;
            }
            set
            {
                if ((_ScanDeltaX / _XMotorSubdivision) != value)
                {
                    _ScanDeltaX = Math.Round(value * _XMotorSubdivision);
                }
            }
        }

        public double ScanDeltaY
        {
            get
            {
                double scanDeltaY = 0.0;
                if (_YMotorSubdivision != 0)
                {
                    scanDeltaY = _ScanDeltaY / _YMotorSubdivision;
                }
                return scanDeltaY;
            }
            set
            {
                if ((_ScanDeltaY / _YMotorSubdivision) != value)
                {
                    _ScanDeltaY = Math.Round(value * _YMotorSubdivision);
                }
            }
        }

        public double ScanDeltaZ
        {
            get
            {
                double scanDeltaZ = 0.0;
                if (_ZMotorSubdivision != 0)
                {
                    scanDeltaZ = _ScanDeltaZ / _ZMotorSubdivision;
                }
                return scanDeltaZ;
            }
            set
            {
                if ((_ScanDeltaZ / _ZMotorSubdivision) != value)
                {
                    _ScanDeltaZ = Math.Round(value * _ZMotorSubdivision);
                }
            }
        }

        public int RemainingTime
        {
            get { return _RemainingTime; }
            set
            {
                if (_RemainingTime != value)
                {
                    _RemainingTime = value;
                    EstTimeRemaining = "Scan: " + ImagingSystemHelper.FormatTime(_RemainingTime);
                }
            }
        }

        public string EstTimeRemaining
        {
            get { return _EstTimeRemaining; }
            set
            {
                if (_EstTimeRemaining != value)
                {
                    _EstTimeRemaining = value;
                    RaisePropertyChanged("EstTimeRemaining");
                }
            }
        }

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

        public double ProgressMin { get; set; }
        public double ProgressMax { get; set; }

        #region public ICommand StartScanCommand

        public ICommand StartScanCommand
        {
            get
            {
                if (_StartScanCommand == null)
                {
                    _StartScanCommand = new RelayCommand(ExecuteStartScanCommand, CanExecuteStartScanCommand);
                }

                return _StartScanCommand;
            }
        }
        public void ExecuteStartScanCommand(object parameter)
        {
            if (_SelectedAppProtocol == null)
            {
                string caption = "No scan protocol selected...";
                string message = "No scan protocol selected\nPlease select a scan protocol and try again.";
                Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _CurrentScanType = (ScanType)parameter;
            if (_CurrentScanType == null)
            {
                _CurrentScanType = ScanType.Normal;
            }

            // Setting this flag here to avoid null reference on preview channels changed.
            _IsPrescanCompleted = false;

            bool bIsYCompensationEnabled = SettingsManager.ConfigSettings.YCompenSationBitAt;
            double dYCompensationOffset = SettingsManager.ConfigSettings.YCompenOffset;

            if (!SettingsManager.ConfigSettings.IsSimulationMode && Workspace.This.EthernetController.IsConnected)
            {
                var itemsFound = Workspace.This.LaserOptions.Where(item => item.SensorType == IvSensorType.PMT).ToList();
                if (itemsFound != null && itemsFound.Count > 0)
                {
                    if (itemsFound[0].LaserChannel != LaserChannels.ChannelC)
                    {
                        string caption = "Sapphire FL Biomolecular Imager";
                        string message = "Phosphor Imaging laser module is detected in L1 lasers slot.\nPlease make sure the Phosphor laser module is mounted on the left most slot (L1).";
                        Xceed.Wpf.Toolkit.MessageBox.Show(Workspace.This.Owner, message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                        return;
                    }
                }
                else
                {
                    string caption = "Sapphire FL Biomolecular Imager";
                    string message = "Phosphor Imaging laser module is not detected.\nPlease make sure the Phosphor laser module is mounted on the left most slot (L1).";
                    Xceed.Wpf.Toolkit.MessageBox.Show(Workspace.This.Owner, message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                    return;
                }
                // Read the scanner's parameters
                Workspace.This.NewParameterVM.ExecuteParametersReadCommand(null);
            }

            List<ScanParameterStruct> scanParams = new List<ScanParameterStruct>();

            // Validate each scan region selected signals settings
            foreach (var scanRegion in SelectedAppProtocol.ScanRegions)
            {
                if (scanRegion.SelectedPixelSize == null || scanRegion.SelectedScanSpeed == null || scanRegion.SelectedSignalLevel == null)
                {
                    string caption = "Incorrect scan parameters...";
                    string message = "Please make sure the scan parameters are correctly set up and try again.";
                    Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Don't allow duplicate dye/laser type or duplicate color channel
                //if (scanRegion.SignalList.Count > 1)
                //{
                //    List<string> colorChannels = new List<string>();
                //    List<LaserChannels> lasersChannels = new List<LaserChannels>();
                //    foreach (var signal in scanRegion.SignalList)
                //    {
                //        colorChannels.Add(signal.SelectedColorChannel.DisplayName);
                //        lasersChannels.Add(signal.SelectedLaser.LaserChannel);
                //    }
                //    // Find duplicate color channels
                //    var duplicatedColors = colorChannels.GroupBy(x => x)
                //          .Where(g => g.Count() > 1)
                //          .Select(y => y.Key)
                //          .ToList();
                //    if (duplicatedColors != null && duplicatedColors.Count > 0)
                //    {
                //        string caption = "Sapphire Biomolecular Imager";
                //        string message = string.Format("Duplicate color channels [{0}] in {1}.\nPlease make sure there's no duplicate imaging color channels.", duplicatedColors[0], scanRegion.ScanRegionHeader);
                //        Xceed.Wpf.Toolkit.MessageBox.Show(Workspace.This.Owner, message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                //        return;
                //    }
                //    // Find duplicate laser types
                //    var duplicatedLasers = lasersChannels.GroupBy(x => x)
                //      .Where(g => g.Count() > 1)
                //      .Select(y => y.Key)
                //      .ToList();
                //    if (duplicatedLasers != null && duplicatedLasers.Count > 0)
                //    {
                //        string caption = "Sapphire Biomolecular Imager";
                //
                //        string message = string.Format("Duplicate laser type selected [Laser: {0}] in {1}.\nPlease make sure there's no duplicate laser selected.", (int)duplicatedLasers[0], scanRegion.ScanRegionHeader);
                //        Xceed.Wpf.Toolkit.MessageBox.Show(Workspace.This.Owner, message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                //        return;
                //    }
                //}

                ScanParameterStruct scanParam = new ScanParameterStruct();

                int scanResolution = scanRegion.SelectedPixelSize.Value;
                int scanSpeed = scanRegion.SelectedScanSpeed.Value;
                int width = 0;
                int height = 0;
                // Preview scan
                //if (_CurrentScanType == ScanType.Preview)
                //{
                //    scanResolution = 1000;  // Preview scans at 1000 micron
                //    scanSpeed = 1;          // Preview scans at highest speed (1 = Highest)
                //    if (_CurrentScanType == ScanType.Preview && _SelectedAppProtocol.SelectedScanRegion.IsZScan)
                //    {
                //        _ScanZ0 = _SelectedAppProtocol.SelectedScanRegion.ZScanSetting.BottomImageFocus * _ZMotorSubdivision;
                //    }
                //}

                //if (scanRegion.IsCustomFocus)
                //{
                //    _ScanZ0 = (Workspace.This.MotorVM.AbsZPos - scanRegion.CustomFocusValue) * _ZMotorSubdivision;
                //}
                //else
                //{
                //    // Focus position of the selected sample type.
                //    // SampleType focus position now store focus position relative to the AbsFocusPosition (absolute focus position).
                //    _ScanZ0 = (Workspace.This.MotorVM.AbsZPos - scanRegion.SelectedSampleType.FocusPosition) * _ZMotorSubdivision;
                //}

                // Focus position of the selected sample type.
                //_ScanZ0 = (Workspace.This.MotorVM.AbsZPos - scanRegion.SelectedSampleType.FocusPosition) * _ZMotorSubdivision;
                // Use absolute focus position stored on the scanner for Phosphor imaging.
                _ScanZ0 = Workspace.This.MotorVM.AbsZPos * _ZMotorSubdivision;
                if (_ScanZ0 < 0 || _ScanZ0 > SettingsManager.ConfigSettings.ZMaxValue)
                {
                    if (_ScanZ0 < 0)
                        _ScanZ0 = Workspace.This.MotorVM.AbsZPos * _ZMotorSubdivision;
                    else if (_ScanZ0 * _ZMotorSubdivision > SettingsManager.ConfigSettings.ZMaxValue)
                        _ScanZ0 = SettingsManager.ConfigSettings.ZMaxValue;
                }

                // Set scan parameters
                //

                // Phosphor Imaging tab no longer has the scan Quality selection option
                // Unidirection or 2-line average scan
                bool bIsUnidirectionalScan = SettingsManager.ConfigSettings.PhosphorModuleProcessing;
                //Sawtooth correction
                bool bIsPixelOffsetProcessing = SettingsManager.ConfigSettings.PixelOffsetProcessing;
                if (bIsUnidirectionalScan)
                {
                    // Don't turn on sawtooth correction if 2-line average is ON
                    bIsPixelOffsetProcessing = false;
                }

                if (ImagingVm != null && ImagingVm.CellSize > 0)
                {
                    ScanDeltaX = (int)(_ScanWidthInMm * scanRegion.ScanRect.Width / (ImagingVm.CellSize * ImagingVm.NumOfCells));
                    ScanDeltaY = (int)(_ScanHeightInMm * scanRegion.ScanRect.Height / (ImagingVm.CellSize * ImagingVm.NumOfCells));
                    ScanX0 = (int)(_ScanWidthInMm * scanRegion.ScanRect.X / (ImagingVm.CellSize * ImagingVm.NumOfCells));
                    ScanY0 = (int)(_ScanHeightInMm * scanRegion.ScanRect.Y / (ImagingVm.CellSize * ImagingVm.NumOfCells));

                    // Add the overscan width and height
                    //_ScanDeltaX += (int)(SettingsManager.ConfigSettings.XMotionExtraMoveLength * _XMotorSubdivision);
                    //_ScanDeltaY += (int)(SettingsManager.ConfigSettings.YMotionExtraMoveLength * _YMotorSubdivision);

                    if (scanResolution > 0)
                    {
                        width = (int)(ScanDeltaX * 1000.0 / scanResolution);
                        height = (int)(ScanDeltaY * 1000.0 / scanResolution);
                        if (bIsUnidirectionalScan)
                        {
                            Time = height * scanRegion.SelectedScanSpeed.Value;
                        }
                        else
                        {
                            Time = height * scanRegion.SelectedScanSpeed.Value / 2;
                        }
                    }
                }

                //启用了Y轴补偿
                //Y-axis compensation is enabled
                if (bIsYCompensationEnabled && (int)((dYCompensationOffset * 1000) / scanResolution) >= 1)
                {
                    _ScanY0 = _ScanY0 - (int)(dYCompensationOffset * _YMotorSubdivision);
                    _ScanDeltaY = (int)_ScanDeltaY + (int)(dYCompensationOffset * _YMotorSubdivision);
                }

                // Scan 5um at 10um and at the speed of 2
                scanParam.Is5micronScan = false;
                if (scanResolution == 5)
                {
                    scanResolution = 10;
                    scanSpeed = 2;  // high (1 = highest)
                    scanParam.Is5micronScan = true;
                    width = (int)(ScanDeltaX * 1000.0 / scanResolution);
                    height = (int)(ScanDeltaY * 1000.0 / scanResolution);
                }

                if (bIsUnidirectionalScan)
                {
                    // Double the height on unidirectional scan to synchronize the scanner's LED progress bar
                    height *= 2;
                }

                scanParam.Width = width;
                scanParam.Height = height;
                scanParam.ScanDeltaX = (int)_ScanDeltaX;
                scanParam.ScanDeltaY = (int)_ScanDeltaY;
                scanParam.ScanDeltaZ = (int)_ScanDeltaZ;
                scanParam.ScanX0 = (int)_ScanX0;
                scanParam.ScanY0 = (int)_ScanY0;
                scanParam.ScanZ0 = (int)_ScanZ0;
                scanParam.AbsFocusPosition = Workspace.This.MotorVM.AbsZPos;
                scanParam.Res = scanResolution; // scan resolution in micron
                scanParam.Quality = scanSpeed;  // scan quality/speed
                scanParam.Time = Time;
                scanParam.XMotorSubdivision = _XMotorSubdivision;
                scanParam.YMotorSubdivision = _YMotorSubdivision;
                scanParam.ZMotorSubdivision = _ZMotorSubdivision;
                //EL: TODO:
                //scanParam.DataRate = DataRate;
                //scanParam.LineCounts = LineCounts;
                scanParam.DataRate = 25;
                scanParam.LineCounts = 1000;
                scanParam.XMotorSpeed = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[0].Speed * SettingsManager.ConfigSettings.XMotorSubdivision);
                scanParam.XMotionAccVal = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[0].Accel * SettingsManager.ConfigSettings.XMotorSubdivision);
                scanParam.XMotionDccVal = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[0].Dccel * SettingsManager.ConfigSettings.XMotorSubdivision);
                scanParam.YMotorSpeed = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[1].Speed * SettingsManager.ConfigSettings.YMotorSubdivision);
                scanParam.YMotionAccVal = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[1].Accel * SettingsManager.ConfigSettings.YMotorSubdivision);
                scanParam.YMotionDccVal = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[1].Dccel * SettingsManager.ConfigSettings.YMotorSubdivision);
                scanParam.ZMotorSpeed = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[2].Speed * SettingsManager.ConfigSettings.ZMotorSubdivision);
                scanParam.ZMotionAccVal = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[2].Accel * SettingsManager.ConfigSettings.ZMotorSubdivision);
                scanParam.ZMotionDccVal = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[2].Dccel * SettingsManager.ConfigSettings.ZMotorSubdivision);
                scanParam.IsNewFirmwire = Workspace.This.MotorVM.IsNewFirmware;
                scanParam.XmotionTurnAroundDelay = SettingsManager.ConfigSettings.XMotionTurnDelay;
                scanParam.XMotionExtraMoveLength = SettingsManager.ConfigSettings.XMotionExtraMoveLength;
                scanParam.LCoefficient = Workspace.This.NewParameterVM.LCoefficient;
                scanParam.L375Coefficient = Workspace.This.NewParameterVM.L375Coefficient;
                scanParam.R1Coefficient = Workspace.This.NewParameterVM.R1Coefficient;
                scanParam.R2Coefficient = Workspace.This.NewParameterVM.R2Coefficient;
                scanParam.R2532Coefficient = Workspace.This.NewParameterVM.R2532Coefficient;
                scanParam.IsIgnoreCompCoefficient = SettingsManager.ConfigSettings.IsIgnoreCompCoefficient;
                //scanParam.YMotionExtraMoveLength = SettingsManager.ConfigSettings.YMotionExtraMoveLength;
                //scanParam.DynamicBits = SelectedDynamicBits;
                //Grating ruler pulse
                scanParam.XEncoderSubdivision = Workspace.This.NewParameterVM.XEncoderSubdivision;
                if (SettingsManager.ConfigSettings.IsSimulationMode)
                {
                    scanParam.XEncoderSubdivision = SettingsManager.ConfigSettings.XEncoderSubdivision;
                }
                //scanParam.HorizontalCalibrationSpeed = HorizontalCalibrationSpeed;    //EL: TODO: do we need this value?
                //是否启用动态为补偿   Whether to enable dynamic as compensation
                scanParam.DynamicBitsAt = SettingsManager.ConfigSettings.ScanDynamicBitAt;
                //scanParam.IsUnidirectionalScan = SettingsManager.ConfigSettings.AllModuleProcessing;
                //scanParam.IsUnidirectionalScan = SettingsManager.ConfigSettings.IsFluorescence2LinesAvgScan;
                //scanParam.IsUnidirectionalScan = SettingsManager.ConfigSettings.PhosphorModuleProcessing;
                scanParam.IsUnidirectionalScan = bIsUnidirectionalScan;

                //（因为在设计中将R1透镜的位置和R2透镜的位置更换了，然后参数名没有更改成对应的通道，所以下面参数名R1其实是R2,R2其实是R1）....
                // (in the design, the position of R1 lens and R2 lens is changed, and the parameter name is not changed to the corresponding channel, so the following parameter name R1 is actually R2, R2 is actually R1)....
                //L透镜到R2透镜之间的距离（mm），毫米为单位
                //The distance between L lens and R2 lens (mm), in millimeters
                int OpticalL_R1Distance = (int)Workspace.This.NewParameterVM.OpticalL_R1Distance;
                if (SettingsManager.ConfigSettings.IsSimulationMode)
                {
                    OpticalL_R1Distance = 48;
                    Workspace.This.NewParameterVM.OpticalL_R1Distance = OpticalL_R1Distance;
                }
                //根据当前选择的分辨率计算L透镜到R2透镜之间的实际像素宽度
                //// Calculate the actual pixel width between L lens and R2 lens based on the currently selected resolution
                int OffsetWidth = (int)(OpticalL_R1Distance * 1000 / scanParam.Res);
                //扫描图像宽度加上补偿透镜之间距离的宽度（像素）
                // The width of the scanned image plus the width of the distance between the compensating lenses (pixels)
                int currentRangeWidth = scanParam.Width + OffsetWidth;
                int currentRangeHeight = scanParam.Height;
                scanParam.Width = currentRangeWidth;
                int offsetDeltaXPulse = 0;
                //将L到R2透镜之间的距离（mm）累加到ScanDeltaX上
                //Add the distance (mm) between L and R2 lens to ScanDeltaX
                //if (Workspace.This.NewParameterVM.OpticalL_R1Distance > 0 && scanParam.Res == 150)
                //{
                //    offsetDeltaXPulse = (int)Workspace.This.NewParameterVM.OpticalL_R1Distance - 1;
                //}
                //else
                //{
                //    offsetDeltaXPulse = (int)Workspace.This.NewParameterVM.OpticalL_R1Distance;
                //}
                offsetDeltaXPulse = (int)Workspace.This.NewParameterVM.OpticalL_R1Distance;
                offsetDeltaXPulse = (int)(offsetDeltaXPulse * _XMotorSubdivision);
                scanParam.ScanDeltaX += offsetDeltaXPulse;

                //scanParam.Width = (int)Math.Round(scanParam.ScanDeltaX / scanParam.XMotorSubdivision * 1000.0 / scanParam.Res);

                //TODO: temporary work-around: make sure the width is an even value to avoid the skewed on the scanned image
                if (scanParam.Width % 2 != 0)
                {
                    //int deltaX = scanParam.ScanDeltaX - (int)Math.Round(scanParam.Res / 1000.0 * scanParam.XMotorSubdivision);
                    //scanParam.ScanDeltaX = deltaX;
                    //scanParam.Width = (int)Math.Round(scanParam.ScanDeltaX / scanParam.XMotorSubdivision * 1000.0 / scanParam.Res);
                    scanParam.Width--;
                    scanParam.ScanDeltaX = (int)(scanParam.Width * scanParam.Res / 1000.0 * _XMotorSubdivision);
                }

                scanParam.AlignmentParam = new ImageAlignParam();
                scanParam.AlignmentParam.Resolution = scanParam.Res;
                scanParam.AlignmentParam.PixelOddX = SettingsManager.ConfigSettings.XOddNumberedLine;   //X odd Line
                scanParam.AlignmentParam.PixelEvenX = SettingsManager.ConfigSettings.XEvenNumberedLine; //X even Line
                scanParam.AlignmentParam.PixelOddY = SettingsManager.ConfigSettings.YOddNumberedLine;   //Y odd Line
                scanParam.AlignmentParam.PixelEvenY = SettingsManager.ConfigSettings.YEvenNumberedLine; //Y Even Line
                scanParam.AlignmentParam.YCompOffset = SettingsManager.ConfigSettings.YCompenOffset;
                scanParam.AlignmentParam.OpticalL_R1Distance = Workspace.This.NewParameterVM.OpticalL_R1Distance;
                scanParam.AlignmentParam.Pixel_10_L_DX = Workspace.This.NewParameterVM.Pixel_10_L_DX;
                scanParam.AlignmentParam.Pixel_10_L_DY = Workspace.This.NewParameterVM.Pixel_10_L_DY;
                scanParam.AlignmentParam.OpticalR2_R1Distance = Workspace.This.NewParameterVM.OpticalR2_R1Distance;
                scanParam.AlignmentParam.Pixel_10_R2_DX = Workspace.This.NewParameterVM.Pixel_10_R2_DX;
                scanParam.AlignmentParam.Pixel_10_R2_DY = Workspace.This.NewParameterVM.Pixel_10_R2_DY;
                scanParam.AlignmentParam.IsImageOffsetProcessing = SettingsManager.ConfigSettings.ImageOffsetProcessing;
                //scanParam.AlignmentParam.IsPixelOffsetProcessing = SettingsManager.ConfigSettings.PixelOffsetProcessing;
                scanParam.AlignmentParam.IsPixelOffsetProcessing = bIsPixelOffsetProcessing;
                scanParam.AlignmentParam.PixelOffsetProcessingRes = SettingsManager.ConfigSettings.PixelOffsetProcessingRes;
                scanParam.AlignmentParam.IsYCompensationBitAt = SettingsManager.ConfigSettings.YCompenSationBitAt;
                scanParam.AlignmentParam.XMotionExtraMoveLength = SettingsManager.ConfigSettings.XMotionExtraMoveLength;
                //scanParam.AlignmentParam.YMotionExtraMoveLength = SettingsManager.ConfigSettings.YMotionExtraMoveLength;

                // Motor speed settings
                //if (SettingsManager.ConfigSettings.MotorSettings != null)
                //{
                //    foreach (var motorSetting in SettingsManager.ConfigSettings.MotorSettings)
                //    {
                //        switch (motorSetting.MotorType)
                //        {
                //            case MotorType.X:
                //                scanParam.XMotorSpeed = (int)Math.Round(motorSetting.Speed * _XMotorSubdivision);
                //                scanParam.XMotionAccVal = (int)Math.Round(motorSetting.Accel * _XMotorSubdivision);
                //                scanParam.XMotionDccVal = (int)Math.Round(motorSetting.Dccel * _XMotorSubdivision);
                //                break;
                //            case MotorType.Y:
                //                scanParam.YMotorSpeed = (int)Math.Round(motorSetting.Speed * _YMotorSubdivision);
                //                scanParam.YMotionAccVal = (int)Math.Round(motorSetting.Accel * _YMotorSubdivision);
                //                scanParam.YMotionDccVal = (int)Math.Round(motorSetting.Dccel * _YMotorSubdivision);
                //                break;
                //            case MotorType.Z:
                //                scanParam.ZMotorSpeed = (int)Math.Round(motorSetting.Speed * _ZMotorSubdivision);
                //                scanParam.ZMotionAccVal = (int)Math.Round(motorSetting.Accel * _ZMotorSubdivision);
                //                scanParam.ZMotionDccVal = (int)Math.Round(motorSetting.Dccel * _ZMotorSubdivision);
                //                break;
                //        }
                //    }
                //}
                //scanParam.IsChannelALightShadeFix = SettingsManager.ConfigSettings.IsChannelALightShadeFix;
                //scanParam.IsChannelBLightShadeFix = SettingsManager.ConfigSettings.IsChannelBLightShadeFix;
                //scanParam.IsChannelCLightShadeFix = SettingsManager.ConfigSettings.IsChannelCLightShadeFix;
                //scanParam.IsChannelDLightShadeFix = SettingsManager.ConfigSettings.IsChannelDLightShadeFix;

                //if (_CurrentScanType == ScanType.Auto)
                //{
                //    scanParam.IsSmartScanning = true;
                //    scanParam.SmartScanResolution = SettingsManager.ConfigSettings.AutoScanSettings.Resolution;
                //    List<int> keyList = new List<int>(SettingsManager.ConfigSettings.LasersSignalList.Keys);
                //    scanParam.SmartScanSignalLevels = SettingsManager.ConfigSettings.LasersSignalList[keyList[0]].Count;
                //    scanParam.SmartScanFloor = SettingsManager.ConfigSettings.AutoScanSettings.Floor;
                //    scanParam.SmartScanCeiling = SettingsManager.ConfigSettings.AutoScanSettings.Ceiling;
                //    scanParam.SmartScanOptimalVal = SettingsManager.ConfigSettings.AutoScanSettings.OptimalVal;
                //    scanParam.SmartScanOptimalDelta = SettingsManager.ConfigSettings.AutoScanSettings.OptimalDelta;
                //    scanParam.SmartScanAlpha488 = SettingsManager.ConfigSettings.AutoScanSettings.Alpha488;
                //    scanParam.SmartScanInitSignalLevel = SettingsManager.ConfigSettings.AutoScanSettings.StartingSignalLevel;
                //    scanParam.SmartScanSignalStepdownLevel = SettingsManager.ConfigSettings.AutoScanSettings.HighSignalStepdownLevel;
                //    scanParam.LaserL1SignalOptions = SettingsManager.ConfigSettings.LasersSignalList[Workspace.This.LaserL1];
                //    scanParam.LaserR1SignalOptions = SettingsManager.ConfigSettings.LasersSignalList[Workspace.This.LaserR1];
                //    scanParam.LaserR2SignalOptions = SettingsManager.ConfigSettings.LasersSignalList[Workspace.This.LaserR2];
                //}

                #region === Get prefined signal settings ===

                List<Signal> scanRegionSignals = new List<Signal>();
                Signal scanSignal = null;
                if (SettingsManager.ConfigSettings.PhosphorSignalOptions != null && SettingsManager.ConfigSettings.PhosphorSignalOptions.Count > 0)
                {
                    scanSignal = SettingsManager.ConfigSettings.PhosphorSignalOptions[scanRegion.SelectedSignalLevel.IntensityLevel - 1];          // config file 1 index
                    scanSignal.ColorChannel = (int)ImageChannelType.Gray;
                    scanSignal.SignalLevel = scanRegion.SelectedSignalLevel.IntensityLevel;
                    scanRegionSignals.Add(scanSignal);
                }
                else
                {
                    string caption = "Scan signal...";
                    string message = "Scan signal is not properly setup.\nScannig will terminate.";
                    Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                    return;
                }


//                scanSignal.ColorChannel = (int)SelectedColorChannel.ImageColorChannel;
                //scanSignal.LaserWavelength = ImagingSystemHelper.GetLaserWaveLength(signal.SelectedDye.LaserType);
                //scanSignal.LaserChannel = ImagingSystemHelper.GetLaserChannel(Int32.Parse(scanSignal.LaserWavelength));
                // Get the laser channel corresponding to the wave length
                //scanSignal.LaserChannel = ImagingSystemHelper.GetLaserChannel(scanSignal.LaserWavelength);
//                scanSignal.LaserChannel = signal.SelectedLaser.LaserChannel;
//                scanSignal.LaserWavelength = signal.SelectedLaser.WaveLength;
//                scanSignal.SensorType = signal.SelectedLaser.SensorType;
                //if (scanSignal.LaserChannel == LaserChannels.ChannelA)      //R1
                //    scanSignal.LaserWavelength = Workspace.This.LaserR1.ToString();
                //else if (scanSignal.LaserChannel == LaserChannels.ChannelB) //R2
                //    scanSignal.LaserWavelength = Workspace.This.LaserR2.ToString();
                //else if (scanSignal.LaserChannel == LaserChannels.ChannelC) //L1
                //    scanSignal.LaserWavelength = Workspace.This.LaserL1.ToString();

                //scanRegionSignals.Add(scanSignal);



                // Signal validation
                /*if (scanRegion.SignalList.Count > 0)
                {
                    //Signal scanSignal = null;
                    foreach (var signal in scanRegion.SignalList)
                    {
                        // Check to make sure all the signal options are selected.
                        //
                        //if (signal.SelectedSignalLevel == null ||
                        //    signal.SelectedColorChannel == null)
                        //{
                        //    string caption = "Signal options error...";
                        //    string message = string.Empty;
                        //    if (signal.SelectedLaser == null)
                        //    {
                        //        message = string.Format("A dye not selected [{0}].\nPlease select a dye.", scanRegion.ScanRegionHeader);
                        //        Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                        //        return;
                        //    }
                        //    if (signal.SelectedSignalLevel == null)
                        //    {
                        //        message = string.Format("Intensity level not selected [{0}].\nPlease select an intensity.", scanRegion.ScanRegionHeader);
                        //        Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                        //        return;
                        //    }
                        //    if (signal.SelectedColorChannel == null)
                        //    {
                        //        message = string.Format("Color channel not selected [{0}].\nPlease select a color channel.", scanRegion.ScanRegionHeader);
                        //        Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                        //        return;
                        //    }
                        //}

                        // Laser module L1 = laser channel C, R1 = laser channel A, R2 = laser channel B
                        //Signal scanSignal = null;
                        //if (signal.SelectedLaser.LaserChannel == LaserChannels.ChannelC)
                        //{
                        //    //EL: TODO: setup lasers' signal lookup table
                        //    scanSignal = SettingsManager.ConfigSettings.LasersSignalList[Workspace.This.LaserL1][signal.SelectedSignalLevel.IntensityLevel - 1];  // config file 1 index
                        //}
                        //if (signal.SelectedLaser.LaserChannel == LaserChannels.ChannelA)
                        //{
                        //    //EL: TODO: setup lasers' signal lookup table
                        //    scanSignal = SettingsManager.ConfigSettings.LasersSignalList[Workspace.This.LaserR1][signal.SelectedSignalLevel.IntensityLevel - 1];  // config file 1 index
                        //}
                        //if (signal.SelectedLaser.LaserChannel == LaserChannels.ChannelB)
                        //{
                        //    //EL: TODO: setup lasers' signal lookup table
                        //    scanSignal = SettingsManager.ConfigSettings.LasersSignalList[Workspace.This.LaserR2][signal.SelectedSignalLevel.IntensityLevel - 1];  // config file 1 index
                        //}

                        Signal scanSignal = null;
                        if (SettingsManager.ConfigSettings.PhosphorSignalOptions != null && SettingsManager.ConfigSettings.PhosphorSignalOptions.Count > 0)
                        {
                            //IR700 = LaserC = 658nm
                            //Signal scanSignal = SettingsManager.ConfigSettings.PhosphorCSignalOptions[scanRegion.SelectedSignalLevel.IntensityLevel - 1];  // config file 1 index
                            //scanSignal.ColorChannel = (int)ImageChannelType.Gray;
                            //scanSignal.LaserWavelength = ImagingSystemHelper.GetLaserWaveLength(LaserType.LaserC);  //EL: TODO:
                            //signalOptions.Add(scanSignal);
                            scanSignal = SettingsManager.ConfigSettings.PhosphorSignalOptions[scanRegion.SelectedSignalLevel.IntensityLevel - 1];          // config file 1 index
                            scanSignal.ColorChannel = (int)ImageChannelType.Gray;
                            //scanSignal.LaserWavelength = ImagingSystemHelper.GetLaserWaveLength(LaserType.LaserD);  //EL: TODO:
                            //signalOptions.Add(scanSignal);
                        }
                        else
                        {
                            string caption = "Scan signal...";
                            string message = "Scan signal is not properly setup.\nScannig will terminate.";
                            Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                            return;
                        }


                        scanSignal.ColorChannel = (int)signal.SelectedColorChannel.ImageColorChannel;
                        //scanSignal.LaserWavelength = ImagingSystemHelper.GetLaserWaveLength(signal.SelectedDye.LaserType);
                        //scanSignal.LaserChannel = ImagingSystemHelper.GetLaserChannel(Int32.Parse(scanSignal.LaserWavelength));
                        // Get the laser channel corresponding to the wave length
                        //scanSignal.LaserChannel = ImagingSystemHelper.GetLaserChannel(scanSignal.LaserWavelength);
                        scanSignal.LaserChannel = signal.SelectedLaser.LaserChannel;
                        scanSignal.LaserWavelength = signal.SelectedLaser.WaveLength;
                        scanSignal.SensorType = signal.SelectedLaser.SensorType;
                        //if (scanSignal.LaserChannel == LaserChannels.ChannelA)      //R1
                        //    scanSignal.LaserWavelength = Workspace.This.LaserR1.ToString();
                        //else if (scanSignal.LaserChannel == LaserChannels.ChannelB) //R2
                        //    scanSignal.LaserWavelength = Workspace.This.LaserR2.ToString();
                        //else if (scanSignal.LaserChannel == LaserChannels.ChannelC) //L1
                        //    scanSignal.LaserWavelength = Workspace.This.LaserL1.ToString();

                        scanRegionSignals.Add(scanSignal);

                    }   //foreach scanRegion.SignalList
                }*/

                //_ScanChannelCount = scanRegionSignals.Count;

                //check laser status
                //if (IsLaserL1On == false && IsLaserR1On == false &&
                //    IsLaserR2On == false)
                //{
                //    string caption = "No laser selected...";
                //    string message = string.Format("No laser selected [{0}].\n Do you want to continue scanning?", scanRegion.ScanRegionHeader);
                //    if (Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.YesNo, MessageBoxImage.Information) != MessageBoxResult.Yes)
                //    {
                //        return;
                //    }
                //}

                #endregion


                //if (scanRegionSignals.Count > 1)
                //{
                //    scanParam.IsSequentialScanning = scanRegion.IsSequentialScanning;
                //    if (scanRegion.IsSequentialScanning && !scanRegion.IsZScan)
                //    {
                //        _ScanImageBaseFilename.Clear();
                //        _ScanImageBaseFilename.Add(Workspace.This.GenerateFileName(string.Empty, string.Empty));
                //    }
                //}
                //else
                //{
                //    // Scanning channels must be greater 1, otherwise turn OFF sequential scanning.
                //    scanRegion.IsSequentialScanning = false;
                //    scanParam.IsSequentialScanning = false;
                //}

                // Z-Scanning setup
                //if ((_CurrentScanType == ScanType.Normal || _CurrentScanType == ScanType.Auto) && scanRegion.IsZScan)
                //{
                //    if (scanRegion.IsZScan)
                //    {
                //        if (scanRegion.ZScanSetting.DeltaFocus <= 0 || scanRegion.ZScanSetting.NumOfImages < 2)
                //        {
                //            string caption = "Z-Scanning...";
                //            string message = string.Format("Z-Scan: The Focus Delta must be greater 0 [{0}].", scanRegion.ScanRegionHeader);
                //            if (scanRegion.ZScanSetting.NumOfImages < 2)
                //                message = string.Format("Z-Scan: The number of images must be 2 or greater [{0}].", scanRegion.ScanRegionHeader);
                //            Xceed.Wpf.Toolkit.MessageBox.Show(Workspace.This.Owner, message, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                //            return;
                //        }
                //    }
                //
                //    scanParam.IsZScanning = scanRegion.IsZScan;
                //    scanParam.BottomImageFocus = scanRegion.ZScanSetting.BottomImageFocus;
                //    scanParam.DeltaFocus = scanRegion.ZScanSetting.DeltaFocus;
                //    scanParam.NumOfImages = scanRegion.ZScanSetting.NumOfImages;
                //
                //    _ScanImageBaseFilename.Clear();
                //    if (scanRegion.IsSequentialScanning) // Z-Scanning + Sequential
                //    {
                //        // Common name for each set
                //        for (int i = 0; i < scanParam.NumOfImages; i++)
                //        {
                //            _ScanImageBaseFilename.Add(Workspace.This.GenerateFileName(string.Empty, string.Empty));
                //        }
                //    }
                //    else
                //    {
                //        _ScanImageBaseFilename.Add(Workspace.This.GenerateFileName(string.Empty, string.Empty));
                //    }
                //}
                //else
                //{
                //    // Number of images must be greater 1, otherwise turn OFF Z-scanning.
                //    scanParam.IsZScanning = false;
                //}

                scanParam.Signals = scanRegionSignals;
                scanParams.Add(scanParam);

            } // foreach scan regions

            // Setting this flag here to avoid null reference on preview channels changed.
            _IsPrescanCompleted = false;

            //
            // Clear previous preview image buffers
            //
            _PreviewImage = null;
            _ChannelL1PrevImage = null;
            _ChannelR1PrevImage = null;
            _ChannelR2PrevImage = null;
            _ChannelL1PrevImageUnAligned = null;
            _ChannelR1PrevImageUnAligned = null;
            _ChannelR2PrevImageUnAligned = null;

            if (this._ImagingVm != null && this._ImagingVm.PreviewImage != null)
            {
                this.ImagingVm.PreviewImage = null;     //clear preview image
            }

            if (this._ImagingVm != null && this._ImagingVm.PreviewImages != null)
            {
                _ImagingVm.PreviewImages.Clear();
            }

            _SelectedAppProtocol.SelectedScanRegion = _SelectedAppProtocol.ScanRegions[0];
            //if (_CurrentScanType != ScanType.Auto)
            //{
            //    PreviewImageSetup(scanParams[0], 0);
            //}

            #region === Auto save file location verification ===

            for (int index = 0; index < _SelectedAppProtocol.ScanRegions.Count; index++)
            {
                // Auto-save verification
                if (_SelectedAppProtocol.ScanRegions[index].FileLocationVm.IsAutoSave)
                {
                    if (string.IsNullOrEmpty(_SelectedAppProtocol.ScanRegions[index].FileLocationVm.DestinationFolder))
                    {
                        string strCaption = "File saving location...";
                        string strMessage = "Please specify the destination folder.";
                        Xceed.Wpf.Toolkit.MessageBox.Show(strMessage, strCaption, MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    else if (!System.IO.Directory.Exists(_SelectedAppProtocol.ScanRegions[index].FileLocationVm.DestinationFolder))
                    {
                        string strCaption = "File saving location...";
                        string strMessage = string.Format("The folder \"{0}\" does not exists\nPlease select another folder.",
                            _SelectedAppProtocol.ScanRegions[index].FileLocationVm.DestinationFolder);
                        Xceed.Wpf.Toolkit.MessageBox.Show(strMessage, strCaption, MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    else
                    {
                        //Check if file already exists
                        /*string fileName = _SelectedAppProtocol.ScanRegions[index].FileLocationVm.FileName.Trim();
                        string targetDirectory = _SelectedAppProtocol.ScanRegions[index].FileLocationVm.DestinationFolder;
                        if (!Workspace.This.CheckSupportedFileType(fileName))
                        {
                            fileName += ".tif";
                        }
                        string filePath = System.IO.Path.Combine(targetDirectory, fileName);
                        if (System.IO.File.Exists(filePath))
                        {
                            string strCaption = "File already exists...";
                            string strMessage = string.Format("The specified file \"{0}\" already exists in the selected folder [Scan Region #{1}]",
                                _SelectedAppProtocol.ScanRegions[index].FileLocationVm.FileName, _SelectedAppProtocol.ScanRegions[index].ScanRegionNum);
                            Xceed.Wpf.Toolkit.MessageBox.Show(strMessage, strCaption, MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }*/

                        string fileName = _SelectedAppProtocol.ScanRegions[index].FileLocationVm.FileName.Trim();
                        if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) > 0)
                        {
                            string strCaption = "Auto-save: Invalid file name...";
                            string strMessage = "Please make sure you've entered a valid file name.";
                            Xceed.Wpf.Toolkit.MessageBox.Show(strMessage, strCaption, MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        string targetDirectory = _SelectedAppProtocol.ScanRegions[index].FileLocationVm.DestinationFolder;
                        System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo(targetDirectory);
                        System.IO.FileInfo[] fileEntries = dirInfo.GetFiles();
                        foreach (System.IO.FileInfo fileInfo in fileEntries)
                        {
                            string fileNameWithoutExt = System.IO.Path.GetFileNameWithoutExtension(fileInfo.Name);
                            if (fileNameWithoutExt.StartsWith(fileName, StringComparison.OrdinalIgnoreCase))
                            {
                                string strCaption = "File already exists...";
                                string strMessage = string.Format("A file name starting with \"{0}\" already exists in the selected folder [Scan Region #{1}]",
                                    _SelectedAppProtocol.ScanRegions[index].FileLocationVm.FileName, _SelectedAppProtocol.ScanRegions[index].ScanRegionNum);
                                Xceed.Wpf.Toolkit.MessageBox.Show(strMessage, strCaption, MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                            }
                        }
                    }
                }
            }

            #endregion

            if (!SettingsManager.ConfigSettings.IsSimulationMode)
            {
                if (!Workspace.This.EthernetController.IsConnected)
                {
                    string caption = "Sapphire FL Biomolecular Imager";
                    string message = "Scanner is not connected.\nPlease make sure the system power is turned on, \nand the USB cable is securely connected.";
                    Xceed.Wpf.Toolkit.MessageBox.Show(Workspace.This.Owner, message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                    return;
                }
                if (!Workspace.This.MotorVM.IsMotorAlreadyHome)
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show("Please wait. Motor is being initialized.");
                    return;
                }

                //Turn off all the lasers before scanning just in case they were left on from previous scan.
                Workspace.This.TurnOffAllLasers();

                //EL: TODO: check if the stage is homed?
                // Is the scan head in the locked position?
                //if (Workspace.This.MotorVM.GalilMotor.IsScanheadLocked)
                //{
                //    string caption = "Sapphire Biomolecular Imager";
                //    string message = "The scan head is in the locked position.\n" +
                //                     "Please power cycle the Sapphire scanner then relaunch the application to unlock the scan head.";
                //    Xceed.Wpf.Toolkit.MessageBox.Show(Workspace.This.Owner, message, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                //    return;
                //}

                // TODO: sometimes not detecting lid opened; hang around and recheck
                // Is the scanner lid opened?
                if (Workspace.This.EthernetController.LidIsOpen)
                {
                    string caption = "Sapphire FL Biomolecular Imager";
                    string message = "The scanner lid is opened.\n" +
                                        "Please close the lid, then try scanning again.";
                    Xceed.Wpf.Toolkit.MessageBox.Show(Workspace.This.Owner, message, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            ImageProcessing.FlipAxis flipAxis = ImageProcessing.FlipAxis.None;
            if (SettingsManager.ApplicationSettings.IsHorizontalFlipEnabled)
            {
                flipAxis = ImageProcessing.FlipAxis.Horizontal;
            }
            else if (SettingsManager.ApplicationSettings.IsVerticalFlipEnabled)
            {
                flipAxis = ImageProcessing.FlipAxis.Vertical;
            }

            bool bIsSaveDebuggingImages = SettingsManager.ConfigSettings.IsSaveDebuggingImages;
            _ImageScanCommand = new LaserScanCommand(Application.Current.Dispatcher,
                                                     Workspace.This.EthernetController,
                                                     Workspace.This.MotorVM.MotionController,
                                                     scanParams,
                                                     true,
                                                     SettingsManager.ConfigSettings.IsApplyImageSmoothing,
                                                     Workspace.This.AppDataPath,
                                                     flipAxis,
                                                     bIsSaveDebuggingImages);
            _ImageScanCommand.Completed += ImageScanCommand_Completed;
            _ImageScanCommand.CommandStatus += ImageScanCommand_CommandStatus;
            //_ImageScanCommand.ReceiveTransfer += ImageScanCommand_ReceiveTransfer;  //EL: TODO:
            _ImageScanCommand.OnScanDataReceived += ImageScanCommand_OnScanDataReceived;    //EL: TODO:
            _ImageScanCommand.DataReceived += ImageScanCommand_DataReceived;        //EL: TODO:
            _ImageScanCommand.ScanRegionCompleted += ImageScanCommand_ScanRegionCompleted;
            _ImageScanCommand.IsSimulationMode = SettingsManager.ConfigSettings.IsSimulationMode;
            _ImageScanCommand.IsKeepRawImages = SettingsManager.ConfigSettings.IsKeepRawImages;

            if (SettingsManager.ConfigSettings.IsSimulationMode)
            {
                _ImageScanCommand.CompletionEstimate += ImageScanCommand_CompletionEstimate;
            }
            // SmartScan (formally 'AutoScan')
            //if (_CurrentScanType == ScanType.Auto)
            //{
            //    _IsAutoScanCompleted = false;
            //    _ImageScanCommand.SmartScanStarting += ImageScanCommand_SmartScanStarting;
            //    _ImageScanCommand.SmartScanUpdated += ImageScanCommand_SmartScanUpdated;
            //    _ImageScanCommand.SmartScanCompleted += ImageScanCommand_SmartScanCompleted;
            //}

            //foreach (var scanParam in scanParams)
            //{
            //    if (scanParam.IsZScanning)
            //    {
            //        _ImageScanCommand.ZScanningCompleted += ImageScanCommand_ZScanningCompleted;
            //        break;
            //    }
            //
            //}

            Workspace.This.IsScanning = true;

            if (!SettingsManager.ConfigSettings.IsSimulationMode)
                Workspace.This.IsPreparingToScan = true;

            _IsSaveScanDataOnAborted = false;
            _IsAbortedOnLidOpened = false;
            // Disable scan region adorner while scanning.
            //_ImagingVm.IsAdornerEnabled = false;

            PreviewImageSetup(scanParams[0], 0);

            if (SettingsManager.ConfigSettings.IsSimulationMode)
            {
                _ImageScanCommand.Start();
            }
            else
            {
                System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    Workspace.This.StatusTextProgress = "Scanning setup in progress...please wait....";

                    //EL: TODO: move the motor to position
                    //
                    //first,make the motor goes to start position
                    /*Workspace.This.MotorVM.GalilMotor.SendCommand("HX");
                    Workspace.This.MotorVM.GalilMotor.SendCommand("ST");
                    #region motor program one
                    string temp = "#SCAN\r" +
                                  "SHXYZ\r" +
                                  "STXYZ\r" +
                                  "SPX=50000\r" +
                                  "SPY=5000\r" +
                                  "SPZ=2000\r" +
                                  "PAB=" + (scanParams[0].ScanY0).ToString() + "\r" +
                                  "BGY\r" +
                                  "PAA=" + (scanParams[0].ScanX0).ToString() + "\r" +
                                  "BGX\r" +
                                  "PAC=" + (scanParams[0].ScanZ0).ToString() + "\r" +
                                  "BGZ\r" +
                                  "AMX\r" +
                                  "AMY\r" +
                                  "AMZ\r" +
                                  "EN";
                    #endregion motor program one
                    Workspace.This.MotorVM.GalilMotor.ProgramDownload(temp);
                    //waitting for the motor goes to the position
                    while (!(Workspace.This.MotorVM.GalilMotor.XCurrentP == scanParams[0].ScanX0 &&
                            Workspace.This.MotorVM.GalilMotor.YCurrentP == scanParams[0].ScanY0 &&
                            Workspace.This.MotorVM.GalilMotor.ZCurrentP == scanParams[0].ScanZ0))
                    {
                        Thread.Sleep(1);
                    }

                    while (!Workspace.This.ApdVM.IsPMTGainRecovered)
                    {
                        Thread.Sleep(1);
                    }*/

                    //if (_CurrentScanType == ScanType.Auto)
                    //{
                    //    Workspace.This.StatusTextProgress = "SMARTSCAN in progress....";
                    //}

                    // Start the image scanning thread
                    _ImageScanCommand.Start();
                });
            }

            //Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
            //{
            //    Workspace.This.CaptureDispatcherTimer.Start();
            //});
        }
        public bool CanExecuteStartScanCommand(object parameter)
        {
            return true;
        }

        #endregion

        private void ImageScanCommand_CommandStatus(object sender, string statusText)
        {
            Workspace.This.Owner.Dispatcher.BeginInvoke((Action)delegate
            {
                Workspace.This.StatusTextProgress = statusText;
            });
        }

        private void ImageScanCommand_CompletionEstimate(ThreadBase sender, DateTime dateTime, double estTime, double percentCompleted)
        {
            Workspace.This.Owner.Dispatcher.BeginInvoke((Action)delegate
            {
                this.Time = (int)estTime;
            });
        }

        private void ImageScanCommand_Completed(CommandLib.ThreadBase sender, CommandLib.ThreadBase.ThreadExitStat exitState)
        {
            Workspace.This.Owner.Dispatcher.BeginInvoke((Action)delegate
            {
                //Workspace.This.CaptureDispatcherTimer.Stop();
                Workspace.This.IsPreparingToScan = false;
                Workspace.This.IsReadyScanning = false;
                Workspace.This.IsScanning = false;
                //Reset status
                Time = 0;
                RemainingTime = 0;
                Workspace.This.StatusTextProgress = string.Empty;
                Workspace.This.PercentCompleted = 0;
                _ImagingVm.IsAdornerEnabled = true;
                _IsUpdatingPreviewImage = false;
                _IsAligningPreviewImage = false;

                _ChannelL1PrevImageUnAligned = null;
                _ChannelR1PrevImageUnAligned = null;
                _ChannelR2PrevImageUnAligned = null;

                ThreadBase scannedThread = (sender as LaserScanCommand);
                int nCurrScanRegion = ((LaserScanCommand)scannedThread).CurrentScanRegion;

                if (_IsAbortedOnLidOpened)
                {
                    IsPreviewChannels = false;  // close preview image contrast window.
                    string caption = "Lid open detected...";
                    string message = "Lid open detected. The scan was terminated.\n" +
                                     "Would you like to save your scanned data?";
                    var result = Xceed.Wpf.Toolkit.MessageBox.Show(Workspace.This.Owner, message, caption, MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Yes)
                    {
                        _IsSaveScanDataOnAborted = true;
                        _ImageScanCommand.IsSaveDataOnScanAbort = _IsSaveScanDataOnAborted;
                    }
                    else
                    {
                        _IsSaveScanDataOnAborted = false;
                        _ImageScanCommand.IsSaveDataOnScanAbort = _IsSaveScanDataOnAborted;
                    }
                }

                if (exitState == ThreadBase.ThreadExitStat.None)
                {
                    // Clear preview image(s)
                    //if (_ImagingVm.PreviewImages != null && _ImagingVm.PreviewImages.Count > 0)
                    //{
                    //    _ImagingVm.PreviewImages.Clear();
                    //}

                    // Switch to gallery tab
                    Workspace.This.SelectedApplicationTab = ApplicationTabType.Gallery;

                    // Generate new default file name
                    if (_SelectedAppProtocol != null && _SelectedAppProtocol.ScanRegions.Count > 0)
                    {
                        int nScanRegions = _SelectedAppProtocol.ScanRegions.Count;
                        string generatedFileName = Workspace.This.GenerateFileName(string.Empty, string.Empty);

                        foreach (var scanRegion in _SelectedAppProtocol.ScanRegions)
                        {
                            string fileName = scanRegion.FileLocationVm.FileName;
                            if (!string.IsNullOrEmpty(fileName))
                            {
                                if (Workspace.This.IsGeneratedFileName(fileName))
                                {
                                    if (nScanRegions > 1)
                                    {
                                        scanRegion.FileLocationVm.FileName = string.Format("{0}_SR{1}", generatedFileName, scanRegion.ScanRegionNum);
                                    }
                                    else
                                    {
                                        scanRegion.FileLocationVm.FileName = generatedFileName;
                                    }
                                }
                                else
                                {
                                    string fileExt = ".tif";
                                    if (!Workspace.This.CheckSupportedFileType(fileName))
                                    {
                                        fileName += fileExt;
                                    }
                                    // User's specified file name - add suffix to avoid duplicate file name.
                                    string directoryName = scanRegion.FileLocationVm.DestinationFolder;
                                    string filePath = System.IO.Path.Combine(directoryName, fileName);
                                    if (System.IO.File.Exists(filePath))
                                    {
                                        // Get a unique file name in the destination folder
                                        scanRegion.FileLocationVm.FileName = Workspace.This.GetUniqueFilenameInFolder(directoryName, fileName);
                                    }
                                }
                            }
                            else
                            {
                                if (nScanRegions > 1)
                                {
                                    scanRegion.FileLocationVm.FileName = string.Format("{0}_SR{1}", generatedFileName, scanRegion.ScanRegionNum);
                                }
                                else
                                {
                                    scanRegion.FileLocationVm.FileName = generatedFileName;
                                }
                            }
                        }
                    }
                }
                else if ((exitState == ThreadBase.ThreadExitStat.Abort && _IsSaveScanDataOnAborted) ||
                         (_IsAbortedOnLidOpened && _IsSaveScanDataOnAborted))
                {
                    #region === Scan Aborted (Save Data) ===

                    // Save data on abort
                    //    previously save the scan data here
                    //    now save in 'ImageScanCommand_ScanRegionCompleted' or 'ImageScanCommand_ZScanningCompleted' for Z-Scan)
                    //

                    if (scannedThread != null)
                    {
                        if (((LaserScanCommand)scannedThread).IsScanAborted && ((LaserScanCommand)scannedThread).IsSaveDataOnScanAbort)
                        {
                            ((LaserScanCommand)scannedThread).KeepDataOnAbort();
                        }
                    }

                    // Switch to gallery tab
                    Workspace.This.SelectedApplicationTab = ApplicationTabType.Gallery;

                    #endregion
                }
                else if (exitState == ThreadBase.ThreadExitStat.Error)
                {
                    // Oh oh something went wrong - handle the error

                    if (_PreviewContrastWindow != null && _PreviewContrastWindow.IsLoaded)
                    {
                        // Close preview channels/contrast window
                        _PreviewContrastWindow.Close();
                        _PreviewContrastWindow = null;
                    }

                    string message = scannedThread.Error.Message + "\n\nStack Trace:\n" + scannedThread.Error.StackTrace;
                    Workspace.This.LogMessage(message);

                    if (!_IsAbortedOnLidOpened)     // Lid opened already handled above
                    {
                        string caption = "Scanning error...";
                        var parent = Application.Current.MainWindow;
                        Xceed.Wpf.Toolkit.MessageBox.Show(parent, message, caption, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                }

                _IsPrescanCompleted = true;
                _IsSaveScanDataOnAborted = false;
                _IsAbortedOnLidOpened = false;

                // Clear the preview channels.
                if (_CurrentScanType != ScanType.Preview)
                {
                    this._PreviewImage = null;              //clear preview buffer
                    this._ImagingVm.PreviewImage = null;    //clear contrast preview image
                    this.IsPreviewChannels = false;         //close preview/contrast window
                }

                _CurrentScanType = ScanType.Normal;
                _IsSaveScanDataOnAborted = false;
                _IsAbortedOnLidOpened = false;

                // Turn off all the lasers after scanning is completed
                //EL: TODO:
                //TurnOffAllLasers();

                _ImageScanCommand.Completed -= new CommandLib.ThreadBase.CommandCompletedHandler(ImageScanCommand_Completed);
                _ImageScanCommand.CommandStatus -= new LaserScanCommand.CommandStatusHandler(ImageScanCommand_CommandStatus);
                //_ImageScanCommand.ReceiveTransfer -= new LaserScanCommand.ScanReceiveDataHandler(ImageScanCommand_ScanReceiveDataHandler);
                //_ImageScanCommand.DataReceived -= new LaserScanCommand.DataReceivedHandler(ImageScanCommand_DataReceived);
                _ImageScanCommand.ScanRegionCompleted -= ImageScanCommand_ScanRegionCompleted;
                if (SettingsManager.ConfigSettings.IsSimulationMode)
                {
                    _ImageScanCommand.CompletionEstimate -= new LaserScanCommand.CommandCompletionEstHandler(ImageScanCommand_CompletionEstimate);
                }
                _ImageScanCommand = null;

                // Did scan region changed while scanning? Get the modified scan region
                if (_ImagingVm.CropServices != null && _IsScanRegionChanged)
                {
                    if (_ImagingVm.CellSize > 0)
                    {
                        foreach (var scanRegion in SelectedAppProtocol.ScanRegions)
                        {
                            if (scanRegion != null)
                            {
                                foreach (var cropService in _ImagingVm.CropServices)
                                {
                                    if (cropService.AdornerID == scanRegion.ScanRegionNum)
                                    {
                                        scanRegion.IsScanRectDragged = true;  // dragged vs manually entered value
                                        var cropArea = cropService.GetCroppedArea();
                                        double x = Math.Round(cropArea.CroppedRectAbsolute.X / _ImagingVm.CellSize, 2);
                                        double y = Math.Round(cropArea.CroppedRectAbsolute.Y / _ImagingVm.CellSize, 2);
                                        double w = Math.Round(cropArea.CroppedRectAbsolute.Width / _ImagingVm.CellSize, 2);
                                        double h = Math.Round(cropArea.CroppedRectAbsolute.Height / _ImagingVm.CellSize, 2);
                                        if (x < 0) { x = 0; }
                                        if (y < 0) { y = 0; }
                                        if (w > 25) { w = 25; }
                                        if (h > 25) { h = 25; }
                                        if (x + w > 25) { x -= (x + w) - 25; }
                                        if (y + h > 25) { y -= (y + h) - 25; }
                                        scanRegion.X = x;
                                        scanRegion.Y = y;
                                        scanRegion.Width = w;
                                        scanRegion.Height = h;
                                        scanRegion.IsScanRectDragged = false;
                                        break;
                                    }
                                }
                            }
                        }
                        _IsScanRegionChanged = false;
                    }
                }
            });
        }

        /// <summary>
        /// Color channel to laser's wavelength
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        private string ChannelToSignal(ImageChannelType channel)
        {
            string retVal = string.Empty;
            switch (channel)
            {
                case ImageChannelType.Red:
                    retVal = "658";
                    break;
                case ImageChannelType.Green:
                    retVal = "520";
                    break;
                case ImageChannelType.Blue:
                    retVal = "488";
                    break;
                case ImageChannelType.Gray:
                    retVal = "784";
                    break;
            }
            return retVal;
        }

        //EL: TODO:
        /*public void TurnOffAllLasers()
        {
            if (Workspace.This.ApdVM.APDTransfer != null &&
                Workspace.This.ApdVM.APDTransfer.APDTransferIsAlive)
            {
                Workspace.This.ApdVM.APDTransfer.LaserSetA(0);
                System.Threading.Thread.Sleep(100);
                Workspace.This.ApdVM.APDTransfer.LaserSetB(0);
                System.Threading.Thread.Sleep(100);
                Workspace.This.ApdVM.APDTransfer.LaserSetC(0);
                System.Threading.Thread.Sleep(100);
                Workspace.This.ApdVM.APDTransfer.LaserSetD(0);
            }
        }*/

        /*private void ImageScanCommand_ScanReceiveDataHandler(ThreadBase sender, string scanType)//display received data in real time
        {
            LaserScanCommand scannedThread = (sender as LaserScanCommand);

            if (scanType == "ScannerIsReady")
            {
                Workspace.This.IsReadyScanning = true;
                Workspace.This.IsPreparingToScan = false;
            }

            if (scanType == "LIDStatus")
            {
                //EL: TODO:
                //Workspace.This.ApdVM.LIDIsOpen = Workspace.This.ApdVM.APDTransfer.LIDIsOpen;
                //if (Workspace.This.ApdVM.APDTransfer.LIDIsOpen)
                //{
                //    _IsAbortedOnLidOpened = true;
                //    ExecuteStopScanCommand("LIDOpened");
                //}
            }

            if (scanType == "RemainingTime")
            {
                RemainingTime = scannedThread.RemainingTime;
                double timeElapsed = Time - RemainingTime;
                //Workspace.This.StatusTextProgress = ImagingSystemHelper.FormatTime(timeElapsed);
                //Workspace.This.PercentCompleted = (int)((timeElapsed / (double)Time) * 100.0);
                Workspace.This.StatusTextProgress = ImagingSystemHelper.FormatTime(RemainingTime);
                Workspace.This.PercentCompleted = (int)((timeElapsed / (double)Time) * 100.0);
            }
        }*/

        private void ImageScanCommand_OnScanDataReceived(object sender, string dataName)
        {
            //switch (dataName)
            //{
            //    case "RemainingTime":
            //        RemainingTime = scanCommand.RemainingTime;
            //        break;
            //    //case "OnXRemainingTime":
            //    //    TdOnXRemaining = new Thread(OnXRemainingTime);
            //    //    TdOnXRemaining.IsBackground = true;
            //    //    TdOnXRemaining.Start();
            //    //    break;
            //}

            Application.Current.Dispatcher.BeginInvoke((Action)delegate
            {
                if (dataName == "ScanningPrepStarted" || dataName == "ScanningPrepCompleted")
                {
                    if (dataName == "ScanningPrepStarted")
                    {
                        Workspace.This.IsPreparingToScan = true;
                    }
                    else if (dataName == "ScanningPrepCompleted")
                    {
                        Workspace.This.IsPreparingToScan = false;
                    }
                }
                else if (dataName == "LIDOpened")
                {
                    if (!_IsAbortedOnLidOpened)
                    {
                        //System.Diagnostics.Trace.WriteLine("LID is opened");
                        _IsAbortedOnLidOpened = true;
                        ExecuteStopScanCommand("LIDOpened");
                        return;
                    }
                }
                else if (dataName == "RemainingTime")
                {
                    RemainingTime = _ImageScanCommand.RemainingTime;
                    double timeElapsed = Time - RemainingTime;
                    Workspace.This.StatusTextProgress = ImagingSystemHelper.FormatTime(RemainingTime);
                    double percentCompleted = (timeElapsed / (double)Time) * 100.0;
                    Workspace.This.PercentCompleted = (int)percentCompleted;
                }
            });
        }

        /*private void ImageScanCommand_DataReceived(ushort[] apdChannelA, ushort[] apdChannelB, ushort[] apdChannelC, ushort[] apdChannelD)
        {
            if (!_IsPreviewSetupCompleted) { return; }

            if (apdChannelD == null)
            {
                return;
            }

            if (_ImagingVm.PreviewChannels == null || _ImagingVm.NumOfDisplayChannels == 0)
            {
                if (ImagingVm.PreviewImage != null)
                {
                    ImagingVm.PreviewImage = null;     //Clear prievew image
                }
                return;
            }

            if (_IsUpdatingPreviewImage)
            {
                return;
            }

            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                _IsUpdatingPreviewImage = true;

                if (ImagingVm.NumOfDisplayChannels > 0)
                {
                    //
                    // NOTE: Phosphor imaging: Turn on the Laser C and get data from Laser D channel.
                    //

                    if (apdChannelD != null && _ChannelDPrevImage != null)
                    {
                        ImageProcessing.FrameToBitmap(apdChannelD, ref _ChannelDPrevImage);
                    }
                }

                UpdatePreviewDisplayImage();
            }));
        }*/
        private void ImageScanCommand_DataReceived(object sender)
        {
            if (!_IsPreviewSetupCompleted || _IsAligningPreviewImage || _IsUpdatingPreviewImage) { return; }

            if (_ImagingVm.PreviewChannels == null || _ImagingVm.NumOfDisplayChannels == 0)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (_PreviewImage != null && !_IsPreviewImageCleared)
                    {
                        try
                        {
                            // Clear the preview image if no display channel is selected.
                            _PreviewImage.Lock();
                            ImageProcessingHelper.FastClear(ref _PreviewImage);
                            // Specify the area of the bitmap that changed.
                            _PreviewImage.AddDirtyRect(new Int32Rect(0, 0, _PreviewImage.PixelWidth, _PreviewImage.PixelHeight));
                            _PreviewImage.Unlock();
                            _IsPreviewImageCleared = true;
                        }
                        catch
                        {
                        }
                    }
                }));
                return;
            }
            else
            {
                _IsPreviewImageCleared = false;
            }

            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    #region Convert ushort to WriteableBitmap

                    /*if (_ImagingVm.IsLaserL1PrvSelected)
                    {
                        if (apdChannelC != null && _ChannelL1PrevImageUnAligned != null)
                        {
                            if (_IsCropPreviewImage)
                            {
                                ImageProcessing.FrameToBitmap(apdChannelC, ref _ChannelL1PrevImageUnAligned);
                            }
                            else
                            {
                                ImageProcessing.FrameToBitmap(apdChannelC, ref _ChannelL1PrevImageUnAligned);
                                _ChannelL1PrevImage = _ChannelL1PrevImageUnAligned;
                            }
                        }
                    }
                    if (_ImagingVm.IsLaserR1PrvSelected)
                    {
                        if (apdChannelA != null && _ChannelR1PrevImageUnAligned != null)
                        {
                            if (_IsCropPreviewImage)
                            {
                                ImageProcessing.FrameToBitmap(apdChannelA, ref _ChannelR1PrevImageUnAligned);
                            }
                            else
                            {
                                ImageProcessing.FrameToBitmap(apdChannelA, ref _ChannelR1PrevImageUnAligned);
                                _ChannelR1PrevImage = _ChannelR1PrevImageUnAligned;
                            }
                        }
                    }
                    if (_ImagingVm.IsLaserR2PrvSelected)
                    {
                        if (apdChannelB != null && _ChannelR2PrevImageUnAligned != null)
                        {
                            if (_IsCropPreviewImage)
                            {
                                ImageProcessing.FrameToBitmap(apdChannelB, ref _ChannelR2PrevImageUnAligned);
                            }
                            else
                            {
                                ImageProcessing.FrameToBitmap(apdChannelB, ref _ChannelR2PrevImageUnAligned);
                                _ChannelR2PrevImage = _ChannelR2PrevImageUnAligned;
                            }
                        }
                    }*/

                    #endregion

                    if (_IsCropPreviewImage)
                    {
                        if (_IsAligningPreviewImage) return;

                        unsafe
                        {
                            #region Image Alignment Setup

                            byte* psrcimgL1 = null;
                            byte* psrcimgR1 = null;
                            byte* psrcimgR2 = null;
                            byte* pdstimgL1 = null;
                            byte* pdstimgR1 = null;
                            byte* pdstimgR2 = null;
                            byte* psrcimgTemp = null;
                            byte* pdstimgTemp = null;
                            int srcwidth = 0;
                            int srcheight = 0;
                            int srcstride = 0;
                            int dstwidth = 0;
                            int dstheight = 0;
                            int dststride = 0;
                            PixelFormat format = PixelFormats.Gray16;

                            if (_ImagingVm.IsLaserL1PrvSelected && _ChannelL1PrevImageUnAligned != null)
                            {
                                srcwidth = _ChannelL1PrevImageUnAligned.PixelWidth;
                                srcheight = _ChannelL1PrevImageUnAligned.PixelHeight;
                                srcstride = _ChannelL1PrevImageUnAligned.BackBufferStride;
                                dstwidth = _ChannelL1PrevImage.PixelWidth;
                                dstheight = _ChannelL1PrevImage.PixelHeight;
                                dststride = _ChannelL1PrevImage.BackBufferStride;
                                psrcimgL1 = (byte*)_ChannelL1PrevImageUnAligned.BackBuffer.ToPointer();
                                pdstimgL1 = (byte*)_ChannelL1PrevImage.BackBuffer.ToPointer();
                                format = _ChannelL1PrevImageUnAligned.Format;
                            }
                            if (_ImagingVm.IsLaserR1PrvSelected && _ChannelR1PrevImageUnAligned != null)
                            {
                                srcwidth = _ChannelR1PrevImageUnAligned.PixelWidth;
                                srcheight = _ChannelR1PrevImageUnAligned.PixelHeight;
                                srcstride = _ChannelR1PrevImageUnAligned.BackBufferStride;
                                dstwidth = _ChannelR1PrevImage.PixelWidth;
                                dstheight = _ChannelR1PrevImage.PixelHeight;
                                dststride = _ChannelR1PrevImage.BackBufferStride;
                                psrcimgR1 = (byte*)_ChannelR1PrevImageUnAligned.BackBuffer.ToPointer();
                                pdstimgR1 = (byte*)_ChannelR1PrevImage.BackBuffer.ToPointer();
                                format = _ChannelR1PrevImageUnAligned.Format;
                            }
                            if (_ImagingVm.IsLaserR2PrvSelected && _ChannelR2PrevImageUnAligned != null)
                            {
                                srcwidth = _ChannelR2PrevImageUnAligned.PixelWidth;
                                srcheight = _ChannelR2PrevImageUnAligned.PixelHeight;
                                srcstride = _ChannelR2PrevImageUnAligned.BackBufferStride;
                                dstwidth = _ChannelR2PrevImage.PixelWidth;
                                dstheight = _ChannelR2PrevImage.PixelHeight;
                                dststride = _ChannelR2PrevImage.BackBufferStride;
                                psrcimgR2 = (byte*)_ChannelR2PrevImageUnAligned.BackBuffer.ToPointer();
                                pdstimgR2 = (byte*)_ChannelR2PrevImage.BackBuffer.ToPointer();
                                format = _ChannelR2PrevImageUnAligned.Format;
                            }

                            #endregion

                            #region Image Alignment

                            System.Threading.Tasks.Task taskAlign = System.Threading.Tasks.Task.Factory.StartNew(() =>
                            {
                                if (_ImageScanCommand != null)
                                {
                                    if (!_IsAligningPreviewImage)
                                    {
                                        _IsAligningPreviewImage = true;

                                        if (_ImagingVm.IsLaserL1PrvSelected && psrcimgL1 != null && pdstimgL1 != null)
                                        {
                                            if (_ImageScanCommand != null)
                                            {
                                                var alignParam = _ImageScanCommand.CurrentScanParam.AlignmentParam;
                                                alignParam.LaserChannel = LaserChannels.ChannelC;
                                                try
                                                {
                                                    ImagingHelper.SFLImageAlign(psrcimgL1, srcwidth, srcheight, srcstride,
                                                                                pdstimgL1, dstwidth, dstheight, dststride,
                                                                                format, alignParam);
                                                }
                                                catch (Exception ex)
                                                {
                                                    _IsAligningPreviewImage = false;
                                                    Workspace.This.LogException("ImageScanCommand_DataReceived: AlignImage(): L1.", ex);
                                                    return;
                                                }
                                            }
                                        }
                                        if (_ImagingVm.IsLaserR1PrvSelected && psrcimgR1 != null && pdstimgR1 != null)
                                        {
                                            if (_ImageScanCommand != null)
                                            {
                                                var alignParam = _ImageScanCommand.CurrentScanParam.AlignmentParam;
                                                alignParam.LaserChannel = LaserChannels.ChannelA;
                                                try
                                                {
                                                    ImagingHelper.SFLImageAlign(psrcimgR1, srcwidth, srcheight, srcstride,
                                                                                pdstimgR1, dstwidth, dstheight, dststride,
                                                                                format, alignParam);
                                                }
                                                catch (Exception ex)
                                                {
                                                    _IsAligningPreviewImage = false;
                                                    Workspace.This.LogException("ImageScanCommand_DataReceived: AlignImage(): R1.", ex);
                                                    return;
                                                }
                                            }
                                        }
                                        if (_ImagingVm.IsLaserR2PrvSelected && psrcimgR2 != null && pdstimgR2 != null)
                                        {
                                            if (_ImageScanCommand != null)
                                            {
                                                var alignParam = _ImageScanCommand.CurrentScanParam.AlignmentParam;
                                                alignParam.LaserChannel = LaserChannels.ChannelB;
                                                try
                                                {
                                                    ImagingHelper.SFLImageAlign(psrcimgR2, srcwidth, srcheight, srcstride,
                                                                                pdstimgR2, dstwidth, dstheight, dststride,
                                                                                format, alignParam);
                                                }
                                                catch (Exception ex)
                                                {
                                                    _IsAligningPreviewImage = false;
                                                    Workspace.This.LogException("ImageScanCommand_DataReceived: AlignImage(): R2.", ex);
                                                    return;
                                                }
                                            }
                                        }
                                        _IsAligningPreviewImage = false;
                                    }
                                }
                            });

                            #endregion
                        }
                    }

                    if (!_IsAligningPreviewImage)
                    {
                        if (_SelectedAppProtocol.SelectedScanRegion.IsEdrScanning)
                        {
                            _ImagingVm.ContrastVm.DisplayImageInfo.IsSaturationChecked = false;
                        }
                        UpdatePreviewDisplayImage();
                    }
                }
                catch (Exception ex)
                {
                    _IsAligningPreviewImage = false;
                    _IsUpdatingPreviewImage = false;
                    Workspace.This.LogException("ImageScanCommand_DataReceived().", ex);
                    //_ImagingVm.PreviewImage = null;     //Clear prievew image
                    _ContrastThreadEvent.Set();
                }
            }));
        }

        /*private void ImageScanCommand_ScanRegionCompleted(ThreadBase sender, ImageInfo imageInfo, int nScanRegion)
        {
            LaserScanCommand scanningCommand = sender as LaserScanCommand;

            if (scanningCommand == null) { return; }

            var currScanParam = scanningCommand.CurrentScanParam;
            //int currScanRegion = scanningCommand.CurrentScanRegion;

            Workspace.This.Owner.Dispatcher.BeginInvoke((Action)delegate
            {
                if (imageInfo != null)
                {
                    imageInfo.CaptureType = "Phosphor Imaging";
                    imageInfo.SoftwareVersion = Workspace.This.ProductVersion;
                    imageInfo.FpgaFirmware = Workspace.This.FpgaFirmware;
                    imageInfo.SystemSN = Workspace.This.SystemSN;

                    // Get scan region
                    //EL: TODO:
                    //if (imageInfo.ScanX0 > 0)
                    //{
                    //    imageInfo.ScanX0 = (int)((imageInfo.ScanX0 - Workspace.This.ApdVM.APDTransfer.DeviceProperties.XHome) / (double)_XMotorSubdivision);
                    //}
                    //if (imageInfo.ScanY0 > 0)
                    //{
                    //    imageInfo.ScanY0 = (int)((imageInfo.ScanY0 - Workspace.This.ApdVM.APDTransfer.DeviceProperties.YHome) / (double)_YMotorSubdivision);
                    //}
                    imageInfo.SampleType = _SelectedAppProtocol.ScanRegions[nScanRegion].SelectedSampleType.DisplayName;
                    imageInfo.ScanSpeed = _SelectedAppProtocol.ScanRegions[nScanRegion].SelectedScanSpeed.DisplayName;
                    imageInfo.IntensityLevel = _SelectedAppProtocol.ScanRegions[nScanRegion].SelectedSignalLevel.IntensityLevel.ToString();
                    imageInfo.ScanZ0Abs = Workspace.This.MotorVM.AbsZPos;
                    imageInfo.MixChannel.ScanZ0 = imageInfo.ScanZ0;
                    imageInfo.MixChannel.LaserIntensityLevel = _SelectedAppProtocol.ScanRegions[nScanRegion].SelectedSignalLevel.IntensityLevel;
                    imageInfo.Comment = _SelectedAppProtocol.ScanRegions[nScanRegion].FileLocationVm.Notes;
                }

                //EL: TODO:
                //if (((ScanProcessing)scanningCommand).ScanType == "Dynamic")
                {
                    try
                    {
                        // 784: A channel : Gray (IR800?)
                        // 520: B channel : Green
                        // 658: C channel : Red
                        // 488: D channel : Blue
                        //
                        //Save and/or send image to Gallery

                        //
                        //NOTE: Phosphor imaging: Turn on the Laser C and get data from Laser D channel.
                        //

                        string docTitle = string.Empty;
                        string fileNameWithoutExt = string.Empty;
                        string fileExtension = ".tif";  // default file extension

                        string destinationFolder = _SelectedAppProtocol.ScanRegions[nScanRegion].FileLocationVm.DestinationFolder;
                        string fileFullPath = string.Empty;

                        if (string.IsNullOrEmpty(_SelectedAppProtocol.ScanRegions[nScanRegion].FileLocationVm.FileName))
                        {
                            // file name not specified, generate a new file name
                            fileNameWithoutExt = Workspace.This.GenerateFileName(string.Empty, string.Empty);
                        }
                        else
                        {
                            if (Workspace.This.CheckSupportedFileType(_SelectedAppProtocol.ScanRegions[nScanRegion].FileLocationVm.FileName))
                                fileNameWithoutExt = System.IO.Path.GetFileNameWithoutExtension(_SelectedAppProtocol.ScanRegions[nScanRegion].FileLocationVm.FileName);
                            else
                                fileNameWithoutExt = _SelectedAppProtocol.ScanRegions[nScanRegion].FileLocationVm.FileName;
                        }

                        //EL: TODO:
                        //WriteableBitmap wbScannedChanD = ((LaserImageScanCommand)scanningCommand).ChannelDImage;
                        WriteableBitmap wbScannedChanD = null;
                        if (wbScannedChanD != null)
                        {
                            docTitle = string.Format("{0}_PI{1}", fileNameWithoutExt, fileExtension);
                            bool bIsSaveAsPUB = false;

                            if (_SelectedAppProtocol.ScanRegions[nScanRegion].FileLocationVm.IsAutoSave)
                            {
                                bIsSaveAsPUB = SettingsManager.ApplicationSettings.IsAutoSavePubFileEnabled;
                                fileFullPath = System.IO.Path.Combine(destinationFolder, docTitle);
                                Workspace.This.SaveAsync(wbScannedChanD, imageInfo, fileFullPath);
                            }
                            else
                            {
                                fileFullPath = null;
                            }

                            // Add new image doc/file
                            Workspace.This.NewDocument(wbScannedChanD, imageInfo, docTitle, fileFullPath, bIsSaveAsPUB, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Scanning error.", ex);
                    }
                }

                // Select the next scan region and setup the next scan region's preview images.
                if (nScanRegion + 1 < scanningCommand.ScanParams.Count)
                {
                    // reset image buffers
                    //EL: TODO:
                    //scanningCommand.ChannelAImage = null;
                    //scanningCommand.ChannelBImage = null;
                    //scanningCommand.ChannelCImage = null;
                    //scanningCommand.ChannelDImage = null;

                    nScanRegion++;
                    // Set up the next scan region preview image settings
                    //EL: TODO:
                    //PreviewImageSetup(scanningCommand.ScanParams[nScanRegion], nScanRegion);
                    // Switch scan region
                    _SelectedAppProtocol.SelectedScanRegion = _SelectedAppProtocol.ScanRegions[nScanRegion];
                }
            });
        }*/
        private void ImageScanCommand_ScanRegionCompleted(ThreadBase sender, ImageInfo imageInfo, int nScanRegion)
        {
            LaserScanCommand scanningCommand = sender as LaserScanCommand;

            if (scanningCommand == null) { return; }

            var currScanParam = scanningCommand.ScanParams[nScanRegion];

            Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
            {
                //L1 = ChannelC
                //R1 = ChannelA
                //R2 = ChannelB
                //WriteableBitmap laserL1scannedImage = null;
                //WriteableBitmap laserR1scannedImage = null;
                //WriteableBitmap laserR2scannedImage = null;

                WriteableBitmap scannedImage = null;
                WriteableBitmap laserL1RawImage = null;
                var signals = scanningCommand.CurrentScanParam.Signals;

                for (int i = 0; i < signals.Count; i++)
                {
                    if (signals[i].LaserChannel == LaserChannels.ChannelC)      //L1
                    {
                        scannedImage = (WriteableBitmap)((LaserScanCommand)scanningCommand).ChannelCImage.Clone();
                        if (SettingsManager.ConfigSettings.IsKeepRawImages && scanningCommand.ChannelCRawImage != null)
                        {
                            laserL1RawImage = scanningCommand.ChannelCRawImage.Clone();
                        }
                    }
                    else if (signals[i].LaserChannel == LaserChannels.ChannelA) //R1
                    {
                        scannedImage = (WriteableBitmap)((LaserScanCommand)scanningCommand).ChannelAImage.Clone();
                    }
                    else if (signals[i].LaserChannel == LaserChannels.ChannelB) //R2
                    {
                        scannedImage = (WriteableBitmap)((LaserScanCommand)scanningCommand).ChannelBImage.Clone();
                    }
                }

                if (imageInfo != null)
                {
                    imageInfo.CaptureType = "Phosphor Imaging";
                    imageInfo.SoftwareVersion = Workspace.This.ProductVersion;
                    imageInfo.FWVersion = Workspace.This.FWVersion;
                    imageInfo.SystemSN = Workspace.This.SystemSN;
                    imageInfo.Software = "Sapphire FL";

                    // Get scan region
                    //EL: TODO:
                    //if (imageInfo.ScanX0 > 0)
                    //{
                    //    imageInfo.ScanX0 = (int)((imageInfo.ScanX0 - Workspace.This.ApdVM.APDTransfer.DeviceProperties.XHome) / (double)_XMotorSubdivision);
                    //}
                    //if (imageInfo.ScanY0 > 0)
                    //{
                    //    imageInfo.ScanY0 = (int)((imageInfo.ScanY0 - Workspace.This.ApdVM.APDTransfer.DeviceProperties.YHome) / (double)_YMotorSubdivision);
                    //}

                    imageInfo.SampleType = _SelectedAppProtocol.ScanRegions[nScanRegion].SelectedSampleType.DisplayName;
                    imageInfo.ScanSpeed = _SelectedAppProtocol.ScanRegions[nScanRegion].SelectedScanSpeed.DisplayName;
                    imageInfo.ScanQuality = (SettingsManager.ConfigSettings.PhosphorModuleProcessing) ? "Highest" : "High";
                    imageInfo.IntensityLevel = _SelectedAppProtocol.ScanRegions[nScanRegion].SelectedSignalLevel.IntensityLevel.ToString();
                    imageInfo.ScanZ0Abs = Workspace.This.MotorVM.AbsZPos;
                    imageInfo.MixChannel.ScanZ0 = imageInfo.ScanZ0;
                    imageInfo.MixChannel.LaserIntensityLevel = _SelectedAppProtocol.ScanRegions[nScanRegion].SelectedSignalLevel.IntensityLevel;
                    imageInfo.Comment = _SelectedAppProtocol.ScanRegions[nScanRegion].FileLocationVm.Notes;
                    int x1 = (int)Math.Round(_SelectedAppProtocol.ScanRegions[nScanRegion].ScanRect.X / _ImagingVm.CellSize);
                    int y1 = (int)Math.Round(_SelectedAppProtocol.ScanRegions[nScanRegion].ScanRect.Y / _ImagingVm.CellSize);
                    int x2 = (int)Math.Round(x1 + (_SelectedAppProtocol.ScanRegions[nScanRegion].ScanRect.Width / _ImagingVm.CellSize));
                    int y2 = (int)Math.Round(y1 + (_SelectedAppProtocol.ScanRegions[nScanRegion].ScanRect.Height / _ImagingVm.CellSize));
                    string row = string.Format("{0}-{1}", Workspace.This.IndexToRow(y1), Workspace.This.IndexToRow(y2));
                    string col = string.Format("{0}-{1}", x1, x2);
                    imageInfo.ScanRegion = row + ", " + col;
                    // User selected signal level
                    if (imageInfo.RedChannel.LaserIntensity > 0) { imageInfo.RedChannel.SignalLevel = imageInfo.RedChannel.LaserIntensityLevel.ToString(); }
                    if (imageInfo.GreenChannel.LaserIntensity > 0) { imageInfo.GreenChannel.SignalLevel = imageInfo.GreenChannel.LaserIntensityLevel.ToString(); }
                    if (imageInfo.BlueChannel.LaserIntensity > 0) { imageInfo.BlueChannel.SignalLevel = imageInfo.BlueChannel.LaserIntensityLevel.ToString(); }
                    if (imageInfo.GrayChannel.LaserIntensity > 0) { imageInfo.GrayChannel.SignalLevel = imageInfo.GrayChannel.LaserIntensityLevel.ToString(); }
                    if (imageInfo.MixChannel.LaserIntensity > 0) { imageInfo.MixChannel.SignalLevel = imageInfo.MixChannel.LaserIntensityLevel.ToString(); }
                }

                string docTitle = string.Empty;
                string fileNameWithoutExt = string.Empty;
                string fileExtension = ".tif";  // default file extension

                string destinationFolder = _SelectedAppProtocol.ScanRegions[nScanRegion].FileLocationVm.DestinationFolder;
                string fileFullPath = string.Empty;

                if (string.IsNullOrEmpty(_SelectedAppProtocol.ScanRegions[nScanRegion].FileLocationVm.FileName))
                {
                    // file name not specified, generate a new file name
                    fileNameWithoutExt = Workspace.This.GenerateFileName(string.Empty, string.Empty);
                }
                else
                {
                    if (Workspace.This.CheckSupportedFileType(_SelectedAppProtocol.ScanRegions[nScanRegion].FileLocationVm.FileName))
                        fileNameWithoutExt = System.IO.Path.GetFileNameWithoutExtension(_SelectedAppProtocol.ScanRegions[nScanRegion].FileLocationVm.FileName);
                    else
                        fileNameWithoutExt = _SelectedAppProtocol.ScanRegions[nScanRegion].FileLocationVm.FileName;
                }

                if (scannedImage != null)
                {
                    docTitle = string.Format("{0}_PI{1}", fileNameWithoutExt, fileExtension);
                    bool bIsSaveAsPUB = false;

                    if (_SelectedAppProtocol.ScanRegions[nScanRegion].FileLocationVm.IsAutoSave)
                    {
                        bIsSaveAsPUB = SettingsManager.ApplicationSettings.IsAutoSavePubFileEnabled;
                        fileFullPath = System.IO.Path.Combine(destinationFolder, docTitle);
                        var clonedImage = scannedImage.Clone();
                        clonedImage.Freeze();
                        Workspace.This.SaveAsync(clonedImage, imageInfo, fileFullPath);
                    }
                    else
                    {
                        fileFullPath = null;
                    }

                    // Keep raw image enabled?
                    if (SettingsManager.ConfigSettings.IsKeepRawImages && laserL1RawImage != null)
                    {
                        string rawDocTitle = string.Format("{0}_{1}_RAW{2}", fileNameWithoutExt, Workspace.This.LaserChannelTypeList[LaserChannels.ChannelC], fileExtension);
                        Workspace.This.NewDocument(laserL1RawImage, imageInfo, rawDocTitle, fileFullPath, false, true);
                    }

                    // Add new image doc/file
                    Workspace.This.NewDocument(scannedImage, imageInfo, docTitle, fileFullPath, bIsSaveAsPUB, true);
                }
            });

            // Select the next scan region and setup the next scan region's preview images.
            if (nScanRegion + 1 < scanningCommand.ScanParams.Count)
            {
                // reset image buffer
                //EL: TODO:
                //scanningCommand.ChannelAImage = null;
                //scanningCommand.ChannelBImage = null;
                //scanningCommand.ChannelCImage = null;
                //scanningCommand.ChannelDImage = null;

                Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
                {
                    nScanRegion++;
                    // Set up the next scan region preview image settings
                    PreviewImageSetup(scanningCommand.ScanParams[nScanRegion], nScanRegion);
                    // Switch scan region
                    _SelectedAppProtocol.SelectedScanRegion = _SelectedAppProtocol.ScanRegions[nScanRegion];
                });
            }
        }

        private unsafe void UpdatePreviewDisplayImage()
        {
            if (_PreviewImage == null) { return; }

            var imageInfo = _ImagingVm.ContrastVm.DisplayImageInfo;
            if (imageInfo != null)
            {
                imageInfo.IsSaturationChecked = true;
                imageInfo.SelectedChannel = ImageChannelType.Mix;

                // color order R-G-B-K(gray)
                WriteableBitmap[] displayChannels = { null, null, null, null };

                bool bIsGrayscale = false;
                int nDstStride = 0;
                if (_ImagingVm.PreviewChannels.Count == 1)
                {
                    bIsGrayscale = true;
                    imageInfo.NumOfChannels = 1;
                    if (_ImagingVm.IsLaserL1PrvSelected)
                        displayChannels[0] = _ChannelL1PrevImage;
                    if (_ImagingVm.IsLaserR1PrvSelected)
                        displayChannels[0] = _ChannelR1PrevImage;
                    if (_ImagingVm.IsLaserR2PrvSelected)
                        displayChannels[0] = _ChannelR2PrevImage;

                    if (_PreviewImage != null)
                        nDstStride = _PreviewImage.BackBufferStride;
                }

                /*if (!bIsGrayscale)
                {
                    if (_ImagingVm.IsLaserL1PrvSelected)
                    {
                        if (_ImagingVm.LaserL1ColorChannel == ImageChannelType.Red)
                            displayChannels[0] = _ChannelL1PrevImage;
                        else if (_ImagingVm.LaserL1ColorChannel == ImageChannelType.Green)
                            displayChannels[1] = _ChannelL1PrevImage;
                        else if (_ImagingVm.LaserL1ColorChannel == ImageChannelType.Blue)
                            displayChannels[2] = _ChannelL1PrevImage;
                        else if (_ImagingVm.LaserL1ColorChannel == ImageChannelType.Gray)
                            displayChannels[3] = _ChannelL1PrevImage;
                    }
                    if (_ImagingVm.IsLaserR1PrvSelected)
                    {
                        if (_ImagingVm.LaserR1ColorChannel == ImageChannelType.Red)
                            displayChannels[0] = _ChannelR1PrevImage;
                        else if (_ImagingVm.LaserR1ColorChannel == ImageChannelType.Green)
                            displayChannels[1] = _ChannelR1PrevImage;
                        else if (_ImagingVm.LaserR1ColorChannel == ImageChannelType.Blue)
                            displayChannels[2] = _ChannelR1PrevImage;
                        else if (_ImagingVm.LaserR1ColorChannel == ImageChannelType.Gray)
                            displayChannels[3] = _ChannelR1PrevImage;
                    }
                    if (_ImagingVm.IsLaserR2PrvSelected)
                    {
                        if (_ImagingVm.LaserR2ColorChannel == ImageChannelType.Red)
                            displayChannels[0] = _ChannelR2PrevImage;
                        else if (_ImagingVm.LaserR2ColorChannel == ImageChannelType.Green)
                            displayChannels[1] = _ChannelR2PrevImage;
                        else if (_ImagingVm.LaserR2ColorChannel == ImageChannelType.Blue)
                            displayChannels[2] = _ChannelR2PrevImage;
                        else if (_ImagingVm.LaserR2ColorChannel == ImageChannelType.Gray)
                            displayChannels[3] = _ChannelR2PrevImage;
                    }
                }*/

                //Contrast in another thread
                //
                int nWidth = 0;
                int nHeight = 0;
                int nStride = 0;
                foreach (var dispChan in displayChannels)
                {
                    if (dispChan != null)
                    {
                        nWidth = dispChan.PixelWidth;
                        nHeight = dispChan.PixelHeight;
                        nStride = dispChan.BackBufferStride;
                        break;
                    }
                }

                //byte* pPreviewImageData = null;
                byte* pRchImageData = null;
                byte* pGchImageData = null;
                byte* pBchImageData = null;
                byte* pKchImageData = null; //gray channel
                byte*[] pDisplayImageData = null;

                if (_ChannelL1PrevImage != null)
                    _ChannelL1PrevImage.Lock();
                if (_ChannelR1PrevImage != null)
                    _ChannelR1PrevImage.Lock();
                if (_ChannelR2PrevImage != null)
                    _ChannelR2PrevImage.Lock();

                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    if (displayChannels[0] != null)
                    {
                        pRchImageData = (byte*)displayChannels[0].BackBuffer.ToPointer();
                    }
                    if (displayChannels[1] != null)
                    {
                        pGchImageData = (byte*)displayChannels[1].BackBuffer.ToPointer();
                    }
                    if (displayChannels[2] != null)
                    {
                        pBchImageData = (byte*)displayChannels[2].BackBuffer.ToPointer();
                    }
                    if (displayChannels[3] != null)
                    {
                        pKchImageData = (byte*)displayChannels[3].BackBuffer.ToPointer();
                    }

                    pDisplayImageData = new byte*[4] { pRchImageData, pGchImageData, pBchImageData, pKchImageData };
                });

                // Contrast in its own thread.
                //
                _ContrastThreadEvent.Set();
                Thread contrastThread = new Thread(() => ContrastTask(pDisplayImageData, nWidth, nHeight, nStride,
                                                                      _ImagingVm.ContrastVm.DisplayImageInfo, bIsGrayscale));
                contrastThread.Start();
            }

        }

        private unsafe void ContrastTask(byte*[] pSrcImageData, int nWidth, int nHeight, int nSrcStride,
                                         ImageInfo imageInfo, bool bIsGrayscale)
        {
            _ContrastThreadEvent.WaitOne();

            int nDstStride = 0;
            byte* pDstImageData = null;
            bool bIsAutoProcessed = false;

            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                if (_PreviewImage != null)
                {
                    nDstStride = _PreviewImage.BackBufferStride;
                    _PreviewImage.Lock();
                    pDstImageData = (byte*)_PreviewImage.BackBuffer.ToPointer();
                }
            });

            try
            {
                PixelFormatType srcPixelFormat = PixelFormatType.P16u_C1;
                if (bIsGrayscale)
                {
                    // scale single channel grayscale image
                    ImageProcessingHelper.UpdateDisplayImage(pSrcImageData[0], nWidth, nHeight, nSrcStride, srcPixelFormat,
                                                             imageInfo, pDstImageData, nDstStride, ref bIsAutoProcessed);
                }
                else
                {
                    // scale multi-channel image
                    ImageProcessingHelper.UpdateDisplayImage(pSrcImageData, nWidth, nHeight, nSrcStride, srcPixelFormat,
                                                             imageInfo, pDstImageData, nDstStride, ref bIsAutoProcessed);
                }
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    Workspace.This.LogException("Error updating display image.", ex);

                    if (_PreviewImage != null)
                        _PreviewImage.Unlock();
                    if (_ChannelL1PrevImage != null)
                        _ChannelL1PrevImage.Unlock();
                    if (_ChannelR1PrevImage != null)
                        _ChannelR1PrevImage.Unlock();
                    if (_ChannelR2PrevImage != null)
                        _ChannelR2PrevImage.Unlock();
                });
                _IsUpdatingPreviewImage = false;
                _ContrastThreadEvent.Set();
                return;
            }

            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                // This is in case the user deselected all the preview channels.
                if (_ImagingVm.PreviewImage == null)
                    _ImagingVm.PreviewImage = _PreviewImage;

                if (_PreviewImage != null)
                {
                    // Specify the area of the bitmap that changed.
                    _PreviewImage.AddDirtyRect(new Int32Rect(0, 0, _PreviewImage.PixelWidth, _PreviewImage.PixelHeight));
                }
                if (_ImagingVm.ContrastVm.DisplayImageInfo.RedChannel.IsAutoChecked ||
                    _ImagingVm.ContrastVm.DisplayImageInfo.GreenChannel.IsAutoChecked ||
                    _ImagingVm.ContrastVm.DisplayImageInfo.BlueChannel.IsAutoChecked ||
                    _ImagingVm.ContrastVm.DisplayImageInfo.GrayChannel.IsAutoChecked ||
                    _ImagingVm.ContrastVm.DisplayImageInfo.MixChannel.IsAutoChecked)
                {
                    if (bIsAutoProcessed)
                    {
                        // Reset auto-contrast flags.
                        _ImagingVm.ContrastVm.ResetAuto();
                    }
                }

                if (_PreviewImage != null)
                    _PreviewImage.Unlock();
                if (_ChannelL1PrevImage != null)
                    _ChannelL1PrevImage.Unlock();
                if (_ChannelR1PrevImage != null)
                    _ChannelR1PrevImage.Unlock();
                if (_ChannelR2PrevImage != null)
                    _ChannelR2PrevImage.Unlock();
            });

            _ContrastThreadEvent.Set();
            _IsUpdatingPreviewImage = false;
        }


        private void ChannelsDataToBitmap(out WriteableBitmap image, Int32 width, Int32 height, ushort[] red, ushort[] green, ushort[] blue)
        {
            image = null;
            int i = 0;
            int channelDatalength = width * height;
            ushort[] channels = new ushort[channelDatalength * 3];
            for (i = 0; i < channelDatalength; i++)
            {
                channels[i * 3] = red[i];
                channels[i * 3 + 1] = green[i];
                channels[i * 3 + 2] = blue[i];
            }
            image = new WriteableBitmap(width, height, 96, 96, PixelFormats.Rgb48, null);
            int iStride = (width * 48 + 7) / 8;
            int temp = image.BackBufferStride;
            image.WritePixels(new Int32Rect(0, 0, width, height), channels, iStride, 0);
        }

        #region ICommand StopScanCommand

        public ICommand StopScanCommand
        {
            get
            {
                if (_StopScanCommand == null)
                {
                    _StopScanCommand = new RelayCommand(ExecuteStopScanCommand, CanExecuteStopScanCommand);
                }

                return _StopScanCommand;
            }
        }
        public void ExecuteStopScanCommand(object parameter)
        {
            if (_ImageScanCommand != null)
            {
                //Enable Motor control
                //_UiUpdateTimer.Stop();

                // Close preview contrast window to avoid it from overlapping messagebox below.
                IsPreviewChannels = false;

                string lidStatus = string.Empty;
                if (parameter != null && parameter is string)
                {
                    lidStatus = parameter as string;
                }

                if (!string.IsNullOrEmpty(lidStatus) && lidStatus.Equals("LIDOpened"))
                {
                    _IsSaveScanDataOnAborted = true;
                    _ImageScanCommand.IsSaveDataOnScanAbort = _IsSaveScanDataOnAborted;
                    _ImageScanCommand.Abort();  // Abort the scanning thread
                    _ImageScanCommand.DataReceived -= ImageScanCommand_DataReceived;
                }
                else
                {
                    string caption = "Abort scanning...";
                    string message = "Would you like to save your scanned data?";
                    MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.YesNoCancel, MessageBoxImage.Information);
                    if (result == MessageBoxResult.Yes)
                    {
                        _IsSaveScanDataOnAborted = true;
                        _ImageScanCommand.IsSaveDataOnScanAbort = _IsSaveScanDataOnAborted;
                    }
                    else
                    {
                        _IsSaveScanDataOnAborted = false;
                        _ImageScanCommand.IsSaveDataOnScanAbort = _IsSaveScanDataOnAborted;
                    }

                    if (result != MessageBoxResult.Cancel)
                    {
                        _ImageScanCommand.DataReceived -= ImageScanCommand_DataReceived;
                        _ImageScanCommand.Abort();  // Abort the scanning thread
                    }
                }
            }
        }
        public bool CanExecuteStopScanCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region ClosePreviewWinCommand
        private RelayCommand _ClosePreviewWinCommand = null;
        public ICommand ClosePreviewWinCommand
        {
            get
            {
                if (_ClosePreviewWinCommand == null)
                {
                    _ClosePreviewWinCommand = new RelayCommand((p) => OnClosePreviewWin(p), (p) => CanClosePreviewWin(p));
                }
                return _ClosePreviewWinCommand;
            }
        }

        private void OnClosePreviewWin(object parameter)
        {
            IsPreviewChannels = false;
        }

        private bool CanClosePreviewWin(object parameter)
        {
            return IsPreviewChannels;
        }

        #endregion

        #region SaveProtocolCommand

        private RelayCommand _SaveProtocolCommand = null;
        public ICommand SaveProtocolCommand
        {
            get
            {
                if (_SaveProtocolCommand == null)
                {
                    _SaveProtocolCommand = new RelayCommand(ExecuteSaveProtocolCommand, CanExecuteSaveProtocolCommand);
                }

                return _SaveProtocolCommand;
            }
        }
        public void ExecuteSaveProtocolCommand(object parameter)
        {
            // Now allowing custom focus to be save without first creating a custom sample type
            //if (SelectedAppProtocol.SelectedSampleType.DisplayName.Equals("Custom", StringComparison.OrdinalIgnoreCase))
            //{
            //    string caption = "Create new protocol...";
            //    string message = "Please select a sample type or create a new sample type with the current focus value.";
            //    Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
            //    return;
            //}

            SaveProtocolWindow saveProtocolWin = new SaveProtocolWindow();
            saveProtocolWin.Owner = Workspace.This.Owner;
            saveProtocolWin.DataContext = this;
            bool? dialogResult = saveProtocolWin.ShowDialog();
            if (dialogResult == true)
            {
                // save item to the config file
                if (SaveAppProtocol(SelectedAppProtocol, saveProtocolWin.ProtocolName))
                {
                    // Add item to protocol options list
                    ProtocolViewModel protocol = new ProtocolViewModel(SelectedAppProtocol);
                    //protocol.ProtocolOptionChanged += new ProtocolViewModel.ProtocolOptionChangedDelegate(Protocol_ProtocolOptionChanged);
                    //protocol.PixelSizeOptions = new List<ResolutionType>(SettingsManager.ConfigSettings.ResolutionOptions);
                    //protocol.ScanSpeedOptions = new List<ScanSpeedType>(SettingsManager.ConfigSettings.ScanSpeedOptions);
                    //protocol.SampleTypeOptions = new List<SampleTypeSetting>(SettingsManager.ConfigSettings.SampleTypeSettings);
                    protocol.ProtocolName = saveProtocolWin.ProtocolName;  //new protocol name
                    ObservableCollection<ProtocolViewModel> tmpProtocols = new ObservableCollection<ProtocolViewModel>(_AppProtocolOptions);
                    tmpProtocols.Add(protocol);
                    AppProtocolOptions = tmpProtocols;
                    SelectedAppProtocol = protocol;
                    SelectedAppProtocol.IsModified = false;
                }
            }
        }
        public bool CanExecuteSaveProtocolCommand(object parameter)
        {
            return true;
        }

        public bool ValidateProtocolName(string protocolName)
        {
            bool bResult = true;

            // Check if the protocol name is already exists
            if (AppProtocolOptions != null)
            {
                foreach (var protocol in AppProtocolOptions)
                {
                    if (string.Equals(protocol.ProtocolName, protocolName, StringComparison.OrdinalIgnoreCase))
                    {
                        string caption = "Protocol name already exists.";
                        string message = "The protocol name already exists. Please enter a different name.";
                        Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                        //saveProtocolWin.DialogResult = false;
                        bResult = false;
                    }
                }
            }

            return bResult;
        }

        #endregion

        #region DeleteProtocolCommand

        private RelayCommand _DeleteProtocolCommand = null;
        public ICommand DeleteProtocolCommand
        {
            get
            {
                if (_DeleteProtocolCommand == null)
                {
                    _DeleteProtocolCommand = new RelayCommand(ExecuteDeleteProtocolCommand, CanExecuteDeleteProtocolCommand);
                }

                return _DeleteProtocolCommand;
            }
        }
        public void ExecuteDeleteProtocolCommand(object parameter)
        {
            if (SelectedAppProtocol != null)
            {
                string caption = "Delete protocol...";
                string message = string.Format("Are you sure you want to delete the protocol \"{0}\".", SelectedAppProtocol.ProtocolName);
                MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No)
                {
                    return;
                }

                // Delete selected item from the config file
                DeleteAppProtocol(SelectedAppProtocol.ProtocolName);

                // Select the next item after removing the selected item
                int nextItem = _AppProtocolOptions.IndexOf(SelectedAppProtocol) - 1;

                // Delete selected item from protocol options list
                ObservableCollection<ProtocolViewModel> tmpProtocols = new ObservableCollection<ProtocolViewModel>(_AppProtocolOptions);
                tmpProtocols.Remove(SelectedAppProtocol);
                AppProtocolOptions = tmpProtocols;

                if (nextItem < 0) { nextItem = 0; }
                SelectedAppProtocol = _AppProtocolOptions[nextItem];
                ResetPreviewImages();
            }
        }
        public bool CanExecuteDeleteProtocolCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region AddScanRegionCommand
        private RelayCommand _AddScanRegionCommand = null;
        public ICommand AddScanRegionCommand
        {
            get
            {
                if (_AddScanRegionCommand == null)
                {
                    _AddScanRegionCommand = new RelayCommand(ExecuteAddScanRegionCommand, CanExecuteAddScanRegionCommand);
                }
                return _AddScanRegionCommand;
            }
        }
        private void ExecuteAddScanRegionCommand(object parameter)
        {
            if (Workspace.This.IsScanning) { return; }

            if (SelectedAppProtocol != null)
            {
                if (!SelectedAppProtocol.IsModified)
                {
                    // Mark as modified (and backup default settings)
                    SelectedAppProtocol.IsModified = true;
                }

                // Add a new scan region
                int nScanRegionId = _ImagingVm.GetAdornerId();
                ScanRegionViewModel scanRegionVm = new ScanRegionViewModel
                {
                    ScanRegionNum = nScanRegionId,
                    CellSize = _ImagingVm.CellSize,
                    NumOfCells = _ImagingVm.NumOfCells
                };
                ScanRegionRect scanRect = null;

                if (SelectedAppProtocol.SelectedScanRegion != null)
                {
                    // Duplicate the scan settings from the current selected scan region
                    //
                    scanRect = (ScanRegionRect)SelectedAppProtocol.SelectedScanRegion.ScanRect.Clone();
                    int pixelSize = SelectedAppProtocol.SelectedScanRegion.SelectedPixelSize.Value;
                    int scanSpeed = SelectedAppProtocol.SelectedScanRegion.SelectedScanSpeed.Value;
                    //int scanQuality = SelectedAppProtocol.SelectedScanRegion.SelectedScanQuality.Value;
                    scanRegionVm.SelectedPixelSize = scanRegionVm.PixelSizeOptions.FirstOrDefault(x => x.Value == pixelSize);
                    scanRegionVm.SelectedScanSpeed = scanRegionVm.ScanSpeedOptions.FirstOrDefault(x => x.Value == scanSpeed);
                    // Phosphor Imaging tab no longer has the scan Quality selection option
                    //scanRegionVm.SelectedScanQuality = scanRegionVm.ScanQualityOptions.FirstOrDefault(x => x.Value == scanQuality);

                    var sampleType = SelectedAppProtocol.SelectedScanRegion.SelectedSampleType;
                    scanRegionVm.SelectedSampleType = scanRegionVm.SampleTypeOptions.FirstOrDefault(x => x.DisplayName.Contains(sampleType.DisplayName));

                    scanRegionVm.SelectedSignalLevel = scanRegionVm.SignalLevelOptions.Where(x => x.DisplayName == SelectedAppProtocol.SelectedScanRegion.SelectedSignalLevel.DisplayName).FirstOrDefault();
                    
                    scanRegionVm.FileLocationVm.IsAutoSave = SelectedAppProtocol.SelectedScanRegion.FileLocationVm.IsAutoSave;
                    if (scanRegionVm.FileLocationVm.IsAutoSave)
                    {
                        string fileName = SelectedAppProtocol.SelectedScanRegion.FileLocationVm.FileName;
                        if (string.IsNullOrEmpty(fileName))
                        {
                            fileName = Workspace.This.GenerateFileName(string.Empty, string.Empty);
                        }
                        else
                        {
                            fileName = string.Format("{0} ({1})", fileName, nScanRegionId);
                        }
                        scanRegionVm.FileLocationVm.FileName = fileName;
                        scanRegionVm.FileLocationVm.DestinationFolder = SelectedAppProtocol.SelectedScanRegion.FileLocationVm.DestinationFolder;
                    }
                }
                else
                {
                    if (scanRegionVm.PixelSizeOptions != null && scanRegionVm.PixelSizeOptions.Count > 0)
                    {
                        scanRegionVm.SelectedPixelSize = scanRegionVm.PixelSizeOptions.FirstOrDefault(x => x.Value == 200);
                    }
                    if (scanRegionVm.ScanSpeedOptions != null && scanRegionVm.ScanSpeedOptions.Count > 0)
                    {
                        scanRegionVm.SelectedScanSpeed = scanRegionVm.ScanSpeedOptions.FirstOrDefault(x => x.DisplayName == "Highest");
                    }
                    // Phosphor Imaging tab no longer has the scan Quality selection option
                    //if (scanRegionVm.ScanQualityOptions != null && scanRegionVm.ScanQualityOptions.Count > 0)
                    //{
                    //    scanRegionVm.SelectedScanQuality = scanRegionVm.ScanQualityOptions.FirstOrDefault(x => x.DisplayName == "High");
                    //}
                    if (scanRegionVm.SampleTypeOptions != null && scanRegionVm.SampleTypeOptions.Count > 0)
                    {
                        int index = scanRegionVm.SampleTypeOptions.ToList().FindIndex(x => x.DisplayName.Contains("Membrane"));
                        if (index >= 0 && index < scanRegionVm.SampleTypeOptions.Count)
                        {
                            scanRegionVm.SelectedSampleType = scanRegionVm.SampleTypeOptions[index];
                        }
                    }
                    if (scanRegionVm.SignalLevelOptions != null && scanRegionVm.SignalLevelOptions.Count > 0)
                    {
                        scanRegionVm.SelectedSignalLevel = scanRegionVm.SignalLevelOptions.FirstOrDefault(x => x.IntensityLevel == 3);
                    }

                    double left = 0;
                    double top = 0; //NOTE: region is flipped vertically
                    if (SelectedAppProtocol.ScanRegions.Count > 0)
                    {
                        switch (nScanRegionId)
                        {
                            case 2:
                                left = 10 * _ImagingVm.CellSize;
                                top = 0;
                                break;
                            case 3:
                                left = 0;
                                top = 10 * _ImagingVm.CellSize;
                                break;
                            case 4:
                                left = 10 * _ImagingVm.CellSize;
                                top = 10 * _ImagingVm.CellSize;
                                break;
                            default:
                                if (SelectedAppProtocol.ScanRegions.Count >= 4 && SelectedAppProtocol.ScanRegions.Count < 19)
                                {
                                    int offset = SelectedAppProtocol.ScanRegions.Count - 3;
                                    left = offset * _ImagingVm.CellSize;
                                    top = offset * _ImagingVm.CellSize;
                                }
                                else if (SelectedAppProtocol.ScanRegions.Count >= 19)
                                {
                                    int offset = SelectedAppProtocol.ScanRegions.Count % 19;
                                    left = offset * _ImagingVm.CellSize;
                                    top = offset * _ImagingVm.CellSize;
                                }
                                break;
                        }
                    }
                    double width = _ImagingVm.CellSize * 10;
                    double height = _ImagingVm.CellSize * 10;
                    scanRect = new ScanRegionRect(left, top, width, height);
                }

                scanRegionVm.ScanRect = scanRect;
                scanRegionVm.ScanRectChanged += ScanRegion_ScanRectChanged;
                scanRegionVm.ScanRegionSettingChanged += ScanRegion_ScanRegionSettingChanged;

                SelectedAppProtocol.AddScanRegion(scanRegionVm, false);

                // Add scan region adorner
                _ImagingVm.IsInitializing = true;   // don't trigger scan rect changed event
                _ImagingVm.AddScanRegionSelection(nScanRegionId, scanRect, scanRegionVm.Color);
                _ImagingVm.IsInitializing = false;

                // Selected the added scan region
                if (SelectedAppProtocol.ScanRegions != null)
                {
                    SelectedAppProtocol.SelectedScanRegion = SelectedAppProtocol.ScanRegions.Where(x => x.ScanRegionNum == nScanRegionId).FirstOrDefault();
                }
            }
        }
        private bool CanExecuteAddScanRegionCommand(object parameter)
        {
            return true;
        }
        #endregion

        #region RemoveScanRegionCommand
        private RelayCommand _RemoveScanRegionCommand = null;
        public ICommand RemoveScanRegionCommand
        {
            get
            {
                if (_RemoveScanRegionCommand == null)
                {
                    _RemoveScanRegionCommand = new RelayCommand(ExecuteRemoveScanRegionCommand, CanExecuteRemoveScanRegionCommand);
                }
                return _RemoveScanRegionCommand;
            }
        }
        private void ExecuteRemoveScanRegionCommand(object parameter)
        {
            if (SelectedAppProtocol != null)
            {
                var scanRegion = parameter as ScanRegionViewModel;
                if (scanRegion != null)
                {
                    scanRegion.ScanRectChanged -= ScanRegion_ScanRectChanged;
                    scanRegion.ScanRegionSettingChanged -= ScanRegion_ScanRegionSettingChanged;
                    _ImagingVm.RemoveScanRegionSelection(scanRegion);
                    SelectedAppProtocol.RemoveScanRegion(scanRegion);
                    if (_SelectedAppProtocol.SelectedScanRegion != null)
                    {
                        ScanTime = ImagingSystemHelper.FormatTime(_SelectedAppProtocol.SelectedScanRegion.GetScanTime());
                    }
                    if (_ImagingVm.PreviewImages != null &&
                        _ImagingVm.PreviewImages.Count >= scanRegion.ScanRegionNum)
                    {
                        for (int i = _ImagingVm.PreviewImages.Count - 1; i >= 0; i++)
                        {
                            if (scanRegion.ScanRegionNum == _ImagingVm.PreviewImages[i].RegionNumber)
                            {
                                _ImagingVm.PreviewImages.Remove(_ImagingVm.PreviewImages[i]);
                                break;
                            }
                        }
                    }
                }
            }
        }
        private bool CanExecuteRemoveScanRegionCommand(object parameter)
        {
            return true;
        }
        #endregion

        /// <summary>
        /// Update AppProtocolOptions from existing protocols.
        /// </summary>
        /// <param name="protocols"></param>
        /*public void LoadAppProtocols(List<Protocol> protocols)
        {
            if (protocols == null || protocols.Count == 0)
            {
                return;
            }

            // Clear application protocols (starting at the end)
            for (int i = 0, j = AppProtocolOptions.Count; i < j; i++)
            {
                AppProtocolOptions.RemoveAt(0);
            }

            foreach (var protocol in protocols)
            {
                if (protocol != null)
                {
                    ProtocolViewModel protocolVm = new ProtocolViewModel();
                    protocolVm.ProtocolName = protocol.Name;
                    //protocolVm.IsInitializing = true; // Don't trigger selected scan region changed event
                    protocolVm.ScanRegions.Clear();
                    foreach (var protocolScanRegion in protocol.ScanRegions)
                    {
                        ScanRegionViewModel scanRegionVm = new ScanRegionViewModel
                        {
                            ScanRegionNum = protocolScanRegion.RegionNumber,
                            CellSize = _ImagingVm.CellSize,
                            NumOfCells = _ImagingVm.NumOfCells,
                            // Don't trigger selected scan region changed event
                            IsInitializing = true
                        };

                        if (scanRegionVm.PixelSizeOptions != null && scanRegionVm.PixelSizeOptions.Count > 0)
                            scanRegionVm.SelectedPixelSize = scanRegionVm.PixelSizeOptions[protocolScanRegion.PixelSize - 1];     // config file is 1 index
                        if (scanRegionVm.ScanSpeedOptions != null && scanRegionVm.ScanSpeedOptions.Count > 0)
                            scanRegionVm.SelectedScanSpeed = scanRegionVm.ScanSpeedOptions[protocolScanRegion.ScanSpeed - 1];     // config file is 1 index
                        if (scanRegionVm.SampleTypeOptions != null && scanRegionVm.SampleTypeOptions.Count > 0)
                        {
                            if (protocolScanRegion.IsCustomFocus)
                            {
                                scanRegionVm.IsCustomFocus = protocolScanRegion.IsCustomFocus;
                                scanRegionVm.CustomFocusValue = protocolScanRegion.CustomFocusPosition;
                                int index = scanRegionVm.SampleTypeOptions.ToList().FindIndex(x => x.DisplayName == "Custom");
                                if (index >= 0 && index < scanRegionVm.SampleTypeOptions.Count)
                                {
                                    scanRegionVm.SelectedSampleType = scanRegionVm.SampleTypeOptions[index];
                                }
                            }
                            else
                            {
                                int index = scanRegionVm.SampleTypeOptions.ToList().FindIndex(x => x.Position == protocolScanRegion.SampleType);
                                if (index >= 0 && index < scanRegionVm.SampleTypeOptions.Count)
                                {
                                    scanRegionVm.SelectedSampleType = scanRegionVm.SampleTypeOptions[index];
                                }
                            }
                        }

                        if (scanRegionVm.SignalLevelOptions != null && scanRegionVm.SignalLevelOptions.Count > 0 && protocolScanRegion.IntensityLevel > 0)
                        {
                            scanRegionVm.SelectedSignalLevel = scanRegionVm.SignalLevelOptions[protocolScanRegion.IntensityLevel - 1];     // config file is 1 index
                        }

                        Rect rect = new Rect(
                            protocolScanRegion.ScanRect.X * _ImagingVm.CellSize, protocolScanRegion.ScanRect.Y * _ImagingVm.CellSize,
                            protocolScanRegion.ScanRect.Width * _ImagingVm.CellSize, protocolScanRegion.ScanRect.Height * _ImagingVm.CellSize);
                        scanRegionVm.ScanRect = new ScanRegionRect(rect);

                        // Subcribe to scan rect property changed
                        scanRegionVm.ScanRectChanged += ScanRegion_ScanRectChanged;
                        // Subcribe to scan region settings property changed
                        scanRegionVm.ScanRegionSettingChanged += ScanRegion_ScanRegionSettingChanged;

                        scanRegionVm.IsInitializing = false;

                        protocolVm.AddScanRegion(scanRegionVm, true);
                    }

                    // Subcribe to the protocol's scan region changed notification
                    protocolVm.ScanRegionChanged += ProtocolVm_ScanRegionChanged;

                    if (protocolVm.SelectedScanRegion == null)
                    {
                        // Make sure the tab header on each scan region is selected to update scan region settings UI.
                        protocolVm.SelectedScanRegion = protocolVm.ScanRegions.First();
                    }

                    //protocolVm.IsInitializing = false;

                    _AppProtocolOptions.Add(protocolVm);
                }
            }

            if (AppProtocolOptions.Count > 0)
            {
                SelectedAppProtocol = AppProtocolOptions.Last();
            }
            else
            {
                SelectedAppProtocol = null;
            }
        }*/
        /// <summary>
        /// Update AppProtocolOptions from existing protocols.
        /// </summary>
        /// <param name="protocols"></param>
        public void LoadAppProtocols(List<Protocol> protocols)
        {
            if (protocols == null || protocols.Count == 0)
            {
                return;
            }

            // Clear application protocols (starting at the end)
            for (int i = 0, j = AppProtocolOptions.Count; i < j; i++)
            {
                AppProtocolOptions.RemoveAt(0);
            }

            foreach (var protocol in protocols)
            {
                if (protocol != null)
                {
                    ProtocolViewModel protocolVm = new ProtocolViewModel();
                    protocolVm.ProtocolName = protocol.Name;
                    //protocolVm.IsInitializing = true; // Don't trigger selected scan region changed event
                    protocolVm.ScanRegions.Clear();
                    foreach (var protocolScanRegion in protocol.ScanRegions)
                    {
                        ScanRegionViewModel scanRegionVm = new ScanRegionViewModel
                        {
                            ScanRegionNum = protocolScanRegion.RegionNumber,
                            CellSize = _ImagingVm.CellSize,
                            NumOfCells = _ImagingVm.NumOfCells,
                            // Don't trigger selected scan region changed event
                            IsInitializing = true
                        };

                        if (scanRegionVm.PixelSizeOptions != null && scanRegionVm.PixelSizeOptions.Count > 0)
                            scanRegionVm.SelectedPixelSize = scanRegionVm.PixelSizeOptions[protocolScanRegion.PixelSize - 1];     // config file is 1 index
                        if (scanRegionVm.ScanSpeedOptions != null && scanRegionVm.ScanSpeedOptions.Count > 0)
                            scanRegionVm.SelectedScanSpeed = scanRegionVm.ScanSpeedOptions[protocolScanRegion.ScanSpeed - 1];     // config file is 1 index
                        // Phosphor Imaging tab no longer has the scan Quality selection option
                        //if (scanRegionVm.ScanQualityOptions != null && scanRegionVm.ScanQualityOptions.Count > 0)
                        //{
                        //    scanRegionVm.SelectedScanQuality = scanRegionVm.ScanQualityOptions.FirstOrDefault(x => x.Position == protocolScanRegion.ScanQuality);
                        //    if (scanRegionVm.SelectedScanQuality == null)
                        //    {
                        //        scanRegionVm.SelectedScanQuality = scanRegionVm.ScanQualityOptions[1];  //select the second item ('High')
                        //    }
                        //}
                        if (scanRegionVm.SampleTypeOptions != null && scanRegionVm.SampleTypeOptions.Count > 0)
                        {
                            if (protocolScanRegion.IsCustomFocus)
                            {
                                scanRegionVm.IsCustomFocus = protocolScanRegion.IsCustomFocus;
                                scanRegionVm.CustomFocusValue = protocolScanRegion.CustomFocusPosition;
                                int index = scanRegionVm.SampleTypeOptions.ToList().FindIndex(x => x.DisplayName == "Custom");
                                if (index >= 0 && index < scanRegionVm.SampleTypeOptions.Count)
                                {
                                    scanRegionVm.SelectedSampleType = scanRegionVm.SampleTypeOptions[index];
                                }
                            }
                            else
                            {
                                int index = scanRegionVm.SampleTypeOptions.ToList().FindIndex(x => x.Position == protocolScanRegion.SampleType);
                                if (index >= 0 && index < scanRegionVm.SampleTypeOptions.Count)
                                {
                                    scanRegionVm.SelectedSampleType = scanRegionVm.SampleTypeOptions[index];
                                }
                            }
                        }

                        if (scanRegionVm.SignalLevelOptions != null && scanRegionVm.SignalLevelOptions.Count > 0)
                        {
                            scanRegionVm.SelectedSignalLevel = scanRegionVm.SignalLevelOptions[protocolScanRegion.IntensityLevel - 1];     // config file is 1 index
                        }

                        /*if (protocolScanRegion.Lasers != null)
                        {
                            foreach (var laser in protocolScanRegion.Lasers)
                            {
                                List<int> keyList = new List<int>(SettingsManager.ConfigSettings.LasersSignalList.Keys);
                                SignalViewModel signal = new SignalViewModel(SettingsManager.ConfigSettings.LasersSignalList[keyList[0]].Count);
                                signal.LaserOptions = new ObservableCollection<LaserTypes>(Workspace.This.LaserOptions);
                                if (signal != null && signal.LaserOptions != null && signal.LaserOptions.Count > 0)
                                {
                                    var itemsFound = signal.LaserOptions.Where(item => item.LaserChannel == laser.LaserChannel).ToList();
                                    if (itemsFound != null && itemsFound.Count > 0)
                                    {
                                        signal.SelectedLaser = itemsFound[0];
                                    }
                                    //signal.SelectedSignalLevel = signal.SignalLevelOptions[laser.SignalIntensity - 1];
                                    //for (int i = 0; i < signal.ColorChannelOptions.Count; i++)
                                    //{
                                    //    if (laser.ColorChannel == signal.ColorChannelOptions[i].ImageColorChannel)
                                    //    {
                                    //        signal.SelectedColorChannel = signal.ColorChannelOptions[i];
                                    //        break;
                                    //    }
                                    //}
                                    signal.SelectedColorChannel = signal.ColorChannelOptions.FirstOrDefault(item => item.ImageColorChannel == ImageChannelType.Gray);
                                    //signal.SignalChanged += new SignalViewModel.SignalChangedDelegate(Protocol_SignalChanged);
                                    scanRegionVm.SignalList.Add(signal);
                                }
                            }
                        }*/

                        Rect rect = new Rect(
                            protocolScanRegion.ScanRect.X * _ImagingVm.CellSize, protocolScanRegion.ScanRect.Y * _ImagingVm.CellSize,
                            protocolScanRegion.ScanRect.Width * _ImagingVm.CellSize, protocolScanRegion.ScanRect.Height * _ImagingVm.CellSize);
                        scanRegionVm.ScanRect = new ScanRegionRect(rect);

                        if (protocolScanRegion.IsAutoSaveEnabled)
                        {
                            scanRegionVm.FileLocationVm.IsAutoSave = protocolScanRegion.IsAutoSaveEnabled;
                            scanRegionVm.FileLocationVm.DestinationFolder = protocolScanRegion.AutoSavePath;
                        }

                        // Subcribe to scan rect property changed
                        scanRegionVm.ScanRectChanged += ScanRegion_ScanRectChanged;
                        // Subcribe to scan region settings property changed
                        scanRegionVm.ScanRegionSettingChanged += ScanRegion_ScanRegionSettingChanged;

                        scanRegionVm.IsInitializing = false;

                        protocolVm.AddScanRegion(scanRegionVm, true);
                    }

                    // Subcribe to the protocol's scan region changed notification
                    protocolVm.ScanRegionChanged += ProtocolVm_ScanRegionChanged;

                    if (protocolVm.SelectedScanRegion == null)
                    {
                        // Make sure the tab header on each scan region is selected to update scan region settings UI.
                        protocolVm.SelectedScanRegion = protocolVm.ScanRegions.First();
                    }

                    //protocolVm.IsInitializing = false;

                    _AppProtocolOptions.Add(protocolVm);
                }
            }

            if (AppProtocolOptions.Count > 0)
            {
                SelectedAppProtocol = AppProtocolOptions.First();
            }
            else
            {
                SelectedAppProtocol = null;
            }
        }

        private void ProtocolVm_ScanRegionChanged(object sender)
        {
            if (_SelectedAppProtocol != null)
            {
                if (_SelectedAppProtocol.SelectedScanRegion != null)
                {
                    _SelectedAppProtocol.SelectedScanRegion.FileLocationVm.FileSize = GetFileSize();
                    ScanTime = ImagingSystemHelper.FormatTime(_SelectedAppProtocol.SelectedScanRegion.GetScanTime());
                }
            }
        }

        private void ImagingVm_ScanRectChanged(object sender)
        {
            CropService cropService = (CropService)sender;
            if (cropService != null)
            {
                // Selected the added scan region
                if (SelectedAppProtocol.ScanRegions != null && !Workspace.This.IsScanning)
                {
                    if (_ImagingVm.CellSize > 0)
                    {
                        SelectedAppProtocol.SelectedScanRegion = SelectedAppProtocol.ScanRegions.Where(z => z.ScanRegionNum == cropService.AdornerID).FirstOrDefault();

                        if (SelectedAppProtocol.SelectedScanRegion != null)
                        {
                            SelectedAppProtocol.SelectedScanRegion.IsScanRectDragged = true;  // dragged vs manually entered value
                            var cropArea = cropService.GetCroppedArea();
                            double x = Math.Round(cropArea.CroppedRectAbsolute.X / _ImagingVm.CellSize, 2);
                            double y = Math.Round(cropArea.CroppedRectAbsolute.Y / _ImagingVm.CellSize, 2);
                            double w = Math.Round(cropArea.CroppedRectAbsolute.Width / _ImagingVm.CellSize, 2);
                            double h = Math.Round(cropArea.CroppedRectAbsolute.Height / _ImagingVm.CellSize, 2);
                            if (x < 0) { x = 0; }
                            if (y < 0) { y = 0; }
                            if (w > 25) { w = 25; }
                            if (h > 25) { h = 25; }
                            if (x + w > 25) { x -= (x + w) - 25; }
                            if (y + h > 25) { y -= (y + h) - 25; }
                            SelectedAppProtocol.SelectedScanRegion.X = x;
                            SelectedAppProtocol.SelectedScanRegion.Y = y;
                            SelectedAppProtocol.SelectedScanRegion.Width = w;
                            SelectedAppProtocol.SelectedScanRegion.Height = h;
                            SelectedAppProtocol.SelectedScanRegion.IsScanRectDragged = false;
                        }
                    }
                }
                else if (Workspace.This.IsScanning)
                {
                    _IsScanRegionChanged = true;
                }
            }
        }

        private void ScanRegion_ScanRectChanged(object sender, bool bIsScanRectChanged, bool bIsScanRectDragged)
        {
            if (_SelectedAppProtocol == null) { return; }

            if (!bIsScanRectChanged)
            {
                // The selection is changing, backup settings of not already backed up
                _SelectedAppProtocol.IsModified = true;
            }

            if (bIsScanRectChanged && !bIsScanRectDragged)
            {
                // Clear preview image(s)
                //if (_ImagingVm.PreviewImages != null && _ImagingVm.PreviewImages.Count > 0)
                //{
                //    _ImagingVm.PreviewImages.Clear();
                //}

                if (_ImagingVm.RemoveAllScanRegions())   // remove scan rect adorner
                {
                    // Start from the end of the list to select the first tab item
                    for (int index = 0; index < _SelectedAppProtocol.ScanRegions.Count; index++)
                    {
                        int nScanRegionId = (_SelectedAppProtocol.ScanRegions[index].ScanRegionNum == 0) ? index + 1 : _SelectedAppProtocol.ScanRegions[index].ScanRegionNum;
                        _ImagingVm.IsInitializing = true;   // Don't trigger scan region changed
                        _ImagingVm.AddScanRegionSelection(nScanRegionId, _SelectedAppProtocol.ScanRegions[index].ScanRect, _SelectedAppProtocol.ScanRegions[index].Color);
                        _ImagingVm.IsInitializing = false;
                    }
                }
            }

            ScanTime = ImagingSystemHelper.FormatTime(_SelectedAppProtocol.SelectedScanRegion.GetScanTime());
            _SelectedAppProtocol.SelectedScanRegion.FileLocationVm.FileSize = GetFileSize();
        }

        private void ScanRegion_ScanRegionSettingChanged(object sender, bool bIsSelectionChanged, bool bIsSignalChanged)
        {
            if (_SelectedAppProtocol == null) { return; }

            if (!bIsSelectionChanged)
            {
                // The selection (or property value) is changing, backup settings of not already backed up
                _SelectedAppProtocol.IsModified = true;
            }
            else
            {
                // The selection/property has changed, calculate estimate scan time and file size

                if (_SelectedAppProtocol.SelectedScanRegion != null)
                {
                    ScanTime = ImagingSystemHelper.FormatTime(_SelectedAppProtocol.SelectedScanRegion.GetScanTime());
                    _SelectedAppProtocol.SelectedScanRegion.FileLocationVm.FileSize = GetFileSize();
                }
            }
        }

        /*private void PreviewImageSetup(ScanParameterStruct scanParam, int nScanRegion)
        {
            // Calcuate deltaX and deltaY for preview image alignment/cropping.
            //
            int width = scanParam.ScanDeltaX / scanParam.XMotorSubdivision * 1000 / scanParam.Res;
            int height = scanParam.ScanDeltaY / scanParam.YMotorSubdivision * 1000 / scanParam.Res;
            _IsPreviewSetupCompleted = false;

            // Automatically check all preview the channels
            _ImagingVm.IsLaserAPrvSelected = false;
            _ImagingVm.IsLaserBPrvSelected = false;
            _ImagingVm.IsLaserCPrvSelected = false;
            _ImagingVm.IsLaserDPrvSelected = false;
            _ImagingVm.IsContrastLaserAChannel = false;
            _ImagingVm.IsContrastLaserBChannel = false;
            _ImagingVm.IsContrastLaserCChannel = false;
            _ImagingVm.IsContrastLaserDChannel = false;
            _IsUpdatingPreviewImage = false;
            _ImagingVm.ContrastVm.DisplayImageInfo.IsContrastRedChannel = false;
            _ImagingVm.ContrastVm.DisplayImageInfo.IsContrastGreenChannel = false;
            _ImagingVm.ContrastVm.DisplayImageInfo.IsContrastBlueChannel = false;
            _ImagingVm.ContrastVm.DisplayImageInfo.IsContrastGrayChannel = false;
            _ImagingVm.ContrastVm.DisplayImageInfo.IsDisplayRedChannel = false;
            _ImagingVm.ContrastVm.DisplayImageInfo.IsDisplayGreenChannel = false;
            _ImagingVm.ContrastVm.DisplayImageInfo.IsDisplayBlueChannel = false;
            _ImagingVm.ContrastVm.DisplayImageInfo.IsDisplayGrayChannel = false;

            _ChannelDPrevImage = null;

            _ImagingVm.IsLaserCPrvSelected = true;  // Select C preview channel
            _ImagingVm.IsLaserCPrvVisible = true;   // Show C preview channel
            _ImagingVm.IsContrastLaserCChannel = true;
            _ImagingVm.PreviewImage = _PreviewImage;
            _ImagingVm.ContrastVm.NumOfChannels = 1;
            _ImagingVm.ContrastVm.DisplayImageInfo.NumOfChannels = 1;
            _ImagingVm.ContrastVm.DisplayImageInfo.IsContrastGrayChannel = true;
            _ImagingVm.ContrastVm.DisplayImageInfo.IsDisplayGrayChannel = true;
            _ImagingVm.IsContrastChannelAllowed = false;    // Single channel don't display the contrast channel checkbox

            foreach (var signal in scanParam.Signals)
            {
                if (signal.LaserType == LaserType.LaserD)
                {
                    Workspace.This.ApdVM.ApdDGain = signal.ApdGain;
                    break;
                }
            }

            _ImagingVm.IsContrastLaserCChannel = true;

            // Allocate preview display image buffer
            BitmapPalette pal = new BitmapPalette(ImageProcessing.GetColorTableIndexed(true));
            _PreviewImage = new WriteableBitmap(width, height, 96, 96, PixelFormats.Indexed8, pal);

            // Allocate D channel 16-bit preview image buffer
            _ChannelDPrevImage = new WriteableBitmap(width, height, 96, 96, PixelFormats.Gray16, null);

            //EL: TODO:
            double x = 0, y = 0;
            //double x = (scanParam.ScanX0 - Workspace.This.ApdVM.APDTransfer.DeviceProperties.XHome) / (double)scanParam.XMotorSubdivision * ImagingVm.CellSize * ImagingVm.NumOfCells / _ScanWidthInMm;
            //double y = (scanParam.ScanY0 - Workspace.This.ApdVM.APDTransfer.DeviceProperties.YHome) / (double)scanParam.YMotorSubdivision * ImagingVm.CellSize * ImagingVm.NumOfCells / _ScanHeightInMm;
            double w = Math.Round(scanParam.ScanDeltaX * ImagingVm.CellSize * ImagingVm.NumOfCells / _ScanWidthInMm / scanParam.XMotorSubdivision);
            double h = Math.Round(scanParam.ScanDeltaY * ImagingVm.CellSize * ImagingVm.NumOfCells / _ScanHeightInMm / scanParam.YMotorSubdivision);

            var imageItem = new ImageItem();
            imageItem.Top = y;
            imageItem.Left = x;
            imageItem.Width = w;
            imageItem.Height = h;
            imageItem.Image = _PreviewImage;
            _ImagingVm.PreviewImages.Add(imageItem);

            Time = scanParam.Time;  // Estimated scan time for the scan region

            _IsPreviewSetupCompleted = true;
        }*/
        private void PreviewImageSetup(ScanParameterStruct scanParam, int nScanRegion)
        {
            // Calcuate preview image (aligned and unalighned image) width and height.
            //
            int previewImageWidth = 0;
            int previewImageHeight = 0;
            int width = scanParam.Width;
            int height = scanParam.Height;
            
            if (scanParam.IsUnidirectionalScan)
            {
                // The height was doubled on unidirectional scan to synchronize the scanner's LED progress bar
                height /= 2;
            }

            //int width = (int)Math.Round(scanParam.ScanDeltaX / scanParam.XMotorSubdivision * 1000.0 / scanParam.Res);
            //int height = (int)Math.Round(scanParam.ScanDeltaY / scanParam.YMotorSubdivision * 1000.0 / scanParam.Res);
            //TODO: temporary work-around: make sure the width is an even value to avoid the skewed on the scanned image
            //if (width % 2 != 0)
            //{
            //    int deltaX = scanParam.ScanDeltaX - (int)Math.Round(scanParam.Res / 1000.0 * scanParam.XMotorSubdivision);
            //    scanParam.ScanDeltaX = deltaX;
            //    scanParam.Width = (int)Math.Round(scanParam.ScanDeltaX / scanParam.XMotorSubdivision * 1000.0 / scanParam.Res);
            //}
            //int offsetDeltaXPulse = 0;
            _IsPreviewSetupCompleted = false;
            _IsAligningPreviewImage = false;
            _IsUpdatingPreviewImage = false;
            bool bIsZScanning = scanParam.IsZScanning;
            bool bIsSequentialScanning = scanParam.IsSequentialScanning;

            /*if (Workspace.This.EthernetController != null && Workspace.This.EthernetController.IsConnected)
            {
                //（因为在设计中将R1透镜的位置和R2透镜的位置更换了，然后参数名没有更改成对应的通道，所以下面参数名R1其实是R2,R2其实是R1）....
                // (in the design, the position of R1 lens and R2 lens is changed, and the parameter name is not changed to the corresponding channel, so the following parameter name R1 is actually R2, R2 is actually R1)....
                //L透镜到R2透镜之间的距离（mm），毫米为单位
                //The distance between L lens and R2 lens (mm), in millimeters
                //int OpticalL_R1Distance = (int)Workspace.This.NewParameterVM.OpticalL_R1Distance;
                //int opticalL_R1Distance = (int)Workspace.This.EthernetController.DeviceProperties.OpticalLR1Distance;

                //nOffsetWidth = opticalL_R1Distance * 1000 / scanParam.Res;

                //根据当前选择的分辨率计算L透镜到R2透镜之间的实际像素宽度
                // Calculate the actual pixel width between L lens and R2 lens based on the currently selected resolution
                double offsetWidth = (int)(scanParam.AlignmentParam.OpticalL_R1Distance * 1000 / scanParam.Res);

                previewImageWidth = width - (int)offsetWidth;
                previewImageHeight = height;

                //ScanDeltaX adjustment for the distance (mm) between L and R2 lens
                //if (scanParam.AlignmentParam.OpticalL_R1Distance > 0 && scanParam.Res == 150)
                //{
                //    offsetDeltaXPulse = (int)scanParam.AlignmentParam.OpticalL_R1Distance - 1;
                //}
                //else
                //{
                //    offsetDeltaXPulse = (int)scanParam.AlignmentParam.OpticalL_R1Distance;
                //}
                //offsetDeltaXPulse = (int)(offsetDeltaXPulse * scanParam.XMotorSubdivision);

            }
            else if (SettingsManager.ConfigSettings.IsSimulationMode)
            {
                previewImageWidth = width;
                previewImageHeight = height;
            }*/

            previewImageWidth = width;
            previewImageHeight = height;
            if (!SettingsManager.ConfigSettings.IsSimulationMode)
            {
                // Calculate the actual pixel width between L lens and R2 lens based on the currently selected resolution
                int opticalDist = (int)(scanParam.AlignmentParam.OpticalL_R1Distance * 1000.0 / scanParam.Res);
                // Calculate the preview image width and height
                //previewImageWidth = width - opticalDist - (int)(scanParam.AlignmentParam.XOverscanExtraMoveLength * 1000.0 / scanParam.Res);
                //previewImageHeight = height - (int)(scanParam.AlignmentParam.YOverscanExtraMoveLength * 1000.0 / scanParam.Res);
                // No overscan compensation
                previewImageWidth = width - opticalDist;
                previewImageHeight = height;
            }

            // If actual and aligned/cropped image size are the same; don't align/crop the images.
            //
            _IsCropPreviewImage = false;
            if (previewImageWidth != width || previewImageHeight != height)
            {
                _IsCropPreviewImage = true;
            }

            //scanParam.Width = width;
            //scanParam.Height = height;

            // Automatically check all preview the channels
            _ImagingVm.IsLaserL1PrvSelected = false;
            _ImagingVm.IsLaserR1PrvSelected = false;
            _ImagingVm.IsLaserR2PrvSelected = false;
            _ImagingVm.IsContrastLaserL1Channel = false;
            _ImagingVm.IsContrastLaserR1Channel = false;
            _ImagingVm.IsContrastLaserR2Channel = false;
            _IsUpdatingPreviewImage = false;
            _ImagingVm.ContrastVm.DisplayImageInfo.IsContrastRedChannel = false;
            _ImagingVm.ContrastVm.DisplayImageInfo.IsContrastGreenChannel = false;
            _ImagingVm.ContrastVm.DisplayImageInfo.IsContrastBlueChannel = false;
            _ImagingVm.ContrastVm.DisplayImageInfo.IsContrastGrayChannel = false;
            _ImagingVm.ContrastVm.DisplayImageInfo.IsDisplayRedChannel = false;
            _ImagingVm.ContrastVm.DisplayImageInfo.IsDisplayGreenChannel = false;
            _ImagingVm.ContrastVm.DisplayImageInfo.IsDisplayBlueChannel = false;
            _ImagingVm.ContrastVm.DisplayImageInfo.IsDisplayGrayChannel = false;

            _ChannelL1PrevImage = null;
            _ChannelL1PrevImageUnAligned = null;
            _ChannelR1PrevImage = null;
            _ChannelR1PrevImageUnAligned = null;
            _ChannelR2PrevImage = null;
            _ChannelR2PrevImageUnAligned = null;

            bool bIs4channelImage = false;
            bool bIsGrayscaleImage = false;

            foreach (var signal in scanParam.Signals)
            {
                if (signal.ColorChannel == (int)ImageChannelType.Red)
                {
                    _ImagingVm.ContrastVm.DisplayImageInfo.IsContrastRedChannel = true;
                    _ImagingVm.ContrastVm.DisplayImageInfo.IsDisplayRedChannel = true;
                }
                else if (signal.ColorChannel == (int)ImageChannelType.Green)
                {
                    _ImagingVm.ContrastVm.DisplayImageInfo.IsContrastGreenChannel = true;
                    _ImagingVm.ContrastVm.DisplayImageInfo.IsDisplayGreenChannel = true;
                }
                else if (signal.ColorChannel == (int)ImageChannelType.Blue)
                {
                    _ImagingVm.ContrastVm.DisplayImageInfo.IsContrastBlueChannel = true;
                    _ImagingVm.ContrastVm.DisplayImageInfo.IsDisplayBlueChannel = true;
                }
                else if (signal.ColorChannel == (int)ImageChannelType.Gray)
                {
                    _ImagingVm.ContrastVm.DisplayImageInfo.IsContrastGrayChannel = true;
                    _ImagingVm.ContrastVm.DisplayImageInfo.IsDisplayGrayChannel = true;
                    if (scanParam.Signals.Count > 1)
                    {
                        bIs4channelImage = true;
                    }
                    else
                    {
                        bIsGrayscaleImage = true;
                    }
                }

                if (signal.LaserChannel == LaserChannels.ChannelC)      //L1
                {
                    //_ImagingVm.PreviewChannels.Add(LaserType.LaserA);
                    _LaserL1ColorChannel = (ImageChannelType)signal.ColorChannel;
                    _ImagingVm.LaserL1ColorChannel = (ImageChannelType)signal.ColorChannel;
                    _ImagingVm.IsLaserL1PrvSelected = true;
                    _ImagingVm.IsContrastLaserL1Channel = true;
                    if (_IsCropPreviewImage)
                    {
                        _ChannelL1PrevImage = new WriteableBitmap(previewImageWidth, previewImageHeight, 96, 96, PixelFormats.Gray16, null);
                        _ChannelL1PrevImageUnAligned = new WriteableBitmap(width, height, 96, 96, PixelFormats.Gray16, null);
                        scanParam.BackBufferStride = _ChannelL1PrevImageUnAligned.BackBufferStride;
                        _ImageScanCommand.ChannelCBackBuffer = _ChannelL1PrevImageUnAligned.BackBuffer;
                    }
                    else
                    {
                        _ChannelL1PrevImage = new WriteableBitmap(width, height, 96, 96, PixelFormats.Gray16, null);
                    }
                }
                else if (signal.LaserChannel == LaserChannels.ChannelA)  //R1
                {
                    //_ImagingVm.PreviewChannels.Add(LaserType.LaserB);
                    _LaserR1ColorChannel = (ImageChannelType)signal.ColorChannel;
                    _ImagingVm.LaserR1ColorChannel = (ImageChannelType)signal.ColorChannel;
                    _ImagingVm.IsLaserR1PrvSelected = true;
                    _ImagingVm.IsContrastLaserR1Channel = true;
                    if (_IsCropPreviewImage)
                    {
                        _ChannelR1PrevImage = new WriteableBitmap(previewImageWidth, previewImageHeight, 96, 96, PixelFormats.Gray16, null);
                        _ChannelR1PrevImageUnAligned = new WriteableBitmap(width, height, 96, 96, PixelFormats.Gray16, null);
                        scanParam.BackBufferStride = _ChannelR1PrevImageUnAligned.BackBufferStride;
                        _ImageScanCommand.ChannelABackBuffer = _ChannelR1PrevImageUnAligned.BackBuffer;
                    }
                    else
                    {
                        _ChannelR1PrevImage = new WriteableBitmap(width, height, 96, 96, PixelFormats.Gray16, null);
                    }
                }
                else if (signal.LaserChannel == LaserChannels.ChannelB)  //R2
                {
                    //_ImagingVm.PreviewChannels.Add(LaserType.LaserC);
                    _LaserR2ColorChannel = (ImageChannelType)signal.ColorChannel;
                    _ImagingVm.LaserR2ColorChannel = (ImageChannelType)signal.ColorChannel;
                    _ImagingVm.IsLaserR2PrvSelected = true;
                    _ImagingVm.IsContrastLaserR2Channel = true;
                    if (_IsCropPreviewImage)
                    {
                        _ChannelR2PrevImage = new WriteableBitmap(previewImageWidth, previewImageHeight, 96, 96, PixelFormats.Gray16, null);
                        _ChannelR2PrevImageUnAligned = new WriteableBitmap(width, height, 96, 96, PixelFormats.Gray16, null);
                        scanParam.BackBufferStride = _ChannelR2PrevImageUnAligned.BackBufferStride;
                        _ImageScanCommand.ChannelBBackBuffer = _ChannelR2PrevImageUnAligned.BackBuffer;
                    }
                    else
                    {
                        _ChannelR2PrevImage = new WriteableBitmap(width, height, 96, 96, PixelFormats.Gray16, null);
                    }
                }
            }   //foreach Signals

            _PreviewImage = null;

            // Allocate preview display image buffer
            if (bIsGrayscaleImage)
            {
                //if ((previewImageWidth * previewImageHeight) > (20000 * 20000))
                //{
                //    //_PreviewImage = null;
                //    BitmapPalette pal = new BitmapPalette(ImageProcessing.GetColorTableIndexed(true));
                //    _PreviewImage = new WriteableBitmap(previewImageWidth, previewImageHeight, 96, 96, PixelFormats.Indexed8, pal);
                //}
                //else
                //{
                //    _PreviewImage = new WriteableBitmap(previewImageWidth, previewImageHeight, 96, 96, PixelFormats.Rgb24, null);
                //}
                BitmapPalette pal = new BitmapPalette(ImageProcessing.GetColorTableIndexed(true));
                _PreviewImage = new WriteableBitmap(previewImageWidth, previewImageHeight, 96, 96, PixelFormats.Indexed8, pal);
                _ImagingVm.ContrastVm.NumOfChannels = 1;
                _ImagingVm.ContrastVm.DisplayImageInfo.NumOfChannels = 1;
            }
            else
            {
                //if (_PreviewImage == null || _PreviewImage.Format != PixelFormats.Rgb24)
                //{
                //    _PreviewImage = null;
                //    _PreviewImage = new WriteableBitmap(previewImageWidth, previewImageHeight, 96, 96, PixelFormats.Rgb24, null);
                //}
                _PreviewImage = new WriteableBitmap(previewImageWidth, previewImageHeight, 96, 96, PixelFormats.Rgb24, null);

                if (bIs4channelImage)
                {
                    _ImagingVm.ContrastVm.NumOfChannels = 4;
                    _ImagingVm.ContrastVm.DisplayImageInfo.NumOfChannels = 4;
                }
                else
                {
                    _ImagingVm.ContrastVm.NumOfChannels = 3;
                    _ImagingVm.ContrastVm.DisplayImageInfo.NumOfChannels = 3;
                }

                //
                // Don't merge the images if total image size after a channel merges is > 2GB
                //
                //double totalFileSize = GetFileSizeInBytes();
                //_IsAutoMergeImages = true;
                //if (totalFileSize > 2100000000)
                //{
                //    _IsAutoMergeImages = false;
                //
                //    string caption = "File size to large...";
                //    string message = "Image size is too large. Scanned image will appear and save as individual channnels.";
                //    Xceed.Wpf.Toolkit.MessageBox.Show(message, caption);
                //}
            }

            // Preview contrast window settings.
            if (scanParam.Signals.Count == 1)
            {
                _ImagingVm.IsContrastChannelAllowed = false;    // Single channel don't display the contrast channel checkbox
            }
            _ImagingVm.ContrastVm.NumOfChannels = scanParam.Signals.Count;
            _ImagingVm.PreviewImage = _PreviewImage;

            // Set preview image coordinate
            //double x = (scanParam.ScanX0 - (int)(Workspace.This.MotorVM.AbsXPos * scanParam.XMotorSubdivision)) / (double)scanParam.XMotorSubdivision * ImagingVm.CellSize * ImagingVm.NumOfCells / _ScanWidthInMm;
            //double y = (scanParam.ScanY0 - (int)(Workspace.This.MotorVM.AbsYPos * scanParam.YMotorSubdivision)) / (double)scanParam.YMotorSubdivision * ImagingVm.CellSize * ImagingVm.NumOfCells / _ScanHeightInMm;
            //double w = (scanParam.ScanDeltaX - offsetDeltaXPulse) * ImagingVm.CellSize * ImagingVm.NumOfCells / _ScanWidthInMm / scanParam.XMotorSubdivision;
            //double h = scanParam.ScanDeltaY * ImagingVm.CellSize * ImagingVm.NumOfCells / _ScanHeightInMm / scanParam.YMotorSubdivision;
            //var imageItem = new ImageItem();
            //imageItem.Top = y;
            //imageItem.Left = x;
            //imageItem.Width = w;
            //imageItem.Height = h;
            //imageItem.Image = _PreviewImage;
            //imageItem.RegionNumber = nScanRegion;
            var imageItem = new ImageItem();
            var sr = _SelectedAppProtocol.ScanRegions[nScanRegion];
            imageItem.Top = sr.ScanRect.Y;
            imageItem.Left = sr.ScanRect.X;
            imageItem.Width = sr.ScanRect.Width;
            imageItem.Height = sr.ScanRect.Height;
            imageItem.Image = _PreviewImage;
            imageItem.RegionNumber = sr.ScanRegionNum;
            for (int index = _ImagingVm.PreviewImages.Count - 1; index >= 0; index--)
            {
                //if (_ImagingVm.PreviewImages[index].Top == y && _ImagingVm.PreviewImages[index].Left == x &&
                //    _ImagingVm.PreviewImages[index].Width == w && _ImagingVm.PreviewImages[index].Height == h)
                //{
                //    _ImagingVm.PreviewImages.Remove(_ImagingVm.PreviewImages[index]);
                //    break;
                //}
                // Scan region tab's region number is 1 based
                if (_ImagingVm.PreviewImages[index].RegionNumber == nScanRegion + 1 &&
                    _ImagingVm.PreviewImages[index].Top == sr.ScanRect.Y && _ImagingVm.PreviewImages[index].Left == sr.ScanRect.X &&
                    _ImagingVm.PreviewImages[index].Width == sr.ScanRect.Width && _ImagingVm.PreviewImages[index].Height == sr.ScanRect.Height)
                {
                    _ImagingVm.PreviewImages.Remove(_ImagingVm.PreviewImages[index]);
                    break;
                }
            }
            _ImagingVm.PreviewImages.Add(imageItem);

            Time = scanParam.Time;  // Estimated scan time for the scan region

            _IsPreviewSetupCompleted = true;
        }


        private void UpdateScanRegionPreviewChannels()
        {
            if (_SelectedAppProtocol != null)
            {
                // Add scan rect adorner
                if (_ImagingVm.DisplayCanvas != null)
                {
                    // Clear preview image(s)
                    if (_ImagingVm.PreviewImages != null && _ImagingVm.PreviewImages.Count > 0)
                    {
                        _ImagingVm.PreviewImages.Clear();
                    }

                    if (_ImagingVm.RemoveAllScanRegions())   // remove scan rect adorner
                    {
                        //foreach (var scanRegion in _SelectedAppProtocol.ScanRegions)
                        //{
                        //    int nScanRegionId = _ImagingVm.GetAdornerId();
                        //    _ImagingVm.AddScanRegionSelection(nScanRegionId, scanRegion.ScanRect);
                        //}
                        // Start from the end of the list to select the first tab item
                        for (int index = _SelectedAppProtocol.ScanRegions.Count - 1; index >= 0; index--)
                        {
                            int nScanRegionId = (_SelectedAppProtocol.ScanRegions[index].ScanRegionNum == 0) ? index + 1 : _SelectedAppProtocol.ScanRegions[index].ScanRegionNum;
                            _ImagingVm.IsInitializing = true;   // Don't trigger scan region changed
                            _ImagingVm.AddScanRegionSelection(nScanRegionId, _SelectedAppProtocol.ScanRegions[index].ScanRect, _SelectedAppProtocol.ScanRegions[index].Color);
                            _ImagingVm.IsInitializing = false;
                        }
                    }
                }

                if (_SelectedAppProtocol.SelectedScanRegion != null)
                {
                    if (_ImagingVm != null)
                    {
                        _ImagingVm.IsLaserL1PrvVisible = false;
                        _ImagingVm.IsLaserR1PrvVisible = false;
                        _ImagingVm.IsLaserR2PrvVisible = false;
                        _ImagingVm.IsContrastLaserL1Channel = false;
                        _ImagingVm.IsContrastLaserR1Channel = false;
                        _ImagingVm.IsContrastLaserR2Channel = false;

                        // image color channel
                        _LaserL1ColorChannel = ImageChannelType.Gray;
                        _LaserR1ColorChannel = ImageChannelType.None;
                        _LaserR2ColorChannel = ImageChannelType.None;

                        _ImagingVm.IsLaserL1PrvSelected = true;     // Select L1 preview channel
                        _ImagingVm.IsLaserR1PrvSelected = false;
                        _ImagingVm.IsLaserR2PrvSelected = false;

                        _ImagingVm.IsLaserL1PrvVisible = true;   // Show L1 preview channel
                        _ImagingVm.ContrastVm.NumOfChannels = 1;
                        _ImagingVm.IsContrastChannelAllowed = false;
                        _ImagingVm.ContrastVm.DisplayImageInfo.IsContrastGrayChannel = true;
                        _ImagingVm.ContrastVm.DisplayImageInfo.IsDisplayGrayChannel = true;

                        //var itemsFound = Workspace.This.LaserOptions.Where(item => item.SensorType == IvSensorType.PMT).ToList();
                        //if (itemsFound != null && itemsFound.Count > 0)
                        //{
                        //    //signal.SelectedLaser = itemsFound[0];
                        //}
                    }
                }
            }
        }

        /// <summary>
        /// Calculate estimated scan time
        /// </summary>
        /// <returns></returns>
        int GetScanTime()
        {
            int totalScanTime = 0;
            if (_SelectedAppProtocol != null && _SelectedAppProtocol.SelectedScanRegion != null)
            {
                if (_SelectedAppProtocol.SelectedScanRegion.SelectedPixelSize != null &&
                    _SelectedAppProtocol.SelectedScanRegion.SelectedScanSpeed != null)
                {
                    int deltaY = (int)(_ScanHeightInMm * _SelectedAppProtocol.SelectedScanRegion.ScanRect.Height / (_ImagingVm.CellSize * _ImagingVm.NumOfCells));   //250: Total scan area in mm
                    int height = (int)(deltaY * 1000.0 / (double)_SelectedAppProtocol.SelectedScanRegion.SelectedPixelSize.Value);
                    totalScanTime = (int)((double)height * (double)_SelectedAppProtocol.SelectedScanRegion.SelectedScanSpeed.Value / 2.0);

                    if (SettingsManager.ConfigSettings.PhosphorModuleProcessing)
                    {
                        totalScanTime *= 2;
                    }
                }
            }
            return totalScanTime;
        }

        /// <summary>
        /// Get file size for the selected region
        /// </summary>
        /// <returns></returns>
        private string GetFileSize()
        {
            string retValue = string.Empty;

            if (_SelectedAppProtocol != null &&
                _SelectedAppProtocol.SelectedScanRegion != null &&
                _SelectedAppProtocol.SelectedScanRegion.SelectedPixelSize != null)
            {
                double deltaX = _ScanWidthInMm * _SelectedAppProtocol.SelectedScanRegion.ScanRect.Width / (_ImagingVm.CellSize * _ImagingVm.NumOfCells);   //250: Total scan area in mm
                double deltaY = _ScanHeightInMm * _SelectedAppProtocol.SelectedScanRegion.ScanRect.Height / (_ImagingVm.CellSize * _ImagingVm.NumOfCells);   //250: Total scan area in mm
                double width = deltaX * 1000.0 / (double)_SelectedAppProtocol.SelectedScanRegion.SelectedPixelSize.Value;
                double height = deltaY * 1000.0 / (double)_SelectedAppProtocol.SelectedScanRegion.SelectedPixelSize.Value;

                //int nNumChannels = 1;
                double dNumBytes = 2.0;
                double sizePerFile = width * height * dNumBytes;

                string strSizePerFile = string.Empty;

                // Return in KB if less a MB
                // 1KB = 1024 bytes 
                // 1MB = 1024 * 1024
                //
                if (sizePerFile < (1024 * 1024))
                {
                    double fileSizeInKB = Math.Round((sizePerFile / 1024), 2);
                    strSizePerFile = string.Format("{0} KB", fileSizeInKB);
                }
                else
                {
                    double fileSizeInMB = Math.Round((sizePerFile / (1024 * 1024)), 2);
                    strSizePerFile = string.Format("{0} MB", fileSizeInMB);
                }
                retValue = strSizePerFile;
            }
            return retValue;
        }

        private void ResetPreviewImages()
        {
            //_ChannelAPrevImage = null;
            //_ChannelAPrevImageUnAligned = null;
            //_ChannelBPrevImage = null;
            //_ChannelBPrevImageUnAligned = null;
            //_ChannelCPrevImage = null;
            //_ChannelCPrevImageUnAligned = null;
            //_PrevImageAligned = null;
            //_ChannelDPrevImageUnAligned = null;
            _PreviewImage = null;
            _ImagingVm.PreviewImage = _PreviewImage;
        }

        //
        /// <summary>
        /// Save the selected protocol to protocol file (protocols.xml)
        /// </summary>
        /// <param name="protocolToBeSave"></param>
        /// <param name="saveasProtocolName"></param>
        public bool SaveAppProtocol(ProtocolViewModel protocolToBeSave, string saveasProtocolName)
        {
            string filePath = System.IO.Path.Combine(Workspace.This.AppDataPath, _ProtocolConfigFile);

            XmlDocument XDoc = new XmlDocument();
            //XDoc.PreserveWhitespace = true;   //this flag override auto indent settings
            XmlNode protocolsParentNode = null;
            bool bUserConfigExists = false;

            if (System.IO.File.Exists(filePath))
            {
                XDoc.Load(filePath);
                XmlNodeList nodeList = XDoc.GetElementsByTagName("PhosphorProtocols");
                protocolsParentNode = nodeList.Item(0);
                bUserConfigExists = true;
            }

            if (!bUserConfigExists)
            {
                var comment = XDoc.CreateComment("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
                XmlNode rootNode = XDoc.CreateElement("Config");
                XDoc.InsertBefore(comment, rootNode);
                XDoc.AppendChild(rootNode);

                protocolsParentNode = XDoc.CreateElement("PhosphorProtocols");
                rootNode.AppendChild(protocolsParentNode);
            }

            XmlNode protocolNode = XDoc.CreateElement("PhosphorProtocol");
            protocolsParentNode.AppendChild(protocolNode);

            XmlAttribute xmlAttrib = XDoc.CreateAttribute("DisplayName");
            xmlAttrib.Value = saveasProtocolName; //new protocol name
            protocolNode.Attributes.Append(xmlAttrib);

            foreach (var scanRegion in protocolToBeSave.ScanRegions)
            {
                if (scanRegion.SelectedPixelSize == null ||
                    scanRegion.SelectedSampleType == null ||
                    scanRegion.SelectedScanSpeed == null ||
                    scanRegion.SelectedSignalLevel == null)
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show(string.Format("Please make sure all the scanning options are selected [{0}].", scanRegion.ScanRegionHeader));
                    return false;
                }

                XmlNode scanRegionNode = XDoc.CreateElement("ScanRegion");
                protocolNode.AppendChild(scanRegionNode);

                xmlAttrib = XDoc.CreateAttribute("SampleType");
                xmlAttrib.Value = scanRegion.SelectedSampleType.Position.ToString();
                scanRegionNode.Attributes.Append(xmlAttrib);

                xmlAttrib = XDoc.CreateAttribute("PixelSize");
                xmlAttrib.Value = scanRegion.SelectedPixelSize.Position.ToString();
                scanRegionNode.Attributes.Append(xmlAttrib);

                xmlAttrib = XDoc.CreateAttribute("ScanSpeed");
                xmlAttrib.Value = scanRegion.SelectedScanSpeed.Position.ToString();
                scanRegionNode.Attributes.Append(xmlAttrib);

                // Phosphor Imaging tab no longer has the scan Quality selection option
                //xmlAttrib = XDoc.CreateAttribute("ScanQuality");
                //xmlAttrib.Value = scanRegion.SelectedScanQuality.Position.ToString();
                //scanRegionNode.Attributes.Append(xmlAttrib);

                xmlAttrib = XDoc.CreateAttribute("IntensityLevel");
                xmlAttrib.Value = scanRegion.SelectedSignalLevel.IntensityLevel.ToString();
                scanRegionNode.Attributes.Append(xmlAttrib);

                double x = Math.Round(scanRegion.ScanRect.X / _ImagingVm.CellSize, 2);
                double y = Math.Round(scanRegion.ScanRect.Y / _ImagingVm.CellSize, 2);
                double w = Math.Round(scanRegion.ScanRect.Width / _ImagingVm.CellSize, 2);
                double h = Math.Round(scanRegion.ScanRect.Height / _ImagingVm.CellSize, 2);
                ScanRegionRect scanRect = new ScanRegionRect(x, y, w, h);
                xmlAttrib = XDoc.CreateAttribute("ScanRegionRect");
                xmlAttrib.Value = scanRect.ToString();
                scanRegionNode.Attributes.Append(xmlAttrib);

                if (scanRegion.FileLocationVm.IsAutoSave)
                {
                    xmlAttrib = XDoc.CreateAttribute("IsAutoSave");
                    xmlAttrib.Value = scanRegion.FileLocationVm.IsAutoSave.ToString();
                    scanRegionNode.Attributes.Append(xmlAttrib);
                    XmlNode nodeAutoSave = XDoc.CreateElement("AutoSave");
                    scanRegionNode.AppendChild(nodeAutoSave);
                    xmlAttrib = XDoc.CreateAttribute("Path");
                    xmlAttrib.Value = scanRegion.FileLocationVm.DestinationFolder;
                    nodeAutoSave.Attributes.Append(xmlAttrib);
                }
            }   //closing: foreach

            XDoc.Beautify();
            XDoc.Save(filePath);

            return true;
        }

        /// <summary>
        /// Delete selected protocol from config file (protocols.xml)
        /// </summary>
        /// <param name="selectedProtocol"></param>
        public void DeleteAppProtocol(string selectedProtocol)
        {
            string configFilePath = System.IO.Path.Combine(Workspace.This.AppDataPath, _ProtocolConfigFile);
            bool bIsProtocolRemoved = false;

            // find and remove from 'config.xml'
            if (System.IO.File.Exists(configFilePath))
            {
                XmlDocument doc = new XmlDocument();
                //doc.PreserveWhitespace = true;    //this flag override auto indent settings
                doc.Load(configFilePath);

                XmlNodeList elemList = doc.GetElementsByTagName("PhosphorProtocol");
                for (int i = 0; i < elemList.Count; i++)
                {
                    string attrVal = elemList[i].Attributes["DisplayName"].Value;
                    if (attrVal == selectedProtocol)
                    {
                        elemList[i].ParentNode.RemoveChild(elemList[i]);
                        bIsProtocolRemoved = true;
                        break;
                    }
                }

                if (bIsProtocolRemoved)
                {
                    doc.Beautify();
                    doc.Save(configFilePath);
                    string caption = "Protocol deleted...";
                    string message = string.Format("Protocol \"{0}\" deleted.", selectedProtocol);
                    Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

    }
}
