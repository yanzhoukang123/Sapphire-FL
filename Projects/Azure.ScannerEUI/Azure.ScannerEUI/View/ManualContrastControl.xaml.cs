using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Azure.ScannerEUI.ViewModel;

namespace Azure.ScannerEUI.View
{
    /// <summary>
    /// Interaction logic for ManualContrastControl.xaml
    /// </summary>
    public partial class ManualContrastControl : UserControl
    {
        public ManualContrastControl()
        {
            InitializeComponent();

            _BlackValueSlider.PreviewMouseLeftButtonUp += Slider_MouseLeftButtonUp;
            _WhiteValueSlider.PreviewMouseLeftButtonUp += Slider_MouseLeftButtonUp;
            _GammaValueSlider.PreviewMouseLeftButtonUp += Slider_MouseLeftButtonUp;

            //this.MouseDown += Window_MouseDown;

            DataContext = Workspace.This;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // place the window at the bottom right corner of the screen
            //var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
            //this.Left = desktopWorkingArea.Right - this.Width;
            //this.Top = desktopWorkingArea.Bottom - this.Height;

            // place the window at the bottom right corner of the parent window
            //this.Left = this.Owner.Left + (this.Owner.Width - this.ActualWidth);
            //this.Top = this.Owner.Top + this.Owner.Height - this.ActualHeight;
        }

        /*private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Workspace.This != null)
            {
                if (Workspace.This.IsManualContrast)
                {
                    Workspace.This.IsManualContrast = false;    // Uncheck the manual contrast button
                }
            }
        }*/

        private void Slider_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //Workspace.This.ImageProcessing.BWGChangedImageUpdate();
        }

        private void _ButtonClose_Click(object sender, RoutedEventArgs e)
        {

        }

        //private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        //{
        //    if (e.ChangedButton == MouseButton.Left)
        //    {
        //        this.DragMove();
        //    }
        //}

        //private void _ButtonClose_Click(object sender, RoutedEventArgs e)
        //{
        //    this.Close();

        //    if (Workspace.This != null)
        //    {
        //        //if (Workspace.This.IsManualContrast)
        //        //{
        //        //    Workspace.This.IsManualContrast = false;    // Uncheck the manual contrast button
        //        //}
        //    }
        //}

    }
}
