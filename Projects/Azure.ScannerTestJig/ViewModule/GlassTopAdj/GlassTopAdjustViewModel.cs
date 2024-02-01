using Azure.WPF.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using Azure.ImagingSystem;
using System.Windows;
using System.Collections.ObjectModel;
using Azure.Configuration.Settings;
using System.Threading;
using Azure.CommandLib;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using System.IO;
using Azure.EthernetCommLib;

namespace Azure.ScannerTestJig.ViewModule.GlassTopAdj
{
    class GlassTopAdjustViewModel : ViewModelBase
    {
        #region privert
        private SelectLaserChannel _SelectedLaser = null;
        private ObservableCollection<APDGainType> _APDGainOptions = null;
        private ObservableCollection<APDPgaType> _PGAOptionsModule = null;
        private ObservableCollection<SelectLaserChannel> _LaserChannelOptionsModule = null;
        private EthernetController _EthernetController;
        private TestJigScanProcessing _ScanningProcess;
        private RelayCommand _ScanCommand = null;
        private RelayCommand _StopScanCommand = null;
        private bool _IsScannerMode = true;
        private bool _IsStopMode = false;
        private Visibility _IsScanVisibility = Visibility.Visible;
        private Visibility _IsStopVisibility = Visibility.Hidden;
        private SelectLaserChannel _SelectLaserChannel = null;
        private APDPgaType _SelectedPgaMModuleR2 = null;
        private APDGainType _SelectedGainComModuleR2 = null;
        private bool _SelectVisChannel = true;
        private Visibility _GainComVisbleR2 = Visibility.Visible;
        private Visibility _GainTxtVisbleR2 = Visibility.Visible;
        private string _WL1 = "NA";
        private string _WR1 = "NA";
        private string _WR2 = "NA";
        int _TxtApdGainR2 = 500;
        private double _LaserCPower = 0;
        private double _ChMaxPeak = 0;
        private double _ChMinPeak = 0;
        private double _ChResultPeak = 0;
        private bool _IsLaserR2Selected = false;
        private bool _IsNullEnabledR2 = false;
        private IvSensorType _SensorML1 = IvSensorType.NA;
        private IvSensorType _SensorMR1 = IvSensorType.NA;
        private IvSensorType _SensorMR2 = IvSensorType.NA;
        private string _SensorSNL1 = "L1";
        private string _SensorSNR1 = "R1";
        private string _SensorSNR2 = "R2";
        private string _LaserSNL1 = "L";
        private string _LaserSNR1 = "R1";
        private string _LaserSNR2 = "R2";
        private bool _IsReadOnly = true;
        private Thread _GlassTopAdjThread = null;
        private double _XMotorSubdivision = 0;
        private double _YMotorSubdivision = 0;
        private double _ZMotorSubdivision = 0;
        private int _XMaxValue = 0;
        private int _YMaxValue = 0;
        private int _ZMaxValue = 0;
        private double _ScanDeltaZ = 0;
        private System.Drawing.Point _TopLeft = new System.Drawing.Point();
        private System.Drawing.Point _TopRight = new System.Drawing.Point();
        private System.Drawing.Point _LowerRight = new System.Drawing.Point();
        private System.Drawing.Point _LowerLeft = new System.Drawing.Point();
        public Thread tdHome = null;
        #endregion
        #region Constructors...

        public GlassTopAdjustViewModel(EthernetController ethernetController)
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
        #endregion
        public void InitIVControls()
        {
            _APDGainOptions = SettingsManager.ConfigSettings.APDGains;
            _PGAOptionsModule = SettingsManager.ConfigSettings.APDPgas;
            _LaserChannelOptionsModule= SettingsManager.ConfigSettings.LaserChannel;
            RaisePropertyChanged("PGAOptionsModule");
            RaisePropertyChanged("GainComModule");
            RaisePropertyChanged("LaserChannelOptionsModule");
            if (_PGAOptionsModule != null && _PGAOptionsModule.Count >= 3)
            {
                SelectedMModuleR2 = _PGAOptionsModule[3];
            }
            if (_APDGainOptions != null && _APDGainOptions.Count >= 6)
            {
                SelectedGainComModuleR2 = _APDGainOptions[5];    // select the 6th item
                GainTxtModuleR2 = 4000;
            }
            if (_LaserChannelOptionsModule != null && _LaserChannelOptionsModule.Count >= 3)
            {
                SelectLaserChannel = _LaserChannelOptionsModule[2];
            }
            VisbleAndEnable();
            TurnOffAllLasers();//lasers off
            _XMotorSubdivision = SettingsManager.ConfigSettings.XMotorSubdivision;
            _YMotorSubdivision = SettingsManager.ConfigSettings.YMotorSubdivision;
            _ZMotorSubdivision = SettingsManager.ConfigSettings.ZMotorSubdivision;
            _XMaxValue = SettingsManager.ConfigSettings.XMaxValue;
            _YMaxValue = SettingsManager.ConfigSettings.YMaxValue;
            _ZMaxValue = SettingsManager.ConfigSettings.ZMaxValue;
            ScanDeltaZ = (int)Math.Round(_ZMaxValue / _ZMotorSubdivision, 2);
            EnabledButton = false;
            Workspace.This.GlassTopPosVM.IsPosASelected = false;
            Workspace.This.GlassTopPosVM.IsPosBSelected = false;
            Workspace.This.GlassTopPosVM.IsPosCSelected = false;
            Workspace.This.GlassTopPosVM.IsPosDSelected = false;
            Workspace.This.GlassTopPosVM.IsPosCenterSelected = false;
            Workspace.This.GlassTopPosVM.IsPosCenterDownSelected = false;
            Workspace.This.GlassTopPosVM.IsPosCenterTopSelected = false;
            tdHome = new Thread(HomeMotor);
            tdHome.IsBackground = true;
            tdHome.Start();
        }
        /// <summary>
        /// visble and Enable
        /// </summary>
        private void VisbleAndEnable()
        {
            if (!Workspace.This.IsSelectGlassCM)
            {
                Workspace.This.GlassTopPosVM.CenterVis = Visibility.Hidden;
                Workspace.This.ZScanChartFocusVM.CenterVis = Visibility.Hidden;
            }
            else
            {
                Workspace.This.GlassTopPosVM.CenterVis = Visibility.Visible;
                Workspace.This.ZScanChartFocusVM.CenterVis = Visibility.Visible;
            }
            if (SelectLaserChannel.DisplayName == "L" && WR2 != "NA")
            {
                if (SensorML1 == IvSensorType.APD)
                {
                    _IsNullEnabledR2 = true;
                    _GainComVisbleR2 = Visibility.Visible;
                    _GainTxtVisbleR2 = Visibility.Hidden;
                    RaisePropertyChanged("IsNullEnabledR2");
                    RaisePropertyChanged("GainComVisFlagR2");
                    RaisePropertyChanged("GainVisTxtFlagR2");
                }
                else if (SensorML1 == IvSensorType.PMT)
                {
                    _IsNullEnabledR2 = true;
                    _GainComVisbleR2 = Visibility.Hidden;
                    _GainTxtVisbleR2 = Visibility.Visible;
                    RaisePropertyChanged("IsNullEnabledR2");
                    RaisePropertyChanged("GainComVisFlagR2");
                    RaisePropertyChanged("GainVisTxtFlagR2");
                }
                else
                {
                    _IsNullEnabledR2 = false;
                    RaisePropertyChanged("IsNullEnabledR2");
                }
            }


            if (SelectLaserChannel.DisplayName == "R1" && WR2 != "NA")
            {
                if (SensorMR1 == IvSensorType.APD)
                {
                    _IsNullEnabledR2 = true;
                    _GainComVisbleR2 = Visibility.Visible;
                    _GainTxtVisbleR2 = Visibility.Hidden;
                    RaisePropertyChanged("IsNullEnabledR2");
                    RaisePropertyChanged("GainComVisFlagR2");
                    RaisePropertyChanged("GainVisTxtFlagR2");
                }
                else if (SensorMR1 == IvSensorType.PMT)
                {
                    _IsNullEnabledR2 = true;
                    _GainComVisbleR2 = Visibility.Hidden;
                    _GainTxtVisbleR2 = Visibility.Visible;
                    RaisePropertyChanged("IsNullEnabledR2");
                    RaisePropertyChanged("GainComVisFlagR2");
                    RaisePropertyChanged("GainVisTxtFlagR2");
                }
                else
                {
                    _IsNullEnabledR2 = false;
                    RaisePropertyChanged("IsNullEnabledR2");
                }
            }

            if (SelectLaserChannel.DisplayName == "R2" && WR2 != "NA")
            {
                if (SensorMR2 == IvSensorType.APD)
                {
                    _IsNullEnabledR2 = true;
                    _GainComVisbleR2 = Visibility.Visible;
                    _GainTxtVisbleR2 = Visibility.Hidden;
                    RaisePropertyChanged("IsNullEnabledR2");
                    RaisePropertyChanged("GainComVisFlagR2");
                    RaisePropertyChanged("GainVisTxtFlagR2");
                }
                else if (SensorMR2 == IvSensorType.PMT)
                {
                    _IsNullEnabledR2 = true;
                    _GainComVisbleR2 = Visibility.Hidden;
                    _GainTxtVisbleR2 = Visibility.Visible;
                    RaisePropertyChanged("IsNullEnabledR2");
                    RaisePropertyChanged("GainComVisFlagR2");
                    RaisePropertyChanged("GainVisTxtFlagR2");
                }
                else
                {
                    _IsNullEnabledR2 = false;
                    RaisePropertyChanged("IsNullEnabledR2");
                }
            }
        }

        public void TurnOffAllLasers()
        {
            IsLaserR2Selected = false;
            System.Threading.Thread.Sleep(200);
        }

        #region Public Pro
      
        #region IsControlReadOnly
        public bool IsReadOnly
        {
            get
            {
                return _IsReadOnly;
            }
            set
            {
                if (_IsReadOnly != value)
                {
                    _IsReadOnly = value;
                    RaisePropertyChanged("IsReadOnly");
                }
            }
        }

        #endregion

        #region Sensor
        public IvSensorType SensorML1
        {
            get
            {

                return _SensorML1;
            }
            set
            {
                if (_SensorML1 != value)
                {
                    if ((int)value == Workspace.This.Uint8Code)
                    {
                        value = IvSensorType.NA;
                    }
                    _SensorML1 = value;
                }
            }
        }
        public IvSensorType SensorMR1
        {
            get { return _SensorMR1; }
            set
            {
                if (_SensorMR1 != value)
                {
                    if ((int)value == Workspace.This.Uint8Code)
                    {
                        value = IvSensorType.NA;
                    }
                    _SensorMR1 = value;
                }
            }
        }
        public IvSensorType SensorMR2
        {
            get { return _SensorMR2; }
            set
            {
                if (_SensorMR2 != value)
                {
                    if ((int)value == Workspace.This.Uint8Code)
                    {
                        value = IvSensorType.NA;
                    }
                    _SensorMR2 = value;
                }
            }
        }
        #endregion

        #region SensorSN
        public string SensorSNL1
        {
            get { return _SensorSNL1; }
            set
            {

                if (_SensorSNL1 != value)
                {
                    if (value == Workspace.This.StrEmptyCode1)
                    {
                        value = "NaN";
                    }
                    _SensorSNL1 = value;
                }
            }
        }
        public string SensorSNR1
        {
            get { return _SensorSNR1; }
            set
            {

                if (_SensorSNR1 != value)
                {
                    if (value == Workspace.This.StrEmptyCode1)
                    {
                        value = "NaN";
                    }
                    _SensorSNR1 = value;
                }
            }
        }
        public string SensorSNR2
        {
            get { return _SensorSNR2; }
            set
            {

                if (_SensorSNR2 != value)
                {
                    if (value == Workspace.This.StrEmptyCode1)
                    {
                        value = "NaN";
                    }
                    _SensorSNR2 = value;
                }
            }
        }
        #endregion

        #region LaserSNL
        public string LaserSNL1
        {
            get { return _LaserSNL1; }
            set
            {

                if (_LaserSNL1 != value)
                {
                    if (value == Workspace.This.StrEmptyCode1)
                    {
                        value = "NaN";
                    }
                    _LaserSNL1 = value;
                }
            }
        }
        public string LaserSNR1
        {
            get { return _LaserSNR1; }
            set
            {

                if (_LaserSNR1 != value)
                {
                    if (value == Workspace.This.StrEmptyCode1)
                    {
                        value = "NaN";
                    }
                    _LaserSNR1 = value;
                }
            }
        }
        public string LaserSNR2
        {
            get { return _LaserSNR2; }
            set
            {

                if (_LaserSNR2 != value)
                {
                    if (value == Workspace.This.StrEmptyCode1)
                    {
                        value = "NaN";
                    }
                    _LaserSNR2 = value;
                }
            }
        }
        #endregion

        #region WL
        public string WL1
        {
            get { return _WL1; }
            set
            {

                if (_WL1 != value)
                {
                    if (value == "4880")
                    {
                        _WL1 = "488-YFP";
                    }
                    else if (value == "5320")
                    {
                        _WL1 = "532-Propidium";
                    }
                    else
                    {
                        _WL1 = value;
                    }
                    RaisePropertyChanged("WL1");
                }
            }
        }
        public string WR1
        {
            get { return _WR1; }
            set
            {

                if (_WR1 != value)
                {
                    if (value == "4880")
                    {
                        _WR1 = "488-YFP";
                    }
                    else if (value == "5320")
                    {
                        _WR1 = "532-Propidium";
                    }
                    else
                    {
                        _WR1 = value;
                    }
                    RaisePropertyChanged("WR1");
                }
            }
        }
        public string WR2
        {
            get { return _WR2; }
            set
            {

                if (_WR2 != value)
                {
                    if (value == "4880")
                    {
                        _WR2 = "488-YFP";
                    }
                    else if (value == "5320")
                    {
                        _WR2 = "532-Propidium";
                    }
                    else
                    {
                        _WR2 = value;
                    }
                    RaisePropertyChanged("WR2");
                }
            }
        }
        #endregion

        #region APD PGA
        public ObservableCollection<APDPgaType> PGAOptionsModule
        {
            get { return _PGAOptionsModule; }
        }
        public APDPgaType SelectedMModuleR2
        {
            get { return _SelectedPgaMModuleR2; }
            set
            {
                if (_SelectedPgaMModuleR2 != value)
                {
                    _SelectedPgaMModuleR2 = value;
                    RaisePropertyChanged("SelectedMModuleR2");
                    if (SelectLaserChannel != null)
                    {
                        if (SelectLaserChannel.DisplayName == "R2")
                        {
                            if (SensorMR2 != IvSensorType.NA)
                            {
                                EthernetDevice.SetIvPga(IVChannels.ChannelB, (ushort)_SelectedPgaMModuleR2.Value);
                            }
                        }
                        if (SelectLaserChannel.DisplayName == "R1")
                        {
                            if (SensorMR1 != IvSensorType.NA)
                            {
                                EthernetDevice.SetIvPga(IVChannels.ChannelA, (ushort)_SelectedPgaMModuleR2.Value);
                            }
                        }
                        if (SelectLaserChannel.DisplayName == "L")
                        {
                            if (SensorML1 != IvSensorType.NA)
                            {
                                EthernetDevice.SetIvPga(IVChannels.ChannelC, (ushort)_SelectedPgaMModuleR2.Value);
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Gain APD
        public ObservableCollection<APDGainType> GainComModule
        {
            get { return _APDGainOptions; }
        }
        public APDGainType SelectedGainComModuleR2
        {
            get { return _SelectedGainComModuleR2; }
            set
            {
                if (_SelectedGainComModuleR2 != value)
                {
                    _SelectedGainComModuleR2 = value;
                    RaisePropertyChanged("SelectedGainComModuleR2");
                    if (SelectLaserChannel != null)
                    {
                        if (SelectLaserChannel.DisplayName == "R2")
                        {
                            if (SensorMR2 == IvSensorType.APD)
                            {
                                EthernetDevice.SetIvApdGain(IVChannels.ChannelB, (ushort)_SelectedGainComModuleR2.Value);
                            }
                        }
                        if (SelectLaserChannel.DisplayName == "R1")
                        {
                            if (SensorMR1 == IvSensorType.APD)
                            {
                                EthernetDevice.SetIvApdGain(IVChannels.ChannelA, (ushort)_SelectedGainComModuleR2.Value);
                            }
                        }
                        if (SelectLaserChannel.DisplayName == "L")
                        {
                            if (SensorML1 == IvSensorType.APD)
                            {
                                EthernetDevice.SetIvApdGain(IVChannels.ChannelC, (ushort)_SelectedGainComModuleR2.Value);
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Gain PMT
        public int GainTxtModuleR2
        {
            get { return _TxtApdGainR2; }
            set
            {
                if (_TxtApdGainR2 != value)
                {
                    _TxtApdGainR2 = value;
                    RaisePropertyChanged("GainTxtModuleR2");
                    if (SelectLaserChannel != null)
                    {
                        if (SelectLaserChannel.DisplayName == "R2")
                        {
                            if (SensorMR2 == IvSensorType.PMT)
                            {
                                EthernetDevice.SetIvPmtGain(IVChannels.ChannelB, (ushort)_TxtApdGainR2);
                            }

                        }
                        if (SelectLaserChannel.DisplayName == "R1")
                        {
                            if (SensorMR1 == IvSensorType.PMT)
                            {
                                EthernetDevice.SetIvPmtGain(IVChannels.ChannelA, (ushort)_TxtApdGainR2);
                            }

                        }
                        if (SelectLaserChannel.DisplayName == "L")
                        {
                            if (SensorML1 == IvSensorType.PMT)
                            {
                                EthernetDevice.SetIvPmtGain(IVChannels.ChannelC, (ushort)_TxtApdGainR2);
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region  Gain Com visble
        public Visibility GainComVisFlagR2
        {
            get
            {

                return _GainComVisbleR2;

            }

        }
        #endregion

        #region  Gain Txt Visble
        public Visibility GainVisTxtFlagR2
        {
            get
            {
                return _GainTxtVisbleR2;
            }

        }
        #endregion

        #region Power

        public double LaserCPower
        {
            get { return _LaserCPower; }
            set
            {
                if (_LaserCPower != value)
                {

                    //if ((value < 0) || (value > 0 && value < 5))
                    //{
                    //    string caption = "Laser C...";
                    //    string message = LaserSetErrorMessage;
                    //    MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                    //}
                    //else
                    //{
                    _LaserCPower = value;
                    //LaserCIntensity = Workspace.This.ApdVM.APDTransfer.LaserPowerToIntensity(ScannerDataStruct.APDLaserChannelType.C, _LaserCPower);
                    RaisePropertyChanged("LaserCPower");
                    //}
                }
            }
        }
        #endregion

        #region Calculate difference
        public double ChMaxPeak
        {
            get
            {
                return _ChMaxPeak;
            }
            set
            {
                if (_ChMaxPeak != value)
                {
                    _ChMaxPeak = value;
                    RaisePropertyChanged("ChMaxPeak");
                }
            }
        }
        public double ChMinPeak
        {
            get
            {
                return _ChMinPeak;
            }
            set
            {
                if (_ChMinPeak != value)
                {
                    _ChMinPeak = value;
                    RaisePropertyChanged("ChMinPeak");
                }
            }
        }
        public double ChResultPeak
        {
            get
            {
                return _ChResultPeak;
            }
            set
            {
                if (_ChResultPeak != value)
                {
                    _ChResultPeak = value;
                    RaisePropertyChanged("ChResultPeak");
                }
            }
        }
        #endregion

        #region IsLaserSelected

        public bool IsLaserR2Selected
        {
            get
            {
                return _IsLaserR2Selected;
            }
            set
            {
                if (_IsLaserR2Selected != value)
                {
                    _IsLaserR2Selected = value;
                    RaisePropertyChanged("IsLaserR2Selected");
                    if (SelectLaserChannel != null)
                    {
                        if (SelectLaserChannel.DisplayName == "R2")
                        {
                            if (value == true)
                            {
                                EthernetDevice.SetLaserPower(LaserChannels.ChannelB, LaserCPower);
                            }
                            else
                            {
                                EthernetDevice.SetLaserPower(LaserChannels.ChannelB, 0);
                            }
                        }
                        if (SelectLaserChannel.DisplayName == "R1")
                        {
                            if (value == true)
                            {
                                EthernetDevice.SetLaserPower(LaserChannels.ChannelA, LaserCPower);
                            }
                            else
                            {
                                EthernetDevice.SetLaserPower(LaserChannels.ChannelA, 0);
                            }
                        }
                        if (SelectLaserChannel.DisplayName == "L")
                        {
                            if (value == true)
                            {
                                EthernetDevice.SetLaserPower(LaserChannels.ChannelC, LaserCPower);
                            }
                            else
                            {
                                EthernetDevice.SetLaserPower(LaserChannels.ChannelC, 0);
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region IsEnabled
        public bool IsNullEnabledR2
        {
            get { return _IsNullEnabledR2; }
            set
            {
                _IsNullEnabledR2 = value;
                RaisePropertyChanged("IsNullEnabledR2");
            }
        }
        #endregion

        #region IsEnabled
        private bool _EnabledButton = false;
        public bool EnabledButton
        {
            get { return _EnabledButton; }
            set
            {
                _EnabledButton = value;
                RaisePropertyChanged("EnabledButton");
            }
        }
        #endregion

        #region LaserChannel
        public bool SelectVisChannel
        {
            get { return _SelectVisChannel; }
            set
            {
                _SelectVisChannel = value;
                Workspace.This.GlassTopPosVM.SelectVisChannel = value;
                RaisePropertyChanged("SelectVisChannel");
            }
        }
        public ObservableCollection<SelectLaserChannel> LaserChannelOptionsModule
        {
            get { return _LaserChannelOptionsModule; }
        }
        public SelectLaserChannel SelectLaserChannel
        {
            get { return _SelectLaserChannel; }
            set
            {
                if (_SelectLaserChannel != value)
                {
                    _SelectLaserChannel = value;
                    RaisePropertyChanged("SelectLaserChannel");
                    if (value.DisplayName == "L")
                    {
                        WR2 = EthernetController.LaserWaveLengths[LaserChannels.ChannelC].ToString();
                        if (WR2 == "0"|| Convert.ToUInt32(WR2) ==Workspace.This.Uint16Code)
                        {
                            WR2 = "NA";
                        }
                        VisbleAndEnable();
                    }
                    if (value.DisplayName == "R1")
                    {
                        WR2 = EthernetController.LaserWaveLengths[LaserChannels.ChannelA].ToString();
                        if (WR2 == "0" || Convert.ToUInt32(WR2) == Workspace.This.Uint16Code)
                        {
                            WR2 = "NA";
                        }
                        VisbleAndEnable();
                    }
                    if (value.DisplayName == "R2")
                    {
                        WR2 = EthernetController.LaserWaveLengths[LaserChannels.ChannelB].ToString();
                        if (WR2 == "0" || Convert.ToUInt32(WR2) == Workspace.This.Uint16Code)
                        {
                            WR2 = "NA";
                        }
                        VisbleAndEnable();
                    }
                        InitConfigInfo();
                }
            }
        }
        #endregion

        void InitConfigInfo() 
        {

            #region PGA
            if (SelectLaserChannel != null)
            {
                if (SelectLaserChannel.DisplayName == "R2")
                {
                    if (SensorMR2 != IvSensorType.NA)
                    {
                        EthernetDevice.SetIvPga(IVChannels.ChannelB, (ushort)_SelectedPgaMModuleR2.Value);
                    }
                }
                if (SelectLaserChannel.DisplayName == "R1")
                {
                    if (SensorMR1 != IvSensorType.NA)
                    {
                        EthernetDevice.SetIvPga(IVChannels.ChannelA, (ushort)_SelectedPgaMModuleR2.Value);
                    }
                }
                if (SelectLaserChannel.DisplayName == "L")
                {
                    if (SensorML1 != IvSensorType.NA)
                    {
                        EthernetDevice.SetIvPga(IVChannels.ChannelC, (ushort)_SelectedPgaMModuleR2.Value);
                    }
                }
            }
            #endregion

            #region Gain APD
            if (SelectLaserChannel != null)
            {
                if (SelectLaserChannel.DisplayName == "R2")
                {
                    if (SensorMR2 == IvSensorType.APD)
                    {
                        EthernetDevice.SetIvApdGain(IVChannels.ChannelB, (ushort)_SelectedGainComModuleR2.Value);
                    }
                }
                if (SelectLaserChannel.DisplayName == "R1")
                {
                    if (SensorMR1 == IvSensorType.APD)
                    {
                        EthernetDevice.SetIvApdGain(IVChannels.ChannelA, (ushort)_SelectedGainComModuleR2.Value);
                    }
                }
                if (SelectLaserChannel.DisplayName == "L")
                {
                    if (SensorML1 == IvSensorType.APD)
                    {
                        EthernetDevice.SetIvApdGain(IVChannels.ChannelC, (ushort)_SelectedGainComModuleR2.Value);
                    }
                }
            }
            #endregion

            #region Gain PMT
            if (SelectLaserChannel != null)
            {
                if (SelectLaserChannel.DisplayName == "R2")
                {
                    if (SensorMR2 == IvSensorType.PMT)
                    {
                        EthernetDevice.SetIvPmtGain(IVChannels.ChannelB, (ushort)_TxtApdGainR2);
                    }

                }
                if (SelectLaserChannel.DisplayName == "R1")
                {
                    if (SensorMR1 == IvSensorType.PMT)
                    {
                        EthernetDevice.SetIvPmtGain(IVChannels.ChannelA, (ushort)_TxtApdGainR2);
                    }

                }
                if (SelectLaserChannel.DisplayName == "L")
                {
                    if (SensorML1 == IvSensorType.PMT)
                    {
                        EthernetDevice.SetIvPmtGain(IVChannels.ChannelC, (ushort)_TxtApdGainR2);
                    }
                }
            }
            #endregion
        }

        #endregion

        public double ScanDeltaZ
        {
            get
            {
                double dRetVal = 0.0;
                if (_ZMotorSubdivision != 0)
                {
                    dRetVal = Math.Round((double)_ScanDeltaZ / (double)_ZMotorSubdivision, 3);
                }
                return dRetVal;
            }
            set
            {
                if (((double)_ScanDeltaZ / (double)_ZMotorSubdivision) != value)
                {
                    if (value >= 0 && ((value + 0) <= ((double)_ZMaxValue / (double)_ZMotorSubdivision)))
                    {
                        _ScanDeltaZ = (int)(value * _ZMotorSubdivision);
                    }
                    else
                    {
                        _ScanDeltaZ = 0;
                        //MessageBox.Show(String.Format("The DZ should be 0=<Z0+DZ<={0}", (double)_ZMaxValue / (double)_ZMotorSubdivision), "Error");
                    }
                    RaisePropertyChanged("ScanDeltaZ");
                }
            }
        }

        public Visibility IsScanVisibility
        {
            get { return _IsScanVisibility; }
            set
            {
                if (_IsScanVisibility != value)
                {
                    _IsScanVisibility = value;
                    RaisePropertyChanged("IsScanVisibility");

                }
            }
        }

        public Visibility IsStopVisibility
        {
            get { return _IsStopVisibility; }
            set
            {
                if (_IsStopVisibility != value)
                {
                    _IsStopVisibility = value;
                    RaisePropertyChanged("IsStopVisibility");
                }
            }
        }

        public bool IsScanning
        {
            get { return _IsScannerMode; }
            set
            {
                if (_IsScannerMode != value)
                {
                    _IsScannerMode = value;
                    if (_IsScannerMode)
                    {
                        IsScanVisibility = Visibility.Visible;
                    }
                    else
                    {
                        IsScanVisibility = Visibility.Hidden;
                    }
                }
            }
        }

        public bool IsStopning
        {
            get { return _IsStopMode; }
            set
            {
                if (_IsStopMode != value)
                {
                    _IsStopMode = value;
                    if (_IsStopMode)
                    {
                        IsStopVisibility = Visibility.Visible;
                    }
                    else
                    {
                        IsStopVisibility = Visibility.Hidden;
                    }
                }
            }
        }


        #region StopScanCommand

        public ICommand StopScanCommand
        {
            get
            {
                if (_StopScanCommand == null)
                {
                    _StopScanCommand = new RelayCommand(ExecuteStopScanCommand, CanExecuteStopScanCommand);
                }

                return _StopScanCommand;
            }
        }
        public void ExecuteStopScanCommand(object parameter)
        {
            if (_ScanningProcess != null)
            {
                //Enable Motor control
                _ScanningProcess.Abort();  // Abort the scanning thread
                if (Workspace.This.MotorVM.IsNewFirmware)
                {
                    Workspace.This.MotorVM.MotionController.SetStart(EthernetCommLib.MotorTypes.X | EthernetCommLib.MotorTypes.Y | EthernetCommLib.MotorTypes.Z,
                        new bool[] { false, false, false });
                }
                IsScanning = true;
                IsStopning = false;
         
            }
            Workspace.This.GlassTopPosVM.IsPosASelected = false;
            Workspace.This.GlassTopPosVM.IsPosBSelected = false;
            Workspace.This.GlassTopPosVM.IsPosCSelected = false;
            Workspace.This.GlassTopPosVM.IsPosDSelected = false;
            Workspace.This.GlassTopPosVM.IsPosCenterSelected = false;
            Workspace.This.GlassTopPosVM.IsPosCenterDownSelected = false;
            Workspace.This.GlassTopPosVM.IsPosCenterTopSelected = false;
            SelectVisChannel = true;
            TurnOffAllLasers();
        }
        public bool CanExecuteStopScanCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region ScanCommand           
        int _scanCount = 0;
        bool isScanGlass = true;
        public ICommand ScanCommand
        {
            get
            {
                if (_ScanCommand == null)
                {
                    _ScanCommand = new RelayCommand(ExecuteScanCommand, CanExecuteScanCommand);
                }

                return _ScanCommand;
            }
        }

        public void ExecuteScanCommand(object parameter)
        {
            IsLaserR2Selected = false;
            SelectVisChannel = false;
            Workspace.This.GlassTopPosVM.SelectVisChannel = false;
            Workspace.This.GlassTopPosVM.ChLeftDownPeak = 0;
            Workspace.This.GlassTopPosVM.ChLeftTopPeak = 0;
            Workspace.This.GlassTopPosVM.ChRightTopPeak = 0;
            Workspace.This.GlassTopPosVM.ChRightDownPeak = 0;
            Workspace.This.GlassTopPosVM.ChCenterPeak = 0;
            Workspace.This.GlassTopPosVM.ChCenterDownPeak = 0;
            Workspace.This.GlassTopPosVM.ChCenterTopPeak = 0;
            Workspace.This.ZScanChartGlassVM.LeftTop = null;
            Workspace.This.ZScanChartGlassVM.LeftDown = null;
            Workspace.This.ZScanChartGlassVM.RightTop = null;
            Workspace.This.ZScanChartGlassVM.RightDown = null;
            Workspace.This.ZScanChartGlassVM.Center = null;
            Workspace.This.ZScanChartGlassVM.CenterDown = null;
            Workspace.This.ZScanChartGlassVM.CenterTop = null;
            _scanCount = 0;
            //if (WR2 == "NA")
            //{
            //    SelectVisChannel = true;
            //    MessageBox.Show("未检测到模块", "", MessageBoxButton.OK, MessageBoxImage.Error);
            //    return;
            //}
            if (Workspace.This.GlassTopPosVM.IsPosASelected == false && Workspace.This.GlassTopPosVM.IsPosBSelected == false &&
             Workspace.This.GlassTopPosVM.IsPosCSelected == false && Workspace.This.GlassTopPosVM.IsPosDSelected == false&& Workspace.This.GlassTopPosVM.IsPosCenterSelected == false&&
             Workspace.This.GlassTopPosVM.IsPosCenterDownSelected == false&& Workspace.This.GlassTopPosVM.IsPosCenterTopSelected == false)
            {
                SelectVisChannel = true;
                MessageBox.Show("请至少选择一个位置点进行扫描！");
                return;
            }
            if (Workspace.This.MotorVM.IsNewFirmware)
            {
                if (LaserCPower <= 0)
                {
                    MessageBoxResult boxResult = MessageBoxResult.None;
                    boxResult = MessageBox.Show("光功率为0是否继续！", "警告", MessageBoxButton.YesNo);
                    if (boxResult == MessageBoxResult.No)
                    {
                        SelectVisChannel = true;
                        return;
                    }
                }
                else
                {
                    IsLaserR2Selected = true;//扫描之前打开激光头
                }
                // _GlassTopAdjustProcess();
                _GlassTopAdjThread = new Thread(_GlassTopAdjustProcess);
                _GlassTopAdjThread.IsBackground = true;
                _GlassTopAdjThread.Start();
            }

            IsScanning = false;
            IsStopning = true;

        }
        void Z_HomeMotor()
        {
            bool _tempCurrent = true;
            //sif (SettingsManager.ConfigSettings.HomeMotionsAtLaunchTime)
            {
                if (!Workspace.This.MotorVM.HomeXYZmotor(0,0))
                {
                    //MessageBox.Show("无法回到Home位置，请检查连接", "", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                while (_tempCurrent)
                {
                    Thread.Sleep(500);
                    if (Workspace.This.MotorVM.MotionController.CrntState[EthernetCommLib.MotorTypes.Z].AtHome)
                    {
                        EnabledButton = true;
                        _tempCurrent = false;
                    }
                    else
                    {
                        _tempCurrent = true;

                    }
                }
            }
            //else
            //{
            //    _tempCurrent = false;
            //}


        }
        void HomeMotor()
        {
            bool _tempCurrent = true;
            //sif (SettingsManager.ConfigSettings.HomeMotionsAtLaunchTime)
            {
                if (!Workspace.This.MotorVM.HomeXYZmotor())
                {
                    //MessageBox.Show("无法回到Home位置，请检查连接", "", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                while (_tempCurrent)
                {
                    Thread.Sleep(500);
                    if (Workspace.This.MotorVM.MotionController.CrntState[EthernetCommLib.MotorTypes.X].AtHome &&
                  Workspace.This.MotorVM.MotionController.CrntState[EthernetCommLib.MotorTypes.Y].AtHome &&
                  Workspace.This.MotorVM.MotionController.CrntState[EthernetCommLib.MotorTypes.Z].AtHome)
                    {
                        EnabledButton = true;
                        _tempCurrent = false;
                    }
                    else
                    {
                        _tempCurrent = true;

                    }
                }
            }
            //else
            //{
            //    _tempCurrent = false;
            //}


        }

        private void _GlassTopAdjustProcess()
        {
            Z_HomeMotor();
            Thread.Sleep(1000);
            //HomeMotor();
            if (!Workspace.This.InitControlValue())
            {
                MessageBox.Show("连接失败！", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            TestJigScanParameterStruct scanParameter = new TestJigScanParameterStruct();  //Set scan parameter
            scanParameter.Width = 0;
            scanParameter.Height = 0;
            scanParameter.ScanDeltaX = 0;
            scanParameter.ScanDeltaY = 0;
            scanParameter.ScanDeltaZ = (int)_ScanDeltaZ;
            //scanParameter.ScanDeltaZ = (int)Math.Round(_ZMaxValue / _ZMotorSubdivision, 2); ;
            scanParameter.Res = 10;
            scanParameter.Quality = 1;
            scanParameter.DataRate = 1;
            scanParameter.LineCounts = 0;
            scanParameter.Time = 0;
            _TopLeft = SettingsManager.ConfigSettings.GlassLevelingTopLeft;
            _TopRight = SettingsManager.ConfigSettings.GlassLevelingTopRight;
            _LowerRight = SettingsManager.ConfigSettings.GlassLevelingLowerRight;
            _LowerLeft = SettingsManager.ConfigSettings.GlassLevelingLowerLeft;
            scanParameter.XMotorSubdivision = _XMotorSubdivision;
            scanParameter.YMotorSubdivision = _YMotorSubdivision;
            scanParameter.ZMotorSubdivision = _ZMotorSubdivision;
            scanParameter.XMotorSpeed = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[0].Speed * SettingsManager.ConfigSettings.XMotorSubdivision);
            scanParameter.XMotionAccVal = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[0].Accel * SettingsManager.ConfigSettings.XMotorSubdivision);
            scanParameter.XMotionDccVal = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[0].Dccel * SettingsManager.ConfigSettings.XMotorSubdivision);
            scanParameter.YMotorSpeed = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[1].Speed * SettingsManager.ConfigSettings.YMotorSubdivision);
            scanParameter.YMotionAccVal = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[1].Accel * SettingsManager.ConfigSettings.YMotorSubdivision);
            scanParameter.YMotionDccVal = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[1].Dccel * SettingsManager.ConfigSettings.YMotorSubdivision);
            scanParameter.ZMotorSpeed = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[2].Speed * SettingsManager.ConfigSettings.ZMotorSubdivision);
            scanParameter.ZMotionAccVal = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[2].Accel * SettingsManager.ConfigSettings.ZMotorSubdivision);
            scanParameter.ZMotionDccVal = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[2].Dccel * SettingsManager.ConfigSettings.ZMotorSubdivision);
            scanParameter.IsNewFirmwire = Workspace.This.MotorVM.IsNewFirmware;
            scanParameter.XmotionTurnAroundDelay = SettingsManager.ConfigSettings.XMotionTurnDelay;
            scanParameter.XMotionExtraMoveLength = (int)SettingsManager.ConfigSettings.XMotionExtraMoveLength;
            scanParameter.ZScanValueThreshold = SettingsManager.ConfigSettings.ZScanValueThreshold;
            scanParameter.ScanZ0 = 0;
            scanParameter.IsGlassScan = true;
            int _Scan0X = 0;
            if (SelectLaserChannel.DisplayName == "R1")
            {
                _Scan0X = (int)(SettingsManager.ConfigSettings.ModuleInterval * _XMotorSubdivision);
            }
            if (SelectLaserChannel.DisplayName == "L")
            {
                _Scan0X = (int)((SettingsManager.ConfigSettings.ModuleInterval * 2) * _XMotorSubdivision);
            }
            int trip = 240;
            if (Workspace.This.IsSelectGlassCM)
            {
                _TopRight.X += trip;
                _LowerRight.X += trip;
            }

            if (Workspace.This.GlassTopPosVM.IsPosCenterDownSelected)       // CenterDown
            {
                int _tempScanx0;
                _tempScanx0 = (int)((SettingsManager.ConfigSettings.CentralCoordX + 88) * _XMotorSubdivision) + _Scan0X;
                // Workspace.This.InitControlValue();
                isScanGlass = true;
                _scanCount = 6;
                scanParameter.ScanX0 = _tempScanx0;
                scanParameter.ScanY0 = (int)(_LowerRight.Y * _YMotorSubdivision);
                _ScanningProcess = new TestJigScanProcessing(Workspace.This.EthernetController, Workspace.This.MotorVM.MotionController, scanParameter, null);
                _ScanningProcess.Completed += _ImageScanCommand_Completed;
                _ScanningProcess.OnScanDataReceived += _ScanningProcess_OnScanDataReceived;
                _ScanningProcess.Start();
                IsScanning = false;
                IsStopning = true;
                while (isScanGlass)
                {
                    Thread.Sleep(1);
                }
                Thread.Sleep(1000);
            }
            
            if (Workspace.This.GlassTopPosVM.IsPosDSelected)       // Lower right
            {
                // Workspace.This.InitControlValue();
                isScanGlass = true;
                _scanCount = 4;
                scanParameter.ScanX0 = (int)(_LowerRight.X * _XMotorSubdivision) + _Scan0X - (int)(10 * _XMotorSubdivision);
                scanParameter.ScanY0 = (int)(_LowerRight.Y * _YMotorSubdivision);
                _ScanningProcess = new TestJigScanProcessing(Workspace.This.EthernetController, Workspace.This.MotorVM.MotionController, scanParameter, null);
                _ScanningProcess.Completed += _ImageScanCommand_Completed;
                _ScanningProcess.OnScanDataReceived += _ScanningProcess_OnScanDataReceived;
                _ScanningProcess.Start();
                IsScanning = false;
                IsStopning = true;
                while (isScanGlass)
                {
                    Thread.Sleep(1);
                }
                Thread.Sleep(1000);
            }


            if (Workspace.This.GlassTopPosVM.IsPosCenterSelected)       // Center
            {
                int _tempScanx0, _tempScany0;
                _tempScanx0 = (int)((SettingsManager.ConfigSettings.CentralCoordX + 88) * _XMotorSubdivision) + _Scan0X;
                _tempScany0 = (int)(SettingsManager.ConfigSettings.CentralCoordY * _YMotorSubdivision);
                // Workspace.This.InitControlValue();
                isScanGlass = true;
                _scanCount = 5;
                scanParameter.ScanX0 = _tempScanx0;
                scanParameter.ScanY0 = _tempScany0;
                _ScanningProcess = new TestJigScanProcessing(Workspace.This.EthernetController, Workspace.This.MotorVM.MotionController, scanParameter, null);
                _ScanningProcess.Completed += _ImageScanCommand_Completed;
                _ScanningProcess.OnScanDataReceived += _ScanningProcess_OnScanDataReceived;
                _ScanningProcess.Start();
                IsScanning = false;
                IsStopning = true;
                while (isScanGlass)
                {
                    Thread.Sleep(1);
                }
                Thread.Sleep(1000);
            }
            
           
            if (Workspace.This.GlassTopPosVM.IsPosCSelected)       // top right
            {
                // Workspace.This.InitControlValue();
                isScanGlass = true;
                _scanCount = 3;
                scanParameter.ScanX0 = (int)(_TopRight.X * _XMotorSubdivision) + _Scan0X - (int)(10 * _XMotorSubdivision);
                scanParameter.ScanY0 = (int)(_TopRight.Y * _YMotorSubdivision);
                _ScanningProcess = new TestJigScanProcessing(Workspace.This.EthernetController, Workspace.This.MotorVM.MotionController, scanParameter, null);
                _ScanningProcess.Completed += _ImageScanCommand_Completed;
                _ScanningProcess.OnScanDataReceived += _ScanningProcess_OnScanDataReceived;
                _ScanningProcess.Start();
                IsScanning = false;
                IsStopning = true;
                while (isScanGlass)
                {
                    Thread.Sleep(1);
                }
                Thread.Sleep(1000);
            }

            if (Workspace.This.GlassTopPosVM.IsPosCenterTopSelected)       // CenterTop
            {
                int _tempScanx0;
                _tempScanx0 = (int)((SettingsManager.ConfigSettings.CentralCoordX + 88) * _XMotorSubdivision) + _Scan0X;
                // Workspace.This.InitControlValue();
                isScanGlass = true;
                _scanCount = 7;
                scanParameter.ScanX0 = _tempScanx0;
                scanParameter.ScanY0 = (int)(_TopRight.Y * _YMotorSubdivision);
                _ScanningProcess = new TestJigScanProcessing(Workspace.This.EthernetController, Workspace.This.MotorVM.MotionController, scanParameter, null);
                _ScanningProcess.Completed += _ImageScanCommand_Completed;
                _ScanningProcess.OnScanDataReceived += _ScanningProcess_OnScanDataReceived;
                _ScanningProcess.Start();
                IsScanning = false;
                IsStopning = true;
                while (isScanGlass)
                {
                    Thread.Sleep(1);
                }
                Thread.Sleep(1000);
            }

            if (Workspace.This.GlassTopPosVM.IsPosBSelected)       // top left
            {
                // Workspace.This.InitControlValue();
                isScanGlass = true;
                _scanCount = 2;
                scanParameter.ScanX0 = (int)(_TopLeft.X * _XMotorSubdivision) + _Scan0X + (int)(10 * _XMotorSubdivision);
                scanParameter.ScanY0 = (int)(_TopLeft.Y * _YMotorSubdivision);
                _ScanningProcess = new TestJigScanProcessing(Workspace.This.EthernetController, Workspace.This.MotorVM.MotionController, scanParameter, null);
                _ScanningProcess.Completed += _ImageScanCommand_Completed;
                _ScanningProcess.OnScanDataReceived += _ScanningProcess_OnScanDataReceived;
                _ScanningProcess.Start();
                IsScanning = false;
                IsStopning = true;
                while (isScanGlass)
                {
                    Thread.Sleep(1);
                }
                Thread.Sleep(1000);
            }

            if (Workspace.This.GlassTopPosVM.IsPosASelected)       // Lower left
            {
                isScanGlass = true;
                _scanCount = 1;
                scanParameter.ScanX0 = (int)(_LowerLeft.X * _XMotorSubdivision) + _Scan0X + (int)(10 * _XMotorSubdivision);
                scanParameter.ScanY0 = (int)(_LowerLeft.Y * _YMotorSubdivision);
                _ScanningProcess = new TestJigScanProcessing(Workspace.This.EthernetController, Workspace.This.MotorVM.MotionController, scanParameter, null);
                _ScanningProcess.Completed += _ImageScanCommand_Completed;
                _ScanningProcess.OnScanDataReceived += _ScanningProcess_OnScanDataReceived;
                _ScanningProcess.Start();
                IsScanning = false;
                IsStopning = true;
                while (isScanGlass)
                {
                    Thread.Sleep(1);
                }
                Thread.Sleep(1000);
            }

            List<double> arrayPeak = new List<double>();
            if (Workspace.This.GlassTopPosVM.ChLeftDownPeak != 0)
            {
                arrayPeak.Add(Workspace.This.GlassTopPosVM.ChLeftDownPeak);
            }
            if (Workspace.This.GlassTopPosVM.ChLeftTopPeak != 0)
            {
                arrayPeak.Add(Workspace.This.GlassTopPosVM.ChLeftTopPeak);
            }
            if (Workspace.This.GlassTopPosVM.ChRightTopPeak != 0)
            {
                arrayPeak.Add(Workspace.This.GlassTopPosVM.ChRightTopPeak);
            }
            if (Workspace.This.GlassTopPosVM.ChRightDownPeak != 0)
            {
                arrayPeak.Add(Workspace.This.GlassTopPosVM.ChRightDownPeak);
            }
            if (Workspace.This.GlassTopPosVM.ChCenterPeak != 0)
            {
                arrayPeak.Add(Workspace.This.GlassTopPosVM.ChCenterPeak);
            }
            if (Workspace.This.GlassTopPosVM.ChCenterDownPeak != 0)
            {
                arrayPeak.Add(Workspace.This.GlassTopPosVM.ChCenterDownPeak);
            }
            if (Workspace.This.GlassTopPosVM.ChCenterTopPeak != 0)
            {
                arrayPeak.Add(Workspace.This.GlassTopPosVM.ChCenterTopPeak);
            }
            ChMaxPeak=arrayPeak.Max();
            ChMinPeak=arrayPeak.Min();
            ChResultPeak = ChMaxPeak - ChMinPeak;
            SelectVisChannel = true;
            IsLaserR2Selected = false;
        }
        private void _ScanningProcess_OnScanDataReceived(string dataName)
        {
            if (dataName == "GlassScan")
            {
               // IsLaserR2Selected = true;//扫描之前打开激光头
            }
        }
        private void _ImageScanCommand_Completed(CommandLib.ThreadBase sender, CommandLib.ThreadBase.ThreadExitStat exitState)
        {
            Workspace.This.Owner.Dispatcher.BeginInvoke((Action)delegate
            {
                TestJigScanProcessing scannedThread = (sender as TestJigScanProcessing);

                if (exitState == ThreadBase.ThreadExitStat.None)
                {
                    try
                    {
                        Point[] chA = new Point[scannedThread.SampleValueChannelA.Length];
                        Point[] chB = new Point[scannedThread.SampleValueChannelB.Length];
                        Point[] chC = new Point[scannedThread.SampleValueChannelC.Length];

                        double coeffX = 0;
                        double coeffChA = 1;
                        double coeffChB = 1;
                        double coeffChC = 1;
                        switch (scannedThread.ScanType)
                        {
                            case TestJigScanTypes.Static:
                                // coeffX = DataRate;
                                coeffChA = 1;
                                coeffChB = 1;
                                coeffChC = 1;
                                break;
                            case TestJigScanTypes.Vertical:
                                coeffX = 8 / SettingsManager.ConfigSettings.ZMotorSubdivision;
                                coeffChA = 1;
                                coeffChB = 1;
                                coeffChC = 1;
                                break;
                            case TestJigScanTypes.XAxis:
                                coeffX = 1;
                                coeffChA = -1 / SettingsManager.ConfigSettings.XMotorSubdivision;
                                coeffChB = 1 / SettingsManager.ConfigSettings.XEncoderSubdivision;
                                coeffChC = 1;
                                break;
                        }
                        if (scannedThread.ScanType == TestJigScanTypes.XAxis)
                        {
                            for (int i = 0; i < chA.Length; i++)
                            {
                                chA[i].X = scannedThread.SampleIndex[i] * coeffX;
                                chA[i].Y = scannedThread.SampleValueChannelA[i] * coeffChA;
                                chB[i].X = chA[i].X;
                                chB[i].Y = scannedThread.SampleValueChannelB[i] * coeffChB;
                                chC[i].X = chA[i].X;
                                chC[i].Y = scannedThread.SampleValueChannelC[i] * coeffChC;
                            }
                        }
                        else
                        {
                            int[] _tempA = MovingAverage(scannedThread.SampleValueChannelA, 5);
                            int[] _tempB = MovingAverage(scannedThread.SampleValueChannelB, 5);
                            int[] _tempC = MovingAverage(scannedThread.SampleValueChannelC, 5);
                            for (int i = 0; i < chA.Length; i++)
                            {
                                chA[i].X = scannedThread.SampleIndex[i] * coeffX;
                                chA[i].Y = _tempA[i] * coeffChA;
                                chB[i].X = chA[i].X;
                                chB[i].Y = _tempB[i] * coeffChB;
                                chC[i].X = chA[i].X;
                                chC[i].Y = _tempC[i] * coeffChC;
                            }
                            if (SelectLaserChannel.DisplayName == "R2")
                            {

                                double positionOfMinValue, valueMin;
                                ThresholdProcess(chB, out positionOfMinValue, out valueMin);
                                if (Workspace.This.GlassTopPosVM.IsPosASelected && _scanCount == 1)
                                {//左下
                                    Workspace.This.ZScanChartGlassVM.LeftDown = new EnumerableDataSource<Point>(chB);
                                    Workspace.This.ZScanChartGlassVM.LeftDown.SetXYMapping(p => p);
                                    //Workspace.This.GlassTopPosVM.ChLeftDownPeak = Math.Round(((TestJigScanProcessing)scannedThread).StaticChannelBMax.X, 3);
                                    Workspace.This.GlassTopPosVM.ChLeftDownPeak = Math.Floor(positionOfMinValue * 1000) / 1000;
                                    Workspace.This.GlassTopPosVM.IsPosASelected = false;

                                }
                                if (Workspace.This.GlassTopPosVM.IsPosBSelected && _scanCount == 2)
                                {//左上

                                    Workspace.This.ZScanChartGlassVM.LeftTop = new EnumerableDataSource<Point>(chB);
                                    Workspace.This.ZScanChartGlassVM.LeftTop.SetXYMapping(p => p);
                                    //Workspace.This.GlassTopPosVM.ChLeftTopPeak = Math.Round(((TestJigScanProcessing)scannedThread).StaticChannelBMax.X, 3);
                                    Workspace.This.GlassTopPosVM.ChLeftTopPeak = Math.Floor(positionOfMinValue * 1000) / 1000;
                                    Workspace.This.GlassTopPosVM.IsPosBSelected = false;
                                }
                                if (Workspace.This.GlassTopPosVM.IsPosCSelected && _scanCount == 3)
                                {//右上

                                    Workspace.This.ZScanChartGlassVM.RightTop = new EnumerableDataSource<Point>(chB);
                                    Workspace.This.ZScanChartGlassVM.RightTop.SetXYMapping(p => p);
                                    //Workspace.This.GlassTopPosVM.ChRightTopPeak = Math.Round(((TestJigScanProcessing)scannedThread).StaticChannelBMax.X, 3);
                                    Workspace.This.GlassTopPosVM.ChRightTopPeak = Math.Floor(positionOfMinValue * 1000) / 1000;
                                    Workspace.This.GlassTopPosVM.IsPosCSelected = false;
                                }
                                if (Workspace.This.GlassTopPosVM.IsPosDSelected && _scanCount == 4)
                                {//右下

                                    Workspace.This.ZScanChartGlassVM.RightDown = new EnumerableDataSource<Point>(chB);
                                    Workspace.This.ZScanChartGlassVM.RightDown.SetXYMapping(p => p);
                                    //Workspace.This.GlassTopPosVM.ChRightDownPeak = Math.Round(((TestJigScanProcessing)scannedThread).StaticChannelBMax.X, 3);
                                    Workspace.This.GlassTopPosVM.ChRightDownPeak = Math.Floor(positionOfMinValue * 1000) / 1000;
                                    Workspace.This.GlassTopPosVM.IsPosDSelected = false;

                                }
                                if (Workspace.This.GlassTopPosVM.IsPosCenterSelected && _scanCount == 5)
                                {//中间

                                    Workspace.This.ZScanChartGlassVM.Center = new EnumerableDataSource<Point>(chB);
                                    Workspace.This.ZScanChartGlassVM.Center.SetXYMapping(p => p);
                                    //Workspace.This.GlassTopPosVM.ChRightDownPeak = Math.Round(((TestJigScanProcessing)scannedThread).StaticChannelBMax.X, 3);
                                    Workspace.This.GlassTopPosVM.ChCenterPeak = Math.Floor(positionOfMinValue * 1000) / 1000;
                                    Workspace.This.GlassTopPosVM.IsPosCenterSelected = false;

                                }
                                if (Workspace.This.GlassTopPosVM.IsPosCenterDownSelected && _scanCount == 6)
                                {//中下

                                    Workspace.This.ZScanChartGlassVM.CenterDown = new EnumerableDataSource<Point>(chB);
                                    Workspace.This.ZScanChartGlassVM.CenterDown.SetXYMapping(p => p);
                                    //Workspace.This.GlassTopPosVM.ChRightDownPeak = Math.Round(((TestJigScanProcessing)scannedThread).StaticChannelBMax.X, 3);
                                    Workspace.This.GlassTopPosVM.ChCenterDownPeak = Math.Floor(positionOfMinValue * 1000) / 1000;
                                    Workspace.This.GlassTopPosVM.IsPosCenterDownSelected = false;

                                }
                                if (Workspace.This.GlassTopPosVM.IsPosCenterTopSelected && _scanCount == 7)
                                {//中上

                                    Workspace.This.ZScanChartGlassVM.CenterTop = new EnumerableDataSource<Point>(chB);
                                    Workspace.This.ZScanChartGlassVM.CenterTop.SetXYMapping(p => p);
                                    //Workspace.This.GlassTopPosVM.ChRightDownPeak = Math.Round(((TestJigScanProcessing)scannedThread).StaticChannelBMax.X, 3);
                                    Workspace.This.GlassTopPosVM.ChCenterTopPeak = Math.Floor(positionOfMinValue * 1000) / 1000;
                                    Workspace.This.GlassTopPosVM.IsPosCenterTopSelected = false;

                                }
                            }
                            if (SelectLaserChannel.DisplayName == "R1")
                            {
                                double positionOfMinValue, valueMin;
                                ThresholdProcess(chA, out positionOfMinValue, out valueMin);
                                if (Workspace.This.GlassTopPosVM.IsPosASelected && _scanCount == 1)
                                {//左下
                                    Workspace.This.ZScanChartGlassVM.LeftDown = new EnumerableDataSource<Point>(chA);
                                    Workspace.This.ZScanChartGlassVM.LeftDown.SetXYMapping(p => p);
                                    //Workspace.This.GlassTopPosVM.ChLeftDownPeak = Math.Round(((TestJigScanProcessing)scannedThread).StaticChannelAMax.X, 3);
                                    Workspace.This.GlassTopPosVM.ChLeftDownPeak = Math.Floor(positionOfMinValue * 1000) / 1000;
                                    Workspace.This.GlassTopPosVM.IsPosASelected = false;

                                }
                                if (Workspace.This.GlassTopPosVM.IsPosBSelected && _scanCount == 2)
                                {//左上

                                    Workspace.This.ZScanChartGlassVM.LeftTop = new EnumerableDataSource<Point>(chA);
                                    Workspace.This.ZScanChartGlassVM.LeftTop.SetXYMapping(p => p);
                                   // Workspace.This.GlassTopPosVM.ChLeftTopPeak = Math.Round(((TestJigScanProcessing)scannedThread).StaticChannelAMax.X, 3);
                                    Workspace.This.GlassTopPosVM.ChLeftTopPeak = Math.Floor(positionOfMinValue * 1000) / 1000;
                                    Workspace.This.GlassTopPosVM.IsPosBSelected = false;
                                }
                                if (Workspace.This.GlassTopPosVM.IsPosCSelected && _scanCount == 3)
                                {//右上

                                    Workspace.This.ZScanChartGlassVM.RightTop = new EnumerableDataSource<Point>(chA);
                                    Workspace.This.ZScanChartGlassVM.RightTop.SetXYMapping(p => p);
                                    //Workspace.This.GlassTopPosVM.ChRightTopPeak = Math.Round(((TestJigScanProcessing)scannedThread).StaticChannelAMax.X, 3);
                                    Workspace.This.GlassTopPosVM.ChRightTopPeak = Math.Floor(positionOfMinValue * 1000) / 1000;
                                    Workspace.This.GlassTopPosVM.IsPosCSelected = false;
                                }
                                if (Workspace.This.GlassTopPosVM.IsPosDSelected && _scanCount == 4)
                                {//右下

                                    Workspace.This.ZScanChartGlassVM.RightDown = new EnumerableDataSource<Point>(chA);
                                    Workspace.This.ZScanChartGlassVM.RightDown.SetXYMapping(p => p);
                                    //Workspace.This.GlassTopPosVM.ChRightDownPeak = Math.Round(((TestJigScanProcessing)scannedThread).StaticChannelAMax.X, 3);
                                    Workspace.This.GlassTopPosVM.ChRightDownPeak = Math.Floor(positionOfMinValue * 1000) / 1000;
                                    Workspace.This.GlassTopPosVM.IsPosDSelected = false;

                                }
                                if (Workspace.This.GlassTopPosVM.IsPosCenterSelected && _scanCount == 5)
                                {//中间

                                    Workspace.This.ZScanChartGlassVM.Center = new EnumerableDataSource<Point>(chA);
                                    Workspace.This.ZScanChartGlassVM.Center.SetXYMapping(p => p);
                                    //Workspace.This.GlassTopPosVM.ChRightDownPeak = Math.Round(((TestJigScanProcessing)scannedThread).StaticChannelBMax.X, 3);
                                    Workspace.This.GlassTopPosVM.ChCenterPeak = Math.Floor(positionOfMinValue * 1000) / 1000;
                                    Workspace.This.GlassTopPosVM.IsPosCenterSelected = false;

                                }
                                if (Workspace.This.GlassTopPosVM.IsPosCenterDownSelected && _scanCount == 6)
                                {//中下

                                    Workspace.This.ZScanChartGlassVM.CenterDown = new EnumerableDataSource<Point>(chA);
                                    Workspace.This.ZScanChartGlassVM.CenterDown.SetXYMapping(p => p);
                                    //Workspace.This.GlassTopPosVM.ChRightDownPeak = Math.Round(((TestJigScanProcessing)scannedThread).StaticChannelBMax.X, 3);
                                    Workspace.This.GlassTopPosVM.ChCenterDownPeak = Math.Floor(positionOfMinValue * 1000) / 1000;
                                    Workspace.This.GlassTopPosVM.IsPosCenterDownSelected = false;

                                }
                                if (Workspace.This.GlassTopPosVM.IsPosCenterTopSelected && _scanCount == 7)
                                {//中上

                                    Workspace.This.ZScanChartGlassVM.CenterTop = new EnumerableDataSource<Point>(chA);
                                    Workspace.This.ZScanChartGlassVM.CenterTop.SetXYMapping(p => p);
                                    //Workspace.This.GlassTopPosVM.ChRightDownPeak = Math.Round(((TestJigScanProcessing)scannedThread).StaticChannelBMax.X, 3);
                                    Workspace.This.GlassTopPosVM.ChCenterTopPeak = Math.Floor(positionOfMinValue * 1000) / 1000;
                                    Workspace.This.GlassTopPosVM.IsPosCenterTopSelected = false;

                                }

                            }
                            if (SelectLaserChannel.DisplayName == "L")
                            {
                                double positionOfMinValue, valueMin;
                                ThresholdProcess(chC, out positionOfMinValue, out valueMin);
                                if (Workspace.This.GlassTopPosVM.IsPosASelected && _scanCount == 1)
                                {//左下
                                  
                                    Workspace.This.ZScanChartGlassVM.LeftDown = new EnumerableDataSource<Point>(chC);
                                    Workspace.This.ZScanChartGlassVM.LeftDown.SetXYMapping(p => p);
                                    //Workspace.This.GlassTopPosVM.ChLeftDownPeak = Math.Round(((TestJigScanProcessing)scannedThread).StaticChannelCMax.X, 3);
                                    Workspace.This.GlassTopPosVM.ChLeftDownPeak = Math.Floor(positionOfMinValue * 1000) / 1000;
                                    Workspace.This.GlassTopPosVM.IsPosASelected = false;

                                }
                                if (Workspace.This.GlassTopPosVM.IsPosBSelected && _scanCount == 2)
                                {//左上

                                    Workspace.This.ZScanChartGlassVM.LeftTop = new EnumerableDataSource<Point>(chC);
                                    Workspace.This.ZScanChartGlassVM.LeftTop.SetXYMapping(p => p);
                                    //Workspace.This.GlassTopPosVM.ChLeftTopPeak = Math.Round(((TestJigScanProcessing)scannedThread).StaticChannelCMax.X, 3);
                                    Workspace.This.GlassTopPosVM.ChLeftTopPeak = Math.Floor(positionOfMinValue * 1000) / 1000;
                                    Workspace.This.GlassTopPosVM.IsPosBSelected = false;
                                }
                                if (Workspace.This.GlassTopPosVM.IsPosCSelected && _scanCount == 3)
                                {//右上

                                    Workspace.This.ZScanChartGlassVM.RightTop = new EnumerableDataSource<Point>(chC);
                                    Workspace.This.ZScanChartGlassVM.RightTop.SetXYMapping(p => p);
                                   // Workspace.This.GlassTopPosVM.ChRightTopPeak = Math.Round(((TestJigScanProcessing)scannedThread).StaticChannelCMax.X, 3);
                                    Workspace.This.GlassTopPosVM.ChRightTopPeak = Math.Floor(positionOfMinValue * 1000) / 1000;
                                    Workspace.This.GlassTopPosVM.IsPosCSelected = false;
                                }
                                if (Workspace.This.GlassTopPosVM.IsPosDSelected && _scanCount == 4)
                                {//右下

                                    Workspace.This.ZScanChartGlassVM.RightDown = new EnumerableDataSource<Point>(chC);
                                    Workspace.This.ZScanChartGlassVM.RightDown.SetXYMapping(p => p);
                                    //Workspace.This.GlassTopPosVM.ChRightDownPeak = Math.Round(((TestJigScanProcessing)scannedThread).StaticChannelCMax.X, 3);
                                    Workspace.This.GlassTopPosVM.ChRightDownPeak = Math.Floor(positionOfMinValue * 1000) / 1000;
                                    Workspace.This.GlassTopPosVM.IsPosDSelected = false;

                                }
                                if (Workspace.This.GlassTopPosVM.IsPosCenterSelected && _scanCount == 5)
                                {//中间

                                    Workspace.This.ZScanChartGlassVM.Center = new EnumerableDataSource<Point>(chC);
                                    Workspace.This.ZScanChartGlassVM.Center.SetXYMapping(p => p);
                                    //Workspace.This.GlassTopPosVM.ChRightDownPeak = Math.Round(((TestJigScanProcessing)scannedThread).StaticChannelBMax.X, 3);
                                    Workspace.This.GlassTopPosVM.ChCenterPeak = Math.Floor(positionOfMinValue * 1000) / 1000;
                                    Workspace.This.GlassTopPosVM.IsPosCenterSelected = false;

                                }
                                if (Workspace.This.GlassTopPosVM.IsPosCenterDownSelected && _scanCount == 6)
                                {//中下

                                    Workspace.This.ZScanChartGlassVM.CenterDown = new EnumerableDataSource<Point>(chC);
                                    Workspace.This.ZScanChartGlassVM.CenterDown.SetXYMapping(p => p);
                                    //Workspace.This.GlassTopPosVM.ChRightDownPeak = Math.Round(((TestJigScanProcessing)scannedThread).StaticChannelBMax.X, 3);
                                    Workspace.This.GlassTopPosVM.ChCenterDownPeak = Math.Floor(positionOfMinValue * 1000) / 1000;
                                    Workspace.This.GlassTopPosVM.IsPosCenterDownSelected = false;

                                }
                                if (Workspace.This.GlassTopPosVM.IsPosCenterTopSelected && _scanCount == 7)
                                {//中上

                                    Workspace.This.ZScanChartGlassVM.CenterTop = new EnumerableDataSource<Point>(chC);
                                    Workspace.This.ZScanChartGlassVM.CenterTop.SetXYMapping(p => p);
                                    //Workspace.This.GlassTopPosVM.ChRightDownPeak = Math.Round(((TestJigScanProcessing)scannedThread).StaticChannelBMax.X, 3);
                                    Workspace.This.GlassTopPosVM.ChCenterTopPeak = Math.Floor(positionOfMinValue * 1000) / 1000;
                                    Workspace.This.GlassTopPosVM.IsPosCenterTopSelected = false;

                                }

                            }
                            Workspace.This.ZScanChartGlassVM.SaveGlassData(@".\测试报告\玻璃面调平原始数据.csv");
                        }
                        isScanGlass = false;
                        IsScanning = true;
                        IsStopning = false;
                        //HomeMotor();
                    }
                    catch
                    {

                    }



                }
            });
            Z_HomeMotor();
        }

        public bool CanExecuteScanCommand(object parameter)
        {
            return true;
        }

        public void ThresholdProcess(System.Windows.Point[] rawArray, out double positionOfMinValue, out double valueMin)
        {

            //`如果是小大小，就是波峰，大小大就是波谷
            //波峰比附近的值都大，波谷比附近的值都小。。。
            System.Windows.Point Data = new System.Windows.Point();
            List<System.Windows.Point> Datalist = new List<System.Windows.Point>();
            int direction = rawArray[0].Y > 0 ? -1 : 1;
            //找到所有峰值
            for (int i = 0; i < rawArray.Length - 1; i++)
            {
                if ((rawArray[i + 1].Y - rawArray[i].Y) * direction > 0)
                {
                    direction *= -1;
                    if (direction == 1)
                    {
                        Data.X = rawArray[i].X;
                        Data.Y = rawArray[i].Y;
                        Datalist.Add(Data);
                        Console.WriteLine("(" + rawArray[i].X + "," + rawArray[i].Y + ")" + "波峰");
                        //获取数据中多个波峰
                    }
                    //else
                    //{
                    //    positionOfMinValue = rawArray[i].X;
                    //    valueMin = rawArray[i].Y;
                    //    Console.WriteLine("(" + rawArray[i].X + "," + rawArray[i].Y + ")" + "波谷");
                    //    //获取数据中多个波谷
                    //}
                }
            }

            //找到两个最大峰值的下标
            double _positionOfMinValue = 0;
            double _valueMin = 65535;
            double larges = 0;
            double second = 0;
            double largestIndex = 0;
            double secondIndex = 0;
            foreach (System.Windows.Point i in Datalist)
            {
                if (larges < i.Y)
                {
                    larges = i.Y;
                    largestIndex = i.X;
                    Data = i;
                }
            }
            Datalist.Remove(Data);
            bool index = true;

            //移除相近的波峰，
            while (index)
            {
                index = false;
                for (int i = 0; i < Datalist.Count; i++)
                {
                    if (Math.Abs(Datalist[i].X - largestIndex) < 0.3)
                    {
                        Datalist.Remove(Datalist[i]);
                        index = true;
                    }

                }
            }

            foreach (System.Windows.Point i in Datalist)
            {
                if (second < i.Y)
                {
                    second = i.Y;
                    secondIndex = i.X;
                }
            }
            List<System.Windows.Point> LowEddlist = new List<System.Windows.Point>();
            //找寻这两个位置之间最低的值
            if (largestIndex < secondIndex)//最大波峰在前面
            {
                for (int i = 0; i < rawArray.Length - 1; i++)
                {

                    if (rawArray[i].X > largestIndex && rawArray[i].X < secondIndex)
                    {
                        if (_valueMin >= rawArray[i].Y)
                        {
                            _valueMin = rawArray[i].Y;
                            _positionOfMinValue = rawArray[i].X;
                        }
                        //Console.WriteLine("X: " + rawArray[i].X + "  Y: " + rawArray[i].Y);
                    }
                }
                //找到所有低谷的值
                for (int i = 0; i < rawArray.Length - 1; i++)
                {
                    if (rawArray[i].X > largestIndex && rawArray[i].X < secondIndex)
                    {
                        if (_valueMin == rawArray[i].Y)
                        {
                            LowEddlist.Add(rawArray[i]);
                        }
                        //Console.WriteLine("X: " + rawArray[i].X + "  Y: " + rawArray[i].Y);
                    }
                }
                if (LowEddlist.Count > 1)
                {
                    double startPos = LowEddlist[0].X;
                    double endPos = LowEddlist[LowEddlist.Count - 1].X;
                    _positionOfMinValue = (endPos - startPos) / 2 + startPos;
                }
                //Console.WriteLine("X: " + LowEddlist[0].X + "  Y: " + LowEddlist[0].Y);
                //Console.WriteLine("X: " + LowEddlist[LowEddlist.Count-1].X + "  Y: " + LowEddlist[LowEddlist.Count - 1].Y);
            }
            else
            {
                for (int i = 0; i < rawArray.Length - 1; i++)
                {
                    if (rawArray[i].X > secondIndex && rawArray[i].X < largestIndex)
                    {
                        if (_valueMin >= rawArray[i].Y)
                        {
                            _valueMin = rawArray[i].Y;
                            _positionOfMinValue = rawArray[i].X;
                        }
                        //Console.WriteLine("X: " + rawArray[i].X + "  Y: " + rawArray[i].Y);
                    }
                }
                //找到所有低谷的值
                for (int i = 0; i < rawArray.Length - 1; i++)
                {
                    if (rawArray[i].X > secondIndex && rawArray[i].X < largestIndex)
                    {
                        if (_valueMin == rawArray[i].Y)
                        {
                            LowEddlist.Add(rawArray[i]);
                        }
                        //Console.WriteLine("X: " + rawArray[i].X + "  Y: " + rawArray[i].Y);
                    }
                }
                if (LowEddlist.Count > 1)
                {
                    double startPos = LowEddlist[0].X;
                    double endPos = LowEddlist[LowEddlist.Count - 1].X;
                    _positionOfMinValue = (endPos - startPos) / 2 + startPos;
                }
                //Console.WriteLine("X: " + LowEddlist[0].X + "  Y: " + LowEddlist[0].Y);
                //Console.WriteLine("X: " + LowEddlist[LowEddlist.Count - 1].X + "  Y: " + LowEddlist[LowEddlist.Count - 1].Y);
            }



            positionOfMinValue = _positionOfMinValue;
            valueMin = _valueMin;
            //Console.WriteLine("X :" + positionOfMinValue);
            //Console.WriteLine("Y : " + valueMin);

        }

        #region MovingAverage

        public static int[] MovingAverage(int[] data, int span)
        {
            int b = 0;
            if (span % 2 == 1)
                b = (span - 1) / 2;
            else
            {
                span -= 1;
                b = (span - 1) / 2;
            }
            int[] smoothArray = new int[data.Length];
            if (data.Length > span)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    if (i < b)
                    {
                        smoothArray[i] = 0;
                        for (int j = -i; j < i + 1; j++)
                        {
                            smoothArray[i] += data[i + j];
                        }
                        smoothArray[i] /= (2 * i + 1);
                    }
                    else if (i >= b && (data.Length - i) > b)
                    {
                        smoothArray[i] = 0;
                        for (int j = -b; j < b + 1; j++)
                        {
                            smoothArray[i] += data[i + j];
                        }
                        smoothArray[i] /= span;
                    }
                    else
                    {
                        smoothArray[i] = 0;
                        int c = data.Length - i - 1;
                        for (int j = -c; j < c + 1; j++)
                        {
                            smoothArray[i] += data[i + j];
                        }
                        smoothArray[i] /= (2 * c + 1);

                    }
                }
            }
            else
            {
                throw new ArgumentException("span is bigger than data's count");

            }

            return smoothArray;
        }
        #endregion
        private void _LaserOn()
        {
            if (Workspace.This.GlassTopPosVM.IsPosBSelected)
            {
                IsLaserR2Selected = true;
            }
            //else if (Workspace.This.GlassTopPosVM.IsPosASelected)
            //{
            //    _LaserChannelsControlVM.LaserBSelected = true;
            //}
            //else if (Workspace.This.GlassTopPosVM.IsPosASelected)
            //{
            //    _LaserChannelsControlVM.LaserCSelected = true;
            //}
            ////else if (IsLaserDSelected)
            ////{
            ////    _LaserChannelsControlVM.LaserDSelected = true;
            ////}
        }
        #endregion

        #region Observe Data Command
        private RelayCommand _ObserveDataCommand = null;
        public ICommand ObserveDataCommand
        {
            get
            {
                if (_ObserveDataCommand == null)
                {
                    _ObserveDataCommand = new RelayCommand(ExcuteObserveDataCommand, CanExcuteObserveDataCommand);
                }
                return _ObserveDataCommand;
            }
        }
        public void ExcuteObserveDataCommand(object parameter)
        {
            //if (_TestGlassTopAdjust.IsChecked)
            {
                try
                {
                    FileInfo fileInfo = new FileInfo(@".\测试报告\玻璃面调平原始数据.csv");
                    Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
                    Microsoft.Office.Interop.Excel.Workbook book = excel.Application.Workbooks.Add(fileInfo.FullName);
                    excel.Visible = true;
                }
                catch
                {
                    if (Directory.Exists(@".\测试报告"))
                    {
                        System.Diagnostics.Process.Start("Explorer.exe", @".\测试报告");
                    }
                }
            }
        }
        public bool CanExcuteObserveDataCommand(object parameter)
        {
            return true;
        }
        #endregion
    }
}
