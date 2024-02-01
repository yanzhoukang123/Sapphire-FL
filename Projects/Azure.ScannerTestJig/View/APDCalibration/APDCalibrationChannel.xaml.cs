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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Azure.ScannerTestJig.View.APDCalibration
{
    /// <summary>
    /// APDCalibrationChannel.xaml 的交互逻辑
    /// </summary>
    public partial class APDCalibrationChannel : UserControl
    {
        public APDCalibrationChannel()
        {
            InitializeComponent();
            DataContext = Workspace.This.APDCalibrationChannelAVM;
            dataGrid.DataContext = Workspace.This.APDCalibrationChannelAVM.GainOptions;
        }
        private void CalibrationVoltBox_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox txt = sender as TextBox;
            if ((e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) || e.Key == Key.Decimal)
            {
                if (txt.Text.Contains(".") && e.Key == Key.Decimal)
                {
                    e.Handled = true;
                    return;
                }
                e.Handled = false;
            }
            else if (((e.Key >= Key.D0 && e.Key <= Key.D9) || e.Key == Key.OemPeriod) && e.KeyboardDevice.Modifiers != ModifierKeys.Shift)
            {
                if (txt.Text.Contains(".") && e.Key == Key.OemPeriod)
                {
                    e.Handled = true;
                    return;
                }
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void CalibrationTempBox_KeyDown(object sender, KeyEventArgs e)
        {
            CalibrationVoltBox_KeyDown(sender, e);
        }

    }
}
