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
using System.Text.RegularExpressions;   // Regex
using Azure.ScannerEUI.ViewModel;
using Azure.Configuration.Settings;

namespace Azure.ScannerEUI
{
    /// <summary>
    /// SplashScreenWindow.xaml 
    /// </summary>
    public partial class SplashScreenWindow : Window
    {
        public SplashScreenWindow()
        {
            InitializeComponent();
            this.Title = string.Format("Avocado Captrue V{0}", Workspace.This.ProductVersion);
        }
        public void SetProgressValue(double value)
        {
            this.progressBarSplashScreen.Value = value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="TextColor">string Red,string Black</param>
        /// <param name="mess"></param>
        public void SetMessage(string TextColor, string mess)
        {
            if (TextColor == "Red")
            {
                infoBlock.Foreground = Brushes.Red;
            }
            else
            {
                infoBlock.Foreground = Brushes.Black;
            }
            this.infoBlock.AppendText("\n"+mess);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            throw new UserExitException();
        }

        public class UserExitException : Exception
        {
            public UserExitException()
                : base("User Close!!")
            {
            }
        }

        private void infoBlock_TextChanged(object sender, TextChangedEventArgs e)
        {
            //count > 0
            if (infoBlock.LineCount > 0)
            {
                this.infoBlock.ScrollToLine(this.infoBlock.LineCount - 1);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Workspace.This.Owner.Close();
        }
    }

}
