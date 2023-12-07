using Azure.ScannerEUI.ViewModel;
using Gif.Components;
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

namespace Azure.ScannerEUI.View
{
    /// <summary>
    /// GifAnimatedWindow.xaml 的交互逻辑
    /// </summary>
    public partial class GifAnimatedWindow : Window
    {
        private bool _IsAvalonLoaded = false;
        public GifAnimatedWindow()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(MotorControl_Loaded);
        }

        private void MotorControl_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = Workspace.This.ImageRotatingPrcessVM;
            ImageRotatingPrcessViewModel viewModel = DataContext as ImageRotatingPrcessViewModel;
            if (viewModel != null)
            {
                viewModel.Initialize();
            }
        }

        private void _CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private string _AdDefaultLayoutResourceName = "Azure.ScannerEUI.Resources.AdDefaultLayoutFile.xml";
        private void avalonDockHost_AvalonDockLoaded(object sender, EventArgs e)
        {
            //
            // This line of code can be uncommented to get a list of resources.
            //
            //string[] names = this.GetType().Assembly.GetManifestResourceNames();

            //
            // Load the default AvalonDock layout from an embedded resource.
            //  private static readonly string DefaultLayoutResourceName = "cSeries.UI.Resources.DefaultLayoutFile.xml";

            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(_AdDefaultLayoutResourceName))
            {
                if (stream != null && !_IsAvalonLoaded)
                {
                    AvalonDockHost.DockingManager.RestoreLayout(stream);
                    _IsAvalonLoaded = true;
                }
            }
        }

        private void avalonDockHost_DocumentClosing(object sender, AvalonDockMVVM.DocumentClosingEventArgs e)
        {
            var document = (GIFFileViewModel)e.Document;
            if (!Workspace.This.ImageRotatingPrcessVM.Close(document))
            {
                e.Cancel = true;
            }
        }

      
    }
}
