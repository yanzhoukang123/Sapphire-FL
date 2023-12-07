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
using Azure.LaserScanner.ViewModel;
using Azure.Configuration.Settings;

namespace Azure.LaserScanner
{
    /// <summary>
    /// Interaction logic for MainWindowContent.xaml
    /// </summary>
    public partial class MainWindowContent : UserControl
    {
        public MainWindowContent()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Update imaging selected icon
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnImaging_Checked(object sender, RoutedEventArgs e)
        {
            Uri source = new Uri("pack://application:,,,/Resources/Images/General/icon_imaging-selected.png");
            BitmapImage img = new BitmapImage(source);

            btnImaging.Content = img;
        }

        /// <summary>
        /// Update imaging unselected icon
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnImaging_Unchecked(object sender, RoutedEventArgs e)
        {
            Uri source = new Uri("pack://application:,,,/Resources/Images/General/icon_imaging-default.png");
            BitmapImage img = new BitmapImage(source);

            btnImaging.Content = img;
        }

        /// <summary>
        /// Update gallery selected icon
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnGallery_Checked(object sender, RoutedEventArgs e)
        {
            Uri source = new Uri("pack://application:,,,/Resources/Images/General/icon_gallery-selected.png");
            BitmapImage img = new BitmapImage(source);

            btnGallery.Content = img;
        }

        /// <summary>
        /// Update gallery unselected icon
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnGallery_Unchecked(object sender, RoutedEventArgs e)
        {
            Uri source = new Uri("pack://application:,,,/Resources/Images/General/icon_gallery-default.png");
            BitmapImage img = new BitmapImage(source);

            btnGallery.Content = img;
        }

        private void btnUserManual_Checked(object sender, RoutedEventArgs e)
        {
            Uri source = new Uri("pack://application:,,,/Resources/Images/General/icon_user_manual-selected.png");
            BitmapImage img = new BitmapImage(source);

            btnUserManual.Content = img;
        }

        private void btnUserManual_Unchecked(object sender, RoutedEventArgs e)
        {
            Uri source = new Uri("pack://application:,,,/Resources/Images/General/icon_user_manual-default.png");
            BitmapImage img = new BitmapImage(source);

            btnUserManual.Content = img;
        }

        /// <summary>
        /// Update settings selected icon
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSettings_Checked(object sender, RoutedEventArgs e)
        {
            Uri source = new Uri("pack://application:,,,/Resources/Images/General/icon_settings-selected.png");
            BitmapImage img = new BitmapImage(source);

            btnSettings.Content = img;
        }

        /// <summary>
        /// Update settings unselected icon
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSettings_Unchecked(object sender, RoutedEventArgs e)
        {
            Uri source = new Uri("pack://application:,,,/Resources/Images/General/icon_settings-default.png");
            BitmapImage img = new BitmapImage(source);

            btnSettings.Content = img;
        }

        /// <summary>
        /// Update gallery file button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            Uri source = new Uri("pack://application:,,,/Resources/Images/Gallery/icon_file-selected.png");
            BitmapImage img = new BitmapImage(source);

            FileToggleButton.Content = img;
        }

        /// <summary>
        /// Update gallery unselected file button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            Uri source = new Uri("pack://application:,,,/Resources/Images/Gallery/icon_file-default.png");
            BitmapImage img = new BitmapImage(source);

            FileToggleButton.Content = img;
        }

        /*private void ZoomSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var textFormattingMode = e.NewValue > 1.0 || Math.Abs(e.NewValue - 1.0) < double.Epsilon ? TextFormattingMode.Ideal : TextFormattingMode.Display;
            TextOptions.SetTextFormattingMode(this, textFormattingMode);
        }*/
    }
}
