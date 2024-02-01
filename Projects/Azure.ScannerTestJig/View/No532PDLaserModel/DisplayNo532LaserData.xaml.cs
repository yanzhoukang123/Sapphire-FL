using Azure.ScannerTestJig.ViewModule;
using Azure.ScannerTestJig.ViewModule.No532PDLaserModel;
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

namespace Azure.ScannerTestJig.View.No532PDLaserModel
{
    /// <summary>
    /// Display532LaserData.xaml 的交互逻辑
    /// </summary>
    public partial class DisplayNo532LaserData : Window
    {
        public DisplayNo532LaserData()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(IVContorl_Loaded);
        }
        void IVContorl_Loaded(object sender, EventArgs e)
        {

            WaveViewChartNo532LaserViewModel vm = Workspace.This.WaveViewChartNo532LaserViewModel;
            if (vm != null)
            {
                this.DataContext = vm;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (Workspace.This.WaveViewChart532LaserViewModel.HistoryThread != null)
            {
                if (Workspace.This.WaveViewChart532LaserViewModel.HistoryThread.IsAlive)
                {
                    Workspace.This.WaveViewChart532LaserViewModel.HistoryThread.Abort();
                }
            }
        }
    }
}
