using Azure.EthernetCommLib;
using Azure.ScannerTestJig.View;
using Azure.ScannerTestJig.View.APDCalibration;
using Azure.ScannerTestJig.View.GlassTopAdj;
using Azure.ScannerTestJig.View.LightModeCalibration;
using Azure.ScannerTestJig.View.TotalMachine;
using Azure.ScannerTestJig.View.Upgrade;
using Azure.ScannerTestJig.ViewModule._532LaserModel;
using Azure.ScannerTestJig.ViewModule._532WaveForm;
using Azure.ScannerTestJig.ViewModule.APDCalibration;
using Azure.ScannerTestJig.ViewModule.APDIVTempCalibration;
using Azure.ScannerTestJig.ViewModule.Camera;
using Azure.ScannerTestJig.ViewModule.FocusAdjust;
using Azure.ScannerTestJig.ViewModule.GlassTopAdj;
using Azure.ScannerTestJig.ViewModule.LightModeCalibration;
using Azure.ScannerTestJig.ViewModule.MultiChannelLaserCalibration;
using Azure.ScannerTestJig.ViewModule.No532PDLaserModel;
using Azure.ScannerTestJig.ViewModule.PDCal532LaserModel;
using Azure.ScannerTestJig.ViewModule.TotalMachine;
using Azure.ScannerTestJig.ViewModule.Upgrade;
using Azure.WPF.Framework;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Azure.ScannerTestJig.ViewModule
{
    class Workspace : ViewModelBase, IDisposable
    {
        #region Private data..._GlassTopPosVM
        private APDCalibrationChannelViewModel _APDCalibrationChannelAViewModel = null;
        private APDCalibrationViewModel _APDCalibrationViewModel = null;
        private APDIVTempCalibrationViewModel _APDIvTempCalibrationViewModel = null;
        private WaveViewCalibrationViewModel _WaveViewCalibrationViewModel = null;
        private WaveViewChartViewModel _WaveViewChartViewModel = null;
        private PDCalWaveViewChart532LaserViewModel _PDCalWaveViewChart532LaserViewModel = null;
        private WaveViewChart532LaserViewModel _WaveViewChart532LaserViewModel = null;
        private WaveViewChartNo532LaserViewModel _WaveViewChartNo532LaserViewModel = null;
        private ViewModuleChartMultiChannelLaserCailbration _ViewModuleChartMultiChannelLaserCailbration = null;
        private TotalMachineViewModule _TotalMachiVM = null;
        private GlassTopAdjPosControlViewModel _GlassTopPosVM = null;
        private ZScanChartViewModel _ZScanChartFocusVM = null;
        private ZScanChartViewModel _ZScanChartGlassVM = null;
        private MotorViewModel _MotorViewModel = null;
        private GlassTopAdjustViewModel _GlassAdjustWindVM = null;
        private FocusAdjustViewModel _FocusAdjustViewModel = null;
        private PramaSetteViewModule _PramaSetteViewModule = null;
        private TotalMachineLaserPowerCalibrationvm _TolalPowerCliVm = null;
        private LightModeCalibrationViewModule _LightCaliModeVM = null;
        private TotalMacheine532LaserModule _Tolal532LaserMoudle = null;
        private UpgradeViewModule _UpgradeViewModule = null;
        private CameraViewModule _CameraViewModule = null;

        private RelayCommand _APDIvTempCaliAdjustCommand = null;
        private RelayCommand _WaveViewAdjustCommand = null;
        private RelayCommand _LaserModel532Command = null;
        private RelayCommand _PDCalLaserModel532Command = null;
        private RelayCommand _LaserModelNo532Command = null;
        private RelayCommand _MultiChannelLaserCalibrationCommand = null;
        private RelayCommand _APDCaliAdjustCommand = null;
        private RelayCommand _FocusAdjustCommand = null;
        private RelayCommand _GlassAdjustCommand = null;
        private RelayCommand _TotalAdjustCommand = null;
        private RelayCommand _LightTotalAdjustCommand = null;
        private RelayCommand _UpgradeCommand = null;
        private RelayCommand _CameraCommand = null;
        
        private EthernetController _EthernetController;
        UpgradeSub upgradeSub = null;
       //private SerialPortCon _SerialPort = null;
        private double _EthernetTransactionRate; 
        private bool _IsSelectGlassCM;
        #endregion
        // Flag: Has Dispose already been called?
        bool disposed = false;
        // Instantiate a SafeHandle instance.
        SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);
        private bool _APDPowreValueFlat = false;
        #region Constructors...
        public Workspace()
        {
          
            _EthernetController = new EthernetController();
            _EthernetController.OnDataRateChanged += _EthernetController_OnDataRateChanged;
            _MotorViewModel = new MotorViewModel();
            _PramaSetteViewModule = new PramaSetteViewModule(_EthernetController);
            _ZScanChartFocusVM = new ZScanChartViewModel();
            _GlassTopPosVM = new GlassTopAdjPosControlViewModel();
            _GlassAdjustWindVM = new GlassTopAdjustViewModel(_EthernetController);
            _ZScanChartGlassVM = new ZScanChartViewModel();
            _TotalMachiVM = new TotalMachineViewModule(_EthernetController);
            _TolalPowerCliVm = new TotalMachineLaserPowerCalibrationvm(_EthernetController);
            _LightCaliModeVM = new LightModeCalibrationViewModule(_EthernetController);
            _APDCalibrationViewModel = new APDCalibrationViewModel();
            _APDIvTempCalibrationViewModel = new APDIVTempCalibrationViewModel();
            _WaveViewChartViewModel = new WaveViewChartViewModel();
            _WaveViewChart532LaserViewModel = new WaveViewChart532LaserViewModel();
            _PDCalWaveViewChart532LaserViewModel = new PDCalWaveViewChart532LaserViewModel();
            _WaveViewChartNo532LaserViewModel = new WaveViewChartNo532LaserViewModel();
            _ViewModuleChartMultiChannelLaserCailbration = new ViewModuleChartMultiChannelLaserCailbration();
            _WaveViewCalibrationViewModel = new WaveViewCalibrationViewModel();
            _APDCalibrationChannelAViewModel = new APDCalibrationChannelViewModel(
               "通道A",
               APDCalibrationVM.APDCalibrationRecords,
               _APDCalibrationViewModel);
            _Tolal532LaserMoudle = new TotalMacheine532LaserModule(_EthernetController);
            _UpgradeViewModule = new UpgradeViewModule();
            _CameraViewModule = new CameraViewModule();

        }
        #endregion
        public MainWindow Owner { get; set; }

        //0xFE代表是非法值,
        public uint Uint8Code = 0xFE;
        //0xFFFE代表是非法值，
        public uint Uint16Code = 0xFFFE;
        //0xFFFFFFFE代表是非法值，
        public uint Uint32Code = 0xFFFFFFFE;
        //FFFF255254代表是非法值 ，
        public string StrEmptyCode = "FFFF255254";
        //FFFFFFFE代表是非法值 ，
        public string StrEmptyCode1 = "FFFFFFFE";

        public string DefaultStrCode = "NaN";

        public int DefaultIntCode = -1;

        public double DefaultIntCode2 = -2;

        #region SubCommand

        #region  PDCal532激光模块
        public ICommand PDCalLaserModel532Command
        {
            get
            {
                if (_PDCalLaserModel532Command == null)
                {
                    _PDCalLaserModel532Command = new RelayCommand(Execute_PDCalLaserModel532CommandAdjustCommand, CanExecute_PDCalLaserModel532CommandAdjustCommand);
                }

                return _PDCalLaserModel532Command;
            }
        }

        public void Execute_PDCalLaserModel532CommandAdjustCommand(object parameter)
        {
            This.Owner.Hide();
            _PDCalWaveViewChart532LaserViewModel.ExcutePortConnectCommand();
        }

        public bool CanExecute_PDCalLaserModel532CommandAdjustCommand(object parameter)
        {
            return true;
        }
        #endregion


        #region  多通道激光测试
        public ICommand MultiChannelLaserCalibrationCommand
        {
            get
            {
                if (_MultiChannelLaserCalibrationCommand == null)
                {
                    _MultiChannelLaserCalibrationCommand = new RelayCommand(Execute_MultiChannelLaserCalibrationCommand, CanExecute_MultiChannelLaserCalibrationCommand);
                }

                return _MultiChannelLaserCalibrationCommand;
            }
        }

        public void Execute_MultiChannelLaserCalibrationCommand(object parameter)
        {
            This.Owner.Hide();
            _ViewModuleChartMultiChannelLaserCailbration.ExcutePortConnectCommand();
        }

        public bool CanExecute_MultiChannelLaserCalibrationCommand(object parameter)
        {
            return true;
        }
        #endregion

        #region  No532激光模块
        public ICommand LaserModelNo532Command
        {
            get
            {
                if (_LaserModelNo532Command == null)
                {
                    _LaserModelNo532Command = new RelayCommand(Execute_LaserModelNo532CommandAdjustCommand, CanExecute_LaserModelNo532CommandAdjustCommand);
                }

                return _LaserModelNo532Command;
            }
        }

        public void Execute_LaserModelNo532CommandAdjustCommand(object parameter)
        {
            This.Owner.Hide();
            _WaveViewChartNo532LaserViewModel.ExcutePortConnectCommand();
        }

        public bool CanExecute_LaserModelNo532CommandAdjustCommand(object parameter)
        {
            return true;
        }
        #endregion

        #region  532激光模块
        public ICommand LaserModel532Command
        {
            get
            {
                if (_LaserModel532Command == null)
                {
                    _LaserModel532Command = new RelayCommand(Execute_LaserModel532CommandAdjustCommand, CanExecute_LaserModel532CommandAdjustCommand);
                }

                return _LaserModel532Command;
            }
        }

        public void Execute_LaserModel532CommandAdjustCommand(object parameter)
        {
            This.Owner.Hide();
            _WaveViewChart532LaserViewModel.ExcutePortConnectCommand();
        }

        public bool CanExecute_LaserModel532CommandAdjustCommand(object parameter)
        {
            return true;
        }
        #endregion

        #region  532波形标定
        public ICommand WaveViewCalibrationCommand
        {
            get
            {
                if (_WaveViewAdjustCommand == null)
                {
                    _WaveViewAdjustCommand = new RelayCommand(ExecuteWaveViewAdjustCommand, CanExecuteWaveViewCaliAdjustCommand);
                }

                return _WaveViewAdjustCommand;
            }
        }

        public void ExecuteWaveViewAdjustCommand(object parameter)
        {
            This.Owner.Hide();
            _WaveViewChartViewModel.ExcutePortConnectCommand();
        }

        public bool CanExecuteWaveViewCaliAdjustCommand(object parameter)
        {
            return true;
        }
        #endregion

        #region  APDIvTemp标定
        public ICommand APDIvTempCalibrationCommand
        {
            get
            {
                if (_APDIvTempCaliAdjustCommand == null)
                {
                    _APDIvTempCaliAdjustCommand = new RelayCommand(ExecuteAPDIvTempCaliAdjustCommand, CanExecuteAPDIvTempCaliAdjustCommand);
                }

                return _APDIvTempCaliAdjustCommand;
            }
        }

        public void ExecuteAPDIvTempCaliAdjustCommand(object parameter)
        {
            This.Owner.Hide();
            _APDIvTempCalibrationViewModel.ExcuteAPDIvTempCalibrationCommand(null);
            //APDCalibrationSubWind apdCaliAdjustSubWind = new APDCalibrationSubWind();
            //apdCaliAdjustSubWind.Title = This.Owner.Title + "-APD Calibration Kit";
            //apdCaliAdjustSubWind.ShowDialog();
        }

        public bool CanExecuteAPDIvTempCaliAdjustCommand(object parameter)
        {
            return true;
        }
        #endregion

        #region  APD标定
        public ICommand APDCalibrationCommand
        {
            get
            {
                if (_APDCaliAdjustCommand == null)
                {
                    _APDCaliAdjustCommand = new RelayCommand(ExecuteAPDCaliAdjustCommand, CanExecuteAPDCaliAdjustCommand);
                }

                return _APDCaliAdjustCommand;
            }
        }

        public void ExecuteAPDCaliAdjustCommand(object parameter)
        {
            This.Owner.Hide();
            _APDCalibrationViewModel.ExcuteAPDCalibrationCommand(null) ;
            //APDCalibrationSubWind apdCaliAdjustSubWind = new APDCalibrationSubWind();
            //apdCaliAdjustSubWind.Title = This.Owner.Title + "-APD Calibration Kit";
            //apdCaliAdjustSubWind.ShowDialog();
        }

        public bool CanExecuteAPDCaliAdjustCommand(object parameter)
        {
            return true;
        }
        #endregion

        #region  整机测试
        public ICommand TotalAdjustCommand
        {
            get
            {
                if (_TotalAdjustCommand == null)
                {
                    _TotalAdjustCommand = new RelayCommand(ExecuteTotalAdjustCommand, CanExecuteTotalAdjustCommand);
                }

                return _TotalAdjustCommand;
            }
        }

        public void ExecuteTotalAdjustCommand(object parameter)
        {
            if (!InitControlValue())
            {
               return;
            }
            Window1 XMaxStroke = new Window1();
            XMaxStroke.ShowDialog();
            if (XMaxStroke.CM250.IsChecked == true || XMaxStroke.CM500.IsChecked == true)
            {
                WindownCount = 1;//整机
                This.Owner.Hide();
                TotalMachineSubWind focusAdjustSubWind = new TotalMachineSubWind();
                focusAdjustSubWind.Title = This.Owner.Title + "-整机测试";
                focusAdjustSubWind.ShowDialog();
            }
        }

        public bool CanExecuteTotalAdjustCommand(object parameter)
        {
            return true;
        }
        #endregion

        #region  对焦
        public ICommand FocusAdjustCommand
        {
            get
            {
                if (_FocusAdjustCommand == null)
                {
                    _FocusAdjustCommand = new RelayCommand(ExecuteFocusAdjustCommand, CanExecuteFocusAdjustCommand);
                }

                return _FocusAdjustCommand;
            }
        }

        public void ExecuteFocusAdjustCommand(object parameter)
        {
            if (!InitControlValue())
            {
                return;
            }
            Window1 XMaxStroke = new Window1();
            XMaxStroke.ShowDialog();
            if (XMaxStroke.CM250.IsChecked == true || XMaxStroke.CM500.IsChecked == true)
            {
                WindownCount = 2;//对焦
                This.Owner.Hide();
                FocusAdjustSubWind focusAdjustSubWind = new FocusAdjustSubWind();
                focusAdjustSubWind.Title = This.Owner.Title + "-共焦距";
                ZScanChartViewModel vm = ZScanChartFocusVM;
                if (vm != null)
                {
                    focusAdjustSubWind.DataContext = vm;
                    vm.FocusVisibility = Visibility.Visible;
                    vm.GlassVisibility = Visibility.Hidden;
                }
                focusAdjustSubWind.ShowDialog();
            }

        }
        public bool CanExecuteFocusAdjustCommand(object parameter)
        {
            return true;
        }
        #endregion

        #region 玻璃面调平
        public ICommand GlassAdjustCommand
        {
            get
            {
                if (_GlassAdjustCommand == null)
                {
                    _GlassAdjustCommand = new RelayCommand(ExecuteGlassAdjustCommand, CanExecuteGlassAdjustCommand);
                }

                return _GlassAdjustCommand;
            }
        }

        public void ExecuteGlassAdjustCommand(object parameter)
        {
            if (!InitControlValue())
            {
                return;
            }
            Window1 XMaxStroke = new Window1();
            XMaxStroke.ShowDialog();
            if (XMaxStroke.CM250.IsChecked == true || XMaxStroke.CM500.IsChecked == true)
            {
                WindownCount = 3;//玻璃门调平
                This.Owner.Hide();
                GlassTopAdjSubWind glassTopAdjSubWind = new GlassTopAdjSubWind();
                glassTopAdjSubWind.Title = This.Owner.Title + "-玻璃面调平";
                ZScanChartViewModel vm = ZScanChartGlassVM;
                if (vm != null)
                {
                    glassTopAdjSubWind.DataContext = vm;
                    vm.FocusVisibility = Visibility.Hidden;
                    vm.GlassVisibility = Visibility.Visible;

                }
                glassTopAdjSubWind.ShowDialog();
            }
        }

        public bool CanExecuteGlassAdjustCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region  光学模块
        public ICommand LightTotalAdjustCommand
        {
            get
            {
                if (_LightTotalAdjustCommand == null)
                {
                    _LightTotalAdjustCommand = new RelayCommand(LightExecuteTotalAdjustCommand, LightCanExecuteTotalAdjustCommand);
                }

                return _LightTotalAdjustCommand;
            }
        }

        public void LightExecuteTotalAdjustCommand(object parameter)
        {
            if (!InitControlValue())
            {
                //return;
            }
            This.Owner.Hide();
            _LightCaliModeVM.ExcuteLightModeCalibrationCommand(null);
        }

        public bool LightCanExecuteTotalAdjustCommand(object parameter)
        {
            return true;
        }
        #endregion

        #region  升级
        public ICommand UpgradeCommand
        {
            get
            {
                if (_UpgradeCommand == null)
                {
                    _UpgradeCommand = new RelayCommand(ExecuteUpgradeCommand, CanExecuteUpgradeCommand);
                }

                return _UpgradeCommand;
            }
        }

        public void ExecuteUpgradeCommand(object parameter)
        {
            This.Owner.Hide();
            _UpgradeViewModule.ExcutePortConnectCommand();
        }

        public bool CanExecuteUpgradeCommand(object parameter)
        {
            return true;
        }
        #endregion

        #region  相机
        public ICommand CameraCommand
        {
            get
            {
                if (_CameraCommand == null)
                {
                    _CameraCommand = new RelayCommand(ExecuteCameraCommand, CanExecuteCameraCommand);
                }

                return _CameraCommand;
            }
        }

        public void ExecuteCameraCommand(object parameter)
        {
            This.Owner.Hide();
            _CameraViewModule.ExcuteCameraConnectCommand();
        }

        public bool CanExecuteCameraCommand(object parameter)
        {
            return true;
        }
        #endregion

        public bool InitControlValue()
        {
            if (!ConnectEthernetSlave())
            {
                MessageBox.Show("连接失败，请检查连接！\n");
                return false;

            }
            EthernetController.GetAllIvModulesInfo();
            EthernetController.GetAllLaserModulseInfo();
            EthernetController.GetAllIvModulesInfo();
            EthernetController.GetAllLaserModulseInfo();
            MotorVM.InitMotionController();
            MotorVM.IsNewFirmware = true;
            string WL1, WR1, WR2;
            This.PramaSetteVM.SensorML1 = EthernetController.IvSensorTypes[IVChannels.ChannelC];
            This.PramaSetteVM.SensorMR1 = EthernetController.IvSensorTypes[IVChannels.ChannelA];
            This.PramaSetteVM.SensorMR2 = EthernetController.IvSensorTypes[IVChannels.ChannelB];
            This.GlassAdjustWindVM.SensorML1 = EthernetController.IvSensorTypes[IVChannels.ChannelC];
            This.GlassAdjustWindVM.SensorMR1 = EthernetController.IvSensorTypes[IVChannels.ChannelA];
            This.GlassAdjustWindVM.SensorMR2 = EthernetController.IvSensorTypes[IVChannels.ChannelB];
            This.PramaSetteVM.SensorSNL1 = EthernetController.IvSensorSerialNumbers[IVChannels.ChannelC].ToString("X8");
            This.PramaSetteVM.SensorSNR1 = EthernetController.IvSensorSerialNumbers[IVChannels.ChannelA].ToString("X8");
            This.PramaSetteVM.SensorSNR2 = EthernetController.IvSensorSerialNumbers[IVChannels.ChannelB].ToString("X8");
            This.PramaSetteVM.LaserSNL1 = EthernetController.LaserSerialNumbers[LaserChannels.ChannelC].ToString("X8");
            This.PramaSetteVM.LaserSNR1 = EthernetController.LaserSerialNumbers[LaserChannels.ChannelA].ToString("X8");
            This.PramaSetteVM.LaserSNR2 = EthernetController.LaserSerialNumbers[LaserChannels.ChannelB].ToString("X8");
            This.GlassAdjustWindVM.SensorSNL1 = EthernetController.IvSensorSerialNumbers[IVChannels.ChannelC].ToString("X8");
            This.GlassAdjustWindVM.SensorSNR1 = EthernetController.IvSensorSerialNumbers[IVChannels.ChannelA].ToString("X8");
            This.GlassAdjustWindVM.SensorSNR2 = EthernetController.IvSensorSerialNumbers[IVChannels.ChannelB].ToString("X8");
            This.GlassAdjustWindVM.LaserSNL1 = EthernetController.LaserSerialNumbers[LaserChannels.ChannelC].ToString("X8");
            This.GlassAdjustWindVM.LaserSNR1 = EthernetController.LaserSerialNumbers[LaserChannels.ChannelA].ToString("X8");
            This.GlassAdjustWindVM.LaserSNR2 = EthernetController.LaserSerialNumbers[LaserChannels.ChannelB].ToString("X8");
            WL1 = EthernetController.LaserWaveLengths[LaserChannels.ChannelC].ToString();
            if (WL1 == "0"|| Convert.ToUInt32(WL1)== Uint16Code)
            {
                WL1 = "NA";
            }
            WR1 = EthernetController.LaserWaveLengths[LaserChannels.ChannelA].ToString();
            if (WR1 == "0"|| Convert.ToUInt32(WR1) == Uint16Code)
            {
                WR1 = "NA";
            }
            WR2 = EthernetController.LaserWaveLengths[LaserChannels.ChannelB].ToString();
            if (WR2 == "0"|| Convert.ToUInt32(WR2) == Uint16Code)
            {
                WR2 = "NA";
            }
            This.PramaSetteVM.WL1 = WL1;
            This.PramaSetteVM.WR1 = WR1;
            This.PramaSetteVM.WR2 = WR2;
            //This.GlassAdjustWindVM.WL1 = WL1;
            //This.GlassAdjustWindVM.WR1 = WR1;
            //This.GlassAdjustWindVM.WR2 = WR2;
            This.TolalPowerCliVm.WL1 = WL1;
            This.TolalPowerCliVm.WR1 = WR1;
            This.TolalPowerCliVm.WR2 = WR2;
            return true;
        }


        #endregion

        private void _EthernetController_OnDataRateChanged()
        {
            EthernetDataRate = Math.Round(_EthernetController.ReadingRate);
        }
        //public bool serialState
        //{
        //    get { return _serialState; }
        //    set
        //    {
        //        if (_serialState != value)
        //        {
        //            _serialState = value;
        //        }
        //    }
        //}

        public double EthernetDataRate
        {
            get { return _EthernetTransactionRate; }
            set
            {
                if (_EthernetTransactionRate != value)
                {
                    _EthernetTransactionRate = value;
                    RaisePropertyChanged(nameof(EthernetDataRate));
                }
            }
        }

        public bool IsSelectGlassCM
        {
            get { return _IsSelectGlassCM; }
            set
            {
                if (_IsSelectGlassCM != value)
                {
                    _IsSelectGlassCM = value;
                }
            }
        }
        /// <summary>
        /// internal method
        /// </summary>
        /// <returns></returns>
        internal bool ConnectEthernetSlave()
        {
            return _EthernetController.Connect(new System.Net.IPAddress(new byte[] { 192, 168, 1, 110 }), 5000, 8000, new System.Net.IPAddress(new byte[] { 192, 168, 1, 100 }));

        }
        internal EthernetController EthernetController
        {
            get { return _EthernetController; }
        }
        //internal SerialPortCon SerialPorts
        //{
        //    get { return _SerialPort; }
        //}
        protected override void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                handle.Dispose();
                // Free any other managed objects here.
                //

            }

            // Free any unmanaged objects here.
            //

            disposed = true;
            // Call base class implementation.
            base.Dispose(disposing);
        }

        static Workspace _this = new Workspace();
        public static Workspace This
        {
            get { return _this; }
            set { _this = value; }
        }
        public bool APDPowreValueFlat
        {
            get { return _APDPowreValueFlat; }
            set { _APDPowreValueFlat = value; }
        }
        
        #region  public
        public string ProductVersion { get; set; }

        public ZScanChartViewModel ZScanChartGlassVM
        {
            get
            {
                return _ZScanChartGlassVM;
            }
        }

        public GlassTopAdjustViewModel GlassAdjustWindVM
        {
            get { return _GlassAdjustWindVM; }
        }

        public GlassTopAdjPosControlViewModel GlassTopPosVM
        {
            get
            {
                return _GlassTopPosVM;
            }
        }
        public TotalMacheine532LaserModule Tolal532LaserMoudle
        {
            get
            {
                return _Tolal532LaserMoudle;
            }
        }
        public UpgradeViewModule UpgradeViewModule
        {
            get
            {
                return _UpgradeViewModule;
            }
        }
        public CameraViewModule CameraViewModule
        {
            get
            {
                return _CameraViewModule;
            }
        }
        
        public ZScanChartViewModel ZScanChartFocusVM
        {
            get
            {
                return _ZScanChartFocusVM;
            }
        }

        public FocusAdjustViewModel FocusAdjustVM
        {
            get
            {
                return _FocusAdjustViewModel;
            }
        }

        public PramaSetteViewModule PramaSetteVM
        {
            get
            {
                return _PramaSetteViewModule;
            }
        }
        public TotalMachineLaserPowerCalibrationvm TolalPowerCliVm
        {
            get
            {
                return _TolalPowerCliVm;
            }
        }
        public MotorViewModel MotorVM
        {
            get { return _MotorViewModel; }
        }

       public TotalMachineViewModule TotalMachiVM
        {
            get { return _TotalMachiVM; }
        }


        
        public APDCalibrationViewModel APDCalibrationVM
        {
            get
            {
                return _APDCalibrationViewModel;
            }
        }
        public APDIVTempCalibrationViewModel APDIvTempCalibrationVM
        {
            get
            {
                return _APDIvTempCalibrationViewModel;
            }
        }

        public WaveViewCalibrationViewModel WaveViewCalibrationViewModel
        {
            get
            {
                return _WaveViewCalibrationViewModel;
            }
        }
        public WaveViewChartViewModel WaveViewChartViewModel
        {
            get
            {
                return _WaveViewChartViewModel;
            }
        }

        public WaveViewChart532LaserViewModel WaveViewChart532LaserViewModel
        {
            get
            {
                return _WaveViewChart532LaserViewModel;
            }
        }
        public PDCalWaveViewChart532LaserViewModel PDCalWaveViewChart532LaserViewModel
        {
            get
            {
                return _PDCalWaveViewChart532LaserViewModel;
            }
        }
        public WaveViewChartNo532LaserViewModel WaveViewChartNo532LaserViewModel
        {
            get
            {
                return _WaveViewChartNo532LaserViewModel;
            }
        }
        public ViewModuleChartMultiChannelLaserCailbration ViewModuleChartMultiChannelLaserCailbration
        {
            get
            {
                return _ViewModuleChartMultiChannelLaserCailbration;
            }
        }
        

        public APDCalibrationChannelViewModel APDCalibrationChannelAVM
        {
            get
            {
                return _APDCalibrationChannelAViewModel;
            }
        }
        public LightModeCalibrationViewModule LightModeCaliVM
        {
            get
            {
                return _LightCaliModeVM;
            }
        }
        public int WindownCount 
        {
            get;set;
        }

        string path = Directory.GetCurrentDirectory() + @"\测试报告\";
        public string GetManualPath(string csvName)
        {
            // 创建对话框实例
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            // 设置对话框的标题和默认文件名
            //saveFileDialog.Title = "保存文件";
            saveFileDialog.FileName = csvName;

            // 设置文件类型过滤器
            saveFileDialog.Filter = "CSV文件 (*.csv)|*.csv";

            // 显示对话框并获取用户输入
            bool? result = saveFileDialog.ShowDialog();

            // 检查是否点击了保存按钮
            if (result == true)
            {
                // 获取选择的文件路径
                string filePath = saveFileDialog.FileName;

                // 进行文件保存操作
                // ...
                return filePath;
            }
            return "";
        }

        public string GetManualFolder()
        {
            System.Windows.Forms.FolderBrowserDialog folderDialog = new System.Windows.Forms.FolderBrowserDialog();

            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string selectedPath = folderDialog.SelectedPath;
                return selectedPath+"\\";
                // 处理选中的文件夹路径
            }
            return "";
        }
        #endregion

        #region  InitControl

        #endregion
    }
}
