using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Azure.LaserScanner.ViewModel
{
    class ImageItem
    {
        public int RegionNumber { get; set; }
        public WriteableBitmap Image { get; set; }
        public double Left { get; set; }
        public double Top { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

    }
}
