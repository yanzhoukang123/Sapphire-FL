using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// Interaction logic for GalleryROIControl.xaml
    /// </summary>
    public partial class GalleryROIControl : UserControl
    {
        public GalleryROIControl()
        {
            InitializeComponent();
        }

        private void _SelectBtn_Checked(object sender, RoutedEventArgs e)
        {
            Uri source = null;
            BitmapImage img = null;

            if ((bool)_SelectBtn.IsChecked)
            {
                source = new Uri("pack://application:,,,/Resources/Images/Gallery/btn_gallery-select-selected.png");
            }
            else
            {
                source = new Uri("pack://application:,,,/Resources/Images/Gallery/btn_gallery-select-default.png");
            }

            img = new BitmapImage(source);
            _SelectBtn.Content = img;
        }
    }
}
