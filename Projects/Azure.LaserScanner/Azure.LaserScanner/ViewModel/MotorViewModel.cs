using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input; //ICommand
using System.Windows;       //MessageBox
using Azure.WPF.Framework;
using Azure.ImagingSystem;
using Azure.Configuration.Settings;
using Azure.MotionLib;

namespace Azure.LaserScanner.ViewModel
{
    class MotorViewModel : ViewModelBase
    {
        #region Private data...
        private double _XMotorSubdivision = 1000;
        private double _YMotorSubdivision = 800;
        private double _ZMotorSubdivision = 5000;
        private double _WMotorSubdivision = 1000;

        private double _XMaxValue = 330000;
        private double _YMaxValue = 156000;
        private double _ZMaxValue = 48000;
        private double _WMaxValue = 10000;

        private double _MotorXSpeed = 20000;
        private double _MotorYSpeed = 3840;
        private double _MotorZSpeed = 0;
        private double _MotorWSpeed = 0;

        private double _MotorXAccel = 8000000;
        private double _MotorYAccel = 8000000;
        private double _MotorZAccel = 1024000;
        private double _MotorZDccel = 64000;
        private double _MotorWAccel = 10000;

        private double _AbsXPos = 0;
        private double _AbsYPos = 0;
        private double _AbsZPos = 0;
        private double _AbsWPos = 0;

        private double _RelativeXPos = 0;
        private double _RelativeYPos = 0;
        private double _RelativeZPos = 0;
        private double _RelativeWPos = 0;

        private double _CurrentXPos = 0;
        private double _CurrentYPos = 0;
        private double _CurrentZPos = 0;
        private double _CurrentWPos = 0;

        private bool _IsXLimited = false;
        private bool _IsYLimited = false;
        private bool _IsZLimited = false;

        private bool _MotorXPowerSwitch = true;
        private bool _MotorYPowerSwitch = true;
        private bool _MotorZPowerSwitch = true;
        private bool _MotorWPowerSwitch = true;

        private bool _IsMotorXEnabled = true;
        private bool _IsMotorYEnabled = true;
        private bool _IsMotorZEnabled = true;
        private bool _IsMotorWEnabled = true;

        private double _LimitsXMinus = 0;
        private double _LimitsYMinus = 0;
        private double _LimitsZMinus = -2;
        private double _LimitsWMinus = -2;
        private double _LimitsXPlus = 0;
        private double _LimitsYPlus = 0;
        private double _LimitsZPlus = 0;
        private double _LimitsWPlus = 0;

        private bool _IsMotorAlreadyHome = false;


        private RelayCommand _HomeCommand = null;
        private RelayCommand _GoAbsPosCommand = null;
        private RelayCommand _RelativePosMinusCommand = null;
        private RelayCommand _RelativePosPlusCommand = null;

        private MotionController _MotionController;
        #endregion

        #region Constructors...

        public MotorViewModel()
        {
            /*_XMotorSubdivision = SettingsManager.ConfigSettings.XMotorSubdivision;
            _YMotorSubdivision = SettingsManager.ConfigSettings.YMotorSubdivision;
            _ZMotorSubdivision = SettingsManager.ConfigSettings.ZMotorSubdivision;
            _XMaxValue = SettingsManager.ConfigSettings.XMaxValue;
            _YMaxValue = SettingsManager.ConfigSettings.YMaxValue;
            _ZMaxValue = SettingsManager.ConfigSettings.ZMaxValue;
            if (_XMotorSubdivision > 0)
                _LimitsXPlus = Math.Round(_XMaxValue / _XMotorSubdivision, 3);
            if (_YMotorSubdivision > 0)
                _LimitsYPlus = Math.Round(_YMaxValue / _YMotorSubdivision, 3);
            if (_ZMotorSubdivision > 0)
                _LimitsZPlus = Math.Round(_ZMaxValue / _ZMotorSubdivision, 3);

            if (SettingsManager.ConfigSettings.MotorSettings != null && SettingsManager.ConfigSettings.MotorSettings.Count > 0)
            {
                foreach (var motorSetting in SettingsManager.ConfigSettings.MotorSettings)
                {
                    switch (motorSetting.MotorType)
                    {
                        case MotorType.X:
                            _MotorXSpeed = motorSetting.Speed * _XMotorSubdivision;
                            _AbsXPos = motorSetting.Position * _XMotorSubdivision;
                            break;
                        case MotorType.Y:
                            _MotorYSpeed = motorSetting.Speed * _YMotorSubdivision;
                            _AbsYPos = motorSetting.Position * _YMotorSubdivision;
                            break;
                        case MotorType.Z:
                            _MotorZSpeed = motorSetting.Speed * _ZMotorSubdivision;
                            _AbsZPos = motorSetting.Position * _ZMotorSubdivision;
                            break;
                    }
                }
            }*/

        }

        #endregion

        #region Public properties...
        public bool IsNewFirmware { get; set; }

        public bool IsMotorAlreadyHome
        {
            get
            {
                return _IsMotorAlreadyHome;
            }
        }

        public double XMaxValue
        {
            get
            {
                double result = 0;
                if (_XMotorSubdivision > 0)
                    result = Math.Round(_XMaxValue / _XMotorSubdivision, 2);
                return result;
            }
            set
            {
                _XMaxValue = value * _XMotorSubdivision;
            }
        }

        public double YMaxValue
        {
            get
            {
                double result = 0;
                if (_YMotorSubdivision > 0)
                    result = Math.Round(_YMaxValue / _YMotorSubdivision, 2);
                return result;
            }
            set
            {
                _YMaxValue = value * _YMotorSubdivision;
            }
        }

        public double ZMaxValue
        {
            get
            {
                double result = 0;
                if (_ZMotorSubdivision > 0)
                    result = Math.Round(_ZMaxValue / _ZMotorSubdivision, 2);
                return result;
            }
            set
            {
                _ZMaxValue = value * _ZMotorSubdivision;
            }
        }


        public double MotorXSpeed
        {
            get
            {
                double result = 0;
                if (_XMotorSubdivision > 0)
                {
                    result = Math.Round(_MotorXSpeed / _XMotorSubdivision, 2);
                }
                return result;
            }
            set
            {
                if (_MotorXSpeed != value)
                {
                    _MotorXSpeed = value * _XMotorSubdivision;
                    RaisePropertyChanged("MotorXSpeed");
                    // set X motor speed
                    if (IsNewFirmware)
                    {
                        //Workspace.This.ApdVM.APDTransfer.MotionControl.SetTopSpeed(Hats.APDCom.MotionTypes.X, (int)_MotorXSpeed);
                    }
                }
            }
        }
        public double MotorYSpeed
        {
            get
            {
                double result = 0;
                if (_YMotorSubdivision > 0)
                    result = Math.Round(_MotorYSpeed / _YMotorSubdivision, 2);
                return result;
            }
            set
            {
                if (_MotorYSpeed != value)
                {
                    _MotorYSpeed = value * _YMotorSubdivision;
                    RaisePropertyChanged("MotoreYSpeed");
                    // set Y motor speed
                    if (IsNewFirmware)
                    {
                        //Workspace.This.ApdVM.APDTransfer.MotionControl.SetTopSpeed(Hats.APDCom.MotionTypes.Y, (int)_MotorYSpeed);
                    }
                }
            }
        }
        public double MotorZSpeed
        {
            get
            {
                double result = 0;
                if (_ZMotorSubdivision > 0)
                    result = Math.Round(_MotorZSpeed / _ZMotorSubdivision, 2);
                return result;
            }
            set
            {
                if ((_MotorZSpeed / _ZMotorSubdivision) != value)
                {
                    _MotorZSpeed = value * _ZMotorSubdivision;
                    RaisePropertyChanged("MotorZSpeed");
                    // set Z motor speed
                    if (IsNewFirmware)
                    {
                        //Workspace.This.ApdVM.APDTransfer.MotionControl.SetTopSpeed(Hats.APDCom.MotionTypes.Z, (int)_MotorZSpeed);
                    }
                }
            }
        }
        public double MotorWSpeed
        {
            get
            {
                double result = 0;
                if (_WMotorSubdivision > 0)
                    result = Math.Round(_MotorWSpeed / _WMotorSubdivision, 3);
                return result;
            }
            set
            {
                if ((_MotorWSpeed / _WMotorSubdivision) != value)
                {
                    _MotorWSpeed = value * _WMotorSubdivision;
                    RaisePropertyChanged("MotorWSpeed");
                    // set Z motor speed
                    if (IsNewFirmware)
                    {
                        //Workspace.This.ApdVM.APDTransfer.MotionControl.SetTopSpeed(Hats.APDCom.MotionTypes.W, (int)_MotorWSpeed);
                    }
                }
            }
        }


        public double MotorXAccel
        {
            get { return _MotorXAccel / _XMotorSubdivision; }
            set
            {
                if (_MotorXAccel / _XMotorSubdivision != value)
                {
                    _MotorXAccel = value * _XMotorSubdivision;
                    RaisePropertyChanged("MotorXAccel");
                    // set X motor acceleration
                    if (_MotorXAccel > 0 || _MotorXAccel < 65535)
                    {
                        if (IsNewFirmware)
                        {
                            //Workspace.This.ApdVM.APDTransfer.MotionControl.SetAccVal(Hats.APDCom.MotionTypes.X, (int)_MotorXAccel);
                        }
                    }
                    else
                    {
                        MessageBox.Show("The ACCEL is not more than 65535！", "Error");
                    }

                }
            }
        }
        public double MotorYAccel
        {
            get { return _MotorYAccel / _YMotorSubdivision; }
            set
            {
                if (_MotorYAccel / _YMotorSubdivision != value)
                {
                    _MotorYAccel = value * _YMotorSubdivision;
                    RaisePropertyChanged("MotorYAccel");
                    // set Y motor acceleration
                    if (_MotorYAccel > 0 || _MotorYAccel < 65535)
                    {
                        if (IsNewFirmware)
                        {
                            //Workspace.This.ApdVM.APDTransfer.MotionControl.SetAccVal(Hats.APDCom.MotionTypes.X, (int)_MotorYAccel);
                        }
                    }
                    else
                    {
                        MessageBox.Show("The ACCEL is not more than 65535！", "Error");
                    }
                }
            }
        }
        public double MotorZAccel
        {
            get { return _MotorZAccel / _ZMotorSubdivision; }
            set
            {
                if (_MotorZAccel / _ZMotorSubdivision != value)
                {
                    _MotorZAccel = value * _ZMotorSubdivision;
                    RaisePropertyChanged("MotorZAccel");
                    // set Z motor acceleration
                    if (_MotorZAccel > 0 || _MotorZAccel < 65535)
                    {
                        if (IsNewFirmware)
                        {
                            //Workspace.This.ApdVM.APDTransfer.MotionControl.SetAccVal(Hats.APDCom.MotionTypes.Z, (int)_MotorZAccel);
                        }
                    }
                    else
                    {
                        MessageBox.Show("The ACCEL is not more than 65535！", "Error");
                    }
                }
            }
        }
        public double MotorZDccel
        {
            get { return _MotorZDccel / _ZMotorSubdivision; }
            set
            {
                if (_MotorZDccel / _ZMotorSubdivision != value)
                {
                    _MotorZDccel = value * _ZMotorSubdivision;
                    RaisePropertyChanged("MotorZAccel");
                    // set Z motor deceleration
                    if (_MotorZDccel > 0 || _MotorZDccel < 65535)
                    {
                        if (IsNewFirmware)
                        {
                            //Workspace.This.ApdVM.APDTransfer.MotionControl.SetDccVal(Hats.APDCom.MotionTypes.Z, (int)_MotorZDccel);
                        }
                    }
                    else
                    {
                        MessageBox.Show("The ACCEL is not more than 65535！", "Error");
                    }
                }
            }
        }
        public double MotorWAccel
        {
            get { return _MotorWAccel / _WMotorSubdivision; }
            set
            {
                if (_MotorWAccel / _WMotorSubdivision != value)
                {
                    _MotorWAccel = value * _WMotorSubdivision;
                    RaisePropertyChanged("MotorWAccel");
                    // set Z motor acceleration
                    if (_MotorWAccel > 0 || _MotorWAccel < 65535)
                    {
                        if (IsNewFirmware)
                        {
                            //Workspace.This.ApdVM.APDTransfer.MotionControl.SetAccVal(Hats.APDCom.MotionTypes.W, (int)_MotorWAccel);
                        }
                    }
                    else
                    {
                        MessageBox.Show("The ACCEL is not more than 65535！", "Error");
                    }
                }
            }
        }

        public double AbsXPos
        {
            get
            {
                double result = 0;
                if (_XMotorSubdivision > 0)
                    result = Math.Round(_AbsXPos / _XMotorSubdivision, 2);
                return result;
            }
            set
            {
                if (_AbsXPos != value)
                {
                    if (value <= XMaxValue && value >= 0)
                    {
                        _AbsXPos = value * _XMotorSubdivision;
                        RaisePropertyChanged("AbsXPos");
                    }
                    else
                    {
                        MessageBox.Show(string.Format("you should type value 0-{0}!", XMaxValue), "Error");
                    }

                }
            }
        }
        public double AbsYPos
        {
            get
            {
                double result = 0;
                if (_YMotorSubdivision > 0)
                    result = Math.Round(_AbsYPos / _YMotorSubdivision, 2);
                return result;
            }
            set
            {
                if (_AbsYPos != value)
                {
                    if (value <= YMaxValue && value >= 0)
                    {
                        _AbsYPos = value * _YMotorSubdivision;
                        RaisePropertyChanged("AbsYPos");
                    }
                    else
                    {
                        MessageBox.Show(string.Format("you should type value 0-{0}!", YMaxValue), "Error");
                    }

                }
            }
        }
        public double AbsZPos
        {
            get
            {
                double result = 0;
                if (_ZMotorSubdivision > 0)
                    result = Math.Round(_AbsZPos / _ZMotorSubdivision, 2);
                return result;
            }
            set
            {
                if ((_AbsZPos / _ZMotorSubdivision) != value)
                {
                    if (value <= ZMaxValue && value >= _LimitsZMinus)
                    {
                        _AbsZPos = value * _ZMotorSubdivision;
                        RaisePropertyChanged("AbsZPos");
                    }
                    else
                    {
                        MessageBox.Show(string.Format("you should type value {1}-{0}!", ZMaxValue, _LimitsZMinus), "Error");
                    }
                }
            }
        }
        public double AbsWPos
        {
            get
            {
                double result = 0;
                if (_WMotorSubdivision > 0)
                    result = Math.Round(_AbsWPos / _WMotorSubdivision, 3);
                return result;
            }
            set
            {
                if ((_AbsZPos / _WMotorSubdivision) != value)
                {
                    _AbsZPos = value * _WMotorSubdivision;
                    RaisePropertyChanged("AbsWPos");
                }
            }
        }

        public double RelativeXPos
        {
            get
            {
                double result = 0;
                if (_XMotorSubdivision > 0)
                    result = Math.Round(_RelativeXPos / _XMotorSubdivision, 2);
                return result;
            }
            set
            {
                if (_RelativeXPos != value)
                {
                    _RelativeXPos = value * _XMotorSubdivision;
                    RaisePropertyChanged("RelativeXPos");
                }
            }
        }
        public double RelativeYPos
        {
            get
            {
                double result = 0;
                if (_YMotorSubdivision > 0)
                    result = Math.Round(_RelativeYPos / _YMotorSubdivision, 2);
                return result;
            }
            set
            {
                if (_RelativeYPos != value)
                {
                    _RelativeYPos = value * _YMotorSubdivision;
                    RaisePropertyChanged("RelativeYPos");
                }
            }
        }
        public double RelativeZPos
        {
            get
            {
                double result = 0;
                if (_ZMotorSubdivision > 0)
                    result = Math.Round(_RelativeZPos / _ZMotorSubdivision, 2);
                return result;
            }
            set
            {
                if ((_RelativeZPos / _ZMotorSubdivision) != value)
                {
                    _RelativeZPos = value * _ZMotorSubdivision;
                    RaisePropertyChanged("RelativeZPos");
                }
            }
        }
        public double RelativeWPos
        {
            get
            {
                double result = 0;
                if (_WMotorSubdivision > 0)
                    result = Math.Round(_RelativeWPos / _WMotorSubdivision, 3);
                return result;
            }
            set
            {
                if ((_RelativeWPos / _WMotorSubdivision) != value)
                {
                    _RelativeWPos = value * _WMotorSubdivision;
                    RaisePropertyChanged("RelativeWPos");
                }
            }
        }

        public double CurrentXPos
        {
            get { return _CurrentXPos; }
            set
            {
                if (_CurrentXPos != value)
                {
                    _CurrentXPos = value;
                    RaisePropertyChanged("CurrentXPos");
                }
            }
        }
        public double CurrentYPos
        {
            get { return _CurrentYPos; }
            set
            {
                if (_CurrentYPos != value)
                {
                    _CurrentYPos = value;
                    RaisePropertyChanged("CurrentYPos");
                }
            }
        }
        public double CurrentZPos
        {
            get { return _CurrentZPos; }
            set
            {
                if (_CurrentZPos != value)
                {
                    _CurrentZPos = value;
                    RaisePropertyChanged("CurrentZPos");
                }
            }
        }
        public double CurrentWPos
        {
            get { return _CurrentWPos; }
            set
            {
                if (_CurrentWPos != value)
                {
                    _CurrentWPos = value;
                    RaisePropertyChanged("CurrentWPos");
                }
            }
        }
        public bool IsXLimited
        {
            get
            {
                return _IsXLimited;
            }
            set
            {
                if (_IsXLimited != value)
                {
                    _IsXLimited = value;
                    RaisePropertyChanged("IsXLimited");
                }
            }
        }
        public bool IsYLimited
        {
            get
            {
                return _IsYLimited;
            }
            set
            {
                if (_IsYLimited != value)
                {
                    _IsYLimited = value;
                    RaisePropertyChanged("IsYLimited");
                }
            }
        }

        public bool IsZLimited
        {
            get
            {
                return _IsZLimited;
            }
            set
            {
                if (_IsZLimited != value)
                {
                    _IsZLimited = value;
                    RaisePropertyChanged("IsZLimited");
                }
            }
        }



        public bool MotorXPowerSwitch
        {
            get
            {
                return _MotorXPowerSwitch;
            }
            set
            {
                if (_MotorXPowerSwitch != value)
                {
                    _MotorXPowerSwitch = value;
                    IsMotorXEnabled = value;
                    RaisePropertyChanged("_MotorXPowerSwitch");
                }
            }
        }

        public bool MotorYPowerSwitch
        {
            get
            {
                return _MotorYPowerSwitch;
            }
            set
            {
                if (_MotorYPowerSwitch != value)
                {
                    _MotorYPowerSwitch = value;
                    IsMotorYEnabled = value;
                    RaisePropertyChanged("_MotorYPowerSwitch");
                }
            }
        }

        public bool MotorZPowerSwitch
        {
            get
            {
                return _MotorZPowerSwitch;
            }
            set
            {
                if (_MotorZPowerSwitch != value)
                {
                    _MotorZPowerSwitch = value;
                    IsMotorZEnabled = value;
                    RaisePropertyChanged("_MotorZPowerSwitch");
                }
            }
        }
        public bool MotorWPowerSwitch
        {
            get
            {
                return _MotorWPowerSwitch;
            }
            set
            {
                if (_MotorWPowerSwitch != value)
                {
                    _MotorZPowerSwitch = value;
                    IsMotorWEnabled = value;
                    RaisePropertyChanged("_MotorWPowerSwitch");
                }
            }
        }

        public bool IsMotorXEnabled
        {
            get { return _IsMotorXEnabled; }
            set
            {
                if (_IsMotorXEnabled != value)
                {
                    _IsMotorXEnabled = value;
                    RaisePropertyChanged("IsMotorXEnabled");
                    if (IsNewFirmware)
                    {
                        MotionController.SetEnables(EthernetCommLib.MotorTypes.X, new bool[] { _IsMotorXEnabled });
                    }
                }
            }
        }
        public bool IsMotorYEnabled
        {
            get { return _IsMotorYEnabled; }
            set
            {
                if (_IsMotorYEnabled != value)
                {
                    _IsMotorYEnabled = value;
                    RaisePropertyChanged("IsMotorYEnabled");
                    if (IsNewFirmware)
                    {
                        MotionController.SetEnables(EthernetCommLib.MotorTypes.Y, new bool[] { _IsMotorYEnabled });
                    }
                }
            }
        }
        public bool IsMotorZEnabled
        {
            get { return _IsMotorZEnabled; }
            set
            {
                if (_IsMotorZEnabled != value)
                {
                    _IsMotorZEnabled = value;
                    RaisePropertyChanged("IsMotorZEnabled");
                    if (IsNewFirmware)
                    {
                        MotionController.SetEnables(EthernetCommLib.MotorTypes.Z, new bool[] { _IsMotorZEnabled });
                    }
                }
            }
        }
        public bool IsMotorWEnabled
        {
            get { return _IsMotorWEnabled; }
            set
            {
                if (_IsMotorWEnabled != value)
                {
                    _IsMotorWEnabled = value;
                    RaisePropertyChanged("IsMotorWEnabled");
                    if (IsNewFirmware)
                    {
                        MotionController.SetEnables(EthernetCommLib.MotorTypes.W, new bool[] { _IsMotorWEnabled });
                    }
                }
            }
        }

        public double LimitsXMinus
        {
            get { return _LimitsXMinus; }
            set
            {
                if (_LimitsXMinus != value)
                {
                    _LimitsXMinus = value;
                    RaisePropertyChanged("LimitsXMinus");
                }
            }
        }
        public double LimitsYMinus
        {
            get { return _LimitsYMinus; }
            set
            {
                if (_LimitsYMinus != value)
                {
                    _LimitsYMinus = value;
                    RaisePropertyChanged("LimitsYMinus");
                }
            }
        }
        public double LimitsZMinus
        {
            get { return _LimitsZMinus; }
            set
            {
                if (_LimitsZMinus != value)
                {
                    _LimitsZMinus = value;
                    RaisePropertyChanged("LimitsZMinus");
                }
            }
        }
        public double LimitsWMinus
        {
            get { return _LimitsWMinus; }
            set
            {
                if (_LimitsWMinus != value)
                {
                    _LimitsWMinus = value;
                    RaisePropertyChanged("LimitsWMinus");
                }
            }
        }
        public double LimitsXPlus
        {
            get { return _LimitsXPlus; }
            set
            {
                if (_LimitsXPlus != value)
                {
                    _LimitsXPlus = value;
                    RaisePropertyChanged("LimitsXPlus");
                }
            }
        }
        public double LimitsYPlus
        {
            get { return _LimitsYPlus; }
            set
            {
                if (_LimitsYPlus != value)
                {
                    _LimitsYPlus = value;
                    RaisePropertyChanged("LimitsYPlus");
                }
            }
        }
        public double LimitsZPlus
        {
            get { return _LimitsZPlus; }
            set
            {
                if (_LimitsZPlus != value)
                {
                    _LimitsZPlus = value;
                    RaisePropertyChanged("LimitsZPlus");
                }
            }
        }
        public double LimitsWPlus
        {
            get { return _LimitsWPlus; }
            set
            {
                if (_LimitsWPlus != value)
                {
                    _LimitsWPlus = value;
                    RaisePropertyChanged("LimitsWPlus");
                }
            }
        }
        #endregion

        #region ICommand...

        #region HomeCommand

        public ICommand HomeCommand
        {
            get
            {
                if (_HomeCommand == null)
                {
                    _HomeCommand = new RelayCommand(ExecuteHomeCommand, CanExecuteHomeCommand);
                }

                return _HomeCommand;
            }
        }
        public void ExecuteHomeCommand(object parameter)
        {
            MotorType motorType = (MotorType)parameter;
            if (motorType == MotorType.X)
            {
                if (IsNewFirmware)
                {
                    //Workspace.This.ApdVM.APDTransfer.MotionControl.HomeMotion(Hats.APDCom.MotionTypes.X, 256, (int)_MotorXSpeed, (int)_MotorXAccel, (int)_MotorXAccel, false);
                    MotionController.HomeMotion(EthernetCommLib.MotorTypes.X,
                        new int[] { 256 }, new int[] { (int)_MotorXSpeed }, new int[] { (int)_MotorXAccel }, new int[] { (int)_MotorXAccel }, false);
                }
            }
            else if (motorType == MotorType.Y)
            {
                if (IsNewFirmware)
                {
                    //Workspace.This.ApdVM.APDTransfer.MotionControl.HomeMotion(Hats.APDCom.MotionTypes.Y, 256, (int)_MotorYSpeed, (int)_MotorYAccel, (int)_MotorYAccel, false);
                    MotionController.HomeMotion(EthernetCommLib.MotorTypes.Y,
                        new int[] { 256 }, new int[] { (int)_MotorYSpeed }, new int[] { (int)_MotorYAccel }, new int[] { (int)_MotorYAccel }, false);
                }
            }
            else if (motorType == MotorType.Z)
            {
                if (IsNewFirmware)
                {
                    //Workspace.This.ApdVM.APDTransfer.MotionControl.HomeMotion(Hats.APDCom.MotionTypes.Z, 256, (int)_MotorZSpeed, (int)_MotorZAccel, (int)_MotorZDccel, false);
                    MotionController.HomeMotion(EthernetCommLib.MotorTypes.Z,
               new int[] { 256 }, new int[] { (int)_MotorZSpeed }, new int[] { (int)_MotorZAccel }, new int[] { (int)_MotorZAccel }, false);
                }
            }
            else if (motorType == MotorType.W)
            {
                if (IsNewFirmware)
                {
                    //Workspace.This.ApdVM.APDTransfer.MotionControl.HomeMotion(Hats.APDCom.MotionTypes.W, 256, (int)_MotorWSpeed, (int)_MotorWAccel, (int)_MotorWAccel, false);
                    MotionController.HomeMotion(EthernetCommLib.MotorTypes.W,
               new int[] { 256 }, new int[] { (int)_MotorWSpeed }, new int[] { (int)_MotorWAccel }, new int[] { (int)_MotorWAccel }, false);
                }
            }
        }

        public bool CanExecuteHomeCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region GoAbsPosCommand

        public ICommand GoAbsPosCommand
        {
            get
            {
                if (_GoAbsPosCommand == null)
                {
                    _GoAbsPosCommand = new RelayCommand(ExecuteGoAbsPosCommand, CanExecuteGoAbsPosCommand);
                }

                return _GoAbsPosCommand;
            }
        }
        public void ExecuteGoAbsPosCommand(object parameter)
        {
            if (_IsMotorAlreadyHome == false && !IsNewFirmware)
            {
                return;
            }
            MotorType motorType = (MotorType)parameter;

            if (motorType == MotorType.X)
            {
                if (AbsXPos > 0 || AbsXPos <= _XMaxValue)
                {
                    if (IsNewFirmware)
                    {
                        //Workspace.This.ApdVM.APDTransfer.MotionControl.AbsoluteMove(Hats.APDCom.MotionTypes.X, 
                        //    256, (int)_MotorXSpeed, (int)_MotorXAccel, (int)_MotorXAccel, (int)_AbsXPos, false, true);
                        if (MotionController.AbsoluteMoveSingleMotion(EthernetCommLib.MotorTypes.X,
                      256, (int)_MotorXSpeed, (int)_MotorXAccel, (int)_MotorXAccel, (int)_AbsXPos, true, false) == false)
                        {
                            MessageBox.Show("Failed to Set new position");
                        }
                    }
                }
            }
            else if (motorType == MotorType.Y)
            {
                if (AbsYPos > 0 || AbsYPos <= _YMaxValue)
                {
                    if (IsNewFirmware)
                    {
                        //Workspace.This.ApdVM.APDTransfer.MotionControl.AbsoluteMove(Hats.APDCom.MotionTypes.Y,
                        //    256, (int)_MotorYSpeed, (int)_MotorYAccel, (int)_MotorYAccel, (int)_AbsYPos, false, true);
                        if (MotionController.AbsoluteMoveSingleMotion(EthernetCommLib.MotorTypes.Y,
             256, (int)_MotorYSpeed, (int)_MotorYAccel, (int)_MotorYAccel, (int)_AbsYPos, true, false) == false)
                        {
                            MessageBox.Show("Failed to Set new position");
                        }
                    }
                }

            }
            else if (motorType == MotorType.Z)
            {
                if (AbsZPos > _LimitsZMinus || AbsZPos <= _ZMaxValue)
                {
                    if (IsNewFirmware)
                    {
                        //Workspace.This.ApdVM.APDTransfer.MotionControl.AbsoluteMove(Hats.APDCom.MotionTypes.Z, 
                        //    256, (int)_MotorZSpeed, (int)_MotorZAccel, (int)_MotorZDccel, (int)_AbsZPos, false, true);
                        if (MotionController.AbsoluteMoveSingleMotion(EthernetCommLib.MotorTypes.Z,
        256, (int)_MotorZSpeed, (int)_MotorZAccel, (int)_MotorZAccel, (int)_AbsZPos, true, false) == false)
                        {
                            MessageBox.Show("Failed to Set new position");
                        }
                    }
                }
            }
            else if (motorType == MotorType.W)
            {
                if (IsNewFirmware)
                {
                    //Workspace.This.ApdVM.APDTransfer.MotionControl.AbsoluteMove(Hats.APDCom.MotionTypes.W,
                    //    256, (int)_MotorWSpeed, (int)_MotorWAccel, (int)_MotorWAccel, (int)_AbsWPos, false, true);
                    if (MotionController.AbsoluteMoveSingleMotion(EthernetCommLib.MotorTypes.W,
                 256, (int)_MotorWSpeed, (int)_MotorWAccel, (int)_MotorWAccel, (int)_AbsWPos, true, false) == false)
                    {
                        MessageBox.Show("Failed to Set new position");
                    }
                }
            }
        }

        public bool CanExecuteGoAbsPosCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region GoRelativePosCommand
        public ICommand RelativePosPlusCommand
        {
            get
            {
                if (_RelativePosPlusCommand == null)
                {
                    _RelativePosPlusCommand = new RelayCommand(ExecuteRelativePosPlusCommand, CanExecuteRelativePosPlusCommand);
                }
                return _RelativePosPlusCommand;
            }
        }

        private void ExecuteRelativePosPlusCommand(object obj)
        {
            if (_IsMotorAlreadyHome == false && !IsNewFirmware)
            {
                return;
            }
            MotorType motorType = (MotorType)obj;

            switch (motorType)
            {
                case MotorType.X:
                    var tgtPos = (CurrentXPos + RelativeXPos) * _XMotorSubdivision;
                    if (tgtPos >= 0 && tgtPos <= _XMaxValue)
                    {
                        if (MotionController.AbsoluteMoveSingleMotion(EthernetCommLib.MotorTypes.X,
                      256, (int)_MotorXSpeed, (int)_MotorXAccel, (int)_MotorXAccel, (int)tgtPos, true, false) == false)
                        {
                            MessageBox.Show("Failed to Set new position");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Out of valid range.");
                    }
                    break;
                case MotorType.Y:
                    tgtPos = (CurrentYPos + RelativeYPos) * _YMotorSubdivision;
                    if (tgtPos >= 0 && tgtPos <= _YMaxValue)
                    {
                        if (MotionController.AbsoluteMoveSingleMotion(EthernetCommLib.MotorTypes.Y,
                      256, (int)_MotorYSpeed, (int)_MotorYAccel, (int)_MotorYAccel, (int)tgtPos, true, false) == false)
                        {
                            MessageBox.Show("Failed to Set new position");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Out of valid range.");
                    }
                    break;
                case MotorType.Z:
                    tgtPos = (CurrentZPos + RelativeZPos) * _ZMotorSubdivision;
                    if (tgtPos >= 0 && tgtPos <= _ZMaxValue)
                    {
                        if (MotionController.AbsoluteMoveSingleMotion(EthernetCommLib.MotorTypes.Z,
                      256, (int)_MotorZSpeed, (int)_MotorZAccel, (int)_MotorZAccel, (int)tgtPos, true, false) == false)
                        {
                            MessageBox.Show("Failed to Set new position");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Out of valid range.");
                    }
                    break;
            }
        }

        private bool CanExecuteRelativePosPlusCommand(object obj)
        {
            return true;
        }

        public ICommand RelativePosMinusCommand
        {
            get
            {
                if (_RelativePosMinusCommand == null)
                {
                    _RelativePosMinusCommand = new RelayCommand(ExecuteRelativePosMinusCommand, CanExecuteRelativePosMinusCommand);
                }
                return _RelativePosMinusCommand;
            }
        }

        private void ExecuteRelativePosMinusCommand(object obj)
        {
            if (_IsMotorAlreadyHome == false && !IsNewFirmware)
            {
                return;
            }
            MotorType motorType = (MotorType)obj;

            switch (motorType)
            {
                case MotorType.X:
                    var tgtPos = (CurrentXPos - RelativeXPos) * _XMotorSubdivision;
                    if (tgtPos >= 0 && tgtPos <= _XMaxValue)
                    {
                        if (MotionController.AbsoluteMoveSingleMotion(EthernetCommLib.MotorTypes.X,
                      256, (int)_MotorXSpeed, (int)_MotorXAccel, (int)_MotorXAccel, (int)tgtPos, true, false) == false)
                        {
                            MessageBox.Show("Failed to Set new position");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Out of valid range.");
                    }
                    break;
                case MotorType.Y:
                    tgtPos = (CurrentYPos - RelativeYPos) * _YMotorSubdivision;
                    if (tgtPos >= 0 && tgtPos <= _YMaxValue)
                    {
                        if (MotionController.AbsoluteMoveSingleMotion(EthernetCommLib.MotorTypes.Y,
                      256, (int)_MotorYSpeed, (int)_MotorYAccel, (int)_MotorYAccel, (int)tgtPos, true, false) == false)
                        {
                            MessageBox.Show("Failed to Set new position");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Out of valid range.");
                    }
                    break;
                case MotorType.Z:
                    tgtPos = (CurrentZPos - RelativeZPos) * _ZMotorSubdivision;
                    if (tgtPos >= 0 && tgtPos <= _ZMaxValue)
                    {
                        if (MotionController.AbsoluteMoveSingleMotion(EthernetCommLib.MotorTypes.Z,
                      256, (int)_MotorZSpeed, (int)_MotorZAccel, (int)_MotorZAccel, (int)tgtPos, true, false) == false)
                        {
                            MessageBox.Show("Failed to Set new position");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Out of valid range.");
                    }
                    break;
            }
        }

        private bool CanExecuteRelativePosMinusCommand(object obj)
        {
            return true;
        }
        #endregion GoRelativePosCommand

        #endregion ICommand...

        public void InitMotorControls()
        {
            _XMotorSubdivision = SettingsManager.ConfigSettings.XMotorSubdivision;
            _YMotorSubdivision = SettingsManager.ConfigSettings.YMotorSubdivision;
            _ZMotorSubdivision = SettingsManager.ConfigSettings.ZMotorSubdivision;
            _WMotorSubdivision = SettingsManager.ConfigSettings.WMotorSubdivision;

            _XMaxValue = SettingsManager.ConfigSettings.XMaxValue;
            _YMaxValue = SettingsManager.ConfigSettings.YMaxValue;
            _ZMaxValue = SettingsManager.ConfigSettings.ZMaxValue;
            _WMaxValue = SettingsManager.ConfigSettings.WMaxValue;

            if (_XMotorSubdivision > 0)
                _LimitsXPlus = Math.Round(_XMaxValue / _XMotorSubdivision, 2); 
            if (_YMotorSubdivision > 0)
                _LimitsYPlus = Math.Round(_YMaxValue / _YMotorSubdivision, 2);
            if (_ZMotorSubdivision > 0)
                _LimitsZPlus = Math.Round(_ZMaxValue / _ZMotorSubdivision, 2);
            if (_WMotorSubdivision > 0)
            {
                _LimitsWMinus = Math.Round(SettingsManager.ConfigSettings.WMinValue / _WMotorSubdivision, 2);
                _LimitsWPlus = Math.Round(_WMaxValue / _WMotorSubdivision, 2);
            }

            // Trigger UI update
            RaisePropertyChanged("XMaxValue");
            RaisePropertyChanged("YMaxValue");
            RaisePropertyChanged("ZMaxValue");
            RaisePropertyChanged("WMaxValue");
            RaisePropertyChanged("LimitsXPlus");
            RaisePropertyChanged("LimitsYPlus");
            RaisePropertyChanged("LimitsZPlus");
            RaisePropertyChanged("LimitsWPlus");

            if (SettingsManager.ConfigSettings.MotorSettings != null && SettingsManager.ConfigSettings.MotorSettings.Count > 0)
            {
                foreach (var motorSetting in SettingsManager.ConfigSettings.MotorSettings)
                {
                    switch (motorSetting.MotorType)
                    {
                        case MotorType.X:
                            _MotorXSpeed = motorSetting.Speed * _XMotorSubdivision;
                            _AbsXPos = motorSetting.Position * _XMotorSubdivision;
                            break;
                        case MotorType.Y:
                            _MotorYSpeed = motorSetting.Speed * _YMotorSubdivision;
                            _AbsYPos = motorSetting.Position * _YMotorSubdivision;
                            break;
                        case MotorType.Z:
                            _MotorZSpeed = motorSetting.Speed * _ZMotorSubdivision;
                            _AbsZPos = motorSetting.Position * _ZMotorSubdivision;
                            break;
                        case MotorType.W:
                            _MotorWSpeed = motorSetting.Speed * _WMotorSubdivision;
                            _AbsWPos = motorSetting.Position * _WMotorSubdivision;
                            break;
                    }
                }
                // Trigger motor speed UI update
                RaisePropertyChanged("AbsXPos");
                RaisePropertyChanged("AbsYPos");
                RaisePropertyChanged("AbsZPos");
                RaisePropertyChanged("AbsWPos");
                RaisePropertyChanged("MotorXSpeed");
                RaisePropertyChanged("MotorYSpeed");
                RaisePropertyChanged("MotorZSpeed");
                RaisePropertyChanged("MotorWSpeed");
            }
        }

        public void UpdateMotorHomed()
        {
            _IsMotorAlreadyHome = true;
        }

        public void InitMotionController()
        {
            _MotionController = new MotionController(Workspace.This.EthernetController);
            _MotionController.AutoQuery = true;
            _MotionController.OnQueryUpdated += _MotionController_OnQueryUpdated;
            MotionSignalPolarity xPolarity = new MotionSignalPolarity()
            {
                ClkPolar = SettingsManager.ConfigSettings.MotionPolarityClkX,
                DirPolar = SettingsManager.ConfigSettings.MotionPolarityDirX,
                EnaPolar = SettingsManager.ConfigSettings.MotionPolarityEnableX,
                HomePolar = SettingsManager.ConfigSettings.MotionPolarityHomeX,
                FwdLmtPolar = SettingsManager.ConfigSettings.MotionPolarityFwdX,
                BwdLmtPolar = SettingsManager.ConfigSettings.MotionPolarityBwdX
            };
            MotionSignalPolarity yPolarity = new MotionSignalPolarity()
            {
                ClkPolar = SettingsManager.ConfigSettings.MotionPolarityClkY,
                DirPolar = SettingsManager.ConfigSettings.MotionPolarityDirY,
                EnaPolar = SettingsManager.ConfigSettings.MotionPolarityEnableY,
                HomePolar = SettingsManager.ConfigSettings.MotionPolarityHomeY,
                FwdLmtPolar = SettingsManager.ConfigSettings.MotionPolarityFwdY,
                BwdLmtPolar = SettingsManager.ConfigSettings.MotionPolarityBwdY
            };
            MotionSignalPolarity zPolarity = new MotionSignalPolarity()
            {
                ClkPolar = SettingsManager.ConfigSettings.MotionPolarityClkZ,
                DirPolar = SettingsManager.ConfigSettings.MotionPolarityDirZ,
                EnaPolar = SettingsManager.ConfigSettings.MotionPolarityEnableZ,
                HomePolar = SettingsManager.ConfigSettings.MotionPolarityHomeZ,
                FwdLmtPolar = SettingsManager.ConfigSettings.MotionPolarityFwdZ,
                BwdLmtPolar = SettingsManager.ConfigSettings.MotionPolarityBwdZ
            };
            MotionSignalPolarity wPolarity = new MotionSignalPolarity()
            {
                ClkPolar = SettingsManager.ConfigSettings.MotionPolarityClkW,
                DirPolar = SettingsManager.ConfigSettings.MotionPolarityDirW,
                EnaPolar = SettingsManager.ConfigSettings.MotionPolarityEnableW,
                HomePolar = SettingsManager.ConfigSettings.MotionPolarityHomeW,
                FwdLmtPolar = SettingsManager.ConfigSettings.MotionPolarityFwdW,
                BwdLmtPolar = SettingsManager.ConfigSettings.MotionPolarityBwdW
            };
            _MotionController.SetMotionPolarities(xPolarity, yPolarity, zPolarity, wPolarity);

            Workspace.This.MotorVM.InitMotorControls();
        
            //MotionController.HomeMotion(Avocado.EthernetCommLib.MotorTypes.X | Avocado.EthernetCommLib.MotorTypes.Y | Avocado.EthernetCommLib.MotorTypes.Z | Avocado.EthernetCommLib.MotorTypes.W,
            //    new int[] { 256, 256, 256, 256 },
            //    new int[] { (int)_MotorXSpeed, (int)_MotorYSpeed, (int)_MotorZSpeed, (int)_MotorWSpeed },
            //    new int[] { (int)_MotorXAccel, (int)_MotorYAccel, (int)_MotorZAccel, (int)_MotorWAccel },
            //    new int[] { (int)_MotorXAccel, (int)_MotorYAccel, (int)_MotorZAccel, (int)_MotorWAccel },
            //    false);
        }

        public bool HomeXYZmotor() {
            try
            {
                if (!_MotionController.CrntState[EthernetCommLib.MotorTypes.X].AtHome)
                {
                    ExecuteHomeCommand(MotorType.X);
                }
                if (!_MotionController.CrntState[EthernetCommLib.MotorTypes.Y].AtHome)
                {
                    ExecuteHomeCommand(MotorType.Y);
                }
                if  (!_MotionController.CrntState[EthernetCommLib.MotorTypes.Z].AtHome)
                {
                    ExecuteHomeCommand(MotorType.Z);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void _MotionController_OnQueryUpdated()
        {
            CurrentXPos = Math.Round(_MotionController.CrntPositions[EthernetCommLib.MotorTypes.X] / SettingsManager.ConfigSettings.XMotorSubdivision, 2);
            CurrentYPos = Math.Round(_MotionController.CrntPositions[EthernetCommLib.MotorTypes.Y] / SettingsManager.ConfigSettings.YMotorSubdivision, 2);
            CurrentZPos = Math.Round(_MotionController.CrntPositions[EthernetCommLib.MotorTypes.Z] / SettingsManager.ConfigSettings.ZMotorSubdivision, 2);
            CurrentWPos = Math.Round(_MotionController.CrntPositions[EthernetCommLib.MotorTypes.W] / SettingsManager.ConfigSettings.WMotorSubdivision, 2);
            UpdateMotorHomed();

            Workspace.This.ApdVM.LIDIsOpen = Workspace.This.EthernetController.LidIsOpen;
        }

        public MotionController MotionController
        {
            get { return _MotionController; }
        }
    }
}
