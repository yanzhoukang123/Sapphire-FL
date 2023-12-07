using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Azure.Configuration.Settings;
using Azure.EthernetCommLib;
using Azure.LaserScanner.ViewModel;
using Azure.WPF.Framework;

namespace Azure.LaserScanner
{
    class GeneralSettingsViewModel : ViewModelBase, IPageViewModel
    {
        private bool _IsSelected = false;
        private bool _IsAddFocus = false;
        private bool _IsEditFocus = false;
        private FocusSettingsViewModel _FocusSettingsVm = null;
        private AddFilterViewModel _AddFilterVm = null;
        private GeneralAppSettingsViewModel _GenAppSettingsVm = null;

        public GeneralSettingsViewModel()
        {
            _FocusSettingsVm = new FocusSettingsViewModel();
            _AddFilterVm = new AddFilterViewModel();
            _GenAppSettingsVm = new GeneralAppSettingsViewModel();
        }

        public string Name
        {
            get
            {
                return "GENERAL";
            }
        }

        public bool IsSelected
        {
            get { return _IsSelected; }
            set
            {
                _IsSelected = value;
                RaisePropertyChanged("IsSelected");
            }
        }

        public bool IsAddFocus
        {
            get { return _IsAddFocus; }
            set
            {
                _IsAddFocus = value;
                RaisePropertyChanged("IsAddFocus");
                if (_IsAddFocus)
                {
                    IsEditFocus = false;
                }
            }
        }

        public bool IsEditFocus
        {
            get { return _IsEditFocus; }
            set
            {
                _IsEditFocus = value;
                RaisePropertyChanged("IsEditFocus");
                if (_IsEditFocus)
                {
                    IsAddFocus = false;
                }
            }
        }

        public FocusSettingsViewModel FocusSettingsVm
        {
            get { return _FocusSettingsVm; }
        }

        public AddFilterViewModel AddFilterVm
        {
            get { return _AddFilterVm; }
        }

        public GeneralAppSettingsViewModel GenAppSettingsVm
        {
            get { return _GenAppSettingsVm; }
        }

        #region HomeScanheadCommand

        private RelayCommand _HomeScanheadCommand = null;
        public ICommand HomeScanheadCommand
        {
            get
            {
                if (_HomeScanheadCommand == null)
                {
                    _HomeScanheadCommand = new RelayCommand(ExecuteHomeScanheadCommand, CanExecuteHomeScanheadCommand);
                }
                return _HomeScanheadCommand;
            }
        }
        protected void ExecuteHomeScanheadCommand(object parameter)
        {
            if (!Workspace.This.EthernetController.IsConnected ||
                SettingsManager.ConfigSettings.IsSimulationMode)
            {
                return;
            }

            Workspace.This.StartWaitAnimation("Homing the scan head. Please wait...");

            if (!Workspace.This.MotorVM.HomeXYZmotor())
            {
                string caption = "Sapphire FL Biomolecular Imager";
                string message = "Error homing the scan head motors.\n" +
                                "Please restart the application to home the scan head.";
                Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }
            bool bIsXYZmotorHoming = true;
            while (bIsXYZmotorHoming)
            {
                System.Threading.Thread.Sleep(500);

                if (Workspace.This.MotorVM.MotionController.CrntState[MotorTypes.X].AtHome &&
                    Workspace.This.MotorVM.MotionController.CrntState[MotorTypes.Y].AtHome &&
                    Workspace.This.MotorVM.MotionController.CrntState[MotorTypes.Z].AtHome)
                {
                    Workspace.This.StopWaitAnimation();

                    bIsXYZmotorHoming = false;
                    Workspace.This.LogMessage("Homing X/Y/Z motors: Succeeded!");

                    string caption = "Sapphire FL Biomolecular Imager";
                    string message = "The scan head is successfully homed.";
                    Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    bIsXYZmotorHoming = true;
                }
            }
        }
        protected bool CanExecuteHomeScanheadCommand(object parameter)
        {
            return true;
        }

        #endregion
    }
}
