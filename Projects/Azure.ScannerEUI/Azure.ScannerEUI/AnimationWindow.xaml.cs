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

namespace Azure.ScannerEUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class AnimationWindow : Window
    {
        public string LoadingText
        {
            get
            {
                if (_BusyIndicator != null)
                {
                    return (string)_BusyIndicator.BusyContent;
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                if (_BusyIndicator != null)
                {
                    _BusyIndicator.BusyContent = value;
                }
            }
        }

        public AnimationWindow()
        {
            InitializeComponent();
        }

        public void SafeClose()
        {
            // Make sure we're running on the UI thread
            if (!this.Dispatcher.CheckAccess())
            {
                System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke(new Action(SafeClose));
                return;
            }

            // Close the window now that we're running on the UI thread
            Close();
        }

    }
}
