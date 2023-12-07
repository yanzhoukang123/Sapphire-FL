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

namespace Azure.LaserScanner.View
{
    /// <summary>
    /// Interaction logic for MultipleExposureControl.xaml
    /// </summary>
    public partial class ZScanControl : UserControl
    {
        public ZScanControl()
        {
            InitializeComponent();
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

        /// <summary>
        /// Allow numeric and only 2 decimals places
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            //const int KEYCODE_Dot_OnKeyboard = 190;
            //const int KEYCODE_Dot_OnNumericKeyPad = 110;

            // Do not allow special characters
            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                e.Handled = true;
            }

            // Only allow numbers and decimal
            if ((e.Key >= Key.D0 && e.Key <= Key.D9 ||
                    e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9 ||
                    e.Key == Key.Decimal || e.Key == Key.OemPeriod))
            {
                string strkey = e.Key.ToString().Substring(e.Key.ToString().Length - 1,
                        e.Key.ToString().Length - (e.Key.ToString().Length - 1));

                if (e.Key >= Key.D0 && e.Key <= Key.D9 ||
                    e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
                {
                    // Do not allow more than 2 digits after decimal point
                    TextBox tb = sender as TextBox;
                    int cursorPosLeft = tb.SelectionStart;
                    int cursorPosRight = tb.SelectionStart + tb.SelectionLength;
                    string result1 = tb.Text.Substring(0, cursorPosLeft) +
                          strkey + tb.Text.Substring(cursorPosRight);
                    string[] parts = result1.Split('.');
                    if (parts.Length > 1)
                    {
                        if (parts[1].Length > 2 || parts.Length > 2)
                        {
                            e.Handled = true;
                        }
                    }
                }

                // Do not allow multiple decimal points
                if (((TextBox)sender).Text.Contains(".") && (e.Key == Key.Decimal || e.Key == Key.OemPeriod))
                {
                    e.Handled = true;
                }
            }
            else
            {
                if (e.Key != Key.Tab && e.Key != Key.OemBackTab)
                    e.Handled = true;
            }

            // Do not allow alphabets and space
            if (e.Key >= Key.A && e.Key <= Key.Z || e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }
    }
}
