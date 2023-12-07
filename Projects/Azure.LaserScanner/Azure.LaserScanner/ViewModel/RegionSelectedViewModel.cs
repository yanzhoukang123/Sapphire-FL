using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.WPF.Framework;

namespace Azure.LaserScanner.ViewModel
{
    [Serializable]
    class RegionSelectedViewModel : ViewModelBase
    {
        private double _ScanRegionX;
        private double _ScanRegionY;
        private double _ScanRegionWidth;
        private double _ScanRegionHeight;

        public RegionSelectedViewModel()
        {
        }

        public double ScanRegionX
        {
            get { return _ScanRegionX; }
            set
            {
                _ScanRegionX = value;
                RaisePropertyChanged("ScanRegionX");
            }
        }
        public double ScanRegionY
        {
            get { return _ScanRegionY; }
            set
            {
                _ScanRegionY = value;
                RaisePropertyChanged("ScanRegionY");
            }
        }
        public double ScanRegionWidth
        {
            get { return _ScanRegionWidth; }
            set
            {
                _ScanRegionWidth = value;
                RaisePropertyChanged("ScanRegionWidth");
            }
        }
        public double ScanRegionHeight
        {
            get { return _ScanRegionHeight; }
            set
            {
                _ScanRegionHeight = value;
                RaisePropertyChanged("ScanRegionHeight");
            }
        }
    }
}
