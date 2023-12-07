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
using System.Windows.Shapes;
using System.Text.RegularExpressions;   // Regex
using Azure.ScannerEUI.ViewModel;

namespace Azure.ScannerEUI.View
{
    /// <summary>
    /// Interaction logic for ParameterSetup.xaml
    /// </summary>
    public partial class ParameterSetup : Window
    {
        public ParameterSetup()
        {
            InitializeComponent();
            DataContext = Workspace.This.ParameterVM;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ParameterSetupViewModel viewModel = DataContext as ParameterSetupViewModel;
            if (viewModel != null)
            {
                viewModel.Initialize();
            }

            if (Workspace.This.ParameterVM != null)
                Workspace.This.ParameterVM.ExecuteParametersReadCommand(null);      // automatically read parameters before opening the window
        }

        private static bool IsTextAllowed(string text)
        {
            Regex regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
            return !regex.IsMatch(text);
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

    }
}
