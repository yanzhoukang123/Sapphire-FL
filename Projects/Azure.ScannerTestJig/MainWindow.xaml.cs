using Azure.EthernetCommLib;
using Azure.Configuration.Settings;
using Azure.ScannerTestJig.ViewModule;
using Azure.ScannerTestJig.ViewModule.FocusAdjust;
using Azure.ScannerTestJig.ViewModule.GlassTopAdj;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Utilities;

namespace Azure.ScannerTestJig
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public string ProductVersion { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            //_WindowStateManager = new WindowStateManager(SettingsManager.ApplicationSettings.MainWindowStateInfo, this);
            Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            ProductVersion = string.Format("Scanner2.0 TestJig V{0}.{1}.{2}.{3}",
                version.Major,
                version.Minor,
                version.Build,
                version.Revision
                );
            this.Title +=ProductVersion;
            DataContext = Workspace.This;
            Workspace.This.Owner = this;
            
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (Workspace.This.PramaSetteVM.thread != null)
            {
                Workspace.This.PramaSetteVM.thread.Abort();
            }
            if (Workspace.This.GlassAdjustWindVM.tdHome != null)
            {
                Workspace.This.GlassAdjustWindVM.tdHome.Abort();
            }
            if (Workspace.This.TotalMachiVM.tdHome != null)
            {
                Workspace.This.TotalMachiVM.tdHome.Abort();
            }
           
            System.Environment.Exit(0);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
           
        }
    }
}
