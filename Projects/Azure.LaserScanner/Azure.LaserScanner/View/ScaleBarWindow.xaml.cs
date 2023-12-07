using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
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
using Azure.LaserScanner.ViewModel;

namespace Azure.LaserScanner.View
{
    /// <summary>
    /// Interaction logic for ScaleBarWindow.xaml
    /// </summary>
    public partial class ScaleBarWindow : Window
    {
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private const int GWL_STYLE = -16;

        private const int WS_MAXIMIZEBOX = 0x10000; //maximize button
        private const int WS_MINIMIZEBOX = 0x20000; //minimize button
        private const int WS_SYSMENU = 0x80000;
        //private IntPtr _windowHandle;

        public ScaleBarWindow()
        {
            InitializeComponent();
            SourceInitialized += ScaleBarWindow_SourceInitialized;
            this.Loaded += ScaleBarWindow_Loaded;
        }

        private void ScaleBarWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // WORK-AROUND: scale bar not displaying on the initial window loaded (DrawingCanvas.ActualWidth/ActualHeight is 0).
            var viewModel = this.DataContext as ScaleBarViewModel;
            if (viewModel != null && viewModel.IsShowScalebar)
            {
                viewModel.UpdateScaleBar();
            }
        }

        private void ScaleBarWindow_SourceInitialized(object sender, EventArgs e)
        {
            //_windowHandle = new WindowInteropHelper(this).Handle;

            //disable minimize button
            //DisableMinimizeButton();
            var hwnd = new WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
