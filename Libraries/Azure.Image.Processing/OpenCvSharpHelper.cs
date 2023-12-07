using OpenCvSharp.WpfExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Azure.Image.Processing
{
    public static class OpenCvSharpHelper
    {
        public static WriteableBitmap Filter2D(WriteableBitmap srcimg)
        {
            // Declare variables
            //OpenCvSharp.Mat kernel;
            //OpenCvSharp.Point anchor;
            //double delta;
            int ddepth;
            //int kernel_size;

            // Convert WriteableBitmap to OpenCvSharp.Mat
            OpenCvSharp.Mat srcmat = new OpenCvSharp.Mat();
            OpenCvSharp.Mat dstmat = new OpenCvSharp.Mat();
            srcmat = srcimg.ToMat();
            dstmat = srcmat.Clone();

            // Initialize arguments for the filter
            //anchor = new OpenCvSharp.Point(-1, -1);
            //delta = 0;
            ddepth = -1;
            float[] data = { 0.25F, 0.50F, 0.25F };
            var kernel = new OpenCvSharp.Mat(rows: 3, cols: 1, type: OpenCvSharp.MatType.CV_32FC1, data: data);
            //OpenCvSharp.Cv2.Filter2D(srcmat, dstmat, ddepth, kernel, anchor, delta, OpenCvSharp.BorderTypes.Default);
            OpenCvSharp.Cv2.Filter2D(srcmat, dstmat, ddepth, kernel);
            WriteableBitmap wbResult = dstmat.ToWriteableBitmap();

            return wbResult;
        }
    }
}
