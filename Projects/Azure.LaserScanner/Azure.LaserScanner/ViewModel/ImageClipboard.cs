using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;   //Size, Rect
using System.Windows.Media.Imaging; //WriteableBitmap

namespace Azure.LaserScanner.ViewModel
{
    public class ImageClipboard
    {
        public string Title { get; set; }
        public Size OrigSize { get; set; }
        public Rect ClipRect { get; set; }
        public WriteableBitmap ClipImage { get; set; }
        public WriteableBitmap BlobsImage { get; set; }

        public ImageClipboard()
        {
        }
    }

}
