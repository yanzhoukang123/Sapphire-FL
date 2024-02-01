using Azure.ScannerTestJig.ViewModule;
using Azure.ScannerTestJig.ViewModule.MultiChannelLaserCalibration;
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
using System.Windows.Shapes;

namespace Azure.ScannerTestJig.View.MultiChannelLaserCalibration
{
    /// <summary>
    /// MutiChannelLaserCalibrationSetting.xaml 的交互逻辑
    /// </summary>
    public partial class MutiChannelLaserCalibrationSetting : Window
    {
        public MutiChannelLaserCalibrationSetting()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(IVContorl_Loaded);
        }
        void IVContorl_Loaded(object sender, EventArgs e)
        {

            ViewModuleChartMultiChannelLaserCailbration vm = Workspace.This.ViewModuleChartMultiChannelLaserCailbration;
            if (vm != null)
            {
                this.DataContext = vm;
            }
        }

        private void jiguangWaveButton55_Click(object sender, RoutedEventArgs e)
        {
            Workspace.This.ViewModuleChartMultiChannelLaserCailbration.IsSettingClose = true;
            this.Hide();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Workspace.This.ViewModuleChartMultiChannelLaserCailbration.IsSettingClose = false;
            this.Close();
        }
    }
}
