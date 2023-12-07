using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input; //ICommand
using Azure.WPF.Framework;  //RelayCommand
using Azure.ImagingSystem;
using Azure.Configuration.Settings;
using System.Windows;
using Azure.EthernetCommLib;
using System.Runtime.InteropServices;

namespace Azure.ScannerEUI.ViewModel
{
    class NewParameterSetupViewModel : ViewModelBase
    {
        #region private data
        private double _XMotorSubdivision = 0;
        private double _YMotorSubdivision = 0;
        private RelayCommand _ParametersWriteCommand = null;
        private RelayCommand _ParametersReadCommand = null;
        private int _R1XLogicalHome = 0;
        private int _R1YLogicalHome = 0;
        private int _OpticalR2_R1Distance = 0;
        private int _Pixel_10_Offset_R2_R1 = 0;
        private int _OpticalL_R1Distance = 0;
        private int _Pixel_10_Offset_L_R1 = 0;
        private int _Pixel_10_L_DX;
        private int _Pixel_10_L_DY;
        private int _Pixel_10_R1_DX;
        private int _Pixel_10_R1_DY;
        private int _Pixel_10_R2_DX;
        private int _Pixel_10_R2_DY;
        private float _FocusLength=0;
        private float _XEncoderSubdivision;
        private float _FanSwitchInterval;
        private float _FanReserveTemperature;
        private string _SystemSN;
        private float _LCoefficient;
        private float _L375Coefficient;
        private float _R1Coefficient;
        private float _R2Coefficient;
        private float _R2532Coefficient;
        #endregion
        public void Initialize()
        {

            _XMotorSubdivision = SettingsManager.ConfigSettings.XMotorSubdivision;
            _YMotorSubdivision = SettingsManager.ConfigSettings.YMotorSubdivision;
            // TODO: read current parameter values
            //

            // Test values...to be replace by the real (read) values
        }
        #region public properties
        public double R1XLogicalHome
        {
            get
            {
                //double result = 0;
                //if (_XMotorSubdivision > 0)
                //    result = Math.Round((double)_R1XLogicalHome / (double)_XMotorSubdivision, 3);
                //return result;
                return _R1XLogicalHome;
            }
            set
            {
                if (_R1XLogicalHome != value)
                {
                    //_R1XLogicalHome = (int)(value * _XMotorSubdivision);
                    _R1XLogicalHome = (int)value;
                    RaisePropertyChanged("R1XLogicalHome");
                }
            }
        }
        public double R1YLogicalHome
        {
            get
            {
                //double result = 0;
                //if (_YMotorSubdivision > 0)
                //    result = Math.Round((double)_R1YLogicalHome / (double)_YMotorSubdivision, 3);
                //return result;
                return _R1YLogicalHome;
            }
            set
            {
                if (_R1YLogicalHome != value)
                {
                    //_R1YLogicalHome = (int)(value * _YMotorSubdivision);
                    _R1YLogicalHome=(int)value;
                    RaisePropertyChanged("R1YLogicalHome");
                }
            }
        }
        public double OpticalR2_R1Distance
        {
            get
            {
                //double result = 0;
                //if (_XMotorSubdivision > 0)
                //    result = Math.Round((double)_OpticalR2_R1Distance / (double)_XMotorSubdivision, 3);
                //return result;
                return _OpticalR2_R1Distance;
            }
            set
            {
                if (_OpticalR2_R1Distance != value)
                {
                    //_OpticalR2_R1Distance = (int)(value * _XMotorSubdivision);
                    _OpticalR2_R1Distance=(int)value;
                    RaisePropertyChanged("OpticalR2_R1Distance");
                }
            }
        }
        public int Pixel_10_Offset_R2_R1
        {
            get { return _Pixel_10_Offset_R2_R1; }
            set
            {
                if (_Pixel_10_Offset_R2_R1 != value)
                {
                    _Pixel_10_Offset_R2_R1 = value;
                    RaisePropertyChanged("Pixel_10_Offset_R2_R1");
                }
            }
        }
        public double OpticalL_R1Distance
        {
            get
            {
                //double result = 0;
                //if (_XMotorSubdivision > 0)
                //    result = Math.Round((double)_OpticalL_R1Distance / (double)_XMotorSubdivision, 3);
                //return result;
                return _OpticalL_R1Distance;
            }
            set
            {
                if (_OpticalL_R1Distance != value)
                {
                    //    _OpticalL_R1Distance = (int)(value * _XMotorSubdivision);
                    _OpticalL_R1Distance=(int)value;
                    RaisePropertyChanged("OpticalL_R1Distance");
                }
            }
        }
        public int Pixel_10_Offset_L_R1
        {
            get { return _Pixel_10_Offset_L_R1; }
            set
            {
                if (_Pixel_10_Offset_L_R1 != value)
                {
                    _Pixel_10_Offset_L_R1 = value;
                    RaisePropertyChanged("Pixel_10_Offset_L_R1");
                }
            }
        }
        public int Pixel_10_L_DX
        {
            get { return _Pixel_10_L_DX; }
            set
            {
                if (_Pixel_10_L_DX != value)
                {
                    _Pixel_10_L_DX = value;
                    RaisePropertyChanged("Pixel_10_L_DX");
                }
            }
        }
        public int Pixel_10_L_DY
        {
            get { return _Pixel_10_L_DY; }
            set
            {
                if (_Pixel_10_L_DY != value)
                {
                    _Pixel_10_L_DY = value;
                    RaisePropertyChanged("Pixel_10_L_DY");
                }
            }
        }
        public int Pixel_10_R1_DX
        {
            get { return _Pixel_10_R1_DX; }
            set
            {
                if (_Pixel_10_R1_DX != value)
                {
                    _Pixel_10_R1_DX = value;
                    RaisePropertyChanged("Pixel_10_R1_DX");
                }
            }
        }
        public int Pixel_10_R1_DY
        {
            get { return _Pixel_10_R1_DY; }
            set
            {
                if (_Pixel_10_R1_DY != value)
                {
                    _Pixel_10_R1_DY = value;
                    RaisePropertyChanged("Pixel_10_R1_DY");
                }
            }
        }
        public int Pixel_10_R2_DX
        {
            get { return _Pixel_10_R2_DX; }
            set
            {
                if (_Pixel_10_R2_DX != value)
                {
                    _Pixel_10_R2_DX = value;
                    RaisePropertyChanged("Pixel_10_R2_DX");
                }
            }
        }
        public int Pixel_10_R2_DY
        {
            get { return _Pixel_10_R2_DY; }
            set
            {
                if (_Pixel_10_R2_DY != value)
                {
                    _Pixel_10_R2_DY = value;
                    RaisePropertyChanged("Pixel_10_R2_DY");
                }
            }
        }
        public float FocusLength
        {
            get
            {
                return _FocusLength;
            }
            set
            {
                if (_FocusLength != value)
                {
                    _FocusLength = value;
                    RaisePropertyChanged("FocusLength");
                }
            }
        }
        public string SystemSN
        {
            get { return _SystemSN; }
            set
            {
                if (_SystemSN != value)
                {
                    _SystemSN = value;
                    RaisePropertyChanged("SystemSN");
                }
            }
        }
        public float XEncoderSubdivision
        {
            get { return _XEncoderSubdivision; }
            set
            {
                if (_XEncoderSubdivision != value)
                {
                    _XEncoderSubdivision = value;
                    RaisePropertyChanged("XEncoderSubdivision");
                }
            }
        }
        public float FanReserveTemperature
        {
            get { return _FanReserveTemperature; }
            set
            {
                if (_FanReserveTemperature != value)
                {
                    _FanReserveTemperature = value;
                    RaisePropertyChanged("FanReserveTemperature");
                }
            }
        }

        public float LCoefficient
        {
            get { return _LCoefficient; }
            set
            {
                if (_LCoefficient != value)
                {
                    _LCoefficient = value;
                    RaisePropertyChanged("LCoefficient");
                }
            }
        }

        public float L375Coefficient
        {
            get { return _L375Coefficient; }
            set
            {
                if (_L375Coefficient != value)
                {
                    _L375Coefficient = value;
                    RaisePropertyChanged("L375Coefficient");
                }
            }
        }
        public float R1Coefficient
        {
            get { return _R1Coefficient; }
            set
            {
                if (_R1Coefficient != value)
                {
                    _R1Coefficient = value;
                    RaisePropertyChanged("R1Coefficient");
                }
            }
        }
        public float R2Coefficient
        {
            get { return _R2Coefficient; }
            set
            {
                if (_R2Coefficient != value)
                {
                    _R2Coefficient = value;
                    RaisePropertyChanged("R2Coefficient");
                }
            }
        }

        public float R2532Coefficient
        {
            get { return _R2532Coefficient; }
            set
            {
                if (_R2532Coefficient != value)
                {
                    _R2532Coefficient = value;
                    RaisePropertyChanged("R2532Coefficient");
                }
            }
        }

        public float FanSwitchInterval
        {
            get { return _FanSwitchInterval; }
            set
            {
                if (_FanSwitchInterval != value)
                {
                    _FanSwitchInterval = value;
                    RaisePropertyChanged("FanSwitchInterval");
                }
            }
        }
        #endregion 

        #region ParametersWriteCommand

        public ICommand ParametersWriteCommand
        {
            get
            {
                if (_ParametersWriteCommand == null)
                {
                    _ParametersWriteCommand = new RelayCommand(ExecuteParametersWriteCommand, CanExecuteParametersWriteCommand);
                }

                return _ParametersWriteCommand;
            }
        }
        public void ExecuteParametersWriteCommand(object parameter)
        {
            AvocadoDeviceProperties deviceProperties;
            var ptr = Marshal.AllocHGlobal(256);
            deviceProperties = (AvocadoDeviceProperties)Marshal.PtrToStructure(ptr, typeof(AvocadoDeviceProperties));
            Marshal.FreeHGlobal(ptr);
            deviceProperties.LogicalHomeX = (float)R1XLogicalHome;
            deviceProperties.LogicalHomeY = (float)R1YLogicalHome;
            deviceProperties.OpticalLR1Distance = (float)OpticalR2_R1Distance;
            deviceProperties.PixelOffsetR1 = Pixel_10_Offset_R2_R1;
            deviceProperties.OpticalLR2Distance = (float)OpticalL_R1Distance;
            deviceProperties.PixelOffsetR2 = Pixel_10_Offset_L_R1;
            deviceProperties.PixelOffsetDxCHR1 = Pixel_10_R1_DX;
            deviceProperties.PixelOffsetDyCHR1 = Pixel_10_R1_DY;
            deviceProperties.PixelOffsetDxCHR2 = Pixel_10_R2_DX;
            deviceProperties.PixelOffsetDyCHR2 = Pixel_10_R2_DY;
            deviceProperties.ZFocusPosition = FocusLength;
            if (SystemSN != null)
            {
                byte[] sysSN = System.Text.Encoding.Default.GetBytes(SystemSN);
                for (int i = 0; i < deviceProperties.SysSN.Length; i++)
                {
                    if (i < sysSN.Length)
                    {
                        deviceProperties.SysSN[i] = sysSN[i];
                    }
                    else
                    {
                        deviceProperties.SysSN[i] = 0;
                    }
                }
            }
            deviceProperties.PixelOffsetDxCHL = Pixel_10_L_DX;
            deviceProperties.PixelOffsetDyCHL = Pixel_10_L_DY;
            deviceProperties.XEncoderSubdivision = XEncoderSubdivision;
            deviceProperties.FanSwitchInterval = ((int)FanSwitchInterval * 100) * 2;
            deviceProperties.FanReserveTemperature = ((int)FanReserveTemperature * 100) - ((int)FanSwitchInterval * 100);
            deviceProperties.LCoefficient = LCoefficient;
            deviceProperties.L375Coefficient = L375Coefficient;
            deviceProperties.R1Coefficient = R1Coefficient;
            deviceProperties.R2Coefficient = R2Coefficient;
            deviceProperties.R2532Coefficient = R2532Coefficient;
            if (Workspace.This.EthernetController.SetDeviceProperties(deviceProperties) == false)
            {
                MessageBox.Show("Write Failed.");
            }
            else
            {
                MessageBox.Show("Write Succeed");
                Workspace.This.ScannerVM.ScanDeltaX= Workspace.This.MotorVM.XMaxValue - Workspace.This.EthernetController.DeviceProperties.OpticalLR2Distance- (int)Workspace.This.ScannerVM.ScanX0-1;
                Workspace.This.MotorVM.LimitsXPlus = Workspace.This.MotorVM.XMaxValue - Workspace.This.EthernetController.DeviceProperties.OpticalLR2Distance;
            }
        }

        public bool CanExecuteParametersWriteCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region ParametersReadCommand

        public ICommand ParametersReadCommand
        {
            get
            {
                if (_ParametersReadCommand == null)
                {
                    _ParametersReadCommand = new RelayCommand(ExecuteParametersReadCommand, CanExecuteParametersReadCommand);
                }

                return _ParametersReadCommand;
            }
        }
        public void ExecuteParametersReadCommand(object parameter)
        {
            if (Workspace.This.EthernetController.GetDeviceProperties() == false)
            {
                MessageBox.Show("Read failed.");
                return;
            }

            R1XLogicalHome = Workspace.This.EthernetController.DeviceProperties.LogicalHomeX;
            R1YLogicalHome = Workspace.This.EthernetController.DeviceProperties.LogicalHomeY;
            OpticalR2_R1Distance = Workspace.This.EthernetController.DeviceProperties.OpticalLR1Distance;
            Pixel_10_Offset_R2_R1 = Workspace.This.EthernetController.DeviceProperties.PixelOffsetR1;
            OpticalL_R1Distance = Workspace.This.EthernetController.DeviceProperties.OpticalLR2Distance;
            Pixel_10_Offset_L_R1 = Workspace.This.EthernetController.DeviceProperties.PixelOffsetR2;
            Pixel_10_R1_DX = Workspace.This.EthernetController.DeviceProperties.PixelOffsetDxCHR1;
            Pixel_10_R1_DY = Workspace.This.EthernetController.DeviceProperties.PixelOffsetDyCHR1;
            Pixel_10_R2_DX = Workspace.This.EthernetController.DeviceProperties.PixelOffsetDxCHR2;
            Pixel_10_R2_DY = Workspace.This.EthernetController.DeviceProperties.PixelOffsetDyCHR2;
            FocusLength = Workspace.This.EthernetController.DeviceProperties.ZFocusPosition;
            SystemSN = Encoding.ASCII.GetString(Workspace.This.EthernetController.DeviceProperties.SysSN).TrimEnd('\0');
            Pixel_10_L_DX = Workspace.This.EthernetController.DeviceProperties.PixelOffsetDxCHL;
            Pixel_10_L_DY = Workspace.This.EthernetController.DeviceProperties.PixelOffsetDyCHL;
            XEncoderSubdivision = Workspace.This.EthernetController.DeviceProperties.XEncoderSubdivision;
            FanSwitchInterval = (Workspace.This.EthernetController.DeviceProperties.FanSwitchInterval / 100) / 2;
            FanReserveTemperature = (Workspace.This.EthernetController.DeviceProperties.FanReserveTemperature / 100) + FanSwitchInterval;

            try
            {
                var lCoefficient = (float)Math.Round(Workspace.This.EthernetController.DeviceProperties.LCoefficient, 3);
                LCoefficient = (lCoefficient > 0 && lCoefficient < 0.6) ? lCoefficient : 0.0F;
                var l375Coefficient = (float)Math.Round(Workspace.This.EthernetController.DeviceProperties.L375Coefficient, 3);
                L375Coefficient = (l375Coefficient > 0 && l375Coefficient < 0.6) ? l375Coefficient : 0F;
                var r1Coefficient = (float)Math.Round(Workspace.This.EthernetController.DeviceProperties.R1Coefficient, 3);
                R1Coefficient = (r1Coefficient > 0 && r1Coefficient < 0.6) ? r1Coefficient : 0F;
                var r2Coefficient = (float)Math.Round(Workspace.This.EthernetController.DeviceProperties.R2Coefficient, 3);
                R2Coefficient = (r2Coefficient > 0 && r2Coefficient < 0.6) ? r2Coefficient : 0F;
                var r2532Coefficient = (float)Math.Round(Workspace.This.EthernetController.DeviceProperties.R2532Coefficient, 3);
                R2532Coefficient = (r2532Coefficient > 0 && r2532Coefficient < 0.6) ? r2532Coefficient : 0F;
            }
            catch
            {
                LCoefficient = 0;
                L375Coefficient = 0;
                R1Coefficient = 0;
                R2Coefficient = 0;
                R2532Coefficient = 0;
            }
        }

        public bool CanExecuteParametersReadCommand(object parameter)
        {
            return true;
        }

        #endregion
    }
}
