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
    /// Interaction logic for GalleryPanel.xaml
    /// </summary>
    public partial class GalleryPanel : UserControl
    {
        public GalleryPanel()
        {
            InitializeComponent();

            //AdjustmentsToggleButton.IsChecked = false;
            //AdjustmentsToggleButton_Checked(this, null);
        }

        /// <summary>
        /// Update icon for adjustments panel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /*private void AdjustmentsToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            Uri source = null;
            BitmapImage img = null;

            if ((bool)AdjustmentsToggleButton.IsChecked)
            {
                source = new Uri("pack://application:,,,/Resources/Images/Gallery/btn_gallery-adjustments-selected.png");

                this._ContrastControl.Visibility = Visibility.Visible;
                this._AutoContrastControl.Visibility = Visibility.Visible;
            }
            else
            {
                source = new Uri("pack://application:,,,/Resources/Images/Gallery/btn_gallery-adjustments-default.png");

                this._ContrastControl.Visibility = Visibility.Collapsed;
                this._AutoContrastControl.Visibility = Visibility.Collapsed;
                //this._RgbChanControl.Visibility = Visibility.Collapsed;
            }
            
            img = new BitmapImage(source);
            AdjustmentsToggleButton.Content = img;
        }*/

        /// <summary>
        /// Update icon for crop panel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /*private void CropToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            Uri source = null;
            BitmapImage img = null;

            if ((bool)CropToggleButton.IsChecked)
            {
                source = new Uri("pack://application:,,,/Resources/Images/Gallery/btn_gallery-crop-selected.png");
            }
            else
            {
                source = new Uri("pack://application:,,,/Resources/Images/Gallery/btn_gallery-crop-default.png");
            }

            img = new BitmapImage(source);
            CropToggleButton.Content = img;
        }*/

        /// <summary>
        /// Update icon for transform panel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /*private void TransformToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            Uri source = null;
            BitmapImage img = null;

            if ((bool)TransformToggleButton.IsChecked)
            {
                source = new Uri("pack://application:,,,/Resources/Images/Gallery/btn_gallery-transform-selected.png");
                this.RotateControlPanel.Visibility = Visibility.Visible;
            }
            else
            {
                source = new Uri("pack://application:,,,/Resources/Images/Gallery/btn_gallery-transform-default.png");
                this.RotateControlPanel.Visibility = Visibility.Collapsed;
            }

            img = new BitmapImage(source);
            TransformToggleButton.Content = img;
        }*/

        /// <summary>
        /// Update icon for resize panel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /*private void ResizeToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            Uri source = null;
            BitmapImage img = null;

            if ((bool)ResizeToggleButton.IsChecked)
            {
                source = new Uri("pack://application:,,,/Resources/Images/Gallery/btn_gallery-resize-selected.png");
            }
            else
            {
                source = new Uri("pack://application:,,,/Resources/Images/Gallery/btn_gallery-resize-default.png");
            }

            img = new BitmapImage(source);
            ResizeToggleButton.Content = img;
        }*/
    }
}
