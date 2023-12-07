using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input; //ICommand
using Azure.WPF.Framework;  //ViewModelBase
using System.Collections.ObjectModel;   //ObservableCollection
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using System.IO;
using Azure.ImagingSystem;
using Azure.Configuration.Settings;
using System.Threading;
using Azure.EthernetCommLib;

namespace Azure.ScannerEUI.ViewModel
{
    enum ChannelTypes
    {
        A,
        B,
        C,
        D,
        Maintenance
    }

    class ApdViewModel : ViewModelBase
    {        
        #region Private data...

        int _ApdAGain = 100;
        int _ApdBGain = 100;
        int _ApdCGain = 100;
        int _ApdDGain = 500;

        int _ApdAPga = 0;
        int _ApdBPga = 0;
        int _ApdCPga = 0;
        int _ApdDPga = 0;

        int _ApdAValue = 0;
        int _ApdBValue = 0;
        int _ApdCValue = 0;
        int _ApdDValue = 0;

        int _RunningMode = 0;

        bool? _LIDIsOpen = null;

        private ObservableCollection<APDGainType> _APDGainOptions = null;
        private APDGainType _SelectedApdAGain = null;
        private APDGainType _SelectedApdBGain = null;
        //private APDGainType _SelectedApdCGain = null;
        //private APDGainType _SelectedApdDGain = null;

        private ObservableCollection<APDPgaType> _APDPgaOptions = null;
        private APDPgaType _SelectedAPDPgaA = null;
        private APDPgaType _SelectedAPDPgaB = null;
        private APDPgaType _SelectedAPDPgaC = null;
        private APDPgaType _SelectedAPDPgaD = null;

        private EthernetController _EthernetController;

        private ChannelTypes _SelectedFirstChannel;
        private bool _IsAChannel;
        private bool _IsBChannel;
        private ChannelTypes _SelectedThirdChannel;
        private bool _IsCChannel;
        private bool _IsDChannel;
        public int _TempLPMTValue = 0;
        public int _TempR1PMTValue = 0;
        public int _TempR2PMTValue = 0;
        public int _TempPMTValueSinge = 0;
        #endregion

        /// <summary>
        /// The state of the door
        /// </summary>
        public bool? LIDIsOpen
        {
            get
            {
                return _LIDIsOpen;
            }
            set
            {
                if (_LIDIsOpen != value)
                {
                    _LIDIsOpen = value;
                    if (_LIDIsOpen == true)
                    {
                        Workspace.This.DoorStatus = "Open";
                        IsCheckPMTAndOpenDoor();
                    }
                    else
                    {
                        Workspace.This.DoorStatus = "Close";
                        IsCheckPMTAndCloseDoor();
                    }
                }
            }
        }
        /// <summary>
        /// 盖子打开时，如果PMT传感器设置的增益高于4000就重新设置增益到4000 
        /// When the cover is open, reset the gain to 4000 if the PMT sensor set gain is higher than 4000
        /// </summary>
        void IsCheckPMTAndOpenDoor()
        {
            if (_TempPMTValueSinge == 0)
            {
                _TempLPMTValue = Workspace.This.IVVM.GainTxtModuleL1;
                _TempR1PMTValue = Workspace.This.IVVM.GainTxtModuleR1;
                _TempR2PMTValue = Workspace.This.IVVM.GainTxtModuleR2;
                if (Workspace.This.IVVM.SensorML1 == IvSensorType.PMT)
                {
                    Workspace.This.IVVM.GainTxtModuleL1 = 4000;
                }
                if (Workspace.This.IVVM.SensorMR1 == IvSensorType.PMT)
                {
                    Workspace.This.IVVM.GainTxtModuleR1 = 4000;
                }
                if (Workspace.This.IVVM.SensorMR2 == IvSensorType.PMT)
                {
                    Workspace.This.IVVM.GainTxtModuleR2 = 4000;
                }
                _TempPMTValueSinge = 1;
            }
        }
        /// <summary>
        /// 盖子关闭时，将PMT的增益还原  
        /// When the cover is closed, the gain of PMT is restored
        /// </summary>
        void IsCheckPMTAndCloseDoor()
        {
            if (_TempPMTValueSinge == 1)
            {
                if (Workspace.This.IVVM.SensorML1 == IvSensorType.PMT)
                {
                    Workspace.This.IVVM.GainTxtModuleL1 = _TempLPMTValue;
                }
                if (Workspace.This.IVVM.SensorMR1 == IvSensorType.PMT)
                {
                    Workspace.This.IVVM.GainTxtModuleR1 = _TempR1PMTValue;
                }
                if (Workspace.This.IVVM.SensorMR2 == IvSensorType.PMT)
                {
                    Workspace.This.IVVM.GainTxtModuleR2 = _TempR2PMTValue;
                }
                _TempPMTValueSinge = 0;
            }
        }
        public int RunningMode
        {

            get { return _RunningMode; }
        }

        public bool IsAChannel
        {
            get { return _IsAChannel; }
            set
            {
                if (_IsAChannel != value)
                {
                    _IsAChannel = value;
                    RaisePropertyChanged(nameof(IsAChannel));
                }
            }
        }
        public bool IsBChannel
        {
            get { return _IsBChannel; }
            set
            {
                if (_IsBChannel != value)
                {
                    _IsBChannel = value;
                    RaisePropertyChanged(nameof(IsBChannel));
                }
            }
        }
        public ObservableCollection<APDGainType> APDGainOptions
        {

            get { return _APDGainOptions; }
        }

        public APDGainType SelectedApdAGain
        {
            get { return _SelectedApdAGain; }
            set
            {
                if (_SelectedApdAGain != value)
                {
                    _SelectedApdAGain = value;
                    RaisePropertyChanged("SelectedApdAGain");
                    ApdAGain = _SelectedApdAGain.Value;
                }
            }
        }

        public APDGainType SelectedApdBGain
        {
            get { return _SelectedApdBGain; }
            set
            {
                if (_SelectedApdBGain != value)
                {
                    _SelectedApdBGain = value;
                    RaisePropertyChanged("SelectedApdBGain");
                    ApdBGain = _SelectedApdBGain.Value;
                }
            }
        }

        //public APDGainType SelectedApdCGain
        //{
        //    get { return _SelectedApdCGain; }
        //    set
        //    {
        //        if (_SelectedApdCGain != value)
        //        {
        //            _SelectedApdCGain = value;
        //            RaisePropertyChanged("SelectedApdCGain");
        //            ApdCGain = _SelectedApdCGain.Value;
        //        }
        //    }
        //}

        public ObservableCollection<APDPgaType> APDPgaOptions
        {

            get { return _APDPgaOptions; }
        }

        public APDPgaType SelectedAPDPgaA
        {
            get { return _SelectedAPDPgaA; }
            set
            {
                if (_SelectedAPDPgaA != value)
                {
                    _SelectedAPDPgaA = value;
                    ApdAPga = _SelectedAPDPgaA.Value;
                    RaisePropertyChanged("SelectedAPDPgaA");
                }
            }
        }

        public APDPgaType SelectedAPDPgaB
        {
            get { return _SelectedAPDPgaB; }
            set
            {
                if (_SelectedAPDPgaB != value)
                {
                    _SelectedAPDPgaB = value;
                    ApdBPga = _SelectedAPDPgaB.Value;
                    RaisePropertyChanged("SelectedAPDPgaB"); 
                }
            }
        }
        public APDPgaType SelectedAPDPgaC
        {
            get { return _SelectedAPDPgaC; }
            set
            {
                if (_SelectedAPDPgaC != value)
                {
                    _SelectedAPDPgaC = value;
                    ApdCPga = _SelectedAPDPgaC.Value;
                    RaisePropertyChanged("SelectedAPDPgaC");
                }
            }
        }

        public APDPgaType SelectedAPDPgaD
        {
            get { return _SelectedAPDPgaD; }
            set
            {
                if (_SelectedAPDPgaD != value)
                {
                    _SelectedAPDPgaD = value;
                    ApdDPga = _SelectedAPDPgaD.Value;
                    RaisePropertyChanged("SelectedAPDPgaD");
                }
            }
        }


        public EthernetController EthernetDevice
        {
            get
            {
                return _EthernetController;
            }
        }

        #region Constructors...

        public ApdViewModel(EthernetController ethernetController)
        {
            _EthernetController = ethernetController;
        }

        #endregion

        public void InitApdControls()
        {
            _APDGainOptions = SettingsManager.ConfigSettings.APDGains;
            _APDPgaOptions = SettingsManager.ConfigSettings.APDPgas;
            RaisePropertyChanged("APDGainOptions");
            RaisePropertyChanged("APDPgaOptions");

            //if (_APDPgaOptions != null && _APDPgaOptions.Count >= 4)
            //{
            //    SelectedAPDPgaA = _APDPgaOptions[3];    // select the 4th item
            //    SelectedAPDPgaB = _APDPgaOptions[3];    // select the 4th item
            //    SelectedAPDPgaC = _APDPgaOptions[3];    // select the 4th item
            //    SelectedAPDPgaD = _APDPgaOptions[3];    // select the 4th item
            //}
            //if (_APDGainOptions != null && _APDGainOptions.Count >= 6)
            //{
            //    SelectedApdAGain = _APDGainOptions[5];    // select the 6th item
            //    SelectedApdBGain = _APDGainOptions[5];    // select the 6th item
            //    //SelectedApdCGain = _APDGainOptions[5];    // select the 6th item
            //    ApdCGain = 4000;
            //    ApdDGain = 5000;
            //}
        }

        #region Public properties...

        public int ApdAGain
        {
            get { return _ApdAGain; }
            set
            {
                if (_ApdAGain != value)
                {
                    _ApdAGain = value;
                    RaisePropertyChanged("ApdAGain");
                    //if (_APDTransfer.APDTransferIsAlive)
                    //{
                    //    _APDTransfer.APDSetA(_ApdAGain);
                    //}
                }
            }
        }
        public int ApdBGain
        {
            get { return _ApdBGain; }
            set
            {
                if (_ApdBGain != value)
                {
                    _ApdBGain = value;
                    RaisePropertyChanged("ApdBGain");
                    //if (_APDTransfer.APDTransferIsAlive)
                    //{
                    //    _APDTransfer.APDSetB(_ApdBGain);
                    //}
                }
            }
        }
        public int ApdCGain
        {
            get { return _ApdCGain; }
            set
            {
                if (_ApdCGain != value)
                {
                    _ApdCGain = value;
                    RaisePropertyChanged("ApdCGain");
                    //if (_APDTransfer.APDTransferIsAlive)
                    //{
                    //    _APDTransfer.APDSetC(_ApdCGain);
                    //}
                }
            }
        }
        public int ApdDGain
        {
            get { return _ApdDGain; }
            set
            {
                if (_ApdDGain != value)
                {
                    _ApdDGain = value;
                    RaisePropertyChanged("ApdDGain");
                    //if (_APDTransfer.APDTransferIsAlive)
                    //{
                    //    _APDTransfer.APDSetD(_ApdDGain);
                    //}
                }
            }
        }

        public int ApdAPga
        {
            get { return _ApdAPga; }
            set
            {
                if (_ApdAPga != value)
                {
                    _ApdAPga = value;
                   // _EthernetController.SetIvPga(IVChannels.ChannelA, (ushort)value);
                    //if (_APDTransfer.APDTransferIsAlive)
                    //{
                    //    _APDTransfer.APDPgaSetA(_ApdAPga);
                    //}
                }
            }
        }

        public int ApdBPga
        {
            get { return _ApdBPga; }
            set
            {
                if (_ApdBPga != value)
                {
                    _ApdBPga = value;
                   // _EthernetController.SetIvPga(IVChannels.ChannelB, (ushort)value);
                    //if (_APDTransfer.APDTransferIsAlive)
                    //{
                    //    _APDTransfer.APDPgaSetB(_ApdBPga);
                    //}
                }
            }
        }

        public int ApdCPga
        {
            get { return _ApdCPga; }
            set
            {
                if (_ApdCPga != value)
                {
                    _ApdCPga = value;
                   // _EthernetController.SetIvPga(IVChannels.ChannelC, (ushort)value);
                    //if (_APDTransfer.APDTransferIsAlive)
                    //{
                    //    _APDTransfer.APDPgaSetC(_ApdCPga);
                    //}
                }
            }
        }

        public int ApdDPga
        {
            get { return _ApdDPga; }
            set
            {
                if (_ApdDPga != value)
                {
                    _ApdDPga = value;
                    //if (_APDTransfer.APDTransferIsAlive)
                    //{
                    //    _APDTransfer.APDPgaSetD(_ApdDPga);
                    //}
                }
            }
        }

        public int ApdAValue
        {
            get { return _ApdAValue; }
            set
            {
                if (_ApdAValue != value)
                {
                    _ApdAValue = value;
                    RaisePropertyChanged("ApdAValue");
                }
            }
        }
        public int ApdBValue
        {
            get { return _ApdBValue; }
            set
            {
                if (_ApdBValue != value)
                {
                    _ApdBValue = value;
                    RaisePropertyChanged("ApdBValue");
                }
            }
        }
        public int ApdCValue
        {
            get { return _ApdCValue; }
            set
            {
                if (_ApdCValue != value)
                {
                    _ApdCValue = value;
                    RaisePropertyChanged("ApdCValue");
                }
            }
        }
        public int ApdDValue
        {
            get { return _ApdDValue; }
            set
            {
                if (_ApdDValue != value)
                {
                    _ApdDValue = value;
                    RaisePropertyChanged("ApdDValue");
                }
            }
        }

        #endregion
        #region ScanMode function
        public void DynamicScanMode(int quality)
        {
            //_APDTransfer.APDLaserScanQualitySet(quality);
         }
        public void StaticScanMode(int dataRate, int lineCounts)
        {
            //_APDTransfer.APDLaserScanDataRateSet(dataRate);
            //_APDTransfer.APDLaserScanLineCountsSet(lineCounts);
        }

        #endregion ScanMode function
        #region ReadApdCommand

        private RelayCommand _ReadApdCommand = null;

        public ICommand ReadApdCommand
        {
            get
            {
                if (_ReadApdCommand == null)
                {
                    _ReadApdCommand = new RelayCommand(this.Execute_ReadApdCommand, this.CanExecute_ReadApdCommand);
                }

                return _ReadApdCommand;
            }
        }
        public void Execute_ReadApdCommand(object parameter)
        {
            //TODO: implement the read apd values command
            //if (Workspace.This.ApdVM._APDTransfer.APDTransferIsAlive)
            //{
            //    //System.Windows.MessageBox.Show(string.Format("TODO: Set APD D Gain: {0}", _ApdDGain));
            //    _APDTransfer.APDLaserReadAPD();
            //}

            if (_EthernetController.TriggerSingleScan())
            {
                ApdAValue = (int)_EthernetController.SampleValueChA;
                ApdBValue = (int)_EthernetController.SampleValueChB;
                ApdCValue = (int)_EthernetController.SampleValueChC;
            }
        }

        public bool CanExecute_ReadApdCommand(object parameter)
        {
            return true;
        }

        #endregion
    }

}
