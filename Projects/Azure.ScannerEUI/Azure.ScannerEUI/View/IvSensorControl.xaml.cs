using Azure.ScannerEUI.ViewModel;
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
    /// Interaction logic for IvSensor.xaml 
    /// </summary>
    public partial class IvSensorControl : UserControl
    {
        public IvSensorControl()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(IVContorl_Loaded);
        }
        void IVContorl_Loaded(object sender,EventArgs e) {

            IvViewModel vm = Workspace.This.IVVM;
            if (vm != null)
            {
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
                IvViewModel IvVm = DataContext as IvViewModel;
                if (IvVm != null)
                {
                    if (tBox.Equals(_LaserAPowerTextBox))
                    {
                        IvVm.LaserAPower = laserPower;
                        if (laserPower == 0)
                        {
                            IvVm.IsLaserL1Selected = false;
                        }
                        else
                        {
                            IvVm.IsLaserL1Selected = true;
                        }
                    }
                    else if (tBox.Equals(_LaserBPowerTextBox))
                    {
                        IvVm.LaserBPower = laserPower;
                        if (laserPower == 0)
                        {
                            IvVm.IsLaserR1Selected = false;
                        }
                        else
                        {
                            IvVm.IsLaserR1Selected = true;
                        }
                    }
                    if (tBox.Equals(_LaserCPowerTextBox))
                    {
                        IvVm.LaserCPower = laserPower;
                        if (laserPower == 0)
                        {
                            IvVm.IsLaserR2Selected = false;
                        }
                        else
                        {
                            IvVm.IsLaserR2Selected = true;
                        }
                    }
                }
            }
        }
    }
}
