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
    /// WaveViewSubWind.xaml 的交互逻辑
    /// </summary>
    public partial class WaveViewSubWind : Window
    {
        public WaveViewSubWind()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(IVContorl_Loaded);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Workspace.This.Owner.Show();
        }

        void IVContorl_Loaded(object sender, EventArgs e)
        {

            WaveViewChartViewModel vm = Workspace.This.WaveViewChartViewModel;
            if (vm != null)
            {
                this.DataContext = vm;
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Workspace.This.WaveViewChartViewModel.Laser532WaveViewPort != null)
            {
                Workspace.This.WaveViewChartViewModel.Laser532WaveViewPort.Dispose();
                Workspace.This.WaveViewChartViewModel.DataProcessThread.Abort();
            }
        }

        private void StartWaveButton_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
          
        }

        private void StartWaveButton_Click(object sender, RoutedEventArgs e)
        {
            //if (StartWaveButton.Content.ToString() == "开始")
            //{
            //    StartWaveButton.Content = "停止";
            //    StartWaveButton.Foreground = Brushes.Red;
            //}
            //else
            //{
            //    StartWaveButton.Content = "开始";
            //    StartWaveButton.Foreground = Brushes.White;
            //}
        }
        public void SavePng(string name)
        {
            this.charttt._WaveChart.FitToView();
            this.charttt._WaveChart.UpdateLayout();
            this.charttt._WaveChart.SaveScreenshot(name);

        }
    }
}
