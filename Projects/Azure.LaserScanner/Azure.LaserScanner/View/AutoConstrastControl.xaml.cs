using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Azure.LaserScanner.View
{
    /// <summary>
    /// Interaction logic for AutoConstrastControl.xaml
    /// </summary>
    public partial class AutoContrastControl : UserControl
    {
        public AutoContrastControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Update auto contrast image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _AutoContrastButton_Checked(object sender, RoutedEventArgs e)
        {
            Uri source = null;
            BitmapImage img = null;

            if ((bool)_AutoContrastButton.IsChecked)
            {
                source = new Uri("pack://application:,,,/Resources/Images/Gallery/btn_auto-selected.png");
            }
            else
            {
                source = new Uri("pack://application:,,,/Resources/Images/Gallery/btn_auto-default.png");
            }

            img = new BitmapImage(source);
            _AutoContrastButton.Content = img;
        }

        /// <summary>
        /// Update invert button image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _InvertButton_Checked(object sender, RoutedEventArgs e)
        {
            Uri source = null;
            BitmapImage img = null;

            if ((bool)_InvertButton.IsChecked)
            {
                source = new Uri("pack://application:,,,/Resources/Images/Gallery/btn_invert-selected.png");
            }
            else
            {
                source = new Uri("pack://application:,,,/Resources/Images/Gallery/btn_invert-default.png");
            }

            img = new BitmapImage(source);
            _InvertButton.Content = img;
        }

        /// <summary>
        /// Update saturation image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _SaturationButton_Checked(object sender, RoutedEventArgs e)
        {
            Uri source = null;
            BitmapImage img = null;

            if ((bool)_SaturationButton.IsChecked)
            {
                source = new Uri("pack://application:,,,/Resources/Images/Gallery/btn_saturation-selected.png");
            }
            else
            {
                source = new Uri("pack://application:,,,/Resources/Images/Gallery/btn_saturation-default.png");
            }

            img = new BitmapImage(source);
            _SaturationButton.Content = img;
        }
    }
}
