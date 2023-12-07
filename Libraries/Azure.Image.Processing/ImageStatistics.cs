using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Diagnostics;

namespace Azure.Image.Processing
{
    public class ImageStatistics : IImageStatistics
    {
        #region IImageStatistics Members

        /// <summary>
        /// Method to calculate total sum value of all pixels in a given Rect on the image
        /// </summary>
        public unsafe double GetTotalSum(WriteableBitmap srcimg, Rectangle rectROI)
        {
            //int iWidth = srcimg.PixelWidth;
            //int iHeight = srcimg.PixelHeight;
            int iBitsPerPixel = srcimg.Format.BitsPerPixel;

            if ((iBitsPerPixel != 8) && (iBitsPerPixel != 16))
                return 0.0;

            Rectangle rectImageROI = CreateRectangleFromLTRB(srcimg, rectROI);
            if (rectImageROI.IsEmpty)
            {
                return 0.0;
            }

            if (srcimg == null) { return 0.0; }

            // Reserves the back buffer for updates.
            if (!srcimg.IsFrozen)
                srcimg.Lock();

            byte* source = (byte*)srcimg.BackBuffer.ToPointer();
            int iBufferWidth = srcimg.BackBufferStride;

            // Calculate sum
            double totalSum = 0.0;
            for (int i = rectImageROI.Top; i < rectImageROI.Bottom; i++)
            {
                int roiWidth = rectImageROI.Width;
                if (iBitsPerPixel == 8)
                {
                    byte* pSrc = (byte*)source + rectImageROI.Left + i * iBufferWidth;
                    for (int j = 0; j < roiWidth; j++)
                        totalSum += *pSrc++;
                }
                else if (iBitsPerPixel == 16)
                {
                    ushort* pSrc = (ushort*)source + rectImageROI.Left + i * (iBufferWidth / 2);
                    for (int j = 0; j < roiWidth; j++)
                        totalSum += *pSrc++;
                }
            }

            // Releases the back buffer to make it available for display.
            if (!srcimg.IsFrozen)
                srcimg.Unlock();

            return totalSum;
        }

        /// <summary>
        /// Method to find an average value of all pixels in a given ROI on the image:
        /// </summary>
        public double GetAverage(WriteableBitmap srcimg, Rectangle rectROI)
        {
            Rectangle rectImageROI = CreateRectangleFromLTRB(srcimg, rectROI);
            if (rectImageROI.IsEmpty)
                return 0.0;

            double totalSum = GetTotalSum(srcimg, rectImageROI);
            return totalSum / (rectImageROI.Width * rectImageROI.Height);
        }

        public double GetStdDeviation(WriteableBitmap srcimg, Rectangle rectROI)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// Method to get the median value of all pixels in a given ROI on the image.
        /// </summary>
        public unsafe double GetMedian(WriteableBitmap srcimg, Rectangle rectROI)
        {
            int iHeight = srcimg.PixelHeight;
            int iBitsPerPixel = srcimg.Format.BitsPerPixel;

            if ((iBitsPerPixel != 8) && (iBitsPerPixel != 16)) { return 0.0; }

            Rectangle rectImageROI = CreateRectangleFromLTRB(srcimg, rectROI);

            if (rectImageROI.IsEmpty) { return 0.0; }

            int iBufferWidth = srcimg.BackBufferStride;

            if (srcimg == null)
                return 0.0;

            // Reserves the back buffer for updates.
            srcimg.Lock();

            void* source = srcimg.BackBuffer.ToPointer();
            byte* pSource = (byte*)source;

            // iterate all pixels in ROI and store in it array
            int index = 0;
            double[] intensities = new double[rectROI.Width * rectROI.Height];
            for (int i = rectImageROI.Top; i < rectImageROI.Bottom; i++)
            {
                int roiWidth = rectImageROI.Width;
                if (iBitsPerPixel == 8)
                {
                    byte* pSrc = (byte*)source + rectImageROI.Left + i * iBufferWidth;
                    for (int j = 0; j < roiWidth; j++)
                        intensities[index++] = *pSrc++;

                }
                else if (iBitsPerPixel == 16)
                {
                    ushort* pSrc = (ushort*)source + rectImageROI.Left + i * (iBufferWidth / 2);
                    for (int j = 0; j < roiWidth; j++)
                        intensities[index++] = *pSrc++;
                }
            }

            // Releases the back buffer to make it available for display.
            srcimg.Unlock();

            // Get median value
            Array.Sort(intensities);
            if (intensities.Length % 2 == 0) // If even
            {
                int mid = (intensities.Length / 2) - 1;
                return (intensities[mid] + intensities[mid + 1]) / 2.0;
            }
            else
            {
                int mid = ((intensities.Length + 1) / 2) - 1;
                return intensities[mid];
            }
        }

        /// <summary>
        /// Method to find a pixel with maximum value on an image within given region of interest.
        /// Returns pixel coordinates will be relative to image coordinates
        /// </summary>
        public unsafe Point FindMaximumPixel(WriteableBitmap srcimage, Rectangle rectROI)
        {
            int iHeight = srcimage.PixelHeight;
            int iBitsPerPixel = srcimage.Format.BitsPerPixel;
            Point maxPixel = new Point();

            if ((iBitsPerPixel != 8) && (iBitsPerPixel != 16))
                return maxPixel;

            Rectangle rectImageROI = CreateRectangleFromLTRB(srcimage, rectROI);
            if (rectImageROI.IsEmpty)
                return maxPixel;

            if (srcimage == null)
                return maxPixel;

            // Reserves the back buffer for updates.
            if (!srcimage.IsFrozen)
                srcimage.Lock();

            void* source = srcimage.BackBuffer.ToPointer();
            int iBufferWidth = srcimage.BackBufferStride;

            // Find a pixel with maximum value:
            int maxLevel = 0;
            for (int i = rectImageROI.Top; i < rectImageROI.Bottom; i++)
            {
                int roiWidth = rectImageROI.Width;
                if (iBitsPerPixel == 8)
                {
                    byte* pSrc = (byte*)source + rectImageROI.Left + i * iBufferWidth;
                    for (int j = 0; j < roiWidth; j++)
                        if (maxLevel < *pSrc++)
                        {
                            maxLevel = *(pSrc - 1);

                            maxPixel.X = j;
                            maxPixel.Y = i;
                        }
                }
                else if (iBitsPerPixel == 16)
                {
                    ushort* pSrc = (ushort*)source + rectImageROI.Left + i * (iBufferWidth / 2);
                    for (int j = 0; j < roiWidth; j++)
                        if (maxLevel < *pSrc++)
                        {
                            maxLevel = *(pSrc - 1);

                            maxPixel.X = j;
                            maxPixel.Y = i;
                        }
                }
            }

            // Releases the back buffer to make it available for display.
            if (!srcimage.IsFrozen)
                srcimage.Unlock();

            maxPixel.X += rectImageROI.Left;

            return maxPixel;
        }

        /// <summary>
        /// Method to get the minimum pixel intensity value on an image within the given region of interest (ROI).
        /// </summary>
        /// <param name="image"></param>
        /// <param name="rectROI"></param>
        /// <returns></returns>
        public unsafe int GetPixelMin(WriteableBitmap srcimage, Rectangle rectROI)
        {
            int iMinLevel = 65535;
            int iHeight = srcimage.PixelHeight;
            int iBitsPerPixel = srcimage.Format.BitsPerPixel;

            if ((iBitsPerPixel != 8) && (iBitsPerPixel != 16)) { return 0; }

            // region of interest
            Rectangle rectImageROI = CreateRectangleFromLTRB(srcimage, rectROI);

            if (rectImageROI.IsEmpty) { return 0; }

            if (srcimage == null) { return 0; }

            // Reserves the back buffer for updates.
            if (!srcimage.IsFrozen)
                srcimage.Lock();

            void* source = srcimage.BackBuffer.ToPointer();
            int iBufferWidth = srcimage.BackBufferStride;

            // Find a pixel with minimum value
            for (int i = rectImageROI.Top; i < rectImageROI.Bottom; i++)
            {
                int roiWidth = rectImageROI.Width;
                if (iBitsPerPixel == 8)
                {
                    byte* pSrc = (byte*)source + rectImageROI.Left + i * iBufferWidth;
                    for (int j = 0; j < roiWidth; j++)
                    {
                        if (iMinLevel > *pSrc++)
                        {
                            iMinLevel = *(pSrc - 1);
                        }
                    }
                }
                else if (iBitsPerPixel == 16)
                {
                    ushort* pSrc = (ushort*)source + rectImageROI.Left + i * (iBufferWidth / 2);
                    for (int j = 0; j < roiWidth; j++)
                    {
                        if (iMinLevel > *pSrc++)
                        {
                            iMinLevel = *(pSrc - 1);
                        }
                    }
                }
            }

            // Releases the back buffer to make it available for display.
            if (!srcimage.IsFrozen)
                srcimage.Unlock();

            return iMinLevel;
        }

        /// <summary>
        /// Method to get the maximum pixel intensity value on an image within the given region of interest (ROI).
        /// </summary>
        /// <param name="srcimage"></param>
        /// <param name="rectROI"></param>
        /// <returns></returns>
        public unsafe int GetPixelMax(WriteableBitmap srcimage, Rectangle rectROI)
        {
            if (srcimage == null) { return 0; }

            int iMaxLevel = 0;
            int iHeight = srcimage.PixelHeight;
            int iBitsPerPixel = srcimage.Format.BitsPerPixel;

            if ((iBitsPerPixel != 8) && (iBitsPerPixel != 16)) { return iMaxLevel; }

            Rectangle rectImageROI = CreateRectangleFromLTRB(srcimage, rectROI);
            
            if (rectImageROI.IsEmpty ||
                rectImageROI.Width > srcimage.PixelWidth || rectImageROI.Height > srcimage.PixelHeight ||
                rectImageROI.Width < 1 || rectImageROI.Height < 1)
            {
                return iMaxLevel;
            }

            if (srcimage == null) { return iMaxLevel; }

            // Reserve the back buffer for updates.
            if (!srcimage.IsFrozen)
                srcimage.Lock();

            void* source = srcimage.BackBuffer.ToPointer();
            int iBufferWidth = srcimage.BackBufferStride;

            // Find a pixel with maximum value
            for (int i = rectImageROI.Top; i < rectImageROI.Bottom; i++)
            {
                int roiWidth = rectImageROI.Width;
                if (iBitsPerPixel == 8)
                {
                    byte* pSrc = (byte*)source + rectImageROI.Left + i * iBufferWidth;
                    for (int j = 0; j < roiWidth; j++)
                    {
                        if (iMaxLevel < *pSrc++)
                        {
                            iMaxLevel = *(pSrc - 1);
                        }
                    }
                }
                else if (iBitsPerPixel == 16)
                {
                    ushort* pSrc = (ushort*)source + rectImageROI.Left + i * (iBufferWidth / 2);
                    for (int j = 0; j < roiWidth; j++)
                    {
                        if (iMaxLevel < *pSrc++)
                        {
                            iMaxLevel = *(pSrc - 1);
                        }
                    }
                }
            }

            // Release the back buffer and make it available for display.
            if (!srcimage.IsFrozen)
                srcimage.Unlock();

            return iMaxLevel;
        }

        #endregion

        // Helper method
        private Rectangle CreateRectangleFromLTRB(WriteableBitmap image, Rectangle rectROI)
        {
            Rectangle rectImageROI = Rectangle.FromLTRB(0, 0, image.PixelWidth, image.PixelHeight);

            if (rectROI.IsEmpty)
            {
                return rectImageROI;
            }

            rectImageROI.Intersect(rectROI);

            return rectImageROI;
        }

        /// <summary>
        /// Fits a line to a collection of (x,y) points.
        /// </summary>
        /// <param name="xVals">The x-axis values.</param>
        /// <param name="yVals">The y-axis values.</param>
        /// <param name="inclusiveStart">The inclusive inclusiveStart index.</param>
        /// <param name="exclusiveEnd">The exclusive exclusiveEnd index.</param>
        /// <param name="rsquared">The r^2 value of the line.</param>
        /// <param name="yintercept">The y-intercept value of the line (i.e. y = ax + b, yintercept is b).</param>
        /// <param name="slope">The slop of the line (i.e. y = ax + b, slope is a).</param>
        public void LinearRegression(double[] xVals, double[] yVals,
                                     int inclusiveStart, int exclusiveEnd,
                                     out double rsquared, out double yintercept,
                                     out double slope)
        {
            Debug.Assert(xVals.Length == yVals.Length);
            double sumOfX = 0;
            double sumOfY = 0;
            double sumOfXSq = 0;
            double sumOfYSq = 0;
            double ssX = 0;
            double ssY = 0;
            double sumCodeviates = 0;
            double sCo = 0;
            double count = exclusiveEnd - inclusiveStart;

            for (int ctr = inclusiveStart; ctr < exclusiveEnd; ctr++)
            {
                double x = xVals[ctr];
                double y = yVals[ctr];
                sumCodeviates += x * y;
                sumOfX += x;
                sumOfY += y;
                sumOfXSq += x * x;
                sumOfYSq += y * y;
            }
            ssX = sumOfXSq - ((sumOfX * sumOfX) / count);
            ssY = sumOfYSq - ((sumOfY * sumOfY) / count);
            double RNumerator = (count * sumCodeviates) - (sumOfX * sumOfY);
            double RDenom = (count * sumOfXSq - (sumOfX * sumOfX))
             * (count * sumOfYSq - (sumOfY * sumOfY));
            sCo = sumCodeviates - ((sumOfX * sumOfY) / count);

            double meanX = sumOfX / count;
            double meanY = sumOfY / count;
            double dblR = RNumerator / Math.Sqrt(RDenom);
            rsquared = dblR * dblR;
            yintercept = meanY - ((sCo / ssX) * meanX);
            slope = sCo / ssX;
        }

    }
}
