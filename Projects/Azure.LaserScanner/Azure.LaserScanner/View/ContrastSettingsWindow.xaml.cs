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
using System.Windows.Shapes;

namespace Azure.LaserScanner.View
{
    /// <summary>
    /// Interaction logic for ContrastSettingsWindow.xaml
    /// </summary>
    public partial class ContrastSettingsWindow : Window
    {
        public ContrastSettingsWindow()
        {
            InitializeComponent();
            //this.Loaded += new RoutedEventHandler(ContrastSettingsWindow_Loaded);
            this.MouseDown += new MouseButtonEventHandler(ContrastSettingsWindow_MouseDown);
        }

        /*void ContrastSettingsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            #region === Auto size a window to fit its content ===

            // Manually alter window height and width
            this.SizeToContent = SizeToContent.Manual;

            // Automatically resize width relative to content
            this.SizeToContent = SizeToContent.Width;

            // Automatically resize height relative to content
            this.SizeToContent = SizeToContent.Height;

            // Automatically resize height and width relative to content
            this.SizeToContent = SizeToContent.WidthAndHeight;

            #endregion
        }*/

        void ContrastSettingsWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Allow the window to be drag
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

    }
}
