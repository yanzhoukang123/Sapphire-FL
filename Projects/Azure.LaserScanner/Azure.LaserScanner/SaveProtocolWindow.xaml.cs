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
    public partial class SaveProtocolWindow : Window
    {
        #region Private data...

        private string _ProtocolName = string.Empty;

        #endregion

        #region Public properties...

        public string ProtocolName
        {
            get { return _ProtocolName; }
            set
            {
                _ProtocolName = value;
            }
        }

        #endregion

        #region Constructors...

        public SaveProtocolWindow()
        {
            InitializeComponent();
        }

        #endregion

        //private static bool IsTextAllowed(string text)
        //{
        //    Regex regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
        //    return !regex.IsMatch(text);
        //}

        //private void _MethodNameTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        //{
        //    e.Handled = !IsValidFileName(e.Text);
        //}

        //private void _DestFolderTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        //{
        //    e.Handled = !IsValidPathName(e.Text);
        //}

        //private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == Key.Return)
        //    {
        //        TextBox tBox = (TextBox)sender;
        //        DependencyProperty prop = TextBox.TextProperty;
        //
        //        BindingExpression binding = BindingOperations.GetBindingExpression(tBox, prop);
        //        if (binding != null)
        //        {
        //            binding.UpdateSource();
        //        }
        //    }
        //}

        //bool IsValidFileName(string text)
        //{
        //    Regex containsABadCharacter = new Regex("["
        //          + Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars())) + "]");
        //    if (containsABadCharacter.IsMatch(text)) { return false; };
        //
        //    return true;
        //}

        //bool IsValidPathName(string text)
        //{
        //    Regex containsABadCharacter = new Regex("["
        //          + Regex.Escape(new string(System.IO.Path.GetInvalidPathChars())) + "]");
        //    if (containsABadCharacter.IsMatch(text)) { return false; };

        //    return true;
        //}

        private void _SaveButton_Click(object sender, RoutedEventArgs e)
        {
            ProtocolName = _ProtocolNameTextBox.Text.Trim();
            if (string.IsNullOrEmpty(ProtocolName))
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Invalid protocol name. Please enter a valid protocol name", "Invalid protocol name", MessageBoxButton.OK, MessageBoxImage.Stop);
                //this.DialogResult = false;
                return;
            }

            if (DataContext == null) { return; }

            if (DataContext is FluorescenceViewModel)
            {
                FluorescenceViewModel viewModel = DataContext as FluorescenceViewModel;
                if (viewModel.ValidateProtocolName(ProtocolName))
                {
                    DialogResult = true;
                }
                else
                {
                    //DialogResult = false;
                    return;
                }
            }
            else if (DataContext is PhosphorViewModel)
            {
                PhosphorViewModel viewModel = DataContext as PhosphorViewModel;
                if (viewModel.ValidateProtocolName(ProtocolName))
                {
                    DialogResult = true;
                }
                else
                {
                    //DialogResult = false;
                    return;
                }
            }
        }

        private void _Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        //private void Window_Closed(object sender, EventArgs e)
        //{
        //    // It's time to close the onscreen keyboard.
        //    //CloseOnscreenKeyboard();
        //}

        //private void CloseOnscreenKeyboard()
        //{
        //    // retrieve the handler of the window
        //    int iHandle = Utilities.WindowsInvoke.FindWindow("IPTIP_Main_Window", "");
        //    if (iHandle > 0)
        //    {
        //        // close the window using API
        //        Utilities.WindowsInvoke.SendMessage(iHandle, Utilities.WindowsInvoke.WM_SYSCOMMAND, Utilities.WindowsInvoke.SC_CLOSE, 0);
        //    }
        //}

        //private void _KeyboardBtn_Click(object sender, RoutedEventArgs e)
        //{
        //    string progFiles = @"C:\Program Files\Common Files\Microsoft Shared\ink";
        //    string keyboardPath = System.IO.Path.Combine(progFiles, "TabTip.exe");
        //
        //    System.Diagnostics.Process.Start(keyboardPath);
        //}
    }

    //<Window...
    //    xmlns:xc="clr-namespace:Azure.LaserScanner"
    //    xc:DialogCloser.DialogResult="{Binding DialogResult}">
    /*public static class DialogCloser
    {
        public static readonly DependencyProperty DialogResultProperty =
            DependencyProperty.RegisterAttached(
                "DialogResult",
                typeof(bool?),
                typeof(DialogCloser),
                new PropertyMetadata(DialogResultChanged));

        private static void DialogResultChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var window = d as Window;
            if (window != null)
                window.DialogResult = e.NewValue as bool?;
        }
        public static void SetDialogResult(Window target, bool? value)
        {
            target.SetValue(DialogResultProperty, value);
        }
    }*/

}
