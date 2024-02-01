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
    /// WaveViewNo532LaserSubWind.xaml 的交互逻辑
    /// </summary>
    public partial class WaveViewNo532LaserSubWind : Window
    {
        public WaveViewNo532LaserSubWind()
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
            if (Workspace.This.WaveViewChartNo532LaserViewModel.Laser532ModelViewPort != null)
            {
                Workspace.This.WaveViewChartNo532LaserViewModel.Laser532ModelViewPort.Dispose();
                Workspace.This.WaveViewChartNo532LaserViewModel.DataProcessThread.Abort();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Workspace.This.Owner.Show();
        }
        public void SavePng(string name)
        {
            this.charttt._WaveChart.FitToView();
            this.charttt._WaveChart.UpdateLayout();
            this.charttt._WaveChart.SaveScreenshot(name);

        }

        private void StartWaveButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
