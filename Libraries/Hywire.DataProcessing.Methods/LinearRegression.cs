using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Hywire.DataProcessing
{
    namespace Methods
    {
        public static class LinearRegression
        {
            public static void Process(Point[] points, out double slope, out double intercept, out double correlation)
            {
                slope = 0;
                intercept = 0;
                correlation = 0;
                if (points.Length < 2)
                {
                    return;
                }

                double aver_x = 0;
                double aver_y = 0;
                double aver_x2 = 0;
                double aver_xy = 0;
                for (int i = 0; i < points.Length; i++)
                {
                    aver_x += points[i].X;
                    aver_y += points[i].Y;
                    aver_x2 += points[i].X * points[i].X;
                    aver_xy += points[i].X * points[i].Y;
                }
                aver_x /= points.Length;
                aver_y /= points.Length;
                aver_x2 /= points.Length;
                aver_xy /= points.Length;

                slope = (aver_xy - aver_x * aver_y) / (aver_x2 - aver_x * aver_x);
                intercept = aver_y - slope * aver_x;
                double tmpNum = 0;
                double tmpDeNum1 = 0;
                double tmpDeNum2 = 0;
                for(int i = 0; i < points.Length; i++)
                {
                    tmpNum += (points[i].X - aver_x) * (points[i].Y - aver_y);
                    tmpDeNum1 += Math.Pow(points[i].X - aver_x, 2.0);
                    tmpDeNum2 += Math.Pow(points[i].Y - aver_y, 2.0);
                }
                correlation = (tmpNum) / (Math.Sqrt(tmpDeNum1) * Math.Sqrt(tmpDeNum2));
                correlation = Math.Pow(correlation, 2.0);    // return R^2
            }
        }
    }
}
