using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Azure.WPF.Framework;
using System.Windows;   //Rect
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging; //BitmapSource
using Azure.Adorners;
using Azure.EthernetCommLib;
using Azure.Image.Processing;   //ImageInfo
using Azure.ImagingSystem;
using CroppingImageLibrary.Services;

namespace Azure.LaserScanner.ViewModel
{
    class ImagingViewModel : ViewModelBase
    {
        // Image capture status delegate
        public delegate void ChannelChangedHandler(object sender, LaserChannels laserChannel, bool bOnOffFlag);
        // Image capture status event
        public event ChannelChangedHandler ChannelChanged;

        public delegate void SelectedImagingRegionChangeHandle(object sender, EventArgs e);
        public event SelectedImagingRegionChangeHandle SelectedImagingRegionChange;

        public delegate void ShowPreviewChannelsClickedHandler(object sender);
        public event ShowPreviewChannelsClickedHandler ShowPreviewChannelsClicked;

        public delegate void UpdateDisplayImagedHandler(object sender, ImageInfo imageInfo);
        public event UpdateDisplayImagedHandler UpdateDisplayImage;

        public delegate void ScanRegionChangedHandler(object sender);
        public event ScanRegionChangedHandler ScanRegionChanged;


        #region Private data...

        private Rect _SelectedImagingRegion;
        private BitmapSource _PreviewImage = null;
        //private ImageInfo _PreviewImageInfo = new ImageInfo();
        private double _NumOfCells = 26.0;
        private double _CellSize = 0.0;

        List<LaserChannels> _PreviewChannels = null;

        private bool _IsLaserL1PrvSelected = false;
        private bool _IsLaserR1PrvSelected = false;
        private bool _IsLaserR2PrvSelected = false;
        private bool _IsLaserL1PrvVisible = false;
        private bool _IsLaserR1PrvVisible = false;
        private bool _IsLaserR2PrvVisible = false;
        private string _LaserL1Wavelength = "685";
        private string _LaserR1Wavelength = "532";
        private string _LaserR2Wavelength = "488";
        private ImageChannelType _LaserL1ColorChannel;
        private ImageChannelType _LaserR1ColorChannel;
        private ImageChannelType _LaserR2ColorChannel;
        private bool _IsContrastChannelAllowed = false;
        private bool _IsContrastLaserL1Channel = false;
        private bool _IsContrastLaserR1Channel = false;
        private bool _IsContrastLaserR2Channel = false;

        //private ColorChannel _SelectedColorChannel;
        private ContrastViewModel _ContrastVm = null;

        private ObservableCollection<ImageItem> _PreviewImages = null;
        //private AdornerLayer _AdornerLayer; //scan region adorner
        private FrameworkElement _DisplayCanvas;
        private List<CropService> _CropServices;
        private bool _IsAdornerEnabled = true;

        #endregion

        #region Constructors...

        public ImagingViewModel()
        {
            _ContrastVm = new ContrastViewModel();
            _PreviewChannels = new List<LaserChannels>();
            if (Workspace.This != null)
            {
                _LaserL1Wavelength = Workspace.This.LaserChannelTypeList[LaserChannels.ChannelC].ToString();
                _LaserR1Wavelength = Workspace.This.LaserChannelTypeList[LaserChannels.ChannelA].ToString();
                _LaserR2Wavelength = Workspace.This.LaserChannelTypeList[LaserChannels.ChannelB].ToString();
            }

            _PreviewImages = new ObservableCollection<ImageItem>();
            _CropServices = new List<CropService>();
        }

        #endregion

        #region Pubic properties...

        public ContrastViewModel ContrastVm
        {
            get { return _ContrastVm; }
        }

        public Rect SelectedImagingRegion
        {
            get
            {
                return _SelectedImagingRegion;
            }
            set
            {
                _SelectedImagingRegion = value;
                RaisePropertyChanged("SelectedImagingRegion");
                if (this.SelectedImagingRegionChange != null)
                {
                    SelectedImagingRegionChange(this, new EventArgs());
                }
            }
        }

        public BitmapSource PreviewImage
        {
            get
            {
                return _PreviewImage;
            }
            set
            {
                _PreviewImage = value;
                RaisePropertyChanged("PreviewImage");
            }
        }

        //public ImageInfo PreviewImageInfo
        //{
        //    get { return _PreviewImageInfo; }
        //}

        public double NumOfCells
        {
            get { return _NumOfCells; }
            set
            {
                if (_NumOfCells != value)
                {
                    _NumOfCells = value;
                    RaisePropertyChanged("NumOfCells");
                }
            }
        }

        public double CellSize
        {
            get { return _CellSize; }
            set
            {
                _CellSize = value;
                RaisePropertyChanged("CellSize");
            }
        }

        //public Rect ThumbRect
        //{
        //    get { return _ThumbRect; }
        //    set
        //    {
        //        _ThumbRect = value;
        //        RaisePropertyChanged("ThumbRect");
        //    }
        //}

        public List<LaserChannels> PreviewChannels
        {
            get
            {
                return _PreviewChannels;
            }
        }

        public bool IsLaserL1PrvSelected
        {
            get { return _IsLaserL1PrvSelected; }
            set
            {
                if (_IsLaserL1PrvSelected != value)
                {
                    _IsLaserL1PrvSelected = value;
                    if (_IsLaserL1PrvSelected)
                    {
                        if (!_IsContrastLaserL1Channel)
                        {
                            _IsContrastLaserL1Channel = true;
                            RaisePropertyChanged("IsContrastLaserL1Channel");
                            SetContrastColorChannel(_LaserL1ColorChannel, _IsContrastLaserL1Channel);
                        }
                        _PreviewChannels.Add(LaserChannels.ChannelC);
                    }
                    else
                    {
                        _PreviewChannels.Remove(LaserChannels.ChannelC);
                        if (_IsContrastLaserL1Channel)
                        {
                            _IsContrastLaserL1Channel = false;
                            RaisePropertyChanged("IsContrastLaserL1Channel");
                            SetContrastColorChannel(_LaserL1ColorChannel, _IsContrastLaserL1Channel);
                        }
                    }
                    RaisePropertyChanged("IsLaserL1PrvSelected");

                    SetPreviewDisplayChannel(_LaserL1ColorChannel, _IsLaserL1PrvSelected);

                    // if anyone has subscribed, notify them of selection changed.
                    if (ChannelChanged != null)
                    {
                        ChannelChanged(this, LaserChannels.ChannelC, _IsLaserL1PrvSelected);
                    }
                }
            }
        }
        public bool IsLaserR1PrvSelected
        {
            get { return _IsLaserR1PrvSelected; }
            set
            {
                if (_IsLaserR1PrvSelected != value)
                {
                    _IsLaserR1PrvSelected = value;
                    if (_IsLaserR1PrvSelected)
                    {
                        if (!_IsContrastLaserR1Channel)
                        {
                            _IsContrastLaserR1Channel = true;
                            RaisePropertyChanged("IsContrastLaserR1Channel");
                            SetContrastColorChannel(_LaserR1ColorChannel, _IsContrastLaserR1Channel);
                        }
                        _PreviewChannels.Add(LaserChannels.ChannelA);
                    }
                    else
                    {
                        _PreviewChannels.Remove(LaserChannels.ChannelA);
                        if (_IsContrastLaserR1Channel)
                        {
                            _IsContrastLaserR1Channel = false;
                            RaisePropertyChanged("IsContrastLaserR1Channel");
                            SetContrastColorChannel(_LaserR1ColorChannel, _IsContrastLaserR1Channel);
                        }
                    }
                    RaisePropertyChanged("IsLaserR1PrvSelected");

                    SetPreviewDisplayChannel(_LaserR1ColorChannel, _IsLaserR1PrvSelected);

                    // if anyone has subscribed, notify them
                    if (ChannelChanged != null)
                    {
                        ChannelChanged(this, LaserChannels.ChannelA, _IsLaserR1PrvSelected);
                    }
                }
            }
        }
        public bool IsLaserR2PrvSelected
        {
            get { return _IsLaserR2PrvSelected; }
            set
            {
                if (_IsLaserR2PrvSelected != value)
                {
                    _IsLaserR2PrvSelected = value;
                    if (_IsLaserR2PrvSelected)
                    {
                        if (!_IsContrastLaserR2Channel)
                        {
                            _IsContrastLaserR2Channel = true;
                            RaisePropertyChanged("IsContrastLaserR2Channel");
                            SetContrastColorChannel(_LaserR2ColorChannel, _IsContrastLaserR2Channel);
                        }
                        _PreviewChannels.Add(LaserChannels.ChannelB);
                    }
                    else
                    {
                        _PreviewChannels.Remove(LaserChannels.ChannelB);
                        if (_IsContrastLaserR2Channel)
                        {
                            _IsContrastLaserR2Channel = false;
                            RaisePropertyChanged("IsContrastLaserR2Channel");
                            SetContrastColorChannel(_LaserR2ColorChannel, _IsContrastLaserR2Channel);
                        }

                    }
                    RaisePropertyChanged("IsLaserR2PrvSelected");

                    SetPreviewDisplayChannel(_LaserR2ColorChannel, _IsLaserR2PrvSelected);

                    // if anyone has subscribed, notify them
                    if (ChannelChanged != null)
                    {
                        ChannelChanged(this, LaserChannels.ChannelB, _IsLaserR2PrvSelected);
                    }
                }
            }
        }

        private void SetPreviewDisplayChannel(ImageChannelType colorChannel, bool bFlag)
        {
            switch (colorChannel)
            {
                case ImageChannelType.Red:
                    _ContrastVm.DisplayImageInfo.IsDisplayRedChannel = bFlag;
                    break;
                case ImageChannelType.Green:
                    _ContrastVm.DisplayImageInfo.IsDisplayGreenChannel = bFlag;
                    break;
                case ImageChannelType.Blue:
                    _ContrastVm.DisplayImageInfo.IsDisplayBlueChannel = bFlag;
                    break;
                case ImageChannelType.Gray:
                    _ContrastVm.DisplayImageInfo.IsDisplayGrayChannel = bFlag;
                    break;
            }
            //_NumOfDisplayChannel = 0;
            //if (_ContrastVm.DisplayImageInfo.IsDisplayRedChannel)
            //    _NumOfDisplayChannel++;
            //if (_ContrastVm.DisplayImageInfo.IsDisplayGreenChannel)
            //    _NumOfDisplayChannel++;
            //if (_ContrastVm.DisplayImageInfo.IsDisplayBlueChannel)
            //    _NumOfDisplayChannel++;
            //if (_ContrastVm.DisplayImageInfo.IsDisplayGrayChannel)
            //    _NumOfDisplayChannel++;
            //_ContrastVm.NumOfDisplayChannels = _NumOfDisplayChannel;
        }
        /// <summary>
        /// Number of selected display/preview channels.
        /// </summary>
        public int NumOfDisplayChannels
        {
            get
            {
                int nNumOfDisplayChannels = 0;
                if (_ContrastVm.DisplayImageInfo.IsDisplayRedChannel)
                    nNumOfDisplayChannels++;
                if (_ContrastVm.DisplayImageInfo.IsDisplayGreenChannel)
                    nNumOfDisplayChannels++;
                if (_ContrastVm.DisplayImageInfo.IsDisplayBlueChannel)
                    nNumOfDisplayChannels++;
                if (_ContrastVm.DisplayImageInfo.IsDisplayGrayChannel)
                    nNumOfDisplayChannels++;
                //_ContrastVm.NumOfDisplayChannels = _NumOfDisplayChannels;
                return nNumOfDisplayChannels;
            }
        }

        public bool IsLaserL1PrvVisible
        {
            get { return _IsLaserL1PrvVisible; }
            set
            {
                _IsLaserL1PrvVisible = value;
                RaisePropertyChanged("IsLaserL1PrvVisible");
                RaisePropertyChanged("LaserL1ColorChannel");
                if (!_IsLaserL1PrvVisible)
                {
                    IsLaserL1PrvSelected = false;
                }
            }
        }
        public bool IsLaserR1PrvVisible
        {
            get { return _IsLaserR1PrvVisible; }
            set
            {
                _IsLaserR1PrvVisible = value;
                RaisePropertyChanged("IsLaserR1PrvVisible");
                RaisePropertyChanged("LaserR1ColorChannel");
                if (!_IsLaserR1PrvVisible)
                {
                    IsLaserR1PrvSelected = false;
                }
            }
        }
        public bool IsLaserR2PrvVisible
        {
            get { return _IsLaserR2PrvVisible; }
            set
            {
                _IsLaserR2PrvVisible = value;
                RaisePropertyChanged("IsLaserR2PrvVisible");
                RaisePropertyChanged("LaserR2ColorChannel");
                if (!_IsLaserR2PrvVisible)
                {
                    IsLaserR2PrvSelected = false;
                }
            }
        }

        public ImageChannelType LaserL1ColorChannel
        {
            get { return _LaserL1ColorChannel; }
            set
            {
                _LaserL1ColorChannel = value;
                RaisePropertyChanged("LaserL1ColorChannel");
            }
        }
        public ImageChannelType LaserR1ColorChannel
        {
            get { return _LaserR1ColorChannel; }
            set
            {
                _LaserR1ColorChannel = value;
                RaisePropertyChanged("LaserR1ColorChannel");
            }
        }
        public ImageChannelType LaserR2ColorChannel
        {
            get { return _LaserR2ColorChannel; }
            set
            {
                _LaserR2ColorChannel = value;
                RaisePropertyChanged("LaserR2ColorChannel");
            }
        }

        public bool IsContrastChannelAllowed
        {
            get { return _IsContrastChannelAllowed; }
            set
            {
                _IsContrastChannelAllowed = value;
                RaisePropertyChanged("IsContrastChannelAllowed");
            }
        }
        public bool IsContrastLaserL1Channel
        {
            get { return _IsContrastLaserL1Channel; }
            set
            {
                _IsContrastLaserL1Channel = value;
                RaisePropertyChanged("IsContrastLaserL1Channel");

                SetContrastColorChannel(_LaserL1ColorChannel, _IsContrastLaserL1Channel);
                if (_IsContrastLaserL1Channel && !_IsLaserL1PrvSelected)
                {
                    IsLaserL1PrvSelected = true;
                }
            }
        }
        public bool IsContrastLaserR1Channel
        {
            get { return _IsContrastLaserR1Channel; }
            set
            {
                _IsContrastLaserR1Channel = value;
                RaisePropertyChanged("IsContrastLaserR1Channel");

                SetContrastColorChannel(_LaserR1ColorChannel, _IsContrastLaserR1Channel);
                if (_IsContrastLaserR1Channel && !_IsLaserR1PrvSelected)
                {
                    IsLaserR1PrvSelected = true;
                }
            }
        }
        public bool IsContrastLaserR2Channel
        {
            get { return _IsContrastLaserR2Channel; }
            set
            {
                _IsContrastLaserR2Channel = value;
                RaisePropertyChanged("IsContrastLaserR2Channel");

                SetContrastColorChannel(_LaserR2ColorChannel, _IsContrastLaserR2Channel);
                if (_IsContrastLaserR2Channel && !_IsLaserR2PrvSelected)
                {
                    IsLaserR2PrvSelected = true;
                }
            }
        }
        public void SetContrastColorChannel(ImageChannelType colorChannel, bool bFlag)
        {
            switch (colorChannel)
            {
                case ImageChannelType.Red:
                    _ContrastVm.DisplayImageInfo.IsContrastRedChannel = bFlag;
                    break;
                case ImageChannelType.Green:
                    _ContrastVm.DisplayImageInfo.IsContrastGreenChannel = bFlag;
                    break;
                case ImageChannelType.Blue:
                    _ContrastVm.DisplayImageInfo.IsContrastBlueChannel = bFlag;
                    break;
                case ImageChannelType.Gray:
                    if (bFlag)
                    {
                        if (_ContrastVm.IsRgbImage)
                        {
                            _ContrastVm.DisplayImageInfo.GrayChannel.BlackValue = _ContrastVm.BlackValue;
                            _ContrastVm.DisplayImageInfo.GrayChannel.WhiteValue = _ContrastVm.WhiteValue;
                        }
                        else
                        {
                            _ContrastVm.DisplayImageInfo.MixChannel.BlackValue = _ContrastVm.BlackValue;
                            _ContrastVm.DisplayImageInfo.MixChannel.WhiteValue = _ContrastVm.WhiteValue;
                        }
                    }
                    if (_ContrastVm.IsRgbImage)
                        _ContrastVm.DisplayImageInfo.IsContrastGrayChannel = bFlag;
                    break;
                    //default:
                    //    if (bFlag)
                    //    {
                    //        _ContrastVm.DisplayImageInfo.MixChannel.BlackValue = _ContrastVm.BlackValue;
                    //        _ContrastVm.DisplayImageInfo.MixChannel.WhiteValue = _ContrastVm.WhiteValue;
                    //    }
                    //    break;
            }
            //_NumOfContrastChannel = 0;
            //if (_ContrastVm.DisplayImageInfo.IsContrastRedChannel)
            //    _NumOfContrastChannel++;
            //if (_ContrastVm.DisplayImageInfo.IsContrastGreenChannel)
            //    _NumOfContrastChannel++;
            //if (_ContrastVm.DisplayImageInfo.IsContrastBlueChannel)
            //    _NumOfContrastChannel++;
            //if (_ContrastVm.DisplayImageInfo.IsContrastGrayChannel)
            //    _NumOfContrastChannel++;
            //_ContrastVm.NumOfContrastChannels = _NumOfContrastChannel;

            if (NumOfContrastChannels == 1)
            {
                if (_ContrastVm.DisplayImageInfo.IsContrastRedChannel)
                {
                    _ContrastVm.BlackValue = _ContrastVm.DisplayImageInfo.RedChannel.BlackValue;
                    _ContrastVm.WhiteValue = _ContrastVm.DisplayImageInfo.RedChannel.WhiteValue;
                }
                else if (_ContrastVm.DisplayImageInfo.IsContrastGreenChannel)
                {
                    _ContrastVm.BlackValue = _ContrastVm.DisplayImageInfo.GreenChannel.BlackValue;
                    _ContrastVm.WhiteValue = _ContrastVm.DisplayImageInfo.GreenChannel.WhiteValue;
                }
                else if (_ContrastVm.DisplayImageInfo.IsContrastBlueChannel)
                {
                    _ContrastVm.BlackValue = _ContrastVm.DisplayImageInfo.BlueChannel.BlackValue;
                    _ContrastVm.WhiteValue = _ContrastVm.DisplayImageInfo.BlueChannel.WhiteValue;
                }
                else if (_ContrastVm.DisplayImageInfo.IsContrastGrayChannel)
                {
                    _ContrastVm.BlackValue = _ContrastVm.DisplayImageInfo.GrayChannel.BlackValue;
                    _ContrastVm.WhiteValue = _ContrastVm.DisplayImageInfo.GrayChannel.WhiteValue;
                }
            }
        }
        /// <summary>
        /// Number of selected contrast channels.
        /// </summary>
        public int NumOfContrastChannels
        {
            get
            {
                int nNumOfContrastChannels = 0;
                if (_ContrastVm.DisplayImageInfo.IsContrastRedChannel)
                    nNumOfContrastChannels++;
                if (_ContrastVm.DisplayImageInfo.IsContrastGreenChannel)
                    nNumOfContrastChannels++;
                if (_ContrastVm.DisplayImageInfo.IsContrastBlueChannel)
                    nNumOfContrastChannels++;
                if (_ContrastVm.DisplayImageInfo.IsContrastGrayChannel)
                    nNumOfContrastChannels++;
                //_ContrastVm.NumOfContrastChannels = _NumOfContrastChannel;
                return nNumOfContrastChannels;
            }
        }

        public bool IsEnablePreviewContrast
        {
            get
            {
                if (_PreviewImage != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public string LaserL1Wavelength
        {
            get { return _LaserL1Wavelength; }
            set
            {
                _LaserL1Wavelength = value;
                RaisePropertyChanged("LaserL1Wavelength");
            }
        }
        public string LaserR1Wavelength
        {
            get { return _LaserR1Wavelength; }
            set
            {
                _LaserR1Wavelength = value;
                RaisePropertyChanged("LaserR1Wavelength");
            }
        }
        public string LaserR2Wavelength
        {
            get { return _LaserR2Wavelength; }
            set
            {
                _LaserR2Wavelength = value;
                RaisePropertyChanged("LaserR2Wavelength");
            }
        }

        public bool IsInitializing { get; set; } = false;

        public ObservableCollection<ImageItem> PreviewImages
        {
            get { return _PreviewImages; }
            set
            {
                _PreviewImages = value;
                RaisePropertyChanged("PreviewImages");
            }
        }

        public FrameworkElement DisplayCanvas
        {
            get { return _DisplayCanvas; }
            set
            {
                _DisplayCanvas = value;
            }
        }
        //public AdornerLayer ScanRegionAdornerLayer
        //{
        //    get { return _AdornerLayer; }
        //    set { _AdornerLayer = value; }
        //}

        public List<CropService> CropServices
        {
            get { return _CropServices; }
            set
            {
                _CropServices = value;
                RaisePropertyChanged("CropServices");
            }
        }

        public bool IsAdornerEnabled
        {
            get { return _IsAdornerEnabled; }
            set
            {
                if (_DisplayCanvas != null)
                {
                    _IsAdornerEnabled = value;
                    RaisePropertyChanged("IsAdornerEnabled");
                    var adornerLayer = AdornerLayer.GetAdornerLayer(_DisplayCanvas);
                    adornerLayer.IsEnabled = _IsAdornerEnabled;
                }
            }
        }

        /// <summary>
        /// Add scan region adorner (CroppingAdorner)
        /// </summary>
        /// <param name="scanRegionId"></param>
        /// <param name="scanRect"></param>
        public void AddScanRegionSelection(int scanRegionId, ScanRegionRect scanRect, Color clr)
        {
            double numBlocks = this.NumOfCells;
            double x = 0;
            double y = 0;

            double width = _CellSize * 10.0;
            double height = _CellSize * 10.0;
            double maxWidth = _CellSize * numBlocks;
            double maxHeight = _CellSize * numBlocks;

            if (scanRect != null && scanRect.Width > 0 && scanRect.Height > 0)
            {
                x = scanRect.X;
                y = scanRect.Y;
                width = scanRect.Width;
                height = scanRect.Height;
            }

            Rect rcInterior = new Rect(
                x,
                y,
                width,
                height);

            CropService cropService = new CropService(_DisplayCanvas, scanRegionId, clr, new Rect(scanRect.X, scanRect.Y, scanRect.Width, scanRect.Height));
            cropService.CropChanged += CropService_CropChanged;
            CropServices.Add(cropService);
        }

        private void CropService_CropChanged(Object sender)
        {
            if (ScanRegionChanged != null)
            {
                CropService cropService = sender as CropService;
                ScanRegionChanged(cropService);
            }
        }

        /// <summary>
        /// Remove the scan region adorner (CropService/CropAdorner)
        /// </summary>
        /// <param name="scanRegion"></param>
        public void RemoveScanRegionSelection(ScanRegionViewModel scanRegion)
        {
            if (_DisplayCanvas != null)
            {
                // Remove ROI rectangle adorner
                var adornerLayer = AdornerLayer.GetAdornerLayer(_DisplayCanvas);
                Adorner[] adornerArray = adornerLayer.GetAdorners(_DisplayCanvas);
                foreach (var adorner in adornerArray)
                {
                    if (((CropAdorner)adorner).AdornerID == scanRegion.ScanRegionNum)
                    {
                        adornerLayer.Remove(adorner);
                        break;
                    }
                }
            }
        }

        public bool RemoveAllScanRegions()
        {
            bool bResult = false;
            try
            {
                if (_DisplayCanvas != null)
                {
                    var adornerLayer = AdornerLayer.GetAdornerLayer(_DisplayCanvas);
                    Adorner[] adornerArray = adornerLayer.GetAdorners(DisplayCanvas);
                    if (adornerArray == null || adornerArray.Length == 0)
                    {
                        bResult = true;
                    }
                    else
                    {
                        for (int index = adornerArray.Length - 1; index >= 0; index--)
                        {
                            adornerLayer.Remove(adornerArray[index]);
                        }
                        CropServices.Clear();

                        bResult = true;
                    }
                }
            }
            catch
            {
                bResult = false;
            }

            return bResult;
        }

        public int GetAdornerId()
        {
            int id = 1;
            var adornerLayer = AdornerLayer.GetAdornerLayer(_DisplayCanvas);
            Adorner[] adornerArray = adornerLayer.GetAdorners(_DisplayCanvas);
            if (adornerArray != null)
            {
                while (true)
                {
                    if (adornerArray.Any(x => ((CropAdorner)x).AdornerID == id))
                    {
                        id++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return id;
        }

        #endregion

        #region ShowPreviewContrastCommand
        private RelayCommand _ShowPreviewContrastCommand = null;
        public ICommand ShowPreviewContrastCommand
        {
            get
            {
                if (_ShowPreviewContrastCommand == null)
                {
                    _ShowPreviewContrastCommand = new RelayCommand((p) => ExecutePreviewContrastCommand(p), (p) => CanExecutePreviewContrastCommand(p));
                }

                return _ShowPreviewContrastCommand;
            }
        }

        private void ExecutePreviewContrastCommand(object parameter)
        {
            if (ShowPreviewChannelsClicked != null)
            {
                ShowPreviewChannelsClicked(this);
            }
        }

        private bool CanExecutePreviewContrastCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region public ICommand BlackValueChangedCommand

        private RelayCommand _BlackValueChangedCommand = null;
        public ICommand BlackValueChangedCommand
        {
            get
            {
                if (_BlackValueChangedCommand == null)
                {
                    _BlackValueChangedCommand = new RelayCommand(ExecuteBlackValueChangedCommand, CanExecuteBlackValueChangedCommand);
                }

                return _BlackValueChangedCommand;
            }
        }
        protected void ExecuteBlackValueChangedCommand(object parameter)
        {
            if (_ContrastVm.DisplayImageInfo.DisplayChannelFlags != ImageChannelFlag.None)
            {
                if (UpdateDisplayImage != null)
                {
                    _ContrastVm.ResetAuto();
                    if (!Workspace.This.IsScanning)
                    {
                        UpdateDisplayImage(this, _ContrastVm.DisplayImageInfo);
                    }
                }
            }
        }
        protected bool CanExecuteBlackValueChangedCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region public ICommand WhiteValueChangedCommand

        private RelayCommand _WhiteValueChangedCommand = null;
        public ICommand WhiteValueChangedCommand
        {
            get
            {
                if (_WhiteValueChangedCommand == null)
                {
                    _WhiteValueChangedCommand = new RelayCommand(ExecuteWhiteValueChangedCommand, CanExecuteWhiteValueChangedCommand);
                }

                return _WhiteValueChangedCommand;
            }
        }
        protected void ExecuteWhiteValueChangedCommand(object parameter)
        {
            if (_ContrastVm.DisplayImageInfo.DisplayChannelFlags != ImageChannelFlag.None)
            {
                if (UpdateDisplayImage != null)
                {
                    _ContrastVm.ResetAuto();
                    if (!Workspace.This.IsScanning)
                    {
                       UpdateDisplayImage(this, _ContrastVm.DisplayImageInfo);
                    }
                }
            }
        }
        protected bool CanExecuteWhiteValueChangedCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region public ICommand GammaValueChangedCommand

        private RelayCommand _GammaValueChangedCommand = null;
        public ICommand GammaValueChangedCommand
        {
            get
            {
                if (_GammaValueChangedCommand == null)
                {
                    _GammaValueChangedCommand = new RelayCommand(ExecuteGammaValueChangedCommand, CanExecuteGammaValueChangedCommand);
                }

                return _GammaValueChangedCommand;
            }
        }
        protected void ExecuteGammaValueChangedCommand(object parameter)
        {
            if (_ContrastVm.DisplayImageInfo.DisplayChannelFlags != ImageChannelFlag.None)
            {
                if (UpdateDisplayImage != null)
                {
                    // Reset auto-contrast on gamma changed?
                    _ContrastVm.ResetAuto();
                    if (!Workspace.This.IsScanning)
                    {
                        UpdateDisplayImage(this, _ContrastVm.DisplayImageInfo);
                    }
                }
            }
        }
        protected bool CanExecuteGammaValueChangedCommand(object parameter)
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
            //if (DisplayImage == null) { return; }
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
                    _ContrastVm.BlackValue = blackValue;
                    if (!Workspace.This.IsScanning)
                    {
                        ExecuteBlackValueChangedCommand(null);
                    }
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
            //if (DisplayImage == null) { return; }
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
                    _ContrastVm.WhiteValue = whiteValue;
                    if (!Workspace.This.IsScanning)
                    {
                        ExecuteWhiteValueChangedCommand(null);
                    }
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
                    _ContrastVm.GammaValue = gammaValue;
                    if (!Workspace.This.IsScanning)
                    {
                        ExecuteGammaValueChangedCommand(null);
                    }
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

        #region public ICommand AutoContrastCommand
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
        ///Flip vertical command implementation.
        /// </summary>
        /// <param name="parameter"></param>
        protected void ExecuteAutoContrastCommand(object parameter)
        {
            if (_ContrastVm.DisplayImageInfo != null)
            {
                if (_ContrastVm.IsRgbImage && _ContrastVm.DisplayImageInfo.ContrastChannelFlags == ImageChannelFlag.None) { return; }

                if (UpdateDisplayImage != null)
                {
                    if (_ContrastVm.DisplayImageInfo.IsContrastRedChannel)
                        _ContrastVm.DisplayImageInfo.RedChannel.IsAutoChecked = true;
                    if (_ContrastVm.DisplayImageInfo.IsContrastGreenChannel)
                        _ContrastVm.DisplayImageInfo.GreenChannel.IsAutoChecked = true;
                    if (_ContrastVm.DisplayImageInfo.IsContrastBlueChannel)
                        _ContrastVm.DisplayImageInfo.BlueChannel.IsAutoChecked = true;
                    if (_ContrastVm.DisplayImageInfo.IsContrastGrayChannel)
                        _ContrastVm.DisplayImageInfo.GrayChannel.IsAutoChecked = true;
                    if ((!_ContrastVm.DisplayImageInfo.IsContrastRedChannel && !_ContrastVm.DisplayImageInfo.IsContrastGreenChannel &&
                        !_ContrastVm.DisplayImageInfo.IsContrastBlueChannel && !_ContrastVm.DisplayImageInfo.IsContrastGrayChannel) || !_ContrastVm.IsRgbImage)
                    {
                        _ContrastVm.DisplayImageInfo.MixChannel.IsAutoChecked = true;
                    }
                    if (!Workspace.This.IsScanning)
                    {
                        UpdateDisplayImage(this, _ContrastVm.DisplayImageInfo);
                    }
                }
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
            return true;
        }
        #endregion

        #endregion


        #region Helper methods...

        #endregion
    }

    /*class PreviewLaserImageChannel
    {
        public LaserType Laser { get; set; }
        public ImageChannelType ImageChannel { get; set; }
        public PreviewLaserImageChannel()
        {
        }
        public PreviewLaserImageChannel(LaserType laser, ImageChannelType imageChannel)
        {
            this.Laser = laser;
            this.ImageChannel = imageChannel;
        }
    }*/

}
