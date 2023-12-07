using Azure.LaserScanner.ViewModel;
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
    /// Interaction logic for RotateControlPanel.xaml
    /// </summary>
    public partial class RotateControlPanel : UserControl
    {
        public RotateControlPanel()
        {
            InitializeComponent();
        }

        /*private void _RotateArbitraryBtn_Checked(object sender, RoutedEventArgs e)
        {
            Uri source = null;
            BitmapImage img = null;

            if ((bool)_RotateArbitraryBtn.IsChecked)
            {
                source = new Uri("pack://application:,,,/Resources/Images/Gallery/btn_gallery-rotate-selected.png");
            }
            else
            {
                source = new Uri("pack://application:,,,/Resources/Images/Gallery/btn_gallery-rotate-default.png");
            }

            img = new BitmapImage(source);
            _RotateArbitraryBtn.Content = img;
        }*/
    }
}
