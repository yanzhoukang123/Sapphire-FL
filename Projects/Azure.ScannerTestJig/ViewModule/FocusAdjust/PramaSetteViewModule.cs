using Azure.CommandLib;
using Azure.Configuration.Settings;
using Azure.EthernetCommLib;
using Azure.ImagingSystem;
using Azure.WPF.Framework;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Azure.ScannerTestJig.ViewModule.FocusAdjust
{
    class PramaSetteViewModule : ViewModelBase
    {
        #region privert        

        private TestJigScanProcessing _ScanningProcess;
        private RelayCommand _ScanCommand = null;
        private RelayCommand _StopScanCommand = null;
        private RelayCommand _ScanLightGainCommand = null;
        private RelayCommand _HomeCommand = null;
        private EthernetController _EthernetController;
        private ObservableCollection<APDGainType> _APDGainOptions = null;
        private ObservableCollection<APDPgaType> _PGAOptionsModule = null;
        private ObservableCollection<LightGain> _LightGain = null;
        //private IvSensorController _IvSensorController;
        private Visibility _GainComVisbleL1 = Visibility.Visible;
        private Visibility _GainTxtVisbleL1 = Visibility.Visible;
        private Visibility _GainComVisbleR1 = Visibility.Visible;
        private Visibility _GainTxtVisbleR1 = Visibility.Visible;
        private Visibility _GainComVisbleR2 = Visibility.Visible;
        private Visibility _GainTxtVisbleR2 = Visibility.Visible;
        private LightGain _lightGainModule = null;
        private APDPgaType _SelectedPgaMModuleL1 = null;
        private APDPgaType _SelectedPgaMModuleR1 = null;
        private APDPgaType _SelectedPgaMModuleR2 = null;
        private APDGainType _SelectedGainComModuleL1 = null;
        private APDGainType _SelectedGainComModuleR1 = null;
        private APDGainType _SelectedGainComModuleR2 = null;
        private string _WL1 = "NA";
        private string _WR1 = "NA";
        private string _WR2 = "NA";
        int _TxtApdGainL1 = 500;
        int _TxtApdGainR1 = 500;
        int _TxtApdGainR2 = 500;
        private double _LaserAPower = 0;
        private double _LaserBPower = 0;
        private double _LaserCPower = 0;
        private bool _IsLaserL1Selected = false;
        private bool _IsLaserR1Selected = false;
        private bool _IsLaserR2Selected = false;
        private bool _IsNullEnabledL1 = false;
        private bool _IsNullEnabledR1 = false;
        private bool _IsNullEnabledR2 = false;
        private IvSensorType _SensorML1 = IvSensorType.NA;
        private IvSensorType _SensorMR1 = IvSensorType.NA;
        private IvSensorType _SensorMR2 = IvSensorType.NA;
        private string _SensorSNL1 = "L1";
        private string _SensorSNR1 = "R1";
        private string _SensorSNR2 = "R2";
        private string _LaserSNL1 = "L";
        private string _LaserSNR1 = "R1";
        private string _LaserSNR2 = "R2";
        private bool _IsReadOnly = true;
        private bool _MotorIsAlive = false;
        private double _XMotorSubdivision = 0;
        private double _YMotorSubdivision = 0;
        private double _ZMotorSubdivision = 5000;
        private Thread _FoustTopAdjThread = null;
        private int _XMaxValue = 0;
        private int _YMaxValue = 0;
        private double _ZMaxValue = 48000;
        private int _ScanX0 = 0;
        private int _ScanY0 = 0;
        private int _ScanZ0 = 0;
        private double _ScanDeltaX = 0;
        private double _ScanDeltaY = 0;
        private double _ScanDeltaZ = 0;
        private double _CurrentXPos = 0;
        private int _Width = 0;
        private int _Height = 0;
        int _ApdAValue = 0;
        int _ApdBValue = 0;
        int _ApdCValue = 0;
        private bool _IsScannerMode = true;
        private bool _IsStopMode = false;
        private bool _IsLightScannerMode = true;
        private bool _IsLightStopMode = false;
        private Visibility _IsScanVisibility = Visibility.Visible;
        private Visibility _IsStopVisibility = Visibility.Hidden;
        private Visibility _IsLightScanVisibility = Visibility.Visible;
        private Visibility _IsLightStopVisibility = Visibility.Hidden;
        private Visibility _IsGridPevkVisibility = Visibility.Visible;
        private double _ChLMaxValue;
        private double _ChR1MaxValue;
        private double _ChR2MaxValue;
        private double _MotorXSpeed = 20000;
        private double _MotorYSpeed = 3840;
        private double _MotorZSpeed = 0;
        private double _MotorXAccel = 8000000;
        private double _MotorYAccel = 8000000;
        private double _MotorZAccel = 1024000;
        private bool _FrockTest = false;
        private bool _IsLightEnabled = false;
        public Thread thread = null;
        #endregion

        #region Constructors...

        public PramaSetteViewModule(EthernetController ethernetController)
        {
            _EthernetController = ethernetController;
        }
        #endregion
        public void InitIVControls()
        {
            _APDGainOptions = SettingsManager.ConfigSettings.APDGains;
            _PGAOptionsModule = SettingsManager.ConfigSettings.APDPgas;
            _LightGain= SettingsManager.ConfigSettings.LightGains;
            RaisePropertyChanged("PGAOptionsModule");
            RaisePropertyChanged("GainComModule");
            RaisePropertyChanged("LightGainOption");
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
            if (_LightGain!=null&&_LightGain.Count>0) 
            {
                SelectedLightGain = _LightGain[0];
            }
            _IsNullEnabledL1 = true;
            _IsNullEnabledR1 = true;
            _IsNullEnabledR2 = true;
            //RaisePropertyChanged("IsNullEnabledL1");
            //RaisePropertyChanged("IsNullEnabledR1");
            //RaisePropertyChanged("IsNullEnabledR2");
            VisbleAndEnable();
            TurnOffAllLasers();//lasers off
            //_ResolutionOptions = SettingsManager.ConfigSettings.ResolutionOptions;
            _XMotorSubdivision = SettingsManager.ConfigSettings.XMotorSubdivision;
            _YMotorSubdivision = SettingsManager.ConfigSettings.YMotorSubdivision;
            _ZMotorSubdivision = SettingsManager.ConfigSettings.ZMotorSubdivision;
            _XMaxValue = SettingsManager.ConfigSettings.XMaxValue;
            _YMaxValue = SettingsManager.ConfigSettings.YMaxValue;
            _ZMaxValue = SettingsManager.ConfigSettings.ZMaxValue;
            // WL1 = "NA";WR1 = "NA"; WR2 = "NA";

            ScanDeltaZ = Math.Round(_ZMaxValue / _ZMotorSubdivision, 2);
            EnabledButton = false;
            thread = new Thread(HomeMotor);
            thread.IsBackground = true;
            thread.Start();
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
        /// <summary>
        /// visble and Enable
        /// </summary>
        private void VisbleAndEnable()
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
            //_GainComVisbleR2 = Visibility.Visible;
            //_GainTxtVisbleR2 = Visibility.Hidden;
            //RaisePropertyChanged("IsNullEnabledR2");
            //RaisePropertyChanged("GainComVisFlagR2");
            //RaisePropertyChanged("GainVisTxtFlagR2");
        }
        public EthernetController EthernetDevice
        {
            get
            {
                return _EthernetController;
            }
        }

        #region StopScanCommand

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
            if (_ScanningProcess != null)
            {
                //TurnOffAllLasers();
                //Enable Motor control
                _ScanningProcess.Abort();  // Abort the scanning thread
                if (Workspace.This.MotorVM.IsNewFirmware)
                {
                    Workspace.This.MotorVM.MotionController.SetStart(EthernetCommLib.MotorTypes.X | EthernetCommLib.MotorTypes.Y | EthernetCommLib.MotorTypes.Z,
                        new bool[] { false, false, false });
                }
            }
            if (IsLightGainModule)
            {
                IsLightScanning = true;
                IsLightStopning = false;

            }
            else {
                IsScanning = true;
                IsStopning = false;

            }
            TurnOffAllLasers();
        }
        public bool CanExecuteStopScanCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region ScanCommand

        public ICommand ScanCommand
        {
            get
            {
                if (_ScanCommand == null)
                {
                    _ScanCommand = new RelayCommand(ExecuteScanCommand, CanExecuteScanCommand);
                }

                return _ScanCommand;
            }
        }
        void HomeMotor()
        {
            bool _tempCurrent = true;
            if (SettingsManager.ConfigSettings.HomeMotionsAtLaunchTime)
            {
                if (!Workspace.This.MotorVM.HomeXYZmotor())
                {
                    //MessageBox.Show("无法回到Home位置，请检查连接", "", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                while (_tempCurrent)
                {
                    if (FrockTest)
                    {
                        Thread.Sleep(500);
                        if (Workspace.This.MotorVM.MotionController.CrntState[EthernetCommLib.MotorTypes.Z].AtHome)
                        {
                            EnabledButton = true;
                            _tempCurrent = false;
                        }
                        else
                        {

                            _tempCurrent = true;
                        }
                    }
                    else
                    {
                        Thread.Sleep(500);
                        if (Workspace.This.MotorVM.MotionController.CrntState[EthernetCommLib.MotorTypes.X].AtHome &&
                      Workspace.This.MotorVM.MotionController.CrntState[EthernetCommLib.MotorTypes.Y].AtHome &&
                      Workspace.This.MotorVM.MotionController.CrntState[EthernetCommLib.MotorTypes.Z].AtHome)
                        {
                            EnabledButton = true;
                            _tempCurrent = false;
                        }
                        else
                        {

                            _tempCurrent = true;
                        }
                    }
                }
            }
            else
            {
                _tempCurrent = false;
            }


        }
        void HomeMotor(int _tempScanx0,int _tempScany0)
        {
            bool _tempCurrent = true;
            if (SettingsManager.ConfigSettings.HomeMotionsAtLaunchTime)
            {
                if (!Workspace.This.MotorVM.HomeXYZmotor(_tempScanx0,_tempScany0))
                {
                    //MessageBox.Show("无法回到Home位置，请检查连接", "", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                while (_tempCurrent)
                {
                    int count = 0;
                    Thread.Sleep(500);
                    //if (Workspace.This.MotorVM.MotionController.CrntPositions[MotorTypes.X] != _tempScanx0)
                    //{
                    //    if (Workspace.This.MotorVM.MotionController.CrntState[Avocado.EthernetCommLib.MotorTypes.X].AtHome)
                    //    {
                    //        count++;
                    //    }
                    //}
                    //else
                    //{

                    //    count++;
                    //}
                    //if (Workspace.This.MotorVM.MotionController.CrntPositions[MotorTypes.Y] != _tempScany0)
                    //{
                    //    if (Workspace.This.MotorVM.MotionController.CrntState[Avocado.EthernetCommLib.MotorTypes.Y].AtHome)
                    //    {
                    //        count++;
                    //    }
                    //}
                    //else
                    //{
                    //    count++;

                    //}
                    if (Workspace.This.MotorVM.MotionController.CrntState[EthernetCommLib.MotorTypes.Z].AtHome)
                    {
                        count++;
                    }
                    if (count == 1)
                    {
                        _tempCurrent = false;
                    }
                    else
                    {
                        _tempCurrent = true;
                    }
                }
            }
            else
            {
                _tempCurrent = false;
            }


        }
        private void FoustTopThread() {
            int _tempScanx0, _tempScany0;
            if (!Workspace.This.InitControlValue())
            {
                MessageBox.Show("连接失败！", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            _tempScanx0 =(int)(SettingsManager.ConfigSettings.CentralCoordX * _XMotorSubdivision);
            if (Workspace.This.IsSelectGlassCM)
            {
                _tempScanx0 = (int)((SettingsManager.ConfigSettings.CentralCoordX + 120) * _XMotorSubdivision);
            }
            _tempScany0=(int)(SettingsManager.ConfigSettings.CentralCoordY * _YMotorSubdivision);
            if (IsLaserL1Selected || IsLaserR1Selected || IsLaserR2Selected)
            {
                if (!FrockTest)
                {

                    HomeMotor(_tempScanx0, _tempScany0);
                    Thread.Sleep(1000);
                    //HomeMotor(_tempScanx0, _tempScany0);
                }
                else
                {
                    bool _tempCurrent = true;
                    Workspace.This.MotorVM.ExecuteHomeCommand(MotorType.Z);
                    List<double> _tempCount = new List<double>();
                    while (_tempCurrent)
                    {
                        Thread.Sleep(1000);
                        if (Workspace.This.MotorVM.MotionController.CrntState[EthernetCommLib.MotorTypes.Z].AtHome)
                        {
                            _tempCurrent = false;
                        }
                        else
                        {
                            _tempCurrent = true;
                        }
                        _tempCount.Add(Workspace.This.MotorVM.CurrentZPos);
                        if (_tempCount.Count > 10)
                        {
                            if (_tempCount[0] == _tempCount[1] && _tempCount[0] == _tempCount[2] && _tempCount[0] == _tempCount[3] && _tempCount[0] == _tempCount[4] && _tempCount[0] == _tempCount[5] && _tempCount[0] == _tempCount[6] && _tempCount[0] == _tempCount[7] &&
                                _tempCount[0] == _tempCount[8] && _tempCount[0] == _tempCount[9])
                            {
                                MessageBoxResult boxResult = MessageBoxResult.None;
                                boxResult = MessageBox.Show("Z轴10秒移动位置未发生变化！，是否重新开始扫描！", "警告", MessageBoxButton.YesNo);
                                if (boxResult == MessageBoxResult.No)
                                {
                                    _tempCount.Clear();
                                    Workspace.This.MotorVM.ExecuteHomeCommand(MotorType.Z);
                                }
                                else
                                {
                                    _tempCount.Clear();
                                    return;
                                }
                            }
                            _tempCount.Clear();
                        }

                    }
                }
                if (IsLaserL1Selected)
                {
                    if (LaserAPower <= 0)
                    {

                        MessageBoxResult boxResult = MessageBoxResult.None;
                        boxResult = MessageBox.Show("L1通道的光功率为0！", "警告", MessageBoxButton.YesNo);
                        if (boxResult == MessageBoxResult.No)
                        {
                            IsLaserL1Selected = false;
                            return;
                        }
                    }
                }
                if (IsLaserR1Selected)
                {
                    if (LaserBPower <= 0)
                    {
                        MessageBoxResult boxResult = MessageBoxResult.None;
                        boxResult = MessageBox.Show("R1通道的光功率为0！", "警告", MessageBoxButton.YesNo);
                        if (boxResult == MessageBoxResult.No)
                        {
                            IsLaserR1Selected = false;
                            return;
                        }
                    }
                }
                if (IsLaserR2Selected)
                {
                    if (LaserCPower <= 0)
                    {
                        MessageBoxResult boxResult = MessageBoxResult.None;
                        boxResult = MessageBox.Show("R2通道的光功率为0！", "警告", MessageBoxButton.YesNo);
                        if (boxResult == MessageBoxResult.No)
                        {
                            IsLaserR2Selected = false;
                            return;
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("激光通道未打开！", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (Workspace.This.MotorVM.IsNewFirmware)
            {
                //Workspace.This.ZScanChartFocusVM.ChannelA = null;
                //Workspace.This.ZScanChartFocusVM.ChannelB = null;
                //Workspace.This.ZScanChartFocusVM.ChannelC = null;
                //Workspace.This.ZScanChartFocusVM.Light = null;
                // LaserOpen();
                TestJigScanParameterStruct scanParameter = new TestJigScanParameterStruct();  //Set scan parameter
                scanParameter.Width = 0;
                scanParameter.Height = 0;
                scanParameter.ScanDeltaX = 0;
                scanParameter.ScanDeltaY = 0;
                //if (_XMotorSubdivision > 0)
                //    _LimitsXPlus = Math.Round(_XMaxValue / _XMotorSubdivision, 2);
                //if (_YMotorSubdivision > 0)
                //    _LimitsYPlus = Math.Round(_YMaxValue / _YMotorSubdivision, 2);
                //if (_ZMotorSubdivision > 0)
                //    _LimitsZPlus = Math.Round(_ZMaxValue / _ZMotorSubdivision, 2);
                scanParameter.ScanDeltaZ = (int)_ScanDeltaZ;
                //scanParameter.ScanDeltaZ = (int)Math.Round(_ZMaxValue / _ZMotorSubdivision, 2); ;
                scanParameter.Res = 10;
                scanParameter.Quality = 1;
                scanParameter.DataRate = 1;
                scanParameter.LineCounts = 0;
                scanParameter.Time = 0;
                if (!FrockTest)//非工装调试
                {
                    scanParameter.ScanX0 = _tempScanx0;
                    scanParameter.ScanY0 = _tempScany0;
                }
                else
                {
                    scanParameter.ScanX0 = 0;
                    scanParameter.ScanY0 = 0;
                }
                scanParameter.ScanZ0 = _ScanZ0;
                scanParameter.IsLightGainModule = IsLightGainModule;
                scanParameter.LightGain = SelectedLightGain.Value;
                scanParameter.XMotorSubdivision = _XMotorSubdivision;
                scanParameter.YMotorSubdivision = _YMotorSubdivision;
                scanParameter.ZMotorSubdivision = _ZMotorSubdivision;
                scanParameter.ScanPreciseAt = SettingsManager.ConfigSettings.ScanPreciseAt;
                scanParameter.ScanPreciseParameter = SettingsManager.ConfigSettings.ScanPreciseParameter;
                scanParameter.XMotorSpeed = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[0].Speed * SettingsManager.ConfigSettings.XMotorSubdivision);
                scanParameter.XMotionAccVal = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[0].Accel * SettingsManager.ConfigSettings.XMotorSubdivision);
                scanParameter.XMotionDccVal = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[0].Dccel * SettingsManager.ConfigSettings.XMotorSubdivision);
                scanParameter.YMotorSpeed = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[1].Speed * SettingsManager.ConfigSettings.YMotorSubdivision);
                scanParameter.YMotionAccVal = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[1].Accel * SettingsManager.ConfigSettings.YMotorSubdivision);
                scanParameter.YMotionDccVal = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[1].Dccel * SettingsManager.ConfigSettings.YMotorSubdivision);
                scanParameter.ZMotorSpeed = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[2].Speed * SettingsManager.ConfigSettings.ZMotorSubdivision);
                scanParameter.ZMotionAccVal = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[2].Accel * SettingsManager.ConfigSettings.ZMotorSubdivision);
                scanParameter.ZMotionDccVal = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[2].Dccel * SettingsManager.ConfigSettings.ZMotorSubdivision);
                scanParameter.IsNewFirmwire = Workspace.This.MotorVM.IsNewFirmware;
                scanParameter.XmotionTurnAroundDelay = SettingsManager.ConfigSettings.XMotionTurnDelay;
                scanParameter.XMotionExtraMoveLength = (int)SettingsManager.ConfigSettings.XMotionExtraMoveLength;
                scanParameter.ZScanValueThreshold = SettingsManager.ConfigSettings.ZScanValueThreshold;
                scanParameter.IsGlassScan = false;
                scanParameter.IsFrockTest = FrockTest;
                _ScanningProcess = new TestJigScanProcessing(Workspace.This.EthernetController, Workspace.This.MotorVM.MotionController, scanParameter, null);
                _ScanningProcess.Completed += _ImageScanCommand_Completed;
                _ScanningProcess.OnScanDataReceived += _ScanningProcess_OnScanDataReceived;
                _ScanningProcess.Start();
                if (!IsLightGainModule)
                {
                    IsScanning = false;
                    IsStopning = true;
                }
                else
                {
                    IsLightScanning = false;
                    IsLightStopning = true;

                }
              
            }
            else
            {
                MessageBox.Show("请检查连接是否正常！", "");
                return;
            }
        }
        public void ExecuteScanCommand(object parameter)
        {
            _FoustTopAdjThread = new Thread(FoustTopThread);
            _FoustTopAdjThread.IsBackground = true;
            _FoustTopAdjThread.Start();

        }
        private void _ScanningProcess_OnScanDataReceived(string dataName)
        {

        }
        private void _ImageScanCommand_Completed(CommandLib.ThreadBase sender, CommandLib.ThreadBase.ThreadExitStat exitState)
        {
            Workspace.This.Owner.Dispatcher.BeginInvoke((Action)delegate
            {
                TestJigScanProcessing scannedThread = (sender as TestJigScanProcessing);

                if (exitState == ThreadBase.ThreadExitStat.None)
                {
                    //Workspace.This.ZScanChartFocusVM.ChannelA = null;
                    //Workspace.This.ZScanChartFocusVM.ChannelB = null;
                    //Workspace.This.ZScanChartFocusVM.ChannelC = null;
                    //Workspace.This.ZScanChartFocusVM.Light = null;
                    if (!IsLightGainModule)
                    {
                        IsScanning = true;
                        IsStopning = false;
                    }
                    else
                    {
                        IsLightScanning = true;
                        IsLightStopning = false;

                    }
                    scannedThread = (sender as TestJigScanProcessing);
                    if (scannedThread.ScanType == TestJigScanTypes.Static || scannedThread.ScanType == TestJigScanTypes.Vertical || scannedThread.ScanType == TestJigScanTypes.XAxis)
                    {
                        try
                        {
                            Point[] chA = new Point[scannedThread.SampleValueChannelA.Length];
                            Point[] chB = new Point[scannedThread.SampleValueChannelB.Length];
                            Point[] chC = new Point[scannedThread.SampleValueChannelC.Length];
                            double coeffX = 0;
                            double coeffChA = 1;
                            double coeffChB = 1;
                            double coeffChC = 1;
                            switch (scannedThread.ScanType)
                            {
                                case TestJigScanTypes.Static:
                                    // coeffX = DataRate;
                                    coeffChA = 1;
                                    coeffChB = 1;
                                    coeffChC = 1;
                                    break;
                                case TestJigScanTypes.Vertical:
                                    coeffX = 8 / SettingsManager.ConfigSettings.ZMotorSubdivision;
                                    coeffChA = 1;
                                    coeffChB = 1;
                                    coeffChC = 1;
                                    break;
                                case TestJigScanTypes.XAxis:
                                    coeffX = 1;
                                    coeffChA = -1 / SettingsManager.ConfigSettings.XMotorSubdivision;
                                    coeffChB = 1 / SettingsManager.ConfigSettings.XEncoderSubdivision;
                                    coeffChC = 1;
                                    break;
                            }
                            if (scannedThread.ScanType == TestJigScanTypes.XAxis)
                            {
                                for (int i = 0; i < chA.Length; i++)
                                {
                                    chA[i].X = scannedThread.SampleIndex[i] * coeffX;
                                    chA[i].Y = scannedThread.SampleValueChannelA[i] * coeffChA;
                                    chB[i].X = chA[i].X;
                                    chB[i].Y = scannedThread.SampleValueChannelB[i] * coeffChB;
                                    chC[i].X = chA[i].X;
                                    chC[i].Y = scannedThread.SampleValueChannelC[i] * coeffChC;
                                }
                            }
                            else 
                            {

                                int[] _tempA = MovingAverage(scannedThread.SampleValueChannelA, 5);
                                int[] _tempB = MovingAverage(scannedThread.SampleValueChannelB, 5);
                                int[] _tempC = MovingAverage(scannedThread.SampleValueChannelC, 5);
                                for (int i = 0; i < chA.Length; i++)
                                {
                                    chA[i].X = scannedThread.SampleIndex[i] * coeffX;
                                    chA[i].Y = _tempA[i] * coeffChA;
                                    chB[i].X = chA[i].X;
                                    chB[i].Y = _tempB[i] * coeffChB;
                                    chC[i].X = chA[i].X;
                                    chC[i].Y = _tempC[i] * coeffChC;
                                }
                                

                                Workspace.This.ZScanChartFocusVM.ChannelA = new EnumerableDataSource<Point>(chA);
                                Workspace.This.ZScanChartFocusVM.ChannelB = new EnumerableDataSource<Point>(chB);
                                Workspace.This.ZScanChartFocusVM.ChannelC = new EnumerableDataSource<Point>(chC);

                                if (SensorMR1 != IvSensorType.NA&&IsLaserR1Selected)//A
                                {
                                    // ChR1MaxValue = Math.Round(((TestJigScanProcessing)scannedThread).StaticChannelAMax.X, 3);
                                    double positionOfMinValue, valueMin;
                                    ThresholdProcess(chA,out positionOfMinValue,out valueMin);
                                    ChR1MaxValue=Math.Floor(positionOfMinValue * 1000) / 1000;
                                    //ChR1MaxValue = Math.Round(positionOfMinValue, 3);
                                    Workspace.This.ZScanChartFocusVM.ChannelA.SetXYMapping(p => p);
                                }
                                if (SensorMR2 != IvSensorType.NA && IsLaserR2Selected)//B
                                {
                                    //ChR2MaxValue = Math.Round(((TestJigScanProcessing)scannedThread).StaticChannelBMax.X, 3);
                                    double positionOfMinValue, valueMin;
                                    ThresholdProcess(chB, out positionOfMinValue, out valueMin);
                                    ChR2MaxValue = Math.Floor(positionOfMinValue * 1000) / 1000;
                                    //ChR2MaxValue = Math.Round(positionOfMinValue, 3);
                                    Workspace.This.ZScanChartFocusVM.ChannelB.SetXYMapping(p => p);
                                }
                                if (SensorML1 != IvSensorType.NA && IsLaserL1Selected)//C
                                {
                                    //ChLMaxValue = Math.Round(((TestJigScanProcessing)scannedThread).StaticChannelCMax.X, 3);
                                    //ChLMaxValue = Math.Round(positionOfMinValue, 3);
                                    Workspace.This.ZScanChartFocusVM.ChannelC.SetXYMapping(p => p);
                                    double positionOfMinValue, valueMin;
                                    ThresholdProcess(chC, out positionOfMinValue, out valueMin);
                                    ChLMaxValue = Math.Floor(positionOfMinValue * 1000) / 1000;
                                }
                                TurnOffAllLasers();
                                Workspace.This.ZScanChartFocusVM.SaveFocusData(@".\测试报告\Z轴调焦数据.csv");
                            }

                        }
                        catch
                        {

                        }

                    }
                    else
                    {
                        if (scannedThread.ScanType == TestJigScanTypes.LightGainVertical)
                        {
                            Point[] chLight = new Point[scannedThread.LightGainValueChannel0.Length];
                            double coeffX = 0; double coeffChLight = 1;
                            coeffX = 8 /SettingsManager.ConfigSettings.ZMotorSubdivision;
                            int[] _tempA = scannedThread.LightGainValueChannel0;
                            for (int i = 0; i < chLight.Length; i++)
                            {
                                chLight[i].X = scannedThread.SampleIndex[i] * coeffX;
                                chLight[i].Y = _tempA[i] * coeffChLight;
  
                            }
                            Workspace.This.ZScanChartFocusVM.Light = new EnumerableDataSource<Point>(chLight);
                            Workspace.This.ZScanChartFocusVM.Light.SetXYMapping(p => p);
                            Workspace.This.ZScanChartFocusVM.SaveLightData(@".\测试报告\Z轴光强校准数据.csv");
                            
                        }

                    }
                }
                else if (exitState == ThreadBase.ThreadExitStat.Error)
                {
                    // Oh oh something went wrong - handle the error
                    IsLightGainModule = false;
                    string caption = "Scanning error...";
                    string message = string.Format("Scanning error: {0}", scannedThread.Error.Message);
                    MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                IsLightGainModule = false;
                //Turn off all the lasers after scanning is completed
                TurnOffAllLasers();
                _ScanningProcess.Completed -= new CommandLib.ThreadBase.CommandCompletedHandler(_ImageScanCommand_Completed);
                //_ImageScanCommand.CommandStatus -= new ImageScanCommand.CommandStatusHandler(_ImageScanCommand_CommandStatus);
                _ScanningProcess = null;
            });

        }

        public bool CanExecuteScanCommand(object parameter)
        {
            return true;
        }

        public void ThresholdProcess(System.Windows.Point[] rawArray, out double positionOfMinValue, out double valueMin)
        {

            //`如果是小大小，就是波峰，大小大就是波谷
            //波峰比附近的值都大，波谷比附近的值都小。。。
            System.Windows.Point Data = new System.Windows.Point();
            List<System.Windows.Point> Datalist = new List<System.Windows.Point>();
            int direction = rawArray[0].Y > 0 ? -1 : 1;
            //找到所有峰值
            for (int i = 0; i < rawArray.Length - 1; i++)
            {
                if ((rawArray[i + 1].Y - rawArray[i].Y) * direction > 0)
                {
                    direction *= -1;
                    if (direction == 1)
                    {
                        Data.X = rawArray[i].X;
                        Data.Y = rawArray[i].Y;
                        Datalist.Add(Data);
                        Console.WriteLine("(" + rawArray[i].X + "," + rawArray[i].Y + ")" + "波峰");
                        //获取数据中多个波峰
                    }
                    //else
                    //{
                    //    positionOfMinValue = rawArray[i].X;
                    //    valueMin = rawArray[i].Y;
                    //    Console.WriteLine("(" + rawArray[i].X + "," + rawArray[i].Y + ")" + "波谷");
                    //    //获取数据中多个波谷
                    //}
                }
            }

            //找到两个最大峰值的下标
            double _positionOfMinValue = 0;
            double _valueMin = 65535;
            double larges = 0;
            double second = 0;
            double largestIndex = 0;
            double secondIndex = 0;
            foreach (System.Windows.Point i in Datalist)
            {
                if (larges < i.Y)
                {
                    larges = i.Y;
                    largestIndex = i.X;
                    Data = i;
                }
            }
            Datalist.Remove(Data);
            bool index = true;

            //移除相近的波峰，
            while (index)
            {
                index = false;
                for (int i = 0; i < Datalist.Count; i++)
                {
                    if (Math.Abs(Datalist[i].X - largestIndex) < 0.3)
                    {
                        Datalist.Remove(Datalist[i]);
                        index = true;
                    }

                }
            }

            foreach (System.Windows.Point i in Datalist)
            {
                if (second < i.Y)
                {
                    second = i.Y;
                    secondIndex = i.X;
                }
            }
            List<System.Windows.Point> LowEddlist = new List<System.Windows.Point>();
            //找寻这两个位置之间最低的值
            if (largestIndex < secondIndex)//最大波峰在前面
            {
                for (int i = 0; i < rawArray.Length - 1; i++)
                {

                    if (rawArray[i].X > largestIndex && rawArray[i].X < secondIndex)
                    {
                        if (_valueMin >= rawArray[i].Y)
                        {
                            _valueMin = rawArray[i].Y;
                            _positionOfMinValue = rawArray[i].X;
                        }
                        //Console.WriteLine("X: " + rawArray[i].X + "  Y: " + rawArray[i].Y);
                    }
                }
                //找到所有低谷的值
                for (int i = 0; i < rawArray.Length - 1; i++)
                {
                    if (rawArray[i].X > largestIndex && rawArray[i].X < secondIndex)
                    {
                        if (_valueMin == rawArray[i].Y)
                        {
                            LowEddlist.Add(rawArray[i]);
                        }
                        //Console.WriteLine("X: " + rawArray[i].X + "  Y: " + rawArray[i].Y);
                    }
                }
                if (LowEddlist.Count > 1)
                {
                    double startPos = LowEddlist[0].X;
                    double endPos = LowEddlist[LowEddlist.Count - 1].X;
                    _positionOfMinValue = (endPos - startPos) / 2 + startPos;
                }
                //Console.WriteLine("X: " + LowEddlist[0].X + "  Y: " + LowEddlist[0].Y);
                //Console.WriteLine("X: " + LowEddlist[LowEddlist.Count-1].X + "  Y: " + LowEddlist[LowEddlist.Count - 1].Y);
            }
            else
            {
                for (int i = 0; i < rawArray.Length - 1; i++)
                {
                    if (rawArray[i].X > secondIndex && rawArray[i].X < largestIndex)
                    {
                        if (_valueMin >= rawArray[i].Y)
                        {
                            _valueMin = rawArray[i].Y;
                            _positionOfMinValue = rawArray[i].X;
                        }
                        //Console.WriteLine("X: " + rawArray[i].X + "  Y: " + rawArray[i].Y);
                    }
                }
                //找到所有低谷的值
                for (int i = 0; i < rawArray.Length - 1; i++)
                {
                    if (rawArray[i].X > secondIndex && rawArray[i].X < largestIndex)
                    {
                        if (_valueMin == rawArray[i].Y)
                        {
                            LowEddlist.Add(rawArray[i]);
                        }
                        //Console.WriteLine("X: " + rawArray[i].X + "  Y: " + rawArray[i].Y);
                    }
                }
                if (LowEddlist.Count > 1)
                {
                    double startPos = LowEddlist[0].X;
                    double endPos = LowEddlist[LowEddlist.Count - 1].X;
                    _positionOfMinValue = (endPos - startPos) / 2 + startPos;
                }
                //Console.WriteLine("X: " + LowEddlist[0].X + "  Y: " + LowEddlist[0].Y);
                //Console.WriteLine("X: " + LowEddlist[LowEddlist.Count - 1].X + "  Y: " + LowEddlist[LowEddlist.Count - 1].Y);
            }
         


            positionOfMinValue = _positionOfMinValue;
            valueMin = _valueMin;
            //Console.WriteLine("X :" + positionOfMinValue);
            //Console.WriteLine("Y : " + valueMin);

        }

        #region MovingAverage

        public static int[] MovingAverage(int[] data, int span)
        {
            int b = 0;
            if (span % 2 == 1)
                b = (span - 1) / 2;
            else
            {
                span -= 1;
                b = (span - 1) / 2;
            }
            int[] smoothArray = new int[data.Length];
            if (data.Length > span)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    if (i < b)
                    {
                        smoothArray[i] = 0;
                        for (int j = -i; j < i + 1; j++)
                        {
                            smoothArray[i] += data[i + j];
                        }
                        smoothArray[i] /= (2 * i + 1);
                    }
                    else if (i >= b && (data.Length - i) > b)
                    {
                        smoothArray[i] = 0;
                        for (int j = -b; j < b + 1; j++)
                        {
                            smoothArray[i] += data[i + j];
                        }
                        smoothArray[i] /= span;
                    }
                    else
                    {
                        smoothArray[i] = 0;
                        int c = data.Length - i - 1;
                        for (int j = -c; j < c + 1; j++)
                        {
                            smoothArray[i] += data[i + j];
                        }
                        smoothArray[i] /= (2 * c + 1);

                    }
                }
            }
            else
            {
                throw new ArgumentException("span is bigger than data's count");

            }

            return smoothArray;
        }
        #endregion
        #endregion

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
            //
            //TODO: implement the command
            //

            MotorType motorType = (MotorType)parameter;
            if (motorType == MotorType.X)
            {
                //TODO: home the X motor
                //MessageBox.Show("TODO: home the X motor...");
                if (Workspace.This.MotorVM.IsNewFirmware)
                {
                    //Workspace.This.ApdVM.APDTransfer.MotionControl.HomeMotion(Hats.APDCom.MotionTypes.X, 256, (int)_MotorXSpeed, (int)_MotorXAccel, (int)_MotorXAccel, false);
                    Workspace.This.MotorVM.MotionController.HomeMotion(EthernetCommLib.MotorTypes.X,
                        new int[] { 256 }, new int[] { (int)_MotorXSpeed }, new int[] { (int)_MotorXAccel }, new int[] { (int)_MotorXAccel }, false);
                }
            }
            else if (motorType == MotorType.Y)
            {
                //TODO: home the Y motor
                //MessageBox.Show("TODO: home the Y motor...");
                if (Workspace.This.MotorVM.IsNewFirmware)
                {
                    //Workspace.This.ApdVM.APDTransfer.MotionControl.HomeMotion(Hats.APDCom.MotionTypes.Y, 256, (int)_MotorYSpeed, (int)_MotorYAccel, (int)_MotorYAccel, false);
                    Workspace.This.MotorVM.MotionController.HomeMotion(EthernetCommLib.MotorTypes.Y,
                        new int[] { 256 }, new int[] { (int)_MotorYSpeed }, new int[] { (int)_MotorYAccel }, new int[] { (int)_MotorYAccel }, false);
                }
            }
            else if (motorType == MotorType.Z)
            {
                //TODO: home the Z motor
                //MessageBox.Show("TODO: home the Z motor...");
                if (Workspace.This.MotorVM.IsNewFirmware)
                {
                    int testZMotorSpeed = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[2].Speed * SettingsManager.ConfigSettings.ZMotorSubdivision);
                    int testZMotionAccVal = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[2].Accel * SettingsManager.ConfigSettings.ZMotorSubdivision);
                    int testZMotionDccVal = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[2].Dccel * SettingsManager.ConfigSettings.ZMotorSubdivision);
                    //Workspace.This.ApdVM.APDTransfer.MotionControl.HomeMotion(Hats.APDCom.MotionTypes.Z, 256, (int)_MotorZSpeed, (int)_MotorZAccel, (int)_MotorZDccel, false);
                    Workspace.This.MotorVM.MotionController.HomeMotion(EthernetCommLib.MotorTypes.Z,
               new int[] { 256 }, new int[] { (int)testZMotorSpeed }, new int[] { (int)testZMotionAccVal }, new int[] { (int)testZMotionDccVal }, false);
                }
            }
            //else if (motorType == MotorType.W)
            //{
            //    //TODO: home the Z motor
            //    //MessageBox.Show("TODO: home the Z motor...");
            //    if (Workspace.This.MotorVM.IsNewFirmware)
            //    {
            //        //Workspace.This.ApdVM.APDTransfer.MotionControl.HomeMotion(Hats.APDCom.MotionTypes.W, 256, (int)_MotorWSpeed, (int)_MotorWAccel, (int)_MotorWAccel, false);
            //        Workspace.This.MotorVM.MotionController.HomeMotion(Avocado.EthernetCommLib.MotorTypes.W,
            //   new int[] { 256 }, new int[] { (int)_MotorWSpeed }, new int[] { (int)_MotorWAccel }, new int[] { (int)_MotorWAccel }, false);
            //    }
            //}
        }

        public bool CanExecuteHomeCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region GoAbsPosCommand
        private RelayCommand _GoAbsPosCommand = null;
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
            MotorType motorType = (MotorType)parameter;
            if (motorType == MotorType.Z)
            {
                if (AbsZPos > _LimitsZMinus || _AbsZPos <= _ZMaxValue)
                {
                    if (Workspace.This.MotorVM.IsNewFirmware)
                    {
                        int testZMotorSpeed = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[2].Speed * SettingsManager.ConfigSettings.ZMotorSubdivision);
                        int testZMotionAccVal = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[2].Accel * SettingsManager.ConfigSettings.ZMotorSubdivision);
                        int testZMotionDccVal = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[2].Dccel * SettingsManager.ConfigSettings.ZMotorSubdivision);
                        if (Workspace.This.MotorVM.MotionController.AbsoluteMoveSingleMotion(EthernetCommLib.MotorTypes.Z,
        256, (int)testZMotorSpeed, (int)testZMotionAccVal, (int)testZMotionDccVal, (int)_AbsZPos, true, false) == false)
                        {
                            MessageBox.Show("Failed to Set new position");
                        }
                    }
                    else
                    {
                        MessageBox.Show("请检查网络连接！");
                        return;
                    }
                }
            }
        }

        public bool CanExecuteGoAbsPosCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region  GainLight
        #region  ScanLightGainCommand
        public ICommand ScanLigthCommand
        {
            get
            {
                if (_ScanLightGainCommand == null)
                {
                    _ScanLightGainCommand = new RelayCommand(ExecuteScanLightScanScanCommand, CanExecuteScanLightScanCommand);
                }

                return _ScanLightGainCommand;
            }
        }
        public void ExecuteScanLightScanScanCommand(object parameter)
        {
           IsLightGainModule = true;
            ExecuteScanCommand(null);
        }
        public bool CanExecuteScanLightScanCommand(object parameter)
        {
            return true;
        }
        #endregion

        #endregion

        #region Observe Data Command
        private RelayCommand _ObserveDataCommand = null;
        public ICommand ObserveDataCommand
        {
            get
            {
                if (_ObserveDataCommand == null)
                {
                    _ObserveDataCommand = new RelayCommand(ExcuteObserveDataCommand, CanExcuteObserveDataCommand);
                }
                return _ObserveDataCommand;
            }
        }
        public void ExcuteObserveDataCommand(object parameter)
        {
            //if (_TestFocusAdjust.IsChecked)
            {
                try
                {
                    FileInfo fileInfo = new FileInfo(@".\测试报告");
                    Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
                    Microsoft.Office.Interop.Excel.Workbook book = excel.Application.Workbooks.Add(fileInfo.FullName);
                    excel.Visible = true;
                }
                catch
                {
                    if (Directory.Exists(@".\测试报告"))
                    {
                        System.Diagnostics.Process.Start("Explorer.exe", @".\测试报告");
                    }
                }
            }
        }
        public bool CanExcuteObserveDataCommand(object parameter)
        {
            return true;
        }
        #endregion Observe Data Command

        #region  Public pre

        #region  MaxValue
        public double ChLMaxValue
        {
            get { return _ChLMaxValue; }
            set
            {

                if (_ChLMaxValue != value)
                {
                    _ChLMaxValue = value;
                    RaisePropertyChanged("ChLMaxValue");
                }
            }
        }
        public double ChR1MaxValue
        {
            get { return _ChR1MaxValue; }
            set
            {

                if (_ChR1MaxValue != value)
                {
                    _ChR1MaxValue = value;
                    RaisePropertyChanged("ChR1MaxValue");
                }
            }
        }
        public double ChR2MaxValue
        {
            get { return _ChR2MaxValue; }
            set
            {

                if (_ChR2MaxValue != value)
                {
                    _ChR2MaxValue = value;
                    RaisePropertyChanged("ChR2MaxValue");
                }
            }
        }
        #endregion
        public int ApdAValue
        {
            get { return _ApdAValue; }
            set
            {
                if (_ApdAValue != value)
                {
                    _ApdAValue = value;
                    RaisePropertyChanged("ApdAValue");
                }
            }
        }
        public int ApdBValue
        {
            get { return _ApdBValue; }
            set
            {
                if (_ApdBValue != value)
                {
                    _ApdBValue = value;
                    RaisePropertyChanged("ApdBValue");
                }
            }
        }
        public int ApdCValue
        {
            get { return _ApdCValue; }
            set
            {
                if (_ApdCValue != value)
                {
                    _ApdCValue = value;
                    RaisePropertyChanged("ApdCValue");
                }
            }
        }
        public Visibility IsGridPevkVisibility
        {
            get { return _IsGridPevkVisibility; }
            set
            {
                if (_IsGridPevkVisibility != value)
                {
                    _IsGridPevkVisibility = value;
                    RaisePropertyChanged("IsScanVisibility");

                }
            }
        }

        public Visibility IsScanVisibility
        {
            get { return _IsScanVisibility; }
            set
            {
                if (_IsScanVisibility != value)
                {
                    _IsScanVisibility = value;
                    RaisePropertyChanged("IsScanVisibility");

                }
            }
        }
        public Visibility IsStopVisibility
        {
            get { return _IsStopVisibility; }
            set
            {
                if (_IsStopVisibility != value)
                {
                    _IsStopVisibility = value;
                    RaisePropertyChanged("IsStopVisibility");
                }
            }
        }
        public bool IsScanning
        {
            get { return _IsScannerMode; }
            set
            {
                if (_IsScannerMode != value)
                {
                    _IsScannerMode = value;
                    if (_IsScannerMode)
                    {
                        IsScanVisibility = Visibility.Visible;
                    }
                    else
                    {
                        IsScanVisibility = Visibility.Hidden;
                    }
                }
            }
        }

        public bool IsStopning
        {
            get { return _IsStopMode; }
            set
            {
                if (_IsStopMode != value)
                {
                    _IsStopMode = value;
                    if (_IsStopMode)
                    {
                        IsStopVisibility = Visibility.Visible;
                    }
                    else
                    {
                        IsStopVisibility = Visibility.Hidden;
                    }
                }
            }
        }

        public double ScanZ0
        {
            get
            {
                double dRetVal = 0;
                if (_ZMotorSubdivision != 0)
                {
                    dRetVal = Math.Round((double)_ScanZ0 / (double)_ZMotorSubdivision, 3);
                }
                return dRetVal;
            }
            set
            {
                if ((double)_ScanZ0 / (double)_ZMotorSubdivision != value)
                {
                    if (value >= 0 && value <= ((double)_ZMaxValue / (double)_ZMotorSubdivision))
                    {
                        _ScanZ0 = (int)(value * _ZMotorSubdivision);
                    }
                    else
                    {
                        _ScanZ0 = (int)_ZMaxValue;
                        MessageBox.Show(String.Format("you should type value 0-{0}", (double)_ZMaxValue / (double)_ZMotorSubdivision), "Error");
                    }
                    RaisePropertyChanged("ScanZ0");
                }
            }
        }

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
                    if ((int)value == Workspace.This.Uint8Code)
                    {
                        value = IvSensorType.NA;
                    }
                    _SensorML1 = value;
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
                    if ((int)value == Workspace.This.Uint8Code)
                    {
                        value = IvSensorType.NA;
                    }
                    _SensorMR1 = value;
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
                    if ((int)value == Workspace.This.Uint8Code)
                    {
                        value = IvSensorType.NA;
                    }
                    _SensorMR2 = value;
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
                    if (value == Workspace.This.StrEmptyCode1)
                    {
                        value = "NaN";
                    }
                    _SensorSNL1 = value;
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
                    if (value == Workspace.This.StrEmptyCode1)
                    {
                        value = "NaN";
                    }
                    _SensorSNR1 = value;
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
                    if (value == Workspace.This.StrEmptyCode1)
                    {
                        value = "NaN";
                    }
                    _SensorSNR2 = value;
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
                    if (value ==Workspace.This.StrEmptyCode1)
                    {
                        value = "NaN";
                    }
                    _LaserSNL1 = value;
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
                    if (value == Workspace.This.StrEmptyCode1)
                    {
                        value = "NaN";
                    }
                    _LaserSNR1 = value;
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
                    if (value == Workspace.This.StrEmptyCode1)
                    {
                        value = "NaN";
                    }
                    _LaserSNR2 = value;
                }
            }
        }
        #endregion

        #region WL
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
                    if (value == true)
                    {
                        EthernetDevice.SetLaserPower(LaserChannels.ChannelC, LaserAPower);
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
                        EthernetDevice.SetLaserPower(LaserChannels.ChannelA, LaserBPower);
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
                        EthernetDevice.SetLaserPower(LaserChannels.ChannelB, LaserCPower);
                    }
                    else
                    {
                        EthernetDevice.SetLaserPower(LaserChannels.ChannelB, 0);
                    }
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

        #region LightGain
        public ObservableCollection<LightGain> LightGainOption
        {
            get { return _LightGain; }
        }
        public LightGain SelectedLightGain
        {
            get { return _lightGainModule; }
            set
            {
                if (_lightGainModule != value)
                {
                    _lightGainModule = value;
                    RaisePropertyChanged("SelectedLightGain");
                }
            }
        }
        #endregion

        public double ScanDeltaZ
        {
            get
            {
                double dRetVal = 0.0;
                if (_ZMotorSubdivision != 0)
                {
                    dRetVal = Math.Round((double)_ScanDeltaZ / (double)_ZMotorSubdivision, 3);
                }
                return dRetVal;
            }
            set
            {
                if (((double)_ScanDeltaZ / (double)_ZMotorSubdivision) != value)
                {
                    if (value >= 0 && ((value + 0) <= ((double)_ZMaxValue / (double)_ZMotorSubdivision)))
                    {
                        _ScanDeltaZ = (int)(value * _ZMotorSubdivision);
                    }
                    else
                    {
                        _ScanDeltaZ = Math.Round((double)_ScanDeltaZ / (double)_ZMotorSubdivision, 3);
                        //string format = string.Format("The DZ should be 0=<Z0+DZ<={0}", (double)_ZMaxValue / (double)_ZMotorSubdivision);
                        //throw new Exception(format);
                    }
                    RaisePropertyChanged("ScanDeltaZ");
                }
            }
        }

        private double _AbsZPos = 0;
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


        private double _LimitsZMinus = -2;
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
        public bool FrockTest
        {
            get { return _FrockTest; }
            set
            {
                _FrockTest = value;
                if (value)
                {
                    IsLightEnabled = true;
                }
                else {
                    IsLightEnabled = false;

                }
                RaisePropertyChanged("FrockTest");
            }
        }

        public bool IsLightEnabled
        {
            get { return _IsLightEnabled; }
            set
            {
                _IsLightEnabled = value;
                RaisePropertyChanged("IsLightEnabled");
            }
        }

        public   void LaserOpen()
        {
            IsLaserL1Selected = false;
            Thread.Sleep(500);
            IsLaserR1Selected = false;
            Thread.Sleep(500);
            IsLaserR2Selected = false;
            Thread.Sleep(500);
        }


        public Visibility IsLightScanVisibility
        {
            get { return _IsLightScanVisibility; }
            set
            {
                if (_IsLightScanVisibility != value)
                {
                    _IsLightScanVisibility = value;
                    RaisePropertyChanged("IsLightScanVisibility");

                }
            }
        }
        public Visibility IsLightStopVisibility
        {
            get { return _IsLightStopVisibility; }
            set
            {
                if (_IsLightStopVisibility != value)
                {
                    _IsLightStopVisibility = value;
                    RaisePropertyChanged("IsLightStopVisibility");
                }
            }
        }
        public bool IsLightScanning
        {
            get { return _IsLightScannerMode; }
            set
            {
                if (_IsLightScannerMode != value)
                {
                    _IsLightScannerMode = value;
                    if (_IsLightScannerMode)
                    {
                        IsLightScanVisibility = Visibility.Visible;
                    }
                    else
                    {
                        IsLightScanVisibility = Visibility.Hidden;
                    }
                }
            }
        }

        public bool IsLightStopning
        {
            get { return _IsLightStopMode; }
            set
            {
                if (_IsLightStopMode != value)
                {
                    _IsLightStopMode = value;
                    if (_IsLightStopMode)
                    {
                        IsLightStopVisibility = Visibility.Visible;
                    }
                    else
                    {
                        IsLightStopVisibility = Visibility.Hidden;
                    }
                }
            }
        }

        private bool IsLightGainModule 
        {
            get;
            set;
        }


        #region 用来计算
        private double _ZParam1=0;
        public double ZParam1
        {
            get { return _ZParam1; }
            set
            {

                if (_ZParam1 != value)
                {
                    _ZParam1 = value;
                    RaisePropertyChanged("ZParam1");
                    if (_ZParam2 > 0 || _ZParam1 > 0)
                    {
                        _ZResult = (_ZParam2 + _ZParam1) / 2;
                        RaisePropertyChanged("ZResult");
                    }
                }
            }
        }
        private double _ZParam2 = 0;
        public double ZParam2
        {
            get { return _ZParam2; }
            set
            {

                if (_ZParam2 != value)
                {
                    _ZParam2 = value;
                    RaisePropertyChanged("ZParam2");
                    if (_ZParam2 > 0 || _ZParam1 > 0)
                    {
                        _ZResult = (_ZParam2 + _ZParam1) / 2;
                        RaisePropertyChanged("ZResult");
                    }
                }
            }
        }
        private double _ZResult = 0;
        public double ZResult
        {
            get { return _ZResult; }
            set
            {

                if (_ZResult != value)
                {
                    _ZResult = value;
                    RaisePropertyChanged("ZResult");

                }
            }
        }
        #endregion

        #region IsEnabled
        private bool _EnabledButton = false;
        public bool EnabledButton
        {
            get { return _EnabledButton; }
            set
            {
                _EnabledButton = value;
                RaisePropertyChanged("EnabledButton");
            }
        }
        #endregion
        #endregion

        #region ReadApdCommand

        private RelayCommand _ReadApdCommand = null;

        public ICommand ReadApdCommand
        {
            get
            {
                if (_ReadApdCommand == null)
                {
                    _ReadApdCommand = new RelayCommand(this.Execute_ReadApdCommand, this.CanExecute_ReadApdCommand);
                }

                return _ReadApdCommand;
            }
        }
        public void Execute_ReadApdCommand(object parameter)
        {
            //TODO: implement the read apd values command
            //if (Workspace.This.ApdVM._APDTransfer.APDTransferIsAlive)
            //{
            //    //System.Windows.MessageBox.Show(string.Format("TODO: Set APD D Gain: {0}", _ApdDGain));
            //    _APDTransfer.APDLaserReadAPD();
            //}

            if (_EthernetController.TriggerSingleScan())
            {
                ApdAValue = (int)_EthernetController.SampleValueChA;
                ApdBValue = (int)_EthernetController.SampleValueChB;
                ApdCValue = (int)_EthernetController.SampleValueChC;
            }
        }

        public bool CanExecute_ReadApdCommand(object parameter)
        {
            return true;
        }

        #endregion

    }
}
