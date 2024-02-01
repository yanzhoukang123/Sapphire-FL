using Azure.Configuration.Settings;
using Azure.EthernetCommLib;
using Azure.ImagingSystem;
using Azure.WPF.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Azure.ScannerTestJig.ViewModule.TotalMachine
{
    class TotalMacheine532LaserModule : ViewModelBase
    {
        private RelayCommand _GetCurrentLightPowerCommand = null;
        private RelayCommand _GetCurrentLightPowerKpCommand = null;
        private RelayCommand _GetCurrentLightPowerKiCommand = null;
        private RelayCommand _GetCurrentLightPowerKdCommand = null;
        private RelayCommand _SetCurrentLightPowerKpCommand = null;
        private RelayCommand _SetCurrentLightPowerKiCommand = null;
        private RelayCommand _SetCurrentLightPowerKdCommand = null;
        private RelayCommand _GetRadioDiodeVoltageCommand = null;
        private RelayCommand _GetRadioDiodeSlopeCommand = null;
        private RelayCommand _SetRadioDiodeSlopeCommand = null;
        private RelayCommand _GetRadioDiodeConstantCommand = null;
        private RelayCommand _SetRadioDiodeConstantCommand = null;
        private RelayCommand _SetLaserControlVoltageCommand = null;
        private RelayCommand _GetCurrentValuelaserCommand = null;
        private RelayCommand _SetCurrentValuelaserCommand = null;


        private RelayCommand _GetOpticalPowerLessThanOrEqual15mWKpCommand = null;
        private RelayCommand _GetOpticalPowerLessThanOrEqual15mWKiCommand = null;
        private RelayCommand _GetOpticalPowerLessThanOrEqual15mWKdCommand = null;
        private RelayCommand _GetOpticalPowerGreaterThan15mWKpCommand = null;
        private RelayCommand _GetOpticalPowerGreaterThan15mWKiCommand = null;
        private RelayCommand _GetOpticalPowerGreaterThan15mWKdCommand = null;
        private RelayCommand _GetOpticalPowerControlKpUpperLimitLessThanOrEqual15Command = null;
        private RelayCommand _GetOpticalPowerControlKpDownLimitLessThanOrEqual15Command = null;
        private RelayCommand _GetOpticalPowerControlKiUpperLimitLessThanOrEqual15Command = null;
        private RelayCommand _GetOpticalPowerControlKiDownLimitLessThanOrEqual15Command = null;
        private RelayCommand _GetOpticalPowerControlKpUpperLimitLessThan15Command = null;
        private RelayCommand _GetOpticalPowerControlKpDownLimitLessThan15Command = null;
        private RelayCommand _GetOpticalPowerControlKiUpperLimitLessThan15Command = null;
        private RelayCommand _GetOpticalPowerControlKiDownLimitLessThan15Command = null;
        private RelayCommand _GetLaserMaxCurrentCommand = null;
        private RelayCommand _GetLaserMinCurrentCommand = null;
        private RelayCommand _GetTECkpCommand = null;
        private RelayCommand _GetTECkiCommand = null;
        private RelayCommand _GetTECkdCommand = null;

        private RelayCommand _SetOpticalPowerLessThanOrEqual15mWKpCommand = null;
        private RelayCommand _SetOpticalPowerLessThanOrEqual15mWKiCommand = null;
        private RelayCommand _SetOpticalPowerLessThanOrEqual15mWKdCommand = null;
        private RelayCommand _SetOpticalPowerGreaterThan15mWKpCommand = null;
        private RelayCommand _SetOpticalPowerGreaterThan15mWKiCommand = null;
        private RelayCommand _SetOpticalPowerGreaterThan15mWKdCommand = null;
        private RelayCommand _SetOpticalPowerControlKpUpperLimitLessThanOrEqual15Command = null;
        private RelayCommand _SetOpticalPowerControlKpDownLimitLessThanOrEqual15Command = null;
        private RelayCommand _SetOpticalPowerControlKiUpperLimitLessThanOrEqual15Command = null;
        private RelayCommand _SetOpticalPowerControlKiDownLimitLessThanOrEqual15Command = null;
        private RelayCommand _SetOpticalPowerControlKpUpperLimitLessThan15Command = null;
        private RelayCommand _SetOpticalPowerControlKpDownLimitLessThan15Command = null;
        private RelayCommand _SetOpticalPowerControlKiUpperLimitLessThan15Command = null;
        private RelayCommand _SetOpticalPowerControlKiDownLimitLessThan15Command = null;
        private RelayCommand _SetLaserMaxCurrentCommand = null;
        private RelayCommand _SetLaserMinCurrentCommand = null;
        private RelayCommand _SetTECkpCommand = null;
        private RelayCommand _SetTECkiCommand = null;
        private RelayCommand _SetTECkdCommand = null;


        private double _SetOpticalPowerLessThanOrEqual15mWKp;
        private double _SetOpticalPowerLessThanOrEqual15mWKi;
        private double _SetOpticalPowerLessThanOrEqual15mWKd;
        private double _SetOpticalPowerGreaterThan15mWKp;
        private double _SetOpticalPowerGreaterThan15mWKi;
        private double _SetOpticalPowerGreaterThan15mWKd;
        private double _SetOpticalPowerControlKpUpperLimitLessThanOrEqual15;
        private double _SetOpticalPowerControlKpDownLimitLessThanOrEqual15;
        private double _SetOpticalPowerControlKiUpperLimitLessThanOrEqual15;
        private double _SetOpticalPowerControlKiDownLimitLessThanOrEqual15;
        private double _SetOpticalPowerControlKpUpperLimitLessThan15;
        private double _SetOpticalPowerControlKpDownLimitLessThan15;
        private double _SetOpticalPowerControlKiUpperLimitLessThan15;
        private double _SetOpticalPowerControlKiDownLimitLessThan15;
        private double _SetLaserMaxCurrent;
        private double _SetLaserMinCurrent;
        private double _SetTECkp;
        private double _SetTECki;
        private double _SetTECkd;

        private double _GetOpticalPowerLessThanOrEqual15mWKp;
        private double _GetOpticalPowerLessThanOrEqual15mWKi;
        private double _GetOpticalPowerLessThanOrEqual15mWKd;
        private double _GetOpticalPowerGreaterThan15mWKp;
        private double _GetOpticalPowerGreaterThan15mWKi;
        private double _GetOpticalPowerGreaterThan15mWKd;
        private double _GetOpticalPowerControlKpUpperLimitLessThanOrEqual15;
        private double _GetOpticalPowerControlKpDownLimitLessThanOrEqual15;
        private double _GetOpticalPowerControlKiUpperLimitLessThanOrEqual15;
        private double _GetOpticalPowerControlKiDownLimitLessThanOrEqual15;
        private double _GetOpticalPowerControlKpUpperLimitLessThan15;
        private double _GetOpticalPowerControlKpDownLimitLessThan15;
        private double _GetOpticalPowerControlKiUpperLimitLessThan15;
        private double _GetOpticalPowerControlKiDownLimitLessThan15;
        private double _GetLaserMaxCurrent;
        private double _GetLaserMinCurrent;
        private double _GetTECkp;
        private double _GetTECki;
        private double _GetTECkd;




        private LaserPower _SelectedLaserControlVoltageModule = null;
        private ObservableCollection<LaserPower> _PowerOptions = null;
        private EthernetController _EthernetController;
        public TotalMacheine532LaserModule(EthernetController ethernetController)
        {
            _EthernetController = ethernetController;
        }
        public EthernetController EthernetDevice
        {
            get
            {
                return _EthernetController;
            }
        }
        public void InitIVControls()
        {
            _PowerOptions = SettingsManager.ConfigSettings.LaserPowers;
            RaisePropertyChanged("LaserControlVoltageModule");
            if (_PowerOptions != null && _PowerOptions.Count >= 3)
            {
                SelectedLaserControlVoltageModule = _PowerOptions[0];
            }
        }
        public ObservableCollection<LaserPower> LaserControlVoltageModule
        {
            get { return _PowerOptions; }
        }

        #region 当前光功率
        public ICommand GetCurrentLightPowerCommand
        {
            get
            {
                if (_GetCurrentLightPowerCommand == null)
                {
                    _GetCurrentLightPowerCommand = new RelayCommand(Execute_GetCurrentLightPowerCommand, CanExecut_GetCurrentLightPowerCommand);
                }
                return _GetCurrentLightPowerCommand;
            }
        }
        public void Execute_GetCurrentLightPowerCommand(object parameter)
        {
            if (!Workspace.This.MotorVM.IsNewFirmware)
            {
                MessageBox.Show("网络未连接！", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "L")
            {
                EthernetDevice.GetCurrentLightPower(LaserChannels.ChannelC);
                Thread.Sleep(100);
                EthernetDevice.GetCurrentLightPower(LaserChannels.ChannelC);
                GetCurrentLightPower = (int)EthernetDevice.AllCurrentLightPower[LaserChannels.ChannelC];
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R1")
            {
                EthernetDevice.GetCurrentLightPower(LaserChannels.ChannelA);
                Thread.Sleep(100);
                EthernetDevice.GetCurrentLightPower(LaserChannels.ChannelA);
                GetCurrentLightPower = (int)EthernetDevice.AllCurrentLightPower[LaserChannels.ChannelA];
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R2")
            {
                EthernetDevice.GetCurrentLightPower(LaserChannels.ChannelB);
                Thread.Sleep(100);
                EthernetDevice.GetCurrentLightPower(LaserChannels.ChannelB);
                GetCurrentLightPower = (int)EthernetDevice.AllCurrentLightPower[LaserChannels.ChannelB];
            }

        }
        public bool CanExecut_GetCurrentLightPowerCommand(object parameter)
        {
            return true;
        }

        private int _GetCurrentLightPower = 0;
        public int GetCurrentLightPower
        {
            get { return _GetCurrentLightPower; }
            set
            {
                if (_GetCurrentLightPower != value)
                {
                    if (value == Workspace.This.Uint32Code)
                    {
                        value = 0;
                    }
                    _GetCurrentLightPower = value;
                    RaisePropertyChanged("GetCurrentLightPower");
                }
            }
        }

        #endregion

        #region 光功率控制Kp

        #region 读取
        public ICommand GetLightPowerKpCommand
        {
            get
            {
                if (_GetCurrentLightPowerKpCommand == null)
                {
                    _GetCurrentLightPowerKpCommand = new RelayCommand(Execute_GetCurrentLightPowerKpCommand, CanExecut_GetCurrentLightPowerKpCommand);
                }
                return _GetCurrentLightPowerKpCommand;
            }
        }
        public void Execute_GetCurrentLightPowerKpCommand(object parameter)
        {
            if (!Workspace.This.MotorVM.IsNewFirmware)
            {
                MessageBox.Show("网络未连接！", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "L")
            {
                EthernetDevice.GetOpticalPowerGreaterThan15mWKp(LaserChannels.ChannelC);
                Thread.Sleep(100);
                EthernetDevice.GetOpticalPowerGreaterThan15mWKp(LaserChannels.ChannelC);
                GetLightPowerKp = (int)EthernetController.AllOpticalPowerGreaterThan15mWKp[LaserChannels.ChannelC];
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R1")
            {
                EthernetDevice.GetOpticalPowerGreaterThan15mWKp(LaserChannels.ChannelA);
                Thread.Sleep(100);
                EthernetDevice.GetOpticalPowerGreaterThan15mWKp(LaserChannels.ChannelA);
                GetLightPowerKp = (int)EthernetController.AllOpticalPowerGreaterThan15mWKp[LaserChannels.ChannelA];
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R2")
            {
                EthernetDevice.GetOpticalPowerGreaterThan15mWKp(LaserChannels.ChannelB);
                Thread.Sleep(100);
                EthernetDevice.GetOpticalPowerGreaterThan15mWKp(LaserChannels.ChannelB);
                GetLightPowerKp = (int)EthernetController.AllOpticalPowerGreaterThan15mWKp[LaserChannels.ChannelB];
            }

        }
        public bool CanExecut_GetCurrentLightPowerKpCommand(object parameter)
        {
            return true;
        }
        private int _GetLightPowerKp = 0;
        public int GetLightPowerKp
        {
            get { return _GetLightPowerKp; }
            set
            {

                if (_GetLightPowerKp != value)
                {
                    if (value == Workspace.This.Uint16Code)
                    {
                        value = 0;
                    }
                    _GetLightPowerKp = value;
                    RaisePropertyChanged("GetLightPowerKp");
                }
            }
        }
        #endregion

        #region 写入
        public ICommand SetLightPowerKpCommand
        {
            get
            {
                if (_SetCurrentLightPowerKpCommand == null)
                {
                    _SetCurrentLightPowerKpCommand = new RelayCommand(Execute_SetCurrentLightPowerKpCommand, CanExecut_SetCurrentLightPowerKpCommand);
                }
                return _SetCurrentLightPowerKpCommand;
            }
        }
        public void Execute_SetCurrentLightPowerKpCommand(object parameter)
        {
            if (!Workspace.This.MotorVM.IsNewFirmware)
            {
                MessageBox.Show("网络未连接！", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "L")
            {
                EthernetDevice.SetOpticalPowerGreaterThan15mWKp(LaserChannels.ChannelC, SetLaserLightPowerKp);
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R1")
            {
                EthernetDevice.SetOpticalPowerGreaterThan15mWKp(LaserChannels.ChannelA, SetLaserLightPowerKp);

            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R2")
            {
                EthernetDevice.SetOpticalPowerGreaterThan15mWKp(LaserChannels.ChannelB, SetLaserLightPowerKp);
            }

        }
        public bool CanExecut_SetCurrentLightPowerKpCommand(object parameter)
        {
            return true;
        }
        private int _SetLaserLightPowerKp = 0;
        public int SetLaserLightPowerKp
        {
            get { return _SetLaserLightPowerKp; }
            set
            {

                if (_SetLaserLightPowerKp != value)
                {
                    _SetLaserLightPowerKp = value;
                    RaisePropertyChanged("SetLaserLightPowerKp");
                }
            }
        }

        #endregion

        #endregion

        #region 光功率控制Ki

        #region 读取
        public ICommand GetLightPowerKiCommand
        {
            get
            {
                if (_GetCurrentLightPowerKiCommand == null)
                {
                    _GetCurrentLightPowerKiCommand = new RelayCommand(Execute_GetCurrentLightPowerKiCommand, CanExecut_GetCurrentLightPowerKiCommand);
                }
                return _GetCurrentLightPowerKiCommand;
            }
        }
        public void Execute_GetCurrentLightPowerKiCommand(object parameter)
        {
            if (!Workspace.This.MotorVM.IsNewFirmware)
            {
                MessageBox.Show("网络未连接！", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "L")
            {
                EthernetDevice.GetOpticalPowerGreaterThan15mWKi(LaserChannels.ChannelC);
                Thread.Sleep(100);
                EthernetDevice.GetOpticalPowerGreaterThan15mWKi(LaserChannels.ChannelC);
                GetLightPowerKi = (int)EthernetController.AllOpticalPowerGreaterThan15mWKi[LaserChannels.ChannelC];
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R1")
            {
                EthernetDevice.GetOpticalPowerGreaterThan15mWKi(LaserChannels.ChannelA);
                Thread.Sleep(100);
                EthernetDevice.GetOpticalPowerGreaterThan15mWKi(LaserChannels.ChannelA);
                GetLightPowerKi = (int)EthernetController.AllOpticalPowerGreaterThan15mWKi[LaserChannels.ChannelA];
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R2")
            {
                EthernetDevice.GetOpticalPowerGreaterThan15mWKi(LaserChannels.ChannelB);
                Thread.Sleep(100);
                EthernetDevice.GetOpticalPowerGreaterThan15mWKi(LaserChannels.ChannelB);
                GetLightPowerKi = (int)EthernetController.AllOpticalPowerGreaterThan15mWKi[LaserChannels.ChannelB];
            }

        }
        public bool CanExecut_GetCurrentLightPowerKiCommand(object parameter)
        {
            return true;
        }
        private int _GetLightPowerKi = 0;
        public int GetLightPowerKi
        {
            get { return _GetLightPowerKi; }
            set
            {

                if (_GetLightPowerKi != value)
                {
                    if (value == Workspace.This.Uint16Code)
                    {
                        value = 0;
                    }
                    _GetLightPowerKi = value;
                    RaisePropertyChanged("GetLightPowerKi");
                }
            }
        }
        #endregion

        #region 写入
        public ICommand SetLightPowerKiCommand
        {
            get
            {
                if (_SetCurrentLightPowerKiCommand == null)
                {
                    _SetCurrentLightPowerKiCommand = new RelayCommand(Execute_SetCurrentLightPowerKiCommand, CanExecut_SetCurrentLightPowerKiCommand);
                }
                return _SetCurrentLightPowerKiCommand;
            }
        }
        public void Execute_SetCurrentLightPowerKiCommand(object parameter)
        {
            if (!Workspace.This.MotorVM.IsNewFirmware)
            {
                MessageBox.Show("网络未连接！", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "L")
            {
                EthernetDevice.SetOpticalPowerGreaterThan15mWKi(LaserChannels.ChannelC, SetLaserLightPowerKi);
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R1")
            {
                EthernetDevice.SetOpticalPowerGreaterThan15mWKi(LaserChannels.ChannelA, SetLaserLightPowerKi);
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R2")
            {
                EthernetDevice.SetOpticalPowerGreaterThan15mWKi(LaserChannels.ChannelB, SetLaserLightPowerKi);
            }

        }
        public bool CanExecut_SetCurrentLightPowerKiCommand(object parameter)
        {
            return true;
        }
        private int _SetLaserLightPowerKi = 0;
        public int SetLaserLightPowerKi
        {
            get { return _SetLaserLightPowerKi; }
            set
            {

                if (_SetLaserLightPowerKi != value)
                {
                    _SetLaserLightPowerKi = value;
                    RaisePropertyChanged("SetLaserLightPowerKi");
                }
            }
        }
        #endregion
        #endregion

        #region 光功率控制Kd

        #region 读取
        public ICommand GetLightPowerKdCommand
        {
            get
            {
                if (_GetCurrentLightPowerKdCommand == null)
                {
                    _GetCurrentLightPowerKdCommand = new RelayCommand(Execute_GetCurrentLightPowerKdCommand, CanExecut_GetCurrentLightPowerKdCommand);
                }
                return _GetCurrentLightPowerKdCommand;
            }
        }
        public void Execute_GetCurrentLightPowerKdCommand(object parameter)
        {
            if (!Workspace.This.MotorVM.IsNewFirmware)
            {
                MessageBox.Show("网络未连接！", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "L")
            {
                EthernetDevice.GetAllOpticalPowerGreaterThan15mWKd(LaserChannels.ChannelC);
                Thread.Sleep(100);
                EthernetDevice.GetAllOpticalPowerGreaterThan15mWKd(LaserChannels.ChannelC);
                GetLightPowerKd = (int)EthernetController.AllOpticalPowerGreaterThan15mWKd[LaserChannels.ChannelC];
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R1")
            {
                EthernetDevice.GetAllOpticalPowerGreaterThan15mWKd(LaserChannels.ChannelA);
                Thread.Sleep(100);
                EthernetDevice.GetAllOpticalPowerGreaterThan15mWKd(LaserChannels.ChannelA);
                GetLightPowerKd = (int)EthernetController.AllOpticalPowerGreaterThan15mWKd[LaserChannels.ChannelA];
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R2")
            {
                EthernetDevice.GetAllOpticalPowerGreaterThan15mWKd(LaserChannels.ChannelB);
                Thread.Sleep(100);
                EthernetDevice.GetAllOpticalPowerGreaterThan15mWKd(LaserChannels.ChannelB);
                GetLightPowerKd = (int)EthernetController.AllOpticalPowerGreaterThan15mWKd[LaserChannels.ChannelB];
            }

        }
        public bool CanExecut_GetCurrentLightPowerKdCommand(object parameter)
        {
            return true;
        }
        private int _GetLightPowerKd = 0;
        public int GetLightPowerKd
        {
            get { return _GetLightPowerKd; }
            set
            {

                if (_GetLightPowerKd != value)
                {
                    if (value == Workspace.This.Uint16Code)
                    {
                        value = 0;
                    }
                    _GetLightPowerKd = value;
                    RaisePropertyChanged("GetLightPowerKd");
                }
            }
        }
        #endregion

        #region 写入
        public ICommand SetLightPowerKdCommand
        {
            get
            {
                if (_SetCurrentLightPowerKdCommand == null)
                {
                    _SetCurrentLightPowerKdCommand = new RelayCommand(Execute_SetCurrentLightPowerKdCommand, CanExecut_SetCurrentLightPowerKdCommand);
                }
                return _SetCurrentLightPowerKdCommand;
            }
        }
        public void Execute_SetCurrentLightPowerKdCommand(object parameter)
        {
            if (!Workspace.This.MotorVM.IsNewFirmware)
            {
                MessageBox.Show("网络未连接！", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "L")
            {
                EthernetDevice.SetOpticalPowerGreaterThan15mWKd(LaserChannels.ChannelC, SetLaserLightPowerKd);
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R1")
            {
                EthernetDevice.SetOpticalPowerGreaterThan15mWKd(LaserChannels.ChannelA, SetLaserLightPowerKd);
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R2")
            {
                EthernetDevice.SetOpticalPowerGreaterThan15mWKd(LaserChannels.ChannelB, SetLaserLightPowerKd);
            }

        }
        public bool CanExecut_SetCurrentLightPowerKdCommand(object parameter)
        {
            return true;
        }
        private int _SetLaserLightPowerKd = 0;
        public int SetLaserLightPowerKd
        {
            get { return _SetLaserLightPowerKd; }
            set
            {

                if (_SetLaserLightPowerKd != value)
                {
                    _SetLaserLightPowerKd = value;
                    RaisePropertyChanged("SetLaserLightPowerKd");
                }
            }
        }
        #endregion
        #endregion

        #region 光功率控制电压

        #region 读取
        private int _GetLaserControlVoltage = 0;
        public int GetLaserControlVoltage
        {
            get { return _GetLaserControlVoltage; }
            set
            {

                if (_GetLaserControlVoltage != value)
                {
                    if (value == Workspace.This.Uint32Code)
                    {
                        value = 0;
                    }
                    _GetLaserControlVoltage = value;
                    RaisePropertyChanged("GetLaserControlVoltage");
                }
            }
        }
        public LaserPower SelectedLaserControlVoltageModule
        {
            get { return _SelectedLaserControlVoltageModule; }
            set
            {
                if (_SelectedLaserControlVoltageModule != value)
                {
                    _SelectedLaserControlVoltageModule = value;
                    RaisePropertyChanged("SelectedLaserControlVoltageModule");
                    if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "L")
                    {
                        EthernetDevice.GetLaserControlVoltageCurrent(LaserChannels.ChannelC, SelectedLaserControlVoltageModule.Value);
                        Thread.Sleep(100);
                        EthernetDevice.GetLaserControlVoltageCurrent(LaserChannels.ChannelC, SelectedLaserControlVoltageModule.Value);
                        Thread.Sleep(100);
                        try
                        {
                            GetLaserControlVoltage = Convert.ToInt32(EthernetDevice.AllOpticalPowerControlvoltage[LaserChannels.ChannelC] * 10000);
                        }
                        catch { }
                    }
                    if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R1")
                    {
                        EthernetDevice.GetLaserControlVoltageCurrent(LaserChannels.ChannelA, SelectedLaserControlVoltageModule.Value);
                        Thread.Sleep(100);
                        EthernetDevice.GetLaserControlVoltageCurrent(LaserChannels.ChannelA, SelectedLaserControlVoltageModule.Value);
                        Thread.Sleep(100);
                        try
                        {
                            GetLaserControlVoltage = Convert.ToInt32(EthernetDevice.AllOpticalPowerControlvoltage[LaserChannels.ChannelA] * 10000);
                        }
                        catch { }
                    }
                    if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R2")
                    {
                        EthernetDevice.GetLaserControlVoltageCurrent(LaserChannels.ChannelB, SelectedLaserControlVoltageModule.Value);
                        Thread.Sleep(100);
                        EthernetDevice.GetLaserControlVoltageCurrent(LaserChannels.ChannelB, SelectedLaserControlVoltageModule.Value);
                        Thread.Sleep(100);
                        try
                        {
                            GetLaserControlVoltage = Convert.ToInt32(EthernetDevice.AllOpticalPowerControlvoltage[LaserChannels.ChannelB] * 10000);
                        }
                        catch { }
                    }

                }
            }
        }

        #endregion

        #region 写入
        public ICommand SetLaserControlVoltageCommand
        {
            get
            {
                if (_SetLaserControlVoltageCommand == null)
                {
                    _SetLaserControlVoltageCommand = new RelayCommand(Execute___SetLaserControlVoltageCommand, CanExecut___SetLaserControlVoltageCommand);
                }
                return _SetLaserControlVoltageCommand;
            }
        }
        public void Execute___SetLaserControlVoltageCommand(object parameter)
        {
            if (!Workspace.This.MotorVM.IsNewFirmware)
            {
                MessageBox.Show("网络未连接！", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "L")
            {
                EthernetDevice.SetLaserControlVoltageCurrent(LaserChannels.ChannelC, SelectedLaserControlVoltageModule.Value, SetLaserControlVoltage);
                Thread.Sleep(100);
                EthernetDevice.GetLaserControlVoltageCurrent(LaserChannels.ChannelC, SelectedLaserControlVoltageModule.Value);
                Thread.Sleep(100);
                EthernetDevice.GetLaserControlVoltageCurrent(LaserChannels.ChannelC, SelectedLaserControlVoltageModule.Value);
                Thread.Sleep(100);
                GetLaserControlVoltage = Convert.ToInt32(EthernetDevice.AllOpticalPowerControlvoltage[LaserChannels.ChannelC]*10000);
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R1")
            {
                EthernetDevice.SetLaserControlVoltageCurrent(LaserChannels.ChannelA, SelectedLaserControlVoltageModule.Value, SetLaserControlVoltage);
                Thread.Sleep(100);
                EthernetDevice.GetLaserControlVoltageCurrent(LaserChannels.ChannelA, SelectedLaserControlVoltageModule.Value);
                Thread.Sleep(100);
                EthernetDevice.GetLaserControlVoltageCurrent(LaserChannels.ChannelA, SelectedLaserControlVoltageModule.Value);
                Thread.Sleep(100);
                GetLaserControlVoltage = Convert.ToInt32(EthernetDevice.AllOpticalPowerControlvoltage[LaserChannels.ChannelA]*10000);
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R2")
            {
                EthernetDevice.SetLaserControlVoltageCurrent(LaserChannels.ChannelB, SelectedLaserControlVoltageModule.Value, SetLaserControlVoltage);
                Thread.Sleep(100);
                EthernetDevice.GetLaserControlVoltageCurrent(LaserChannels.ChannelB, SelectedLaserControlVoltageModule.Value);
                Thread.Sleep(100);
                EthernetDevice.GetLaserControlVoltageCurrent(LaserChannels.ChannelB, SelectedLaserControlVoltageModule.Value);
                Thread.Sleep(100);
                GetLaserControlVoltage =Convert.ToInt32(EthernetDevice.AllOpticalPowerControlvoltage[LaserChannels.ChannelB]*10000);
            }

        }
        public bool CanExecut___SetLaserControlVoltageCommand(object parameter)
        {
            return true;
        }

        private int _SetLaserControlVoltage = 0;
        public int SetLaserControlVoltage
        {
            get { return _SetLaserControlVoltage; }
            set
            {

                if (_SetLaserControlVoltage != value)
                {
                    _SetLaserControlVoltage = value;
                    RaisePropertyChanged("SetLaserControlVoltage");
                }
            }
        }
        #endregion
        #endregion

        #region 光电二极管电压
        public ICommand GetRadioDiodeVoltageCommand
        {
            get
            {
                if (_GetRadioDiodeVoltageCommand == null)
                {
                    _GetRadioDiodeVoltageCommand = new RelayCommand(Execute__GetRadioDiodeVoltageCommandCommand, CanExecut__GetRadioDiodeVoltageCommandCommand);
                }
                return _GetRadioDiodeVoltageCommand;
            }
        }
        public void Execute__GetRadioDiodeVoltageCommandCommand(object parameter)
        {
            if (!Workspace.This.MotorVM.IsNewFirmware)
            {
                MessageBox.Show("网络未连接！", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "L")
            {
                EthernetDevice.GetRadioDiodeVoltage(LaserChannels.ChannelC);
                Thread.Sleep(100);
                EthernetDevice.GetRadioDiodeVoltage(LaserChannels.ChannelC);
                GetRadioDiodeVoltage = (int)EthernetDevice.AllRadioDiodeVoltage[LaserChannels.ChannelC];
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R1")
            {
                EthernetDevice.GetRadioDiodeVoltage(LaserChannels.ChannelA);
                Thread.Sleep(100);
                EthernetDevice.GetRadioDiodeVoltage(LaserChannels.ChannelA);
                GetRadioDiodeVoltage = (int)EthernetDevice.AllRadioDiodeVoltage[LaserChannels.ChannelA];
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R2")
            {
                EthernetDevice.GetRadioDiodeVoltage(LaserChannels.ChannelB);
                Thread.Sleep(100);
                EthernetDevice.GetRadioDiodeVoltage(LaserChannels.ChannelB);
                GetRadioDiodeVoltage = (int)EthernetDevice.AllRadioDiodeVoltage[LaserChannels.ChannelB];
            }

        }
        public bool CanExecut__GetRadioDiodeVoltageCommandCommand(object parameter)
        {
            return true;
        }

        private int _GetRadioDiodeVoltage = 0;
        public int GetRadioDiodeVoltage
        {
            get { return _GetRadioDiodeVoltage; }
            set
            {

                if (_GetRadioDiodeVoltage != value)
                {
                    if (value == Workspace.This.Uint32Code)
                    {
                        value = 0;
                    }
                    _GetRadioDiodeVoltage = value;
                    RaisePropertyChanged("GetRadioDiodeVoltage");
                }
            }
        }
        #endregion

        #region 光电二极管校准斜率

        #region 读取
        public ICommand GetRadioDiodeSlopeCommand
        {
            get
            {
                if (_GetRadioDiodeSlopeCommand == null)
                {
                    _GetRadioDiodeSlopeCommand = new RelayCommand(Execute__GetRadioDiodeSlopeCommand, CanExecut_GetRadioDiodeSlopeCommand);
                }
                return _GetRadioDiodeSlopeCommand;
            }
        }
        public void Execute__GetRadioDiodeSlopeCommand(object parameter)
        {
            if (!Workspace.This.MotorVM.IsNewFirmware)
            {
                MessageBox.Show("网络未连接！", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "L")
            {
                EthernetDevice.GetRadioDiodeSlope(LaserChannels.ChannelC);
                Thread.Sleep(100);
                EthernetDevice.GetRadioDiodeSlope(LaserChannels.ChannelC);
                GetRadioDiodeSlope = (int)EthernetDevice.AllGetRadioDiodeSlope[LaserChannels.ChannelC];
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R1")
            {
                EthernetDevice.GetRadioDiodeSlope(LaserChannels.ChannelA);
                Thread.Sleep(100);
                EthernetDevice.GetRadioDiodeSlope(LaserChannels.ChannelA);
                GetRadioDiodeSlope = (int)EthernetDevice.AllGetRadioDiodeSlope[LaserChannels.ChannelA];
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R2")
            {
                EthernetDevice.GetRadioDiodeSlope(LaserChannels.ChannelB);
                Thread.Sleep(100);
                EthernetDevice.GetRadioDiodeSlope(LaserChannels.ChannelB);
                GetRadioDiodeSlope = (int)EthernetDevice.AllGetRadioDiodeSlope[LaserChannels.ChannelB];
            }

        }
        public bool CanExecut_GetRadioDiodeSlopeCommand(object parameter)
        {
            return true;
        }
        private int _GetRadioDiodeSlope = 0;
        public int GetRadioDiodeSlope
        {
            get { return _GetRadioDiodeSlope; }
            set
            {

                if (_GetRadioDiodeSlope != value)
                {
                    if (value == Workspace.This.Uint32Code)
                    {
                        value = 0;
                    }
                    _GetRadioDiodeSlope = value;
                    RaisePropertyChanged("GetRadioDiodeSlope");
                }
            }
        }
        #endregion

        #region 写入
        public ICommand SetRadioDiodeSlopeCommand
        {
            get
            {
                if (_SetRadioDiodeSlopeCommand == null)
                {
                    _SetRadioDiodeSlopeCommand = new RelayCommand(Execute__SetRadioDiodeSlopeCommand, CanExecut_SetRadioDiodeSlopeCommand);
                }
                return _SetRadioDiodeSlopeCommand;
            }
        }
        public void Execute__SetRadioDiodeSlopeCommand(object parameter)
        {
            if (!Workspace.This.MotorVM.IsNewFirmware)
            {
                MessageBox.Show("网络未连接！", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "L")
            {
                EthernetDevice.SetRadioDiodeSlope(LaserChannels.ChannelC, SetRadioDiodeSlope);
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R1")
            {
                EthernetDevice.SetRadioDiodeSlope(LaserChannels.ChannelA, SetRadioDiodeSlope);
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R2")
            {
                EthernetDevice.SetRadioDiodeSlope(LaserChannels.ChannelB, SetRadioDiodeSlope);
            }

        }
        public bool CanExecut_SetRadioDiodeSlopeCommand(object parameter)
        {
            return true;
        }
        private int _SetRadioDiodeSlope = 0;
        public int SetRadioDiodeSlope
        {
            get { return _SetRadioDiodeSlope; }
            set
            {

                if (_SetRadioDiodeSlope != value)
                {
                    _SetRadioDiodeSlope = value;
                    RaisePropertyChanged("SetRadioDiodeSlope");
                }
            }
        }
        #endregion

        #endregion

        #region 光电二极管校准常数

        #region 读取
        public ICommand GetRadioDiodeConstantCommand
        {
            get
            {
                if (_GetRadioDiodeConstantCommand == null)
                {
                    _GetRadioDiodeConstantCommand = new RelayCommand(Execute__GetRadioDiodeConstantCommand, CanExecut_GetRadioDiodeConstantCommand);
                }
                return _GetRadioDiodeConstantCommand;
            }
        }
        public void Execute__GetRadioDiodeConstantCommand(object parameter)
        {
            if (!Workspace.This.MotorVM.IsNewFirmware)
            {
                MessageBox.Show("网络未连接！", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "L")
            {
                EthernetDevice.GetRadioDiodeConstant(LaserChannels.ChannelC);
                Thread.Sleep(100);
                EthernetDevice.GetRadioDiodeConstant(LaserChannels.ChannelC);
                GetRadioDiodeConstant = (int)EthernetDevice.AllGetRadioAndTelevisionDiodeCalibrationConstant[LaserChannels.ChannelC];
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R1")
            {
                EthernetDevice.GetRadioDiodeConstant(LaserChannels.ChannelA);
                Thread.Sleep(100);
                EthernetDevice.GetRadioDiodeConstant(LaserChannels.ChannelA);
                GetRadioDiodeConstant = (int)EthernetDevice.AllGetRadioAndTelevisionDiodeCalibrationConstant[LaserChannels.ChannelA];
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R2")
            {
                EthernetDevice.GetRadioDiodeConstant(LaserChannels.ChannelB);
                Thread.Sleep(100);
                EthernetDevice.GetRadioDiodeConstant(LaserChannels.ChannelB);
                GetRadioDiodeConstant = (int)EthernetDevice.AllGetRadioAndTelevisionDiodeCalibrationConstant[LaserChannels.ChannelB];
            }

        }
        public bool CanExecut_GetRadioDiodeConstantCommand(object parameter)
        {
            return true;
        }
        private int _GetRadioDiodeConstant = 0;
        public int GetRadioDiodeConstant
        {
            get { return _GetRadioDiodeConstant; }
            set
            {

                if (_GetRadioDiodeConstant != value)
                {
                    if (value == Workspace.This.Uint32Code)
                    {
                        value = 0;
                    }
                    _GetRadioDiodeConstant = value;
                    RaisePropertyChanged("GetRadioDiodeConstant");
                }
            }
        }
        #endregion

        #region 写入
        public ICommand SetRadioDiodeConstantCommand
        {
            get
            {
                if (_SetRadioDiodeConstantCommand == null)
                {
                    _SetRadioDiodeConstantCommand = new RelayCommand(Execute_SetRadioDiodeConstantCommand, CanExecut_SetRadioDiodeConstantCommand);
                }
                return _SetRadioDiodeConstantCommand;
            }
        }
        public void Execute_SetRadioDiodeConstantCommand(object parameter)
        {
            if (!Workspace.This.MotorVM.IsNewFirmware)
            {
                MessageBox.Show("网络未连接！", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "L")
            {
                EthernetDevice.SetRadioDiodeConstant(LaserChannels.ChannelC, SetRadioDiodeConstant);
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R1")
            {
                EthernetDevice.SetRadioDiodeConstant(LaserChannels.ChannelA, SetRadioDiodeConstant);
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R2")
            {
                EthernetDevice.SetRadioDiodeConstant(LaserChannels.ChannelB, SetRadioDiodeConstant);
            }

        }
        public bool CanExecut_SetRadioDiodeConstantCommand(object parameter)
        {
            return true;
        }
        private int _SetRadioDiodeConstant = 0;
        public int SetRadioDiodeConstant
        {
            get { return _SetRadioDiodeConstant; }
            set
            {

                if (_SetRadioDiodeConstant != value)
                {
                    _SetRadioDiodeConstant = value;
                    RaisePropertyChanged("SetRadioDiodeConstant");
                }
            }
        }
        #endregion

        #endregion

        #region IsLaserSelected
        private bool _IsLaserSelected = false;
        public bool IsLaserSelected
        {
            get { return _IsLaserSelected; }
            set
            {
                if (_IsLaserSelected != value)
                {
                    _IsLaserSelected = value;
                    RaisePropertyChanged("_IsLaserSelected");
                    if (value == true)
                    {
                        if (Workspace.This.MotorVM.IsNewFirmware)
                        {
                            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "L")
                            {
                                EthernetDevice.SetLaserPower(LaserChannels.ChannelC, SelectedLaserControlVoltageModule.Value);
                            }
                            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R1")
                            {
                                EthernetDevice.SetLaserPower(LaserChannels.ChannelA, SelectedLaserControlVoltageModule.Value);
                            }
                            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R2")
                            {
                                EthernetDevice.SetLaserPower(LaserChannels.ChannelB, SelectedLaserControlVoltageModule.Value);
                            }
                        }
                        else
                        {
                            MessageBox.Show("网络未连接！", "", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;

                        }
                    }
                    else
                    {
                        if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "L")
                            EthernetDevice.SetLaserPower(LaserChannels.ChannelC, 0);
                        if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R1")
                            EthernetDevice.SetLaserPower(LaserChannels.ChannelA, 0);
                        if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R2")
                            EthernetDevice.SetLaserPower(LaserChannels.ChannelB, 0);
                    }
                }
            }
        }
        #endregion

        #region  Temperature
        public string _SensorTemperature;
        public string SensorTemperature
        {
            get { return _SensorTemperature; }
            set
            {
                if (_SensorTemperature != value)
                {
                    _SensorTemperature = value;
                    RaisePropertyChanged("SensorTemperature");
                }
            }
        }
        #endregion

        #region 激光器电流值

        #region 读取
        public ICommand GetCurrentValuelaserCommand
        {
            get
            {
                if (_GetCurrentValuelaserCommand == null)
                {
                    _GetCurrentValuelaserCommand = new RelayCommand(Execute__GeCurrentValuelaserCommand, CanExecut_GetCurrentValuelaserCommand);
                }
                return _GetCurrentValuelaserCommand;
            }
        }
        public void Execute__GeCurrentValuelaserCommand(object parameter)
        {
            if (!Workspace.This.MotorVM.IsNewFirmware)
            {
                MessageBox.Show("网络未连接！", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "L")
            {
                EthernetDevice.GetCurrentValuelaser(LaserChannels.ChannelC);
                Thread.Sleep(100);
                EthernetDevice.GetCurrentValuelaser(LaserChannels.ChannelC);
                GetCurrentValuelaser = (int)EthernetDevice.AllCurrentValuelaser[LaserChannels.ChannelC];
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R1")
            {
                EthernetDevice.GetCurrentValuelaser(LaserChannels.ChannelA);
                Thread.Sleep(100);
                EthernetDevice.GetCurrentValuelaser(LaserChannels.ChannelA);
                GetCurrentValuelaser = (int)EthernetDevice.AllCurrentValuelaser[LaserChannels.ChannelA];
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R2")
            {
                EthernetDevice.GetCurrentValuelaser(LaserChannels.ChannelB);
                Thread.Sleep(100);
                EthernetDevice.GetCurrentValuelaser(LaserChannels.ChannelB);
                GetCurrentValuelaser = (int)EthernetDevice.AllCurrentValuelaser[LaserChannels.ChannelB];
            }

        }
        public bool CanExecut_GetCurrentValuelaserCommand(object parameter)
        {
            return true;
        }
        private int _GetCurrentValuelaser = 0;
        public int GetCurrentValuelaser
        {
            get { return _GetCurrentValuelaser; }
            set
            {

                if (_GetCurrentValuelaser != value)
                {
                    if (value == Workspace.This.Uint32Code)
                    {
                        value = 0;
                    }
                    _GetCurrentValuelaser = value;
                    RaisePropertyChanged("GetCurrentValuelaser");
                }
            }
        }
        #endregion

        #region 写入
        private bool _IsSetCurrentValuelaserSelected = false;
        public bool IsSetCurrentValuelaserSelected
        {
            get { return _IsSetCurrentValuelaserSelected; }
            set
            {
                if (_IsSetCurrentValuelaserSelected != value)
                {
                    _IsSetCurrentValuelaserSelected = value;
                    RaisePropertyChanged("IsSetCurrentValuelaserSelected");
                    if (value == true)
                    {
                        if (Workspace.This.MotorVM.IsNewFirmware)
                        {
                            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "L")
                            {
                                EthernetDevice.SetLaserCurrent(LaserChannels.ChannelC, SetCurrentValuelaser);
                            }
                            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R1")
                            {
                                EthernetDevice.SetLaserCurrent(LaserChannels.ChannelA, SetCurrentValuelaser);
                            }
                            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R2")
                            {
                                EthernetDevice.SetLaserCurrent(LaserChannels.ChannelB, SetCurrentValuelaser);
                            }
                        }
                        else
                        {
                            MessageBox.Show("网络未连接！", "", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;

                        }
                    }
                    else
                    {
                        if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "L")
                            EthernetDevice.SetLaserCurrent(LaserChannels.ChannelC, 0);
                        if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R1")
                            EthernetDevice.SetLaserCurrent(LaserChannels.ChannelA, 0);
                        if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R2")
                            EthernetDevice.SetLaserCurrent(LaserChannels.ChannelB, 0);
                    }
                }
            }
        }
        //public ICommand SetCurrentValuelaserCommand
        //{
        //    get
        //    {
        //        if (_SetCurrentValuelaserCommand == null)
        //        {
        //            _SetCurrentValuelaserCommand = new RelayCommand(Execute_SetCurrentValuelaserCommand, CanExecut_SetCurrentValuelaserCommand);
        //        }
        //        return _SetCurrentValuelaserCommand;
        //    }
        //}
        //public void Execute_SetCurrentValuelaserCommand(object parameter)
        //{
        //    if (!Workspace.This.MotorVM.IsNewFirmware)
        //    {
        //        MessageBox.Show("网络未连接！", "", MessageBoxButton.OK, MessageBoxImage.Error);
        //        return;
        //    }
        //    if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "L")
        //    {
        //        EthernetDevice.SetLaserCurrent(LaserChannels.ChannelC, SetCurrentValuelaser);
        //    }
        //    if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R1")
        //    {
        //        EthernetDevice.SetLaserCurrent(LaserChannels.ChannelA, SetCurrentValuelaser);
        //    }
        //    if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R2")
        //    {
        //        EthernetDevice.SetLaserCurrent(LaserChannels.ChannelB, SetCurrentValuelaser);
        //    }

        //}
        //public bool CanExecut_SetCurrentValuelaserCommand(object parameter)
        //{
        //    return true;
        //}
        private int _SetCurrentValuelaser = 0;
        public int SetCurrentValuelaser
        {
            get { return _SetCurrentValuelaser; }
            set
            {

                if (_SetCurrentValuelaser != value)
                {
                    _SetCurrentValuelaser = value;
                    RaisePropertyChanged("SetCurrentValuelaser");
                }
            }
        }
        #endregion

        #endregion

        #region TED PID
        #region KP读取
        public ICommand GetTECkpCommand
        {
            get
            {
                if (_GetTECkpCommand == null)
                {
                    _GetTECkpCommand = new RelayCommand(Execute_GetTECkpCommand, CanExecut_GetTECkpCommand);
                }
                return _GetTECkpCommand;
            }
        }
        public void Execute_GetTECkpCommand(object parameter)
        {
            if (!Workspace.This.MotorVM.IsNewFirmware)
            {
                MessageBox.Show("网络未连接！", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "L")
            {
                EthernetDevice.GetAllCurrentTECRefrigerationControlParameterKp(LaserChannels.ChannelC);
                Thread.Sleep(100);
                EthernetDevice.GetAllCurrentTECRefrigerationControlParameterKp(LaserChannels.ChannelC);
                GetTECkp = (int)EthernetController.TECRefrigerationControlParameterKp[LaserChannels.ChannelC];
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R1")
            {
                EthernetDevice.GetAllCurrentTECRefrigerationControlParameterKp(LaserChannels.ChannelA);
                Thread.Sleep(100);
                EthernetDevice.GetAllCurrentTECRefrigerationControlParameterKp(LaserChannels.ChannelA);
                GetTECkp = (int)EthernetController.TECRefrigerationControlParameterKp[LaserChannels.ChannelA];
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R2")
            {
                EthernetDevice.GetAllCurrentTECRefrigerationControlParameterKp(LaserChannels.ChannelB);
                Thread.Sleep(100);
                EthernetDevice.GetAllCurrentTECRefrigerationControlParameterKp(LaserChannels.ChannelB);
                GetTECkp = (int)EthernetController.TECRefrigerationControlParameterKp[LaserChannels.ChannelB];
            }

        }
        public bool CanExecut_GetTECkpCommand(object parameter)
        {
            return true;
        }
        public double GetTECkp
        {
            get { return _GetTECkp; }
            set
            {

                if (_GetTECkp != value)
                {
                    if (value == Workspace.This.Uint16Code)
                    {
                        value = double.NaN;
                    }
                    _GetTECkp = value;
                    RaisePropertyChanged("GetTECkp");
                }
            }
        }
        #endregion

        #region KP写入
        public ICommand SetTECkpCommand
        {
            get
            {
                if (_SetTECkpCommand == null)
                {
                    _SetTECkpCommand = new RelayCommand(Execute_SetTECkpCommand, CanExecut_SetTECkpCommand);
                }
                return _SetTECkpCommand;
            }
        }
        public void Execute_SetTECkpCommand(object parameter)
        {
            if (!Workspace.This.MotorVM.IsNewFirmware)
            {
                MessageBox.Show("网络未连接！", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "L")
            {
                EthernetDevice.SetTECControlKp(LaserChannels.ChannelC, SetTECkp);
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R1")
            {
                EthernetDevice.SetTECControlKp(LaserChannels.ChannelA, SetTECkp);

            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R2")
            {
                EthernetDevice.SetTECControlKp(LaserChannels.ChannelB, SetTECkp);
            }

        }
        public bool CanExecut_SetTECkpCommand(object parameter)
        {
            return true;
        }
        public double SetTECkp
        {
            get { return _SetTECkp; }
            set
            {

                if (_SetTECkp != value)
                {
                    _SetTECkp = value;
                    RaisePropertyChanged("SetTECkp");
                }
            }
        }

        #endregion

        #region KI读取
        public ICommand GetTECkiCommand
        {
            get
            {
                if (_GetTECkiCommand == null)
                {
                    _GetTECkiCommand = new RelayCommand(Execute_GetTECkiCommand, CanExecut_GetTECkiCommand);
                }
                return _GetTECkiCommand;
            }
        }
        public void Execute_GetTECkiCommand(object parameter)
        {
            if (!Workspace.This.MotorVM.IsNewFirmware)
            {
                MessageBox.Show("网络未连接！", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "L")
            {
                EthernetDevice.GetAllCurrentTECRefrigerationControlParameterKi(LaserChannels.ChannelC);
                Thread.Sleep(100);
                EthernetDevice.GetAllCurrentTECRefrigerationControlParameterKi(LaserChannels.ChannelC);
                GetTECki = (int)EthernetController.TECRefrigerationControlParameterKi[LaserChannels.ChannelC];
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R1")
            {
                EthernetDevice.GetAllCurrentTECRefrigerationControlParameterKi(LaserChannels.ChannelA);
                Thread.Sleep(100);
                EthernetDevice.GetAllCurrentTECRefrigerationControlParameterKi(LaserChannels.ChannelA);
                GetTECki = (int)EthernetController.TECRefrigerationControlParameterKi[LaserChannels.ChannelA];
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R2")
            {
                EthernetDevice.GetAllCurrentTECRefrigerationControlParameterKi(LaserChannels.ChannelB);
                Thread.Sleep(100);
                EthernetDevice.GetAllCurrentTECRefrigerationControlParameterKi(LaserChannels.ChannelB);
                GetTECki = (int)EthernetController.TECRefrigerationControlParameterKi[LaserChannels.ChannelB];
            }

        }
        public bool CanExecut_GetTECkiCommand(object parameter)
        {
            return true;
        }
        public double GetTECki
        {
            get { return _GetTECki; }
            set
            {

                if (_GetTECki != value)
                {
                    if (value == Workspace.This.Uint16Code)
                    {
                        value = double.NaN;
                    }
                    _GetTECki = value;
                    RaisePropertyChanged("GetTECki");
                }
            }
        }
        #endregion

        #region KI写入
        public ICommand SetTECkiCommand
        {
            get
            {
                if (_SetTECkiCommand == null)
                {
                    _SetTECkiCommand = new RelayCommand(Execute_SetTECkiCommand, CanExecut_SetTECkiCommand);
                }
                return _SetTECkiCommand;
            }
        }
        public void Execute_SetTECkiCommand(object parameter)
        {
            if (!Workspace.This.MotorVM.IsNewFirmware)
            {
                MessageBox.Show("网络未连接！", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "L")
            {
                EthernetDevice.SetTECControlKi(LaserChannels.ChannelC, SetTECki);
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R1")
            {
                EthernetDevice.SetTECControlKi(LaserChannels.ChannelA, SetTECki);

            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R2")
            {
                EthernetDevice.SetTECControlKi(LaserChannels.ChannelB, SetTECki);
            }

        }
        public bool CanExecut_SetTECkiCommand(object parameter)
        {
            return true;
        }
        public double SetTECki
        {
            get { return _SetTECki; }
            set
            {

                if (_SetTECki != value)
                {
                    _SetTECki = value;
                    RaisePropertyChanged("SetTECki");
                }
            }
        }

        #endregion

        #region KD读取
        public ICommand GetTECkdCommand
        {
            get
            {
                if (_GetTECkdCommand == null)
                {
                    _GetTECkdCommand = new RelayCommand(Execute_GetTECkdCommand, CanExecut_GetTECkdCommand);
                }
                return _GetTECkdCommand;
            }
        }
        public void Execute_GetTECkdCommand(object parameter)
        {
            if (!Workspace.This.MotorVM.IsNewFirmware)
            {
                MessageBox.Show("网络未连接！", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "L")
            {
                EthernetDevice.GetAllCurrentTECRefrigerationControlParameterKd(LaserChannels.ChannelC);
                Thread.Sleep(100);
                EthernetDevice.GetAllCurrentTECRefrigerationControlParameterKd(LaserChannels.ChannelC);
                GetTECkd = (int)EthernetController.TECRefrigerationControlParameterKd[LaserChannels.ChannelC];
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R1")
            {
                EthernetDevice.GetAllCurrentTECRefrigerationControlParameterKd(LaserChannels.ChannelA);
                Thread.Sleep(100);
                EthernetDevice.GetAllCurrentTECRefrigerationControlParameterKd(LaserChannels.ChannelA);
                GetTECkd = (int)EthernetController.TECRefrigerationControlParameterKd[LaserChannels.ChannelA];
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R2")
            {
                EthernetDevice.GetAllCurrentTECRefrigerationControlParameterKd(LaserChannels.ChannelB);
                Thread.Sleep(100);
                EthernetDevice.GetAllCurrentTECRefrigerationControlParameterKd(LaserChannels.ChannelB);
                GetTECkd = (int)EthernetController.TECRefrigerationControlParameterKd[LaserChannels.ChannelB];
            }

        }
        public bool CanExecut_GetTECkdCommand(object parameter)
        {
            return true;
        }
        public double GetTECkd
        {
            get { return _GetTECkd; }
            set
            {

                if (_GetTECkd != value)
                {
                    if (value == Workspace.This.Uint16Code)
                    {
                        value = double.NaN;
                    }
                    _GetTECkd = value;
                    RaisePropertyChanged("GetTECkd");
                }
            }
        }
        #endregion

        #region KI写入
        public ICommand SetTECkdCommand
        {
            get
            {
                if (_SetTECkdCommand == null)
                {
                    _SetTECkdCommand = new RelayCommand(Execute_SetTECkdCommand, CanExecut_SetTECkdCommand);
                }
                return _SetTECkdCommand;
            }
        }
        public void Execute_SetTECkdCommand(object parameter)
        {
            if (!Workspace.This.MotorVM.IsNewFirmware)
            {
                MessageBox.Show("网络未连接！", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "L")
            {
                EthernetDevice.SetTECControlKd(LaserChannels.ChannelC, SetTECkd);
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R1")
            {
                EthernetDevice.SetTECControlKd(LaserChannels.ChannelA, SetTECkd);

            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R2")
            {
                EthernetDevice.SetTECControlKd(LaserChannels.ChannelB, SetTECkd);
            }

        }
        public bool CanExecut_SetTECkdCommand(object parameter)
        {
            return true;
        }
        public double SetTECkd
        {
            get { return _SetTECkd; }
            set
            {

                if (_SetTECkd != value)
                {
                    _SetTECkd = value;
                    RaisePropertyChanged("SetTECkd");
                }
            }
        }

        #endregion
        #endregion

        public double SetOpticalPowerLessThanOrEqual15mWKp
        {
            get { return _SetOpticalPowerLessThanOrEqual15mWKp; }
            set
            {

                if (_SetOpticalPowerLessThanOrEqual15mWKp != value)
                {
                    _SetOpticalPowerLessThanOrEqual15mWKp = value;
                    RaisePropertyChanged("SetOpticalPowerLessThanOrEqual15mWKp");
                }
            }
        }
        public double SetOpticalPowerLessThanOrEqual15mWKi
        {
            get { return _SetOpticalPowerLessThanOrEqual15mWKi; }
            set
            {

                if (_SetOpticalPowerLessThanOrEqual15mWKi != value)
                {
                    _SetOpticalPowerLessThanOrEqual15mWKi = value;
                    RaisePropertyChanged("SetOpticalPowerLessThanOrEqual15mWKi");
                }
            }
        }
        public double SetOpticalPowerLessThanOrEqual15mWKd
        {
            get { return _SetOpticalPowerLessThanOrEqual15mWKd; }
            set
            {

                if (_SetOpticalPowerLessThanOrEqual15mWKd != value)
                {
                    _SetOpticalPowerLessThanOrEqual15mWKd = value;
                    RaisePropertyChanged("SetOpticalPowerLessThanOrEqual15mWKd");
                }
            }
        }
        public double SetOpticalPowerGreaterThan15mWKp
        {
            get { return _SetOpticalPowerGreaterThan15mWKp; }
            set
            {

                if (_SetOpticalPowerGreaterThan15mWKp != value)
                {
                    _SetOpticalPowerGreaterThan15mWKp = value;
                    RaisePropertyChanged("SetOpticalPowerGreaterThan15mWKp");
                }
            }
        }
        public double SetOpticalPowerGreaterThan15mWKi
        {
            get { return _SetOpticalPowerGreaterThan15mWKi; }
            set
            {

                if (_SetOpticalPowerGreaterThan15mWKi != value)
                {
                    _SetOpticalPowerGreaterThan15mWKi = value;
                    RaisePropertyChanged("SetOpticalPowerGreaterThan15mWKi");
                }
            }
        }
        public double SetOpticalPowerGreaterThan15mWKd
        {
            get { return _SetOpticalPowerGreaterThan15mWKd; }
            set
            {

                if (_SetOpticalPowerGreaterThan15mWKd != value)
                {
                    _SetOpticalPowerGreaterThan15mWKd = value;
                    RaisePropertyChanged("SetOpticalPowerGreaterThan15mWKd");
                }
            }
        }

        public double SetOpticalPowerControlKpUpperLimitLessThanOrEqual15
        {
            get { return _SetOpticalPowerControlKpUpperLimitLessThanOrEqual15; }
            set
            {

                if (_SetOpticalPowerControlKpUpperLimitLessThanOrEqual15 != value)
                {
                    _SetOpticalPowerControlKpUpperLimitLessThanOrEqual15 = value;
                    RaisePropertyChanged("SetOpticalPowerControlKpUpperLimitLessThanOrEqual15");
                }
            }
        }
        public double SetOpticalPowerControlKpDownLimitLessThanOrEqual15
        {
            get { return _SetOpticalPowerControlKpDownLimitLessThanOrEqual15; }
            set
            {

                if (_SetOpticalPowerControlKpDownLimitLessThanOrEqual15 != value)
                {
                    _SetOpticalPowerControlKpDownLimitLessThanOrEqual15 = value;
                    RaisePropertyChanged("SetOpticalPowerControlKpDownLimitLessThanOrEqual15");
                }
            }
        }
        public double SetOpticalPowerControlKiUpperLimitLessThanOrEqual15
        {
            get { return _SetOpticalPowerControlKiUpperLimitLessThanOrEqual15; }
            set
            {

                if (_SetOpticalPowerControlKiUpperLimitLessThanOrEqual15 != value)
                {
                    _SetOpticalPowerControlKiUpperLimitLessThanOrEqual15 = value;
                    RaisePropertyChanged("SetOpticalPowerControlKiUpperLimitLessThanOrEqual15");
                }
            }
        }
        public double SetOpticalPowerControlKiDownLimitLessThanOrEqual15
        {
            get { return _SetOpticalPowerControlKiDownLimitLessThanOrEqual15; }
            set
            {

                if (_SetOpticalPowerControlKiDownLimitLessThanOrEqual15 != value)
                {
                    _SetOpticalPowerControlKiDownLimitLessThanOrEqual15 = value;
                    RaisePropertyChanged("SetOpticalPowerControlKiDownLimitLessThanOrEqual15");
                }
            }
        }
        public double SetOpticalPowerControlKpUpperLimitLessThan15
        {
            get { return _SetOpticalPowerControlKpUpperLimitLessThan15; }
            set
            {

                if (_SetOpticalPowerControlKpUpperLimitLessThan15 != value)
                {
                    _SetOpticalPowerControlKpUpperLimitLessThan15 = value;
                    RaisePropertyChanged("SetOpticalPowerControlKpUpperLimitLessThan15");
                }
            }
        }
        public double SetLaserMinCurrent
        {
            get { return _SetLaserMinCurrent; }
            set
            {

                if (_SetLaserMinCurrent != value)
                {
                    _SetLaserMinCurrent = value;
                    RaisePropertyChanged("SetLaserMinCurrent");
                }
            }
        }
        public double SetLaserMaxCurrent
        {
            get { return _SetLaserMaxCurrent; }
            set
            {

                if (_SetLaserMaxCurrent != value)
                {
                    _SetLaserMaxCurrent = value;
                    RaisePropertyChanged("SetLaserMaxCurrent");
                }
            }
        }
        public double SetOpticalPowerControlKiDownLimitLessThan15
        {
            get { return _SetOpticalPowerControlKiDownLimitLessThan15; }
            set
            {

                if (_SetOpticalPowerControlKiDownLimitLessThan15 != value)
                {
                    _SetOpticalPowerControlKiDownLimitLessThan15 = value;
                    RaisePropertyChanged("SetOpticalPowerControlKiDownLimitLessThan15");
                }
            }
        }
        public double SetOpticalPowerControlKiUpperLimitLessThan15
        {
            get { return _SetOpticalPowerControlKiUpperLimitLessThan15; }
            set
            {

                if (_SetOpticalPowerControlKiUpperLimitLessThan15 != value)
                {
                    _SetOpticalPowerControlKiUpperLimitLessThan15 = value;
                    RaisePropertyChanged("SetOpticalPowerControlKiUpperLimitLessThan15");
                }
            }
        }
        public double SetOpticalPowerControlKpDownLimitLessThan15
        {
            get { return _SetOpticalPowerControlKpDownLimitLessThan15; }
            set
            {

                if (_SetOpticalPowerControlKpDownLimitLessThan15 != value)
                {
                    _SetOpticalPowerControlKpDownLimitLessThan15 = value;
                    RaisePropertyChanged("SetOpticalPowerControlKpDownLimitLessThan15");
                }
            }
        }


        public double GetLaserMinCurrent
        {
            get { return _GetLaserMinCurrent; }
            set
            {

                if (_GetLaserMinCurrent != value)
                {
                    if ((value * 10) == Workspace.This.Uint16Code)
                    {
                        value = 0;
                    }
                    _GetLaserMinCurrent = value;
                    RaisePropertyChanged("GetLaserMinCurrent");
                }
            }
        }
        public double GetLaserMaxCurrent
        {
            get { return _GetLaserMaxCurrent; }
            set
            {

                if (_GetLaserMaxCurrent != value)
                {
                    if ((value * 10) == Workspace.This.Uint16Code)
                    {
                        value = 0;
                    }
                    _GetLaserMaxCurrent = value;
                    RaisePropertyChanged("GetLaserMaxCurrent");
                }
            }
        }
        public double GetOpticalPowerControlKiDownLimitLessThan15
        {
            get { return _GetOpticalPowerControlKiDownLimitLessThan15; }
            set
            {

                if (_GetOpticalPowerControlKiDownLimitLessThan15 != value)
                {
                    if (value == Workspace.This.Uint32Code)
                    {
                        value = 0;
                    }
                    _GetOpticalPowerControlKiDownLimitLessThan15 = value;
                    RaisePropertyChanged("GetOpticalPowerControlKiDownLimitLessThan15");
                }
            }
        }
        public double GetOpticalPowerControlKiUpperLimitLessThan15
        {
            get { return _GetOpticalPowerControlKiUpperLimitLessThan15; }
            set
            {

                if (_GetOpticalPowerControlKiUpperLimitLessThan15 != value)
                {
                    if (value == Workspace.This.Uint32Code)
                    {
                        value = 0;
                    }
                    _GetOpticalPowerControlKiUpperLimitLessThan15 = value;
                    RaisePropertyChanged("GetOpticalPowerControlKiUpperLimitLessThan15");
                }
            }
        }
        public double GetOpticalPowerControlKpDownLimitLessThan15
        {
            get { return _GetOpticalPowerControlKpDownLimitLessThan15; }
            set
            {

                if (_GetOpticalPowerControlKpDownLimitLessThan15 != value)
                {
                    if (value == Workspace.This.Uint32Code)
                    {
                        value = 0;
                    }
                    _GetOpticalPowerControlKpDownLimitLessThan15 = value;
                    RaisePropertyChanged("GetOpticalPowerControlKpDownLimitLessThan15");
                }
            }
        }
        public double GetOpticalPowerControlKpUpperLimitLessThan15
        {
            get { return _GetOpticalPowerControlKpUpperLimitLessThan15; }
            set
            {

                if (_GetOpticalPowerControlKpUpperLimitLessThan15 != value)
                {
                    if (value == Workspace.This.Uint32Code)
                    {
                        value = 0;
                    }
                    _GetOpticalPowerControlKpUpperLimitLessThan15 = value;
                    RaisePropertyChanged("GetOpticalPowerControlKpUpperLimitLessThan15");
                }
            }
        }
        public double GetOpticalPowerControlKiDownLimitLessThanOrEqual15
        {
            get { return _GetOpticalPowerControlKiDownLimitLessThanOrEqual15; }
            set
            {

                if (_GetOpticalPowerControlKiDownLimitLessThanOrEqual15 != value)
                {
                    if (value == Workspace.This.Uint32Code)
                    {
                        value = 0;
                    }
                    _GetOpticalPowerControlKiDownLimitLessThanOrEqual15 = value;
                    RaisePropertyChanged("GetOpticalPowerControlKiDownLimitLessThanOrEqual15");
                }
            }
        }
        public double GetOpticalPowerControlKiUpperLimitLessThanOrEqual15
        {
            get { return _GetOpticalPowerControlKiUpperLimitLessThanOrEqual15; }
            set
            {

                if (_GetOpticalPowerControlKiUpperLimitLessThanOrEqual15 != value)
                {
                    if (value == Workspace.This.Uint32Code)
                    {
                        value = 0;
                    }
                    _GetOpticalPowerControlKiUpperLimitLessThanOrEqual15 = value;
                    RaisePropertyChanged("GetOpticalPowerControlKiUpperLimitLessThanOrEqual15");
                }
            }
        }
        public double GetOpticalPowerControlKpDownLimitLessThanOrEqual15
        {
            get { return _GetOpticalPowerControlKpDownLimitLessThanOrEqual15; }
            set
            {

                if (_GetOpticalPowerControlKpDownLimitLessThanOrEqual15 != value)
                {
                    if (value == Workspace.This.Uint32Code)
                    {
                        value = 0;
                    }
                    _GetOpticalPowerControlKpDownLimitLessThanOrEqual15 = value;
                    RaisePropertyChanged("GetOpticalPowerControlKpDownLimitLessThanOrEqual15");
                }
            }
        }
        public double GetOpticalPowerControlKpUpperLimitLessThanOrEqual15
        {
            get { return _GetOpticalPowerControlKpUpperLimitLessThanOrEqual15; }
            set
            {

                if (_GetOpticalPowerControlKpUpperLimitLessThanOrEqual15 != value)
                {
                    if (value == Workspace.This.Uint32Code)
                    {
                        value = 0;
                    }
                    _GetOpticalPowerControlKpUpperLimitLessThanOrEqual15 = value;
                    RaisePropertyChanged("GetOpticalPowerControlKpUpperLimitLessThanOrEqual15");
                }
            }
        }
        public double GetOpticalPowerGreaterThan15mWKd
        {
            get { return _GetOpticalPowerGreaterThan15mWKd; }
            set
            {

                if (_GetOpticalPowerGreaterThan15mWKd != value)
                {
                    if (value == Workspace.This.Uint16Code)
                    {
                        value = double.NaN;
                    }
                    _GetOpticalPowerGreaterThan15mWKd = value;
                    RaisePropertyChanged("GetOpticalPowerGreaterThan15mWKd");
                }
            }
        }
        public double GetOpticalPowerGreaterThan15mWKi
        {
            get { return _GetOpticalPowerGreaterThan15mWKi; }
            set
            {

                if (_GetOpticalPowerGreaterThan15mWKi != value)
                {
                    if (value == Workspace.This.Uint16Code)
                    {
                        value = double.NaN;
                    }
                    _GetOpticalPowerGreaterThan15mWKi = value;
                    RaisePropertyChanged("GetOpticalPowerGreaterThan15mWKi");
                }
            }
        }
        public double GetOpticalPowerGreaterThan15mWKp
        {
            get { return _GetOpticalPowerGreaterThan15mWKp; }
            set
            {

                if (_GetOpticalPowerGreaterThan15mWKp != value)
                {
                    if (value == Workspace.This.Uint16Code)
                    {
                        value = double.NaN;
                    }
                    _GetOpticalPowerGreaterThan15mWKp = value;
                    RaisePropertyChanged("GetOpticalPowerGreaterThan15mWKp");
                }
            }
        }
        public double GetOpticalPowerLessThanOrEqual15mWKd
        {
            get { return _GetOpticalPowerLessThanOrEqual15mWKd; }
            set
            {

                if (_GetOpticalPowerLessThanOrEqual15mWKd != value)
                {
                    if (value == Workspace.This.Uint16Code)
                    {
                        value = double.NaN;
                    }
                    _GetOpticalPowerLessThanOrEqual15mWKd = value;
                    RaisePropertyChanged("GetOpticalPowerLessThanOrEqual15mWKd");
                }
            }
        }

        public double GetOpticalPowerLessThanOrEqual15mWKi
        {
            get { return _GetOpticalPowerLessThanOrEqual15mWKi; }
            set
            {

                if (_GetOpticalPowerLessThanOrEqual15mWKi != value)
                {
                    if (value == Workspace.This.Uint16Code)
                    {
                        value = double.NaN;
                    }
                    _GetOpticalPowerLessThanOrEqual15mWKi = value;
                    RaisePropertyChanged("GetOpticalPowerLessThanOrEqual15mWKi");
                }
            }
        }
        public double GetOpticalPowerLessThanOrEqual15mWKp
        {
            get { return _GetOpticalPowerLessThanOrEqual15mWKp; }
            set
            {

                if (_GetOpticalPowerLessThanOrEqual15mWKp != value)
                {
                    if (value == Workspace.This.Uint16Code)
                    {
                        value = double.NaN;
                    }
                    _GetOpticalPowerLessThanOrEqual15mWKp = value;
                    RaisePropertyChanged("GetOpticalPowerLessThanOrEqual15mWKp");
                }
            }
        }

        #region GetOpticalPowerLessThanOrEqual15mWKpCommand

        public ICommand GetOpticalPowerLessThanOrEqual15mWKpCommand
        {
            get
            {
                if (_GetOpticalPowerLessThanOrEqual15mWKpCommand == null)
                {
                    _GetOpticalPowerLessThanOrEqual15mWKpCommand = new RelayCommand(ExecuteGetOpticalPowerLessThanOrEqual15mWKpCommand, CanExecuteGetOpticalPowerLessThanOrEqual15mWKpCommand);
                }
                return _GetOpticalPowerLessThanOrEqual15mWKpCommand;
            }
        }
        public void ExecuteGetOpticalPowerLessThanOrEqual15mWKpCommand(object parameter)
        {
            Workspace.This.EthernetController.GetOpticalPowerLessThanOrEqual15mWKp();
            Thread.Sleep(1000);
            Workspace.This.EthernetController.GetOpticalPowerLessThanOrEqual15mWKp();
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "L")
            {
                GetOpticalPowerLessThanOrEqual15mWKp = EthernetController.AllOpticalPowerLessThanOrEqual15mWKp[LaserChannels.ChannelC];
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R1")
            {
                GetOpticalPowerLessThanOrEqual15mWKp = EthernetController.AllOpticalPowerLessThanOrEqual15mWKp[LaserChannels.ChannelA];
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R2")
            {
                GetOpticalPowerLessThanOrEqual15mWKp = EthernetController.AllOpticalPowerLessThanOrEqual15mWKp[LaserChannels.ChannelB];
            }
        }
        public bool CanExecuteGetOpticalPowerLessThanOrEqual15mWKpCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region GetOpticalPowerLessThanOrEqual15mWKiCommand

        public ICommand GetOpticalPowerLessThanOrEqual15mWKiCommand
        {
            get
            {
                if (_GetOpticalPowerLessThanOrEqual15mWKiCommand == null)
                {
                    _GetOpticalPowerLessThanOrEqual15mWKiCommand = new RelayCommand(ExecuteGetOpticalPowerLessThanOrEqual15mWKiCommand, CanExecuteGetOpticalPowerLessThanOrEqual15mWKiCommand);
                }
                return _GetOpticalPowerLessThanOrEqual15mWKiCommand;
            }
        }
        public void ExecuteGetOpticalPowerLessThanOrEqual15mWKiCommand(object parameter)
        {
            Workspace.This.EthernetController.GetOpticalPowerLessThanOrEqual15mWKi();
            Thread.Sleep(1000);
            Workspace.This.EthernetController.GetOpticalPowerLessThanOrEqual15mWKi();
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "L")
            {
                GetOpticalPowerLessThanOrEqual15mWKi = EthernetController.AllOpticalPowerLessThanOrEqual15mWKi[LaserChannels.ChannelC];
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R1")
            {
                GetOpticalPowerLessThanOrEqual15mWKi = EthernetController.AllOpticalPowerLessThanOrEqual15mWKi[LaserChannels.ChannelA];
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R2")
            {
                GetOpticalPowerLessThanOrEqual15mWKi = EthernetController.AllOpticalPowerLessThanOrEqual15mWKi[LaserChannels.ChannelB];
            }
        }
        public bool CanExecuteGetOpticalPowerLessThanOrEqual15mWKiCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region GetOpticalPowerLessThanOrEqual15mWKdCommand

        public ICommand GetOpticalPowerLessThanOrEqual15mWKdCommand
        {
            get
            {
                if (_GetOpticalPowerLessThanOrEqual15mWKdCommand == null)
                {
                    _GetOpticalPowerLessThanOrEqual15mWKdCommand = new RelayCommand(ExecuteGetOpticalPowerLessThanOrEqual15mWKdCommand, CanExecuteGetOpticalPowerLessThanOrEqual15mWKdCommand);
                }
                return _GetOpticalPowerLessThanOrEqual15mWKdCommand;
            }
        }
        public void ExecuteGetOpticalPowerLessThanOrEqual15mWKdCommand(object parameter)
        {
            Workspace.This.EthernetController.GetOpticalPowerLessThanOrEqual15mWKd();
            Thread.Sleep(1000);
            Workspace.This.EthernetController.GetOpticalPowerLessThanOrEqual15mWKd();
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "L")
            {
                GetOpticalPowerLessThanOrEqual15mWKd = EthernetController.AllOpticalPowerLessThanOrEqual15mWKd[LaserChannels.ChannelC];
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R1")
            {
                GetOpticalPowerLessThanOrEqual15mWKd = EthernetController.AllOpticalPowerLessThanOrEqual15mWKd[LaserChannels.ChannelA];
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R2")
            {
                GetOpticalPowerLessThanOrEqual15mWKd = EthernetController.AllOpticalPowerLessThanOrEqual15mWKd[LaserChannels.ChannelB];
            }

        }
        public bool CanExecuteGetOpticalPowerLessThanOrEqual15mWKdCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region GetOpticalPowerGreaterThan15mWKpCommand

        public ICommand GetOpticalPowerGreaterThan15mWKpCommand
        {
            get
            {
                if (_GetOpticalPowerGreaterThan15mWKpCommand == null)
                {
                    _GetOpticalPowerGreaterThan15mWKpCommand = new RelayCommand(ExecuteGetOpticalPowerGreaterThan15mWKpCommand, CanExecuteGetOpticalPowerGreaterThan15mWKpCommand);
                }
                return _GetOpticalPowerGreaterThan15mWKpCommand;
            }
        }
        public void ExecuteGetOpticalPowerGreaterThan15mWKpCommand(object parameter)
        {
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "L")
            {
                Workspace.This.EthernetController.GetOpticalPowerGreaterThan15mWKp(LaserChannels.ChannelC);
                Thread.Sleep(1000);
                Workspace.This.EthernetController.GetOpticalPowerGreaterThan15mWKp(LaserChannels.ChannelC);
                GetOpticalPowerGreaterThan15mWKp = EthernetController.AllOpticalPowerGreaterThan15mWKp[LaserChannels.ChannelC];
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R1")
            {
                Workspace.This.EthernetController.GetOpticalPowerGreaterThan15mWKp(LaserChannels.ChannelA);
                Thread.Sleep(1000);
                Workspace.This.EthernetController.GetOpticalPowerGreaterThan15mWKp(LaserChannels.ChannelA);
                GetOpticalPowerGreaterThan15mWKp = EthernetController.AllOpticalPowerGreaterThan15mWKp[LaserChannels.ChannelA];
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R2")
            {
                Workspace.This.EthernetController.GetOpticalPowerGreaterThan15mWKp(LaserChannels.ChannelB);
                Thread.Sleep(1000);
                Workspace.This.EthernetController.GetOpticalPowerGreaterThan15mWKp(LaserChannels.ChannelB);
                GetOpticalPowerGreaterThan15mWKp = EthernetController.AllOpticalPowerGreaterThan15mWKp[LaserChannels.ChannelB];
            }

        }
        public bool CanExecuteGetOpticalPowerGreaterThan15mWKpCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region GetOpticalPowerGreaterThan15mWKiCommand

        public ICommand GetOpticalPowerGreaterThan15mWKiCommand
        {
            get
            {
                if (_GetOpticalPowerGreaterThan15mWKiCommand == null)
                {
                    _GetOpticalPowerGreaterThan15mWKiCommand = new RelayCommand(ExecuteGetOpticalPowerGreaterThan15mWKiCommand, CanExecuteGetOpticalPowerGreaterThan15mWKiCommand);
                }
                return _GetOpticalPowerGreaterThan15mWKiCommand;
            }
        }
        public void ExecuteGetOpticalPowerGreaterThan15mWKiCommand(object parameter)
        {
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "L")
            {
                Workspace.This.EthernetController.GetOpticalPowerGreaterThan15mWKi(LaserChannels.ChannelC);
                Thread.Sleep(1000);
                Workspace.This.EthernetController.GetOpticalPowerGreaterThan15mWKi(LaserChannels.ChannelC);
                GetOpticalPowerGreaterThan15mWKi = EthernetController.AllOpticalPowerGreaterThan15mWKi[LaserChannels.ChannelC];
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R1")
            {
                Workspace.This.EthernetController.GetOpticalPowerGreaterThan15mWKi(LaserChannels.ChannelA);
                Thread.Sleep(1000);
                Workspace.This.EthernetController.GetOpticalPowerGreaterThan15mWKi(LaserChannels.ChannelA);
                GetOpticalPowerGreaterThan15mWKi = EthernetController.AllOpticalPowerGreaterThan15mWKi[LaserChannels.ChannelA];
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R2")
            {
                Workspace.This.EthernetController.GetOpticalPowerGreaterThan15mWKi(LaserChannels.ChannelB);
                Thread.Sleep(1000);
                Workspace.This.EthernetController.GetOpticalPowerGreaterThan15mWKi(LaserChannels.ChannelB);
                GetOpticalPowerGreaterThan15mWKi = EthernetController.AllOpticalPowerGreaterThan15mWKi[LaserChannels.ChannelB];
            }
        }
        public bool CanExecuteGetOpticalPowerGreaterThan15mWKiCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region GetOpticalPowerGreaterThan15mWKdCommand

        public ICommand GetOpticalPowerGreaterThan15mWKdCommand
        {
            get
            {
                if (_GetOpticalPowerGreaterThan15mWKdCommand == null)
                {
                    _GetOpticalPowerGreaterThan15mWKdCommand = new RelayCommand(ExecuteGetOpticalPowerGreaterThan15mWKdCommand, CanExecuteGetOpticalPowerGreaterThan15mWKdCommand);
                }
                return _GetOpticalPowerGreaterThan15mWKdCommand;
            }
        }
        public void ExecuteGetOpticalPowerGreaterThan15mWKdCommand(object parameter)
        {
            Workspace.This.EthernetController.GetAllOpticalPowerGreaterThan15mWKd(LaserChannels.ChannelC);
            Thread.Sleep(1000);
            Workspace.This.EthernetController.GetAllOpticalPowerGreaterThan15mWKd(LaserChannels.ChannelC);
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "L")
            {
                GetOpticalPowerGreaterThan15mWKd = EthernetController.AllOpticalPowerGreaterThan15mWKd[LaserChannels.ChannelC];
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R1")
            {
                GetOpticalPowerGreaterThan15mWKd = EthernetController.AllOpticalPowerGreaterThan15mWKd[LaserChannels.ChannelA];
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R2")
            {
                GetOpticalPowerGreaterThan15mWKd = EthernetController.AllOpticalPowerGreaterThan15mWKd[LaserChannels.ChannelB];
            }

        }
        public bool CanExecuteGetOpticalPowerGreaterThan15mWKdCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region GetOpticalPowerControlKpUpperLimitLessThanOrEqual15Command

        public ICommand GetOpticalPowerControlKpUpperLimitLessThanOrEqual15Command
        {
            get
            {
                if (_GetOpticalPowerControlKpUpperLimitLessThanOrEqual15Command == null)
                {
                    _GetOpticalPowerControlKpUpperLimitLessThanOrEqual15Command = new RelayCommand(ExecuteGetOpticalPowerControlKpUpperLimitLessThanOrEqual15Command, CanExecuteGetOpticalPowerControlKpUpperLimitLessThanOrEqual15Command);
                }
                return _GetOpticalPowerControlKpUpperLimitLessThanOrEqual15Command;
            }
        }
        public void ExecuteGetOpticalPowerControlKpUpperLimitLessThanOrEqual15Command(object parameter)
        {
            Workspace.This.EthernetController.GetAllOpticalPowerControlKpUpperLimitLessThan15();
            Thread.Sleep(1000);
            Workspace.This.EthernetController.GetAllOpticalPowerControlKpUpperLimitLessThan15();
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "L")
            {
                GetOpticalPowerControlKpUpperLimitLessThanOrEqual15 = EthernetController.AllGetOpticalPowerControlKpUpperLimitLessThan15[LaserChannels.ChannelC];
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R1")
            {
                GetOpticalPowerControlKpUpperLimitLessThanOrEqual15 = EthernetController.AllGetOpticalPowerControlKpUpperLimitLessThan15[LaserChannels.ChannelA];
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R2")
            {
                GetOpticalPowerControlKpUpperLimitLessThanOrEqual15 = EthernetController.AllGetOpticalPowerControlKpUpperLimitLessThan15[LaserChannels.ChannelB];
            }

        }
        public bool CanExecuteGetOpticalPowerControlKpUpperLimitLessThanOrEqual15Command(object parameter)
        {
            return true;
        }

        #endregion

        #region GetOpticalPowerControlKpDownLimitLessThanOrEqual15Command

        public ICommand GetOpticalPowerControlKpDownLimitLessThanOrEqual15Command
        {
            get
            {
                if (_GetOpticalPowerControlKpDownLimitLessThanOrEqual15Command == null)
                {
                    _GetOpticalPowerControlKpDownLimitLessThanOrEqual15Command = new RelayCommand(ExecuteGetOpticalPowerControlKpDownLimitLessThanOrEqual15Command, CanExecuteGetOpticalPowerControlKpDownLimitLessThanOrEqual15Command);
                }
                return _GetOpticalPowerControlKpDownLimitLessThanOrEqual15Command;
            }
        }
        public void ExecuteGetOpticalPowerControlKpDownLimitLessThanOrEqual15Command(object parameter)
        {
            Workspace.This.EthernetController.GetAllOpticalPowerControlKpDownLimitLessThan15();
            Thread.Sleep(1000);
            Workspace.This.EthernetController.GetAllOpticalPowerControlKpDownLimitLessThan15();
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "L")
            {
                GetOpticalPowerControlKpDownLimitLessThanOrEqual15 = EthernetController.AllGetOpticalPowerControlKpDownLimitLessThan15[LaserChannels.ChannelC];
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R1")
            {
                GetOpticalPowerControlKpDownLimitLessThanOrEqual15 = EthernetController.AllGetOpticalPowerControlKpDownLimitLessThan15[LaserChannels.ChannelA];
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R2")
            {
                GetOpticalPowerControlKpDownLimitLessThanOrEqual15 = EthernetController.AllGetOpticalPowerControlKpDownLimitLessThan15[LaserChannels.ChannelB];
            }
        }
        public bool CanExecuteGetOpticalPowerControlKpDownLimitLessThanOrEqual15Command(object parameter)
        {
            return true;
        }

        #endregion

        #region GetOpticalPowerControlKiUpperLimitLessThanOrEqual15Command

        public ICommand GetOpticalPowerControlKiUpperLimitLessThanOrEqual15Command
        {
            get
            {
                if (_GetOpticalPowerControlKiUpperLimitLessThanOrEqual15Command == null)
                {
                    _GetOpticalPowerControlKiUpperLimitLessThanOrEqual15Command = new RelayCommand(ExecuteGetOpticalPowerControlKiUpperLimitLessThanOrEqual15Command, CanExecuteGetOpticalPowerControlKiUpperLimitLessThanOrEqual15Command);
                }
                return _GetOpticalPowerControlKiUpperLimitLessThanOrEqual15Command;
            }
        }
        public void ExecuteGetOpticalPowerControlKiUpperLimitLessThanOrEqual15Command(object parameter)
        {
            Workspace.This.EthernetController.GetAllOpticalPowerControlKiUpperLimitLessThan15();
            Thread.Sleep(1000);
            Workspace.This.EthernetController.GetAllOpticalPowerControlKiUpperLimitLessThan15();
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "L")
            {
                GetOpticalPowerControlKiUpperLimitLessThanOrEqual15 = EthernetController.AllGetOpticalPowerControlKiUpperLimitLessThan15[LaserChannels.ChannelC];
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R1")
            {
                GetOpticalPowerControlKiUpperLimitLessThanOrEqual15 = EthernetController.AllGetOpticalPowerControlKiUpperLimitLessThan15[LaserChannels.ChannelA];
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R2")
            {
                GetOpticalPowerControlKiUpperLimitLessThanOrEqual15 = EthernetController.AllGetOpticalPowerControlKiUpperLimitLessThan15[LaserChannels.ChannelB];
            }

        }
        public bool CanExecuteGetOpticalPowerControlKiUpperLimitLessThanOrEqual15Command(object parameter)
        {
            return true;
        }

        #endregion

        #region GetOpticalPowerControlKiDownLimitLessThanOrEqual15Command

        public ICommand GetOpticalPowerControlKiDownLimitLessThanOrEqual15Command
        {
            get
            {
                if (_GetOpticalPowerControlKiDownLimitLessThanOrEqual15Command == null)
                {
                    _GetOpticalPowerControlKiDownLimitLessThanOrEqual15Command = new RelayCommand(ExecuteGetOpticalPowerControlKiDownLimitLessThanOrEqual15Command, CanExecuteGetOpticalPowerControlKiDownLimitLessThanOrEqual15Command);
                }
                return _GetOpticalPowerControlKiDownLimitLessThanOrEqual15Command;
            }
        }
        public void ExecuteGetOpticalPowerControlKiDownLimitLessThanOrEqual15Command(object parameter)
        {

            Workspace.This.EthernetController.GetAllOpticalPowerControlKiDownLimitLessThan15();
            Thread.Sleep(1000);
            Workspace.This.EthernetController.GetAllOpticalPowerControlKiDownLimitLessThan15();
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "L")
            {
                GetOpticalPowerControlKiDownLimitLessThanOrEqual15 = EthernetController.AllGetOpticalPowerControlKiDownLimitLessThan15[LaserChannels.ChannelC];
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R1")
            {
                GetOpticalPowerControlKiDownLimitLessThanOrEqual15 = EthernetController.AllGetOpticalPowerControlKiDownLimitLessThan15[LaserChannels.ChannelA];
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R2")
            {
                GetOpticalPowerControlKiDownLimitLessThanOrEqual15 = EthernetController.AllGetOpticalPowerControlKiDownLimitLessThan15[LaserChannels.ChannelB];
            }
        }
        public bool CanExecuteGetOpticalPowerControlKiDownLimitLessThanOrEqual15Command(object parameter)
        {
            return true;
        }

        #endregion

        #region GetOpticalPowerControlKpUpperLimitLessThan15Command

        public ICommand GetOpticalPowerControlKpUpperLimitLessThan15Command
        {
            get
            {
                if (_GetOpticalPowerControlKpUpperLimitLessThan15Command == null)
                {
                    _GetOpticalPowerControlKpUpperLimitLessThan15Command = new RelayCommand(ExecuteGetOpticalPowerControlKpUpperLimitLessThan15Command, CanExecuteGetOpticalPowerControlKpUpperLimitLessThan15Command);
                }
                return _GetOpticalPowerControlKpUpperLimitLessThan15Command;
            }
        }
        public void ExecuteGetOpticalPowerControlKpUpperLimitLessThan15Command(object parameter)
        {

            Workspace.This.EthernetController.GetAllOpticalPowerControlKpUpperLimitLessThanOrEqual15();
            Thread.Sleep(1000);
            Workspace.This.EthernetController.GetAllOpticalPowerControlKpUpperLimitLessThanOrEqual15();
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "L")
            {
                GetOpticalPowerControlKpUpperLimitLessThan15 = EthernetController.AllGetOpticalPowerControlKpUpperLimitLessThanOrEqual15[LaserChannels.ChannelC];
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R1")
            {
                GetOpticalPowerControlKpUpperLimitLessThan15 = EthernetController.AllGetOpticalPowerControlKpUpperLimitLessThanOrEqual15[LaserChannels.ChannelA];
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R2")
            {
                GetOpticalPowerControlKpUpperLimitLessThan15 = EthernetController.AllGetOpticalPowerControlKpUpperLimitLessThanOrEqual15[LaserChannels.ChannelB];
            }
        }
        public bool CanExecuteGetOpticalPowerControlKpUpperLimitLessThan15Command(object parameter)
        {
            return true;
        }

        #endregion

        #region GetOpticalPowerControlKpDownLimitLessThan15Command

        public ICommand GetOpticalPowerControlKpDownLimitLessThan15Command
        {
            get
            {
                if (_GetOpticalPowerControlKpDownLimitLessThan15Command == null)
                {
                    _GetOpticalPowerControlKpDownLimitLessThan15Command = new RelayCommand(ExecuteGetOpticalPowerControlKpDownLimitLessThan15Command, CanExecuteGetOpticalPowerControlKpDownLimitLessThan15Command);
                }
                return _GetOpticalPowerControlKpDownLimitLessThan15Command;
            }
        }
        public void ExecuteGetOpticalPowerControlKpDownLimitLessThan15Command(object parameter)
        {

            Workspace.This.EthernetController.GetAllOpticalPowerControlKpDownLimitLessThanOrEqual15();
            Thread.Sleep(1000);
            Workspace.This.EthernetController.GetAllOpticalPowerControlKpDownLimitLessThanOrEqual15();
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "L")
            {
                GetOpticalPowerControlKpDownLimitLessThan15 = EthernetController.AllGetOpticalPowerControlKpDownLimitLessThanOrEqual15[LaserChannels.ChannelC];
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R1")
            {
                GetOpticalPowerControlKpDownLimitLessThan15 = EthernetController.AllGetOpticalPowerControlKpDownLimitLessThanOrEqual15[LaserChannels.ChannelA];
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R2")
            {
                GetOpticalPowerControlKpDownLimitLessThan15 = EthernetController.AllGetOpticalPowerControlKpDownLimitLessThanOrEqual15[LaserChannels.ChannelB];
            }
        }
        public bool CanExecuteGetOpticalPowerControlKpDownLimitLessThan15Command(object parameter)
        {
            return true;
        }

        #endregion

        #region GetOpticalPowerControlKiUpperLimitLessThan15Command

        public ICommand GetOpticalPowerControlKiUpperLimitLessThan15Command
        {
            get
            {
                if (_GetOpticalPowerControlKiUpperLimitLessThan15Command == null)
                {
                    _GetOpticalPowerControlKiUpperLimitLessThan15Command = new RelayCommand(ExecuteGetOpticalPowerControlKiUpperLimitLessThan15Command, CanExecuteGetOpticalPowerControlKiUpperLimitLessThan15Command);
                }
                return _GetOpticalPowerControlKiUpperLimitLessThan15Command;
            }
        }
        public void ExecuteGetOpticalPowerControlKiUpperLimitLessThan15Command(object parameter)
        {
            Workspace.This.EthernetController.GetAllOpticalPowerControlKiUpperLimitLessThanOrEqual15();
            Thread.Sleep(1000);
            Workspace.This.EthernetController.GetAllOpticalPowerControlKiUpperLimitLessThanOrEqual15(); 
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "L")
            {
                GetOpticalPowerControlKiUpperLimitLessThan15 = EthernetController.AllGetOpticalPowerControlKiUpperLimitLessThanOrEqual15[LaserChannels.ChannelC];
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R1")
            {
                GetOpticalPowerControlKiUpperLimitLessThan15 = EthernetController.AllGetOpticalPowerControlKiUpperLimitLessThanOrEqual15[LaserChannels.ChannelA];
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R2")
            {
                GetOpticalPowerControlKiUpperLimitLessThan15 = EthernetController.AllGetOpticalPowerControlKiUpperLimitLessThanOrEqual15[LaserChannels.ChannelB];
            }
        }
        public bool CanExecuteGetOpticalPowerControlKiUpperLimitLessThan15Command(object parameter)
        {
            return true;
        }

        #endregion

        #region GetOpticalPowerControlKiDownLimitLessThan15Command

        public ICommand GetOpticalPowerControlKiDownLimitLessThan15Command
        {
            get
            {
                if (_GetOpticalPowerControlKiDownLimitLessThan15Command == null)
                {
                    _GetOpticalPowerControlKiDownLimitLessThan15Command = new RelayCommand(ExecuteGetOpticalPowerControlKiDownLimitLessThan15Command, CanExecuteGetOpticalPowerControlKiDownLimitLessThan15Command);
                }
                return _GetOpticalPowerControlKiDownLimitLessThan15Command;
            }
        }
        public void ExecuteGetOpticalPowerControlKiDownLimitLessThan15Command(object parameter)
        {
            Workspace.This.EthernetController.GetAllOpticalPowerControlKiDownLimitLessThanOrEqual15();
            Thread.Sleep(1000);
            Workspace.This.EthernetController.GetAllOpticalPowerControlKiDownLimitLessThanOrEqual15();
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "L")
            {
                GetOpticalPowerControlKiDownLimitLessThan15 = EthernetController.AllGetOpticalPowerControlKiDownLimitLessThanOrEqual15[LaserChannels.ChannelC];
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R1")
            {
                GetOpticalPowerControlKiDownLimitLessThan15 = EthernetController.AllGetOpticalPowerControlKiDownLimitLessThanOrEqual15[LaserChannels.ChannelA];
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R2")
            {
                GetOpticalPowerControlKiDownLimitLessThan15 = EthernetController.AllGetOpticalPowerControlKiDownLimitLessThanOrEqual15[LaserChannels.ChannelB];
            }
        }
        public bool CanExecuteGetOpticalPowerControlKiDownLimitLessThan15Command(object parameter)
        {
            return true;
        }

        #endregion

        #region GetLaserMaxCurrentCommand

        public ICommand GetLaserMaxCurrentCommand
        {
            get
            {
                if (_GetLaserMaxCurrentCommand == null)
                {
                    _GetLaserMaxCurrentCommand = new RelayCommand(ExecuteGetLaserMaxCurrentCommand, CanExecuteGetLaserMaxCurrentCommand);
                }
                return _GetLaserMaxCurrentCommand;
            }
        }
        public void ExecuteGetLaserMaxCurrentCommand(object parameter)
        {
            Workspace.This.EthernetController.GetAllLaserMaximumCurrent();
            Thread.Sleep(1000);
            Workspace.This.EthernetController.GetAllLaserMaximumCurrent();
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "L")
            {
                GetLaserMaxCurrent = EthernetController.AllGetLaserMaximumCurrent[LaserChannels.ChannelC];
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R1")
            {
                GetLaserMaxCurrent = EthernetController.AllGetLaserMaximumCurrent[LaserChannels.ChannelA];
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R2")
            {
                GetLaserMaxCurrent = EthernetController.AllGetLaserMaximumCurrent[LaserChannels.ChannelB];
            }
        }
        public bool CanExecuteGetLaserMaxCurrentCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region GetLaserMinCurrentCommand

        public ICommand GetLaserMinCurrentCommand
        {
            get
            {
                if (_GetLaserMinCurrentCommand == null)
                {
                    _GetLaserMinCurrentCommand = new RelayCommand(ExecuteGetLaserMinCurrentCommand, CanExecuteGetLaserMinCurrentCommand);
                }
                return _GetLaserMinCurrentCommand;
            }
        }
        public void ExecuteGetLaserMinCurrentCommand(object parameter)
        {
            Workspace.This.EthernetController.GetAllLaserMinimumCurrent();
            Thread.Sleep(1000);
            Workspace.This.EthernetController.GetAllLaserMinimumCurrent();
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "L")
            {
                GetLaserMinCurrent = EthernetController.AllGetLaserMinimumCurrent[LaserChannels.ChannelC];
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R1")
            {
                GetLaserMinCurrent = EthernetController.AllGetLaserMinimumCurrent[LaserChannels.ChannelA];
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R2")
            {
                GetLaserMinCurrent = EthernetController.AllGetLaserMinimumCurrent[LaserChannels.ChannelB];
            }
        }
        public bool CanExecuteGetLaserMinCurrentCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region SetOpticalPowerLessThanOrEqual15mWKpCommand

        public ICommand SetOpticalPowerLessThanOrEqual15mWKpCommand
        {
            get
            {
                if (_SetOpticalPowerLessThanOrEqual15mWKpCommand == null)
                {
                    _SetOpticalPowerLessThanOrEqual15mWKpCommand = new RelayCommand(ExecuteSetOpticalPowerLessThanOrEqual15mWKpCommand, CanExecuteSetOpticalPowerLessThanOrEqual15mWKpCommand);
                }
                return _SetOpticalPowerLessThanOrEqual15mWKpCommand;
            }
        }
        public void ExecuteSetOpticalPowerLessThanOrEqual15mWKpCommand(object parameter)
        {
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "L")
            {
                Workspace.This.EthernetController.SetOpticalPowerLessThanOrEqual15mWKp(LaserChannels.ChannelC, SetOpticalPowerLessThanOrEqual15mWKp);
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R1")
            {
                Workspace.This.EthernetController.SetOpticalPowerLessThanOrEqual15mWKp(LaserChannels.ChannelA, SetOpticalPowerLessThanOrEqual15mWKp);
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R2")
            {
                Workspace.This.EthernetController.SetOpticalPowerLessThanOrEqual15mWKp(LaserChannels.ChannelB, SetOpticalPowerLessThanOrEqual15mWKp);
            }
        }
        public bool CanExecuteSetOpticalPowerLessThanOrEqual15mWKpCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region SetOpticalPowerLessThanOrEqual15mWKiCommand

        public ICommand SetOpticalPowerLessThanOrEqual15mWKiCommand
        {
            get
            {
                if (_SetOpticalPowerLessThanOrEqual15mWKiCommand == null)
                {
                    _SetOpticalPowerLessThanOrEqual15mWKiCommand = new RelayCommand(ExecuteSetOpticalPowerLessThanOrEqual15mWKiCommand, CanExecuteSetOpticalPowerLessThanOrEqual15mWKiCommand);
                }
                return _SetOpticalPowerLessThanOrEqual15mWKiCommand;
            }
        }
        public void ExecuteSetOpticalPowerLessThanOrEqual15mWKiCommand(object parameter)
        {
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "L")
            {
                Workspace.This.EthernetController.SetOpticalPowerLessThanOrEqual15mWKi(LaserChannels.ChannelC, SetOpticalPowerLessThanOrEqual15mWKi);
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R1")
            {
                Workspace.This.EthernetController.SetOpticalPowerLessThanOrEqual15mWKi(LaserChannels.ChannelA, SetOpticalPowerLessThanOrEqual15mWKi);
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R2")
            {
                Workspace.This.EthernetController.SetOpticalPowerLessThanOrEqual15mWKi(LaserChannels.ChannelB, SetOpticalPowerLessThanOrEqual15mWKi);
            }
        }
        public bool CanExecuteSetOpticalPowerLessThanOrEqual15mWKiCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region SetOpticalPowerLessThanOrEqual15mWKdCommand

        public ICommand SetOpticalPowerLessThanOrEqual15mWKdCommand
        {
            get
            {
                if (_SetOpticalPowerLessThanOrEqual15mWKdCommand == null)
                {
                    _SetOpticalPowerLessThanOrEqual15mWKdCommand = new RelayCommand(ExecuteSetOpticalPowerLessThanOrEqual15mWKdCommand, CanExecuteSetOpticalPowerLessThanOrEqual15mWKdCommand);
                }
                return _SetOpticalPowerLessThanOrEqual15mWKdCommand;
            }
        }
        public void ExecuteSetOpticalPowerLessThanOrEqual15mWKdCommand(object parameter)
        {
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "L")
            {
                Workspace.This.EthernetController.SetOpticalPowerLessThanOrEqual15mWKd(LaserChannels.ChannelC, SetOpticalPowerLessThanOrEqual15mWKd);
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R1")
            {
                Workspace.This.EthernetController.SetOpticalPowerLessThanOrEqual15mWKd(LaserChannels.ChannelA, SetOpticalPowerLessThanOrEqual15mWKd);
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R2")
            {
                Workspace.This.EthernetController.SetOpticalPowerLessThanOrEqual15mWKd(LaserChannels.ChannelB, SetOpticalPowerLessThanOrEqual15mWKd);
            }
        }
        public bool CanExecuteSetOpticalPowerLessThanOrEqual15mWKdCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region SetOpticalPowerGreaterThan15mWKpCommand

        public ICommand SetOpticalPowerGreaterThan15mWKpCommand
        {
            get
            {
                if (_SetOpticalPowerGreaterThan15mWKpCommand == null)
                {
                    _SetOpticalPowerGreaterThan15mWKpCommand = new RelayCommand(ExecuteSetOpticalPowerGreaterThan15mWKpCommand, CanExecuteSetOpticalPowerGreaterThan15mWKpCommand);
                }
                return _SetOpticalPowerGreaterThan15mWKpCommand;
            }
        }
        public void ExecuteSetOpticalPowerGreaterThan15mWKpCommand(object parameter)
        {
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "L")
            {
                Workspace.This.EthernetController.SetOpticalPowerGreaterThan15mWKp(LaserChannels.ChannelC, SetOpticalPowerGreaterThan15mWKp);
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R1")
            {
                Workspace.This.EthernetController.SetOpticalPowerGreaterThan15mWKp(LaserChannels.ChannelA, SetOpticalPowerGreaterThan15mWKp);
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R2")
            {
                Workspace.This.EthernetController.SetOpticalPowerGreaterThan15mWKp(LaserChannels.ChannelB, SetOpticalPowerGreaterThan15mWKp);
            }
        }
        public bool CanExecuteSetOpticalPowerGreaterThan15mWKpCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region SetOpticalPowerGreaterThan15mWKiCommand

        public ICommand SetOpticalPowerGreaterThan15mWKiCommand
        {
            get
            {
                if (_SetOpticalPowerGreaterThan15mWKiCommand == null)
                {
                    _SetOpticalPowerGreaterThan15mWKiCommand = new RelayCommand(ExecuteSetOpticalPowerGreaterThan15mWKiCommand, CanExecuteSetOpticalPowerGreaterThan15mWKiCommand);
                }
                return _SetOpticalPowerGreaterThan15mWKiCommand;
            }
        }
        public void ExecuteSetOpticalPowerGreaterThan15mWKiCommand(object parameter)
        {
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "L")
            {
                Workspace.This.EthernetController.SetOpticalPowerGreaterThan15mWKi(LaserChannels.ChannelC, SetOpticalPowerGreaterThan15mWKi);
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R1")
            {
                Workspace.This.EthernetController.SetOpticalPowerGreaterThan15mWKi(LaserChannels.ChannelA, SetOpticalPowerGreaterThan15mWKi);
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R2")
            {
                Workspace.This.EthernetController.SetOpticalPowerGreaterThan15mWKi(LaserChannels.ChannelB, SetOpticalPowerGreaterThan15mWKi);
            }
        }
        public bool CanExecuteSetOpticalPowerGreaterThan15mWKiCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region SetOpticalPowerGreaterThan15mWKdCommand

        public ICommand SetOpticalPowerGreaterThan15mWKdCommand
        {
            get
            {
                if (_SetOpticalPowerGreaterThan15mWKdCommand == null)
                {
                    _SetOpticalPowerGreaterThan15mWKdCommand = new RelayCommand(ExecuteSetOpticalPowerGreaterThan15mWKdCommand, CanExecuteSetOpticalPowerGreaterThan15mWKdCommand);
                }
                return _SetOpticalPowerGreaterThan15mWKdCommand;
            }
        }
        public void ExecuteSetOpticalPowerGreaterThan15mWKdCommand(object parameter)
        {
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "L")
            {
                Workspace.This.EthernetController.SetOpticalPowerGreaterThan15mWKd(LaserChannels.ChannelC, SetOpticalPowerGreaterThan15mWKd);
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R1")
            {
                Workspace.This.EthernetController.SetOpticalPowerGreaterThan15mWKd(LaserChannels.ChannelA, SetOpticalPowerGreaterThan15mWKd);
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R2")
            {
                Workspace.This.EthernetController.SetOpticalPowerGreaterThan15mWKd(LaserChannels.ChannelB, SetOpticalPowerGreaterThan15mWKd);
            }
        }
        public bool CanExecuteSetOpticalPowerGreaterThan15mWKdCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region SetOpticalPowerControlKpUpperLimitLessThanOrEqual15Command

        public ICommand SetOpticalPowerControlKpUpperLimitLessThanOrEqual15Command
        {
            get
            {
                if (_SetOpticalPowerControlKpUpperLimitLessThanOrEqual15Command == null)
                {
                    _SetOpticalPowerControlKpUpperLimitLessThanOrEqual15Command = new RelayCommand(ExecuteSetOpticalPowerControlKpUpperLimitLessThanOrEqual15Command, CanExecuteSetOpticalPowerControlKpUpperLimitLessThanOrEqual15Command);
                }
                return _SetOpticalPowerControlKpUpperLimitLessThanOrEqual15Command;
            }
        }
        public void ExecuteSetOpticalPowerControlKpUpperLimitLessThanOrEqual15Command(object parameter)
        {
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "L")
            {
                Workspace.This.EthernetController.SetOpticalPowerControlKpUpperLimitLessThan15(LaserChannels.ChannelC, SetOpticalPowerControlKpUpperLimitLessThanOrEqual15);
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R1")
            {
                Workspace.This.EthernetController.SetOpticalPowerControlKpUpperLimitLessThan15(LaserChannels.ChannelA, SetOpticalPowerControlKpUpperLimitLessThanOrEqual15);
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R2")
            {
                Workspace.This.EthernetController.SetOpticalPowerControlKpUpperLimitLessThan15(LaserChannels.ChannelB, SetOpticalPowerControlKpUpperLimitLessThanOrEqual15);
            }
        }
        public bool CanExecuteSetOpticalPowerControlKpUpperLimitLessThanOrEqual15Command(object parameter)
        {
            return true;
        }

        #endregion

        #region SetOpticalPowerControlKpDownLimitLessThanOrEqual15Command

        public ICommand SetOpticalPowerControlKpDownLimitLessThanOrEqual15Command
        {
            get
            {
                if (_SetOpticalPowerControlKpDownLimitLessThanOrEqual15Command == null)
                {
                    _SetOpticalPowerControlKpDownLimitLessThanOrEqual15Command = new RelayCommand(ExecuteSetOpticalPowerControlKpDownLimitLessThanOrEqual15Command, CanExecuteSetOpticalPowerControlKpDownLimitLessThanOrEqual15Command);
                }
                return _SetOpticalPowerControlKpDownLimitLessThanOrEqual15Command;
            }
        }
        public void ExecuteSetOpticalPowerControlKpDownLimitLessThanOrEqual15Command(object parameter)
        {
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "L")
            {
                Workspace.This.EthernetController.SetOpticalPowerControlKpDownLimitLessThan15(LaserChannels.ChannelC, SetOpticalPowerControlKpDownLimitLessThanOrEqual15);
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R1")
            {
                Workspace.This.EthernetController.SetOpticalPowerControlKpDownLimitLessThan15(LaserChannels.ChannelA, SetOpticalPowerControlKpDownLimitLessThanOrEqual15);
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R2")
            {
                Workspace.This.EthernetController.SetOpticalPowerControlKpDownLimitLessThan15(LaserChannels.ChannelB, SetOpticalPowerControlKpDownLimitLessThanOrEqual15);
            }

        }
        public bool CanExecuteSetOpticalPowerControlKpDownLimitLessThanOrEqual15Command(object parameter)
        {
            return true;
        }

        #endregion

        #region SetOpticalPowerControlKiUpperLimitLessThanOrEqual15Command

        public ICommand SetOpticalPowerControlKiUpperLimitLessThanOrEqual15Command
        {
            get
            {
                if (_SetOpticalPowerControlKiUpperLimitLessThanOrEqual15Command == null)
                {
                    _SetOpticalPowerControlKiUpperLimitLessThanOrEqual15Command = new RelayCommand(ExecuteSetOpticalPowerControlKiUpperLimitLessThanOrEqual15Command, CanExecuteSetOpticalPowerControlKiUpperLimitLessThanOrEqual15Command);
                }
                return _SetOpticalPowerControlKiUpperLimitLessThanOrEqual15Command;
            }
        }
        public void ExecuteSetOpticalPowerControlKiUpperLimitLessThanOrEqual15Command(object parameter)
        {
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "L")
            {
                Workspace.This.EthernetController.SetOpticalPowerControlKiUpperLimitLessThan15(LaserChannels.ChannelC, SetOpticalPowerControlKiUpperLimitLessThanOrEqual15);
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R1")
            {
                Workspace.This.EthernetController.SetOpticalPowerControlKiUpperLimitLessThan15(LaserChannels.ChannelA, SetOpticalPowerControlKiUpperLimitLessThanOrEqual15);
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R2")
            {
                Workspace.This.EthernetController.SetOpticalPowerControlKiUpperLimitLessThan15(LaserChannels.ChannelB, SetOpticalPowerControlKiUpperLimitLessThanOrEqual15);
            }

        }
        public bool CanExecuteSetOpticalPowerControlKiUpperLimitLessThanOrEqual15Command(object parameter)
        {
            return true;
        }

        #endregion

        #region SetOpticalPowerControlKiDownLimitLessThanOrEqual15Command

        public ICommand SetOpticalPowerControlKiDownLimitLessThanOrEqual15Command
        {
            get
            {
                if (_SetOpticalPowerControlKiDownLimitLessThanOrEqual15Command == null)
                {
                    _SetOpticalPowerControlKiDownLimitLessThanOrEqual15Command = new RelayCommand(ExecuteSetOpticalPowerControlKiDownLimitLessThanOrEqual15Command, CanExecuteSetOpticalPowerControlKiDownLimitLessThanOrEqual15Command);
                }
                return _SetOpticalPowerControlKiDownLimitLessThanOrEqual15Command;
            }
        }
        public void ExecuteSetOpticalPowerControlKiDownLimitLessThanOrEqual15Command(object parameter)
        {
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "L")
            {
                Workspace.This.EthernetController.SetOpticalPowerControlKiDownLimitLessThan15(LaserChannels.ChannelC, SetOpticalPowerControlKiDownLimitLessThanOrEqual15);
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R1")
            {
                Workspace.This.EthernetController.SetOpticalPowerControlKiDownLimitLessThan15(LaserChannels.ChannelA, SetOpticalPowerControlKiDownLimitLessThanOrEqual15);
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R2")
            {
                Workspace.This.EthernetController.SetOpticalPowerControlKiDownLimitLessThan15(LaserChannels.ChannelB, SetOpticalPowerControlKiDownLimitLessThanOrEqual15);
            }

        }
        public bool CanExecuteSetOpticalPowerControlKiDownLimitLessThanOrEqual15Command(object parameter)
        {
            return true;
        }

        #endregion

        #region SetOpticalPowerControlKpUpperLimitLessThan15Command

        public ICommand SetOpticalPowerControlKpUpperLimitLessThan15Command
        {
            get
            {
                if (_SetOpticalPowerControlKpUpperLimitLessThan15Command == null)
                {
                    _SetOpticalPowerControlKpUpperLimitLessThan15Command = new RelayCommand(ExecuteSetOpticalPowerControlKpUpperLimitLessThan15Command, CanExecuteSetOpticalPowerControlKpUpperLimitLessThan15Command);
                }
                return _SetOpticalPowerControlKpUpperLimitLessThan15Command;
            }
        }
        public void ExecuteSetOpticalPowerControlKpUpperLimitLessThan15Command(object parameter)
        {
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "L")
            {
                Workspace.This.EthernetController.SetOpticalPowerControlKpUpperLimitLessThanOrEqual15(LaserChannels.ChannelC, SetOpticalPowerControlKpUpperLimitLessThan15);
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R1")
            {
                Workspace.This.EthernetController.SetOpticalPowerControlKpUpperLimitLessThanOrEqual15(LaserChannels.ChannelA, SetOpticalPowerControlKpUpperLimitLessThan15);
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R2")
            {
                Workspace.This.EthernetController.SetOpticalPowerControlKpUpperLimitLessThanOrEqual15(LaserChannels.ChannelB, SetOpticalPowerControlKpUpperLimitLessThan15);
            }
        }
        public bool CanExecuteSetOpticalPowerControlKpUpperLimitLessThan15Command(object parameter)
        {
            return true;
        }

        #endregion

        #region SetOpticalPowerControlKpDownLimitLessThan15Command

        public ICommand SetOpticalPowerControlKpDownLimitLessThan15Command
        {
            get
            {
                if (_SetOpticalPowerControlKpDownLimitLessThan15Command == null)
                {
                    _SetOpticalPowerControlKpDownLimitLessThan15Command = new RelayCommand(ExecuteSetOpticalPowerControlKpDownLimitLessThan15Command, CanExecuteSetOpticalPowerControlKpDownLimitLessThan15Command);
                }
                return _SetOpticalPowerControlKpDownLimitLessThan15Command;
            }
        }
        public void ExecuteSetOpticalPowerControlKpDownLimitLessThan15Command(object parameter)
        {
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "L")
            {
                Workspace.This.EthernetController.SetOpticalPowerControlKpDownLimitLessThanOrEqual15(LaserChannels.ChannelC, SetOpticalPowerControlKpDownLimitLessThan15);
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R1")
            {
                Workspace.This.EthernetController.SetOpticalPowerControlKpDownLimitLessThanOrEqual15(LaserChannels.ChannelA, SetOpticalPowerControlKpDownLimitLessThan15);
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R2")
            {
                Workspace.This.EthernetController.SetOpticalPowerControlKpDownLimitLessThanOrEqual15(LaserChannels.ChannelB, SetOpticalPowerControlKpDownLimitLessThan15);
            }
        }
        public bool CanExecuteSetOpticalPowerControlKpDownLimitLessThan15Command(object parameter)
        {
            return true;
        }

        #endregion

        #region SetOpticalPowerControlKiUpperLimitLessThan15Command

        public ICommand SetOpticalPowerControlKiUpperLimitLessThan15Command
        {
            get
            {
                if (_SetOpticalPowerControlKiUpperLimitLessThan15Command == null)
                {
                    _SetOpticalPowerControlKiUpperLimitLessThan15Command = new RelayCommand(ExecuteSetOpticalPowerControlKiUpperLimitLessThan15Command, CanExecuteSetOpticalPowerControlKiUpperLimitLessThan15Command);
                }
                return _SetOpticalPowerControlKiUpperLimitLessThan15Command;
            }
        }
        public void ExecuteSetOpticalPowerControlKiUpperLimitLessThan15Command(object parameter)
        {
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "L")
            {
                Workspace.This.EthernetController.SetOpticalPowerControlKiUpperLimitLessThanOrEqual15(LaserChannels.ChannelC, SetOpticalPowerControlKiUpperLimitLessThan15);
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R1")
            {
                Workspace.This.EthernetController.SetOpticalPowerControlKiUpperLimitLessThanOrEqual15(LaserChannels.ChannelA, SetOpticalPowerControlKiUpperLimitLessThan15);
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R2")
            {
                Workspace.This.EthernetController.SetOpticalPowerControlKiUpperLimitLessThanOrEqual15(LaserChannels.ChannelB, SetOpticalPowerControlKiUpperLimitLessThan15);
            }
        }
        public bool CanExecuteSetOpticalPowerControlKiUpperLimitLessThan15Command(object parameter)
        {
            return true;
        }

        #endregion

        #region SetOpticalPowerControlKiDownLimitLessThan15Command

        public ICommand SetOpticalPowerControlKiDownLimitLessThan15Command
        {
            get
            {
                if (_SetOpticalPowerControlKiDownLimitLessThan15Command == null)
                {
                    _SetOpticalPowerControlKiDownLimitLessThan15Command = new RelayCommand(ExecuteSetOpticalPowerControlKiDownLimitLessThan15Command, CanExecuteSetOpticalPowerControlKiDownLimitLessThan15Command);
                }
                return _SetOpticalPowerControlKiDownLimitLessThan15Command;
            }
        }
        public void ExecuteSetOpticalPowerControlKiDownLimitLessThan15Command(object parameter)
        {
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "L")
            {
                Workspace.This.EthernetController.SetOpticalPowerControlKiDownLimitLessThanOrEqual15(LaserChannels.ChannelC, SetOpticalPowerControlKiDownLimitLessThan15);
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R1")
            {
                Workspace.This.EthernetController.SetOpticalPowerControlKiDownLimitLessThanOrEqual15(LaserChannels.ChannelA, SetOpticalPowerControlKiDownLimitLessThan15);
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R2")
            {
                Workspace.This.EthernetController.SetOpticalPowerControlKiDownLimitLessThanOrEqual15(LaserChannels.ChannelB, SetOpticalPowerControlKiDownLimitLessThan15);
            }
        }
        public bool CanExecuteSetOpticalPowerControlKiDownLimitLessThan15Command(object parameter)
        {
            return true;
        }

        #endregion

        #region SetLaserMaxCurrentCommand

        public ICommand SetLaserMaxCurrentCommand
        {
            get
            {
                if (_SetLaserMaxCurrentCommand == null)
                {
                    _SetLaserMaxCurrentCommand = new RelayCommand(ExecuteSetLaserMaxCurrentCommand, CanExecuteSetLaserMaxCurrentCommand);
                }
                return _SetLaserMaxCurrentCommand;
            }
        }
        public void ExecuteSetLaserMaxCurrentCommand(object parameter)
        {
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "L")
            {
                Workspace.This.EthernetController.SetLaserMaximumCurrent(LaserChannels.ChannelC, SetLaserMaxCurrent);
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R1")
            {
                Workspace.This.EthernetController.SetLaserMaximumCurrent(LaserChannels.ChannelA, SetLaserMaxCurrent);
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R2")
            {
                Workspace.This.EthernetController.SetLaserMaximumCurrent(LaserChannels.ChannelB, SetLaserMaxCurrent);
            }
        }
        public bool CanExecuteSetLaserMaxCurrentCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region SetLaserMinCurrentCommand

        public ICommand SetLaserMinCurrentCommand
        {
            get
            {
                if (_SetLaserMinCurrentCommand == null)
                {
                    _SetLaserMinCurrentCommand = new RelayCommand(ExecuteSetLaserMinCurrentCommand, CanExecuteSetLaserMinCurrentCommand);
                }
                return _SetLaserMinCurrentCommand;
            }
        }
        public void ExecuteSetLaserMinCurrentCommand(object parameter)
        {
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "L")
            {
                Workspace.This.EthernetController.SetLaserMinimumCurrent(LaserChannels.ChannelC, SetLaserMinCurrent);
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R1")
            {
                Workspace.This.EthernetController.SetLaserMinimumCurrent(LaserChannels.ChannelA, SetLaserMinCurrent);
            }
            if (Workspace.This.TolalPowerCliVm.Current532SelectChannel == "R2")
            {
                Workspace.This.EthernetController.SetLaserMinimumCurrent(LaserChannels.ChannelB, SetLaserMinCurrent);
            }
        }
        public bool CanExecuteSetLaserMinCurrentCommand(object parameter)
        {
            return true;
        }

        #endregion

    }
}
