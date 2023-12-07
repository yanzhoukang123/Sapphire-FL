using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging; //WriteableBitmap
using System.Windows.Media; //PixelFormats
using Azure.EthernetCommLib;
using Azure.Image.Processing;

namespace Azure.ImagingSystem
{
    public class ImagingHelper
    {
        public static void Delay(int msec)
        {
            try
            {
                DateTime current = DateTime.Now;

                while (current.AddMilliseconds(msec) > DateTime.Now)
                {
                    System.Windows.Forms.Application.DoEvents();
                }
            }
            catch
            {
                System.Threading.Thread.Sleep(msec);
            }
            return;
        }

        /// <summary>
        /// Sapphire image channel alignment [original Sapphire].
        /// Align the source images to the reference (the laser C/Red channel) image.
        /// </summary>
        /// <param name="imagesUnaligned"></param>
        /// <param name="imagesAligned"></param>
        /// <param name="deltaX">the x-axis offset of other channels to channel C</param>
        /// <param name="deltaY">the y-axis offset of other channels to channel C</param>
        public static unsafe void AlignImage(byte*[] imagesUnaligned, byte*[] imagesAligned, int[] deltaX, int[] deltaY, int imageWidth, int imageHeight)
        {
            int positiveDeltaXMax = 0;
            int negativeDeltaXMax = 0;
            int positiveDeltaYMax = 0;
            int negativeDeltaYMax = 0;

            positiveDeltaXMax = deltaX.Max();
            if (positiveDeltaXMax < 0)
            {
                positiveDeltaXMax = 0;
            }
            negativeDeltaXMax = deltaX.Min();
            if (negativeDeltaXMax > 0)
            {
                negativeDeltaXMax = 0;
            }

            positiveDeltaYMax = deltaY.Max();
            if (positiveDeltaYMax < 0)
            {
                positiveDeltaYMax = 0;
            }
            negativeDeltaYMax = deltaY.Min();
            if (negativeDeltaYMax > 0)
            {
                negativeDeltaYMax = 0;
            }

            int destImagePixelWidth = (int)imageWidth + negativeDeltaXMax - positiveDeltaXMax;
            int destImagePixelHeight = (int)imageHeight + negativeDeltaYMax - positiveDeltaYMax;

            if (destImagePixelWidth != imageWidth || destImagePixelHeight != imageHeight)
            {
                int fixSourceImageWidthNoDouble = 0;
                int fixDestImageWidthNoDouble = 0;
                if (imageWidth % 2 != 0)
                {
                    fixSourceImageWidthNoDouble = 1;
                }
                if (destImagePixelWidth % 2 != 0)
                {
                    fixDestImageWidthNoDouble = 1;
                }

                unsafe
                {
                    // reference image: laser c/red channel
                    if (imagesUnaligned[2] != null)
                    {
                        var sourcePtr = (UInt16*)imagesUnaligned[2];
                        var destPtr = (UInt16*)imagesAligned[2];

                        for (int y = 0; y < destImagePixelHeight; y++)
                        {
                            for (int x = 0; x < destImagePixelWidth; x++)
                            {
                                destPtr[y * (destImagePixelWidth + fixDestImageWidthNoDouble) + x] = sourcePtr[(y + positiveDeltaYMax) * (imageWidth + fixSourceImageWidthNoDouble) + (x + positiveDeltaXMax)];
                            }
                        }
                    }
                    for (int i = 0; i < imagesUnaligned.Length; i++)
                    {
                        // skip reference image (already processed).
                        if (imagesUnaligned[i] != null && i != 2)
                        {
                            var sourcePtr = (UInt16*)imagesUnaligned[i];
                            var destPtr = (UInt16*)imagesAligned[i];

                            for (int y = 0; y < destImagePixelHeight; y++)
                            {
                                for (int x = 0; x < destImagePixelWidth; x++)
                                {
                                    destPtr[y * (destImagePixelWidth + fixDestImageWidthNoDouble) + x] =
                                        sourcePtr[(y + positiveDeltaYMax - deltaY[i]) * (imageWidth + fixSourceImageWidthNoDouble) + (x + positiveDeltaXMax - deltaX[i])];
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < imagesUnaligned.Length; i++)
                {
                    if (imagesUnaligned[i] != null && imagesAligned[i] != null)
                    {
                        imagesAligned[i] = imagesUnaligned[i];
                    }
                }
            }
        }

        /// <summary>
        /// Sapphire FL image channel pixel alignment
        /// </summary>
        /// <param name="srcimg"></param>
        /// <param name="alignParam"></param>
        public static void AlignImage(ref WriteableBitmap srcimg, ImageAlignParam alignParam)
        {
            int resolution = alignParam.Resolution;
            double resolutionMove = resolution / 10.0;   //MoveImageResolution
            //像素左右偏移参数值
            //Pixel left and right offset parameter value
            int pixelOddX = alignParam.PixelOddX;   //X odd Line
            int pixelEvenX = alignParam.PixelEvenX; //X even Line
            int pixelOddY = alignParam.PixelOddY;   //Y odd Line
            int pixelEvenY = alignParam.PixelEvenY; //Y Even Line
            double yCompOffset = alignParam.YCompOffset;
            //Y轴截取像素前几行
            ////Y-axis intercepts the first few rows of pixels
            int Y_MovePixelDY = (int)(yCompOffset * 1000.0 / resolution);
            LaserChannels laserChannel = alignParam.LaserChannel;

            try
            {
                //（因为在设计中将R1透镜的位置和R2透镜的位置更换了，然后参数名没有更改成对应的通道，所以下面参数名R1其实是R2，R2其实是R1）....
                // (in the design, the position of R1 lens and R2 lens is changed, and the parameter name is not changed to the corresponding channel, so the following parameter name R1 is actually R2, R2 is actually R1)....
                if (laserChannel == LaserChannels.ChannelC)         //L1
                {
                    #region L1/channel C laser
                    if (alignParam.IsPixelOffsetProcessing)
                    {
                        if (resolution < alignParam.PixelOffsetProcessingRes)
                        {
                            if (pixelOddX != 0 || pixelEvenX != 0)
                            {
                                //处理高分辨率图片出现的锯齿
                                //Handle the sawtooth caused by high resolution
                                //FileViewModel.UpdatePixelSingleMoveDisplayImage(ref image, pixelOddx, pixelEvenx, pixelOddy, pixelEveny);
                                ImageProcessingHelper.UpdatePixelSingleMoveDisplayImage(ref srcimg, pixelOddX, pixelEvenX, pixelOddY, pixelEvenY);
                            }
                        }
                    }
                    if (alignParam.IsImageOffsetProcessing)
                    {
                        //根据当前分辨率计算L到R2之间需要截取掉多少列像素
                        //Calculate how many columns of pixels need to be intercepted between L and R2 according to the current resolution
                        int leffectivePixel = (int)(alignParam.OpticalL_R1Distance * 1000.0 / resolution);
                        if (leffectivePixel != 0)
                        {
                            //将图片从左到右截取的像素列，保持图片位置对齐
                            //The pixel column of the picture taken from left to right, keeping the position aligned with the picture
                            //FileViewModel.UpdatePixeleffectiveMoveDisplayImage(ref image, LeffectivePixel);//image effective
                            ImageProcessingHelper.UpdatePixeleffectiveMoveDisplayImage(ref srcimg, leffectivePixel);
                        }
                        //图片像素向右或者向下移动
                        //Move the picture pixel to the right or down
                        int lMovePixelDX = (int)(alignParam.Pixel_10_L_DX / resolutionMove);
                        int lMovePixelDY = (int)(alignParam.Pixel_10_L_DY / resolutionMove);
                        if (lMovePixelDX != 0 || lMovePixelDY != 0)
                        {
                            //FileViewModel.UpdatePixelSingleMoveDisplayImage(ref image, LMovePixelDX, LMovePixelDX, LMovePixelDY, LMovePixelDY);//Align the three images
                            ImageProcessingHelper.UpdatePixelSingleMoveDisplayImage(ref srcimg, lMovePixelDX, lMovePixelDX, lMovePixelDY, lMovePixelDY);
                        }
                    }
                    if (alignParam.IsYCompensationBitAt && (int)(yCompOffset * 1000) / resolution >= 1)
                    {
                        //去掉图像前几行，避免图片前几行造成的倾斜
                        //Remove the first few lines of the image to avoid the tilt caused by the first few lines of the image
                        //FileViewModel.YAxisUpdatePixeleffectiveMoveDisplayImage(ref image, Y_MovePixelDY);
                        ImageProcessingHelper.YAxisUpdatePixeleffectiveMoveDisplayImage(ref srcimg, Y_MovePixelDY);
                    }
                    #endregion
                }
                else if (laserChannel == LaserChannels.ChannelA)    //R1
                {
                    #region R1/channel A laser
                    if (alignParam.IsPixelOffsetProcessing)
                    {
                        if (resolution < alignParam.PixelOffsetProcessingRes)
                        {
                            if (pixelOddX != 0 || pixelEvenX != 0)
                            {
                                //处理高分辨率图片出现的锯齿
                                //Handle the sawtooth caused by high resolution
                                //FileViewModel.UpdatePixelSingleMoveDisplayImage(ref image, pixelOddX, pixelEvenX, pixelOddY, pixelEvenY);//Handle the sawtooth caused by high resolution
                                ImageProcessingHelper.UpdatePixelSingleMoveDisplayImage(ref srcimg, pixelOddX, pixelEvenX, pixelOddY, pixelEvenY);
                            }
                        }
                    }
                    if (alignParam.IsImageOffsetProcessing)
                    {
                        //根据当前分辨率计算R1到R2之间需要截取掉的像素列
                        //Calculate the pixel column between R1 and R2 that needs to be intercepted according to the current resolution
                        int R2MovePixelX = (int)(alignParam.OpticalR2_R1Distance * 1000.0 / resolution);
                        int R2MovePixelY = 0;
                        int R2effectivePixel = R2MovePixelX * 2;
                        if (R2MovePixelX != 0)
                        {
                            //将图片向右移动R1到R2透镜之间的距离，保持图像里内容对齐
                            //The pixel column of the picture taken from left to right, keeping the position aligned with the picture
                            //FileViewModel.UpdatePixelSingleMoveDisplayImage(ref image, R2MovePixelX, R2MovePixelX, R2MovePixelY, R2MovePixelY);//image move
                            ImageProcessingHelper.UpdatePixelSingleMoveDisplayImage(ref srcimg, R2MovePixelX, R2MovePixelX, R2MovePixelY, R2MovePixelY);
                            //将图片从左到右截取掉多补偿的宽度部分
                            //Cut off the multi-compensated width of the picture from left to right
                            //FileViewModel.UpdatePixeleffectiveMoveDisplayImage(ref image, R2effectivePixel);//image effective
                            ImageProcessingHelper.UpdatePixeleffectiveMoveDisplayImage(ref srcimg, R2effectivePixel);
                        }
                        //图片像素向右或者向下移动
                        ////Move the picture pixel to the right or down
                        int R2PixelDX = (int)(alignParam.Pixel_10_R2_DX / resolutionMove);
                        int R2PixelDY = (int)(alignParam.Pixel_10_R2_DY / resolutionMove);
                        if (R2PixelDX != 0 || R2PixelDY != 0)
                        {
                            //FileViewModel.UpdatePixelSingleMoveDisplayImage(ref image, R2PixelDX, R2PixelDX, R2PixelDY, R2PixelDY);//image move
                            ImageProcessingHelper.UpdatePixelSingleMoveDisplayImage(ref srcimg, R2PixelDX, R2PixelDX, R2PixelDY, R2PixelDY);
                        }
                    }
                    if (alignParam.IsYCompensationBitAt && (int)(yCompOffset * 1000.0 / resolution) >= 1)
                    {
                        //去掉图像前几行，避免图片前几行造成的倾斜
                        //Remove the first few lines of the image to avoid the tilt caused by the first few lines of the image
                        //FileViewModel.YAxisUpdatePixeleffectiveMoveDisplayImage(ref image, Y_MovePixelDY);
                        ImageProcessingHelper.YAxisUpdatePixeleffectiveMoveDisplayImage(ref srcimg, Y_MovePixelDY);
                    }
                    #endregion
                }
                else if (laserChannel == LaserChannels.ChannelB)    //R2
                {
                    #region R2/channel B laser
                    if (alignParam.IsPixelOffsetProcessing)
                    {
                        if (resolution < alignParam.PixelOffsetProcessingRes)
                        {
                            if (pixelOddX != 0 || pixelEvenX != 0)
                            {
                                //处理高分辨率图片出现的锯齿
                                //Handle the sawtooth caused by high resolution
                                //FileViewModel.UpdatePixelSingleMoveDisplayImage(ref image, pixelOddX, pixelEvenX, pixelOddY, pixelEvenY);//Handle the sawtooth caused by high resolution
                                ImageProcessingHelper.UpdatePixelSingleMoveDisplayImage(ref srcimg, pixelOddX, pixelEvenX, pixelOddY, pixelEvenY);
                            }
                        }
                    }
                    if (alignParam.IsImageOffsetProcessing)
                    {
                        //根据当前分辨率计算L到R2之间需要截取掉多少列像素
                        //Calculate how many columns of pixels need to be intercepted between L and R2 according to the current resolution
                        int R1MovePixelDX = (int)(alignParam.OpticalL_R1Distance * 1000.0 / resolution);
                        int R1MovePixelDY = 0;
                        if (R1MovePixelDX != 0)
                        {
                            //将图片向右移动L到R2透镜之间的距离，保持图像里内容对齐
                            //The pixel column of the picture taken from left to right, keeping the position aligned with the picture
                            //FileViewModel.UpdatePixelSingleMoveDisplayImage(ref image, R1MovePixelDX, R1MovePixelDX, R1MovePixelDY, R1MovePixelDY);
                            ImageProcessingHelper.UpdatePixelSingleMoveDisplayImage(ref srcimg, R1MovePixelDX, R1MovePixelDX, R1MovePixelDY, R1MovePixelDY);
                            //将图片从左到右截取掉多补偿的宽度部分
                            //Cut off the multi-compensated width of the picture from left to right
                            //FileViewModel.UpdatePixeleffectiveMoveDisplayImage(ref image, R1MovePixelDX);
                            ImageProcessingHelper.UpdatePixeleffectiveMoveDisplayImage(ref srcimg, R1MovePixelDX);
                        }
                    }
                    if (alignParam.IsYCompensationBitAt && (int)(yCompOffset * 1000.0 / resolution) >= 1)
                    {
                        //去掉图像前几行，避免图片前几行造成的倾斜
                        //Remove the first few lines of the image to avoid the tilt caused by the first few lines of the image
                        //FileViewModel.YAxisUpdatePixeleffectiveMoveDisplayImage(ref image, Y_MovePixelDY);//Cut out the first few lines of the X-axis
                        ImageProcessingHelper.YAxisUpdatePixeleffectiveMoveDisplayImage(ref srcimg, Y_MovePixelDY);
                    }
                    #endregion
                }
            }
            catch
            {
                //StopWaitAnimation();
                //_IsLoading = false;
            }
        }

        public static WriteableBitmap SFLImageAlign(WriteableBitmap srcimg, ImageAlignParam alignParam)
        {
            int pixelWidth = srcimg.PixelWidth;
            int pixelHeight = srcimg.PixelHeight;
            int scanResolution = alignParam.Resolution;
            int overscanInPixels = (int)(alignParam.YMotionExtraMoveLength / 2.0 * 1000.0 / scanResolution);
            double pixel10Dist = scanResolution / 10.0;
            LaserChannels laserChannel = alignParam.LaserChannel;

            try
            {
                #region  'Sawtooth' correction
                if (alignParam.IsPixelOffsetProcessing)
                {
                    if (scanResolution < alignParam.PixelOffsetProcessingRes)
                    {
                        if (alignParam.PixelOddX != 0 || alignParam.PixelEvenX != 0)
                        {
                            if (alignParam.PixelOddX != 0)
                            {
                                ImageProcessing.OddLineShiftCol(ref srcimg, alignParam.PixelOddX);
                            }
                        }
                    }
                }
                #endregion

                if (laserChannel == LaserChannels.ChannelC)         //L1
                {
                    int opticalDist = (int)(alignParam.OpticalL_R1Distance * 1000.0 / scanResolution);
                    int pixel10DX = opticalDist - (int)(alignParam.Pixel_10_L_DX / pixel10Dist);
                    int pixel10DY = overscanInPixels - (int)(alignParam.Pixel_10_L_DY / pixel10Dist);

                    if (opticalDist != 0 && (pixel10DX != 0 || pixel10DY != 0))
                    {
                        int x = pixel10DX;
                        int y = pixel10DY;
                        // X overscan compensation (using the same amount of overscan for X and Y direction)
                        int width = pixelWidth - opticalDist - (int)(alignParam.YMotionExtraMoveLength * 1000.0 / scanResolution);
                        //int width = pixelWidth - opticalDist;
                        int height = pixelHeight - (int)(alignParam.YMotionExtraMoveLength * 1000.0 / scanResolution);
                        // Currently we're not over-scanning vertically in Phosphor Imaging.
                        if (y + height > pixelHeight)
                        {
                            y -= ((y + height) - pixelHeight);
                        }
                        if (x < 0) { x = 0; }
                        if (y < 0) { y = 0; }
                        srcimg = ImageProcessing.Crop(srcimg, new System.Windows.Rect(x, y, width, height));
                    }
                }
                else if (laserChannel == LaserChannels.ChannelA)    //R1
                {
                    int opticalDist = (int)(alignParam.OpticalR2_R1Distance * 1000.0 / scanResolution);
                    int pixel10DX = opticalDist - (int)(alignParam.Pixel_10_R2_DX / pixel10Dist);
                    int pixel10DY = overscanInPixels - (int)(alignParam.Pixel_10_R2_DY / pixel10Dist);
                    if (opticalDist != 0 && (pixel10DX != 0 || pixel10DY != 0))
                    {
                        int x = pixel10DX;
                        int y = pixel10DY;
                        // X overscan compensation (using the same amount of overscan for X and Y direction)
                        int width = pixelWidth - (opticalDist * 2) - (int)(alignParam.YMotionExtraMoveLength * 1000.0 / scanResolution);
                        //int width = pixelWidth - (opticalDist * 2);
                        int height = pixelHeight - (int)(alignParam.YMotionExtraMoveLength * 1000.0 / scanResolution);
                        if (y + height > pixelHeight)
                        {
                            y -= ((y + height) - pixelHeight);
                        }
                        if (x < 0) { x = 0; }
                        if (y < 0) { y = 0; }
                        srcimg = ImageProcessing.Crop(srcimg, new System.Windows.Rect(x, y, width, height));
                    }
                }
                else if (laserChannel == LaserChannels.ChannelB)    //R2
                {
                    int opticalDist = (int)(alignParam.OpticalL_R1Distance * 1000.0 / scanResolution);
                    int pixel10DX = 0;
                    int pixel10DY = overscanInPixels;
                    if (opticalDist != 0)
                    {
                        int x = pixel10DX;
                        int y = pixel10DY;
                        // X overscan compensation (using the same amount of overscan for X and Y direction)
                        int width = pixelWidth - opticalDist - (int)(alignParam.YMotionExtraMoveLength * 1000.0 / scanResolution);
                        //int width = pixelWidth - opticalDist;
                        int height = pixelHeight - (int)(alignParam.YMotionExtraMoveLength * 1000.0 / scanResolution);
                        if (y + height > pixelHeight)
                        {
                            y -= ((y + height) - pixelHeight);
                        }
                        if (x < 0) { x = 0; }
                        if (y < 0) { y = 0; }
                        srcimg = ImageProcessing.Crop(srcimg, new System.Windows.Rect(x, y, width, height));
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
            }

            return srcimg;
        }

        public static void SFLImageAlign(ref WriteableBitmap srcimg, ref WriteableBitmap dstimg, ImageAlignParam alignParam)
        {
            int pixelWidth = srcimg.PixelWidth;
            int pixelHeight = srcimg.PixelHeight;
            int scanResolution = alignParam.Resolution;
            int nYOverscanInPixels = (int)(alignParam.YMotionExtraMoveLength / 2.0 * 1000.0 / scanResolution);
            double pixel10Dist = scanResolution / 10.0;
            LaserChannels laserChannel = alignParam.LaserChannel;

            try
            {
                #region  'Sawtooth' correction
                if (alignParam.IsPixelOffsetProcessing)
                {
                    if (scanResolution < alignParam.PixelOffsetProcessingRes)
                    {
                        if (alignParam.PixelOddX != 0 || alignParam.PixelEvenX != 0)
                        {
                            if (alignParam.PixelOddX != 0)
                            {
                                ImageProcessing.OddLineShiftCol(ref srcimg, alignParam.PixelOddX);
                            }
                        }
                    }
                }
                #endregion

                if (laserChannel == LaserChannels.ChannelC)         //L1
                {
                    int opticalDist = (int)(alignParam.OpticalL_R1Distance * 1000.0 / scanResolution);
                    int pixel10DX = opticalDist - (int)(alignParam.Pixel_10_L_DX / pixel10Dist);
                    int pixel10DY = nYOverscanInPixels - (int)(alignParam.Pixel_10_L_DY / pixel10Dist);

                    if (opticalDist != 0 && (pixel10DX != 0 || pixel10DY != 0))
                    {
                        int x = pixel10DX;
                        int y = pixel10DY;
                        // X overscan compensation
                        //int width = pixelWidth - opticalDist - (int)(alignParam.XMotionExtraMoveLength * 1000.0 / scanResolution);
                        int width = pixelWidth - opticalDist;
                        int height = pixelHeight - (int)(alignParam.YMotionExtraMoveLength * 1000.0 / scanResolution);
                        // Currently we're not over-scanning vertically in Phosphor Imaging.
                        if (y + height > pixelHeight)
                        {
                            y -= ((y + height) - pixelHeight);
                        }
                        if (x < 0) { x = 0; }
                        if (y < 0) { y = 0; }
                        dstimg = ImageProcessing.Crop(srcimg, new System.Windows.Rect(x, y, width, height));
                    }
                }
                else if (laserChannel == LaserChannels.ChannelA)    //R1
                {
                    int opticalDist = (int)(alignParam.OpticalR2_R1Distance * 1000.0 / scanResolution);
                    int pixel10DX = opticalDist - (int)(alignParam.Pixel_10_R2_DX / pixel10Dist);
                    int pixel10DY = nYOverscanInPixels - (int)(alignParam.Pixel_10_R2_DY / pixel10Dist);
                    if (opticalDist != 0 && (pixel10DX != 0 || pixel10DY != 0))
                    {
                        int x = pixel10DX;
                        int y = pixel10DY;
                        // X overscan compensation
                        //int width = pixelWidth - (opticalDist * 2) - (int)(alignParam.XMotionExtraMoveLength * 1000.0 / scanResolution);
                        int width = pixelWidth - (opticalDist * 2);
                        int height = pixelHeight - (int)(alignParam.YMotionExtraMoveLength * 1000.0 / scanResolution);
                        if (y + height > pixelHeight)
                        {
                            y -= ((y + height) - pixelHeight);
                        }
                        if (x < 0) { x = 0; }
                        if (y < 0) { y = 0; }
                        dstimg = ImageProcessing.Crop(srcimg, new System.Windows.Rect(x, y, width, height));
                    }
                }
                else if (laserChannel == LaserChannels.ChannelB)    //R2
                {
                    int opticalDist = (int)(alignParam.OpticalL_R1Distance * 1000.0 / scanResolution);
                    int pixel10DX = 0;
                    int pixel10DY = nYOverscanInPixels;
                    if (opticalDist != 0)
                    {
                        int x = pixel10DX;
                        int y = pixel10DY;
                        // X overscan compensation
                        //int width = pixelWidth - opticalDist - (int)(alignParam.XMotionExtraMoveLength * 1000.0 / scanResolution);
                        int width = pixelWidth - opticalDist;
                        int height = pixelHeight - (int)(alignParam.YMotionExtraMoveLength * 1000.0 / scanResolution);
                        if (y + height > pixelHeight)
                        {
                            y -= ((y + height) - pixelHeight);
                        }
                        if (x < 0) { x = 0; }
                        if (y < 0) { y = 0; }
                        srcimg = ImageProcessing.Crop(srcimg, new System.Windows.Rect(x, y, width, height));
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
            }
        }

        public static unsafe void SFLImageAlign(byte* psrcimg, int srcwidth, int srcheight, int srcbufferstride,
                                                byte* pdstimg, int dstwidth, int dstheight, int dstbufferstride,
                                                PixelFormat format, ImageAlignParam alignParam)
        {
            int scanResolution = alignParam.Resolution;
            int nYOverscanInPixels = (int)(alignParam.YMotionExtraMoveLength / 2.0 * 1000.0 / scanResolution);
            double pixel10Dist = scanResolution / 10.0;
            LaserChannels laserChannel = alignParam.LaserChannel;

            try
            {
                #region  'Sawtooth' correction
                //if (alignParam.IsPixelOffsetProcessing)
                //{
                //    if (scanResolution < alignParam.PixelOffsetProcessingRes)
                //    {
                //        if (alignParam.PixelOddX != 0 || alignParam.PixelEvenX != 0)
                //        {
                //            if (alignParam.PixelOddX != 0)
                //            {
                //                ImageProcessing.OddLineShiftCol(psrcimg, srcwidth, srcheight, srcbufferstride, alignParam.PixelOddX);
                //            }
                //        }
                //    }
                //}
                #endregion

                if (laserChannel == LaserChannels.ChannelC)         //L1
                {
                    int opticalDist = (int)(alignParam.OpticalL_R1Distance * 1000.0 / scanResolution);
                    int pixel10DX = opticalDist - (int)(alignParam.Pixel_10_L_DX / pixel10Dist);
                    int pixel10DY = nYOverscanInPixels - (int)(alignParam.Pixel_10_L_DY / pixel10Dist);

                    if (opticalDist != 0 && (pixel10DX != 0 || pixel10DY != 0))
                    {
                        int x = pixel10DX;
                        int y = pixel10DY;
                        // X overscan compensation (using the same amount of overscan for X and Y direction)
                        int width = srcwidth - opticalDist - (int)(alignParam.YMotionExtraMoveLength * 1000.0 / scanResolution);
                        //int width = srcwidth - opticalDist;
                        int height = srcheight - (int)(alignParam.YMotionExtraMoveLength * 1000.0 / scanResolution);
                        // Currently we're not over-scanning vertically in Phosphor Imaging.
                        if (y + height > srcheight)
                        {
                            y -= ((y + height) - srcheight);
                        }
                        if (x < 0) { x = 0; }
                        if (y < 0) { y = 0; }
                        ImageProcessing.Crop(psrcimg, srcbufferstride, pdstimg, dstbufferstride, format, new System.Windows.Rect(x, y, width, height));
                    }
                }
                else if (laserChannel == LaserChannels.ChannelA)    //R1
                {
                    int opticalDist = (int)(alignParam.OpticalR2_R1Distance * 1000.0 / scanResolution);
                    int pixel10DX = opticalDist - (int)(alignParam.Pixel_10_R2_DX / pixel10Dist);
                    int pixel10DY = nYOverscanInPixels - (int)(alignParam.Pixel_10_R2_DY / pixel10Dist);
                    if (opticalDist != 0 && (pixel10DX != 0 || pixel10DY != 0))
                    {
                        int x = pixel10DX;
                        int y = pixel10DY;
                        // X overscan compensation (using the same amount of overscan for X and Y direction)
                        int width = srcwidth - (opticalDist * 2) - (int)(alignParam.YMotionExtraMoveLength * 1000.0 / scanResolution);
                        //int width = srcwidth - (opticalDist * 2);
                        int height = srcheight - (int)(alignParam.YMotionExtraMoveLength * 1000.0 / scanResolution);
                        if (y + height > srcheight)
                        {
                            y -= ((y + height) - srcheight);
                        }
                        if (x < 0) { x = 0; }
                        if (y < 0) { y = 0; }
                        ImageProcessing.Crop(psrcimg, srcbufferstride, pdstimg, dstbufferstride, format, new System.Windows.Rect(x, y, width, height));
                    }
                }
                else if (laserChannel == LaserChannels.ChannelB)    //R2
                {
                    int opticalDist = (int)(alignParam.OpticalL_R1Distance * 1000.0 / scanResolution);
                    int pixel10DX = 0;
                    int pixel10DY = nYOverscanInPixels;
                    if (opticalDist != 0)
                    {
                        int x = pixel10DX;
                        int y = pixel10DY;
                        // X overscan compensation (using the same amount of overscan for X and Y direction)
                        int width = srcwidth - opticalDist - (int)(alignParam.YMotionExtraMoveLength * 1000.0 / scanResolution);
                        //int width = srcwidth - opticalDist;
                        int height = srcheight - (int)(alignParam.YMotionExtraMoveLength * 1000.0 / scanResolution);
                        if (y + height > srcheight)
                        {
                            y -= ((y + height) - srcheight);
                        }
                        if (x < 0) { x = 0; }
                        if (y < 0) { y = 0; }
                        ImageProcessing.Crop(psrcimg, srcbufferstride, pdstimg, dstbufferstride, format, new System.Windows.Rect(x, y, width, height));
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Sapphire FL image channel pixel alignment
        /// </summary>
        /// <param name="srcimg"></param>
        /// <param name="alignParam"></param>
        /*public static void AlignImage(ref WriteableBitmap srcimg, ref WriteableBitmap dstimg, ImageAlignParam alignParam)
        {
            int resolution = alignParam.Resolution;
            double resolutionMove = resolution / 10.0;   //MoveImageResolution
            //像素左右偏移参数值
            //Pixel left and right offset parameter value
            int pixelOddX = alignParam.PixelOddX;   //X odd Line
            int pixelEvenX = alignParam.PixelEvenX; //X even Line
            int pixelOddY = alignParam.PixelOddY;   //Y odd Line
            int pixelEvenY = alignParam.PixelEvenY; //Y Even Line
            double yCompOffset = alignParam.YCompOffset;
            //Y轴截取像素前几行
            ////Y-axis intercepts the first few rows of pixels
            int Y_MovePixelDY = (int)(yCompOffset * 1000.0 / resolution);
            var laserChannel = alignParam.LaserChannel;

            try
            {
                //（因为在设计中将R1透镜的位置和R2透镜的位置更换了，然后参数名没有更改成对应的通道，所以下面参数名R1其实是R2，R2其实是R1）....
                // (in the design, the position of R1 lens and R2 lens is changed, and the parameter name is not changed to the corresponding channel, so the following parameter name R1 is actually R2, R2 is actually R1)....
                if (laserChannel == LaserChannels.ChannelC)         //L1
                {
                    #region L1/channel C laser
                    //根据当前分辨率计算L到R2之间需要截取掉多少列像素
                    //Calculate how many columns of pixels need to be intercepted between L and R2 according to the current resolution
                    int leffectivePixel = (int)(alignParam.OpticalL_R1Distance * 1000.0 / resolution);
                    if (leffectivePixel != 0)
                    {
                        //将图片从左到右截取的像素列，保持图片位置对齐
                        //The pixel column of the picture taken from left to right, keeping the position aligned with the picture
                        //FileViewModel.UpdatePixeleffectiveMoveDisplayImage(ref image, LeffectivePixel);//image effective
                        ImageProcessingHelper.UpdatePixeleffectiveMoveDisplayImage(ref srcimg, ref dstimg, leffectivePixel);    //result in dstimg
                    }
                    else
                    {
                        dstimg = srcimg;
                    }
                    if (alignParam.IsImageOffsetProcessing)
                    {
                        //图片像素向右或者向下移动
                        //Move the picture pixel to the right or down
                        int lMovePixelDX = (int)(alignParam.Pixel_10_L_DX / resolutionMove);
                        int lMovePixelDY = (int)(alignParam.Pixel_10_L_DY / resolutionMove);
                        if (lMovePixelDX != 0 || lMovePixelDY != 0)
                        {
                            //FileViewModel.UpdatePixelSingleMoveDisplayImage(ref image, LMovePixelDX, LMovePixelDX, LMovePixelDY, LMovePixelDY);//Align the three images
                            ImageProcessingHelper.UpdatePixelSingleMoveDisplayImage(ref dstimg, lMovePixelDX, lMovePixelDX, lMovePixelDY, lMovePixelDY);
                        }
                    }

                    if (alignParam.IsPixelOffsetProcessing)
                    {
                        if (resolution < 50)
                        {
                            //处理高分辨率图片出现的锯齿
                            //Handle the sawtooth caused by high resolution
                            //FileViewModel.UpdatePixelSingleMoveDisplayImage(ref image, pixelOddx, pixelEvenx, pixelOddy, pixelEveny);
                            ImageProcessingHelper.UpdatePixelSingleMoveDisplayImage(ref dstimg, pixelOddX, pixelEvenX, pixelOddY, pixelEvenY);
                        }
                    }
                    if (alignParam.IsYCompensationBitAt && (int)(yCompOffset * 1000.0 / resolution) >= 1)
                    {
                        //去掉图像前几行，避免图片前几行造成的倾斜
                        //Remove the first few lines of the image to avoid the tilt caused by the first few lines of the image
                        //FileViewModel.YAxisUpdatePixeleffectiveMoveDisplayImage(ref image, Y_MovePixelDY);
                        ImageProcessingHelper.YAxisUpdatePixeleffectiveMoveDisplayImage(ref dstimg, Y_MovePixelDY);
                    }
                    #endregion
                }
                else if (laserChannel == LaserChannels.ChannelA)    //R1
                {
                    #region R1/channel A laser
                    //根据当前分辨率计算R1到R2之间需要截取掉的像素列
                    //Calculate the pixel column between R1 and R2 that needs to be intercepted according to the current resolution
                    int R2MovePixelX = (int)(alignParam.OpticalR2_R1Distance * 1000.0 / resolution);
                    int R2MovePixelY = 0;
                    int R2effectivePixel = R2MovePixelX * 2;
                    if (R2MovePixelX != 0)
                    {
                        //将图片向右移动R1到R2透镜之间的距离，保持图像里内容对齐
                        //The pixel column of the picture taken from left to right, keeping the position aligned with the picture
                        //FileViewModel.UpdatePixelSingleMoveDisplayImage(ref image, R2MovePixelX, R2MovePixelX, R2MovePixelY, R2MovePixelY);//image move
                        ImageProcessingHelper.UpdatePixelSingleMoveDisplayImage(ref srcimg, R2MovePixelX, R2MovePixelX, R2MovePixelY, R2MovePixelY);
                        //将图片从左到右截取掉多补偿的宽度部分
                        //Cut off the multi-compensated width of the picture from left to right
                        //FileViewModel.UpdatePixeleffectiveMoveDisplayImage(ref image, R2effectivePixel);//image effective
                        ImageProcessingHelper.UpdatePixeleffectiveMoveDisplayImage(ref srcimg, ref dstimg, R2effectivePixel);   //result in dstimg
                    }
                    else
                    {
                        dstimg = srcimg;    //result expected in dstimg
                    }
                    if (alignParam.IsImageOffsetProcessing)
                    {
                        //图片像素向右或者向下移动
                        ////Move the picture pixel to the right or down
                        int R2PixelDX = (int)(alignParam.Pixel_10_R2_DX / resolutionMove);
                        int R2PixelDY = (int)(alignParam.Pixel_10_R2_DY / resolutionMove);
                        if (R2PixelDX != 0 || R2PixelDY != 0)
                        {
                            //FileViewModel.UpdatePixelSingleMoveDisplayImage(ref image, R2PixelDX, R2PixelDX, R2PixelDY, R2PixelDY);//image move
                            //ImageProcessingHelper.UpdatePixelSingleMoveDisplayImage(ref srcimg, R2PixelDX, R2PixelDX, R2PixelDY, R2PixelDY);
                            ImageProcessingHelper.UpdatePixelSingleMoveDisplayImage(ref dstimg, R2PixelDX, R2PixelDX, R2PixelDY, R2PixelDY);
                        }
                    }
                    if (alignParam.IsPixelOffsetProcessing)
                    {
                        if (resolution < 50)
                        {
                            //处理高分辨率图片出现的锯齿
                            //Handle the sawtooth caused by high resolution
                            //FileViewModel.UpdatePixelSingleMoveDisplayImage(ref image, pixelOddX, pixelEvenX, pixelOddY, pixelEvenY);//Handle the sawtooth caused by high resolution
                            //ImageProcessingHelper.UpdatePixelSingleMoveDisplayImage(ref srcimg, pixelOddX, pixelEvenX, pixelOddY, pixelEvenY);
                            ImageProcessingHelper.UpdatePixelSingleMoveDisplayImage(ref dstimg, pixelOddX, pixelEvenX, pixelOddY, pixelEvenY);
                        }
                    }
                    if (alignParam.IsYCompensationBitAt && (int)(yCompOffset * 1000.0 / resolution) >= 1)
                    {
                        //去掉图像前几行，避免图片前几行造成的倾斜
                        //Remove the first few lines of the image to avoid the tilt caused by the first few lines of the image
                        //FileViewModel.YAxisUpdatePixeleffectiveMoveDisplayImage(ref image, Y_MovePixelDY);
                        ImageProcessingHelper.YAxisUpdatePixeleffectiveMoveDisplayImage(ref dstimg, Y_MovePixelDY);
                    }
                    #endregion
                }
                else if (laserChannel == LaserChannels.ChannelB)    //R2
                {
                    #region R2/channel B laser
                    //根据当前分辨率计算L到R2之间需要截取掉多少列像素
                    //Calculate how many columns of pixels need to be intercepted between L and R2 according to the current resolution
                    int R1MovePixelDX = (int)(alignParam.OpticalL_R1Distance * 1000.0 / resolution);
                    int R1MovePixelDY = 0;
                    if (R1MovePixelDX != 0)
                    {
                        //将图片向右移动L到R2透镜之间的距离，保持图像里内容对齐
                        //The pixel column of the picture taken from left to right, keeping the position aligned with the picture
                        //FileViewModel.UpdatePixelSingleMoveDisplayImage(ref image, R1MovePixelDX, R1MovePixelDX, R1MovePixelDY, R1MovePixelDY);
                        ImageProcessingHelper.UpdatePixelSingleMoveDisplayImage(ref srcimg, R1MovePixelDX, R1MovePixelDX, R1MovePixelDY, R1MovePixelDY);

                        //将图片从左到右截取掉多补偿的宽度部分
                        //Cut off the multi-compensated width of the picture from left to right
                        //FileViewModel.UpdatePixeleffectiveMoveDisplayImage(ref image, R1MovePixelDX);
                        ImageProcessingHelper.UpdatePixeleffectiveMoveDisplayImage(ref srcimg, ref dstimg, R1MovePixelDX);
                    }
                    else
                    {
                        dstimg = srcimg;
                    }
                    if (alignParam.IsImageOffsetProcessing)
                    {
                        ///
                    }
                    if (alignParam.IsPixelOffsetProcessing)
                    {
                        if (resolution < 50)
                        {
                            //处理高分辨率图片出现的锯齿
                            //Handle the sawtooth caused by high resolution
                            //FileViewModel.UpdatePixelSingleMoveDisplayImage(ref image, pixelOddX, pixelEvenX, pixelOddY, pixelEvenY);//Handle the sawtooth caused by high resolution
                            ImageProcessingHelper.UpdatePixelSingleMoveDisplayImage(ref dstimg, pixelOddX, pixelEvenX, pixelOddY, pixelEvenY);
                        }
                    }
                    if (alignParam.IsYCompensationBitAt && (int)(yCompOffset * 1000.0 / resolution) >= 1)
                    {
                        //去掉图像前几行，避免图片前几行造成的倾斜
                        //Remove the first few lines of the image to avoid the tilt caused by the first few lines of the image
                        //FileViewModel.YAxisUpdatePixeleffectiveMoveDisplayImage(ref image, Y_MovePixelDY);//Cut out the first few lines of the X-axis
                        ImageProcessingHelper.YAxisUpdatePixeleffectiveMoveDisplayImage(ref dstimg, Y_MovePixelDY);
                    }
                    #endregion
                }
            }
            catch
            {
                //StopWaitAnimation();
                //_IsLoading = false;
            }
        }*/

        /// <summary>
        /// Use to align the capture preview image (using byte pointer to avoid crossing thread)
        /// </summary>
        /*public static unsafe void AlignImage(byte* psrcimg, int srcWidth, int srcHeight, int srcBufferStride,
                                             byte* pdstimg, int dstWidth, int dstHeight, int dstBufferStride, ImageAlignParam alignParam, byte* psrcimgtemp = null, byte* pdstimgtemp = null)
        {
            int resolution = alignParam.Resolution;
            double resolutionMove = resolution / 10.0;   //MoveImageResolution
            //像素左右偏移参数值
            //Pixel left and right offset parameter value
            int pixelOddX = alignParam.PixelOddX;   //X odd Line
            int pixelEvenX = alignParam.PixelEvenX; //X even Line
            int pixelOddY = alignParam.PixelOddY;   //Y odd Line
            int pixelEvenY = alignParam.PixelEvenY; //Y Even Line
            double yCompOffset = alignParam.YCompOffset;
            //Y轴截取像素前几行
            ////Y-axis intercepts the first few rows of pixels
            int Y_MovePixelDY = (int)(yCompOffset * 1000.0 / resolution);
            var laserChannel = alignParam.LaserChannel;
            try
            {
                //（因为在设计中将R1透镜的位置和R2透镜的位置更换了，然后参数名没有更改成对应的通道，所以下面参数名R1其实是R2，R2其实是R1）....
                // (in the design, the position of R1 lens and R2 lens is changed, and the parameter name is not changed to the corresponding channel, so the following parameter name R1 is actually R2, R2 is actually R1)....
                if (laserChannel == LaserChannels.ChannelC)         //L1
                {
                    #region L1/channel C laser
                    //根据当前分辨率计算L到R2之间需要截取掉多少列像素
                    //Calculate how many columns of pixels need to be intercepted between L and R2 according to the current resolution
                    int leffectivePixel = (int)(alignParam.OpticalL_R1Distance * 1000.0 / resolution);
                    if (leffectivePixel != 0)
                    {
                        //将图片从左到右截取的像素列，保持图片位置对齐
                        //The pixel column of the picture taken from left to right, keeping the position aligned with the picture
                        //FileViewModel.UpdatePixeleffectiveMoveDisplayImage(ref image, LeffectivePixel);//image effective
                        //ImageProcessingHelper.UpdatePixeleffectiveMoveDisplayImage(ref srcimg, ref dstimg, leffectivePixel);

                        int lMovePixelDX = (int)(alignParam.Pixel_10_L_DX / resolutionMove);
                        int lMovePixelDY = (int)(alignParam.Pixel_10_L_DY / resolutionMove);
                        if ((alignParam.IsImageOffsetProcessing || alignParam.IsPixelOffsetProcessing) && (lMovePixelDX != 0 || lMovePixelDY != 0))
                        {
                            //Put result in psrcimgtemp
                            if (psrcimgtemp == null)
                            {
                                WriteableBitmap dstTemp = null;
                                dstTemp = new WriteableBitmap(srcWidth, srcHeight, 96, 96, PixelFormats.Gray16, null);
                                psrcimgtemp = (byte*)dstTemp.BackBuffer.ToPointer();
                            }
                            PixelSingleMoveCapturetheimage(psrcimg, srcWidth, srcHeight, srcBufferStride, psrcimgtemp, dstBufferStride, leffectivePixel);
                        }
                        else
                        {
                            //Put result in pdstimg - no other processing is being done.
                            PixelSingleMoveCapturetheimage(psrcimg, srcWidth, srcHeight, srcBufferStride, pdstimg, dstBufferStride, leffectivePixel);
                        }
                    }

                    WriteableBitmap wbPixelOffsetProc = null;
                    if (alignParam.IsImageOffsetProcessing)
                    {
                        //图片像素向右或者向下移动
                        //Move the picture pixel to the right or down
                        int lMovePixelDX = (int)(alignParam.Pixel_10_L_DX / resolutionMove);
                        int lMovePixelDY = (int)(alignParam.Pixel_10_L_DY / resolutionMove);
                        if (lMovePixelDX != 0 || lMovePixelDY != 0)
                        {
                            //FileViewModel.UpdatePixelSingleMoveDisplayImage(ref image, LMovePixelDX, LMovePixelDX, LMovePixelDY, LMovePixelDY);//Align the three images
                            //ImageProcessingHelper.UpdatePixelSingleMoveDisplayImage(ref dstimg, lMovePixelDX, lMovePixelDX, lMovePixelDY, lMovePixelDY);

                            if (alignParam.IsPixelOffsetProcessing && resolution < alignParam.PixelOffsetProcessingRes)
                            {
                                wbPixelOffsetProc = new WriteableBitmap(dstWidth, dstHeight, 96, 96, PixelFormats.Gray16, null);
                                byte* pPixelOffset = (byte*)wbPixelOffsetProc.BackBuffer.ToPointer();
                                PixelSingleMove(pdstimgtemp, dstWidth, dstHeight, dstBufferStride, 96, 96, PixelFormats.Gray16, pPixelOffset, lMovePixelDX, lMovePixelDX, lMovePixelDY, lMovePixelDY);
                            }
                            else
                            {
                                PixelSingleMove(pdstimgtemp, dstWidth, dstHeight, dstBufferStride, 96, 96, PixelFormats.Gray16, pdstimg, lMovePixelDX, lMovePixelDX, lMovePixelDY, lMovePixelDY);
                            }
                        }
                    }
                    WriteableBitmap wbYComp = null;
                    if (alignParam.IsPixelOffsetProcessing)
                    {
                        if (resolution < alignParam.PixelOffsetProcessingRes)
                        {
                            //处理高分辨率图片出现的锯齿
                            //Handle the sawtooth caused by high resolution
                            //FileViewModel.UpdatePixelSingleMoveDisplayImage(ref image, pixelOddx, pixelEvenx, pixelOddy, pixelEveny);
                            //ImageProcessingHelper.UpdatePixelSingleMoveDisplayImage(ref dstimg, pixelOddX, pixelEvenX, pixelOddY, pixelEvenY);

                            if (wbPixelOffsetProc != null && (alignParam.IsYCompensationBitAt && (int)(yCompOffset * 1000) / resolution >= 1))
                            {
                                byte* pPixelOffset = (byte*)wbPixelOffsetProc.BackBuffer.ToPointer();
                                wbYComp = new WriteableBitmap(dstWidth, dstHeight - Y_MovePixelDY, 96, 96, PixelFormats.Gray16, null);
                                byte* pdstYComp = (byte*)wbYComp.BackBuffer.ToPointer();
                                PixelSingleMove(pPixelOffset, dstWidth, dstHeight, dstBufferStride, 96, 96, PixelFormats.Gray16, pdstYComp, pixelOddX, pixelEvenX, pixelOddY, pixelEvenY);
                            }
                            else
                            {
                                if (wbPixelOffsetProc != null)
                                {
                                    byte* pPixelOffset = (byte*)wbPixelOffsetProc.BackBuffer.ToPointer();
                                    PixelSingleMove(pPixelOffset, dstWidth, dstHeight, dstBufferStride, 96, 96, PixelFormats.Gray16, pdstimg, pixelOddX, pixelEvenX, pixelOddY, pixelEvenY);
                                }
                            }
                        }
                    }
                    if (alignParam.IsYCompensationBitAt && (int)(yCompOffset * 1000) / resolution >= 1)
                    {
                        //去掉图像前几行，避免图片前几行造成的倾斜
                        //Remove the first few lines of the image to avoid the tilt caused by the first few lines of the image
                        //FileViewModel.YAxisUpdatePixeleffectiveMoveDisplayImage(ref image, Y_MovePixelDY);
                        //ImageProcessingHelper.YAxisUpdatePixeleffectiveMoveDisplayImage(ref srcimg, Y_MovePixelDY);
                        if (wbYComp != null)
                        {
                            byte* psrcYComp = (byte*)wbYComp.BackBuffer.ToPointer();
                            int width = wbYComp.PixelWidth;
                            int height = wbYComp.PixelHeight;
                            int stride = wbYComp.BackBufferStride;
                            YAxiePixelSingleMoveCapturetheimage(psrcYComp, width, height, stride, pdstimg, dstBufferStride, Y_MovePixelDY);
                        }
                    }
                    #endregion
                }
                else if (laserChannel == LaserChannels.ChannelA)    //R1
                {
                    #region R1/channel A laser
                    //根据当前分辨率计算R1到R2之间需要截取掉的像素列
                    //Calculate the pixel column between R1 and R2 that needs to be intercepted according to the current resolution
                    int R2MovePixelX = (int)(alignParam.OpticalR2_R1Distance * 1000) / resolution;
                    int R2MovePixelY = 0;
                    int R2effectivePixel = R2MovePixelX * 2;
                    if (R2MovePixelX != 0)
                    {
                        //将图片向右移动R1到R2透镜之间的距离，保持图像里内容对齐
                        //The pixel column of the picture taken from left to right, keeping the position aligned with the picture
                        //FileViewModel.UpdatePixelSingleMoveDisplayImage(ref image, R2MovePixelX, R2MovePixelX, R2MovePixelY, R2MovePixelY);//image move
                        //ImageProcessingHelper.UpdatePixelSingleMoveDisplayImage(ref srcimg, R2MovePixelX, R2MovePixelX, R2MovePixelY, R2MovePixelY);

                        if (psrcimgtemp == null)
                        {
                            WriteableBitmap srcTemp = null;
                            srcTemp = new WriteableBitmap(srcWidth, srcHeight, 96, 96, PixelFormats.Gray16, null);
                            psrcimgtemp = (byte*)srcTemp.BackBuffer.ToPointer();
                        }
                        //srcimg -> srctemp
                        PixelSingleMove(psrcimg, srcWidth, srcHeight, srcBufferStride, 96, 96, PixelFormats.Gray16, psrcimgtemp, R2MovePixelX, R2MovePixelX, R2MovePixelY, R2MovePixelY);

                        //将图片从左到右截取掉多补偿的宽度部分
                        //Cut off the multi-compensated width of the picture from left to right
                        //FileViewModel.UpdatePixeleffectiveMoveDisplayImage(ref image, R2effectivePixel);//image effective
                        //ImageProcessingHelper.UpdatePixeleffectiveMoveDisplayImage(ref srcimg, ref dstimg, R2effectivePixel);

                        int R2PixelDX = (int)(alignParam.Pixel_10_R2_DX / resolutionMove);
                        int R2PixelDY = (int)(alignParam.Pixel_10_R2_DY / resolutionMove);
                        if (alignParam.IsImageOffsetProcessing &&
                            resolution < alignParam.PixelOffsetProcessingRes && (R2PixelDX != 0 || R2PixelDY != 0))
                        {
                            if (pdstimgtemp == null)
                            {
                                WriteableBitmap dstTemp = null;
                                dstTemp = new WriteableBitmap(dstWidth, dstHeight, 96, 96, PixelFormats.Gray16, null);
                                pdstimgtemp = (byte*)dstTemp.BackBuffer.ToPointer();
                            }
                            PixelSingleMoveCapturetheimage(psrcimgtemp, srcWidth, srcHeight, srcBufferStride, pdstimgtemp, dstBufferStride, R2effectivePixel);
                        }
                        else
                        {
                            ImagingHelper.PixelSingleMoveCapturetheimage(psrcimgtemp, srcWidth, srcHeight, srcBufferStride, pdstimg, dstBufferStride, R2effectivePixel);
                        }
                    }
                    WriteableBitmap wbPixelOffsetProc = null;
                    if (alignParam.IsImageOffsetProcessing)
                    {
                        //图片像素向右或者向下移动
                        //Move the picture pixel to the right or down
                        int R2PixelDX = (int)(alignParam.Pixel_10_R2_DX / resolutionMove);
                        int R2PixelDY = (int)(alignParam.Pixel_10_R2_DY / resolutionMove);
                        if (R2PixelDX != 0 || R2PixelDY != 0 && R2MovePixelX != 0)
                        {
                            //FileViewModel.UpdatePixelSingleMoveDisplayImage(ref image, R2PixelDX, R2PixelDX, R2PixelDY, R2PixelDY);//image move
                            //ImageProcessingHelper.UpdatePixelSingleMoveDisplayImage(ref srcimg, R2PixelDX, R2PixelDX, R2PixelDY, R2PixelDY);
                            if (alignParam.IsPixelOffsetProcessing && resolution < alignParam.PixelOffsetProcessingRes)
                            {
                                wbPixelOffsetProc = new WriteableBitmap(dstWidth, dstHeight, 96, 96, PixelFormats.Gray16, null);
                                byte* pPixelOffset = (byte*)wbPixelOffsetProc.BackBuffer.ToPointer();
                                PixelSingleMove(pdstimgtemp, dstWidth, dstHeight, dstBufferStride, 96, 96, PixelFormats.Gray16, pPixelOffset, R2PixelDX, R2PixelDX, R2PixelDY, R2PixelDY);
                            }
                            else
                            {
                                PixelSingleMove(pdstimgtemp, dstWidth, dstHeight, dstBufferStride, 96, 96, PixelFormats.Gray16, pdstimg, R2PixelDX, R2PixelDX, R2PixelDY, R2PixelDY);
                            }
                        }
                    }
                    WriteableBitmap wbYComp = null;
                    if (alignParam.IsPixelOffsetProcessing)
                    {
                        if (resolution < alignParam.PixelOffsetProcessingRes)
                        {
                            //处理高分辨率图片出现的锯齿
                            //Handle the sawtooth caused by high resolution
                            //FileViewModel.UpdatePixelSingleMoveDisplayImage(ref image, pixelOddX, pixelEvenX, pixelOddY, pixelEvenY);//Handle the sawtooth caused by high resolution
                            //ImageProcessingHelper.UpdatePixelSingleMoveDisplayImage(ref srcimg, pixelOddX, pixelEvenX, pixelOddY, pixelEvenY);

                            if (wbPixelOffsetProc != null && (alignParam.IsYCompensationBitAt && (int)(yCompOffset * 1000) / resolution >= 1))
                            {
                                if (wbPixelOffsetProc != null)
                                {
                                    byte* pPixelOffset = (byte*)wbPixelOffsetProc.BackBuffer.ToPointer();
                                    wbYComp = new WriteableBitmap(dstWidth, dstHeight - Y_MovePixelDY, 96, 96, PixelFormats.Gray16, null);
                                    byte* pdstYComp = (byte*)wbYComp.BackBuffer.ToPointer();
                                    PixelSingleMove(pPixelOffset, dstWidth, dstHeight, dstBufferStride, 96, 96, PixelFormats.Gray16, pdstYComp, pixelOddX, pixelEvenX, pixelOddY, pixelEvenY);
                                }
                            }
                            else
                            {
                                if (wbPixelOffsetProc != null)
                                {
                                    byte* pPixelOffset = (byte*)wbPixelOffsetProc.BackBuffer.ToPointer();
                                    PixelSingleMove(pPixelOffset, dstWidth, dstHeight, dstBufferStride, 96, 96, PixelFormats.Gray16, pdstimg, pixelOddX, pixelEvenX, pixelOddY, pixelEvenY);
                                }
                            }
                        }
                    }
                    if (alignParam.IsYCompensationBitAt && (int)(yCompOffset * 1000.0 / resolution) >= 1)
                    {
                        //去掉图像前几行，避免图片前几行造成的倾斜
                        //Remove the first few lines of the image to avoid the tilt caused by the first few lines of the image
                        //FileViewModel.YAxisUpdatePixeleffectiveMoveDisplayImage(ref image, Y_MovePixelDY);
                        //ImageProcessingHelper.YAxisUpdatePixeleffectiveMoveDisplayImage(ref srcimg, Y_MovePixelDY);

                        if (wbYComp != null)
                        {
                            byte* psrcYComp = (byte*)wbYComp.BackBuffer.ToPointer();
                            int width = wbYComp.PixelWidth;
                            int height = wbYComp.PixelHeight;
                            int stride = wbYComp.BackBufferStride;
                            YAxiePixelSingleMoveCapturetheimage(psrcYComp, width, height, stride, pdstimg, dstBufferStride, Y_MovePixelDY);
                        }
                    }
                    #endregion
                }
                else if (laserChannel == LaserChannels.ChannelB)    //R2
                {
                    #region R2/channel B laser

                    //根据当前分辨率计算L到R2之间需要截取掉多少列像素
                    //Calculate how many columns of pixels need to be intercepted between L and R2 according to the current resolution
                    int R1MovePixelDX = (int)(alignParam.OpticalL_R1Distance * 1000.0 / (double)resolution);
                    int R1MovePixelDY = 0;
                    if (R1MovePixelDX != 0)
                    {
                        //将图片向右移动L到R2透镜之间的距离，保持图像里内容对齐
                        //The pixel column of the picture taken from left to right, keeping the position aligned with the picture
                        //FileViewModel.UpdatePixelSingleMoveDisplayImage(ref image, R1MovePixelDX, R1MovePixelDX, R1MovePixelDY, R1MovePixelDY);
                        //ImageProcessingHelper.UpdatePixelSingleMoveDisplayImage(ref srcimg, R1MovePixelDX, R1MovePixelDX, R1MovePixelDY, R1MovePixelDY);
                        //ImageProcessing.PixelSingleMove(psrcimg, srcWidth, srcHeight, srcBufferStride, 96, 96, PixelFormats.Gray16, R1MovePixelDX, R1MovePixelDX, R1MovePixelDY, R1MovePixelDY);
                        if (psrcimgtemp == null)
                        {
                            WriteableBitmap srcTemp = srcTemp = new WriteableBitmap(srcWidth, srcHeight, 96, 96, PixelFormats.Gray16, null);
                            psrcimgtemp = (byte*)srcTemp.BackBuffer.ToPointer();
                        }
                        PixelSingleMove(psrcimg, srcWidth, srcHeight, srcBufferStride, 96, 96, PixelFormats.Gray16, psrcimgtemp, R1MovePixelDX, R1MovePixelDX, R1MovePixelDY, R1MovePixelDY);

                        //将图片从左到右截取掉多补偿的宽度部分
                        //Cut off the multi-compensated width of the picture from left to right
                        //FileViewModel.UpdatePixeleffectiveMoveDisplayImage(ref image, R1MovePixelDX);
                        //ImageProcessingHelper.UpdatePixeleffectiveMoveDisplayImage(ref srcimg, ref dstimg, R1MovePixelDX);

                        if (alignParam.IsPixelOffsetProcessing)
                        {
                            if (psrcimgtemp == null)
                            {
                                WriteableBitmap dstTemp = null;
                                dstTemp = new WriteableBitmap(srcWidth, srcHeight, 96, 96, PixelFormats.Gray16, null);
                                pdstimgtemp = (byte*)dstTemp.BackBuffer.ToPointer();
                            }
                            PixelSingleMoveCapturetheimage(psrcimgtemp, srcWidth, srcHeight, srcBufferStride, pdstimgtemp, dstBufferStride, R1MovePixelDX);
                        }
                        else
                        {
                            PixelSingleMoveCapturetheimage(psrcimgtemp, srcWidth, srcHeight, srcBufferStride, pdstimg, dstBufferStride, R1MovePixelDX);
                        }
                    }
                    WriteableBitmap wbYComp = null;
                    if (alignParam.IsPixelOffsetProcessing)
                    {
                        if (resolution < alignParam.PixelOffsetProcessingRes && R1MovePixelDX != 0)
                        {
                            //处理高分辨率图片出现的锯齿
                            //Handle the sawtooth caused by high resolution
                            //FileViewModel.UpdatePixelSingleMoveDisplayImage(ref image, pixelOddX, pixelEvenX, pixelOddY, pixelEvenY);//Handle the sawtooth caused by high resolution
                            //ImageProcessingHelper.UpdatePixelSingleMoveDisplayImage(ref dstimg, pixelOddX, pixelEvenX, pixelOddY, pixelEvenY);

                            if (alignParam.IsYCompensationBitAt && (int)(yCompOffset * 1000) / resolution >= 1)
                            {
                                wbYComp = new WriteableBitmap(dstWidth, dstHeight, 96, 96, PixelFormats.Gray16, null);
                                byte* pdstYComp = (byte*)wbYComp.BackBuffer.ToPointer();
                                PixelSingleMove(pdstimgtemp, dstWidth, dstHeight, dstBufferStride, 96, 96, PixelFormats.Gray16, pdstYComp, pixelOddX, pixelEvenX, pixelOddY, pixelEvenY);
                            }
                            else
                            {
                                PixelSingleMove(pdstimgtemp, dstWidth, dstHeight, dstBufferStride, 96, 96, PixelFormats.Gray16, pdstimg, pixelOddX, pixelEvenX, pixelOddY, pixelEvenY);
                            }
                        }
                        else
                        {
                            if (alignParam.IsYCompensationBitAt && (int)(yCompOffset * 1000) / resolution >= 1)
                            {
                                wbYComp = new WriteableBitmap(dstWidth, dstHeight, 96, 96, PixelFormats.Gray16, null);
                                byte* pdstYComp = (byte*)wbYComp.BackBuffer.ToPointer();
                                PixelSingleMove(psrcimg, dstWidth, dstHeight, dstBufferStride, 96, 96, PixelFormats.Gray16, pdstYComp, pixelOddX, pixelEvenX, pixelOddY, pixelEvenY);
                            }
                            else
                            {
                                PixelSingleMove(psrcimg, dstWidth, dstHeight, dstBufferStride, 96, 96, PixelFormats.Gray16, pdstimg, pixelOddX, pixelEvenX, pixelOddY, pixelEvenY);
                            }
                        }
                    }
                    if (alignParam.IsYCompensationBitAt && (int)(yCompOffset * 1000) / resolution >= 1)
                    {
                        //去掉图像前几行，避免图片前几行造成的倾斜
                        //Remove the first few lines of the image to avoid the tilt caused by the first few lines of the image
                        //FileViewModel.YAxisUpdatePixeleffectiveMoveDisplayImage(ref image, Y_MovePixelDY);//Cut out the first few lines of the X-axis
                        //ImageProcessingHelper.YAxisUpdatePixeleffectiveMoveDisplayImage(ref dstimg, Y_MovePixelDY);

                        if (wbYComp != null)
                        {
                            byte* psrcYComp = (byte*)wbYComp.BackBuffer.ToPointer();
                            int width = wbYComp.PixelWidth;
                            int height = wbYComp.PixelHeight;
                            int stride = wbYComp.BackBufferStride;
                            YAxiePixelSingleMoveCapturetheimage(psrcYComp, width, height, stride, pdstimg, dstBufferStride, Y_MovePixelDY);
                        }
                    }

                    #endregion
                }
            }
            catch
            {
                //StopWaitAnimation();
                //_IsLoading = false;
            }
        }*/


        public static unsafe void AlignImage(byte* psrcimg, int srcWidth, int srcHeight, int srcBufferStride,
                                             byte* pdstimg, int dstWidth, int dstHeight, int dstBufferStride,
                                             ImageAlignParam alignParam, byte* psrcimgtemp = null, byte* pdstimgtemp = null)
        {
            int resolution = alignParam.Resolution;
            double resolutionMove = (double)resolution / 10.0;   //MoveImageResolution
            //像素左右偏移参数值
            //Pixel left and right offset parameter value
            int pixelOddX = alignParam.PixelOddX;   //X odd Line
            int pixelEvenX = alignParam.PixelEvenX; //X even Line
            int pixelOddY = alignParam.PixelOddY;   //Y odd Line
            int pixelEvenY = alignParam.PixelEvenY; //Y Even Line
            double yCompOffset = alignParam.YCompOffset;
            //Y轴截取像素前几行
            ////Y-axis intercepts the first few rows of pixels
            int Y_MovePixelDY = (int)(yCompOffset * 1000.0 / resolution);
            var laserChannel = alignParam.LaserChannel;

            //（因为在设计中将R1透镜的位置和R2透镜的位置更换了，然后参数名没有更改成对应的通道，所以下面参数名R1其实是R2，R2其实是R1）....
            // (in the design, the position of R1 lens and R2 lens is changed, and the parameter name is not changed to the corresponding channel, so the following parameter name R1 is actually R2, R2 is actually R1)....
            if (laserChannel == LaserChannels.ChannelC)         //L1
            {
                #region L1/channel C laser
                //根据当前分辨率计算L到R2之间需要截取掉多少列像素
                //Calculate how many columns of pixels need to be intercepted between L and R2 according to the current resolution
                int leffectivePixel = (int)Math.Round(alignParam.OpticalL_R1Distance * 1000.0 / resolution);
                if (leffectivePixel != 0)
                {
                    //将图片从左到右截取的像素列，保持图片位置对齐
                    //The pixel column of the picture taken from left to right, keeping the position aligned with the picture
                    //FileViewModel.UpdatePixeleffectiveMoveDisplayImage(ref image, LeffectivePixel);//image effective
                    //ImageProcessingHelper.UpdatePixeleffectiveMoveDisplayImage(ref srcimg, ref dstimg, leffectivePixel);

                    if (pdstimgtemp == null)
                    {
                        WriteableBitmap dstTemp = null;
                        dstTemp = new WriteableBitmap(dstWidth, dstHeight, 96, 96, PixelFormats.Gray16, null);
                        pdstimgtemp = (byte*)dstTemp.BackBuffer.ToPointer();
                    }
                    int lMovePixelDX = (int)(alignParam.Pixel_10_L_DX / resolutionMove);
                    int lMovePixelDY = (int)(alignParam.Pixel_10_L_DY / resolutionMove);
                    if (lMovePixelDX != 0 || lMovePixelDY != 0)
                    {
                        //srcimg -> dsttemp
                        PixelSingleMoveCapturetheimage(psrcimg, srcWidth, srcHeight, srcBufferStride, pdstimgtemp, dstBufferStride, leffectivePixel);
                        //dsttemp -> dstimg
                        PixelSingleMove(pdstimgtemp, dstWidth, dstHeight, dstBufferStride, 96, 96, PixelFormats.Gray16, pdstimg, lMovePixelDX, lMovePixelDX, lMovePixelDY, lMovePixelDY);
                    }
                    else
                    {
                        //srcimg -> dstimg
                        PixelSingleMoveCapturetheimage(psrcimg, srcWidth, srcHeight, srcBufferStride, pdstimg, dstBufferStride, leffectivePixel);
                    }
                }

                #endregion
            }
            else if (laserChannel == LaserChannels.ChannelA)    //R1
            {
                #region R1/channel A laser
                //根据当前分辨率计算R1到R2之间需要截取掉的像素列
                //Calculate the pixel column between R1 and R2 that needs to be intercepted according to the current resolution
                int R2MovePixelX = (int)(alignParam.OpticalR2_R1Distance * 1000.0 / resolution);
                int R2MovePixelY = 0;
                int R2effectivePixel = R2MovePixelX * 2;
                if (R2MovePixelX != 0)
                {
                    //将图片向右移动R1到R2透镜之间的距离，保持图像里内容对齐
                    //The pixel column of the picture taken from left to right, keeping the position aligned with the picture
                    //FileViewModel.UpdatePixelSingleMoveDisplayImage(ref image, R2MovePixelX, R2MovePixelX, R2MovePixelY, R2MovePixelY);//image move
                    //ImageProcessingHelper.UpdatePixelSingleMoveDisplayImage(ref srcimg, R2MovePixelX, R2MovePixelX, R2MovePixelY, R2MovePixelY);

                    if (psrcimgtemp == null)
                    {
                        WriteableBitmap srcTemp = null;
                        srcTemp = new WriteableBitmap(srcWidth, srcHeight, 96, 96, PixelFormats.Gray16, null);
                        psrcimgtemp = (byte*)srcTemp.BackBuffer.ToPointer();
                    }
                    //srcimg -> srctemp
                    PixelSingleMove(psrcimg, srcWidth, srcHeight, srcBufferStride, 96, 96, PixelFormats.Gray16, psrcimgtemp, R2MovePixelX, R2MovePixelX, R2MovePixelY, R2MovePixelY);

                    if (pdstimgtemp == null)
                    {
                        WriteableBitmap dstTemp = null;
                        dstTemp = new WriteableBitmap(dstWidth, dstHeight, 96, 96, PixelFormats.Gray16, null);
                        pdstimgtemp = (byte*)dstTemp.BackBuffer.ToPointer();
                    }
                    int R2PixelDX = (int)((double)alignParam.Pixel_10_R2_DX / resolutionMove);
                    int R2PixelDY = (int)((double)alignParam.Pixel_10_R2_DY / resolutionMove);
                    if (R2PixelDX != 0 || R2PixelDY != 0)
                    {
                        //srctemp -> dsttemp
                        PixelSingleMoveCapturetheimage(psrcimgtemp, srcWidth, srcHeight, srcBufferStride, pdstimgtemp, dstBufferStride, R2effectivePixel);
                        PixelSingleMove(pdstimgtemp, dstWidth, dstHeight, dstBufferStride, 96, 96, PixelFormats.Gray16, pdstimg, R2PixelDX, R2PixelDX, R2PixelDY, R2PixelDY);
                    }
                    else
                    {
                        //srctemp -> dsttemp
                        PixelSingleMoveCapturetheimage(psrcimgtemp, srcWidth, srcHeight, srcBufferStride, pdstimg, dstBufferStride, R2effectivePixel);
                    }
                }

                #endregion
            }
            else if (laserChannel == LaserChannels.ChannelB)    //R2
            {
                #region R2/channel B laser

                //根据当前分辨率计算L到R2之间需要截取掉多少列像素
                //Calculate how many columns of pixels need to be intercepted between L and R2 according to the current resolution
                int R1MovePixelDX = (int)(alignParam.OpticalL_R1Distance * 1000.0 / resolution);
                int R1MovePixelDY = 0;
                if (R1MovePixelDX != 0)
                {
                    //将图片向右移动L到R2透镜之间的距离，保持图像里内容对齐
                    //The pixel column of the picture taken from left to right, keeping the position aligned with the picture
                    //FileViewModel.UpdatePixelSingleMoveDisplayImage(ref image, R1MovePixelDX, R1MovePixelDX, R1MovePixelDY, R1MovePixelDY);
                    //ImageProcessingHelper.UpdatePixelSingleMoveDisplayImage(ref srcimg, R1MovePixelDX, R1MovePixelDX, R1MovePixelDY, R1MovePixelDY);
                    //ImageProcessing.PixelSingleMove(psrcimg, srcWidth, srcHeight, srcBufferStride, 96, 96, PixelFormats.Gray16, R1MovePixelDX, R1MovePixelDX, R1MovePixelDY, R1MovePixelDY);
                    if (psrcimgtemp == null)
                    {
                        WriteableBitmap srcTemp = srcTemp = new WriteableBitmap(srcWidth, srcHeight, 96, 96, PixelFormats.Gray16, null);
                        psrcimgtemp = (byte*)srcTemp.BackBuffer.ToPointer();
                    }
                    //srimg -> srctemp
                    PixelSingleMove(psrcimg, srcWidth, srcHeight, srcBufferStride, 96, 96, PixelFormats.Gray16, psrcimgtemp, R1MovePixelDX, R1MovePixelDX, R1MovePixelDY, R1MovePixelDY);
                    //srctemp -> dstimg
                    PixelSingleMoveCapturetheimage(psrcimgtemp, srcWidth, srcHeight, srcBufferStride, pdstimg, dstBufferStride, R1MovePixelDX);
                }

                #endregion
            }
        }

        public static unsafe void PixelSingleMoveCapturetheimage(byte* psrcimg, int srcwidth, int srcheight, int srcStride, byte* pdstimg, int dstStride, int pixelOffset)
        {
            if (pixelOffset == 0) { return; }

            unsafe
            {
                ushort* newpbuff = (ushort*)psrcimg;//原图数据
                ushort* pbuffX = (ushort*)pdstimg;
                int indexwb = 0;
                int indexcap = 0;
                for (int x = 0; x < srcwidth - pixelOffset; x++)
                {
                    for (int y = 0; y < srcheight; y++)
                    {
                        indexwb = x + y * srcStride / 2;
                        indexcap = x + y * dstStride / 2;
                        pbuffX[indexcap] = newpbuff[indexwb + pixelOffset];
                    }
                }
            }
        }

        public static unsafe void PixelSingleMove(byte* psrcimg, int width, int height, int stride, double dpix, double dpiy, PixelFormat format, byte* psrcimgtemp, int PixelX1, int PixelX2, int PixelY1, int PixelY2)
        {
            WriteableBitmap wbbx = null;
            wbbx = new WriteableBitmap(width, height, dpix, dpiy, format, null);
            ushort* newpbuff = (ushort*)psrcimg;
            ushort* pbuffX = (ushort*)(byte*)wbbx.BackBuffer.ToPointer();
            ushort* pbuffY = (ushort*)psrcimgtemp;
            int index = 0; int Y1temp = 0; int Y2temp = 0;
            #region 
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    index = x + y * stride / 2;
                    if ((y % 2) == 1)
                    {
                        if (index - PixelX1 > 0)
                        {
                            pbuffX[index] = newpbuff[index - PixelX1];
                        }
                    }
                    else
                    {
                        if (index - PixelX2 > 0)
                        {
                            pbuffX[index] = newpbuff[index - PixelX2];
                        }
                    }
                }
            }
            index = 0;
            int _x1 = Math.Abs(PixelX1);
            int _x2 = Math.Abs(PixelX2);
            for (int y = 0; y < wbbx.PixelHeight; y++)
            {
                index = y * wbbx.BackBufferStride / 2;
                if ((y % 2) == 1)
                {
                    if (PixelX1 > 0)
                    {
                        for (int i = index; i < index + PixelX1; i++)
                        {
                            pbuffX[i] = 0;
                        }
                    }
                    else
                    {
                        if (index > 0)
                        {
                            for (int i = index - _x1; i < index; i++)
                            {
                                pbuffX[i] = 0;
                            }
                        }
                    }
                }
                else
                {
                    if (PixelX2 > 0)
                    {
                        for (int i = index; i < index + PixelX2; i++)
                        {
                            pbuffX[i] = 0;
                        }
                    }
                    else
                    {
                        if (index > 0)
                        {
                            for (int i = index - _x2; i < index; i++)
                            {
                                pbuffX[i] = 0;
                            }
                        }
                    }
                }
            }
            #endregion

            int temp = 1 * wbbx.BackBufferStride / 2;
            Y1temp = temp * PixelY1;
            Y2temp = temp * PixelY2;
            #region 
            for (int x = 0; x < wbbx.PixelWidth; x++)
            {
                for (int y = 0; y < wbbx.PixelHeight; y++)
                {
                    index = x + y * wbbx.BackBufferStride / 2;
                    if ((x % 2) == 1)
                    {
                        if (Y1temp > 0)
                        {
                            if ((index) + Y1temp < wbbx.PixelWidth * wbbx.PixelHeight)
                            {
                                pbuffY[(index) + Y1temp] = pbuffX[index];
                            }
                        }
                        else
                        {
                            if ((index) + Y1temp < wbbx.PixelWidth * wbbx.PixelHeight)
                            {
                                if ((index) + Y1temp > 0)
                                {
                                    pbuffY[(index) + Y1temp] = pbuffX[index];
                                }

                            }
                        }
                    }
                    else
                    {
                        if (Y2temp > 0)
                        {
                            if ((index) + Y2temp < wbbx.PixelWidth * wbbx.PixelHeight)
                            {
                                pbuffY[(index) + Y2temp] = pbuffX[index];
                            }
                        }
                        else
                        {
                            if ((index) + Y2temp < wbbx.PixelWidth * wbbx.PixelHeight)
                            {
                                if ((index) + Y2temp > 0)
                                {
                                    pbuffY[(index) + Y2temp] = pbuffX[index];
                                }

                            }

                        }

                    }
                }

            }
            #endregion
        }

        public static unsafe void YAxiePixelSingleMoveCapturetheimage(byte* psrcimg, int width, int height, int srcBufferStride, byte* pdstimg, int dstBufferStride, int pixelOffset)
        {
            if (pixelOffset == 0) { return; }

            ushort* newpbuff = (ushort*)psrcimg;
            ushort* pbuffX = (ushort*)pdstimg;
            int indexwb = 0;
            int indexcap = 0;
            for (int x = 0; x < width; x++)
            {
                for (int y = pixelOffset; y < height; y++)
                {
                    indexwb = x + y * srcBufferStride / 2;
                    indexcap = x + (y - pixelOffset) * dstBufferStride / 2;
                    pbuffX[indexcap] = newpbuff[indexwb];
                }
            }
        }

        /// <summary>
        /// Save test images for debugging purposes (image info not saved).
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="filePath"></param>
        /// <param name="imageFile"></param>
        public static void SaveFile(string filePath, WriteableBitmap imageFile, ImageInfo imgInfo = null)
        {
            //filePath = System.IO.Path.Combine(filePath, fileName);
            using (System.IO.FileStream fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite, System.IO.FileShare.None))
            {
                ImageProcessing.Save(fileStream, ImageProcessing.TIFF_FILE, imageFile, imgInfo);
            }
        }

    }
}
