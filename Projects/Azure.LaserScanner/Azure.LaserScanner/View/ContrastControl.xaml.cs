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
using Azure.LaserScanner.ViewModel;

namespace Azure.LaserScanner.View
{
    /// <summary>
    /// Interaction logic for ContrastControl.xaml
    /// </summary>
    public partial class ContrastControl : UserControl
    {
        public ContrastControl()
        {
            InitializeComponent();
        }

        void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        static bool IsTextAllowed(string text)
        {
            Regex regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
            return !regex.IsMatch(text);
        }

        /*private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                TextBox textBox = (TextBox)sender;
                if (textBox != null)
                {
                    if (textBox == blackValueTextBox)
                    {
                        int blackVal = 0;
                        int.TryParse(textBox.Text, out blackVal);
                        var viewModel = (ContrastViewModel)DataContext;
                        if (viewModel != null)
                        {
                            viewModel.BlackValue = blackVal;
                        }
                    }
                    else if (textBox == whiteValueTextBox)
                    {
                        int whiteVal = 0;
                        int.TryParse(textBox.Text, out whiteVal);
                        var viewModel = (ContrastViewModel)DataContext;
                        if (viewModel != null)
                        {
                            viewModel.BlackValue = whiteVal;
                        }
                    }
                    else if (textBox == gammaValueTextBox)
                    {
                        double gammaVal = 0;
                        double.TryParse(textBox.Text, out gammaVal);
                        var viewModel = (ContrastViewModel)DataContext;
                        if (viewModel != null)
                        {
                            viewModel.GammaValue = gammaVal;
                        }
                    }
                }
            }
        }*/

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox != null)
            {
                if (textBox == blackValueTextBox)
                {
                    int blackVal = 0;
                    int.TryParse(textBox.Text, out blackVal);
                    ((ContrastViewModel)DataContext).BlackValue = blackVal;
                }
                else if (textBox == whiteValueTextBox)
                {
                    int whiteVal = 0;
                    int.TryParse(textBox.Text, out whiteVal);
                    ((ContrastViewModel)DataContext).WhiteValue = whiteVal;
                }
                else if (textBox == gammaValueTextBox)
                {
                    double gammaVal = 0;
                    double.TryParse(textBox.Text, out gammaVal);
                    ((ContrastViewModel)DataContext).GammaValue = gammaVal;
                }
            }
        }

        private void TextBox_KeyEnterUpdate(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox tBox = (TextBox)sender;
                DependencyProperty prop = TextBox.TextProperty;

                BindingExpression binding = BindingOperations.GetBindingExpression(tBox, prop);
                if (binding != null) { binding.UpdateSource(); }
            }
        }

    }
}
