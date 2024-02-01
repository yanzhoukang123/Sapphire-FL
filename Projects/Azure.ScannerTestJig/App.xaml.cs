using Azure.Configuration.Settings;
using Azure.ScannerTestJig.ViewModule;
using Azure.WPF.Framework;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;

namespace Azure.ScannerTestJig
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {



        protected override void OnStartup(StartupEventArgs e)
        {

                base.OnStartup(e);

                #region Get product information from assembly...
                string companyName = string.Empty;
                string productName = "Sapphire";
                object[] customAttributes = null;
                System.Windows.FrameworkCompatibilityPreferences.KeepTextBoxDisplaySynchronizedWithTextProperty = false;
                try
                {
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
                            productName = "Sapphire";
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
                CommonApplicationData mastersAppData = new CommonApplicationData(companyName, productName + "\\Masters", true);
                //CommonApplicationData protocolsAppData = new CommonApplicationData(companyName, productName + "\\Protocols", true);
                //CommonApplicationData protocolsCustomAppData = new CommonApplicationData(companyName, productName + "\\Protocols\\Custom", true);
                #endregion

                //string appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                //SettingsManager.ApplicationPath = appPath;
                SettingsManager.ApplicationDataPath = commonAppDataPath;
                SettingsManager.IsEngrUI = true;
                SettingsManager.OnStartup();    // load configuration settings

                // Version format: major.minor.build.revision
                Version version = Assembly.GetExecutingAssembly().GetName().Version;
                string productVersion = string.Format("{0}.{1}.{2}.{3}",
                                                      version.Major,
                                                      version.Minor,
                                                      version.Build,
                                                      version.Revision.ToString("D4"));

                Workspace.This.ProductVersion = productVersion;

  

        }
    } 
            
}
