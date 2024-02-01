using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;  //XDocument
using System.Xml.XPath; //XPathDocument
using Azure.Common;
using Azure.ImagingSystem;
using Azure.Image.Processing;
using System.Windows;

namespace Azure.Configuration
{
    namespace Settings
    {
        /// <summary>
        /// Class is responsible for loading and saving persistent application settings.
        /// Call OnStartup method from App.OnStartup.
        /// Call OnExit method from App.OnExit.
        /// 
        /// Class contains Settings instance. Settings contains all persistent
        /// application settings and can be accessed from every place in the program
        /// using SettingsManager.ApplicationSettings property.
        /// 
        /// OnStartup creates Settings instance in any case, even if file with application
        /// settings cannot be loaded.
        /// 
        /// Application settings are kept in XML file inside of Application Data directory.
        /// For every program where this class is used, change applicationDirectory
        /// and optionally settingsFileName values in this class.
        /// 
        /// For example, if applicationDirectory = "MyProgram", and 
        /// settingsFileName = "Settings.xml", settings file name is:
        /// (System Drive):\Documents and Settings\(User Name)\Local Settings\Application Data\MyProgram\Settings.xml
        /// 
        /// See also: Settings class.
        /// </summary>
        static public class SettingsManager
        {
            #region Class Members

            // Persistent application settings
            static ApplicationSettings appSettings = new ApplicationSettings(); // default application Settings instance
            // in the case settings cannot be loaded from file.

            static ConfigSettings configSettings = new ConfigSettings();        // default imaging system settings

            //static MasterLibrary masterLibrary = null;                          // Darkmaster library and flat field image

            // Subdirectory in Application Data where settings file is kept.
            // Change this value for every program where SettingsManager class is used.
            //static string applicationDataPath = string.Empty;

            static string applicationDataPath = string.Empty;
            const string appSettingsFileName = "Settings.xml";      // Windows application settings
            //static string applicationPath = string.Empty;
            const string configFilename = "Config.xml";             // Imaging system configuration
            const string custSettingsFileName = "CustSettings.xml"; // Custom settings
            const string sysSettingsFileName = "SysSettings.xml";   // System specific settings
            const string protocolsFilename = "Protocols.xml";       // default scanning protocols

            #endregion Class Members

            #region Constructor

            static SettingsManager()
            {
                //EnsureDirectoryExists();
                IsEngrUI = false;
            }

            #endregion Constructor

            #region Properties

            // Differentiate between standard and Engineering UI
            public static bool IsEngrUI { get; set; }

            public static ApplicationSettings ApplicationSettings
            {
                get { return appSettings; }
            }

            public static ConfigSettings ConfigSettings
            {
                get { return configSettings; }
            }

            public static string ApplicationDataPath
            {
                get { return applicationDataPath; }
                set { applicationDataPath = value; }
            }

            //public static string ApplicationPath
            //{
            //    get { return applicationPath; }
            //    set { applicationPath = value; }
            //}

            //public static MasterLibrary MasterLibrary
            //{
            //    get { return masterLibrary; }
            //}

            #endregion Properties

            #region Startup, Exit

            /// <summary>
            /// Call this function from App.OnStartup function
            /// </summary>
            public static void OnStartup()
            {
                // Copy configuration files to common data directory (if doesn't exist)
                EnsureConfigFilesExist();

                //if (Directory.Exists(applicationDataPath))
                //{
                //    LoadSettings();
                //}
                LoadAppSettings();
                LoadConfigFiles();
                //LoadMasterLibraryInfo();
            }

            /// <summary>
            /// Call this function from App.OnExit function
            /// </summary>
            public static void OnExit()
            {
                //if (Directory.Exists(applicationPath))
                //{
                //    //SaveSettings();
                //}
                SaveAppSettings();
            }

            #endregion Overrides

            #region Other Functions

            /// <summary>
            /// Returns application settings file name
            /// </summary>
            static string AppSettingsFileName
            {
                get
                {
                    // File is kept in Application Data directory, program subdirectory.
                    // See also: EnsureDirectoryExists function.
                    return Path.Combine(applicationDataPath, appSettingsFileName);
                }
            }

            /// <summary>
            /// Load application settings from xml file
            /// </summary>
            static void LoadAppSettings()
            {
                ApplicationSettings tmp;

                try
                {
                    //Throws FileNotFoundException
                    //Could not load file or assembly 'WpfApplication.XmlSerializers
                    XmlSerializer xml = new XmlSerializer(typeof(ApplicationSettings));
                    // Avoid FileNotFoundException (the above statement throw an exception),
                    // hasitant to use because someone said it causes memory leaks.
                    //XmlSerializer xml = XmlSerializer.FromTypes(new[] { typeof(Settings) })[0];

                    using (Stream stream = new FileStream(AppSettingsFileName,
                        FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        tmp = (ApplicationSettings)xml.Deserialize(stream);
                    }
                }
                catch (Exception e)
                {
                    Trace.WriteLine(e.Message);
                    return;
                }

                // If everything is OK, replace default Settings instance
                // with instance loaded from file
                appSettings = tmp;
            }

            /// <summary>
            /// Save application settings to xml file
            /// </summary>
            static void SaveAppSettings()
            {
                try
                {
                    //Throws FileNotFoundException
                    //Could not load file or assembly 'WpfApplication.XmlSerializers
                    XmlSerializer xml = new XmlSerializer(typeof(ApplicationSettings));
                    // Avoid FileNotFoundException (the above statement throw an exception),
                    // hasitant to use because someone said it causes memory leaks.
                    //XmlSerializer xml = XmlSerializer.FromTypes(new[] { typeof(Settings) })[0];

                    using (Stream stream = new FileStream(AppSettingsFileName,
                           FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        xml.Serialize(stream, appSettings);
                    }
                }
                catch (Exception e)
                {
                    Trace.WriteLine(e.Message);
                }
            }

            // SuppressMessage doesn't work, to ask in MSDN forum
            /***[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
            static void EnsureDirectoryExists()
            {
                try
                {
                    DirectoryInfo info = new DirectoryInfo(
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        applicationDataPath));

                    if (!info.Exists)
                    {
                        info.Create();
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                }
            }***/

            /// <summary>
            /// Make sure the configuration files exist in ProgramData
            /// Copy configuration files from Program Files folder to ProgramData (if it doesn't exist)
            /// </summary>
            static void EnsureConfigFilesExist()
            {
                #region === Copy config.xml ===

                string sourceConfigFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configFilename);
                string targetConfigFilePath = System.IO.Path.Combine(ApplicationDataPath, configFilename);

                if (!System.IO.File.Exists(sourceConfigFilePath) && !System.IO.File.Exists(targetConfigFilePath))
                {
                    throw new Exception("Missing system configuration file: " + configFilename);
                }
#if DEBUG
                // config.xml in Azure.ScannerEUI project is no longer being used,
                // it's now using the same config.xml as the standard UI version.
                if (!IsEngrUI)
                {
                    //Always overwrite in DEBUG mode
                    if (System.IO.File.Exists(sourceConfigFilePath))
                    {
                        System.IO.File.Copy(sourceConfigFilePath, targetConfigFilePath, true);
                    }
                }
                else
                {
                    if (!System.IO.File.Exists(targetConfigFilePath))
                    {
                        if (System.IO.File.Exists(sourceConfigFilePath))
                        {
                            System.IO.File.Copy(sourceConfigFilePath, targetConfigFilePath);
                        }
                    }
                }
#else
                if (!System.IO.File.Exists(targetConfigFilePath))
                {
                    if (System.IO.File.Exists(sourceConfigFilePath))
                    {
                        System.IO.File.Copy(sourceConfigFilePath, targetConfigFilePath);
                    }
                }
#endif
                #endregion

                if (IsEngrUI)
                {
                    #region === Copy secure.xml ===

                    string authenConfigFile = "EUIAuthen.xml";
                    string sourceAuthenConfigFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, authenConfigFile);
                    string strTargetAuthenFilePath = System.IO.Path.Combine(ApplicationDataPath, authenConfigFile);

                    if (!File.Exists(sourceAuthenConfigFilePath) && !File.Exists(strTargetAuthenFilePath))
                    {
                        throw new Exception("Missing system configuration file: " + authenConfigFile);
                    }

                    if (!File.Exists(strTargetAuthenFilePath))
                    {
                        File.Copy(sourceAuthenConfigFilePath, strTargetAuthenFilePath);
                    }

                    #endregion
                }
                else
                {
                    #region === Copy lens.xml ===
                    /*
                    string sourceLensConfigFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, lensSettingsFilename);
                    string targetLensFilePath = System.IO.Path.Combine(ApplicationDataPath, lensSettingsFilename);

                    if (!File.Exists(sourceLensConfigFilePath) && !File.Exists(targetLensFilePath))
                    {
                        throw new Exception("Missing system configuration file: " + lensSettingsFilename);
                    }

                    if (!File.Exists(targetLensFilePath))
                    {
                        if (File.Exists(sourceLensConfigFilePath))
                        {
                            File.Copy(sourceLensConfigFilePath, targetLensFilePath);
                        }
                    }
                    */
                    #endregion

                    #region === Copy SysSettings.xml ===

                    //string strSysSettingsFile = "settings.xml";
                    string sourceSysSettingsFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, sysSettingsFileName);
                    string targetSysSettingsFilePath = System.IO.Path.Combine(ApplicationDataPath, sysSettingsFileName);

                    if (!File.Exists(sourceSysSettingsFilePath) && !File.Exists(targetSysSettingsFilePath))
                    {
                        throw new Exception("Missing system configuration file: " + sysSettingsFileName);
                    }

                    if (!File.Exists(targetSysSettingsFilePath))
                    {
                        if (File.Exists(sourceSysSettingsFilePath))
                        {
                            File.Copy(sourceSysSettingsFilePath, targetSysSettingsFilePath);
                        }
                    }

                    #endregion

                    #region === Copy secure.xml ===

                    string secureConfigFile = "secure.xml";
                    string sourceSecureConfigFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, secureConfigFile);
                    string strTargetSecureFilePath = System.IO.Path.Combine(ApplicationDataPath, secureConfigFile);

                    if (!File.Exists(sourceSecureConfigFilePath) && !File.Exists(strTargetSecureFilePath))
                    {
                        throw new Exception("Missing system configuration file: " + secureConfigFile);
                    }

                    if (!File.Exists(strTargetSecureFilePath))
                    {
                        File.Copy(sourceSecureConfigFilePath, strTargetSecureFilePath);
                    }

                    #endregion

                    #region === Copy users.xml ===
                    /*
                    string sourceUsersAcctFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, usersAccountFilename);
                    string targetUsersAcctFilePath = System.IO.Path.Combine(ApplicationDataPath, usersAccountFilename);

                    if (!File.Exists(sourceUsersAcctFilePath) && !File.Exists(targetUsersAcctFilePath))
                    {
                        throw new Exception("Missing system configuration file: " + usersAccountFilename);
                    }

                    if (!File.Exists(targetUsersAcctFilePath))
                    {
                        if (File.Exists(sourceUsersAcctFilePath))
                        {
                            File.Copy(sourceUsersAcctFilePath, targetUsersAcctFilePath);
                        }
                    }
                    */
                    #endregion

                    #region === Copy Protocols.xml ===

                    string sourceProtocolsFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, protocolsFilename);
                    string targetProtocolsFilePath = System.IO.Path.Combine(ApplicationDataPath, protocolsFilename);

                    if (!System.IO.File.Exists(sourceProtocolsFilePath) && !System.IO.File.Exists(targetProtocolsFilePath))
                    {
                        throw new Exception("Missing default protocols configuration file: " + protocolsFilename);
                    }

#if DEBUG
                    //Always overwrite in DEBUG mode
                    //if (System.IO.File.Exists(sourceProtocolsFilePath))
                    //{
                    // System.IO.File.Copy(sourceProtocolsFilePath, targetProtocolsFilePath, true);
                    //}
                    if (!System.IO.File.Exists(targetProtocolsFilePath))
                    {
                        if (System.IO.File.Exists(sourceProtocolsFilePath))
                        {
                            System.IO.File.Copy(sourceProtocolsFilePath, targetProtocolsFilePath);
                        }
                    }
#else
                    if (!System.IO.File.Exists(targetProtocolsFilePath))
                    {
                        if (System.IO.File.Exists(sourceProtocolsFilePath))
                        {
                            System.IO.File.Copy(sourceProtocolsFilePath, targetProtocolsFilePath);
                        }
                    }
#endif
                    #endregion
                }
            }

            public static void LoadConfigFiles()
            {
                LoadSystemSettings();       // system specific settings
                LoadConfigurationFile();    // default settings
                //LoadCustomSettingsFile();   // custom settings

                if (!IsEngrUI)
                {
                    LoadProtocols();   // pre-defined application protocols
                }

                // Merge the dyes list (default + custom dyes).
                //if (configSettings.DyeOptions != null && configSettings.CustDyeOptions != null)
                //{
                //    if (configSettings.CustDyeOptions.Count > 0)
                //    {
                //        foreach (var dye in configSettings.CustDyeOptions)
                //        {
                //            configSettings.DyeOptions.Add(dye);
                //        }
                //    }
                //}

                // Set dye's wavelength using a system specific wavelength settings in 'SysSettings.xml'
                // If exists, wavelength set in SysSettings.xml override those set in 'Config.xml' (still leaving it 'Config.xml'
                // for backward compatibility purposes)
                //if (configSettings.LasersWavelength != null & configSettings.LasersWavelength.Count > 0)    //EL: TODO:
                //{
                //    for (int i = 0; i < configSettings.LasersWavelength.Count; i++)
                //    {
                //        for (int j = 0; j < configSettings.DyeOptions.Count; j++)
                //        {
                //            if (configSettings.LasersWavelength[i].LaserType == configSettings.DyeOptions[j].LaserType)
                //            {
                //                configSettings.DyeOptions[j].WaveLength = configSettings.LasersWavelength[i].Wavelength;
                //            }
                //        }
                //    }
                //}
            }

            #region public static void LoadConfigurationFile()
            /// <summary>
            /// Load the config.xml file.
            /// </summary>
            /// <param name="configFile"></param>
            public static void LoadConfigurationFile()
            {
                string configFilePath = Path.Combine(applicationDataPath, configFilename);

                if (!File.Exists(configFilePath))
                {
                    throw new Exception("Configuration file does not exits: " + configFilename);
                }

                XPathDocument xpathDoc = new XPathDocument(configFilePath);
                XPathNavigator xpathNav = xpathDoc.CreateNavigator();

                XPathNodeIterator iter = null;
                //iter = xpathNav.Select("/Config/ComPort");
                //if (iter.MoveNext())
                //{
                //    configSettings.ComPort = int.Parse(iter.Current.GetAttribute("Value", ""));
                //}

                //iter = xpathNav.Select("/Config/GalilAddress");
                //if (iter.MoveNext())
                //{
                //    configSettings.GalilAddress = iter.Current.GetAttribute("Value", "");
                //}

                iter = xpathNav.Select("/Config/IsSimulationMode");
                if (iter.MoveNext())
                {
                    bool simFlag;
                    string simValue = iter.Current.GetAttribute("Value", "");
                    if (!Boolean.TryParse(simValue, out simFlag))
                    {
                        simFlag = false;
                    }
                    configSettings.IsSimulationMode = simFlag;
                }

                iter = xpathNav.Select("/Config/IsApplyImageSmoothing");
                if (iter.MoveNext())
                {
                    bool flag;
                    string strValue = iter.Current.GetAttribute("Value", "");
                    if (!Boolean.TryParse(strValue, out flag))
                    {
                        flag = false;
                    }
                    ConfigSettings.IsApplyImageSmoothing = flag;
                }

                iter = xpathNav.Select("/Config/IsChannelALightShadeFix");
                if (iter.MoveNext())
                {
                    bool lightShadeFixFlag;
                    string lightShadeFixValue = iter.Current.GetAttribute("Value", "");
                    if (!Boolean.TryParse(lightShadeFixValue, out lightShadeFixFlag))
                    {
                        lightShadeFixFlag = false;
                    }
                    configSettings.IsChannelALightShadeFix = lightShadeFixFlag;
                }

                iter = xpathNav.Select("/Config/IsChannelBLightShadeFix");
                if (iter.MoveNext())
                {
                    bool lightShadeFixFlag;
                    string lightShadeFixValue = iter.Current.GetAttribute("Value", "");
                    if (!Boolean.TryParse(lightShadeFixValue, out lightShadeFixFlag))
                    {
                        lightShadeFixFlag = false;
                    }
                    configSettings.IsChannelBLightShadeFix = lightShadeFixFlag;
                }

                iter = xpathNav.Select("/Config/IsChannelCLightShadeFix");
                if (iter.MoveNext())
                {
                    bool lightShadeFixFlag;
                    string lightShadeFixValue = iter.Current.GetAttribute("Value", "");
                    if (!Boolean.TryParse(lightShadeFixValue, out lightShadeFixFlag))
                    {
                        lightShadeFixFlag = false;
                    }
                    configSettings.IsChannelCLightShadeFix = lightShadeFixFlag;
                }

                iter = xpathNav.Select("/Config/IsChannelDLightShadeFix");
                if (iter.MoveNext())
                {
                    bool lightShadeFixFlag;
                    string lightShadeFixValue = iter.Current.GetAttribute("Value", "");
                    if (!Boolean.TryParse(lightShadeFixValue, out lightShadeFixFlag))
                    {
                        lightShadeFixFlag = false;
                    }
                    configSettings.IsChannelDLightShadeFix = lightShadeFixFlag;
                }

                // Replaced with "Fluorescence2LinesAvgScan" and "Phosphor2LinesAvgScan"
                // to be able to apply the correction only for Fluorescence or Phosphor scan or both.
                /*iter = xpathNav.Select("/Config/IsUnidirectionalScan");
                if (iter.MoveNext())
                {
                    bool unidirectionalScan;
                    string unidirectionalScanStr = iter.Current.GetAttribute("Value", "");
                    if (!Boolean.TryParse(unidirectionalScanStr, out unidirectionalScan))
                    {
                        unidirectionalScan = false;
                    }
                    configSettings.IsUnidirectionalScan = unidirectionalScan;
                }*/

                //iter = xpathNav.Select("/Config/Fluorescence2LinesAvgScan");
                //if (iter.MoveNext())
                //{
                //    bool bFl2LinesAvgScan = false;
                //    string strFl2LinesAvgScan = iter.Current.GetAttribute("Value", "");
                //    if (!Boolean.TryParse(strFl2LinesAvgScan, out bFl2LinesAvgScan))
                //    {
                //        bFl2LinesAvgScan = false;
                //    }
                //    configSettings.IsFluorescence2LinesAvgScan = bFl2LinesAvgScan;
                //}

                //iter = xpathNav.Select("/Config/Phosphor2LinesAvgScan");
                //if (iter.MoveNext())
                //{
                //    bool bPh2LinesAvgScan = false;
                //    string strPh2LinesAvgScan = iter.Current.GetAttribute("Value", "");
                //    if (!Boolean.TryParse(strPh2LinesAvgScan, out bPh2LinesAvgScan))
                //    {
                //        bPh2LinesAvgScan = false;
                //    }
                //    configSettings.IsPhosphor2LinesAvgScan = bPh2LinesAvgScan;
                //}

                iter = xpathNav.Select("/Config/EdrScaleFactor");
                if (iter.MoveNext())
                {
                    int nEdrScaleFactor = 0;
                    string strEdrScaleFactor = iter.Current.GetAttribute("Value", "");
                    if (!int.TryParse(strEdrScaleFactor, out nEdrScaleFactor))
                    {
                        nEdrScaleFactor = 1;
                    }
                    configSettings.EdrScaleFactor = nEdrScaleFactor;
                }

                iter = xpathNav.Select("/Config/SaveDebuggingImages");
                if (iter.MoveNext())
                {
                    bool bIsSaveDebuggingImages = false;
                    string strIsSaveDebuggingImages = iter.Current.GetAttribute("Value", "");
                    if (!bool.TryParse(strIsSaveDebuggingImages, out bIsSaveDebuggingImages))
                    {
                        bIsSaveDebuggingImages = false;
                    }
                    configSettings.IsSaveDebuggingImages = bIsSaveDebuggingImages;
                }
                iter = xpathNav.Select("/Config/SendAutoAlignedImageToGallery");
                if (iter.MoveNext())
                {
                    bool bIsSendAutoAlignedImageToGallery = false;
                    string strSendAutoAlignedImageToGallery = iter.Current.GetAttribute("Value", "");
                    if (!bool.TryParse(strSendAutoAlignedImageToGallery, out bIsSendAutoAlignedImageToGallery))
                    {
                        bIsSendAutoAlignedImageToGallery = false;
                    }
                    configSettings.IsSendAutoAlignedImageToGallery = bIsSendAutoAlignedImageToGallery;
                }
                iter = xpathNav.Select("/Config/SaveAutoAlignImages");
                if (iter.MoveNext())
                {
                    bool bIsSaveAutoAlignImages = false;
                    string strSaveAutoAlignImages = iter.Current.GetAttribute("Value", "");
                    if (!bool.TryParse(strSaveAutoAlignImages, out bIsSaveAutoAlignImages))
                    {
                        bIsSaveAutoAlignImages = false;
                    }
                    configSettings.IsSaveAutoAlignImages = bIsSaveAutoAlignImages;
                }
                iter = xpathNav.Select("/Config/SaveEdrAs24bit");
                if (iter.MoveNext())
                {
                    bool bIsSaveEdrAs24bit = false;
                    string strSaveEdrAs24bit = iter.Current.GetAttribute("Value", "");
                    if (!bool.TryParse(strSaveEdrAs24bit, out bIsSaveEdrAs24bit))
                    {
                        bIsSaveEdrAs24bit = false;
                    }
                    configSettings.IsSaveEdrAs24bit = bIsSaveEdrAs24bit;
                }
                iter = xpathNav.Select("/Config/KeepRawImages");
                if (iter.MoveNext())
                {
                    bool bIsKeepRawImages = false;
                    string strKeepRawImages = iter.Current.GetAttribute("Value", "");
                    if (!bool.TryParse(strKeepRawImages, out bIsKeepRawImages))
                    {
                        bIsKeepRawImages = false;
                    }
                    configSettings.IsKeepRawImages = bIsKeepRawImages;
                }

                iter = xpathNav.Select("/Config/BitmapScalingMode");
                if (iter.MoveNext())
                {
                    string strBitmapScalingMode = iter.Current.GetAttribute("Value", "");
                    if (string.IsNullOrEmpty(strBitmapScalingMode))
                    {
                        strBitmapScalingMode = "HighQuality";
                    }
                    configSettings.BitmapScalingMode = strBitmapScalingMode;
                }

                // Wi-Fi url
                iter = xpathNav.Select("/Config/ChemiModuleWiFiUrl");
                if (iter.MoveNext())
                {
                    string strChemiModuleURL = iter.Current.GetAttribute("Value", "");
                    if (string.IsNullOrEmpty(strChemiModuleURL))
                    {
                        strChemiModuleURL = "http://192.168.12.1/home";
                    }
                    configSettings.ChemiModuleWiFiUrl = strChemiModuleURL;
                }
                // LAN url
                iter = xpathNav.Select("/Config/ChemiModuleLANUrl");
                if (iter.MoveNext())
                {
                    string strChemiModuleURL = iter.Current.GetAttribute("Value", "");
                    if (string.IsNullOrEmpty(strChemiModuleURL))
                    {
                        strChemiModuleURL = "http://192.168.1.40/home";
                    }
                    configSettings.ChemiModuleLANUrl = strChemiModuleURL;
                }

                iter = xpathNav.Select("/Config/LaserTempLogging");
                if (iter.MoveNext())
                {
                    bool bIsLaserTempLogging = false;
                    string strLaserTempLogging = iter.Current.GetAttribute("Enable", "");
                    if (!bool.TryParse(strLaserTempLogging, out bIsLaserTempLogging))
                    {
                        bIsLaserTempLogging = false;
                    }
                    configSettings.IsLaserTempLogging = bIsLaserTempLogging;
                    int laserTempLoggingInterval = 30;
                    string strLaserTempLoggingInterval = iter.Current.GetAttribute("Interval", "");
                    if (!int.TryParse(strLaserTempLoggingInterval, out laserTempLoggingInterval))
                    {
                        laserTempLoggingInterval = 30;
                    }
                    configSettings.LaserTempLoggingInterval = laserTempLoggingInterval;
                }

                iter = xpathNav.Select("/Config/Resolutions/Resolution");
                while (iter.MoveNext())
                {
                    XPathNavigator nav = iter.Current;
                    ResolutionType resItem = new ResolutionType();
                    resItem.Position = int.Parse(nav.GetAttribute("Position", ""));
                    resItem.Value = int.Parse(nav.GetAttribute("Value", ""));
                    resItem.DisplayName = nav.GetAttribute("DisplayName", "");
                    configSettings.ResolutionOptions.Add(resItem);
                }

                iter = xpathNav.Select("/Config/Qualities/Quality");
                while (iter.MoveNext())
                {
                    XPathNavigator nav = iter.Current;
                    QualityType qualityItem = new QualityType();
                    qualityItem.Position = int.Parse(nav.GetAttribute("Position", ""));
                    qualityItem.Value = int.Parse(nav.GetAttribute("Value", ""));
                    qualityItem.DisplayName = nav.GetAttribute("DisplayName", "");
                    configSettings.QualityOptions.Add(qualityItem);
                }

                iter = xpathNav.Select("/Config/ScanSpeeds/ScanSpeed");
                while (iter.MoveNext())
                {
                    XPathNavigator nav = iter.Current;
                    ScanSpeedType scanSpeedItem = new ScanSpeedType();
                    scanSpeedItem.Position = int.Parse(nav.GetAttribute("Position", ""));
                    scanSpeedItem.Value = int.Parse(nav.GetAttribute("Value", ""));
                    scanSpeedItem.DisplayName = nav.GetAttribute("DisplayName", "");
                    configSettings.ScanSpeedOptions.Add(scanSpeedItem);
                }

                iter = xpathNav.Select("/Config/ScanQualities/ScanQuality");
                while (iter.MoveNext())
                {
                    XPathNavigator nav = iter.Current;
                    ScanQualityType scanQualityItem = new ScanQualityType();
                    scanQualityItem.Position = int.Parse(nav.GetAttribute("Position", ""));
                    scanQualityItem.Value = int.Parse(nav.GetAttribute("Value", ""));
                    scanQualityItem.DisplayName = nav.GetAttribute("DisplayName", "");
                    configSettings.ScanQualityOptions.Add(scanQualityItem);
                }

                iter = xpathNav.Select("/Config/MotorParameters/MotorParameter");
                string MotorParameterName = null;
                while (iter.MoveNext())
                {
                    XPathNavigator nav = iter.Current;
                    MotorParameterName = nav.GetAttribute("Name", "");
                    switch (MotorParameterName)
                    {
                        case "XMaxValue":
                            configSettings.XMaxValue = int.Parse(nav.GetAttribute("Value", ""));
                            break;
                        case "YMaxValue":
                            configSettings.YMaxValue = int.Parse(nav.GetAttribute("Value", ""));
                            break;
                        case "ZMaxValue":
                            configSettings.ZMaxValue = int.Parse(nav.GetAttribute("Value", ""));
                            break;
                        case "WMaxValue":
                            ConfigSettings.WMaxValue = int.Parse(nav.GetAttribute("Value", ""));
                            break;
                        case "WMinValue":
                            ConfigSettings.WMinValue = int.Parse(nav.GetAttribute("Value", ""));
                            break;
                        case "WMediumValue":
                            ConfigSettings.WMediumValue = int.Parse(nav.GetAttribute("Value", ""));
                            break;
                        case "XMotorSubdivision":
                            configSettings.XMotorSubdivision = double.Parse(nav.GetAttribute("Value", ""));
                            break;
                        case "YMotorSubdivision":
                            configSettings.YMotorSubdivision = double.Parse(nav.GetAttribute("Value", ""));
                            break;
                        case "ZMotorSubdivision":
                            configSettings.ZMotorSubdivision = double.Parse(nav.GetAttribute("Value", ""));
                            break;
                        case "WMotorSubdivision":
                            ConfigSettings.WMotorSubdivision = double.Parse(nav.GetAttribute("Value", ""));
                            break;
                        case "XEncoderSubdivision":
                            ConfigSettings.XEncoderSubdivision = double.Parse(nav.GetAttribute("Value", ""));
                            break;
                    }
                }

                iter = xpathNav.Select("/Config/MotorPolarities/Polarities");
                string palarityName;
                while (iter.MoveNext())
                {
                    XPathNavigator nav = iter.Current;
                    palarityName = nav.GetAttribute("Name", "");
                    switch (palarityName)
                    {
                        case "DirX":
                            configSettings.MotionPolarityDirX = bool.Parse(nav.GetAttribute("Value", ""));
                            break;
                        case "EnableX":
                            configSettings.MotionPolarityEnableX = bool.Parse(nav.GetAttribute("Value", ""));
                            break;
                        case "ClockX":
                            configSettings.MotionPolarityClkX = bool.Parse(nav.GetAttribute("Value", ""));
                            break;
                        case "HomeX":
                            configSettings.MotionPolarityHomeX = bool.Parse(nav.GetAttribute("Value", ""));
                            break;
                        case "FwdLimitX":
                            configSettings.MotionPolarityFwdX = bool.Parse(nav.GetAttribute("Value", ""));
                            break;
                        case "BwdLimitX":
                            configSettings.MotionPolarityBwdX = bool.Parse(nav.GetAttribute("Value", ""));
                            break;
                        case "DirY":
                            configSettings.MotionPolarityDirY = bool.Parse(nav.GetAttribute("Value", ""));
                            break;
                        case "EnableY":
                            configSettings.MotionPolarityEnableY = bool.Parse(nav.GetAttribute("Value", ""));
                            break;
                        case "ClockY":
                            configSettings.MotionPolarityClkY = bool.Parse(nav.GetAttribute("Value", ""));
                            break;
                        case "HomeY":
                            configSettings.MotionPolarityHomeY = bool.Parse(nav.GetAttribute("Value", ""));
                            break;
                        case "FwdLimitY":
                            configSettings.MotionPolarityFwdY = bool.Parse(nav.GetAttribute("Value", ""));
                            break;
                        case "BwdLimitY":
                            configSettings.MotionPolarityBwdY = bool.Parse(nav.GetAttribute("Value", ""));
                            break;
                        case "DirZ":
                            configSettings.MotionPolarityDirZ = bool.Parse(nav.GetAttribute("Value", ""));
                            break;
                        case "EnableZ":
                            configSettings.MotionPolarityEnableZ = bool.Parse(nav.GetAttribute("Value", ""));
                            break;
                        case "ClockZ":
                            configSettings.MotionPolarityClkZ = bool.Parse(nav.GetAttribute("Value", ""));
                            break;
                        case "HomeZ":
                            configSettings.MotionPolarityHomeZ = bool.Parse(nav.GetAttribute("Value", ""));
                            break;
                        case "FwdLimitZ":
                            configSettings.MotionPolarityFwdZ = bool.Parse(nav.GetAttribute("Value", ""));
                            break;
                        case "BwdLimitZ":
                            configSettings.MotionPolarityBwdZ = bool.Parse(nav.GetAttribute("Value", ""));
                            break;
                        case "DirW":
                            configSettings.MotionPolarityDirW = bool.Parse(nav.GetAttribute("Value", ""));
                            break;
                        case "EnableW":
                            configSettings.MotionPolarityEnableW = bool.Parse(nav.GetAttribute("Value", ""));
                            break;
                        case "ClockW":
                            configSettings.MotionPolarityClkW = bool.Parse(nav.GetAttribute("Value", ""));
                            break;
                        case "HomeW":
                            configSettings.MotionPolarityHomeW = bool.Parse(nav.GetAttribute("Value", ""));
                            break;
                        case "FwdLimitW":
                            configSettings.MotionPolarityFwdW = bool.Parse(nav.GetAttribute("Value", ""));
                            break;
                        case "BwdLimitW":
                            configSettings.MotionPolarityBwdW = bool.Parse(nav.GetAttribute("Value", ""));
                            break;
                    }
                }

                iter = xpathNav.Select("/Config/APDGains/APDGain");
                while (iter.MoveNext())
                {
                    XPathNavigator nav = iter.Current;
                    APDGainType aPDGainItem = new APDGainType();
                    aPDGainItem.Position = int.Parse(nav.GetAttribute("Position", ""));
                    aPDGainItem.Value = int.Parse(nav.GetAttribute("Value", ""));
                    aPDGainItem.DisplayName = nav.GetAttribute("DisplayName", "");
                    configSettings.APDGains.Add(aPDGainItem);
                }

                iter = xpathNav.Select("/Config/APDPgas/APDPga");
                while (iter.MoveNext())
                {
                    XPathNavigator nav = iter.Current;
                    APDPgaType APDPgaItem = new APDPgaType();
                    APDPgaItem.Position = int.Parse(nav.GetAttribute("Position", ""));
                    APDPgaItem.Value = int.Parse(nav.GetAttribute("Value", ""));
                    APDPgaItem.DisplayName = nav.GetAttribute("DisplayName", "");
                    configSettings.APDPgas.Add(APDPgaItem);
                }
                iter = xpathNav.Select("/Config/MotorSettings/MotorSetting");
                while (iter.MoveNext())
                {
                    XPathNavigator nav = iter.Current;
                    MotorSettingsType motorSetting = new MotorSettingsType();
                    MotorType motorType;
                    Enum.TryParse(nav.GetAttribute("Motor", ""), out motorType); //string to enum
                    double position = 0;
                    double.TryParse(nav.GetAttribute("Position", ""), out position);
                    double speed = 0;
                    double.TryParse(nav.GetAttribute("Speed", ""), out speed);
                    double acc = 0;
                    double.TryParse(nav.GetAttribute("Accel", ""), out acc);
                    double dcc = 0;
                    double.TryParse(nav.GetAttribute("Dccel", ""), out dcc);
                    motorSetting.MotorType = motorType;
                    motorSetting.Position = position;
                    motorSetting.Speed = speed;
                    motorSetting.Accel = acc;
                    motorSetting.Dccel = dcc;
                    configSettings.MotorSettings.Add(motorSetting);
                }
                iter = xpathNav.Select("/Config/MotorSettings/ScanningSetting");
                while (iter.MoveNext())
                {
                    var nav = iter.Current;
                    int delay = 0;
                    string name = nav.GetAttribute("Name", "");
                    if (name == "XMotorTurnDelay")
                    {
                        int.TryParse(nav.GetAttribute("Value", ""), out delay);
                        configSettings.XMotionTurnDelay = delay;
                    }
                    else if (name == "XMotionExtraMoveLength")
                    {
                        configSettings.XMotionExtraMoveLength = double.Parse(nav.GetAttribute("Value", ""));
                    }
                    else if (name == "YMotionExtraMoveLength")
                    {
                        configSettings.YMotionExtraMoveLength = double.Parse(nav.GetAttribute("Value", ""));
                    }
                }
                iter = xpathNav.Select("/Config/MotorSettings/LaunchSetting");
                while (iter.MoveNext())
                {
                    var nav = iter.Current;
                    bool homeMotion;
                    string name = nav.GetAttribute("Name", "");
                    if (name == "HomeMotionsAtLaunchTime")
                    {
                        bool.TryParse(nav.GetAttribute("Value", ""), out homeMotion);
                        configSettings.HomeMotionsAtLaunchTime = homeMotion;
                    }
                }

                iter = xpathNav.Select("/Config/LaserPowers/LaserPower");
                while (iter.MoveNext())
                {
                    XPathNavigator nav = iter.Current;
                    string strLaserType = nav.GetAttribute("Laser", "");
                    int laserIntensity = int.Parse(nav.GetAttribute("Intensity", ""));
                    int laserMaxIntensity = int.Parse(nav.GetAttribute("MaxIntensity", ""));
                    switch (strLaserType.ToUpper())
                    {
                        case "A":
                            configSettings.LaserAIntensity = laserIntensity;
                            configSettings.LaserAMaxIntensity = laserMaxIntensity;
                            break;
                        case "B":
                            configSettings.LaserBIntensity = laserIntensity;
                            configSettings.LaserBMaxIntensity = laserMaxIntensity;
                            break;
                        case "C":
                            configSettings.LaserCIntensity = laserIntensity;
                            configSettings.LaserCMaxIntensity = laserMaxIntensity;
                            break;
                        case "D":
                            configSettings.LaserDIntensity = laserIntensity;
                            configSettings.LaserDMaxIntensity = laserMaxIntensity;
                            break;
                    }
                }

                iter = xpathNav.Select("/Config/ScanSettings/ScannSetting");
                while (iter.MoveNext())
                {
                    var nav = iter.Current;
                    bool dynamicBitAt;
                    int delay = 0;
                    string name = nav.GetAttribute("Name", "");
                    if (name == "ScanDynamicBitAt")
                    {
                        bool.TryParse(nav.GetAttribute("Value", ""), out dynamicBitAt);
                        configSettings.ScanDynamicBitAt = dynamicBitAt;
                    }
                    bool ImageOffsetProcessing;
                    if (name == "ImageOffsetProcessing")
                    {
                        bool.TryParse(nav.GetAttribute("Value", ""), out ImageOffsetProcessing);
                        configSettings.ImageOffsetProcessing = ImageOffsetProcessing;
                    }
                    bool PixelOffsetProcessing;
                    if (name == "PixelOffsetProcessing")
                    {
                        bool.TryParse(nav.GetAttribute("Value", ""), out PixelOffsetProcessing);
                        configSettings.PixelOffsetProcessing = PixelOffsetProcessing;
                    }
                    int pixelOffsetProcessingRes;
                    if (name == "PixelOffsetProcessingRes")
                    {
                        int.TryParse(nav.GetAttribute("Value", ""), out pixelOffsetProcessingRes);
                        configSettings.PixelOffsetProcessingRes = pixelOffsetProcessingRes;
                    }
                    bool ycompensationBitAt;
                    if (name == "YCompenSationBitAt")
                    {
                        bool.TryParse(nav.GetAttribute("Value", ""), out ycompensationBitAt);
                        configSettings.YCompenSationBitAt = ycompensationBitAt;
                    }
                    if (name == "YCompenOffset")
                    {
                        configSettings.YCompenOffset = Convert.ToDouble(nav.GetAttribute("Value", ""));
                    }
                    if (name == "XOddNumberedLine")
                    {
                        int.TryParse(nav.GetAttribute("Value", ""), out delay);
                        configSettings.XOddNumberedLine = delay;
                    }
                    if (name == "XEvenNumberedLine")
                    {
                        int.TryParse(nav.GetAttribute("Value", ""), out delay);
                        configSettings.XEvenNumberedLine = delay;
                    }
                    if (name == "YOddNumberedLine")
                    {
                        int.TryParse(nav.GetAttribute("Value", ""), out delay);
                        configSettings.YOddNumberedLine = delay;
                    }
                    if (name == "YEvenNumberedLine")
                    {
                        int.TryParse(nav.GetAttribute("Value", ""), out delay);
                        configSettings.YEvenNumberedLine = delay;
                    }
                    bool bIsPhosphorModuleProcessing;
                    if (name == "PhosphorModuleProcessing")
                    {
                        bool.TryParse(nav.GetAttribute("Value", ""), out bIsPhosphorModuleProcessing);
                        configSettings.PhosphorModuleProcessing = bIsPhosphorModuleProcessing;
                    }
                    bool AllModuleProcessing;
                    if (name == "AllModuleProcessing")
                    {
                        bool.TryParse(nav.GetAttribute("Value", ""), out AllModuleProcessing);
                        configSettings.AllModuleProcessing = AllModuleProcessing;
                    }
                    bool ENGGUI_PhosphorModuleProcessing;
                    if (name == "ENGGUI_PhosphorModuleProcessing")
                    {
                        bool.TryParse(nav.GetAttribute("Value", ""), out ENGGUI_PhosphorModuleProcessing);
                        configSettings.ENGGUI_PhosphorModuleProcessing = ENGGUI_PhosphorModuleProcessing;
                    }
                }
                iter = xpathNav.Select("/Config/RadiatorTemperatures/RadiatorTemperature");
                while (iter.MoveNext())
                {
                    var nav = iter.Current;
                    int delay = 0;
                    string name = nav.GetAttribute("Name", "");
                    if (name == "RadiatorL")
                    {
                        int.TryParse(nav.GetAttribute("Value", ""), out delay);
                        configSettings.RadiatorTemperatureL = delay;
                    }
                    if (name == "RadiatorR1")
                    {
                        int.TryParse(nav.GetAttribute("Value", ""), out delay);
                        configSettings.RadiatorTemperatureR1 = delay;
                    }
                    if (name == "RadiatorR2")
                    {
                        int.TryParse(nav.GetAttribute("Value", ""), out delay);
                        configSettings.RadiatorTemperatureR2 = delay;
                    }
                }
                iter = xpathNav.Select("/Config/PhosphorLaserModules/LaserModule");
                while (iter.MoveNext())
                {
                    XPathNavigator nav = iter.Current;
                    PhosphorLaserModules PhosphorItem = new PhosphorLaserModules();
                    PhosphorItem.DisplayName = nav.GetAttribute("LaserNumber", "");
                    configSettings.PhosphorLaserModules.Add(PhosphorItem);
                }

                iter = xpathNav.Select("/Config/LasersIntensities/LaserIntensity");
                while (iter.MoveNext())
                {
                    XPathNavigator nav = iter.Current;
                    LaserSettingsType laserSetting = new LaserSettingsType();
                    string strLaserType = nav.GetAttribute("Laser", "");
                    LaserType laserType;
                    Enum.TryParse(strLaserType, out laserType); //string to enum

                    laserSetting.LaserType = laserType;

                    string strLaserInt = nav.GetAttribute("Intensities", "");
                    string[] arrLasersInt = strLaserInt.Split(' ');
                    int laserInt = 0;
                    for (int i = 0; i < arrLasersInt.Length; i++)
                    {
                        int.TryParse(arrLasersInt[i], out laserInt);
                        laserSetting.Intensities.Add(laserInt);
                    }

                    laserSetting.MaxIntensity = int.Parse(nav.GetAttribute("MaxIntensity", ""));

                    configSettings.LasersIntensitySettings.Add(laserSetting);
                }

                //iter = xpathNav.Select("/Config/Dyes/Dye");
                //while (iter.MoveNext())
                //{
                //    XPathNavigator nav = iter.Current;
                //    string strPosition = nav.GetAttribute("Position", "");
                //    int position = 0;
                //    int.TryParse(strPosition, out position);
                //
                //    string displayName = nav.GetAttribute("DisplayName", "");
                //
                //    string strLaserType = nav.GetAttribute("Laser", "");
                //    LaserType laserType;
                //    Enum.TryParse(strLaserType, out laserType); //string to enum
                //
                //    string strWaveLength = nav.GetAttribute("WaveLength", "");
                //    //int waveLength = 0;
                //    //int.TryParse(strWaveLength, out waveLength);
                //
                //    DyeType dyeType = new DyeType();
                //    dyeType.Position = position;
                //    dyeType.DisplayName = displayName;
                //    dyeType.LaserType = laserType;
                //    //dyeType.WaveLength = waveLength;
                //    dyeType.WaveLength = strWaveLength;
                //    configSettings.DyeOptions.Add(dyeType);
                //}
                //// Sort the list and put the Azure dyes on top of the list
                //// Not manually sorting the list in 'config.xml' to be backward compatible with previously saved protocols
                ////
                //configSettings.DyeOptions = new List<DyeType>(configSettings.DyeOptions.OrderBy(DyeType => DyeType.DisplayName));
                //// Retrieve all Azure dyes
                //var allAzureDyes = configSettings.DyeOptions.Where(DyeType => DyeType.DisplayName.Contains("Azure")).ToList();
                //// Remove all Azure dyes
                //configSettings.DyeOptions.RemoveAll(IsAzureDye);
                //configSettings.DyeOptions = allAzureDyes.Concat<DyeType>(configSettings.DyeOptions).ToList();

                /*
                iter = xpathNav.Select("/Config/Protocols/Protocol");
                while (iter.MoveNext())
                {
                    XPathNavigator nav = iter.Current;

                    string displayName = nav.GetAttribute("DisplayName", "");

                    string strSampleType = nav.GetAttribute("SampleType", "");
                    int sampleType;
                    int.TryParse(strSampleType, out sampleType);

                    string strPixelSize = nav.GetAttribute("PixelSize", "");
                    int pixelSize = 0;
                    int.TryParse(strPixelSize, out pixelSize);

                    string strScanspeed = nav.GetAttribute("ScanSpeed", "");
                    int scanSpeed = 0;
                    int.TryParse(strScanspeed, out scanSpeed);

                    AppProtocol appProtocol = new AppProtocol();
                    appProtocol.Name = displayName;
                    appProtocol.Sampletype = sampleType;
                    appProtocol.Pixelsize = pixelSize;
                    appProtocol.Scanspeed = scanSpeed;

                    if (nav.HasChildren)
                    {
                        nav.MoveToFirstChild();
                        do
                        {
                            string strDyeType = nav.GetAttribute("DyeType", "");
                            int dyeType = 0;
                            int.TryParse(strDyeType, out dyeType);
                            string strSignalInt = nav.GetAttribute("SignalIntensity", "");
                            int signalInt = 0;
                            int.TryParse(strSignalInt, out signalInt);
                            string strColorChan = nav.GetAttribute("ColorChannel", "");
                            ImageChannelType colorChannel;
                            Enum.TryParse(strColorChan, out colorChannel); //string to enum

                            AppDyeData dyeData = new AppDyeData();
                            dyeData.DyeType = dyeType;
                            dyeData.SignalIntensity = signalInt;
                            dyeData.ColorChannel = colorChannel;

                            appProtocol.Dyes.Add(dyeData);

                        } while (nav.MoveToNext());
                    }

                    configSettings.AppProtocols.Add(appProtocol);
                }*/

                // 20180221: Moved from SysSettings.xml
                // Read signal options
                iter = xpathNav.Select("/Config/Signals/Signal");
                while (iter.MoveNext())
                {
                    XPathNavigator nav = iter.Current;

                    //string strLaserType = nav.GetAttribute("LaserType", "");
                    //strLaserType = "Laser" + strLaserType;
                    //LaserType laserType = (LaserType)Enum.Parse(typeof(LaserType), strLaserType);

                    string strLaserType = nav.GetAttribute("LaserType", "");

                    int nLaserType = int.Parse(strLaserType);
                    var signalList = new List<Signal>();
                    configSettings.LasersSignalList.Add(nLaserType, signalList);

                    if (nav.HasChildren)
                    {
                        if (nav.MoveToFirstChild())
                        {
                            do
                            {
                                Signal signal = new Signal();
                                signal.Position = int.Parse(nav.GetAttribute("Position", ""));
                                signal.DisplayName = nav.GetAttribute("DisplayName", "");
                                signal.LaserIntensity = int.Parse(nav.GetAttribute("LaserIntensity", ""));
                                //LaserIntensity will be overrided with the laser intensity read from the scanner.
                                //Laser intensity in mW is needed for SmartScan calculation so we're now saving the laser power in mW (previously not needed).
                                signal.LaserIntInmW = signal.LaserIntensity;
                                signal.ApdGain = int.Parse(nav.GetAttribute("ApdGain", ""));
                                signal.ApdPga = int.Parse(nav.GetAttribute("ApdPga", ""));
                                //signal.LaserType = laserType; //EL: TODO:
                                //signal.LaserWavelength = GetLaserWaveLength(laserType); //EL: TODO:

                                //switch (laserType)
                                //{
                                //    case LaserType.LaserA:
                                //        configSettings.LaserASignalOptions.Add(signal);
                                //        break;
                                //    case LaserType.LaserB:
                                //        configSettings.LaserBSignalOptions.Add(signal);
                                //        break;
                                //    case LaserType.LaserC:
                                //        configSettings.LaserCSignalOptions.Add(signal);
                                //        break;
                                //    case LaserType.LaserD:
                                //        configSettings.LaserDSignalOptions.Add(signal);
                                //        break;
                                //}

                                if (configSettings.LasersSignalList.ContainsKey(nLaserType))
                                {
                                    configSettings.LasersSignalList[nLaserType].Add(signal);
                                }
                            }
                            while (nav.MoveToNext());
                            nav.MoveToParent();
                        }
                    }
                }

                // 20180221: Moved from SysSettings.xml
                // Read signal options
                iter = xpathNav.Select("/Config/PhosphorSignals/PhosphorSignal");
                while (iter.MoveNext())
                {
                    XPathNavigator nav = iter.Current;

                    string strLaserType = nav.GetAttribute("LaserType", "");
                    //strLaserType = "Laser" + strLaserType;
                    //LaserType laserType = (LaserType)Enum.Parse(typeof(LaserType), strLaserType);
                    int waveLength = int.Parse(strLaserType);

                    if (nav.HasChildren)
                    {
                        if (nav.MoveToFirstChild())
                        {
                            do
                            {
                                Signal signal = new Signal();
                                signal.Position = int.Parse(nav.GetAttribute("Position", ""));
                                signal.LaserIntensity = int.Parse(nav.GetAttribute("LaserIntensity", ""));
                                signal.ApdGain = int.Parse(nav.GetAttribute("ApdGain", ""));
                                signal.ApdPga = int.Parse(nav.GetAttribute("ApdPga", ""));
                                signal.LaserWavelength = waveLength;
                                configSettings.PhosphorSignalOptions.Add(signal);

                                //switch (laserType)
                                //{
                                //    //case LaserType.LaserA:
                                //    //    configSettings.LaserASignalOptions.Add(signal);
                                //    //    break;
                                //    //case LaserType.LaserB:
                                //    //    configSettings.LaserBSignalOptions.Add(signal);
                                //    //    break;
                                //    case LaserType.LaserC:
                                //        configSettings.PhosphorCSignalOptions.Add(signal);
                                //        break;
                                //    case LaserType.LaserD:
                                //        configSettings.PhosphorDSignalOptions.Add(signal);
                                //        break;
                                //}
                            }
                            while (nav.MoveToNext());
                            nav.MoveToParent();
                        }
                    }
                }

                #region === Auto scan Settings ===

                iter = xpathNav.Select("/Config/AutoScan/Resolution");
                if (iter.MoveNext())
                {
                    ConfigSettings.AutoScanSettings.Resolution = int.Parse(iter.Current.GetAttribute("Value", ""));
                }
                iter = xpathNav.Select("/Config/AutoScan/HighResolution");
                if (iter.MoveNext())
                {
                    ConfigSettings.AutoScanSettings.HighResolution = int.Parse(iter.Current.GetAttribute("Value", ""));
                }
                iter = xpathNav.Select("/Config/AutoScan/OptimalVal");
                if (iter.MoveNext())
                {
                    ConfigSettings.AutoScanSettings.OptimalVal = int.Parse(iter.Current.GetAttribute("Value", ""));
                }
                iter = xpathNav.Select("/Config/AutoScan/OptimalDelta");
                if (iter.MoveNext())
                {
                    ConfigSettings.AutoScanSettings.OptimalDelta = double.Parse(iter.Current.GetAttribute("Value", ""));
                }
                iter = xpathNav.Select("/Config/AutoScan/Ceiling");
                if (iter.MoveNext())
                {
                    ConfigSettings.AutoScanSettings.Ceiling = int.Parse(iter.Current.GetAttribute("Value", ""));
                }
                iter = xpathNav.Select("/Config/AutoScan/Floor");
                if (iter.MoveNext())
                {
                    ConfigSettings.AutoScanSettings.Floor = int.Parse(iter.Current.GetAttribute("Value", ""));
                }
                iter = xpathNav.Select("/Config/AutoScan/Alpha488");
                if (iter.MoveNext())
                {
                    ConfigSettings.AutoScanSettings.Alpha488 = double.Parse(iter.Current.GetAttribute("Value", ""));
                }
                iter = xpathNav.Select("/Config/AutoScan/StartingSignalLevel");
                if (iter.MoveNext())
                {
                    ConfigSettings.AutoScanSettings.StartingSignalLevel = int.Parse(iter.Current.GetAttribute("Position", ""));
                }
                iter = xpathNav.Select("/Config/AutoScan/HighSignalStepdownLevel");
                if (iter.MoveNext())
                {
                    ConfigSettings.AutoScanSettings.HighSignalStepdownLevel = int.Parse(iter.Current.GetAttribute("Position", ""));
                }
                iter = xpathNav.Select("/Config/AutoAlign");
                while (iter.MoveNext())
                {
                    XPathNavigator nav = iter.Current;
                    string strX = nav.GetAttribute("X", "");
                    string strY = nav.GetAttribute("Y", "");
                    string strW = nav.GetAttribute("Width", "");
                    string strH = nav.GetAttribute("Height", "");
                    if (!double.TryParse(strX, out double x)) { x = 3.5; }
                    if (!double.TryParse(strY, out double y)) { y = 0.5; }
                    if (!double.TryParse(strW, out double w)) { w = 4.0; }
                    if (!double.TryParse(strH, out double h)) { h = 1.5; }
                    configSettings.AutoAlignScanRegion = new Rect(x, y, w, h);
                }

                /*iter = xpathNav.Select("/Config/AutoScan/LaserASignalLevel");
                if (iter.MoveNext())
                {
                    int signalLevelPos = int.Parse(iter.Current.GetAttribute("Position", ""));
                    if (configSettings.LaserASignalOptions != null && signalLevelPos < configSettings.LaserASignalOptions.Count - 1)
                    {
                        ConfigSettings.AutoScanSettings.LaserASignalLevel = configSettings.LaserASignalOptions[signalLevelPos - 1];
                    }
                }
                iter = xpathNav.Select("/Config/AutoScan/LaserBSignalLevel");
                if (iter.MoveNext())
                {
                    int signalLevelPos = int.Parse(iter.Current.GetAttribute("Position", ""));
                    if (configSettings.LaserBSignalOptions != null && signalLevelPos < configSettings.LaserBSignalOptions.Count - 1)
                    {
                        ConfigSettings.AutoScanSettings.LaserBSignalLevel = configSettings.LaserBSignalOptions[signalLevelPos - 1];
                    }
                }
                iter = xpathNav.Select("/Config/AutoScan/LaserCSignalLevel");
                if (iter.MoveNext())
                {
                    int signalLevelPos = int.Parse(iter.Current.GetAttribute("Position", ""));
                    if (configSettings.LaserCSignalOptions != null && signalLevelPos < configSettings.LaserCSignalOptions.Count - 1)
                    {
                        ConfigSettings.AutoScanSettings.LaserCSignalLevel = configSettings.LaserCSignalOptions[signalLevelPos - 1];
                    }
                }
                iter = xpathNav.Select("/Config/AutoScan/LaserDSignalLevel");
                if (iter.MoveNext())
                {
                    int signalLevelPos = int.Parse(iter.Current.GetAttribute("Position", ""));
                    if (configSettings.LaserDSignalOptions != null && signalLevelPos < configSettings.LaserDSignalOptions.Count - 1)
                    {
                        ConfigSettings.AutoScanSettings.LaserDSignalLevel = configSettings.LaserDSignalOptions[signalLevelPos - 1];
                    }
                }*/

                #endregion

                //Read Control conditions for the fan speed 
                iter = xpathNav.Select("/Config/ShellFanModules/ShellFanModule");
                while (iter.MoveNext())
                {
                    var nav = iter.Current;
                    double delay = 0;
                    string name = nav.GetAttribute("ShellFan", "");
                    if (name == "InternalLowTemperature")
                    {
                        double.TryParse(nav.GetAttribute("Value", ""), out delay);
                        configSettings.InternalLowTemperature = delay;
                    }
                    if (name == "InternalModerateTemperature")
                    {
                        double.TryParse(nav.GetAttribute("Value", ""), out delay);
                        configSettings.InternalModerateTemperature = delay;
                    }
                    if (name == "InternalHighTemperature")
                    {
                        double.TryParse(nav.GetAttribute("Value", ""), out delay);
                        configSettings.InternalHighTemperature = delay;
                    }
                    if (name == "ModuleLowTemperature")
                    {
                        double.TryParse(nav.GetAttribute("Value", ""), out delay);
                        configSettings.ModuleLowTemperature = delay;
                    }
                    if (name == "ModuleModerateTemperature")
                    {
                        double.TryParse(nav.GetAttribute("Value", ""), out delay);
                        configSettings.ModuleModerateTemperature = delay;
                    }
                    if (name == "ModuleHighTemperature")
                    {
                        double.TryParse(nav.GetAttribute("Value", ""), out delay);
                        configSettings.ModuleHighTemperature = delay;
                    }
                }
                #region TestJjg
                iter = xpathNav.Select("/Config/LightGains/LightGain");
                while (iter.MoveNext())
                {
                    XPathNavigator nav = iter.Current;
                    LightGain lightGainItem = new LightGain();
                    lightGainItem.Position = int.Parse(nav.GetAttribute("Position", ""));
                    lightGainItem.Value = int.Parse(nav.GetAttribute("Value", ""));
                    lightGainItem.DisplayName = nav.GetAttribute("DisplayName", "");
                    configSettings.LightGains.Add(lightGainItem);
                }
                iter = xpathNav.Select("/Config/TestJjgScanSettings/ScanSetting");
                while (iter.MoveNext())
                {
                    XPathNavigator nav = iter.Current;
                    string settingName = nav.GetAttribute("Name", "");
                    string settingValue = nav.GetAttribute("Value", "");
                    switch (settingName)
                    {
                        case "ScanPreciseAt":
                            configSettings.ScanPreciseAt = bool.Parse(settingValue);
                            break;
                        case "ScanPreciseParameter":
                            configSettings.ScanPreciseParameter = int.Parse(settingValue);
                            break;
                    }
                }
                iter = xpathNav.Select("/Config/FocusAdjustSettings/Setting");
                while (iter.MoveNext())
                {
                    XPathNavigator nav = iter.Current;
                    string settingName = nav.GetAttribute("Name", "");
                    string settingValue = nav.GetAttribute("Value", "");
                    switch (settingName)
                    {
                        case "ZScanValueThreshold":
                            configSettings.ZScanValueThreshold = int.Parse(settingValue);
                            break;
                        case "CentralCoordX":
                            configSettings.CentralCoordX = int.Parse(settingValue);
                            break;
                        case "CentralCoordY":
                            configSettings.CentralCoordY = int.Parse(settingValue);
                            break;
                    }
                }
                iter = xpathNav.Select("/Config/GlassLevelingSettings/Setting");
                System.Drawing.Point[] point = new System.Drawing.Point[4];
                while (iter.MoveNext())
                {
                    XPathNavigator nav = iter.Current;
                    string settingName = nav.GetAttribute("Name", "");
                    string settingValue = nav.GetAttribute("Value", "");
                    switch (settingName)
                    {
                        case "TopLeftX":
                            point[0].X = int.Parse(settingValue);
                            break;
                        case "TopLeftY":
                            point[0].Y = int.Parse(settingValue);
                            break;
                        case "TopRightX":
                            point[1].X = int.Parse(settingValue);
                            break;
                        case "TopRightY":
                            point[1].Y = int.Parse(settingValue);
                            break;
                        case "LowerRightX":
                            point[2].X = int.Parse(settingValue);
                            break;
                        case "LowerRightY":
                            point[2].Y = int.Parse(settingValue);
                            break;
                        case "LowerLeftX":
                            point[3].X = int.Parse(settingValue);
                            break;
                        case "LowerLeftY":
                            point[3].Y = int.Parse(settingValue);
                            break;
                        case "ModuleInterval":
                            configSettings.ModuleInterval = int.Parse(settingValue);
                            break;
                    }
                }
                configSettings.GlassLevelingTopLeft = point[0];
                configSettings.GlassLevelingTopRight = point[1];
                configSettings.GlassLevelingLowerRight = point[2];
                configSettings.GlassLevelingLowerLeft = point[3];
                iter = xpathNav.Select("/Config/APDCalibrationSettings/Setting");
                while (iter.MoveNext())
                {
                    XPathNavigator nav = iter.Current;
                    string settingName = nav.GetAttribute("Name", "");
                    string settingValue = nav.GetAttribute("Value", "");
                    switch (settingName)
                    {
                        case "ApdOutputAtG0":
                            configSettings.APDOutputAtG0 = double.Parse(settingValue);
                            break;
                        case "ApdOutputErrorAtG0":
                            configSettings.APDOutputErrorAtG0 = double.Parse(settingValue);
                            break;
                        case "DarkCurrentLimitH":
                            configSettings.APDDarkCurrentLimitH = double.Parse(settingValue);
                            break;
                        case "DarkCurrentLimitL":
                            configSettings.APDDarkCurrentLimitL = double.Parse(settingValue);
                            break;
                        case "ApdOutputStableLongTime":
                            configSettings.APDOutputStableLongTime = int.Parse(settingValue);
                            break;
                        case "ApdOutputStableShortTime":
                            configSettings.APDOutputStableShortTime = int.Parse(settingValue);
                            break;
                        case "ApdPGA":
                            configSettings.APDPGA = int.Parse(settingValue);
                            break;
                    }
                }
                iter = xpathNav.Select("/Config/Powers/Power");
                while (iter.MoveNext())
                {
                    XPathNavigator nav = iter.Current;
                    LaserPower laserPowerItem = new LaserPower();
                    laserPowerItem.Position = int.Parse(nav.GetAttribute("Position", ""));
                    laserPowerItem.Value = int.Parse(nav.GetAttribute("Value", ""));
                    laserPowerItem.DisplayName = nav.GetAttribute("DisplayName", "");
                    configSettings.LaserPowers.Add(laserPowerItem);
                }
                #endregion
                xpathNav = null;
                xpathDoc = null;
            }
            #endregion

            /// <summary>
            /// Load system specific settings file
            /// </summary>
            public static void LoadSystemSettings()
            {
                // 2017/07/07: Currently Engineering UI does not use SysSettings.xml
                if (IsEngrUI) { return; }

                string sysSettingsFilePath = Path.Combine(applicationDataPath, sysSettingsFileName);

                if (!File.Exists(sysSettingsFilePath))
                {
                    throw new ArgumentException("Configuration file does not exits: " + sysSettingsFileName);
                }

                try
                {
                    XPathDocument xpathDoc = new XPathDocument(sysSettingsFilePath);
                    XPathNavigator xpathNav = xpathDoc.CreateNavigator();
                    XPathNodeIterator iter;

                    iter = xpathNav.Select("/SysSettings/ChemiModule");
                    if (iter.MoveNext())
                    {
                        bool bIsChemiModule = false;
                        string strChemiModule = iter.Current.GetAttribute("Value", "");
                        if (!string.IsNullOrEmpty(strChemiModule))
                        {
                            if (!Boolean.TryParse(strChemiModule, out bIsChemiModule))
                            {
                                bIsChemiModule = false;
                            }
                        }
                        configSettings.IsChemiModule = bIsChemiModule;
                    }
                    iter = xpathNav.Select("/SysSettings/IgnoreCompCoefficient");
                    if (iter.MoveNext())
                    {
                        bool bIsIgnoreCompCoefficient = false;
                        string strIgnoreCompCoefficient = iter.Current.GetAttribute("Value", "");
                        if (!string.IsNullOrEmpty(strIgnoreCompCoefficient))
                        {
                            if (!Boolean.TryParse(strIgnoreCompCoefficient, out bIsIgnoreCompCoefficient))
                            {
                                bIsIgnoreCompCoefficient = false;
                            }
                        }
                        configSettings.IsIgnoreCompCoefficient = bIsIgnoreCompCoefficient;
                    }

                    iter = xpathNav.Select("/SysSettings/ImagingTabs/ImagingTab");
                    while (iter.MoveNext())
                    {
                        XPathNavigator nav = iter.Current;
                        ImagingSettings imagingSettings = new ImagingSettings();
                        string strImagingType = nav.GetAttribute("ImagingType", "");
                        ImagingType imagingType;
                        Enum.TryParse(strImagingType, out imagingType); //string to enum

                        bool bIsVisible;
                        string strVisibility = iter.Current.GetAttribute("IsVisible", "");
                        if (!Boolean.TryParse(strVisibility, out bIsVisible))
                        {
                            bIsVisible = false;
                        }
                        imagingSettings.ImagingTabType = imagingType;
                        imagingSettings.IsVisible = bIsVisible;
                        configSettings.ImagingSettings.Add(imagingSettings);
                    }

                    iter = xpathNav.Select("/SysSettings/SampleTypes/SampleType");
                    while (iter.MoveNext())
                    {
                        XPathNavigator nav = iter.Current;
                        SampleTypeSetting sampleTypeSetting = new SampleTypeSetting();

                        string strPosition = nav.GetAttribute("Position", "");
                        int position = 0;
                        int.TryParse(strPosition, out position);

                        string strSampleType = nav.GetAttribute("DisplayName", "");
                        //SampleType sampleType;
                        //Enum.TryParse(strSampleType, out sampleType); //string to enum

                        string strFocusPos = nav.GetAttribute("FocusPosition", "");
                        double focusPosition = 0;
                        double.TryParse(strFocusPos, out focusPosition);

                        sampleTypeSetting.Position = position;
                        sampleTypeSetting.DisplayName = strSampleType;
                        //sampleTypeSetting.SampleType = sampleType;
                        sampleTypeSetting.FocusPosition = focusPosition;

                        configSettings.SampleTypeSettings.Add(sampleTypeSetting);
                    }

                    iter = xpathNav.Select("/SysSettings/EmissionFilters/EmissionFilter");
                    while (iter.MoveNext())
                    {
                        XPathNavigator nav = iter.Current;

                        FilterType filterType = new FilterType();
                        string strWavelength = nav.GetAttribute("Wavelength", "");
                        int nWavelength = 0;
                        int.TryParse(strWavelength, out nWavelength);
                        filterType.Wavelength = nWavelength;
                        filterType.Bandpass = nav.GetAttribute("Bandpass", "");
                        //string strBandpass = nav.GetAttribute("Bandpass", "");
                        //int nBandpass = 0;
                        //int.TryParse(strBandpass, out nBandpass);
                        //filterType.Bandpass = nBandpass;
                        configSettings.FilterOptions.Add(filterType);
                    }

                    //iter = xpathNav.Select("/SysSettings/LaserTypes/LaserType");
                    //while (iter.MoveNext())
                    //{
                    //    XPathNavigator nav = iter.Current;
                    //
                    //    LaserTypeWavelength laserTypeWavelength = new LaserTypeWavelength();
                    //    string strLaserType = nav.GetAttribute("Laser", "");
                    //    LaserType laserType;
                    //    Enum.TryParse(strLaserType, out laserType); //string to enum
                    //    string strWavelength = nav.GetAttribute("WaveLength", "");
                    //    laserTypeWavelength.LaserType = laserType;
                    //    laserTypeWavelength.Wavelength = strWavelength;
                    //    configSettings.LasersWavelength.Add(laserTypeWavelength);
                    //}

                    // 20180221: Moved to config.xml
                    // Read signal options
                    /*iter = xpathNav.Select("/SysSettings/Signals/Signal");
                    while (iter.MoveNext())
                    {
                        XPathNavigator nav = iter.Current;

                        string strLaserType = nav.GetAttribute("LaserType", "");
                        strLaserType = "Laser" + strLaserType;
                        LaserType laserType = (LaserType)Enum.Parse(typeof(LaserType), strLaserType);

                        if (nav.HasChildren)
                        {
                            if (nav.MoveToFirstChild())
                            {
                                do
                                {
                                    Signal signal = new Signal();
                                    signal.Position = int.Parse(nav.GetAttribute("Position", ""));
                                    signal.LaserIntensity = int.Parse(nav.GetAttribute("LaserIntensity", ""));
                                    signal.ApdGain = int.Parse(nav.GetAttribute("ApdGain", ""));
                                    signal.ApdPga = int.Parse(nav.GetAttribute("ApdPga", ""));
                                    signal.LaserType = laserType;

                                    switch (laserType)
                                    {
                                        case LaserType.LaserA:
                                            configSettings.LaserASignalOptions.Add(signal);
                                            break;
                                        case LaserType.LaserB:
                                            configSettings.LaserBSignalOptions.Add(signal);
                                            break;
                                        case LaserType.LaserC:
                                            configSettings.LaserCSignalOptions.Add(signal);
                                            break;
                                        case LaserType.LaserD:
                                            configSettings.LaserDSignalOptions.Add(signal);
                                            break;
                                    }
                                }
                                while (nav.MoveToNext());
                                nav.MoveToParent();
                            }
                        }
                    }*/

                    // 20180221: Moved to config.xml
                    // Read signal options
                    /*iter = xpathNav.Select("/SysSettings/PhosphorSignals/PhosphorSignal");
                    while (iter.MoveNext())
                    {
                        XPathNavigator nav = iter.Current;

                        string strLaserType = nav.GetAttribute("LaserType", "");
                        strLaserType = "Laser" + strLaserType;
                        LaserType laserType = (LaserType)Enum.Parse(typeof(LaserType), strLaserType);

                        if (nav.HasChildren)
                        {
                            if (nav.MoveToFirstChild())
                            {
                                do
                                {
                                    Signal signal = new Signal();
                                    signal.Position = int.Parse(nav.GetAttribute("Position", ""));
                                    signal.LaserIntensity = int.Parse(nav.GetAttribute("LaserIntensity", ""));
                                    signal.ApdGain = int.Parse(nav.GetAttribute("ApdGain", ""));
                                    signal.ApdPga = int.Parse(nav.GetAttribute("ApdPga", ""));
                                    signal.LaserType = laserType;

                                    switch (laserType)
                                    {
                                        //case LaserType.LaserA:
                                        //    configSettings.LaserASignalOptions.Add(signal);
                                        //    break;
                                        //case LaserType.LaserB:
                                        //    configSettings.LaserBSignalOptions.Add(signal);
                                        //    break;
                                        case LaserType.LaserC:
                                            configSettings.PhosphorCSignalOptions.Add(signal);
                                            break;
                                        case LaserType.LaserD:
                                            configSettings.PhosphorDSignalOptions.Add(signal);
                                            break;
                                    }
                                }
                                while (nav.MoveToNext());
                                nav.MoveToParent();
                            }
                        }
                    }*/

                    /*
                    iter = xpathNav.Select("/SysSettings/MotorSettings/MotorSetting");
                    while (iter.MoveNext())
                    {
                        XPathNavigator nav = iter.Current;
                        MotorSettingsType motorSetting = new MotorSettingsType();
                        MotorType motorType;
                        Enum.TryParse(nav.GetAttribute("Motor", ""), out motorType); //string to enum
                        double speed = 0;
                        double.TryParse(nav.GetAttribute("Speed", ""), out speed);

                        motorSetting.MotorType = motorType;
                        motorSetting.Speed = speed;
                        configSettings.MotorSettings.Add(motorSetting);
                    }*/

                    xpathNav = null;
                    xpathDoc = null;
                }
                catch (Exception ex)
                {
                    string message = string.Format("Error reading system settings: \"{0}\"\nERROR: {1}", sysSettingsFileName, ex.Message);
                    throw new Exception(message, ex);
                }
            }

            /// <summary>
            /// Load application protocols
            /// </summary>
            public static void LoadProtocols()
            {
                string configFilePath = Path.Combine(applicationDataPath, protocolsFilename);

                if (!File.Exists(configFilePath))
                {
                    throw new Exception("Configuration file does not exits: " + configFilePath);
                }

                XPathDocument xpathDoc = new XPathDocument(configFilePath);
                XPathNavigator xpathNav = xpathDoc.CreateNavigator();

                //XPath query for the child node called ScanRegion
                XPathNodeIterator iter = xpathNav.Select("//Protocols/Protocol/ScanRegion");

                if (iter.Count == 0)
                {
                    // ScanRegion child node not found, assume old protocol file (without multi-region scan)
                    //
                    // Import old protocol file
                    ImportAndExportProtocol();

                    // Save the old/original 'Protocols.xml' to 'Protocols.OLD'
                    string dstFileName = "Protocols.OLD";
                    string dstFilePath = Path.Combine(applicationDataPath, dstFileName);
                    File.Copy(configFilePath, dstFilePath, true);

                    // Remove all existing protocols
                    RemoveAppProtocol(configSettings.Protocols);
                    // Export Fluorescent protocols
                    ExportAppProtocol(configSettings.Protocols, "Protocols", "Protocol");
                    // Export Phosphor protocols
                    ExportAppProtocol(configSettings.PhosphorProtocols, "PhosphorProtocols", "PhosphorProtocol");
                }
                else
                {
                    //iter = xpathNav.Select("/Config/Dyes/Dye");
                    //while (iter.MoveNext())
                    //{
                    //    XPathNavigator nav = iter.Current;
                    //    string strPosition = nav.GetAttribute("Position", "");
                    //    int position = 0;
                    //    int.TryParse(strPosition, out position);
                    //
                    //    string displayName = nav.GetAttribute("DisplayName", "");
                    //
                    //    string strLaserType = nav.GetAttribute("Laser", "");
                    //    LaserType laserType;
                    //    Enum.TryParse(strLaserType, out laserType); //string to enum
                    //
                    //    string strWaveLength = nav.GetAttribute("WaveLength", "");
                    //    //int waveLength = 0;
                    //    //int.TryParse(strWaveLength, out waveLength);
                    //
                    //    DyeType dyeType = new DyeType();
                    //    dyeType.Position = position;
                    //    dyeType.DisplayName = displayName;
                    //    dyeType.LaserType = laserType;
                    //    //dyeType.WaveLength = waveLength;
                    //    dyeType.WaveLength = strWaveLength;
                    //    configSettings.DyeOptions.Add(dyeType);
                    //}

                    iter = xpathNav.Select("/Config/Protocols/Protocol");
                    while (iter.MoveNext())
                    {
                        XPathNavigator nav = iter.Current;
                        string protocolName = nav.GetAttribute("DisplayName", "");
                        string strIsAlwaysVisible = nav.GetAttribute("IsAlwaysVisible", "");
                        bool bIsAlwaysVisible = false;
                        bool.TryParse(strIsAlwaysVisible, out bIsAlwaysVisible);

                        Protocol protocol = new Protocol();
                        protocol.Name = protocolName;
                        protocol.IsAlwaysVisible = bIsAlwaysVisible;

                        if (nav.HasChildren)
                        {
                            int nScanRegionCount = 0;
                            nav.MoveToFirstChild();
                            do
                            {
                                if (nav.Name == "ScanRegion")
                                {
                                    ScanRegion scanRegion = new ScanRegion();

                                    string strSampleType = nav.GetAttribute("SampleType", "");
                                    int sampleType;
                                    int.TryParse(strSampleType, out sampleType);

                                    string strPixelSize = nav.GetAttribute("PixelSize", "");
                                    int pixelSize = 0;
                                    int.TryParse(strPixelSize, out pixelSize);

                                    string strScanspeed = nav.GetAttribute("ScanSpeed", "");
                                    int scanSpeed = 0;
                                    int.TryParse(strScanspeed, out scanSpeed);

                                    string strScanQuality = nav.GetAttribute("ScanQuality", "");
                                    int scanQuality = 0;
                                    int.TryParse(strScanQuality, out scanQuality);

                                    string strScanRegionRect = nav.GetAttribute("ScanRegionRect", "");
                                    Rect scanRegionRect = Rect.Parse(strScanRegionRect);

                                    string strAutoSaveEnabled = nav.GetAttribute("IsAutoSave", "");
                                    bool bIsAutoSaveEnabled = false;
                                    bool.TryParse(strAutoSaveEnabled, out bIsAutoSaveEnabled);

                                    string strCustomFocus = nav.GetAttribute("IsCustomFocus", "");
                                    bool bIsCustomFocus = false;
                                    bool.TryParse(strCustomFocus, out bIsCustomFocus);
                                    double dCustomFocusVal = 0;
                                    if (bIsCustomFocus)
                                    {
                                        string strCustomFocusVal = nav.GetAttribute("CustomFocus", "");
                                        double.TryParse(strCustomFocusVal, out dCustomFocusVal);
                                    }
                                    string strZScanning = nav.GetAttribute("IsZScan", "");
                                    bool bIsZScan = false;
                                    bool.TryParse(strZScanning, out bIsZScan);

                                    //scanRegion.DisplayName = scanRegionName;
                                    scanRegion.RegionNumber = ++nScanRegionCount;
                                    scanRegion.SampleType = sampleType;
                                    scanRegion.PixelSize = pixelSize;
                                    scanRegion.ScanSpeed = scanSpeed;
                                    scanRegion.ScanQuality = scanQuality;
                                    scanRegion.ScanRect = scanRegionRect;
                                    scanRegion.IsAutoSaveEnabled = bIsAutoSaveEnabled;
                                    scanRegion.IsCustomFocus = bIsCustomFocus;
                                    scanRegion.CustomFocusPosition = dCustomFocusVal;
                                    scanRegion.IsZScan = bIsZScan;

                                    //appProtocol.Name = protocolName;
                                    //appProtocol.ScanRegions.Add(scanRegion);

                                    if (nav.HasChildren)
                                    {
                                        nav.MoveToFirstChild();
                                        do
                                        {
                                            if (nav.Name == "Laser")
                                            {
                                                string strLaserType = nav.GetAttribute("LaserType", "");
                                                int laserType = 0;
                                                int.TryParse(strLaserType, out laserType);
                                                string strSignalInt = nav.GetAttribute("SignalIntensity", "");
                                                int signalInt = 0;
                                                int.TryParse(strSignalInt, out signalInt);
                                                string strColorChan = nav.GetAttribute("ColorChannel", "");
                                                ImageChannelType colorChannel;
                                                Enum.TryParse(strColorChan, out colorChannel); //string to enum

                                                //AppDyeData dyeData = new AppDyeData();
                                                //dyeData.LaserType = laserType;
                                                //dyeData.SignalIntensity = signalInt;
                                                //dyeData.ColorChannel = colorChannel;
                                                //
                                                //scanRegion.Dyes.Add(dyeData);
                                                LaserTypes laserData = new LaserTypes();
                                                laserData.Wavelength = laserType;
                                                laserData.SignalIntensity = signalInt;
                                                laserData.ColorChannel = colorChannel;

                                                scanRegion.Lasers.Add(laserData);
                                            }

                                            if (bIsAutoSaveEnabled && nav.Name == "AutoSave")
                                            {
                                                string strAutoSavePath = nav.GetAttribute("Path", "");
                                                scanRegion.AutoSavePath = strAutoSavePath;
                                            }

                                            if (nav.Name == "ZScan")
                                            {
                                                string strBotFocus = nav.GetAttribute("BottomImageFocus", "");
                                                double botFocus = 0;
                                                double.TryParse(strBotFocus, out botFocus);
                                                string strFocusDelta = nav.GetAttribute("DeltaFocus", "");
                                                double focusDelta = 0;
                                                double.TryParse(strFocusDelta, out focusDelta);
                                                string strNumOfImages = nav.GetAttribute("NumOfImages", "");
                                                int numOfImages = 0;
                                                int.TryParse(strNumOfImages, out numOfImages);

                                                scanRegion.BottomImageFocus = botFocus;
                                                scanRegion.DeltaFocus = focusDelta;
                                                scanRegion.NumOfImages = numOfImages;
                                            }

                                        } while (nav.MoveToNext());
                                        nav.MoveToParent();
                                    }

                                    protocol.ScanRegions.Add(scanRegion);
                                }
                            } while (nav.MoveToNext());
                        }

                        configSettings.Protocols.Add(protocol);
                    }

                    iter = xpathNav.Select("/Config/PhosphorProtocols/PhosphorProtocol");
                    while (iter.MoveNext())
                    {
                        XPathNavigator nav = iter.Current;

                        string protocolDisplayName = nav.GetAttribute("DisplayName", "");

                        Protocol protocol = new Protocol();
                        protocol.Name = protocolDisplayName;

                        if (nav.HasChildren)
                        {
                            int nScanRegionCount = 0;
                            nav.MoveToFirstChild();
                            do
                            {
                                if (nav.Name == "ScanRegion")
                                {
                                    ScanRegion scanRegion = new ScanRegion();

                                    //string scanRegionName = nav.GetAttribute("DisplayName", "");

                                    string strSampleType = nav.GetAttribute("SampleType", "");
                                    int sampleType;
                                    int.TryParse(strSampleType, out sampleType);

                                    string strPixelSize = nav.GetAttribute("PixelSize", "");
                                    int pixelSize = 0;
                                    int.TryParse(strPixelSize, out pixelSize);

                                    string strScanspeed = nav.GetAttribute("ScanSpeed", "");
                                    int scanSpeed = 0;
                                    int.TryParse(strScanspeed, out scanSpeed);

                                    // Phosphor Imaging tab no longer has the scan Quality selection option
                                    //string strScanQuality = nav.GetAttribute("ScanQuality", "");
                                    //int scanQuality = 0;
                                    //int.TryParse(strScanQuality, out scanQuality);

                                    string strIntensityLevel = nav.GetAttribute("IntensityLevel", "");
                                    int intensityLevel = 0;
                                    int.TryParse(strIntensityLevel, out intensityLevel);
                                    if (intensityLevel == 0)
                                    {
                                        // Reading error or missing 'IntensityLevel' - default to intensity level 3
                                        intensityLevel = 3;
                                    }

                                    string strScanRegionRect = nav.GetAttribute("ScanRegionRect", "");
                                    Rect scanRegionRect = Rect.Parse(strScanRegionRect);

                                    scanRegion.RegionNumber = ++nScanRegionCount;
                                    scanRegion.SampleType = sampleType;
                                    scanRegion.PixelSize = pixelSize;
                                    scanRegion.ScanSpeed = scanSpeed;
                                    //scanRegion.ScanQuality = scanQuality;
                                    scanRegion.IntensityLevel = intensityLevel;
                                    scanRegion.ScanRect = scanRegionRect;

                                    protocol.ScanRegions.Add(scanRegion);
                                }
                            } while (nav.MoveToNext());

                            configSettings.PhosphorProtocols.Add(protocol);
                        }
                    }

                    xpathNav = null;
                    xpathDoc = null;
                }
            }

            /// <summary>
            /// Load custom settings (CustSettings.xml).
            /// </summary>
            /*public static void LoadCustomSettingsFile()
            {
                try
                {
                    string configFilePath = Path.Combine(applicationDataPath, custSettingsFileName);

                    if (!File.Exists(configFilePath))
                    {
                        //throw new Exception("Configuration file does not exits: " + configFilePath);
                        return;
                    }

                    XPathDocument xpathDoc = new XPathDocument(configFilePath);
                    XPathNavigator xpathNav = xpathDoc.CreateNavigator();

                    XPathNodeIterator iter = xpathNav.Select("/CustSettings/Dyes/Dye");
                    while (iter.MoveNext())
                    {
                        XPathNavigator nav = iter.Current;
                        string strPosition = nav.GetAttribute("Position", "");
                        int position = 0;
                        int.TryParse(strPosition, out position);

                        string displayName = nav.GetAttribute("DisplayName", "");
                        string strLaserType = nav.GetAttribute("Laser", "");
                        LaserType laserType;
                        Enum.TryParse(strLaserType, out laserType); //string to enum
                        string strWaveLength = nav.GetAttribute("WaveLength", "");

                        DyeType dyeType = new DyeType();
                        dyeType.Position = position;
                        dyeType.DisplayName = displayName;
                        dyeType.LaserType = laserType;
                        dyeType.WaveLength = strWaveLength;
                        dyeType.IsCustomDye = true;
                        configSettings.CustDyeOptions.Add(dyeType);
                    }

                    xpathNav = null;
                    xpathDoc = null;
                }
                catch (Exception ex)
                {
                    string message = string.Format("Error reading custom settings: \"{0}\"\nERROR: {1}", custSettingsFileName, ex.Message);
                    throw new Exception(message, ex);
                }
            }*/

            //private static string GetLaserWaveLength(ImagingSystem.LaserType laserType)
            //{
            //    string result = string.Empty;
            //    if (SettingsManager.ConfigSettings.LasersWavelength != null && SettingsManager.ConfigSettings.LasersWavelength.Count > 0)
            //    {
            //        foreach (var wavelength in SettingsManager.ConfigSettings.LasersWavelength)
            //        {
            //            if (wavelength.LaserType == laserType)
            //            {
            //                string[] values = wavelength.Wavelength.Split('/');
            //                result = values[0].Trim();
            //                break;
            //            }
            //        }
            //    }
            //    else if (SettingsManager.ConfigSettings.DyeOptions != null && SettingsManager.ConfigSettings.DyeOptions.Count > 0)
            //    {
            //        foreach (var dye in SettingsManager.ConfigSettings.DyeOptions)
            //        {
            //            if (dye.LaserType == laserType)
            //            {
            //                string[] values = dye.WaveLength.Split('/');
            //                result = values[0].Trim();
            //                break;
            //            }
            //        }
            //    }
            //    return result;
            //}
            #endregion Other Functions

            #region Helper Functions

            // Search predicate returns true if a string contains "Azure".
            //private static bool IsAzureDye(DyeType dye)
            //{
            //    bool bResult = false;
            //    if (dye.DisplayName.Contains("Azure"))
            //        bResult = true;
            //    return bResult;
            //}

            private static void ImportAndExportProtocol()
            {
                string configFilePath = Path.Combine(applicationDataPath, protocolsFilename);

                if (!File.Exists(configFilePath))
                {
                    throw new Exception("Configuration file does not exits: " + configFilePath);
                }

                XPathDocument xpathDoc = new XPathDocument(configFilePath);
                XPathNavigator xpathNav = xpathDoc.CreateNavigator();

                //XPathNodeIterator iter = xpathNav.Select("/Config/Dyes/Dye");
                //while (iter.MoveNext())
                //{
                //    XPathNavigator nav = iter.Current;
                //    string strPosition = nav.GetAttribute("Position", "");
                //    int position = 0;
                //    int.TryParse(strPosition, out position);

                //    string displayName = nav.GetAttribute("DisplayName", "");

                //    string strLaserType = nav.GetAttribute("Laser", "");
                //    LaserType laserType;
                //    Enum.TryParse(strLaserType, out laserType); //string to enum

                //    string strWaveLength = nav.GetAttribute("WaveLength", "");
                //    //int waveLength = 0;
                //    //int.TryParse(strWaveLength, out waveLength);

                //    DyeType dyeType = new DyeType();
                //    dyeType.Position = position;
                //    dyeType.DisplayName = displayName;
                //    dyeType.LaserType = laserType;
                //    //dyeType.WaveLength = waveLength;
                //    dyeType.WaveLength = strWaveLength;
                //    configSettings.DyeOptions.Add(dyeType);
                //}

                XPathNodeIterator iter = xpathNav.Select("/Config/Protocols/Protocol");
                while (iter.MoveNext())
                {
                    XPathNavigator nav = iter.Current;

                    string displayName = nav.GetAttribute("DisplayName", "");

                    string strSampleType = nav.GetAttribute("SampleType", "");
                    int sampleType;
                    int.TryParse(strSampleType, out sampleType);

                    string strPixelSize = nav.GetAttribute("PixelSize", "");
                    int pixelSize = 0;
                    int.TryParse(strPixelSize, out pixelSize);

                    string strScanspeed = nav.GetAttribute("ScanSpeed", "");
                    int scanSpeed = 0;
                    int.TryParse(strScanspeed, out scanSpeed);

                    string strScanQuality = nav.GetAttribute("ScanQuality", "");
                    int scanQuality = 0;
                    int.TryParse(strScanQuality, out scanQuality);

                    string strCustomFocus = nav.GetAttribute("IsCustomFocus", "");
                    bool bIsCustomFocus = false;
                    bool.TryParse(strCustomFocus, out bIsCustomFocus);
                    double dCustomFocusVal = 0;
                    if (bIsCustomFocus)
                    {
                        string strCustomFocusVal = nav.GetAttribute("CustomFocus", "");
                        double.TryParse(strCustomFocusVal, out dCustomFocusVal);
                    }
                    string strZScanning = nav.GetAttribute("IsZScan", "");
                    bool bIsZScan = false;
                    bool.TryParse(strZScanning, out bIsZScan);

                    Protocol protocol = new Protocol();
                    protocol.Name = displayName;

                    ScanRegion scanRegion = new ScanRegion();
                    scanRegion.RegionNumber = 1;
                    scanRegion.SampleType = sampleType;
                    scanRegion.PixelSize = pixelSize;
                    scanRegion.ScanSpeed = scanSpeed;
                    scanRegion.ScanQuality = scanQuality;
                    scanRegion.IsCustomFocus = bIsCustomFocus;
                    scanRegion.CustomFocusPosition = dCustomFocusVal;
                    scanRegion.IsZScan = bIsZScan;
                    Rect scanRegionRect = new Rect(0, 0, 10, 10);
                    scanRegion.ScanRect = scanRegionRect;

                    if (nav.HasChildren)
                    {
                        nav.MoveToFirstChild();
                        do
                        {
                            if (nav.Name == "Laser")
                            {
                                string strDyeType = nav.GetAttribute("LaserType", "");
                                int laserType = 0;
                                int.TryParse(strDyeType, out laserType);
                                string strSignalInt = nav.GetAttribute("SignalIntensity", "");
                                int signalInt = 0;
                                int.TryParse(strSignalInt, out signalInt);
                                string strColorChan = nav.GetAttribute("ColorChannel", "");
                                ImageChannelType colorChannel;
                                Enum.TryParse(strColorChan, out colorChannel); //string to enum

                                //AppDyeData dyeData = new AppDyeData();
                                //dyeData.DyeType = dyeType;
                                //dyeData.SignalIntensity = signalInt;
                                //dyeData.ColorChannel = colorChannel;
                                //
                                //scanRegion.Dyes.Add(dyeData);

                                LaserTypes laserData = new LaserTypes();
                                laserData.Wavelength = laserType;
                                laserData.SignalIntensity = signalInt;
                                laserData.ColorChannel = colorChannel;
                                scanRegion.Lasers.Add(laserData);

                            }
                            if (nav.Name == "ZScan")
                            {
                                string strBotFocus = nav.GetAttribute("BottomImageFocus", "");
                                double botFocus = 0;
                                double.TryParse(strBotFocus, out botFocus);
                                string strFocusDelta = nav.GetAttribute("DeltaFocus", "");
                                double focusDelta = 0;
                                double.TryParse(strFocusDelta, out focusDelta);
                                string strNumOfImages = nav.GetAttribute("NumOfImages", "");
                                int numOfImages = 0;
                                int.TryParse(strNumOfImages, out numOfImages);

                                scanRegion.BottomImageFocus = botFocus;
                                scanRegion.DeltaFocus = focusDelta;
                                scanRegion.NumOfImages = numOfImages;
                            }
                        } while (nav.MoveToNext());
                    }
                    protocol.ScanRegions.Add(scanRegion);
                    configSettings.Protocols.Add(protocol);
                }

                iter = xpathNav.Select("/Config/PhosphorProtocols/PhosphorProtocol");
                while (iter.MoveNext())
                {
                    XPathNavigator nav = iter.Current;

                    string displayName = nav.GetAttribute("DisplayName", "");

                    string strSampleType = nav.GetAttribute("SampleType", "");
                    int sampleType;
                    int.TryParse(strSampleType, out sampleType);

                    string strPixelSize = nav.GetAttribute("PixelSize", "");
                    int pixelSize = 0;
                    int.TryParse(strPixelSize, out pixelSize);

                    string strScanspeed = nav.GetAttribute("ScanSpeed", "");
                    int scanSpeed = 0;
                    int.TryParse(strScanspeed, out scanSpeed);

                    string strScanQuality = nav.GetAttribute("ScanQuality", "");
                    int scanQuality = 0;
                    int.TryParse(strScanQuality, out scanQuality);

                    string strIntensityLevel = nav.GetAttribute("IntensityLevel", "");
                    int intensityLevel = 0;
                    int.TryParse(strIntensityLevel, out intensityLevel);

                    Protocol protocol = new Protocol();
                    protocol.Name = displayName;
                    ScanRegion scanRegion = new ScanRegion();
                    scanRegion.RegionNumber = 1;
                    scanRegion.SampleType = sampleType;
                    scanRegion.PixelSize = pixelSize;
                    scanRegion.ScanSpeed = scanSpeed;
                    scanRegion.ScanQuality = scanQuality;
                    scanRegion.IntensityLevel = intensityLevel;
                    Rect scanRegionRect = new Rect(0, 0, 10, 10);
                    scanRegion.ScanRect = scanRegionRect;

                    protocol.ScanRegions.Add(scanRegion);
                    configSettings.PhosphorProtocols.Add(protocol);
                }

                xpathNav = null;
                xpathDoc = null;
            }

            private static bool ExportAppProtocol(List<Protocol> protocols, string parentNodeName, string childNodeName)
            {
                string configFilePath = Path.Combine(applicationDataPath, protocolsFilename);

                XmlDocument XDoc = new XmlDocument();
                //XDoc.PreserveWhitespace = true;   //this flag override auto indent settings
                XmlNode protocolsParentNode = null;
                bool bUserConfigExists = false;

                if (System.IO.File.Exists(configFilePath))
                {
                    XDoc.Load(configFilePath);
                    XmlNodeList nodeList = XDoc.GetElementsByTagName(parentNodeName);
                    if (nodeList.Count == 0)
                    {
                        XmlNode rootNode = XDoc.DocumentElement;
                        protocolsParentNode = XDoc.CreateElement(parentNodeName);
                        rootNode.AppendChild(protocolsParentNode);
                    }
                    else
                    {
                        protocolsParentNode = nodeList.Item(0);
                    }
                    bUserConfigExists = true;
                }

                if (!bUserConfigExists)
                {
                    var comment = XDoc.CreateComment("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
                    XmlNode rootNode = XDoc.CreateElement("Config");
                    XDoc.InsertBefore(comment, rootNode);
                    XDoc.AppendChild(rootNode);

                    protocolsParentNode = XDoc.CreateElement(parentNodeName);
                    rootNode.AppendChild(protocolsParentNode);
                }

                foreach (Protocol protocol in protocols)
                {
                    if (protocol != null)
                    {
                        XmlNode protocolNode = XDoc.CreateElement(childNodeName);
                        protocolsParentNode.AppendChild(protocolNode);

                        XmlAttribute xmlAttrib = XDoc.CreateAttribute("DisplayName");
                        xmlAttrib.Value = protocol.Name; //new protocol name
                        protocolNode.Attributes.Append(xmlAttrib);

                        foreach (ScanRegion scanRegion in protocol.ScanRegions)
                        {
                            XmlNode scanRegionNode = XDoc.CreateElement("ScanRegion");
                            protocolNode.AppendChild(scanRegionNode);

                            xmlAttrib = XDoc.CreateAttribute("SampleType");
                            xmlAttrib.Value = scanRegion.SampleType.ToString();
                            scanRegionNode.Attributes.Append(xmlAttrib);

                            xmlAttrib = XDoc.CreateAttribute("PixelSize");
                            xmlAttrib.Value = scanRegion.PixelSize.ToString();
                            scanRegionNode.Attributes.Append(xmlAttrib);

                            xmlAttrib = XDoc.CreateAttribute("ScanSpeed");
                            xmlAttrib.Value = scanRegion.ScanSpeed.ToString();
                            scanRegionNode.Attributes.Append(xmlAttrib);

                            xmlAttrib = XDoc.CreateAttribute("ScanQuality");
                            xmlAttrib.Value = scanRegion.ScanQuality.ToString();
                            scanRegionNode.Attributes.Append(xmlAttrib);

                            // Phosphor Imaging has no dye selection
                            if (parentNodeName.Contains("Phosphor"))
                            {
                                xmlAttrib = XDoc.CreateAttribute("IntensityLevel");
                                xmlAttrib.Value = scanRegion.IntensityLevel.ToString();
                                scanRegionNode.Attributes.Append(xmlAttrib);
                            }

                            xmlAttrib = XDoc.CreateAttribute("ScanRegionRect");
                            xmlAttrib.Value = scanRegion.ScanRect.ToString();
                            scanRegionNode.Attributes.Append(xmlAttrib);

                            if (scanRegion.IsCustomFocus)
                            {
                                xmlAttrib = XDoc.CreateAttribute("IsCustomFocus");
                                xmlAttrib.Value = scanRegion.IsCustomFocus.ToString();
                                scanRegionNode.Attributes.Append(xmlAttrib);
                                xmlAttrib = XDoc.CreateAttribute("CustomFocus");
                                xmlAttrib.Value = scanRegion.CustomFocusPosition.ToString();
                                scanRegionNode.Attributes.Append(xmlAttrib);
                            }
                            else if (scanRegion.IsZScan)
                            {
                                xmlAttrib = XDoc.CreateAttribute("IsZScan");
                                xmlAttrib.Value = scanRegion.IsZScan.ToString();
                                scanRegionNode.Attributes.Append(xmlAttrib);
                            }

                            // Phosphor Imaging has no dye selection
                            if (!parentNodeName.Contains("Phosphor") && scanRegion.Lasers.Count > 0)
                            {
                                for (int index = 0; index < scanRegion.Lasers.Count; index++)
                                {
                                    LaserTypes laser = scanRegion.Lasers[index];

                                    if (laser != null)
                                    {
                                        XmlNode dyeNode = XDoc.CreateElement("Laser");
                                        scanRegionNode.AppendChild(dyeNode);
                                        xmlAttrib = XDoc.CreateAttribute("LaserType");
                                        xmlAttrib.Value = laser.Wavelength.ToString();
                                        dyeNode.Attributes.Append(xmlAttrib);
                                        xmlAttrib = XDoc.CreateAttribute("SignalIntensity");
                                        xmlAttrib.Value = laser.SignalIntensity.ToString();
                                        dyeNode.Attributes.Append(xmlAttrib);
                                        xmlAttrib = XDoc.CreateAttribute("ColorChannel");
                                        xmlAttrib.Value = laser.ColorChannel.ToString();
                                        dyeNode.Attributes.Append(xmlAttrib);
                                    }
                                }
                            }

                            // Save z-scanning settings
                            if (scanRegion.IsZScan)
                            {
                                XmlNode dyeNode = XDoc.CreateElement("ZScan");
                                scanRegionNode.AppendChild(dyeNode);
                                xmlAttrib = XDoc.CreateAttribute("BottomImageFocus");
                                xmlAttrib.Value = scanRegion.BottomImageFocus.ToString();
                                dyeNode.Attributes.Append(xmlAttrib);
                                xmlAttrib = XDoc.CreateAttribute("DeltaFocus");
                                xmlAttrib.Value = scanRegion.DeltaFocus.ToString();
                                dyeNode.Attributes.Append(xmlAttrib);
                                xmlAttrib = XDoc.CreateAttribute("NumOfImages");
                                xmlAttrib.Value = scanRegion.NumOfImages.ToString();
                                dyeNode.Attributes.Append(xmlAttrib);
                            }
                        }   //closing: foreach ScanRegions
                    }
                }   // closing foreach protocols

                XDoc.Beautify();
                XDoc.Save(configFilePath);

                return true;
            }

            private static bool RemoveAppProtocol(List<Protocol> protocols)
            {
                bool bResult = false;
                //bool bSaveDoc = false;

                string configFilePath = Path.Combine(applicationDataPath, protocolsFilename);
                XmlDocument xdoc = new XmlDocument();
                xdoc.PreserveWhitespace = true;

                try
                {
                    xdoc.Load(configFilePath);

                    // Remove all elements
                    XmlNode root = xdoc.DocumentElement;
                    //Remove all attribute and child nodes.
                    root.RemoveAll();

                    /*XmlNodeList protocolElements = xdoc.GetElementsByTagName("Protocol");
                    if (protocolElements != null)
                    {
                        //XmlNode commentNode = null;
                        for (int i = 0; i < protocolElements.Count; i++)
                        {
                            if (protocolElements[i].NodeType == XmlNodeType.Element)
                            {
                                //commentNode = xdoc.CreateComment(protocolElements[i].OuterXml);
                                protocolElements[i].ParentNode.RemoveChild(protocolElements[i]);
                                i--; // hack: for some reason it skips a node after a child is replaced (when removing a node from top down).
                                bSaveDoc = true;
                            }
                        }
                    }*/

                    xdoc.Beautify();
                    xdoc.Save(configFilePath);
                    bResult = true;
                }
                catch
                {
                    bResult = false;
                }

                return bResult;
            }

            #endregion
        }

    }

}
