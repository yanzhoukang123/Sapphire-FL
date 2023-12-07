using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.Common
{
    [Serializable()]
    public class ZScan
    {
        private double _BottomImageFocus = 0;
        private double _DeltaFocus = 0;
        private int _NumOfImages = 2;

        public ZScan()
        {
        }

        public object Clone()
        {
            ZScan clone = (ZScan)this.MemberwiseClone();
            return clone;
        }

        public double BottomImageFocus
        {
            get { return _BottomImageFocus; }
            set { _BottomImageFocus = value; }
        }
        public double DeltaFocus
        {
            get { return _DeltaFocus; }
            set { _DeltaFocus = value; }
        }
        public int NumOfImages
        {
            get { return _NumOfImages; }
            set { _NumOfImages = value; }
        }
    }
}
