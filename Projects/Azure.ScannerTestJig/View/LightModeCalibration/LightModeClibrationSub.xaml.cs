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

namespace Azure.ScannerTestJig.View.LightModeCalibration
{
    /// <summary>
    /// LightModeClibrationSub.xaml 的交互逻辑
    /// </summary>
    public partial class LightModeClibrationSub : Window
    {
        public LightModeClibrationSub()
        {
            InitializeComponent();
            Workspace.This.Owner.Visibility = Visibility.Hidden;
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            //if (Workspace.This.serialState)
            //{
            //    Workspace.This.SerialPorts.Close();
            //}
            Workspace.This.Owner.Show();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Workspace.This.LightModeCaliVM.LightModeSettingsPort != null)
            {
                Workspace.This.LightModeCaliVM.LightModeSettingsPort.Dispose();
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.H)
            {
                Login login = new Login();
                login.ShowDialog();
            }
        }
    }

}
