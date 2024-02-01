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
    /// PDCalWaveView532LaserSubWind.xaml 的交互逻辑
    /// </summary>
    public partial class PDCalWaveView532LaserSubWind : Window
    {
        public PDCalWaveView532LaserSubWind()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(IVContorl_Loaded);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Workspace.This.Owner.Show();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Workspace.This.PDCalWaveViewChart532LaserViewModel.Laser532ModelViewPort != null)
            {
                Workspace.This.PDCalWaveViewChart532LaserViewModel.Laser532ModelViewPort.Dispose();
                Workspace.This.PDCalWaveViewChart532LaserViewModel.DataProcessThread.Abort();
            }
        }
        void IVContorl_Loaded(object sender, EventArgs e)
        {

            PDCalWaveViewChart532LaserViewModel vm = Workspace.This.PDCalWaveViewChart532LaserViewModel;
            if (vm != null)
            {
                this.DataContext = vm;
            }
        }
        public void SavePng(string name)
        {
            this.charttt._WaveChart.FitToView();
            this.charttt._WaveChart.UpdateLayout();
            this.charttt._WaveChart.SaveScreenshot(name);

        }
    }
}
