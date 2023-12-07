using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input; //ICommand
using Azure.WPF.Framework;  //RelayCommand
using Azure.ImagingSystem;
using Azure.Configuration.Settings;
using System.Windows;
using Azure.EthernetCommLib;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections.ObjectModel;

namespace Azure.LaserScanner.ViewModel
{
    class NewParameterSetupViewModel : ViewModelBase
    {
        #region private data

        private double _XMotorSubdivision = 0;
        private double _YMotorSubdivision = 0;
        private RelayCommand _ParametersWriteCommand = null;
        private RelayCommand _ParametersReadCommand = null;
        private int _R1XLogicalHome = 0;
        private int _R1YLogicalHome = 0;
        private int _OpticalR2_R1Distance = 0;
        private int _Pixel_10_Offset_R2_R1 = 0;
        private int _OpticalL_R1Distance = 0;
        private int _Pixel_10_Offset_L_R1 = 0;
        private int _Pixel_10_L_DX;
        private int _Pixel_10_L_DY;
        private int _Pixel_10_R1_DX;
        private int _Pixel_10_R1_DY;
        private int _Pixel_10_R2_DX;
        private int _Pixel_10_R2_DY;
        private float _FocusLength = 0;
        private float _XEncoderSubdivision;
        private float _FanSwitchInterval;
        private float _FanReserveTemperature;
        private string _SystemSN;
        private float _LCoefficient;
        private float _L375Coefficient;
        private float _R1Coefficient;
        private float _R2Coefficient;
        private float _R2532Coefficient;
        private ushort _ShellFanDefaultSpeed;

        private string _LIVFirmwareVersionSN = "0";
        private string _LIVFirmwareSN = "0";
        private double _LTEControlTemperature = 0;
        private double _LTECMaximumCoolingCurrent = 0;
        private double _LOpticalPowerLessThanOrEqual15mWKp = 0;
        private double _LOpticalPowerLessThanOrEqual15mWKi = 0;
        private double _LOpticalPowerLessThanOrEqual15mWKd = 0;
        private double _LOpticalPowerGreaterThan15mWKp = 0;
        private double _LOpticalPowerGreaterThan15mWKi = 0;
        private double _LOpticalPowerGreaterThan15mWKd = 0;
        private double _LOpticalPowerControlKpUpperLimitLessThanOrEqual15 = 0;
        private double _LOpticalPowerControlKpDownLimitLessThanOrEqual15 = 0;
        private double _LOpticalPowerControlKiUpperLimitLessThanOrEqual15 = 0;
        private double _LOpticalPowerControlKiDownLimitLessThanOrEqual15 = 0;
        private double _LOpticalPowerControlKpUpperLimitLessThan15 = 0;
        private double _LOpticalPowerControlKpDownLimitLessThan15 = 0;
        private double _LOpticalPowerControlKiUpperLimitLessThan15 = 0;
        private double _LOpticalPowerControlKiDownLimitLessThan15 = 0;
        private bool _IsLTEControlTemperature = false;
        private bool _IsLTECMaximumCoolingCurrent = false;
        private bool _IsLTECControlKp = false;
        private bool _IsLTECControlKi = false;
        private bool _IsLTECControlKd = false;
        private bool _IsLLaserMaxCurrent = false;
        private bool _IsLLaserMinCurrent = false;
        private bool _IsLOpticalPowerLessThanOrEqual15mWKp = false;
        private bool _IsLOpticalPowerLessThanOrEqual15mWKi = false;
        private bool _IsLOpticalPowerLessThanOrEqual15mWKd = false;
        private bool _IsLOpticalPowerGreaterThan15mWKp = false;
        private bool _IsLOpticalPowerGreaterThan15mWKi = false;
        private bool _IsLOpticalPowerGreaterThan15mWKd = false;

        private string _R1IVFirmwareVersionSN = "0";
        private string _R1IVFirmwareSN = "0";
        private double _R1TEControlTemperature = 0;
        private double _R1TECMaximumCoolingCurrent = 0;
        private double _R1OpticalPowerLessThanOrEqual15mWKp = 0;
        private double _R1OpticalPowerLessThanOrEqual15mWKi = 0;
        private double _R1OpticalPowerLessThanOrEqual15mWKd = 0;
        private double _R1OpticalPowerGreaterThan15mWKp = 0;
        private double _R1OpticalPowerGreaterThan15mWKi = 0;
        private double _R1OpticalPowerGreaterThan15mWKd = 0;
        private double _R1OpticalPowerControlKpUpperLimitLessThanOrEqual15 = 0;
        private double _R1OpticalPowerControlKpDownLimitLessThanOrEqual15 = 0;
        private double _R1OpticalPowerControlKiUpperLimitLessThanOrEqual15 = 0;
        private double _R1OpticalPowerControlKiDownLimitLessThanOrEqual15 = 0;
        private double _R1OpticalPowerControlKpUpperLimitLessThan15 = 0;
        private double _R1OpticalPowerControlKpDownLimitLessThan15 = 0;
        private double _R1OpticalPowerControlKiUpperLimitLessThan15 = 0;
        private double _R1OpticalPowerControlKiDownLimitLessThan15 = 0;
        private bool _IsR1TEControlTemperature = false;
        private bool _IsR1TECMaximumCoolingCurrent = false;
        private bool _IsR1TECControlKp = false;
        private bool _IsR1TECControlKi = false;
        private bool _IsR1TECControlKd = false;
        private bool _IsR1LaserMaxCurrent = false;
        private bool _IsR1LaserMinCurrent = false;
        private bool _IsR1OpticalPowerLessThanOrEqual15mWKp = false;
        private bool _IsR1OpticalPowerLessThanOrEqual15mWKi = false;
        private bool _IsR1OpticalPowerLessThanOrEqual15mWKd = false;
        private bool _IsR1OpticalPowerGreaterThan15mWKp = false;
        private bool _IsR1OpticalPowerGreaterThan15mWKi = false;
        private bool _IsR1OpticalPowerGreaterThan15mWKd = false;

        private string _R2IVFirmwareVersionSN = "0";
        private string _R2IVFirmwareSN = "0";
        private double _R2TEControlTemperature = 0;
        private double _R2TECMaximumCoolingCurrent = 0;
        private double _R2OpticalPowerLessThanOrEqual15mWKp = 0;
        private double _R2OpticalPowerLessThanOrEqual15mWKi = 0;
        private double _R2OpticalPowerLessThanOrEqual15mWKd = 0;
        private double _R2OpticalPowerGreaterThan15mWKp = 0;
        private double _R2OpticalPowerGreaterThan15mWKi = 0;
        private double _R2OpticalPowerGreaterThan15mWKd = 0;
        private double _R2OpticalPowerControlKpUpperLimitLessThanOrEqual15 = 0;
        private double _R2OpticalPowerControlKpDownLimitLessThanOrEqual15 = 0;
        private double _R2OpticalPowerControlKiUpperLimitLessThanOrEqual15 = 0;
        private double _R2OpticalPowerControlKiDownLimitLessThanOrEqual15 = 0;
        private double _R2OpticalPowerControlKpUpperLimitLessThan15 = 0;
        private double _R2OpticalPowerControlKpDownLimitLessThan15 = 0;
        private double _R2OpticalPowerControlKiUpperLimitLessThan15 = 0;
        private double _R2OpticalPowerControlKiDownLimitLessThan15 = 0;
        private bool _IsR2TEControlTemperature = false;
        private bool _IsR2TECMaximumCoolingCurrent = false;
        private bool _IsR2TECControlKp = false;
        private bool _IsR2TECControlKi = false;
        private bool _IsR2TECControlKd = false;
        private bool _IsR2LaserMaxCurrent = false;
        private bool _IsR2LaserMinCurrent = false;
        private bool _IsR2OpticalPowerLessThanOrEqual15mWKp = false;
        private bool _IsR2OpticalPowerLessThanOrEqual15mWKi = false;
        private bool _IsR2OpticalPowerLessThanOrEqual15mWKd = false;
        private bool _IsR2OpticalPowerGreaterThan15mWKp = false;
        private bool _IsR2OpticalPowerGreaterThan15mWKi = false;
        private bool _IsR2OpticalPowerGreaterThan15mWKd = false;

        private double _LLaserMaxCurrent = 0;
        private double _LLaserMinCurrent = 0;
        private double _R1LaserMaxCurrent = 0;
        private double _R1LaserMinCurrent = 0;
        private double _R2LaserMaxCurrent = 0;
        private double _R2LaserMinCurrent = 0;

        private double _LTECControlKp = 0;
        private double _LTECControlKi = 0;
        private double _LTECControlKd = 0;
        private double _R1TECControlKp = 0;
        private double _R1TECControlKi = 0;
        private double _R1TECControlKd = 0;
        private double _R2TECControlKp = 0;
        private double _R2TECControlKi = 0;
        private double _R2TECControlKd = 0;

        private bool _IsEnabledL1 = false;
        private bool _IsEnabledR1 = false;
        private bool _IsEnabledR2 = false;

        private Visibility _Is532EnabledL1 = Visibility.Hidden;
        private Visibility _Is532EnabledR1 = Visibility.Hidden;
        private Visibility _Is532EnabledR2 = Visibility.Hidden;

        private bool _IsLCheckALL = false;
        private bool _IsR1CheckALL = false;
        private bool _IsR2CheckALL = false;

        private string _SelectedIncrustation; //Shell Fan
        private bool _IsShowParameterWindow = false;
        private ObservableCollection<string> _IncrustationOptions = new ObservableCollection<string>();
        private Visibility _VesionVisFlag = Visibility.Visible;   //当硬件版本是1.1时显示光学模块下电按钮，相反不显示光学模块下电按钮 When the hardware version is 1.1, the optical module power-off button is displayed, and if not, the optical module power-off button is displayed
        private Visibility _LEDVesionVisFlag = Visibility.Visible;   //当硬件版本是1.1并且LED版本是254.255.255.255不显示 Not displayed when the hardware version is 1.1 and the LED version is 254.255.255.255
        private bool _OtherSettingBtnIsEnable = true;
        //0xFE代表是非法值, invalid value
        public uint Uint8Code = 0xFE;
        //0xFFFE代表是非法值， invalid value
        public uint Uint16Code = 0xFFFE;
        //0xFFFE代表是非法值， invalid value
        public string Str16Code = "254.255.255.255";

        //0xFFFFFFFE代表是非法值， invalid value
        public uint Uint32Code = 0xFFFFFFFE;
        //FFFF255254代表是非法值 ， invalid value
        public string StrEmptyCode = "FFFF255254";
        //FFFFFFFE代表是非法值 ，invalid value
        public string StrEmptyCode1 = "FFFFFFFE";
        //invalid value
        public string DefaultStrCode = "NaN";

        public int DefaultIntCode = -1;

        private Visibility _CH1AlertFlag = Visibility.Hidden;
        private ObservableCollection<string> _CH1AlertWarningOptions = new ObservableCollection<string>();
        private string _SelectedCH1AlertWarning;
        private ushort _CH1AlertWarningSwitch;
        private float _CH1WarningTemperature;
        #endregion
        public void Initialize()
        {
            //When the configuration parameters are incorrect, it means that the current device is new and given default parameters
            if (Workspace.This.EthernetController.DeviceProperties.OpticalLR1Distance != 24 && Workspace.This.EthernetController.DeviceProperties.PixelOffsetR1 != 2400 &&
              Workspace.This.EthernetController.DeviceProperties.OpticalLR2Distance != 48 && Workspace.This.EthernetController.DeviceProperties.PixelOffsetR2 != 4800 &&
              Workspace.This.EthernetController.DeviceProperties.ZFocusPosition != 1 && Workspace.This.EthernetController.DeviceProperties.XEncoderSubdivision != 1000 &&
              Workspace.This.EthernetController.DeviceProperties.FanReserveTemperature != 1400 && Workspace.This.EthernetController.DeviceProperties.FanSwitchInterval != 400)
            {
                MessageBoxResult boxResult = MessageBoxResult.None;
                boxResult = MessageBox.Show("Currently, no personalization parameters are detected. Do you want to write the default value！\n", "warning", MessageBoxButton.YesNo);
                if (boxResult == MessageBoxResult.Yes)
                {
                    LCoefficient = 0.15F;
                    L375Coefficient = 0.45F;
                    R1Coefficient = 0.15F;
                    R2Coefficient = 0.15F;
                    R2532Coefficient = 0.2F;
                    R1XLogicalHome = 10;
                    R1YLogicalHome = 10;
                    OpticalR2_R1Distance = 24;
                    Pixel_10_Offset_R2_R1 = 2400;
                    OpticalL_R1Distance = 48;
                    Pixel_10_Offset_L_R1 = 4800;
                    Pixel_10_L_DX = 0;
                    Pixel_10_L_DY = 0;
                    Pixel_10_R2_DX = 0;
                    Pixel_10_R2_DY = 0;
                    FocusLength = 1;
                    XEncoderSubdivision = 1000;
                    FanSwitchInterval = 2;
                    FanReserveTemperature = 24;
                    SystemSN = "0";
                    ShellFanDefaultSpeed = 2;
                    CH1AlertWarningSwitch = 0;
                    CH1WarningTemperature = 30;
                    ExecuteParametersWriteCommand(null);
                }
            }

            _XMotorSubdivision = SettingsManager.ConfigSettings.XMotorSubdivision;
            _YMotorSubdivision = SettingsManager.ConfigSettings.YMotorSubdivision;
            //Housing fan rating
            if (_IncrustationOptions.Count == 0)
            {
                _IncrustationOptions.Add("Auto");
                _IncrustationOptions.Add("0");
                _IncrustationOptions.Add("1");
                _IncrustationOptions.Add("2");
                _IncrustationOptions.Add("3");
                SelectedIncrustation = _IncrustationOptions[0];

            }
            // TODO: read current parameter values
            //
            if (_CH1AlertWarningOptions.Count == 0)
            {
                _CH1AlertWarningOptions.Add("Close");
                _CH1AlertWarningOptions.Add("Open");
                SelectedCH1AlertWarning = _CH1AlertWarningOptions[0];
            }
        }
        #region public properties
        public double R1XLogicalHome
        {
            get
            {
                //double result = 0;
                //if (_XMotorSubdivision > 0)
                //    result = Math.Round((double)_R1XLogicalHome / (double)_XMotorSubdivision, 3);
                //return result;
                return _R1XLogicalHome;
            }
            set
            {
                if (_R1XLogicalHome != value)
                {
                    //_R1XLogicalHome = (int)(value * _XMotorSubdivision);
                    _R1XLogicalHome = (int)value;
                    RaisePropertyChanged("R1XLogicalHome");
                }
            }
        }
        public double R1YLogicalHome
        {
            get
            {
                //double result = 0;
                //if (_YMotorSubdivision > 0)
                //    result = Math.Round((double)_R1YLogicalHome / (double)_YMotorSubdivision, 3);
                //return result;
                return _R1YLogicalHome;
            }
            set
            {
                if (_R1YLogicalHome != value)
                {
                    //_R1YLogicalHome = (int)(value * _YMotorSubdivision);
                    _R1YLogicalHome = (int)value;
                    RaisePropertyChanged("R1YLogicalHome");
                }
            }
        }
        public double OpticalR2_R1Distance
        {
            get
            {
                //double result = 0;
                //if (_XMotorSubdivision > 0)
                //    result = Math.Round((double)_OpticalR2_R1Distance / (double)_XMotorSubdivision, 3);
                //return result;
                return _OpticalR2_R1Distance;
            }
            set
            {
                if (_OpticalR2_R1Distance != value)
                {
                    //_OpticalR2_R1Distance = (int)(value * _XMotorSubdivision);
                    _OpticalR2_R1Distance = (int)value;
                    RaisePropertyChanged("OpticalR2_R1Distance");
                }
            }
        }
        public int Pixel_10_Offset_R2_R1
        {
            get { return _Pixel_10_Offset_R2_R1; }
            set
            {
                if (_Pixel_10_Offset_R2_R1 != value)
                {
                    _Pixel_10_Offset_R2_R1 = value;
                    RaisePropertyChanged("Pixel_10_Offset_R2_R1");
                }
            }
        }
        public double OpticalL_R1Distance
        {
            get
            {
                //double result = 0;
                //if (_XMotorSubdivision > 0)
                //    result = Math.Round((double)_OpticalL_R1Distance / (double)_XMotorSubdivision, 3);
                //return result;
                return _OpticalL_R1Distance;
            }
            set
            {
                if (_OpticalL_R1Distance != value)
                {
                    //    _OpticalL_R1Distance = (int)(value * _XMotorSubdivision);
                    _OpticalL_R1Distance = (int)value;
                    RaisePropertyChanged("OpticalL_R1Distance");
                }
            }
        }
        public int Pixel_10_Offset_L_R1
        {
            get { return _Pixel_10_Offset_L_R1; }
            set
            {
                if (_Pixel_10_Offset_L_R1 != value)
                {
                    _Pixel_10_Offset_L_R1 = value;
                    RaisePropertyChanged("Pixel_10_Offset_L_R1");
                }
            }
        }
        public int Pixel_10_L_DX
        {
            get { return _Pixel_10_L_DX; }
            set
            {
                if (_Pixel_10_L_DX != value)
                {
                    _Pixel_10_L_DX = value;
                    RaisePropertyChanged("Pixel_10_L_DX");
                }
            }
        }
        public int Pixel_10_L_DY
        {
            get { return _Pixel_10_L_DY; }
            set
            {
                if (_Pixel_10_L_DY != value)
                {
                    _Pixel_10_L_DY = value;
                    RaisePropertyChanged("Pixel_10_L_DY");
                }
            }
        }
        public int Pixel_10_R1_DX
        {
            get { return _Pixel_10_R1_DX; }
            set
            {
                if (_Pixel_10_R1_DX != value)
                {
                    _Pixel_10_R1_DX = value;
                    RaisePropertyChanged("Pixel_10_R1_DX");
                }
            }
        }
        public int Pixel_10_R1_DY
        {
            get { return _Pixel_10_R1_DY; }
            set
            {
                if (_Pixel_10_R1_DY != value)
                {
                    _Pixel_10_R1_DY = value;
                    RaisePropertyChanged("Pixel_10_R1_DY");
                }
            }
        }
        public int Pixel_10_R2_DX
        {
            get { return _Pixel_10_R2_DX; }
            set
            {
                if (_Pixel_10_R2_DX != value)
                {
                    _Pixel_10_R2_DX = value;
                    RaisePropertyChanged("Pixel_10_R2_DX");
                }
            }
        }
        public int Pixel_10_R2_DY
        {
            get { return _Pixel_10_R2_DY; }
            set
            {
                if (_Pixel_10_R2_DY != value)
                {
                    _Pixel_10_R2_DY = value;
                    RaisePropertyChanged("Pixel_10_R2_DY");
                }
            }
        }
        public float FocusLength
        {
            get
            {
                return _FocusLength;
            }
            set
            {
                if (_FocusLength != value)
                {
                    _FocusLength = value;
                    RaisePropertyChanged("FocusLength");
                }
            }
        }
        public string SystemSN
        {
            get { return _SystemSN; }
            set
            {
                if (_SystemSN != value)
                {
                    _SystemSN = value;
                    RaisePropertyChanged("SystemSN");
                }
            }
        }
        public float XEncoderSubdivision
        {
            get { return _XEncoderSubdivision; }
            set
            {
                if (_XEncoderSubdivision != value)
                {
                    _XEncoderSubdivision = value;
                    RaisePropertyChanged("XEncoderSubdivision");
                }
            }
        }
        public float FanReserveTemperature
        {
            get { return _FanReserveTemperature; }
            set
            {
                if (_FanReserveTemperature != value)
                {
                    _FanReserveTemperature = value;
                    RaisePropertyChanged("FanReserveTemperature");
                }
            }
        }

        public float LCoefficient
        {
            get { return _LCoefficient; }
            set
            {
                if (_LCoefficient != value)
                {
                    _LCoefficient = value;
                    RaisePropertyChanged("LCoefficient");
                }
            }
        }

        public float L375Coefficient
        {
            get { return _L375Coefficient; }
            set
            {
                if (_L375Coefficient != value)
                {
                    _L375Coefficient = value;
                    RaisePropertyChanged("L375Coefficient");
                }
            }
        }
        public float R1Coefficient
        {
            get { return _R1Coefficient; }
            set
            {
                if (_R1Coefficient != value)
                {
                    _R1Coefficient = value;
                    RaisePropertyChanged("R1Coefficient");
                }
            }
        }
        public float R2Coefficient
        {
            get { return _R2Coefficient; }
            set
            {
                if (_R2Coefficient != value)
                {
                    _R2Coefficient = value;
                    RaisePropertyChanged("R2Coefficient");
                }
            }
        }

        public float R2532Coefficient
        {
            get { return _R2532Coefficient; }
            set
            {
                if (_R2532Coefficient != value)
                {
                    _R2532Coefficient = value;
                    RaisePropertyChanged("R2532Coefficient");
                }
            }
        }


        //当软件和设备断开连接时默认外壳风扇等级，The default enclosure fan rating is when the software and device are disconnected,
        public ushort ShellFanDefaultSpeed
        {
            get { return _ShellFanDefaultSpeed; }
            set
            {
                if (_ShellFanDefaultSpeed != value)
                {
                    _ShellFanDefaultSpeed = value;
                    RaisePropertyChanged("ShellFanDefaultSpeed");
                }
            }
        }
        public float FanSwitchInterval
        {
            get { return _FanSwitchInterval; }
            set
            {
                if (_FanSwitchInterval != value)
                {
                    _FanSwitchInterval = value;
                    RaisePropertyChanged("FanSwitchInterval");
                }
            }
        }

        //Shell Fan
        public ObservableCollection<string> IncrustationOptions
        {
            get { return _IncrustationOptions; }

        }
        // selectd Shell Fan level
        public string SelectedIncrustation
        {
            get { return _SelectedIncrustation; }
            set
            {
                if (_SelectedIncrustation != value)
                {
                    _SelectedIncrustation = value;
                    if (value != "Auto")
                    {
                        //Manually control the fan level
                        Workspace.This.EthernetController.SetIncrustationFan(1, Convert.ToInt32(value));
                    }
                    RaisePropertyChanged("SelectedIncrustation");
                }
            }
        }

        public Visibility VesionVisFlag
        {
            get
            {
                return _VesionVisFlag;

            }
            set
            {
                if (_VesionVisFlag != value)
                {
                    _VesionVisFlag = value;
                    RaisePropertyChanged("VesionVisFlag");
                }
            }
        }
        public Visibility LEDVesionVisFlag
        {
            get
            {
                return _LEDVesionVisFlag;

            }
            set
            {
                if (_LEDVesionVisFlag != value)
                {
                    _LEDVesionVisFlag = value;
                    RaisePropertyChanged("LEDVesionVisFlag");
                }
            }
        }

        public bool IsShowParameterWindow
        {
            get { return _IsShowParameterWindow; }
            set
            {
                if (_IsShowParameterWindow != value)
                {
                    _IsShowParameterWindow = value;
                }
            }
        }
        public bool OtherSettingBtnIsEnable
        {
            get
            {
                return _OtherSettingBtnIsEnable;

            }
            set
            {
                if (_OtherSettingBtnIsEnable != value)
                {
                    _OtherSettingBtnIsEnable = value;
                    RaisePropertyChanged("OtherSettingBtnIsEnable");
                }
            }
        }
        //
        public Visibility CH1AlertFlag
        {
            get
            {
                return _CH1AlertFlag;

            }
            set
            {
                if (_CH1AlertFlag != value)
                {
                    _CH1AlertFlag = value;
                    RaisePropertyChanged("CH1AlertFlag");
                }
            }
        }
        public float CH1WarningTemperature
        {
            get { return _CH1WarningTemperature; }
            set
            {
                if (_CH1WarningTemperature != value)
                {
                    _CH1WarningTemperature = value;
                    RaisePropertyChanged("CH1WarningTemperature");
                }
            }
        }
        public ushort CH1AlertWarningSwitch
        {
            get { return _CH1AlertWarningSwitch; }
            set
            {
                if (_CH1AlertWarningSwitch != value)
                {
                    _CH1AlertWarningSwitch = value;
                    if (value == 0)
                    {
                        SelectedCH1AlertWarning = "Close";
                    }
                    else
                    {
                        SelectedCH1AlertWarning = "Open";
                    }
                }
            }
        }

        //CH1 Warning
        public ObservableCollection<string> CH1AlertWarningOptions
        {
            get { return _CH1AlertWarningOptions; }

        }

        // selectd Shell Fan level
        public string SelectedCH1AlertWarning
        {
            get { return _SelectedCH1AlertWarning; }
            set
            {
                if (_SelectedCH1AlertWarning != value)
                {
                    _SelectedCH1AlertWarning = value;
                    if (value == "Close")
                    {
                        CH1AlertWarningSwitch = 0;
                        CH1AlertFlag = Visibility.Hidden;
                    }
                    else
                    {
                        CH1AlertWarningSwitch = 1;
                        CH1AlertFlag = Visibility.Visible;
                    }
                    RaisePropertyChanged("SelectedCH1AlertWarning");
                }
            }
        }

        #region  L 

        public string LIVFirmwareVersionSN
        {
            get { return _LIVFirmwareVersionSN; }
            set
            {
                if (_LIVFirmwareVersionSN != value)
                {
                    if (StrEmptyCode == value || value == StrEmptyCode1)
                    {
                        _LIVFirmwareVersionSN = DefaultStrCode;
                        RaisePropertyChanged("LIVFirmwareVersionSN");
                    }
                    else
                    {
                        _LIVFirmwareVersionSN = value;
                        RaisePropertyChanged("LIVFirmwareVersionSN");
                    }

                }
            }
        }

        public string LIVFirmwareSN
        {
            get { return _LIVFirmwareSN; }
            set
            {
                if (_LIVFirmwareSN != value)
                {
                    if (value == StrEmptyCode || value == StrEmptyCode1)
                    {
                        _LIVFirmwareSN = DefaultStrCode;
                        RaisePropertyChanged("LIVFirmwareSN");
                    }
                    else
                    {
                        _LIVFirmwareSN = value;
                        RaisePropertyChanged("LIVFirmwareSN");
                    }
                }
            }
        }

        public double LTEControlTemperature
        {
            get { return _LTEControlTemperature; }
            set
            {
                if (_LTEControlTemperature != value)
                {
                    if ((int)(value * 10) == Uint16Code)
                    {
                        _LTEControlTemperature = double.NaN;
                        IsLTEControlTemperature = false;
                        RaisePropertyChanged("IsLTEControlTemperature");
                        RaisePropertyChanged("LTEControlTemperature");
                    }
                    else
                    {
                        _LTEControlTemperature = value;
                        RaisePropertyChanged("LTEControlTemperature");
                    }
                }
            }
        }
        public double LTECMaximumCoolingCurrent
        {
            get { return _LTECMaximumCoolingCurrent; }
            set
            {
                if (_LTECMaximumCoolingCurrent != value)
                {
                    if ((uint)(value) == Uint16Code)
                    {
                        IsLTECMaximumCoolingCurrent = false;
                        RaisePropertyChanged("IsLTECMaximumCoolingCurrent");
                        _LTECMaximumCoolingCurrent = double.NaN;
                        RaisePropertyChanged("LTECMaximumCoolingCurrent");
                    }
                    else
                    {
                        _LTECMaximumCoolingCurrent = value;
                        RaisePropertyChanged("LTECMaximumCoolingCurrent");
                    }
                }
            }
        }
        public double LOpticalPowerLessThanOrEqual15mWKp
        {
            get { return _LOpticalPowerLessThanOrEqual15mWKp; }
            set
            {
                if (_LOpticalPowerLessThanOrEqual15mWKp != value)
                {
                    if ((int)(value) == Uint16Code)
                    {
                        _LOpticalPowerLessThanOrEqual15mWKp = double.NaN;
                        IsLOpticalPowerLessThanOrEqual15mWKp = false;
                        RaisePropertyChanged("IsLOpticalPowerLessThanOrEqual15mWKp");
                        RaisePropertyChanged("LOpticalPowerLessThanOrEqual15mWKp");
                    }
                    else
                    {
                        _LOpticalPowerLessThanOrEqual15mWKp = value;
                        RaisePropertyChanged("LOpticalPowerLessThanOrEqual15mWKp");
                    }
                }
            }
        }

        public double LOpticalPowerLessThanOrEqual15mWKi
        {
            get { return _LOpticalPowerLessThanOrEqual15mWKi; }
            set
            {
                if (_LOpticalPowerLessThanOrEqual15mWKi != value)
                {
                    if ((int)(value) == Uint16Code)
                    {
                        IsLOpticalPowerLessThanOrEqual15mWKi = false;
                        RaisePropertyChanged("IsLOpticalPowerLessThanOrEqual15mWKi");
                        _LOpticalPowerLessThanOrEqual15mWKi = double.NaN;
                        RaisePropertyChanged("LOpticalPowerLessThanOrEqual15mWKi");
                    }
                    else
                    {
                        _LOpticalPowerLessThanOrEqual15mWKi = value;
                        RaisePropertyChanged("LOpticalPowerLessThanOrEqual15mWKi");
                    }
                }
            }
        }

        public double LOpticalPowerLessThanOrEqual15mWKd
        {
            get { return _LOpticalPowerLessThanOrEqual15mWKd; }
            set
            {
                if (_LOpticalPowerLessThanOrEqual15mWKd != value)
                {
                    if ((int)(value) == Uint16Code)
                    {
                        IsLOpticalPowerLessThanOrEqual15mWKd = false;
                        RaisePropertyChanged("IsLOpticalPowerLessThanOrEqual15mWKd");
                        _LOpticalPowerLessThanOrEqual15mWKd = double.NaN;
                        RaisePropertyChanged("LOpticalPowerLessThanOrEqual15mWKd");
                    }
                    else
                    {
                        _LOpticalPowerLessThanOrEqual15mWKd = value;
                        RaisePropertyChanged("LOpticalPowerLessThanOrEqual15mWKd");
                    }
                }
            }
        }
        public double LOpticalPowerGreaterThan15mWKp
        {
            get { return _LOpticalPowerGreaterThan15mWKp; }
            set
            {
                if (_LOpticalPowerGreaterThan15mWKp != value)
                {
                    if ((int)(value) == Uint16Code)
                    {
                        IsLOpticalPowerGreaterThan15mWKp = false;
                        RaisePropertyChanged("IsLOpticalPowerGreaterThan15mWKp");
                        _LOpticalPowerGreaterThan15mWKp = double.NaN;
                        RaisePropertyChanged("LOpticalPowerGreaterThan15mWKp");
                    }
                    else
                    {
                        _LOpticalPowerGreaterThan15mWKp = value;
                        RaisePropertyChanged("LOpticalPowerGreaterThan15mWKp");
                    }
                }
            }
        }
        public double LOpticalPowerGreaterThan15mWKi
        {
            get { return _LOpticalPowerGreaterThan15mWKi; }
            set
            {
                if (_LOpticalPowerGreaterThan15mWKi != value)
                {
                    if ((int)(value) == Uint16Code)
                    {
                        IsLOpticalPowerGreaterThan15mWKi = false;
                        RaisePropertyChanged("IsLOpticalPowerGreaterThan15mWKi");
                        _LOpticalPowerGreaterThan15mWKi = double.NaN;
                        RaisePropertyChanged("LOpticalPowerGreaterThan15mWKi");
                    }
                    else
                    {
                        _LOpticalPowerGreaterThan15mWKi = value;
                        RaisePropertyChanged("LOpticalPowerGreaterThan15mWKi");
                    }
                }
            }
        }

        public double LOpticalPowerGreaterThan15mWKd
        {
            get { return _LOpticalPowerGreaterThan15mWKd; }
            set
            {
                if (_LOpticalPowerGreaterThan15mWKd != value)
                {
                    if ((int)(value) == Uint16Code)
                    {
                        IsLOpticalPowerGreaterThan15mWKd = false;
                        RaisePropertyChanged("IsLOpticalPowerGreaterThan15mWKd");
                        _LOpticalPowerGreaterThan15mWKd = double.NaN;
                        RaisePropertyChanged("LOpticalPowerGreaterThan15mWKd");
                    }
                    else
                    {
                        _LOpticalPowerGreaterThan15mWKd = value;
                        RaisePropertyChanged("LOpticalPowerGreaterThan15mWKd");
                    }
                }
            }
        }

        public double LOpticalPowerControlKpUpperLimitLessThanOrEqual15
        {
            get { return _LOpticalPowerControlKpUpperLimitLessThanOrEqual15; }
            set
            {
                if (_LOpticalPowerControlKpUpperLimitLessThanOrEqual15 != value)
                {
                    if ((int)(value) == Uint16Code)
                    {
                        _LOpticalPowerControlKpUpperLimitLessThanOrEqual15 = double.NaN;
                        RaisePropertyChanged("LOpticalPowerControlKpUpperLimitLessThanOrEqual15");
                    }
                    else
                    {
                        _LOpticalPowerControlKpUpperLimitLessThanOrEqual15 = value;
                        RaisePropertyChanged("LOpticalPowerControlKpUpperLimitLessThanOrEqual15");
                    }
                }
            }
        }

        public double LOpticalPowerControlKpDownLimitLessThanOrEqual15
        {
            get { return _LOpticalPowerControlKpDownLimitLessThanOrEqual15; }
            set
            {
                if (_LOpticalPowerControlKpDownLimitLessThanOrEqual15 != value)
                {
                    if ((int)(value) == Uint16Code)
                    {
                        _LOpticalPowerControlKpDownLimitLessThanOrEqual15 = double.NaN;
                        RaisePropertyChanged("LOpticalPowerControlKpDownLimitLessThanOrEqual15");
                    }
                    else
                    {
                        _LOpticalPowerControlKpDownLimitLessThanOrEqual15 = value;
                        RaisePropertyChanged("LOpticalPowerControlKpDownLimitLessThanOrEqual15");
                    }
                }
            }
        }

        public double LOpticalPowerControlKiUpperLimitLessThanOrEqual15
        {
            get { return _LOpticalPowerControlKiUpperLimitLessThanOrEqual15; }
            set
            {
                if (_LOpticalPowerControlKiUpperLimitLessThanOrEqual15 != value)
                {
                    if ((int)(value) == Uint16Code)
                    {
                        _LOpticalPowerControlKiUpperLimitLessThanOrEqual15 = double.NaN;
                        RaisePropertyChanged("LOpticalPowerControlKiUpperLimitLessThanOrEqual15");
                    }
                    else
                    {
                        _LOpticalPowerControlKiUpperLimitLessThanOrEqual15 = value;
                        RaisePropertyChanged("LOpticalPowerControlKiUpperLimitLessThanOrEqual15");
                    }
                }
            }
        }

        public double LOpticalPowerControlKiDownLimitLessThanOrEqual15
        {
            get { return _LOpticalPowerControlKiDownLimitLessThanOrEqual15; }
            set
            {
                if (_LOpticalPowerControlKiDownLimitLessThanOrEqual15 != value)
                {
                    if ((int)(value) == Uint16Code)
                    {
                        _LOpticalPowerControlKiDownLimitLessThanOrEqual15 = double.NaN;
                        RaisePropertyChanged("LOpticalPowerControlKiDownLimitLessThanOrEqual15");
                    }
                    else
                    {
                        _LOpticalPowerControlKiDownLimitLessThanOrEqual15 = value;
                        RaisePropertyChanged("LOpticalPowerControlKiDownLimitLessThanOrEqual15");
                    }
                }
            }
        }

        public double LOpticalPowerControlKpUpperLimitLessThan15
        {
            get { return _LOpticalPowerControlKpUpperLimitLessThan15; }
            set
            {
                if (_LOpticalPowerControlKpUpperLimitLessThan15 != value)
                {
                    if ((int)(value) == Uint16Code)
                    {
                        _LOpticalPowerControlKpUpperLimitLessThan15 = double.NaN;
                        RaisePropertyChanged("LOpticalPowerControlKpUpperLimitLessThan15");
                    }
                    else
                    {
                        _LOpticalPowerControlKpUpperLimitLessThan15 = value;
                        RaisePropertyChanged("LOpticalPowerControlKpUpperLimitLessThan15");
                    }
                }
            }
        }

        public double LOpticalPowerControlKpDownLimitLessThan15
        {
            get { return _LOpticalPowerControlKpDownLimitLessThan15; }
            set
            {
                if (_LOpticalPowerControlKpDownLimitLessThan15 != value)
                {
                    if ((int)(value) == Uint16Code)
                    {
                        _LOpticalPowerControlKpDownLimitLessThan15 = double.NaN;
                        RaisePropertyChanged("LOpticalPowerControlKpDownLimitLessThan15");
                    }
                    else
                    {
                        _LOpticalPowerControlKpDownLimitLessThan15 = value;
                        RaisePropertyChanged("LOpticalPowerControlKpDownLimitLessThan15");
                    }
                }
            }
        }

        public double LOpticalPowerControlKiUpperLimitLessThan15
        {
            get { return _LOpticalPowerControlKiUpperLimitLessThan15; }
            set
            {
                if (_LOpticalPowerControlKiUpperLimitLessThan15 != value)
                {
                    if ((int)(value) == Uint16Code)
                    {
                        _LOpticalPowerControlKiUpperLimitLessThan15 = double.NaN;
                        RaisePropertyChanged("LOpticalPowerControlKiUpperLimitLessThan15");
                    }
                    else
                    {
                        _LOpticalPowerControlKiUpperLimitLessThan15 = value;
                        RaisePropertyChanged("LOpticalPowerControlKiUpperLimitLessThan15");
                    }
                }
            }
        }

        public double LOpticalPowerControlKiDownLimitLessThan15
        {
            get { return _LOpticalPowerControlKiDownLimitLessThan15; }
            set
            {
                if (_LOpticalPowerControlKiDownLimitLessThan15 != value)
                {
                    if ((int)(value) == Uint16Code)
                    {
                        _LOpticalPowerControlKiDownLimitLessThan15 = double.NaN;
                        RaisePropertyChanged("LOpticalPowerControlKiDownLimitLessThan15");
                    }
                    else
                    {
                        _LOpticalPowerControlKiDownLimitLessThan15 = value;
                        RaisePropertyChanged("LOpticalPowerControlKiDownLimitLessThan15");
                    }
                }
            }
        }
        public double LLaserMaxCurrent
        {
            get { return _LLaserMaxCurrent; }
            set
            {
                if (_LLaserMaxCurrent != value)
                {
                    if ((int)(value * 10) == Uint16Code)
                    {
                        IsLLaserMaxCurrent = false;
                        RaisePropertyChanged("IsLLaserMaxCurrent");
                        _LLaserMaxCurrent = double.NaN;
                        RaisePropertyChanged("LLaserMaxCurrent");
                    }
                    else
                    {
                        _LLaserMaxCurrent = value;
                        RaisePropertyChanged("LLaserMaxCurrent");
                    }
                }
            }
        }

        public double LLaserMinCurrent
        {
            get { return _LLaserMinCurrent; }
            set
            {
                if (_LLaserMinCurrent != value)
                {
                    if ((int)(value * 10) == Uint16Code)
                    {
                        IsLLaserMinCurrent = false;
                        RaisePropertyChanged("IsLLaserMinCurrent");
                        _LLaserMaxCurrent = double.NaN;
                        RaisePropertyChanged("LLaserMinCurrent");
                    }
                    else
                    {
                        _LLaserMinCurrent = value;
                        RaisePropertyChanged("LLaserMinCurrent");
                    }
                }
            }
        }


        public double LTECControlKp
        {
            get { return _LTECControlKp; }
            set
            {
                if (_LTECControlKp != value)
                {
                    if ((int)(value) == Uint16Code)
                    {
                        IsLTECControlKp = false;
                        RaisePropertyChanged("IsLTECControlKp");
                        _LTECControlKp = double.NaN;
                        RaisePropertyChanged("LTECControlKp");
                    }
                    else
                    {
                        _LTECControlKp = value;
                        RaisePropertyChanged("LTECControlKp");
                    }
                }
            }
        }
        public double LTECControlKi
        {
            get { return _LTECControlKi; }
            set
            {
                if (_LTECControlKi != value)
                {
                    if ((int)(value) == Uint16Code)
                    {
                        IsLTECControlKi = false;
                        RaisePropertyChanged("IsLTECControlKi");
                        _LTECControlKi = double.NaN;
                        RaisePropertyChanged("LTECControlKi");
                    }
                    else
                    {
                        _LTECControlKi = value;
                        RaisePropertyChanged("LTECControlKi");
                    }

                }
            }
        }
        public double LTECControlKd
        {
            get { return _LTECControlKd; }
            set
            {
                if (_LTECControlKd != value)
                {
                    if ((int)(value) == Uint16Code)
                    {
                        IsLTECControlKd = false;
                        RaisePropertyChanged("IsLTECControlKd");
                        _LTECControlKd = double.NaN;
                        RaisePropertyChanged("LTECControlKd");
                    }
                    else
                    {
                        _LTECControlKd = value;
                        RaisePropertyChanged("LTECControlKd");
                    }
                }
            }
        }

        public bool IsEnabledL1
        {
            get { return _IsEnabledL1; }
            set
            {
                _IsEnabledL1 = value;
                RaisePropertyChanged("IsEnabledL1");
            }
        }


        public Visibility Is532EnabledL1
        {
            get { return _Is532EnabledL1; }
            set
            {
                _Is532EnabledL1 = value;
                RaisePropertyChanged("Is532EnabledL1");
            }
        }

        #region CheckBox
        public bool IsLCheckALL
        {
            get { return _IsLCheckALL; }
            set
            {
                _IsLCheckALL = value;
                if (Workspace.This.LaserModuleL1.LaserWavelength != 0 && Workspace.This.LaserModuleL1.LaserWavelength != Workspace.This.NewParameterVM.Uint16Code)
                {
                    IsLTEControlTemperature = value;
                    IsLTECMaximumCoolingCurrent = value;
                    IsLTECControlKp = value;
                    IsLTECControlKi = value;
                    IsLTECControlKd = value;
                    if (Workspace.This.LaserModuleL1.LaserWavelength == 532)
                    {
                        IsLLaserMaxCurrent = value;
                        IsLLaserMinCurrent = value;
                        IsLOpticalPowerLessThanOrEqual15mWKp = value;
                        IsLOpticalPowerLessThanOrEqual15mWKi = value;
                        IsLOpticalPowerLessThanOrEqual15mWKd = value;
                        IsLOpticalPowerGreaterThan15mWKp = value;
                        IsLOpticalPowerGreaterThan15mWKi = value;
                        IsLOpticalPowerGreaterThan15mWKd = value;
                    }
                }
                RaisePropertyChanged("IsLCheckALL");
            }
        }
        public bool IsLTEControlTemperature
        {
            get { return _IsLTEControlTemperature; }
            set
            {
                if (_IsLTEControlTemperature != value)
                {
                    if (double.IsNaN(LTEControlTemperature))
                    {
                        //_IsLCheckALL = false;
                        //RaisePropertyChanged("IsLCheckALL");
                        _IsLTEControlTemperature = false;
                        RaisePropertyChanged("IsLTEControlTemperature");
                    }
                    else
                    {
                        if (!value)
                        {
                            _IsLCheckALL = false;
                            RaisePropertyChanged("IsLCheckALL");
                        }
                        _IsLTEControlTemperature = value;
                        RaisePropertyChanged("IsLTEControlTemperature");
                    }
                }
            }
        }
        public bool IsLTECMaximumCoolingCurrent
        {
            get { return _IsLTECMaximumCoolingCurrent; }
            set
            {
                if (_IsLTECMaximumCoolingCurrent != value)
                {
                    if (double.IsNaN(LTECMaximumCoolingCurrent))
                    {
                        //_IsLCheckALL = false;
                        //RaisePropertyChanged("IsLCheckALL");
                        _IsLTECMaximumCoolingCurrent = false;
                        RaisePropertyChanged("IsLTECMaximumCoolingCurrent");
                    }
                    else
                    {
                        if (!value)
                        {
                            _IsLCheckALL = false;
                            RaisePropertyChanged("IsLCheckALL");
                        }
                        _IsLTECMaximumCoolingCurrent = value;
                        RaisePropertyChanged("IsLTECMaximumCoolingCurrent");
                    }
                }
            }
        }
        public bool IsLOpticalPowerLessThanOrEqual15mWKp
        {
            get { return _IsLOpticalPowerLessThanOrEqual15mWKp; }
            set
            {
                if (_IsLOpticalPowerLessThanOrEqual15mWKp != value)
                {
                    if (double.IsNaN(LOpticalPowerLessThanOrEqual15mWKp))
                    {
                        //_IsLCheckALL = false;
                        //RaisePropertyChanged("IsLCheckALL");
                        _IsLOpticalPowerLessThanOrEqual15mWKp = false;
                        RaisePropertyChanged("IsLOpticalPowerLessThanOrEqual15mWKp");
                    }
                    else
                    {
                        if (!value)
                        {
                            _IsLCheckALL = false;
                            RaisePropertyChanged("IsLCheckALL");
                        }
                        _IsLOpticalPowerLessThanOrEqual15mWKp = value;
                        RaisePropertyChanged("IsLOpticalPowerLessThanOrEqual15mWKp");
                    }
                }
            }
        }

        public bool IsLOpticalPowerLessThanOrEqual15mWKi
        {
            get { return _IsLOpticalPowerLessThanOrEqual15mWKi; }
            set
            {
                if (_IsLOpticalPowerLessThanOrEqual15mWKi != value)
                {
                    if (double.IsNaN(LOpticalPowerLessThanOrEqual15mWKi))
                    {
                        //_IsLCheckALL = false;
                        //RaisePropertyChanged("IsLCheckALL");
                        _IsLOpticalPowerLessThanOrEqual15mWKi = false;
                        RaisePropertyChanged("IsLOpticalPowerLessThanOrEqual15mWKi");
                    }
                    else
                    {
                        if (!value)
                        {
                            _IsLCheckALL = false;
                            RaisePropertyChanged("IsLCheckALL");
                        }
                        _IsLOpticalPowerLessThanOrEqual15mWKi = value;
                        RaisePropertyChanged("IsLOpticalPowerLessThanOrEqual15mWKi");
                    }
                }
            }
        }

        public bool IsLOpticalPowerLessThanOrEqual15mWKd
        {
            get { return _IsLOpticalPowerLessThanOrEqual15mWKd; }
            set
            {
                if (_IsLOpticalPowerLessThanOrEqual15mWKd != value)
                {
                    if (double.IsNaN(LOpticalPowerLessThanOrEqual15mWKd))
                    {
                        //_IsLCheckALL = false;
                        //RaisePropertyChanged("IsLCheckALL");
                        _IsLOpticalPowerLessThanOrEqual15mWKd = false;
                        RaisePropertyChanged("IsLOpticalPowerLessThanOrEqual15mWKd");
                    }
                    else
                    {
                        if (!value)
                        {
                            _IsLCheckALL = false;
                            RaisePropertyChanged("IsLCheckALL");
                        }
                        _IsLOpticalPowerLessThanOrEqual15mWKd = value;
                        RaisePropertyChanged("IsLOpticalPowerLessThanOrEqual15mWKd");
                    }
                }
            }
        }
        public bool IsLOpticalPowerGreaterThan15mWKp
        {
            get { return _IsLOpticalPowerGreaterThan15mWKp; }
            set
            {
                if (_IsLOpticalPowerGreaterThan15mWKp != value)
                {
                    if (double.IsNaN(LOpticalPowerGreaterThan15mWKp))
                    {
                        //_IsLCheckALL = false;
                        //RaisePropertyChanged("IsLCheckALL");
                        _IsLOpticalPowerGreaterThan15mWKp = false;
                        RaisePropertyChanged("IsLOpticalPowerGreaterThan15mWKp");
                    }
                    else
                    {
                        if (!value)
                        {
                            _IsLCheckALL = false;
                            RaisePropertyChanged("IsLCheckALL");
                        }
                        _IsLOpticalPowerGreaterThan15mWKp = value;
                        RaisePropertyChanged("IsLOpticalPowerGreaterThan15mWKp");
                    }
                }
            }
        }
        public bool IsLOpticalPowerGreaterThan15mWKi
        {
            get { return _IsLOpticalPowerGreaterThan15mWKi; }
            set
            {
                if (_IsLOpticalPowerGreaterThan15mWKi != value)
                {
                    if (double.IsNaN(LOpticalPowerGreaterThan15mWKi))
                    {
                        //_IsLCheckALL = false;
                        //RaisePropertyChanged("IsLCheckALL");
                        _IsLOpticalPowerGreaterThan15mWKi = false;
                        RaisePropertyChanged("IsLOpticalPowerGreaterThan15mWKi");
                    }
                    else
                    {
                        if (!value)
                        {
                            _IsLCheckALL = false;
                            RaisePropertyChanged("IsLCheckALL");
                        }
                        _IsLOpticalPowerGreaterThan15mWKi = value;
                        RaisePropertyChanged("IsLOpticalPowerGreaterThan15mWKi");
                    }
                }
            }
        }

        public bool IsLOpticalPowerGreaterThan15mWKd
        {
            get { return _IsLOpticalPowerGreaterThan15mWKd; }
            set
            {
                if (_IsLOpticalPowerGreaterThan15mWKd != value)
                {
                    if (double.IsNaN(LOpticalPowerGreaterThan15mWKd))
                    {
                        //_IsLCheckALL = false;
                        //RaisePropertyChanged("IsLCheckALL");
                        _IsLOpticalPowerGreaterThan15mWKd = false;
                        RaisePropertyChanged("IsLOpticalPowerGreaterThan15mWKd");
                    }
                    else
                    {
                        if (!value)
                        {
                            _IsLCheckALL = false;
                            RaisePropertyChanged("IsLCheckALL");
                        }
                        _IsLOpticalPowerGreaterThan15mWKd = value;
                        RaisePropertyChanged("IsLOpticalPowerGreaterThan15mWKd");
                    }
                }
            }
        }

        public bool IsLLaserMaxCurrent
        {
            get { return _IsLLaserMaxCurrent; }
            set
            {
                if (_IsLLaserMaxCurrent != value)
                {
                    if (double.IsNaN(LLaserMaxCurrent))
                    {
                        //_IsLCheckALL = false;
                        //RaisePropertyChanged("IsLCheckALL");
                        _IsLLaserMaxCurrent = false;
                        RaisePropertyChanged("IsLLaserMaxCurrent");
                    }
                    else
                    {
                        if (!value)
                        {
                            _IsLCheckALL = false;
                            RaisePropertyChanged("IsLCheckALL");
                        }
                        _IsLLaserMaxCurrent = value;
                        RaisePropertyChanged("IsLLaserMaxCurrent");
                    }
                }
            }
        }

        public bool IsLLaserMinCurrent
        {
            get { return _IsLLaserMinCurrent; }
            set
            {
                if (_IsLLaserMinCurrent != value)
                {
                    if (double.IsNaN(LLaserMinCurrent))
                    {
                        //_IsLCheckALL = false;
                        //RaisePropertyChanged("IsLCheckALL");
                        _IsLLaserMinCurrent = false;
                        RaisePropertyChanged("IsLLaserMinCurrent");
                    }
                    else
                    {
                        if (!value)
                        {
                            _IsLCheckALL = false;
                            RaisePropertyChanged("IsLCheckALL");
                        }
                        _IsLLaserMinCurrent = value;
                        RaisePropertyChanged("IsLLaserMinCurrent");
                    }
                }
            }
        }


        public bool IsLTECControlKp
        {
            get { return _IsLTECControlKp; }
            set
            {
                if (_IsLTECControlKp != value)
                {
                    if (double.IsNaN(LTECControlKp))
                    {
                        //_IsLCheckALL = false;
                        //RaisePropertyChanged("IsLCheckALL");
                        _IsLTECControlKp = false;
                        RaisePropertyChanged("IsLTECControlKp");
                    }
                    else
                    {
                        if (!value)
                        {
                            _IsLCheckALL = false;
                            RaisePropertyChanged("IsLCheckALL");
                        }
                        _IsLTECControlKp = value;
                        RaisePropertyChanged("IsLTECControlKp");
                    }
                }
            }
        }
        public bool IsLTECControlKi
        {
            get { return _IsLTECControlKi; }
            set
            {
                if (_IsLTECControlKi != value)
                {
                    if (double.IsNaN(LTECControlKi))
                    {
                        //_IsLCheckALL = false;
                        //RaisePropertyChanged("IsLCheckALL");
                        _IsLTECControlKi = false;
                        RaisePropertyChanged("IsLTECControlKi");
                    }
                    else
                    {
                        if (!value)
                        {
                            _IsLCheckALL = false;
                            RaisePropertyChanged("IsLCheckALL");
                        }
                        _IsLTECControlKi = value;
                        RaisePropertyChanged("IsLTECControlKi");
                    }
                }
            }
        }
        public bool IsLTECControlKd
        {
            get { return _IsLTECControlKd; }
            set
            {
                if (_IsLTECControlKd != value)
                {
                    if (double.IsNaN(LTECControlKd))
                    {
                        //_IsLCheckALL = false;
                        //RaisePropertyChanged("IsLCheckALL");
                        _IsLTECControlKd = false;
                        RaisePropertyChanged("IsLTECControlKd");
                    }
                    else
                    {
                        if (!value)
                        {
                            _IsLCheckALL = false;
                            RaisePropertyChanged("IsLCheckALL");
                        }
                        _IsLTECControlKd = value;
                        RaisePropertyChanged("IsLTECControlKd");
                    }
                }
            }
        }
        #endregion

        #endregion

        #region  R1

        public string R1IVFirmwareVersionSN
        {
            get { return _R1IVFirmwareVersionSN; }
            set
            {
                if (_R1IVFirmwareVersionSN != value)
                {
                    if (value == StrEmptyCode || value == StrEmptyCode1)
                    {
                        _R1IVFirmwareVersionSN = DefaultStrCode;
                        RaisePropertyChanged("R1IVFirmwareVersionSN");
                    }
                    else
                    {
                        _R1IVFirmwareVersionSN = value;
                        RaisePropertyChanged("R1IVFirmwareVersionSN");
                    }
                }
            }
        }

        public string R1IVFirmwareSN
        {
            get { return _R1IVFirmwareSN; }
            set
            {
                if (_R1IVFirmwareSN != value)
                {
                    if (value == StrEmptyCode || value == StrEmptyCode1)
                    {
                        _R1IVFirmwareSN = DefaultStrCode;
                        RaisePropertyChanged("R1IVFirmwareSN");
                    }
                    else
                    {
                        _R1IVFirmwareSN = value;
                        RaisePropertyChanged("R1IVFirmwareSN");
                    }
                }
            }
        }

        public double R1TEControlTemperature
        {
            get { return _R1TEControlTemperature; }
            set
            {
                if (_R1TEControlTemperature != value)
                {
                    if ((int)(value * 10) == Uint16Code)
                    {
                        IsR1TEControlTemperature = false;
                        RaisePropertyChanged("IsR1TEControlTemperature");
                        _R1TEControlTemperature = double.NaN;
                        RaisePropertyChanged("R1TEControlTemperature");
                    }
                    else
                    {
                        _R1TEControlTemperature = value;
                        RaisePropertyChanged("R1TEControlTemperature");
                    }
                }
            }
        }
        public double R1TECMaximumCoolingCurrent
        {
            get { return _R1TECMaximumCoolingCurrent; }
            set
            {
                if (_R1TECMaximumCoolingCurrent != value)
                {
                    if ((uint)(value) == Uint16Code)
                    {
                        IsR1TECMaximumCoolingCurrent = false;
                        RaisePropertyChanged("IsR1TECMaximumCoolingCurrent");
                        _R1TECMaximumCoolingCurrent = double.NaN;
                        RaisePropertyChanged("R1TECMaximumCoolingCurrent");
                    }
                    else
                    {
                        _R1TECMaximumCoolingCurrent = value;
                        RaisePropertyChanged("R1TECMaximumCoolingCurrent");
                    }
                }
            }
        }
        public double R1OpticalPowerLessThanOrEqual15mWKp
        {
            get { return _R1OpticalPowerLessThanOrEqual15mWKp; }
            set
            {
                if (_R1OpticalPowerLessThanOrEqual15mWKp != value)
                {
                    if ((int)(value) == Uint16Code)
                    {
                        IsR1OpticalPowerLessThanOrEqual15mWKp = false;
                        RaisePropertyChanged("IsR1OpticalPowerLessThanOrEqual15mWKp");
                        _R1OpticalPowerLessThanOrEqual15mWKp = double.NaN;
                        RaisePropertyChanged("R1OpticalPowerLessThanOrEqual15mWKp");
                    }
                    else
                    {
                        _R1OpticalPowerLessThanOrEqual15mWKp = value;
                        RaisePropertyChanged("R1OpticalPowerLessThanOrEqual15mWKp");
                    }
                }
            }
        }

        public double R1OpticalPowerLessThanOrEqual15mWKi
        {
            get { return _R1OpticalPowerLessThanOrEqual15mWKi; }
            set
            {
                if (_R1OpticalPowerLessThanOrEqual15mWKi != value)
                {
                    if ((int)(value) == Uint16Code)
                    {
                        IsR1OpticalPowerLessThanOrEqual15mWKi = false;
                        RaisePropertyChanged("IsR1OpticalPowerLessThanOrEqual15mWKi");
                        _R1OpticalPowerLessThanOrEqual15mWKi = double.NaN;
                        RaisePropertyChanged("R1OpticalPowerLessThanOrEqual15mWKi");
                    }
                    else
                    {
                        _R1OpticalPowerLessThanOrEqual15mWKi = value;
                        RaisePropertyChanged("R1OpticalPowerLessThanOrEqual15mWKi");
                    }
                }
            }
        }

        public double R1OpticalPowerLessThanOrEqual15mWKd
        {
            get { return _R1OpticalPowerLessThanOrEqual15mWKd; }
            set
            {
                if (_R1OpticalPowerLessThanOrEqual15mWKd != value)
                {
                    if ((int)(value) == Uint16Code)
                    {
                        IsR1OpticalPowerLessThanOrEqual15mWKd = false;
                        RaisePropertyChanged("IsR1OpticalPowerLessThanOrEqual15mWKd");
                        _R1OpticalPowerLessThanOrEqual15mWKd = double.NaN;
                        RaisePropertyChanged("R1OpticalPowerLessThanOrEqual15mWKd");
                    }
                    else
                    {
                        _R1OpticalPowerLessThanOrEqual15mWKd = value;
                        RaisePropertyChanged("R1OpticalPowerLessThanOrEqual15mWKd");
                    }
                }
            }
        }
        public double R1OpticalPowerGreaterThan15mWKp
        {
            get { return _R1OpticalPowerGreaterThan15mWKp; }
            set
            {
                if (_R1OpticalPowerGreaterThan15mWKp != value)
                {
                    if ((int)(value) == Uint16Code)
                    {
                        IsR1OpticalPowerGreaterThan15mWKp = false;
                        RaisePropertyChanged("IsR1OpticalPowerGreaterThan15mWKp");
                        _R1OpticalPowerGreaterThan15mWKp = double.NaN;
                        RaisePropertyChanged("R1OpticalPowerGreaterThan15mWKp");
                    }
                    else
                    {
                        _R1OpticalPowerGreaterThan15mWKp = value;
                        RaisePropertyChanged("R1OpticalPowerGreaterThan15mWKp");
                    }
                }
            }
        }
        public double R1OpticalPowerGreaterThan15mWKi
        {
            get { return _R1OpticalPowerGreaterThan15mWKi; }
            set
            {
                if (_R1OpticalPowerGreaterThan15mWKi != value)
                {
                    if ((int)(value) == Uint16Code)
                    {
                        IsR1OpticalPowerGreaterThan15mWKi = false;
                        RaisePropertyChanged("IsR1OpticalPowerGreaterThan15mWKi");
                        _R1OpticalPowerGreaterThan15mWKi = double.NaN;
                        RaisePropertyChanged("R1OpticalPowerGreaterThan15mWKi");
                    }
                    else
                    {
                        _R1OpticalPowerGreaterThan15mWKi = value;
                        RaisePropertyChanged("R1OpticalPowerGreaterThan15mWKi");
                    }
                }
            }
        }

        public double R1OpticalPowerGreaterThan15mWKd
        {
            get { return _R1OpticalPowerGreaterThan15mWKd; }
            set
            {
                if (_R1OpticalPowerGreaterThan15mWKd != value)
                {
                    if ((int)(value) == Uint16Code)
                    {
                        IsR1OpticalPowerGreaterThan15mWKd = false;
                        RaisePropertyChanged("IsR1OpticalPowerGreaterThan15mWKd");
                        _R1OpticalPowerGreaterThan15mWKd = double.NaN;
                        RaisePropertyChanged("R1OpticalPowerGreaterThan15mWKd");
                    }
                    else
                    {
                        _R1OpticalPowerGreaterThan15mWKd = value;
                        RaisePropertyChanged("R1OpticalPowerGreaterThan15mWKd");
                    }
                }
            }
        }

        public double R1OpticalPowerControlKpUpperLimitLessThanOrEqual15
        {
            get { return _R1OpticalPowerControlKpUpperLimitLessThanOrEqual15; }
            set
            {
                if (_R1OpticalPowerControlKpUpperLimitLessThanOrEqual15 != value)
                {
                    if ((int)(value) == Uint16Code)
                    {
                        _R1OpticalPowerControlKpUpperLimitLessThanOrEqual15 = double.NaN;
                        RaisePropertyChanged("R1OpticalPowerControlKpUpperLimitLessThanOrEqual15");
                    }
                    else
                    {
                        _R1OpticalPowerControlKpUpperLimitLessThanOrEqual15 = value;
                        RaisePropertyChanged("R1OpticalPowerControlKpUpperLimitLessThanOrEqual15");
                    }
                }
            }
        }

        public double R1OpticalPowerControlKpDownLimitLessThanOrEqual15
        {
            get { return _R1OpticalPowerControlKpDownLimitLessThanOrEqual15; }
            set
            {
                if (_R1OpticalPowerControlKpDownLimitLessThanOrEqual15 != value)
                {
                    if ((int)(value) == Uint16Code)
                    {
                        _R1OpticalPowerControlKpUpperLimitLessThanOrEqual15 = double.NaN;
                        RaisePropertyChanged("R1OpticalPowerControlKpDownLimitLessThanOrEqual15");
                    }
                    else
                    {
                        _R1OpticalPowerControlKpDownLimitLessThanOrEqual15 = value;
                        RaisePropertyChanged("R1OpticalPowerControlKpDownLimitLessThanOrEqual15");
                    }
                }
            }
        }

        public double R1OpticalPowerControlKiUpperLimitLessThanOrEqual15
        {
            get { return _R1OpticalPowerControlKiUpperLimitLessThanOrEqual15; }
            set
            {
                if (_R1OpticalPowerControlKiUpperLimitLessThanOrEqual15 != value)
                {
                    if ((int)(value) == Uint16Code)
                    {
                        _R1OpticalPowerControlKiUpperLimitLessThanOrEqual15 = double.NaN;
                        RaisePropertyChanged("R1OpticalPowerControlKiUpperLimitLessThanOrEqual15");
                    }
                    else
                    {
                        _R1OpticalPowerControlKiUpperLimitLessThanOrEqual15 = value;
                        RaisePropertyChanged("R1OpticalPowerControlKiUpperLimitLessThanOrEqual15");
                    }
                }
            }
        }

        public double R1OpticalPowerControlKiDownLimitLessThanOrEqual15
        {
            get { return _R1OpticalPowerControlKiDownLimitLessThanOrEqual15; }
            set
            {
                if (_R1OpticalPowerControlKiDownLimitLessThanOrEqual15 != value)
                {
                    if ((int)(value) == Uint16Code)
                    {
                        _R1OpticalPowerControlKiDownLimitLessThanOrEqual15 = double.NaN;
                        RaisePropertyChanged("R1OpticalPowerControlKiDownLimitLessThanOrEqual15");
                    }
                    else
                    {
                        _R1OpticalPowerControlKiDownLimitLessThanOrEqual15 = value;
                        RaisePropertyChanged("R1OpticalPowerControlKiDownLimitLessThanOrEqual15");
                    }
                }
            }
        }

        public double R1OpticalPowerControlKpUpperLimitLessThan15
        {
            get { return _R1OpticalPowerControlKpUpperLimitLessThan15; }
            set
            {
                if (_R1OpticalPowerControlKpUpperLimitLessThan15 != value)
                {
                    if ((int)(value) == Uint16Code)
                    {
                        _R1OpticalPowerControlKpUpperLimitLessThan15 = double.NaN;
                        RaisePropertyChanged("R1OpticalPowerControlKpUpperLimitLessThan15");
                    }
                    else
                    {
                        _R1OpticalPowerControlKpUpperLimitLessThan15 = value;
                        RaisePropertyChanged("R1OpticalPowerControlKpUpperLimitLessThan15");
                    }
                }
            }
        }

        public double R1OpticalPowerControlKpDownLimitLessThan15
        {
            get { return _R1OpticalPowerControlKpDownLimitLessThan15; }
            set
            {
                if (_R1OpticalPowerControlKpDownLimitLessThan15 != value)
                {
                    if ((int)(value) == Uint16Code)
                    {
                        _R1OpticalPowerControlKpDownLimitLessThan15 = double.NaN;
                        RaisePropertyChanged("R1OpticalPowerControlKpDownLimitLessThan15");
                    }
                    else
                    {
                        _R1OpticalPowerControlKpDownLimitLessThan15 = value;
                        RaisePropertyChanged("R1OpticalPowerControlKpDownLimitLessThan15");
                    }
                }
            }
        }

        public double R1OpticalPowerControlKiUpperLimitLessThan15
        {
            get { return _R1OpticalPowerControlKiUpperLimitLessThan15; }
            set
            {
                if (_R1OpticalPowerControlKiUpperLimitLessThan15 != value)
                {
                    if ((int)(value) == Uint16Code)
                    {
                        _R1OpticalPowerControlKiUpperLimitLessThan15 = double.NaN;
                        RaisePropertyChanged("R1OpticalPowerControlKiUpperLimitLessThan15");
                    }
                    else
                    {
                        _R1OpticalPowerControlKiUpperLimitLessThan15 = value;
                        RaisePropertyChanged("R1OpticalPowerControlKiUpperLimitLessThan15");
                    }
                }
            }
        }

        public double R1OpticalPowerControlKiDownLimitLessThan15
        {
            get { return _R1OpticalPowerControlKiDownLimitLessThan15; }
            set
            {
                if (_R1OpticalPowerControlKiDownLimitLessThan15 != value)
                {
                    if ((int)(value) == Uint16Code)
                    {
                        _R1OpticalPowerControlKiDownLimitLessThan15 = double.NaN;
                        RaisePropertyChanged("R1OpticalPowerControlKiDownLimitLessThan15");
                    }
                    else
                    {
                        _R1OpticalPowerControlKiDownLimitLessThan15 = value;
                        RaisePropertyChanged("R1OpticalPowerControlKiDownLimitLessThan15");
                    }
                }
            }
        }
        public double R1LaserMaxCurrent
        {
            get { return _R1LaserMaxCurrent; }
            set
            {
                if (_R1LaserMaxCurrent != value)
                {
                    if ((int)(value * 10) == Uint16Code)
                    {
                        IsR1LaserMaxCurrent = false;
                        RaisePropertyChanged("IsR1LaserMaxCurrent");
                        _R1LaserMaxCurrent = double.NaN;
                        RaisePropertyChanged("R1LaserMaxCurrent");
                    }
                    else
                    {
                        _R1LaserMaxCurrent = value;
                        RaisePropertyChanged("R1LaserMaxCurrent");
                    }
                }
            }
        }

        public double R1LaserMinCurrent
        {
            get { return _R1LaserMinCurrent; }
            set
            {
                if (_R1LaserMinCurrent != value)
                {
                    if ((int)(value * 10) == Uint16Code)
                    {
                        IsR1LaserMinCurrent = false;
                        RaisePropertyChanged("IsR1LaserMinCurrent");
                        _R1LaserMinCurrent = double.NaN;
                        RaisePropertyChanged("R1LaserMinCurrent");
                    }
                    else
                    {
                        _R1LaserMinCurrent = value;
                        RaisePropertyChanged("R1LaserMinCurrent");
                    }
                }
            }
        }

        public double R1TECControlKp
        {
            get { return _R1TECControlKp; }
            set
            {
                if (_R1TECControlKp != value)
                {
                    if ((int)(value) == Uint16Code)
                    {
                        IsR1TECControlKp = false;
                        RaisePropertyChanged("IsR1TECControlKp");
                        _R1TECControlKp = double.NaN;
                        RaisePropertyChanged("R1TECControlKp");
                    }
                    else
                    {
                        _R1TECControlKp = value;
                        RaisePropertyChanged("R1TECControlKp");
                    }
                }
            }
        }
        public double R1TECControlKi
        {
            get { return _R1TECControlKi; }
            set
            {
                if (_R1TECControlKi != value)
                {
                    if ((int)(value) == Uint16Code)
                    {
                        IsR1TECControlKi = false;
                        RaisePropertyChanged("IsR1TECControlKi");
                        _R1TECControlKi = double.NaN;
                        RaisePropertyChanged("R1TECControlKi");
                    }
                    else
                    {
                        _R1TECControlKi = value;
                        RaisePropertyChanged("R1TECControlKi");
                    }
                }
            }
        }
        public double R1TECControlKd
        {
            get { return _R1TECControlKd; }
            set
            {
                if (_R1TECControlKd != value)
                {
                    if ((int)(value) == Uint16Code)
                    {
                        IsR1TECControlKd = false;
                        RaisePropertyChanged("IsR1TECControlKd");
                        _R1TECControlKd = double.NaN;
                        RaisePropertyChanged("R1TECControlKd");
                    }
                    else
                    {
                        _R1TECControlKd = value;
                        RaisePropertyChanged("R1TECControlKd");
                    }
                }
            }
        }
        public bool IsEnabledR1
        {
            get { return _IsEnabledR1; }
            set
            {
                _IsEnabledR1 = value;
                RaisePropertyChanged("IsEnabledR1");
            }
        }
        public Visibility Is532EnabledR1
        {
            get { return _Is532EnabledR1; }
            set
            {
                _Is532EnabledR1 = value;
                RaisePropertyChanged("Is532EnabledR1");
            }
        }

        #region CheckBox
        public bool IsR1CheckALL
        {
            get { return _IsR1CheckALL; }
            set
            {
                _IsR1CheckALL = value;
                if (Workspace.This.LaserModuleR1.LaserWavelength != 0 && Workspace.This.LaserModuleR1.LaserWavelength != Workspace.This.NewParameterVM.Uint16Code)
                {
                    IsR1TEControlTemperature = value;
                    IsR1TECMaximumCoolingCurrent = value;
                    IsR1TECControlKp = value;
                    IsR1TECControlKi = value;
                    IsR1TECControlKd = value;
                    if (Workspace.This.LaserModuleR1.LaserWavelength == 532)
                    {
                        IsR1LaserMaxCurrent = value;
                        IsR1LaserMinCurrent = value;
                        IsR1OpticalPowerLessThanOrEqual15mWKp = value;
                        IsR1OpticalPowerLessThanOrEqual15mWKi = value;
                        IsR1OpticalPowerLessThanOrEqual15mWKd = value;
                        IsR1OpticalPowerGreaterThan15mWKp = value;
                        IsR1OpticalPowerGreaterThan15mWKi = value;
                        IsR1OpticalPowerGreaterThan15mWKd = value;
                    }
                }
                RaisePropertyChanged("IsR1CheckALL");
            }
        }
        public bool IsR1TEControlTemperature
        {
            get { return _IsR1TEControlTemperature; }
            set
            {
                if (_IsR1TEControlTemperature != value)
                {
                    if (double.IsNaN(R1TEControlTemperature))
                    {
                        //_IsR1CheckALL = false;
                        //RaisePropertyChanged("IsR1CheckALL");
                        _IsR1TEControlTemperature = false;
                        RaisePropertyChanged("IsR1TEControlTemperature");
                    }
                    else
                    {
                        if (!value)
                        {
                            _IsR1CheckALL = false;
                            RaisePropertyChanged("IsR1CheckALL");
                        }
                        _IsR1TEControlTemperature = value;
                        RaisePropertyChanged("IsR1TEControlTemperature");
                    }
                }
            }
        }
        public bool IsR1TECMaximumCoolingCurrent
        {
            get { return _IsR1TECMaximumCoolingCurrent; }
            set
            {
                if (_IsR1TECMaximumCoolingCurrent != value)
                {
                    if (double.IsNaN(R1TECMaximumCoolingCurrent))
                    {
                        //_IsR1CheckALL = false;
                        //RaisePropertyChanged("IsR1CheckALL");
                        _IsR1TECMaximumCoolingCurrent = false;
                        RaisePropertyChanged("IsR1TECMaximumCoolingCurrent");
                    }
                    else
                    {
                        if (!value)
                        {
                            _IsR1CheckALL = false;
                            RaisePropertyChanged("IsR1CheckALL");
                        }
                        _IsR1TECMaximumCoolingCurrent = value;
                        RaisePropertyChanged("IsR1TECMaximumCoolingCurrent");
                    }
                }
            }
        }
        public bool IsR1OpticalPowerLessThanOrEqual15mWKp
        {
            get { return _IsR1OpticalPowerLessThanOrEqual15mWKp; }
            set
            {
                if (_IsR1OpticalPowerLessThanOrEqual15mWKp != value)
                {
                    if (double.IsNaN(R1OpticalPowerLessThanOrEqual15mWKp))
                    {
                        //_IsR1CheckALL = false;
                        //RaisePropertyChanged("IsR1CheckALL");
                        _IsR1OpticalPowerLessThanOrEqual15mWKp = false;
                        RaisePropertyChanged("IsR1OpticalPowerLessThanOrEqual15mWKp");
                    }
                    else
                    {
                        if (!value)
                        {
                            _IsR1CheckALL = false;
                            RaisePropertyChanged("IsR1CheckALL");
                        }
                        _IsR1OpticalPowerLessThanOrEqual15mWKp = value;
                        RaisePropertyChanged("IsR1OpticalPowerLessThanOrEqual15mWKp");
                    }
                }
            }
        }

        public bool IsR1OpticalPowerLessThanOrEqual15mWKi
        {
            get { return _IsR1OpticalPowerLessThanOrEqual15mWKi; }
            set
            {
                if (_IsR1OpticalPowerLessThanOrEqual15mWKi != value)
                {
                    if (double.IsNaN(R1OpticalPowerLessThanOrEqual15mWKi))
                    {
                        //_IsR1CheckALL = false;
                        //RaisePropertyChanged("IsR1CheckALL");
                        _IsR1OpticalPowerLessThanOrEqual15mWKi = false;
                        RaisePropertyChanged("IsR1OpticalPowerLessThanOrEqual15mWKi");
                    }
                    else
                    {
                        if (!value)
                        {
                            _IsR1CheckALL = false;
                            RaisePropertyChanged("IsR1CheckALL");
                        }
                        _IsR1OpticalPowerLessThanOrEqual15mWKi = value;
                        RaisePropertyChanged("IsR1OpticalPowerLessThanOrEqual15mWKi");
                    }
                }
            }
        }

        public bool IsR1OpticalPowerLessThanOrEqual15mWKd
        {
            get { return _IsR1OpticalPowerLessThanOrEqual15mWKd; }
            set
            {
                if (_IsR1OpticalPowerLessThanOrEqual15mWKd != value)
                {
                    if (double.IsNaN(R1OpticalPowerLessThanOrEqual15mWKd))
                    {
                        //_IsR1CheckALL = false;
                        //RaisePropertyChanged("IsR1CheckALL");
                        _IsR1OpticalPowerLessThanOrEqual15mWKd = false;
                        RaisePropertyChanged("IsR1OpticalPowerLessThanOrEqual15mWKd");
                    }
                    else
                    {
                        if (!value)
                        {
                            _IsR1CheckALL = false;
                            RaisePropertyChanged("IsR1CheckALL");
                        }
                        _IsR1OpticalPowerLessThanOrEqual15mWKd = value;
                        RaisePropertyChanged("IsR1OpticalPowerLessThanOrEqual15mWKd");
                    }
                }
            }
        }
        public bool IsR1OpticalPowerGreaterThan15mWKp
        {
            get { return _IsR1OpticalPowerGreaterThan15mWKp; }
            set
            {
                if (_IsR1OpticalPowerGreaterThan15mWKp != value)
                {
                    if (double.IsNaN(R1OpticalPowerGreaterThan15mWKp))
                    {
                        //_IsR1CheckALL = false;
                        //RaisePropertyChanged("IsR1CheckALL");
                        _IsR1OpticalPowerGreaterThan15mWKp = false;
                        RaisePropertyChanged("IsR1OpticalPowerGreaterThan15mWKp");
                    }
                    else
                    {
                        if (!value)
                        {
                            _IsR1CheckALL = false;
                            RaisePropertyChanged("IsR1CheckALL");
                        }
                        _IsR1OpticalPowerGreaterThan15mWKp = value;
                        RaisePropertyChanged("IsR1OpticalPowerGreaterThan15mWKp");
                    }
                }
            }
        }
        public bool IsR1OpticalPowerGreaterThan15mWKi
        {
            get { return _IsR1OpticalPowerGreaterThan15mWKi; }
            set
            {
                if (_IsR1OpticalPowerGreaterThan15mWKi != value)
                {
                    if (double.IsNaN(R1OpticalPowerGreaterThan15mWKi))
                    {
                        //_IsR1CheckALL = false;
                        //RaisePropertyChanged("IsR1CheckALL");
                        _IsR1OpticalPowerGreaterThan15mWKi = false;
                        RaisePropertyChanged("IsR1OpticalPowerGreaterThan15mWKi");
                    }
                    else
                    {
                        if (!value)
                        {
                            _IsR1CheckALL = false;
                            RaisePropertyChanged("IsR1CheckALL");
                        }
                        _IsR1OpticalPowerGreaterThan15mWKi = value;
                        RaisePropertyChanged("IsR1OpticalPowerGreaterThan15mWKi");
                    }

                }
            }
        }

        public bool IsR1OpticalPowerGreaterThan15mWKd
        {
            get { return _IsR1OpticalPowerGreaterThan15mWKd; }
            set
            {
                if (_IsR1OpticalPowerGreaterThan15mWKd != value)
                {
                    if (double.IsNaN(R1OpticalPowerGreaterThan15mWKd))
                    {
                        //_IsR1CheckALL = false;
                        //RaisePropertyChanged("IsR1CheckALL");
                        _IsR1OpticalPowerGreaterThan15mWKd = false;
                        RaisePropertyChanged("IsR1OpticalPowerGreaterThan15mWKd");
                    }
                    else
                    {
                        if (!value)
                        {
                            _IsR1CheckALL = false;
                            RaisePropertyChanged("IsR1CheckALL");
                        }
                        _IsR1OpticalPowerGreaterThan15mWKd = value;
                        RaisePropertyChanged("IsR1OpticalPowerGreaterThan15mWKd");
                    }
                }
            }
        }

        public bool IsR1LaserMaxCurrent
        {
            get { return _IsR1LaserMaxCurrent; }
            set
            {
                if (_IsR1LaserMaxCurrent != value)
                {
                    if (double.IsNaN(R1LaserMaxCurrent))
                    {
                        //_IsR1CheckALL = false;
                        //RaisePropertyChanged("IsR1CheckALL");
                        _IsR1LaserMaxCurrent = false;
                        RaisePropertyChanged("IsR1LaserMaxCurrent");
                    }
                    else
                    {
                        if (!value)
                        {
                            _IsR1CheckALL = false;
                            RaisePropertyChanged("IsR1CheckALL");
                        }
                        _IsR1LaserMaxCurrent = value;
                        RaisePropertyChanged("IsR1LaserMaxCurrent");
                    }
                }
            }
        }

        public bool IsR1LaserMinCurrent
        {
            get { return _IsR1LaserMinCurrent; }
            set
            {
                if (_IsR1LaserMinCurrent != value)
                {
                    if (double.IsNaN(R1LaserMinCurrent))
                    {
                        //_IsR1CheckALL = false;
                        //RaisePropertyChanged("IsR1CheckALL");
                        _IsR1LaserMinCurrent = false;
                        RaisePropertyChanged("IsR1LaserMinCurrent");
                    }
                    else
                    {
                        if (!value)
                        {
                            _IsR1CheckALL = false;
                            RaisePropertyChanged("IsR1CheckALL");
                        }
                        _IsR1LaserMinCurrent = value;
                        RaisePropertyChanged("IsR1LaserMinCurrent");
                    }
                }
            }
        }


        public bool IsR1TECControlKp
        {
            get { return _IsR1TECControlKp; }
            set
            {
                if (_IsR1TECControlKp != value)
                {
                    if (double.IsNaN(R1TECControlKp))
                    {
                        //_IsR1CheckALL = false;
                        //RaisePropertyChanged("IsR1CheckALL");
                        _IsR1TECControlKp = false;
                        RaisePropertyChanged("IsR1TECControlKp");
                    }
                    else
                    {
                        if (!value)
                        {
                            _IsR1CheckALL = false;
                            RaisePropertyChanged("IsR1CheckALL");
                        }
                        _IsR1TECControlKp = value;
                        RaisePropertyChanged("IsR1TECControlKp");
                    }
                }
            }
        }
        public bool IsR1TECControlKi
        {
            get { return _IsR1TECControlKi; }
            set
            {
                if (_IsR1TECControlKi != value)
                {
                    if (double.IsNaN(R1TECControlKi))
                    {
                        //_IsR1CheckALL = false;
                        //RaisePropertyChanged("IsR1CheckALL");
                        _IsR1TECControlKi = false;
                        RaisePropertyChanged("IsR1TECControlKi");
                    }
                    else
                    {
                        if (!value)
                        {
                            _IsR1CheckALL = false;
                            RaisePropertyChanged("IsR1CheckALL");
                        }
                        _IsR1TECControlKi = value;
                        RaisePropertyChanged("IsR1TECControlKi");
                    }
                }
            }
        }
        public bool IsR1TECControlKd
        {
            get { return _IsR1TECControlKd; }
            set
            {
                if (_IsR1TECControlKd != value)
                {
                    if (double.IsNaN(R1TECControlKd))
                    {
                        //_IsR1CheckALL = false;
                        //RaisePropertyChanged("IsR1CheckALL");
                        _IsR1TECControlKd = false;
                        RaisePropertyChanged("IsR1TECControlKd");
                    }
                    else
                    {
                        if (!value)
                        {
                            _IsR1CheckALL = false;
                            RaisePropertyChanged("IsR1CheckALL");
                        }
                        _IsR1TECControlKd = value;
                        RaisePropertyChanged("IsR1TECControlKd");
                    }
                }
            }
        }
        #endregion

        #endregion

        #region  R2 

        public string R2IVFirmwareVersionSN
        {
            get { return _R2IVFirmwareVersionSN; }
            set
            {
                if (_R2IVFirmwareVersionSN != value)
                {
                    if (value == StrEmptyCode || value == StrEmptyCode1)
                    {
                        _R2IVFirmwareVersionSN = DefaultStrCode;
                        RaisePropertyChanged("R2IVFirmwareVersionSN");
                    }
                    else
                    {
                        _R2IVFirmwareVersionSN = value;
                        RaisePropertyChanged("R2IVFirmwareVersionSN");
                    }
                }
            }
        }

        public string R2IVFirmwareSN
        {
            get { return _R2IVFirmwareSN; }
            set
            {
                if (_R2IVFirmwareSN != value)
                {
                    if (value == StrEmptyCode || value == StrEmptyCode1)
                    {
                        _R2IVFirmwareSN = DefaultStrCode;
                        RaisePropertyChanged("R2IVFirmwareSN");
                    }
                    else
                    {
                        _R2IVFirmwareSN = value;
                        RaisePropertyChanged("R2IVFirmwareSN");
                    }
                }
            }
        }

        public double R2TEControlTemperature
        {
            get { return _R2TEControlTemperature; }
            set
            {
                if (_R2TEControlTemperature != value)
                {
                    if ((int)(value * 10) == Uint16Code)
                    {
                        IsR2TEControlTemperature = false;
                        RaisePropertyChanged("IsR2TEControlTemperature");
                        _R2TEControlTemperature = double.NaN;
                        RaisePropertyChanged("R2TEControlTemperature");
                    }
                    else
                    {
                        _R2TEControlTemperature = value;
                        RaisePropertyChanged("R2TEControlTemperature");
                    }
                }
            }
        }
        public double R2TECMaximumCoolingCurrent
        {
            get { return _R2TECMaximumCoolingCurrent; }
            set
            {
                if (_R2TECMaximumCoolingCurrent != value)
                {
                    if ((uint)(value) == Uint16Code)
                    {
                        IsR2TECMaximumCoolingCurrent = false;
                        RaisePropertyChanged("IsR2TECMaximumCoolingCurrent");
                        _R2TECMaximumCoolingCurrent = double.NaN;
                        RaisePropertyChanged("R2TECMaximumCoolingCurrent");
                    }
                    else
                    {
                        _R2TECMaximumCoolingCurrent = value;
                        RaisePropertyChanged("R2TECMaximumCoolingCurrent");
                    }
                }
            }
        }
        public double R2OpticalPowerLessThanOrEqual15mWKp
        {
            get { return _R2OpticalPowerLessThanOrEqual15mWKp; }
            set
            {
                if (_R2OpticalPowerLessThanOrEqual15mWKp != value)
                {
                    if ((int)(value) == Uint16Code)
                    {
                        IsR2OpticalPowerLessThanOrEqual15mWKp = false;
                        RaisePropertyChanged("IsR2OpticalPowerLessThanOrEqual15mWKp");
                        _R2OpticalPowerLessThanOrEqual15mWKp = double.NaN;
                        RaisePropertyChanged("R2OpticalPowerLessThanOrEqual15mWKp");
                    }
                    else
                    {
                        _R2OpticalPowerLessThanOrEqual15mWKp = value;
                        RaisePropertyChanged("R2OpticalPowerLessThanOrEqual15mWKp");
                    }

                }
            }
        }

        public double R2OpticalPowerLessThanOrEqual15mWKi
        {
            get { return _R2OpticalPowerLessThanOrEqual15mWKi; }
            set
            {
                if (_R2OpticalPowerLessThanOrEqual15mWKi != value)
                {
                    if ((int)(value) == Uint16Code)
                    {
                        IsR2OpticalPowerLessThanOrEqual15mWKi = false;
                        RaisePropertyChanged("IsR2OpticalPowerLessThanOrEqual15mWKi");
                        _R2OpticalPowerLessThanOrEqual15mWKi = double.NaN;
                        RaisePropertyChanged("R2OpticalPowerLessThanOrEqual15mWKi");
                    }
                    else
                    {
                        _R2OpticalPowerLessThanOrEqual15mWKi = value;
                        RaisePropertyChanged("R2OpticalPowerLessThanOrEqual15mWKi");
                    }
                }
            }
        }

        public double R2OpticalPowerLessThanOrEqual15mWKd
        {
            get { return _R2OpticalPowerLessThanOrEqual15mWKd; }
            set
            {
                if (_R2OpticalPowerLessThanOrEqual15mWKd != value)
                {
                    if ((int)(value) == Uint16Code)
                    {
                        IsR2OpticalPowerLessThanOrEqual15mWKd = false;
                        RaisePropertyChanged("IsR2OpticalPowerLessThanOrEqual15mWKd");
                        _R2OpticalPowerLessThanOrEqual15mWKd = double.NaN;
                        RaisePropertyChanged("R2OpticalPowerLessThanOrEqual15mWKd");
                    }
                    else
                    {
                        _R2OpticalPowerLessThanOrEqual15mWKd = value;
                        RaisePropertyChanged("R2OpticalPowerLessThanOrEqual15mWKd");
                    }
                }
            }
        }
        public double R2OpticalPowerGreaterThan15mWKp
        {
            get { return _R2OpticalPowerGreaterThan15mWKp; }
            set
            {
                if (_R2OpticalPowerGreaterThan15mWKp != value)
                {
                    if ((int)(value) == Uint16Code)
                    {
                        IsR2OpticalPowerGreaterThan15mWKp = false;
                        RaisePropertyChanged("IsR2OpticalPowerGreaterThan15mWKp");
                        _R2OpticalPowerGreaterThan15mWKp = double.NaN;
                        RaisePropertyChanged("R2OpticalPowerGreaterThan15mWKp");
                    }
                    else
                    {
                        _R2OpticalPowerGreaterThan15mWKp = value;
                        RaisePropertyChanged("R2OpticalPowerGreaterThan15mWKp");
                    }
                }
            }
        }
        public double R2OpticalPowerGreaterThan15mWKi
        {
            get { return _R2OpticalPowerGreaterThan15mWKi; }
            set
            {
                if (_R2OpticalPowerGreaterThan15mWKi != value)
                {
                    if ((int)(value) == Uint16Code)
                    {
                        IsR2OpticalPowerGreaterThan15mWKi = false;
                        RaisePropertyChanged("IsR2OpticalPowerGreaterThan15mWKi");
                        _R2OpticalPowerGreaterThan15mWKi = double.NaN;
                        RaisePropertyChanged("R2OpticalPowerGreaterThan15mWKi");
                    }
                    else
                    {
                        _R2OpticalPowerGreaterThan15mWKi = value;
                        RaisePropertyChanged("R2OpticalPowerGreaterThan15mWKi");
                    }
                }
            }
        }

        public double R2OpticalPowerGreaterThan15mWKd
        {
            get { return _R2OpticalPowerGreaterThan15mWKd; }
            set
            {
                if (_R2OpticalPowerGreaterThan15mWKd != value)
                {
                    if ((int)(value) == Uint16Code)
                    {
                        IsR2OpticalPowerGreaterThan15mWKd = false;
                        RaisePropertyChanged("IsR2OpticalPowerGreaterThan15mWKd");
                        _R2OpticalPowerGreaterThan15mWKd = double.NaN;
                        RaisePropertyChanged("R2OpticalPowerGreaterThan15mWKd");
                    }
                    else
                    {
                        _R2OpticalPowerGreaterThan15mWKd = value;
                        RaisePropertyChanged("R2OpticalPowerGreaterThan15mWKd");
                    }
                }
            }
        }

        public double R2OpticalPowerControlKpUpperLimitLessThanOrEqual15
        {
            get { return _R2OpticalPowerControlKpUpperLimitLessThanOrEqual15; }
            set
            {
                if (_R2OpticalPowerControlKpUpperLimitLessThanOrEqual15 != value)
                {
                    if ((int)(value) == Uint16Code)
                    {
                        _R2OpticalPowerControlKpUpperLimitLessThanOrEqual15 = double.NaN;
                        RaisePropertyChanged("R2OpticalPowerControlKpUpperLimitLessThanOrEqual15");
                    }
                    else
                    {
                        _R2OpticalPowerControlKpUpperLimitLessThanOrEqual15 = value;
                        RaisePropertyChanged("R2OpticalPowerControlKpUpperLimitLessThanOrEqual15");
                    }
                }
            }
        }

        public double R2OpticalPowerControlKpDownLimitLessThanOrEqual15
        {
            get { return _R2OpticalPowerControlKpDownLimitLessThanOrEqual15; }
            set
            {
                if (_R2OpticalPowerControlKpDownLimitLessThanOrEqual15 != value)
                {
                    if ((int)(value) == Uint16Code)
                    {
                        _R2OpticalPowerControlKpDownLimitLessThanOrEqual15 = double.NaN;
                        RaisePropertyChanged("R2OpticalPowerControlKpDownLimitLessThanOrEqual15");
                    }
                    else
                    {
                        _R2OpticalPowerControlKpDownLimitLessThanOrEqual15 = value;
                        RaisePropertyChanged("R2OpticalPowerControlKpDownLimitLessThanOrEqual15");
                    }

                }
            }
        }

        public double R2OpticalPowerControlKiUpperLimitLessThanOrEqual15
        {
            get { return _R2OpticalPowerControlKiUpperLimitLessThanOrEqual15; }
            set
            {
                if (_R2OpticalPowerControlKiUpperLimitLessThanOrEqual15 != value)
                {
                    if ((int)(value) == Uint16Code)
                    {
                        _R2OpticalPowerControlKiUpperLimitLessThanOrEqual15 = double.NaN;
                        RaisePropertyChanged("R2OpticalPowerControlKiUpperLimitLessThanOrEqual15");
                    }
                    else
                    {
                        _R2OpticalPowerControlKiUpperLimitLessThanOrEqual15 = value;
                        RaisePropertyChanged("R2OpticalPowerControlKiUpperLimitLessThanOrEqual15");
                    }
                }
            }
        }

        public double R2OpticalPowerControlKiDownLimitLessThanOrEqual15
        {
            get { return _R2OpticalPowerControlKiDownLimitLessThanOrEqual15; }
            set
            {
                if (_R2OpticalPowerControlKiDownLimitLessThanOrEqual15 != value)
                {
                    if ((int)(value) == Uint16Code)
                    {
                        _R2OpticalPowerControlKiDownLimitLessThanOrEqual15 = double.NaN;
                        RaisePropertyChanged("R2OpticalPowerControlKiDownLimitLessThanOrEqual15");
                    }
                    else
                    {
                        _R2OpticalPowerControlKiDownLimitLessThanOrEqual15 = value;
                        RaisePropertyChanged("R2OpticalPowerControlKiDownLimitLessThanOrEqual15");
                    }
                }
            }
        }

        public double R2OpticalPowerControlKpUpperLimitLessThan15
        {
            get { return _R2OpticalPowerControlKpUpperLimitLessThan15; }
            set
            {
                if (_R2OpticalPowerControlKpUpperLimitLessThan15 != value)
                {
                    if ((int)(value) == Uint16Code)
                    {
                        _R2OpticalPowerControlKpUpperLimitLessThan15 = double.NaN;
                        RaisePropertyChanged("R2OpticalPowerControlKpUpperLimitLessThan15");
                    }
                    else
                    {
                        _R2OpticalPowerControlKpUpperLimitLessThan15 = value;
                        RaisePropertyChanged("R2OpticalPowerControlKpUpperLimitLessThan15");
                    }
                }
            }
        }

        public double R2OpticalPowerControlKpDownLimitLessThan15
        {
            get { return _R2OpticalPowerControlKpDownLimitLessThan15; }
            set
            {
                if (_R2OpticalPowerControlKpDownLimitLessThan15 != value)
                {
                    if ((int)(value) == Uint16Code)
                    {
                        _R2OpticalPowerControlKpDownLimitLessThan15 = double.NaN;
                        RaisePropertyChanged("R2OpticalPowerControlKpDownLimitLessThan15");
                    }
                    else
                    {
                        _R2OpticalPowerControlKpDownLimitLessThan15 = value;
                        RaisePropertyChanged("R2OpticalPowerControlKpDownLimitLessThan15");
                    }
                }
            }
        }

        public double R2OpticalPowerControlKiUpperLimitLessThan15
        {
            get { return _R2OpticalPowerControlKiUpperLimitLessThan15; }
            set
            {
                if (_R2OpticalPowerControlKiUpperLimitLessThan15 != value)
                {
                    if ((int)(value) == Uint16Code)
                    {
                        _R2OpticalPowerControlKiUpperLimitLessThan15 = double.NaN;
                        RaisePropertyChanged("R2OpticalPowerControlKiUpperLimitLessThan15");
                    }
                    else
                    {
                        _R2OpticalPowerControlKiUpperLimitLessThan15 = value;
                        RaisePropertyChanged("R2OpticalPowerControlKiUpperLimitLessThan15");
                    }
                }
            }
        }

        public double R2OpticalPowerControlKiDownLimitLessThan15
        {
            get { return _R2OpticalPowerControlKiDownLimitLessThan15; }
            set
            {
                if (_R2OpticalPowerControlKiDownLimitLessThan15 != value)
                {
                    if ((int)(value) == Uint16Code)
                    {
                        _R2OpticalPowerControlKiDownLimitLessThan15 = double.NaN;
                        RaisePropertyChanged("R2OpticalPowerControlKiDownLimitLessThan15");
                    }
                    else
                    {
                        _R2OpticalPowerControlKiDownLimitLessThan15 = value;
                        RaisePropertyChanged("R2OpticalPowerControlKiDownLimitLessThan15");
                    }
                }
            }
        }

        public double R2LaserMaxCurrent
        {
            get { return _R2LaserMaxCurrent; }
            set
            {
                if (_R2LaserMaxCurrent != value)
                {
                    if ((int)(value * 10) == Uint16Code)
                    {
                        IsR2LaserMaxCurrent = false;
                        RaisePropertyChanged("IsR2LaserMaxCurrent");
                        _R2LaserMaxCurrent = double.NaN;
                        RaisePropertyChanged("R2LaserMaxCurrent");
                    }
                    else
                    {
                        _R2LaserMaxCurrent = value;
                        RaisePropertyChanged("R2LaserMaxCurrent");
                    }
                }
            }
        }

        public double R2LaserMinCurrent
        {
            get { return _R2LaserMinCurrent; }
            set
            {
                if (_R2LaserMinCurrent != value)
                {
                    if ((int)(value * 10) == Uint16Code)
                    {
                        IsR2LaserMinCurrent = false;
                        RaisePropertyChanged("IsR2LaserMinCurrent");
                        _R2LaserMinCurrent = double.NaN;
                        RaisePropertyChanged("R2LaserMinCurrent");
                    }
                    else
                    {
                        _R2LaserMinCurrent = value;
                        RaisePropertyChanged("R2LaserMinCurrent");
                    }
                }
            }
        }

        public double R2TECControlKp
        {
            get { return _R2TECControlKp; }
            set
            {
                if (_R2TECControlKp != value)
                {
                    if ((int)(value) == Uint16Code)
                    {
                        IsR2TECControlKp = false;
                        RaisePropertyChanged("IsR2TECControlKp");
                        _R2TECControlKp = double.NaN;
                        RaisePropertyChanged("R2TECControlKp");
                    }
                    else
                    {
                        _R2TECControlKp = value;
                        RaisePropertyChanged("R2TECControlKp");
                    }
                }
            }
        }
        public double R2TECControlKi
        {
            get { return _R2TECControlKi; }
            set
            {
                if (_R2TECControlKi != value)
                {
                    if ((int)(value) == Uint16Code)
                    {
                        IsR2TECControlKi = false;
                        RaisePropertyChanged("IsR2TECControlKi");
                        _R2TECControlKi = double.NaN;
                        RaisePropertyChanged("R2TECControlKi");
                    }
                    else
                    {
                        _R2TECControlKi = value;
                        RaisePropertyChanged("R2TECControlKi");
                    }
                }
            }
        }
        public double R2TECControlKd
        {
            get { return _R2TECControlKd; }
            set
            {
                if (_R2TECControlKd != value)
                {
                    if ((int)(value) == Uint16Code)
                    {
                        IsR2TECControlKd = false;
                        RaisePropertyChanged("IsR2TECControlKd");
                        _R2TECControlKd = double.NaN;
                        RaisePropertyChanged("R2TECControlKd");
                    }
                    else
                    {
                        _R2TECControlKd = value;
                        RaisePropertyChanged("R2TECControlKd");
                    }
                }
            }
        }

        public bool IsEnabledR2
        {
            get { return _IsEnabledR2; }
            set
            {
                _IsEnabledR2 = value;
                RaisePropertyChanged("IsEnabledR2");
            }
        }
        public Visibility Is532EnabledR2
        {
            get { return _Is532EnabledR2; }
            set
            {
                _Is532EnabledR2 = value;
                RaisePropertyChanged("Is532EnabledR2");
            }
        }

        #region CheckBox
        public bool IsR2CheckALL
        {
            get { return _IsR2CheckALL; }
            set
            {
                _IsR2CheckALL = value;
                if (Workspace.This.LaserModuleR2.LaserWavelength != 0 && Workspace.This.LaserModuleR2.LaserWavelength != Workspace.This.NewParameterVM.Uint16Code)
                {
                    IsR2TEControlTemperature = value;
                    IsR2TECMaximumCoolingCurrent = value;
                    IsR2TECControlKp = value;
                    IsR2TECControlKi = value;
                    IsR2TECControlKd = value;
                    if (Workspace.This.LaserModuleR2.LaserWavelength == 532)
                    {
                        IsR2LaserMaxCurrent = value;
                        IsR2LaserMinCurrent = value;
                        IsR2OpticalPowerLessThanOrEqual15mWKp = value;
                        IsR2OpticalPowerLessThanOrEqual15mWKi = value;
                        IsR2OpticalPowerLessThanOrEqual15mWKd = value;
                        IsR2OpticalPowerGreaterThan15mWKp = value;
                        IsR2OpticalPowerGreaterThan15mWKi = value;
                        IsR2OpticalPowerGreaterThan15mWKd = value;
                    }
                }
                RaisePropertyChanged("IsR2CheckALL");
            }
        }
        public bool IsR2TEControlTemperature
        {
            get { return _IsR2TEControlTemperature; }
            set
            {
                if (_IsR2TEControlTemperature != value)
                {
                    if (double.IsNaN(R2TEControlTemperature))
                    {
                        //_IsR2CheckALL = false;
                        //RaisePropertyChanged("IsR2CheckALL");
                        _IsR2TEControlTemperature = false;
                        RaisePropertyChanged("IsR2TEControlTemperature");
                    }
                    else
                    {
                        _IsR2TEControlTemperature = value;
                        if (!value)
                        {
                            _IsR2CheckALL = false;
                            RaisePropertyChanged("IsR2CheckALL");
                        }
                        RaisePropertyChanged("IsR2TEControlTemperature");
                    }
                }
            }
        }
        public bool IsR2TECMaximumCoolingCurrent
        {
            get { return _IsR2TECMaximumCoolingCurrent; }
            set
            {
                if (_IsR2TECMaximumCoolingCurrent != value)
                {
                    if (double.IsNaN(R2TECMaximumCoolingCurrent))
                    {
                        //_IsR2CheckALL = false;
                        //RaisePropertyChanged("IsR2CheckALL");
                        _IsR2TECMaximumCoolingCurrent = false;
                        RaisePropertyChanged("IsR2TECMaximumCoolingCurrent");
                    }
                    else
                    {
                        _IsR2TECMaximumCoolingCurrent = value;
                        if (!value)
                        {
                            _IsR2CheckALL = false;
                            RaisePropertyChanged("IsR2CheckALL");
                        }
                        RaisePropertyChanged("IsR2TECMaximumCoolingCurrent");
                    }
                }
            }
        }
        public bool IsR2OpticalPowerLessThanOrEqual15mWKp
        {
            get { return _IsR2OpticalPowerLessThanOrEqual15mWKp; }
            set
            {
                if (_IsR2OpticalPowerLessThanOrEqual15mWKp != value)
                {
                    if (double.IsNaN(R2OpticalPowerLessThanOrEqual15mWKp))
                    {
                        //_IsR2CheckALL = false;
                        //RaisePropertyChanged("IsR2CheckALL");
                        _IsR2OpticalPowerLessThanOrEqual15mWKp = false;
                        RaisePropertyChanged("IsR2OpticalPowerLessThanOrEqual15mWKp");
                    }
                    else
                    {
                        _IsR2OpticalPowerLessThanOrEqual15mWKp = value;
                        if (!value)
                        {
                            _IsR2CheckALL = false;
                            RaisePropertyChanged("IsR2CheckALL");
                        }
                        RaisePropertyChanged("IsR2OpticalPowerLessThanOrEqual15mWKp");
                    }
                }
            }
        }

        public bool IsR2OpticalPowerLessThanOrEqual15mWKi
        {
            get { return _IsR2OpticalPowerLessThanOrEqual15mWKi; }
            set
            {
                if (_IsR2OpticalPowerLessThanOrEqual15mWKi != value)
                {
                    if (double.IsNaN(R2OpticalPowerLessThanOrEqual15mWKi))
                    {
                        //_IsR2CheckALL = false;
                        //RaisePropertyChanged("IsR2CheckALL");
                        _IsR2OpticalPowerLessThanOrEqual15mWKi = false;
                        RaisePropertyChanged("IsR2OpticalPowerLessThanOrEqual15mWKi");
                    }
                    else
                    {
                        _IsR2OpticalPowerLessThanOrEqual15mWKi = value;
                        if (!value)
                        {
                            _IsR2CheckALL = false;
                            RaisePropertyChanged("IsR2CheckALL");
                        }
                        RaisePropertyChanged("IsR2OpticalPowerLessThanOrEqual15mWKi");
                    }

                }
            }
        }

        public bool IsR2OpticalPowerLessThanOrEqual15mWKd
        {
            get { return _IsR2OpticalPowerLessThanOrEqual15mWKd; }
            set
            {
                if (_IsR2OpticalPowerLessThanOrEqual15mWKd != value)
                {
                    if (double.IsNaN(R2OpticalPowerLessThanOrEqual15mWKd))
                    {
                        //_IsR2CheckALL = false;
                        //RaisePropertyChanged("IsR2CheckALL");
                        _IsR2OpticalPowerLessThanOrEqual15mWKd = false;
                        RaisePropertyChanged("IsR2OpticalPowerLessThanOrEqual15mWKd");
                    }
                    else
                    {
                        if (!value)
                        {
                            _IsR2CheckALL = false;
                            RaisePropertyChanged("IsR2CheckALL");
                        }
                        _IsR2OpticalPowerLessThanOrEqual15mWKd = value;
                        RaisePropertyChanged("IsR2OpticalPowerLessThanOrEqual15mWKd");
                    }
                }
            }
        }
        public bool IsR2OpticalPowerGreaterThan15mWKp
        {
            get { return _IsR2OpticalPowerGreaterThan15mWKp; }
            set
            {
                if (_IsR2OpticalPowerGreaterThan15mWKp != value)
                {
                    if (double.IsNaN(R2OpticalPowerGreaterThan15mWKp))
                    {
                        //_IsR2CheckALL = false;
                        //RaisePropertyChanged("IsR2CheckALL");
                        _IsR2OpticalPowerGreaterThan15mWKp = false;
                        RaisePropertyChanged("IsR2OpticalPowerGreaterThan15mWKp");
                    }
                    else
                    {
                        if (!value)
                        {
                            _IsR2CheckALL = false;
                            RaisePropertyChanged("IsR2CheckALL");
                        }
                        _IsR2OpticalPowerGreaterThan15mWKp = value;
                        RaisePropertyChanged("IsR2OpticalPowerGreaterThan15mWKp");
                    }
                }
            }
        }
        public bool IsR2OpticalPowerGreaterThan15mWKi
        {
            get { return _IsR2OpticalPowerGreaterThan15mWKi; }
            set
            {
                if (_IsR2OpticalPowerGreaterThan15mWKi != value)
                {
                    if (double.IsNaN(R2OpticalPowerGreaterThan15mWKi))
                    {
                        //_IsR2CheckALL = false;
                        //RaisePropertyChanged("IsR2CheckALL");
                        _IsR2OpticalPowerGreaterThan15mWKi = false;
                        RaisePropertyChanged("IsR2OpticalPowerGreaterThan15mWKi");
                    }
                    else
                    {
                        if (!value)
                        {
                            _IsR2CheckALL = false;
                            RaisePropertyChanged("IsR2CheckALL");
                        }
                        _IsR2OpticalPowerGreaterThan15mWKi = value;
                        RaisePropertyChanged("IsR2OpticalPowerGreaterThan15mWKi");
                    }
                }
            }
        }

        public bool IsR2OpticalPowerGreaterThan15mWKd
        {
            get { return _IsR2OpticalPowerGreaterThan15mWKd; }
            set
            {
                if (_IsR2OpticalPowerGreaterThan15mWKd != value)
                {
                    if (double.IsNaN(R2OpticalPowerGreaterThan15mWKd))
                    {
                        //_IsR2CheckALL = false;
                        //RaisePropertyChanged("IsR2CheckALL");
                        _IsR2OpticalPowerGreaterThan15mWKd = false;
                        RaisePropertyChanged("IsR2OpticalPowerGreaterThan15mWKd");
                    }
                    else
                    {
                        if (!value)
                        {
                            _IsR2CheckALL = false;
                            RaisePropertyChanged("IsR2CheckALL");
                        }
                        _IsR2OpticalPowerGreaterThan15mWKd = value;
                        RaisePropertyChanged("IsR2OpticalPowerGreaterThan15mWKd");
                    }
                }
            }
        }

        public bool IsR2LaserMaxCurrent
        {
            get { return _IsR2LaserMaxCurrent; }
            set
            {
                if (_IsR2LaserMaxCurrent != value)
                {
                    if (double.IsNaN(R2LaserMaxCurrent))
                    {
                        //_IsR2CheckALL = false;
                        //RaisePropertyChanged("IsR2CheckALL");
                        _IsR2LaserMaxCurrent = false;
                        RaisePropertyChanged("IsR2LaserMaxCurrent");
                    }
                    else
                    {
                        if (!value)
                        {
                            _IsR2CheckALL = false;
                            RaisePropertyChanged("IsR2CheckALL");
                        }
                        _IsR2LaserMaxCurrent = value;
                        RaisePropertyChanged("IsR2LaserMaxCurrent");

                    }

                }
            }
        }

        public bool IsR2LaserMinCurrent
        {
            get { return _IsR2LaserMinCurrent; }
            set
            {
                if (_IsR2LaserMinCurrent != value)
                {
                    if (double.IsNaN(R2LaserMinCurrent))
                    {
                        //_IsR2CheckALL = false;
                        //RaisePropertyChanged("IsR2CheckALL");
                        _IsR2LaserMinCurrent = false;
                        RaisePropertyChanged("IsR2LaserMinCurrent");
                    }
                    else
                    {
                        if (!value)
                        {
                            _IsR2CheckALL = false;
                            RaisePropertyChanged("IsR2CheckALL");
                        }
                        _IsR2LaserMinCurrent = value;
                        RaisePropertyChanged("IsR2LaserMinCurrent");
                    }
                }
            }
        }


        public bool IsR2TECControlKp
        {
            get { return _IsR2TECControlKp; }
            set
            {
                if (_IsR2TECControlKp != value)
                {
                    if (double.IsNaN(R2TECControlKp))
                    {
                        //_IsR2CheckALL = false;
                        //RaisePropertyChanged("IsR2CheckALL");
                        _IsR2TECControlKp = false;
                        RaisePropertyChanged("IsR2TECControlKp");
                    }
                    else
                    {
                        if (!value)
                        {
                            _IsR2CheckALL = false;
                            RaisePropertyChanged("IsR2CheckALL");
                        }
                        _IsR2TECControlKp = value;
                        RaisePropertyChanged("IsR2TECControlKp");
                    }
                }
            }
        }
        public bool IsR2TECControlKi
        {
            get { return _IsR2TECControlKi; }
            set
            {
                if (_IsR2TECControlKi != value)
                {
                    if (double.IsNaN(R2TECControlKi))
                    {
                        //_IsR2CheckALL = false;
                        //RaisePropertyChanged("IsR2CheckALL");
                        _IsR2TECControlKi = false;
                        RaisePropertyChanged("IsR2TECControlKi");
                    }
                    else
                    {
                        if (!value)
                        {
                            _IsR2CheckALL = false;
                            RaisePropertyChanged("IsR2CheckALL");
                        }
                        _IsR2TECControlKi = value;
                        RaisePropertyChanged("IsR2TECControlKi");
                    }
                }
            }
        }
        public bool IsR2TECControlKd
        {
            get { return _IsR2TECControlKd; }
            set
            {
                if (_IsR2TECControlKd != value)
                {
                    if (double.IsNaN(R2TECControlKd))
                    {
                        //_IsR2CheckALL = false;
                        //RaisePropertyChanged("IsR2CheckALL");
                        _IsR2TECControlKd = false;
                        RaisePropertyChanged("IsR2TECControlKd");
                    }
                    else
                    {
                        if (!value)
                        {
                            _IsR2CheckALL = false;
                            RaisePropertyChanged("IsR2CheckALL");
                        }
                        _IsR2TECControlKd = value;
                        RaisePropertyChanged("IsR2TECControlKd");
                    }
                }
            }
        }
        #endregion
        #endregion
        #endregion 

        #region ParametersWriteCommand

        public ICommand ParametersWriteCommand
        {
            get
            {
                if (_ParametersWriteCommand == null)
                {
                    _ParametersWriteCommand = new RelayCommand(ExecuteParametersWriteCommand, CanExecuteParametersWriteCommand);
                }

                return _ParametersWriteCommand;
            }
        }
        public void ExecuteParametersWriteCommand(object parameter)
        {
            bool IsSelectAnyCheckBox = false;
            /*if (Workspace.This.LaserModuleL1.LaserWavelength != 0 && Workspace.This.LaserModuleL1.LaserWavelength != Workspace.This.NewParameterVM.Uint16Code)
            {
                if (IsLTEControlTemperature)
                {
                    IsSelectAnyCheckBox = true;
                    if (Workspace.This.EthernetController.SetTECControlTemperature(LaserChannels.ChannelC, LTEControlTemperature) == false)
                    {
                        MessageBox.Show("LTEControlTemperature Write Failed.");
                    }
                }
                if (IsLTECMaximumCoolingCurrent)
                {
                    IsSelectAnyCheckBox = true;
                    if (Workspace.This.EthernetController.SetTECMaximumCurrent(LaserChannels.ChannelC, LTECMaximumCoolingCurrent) == false)
                    {
                        MessageBox.Show("LTECMaximumCoolingCurrent Write Failed.");
                    }
                }
                if (IsLTECControlKp)
                {
                    IsSelectAnyCheckBox = true;
                    if (Workspace.This.EthernetController.SetTECControlKp(LaserChannels.ChannelC, LTECControlKp) == false)
                    {
                        MessageBox.Show("LTECControlKp Write Failed.");
                    }

                }
                if (IsLTECControlKi)
                {
                    IsSelectAnyCheckBox = true;
                    if (Workspace.This.EthernetController.SetTECControlKi(LaserChannels.ChannelC, LTECControlKi) == false)
                    {
                        MessageBox.Show("LTECControlKi Write Failed.");
                    }
                }
                if (IsLTECControlKd)
                {
                    IsSelectAnyCheckBox = true;
                    if (Workspace.This.EthernetController.SetTECControlKd(LaserChannels.ChannelC, LTECControlKd) == false)
                    {
                        MessageBox.Show("LTECControlKd Write Failed.");
                    }
                }
                if (Workspace.This.LaserModuleL1.LaserWavelength == 532)
                {
                    if (IsLLaserMaxCurrent)
                    {
                        IsSelectAnyCheckBox = true;
                        if (Workspace.This.EthernetController.SetLaserMaximumCurrent(LaserChannels.ChannelC, LLaserMaxCurrent) == false)
                        {
                            MessageBox.Show("LLaserMaxCurrent Write Failed.");
                        }
                    }
                    if (IsLLaserMinCurrent)
                    {
                        IsSelectAnyCheckBox = true;
                        if (Workspace.This.EthernetController.SetLaserMinimumCurrent(LaserChannels.ChannelC, LLaserMinCurrent) == false)
                        {
                            MessageBox.Show("LLaserMinCurrent Write Failed.");
                        }
                    }
                    if (IsLOpticalPowerLessThanOrEqual15mWKp)
                    {
                        IsSelectAnyCheckBox = true;
                        if (Workspace.This.EthernetController.SetOpticalPowerLessThanOrEqual15mWKp(LaserChannels.ChannelC, LOpticalPowerLessThanOrEqual15mWKp) == false)
                        {
                            MessageBox.Show("LOpticalPowerLessThanOrEqual15mWKp Write Failed.");
                        }

                    }
                    if (IsLOpticalPowerLessThanOrEqual15mWKi)
                    {
                        IsSelectAnyCheckBox = true;
                        if (Workspace.This.EthernetController.SetOpticalPowerLessThanOrEqual15mWKi(LaserChannels.ChannelC, LOpticalPowerLessThanOrEqual15mWKi) == false)
                        {
                            MessageBox.Show("LOpticalPowerLessThanOrEqual15mWKi Write Failed.");
                        }
                    }
                    if (IsLOpticalPowerLessThanOrEqual15mWKd)
                    {
                        IsSelectAnyCheckBox = true;
                        if (Workspace.This.EthernetController.SetOpticalPowerLessThanOrEqual15mWKd(LaserChannels.ChannelC, LOpticalPowerLessThanOrEqual15mWKd) == false)
                        {
                            MessageBox.Show("LOpticalPowerLessThanOrEqual15mWKd Write Failed.");
                        }
                    }
                    if (IsLOpticalPowerGreaterThan15mWKp)
                    {
                        IsSelectAnyCheckBox = true;
                        if (Workspace.This.EthernetController.SetOpticalPowerGreaterThan15mWKp(LaserChannels.ChannelC, LOpticalPowerGreaterThan15mWKp) == false)
                        {
                            MessageBox.Show("LOpticalPowerGreaterThan15mWKp Write Failed.");
                        }
                    }
                    if (IsLOpticalPowerGreaterThan15mWKi)
                    {
                        IsSelectAnyCheckBox = true;
                        if (Workspace.This.EthernetController.SetOpticalPowerGreaterThan15mWKi(LaserChannels.ChannelC, LOpticalPowerGreaterThan15mWKi) == false)
                        {
                            MessageBox.Show("LOpticalPowerGreaterThan15mWKi Write Failed.");
                        }

                    }
                    if (IsLOpticalPowerGreaterThan15mWKd)
                    {
                        IsSelectAnyCheckBox = true;
                        if (Workspace.This.EthernetController.SetOpticalPowerGreaterThan15mWKd(LaserChannels.ChannelC, LOpticalPowerGreaterThan15mWKd) == false)
                        {
                            MessageBox.Show("LOpticalPowerGreaterThan15mWKd Write Failed.");
                        }
                    }
                }
            }*/

            /*if (Workspace.This.LaserModuleR1.LaserWavelength != 0 && Workspace.This.LaserModuleR1.LaserWavelength != Workspace.This.NewParameterVM.Uint16Code)
            {
                if (IsR1TEControlTemperature)
                {
                    IsSelectAnyCheckBox = true;
                    if (Workspace.This.EthernetController.SetTECControlTemperature(LaserChannels.ChannelA, R1TEControlTemperature) == false)
                    {
                        MessageBox.Show("R1TEControlTemperature Write Failed.");
                    }
                }
                if (IsR1TECMaximumCoolingCurrent)
                {
                    IsSelectAnyCheckBox = true;
                    if (Workspace.This.EthernetController.SetTECMaximumCurrent(LaserChannels.ChannelA, R1TECMaximumCoolingCurrent) == false)
                    {
                        MessageBox.Show("R1TECMaximumCoolingCurrent Write Failed.");
                    }
                }
                if (IsR1TECControlKp)
                {
                    IsSelectAnyCheckBox = true;
                    if (Workspace.This.EthernetController.SetTECControlKp(LaserChannels.ChannelA, R1TECControlKp) == false)
                    {
                        MessageBox.Show("R1TECControlKp Write Failed.");
                    }
                }
                if (IsR1TECControlKi)
                {
                    IsSelectAnyCheckBox = true;
                    if (Workspace.This.EthernetController.SetTECControlKi(LaserChannels.ChannelA, R1TECControlKi) == false)
                    {
                        MessageBox.Show("R1TECControlKi Write Failed.");
                    }
                }
                if (IsR1TECControlKd)
                {
                    IsSelectAnyCheckBox = true;
                    if (Workspace.This.EthernetController.SetTECControlKd(LaserChannels.ChannelA, R1TECControlKd) == false)
                    {
                        MessageBox.Show("R1TECControlKd Write Failed.");
                    }
                }
                if (Workspace.This.LaserModuleR1.LaserWavelength == 532)
                {
                    if (IsR1LaserMaxCurrent)
                    {
                        IsSelectAnyCheckBox = true;
                        if (Workspace.This.EthernetController.SetLaserMaximumCurrent(LaserChannels.ChannelA, R1LaserMaxCurrent) == false)
                        {
                            MessageBox.Show("R1LaserMaxCurrent Write Failed.");
                        }
                    }
                    if (IsR1LaserMinCurrent)
                    {
                        IsSelectAnyCheckBox = true;
                        if (Workspace.This.EthernetController.SetLaserMinimumCurrent(LaserChannels.ChannelA, R1LaserMinCurrent) == false)
                        {
                            MessageBox.Show("R1LaserMinCurrent Write Failed.");
                        }
                    }
                    if (IsR1OpticalPowerLessThanOrEqual15mWKp)
                    {
                        IsSelectAnyCheckBox = true;
                        if (Workspace.This.EthernetController.SetOpticalPowerLessThanOrEqual15mWKp(LaserChannels.ChannelA, R1OpticalPowerLessThanOrEqual15mWKp) == false)
                        {
                            MessageBox.Show("R1OpticalPowerLessThanOrEqual15mWKp Write Failed.");
                        }
                    }
                    if (IsR1OpticalPowerLessThanOrEqual15mWKi)
                    {
                        IsSelectAnyCheckBox = true;
                        if (Workspace.This.EthernetController.SetOpticalPowerLessThanOrEqual15mWKi(LaserChannels.ChannelA, R1OpticalPowerLessThanOrEqual15mWKi) == false)
                        {
                            MessageBox.Show("R1OpticalPowerLessThanOrEqual15mWKi Write Failed.");
                        }
                    }
                    if (IsR1OpticalPowerLessThanOrEqual15mWKd)
                    {
                        IsSelectAnyCheckBox = true;
                        if (Workspace.This.EthernetController.SetOpticalPowerLessThanOrEqual15mWKd(LaserChannels.ChannelA, R1OpticalPowerLessThanOrEqual15mWKd) == false)
                        {
                            MessageBox.Show("R1OpticalPowerLessThanOrEqual15mWKd Write Failed.");
                        }
                    }
                    if (IsR1OpticalPowerGreaterThan15mWKp)
                    {
                        IsSelectAnyCheckBox = true;
                        if (Workspace.This.EthernetController.SetOpticalPowerGreaterThan15mWKp(LaserChannels.ChannelA, R1OpticalPowerGreaterThan15mWKp) == false)
                        {
                            MessageBox.Show("R1OpticalPowerGreaterThan15mWKp Write Failed.");
                        }
                    }
                    if (IsR1OpticalPowerGreaterThan15mWKi)
                    {
                        IsSelectAnyCheckBox = true;
                        if (Workspace.This.EthernetController.SetOpticalPowerGreaterThan15mWKi(LaserChannels.ChannelA, R1OpticalPowerGreaterThan15mWKi) == false)
                        {
                            MessageBox.Show("R1OpticalPowerGreaterThan15mWKi Write Failed.");
                        }
                    }
                    if (IsR1OpticalPowerGreaterThan15mWKd)
                    {
                        IsSelectAnyCheckBox = true;
                        if (Workspace.This.EthernetController.SetOpticalPowerGreaterThan15mWKd(LaserChannels.ChannelA, R1OpticalPowerGreaterThan15mWKd) == false)
                        {
                            MessageBox.Show("R1OpticalPowerGreaterThan15mWKd Write Failed.");
                        }
                    }
                }
            }*/

            /*if (Workspace.This.LaserModuleR2.LaserWavelength != 0 && Workspace.This.LaserModuleR2.LaserWavelength != Workspace.This.NewParameterVM.Uint16Code)
            {
                if (IsR2TEControlTemperature)
                {
                    IsSelectAnyCheckBox = true;
                    if (Workspace.This.EthernetController.SetTECControlTemperature(LaserChannels.ChannelB, R2TEControlTemperature) == false)
                    {
                        MessageBox.Show("R2TEControlTemperature Write Failed.");
                    }
                }
                if (IsR2TECMaximumCoolingCurrent)
                {
                    IsSelectAnyCheckBox = true;
                    if (Workspace.This.EthernetController.SetTECMaximumCurrent(LaserChannels.ChannelB, R2TECMaximumCoolingCurrent) == false)
                    {
                        MessageBox.Show("R2TECMaximumCoolingCurrent Write Failed.");
                    }
                }
                if (IsR2TECControlKp)
                {
                    IsSelectAnyCheckBox = true;
                    if (Workspace.This.EthernetController.SetTECControlKp(LaserChannels.ChannelB, R2TECControlKp) == false)
                    {
                        MessageBox.Show("R2TECControlKp Write Failed.");
                    }
                }
                if (IsR2TECControlKi)
                {
                    IsSelectAnyCheckBox = true;
                    if (Workspace.This.EthernetController.SetTECControlKi(LaserChannels.ChannelB, R2TECControlKi) == false)
                    {
                        MessageBox.Show("R2TECControlKi Write Failed.");
                    }
                }
                if (IsR2TECControlKd)
                {
                    IsSelectAnyCheckBox = true;
                    if (Workspace.This.EthernetController.SetTECControlKd(LaserChannels.ChannelB, R2TECControlKd) == false)
                    {
                        MessageBox.Show("R2TECControlKd Write Failed.");
                    }
                }
                if (Workspace.This.LaserModuleR2.LaserWavelength == 532)
                {
                    if (IsR2LaserMaxCurrent)
                    {
                        IsSelectAnyCheckBox = true;
                        if (Workspace.This.EthernetController.SetLaserMaximumCurrent(LaserChannels.ChannelB, R2LaserMaxCurrent) == false)
                        {
                            MessageBox.Show("R2LaserMaxCurrent Write Failed.");
                        }
                    }
                    if (IsR2LaserMinCurrent)
                    {
                        IsSelectAnyCheckBox = true;
                        if (Workspace.This.EthernetController.SetLaserMinimumCurrent(LaserChannels.ChannelB, R2LaserMinCurrent) == false)
                        {
                            MessageBox.Show("R2LaserMinCurrent Write Failed.");
                        }
                    }
                    if (IsR2OpticalPowerLessThanOrEqual15mWKp)
                    {
                        IsSelectAnyCheckBox = true;
                        if (Workspace.This.EthernetController.SetOpticalPowerLessThanOrEqual15mWKp(LaserChannels.ChannelB, R2OpticalPowerLessThanOrEqual15mWKp) == false)
                        {
                            MessageBox.Show("R2OpticalPowerLessThanOrEqual15mWKp Write Failed.");
                        }
                    }
                    if (IsR2OpticalPowerLessThanOrEqual15mWKi)
                    {
                        IsSelectAnyCheckBox = true;
                        if (Workspace.This.EthernetController.SetOpticalPowerLessThanOrEqual15mWKi(LaserChannels.ChannelB, R2OpticalPowerLessThanOrEqual15mWKi) == false)
                        {
                            MessageBox.Show("R2OpticalPowerLessThanOrEqual15mWKi Write Failed.");
                        }
                    }
                    if (IsR2OpticalPowerLessThanOrEqual15mWKd)
                    {
                        IsSelectAnyCheckBox = true;
                        if (Workspace.This.EthernetController.SetOpticalPowerLessThanOrEqual15mWKd(LaserChannels.ChannelB, R2OpticalPowerLessThanOrEqual15mWKd) == false)
                        {
                            MessageBox.Show("R2OpticalPowerLessThanOrEqual15mWKd Write Failed.");
                        }
                    }
                    if (IsR2OpticalPowerGreaterThan15mWKp)
                    {
                        IsSelectAnyCheckBox = true;
                        if (Workspace.This.EthernetController.SetOpticalPowerGreaterThan15mWKp(LaserChannels.ChannelB, R2OpticalPowerGreaterThan15mWKp) == false)
                        {
                            MessageBox.Show("R2OpticalPowerGreaterThan15mWKp Write Failed.");
                        }

                    }
                    if (IsR2OpticalPowerGreaterThan15mWKi)
                    {
                        IsSelectAnyCheckBox = true;
                        if (Workspace.This.EthernetController.SetOpticalPowerGreaterThan15mWKi(LaserChannels.ChannelB, R2OpticalPowerGreaterThan15mWKi) == false)
                        {
                            MessageBox.Show("R2OpticalPowerGreaterThan15mWKi Write Failed.");
                        }
                    }
                    if (IsR2OpticalPowerGreaterThan15mWKd)
                    {
                        IsSelectAnyCheckBox = true;
                        if (Workspace.This.EthernetController.SetOpticalPowerGreaterThan15mWKd(LaserChannels.ChannelB, R2OpticalPowerGreaterThan15mWKd) == false)
                        {
                            MessageBox.Show("R2OpticalPowerGreaterThan15mWKd Write Failed.");
                        }
                    }

                }
            }*/

            if (!IsSelectAnyCheckBox)
            {
                AvocadoDeviceProperties deviceProperties;
                var ptr = Marshal.AllocHGlobal(256);
                deviceProperties = (AvocadoDeviceProperties)Marshal.PtrToStructure(ptr, typeof(AvocadoDeviceProperties));
                Marshal.FreeHGlobal(ptr);
                deviceProperties.LogicalHomeX = (float)R1XLogicalHome;
                deviceProperties.LogicalHomeY = (float)R1YLogicalHome;
                deviceProperties.OpticalLR1Distance = (float)OpticalR2_R1Distance;
                deviceProperties.PixelOffsetR1 = Pixel_10_Offset_R2_R1;
                deviceProperties.OpticalLR2Distance = (float)OpticalL_R1Distance;
                deviceProperties.PixelOffsetR2 = Pixel_10_Offset_L_R1;
                deviceProperties.PixelOffsetDxCHR1 = Pixel_10_R1_DX;
                deviceProperties.PixelOffsetDyCHR1 = Pixel_10_R1_DY;
                deviceProperties.PixelOffsetDxCHR2 = Pixel_10_R2_DX;
                deviceProperties.PixelOffsetDyCHR2 = Pixel_10_R2_DY;
                deviceProperties.ZFocusPosition = FocusLength;
                if (SystemSN != null)
                {
                    byte[] sysSN = System.Text.Encoding.Default.GetBytes(SystemSN);
                    for (int i = 0; i < deviceProperties.SysSN.Length; i++)
                    {
                        if (i < sysSN.Length)
                        {
                            deviceProperties.SysSN[i] = sysSN[i];
                        }
                        else
                        {
                            deviceProperties.SysSN[i] = 0;
                        }
                    }
                }
                deviceProperties.PixelOffsetDxCHL = Pixel_10_L_DX;
                deviceProperties.PixelOffsetDyCHL = Pixel_10_L_DY;
                deviceProperties.XEncoderSubdivision = XEncoderSubdivision;
                deviceProperties.FanSwitchInterval = ((int)FanSwitchInterval * 100) * 2;
                deviceProperties.FanReserveTemperature = ((int)FanReserveTemperature * 100) - ((int)FanSwitchInterval * 100);
                //EL: TODO:
                //deviceProperties.LCoefficient = LCoefficient;
                //deviceProperties.L375Coefficient = L375Coefficient;
                //deviceProperties.R1Coefficient = R1Coefficient;
                //deviceProperties.R2Coefficient = R2Coefficient;
                //deviceProperties.R2532Coefficient = R2532Coefficient;
                //deviceProperties.ShellFanDefaultSpeed = ShellFanDefaultSpeed;
                //deviceProperties.CH1AlertWarningSwitch = CH1AlertWarningSwitch;
                //deviceProperties.CH1WarningTemperature = CH1WarningTemperature;
                if (Workspace.This.EthernetController.SetDeviceProperties(deviceProperties) == false)
                {
                    MessageBox.Show("Write Failed.");
                    return;
                }
                else
                {
                    //Workspace.This.ScannerVM.ScanDeltaX= Workspace.This.MotorVM.XMaxValue - Workspace.This.EthernetController.DeviceProperties.OpticalLR2Distance- (int)Workspace.This.ScannerVM.ScanX0-1;
                    Workspace.This.MotorVM.LimitsXPlus = Workspace.This.MotorVM.XMaxValue - Workspace.This.EthernetController.DeviceProperties.OpticalLR2Distance;
                }
            }

            //CannelCheckBox();
            Thread.Sleep(1000);
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                Window window = new Window();
                MessageBox.Show(window, "Write Succeed");
            });
        }

        public bool CanExecuteParametersWriteCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region ParametersReadCommand

        public ICommand ParametersReadCommand
        {
            get
            {
                if (_ParametersReadCommand == null)
                {
                    _ParametersReadCommand = new RelayCommand(ExecuteParametersReadCommand, CanExecuteParametersReadCommand);
                }

                return _ParametersReadCommand;
            }
        }
        public void ExecuteParametersReadCommand(object parameter)
        {
            /*IsEnabledL1 = true;
            IsEnabledR1 = true;
            IsEnabledR2 = true;
            Is532EnabledL1 = Visibility.Visible;
            Is532EnabledR1 = Visibility.Visible;
            Is532EnabledR2 = Visibility.Visible;
            if (Workspace.This.LaserModuleL1.LaserWavelength != 0 && Workspace.This.LaserModuleL1.LaserWavelength != Workspace.This.NewParameterVM.Uint16Code)
            {
                if (Workspace.This.LaserModuleL1.LaserWavelength != 532)
                {
                    Workspace.This.NewParameterVM.Is532EnabledL1 = Visibility.Hidden;
                }
            }
            else
            {
                Workspace.This.NewParameterVM.IsEnabledL1 = false;
                Workspace.This.NewParameterVM.Is532EnabledL1 = Visibility.Hidden;
            }


            if (Workspace.This.LaserModuleR1.LaserWavelength != 0 && Workspace.This.LaserModuleR1.LaserWavelength != Workspace.This.NewParameterVM.Uint16Code)
            {
                if (Workspace.This.LaserModuleR1.LaserWavelength != 532)
                {
                    Workspace.This.NewParameterVM.Is532EnabledR1 = Visibility.Hidden;
                }
            }
            else
            {
                Workspace.This.NewParameterVM.IsEnabledR1 = false;
                Workspace.This.NewParameterVM.Is532EnabledR1 = Visibility.Hidden;
            }

            if (Workspace.This.LaserModuleR2.LaserWavelength != 0 && Workspace.This.LaserModuleR2.LaserWavelength != Workspace.This.NewParameterVM.Uint16Code)
            {

                if (Workspace.This.LaserModuleR2.LaserWavelength != 532)
                {
                    Workspace.This.NewParameterVM.Is532EnabledR2 = Visibility.Hidden;
                }
            }
            else
            {
                Workspace.This.NewParameterVM.IsEnabledR2 = false;
                Workspace.This.NewParameterVM.Is532EnabledR2 = Visibility.Hidden;
            }*/

            if (Workspace.This.EthernetController.GetDeviceProperties() == false)
            {
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    Window window = new Window();
                    MessageBox.Show(window, "Read failed.");
                });
                return;
            }

            R1XLogicalHome = Workspace.This.EthernetController.DeviceProperties.LogicalHomeX;
            R1YLogicalHome = Workspace.This.EthernetController.DeviceProperties.LogicalHomeY;
            OpticalR2_R1Distance = Workspace.This.EthernetController.DeviceProperties.OpticalLR1Distance;
            Pixel_10_Offset_R2_R1 = Workspace.This.EthernetController.DeviceProperties.PixelOffsetR1;
            OpticalL_R1Distance = Workspace.This.EthernetController.DeviceProperties.OpticalLR2Distance;
            Pixel_10_Offset_L_R1 = Workspace.This.EthernetController.DeviceProperties.PixelOffsetR2;
            Pixel_10_R1_DX = Workspace.This.EthernetController.DeviceProperties.PixelOffsetDxCHR1;
            Pixel_10_R1_DY = Workspace.This.EthernetController.DeviceProperties.PixelOffsetDyCHR1;
            Pixel_10_R2_DX = Workspace.This.EthernetController.DeviceProperties.PixelOffsetDxCHR2;
            Pixel_10_R2_DY = Workspace.This.EthernetController.DeviceProperties.PixelOffsetDyCHR2;
            FocusLength = Workspace.This.EthernetController.DeviceProperties.ZFocusPosition;
            SystemSN = Encoding.ASCII.GetString(Workspace.This.EthernetController.DeviceProperties.SysSN).TrimEnd('\0');
            Pixel_10_L_DX = Workspace.This.EthernetController.DeviceProperties.PixelOffsetDxCHL;
            Pixel_10_L_DY = Workspace.This.EthernetController.DeviceProperties.PixelOffsetDyCHL;
            XEncoderSubdivision = Workspace.This.EthernetController.DeviceProperties.XEncoderSubdivision;
            FanSwitchInterval = (Workspace.This.EthernetController.DeviceProperties.FanSwitchInterval / 100) / 2;
            FanReserveTemperature = (Workspace.This.EthernetController.DeviceProperties.FanReserveTemperature / 100) + FanSwitchInterval;
            try
            {
                var lCoefficient = (float)Math.Round(Workspace.This.EthernetController.DeviceProperties.LCoefficient, 3);
                LCoefficient = (lCoefficient > 0 && lCoefficient < 0.6) ? lCoefficient : 0.0F;
                var l375Coefficient = (float)Math.Round(Workspace.This.EthernetController.DeviceProperties.L375Coefficient, 3);
                L375Coefficient = (l375Coefficient > 0 && l375Coefficient < 0.6) ? l375Coefficient : 0F;
                var r1Coefficient = (float)Math.Round(Workspace.This.EthernetController.DeviceProperties.R1Coefficient, 3);
                R1Coefficient = (r1Coefficient > 0 && r1Coefficient < 0.6) ? r1Coefficient : 0F;
                var r2Coefficient = (float)Math.Round(Workspace.This.EthernetController.DeviceProperties.R2Coefficient, 3);
                R2Coefficient = (r2Coefficient > 0 && r2Coefficient < 0.6) ? r2Coefficient : 0F;
                var r2532Coefficient = (float)Math.Round(Workspace.This.EthernetController.DeviceProperties.R2532Coefficient, 3);
                R2532Coefficient = (r2532Coefficient > 0 && r2532Coefficient < 0.6) ? r2532Coefficient : 0F;
            }
            catch
            {
                LCoefficient = 0;
                L375Coefficient = 0;
                R1Coefficient = 0;
                R2Coefficient = 0;
                R2532Coefficient = 0;
            }
            //ShellFanDefaultSpeed = Workspace.This.EthernetController.DeviceProperties.ShellFanDefaultSpeed;
            //CH1WarningTemperature = Workspace.This.EthernetController.DeviceProperties.CH1WarningTemperature;
            //CH1AlertWarningSwitch = Workspace.This.EthernetController.DeviceProperties.CH1AlertWarningSwitch;
            //GetModelInfo();
        }

        public bool CanExecuteParametersReadCommand(object parameter)
        {
            return true;
        }
        /*public void GetOtherSetting()
        {
            if (Workspace.This.EthernetController.GetDeviceProperties() == false)
            {
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    Window window = new Window();
                    MessageBox.Show(window, "Read failed.");
                });
                return;
            }
            R1XLogicalHome = Workspace.This.EthernetController.DeviceProperties.LogicalHomeX;
            R1YLogicalHome = Workspace.This.EthernetController.DeviceProperties.LogicalHomeY;
            OpticalR2_R1Distance = Workspace.This.EthernetController.DeviceProperties.OpticalLR1Distance;
            Pixel_10_Offset_R2_R1 = Workspace.This.EthernetController.DeviceProperties.PixelOffsetR1;
            OpticalL_R1Distance = Workspace.This.EthernetController.DeviceProperties.OpticalLR2Distance;
            Pixel_10_Offset_L_R1 = Workspace.This.EthernetController.DeviceProperties.PixelOffsetR2;
            Pixel_10_R1_DX = Workspace.This.EthernetController.DeviceProperties.PixelOffsetDxCHR1;
            Pixel_10_R1_DY = Workspace.This.EthernetController.DeviceProperties.PixelOffsetDyCHR1;
            Pixel_10_R2_DX = Workspace.This.EthernetController.DeviceProperties.PixelOffsetDxCHR2;
            Pixel_10_R2_DY = Workspace.This.EthernetController.DeviceProperties.PixelOffsetDyCHR2;
            FocusLength = Workspace.This.EthernetController.DeviceProperties.ZFocusPosition;
            SystemSN = Encoding.ASCII.GetString(Workspace.This.EthernetController.DeviceProperties.SysSN).TrimEnd('\0');
            Pixel_10_L_DX = Workspace.This.EthernetController.DeviceProperties.PixelOffsetDxCHL;
            Pixel_10_L_DY = Workspace.This.EthernetController.DeviceProperties.PixelOffsetDyCHL;
            XEncoderSubdivision = Workspace.This.EthernetController.DeviceProperties.XEncoderSubdivision;
            FanSwitchInterval = (Workspace.This.EthernetController.DeviceProperties.FanSwitchInterval / 100) / 2;
            FanReserveTemperature = (Workspace.This.EthernetController.DeviceProperties.FanReserveTemperature / 100) + FanSwitchInterval;
            LCoefficient = Workspace.This.EthernetController.DeviceProperties.LCoefficient;
            L375Coefficient = Workspace.This.EthernetController.DeviceProperties.L375Coefficient;
            R1Coefficient = Workspace.This.EthernetController.DeviceProperties.R1Coefficient;
            R2Coefficient = Workspace.This.EthernetController.DeviceProperties.R2Coefficient;
            R2532Coefficient = Workspace.This.EthernetController.DeviceProperties.R2532Coefficient;
            ShellFanDefaultSpeed = Workspace.This.EthernetController.DeviceProperties.ShellFanDefaultSpeed;
            CH1WarningTemperature = Workspace.This.EthernetController.DeviceProperties.CH1WarningTemperature;
            CH1AlertWarningSwitch = Workspace.This.EthernetController.DeviceProperties.CH1AlertWarningSwitch;
        }*/
        //Reading parameters in the optical module
        //读取光学模块中的参数
        /*public void GetModelInfo()
        {
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                Workspace.This.PreloadModuleInformation();
            });
            CannelCheckBox();
        }*/

        #endregion

        /*private void CannelCheckBox()
        {
            IsLTEControlTemperature = false;
            IsLTECMaximumCoolingCurrent = false;
            IsLTECControlKp = false;
            IsLTECControlKi = false;
            IsLTECControlKd = false;
            IsLLaserMaxCurrent = false;
            IsLLaserMinCurrent = false;
            IsLOpticalPowerLessThanOrEqual15mWKp = false;
            IsLOpticalPowerLessThanOrEqual15mWKi = false;
            IsLOpticalPowerLessThanOrEqual15mWKd = false;
            IsLOpticalPowerGreaterThan15mWKp = false;
            IsLOpticalPowerGreaterThan15mWKi = false;
            IsLOpticalPowerGreaterThan15mWKd = false;
            IsR1TEControlTemperature = false;
            IsR1TECMaximumCoolingCurrent = false;
            IsR1TECControlKp = false;
            IsR1TECControlKi = false;
            IsR1TECControlKd = false;
            IsR1LaserMaxCurrent = false;
            IsR1LaserMinCurrent = false;
            IsR1OpticalPowerLessThanOrEqual15mWKp = false;
            IsR1OpticalPowerLessThanOrEqual15mWKi = false;
            IsR1OpticalPowerLessThanOrEqual15mWKd = false;
            IsR1OpticalPowerGreaterThan15mWKp = false;
            IsR1OpticalPowerGreaterThan15mWKi = false;
            IsR1OpticalPowerGreaterThan15mWKd = false;
            IsR2TEControlTemperature = false;
            IsR2TECMaximumCoolingCurrent = false;
            IsR2TECControlKp = false;
            IsR2TECControlKi = false;
            IsR2TECControlKd = false;
            IsR2LaserMaxCurrent = false;
            IsR2LaserMinCurrent = false;
            IsR2OpticalPowerLessThanOrEqual15mWKp = false;
            IsR2OpticalPowerLessThanOrEqual15mWKi = false;
            IsR2OpticalPowerLessThanOrEqual15mWKd = false;
            IsR2OpticalPowerGreaterThan15mWKp = false;
            IsR2OpticalPowerGreaterThan15mWKi = false;
            IsR2OpticalPowerGreaterThan15mWKd = false;
            IsLCheckALL = false;
            IsR1CheckALL = false;
            IsR2CheckALL = false;
        }*/
    }
}
