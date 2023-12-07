using Azure.LaserScanner.ViewModel;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
//using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
//using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Azure.LaserScanner.View
{
    /// <summary>
    /// Interaction logic for UserManual.xaml
    /// </summary>
    public partial class UserManual : UserControl
    {
        //private string myHTMLContent = "Azure Imaging Systems Manual.htm";
        string userManualFileName = "Sapphire_FL_User_Manual.pdf";
        //string userManualUrl = @"https://azurebiosystems.com/wp-content/uploads/2023/07/Azure-Imaging-Systems-User-Manual.pdf";
        //string userManualUrl21CFR = @"https://azurebiosystems.com/wp-content/uploads/2023/07/AIS-21CFR11-Capture-Software-User-Manual_2.pdf";
        //string userManualUrl = @"https://azurebiosystems.com/AISmanual";
        //string userManualUrl21CFR = @"https://azurebiosystems.com/AISmanual/21CFR";

        public UserManual()
        {
            InitializeComponent();
            //this.Loaded += UserManual_Loaded;
            // Solution 2:
            //webBrowserHelp.Navigated += new NavigatedEventHandler(webBrowserHelp_Navigated);

            this.Loaded += (s, e) =>
            {
                var viewModel = DataContext as Workspace;
                if (viewModel != null)
                {
                    if (viewModel.IsAdobeAcrobatInstalled)
                    {
                        var userManualLocalPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UserManual", userManualFileName);
                        if (File.Exists(userManualLocalPath))
                        {
                            //double height = _MainTabControl.ActualHeight - _HeaderStackPanel.ActualHeight - 10.0;
                            //webBrowserHelp.Height = height;
                            webBrowserHelp.Navigate(userManualLocalPath);
                        }
                        //else
                        //{
                        //    string manualUrl = userManualUrl;
                        //    if (viewModel.Owner.ImagingSysSettings.IsQcVersion)
                        //    {
                        //        manualUrl = userManualUrl21CFR;
                        //    }
                        //    webBrowserHelp.Navigate(manualUrl);
                        //}
                    }
                    /*else
                    {
                        //string target = "http://www.microsoft.com";
                        //Use no more than one assignment when you test this code.
                        //string target = "ftp://ftp.microsoft.com";
                        //string target = "C:\\Program Files\\Microsoft Visual Studio\\INSTALL.HTM";
                        try
                        {
                            System.Diagnostics.Process.Start(myWebUserManual);
                        }
                        catch (System.ComponentModel.Win32Exception noBrowser)
                        {
                            if (noBrowser.ErrorCode == -2147467259)
                            {
                                Console.WriteLine(noBrowser.Message);
                            }
                        }
                        catch (System.Exception other)
                        {
                            Console.WriteLine(other.Message);
                        }
                    }*/
                }
            };

            this.webBrowserHelp.Loaded += (s, e) =>
            {
                // get the underlying WebBrowser ActiveX object;
                // this code depends on SHDocVw.dll COM interop assembly,
                // generate SHDocVw.dll: "tlbimp.exe ieframe.dll",
                // and add as a reference to the project

                // Ignore ActiveX script error
                //
                dynamic activeX = this.webBrowserHelp.GetType().InvokeMember("ActiveXInstance",
                    BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                    null, this.webBrowserHelp, new object[] { });

                if (activeX != null)
                {
                    activeX.Silent = true;
                }
            };
        }

        //private void ViewModel_LoadUserManual(object sender)
        //{
        //    var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UserManual\\" + myHTMLContent);
        //    if (File.Exists(path))
        //    {
        //        if (webBrowserHelp.CanGoBack)
        //            webBrowserHelp.Navigate(path);
        //    }
        //}

        /*private void UserManual_Loaded(object sender, RoutedEventArgs e)
        {
            var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UserManual\\" + myHTMLContent);
            if (File.Exists(path))
            {
                //double height = _MainTabControl.ActualHeight - _HeaderStackPanel.ActualHeight - 10.0;
                //webBrowserHelp.Height = height;
                webBrowserHelp.Navigate(path);

                dynamic activeX = this.webBrowserHelp.GetType().InvokeMember("ActiveXInstance",
                    BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                    null, this.webBrowserHelp, new object[] { });

                // Ignore ActiveX script error
                activeX.Silent = true;
            }
        }*/

        // NOTE: Solution 2:
        //void webBrowserHelp_Navigated(object sender, NavigationEventArgs e)
        //{
        //    SetSilent(webBrowserHelp, true); // make it silent
        //}

        // NOTE: Solution 2:
        //public static void SetSilent(WebBrowser browser, bool silent)
        //{
        //    if (browser == null)
        //        throw new ArgumentNullException("browser");
        //
        //    // get an IWebBrowser2 from the document
        //    IOleServiceProvider sp = browser.Document as IOleServiceProvider;
        //    if (sp != null)
        //    {
        //        Guid IID_IWebBrowserApp = new Guid("0002DF05-0000-0000-C000-000000000046");
        //        Guid IID_IWebBrowser2 = new Guid("D30C1661-CDAF-11d0-8A3E-00C04FC9E26E");

        //        object webBrowser;
        //        sp.QueryService(ref IID_IWebBrowserApp, ref IID_IWebBrowser2, out webBrowser);
        //        if (webBrowser != null)
        //        {
        //            webBrowser.GetType().InvokeMember("Silent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.PutDispProperty, null, webBrowser, new object[] { silent });
        //        }
        //    }
        //}

        // NOTE: Solution 2:
        //[ComImport, Guid("6D5140C1-7436-11CE-8034-00AA006009FA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        //private interface IOleServiceProvider
        //{
        //    [PreserveSig]
        //    int QueryService([In] ref Guid guidService, [In] ref Guid riid, [MarshalAs(UnmanagedType.IDispatch)] out object ppvObject);
        //}
    }
}
