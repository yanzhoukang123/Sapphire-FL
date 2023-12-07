using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfAnimatedGif;
using Azure.LaserScanner.ViewModel;

namespace Azure.LaserScanner.View
{
    /// <summary>
    /// Interaction logic for ZStackingWindow.xaml
    /// </summary>
    public partial class ZStackingWindow : Window
    {
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private const int GWL_STYLE = -16;

        private const int WS_MAXIMIZEBOX = 0x10000; //maximize button
        private const int WS_MINIMIZEBOX = 0x20000; //minimize button

        public ZStackingWindow()
        {
            InitializeComponent();
            this.SourceInitialized += MainWindow_SourceInitialized;
        }

        private IntPtr _windowHandle;
        private void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            _windowHandle = new WindowInteropHelper(this).Handle;

            //disable minimize button
            DisableMinimizeButton();
        }

        protected void DisableMinimizeButton()
        {
            if (_windowHandle == IntPtr.Zero)
                throw new InvalidOperationException("The window has not yet been completely initialized");

            SetWindowLong(_windowHandle, GWL_STYLE, GetWindowLong(_windowHandle, GWL_STYLE) & ~WS_MINIMIZEBOX);
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

        //
        // The ManipulationBoundaryFeedback event enables applications or components to provide visual feedback
        // when an object hits a boundary.For example, the Window class handles the ManipulationBoundaryFeedback
        // event to cause the window to slightly move when its edge is encountered.
        private void OnManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e)
        {
            e.Handled = true;
        }

        /*private void imageControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var animationController = ImageBehavior.GetAnimationController(imageControl);
            if (animationController != null)
            {
                if (animationController.IsComplete || animationController.IsPaused)
                {
                    if (animationController.IsComplete)
                        animationController.GotoFrame(1);
                    // Resume the animation (or restart it if it was completed)
                    animationController.Play();
                }
                else
                {
                    // Pause the animation
                    animationController.Pause();
                }
            }
        }*/


    }
}
