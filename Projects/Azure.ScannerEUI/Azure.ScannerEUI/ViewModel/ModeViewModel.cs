using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input; //ICommand
using Azure.CameraLib;
using Azure.WPF.Framework;  //RelayCommand
using Azure.ScannerEUI.View;
using Azure.Configuration.Settings;

namespace Azure.ScannerEUI.ViewModel
{
    class ModeViewModel
    {
        #region Private data...

        private RelayCommand _CameraModeCommand = null;
        private RelayCommand _ScannerModeCommand = null;
        private RelayCommand _SettingsCommand = null;

        #endregion

        #region Constructors...

        public ModeViewModel()
        {
        }

        #endregion

        #region Public properties...
        #endregion

        #region CameraModeCommand

        public ICommand CameraModeCommand
        {
            get
            {
                if (_CameraModeCommand == null)
                {
                    _CameraModeCommand = new RelayCommand(ExecuteCameraModeCommand, CanExecuteCameraModeCommand);
                }

                return _CameraModeCommand;
            }
        }
        public void ExecuteCameraModeCommand(object parameter)
        {
            if (Workspace.This.IsScanning)
            {
                string caption = "Switch to camera mode...";
                string message = "The system is busy scanning.\nPlease stop scanning before switching to the camera mode.";
                System.Windows.MessageBox.Show(message, caption, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Stop);
                return;
            }

            bool bAnimationStarted = false;

            if (Workspace.This.MotorVM.GalilMotor.IsAlive)
            {
                string caption = "Switch to camera mode...";
                string message = "Switch to the camera mode requires the Y stage to move.\n" +
                                 "It will first home the Y motor then move it 52mm.\n" +
                                 "Press \"OK\" to proceed";
                System.Windows.MessageBoxResult result =  System.Windows.MessageBox.Show(message, caption, System.Windows.MessageBoxButton.OKCancel, System.Windows.MessageBoxImage.Information);

                if (result == System.Windows.MessageBoxResult.Cancel) { return; }

                Workspace.This.StartWaitAnimation("Switching to camera mode...");
                bAnimationStarted = true;

                //if (Workspace.This.ApdVM.APDTransfer.APDTransferIsAlive)
                //{
                //    //Turn off the lasers.
                //    ScannerViewModel viewModel = Workspace.This.ScannerVM;
                //    if (viewModel != null)
                //    {
                //        viewModel.TurnOffAllLasers();
                //    }
                //    //use timer to avoid expection throw
                //}

                // Home the Y motor
                Workspace.This.MotorVM.GalilMotor.HomeMotor(GalilMotor.GalilMotorType.Y, true);
                Workspace.This.MotorVM.GalilMotor.HomeMotor(GalilMotor.GalilMotorType.X, true);
                System.Threading.Thread.Sleep(1000);
                // Move Y motor 52 before switching to camera mode
                Workspace.This.MotorVM.GalilMotor.SetAbsPos(GalilMotor.GalilMotorType.Y,
                    (int)(52 * SettingsManager.ConfigSettings.YMotorSubdivision),
                    (int)(6 * SettingsManager.ConfigSettings.YMotorSubdivision), true);
                System.Threading.Thread.Sleep(1000);
                Workspace.This.MotorVM.GalilMotor.Disconnect();
                //Workspace.This.MotorVM.GalilMotor.XMotorDisable();
                System.Threading.Thread.Sleep(1000);
            }

            if (!bAnimationStarted)
            {
                Workspace.This.StartWaitAnimation("Switching to camera mode...");
            }

            //if (Workspace.This.ApdVM.APDTransfer.APDTransferIsAlive)
            //{
            //    Workspace.This.ApdVM.APDTransfer.APDLaserTurnCCD();
            //    System.Threading.Thread.Sleep(6000);//Warning!This must be keep for turning to mode of CCD.You can take a loading picture fot it.
            //}

            Workspace.This.StopWaitAnimation();
            Workspace.This.MotorIsAlive = false;
            Workspace.This.IsCameraMode = true;
        }

        public bool CanExecuteCameraModeCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region ScannerModeCommand

        public ICommand ScannerModeCommand
        {
            get
            {
                if (_ScannerModeCommand == null)
                {
                    _ScannerModeCommand = new RelayCommand(ExecuteScannerModeCommand, CanExecuteScannerModeCommand);
                }

                return _ScannerModeCommand;
            }
        }
        public void ExecuteScannerModeCommand(object parameter)
        {
            if (Workspace.This.IsCapturing || Workspace.This.IsContinuous)
            {
                string caption = "Camera Mode";
                string message = "Camera mode is busy.\nWould you like to terminate the current operation?";
                System.Windows.MessageBoxResult dlgResult = System.Windows.MessageBox.Show(message, caption, System.Windows.MessageBoxButton.YesNo);
                if (dlgResult == System.Windows.MessageBoxResult.No)
                {
                    return;
                }

                
                string content = string.Empty;
                if (Workspace.This.IsCapturing)
                {
                    content = "Aborting image capture...";
                }
                else if (Workspace.This.IsContinuous)
                {
                    content = "Aborting live mode...";
                }

                Workspace.This.StartWaitAnimation(content);

                CameraViewModel viewModel = Workspace.This.CameraVM;

                if (Workspace.This.IsCapturing)
                {
                    viewModel.ExecuteStopCaptureCommand(null);
                }
                else
                {
                    viewModel.ExecuteStopContinuousCommand(null);
                }

                Workspace.This.StopWaitAnimation();
            }
            //close all led
            Workspace.This.CameraVM.IsLedBlueSelected = false;
            Workspace.This.CameraVM.IsLedGreenSelected = false;
            Workspace.This.CameraVM.IsLedRedSelected = false;
            System.Threading.Thread.Sleep(100);
            Workspace.This.StartWaitAnimation("Switching to scanner mode...");

            //if (Workspace.This.ApdVM.APDTransfer.APDTransferIsAlive)
            //{
            //    Workspace.This.ApdVM.APDTransfer.APDLaserCloseEX();
            //    Workspace.This.ApdVM.APDTransfer.APDLaserTurnAPD();
            //    System.Threading.Thread.Sleep(3000);//Warning!This must be keep for turning to mode of APD.You can take a loading picture fot it.

            //    // If the application starts up in camera mode - the Galil motor was not connected.
            //    // We need to connect to the Galil motor when switching to scanner mode.
            //    if (!Workspace.This.MotorVM.GalilMotor.IsAlive)
            //    {
            //        Workspace.This.MotorVM.GalilMotor.Connect();
            //    }
            //    else
            //    {
            //        Workspace.This.MotorVM.GalilMotor.IsActive = true;
            //        Workspace.This.MotorVM.GalilMotor.HomeMotor(GalilMotor.GalilMotorType.Y, true);
            //        Workspace.This.MotorVM.GalilMotor.HomeMotor(GalilMotor.GalilMotorType.X, true);
            //        Workspace.This.MotorVM.GalilMotor.HomeMotor(GalilMotor.GalilMotorType.Z, true);
            //    }
            //}
            Workspace.This.StopWaitAnimation();
            Workspace.This.MotorIsAlive = true;
            Workspace.This.IsScannerMode = true;
        }

        public bool CanExecuteScannerModeCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region SettingsCommand

        public ICommand SettingsCommand
        {
            get
            {
                if (_SettingsCommand == null)
                {
                    _SettingsCommand = new RelayCommand(ExecuteSettingsCommand, CanExecuteSettingsCommand);
                }

                return _SettingsCommand;
            }
        }
        public void ExecuteSettingsCommand(object parameter)
        {
            if (Workspace.This.IsCapturing || Workspace.This.IsContinuous)
            {
                string caption = "Camera Mode";
                string message = "Camera mode is busy.\nWould you like to terminate the current operation?";
                System.Windows.MessageBoxResult dlgResult = System.Windows.MessageBox.Show(message, caption, System.Windows.MessageBoxButton.YesNo);
                if (dlgResult == System.Windows.MessageBoxResult.No)
                {
                    return;
                }

                CameraViewModel viewModel = Workspace.This.CameraVM;
                if (Workspace.This.IsCapturing)
                {
                    viewModel.ExecuteStopCaptureCommand(null);
                }
                else
                {
                    viewModel.ExecuteStopContinuousCommand(null);
                }
            }

            ParameterSetup paramSetupWin = new ParameterSetup();
            // Needed for centering this dialogbox in the center of the parent window
            paramSetupWin.Owner = Workspace.This.Owner;
            paramSetupWin.ShowDialog();
        }

        public bool CanExecuteSettingsCommand(object parameter)
        {
            return true;
        }

        #endregion

    }
}
