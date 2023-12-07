using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Windows.Threading;
using System.Diagnostics;
using Azure.WPF.Framework;
using Azure.Configuration.Settings;
using Azure.LaserScanner.ViewModel; //Workspace
using Azure.ImagingSystem;
using Azure.LaserScanner.SplashScreen;

namespace Azure.LaserScanner
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static System.Threading.Mutex mutex = new System.Threading.Mutex(true, "{7174CED8-6862-4E06-936A-DE18CB778C94}");
        private MainWindow _MainWindow = null;

        /// <summary>
        /// Application startup event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                try
                {
                    #region Get product information from assembly...
                    string companyName = string.Empty;
                    string productName = string.Empty;
                    string productVersion = string.Empty;
                    object[] customAttributes = null;

                    try
                    {
                        Splasher.Splash = new SplashScreenWindow();
                        Splasher.ShowSplash();
                        MessageListener.Instance.ReceiveProgressMessage("Initializing system...please wait...");

                        System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();

                        if (assembly != null)
                        {
                            customAttributes = assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                            if ((customAttributes != null) && (customAttributes.Length > 0))
                            {
                                companyName = ((AssemblyCompanyAttribute)customAttributes[0]).Company;
                            }

                            if (string.IsNullOrEmpty(companyName))
                            {
                                companyName = "Azure Biosystems";
                            }

                            customAttributes = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                            if ((customAttributes != null) && (customAttributes.Length > 0))
                            {
                                productName = ((AssemblyProductAttribute)customAttributes[0]).Product;
                            }

                            if (string.IsNullOrEmpty(productName))
                            {
                                productName = "Sapphire FL";
                            }

                            //
                            // Does not work because assembly version is NOT a custom attribute.
                            //
                            //customAttributes = assembly.GetCustomAttributes(typeof(AssemblyVersionAttribute), false);
                            //if ((customAttributes != null) && (customAttributes.Length > 0))
                            //{
                            //    productVersion = ((AssemblyVersionAttribute)customAttributes[0]).Version;
                            //}
                        }

                        // Version format: major.minor.build.revision
                        Version version = Assembly.GetExecutingAssembly().GetName().Version;
                        //_MainWindow.ProductVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                        // New version scheme: Major.Minor.BuildNumber.Revision
                        //                     Build number: 4 digits(display leading zero).
                        if (version != null)
                        {
                            productVersion = string.Format("{0}.{1}.{2}.{3}",
                                                           version.Major,
                                                           version.Minor,
                                                           version.Build,
                                                           version.Revision.ToString("D4"));
                        }
                    }
                    catch
                    {
                    }
                    #endregion

                    #region Create common application folders
                    //
                    // Make sure the application program data directory exists.
                    // Create common application folders (if not already exists).
                    //
                    string commonAppDataPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                                           companyName + "\\" + productName);

                    CommonApplicationData commonAppData = new CommonApplicationData(companyName, productName, true);
                    //CommonApplicationData mastersAppData = new CommonApplicationData(companyName, productName + "\\Masters", true);
                    //CommonApplicationData protocolsAppData = new CommonApplicationData(companyName, productName + "\\Protocols", true);
                    //CommonApplicationData protocolsCustomAppData = new CommonApplicationData(companyName, productName + "\\Protocols\\Custom", true);
                    #endregion

                    SettingsManager.IsEngrUI = false;
                    SettingsManager.ApplicationDataPath = commonAppDataPath;
                    SettingsManager.OnStartup();    // load configuration settings

                    //Show startup splash screen
                    //Splasher.ShowSplash();
                    //MessageListener.Instance.ReceiveMessage(string.Format("Initializing...please wait..."));

                    _MainWindow = new MainWindow(commonAppDataPath,
                                                 companyName,
                                                 productName,
                                                 productVersion);

                    _MainWindow.ProductVersion = productVersion;
                    string simulation = string.Empty;
                    if (SettingsManager.ConfigSettings.IsSimulationMode)
                        simulation = " [SIMULATION MODE]";
                    _MainWindow.Title = productName + " [" + productVersion + "]" + simulation;

                    App.Current.MainWindow = _MainWindow;
                    //App.Current.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(Current_DispatcherUnhandledException);
                }
                catch (Exception ex)
                {
                    if (!Splasher.IsSplashClosed)
                        Splasher.CloseSplash();

                    // Using WinForms' MessageBox to display the error message, WPF MessageBox automatically closes.
                    string message = string.Empty;
                    message = ex.Message + "\n\nStack Trace:\n" + ex.StackTrace;
                    Workspace.This.LogMessage(message);
                    System.Windows.Forms.MessageBox.Show(new System.Windows.Forms.NativeWindow(),
                                                         message,
                                                         "Error: Unhandled Application Exception",
                                                         System.Windows.Forms.MessageBoxButtons.OK,
                                                         System.Windows.Forms.MessageBoxIcon.Exclamation);
                }

                AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(AppDomainUnhandledExceptionHandler);   //global
                Application.Current.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(Current_DispatcherUnhandledException); // main UI thread
                //AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

                _MainWindow.Show();
                // Bring the application to the front (make active application)
                if (_MainWindow.WindowState != WindowState.Normal && _MainWindow.WindowState != WindowState.Maximized)
                {
                    _MainWindow.WindowState = WindowState.Normal;
                }
                _MainWindow.Activate();
                _MainWindow.Topmost = true;  // important
                _MainWindow.Topmost = false; // important
                _MainWindow.Focus();         // important

                mutex.ReleaseMutex();
            }
            else
            {
                // This thread opened an existing kernel object with the same
                // string name; another instance of this app must be running now.

                // Gets a new System.Diagnostics.Process component and the
                // associates it with currently active process.
                Process current = Process.GetCurrentProcess();

                // Enumerate through all the process resources on the share
                // local computer that the specified process name.
                foreach (Process process in Process.GetProcessesByName(current.ProcessName))
                {
                    if (process.Id != current.Id)
                    {
                        // This sometimes happen:
                        // 1. Launch application - controller board / camera initialization failed
                        // 2. Close the application
                        // 3. The process is still running in the background
                        // Here we're checking if another process is running and try to bring it to the
                        // foreground and maximize it and if it's not in maximized mode then close it.

                        // bring the application to the foreground in maximize mode
                        NativeMethods.SetForegroundWindow(process.MainWindowHandle);
                        NativeMethods.ShowWindow(process.MainWindowHandle, WindowShowStyle.Maximize);

                        // if not in maximize mode; close it
                        const int GWL_STYLE = (-16);
                        const int WS_MAXIMIZE = 0x1000000;
                        int style = 0;

                        for (int i = 0; i < 3; i++)
                        {
                            style = NativeMethods.GetWindowLong(process.MainWindowHandle, GWL_STYLE);
                            if ((style & WS_MAXIMIZE) != WS_MAXIMIZE)
                            {
                                ImagingHelper.Delay(1000);
                                continue;
                            }
                            else
                            {
                                break;
                            }
                        }

                        if ((style & WS_MAXIMIZE) != WS_MAXIMIZE)
                        {
                            process.Kill();
                        }
                        break;
                    }
                }

                // Terminate this process and gives the underlying operating 
                // system the specified exit code.
                Environment.Exit(-2);

            }
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs ea)
        {
            if (!Splasher.IsSplashClosed)
                Splasher.CloseSplash();

            Exception e = (Exception)ea.ExceptionObject;
            string message = e.Message + "\n\nStack Trace:\n" + e.StackTrace;
            if (_MainWindow != null)
            {
                Workspace.This.LogMessage(message);
                //var parentWindow = Application.Current.MainWindow;
                Xceed.Wpf.Toolkit.MessageBox.Show(_MainWindow, message, "Error: Unhandled Application Domain Exception",
                                MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        /// <summary>
        /// Unhandled application domain exception handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ea"></param>
        void AppDomainUnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs ea)
        {
            if (!Splasher.IsSplashClosed)
                Splasher.CloseSplash();

            Exception e = (Exception)ea.ExceptionObject;
            if (!e.Message.Contains("Thread was being aborted"))
            {
                string message = e.Message + "\n\nStack Trace:\n" + e.StackTrace;
                if (_MainWindow != null)
                {
                    Workspace.This.LogMessage(message);
                    //var parentWindow = Application.Current.MainWindow;
                    Xceed.Wpf.Toolkit.MessageBox.Show(_MainWindow, message, "Error: Unhandled Application Domain Exception",
                                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
        }

        /// <summary>
        /// Unhandled application exception handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // Process unhandled exception

            // Prevent default unhandled exception processing
            //e.Handled = true;

            if (!Splasher.IsSplashClosed)
                Splasher.CloseSplash();

            Exception ex = e.Exception;
            string message = ex.Message + "\n\nStack Trace:\n" + ex.StackTrace;
            if (_MainWindow != null)
            {
                Workspace.This.LogMessage(message);
                //var parentWindow = Application.Current.MainWindow;
                Xceed.Wpf.Toolkit.MessageBox.Show(_MainWindow, message, "Error: Unhandled Application Exception",
                                MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

    }

    public static class NativeMethods
    {
        /// <summary>
        /// Brings the thread that created the specified window into the
        /// foreground and activates the window. Keyboard input is directed
        /// to the window, and various visual cues are changed for the user.
        /// The system assigns a slightly higher priority to the thread that
        /// created the foreground window than it does to other threads.
        /// </summary>
        /// <param name="hWnd">A handle to the window that should be
        /// activated and brought to the foreground.
        /// </param>
        /// <returns>If the window was brought to the foreground, the
        /// return value is nonzero. </returns>
        [DllImport("user32.dll")]
        internal static extern bool SetForegroundWindow(IntPtr hWnd);

        /// <summary>Shows a Window</summary>
        /// <remarks>
        /// <para>To perform certain special effects when showing or hiding a
        /// window, use AnimateWindow.</para>
        /// <para>The first time an application calls ShowWindow, it should use
        /// the WinMain function's nCmdShow parameter as its nCmdShow ..
        /// Subsequent calls to ShowWindow must use one of the values in the
        /// given list, instead of the one specified by the WinMain function's
        /// nCmdShow parameter.</para>
        /// <para>As noted in the discussion of the nCmdShow parameter, the
        /// nCmdShow value is ignored in the first call to ShowWindow if the
        /// program that launched the application specifies startup information
        /// in the structure. In this case, ShowWindow uses the information
        /// specified in the STARTUPINFO structure to show the window. On
        /// subsequent calls, the application must call ShowWindow with ..
        /// set to SW_SHOWDEFAULT to use the startup information provided by ..
        /// program that launched the application. This behavior is designed ..
        /// the following situations: </para>
        /// <list type="">
        ///    <item>Applications create their main window by calling ..
        ///    with the WS_VISIBLE flag set. </item>
        ///    <item>Applications create their main window by calling ..
        ///    with the WS_VISIBLE flag cleared, and later call ShowWindow ..
        ///    SW_SHOW flag set to make it visible.</item>
        /// </list></remarks>
        /// <param name="hWnd">Handle to the window.</param>
        /// <param name="nCmdShow">Specifies how the window is to be shown.
        /// This parameter is ignored the first time an application calls
        /// ShowWindow, if the program that launched the application provides a
        /// STARTUPINFO structure. Otherwise, the first time ShowWindow .. ,
        /// the value should be the value obtained by the WinMain function ..
        /// nCmdShow parameter. In subsequent calls, this parameter ..
        /// the WindowShowStyle members.</param>
        /// <returns>
        /// If the window was previously visible, the return value is nonzero.
        /// If the window was previously hidden, the return value is zero.
        /// </returns>
        [DllImport("user32.dll")]
        internal static extern bool ShowWindow(IntPtr hWnd,
            WindowShowStyle nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        // Import SetThreadExecutionState Win32 API and all flags
        [DllImport("kernel32.dll")]
        public static extern uint SetThreadExecutionState(uint esFlags);

        public const uint ES_SYSTEM_REQUIRED = 0x00000001;
        public const uint ES_DISPLAY_REQUIRED = 0x00000002;
        public const uint ES_USER_PRESENT = 0x00000004;
        public const uint ES_AWAYMODE_REQUIRED = 0x00000040;
        public const uint ES_CONTINUOUS = 0x80000000;
    }

    /// <summary>
    /// Enumeration of the different ways of showing a window.</summary>
    internal enum WindowShowStyle : uint
    {
        /// <summary>Hides the window and activates another window.</summary>
        /// <remarks>See SW_HIDE</remarks>
        Hide = 0,
        /// <summary>Activates and displays a window. If the window ..
        /// or maximized, the system restores it to its original size and
        /// position. An application should specify this flag when displaying
        /// the window for the first time.</summary>
        /// <remarks>See SW_SHOWNORMAL</remarks>
        ShowNormal = 1,
        /// <summary>Activates the window and displays it ..</summary>
        /// <remarks>See SW_SHOWMINIMIZED</remarks>
        ShowMinimized = 2,
        /// <summary>Activates the window and displays it ..</summary>
        /// <remarks>See SW_SHOWMAXIMIZED</remarks>
        ShowMaximized = 3,
        /// <summary>Maximizes the specified window.</summary>
        /// <remarks>See SW_MAXIMIZE</remarks>
        Maximize = 3,
        /// <summary>Displays a window in its most recent size and position.
        /// This value is similar to "ShowNormal", except the window is not
        /// actived.</summary>
        /// <remarks>See SW_SHOWNOACTIVATE</remarks>
        ShowNormalNoActivate = 4,
        /// <summary>Activates the window and displays it in its current size
        /// and position.</summary>
        /// <remarks>See SW_SHOW</remarks>
        Show = 5,
        /// <summary>Minimizes the specified window and activates the next
        /// top-level window in the Z order.</summary>
        /// <remarks>See SW_MINIMIZE</remarks>
        Minimize = 6,
        /// <summary>Displays the window as a minimized window. This value is
        /// similar to "ShowMinimized", except the window ..</summary>
        /// <remarks>See SW_SHOWMINNOACTIVE</remarks>
        ShowMinNoActivate = 7,
        /// <summary>Displays the window in its current size and position. This
        /// value is similar to "Show", except the window ..</summary>
        /// <remarks>See SW_SHOWNA</remarks>
        ShowNoActivate = 8,
        /// <summary>Activates and displays the window. If the window is
        /// minimized or maximized, the system restores it to its original size
        /// and position. An application should specify this flag ..
        /// a minimized window.</summary>
        /// <remarks>See SW_RESTORE</remarks>
        Restore = 9,
        /// <summary>Sets the show state based on the SW_ value specified ..
        /// STARTUPINFO structure passed to the CreateProcess function by the
        /// program that started the application.</summary>
        /// <remarks>See SW_SHOWDEFAULT</remarks>
        ShowDefault = 10,
        /// <summary>Windows 2000/XP: Minimizes a window, even if the thread
        /// that owns the window is hung. This flag should only be used when
        /// minimizing windows from a different thread.</summary>
        /// <remarks>See SW_FORCEMINIMIZE</remarks>
        ForceMinimized = 11
    }

}
