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
    public class ParameterSetupViewModel : ViewModelBase
    {
        #region Private data...

        private int _XLogicalHome = 0;
        private int _YLogicalHome = 0;

        private int _OpticalModuleDistance = 0;

        private int _Pixel_10_Offset = 0;
        private int _Pixel_10_ChannelA_DX;//
        private int _Pixel_10_ChannelA_DY;
        private int _Pixel_10_ChannelB_DX;
        private int _Pixel_10_ChannelB_DY;
        private int _Pixel_10_ChannelD_DX;
        private int _Pixel_10_ChannelD_DY;

        private byte _IsPhosphorImagingOn;

        private int _IRIntensityAt5mW;
        private int _IRIntensityAt10mW;
        private int _IRIntensityAt15mW;
        private int _IRIntensityAt20mW;
        private int _GreenIntensityAt5mW;
        private int _GreenIntensityAt10mW;
        private int _GreenIntensityAt15mW;
        private int _GreenIntensityAt20mW;
        private int _RedIntensityAt5mW;
        private int _RedIntensityAt10mW;
        private int _RedIntensityAt15mW;
        private int _RedIntensityAt20mW;
        private int _BlueIntensityAt5mW;
        private int _BlueIntensityAt10mW;
        private int _BlueIntensityAt15mW;
        private int _BlueIntensityAt20mW;
        private float _FocusLength;
        private string _SystemSN;
        private int _PMTCompensation;

        private int _LaserAMaxTime = 0;
        private int _LaserBMaxTime = 0;
        private int _LaserCMaxTime = 0;
        private int _LaserDMaxTime = 0;

        private int _ApdAMaxGain = 0;
        private int _ApdBMaxGain = 0;
        private int _ApdCMaxGain = 0;
        private int _ApdDMaxGain = 0;

        private int _MaxResolution = 0;
        private int _MinResolution = 0;

        private int _MotorXMaxSpeed = 0;
        private int _MotorYMaxSpeed = 0;
        private int _MotorZMaxSpeed = 0;

        private int _MotorXMaxAccel = 0;
        private int _MotorYMaxAccel = 0;
        private int _MotorZMaxAccel = 0;

        private double _XMotorSubdivision = 0;
        private double _YMotorSubdivision = 0;

        private RelayCommand _ParametersWriteCommand = null;
        private RelayCommand _ParametersReadCommand = null;
        private RelayCommand _LaserMaxTimeSetupCommand = null;
        private RelayCommand _ApdMaxGainSetupCommand = null;
        //private RelayCommand _MaxResolutionSetupCommand = null;
        //private RelayCommand _MinResolutionSetupCommand = null;
        private RelayCommand _ResolutionSetupCommand = null;
        private RelayCommand _MotorMaxSpeedSetupCommand = null;
        private RelayCommand _MotorMaxAccelSetupCommand = null;

        #endregion

        #region Constructors...

        public ParameterSetupViewModel()
        {
            //_XMotorSubdivision = SettingsManager.ConfigSettings.XMotorSubdivision;
            //_YMotorSubdivision = SettingsManager.ConfigSettings.YMotorSubdivision;
        }

        #endregion

        #region Public properties...

        public double XLogicalHome
        {
            get
            {
                double result = 0;
                if (_XMotorSubdivision > 0)
                    result = Math.Round((double)_XLogicalHome / (double)_XMotorSubdivision,3);
                return result;
            }
            set
            {
                if (_XLogicalHome != value)
                {
                    _XLogicalHome = (int)(value * _XMotorSubdivision);
                    RaisePropertyChanged("XLogicalHome");
                }
            }
        }
        public double YLogicalHome
        {
            get
            {
                double result = 0;
                if (_YMotorSubdivision > 0)
                    result = Math.Round((double)_YLogicalHome / (double)_YMotorSubdivision,3);
                return result;
            }
            set
            {
                if (_YLogicalHome != value)
                {
                    _YLogicalHome = (int)(value * _YMotorSubdivision);
                    RaisePropertyChanged("YLogicalHome");
                }
            }
        }
        public double OpticalModuleDistance
        {
            get
            {
                double result = 0;
                if (_XMotorSubdivision > 0)
                    result = Math.Round((double)_OpticalModuleDistance / (double)_XMotorSubdivision,3);
                return result;
            }
            set
            {
                if (_OpticalModuleDistance != value)
                {
                    _OpticalModuleDistance = (int)(value * _XMotorSubdivision);
                    RaisePropertyChanged("OpticalModuleDistance");
                }
            }
        }

        public int Pixel_10_Offset
        {
            get { return _Pixel_10_Offset; }
            set
            {
                if (_Pixel_10_Offset != value)
                {
                    _Pixel_10_Offset = value;
                    RaisePropertyChanged("Pixel_10_Offset");
                }
            }
        }

        public int Pixel_10_ChannelA_DX
        {
            get { return _Pixel_10_ChannelA_DX; }
            set
            {
                if (_Pixel_10_ChannelA_DX != value)
                {
                    _Pixel_10_ChannelA_DX = value;
                    RaisePropertyChanged("Pixel_10_ChannelA_DX");
                }
            }
        }

        public int Pixel_10_ChannelA_DY
        {
            get { return _Pixel_10_ChannelA_DY; }
            set
            {
                if (_Pixel_10_ChannelA_DY != value)
                {
                    _Pixel_10_ChannelA_DY = value;
                    RaisePropertyChanged("Pixel_10_ChannelA_DY");
                }
            }
        }

        public int Pixel_10_ChannelB_DX
        {
            get { return _Pixel_10_ChannelB_DX; }
            set
            {
                if (_Pixel_10_ChannelB_DX != value)
                {
                    _Pixel_10_ChannelB_DX = value;
                    RaisePropertyChanged("Pixel_10_ChannelB_DX");
                }
            }
        }

        public int Pixel_10_ChannelB_DY
        {
            get { return _Pixel_10_ChannelB_DY; }
            set
            {
                if (_Pixel_10_ChannelB_DY != value)
                {
                    _Pixel_10_ChannelB_DY = value;
                    RaisePropertyChanged("Pixel_10_ChannelB_DY");
                }
            }
        }

        public int Pixel_10_ChannelD_DX
        {
            get { return _Pixel_10_ChannelD_DX; }
            set
            {
                if (_Pixel_10_ChannelD_DX != value)
                {
                    _Pixel_10_ChannelD_DX = value;
                    RaisePropertyChanged("Pixel_10_ChannelD_DX");
                }
            }
        }

        public int Pixel_10_ChannelD_DY
        {
            get { return _Pixel_10_ChannelD_DY; }
            set
            {
                if (_Pixel_10_ChannelD_DY != value)
                {
                    _Pixel_10_ChannelD_DY = value;
                    RaisePropertyChanged("Pixel_10_ChannelD_DY");
                }
            }
        }

        public byte IsPhosphorImagingOn
        {
            get { return _IsPhosphorImagingOn; }
            set
            {
                if (_IsPhosphorImagingOn != value)
                {
                    _IsPhosphorImagingOn = value;
                    RaisePropertyChanged("IsPhosphorImagingOn");
                }
            }
        }

        public int IRIntensityAt5mW
        {
            get { return _IRIntensityAt5mW; }
            set
            {
                if (_IRIntensityAt5mW != value)
                {
                    if (value < 1000 || value > 14500)
                    {
                        MessageBox.Show("Intensities must be set within 1000 to 14500!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        _IRIntensityAt5mW = value;
                    }
                    RaisePropertyChanged("IRIntensityAt5mW");
                }
            }
        }
        public int IRIntensityAt10mW
        {
            get { return _IRIntensityAt10mW; }
            set
            {
                if (_IRIntensityAt10mW != value)
                {
                    if (value < 1000 || value > 14500)
                    {
                        MessageBox.Show("Intensities must be set within 1000 to 14500!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        _IRIntensityAt10mW = value;
                    }
                    RaisePropertyChanged("IRIntensityAt10mW");
                }
            }
        }
        public int IRIntensityAt15mW
        {
            get { return _IRIntensityAt15mW; }
            set
            {
                if (_IRIntensityAt15mW != value)
                {
                    if (value < 1000 || value > 14500)
                    {
                        MessageBox.Show("Intensities must be set within 1000 to 14500!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        _IRIntensityAt15mW = value;
                    }
                    RaisePropertyChanged("IRIntensityAt15mW");
                }
            }
        }
        public int IRIntensityAt20mW
        {
            get { return _IRIntensityAt20mW; }
            set
            {
                if (_IRIntensityAt20mW != value)
                {
                    if (value < 1000 || value > 14500)
                    {
                        MessageBox.Show("Intensities must be set within 1000 to 14500!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        _IRIntensityAt20mW = value;
                    }
                    RaisePropertyChanged("IRIntensityAt20mW");
                }
            }
        }
        public int GreenIntensityAt5mW
        {
            get { return _GreenIntensityAt5mW; }
            set
            {
                if (_GreenIntensityAt5mW != value)
                {
                    if (value < 1000 || value > 14500)
                    {
                        MessageBox.Show("Intensities must be set within 1000 to 14500!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        _GreenIntensityAt5mW = value;
                    }
                    RaisePropertyChanged("GreenIntensityAt5mW");
                }
            }
        }
        public int GreenIntensityAt10mW
        {
            get { return _GreenIntensityAt10mW; }
            set
            {
                if (_GreenIntensityAt10mW != value)
                {
                    if (value < 1000 || value > 14500)
                    {
                        MessageBox.Show("Intensities must be set within 1000 to 14500!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        _GreenIntensityAt10mW = value;
                    }
                    RaisePropertyChanged("GreenIntensityAt10mW");
                }
            }
        }
        public int GreenIntensityAt15mW
        {
            get { return _GreenIntensityAt15mW; }
            set
            {
                if (_GreenIntensityAt15mW != value)
                {
                    if (value < 1000 || value > 14500)
                    {
                        MessageBox.Show("Intensities must be set within 1000 to 14500!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        _GreenIntensityAt15mW = value;
                    }
                    RaisePropertyChanged("GreenIntensityAt15mW");
                }
            }
        }
        public int GreenIntensityAt20mW
        {
            get { return _GreenIntensityAt20mW; }
            set
            {
                if (_GreenIntensityAt20mW != value)
                {
                    if (value < 1000 || value > 14500)
                    {
                        MessageBox.Show("Intensities must be set within 1000 to 14500!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        _GreenIntensityAt20mW = value;
                    }
                    RaisePropertyChanged("GreenIntensityAt20mW");
                }
            }
        }
        public int RedIntensityAt5mW
        {
            get { return _RedIntensityAt5mW; }
            set
            {
                if (_RedIntensityAt5mW != value)
                {
                    if (value < 1000 || value > 14500)
                    {
                        MessageBox.Show("Intensities must be set within 1000 to 14500!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        _RedIntensityAt5mW = value;
                    }
                    RaisePropertyChanged("RedIntensityAt5mW");
                }
            }
        }
        public int RedIntensityAt10mW
        {
            get { return _RedIntensityAt10mW; }
            set
            {
                if (_RedIntensityAt10mW != value)
                {
                    if (value < 1000 || value > 14500)
                    {
                        MessageBox.Show("Intensities must be set within 1000 to 14500!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        _RedIntensityAt10mW = value;
                    }
                    RaisePropertyChanged("RedIntensityAt10mW");
                }
            }
        }
        public int RedIntensityAt15mW
        {
            get { return _RedIntensityAt15mW; }
            set
            {
                if (_RedIntensityAt15mW != value)
                {
                    if (value < 1000 || value > 14500)
                    {
                        MessageBox.Show("Intensities must be set within 1000 to 14500!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        _RedIntensityAt15mW = value;
                    }
                    RaisePropertyChanged("RedIntensityAt15mW");
                }
            }
        }
        public int RedIntensityAt20mW
        {
            get { return _RedIntensityAt20mW; }
            set
            {
                if (_RedIntensityAt20mW != value)
                {
                    if (value < 1000 || value > 14500)
                    {
                        MessageBox.Show("Intensities must be set within 1000 to 14500!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        _RedIntensityAt20mW = value;
                    }
                    RaisePropertyChanged("RedIntensityAt20mW");
                }
            }
        }
        public int BlueIntensityAt5mW
        {
            get { return _BlueIntensityAt5mW; }
            set
            {
                if (_BlueIntensityAt5mW != value)
                {
                    if (value < 1000 || value > 14500)
                    {
                        MessageBox.Show("Intensities must be set within 1000 to 14500!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        _BlueIntensityAt5mW = value;
                    }
                    RaisePropertyChanged("BlueIntensityAt5mW");
                }
            }
        }
        public int BlueIntensityAt10mW
        {
            get { return _BlueIntensityAt10mW; }
            set
            {
                if (_BlueIntensityAt10mW != value)
                {
                    if (value < 1000 || value > 14500)
                    {
                        MessageBox.Show("Intensities must be set within 1000 to 14500!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        _BlueIntensityAt10mW = value;
                    }
                    RaisePropertyChanged("BlueIntensityAt10mW");
                }
            }
        }
        public int BlueIntensityAt15mW
        {
            get { return _BlueIntensityAt15mW; }
            set
            {
                if (_BlueIntensityAt15mW != value)
                {
                    if (value < 1000 || value > 14500)
                    {
                        MessageBox.Show("Intensities must be set within 1000 to 14500!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        _BlueIntensityAt15mW = value;
                    }
                    RaisePropertyChanged("BlueIntensityAt15mW");
                }
            }
        }
        public int BlueIntensityAt20mW
        {
            get { return _BlueIntensityAt20mW; }
            set
            {
                if (_BlueIntensityAt20mW != value)
                {
                    if (value < 1000 || value > 14500)
                    {
                        MessageBox.Show("Intensities must be set within 1000 to 14500!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        _BlueIntensityAt20mW = value;
                    }
                    RaisePropertyChanged("BlueIntensityAt20mW");
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
        public int PMTCompensation
        {
            get { return _PMTCompensation; }
            set
            {
                if (_PMTCompensation != value)
                {
                    _PMTCompensation = value;
                    RaisePropertyChanged("PMTCompensation");
                }
            }
        }
        public int LaserAMaxTime
        {
            get { return _LaserAMaxTime; }
            set
            {
                if (_LaserAMaxTime != value)
                {
                    _LaserAMaxTime = value;
                    RaisePropertyChanged("LaserAMaxTime");
                }
            }
        }
        public int LaserBMaxTime
        {
            get { return _LaserBMaxTime; }
            set
            {
                if (_LaserBMaxTime != value)
                {
                    _LaserBMaxTime = value;
                    RaisePropertyChanged("LaserBMaxTime");
                }
            }
        }
        public int LaserCMaxTime
        {
            get { return _LaserCMaxTime; }
            set
            {
                if (_LaserCMaxTime != value)
                {
                    _LaserCMaxTime = value;
                    RaisePropertyChanged("LaserCMaxTime");
                }
            }
        }
        public int LaserDMaxTime
        {
            get { return _LaserDMaxTime; }
            set
            {
                if (_LaserDMaxTime != value)
                {
                    _LaserDMaxTime = value;
                    RaisePropertyChanged("LaserDMaxTime");
                }
            }
        }

        public int ApdAMaxGain
        {
            get { return _ApdAMaxGain; }
            set
            {
                if (_ApdAMaxGain != value)
                {
                    _ApdAMaxGain = value;
                    RaisePropertyChanged("ApdAMaxGain");
                }
            }
        }
        public int ApdBMaxGain
        {
            get { return _ApdBMaxGain; }
            set
            {
                if (_ApdBMaxGain != value)
                {
                    _ApdBMaxGain = value;
                    RaisePropertyChanged("ApdBMaxGain");
                }
            }
        }
        public int ApdCMaxGain
        {
            get { return _ApdCMaxGain; }
            set
            {
                if (_ApdCMaxGain != value)
                {
                    _ApdCMaxGain = value;
                    RaisePropertyChanged("ApdCMaxGain");
                }
            }
        }
        public int ApdDMaxGain
        {
            get { return _ApdDMaxGain; }
            set
            {
                if (_ApdDMaxGain != value)
                {
                    _ApdDMaxGain = value;
                    RaisePropertyChanged("ApdDMaxGain");
                }
            }
        }

        public int MaxResolution
        {
            get { return _MaxResolution; }
            set
            {
                if (_MaxResolution != value)
                {
                    _MaxResolution = value;
                    RaisePropertyChanged("MaxResolution");
                }
            }
        }
        public int MinResolution
        {
            get { return _MinResolution; }
            set
            {
                if (_MinResolution != value)
                {
                    _MinResolution = value;
                    RaisePropertyChanged("MinResolution");
                }
            }
        }

        public int MotorXMaxSpeed
        {
            get { return _MotorXMaxSpeed; }
            set
            {
                if (_MotorXMaxSpeed != value)
                {
                    _MotorXMaxSpeed = value;
                    RaisePropertyChanged("MotorXMaxSpeed");
                }
            }
        }
        public int MotorYMaxSpeed
        {
            get { return _MotorYMaxSpeed; }
            set
            {
                if (_MotorYMaxSpeed != value)
                {
                    _MotorYMaxSpeed = value;
                    RaisePropertyChanged("MotorYMaxSpeed");
                }
            }
        }
        public int MotorZMaxSpeed
        {
            get { return _MotorZMaxSpeed; }
            set
            {
                if (_MotorZMaxSpeed != value)
                {
                    _MotorZMaxSpeed = value;
                    RaisePropertyChanged("MotorZMaxSpeed");
                }
            }
        }

        public int MotorXMaxAccel
        {
            get { return _MotorXMaxAccel; }
            set
            {
                if (_MotorXMaxAccel != value)
                {
                    _MotorXMaxAccel = value;
                    RaisePropertyChanged("MotorXMaxAccel");
                }
            }
        }
        public int MotorYMaxAccel
        {
            get { return _MotorYMaxAccel; }
            set
            {
                if (_MotorYMaxAccel != value)
                {
                    _MotorYMaxAccel = value;
                    RaisePropertyChanged("MotorYMaxAccel");
                }
            }
        }
        public int MotorZMaxAccel
        {
            get { return _MotorZMaxAccel; }
            set
            {
                if (_MotorZMaxAccel != value)
                {
                    _MotorZMaxAccel = value;
                    RaisePropertyChanged("MotorZMaxAccel");
                }
            }
        }

        #endregion

        /// <summary>
        /// Read current parameters.
        /// </summary>
        /// 
        public void Initialize()
        {
            _XMotorSubdivision = SettingsManager.ConfigSettings.XMotorSubdivision;
            _YMotorSubdivision = SettingsManager.ConfigSettings.YMotorSubdivision;

            //
            // TODO: read current parameter values
            //

            // Test values...to be replace by the real (read) values
        }

        #region Commands...

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
            deviceProperties.LogicalHomeX = (float)XLogicalHome;
            deviceProperties.LogicalHomeY = (float)YLogicalHome;
            deviceProperties.OpticalLR1Distance = (float)OpticalModuleDistance;
            deviceProperties.PixelOffsetR1 = Pixel_10_Offset;
            deviceProperties.PixelOffsetDxCHR1 = Pixel_10_ChannelA_DX;
            deviceProperties.PixelOffsetDyCHR1 = Pixel_10_ChannelA_DY;
            deviceProperties.PixelOffsetDxCHR2 = Pixel_10_ChannelB_DX;
            deviceProperties.PixelOffsetDyCHR2 = Pixel_10_ChannelB_DY;
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
            if(Workspace.This.EthernetController.SetDeviceProperties(deviceProperties)==false)
            {
                MessageBox.Show("Write Failed.");
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
            if (Workspace.This.EthernetController.GetDeviceProperties()==false)
            {
                MessageBox.Show("Read failed.");
                return;
            }

            XLogicalHome = Workspace.This.EthernetController.DeviceProperties.LogicalHomeX;
            YLogicalHome = Workspace.This.EthernetController.DeviceProperties.LogicalHomeY;
            OpticalModuleDistance = Workspace.This.EthernetController.DeviceProperties.OpticalLR1Distance;
            Pixel_10_Offset = Workspace.This.EthernetController.DeviceProperties.PixelOffsetR1;
            Pixel_10_ChannelA_DX = Workspace.This.EthernetController.DeviceProperties.PixelOffsetDxCHR1;
            Pixel_10_ChannelA_DY = Workspace.This.EthernetController.DeviceProperties.PixelOffsetDyCHR1;
            Pixel_10_ChannelB_DX = Workspace.This.EthernetController.DeviceProperties.PixelOffsetDxCHR2;
            Pixel_10_ChannelB_DY = Workspace.This.EthernetController.DeviceProperties.PixelOffsetDyCHR2;
            FocusLength = Workspace.This.EthernetController.DeviceProperties.ZFocusPosition;
            SystemSN = Encoding.ASCII.GetString(Workspace.This.EthernetController.DeviceProperties.SysSN).TrimEnd('\0');
        }

        public bool CanExecuteParametersReadCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region LaserMaxTimeSetupCommand

        public ICommand LaserMaxTimeSetupCommand
        {
            get
            {
                if (_LaserMaxTimeSetupCommand == null)
                {
                    _LaserMaxTimeSetupCommand = new RelayCommand(ExecuteLaserMaxTimeSetupCommand, CanExecuteLaserMaxTimeSetupCommand);
                }

                return _LaserMaxTimeSetupCommand;
            }
        }
        public void ExecuteLaserMaxTimeSetupCommand(object parameter)
        {
            //
            //TODO: implement the command
            //

            LaserType laserType = (LaserType)parameter;
            if (laserType == LaserType.LaserA)
            {
                //TODO: Set the laser A max time
                System.Windows.MessageBox.Show(string.Format("TODO: Set laser A max time : {0}", LaserAMaxTime));

            }
            else if (laserType == LaserType.LaserB)
            {
                //TODO: Set the laser B max time
                System.Windows.MessageBox.Show(string.Format("TODO: Set laser B max time : {0}", LaserBMaxTime));
            }
            else if (laserType == LaserType.LaserC)
            {
                //TODO: Set the laser C max time
                System.Windows.MessageBox.Show(string.Format("TODO: Set laser C max time : {0}", LaserCMaxTime));
            }
            else if (laserType == LaserType.LaserD)
            {
                //TODO: Set the laser D max time
                System.Windows.MessageBox.Show(string.Format("TODO: Set laser D max time : {0}", LaserDMaxTime));
            }
        }

        public bool CanExecuteLaserMaxTimeSetupCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region ApdMaxGainSetupCommand

        public ICommand ApdMaxGainSetupCommand
        {
            get
            {
                if (_ApdMaxGainSetupCommand == null)
                {
                    _ApdMaxGainSetupCommand = new RelayCommand(ExecuteApdMaxGainSetupCommand, CanExecuteApdMaxGainSetupCommand);
                }

                return _ApdMaxGainSetupCommand;
            }
        }
        public void ExecuteApdMaxGainSetupCommand(object parameter)
        {
            //
            //TODO: implement the command
            //

            ApdType apdType = (ApdType)parameter;

            if (apdType == ApdType.ApdA)
            {
                //TODO: Set the APD A max gain
                //System.Windows.MessageBox.Show(string.Format("TODO: Set APD A max gain : {0}", ApdAMaxGain));
            }
            else if (apdType == ApdType.ApdB)
            {
                //TODO: Set the APD B max gain
                //System.Windows.MessageBox.Show(string.Format("TODO: Set APD B max gain : {0}", ApdBMaxGain));
            }
            else if (apdType == ApdType.ApdC)
            {
                //TODO: Set the APD C max gain
                //System.Windows.MessageBox.Show(string.Format("TODO: Set APD C max gain : {0}", ApdCMaxGain));
            }
            else if (apdType == ApdType.ApdD)
            {
                //TODO: Set the APD D max gain
                //System.Windows.MessageBox.Show(string.Format("TODO: Set APD D max gain : {0}", ApdDMaxGain));
            }
        }

        public bool CanExecuteApdMaxGainSetupCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region ResolutionSetupCommand

        public ICommand ResolutionSetupCommand
        {
            get
            {
                if (_ResolutionSetupCommand == null)
                {
                    _ResolutionSetupCommand = new RelayCommand(ExecuteResolutionSetupCommand, CanExecuteResolutionSetupCommand);
                }

                return _ResolutionSetupCommand;
            }
        }
        public void ExecuteResolutionSetupCommand(object parameter)
        {
            //
            //TODO: implement the command
            //

            string resType = parameter as string;

            if (resType.Equals("Maximum"))
            {
                //TODO: Set maximum resolution
                //System.Windows.MessageBox.Show(string.Format("TODO: Set maximum resolution : {0}", MaxResolution));
            }
            else if (resType.Equals("Minimum"))
            {
                //TODO: Set minimum resolution
                //System.Windows.MessageBox.Show(string.Format("TODO: Set minimum resolution : {0}", MinResolution));
            }
        }

        public bool CanExecuteResolutionSetupCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region MotorMaxSpeedSetupCommand

        public ICommand MotorMaxSpeedSetupCommand
        {
            get
            {
                if (_MotorMaxSpeedSetupCommand == null)
                {
                    _MotorMaxSpeedSetupCommand = new RelayCommand(ExecuteMotorMaxSpeedSetupCommand, CanExecuteMotorMaxSpeedSetupCommand);
                }

                return _MotorMaxSpeedSetupCommand;
            }
        }
        public void ExecuteMotorMaxSpeedSetupCommand(object parameter)
        {
            //
            //TODO: implement the command
            //

            MotorType motorType = (MotorType)parameter;

            if (motorType == MotorType.X)
            {
                //TODO: Set the X motor max speed
                //System.Windows.MessageBox.Show(string.Format("TODO: Set X motor max speed : {0}", MotorXMaxSpeed));
            }
            else if (motorType == MotorType.Y)
            {
                //TODO: Set the Y motor max speed
                //System.Windows.MessageBox.Show(string.Format("TODO: Set Y motor max speed : {0}", MotorYMaxSpeed));
            }
            else if (motorType == MotorType.Z)
            {
                //TODO: Set the Z motor max speed
                //System.Windows.MessageBox.Show(string.Format("TODO: Set Z motor max speed : {0}", MotorZMaxSpeed));
            }
        }

        public bool CanExecuteMotorMaxSpeedSetupCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region MotorMaxAccelSetupCommand

        public ICommand MotorMaxAccelSetupCommand
        {
            get
            {
                if (_MotorMaxAccelSetupCommand == null)
                {
                    _MotorMaxAccelSetupCommand = new RelayCommand(ExecuteMotorMaxAccelSetupCommand, CanExecuteMotorMaxAccelSetupCommand);
                }

                return _MotorMaxAccelSetupCommand;
            }
        }
        public void ExecuteMotorMaxAccelSetupCommand(object parameter)
        {
            //
            //TODO: implement the command
            //

            MotorType motorType = (MotorType)parameter;

            if (motorType == MotorType.X)
            {
                //TODO: Set the X motor max accel
                //System.Windows.MessageBox.Show(string.Format("TODO: Set X motor max accel : {0}", MotorXMaxAccel));
            }
            else if (motorType == MotorType.Y)
            {
                //TODO: Set the Y motor max speed
                //System.Windows.MessageBox.Show(string.Format("TODO: Set Y motor max accel : {0}", MotorYMaxAccel));
            }
            else if (motorType == MotorType.Z)
            {
                //TODO: Set the Z motor max speed
                System.Windows.MessageBox.Show(string.Format("TODO: Set Z motor max accel : {0}", MotorZMaxAccel));
            }
        }

        public bool CanExecuteMotorMaxAccelSetupCommand(object parameter)
        {
            return true;
        }

        #endregion

        #endregion
    }
}
