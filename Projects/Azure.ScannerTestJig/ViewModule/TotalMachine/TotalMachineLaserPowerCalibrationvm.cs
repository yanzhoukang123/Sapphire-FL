using Azure.Configuration.Settings;
using Azure.EthernetCommLib;
using Azure.ImagingSystem;
using Azure.ScannerTestJig.View.TotalMachine;
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
    class TotalMachineLaserPowerCalibrationvm : ViewModelBase
    {
        #region private
        private ObservableCollection<LaserPower> _PowerOptionsL1 = null;
        private ObservableCollection<LaserPower> _PowerOptionsR1 = null;
        private ObservableCollection<LaserPower> _PowerOptionsR2 = null;
        private EthernetController _EthernetController;
        private string _Current532SelectChannelL1 = "NA";
        private string _WL1 = "NA";
        private string _WR1 = "NA";
        private string _WR2 = "NA";
        private bool _IsLaserL1Selected = false;
        private bool _IsLaserR1Selected = false;
        private bool _IsLaserR2Selected = false;
        private bool _IsNullEnabledL1 = true;
        private bool _IsNullEnabledR1 = true;
        private bool _IsNullEnabledR2 = true;
        private LaserPower _SelectedLaserPowerL1Module = null;
        private LaserPower _SelectedLaserPowerR1Module = null;
        private LaserPower _SelectedLaserPowerR2Module = null;
        private string _SensorTemperatureL1 = "0";
        private string _SensorTemperatureR1 = "0";
        private string _SensorTemperatureR2 = "0";
        private Thread ShowTemperatureTimer = null;
        private RelayCommand _LWriteCommand = null;
        private RelayCommand _R1WriteCommand = null;
        private RelayCommand _R2WriteCommand = null;
        private RelayCommand _532L_ConfigCommand = null;
        private RelayCommand _532R1_ConfigCommand = null;
        private RelayCommand _532R2_ConfigCommand = null;
        private int _LaserAIntensity = 0;
        private int _LaserBIntensity = 0;
        private int _LaserCIntensity = 0;
        private bool _TotalTest = false;

        #endregion
        public TotalMachineLaserPowerCalibrationvm(EthernetController ethernetController)
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
            //WL1 = "532"; WR1 = "784"; WR2 = "784";
            _PowerOptionsL1 = SettingsManager.ConfigSettings.LaserPowers;
            _PowerOptionsR1 = SettingsManager.ConfigSettings.LaserPowers;
            _PowerOptionsR2 = SettingsManager.ConfigSettings.LaserPowers;
            RaisePropertyChanged("LaserPowerModule");
            if (_PowerOptionsL1 != null && _PowerOptionsL1.Count >= 3)
            {
                SelectedLaserPowerL1Module = _PowerOptionsL1[0];
            }
            if (_PowerOptionsR1 != null && _PowerOptionsR1.Count >= 3)
            {
                SelectedLaserPowerR1Module = _PowerOptionsR1[0];
            }
            if (_PowerOptionsR2 != null && _PowerOptionsR2.Count >= 3)
            {
                SelectedLaserPowerR2Module = _PowerOptionsR2[0];
            }
            if (_PowerOptionsR1.Count<7)
            {
                LaserPower lp35 = new LaserPower();
                lp35.DisplayName = "35";
                lp35.Position = 6;
                lp35.Value = 35;
                LaserPower lp40 = new LaserPower();
                lp40.DisplayName = "40";
                lp40.Position = 7;
                lp40.Value = 40;
                LaserPower lp45 = new LaserPower();
                lp45.DisplayName = "45";
                lp45.Position = 8;
                lp45.Value = 45;
                LaserPower lp50 = new LaserPower();
                lp50.DisplayName = "50";
                lp50.Position = 9;
                lp50.Value = 50;
                _PowerOptionsL1.Add(lp35);
                _PowerOptionsL1.Add(lp40);
                _PowerOptionsL1.Add(lp45);
                _PowerOptionsL1.Add(lp50); 
            }
            if (_PowerOptionsR1.Count < 7)
            {
                LaserPower lp35 = new LaserPower();
                lp35.DisplayName = "35";
                lp35.Position = 6;
                lp35.Value = 35;
                LaserPower lp40 = new LaserPower();
                lp40.DisplayName = "40";
                lp40.Position = 7;
                lp40.Value = 40;
                LaserPower lp45 = new LaserPower();
                lp45.DisplayName = "45";
                lp45.Position = 8;
                lp45.Value = 45;
                LaserPower lp50 = new LaserPower();
                lp50.DisplayName = "50";
                lp50.Position = 9;
                lp50.Value = 50;
                _PowerOptionsR1.Add(lp35);
                _PowerOptionsR1.Add(lp40);
                _PowerOptionsR1.Add(lp45);
                _PowerOptionsR1.Add(lp50);
            }
            if (_PowerOptionsR2.Count < 7)
            {
                LaserPower lp35 = new LaserPower();
                lp35.DisplayName = "35";
                lp35.Position = 6;
                lp35.Value = 35;
                LaserPower lp40 = new LaserPower();
                lp40.DisplayName = "40";
                lp40.Position = 7;
                lp40.Value = 40;
                LaserPower lp45 = new LaserPower();
                lp45.DisplayName = "45";
                lp45.Position = 8;
                lp45.Value = 45;
                LaserPower lp50 = new LaserPower();
                lp50.DisplayName = "50";
                lp50.Position = 9;
                lp50.Value = 50;
                _PowerOptionsR2.Add(lp35);
                _PowerOptionsR2.Add(lp40);
                _PowerOptionsR2.Add(lp45);
                _PowerOptionsR2.Add(lp50);
            }
            TurnOffAllLasers();
            VisbleAndEnable();
            OnTemperatureInit();

        }
        public void TurnOffAllLasers()
        {
            IsLaserL1Selected = false;
            System.Threading.Thread.Sleep(200);
            IsLaserR1Selected = false;
            System.Threading.Thread.Sleep(200);
            IsLaserR2Selected = false;
        }
        private void VisbleAndEnable()
        {
            if (WL1 != "NA")
            {
                _IsNullEnabledL1 = true;
                RaisePropertyChanged("IsNullEnabledL1");
            }
            else
            {
                _IsNullEnabledL1 = false;
                RaisePropertyChanged("IsNullEnabledL1");

            }
            if (WR1 != "NA")
            {
                _IsNullEnabledR1 = true;
                RaisePropertyChanged("IsNullEnabledR1");
            }
            else
            {
                _IsNullEnabledR1 = false;
                RaisePropertyChanged("IsNullEnabledR1");

            }
            if (WR2 != "NA")
            {
                _IsNullEnabledR2 = true;
                RaisePropertyChanged("IsNullEnabledR2");
            }
            else
            {
                _IsNullEnabledR2 = false;
                RaisePropertyChanged("IsNullEnabledR2");
            }

        }
        #region  public
        #region LWriteCommand

        public ICommand LWriteCommand
        {
            get
            {
                if (_LWriteCommand == null)
                {
                    _LWriteCommand = new RelayCommand(ExecuteLWriteCommand, CanExecuteLWriteCommand);
                }
                return _LWriteCommand;
            }
        }
        public void ExecuteLWriteCommand(object parameter)
        {
            if (!Workspace.This.MotorVM.IsNewFirmware)
            {
                MessageBox.Show("网络未连接！", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            IsLaserL1Selected = false;
            EthernetDevice.SetLaserValuePower(LaserChannels.ChannelC, SelectedLaserPowerL1Module.Value, _LaserAIntensity);
            Thread.Sleep(100);
            EthernetDevice.GetLaserValueCurrent(LaserChannels.ChannelC, SelectedLaserPowerL1Module.Value);
            Thread.Sleep(100);
            EthernetDevice.GetLaserValueCurrent(LaserChannels.ChannelC, SelectedLaserPowerL1Module.Value);
            Thread.Sleep(100);
            GetACurrentValue = (int)EthernetDevice.AllIntensity[LaserChannels.ChannelC];
            GetACurrentValue = (int)EthernetDevice.AllIntensity[LaserChannels.ChannelC];
            IsLaserL1Selected = true;
            //switch (SelectedLaserPowerL1Module.Value)
            //{
            //    case 5:
            //        //if (TotalTest)
            //        //    Workspace.This.SerialPorts.SetLaserValuerContoll(SubDevice.Frock, Register.LaserValue5ma, _LaserAIntensity);
            //        //else

            //       // Workspace.This.SerialPorts.SetLaserValuerContoll(SubDevice.L, Register.LaserValue5ma, _LaserAIntensity);
            //        break;
            //    case 10:
            //        //if (TotalTest)
            //        //    Workspace.This.SerialPorts.SetLaserValuerContoll(SubDevice.Frock, Register.LaserValue10ma, _LaserAIntensity);
            //        //else
            //            Workspace.This.SerialPorts.SetLaserValuerContoll(SubDevice.L, Register.LaserValue10ma, _LaserAIntensity);
            //        break;
            //    case 15:
            //        //if (TotalTest)
            //        //    Workspace.This.SerialPorts.SetLaserValuerContoll(SubDevice.Frock, Register.LaserValue15ma, _LaserAIntensity);
            //        //else
            //            Workspace.This.SerialPorts.SetLaserValuerContoll(SubDevice.L, Register.LaserValue15ma, _LaserAIntensity);
            //        break;
            //    case 20:
            //        //if (TotalTest)
            //        //    Workspace.This.SerialPorts.SetLaserValuerContoll(SubDevice.Frock, Register.LaserValue20ma, _LaserAIntensity);
            //        //else
            //            Workspace.This.SerialPorts.SetLaserValuerContoll(SubDevice.L, Register.LaserValue20ma, _LaserAIntensity);
            //        break;
            //    case 25:
            //        //if (TotalTest)
            //        //    Workspace.This.SerialPorts.SetLaserValuerContoll(SubDevice.Frock, Register.LaserValue25ma, _LaserAIntensity);
            //        //else
            //            Workspace.This.SerialPorts.SetLaserValuerContoll(SubDevice.L, Register.LaserValue25ma, _LaserAIntensity);
            //        break;
            //    case 30:
            //        //if (TotalTest)
            //        //    Workspace.This.SerialPorts.SetLaserValuerContoll(SubDevice.Frock, Register.LaserValue30ma, _LaserAIntensity);
            //        //else
            //            Workspace.This.SerialPorts.SetLaserValuerContoll(SubDevice.L, Register.LaserValue30ma, _LaserAIntensity);
            //        break;
            //}

        }
        public bool CanExecuteLWriteCommand(object parameter)
        {
            return true;
        }

        #endregion
        #region R1WriteCommand

        public ICommand R1WriteCommand
        {
            get
            {
                if (_R1WriteCommand == null)
                {
                    _R1WriteCommand = new RelayCommand(ExecuteR1WriteCommand, CanExecuteR1WriteCommand);
                }

                return _R1WriteCommand;
            }
        }
        public void ExecuteR1WriteCommand(object parameter)
        {
            if (!Workspace.This.MotorVM.IsNewFirmware)
            {
                MessageBox.Show("网络未连接！", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            IsLaserR1Selected = false;
            EthernetDevice.SetLaserValuePower(LaserChannels.ChannelA, SelectedLaserPowerR1Module.Value, _LaserBIntensity);
            Thread.Sleep(100);
            EthernetDevice.GetLaserValueCurrent(LaserChannels.ChannelA, SelectedLaserPowerR1Module.Value);
            Thread.Sleep(100);
            EthernetDevice.GetLaserValueCurrent(LaserChannels.ChannelA, SelectedLaserPowerR1Module.Value);
            Thread.Sleep(100);
            GetBCurrentValue = (int)EthernetDevice.AllIntensity[LaserChannels.ChannelA];
            GetBCurrentValue = (int)EthernetDevice.AllIntensity[LaserChannels.ChannelA];
            IsLaserR1Selected = true;

            //switch (SelectedLaserPowerR1Module.Value)
            //{
            //    case 5:
            //        //if (TotalTest)
            //        //    Workspace.This.SerialPorts.SetLaserValuerContoll(SubDevice.Frock, Register.LaserValue5ma, _LaserBIntensity);
            //        //else
            //            Workspace.This.SerialPorts.SetLaserValuerContoll(SubDevice.R1, Register.LaserValue5ma, _LaserBIntensity);
            //        break;
            //    case 10:
            //        //if (TotalTest)
            //        //    Workspace.This.SerialPorts.SetLaserValuerContoll(SubDevice.Frock, Register.LaserValue10ma, _LaserBIntensity);
            //        //else
            //            Workspace.This.SerialPorts.SetLaserValuerContoll(SubDevice.R1, Register.LaserValue10ma, _LaserBIntensity);
            //        break;
            //    case 15:
            //        //if (TotalTest)
            //        //    Workspace.This.SerialPorts.SetLaserValuerContoll(SubDevice.Frock, Register.LaserValue15ma, _LaserBIntensity);
            //        //else
            //            Workspace.This.SerialPorts.SetLaserValuerContoll(SubDevice.R1, Register.LaserValue15ma, _LaserBIntensity);
            //        break;
            //    case 20:
            //        //if (TotalTest)
            //        //    Workspace.This.SerialPorts.SetLaserValuerContoll(SubDevice.Frock, Register.LaserValue20ma, _LaserBIntensity);
            //        //else
            //            Workspace.This.SerialPorts.SetLaserValuerContoll(SubDevice.R1, Register.LaserValue20ma, _LaserBIntensity);
            //        break;
            //    case 25:
            //        //if (TotalTest)
            //        //    Workspace.This.SerialPorts.SetLaserValuerContoll(SubDevice.Frock, Register.LaserValue25ma, _LaserBIntensity);
            //        //else
            //            Workspace.This.SerialPorts.SetLaserValuerContoll(SubDevice.R1, Register.LaserValue25ma, _LaserBIntensity);
            //        break;
            //    case 30:
            //        //if (TotalTest)
            //        //    Workspace.This.SerialPorts.SetLaserValuerContoll(SubDevice.Frock, Register.LaserValue30ma, _LaserBIntensity);
            //        //else
            //            Workspace.This.SerialPorts.SetLaserValuerContoll(SubDevice.R1, Register.LaserValue30ma, _LaserBIntensity);
            //        break;
            //}
        }
        public bool CanExecuteR1WriteCommand(object parameter)
        {
            return true;
        }

        #endregion
        #region R2WriteCommand

        public ICommand R2WriteCommand
        {
            get
            {
                if (_R2WriteCommand == null)
                {
                    _R2WriteCommand = new RelayCommand(ExecuteR2WriteCommand, CanExecuteR2WriteCommand);
                }

                return _R2WriteCommand;
            }
        }
        public void ExecuteR2WriteCommand(object parameter)
        {
            if (!Workspace.This.MotorVM.IsNewFirmware)
            {
                MessageBox.Show("网络未连接！", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            IsLaserR2Selected = false;
            EthernetDevice.SetLaserValuePower(LaserChannels.ChannelB, SelectedLaserPowerR2Module.Value, _LaserCIntensity);
            Thread.Sleep(100);
            EthernetDevice.GetLaserValueCurrent(LaserChannels.ChannelB, SelectedLaserPowerR2Module.Value);
            Thread.Sleep(100);
            EthernetDevice.GetLaserValueCurrent(LaserChannels.ChannelB, SelectedLaserPowerR2Module.Value);
            Thread.Sleep(100);
            GetCCurrentValue = (int)EthernetDevice.AllIntensity[LaserChannels.ChannelB];
            GetCCurrentValue = (int)EthernetDevice.AllIntensity[LaserChannels.ChannelB];
            IsLaserR2Selected = true;
            //switch (SelectedLaserPowerR2Module.Value)
            //{
            //    case 5:
            //        //if (TotalTest)
            //        //    Workspace.This.SerialPorts.SetLaserValuerContoll(SubDevice.Frock, Register.LaserValue5ma, _LaserCIntensity);
            //        //else
            //            Workspace.This.SerialPorts.SetLaserValuerContoll(SubDevice.R2, Register.LaserValue5ma, _LaserCIntensity);
            //        break;
            //    case 10:
            //        //if (TotalTest)
            //        //    Workspace.This.SerialPorts.SetLaserValuerContoll(SubDevice.Frock, Register.LaserValue10ma, _LaserCIntensity);
            //        //else
            //            Workspace.This.SerialPorts.SetLaserValuerContoll(SubDevice.R2, Register.LaserValue10ma, _LaserCIntensity);
            //        break;
            //    case 15:
            //        //if (TotalTest)
            //        //    Workspace.This.SerialPorts.SetLaserValuerContoll(SubDevice.Frock, Register.LaserValue15ma, _LaserCIntensity);
            //        //else
            //            Workspace.This.SerialPorts.SetLaserValuerContoll(SubDevice.R2, Register.LaserValue15ma, _LaserCIntensity);
            //        break;
            //    case 20:
            //        //if (TotalTest)
            //        //    Workspace.This.SerialPorts.SetLaserValuerContoll(SubDevice.Frock, Register.LaserValue20ma, _LaserCIntensity);
            //        //else
            //            Workspace.This.SerialPorts.SetLaserValuerContoll(SubDevice.R2, Register.LaserValue20ma, _LaserCIntensity);
            //        break;
            //    case 25:
            //        //if (TotalTest)
            //        //    Workspace.This.SerialPorts.SetLaserValuerContoll(SubDevice.Frock, Register.LaserValue25ma, _LaserCIntensity);
            //        //else
            //            Workspace.This.SerialPorts.SetLaserValuerContoll(SubDevice.R2, Register.LaserValue25ma, _LaserCIntensity);
            //        break;
            //    case 30:
            //        //if (TotalTest)
            //        //    Workspace.This.SerialPorts.SetLaserValuerContoll(SubDevice.Frock, Register.LaserValue30ma, _LaserCIntensity);
            //        //else
            //            Workspace.This.SerialPorts.SetLaserValuerContoll(SubDevice.R2, Register.LaserValue30ma, _LaserCIntensity);
            //        break;
            //}
        }
        public bool CanExecuteR2WriteCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region L_532Modul_ConfigCommand

        public ICommand L_532ConfigCommand
        {
            get
            {
                if (_532L_ConfigCommand == null)
                {
                    _532L_ConfigCommand = new RelayCommand(ExecuteL532_ConfigCommand, CanExecuteL532_ConfigCommand);
                }
                return _532L_ConfigCommand;
            }
        }
        public void ExecuteL532_ConfigCommand(object parameter)
        {
            if (WL1 == "532" && WL1 != "NA"|| "532-Propidium"== WL1 && WL1 != "NA")
            {
                Current532SelectChannel = "L";
                TotalMachine532LaserConfigWind _532LaserConfig = new TotalMachine532LaserConfigWind();
                _532LaserConfig.Title = "532模块参数配置";
                _532LaserConfig.ShowDialog();
            }
            else
            {
                MessageBox.Show("该模块不是532模块！", "", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
        public bool CanExecuteL532_ConfigCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region R1_532Modul_ConfigCommand

        public ICommand R1_532ConfigCommand
        {
            get
            {
                if (_532R1_ConfigCommand == null)
                {
                    _532R1_ConfigCommand = new RelayCommand(ExecuteR1532_ConfigCommand, CanExecuteR1532_ConfigCommand);
                }
                return _532R1_ConfigCommand;
            }
        }
        public void ExecuteR1532_ConfigCommand(object parameter)
        {
            if (WR1 == "532" && WR1 != "NA"|| "532-Propidium" == WR1 && WR1 != "NA")
            {
                Current532SelectChannel = "R1";
                TotalMachine532LaserConfigWind _532LaserConfig = new TotalMachine532LaserConfigWind();
                _532LaserConfig.Title = "532模块参数配置";
                _532LaserConfig.ShowDialog();
            }
            else
            {
                MessageBox.Show("该模块不是532模块！", "", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
        public bool CanExecuteR1532_ConfigCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region R2_532Modul_ConfigCommand

        public ICommand R2_532ConfigCommand
        {
            get
            {
                if (_532R2_ConfigCommand == null)
                {
                    _532R2_ConfigCommand = new RelayCommand(ExecuteR2532_ConfigCommand, CanExecuteR2532_ConfigCommand);
                }
                return _532R2_ConfigCommand;
            }
        }
        public void ExecuteR2532_ConfigCommand(object parameter)
        {
            if (WR2 == "532" && WR2 != "NA"|| "532-Propidium" == WR2 && WR2 != "NA")
            {
                Current532SelectChannel = "R2";
                TotalMachine532LaserConfigWind _532LaserConfig = new TotalMachine532LaserConfigWind();
                _532LaserConfig.Title = "532模块参数配置";
                _532LaserConfig.ShowDialog();
            }
            else
            {
                MessageBox.Show("该模块不是532模块！", "", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
        public bool CanExecuteR2532_ConfigCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region LaserPower
        public ObservableCollection<LaserPower> LaserPowerModuleL1
        {
            get { return _PowerOptionsL1; }
        }
        public ObservableCollection<LaserPower> LaserPowerModuleR1
        {
            get { return _PowerOptionsR1; }
        }
        public ObservableCollection<LaserPower> LaserPowerModuleR2
        {
            get { return _PowerOptionsR2; }
        }
        public LaserPower SelectedLaserPowerL1Module
        {
            get { return _SelectedLaserPowerL1Module; }
            set
            {
                if (_SelectedLaserPowerL1Module != value)
                {
                    _SelectedLaserPowerL1Module = value;
                    RaisePropertyChanged("SelectedLaserPowerL1Module");
                    Thread.Sleep(100);
                    EthernetDevice.GetLaserValueCurrent(LaserChannels.ChannelC, SelectedLaserPowerL1Module.Value);
                    Thread.Sleep(100);
                    EthernetDevice.GetLaserValueCurrent(LaserChannels.ChannelC, SelectedLaserPowerL1Module.Value);
                    Thread.Sleep(100);
                    try
                    {
                        GetACurrentValue = (int)EthernetDevice.AllIntensity[LaserChannels.ChannelC];
                        GetACurrentValue = (int)EthernetDevice.AllIntensity[LaserChannels.ChannelC];
                    }
                    catch { }
                }
            }
        }
        public LaserPower SelectedLaserPowerR1Module
        {
            get { return _SelectedLaserPowerR1Module; }
            set
            {
                if (_SelectedLaserPowerR1Module != value)
                {
                    _SelectedLaserPowerR1Module = value;
                    RaisePropertyChanged("SelectedLaserPowerR1Module");
                    Thread.Sleep(100);
                    EthernetDevice.GetLaserValueCurrent(LaserChannels.ChannelA, SelectedLaserPowerR1Module.Value);
                    Thread.Sleep(100);
                    EthernetDevice.GetLaserValueCurrent(LaserChannels.ChannelA, SelectedLaserPowerR1Module.Value);
                    Thread.Sleep(100);
                    try
                    {
                        GetBCurrentValue = (int)EthernetDevice.AllIntensity[LaserChannels.ChannelA];
                        GetBCurrentValue = (int)EthernetDevice.AllIntensity[LaserChannels.ChannelA];
                    }
                    catch { }
                }
            }
        }
        public LaserPower SelectedLaserPowerR2Module
        {
            get { return _SelectedLaserPowerR2Module; }
            set
            {
                if (_SelectedLaserPowerR2Module != value)
                {
                    _SelectedLaserPowerR2Module = value;
                    RaisePropertyChanged("SelectedLaserPowerR2Module");
                    Thread.Sleep(100);
                    EthernetDevice.GetLaserValueCurrent(LaserChannels.ChannelB, SelectedLaserPowerR2Module.Value);
                    Thread.Sleep(100);
                    EthernetDevice.GetLaserValueCurrent(LaserChannels.ChannelB, SelectedLaserPowerR2Module.Value);
                    Thread.Sleep(100);
                    try
                    {
                        GetCCurrentValue = (int)EthernetDevice.AllIntensity[LaserChannels.ChannelB];
                        GetCCurrentValue = (int)EthernetDevice.AllIntensity[LaserChannels.ChannelB];
                    }
                    catch { }
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
        public int LaserAIntensity
        {
            get { return _LaserAIntensity; }
            set
            {

                if (_LaserAIntensity != value)
                {
                    if (value >= 0 && value <= 15000)
                    {
                        _LaserAIntensity = value;
                        //MessageBox.Show("该值不能超过150或低于0");
                    }
                    else
                    {
                        if (value == 65535)
                        {
                            _LaserAIntensity = value;
                        }
                        else
                        {
                            return;
                        }
                    }
                    RaisePropertyChanged("LaserAIntensity");
                }
            }
        }
        public int LaserBIntensity
        {
            get { return _LaserBIntensity; }
            set
            {

                if (_LaserBIntensity != value)
                {
                    if (value >= 0 && value <= 15000)
                    {
                        _LaserBIntensity = value;
                        //MessageBox.Show("该值不能超过150或低于0");
                    }
                    else
                    {
                        if (value == 65535)
                        {
                            _LaserBIntensity = value;
                        }
                        else
                        {
                            return;
                        }
                    }
                    RaisePropertyChanged("LaserBIntensity");
                }
            }
        }
        public int LaserCIntensity
        {
            get { return _LaserCIntensity; }
            set
            {

                if (_LaserCIntensity != value)
                {
                    if (value >= 0 && value <= 15000)
                    {
                        _LaserCIntensity = value;
                        //MessageBox.Show("该值不能超过150或低于0");
                    }
                    else
                    {
                        if (value == 65535)
                        {
                            _LaserCIntensity = value;
                        }
                        else
                        {
                            return;
                        }
                    }
                    RaisePropertyChanged("LaserCIntensity");
                }
            }
        }
        public string WL1
        {
            get { return _WL1; }
            set
            {

                if (_WL1 != value)
                {
                    if (value == "4880")
                    {
                        _WL1 = "488-YFP";
                    }
                    else if (value == "5320")
                    {
                        _WL1 = "532-Propidium";
                    }
                    else
                    {
                        _WL1 = value;
                    }
                    RaisePropertyChanged("WL1");
                }
            }
        }
        public string WR1
        {
            get { return _WR1; }
            set
            {

                if (_WR1 != value)
                {
                    if (value == "4880")
                    {
                        _WR1 = "488-YFP";
                    }
                    else if (value == "5320")
                    {
                        _WR1 = "532-Propidium";
                    }
                    else
                    {
                        _WR1 = value;
                    }
                    RaisePropertyChanged("WR1");
                }
            }
        }
        public string WR2
        {
            get { return _WR2; }
            set
            {

                if (_WR2 != value)
                {
                    if (value == "4880")
                    {
                        _WR2 = "488-YFP";
                    }
                    else if (value == "5320")
                    {
                        _WR2 = "532-Propidium";
                    }
                    else
                    {
                        _WR2 = value;
                    }
                    RaisePropertyChanged("WR2");
                }
            }
        }

        public bool TotalTest
        {
            get { return _TotalTest; }
            set
            {
                _TotalTest = value;
                RaisePropertyChanged("TotalTest");
            }
        }

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
                    if (value == true)
                    {
                        if (Workspace.This.MotorVM.IsNewFirmware)
                        {
                            EthernetDevice.SetLaserPower(LaserChannels.ChannelC, _SelectedLaserPowerL1Module.Value);
                        }
                        else
                        {
                            MessageBox.Show("网络未连接！", "", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;

                        }
                        //if (Workspace.This.SerialPorts == null)
                        //{
                        //    MessageBox.Show("串口未连接！", "", MessageBoxButton.OK, MessageBoxImage.Error);
                        //    return;
                        //}
                        //if (TotalTest)
                        //{
                        //    Workspace.This.SerialPorts.SetLaserValuerContoll(SubDevice.Frock, Register.LaserValue, _LaserAIntensity);
                        //}
                        //else {

                        //    Workspace.This.SerialPorts.SetLaserValuerContoll(SubDevice.L, Register.LaserValue, _LaserAIntensity);
                        //}

                    }
                    else
                    {
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
                    if (value == true)
                    {
                        if (Workspace.This.MotorVM.IsNewFirmware)
                        {
                            EthernetDevice.SetLaserPower(LaserChannels.ChannelA, _SelectedLaserPowerR1Module.Value);
                        }
                        else
                        {

                            MessageBox.Show("网络未连接！", "", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        //if (Workspace.This.SerialPorts == null)
                        //{
                        //    MessageBox.Show("串口未连接！", "", MessageBoxButton.OK, MessageBoxImage.Error);
                        //    return;
                        //}
                        //if (TotalTest)
                        //{
                        //    Workspace.This.SerialPorts.SetLaserValuerContoll(SubDevice.Frock, Register.LaserValue, _LaserBIntensity);
                        //}
                        //else {
                        //    Workspace.This.SerialPorts.SetLaserValuerContoll(SubDevice.R1, Register.LaserValue, _LaserBIntensity);
                        //}

                    }
                    else
                    {
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
                    if (value == true)
                    {
                        if (Workspace.This.MotorVM.IsNewFirmware)
                        {
                            EthernetDevice.SetLaserPower(LaserChannels.ChannelB, _SelectedLaserPowerR2Module.Value);
                        }
                        else
                        {
                            MessageBox.Show("网络未连接！", "", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        //if (Workspace.This.SerialPorts == null)
                        //{
                        //    MessageBox.Show("串口未连接！", "", MessageBoxButton.OK, MessageBoxImage.Error);
                        //    return;
                        //}
                        //if (TotalTest)
                        //{

                        //    Workspace.This.SerialPorts.SetLaserValuerContoll(SubDevice.R2, Register.LaserValue, _LaserCIntensity);
                        //}
                        //else {

                        //    Workspace.This.SerialPorts.SetLaserValuerContoll(SubDevice.Frock, Register.LaserValue, _LaserCIntensity);
                        //}

                    }
                    else
                    {
                        EthernetDevice.SetLaserPower(LaserChannels.ChannelB, 0);
                    }
                }
            }
        }
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
        #region TemperatureTimerDelay
        //one sec ref Temperature
        private void OnTemperatureTime()
        {
            while (true)
            {
                Thread.Sleep(3000);
                EthernetDevice.GetAllLaserTemperatures();
                SensorTemperatureL1 = EthernetDevice.LaserTemperatures[LaserChannels.ChannelC].ToString();
                SensorTemperatureR1 = EthernetDevice.LaserTemperatures[LaserChannels.ChannelA].ToString();
                SensorTemperatureR2 = EthernetDevice.LaserTemperatures[LaserChannels.ChannelB].ToString();
            }
        }
        private void OnTemperatureInit()
        {
            ShowTemperatureTimer = new Thread(OnTemperatureTime);
            ShowTemperatureTimer.IsBackground = true;
            ShowTemperatureTimer.Start();
        }

        #endregion


        #region 电流值
        private int _GetACurrentValue = 0;
        private int _GetBCurrentValue = 0;
        private int _GetCCurrentValue = 0;
        public int GetACurrentValue
        {
            get { return _GetACurrentValue; }
            set
            {

                if (_GetACurrentValue != value)
                {
                    if (value == Workspace.This.Uint32Code)
                    {
                        value = 0;
                    }
                    _GetACurrentValue = value;
                    RaisePropertyChanged("GetACurrentValue");
                }
            }
        }
        public int GetBCurrentValue
        {
            get { return _GetBCurrentValue; }
            set
            {

                if (_GetBCurrentValue != value)
                {
                    if (value == Workspace.This.Uint32Code)
                    {
                        value = 0;
                    }
                    _GetBCurrentValue = value;
                    RaisePropertyChanged("GetBCurrentValue");
                }
            }
        }
        public int GetCCurrentValue
        {
            get { return _GetCCurrentValue; }
            set
            {
                if (_GetCCurrentValue != value)
                {
                    if (value == Workspace.This.Uint32Code)
                    {
                        value = 0;
                    }
                    _GetCCurrentValue = value;
                    RaisePropertyChanged("GetCCurrentValue");
                }
            }
        }

        public string Current532SelectChannel
        {
            get { return _Current532SelectChannelL1; }
            set
            {

                if (_Current532SelectChannelL1 != value)
                {
                    _Current532SelectChannelL1 = value;
                    RaisePropertyChanged("WL1");
                }
            }
        }
        #endregion
        #endregion
        #endregion
        #endregion
    }
}
