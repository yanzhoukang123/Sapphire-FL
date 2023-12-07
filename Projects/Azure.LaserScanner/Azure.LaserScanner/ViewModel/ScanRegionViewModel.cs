using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Azure.WPF.Framework;
using Azure.ImagingSystem;
using Azure.Image.Processing;
using Azure.Configuration.Settings;
using CroppingImageLibrary.Services;

namespace Azure.LaserScanner.ViewModel
{
    class ScanRegionViewModel : ViewModelBase
    {
        //public delegate void ScanRegionSettingChangingDelegate(object sender);
        //public event ScanRegionSettingChangingDelegate ScanRegionSettingChanging;
        public delegate void ScanRegionSettingChangedDelegate(object sender, bool bIsSelectionChanged, bool bIsSignalChanged = false);
        public event ScanRegionSettingChangedDelegate ScanRegionSettingChanged;
        public delegate void ScanRectChangedDelegate(object sender, bool bIsRectChanged, bool bIsScanRectDragged);
        public event ScanRectChangedDelegate ScanRectChanged;

        private int _ScanRegionNum = 1;
        private string _ScanRegionHeader = string.Empty;
        private ScanRegionRect _ScanRect = null;

        private ObservableCollection<ResolutionType> _PixelSizeOptions = null;
        private ObservableCollection<ScanSpeedType> _ScanSpeedOptions = null;
        private ObservableCollection<ScanQualityType> _ScanQualityOptions = null;
        private ObservableCollection<SampleTypeSetting> _SampleTypeOptions = null;
        private ObservableCollection<SignalViewModel> _SignalList = null;

        private ResolutionType _SelectedPixelSize = null;
        private ScanSpeedType _SelectedScanSpeed = null;
        private ScanQualityType _SelectedScanQuality = null;
        private SampleTypeSetting _SelectedSampleType = null;
        private bool _Is5micronScanSelected = false;

        // Phosphor Imaging
        private ObservableCollection<SignalIntensity> _SignalLevelOptions = new ObservableCollection<SignalIntensity>();
        private SignalIntensity _SelectedSignalLevel;

        //private bool _IsModified = false;
        private bool _IsCustomFocus = false;
        private double _CustomFocusValue = 0.00;
        private bool _IsSequentialScanning = false;
        private bool _IsEdrScanning = false;
        private bool _IsZScan = false;
        private ZScanSetting _ZScanSetting = null;

        private double _FocusMinInMm = 0;
        private double _FocusMaxInMm = 0;

        // Save settings (for backup and restore)
        //private ResolutionType _SelectedPixelSizeBkup;
        //private ScanSpeedType _SelectedScanSpeedBkup;
        //private SampleTypeSetting _SelectedSampleTypeBkup;
        //private SignalIntensity _SelectedSignalLevelBkup;
        //private List<SignalViewModel> _SignalListBkup;
        //private ZScanSetting _ZScanSettingBkup;

        //private double _ScanTime = 0.0;
        private double _CellSize = 0.0;
        private double _NumOfCells = 0.0;

        private FileLocationViewModel _FileLocationVm = null;

        //private SolidColorBrush _Color;
        private List<System.Windows.Media.Color> _RegionColors;

        public ScanRegionViewModel()
        {
            _RegionColors = new List<System.Windows.Media.Color>();
            _RegionColors.Add(Colors.DeepSkyBlue);
            _RegionColors.Add(Colors.Blue);
            _RegionColors.Add(Colors.CornflowerBlue);
            _RegionColors.Add(Colors.Green);
            _RegionColors.Add(Colors.DarkViolet);
            _RegionColors.Add(Colors.LightBlue);
            _RegionColors.Add(Colors.LightSteelBlue);
            _RegionColors.Add(Colors.LightGreen);
            _RegionColors.Add(Colors.Orange);
            _RegionColors.Add(Colors.OrangeRed);
            _RegionColors.Add(Colors.Yellow);
            _RegionColors.Add(Colors.YellowGreen);
            

            _PixelSizeOptions = new ObservableCollection<ResolutionType>(SettingsManager.ConfigSettings.ResolutionOptions);
            _ScanSpeedOptions = new ObservableCollection<ScanSpeedType>(SettingsManager.ConfigSettings.ScanSpeedOptions);
            _ScanQualityOptions = new ObservableCollection<ScanQualityType>(SettingsManager.ConfigSettings.ScanQualityOptions);
            _SampleTypeOptions = new ObservableCollection<SampleTypeSetting>(SettingsManager.ConfigSettings.SampleTypeSettings);
            _SignalList = new ObservableCollection<SignalViewModel>();
            //_ZScanSetting = new ZScanSetting();

            double absFocusPos = Workspace.This.MotorVM.AbsZPos;
            if (SettingsManager.ConfigSettings.IsSimulationMode)
            {
                absFocusPos = 1.0; //Sapphire FL focus position is around 1
            }
            _FocusMinInMm = absFocusPos * (-1);
            _FocusMaxInMm = Math.Round((double)SettingsManager.ConfigSettings.ZMaxValue / (double)SettingsManager.ConfigSettings.ZMotorSubdivision, 2);
            _FocusMaxInMm -= absFocusPos;
            _ZScanSetting = new ZScanSetting(absFocusPos, _FocusMinInMm, _FocusMaxInMm);

            // Phosphor Imaging ONLY
            ObservableCollection<SignalIntensity> signalLevelOptions = new ObservableCollection<SignalIntensity>();
            SignalIntensity signalLevel = null;
            int nSignalLevels = 5;
            if (SettingsManager.ConfigSettings.PhosphorSignalOptions != null)
            {
                if (SettingsManager.ConfigSettings.PhosphorSignalOptions.Count > 0)
                {
                    nSignalLevels = SettingsManager.ConfigSettings.PhosphorSignalOptions.Count;
                }
            }
            for (int i = 1; i <= nSignalLevels; i++)
            {
                signalLevel = new SignalIntensity(i.ToString(), i);
                signalLevelOptions.Add(signalLevel);
            }

            _SignalLevelOptions = signalLevelOptions;

            // Add 'Custom' and 'Z-Scan' to SampleType options
            SampleTypeSetting customSampleType = new SampleTypeSetting();
            string customDisplayName = "Custom";
            customSampleType.DisplayName = customDisplayName;
            customSampleType.Position = _SampleTypeOptions.Count + 1;
            customSampleType.FocusPosition = 0;
            _SampleTypeOptions.Add(customSampleType);
            SampleTypeSetting zscanOption = new SampleTypeSetting();
            string zscanDisplayName = "Z-Scan";
            zscanOption.DisplayName = zscanDisplayName;
            zscanOption.Position = _SampleTypeOptions.Count + 1;
            zscanOption.FocusPosition = 0;
            _SampleTypeOptions.Add(zscanOption);

            _ScanRect = new ScanRegionRect();

            _FileLocationVm = new FileLocationViewModel();
            _FileLocationVm.DestinationFolder = SettingsManager.ApplicationSettings.InitialDirectory;
        }

        public object Clone()
        {
            ScanRegionViewModel clone = (ScanRegionViewModel)this.MemberwiseClone();
            clone._SelectedPixelSize = (ResolutionType)_SelectedPixelSize.Clone();
            clone._SelectedScanSpeed = (ScanSpeedType)_SelectedScanSpeed.Clone();
            clone._SelectedSampleType = (SampleTypeSetting)_SelectedSampleType.Clone();
            clone._ScanRect =  (ScanRegionRect)_ScanRect.Clone();
            if (_SignalList != null)
            {
                clone._SignalList = new ObservableCollection<SignalViewModel>();
                foreach (var signal in _SignalList)
                {
                    clone._SignalList.Add((SignalViewModel)signal.Clone());
                }
            }

            // Phosphor imaging only (Phosphor Imaging has no dye selection option).
            if (_SelectedSignalLevel != null)
            {
                clone._SelectedSignalLevel = (SignalIntensity)_SelectedSignalLevel.Clone();
            }

            if (_ZScanSetting != null)
            {
                clone._ZScanSetting = (ZScanSetting)_ZScanSetting.Clone();
            }

            return clone;
        }

        /// <summary>
        /// Scan region identifier (scan region number)
        /// </summary>
        public int ScanRegionNum
        {
            get { return _ScanRegionNum; }
            set
            {
                _ScanRegionNum = value;
                RaisePropertyChanged("ScanRegionNum");
                RaisePropertyChanged("ScanRegionHeader");
            }
        }

        public string ScanRegionHeader
        {
            get
            {
                return string.Format("Scan Region #{0}", _ScanRegionNum);
            }
        }

        public ScanRegionRect ScanRect
        {
            get { return _ScanRect; }
            set
            {
                _ScanRect = value;
                RaisePropertyChanged("ScanRect");
            }
        }

        public double X
        {
            get
            {
                return Math.Round(_ScanRect.X / _CellSize, 2);
            }
            set
            {
                if (_ScanRect.X != value * _CellSize)
                {
                    ScanRectChanged?.Invoke(this, false, IsScanRectDragged);

                    if (value > _NumOfCells)
                    {
                        value = _NumOfCells;
                    }
                    else if (value < 0)
                    {
                        value = 0;
                    }

                    _ScanRect.X = Math.Round(value * _CellSize, 2);
                    RaisePropertyChanged("X");

                    ScanRectChanged?.Invoke(this, true, IsScanRectDragged);
                }
            }
        }
        public double Y
        {
            get
            {
                return Math.Round(_ScanRect.Y / _CellSize, 2);
            }
            set
            {
                if (_ScanRect.Y != value * _CellSize)
                {
                    ScanRectChanged?.Invoke(this, false, IsScanRectDragged);

                    if (value > _NumOfCells)
                    {
                        value = _NumOfCells;
                    }
                    else if (value < 0)
                    {
                        value = 0;
                    }
                    _ScanRect.Y = Math.Round(value * _CellSize, 2);
                    RaisePropertyChanged("Y");

                    ScanRectChanged?.Invoke(this, true, IsScanRectDragged);

                    //ScanTime = GetScanTime();
                }
            }
        }
        public double Width
        {
            get
            {
                return Math.Round(_ScanRect.Width / _CellSize, 2);
            }
            set
            {
                if (_ScanRect.Width != value * _CellSize)
                {
                    ScanRectChanged?.Invoke(this, false, IsScanRectDragged);

                    if (_ScanRect.X / _CellSize + value > _NumOfCells)
                    {
                        value = _NumOfCells - (_ScanRect.X / _CellSize);
                    }
                    else if (value < 0)
                    {
                        value = 0;
                    }

                    _ScanRect.Width = Math.Round(value * _CellSize, 2);
                    RaisePropertyChanged("Width");

                    ScanRectChanged?.Invoke(this, true, IsScanRectDragged);
                }
            }
        }
        public double Height
        {
            get
            {
                return Math.Round(_ScanRect.Height / _CellSize, 2);
            }
            set
            {
                if (_ScanRect.Height != value * _CellSize)
                {
                    ScanRectChanged?.Invoke(this, false, IsScanRectDragged);

                    if (_ScanRect.Y / _CellSize + value > _NumOfCells)
                    {
                        value = _NumOfCells - (_ScanRect.Y / _CellSize);
                    }
                    else if (value < 0)
                    {
                        value = 0;
                    }

                    _ScanRect.Height = Math.Round(value * _CellSize, 2);
                    RaisePropertyChanged("Height");

                    ScanRectChanged?.Invoke(this, true, IsScanRectDragged);

                    //ScanTime = GetScanTime();
                }
            }
        }

        //public double ScanTime
        //{
        //    get { return _ScanTime; }
        //    set
        //    {
        //        _ScanTime = value;
        //        RaisePropertyChanged("ScanTime");
        //    }
        //}

        public bool IsScanRectDragged { get; set; } = false;
        public bool IsAutoMergeImages { get; set; } = false;

        public ObservableCollection<ResolutionType> PixelSizeOptions
        {
            get { return _PixelSizeOptions; }
            set
            {
                if (_PixelSizeOptions != value)
                {
                    _PixelSizeOptions = value;
                    RaisePropertyChanged("PixelSizeOptions");
                }
            }
        }

        public ObservableCollection<ScanSpeedType> ScanSpeedOptions
        {
            get { return _ScanSpeedOptions; }
            set
            {
                if (_ScanSpeedOptions != value)
                {
                    _ScanSpeedOptions = value;
                    RaisePropertyChanged("ScanSpeedOptions");
                }
            }
        }
        public ObservableCollection<ScanQualityType> ScanQualityOptions
        {
            get { return _ScanQualityOptions; }
            set
            {
                if (_ScanQualityOptions != value)
                {
                    _ScanQualityOptions = value;
                    RaisePropertyChanged("ScanQualityOptions");
                }
            }
        }

        public ObservableCollection<SampleTypeSetting> SampleTypeOptions
        {
            get { return _SampleTypeOptions; }
            set
            {
                if (_SampleTypeOptions != value)
                {
                    _SampleTypeOptions = value;
                    RaisePropertyChanged("SampleTypeOptions");
                }
            }
        }

        // Phosphor Imaging ONLY
        public ObservableCollection<SignalIntensity> SignalLevelOptions
        {
            get { return _SignalLevelOptions; }
            set
            {
                _SignalLevelOptions = value;
                RaisePropertyChanged("SignalLevelOptions");
            }
        }

        public ResolutionType SelectedPixelSize
        {
            get { return _SelectedPixelSize; }
            set
            {
                if (_SelectedPixelSize != value)
                {
                    if (_SelectedPixelSize != value && !IsInitializing)
                    {
                        ScanRegionSettingChanged?.Invoke(this, false);
                    }

                    _SelectedPixelSize = value;
                    RaisePropertyChanged("SelectedPixelSize");

                    if (_SelectedPixelSize != null)
                    {
                        if (_SelectedPixelSize.Value == 5)
                        {
                            int index = _ScanQualityOptions.ToList().FindIndex(x => x.DisplayName == "High");
                            if (index >= 0)
                            {
                                _SelectedScanQuality = _ScanQualityOptions[index];
                                RaisePropertyChanged("SelectedScanQuality");
                            }
                            _Is5micronScanSelected = true;
                            RaisePropertyChanged("Is5micronScanSelected");
                        }
                        else
                        {
                            _Is5micronScanSelected = false;
                            RaisePropertyChanged("Is5micronScanSelected");
                        }
                    }

                    if (!IsInitializing)
                    {
                        ScanRegionSettingChanged?.Invoke(this, true);
                    }

                    //if (SelectedPixelSize != null)
                    //{
                    //    ScanTime = GetScanTime();
                    //}
                }
            }
        }

        public ScanSpeedType SelectedScanSpeed
        {
            get { return _SelectedScanSpeed; }
            set
            {
                if (_SelectedScanSpeed != value)
                {
                    if (_SelectedScanSpeed != value && !IsInitializing)
                    {
                        ScanRegionSettingChanged?.Invoke(this, false);
                    }

                    _SelectedScanSpeed = value;
                    RaisePropertyChanged("SelectedScanSpeed");

                    if (!IsInitializing)
                    {
                        ScanRegionSettingChanged?.Invoke(this, true);
                    }

                    //if (_SelectedScanSpeed != null)
                    //{
                    //    ScanTime = GetScanTime();
                    //}
                }
            }
        }

        public ScanQualityType SelectedScanQuality
        {
            get { return _SelectedScanQuality; }
            set
            {
                if (_SelectedScanQuality != value)
                {
                    if (_SelectedScanQuality != value && !IsInitializing)
                    {
                        ScanRegionSettingChanged?.Invoke(this, false);
                    }

                    _SelectedScanQuality = value;
                    RaisePropertyChanged("SelectedScanQuality");

                    if (!IsInitializing)
                    {
                        ScanRegionSettingChanged?.Invoke(this, true);
                    }
                }
            }
        }
        public bool Is5micronScanSelected
        {
            get { return _Is5micronScanSelected; }
            set
            {
                _Is5micronScanSelected = value;
                RaisePropertyChanged("Is5micronScanSelected");
            }
        }
        public SampleTypeSetting SelectedSampleType
        {
            get { return _SelectedSampleType; }
            set
            {
                if (_SelectedSampleType != value)
                {
                    if (_SelectedSampleType != null && !IsInitializing)
                    {
                        ScanRegionSettingChanged?.Invoke(this, false);
                    }

                    _SelectedSampleType = value;
                    RaisePropertyChanged("SelectedSampleType");

                    if (_SelectedSampleType != null)
                    {
                        if (_SelectedSampleType.DisplayName.ToLower() == "custom" ||
                            _SelectedSampleType.DisplayName.ToLower() == "z-scan")
                        {
                            if (_SelectedSampleType.DisplayName.ToLower() == "custom")
                            {
                                IsCustomFocus = true;
                            }
                            else if (_SelectedSampleType.DisplayName.ToLower() == "z-scan")
                            {
                                IsZScan = true;
                            }
                        }
                        else
                        {
                            IsCustomFocus = false;
                            IsZScan = false;
                        }
                    }
                }
            }
        }

        // Phosphor Imaging ONLY.
        public SignalIntensity SelectedSignalLevel
        {
            get { return _SelectedSignalLevel; }
            set
            {
                if (SelectedSignalLevel != null && !IsInitializing)
                {
                    ScanRegionSettingChanged?.Invoke(this, false);
                }

                _SelectedSignalLevel = value;
                RaisePropertyChanged("SelectedSignalLevel");

                if (!IsInitializing)
                {
                    ScanRegionSettingChanged?.Invoke(this, true);
                }

                // change the PMT's gain value in ApdVM (not write to device yet)
                if (Workspace.This != null)
                {
                    Signal signal = SettingsManager.ConfigSettings.PhosphorSignalOptions[SelectedSignalLevel.IntensityLevel - 1];  // config file 1 index
                    Workspace.This.ApdVM.ApdDGain = signal.ApdGain;
                }
            }
        }

        public ObservableCollection<SignalViewModel> SignalList
        {
            get { return _SignalList; }
            set
            {
                if (_SignalList != value)
                {
                    _SignalList = value;
                    RaisePropertyChanged("SignalList");
                    RaisePropertyChanged("IsSequentialScanAllowed");
                    if (_SignalList != null && _SignalList.Count < 2)
                    {
                        IsSequentialScanning = false;
                    }
                }
            }
        }

        public int SignalCount
        {
            get
            {
                int signalCount = 0;
                if (_SignalList != null)
                {
                    signalCount = _SignalList.Count;
                }
                return signalCount;
            }
        }

        /// <summary>
        /// Indicating property is initializing/restoring/reverting back
        /// to previous setting/value (to avoid trigger property changed)
        /// </summary>
        public bool IsInitializing { get; set; } = false;

        public bool IsSequentialScanAllowed
        {
            get
            {
                bool bIsSequentialScanAllowed = false;
                if (SignalCount > 1)
                {
                    bIsSequentialScanAllowed = true;
                }
                return bIsSequentialScanAllowed;
            }
        }

        public bool IsSequentialScanning
        {
            get { return _IsSequentialScanning; }
            set
            {
                if (_IsSequentialScanning != value)
                {
                    _IsSequentialScanning = value;
                    RaisePropertyChanged("IsSequentialScanning");

                    if (_IsSequentialScanning)
                    {
                        string caption = "Sapphire FL Biomolecular  Imager";
                        string message = "When Sequential Scanning is ON, each selected dye will be scanned individually and the scanning preview image will display the active scanning channel. Scan Time displayed is for a single channel.";
                        Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }

        public bool IsEdrScanning
        {
            get { return _IsEdrScanning; }
            set
            {
                if (_IsEdrScanning != value)
                {
                    if (!IsInitializing)
                    {
                        ScanRegionSettingChanged?.Invoke(this, false);
                    }

                    _IsEdrScanning = value;
                    RaisePropertyChanged("IsEdrScanning");

                    if (!IsInitializing)
                    {
                        ScanRegionSettingChanged?.Invoke(this, true);
                    }
                }
            }
        }

        public bool IsZScan
        {
            get { return _IsZScan; }
            set
            {
                if (_IsZScan != value)
                {
                    _IsZScan = value;
                    RaisePropertyChanged("IsZScan");
                    if (_IsZScan)
                    {
                        _IsCustomFocus = false;
                        RaisePropertyChanged("IsCustomFocus");
                    }
                }
            }
        }

        public ZScanSetting ZScanSetting
        {
            get { return _ZScanSetting; }
            set { _ZScanSetting = value; }
        }

        public double FocusMinInMm
        {
            get { return _FocusMinInMm; }
            set
            {
                _FocusMinInMm = value;
                RaisePropertyChanged("FocusMinInMm");
            }
        }
        public double FocusMaxInMm
        {
            get { return _FocusMaxInMm; }
            set
            {
                _FocusMaxInMm = value;
                RaisePropertyChanged("FocusMaxInMm");
            }
        }
        public bool IsCustomFocus
        {
            get { return _IsCustomFocus; }
            set
            {
                if (_IsCustomFocus != value)
                {
                    _IsCustomFocus = value;
                    RaisePropertyChanged("IsCustomFocus");
                    if (_IsCustomFocus)
                    {
                        _IsZScan = false;
                        RaisePropertyChanged("IsZScan");
                    }
                }
            }
        }

        public double CustomFocusValue
        {
            get { return _CustomFocusValue; }
            set
            {
                if (_CustomFocusValue != value)
                {
                    _CustomFocusValue = value;
                    if (_CustomFocusValue < _FocusMinInMm || _CustomFocusValue > _FocusMaxInMm)
                    {
                        if (_CustomFocusValue < _FocusMinInMm) _CustomFocusValue = _FocusMinInMm;
                        if (_CustomFocusValue > _FocusMaxInMm) _CustomFocusValue = _FocusMaxInMm;
                        string caption = "Custom focus...";
                        string message = string.Format("The focus value entered is out of range.\nThe focus value must be between {0} and {1}.", _FocusMinInMm, _FocusMaxInMm);
                        Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Stop);
                    }
                    RaisePropertyChanged("CustomFocusValue");
                }
            }
        }

        public double CellSize
        {
            get { return _CellSize; }
            set
            {
                _CellSize = value;
                RaisePropertyChanged("X");
                RaisePropertyChanged("Y");
                RaisePropertyChanged("Width");
                RaisePropertyChanged("Height");

                //ScanTime = GetScanTime();
            }
        }
        public double NumOfCells
        {
            get { return _NumOfCells; }
            set { _NumOfCells = value; }
        }

        public FileLocationViewModel FileLocationVm
        {
            get { return _FileLocationVm; }
        }

        public System.Windows.Media.Color Color
        {
            get
            {
                System.Windows.Media.Color clr;
                if (ScanRegionNum > 0 && ScanRegionNum <= _RegionColors.Count)
                {
                    clr = _RegionColors[ScanRegionNum - 1];
                }
                else
                {
                    if (ScanRegionNum > _RegionColors.Count &&
                        (ScanRegionNum - _RegionColors.Count) <= _RegionColors.Count)
                    {
                        clr = _RegionColors[ScanRegionNum - _RegionColors.Count- 1];
                    }
                    else
                    {
                        clr = _RegionColors[0];
                    }
                }
                return clr;
            }
            //set 
            //{
            //    _Color = value;
            //    RaisePropertyChanged("Color");
            //}
        }


        /*public bool IsModified
        {
            get { return _IsModified; }
            set
            {
                if (!_IsModified)
                {
                    //Backup protocol settings
                    BackupSettings();
                }
                else if (_IsModified && value == false)
                {
                    //Restore protocol settings
                    RestoreSettings();
                }
                _IsModified = value;
                RaisePropertyChanged("IsModified");
            }
        }*/
        /*public void BackupSettings()
        {
            _SelectedPixelSizeBkup = (ResolutionType)_SelectedPixelSize.Clone();
            _SelectedScanSpeedBkup = (ScanSpeedType)_SelectedScanSpeed.Clone();
            _SelectedSampleTypeBkup = (SampleTypeSetting)_SelectedSampleType.Clone();

            //if (_SelectedSignalLevel != null)
            //{
            //    _SelectedSignalLevelBkup = (SignalIntensity)_SelectedSignalLevel.Clone();
            //}

            if (_SignalList != null)
            {
                if (_SignalListBkup == null)
                {
                    _SignalListBkup = new List<SignalViewModel>();
                }
                else
                {
                    _SignalListBkup.Clear();
                }

                foreach (var signal in _SignalList)
                {
                    _SignalListBkup.Add((SignalViewModel)signal.Clone());
                }
            }

            if (_ZScanSetting != null)
            {
                _ZScanSettingBkup = (ZScanSetting)_ZScanSetting.Clone();
            }
        }*/
        /*public void RestoreSettings()
        {
            if (_SelectedPixelSizeBkup != null && _SelectedPixelSize != null &&
                (_SelectedPixelSizeBkup.Position != _SelectedPixelSize.Position))
            {
                var index = _PixelSizeOptions.ToList().FindIndex(x => x.Position == _SelectedPixelSizeBkup.Position);
                if (index >= 0)
                {
                    IsRestoring = true;
                    SelectedPixelSize = _PixelSizeOptions[index];
                    IsRestoring = false;
                    //RaisePropertyChanged("SelectedPixelSize");
                }
            }
            if (_SelectedScanSpeedBkup != null && _SelectedScanSpeed != null &&
                (_SelectedScanSpeedBkup.Position != _SelectedScanSpeed.Position))
            {
                var index = _ScanSpeedOptions.ToList().FindIndex(x => x.Position == _SelectedScanSpeedBkup.Position);
                if (index >= 0)
                {
                    IsRestoring = true;
                    SelectedScanSpeed = _ScanSpeedOptions[index];
                    IsRestoring = false;
                    //RaisePropertyChanged("SelectedScanSpeed");
                }
            }
            if (_SelectedSampleTypeBkup != null && _SelectedSampleType != null &&
                (_SelectedSampleTypeBkup.Position != _SelectedSampleType.Position))
            {
                var index = _SampleTypeOptions.ToList().FindIndex(x => x.Position == _SelectedSampleTypeBkup.Position);
                if (index >= 0)
                {
                    IsRestoring = true;
                    SelectedSampleType = _SampleTypeOptions[index];
                    IsRestoring = false;
                    //RaisePropertyChanged("SelectedScanSpeed");
                }
            }

            _SignalList.Clear();
            _SignalList = new ObservableCollection<SignalViewModel>(_SignalListBkup);
            for (int i = 0; i < _SignalListBkup.Count; i++)
            {
                // point the selected dye to the dyes list
                var dyeItem = _SignalListBkup[i].DyeOptions.Single(item => item.Position == _SignalListBkup[i].SelectedDye.Position);
                var index = _SignalListBkup[i].DyeOptions.IndexOf(dyeItem);
                if (index >= 0)
                {
                    _SignalList[i].IsReverting = true;
                    _SignalList[i].SelectedDye = _SignalList[i].DyeOptions[index];
                    _SignalList[i].IsReverting = false;
                }
                // point the selected intensity level to the intensities list
                var intItem = _SignalListBkup[i].SignalLevelOptions.Single(item => item.IntensityLevel == _SignalListBkup[i].SelectedSignalLevel.IntensityLevel);
                index = _SignalListBkup[i].SignalLevelOptions.IndexOf(intItem);
                if (index >= 0)
                {
                    _SignalList[i].IsReverting = true;
                    _SignalList[i].SelectedSignalLevel = _SignalList[i].SignalLevelOptions[index];
                    _SignalList[i].IsReverting = false;
                }
                // point the selected color channel to the color channels list
                var colorChItem = _SignalListBkup[i].ColorChannelOptions.Single(item => item.ImageColorChannel == _SignalListBkup[i].SelectedColorChannel.ImageColorChannel);
                index = _SignalListBkup[i].ColorChannelOptions.IndexOf(colorChItem);
                if (index >= 0)
                {
                    _SignalList[i].IsReverting = true;
                    _SignalList[i].SelectedColorChannel = _SignalList[i].ColorChannelOptions[index];
                    _SignalList[i].IsReverting = false;
                }
            }

            if (_ZScanSettingBkup != null)
            {
                _ZScanSetting = _ZScanSettingBkup;
            }
        }*/

        /*public void TriggerScanRegionUpdate()
        {
            //RaisePropertyChanged("SelectedPixelSize");
            //RaisePropertyChanged("SelectedScanSpeed");
            //RaisePropertyChanged("SelectedSignalLevel");
            //RaisePropertyChanged("SelectedSampleType");
            //RaisePropertyChanged("IsCustomFocus");
            //RaisePropertyChanged("IsZScan");
        }*/

        #region AddScanSignalCommand
        /*
        private RelayCommand _AddScanSignalCommand = null;
        public ICommand AddScanSignalCommand
        {
            get
            {
                if (_AddScanSignalCommand == null)
                {
                    _AddScanSignalCommand = new RelayCommand(ExecuteAddScanSignalCommand, CanExecuteAddScanSignalCommand);
                }

                return _AddScanSignalCommand;
            }
        }
        public void ExecuteAddScanSignalCommand(object parameter)
        {
            if (SignalCount < 4)
            {
                if (!IsInitializing)
                {
                    ScanRegionSettingChanged?.Invoke(this, false, false);
                }

                int nLaserIndex = 0;
                int nColorChannelIndex = 0;
                bool bAvailLaserFound = false;
                var signalList = new ObservableCollection<SignalViewModel>(SignalList);
                if (signalList.Count > 0)
                {
                    // Automatically select an un-use laser type
                    var bIsLaserAInUse = signalList.Any(item => item.SelectedDye.LaserType == LaserType.LaserA);
                    var bIsLaserBInUse = signalList.Any(item => item.SelectedDye.LaserType == LaserType.LaserB);
                    var bIsLaserCInUse = signalList.Any(item => item.SelectedDye.LaserType == LaserType.LaserC);
                    var bIsLaserDInUse = signalList.Any(item => item.SelectedDye.LaserType == LaserType.LaserD);
                    var bIsLaserAAvail = signalList[0].DyeOptions.Any(item => item.LaserType == LaserType.LaserA);
                    var bIsLaserBAvail = signalList[0].DyeOptions.Any(item => item.LaserType == LaserType.LaserB);
                    var bIsLaserCAvail = signalList[0].DyeOptions.Any(item => item.LaserType == LaserType.LaserC);
                    var bIsLaserDAvail = signalList[0].DyeOptions.Any(item => item.LaserType == LaserType.LaserD);
                    if (!bIsLaserAInUse && bIsLaserAAvail)
                    {
                        var itemsFound = signalList[0].DyeOptions.Where(item => item.LaserType == LaserType.LaserA).ToList();
                        if (itemsFound != null)
                        {
                            nLaserIndex = (int)signalList[0].DyeOptions.IndexOf(itemsFound[0]);
                            bAvailLaserFound = true;
                        }
                    }
                    else if (!bIsLaserBInUse && bIsLaserBAvail)
                    {
                        var itemsFound = signalList[0].DyeOptions.Where(item => item.LaserType == LaserType.LaserB).ToList();
                        if (itemsFound != null)
                        {
                            nLaserIndex = (int)signalList[0].DyeOptions.IndexOf(itemsFound[0]);
                            bAvailLaserFound = true;
                        }
                    }
                    else if (!bIsLaserCInUse && bIsLaserCAvail)
                    {
                        var itemsFound = signalList[0].DyeOptions.Where(item => item.LaserType == LaserType.LaserC).ToList();
                        if (itemsFound != null)
                        {
                            nLaserIndex = (int)signalList[0].DyeOptions.IndexOf(itemsFound[0]);
                            bAvailLaserFound = true;
                        }
                    }
                    else if (!bIsLaserDInUse && bIsLaserDAvail)
                    {
                        var itemsFound = signalList[0].DyeOptions.Where(item => item.LaserType == LaserType.LaserD).ToList();
                        if (itemsFound != null)
                        {
                            nLaserIndex = (int)signalList[0].DyeOptions.IndexOf(itemsFound[0]);
                            bAvailLaserFound = true;
                        }
                    }

                    if (bAvailLaserFound)
                    {
                        // Automatically select an un-use color channel
                        var bIsRedChannelInUse = signalList.Any(item => item.SelectedColorChannel.ImageColorChannel == ImageChannelType.Red);
                        var bIsGreenChannelInUse = signalList.Any(item => item.SelectedColorChannel.ImageColorChannel == ImageChannelType.Green);
                        var bIsBlueChannelInUse = signalList.Any(item => item.SelectedColorChannel.ImageColorChannel == ImageChannelType.Blue);
                        var bIsGrayChannelInUse = signalList.Any(item => item.SelectedColorChannel.ImageColorChannel == ImageChannelType.Gray);
                        if (!bIsRedChannelInUse)
                            nColorChannelIndex = 0;
                        else if (!bIsGreenChannelInUse)
                            nColorChannelIndex = 1;
                        else if (!bIsBlueChannelInUse)
                            nColorChannelIndex = 2;
                        else if (!bIsGrayChannelInUse)
                            nColorChannelIndex = 3;
                    }
                }

                if (bAvailLaserFound)
                {
                    SignalViewModel signal = new SignalViewModel(SettingsManager.ConfigSettings.LaserCSignalOptions.Count);
                    signal.DyeOptions = new ObservableCollection<DyeType>(SettingsManager.ConfigSettings.DyeOptions);
                    signal.SelectedDye = signal.DyeOptions[nLaserIndex];
                    signal.SelectedColorChannel = signal.ColorChannelOptions[nColorChannelIndex];
                    //signal.SelectedSignalLevel = signal.SignalLevelOptions[4];
                    signal.SelectedSignalLevel = signal.SignalLevelOptions.Single(item => item.DisplayName.Equals("1"));
                    signal.SignalChanged += Signal_SignalChanged; //EL: TODO:
                    signalList.Add(signal);
                    SignalList = new ObservableCollection<SignalViewModel>(signalList);

                    // Setup preview contrast channel
                    // Set color channel to newly selected laser type (dye)
                    //SetPreviewColorChannel(signal.SelectedDye, signal1.SelectedColorChannel.ImageColorChannel);  //EL: TODO:
                    // Show the newly selected laser type (or dye)
                    //SetPreviewLaserVisibility(signal1.SelectedDye.LaserType, true);   //EL: TODO:

                    if (!IsInitializing)
                    {
                        ScanRegionSettingChanged?.Invoke(this, true, true);
                    }
                }
                else
                {
                    string caption = "Sapphire Biomolecular Imager";
                    string message = "Cannot add another scan channel. All available lasers are in used";
                    Xceed.Wpf.Toolkit.MessageBox.Show(Workspace.This.Owner, message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                    return;
                }
            }
        }
        public bool CanExecuteAddScanSignalCommand(object parameter)
        {
            bool bResult = false;
            if (SignalCount < 4)
            {
                bResult = true;
            }
            return bResult;
        }
        */
        #endregion

        #region DeleteScanSignalCommand
        /*
        private RelayCommand _DeleteScanSignalCommand = null;
        public ICommand DeleteScanSignalCommand
        {
            get
            {
                if (_DeleteScanSignalCommand == null)
                {
                    _DeleteScanSignalCommand = new RelayCommand(ExecuteDeleteScanSignalCommand, CanExecuteDeleteScanSignalCommand);
                }

                return _DeleteScanSignalCommand;
            }
        }
        public void ExecuteDeleteScanSignalCommand(object parameter)
        {
            if (SignalList != null)
            {
                if (SignalList.Count > 1)
                {
                    SignalViewModel selectedSignal = parameter as SignalViewModel;
                    if (selectedSignal != null)
                    {
                        if (!IsInitializing)
                        {
                            ScanRegionSettingChanged?.Invoke(this, false, false);
                        }

                        ObservableCollection<SignalViewModel> signalList = new ObservableCollection<SignalViewModel>(SignalList);
                        selectedSignal.SignalChanged -= new SignalViewModel.SignalChangedDelegate(Signal_SignalChanged);
                        signalList.Remove(selectedSignal);
                        SignalList = signalList;

                        //if (selectedSignal.SelectedDye != null)
                        //{
                        //    ResetPreviewImages();
                        //    // Hide the preview laser type of the deleted signal
                        //    SetPreviewLaserVisibility(selectedSignal.SelectedDye.LaserType, false);
                        //}

                        if (!IsInitializing)
                        {
                            ScanRegionSettingChanged?.Invoke(this, true, true);
                        }
                    }
                }
            }
        }
        public bool CanExecuteDeleteScanSignalCommand(object parameter)
        {
            return true;
        }
        */
        #endregion

        #region public ICommand UpdateScanRectOnEnterCommand

        private RelayCommand _UpdateScanRectOnEnterCommand = null;
        public ICommand UpdateScanRectOnEnterCommand
        {
            get
            {
                if (_UpdateScanRectOnEnterCommand == null)
                {
                    _UpdateScanRectOnEnterCommand = new RelayCommand(ExecuteUpdateScanRectOnEnterCommand, CanExecuteUpdateScanRectOnEnterCommand);
                }

                return _UpdateScanRectOnEnterCommand;
            }
        }
        protected void ExecuteUpdateScanRectOnEnterCommand(object parameter)
        {
            System.Windows.Controls.TextBox txtBox = parameter as System.Windows.Controls.TextBox;
            if (txtBox != null)
            {
                DependencyProperty prop = System.Windows.Controls.TextBox.TextProperty;
                System.Windows.Data.BindingExpression binding = System.Windows.Data.BindingOperations.GetBindingExpression(txtBox, prop);
                if (binding != null)
                {
                    binding.UpdateSource();
                }
            }
        }
        protected bool CanExecuteUpdateScanRectOnEnterCommand(object parameter)
        {
            return true;
        }

        #endregion

        /*public void AddSignal(AppDyeData dye)
        {
            SignalViewModel signal = new SignalViewModel(SettingsManager.ConfigSettings.LaserASignalOptions.Count);
            signal.SignalChanged += new SignalViewModel.SignalChangedDelegate(Signal_SignalChanged);
            signal.DyeOptions = new ObservableCollection<DyeType>(SettingsManager.ConfigSettings.DyeOptions);
            signal.IsInitializing = true;
            if (signal != null && signal.DyeOptions != null && signal.DyeOptions.Count > 0)
            {
                // Now that we're allowoing the user to add and/or removing sample type, the sample type list may no longer be in sequential order
                //signal.SelectedDye = signal.DyeOptions[dye.DyeType - 1];
                signal.SelectedDye = GetDyeType(signal.DyeOptions, dye.DyeType);
                signal.SelectedSignalLevel = signal.SignalLevelOptions[dye.SignalIntensity - 1];
                for (int i = 0; i < signal.ColorChannelOptions.Count; i++)
                {
                    if (dye.ColorChannel == signal.ColorChannelOptions[i].ImageColorChannel)
                    {
                        signal.SelectedColorChannel = signal.ColorChannelOptions[i];
                        break;
                    }
                }
                signal.IsInitializing = false;
                SignalList.Add(signal);
            }
        }*/

        /// <summary>
        /// Returns dye type based on dye type position.
        /// </summary>
        /// <param name="nSampleTypePos"></param>
        /// <returns></returns>
        /*private DyeType GetDyeType(ObservableCollection<DyeType> dyeTypeOptions, int nDyeTypePos)
        {
            DyeType result = null;
            if (dyeTypeOptions != null && dyeTypeOptions.Count > 0)
            {
                for (int i = 0; i < dyeTypeOptions.Count; i++)
                {
                    if (dyeTypeOptions[i].Position == nDyeTypePos)
                    {
                        result = dyeTypeOptions[i];
                        break;
                    }
                }
            }
            return result;
        }*/

        /*private void Signal_SignalChanged(object sender, bool bIsSelectionChanged)
        {
            SignalViewModel signal = sender as SignalViewModel;
            if (signal != null)
            {
                // Don't check for duplicate selection if single scan channel.
                if (SignalList.Count > 1)
                {
                    // Selection is changing (not changed), check for duplicating setting
                    if (!bIsSelectionChanged)
                    {
                        if (signal.SelectingColorChannel != null)
                        {
                            // check for duplicate color channel
                            var colorChannelFound = SignalList.Any(item => item.SelectedColorChannel.ImageColorChannel == signal.SelectingColorChannel.ImageColorChannel);
                            if (colorChannelFound)
                            {
                                string caption = "Color channel already selected...";
                                string message = "The color channel is already selected in the current protocol.\nPlease select another color channel.";
                                Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Information);
                                Workspace.This.Owner.Dispatcher.BeginInvoke((Action)delegate
                                {
                                    signal.IsInitializing = true;
                                // Revert back to the previous selected color channel.
                                signal.SelectedColorChannel = signal.PrevSelectedColorChannel;
                                    signal.IsInitializing = false;
                                });
                                return;
                            }
                        }
                        else if (signal.SelectingDye != null)
                        {
                            // check for duplicate dye's laser type
                            var dyeFound = SignalList.Any(item => item.SelectedDye.LaserType == signal.SelectingDye.LaserType);
                            if (dyeFound)
                            {
                                if (signal.SelectedDye.LaserType != signal.SelectingDye.LaserType)
                                {
                                    string caption = "Laser already selected...";
                                    string message = "The dye's laser type is already selected in the current protocol.\nPlease select another dye.";
                                    Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Information);

                                    Workspace.This.Owner.Dispatcher.BeginInvoke((Action)delegate
                                    {
                                    // Prevent triggering of a selection changed
                                    // when reverting back to the previous selected dye.
                                    signal.IsInitializing = true;
                                    // Revert back to the previous selected dye.
                                    signal.SelectedDye = signal.PrevSelectedDye;
                                        signal.IsInitializing = false;
                                    });
                                    return;
                                }
                            }
                        }
                    }

                    if (!IsInitializing)
                    {
                        ScanRegionSettingChanged?.Invoke(this, bIsSelectionChanged, true);
                    }
                }
                else
                {
                }

                if (!IsInitializing)
                {
                    ScanRegionSettingChanged?.Invoke(this, bIsSelectionChanged, true);
                }
            }
        }*/

        public double GetScanTime()
        {
            double dScanTime = 0.0;
            if (_ScanRect != null && _SelectedPixelSize != null && _SelectedScanSpeed != null)
            {
                double deltaY = 250.0 * _ScanRect.Height / (_CellSize * _NumOfCells);   //250: total scan area in mm
                deltaY += SettingsManager.ConfigSettings.YMotionExtraMoveLength;
                double height = Math.Round(deltaY, 2) * 1000.0 / (double)_SelectedPixelSize.Value;
                dScanTime = height * (double)_SelectedScanSpeed.Value / 2.0;
                // Phosphor Imaging tab no longer has the scan Quality selection option
                if (Workspace.This.SelectedImagingType == ImagingType.Fluorescence)
                {
                    // 2 = Highest (see: config.xml)
                    if (SelectedScanQuality != null && SelectedScanQuality.Position == 2)
                    {
                        dScanTime *= 2;
                    }
                }
                else if (Workspace.This.SelectedImagingType == ImagingType.PhosphorImaging)
                {
                    if (SettingsManager.ConfigSettings.PhosphorModuleProcessing)
                    {
                        dScanTime *= 2;
                    }
                }
            }
            // Making sure we return a valid value
            if (double.IsNaN(dScanTime) || double.IsInfinity(dScanTime) || dScanTime < 0)
            {
                dScanTime = 0;
            }

            return Math.Round(dScanTime);
        }

    }
}
