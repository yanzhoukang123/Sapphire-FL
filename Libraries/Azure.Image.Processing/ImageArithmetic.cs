using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Azure.Image.Processing
{
    public class ImageArithmetic
    {
        #region public unsafe WriteableBitmap AddImage(ref WriteableBitmap srcimg, ref WriteableBitmap oprimg)
        /// <summary>
        /// Adds pixel values of two images
        /// </summary>
        /// <param name="srcimg">Source image</param>
        /// <param name="oprimg">Operator image</param>
        /// <returns>Result image</returns>
        public unsafe WriteableBitmap AddImage(ref WriteableBitmap srcimg, ref WriteableBitmap oprimg)
        {
            ValidateImages(new WriteableBitmap[] { srcimg, oprimg });

            double dDpiX = srcimg.DpiX;
            double dDpiY = srcimg.DpiY;

            WriteableBitmap resimg = new WriteableBitmap(srcimg.PixelWidth, srcimg.PixelHeight, dDpiX, dDpiY, srcimg.Format, null);

            int width = srcimg.PixelWidth;
            int height = srcimg.PixelHeight;
            int bitsPerPixel = srcimg.Format.BitsPerPixel;
            int stride = 4 * ((bitsPerPixel * width + 31) / 32);
            //int stride = (width * bitsPerPixel + 7) / 8;
            int buffSize = height * stride;
            int bufferWidth = srcimg.BackBufferStride;
            int resultValue = 0;

            // Reserves the back buffer for updates.
            srcimg.Lock();
            oprimg.Lock();
            resimg.Lock();

            byte* pSrcBuffer = (byte*)srcimg.BackBuffer.ToPointer();
            byte* pOprBuffer = (byte*)oprimg.BackBuffer.ToPointer();
            byte* pResBuffer = (byte*)resimg.BackBuffer.ToPointer();

            byte* pSrc = null;
            byte* pOpr = null;
            byte* pRes = null;

            switch (bitsPerPixel)
            {
                case 8:
                    for (int i = 0; i < height; i++)
                    {
                        pSrc = pSrcBuffer + (i * bufferWidth);
                        pOpr = pOprBuffer + (i * bufferWidth);
                        pRes = pResBuffer + (i * bufferWidth);

                        for (int j = 0; j < width; j++)
                        {
                            resultValue = (*pSrc++) + (*pOpr++);
                            *pRes++ = (byte)((resultValue > 255) ? 255 : resultValue);
                        }
                    }
                    break;

                case 16:
                    ushort* pSrc16;
                    ushort* pOpr16;
                    ushort* pRes16;
                    for (int i = 0; i < height; i++)
                    {
                        pSrc16 = (ushort*)(pSrcBuffer + (i * bufferWidth));
                        pOpr16 = (ushort*)(pOprBuffer + (i * bufferWidth));
                        pRes16 = (ushort*)(pResBuffer + (i * bufferWidth));
                        for (int j = 0; j < width; j++)
                        {
                            resultValue = (*pSrc16++) + (*pOpr16++);
                            *pRes16++ = (ushort)((resultValue > 65535) ? 65535 : resultValue);
                        }
                    }
                    break;

                case 24:
                    for (int i = 0; i < height; i++)
                    {
                        pSrc = pSrcBuffer + (i * bufferWidth);
                        pOpr = pOprBuffer + (i * bufferWidth);
                        pRes = pResBuffer + (i * bufferWidth);

                        for (int j = 0; j < width; j++)
                        {
                            resultValue = (*pSrc++) + (*pOpr++);
                            *pRes++ = (byte)((resultValue > 255) ? 255 : resultValue);
                            resultValue = (*pSrc++) + (*pOpr++);
                            *pRes++ = (byte)((resultValue > 255) ? 255 : resultValue);
                            resultValue = (*pSrc++) + (*pOpr++);
                            *pRes++ = (byte)((resultValue > 255) ? 255 : resultValue);
                        }
                    }
                    break;
            }

            // Releases the back buffer to make it available for display.
            srcimg.Unlock();
            oprimg.Unlock();
            resimg.Unlock();
            //resimg.Freeze();

            return resimg;
        }
        #endregion

        #region public WriteableBitmap Add(WriteableBitmap[] arrImages)
        /// <summary>
        /// Returns the sum of all passed images
        /// </summary>
        /// <param name="arrImages">Array of images</param>
        /// <returns></returns>
        public WriteableBitmap Add(WriteableBitmap[] arrImages)
        {
            if (arrImages.Length >= 1)
            {
                ValidateImages(arrImages);
                WriteableBitmap tempImage = arrImages[0];

                for (int i = 1; i < arrImages.Length; i++)
                {
                    tempImage = AddImage(ref tempImage, ref arrImages[i]);
                }

                return tempImage;
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region public unsafe WriteableBitmap SubtractImage(ref WriteableBitmap srcimg, ref WriteableBitmap oprimg)
        /// <summary>
        /// Subtracts pixel values of two images
        /// </summary>
        /// <param name="srcimg">Source image</param>
        /// <param name="oprimg">Operator image</param>
        /// <returns>Result image</returns>
        public unsafe WriteableBitmap SubtractImage(WriteableBitmap srcimg, WriteableBitmap oprimg)
        {
            if (srcimg == null || oprimg == null) { return null; }

            ValidateImages(new WriteableBitmap[] { srcimg, oprimg });

            double dDpiX = srcimg.DpiX;
            double dDpiY = srcimg.DpiY;

            WriteableBitmap resimg = new WriteableBitmap(srcimg.PixelWidth, srcimg.PixelHeight, dDpiX, dDpiY, PixelFormats.Gray16, null);

            int width = srcimg.PixelWidth;
            int height = srcimg.PixelHeight;
            int bitsPerPixel = srcimg.Format.BitsPerPixel;
            //int stride = 4 * ((bitsPerPixel * width + 31) / 32);
            //int buffSize = height * stride;
            int bufferWidth = srcimg.BackBufferStride;
            int resultValue = 0;

            // Reserves the back buffer for updates.
            if (!srcimg.IsFrozen)
                srcimg.Lock();
            if (!oprimg.IsFrozen)
                oprimg.Lock();
            if (!resimg.IsFrozen)
                resimg.Lock();

            switch (bitsPerPixel)
            {
                case 8:
                    throw new NotImplementedException("8-bit image format is not yet implemented.");
                case 16:
                    ushort* pSrc16;
                    ushort* pOpr16;
                    ushort* pRes16;
                    for (int i = 0; i < height; i++)
                    {
                        pSrc16 = (ushort*)((byte*)(void*)srcimg.BackBuffer.ToPointer() + (i * bufferWidth));
                        pOpr16 = (ushort*)((byte*)(void*)oprimg.BackBuffer.ToPointer() + (i * bufferWidth));
                        pRes16 = (ushort*)((byte*)(void*)resimg.BackBuffer.ToPointer() + (i * bufferWidth));
                        for (int j = 0; j < width; j++)
                        {
                            resultValue = (*pSrc16++) - (*pOpr16++);
                            *pRes16++ = (ushort)((resultValue < 0) ? 0 : resultValue);
                        }
                    }
                    //uint imgSize = (uint)(width * height);
                    //ushort* pSrc16 = (ushort*)srcimg.BackBuffer.ToPointer();
                    //ushort* pOpr16 = (ushort*)oprimg.BackBuffer.ToPointer();
                    //ushort* pRes16 = (ushort*)resimg.BackBuffer.ToPointer();
                    //for (int i = 0; i < imgSize; i++)
                    //{
                    //    resultValue = (*pSrc16++) - (*pOpr16++);
                    //    *pRes16++ = (ushort)((resultValue < 0) ? 0 : resultValue);
                    //}
                    break;
                case 24:
                    throw new NotImplementedException("24-bit image format is not yet implemented.");
                default:
                    throw new ArgumentException("Unsupported image format.");
            }

            // Releases the back buffer to make it available for display.
            if (!srcimg.IsFrozen)
                srcimg.Unlock();
            if (!oprimg.IsFrozen)
                oprimg.Unlock();
            if (!resimg.IsFrozen)
                resimg.Unlock();

            return resimg;
        }
        #endregion

        #region public unsafe WriteableBitmap Add(WriteableBitmap srcImage, int oprValue)
        /// <summary>
        /// Add each image pixel by the parameter value.
        /// </summary>
        /// <param name="srcImage"></param>
        /// <param name="oprValue"></param>
        /// <returns></returns>
        public unsafe WriteableBitmap Add(WriteableBitmap srcImage, int oprValue)
        {
            if (srcImage == null || srcImage.PixelWidth == 0 || srcImage.PixelHeight == 0)
            {
                throw new ArgumentNullException("Source image cannot be null or empty.");
            }

            int iResult = 0;
            int width = srcImage.PixelWidth;
            int height = srcImage.PixelHeight;
            int bitsPerPixel = srcImage.Format.BitsPerPixel;
            //int stride = 4 * ((bitsPerPixel * width + 31) / 32);
            //int bytesPerPixel = (bitsPerPixel + 7) / 8;
            //int stride = 4 * ((width * bytesPerPixel + 3) / 4);
            //int buffSize = height * stride;
            int bufferWidth = srcImage.BackBufferStride;

            // Reserves the back buffer for updates.
            srcImage.Lock();

            switch (bitsPerPixel)
            {
                case 8:
                    throw new NotImplementedException("8-bit image format is not yet implemented.");

                case 16:
                    ushort* pShort;
                    for (int i = 0; i < height; i++)
                    {
                        pShort = (ushort*)((byte*)(void*)srcImage.BackBuffer.ToPointer() + (i * bufferWidth));
                        for (int j = 0; j < width; j++)
                        {
                            iResult = *(pShort) + oprValue;
                            if (iResult > 65535)
                            {
                                iResult = 65535;
                            }
                            *pShort++ = (ushort)iResult;
                        }
                    }

                    //uint imgSize = (uint)(width * height);
                    //ushort* pBuffer = (ushort*)srcImage.BackBuffer.ToPointer();
                    //for (int i = 0; i < imgSize; i++)
                    //{
                    //    iResult = *(pBuffer) + oprValue;
                    //    if (iResult > 65535)
                    //    {
                    //        iResult = 65535;
                    //    }
                    //    *pBuffer++ = (ushort)iResult;
                    //}
                    break;

                case 32:
                    throw new NotImplementedException("32-bit image format is not yet implemented.");

                default:
                    throw new ArgumentException("Unsupported image format.");
            }

            // Releases the back buffer to make it available for display.
            srcImage.Unlock();
            //srcImage.Freeze();

            return srcImage;
        }
        #endregion

        #region public unsafe WriteableBitmap Subtract(WriteableBitmap srcImage, int oprValue)
        /// <summary>
        /// Subtract each image pixel by the parameter value.
        /// </summary>
        /// <param name="srcImage"></param>
        /// <param name="oprValue"></param>
        /// <returns></returns>
        public unsafe WriteableBitmap Subtract(WriteableBitmap srcImage, int oprValue)
        {
            if (srcImage == null || srcImage.PixelWidth == 0 || srcImage.PixelHeight == 0)
            {
                throw new ArgumentNullException("Source image cannot be null or empty.");
            }

            int iResult = 0;
            int width = srcImage.PixelWidth;
            int height = srcImage.PixelHeight;
            int bitsPerPixel = srcImage.Format.BitsPerPixel;
            //int stride = 4 * ((bitsPerPixel * width + 31) / 32);
            //int bytesPerPixel = (bitsPerPixel + 7) / 8;
            //int stride = 4 * ((width * bytesPerPixel + 3) / 4);
            //int buffSize = height * stride;
            int bufferWidth = srcImage.BackBufferStride;

            // Reserves the back buffer for updates.
            if (!srcImage.IsFrozen)
                srcImage.Lock();

            switch (bitsPerPixel)
            {
                case 8:
                    throw new NotImplementedException("8-bit image format is not yet implemented.");

                case 16:
                    ushort* pShort;
                    for (int i = 0; i < height; i++)
                    {
                        pShort = (ushort*)((byte*)(void*)srcImage.BackBuffer.ToPointer() + (i * bufferWidth));
                        for (int j = 0; j < width; j++)
                        {
                            iResult = *(pShort) - oprValue;
                            if (iResult < 0)
                            {
                                iResult = 0;
                            }
                            *pShort++ = (ushort)iResult;
                        }
                    }

                    //uint imgSize = (uint)(width * height);
                    //ushort* pBuffer = (ushort*)srcImage.BackBuffer.ToPointer();
                    //for (int i = 0; i < imgSize; i++)
                    //{
                    //    iResult = *(pBuffer) - oprValue;
                    //    if (iResult < 0)
                    //    {
                    //        iResult = 0;
                    //    }
                    //    *pBuffer++ = (ushort)iResult;
                    //}
                    break;

                case 32:
                    throw new NotImplementedException("32-bit image format is not yet implemented.");

                default:
                    throw new ArgumentException("Unsupported image format.");
            }

            // Releases the back buffer to make it available for display.
            if (!srcImage.IsFrozen)
                srcImage.Unlock();

            return srcImage;
        }
        #endregion

        #region public unsafe WriteableBitmap Multiply(WriteableBitmap srcImage, double oprValue)
        /// <summary>
        /// Multiply each image pixel by the parameter value.
        /// </summary>
        /// <param name="srcImage"></param>
        /// <param name="oprValue"></param>
        /// <returns></returns>
        public unsafe WriteableBitmap Multiply(WriteableBitmap srcImage, double oprValue)
        {
            if ((oprValue == 1.0) || (srcImage == null))
                return srcImage;

            var cpyImage = (WriteableBitmap)srcImage.Clone();

            int width = srcImage.PixelWidth;
            int height = srcImage.PixelHeight;
            int bitsPerPixel = srcImage.Format.BitsPerPixel;
            //int stride = 4 * ((bitsPerPixel * width + 31) / 32);
            //int stride = (width * bitsPerPixel + 7) / 8;
            //int buffSize = height * stride;
            //byte[] srcBuffer = new byte[width * height * 2];
            //srcImage.CopyPixels(srcBuffer, stride, 0);
            //int bytesPerPixel = (bitsPerPixel + 7) / 8;
            //int bufferWidth = 4 * ((width * bytesPerPixel + 3) / 4);
            int i, j;
            byte* pByte;
            ushort* pShort;
            int bufferWidth = srcImage.BackBufferStride;

            // Reserves the back buffer for updates.
            if (!srcImage.IsFrozen)
                srcImage.Lock();
            if (!cpyImage.IsFrozen)
                cpyImage.Lock();

            switch (bitsPerPixel)
            {
                case 8:
                    for (i = 0; i < height; i++)
                    {
                        pByte = (byte*)cpyImage.BackBuffer.ToPointer() + (i * bufferWidth);
                        for (j = 0; j < width; j++)
                        {
                            double value = oprValue * ((double)*pByte) + 0.5;
                            if (value > 255)
                            {
                                value = 255;
                            }
                            *pByte++ = (byte)value;
                        }
                    }
                    break;

                case 16:
                case 12:
                    for (i = 0; i < height; i++)
                    {
                        pShort = (ushort*)((byte*)(void*)cpyImage.BackBuffer.ToPointer() + (i * bufferWidth));
                        for (j = 0; j < width; j++)
                        {
                            double value = oprValue * ((double)*pShort) + 0.5;

                            if (value > 65535)
                            {
                                value = 65535;
                            }

                            *pShort++ = (ushort)value;
                        }
                    }
                    break;

                case 24:
                    for (i = 0; i < height; i++)
                    {
                        pByte = (byte*)cpyImage.BackBuffer.ToPointer() + (i * bufferWidth);
                        for (j = 0; j < width; j++)
                        {
                            double value = oprValue * ((double)*pByte) + 0.5;
                            if (value > 255)
                            {
                                value = 255;
                            }
                            *pByte++ = (byte)value;

                            value = oprValue * ((double)*pByte) + 0.5;
                            if (value > 255)
                            {
                                value = 255;
                            }
                            *pByte++ = (byte)value;

                            value = oprValue * ((double)*pByte) + 0.5;
                            if (value > 255)
                            {
                                value = 255;
                            }
                            *pByte++ = (byte)value;
                        }
                    }
                    break;

                default:
                    break;
            }

            // Releases the back buffer to make it available for display.
            if (!srcImage.IsFrozen)
                srcImage.Unlock();
            if (!cpyImage.IsFrozen)
                cpyImage.Unlock();

            return cpyImage;
        }
        #endregion

        #region public unsafe WriteableBitmap Divide(WriteableBitmap srcImage, double oprValue)
        /// <summary>
        /// Divide each image pixel by an integer parameter value.
        /// </summary>
        /// <param name="srcImage"></param>
        /// <param name="oprValue"></param>
        /// <returns></returns>
        public unsafe WriteableBitmap Divide(WriteableBitmap srcImage, int oprValue)
        {
            if ((oprValue == 1.0) || (oprValue == 1.0) || (srcImage == null))
                return srcImage;

            int width = srcImage.PixelWidth;
            int height = srcImage.PixelHeight;
            int bitsPerPixel = srcImage.Format.BitsPerPixel;
            int stride = 4 * ((bitsPerPixel * width + 31) / 32);
            //int stride = (width * bitsPerPixel + 7) / 8;
            int buffSize = height * stride;
            //byte[] srcBuffer = new byte[width * height * 2];
            //srcImage.CopyPixels(srcBuffer, stride, 0);
            //int bytesPerPixel = (bitsPerPixel + 7) / 8;
            //int bufferWidth = 4 * ((width * bytesPerPixel + 3) / 4);
            int i, j;
            byte* pByte;
            ushort* pShort;
            int bufferWidth = srcImage.BackBufferStride;

            // Reserves the back buffer for updates.
            srcImage.Lock();

            switch (bitsPerPixel)
            {
                case 8:
                    for (i = 0; i < height; i++)
                    {
                        pByte = (byte*)srcImage.BackBuffer.ToPointer() + (i * bufferWidth);
                        for (j = 0; j < width; j++)
                        {
                            int value = ((int)*pByte) / oprValue;
                            if (value > 255)
                            {
                                value = 255;
                            }
                            *pByte++ = (byte)value;
                        }
                    }
                    break;

                case 16:
                case 12:
                    for (i = 0; i < height; i++)
                    {
                        pShort = (ushort*)((byte*)(void*)srcImage.BackBuffer.ToPointer() + (i * bufferWidth));
                        for (j = 0; j < width; j++)
                        {
                            int value = ((int)*pShort) / oprValue;

                            if (value > 65535)
                            {
                                value = 65535;
                            }

                            *pShort++ = (ushort)value;
                        }
                    }
                    break;

                case 24:
                    for (i = 0; i < height; i++)
                    {
                        pByte = (byte*)srcImage.BackBuffer.ToPointer() + (i * bufferWidth);
                        for (j = 0; j < width; j++)
                        {
                            int value = ((int)*pByte) / oprValue;
                            if (value > 255)
                            {
                                value = 255;
                            }
                            *pByte++ = (byte)value;

                            value = ((int)*pByte) / oprValue;
                            if (value > 255)
                            {
                                value = 255;
                            }
                            *pByte++ = (byte)value;

                            value = ((int)*pByte) / oprValue;
                            if (value > 255)
                            {
                                value = 255;
                            }
                            *pByte++ = (byte)value;
                        }
                    }
                    break;

                default:
                    break;
            }

            // Releases the back buffer to make it available for display.
            srcImage.Unlock();
            //srcImage.Freeze();

            return srcImage;
        }
        #endregion

        #region public unsafe WriteableBitmap MulC(WriteableBitmap srcImage, double oprValue)
        /// <summary>
        /// Multiply each image pixel by the parameter value
        /// Not inplace / non-destructive.
        /// </summary>
        /// <param name="srcImage"></param>
        /// <param name="oprValue"></param>
        /// <returns></returns>
        public unsafe WriteableBitmap MulC(WriteableBitmap srcImage, double oprValue)
        {
            if ((oprValue == 1.0) || (srcImage == null))
                return srcImage;

            int width = srcImage.PixelWidth;
            int height = srcImage.PixelHeight;
            int bitsPerPixel = srcImage.Format.BitsPerPixel;
            byte* pByte;
            ushort* pShort;
            int bufferWidth = srcImage.BackBufferStride;
            WriteableBitmap dstImage = (WriteableBitmap)srcImage.Clone();

            // Reserves the back buffer for updates.
            if (!srcImage.IsFrozen)
                srcImage.Lock();
            dstImage.Lock();

            byte* pDstBuffer = (byte*)dstImage.BackBuffer.ToPointer();

            switch (bitsPerPixel)
            {
                case 8:
                    for (int i = 0; i < height; i++)
                    {
                        //pByte = (byte*)dstImage.BackBuffer.ToPointer() + (i * bufferWidth);
                        pByte = (byte*)(pDstBuffer + (i * bufferWidth));
                        for (int j = 0; j < width; j++)
                        {
                            double value = oprValue * ((double)*pByte) + 0.5;
                            if (value > 255)
                            {
                                value = 255;
                            }
                            *pByte++ = (byte)value;
                        }
                    }
                    break;

                case 16:
                case 12:
                    for (int i = 0; i < height; i++)
                    {
                        //pShort = (ushort*)((byte*)(void*)dstImage.BackBuffer.ToPointer() + (i * bufferWidth));
                        pShort = (ushort*)(pDstBuffer + (i * bufferWidth));
                        for (int j = 0; j < width; j++)
                        {
                            double value = oprValue * ((double)*pShort) + 0.5;

                            if (value > 65535)
                            {
                                value = 65535;
                            }
                            *pShort++ = (ushort)value;
                        }
                    }
                    break;

                case 24:
                    for (int i = 0; i < height; i++)
                    {
                        //pByte = (byte*)dstImage.BackBuffer.ToPointer() + (i * bufferWidth);
                        pByte = (byte*)(pDstBuffer + (i * bufferWidth));
                        for (int j = 0; j < width; j++)
                        {
                            double value = oprValue * ((double)*pByte) + 0.5;
                            if (value > 255)
                            {
                                value = 255;
                            }
                            *pByte++ = (byte)value;

                            value = oprValue * ((double)*pByte) + 0.5;
                            if (value > 255)
                            {
                                value = 255;
                            }
                            *pByte++ = (byte)value;

                            value = oprValue * ((double)*pByte) + 0.5;
                            if (value > 255)
                            {
                                value = 255;
                            }
                            *pByte++ = (byte)value;
                        }
                    }
                    break;

                default:
                    break;
            }

            // Releases the back buffer to make it available for display.
            if (!srcImage.IsFrozen)
                srcImage.Lock();
            dstImage.Unlock();

            return dstImage;
        }
        #endregion

        #region public unsafe WriteableBitmap DivC(WriteableBitmap srcImage, double oprValue)
        /// <summary>
        /// Divide each image pixel by an integer parameter value.
        /// Not inplace/non-destructive.
        /// </summary>
        /// <param name="srcImage"></param>
        /// <param name="oprValue"></param>
        /// <returns></returns>
        public unsafe WriteableBitmap DivC(WriteableBitmap srcImage, int oprValue)
        {
            if ((oprValue == 1.0) || (oprValue == 1.0) || (srcImage == null))
                return srcImage;

            int width = srcImage.PixelWidth;
            int height = srcImage.PixelHeight;
            int bitsPerPixel = srcImage.Format.BitsPerPixel;
            //int stride = 4 * ((bitsPerPixel * width + 31) / 32);
            //int stride = (width * bitsPerPixel + 7) / 8;
            //int buffSize = height * stride;
            //byte[] srcBuffer = new byte[width * height * 2];
            //srcImage.CopyPixels(srcBuffer, stride, 0);
            //int bytesPerPixel = (bitsPerPixel + 7) / 8;
            //int bufferWidth = 4 * ((width * bytesPerPixel + 3) / 4);
            int i, j;
            byte* pByte;
            ushort* pShort;
            int bufferWidth = srcImage.BackBufferStride;
            WriteableBitmap dstImage = (WriteableBitmap)srcImage.Clone();

            // Reserves the back buffer for updates.
            dstImage.Lock();

            switch (bitsPerPixel)
            {
                case 8:
                    for (i = 0; i < height; i++)
                    {
                        pByte = (byte*)dstImage.BackBuffer.ToPointer() + (i * bufferWidth);
                        for (j = 0; j < width; j++)
                        {
                            int value = ((int)*pByte) / oprValue;
                            if (value > 255)
                            {
                                value = 255;
                            }
                            *pByte++ = (byte)value;
                        }
                    }
                    break;

                case 16:
                case 12:
                    for (i = 0; i < height; i++)
                    {
                        pShort = (ushort*)((byte*)(void*)dstImage.BackBuffer.ToPointer() + (i * bufferWidth));
                        for (j = 0; j < width; j++)
                        {
                            int value = ((int)*pShort) / oprValue;

                            if (value > 65535)
                            {
                                value = 65535;
                            }

                            *pShort++ = (ushort)value;
                        }
                    }
                    break;

                case 24:
                    for (i = 0; i < height; i++)
                    {
                        pByte = (byte*)dstImage.BackBuffer.ToPointer() + (i * bufferWidth);
                        for (j = 0; j < width; j++)
                        {
                            int value = ((int)*pByte) / oprValue;
                            if (value > 255)
                            {
                                value = 255;
                            }
                            *pByte++ = (byte)value;

                            value = ((int)*pByte) / oprValue;
                            if (value > 255)
                            {
                                value = 255;
                            }
                            *pByte++ = (byte)value;

                            value = ((int)*pByte) / oprValue;
                            if (value > 255)
                            {
                                value = 255;
                            }
                            *pByte++ = (byte)value;
                        }
                    }
                    break;

                default:
                    break;
            }

            // Releases the back buffer to make it available for display.
            dstImage.Unlock();
            //srcImage.Freeze();

            return dstImage;
        }
        #endregion

        #region public unsafe WriteableBitmap MultiplyImage(ref WriteableBitmap srcimg, ref WriteableBitmap oprimg)
        /// <summary>
        /// Adds pixel values of two images
        /// </summary>
        /// <param name="srcimg">Source image</param>
        /// <param name="oprimg">Operator image</param>
        /// <returns>Result image</returns>
        public unsafe WriteableBitmap MultiplyImage(ref WriteableBitmap srcimg, ref WriteableBitmap oprimg)
        {
            ValidateImages(new WriteableBitmap[] { srcimg, oprimg });

            double dDpiX = srcimg.DpiX;
            double dDpiY = srcimg.DpiY;

            WriteableBitmap resimg = new WriteableBitmap(srcimg.PixelWidth, srcimg.PixelHeight, dDpiX, dDpiY, PixelFormats.Gray16, null);

            int width = srcimg.PixelWidth;
            int height = srcimg.PixelHeight;
            int bitsPerPixel = srcimg.Format.BitsPerPixel;
            int stride = 4 * ((bitsPerPixel * width + 31) / 32);
            //int stride = (width * bitsPerPixel + 7) / 8;
            int buffSize = height * stride;
            int bufferWidth = srcimg.BackBufferStride;
            double resultValue = 0;

            // Reserves the back buffer for updates.
            srcimg.Lock();
            oprimg.Lock();
            resimg.Lock();

            byte* pSrcBuffer = (byte*)srcimg.BackBuffer.ToPointer();
            byte* pOprBuffer = (byte*)oprimg.BackBuffer.ToPointer();
            byte* pResBuffer = (byte*)resimg.BackBuffer.ToPointer();

            byte* pSrc = null;
            byte* pOpr = null;
            byte* pRes = null;

            switch (bitsPerPixel)
            {
                case 8:
                    for (int i = 0; i < height; i++)
                    {
                        pSrc = pSrcBuffer + (i * bufferWidth);
                        pOpr = pOprBuffer + (i * bufferWidth);
                        pRes = pResBuffer + (i * bufferWidth);

                        for (int j = 0; j < width; j++)
                        {
                            resultValue = (*pSrc++) * (*pOpr++);
                            *pRes++ = (byte)((resultValue > 255) ? 255 : resultValue);
                        }
                    }
                    break;

                case 16:
                    ushort* pSrc16;
                    ushort* pOpr16;
                    ushort* pRes16;
                    for (int i = 0; i < height; i++)
                    {
                        pSrc16 = (ushort*)(pSrcBuffer + (i * bufferWidth));
                        pOpr16 = (ushort*)(pOprBuffer + (i * bufferWidth));
                        pRes16 = (ushort*)(pResBuffer + (i * bufferWidth));
                        for (int j = 0; j < width; j++)
                        {
                            resultValue = (*pSrc16++) * (*pOpr16++);
                            *pRes16++ = (ushort)((resultValue > 65535) ? 65535 : resultValue);
                        }
                    }
                    break;

                case 24:
                    for (int i = 0; i < height; i++)
                    {
                        pSrc = pSrcBuffer + (i * bufferWidth);
                        pOpr = pOprBuffer + (i * bufferWidth);
                        pRes = pResBuffer + (i * bufferWidth);

                        for (int j = 0; j < width; j++)
                        {
                            resultValue = (*pSrc++) * (*pOpr++);
                            *pRes++ = (byte)((resultValue > 255) ? 255 : resultValue);
                            resultValue = (*pSrc++) * (*pOpr++);
                            *pRes++ = (byte)((resultValue > 255) ? 255 : resultValue);
                            resultValue = (*pSrc++) * (*pOpr++);
                            *pRes++ = (byte)((resultValue > 255) ? 255 : resultValue);
                        }
                    }
                    break;
            }

            // Releases the back buffer to make it available for display.
            srcimg.Unlock();
            oprimg.Unlock();
            resimg.Unlock();
            //resimg.Freeze();

            return resimg;
        }
        #endregion

        #region private void ValidateImages(BitmapSource[] arrImages)
        /// <summary>
        /// Compare images size, and bit depth validation
        /// </summary>
        /// <param name="arrImages"></param>
        private void ValidateImages(WriteableBitmap[] arrImages)
        {
            if (arrImages.Length >= 1)
            {
                WriteableBitmap firstImage = arrImages[0];

                for (int i = 1; i < arrImages.Length; i++)
                {
                    WriteableBitmap secondImage = arrImages[i];
                    if ((firstImage.PixelWidth != secondImage.PixelWidth) ||
                        (firstImage.PixelHeight != secondImage.PixelHeight) ||
                        (firstImage.Format.BitsPerPixel != secondImage.Format.BitsPerPixel))
                    {
                        throw new Exception("Images size and bit depth should be same.");
                    }
                }
            }
        }
        #endregion

    }
}
