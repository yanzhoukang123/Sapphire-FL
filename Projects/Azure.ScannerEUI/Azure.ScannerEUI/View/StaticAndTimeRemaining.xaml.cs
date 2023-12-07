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
using System.Text.RegularExpressions;   // Regex
using System.Windows.Shapes;

namespace Azure.ScannerEUI.View
{
    /// <summary>
    /// StaticAndTimeRemaining.xaml 的交互逻辑
    /// </summary>
    public partial class StaticAndTimeRemaining : UserControl
    {
        public StaticAndTimeRemaining()
        {
            InitializeComponent();
        }
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }
        private static bool IsTextAllowed(string text)
        {
            Regex regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
            return !regex.IsMatch(text);
        }

        private void Line_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
