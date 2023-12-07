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
using Azure.Image.Processing;
//using Azure.WindowsAPI.Interop;
using Azure.LaserScanner.ViewModel;

namespace Azure.LaserScanner
{
    /// <summary>
    /// Interaction logic for SelectFilePath.xaml
    /// </summary>
    public partial class SelectFilePath : Window
    {
        #region Private data...

        private string _FileName = string.Empty;
        private string _DestinationFolder = string.Empty;

        #endregion

        #region Public properties...

        public string FileName
        {
            get { return _FileName; }
            set
            {
                _FileName = value;
                _FileNameTextBox.Text = _FileName;
            }
        }

        public string DestinationFolder
        {
            get { return _DestinationFolder; }
            set
            {
                _DestinationFolder = value;
                _DestFolderTextBox.Text = _DestinationFolder;
            }
        }

        #endregion

        #region Constructors...

        public SelectFilePath(string fileName, string destFolder)
        {
            InitializeComponent();
        }

        #endregion

        //private static bool IsTextAllowed(string text)
        //{
        //    Regex regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
        //    return !regex.IsMatch(text);
        //}

        private void _FileNameTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsValidFileName(e.Text);
        }

        private void _DestFolderTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsValidPathName(e.Text);
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
            }
        }

        bool IsValidFileName(string text)
        {
            Regex containsABadCharacter = new Regex("["
                  + Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars())) + "]");
            if (containsABadCharacter.IsMatch(text)) { return false; };

            return true;
        }

        bool IsValidPathName(string text)
        {
            Regex containsABadCharacter = new Regex("["
                  + Regex.Escape(new string(System.IO.Path.GetInvalidPathChars())) + "]");
            if (containsABadCharacter.IsMatch(text)) { return false; };

            return true;
        }

        private void _Browse_Click(object sender, RoutedEventArgs e)
        {
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

        private void _OkButton_Click(object sender, RoutedEventArgs e)
        {
            FileName = _FileNameTextBox.Text;
            DestinationFolder = _DestFolderTextBox.Text;

            if (!string.IsNullOrEmpty(_DestinationFolder))
            {
                // close the touch screen keyboard
                //CloseOnscreenKeyboard();

                if (!System.IO.Directory.Exists(_DestinationFolder))
                {
                    try
                    {
                        System.IO.Directory.CreateDirectory(_DestinationFolder);
                    }
                    catch (Exception ex)
                    {
                        Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, "Error: Creating the specified directory",
                            MessageBoxButton.OK, MessageBoxImage.Stop);
                        return;
                    }
                }

                this.DialogResult = true;
            }
            else
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Make sure you select a valid destination folder.", "Invalid folder...", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        private void _Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        //private void Window_Loaded(object sender, RoutedEventArgs e)
        //{
        //    var viewModel = Workspace.This.FluorescenceVM;
        //    if (viewModel != null)
        //    {
        //        FileName = viewModel.FileName;
        //        DestinationFolder = viewModel.DestFolder;
        //    }
        //}

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
            //CloseOnscreenKeyboard();
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

        /*private void SaveAsFileTypeCb_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
        }*/

    }
}
