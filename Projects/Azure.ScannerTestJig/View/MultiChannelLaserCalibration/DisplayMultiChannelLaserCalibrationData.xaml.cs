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
    /// DisplayMultiChannelLaserCalibrationData.xaml 的交互逻辑
    /// </summary>
    public partial class DisplayMultiChannelLaserCalibrationData : Window
    {
        public DisplayMultiChannelLaserCalibrationData()
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

        private void Window_Closed(object sender, EventArgs e)
        {
            if (Workspace.This.ViewModuleChartMultiChannelLaserCailbration.HistoryThread.IsAlive)
            {
                Workspace.This.ViewModuleChartMultiChannelLaserCailbration.HistoryThread.Abort();
            }
        }
    }
}
