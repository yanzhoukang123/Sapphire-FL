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
using Azure.ScannerEUI.ViewModel;

namespace Azure.ScannerEUI.View
{
    /// <summary>
    /// Interaction logic for ImageQualityControl.xaml
    /// </summary>
    public partial class ImageQualityControl : UserControl
    {
        public ImageQualityControl()
        {
            InitializeComponent();
            this.Loaded += ImageQualityControl_Loaded;
        }

        private void ImageQualityControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext != null)
            {
                if (DataContext is ScannerViewModel)
                {
                    ((ScannerViewModel)DataContext).Initialize();
                }
            }
        }
    }
}
