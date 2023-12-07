using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace Azure.ScannerEUI.View
{
    /// <summary>
    /// PixelMove.xaml 
    /// </summary>
    public partial class PixelMove : UserControl
    {
        public PixelMove()
        {
            InitializeComponent();
        }
        private void btnPlus_txt2_Click(object sender, RoutedEventArgs e)
        {
            int Result = 0;
            if (int.TryParse(txtBox2.Text, out Result))
            {
                txtBox2.Text = (Result + 1).ToString();
            }
           
        }

        private void btnMinus_txt2_Click(object sender, RoutedEventArgs e)
        {
            int Result = 0;
            if (int.TryParse(txtBox2.Text, out Result))
            {
                txtBox2.Text = (Result - 1).ToString();
            }

        }
        private void btnPlus_txt3_Click(object sender, RoutedEventArgs e)
        {
            int Result = 0;
            if (int.TryParse(txtBox3.Text, out Result))
            {
                txtBox3.Text = (Result + 1).ToString();
            }
        }

        private void btnMinus_txt3_Click(object sender, RoutedEventArgs e)
        {
            int Result = 0;
            if (int.TryParse(txtBox3.Text, out Result))
            {
                txtBox3.Text = (Result - 1).ToString();
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
