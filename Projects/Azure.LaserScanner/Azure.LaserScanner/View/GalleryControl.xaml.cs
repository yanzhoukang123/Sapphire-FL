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
using Azure.LaserScanner.View;
using Azure.LaserScanner.ViewModel;

namespace Azure.LaserScanner.View
{
    /// <summary>
    /// Interaction logic for GalleryControl.xaml
    /// </summary>
    public partial class GalleryControl : UserControl
    {
        private string _AdDefaultLayoutResourceName = "Azure.LaserScanner.Resources.AdDefaultLayoutFile.xml";
        //private string _AdLayoutFileName = System.IO.Path.Combine(Environment.CurrentDirectory, "AdLayoutFile.xml");
        //private string _AdLayoutFileName = "AdLayoutFile.xml";
        private bool _IsAvalonLoaded = false;

        public GalleryControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Event raised when AvalonDock has loaded.
        /// Currently only loaidng the default layout.
        /// </summary>
        private void avalonDockHost_AvalonDockLoaded(object sender, EventArgs e)
        {
            //
            // This line of code can be uncommented to get a list of resources.
            //
            //string[] names = this.GetType().Assembly.GetManifestResourceNames();

            //
            // Load the default AvalonDock layout from an embedded resource.
            //  private static readonly string DefaultLayoutResourceName = "Azure.Resources.DefaultLayoutFile.xml";

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

        /// <summary>
        /// Event raised when a document is being closed by clicking the 'X' button in AvalonDock.
        /// </summary>
        private void avalonDockHost_DocumentClosing(object sender, AvalonDockMVVM.DocumentClosingEventArgs e)
        {
            var document = (FileViewModel)e.Document;
            if (!Workspace.This.Close(document))
            {
                e.Cancel = true;
            }
        }

        //
        // The ManipulationBoundaryFeedback event enables applications or components to provide visual feedback
        // when an object hits a boundary.For example, the Window class handles the ManipulationBoundaryFeedback
        // event to cause the window to slightly move when its edge is encountered.
        private void OnManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e)
        {
            e.Handled = true;
        }
    }
}
