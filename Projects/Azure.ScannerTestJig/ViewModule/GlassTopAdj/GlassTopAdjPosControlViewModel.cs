using Azure.WPF.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Azure.ScannerTestJig.ViewModule.GlassTopAdj
{
    class GlassTopAdjPosControlViewModel : ViewModelBase
    {
        #region Private data
        private double _ChLeftDownPeak = 0;
        private double _ChLeftTopPeak = 0;
        private double _ChRightTopPeak = 0;
        private double _ChRightDownPeak = 0;
        private double _ChCenterPeak = 0;
        private double _ChCenterDownPeak = 0;
        private double _ChCenterTopPeak = 0;
        private bool _IsPosASelected = false;
        private bool _IsPosBSelected = false;
        private bool _IsPosCSelected = false;
        private bool _IsPosDSelected = false;
        private bool _IsPosCenterSelected = false;
        private bool _IsPosCenterDownSelected = false;
        private bool _IsPosCenterTopSelected = false;
        private bool _SelectVisChannel = true;
        private Visibility _CenterVis = Visibility.Visible;
        #endregion
        #region public properties
        public Visibility CenterVis
        {
            get { return _CenterVis; }
            set
            {
                _CenterVis = value;
                RaisePropertyChanged("CenterVis");
            }
        }
        public bool SelectVisChannel
        {
            get { return _SelectVisChannel; }
            set
            {
                _SelectVisChannel = value;
                RaisePropertyChanged("SelectVisChannel");
            }
        }
        public double ChLeftDownPeak
        {
            get
            {
                return _ChLeftDownPeak;
            }
            set
            {
                if (_ChLeftDownPeak != value)
                {
                    _ChLeftDownPeak = value;
                    RaisePropertyChanged("ChLeftDownPeak");
                }
            }
        }

        public double ChLeftTopPeak
        {
            get
            {
                return _ChLeftTopPeak;
            }
            set
            {
                if (_ChLeftTopPeak != value)
                {
                    _ChLeftTopPeak = value;
                    RaisePropertyChanged("ChLeftTopPeak");
                }
            }
        }

        public double ChRightTopPeak
        {
            get
            {
                return _ChRightTopPeak;
            }
            set
            {
                if (_ChRightTopPeak != value)
                {
                    _ChRightTopPeak = value;
                    RaisePropertyChanged("ChRightTopPeak");
                }
            }
        }

        public double ChRightDownPeak
        {
            get
            {
                return _ChRightDownPeak;
            }
            set
            {
                if (_ChRightDownPeak != value)
                {
                    _ChRightDownPeak = value;
                    RaisePropertyChanged("ChRightDownPeak");
                }
            }
        }
        public double ChCenterPeak
        {
            get
            {
                return _ChCenterPeak;
            }
            set
            {
                if (_ChCenterPeak != value)
                {
                    _ChCenterPeak = value;
                    RaisePropertyChanged("ChCenterPeak");
                }
            }
        }

        public double ChCenterDownPeak
        {
            get
            {
                return _ChCenterDownPeak;
            }
            set
            {
                if (_ChCenterDownPeak != value)
                {
                    _ChCenterDownPeak = value;
                    RaisePropertyChanged("ChCenterDownPeak");
                }
            }
        }

        public double ChCenterTopPeak
        {
            get
            {
                return _ChCenterTopPeak;
            }
            set
            {
                if (_ChCenterTopPeak != value)
                {
                    _ChCenterTopPeak = value;
                    RaisePropertyChanged("ChCenterTopPeak");
                }
            }
        }

        public bool IsPosASelected
        {
            get
            {
                return _IsPosASelected;
            }
            set
            {
                if (_IsPosASelected != value)
                {
                    _IsPosASelected = value;
                    RaisePropertyChanged("IsPosASelected");
                }
            }
        }

        public bool IsPosBSelected
        {
            get
            {
                return _IsPosBSelected;
            }
            set
            {
                if (_IsPosBSelected != value)
                {
                    _IsPosBSelected = value;
                    RaisePropertyChanged("IsPosBSelected");
                }
            }
        }

        public bool IsPosCSelected
        {
            get
            {
                return _IsPosCSelected;
            }
            set
            {
                if (_IsPosCSelected != value)
                {
                    _IsPosCSelected = value;
                    RaisePropertyChanged("IsPosCSelected");
                }
            }
        }

        public bool IsPosDSelected
        {
            get
            {
                return _IsPosDSelected;
            }
            set
            {
                if (_IsPosDSelected != value)
                {
                    _IsPosDSelected = value;
                    RaisePropertyChanged("IsPosDSelected");
                }
            }
        }
        public bool IsPosCenterSelected
        {
            get
            {
                return _IsPosCenterSelected;
            }
            set
            {
                if (_IsPosCenterSelected != value)
                {
                    _IsPosCenterSelected = value;
                    RaisePropertyChanged("IsPosCenterSelected");
                }
            }
        }

        public bool IsPosCenterDownSelected
        {
            get
            {
                return _IsPosCenterDownSelected;
            }
            set
            {
                if (_IsPosCenterDownSelected != value)
                {
                    _IsPosCenterDownSelected = value;
                    RaisePropertyChanged("IsPosCenterDownSelected");
                }
            }
        }
        public bool IsPosCenterTopSelected
        {
            get
            {
                return _IsPosCenterTopSelected;
            }
            set
            {
                if (_IsPosCenterTopSelected != value)
                {
                    _IsPosCenterTopSelected = value;
                    RaisePropertyChanged("IsPosCenterTopSelected");
                }
            }
        }
        #endregion

    }
}
