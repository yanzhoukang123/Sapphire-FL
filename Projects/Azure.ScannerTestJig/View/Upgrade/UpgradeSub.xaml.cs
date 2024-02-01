using Azure.ScannerTestJig.ViewModule;
using Azure.ScannerTestJig.ViewModule.Upgrade;
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
using System.Windows.Shapes;

namespace Azure.ScannerTestJig.View.Upgrade
{
    /// <summary>
    /// Upgrade.xaml 的交互逻辑
    /// </summary>
    public partial class UpgradeSub : Window
    {
        public UpgradeSub()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(IVContorl_Loaded);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Workspace.This.Owner.Show();
            if (Workspace.This.UpgradeViewModule.UpgradePort != null)
            {
                Workspace.This.UpgradeViewModule.UpgradePort.Dispose();
            }
        }
        void IVContorl_Loaded(object sender, EventArgs e)
        {

            UpgradeViewModule vm = Workspace.This.UpgradeViewModule;
            if (vm != null)
            {
                this.DataContext = vm;
                vm.InitIVControls();
            }
        }
    }
}
