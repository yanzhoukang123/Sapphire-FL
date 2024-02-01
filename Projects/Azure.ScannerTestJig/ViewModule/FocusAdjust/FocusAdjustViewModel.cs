using Azure.WPF.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Azure.ScannerTestJig.ViewModule
{
    class FocusAdjustViewModel : ViewModelBase
    {
        #region privert        
        private RelayCommand _FocusAdjustCommand = null;
        #endregion
        public FocusAdjustViewModel() { 
        
        
        }
        public  void InitIVControls() { 
        
        
        }

























        #region Focus Adjust Command
        public ICommand FocusAdjustCommand
        {
            get
            {
                if (_FocusAdjustCommand == null)
                {
                    _FocusAdjustCommand = new RelayCommand(ExcuteFocusAdjustCommand, CanExcuteFocusAdjustCommand);
                }
                return _FocusAdjustCommand;
            }
        }
        public void ExcuteFocusAdjustCommand(object parameter)
        {
            //if (!_APDTransfer.APDTransferIsAlive)
            //{
            //    MessageBox.Show("未发现USB设备，请确认连接！");
            //    return;
            //}
            //if (!Workspace.This.TotalMachineVM.GalilMotor.IsActive)
            //{
            //    MessageBox.Show("电机错误，请确认连接！");
            //    return;
            //}
            //_FocusAdjustWind = new FocusAdjustSubWind();
            //_FocusAdjustWind.ShowDialog();
        }
        public bool CanExcuteFocusAdjustCommand(object parameter)
        {
            return true;
        }
        #endregion
    }
}
