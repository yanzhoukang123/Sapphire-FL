using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Azure.WPF.Framework;

namespace Azure.LaserScanner.ViewModel
{
    class ScanRegionRect : ViewModelBase
    {
        public delegate void ScanRegionChangedHandler(object sender);
        public event ScanRegionChangedHandler ScanRegionChanged;

        private double _Y = 0;
        private double _X = 0;
        private double _Width = 0;
        private double _Height = 0;
        
        public ScanRegionRect()
        {
        }
        public ScanRegionRect(double x, double y, double width, double height)
        {
            _X = x;
            _Y = y;
            _Width = width;
            _Height = height;
        }
        public ScanRegionRect(Rect rect)
        {
            _X = rect.X;
            _Y = rect.Y;
            _Width = rect.Width;
            _Height = rect.Height;
        }

        public object Clone()
        {
            ScanRegionRect clone = (ScanRegionRect)this.MemberwiseClone();

            return clone;
        }

        public double X
        {
            get { return _X; }
            set
            {
                _X = value;
                RaisePropertyChanged("X");
                ScanRegionChanged?.Invoke(this);
            }
        }
        public double Y
        {
            get { return _Y; }
            set
            {
                _Y = value;
                RaisePropertyChanged("Y");
                ScanRegionChanged?.Invoke(this);
            }
        }
        public double Width 
        {
            get { return _Width; }
            set
            {
                _Width = value;
                RaisePropertyChanged("Width");
                ScanRegionChanged?.Invoke(this);
            }
        }
        public double Height
        {
            get
            {
                return _Height;
            }
            set
            {
                _Height = value;
                RaisePropertyChanged("Height");
                if (ScanRegionChanged != null)
                {
                    ScanRegionChanged(this);
                }
            }
        }

        public override string ToString()
        {
            return string.Format("{0},{1},{2},{3}", _X, _Y, _Width, _Height);
        }
    }
}
