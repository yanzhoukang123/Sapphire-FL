using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Azure.Image.Processing
{
    public class ZProjector
    {
        public enum ProjectionType
        {
            AVG_METHOD = 0,
            MAX_METHOD = 1,
            MIN_METHOD = 2,
            SUM_METHOD = 3,
            SD_METHOD = 4,
            MEDIAN_METHOD = 5,
        }
        public enum ProjectionDataType
        {
            BYTE_TYPE = 0,
            SHORT_TYPE = 1,
            FLOAT_TYPE = 2,
        }

        private int _Width;
        private int _Height;
        /// <summary>
        ///Image to hold z-projection.
        /// </summary>
        private WriteableBitmap _ProjImage = null;
        /// <summary>
        /// List of images to project.
        /// </summary>
        private List<WriteableBitmap> _ProjImageList = null;

        /// <summary>
        /// Projection starts from this slice.
        /// </summary>
        private int _StartSlice = 1;
        /// <summary>
        /// Projection ends at this slice.
        /// </summary>
        private int _StopSlice = 1;
        private int _Increment = 1;
        private int _SliceCount = 0;
        private ProjectionType _ProjMethod = ProjectionType.MAX_METHOD;

        public ZProjector()
        {
        }

        /// <summary>
        /// Construction of ZProjector with image to be projected.
        /// </summary>
        /// <param name="projImages"></param>
        public ZProjector(List<WriteableBitmap> projImages)
        {
            SetImage(projImages);
        }

        public WriteableBitmap ProjImage
        {
            get { return _ProjImage; }
            set { _ProjImage = value; }
        }

        public List<WriteableBitmap> ProjImageList
        {
            get { return _ProjImageList; }
            set { _ProjImageList = value; }
        }

        public ProjectionType ProjectionMethod
        {
            get { return _ProjMethod; }
            set { _ProjMethod = value; }
        }

        /// <summary>
        /// Explicitly set image to be projected. This is useful if
        /// ZProjection_ object is to be used not as a plugin but as a
        /// stand alone processing object.
        /// </summary>
        /// <param name="projImages"></param>
        public void SetImage(List<WriteableBitmap> projImages)
        {
            _ProjImageList = projImages;
            _StartSlice = 1;
            if (projImages != null && projImages.Count > 1)
            {
                _StopSlice = projImages.Count;
                _Width = ProjImageList[0].PixelWidth;
                _Height = ProjImageList[0].PixelHeight;
            }
        }

        /** Performs actual projection using specified method. */
        public void DoProjection()
        {
            if (_ProjImageList == null)
                return;
            //if (imp.getBitDepth() == 24)
            //{
            //    doRGBProjection();
            //    return;
            //}
            _SliceCount = 0;
            if (_ProjMethod < ProjectionType.AVG_METHOD || _ProjMethod > ProjectionType.MEDIAN_METHOD)
                _ProjMethod = ProjectionType.AVG_METHOD;
            for (int slice = _StartSlice; slice <= _StopSlice; slice += _Increment)
                _SliceCount++;

            if (_ProjMethod == ProjectionType.MEDIAN_METHOD)
            {
                _ProjImage = DoMedianProjection();
                return;
            }

            int width = _ProjImageList[0].PixelWidth;
            int height = _ProjImageList[0].PixelHeight;
            float[] pixels = null;
            float[] fpixels = new float[width * height];
            RayFunction rayFunc = GetRayFunction(_ProjMethod, fpixels);

            for (int n = _StartSlice - 1; n <= _StopSlice - 1; n += _Increment)
            {
                //if (!isHyperstack)
                //{
                //    IJ.showStatus("ZProjection " + color + ": " + n + "/" + stopSlice);
                //    IJ.showProgress(n - startSlice, stopSlice - startSlice);
                //}

                //
                // Create new float processor for projected pixels.
                //FloatProcessor fp = new FloatProcessor(imp.getWidth(), imp.getHeight());
                //ImageStack stack = imp.getStack();
                //RayFunction rayFunc = getRayFunction(method, fp);
                //if (IJ.debugMode == true)
                //{
                //    IJ.log("\nProjecting stack from: " + startSlice
                //        + " to: " + stopSlice);
                //}

                // Determine type of input image. Explicit determination of
                // processor type is required for subsequent pixel
                // manipulation.  This approach is more efficient than the
                // more general use of ImageProcessor's getPixelValue and
                // putPixel methods.
                //int ptype;
                //if (stack.getProcessor(1) instanceof ByteProcessor) ptype = BYTE_TYPE;
                //else if (stack.getProcessor(1) instanceof ShortProcessor) ptype = SHORT_TYPE;
                //else if (stack.getProcessor(1) instanceof FloatProcessor) ptype = FLOAT_TYPE;
                //else
                //{
                //    IJ.error("Z Project", "Non-RGB stack required");
                //    return;
                //}

                pixels = ImageProcessing.Convert16u32f(_ProjImageList[n]);

                // Do the projection
                ProjectSlice(pixels, rayFunc, ProjectionDataType.FLOAT_TYPE);
            }

            //
            // Finish up projection.
            if (_ProjMethod == ProjectionType.SUM_METHOD)
            {
                //if (imp.getCalibration().isSigned16Bit())
                //    fp.subtract(sliceCount * 32768.0);
                //fp.resetMinAndMax();
                //_ProjImage = new ImagePlus(makeTitle(), fp);
                _ProjImage = ImageProcessing.Convert32f16u(fpixels, width, height);
            }
            else if (_ProjMethod == ProjectionType.SD_METHOD)
            {
                rayFunc.PostProcess();
                //fp.resetMinAndMax();
                //_ProjImage = new ImagePlus(makeTitle(), fp);
                _ProjImage = ImageProcessing.Convert32f16u(fpixels, width, height);
            }
            else
            {
                rayFunc.PostProcess();
                //_ProjImage = makeOutputImage(imp, fp, ptype);
                _ProjImage = ImageProcessing.Convert32f16u(fpixels, width, height);
            }
            //
            //if (_ProjImage == null)
            //{
            //    //IJ.error("Z Project", "Error computing projection.");
            //}
            //_ProjImage = ImageProcessing.Convert32f16u(fpixels, width, height);
        }


        private RayFunction GetRayFunction(ProjectionType method, float[] fpixels)
        {
            switch (method)
            {
                case ProjectionType.AVG_METHOD:
                case ProjectionType.SUM_METHOD:
                    return new AverageIntensity(fpixels, _SliceCount);
                case ProjectionType.MAX_METHOD:
                    return new MaxIntensity(fpixels);
                case ProjectionType.MIN_METHOD:
                    return new MinIntensity(fpixels);
                case ProjectionType.SD_METHOD:
                    return new StandardDeviation(fpixels, _SliceCount);
                default:
                    throw new Exception("Z Project: Unknown projection method.");
            }
        }

        /** Handles mechanics of projection by selecting appropriate pixel
        array type. We do this rather than using more general
        ImageProcessor getPixelValue() and putPixel() methods because
        direct manipulation of pixel arrays is much more efficient.  */
        private void ProjectSlice(Object pixelArray, RayFunction rayFunc, ProjectionDataType pdtype)
        {
            switch (pdtype)
            {
                case ProjectionDataType.BYTE_TYPE:
                    rayFunc.ProjectSlice((byte[])pixelArray);
                    break;
                case ProjectionDataType.SHORT_TYPE:
                    rayFunc.ProjectSlice((short[])pixelArray);
                    break;
                case ProjectionDataType.FLOAT_TYPE:
                    rayFunc.ProjectSlice((float[])pixelArray);
                    break;
            }
        }

        /*WriteableBitmap DoMedianProjection()
        {
            //IJ.showStatus("Calculating median...");
            //ImageStack stack = imp.getStack();
            //ImageProcessor[] slices = new ImageProcessor[sliceCount];
            //List<float[]> slices = new List<float[]>();
            float[][] slices = new float[_SliceCount][];
            int index = 0;
            for (int slice = _StartSlice - 1; slice <= _StopSlice - 1; slice += _Increment)
            {
                //index++;
                //slices.Add(ImageProcessing.Convert16u32f(ProjImageList[slice]));
                slices[index++] = ImageProcessing.Convert16u32f(ProjImageList[slice]);
            }
            //ImageProcessor ip2 = slices[0].duplicate();
            //ip2 = ip2.convertToFloat();
            float[] ip2 = ImageProcessing.Convert16u32f(ProjImageList[0].Clone());
            float[] values = new float[_SliceCount];
            int width = ProjImageList[0].PixelWidth;
            int height = ProjImageList[0].PixelHeight;
            //int stride = ProjImageList[0].BackBufferStride;
            int stride = width * sizeof(float) / 2;
            int len = ip2.Length;

            index = 0;
            //int inc = Math.Max(height / 30, 1);
            for (int y = 0; y < height; y++)
            {
                //if (y % inc == 0) IJ.showProgress(y, height - 1);
                for (int x = 0; x < width; x++)
                {
                    try
                    {
                        index = stride * y + x;
                        for (int i = 0; i < _SliceCount; i++)
                        {
                            //values[i] = slices[i].getPixelValue(x, y);
                            if (index < len)
                                values[i] = slices[i][index];
                        }
                        if (index < len)
                            ip2[index] = Median(values);
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            //if (ProjImageList[0].Format.BitsPerPixel == 8)
            //    ip2 = ip2.convertToByte(false);
            //IJ.showProgress(1, 1);
            WriteableBitmap result = ImageProcessing.Convert32f16u(ip2, width, height);
            return result;
        }*/

        unsafe WriteableBitmap DoMedianProjection()
        {
            byte*[] slices = new byte*[_SliceCount];
            int index = 0;
            for (int slice = _StartSlice - 1; slice <= _StopSlice - 1; slice += _Increment)
            {
                slices[index++] = (byte*)ProjImageList[slice].BackBuffer.ToPointer();
            }
            float[] values = new float[_SliceCount];
            int width = ProjImageList[0].PixelWidth;
            int height = ProjImageList[0].PixelHeight;
            int bufWidth = ProjImageList[0].BackBufferStride;
            WriteableBitmap ip2 = ProjImageList[0].Clone();
            byte* pDstData = (byte*)ip2.BackBuffer.ToPointer();
            ushort* pDst16 = null;
            ushort* pSrc16 = null;
            index = 0;
            for (int y = 0; y < height; y++)
            {
                pDst16 = (ushort*)(pDstData + (y * bufWidth));
                for (int x = 0; x < width; x++)
                {
                    for (int i = 0; i < _SliceCount; i++)
                    {
                        pSrc16 = (ushort*)(slices[i] + (y * bufWidth));
                        values[i] = *(pSrc16 + x);
                    }
                    *pDst16++ = (ushort)Median(values);
                }
            }
            return ip2;
        }

        float Median(float[] arr)
        {
            //Arrays.sort(a);
            Array.Sort(arr);
            int middle = arr.Length / 2;
            if ((arr.Length & 1) == 0) //even
                return (arr[middle - 1] + arr[middle]) / 2f;
            else
                return arr[middle];
        }


        /** Abstract class that specifies structure of ray
            function. Preprocessing should be done in derived class
            constructors.
        */
        public abstract class RayFunction
        {
            /** Do actual slice projection for specific data types. */
            public abstract void ProjectSlice(byte[] pixels);
            public abstract void ProjectSlice(short[] pixels);
            public abstract void ProjectSlice(float[] pixels);

            /** Perform any necessary post processing operations, e.g.
                averging values. */
            public virtual void PostProcess() { }

        } // end RayFunction

        /** Compute average intensity projection. */
        public class AverageIntensity : RayFunction
        {
            private float[] fpixels;
            private int num, len;

            /** Constructor requires number of slices to be
                projected. This is used to determine average at each
                pixel. */
            public AverageIntensity(float[] pixels, int num)
            {
                //fpixels = (float[])fp.getPixels();
                fpixels = pixels;
                len = fpixels.Length;
                this.num = num;
            }

            public override void ProjectSlice(byte[] pixels)
            {
                for (int i = 0; i < len; i++)
                    fpixels[i] += (pixels[i] & 0xff);
            }

            public override void ProjectSlice(short[] pixels)
            {
                for (int i = 0; i < len; i++)
                    fpixels[i] += pixels[i] & 0xffff;
            }

            public override void ProjectSlice(float[] pixels)
            {
                for (int i = 0; i < len; i++)
                    fpixels[i] += pixels[i];
            }

            public override void PostProcess()
            {
                float fnum = num;
                for (int i = 0; i < len; i++)
                    fpixels[i] /= fnum;
            }

        } // end AverageIntensity

        /** Compute max intensity projection. */
        public class MaxIntensity : RayFunction
        {
            private float[] fpixels;
            private int len;

            /** Simple constructor since no preprocessing is necessary. */
            public MaxIntensity(float[] pixels)
            {
                //fpixels = (float[])fp.getPixels();
                fpixels = pixels;
                len = fpixels.Length;
                for (int i = 0; i < len; i++)
                    fpixels[i] = -float.MaxValue;
            }

            public override void ProjectSlice(byte[] pixels)
            {
                for (int i = 0; i < len; i++)
                {
                    if ((pixels[i] & 0xff) > fpixels[i])
                        fpixels[i] = (pixels[i] & 0xff);
                }
            }

            public override void ProjectSlice(short[] pixels)
            {
                for (int i = 0; i < len; i++)
                {
                    if ((pixels[i] & 0xffff) > fpixels[i])
                        fpixels[i] = pixels[i] & 0xffff;
                }
            }

            public override void ProjectSlice(float[] pixels)
            {
                for (int i = 0; i < len; i++)
                {
                    if (!float.IsNaN(pixels[i]) && pixels[i] > fpixels[i])
                        fpixels[i] = pixels[i];
                }
            }

        } // end MaxIntensity

        /** Compute min intensity projection. */
        public class MinIntensity : RayFunction
        {
            private float[] fpixels;
            private int len;

            /** Simple constructor since no preprocessing is necessary. */
            public MinIntensity(float[] pixels)
            {
                //fpixels = (float[])fp.getPixels();
                fpixels = pixels;
                len = fpixels.Length;
                for (int i = 0; i < len; i++)
                    fpixels[i] = float.MaxValue;
            }

            public override void ProjectSlice(byte[] pixels)
            {
                for (int i = 0; i < len; i++)
                {
                    if ((pixels[i] & 0xff) < fpixels[i])
                        fpixels[i] = (pixels[i] & 0xff);
                }
            }

            public override void ProjectSlice(short[] pixels)
            {
                for (int i = 0; i < len; i++)
                {
                    if ((pixels[i] & 0xffff) < fpixels[i])
                        fpixels[i] = pixels[i] & 0xffff;
                }
            }

            public override void ProjectSlice(float[] pixels)
            {
                for (int i = 0; i < len; i++)
                {
                    if (pixels[i] < fpixels[i])
                        fpixels[i] = pixels[i];
                }
            }

        } // end MaxIntensity

        /** Compute standard deviation projection. */
        public class StandardDeviation : RayFunction
        {
            //private float[] result;
            private float[] fpixels;
            private double[] sum, sum2;
            private int num, len;

            public StandardDeviation(float[] pixels, int num)
            {
                //result = (float[])fp.getPixels();
                //result = pixels;
                //len = result.Length;
                fpixels = pixels;
                len = fpixels.Length;
                this.num = num;
                sum = new double[len];
                sum2 = new double[len];
            }

            public override void ProjectSlice(byte[] pixels)
            {
                int v;
                for (int i = 0; i < len; i++)
                {
                    v = pixels[i] & 0xff;
                    sum[i] += v;
                    sum2[i] += v * v;
                }
            }

            public override void ProjectSlice(short[] pixels)
            {
                double v;
                for (int i = 0; i < len; i++)
                {
                    v = pixels[i] & 0xffff;
                    sum[i] += v;
                    sum2[i] += v * v;
                }
            }

            public override void ProjectSlice(float[] pixels)
            {
                double v;
                for (int i = 0; i < len; i++)
                {
                    v = pixels[i];
                    sum[i] += v;
                    sum2[i] += v * v;
                }
            }

            public override void PostProcess()
            {
                double stdDev;
                double n = num;
                for (int i = 0; i < len; i++)
                {
                    if (num > 1)
                    {
                        stdDev = (n * sum2[i] - sum[i] * sum[i]) / n;
                        if (stdDev > 0.0)
                            //result[i] = (float)Math.Sqrt(stdDev / (n - 1.0));
                            fpixels[i] = (float)Math.Sqrt(stdDev / (n - 1.0));
                        else
                            //result[i] = 0f;
                            fpixels[i] = 0f;
                    }
                    else
                        //result[i] = 0f;
                        fpixels[i] = 0f;
                }
            }

        } // end StandardDeviation
    }
}

