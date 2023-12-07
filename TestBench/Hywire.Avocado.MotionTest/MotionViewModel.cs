using Azure.Avocado.EthernetCommLib;
using Azure.Avocado.MotionLib;
using Azure.WPF.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hywire.Avocado.MotionTest
{
    class MotionViewModel : ViewModelBase
    {
        #region Private Fields
        private double _StartSpeed;
        private double _TopSpeed;
        private double _AccVal;
        private double _DccVal;
        private double _TgtPos;
        private double _CrntPos;
        private double _Coeff;
        private bool _IsEnabled;
        private bool _AtFwdLimit;
        private bool _AtBwdLimit;
        private bool _AtHome;
        private bool _IsBusy;
        private MotorTypes _MotionType;
        private MotionSignalPolarity _Polarities;
        #endregion Private Fields

        public MotionViewModel(MotorTypes type)
        {
            _MotionType = type;
            _Polarities = new MotionSignalPolarity();
        }

        #region Public Properties
        public MotorTypes MotionType
        {
            get { return _MotionType; }
        }
        public double StartSpeed
        {
            get { return _StartSpeed; }
            set
            {
                if (_StartSpeed != value)
                {
                    _StartSpeed = value;
                    RaisePropertyChanged(nameof(StartSpeed));
                }
            }
        }
        public double TopSpeed
        {
            get { return _TopSpeed; }
            set
            {
                if (_TopSpeed != value)
                {
                    _TopSpeed = value;
                    RaisePropertyChanged(nameof(TopSpeed));
                }
            }
        }
        public double AccVal
        {
            get { return _AccVal; }
            set
            {
                if (_AccVal != value)
                {
                    _AccVal = value;
                    RaisePropertyChanged(nameof(AccVal));
                }
            }
        }
        public double DccVal
        {
            get { return _DccVal; }
            set
            {
                if (_DccVal != value)
                {
                    _DccVal = value;
                    RaisePropertyChanged(nameof(DccVal));
                }
            }
        }
        public double TgtPos
        {
            get { return _TgtPos; }
            set
            {
                if (_TgtPos != value)
                {
                    _TgtPos = value;
                    RaisePropertyChanged(nameof(TgtPos));
                }
            }
        }
        public double CrntPos
        {
            get { return _CrntPos; }
            set
            {
                if (_CrntPos != value)
                {
                    _CrntPos = value;
                    RaisePropertyChanged(nameof(CrntPos));
                }
            }
        }
        public double Coeff
        {
            get { return _Coeff; }
            set
            {
                if (_Coeff != value)
                {
                    _Coeff = value;
                    RaisePropertyChanged(nameof(Coeff));
                }
            }
        }
        public bool IsEnabled
        {
            get { return _IsEnabled; }
            set
            {
                if (_IsEnabled != value)
                {
                    _IsEnabled = value;
                    RaisePropertyChanged(nameof(IsEnabled));
                }
            }
        }
        public bool AtFwdLimit
        {
            get { return _AtFwdLimit; }
            set
            {
                if (_AtFwdLimit != value)
                {
                    _AtFwdLimit = value;
                    RaisePropertyChanged(nameof(AtFwdLimit));
                }
            }
        }
        public bool AtBwdLimit
        {
            get { return _AtBwdLimit; }
            set
            {
                if (_AtBwdLimit != value)
                {
                    _AtBwdLimit = value;
                    RaisePropertyChanged(nameof(AtBwdLimit));
                }
            }
        }
        public bool AtHome
        {
            get { return _AtHome; }
            set
            {
                if (_AtHome != value)
                {
                    _AtHome = value;
                    RaisePropertyChanged(nameof(AtHome));
                }
            }
        }
        public bool IsBusy
        {
            get { return _IsBusy; }
            set
            {
                if (_IsBusy != value)
                {
                    _IsBusy = value;
                    RaisePropertyChanged(nameof(IsBusy));
                }
            }
        }

        public MotionSignalPolarity Polarities
        {
            get { return _Polarities; }
        }

        #endregion Public Properties
    }
}
