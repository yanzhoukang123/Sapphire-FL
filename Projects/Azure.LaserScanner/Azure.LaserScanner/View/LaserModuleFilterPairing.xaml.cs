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
    /// Interaction logic for LaserModuleFilterPairing.xaml
    /// </summary>
    public partial class LaserModuleFilterPairing : Window
    {
        public LaserModuleFilterPairing()
        {
            InitializeComponent();
        }

        public string InfoText
        {
            get
            {
                if (infoTextBlock != null)
                {
                    return (string)infoTextBlock.Text;
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                if (infoTextBlock != null)
                {
                    infoTextBlock.Text = value;
                }
            }
        }

        private void _OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext != null && DataContext is LaserModuleFilterPairingViewModel)
            {
                LaserModuleFilterPairingViewModel viewModel = DataContext as LaserModuleFilterPairingViewModel;
                if (viewModel != null)
                {
                    if (viewModel.HasLasersAndFiltersPaired)
                    {
                        this.DialogResult = true;
                    }
                    else
                    {
                        string caption = "Laser and Filter Pairing";
                        string message = "";
                        if (viewModel.LaserModuleL1 != null && viewModel.SelectedFilterL1 == null)
                        {
                            message = string.Format("Please select the filter installed on the {0} laser module.", viewModel.LaserModuleL1.LaserWavelength);
                            Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        else if (viewModel.LaserModuleR1 != null && viewModel.SelectedFilterR1 == null)
                        {
                            message = string.Format("Please select the filter installed on the {0} laser module.", viewModel.LaserModuleR1.LaserWavelength);
                            Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        else if (viewModel.LaserModuleR2 != null && viewModel.SelectedFilterR2 == null)
                        {
                            message = string.Format("Please select the filter installed on the {0} laser module.", viewModel.LaserModuleR2.LaserWavelength);
                            Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
            }
        }

        private void _Cancel_Click(object sender, RoutedEventArgs e)
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
