using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.ObjectModel;   //ObservableCollection
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using Azure.WPF.Framework;
using Azure.ImagingSystem;
using Azure.Configuration.Settings;
using Azure.LaserScanner.ViewModel;

namespace Azure.LaserScanner
{
    public class AdvancedSettingsViewModel : ViewModelBase, IPageViewModel
    {
        #region Fields...

        //private List<SampleTypeSetting> _SampleTypeFocusOptions;
        //private SampleTypeSetting _SelectedSampleType;
        private bool _IsSelected = false;
        private bool _IsEnablePhosphor = false;
        private bool _IsScanheadLocking = false;
        private RelayCommand _EnablePhosphorImagingCommand = null;
        private RelayCommand _DisablePhosphorImagingCommand = null;

        private RelayCommand _LockScanHeadCommand = null;

        #endregion

        #region Constructors...

        public AdvancedSettingsViewModel()
        {
        }

        #endregion

        public string Name
        {
            get
            {
                return "ADVANCED";
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

        public bool IsEnablePhosphor
        {
            get { return _IsEnablePhosphor; }
            set
            {
                _IsEnablePhosphor = value;
                RaisePropertyChanged("IsEnablePhosphor");
            }
        }

        #region public ICommand EnablePhosphorImagingCommand

        public ICommand EnablePhosphorImagingCommand
        {
            get
            {
                if (_EnablePhosphorImagingCommand == null)
                {
                    _EnablePhosphorImagingCommand = new RelayCommand(ExecuteEnablePhosphorImagingCommand, CanExecuteEnablePhosphorImagingCommand);
                }

                return _EnablePhosphorImagingCommand;
            }
        }

        public void ExecuteEnablePhosphorImagingCommand(object parameter)
        {
            //EL: TODO: enable phosphor imaging
            /*if (Workspace.This.ApdVM.APDTransfer.APDTransferIsAlive)
            {
                // Get the current device properties
                Workspace.This.ApdVM.APDTransfer.DevicePropertiesGet();
                System.Threading.Thread.Sleep(100);
                Hats.APDCom.DevicePropertiesStruct devProperties = Workspace.This.ApdVM.APDTransfer.DeviceProperties;
                if (devProperties.IsPhosphorImagingOn == 0)
                {
                    devProperties.IsPhosphorImagingOn = 1;  // 1 = On, 0 = off
                    Workspace.This.ApdVM.APDTransfer.DevicePropertiesSet(devProperties);

                    // Show Phosphor imaging enable/disable control
                    Workspace.This.IsPhosphorImagingVisible = true;
                }

                // Hide Phosphor imaging enable/disable control after writing to the firmware.
                foreach (var settingPage in Workspace.This.SettingsVM.PageViewModels)
                {
                    if (settingPage is AdvancedSettingsViewModel)
                    {
                        ((AdvancedSettingsViewModel)settingPage).IsEnablePhosphor = false;
                        break;
                    }
                }
            }
            else
            {
                string caption = "Sapphire...";
                string message = "Error enabling Phosphor imaging.\n" +
                                 "Please make sure you're connected to the Sapphire Biomolecular Image.";
                Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }*/
        }

        public bool CanExecuteEnablePhosphorImagingCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region public ICommand DisablePhosphorImagingCommand

        public ICommand DisablePhosphorImagingCommand
        {
            get
            {
                if (_DisablePhosphorImagingCommand == null)
                {
                    _DisablePhosphorImagingCommand = new RelayCommand(ExecuteDisablePhosphorImagingCommand, CanExecuteDisablePhosphorImagingCommand);
                }

                return _DisablePhosphorImagingCommand;
            }
        }

        public void ExecuteDisablePhosphorImagingCommand(object parameter)
        {
            //EL: TODO: disable phosphor imaging
            /*if (Workspace.This.ApdVM.APDTransfer.APDTransferIsAlive)
            {
                // Get the current device properties
                Workspace.This.ApdVM.APDTransfer.DevicePropertiesGet();
                System.Threading.Thread.Sleep(100);
                Hats.APDCom.DevicePropertiesStruct devProperties = Workspace.This.ApdVM.APDTransfer.DeviceProperties;
                if (devProperties.IsPhosphorImagingOn == 1)
                {
                    devProperties.IsPhosphorImagingOn = 0;  // 1 = On, 0 = off
                    Workspace.This.ApdVM.APDTransfer.DevicePropertiesSet(devProperties);

                    // Registry key exists; also disable it in the registry (for backward compatibility).
                    Utilities.RegistryModify regMod = new Utilities.RegistryModify();
                    string strEnablePhosphor = regMod.Read("EnablePhosphor");
                    if (!string.IsNullOrEmpty(strEnablePhosphor))
                    {
                        int retValue = 0;
                        Int32.TryParse(strEnablePhosphor, out retValue);
                        if (retValue == 1)
                        {
                            regMod.Write("EnablePhosphor", 0);
                        }
                    }

                    // Show Phosphor imaging enable/disable control
                    Workspace.This.IsPhosphorImagingVisible = false;
                }

                // Hide Phosphor imaging enable/disable control after writing to the firmware.
                foreach (var settingPage in Workspace.This.SettingsVM.PageViewModels)
                {
                    if (settingPage is AdvancedSettingsViewModel)
                    {
                        ((AdvancedSettingsViewModel)settingPage).IsEnablePhosphor = false;
                        break;
                    }
                }
            }
            else
            {
                string caption = "Sapphire...";
                string message = "Error disabling Phosphor imaging.\n" +
                                 "Please make sure you're connected to the Sapphire Biomolecular Image.";
                Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }*/
        }

        public bool CanExecuteDisablePhosphorImagingCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region LockScanHeadCommand
        public ICommand LockScanHeadCommand
        {
            get
            {
                if (_LockScanHeadCommand == null)
                {
                    _LockScanHeadCommand = new RelayCommand(ExecuteLockScanHeadCommand, CanExecuteLockScanHeadCommand);
                }
                return _LockScanHeadCommand;
            }
        }
        public void ExecuteLockScanHeadCommand(object parameter)
        {
            //EL: TODO: scan head locking
            /*if (Workspace.This.MotorVM.GalilMotor.IsAlive && !Workspace.This.MotorVM.IsXLimited)
            {
                _IsScanheadLocking = true;

                // X Max position: 330000
                Workspace.This.MotorVM.GalilMotor.SetAbsPos(GalilMotor.GalilMotorType.X, 360000, 50000);

                // Create an AutoResetEvent to signal the timeout threshold in the
                // timer callback has been reached.
                var autoEvent = new AutoResetEvent(false);
                // Create a timer that invokes ScanHeadLockStatusCheck after one second
                var shLockStatusTimer = new Timer(ScanHeadLockStatusCheck, autoEvent, 1000, 1000);
                // Wait for the signal, then dispose of the timer.
                autoEvent.WaitOne();
                shLockStatusTimer.Dispose();
            }*/
        }
        public bool CanExecuteLockScanHeadCommand(object parameter)
        {
            return (!_IsScanheadLocking);
        }

        private void ScanHeadLockStatusCheck(Object stateInfo)
        {
            if (Workspace.This.MotorVM.IsXLimited && _IsScanheadLocking)
            {
                _IsScanheadLocking = false;
                AutoResetEvent autoEvent = (AutoResetEvent)stateInfo;
                autoEvent.Set();

                Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
                {
                    string caption = "Scan head lock...";
                    string message = "The scan head is now in the locked position.\n" +
                                     "IMPORTANT: The scanner MUST be power off before relaunching the application to unlock the scan head.";
                    //var parent = Application.Current.MainWindow;
                    Xceed.Wpf.Toolkit.MessageBox.Show(Workspace.This.Owner, message, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                });
            }
        }

        #endregion LockScanHeadCommand

        //public List<SampleTypeSetting> SampleTypeFocusOptions
        //{
        //    get { return _SampleTypeFocusOptions; }
        //    set
        //    {
        //        _SampleTypeFocusOptions = value;
        //        RaisePropertyChanged("SampleTypeFocusOptions");

        //    }
        //}

        //public SampleTypeSetting SelectedSampleType
        //{
        //    get { return _SelectedSampleType; }
        //    set
        //    {
        //        if (_SelectedSampleType != value)
        //        {
        //            _SelectedSampleType = value;
        //            RaisePropertyChanged("SelectedSampleType");
        //        }
        //    }
        //}
    }
}
