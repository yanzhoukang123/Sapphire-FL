using Azure.Configuration.Settings;
using Azure.EthernetCommLib;
using Azure.ImagingSystem;
using Azure.WPF.Framework;
using Hywire.FileAccess;
using LogW;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using static Azure.EthernetCommLib.AvocadoProtocol;

namespace Azure.ScannerEUI.ViewModel
{
    class IvViewModel : ViewModelBase
    {
        #region Private data...
        private EthernetController _EthernetController;
        private ObservableCollection<APDGainType> _APDGainOptions = null;
        private ObservableCollection<APDPgaType> _PGAOptionsModule = null;
        //private IvSensorController _IvSensorController;
        private Visibility _GainComVisbleL1 = Visibility.Visible;
        private Visibility _GainTxtVisbleL1 = Visibility.Visible;
        private Visibility _GainComVisbleR1 = Visibility.Visible;
        private Visibility _GainTxtVisbleR1 = Visibility.Visible;
        private Visibility _GainComVisbleR2 = Visibility.Visible;
        private Visibility _GainTxtVisbleR2 = Visibility.Visible;
        private APDPgaType _SelectedPgaMModuleL1 = null;
        private APDPgaType _SelectedPgaMModuleR1 = null;
        private APDPgaType _SelectedPgaMModuleR2 = null;
        private APDGainType _SelectedGainComModuleL1 = null;
        private APDGainType _SelectedGainComModuleR1 = null;
        private APDGainType _SelectedGainComModuleR2 = null;
        private IvSensorType _SensorML1;
        private IvSensorType _SensorMR1;
        private IvSensorType _SensorMR2;
        private string _SensorSNL1 = "L1";
        private string _SensorSNR1 = "R1";
        private string _SensorSNR2 = "R2";
        private string _LaserSNL1 = "L";
        private string _LaserSNR1 = "R1";
        private string _LaserSNR2 = "R2";
        private int _WL1 = 0;
        private int _WR1 = 0;
        private int _WR2 = 0;
        private string _SensorTemperatureL1 = "0";
        private string _SensorTemperatureR1 = "0";
        private string _SensorTemperatureR2 = "0";
        private double _SensorRadTemperaTureL1 = 0;
        private double _SensorRadTemperaTureR1 = 0;
        private double _SensorRadTemperaTureR2 = 0;
        int _TxtApdGainL1 = 500;
        int _TxtApdGainR1 = 500;
        int _TxtApdGainR2 = 500;
        private double _LaserAPower = 0;
        private double _LaserBPower = 0;
        private double _LaserCPower = 0;
        private bool _IsLaserL1Selected = false;
        private bool _IsLaserR1Selected = false;
        private bool _IsLaserR2Selected = false;
        private bool _IsCaptrueL1Selected = false;
        private bool _IsCaptrueR1Selected = false;
        private bool _IsCaptrueR2Selected = false;
        private bool _IsNullEnabledL1 = false;
        private bool _IsNullEnabledR1 = false;
        private bool _IsNullEnabledR2 = false;
        private bool _IsReadOnly = true;
        private const string LaserSetErrorMessage = "Laser Power should be set to 0 or above 5 mW";
        private Thread ShowTemperatureTimer = null;
        private Thread ShowRedTemperatureTimer = null;
        private int RadiatorTemperaTureLMax;
        private int RadiatorTemperaTureR1Max;
        private int RadiatorTemperaTureR2Max;
        //Control conditions for the fan speed 
        private double InternalLowTemperature = 22;
        private double InternalModerateTemperature = 25;
        private double InternalHighTemperature = 27;
        private double ModuleLowTemperature = 3;
        private double ModuleModerateTemperature = 5;
        private double ModuleHighTemperature = 7;
        private string _WL1Sign;
        private string _WR1Sign;
        private string _WR2Sign;
        #endregion
        public EthernetController EthernetDevice
        {
            get
            {
                return _EthernetController;
            }
        }

        public void InitIVControls()
        {
            //_IvSensorController = new IvSensorController(Workspace.This.EthernetController);
            //Control conditions for the fan speed 
            InternalLowTemperature = SettingsManager.ConfigSettings.InternalLowTemperature;
            InternalModerateTemperature = SettingsManager.ConfigSettings.InternalModerateTemperature;
            InternalHighTemperature = SettingsManager.ConfigSettings.InternalHighTemperature;
            ModuleLowTemperature = SettingsManager.ConfigSettings.ModuleLowTemperature;
            ModuleModerateTemperature = SettingsManager.ConfigSettings.ModuleModerateTemperature;
            ModuleHighTemperature = SettingsManager.ConfigSettings.ModuleHighTemperature;

            RadiatorTemperaTureLMax = SettingsManager.ConfigSettings.RadiatorTemperatureL;
            RadiatorTemperaTureR1Max = SettingsManager.ConfigSettings.RadiatorTemperatureR1;
            RadiatorTemperaTureR2Max = SettingsManager.ConfigSettings.RadiatorTemperatureR2;

            _APDGainOptions = SettingsManager.ConfigSettings.APDGains;
            _PGAOptionsModule = SettingsManager.ConfigSettings.APDPgas;
            RaisePropertyChanged("PGAOptionsModule");
            RaisePropertyChanged("GainComModule");
            if (_PGAOptionsModule != null && _PGAOptionsModule.Count >= 3)
            {
                SelectedMModuleL1 = _PGAOptionsModule[3];
                SelectedMModuleR1 = _PGAOptionsModule[3];
                SelectedMModuleR2 = _PGAOptionsModule[3];
            }
            if (_APDGainOptions != null && _APDGainOptions.Count >= 6)
            {
                SelectedGainComModuleL1 = _APDGainOptions[5];    // select the 6th item
                SelectedGainComModuleR1 = _APDGainOptions[5];    // select the 6th item
                SelectedGainComModuleR2 = _APDGainOptions[5];    // select the 6th item
                GainTxtModuleL1 = 4000;
                GainTxtModuleR1 = 4000;
                GainTxtModuleR2 = 4000;
            }
            VisbleAndEnable();
            TurnOffAllLasers();
            OnTemperatureInit();

            //SetFanTemperature();
        }
        //private void SetFanTemperature() 
        //{
        //    if (RadiatorTemperaTureLMax<=0|| RadiatorTemperaTureR1Max<=0|| RadiatorTemperaTureR2Max<=0)
        //    {
        //        MessageBox.Show("设置的风扇启动预定值不合理！","提示");
        //        return;
        //    }
        //    EthernetDevice.SetFanTemperature(LaserChannels.ChannelC, RadiatorTemperaTureLMax);
        //    EthernetDevice.SetFanTemperature(LaserChannels.ChannelA, RadiatorTemperaTureR1Max);
        //    EthernetDevice.SetFanTemperature(LaserChannels.ChannelB, RadiatorTemperaTureR2Max);
        //    Thread.Sleep(500);

        //}
        /// <summary>
        /// visble and Enable
        /// </summary>
        public void VisbleAndEnable()
        {
            if (SensorML1 == IvSensorType.APD)
            {
                _IsNullEnabledL1 = true;
                _GainComVisbleL1 = Visibility.Visible;
                _GainTxtVisbleL1 = Visibility.Hidden;
                RaisePropertyChanged("IsNullEnabledL1");
                RaisePropertyChanged("GainComVisFlagL1");
                RaisePropertyChanged("GainVisTxtFlagL1");
            }
            else if (SensorML1 == IvSensorType.PMT)
            {
                _IsNullEnabledL1 = true;
                _GainComVisbleL1 = Visibility.Hidden;
                _GainTxtVisbleL1 = Visibility.Visible;
                RaisePropertyChanged("IsNullEnabledL1");
                RaisePropertyChanged("GainComVisFlagL1");
                RaisePropertyChanged("GainVisTxtFlagL1");
            }
            else
            {
                _IsNullEnabledL1 = false;
                RaisePropertyChanged("IsNullEnabledL1");
            }
            if (SensorMR1 == IvSensorType.APD)
            {
                _IsNullEnabledR1 = true;
                _GainComVisbleR1 = Visibility.Visible;
                _GainTxtVisbleR1 = Visibility.Hidden;
                RaisePropertyChanged("IsNullEnabledR1");
                RaisePropertyChanged("GainComVisFlagR1");
                RaisePropertyChanged("GainVisTxtFlagR1");
            }
            else if (SensorMR1 == IvSensorType.PMT)
            {
                _IsNullEnabledR1 = true;
                _GainComVisbleR1 = Visibility.Hidden;
                _GainTxtVisbleR1 = Visibility.Visible;
                RaisePropertyChanged("IsNullEnabledR1");
                RaisePropertyChanged("GainComVisFlagR1");
                RaisePropertyChanged("GainVisTxtFlagR1");
            }
            else
            {
                _IsNullEnabledR1 = false;
                RaisePropertyChanged("IsNullEnabledR1");

            }
            if (SensorMR2 == IvSensorType.APD)
            {
                _IsNullEnabledR2 = true;
                _GainComVisbleR2 = Visibility.Visible;
                _GainTxtVisbleR2 = Visibility.Hidden;
                RaisePropertyChanged("IsNullEnabledR2");
                RaisePropertyChanged("GainComVisFlagR2");
                RaisePropertyChanged("GainVisTxtFlagR2");
            }
            else if (SensorMR2 == IvSensorType.PMT)
            {
                _IsNullEnabledR2 = true;
                _GainComVisbleR2 = Visibility.Hidden;
                _GainTxtVisbleR2 = Visibility.Visible;
                RaisePropertyChanged("IsNullEnabledR2");
                RaisePropertyChanged("GainComVisFlagR2");
                RaisePropertyChanged("GainVisTxtFlagR2");
            }
            else
            {
                _IsNullEnabledR2 = false;
                RaisePropertyChanged("IsNullEnabledR2");
            }
        }

        //Turn off all the lasers after scanning is completed
        public void TurnOffAllLasers()
        {
            IsLaserL1Selected = false;
            System.Threading.Thread.Sleep(200);
            IsLaserR1Selected = false;
            System.Threading.Thread.Sleep(200);
            IsLaserR2Selected = false;
        }

        #region Constructors...

        public IvViewModel(EthernetController ethernetController)
        {
            _EthernetController = ethernetController;

        }
        #endregion

        #region Public properties...
        public delegate void SetScanRegionEvent(string name);
        public event SetScanRegionEvent OnScanRegionReceived;
        #region Sensor
        public IvSensorType SensorML1
        {
            get
            {

                return _SensorML1;
            }
            set
            {
                if (_SensorML1 != value)
                {
                    if ((int)value == Workspace.This.NewParameterVM.Uint8Code)
                    {
                        _SensorML1 = IvSensorType.NA;
                        RaisePropertyChanged("SensorML1");
                    }
                    else
                    {
                        _SensorML1 = value;
                        RaisePropertyChanged("SensorML1");
                    }

                }
            }
        }
        public IvSensorType SensorMR1
        {
            get { return _SensorMR1; }
            set
            {
                if (_SensorMR1 != value)
                {
                    if ((int)value == Workspace.This.NewParameterVM.Uint8Code)
                    {
                        _SensorMR1 = IvSensorType.NA;
                        RaisePropertyChanged("SensorMR1");
                    }
                    else
                    {
                        _SensorMR1 = value;
                        RaisePropertyChanged("SensorMR1");
                    }
                }
            }
        }
        public IvSensorType SensorMR2
        {
            get { return _SensorMR2; }
            set
            {
                if (_SensorMR2 != value)
                {
                    if ((int)value == Workspace.This.NewParameterVM.Uint8Code)
                    {
                        _SensorMR2 = IvSensorType.NA;
                        RaisePropertyChanged("SensorMR2");
                    }
                    else
                    {
                        _SensorMR2 = value;
                        RaisePropertyChanged("SensorMR2");
                    }
                }
            }
        }
        #endregion

        #region SensorSN
        public string SensorSNL1
        {
            get { return _SensorSNL1; }
            set
            {

                if (_SensorSNL1 != value)
                {
                    if (value == Workspace.This.NewParameterVM.StrEmptyCode1)
                    {
                        _SensorSNL1 = "NaN";
                        RaisePropertyChanged("SensorSNL1");
                    }
                    else
                    {
                        _SensorSNL1 = value;
                        RaisePropertyChanged("SensorSNL1");
                    }
                }
            }
        }
        public string SensorSNR1
        {
            get { return _SensorSNR1; }
            set
            {

                if (_SensorSNR1 != value)
                {
                    if (value == Workspace.This.NewParameterVM.StrEmptyCode1)
                    {
                        _SensorSNR1 = "NaN";
                        RaisePropertyChanged("SensorSNR1");
                    }
                    else
                    {
                        _SensorSNR1 = value;
                        RaisePropertyChanged("SensorSNR1");
                    }
                }
            }
        }
        public string SensorSNR2
        {
            get { return _SensorSNR2; }
            set
            {

                if (_SensorSNR2 != value)
                {
                    if (value == Workspace.This.NewParameterVM.StrEmptyCode1)
                    {
                        _SensorSNR2 = "NaN";
                        RaisePropertyChanged("SensorSNR2");
                    }
                    else
                    {
                        _SensorSNR2 = value;
                        RaisePropertyChanged("SensorSNR2");
                    }
                }
            }
        }
        #endregion

        #region LaserSNL
        public string LaserSNL1
        {
            get { return _LaserSNL1; }
            set
            {

                if (_LaserSNL1 != value)
                {
                    if (value == Workspace.This.NewParameterVM.StrEmptyCode1)
                    {
                        _LaserSNL1 = "NaN";
                        RaisePropertyChanged("LaserSNL1");
                    }
                    else
                    {
                        _LaserSNL1 = value;
                        RaisePropertyChanged("LaserSNL1");
                    }
                }
            }
        }
        public string LaserSNR1
        {
            get { return _LaserSNR1; }
            set
            {

                if (_LaserSNR1 != value)
                {
                    if (value == Workspace.This.NewParameterVM.StrEmptyCode1)
                    {
                        _LaserSNR1 = "NaN";
                        RaisePropertyChanged("LaserSNR1");
                    }
                    else
                    {
                        _LaserSNR1 = value;
                        RaisePropertyChanged("LaserSNR1");
                    }
                }
            }
        }
        public string LaserSNR2
        {
            get { return _LaserSNR2; }
            set
            {

                if (_LaserSNR2 != value)
                {
                    if (value == Workspace.This.NewParameterVM.StrEmptyCode1)
                    {
                        _LaserSNR2 = "NaN";
                        RaisePropertyChanged("LaserSNR2");
                    }
                    else
                    {
                        _LaserSNR2 = value;
                        RaisePropertyChanged("LaserSNR2");
                    }
                }
            }
        }
        #endregion

        #region WL
        public int WL1
        {
            get { return _WL1; }
            set
            {
                WL1Sign = "";
                if (_WL1 != value)
                {
                    if (value == Workspace.This.NewParameterVM.Uint16Code)
                    {
                        _WL1 = 0;
                    }
                    else if (value == 4880)
                    {
                        _WL1 = 488;
                        WL1Sign = "-YFP";
                    }
                    else if (value == 5320)
                    {
                        _WL1 = 532;
                        WL1Sign = "-Propidium";
                    }
                    else
                    {
                        _WL1 = value;
                    }
                    RaisePropertyChanged("WL1");
                }
            }
        }
        public int WR1
        {
            get { return _WR1; }
            set
            {
                WR1Sign = " ";
                if (_WR1 != value)
                {
                    if (value == Workspace.This.NewParameterVM.Uint16Code)
                    {
                        _WR1 = 0;
                    }
                    else if (value == 4880)
                    {
                        _WR1 = 488;
                        WR1Sign = "-YFP";
                    }
                    else if (value == 5320)
                    {
                        _WR1 = 532;
                        WR1Sign = "-Propidium";
                    }
                    else
                    {
                        _WR1 = value;
                    }
                    RaisePropertyChanged("WR1");
                }
            }
        }
        public int WR2
        {
            get { return _WR2; }
            set
            {
                WR2Sign = " ";
                if (_WR2 != value)
                {
                    if (value == Workspace.This.NewParameterVM.Uint16Code)
                    {
                        _WR2 = 0;
                    }
                    else if (value == 4880)
                    {
                        _WR2 = 488;
                        WR2Sign = "-YFP";
                    }
                    else if (value == 5320)
                    {
                        _WR2 = 532;
                        WR2Sign = "-Propidium";
                    }
                    else
                    {
                        _WR2 = value;
                    }
                    RaisePropertyChanged("WR2");
                }
            }
        }
        #endregion

        #region Sign
        public string WL1Sign
        {
            get { return _WL1Sign; }
            set
            {

                if (_WL1Sign != value)
                {
                    _WL1Sign = value;
                    RaisePropertyChanged("WL1Sign");
                }
            }
        }

        public string WR1Sign
        {
            get { return _WR1Sign; }
            set
            {

                if (_WR1Sign != value)
                {
                    _WR1Sign = value;
                    RaisePropertyChanged("WR1Sign");
                }
            }
        }

        public string WR2Sign
        {
            get { return _WR2Sign; }
            set
            {

                if (_WR2Sign != value)
                {
                    _WR2Sign = value;
                    RaisePropertyChanged("WR2Sign");
                }
            }
        }

        #endregion

        #region  Temperature
        public string SensorTemperatureL1
        {
            get { return _SensorTemperatureL1; }
            set
            {
                if (_SensorTemperatureL1 != value)
                {
                    _SensorTemperatureL1 = value;
                    RaisePropertyChanged("SensorTemperatureL1");
                }
            }
        }
        public string SensorTemperatureR1
        {
            get { return _SensorTemperatureR1; }
            set
            {

                if (_SensorTemperatureR1 != value)
                {
                    _SensorTemperatureR1 = value;
                    RaisePropertyChanged("SensorTemperatureR1");
                }
            }
        }
        public string SensorTemperatureR2
        {
            get { return _SensorTemperatureR2; }
            set
            {

                if (_SensorTemperatureR2 != value)
                {
                    _SensorTemperatureR2 = value;
                    RaisePropertyChanged("SensorTemperatureR2");
                }
            }
        }

        #endregion

        #region  RadTemperaTure
        public double SensorRadTemperaTureL1
        {
            get { return _SensorRadTemperaTureL1; }
            set
            {
                if (_SensorRadTemperaTureL1 != value)
                {
                    _SensorRadTemperaTureL1 = value;
                    RaisePropertyChanged("SensorRadTemperaTureL1");
                }
            }
        }
        public double SensorRadTemperaTureR1
        {
            get { return _SensorRadTemperaTureR1; }
            set
            {

                if (_SensorRadTemperaTureR1 != value)
                {
                    _SensorRadTemperaTureR1 = value;
                    RaisePropertyChanged("SensorRadTemperaTureR1");
                }
            }
        }
        public double SensorRadTemperaTureR2
        {
            get { return _SensorRadTemperaTureR2; }
            set
            {

                if (_SensorRadTemperaTureR2 != value)
                {
                    _SensorRadTemperaTureR2 = value;
                    RaisePropertyChanged("SensorRadTemperaTureR2");
                }
            }
        }
        #endregion

        #region APD PGA
        public ObservableCollection<APDPgaType> PGAOptionsModule
        {
            get { return _PGAOptionsModule; }
        }
        public APDPgaType SelectedMModuleL1
        {
            get { return _SelectedPgaMModuleL1; }
            set
            {
                if (_SelectedPgaMModuleL1 != value)
                {
                    _SelectedPgaMModuleL1 = value;
                    RaisePropertyChanged("SelectedMModuleL1");
                    OnScanRegionReceived("SelectedMModuleL1");
                    if (SensorML1 != IvSensorType.NA)
                    {
                        EthernetDevice.SetIvPga(IVChannels.ChannelC, (ushort)_SelectedPgaMModuleL1.Value);
                    }
                }
            }
        }
        public APDPgaType SelectedMModuleR1
        {
            get { return _SelectedPgaMModuleR1; }
            set
            {
                if (_SelectedPgaMModuleR1 != value)
                {
                    _SelectedPgaMModuleR1 = value;
                    RaisePropertyChanged("SelectedMModuleR1");
                    OnScanRegionReceived("SelectedMModuleR1");
                    if (SensorMR1 != IvSensorType.NA)
                    {
                        EthernetDevice.SetIvPga(IVChannels.ChannelA, (ushort)_SelectedPgaMModuleR1.Value);
                    }
                }
            }
        }
        public APDPgaType SelectedMModuleR2
        {
            get { return _SelectedPgaMModuleR2; }
            set
            {
                if (_SelectedPgaMModuleR2 != value)
                {
                    _SelectedPgaMModuleR2 = value;
                    RaisePropertyChanged("SelectedMModuleR2");
                    OnScanRegionReceived("SelectedMModuleR2");
                    if (SensorMR2 != IvSensorType.NA)
                    {
                        EthernetDevice.SetIvPga(IVChannels.ChannelB, (ushort)_SelectedPgaMModuleR2.Value);
                    }
                }
            }
        }
        #endregion

        #region Gain APD
        public ObservableCollection<APDGainType> GainComModule
        {
            get { return _APDGainOptions; }
        }
        public APDGainType SelectedGainComModuleL1
        {
            get { return _SelectedGainComModuleL1; }
            set
            {
                if (_SelectedGainComModuleL1 != value)
                {
                    _SelectedGainComModuleL1 = value;
                    RaisePropertyChanged("SelectedGainComModuleL1");
                    OnScanRegionReceived("SelectedGainComModuleL1");
                    if (SensorML1 == IvSensorType.APD)
                    {
                        EthernetDevice.SetIvApdGain(IVChannels.ChannelC, (ushort)_SelectedGainComModuleL1.Value);
                    }
                }
            }
        }
        public APDGainType SelectedGainComModuleR1
        {
            get { return _SelectedGainComModuleR1; }
            set
            {
                if (_SelectedGainComModuleR1 != value)
                {
                    _SelectedGainComModuleR1 = value;
                    RaisePropertyChanged("SelectedGainComModuleR1");
                    OnScanRegionReceived("SelectedGainComModuleR1");
                    if (SensorMR1 == IvSensorType.APD)
                    {
                        EthernetDevice.SetIvApdGain(IVChannels.ChannelA, (ushort)_SelectedGainComModuleR1.Value);
                    }
                }
            }
        }
        public APDGainType SelectedGainComModuleR2
        {
            get { return _SelectedGainComModuleR2; }
            set
            {
                if (_SelectedGainComModuleR2 != value)
                {
                    _SelectedGainComModuleR2 = value;
                    RaisePropertyChanged("SelectedGainComModuleR2");
                    OnScanRegionReceived("SelectedGainComModuleR2");
                    if (SensorMR2 == IvSensorType.APD)
                    {
                        EthernetDevice.SetIvApdGain(IVChannels.ChannelB, (ushort)_SelectedGainComModuleR2.Value);
                    }
                }
            }
        }
        #endregion

        #region Gain PMT
        public int GainTxtModuleL1
        {
            get { return _TxtApdGainL1; }
            set
            {

                if (_TxtApdGainL1 != value)
                {
                    _TxtApdGainL1 = value;
                    RaisePropertyChanged("GainTxtModuleL1");
                    OnScanRegionReceived("GainTxtModuleL1");
                    if (SensorML1 == IvSensorType.PMT)
                    {
                        EthernetDevice.SetIvPmtGain(IVChannels.ChannelC, (ushort)_TxtApdGainL1);
                    }
                }
            }
        }
        public int GainTxtModuleR1
        {
            get { return _TxtApdGainR1; }
            set
            {
                if (_TxtApdGainR1 != value)
                {
                    _TxtApdGainR1 = value;
                    RaisePropertyChanged("GainTxtModuleR1");
                    OnScanRegionReceived("GainTxtModuleR1");
                    if (SensorMR1 == IvSensorType.PMT)
                    {
                        EthernetDevice.SetIvPmtGain(IVChannels.ChannelA, (ushort)_TxtApdGainR1);
                    }
                }
            }
        }
        public int GainTxtModuleR2
        {
            get { return _TxtApdGainR2; }
            set
            {
                if (_TxtApdGainR2 != value)
                {
                    _TxtApdGainR2 = value;
                    RaisePropertyChanged("GainTxtModuleR2");
                    OnScanRegionReceived("GainTxtModuleR2");
                    if (SensorMR2 == IvSensorType.PMT)
                    {
                        EthernetDevice.SetIvPmtGain(IVChannels.ChannelB, (ushort)_TxtApdGainR2);
                    }
                }
            }
        }

        #endregion

        #region  Gain Com visble
        public Visibility GainComVisFlagL1
        {
            get
            {
                return _GainComVisbleL1;

            }

        }
        public Visibility GainComVisFlagR1
        {
            get
            {

                return _GainComVisbleR1;

            }

        }
        public Visibility GainComVisFlagR2
        {
            get
            {

                return _GainComVisbleR2;

            }

        }
        #endregion

        #region  Gain Txt Visble

        public Visibility GainVisTxtFlagL1
        {
            get
            {
                return _GainTxtVisbleL1;
            }

        }
        public Visibility GainVisTxtFlagR1
        {
            get
            {
                return _GainTxtVisbleR1;
            }

        }
        public Visibility GainVisTxtFlagR2
        {
            get
            {
                return _GainTxtVisbleR2;
            }

        }
        #endregion

        #region Power

        public double LaserAPower
        {
            get { return _LaserAPower; }
            set
            {

                if (_LaserAPower != value)
                {

                    //if ((value < 0) || (value > 0 && value < 5))
                    //{
                    //    string caption = "Laser A...";
                    //    string message = LaserSetErrorMessage;
                    //    MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                    //}
                    //else
                    //{
                    _LaserAPower = value;
                    //LaserAIntensity = Workspace.This.ApdVM.APDTransfer.LaserPowerToIntensity(ScannerDataStruct.APDLaserChannelType.A, _LaserAPower);
                    RaisePropertyChanged("LaserAPower");
                    OnScanRegionReceived("LaserAPower");
                    //}


                }
            }
        }

        public double LaserBPower
        {
            get { return _LaserBPower; }
            set
            {

                if (_LaserBPower != value)
                {

                    //if ((value < 0) || (value > 0 && value < 5))
                    //{
                    //    string caption = "Laser B...";
                    //    string message = LaserSetErrorMessage;
                    //    MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                    //}
                    //else
                    //{
                    _LaserBPower = value;
                    //LaserBIntensity = Workspace.This.ApdVM.APDTransfer.LaserPowerToIntensity(ScannerDataStruct.APDLaserChannelType.B, _LaserBPower);
                    RaisePropertyChanged("LaserBPower");
                    OnScanRegionReceived("LaserBPower");
                    //}
                }
            }
        }

        public double LaserCPower
        {
            get { return _LaserCPower; }
            set
            {
                if (_LaserCPower != value)
                {

                    //if ((value < 0) || (value > 0 && value < 5))
                    //{
                    //    string caption = "Laser C...";
                    //    string message = LaserSetErrorMessage;
                    //    MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                    //}
                    //else
                    //{
                    _LaserCPower = value;
                    //LaserCIntensity = Workspace.This.ApdVM.APDTransfer.LaserPowerToIntensity(ScannerDataStruct.APDLaserChannelType.C, _LaserCPower);
                    RaisePropertyChanged("LaserCPower");
                    OnScanRegionReceived("LaserCPower");
                    //}
                }
            }
        }
        #endregion

        #region IsLaserSelected
        public bool IsLaserL1Selected
        {
            get { return _IsLaserL1Selected; }
            set
            {
                if (_IsLaserL1Selected != value)
                {
                    _IsLaserL1Selected = value;
                    RaisePropertyChanged("IsLaserL1Selected");
                    OnScanRegionReceived("IsLaserL1Selected");
                    if (value == true)
                    {
                        IsCaptrueL1Selected = true;
                        if (Workspace.This.IVVM.WL1 == 375)                          //Use the specified 375 module coefficient  使用指定的375模块系数
                        {
                            if (Workspace.This.NewParameterVM.L375Coefficient > 0 && Workspace.This.NewParameterVM.L375Coefficient < 0.6)
                            {
                                //Calculate new values based on coefficients
                                //  1/(1-x)*w=y
                                double DummyParameter = 1 / (1 - Workspace.This.NewParameterVM.L375Coefficient) * LaserAPower;
                                EthernetDevice.SetLaserPower(LaserChannels.ChannelC, DummyParameter);
                            }
                            else
                            {
                                EthernetDevice.SetLaserPower(LaserChannels.ChannelC, LaserAPower);
                            }
                        }
                        else
                        {
                            if (Workspace.This.NewParameterVM.LCoefficient > 0 && Workspace.This.NewParameterVM.LCoefficient < 0.6)
                            {
                                //Calculate new values based on coefficients
                                //  1/(1-x)*w=y
                                double DummyParameter = 1 / (1 - Workspace.This.NewParameterVM.LCoefficient) * LaserAPower;
                                EthernetDevice.SetLaserPower(LaserChannels.ChannelC, DummyParameter);
                            }
                            else
                            {
                                EthernetDevice.SetLaserPower(LaserChannels.ChannelC, LaserAPower);
                            }
                        }
                    }
                    else
                    {
                        //IsCaptrueL1Selected = false;
                        EthernetDevice.SetLaserPower(LaserChannels.ChannelC, 0);
                    }
                }
            }
        }

        public bool IsLaserR1Selected
        {
            get { return _IsLaserR1Selected; }
            set
            {
                if (_IsLaserR1Selected != value)
                {
                    _IsLaserR1Selected = value;
                    RaisePropertyChanged("IsLaserR1Selected");
                    OnScanRegionReceived("IsLaserR1Selected");
                    if (value == true)
                    {
                        IsCaptrueR1Selected = true;
                        if (Workspace.This.NewParameterVM.R1Coefficient > 0 && Workspace.This.NewParameterVM.R1Coefficient < 0.6)
                        {
                            //Calculate new values based on coefficients
                            //  1/(1-x)*w=y
                            double DummyParameter = 1 / (1 - Workspace.This.NewParameterVM.R1Coefficient) * LaserBPower;
                            EthernetDevice.SetLaserPower(LaserChannels.ChannelA, DummyParameter);
                        }
                        else
                        {
                            EthernetDevice.SetLaserPower(LaserChannels.ChannelA, LaserBPower);
                        }
                    }
                    else
                    {
                        //IsCaptrueR1Selected = false;
                        EthernetDevice.SetLaserPower(LaserChannels.ChannelA, 0);
                    }
                }
            }
        }

        public bool IsLaserR2Selected
        {
            get
            {
                return _IsLaserR2Selected;
            }
            set
            {
                if (_IsLaserR2Selected != value)
                {
                    _IsLaserR2Selected = value;
                    RaisePropertyChanged("IsLaserR2Selected");
                    OnScanRegionReceived("IsLaserR2Selected");
                    if (value == true)
                    {
                        IsCaptrueR2Selected = true;
                        //The optical module wavelength of the R2 channel is 532, and a coefficient of 532 needs to be specified R2通道的光学模块波长是532需要使用532指定系数
                        if (Workspace.This.IVVM.WR2 == 532)
                        {
                            if (Workspace.This.NewParameterVM.R2532Coefficient > 0 && Workspace.This.NewParameterVM.R2532Coefficient < 0.6)
                            {
                                //Calculate new values based on coefficients
                                //  1/(1-x)*w=y
                                double  DummyParameter = 1 / (1 - Workspace.This.NewParameterVM.R2532Coefficient) * LaserCPower;
                                EthernetDevice.SetLaserPower(LaserChannels.ChannelB, DummyParameter);
                            }
                            else
                            {
                                EthernetDevice.SetLaserPower(LaserChannels.ChannelB, LaserCPower);
                            }

                        }
                        else
                        {
                            if (Workspace.This.NewParameterVM.R2Coefficient > 0 && Workspace.This.NewParameterVM.R2Coefficient < 0.6)
                            {
                                //Calculate new values based on coefficients
                                //  1/(1-x)*w=y
                                double DummyParameter = 1 / (1 - Workspace.This.NewParameterVM.R2Coefficient) * LaserCPower;
                                EthernetDevice.SetLaserPower(LaserChannels.ChannelB, DummyParameter);
                            }
                            else
                            {
                                EthernetDevice.SetLaserPower(LaserChannels.ChannelB, LaserCPower);
                            }
                        }
                    }
                    else
                    {
                        //IsCaptrueR2Selected = false;
                        EthernetDevice.SetLaserPower(LaserChannels.ChannelB, 0);
                    }
                }
            }
        }
        #endregion

        #region IsCaptrueSelected
        public bool IsCaptrueL1Selected
        {
            get { return _IsCaptrueL1Selected; }
            set
            {
                if (_IsCaptrueL1Selected != value)
                {
                    _IsCaptrueL1Selected = value;
                    RaisePropertyChanged("IsCaptrueL1Selected");
                    OnScanRegionReceived("IsCaptrueL1Selected");
                }
            }
        }

        public bool IsCaptrueR1Selected
        {
            get { return _IsCaptrueR1Selected; }
            set
            {
                if (_IsCaptrueR1Selected != value)
                {
                    _IsCaptrueR1Selected = value;
                    RaisePropertyChanged("IsCaptrueR1Selected");
                    OnScanRegionReceived("IsCaptrueR1Selected");
                }
            }
        }

        public bool IsCaptrueR2Selected
        {
            get
            {
                return _IsCaptrueR2Selected;
            }
            set
            {
                if (_IsCaptrueR2Selected != value)
                {
                    _IsCaptrueR2Selected = value;
                    RaisePropertyChanged("IsCaptrueR2Selected");
                    OnScanRegionReceived("IsCaptrueR2Selected");
                }
            }
        }
        #endregion

        #region IsEnabled
        public bool IsNullEnabledL1
        {
            get { return _IsNullEnabledL1; }
            set
            {
                _IsNullEnabledL1 = value;
                RaisePropertyChanged("IsNullEnabled");
            }
        }
        public bool IsNullEnabledR1
        {
            get { return _IsNullEnabledR1; }
            set
            {
                _IsNullEnabledR1 = value;
                RaisePropertyChanged("IsNullEnabledR1");
            }
        }
        public bool IsNullEnabledR2
        {
            get { return _IsNullEnabledR2; }
            set
            {
                _IsNullEnabledR2 = value;
                RaisePropertyChanged("IsNullEnabledR2");
            }
        }
        #endregion

        #region TemperatureTimerDelay
        FileVisit ReportFile = null;
        CSVFile Report = null;
        int columnWriteCount = 0;
        string[] StrTemperature = new string[1];
        bool AlerWindow = true;
        //one sec ref Temperature
        private void OnTemperatureTime()
        {
            //GenerateReportTemperature();
            while (true)
            {
                Thread.Sleep(3000);
                //   When the optical module is powered on and the version is 1.1.0.0   光学模块上电并且版本是1.1.0.0（FW Version 1.1.0.0） 
                if (Workspace.This.ScannerVM.HWversion == Workspace.This.DefaultHWversion)
                {
                    //Obtain ambient temperature
                    EthernetDevice.GetAmbientTemperature();
                    double CH1Temerature = EthernetDevice.AmbientTemperature[AmbientTemperatureChannel.CH1];
                    // double CH2Temerature = EthernetDevice.AmbientTemperature[AmbientTemperatureChannel.CH2];
                    Workspace.This.AmbientTemperatureCH1 = CH1Temerature;
                    //Workspace.This.AmbientTemperatureCH2 = CH2Temerature;
                    //double MaxTemperature = Math.Max(CH1Temerature, CH2Temerature);
                    double MaxTemperature = CH1Temerature;
                    int FanSleep = 0;
                    double L1ModuleTemperatureDiff = 0;
                    double R1ModuleTemperatureDiff = 0;
                    double R2ModuleTemperatureDiff = 0;
                    //L通道有模块存在 There are modules present in the L channel
                    if (Workspace.This.IVVM.WL1 != 0 && Workspace.This.IVVM.WL1 != Workspace.This.NewParameterVM.Uint16Code && Workspace.This.OpticalModulePowerStatus)
                    {
                        EthernetDevice.GetSingeLaserTemperatures(SubSys.LaserChC);
                        SensorTemperatureL1 = EthernetDevice.LaserTemperatures[LaserChannels.ChannelC].ToString();
                        EthernetDevice.GetSingeRadiatorTemperatures(SubSys.LaserChC);
                        SensorRadTemperaTureL1 = EthernetDevice.RadiatorTemperatures[LaserChannels.ChannelC];
                        L1ModuleTemperatureDiff = Convert.ToDouble(SensorRadTemperaTureL1) - Convert.ToDouble(SensorTemperatureL1);

                        //停止和低速
                        //Stop and low speed
                        if (ModuleLowTemperature > L1ModuleTemperatureDiff || MaxTemperature < InternalLowTemperature)
                        {
                            FanSleep = 0;
                        }
                        if (ModuleLowTemperature <= L1ModuleTemperatureDiff || MaxTemperature < InternalLowTemperature)
                        {
                            FanSleep = 1;
                        }

                        //停止和低中速
                        //Stop and low to medium speed
                        if (ModuleLowTemperature > L1ModuleTemperatureDiff || InternalLowTemperature <= MaxTemperature && MaxTemperature < InternalModerateTemperature)
                        {
                            FanSleep = 0;
                        }
                        if (ModuleLowTemperature <= L1ModuleTemperatureDiff && L1ModuleTemperatureDiff < ModuleModerateTemperature || InternalLowTemperature <= MaxTemperature && MaxTemperature < InternalModerateTemperature)
                        {
                            FanSleep = 1;
                        }
                        if (ModuleModerateTemperature <= L1ModuleTemperatureDiff && L1ModuleTemperatureDiff < ModuleHighTemperature || InternalLowTemperature <= MaxTemperature && MaxTemperature < InternalModerateTemperature)
                        {
                            FanSleep = 2;
                        }
                        if (ModuleHighTemperature <= L1ModuleTemperatureDiff || InternalLowTemperature <= MaxTemperature && MaxTemperature < InternalModerateTemperature)
                        {
                            FanSleep = 3;
                        }

                        //停止和低中高速
                        //Stop and low, medium, and high speed
                        if (ModuleLowTemperature > L1ModuleTemperatureDiff || InternalModerateTemperature <= MaxTemperature && MaxTemperature < InternalHighTemperature)
                        {
                            FanSleep = 0;
                        }
                        if (ModuleLowTemperature <= L1ModuleTemperatureDiff && L1ModuleTemperatureDiff < ModuleModerateTemperature || InternalModerateTemperature <= MaxTemperature && MaxTemperature < InternalHighTemperature)
                        {
                            FanSleep = 1;
                        }
                        if (ModuleModerateTemperature <= L1ModuleTemperatureDiff && L1ModuleTemperatureDiff < ModuleHighTemperature || InternalModerateTemperature <= MaxTemperature && MaxTemperature < InternalHighTemperature)
                        {
                            FanSleep = 2;
                        }
                        if (ModuleHighTemperature <= L1ModuleTemperatureDiff || InternalModerateTemperature <= MaxTemperature && MaxTemperature < InternalHighTemperature)
                        {
                            FanSleep = 3;
                        }
                        //high speed
                        if (InternalHighTemperature <= L1ModuleTemperatureDiff)
                        {
                            FanSleep = 3;
                        }

                    }
                    //R1通道有模块存在 There are modules present in the R1 channel
                    if (Workspace.This.IVVM.WR1 != 0 && Workspace.This.IVVM.WR1 != Workspace.This.NewParameterVM.Uint16Code && Workspace.This.OpticalModulePowerStatus)
                    {
                        EthernetDevice.GetSingeLaserTemperatures(SubSys.LaserChA);
                        SensorTemperatureR1 = EthernetDevice.LaserTemperatures[LaserChannels.ChannelA].ToString();
                        EthernetDevice.GetSingeRadiatorTemperatures(SubSys.LaserChA);
                        SensorRadTemperaTureR1 = EthernetDevice.RadiatorTemperatures[LaserChannels.ChannelA];
                        R1ModuleTemperatureDiff = Convert.ToDouble(SensorRadTemperaTureR1) - Convert.ToDouble(SensorTemperatureR1);
                        //停止和低速
                        //Stop and low speed
                        if (ModuleLowTemperature > R1ModuleTemperatureDiff || MaxTemperature < InternalLowTemperature)
                        {
                            FanSleep = 0;
                        }
                        if (ModuleLowTemperature <= R1ModuleTemperatureDiff || MaxTemperature < InternalLowTemperature)
                        {
                            FanSleep = 1;
                        }

                        //停止和低中速
                        //Stop and low to medium speed
                        if (ModuleLowTemperature > R1ModuleTemperatureDiff || InternalLowTemperature <= MaxTemperature && MaxTemperature < InternalModerateTemperature)
                        {
                            FanSleep = 0;
                        }
                        if (ModuleLowTemperature <= R1ModuleTemperatureDiff && R1ModuleTemperatureDiff < ModuleModerateTemperature || InternalLowTemperature <= MaxTemperature && MaxTemperature < InternalModerateTemperature)
                        {
                            FanSleep = 1;
                        }
                        if (ModuleModerateTemperature <= R1ModuleTemperatureDiff && R1ModuleTemperatureDiff < ModuleHighTemperature || InternalLowTemperature <= MaxTemperature && MaxTemperature < InternalModerateTemperature)
                        {
                            FanSleep = 2;
                        }
                        if (ModuleHighTemperature <= R1ModuleTemperatureDiff || InternalLowTemperature <= MaxTemperature && MaxTemperature < InternalModerateTemperature)
                        {
                            FanSleep = 3;
                        }

                        //停止和低中高速
                        //Stop and low, medium, and high speed
                        if (ModuleLowTemperature > R1ModuleTemperatureDiff || InternalModerateTemperature <= MaxTemperature && MaxTemperature < InternalHighTemperature)
                        {
                            FanSleep = 0;
                        }
                        if (ModuleLowTemperature <= R1ModuleTemperatureDiff && R1ModuleTemperatureDiff < ModuleModerateTemperature || InternalModerateTemperature <= MaxTemperature && MaxTemperature < InternalHighTemperature)
                        {
                            FanSleep = 1;
                        }
                        if (ModuleModerateTemperature <= R1ModuleTemperatureDiff && R1ModuleTemperatureDiff < ModuleHighTemperature || InternalModerateTemperature <= MaxTemperature && MaxTemperature < InternalHighTemperature)
                        {
                            FanSleep = 2;
                        }
                        if (ModuleHighTemperature <= R1ModuleTemperatureDiff || InternalModerateTemperature <= MaxTemperature && MaxTemperature < InternalHighTemperature)
                        {
                            FanSleep = 3;
                        }
                        //high speed
                        if (InternalHighTemperature <= R1ModuleTemperatureDiff)
                        {
                            FanSleep = 3;
                        }
                    }
                    //R2通道有模块存在 There are modules present in the R2 channel
                    if (Workspace.This.IVVM.WR2 != 0 && Workspace.This.IVVM.WR2 != Workspace.This.NewParameterVM.Uint16Code && Workspace.This.OpticalModulePowerStatus)
                    {
                        EthernetDevice.GetSingeLaserTemperatures(SubSys.LaserChB);
                        SensorTemperatureR2 = EthernetDevice.LaserTemperatures[LaserChannels.ChannelB].ToString();
                        EthernetDevice.GetSingeRadiatorTemperatures(SubSys.LaserChB);
                        SensorRadTemperaTureR2 = EthernetDevice.RadiatorTemperatures[LaserChannels.ChannelB];
                        R2ModuleTemperatureDiff = Convert.ToDouble(SensorRadTemperaTureR2) - Convert.ToDouble(SensorTemperatureR2);
                        //Stop and low speed
                        //停止和低速
                        if (ModuleLowTemperature > R2ModuleTemperatureDiff || MaxTemperature < InternalLowTemperature)
                        {
                            FanSleep = 0;
                        }
                        if (ModuleLowTemperature <= R2ModuleTemperatureDiff || MaxTemperature < InternalLowTemperature)
                        {
                            FanSleep = 1;
                        }
                        //Stop and low to medium speed
                        //停止和低中速
                        if (ModuleLowTemperature > R2ModuleTemperatureDiff || InternalLowTemperature <= MaxTemperature && MaxTemperature < InternalModerateTemperature)
                        {
                            FanSleep = 0;
                        }
                        if (ModuleLowTemperature <= R2ModuleTemperatureDiff && R2ModuleTemperatureDiff < ModuleModerateTemperature || InternalLowTemperature <= MaxTemperature && MaxTemperature < InternalModerateTemperature)
                        {
                            FanSleep = 1;
                        }
                        if (ModuleModerateTemperature <= R2ModuleTemperatureDiff && R2ModuleTemperatureDiff < ModuleHighTemperature || InternalLowTemperature <= MaxTemperature && MaxTemperature < InternalModerateTemperature)
                        {
                            FanSleep = 2;
                        }
                        if (ModuleHighTemperature <= R2ModuleTemperatureDiff || InternalLowTemperature <= MaxTemperature && MaxTemperature < InternalModerateTemperature)
                        {
                            FanSleep = 3;
                        }
                        //Stop and low, medium, and high speed
                        //停止和低中高速
                        if (ModuleLowTemperature > R2ModuleTemperatureDiff || InternalModerateTemperature <= MaxTemperature && MaxTemperature < InternalHighTemperature)
                        {
                            FanSleep = 0;
                        }
                        if (ModuleLowTemperature <= R2ModuleTemperatureDiff && R2ModuleTemperatureDiff < ModuleModerateTemperature || InternalModerateTemperature <= MaxTemperature && MaxTemperature < InternalHighTemperature)
                        {
                            FanSleep = 1;
                        }
                        if (ModuleModerateTemperature <= R2ModuleTemperatureDiff && R2ModuleTemperatureDiff < ModuleHighTemperature || InternalModerateTemperature <= MaxTemperature && MaxTemperature < InternalHighTemperature)
                        {
                            FanSleep = 2;
                        }
                        if (ModuleHighTemperature <= R2ModuleTemperatureDiff || InternalModerateTemperature <= MaxTemperature && MaxTemperature < InternalHighTemperature)
                        {
                            FanSleep = 3;
                        }
                        //high speed
                        if (InternalHighTemperature <= R2ModuleTemperatureDiff)
                        {
                            FanSleep = 3;
                        }
                    }
                    //根据AmbientTemperatureCH1和AmbientTemperatureCH2设置风扇等级  //Set fan level based on AmbientTemperatureCH1 and AmbientTemperatureCH2
                    if (Workspace.This.NewParameterVM.SelectedIncrustation == "Auto" || Workspace.This.NewParameterVM.SelectedIncrustation == null)
                    {
                        Workspace.This.EthernetController.SetIncrustationFan(1, FanSleep);
                    }
                    //CH1 Warning temperature
                    if (Workspace.This.NewParameterVM.CH1AlertWarningSwitch == 1 && AlerWindow)
                    {
                        if (CH1Temerature > Workspace.This.NewParameterVM.CH1WarningTemperature && Workspace.This.NewParameterVM.CH1WarningTemperature != 0)
                        {
                            //只弹框提醒一次
                            //Only pop up a reminder once
                            AlerWindow = false;
                            CH1TemperatureShow();
                        }
                    }
                }
                else
                {
                    //L通道有模块存在 There are modules present in the L channel
                    if (Workspace.This.IVVM.WL1 != 0 && Workspace.This.IVVM.WL1 != Workspace.This.NewParameterVM.Uint16Code)
                    {
                        EthernetDevice.GetSingeLaserTemperatures(SubSys.LaserChC);
                        SensorTemperatureL1 = EthernetDevice.LaserTemperatures[LaserChannels.ChannelC].ToString();
                        EthernetDevice.GetSingeRadiatorTemperatures(SubSys.LaserChC);
                        SensorRadTemperaTureL1 = EthernetDevice.RadiatorTemperatures[LaserChannels.ChannelC];
                    }
                    if (Workspace.This.IVVM.WR1 != 0 && Workspace.This.IVVM.WR1 != Workspace.This.NewParameterVM.Uint16Code)
                    {
                        EthernetDevice.GetSingeLaserTemperatures(SubSys.LaserChA);
                        SensorTemperatureR1 = EthernetDevice.LaserTemperatures[LaserChannels.ChannelA].ToString();
                        EthernetDevice.GetSingeRadiatorTemperatures(SubSys.LaserChA);
                        SensorRadTemperaTureR1 = EthernetDevice.RadiatorTemperatures[LaserChannels.ChannelA];
                    }
                    if (Workspace.This.IVVM.WR2 != 0 && Workspace.This.IVVM.WR2 != Workspace.This.NewParameterVM.Uint16Code)
                    {
                        EthernetDevice.GetSingeLaserTemperatures(SubSys.LaserChB);
                        SensorTemperatureR2 = EthernetDevice.LaserTemperatures[LaserChannels.ChannelB].ToString();
                        EthernetDevice.GetSingeRadiatorTemperatures(SubSys.LaserChB);
                        SensorRadTemperaTureR2 = EthernetDevice.RadiatorTemperatures[LaserChannels.ChannelB];
                    }

                }
                //StrTemperature[0] = string.Format("\n{0},{1},{2},{3},{4},{5}", SensorTemperatureL1, SensorTemperatureR1, SensorTemperatureR2, SensorRadTemperaTureL1, SensorRadTemperaTureR1, SensorRadTemperaTureR2);
                //WriteReportData(StrTemperature);
                //EthernetDevice.GetAllFanTemperatures();
                //SensorRadTemperaTureL1 = EthernetDevice.FanTemperatures[LaserChannels.ChannelC];
                //SensorRadTemperaTureR1 = EthernetDevice.FanTemperatures[LaserChannels.ChannelA];
                //SensorRadTemperaTureR2 = EthernetDevice.FanTemperatures[LaserChannels.ChannelB];

            }
        }

        public void WriteReportData(string[] Temperature)
        {
            if (ReportFile == null)
            {
                return;
            }
            if (ReportFile.Open(System.IO.FileAccess.ReadWrite))
            {
                Report.SetColumnContent(columnWriteCount++, Temperature);
                ReportFile.Write(Report.ToString());
            }
            ReportFile.Close();
        }


        private void OnTemperatureInit()
        {
            ShowRedTemperatureTimer = new Thread(OnTemperatureTime);
            ShowRedTemperatureTimer.IsBackground = true;
            ShowRedTemperatureTimer.Start();
        }

        private void GenerateReportTemperature()
        {
            string timeNow = FormatStrDate();
            string _filePath = string.Format(".\\TemperatureReport\\TemperatureReport_{0}.csv",
               timeNow);

            ReportFile = new FileVisit(_filePath);
            ReportFile.VisitError += ReportFile_VisitError;
            string LType = "NA";
            string R1Type = "NA";
            string R2Type = "NA";
            if (SensorML1 == IvSensorType.APD)
            {
                LType = "APD";
            }
            else if (SensorML1 == IvSensorType.PMT)
            {
                LType = "PMT";
            }
            else
            {
                LType = "NA";
            }
            if (SensorMR1 == IvSensorType.APD)
            {
                R1Type = "APD";
            }
            else if (SensorMR1 == IvSensorType.PMT)
            {
                R1Type = "PMT";
            }
            else
            {
                R1Type = "NA";
            }
            if (SensorMR2 == IvSensorType.APD)
            {
                R2Type = "APD";
            }
            else if (SensorMR2 == IvSensorType.PMT)
            {
                R2Type = "PMT";
            }
            else
            {
                R2Type = "NA";
            }
            if (ReportFile.Open(System.IO.FileAccess.ReadWrite))
            {
                StringBuilder _header = new StringBuilder("L:" + LType + "    R1: " + R1Type + "    R2: " + R2Type + "\n", 1024);
                List<string> columHeaderList = new List<string>();
                columHeaderList.Add("L-LaserTemperature");
                columHeaderList.Add("R1-LaserTemperature");
                columHeaderList.Add("R2-LaserTemperature");
                columHeaderList.Add("L-RadiatorTemperature");
                columHeaderList.Add("R1-RadiatorTemperature");
                columHeaderList.Add("R2-RadiatorTemperature");
                string[] _columnHeaders = columHeaderList.ToArray();
                Report = new CSVFile(_header.ToString(), _columnHeaders);
                ReportFile.Write(Report.ToString());
            }
            ReportFile.Close();
        }
        private string StrDate()
        {
            System.DateTime currentTime = DateTime.Now;//获取当前系统时间
            int month = currentTime.Month;
            int day = currentTime.Day;
            int hour = currentTime.Hour;
            int minute = currentTime.Minute;
            string newDay = "";
            if (day.ToString().Length == 1)
            {
                newDay = "0" + day.ToString();
            }
            else
            {
                newDay = day.ToString();
            }
            string timeNow = month.ToString() + newDay + hour.ToString() + minute.ToString();
            return timeNow;
        }
        private void ReportFile_VisitError(object sender, FileAccessErrorArgs e)
        {
            MessageBox.Show("File access error！\n" + e.Message);
        }
        private string FormatStrDate()
        {
            System.DateTime currentTime = DateTime.Now;//获取当前系统时间
            int Year = currentTime.Year;
            int month = currentTime.Month;
            int day = currentTime.Day;
            int hour = currentTime.Hour;
            int minute = currentTime.Minute;
            //string newDay = "";
            //if (day.ToString().Length == 1)
            //{
            //    newDay = "0" + day.ToString();
            //}
            //else
            //{
            //    newDay = day.ToString();
            //}
            string timeNow = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}", hour, ":", minute, " ", Year, "-", month, "-", day);
            return timeNow;
        }
        private void CH1TemperatureShow()
        {
            void msgSend()
            {
                Workspace.This.Owner.Dispatcher.BeginInvoke((Action)delegate
                {
                    Window window = new Window();
                    MessageBox.Show(window, "CH1 Temperature > " + Workspace.This.NewParameterVM.CH1WarningTemperature + "   Time is " + FormatStrDate());
                });
            }
            Thread td_msg = new Thread(msgSend);
            td_msg.Start();
        }
        #endregion
        #endregion

        #region IsControlReadOnly
        public bool IsReadOnly
        {
            get
            {
                return _IsReadOnly;
            }
            set
            {
                if (_IsReadOnly != value)
                {
                    _IsReadOnly = value;
                    RaisePropertyChanged("IsReadOnly");
                }
            }
        }

        #endregion

        #region WriteIVSensorCommand

        private RelayCommand _WriteIvCommand = null;

        public ICommand WriteIvCommand
        {
            get
            {
                if (_WriteIvCommand == null)
                {
                    _WriteIvCommand = new RelayCommand(this.Execute_WriteIvCommand, this.CanExecute_WriteIvCommand);
                }

                return _WriteIvCommand;
            }
        }
        public void Execute_WriteIvCommand(object parameter)
        {

            // IVType = 1;

        }

        public bool CanExecute_WriteIvCommand(object parameter)
        {
            return true;
        }


        #endregion
        //Reset PGA and Gain after the optical module is powered on
        public void ResetPGAandGain()
        {
            _APDGainOptions = SettingsManager.ConfigSettings.APDGains;
            _PGAOptionsModule = SettingsManager.ConfigSettings.APDPgas;
            RaisePropertyChanged("PGAOptionsModule");
            RaisePropertyChanged("GainComModule");
            if (_PGAOptionsModule != null && _PGAOptionsModule.Count >= 3)
            {
                SelectedMModuleL1 = _PGAOptionsModule[3];
                SelectedMModuleR1 = _PGAOptionsModule[3];
                SelectedMModuleR2 = _PGAOptionsModule[3];
            }
            if (_APDGainOptions != null && _APDGainOptions.Count >= 6)
            {
                SelectedGainComModuleL1 = _APDGainOptions[5];    // select the 6th item
                SelectedGainComModuleR1 = _APDGainOptions[5];    // select the 6th item
                SelectedGainComModuleR2 = _APDGainOptions[5];    // select the 6th item
                GainTxtModuleL1 = 4000;
                GainTxtModuleR1 = 4000;
                GainTxtModuleR2 = 4000;
            }
        }
    }
}