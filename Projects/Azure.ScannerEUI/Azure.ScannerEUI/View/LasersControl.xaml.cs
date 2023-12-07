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
    /// Interaction logic for LasersControl.xaml
    /// </summary>
    public partial class LasersControl : UserControl
    {
        public LasersControl()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(LasersControl_Loaded);
            //ScannerViewModel scannerVm = Workspace.This.ScannerVM;
            //if (scannerVm != null)
            //{
            //    scannerVm.InitLasersIntensity();
            //}
        }

        void LasersControl_Loaded(object sender, RoutedEventArgs e)
        {
            ScannerViewModel scannerVm = Workspace.This.ScannerVM;
            //if (scannerVm != null)
            //{
            //    scannerVm.InitLasersIntensity();
            //}
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
                ScannerViewModel scannerVm = DataContext as ScannerViewModel;
                if (scannerVm != null)
                {
                    if (tBox.Equals(_LaserAPowerTextBox))
                    {
                        scannerVm.LaserAPower = laserPower;
                        if (laserPower == 0)
                        {
                            scannerVm.IsLaserASelected = false;
                        }
                        else
                        {
                            scannerVm.IsLaserASelected = true;
                        }
                    }
                    else if (tBox.Equals(_LaserBPowerTextBox))
                    {
                        scannerVm.LaserBPower = laserPower;
                        if (laserPower == 0)
                        {
                            scannerVm.IsLaserBSelected = false;
                        }
                        else
                        {
                            scannerVm.IsLaserBSelected = true;
                        }
                    }
                    if (tBox.Equals(_LaserCPowerTextBox))
                    {
                        scannerVm.LaserCPower = laserPower;
                        if (laserPower == 0)
                        {
                            scannerVm.IsLaserCSelected = false;
                        }
                        else
                        {
                            scannerVm.IsLaserCSelected = true;
                        }
                    }
                    if (tBox.Equals(_LaserDPowerTextBox))
                    {
                        scannerVm.LaserDPower = laserPower;
                        if (laserPower == 0)
                        {
                            scannerVm.IsLaserDSelected = false;
                        }
                        else
                        {
                            scannerVm.IsLaserDSelected = true;
                        }
                    }
                }
            }
        }
    }
}
