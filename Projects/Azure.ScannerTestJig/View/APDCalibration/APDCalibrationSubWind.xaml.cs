using Azure.ScannerTestJig.ViewModule;
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

namespace Azure.ScannerTestJig.View.APDCalibration
{
    /// <summary>
    /// APDCalibrationSubWind.xaml 的交互逻辑
    /// </summary>
    public partial class APDCalibrationSubWind : Window
    {

    public double sWidth = 0;
        public double sHight = 0;
        public APDCalibrationSubWind()
        {
            InitializeComponent();
            DataContext = Workspace.This.APDCalibrationVM;
            //ChannelA.DataContext = Workspace.This.APDCalibrationChannelAVM;
            //ChannelA.dataGrid.DataContext = Workspace.This.APDCalibrationChannelAVM.GainOptions;
            //ChannelB.DataContext = Workspace.This.APDCalibrationChannelBVM;
            //ChannelB.dataGrid.DataContext = Workspace.This.APDCalibrationChannelBVM.GainOptions;

            Workspace.This.APDCalibrationVM.ReadErrorCount = 0;
            Workspace.This.APDCalibrationVM.WriteErrorCount = 0;
            Workspace.This.Owner.Visibility = Visibility.Hidden;
            sWidth = rec.Width;
            sHight = rec.Height;
            // 在此点之下插入创建对象所需的代码。
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            if (Workspace.This.APDCalibrationVM.TestItemAPDCalibration.IsChecking)
            {
                string caption = "APD标定提示！";
                string message = "当前标定还在继续，关闭该窗口?";
                System.Windows.MessageBoxResult dlgResult = System.Windows.MessageBox.Show(message, caption, System.Windows.MessageBoxButton.YesNo);
                if (dlgResult == System.Windows.MessageBoxResult.No)
                {
                    e.Cancel = true; // don't allow the application to close
                    return;
                }

                Workspace.This.APDCalibrationVM.ExcuteStopCalibrationCommand(null);
                if (Workspace.This.APDCalibrationVM.APDCalibrationPort != null)
                {
                    Workspace.This.APDCalibrationVM.APDCalibrationPort.Dispose();
                    Workspace.This.APDCalibrationVM.APDCalibrationPort.TempIvDispose();
                }
                Workspace.This.Owner.Show();
            }
            else {

                Workspace.This.Owner.Show();
            }
            if (Workspace.This.APDCalibrationVM.APDCalibrationPort != null)
            {
                Workspace.This.APDCalibrationVM.APDCalibrationPort.Dispose();
                Workspace.This.APDCalibrationVM.APDCalibrationPort.TempIvDispose();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            //Workspace.This.Owner.Visibility = Visibility.Visible;
            //Workspace.This.Owner.Close();

        }
        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
         
            rec.Width = rec.Width - 2;
            rec.Height = rec.Height - 2;
        }
    }
}
