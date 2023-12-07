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
    /// NewParameterSetup.xaml 
    /// </summary>
    public partial class NewParameterSetup : Window
    {
        public NewParameterSetup()
        {
            InitializeComponent();
            DataContext = Workspace.This.NewParameterVM;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            NewParameterSetupViewModel viewModel = DataContext as NewParameterSetupViewModel;
            if (viewModel != null)
            {
                viewModel.Initialize();
            }
            //窗体加载时加载一般个性化参数
            //The general personalization parameters are loaded when the form loads
            if (Workspace.This.NewParameterVM != null)
                Workspace.This.NewParameterVM.ExecuteParametersReadCommand(null);  
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
