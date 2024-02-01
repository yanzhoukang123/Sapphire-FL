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
    /// MultiChannelLaserCalibrationSub.xaml 的交互逻辑
    /// </summary>
    public partial class MultiChannelLaserCalibrationSub : Window
    {
        public MultiChannelLaserCalibrationSub()
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
        public bool SavePng(string Fristname,string lastname)
        {
            Fit();
            if (Workspace.This.ViewModuleChartMultiChannelLaserCailbration.CH1Check)
            {
                charttt.TabCH1.Focus();
                charttt.TabCH1.UpdateLayout();
                this.charttt._WaveChartCH1.SaveScreenshot(Fristname+"_CH1_"+ Workspace.This.ViewModuleChartMultiChannelLaserCailbration.CH1Txt+ "_658_" + lastname);
            }
            if (Workspace.This.ViewModuleChartMultiChannelLaserCailbration.CH2Check)
            {
                charttt.TabCH2.Focus();
                charttt.TabCH2.UpdateLayout();
                this.charttt._WaveChartCH2.SaveScreenshot(Fristname + "_CH2_" + Workspace.This.ViewModuleChartMultiChannelLaserCailbration.CH2Txt + "_685_" + lastname);
            }
            if (Workspace.This.ViewModuleChartMultiChannelLaserCailbration.CH3Check)
            {
                charttt.TabCH3.Focus();
                charttt.TabCH3.UpdateLayout();
                this.charttt._WaveChartCH3.SaveScreenshot(Fristname + "_CH3_" + Workspace.This.ViewModuleChartMultiChannelLaserCailbration.CH3Txt + "_784_" + lastname);
            }
            if (Workspace.This.ViewModuleChartMultiChannelLaserCailbration.CH4Check)
            {
                charttt.TabCH4.Focus();
                charttt.TabCH4.UpdateLayout();
                this.charttt._WaveChartCH4.SaveScreenshot(Fristname + "_CH4_" + Workspace.This.ViewModuleChartMultiChannelLaserCailbration.CH4Txt + "_488_" + lastname);
            }
            if (Workspace.This.ViewModuleChartMultiChannelLaserCailbration.CH5Check)
            {
                charttt.TabCH5.Focus();
                charttt.TabCH5.UpdateLayout();
                this.charttt._WaveChartCH5.SaveScreenshot(Fristname + "_CH5_" + Workspace.This.ViewModuleChartMultiChannelLaserCailbration.CH5Txt + "_638_" + lastname);
            }
            if (Workspace.This.ViewModuleChartMultiChannelLaserCailbration.CH6Check)
            {
                charttt.TabCH6.Focus();
                charttt.TabCH6.UpdateLayout();
                this.charttt._WaveChartCH6.SaveScreenshot(Fristname + "_CH6_" + Workspace.This.ViewModuleChartMultiChannelLaserCailbration.CH6Txt + "_450_" + lastname);
            }
            if (Workspace.This.ViewModuleChartMultiChannelLaserCailbration.CH7Check)
            {
                charttt.TabCH7.Focus();
                charttt.TabCH7.UpdateLayout();
                this.charttt._WaveChartCH7.SaveScreenshot(Fristname + "_CH7_" + Workspace.This.ViewModuleChartMultiChannelLaserCailbration.CH7Txt + "_730_" + lastname);
            }
            return true;
        }
        public void Fit()
        {
            this.charttt._WaveChartCH1.FitToView();
            this.charttt._WaveChartCH2.FitToView();
            this.charttt._WaveChartCH3.FitToView();
            this.charttt._WaveChartCH4.FitToView();
            this.charttt._WaveChartCH5.FitToView();
            this.charttt._WaveChartCH6.FitToView();
            this.charttt._WaveChartCH7.FitToView();

        }
        private void Window_Closed(object sender, EventArgs e)
        {
            Workspace.This.Owner.Show();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Workspace.This.ViewModuleChartMultiChannelLaserCailbration.Laser532ModelViewPort != null)
            {
                Workspace.This.ViewModuleChartMultiChannelLaserCailbration.Laser532ModelViewPort.Dispose();
                Workspace.This.ViewModuleChartMultiChannelLaserCailbration.DataProcessThread.Abort();
            }
        }

        private void StartWaveButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
