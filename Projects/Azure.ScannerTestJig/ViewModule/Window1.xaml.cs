using Azure.Configuration.Settings;
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

namespace Azure.ScannerTestJig.ViewModule
{
    /// <summary>
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
        }
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            //if (CM250.IsChecked == true)
            //{
            //    CM250.IsChecked = false;
            //}
            //else
            //{
            CM250.IsChecked = true;
            Workspace.This.IsSelectGlassCM = false;
            SettingsManager.ConfigSettings.XMaxValue = 45833;  //330
            this.Hide();
            //}
        }

        private void RadioButton_Checked_1(object sender, RoutedEventArgs e)
        {
            //if (CM500.IsChecked == true)
            //{
            //    CM500.IsChecked = false;
            //}
            //else
            //{
            Workspace.This.IsSelectGlassCM = true;
            SettingsManager.ConfigSettings.XMaxValue = 79167;  //570
            CM500.IsChecked = true;
            this.Hide();
            //}
        }
    }
}
