using Azure.ScannerEUI.ViewModel;
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

namespace Azure.ScannerEUI.View
{
    /// <summary>
    /// ZAutomaticallyFocal.xaml 的交互逻辑
    /// </summary>
    public partial class ZAutomaticallyFocal : UserControl
    {
        public ZAutomaticallyFocal()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(MotorControl_Loaded);
        }

        private void MotorControl_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = Workspace.This.ZAutomaticallyFocalVM;
            ZAutomaticallyFocalViewModel viewModel = DataContext as ZAutomaticallyFocalViewModel;
            if (viewModel != null)
            {
                viewModel.Initialize();
            }
        }
    }
}
