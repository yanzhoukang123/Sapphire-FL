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

namespace Azure.LaserScanner.View
{
    /// <summary>
    /// Interaction logic for RGBChannelControl.xaml
    /// </summary>
    public partial class RGBChannelControl : UserControl
    {
        public RGBChannelControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Update default image for red channel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /*private void _RedChannelButton_Checked(object sender, RoutedEventArgs e)
        {
            Uri source = null;
            BitmapImage img = null;

            if ((bool)_RedChannelButton.IsChecked)
            {
                source = new Uri("pack://application:,,,/Resources/Images/Gallery/icon_r-ch-selected.png");
            }
            else
            {
                source = new Uri("pack://application:,,,/Resources/Images/Gallery/icon_r-ch-default.png");
            }

            img = new BitmapImage(source);
            _RedChannelButton.Content = img;
        }*/

        /// <summary>
        /// Update default image for red channel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /*private void chkRedChannel_Checked(object sender, RoutedEventArgs e)
        {
            Uri source = null;
            BitmapImage img = null;

            if ((bool)chkRedChannel.IsChecked)
            {
                source = new Uri("pack://application:,,,/Resources/Images/Gallery/icon_r-ch-selected.png");
            }
            else
            {
                source = new Uri("pack://application:,,,/Resources/Images/Gallery/icon_r-ch-default.png");
            }

            //_RedChannelButton.IsChecked = chkRedChannel.IsChecked;

            img = new BitmapImage(source);
            _RedChannelButton.Content = img;
        }*/

        /// <summary>
        /// Update default image for green channel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /*private void _GreenChannelButton_Checked(object sender, RoutedEventArgs e)
        {
            Uri source = null;
            BitmapImage img = null;

            if ((bool)_GreenChannelButton.IsChecked)
            {
                source = new Uri("pack://application:,,,/Resources/Images/Gallery/icon_g-ch-selected.png");
            }
            else
            {
                source = new Uri("pack://application:,,,/Resources/Images/Gallery/icon_g-ch-default.png");
            }

            img = new BitmapImage(source);
            _GreenChannelButton.Content = img;
        }*/

        /// <summary>
        /// Update default image for green channel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /*private void chkGreenChannel_Checked(object sender, RoutedEventArgs e)
        {
            Uri source = null;
            BitmapImage img = null;

            if ((bool)chkGreenChannel.IsChecked)
            {
                source = new Uri("pack://application:,,,/Resources/Images/Gallery/icon_g-ch-selected.png");
            }
            else
            {
                source = new Uri("pack://application:,,,/Resources/Images/Gallery/icon_g-ch-default.png");
            }

            //_GreenChannelButton.IsChecked = chkGreenChannel.IsChecked;

            img = new BitmapImage(source);
            _GreenChannelButton.Content = img;
        }*/

        /// <summary>
        /// Update default image for blue channel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /*private void _BlueChannelButton_Checked(object sender, RoutedEventArgs e)
        {
            Uri source = null;
            BitmapImage img = null;

            if ((bool)_BlueChannelButton.IsChecked)
            {
                source = new Uri("pack://application:,,,/Resources/Images/Gallery/icon_b-ch-selected.png");
            }
            else
            {
                source = new Uri("pack://application:,,,/Resources/Images/Gallery/icon_b-ch-default.png");
            }

            img = new BitmapImage(source);
            _BlueChannelButton.Content = img;
        }*/

        /// <summary>
        /// Update default image for blue channel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /*private void chkBlueChannel_Checked(object sender, RoutedEventArgs e)
        {
            Uri source = null;
            BitmapImage img = null;

            if ((bool)chkBlueChannel.IsChecked)
            {
                source = new Uri("pack://application:,,,/Resources/Images/Gallery/icon_b-ch-selected.png");
            }
            else
            {
                source = new Uri("pack://application:,,,/Resources/Images/Gallery/icon_b-ch-default.png");
            }

            //_BlueChannelButton.IsChecked = chkBlueChannel.IsChecked;

            img = new BitmapImage(source);
            _BlueChannelButton.Content = img;
        }*/

        /// <summary>
        /// Update default image for gray channel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /*private void _GrayChannelButton_Checked(object sender, RoutedEventArgs e)
        {
            Uri source = null;
            BitmapImage img = null;

            if ((bool)_GrayChannelButton.IsChecked)
            {
                source = new Uri("pack://application:,,,/Resources/Images/Gallery/icon_gy-ch-selected.png");
            }
            else
            {
                source = new Uri("pack://application:,,,/Resources/Images/Gallery/icon_gy-ch-default.png");
            }

            img = new BitmapImage(source);
            _GrayChannelButton.Content = img;
        }*/

        /// <summary>
        /// Update default image for gray channel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /*private void chkGrayChannel_Checked(object sender, RoutedEventArgs e)
        {
            Uri source = null;
            BitmapImage img = null;

            if ((bool)chkGrayChannel.IsChecked)
            {
                source = new Uri("pack://application:,,,/Resources/Images/Gallery/icon_gy-ch-selected.png");
            }
            else
            {
                source = new Uri("pack://application:,,,/Resources/Images/Gallery/icon_gy-ch-default.png");
            }

            //_GrayChannelButton.IsChecked = chkGrayChannel.IsChecked;

            img = new BitmapImage(source);
            _GrayChannelButton.Content = img;
        }*/

        /// <summary>
        /// Update default image for composit display
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /*private void _CompositDisplayButton_Checked(object sender, RoutedEventArgs e)
        {
            Uri sourceComposit = null;
            BitmapImage imgComposit = null;

            if ((bool)_CompositDisplayButton.IsChecked)
            {
                sourceComposit = new Uri("pack://application:,,,/Resources/Images/Gallery/icon_rgb-selected.png");
            }
            else
            {
                sourceComposit = new Uri("pack://application:,,,/Resources/Images/Gallery/icon_rgb-default.png");
            }

            imgComposit = new BitmapImage(sourceComposit);
            _CompositDisplayButton.Content = imgComposit;
        }*/

        /// <summary>
        /// Update default image for composit display
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /*private void chkCompositDisplay_Checked(object sender, RoutedEventArgs e)
        {
            Uri sourceComposit = null;
            BitmapImage imgComposit = null;

            if ((bool)chkCompositDisplay.IsChecked)
            {
                sourceComposit = new Uri("pack://application:,,,/Resources/Images/Gallery/icon_rgb-selected.png");
            }
            else
            {
                sourceComposit = new Uri("pack://application:,,,/Resources/Images/Gallery/icon_rgb-default.png");
            }

            imgComposit = new BitmapImage(sourceComposit);
            _CompositDisplayButton.Content = imgComposit;
        }*/
    }
}
