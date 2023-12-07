using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;   // Regex
using Azure.Image.Processing;
//using Azure.WindowsAPI.Interop;
using Azure.LaserScanner.ViewModel;

namespace Azure.LaserScanner
{
    /// <summary>
    /// Interaction logic for LaserModuleChange.xaml
    /// </summary>
    public partial class LaserModuleChange : Window
    {
        public LaserModuleChange()
        {
            InitializeComponent();
        }

        private void _StartButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void _CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
    }
}
