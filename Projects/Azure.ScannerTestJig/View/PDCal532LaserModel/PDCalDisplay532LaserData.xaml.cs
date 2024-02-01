using Azure.ScannerTestJig.ViewModule;
using Azure.ScannerTestJig.ViewModule.PDCal532LaserModel;
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

namespace Azure.ScannerTestJig.View.PDCal532LaserModel
{
    /// <summary>
    /// PDCalDisplay532LaserData.xaml 的交互逻辑
    /// </summary>
    public partial class PDCalDisplay532LaserData : Window
    {
        public PDCalDisplay532LaserData()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(IVContorl_Loaded);
        }
        void IVContorl_Loaded(object sender, EventArgs e)
        {

            PDCalWaveViewChart532LaserViewModel vm = Workspace.This.PDCalWaveViewChart532LaserViewModel;
            if (vm != null)
            {
                this.DataContext = vm;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (Workspace.This.PDCalWaveViewChart532LaserViewModel.HistoryThread.IsAlive)
            {
                Workspace.This.PDCalWaveViewChart532LaserViewModel.HistoryThread.Abort();
            }
        }
    }
}
