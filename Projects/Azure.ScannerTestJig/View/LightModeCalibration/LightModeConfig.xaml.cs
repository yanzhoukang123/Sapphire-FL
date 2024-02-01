using Azure.ScannerTestJig.ViewModule;
using Azure.ScannerTestJig.ViewModule.LightModeCalibration;
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

namespace Azure.ScannerTestJig.View.LightModeCalibration
{
    /// <summary>
    /// LightModeConfig.xaml 的交互逻辑
    /// </summary>
    public partial class LightModeConfig : UserControl
    {
        public LightModeConfig()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(IVContorl_Loaded);
        }

        void IVContorl_Loaded(object sender, EventArgs e)
        {

            LightModeCalibrationViewModule vm = Workspace.This.LightModeCaliVM;
            if (vm != null)
            {
                this.DataContext = vm;
                vm.InitIVControls();
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

                float laserPower = Convert.ToSingle(tBox.Text);

            }
        }

        private void SetCurrentPMTNumberButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TextBox_PreviewTextInput1(object sender, TextCompositionEventArgs e)
        {
            if (Workspace.This.LightModeCaliVM.GetIVvNumberValue != "11000000")
                e.Handled = !IsTextAllowed(e.Text);
        }

        private void TextBox_PreviewKeyDown1(object sender, KeyEventArgs e)
        {
            if (Workspace.This.LightModeCaliVM.GetIVvNumberValue != "11000000")
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

                    float laserPower = Convert.ToSingle(tBox.Text);

                }
            }

        }
    }
}
