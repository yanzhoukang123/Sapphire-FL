using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Azure.Utilities;    //WindowStateManager, MruManager
using Azure.ImagingSystem;
using Azure.LaserScanner;
using Azure.LaserScanner.ViewModel;
using Azure.Configuration.Settings;
using Azure.LaserScanner.SplashScreen;
using Azure.EthernetCommLib;
using TaskDialogInterop;

namespace Azure.LaserScanner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private data...

        //private string _AdDefaultLayoutResourceName = "Azure.LaserScanner.Resources.AdDefaultLayoutFile.xml";
        //private string _AdLayoutFileName = System.IO.Path.Combine(Environment.CurrentDirectory, "AdLayoutFile.xml");
        //private string _AdLayoutFileName = "AdLayoutFile.xml";
        //private bool _IsAvalonLoaded = false;
        //private WindowStateManager _WindowStateManager;
        //System.Timers.Timer _GalilMotorStatusTimer;
        //private ProgressDialogHelper _ProgressDialogHelper = null;
        //private int _TempCounter = 0;

        bool _IsLaserModuleL1Changed = false;
        bool _IsLaserModuleR1Changed = false;
        bool _IsLaserModuleR2Changed = false;

        Task _HardwareInitTask;

        #endregion

        public string ProductVersion { get; set; }

        public MainWindow(string commonAppDataPath,
                          string companyName,
                          string productName,
                          string productVersion)
        {
            // Create WindowStateManager and associate it with ApplicationSettings.MainWindowStateInfo.
            // This allows to set initial window state and track state changes in
            // the Settings.MainWindowStateInfo instance.
            // When application is closed, ApplicationSettings is saved with new window state
            // information. Next time this information is loaded from XML file.
            //_WindowStateManager = new WindowStateManager(SettingsManager.ApplicationSettings.MainWindowStateInfo, this);

            InitializeComponent();

            Workspace.This.CompanyName = companyName;
            Workspace.This.ProductName = productName;
            Workspace.This.ProductVersion = productVersion;
            Workspace.This.AppDataPath = commonAppDataPath;
            Workspace.This.LoggerSetup();

            DataContext = Workspace.This;
            Workspace.This.Owner = this;

            Loaded += new RoutedEventHandler(MainWindow_Loaded);
            Closing += new System.ComponentModel.CancelEventHandler(MainWindow_Closing);
            Closed += new EventHandler(MainWindow_Closed);
            StateChanged += new EventHandler(MainWindow_StateChanged);

            if (!SettingsManager.ConfigSettings.IsSimulationMode)
            {
                // System initialization
                //InitializeSystem();

                _HardwareInitTask = Task.Run(() => InitializeSystem())
                    .ContinueWith(task => GetDeviceProperties(task));
                //_HardwareInitTask.Wait();
            }
            else
            {
                Workspace.This.LaserChannelTypeList.Add(LaserChannels.ChannelC, 685);
                Workspace.This.LaserChannelTypeList.Add(LaserChannels.ChannelA, 532);
                Workspace.This.LaserChannelTypeList.Add(LaserChannels.ChannelB, 488);

                var laserL1 = Workspace.This.LaserChannelTypeList[LaserChannels.ChannelC];
                var laserR1 = Workspace.This.LaserChannelTypeList[LaserChannels.ChannelA];
                var laserR2 = Workspace.This.LaserChannelTypeList[LaserChannels.ChannelB];

                Workspace.This.LaserL1 = laserL1;
                Workspace.This.LaserR1 = laserR1;
                Workspace.This.LaserR2 = laserR2;

                LaserTypes laserType = new LaserTypes();
                laserType.LaserChannel = LaserChannels.ChannelC;
                laserType.Wavelength = laserL1;
                laserType.SensorType = IvSensorType.APD;
                Workspace.This.LaserOptions.Add(laserType);
                laserType = new LaserTypes();
                laserType.LaserChannel = LaserChannels.ChannelA;
                laserType.Wavelength = laserR1;
                laserType.SensorType = IvSensorType.APD;
                Workspace.This.LaserOptions.Add(laserType);
                laserType = new LaserTypes();
                laserType.LaserChannel = LaserChannels.ChannelB;
                laserType.Wavelength = laserR2;
                laserType.SensorType = IvSensorType.APD;
                Workspace.This.LaserOptions.Add(laserType);

                Workspace.This.LaserModuleL1.LaserWavelength = laserL1;
                Workspace.This.LaserModuleL1.Filter = "676BP37";
                Workspace.This.LaserModuleR1.LaserWavelength = laserR1;
                Workspace.This.LaserModuleR1.Filter = "572BP28";
                Workspace.This.LaserModuleR2.LaserWavelength = laserR2;
                Workspace.This.LaserModuleR2.Filter = "513BP17";

                Workspace.This.MotorVM.AbsZPos = 1.2;
                Workspace.This.MotorVM.AbsXPos = 9;
                Workspace.This.MotorVM.AbsYPos = 8;

                if (SettingsManager.ConfigSettings.ImagingSettings != null)
                {
                    bool bIsPhosImagingExists = false;
                    foreach (var imagingTab in SettingsManager.ConfigSettings.ImagingSettings)
                    {
                        if (imagingTab != null && imagingTab.ImagingTabType == ImagingType.PhosphorImaging)
                        {
                            bIsPhosImagingExists = true;
                            imagingTab.IsVisible = true;
                        }
                    }
                    //Add the Phosphor Imaging tab
                    if (!bIsPhosImagingExists)
                    {
                        ImagingSettings imagingSet = new ImagingSettings();
                        imagingSet.ImagingTabType = ImagingType.PhosphorImaging;
                        imagingSet.IsVisible = true;
                        SettingsManager.ConfigSettings.ImagingSettings.Add(imagingSet);
                    }
                }
            }

        }

        void MainWindow_StateChanged(object sender, EventArgs e)
        {
            Workspace.This.CurWindowState = this.WindowState;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Workspace.This.ProductVersion = ProductVersion;
            Workspace.This.CurWindowState = this.WindowState;

            if (!SettingsManager.ConfigSettings.IsSimulationMode)
            {
                if (_HardwareInitTask != null)
                {
                    // Wait for the hardware initialization task to be completed
                    _HardwareInitTask.Wait();
                }
            }

            //Workspace.This.SetImagingTabVisibility();

            Workspace.This.LogMessage("Update Protocol");
            UpdateProtocol();

            if (!SettingsManager.ConfigSettings.IsSimulationMode && Workspace.This.EthernetController.IsConnected)
            {
                //System connected...do something...
                //

                // Close splash screen window
                if (!Splasher.IsSplashClosed)
                    Splasher.CloseSplash();

                #region Connection Error Handling

                if (Workspace.This.HasDeviceProperties)
                {
                    if (Workspace.This.LaserL1 == 0 && Workspace.This.LaserR1 == 0 && Workspace.This.LaserR2 == 0)
                    {
                        string caption = "Sapphire FL Biomolecular Imager";
                        string message = "No laser is detected.\nPlease make sure a laser module is installed on the system.";
                        var parent = Application.Current.MainWindow;
                        Xceed.Wpf.Toolkit.MessageBox.Show(parent, message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);

                        // Select the Gallery tab
                        Workspace.This.SelectedApplicationTab = ApplicationTabType.Gallery;
                        return;
                    }

                    // Scanner connected but has a problem reading the device properties
                    if (!Workspace.This.HasDeviceProperties)
                    {
                        Workspace.This.LogMessage("Failed to read system specific parameters!");

                        string caption = "Sapphire FL Biomolecular Imager";
                        string message = "Failed to read system specific parameters. " +
                                        "Please close the application and power cycle the Sapphire FL scanner and try again.\n" +
                                        "If the error persists...please contact our support at\nhttps://azurebiosystems.com/support/";
                        var parent = Application.Current.MainWindow;
                        Xceed.Wpf.Toolkit.MessageBox.Show(parent, message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);

                        // Select the Gallery tab
                        Workspace.This.SelectedApplicationTab = ApplicationTabType.Gallery;
                        return;
                    }
                }
                else
                {
                    string caption = "Sapphire FL Biomolecular Imager";
                    string message = "Error connecting to the scanner.\nPlease make sure the system power is turned on, and the ethernet cable is securely connected and using the correct IP address.";
                    var parent = Application.Current.MainWindow;
                    Xceed.Wpf.Toolkit.MessageBox.Show(parent, message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                    // Select the Gallery tab
                    Workspace.This.SelectedApplicationTab = ApplicationTabType.Gallery;
                    return;
                }
                #endregion

                Workspace.This.LogMessage(string.Format("System serial number:: {0}", Workspace.This.SystemSN));
                Workspace.This.LogMessage(string.Format("Detected laser in L: {0}", Workspace.This.LaserL1));
                Workspace.This.LogMessage(string.Format("Detected laser in R1: {0}", Workspace.This.LaserR1));
                Workspace.This.LogMessage(string.Format("Detected laser in R2: {0}", Workspace.This.LaserR2));

                //780 laser detected- rename the 784 signal level lookup table key to 780
                if (Workspace.This.LaserL1 == 780 || Workspace.This.LaserR1 == 780 || Workspace.This.LaserR2 == 780)
                {
                    if (SettingsManager.ConfigSettings.LasersSignalList.ContainsKey(784))
                    {
                        SettingsManager.ConfigSettings.LasersSignalList.RenameKey(784, 780);
                    }
                }

                // Update lasers' signal list
                Workspace.This.LogMessage("Update Laser Signal");
                UpdateLaserSignal(Workspace.This.SensorML1, Workspace.This.SensorMR1, Workspace.This.SensorMR2);

                // Phosphor Imaging visibility setup: Is Phosphor module installed?
                Workspace.This.LogMessage("Phosphor Imaging Setup");
                PhosphorImagingSetup();

                //Workspace.This.SetImagingTabVisibility();

                // Are the laser modules in the correct port?
                //string strCaption = "Sapphire FL Biomolecular Imager";
                string strMessage1 = string.Empty;
                string strMessage2 = string.Empty;
                int nLaserModuleErrorCount = 0;
                if (Workspace.This.LaserR1 > 0 && !LaserModuleInR1Range(Workspace.This.LaserModuleR1.LaserWavelength))
                {
                    nLaserModuleErrorCount++;
                    strMessage1 = string.Format("Move laser module ({0}) from Port #2 to Port #1 or Port #3.", Workspace.This.LaserModuleR1.LaserWavelength);
                    if (Workspace.This.LaserModuleR1.LaserWavelength == 375)
                        strMessage1 = string.Format("Move laser module ({0}) from Port #2 to Port #1.", Workspace.This.LaserModuleR1.LaserWavelength);
                }
                if (Workspace.This.LaserR2 > 0 && !LaserModuleInR2Range(Workspace.This.LaserModuleR2.LaserWavelength))
                {
                    nLaserModuleErrorCount++;
                    strMessage2 = string.Format("Move laser module ({0}) from Port #3 to Port #1 or Port #2.", Workspace.This.LaserModuleR2.LaserWavelength);
                }
                if (nLaserModuleErrorCount > 0)
                {
                    string lineBreak = (string.IsNullOrEmpty(strMessage1)) ? string.Empty : "\n";
                    string strMessage = string.Format("{0}{1}{2}", strMessage1, lineBreak, strMessage2);
                    bool bIsMultipleLasers = (nLaserModuleErrorCount > 1) ? true : false;
                    DisplayErrorMessageEx(strMessage, bIsMultipleLasers);
                }

                // Laser module(s) changed?
                bool bIsChangeLaserModuleInitiated = SettingsManager.ApplicationSettings.IsChangeLaserModuleInitiated;
                bool bIsAlwaysShowLaserModuleChanged = SettingsManager.ApplicationSettings.IsAlwaysShowLaserModuleChanged;
                if (_IsLaserModuleL1Changed || _IsLaserModuleR1Changed || _IsLaserModuleR2Changed ||
                    bIsChangeLaserModuleInitiated || bIsAlwaysShowLaserModuleChanged)
                {
                    // Laser module changed...do something...
                    //
                    LaserModuleFilterPairingViewModel laserModuleFilterVm = new LaserModuleFilterPairingViewModel();
                    laserModuleFilterVm.FilterOptions = new System.Collections.ObjectModel.ObservableCollection<FilterType>(SettingsManager.ConfigSettings.FilterOptions);

                    if (Workspace.This.LaserL1 > 0)
                    {
                        laserModuleFilterVm.LaserModuleL1 = Workspace.This.LaserModuleL1;
                        if (!_IsLaserModuleL1Changed && Workspace.This.LaserModuleL1.FilterWavelength > 0)
                        {
                            int index = laserModuleFilterVm.FilterOptions.ToList().FindIndex(x => x.Wavelength == Workspace.This.LaserModuleL1.FilterWavelength && x.Bandpass == Workspace.This.LaserModuleL1.FilterBandpass);
                            if (index >= 0)
                            {
                                laserModuleFilterVm.SelectedFilterL1 = laserModuleFilterVm.FilterOptions[index];
                            }
                        }
                        else
                        {
                            laserModuleFilterVm.LaserModuleStatusL1 = "NEW";
                        }
                    }
                    if (Workspace.This.LaserR1 > 0)
                    {
                        laserModuleFilterVm.LaserModuleR1 = Workspace.This.LaserModuleR1;
                        if (!_IsLaserModuleR1Changed && Workspace.This.LaserModuleR1.FilterWavelength > 0)
                        {
                            int index = laserModuleFilterVm.FilterOptions.ToList().FindIndex(x => x.Wavelength == Workspace.This.LaserModuleR1.FilterWavelength && x.Bandpass == Workspace.This.LaserModuleR1.FilterBandpass);
                            if (index >= 0)
                            {
                                laserModuleFilterVm.SelectedFilterR1 = laserModuleFilterVm.FilterOptions[index];
                            }
                        }
                        else
                        {
                            laserModuleFilterVm.LaserModuleStatusR1 = "NEW";
                        }
                    }
                    if (Workspace.This.LaserR2 > 0)
                    {
                        laserModuleFilterVm.LaserModuleR2 = Workspace.This.LaserModuleR2;
                        if (!_IsLaserModuleR2Changed && Workspace.This.LaserModuleR2.FilterWavelength > 0)
                        {
                            int index = laserModuleFilterVm.FilterOptions.ToList().FindIndex(x => x.Wavelength == Workspace.This.LaserModuleR2.FilterWavelength && x.Bandpass == Workspace.This.LaserModuleR2.FilterBandpass);
                            if (index >= 0)
                            {
                                laserModuleFilterVm.SelectedFilterR2 = laserModuleFilterVm.FilterOptions[index];
                            }
                        }
                        else
                        {
                            laserModuleFilterVm.LaserModuleStatusR2 = "NEW";
                        }
                    }

                    string message = string.Empty;
                    if (SettingsManager.ApplicationSettings.IsChangeLaserModuleInitiated ||
                        SettingsManager.ApplicationSettings.IsAlwaysShowLaserModuleChanged)
                    {
                        message = "Did you change a laser module? Please select the corresponding filter.";
                        if (SettingsManager.ApplicationSettings.IsChangeLaserModuleInitiated)
                        {
                            // Reset laser module change initiated flag 
                            SettingsManager.ApplicationSettings.IsChangeLaserModuleInitiated = false;
                        }
                    }
                    else
                    {
                        message = "Detected a laser module changed. Please select the corresponding filter.";
                    }

                    LaserModuleFilterPairing laserModuleFilterEditView = new LaserModuleFilterPairing();
                    laserModuleFilterEditView.Owner = this;
                    laserModuleFilterEditView.InfoText = message;
                    laserModuleFilterEditView.DataContext = laserModuleFilterVm;
                    var dlgResult = laserModuleFilterEditView.ShowDialog();
                    if (dlgResult == true)
                    {
                        //OK selected...do somethinbg
                        //
                        if (Workspace.This.LaserL1 > 0 && laserModuleFilterVm.SelectedFilterL1 != null)
                        {
                            Workspace.This.LaserModuleL1.FilterWavelength = laserModuleFilterVm.SelectedFilterL1.Wavelength;
                            Workspace.This.LaserModuleL1.FilterBandpass = laserModuleFilterVm.SelectedFilterL1.Bandpass;
                            foreach (var laser in Workspace.This.LaserOptions)
                            {
                                // LaserChannels.ChannelC = L1
                                if (laser.LaserChannel == LaserChannels.ChannelC)
                                {
                                    laser.Filter = Workspace.This.LaserModuleL1.Filter;
                                    break;
                                }
                            }
                        }
                        if (Workspace.This.LaserR1 > 0 && laserModuleFilterVm.SelectedFilterR1 != null)
                        {
                            Workspace.This.LaserModuleR1.FilterWavelength = laserModuleFilterVm.SelectedFilterR1.Wavelength;
                            Workspace.This.LaserModuleR1.FilterBandpass = laserModuleFilterVm.SelectedFilterR1.Bandpass;
                            foreach (var laser in Workspace.This.LaserOptions)
                            {
                                // LaserChannels.ChannelA = R1
                                if (laser.LaserChannel == LaserChannels.ChannelA)
                                {
                                    laser.Filter = Workspace.This.LaserModuleR1.Filter;
                                    break;
                                }
                            }
                        }
                        if (Workspace.This.LaserR2 > 0 && laserModuleFilterVm.SelectedFilterR2 != null)
                        {
                            Workspace.This.LaserModuleR2.FilterWavelength = laserModuleFilterVm.SelectedFilterR2.Wavelength;
                            Workspace.This.LaserModuleR2.FilterBandpass = laserModuleFilterVm.SelectedFilterR2.Bandpass;
                            foreach (var laser in Workspace.This.LaserOptions)
                            {
                                // LaserChannels.ChannelB = R2
                                if (laser.LaserChannel == LaserChannels.ChannelB)
                                {
                                    laser.Filter = Workspace.This.LaserModuleR2.Filter;
                                    break;
                                }
                            }
                        }

                        SettingsManager.ApplicationSettings.LaserModuleL1 = Workspace.This.LaserModuleL1;
                        SettingsManager.ApplicationSettings.LaserModuleR1 = Workspace.This.LaserModuleR1;
                        SettingsManager.ApplicationSettings.LaserModuleR2 = Workspace.This.LaserModuleR2;
                        SettingsManager.OnExit();
                    }
                }

                // Enable Sensor Temperature Logging?
                if (SettingsManager.ConfigSettings.IsLaserTempLogging)
                {
                    Workspace.This.LaserSensorTemperatureLogging(true);
                }
            }
            else
            {
                if (SettingsManager.ConfigSettings.IsSimulationMode)
                {
                    //Simulation mode: do something...
                    //

                    if (!Splasher.IsSplashClosed)
                        Splasher.CloseSplash();
                }
                else if (!Workspace.This.EthernetController.IsConnected)
                {
                    //System not connected: do something...
                    //

                    // Select the Gallery tab
                    Workspace.This.SelectedApplicationTab = ApplicationTabType.Gallery;

                    if (!Splasher.IsSplashClosed)
                        Splasher.CloseSplash();

                    string caption = "Sapphire FL Biomolecular Imager";
                    string message = "Error connecting to the scanner.\nPlease make sure the system power is turned on, and the ethernet cable is securely connected and using the correct IP address.";
                    var parent = Application.Current.MainWindow;
                    Xceed.Wpf.Toolkit.MessageBox.Show(parent, message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                }
            }

            Workspace.This.SetImagingTabVisibility();

        }

        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Workspace.This.IsPreparingToScan)
            {
                string caption = "Scanning Mode";
                string message = "Cannot close the application while the scanner is preparing to scan.\nPlease wait for a few second, then try again.";
                var parent = Application.Current.MainWindow;
                Xceed.Wpf.Toolkit.MessageBox.Show(parent, message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                e.Cancel = true; // don't allow the application to close
                return;
            }

            if (Workspace.This.IsScanning)
            {
                string caption = "Scanning Mode";
                string message = "Scanning mode is busy.\nWould you like to terminate the current operation?";
                var parent = Application.Current.MainWindow;
                MessageBoxResult dlgResult = Xceed.Wpf.Toolkit.MessageBox.Show(parent, message, caption, MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (dlgResult == System.Windows.MessageBoxResult.No)
                {
                    e.Cancel = true; // don't allow the application to close
                    return;
                }

                if (Workspace.This.SelectedImagingType == ImagingType.Fluorescence)
                {
                    FluorescenceViewModel viewModel = Workspace.This.FluorescenceVM;
                    if (viewModel != null)
                    {
                        viewModel.ExecuteStopScanCommand(null);
                    }
                }
                else if (Workspace.This.SelectedImagingType == ImagingType.PhosphorImaging)
                {
                    PhosphorViewModel viewModel = Workspace.This.PhosphorVM;
                    if (viewModel != null)
                    {
                        viewModel.ExecuteStopScanCommand(null);
                    }
                }
            }

            if (Workspace.This.CloseAll() == false)
            {
                e.Cancel = true; // don't allow the application to close
                return;
            }

            try
            {
                // Turn off lasers and reset PMT gain
                if (Workspace.This.EthernetController != null && Workspace.This.EthernetController.IsConnected)
                {
                    if (Workspace.This.LaserOptions.Count > 0)
                    {
                        foreach (var laser in Workspace.This.LaserOptions)
                        {
                            if (laser.LaserChannel == LaserChannels.ChannelA)
                            {
                                Workspace.This.EthernetController.SetLaserPower(LaserChannels.ChannelA, 0);
                                if (laser.SensorType == IvSensorType.PMT)
                                {
                                    Workspace.This.EthernetController.SetIvPmtGain(IVChannels.ChannelA, 4000);
                                }
                            }
                            else if (laser.LaserChannel == LaserChannels.ChannelB)
                            {
                                Workspace.This.EthernetController.SetLaserPower(LaserChannels.ChannelB, 0);
                                if (laser.SensorType == IvSensorType.PMT)
                                {
                                    Workspace.This.EthernetController.SetIvPmtGain(IVChannels.ChannelB, 4000);
                                }
                            }
                            else if (laser.LaserChannel == LaserChannels.ChannelC)
                            {
                                Workspace.This.EthernetController.SetLaserPower(LaserChannels.ChannelC, 0);
                                if (laser.SensorType == IvSensorType.PMT)
                                {
                                    Workspace.This.EthernetController.SetIvPmtGain(IVChannels.ChannelC, 4000);
                                }
                            }
                        }
                    }
                    if (SettingsManager.ConfigSettings.IsLaserTempLogging)
                    {
                        Workspace.This.LaserSensorTemperatureLogging(false);
                    }
                    Workspace.This.EthernetController.Disconnect();
                }
            }
            catch
            {
            }

            SettingsManager.OnExit();
        }

        void MainWindow_Closed(object sender, EventArgs e)
        {
            System.Environment.Exit(0);
        }

        private bool InitializeSystem()
        {
            try
            {
                Workspace.This.LogMessage("***** Hardware initialization *****");
                Workspace.This.LogMessage("***********************************");

                //bool bIsXYZmotorHoming = true;
                if (!Workspace.This.ConnectEthernetSlave())
                {
                    Workspace.This.LogMessage("Failed to connect to the main board.");
                    return false;
                }
                else
                {
                    // Short delay before get system parameters/settings
                    //Thread.Sleep(1000);

                    Workspace.This.MotorVM.IsNewFirmware = true;
                    bool hasSystemVersion = false;
                    for (int i = 0; i < 100; i++)
                    {
                        if (Workspace.This.EthernetController.GetSystemVersions())
                        {
                            hasSystemVersion = true;
                            break;
                        }
                        else
                        {
                            Thread.Sleep(1);
                        }
                    }
                    if (!hasSystemVersion)
                    {
                        Workspace.This.LogMessage("Get system version... Failed!");
                        return false;
                    }
                    Workspace.This.FWVersion = Workspace.This.EthernetController.FWVersion;
                    Workspace.This.HWversion = Workspace.This.EthernetController.HWVersion;
                    Workspace.This.LEDVersion = Workspace.This.EthernetController.LEDVersion;
                    Workspace.This.LogMessage(string.Format("Firmware version: {0}", Workspace.This.FWVersion));
                    Workspace.This.LogMessage(string.Format("Hardware version: {0}", Workspace.This.HWversion));
                    Workspace.This.LogMessage(string.Format("LED version: {0}", Workspace.This.LEDVersion));

                    //硬件版本是1.0.0.0时 不支持读取当前的顶部锁状态和顶部磁吸状态，光学模块下电状态 （FW Version 1.1.0.0）
                    // Hardware version 1.0.0.0, does not support reading the current top lock status, top magnet status, and optical module power-off status(FW Version 1.1.0.0)
                    if (Workspace.This.HWversion == Workspace.This.DefaultHWversion)
                    {
                        if (Workspace.This.EthernetController.DevicePowerStatus == false)
                        {
                            return false;
                        }
                        bool OpticalModulePowerStatus = Workspace.This.EthernetController.OpticalModulePowerStatus;
                        if (!OpticalModulePowerStatus)
                        {
                            //If the firmware version is 1.1.0.0 and the LED version number is "254.255.255.255", perform the following operation
                            if (Workspace.This.LEDVersion == Workspace.This.NewParameterVM.Str16Code)
                            {
                                return false;
                            }
                            else
                            {
                                Workspace.This.IsMotorAlive = false;
                            }
                        }
                    }

                    Workspace.This.LogMessage("Initializing motor controller...");
                    Workspace.This.MotorVM.InitMotionController();

                    #region === Home the motors ===

                    // Home the XYZ motors
                    if (SettingsManager.ConfigSettings.HomeMotionsAtLaunchTime)
                    {
                        Workspace.This.LogMessage("Homing X/Y/Z motors...");

                        if (!Workspace.This.MotorVM.HomeXYZmotor())
                        {
                            Workspace.This.LogMessage("Homing X/Y/Z motors: Failed!");
                        }
                        else
                        {
                            const int nWaitTime = 45000;    // takes about 45 seconds to home the stage from the top right corner
                            int nWaitCounter = 0;
                            bool bIsXYZmotorHoming = true;
                            while (bIsXYZmotorHoming && nWaitCounter < nWaitTime)
                            {
                                if (Workspace.This.MotorVM.MotionController.CrntState[MotorTypes.X].AtHome &&
                                    Workspace.This.MotorVM.MotionController.CrntState[MotorTypes.Y].AtHome &&
                                    Workspace.This.MotorVM.MotionController.CrntState[MotorTypes.Z].AtHome)
                                {
                                    bIsXYZmotorHoming = false;
                                    Workspace.This.LogMessage("Homing X/Y/Z motors: Succeeded!");
                                }
                                System.Threading.Thread.Sleep(1);
                                nWaitCounter++;
                            }
                        }
                    }

                    #endregion

                    //Workspace.This.EthernetController.GetXEncoderSubdivision();
                    //SettingsManager.ConfigSettings.XEncoderSubdivision = (int)Workspace.This.EthernetController.XEncoderSubdivision;
                    //获取所有IV板子的信息
                    //Get information about all IV boards
                    if (Workspace.This.EthernetController.GetAllIvModulesInfo())
                    {
                        Workspace.This.LogMessage("GetAllIvModulesInfo: Succeeded!");
                    }
                    else
                    {
                        Workspace.This.LogMessage("GetAllIvModulesInfo: Failed!");
                    }

                    Workspace.This.IsMotorAlive = true;
                    //Workspace.This.EthernetController.GetAllIvModulesInfo();

                    //获取所有通道激光信息
                    //Get all channel laser information
                    if (Workspace.This.EthernetController.GetAllLaserModulseInfo())
                    {
                        Workspace.This.LogMessage("GetAllLaserModulseInfo: Succeeded!");
                    }
                    else
                    {
                        Workspace.This.LogMessage("GetAllLaserModulseInfo: Failed!");
                    }
                }

                System.Threading.Thread.Sleep(2000);

                Workspace.This.LogMessage("System successfully initialized!");
                Workspace.This.LogMessage("***********************************");
                return true;
            }
            catch
            {
                throw;
            }
        }

        private bool GetDeviceProperties(Task<bool> task)
        {
            bool bResult = false;

            if (task.Result == true)
            {
                bool hasDevProperties = false;
                for (int i = 0; i < 100; i++)
                {
                    if (Workspace.This.EthernetController.GetDeviceProperties())
                    {
                        hasDevProperties = true;
                        break;
                    }
                    else
                    {
                        // hang around for a msec and re-try...
                        System.Threading.Thread.Sleep(1);
                    }
                }

                if (hasDevProperties)
                {
                    // hang around for a second before get specific device properties...
                    //System.Threading.Thread.Sleep(1000);

                    //Workspace.This.MotorVM.LimitsXPlus= Workspace.This.MotorVM.LimitsXPlus- Workspace.This.EthernetController.DeviceProperties.OpticalLR2Distance;
                    Workspace.This.MotorVM.AbsZPos = Workspace.This.EthernetController.DeviceProperties.ZFocusPosition;
                    Workspace.This.MotorVM.AbsXPos = Workspace.This.EthernetController.DeviceProperties.LogicalHomeX;
                    Workspace.This.MotorVM.AbsYPos = Workspace.This.EthernetController.DeviceProperties.LogicalHomeY;
                    // Use the XEncoderSubdivision value saved on firmware.
                    SettingsManager.ConfigSettings.XEncoderSubdivision = Workspace.This.EthernetController.DeviceProperties.XEncoderSubdivision;
                    if (Workspace.This.EthernetController.DeviceProperties.SysSN != null)
                    {
                        Workspace.This.SystemSN = Encoding.ASCII.GetString(Workspace.This.EthernetController.DeviceProperties.SysSN).TrimEnd('\0');
                    }

                    // New mainboard with new firmware return 65534 as the wavelength value when a laser module is not mounted in the laser module port.
                    //
                    //int nWavelengthL1 = (int)EthernetController.LaserWaveLengths[LaserChannels.ChannelC];
                    //int nWavelengthR2 = (int)EthernetController.LaserWaveLengths[LaserChannels.ChannelB];
                    //int nWavelengthR1 = (int)EthernetController.LaserWaveLengths[LaserChannels.ChannelA];
                    //Workspace.This.SensorML1 = EthernetController.IvSensorTypes[IVChannels.ChannelC];//C代表L通道     Enumeration C represents an L1 channel 
                    //Workspace.This.SensorMR1 = EthernetController.IvSensorTypes[IVChannels.ChannelA];//A代表R1通道    Enumeration A represents an R1 channel
                    //Workspace.This.SensorMR2 = EthernetController.IvSensorTypes[IVChannels.ChannelB];//B代表R2通道    Enumeration B represents an R2 channel
                    //Workspace.This.LaserSNL1 = EthernetController.LaserSerialNumbers[LaserChannels.ChannelC].ToString("X8");
                    //Workspace.This.LaserSNR1 = EthernetController.LaserSerialNumbers[LaserChannels.ChannelA].ToString("X8");
                    //Workspace.This.LaserSNR2 = EthernetController.LaserSerialNumbers[LaserChannels.ChannelB].ToString("X8");
                    int nWavelengthL1 = EthernetController.LaserWaveLengths[LaserChannels.ChannelC] == Workspace.This.NewParameterVM.Uint16Code ? 0 : (int)EthernetController.LaserWaveLengths[LaserChannels.ChannelC];
                    int nWavelengthR1 = EthernetController.LaserWaveLengths[LaserChannels.ChannelA] == Workspace.This.NewParameterVM.Uint16Code ? 0 : (int)EthernetController.LaserWaveLengths[LaserChannels.ChannelA];
                    int nWavelengthR2 = EthernetController.LaserWaveLengths[LaserChannels.ChannelB] == Workspace.This.NewParameterVM.Uint16Code ? 0 : (int)EthernetController.LaserWaveLengths[LaserChannels.ChannelB];
                    var sensorML1 = EthernetController.IvSensorTypes[IVChannels.ChannelC];
                    Workspace.This.SensorML1 = (int)sensorML1 == Workspace.This.NewParameterVM.Uint8Code ? IvSensorType.NA : sensorML1;
                    var sensorMR1 = EthernetController.IvSensorTypes[IVChannels.ChannelA];
                    Workspace.This.SensorMR1 = (int)sensorMR1 == Workspace.This.NewParameterVM.Uint8Code ? IvSensorType.NA : sensorMR1;
                    var sensorMR2 = EthernetController.IvSensorTypes[IVChannels.ChannelB];
                    Workspace.This.SensorMR2 = (int)sensorMR2 == Workspace.This.NewParameterVM.Uint8Code ? IvSensorType.NA : sensorMR2;
                    var sensorSNL1 = EthernetController.LaserSerialNumbers[LaserChannels.ChannelC].ToString("X8");
                    Workspace.This.LaserSNL1 = (sensorSNL1 == Workspace.This.NewParameterVM.StrEmptyCode1) ? "NaN" : sensorSNL1;
                    var sensorSNR1 = EthernetController.LaserSerialNumbers[LaserChannels.ChannelA].ToString("X8");
                    Workspace.This.LaserSNR1 = (sensorSNR1 == Workspace.This.NewParameterVM.StrEmptyCode1) ? "NaN" : sensorSNR1;
                    var sensorSNR2 = EthernetController.LaserSerialNumbers[LaserChannels.ChannelB].ToString("X8");
                    Workspace.This.LaserSNR2 = (sensorSNR2 == Workspace.This.NewParameterVM.StrEmptyCode1) ? "NaN" : sensorSNR2;

                    //nWavelengthL1 = (nWavelengthL1 == 780) ? 784 : nWavelengthL1;
                    //nWavelengthR1 = (nWavelengthR1 == 780) ? 784 : nWavelengthR1;
                    //nWavelengthR2 = (nWavelengthR2 == 780) ? 784 : nWavelengthR2;

                    // Installed lasers' wavelength
                    Workspace.This.LaserL1 = nWavelengthL1;
                    Workspace.This.LaserR1 = nWavelengthR1;
                    Workspace.This.LaserR2 = nWavelengthR2;

                    UpdateLaserModule(Workspace.This.LaserL1, Workspace.This.LaserR1, Workspace.This.LaserR2,
                                      Workspace.This.SensorML1, Workspace.This.SensorMR1, Workspace.This.SensorMR2,
                                      Workspace.This.LaserSNL1, Workspace.This.LaserSNR1, Workspace.This.LaserSNR2);

                    // WORK-AROUND: don't consider it a successful connection if did not detect any laser module.
                    //              this is to avoid hanging waiting for the motors to home; only home the motors
                    //              when detected a laser module.
                    /*if (Workspace.This.LaserL1 == 0 && Workspace.This.LaserR1 == 0 && Workspace.This.LaserR2 == 0)
                    {
                        Workspace.This.HasDeviceProperties = false;
                        return false;
                    }
                    else
                    {
                        #region === Home the motors ===

                        //Software starts XYZ motor back to Home
                        if (SettingsManager.ConfigSettings.HomeMotionsAtLaunchTime)
                        {
                            Workspace.This.LogMessage("Homing X/Y/Z motors...");

                            if (!Workspace.This.MotorVM.HomeXYZmotor())
                            {
                                Workspace.This.LogMessage("Homing X/Y/Z motors: Failed!");
                            }
                            else
                            {
                                const int nWaitTime = 45000;    // takes about 45 seconds to home the stage from the top right corner
                                int nWaitCounter = 0;
                                bool bIsXYZmotorHoming = true;
                                while (bIsXYZmotorHoming && nWaitCounter < nWaitTime)
                                {
                                    if (Workspace.This.MotorVM.MotionController.CrntState[MotorTypes.X].AtHome &&
                                        Workspace.This.MotorVM.MotionController.CrntState[MotorTypes.Y].AtHome &&
                                        Workspace.This.MotorVM.MotionController.CrntState[MotorTypes.Z].AtHome)
                                    {
                                        bIsXYZmotorHoming = false;
                                        Workspace.This.LogMessage("Homing X/Y/Z motors: Succeeded!");
                                    }
                                    else
                                    {
                                        bIsXYZmotorHoming = true;
                                    }
                                    System.Threading.Thread.Sleep(1);
                                    nWaitCounter++;
                                }
                            }
                        }

                        #endregion
                    }*/

                    Workspace.This.HasDeviceProperties = true;
                    bResult = true;
                }
            }

            return bResult;
        }

        private void UpdateLaserModule(int nWavelengthL1, int nWavelengthR1, int nWavelengthR2,
                                       IvSensorType sensorML1, IvSensorType sensorMR1, IvSensorType sensorMR2,
                                       string laserSNL1, string laserSNR1, string laserSNR2)
        {
            if (nWavelengthL1 != 0)
            {
                Workspace.This.LaserChannelTypeList.Add(LaserChannels.ChannelC, nWavelengthL1);
                LaserTypes laserType = new LaserTypes();
                laserType.LaserChannel = LaserChannels.ChannelC;
                laserType.Wavelength = nWavelengthL1;
                laserType.SensorType = sensorML1;
                if (SettingsManager.ApplicationSettings.LaserModuleL1 != null)
                {
                    if (SettingsManager.ApplicationSettings.LaserModuleL1.LaserWavelength != laserType.Wavelength ||
                        SettingsManager.ApplicationSettings.LaserModuleL1.SensorType != laserType.SensorType ||
                        SettingsManager.ApplicationSettings.LaserModuleL1.LaserSN != laserSNL1)
                    {
                        SettingsManager.ApplicationSettings.LaserModuleL1.LaserWavelength = laserType.Wavelength;
                        SettingsManager.ApplicationSettings.LaserModuleL1.SensorType = laserType.SensorType;
                        SettingsManager.ApplicationSettings.LaserModuleL1.LaserChannel = LaserChannels.ChannelC;
                        SettingsManager.ApplicationSettings.LaserModuleL1.LaserSN = laserSNL1;
                        _IsLaserModuleL1Changed = true;
                    }
                }
                if (SettingsManager.ApplicationSettings.LaserModuleL1.FilterWavelength != 0)
                {
                    laserType.Filter = SettingsManager.ApplicationSettings.LaserModuleL1.Filter;
                }
                Workspace.This.LaserOptions.Add(laserType);
            }
            else
            {
                // Reset previous laser modules
                SettingsManager.ApplicationSettings.LaserModuleL1.LaserWavelength = 0;
                SettingsManager.ApplicationSettings.LaserModuleL1.SensorType = IvSensorType.NA;
                SettingsManager.ApplicationSettings.LaserModuleL1.LaserSN = string.Empty;
                SettingsManager.ApplicationSettings.LaserModuleL1.FilterWavelength = 0;
                SettingsManager.ApplicationSettings.LaserModuleL1.FilterBandpass = string.Empty;
                SettingsManager.ApplicationSettings.LaserModuleL1.LaserSN = string.Empty;
                if (SettingsManager.ApplicationSettings.LaserModuleL1.LaserWavelength != 0)
                {
                    // Laser module removed.
                    _IsLaserModuleL1Changed = true;
                }
            }
            if (nWavelengthR1 != 0)
            {
                Workspace.This.LaserChannelTypeList.Add(LaserChannels.ChannelA, nWavelengthR1);
                LaserTypes laserType = new LaserTypes();
                laserType.LaserChannel = LaserChannels.ChannelA;
                laserType.Wavelength = nWavelengthR1;
                laserType.SensorType = sensorMR1;
                if (SettingsManager.ApplicationSettings.LaserModuleR1 != null)
                {
                    if (SettingsManager.ApplicationSettings.LaserModuleR1.LaserWavelength != laserType.Wavelength ||
                        SettingsManager.ApplicationSettings.LaserModuleR1.SensorType != laserType.SensorType ||
                        SettingsManager.ApplicationSettings.LaserModuleR1.LaserSN != laserSNR1)
                    {
                        SettingsManager.ApplicationSettings.LaserModuleR1.LaserWavelength = laserType.Wavelength;
                        SettingsManager.ApplicationSettings.LaserModuleR1.SensorType = laserType.SensorType;
                        SettingsManager.ApplicationSettings.LaserModuleR1.LaserChannel = LaserChannels.ChannelA;
                        SettingsManager.ApplicationSettings.LaserModuleR1.LaserSN = laserSNR1;
                        _IsLaserModuleR1Changed = true;
                    }
                }
                if (SettingsManager.ApplicationSettings.LaserModuleR1.FilterWavelength != 0)
                {
                    laserType.Filter = SettingsManager.ApplicationSettings.LaserModuleR1.Filter;
                }
                Workspace.This.LaserOptions.Add(laserType);
            }
            else
            {
                // Laser module removed.
                // Reset previous laser modules
                SettingsManager.ApplicationSettings.LaserModuleR1.LaserWavelength = 0;
                SettingsManager.ApplicationSettings.LaserModuleR1.SensorType = IvSensorType.NA;
                SettingsManager.ApplicationSettings.LaserModuleR1.LaserSN = string.Empty;
                SettingsManager.ApplicationSettings.LaserModuleR1.FilterWavelength = 0;
                SettingsManager.ApplicationSettings.LaserModuleR1.FilterBandpass = string.Empty;
                SettingsManager.ApplicationSettings.LaserModuleR1.LaserSN = string.Empty;
                if (SettingsManager.ApplicationSettings.LaserModuleR1.LaserWavelength != 0)
                {
                    _IsLaserModuleR1Changed = true;
                }
            }
            if (nWavelengthR2 != 0)
            {
                Workspace.This.LaserChannelTypeList.Add(LaserChannels.ChannelB, nWavelengthR2);
                LaserTypes laserType = new LaserTypes();
                laserType.LaserChannel = LaserChannels.ChannelB;
                laserType.Wavelength = nWavelengthR2;
                laserType.SensorType = sensorMR2;
                if (SettingsManager.ApplicationSettings.LaserModuleR2 != null)
                {
                    if (SettingsManager.ApplicationSettings.LaserModuleR2.LaserWavelength != laserType.Wavelength ||
                        SettingsManager.ApplicationSettings.LaserModuleR2.SensorType != laserType.SensorType ||
                        SettingsManager.ApplicationSettings.LaserModuleR2.LaserSN != laserSNR2)
                    {
                        SettingsManager.ApplicationSettings.LaserModuleR2.LaserWavelength = laserType.Wavelength;
                        SettingsManager.ApplicationSettings.LaserModuleR2.SensorType = laserType.SensorType;
                        SettingsManager.ApplicationSettings.LaserModuleR2.LaserChannel = LaserChannels.ChannelB;
                        SettingsManager.ApplicationSettings.LaserModuleR2.LaserSN = laserSNR2;
                        _IsLaserModuleR2Changed = true;
                    }
                }
                if (SettingsManager.ApplicationSettings.LaserModuleR2.FilterWavelength != 0)
                {
                    laserType.Filter = SettingsManager.ApplicationSettings.LaserModuleR2.Filter;
                }
                Workspace.This.LaserOptions.Add(laserType);
            }
            else
            {
                // Reset previous laser modules
                SettingsManager.ApplicationSettings.LaserModuleR2.LaserWavelength = 0;
                SettingsManager.ApplicationSettings.LaserModuleR2.SensorType = IvSensorType.NA;
                SettingsManager.ApplicationSettings.LaserModuleR2.LaserSN = string.Empty;
                SettingsManager.ApplicationSettings.LaserModuleR2.FilterWavelength = 0;
                SettingsManager.ApplicationSettings.LaserModuleR2.FilterBandpass = string.Empty;
                SettingsManager.ApplicationSettings.LaserModuleR2.LaserSN = string.Empty;
                if (SettingsManager.ApplicationSettings.LaserModuleR2.LaserWavelength != 0)
                {
                    // Laser module removed.
                    _IsLaserModuleR2Changed = true;
                }
            }

            Workspace.This.LaserModuleL1 = SettingsManager.ApplicationSettings.LaserModuleL1;
            Workspace.This.LaserModuleR1 = SettingsManager.ApplicationSettings.LaserModuleR1;
            Workspace.This.LaserModuleR2 = SettingsManager.ApplicationSettings.LaserModuleR2;

            if (Workspace.This.LaserModuleL1 != null)
            {
                if (Workspace.This.LaserModuleL1.LaserWavelength > 0)
                {
                    if (Workspace.This.LaserModuleL1.FilterWavelength == 0)
                    {
                        _IsLaserModuleL1Changed = true;
                    }
                }
            }
            if (Workspace.This.LaserModuleR1 != null)
            {
                if (Workspace.This.LaserModuleR1.LaserWavelength > 0)
                {
                    if (Workspace.This.LaserModuleR1.FilterWavelength == 0)
                    {
                        _IsLaserModuleR1Changed = true;
                    }
                }
            }
            if (Workspace.This.LaserModuleR2 != null)
            {
                if (Workspace.This.LaserModuleR2.LaserWavelength > 0)
                {
                    if (Workspace.This.LaserModuleR2.FilterWavelength == 0)
                    {
                        _IsLaserModuleR2Changed = true;
                    }
                }
            }
        }

        private void UpdateLaserSignal(IvSensorType sensorML1, IvSensorType sensorMR1, IvSensorType sensorMR2)
        {
            if (SettingsManager.ConfigSettings.LasersSignalList.ContainsKey(Workspace.This.LaserL1))
            {
                for (int i = 0; i < SettingsManager.ConfigSettings.LasersSignalList[Workspace.This.LaserL1].Count; i++)
                {
                    SettingsManager.ConfigSettings.LasersSignalList[Workspace.This.LaserL1][i].SensorType = sensorML1;
                    SettingsManager.ConfigSettings.LasersSignalList[Workspace.This.LaserL1][i].LaserChannel = LaserChannels.ChannelC;
                    SettingsManager.ConfigSettings.LasersSignalList[Workspace.This.LaserL1][i].LaserWavelength = Workspace.This.LaserL1;
                }
            }
            if (SettingsManager.ConfigSettings.LasersSignalList.ContainsKey(Workspace.This.LaserR1))
            {
                for (int i = 0; i < SettingsManager.ConfigSettings.LasersSignalList[Workspace.This.LaserR1].Count; i++)
                {
                    SettingsManager.ConfigSettings.LasersSignalList[Workspace.This.LaserR1][i].SensorType = sensorMR1;
                    SettingsManager.ConfigSettings.LasersSignalList[Workspace.This.LaserR1][i].LaserChannel = LaserChannels.ChannelA;
                    SettingsManager.ConfigSettings.LasersSignalList[Workspace.This.LaserR1][i].LaserWavelength = Workspace.This.LaserR1;

                }
            }
            if (SettingsManager.ConfigSettings.LasersSignalList.ContainsKey(Workspace.This.LaserR2))
            {
                for (int i = 0; i < SettingsManager.ConfigSettings.LasersSignalList[Workspace.This.LaserR2].Count; i++)
                {
                    SettingsManager.ConfigSettings.LasersSignalList[Workspace.This.LaserR2][i].SensorType = sensorMR2;
                    SettingsManager.ConfigSettings.LasersSignalList[Workspace.This.LaserR2][i].LaserChannel = LaserChannels.ChannelB;
                    SettingsManager.ConfigSettings.LasersSignalList[Workspace.This.LaserR2][i].LaserWavelength = Workspace.This.LaserR2;
                }
            }
        }

        private void PhosphorImagingSetup()
        {
            // Is Phosphor module installed?
            bool bIsPhosphorModule = false;
            LaserTypes phosphorLaser = null;
            var itemsFound = Workspace.This.LaserOptions.Where(x => x.SensorType == IvSensorType.PMT).ToList();
            if (itemsFound != null && itemsFound.Count > 0)
            {
                foreach (var item in itemsFound)
                {
                    if (item.Wavelength == 638 || item.Wavelength == 658 || item.Wavelength == 685)
                    {
                        bIsPhosphorModule = true;
                        phosphorLaser = item;
                        break;
                    }
                }
            }

            if (bIsPhosphorModule && Workspace.This.LaserOptions.Count == 1)
            {
                if (SettingsManager.ConfigSettings.ImagingSettings != null)
                {
                    SettingsManager.ConfigSettings.ImagingSettings.Clear();
                    Workspace.This.IsFluorescenceImagingVisible = false;
                }
            }

            // Set Phosphor Imaging tab visibility
            if (bIsPhosphorModule)
            {
                if (SettingsManager.ConfigSettings.ImagingSettings != null)
                {
                    bool bIsPhosImagingExists = false;
                    foreach (var imagingTab in SettingsManager.ConfigSettings.ImagingSettings)
                    {
                        if (imagingTab != null && imagingTab.ImagingTabType == ImagingType.PhosphorImaging)
                        {
                            bIsPhosImagingExists = true;
                            imagingTab.IsVisible = true;
                        }
                    }
                    // Add the Phosphor Imaging tab
                    if (!bIsPhosImagingExists)
                    {
                        ImagingSettings imagingSet = new ImagingSettings();
                        imagingSet.ImagingTabType = ImagingType.PhosphorImaging;
                        imagingSet.IsVisible = true;
                        SettingsManager.ConfigSettings.ImagingSettings.Add(imagingSet);
                    }
                }

                // Set installed/mounted phosphor module laser channel
                if (SettingsManager.ConfigSettings.PhosphorSignalOptions != null && SettingsManager.ConfigSettings.PhosphorSignalOptions.Count > 0)
                {
                    for (int i = 0; i < SettingsManager.ConfigSettings.PhosphorSignalOptions.Count; i++)
                    {
                        if (SettingsManager.ConfigSettings.PhosphorSignalOptions[i].LaserWavelength == phosphorLaser.Wavelength)
                        {
                            SettingsManager.ConfigSettings.PhosphorSignalOptions[i].LaserChannel = phosphorLaser.LaserChannel;
                            SettingsManager.ConfigSettings.PhosphorSignalOptions[i].SensorType = phosphorLaser.SensorType;
                        }
                    }
                }
                if (SettingsManager.ConfigSettings.PhosphorProtocols != null && SettingsManager.ConfigSettings.PhosphorProtocols.Count > 0)
                {
                    for (int i = 0; i < SettingsManager.ConfigSettings.PhosphorProtocols.Count; i++)
                    {
                        foreach (var scanRegion in SettingsManager.ConfigSettings.PhosphorProtocols[i].ScanRegions)
                        {
                            scanRegion.Lasers.Clear();
                            scanRegion.Lasers.Add(phosphorLaser);
                        }
                    }
                }

                // Is Phosphor module in Port #1?
                if (phosphorLaser != null)
                {
                    if (phosphorLaser.LaserChannel != LaserChannels.ChannelC)
                    {
                        string strPort = string.Empty;
                        if (phosphorLaser.LaserChannel == LaserChannels.ChannelA)
                            strPort = "Port #2";
                        else if (phosphorLaser.LaserChannel == LaserChannels.ChannelB)
                            strPort = "Port #3";
                        string strMessage1 = string.Format("Move laser module ({0}) from {1} to Port #1.", phosphorLaser.Wavelength, strPort);
                        DisplayErrorMessageEx(strMessage1);
                    }
                }
            }
        }

        private void UpdateProtocol()
        {
            //var protocols = new List<Protocol>(SettingsManager.ConfigSettings.Protocols);
            bool bIsProtocolRemoved = false;
            if (SettingsManager.ConfigSettings.Protocols != null && SettingsManager.ConfigSettings.Protocols.Count > 0)
            {
                for (int i = SettingsManager.ConfigSettings.Protocols.Count - 1; i >= 0; i--)
                {
                    // Remove a protocol with non-matching laser type,
                    //   and update the laser channel of the protocol with the matching laser type (wavelength)
                    foreach (var scanRegion in SettingsManager.ConfigSettings.Protocols[i].ScanRegions)
                    {
                        for (int j = scanRegion.Lasers.Count - 1; j >= 0; j--)
                        {
                            bIsProtocolRemoved = false;
                            // Throws exception if there's more than one is found on the list
                            //var laserType = Workspace.This.LaserOptions.SingleOrDefault(item => item.Wavelength == scanRegion.Lasers[j].Wavelength);
                            var itemsFound = Workspace.This.LaserOptions.Where(x => x.Wavelength == scanRegion.Lasers[j].Wavelength).ToList();
                            LaserTypes laserType = null;
                            if (itemsFound != null && itemsFound.Count > 0)
                            {
                                // Possible multiple items are found with the same laser wavelength (APD vs PMT)
                                foreach (var item in itemsFound)
                                {
                                    if ((item.Wavelength == 638 || item.Wavelength == 658 || item.Wavelength == 685) && item.SensorType == IvSensorType.PMT)
                                    {
                                        //Found Phosphor module...
                                    }
                                    else
                                    {
                                        laserType = item;
                                    }
                                }
                            }

                            if (laserType != null)
                            {
                                scanRegion.Lasers[j].LaserChannel = laserType.LaserChannel;
                            }
                            else
                            {
                                if (SettingsManager.ConfigSettings.Protocols[i].Name.Contains("New Protocol"))
                                {
                                    if (Workspace.This.LaserOptions != null && Workspace.This.LaserOptions.Count > 0)
                                    {
                                        foreach (var laser in Workspace.This.LaserOptions)
                                        {
                                            if (!Workspace.This.IsPhosphorModule(laser))
                                            {
                                                SettingsManager.ConfigSettings.Protocols[i].ScanRegions[0].Lasers[0].LaserChannel = laser.LaserChannel;
                                                SettingsManager.ConfigSettings.Protocols[i].ScanRegions[0].Lasers[0].SensorType = laser.SensorType;
                                                SettingsManager.ConfigSettings.Protocols[i].ScanRegions[0].Lasers[0].Wavelength = laser.Wavelength;
                                            }
                                        }
                                    }
                                }
                                else if (!SettingsManager.ConfigSettings.Protocols[i].Name.Contains("New Protocol"))
                                {
                                    if (SettingsManager.ConfigSettings.Protocols[i].IsAlwaysVisible)
                                    {
                                        // Special Case: remove the non-matching laser in the scan region, but don't remove the protocol marked as always visible
                                        //   even if all the lasers in the protocol doesn't match the current installed laser modules
                                        if (scanRegion.Lasers.Count > 1)
                                        {
                                            scanRegion.Lasers.Remove(scanRegion.Lasers[j]);
                                        }
                                        else
                                        {
                                            if (Workspace.This.LaserOptions != null && Workspace.This.LaserOptions.Count > 0)
                                            {
                                                // No matching laser module; set to the first mounted laser module
                                                scanRegion.Lasers[0].LaserChannel = Workspace.This.LaserOptions[0].LaserChannel;
                                                scanRegion.Lasers[0].SensorType = Workspace.This.LaserOptions[0].SensorType;
                                                scanRegion.Lasers[0].Wavelength = Workspace.This.LaserOptions[0].Wavelength;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        SettingsManager.ConfigSettings.Protocols.Remove(SettingsManager.ConfigSettings.Protocols[i]);
                                        bIsProtocolRemoved = true;
                                        break;  // break out of for loop
                                    }
                                }
                                else
                                {
                                    if (Workspace.This.LaserOptions != null && Workspace.This.LaserOptions.Count > 0)
                                    {
                                        SettingsManager.ConfigSettings.Protocols[i].ScanRegions[0].Lasers[0].LaserChannel = Workspace.This.LaserOptions[0].LaserChannel;
                                        SettingsManager.ConfigSettings.Protocols[i].ScanRegions[0].Lasers[0].SensorType = Workspace.This.LaserOptions[0].SensorType;
                                        SettingsManager.ConfigSettings.Protocols[i].ScanRegions[0].Lasers[0].Wavelength = Workspace.This.LaserOptions[0].Wavelength;
                                    }
                                }
                            }
                        }
                        if (bIsProtocolRemoved)
                        {
                            break;  // break out of foreach loop
                        }
                    }
                }

                // Sort the protocol list and move "New Protocol" to the top of the list
                if (SettingsManager.ConfigSettings.Protocols != null && SettingsManager.ConfigSettings.Protocols.Count > 1)
                {
                    SettingsManager.ConfigSettings.Protocols = new List<Protocol>(SettingsManager.ConfigSettings.Protocols.OrderBy(p => p.Name));
                    int index = SettingsManager.ConfigSettings.Protocols.FindIndex(x => x.Name == "New Protocol");
                    if (index != -1)
                    {
                        // Move bottom item to top
                        SettingsManager.ConfigSettings.Protocols.MoveToTop(index);
                        //Protocol item = SettingsManager.ConfigSettings.Protocols[index];
                        //for (int i = index; i > 0; i--)
                        //{
                        //    SettingsManager.ConfigSettings.Protocols[i] = SettingsManager.ConfigSettings.Protocols[i - 1];
                        //}
                        //SettingsManager.ConfigSettings.Protocols[0] = item;
                    }
                }
            }

            // Sort the phosphor imaging protocol list and move the default "Phosphor Imaging" protocol to the top of the list
            if (SettingsManager.ConfigSettings.PhosphorProtocols != null && SettingsManager.ConfigSettings.PhosphorProtocols.Count > 1)
            {
                SettingsManager.ConfigSettings.PhosphorProtocols = new List<Protocol>(SettingsManager.ConfigSettings.PhosphorProtocols.OrderBy(p => p.Name));
                int index = SettingsManager.ConfigSettings.PhosphorProtocols.FindIndex(x => x.Name == "Phosphor Imaging");
                if (index != -1)
                {
                    // Move bottom item to top
                    SettingsManager.ConfigSettings.PhosphorProtocols.MoveToTop(index);
                }
            }
        }

        /*private void Delay(int mm)
        {
            DateTime current = DateTime.Now;

            while (current.AddMilliseconds(mm) > DateTime.Now)
            {
                System.Windows.Forms.Application.DoEvents();
            }
            return;
        }*/

        private bool LaserModuleInR1Range(int laserWavelength)
        {
            //R1 ranges: 376 - 525
            //           630 - 750
            bool bResult = false;

            if ((laserWavelength > 375 && laserWavelength <= 525) ||
                (laserWavelength >= 630 && laserWavelength <= 750))
            {
                bResult = true;
            }
            return bResult;
        }

        private bool LaserModuleInR2Range(int laserWavelength)
        {
            //R2 ranges: 530 - 620
            //           765 - 890
            bool bResult = false;

            if ((laserWavelength >= 530 && laserWavelength <= 620) ||
                (laserWavelength >= 765 && laserWavelength <= 890))
            {
                bResult = true;
            }
            return bResult;
        }

        private TaskDialogResult DisplayErrorMessageEx(string strMessage1, bool bIsMultipleItems = false)
        {
            string title = "Sapphire FL Biomolecular Imager";
            string strMessage2 = "\n\nClicking on the \"Close\" button to close the application.";
            string strMessage = string.Format("{0}{1}", strMessage1, strMessage2);
            string mainInstruct = "Detected a laser module in an incompatible Port.\nPlease SHUT DOWN THE INSTRUMENT before moving the following laser module:";
            if (bIsMultipleItems)
            {
                mainInstruct = "Detected laser modules in an incompatible Ports.\nPlease SHUT DOWN THE INSTRUMENT before moving the following laser modules:";
            }

            TaskDialogOptions config = new TaskDialogOptions
            {
                Owner = this,
                Title = title,
                MainInstruction = mainInstruct,
                Content = strMessage,
                //config.ExpandedInfo = "Any expanded content text for the " +
                //                      "task dialog is shown here and the text " +
                //                      "will automatically wrap as needed.";
                //config.VerificationText = "Don't show me this message again";
                //config.FooterText = "Optional footer text with an icon can be included.";
                MainIcon = VistaTaskDialogIcon.Warning,
                CommonButtons = TaskDialogCommonButtons.Close,
                AllowDialogCancellation = true,
            };

            TaskDialogResult taskDlgResult = TaskDialog.Show(config);
            if (taskDlgResult.Result == TaskDialogSimpleResult.Close)
            {
                // Not graceful exit (more like ending the process
                //System.Environment.Exit(0);

                //If you call Application.Current.Shutdown() to close the application;
                //your function will not return immediately. You need to call return; as well for this
                System.Windows.Application.Current.Shutdown();
                return taskDlgResult;
            }
            return taskDlgResult;
        }
    }
}
