using Azure.ScannerTestJig.ViewModule;
using Azure.ScannerTestJig.ViewModule._532LaserModel;
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

namespace Azure.ScannerTestJig.View._532LaserModel
{
    /// <summary>
    /// WaveView532LaserSubWind.xaml 的交互逻辑
    /// </summary>
    public partial class WaveView532LaserSubWind : Window
    {
        public WaveView532LaserSubWind()
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

            WaveViewChart532LaserViewModel vm = Workspace.This.WaveViewChart532LaserViewModel;
            if (vm != null)
            {
                this.DataContext = vm;
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Workspace.This.WaveViewChart532LaserViewModel.Laser532ModelViewPort != null)
            {
                Workspace.This.WaveViewChart532LaserViewModel.Laser532ModelViewPort.Dispose();
                Workspace.This.WaveViewChart532LaserViewModel.DataProcessThread.Abort();
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
            //    COMNumberCoeffComboBox.IsEnabled = false;
            //    jiguangWaveButton2.IsEnabled = false;
            //    jiguangWave1Button2.IsEnabled = false;
            //    jiguangWaveButton.IsEnabled = false;
            //    jiguangWave1Button.IsEnabled = false;
            //    jiguangWaveButton1.IsEnabled = false;
            //    jiguangWave1Button1.IsEnabled = false;
            //    jiguangWaveButton3.IsEnabled = false;
            //    jiguangWave1Button3.IsEnabled = false;
            //    COMNumberCoeffComboBox1.IsEnabled = false;
            //    jiguangWaveButton4.IsEnabled = false;
            //    jiguangWave1Button4.IsEnabled = false;
            //}
            //else
            //{
            //    StartWaveButton.Content = "开始";
            //    StartWaveButton.Foreground = Brushes.White;
            //    COMNumberCoeffComboBox.IsEnabled = true;
            //    jiguangWaveButton2.IsEnabled = true;
            //    jiguangWave1Button2.IsEnabled = true;
            //    jiguangWaveButton.IsEnabled = true;
            //    jiguangWave1Button.IsEnabled = true;
            //    jiguangWaveButton1.IsEnabled = true;
            //    jiguangWave1Button1.IsEnabled = true;
            //    jiguangWaveButton3.IsEnabled = true;
            //    jiguangWave1Button3.IsEnabled = true;
            //    COMNumberCoeffComboBox1.IsEnabled = true;
            //    jiguangWaveButton4.IsEnabled = true;
            //    jiguangWave1Button4.IsEnabled = true;
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
