using Azure.ScannerTestJig.ViewModule;
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
using System.Windows.Shapes;

namespace Azure.ScannerTestJig.View.TotalMachine
{
    /// <summary>
    /// TotalMachineSubWind.xaml 的交互逻辑
    /// </summary>
    public partial class TotalMachineSubWind : Window
    {
        public TotalMachineSubWind()
        {
            InitializeComponent();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            //if (Workspace.This.serialState)
            //{
            //    Workspace.This.SerialPorts.Close();
            //}
            Workspace.This.Owner.Show();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Workspace.This.TotalMachiVM.x_IsStopCheck|| Workspace.This.TotalMachiVM.y_IsStopCheck|| Workspace.This.TotalMachiVM.z_IsStopCheck|| 
                Workspace.This.TotalMachiVM.led_IsStopCheck|| Workspace.This.TotalMachiVM.IsFansBackSelected|| Workspace.This.TotalMachiVM.IsFansDrawerSelected)
            {
                string caption = "Machine Mode";
                string message = "Are you sure?";
                System.Windows.MessageBoxResult dlgResult = System.Windows.MessageBox.Show(message, caption, System.Windows.MessageBoxButton.YesNo);
                if (dlgResult == System.Windows.MessageBoxResult.No)
                {
                    e.Cancel = true; // don't allow the application to close
                    return;
                }
                TotalMachineViewModule viewModel = Workspace.This.TotalMachiVM;
                viewModel.ExecuteStopCommand(null);
                viewModel.ExecuteLedStopCommand(null);
                Workspace.This.TotalMachiVM.IsFansBackSelected = false;
                Workspace.This.TotalMachiVM.IsFansDrawerSelected = false;
                Workspace.This.TolalPowerCliVm.TurnOffAllLasers();
            }
           
        }
    }
}
