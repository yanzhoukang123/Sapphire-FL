using Azure.ScannerTestJig.ViewModule;
using Azure.ScannerTestJig.ViewModule.FocusAdjust;
using Azure.ScannerTestJig.ViewModule.TotalMachine;
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

namespace Azure.ScannerTestJig.View.TotalMachine
{
    /// <summary>
    /// TotalMachineViewModulecs.xaml 的交互逻辑
    /// </summary>
    public partial class TotalMachineViewModulecs : UserControl
    {
        public TotalMachineViewModulecs()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(IVContorl_Loaded);
        }
        void IVContorl_Loaded(object sender, EventArgs e)
        {

            TotalMachineViewModule vm = Workspace.This.TotalMachiVM;
            if (vm != null)
            {
                this.DataContext = vm;
                vm.InitIVControls();
            }
            MotorViewModel mv= Workspace.This.MotorVM; ;
            z_currpos.DataContext = mv;
            y_currpos.DataContext = mv;
            x_currpos.DataContext = mv;
        }
    }
}
