using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;

namespace RotatProcess
{
    public class Rotat
    {
        static   Mat newMat = null;
        //将特别大的图像进行缩放
        // Scale a particularly large image
        public static WriteableBitmap ImagePath(string path, int width, int hight)
        {
            if (width > 5000 || hight > 5000)
            {
                double scale = 0.1;
                Mat mMat = new Mat(path);
                OpenCvSharp.Size dsize = new OpenCvSharp.Size(mMat.Cols * scale, mMat.Rows * scale);
                Mat img2 = new Mat();
                Cv2.Resize(mMat, img2, dsize);
                img2.ImWrite(path);
            }
            newMat = new Mat(path);
            return newMat.ToWriteableBitmap();
        }
        public static void toBitmapImage(WriteableBitmap iamge)
        {
            newMat = iamge.ToMat();

        }
        /// <summary>
        /// 旋转图片任意角度
        /// Arbitrary rotation Angle
        /// </summary>
        /// <param name = "src" ></ param >
        /// < param name="dst"></param>
        /// <param name = "angle" ></ param >
        public static WriteableBitmap rotate_arbitrarily_angle(float angle)
        {
            float _angel = 0 - angle;
            Mat  src = newMat;
            Mat dst = new Mat();
            float radian = (float)(_angel / 180.0 * Cv2.PI);

            //填充图像
            //Fill in the image
            int maxBorder = (int)(Math.Max(src.Cols, src.Rows) * 1.414); //即为sqrt(2)*max
            int dx = (maxBorder - src.Cols) / 2;
            int dy = (maxBorder - src.Rows) / 2;
            Cv2.CopyMakeBorder(src, dst, dy, dy, dx, dx, BorderTypes.Constant);

            //旋转
            //rotat
            Point2f center = new Point2f((float)(dst.Cols / 2), (float)(dst.Rows / 2));
            Mat affine_matrix = Cv2.GetRotationMatrix2D(center, _angel, 1.0);//求得旋转矩阵 //Get the rotation matrix
            Cv2.WarpAffine(dst, dst, affine_matrix, dst.Size());

            //计算图像旋转之后包含图像的最大的矩形
            //// Calculate the largest rectangle containing the image after rotation
            float sinVal = (float)Math.Abs(Math.Sin(radian));
            float cosVal = (float)Math.Abs(Math.Cos(radian));
            OpenCvSharp.Size targetSize = new OpenCvSharp.Size((int)(src.Cols * cosVal + src.Rows * sinVal),
                     (int)(src.Cols * sinVal + src.Rows * cosVal));

            //剪掉多余边框
            // Cut off the extra frames
            int x = (dst.Cols - targetSize.Width) / 2;
            int y = (dst.Rows - targetSize.Height) / 2;
            OpenCvSharp.Rect rect = new OpenCvSharp.Rect(x, y, targetSize.Width, targetSize.Height);
            //返回图像
            // Return the image
            return new Mat(dst, rect).ToWriteableBitmap();
        }
        public static BitmapImage MatToBitmapImage(Mat image)
        {
            Bitmap bitmap = MatToBitmap(image);
            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png); // 坑点：格式选Bmp时，不带透明度
                stream.Position = 0;
                BitmapImage result = new BitmapImage();
                result.BeginInit();
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.StreamSource = stream;
                result.EndInit();
                result.Freeze();
                return result;
            }
        }
        private static Bitmap MatToBitmap(Mat image)
        {
            //return OpenCvSharp.Extensions.BitmapConverter.ToBitmap(image);
            return MatToBitmap(image);
        }
        public static void rotate(ref WriteableBitmap writeable, float angle)
        {
            Mat src = writeable.ToMat();
            Mat dst = new Mat();
            Point2f center = new Point2f(src.Cols / 2, src.Rows / 2);
            Mat rot = Cv2.GetRotationMatrix2D(center, angle, 1);
            Size2f s2f = new Size2f(src.Size().Width, src.Size().Height);
            OpenCvSharp.Rect box = new RotatedRect(new Point2f(0, 0), s2f, angle).BoundingRect();
            double xx = rot.At<double>(0, 2) + box.Width / 2 - src.Cols / 2;
            double zz = rot.At<double>(1, 2) + box.Height / 2 - src.Rows / 2;
            rot.Set(0, 2, xx);
            rot.Set(1, 2, zz);
            Cv2.WarpAffine(src, dst, rot, box.Size);
            writeable = dst.ToWriteableBitmap().Clone();
        }
        public static WriteableBitmap rotate(float angle)
        {
            Mat src = newMat;
            Mat dst = new Mat();
            Point2f center = new Point2f(src.Cols / 2, src.Rows / 2);
            Mat rot = Cv2.GetRotationMatrix2D(center, angle, 1);
            Size2f s2f = new Size2f(src.Size().Width, src.Size().Height);
            OpenCvSharp.Rect box = new RotatedRect(new Point2f(0, 0), s2f, angle).BoundingRect();
            double xx = rot.At<double>(0, 2) + box.Width / 2 - src.Cols / 2;
            double zz = rot.At<double>(1, 2) + box.Height / 2 - src.Rows / 2;
            rot.Set(0, 2, xx);
            rot.Set(1, 2, zz);
            Cv2.WarpAffine(src, dst, rot, box.Size);
            return dst.ToWriteableBitmap().Clone();
        }
        public static WriteableBitmap rotateScrap( float angle)
        {
            Mat src = newMat;
            Mat dst = new Mat();
            //图像中心点 //Image center
            OpenCvSharp.Point center = new OpenCvSharp.Point(src.Cols / 2, src.Rows / 2);
            //计算旋转的仿射变换矩阵   //Calculate the affine transformation matrix of the rotation
            Mat rot = Cv2.GetRotationMatrix2D(center, angle, 1);
            Cv2.WarpAffine(src, dst, rot, new OpenCvSharp.Size(src.Cols, src.Rows));
            //Cv2.Circle(src, center, 2, new Scalar(255, 0, 0));
            return dst.ToWriteableBitmap();
        }
        public static void rotateScrap(ref WriteableBitmap writeable, float angle)
        {
            Mat src = writeable.ToMat();
            Mat dst = new Mat();
            OpenCvSharp.Point center = new OpenCvSharp.Point(src.Cols / 2, src.Rows / 2);
            Mat rot = Cv2.GetRotationMatrix2D(center, angle, 1);
            Cv2.WarpAffine(src, dst, rot, new OpenCvSharp.Size(src.Cols, src.Rows));
            //Cv2.Circle(src, center, 2, new Scalar(255, 0, 0));
            writeable = dst.ToWriteableBitmap();
        }
    }
}
