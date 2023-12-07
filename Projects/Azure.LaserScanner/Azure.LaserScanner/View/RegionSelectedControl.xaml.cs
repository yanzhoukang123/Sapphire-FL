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

namespace Azure.LaserScanner.View
{
    /// <summary>
    /// Interaction logic for RegionSelectedControl.xaml
    /// </summary>
    public partial class RegionSelectedControl : UserControl
    {
        public RegionSelectedControl()
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

        /*private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox != null)
            {
                if (textBox == XValueTextBox)
                {
                    int xValue = 0;
                    int.TryParse(textBox.Text, out xValue);
                    //((ContrastViewModel)DataContext).BlackValue = blackVal;
                }
                else if (textBox == YValueTextBox)
                {
                    int yValue = 0;
                    int.TryParse(textBox.Text, out yValue);
                    //((ContrastViewModel)DataContext).WhiteValue = whiteVal;
                }
                else if (textBox == WidthValueTextBox)
                {
                    double widthValue = 0;
                    double.TryParse(textBox.Text, out widthValue);
                    //((ContrastViewModel)DataContext).GammaValue = gammaVal;
                }
                else if (textBox == HeightValueTextBox)
                {
                    double heightValue = 0;
                    double.TryParse(textBox.Text, out heightValue);
                    //((ContrastViewModel)DataContext).GammaValue = gammaVal;
                }
            }
        }*/

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
