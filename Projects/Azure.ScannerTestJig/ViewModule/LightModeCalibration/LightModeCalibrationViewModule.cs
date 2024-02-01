using Azure.APDCalibrationBench;
using Azure.Configuration.Settings;
using Azure.EthernetCommLib;
using Azure.ImagingSystem;
using Azure.ScannerTestJig.View.LightModeCalibration;
using Azure.WPF.Framework;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Azure.ScannerTestJig.ViewModule.LightModeCalibration
{
    class LightModeCalibrationViewModule : ViewModelBase
    {
        private EthernetController _EthernetController;
        public LightModeCalibrationViewModule(EthernetController ethernetController)
        {
            _EthernetController = ethernetController;
        }
        public EthernetController EthernetDevice
        {
            get
            {
                return _EthernetController;
            }
        }
        public LightModeCalibrationViewModule()
        {
          
        }
        #region  光学模块设置 
        private ObservableCollection<LaserPower> _PowerOptions = null;
        private ObservableCollection<APDGainType> _APDGainOptions = null;
        private ObservableCollection<APDPgaType> _PGAOptionsModule = null;
        private ObservableCollection<double> _TemperatureCoeffOptions = new ObservableCollection<double>();
        private ObservableCollection<string> _CommunicationControlOptions = new ObservableCollection<string>();

        private string _SelectedCommunicationControl;
        private double _SelectedTemperatureCoeff;
        private APDPgaType _SelectedPGAMultipleL1Module = null;
        private APDGainType _SelectedGainComModuleL1 = null;
        private LightModeSettingsPort _LightModeSettingsPort = null;
        private LaserPower _SelectedLaserPowerL1Module = null;
        private LaserPower _SelectedLaserCorrespondingCurrentModule = null;
        private LaserPower _SelectedPhotodiodeVoltageCorrespondingToLaserPowerModule = null;
        private RelayCommand _SetCurrentLaserWavaCommand = null;
        private RelayCommand _SetCurrentLaserNumberCommand = null;
        private RelayCommand _SetCurrentPMTNumbeCommand = null;
        private RelayCommand _SetCurrentTECctrlTempCommand = null;
        private RelayCommand _SetLaserNot532PowserCommand = null;
        private RelayCommand _SetLaserTrue532PowserCommand = null;
        private RelayCommand _SetCurrentLaserPowerValueCommand = null;




        private RelayCommand _GetCurrentLaserWavaCommand = null;
        private RelayCommand _GetCurrentLaserNumberCommand = null;
        private RelayCommand _GetCurrentPMTNumberCommand = null;
        private RelayCommand _GetCurrentTECctrlTempCommand = null;
        private RelayCommand _GetLaserNot532PowserCommand = null;
        private RelayCommand _GetLaserTrue532PowserCommand = null;
        private RelayCommand _GetCurrentLaserPowerValueCommand = null;
        private RelayCommand _GetCurrentLightElectricTowVoleCommand = null;


        private RelayCommand _LightModeCalibrationCommand = null;


        private RelayCommand _GenerateReportCommand = null;
        public LightModeClibrationSub _LightModeClibrationSub = null;
        private double _SetCurrentLaserWavaValue = 0;
        private string _SetCurrentLaserNumberValue = "";
        private string _SetCurrentPMTNumberValue = "";
        private double _SetCurrentTecTempValue = 0;
        private double _SetLaserNot532PowserValue = 0;
        private double _SetLaserTrue532PowserValue = 0;
        private double _SetCurrentLaserPowerValueValue = 0;
        private string _GetCurrentLaserWavaValue;
        private string _GetCurrentLaserNumberValue;
        private string _GetCurrentPMTNumberValue;
        private double _GetCurrentTecTempValue = 0;
        private double _GetLaserNot532PowserValue = 0;
        private double _GetLaserTrue532PowserValue = 0;
        private double _GetCurrentLaserPowerValueValue = 0;
        private double _GetCurrentLightElectricTowVoleValue = 0;

        #endregion

        #region 光学模块隐藏IV部分
        private RelayCommand _SetCurrentSensorNoCommand = null;
        private RelayCommand _SetCurrentPGAMultipleCommand = null;
        private RelayCommand _SetCurrentAPDGainCommand = null;
        private RelayCommand _SetCurrentLightIntensityCalibrationTemperatureCommand = null;
        private RelayCommand _SetCurrenttGain50CalVolCommand = null;
        private RelayCommand _SetCurrenttGain100CalVolCommand = null;
        private RelayCommand _SetCurrenttGain150CalVolCommand = null;
        private RelayCommand _SetCurrenttGain200CalVolCommand = null;
        private RelayCommand _SetCurrenttGain250CalVolCommand = null;
        private RelayCommand _SetCurrenttGain300CalVolCommand = null;
        private RelayCommand _SetCurrenttGain400CalVolCommand = null;
        private RelayCommand _SetCurrenttGain500CalVolCommand = null;
        private RelayCommand _SetCurrenttPMTCtrlVolCommand = null;
        private RelayCommand _SetCurrenttPMTCompensationCoefficientCommand = null;
        private RelayCommand _SetCurrenttADPTemperatureCalibrationFactorCommand=null;
        private RelayCommand _SetIVOpticalModuleNumberCommand = null;

        private RelayCommand _GetCurrentSensorNoCommand = null;
        private RelayCommand _GetCurrentPGAMultipleCommand = null;
        private RelayCommand _GetCurrentAPDGainCommand = null;
        private RelayCommand _GetCurrentAPDTempCommand = null;
        private RelayCommand _GetCurrentAPDHighVoltageCommand = null;
        private RelayCommand _GetCurrentLightIntensityCalibrationTemperatureCommand = null;
        private RelayCommand _GetCurrentGain50CalVolCommand = null;
        private RelayCommand _GetCurrentGain100CalVolCommand = null;
        private RelayCommand _GetCurrentGain150CalVolCommand = null;
        private RelayCommand _GetCurrentGain200CalVolCommand = null;
        private RelayCommand _GetCurrentGain250CalVolCommand = null;
        private RelayCommand _GetCurrentGain300CalVolCommand = null;
        private RelayCommand _GetCurrentGain400CalVolCommand = null;
        private RelayCommand _GetCurrentGain500CalVolCommand = null;
        private RelayCommand _GetCurrentPMTCtrlVolCommand = null;
        private RelayCommand _GetCurrentPMTCompensationCoefficientCommand = null;
        private RelayCommand _GetCurrentTempSenser1282ADValueCommand = null;
        private RelayCommand _GetCurrentTempSenser6459ADValueCommand = null;
        private RelayCommand _GetCurrentTempSenserADValueCommand = null;
        private RelayCommand _GetCurrentIVBoardRunningStateCommand = null;
        private RelayCommand _GetCurrentTemperatureSensorSamplingVoltageCommand = null;
        private RelayCommand _GetCurrentADPTemperatureCalibrationFactorCommand = null;
        private RelayCommand _GetIVvNumberCommand = null;
        private RelayCommand _GetIVErrorCodeCommand = null;
        private RelayCommand _GetIVOpticalModuleNumberCommand = null;


        private string _SetCurrentSensorNoValue;
        private int _SetCurrentPGAMultipleValue;
        private int _SetCurrentAPDGainValue;
        private double _SetCurrentLightIntensityCalibrationTemperatureValue;
        private double _SetCurrentGain50CalVolValue;
        private double _SetCurrentGain100CalVolValue;
        private double _SetCurrentGain150CalVolValue;
        private double _SetCurrentGain200CalVolValue;
        private double _SetCurrentGain250CalVolValue;
        private double _SetCurrentGain300CalVolValue;
        private double _SetCurrentGain400CalVolValue;
        private double _SetCurrentGain500CalVolValue;
        private double _SetCurrentPMTCtrlVolValue;
        private string _SetCurrentPMTCompensationCoefficientValue;
        private double _SetCurrentADPTemperatureCalibrationFactorValue;
        private string _SetIVOpticalModuleNumberValue;

        private string _GetCurrentSensorNoValue;
        private int _GetCurrentPGAMultipleValue;
        private int _GetCurrentAPDGainValue;
        private double _GetCurrentAPDTempValue;
        private double _GetCurrentAPDHighVoltageValue;
        private double _GetCurrentLightIntensityCalibrationTemperatureValue;
        private double _GetCurrentGain50CalVolValue;
        private double _GetCurrentGain100CalVolValue;
        private double _GetCurrentGain150CalVolValue;
        private double _GetCurrentGain200CalVolValue;
        private double _GetCurrentGain250CalVolValue;
        private double _GetCurrentGain300CalVolValue;
        private double _GetCurrentGain400CalVolValue;
        private double _GetCurrentGain500CalVolValue;
        private double _GetCurrentPMTCtrlVolValue;
        private double _GetCurrentPMTCompensationCoefficientValue;
        private double _GetCurrentTempSenser1282ADValue;
        private double _GetCurrentTempSenser6459ADValue;
        private double _GetCurrentIVBoardRunningStateValue;
        private double _GetCurrentTempSenserADValue;
        private double _GetCurrentTemperatureSensorSamplingVoltageValue;
        private double _GetCurrentADPTemperatureCalibrationFactorValue;
        private string _GetIVvNumberValue;
        private string _GetIVErrorCodeValue;
        private string _GetIVOpticalModuleNumberValue;

        #endregion

        #region 光学模块隐藏Laser部分
        private RelayCommand _SetCurrentLaserNoCommand = null;
        private RelayCommand _SetCurrentLaserWavaLengthCommand = null;
        private RelayCommand _SetCurrentLaserCurrentValueCommand = null;
        private RelayCommand _SetCurrentLaserCorrespondingCurrentValueCommand = null;
        private RelayCommand _SetCurrentTEControlTemperatureCommand = null;
        private RelayCommand _SetCurrentTECMaximumCoolingCurrentCommand = null;
        private RelayCommand _SetCurrentTECRefrigerationControlParameterKpCommand = null;
        private RelayCommand _SetCurrentTECRefrigerationControlParameterKiCommand = null;
        private RelayCommand _SetCurrentTECRefrigerationControlParameterKdCommand = null;
        private RelayCommand _SetCurrentLaserLightPowerValueCommand = null;
        private RelayCommand _SetCurrentTECCurrentCompensationCoefficientCommand = null;
        private RelayCommand _SetCurrentPowerClosedloopControlParameterKpCommand = null;
        private RelayCommand _SetCurrentPowerClosedloopControlParameterKiCommand = null;
        private RelayCommand _SetCurrentPowerClosedloopControlParameterKdCommand = null;
        private RelayCommand _SetCurrentPhotodiodeVoltageCorrespondingToLaserPowerCommand = null;
        private RelayCommand _SetCurrentKValueofPhotodiodeTemperatureCurveCommand = null;
        private RelayCommand _SetCurrentBValueofPhotodiodeTemperatureCurveCommand = null;
        private RelayCommand _SetLaserOpticalModuleNumberCommand = null;
        private RelayCommand _SetOpticalPowerKpCommand = null;
        private RelayCommand _SetOpticalPowerKiCommand = null;
        private RelayCommand _SetOpticalPowerKdCommand = null;
        private RelayCommand _SetOpticalPowerLessThanOrEqual15mWKpCommand = null;
        private RelayCommand _SetOpticalPowerLessThanOrEqual15mWKiCommand = null;
        private RelayCommand _SetOpticalPowerLessThanOrEqual15mWKdCommand = null;
        private RelayCommand _SetOpticalPowerGreaterThan15mWKpCommand = null;
        private RelayCommand _SetOpticalPowerGreaterThan15mWKiCommand = null;
        private RelayCommand _SetOpticalPowerGreaterThan15mWKdCommand = null;
        private RelayCommand _SetOpticalPowerControlKpUpperLimitLessThanOrEqual15Command = null;
        private RelayCommand _SetOpticalPowerControlKpDownLimitLessThanOrEqual15Command = null;
        private RelayCommand _SetOpticalPowerControlKiUpperLimitLessThanOrEqual15Command = null;
        private RelayCommand _SetOpticalPowerControlKiDownLimitLessThanOrEqual15Command = null;
        private RelayCommand _SetOpticalPowerControlKpUpperLimitLessThan15Command = null;
        private RelayCommand _SetOpticalPowerControlKpDownLimitLessThan15Command = null;
        private RelayCommand _SetOpticalPowerControlKiUpperLimitLessThan15Command = null;
        private RelayCommand _SetOpticalPowerControlKiDownLimitLessThan15Command = null;
        private RelayCommand _SetLaserMaxCurrentCommand = null;
        private RelayCommand _SetLaserMinCurrentCommand = null;


        private RelayCommand _GetCurrentLaserNoCommand = null;
        private RelayCommand _GetCurrentLaserWavaLengthCommand = null;
        private RelayCommand _GetCurrentLaserCurrentValueCommand = null;
        private RelayCommand _GetCurrentLaserCorrespondingCurrentValueCommand = null;
        private RelayCommand _GetCurrentTECActualTemperatureCommand = null;
        private RelayCommand _GetCurrentTEControlTemperatureCommand = null;
        private RelayCommand _GetCurrentTECMaximumCoolingCurrentCommand = null;
        private RelayCommand _GetCurrentTECRefrigerationControlParameterKpCommand = null;
        private RelayCommand _GetCurrentTECRefrigerationControlParameterKiCommand = null;
        private RelayCommand _GetCurrentTECRefrigerationControlParameterKdCommand = null;
        private RelayCommand _GetCurrentLaserLightPowerValueCommand = null;
        private RelayCommand _GetCurrentTECWorkingStatusCommand = null;
        private RelayCommand _GetCurrentTECCurrentDirectionCommand = null;
        private RelayCommand _GetCurrentRadiatorTemperatureCommand = null;
        private RelayCommand _GetCurrentTECCurrentCompensationCoefficientCommand = null;
        private RelayCommand _Get_CurrentLaserLightPowerValueCommand = null;
        private RelayCommand _GetCurrentPowerClosedloopControlParameterKpCommand = null;
        private RelayCommand _GetCurrentPowerClosedloopControlParameterKiCommand = null;
        private RelayCommand _GetCurrentPowerClosedloopControlParameterKdCommand = null;
        private RelayCommand _GetCurrenPhotodiodeVoltageCorrespondingToLaserPowerCommand = null;
        private RelayCommand _GetCurrenPhotodiodeVoltageCommand = null;
        private RelayCommand _GetCurrenKValueofPhotodiodeTemperatureCurveCommand = null;
        private RelayCommand _GetCurrenBValueofPhotodiodeTemperatureCurveCommand = null;
        private RelayCommand _GetLaservNumberCommand = null;
        private RelayCommand _GetLaserErrorCodeCommand = null;
        private RelayCommand _GetLaserOpticalModuleNumberCommand = null;
        private RelayCommand _GetOpticalPowerKpCommand = null;
        private RelayCommand _GetOpticalPowerKiCommand = null;
        private RelayCommand _GetOpticalPowerKdCommand = null;
        private RelayCommand _GetOpticalPowerLessThanOrEqual15mWKpCommand = null;
        private RelayCommand _GetOpticalPowerLessThanOrEqual15mWKiCommand = null;
        private RelayCommand _GetOpticalPowerLessThanOrEqual15mWKdCommand = null;
        private RelayCommand _GetOpticalPowerGreaterThan15mWKpCommand = null;
        private RelayCommand _GetOpticalPowerGreaterThan15mWKiCommand = null;
        private RelayCommand _GetOpticalPowerGreaterThan15mWKdCommand = null;
        private RelayCommand _GetOpticalPowerControlKpUpperLimitLessThanOrEqual15Command = null;
        private RelayCommand _GetOpticalPowerControlKpDownLimitLessThanOrEqual15Command = null;
        private RelayCommand _GetOpticalPowerControlKiUpperLimitLessThanOrEqual15Command = null;
        private RelayCommand _GetOpticalPowerControlKiDownLimitLessThanOrEqual15Command = null;
        private RelayCommand _GetOpticalPowerControlKpUpperLimitLessThan15Command = null;
        private RelayCommand _GetOpticalPowerControlKpDownLimitLessThan15Command = null;
        private RelayCommand _GetOpticalPowerControlKiUpperLimitLessThan15Command = null;
        private RelayCommand _GetOpticalPowerControlKiDownLimitLessThan15Command = null;
        private RelayCommand _GetLaserMaxCurrentCommand = null;
        private RelayCommand _GetLaserMinCurrentCommand = null;




        private string _SetCurrentLaserNoValue;
        private int _SetCurrentLaserWavaLengthValue;
        private double _SetCurrentLaserCurrentValueValue;
        private double _SetCurrentLaserCorrespondingCurrentValue;
        private double _SetCurrentTEControlTemperatureValue;
        private double _SetCurrentTECMaximumCoolingCurrentValue;
        private double _SetCurrentTECRefrigerationControlParameterKpValue;
        private double _SetCurrentTECRefrigerationControlParameterKiValue;
        private double _SetCurrentTECRefrigerationControlParameterKdValue;
        private double _SetCurrentLaserLightPowerValueValue;
        private double _SetCurrentTECCurrentCompensationCoefficientValue;
        private double _SetCurrentPowerClosedloopControlParameterKpValue;
        private double _SetCurrentPowerClosedloopControlParameterKiValue;
        private double _SetCurrentPowerClosedloopControlParameterKdValue;
        private double _SetCurrenPhotodiodeVoltageCorrespondingToLaserPowerValue;
        private double _SetCurrentKValueofPhotodiodeTemperatureCurveValue;
        private double _SetCurrentBValueofPhotodiodeTemperatureCurveValue;
        private string _SetLaserOpticalModuleNumberValue;
        private double _SetOpticalPowerKpValue;
        private double _SetOpticalPowerKiValue;
        private double _SetOpticalPowerKdValue;

        private double _SetOpticalPowerLessThanOrEqual15mWKp;
        private double _SetOpticalPowerLessThanOrEqual15mWKi;
        private double _SetOpticalPowerLessThanOrEqual15mWKd;
        private double _SetOpticalPowerGreaterThan15mWKp;
        private double _SetOpticalPowerGreaterThan15mWKi;
        private double _SetOpticalPowerGreaterThan15mWKd;
        private double _SetOpticalPowerControlKpUpperLimitLessThanOrEqual15;
        private double _SetOpticalPowerControlKpDownLimitLessThanOrEqual15;
        private double _SetOpticalPowerControlKiUpperLimitLessThanOrEqual15;
        private double _SetOpticalPowerControlKiDownLimitLessThanOrEqual15;
        private double _SetOpticalPowerControlKpUpperLimitLessThan15;
        private double _SetOpticalPowerControlKpDownLimitLessThan15;
        private double _SetOpticalPowerControlKiUpperLimitLessThan15;
        private double _SetOpticalPowerControlKiDownLimitLessThan15;
        private double _SetLaserMaxCurrent;
        private double _SetLaserMinCurrent;

        private string _GetCurrentLaserNoValue;
        private int _GetCurrentLaserWavaLengthValue;
        private double _GetCurrentLaserCurrentValueValue;
        private double _GetCurrentLaserCorrespondingCurrentValueValue;
        private double _GetCurrentTECActualTemperatureValue;
        private double _GetCurrentTEControlTemperatureValue;
        private double _GetCurrentTECMaximumCoolingCurrentValue;
        private double _GetCurrentTECRefrigerationControlParameterKpValue;
        private double _GetCurrentTECRefrigerationControlParameterKiValue;
        private double _GetCurrentTECRefrigerationControlParameterKdValue;
        private double _GetCurrentLaserLightPowerValueValue;
        private double _GetCurrentTECWorkingStatusValue;
        private double _GetCurrentTECCurrentDirectionValue;
        private double _GetCurrenRadiatorTemperatureValue;
        private double _GetCurrenTECCurrentCompensationCoefficientValue;
        private double _Get_CurrentLaserLightPowerValueValue;
        private double _GetCurrentPowerClosedloopControlParameterKpValue;
        private double _GetCurrentPowerClosedloopControlParameterKiValue;
        private double _GetCurrentPowerClosedloopControlParameterKdValue;
        private double _GetCurrentPhotodiodeVoltageCorrespondingToLaserPowerValue;
        private double _GetCurrentPhotodiodeVoltageValue;
        private double _GetCurrentKValueofPhotodiodeTemperatureCurveValue;
        private double _GetCurrentBValueofPhotodiodeTemperatureCurveValue;
        private string _GetLaservNumberValue;
        private string _GetLaserErrorCodeValue;
        private string _GetLaserOpticalModuleNumberValue;
        private double _GetOpticalPowerKpValue;
        private double _GetOpticalPowerKiValue;
        private double _GetOpticalPowerKdValue;

        private double _GetOpticalPowerLessThanOrEqual15mWKp;
        private double _GetOpticalPowerLessThanOrEqual15mWKi;
        private double _GetOpticalPowerLessThanOrEqual15mWKd;
        private double _GetOpticalPowerGreaterThan15mWKp;
        private double _GetOpticalPowerGreaterThan15mWKi;
        private double _GetOpticalPowerGreaterThan15mWKd;
        private double _GetOpticalPowerControlKpUpperLimitLessThanOrEqual15;
        private double _GetOpticalPowerControlKpDownLimitLessThanOrEqual15;
        private double _GetOpticalPowerControlKiUpperLimitLessThanOrEqual15;
        private double _GetOpticalPowerControlKiDownLimitLessThanOrEqual15;
        private double _GetOpticalPowerControlKpUpperLimitLessThan15;
        private double _GetOpticalPowerControlKpDownLimitLessThan15;
        private double _GetOpticalPowerControlKiUpperLimitLessThan15;
        private double _GetOpticalPowerControlKiDownLimitLessThan15;
        private double _GetLaserMaxCurrent;
        private double _GetLaserMinCurrent;




        #endregion
        public void InitIVControls()
        {
            if (_PowerOptions==null)
            {
                _PowerOptions = SettingsManager.ConfigSettings.LaserPowers;
                LaserPower lp35 = new LaserPower();
                lp35.DisplayName = "35";
                lp35.Position = 6;
                lp35.Value = 35;
                LaserPower lp40 = new LaserPower();
                lp40.DisplayName = "40";
                lp40.Position = 7;
                lp40.Value = 40;
                LaserPower lp45 = new LaserPower();
                lp45.DisplayName = "45";
                lp45.Position = 8;
                lp45.Value = 45;
                LaserPower lp50 = new LaserPower();
                lp50.DisplayName = "50";
                lp50.Position = 9;
                lp50.Value = 50;
                _PowerOptions.Add(lp35);
                _PowerOptions.Add(lp40);
                _PowerOptions.Add(lp45);
                _PowerOptions.Add(lp50);
            }
            _APDGainOptions = SettingsManager.ConfigSettings.APDGains;
            _PGAOptionsModule = SettingsManager.ConfigSettings.APDPgas;
            if (_PGAOptionsModule != null && _PGAOptionsModule.Count >= 3)
            {
                _SelectedPGAMultipleL1Module = _PGAOptionsModule[3];
            }
            if (_APDGainOptions != null && _APDGainOptions.Count >= 6)
            {
                SelectedGainComModuleL1 = _APDGainOptions[5];    // select the 6th item
            }
            SelectedLaserPowerL1Module = _PowerOptions[0];
            SelectedLaserCorrespondingCurrentModule = _PowerOptions[0];
            SelectedPhotodiodeVoltageCorrespondingToLaserPowerModule = _PowerOptions[0];

            if( _TemperatureCoeffOptions.Count<=0)
            {
                _TemperatureCoeffOptions.Add(1.85);
                _TemperatureCoeffOptions.Add(0.65);
                SelectedTemperatureCoeff = _TemperatureCoeffOptions[0];

            }
            if (_CommunicationControlOptions.Count <= 0)
            {
                _CommunicationControlOptions.Add("");
                _CommunicationControlOptions.Add("开启");
                _CommunicationControlOptions.Add("关闭");
                SelectedCommunicationControl = _CommunicationControlOptions[2];

            }
            ExecuteGetIVvNumberCommand(null);

        }
        public ObservableCollection<LaserPower> LaserPowerModule
        {
            get { return _PowerOptions; }
        }

        public LaserPower SelectedLaserPowerL1Module
        {
            get { return _SelectedLaserPowerL1Module; }
            set
            {
                if (_SelectedLaserPowerL1Module != value)
                {
                    _SelectedLaserPowerL1Module = value;
                    RaisePropertyChanged("SelectedLaserPowerL1Module");
                }
            }
        }

        public LaserPower SelectedLaserCorrespondingCurrentModule
        {
            get { return _SelectedLaserCorrespondingCurrentModule; }
            set
            {
                if (_SelectedLaserCorrespondingCurrentModule != value)
                {
                    _SelectedLaserCorrespondingCurrentModule = value;
                    RaisePropertyChanged("SelectedLaserCorrespondingCurrentModule");
                }
            }
        }

        public LaserPower SelectedPhotodiodeVoltageCorrespondingToLaserPowerModule
        {
            get { return _SelectedPhotodiodeVoltageCorrespondingToLaserPowerModule; }
            set
            {
                if (_SelectedPhotodiodeVoltageCorrespondingToLaserPowerModule != value)
                {
                    _SelectedPhotodiodeVoltageCorrespondingToLaserPowerModule = value;
                    RaisePropertyChanged("SelectedPhotodiodeVoltageCorrespondingToLaserPowerModule");
                }
            }
        }

        #region 光学模块 Laser隐藏部分

        #region SetCurrentLaserNoCommand

        public ICommand SetCurrentLaserNoCommand
        {
            get
            {
                if (_SetCurrentLaserNoCommand == null)
                {
                    _SetCurrentLaserNoCommand = new RelayCommand(ExecuteSetCurrentLaserNoCommand, CanExecuteSetCurrentLaserNoCommand);
                }
                return _SetCurrentLaserNoCommand;
            }
        }
        public void ExecuteSetCurrentLaserNoCommand(object parameter)
        {
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            _LightModeSettingsPort.SetCurrentLaserNoValue(SetCurrentLaserNoValue);
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    Thread.Sleep(200);
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentLaserNoValue();
                }
            }
           
        }
        public bool CanExecuteSetCurrentLaserNoCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region SetCurrentLaserWavaLengthCommand

        public ICommand SetCurrentLaserWavaLengthCommand
        {
            get
            {
                if (_SetCurrentLaserWavaLengthCommand == null)
                {
                    _SetCurrentLaserWavaLengthCommand = new RelayCommand(ExecuteSetCurrentLaserWavaLengthCommand, CanExecuteSetCurrentLaserWavaLengthCommand);
                }
                return _SetCurrentLaserWavaLengthCommand;
            }
        }
        public void ExecuteSetCurrentLaserWavaLengthCommand(object parameter)
        {
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            _LightModeSettingsPort.SetCurrentLaserWavaLengthValue(SetCurrentLaserWavaLengthValue);
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    Thread.Sleep(200);
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentLaserWavaLengthValue();
                }

            }
           
        }
        public bool CanExecuteSetCurrentLaserWavaLengthCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region SetCurrentLaserCurrentValueCommand

        public ICommand SetCurrentLaserCurrentValueCommand
        {
            get
            {
                if (_SetCurrentLaserCurrentValueCommand == null)
                {
                    _SetCurrentLaserCurrentValueCommand = new RelayCommand(ExecuteSetCurrentLaserCurrentValueCommand, CanExecuteSetCurrentLaserCurrentValueCommand);
                }
                return _SetCurrentLaserCurrentValueCommand;
            }
        }
        public void ExecuteSetCurrentLaserCurrentValueCommand(object parameter)
        {
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            _LightModeSettingsPort.SetCurrentLaserCurrentValueValue(SetCurrentLaserCurrentValueValue);
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    Thread.Sleep(200);
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentLaserCurrentValueValue();
                }
            }
          
        }
        public bool CanExecuteSetCurrentLaserCurrentValueCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region SetCurrentLaserCorrespondingCurrentValueCommand

        public ICommand SetCurrentLaserCorrespondingCurrentValueCommand
        {
            get
            {
                if (_SetCurrentLaserCorrespondingCurrentValueCommand == null)
                {
                    _SetCurrentLaserCorrespondingCurrentValueCommand = new RelayCommand(ExecuteSetCurrentLaserCorrespondingCurrentValueCommand, CanExecuteSetCurrentLaserCorrespondingCurrentValueCommand);
                }
                return _SetCurrentLaserCorrespondingCurrentValueCommand;
            }
        }
        public void ExecuteSetCurrentLaserCorrespondingCurrentValueCommand(object parameter)
        {
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            _LightModeSettingsPort.SetCurrentLaserCorrespondingCurrentValue(SelectedLaserCorrespondingCurrentModule.Value, SetCurrentLaserCorrespondingCurrentValue);
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    Thread.Sleep(200);
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentLaserCorrespondingCurrentValueValue(SelectedLaserCorrespondingCurrentModule.Value);
                }
            }
           
        }
        public bool CanExecuteSetCurrentLaserCorrespondingCurrentValueCommand(object parameter)
        {
            return true;
        }



        #endregion

        #region SetCurrentTEControlTemperatureCommand

        public ICommand SetCurrentTEControlTemperatureCommand
        {
            get
            {
                if (_SetCurrentTEControlTemperatureCommand == null)
                {
                    _SetCurrentTEControlTemperatureCommand = new RelayCommand(ExecuteSetCurrentTEControlTemperatureCommand, CanExecuteSetCurrentTEControlTemperatureCommand);
                }
                return _SetCurrentTEControlTemperatureCommand;
            }
        }
        public void ExecuteSetCurrentTEControlTemperatureCommand(object parameter)
        {
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            _LightModeSettingsPort.SetCurrentTEControlTemperatureValue(SetCurrentTEControlTemperatureValue);
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    Thread.Sleep(200);
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentTEControlTemperatureValue();
                }
            }
           
        }
        public bool CanExecuteSetCurrentTEControlTemperatureCommand(object parameter)
        {
            return true;
        }



        #endregion

        #region SetCurrentTECMaximumCoolingCurrentCommand

        public ICommand SetCurrentTECMaximumCoolingCurrentCommand
        {
            get
            {
                if (_SetCurrentTECMaximumCoolingCurrentCommand == null)
                {
                    _SetCurrentTECMaximumCoolingCurrentCommand = new RelayCommand(ExecuteSetCurrentTECMaximumCoolingCurrentCommand, CanExecuteSetCurrentTECMaximumCoolingCurrentCommand);
                }
                return _SetCurrentTECMaximumCoolingCurrentCommand;
            }
        }
        public void ExecuteSetCurrentTECMaximumCoolingCurrentCommand(object parameter)
        {
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            _LightModeSettingsPort.SetCurrentTECMaximumCoolingCurrentValue(SetCurrentTECMaximumCoolingCurrentValue);
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    Thread.Sleep(200);
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentTECMaximumCoolingCurrentValue();

                }

            }
           
        }
        public bool CanExecuteSetCurrentTECMaximumCoolingCurrentCommand(object parameter)
        {
            return true;
        }



        #endregion

        #region SetCurrentTECRefrigerationControlParameterKpCommand

        public ICommand SetCurrentTECRefrigerationControlParameterKpCommand
        {
            get
            {
                if (_SetCurrentTECRefrigerationControlParameterKpCommand == null)
                {
                    _SetCurrentTECRefrigerationControlParameterKpCommand = new RelayCommand(ExecuteSetCurrentTECRefrigerationControlParameterKpCommand, CanExecuteSetCurrentTECRefrigerationControlParameterKpCommand);
                }
                return _SetCurrentTECRefrigerationControlParameterKpCommand;
            }
        }
        public void ExecuteSetCurrentTECRefrigerationControlParameterKpCommand(object parameter)
        {
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            _LightModeSettingsPort.SetCurrentTECRefrigerationControlParameterKpValue(SetCurrentTECRefrigerationControlParameterKpValue);
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    Thread.Sleep(200);
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentTECRefrigerationControlParameterKpValue();
                }
            }
           
        }
        public bool CanExecuteSetCurrentTECRefrigerationControlParameterKpCommand(object parameter)
        {
            return true;
        }



        #endregion

        #region SetCurrentTECRefrigerationControlParameterKiCommand

        public ICommand SetCurrentTECRefrigerationControlParameterKiCommand
        {
            get
            {
                if (_SetCurrentTECRefrigerationControlParameterKiCommand == null)
                {
                    _SetCurrentTECRefrigerationControlParameterKiCommand = new RelayCommand(ExecuteSetCurrentTECRefrigerationControlParameterKiCommand, CanExecuteSetCurrentTECRefrigerationControlParameterKiCommand);
                }
                return _SetCurrentTECRefrigerationControlParameterKiCommand;
            }
        }
        public void ExecuteSetCurrentTECRefrigerationControlParameterKiCommand(object parameter)
        {
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            _LightModeSettingsPort.SetCurrentTECRefrigerationControlParameterKiValue(SetCurrentTECRefrigerationControlParameterKiValue);
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    Thread.Sleep(200);
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentTECRefrigerationControlParameterKiValue();
                }

            }
           
        }
        public bool CanExecuteSetCurrentTECRefrigerationControlParameterKiCommand(object parameter)
        {
            return true;
        }



        #endregion

        #region SetCurrentTECRefrigerationControlParameterKdCommand

        public ICommand SetCurrentTECRefrigerationControlParameterKdCommand
        {
            get
            {
                if (_SetCurrentTECRefrigerationControlParameterKdCommand == null)
                {
                    _SetCurrentTECRefrigerationControlParameterKdCommand = new RelayCommand(ExecuteSetCurrentTECRefrigerationControlParameterKdCommand, CanExecuteSetCurrentTECRefrigerationControlParameterKdCommand);
                }
                return _SetCurrentTECRefrigerationControlParameterKdCommand;
            }
        }
        public void ExecuteSetCurrentTECRefrigerationControlParameterKdCommand(object parameter)
        {
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            _LightModeSettingsPort.SetCurrentTECRefrigerationControlParameterKdValue(SetCurrentTECRefrigerationControlParameterKdValue);
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    Thread.Sleep(200);
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentTECRefrigerationControlParameterKdValue();
                }
            }
           
        }
        public bool CanExecuteSetCurrentTECRefrigerationControlParameterKdCommand(object parameter)
        {
            return true;
        }



        #endregion

        #region SetCurrentLaserLightPowerValueCommand

        public ICommand SetCurrentLaserLightPowerValueCommand
        {
            get
            {
                if (_SetCurrentLaserLightPowerValueCommand == null)
                {
                    _SetCurrentLaserLightPowerValueCommand = new RelayCommand(ExecuteSetCurrentLaserLightPowerValueCommand, CanExecuteSetCurrentLaserLightPowerValueCommand);
                }
                return _SetCurrentLaserLightPowerValueCommand;
            }
        }
        public void ExecuteSetCurrentLaserLightPowerValueCommand(object parameter)
        {
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            _LightModeSettingsPort.SetCurrentLaserLightPowerValueValue(SetCurrentLaserLightPowerValueValue);
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    Thread.Sleep(200);
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentLaserLightPowerValueValue();
                }
            }
           
        }
        public bool CanExecuteSetCurrentLaserLightPowerValueCommand(object parameter)
        {
            return true;
        }



        #endregion

        #region SetCurrentTECCurrentCompensationCoefficientCommand

        public ICommand SetCurrentTECCurrentCompensationCoefficientCommand
        {
            get
            {
                if (_SetCurrentTECCurrentCompensationCoefficientCommand == null)
                {
                    _SetCurrentTECCurrentCompensationCoefficientCommand = new RelayCommand(ExecuteSetCurrentTECCurrentCompensationCoefficientCommand, CanExecuteSetCurrentTECCurrentCompensationCoefficientCommand);
                }
                return _SetCurrentTECCurrentCompensationCoefficientCommand;
            }
        }
        public void ExecuteSetCurrentTECCurrentCompensationCoefficientCommand(object parameter)
        {
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            _LightModeSettingsPort.SetCurrentTECCurrentCompensationCoefficientValue(SetCurrentTECCurrentCompensationCoefficientValue);
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    Thread.Sleep(200);
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentTECCurrentCompensationCoefficientValue();
                }

            }
            
        }
        public bool CanExecuteSetCurrentTECCurrentCompensationCoefficientCommand(object parameter)
        {
            return true;
        }



        #endregion

        #region SetCurrentPowerClosedloopControlParameterKpCommand

        public ICommand SetCurrentPowerClosedloopControlParameterKpCommand
        {
            get
            {
                if (_SetCurrentPowerClosedloopControlParameterKpCommand == null)
                {
                    _SetCurrentPowerClosedloopControlParameterKpCommand = new RelayCommand(ExecuteSetCurrentPowerClosedloopControlParameterKpCommand, CanExecuteSetCurrentPowerClosedloopControlParameterKpCommand);
                }
                return _SetCurrentPowerClosedloopControlParameterKpCommand;
            }
        }
        public void ExecuteSetCurrentPowerClosedloopControlParameterKpCommand(object parameter)
        {
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            _LightModeSettingsPort.SetCurrentPowerClosedloopControlParameterKpValue(SetCurrentPowerClosedloopControlParameterKpValue);
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    Thread.Sleep(200);
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentPowerClosedloopControlParameterKpValue();
                }

            }
           
        }
        public bool CanExecuteSetCurrentPowerClosedloopControlParameterKpCommand(object parameter)
        {
            return true;
        }



        #endregion

        #region SetCurrentPowerClosedloopControlParameterKiCommand

        public ICommand SetCurrentPowerClosedloopControlParameterKiCommand
        {
            get
            {
                if (_SetCurrentPowerClosedloopControlParameterKiCommand == null)
                {
                    _SetCurrentPowerClosedloopControlParameterKiCommand = new RelayCommand(ExecuteSetCurrentPowerClosedloopControlParameterKiCommand, CanExecuteSetCurrentPowerClosedloopControlParameterKiCommand);
                }
                return _SetCurrentPowerClosedloopControlParameterKiCommand;
            }
        }
        public void ExecuteSetCurrentPowerClosedloopControlParameterKiCommand(object parameter)
        {
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            _LightModeSettingsPort.SetCurrentPowerClosedloopControlParameterKiValue(SetCurrentPowerClosedloopControlParameterKiValue);
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    Thread.Sleep(200);
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentPowerClosedloopControlParameterKiValue();
                }
            }
           
        }
        public bool CanExecuteSetCurrentPowerClosedloopControlParameterKiCommand(object parameter)
        {
            return true;
        }



        #endregion

        #region SetCurrentPowerClosedloopControlParameterKdCommand

        public ICommand SetCurrentPowerClosedloopControlParameterKdCommand
        {
            get
            {
                if (_SetCurrentPowerClosedloopControlParameterKdCommand == null)
                {
                    _SetCurrentPowerClosedloopControlParameterKdCommand = new RelayCommand(ExecuteSetCurrentPowerClosedloopControlParameterKdCommand, CanExecuteSetCurrentPowerClosedloopControlParameterKdCommand);
                }
                return _SetCurrentPowerClosedloopControlParameterKdCommand;
            }
        }
        public void ExecuteSetCurrentPowerClosedloopControlParameterKdCommand(object parameter)
        {
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            _LightModeSettingsPort.SetCurrentPowerClosedloopControlParameterKdValue(SetCurrentPowerClosedloopControlParameterKdValue);
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    Thread.Sleep(200);
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentPowerClosedloopControlParameterKdValue();

                }
            }
           
        }
        public bool CanExecuteSetCurrentPowerClosedloopControlParameterKdCommand(object parameter)
        {
            return true;
        }



        #endregion

        #region SetCurrentPhotodiodeVoltageCorrespondingToLaserPowerCommand

        public ICommand SetCurrentPhotodiodeVoltageCorrespondingToLaserPowerCommand
        {
            get
            {
                if (_SetCurrentPhotodiodeVoltageCorrespondingToLaserPowerCommand == null)
                {
                    _SetCurrentPhotodiodeVoltageCorrespondingToLaserPowerCommand = new RelayCommand(ExecuteSetCurrentPhotodiodeVoltageCorrespondingToLaserPowerCommand, CanExecuteSetCurrentPhotodiodeVoltageCorrespondingToLaserPowerCommand);
                }
                return _SetCurrentPhotodiodeVoltageCorrespondingToLaserPowerCommand;
            }
        }
        public void ExecuteSetCurrentPhotodiodeVoltageCorrespondingToLaserPowerCommand(object parameter)
        {
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            _LightModeSettingsPort.SetCurrentPhotodiodeVoltageCorrespondingToLaserPowerValue(SelectedPhotodiodeVoltageCorrespondingToLaserPowerModule.Value, SetCurrenPhotodiodeVoltageCorrespondingToLaserPowerValue);
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    Thread.Sleep(200);
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentPhotodiodeVoltageCorrespondingToLaserPowerValue(SelectedPhotodiodeVoltageCorrespondingToLaserPowerModule.Value);
                }
            }
           
        }
        public bool CanExecuteSetCurrentPhotodiodeVoltageCorrespondingToLaserPowerCommand(object parameter)
        {
            return true;
        }



        #endregion

        #region SetCurrentKValueofPhotodiodeTemperatureCurveCommand

        public ICommand SetCurrentKValueofPhotodiodeTemperatureCurveCommand
        {
            get
            {
                if (_SetCurrentKValueofPhotodiodeTemperatureCurveCommand == null)
                {
                    _SetCurrentKValueofPhotodiodeTemperatureCurveCommand = new RelayCommand(ExecuteSetCurrentKValueofPhotodiodeTemperatureCurveCommand, CanExecuteSetCurrentKValueofPhotodiodeTemperatureCurveCommand);
                }
                return _SetCurrentKValueofPhotodiodeTemperatureCurveCommand;
            }
        }
        public void ExecuteSetCurrentKValueofPhotodiodeTemperatureCurveCommand(object parameter)
        {
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            _LightModeSettingsPort.SetCurrentKValueofPhotodiodeTemperatureCurveValue(SetCurrentKValueofPhotodiodeTemperatureCurveValue);
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    Thread.Sleep(200);
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentKValueofPhotodiodeTemperatureCurveValue();
                }
            }
            
        }
        public bool CanExecuteSetCurrentKValueofPhotodiodeTemperatureCurveCommand(object parameter)
        {
            return true;
        }



        #endregion

        #region SetCurrentBValueofPhotodiodeTemperatureCurveCommand

        public ICommand SetCurrentBValueofPhotodiodeTemperatureCurveCommand
        {
            get
            {
                if (_SetCurrentBValueofPhotodiodeTemperatureCurveCommand == null)
                {
                    _SetCurrentBValueofPhotodiodeTemperatureCurveCommand = new RelayCommand(ExecuteSetCurrentBValueofPhotodiodeTemperatureCurveCommand, CanExecuteSetCurrentBValueofPhotodiodeTemperatureCurveCommand);
                }
                return _SetCurrentBValueofPhotodiodeTemperatureCurveCommand;
            }
        }
        public void ExecuteSetCurrentBValueofPhotodiodeTemperatureCurveCommand(object parameter)
        {
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            _LightModeSettingsPort.SetCurrentBValueofPhotodiodeTemperatureCurveValue(SetCurrentBValueofPhotodiodeTemperatureCurveValue);
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    Thread.Sleep(200);
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentBValueofPhotodiodeTemperatureCurveValue();

                }
            }
           
        }
        public bool CanExecuteSetCurrentBValueofPhotodiodeTemperatureCurveCommand(object parameter)
        {
            return true;
        }



        #endregion

        #region SetLaserOpticalModuleNumberCommand

        public ICommand SetLaserOpticalModuleNumberCommand
        {
            get
            {
                if (_SetLaserOpticalModuleNumberCommand == null)
                {
                    _SetLaserOpticalModuleNumberCommand = new RelayCommand(ExecuteSetLaserOpticalModuleNumberCommand, CanExecuteSetLaserOpticalModuleNumberCommand);
                }
                return _SetLaserOpticalModuleNumberCommand;
            }
        }
        public void ExecuteSetLaserOpticalModuleNumberCommand(object parameter)
        {
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            _LightModeSettingsPort.SetLaserOpticalModuleNumberValue(SetLaserOpticalModuleNumberValue);
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    Thread.Sleep(200);
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetLaserOpticalModuleNumberValue();
                    Thread.Sleep(200);
                    //GetCurrentADPTemperatureCalibrationFactorValue = _LightModeSettingsPort.CurrentADPTemperatureCalibrationFactorValue;
                }
            }

        }
        public bool CanExecuteSetLaserOpticalModuleNumberCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region SetOpticalPowerKpCommand

        public ICommand SetOpticalPowerKpCommand
        {
            get
            {
                if (_SetOpticalPowerKpCommand == null)
                {
                    _SetOpticalPowerKpCommand = new RelayCommand(ExecuteSetOpticalPowerKpCommand, CanExecuteSetOpticalPowerKpCommand);
                }
                return _SetOpticalPowerKpCommand;
            }
        }
        public void ExecuteSetOpticalPowerKpCommand(object parameter)
        {
            EthernetDevice.SetOpticalPowerLessThanOrEqual15mWKp(LaserChannels.ChannelC, SetOpticalPowerKpValue);
            //Thread.Sleep(500);
            //EthernetDevice.GetOpticalPowerLessThanOrEqual15mWKp();
            //GetOpticalPowerKpValue = EthernetDevice.AllOpticalPowerGreaterThan15mWKp[LaserChannels.ChannelC];
            //Thread.Sleep(500);
            //EthernetDevice.GetOpticalPowerLessThanOrEqual15mWKp();
            //GetOpticalPowerKpValue = EthernetDevice.AllOpticalPowerGreaterThan15mWKp[LaserChannels.ChannelC];
        }
        public bool CanExecuteSetOpticalPowerKpCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region SetOpticalPowerKiCommand

        public ICommand SetOpticalPowerKiCommand
        {
            get
            {
                if (_SetOpticalPowerKiCommand == null)
                {
                    _SetOpticalPowerKiCommand = new RelayCommand(ExecuteSetOpticalPowerKiCommand, CanExecuteSetOpticalPowerKiCommand);
                }
                return _SetOpticalPowerKiCommand;
            }
        }
        public void ExecuteSetOpticalPowerKiCommand(object parameter)
        {
            EthernetDevice.SetOpticalPowerLessThanOrEqual15mWKi(LaserChannels.ChannelC, SetOpticalPowerKiValue);
            //Thread.Sleep(500);
            //EthernetDevice.GetOpticalPowerLessThanOrEqual15mWKi();
            //GetOpticalPowerKiValue = EthernetDevice.AllOpticalPowerGreaterThan15mWKi[LaserChannels.ChannelC];
            //Thread.Sleep(500);
            //EthernetDevice.GetOpticalPowerLessThanOrEqual15mWKi();
            //GetOpticalPowerKiValue = EthernetDevice.AllOpticalPowerGreaterThan15mWKi[LaserChannels.ChannelC];

        }
        public bool CanExecuteSetOpticalPowerKiCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region SetOpticalPowerKdCommand

        public ICommand SetOpticalPowerKdCommand
        {
            get
            {
                if (_SetOpticalPowerKdCommand == null)
                {
                    _SetOpticalPowerKdCommand = new RelayCommand(ExecuteSetOpticalPowerKdCommand, CanExecuteSetOpticalPowerKdCommand);
                }
                return _SetOpticalPowerKdCommand;
            }
        }
        public void ExecuteSetOpticalPowerKdCommand(object parameter)
        {
            EthernetDevice.SetOpticalPowerLessThanOrEqual15mWKd(LaserChannels.ChannelC, SetOpticalPowerKdValue);
            //Thread.Sleep(500);
            //EthernetDevice.GetOpticalPowerLessThanOrEqual15mWKd();
            //GetOpticalPowerKdValue = EthernetDevice.AllOpticalPowerGreaterThan15mWKd[LaserChannels.ChannelC];
            //Thread.Sleep(500);
            //EthernetDevice.GetOpticalPowerLessThanOrEqual15mWKd();
            //GetOpticalPowerKdValue = EthernetDevice.AllOpticalPowerGreaterThan15mWKd[LaserChannels.ChannelC];
        }
        public bool CanExecuteSetOpticalPowerKdCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region SetOpticalPowerLessThanOrEqual15mWKpCommand

        public ICommand SetOpticalPowerLessThanOrEqual15mWKpCommand
        {
            get
            {
                if (_SetOpticalPowerLessThanOrEqual15mWKpCommand == null)
                {
                    _SetOpticalPowerLessThanOrEqual15mWKpCommand = new RelayCommand(ExecuteSetOpticalPowerLessThanOrEqual15mWKpCommand, CanExecuteSetOpticalPowerLessThanOrEqual15mWKpCommand);
                }
                return _SetOpticalPowerLessThanOrEqual15mWKpCommand;
            }
        }
        public void ExecuteSetOpticalPowerLessThanOrEqual15mWKpCommand(object parameter)
        {
            Workspace.This.EthernetController.SetOpticalPowerLessThanOrEqual15mWKp(LaserChannels.ChannelC, SetOpticalPowerLessThanOrEqual15mWKp);
        }
        public bool CanExecuteSetOpticalPowerLessThanOrEqual15mWKpCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region SetOpticalPowerLessThanOrEqual15mWKiCommand

        public ICommand SetOpticalPowerLessThanOrEqual15mWKiCommand
        {
            get
            {
                if (_SetOpticalPowerLessThanOrEqual15mWKiCommand == null)
                {
                    _SetOpticalPowerLessThanOrEqual15mWKiCommand = new RelayCommand(ExecuteSetOpticalPowerLessThanOrEqual15mWKiCommand, CanExecuteSetOpticalPowerLessThanOrEqual15mWKiCommand);
                }
                return _SetOpticalPowerLessThanOrEqual15mWKiCommand;
            }
        }
        public void ExecuteSetOpticalPowerLessThanOrEqual15mWKiCommand(object parameter)
        {
            Workspace.This.EthernetController.SetOpticalPowerLessThanOrEqual15mWKi(LaserChannels.ChannelC, SetOpticalPowerLessThanOrEqual15mWKi);
        }
        public bool CanExecuteSetOpticalPowerLessThanOrEqual15mWKiCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region SetOpticalPowerLessThanOrEqual15mWKdCommand

        public ICommand SetOpticalPowerLessThanOrEqual15mWKdCommand
        {
            get
            {
                if (_SetOpticalPowerLessThanOrEqual15mWKdCommand == null)
                {
                    _SetOpticalPowerLessThanOrEqual15mWKdCommand = new RelayCommand(ExecuteSetOpticalPowerLessThanOrEqual15mWKdCommand, CanExecuteSetOpticalPowerLessThanOrEqual15mWKdCommand);
                }
                return _SetOpticalPowerLessThanOrEqual15mWKdCommand;
            }
        }
        public void ExecuteSetOpticalPowerLessThanOrEqual15mWKdCommand(object parameter)
        {
            Workspace.This.EthernetController.SetOpticalPowerLessThanOrEqual15mWKd(LaserChannels.ChannelC, SetOpticalPowerLessThanOrEqual15mWKd);
        }
        public bool CanExecuteSetOpticalPowerLessThanOrEqual15mWKdCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region SetOpticalPowerGreaterThan15mWKpCommand

        public ICommand SetOpticalPowerGreaterThan15mWKpCommand
        {
            get
            {
                if (_SetOpticalPowerGreaterThan15mWKpCommand == null)
                {
                    _SetOpticalPowerGreaterThan15mWKpCommand = new RelayCommand(ExecuteSetOpticalPowerGreaterThan15mWKpCommand, CanExecuteSetOpticalPowerGreaterThan15mWKpCommand);
                }
                return _SetOpticalPowerGreaterThan15mWKpCommand;
            }
        }
        public void ExecuteSetOpticalPowerGreaterThan15mWKpCommand(object parameter)
        {
            Workspace.This.EthernetController.SetOpticalPowerGreaterThan15mWKp(LaserChannels.ChannelC, SetOpticalPowerGreaterThan15mWKp);
        }
        public bool CanExecuteSetOpticalPowerGreaterThan15mWKpCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region SetOpticalPowerGreaterThan15mWKiCommand

        public ICommand SetOpticalPowerGreaterThan15mWKiCommand
        {
            get
            {
                if (_SetOpticalPowerGreaterThan15mWKiCommand == null)
                {
                    _SetOpticalPowerGreaterThan15mWKiCommand = new RelayCommand(ExecuteSetOpticalPowerGreaterThan15mWKiCommand, CanExecuteSetOpticalPowerGreaterThan15mWKiCommand);
                }
                return _SetOpticalPowerGreaterThan15mWKiCommand;
            }
        }
        public void ExecuteSetOpticalPowerGreaterThan15mWKiCommand(object parameter)
        {
            Workspace.This.EthernetController.SetOpticalPowerGreaterThan15mWKi(LaserChannels.ChannelC, SetOpticalPowerGreaterThan15mWKi);
        }
        public bool CanExecuteSetOpticalPowerGreaterThan15mWKiCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region SetOpticalPowerGreaterThan15mWKdCommand

        public ICommand SetOpticalPowerGreaterThan15mWKdCommand
        {
            get
            {
                if (_SetOpticalPowerGreaterThan15mWKdCommand == null)
                {
                    _SetOpticalPowerGreaterThan15mWKdCommand = new RelayCommand(ExecuteSetOpticalPowerGreaterThan15mWKdCommand, CanExecuteSetOpticalPowerGreaterThan15mWKdCommand);
                }
                return _SetOpticalPowerGreaterThan15mWKdCommand;
            }
        }
        public void ExecuteSetOpticalPowerGreaterThan15mWKdCommand(object parameter)
        {
            Workspace.This.EthernetController.SetOpticalPowerGreaterThan15mWKd(LaserChannels.ChannelC, SetOpticalPowerGreaterThan15mWKd);
        }
        public bool CanExecuteSetOpticalPowerGreaterThan15mWKdCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region SetOpticalPowerControlKpUpperLimitLessThanOrEqual15Command

        public ICommand SetOpticalPowerControlKpUpperLimitLessThanOrEqual15Command
        {
            get
            {
                if (_SetOpticalPowerControlKpUpperLimitLessThanOrEqual15Command == null)
                {
                    _SetOpticalPowerControlKpUpperLimitLessThanOrEqual15Command = new RelayCommand(ExecuteSetOpticalPowerControlKpUpperLimitLessThanOrEqual15Command, CanExecuteSetOpticalPowerControlKpUpperLimitLessThanOrEqual15Command);
                }
                return _SetOpticalPowerControlKpUpperLimitLessThanOrEqual15Command;
            }
        }
        public void ExecuteSetOpticalPowerControlKpUpperLimitLessThanOrEqual15Command(object parameter)
        {
            Workspace.This.EthernetController.SetOpticalPowerControlKpUpperLimitLessThanOrEqual15(LaserChannels.ChannelC, SetOpticalPowerControlKpUpperLimitLessThanOrEqual15);
        }
        public bool CanExecuteSetOpticalPowerControlKpUpperLimitLessThanOrEqual15Command(object parameter)
        {
            return true;
        }

        #endregion

        #region SetOpticalPowerControlKpDownLimitLessThanOrEqual15Command

        public ICommand SetOpticalPowerControlKpDownLimitLessThanOrEqual15Command
        {
            get
            {
                if (_SetOpticalPowerControlKpDownLimitLessThanOrEqual15Command == null)
                {
                    _SetOpticalPowerControlKpDownLimitLessThanOrEqual15Command = new RelayCommand(ExecuteSetOpticalPowerControlKpDownLimitLessThanOrEqual15Command, CanExecuteSetOpticalPowerControlKpDownLimitLessThanOrEqual15Command);
                }
                return _SetOpticalPowerControlKpDownLimitLessThanOrEqual15Command;
            }
        }
        public void ExecuteSetOpticalPowerControlKpDownLimitLessThanOrEqual15Command(object parameter)
        {
            Workspace.This.EthernetController.SetOpticalPowerControlKpDownLimitLessThanOrEqual15(LaserChannels.ChannelC, SetOpticalPowerControlKpDownLimitLessThanOrEqual15);
        }
        public bool CanExecuteSetOpticalPowerControlKpDownLimitLessThanOrEqual15Command(object parameter)
        {
            return true;
        }

        #endregion

        #region SetOpticalPowerControlKiUpperLimitLessThanOrEqual15Command

        public ICommand SetOpticalPowerControlKiUpperLimitLessThanOrEqual15Command
        {
            get
            {
                if (_SetOpticalPowerControlKiUpperLimitLessThanOrEqual15Command == null)
                {
                    _SetOpticalPowerControlKiUpperLimitLessThanOrEqual15Command = new RelayCommand(ExecuteSetOpticalPowerControlKiUpperLimitLessThanOrEqual15Command, CanExecuteSetOpticalPowerControlKiUpperLimitLessThanOrEqual15Command);
                }
                return _SetOpticalPowerControlKiUpperLimitLessThanOrEqual15Command;
            }
        }
        public void ExecuteSetOpticalPowerControlKiUpperLimitLessThanOrEqual15Command(object parameter)
        {
            Workspace.This.EthernetController.SetOpticalPowerControlKiUpperLimitLessThanOrEqual15(LaserChannels.ChannelC, SetOpticalPowerControlKiUpperLimitLessThanOrEqual15);
        }
        public bool CanExecuteSetOpticalPowerControlKiUpperLimitLessThanOrEqual15Command(object parameter)
        {
            return true;
        }

        #endregion

        #region SetOpticalPowerControlKiDownLimitLessThanOrEqual15Command

        public ICommand SetOpticalPowerControlKiDownLimitLessThanOrEqual15Command
        {
            get
            {
                if (_SetOpticalPowerControlKiDownLimitLessThanOrEqual15Command == null)
                {
                    _SetOpticalPowerControlKiDownLimitLessThanOrEqual15Command = new RelayCommand(ExecuteSetOpticalPowerControlKiDownLimitLessThanOrEqual15Command, CanExecuteSetOpticalPowerControlKiDownLimitLessThanOrEqual15Command);
                }
                return _SetOpticalPowerControlKiDownLimitLessThanOrEqual15Command;
            }
        }
        public void ExecuteSetOpticalPowerControlKiDownLimitLessThanOrEqual15Command(object parameter)
        {
            Workspace.This.EthernetController.SetOpticalPowerControlKiDownLimitLessThanOrEqual15(LaserChannels.ChannelC, SetOpticalPowerControlKiDownLimitLessThanOrEqual15);
        }
        public bool CanExecuteSetOpticalPowerControlKiDownLimitLessThanOrEqual15Command(object parameter)
        {
            return true;
        }

        #endregion

        #region SetOpticalPowerControlKpUpperLimitLessThan15Command

        public ICommand SetOpticalPowerControlKpUpperLimitLessThan15Command
        {
            get
            {
                if (_SetOpticalPowerControlKpUpperLimitLessThan15Command == null)
                {
                    _SetOpticalPowerControlKpUpperLimitLessThan15Command = new RelayCommand(ExecuteSetOpticalPowerControlKpUpperLimitLessThan15Command, CanExecuteSetOpticalPowerControlKpUpperLimitLessThan15Command);
                }
                return _SetOpticalPowerControlKpUpperLimitLessThan15Command;
            }
        }
        public void ExecuteSetOpticalPowerControlKpUpperLimitLessThan15Command(object parameter)
        {
            Workspace.This.EthernetController.SetOpticalPowerControlKpUpperLimitLessThan15(LaserChannels.ChannelC, SetOpticalPowerControlKpUpperLimitLessThan15);
        }
        public bool CanExecuteSetOpticalPowerControlKpUpperLimitLessThan15Command(object parameter)
        {
            return true;
        }

        #endregion

        #region SetOpticalPowerControlKpDownLimitLessThan15Command

        public ICommand SetOpticalPowerControlKpDownLimitLessThan15Command
        {
            get
            {
                if (_SetOpticalPowerControlKpDownLimitLessThan15Command == null)
                {
                    _SetOpticalPowerControlKpDownLimitLessThan15Command = new RelayCommand(ExecuteSetOpticalPowerControlKpDownLimitLessThan15Command, CanExecuteSetOpticalPowerControlKpDownLimitLessThan15Command);
                }
                return _SetOpticalPowerControlKpDownLimitLessThan15Command;
            }
        }
        public void ExecuteSetOpticalPowerControlKpDownLimitLessThan15Command(object parameter)
        {
            Workspace.This.EthernetController.SetOpticalPowerControlKpDownLimitLessThan15(LaserChannels.ChannelC, SetOpticalPowerControlKpDownLimitLessThan15);
        }
        public bool CanExecuteSetOpticalPowerControlKpDownLimitLessThan15Command(object parameter)
        {
            return true;
        }

        #endregion

        #region SetOpticalPowerControlKiUpperLimitLessThan15Command

        public ICommand SetOpticalPowerControlKiUpperLimitLessThan15Command
        {
            get
            {
                if (_SetOpticalPowerControlKiUpperLimitLessThan15Command == null)
                {
                    _SetOpticalPowerControlKiUpperLimitLessThan15Command = new RelayCommand(ExecuteSetOpticalPowerControlKiUpperLimitLessThan15Command, CanExecuteSetOpticalPowerControlKiUpperLimitLessThan15Command);
                }
                return _SetOpticalPowerControlKiUpperLimitLessThan15Command;
            }
        }
        public void ExecuteSetOpticalPowerControlKiUpperLimitLessThan15Command(object parameter)
        {
            Workspace.This.EthernetController.SetOpticalPowerControlKiUpperLimitLessThan15(LaserChannels.ChannelC, SetOpticalPowerControlKiUpperLimitLessThan15);
        }
        public bool CanExecuteSetOpticalPowerControlKiUpperLimitLessThan15Command(object parameter)
        {
            return true;
        }

        #endregion

        #region SetOpticalPowerControlKiDownLimitLessThan15Command

        public ICommand SetOpticalPowerControlKiDownLimitLessThan15Command
        {
            get
            {
                if (_SetOpticalPowerControlKiDownLimitLessThan15Command == null)
                {
                    _SetOpticalPowerControlKiDownLimitLessThan15Command = new RelayCommand(ExecuteSetOpticalPowerControlKiDownLimitLessThan15Command, CanExecuteSetOpticalPowerControlKiDownLimitLessThan15Command);
                }
                return _SetOpticalPowerControlKiDownLimitLessThan15Command;
            }
        }
        public void ExecuteSetOpticalPowerControlKiDownLimitLessThan15Command(object parameter)
        {
            Workspace.This.EthernetController.SetOpticalPowerControlKiDownLimitLessThan15(LaserChannels.ChannelC, SetOpticalPowerControlKiDownLimitLessThan15);
        }
        public bool CanExecuteSetOpticalPowerControlKiDownLimitLessThan15Command(object parameter)
        {
            return true;
        }

        #endregion

        #region SetLaserMaxCurrentCommand

        public ICommand SetLaserMaxCurrentCommand
        {
            get
            {
                if (_SetLaserMaxCurrentCommand == null)
                {
                    _SetLaserMaxCurrentCommand = new RelayCommand(ExecuteSetLaserMaxCurrentCommand, CanExecuteSetLaserMaxCurrentCommand);
                }
                return _SetLaserMaxCurrentCommand;
            }
        }
        public void ExecuteSetLaserMaxCurrentCommand(object parameter)
        {
            Workspace.This.EthernetController.SetLaserMaximumCurrent(LaserChannels.ChannelC, SetLaserMaxCurrent);
        }
        public bool CanExecuteSetLaserMaxCurrentCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region SetLaserMinCurrentCommand

        public ICommand SetLaserMinCurrentCommand
        {
            get
            {
                if (_SetLaserMinCurrentCommand == null)
                {
                    _SetLaserMinCurrentCommand = new RelayCommand(ExecuteSetLaserMinCurrentCommand, CanExecuteSetLaserMinCurrentCommand);
                }
                return _SetLaserMinCurrentCommand;
            }
        }
        public void ExecuteSetLaserMinCurrentCommand(object parameter)
        {
            Workspace.This.EthernetController.SetLaserMinimumCurrent(LaserChannels.ChannelC, SetLaserMinCurrent);
        }
        public bool CanExecuteSetLaserMinCurrentCommand(object parameter)
        {
            return true;
        }

        #endregion



        #region GetCurrentLaserNoCommand

        public ICommand GetCurrentLaserNoCommand
        {
            get
            {
                if (_GetCurrentLaserNoCommand == null)
                {
                    _GetCurrentLaserNoCommand = new RelayCommand(ExecuteGetCurrentLaserNoCommand, CanExecuteGetCurrentLaserNoCommand);
                }
                return _GetCurrentLaserNoCommand;
            }
        }
        public void ExecuteGetCurrentLaserNoCommand(object parameter)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    Thread.Sleep(200);
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentLaserNoValue();
                }
            }
           
        }
        public bool CanExecuteGetCurrentLaserNoCommand(object parameter)
        {
            return true;
        }



        #endregion

        #region GetCurrentLaserWavaLengthCommand

        public ICommand GetCurrentLaserWavaLengthCommand
        {
            get
            {
                if (_GetCurrentLaserWavaLengthCommand == null)
                {
                    _GetCurrentLaserWavaLengthCommand = new RelayCommand(ExecuteGetCurrentLaserWavaLengthCommand, CanExecuteGetCurrentLaserWavaLengthCommand);
                }
                return _GetCurrentLaserWavaLengthCommand;
            }
        }
        public void ExecuteGetCurrentLaserWavaLengthCommand(object parameter)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    Thread.Sleep(200);
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentLaserWavaLengthValue();
                }
            }
           
        }
        public bool CanExecuteGetCurrentLaserWavaLengthCommand(object parameter)
        {
            return true;
        }



        #endregion

        #region GetCurrentLaserCurrentValueCommand

        public ICommand GetCurrentLaserCurrentValueCommand
        {
            get
            {
                if (_GetCurrentLaserCurrentValueCommand == null)
                {
                    _GetCurrentLaserCurrentValueCommand = new RelayCommand(ExecuteGetCurrentLaserCurrentValueCommand, CanExecuteGetCurrentLaserCurrentValueCommand);
                }
                return _GetCurrentLaserCurrentValueCommand;
            }
        }
        public void ExecuteGetCurrentLaserCurrentValueCommand(object parameter)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    Thread.Sleep(200);
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentLaserCurrentValueValue();
                }
            }
           
        }
        public bool CanExecuteGetCurrentLaserCurrentValueCommand(object parameter)
        {
            return true;
        }



        #endregion

        #region GetCurrentLaserCurrentValueCommand

        public ICommand GetCurrentLaserCorrespondingCurrentValueCommand
        {
            get
            {
                if (_GetCurrentLaserCorrespondingCurrentValueCommand == null)
                {
                    _GetCurrentLaserCorrespondingCurrentValueCommand = new RelayCommand(ExecuteGetCurrentLaserCorrespondingCurrentValueCommand, CanExecuteGetCurrentLaserCorrespondingCurrentValueCommand);
                }
                return _GetCurrentLaserCorrespondingCurrentValueCommand;
            }
        }
        public void ExecuteGetCurrentLaserCorrespondingCurrentValueCommand(object parameter)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    Thread.Sleep(200);
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentLaserCorrespondingCurrentValueValue(SelectedLaserCorrespondingCurrentModule.Value);
                }
            }
            
        }
        public bool CanExecuteGetCurrentLaserCorrespondingCurrentValueCommand(object parameter)
        {
            return true;
        }



        #endregion

        #region GetCurrentTECActualTemperatureCommand

        public ICommand GetCurrentTECActualTemperatureCommand
        {
            get
            {
                if (_GetCurrentTECActualTemperatureCommand == null)
                {
                    _GetCurrentTECActualTemperatureCommand = new RelayCommand(ExecuteGetCurrentTECActualTemperatureCommand, CanExecuteGetCurrentTECActualTemperatureCommand);
                }
                return _GetCurrentTECActualTemperatureCommand;
            }
        }
        public void ExecuteGetCurrentTECActualTemperatureCommand(object parameter)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    Thread.Sleep(200);
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentTECActualTemperatureValue();
                }

            }
           
        }
        public bool CanExecuteGetCurrentTECActualTemperatureCommand(object parameter)
        {
            return true;
        }



        #endregion

        #region GetCurrentTEControlTemperatureCommand

        public ICommand GetCurrentTEControlTemperatureCommand
        {
            get
            {
                if (_GetCurrentTEControlTemperatureCommand == null)
                {
                    _GetCurrentTEControlTemperatureCommand = new RelayCommand(ExecuteGetCurrentTEControlTemperatureCommand, CanExecuteGetCurrentTEControlTemperatureCommand);
                }
                return _GetCurrentTEControlTemperatureCommand;
            }
        }
        public void ExecuteGetCurrentTEControlTemperatureCommand(object parameter)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    Thread.Sleep(200);
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentTEControlTemperatureValue();
                }

            }
           
        }
        public bool CanExecuteGetCurrentTEControlTemperatureCommand(object parameter)
        {
            return true;
        }



        #endregion

        #region GetCurrentTECMaximumCoolingCurrentCommand

        public ICommand GetCurrentTECMaximumCoolingCurrentCommand
        {
            get
            {
                if (_GetCurrentTECMaximumCoolingCurrentCommand == null)
                {
                    _GetCurrentTECMaximumCoolingCurrentCommand = new RelayCommand(ExecuteGetCurrentTECMaximumCoolingCurrentCommand, CanExecuteGetCurrentTECMaximumCoolingCurrentCommand);
                }
                return _GetCurrentTECMaximumCoolingCurrentCommand;
            }
        }
        public void ExecuteGetCurrentTECMaximumCoolingCurrentCommand(object parameter)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    Thread.Sleep(200);
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentTECMaximumCoolingCurrentValue();
                }
            }
           
        }
        public bool CanExecuteGetCurrentTECMaximumCoolingCurrentCommand(object parameter)
        {
            return true;
        }



        #endregion

        #region GetCurrentTECRefrigerationControlParameterKpCommand

        public ICommand GetCurrentTECRefrigerationControlParameterKpCommand
        {
            get
            {
                if (_GetCurrentTECRefrigerationControlParameterKpCommand == null)
                {
                    _GetCurrentTECRefrigerationControlParameterKpCommand = new RelayCommand(ExecuteGetCurrentTECRefrigerationControlParameterKpCommand, CanExecuteGetCurrentTECRefrigerationControlParameterKpCommand);
                }
                return _GetCurrentTECRefrigerationControlParameterKpCommand;
            }
        }
        public void ExecuteGetCurrentTECRefrigerationControlParameterKpCommand(object parameter)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    Thread.Sleep(200);
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentTECRefrigerationControlParameterKpValue();
                }

            }
           
        }
        public bool CanExecuteGetCurrentTECRefrigerationControlParameterKpCommand(object parameter)
        {
            return true;
        }



        #endregion

        #region GetCurrentTECRefrigerationControlParameterKiCommand

        public ICommand GetCurrentTECRefrigerationControlParameterKiCommand
        {
            get
            {
                if (_GetCurrentTECRefrigerationControlParameterKiCommand == null)
                {
                    _GetCurrentTECRefrigerationControlParameterKiCommand = new RelayCommand(ExecuteGetCurrentTECRefrigerationControlParameterKiCommand, CanExecuteGetCurrentTECRefrigerationControlParameterKiCommand);
                }
                return _GetCurrentTECRefrigerationControlParameterKiCommand;
            }
        }
        public void ExecuteGetCurrentTECRefrigerationControlParameterKiCommand(object parameter)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    Thread.Sleep(200);
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentTECRefrigerationControlParameterKiValue();
                }

            }
           
        }
        public bool CanExecuteGetCurrentTECRefrigerationControlParameterKiCommand(object parameter)
        {
            return true;
        }



        #endregion

        #region GetCurrentTECRefrigerationControlParameterKdCommand

        public ICommand GetCurrentTECRefrigerationControlParameterKdCommand
        {
            get
            {
                if (_GetCurrentTECRefrigerationControlParameterKdCommand == null)
                {
                    _GetCurrentTECRefrigerationControlParameterKdCommand = new RelayCommand(ExecuteGetCurrentTECRefrigerationControlParameterKdCommand, CanExecuteGetCurrentTECRefrigerationControlParameterKdCommand);
                }
                return _GetCurrentTECRefrigerationControlParameterKdCommand;
            }
        }
        public void ExecuteGetCurrentTECRefrigerationControlParameterKdCommand(object parameter)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    Thread.Sleep(200);
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentTECRefrigerationControlParameterKdValue();
                }
            }
           
        }
        public bool CanExecuteGetCurrentTECRefrigerationControlParameterKdCommand(object parameter)
        {
            return true;
        }



        #endregion

        #region GetCurrentLaserLightPowerValueCommand

        public ICommand GetCurrentLaserLightPowerValueCommand
        {
            get
            {
                if (_GetCurrentLaserLightPowerValueCommand == null)
                {
                    _GetCurrentLaserLightPowerValueCommand = new RelayCommand(ExecuteGetCurrentLaserLightPowerValueCommand, CanExecuteGetCurrentLaserLightPowerValueCommand);
                }
                return _GetCurrentLaserLightPowerValueCommand;
            }
        }
        public void ExecuteGetCurrentLaserLightPowerValueCommand(object parameter)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    Thread.Sleep(200);
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentLaserLightPowerValueValue();
                }
            }
          
        }
        public bool CanExecuteGetCurrentLaserLightPowerValueCommand(object parameter)
        {
            return true;
        }



        #endregion

        #region GetCurrentTECWorkingStatusCommand

        public ICommand GetCurrentTECWorkingStatusCommand
        {
            get
            {
                if (_GetCurrentTECWorkingStatusCommand == null)
                {
                    _GetCurrentTECWorkingStatusCommand = new RelayCommand(ExecuteGetCurrentTECWorkingStatusCommand, CanExecuteGetCurrentTECWorkingStatusCommand);
                }
                return _GetCurrentTECWorkingStatusCommand;
            }
        }
        public void ExecuteGetCurrentTECWorkingStatusCommand(object parameter)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    Thread.Sleep(200);
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentTECWorkingStatusValue();
                }
            }
          
        }
        public bool CanExecuteGetCurrentTECWorkingStatusCommand(object parameter)
        {
            return true;
        }



        #endregion

        #region GetCurrentTECCurrentDirectionCommand

        public ICommand GetCurrentTECCurrentDirectionCommand
        {
            get
            {
                if (_GetCurrentTECCurrentDirectionCommand == null)
                {
                    _GetCurrentTECCurrentDirectionCommand = new RelayCommand(ExecuteGetCurrentTECCurrentDirectionCommand, CanExecuteGetCurrentTECCurrentDirectionCommand);
                }
                return _GetCurrentTECCurrentDirectionCommand;
            }
        }
        public void ExecuteGetCurrentTECCurrentDirectionCommand(object parameter)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    Thread.Sleep(200);
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentTECCurrentDirectionValue();
                }
            }
           
        }
        public bool CanExecuteGetCurrentTECCurrentDirectionCommand(object parameter)
        {
            return true;
        }



        #endregion

        #region GetCurrentRadiatorTemperatureCommand

        public ICommand GetCurrentRadiatorTemperatureCommand
        {
            get
            {
                if (_GetCurrentRadiatorTemperatureCommand == null)
                {
                    _GetCurrentRadiatorTemperatureCommand = new RelayCommand(ExecuteGetCurrentRadiatorTemperatureCommand, CanExecuteGetCurrentRadiatorTemperatureCommand);
                }
                return _GetCurrentRadiatorTemperatureCommand;
            }
        }
        public void ExecuteGetCurrentRadiatorTemperatureCommand(object parameter)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    Thread.Sleep(200);
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrenRadiatorTemperatureValue();
                }

            }
           
        }
        public bool CanExecuteGetCurrentRadiatorTemperatureCommand(object parameter)
        {
            return true;
        }



        #endregion

        #region GetCurrentTECCurrentCompensationCoefficientCommand

        public ICommand GetCurrentTECCurrentCompensationCoefficientCommand
        {
            get
            {
                if (_GetCurrentTECCurrentCompensationCoefficientCommand == null)
                {
                    _GetCurrentTECCurrentCompensationCoefficientCommand = new RelayCommand(ExecuteGetCurrentTECCurrentCompensationCoefficientCommand, CanExecuteGetCurrentTECCurrentCompensationCoefficientCommand);
                }
                return _GetCurrentTECCurrentCompensationCoefficientCommand;
            }
        }
        public void ExecuteGetCurrentTECCurrentCompensationCoefficientCommand(object parameter)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    Thread.Sleep(200);
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentTECCurrentCompensationCoefficientValue();
                }
            }
          
        }
        public bool CanExecuteGetCurrentTECCurrentCompensationCoefficientCommand(object parameter)
        {
            return true;
        }



        #endregion

        #region Get_CurrentLaserLightPowerValueCommand

        public ICommand Get_CurrentLaserLightPowerValueCommand
        {
            get
            {
                if (_Get_CurrentLaserLightPowerValueCommand == null)
                {
                    _Get_CurrentLaserLightPowerValueCommand = new RelayCommand(ExecuteGet_CurrentLaserLightPowerValueCommand, CanExecuteGet_CurrentLaserLightPowerValueCommand);
                }
                return _Get_CurrentLaserLightPowerValueCommand;
            }
        }
        public void ExecuteGet_CurrentLaserLightPowerValueCommand(object parameter)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    Thread.Sleep(200);
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.Get_CurrentLaserLightPowerValueValue();
                }
            }
          
        }
        public bool CanExecuteGet_CurrentLaserLightPowerValueCommand(object parameter)
        {
            return true;
        }



        #endregion

        #region GetCurrentPowerClosedloopControlParameterKpCommand

        public ICommand GetCurrentPowerClosedloopControlParameterKpCommand
        {
            get
            {
                if (_GetCurrentPowerClosedloopControlParameterKpCommand == null)
                {
                    _GetCurrentPowerClosedloopControlParameterKpCommand = new RelayCommand(ExecuteGetCurrentPowerClosedloopControlParameterKpCommand, CanExecuteGetCurrentPowerClosedloopControlParameterKpCommand);
                }
                return _GetCurrentPowerClosedloopControlParameterKpCommand;
            }
        }
        public void ExecuteGetCurrentPowerClosedloopControlParameterKpCommand(object parameter)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    Thread.Sleep(200);
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentPowerClosedloopControlParameterKpValue();
                }
            }
          
        }
        public bool CanExecuteGetCurrentPowerClosedloopControlParameterKpCommand(object parameter)
        {
            return true;
        }



        #endregion

        #region GetCurrentPowerClosedloopControlParameterKiCommand

        public ICommand GetCurrentPowerClosedloopControlParameterKiCommand
        {
            get
            {
                if (_GetCurrentPowerClosedloopControlParameterKiCommand == null)
                {
                    _GetCurrentPowerClosedloopControlParameterKiCommand = new RelayCommand(ExecuteGetCurrentPowerClosedloopControlParameterKiCommand, CanExecuteGetCurrentPowerClosedloopControlParameterKiCommand);
                }
                return _GetCurrentPowerClosedloopControlParameterKiCommand;
            }
        }
        public void ExecuteGetCurrentPowerClosedloopControlParameterKiCommand(object parameter)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    Thread.Sleep(200);
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentPowerClosedloopControlParameterKiValue();
                }
            }
           
        }
        public bool CanExecuteGetCurrentPowerClosedloopControlParameterKiCommand(object parameter)
        {
            return true;
        }



        #endregion

        #region GetCurrentPowerClosedloopControlParameterKdCommand

        public ICommand GetCurrentPowerClosedloopControlParameterKdCommand
        {
            get
            {
                if (_GetCurrentPowerClosedloopControlParameterKdCommand == null)
                {
                    _GetCurrentPowerClosedloopControlParameterKdCommand = new RelayCommand(ExecuteGetCurrentPowerClosedloopControlParameterKdCommand, CanExecuteGetCurrentPowerClosedloopControlParameterKdCommand);
                }
                return _GetCurrentPowerClosedloopControlParameterKdCommand;
            }
        }
        public void ExecuteGetCurrentPowerClosedloopControlParameterKdCommand(object parameter)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    Thread.Sleep(200);
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentPowerClosedloopControlParameterKdValue();
                }
            }
           
        }
        public bool CanExecuteGetCurrentPowerClosedloopControlParameterKdCommand(object parameter)
        {
            return true;
        }



        #endregion

        #region GetCurrenPhotodiodeVoltageCorrespondingToLaserPowerCommand

        public ICommand GetCurrenPhotodiodeVoltageCorrespondingToLaserPowerCommand
        {
            get
            {
                if (_GetCurrenPhotodiodeVoltageCorrespondingToLaserPowerCommand == null)
                {
                    _GetCurrenPhotodiodeVoltageCorrespondingToLaserPowerCommand = new RelayCommand(ExecuteGetCurrenPhotodiodeVoltageCorrespondingToLaserPowerCommand, CanExecuteGetCurrenPhotodiodeVoltageCorrespondingToLaserPowerCommand);
                }
                return _GetCurrenPhotodiodeVoltageCorrespondingToLaserPowerCommand;
            }
        }
        public void ExecuteGetCurrenPhotodiodeVoltageCorrespondingToLaserPowerCommand(object parameter)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    Thread.Sleep(200);
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentPhotodiodeVoltageCorrespondingToLaserPowerValue(SelectedPhotodiodeVoltageCorrespondingToLaserPowerModule.Value);
                }

            }
        
        }
        public bool CanExecuteGetCurrenPhotodiodeVoltageCorrespondingToLaserPowerCommand(object parameter)
        {
            return true;
        }



        #endregion

        #region GetCurrenPhotodiodeVoltageCommand

        public ICommand GetCurrenPhotodiodeVoltageCommand
        {
            get
            {
                if (_GetCurrenPhotodiodeVoltageCommand == null)
                {
                    _GetCurrenPhotodiodeVoltageCommand = new RelayCommand(ExecuteGetCurrenPhotodiodeVoltageCommand, CanExecuteGetCurrenPhotodiodeVoltageCommand);
                }
                return _GetCurrenPhotodiodeVoltageCommand;
            }
        }
        public void ExecuteGetCurrenPhotodiodeVoltageCommand(object parameter)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    Thread.Sleep(200);
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentPhotodiodeVoltageValue();
                }
            }
          
        }
        public bool CanExecuteGetCurrenPhotodiodeVoltageCommand(object parameter)
        {
            return true;
        }



        #endregion

        #region GetCurrenKValueofPhotodiodeTemperatureCurveCommand

        public ICommand GetCurrenKValueofPhotodiodeTemperatureCurveCommand
        {
            get
            {
                if (_GetCurrenKValueofPhotodiodeTemperatureCurveCommand == null)
                {
                    _GetCurrenKValueofPhotodiodeTemperatureCurveCommand = new RelayCommand(ExecuteGetCurrenKValueofPhotodiodeTemperatureCurveCommand, CanExecuteGetCurrenKValueofPhotodiodeTemperatureCurveCommand);
                }
                return _GetCurrenKValueofPhotodiodeTemperatureCurveCommand;
            }
        }
        public void ExecuteGetCurrenKValueofPhotodiodeTemperatureCurveCommand(object parameter)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    Thread.Sleep(200);
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentKValueofPhotodiodeTemperatureCurveValue();
                }
            }
           
        }
        public bool CanExecuteGetCurrenKValueofPhotodiodeTemperatureCurveCommand(object parameter)
        {
            return true;
        }



        #endregion

        #region GetCurrenBValueofPhotodiodeTemperatureCurveCommand

        public ICommand GetCurrenBValueofPhotodiodeTemperatureCurveCommand
        {
            get
            {
                if (_GetCurrenBValueofPhotodiodeTemperatureCurveCommand == null)
                {
                    _GetCurrenBValueofPhotodiodeTemperatureCurveCommand = new RelayCommand(ExecuteGetCurrenBValueofPhotodiodeTemperatureCurveCommand, CanExecuteGetCurrenBValueofPhotodiodeTemperatureCurveCommand);
                }
                return _GetCurrenBValueofPhotodiodeTemperatureCurveCommand;
            }
        }
        public void ExecuteGetCurrenBValueofPhotodiodeTemperatureCurveCommand(object parameter)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    Thread.Sleep(200);
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentBValueofPhotodiodeTemperatureCurveValue();
                }
            }
           
        }
        public bool CanExecuteGetCurrenBValueofPhotodiodeTemperatureCurveCommand(object parameter)
        {
            return true;
        }



        #endregion

        #region GetLaservNumberCommand

        public ICommand GetLaservNumberCommand
        {
            get
            {
                if (_GetLaservNumberCommand == null)
                {
                    _GetLaservNumberCommand = new RelayCommand(ExecuteGetLaservNumberCommand, CanExecuteGetLaservNumberCommand);
                }
                return _GetLaservNumberCommand;
            }
        }
        public void ExecuteGetLaservNumberCommand(object parameter)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    Thread.Sleep(200);
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetLaservNumberValue();
                    Thread.Sleep(200);
                    //GetCurrentADPTemperatureCalibrationFactorValue = _LightModeSettingsPort.CurrentADPTemperatureCalibrationFactorValue;
                }

            }

        }
        public bool CanExecuteGetLaservNumberCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region GetLaserErrorCodeCommand

        public ICommand GetLaserErrorCodeCommand
        {
            get
            {
                if (_GetLaserErrorCodeCommand == null)
                {
                    _GetLaserErrorCodeCommand = new RelayCommand(ExecuteGetLaserErrorCodeCommand, CanExecuteLaserErrorCodeCommand);
                }
                return _GetLaserErrorCodeCommand;
            }
        }
        public void ExecuteGetLaserErrorCodeCommand(object parameter)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetLaserErrorCodeValue();
                    Thread.Sleep(200);
                    //GetCurrentADPTemperatureCalibrationFactorValue = _LightModeSettingsPort.CurrentADPTemperatureCalibrationFactorValue;
                }

            }

        }
        public bool CanExecuteLaserErrorCodeCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region GetLaserOpticalModuleNumberCommand

        public ICommand GetLaserOpticalModuleNumberCommand
        {
            get
            {
                if (_GetLaserOpticalModuleNumberCommand == null)
                {
                    _GetLaserOpticalModuleNumberCommand = new RelayCommand(ExecuteGetLaserOpticalModuleNumberCommand, CanExecuteLaserOpticalModuleNumberCommand);
                }
                return _GetLaserOpticalModuleNumberCommand;
            }
        }
        public void ExecuteGetLaserOpticalModuleNumberCommand(object parameter)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetLaserOpticalModuleNumberValue();
                    Thread.Sleep(200);
                    //GetCurrentADPTemperatureCalibrationFactorValue = _LightModeSettingsPort.CurrentADPTemperatureCalibrationFactorValue;
                }

            }

        }
        public bool CanExecuteLaserOpticalModuleNumberCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region GetOpticalPowerKpCommand

        public ICommand GetOpticalPowerKpCommand
        {
            get
            {
                if (_GetOpticalPowerKpCommand == null)
                {
                    _GetOpticalPowerKpCommand = new RelayCommand(ExecuteGetOpticalPowerKpCommand, CanExecuteGetOpticalPowerKpCommand);
                }
                return _GetOpticalPowerKpCommand;
            }
        }
        public void ExecuteGetOpticalPowerKpCommand(object parameter)
        {
            EthernetDevice.GetOpticalPowerLessThanOrEqual15mWKp();
            GetOpticalPowerKpValue = EthernetController.AllOpticalPowerLessThanOrEqual15mWKp[LaserChannels.ChannelC];
            Thread.Sleep(500);
            EthernetDevice.GetOpticalPowerLessThanOrEqual15mWKp();
            GetOpticalPowerKpValue = EthernetController.AllOpticalPowerLessThanOrEqual15mWKp[LaserChannels.ChannelC];
        }
        public bool CanExecuteGetOpticalPowerKpCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region GetOpticalPowerKiCommand

        public ICommand GetOpticalPowerKiCommand
        {
            get
            {
                if (_GetOpticalPowerKiCommand == null)
                {
                    _GetOpticalPowerKiCommand = new RelayCommand(ExecuteGetOpticalPowerKiCommand, CanExecuteGetOpticalPowerKiCommand);
                }
                return _GetOpticalPowerKiCommand;
            }
        }
        public void ExecuteGetOpticalPowerKiCommand(object parameter)
        {
            EthernetDevice.GetOpticalPowerLessThanOrEqual15mWKi();
            GetOpticalPowerKiValue = EthernetController.AllOpticalPowerLessThanOrEqual15mWKi[LaserChannels.ChannelC];
            Thread.Sleep(500);
            EthernetDevice.GetOpticalPowerLessThanOrEqual15mWKi();
            GetOpticalPowerKiValue = EthernetController.AllOpticalPowerLessThanOrEqual15mWKi[LaserChannels.ChannelC];
        }
        public bool CanExecuteGetOpticalPowerKiCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region GetOpticalPowerKdCommand

        public ICommand GetOpticalPowerKdCommand
        {
            get
            {
                if (_GetOpticalPowerKdCommand == null)
                {
                    _GetOpticalPowerKdCommand = new RelayCommand(ExecuteGetOpticalPowerKdCommand, CanExecuteGetOpticalPowerKdCommand);
                }
                return _GetOpticalPowerKdCommand;
            }
        }
        public void ExecuteGetOpticalPowerKdCommand(object parameter)
        {
            EthernetDevice.GetOpticalPowerLessThanOrEqual15mWKd();
            GetOpticalPowerKdValue = EthernetController.AllOpticalPowerLessThanOrEqual15mWKd[LaserChannels.ChannelC];
            Thread.Sleep(500);
            EthernetDevice.GetOpticalPowerLessThanOrEqual15mWKd();
            GetOpticalPowerKdValue = EthernetController.AllOpticalPowerLessThanOrEqual15mWKd[LaserChannels.ChannelC];
        }
        public bool CanExecuteGetOpticalPowerKdCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region GetOpticalPowerLessThanOrEqual15mWKpCommand

        public ICommand GetOpticalPowerLessThanOrEqual15mWKpCommand
        {
            get
            {
                if (_GetOpticalPowerLessThanOrEqual15mWKpCommand == null)
                {
                    _GetOpticalPowerLessThanOrEqual15mWKpCommand = new RelayCommand(ExecuteGetOpticalPowerLessThanOrEqual15mWKpCommand, CanExecuteGetOpticalPowerLessThanOrEqual15mWKpCommand);
                }
                return _GetOpticalPowerLessThanOrEqual15mWKpCommand;
            }
        }
        public void ExecuteGetOpticalPowerLessThanOrEqual15mWKpCommand(object parameter)
        {
            Workspace.This.EthernetController.GetOpticalPowerLessThanOrEqual15mWKp();
            GetOpticalPowerLessThanOrEqual15mWKp = EthernetController.AllOpticalPowerLessThanOrEqual15mWKp[LaserChannels.ChannelC];
        }
        public bool CanExecuteGetOpticalPowerLessThanOrEqual15mWKpCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region GetOpticalPowerLessThanOrEqual15mWKiCommand

        public ICommand GetOpticalPowerLessThanOrEqual15mWKiCommand
        {
            get
            {
                if (_GetOpticalPowerLessThanOrEqual15mWKiCommand == null)
                {
                    _GetOpticalPowerLessThanOrEqual15mWKiCommand = new RelayCommand(ExecuteGetOpticalPowerLessThanOrEqual15mWKiCommand, CanExecuteGetOpticalPowerLessThanOrEqual15mWKiCommand);
                }
                return _GetOpticalPowerLessThanOrEqual15mWKiCommand;
            }
        }
        public void ExecuteGetOpticalPowerLessThanOrEqual15mWKiCommand(object parameter)
        {
            Workspace.This.EthernetController.GetOpticalPowerLessThanOrEqual15mWKi();
            GetOpticalPowerLessThanOrEqual15mWKi = EthernetController.AllOpticalPowerLessThanOrEqual15mWKi[LaserChannels.ChannelC];
        }
        public bool CanExecuteGetOpticalPowerLessThanOrEqual15mWKiCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region GetOpticalPowerLessThanOrEqual15mWKdCommand

        public ICommand GetOpticalPowerLessThanOrEqual15mWKdCommand
        {
            get
            {
                if (_GetOpticalPowerLessThanOrEqual15mWKdCommand == null)
                {
                    _GetOpticalPowerLessThanOrEqual15mWKdCommand = new RelayCommand(ExecuteGetOpticalPowerLessThanOrEqual15mWKdCommand, CanExecuteGetOpticalPowerLessThanOrEqual15mWKdCommand);
                }
                return _GetOpticalPowerLessThanOrEqual15mWKdCommand;
            }
        }
        public void ExecuteGetOpticalPowerLessThanOrEqual15mWKdCommand(object parameter)
        {
            Workspace.This.EthernetController.GetOpticalPowerLessThanOrEqual15mWKd();
            GetOpticalPowerLessThanOrEqual15mWKd = EthernetController.AllOpticalPowerLessThanOrEqual15mWKd[LaserChannels.ChannelC];

        }
        public bool CanExecuteGetOpticalPowerLessThanOrEqual15mWKdCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region GetOpticalPowerGreaterThan15mWKpCommand

        public ICommand GetOpticalPowerGreaterThan15mWKpCommand
        {
            get
            {
                if (_GetOpticalPowerGreaterThan15mWKpCommand == null)
                {
                    _GetOpticalPowerGreaterThan15mWKpCommand = new RelayCommand(ExecuteGetOpticalPowerGreaterThan15mWKpCommand, CanExecuteGetOpticalPowerGreaterThan15mWKpCommand);
                }
                return _GetOpticalPowerGreaterThan15mWKpCommand;
            }
        }
        public void ExecuteGetOpticalPowerGreaterThan15mWKpCommand(object parameter)
        {
            Workspace.This.EthernetController.GetOpticalPowerGreaterThan15mWKp(LaserChannels.ChannelC);
            GetOpticalPowerGreaterThan15mWKp = EthernetController.AllOpticalPowerGreaterThan15mWKp[LaserChannels.ChannelC];
        }
        public bool CanExecuteGetOpticalPowerGreaterThan15mWKpCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region GetOpticalPowerGreaterThan15mWKiCommand

        public ICommand GetOpticalPowerGreaterThan15mWKiCommand
        {
            get
            {
                if (_GetOpticalPowerGreaterThan15mWKiCommand == null)
                {
                    _GetOpticalPowerGreaterThan15mWKiCommand = new RelayCommand(ExecuteGetOpticalPowerGreaterThan15mWKiCommand, CanExecuteGetOpticalPowerGreaterThan15mWKiCommand);
                }
                return _GetOpticalPowerGreaterThan15mWKiCommand;
            }
        }
        public void ExecuteGetOpticalPowerGreaterThan15mWKiCommand(object parameter)
        {
            Workspace.This.EthernetController.GetOpticalPowerGreaterThan15mWKi(LaserChannels.ChannelC);
            GetOpticalPowerGreaterThan15mWKi = EthernetController.AllOpticalPowerGreaterThan15mWKi[LaserChannels.ChannelC];
        }
        public bool CanExecuteGetOpticalPowerGreaterThan15mWKiCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region GetOpticalPowerGreaterThan15mWKdCommand

        public ICommand GetOpticalPowerGreaterThan15mWKdCommand
        {
            get
            {
                if (_GetOpticalPowerGreaterThan15mWKdCommand == null)
                {
                    _GetOpticalPowerGreaterThan15mWKdCommand = new RelayCommand(ExecuteGetOpticalPowerGreaterThan15mWKdCommand, CanExecuteGetOpticalPowerGreaterThan15mWKdCommand);
                }
                return _GetOpticalPowerGreaterThan15mWKdCommand;
            }
        }
        public void ExecuteGetOpticalPowerGreaterThan15mWKdCommand(object parameter)
        {
            Workspace.This.EthernetController.GetAllOpticalPowerGreaterThan15mWKd(LaserChannels.ChannelC);
            GetOpticalPowerGreaterThan15mWKd = EthernetController.AllOpticalPowerGreaterThan15mWKd[LaserChannels.ChannelC];
        }
        public bool CanExecuteGetOpticalPowerGreaterThan15mWKdCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region GetOpticalPowerControlKpUpperLimitLessThanOrEqual15Command

        public ICommand GetOpticalPowerControlKpUpperLimitLessThanOrEqual15Command
        {
            get
            {
                if (_GetOpticalPowerControlKpUpperLimitLessThanOrEqual15Command == null)
                {
                    _GetOpticalPowerControlKpUpperLimitLessThanOrEqual15Command = new RelayCommand(ExecuteGetOpticalPowerControlKpUpperLimitLessThanOrEqual15Command, CanExecuteGetOpticalPowerControlKpUpperLimitLessThanOrEqual15Command);
                }
                return _GetOpticalPowerControlKpUpperLimitLessThanOrEqual15Command;
            }
        }
        public void ExecuteGetOpticalPowerControlKpUpperLimitLessThanOrEqual15Command(object parameter)
        {
            Workspace.This.EthernetController.GetAllOpticalPowerControlKpUpperLimitLessThanOrEqual15();
            GetOpticalPowerControlKpUpperLimitLessThanOrEqual15 = EthernetController.AllGetOpticalPowerControlKpUpperLimitLessThanOrEqual15[LaserChannels.ChannelC];
        }
        public bool CanExecuteGetOpticalPowerControlKpUpperLimitLessThanOrEqual15Command(object parameter)
        {
            return true;
        }

        #endregion

        #region GetOpticalPowerControlKpDownLimitLessThanOrEqual15Command

        public ICommand GetOpticalPowerControlKpDownLimitLessThanOrEqual15Command
        {
            get
            {
                if (_GetOpticalPowerControlKpDownLimitLessThanOrEqual15Command == null)
                {
                    _GetOpticalPowerControlKpDownLimitLessThanOrEqual15Command = new RelayCommand(ExecuteGetOpticalPowerControlKpDownLimitLessThanOrEqual15Command, CanExecuteGetOpticalPowerControlKpDownLimitLessThanOrEqual15Command);
                }
                return _GetOpticalPowerControlKpDownLimitLessThanOrEqual15Command;
            }
        }
        public void ExecuteGetOpticalPowerControlKpDownLimitLessThanOrEqual15Command(object parameter)
        {
            Workspace.This.EthernetController.GetAllOpticalPowerControlKpDownLimitLessThanOrEqual15();
            GetOpticalPowerControlKpDownLimitLessThanOrEqual15 = EthernetController.AllGetOpticalPowerControlKpDownLimitLessThanOrEqual15[LaserChannels.ChannelC];
        }
        public bool CanExecuteGetOpticalPowerControlKpDownLimitLessThanOrEqual15Command(object parameter)
        {
            return true;
        }

        #endregion

        #region GetOpticalPowerControlKiUpperLimitLessThanOrEqual15Command

        public ICommand GetOpticalPowerControlKiUpperLimitLessThanOrEqual15Command
        {
            get
            {
                if (_GetOpticalPowerControlKiUpperLimitLessThanOrEqual15Command == null)
                {
                    _GetOpticalPowerControlKiUpperLimitLessThanOrEqual15Command = new RelayCommand(ExecuteGetOpticalPowerControlKiUpperLimitLessThanOrEqual15Command, CanExecuteGetOpticalPowerControlKiUpperLimitLessThanOrEqual15Command);
                }
                return _GetOpticalPowerControlKiUpperLimitLessThanOrEqual15Command;
            }
        }
        public void ExecuteGetOpticalPowerControlKiUpperLimitLessThanOrEqual15Command(object parameter)
        {
            Workspace.This.EthernetController.GetAllOpticalPowerControlKiUpperLimitLessThanOrEqual15();
            GetOpticalPowerControlKiUpperLimitLessThanOrEqual15 = EthernetController.AllGetOpticalPowerControlKiUpperLimitLessThanOrEqual15[LaserChannels.ChannelC];
        }
        public bool CanExecuteGetOpticalPowerControlKiUpperLimitLessThanOrEqual15Command(object parameter)
        {
            return true;
        }

        #endregion

        #region GetOpticalPowerControlKiDownLimitLessThanOrEqual15Command

        public ICommand GetOpticalPowerControlKiDownLimitLessThanOrEqual15Command
        {
            get
            {
                if (_GetOpticalPowerControlKiDownLimitLessThanOrEqual15Command == null)
                {
                    _GetOpticalPowerControlKiDownLimitLessThanOrEqual15Command = new RelayCommand(ExecuteGetOpticalPowerControlKiDownLimitLessThanOrEqual15Command, CanExecuteGetOpticalPowerControlKiDownLimitLessThanOrEqual15Command);
                }
                return _GetOpticalPowerControlKiDownLimitLessThanOrEqual15Command;
            }
        }
        public void ExecuteGetOpticalPowerControlKiDownLimitLessThanOrEqual15Command(object parameter)
        {
            Workspace.This.EthernetController.GetAllOpticalPowerControlKiDownLimitLessThanOrEqual15();
            GetOpticalPowerControlKiDownLimitLessThanOrEqual15 = EthernetController.AllGetOpticalPowerControlKiDownLimitLessThanOrEqual15[LaserChannels.ChannelC];
        }
        public bool CanExecuteGetOpticalPowerControlKiDownLimitLessThanOrEqual15Command(object parameter)
        {
            return true;
        }

        #endregion

        #region GetOpticalPowerControlKpUpperLimitLessThan15Command

        public ICommand GetOpticalPowerControlKpUpperLimitLessThan15Command
        {
            get
            {
                if (_GetOpticalPowerControlKpUpperLimitLessThan15Command == null)
                {
                    _GetOpticalPowerControlKpUpperLimitLessThan15Command = new RelayCommand(ExecuteGetOpticalPowerControlKpUpperLimitLessThan15Command, CanExecuteGetOpticalPowerControlKpUpperLimitLessThan15Command);
                }
                return _GetOpticalPowerControlKpUpperLimitLessThan15Command;
            }
        }
        public void ExecuteGetOpticalPowerControlKpUpperLimitLessThan15Command(object parameter)
        {
            Workspace.This.EthernetController.GetAllOpticalPowerControlKpUpperLimitLessThan15();
            GetOpticalPowerControlKpUpperLimitLessThan15 = EthernetController.AllGetOpticalPowerControlKpUpperLimitLessThan15[LaserChannels.ChannelC];
        }
        public bool CanExecuteGetOpticalPowerControlKpUpperLimitLessThan15Command(object parameter)
        {
            return true;
        }

        #endregion

        #region GetOpticalPowerControlKpDownLimitLessThan15Command

        public ICommand GetOpticalPowerControlKpDownLimitLessThan15Command
        {
            get
            {
                if (_GetOpticalPowerControlKpDownLimitLessThan15Command == null)
                {
                    _GetOpticalPowerControlKpDownLimitLessThan15Command = new RelayCommand(ExecuteGetOpticalPowerControlKpDownLimitLessThan15Command, CanExecuteGetOpticalPowerControlKpDownLimitLessThan15Command);
                }
                return _GetOpticalPowerControlKpDownLimitLessThan15Command;
            }
        }
        public void ExecuteGetOpticalPowerControlKpDownLimitLessThan15Command(object parameter)
        {
            Workspace.This.EthernetController.GetAllOpticalPowerControlKpDownLimitLessThan15();
            GetOpticalPowerControlKpDownLimitLessThan15 = EthernetController.AllGetOpticalPowerControlKpDownLimitLessThan15[LaserChannels.ChannelC];
        }
        public bool CanExecuteGetOpticalPowerControlKpDownLimitLessThan15Command(object parameter)
        {
            return true;
        }

        #endregion

        #region GetOpticalPowerControlKiUpperLimitLessThan15Command

        public ICommand GetOpticalPowerControlKiUpperLimitLessThan15Command
        {
            get
            {
                if (_GetOpticalPowerControlKiUpperLimitLessThan15Command == null)
                {
                    _GetOpticalPowerControlKiUpperLimitLessThan15Command = new RelayCommand(ExecuteGetOpticalPowerControlKiUpperLimitLessThan15Command, CanExecuteGetOpticalPowerControlKiUpperLimitLessThan15Command);
                }
                return _GetOpticalPowerControlKiUpperLimitLessThan15Command;
            }
        }
        public void ExecuteGetOpticalPowerControlKiUpperLimitLessThan15Command(object parameter)
        {
            Workspace.This.EthernetController.GetAllOpticalPowerControlKiUpperLimitLessThan15();
            GetOpticalPowerControlKiUpperLimitLessThan15 = EthernetController.AllGetOpticalPowerControlKiUpperLimitLessThan15[LaserChannels.ChannelC];
        }
        public bool CanExecuteGetOpticalPowerControlKiUpperLimitLessThan15Command(object parameter)
        {
            return true;
        }

        #endregion

        #region GetOpticalPowerControlKiDownLimitLessThan15Command

        public ICommand GetOpticalPowerControlKiDownLimitLessThan15Command
        {
            get
            {
                if (_GetOpticalPowerControlKiDownLimitLessThan15Command == null)
                {
                    _GetOpticalPowerControlKiDownLimitLessThan15Command = new RelayCommand(ExecuteGetOpticalPowerControlKiDownLimitLessThan15Command, CanExecuteGetOpticalPowerControlKiDownLimitLessThan15Command);
                }
                return _GetOpticalPowerControlKiDownLimitLessThan15Command;
            }
        }
        public void ExecuteGetOpticalPowerControlKiDownLimitLessThan15Command(object parameter)
        {
            Workspace.This.EthernetController.GetAllOpticalPowerControlKiDownLimitLessThan15();
            GetOpticalPowerControlKiDownLimitLessThan15 =EthernetController.AllGetOpticalPowerControlKiDownLimitLessThan15[LaserChannels.ChannelC];
        }
        public bool CanExecuteGetOpticalPowerControlKiDownLimitLessThan15Command(object parameter)
        {
            return true;
        }

        #endregion

        #region GetLaserMaxCurrentCommand

        public ICommand GetLaserMaxCurrentCommand
        {
            get
            {
                if (_GetLaserMaxCurrentCommand == null)
                {
                    _GetLaserMaxCurrentCommand = new RelayCommand(ExecuteGetLaserMaxCurrentCommand, CanExecuteGetLaserMaxCurrentCommand);
                }
                return _GetLaserMaxCurrentCommand;
            }
        }
        public void ExecuteGetLaserMaxCurrentCommand(object parameter)
        {
            Workspace.This.EthernetController.GetAllLaserMaximumCurrent();
            GetLaserMaxCurrent = EthernetController.AllGetLaserMaximumCurrent[LaserChannels.ChannelC];
        }
        public bool CanExecuteGetLaserMaxCurrentCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region GetLaserMinCurrentCommand

        public ICommand GetLaserMinCurrentCommand
        {
            get
            {
                if (_GetLaserMinCurrentCommand == null)
                {
                    _GetLaserMinCurrentCommand = new RelayCommand(ExecuteGetLaserMinCurrentCommand, CanExecuteGetLaserMinCurrentCommand);
                }
                return _GetLaserMinCurrentCommand;
            }
        }
        public void ExecuteGetLaserMinCurrentCommand(object parameter)
        {
            Workspace.This.EthernetController.GetAllLaserMinimumCurrent();
            GetLaserMinCurrent = EthernetController.AllGetLaserMinimumCurrent[LaserChannels.ChannelC];
        }
        public bool CanExecuteGetLaserMinCurrentCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region attribute
        public string SetLaserOpticalModuleNumberValue
        {
            get { return _SetLaserOpticalModuleNumberValue; }
            set
            {

                if (_SetLaserOpticalModuleNumberValue != value)
                {
                    _SetLaserOpticalModuleNumberValue = value;
                    RaisePropertyChanged("SetLaserOpticalModuleNumberValue");
                }
            }
        }
        public string SetCurrentLaserNoValue
        {
            get { return _SetCurrentLaserNoValue; }
            set
            {

                if (_SetCurrentLaserNoValue != value)
                {
                    _SetCurrentLaserNoValue = value;
                    RaisePropertyChanged("SetCurrentLaserNoValue");
                }
            }
        }

        public int SetCurrentLaserWavaLengthValue
        {
            get { return _SetCurrentLaserWavaLengthValue; }
            set
            {

                if (_SetCurrentLaserWavaLengthValue != value)
                {
                    _SetCurrentLaserWavaLengthValue = value;
                    RaisePropertyChanged("SetCurrentLaserWavaLengthValue");
                }
            }
        }

        public double SetCurrentLaserCurrentValueValue
        {
            get { return _SetCurrentLaserCurrentValueValue; }
            set
            {

                if (_SetCurrentLaserCurrentValueValue != value)
                {
                    _SetCurrentLaserCurrentValueValue = value;
                    RaisePropertyChanged("SetCurrentLaserCurrentValueValue");
                }
            }
        }

        public double SetCurrentLaserCorrespondingCurrentValue
        {
            get { return _SetCurrentLaserCorrespondingCurrentValue; }
            set
            {

                if (_SetCurrentLaserCorrespondingCurrentValue != value)
                {
                    _SetCurrentLaserCorrespondingCurrentValue = value;
                    RaisePropertyChanged("SetCurrentLaserCorrespondingCurrentValue");
                }
            }
        }

        public double SetCurrentTEControlTemperatureValue
        {
            get { return _SetCurrentTEControlTemperatureValue; }
            set
            {

                if (_SetCurrentTEControlTemperatureValue != value)
                {
                    _SetCurrentTEControlTemperatureValue = value;
                    RaisePropertyChanged("SetCurrentTEControlTemperatureValue");
                }
            }
        }

        public double SetCurrentTECMaximumCoolingCurrentValue
        {
            get { return _SetCurrentTECMaximumCoolingCurrentValue; }
            set
            {

                if (_SetCurrentTECMaximumCoolingCurrentValue != value)
                {
                    _SetCurrentTECMaximumCoolingCurrentValue = value;
                    RaisePropertyChanged("SetCurrentTECMaximumCoolingCurrentValue");
                }
            }
        }

        public double SetCurrentTECRefrigerationControlParameterKpValue
        {
            get { return _SetCurrentTECRefrigerationControlParameterKpValue; }
            set
            {

                if (_SetCurrentTECRefrigerationControlParameterKpValue != value)
                {
                    _SetCurrentTECRefrigerationControlParameterKpValue = value;
                    RaisePropertyChanged("SetCurrentTECRefrigerationControlParameterKpValue");
                }
            }
        }

        public double SetCurrentTECRefrigerationControlParameterKiValue
        {
            get { return _SetCurrentTECRefrigerationControlParameterKiValue; }
            set
            {

                if (_SetCurrentTECRefrigerationControlParameterKiValue != value)
                {
                    _SetCurrentTECRefrigerationControlParameterKiValue = value;
                    RaisePropertyChanged("SetCurrentTECRefrigerationControlParameterKiValue");
                }
            }
        }

        public double SetCurrentTECRefrigerationControlParameterKdValue
        {
            get { return _SetCurrentTECRefrigerationControlParameterKdValue; }
            set
            {

                if (_SetCurrentTECRefrigerationControlParameterKdValue != value)
                {
                    _SetCurrentTECRefrigerationControlParameterKdValue = value;
                    RaisePropertyChanged("SetCurrentTECRefrigerationControlParameterKdValue");
                }
            }
        }

        public double SetCurrentLaserLightPowerValueValue
        {
            get { return _SetCurrentLaserLightPowerValueValue; }
            set
            {

                if (_SetCurrentLaserLightPowerValueValue != value)
                {
                    _SetCurrentLaserLightPowerValueValue = value;
                    RaisePropertyChanged("SetCurrentLaserLightPowerValueValue");
                }
            }
        }

        public double SetCurrentTECCurrentCompensationCoefficientValue
        {
            get { return _SetCurrentTECCurrentCompensationCoefficientValue; }
            set
            {

                if (_SetCurrentTECCurrentCompensationCoefficientValue != value)
                {
                    _SetCurrentTECCurrentCompensationCoefficientValue = value;
                    RaisePropertyChanged("SetCurrentTECCurrentCompensationCoefficientValue");
                }
            }
        }

        public double SetCurrentPowerClosedloopControlParameterKpValue
        {
            get { return _SetCurrentPowerClosedloopControlParameterKpValue; }
            set
            {

                if (_SetCurrentPowerClosedloopControlParameterKpValue != value)
                {
                    _SetCurrentPowerClosedloopControlParameterKpValue = value;
                    RaisePropertyChanged("SetCurrentPowerClosedloopControlParameterKpValue");
                }
            }
        }

        public double SetCurrentPowerClosedloopControlParameterKiValue
        {
            get { return _SetCurrentPowerClosedloopControlParameterKiValue; }
            set
            {

                if (_SetCurrentPowerClosedloopControlParameterKiValue != value)
                {
                    _SetCurrentPowerClosedloopControlParameterKiValue = value;
                    RaisePropertyChanged("SetCurrentPowerClosedloopControlParameterKiValue");
                }
            }
        }

        public double SetCurrentPowerClosedloopControlParameterKdValue
        {
            get { return _SetCurrentPowerClosedloopControlParameterKdValue; }
            set
            {

                if (_SetCurrentPowerClosedloopControlParameterKdValue != value)
                {
                    _SetCurrentPowerClosedloopControlParameterKdValue = value;
                    RaisePropertyChanged("SetCurrentPowerClosedloopControlParameterKdValue");
                }
            }
        }

        public double SetCurrenPhotodiodeVoltageCorrespondingToLaserPowerValue
        {
            get { return _SetCurrenPhotodiodeVoltageCorrespondingToLaserPowerValue; }
            set
            {

                if (_SetCurrenPhotodiodeVoltageCorrespondingToLaserPowerValue != value)
                {
                    _SetCurrenPhotodiodeVoltageCorrespondingToLaserPowerValue = value;
                    RaisePropertyChanged("SetCurrenPhotodiodeVoltageCorrespondingToLaserPowerValue");
                }
            }
        }

        public double SetCurrentKValueofPhotodiodeTemperatureCurveValue
        {
            get { return _SetCurrentKValueofPhotodiodeTemperatureCurveValue; }
            set
            {

                if (_SetCurrentKValueofPhotodiodeTemperatureCurveValue != value)
                {
                    _SetCurrentKValueofPhotodiodeTemperatureCurveValue = value;
                    RaisePropertyChanged("SetCurrentKValueofPhotodiodeTemperatureCurveValue");
                }
            }
        }

        public double SetCurrentBValueofPhotodiodeTemperatureCurveValue
        {
            get { return _SetCurrentBValueofPhotodiodeTemperatureCurveValue; }
            set
            {

                if (_SetCurrentBValueofPhotodiodeTemperatureCurveValue != value)
                {
                    _SetCurrentBValueofPhotodiodeTemperatureCurveValue = value;
                    RaisePropertyChanged("SetCurrentBValueofPhotodiodeTemperatureCurveValue");
                }
            }
        }

        public double SetLaserMinCurrent
        {
            get { return _SetLaserMinCurrent; }
            set
            {

                if (_SetLaserMinCurrent != value)
                {
                    _SetLaserMinCurrent = value;
                    RaisePropertyChanged("SetLaserMinCurrent");
                }
            }
        }
        public double SetLaserMaxCurrent
        {
            get { return _SetLaserMaxCurrent; }
            set
            {

                if (_SetLaserMaxCurrent != value)
                {
                    _SetLaserMaxCurrent = value;
                    RaisePropertyChanged("SetLaserMaxCurrent");
                }
            }
        }

        public double SetOpticalPowerControlKiDownLimitLessThan15
        {
            get { return _SetOpticalPowerControlKiDownLimitLessThan15; }
            set
            {

                if (_SetOpticalPowerControlKiDownLimitLessThan15 != value)
                {
                    _SetOpticalPowerControlKiDownLimitLessThan15 = value;
                    RaisePropertyChanged("SetOpticalPowerControlKiDownLimitLessThan15");
                }
            }
        }
        public double SetOpticalPowerControlKiUpperLimitLessThan15
        {
            get { return _SetOpticalPowerControlKiUpperLimitLessThan15; }
            set
            {

                if (_SetOpticalPowerControlKiUpperLimitLessThan15 != value)
                {
                    _SetOpticalPowerControlKiUpperLimitLessThan15 = value;
                    RaisePropertyChanged("SetOpticalPowerControlKiUpperLimitLessThan15");
                }
            }
        }
        public double SetOpticalPowerControlKpDownLimitLessThan15
        {
            get { return _SetOpticalPowerControlKpDownLimitLessThan15; }
            set
            {

                if (_SetOpticalPowerControlKpDownLimitLessThan15 != value)
                {
                    _SetOpticalPowerControlKpDownLimitLessThan15 = value;
                    RaisePropertyChanged("SetOpticalPowerControlKpDownLimitLessThan15");
                }
            }
        }
        public double SetOpticalPowerControlKpUpperLimitLessThan15
        {
            get { return _SetOpticalPowerControlKpUpperLimitLessThan15; }
            set
            {

                if (_SetOpticalPowerControlKpUpperLimitLessThan15 != value)
                {
                    _SetOpticalPowerControlKpUpperLimitLessThan15 = value;
                    RaisePropertyChanged("SetOpticalPowerControlKpUpperLimitLessThan15");
                }
            }
        }
        public double SetOpticalPowerControlKiDownLimitLessThanOrEqual15
        {
            get { return _SetOpticalPowerControlKiDownLimitLessThanOrEqual15; }
            set
            {

                if (_SetOpticalPowerControlKiDownLimitLessThanOrEqual15 != value)
                {
                    _SetOpticalPowerControlKiDownLimitLessThanOrEqual15 = value;
                    RaisePropertyChanged("SetOpticalPowerControlKiDownLimitLessThanOrEqual15");
                }
            }
        }

        public double SetOpticalPowerControlKiUpperLimitLessThanOrEqual15
        {
            get { return _SetOpticalPowerControlKiUpperLimitLessThanOrEqual15; }
            set
            {

                if (_SetOpticalPowerControlKiUpperLimitLessThanOrEqual15 != value)
                {
                    _SetOpticalPowerControlKiUpperLimitLessThanOrEqual15 = value;
                    RaisePropertyChanged("SetOpticalPowerControlKiUpperLimitLessThanOrEqual15");
                }
            }
        }

        public double SetOpticalPowerControlKpDownLimitLessThanOrEqual15
        {
            get { return _SetOpticalPowerControlKpDownLimitLessThanOrEqual15; }
            set
            {

                if (_SetOpticalPowerControlKpDownLimitLessThanOrEqual15 != value)
                {
                    _SetOpticalPowerControlKpDownLimitLessThanOrEqual15 = value;
                    RaisePropertyChanged("SetOpticalPowerControlKpDownLimitLessThanOrEqual15");
                }
            }
        }
        public double SetOpticalPowerControlKpUpperLimitLessThanOrEqual15
        {
            get { return _SetOpticalPowerControlKpUpperLimitLessThanOrEqual15; }
            set
            {

                if (_SetOpticalPowerControlKpUpperLimitLessThanOrEqual15 != value)
                {
                    _SetOpticalPowerControlKpUpperLimitLessThanOrEqual15 = value;
                    RaisePropertyChanged("SetOpticalPowerControlKpUpperLimitLessThanOrEqual15");
                }
            }
        }
        public double SetOpticalPowerGreaterThan15mWKd
        {
            get { return _SetOpticalPowerGreaterThan15mWKd; }
            set
            {

                if (_SetOpticalPowerGreaterThan15mWKd != value)
                {
                    _SetOpticalPowerGreaterThan15mWKd = value;
                    RaisePropertyChanged("SetOpticalPowerGreaterThan15mWKd");
                }
            }
        }
        public double SetOpticalPowerGreaterThan15mWKi
        {
            get { return _SetOpticalPowerGreaterThan15mWKi; }
            set
            {

                if (_SetOpticalPowerGreaterThan15mWKi != value)
                {
                    _SetOpticalPowerGreaterThan15mWKi = value;
                    RaisePropertyChanged("SetOpticalPowerGreaterThan15mWKi");
                }
            }
        }
        public double SetOpticalPowerGreaterThan15mWKp
        {
            get { return _SetOpticalPowerGreaterThan15mWKp; }
            set
            {

                if (_SetOpticalPowerGreaterThan15mWKp != value)
                {
                    _SetOpticalPowerGreaterThan15mWKp = value;
                    RaisePropertyChanged("SetOpticalPowerGreaterThan15mWKp");
                }
            }
        }
        public double SetOpticalPowerLessThanOrEqual15mWKd
        {
            get { return _SetOpticalPowerLessThanOrEqual15mWKd; }
            set
            {

                if (_SetOpticalPowerLessThanOrEqual15mWKd != value)
                {
                    _SetOpticalPowerLessThanOrEqual15mWKd = value;
                    RaisePropertyChanged("SetOpticalPowerLessThanOrEqual15mWKd");
                }
            }
        }
        public double SetOpticalPowerLessThanOrEqual15mWKi
        {
            get { return _SetOpticalPowerLessThanOrEqual15mWKi; }
            set
            {

                if (_SetOpticalPowerLessThanOrEqual15mWKi != value)
                {
                    _SetOpticalPowerLessThanOrEqual15mWKi = value;
                    RaisePropertyChanged("SetOpticalPowerLessThanOrEqual15mWKi");
                }
            }
        }
        public double SetOpticalPowerLessThanOrEqual15mWKp
        {
            get { return _SetOpticalPowerLessThanOrEqual15mWKp; }
            set
            {

                if (_SetOpticalPowerLessThanOrEqual15mWKp != value)
                {
                    _SetOpticalPowerLessThanOrEqual15mWKp = value;
                    RaisePropertyChanged("SetOpticalPowerLessThanOrEqual15mWKp");
                }
            }
        }

        public double GetLaserMinCurrent
        {
            get { return _GetLaserMinCurrent; }
            set
            {

                if (_GetLaserMinCurrent != value)
                {
                    if ((value * 10) == Workspace.This.Uint16Code)
                    {
                        value = 0;
                    }
                    _GetLaserMinCurrent = value;
                    RaisePropertyChanged("GetLaserMinCurrent");
                }
            }
        }
        public double GetLaserMaxCurrent
        {
            get { return _GetLaserMaxCurrent; }
            set
            {

                if (_GetLaserMaxCurrent != value)
                {
                    if ((value * 10) ==Workspace.This.Uint16Code)
                    {
                        value = 0;
                    }
                    _GetLaserMaxCurrent = value;
                    RaisePropertyChanged("GetLaserMaxCurrent");
                }
            }
        }
        public double GetOpticalPowerControlKiDownLimitLessThan15
        {
            get { return _GetOpticalPowerControlKiDownLimitLessThan15; }
            set
            {

                if (_GetOpticalPowerControlKiDownLimitLessThan15 != value)
                {
                    if (value == Workspace.This.Uint32Code)
                    {
                        value = 0;
                    }
                    _GetOpticalPowerControlKiDownLimitLessThan15 = value;
                    RaisePropertyChanged("GetOpticalPowerControlKiDownLimitLessThan15");
                }
            }
        }
        public double GetOpticalPowerControlKiUpperLimitLessThan15
        {
            get { return _GetOpticalPowerControlKiUpperLimitLessThan15; }
            set
            {

                if (_GetOpticalPowerControlKiUpperLimitLessThan15 != value)
                {
                    if (value == Workspace.This.Uint32Code)
                    {
                        value = 0;
                    }
                    _GetOpticalPowerControlKiUpperLimitLessThan15 = value;
                    RaisePropertyChanged("GetOpticalPowerControlKiUpperLimitLessThan15");
                }
            }
        }
        public double GetOpticalPowerControlKpDownLimitLessThan15
        {
            get { return _GetOpticalPowerControlKpDownLimitLessThan15; }
            set
            {

                if (_GetOpticalPowerControlKpDownLimitLessThan15 != value)
                {
                    if (value == Workspace.This.Uint32Code)
                    {
                        value = 0;
                    }
                    _GetOpticalPowerControlKpDownLimitLessThan15 = value;
                    RaisePropertyChanged("GetOpticalPowerControlKpDownLimitLessThan15");
                }
            }
        }
        public double GetOpticalPowerControlKpUpperLimitLessThan15
        {
            get { return _GetOpticalPowerControlKpUpperLimitLessThan15; }
            set
            {

                if (_GetOpticalPowerControlKpUpperLimitLessThan15 != value)
                {
                    if (value == Workspace.This.Uint32Code)
                    {
                        value = 0;
                    }
                    _GetOpticalPowerControlKpUpperLimitLessThan15 = value;
                    RaisePropertyChanged("GetOpticalPowerControlKpUpperLimitLessThan15");
                }
            }
        }
        public double GetOpticalPowerControlKiDownLimitLessThanOrEqual15
        {
            get { return _GetOpticalPowerControlKiDownLimitLessThanOrEqual15; }
            set
            {

                if (_GetOpticalPowerControlKiDownLimitLessThanOrEqual15 != value)
                {
                    if (value == Workspace.This.Uint32Code)
                    {
                        value = 0;
                    }
                    _GetOpticalPowerControlKiDownLimitLessThanOrEqual15 = value;
                    RaisePropertyChanged("GetOpticalPowerControlKiDownLimitLessThanOrEqual15");
                }
            }
        }
        public double GetOpticalPowerControlKiUpperLimitLessThanOrEqual15
        {
            get { return _GetOpticalPowerControlKiUpperLimitLessThanOrEqual15; }
            set
            {

                if (_GetOpticalPowerControlKiUpperLimitLessThanOrEqual15 != value)
                {
                    if (value == Workspace.This.Uint32Code)
                    {
                        value = 0;
                    }
                    _GetOpticalPowerControlKiUpperLimitLessThanOrEqual15 = value;
                    RaisePropertyChanged("GetOpticalPowerControlKiUpperLimitLessThanOrEqual15");
                }
            }
        }
        public double GetOpticalPowerControlKpDownLimitLessThanOrEqual15
        {
            get { return _GetOpticalPowerControlKpDownLimitLessThanOrEqual15; }
            set
            {

                if (_GetOpticalPowerControlKpDownLimitLessThanOrEqual15 != value)
                {
                    if (value == Workspace.This.Uint32Code)
                    {
                        value = 0;
                    }
                    _GetOpticalPowerControlKpDownLimitLessThanOrEqual15 = value;
                    RaisePropertyChanged("GetOpticalPowerControlKpDownLimitLessThanOrEqual15");
                }
            }
        }
        public double GetOpticalPowerControlKpUpperLimitLessThanOrEqual15
        {
            get { return _GetOpticalPowerControlKpUpperLimitLessThanOrEqual15; }
            set
            {

                if (_GetOpticalPowerControlKpUpperLimitLessThanOrEqual15 != value)
                {
                    if (value == Workspace.This.Uint32Code)
                    {
                        value = 0;
                    }
                    _GetOpticalPowerControlKpUpperLimitLessThanOrEqual15 = value;
                    RaisePropertyChanged("GetOpticalPowerControlKpUpperLimitLessThanOrEqual15");
                }
            }
        }
        public double GetOpticalPowerGreaterThan15mWKd
        {
            get { return _GetOpticalPowerGreaterThan15mWKd; }
            set
            {

                if (_GetOpticalPowerGreaterThan15mWKd != value)
                {
                    if (value == Workspace.This.Uint16Code)
                    {
                        value = double.NaN;
                    }
                    _GetOpticalPowerGreaterThan15mWKd = value;
                    RaisePropertyChanged("GetOpticalPowerGreaterThan15mWKd");
                }
            }
        }
        public double GetOpticalPowerGreaterThan15mWKi
        {
            get { return _GetOpticalPowerGreaterThan15mWKi; }
            set
            {

                if (_GetOpticalPowerGreaterThan15mWKi != value)
                {
                    if (value == Workspace.This.Uint16Code)
                    {
                        value = double.NaN;
                    }
                    _GetOpticalPowerGreaterThan15mWKi = value;
                    RaisePropertyChanged("GetOpticalPowerGreaterThan15mWKi");
                }
            }
        }
        public double GetOpticalPowerGreaterThan15mWKp
        {
            get { return _GetOpticalPowerGreaterThan15mWKp; }
            set
            {

                if (_GetOpticalPowerGreaterThan15mWKp != value)
                {
                    if (value == Workspace.This.Uint16Code)
                    {
                        value = double.NaN;
                    }
                    _GetOpticalPowerGreaterThan15mWKp = value;
                    RaisePropertyChanged("GetOpticalPowerGreaterThan15mWKp");
                }
            }
        }
        public double GetOpticalPowerLessThanOrEqual15mWKd
        {
            get { return _GetOpticalPowerLessThanOrEqual15mWKd; }
            set
            {

                if (_GetOpticalPowerLessThanOrEqual15mWKd != value)
                {
                    if (value == Workspace.This.Uint16Code)
                    {
                        value = double.NaN;
                    }
                    _GetOpticalPowerLessThanOrEqual15mWKd = value;
                    RaisePropertyChanged("GetOpticalPowerLessThanOrEqual15mWKd");
                }
            }
        }

        public double GetOpticalPowerLessThanOrEqual15mWKi
        {
            get { return _GetOpticalPowerLessThanOrEqual15mWKi; }
            set
            {

                if (_GetOpticalPowerLessThanOrEqual15mWKi != value)
                {
                    if (value == Workspace.This.Uint16Code)
                    {
                        value = double.NaN;
                    }
                    _GetOpticalPowerLessThanOrEqual15mWKi = value;
                    RaisePropertyChanged("GetOpticalPowerLessThanOrEqual15mWKi");
                }
            }
        }
        public double GetOpticalPowerLessThanOrEqual15mWKp
        {
            get { return _GetOpticalPowerLessThanOrEqual15mWKp; }
            set
            {

                if (_GetOpticalPowerLessThanOrEqual15mWKp != value)
                {
                    if (value == Workspace.This.Uint16Code)
                    {
                        value = double.NaN;
                    }
                    _GetOpticalPowerLessThanOrEqual15mWKp = value;
                    RaisePropertyChanged("GetOpticalPowerLessThanOrEqual15mWKp");
                }
            }
        }


        public string GetCurrentLaserNoValue
        {
            get { return _GetCurrentLaserNoValue; }
            set
            {

                if (_GetCurrentLaserNoValue != value)
                {
                    _GetCurrentLaserNoValue = value;
                    RaisePropertyChanged("GetCurrentLaserNoValue");
                }
            }
        }

        public int GetCurrentLaserWavaLengthValue
        {
            get { return _GetCurrentLaserWavaLengthValue; }
            set
            {

                if (_GetCurrentLaserWavaLengthValue != value)
                {
                    _GetCurrentLaserWavaLengthValue = value;
                    RaisePropertyChanged("GetCurrentLaserWavaLengthValue");
                }
            }
        }

        public double GetCurrentLaserCurrentValueValue
        {
            get { return _GetCurrentLaserCurrentValueValue; }
            set
            {

                if (_GetCurrentLaserCurrentValueValue != value)
                {
                    _GetCurrentLaserCurrentValueValue = value;
                    RaisePropertyChanged("GetCurrentLaserCurrentValueValue");
                }
            }
        }

        public double GetCurrentLaserCorrespondingCurrentValueValue
        {
            get { return _GetCurrentLaserCorrespondingCurrentValueValue; }
            set
            {

                if (_GetCurrentLaserCorrespondingCurrentValueValue != value)
                {
                    _GetCurrentLaserCorrespondingCurrentValueValue = value;
                    RaisePropertyChanged("GetCurrentLaserCorrespondingCurrentValueValue");
                }
            }
        }

        public double GetCurrentTECActualTemperatureValue
        {
            get { return _GetCurrentTECActualTemperatureValue; }
            set
            {

                if (_GetCurrentTECActualTemperatureValue != value)
                {
                    _GetCurrentTECActualTemperatureValue = value;
                    RaisePropertyChanged("GetCurrentTECActualTemperatureValue");
                }
            }
        }

        public double GetCurrentTEControlTemperatureValue
        {
            get { return _GetCurrentTEControlTemperatureValue; }
            set
            {

                if (_GetCurrentTEControlTemperatureValue != value)
                {
                    _GetCurrentTEControlTemperatureValue = value;
                    RaisePropertyChanged("GetCurrentTEControlTemperatureValue");
                }
            }
        }

        public double GetCurrentTECMaximumCoolingCurrentValue
        {
            get { return _GetCurrentTECMaximumCoolingCurrentValue; }
            set
            {

                if (_GetCurrentTECMaximumCoolingCurrentValue != value)
                {
                    _GetCurrentTECMaximumCoolingCurrentValue = value;
                    RaisePropertyChanged("GetCurrentTECMaximumCoolingCurrentValue");
                }
            }
        }

        public double GetCurrentTECRefrigerationControlParameterKpValue
        {
            get { return _GetCurrentTECRefrigerationControlParameterKpValue; }
            set
            {

                if (_GetCurrentTECRefrigerationControlParameterKpValue != value)
                {
                    _GetCurrentTECRefrigerationControlParameterKpValue = value;
                    RaisePropertyChanged("GetCurrentTECRefrigerationControlParameterKpValue");
                }
            }
        }

        public double GetCurrentTECRefrigerationControlParameterKiValue
        {
            get { return _GetCurrentTECRefrigerationControlParameterKiValue; }
            set
            {

                if (_GetCurrentTECRefrigerationControlParameterKiValue != value)
                {
                    _GetCurrentTECRefrigerationControlParameterKiValue = value;
                    RaisePropertyChanged("GetCurrentTECRefrigerationControlParameterKiValue");
                }
            }
        }

        public double GetCurrentTECRefrigerationControlParameterKdValue
        {
            get { return _GetCurrentTECRefrigerationControlParameterKdValue; }
            set
            {

                if (_GetCurrentTECRefrigerationControlParameterKdValue != value)
                {
                    _GetCurrentTECRefrigerationControlParameterKdValue = value;
                    RaisePropertyChanged("GetCurrentTECRefrigerationControlParameterKdValue");
                }
            }
        }

        public double GetCurrentLaserLightPowerValueValue
        {
            get { return _GetCurrentLaserLightPowerValueValue; }
            set
            {

                if (_GetCurrentLaserLightPowerValueValue != value)
                {
                    _GetCurrentLaserLightPowerValueValue = value;
                    RaisePropertyChanged("GetCurrentLaserLightPowerValueValue");
                }
            }
        }

        public double GetCurrentTECWorkingStatusValue
        {
            get { return _GetCurrentTECWorkingStatusValue; }
            set
            {

                if (_GetCurrentTECWorkingStatusValue != value)
                {
                    _GetCurrentTECWorkingStatusValue = value;
                    RaisePropertyChanged("GetCurrentTECWorkingStatusValue");
                }
            }
        }

        public double GetCurrentTECCurrentDirectionValue
        {
            get { return _GetCurrentTECCurrentDirectionValue; }
            set
            {

                if (_GetCurrentTECCurrentDirectionValue != value)
                {
                    _GetCurrentTECCurrentDirectionValue = value;
                    RaisePropertyChanged("GetCurrentTECCurrentDirectionValue");
                }
            }
        }

        public double GetCurrenRadiatorTemperatureValue
        {
            get { return _GetCurrenRadiatorTemperatureValue; }
            set
            {

                if (_GetCurrenRadiatorTemperatureValue != value)
                {
                    _GetCurrenRadiatorTemperatureValue = value;
                    RaisePropertyChanged("GetCurrenRadiatorTemperatureValue");
                }
            }
        }

        public double GetCurrenTECCurrentCompensationCoefficientValue
        {
            get { return _GetCurrenTECCurrentCompensationCoefficientValue; }
            set
            {

                if (_GetCurrenTECCurrentCompensationCoefficientValue != value)
                {
                    _GetCurrenTECCurrentCompensationCoefficientValue = value;
                    RaisePropertyChanged("GetCurrenTECCurrentCompensationCoefficientValue");
                }
            }
        }

        public double Get_CurrentLaserLightPowerValueValue
        {
            get { return _Get_CurrentLaserLightPowerValueValue; }
            set
            {

                if (_Get_CurrentLaserLightPowerValueValue != value)
                {
                    _Get_CurrentLaserLightPowerValueValue = value;
                    RaisePropertyChanged("Get_CurrentLaserLightPowerValueValue");
                }
            }
        }

        public double GetCurrentPowerClosedloopControlParameterKpValue
        {
            get { return _GetCurrentPowerClosedloopControlParameterKpValue; }
            set
            {

                if (_GetCurrentPowerClosedloopControlParameterKpValue != value)
                {
                    _GetCurrentPowerClosedloopControlParameterKpValue = value;
                    RaisePropertyChanged("GetCurrentPowerClosedloopControlParameterKpValue");
                }
            }
        }

        public double GetCurrentPowerClosedloopControlParameterKiValue
        {
            get { return _GetCurrentPowerClosedloopControlParameterKiValue; }
            set
            {

                if (_GetCurrentPowerClosedloopControlParameterKiValue != value)
                {
                    _GetCurrentPowerClosedloopControlParameterKiValue = value;
                    RaisePropertyChanged("GetCurrentPowerClosedloopControlParameterKiValue");
                }
            }
        }

        public double GetCurrentPowerClosedloopControlParameterKdValue
        {
            get { return _GetCurrentPowerClosedloopControlParameterKdValue; }
            set
            {

                if (_GetCurrentPowerClosedloopControlParameterKdValue != value)
                {
                    _GetCurrentPowerClosedloopControlParameterKdValue = value;
                    RaisePropertyChanged("GetCurrentPowerClosedloopControlParameterKdValue");
                }
            }
        }

        public double GetCurrentPhotodiodeVoltageCorrespondingToLaserPowerValue
        {
            get { return _GetCurrentPhotodiodeVoltageCorrespondingToLaserPowerValue; }
            set
            {

                if (_GetCurrentPhotodiodeVoltageCorrespondingToLaserPowerValue != value)
                {
                    _GetCurrentPhotodiodeVoltageCorrespondingToLaserPowerValue = value;
                    RaisePropertyChanged("GetCurrentPhotodiodeVoltageCorrespondingToLaserPowerValue");
                }
            }
        }

        public double GetCurrentPhotodiodeVoltageValue
        {
            get { return _GetCurrentPhotodiodeVoltageValue; }
            set
            {

                if (_GetCurrentPhotodiodeVoltageValue != value)
                {
                    _GetCurrentPhotodiodeVoltageValue = value;
                    RaisePropertyChanged("GetCurrentPhotodiodeVoltageValue");
                }
            }
        }

        public double GetCurrentKValueofPhotodiodeTemperatureCurveValue
        {
            get { return _GetCurrentKValueofPhotodiodeTemperatureCurveValue; }
            set
            {

                if (_GetCurrentKValueofPhotodiodeTemperatureCurveValue != value)
                {
                    _GetCurrentKValueofPhotodiodeTemperatureCurveValue = value;
                    RaisePropertyChanged("GetCurrentKValueofPhotodiodeTemperatureCurveValue");
                }
            }
        }

        public double GetCurrentBValueofPhotodiodeTemperatureCurveValue
        {
            get { return _GetCurrentBValueofPhotodiodeTemperatureCurveValue; }
            set
            {

                if (_GetCurrentBValueofPhotodiodeTemperatureCurveValue != value)
                {
                    _GetCurrentBValueofPhotodiodeTemperatureCurveValue = value;
                    RaisePropertyChanged("GetCurrentBValueofPhotodiodeTemperatureCurveValue");
                }
            }
        }

        public double SetOpticalPowerKpValue
        {
            get { return _SetOpticalPowerKpValue; }
            set
            {

                if (_SetOpticalPowerKpValue != value)
                {
                    _SetOpticalPowerKpValue = value;
                    RaisePropertyChanged("SetOpticalPowerKpValue");
                }
            }
        }
        public double SetOpticalPowerKiValue
        {
            get { return _SetOpticalPowerKiValue; }
            set
            {

                if (_SetOpticalPowerKiValue != value)
                {
                    _SetOpticalPowerKiValue = value;
                    RaisePropertyChanged("SetOpticalPowerKiValue");
                }
            }
        }

        public double SetOpticalPowerKdValue
        {
            get { return _SetOpticalPowerKdValue; }
            set
            {

                if (_SetOpticalPowerKdValue != value)
                {
                    _SetOpticalPowerKdValue = value;
                    RaisePropertyChanged("SetOpticalPowerKdValue");
                }
            }
        }

        public double GetOpticalPowerKpValue
        {
            get { return _GetOpticalPowerKpValue; }
            set
            {

                if (_GetOpticalPowerKpValue != value)
                {
                    if (value == Workspace.This.Uint16Code)
                    {
                        value = double.NaN;
                    }
                    _GetOpticalPowerKpValue = value;
                    RaisePropertyChanged("GetOpticalPowerKpValue");
                }
            }
        }
        public double GetOpticalPowerKiValue
        {
            get { return _GetOpticalPowerKiValue; }
            set
            {

                if (_GetOpticalPowerKiValue != value)
                {
                    if (value == Workspace.This.Uint16Code)
                    {
                        value = double.NaN;
                    }
                    _GetOpticalPowerKiValue = value;
                    RaisePropertyChanged("GetOpticalPowerKiValue");
                }
            }
        }

        public double GetOpticalPowerKdValue
        {
            get { return _GetOpticalPowerKdValue; }
            set
            {

                if (_GetOpticalPowerKdValue != value)
                {
                    if (value == Workspace.This.Uint16Code)
                    {
                        value = double.NaN;
                    }
                    _GetOpticalPowerKdValue = value;
                    RaisePropertyChanged("GetOpticalPowerKdValue");
                }
            }
        }
        public string GetLaservNumberValue
        {
            get { return _GetLaservNumberValue; }
            set
            {

                if (_GetLaservNumberValue != value)
                {
                    _GetLaservNumberValue = value;
                    RaisePropertyChanged("GetLaservNumberValue");
                }
            }
        }

        public string GetLaserErrorCodeValue
        {
            get { return _GetLaserErrorCodeValue; }
            set
            {

                if (_GetLaserErrorCodeValue != value)
                {
                    _GetLaserErrorCodeValue = value;
                    RaisePropertyChanged("GetLaserErrorCodeValue");
                }
            }
        }

        public string GetLaserOpticalModuleNumberValue
        {
            get { return _GetLaserOpticalModuleNumberValue; }
            set
            {

                if (_GetLaserOpticalModuleNumberValue != value)
                {
                    _GetLaserOpticalModuleNumberValue = value;
                    RaisePropertyChanged("GetLaserOpticalModuleNumberValue");
                }
            }
        }
        #endregion
        #endregion

        #region 光学模块 IV隐藏部分

        #region SetCurrentLaserWavaCommand

        public ICommand SetCurrentSensorNoCommand
        {
            get
            {
                if (_SetCurrentSensorNoCommand == null)
                {
                    _SetCurrentSensorNoCommand = new RelayCommand(ExecuteSetCurrentSensorNoCommand, CanExecuteSetCurrentSensorNoCommand);
                }
                return _SetCurrentSensorNoCommand;
            }
        }
        public void ExecuteSetCurrentSensorNoCommand(object parameter)
        {
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            _LightModeSettingsPort.SetCurrentSensorNoValue(SetCurrentSensorNoValue);
            Thread.Sleep(200);
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentSensorNoValue();
                    Thread.Sleep(200);
                    //GetCurrentSensorNoValue = _LightModeSettingsPort.CurrentSensorNoValue;
                }

            }
           
        }
        public bool CanExecuteSetCurrentSensorNoCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region SetCurrentPGAMultipleCommand

        public ICommand SetCurrentPGAMultipleCommand
        {
            get
            {
                if (_SetCurrentPGAMultipleCommand == null)
                {
                    _SetCurrentPGAMultipleCommand = new RelayCommand(ExecuteSetCurrentPGAMultipleCommand, CanExecuteSetCurrentPGAMultipleCommand);
                }
                return _SetCurrentPGAMultipleCommand;
            }
        }
        public void ExecuteSetCurrentPGAMultipleCommand(object parameter)
        {
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            _LightModeSettingsPort.SetCurrentPGAMultiple(SelectedPGAMultipleL1Module.Value);
            Thread.Sleep(200);
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentPGAMultiple();
                    Thread.Sleep(200);
                    //GetCurrentPGAMultipleValue = _LightModeSettingsPort.CurrentPGAMultipleValue;
                }

            }
            
        }
        public bool CanExecuteSetCurrentPGAMultipleCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region SetCurrentAPDGainCommand

        public ICommand SetCurrentAPDGainCommand
        {
            get
            {
                if (_SetCurrentAPDGainCommand == null)
                {
                    _SetCurrentAPDGainCommand = new RelayCommand(ExecuteSetCurrentAPDGainCommand, CanExecuteSetCurrentAPDGainCommand);
                }
                return _SetCurrentAPDGainCommand;
            }
        }
        public void ExecuteSetCurrentAPDGainCommand(object parameter)
        {
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            _LightModeSettingsPort.SetCurrentAPDGain(SelectedGainComModuleL1.Value);
            Thread.Sleep(200);
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentAPDGain();
                    Thread.Sleep(200);
                    //GetCurrentAPDGainValue = _LightModeSettingsPort.CurrentAPDGainValue;
                }
            }
           
        }
        public bool CanExecuteSetCurrentAPDGainCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region SetCurrentLightIntensityCalibrationTemperatureCommand

        public ICommand SetCurrentLightIntensityCalibrationTemperatureCommand
        {
            get
            {
                if (_SetCurrentLightIntensityCalibrationTemperatureCommand == null)
                {
                    _SetCurrentLightIntensityCalibrationTemperatureCommand = new RelayCommand(ExecuteSetCurrentLightIntensityCalibrationTemperatureCommand, CanExecuteSetCurrentLightIntensityCalibrationTemperatureCommand);
                }
                return _SetCurrentLightIntensityCalibrationTemperatureCommand;
            }
        }
        public void ExecuteSetCurrentLightIntensityCalibrationTemperatureCommand(object parameter)
        {
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            _LightModeSettingsPort.SetCurrenLightIntensityCalibrationTemperature(SetCurrentLightIntensityCalibrationTemperatureValue);
            Thread.Sleep(200);
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentLightIntensityCalibrationTemperatureValue();
                    Thread.Sleep(200);
                    //GetCurrentLightIntensityCalibrationTemperatureValue = _LightModeSettingsPort.CurrentLightIntensityCalibrationTemperatureValue;
                }
            }
          
        }
        public bool CanExecuteSetCurrentLightIntensityCalibrationTemperatureCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region SetCurrenttGain50CalVolCommand

        public ICommand SetCurrenttGain50CalVolCommand
        {
            get
            {
                if (_SetCurrenttGain50CalVolCommand == null)
                {
                    _SetCurrenttGain50CalVolCommand = new RelayCommand(ExecuteSetCurrenttGain50CalVolCommand, CanExecuteSetCurrenttGain50CalVolCommand);
                }
                return _SetCurrenttGain50CalVolCommand;
            }
        }
        public void ExecuteSetCurrenttGain50CalVolCommand(object parameter)
        {
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            _LightModeSettingsPort.SetCurrenCurrenttGain50CalVol(SetCurrentGain50CalVolValue);
            Thread.Sleep(200);
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentGain50CalVolValue();
                    Thread.Sleep(200);
                    //GetCurrentGain50CalVolValue = _LightModeSettingsPort.CurrentGain50CalVolValue;
                }
            }
           
        }
        public bool CanExecuteSetCurrenttGain50CalVolCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region SetCurrenttGain100CalVolCommand

        public ICommand SetCurrenttGain100CalVolCommand
        {
            get
            {
                if (_SetCurrenttGain100CalVolCommand == null)
                {
                    _SetCurrenttGain100CalVolCommand = new RelayCommand(ExecuteSetCurrenttGain100CalVolCommand, CanExecuteSetCurrenttGain100CalVolCommand);
                }
                return _SetCurrenttGain100CalVolCommand;
            }
        }
        public void ExecuteSetCurrenttGain100CalVolCommand(object parameter)
        {
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            _LightModeSettingsPort.SetCurrenCurrenttGain100CalVol(SetCurrentGain100CalVolValue);
            Thread.Sleep(200);
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentGain100CalVolValue();
                    Thread.Sleep(200);
                    //GetCurrentGain100CalVolValue = _LightModeSettingsPort.CurrentGain100CalVolValue;
                }
            }
           
        }
        public bool CanExecuteSetCurrenttGain100CalVolCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region SetCurrenttGain150CalVolCommand

        public ICommand SetCurrenttGain150CalVolCommand
        {
            get
            {
                if (_SetCurrenttGain150CalVolCommand == null)
                {
                    _SetCurrenttGain150CalVolCommand = new RelayCommand(ExecuteSetCurrenttGain150CalVolCommand, CanExecuteSetCurrenttGain150CalVolCommand);
                }
                return _SetCurrenttGain150CalVolCommand;
            }
        }
        public void ExecuteSetCurrenttGain150CalVolCommand(object parameter)
        {
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            _LightModeSettingsPort.SetCurrenCurrenttGain150CalVol(SetCurrentGain150CalVolValue);
            Thread.Sleep(200);
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentGain150CalVolValue();
                    Thread.Sleep(200);
                    //GetCurrentGain150CalVolValue = _LightModeSettingsPort.CurrentGain150CalVolValue;
                }
            }
           
        }
        public bool CanExecuteSetCurrenttGain150CalVolCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region SetCurrenttGain200CalVolCommand

        public ICommand SetCurrenttGain200CalVolCommand
        {
            get
            {
                if (_SetCurrenttGain200CalVolCommand == null)
                {
                    _SetCurrenttGain200CalVolCommand = new RelayCommand(ExecuteSetCurrenttGain200CalVolCommand, CanExecuteSetCurrenttGain200CalVolCommand);
                }
                return _SetCurrenttGain200CalVolCommand;
            }
        }
        public void ExecuteSetCurrenttGain200CalVolCommand(object parameter)
        {
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            _LightModeSettingsPort.SetCurrenCurrenttGain200CalVol(SetCurrentGain200CalVolValue);
            Thread.Sleep(200);
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentGain200CalVolValue();
                    Thread.Sleep(200);
                    //GetCurrentGain200CalVolValue = _LightModeSettingsPort.CurrentGain200CalVolValue;
                }

            }
           
        }
        public bool CanExecuteSetCurrenttGain200CalVolCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region SetCurrenttGain250CalVolCommand

        public ICommand SetCurrenttGain250CalVolCommand
        {
            get
            {
                if (_SetCurrenttGain250CalVolCommand == null)
                {
                    _SetCurrenttGain250CalVolCommand = new RelayCommand(ExecuteSetCurrenttGain250CalVolCommand, CanExecuteSetCurrenttGain250CalVolCommand);
                }
                return _SetCurrenttGain250CalVolCommand;
            }
        }
        public void ExecuteSetCurrenttGain250CalVolCommand(object parameter)
        {
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            _LightModeSettingsPort.SetCurrenCurrenttGain250CalVol(SetCurrentGain250CalVolValue);
            Thread.Sleep(200);
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentGain250CalVolValue();
                    Thread.Sleep(200);
                    //GetCurrentGain250CalVolValue = _LightModeSettingsPort.CurrentGain250CalVolValue;
                }
            }
           
        }
        public bool CanExecuteSetCurrenttGain250CalVolCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region SetCurrenttGain300CalVolCommand

        public ICommand SetCurrenttGain300CalVolCommand
        {
            get
            {
                if (_SetCurrenttGain300CalVolCommand == null)
                {
                    _SetCurrenttGain300CalVolCommand = new RelayCommand(ExecuteSetCurrenttGain300CalVolCommand, CanExecuteSetCurrenttGain300CalVolCommand);
                }
                return _SetCurrenttGain300CalVolCommand;
            }
        }
        public void ExecuteSetCurrenttGain300CalVolCommand(object parameter)
        {
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            _LightModeSettingsPort.SetCurrenCurrenttGain300CalVol(SetCurrentGain300CalVolValue);
            Thread.Sleep(200);
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentGain300CalVolValue();
                    Thread.Sleep(200);
                    //GetCurrentGain300CalVolValue = _LightModeSettingsPort.CurrentGain300CalVolValue;
                }

            }
           
        }
        public bool CanExecuteSetCurrenttGain300CalVolCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region SetCurrenttGain400CalVolCommand

        public ICommand SetCurrenttGain400CalVolCommand
        {
            get
            {
                if (_SetCurrenttGain400CalVolCommand == null)
                {
                    _SetCurrenttGain400CalVolCommand = new RelayCommand(ExecuteSetCurrenttGain400CalVolCommand, CanExecuteSetCurrenttGain400CalVolCommand);
                }
                return _SetCurrenttGain400CalVolCommand;
            }
        }
        public void ExecuteSetCurrenttGain400CalVolCommand(object parameter)
        {
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            _LightModeSettingsPort.SetCurrenCurrenttGain400CalVol(SetCurrentGain400CalVolValue);
            Thread.Sleep(200);
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {;
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentGain400CalVolValue();
                    Thread.Sleep(200);
                    //GetCurrentGain400CalVolValue = _LightModeSettingsPort.CurrentGain400CalVolValue;
                }
            }
           
        }
        public bool CanExecuteSetCurrenttGain400CalVolCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region SetCurrenttGain500CalVolCommand

        public ICommand SetCurrenttGain500CalVolCommand
        {
            get
            {
                if (_SetCurrenttGain500CalVolCommand == null)
                {
                    _SetCurrenttGain500CalVolCommand = new RelayCommand(ExecuteSetCurrenttGain500CalVolCommand, CanExecuteSetCurrenttGain500CalVolCommand);
                }
                return _SetCurrenttGain500CalVolCommand;
            }
        }
        public void ExecuteSetCurrenttGain500CalVolCommand(object parameter)
        {
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            _LightModeSettingsPort.SetCurrenCurrenttGain500CalVol(SetCurrentGain500CalVolValue);
            Thread.Sleep(200);
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentGain500CalVolValue();
                    Thread.Sleep(200);
                    //GetCurrentGain500CalVolValue = _LightModeSettingsPort.CurrentGain500CalVolValue;
                }
            }
            
        }
        public bool CanExecuteSetCurrenttGain500CalVolCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region SetCurrenttPMTCtrlVolCommand

        public ICommand SetCurrenttPMTCtrlVolCommand
        {
            get
            {
                if (_SetCurrenttPMTCtrlVolCommand == null)
                {
                    _SetCurrenttPMTCtrlVolCommand = new RelayCommand(ExecuteSetCurrenttPMTCtrlVolCommand, CanExecuteSetCurrenttPMTCtrlVolCommand);
                }
                return _SetCurrenttPMTCtrlVolCommand;
            }
        }
        public void ExecuteSetCurrenttPMTCtrlVolCommand(object parameter)
        {
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            _LightModeSettingsPort.SetCurrenPMTCtrlVol(SetCurrentPMTCtrlVolValue);
            Thread.Sleep(200);
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentPMTCtrlVolValue();
                    Thread.Sleep(200);
                    //GetCurrentPMTCtrlVolValue = _LightModeSettingsPort.CurrentPMTCtrlVolValue;
                }
            }
           
        }
        public bool CanExecuteSetCurrenttPMTCtrlVolCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region SetCurrenttPMTCompensationCoefficientCommand

        public ICommand SetCurrenttPMTCompensationCoefficientCommand
        {
            get
            {
                if (_SetCurrenttPMTCompensationCoefficientCommand == null)
                {
                    _SetCurrenttPMTCompensationCoefficientCommand = new RelayCommand(ExecuteSetCurrenttPMTCompensationCoefficientCommand, CanExecuteSetCurrenttPMTCompensationCoefficientCommand);
                }
                return _SetCurrenttPMTCompensationCoefficientCommand;
            }
        }
        public void ExecuteSetCurrenttPMTCompensationCoefficientCommand(object parameter)
        {
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            _LightModeSettingsPort.SetCurrentPMTCompensationCoefficientValue(Convert.ToDouble(SetCurrentPMTCompensationCoefficientValue));
            Thread.Sleep(200);
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentPMTCompensationCoefficientValue();
                    Thread.Sleep(200);
                    //GetCurrentPMTCompensationCoefficientValue = _LightModeSettingsPort.CurrentPMTCompensationCoefficientValue;
                }
            }
           
        }
        public bool CanExecuteSetCurrenttPMTCompensationCoefficientCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region SetCurrenttADPTemperatureCalibrationFactorCommand

        public ICommand SetCurrenttADPTemperatureCalibrationFactorCommand
        {
            get
            {
                if (_SetCurrenttADPTemperatureCalibrationFactorCommand == null)
                {
                    _SetCurrenttADPTemperatureCalibrationFactorCommand = new RelayCommand(ExecuteSetCurrenttADPTemperatureCalibrationFactorCommand, CanExecuteSetCurrenttADPTemperatureCalibrationFactorCommand);
                }
                return _SetCurrenttADPTemperatureCalibrationFactorCommand;
            }
        }
        public void ExecuteSetCurrenttADPTemperatureCalibrationFactorCommand(object parameter)
        {
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            _LightModeSettingsPort.SetCurrentADPTemperatureCalibrationFactorValue(SelectedTemperatureCoeff);
            Thread.Sleep(200);
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentADPTemperatureCalibrationFactorValue();
                    Thread.Sleep(200);
                    //GetCurrentADPTemperatureCalibrationFactorValue = _LightModeSettingsPort.CurrentADPTemperatureCalibrationFactorValue;
                }
            }
           
        }
        public bool CanExecuteSetCurrenttADPTemperatureCalibrationFactorCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region SetIVOpticalModuleNumberCommand

        public ICommand SetIVOpticalModuleNumberCommand
        {
            get
            {
                if (_SetIVOpticalModuleNumberCommand == null)
                {
                    _SetIVOpticalModuleNumberCommand = new RelayCommand(ExecuteSetIVOpticalModuleNumberCommand, CanExecuteSetIVOpticalModuleNumberCommand);
                }
                return _SetIVOpticalModuleNumberCommand;
            }
        }
        public void ExecuteSetIVOpticalModuleNumberCommand(object parameter)
        {
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            _LightModeSettingsPort.SetIVOpticalModuleNumberValue(SetIVOpticalModuleNumberValue);
            Thread.Sleep(200);
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetIVOpticalModuleNumberValue();
                    Thread.Sleep(200);
                    //GetCurrentADPTemperatureCalibrationFactorValue = _LightModeSettingsPort.CurrentADPTemperatureCalibrationFactorValue;
                }
            }

        }
        public bool CanExecuteSetIVOpticalModuleNumberCommand(object parameter)
        {
            return true;
        }

        #endregion


        #region GetCurrentSensorNoCommand

        public ICommand GetCurrentSensorNoCommand
        {
            get
            {
                if (_GetCurrentSensorNoCommand == null)
                {
                    _GetCurrentSensorNoCommand = new RelayCommand(ExecuteGetCurrentSensorNoCommand, CanExecuteGetCurrentSensorNoCommand);
                }
                return _GetCurrentSensorNoCommand;
            }
        }
        public void ExecuteGetCurrentSensorNoCommand(object parameter)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentSensorNoValue();
                    Thread.Sleep(200);
                    //GetCurrentSensorNoValue = _LightModeSettingsPort.CurrentSensorNoValue;
                }

            }
           
        }
        public bool CanExecuteGetCurrentSensorNoCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region GetCurrentPGAMultipleCommand

        public ICommand GetCurrentPGAMultipleCommand
        {
            get
            {
                if (_GetCurrentPGAMultipleCommand == null)
                {
                    _GetCurrentPGAMultipleCommand = new RelayCommand(ExecuteGetCurrentPGAMultipleCommand, CanExecuteGetCurrentPGAMultipleCommand);
                }
                return _GetCurrentPGAMultipleCommand;
            }
        }
        public void ExecuteGetCurrentPGAMultipleCommand(object parameter)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentPGAMultiple();
                    Thread.Sleep(200);
                    //GetCurrentPGAMultipleValue = _LightModeSettingsPort.CurrentPGAMultipleValue;
                }
            }
           
        }
        public bool CanExecuteGetCurrentPGAMultipleCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region GetCurrentAPDGainCommand

        public ICommand GetCurrentAPDGainCommand
        {
            get
            {
                if (_GetCurrentAPDGainCommand == null)
                {
                    _GetCurrentAPDGainCommand = new RelayCommand(ExecuteGetCurrentAPDGainCommand, CanExecuteGetCurrentAPDGainCommand);
                }
                return _GetCurrentAPDGainCommand;
            }
        }
        public void ExecuteGetCurrentAPDGainCommand(object parameter)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentAPDGain();
                    Thread.Sleep(200);
                    //GetCurrentAPDGainValue = _LightModeSettingsPort.CurrentAPDGainValue;
                }
            }
           
        }
        public bool CanExecuteGetCurrentAPDGainCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region GetCurrentAPDTempCommand

        public ICommand GetCurrentAPDTempCommand
        {
            get
            {
                if (_GetCurrentAPDTempCommand == null)
                {
                    _GetCurrentAPDTempCommand = new RelayCommand(ExecuteGetCurrentAPDTempCommand, CanExecuteGetCurrentAPDTempCommand);
                }
                return _GetCurrentAPDTempCommand;
            }
        }
        public void ExecuteGetCurrentAPDTempCommand(object parameter)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentAPDTempValue();
                    Thread.Sleep(200);
                    //GetCurrentAPDTempValue = _LightModeSettingsPort.CurrentAPDTempValue;
                }

            }
           
        }
        public bool CanExecuteGetCurrentAPDTempCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region GetCurrentAPDHighVoltageCommand

        public ICommand GetCurrentAPDHighVoltageCommand
        {
            get
            {
                if (_GetCurrentAPDHighVoltageCommand == null)
                {
                    _GetCurrentAPDHighVoltageCommand = new RelayCommand(ExecuteGetCurrentAPDHighVoltageCommand, CanExecuteGetCurrentAPDHighVoltageCommand);
                }
                return _GetCurrentAPDHighVoltageCommand;
            }
        }
        public void ExecuteGetCurrentAPDHighVoltageCommand(object parameter)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentAPDHighVoltageValue();
                    Thread.Sleep(200);
                    //GetCurrentAPDHighVoltageValue = _LightModeSettingsPort.CurrentAPDHighVoltageValue;
                }
            }
           
        }
        public bool CanExecuteGetCurrentAPDHighVoltageCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region GetCurrentLightIntensityCalibrationTemperatureCommand

        public ICommand GetCurrentLightIntensityCalibrationTemperatureCommand
        {
            get
            {
                if (_GetCurrentLightIntensityCalibrationTemperatureCommand == null)
                {
                    _GetCurrentLightIntensityCalibrationTemperatureCommand = new RelayCommand(ExecuteGetCurrentLightIntensityCalibrationTemperatureCommand, CanExecuteGetCurrentLightIntensityCalibrationTemperatureCommand);
                }
                return _GetCurrentLightIntensityCalibrationTemperatureCommand;
            }
        }
        public void ExecuteGetCurrentLightIntensityCalibrationTemperatureCommand(object parameter)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentLightIntensityCalibrationTemperatureValue();
                    Thread.Sleep(200);
                    //GetCurrentLightIntensityCalibrationTemperatureValue = _LightModeSettingsPort.CurrentLightIntensityCalibrationTemperatureValue;
                }
            }
          
        }
        public bool CanExecuteGetCurrentLightIntensityCalibrationTemperatureCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region GetCurrentGain50CalVolCommand

        public ICommand GetCurrentGain50CalVolCommand
        {
            get
            {
                if (_GetCurrentGain50CalVolCommand == null)
                {
                    _GetCurrentGain50CalVolCommand = new RelayCommand(ExecuteGetCurrentGain50CalVolCommand, CanExecuteGetCurrentGain50CalVolCommand);
                }
                return _GetCurrentGain50CalVolCommand;
            }
        }
        public void ExecuteGetCurrentGain50CalVolCommand(object parameter)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentGain50CalVolValue();
                    Thread.Sleep(200);
                    //GetCurrentGain50CalVolValue = _LightModeSettingsPort.CurrentGain50CalVolValue;
                }
            }
           
        }
        public bool CanExecuteGetCurrentGain50CalVolCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region GetCurrentGain100CalVolCommand

        public ICommand GetCurrentGain100CalVolCommand
        {
            get
            {
                if (_GetCurrentGain100CalVolCommand == null)
                {
                    _GetCurrentGain100CalVolCommand = new RelayCommand(ExecuteGetCurrentGain100CalVolCommand, CanExecuteGetCurrentGain100CalVolCommand);
                }
                return _GetCurrentGain100CalVolCommand;
            }
        }
        public void ExecuteGetCurrentGain100CalVolCommand(object parameter)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentGain100CalVolValue();
                    Thread.Sleep(200);
                    //GetCurrentGain100CalVolValue = _LightModeSettingsPort.CurrentGain100CalVolValue;
                }
            }
          
        }
        public bool CanExecuteGetCurrentGain100CalVolCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region GetCurrentGain150CalVolCommand

        public ICommand GetCurrentGain150CalVolCommand
        {
            get
            {
                if (_GetCurrentGain150CalVolCommand == null)
                {
                    _GetCurrentGain150CalVolCommand = new RelayCommand(ExecuteGetCurrentGain150CalVolCommand, CanExecuteGetCurrentGain150CalVolCommand);
                }
                return _GetCurrentGain150CalVolCommand;
            }
        }
        public void ExecuteGetCurrentGain150CalVolCommand(object parameter)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentGain150CalVolValue();
                    Thread.Sleep(200);
                    //GetCurrentGain150CalVolValue = _LightModeSettingsPort.CurrentGain150CalVolValue;
                }
            }
            
        }
        public bool CanExecuteGetCurrentGain150CalVolCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region GetCurrentGain200CalVolCommand

        public ICommand GetCurrentGain200CalVolCommand
        {
            get
            {
                if (_GetCurrentGain200CalVolCommand == null)
                {
                    _GetCurrentGain200CalVolCommand = new RelayCommand(ExecuteGetCurrentGain200CalVolCommand, CanExecuteGetCurrentGain200CalVolCommand);
                }
                return _GetCurrentGain200CalVolCommand;
            }
        }
        public void ExecuteGetCurrentGain200CalVolCommand(object parameter)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentGain200CalVolValue();
                    Thread.Sleep(200);
                    //GetCurrentGain200CalVolValue = _LightModeSettingsPort.CurrentGain200CalVolValue;
                }

            }
           
        }
        public bool CanExecuteGetCurrentGain200CalVolCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region GetCurrentGain250CalVolCommand

        public ICommand GetCurrentGain250CalVolCommand
        {
            get
            {
                if (_GetCurrentGain250CalVolCommand == null)
                {
                    _GetCurrentGain250CalVolCommand = new RelayCommand(ExecuteGetCurrentGain250CalVolCommand, CanExecuteGetCurrentGain250CalVolCommand);
                }
                return _GetCurrentGain250CalVolCommand;
            }
        }
        public void ExecuteGetCurrentGain250CalVolCommand(object parameter)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentGain250CalVolValue();
                    Thread.Sleep(200);
                    //GetCurrentGain250CalVolValue = _LightModeSettingsPort.CurrentGain250CalVolValue;
                }
            }
           
        }
        public bool CanExecuteGetCurrentGain250CalVolCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region GetCurrentGain300CalVolCommand

        public ICommand GetCurrentGain300CalVolCommand
        {
            get
            {
                if (_GetCurrentGain300CalVolCommand == null)
                {
                    _GetCurrentGain300CalVolCommand = new RelayCommand(ExecuteGetCurrentGain300CalVolCommand, CanExecuteGetCurrentGain300CalVolCommand);
                }
                return _GetCurrentGain300CalVolCommand;
            }
        }
        public void ExecuteGetCurrentGain300CalVolCommand(object parameter)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentGain300CalVolValue();
                    Thread.Sleep(200);
                    //GetCurrentGain300CalVolValue = _LightModeSettingsPort.CurrentGain300CalVolValue;
                }
            }
           
        }
        public bool CanExecuteGetCurrentGain300CalVolCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region GetCurrentGain400CalVolCommand

        public ICommand GetCurrentGain400CalVolCommand
        {
            get
            {
                if (_GetCurrentGain400CalVolCommand == null)
                {
                    _GetCurrentGain400CalVolCommand = new RelayCommand(ExecuteGetCurrentGain400CalVolCommand, CanExecuteGetCurrentGain400CalVolCommand);
                }
                return _GetCurrentGain400CalVolCommand;
            }
        }
        public void ExecuteGetCurrentGain400CalVolCommand(object parameter)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentGain400CalVolValue();
                    Thread.Sleep(200);
                    //GetCurrentGain400CalVolValue = _LightModeSettingsPort.CurrentGain400CalVolValue;

                }
            }
           
        }
        public bool CanExecuteGetCurrentGain400CalVolCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region GetCurrentGain500CalVolCommand

        public ICommand GetCurrentGain500CalVolCommand
        {
            get
            {
                if (_GetCurrentGain500CalVolCommand == null)
                {
                    _GetCurrentGain500CalVolCommand = new RelayCommand(ExecuteGetCurrentGain500CalVolCommand, CanExecuteGetCurrentGain500CalVolCommand);
                }
                return _GetCurrentGain500CalVolCommand;
            }
        }
        public void ExecuteGetCurrentGain500CalVolCommand(object parameter)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentGain500CalVolValue();
                    Thread.Sleep(200);
                    //GetCurrentGain500CalVolValue = _LightModeSettingsPort.CurrentGain500CalVolValue;
                }
            }
           
        }
        public bool CanExecuteGetCurrentGain500CalVolCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region GetCurrentPMTCtrlVolCommand

        public ICommand GetCurrentPMTCtrlVolCommand
        {
            get
            {
                if (_GetCurrentPMTCtrlVolCommand == null)
                {
                    _GetCurrentPMTCtrlVolCommand = new RelayCommand(ExecuteGetCurrentPMTCtrlVolCommand, CanExecuteGetCurrentPMTCtrlVolCommand);
                }
                return _GetCurrentPMTCtrlVolCommand;
            }
        }
        public void ExecuteGetCurrentPMTCtrlVolCommand(object parameter)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentPMTCtrlVolValue();
                    Thread.Sleep(200);
                    //GetCurrentPMTCtrlVolValue = _LightModeSettingsPort.CurrentPMTCtrlVolValue;
                }

            }
         
        }
        public bool CanExecuteGetCurrentPMTCtrlVolCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region GetCurrentPMTCompensationCoefficientCommand

        public ICommand GetCurrentPMTCompensationCoefficientCommand
        {
            get
            {
                if (_GetCurrentPMTCompensationCoefficientCommand == null)
                {
                    _GetCurrentPMTCompensationCoefficientCommand = new RelayCommand(ExecuteGetCurrentPMTCompensationCoefficientCommand, CanExecuteGetCurrentPMTCompensationCoefficientCommand);
                }
                return _GetCurrentPMTCompensationCoefficientCommand;
            }
        }
        public void ExecuteGetCurrentPMTCompensationCoefficientCommand(object parameter)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentPMTCompensationCoefficientValue();
                    Thread.Sleep(200);
                    //GetCurrentPMTCompensationCoefficientValue = _LightModeSettingsPort.CurrentPMTCompensationCoefficientValue;
                }

            }
           
        }
        public bool CanExecuteGetCurrentPMTCompensationCoefficientCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region GetCurrentTempSenser1282ADValueCommand

        public ICommand GetCurrentTempSenser1282ADValueCommand
        {
            get
            {
                if (_GetCurrentTempSenser1282ADValueCommand == null)
                {
                    _GetCurrentTempSenser1282ADValueCommand = new RelayCommand(ExecuteGetCurrentTempSenser1282ADValueCommand, CanExecuteGetCurrentTempSenser1282ADValueCommand);
                }
                return _GetCurrentTempSenser1282ADValueCommand;
            }
        }
        public void ExecuteGetCurrentTempSenser1282ADValueCommand(object parameter)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentTempSenser1282ADValue();
                    Thread.Sleep(200);
                    //GetCurrentTempSenser1282ADValue = _LightModeSettingsPort.CurrentTempSenser1282ADValue;
                }
            }
           
        }
        public bool CanExecuteGetCurrentTempSenser1282ADValueCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region GetCurrentTempSenser6459ADValueCommand

        public ICommand GetCurrentTempSenser6459ADValueCommand
        {
            get
            {
                if (_GetCurrentTempSenser6459ADValueCommand == null)
                {
                    _GetCurrentTempSenser6459ADValueCommand = new RelayCommand(ExecuteGetCurrentTempSenser6459ADValueCommand, CanExecuteExecuteGetCurrentTempSenser6459ADValueCommand);
                }
                return _GetCurrentTempSenser6459ADValueCommand;
            }
        }
        public void ExecuteGetCurrentTempSenser6459ADValueCommand(object parameter)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentTempSenser6459ADValue();
                    Thread.Sleep(200);
                    //GetCurrentTempSenser6459ADValue = _LightModeSettingsPort.CurrentTempSenser6459ADValue;
                }

            }
           
        }
        public bool CanExecuteExecuteGetCurrentTempSenser6459ADValueCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region GetCurrentTempSenserADValueCommand

        public ICommand GetCurrentTempSenserADValueCommand
        {
            get
            {
                if (_GetCurrentTempSenserADValueCommand == null)
                {
                    _GetCurrentTempSenserADValueCommand = new RelayCommand(ExecuteGetCurrentTempSenserADValueCommand, CanExecuteGetCurrentTempSenserADValueCommand);
                }
                return _GetCurrentTempSenserADValueCommand;
            }
        }
        public void ExecuteGetCurrentTempSenserADValueCommand(object parameter)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentTempSenserADValue();
                    Thread.Sleep(200);
                    //GetCurrentTempSenserADValue = _LightModeSettingsPort.CurrentTempSenserADValue;
                }

            }
           
        }
        public bool CanExecuteGetCurrentTempSenserADValueCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region GetCurrentIVBoardRunningStateCommand

        public ICommand GetCurrentIVBoardRunningStateCommand
        {
            get
            {
                if (_GetCurrentIVBoardRunningStateCommand == null)
                {
                    _GetCurrentIVBoardRunningStateCommand = new RelayCommand(ExecuteGetCurrentIVBoardRunningStateCommand, CanExecuteGetCurrentIVBoardRunningStateCommand);
                }
                return _GetCurrentIVBoardRunningStateCommand;
            }
        }
        public void ExecuteGetCurrentIVBoardRunningStateCommand(object parameter)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentIVBoardRunningStateValue();
                    Thread.Sleep(200);
                    //GetCurrentIVBoardRunningStateValue = _LightModeSettingsPort.CurrentIVBoardRunningStateValue;
                }
            }
           
        }
        public bool CanExecuteGetCurrentIVBoardRunningStateCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region GetCurrentTemperatureSensorSamplingVoltageCommand

        public ICommand GetCurrentTemperatureSensorSamplingVoltageCommand
        {
            get
            {
                if (_GetCurrentTemperatureSensorSamplingVoltageCommand == null)
                {
                    _GetCurrentTemperatureSensorSamplingVoltageCommand = new RelayCommand(ExecuteGetCurrentTemperatureSensorSamplingVoltageCommand, CanExecuteGetCurrentTemperatureSensorSamplingVoltageCommand);
                }
                return _GetCurrentTemperatureSensorSamplingVoltageCommand;
            }
        }
        public void ExecuteGetCurrentTemperatureSensorSamplingVoltageCommand(object parameter)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentTemperatureSensorSamplingVoltageValue();
                    Thread.Sleep(200);
                    //GetCurrentTemperatureSensorSamplingVoltageValue = _LightModeSettingsPort.CurrentTemperatureSensorSamplingVoltageValue;

                }

            }
           
        }
        public bool CanExecuteGetCurrentTemperatureSensorSamplingVoltageCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region GetCurrentADPTemperatureCalibrationFactorCommand

        public ICommand GetCurrentADPTemperatureCalibrationFactorCommand
        {
            get
            {
                if (_GetCurrentADPTemperatureCalibrationFactorCommand == null)
                {
                    _GetCurrentADPTemperatureCalibrationFactorCommand = new RelayCommand(ExecuteGetCurrentADPTemperatureCalibrationFactorCommand, CanExecuteGetCurrentADPTemperatureCalibrationFactorCommand);
                }
                return _GetCurrentADPTemperatureCalibrationFactorCommand;
            }
        }
        public void ExecuteGetCurrentADPTemperatureCalibrationFactorCommand(object parameter)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentADPTemperatureCalibrationFactorValue();
                    Thread.Sleep(200);
                    //GetCurrentADPTemperatureCalibrationFactorValue = _LightModeSettingsPort.CurrentADPTemperatureCalibrationFactorValue;
                }

            }
           
        }
        public bool CanExecuteGetCurrentADPTemperatureCalibrationFactorCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region GetIVvNumberCommand

        public ICommand GetIVvNumberCommand
        {
            get
            {
                if (_GetIVvNumberCommand == null)
                {
                    _GetIVvNumberCommand = new RelayCommand(ExecuteGetIVvNumberCommand, CanExecuteGetIVvNumberCommand);
                }
                return _GetIVvNumberCommand;
            }
        }
        public void ExecuteGetIVvNumberCommand(object parameter)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetIVvNumberValue();
                    Thread.Sleep(200);
                    //GetCurrentADPTemperatureCalibrationFactorValue = _LightModeSettingsPort.CurrentADPTemperatureCalibrationFactorValue;
                }

            }

        }
        public bool CanExecuteGetIVvNumberCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region GetIVErrorCodeCommand

        public ICommand GetIVErrorCodeCommand
        {
            get
            {
                if (_GetIVErrorCodeCommand == null)
                {
                    _GetIVErrorCodeCommand = new RelayCommand(ExecuteGetIVErrorCodeCommand, CanExecuteIVErrorCodeCommand);
                }
                return _GetIVErrorCodeCommand;
            }
        }
        public void ExecuteGetIVErrorCodeCommand(object parameter)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetIVErrorCodeValue();
                    Thread.Sleep(200);
                    //GetCurrentADPTemperatureCalibrationFactorValue = _LightModeSettingsPort.CurrentADPTemperatureCalibrationFactorValue;
                }

            }

        }
        public bool CanExecuteIVErrorCodeCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region GetIVOpticalModuleNumberCommand

        public ICommand GetIVOpticalModuleNumberCommand
        {
            get
            {
                if (_GetIVOpticalModuleNumberCommand == null)
                {
                    _GetIVOpticalModuleNumberCommand = new RelayCommand(ExecuteGetIVOpticalModuleNumberCommand, CanExecuteIVOpticalModuleNumberCommand);
                }
                return _GetIVOpticalModuleNumberCommand;
            }
        }
        public void ExecuteGetIVOpticalModuleNumberCommand(object parameter)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetIVOpticalModuleNumberValue();
                    Thread.Sleep(200);
                    //GetCurrentADPTemperatureCalibrationFactorValue = _LightModeSettingsPort.CurrentADPTemperatureCalibrationFactorValue;
                }

            }

        }
        public bool CanExecuteIVOpticalModuleNumberCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region attribute

        public ObservableCollection<double> TemperatureCoeffOptions
        {
            get
            {
                return _TemperatureCoeffOptions;
            }
        }
        public ObservableCollection<string> CommunicationControlOptions
        {
            get
            {
                return _CommunicationControlOptions;
            }
        }
        public string SelectedCommunicationControl
        {
            get
            {
                return _SelectedCommunicationControl;
            }
            set
            {
                if (_SelectedCommunicationControl != value)
                {
                    _SelectedCommunicationControl = value;
                    if (_SelectedCommunicationControl == "开启")
                    {
                        EthernetDevice.SetCommunicationControl(1);
                    }
                    else if (_SelectedCommunicationControl == "关闭")
                    {
                        EthernetDevice.SetCommunicationControl(0);
                    }
                    RaisePropertyChanged("SelectedCommunicationControl");
                }
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
                    RaisePropertyChanged("SelectedTemperatureCoeff");
                }
            }
        }
        public ObservableCollection<APDPgaType> PGAOptionsModule
        {
            get { return _PGAOptionsModule; }
        }
        public APDPgaType SelectedPGAMultipleL1Module
        {
            get { return _SelectedPGAMultipleL1Module; }
            set
            {
                if (_SelectedPGAMultipleL1Module != value)
                {
                    _SelectedPGAMultipleL1Module = value;
                    RaisePropertyChanged("SelectedPGAMultipleL1Module");
                }
            }
        }

        public ObservableCollection<APDGainType> GainMultiplerModule
        {
            get { return _APDGainOptions; }
        }
        public APDGainType SelectedGainComModuleL1
        {
            get { return _SelectedGainComModuleL1; }
            set
            {
                if (_SelectedGainComModuleL1 != value)
                {
                    _SelectedGainComModuleL1 = value;
                }
            }
        }
        public string SetCurrentSensorNoValue
        {
            get { return _SetCurrentSensorNoValue; }
            set
            {

                if (_SetCurrentSensorNoValue != value)
                {
                    _SetCurrentSensorNoValue = value;
                    RaisePropertyChanged("SetCurrentSensorNoValue");
                }
            }
        }
        public string GetCurrentSensorNoValue
        {
            get { return _GetCurrentSensorNoValue; }
            set
            {

                if (_GetCurrentSensorNoValue != value)
                {
                    _GetCurrentSensorNoValue = value;
                    RaisePropertyChanged("GetCurrentSensorNoValue");
                }
            }
        }

        public int SetCurrentPGAMultipleValue
        {
            get { return _SetCurrentPGAMultipleValue; }
            set
            {

                if (_SetCurrentPGAMultipleValue != value)
                {
                    _SetCurrentPGAMultipleValue = value;
                    RaisePropertyChanged("SetCurrentPGAMultipleValue");
                }
            }
        }

        public int GetCurrentPGAMultipleValue
        {
            get { return _GetCurrentPGAMultipleValue; }
            set
            {

                if (_GetCurrentPGAMultipleValue != value)
                {
                    _GetCurrentPGAMultipleValue = value;
                    RaisePropertyChanged("GetCurrentPGAMultipleValue");
                }
            }
        }

        public int SetCurrentAPDGainValue
        {
            get { return _SetCurrentAPDGainValue; }
            set
            {

                if (_SetCurrentAPDGainValue != value)
                {
                    _SetCurrentAPDGainValue = value;
                    RaisePropertyChanged("SetCurrentAPDGainValue");
                }
            }
        }

        public int GetCurrentAPDGainValue
        {
            get { return _GetCurrentAPDGainValue; }
            set
            {

                if (_GetCurrentAPDGainValue != value)
                {
                    _GetCurrentAPDGainValue = value;
                    RaisePropertyChanged("GetCurrentAPDGainValue");
                }
            }
        }

        public double GetCurrentAPDTempValue
        {
            get { return _GetCurrentAPDTempValue; }
            set
            {

                if (_GetCurrentAPDTempValue != value)
                {
                    _GetCurrentAPDTempValue = value;
                    RaisePropertyChanged("GetCurrentAPDTempValue");
                }
            }
        }

        public double GetCurrentAPDHighVoltageValue
        {
            get { return _GetCurrentAPDHighVoltageValue; }
            set
            {

                if (_GetCurrentAPDHighVoltageValue != value)
                {
                    _GetCurrentAPDHighVoltageValue = value;
                    RaisePropertyChanged("GetCurrentAPDHighVoltageValue");
                }
            }
        }


        public double SetCurrentLightIntensityCalibrationTemperatureValue
        {
            get { return _SetCurrentLightIntensityCalibrationTemperatureValue; }
            set
            {

                if (_SetCurrentLightIntensityCalibrationTemperatureValue != value)
                {
                    _SetCurrentLightIntensityCalibrationTemperatureValue = value;
                    RaisePropertyChanged("SetCurrentLightIntensityCalibrationTemperatureValue");
                }
            }
        }

        public double GetCurrentLightIntensityCalibrationTemperatureValue
        {
            get { return _GetCurrentLightIntensityCalibrationTemperatureValue; }
            set
            {

                if (_GetCurrentLightIntensityCalibrationTemperatureValue != value)
                {
                    _GetCurrentLightIntensityCalibrationTemperatureValue = value;
                    RaisePropertyChanged("GetCurrentLightIntensityCalibrationTemperatureValue");
                }
            }
        }
        
        public double SetCurrentGain50CalVolValue
        {
            get { return _SetCurrentGain50CalVolValue; }
            set
            {

                if (_SetCurrentGain50CalVolValue != value)
                {
                    _SetCurrentGain50CalVolValue = value;
                    RaisePropertyChanged("SetCurrentGain50CalVolValue");
                }
            }
        }

        public double GetCurrentGain50CalVolValue
        {
            get { return _GetCurrentGain50CalVolValue; }
            set
            {

                if (_GetCurrentGain50CalVolValue != value)
                {
                    _GetCurrentGain50CalVolValue = value;
                    RaisePropertyChanged("GetCurrentGain50CalVolValue");
                }
            }
        }
        
        public double SetCurrentGain100CalVolValue
        {
            get { return _SetCurrentGain100CalVolValue; }
            set
            {

                if (_SetCurrentGain100CalVolValue != value)
                {
                    _SetCurrentGain100CalVolValue = value;
                    RaisePropertyChanged("SetCurrentGain100CalVolValue");
                }
            }
        }

        public double GetCurrentGain100CalVolValue
        {
            get { return _GetCurrentGain100CalVolValue; }
            set
            {

                if (_GetCurrentGain100CalVolValue != value)
                {
                    _GetCurrentGain100CalVolValue = value;
                    RaisePropertyChanged("GetCurrentGain100CalVolValue");
                }
            }
        }

        public double SetCurrentGain150CalVolValue
        {
            get { return _SetCurrentGain150CalVolValue; }
            set
            {

                if (_SetCurrentGain150CalVolValue != value)
                {
                    _SetCurrentGain150CalVolValue = value;
                    RaisePropertyChanged("SetCurrentGain150CalVolValue");
                }
            }
        }

        public double GetCurrentGain150CalVolValue
        {
            get { return _GetCurrentGain150CalVolValue; }
            set
            {

                if (_GetCurrentGain150CalVolValue != value)
                {
                    _GetCurrentGain150CalVolValue = value;
                    RaisePropertyChanged("GetCurrentGain150CalVolValue");
                }
            }
        }

        public double SetCurrentGain200CalVolValue
        {
            get { return _SetCurrentGain200CalVolValue; }
            set
            {

                if (_SetCurrentGain200CalVolValue != value)
                {
                    _SetCurrentGain200CalVolValue = value;
                    RaisePropertyChanged("SetCurrentGain200CalVolValue");
                }
            }
        }

        public double GetCurrentGain200CalVolValue
        {
            get { return _GetCurrentGain200CalVolValue; }
            set
            {

                if (_GetCurrentGain200CalVolValue != value)
                {
                    _GetCurrentGain200CalVolValue = value;
                    RaisePropertyChanged("GetCurrentGain200CalVolValue");
                }
            }
        }

        public double SetCurrentGain250CalVolValue
        {
            get { return _SetCurrentGain250CalVolValue; }
            set
            {

                if (_SetCurrentGain250CalVolValue != value)
                {
                    _SetCurrentGain250CalVolValue = value;
                    RaisePropertyChanged("SetCurrentGain250CalVolValue");
                }
            }
        }

        public double GetCurrentGain250CalVolValue
        {
            get { return _GetCurrentGain250CalVolValue; }
            set
            {

                if (_GetCurrentGain250CalVolValue != value)
                {
                    _GetCurrentGain250CalVolValue = value;
                    RaisePropertyChanged("GetCurrentGain250CalVolValue");
                }
            }
        }

        public double SetCurrentGain300CalVolValue
        {
            get { return _SetCurrentGain300CalVolValue; }
            set
            {

                if (_SetCurrentGain300CalVolValue != value)
                {
                    _SetCurrentGain300CalVolValue = value;
                    RaisePropertyChanged("SetCurrentGain300CalVolValue");
                }
            }
        }

        public double GetCurrentGain300CalVolValue
        {
            get { return _GetCurrentGain300CalVolValue; }
            set
            {

                if (_GetCurrentGain300CalVolValue != value)
                {
                    _GetCurrentGain300CalVolValue = value;
                    RaisePropertyChanged("GetCurrentGain300CalVolValue");
                }
            }
        }

        public double SetCurrentGain400CalVolValue
        {
            get { return _SetCurrentGain400CalVolValue; }
            set
            {

                if (_SetCurrentGain400CalVolValue != value)
                {
                    _SetCurrentGain400CalVolValue = value;
                    RaisePropertyChanged("SetCurrentGain400CalVolValue");
                }
            }
        }

        public double GetCurrentGain400CalVolValue
        {
            get { return _GetCurrentGain400CalVolValue; }
            set
            {

                if (_GetCurrentGain400CalVolValue != value)
                {
                    _GetCurrentGain400CalVolValue = value;
                    RaisePropertyChanged("GetCurrentGain400CalVolValue");
                }
            }
        }

        public double SetCurrentGain500CalVolValue
        {
            get { return _SetCurrentGain500CalVolValue; }
            set
            {

                if (_SetCurrentGain500CalVolValue != value)
                {
                    _SetCurrentGain500CalVolValue = value;
                    RaisePropertyChanged("SetCurrentGain500CalVolValue");
                }
            }
        }

        public double GetCurrentGain500CalVolValue
        {
            get { return _GetCurrentGain500CalVolValue; }
            set
            {

                if (_GetCurrentGain500CalVolValue != value)
                {
                    _GetCurrentGain500CalVolValue = value;
                    RaisePropertyChanged("GetCurrentGain500CalVolValue");
                }
            }
        }

        public double SetCurrentPMTCtrlVolValue
        {
            get { return _SetCurrentPMTCtrlVolValue; }
            set
            {

                if (_SetCurrentPMTCtrlVolValue != value)
                {
                    _SetCurrentPMTCtrlVolValue = value;
                    RaisePropertyChanged("SetCurrentPMTCtrlVolValue");
                }
            }
        }
        public double GetCurrentPMTCtrlVolValue
        {
            get { return _GetCurrentPMTCtrlVolValue; }
            set
            {

                if (_GetCurrentPMTCtrlVolValue != value)
                {
                    _GetCurrentPMTCtrlVolValue = value;
                    RaisePropertyChanged("GetCurrentPMTCtrlVolValue");
                }
            }
        }
        

        public string SetCurrentPMTCompensationCoefficientValue
        {
            get { return _SetCurrentPMTCompensationCoefficientValue; }
            set
            {
                if (GetIVvNumberValue == "10000000")
                {

                    if (value != null)
                    {
                        if (value.Contains("-"))
                        {
                            _SetCurrentPMTCompensationCoefficientValue = "0";
                            RaisePropertyChanged("SetCurrentPMTCompensationCoefficientValue");
                            return;
                        }

                    }
                }
                if (_SetCurrentPMTCompensationCoefficientValue != value)
                {
                    _SetCurrentPMTCompensationCoefficientValue = value;
                    RaisePropertyChanged("SetCurrentPMTCompensationCoefficientValue");
                }
            }
        }

        public string SetIVOpticalModuleNumberValue
        {
            get { return _SetIVOpticalModuleNumberValue; }
            set
            {

                if (_SetIVOpticalModuleNumberValue != value)
                {
                    _SetIVOpticalModuleNumberValue = value;
                    RaisePropertyChanged("SetIVOpticalModuleNumberValue");
                }
            }
        }

        public double GetCurrentPMTCompensationCoefficientValue
        {
            get { return _GetCurrentPMTCompensationCoefficientValue; }
            set
            {

                if (_GetCurrentPMTCompensationCoefficientValue != value)
                {
                    _GetCurrentPMTCompensationCoefficientValue = value;
                    RaisePropertyChanged("GetCurrentPMTCompensationCoefficientValue");
                }
            }
        }

        public double GetCurrentTempSenser1282ADValue
        {
            get { return _GetCurrentTempSenser1282ADValue; }
            set
            {

                if (_GetCurrentTempSenser1282ADValue != value)
                {
                    _GetCurrentTempSenser1282ADValue = value;
                    RaisePropertyChanged("GetCurrentTempSenser1282ADValue");
                }
            }
        }

        public double GetCurrentTempSenser6459ADValue
        {
            get { return _GetCurrentTempSenser6459ADValue; }
            set
            {

                if (_GetCurrentTempSenser6459ADValue != value)
                {
                    _GetCurrentTempSenser6459ADValue = value;
                    RaisePropertyChanged("GetCurrentTempSenser6459ADValue");
                }
            }
        }

        public double GetCurrentTempSenserADValue
        {
            get { return _GetCurrentTempSenserADValue; }
            set
            {

                if (_GetCurrentTempSenserADValue != value)
                {
                    _GetCurrentTempSenserADValue = value;
                    RaisePropertyChanged("GetCurrentTempSenserADValue");
                }
            }
        }

        public double GetCurrentIVBoardRunningStateValue
        {
            get { return _GetCurrentIVBoardRunningStateValue; }
            set
            {

                if (_GetCurrentIVBoardRunningStateValue != value)
                {
                    _GetCurrentIVBoardRunningStateValue = value;
                    RaisePropertyChanged("GetCurrentIVBoardRunningStateValue");
                }
            }
        }

        public double GetCurrentTemperatureSensorSamplingVoltageValue
        {
            get { return _GetCurrentTemperatureSensorSamplingVoltageValue; }
            set
            {

                if (_GetCurrentTemperatureSensorSamplingVoltageValue != value)
                {
                    _GetCurrentTemperatureSensorSamplingVoltageValue = value;
                    RaisePropertyChanged("GetCurrentTemperatureSensorSamplingVoltageValue");
                }
            }
        }

        public double GetCurrentADPTemperatureCalibrationFactorValue
        {
            get { return _GetCurrentADPTemperatureCalibrationFactorValue; }
            set
            {

                if (_GetCurrentADPTemperatureCalibrationFactorValue != value)
                {
                    _GetCurrentADPTemperatureCalibrationFactorValue = value;
                    RaisePropertyChanged("GetCurrentADPTemperatureCalibrationFactorValue");
                }
            }
        }

        public string GetIVvNumberValue
        {
            get { return _GetIVvNumberValue; }
            set
            {

                if (_GetIVvNumberValue != value)
                {
                    _GetIVvNumberValue = value;
                    RaisePropertyChanged("GetIVvNumberValue");
                }
            }
        }

        public string GetIVErrorCodeValue
        {
            get { return _GetIVErrorCodeValue; }
            set
            {

                if (_GetIVErrorCodeValue != value)
                {
                    _GetIVErrorCodeValue = value;
                    RaisePropertyChanged("GetIVErrorCodeValue");
                }
            }
        }

        public string GetIVOpticalModuleNumberValue
        {
            get { return _GetIVOpticalModuleNumberValue; }
            set
            {

                if (_GetIVOpticalModuleNumberValue != value)
                {
                    _GetIVOpticalModuleNumberValue = value;
                    RaisePropertyChanged("GetIVOpticalModuleNumberValue");
                }
            }
        }

        public double SetCurrentADPTemperatureCalibrationFactorValue
        {
            get { return _SetCurrentADPTemperatureCalibrationFactorValue; }
            set
            {

                if (_SetCurrentADPTemperatureCalibrationFactorValue != value)
                {
                    _SetCurrentADPTemperatureCalibrationFactorValue = value;
                    RaisePropertyChanged("SetCurrentADPTemperatureCalibrationFactorValue");
                }
            }
        }
        
        #endregion
        #endregion

        #region LightModeCalibration Command
        public ICommand LightModeCalibrationCommand
        {
            get
            {
                if (_LightModeCalibrationCommand == null)
                {
                    _LightModeCalibrationCommand = new RelayCommand(ExcuteLightModeCalibrationCommand, CanExcuteLightModeCalibrationCommand);
                }
                return _LightModeCalibrationCommand;
            }
        }
        public void ExcuteLightModeCalibrationCommand(object parameter)
        {
            try
            {
                if (_LightModeSettingsPort == null || _LightModeSettingsPort.IsConnected == false)
                {
                    _LightModeSettingsPort = new LightModeSettingsPort();
                    _LightModeSettingsPort.UpdateCommOutput += LightModeSettingsPort_UpdateCommOutput;
                }
                _LightModeSettingsPort.SearchPort();
                if (_LightModeSettingsPort.AvailablePorts.Length == 0)
                {
                    MessageBox.Show("未找到串口设备！", "连接错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                if (_LightModeSettingsPort.IsConnected == false)
                {
                    MessageBoxResult messageBoxResult = MessageBox.Show("485 错误！", "连接错误", MessageBoxButton.YesNo, MessageBoxImage.Error);
                    if (messageBoxResult == MessageBoxResult.Yes)
                    {
                        _LightModeClibrationSub = new LightModeClibrationSub();
                        _LightModeClibrationSub.Title = Workspace.This.Owner.Title + "-光学模块校准 Kit";
                        _LightModeClibrationSub.IsEnabled = false;
                        _LightModeClibrationSub.ShowDialog();
                        _LightModeClibrationSub = null;
                        return;
                    }
                    else
                    {
                        Workspace.This.Owner.Show();
                        return;
                    }
                }
                else
                {
                    _LightModeClibrationSub = new LightModeClibrationSub();
                    _LightModeClibrationSub.Title = Workspace.This.Owner.Title + "-光学模块校准 Kit";
                    _LightModeClibrationSub.ShowDialog();
                    _LightModeClibrationSub = null;
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }

        }

        private void LightModeSettingsPort_UpdateCommOutput()
        {
            GetCurrentLaserWavaValue = _LightModeSettingsPort.LaserWaveLength;
            GetCurrentLaserNumberValue = _LightModeSettingsPort.LaserNumberNo;
            GetCurrentPMTNumberValue= _LightModeSettingsPort.PMTNumberNo;
            GetCurrentTecTempValue = _LightModeSettingsPort.TECCtrlTemp;
            GetCurrentLightElectricTowVoleValue = _LightModeSettingsPort.LightElectricTowVole;
            GetLaserNot532PowserValue = _LightModeSettingsPort.Not532LaserPower;
            GetLaserTrue532PowserValue = _LightModeSettingsPort.True532LaserPower;

            GetCurrentSensorNoValue = _LightModeSettingsPort.CurrentSensorNoValue;
            GetCurrentPGAMultipleValue = _LightModeSettingsPort.CurrentPGAMultipleValue;
            GetCurrentAPDGainValue = _LightModeSettingsPort.CurrentAPDGainValue;
            GetCurrentAPDTempValue = _LightModeSettingsPort.CurrentAPDTempValue;
            GetCurrentAPDHighVoltageValue = _LightModeSettingsPort.CurrentAPDHighVoltageValue;
            GetCurrentLightIntensityCalibrationTemperatureValue = _LightModeSettingsPort.CurrentLightIntensityCalibrationTemperatureValue;
            GetCurrentGain50CalVolValue = _LightModeSettingsPort.CurrentGain50CalVolValue;
            GetCurrentGain100CalVolValue = _LightModeSettingsPort.CurrentGain100CalVolValue;
            GetCurrentGain150CalVolValue = _LightModeSettingsPort.CurrentGain150CalVolValue;
            GetCurrentGain200CalVolValue = _LightModeSettingsPort.CurrentGain200CalVolValue;
            GetCurrentGain250CalVolValue = _LightModeSettingsPort.CurrentGain250CalVolValue;
            GetCurrentGain300CalVolValue = _LightModeSettingsPort.CurrentGain300CalVolValue;
            GetCurrentGain400CalVolValue = _LightModeSettingsPort.CurrentGain400CalVolValue;
            GetCurrentGain500CalVolValue = _LightModeSettingsPort.CurrentGain500CalVolValue;
            GetCurrentPMTCtrlVolValue = _LightModeSettingsPort.CurrentPMTCtrlVolValue;
            GetCurrentPMTCompensationCoefficientValue = _LightModeSettingsPort.CurrentPMTCompensationCoefficientValue;
            GetCurrentTempSenser1282ADValue = _LightModeSettingsPort.CurrentTempSenser1282ADValue;
            GetCurrentTempSenser6459ADValue = _LightModeSettingsPort.CurrentTempSenser6459ADValue;
            GetCurrentTempSenserADValue = _LightModeSettingsPort.CurrentTempSenserADValue;
            GetCurrentIVBoardRunningStateValue = _LightModeSettingsPort.CurrentIVBoardRunningStateValue;
            GetCurrentTemperatureSensorSamplingVoltageValue = _LightModeSettingsPort.CurrentTemperatureSensorSamplingVoltageValue;
            GetCurrentADPTemperatureCalibrationFactorValue = _LightModeSettingsPort.CurrentADPTemperatureCalibrationFactorValue;

            GetLaservNumberValue = _LightModeSettingsPort.LaserSoftwareVersionNumber;
            GetIVvNumberValue = _LightModeSettingsPort.IVSoftwareVersionNumber;
            GetLaserErrorCodeValue = _LightModeSettingsPort.LaserErrorCode;
            GetIVErrorCodeValue = _LightModeSettingsPort.IVErrorCode;
            GetLaserOpticalModuleNumberValue = _LightModeSettingsPort.LaserOpticalModuleNumberValue;
            GetIVOpticalModuleNumberValue = _LightModeSettingsPort.IVOpticalModuleNumberValue;


            GetCurrentLaserNoValue = _LightModeSettingsPort.CurrentLaserNoValue;
            GetCurrentLaserWavaLengthValue= _LightModeSettingsPort.CurrentLaserWavaLengthValue;
            GetCurrentLaserCurrentValueValue = _LightModeSettingsPort.CurrentLaserCurrentValueValue;
            GetCurrentLaserCorrespondingCurrentValueValue = _LightModeSettingsPort.CurrentLaserCorrespondingCurrentValueValue;
            GetCurrentTECActualTemperatureValue = _LightModeSettingsPort.CurrentTECActualTemperatureValue;
            GetCurrentTEControlTemperatureValue = _LightModeSettingsPort.CurrentTEControlTemperatureValue;
            GetCurrentTECMaximumCoolingCurrentValue = _LightModeSettingsPort.CurrentTECMaximumCoolingCurrentValue;
            GetCurrentTECRefrigerationControlParameterKpValue = _LightModeSettingsPort.CurrentTECRefrigerationControlParameterKpValue;
            GetCurrentTECRefrigerationControlParameterKiValue = _LightModeSettingsPort.CurrentTECRefrigerationControlParameterKiValue;
            GetCurrentTECRefrigerationControlParameterKdValue = _LightModeSettingsPort.CurrentTECRefrigerationControlParameterKdValue;
            GetCurrentLaserLightPowerValueValue = _LightModeSettingsPort.CurrentLaserLightPowerValueValue;
            GetCurrentTECWorkingStatusValue = _LightModeSettingsPort.CurrentTECWorkingStatusValue;
            GetCurrentTECCurrentDirectionValue = _LightModeSettingsPort.CurrentTECCurrentDirectionValue;
            GetCurrenRadiatorTemperatureValue = _LightModeSettingsPort.CurrenRadiatorTemperatureValue;
            GetCurrenTECCurrentCompensationCoefficientValue = _LightModeSettingsPort.CurrenTECCurrentCompensationCoefficientValue;
            Get_CurrentLaserLightPowerValueValue = _LightModeSettingsPort.GetGet_CurrentLaserLightPowerValueValue;
            GetCurrentPowerClosedloopControlParameterKpValue = _LightModeSettingsPort.CurrentPowerClosedloopControlParameterKpValue;
            GetCurrentPowerClosedloopControlParameterKiValue = _LightModeSettingsPort.CurrentPowerClosedloopControlParameterKiValue;
            GetCurrentPowerClosedloopControlParameterKdValue = _LightModeSettingsPort.CurrentPowerClosedloopControlParameterKdValue;
            GetCurrentPhotodiodeVoltageCorrespondingToLaserPowerValue = _LightModeSettingsPort.CurrentPhotodiodeVoltageCorrespondingToLaserPowerValue;
            GetCurrentPhotodiodeVoltageValue = _LightModeSettingsPort.CurrentPhotodiodeVoltageValue;
            GetCurrentKValueofPhotodiodeTemperatureCurveValue = _LightModeSettingsPort.CurrentKValueofPhotodiodeTemperatureCurveValue;
            GetCurrentBValueofPhotodiodeTemperatureCurveValue = _LightModeSettingsPort.CurrentBValueofPhotodiodeTemperatureCurveValue;
        }

        public bool CanExcuteLightModeCalibrationCommand(object parameter)
        {
            return true;
        }
        #endregion

        #region 光学模块设置

        //设置

        #region SetCurrentLaserWavaCommand

        public ICommand SetCurrentLaserWavaCommand
        {
            get
            {
                if (_SetCurrentLaserWavaCommand == null)
                {
                    _SetCurrentLaserWavaCommand = new RelayCommand(ExecuteSetCurrentLaserWavaCommand, CanExecuteSetCurrentLaserWavaCommand);
                }
                return _SetCurrentLaserWavaCommand;
            }
        }
        public void ExecuteSetCurrentLaserWavaCommand(object parameter)
        {
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            _LightModeSettingsPort.SetCurrentLaserWava(SetCurrentLaserWavaValue);
            Thread.Sleep(200);
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentLaserWava();
                    Thread.Sleep(200);
                    //GetCurrentLaserWavaValue = _LightModeSettingsPort.LaserWaveLength;
                }
            }
           
        }
        public bool CanExecuteSetCurrentLaserWavaCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region SetCurrentLaserNumberCommand

        public ICommand SetCurrentLaserNumberCommand
        {
            get
            {
                if (_SetCurrentLaserNumberCommand == null)
                {
                    _SetCurrentLaserNumberCommand = new RelayCommand(ExecuteSetCurrentLaserNumberCommand, CanExecuteSetCurrentLaserNumberCommand);
                }
                return _SetCurrentLaserNumberCommand;
            }
        }
        public void ExecuteSetCurrentLaserNumberCommand(object parameter)
        {
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            _LightModeSettingsPort.SetCurrenLaserNumber(SetCurrentLaserNumberValue.ToString());
            Thread.Sleep(200);
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentLaserNumberNo();
                    Thread.Sleep(200);
                    //GetCurrentLaserNumberValue = _LightModeSettingsPort.LaserNumberNo;
                }

            }
          
        }
        public bool CanExecuteSetCurrentLaserNumberCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region SetCurrentPMTNumbeCommand

        public ICommand SetCurrentPMTNumbeCommand
        {
            get
            {
                if (_SetCurrentPMTNumbeCommand == null)
                {
                    _SetCurrentPMTNumbeCommand = new RelayCommand(ExecuteSetCurrentPMTNumbeCommand, CanExecuteSetCurrentPMTNumbeCommand);
                }
                return _SetCurrentPMTNumbeCommand;
            }
        }
        public void ExecuteSetCurrentPMTNumbeCommand(object parameter)
        {
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            _LightModeSettingsPort.SetCurrenPMTNumber(SetCurrentPMTNumberValue.ToString());
            Thread.Sleep(200);
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentPMTNumberNo();
                    Thread.Sleep(200);
                    //GetCurrentPMTNumberValue = _LightModeSettingsPort.PMTNumberNo;
                    //Thread.Sleep(200);
                    //_LightModeSettingsPort.GetCurrentPMTNumberNo();
                }
            }
           
        }
        public bool CanExecuteSetCurrentPMTNumbeCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region SetCurrentTECctrlTempCommand

        public ICommand SetCurrentTECctrlTempCommand
        {
            get
            {
                if (_SetCurrentTECctrlTempCommand == null)
                {
                    _SetCurrentTECctrlTempCommand = new RelayCommand(ExecuteSetCurrentTECctrlTempCommand, CanExecuteSetCurrentTECctrlTempCommand);
                }
                return _SetCurrentTECctrlTempCommand;
            }
        }
        public void ExecuteSetCurrentTECctrlTempCommand(object parameter)
        {
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            _LightModeSettingsPort.SetCurrenTECctrlTemp(SetCurrentTecTempValue);
            Thread.Sleep(200);
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentTECCtrlTemp();
                    Thread.Sleep(200);
                    // GetCurrentTecTempValue = _LightModeSettingsPort.TECCtrlTemp;
                }
            }
           
        }
        public bool CanExecuteSetCurrentTECctrlTempCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region SetCurrentLaserPowerValueCommand

        public ICommand SetCurrentLaserPowerValueCommand
        {
            get
            {
                if (_SetCurrentLaserPowerValueCommand == null)
                {
                    _SetCurrentLaserPowerValueCommand = new RelayCommand(ExecuteSetCurrentLaserPowerValueCommand, CanExecuteSetCurrentLaserPowerValueCommand);
                }
                return _SetCurrentLaserPowerValueCommand;
            }
        }
        public void ExecuteSetCurrentLaserPowerValueCommand(object parameter)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.SetCurrenLaserPowerValue(SetCurrentLaserPowerValueValue);
                    Thread.Sleep(200);
                }

            }
           
        }
        public bool CanExecuteSetCurrentLaserPowerValueCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region SetLaserNot532PowserCommand

        public ICommand SetLaserNot532PowserCommand
        {
            get
            {
                if (_SetLaserNot532PowserCommand == null)
                {
                    _SetLaserNot532PowserCommand = new RelayCommand(ExecuteSetLaserNot532PowserCommand, CanExecuteSetLaserNot532PowserCommand);
                }
                return _SetLaserNot532PowserCommand;
            }
        }
        public void ExecuteSetLaserNot532PowserCommand(object parameter)
        {
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            _LightModeSettingsPort.SetCurrenLaserNot532Powser(SelectedLaserPowerL1Module.Value, SetLaserNot532PowserValue);
            Thread.Sleep(200);
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentNot532LaserPower(SelectedLaserPowerL1Module.Value);
                    Thread.Sleep(200);
                    //GetLaserNot532PowserValue = _LightModeSettingsPort.Not532LaserPower;
                }
            }
           
        }
        public bool CanExecuteSetLaserNot532PowserCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region SetLaserTrue532PowserCommand

        public ICommand SetLaserTrue532PowserCommand
        {
            get
            {
                if (_SetLaserTrue532PowserCommand == null)
                {
                    _SetLaserTrue532PowserCommand = new RelayCommand(ExecuteSetLaserTrue532PowserCommand, CanExecuteSetLaserTrue532PowserCommand);
                }
                return _SetLaserTrue532PowserCommand;
            }
        }
        public void ExecuteSetLaserTrue532PowserCommand(object parameter)
        {
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            _LightModeSettingsPort.SetCurrenTrue532LaserPower(SelectedLaserPowerL1Module.Value, SetLaserTrue532PowserValue);
            Thread.Sleep(200);
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentTrue532LaserPower(SelectedLaserPowerL1Module.Value);
                    Thread.Sleep(200);
                    //GetLaserTrue532PowserValue = _LightModeSettingsPort.True532LaserPower;
                }
            }
           
        }
        public bool CanExecuteSetLaserTrue532PowserCommand(object parameter)
        {
            return true;
        }

        #endregion

        //读取

        #region GetCurrentLaserWavaCommand

        public ICommand GetCurrentLaserWavaCommand
        {
            get
            {
                if (_GetCurrentLaserWavaCommand == null)
                {
                    _GetCurrentLaserWavaCommand = new RelayCommand(ExecuteGetCurrentLaserWavaCommand, CanExecuteGetCurrentLaserWavaCommand);
                }
                return _GetCurrentLaserWavaCommand;
            }
        }
        public void ExecuteGetCurrentLaserWavaCommand(object parameter)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    Thread.Sleep(200);
                    _LightModeSettingsPort.GetCurrentLaserWava();
                    Thread.Sleep(200);
                    GetCurrentLaserWavaValue = _LightModeSettingsPort.LaserWaveLength;
                    Thread.Sleep(200);
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentLaserWava();
                    Thread.Sleep(200);
                    //GetCurrentLaserWavaValue = _LightModeSettingsPort.LaserWaveLength;
                }
            }
           
        }
        public bool CanExecuteGetCurrentLaserWavaCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region GetCurrentLaserNumberCommand

        public ICommand GetCurrentLaserNumberCommand
        {
            get
            {
                if (_GetCurrentLaserNumberCommand == null)
                {
                    _GetCurrentLaserNumberCommand = new RelayCommand(ExecuteGetCurrentLaserNumberCommand, CanExecuteGetCurrentLaserNumberCommand);
                }
                return _GetCurrentLaserNumberCommand;
            }
        }
        public void ExecuteGetCurrentLaserNumberCommand(object parameter)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentLaserNumberNo();
                    Thread.Sleep(200);
                    //GetCurrentLaserNumberValue = _LightModeSettingsPort.LaserNumberNo;
                }
            }
          
        }
        public bool CanExecuteGetCurrentLaserNumberCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region GetCurrentPMTNumberCommand

        public ICommand GetCurrentPMTNumberCommand
        {
            get
            {
                if (_GetCurrentPMTNumberCommand == null)
                {
                    _GetCurrentPMTNumberCommand = new RelayCommand(ExecuteGetCurrentPMTNumberCommand, CanExecuteGetCurrentPMTNumberCommand);
                }
                return _GetCurrentPMTNumberCommand;
            }
        }
        public void ExecuteGetCurrentPMTNumberCommand(object parameter)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentPMTNumberNo();
                    Thread.Sleep(200);
                    // GetCurrentPMTNumberValue= _LightModeSettingsPort.PMTNumberNo;
                }
            }
           
        }
        public bool CanExecuteGetCurrentPMTNumberCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region GetCurrentTECctrlTempCommand

        public ICommand GetCurrentTECctrlTempCommand
        {
            get
            {
                if (_GetCurrentTECctrlTempCommand == null)
                {
                    _GetCurrentTECctrlTempCommand = new RelayCommand(ExecuteGetCurrentTECctrlTempCommand, CanExecuteGetCurrentTECctrlTempCommand);
                }
                return _GetCurrentTECctrlTempCommand;
            }
        }
        public void ExecuteGetCurrentTECctrlTempCommand(object parameter)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentTECCtrlTemp();
                    Thread.Sleep(200);
                    //GetCurrentTecTempValue = _LightModeSettingsPort.TECCtrlTemp;
                }
            }
           
        }
        public bool CanExecuteGetCurrentTECctrlTempCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region GetLaserNot532PowserCommand

        public ICommand GetLaserNot532PowserCommand
        {
            get
            {
                if (_GetLaserNot532PowserCommand == null)
                {
                    _GetLaserNot532PowserCommand = new RelayCommand(ExecuteGetLaserNot532PowserCommand, CanExecuteGetLaserNot532PowserCommand);
                }
                return _GetLaserNot532PowserCommand;
            }
        }
        public void ExecuteGetLaserNot532PowserCommand(object parameter)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    int PowerMv = SelectedLaserPowerL1Module.Value;
                    _LightModeSettingsPort.GetCurrentNot532LaserPower(PowerMv);
                    Thread.Sleep(200);
                    //GetLaserNot532PowserValue = _LightModeSettingsPort.Not532LaserPower;
                }

            }
          
        }
        public bool CanExecuteGetLaserNot532PowserCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region GetLaserTrue532PowserCommand

        public ICommand GetLaserTrue532PowserCommand
        {
            get
            {
                if (_GetLaserTrue532PowserCommand == null)
                {
                    _GetLaserTrue532PowserCommand = new RelayCommand(ExecuteGetLaserTrue532PowserCommand, CanExecuteGetLaserTrue532PowserCommand);
                }
                return _GetLaserTrue532PowserCommand;
            }
        }
        public void ExecuteGetLaserTrue532PowserCommand(object parameter)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    int PowerMv = SelectedLaserPowerL1Module.Value;
                    _LightModeSettingsPort.GetCurrentTrue532LaserPower(PowerMv);
                    Thread.Sleep(500);
                    //GetLaserTrue532PowserValue = _LightModeSettingsPort.True532LaserPower;
                }
            }
          
        }
        public bool CanExecuteGetLaserTrue532PowserCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region GetCurrentLaserPowerValueCommand

        public ICommand GetCurrentLaserPowerValueCommand
        {
            get
            {
                if (_GetCurrentLaserPowerValueCommand == null)
                {
                    _GetCurrentLaserPowerValueCommand = new RelayCommand(ExecuteGetLaserPowerValueCommand, CanExecuteGetLaserPowerValueCommand);
                }
                return _GetCurrentLaserPowerValueCommand;
            }
        }
        public void ExecuteGetLaserPowerValueCommand(object parameter)
        {
           
        }
        public bool CanExecuteGetLaserPowerValueCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region GetCurrentLightElectricTowVoleCommand

        public ICommand GetCurrentLightElectricTowVoleCommand
        {
            get
            {
                if (_GetCurrentLightElectricTowVoleCommand == null)
                {
                    _GetCurrentLightElectricTowVoleCommand = new RelayCommand(ExecuteGetLightElectricTowVoleCommand, CanExecuteGetLightElectricTowVoleCommand);
                }
                return _GetCurrentLightElectricTowVoleCommand;
            }
        }
        public void ExecuteGetLightElectricTowVoleCommand(object parameter)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_LightModeSettingsPort != null)
                {
                    while (_LightModeSettingsPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _LightModeSettingsPort.GetCurrentLightElectricTowVole();
                    Thread.Sleep(200);
                    //GetCurrentLightElectricTowVoleValue = _LightModeSettingsPort.LightElectricTowVole;
                }

            }
          
        }
        public bool CanExecuteGetLightElectricTowVoleCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region  pro
        public LightModeSettingsPort LightModeSettingsPort
        {
            get
            {
                return _LightModeSettingsPort;
            }
            set
            {
                if (_LightModeSettingsPort != value)
                {
                    _LightModeSettingsPort = value;
                }
            }
        }
        
        public double SetCurrentLaserPowerValueValue
        {
            get { return _SetCurrentLaserPowerValueValue; }
            set
            {

                if (_SetCurrentLaserPowerValueValue != value)
                {
                    _SetCurrentLaserPowerValueValue = value;
                    RaisePropertyChanged("SetCurrentLaserPowerValueValue");
                }
            }
        }

        public double SetCurrentLaserWavaValue
        {
            get { return _SetCurrentLaserWavaValue; }
            set
            {

                if (_SetCurrentLaserWavaValue != value)
                {
                    _SetCurrentLaserWavaValue = value;
                    RaisePropertyChanged("SetCurrentLaserWavaValue");
                }
            }
        }
        public string SetCurrentLaserNumberValue
        {
            get { return _SetCurrentLaserNumberValue; }
            set
            {

                if (_SetCurrentLaserNumberValue != value)
                {
                    _SetCurrentLaserNumberValue = value;
                    RaisePropertyChanged("SetCurrentLaserNumberValue");
                }
            }
        }
        public string SetCurrentPMTNumberValue
        {
            get { return _SetCurrentPMTNumberValue; }
            set
            {

                if (_SetCurrentPMTNumberValue != value)
                {
                    _SetCurrentPMTNumberValue = value;
                    RaisePropertyChanged("SetCurrentPMTNumberValue");
                }
            }
        }
        public double SetCurrentTecTempValue
        {
            get { return _SetCurrentTecTempValue; }
            set
            {

                if (_SetCurrentTecTempValue != value)
                {
                    _SetCurrentTecTempValue = value;
                    RaisePropertyChanged("SetCurrentTecTempValue");
                }
            }
        }
        public double SetLaserNot532PowserValue
        {
            get { return _SetLaserNot532PowserValue; }
            set
            {

                if (_SetLaserNot532PowserValue != value)
                {
                    _SetLaserNot532PowserValue = value;
                    RaisePropertyChanged("SetLaserNot532PowserValue");
                }
            }
        }
        public double SetLaserTrue532PowserValue
        {
            get { return _SetLaserTrue532PowserValue; }
            set
            {

                if (_SetLaserTrue532PowserValue != value)
                {
                    _SetLaserTrue532PowserValue = value;
                    RaisePropertyChanged("SetLaserTrue532PowserValue");
                }
            }
        }

        public string GetCurrentLaserWavaValue
        {
            get { return _GetCurrentLaserWavaValue; }
            set
            {

                if (_GetCurrentLaserWavaValue != value)
                {
                    _GetCurrentLaserWavaValue = value;
                    RaisePropertyChanged("GetCurrentLaserWavaValue");
                }
            }
        }
        public string GetCurrentLaserNumberValue
        {
            get { return _GetCurrentLaserNumberValue; }
            set
            {

                if (_GetCurrentLaserNumberValue != value)
                {
                    _GetCurrentLaserNumberValue = value;
                    RaisePropertyChanged("GetCurrentLaserNumberValue");
                }
            }
        }

        public string GetCurrentPMTNumberValue
        {
            get { return _GetCurrentPMTNumberValue; }
            set
            {

                if (_GetCurrentPMTNumberValue != value)
                {
                    _GetCurrentPMTNumberValue = value;
                    RaisePropertyChanged("GetCurrentPMTNumberValue");
                }
            }
        }
        public double GetCurrentTecTempValue
        {
            get { return _GetCurrentTecTempValue; }
            set
            {

                if (_GetCurrentTecTempValue != value)
                {
                    _GetCurrentTecTempValue = value;
                    RaisePropertyChanged("GetCurrentTecTempValue");
                }
            }
        }

        public double GetCurrentLaserPowerValueValue
        {
            get { return _GetCurrentLaserPowerValueValue; }
            set
            {

                if (_GetCurrentLaserPowerValueValue != value)
                {
                    _GetCurrentLaserPowerValueValue = value;
                    RaisePropertyChanged("GetCurrentLaserPowerValueValue");
                }
            }
        }

        public double GetCurrentLightElectricTowVoleValue
        {
            get { return _GetCurrentLightElectricTowVoleValue; }
            set
            {

                if (_GetCurrentLightElectricTowVoleValue != value)
                {
                    _GetCurrentLightElectricTowVoleValue = value;
                    RaisePropertyChanged("GetCurrentLightElectricTowVoleValue");
                }
            }
        }
        public double GetLaserNot532PowserValue
        {
            get { return _GetLaserNot532PowserValue; }
            set
            {

                if (_GetLaserNot532PowserValue != value)
                {
                    _GetLaserNot532PowserValue = value;
                    RaisePropertyChanged("GetLaserNot532PowserValue");
                }
            }
        }

        public double GetLaserTrue532PowserValue
        {
            get { return _GetLaserTrue532PowserValue; }
            set
            {

                if (_GetLaserTrue532PowserValue != value)
                {
                    _GetLaserTrue532PowserValue = value;
                    RaisePropertyChanged("GetLaserTrue532PowserValue");
                }
            }
        }
        #endregion

        #endregion

        #region 打印报告
        #region GenerateReportCommand

        public ICommand GenerateReportCommand
        {
            get
            {
                if (_GenerateReportCommand == null)
                {
                    _GenerateReportCommand = new RelayCommand(ExecuteGenerateReportCommand, CanExecuteGenerateReportCommand);
                }
                return _GenerateReportCommand;
            }
        }
        private string StrDate()
        {
            System.DateTime currentTime = DateTime.Now;//获取当前系统时间
            int month = currentTime.Month;
            int day = currentTime.Day;
            int hour = currentTime.Hour;
            int minute = currentTime.Minute;
            string newDay = "";
            string newMonth = "";
            if (day.ToString().Length == 1)
            {
                newDay = "0" + day.ToString();
            }
            else
            {
                newDay = day.ToString();
            }
            if (month.ToString().Length == 1)
            {
                newMonth = "0" + month.ToString();
            }
            else
            {
                newMonth = month.ToString();
            }
            string timeNow = month.ToString() + newDay + hour.ToString() + minute.ToString();
            return timeNow;
        }

        public void ExecuteGenerateReportCommand(object parameter)
        {

            //给出文件保存信息，确定保存位置
            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            saveFileDialog.Filter = "PDF文件（*.PDF）|*.PDF";
            //判断是否点击确认按钮
            saveFileDialog.FileName = "光学模块报告_" + ".pdf";
            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string spach = "            ";
                string filePath = saveFileDialog.FileName;//获取PDF文件名称
                if (!GetInfo())
                {
                    return;
                }
                //开始创建PDF文档
                Document document = new Document();//创建实例化对象Document
                //生成pdf路径，创建文件流
                PdfWriter.GetInstance(document, new FileStream(filePath, FileMode.Create));
                document.Open();//打开当前Document
                BaseFont baseFont = BaseFont.CreateFont(@"c:\windows\fonts\SIMSUN.TTC,1", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);//设置字体 
                iTextSharp.text.Font font = new iTextSharp.text.Font(baseFont, 20);






                string Line = "IV";
                string LaserLine = "Laser";
                string IVSoftwareVersionNumber = string.Format("软件版本号: {0}", _LightModeSettingsPort.IVSoftwareVersionNumber);
                string IVSensorType = "";
                if (_LightModeSettingsPort.IVSensorType == "00000000")
                    IVSensorType = string.Format("传感器类型: {0}", "APD");
                else
                    IVSensorType = string.Format("传感器类型: {0}", "PMT");
                string CurrentSensorNoValue = string.Format("编号: {0}", _LightModeSettingsPort.CurrentSensorNoValue);
                //string CurrentPGAMultipleValue = string.Format("PGA倍数: {0}", _LightModeSettingsPort.CurrentPGAMultipleValue);
                //string CurrentAPDGainValue = string.Format("APD增益: {0}", _LightModeSettingsPort.CurrentAPDGainValue);
                //string CurrentAPDTempValue = string.Format("APD温度: {0}", _LightModeSettingsPort.CurrentAPDTempValue);
                //string CurrentAPDHighVoltageValue = string.Format("APD高压: {0}", _LightModeSettingsPort.CurrentAPDHighVoltageValue);
                //string CurrentLightIntensityCalibrationTemperatureValue = string.Format("光强校准时温度（0.01℃）: {0}", _LightModeSettingsPort.CurrentLightIntensityCalibrationTemperatureValue);
                string CurrentLightIntensityCalibrationTemperatureValue = string.Format("标定温度: {0}", _LightModeSettingsPort.CurrentLightIntensityCalibrationTemperatureValue);
                //string CurrentGain50CalVolValue = string.Format("APD增益50时校准电压值（0.01V）: {0}", _LightModeSettingsPort.CurrentGain50CalVolValue);
                //string CurrentGain100CalVolValue = string.Format("APD增益100时校准电压值（0.01V）: {0}", _LightModeSettingsPort.CurrentGain100CalVolValue);
                //string CurrentGain150CalVolValue = string.Format("APD增益150时校准电压值（0.01V）: {0}", _LightModeSettingsPort.CurrentGain150CalVolValue);
                //string CurrentGain200CalVolValue = string.Format("APD增益200时校准电压值（0.01V）: {0}", _LightModeSettingsPort.CurrentGain200CalVolValue);
                //string CurrentGain250CalVolValue = string.Format("APD增益250时校准电压值（0.01V）: {0}", _LightModeSettingsPort.CurrentGain250CalVolValue);
                //string CurrentGain300CalVolValue = string.Format("APD增益300时校准电压值（0.01V）: {0}", _LightModeSettingsPort.CurrentGain300CalVolValue);
                //string CurrentGain400CalVolValue = string.Format("APD增益400时校准电压值（0.01V）: {0}", _LightModeSettingsPort.CurrentGain400CalVolValue);
                //string CurrentGain500CalVolValue = string.Format("APD增益500时校准电压值（0.01V）: {0}", _LightModeSettingsPort.CurrentGain500CalVolValue);
                string CurrentPMTCtrlVolValue = string.Format("控制电压: {0}", _LightModeSettingsPort.CurrentPMTCtrlVolValue);
                string CurrentPMTCompensationCoefficientValue = string.Format("补偿系数: {0}", _LightModeSettingsPort.CurrentPMTCompensationCoefficientValue);
                //string CurrentPMTCtrlVolValue = string.Format("PMT控制电压（0.1mV）: {0}", _LightModeSettingsPort.CurrentPMTCtrlVolValue);
                //string CurrentPMTCompensationCoefficientValue = string.Format("PMT补偿系数（0.1mV）: {0}", _LightModeSettingsPort.CurrentPMTCompensationCoefficientValue);
                //string CurrentTempSenser1282ADValue = string.Format("温度传感器12.82℃（1.05k)时的AD值: {0}", _LightModeSettingsPort.CurrentTempSenser1282ADValue);
                //string CurrentTempSenser6459ADValue = string.Format("温度传感器64.59℃（1.25k)时的AD值: {0}", _LightModeSettingsPort.CurrentTempSenser6459ADValue);
                string TempSenserADValue = string.Format("温度标定系数(105k/1.25k) {0}/{1}", _LightModeSettingsPort.CurrentTempSenser1282ADValue, _LightModeSettingsPort.CurrentTempSenser6459ADValue);
                //string CurrentTempSenserADValue = string.Format("温度传感器的AD值: {0}", _LightModeSettingsPort.CurrentTempSenserADValue);
                //string CurrentIVBoardRunningStateValue = string.Format("IV板运行状态: {0}", _LightModeSettingsPort.CurrentIVBoardRunningStateValue);
                //string CurrentTemperatureSensorSamplingVoltageValue = string.Format("温度传感器采样电压（0.01V）: {0}", _LightModeSettingsPort.CurrentTemperatureSensorSamplingVoltageValue);
                string CurrentTemperatureSensorSamplingVoltageValue = string.Format("电压: {0}", _LightModeSettingsPort.CurrentTemperatureSensorSamplingVoltageValue);
                //string CurrentADPTemperatureCalibrationFactorValue = string.Format("APD温度校准系数（0.01℃）: {0}", _LightModeSettingsPort.CurrentADPTemperatureCalibrationFactorValue);
                string CurrentADPTemperatureCalibrationFactorValue = string.Format("温度系数: {0}", _LightModeSettingsPort.CurrentADPTemperatureCalibrationFactorValue);
                string IVErrorCode = string.Format("错误代码: {0}", _LightModeSettingsPort.IVErrorCode);
                //string IVErrorCode = string.Format("错误代码: {0}", _LightModeSettingsPort.IVErrorCode);

                PdfPTable table = new PdfPTable(2);
                table.WidthPercentage = 100;
                string _IVOpticalModuleNumberValue = string.Format("光学模块序列号: {0}", _LightModeSettingsPort.IVOpticalModuleNumberValue);
                PdfPCell cellIVOpticalModuleNumberValue = new PdfPCell(new Phrase(_IVOpticalModuleNumberValue, font));
                cellIVOpticalModuleNumberValue.DisableBorderSide(15);
                table.AddCell(cellIVOpticalModuleNumberValue);

                string _IVSoftwareVersionNumber = string.Format("软件版本号: {0}", _LightModeSettingsPort.IVSoftwareVersionNumber);
                PdfPCell cellIVSoftwareVersionNumber = new PdfPCell(new Phrase(_IVSoftwareVersionNumber, font));
                cellIVSoftwareVersionNumber.DisableBorderSide(15);
                table.AddCell(cellIVSoftwareVersionNumber);

                PdfPCell cellIVSensorType = new PdfPCell(new Phrase(IVSensorType, font));
                cellIVSensorType.DisableBorderSide(15);
                table.AddCell(cellIVSensorType);

                PdfPCell cellCurrentSensorNoValue = new PdfPCell(new Phrase(CurrentSensorNoValue, font));
                cellCurrentSensorNoValue.DisableBorderSide(15);
                table.AddCell(cellCurrentSensorNoValue);
                if (_LightModeSettingsPort.IVSensorType == "00000000")
                {
                    PdfPCell cellCurrentLightIntensityCalibrationTemperatureValue = new PdfPCell(new Phrase(CurrentLightIntensityCalibrationTemperatureValue, font));
                    cellCurrentLightIntensityCalibrationTemperatureValue.DisableBorderSide(15);
                    table.AddCell(cellCurrentLightIntensityCalibrationTemperatureValue);

                    PdfPCell cellCurrentADPTemperatureCalibrationFactorValue = new PdfPCell(new Phrase(CurrentADPTemperatureCalibrationFactorValue, font));
                    cellCurrentADPTemperatureCalibrationFactorValue.DisableBorderSide(15);
                    table.AddCell(cellCurrentADPTemperatureCalibrationFactorValue);

                    string gain = string.Format("GAIN= 50/{0}  100/{1}  150/{2}  200/{3}  250/{4}  300/{5}  400/{6}  500/{7}", _LightModeSettingsPort.CurrentGain50CalVolValue, _LightModeSettingsPort.CurrentGain100CalVolValue, _LightModeSettingsPort.CurrentGain150CalVolValue, _LightModeSettingsPort.CurrentGain200CalVolValue, _LightModeSettingsPort.CurrentGain250CalVolValue, _LightModeSettingsPort.CurrentGain300CalVolValue, _LightModeSettingsPort.CurrentGain400CalVolValue, _LightModeSettingsPort.CurrentGain500CalVolValue);
                    PdfPCell pic = new PdfPCell(new Phrase(gain, font));
                    pic.DisableBorderSide(15);
                    pic.Colspan = 2;
                    table.AddCell(pic);

                    PdfPCell cellCurrentTemperatureSensorSamplingVoltageValue = new PdfPCell(new Phrase(CurrentTemperatureSensorSamplingVoltageValue, font));
                    cellCurrentTemperatureSensorSamplingVoltageValue.DisableBorderSide(15);
                    cellCurrentTemperatureSensorSamplingVoltageValue.Colspan = 2;
                    table.AddCell(cellCurrentTemperatureSensorSamplingVoltageValue);


                    PdfPCell cellTempSenserADValue = new PdfPCell(new Phrase(TempSenserADValue, font));
                    cellTempSenserADValue.DisableBorderSide(15);
                    cellTempSenserADValue.Colspan = 2;
                    table.AddCell(cellTempSenserADValue);

                    PdfPCell cellIVErrorCode = new PdfPCell(new Phrase(IVErrorCode, font));
                    cellIVErrorCode.DisableBorderSide(15);
                    cellIVErrorCode.Colspan = 2;
                    table.AddCell(cellIVErrorCode);

                    string PGAGAINHighVoltageTemp = string.Format("PGA={0}   /GAIN={1}   /高压={2}   /APD温度{3}", _LightModeSettingsPort.CurrentPGAMultipleValue, _LightModeSettingsPort.CurrentAPDGainValue, _LightModeSettingsPort.CurrentAPDHighVoltageValue, _LightModeSettingsPort.CurrentAPDTempValue);
                    PdfPCell pic1 = new PdfPCell(new Phrase(PGAGAINHighVoltageTemp, font));
                    pic1.DisableBorderSide(15);
                    pic1.Colspan = 2;
                    table.AddCell(pic1);
                }
                else
                {
                    PdfPCell cellCurrentPMTCtrlVolValue = new PdfPCell(new Phrase(CurrentPMTCtrlVolValue, font));
                    cellCurrentPMTCtrlVolValue.DisableBorderSide(15);
                    table.AddCell(cellCurrentPMTCtrlVolValue);

                    PdfPCell cellCurrentPMTCompensationCoefficientValue = new PdfPCell(new Phrase(CurrentPMTCompensationCoefficientValue, font));
                    cellCurrentPMTCompensationCoefficientValue.DisableBorderSide(15);
                    table.AddCell(cellCurrentPMTCompensationCoefficientValue);


                    PdfPCell cellIVErrorCode = new PdfPCell(new Phrase(IVErrorCode, font));
                    cellIVErrorCode.DisableBorderSide(15);
                    cellIVErrorCode.Colspan = 2;
                    table.AddCell(cellIVErrorCode);
                }
                PdfPCell cellLaserLine = new PdfPCell(new Phrase(LaserLine, font));
                cellLaserLine.DisableBorderSide(15);
                cellLaserLine.Colspan = 2;
                table.AddCell(cellLaserLine);

                string LaserOpticalModuleNumberValue = string.Format("光学模块序列号: {0}", _LightModeSettingsPort.LaserOpticalModuleNumberValue);
                PdfPCell cellLaserOpticalModuleNumberValue = new PdfPCell(new Phrase(LaserOpticalModuleNumberValue, font));
                cellLaserOpticalModuleNumberValue.DisableBorderSide(15);
                table.AddCell(cellLaserOpticalModuleNumberValue);

                string LaserSoftwareVersionNumber = string.Format("软件版本号: {0}", _LightModeSettingsPort.LaserSoftwareVersionNumber);
                PdfPCell cellLaserSoftwareVersionNumber = new PdfPCell(new Phrase(LaserSoftwareVersionNumber, font));
                cellLaserSoftwareVersionNumber.DisableBorderSide(15);
                table.AddCell(cellLaserSoftwareVersionNumber);

                string CurrentLaserWavaLengthValue = string.Format("激光器波长: {0}", _LightModeSettingsPort.CurrentLaserWavaLengthValue);
                PdfPCell cellCurrentLaserWavaLengthValue = new PdfPCell(new Phrase(CurrentLaserWavaLengthValue, font));
                cellCurrentLaserWavaLengthValue.DisableBorderSide(15);
                table.AddCell(cellCurrentLaserWavaLengthValue);

                string CurrentLaserNoValue = string.Format("激光器编号: {0}", _LightModeSettingsPort.CurrentLaserNoValue);
                PdfPCell cellCurrentLaserNoValue = new PdfPCell(new Phrase(CurrentLaserNoValue, font));
                cellCurrentLaserNoValue.DisableBorderSide(15);
                table.AddCell(cellCurrentLaserNoValue);


                string CurrentTEControlTemperatureValue = string.Format("TEC控制温度(℃)/最大电流(mA):     {0}/{1}", _LightModeSettingsPort.CurrentTEControlTemperatureValue, _LightModeSettingsPort.CurrentTECMaximumCoolingCurrentValue);
                PdfPCell cellCurrentTEControlTemperatureValue = new PdfPCell(new Phrase(CurrentTEControlTemperatureValue, font));
                cellCurrentTEControlTemperatureValue.DisableBorderSide(15);
                cellCurrentTEControlTemperatureValue.Colspan = 2;
                table.AddCell(cellCurrentTEControlTemperatureValue);



                string TECPID = string.Format("TEC PID(Kp/Ki/Kd): {0}/  {1}/  {2}", _LightModeSettingsPort.CurrentTECRefrigerationControlParameterKpValue, _LightModeSettingsPort.CurrentTECRefrigerationControlParameterKiValue, _LightModeSettingsPort.CurrentTECRefrigerationControlParameterKdValue);
                PdfPCell cellTECPID = new PdfPCell(new Phrase(TECPID, font));
                cellTECPID.DisableBorderSide(15);
                cellTECPID.Colspan = 2;
                table.AddCell(cellTECPID);
                if (_LightModeSettingsPort.CurrentLaserWavaLengthValue == 532)
                {
                    string PID532KpLessThanLine = "光功率≤15mw ";
                    PdfPCell cellPID532KpLessThanLine = new PdfPCell(new Phrase(PID532KpLessThanLine, font));
                    cellPID532KpLessThanLine.DisableBorderSide(15);
                    cellPID532KpLessThanLine.Colspan = 2;
                    table.AddCell(cellPID532KpLessThanLine);

                    string strPIDLessThan532 = string.Format("PID (Kp/Ki/Kd): {0}/  {1}/  {2}", _LightModeSettingsPort.PowerClosedloopControlParameterKpLessThanOrEqual15, _LightModeSettingsPort.PowerClosedloopControlParameterKiLessThanOrEqual15, _LightModeSettingsPort.PowerClosedloopControlParameterKdLessThanOrEqual15);
                    PdfPCell cellPIDstrPIDLessThan532 = new PdfPCell(new Phrase(strPIDLessThan532, font));
                    cellPIDstrPIDLessThan532.DisableBorderSide(15);
                    cellPIDstrPIDLessThan532.Colspan = 2;
                    table.AddCell(cellPIDstrPIDLessThan532);
 

                    string PID532LessThanReferenceRange = string.Format("PID参数范围:{0} <Kp<{1}     {2} <Ki<{3}", _LightModeSettingsPort.LowerLimitKpLessThan15, _LightModeSettingsPort.UpperLimitKpLessThan15, _LightModeSettingsPort.LowerLimitKiLessThan15,  _LightModeSettingsPort.UpperLimitKiLessThan15);
                    PdfPCell cellPID532LessThanReferenceRange = new PdfPCell(new Phrase(PID532LessThanReferenceRange, font));
                    cellPID532LessThanReferenceRange.DisableBorderSide(15);
                    cellPID532LessThanReferenceRange.Colspan = 2;
                    table.AddCell(cellPID532LessThanReferenceRange);
           

                    string PIDGreaterThan532Line = "光功率＞15mw ";
                    PdfPCell cellPIDGreaterThan532Line = new PdfPCell(new Phrase(PIDGreaterThan532Line, font));
                    cellPIDGreaterThan532Line.DisableBorderSide(15);
                    cellPIDGreaterThan532Line.Colspan = 2;
                    table.AddCell(cellPIDGreaterThan532Line);


                    string strPIDGreaterThan532 = string.Format("PID (Kp/Ki/Kd): {0}/  {1}/  {2}", _LightModeSettingsPort.CurrentPowerClosedloopControlParameterKpValue, _LightModeSettingsPort.CurrentPowerClosedloopControlParameterKiValue, _LightModeSettingsPort.CurrentPowerClosedloopControlParameterKdValue);
                    PdfPCell cellstrPIDGreaterThan532 = new PdfPCell(new Phrase(strPIDGreaterThan532, font));
                    cellstrPIDGreaterThan532.DisableBorderSide(15);
                    cellstrPIDGreaterThan532.Colspan = 2;
                    table.AddCell(cellstrPIDGreaterThan532);

                    string PID532GreaterThanReferenceRange = string.Format("PID参数范围:{0} <Kp<{1}     {2} <Ki<{3}", _LightModeSettingsPort.UpperLimitKpGreaterThan15, _LightModeSettingsPort.LowerLimitKpGreaterThan15, _LightModeSettingsPort.UpperLimitKiGreaterThan15, _LightModeSettingsPort.LowerLimitKiGreaterThan15);
                    PdfPCell cellPID532GreaterThanReferenceRange = new PdfPCell(new Phrase(PID532GreaterThanReferenceRange, font));
                    cellPID532GreaterThanReferenceRange.DisableBorderSide(15);
                    cellPID532GreaterThanReferenceRange.Colspan = 2;
                    table.AddCell(cellPID532GreaterThanReferenceRange);

                    string WorkingCurrent = string.Format("工作电流(Max/Min):   {0}/{1}", _LightModeSettingsPort.MaximumOperatingCurrentLaser, _LightModeSettingsPort.MinimumOperatingCurrentLaser);
                    PdfPCell cellWorkingCurrent = new PdfPCell(new Phrase(WorkingCurrent, font));
                    cellWorkingCurrent.DisableBorderSide(15);
                    cellWorkingCurrent.Colspan = 2;
                    table.AddCell(cellWorkingCurrent);

                    string OpticalPowerCalibration = "光功率标定";
                    PdfPCell cellOpticalPowerCalibration = new PdfPCell(new Phrase(OpticalPowerCalibration, font));
                    cellOpticalPowerCalibration.DisableBorderSide(15);
                    cellOpticalPowerCalibration.Colspan = 2;
                    table.AddCell(cellOpticalPowerCalibration);
 

                    string OpticalPower = string.Format("光功率:5={0} / 10={1} / 15={2} / 20={3} / 25={4} / 30={5} / 35={6}", _LightModeSettingsPort.CurrentPhotodiodeVoltageCorrespondingTo5LaserPowerValue, _LightModeSettingsPort.CurrentPhotodiodeVoltageCorrespondingTo10LaserPowerValue, _LightModeSettingsPort.CurrentPhotodiodeVoltageCorrespondingTo15LaserPowerValue, _LightModeSettingsPort.CurrentPhotodiodeVoltageCorrespondingTo20LaserPowerValue
                        , _LightModeSettingsPort.CurrentPhotodiodeVoltageCorrespondingTo25LaserPowerValue, _LightModeSettingsPort.CurrentPhotodiodeVoltageCorrespondingTo30LaserPowerValue, _LightModeSettingsPort.CurrentPhotodiodeVoltageCorrespondingTo35LaserPowerValue);
                    PdfPCell cellOpticalPower = new PdfPCell(new Phrase(OpticalPower, font));
                    cellOpticalPower.DisableBorderSide(15);
                    cellOpticalPower.Colspan = 2;
                    table.AddCell(cellOpticalPower);

                    string TemperatureCurvePhotodiode = string.Format("光电二极管温度曲线K/B: {0}/{1}:", _LightModeSettingsPort.CurrentKValueofPhotodiodeTemperatureCurveValue, _LightModeSettingsPort.CurrentBValueofPhotodiodeTemperatureCurveValue);
                    PdfPCell cellTemperatureCurvePhotodiode = new PdfPCell(new Phrase(TemperatureCurvePhotodiode, font));
                    cellTemperatureCurvePhotodiode.DisableBorderSide(15);
                    cellTemperatureCurvePhotodiode.Colspan = 2;
                    table.AddCell(cellTemperatureCurvePhotodiode);

                    string LaserErrorCode = string.Format("错误代码:{0}", _LightModeSettingsPort.LaserErrorCode);
                    PdfPCell cellLaserErrorCode = new PdfPCell(new Phrase(LaserErrorCode, font));
                    cellLaserErrorCode.DisableBorderSide(15);
                    cellLaserErrorCode.Colspan = 2;
                    table.AddCell(cellLaserErrorCode);

                    //0X15
                    string LaserTempLaserCoefficientandRadiatorTemper = string.Format("激光器温度/TEC电流: {0}/{1}    散热片温度: {2}", _LightModeSettingsPort.CurrentTECActualTemperatureValue, _LightModeSettingsPort.CurrenTECCurrentCompensationCoefficientValue, _LightModeSettingsPort.CurrenRadiatorTemperatureValue);
                    PdfPCell cellLaserTempLaserCoefficientandRadiatorTemper = new PdfPCell(new Phrase(LaserTempLaserCoefficientandRadiatorTemper, font));
                    cellLaserTempLaserCoefficientandRadiatorTemper.DisableBorderSide(15);
                    cellLaserTempLaserCoefficientandRadiatorTemper.Colspan = 2;
                    table.AddCell(cellLaserTempLaserCoefficientandRadiatorTemper);
                    //0X03
                    string TECCurrentCompensationCoefficientAndLaserCurrentValue = string.Format("激光器光功率/电流 {0}/{1}", _LightModeSettingsPort.GetGet_CurrentLaserLightPowerValueValue, _LightModeSettingsPort.CurrentLaserCurrentValueValue);
                    PdfPCell cellTECCurrentCompensationCoefficientAndLaserCurrentValue = new PdfPCell(new Phrase(TECCurrentCompensationCoefficientAndLaserCurrentValue, font));
                    cellTECCurrentCompensationCoefficientAndLaserCurrentValue.DisableBorderSide(15);
                    cellTECCurrentCompensationCoefficientAndLaserCurrentValue.Colspan = 2;
                    table.AddCell(cellTECCurrentCompensationCoefficientAndLaserCurrentValue);


                }
                else
                {
                    string OpticalPowerCalibration = "光功率标定";
                    PdfPCell cellOpticalPowerCalibration = new PdfPCell(new Phrase(OpticalPowerCalibration, font));
                    cellOpticalPowerCalibration.DisableBorderSide(15);
                    cellOpticalPowerCalibration.Colspan = 2;
                    table.AddCell(cellOpticalPowerCalibration);

                    string OpticalPower = string.Format("光功率:5={0} / 10={1} / 15={2} / 20={3} / 25={4} / 30={5} / 35={6} / 40={7} / 45={8} / 50={9}", _LightModeSettingsPort.CurrentLaserCorrespondingCurrent5ValueValue, _LightModeSettingsPort.CurrentLaserCorrespondingCurrent10ValueValue, _LightModeSettingsPort.CurrentLaserCorrespondingCurrent15ValueValue, _LightModeSettingsPort.CurrentLaserCorrespondingCurrent20ValueValue
                        , _LightModeSettingsPort.CurrentLaserCorrespondingCurrent25ValueValue, _LightModeSettingsPort.CurrentLaserCorrespondingCurrent30ValueValue, _LightModeSettingsPort.CurrentLaserCorrespondingCurrent35ValueValue, _LightModeSettingsPort.CurrentLaserCorrespondingCurrent40ValueValue,
                         _LightModeSettingsPort.CurrentLaserCorrespondingCurrent45ValueValue, _LightModeSettingsPort.CurrentLaserCorrespondingCurrent50ValueValue);
                    PdfPCell cellOpticalPower = new PdfPCell(new Phrase(OpticalPower, font));
                    cellOpticalPower.DisableBorderSide(15);
                    cellOpticalPower.Colspan = 2;
                    table.AddCell(cellOpticalPower);

                    //string Voltage = string.Format("电流:{0}", _LightModeSettingsPort.CurrentPhotodiodeVoltageValue);
                    //PdfPCell cellVoltage = new PdfPCell(new Phrase(Voltage, font));
                    //cellVoltage.DisableBorderSide(15);
                    //cellVoltage.Colspan = 2;
                    //table.AddCell(cellVoltage);


                    string LaserErrorCode = string.Format("错误代码:{0}", _LightModeSettingsPort.LaserErrorCode);
                    PdfPCell cellLaserErrorCode = new PdfPCell(new Phrase(LaserErrorCode, font));
                    cellLaserErrorCode.DisableBorderSide(15);
                    cellLaserErrorCode.Colspan = 2;
                    table.AddCell(cellLaserErrorCode);
                    //0X15
                    string LaserTempLaserCoefficientandRadiatorTemper = string.Format("激光器温度/TEC电流: {0}/{1}    散热片温度: {2}", _LightModeSettingsPort.CurrentTECActualTemperatureValue, _LightModeSettingsPort.CurrenTECCurrentCompensationCoefficientValue, _LightModeSettingsPort.CurrenRadiatorTemperatureValue);
                    PdfPCell cellLaserTempLaserCoefficientandRadiatorTemper = new PdfPCell(new Phrase(LaserTempLaserCoefficientandRadiatorTemper, font));
                    cellLaserTempLaserCoefficientandRadiatorTemper.DisableBorderSide(15);
                    cellLaserTempLaserCoefficientandRadiatorTemper.Colspan = 2;
                    table.AddCell(cellLaserTempLaserCoefficientandRadiatorTemper);
                    //0X03
                    string TECCurrentCompensationCoefficientAndLaserCurrentValue = string.Format("激光器电流 {0}",_LightModeSettingsPort.CurrentLaserCurrentValueValue);
                    PdfPCell cellTECCurrentCompensationCoefficientAndLaserCurrentValue = new PdfPCell(new Phrase(TECCurrentCompensationCoefficientAndLaserCurrentValue, font));
                    cellTECCurrentCompensationCoefficientAndLaserCurrentValue.DisableBorderSide(15);
                    cellTECCurrentCompensationCoefficientAndLaserCurrentValue.Colspan = 2;
                    table.AddCell(cellTECCurrentCompensationCoefficientAndLaserCurrentValue);

                }

                //table.AddCell(cellname);
                //table.AddCell(new PdfPCell(new Phrase("第个任务节点11", font)));
                //PdfPCell pic = new PdfPCell(new Phrase("第个任务节点的截图", font));
                //pic.Colspan = 2;
                //table.AddCell(pic);
                //table.AddCell(new PdfPCell(new Phrase("第个任务节点11", font)));
                //table.AddCell(new PdfPCell(new Phrase("第个任务节点22", font)));
                document.Add(table);



                //string IVOpticalModuleNumberValue = string.Format("光学模块序列号: {0}", _LightModeSettingsPort.IVOpticalModuleNumberValue);
                //document.Add(new Paragraph(Line, font));//
                //document.Add(new Paragraph(IVOpticalModuleNumberValue+ IVSoftwareVersionNumber, font));
                //document.Add(new Paragraph(IVSensorType + CurrentSensorNoValue, font));
                //if (_LightModeSettingsPort.IVSensorType == "0")
                //{
                //    document.Add(new Paragraph(CurrentLightIntensityCalibrationTemperatureValue +CurrentADPTemperatureCalibrationFactorValue, font));
                //    string gain = string.Format("GAIN= 50/{0}  100/{1}  150/{2}  200/{3}  250/{4}  300/{5}  400/{6}  500/{7}", _LightModeSettingsPort.CurrentGain50CalVolValue, _LightModeSettingsPort.CurrentGain100CalVolValue, _LightModeSettingsPort.CurrentGain150CalVolValue, _LightModeSettingsPort.CurrentGain200CalVolValue, _LightModeSettingsPort.CurrentGain250CalVolValue, _LightModeSettingsPort.CurrentGain300CalVolValue, _LightModeSettingsPort.CurrentGain400CalVolValue, _LightModeSettingsPort.CurrentGain500CalVolValue);
                //    document.Add(new Paragraph(gain, font));
                //    document.Add(new Paragraph(CurrentTemperatureSensorSamplingVoltageValue, font));
                //    document.Add(new Paragraph(TempSenserADValue, font));
                //    document.Add(new Paragraph(TempSenserADValue, font));
                //    document.Add(new Paragraph(IVErrorCode, font));
                //    string PGAGAINHighVoltageTemp = string.Format("PGA={0}   /GAIN={1}   /高压={2}   /APD温度{3}", _LightModeSettingsPort.CurrentPGAMultipleValue, _LightModeSettingsPort.CurrentAPDGainValue, _LightModeSettingsPort.CurrentAPDHighVoltageValue, _LightModeSettingsPort.CurrentAPDTempValue);
                //    document.Add(new Paragraph(PGAGAINHighVoltageTemp, font));
                //}
                //else
                //{
                //    document.Add(new Paragraph(CurrentPMTCtrlVolValue+CurrentPMTCompensationCoefficientValue, font));
                //    document.Add(new Paragraph(IVErrorCode, font));
                //}
                //document.Add(new Paragraph(LaserLine, font));//
                //string LaserOpticalModuleNumberValue = string.Format("光学模块序列号: {0}", _LightModeSettingsPort.LaserOpticalModuleNumberValue);
                //string LaserSoftwareVersionNumber = string.Format("软件版本号: {0}", _LightModeSettingsPort.LaserSoftwareVersionNumber);
                //document.Add(new Paragraph(LaserOpticalModuleNumberValue + LaserSoftwareVersionNumber, font));
                //string CurrentLaserWavaLengthValue = string.Format("激光器波长: {0}", _LightModeSettingsPort.CurrentLaserWavaLengthValue);
                //string CurrentLaserNoValue = string.Format("激光器编号: {0}", _LightModeSettingsPort.CurrentLaserNoValue);
                //document.Add(new Paragraph(CurrentLaserWavaLengthValue +  CurrentLaserNoValue, font));
                //string CurrentTEControlTemperatureValue = string.Format("TEC控制温度(℃)/最大电流(mA):     {0}/{1}", _LightModeSettingsPort.CurrentTECActualTemperatureValue, _LightModeSettingsPort.CurrentTEControlTemperatureValue);
                //document.Add(new Paragraph(CurrentTEControlTemperatureValue, font));
                //string TECPID = string.Format("TEC PID(Kp/Ki/Kd): {0}/  {1}/  {2}", _LightModeSettingsPort.CurrentTECRefrigerationControlParameterKpValue, _LightModeSettingsPort.CurrentTECRefrigerationControlParameterKiValue, _LightModeSettingsPort.CurrentTECRefrigerationControlParameterKdValue);
                //document.Add(new Paragraph(TECPID, font));
                //if (_LightModeSettingsPort.CurrentLaserWavaLengthValue == 532)
                //{
                //    string PID532KpLessThanLine = "光功率≤15mw ";
                //    document.Add(new Paragraph(PID532KpLessThanLine, font));//

                //    string strPIDLessThan532 = string.Format("PID (Kp/Ki/Kd): {0}/  {1}/  {2}", _LightModeSettingsPort.PowerClosedloopControlParameterKpLessThanOrEqual15, _LightModeSettingsPort.PowerClosedloopControlParameterKiLessThanOrEqual15, _LightModeSettingsPort.PowerClosedloopControlParameterKdLessThanOrEqual15);
                //    document.Add(new Paragraph(strPIDLessThan532, font));

                //    string PID532LessThanReferenceRange = string.Format("PID参数范围:{0} <Kp<{1}     {2} <Ki<{3}", _LightModeSettingsPort.UpperLimitKpLessThan15, _LightModeSettingsPort.LowerLimitKpLessThan15, _LightModeSettingsPort.UpperLimitKiLessThan15, _LightModeSettingsPort.LowerLimitKiLessThan15);
                //    document.Add(new Paragraph(PID532LessThanReferenceRange, font));

                //    string PIDGreaterThan532Line = "光功率＞15mw ";
                //    document.Add(new Paragraph(PIDGreaterThan532Line, font));//

                //    string strPIDGreaterThan532 = string.Format("PID (Kp/Ki/Kd): {0}/  {1}/  {2}", _LightModeSettingsPort.CurrentPowerClosedloopControlParameterKpValue, _LightModeSettingsPort.CurrentPowerClosedloopControlParameterKiValue, _LightModeSettingsPort.CurrentPowerClosedloopControlParameterKdValue);
                //    document.Add(new Paragraph(strPIDGreaterThan532, font));

                //    string PID532GreaterThanReferenceRange = string.Format("PID参数范围:{0} <Kp<{1}     {2} <Ki<{3}", _LightModeSettingsPort.UpperLimitKpGreaterThan15, _LightModeSettingsPort.LowerLimitKpGreaterThan15, _LightModeSettingsPort.UpperLimitKiGreaterThan15, _LightModeSettingsPort.LowerLimitKiGreaterThan15);
                //    document.Add(new Paragraph(PID532GreaterThanReferenceRange, font));

                //    string WorkingCurrent = string.Format("工作电流(Max/Min):   {0}/{1}", _LightModeSettingsPort.MaximumOperatingCurrentLaser, _LightModeSettingsPort.MinimumOperatingCurrentLaser);
                //    document.Add(new Paragraph(WorkingCurrent, font));

                //    string OpticalPowerCalibration = "光功率标定";
                //    document.Add(new Paragraph(OpticalPowerCalibration, font));//

                //    string OpticalPower = string.Format("光功率:5={0}/10={1}/15={2}/20={3}/25={4}/30={5}/35={6}", _LightModeSettingsPort.CurrentPhotodiodeVoltageCorrespondingTo5LaserPowerValue, _LightModeSettingsPort.CurrentPhotodiodeVoltageCorrespondingTo10LaserPowerValue, _LightModeSettingsPort.CurrentPhotodiodeVoltageCorrespondingTo15LaserPowerValue, _LightModeSettingsPort.CurrentPhotodiodeVoltageCorrespondingTo20LaserPowerValue
                //        , _LightModeSettingsPort.CurrentPhotodiodeVoltageCorrespondingTo25LaserPowerValue, _LightModeSettingsPort.CurrentPhotodiodeVoltageCorrespondingTo30LaserPowerValue, _LightModeSettingsPort.CurrentPhotodiodeVoltageCorrespondingTo35LaserPowerValue);
                //    document.Add(new Paragraph(OpticalPower, font));

                //    string Voltage = string.Format("电压:", _LightModeSettingsPort.CurrentPhotodiodeVoltageValue);
                //    document.Add(new Paragraph(Voltage, font));//

                //    string TemperatureCurvePhotodiode = string.Format("光电二极管温度曲线K/B: {0}/{1}:", _LightModeSettingsPort.CurrentKValueofPhotodiodeTemperatureCurveValue, _LightModeSettingsPort.CurrentBValueofPhotodiodeTemperatureCurveValue);
                //    document.Add(new Paragraph(TemperatureCurvePhotodiode, font));//

                //    string LaserErrorCode = string.Format("错误代码:", _LightModeSettingsPort.LaserErrorCode);
                //    document.Add(new Paragraph(LaserErrorCode, font));//

                //    string LaserTempLaserCoefficientandRadiatorTemper = string.Format("激光器温度/激光器电流: {0}/{1} 散热片温度: {2}", _LightModeSettingsPort.CurrentTECActualTemperatureValue, _LightModeSettingsPort.CurrenTECCurrentCompensationCoefficientValue, _LightModeSettingsPort.CurrenRadiatorTemperatureValue);
                //    document.Add(new Paragraph(LaserTempLaserCoefficientandRadiatorTemper, font));//

                //    string TECCurrentCompensationCoefficientAndLaserCurrentValue = string.Format("激光器光功率/电流 {0}/{1}", _LightModeSettingsPort.CurrenTECCurrentCompensationCoefficientValue, _LightModeSettingsPort.CurrenTECCurrentCompensationCoefficientValue, _LightModeSettingsPort.CurrentLaserCurrentValueValue);
                //    document.Add(new Paragraph(TECCurrentCompensationCoefficientAndLaserCurrentValue, font));//


                //}
                //else
                //{
                //    string OpticalPowerCalibration = "光功率标定";
                //    document.Add(new Paragraph(OpticalPowerCalibration, font));//

                //    string OpticalPower = string.Format("光功率:5={0}/10={1}/15={2}/20={3}/25={4}/30={5}/35={6}/40={7}/45={8}/50={9}", _LightModeSettingsPort.CurrentLaserCorrespondingCurrent5ValueValue, _LightModeSettingsPort.CurrentLaserCorrespondingCurrent10ValueValue, _LightModeSettingsPort.CurrentLaserCorrespondingCurrent15ValueValue, _LightModeSettingsPort.CurrentLaserCorrespondingCurrent20ValueValue
                //        , _LightModeSettingsPort.CurrentLaserCorrespondingCurrent25ValueValue, _LightModeSettingsPort.CurrentLaserCorrespondingCurrent30ValueValue, _LightModeSettingsPort.CurrentLaserCorrespondingCurrent35ValueValue, _LightModeSettingsPort.CurrentLaserCorrespondingCurrent40ValueValue,
                //         _LightModeSettingsPort.CurrentLaserCorrespondingCurrent45ValueValue, _LightModeSettingsPort.CurrentLaserCorrespondingCurrent50ValueValue);
                //    document.Add(new Paragraph(OpticalPower, font));

                //    string Voltage = string.Format("电压:", _LightModeSettingsPort.CurrentPhotodiodeVoltageValue);
                //    document.Add(new Paragraph(Voltage, font));//


                //    string LaserErrorCode = string.Format("错误代码:", _LightModeSettingsPort.LaserErrorCode);
                //    document.Add(new Paragraph(LaserErrorCode, font));//

                //    string LaserTempLaserCoefficientandRadiatorTemper = string.Format("激光器温度/激光器电流: {0}/{1} 散热片温度: {2}", _LightModeSettingsPort.CurrentTECActualTemperatureValue, _LightModeSettingsPort.CurrenTECCurrentCompensationCoefficientValue, _LightModeSettingsPort.CurrenRadiatorTemperatureValue);
                //    document.Add(new Paragraph(LaserTempLaserCoefficientandRadiatorTemper, font));//

                //    string TECCurrentCompensationCoefficientAndLaserCurrentValue = string.Format("激光器光功率/电流 {0}/{1}", _LightModeSettingsPort.CurrenTECCurrentCompensationCoefficientValue, _LightModeSettingsPort.CurrenTECCurrentCompensationCoefficientValue, _LightModeSettingsPort.CurrentLaserCurrentValueValue);
                //    document.Add(new Paragraph(TECCurrentCompensationCoefficientAndLaserCurrentValue, font));//

                //}



                //document.Add(new Paragraph(IVSoftwareVersionNumber, font));//
                //document.Add(new Paragraph(IVSensorType, font));//
                //document.Add(new Paragraph(CurrentSensorNoValue, font));//
                //document.Add(new Paragraph(CurrentPGAMultipleValue, font));//
                //document.Add(new Paragraph(CurrentAPDGainValue, font));//
                //document.Add(new Paragraph(CurrentAPDTempValue, font));//
                //document.Add(new Paragraph(CurrentAPDHighVoltageValue, font));//
                //document.Add(new Paragraph(CurrentLightIntensityCalibrationTemperatureValue, font));//
                //document.Add(new Paragraph(CurrentGain50CalVolValue, font));//
                //document.Add(new Paragraph(CurrentGain100CalVolValue, font));//
                //document.Add(new Paragraph(CurrentGain150CalVolValue, font));//
                //document.Add(new Paragraph(CurrentGain200CalVolValue, font));//
                //document.Add(new Paragraph(CurrentGain250CalVolValue, font));//
                //document.Add(new Paragraph(CurrentGain300CalVolValue, font));//
                //document.Add(new Paragraph(CurrentGain400CalVolValue, font));//
                //document.Add(new Paragraph(CurrentGain500CalVolValue, font));//
                //document.Add(new Paragraph(CurrentPMTCtrlVolValue, font));//
                //document.Add(new Paragraph(CurrentPMTCompensationCoefficientValue, font));//
                //document.Add(new Paragraph(CurrentTempSenser1282ADValue, font));//
                //document.Add(new Paragraph(CurrentTempSenser6459ADValue, font));//
                //document.Add(new Paragraph(CurrentTempSenserADValue, font));//
                //document.Add(new Paragraph(CurrentIVBoardRunningStateValue, font));//
                //document.Add(new Paragraph(CurrentTemperatureSensorSamplingVoltageValue, font));//
                //document.Add(new Paragraph(CurrentADPTemperatureCalibrationFactorValue, font));//
                //document.Add(new Paragraph(IVErrorCode, font));//
                //document.Add(new Paragraph(IVOpticalModuleNumberValue, font));//
                //string LaserSoftwareVersionNumber = string.Format("软件版本号: {0}", _LightModeSettingsPort.LaserSoftwareVersionNumber);
                //string CurrentLaserNoValue = string.Format("激光器编号: {0}", _LightModeSettingsPort.CurrentLaserNoValue);
                //string CurrentLaserWavaLengthValue = string.Format("激光器波长（nm）: {0}", _LightModeSettingsPort.CurrentLaserWavaLengthValue);
                //string CurrentLaserCurrentValueValue = string.Format("激光器电流值（0.01mA）: {0}", _LightModeSettingsPort.CurrentLaserCurrentValueValue);
                //string CurrentLaserCorrespondingCurrent5ValueValue = string.Format("5mW激光功率对应的电流值（0.01mA）: {0}", _LightModeSettingsPort.CurrentLaserCorrespondingCurrent5ValueValue);
                //string CurrentLaserCorrespondingCurrent10ValueValue = string.Format("10mW激光功率对应的电流值（0.01mA）: {0}", _LightModeSettingsPort.CurrentLaserCorrespondingCurrent10ValueValue);
                //string CurrentLaserCorrespondingCurrent15ValueValue = string.Format("15mW激光功率对应的电流值（0.01mA）: {0}", _LightModeSettingsPort.CurrentLaserCorrespondingCurrent15ValueValue);
                //string CurrentLaserCorrespondingCurrent20ValueValue = string.Format("20mW激光功率对应的电流值（0.01mA）: {0}", _LightModeSettingsPort.CurrentLaserCorrespondingCurrent20ValueValue);
                //string CurrentLaserCorrespondingCurrent25ValueValue = string.Format("25mW激光功率对应的电流值（0.01mA）: {0}", _LightModeSettingsPort.CurrentLaserCorrespondingCurrent25ValueValue);
                //string CurrentLaserCorrespondingCurrent30ValueValue = string.Format("30mW激光功率对应的电流值（0.01mA）: {0}", _LightModeSettingsPort.CurrentLaserCorrespondingCurrent30ValueValue);
                //string CurrentLaserCorrespondingCurrent35ValueValue = string.Format("35mW激光功率对应的电流值（0.01mA）: {0}", _LightModeSettingsPort.CurrentLaserCorrespondingCurrent35ValueValue);
                //string CurrentLaserCorrespondingCurrent40ValueValue = string.Format("40mW激光功率对应的电流值（0.01mA）: {0}", _LightModeSettingsPort.CurrentLaserCorrespondingCurrent40ValueValue);
                //string CurrentLaserCorrespondingCurrent45ValueValue = string.Format("55mW激光功率对应的电流值（0.01mA）: {0}", _LightModeSettingsPort.CurrentLaserCorrespondingCurrent45ValueValue);
                //string CurrentLaserCorrespondingCurrent50ValueValue = string.Format("50mW激光功率对应的电流值（0.01mA）: {0}", _LightModeSettingsPort.CurrentLaserCorrespondingCurrent50ValueValue);
                //string CurrentTECActualTemperatureValue = string.Format("TEC实际温度（0.1℃）: {0}", _LightModeSettingsPort.CurrentTECActualTemperatureValue);
                //string CurrentTEControlTemperatureValue = string.Format("TEC控制温度（单位0.1℃）: {0}", _LightModeSettingsPort.CurrentTEControlTemperatureValue);
                //string CurrentTECMaximumCoolingCurrentValue = string.Format("TEC最大制冷电流（mA）: {0}", _LightModeSettingsPort.CurrentTECMaximumCoolingCurrentValue);
                //string CurrentTECRefrigerationControlParameterKpValue = string.Format("TEC制冷控制参数Kp: {0}", _LightModeSettingsPort.CurrentTECRefrigerationControlParameterKpValue);
                //string CurrentTECRefrigerationControlParameterKiValue = string.Format("TEC制冷控制参数Ki: {0}", _LightModeSettingsPort.CurrentTECRefrigerationControlParameterKiValue);
                //string CurrentTECRefrigerationControlParameterKdValue = string.Format("TEC制冷控制参数Kd: {0}", _LightModeSettingsPort.CurrentTECRefrigerationControlParameterKdValue);
                //string CurrentLaserLightPowerValueValue = string.Format("激光光功率值（0.1mW）: {0}", _LightModeSettingsPort.CurrentLaserLightPowerValueValue);
                //string CurrentTECWorkingStatusValue = string.Format("TEC工作状态: {0}", _LightModeSettingsPort.CurrentTECWorkingStatusValue);
                //string CurrentTECCurrentDirectionValue = string.Format("TEC电流方向: {0}", _LightModeSettingsPort.CurrentTECCurrentDirectionValue);
                //string CurrenRadiatorTemperatureValue = string.Format("散热器温度（0.1℃）: {0}", _LightModeSettingsPort.CurrenRadiatorTemperatureValue);
                //string CurrenTECCurrentCompensationCoefficientValue = string.Format("TEC电流补偿系数（%）: {0}", _LightModeSettingsPort.CurrenTECCurrentCompensationCoefficientValue);
                //string GetGet_CurrentLaserLightPowerValueValue = string.Format("当前激光光功率值（0.1mW）: {0}", _LightModeSettingsPort.GetGet_CurrentLaserLightPowerValueValue);
                //string CurrentPowerClosedloopControlParameterKpValue = string.Format("功率闭环控制参数Kp(>15mW): {0}", _LightModeSettingsPort.CurrentPowerClosedloopControlParameterKpValue);
                //string CurrentPowerClosedloopControlParameterKiValue = string.Format("功率闭环控制参数Ki(>15mW): {0}", _LightModeSettingsPort.CurrentPowerClosedloopControlParameterKiValue);
                //string CurrentPowerClosedloopControlParameterKdValue = string.Format("功率闭环控制参数Kd(>15mW): {0}", _LightModeSettingsPort.CurrentPowerClosedloopControlParameterKdValue);
                //string CurrentPhotodiodeVoltageCorrespondingTo5LaserPowerValue = string.Format("5mW激光功率对应的光电二极管电压（0.0001V）: {0}", _LightModeSettingsPort.CurrentPhotodiodeVoltageCorrespondingTo5LaserPowerValue);
                //string CurrentPhotodiodeVoltageCorrespondingTo10LaserPowerValue = string.Format("10mW激光功率对应的光电二极管电压（0.0001V）: {0}", _LightModeSettingsPort.CurrentPhotodiodeVoltageCorrespondingTo10LaserPowerValue);
                //string CurrentPhotodiodeVoltageCorrespondingTo15LaserPowerValue = string.Format("15mW激光功率对应的光电二极管电压（0.0001V）: {0}", _LightModeSettingsPort.CurrentPhotodiodeVoltageCorrespondingTo15LaserPowerValue);
                //string CurrentPhotodiodeVoltageCorrespondingTo20LaserPowerValue = string.Format("20mW激光功率对应的光电二极管电压（0.0001V）: {0}", _LightModeSettingsPort.CurrentPhotodiodeVoltageCorrespondingTo20LaserPowerValue);
                //string CurrentPhotodiodeVoltageCorrespondingTo25LaserPowerValue = string.Format("25mW激光功率对应的光电二极管电压（0.0001V）: {0}", _LightModeSettingsPort.CurrentPhotodiodeVoltageCorrespondingTo25LaserPowerValue);
                //string CurrentPhotodiodeVoltageCorrespondingTo30LaserPowerValue = string.Format("30mW激光功率对应的光电二极管电压（0.0001V）: {0}", _LightModeSettingsPort.CurrentPhotodiodeVoltageCorrespondingTo30LaserPowerValue);
                //string CurrentPhotodiodeVoltageCorrespondingTo35LaserPowerValue = string.Format("35mW激光功率对应的光电二极管电压（0.0001V）: {0}", _LightModeSettingsPort.CurrentPhotodiodeVoltageCorrespondingTo35LaserPowerValue);
                //string CurrentPhotodiodeVoltageValue = string.Format("光电二极管电压（0.0001V）: {0}", _LightModeSettingsPort.CurrentPhotodiodeVoltageValue);
                //string CurrentKValueofPhotodiodeTemperatureCurveValue = string.Format("光电二极管温度曲线K值（0.0001V）: {0}", _LightModeSettingsPort.CurrentKValueofPhotodiodeTemperatureCurveValue);
                //string CurrentBValueofPhotodiodeTemperatureCurveValue = string.Format("光电二极管温度曲线b值（0.0001V）: {0}", _LightModeSettingsPort.CurrentBValueofPhotodiodeTemperatureCurveValue);
                //string LaserErrorCode = string.Format("错误代码: {0}", _LightModeSettingsPort.LaserErrorCode);
                //string LaserOpticalModuleNumberValue = string.Format("光学模块序列号: {0}", _LightModeSettingsPort.LaserOpticalModuleNumberValue);
                //string PowerClosedloopControlParameterKpLessThanOrEqual15 = string.Format("功率闭环控制参数Kp(<=15mw）: {0}", _LightModeSettingsPort.PowerClosedloopControlParameterKpLessThanOrEqual15);
                //string PowerClosedloopControlParameterKiLessThanOrEqual15 = string.Format("功率闭环控制参数Ki(<=15mw）: {0}", _LightModeSettingsPort.PowerClosedloopControlParameterKiLessThanOrEqual15);
                //string PowerClosedloopControlParameterKdLessThanOrEqual15 = string.Format("功率闭环控制参数Kd(<=15mw）: {0}", _LightModeSettingsPort.PowerClosedloopControlParameterKdLessThanOrEqual15);
                //string UpperLimitKpLessThan15 = string.Format("15MW及以下的KP的最大设置值: {0}", _LightModeSettingsPort.UpperLimitKpLessThan15);
                //string LowerLimitKpLessThan15 = string.Format("15MW及以下的KP的最小设置值: {0}", _LightModeSettingsPort.LowerLimitKpLessThan15);
                //string UpperLimitKiLessThan15 = string.Format("15MW及以下的KI的最大设置值: {0}", _LightModeSettingsPort.UpperLimitKiLessThan15);
                //string LowerLimitKiLessThan15 = string.Format("15MW及以下的Ki的最小设置值: {0}", _LightModeSettingsPort.LowerLimitKiLessThan15);
                //string UpperLimitKpGreaterThan15 = string.Format("15MW以上的KP的最大设置值: {0}", _LightModeSettingsPort.UpperLimitKpGreaterThan15);
                //string LowerLimitKpGreaterThan15 = string.Format("15MW以上的KP的最小设置值: {0}", _LightModeSettingsPort.LowerLimitKpGreaterThan15);
                //string UpperLimitKiGreaterThan15 = string.Format("15MW以上的KI的最大设置值: {0}", _LightModeSettingsPort.UpperLimitKiGreaterThan15);
                //string LowerLimitKiGreaterThan15 = string.Format("15MW以上的Ki的最小设置值: {0}", _LightModeSettingsPort.LowerLimitKiGreaterThan15);
                //string MaximumOperatingCurrentLaser = string.Format("532NM激光器的最大工作电流（0.1mA）: {0}", _LightModeSettingsPort.MaximumOperatingCurrentLaser);
                //string MinimumOperatingCurrentLaser = string.Format("532NM激光器的最小启动电流（0.1mA）: {0}", _LightModeSettingsPort.MinimumOperatingCurrentLaser);
                //document.Add(new Paragraph(LaserLine, font));//
                //document.Add(new Paragraph(LaserSoftwareVersionNumber, font));//
                //document.Add(new Paragraph(CurrentLaserNoValue, font));//
                //document.Add(new Paragraph(CurrentLaserWavaLengthValue, font));//
                //document.Add(new Paragraph(CurrentLaserCurrentValueValue, font));//
                //document.Add(new Paragraph(CurrentLaserCorrespondingCurrent5ValueValue, font));//
                //document.Add(new Paragraph(CurrentLaserCorrespondingCurrent10ValueValue, font));//
                //document.Add(new Paragraph(CurrentLaserCorrespondingCurrent15ValueValue, font));//
                //document.Add(new Paragraph(CurrentLaserCorrespondingCurrent20ValueValue, font));//
                //document.Add(new Paragraph(CurrentLaserCorrespondingCurrent25ValueValue, font));//
                //document.Add(new Paragraph(CurrentLaserCorrespondingCurrent30ValueValue, font));//
                //document.Add(new Paragraph(CurrentLaserCorrespondingCurrent35ValueValue, font));//
                //document.Add(new Paragraph(CurrentLaserCorrespondingCurrent40ValueValue, font));//
                //document.Add(new Paragraph(CurrentLaserCorrespondingCurrent35ValueValue, font));//
                //document.Add(new Paragraph(CurrentLaserCorrespondingCurrent50ValueValue, font));//
                //document.Add(new Paragraph(CurrentTECActualTemperatureValue, font));//
                //document.Add(new Paragraph(CurrentTEControlTemperatureValue, font));//
                //document.Add(new Paragraph(CurrentTECMaximumCoolingCurrentValue, font));//
                //document.Add(new Paragraph(CurrentTECRefrigerationControlParameterKpValue, font));//
                //document.Add(new Paragraph(CurrentTECRefrigerationControlParameterKiValue, font));//
                //document.Add(new Paragraph(CurrentTECRefrigerationControlParameterKdValue, font));//
                //document.Add(new Paragraph(CurrentLaserLightPowerValueValue, font));//
                //document.Add(new Paragraph(CurrentTECWorkingStatusValue, font));//
                //document.Add(new Paragraph(CurrentTECCurrentDirectionValue, font));//
                //document.Add(new Paragraph(CurrenRadiatorTemperatureValue, font));//
                //document.Add(new Paragraph(CurrenTECCurrentCompensationCoefficientValue, font));//
                //document.Add(new Paragraph(GetGet_CurrentLaserLightPowerValueValue, font));//
                //document.Add(new Paragraph(CurrentPowerClosedloopControlParameterKpValue, font));//
                //document.Add(new Paragraph(CurrentPowerClosedloopControlParameterKiValue, font));//
                //document.Add(new Paragraph(CurrentPowerClosedloopControlParameterKdValue, font));//
                //document.Add(new Paragraph(CurrentPhotodiodeVoltageCorrespondingTo5LaserPowerValue, font));//
                //document.Add(new Paragraph(CurrentPhotodiodeVoltageCorrespondingTo10LaserPowerValue, font));//
                //document.Add(new Paragraph(CurrentPhotodiodeVoltageCorrespondingTo15LaserPowerValue, font));//
                //document.Add(new Paragraph(CurrentPhotodiodeVoltageCorrespondingTo20LaserPowerValue, font));//
                //document.Add(new Paragraph(CurrentPhotodiodeVoltageCorrespondingTo25LaserPowerValue, font));//
                //document.Add(new Paragraph(CurrentPhotodiodeVoltageCorrespondingTo30LaserPowerValue, font));//
                //document.Add(new Paragraph(CurrentPhotodiodeVoltageCorrespondingTo35LaserPowerValue, font));//
                //document.Add(new Paragraph(CurrentPhotodiodeVoltageValue, font));//
                //document.Add(new Paragraph(CurrentKValueofPhotodiodeTemperatureCurveValue, font));//
                //document.Add(new Paragraph(CurrentBValueofPhotodiodeTemperatureCurveValue, font));//
                //document.Add(new Paragraph(LaserErrorCode, font));//
                //document.Add(new Paragraph(LaserOpticalModuleNumberValue, font));//
                //document.Add(new Paragraph(PowerClosedloopControlParameterKpLessThanOrEqual15, font));//
                //document.Add(new Paragraph(PowerClosedloopControlParameterKiLessThanOrEqual15, font));//
                //document.Add(new Paragraph(PowerClosedloopControlParameterKdLessThanOrEqual15, font));//
                //document.Add(new Paragraph(UpperLimitKpLessThan15, font));//
                //document.Add(new Paragraph(LowerLimitKpLessThan15, font));//
                //document.Add(new Paragraph(UpperLimitKiLessThan15, font));//
                //document.Add(new Paragraph(LowerLimitKiLessThan15, font));//
                //document.Add(new Paragraph(UpperLimitKpGreaterThan15, font));//
                //document.Add(new Paragraph(LowerLimitKpGreaterThan15, font));//
                //document.Add(new Paragraph(UpperLimitKiGreaterThan15, font));//
                //document.Add(new Paragraph(LowerLimitKiGreaterThan15, font));//
                //document.Add(new Paragraph(MaximumOperatingCurrentLaser, font));//
                //document.Add(new Paragraph(MinimumOperatingCurrentLaser, font));//
                document.Close();//结束位置
            }
            MessageBox.Show("保存成功");
        }
        private bool GetInfo()
        {
            int sleep = 500;
            _LightModeSettingsPort.IsPrint = false;

            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetIVvNumberValue();
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrentSensorType();
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrentSensorNoValue();
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrentPGAMultiple();
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrentAPDGain();
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrentAPDTempValue();
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrentAPDHighVoltageValue();
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrentLightIntensityCalibrationTemperatureValue();
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrentGain50CalVolValue();
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrentGain100CalVolValue();
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrentGain150CalVolValue();
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrentGain200CalVolValue();
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrentGain250CalVolValue();
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrentGain300CalVolValue();
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrentGain400CalVolValue();
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrentGain500CalVolValue();
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrentPMTCtrlVolValue();
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrentPMTCompensationCoefficientValue();
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrentTempSenser1282ADValue();
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrentTempSenser6459ADValue();
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrentTempSenserADValue();
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrentIVBoardRunningStateValue();
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrentTemperatureSensorSamplingVoltageValue();
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrentADPTemperatureCalibrationFactorValue();
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetIVErrorCodeValue();
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetIVOpticalModuleNumberValue();

            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrentLaserNoValue();
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetLaserOpticalModuleNumberValue();
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetLaserErrorCodeValue();
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetLaservNumberValue();
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrentLaserWavaLengthValue();
      

            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrentLaserCorrespondingCurrentValueValue(5);
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrentLaserCorrespondingCurrentValueValue(10);
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrentLaserCorrespondingCurrentValueValue(15);
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrentLaserCorrespondingCurrentValueValue(20);
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrentLaserCorrespondingCurrentValueValue(25);
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrentLaserCorrespondingCurrentValueValue(30);
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrentLaserCorrespondingCurrentValueValue(35);
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrentLaserCorrespondingCurrentValueValue(40);
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrentLaserCorrespondingCurrentValueValue(45);
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrentLaserCorrespondingCurrentValueValue(50);
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrentTECActualTemperatureValue();

            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrentTEControlTemperatureValue();

            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrentTECMaximumCoolingCurrentValue();

            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrentTECRefrigerationControlParameterKpValue();

            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrentTECRefrigerationControlParameterKiValue();

            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrentTECRefrigerationControlParameterKdValue();

            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrentLaserLightPowerValueValue();

            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrentTECWorkingStatusValue();

            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrentTECCurrentDirectionValue();

            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrenRadiatorTemperatureValue();

            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrentTECCurrentCompensationCoefficientValue();

            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.Get_CurrentLaserLightPowerValueValue();

            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrentPowerClosedloopControlParameterKpValue();

            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrentPowerClosedloopControlParameterKiValue();

            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrentPowerClosedloopControlParameterKdValue();

            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrentPhotodiodeVoltageCorrespondingToLaserPowerValue(5);


            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrentPhotodiodeVoltageCorrespondingToLaserPowerValue(10);
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrentPhotodiodeVoltageCorrespondingToLaserPowerValue(15);
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrentPhotodiodeVoltageCorrespondingToLaserPowerValue(20);
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrentPhotodiodeVoltageCorrespondingToLaserPowerValue(25);
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrentPhotodiodeVoltageCorrespondingToLaserPowerValue(30);
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrentPhotodiodeVoltageCorrespondingToLaserPowerValue(35);
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrentPhotodiodeVoltageValue();

            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrentKValueofPhotodiodeTemperatureCurveValue();

            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrentBValueofPhotodiodeTemperatureCurveValue();

            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetPowerClosedloopControlParameterKpLessThanOrEqual15Value();

            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetPowerClosedloopControlParameterKiLessThanOrEqual15Value();

            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetPowerClosedloopControlParameterKdLessThanOrEqual15Value();

            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetUpperLimitKpLessThan15Value();

            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetLowerLimitKpLessThan15Value();

            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetUpperLimitKiLessThan15Value();

            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetLowerLimitKiLessThan15Value();

            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetUpperLimitKpGreaterThan15Value();

            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetLowerLimitKpGreaterThan15Value();

            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetUpperLimitKiGreaterThan15Value();

            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetLowerLimitKiGreaterThan15Value();

            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetMaximumOperatingCurrentLaserValue();

            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetMinimumOperatingCurrentLaserValue();
            while (_LightModeSettingsPort.IsBusy)
            {
                Thread.Sleep(1);
            }
            Thread.Sleep(sleep);
            _LightModeSettingsPort.GetCurrentLaserCurrentValueValue();
            int i = 0;
            while (!_LightModeSettingsPort.IsPrint)
            {
                i++;
                Thread.Sleep(1000);
                if (i == 30)
                {
                    MessageBox.Show("长时间未获取到数据");
                    return false;
                }
            }
            return true;
        }
        //public void ExecuteGenerateReportCommand(object parameter)
        //{
        //    //给出文件保存信息，确定保存位置
        //    System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
        //    saveFileDialog.Filter = "PDF文件（*.PDF）|*.PDF";
        //    //判断是否点击确认按钮
        //    saveFileDialog.FileName = "光学模块报告_"+ ".pdf";
        //    if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        //    {
        //        string filePath = saveFileDialog.FileName;//获取PDF文件名称
        //        string IVSystemVersions = "";
        //        string IVSystemErrorCode = "";
        //        string IVOpticalModuleSerialNumber = "";
        //        string LaserSystemVersions = "";
        //        string LaserErrorCode = "";
        //        string LaserOpticalModuleSerialNumber = "";
        //        double CurrentLaserCorrespondingCurrent5ValueValue = 0;
        //        double CurrentLaserCorrespondingCurrent10ValueValue = 0;
        //        double CurrentLaserCorrespondingCurrent15ValueValue = 0;
        //        double CurrentLaserCorrespondingCurrent20ValueValue = 0;
        //        double CurrentLaserCorrespondingCurrent25ValueValue = 0;
        //        double CurrentLaserCorrespondingCurrent30ValueValue = 0;
        //        double CurrentLaserCorrespondingCurrent35ValueValue = 0;
        //        double CurrentLaserCorrespondingCurrent40ValueValue = 0;
        //        double CurrentLaserCorrespondingCurrent45ValueValue = 0;
        //        double CurrentLaserCorrespondingCurrent50ValueValue = 0;
        //        double CurrentPhotodiodeVoltageCorrespondingTo5LaserPowerValue = 0;
        //        double CurrentPhotodiodeVoltageCorrespondingTo10LaserPowerValue = 0;
        //        double CurrentPhotodiodeVoltageCorrespondingTo15LaserPowerValue = 0;
        //        double CurrentPhotodiodeVoltageCorrespondingTo20LaserPowerValue = 0;
        //        double CurrentPhotodiodeVoltageCorrespondingTo25LaserPowerValue = 0;
        //        double CurrentPhotodiodeVoltageCorrespondingTo30LaserPowerValue = 0;
        //        double CurrentPhotodiodeVoltageCorrespondingTo35LaserPowerValue = 0;
        //        double CurrentGain50CalVolValue = 0;
        //        double CurrentGain100CalVolValue = 0;
        //        double CurrentGain150CalVolValue = 0;
        //        double CurrentGain200CalVolValue = 0;
        //        double CurrentGain250CalVolValue = 0;
        //        double CurrentGain300CalVolValue = 0;
        //        double CurrentGain400CalVolValue = 0;
        //        double CurrentGain500CalVolValue = 0;
        //        double CurrentPMTCompensationCoefficientValue = 0;
        //        double CurrentTempSenser6459ADValue = 0;
        //        double CurrentTempSenser1282ADValue = 0;
        //        double CurrentLightIntensityCalibrationTemperatureValue = 0;
        //        string LaserWaveLength = "";
        //        string LaserNumberNo = "";
        //        double TECCtrlTemp = 0;
        //        double CurrentTECMaximumCoolingCurrentValue = 0;
        //        double CurrentTECRefrigerationControlParameterKpValue = 0;
        //        double CurrentTECRefrigerationControlParameterKiValue = 0;
        //        double CurrentTECRefrigerationControlParameterKdValue = 0;
        //        double CurrentPowerClosedloopControlParameterKpValue = 0;
        //        double CurrentPowerClosedloopControlParameterKiValue = 0;
        //        double CurrentPowerClosedloopControlParameterKdValue = 0;
        //        string CurrentSensorNoValue = "";
        //        int time = 0;
        //        IvSensorType SensorML1=IvSensorType.NA;
        //        for (int b = 0; b < 1; b++)
        //        {
        //            EthernetDevice.GetIVSystemVersions();
        //            Thread.Sleep(500);
        //            while (true)
        //            {
        //                IVSystemVersions = EthernetController.IVEstimatedVersionNumberBoard[IVChannels.ChannelC];
        //                if (string.IsNullOrEmpty(IVSystemVersions))
        //                {
        //                    time++;
        //                    Thread.Sleep(1000);
        //                }
        //                else
        //                {
        //                    time = 0;
        //                    break;
        //                }
        //                if (time == 5)//5秒未获取到指定值
        //                {
        //                    time = 0;
        //                    break;
        //                }
        //            }
        //            EthernetDevice.GetIVSystemErrorCode();
        //            Thread.Sleep(500);
        //            while (true)
        //            {
        //                IVSystemErrorCode = EthernetController.IVErrorCode[IVChannels.ChannelC];
        //                if (string.IsNullOrEmpty(IVSystemErrorCode))
        //                {
        //                    time = 0;
        //                    time++;
        //                    Thread.Sleep(1000);
        //                }
        //                else
        //                {
        //                    break;
        //                }
        //                if (time == 5)//5秒未获取到指定值
        //                {
        //                    time = 0;
        //                    break;
        //                }
        //            }
        //            EthernetDevice.GetIVOpticalModuleSerialNumber();
        //            Thread.Sleep(500);
        //            while (true)
        //            {
        //                IVOpticalModuleSerialNumber = EthernetController.IVOpticalModuleSerialNumber[IVChannels.ChannelC];
        //                if (string.IsNullOrEmpty(IVOpticalModuleSerialNumber))
        //                {
        //                    time++;
        //                    Thread.Sleep(1000);
        //                }
        //                else
        //                {
        //                    time = 0;
        //                    break;
        //                }
        //                if (time == 5)//5秒未获取到指定值
        //                {
        //                    time = 0;
        //                    break;
        //                }
        //            }
        //            EthernetDevice.GetLaserSystemVersions();
        //            Thread.Sleep(500);
        //            while (true)
        //            {

        //                LaserSystemVersions = EthernetController.LaserBoardFirmwareVersionNumber[LaserChannels.ChannelC];
        //                if (string.IsNullOrEmpty(LaserSystemVersions))
        //                {
        //                    time++;
        //                    Thread.Sleep(1000);
        //                }
        //                else
        //                {
        //                    time = 0;
        //                    break;
        //                }
        //                if (time == 5)//5秒未获取到指定值
        //                {
        //                    time = 0;
        //                    break;
        //                }
        //            }
        //            EthernetDevice.GetLaserErrorCode();
        //            Thread.Sleep(500);
        //            while (true)
        //            {
        //                LaserErrorCode = EthernetController.LaserErrorCode[LaserChannels.ChannelC];
        //                if (string.IsNullOrEmpty(LaserErrorCode))
        //                {
        //                    time++;
        //                    Thread.Sleep(1000);
        //                }
        //                else
        //                {
        //                    time = 0;
        //                    break;
        //                }
        //                if (time == 5)//5秒未获取到指定值
        //                {
        //                    time = 0;
        //                    break;
        //                }
        //            }
        //            EthernetDevice.GetLaserOpticalModuleSerialNumber();
        //            Thread.Sleep(500);
        //            while (true)
        //            {
        //                LaserOpticalModuleSerialNumber = EthernetController.LaserOpticalModuleSerialNumber[LaserChannels.ChannelC];
        //                if (string.IsNullOrEmpty(LaserOpticalModuleSerialNumber))
        //                {
        //                    time++;
        //                    Thread.Sleep(1000);
        //                }
        //                else
        //                {
        //                    time = 0;
        //                    break;
        //                }
        //                if (time == 5)//5秒未获取到指定值
        //                {
        //                    time = 0;
        //                    break;
        //                }
        //            }
        //            SensorML1 = EthernetController.IvSensorTypes[IVChannels.ChannelC];//C代表L通道     Enumeration C represents an L channel 
        //            EthernetDevice.GetSystemVersions();
        //            Thread.Sleep(500);
        //            EthernetDevice.GetAllIvModulesInfo();
        //            Thread.Sleep(500);
        //            while (true)
        //            {
        //                CurrentSensorNoValue = EthernetController.IvSensorSerialNumbers[IVChannels.ChannelC].ToString("X8");
        //                if (string.IsNullOrEmpty(CurrentSensorNoValue))
        //                {
        //                    time++;
        //                    Thread.Sleep(1000);
        //                }
        //                else
        //                {
        //                    time = 0;
        //                    break;
        //                }
        //                if (time == 5)//5秒未获取到指定值
        //                {
        //                    time = 0;
        //                    break;
        //                }
        //            }
        //            EthernetDevice.GetAllLaserModulseInfo();
        //            Thread.Sleep(500);
        //            while (true)
        //            {
        //                LaserWaveLength = EthernetController.LaserWaveLengths[LaserChannels.ChannelC].ToString();
        //                if (string.IsNullOrEmpty(LaserWaveLength))
        //                {
        //                    time++;
        //                    Thread.Sleep(1000);
        //                }
        //                else
        //                {
        //                    time = 0;
        //                    break;
        //                }
        //                if (time == 5)//5秒未获取到指定值
        //                {
        //                    time = 0;
        //                    break;
        //                }
        //            }
        //            Thread.Sleep(500);
        //            while (true)
        //            {
        //                LaserNumberNo = EthernetController.LaserSerialNumbers[LaserChannels.ChannelC].ToString("X8");
        //                if (string.IsNullOrEmpty(LaserNumberNo))
        //                {
        //                    time++;
        //                    Thread.Sleep(1000);
        //                }
        //                else
        //                {
        //                    time = 0;
        //                    break;
        //                }
        //                if (time == 5)//5秒未获取到指定值
        //                {
        //                    time = 0;
        //                    break;
        //                }
        //            }
        //            EthernetDevice.GetAllTECControlTTemperatures();
        //            Thread.Sleep(500);
        //            while (true)
        //            {
        //                TECCtrlTemp = EthernetController.TECControlTemperature[LaserChannels.ChannelC];
        //                if (TECCtrlTemp <= 0)
        //                {
        //                    TECCtrlTemp = 0;
        //                    time++;
        //                    Thread.Sleep(1000);
        //                }
        //                else
        //                {
        //                    time = 0;
        //                    break;
        //                }
        //                if (time == 5)//5秒未获取到指定值
        //                {
        //                    time = 0;
        //                    break;
        //                }
        //            }
        //            EthernetDevice.GetAllTECMaximumCoolingCurrentValue();
        //            Thread.Sleep(500);
        //            while (true)
        //            {
        //                CurrentTECMaximumCoolingCurrentValue = EthernetController.TECMaximumCurrent[LaserChannels.ChannelC];
        //                if (CurrentTECMaximumCoolingCurrentValue <=0)
        //                {
        //                    CurrentTECMaximumCoolingCurrentValue = 0;
        //                    time++;
        //                    Thread.Sleep(1000);
        //                }
        //                else
        //                {
        //                    time = 0;
        //                    break;
        //                }
        //                if (time == 5)//5秒未获取到指定值
        //                {
        //                    time = 0;
        //                    break;
        //                }
        //            }

        //            EthernetDevice.GetAllCurrentTECRefrigerationControlParameterKp();
        //            Thread.Sleep(500);
        //            while (true)
        //            {

        //                CurrentTECRefrigerationControlParameterKpValue = EthernetController.TECRefrigerationControlParameterKp[LaserChannels.ChannelC];
        //                if (CurrentTECRefrigerationControlParameterKpValue <= 0)
        //                {
        //                    CurrentTECRefrigerationControlParameterKpValue = 0;
        //                    time++;
        //                    Thread.Sleep(1000);
        //                }
        //                else
        //                {
        //                    time = 0;
        //                    break;
        //                }
        //                if (time == 5)//5秒未获取到指定值
        //                {
        //                    time = 0;
        //                    break;
        //                }
        //            }
        //            EthernetDevice.GetAllCurrentTECRefrigerationControlParameterKi();
        //            Thread.Sleep(500);
        //            while (true)
        //            {
        //                CurrentTECRefrigerationControlParameterKiValue = EthernetController.TECRefrigerationControlParameterKi[LaserChannels.ChannelC];
        //                if (CurrentTECRefrigerationControlParameterKiValue <= 0)
        //                {
        //                    CurrentTECRefrigerationControlParameterKiValue = 0;
        //                    time++;
        //                    Thread.Sleep(1000);
        //                }
        //                else
        //                {
        //                    time = 0;
        //                    break;
        //                }
        //                if (time == 5)//5秒未获取到指定值
        //                {
        //                    time = 0;
        //                    break;
        //                }
        //            }
        //            EthernetDevice.GetAllCurrentTECRefrigerationControlParameterKd();
        //            Thread.Sleep(500);
        //            while (true)
        //            {
        //                CurrentTECRefrigerationControlParameterKdValue = EthernetController.TECRefrigerationControlParameterKd[LaserChannels.ChannelC];
        //                if (CurrentTECRefrigerationControlParameterKdValue <= 0)
        //                {
        //                    CurrentTECRefrigerationControlParameterKdValue = 0;
        //                    time++;
        //                    Thread.Sleep(1000);
        //                }
        //                else
        //                {
        //                    time = 0;
        //                    break;
        //                }
        //                if (time == 5)//5秒未获取到指定值
        //                {
        //                    time = 0;
        //                    break;
        //                }
        //            }

        //            if (LaserWaveLength == "532")
        //            {
        //                EthernetDevice.GetAllOpticalPowerGreaterThan15mWKp();
        //                Thread.Sleep(500);
        //                while (true)
        //                {
        //                    CurrentPowerClosedloopControlParameterKpValue = EthernetController.AllOpticalPowerGreaterThan15mWKp[LaserChannels.ChannelC];
        //                    if (CurrentPowerClosedloopControlParameterKpValue <= 0)
        //                    {
        //                        CurrentPowerClosedloopControlParameterKpValue = 0;
        //                        time++;
        //                        Thread.Sleep(1000);
        //                    }
        //                    else
        //                    {
        //                        time = 0;
        //                        break;
        //                    }
        //                    if (time == 5)//5秒未获取到指定值
        //                    {
        //                        time = 0;
        //                        break;
        //                    }
        //                }
        //                EthernetDevice.GetAllOpticalPowerGreaterThan15mWKi();
        //                Thread.Sleep(500);
        //                while (true)
        //                {
        //                    CurrentPowerClosedloopControlParameterKiValue = EthernetController.AllOpticalPowerGreaterThan15mWKi[LaserChannels.ChannelC];
        //                    if (CurrentPowerClosedloopControlParameterKiValue <= 0)
        //                    {
        //                        CurrentPowerClosedloopControlParameterKiValue = 0;
        //                        time++;
        //                        Thread.Sleep(1000);
        //                    }
        //                    else
        //                    {
        //                        time = 0;
        //                        break;
        //                    }
        //                    if (time == 5)//5秒未获取到指定值
        //                    {
        //                        time = 0;
        //                        break;
        //                    }
        //                }
        //                Workspace.This.EthernetController.GetAllOpticalPowerGreaterThan15mWKd();
        //                Thread.Sleep(500);
        //                while (true)
        //                {
        //                    CurrentPowerClosedloopControlParameterKdValue = EthernetController.AllOpticalPowerGreaterThan15mWKd[LaserChannels.ChannelC];
        //                    if (CurrentPowerClosedloopControlParameterKdValue <= 0)
        //                    {
        //                        CurrentPowerClosedloopControlParameterKdValue = 0;
        //                        time++;
        //                        Thread.Sleep(1000);
        //                    }
        //                    else
        //                    {
        //                        time = 0;
        //                        break;
        //                    }
        //                    if (time == 5)//5秒未获取到指定值
        //                    {
        //                        time = 0;
        //                        break;
        //                    }
        //                }
        //            }

        //            if (SensorML1 == IvSensorType.APD)
        //            {


        //                EthernetDevice.GetAllTempSenser1282AD();
        //                Thread.Sleep(500);
        //                while (true)
        //                {
        //                    CurrentTempSenser1282ADValue = EthernetController.TempSenser1282AD[IVChannels.ChannelC];
        //                    if (CurrentTempSenser1282ADValue <= 0)
        //                    {
        //                        CurrentTempSenser1282ADValue = 0;
        //                        time++;
        //                        Thread.Sleep(1000);
        //                    }
        //                    else
        //                    {
        //                        time = 0;
        //                        break;
        //                    }
        //                    if (time == 5)//5秒未获取到指定值
        //                    {
        //                        time = 0;
        //                        break;
        //                    }
        //                }


        //                EthernetDevice.GetAllTempSenser6459AD();
        //                Thread.Sleep(500);
        //                while (true)
        //                {

        //                    CurrentTempSenser6459ADValue = EthernetController.TempSenser6459AD[IVChannels.ChannelC];
        //                    if (CurrentTempSenser6459ADValue <= 0)
        //                    {
        //                        CurrentTempSenser6459ADValue = 0;
        //                        time++;
        //                        Thread.Sleep(1000);
        //                    }
        //                    else
        //                    {
        //                        time = 0;
        //                        break;
        //                    }
        //                    if (time == 5)//5秒未获取到指定值
        //                    {
        //                        time = 0;
        //                        break;
        //                    }
        //                }

        //                EthernetDevice.GetAllLightIntensityCalibrationTemperature();
        //                Thread.Sleep(500);
        //                while (true)
        //                {
        //                    CurrentLightIntensityCalibrationTemperatureValue = EthernetController.LightIntensityCalibrationTemperature[IVChannels.ChannelC];
        //                    if (CurrentLightIntensityCalibrationTemperatureValue <= 0)
        //                    {
        //                        CurrentLightIntensityCalibrationTemperatureValue = 0;
        //                        time++;
        //                        Thread.Sleep(1000);
        //                    }
        //                    else
        //                    {
        //                        time = 0;
        //                        break;
        //                    }
        //                    if (time == 5)//5秒未获取到指定值
        //                    {
        //                        time = 0;
        //                        break;
        //                    }
        //                }


        //                EthernetDevice.GetAPDGainCalVol(LaserChannels.ChannelC, 400);//APD增益校准电压
        //                Thread.Sleep(500);
        //                while (true)
        //                {
        //                    CurrentGain400CalVolValue = EthernetController.APDGainCalVol[IVChannels.ChannelC];
        //                    if (CurrentGain400CalVolValue <= 0)
        //                    {
        //                        CurrentGain400CalVolValue = 0;
        //                        time++;
        //                        Thread.Sleep(1000);
        //                    }
        //                    else
        //                    {
        //                        time = 0;
        //                        break;
        //                    }
        //                    if (time == 5)//5秒未获取到指定值
        //                    {
        //                        time = 0;
        //                        break;
        //                    }
        //                }


        //                EthernetDevice.GetAPDGainCalVol(LaserChannels.ChannelC, 500);//APD增益校准电压
        //                Thread.Sleep(500);


        //                while (true)
        //                {
        //                    CurrentGain500CalVolValue = EthernetController.APDGainCalVol[IVChannels.ChannelC];
        //                    if (CurrentGain500CalVolValue <= 0)
        //                    {
        //                        CurrentGain500CalVolValue = 0;
        //                        time++;
        //                        Thread.Sleep(1000);
        //                    }
        //                    else
        //                    {
        //                        time = 0;
        //                        break;
        //                    }
        //                    if (time == 5)//5秒未获取到指定值
        //                    {
        //                        time = 0;
        //                        break;
        //                    }
        //                }



        //                int a = 0;
        //                for (int i = 0; i < 6; i++)
        //                {
        //                    a++;
        //                    int Gain = a * 50;
        //                    EthernetDevice.GetAPDGainCalVol(LaserChannels.ChannelC, Gain);//APD增益校准电压
        //                    Thread.Sleep(500);
        //                    switch (Gain)
        //                    {
        //                        case 50:
        //                            while (true)
        //                            {
        //                                CurrentGain50CalVolValue = EthernetController.APDGainCalVol[IVChannels.ChannelC];
        //                                if (CurrentGain50CalVolValue <= 0)
        //                                {
        //                                    CurrentGain50CalVolValue = 0;
        //                                    time++;
        //                                    Thread.Sleep(1000);
        //                                }
        //                                else
        //                                {
        //                                    time = 0;
        //                                    break;
        //                                }
        //                                if (time == 5)//5秒未获取到指定值
        //                                {
        //                                    time = 0;
        //                                    break;
        //                                }
        //                            }
        //                            break;
        //                        case 100:
        //                            while (true)
        //                            {
        //                                CurrentGain100CalVolValue = EthernetController.APDGainCalVol[IVChannels.ChannelC];
        //                                if (CurrentGain100CalVolValue <= 0)
        //                                {
        //                                    CurrentGain100CalVolValue = 0;
        //                                    time++;
        //                                    Thread.Sleep(1000);
        //                                }
        //                                else
        //                                {
        //                                    time = 0;
        //                                    break;
        //                                }
        //                                if (time == 5)//5秒未获取到指定值
        //                                {
        //                                    time = 0;
        //                                    break;
        //                                }
        //                            }
        //                            break;
        //                        case 150:
        //                            while (true)
        //                            {
        //                                CurrentGain150CalVolValue = EthernetController.APDGainCalVol[IVChannels.ChannelC];
        //                                if (CurrentGain150CalVolValue <= 0)
        //                                {
        //                                    CurrentGain150CalVolValue = 0;
        //                                    time++;
        //                                    Thread.Sleep(1000);
        //                                }
        //                                else
        //                                {
        //                                    time = 0;
        //                                    break;
        //                                }
        //                                if (time == 5)//5秒未获取到指定值
        //                                {
        //                                    time = 0;
        //                                    break;
        //                                }
        //                            }
        //                            break;
        //                        case 200:
        //                            while (true)
        //                            {
        //                                CurrentGain200CalVolValue = EthernetController.APDGainCalVol[IVChannels.ChannelC];
        //                                if (CurrentGain200CalVolValue <= 0)
        //                                {
        //                                    CurrentGain200CalVolValue = 0;
        //                                    time++;
        //                                    Thread.Sleep(1000);
        //                                }
        //                                else
        //                                {
        //                                    time = 0;
        //                                    break;
        //                                }
        //                                if (time == 5)//5秒未获取到指定值
        //                                {
        //                                    time = 0;
        //                                    break;
        //                                }
        //                            }
        //                            break;
        //                        case 250:
        //                            while (true)
        //                            {
        //                                CurrentGain250CalVolValue = EthernetController.APDGainCalVol[IVChannels.ChannelC];
        //                                if (CurrentGain250CalVolValue <= 0)
        //                                {
        //                                    CurrentGain250CalVolValue = 0;
        //                                    time++;
        //                                    Thread.Sleep(1000);
        //                                }
        //                                else
        //                                {
        //                                    time = 0;
        //                                    break;
        //                                }
        //                                if (time == 5)//5秒未获取到指定值
        //                                {
        //                                    time = 0;
        //                                    break;
        //                                }
        //                            }
        //                            break;
        //                        case 300:
        //                            while (true)
        //                            {
        //                                CurrentGain300CalVolValue = EthernetController.APDGainCalVol[IVChannels.ChannelC];
        //                                if (CurrentGain300CalVolValue <= 0)
        //                                {
        //                                    CurrentGain300CalVolValue = 0;
        //                                    time++;
        //                                    Thread.Sleep(1000);
        //                                }
        //                                else
        //                                {
        //                                    time = 0;
        //                                    break;
        //                                }
        //                                if (time == 5)//5秒未获取到指定值
        //                                {
        //                                    time = 0;
        //                                    break;
        //                                }
        //                            }
        //                            break;

        //                    }
        //                }

        //            }
        //            if (SensorML1 == IvSensorType.PMT)
        //            {
        //                EthernetDevice.GetAllPMTCompensationCoefficient();
        //                Thread.Sleep(500);

        //                while (true)
        //                {

        //                    CurrentPMTCompensationCoefficientValue = EthernetController.PMTCompensationCoefficient[IVChannels.ChannelC];
        //                    if (CurrentPMTCompensationCoefficientValue <= 0)
        //                    {
        //                        CurrentPMTCompensationCoefficientValue = 0;
        //                        time++;
        //                        Thread.Sleep(1000);
        //                    }
        //                    else
        //                    {
        //                        time = 0;
        //                        break;
        //                    }
        //                    if (time == 5)//5秒未获取到指定值
        //                    {
        //                        time = 0;
        //                        break;
        //                    }
        //                }
        //            }



        //            for (int i = 0; i < LaserPowerModule.Count; i++)
        //            {
        //                EthernetDevice.GetLaserValueCurrent(LaserChannels.ChannelC, LaserPowerModule[i].Value);//激光校准电流
        //                Thread.Sleep(500);
        //                EthernetDevice.GetOpticalPowerControlvoltaget(LaserChannels.ChannelC, LaserPowerModule[i].Value);//光电二极管电压
        //                Thread.Sleep(500);

        //                switch (LaserPowerModule[i].Value)
        //                {
        //                    case 5:
        //                        if (LaserWaveLength != "532")
        //                        {
        //                            while (true)
        //                            {
        //                                CurrentLaserCorrespondingCurrent5ValueValue = EthernetDevice.AllIntensity[LaserChannels.ChannelC];
        //                                if (CurrentLaserCorrespondingCurrent5ValueValue <= 0)
        //                                {
        //                                    CurrentLaserCorrespondingCurrent5ValueValue = 0;
        //                                    time++;
        //                                    Thread.Sleep(1000);
        //                                }
        //                                else
        //                                {
        //                                    CurrentLaserCorrespondingCurrent5ValueValue = EthernetDevice.AllIntensity[LaserChannels.ChannelC] * 0.01;
        //                                    time = 0;
        //                                    break;
        //                                }
        //                                if (time == 5)//5秒未获取到指定值
        //                                {
        //                                    time = 0;
        //                                    break;
        //                                }
        //                            }
        //                        }

        //                        if (LaserWaveLength == "532")
        //                        {
        //                            while (true)
        //                            {
        //                                CurrentPhotodiodeVoltageCorrespondingTo5LaserPowerValue = EthernetDevice.AllOpticalPowerControlvoltage[LaserChannels.ChannelC];
        //                                if (CurrentPhotodiodeVoltageCorrespondingTo5LaserPowerValue <= 0)
        //                                {
        //                                    CurrentPhotodiodeVoltageCorrespondingTo5LaserPowerValue = 0;
        //                                    time++;
        //                                    Thread.Sleep(1000);
        //                                }
        //                                else
        //                                {
        //                                    time = 0;
        //                                    break;
        //                                }
        //                                if (time == 5)//5秒未获取到指定值
        //                                {
        //                                    time = 0;
        //                                    break;
        //                                }
        //                            }
        //                        }
        //                        break;
        //                    case 10:
        //                        if (LaserWaveLength != "532")
        //                        {
        //                            while (true)
        //                            {
        //                                CurrentLaserCorrespondingCurrent10ValueValue = EthernetDevice.AllIntensity[LaserChannels.ChannelC];
        //                                if (CurrentLaserCorrespondingCurrent10ValueValue <= 0)
        //                                {
        //                                    CurrentLaserCorrespondingCurrent10ValueValue = 0;
        //                                    time++;
        //                                    Thread.Sleep(1000);
        //                                }
        //                                else
        //                                {
        //                                    CurrentLaserCorrespondingCurrent10ValueValue = EthernetDevice.AllIntensity[LaserChannels.ChannelC] * 0.01;
        //                                    time = 0;
        //                                    break;
        //                                }
        //                                if (time == 5)//5秒未获取到指定值
        //                                {
        //                                    time = 0;
        //                                    break;
        //                                }
        //                            }
        //                        }
        //                        if (LaserWaveLength == "532")
        //                        {
        //                            while (true)
        //                            {
        //                                CurrentPhotodiodeVoltageCorrespondingTo10LaserPowerValue = EthernetDevice.AllOpticalPowerControlvoltage[LaserChannels.ChannelC];
        //                                if (CurrentPhotodiodeVoltageCorrespondingTo10LaserPowerValue <= 0)
        //                                {
        //                                    CurrentPhotodiodeVoltageCorrespondingTo10LaserPowerValue = 0;
        //                                    time++;
        //                                    Thread.Sleep(1000);
        //                                }
        //                                else
        //                                {
        //                                    time = 0;
        //                                    break;
        //                                }
        //                                if (time == 5)//5秒未获取到指定值
        //                                {
        //                                    time = 0;
        //                                    break;
        //                                }
        //                            }
        //                        }
        //                        break;
        //                    case 15:
        //                        if (LaserWaveLength != "532")
        //                        {
        //                            while (true)
        //                            {
        //                                CurrentLaserCorrespondingCurrent15ValueValue = EthernetDevice.AllIntensity[LaserChannels.ChannelC];
        //                                if (CurrentLaserCorrespondingCurrent15ValueValue <= 0)
        //                                {
        //                                    CurrentLaserCorrespondingCurrent15ValueValue = 0;
        //                                    time++;
        //                                    Thread.Sleep(1000);
        //                                }
        //                                else
        //                                {
        //                                    CurrentLaserCorrespondingCurrent15ValueValue = EthernetDevice.AllIntensity[LaserChannels.ChannelC] * 0.01;
        //                                    time = 0;
        //                                    break;
        //                                }
        //                                if (time == 5)//5秒未获取到指定值
        //                                {
        //                                    time = 0;
        //                                    break;
        //                                }
        //                            }
        //                        }
        //                        if (LaserWaveLength == "532")
        //                        {
        //                            while (true)
        //                            {
        //                                CurrentPhotodiodeVoltageCorrespondingTo15LaserPowerValue = EthernetDevice.AllOpticalPowerControlvoltage[LaserChannels.ChannelC];
        //                                if (CurrentPhotodiodeVoltageCorrespondingTo15LaserPowerValue <= 0)
        //                                {
        //                                    CurrentPhotodiodeVoltageCorrespondingTo15LaserPowerValue = 0;
        //                                    time++;
        //                                    Thread.Sleep(1000);
        //                                }
        //                                else
        //                                {
        //                                    time = 0;
        //                                    break;
        //                                }
        //                                if (time == 5)//5秒未获取到指定值
        //                                {
        //                                    time = 0;
        //                                    break;
        //                                }
        //                            }
        //                        }
        //                        break;
        //                    case 20:
        //                        if (LaserWaveLength != "532")
        //                        {
        //                            while (true)
        //                            {
        //                                CurrentLaserCorrespondingCurrent20ValueValue = EthernetDevice.AllIntensity[LaserChannels.ChannelC];
        //                                if (CurrentLaserCorrespondingCurrent20ValueValue <= 0)
        //                                {
        //                                    CurrentLaserCorrespondingCurrent20ValueValue = 0;
        //                                    time++;
        //                                    Thread.Sleep(1000);
        //                                }
        //                                else
        //                                {
        //                                    CurrentLaserCorrespondingCurrent20ValueValue = EthernetDevice.AllIntensity[LaserChannels.ChannelC] * 0.01;
        //                                    time = 0;
        //                                    break;
        //                                }
        //                                if (time == 5)//5秒未获取到指定值
        //                                {
        //                                    time = 0;
        //                                    break;
        //                                }
        //                            }
        //                        }
        //                        if (LaserWaveLength == "532")
        //                        {
        //                            while (true)
        //                            {
        //                                CurrentPhotodiodeVoltageCorrespondingTo20LaserPowerValue = EthernetDevice.AllOpticalPowerControlvoltage[LaserChannels.ChannelC];
        //                                if (CurrentPhotodiodeVoltageCorrespondingTo20LaserPowerValue <= 0)
        //                                {
        //                                    CurrentPhotodiodeVoltageCorrespondingTo20LaserPowerValue = 0;
        //                                    time++;
        //                                    Thread.Sleep(1000);
        //                                }
        //                                else
        //                                {
        //                                    time = 0;
        //                                    break;
        //                                }
        //                                if (time == 5)//5秒未获取到指定值
        //                                {
        //                                    time = 0;
        //                                    break;
        //                                }
        //                            }
        //                        }
        //                        break;
        //                    case 25:
        //                        if (LaserWaveLength != "532")
        //                        {
        //                            while (true)
        //                            {
        //                                CurrentLaserCorrespondingCurrent25ValueValue = EthernetDevice.AllIntensity[LaserChannels.ChannelC];
        //                                if (CurrentLaserCorrespondingCurrent25ValueValue <= 0)
        //                                {
        //                                    CurrentLaserCorrespondingCurrent25ValueValue = 0;
        //                                    time++;
        //                                    Thread.Sleep(1000);
        //                                }
        //                                else
        //                                {
        //                                    CurrentLaserCorrespondingCurrent25ValueValue = EthernetDevice.AllIntensity[LaserChannels.ChannelC] * 0.01;
        //                                    time = 0;
        //                                    break;
        //                                }
        //                                if (time == 5)//5秒未获取到指定值
        //                                {
        //                                    time = 0;
        //                                    break;
        //                                }
        //                            }
        //                        }
        //                        if (LaserWaveLength == "532")
        //                        {
        //                            while (true)
        //                            {
        //                                CurrentPhotodiodeVoltageCorrespondingTo25LaserPowerValue = EthernetDevice.AllOpticalPowerControlvoltage[LaserChannels.ChannelC];
        //                                if (CurrentPhotodiodeVoltageCorrespondingTo25LaserPowerValue <= 0)
        //                                {
        //                                    CurrentPhotodiodeVoltageCorrespondingTo25LaserPowerValue = 0;
        //                                    time++;
        //                                    Thread.Sleep(1000);
        //                                }
        //                                else
        //                                {
        //                                    time = 0;
        //                                    break;
        //                                }
        //                                if (time == 5)//5秒未获取到指定值
        //                                {
        //                                    time = 0;
        //                                    break;
        //                                }
        //                            }
        //                        }
        //                        break;
        //                    case 30:
        //                        if (LaserWaveLength != "532")
        //                        {
        //                            while (true)
        //                            {
        //                                CurrentLaserCorrespondingCurrent30ValueValue = EthernetDevice.AllIntensity[LaserChannels.ChannelC];
        //                                if (CurrentLaserCorrespondingCurrent30ValueValue <= 0)
        //                                {
        //                                    CurrentLaserCorrespondingCurrent30ValueValue = 0;
        //                                    time++;
        //                                    Thread.Sleep(1000);
        //                                }
        //                                else
        //                                {
        //                                    CurrentLaserCorrespondingCurrent30ValueValue = EthernetDevice.AllIntensity[LaserChannels.ChannelC] * 0.01;
        //                                    time = 0;
        //                                    break;
        //                                }
        //                                if (time == 5)//5秒未获取到指定值
        //                                {
        //                                    time = 0;
        //                                    break;
        //                                }
        //                            }
        //                        }
        //                        if (LaserWaveLength == "532")
        //                        {
        //                            while (true)
        //                            {
        //                                CurrentPhotodiodeVoltageCorrespondingTo30LaserPowerValue = EthernetDevice.AllOpticalPowerControlvoltage[LaserChannels.ChannelC];
        //                                if (CurrentPhotodiodeVoltageCorrespondingTo30LaserPowerValue <= 0)
        //                                {
        //                                    CurrentPhotodiodeVoltageCorrespondingTo30LaserPowerValue = 0;
        //                                    time++;
        //                                    Thread.Sleep(1000);
        //                                }
        //                                else
        //                                {
        //                                    time = 0;
        //                                    break;
        //                                }
        //                                if (time == 5)//5秒未获取到指定值
        //                                {
        //                                    time = 0;
        //                                    break;
        //                                }
        //                            }
        //                        }
        //                        break;
        //                    case 35:
        //                        if (LaserWaveLength != "532")
        //                        {
        //                            while (true)
        //                            {
        //                                CurrentLaserCorrespondingCurrent35ValueValue = EthernetDevice.AllIntensity[LaserChannels.ChannelC];
        //                                if (CurrentLaserCorrespondingCurrent35ValueValue <= 0)
        //                                {
        //                                    CurrentLaserCorrespondingCurrent35ValueValue = 0;
        //                                    time++;
        //                                    Thread.Sleep(1000);
        //                                }
        //                                else
        //                                {
        //                                    CurrentLaserCorrespondingCurrent35ValueValue = EthernetDevice.AllIntensity[LaserChannels.ChannelC] * 0.01;
        //                                    time = 0;
        //                                    break;
        //                                }
        //                                if (time == 5)//5秒未获取到指定值
        //                                {
        //                                    time = 0;
        //                                    break;
        //                                }
        //                            }
        //                        }
        //                        if (LaserWaveLength == "532")
        //                        {
        //                            while (true)
        //                            {
        //                                CurrentPhotodiodeVoltageCorrespondingTo35LaserPowerValue = EthernetDevice.AllOpticalPowerControlvoltage[LaserChannels.ChannelC];
        //                                if (CurrentPhotodiodeVoltageCorrespondingTo35LaserPowerValue <= 0)
        //                                {
        //                                    CurrentPhotodiodeVoltageCorrespondingTo35LaserPowerValue = 0;
        //                                    time++;
        //                                    Thread.Sleep(1000);
        //                                }
        //                                else
        //                                {
        //                                    time = 0;
        //                                    break;
        //                                }
        //                                if (time == 5)//5秒未获取到指定值
        //                                {
        //                                    time = 0;
        //                                    break;
        //                                }
        //                            }
        //                        }
        //                        break;
        //                    case 40:
        //                        if (LaserWaveLength != "532")
        //                        {
        //                            while (true)
        //                            {
        //                                CurrentLaserCorrespondingCurrent40ValueValue = EthernetDevice.AllIntensity[LaserChannels.ChannelC];
        //                                if (CurrentLaserCorrespondingCurrent40ValueValue <= 0)
        //                                {
        //                                    CurrentLaserCorrespondingCurrent40ValueValue = 0;
        //                                    time++;
        //                                    Thread.Sleep(1000);
        //                                }
        //                                else
        //                                {
        //                                    CurrentLaserCorrespondingCurrent40ValueValue = EthernetDevice.AllIntensity[LaserChannels.ChannelC] * 0.01;
        //                                    time = 0;
        //                                    break;
        //                                }
        //                                if (time == 5)//5秒未获取到指定值
        //                                {
        //                                    time = 0;
        //                                    break;
        //                                }
        //                            }
        //                        }
        //                        // _LightModeSettingsPort.CurrentPhotodiodeVoltageCorrespondingTo40LaserPowerValue = (int)EthernetDevice.AllRadioDiodeVoltage[LaserChannels.ChannelC];
        //                        break;
        //                    case 45:
        //                        if (LaserWaveLength != "532")
        //                        {
        //                            while (true)
        //                            {
        //                                CurrentLaserCorrespondingCurrent45ValueValue = EthernetDevice.AllIntensity[LaserChannels.ChannelC];
        //                                if (CurrentLaserCorrespondingCurrent45ValueValue <= 0)
        //                                {
        //                                    CurrentLaserCorrespondingCurrent45ValueValue = 0;
        //                                    time++;
        //                                    Thread.Sleep(1000);
        //                                }
        //                                else
        //                                {
        //                                    CurrentLaserCorrespondingCurrent45ValueValue = EthernetDevice.AllIntensity[LaserChannels.ChannelC] * 0.01;
        //                                    time = 0;
        //                                    break;
        //                                }
        //                                if (time == 5)//5秒未获取到指定值
        //                                {
        //                                    time = 0;
        //                                    break;
        //                                }
        //                            }
        //                        }
        //                        // _LightModeSettingsPort.CurrentPhotodiodeVoltageCorrespondingTo45LaserPowerValue = (int)EthernetDevice.AllRadioDiodeVoltage[LaserChannels.ChannelC];
        //                        break;
        //                    case 50:
        //                        if (LaserWaveLength != "532")
        //                        {
        //                            while (true)
        //                            {
        //                                CurrentLaserCorrespondingCurrent50ValueValue = EthernetDevice.AllIntensity[LaserChannels.ChannelC];
        //                                if (CurrentLaserCorrespondingCurrent50ValueValue <= 0)
        //                                {
        //                                    CurrentLaserCorrespondingCurrent50ValueValue = 0;
        //                                    time++;
        //                                    Thread.Sleep(1000);
        //                                }
        //                                else
        //                                {
        //                                    CurrentLaserCorrespondingCurrent50ValueValue = EthernetDevice.AllIntensity[LaserChannels.ChannelC] * 0.01;
        //                                    time = 0;
        //                                    break;
        //                                }
        //                                if (time == 5)//5秒未获取到指定值
        //                                {
        //                                    time = 0;
        //                                    break;
        //                                }
        //                            }
        //                        }
        //                        // _LightModeSettingsPort.CurrentPhotodiodeVoltageCorrespondingTo45LaserPowerValue = (int)EthernetDevice.AllRadioDiodeVoltage[LaserChannels.ChannelC];
        //                        break;
        //                }
        //            }





        //        }



        //        string __CurrentLaserWavaValue = string.Format("激光器波长（单位nm）: {0}", LaserWaveLength);
        //        string __GetCurrentLaserNumberValue = string.Format("激光器编号: {0}", LaserNumberNo);
        //        string __GetCurrentTecTempValue = string.Format("TEC设置温度（单位0.1℃）: {0}", TECCtrlTemp);
        //        string __GetCurrentTECMaximumCoolingCurrentValue = string.Format("TEC最大制冷电流（单位mA）: {0}", CurrentTECMaximumCoolingCurrentValue);
        //        string __GetCurrentTEC_KpValue = string.Format("TEC-Kp: {0}", CurrentTECRefrigerationControlParameterKpValue);
        //        string __GetCurrentTEC_KiValue = string.Format("TEC-Ki: {0}", CurrentTECRefrigerationControlParameterKiValue);
        //        string __GetCurrentTEC_KdValue = string.Format("TEC-Kd: {0}", CurrentTECRefrigerationControlParameterKdValue);
        //        string __GetCurrentLaster_KpValue = string.Format("Laster-Kp: {0}", CurrentPowerClosedloopControlParameterKpValue);
        //        string __GetCurrentLaster_KiValue = string.Format("Laster-Ki: {0}", CurrentPowerClosedloopControlParameterKiValue);
        //        string __GetCurrentLaster_KdValue = string.Format("Laster-Kd: {0}", CurrentPowerClosedloopControlParameterKdValue);


        //        string __CurrentPhotodiodeVoltageCorrespondingTo5LaserPowerValue = string.Format("光电二极管电压 5mW_V（单位0.0001V）: {0}", CurrentPhotodiodeVoltageCorrespondingTo5LaserPowerValue);
        //        string __CurrentPhotodiodeVoltageCorrespondingTo10LaserPowerValue = string.Format("光电二极管电压 10mW_V（单位0.0001V）: {0}", CurrentPhotodiodeVoltageCorrespondingTo10LaserPowerValue);
        //        string __CurrentPhotodiodeVoltageCorrespondingTo15LaserPowerValue = string.Format("光电二极管电压 15mW_V（单位0.0001V）: {0}", CurrentPhotodiodeVoltageCorrespondingTo15LaserPowerValue);
        //        string __CurrentPhotodiodeVoltageCorrespondingTo20LaserPowerValue = string.Format("光电二极管电压 20mW_V（单位0.0001V）: {0}", CurrentPhotodiodeVoltageCorrespondingTo20LaserPowerValue);
        //        string __CurrentPhotodiodeVoltageCorrespondingTo25LaserPowerValue = string.Format("光电二极管电压 25mW_V（单位0.0001V）: {0}", CurrentPhotodiodeVoltageCorrespondingTo25LaserPowerValue);
        //        string __CurrentPhotodiodeVoltageCorrespondingTo30LaserPowerValue = string.Format("光电二极管电压 30mW_V（单位0.0001V）: {0}", CurrentPhotodiodeVoltageCorrespondingTo30LaserPowerValue);
        //        string __CurrentPhotodiodeVoltageCorrespondingTo35LaserPowerValue = string.Format("光电二极管电压 35mW_V（单位0.0001V）: {0}", CurrentPhotodiodeVoltageCorrespondingTo35LaserPowerValue);


        //        string __CurrentLaserCorrespondingCurrent5ValueValue = string.Format("激光功率对应的电流值 5mW-电流值（单位0.01mA）: {0}", CurrentLaserCorrespondingCurrent5ValueValue);
        //        string __CurrentLaserCorrespondingCurrent10ValueValue = string.Format("激光功率对应的电流值 10mW-电流值（单位0.01mA）: {0}", CurrentLaserCorrespondingCurrent10ValueValue);
        //        string __CurrentLaserCorrespondingCurrent15ValueValue = string.Format("激光功率对应的电流值 15mW-电流值（单位0.01mA）: {0}", CurrentLaserCorrespondingCurrent15ValueValue);
        //        string __CurrentLaserCorrespondingCurrent20ValueValue = string.Format("激光功率对应的电流值 20mW-电流值（单位0.01mA）: {0}", CurrentLaserCorrespondingCurrent20ValueValue);
        //        string __CurrentLaserCorrespondingCurrent25ValueValue = string.Format("激光功率对应的电流值 25mW-电流值（单位0.01mA）: {0}", CurrentLaserCorrespondingCurrent25ValueValue);
        //        string __CurrentLaserCorrespondingCurrent30ValueValue = string.Format("激光功率对应的电流值 30mW-电流值（单位0.01mA）: {0}", CurrentLaserCorrespondingCurrent30ValueValue);

        //        string __GetCurrentSensorNoValue = string.Format("IV板传感器编号: {0}", CurrentSensorNoValue);
        //        string __GetCurrentTempSenser1282ADValue = string.Format("温度校准值1: {0}", CurrentTempSenser1282ADValue);
        //        string __GetCurrentTempSenser6459ADValue = string.Format("温度校准值2: {0}", CurrentTempSenser6459ADValue);
        //        string __GetCurrentLightIntensityCalibrationTemperatureValue = string.Format("APD标定温度，单位（0.01℃）: {0}", CurrentLightIntensityCalibrationTemperatureValue);

        //        string __GetCurrentGain50CalVolValue = string.Format("APD-GAIN50(0.01V）: {0}", CurrentGain50CalVolValue);
        //        string __GetCurrentGain100CalVolValue = string.Format("APD-GAIN100(0.01V）: {0}", CurrentGain100CalVolValue);
        //        string __GetCurrentGain150CalVolValue = string.Format("APD-GAIN150(0.01V）: {0}", CurrentGain150CalVolValue);
        //        string __GetCurrentGain200CalVolValue = string.Format("APD-GAIN200(0.01V）: {0}", CurrentGain200CalVolValue);
        //        string __GetCurrentGain250CalVolValue = string.Format("APD-GAIN250(0.01V）: {0}", CurrentGain250CalVolValue);
        //        string __GetCurrentGain300CalVolValue = string.Format("APD-GAIN300(0.01V）: {0}", CurrentGain300CalVolValue);
        //        string __GetCurrentGain400CalVolValue = string.Format("APD-GAIN400(0.01V）: {0}", CurrentGain400CalVolValue);
        //        string __GetCurrentGain500CalVolValue = string.Format("APD-GAIN500(0.01V）: {0}", CurrentGain500CalVolValue);

        //        string __GetCurrentPMTCompensationCoefficientValue = string.Format("PMT补偿系数（单位0.1mV）: {0}", CurrentPMTCompensationCoefficientValue);

        //        string __IVSystemVersions = string.Format("IV软件版本号: {0}", IVSystemVersions);
        //        string __IVSystemErrorCode = string.Format("IV错误码: {0}", IVSystemErrorCode);
        //        string __IVOpticalModuleSerialNumber = string.Format("IV光学模块序列号: {0}", IVOpticalModuleSerialNumber);
        //        string __LaserSystemVersions = string.Format("Laser软件版本号: {0}", LaserSystemVersions);
        //        string __LaserErrorCode = string.Format("Laser错误码: {0}", LaserErrorCode);
        //        string __LaserOpticalModuleSerialNumber = string.Format("Laser光学模块序列号: {0}", LaserOpticalModuleSerialNumber);
        //        //开始创建PDF文档
        //        Document document = new Document();//创建实例化对象Document
        //        //生成pdf路径，创建文件流
        //        PdfWriter.GetInstance(document, new FileStream(filePath, FileMode.Create));
        //        document.Open();//打开当前Document
        //        BaseFont baseFont = BaseFont.CreateFont(@"c:\windows\fonts\SIMSUN.TTC,1", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);//设置字体 
        //        iTextSharp.text.Font font = new iTextSharp.text.Font(baseFont, 20);
        //        document.Add(new Paragraph(__CurrentLaserWavaValue, font));//为当前Document添加内容
        //        document.Add(new Paragraph(__GetCurrentLaserNumberValue, font));//为当前Document添加内容
        //        document.Add(new Paragraph(__GetCurrentTecTempValue, font));//为当前Document添加内容
        //        document.Add(new Paragraph(__GetCurrentTECMaximumCoolingCurrentValue, font));//为当前Document添加内容
        //        document.Add(new Paragraph(__GetCurrentTEC_KpValue, font));//为当前Document添加内容
        //        document.Add(new Paragraph(__GetCurrentTEC_KiValue, font));//为当前Document添加内容
        //        document.Add(new Paragraph(__GetCurrentTEC_KdValue, font));//为当前Document添加内容

        //        if (LaserWaveLength == "532")
        //        {
        //            document.Add(new Paragraph(__GetCurrentLaster_KpValue, font));//为当前Document添加内容
        //            document.Add(new Paragraph(__GetCurrentLaster_KiValue, font));//为当前Document添加内容
        //            document.Add(new Paragraph(__GetCurrentLaster_KdValue, font));//为当前Document添加内容
        //            document.Add(new Paragraph(__CurrentPhotodiodeVoltageCorrespondingTo5LaserPowerValue, font));//为当前Document添加内容
        //            document.Add(new Paragraph(__CurrentPhotodiodeVoltageCorrespondingTo10LaserPowerValue, font));//为当前Document添加内容
        //            document.Add(new Paragraph(__CurrentPhotodiodeVoltageCorrespondingTo15LaserPowerValue, font));//为当前Document添加内容
        //            document.Add(new Paragraph(__CurrentPhotodiodeVoltageCorrespondingTo20LaserPowerValue, font));//为当前Document添加内容
        //            document.Add(new Paragraph(__CurrentPhotodiodeVoltageCorrespondingTo25LaserPowerValue, font));//为当前Document添加内容
        //            document.Add(new Paragraph(__CurrentPhotodiodeVoltageCorrespondingTo30LaserPowerValue, font));//为当前Document添加内容
        //            document.Add(new Paragraph(__CurrentPhotodiodeVoltageCorrespondingTo35LaserPowerValue, font));//为当前Document添加内容
        //        }
        //        if (LaserWaveLength != "532")
        //        {
        //            document.Add(new Paragraph(__CurrentLaserCorrespondingCurrent5ValueValue, font));//为当前Document添加内容
        //            document.Add(new Paragraph(__CurrentLaserCorrespondingCurrent10ValueValue, font));//为当前Document添加内容
        //            document.Add(new Paragraph(__CurrentLaserCorrespondingCurrent15ValueValue, font));//为当前Document添加内容
        //            document.Add(new Paragraph(__CurrentLaserCorrespondingCurrent20ValueValue, font));//为当前Document添加内容
        //            document.Add(new Paragraph(__CurrentLaserCorrespondingCurrent25ValueValue, font));//为当前Document添加内容
        //            document.Add(new Paragraph(__CurrentLaserCorrespondingCurrent30ValueValue, font));//为当前Document添加内容
        //            string __CurrentLaserCorrespondingCurrent35ValueValue = string.Format("激光功率对应的电流值 35mW-电流值（单位0.01mA）: {0}", CurrentLaserCorrespondingCurrent35ValueValue);
        //            string __CurrentLaserCorrespondingCurrent40ValueValue = string.Format("激光功率对应的电流值 40mW-电流值（单位0.01mA）: {0}", CurrentLaserCorrespondingCurrent40ValueValue);
        //            string __CurrentLaserCorrespondingCurrent45ValueValue = string.Format("激光功率对应的电流值 45mW-电流值（单位0.01mA）: {0}", CurrentLaserCorrespondingCurrent45ValueValue);
        //            string __CurrentLaserCorrespondingCurrent50ValueValue = string.Format("激光功率对应的电流值 50mW-电流值（单位0.01mA）: {0}", CurrentLaserCorrespondingCurrent50ValueValue);
        //            document.Add(new Paragraph(__CurrentLaserCorrespondingCurrent35ValueValue, font));//为当前Document添加内容
        //            document.Add(new Paragraph(__CurrentLaserCorrespondingCurrent40ValueValue, font));//为当前Document添加内容
        //            document.Add(new Paragraph(__CurrentLaserCorrespondingCurrent45ValueValue, font));//为当前Document添加内容
        //            document.Add(new Paragraph(__CurrentLaserCorrespondingCurrent50ValueValue, font));//为当前Document添加内容
        //        }
        //        document.Add(new Paragraph(__GetCurrentSensorNoValue, font));//为当前Document添加内容
        //        if (SensorML1 == IvSensorType.APD)
        //        {
        //            document.Add(new Paragraph(__GetCurrentTempSenser1282ADValue, font));//为当前Document添加内容
        //            document.Add(new Paragraph(__GetCurrentTempSenser6459ADValue, font));//为当前Document添加内容
        //            document.Add(new Paragraph(__GetCurrentLightIntensityCalibrationTemperatureValue, font));//为当前Document添加内容
        //            document.Add(new Paragraph(__GetCurrentGain50CalVolValue, font));//为当前Document添加内容
        //            document.Add(new Paragraph(__GetCurrentGain100CalVolValue, font));//为当前Document添加内容
        //            document.Add(new Paragraph(__GetCurrentGain150CalVolValue, font));//为当前Document添加内容
        //            document.Add(new Paragraph(__GetCurrentGain200CalVolValue, font));//为当前Document添加内容
        //            document.Add(new Paragraph(__GetCurrentGain250CalVolValue, font));//为当前Document添加内容
        //            document.Add(new Paragraph(__GetCurrentGain300CalVolValue, font));//为当前Document添加内容
        //            document.Add(new Paragraph(__GetCurrentGain400CalVolValue, font));//为当前Document添加内容
        //            document.Add(new Paragraph(__GetCurrentGain500CalVolValue, font));//为当前Document添加内容
        //        }
        //        else
        //        {
        //            document.Add(new Paragraph(__GetCurrentPMTCompensationCoefficientValue, font));//为当前Document添加内容
        //        }

        //        document.Add(new Paragraph(__IVSystemVersions, font));//为当前Document添加内容
        //        document.Add(new Paragraph(__IVSystemErrorCode, font));//为当前Document添加内容
        //        document.Add(new Paragraph(__IVOpticalModuleSerialNumber, font));//为当前Document添加内容
        //        document.Add(new Paragraph(__LaserSystemVersions, font));//为当前Document添加内容
        //        document.Add(new Paragraph(__LaserErrorCode, font));//为当前Document添加内容
        //        document.Add(new Paragraph(__LaserOpticalModuleSerialNumber, font));//为当前Document添加内容

        //        document.Close();//结束位置
        //    }
        //    MessageBox.Show("保存成功");
        //}
        public bool CanExecuteGenerateReportCommand(object parameter)
        {
            return true;
        }

        #endregion
        #endregion
    }
}
