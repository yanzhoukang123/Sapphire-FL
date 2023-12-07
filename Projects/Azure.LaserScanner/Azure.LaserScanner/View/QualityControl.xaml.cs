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
    /// Interaction logic for QualityControl.xaml
    /// </summary>
    public partial class QualityControl : UserControl
    {
        public QualityControl()
        {
            InitializeComponent();
        }

        private void cmbQualityList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Stop the event from bubbling.
            e.Handled = true;
        }
    }
}
