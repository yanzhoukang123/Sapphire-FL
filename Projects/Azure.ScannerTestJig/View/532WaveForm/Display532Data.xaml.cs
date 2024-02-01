using Azure.ScannerTestJig.ViewModule;
using Azure.ScannerTestJig.ViewModule._532WaveForm;
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

namespace Azure.ScannerTestJig.View._532WaveForm
{
    /// <summary>
    /// Display532Data.xaml 的交互逻辑
    /// </summary>
    public partial class Display532Data : Window
    {
        public Display532Data()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(IVContorl_Loaded);
        }
        void IVContorl_Loaded(object sender, EventArgs e)
        {

            WaveViewChartViewModel vm = Workspace.This.WaveViewChartViewModel;
            if (vm != null)
            {
                this.DataContext = vm;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (Workspace.This.WaveViewChartViewModel.HistoryThread.IsAlive)
            {
                Workspace.This.WaveViewChartViewModel.HistoryThread.Abort();
            }
        }
    }
}
