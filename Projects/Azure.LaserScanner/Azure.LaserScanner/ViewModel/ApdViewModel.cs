using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input; //ICommand
using System.Collections.ObjectModel;   //ObservableCollection
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using System.IO;
using Azure.WPF.Framework;  //ViewModelBase
using Azure.ImagingSystem;
using Azure.Configuration.Settings;

namespace Azure.LaserScanner.ViewModel
{
    class ApdViewModel : ViewModelBase
    {        
        #region Private data...

        int _ApdAGain = 5;
        int _ApdBGain = 5;
        int _ApdCGain = 5;
        int _ApdDGain = 5;

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

        private ObservableCollection<APDPgaType> _APDPgaOptions = null;

        //private APDTransfer _APDTransfer;

        private System.Timers.Timer _PMTGainRecoveryTimer;
        private bool _IsPMTGainRecovered;

        #endregion

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
                    }
                    else
                    {
                        Workspace.This.DoorStatus = "Close";
                    }
                }
            }
        }
        public int RunningMode
        {

            get { return _RunningMode; }
        }

        public ObservableCollection<APDGainType> APDGainOptions
        {

            get { return _APDGainOptions; }
        }

        public ObservableCollection<APDPgaType> APDPgaOptions
        {

            get { return _APDPgaOptions; }
        }

        //public APDTransfer APDTransfer
        //{
        //    get
        //    {
        //        return _APDTransfer;
        //    }
        //}

        #region Constructors...

        public ApdViewModel()
        {
            //_APDTransfer = new APDTransfer();
            //_APDTransfer.Initialize();
            //_APDTransfer.ParametersStatusUpdate+=new APDTransfer.ParametersStatusUpdateHandle(ParametersStatusUpdate);
            //_APDTransfer.OnLidStatusChanged += _APDTransfer_OnLidStatusChanged;
            //_APDGainOptions = SettingsManager.ConfigSettings.APDGains;
            //_APDPgaOptions = SettingsManager.ConfigSettings.APDPgas;

            //if (_APDTransfer.APDTransferIsAlive)
            //{
            //    _APDTransfer.APDSetA(_ApdAGain);
            //    _APDTransfer.APDSetB(_ApdBGain);
            //    _APDTransfer.APDSetC(_ApdCGain);
            //    _APDTransfer.APDSetD(_ApdDGain);
            //}

            _PMTGainRecoveryTimer = new System.Timers.Timer();
            _PMTGainRecoveryTimer.AutoReset = false;
            _PMTGainRecoveryTimer.Interval = 20000;
            _PMTGainRecoveryTimer.Elapsed += _PMTGainRecoveryTimer_Elapsed;
            IsPMTGainRecovered = true;
        }

        ~ApdViewModel()
        {
            //_APDTransfer.ParametersStatusUpdate -= new APDTransfer.ParametersStatusUpdateHandle(ParametersStatusUpdate);
            //_APDTransfer.OnLidStatusChanged -= _APDTransfer_OnLidStatusChanged;
            _PMTGainRecoveryTimer.Elapsed -= _PMTGainRecoveryTimer_Elapsed;
        }

        private void _PMTGainRecoveryTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            IsPMTGainRecovered = true;
        }

        private void _APDTransfer_OnLidStatusChanged(bool lidIsOpen)
        {
            if (lidIsOpen)
            {
                // set PMT's gain to 5000 if it is larger than 5000, note that only set to device, remain the field's value
                if (_ApdDGain > 5000)
                {
                    //_APDTransfer.APDSetD(5000);
                    IsPMTGainRecovered = false;
                    _PMTGainRecoveryTimer.Stop();
                }
            }
            else
            {
                // recover PMT's gain to the field's value, and enable the timer for delay purpose
                if (_ApdDGain > 5000)
                {
                    //_APDTransfer.APDSetD(_ApdDGain);
                    IsPMTGainRecovered = false;
                    _PMTGainRecoveryTimer.Start();
                }
            }
        }

        #endregion

        #region Public properties...
        public bool IsPMTGainRecovered { get; private set; }

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
        //public void StaticScanMode(int dataRate, int lineCounts)
        //{
        //    _APDTransfer.APDLaserScanDataRateSet(dataRate);
        //    _APDTransfer.APDLaserScanLineCountsSet(lineCounts);
        //}

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
            //EL: TODO: implement the read apd values command
            //if (Workspace.This.ApdVM._APDTransfer.APDTransferIsAlive)
            //{
            //    //Xceed.Wpf.Toolkit.MessageBox.Show(string.Format("TODO: Set APD D Gain: {0}", _ApdDGain));
            //    _APDTransfer.APDLaserReadAPD();
            //}
        }

        public bool CanExecute_ReadApdCommand(object parameter)
        {
            return true;
        }

        #endregion
        //void ParametersStatusUpdate(object sender)
        //{
        //    if (_APDTransfer.APDTransferIsAlive)
        //    {
        //        ApdAValue = _APDTransfer.SingleReadAPD[0];
        //        ApdBValue = _APDTransfer.SingleReadAPD[1];
        //        ApdCValue = _APDTransfer.SingleReadAPD[2];
        //        ApdDValue = _APDTransfer.SingleReadAPD[3];
        //        _RunningMode = _APDTransfer.RunningMode;
        //        LIDIsOpen = _APDTransfer.LIDIsOpen;
        //    }
        //}
    }

}
