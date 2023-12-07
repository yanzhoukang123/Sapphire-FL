using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;   //ObservableCollection
using Azure.WPF.Framework;
using Azure.ImagingSystem;
using Azure.Image.Processing;
using Azure.Configuration.Settings;

namespace Azure.LaserScanner.ViewModel
{
    [Serializable]
    class SignalViewModel : ViewModelBase
    {
        public delegate void SignalChangedDelegate(object sender, bool bIsSelectionChanged);
        public event SignalChangedDelegate SignalChanged;

        #region Private data...

        private ObservableCollection<LaserTypes> _LaserOptions = null;
        private LaserTypes _SelectedLaser = null;

        private ObservableCollection<SignalIntensity> _SignalLevelOptions = null;
        private SignalIntensity _SelectedSignalLevel;

        private ObservableCollection<ColorChannel> _ColorChannelOptions = null;
        private ColorChannel _SelectedColorChannel;

        //private int _NumEntries = 5;
        //private bool _IsModified = false;

        #endregion

        #region Constructors...

        public SignalViewModel(int numEntries)
        {
            if (Workspace.This.LaserOptions != null && Workspace.This.LaserOptions.Count > 0)
            {
                if (Workspace.This.ContainsPhosphorModule(Workspace.This.LaserOptions))
                {
                    _LaserOptions = Workspace.This.RemovePhosphorModule(Workspace.This.LaserOptions);
                }
                else
                {
                    _LaserOptions = new ObservableCollection<LaserTypes>(Workspace.This.LaserOptions);
                }
            }
            else
            {
                _LaserOptions = new ObservableCollection<LaserTypes>();
            }

            //_NumEntries = (numEntries < 5) ? 5 : numEntries;
            _SignalLevelOptions = new ObservableCollection<SignalIntensity>();

            SignalIntensity signalLevel = null;
            //for (int i = 1; i <= _NumEntries; i++)
            //{
            //    signalLevel = new SignalIntensity(i.ToString(), i);
            //    _SignalLevelOptions.Add(signalLevel);
            //}
            List<int> keyList = new List<int>(SettingsManager.ConfigSettings.LasersSignalList.Keys);
            int signalLevelCount = SettingsManager.ConfigSettings.LasersSignalList[keyList[0]].Count;
            int laserWL = 0;
            if (_LaserOptions != null && _LaserOptions.Count > 0)
            {
                laserWL = _LaserOptions[0].Wavelength;
                for (int i = 0; i < signalLevelCount; i++)
                {
                    signalLevel = new SignalIntensity(SettingsManager.ConfigSettings.LasersSignalList[laserWL][i].DisplayName,
                                                      SettingsManager.ConfigSettings.LasersSignalList[laserWL][i].Position);
                    _SignalLevelOptions.Add(signalLevel);
                }
            }

            _ColorChannelOptions = new ObservableCollection<ColorChannel>();
            ColorChannel colorChannel = null;
            colorChannel = new ColorChannel("Red", ImageChannelType.Red);
            _ColorChannelOptions.Add(colorChannel);
            colorChannel = new ColorChannel("Green", ImageChannelType.Green);
            _ColorChannelOptions.Add(colorChannel);
            colorChannel = new ColorChannel("Blue", ImageChannelType.Blue);
            _ColorChannelOptions.Add(colorChannel);
            colorChannel = new ColorChannel("Gray", ImageChannelType.Gray);
            _ColorChannelOptions.Add(colorChannel);
        }

        #endregion

        public object Clone()
        {
            SignalViewModel clone = (SignalViewModel)this.MemberwiseClone();
            clone._LaserOptions = new ObservableCollection<LaserTypes>(_LaserOptions);
            clone._SignalLevelOptions = new ObservableCollection<SignalIntensity>(_SignalLevelOptions);
            clone._ColorChannelOptions = new ObservableCollection<ColorChannel>(_ColorChannelOptions);
            if (_SelectedLaser != null)
                clone._SelectedLaser = (LaserTypes)this._SelectedLaser.Clone();

            if (_SelectedSignalLevel != null)
                clone._SelectedSignalLevel = (SignalIntensity)this._SelectedSignalLevel.Clone();
            if (_SelectedColorChannel != null)
                clone._SelectedColorChannel = (ColorChannel)this._SelectedColorChannel.Clone();
            return clone;
        }

        #region Public properties...

        public ObservableCollection<LaserTypes> LaserOptions
        {
            get { return _LaserOptions; }
            set
            {
                _LaserOptions = value;
                RaisePropertyChanged("LaserOptions");
            }
        }
        public LaserTypes SelectedLaser
        {
            get { return _SelectedLaser; }
            set
            {
                if (_SelectedLaser != value)
                {
                    PrevSelectedLaser = _SelectedLaser;
                    SelectingLaser = value;
                    SelectingColorChannel = null;
                    if (_SelectedLaser != null && !IsInitializing)
                    {
                        if (SignalChanged != null)
                        {
                            SignalChanged(this, false);
                        }
                    }

                    _SelectedLaser = value;
                    RaisePropertyChanged("SelectedLaser");
                }
            }
        }

        public ObservableCollection<SignalIntensity> SignalLevelOptions
        {
            get { return _SignalLevelOptions; }
            set
            {
                if (_SignalLevelOptions != value)
                {
                    _SignalLevelOptions = value;
                    RaisePropertyChanged("SignalLevelOptions");
                }
            }
        }

        public SignalIntensity SelectedSignalLevel
        {
            get { return _SelectedSignalLevel; }
            set
            {
                if (_SelectedSignalLevel != value)
                {
                    SelectingColorChannel = null;
                    SelectingLaser = null;
                    if (_SelectedSignalLevel != null && !IsInitializing)
                    {
                        SignalChanged?.Invoke(this, false);
                    }

                    _SelectedSignalLevel = value;
                    RaisePropertyChanged("SelectedSignalLevel");
                }
            }
        }

        public ObservableCollection<ColorChannel> ColorChannelOptions
        {
            get { return _ColorChannelOptions; }
            set
            {
                if (_ColorChannelOptions != value)
                {
                    _ColorChannelOptions = value;
                    RaisePropertyChanged("ColorChannelOptions");
                }
            }
        }

        public ColorChannel SelectedColorChannel
        {
            get { return _SelectedColorChannel; }
            set
            {
                if (_SelectedColorChannel != value)
                {
                    PrevSelectedColorChannel = _SelectedColorChannel;
                    SelectingColorChannel = value;
                    SelectingLaser = null;
                    if (_SelectedColorChannel != null && !IsInitializing)
                    {
                        SignalChanged?.Invoke(this, false);
                    }

                    _SelectedColorChannel = value;
                    RaisePropertyChanged("SelectedColorChannel");
                }
            }
        }

        // Previous selected and selecting item for validation purposes.
        //
        public LaserTypes SelectingLaser = null;
        public ColorChannel SelectingColorChannel = null;
        public LaserTypes PrevSelectedLaser = null;
        public ColorChannel PrevSelectedColorChannel = null;
        public bool IsCancelPropertyChange { get; set; } = false;

        /// <summary>
        /// Indicating property is initializing/restoring/reverting back
        /// to previous setting/value (to avoid triggering SignalChanged event)
        /// </summary>
        public bool IsInitializing { get; set; } = false;

        #endregion

    }

    [Serializable]
    class SignalIntensity
    {
        public string DisplayName { get; set; }
        public int IntensityLevel { get; set; }

        public SignalIntensity(string displayName, int intensityLevel)
        {
            this.DisplayName = displayName;
            this.IntensityLevel = intensityLevel;
        }
        public object Clone()
        {
            SignalIntensity clone = (SignalIntensity)this.MemberwiseClone();
            return clone;
        }
    }

    [Serializable]
    class ColorChannel
    {
        public string DisplayName { get; set; }
        public ImageChannelType ImageColorChannel { get; set; }

        public ColorChannel(string displayName, ImageChannelType colorChannel)
        {
            this.DisplayName = displayName;
            this.ImageColorChannel = colorChannel;
        }
        public object Clone()
        {
            ColorChannel clone = (ColorChannel)this.MemberwiseClone();
            return clone;
        }
    }

}
