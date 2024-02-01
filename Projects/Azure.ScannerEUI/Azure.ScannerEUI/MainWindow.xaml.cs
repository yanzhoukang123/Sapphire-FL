using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using Azure.ScannerEUI.ViewModel;
using Azure.ImagingSystem;
using Azure.Configuration.Settings;
using Azure.Utilities;    //WindowStateManager, MruManager
using Azure.EthernetCommLib;
using LogW;
using System.Windows.Threading;
using log4net.Config;
using log4net;
using log4net.Appender;
using System.IO;

namespace Azure.ScannerEUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private data...

        private string _AdDefaultLayoutResourceName = "Azure.ScannerEUI.Resources.AdDefaultLayoutFile.xml";
        //private string _AdLayoutFileName = System.IO.Path.Combine(Environment.CurrentDirectory, "AdLayoutFile.xml");
        //private string _AdLayoutFileName = "AdLayoutFile.xml";
        private bool _IsAvalonLoaded = false;
        private WindowStateManager _WindowStateManager;
        private ProgressDialogHelper _ProgressDialogHelper = null;
        private int _temp = 0;
        #endregion

        //public string ProductVersion { get; set; }
        public MainWindow()
        {

            // Create WindowStateManager and associate it with ApplicationSettings.MainWindowStateInfo.
            // This allows to set initial window state and track state changes in
            // the Settings.MainWindowStateInfo instance.
            // When application is closed, ApplicationSettings is saved with new window state
            // information. Next time this information is loaded from XML file.
            //_WindowStateManager = new WindowStateManager(SettingsManager.ApplicationSettings.MainWindowStateInfo, this);

            InitializeComponent();

            #region Prompt for password
            // Prompt for password
            string strSecureXmlPath = System.IO.Path.Combine(SettingsManager.ApplicationDataPath, "EUIAuthen.xml");
            if (System.IO.File.Exists(strSecureXmlPath))
            {
                // Get the current password
                string passwordHash = SecureSettings.GetPassword(strSecureXmlPath);
                // Don't prompt for a password if current password is blank/empty
                if (!string.IsNullOrEmpty(passwordHash))
                {
                    // Prompt and authenticate the password.
                    PasswordPrompt passwordPrompt = new PasswordPrompt(strSecureXmlPath);
                    passwordPrompt.ShowDialog();
                    if (passwordPrompt.DialogResult != true)
                    {
                        Workspace.This.IsAuthenticated = false;
                        System.Windows.Application.Current.Shutdown();
                    }
                    else
                    {
                        if (SettingsManager.ConfigSettings.AllModuleProcessing&& SettingsManager.ConfigSettings.ENGGUI_PhosphorModuleProcessing)
                        {
                            MessageBox.Show(" it’ll either use the software filter or unidirectional scan!");
                            System.Windows.Application.Current.Shutdown();
                        }
                        Workspace.This.IsAuthenticated = true;
                    }
                }
                else
                {
                    //EL: TODO: not allowing empty password
                    Workspace.This.IsAuthenticated = true;
                }
            }
            else
            {
                string caption = "EUI application authentication";
                string message = string.Format("Could not find the file: \n{0}", strSecureXmlPath);
                Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                Workspace.This.IsAuthenticated = false;
                System.Windows.Application.Current.Shutdown();
            }
            #endregion

            this.Title = string.Format("Avocado Captrue V{0}", Workspace.This.ProductVersion);
            DataContext = Workspace.This;
            Workspace.This.Owner = this;
            Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            string productVersion = string.Format("{0}.{1}.{2}.{3}",
                                                  version.Major,
                                                  version.Minor,
                                                  version.Build,
                                                  version.Revision.ToString("D4"));

            // Override the default path specified in 'log4net.config'
            log4net.ILog log4 = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            Log.log4 = log4;
            XmlConfigurator.Configure();
            log4net.Repository.Hierarchy.Hierarchy h = (log4net.Repository.Hierarchy.Hierarchy)LogManager.GetRepository();
            foreach (IAppender a in h.Root.Appenders)
            {
                if (a is FileAppender)
                {
                    FileAppender fa = (FileAppender)a;
                    // Programmatically set this to the desired location here
                    //string logFileLocation = @"C:\MySpecialFolder\MyFile.log";

                    // Uncomment the lines below if you want to retain the base file name
                    // and change the folder name...
                    FileInfo fileInfo = new FileInfo(fa.File);
                    string logFileLocation = string.Format(@"{0}\Log\{1}", SettingsManager.ApplicationDataPath, fileInfo.Name);

                    fa.File = logFileLocation;
                    fa.ActivateOptions();
                    break;
                }
            }
            // Create 'ImageGif' folder and write/save test images in 'ProgramData' (can't write to 'Program Files' (if run from the installation folder))
            string commonApplicationData = SettingsManager.ApplicationDataPath;
            Workspace.This.ImageRotatingPrcessVM.rotapath = System.IO.Path.Combine(commonApplicationData, "CV2TESTIMAGE.png");
            Workspace.This.ImageRotatingPrcessVM.Gifpath = System.IO.Path.Combine(commonApplicationData, "TESTIMAGEGif.gif");
            Workspace.This.ImageRotatingPrcessVM.ImageGifPath = System.IO.Path.Combine(commonApplicationData, "ImageGif");
            Workspace.This.ImageRotatingPrcessVM.LImageGifPath = System.IO.Path.Combine(commonApplicationData, "ImageGif", "L");
            Workspace.This.ImageRotatingPrcessVM.R1ImageGifPath = System.IO.Path.Combine(commonApplicationData, "ImageGif", "R1");
            Workspace.This.ImageRotatingPrcessVM.R2ImageGifPath = System.IO.Path.Combine(commonApplicationData, "ImageGif", "R2");

            Workspace.This.ProductVersion = productVersion;
            if (!SettingsManager.ConfigSettings.IsSimulationMode)
            {
                //InitSplashScreen();
                Workspace.This.OpticalModulePowerMonitor = true;
                Workspace.This.OpticalModulePowerStatus = true;
            }
            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
            //Workspace.This.IsAuthenticated = true;
            //Workspace.This.MotorIsAlive = true;
            //Workspace.This.MotorVM.InitMotorControls(); 
        }
        /// <summary>
        /// ProgressValue And string Red,string Black And Message
        /// </summary>
        /// <param name="ProgressValue"></param>
        /// <param name="Message"></param>
        private void ProgressValueAndMessage(int Value,string Color,string Message) 
        {
            _ProgressDialogHelper.SetMessage(Color, Message);
            for (; _temp < Value; _temp++)
            {
                System.Threading.Thread.Sleep(50);
                _ProgressDialogHelper.SetValue(Value);
            }
        }
        //Program startup load
        private void InitSplashScreen()
        {
            try
            {
                bool _tempCurrent = true;
                _ProgressDialogHelper = new ProgressDialogHelper();
                _ProgressDialogHelper.Show(() =>
                {
                    Workspace.This.IsAuthenticated = true;
                    ProgressValueAndMessage(10, "Black", "System Initializing…");
                    if (!Workspace.This.ConnectEthernetSlave())
                    {
                        ProgressValueAndMessage(30, "Red", "Failed to connect to the main board.\n" + Workspace.This.EthernetController.ErrorMessage);
                        Log.Fatal(this, "Failed to connect to the main board.…");
                        //Thread.Sleep(4000);
                        //_ProgressDialogHelper.CloseProgressDialog();
                        return;
                        //MessageBox.Show("Failed to connect to the main2 board.\n" + Workspace.This.EthernetController.ErrorMessage);
                    }
                    else
                    {
                        Workspace.This.MotorVM.IsNewFirmware = true;//isconnect
                        ProgressValueAndMessage(40, "Black", "Connect to Mainboard… Succeeded…");
                        if (!Workspace.This.EthernetController.GetSystemVersions())
                        {
                            ProgressValueAndMessage(40, "Red", "SystemVersions…Failed…");
                            Log.Fatal(this, "SystemVersions…Failed.…");
                            return;
                        }
                        Workspace.This.ScannerVM.FPGAVersion = Workspace.This.EthernetController.FWVersion;
                        Workspace.This.ScannerVM.HWversion = Workspace.This.EthernetController.HWVersion;
                        ProgressValueAndMessage(50, "Black", "SystemVersions… Succeeded…");
                        Workspace.This.MotorVM.InitMotionController();
                        //软件启动XYZ电机回到原点
                        //Software starts XYZ motor back to Home
                        if (SettingsManager.ConfigSettings.HomeMotionsAtLaunchTime)
                        {
                            ProgressValueAndMessage(60, "Black", "Home Motion X, Y and Z…");
                            if (!Workspace.This.MotorVM.HomeXYZmotor())
                            {
                                ProgressValueAndMessage(50, "Red", "Home Motion X, Y and Z…Failed…");
                                return;
                            }
                            while (_tempCurrent)
                            {
                                Thread.Sleep(500);

                                if (Workspace.This.MotorVM.MotionController.CrntState[EthernetCommLib.MotorTypes.X].AtHome &&
                                    Workspace.This.MotorVM.MotionController.CrntState[EthernetCommLib.MotorTypes.Y].AtHome &&
                                    Workspace.This.MotorVM.MotionController.CrntState[EthernetCommLib.MotorTypes.Z].AtHome)
                                {
                                    _tempCurrent = false;
                                    ProgressValueAndMessage(70, "Black", "Home Motion X, Y and Z…Succeeded…");
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
                        //double _xMotionSubdivision = SettingsManager.ConfigSettings.XMotorSubdivision;
                        //Workspace.This.EthernetController.SetXMotionSubdivision(_xMotionSubdivision);
                        //Thread.Sleep(200);
                        //Workspace.This.EthernetController.GetXEncoderSubdivision();
                        //SettingsManager.ConfigSettings.XEncoderSubdivision = (int)Workspace.This.EthernetController.XEncoderSubdivision;
                        //获取所有IV板子的信息
                        //Get information about all IV boards
                        Workspace.This.EthernetController.GetAllIvModulesInfo();
                        ProgressValueAndMessage(80, "Black", "Identify Optic Modules A, B and C…Succeeded…");
                        // Workspace.This.IVVM.InitIVControls();
                        Workspace.This.MotorIsAlive = true;
                        Workspace.This.EthernetController.GetAllIvModulesInfo();
                        ProgressValueAndMessage(90, "Black", "LaserModulseInfo…Succeeded…");
                        //获取所有通道激光信息
                        //Get all channel laser information
                        Workspace.This.EthernetController.GetAllLaserModulseInfo();
                        Workspace.This.EthernetController.GetIVSystemVersions();//Ge IV Software Version Number
                        Workspace.This.EthernetController.GetIVOpticalModuleSerialNumber(); //IV Optical Module Serial Number
                        Workspace.This.EthernetController.GetLaserSystemVersions(); //Get Laser Software Version Number
                        Workspace.This.EthernetController.GetLaserOpticalModuleSerialNumber(); //Laser Optical Module Serial Number
                    }
                    ProgressValueAndMessage(100, "Black", "Please Wait For System Preparation…");
                    Thread.Sleep(4000);
                    if (!_tempCurrent)
                    {
                        //删除多余用来创建GIF的图像
                        //Remove unnecessary images used to create GIFs
                        Workspace.This.ImageRotatingPrcessVM.DirectoryUpdate();
                        _ProgressDialogHelper.CloseProgressDialog();
                    }
                });
            }
            catch
            {
                ProgressValueAndMessage(0, "Red", " Error   ProgressDialog…");
                _ProgressDialogHelper.CloseProgressDialog();
                _ProgressDialogHelper.WorkerThreadAbort();
            }
        }
        private DispatcherTimer AbnormalTimer = null;

        #region Abnormal time Method
        /// <summary>
        /// 定义一个定时器，用来定义系统状态
        /// Defines a timer that defines the system state
        /// </summary>
        private void OnAbnormalTimeInit()
        {
            AbnormalTimer = new DispatcherTimer();
            AbnormalTimer.Tick += OnTimeLoadData;
            AbnormalTimer.Interval = new TimeSpan(0, 0, 2);//1 sec

        }

        private void OnAbnormalTimeStop()
        {
            AbnormalTimer.Stop();
        }

        private void OnAbnormalTimeStart()
        {
            AbnormalTimer.Start();
        }
        //Device connection status monitoring
        private void OnTimeLoadData(object sender, EventArgs e)
        {
            if (Workspace.This.EthernetController.ErrorMessage != null)
            {
                MessageBoxResult boxResult = MessageBoxResult.None;
                boxResult = MessageBox.Show("Connection failed!!\n" + Workspace.This.EthernetController.ErrorMessage + "\n\nWould you like to close the application?", "Warning", MessageBoxButton.YesNo);
                if (boxResult == MessageBoxResult.Yes)
                {
                    SettingsManager.OnExit();
                    Application.Current.Shutdown();
                    //Thread.Sleep(2000);
                    //if (!Workspace.This.ConnectEthernetSlave())
                    //{
                    //    MessageBox.Show("Failed to connect to the main board.！，Please check whether the power or network connection is normal.\n");
                    //    OnAbnormalTimeStop();
                    //    return;
                    //}
                    //Workspace.This.MotorVM.IsNewFirmware = true;//isconnect
                    //if (!Workspace.This.EthernetController.GetSystemVersions())
                    //{
                    //    MessageBox.Show("Get System version information fail！，Please check whether the power or network connection is normal.\n");
                    //    Log.Fatal(this, "Get System version information fail！，Please check whether the power or network connection is normal…");
                    //    return;
                    //}
                    //Workspace.This.ScannerVM.FPGAVersion = Workspace.This.EthernetController.FWVersion;
                    //Workspace.This.ScannerVM.HWversion = Workspace.This.EthernetController.HWVersion;
                    //Workspace.This.MotorVM.InitMotionController();
                    //Workspace.This.MotorIsAlive = true;
                    //if (!Workspace.This.EthernetController.GetAllIvModulesInfo())
                    //{
                    //    MessageBox.Show("Get IV Modules information fail！，\n");
                    //    Log.Fatal(this, "Get IV Modules information fail！…");
                    //    return;

                    //}
                    //if (Workspace.This.EthernetController.GetAllLaserModulseInfo())
                    //{
                    //    MessageBox.Show("Get Laser Modules information fail！，\n");
                    //    Log.Fatal(this, "Get Laser Modules information fail！.…");
                    //    return;
                    //}
                    //MainWindow_Loaded(null, null);
                }
                else
                {
                    //SettingsManager.OnExit();
                    //Application.Current.Shutdown();
                }
            }
        }
        #endregion
        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (!SettingsManager.ConfigSettings.IsSimulationMode)
            {
                if (Workspace.This.EthernetController != null && Workspace.This.EthernetController.IsConnected)
                {
                    Workspace.This.IVVM.SensorML1 = EthernetController.IvSensorTypes[IVChannels.ChannelC];//C代表L通道     Enumeration C represents an L channel 
                    Workspace.This.IVVM.SensorMR1 = EthernetController.IvSensorTypes[IVChannels.ChannelA];//A代表R1通道    Enumeration A represents an R1 channel
                    Workspace.This.IVVM.SensorMR2 = EthernetController.IvSensorTypes[IVChannels.ChannelB];//B代表R2通道    Enumeration B represents an R2 channel
                    Workspace.This.IVVM.SensorSNL1 = EthernetController.IvSensorSerialNumbers[IVChannels.ChannelC].ToString("X8");//C代表L通道 Enumeration C represents an L channel 
                    Workspace.This.IVVM.SensorSNR1 = EthernetController.IvSensorSerialNumbers[IVChannels.ChannelA].ToString("X8");//A代表R1通道  Enumeration A represents an R1 channel
                    Workspace.This.IVVM.SensorSNR2 = EthernetController.IvSensorSerialNumbers[IVChannels.ChannelB].ToString("X8");//B代表R2通道 Enumeration B represents an R2 channel
                    Workspace.This.IVVM.LaserSNL1 = EthernetController.LaserSerialNumbers[LaserChannels.ChannelC].ToString("X8");//C代表L通道 Enumeration C represents an L channel 
                    Workspace.This.IVVM.LaserSNR1 = EthernetController.LaserSerialNumbers[LaserChannels.ChannelA].ToString("X8");//A代表R1通道 Enumeration A represents an R1 channel
                    Workspace.This.IVVM.LaserSNR2 = EthernetController.LaserSerialNumbers[LaserChannels.ChannelB].ToString("X8");//B代表R2通道  Enumeration B represents an R2 channel
                    Workspace.This.IVVM.WL1 = (int)EthernetController.LaserWaveLengths[LaserChannels.ChannelC];//C代表L通道 Enumeration C represents an L channel 
                    Workspace.This.IVVM.WR1 = (int)EthernetController.LaserWaveLengths[LaserChannels.ChannelA];//A代表R1通道 Enumeration A represents an R1 channel
                    Workspace.This.IVVM.WR2 = (int)EthernetController.LaserWaveLengths[LaserChannels.ChannelB];//B代表R2通道  Enumeration B represents an R2 channel
                    OnAbnormalTimeInit();
                    OnAbnormalTimeStart();
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Close Temperature Aging
            if (Workspace.This.AgingVM.IsTECTempeSelected)
            {
                MessageBoxResult boxResult = MessageBoxResult.None;
                boxResult = MessageBox.Show("TEC Temperature Aging in progress,Close it?", "Warning", MessageBoxButton.YesNo);
                if (boxResult == MessageBoxResult.Yes)
                {
                    Workspace.This.AgingVM.IsTECTempeSelected = false;
                }
                else
                {
                    return;
                }
            }
            if (Workspace.This.IsPreparing)
            {
                string caption = "Scanning Mode";
                string message = "Scanning mode is preparing to scan.\nWould you like to terminate the scanning process?.";
                System.Windows.MessageBoxResult dlgResult = System.Windows.MessageBox.Show(message, caption, System.Windows.MessageBoxButton.YesNo);
                if (dlgResult == System.Windows.MessageBoxResult.No)
                {
                    e.Cancel = true; // don't allow the application to close
                    return;
                }

                ScannerViewModel viewModel = Workspace.This.ScannerVM;
                viewModel.ExecuteStopScanCommand(null);
            }

            if (Workspace.This.IsScanning)
            {
                string caption = "Scanning Mode";
                string message = "Scanning mode is busy.\nWould you like to terminate the current operation?";
                System.Windows.MessageBoxResult dlgResult = System.Windows.MessageBox.Show(message, caption, System.Windows.MessageBoxButton.YesNo);
                if (dlgResult == System.Windows.MessageBoxResult.No)
                {
                    e.Cancel = true; // don't allow the application to close
                    return;
                }

                ScannerViewModel viewModel = Workspace.This.ScannerVM;
                viewModel.ExecuteStopScanCommand(null);
            }

            if (Workspace.This.CloseAll() == false)
            {
                e.Cancel = true; // don't allow the application to close
                return;
            }

            try
            {
                if (Workspace.This.EthernetController != null && Workspace.This.EthernetController.IsConnected)
                {
                    Workspace.This.EthernetController.SetLaserPower(LaserChannels.ChannelA, 0);
                    Workspace.This.EthernetController.SetLaserPower(LaserChannels.ChannelB, 0);
                    Workspace.This.EthernetController.SetLaserPower(LaserChannels.ChannelC, 0);
                    Workspace.This.EthernetController.SetIvApdGain(IVChannels.ChannelA, 0);
                    Workspace.This.EthernetController.SetIvApdGain(IVChannels.ChannelB, 0);
                    Workspace.This.EthernetController.SetIvApdGain(IVChannels.ChannelC, 0);
                    Workspace.This.EthernetController.SetIvPga(IVChannels.ChannelA, 0);
                    Workspace.This.EthernetController.SetIvPga(IVChannels.ChannelB, 0);
                    Workspace.This.EthernetController.SetIvPga(IVChannels.ChannelC, 0);
                    //Setting the fan level
                    Workspace.This.EthernetController.SetIncrustationFan(1, Workspace.This.NewParameterVM.ShellFanDefaultSpeed);
                    Workspace.This.EthernetController.Disconnect();
                }
            }
            catch
            {
            }
            SettingsManager.OnExit();
        }

        /// <summary>
        /// Event raised when AvalonDock has loaded.
        /// Currently only loaidng the default layout.
        /// </summary>
        private void avalonDockHost_AvalonDockLoaded(object sender, EventArgs e)
        {
            //
            // This line of code can be uncommented to get a list of resources.
            //
            //string[] names = this.GetType().Assembly.GetManifestResourceNames();

            //
            // Load the default AvalonDock layout from an embedded resource.
            //  private static readonly string DefaultLayoutResourceName = "cSeries.UI.Resources.DefaultLayoutFile.xml";

            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(_AdDefaultLayoutResourceName))
            {
                if (stream != null && !_IsAvalonLoaded)
                {
                    AvalonDockHost.DockingManager.RestoreLayout(stream);
                    _IsAvalonLoaded = true;
                }
            }
        }

        /// <summary>
        /// Event raised when a document is being closed by clicking the 'X' button in AvalonDock.
        /// </summary>
        private void avalonDockHost_DocumentClosing(object sender, AvalonDockMVVM.DocumentClosingEventArgs e)
        {
            var document = (FileViewModel)e.Document;
            if (!Workspace.This.Close(document))
            {
                e.Cancel = true;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            System.Environment.Exit(0); 
        }

        private bool RunningModeJudge()
        {
            //Workspace.This.ApdVM.APDTransfer.APDLaserStopScan();
            //Workspace.This.ApdVM.APDTransfer.APDLaserGetHWVersion();
            //Workspace.This.ScannerVM.FPGAVersion = Workspace.This.ApdVM.APDTransfer.FPGAVersion;
            //Workspace.This.MotorVM.IsNewFirmware = true;

            //Workspace.This.ApdVM.APDTransfer.DevicePropertiesGet();
            //Workspace.This.ScannerVM.LaserAPower = 10.0f;
            //Workspace.This.ScannerVM.LaserBPower = 10.0f;
            //Workspace.This.ScannerVM.LaserCPower = 10.0f;
            //Workspace.This.ScannerVM.LaserDPower = 10.0f;
            return true;
        }
    }
}
