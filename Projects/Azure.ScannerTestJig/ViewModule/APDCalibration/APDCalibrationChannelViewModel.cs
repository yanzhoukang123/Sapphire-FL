using Azure.APDCalibrationBench;
using Azure.WPF.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Azure.ScannerTestJig.ViewModule.APDCalibration
{
    public class APDCalibrationChannelViewModel : ViewModelBase
    {
        #region Private data
        private string _ChannelName = string.Empty;
        private static string _IVPCBNumber = string.Empty;
        private static string _LaserPCBNumber = string.Empty;
        private string _APDSerialNumber = string.Empty;
        private double? _CalibrationVoltage = null;
        private double? _CalibrationTemperature = 25;
        private double? _BreakdownVoltage = null;
        private bool _IsAPDModuleAlive = false;
        private ObservableCollection<string> _WaveLengthOptions = new ObservableCollection<string>();
        private int _SelectedWaveLength = 0;
        private ObservableCollection<CalibrationMember> _CalibrationGainOptions = new ObservableCollection<CalibrationMember>();
        private CalibrationMember _SelectedCalibrationGain = null;
        private ObservableCollection<double> _TemperatureCoeffOptions = new ObservableCollection<double>();
        private double _SelectedTemperatureCoeff;
        private int? _CurrentAPDGain = null;
        private int? _CurrentPGA = null;
        private double? _APDHighVoltage = null;
        private double? _APDTemperature = null;
        private double? _TECTemperature = null;
        private double? _APDOutput = null;
        private int _LaserPower = 0;
        private int _CalibrationStepCount = 0;
        private SolidColorBrush _LightBlue;
        private SolidColorBrush _LightReseda;
        private ObservableCollection<CalibrationMember> _GainOptions = new ObservableCollection<CalibrationMember>();

        private APDCalibrationViewModel _APDCalibrationViewModel = null;

        private string _FittedLine = null;
        private string _FittedLine2 = null;
        #endregion
        public APDCalibrationChannelViewModel(
            string channelName,
            CalibrationRecords calibrationRecords,
            APDCalibrationViewModel calibrationViewModel)
        {
           // ChannelName = channelName;
            _APDCalibrationViewModel = calibrationViewModel;

            //_WaveLengthOptions.Add("800");
            //_WaveLengthOptions.Add("650");

            for (int i = 0; i < calibrationRecords.CalibrationRecordsCHA.Count; i++)
            {
                _GainOptions.Add(new CalibrationMember()
                {
                    APDGain = calibrationRecords.CalibrationRecordsCHA[i].APDGain,
                    APDOutput = null,
                    CalibrationVolt = null,
                    CalibrationTemper = null,
                    VerifyAPDOutput = null
                });
            }

            _CalibrationGainOptions.Add(new CalibrationMember() { APDGain = 100, APDOutput = 0 });
            _CalibrationGainOptions.Add(new CalibrationMember() { APDGain = 50, APDOutput = 0 });
            SelectedCalibrationGain = _CalibrationGainOptions[0];

            _TemperatureCoeffOptions.Add(1.85);
            _TemperatureCoeffOptions.Add(0.65);
            SelectedTemperatureCoeff = _TemperatureCoeffOptions[0];
        }
        #region public properties
        public string ChannelName
        {
            get
            {
                return _ChannelName;
            }
            set
            {
                if (_ChannelName != value)
                {
                    _ChannelName = value;
                    RaisePropertyChanged("ChannelName");
                }
            }
        }
        public static string IVPCBNumber
        {
            get
            {
                return _IVPCBNumber;
            }
            set
            {
                if (_IVPCBNumber != value)
                {
                   
                    _IVPCBNumber = value;
                    Emtiy();
                    //RaisePropertyChanged("PCBSerialNumber");
                }
            }
        }
        public static string LaserPCBNumber
        {
            get { return _LaserPCBNumber; }
            set { _LaserPCBNumber = value; }
        }
        public string APDSerialNumber
        {
            get
            {
                return _APDSerialNumber;
            }
            set
            {
                if (_APDSerialNumber != value)
                {
                    _APDSerialNumber = value;
                    RaisePropertyChanged("APDSerialNumber");
                    Emtiy();
                }
            }
        }
        public ObservableCollection<string> WaveLengthOptions
        {
            get
            {
                return _WaveLengthOptions;
            }
        }
        public int SelectedWaveLength
        {
            get
            {
                return _SelectedWaveLength;
            }
            set
            {
                if (_SelectedWaveLength != value)
                {
                    _SelectedWaveLength = value;
                    RaisePropertyChanged("SelectedWaveLength");
                }
            }
        }
        public ObservableCollection<CalibrationMember> CalibrationGainOptions
        {
            get
            {
                return _CalibrationGainOptions;
            }
        }
        public CalibrationMember SelectedCalibrationGain
        {
            get
            {
                return _SelectedCalibrationGain;
            }
            set
            {
                if (_SelectedCalibrationGain != value)
                {
                    _SelectedCalibrationGain = value;
                    RaisePropertyChanged("SelectedCalibrationGain");
                }
            }
        }
        public ObservableCollection<double> TemperatureCoeffOptions
        {
            get
            {
                return _TemperatureCoeffOptions;
            }
        }
        public double SelectedTemperatureCoeff
        {
            get
            {
                return _SelectedTemperatureCoeff;
            }
            set
            {
                if (_SelectedTemperatureCoeff != value)
                {
                    _SelectedTemperatureCoeff = value;
                    if (_SelectedTemperatureCoeff != 0)
                    {
                        if(Workspace.This!=null)
                           Workspace.This.APDCalibrationVM.SetTempscale();
                    }
                    RaisePropertyChanged("SelectedTemperatureCoeff");
                }
            }
        }
        public double? CalibrationVoltage
        {
            get
            {
                return _CalibrationVoltage;
            }
            set
            {
                if (_CalibrationVoltage != value)
                {
                    _CalibrationVoltage = value;
                    RaisePropertyChanged("CalibrationVoltage");
                    Emtiy();
                }
            }
        }
        public double? BreakdownVoltage
        {
            get {
                return _BreakdownVoltage; 
            }
            set
            {
                if (_BreakdownVoltage != value)
                {
                    _BreakdownVoltage = value;
                    RaisePropertyChanged("BreakdownVoltage");
                    Emtiy();
                }
            }
        }
        public double? CalibrationTemperature
        {
            get
            {
                return _CalibrationTemperature;
            }
            set
            {
                if (_CalibrationTemperature != value)
                {
                    _CalibrationTemperature = value;
                    RaisePropertyChanged("CalibrationTemperature");
                }
            }
        }
        public int? CurrentAPDGain
        {
            get
            {
                return _CurrentAPDGain;
            }
            set
            {
                if (_CurrentAPDGain != value)
                {
                    _CurrentAPDGain = value;
                    RaisePropertyChanged("CurrentAPDGain");
                }
            }
        }
        public int? CurrentPGA
        {
            get
            {
                return _CurrentPGA;
            }
            set
            {
                if (_CurrentPGA != value)
                {
                    _CurrentPGA = value;
                    RaisePropertyChanged("CurrentPGA");
                }
            }
        }
        public double? APDHighVoltage
        {
            get
            {
                return _APDHighVoltage;
            }
            set
            {
                if (_APDHighVoltage != value)
                {
                    _APDHighVoltage = value;
                    RaisePropertyChanged("APDHighVoltage");
                }
            }
        }
        public double? APDTemperature
        {
            get
            {
                return _APDTemperature;
            }
            set
            {
                if (_APDTemperature != value)
                {
                    _APDTemperature = value;
                    RaisePropertyChanged("APDTemperature");
                }
            }
        }
        public double? TECTemperature
        {
            get
            {
                return _TECTemperature;
            }
            set
            {
                if (_TECTemperature != value)
                {
                    _TECTemperature = value;
                }
            }
        }
        
        public double? APDOutput
        {
            get
            {
                return _APDOutput;
            }
            set
            {
                if (_APDOutput != value)
                {
                    _APDOutput = value;
                    RaisePropertyChanged("APDOutput");
                }
            }
        }
        public int LaserPower
        {
            get
            {
                return _LaserPower;
            }
            set
            {
                if (_LaserPower != value)
                {
                    _LaserPower = value;
                    RaisePropertyChanged("LaserPower");
                }
            }
        }
        public int CalibrationStepCount
        {
            get
            {
                return _CalibrationStepCount;
            }
            set
            {
                if (_CalibrationStepCount != value)
                {
                    _CalibrationStepCount = value;
                    RaisePropertyChanged("CalibrationStepCount");
                }
            }
        }
        public bool IsAPDModuleAlive
        {
            get
            {
                return _IsAPDModuleAlive;
            }
            set
            {
                if (_IsAPDModuleAlive != value)
                {
                    _IsAPDModuleAlive = value;
                    Workspace.This.APDCalibrationVM.IsAPDModuleAlive = value;
                    RaisePropertyChanged("IsAPDModuleAlive");
                }
            }
        }
        public ObservableCollection<CalibrationMember> GainOptions
        {
            get
            {
                return _GainOptions;
            }
            set
            {
                if (_GainOptions != value)
                {
                    _GainOptions = value;
                    RaisePropertyChanged("GainOptions");
                }
            }
        }
        public APDCalibrationViewModel APDCalibrationVM
        {
            get
            {
                return _APDCalibrationViewModel;
            }
        }

        public string FittedLine
        {
            get
            {
                return _FittedLine;
            }

            set
            {
                if (_FittedLine != value)
                {
                    _FittedLine = value;
                    RaisePropertyChanged("FittedLine");
                }
            }
        }
        public string FittedLine2
        {
            get
            {
                return _FittedLine2;
            }

            set
            {
                if (_FittedLine2 != value)
                {
                    _FittedLine2 = value;
                    RaisePropertyChanged("FittedLine2");
                }
            }
        }
        public SolidColorBrush LightBlue
        {
            get
            {
                return _LightBlue;
            }

            set
            {
                if (_LightBlue != value)
                {
                    _LightBlue = value;
                    RaisePropertyChanged("LightBlue");
                }
            }
        }
        public SolidColorBrush LightReseda
        {
            get
            {
                return _LightReseda;
            }

            set
            {
                if (_LightReseda != value)
                {
                    _LightReseda = value;
                    RaisePropertyChanged("LightReseda");
                }
            }
        }
        public static void Emtiy()
        {
            if (APDCalibrationChannelViewModel.IVPCBNumber == string.Empty ||
                     //APDCalibrationChannelViewModel.LaserPCBNumber == string.Empty ||
                     Workspace.This.APDCalibrationChannelAVM.APDSerialNumber == string.Empty ||
                     Workspace.This.APDCalibrationChannelAVM.SelectedWaveLength == -1 ||
                     Workspace.This.APDCalibrationChannelAVM.CalibrationVoltage == null ||
                     Workspace.This.APDCalibrationChannelAVM.BreakdownVoltage == null ||
                     Workspace.This.APDCalibrationChannelAVM.SelectedCalibrationGain == null ||
                     Workspace.This.APDCalibrationChannelAVM.CalibrationTemperature == null ||
                     Workspace.This.APDCalibrationChannelAVM.SelectedTemperatureCoeff == 0||Workspace.This.APDCalibrationVM.CalibrationButtonState==false|| Workspace.This.APDCalibrationVM.CalibrationButtonState == false)
            {
                Workspace.This.APDCalibrationVM.IsApdModulStateAlive(false);
            }
            else
            {
                Workspace.This.APDCalibrationVM.IsApdModulStateAlive(true);
            }

        }
        #endregion
    }

    public class CalibrationMember : ViewModelBase
    {
        private int _APDGain;
        private double? _APDOutput = null;
        private double? _CalibrationVolt = null;
        private double? _CalibrationTemper = null;
        private double? _VerifyAPDOutput = null;
        private double? _VerifyAPDOutput2 = null;
        public int APDGain
        {
            get
            {
                return _APDGain;
            }
            set
            {
                if (_APDGain != value)
                {
                    _APDGain = value;
                    RaisePropertyChanged("APDGain");
                }
            }
        }
        public double? APDOutput
        {
            get
            {
                return _APDOutput;
            }
            set
            {
                if (_APDOutput != value)
                {
                    _APDOutput = value;
                    RaisePropertyChanged("APDOutput");
                }
            }
        }
        public double? CalibrationVolt
        {
            get
            {
                return _CalibrationVolt;
            }
            set
            {
                if (_CalibrationVolt != value)
                {
                    _CalibrationVolt = value;
                    RaisePropertyChanged("CalibrationVolt");
                }
            }
        }
        public double? CalibrationTemper
        {
            get
            {
                return _CalibrationTemper;
            }
            set
            {
                if (_CalibrationTemper != value)
                {
                    _CalibrationTemper = value;
                    RaisePropertyChanged("CalibrationTemper");
                }
            }
        }
        public double? VerifyAPDOutput
        {
            get
            {
                return _VerifyAPDOutput;
            }

            set
            {
                if (_VerifyAPDOutput != value)
                {
                    _VerifyAPDOutput = value;
                    RaisePropertyChanged("VerifyAPDOutput");
                }
            }
        }
        public double? VerifyAPDOutput2
        {
            get
            {
                return _VerifyAPDOutput2;
            }

            set
            {
                if (_VerifyAPDOutput2 != value)
                {
                    _VerifyAPDOutput2 = value;
                    RaisePropertyChanged("VerifyAPDOutput2");
                }
            }
        }
    }

   
}
