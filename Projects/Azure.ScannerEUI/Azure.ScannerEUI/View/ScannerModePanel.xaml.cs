using Azure.ScannerEUI.ViewModel;
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

namespace Azure.ScannerEUI.View
{
    /// <summary>
    /// Interaction logic for LasersPanel.xaml
    /// </summary>
    public partial class ScannerModePanel : UserControl
    {
        public ScannerModePanel()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(Contorl_Loaded);
        }
        void Contorl_Loaded(object sender, EventArgs e)
        {
            var vm = Workspace.This;
            if (vm != null)
            {
                this.DataContext = vm;
            }
        }
    }
}
