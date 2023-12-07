using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Media.Imaging;


namespace Azure.Image.Processing
{
    public interface IImageStatistics
    {
        double GetTotalSum(WriteableBitmap image, Rectangle roi);

        double GetAverage(WriteableBitmap image, Rectangle roi);

        double GetStdDeviation(WriteableBitmap image, Rectangle roi);

        double GetMedian(WriteableBitmap image, Rectangle roi);

        int GetPixelMin(WriteableBitmap image, Rectangle rectROI);

        int GetPixelMax(WriteableBitmap image, Rectangle roi);
    }
}
