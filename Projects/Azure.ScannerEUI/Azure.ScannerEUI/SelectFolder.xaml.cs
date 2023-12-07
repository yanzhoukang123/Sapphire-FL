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
using Azure.Image.Processing;
//using Azure.WindowsAPI.Interop;

namespace Azure.ScannerEUI
{
    /// <summary>
    /// Interaction logic for SaveAllPrompt.xaml
    /// </summary>
    public partial class SelectFolder : Window
    {
        #region Private data...
        
        private string _DestinationFolder = string.Empty;
        private string _SelectedFileType = ".tif";
        private bool _IsSaveAsCompressed = false;
        
        #endregion

        #region Public properties...

        public string DestinationFolder
        {
            get { return _DestinationFolder; }
            set
            {
                _DestinationFolder = value;
                _DestFolderTextBox.Text = _DestinationFolder;
            }
        }

        public string SelectedFileType
        {
            get { return _SelectedFileType; }
        }

        public bool IsSaveAsCompressed
        {
            get { return _IsSaveAsCompressed; }
        }

        #endregion

        #region Constructors...
        public SelectFolder(string destFolder)
        {
            InitializeComponent();

            this._DestinationFolder = destFolder;
            this.Closed += new EventHandler(Window_Closed);
        }
        #endregion

        private void _Browse_Click(object sender, RoutedEventArgs e)
        {
            //System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            //System.Windows.Forms.DialogResult dlgResult = fbd.ShowDialog();
            //if (dlgResult == System.Windows.Forms.DialogResult.OK)
            //{
            //    this.DestinationFolder = fbd.SelectedPath;
            //}

            this.Topmost = false;
            WPFFolderBrowser.WPFFolderBrowserDialog wpfFBD = new WPFFolderBrowser.WPFFolderBrowserDialog();
            bool? bResult = wpfFBD.ShowDialog();

            if (bResult == true)
            {
                this.DestinationFolder = wpfFBD.FileName;
            }
            this.Topmost = true;
            this.Focus();
        }

        private void _SaveAll_Click(object sender, RoutedEventArgs e)
        {
            _DestinationFolder = _DestFolderTextBox.Text;

            if (!string.IsNullOrEmpty(_DestinationFolder))
            {
                // close the touch screen keyboard
                CloseOnscreenKeyboard();

                if (!System.IO.Directory.Exists(_DestinationFolder))
                {
                    try
                    {
                        System.IO.Directory.CreateDirectory(_DestinationFolder);
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show(ex.Message, "Error: Creating the specified directory",
                            MessageBoxButton.OK, MessageBoxImage.Stop);
                        return;
                    }
                }

                this.DialogResult = true;
            }
            else
            {
                MessageBox.Show("Make sure you select a valid destination folder.", "Invalid folder...", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        private void _Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _DestFolderTextBox.Text = _DestinationFolder;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            // It's time to close the onscreen keyboard.
            CloseOnscreenKeyboard();
        }

        private void CloseOnscreenKeyboard()
        {
            // retrieve the handler of the window
            int iHandle = Utilities.WindowsInvoke.FindWindow("IPTIP_Main_Window", "");
            if (iHandle > 0)
            {
                // close the window using API
                Utilities.WindowsInvoke.SendMessage(iHandle, Utilities.WindowsInvoke.WM_SYSCOMMAND, Utilities.WindowsInvoke.SC_CLOSE, 0);
            }
        }

        private void _KeyboardBtn_Click(object sender, RoutedEventArgs e)
        {
            string progFiles = @"C:\Program Files\Common Files\Microsoft Shared\ink";
            string keyboardPath = System.IO.Path.Combine(progFiles, "TabTip.exe");

            System.Diagnostics.Process.Start(keyboardPath);
        }

        private void SaveAsFileTypeCb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _SelectedFileType = ".tif";
            _IsSaveAsCompressed = false;

            switch (SaveAsFileTypeCb.SelectedIndex)
            {
                default:
                case 0:
                    _SelectedFileType = ".tif";
                    _IsSaveAsCompressed = false;
                    break;
                case 1:
                    _SelectedFileType = ".tif";
                    _IsSaveAsCompressed = true;
                    break;
                case 2:
                    _SelectedFileType = ".jpg";
                    break;
                case 3:
                    _SelectedFileType = ".bmp";
                    break;
            }
        }

    }
}
