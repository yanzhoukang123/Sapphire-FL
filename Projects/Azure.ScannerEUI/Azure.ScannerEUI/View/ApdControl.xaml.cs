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
using Azure.ScannerEUI.ViewModel;

namespace Azure.ScannerEUI.View
{
    /// <summary>
    /// Interaction logic for ApdControl.xaml
    /// </summary>
    public partial class ApdControl : UserControl
    {
        public ApdControl()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(ApdControl_Loaded);
        }

        void ApdControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApdViewModel vm = Workspace.This.ApdVM;
            if (vm != null)
            {
                vm.InitApdControls();
            }
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

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                TextBox tBox = (TextBox)sender;
                DependencyProperty prop = TextBox.TextProperty;

                BindingExpression binding = BindingOperations.GetBindingExpression(tBox, prop);
                if (binding != null)
                {
                    binding.UpdateSource();
                }
            }
        }

    }
}
