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
    /// Interaction logic for ResizeControlPanel.xaml
    /// </summary>
    public partial class ResizeControlPanel : UserControl
    {
        //private bool _IsRaiseTextChangedEvent = true;

        public ResizeControlPanel()
        {
            InitializeComponent();
        }

        // Use the PreviewTextInputHandler to respond to key stroke
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private static bool IsTextAllowed(string text)
        {
            Regex regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
            return !regex.IsMatch(text);
        }

        // Use the DataObject.Pasting Handler to prevent pasting of incorrect data
        private void PastingHandler(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));
                if (!IsTextAllowed(text)) e.CancelCommand();
            }
            else e.CancelCommand();
        }

        //private void HeightTextBox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    var binding = ((TextBox)sender).GetBindingExpression(TextBox.TextProperty);
        //    binding.UpdateSource();
        //}
    }
}
