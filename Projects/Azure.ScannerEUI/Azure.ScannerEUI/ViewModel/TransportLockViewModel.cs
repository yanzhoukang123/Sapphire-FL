using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Azure.WPF.Framework;
using System.Windows.Input;
using System.Windows;

namespace Azure.ScannerEUI.ViewModel
{
    public class TransportLockViewModel: ViewModelBase
    {
        #region Private Fields
        private RelayCommand _TransportLockCommand = null;
        private bool _IsExecuteLocking = false;
        #endregion Private Fields

        #region TransportLockCommand
        public ICommand TransportLockCommand
        {
            get
            {
                if (_TransportLockCommand == null)
                {
                    _TransportLockCommand = new RelayCommand(ExecuteTransportLockCommand, CanExecuteTransportLockCommand);
                }
                return _TransportLockCommand;
            }
        }
        public void ExecuteTransportLockCommand(object parameter)
        {
            if (!Workspace.This.MotorVM.IsMotorXEnabled || !Workspace.This.MotorIsAlive)
            {
                MessageBox.Show("Motor is not available!");
                return;
            }
            if (Workspace.This.MotorVM.IsXLimited)
            {
                MessageBox.Show("Scan head is already locked!");
                return;
            }

            Workspace.This.MotorVM.IsMotorXEnabled = false;
            Workspace.This.MotorVM.MotionController.AbsoluteMoveSingleMotion(EthernetCommLib.MotorTypes.X, 10, 40000, 80000000, 80000000, 360000, true, false);
        }
        public bool CanExecuteTransportLockCommand(object parameter)
        {
            return true;
            //return Workspace.This.MotorVM.IsMotorXEnabled && Workspace.This.MotorIsAlive && !Workspace.This.MotorVM.IsXLimited;
        }
        #endregion TransportLockCommand
    }
}
