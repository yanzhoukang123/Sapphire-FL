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
    /// Interaction logic for CameraModePanel.xaml
    /// </summary>
    public partial class CameraModePanel : UserControl
    {
        public CameraModePanel()
        {
            InitializeComponent();
         
            //this.Loaded += CameraModePanel_Loaded;
        }

        /*private void CameraModePanel_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext != null)
            {
                if (this.DataContext is CameraViewModel)
                {
                    if (((CameraViewModel)this.DataContext).BinningOptions != null &&
                        ((CameraViewModel)this.DataContext).BinningOptions.Count > 0)
                    {
                        ((CameraViewModel)this.DataContext).SelectedBinning = ((CameraViewModel)this.DataContext).BinningOptions[0];
                    }
                    if (((CameraViewModel)this.DataContext).GainOptions != null &&
                        ((CameraViewModel)this.DataContext).GainOptions.Count > 0)
                    {
                        ((CameraViewModel)this.DataContext).SelectedGain = ((CameraViewModel)this.DataContext).GainOptions[0];
                    }
                    if (((CameraViewModel)this.DataContext).ReadoutOptions != null &&
                        ((CameraViewModel)this.DataContext).ReadoutOptions.Count > 0)
                    {
                        ((CameraViewModel)this.DataContext).SelectedReadout = ((CameraViewModel)this.DataContext).ReadoutOptions[0];
                    }
                }
            }
        }*/

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
