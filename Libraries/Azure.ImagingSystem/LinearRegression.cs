using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.ImagingSystem
{
    public static class LinearRegression
    {
        public static double Slope(double[] x, double[] y)
        {
            double slope = 0.0;
            if ((x != null) && (y != null) && (x.Length == y.Length) && (x.Length > 0))
            {
                slope = Correlation(x, y) / SumOfSquares(x);
            }
            return slope;
        }

        public static double Intercept(double[] x, double[] y)
        {
            double intercept = 0.0;
            if ((x != null) && (y != null) && (x.Length == y.Length) && (x.Length > 0))
            {
                double xave = Average(x);
                double yave = Average(y);
                intercept = yave - Slope(x, y) * xave;
            }
            return intercept;
        }

        public static double Average(double[] values)
        {
            double sum = 0, average = 0.0;
            if ((values != null) && (values.Length > 0))
            {
                //average = Arrays.stream(values).average().orElse(0.0);
                //average = Queryable.Average(values.AsQueryable());
                for (int i = 0; i < values.Length; i++)
                {
                    sum += values[i];
                }
                average = sum / values.Length;
            }
            return average;
        }

        public static double SumOfSquares(double[] values)
        {
            double sumOfSquares = 0.0;
            if ((values != null) && (values.Length > 0))
            {
                //sumOfSquares = Arrays.stream(values).map(v->v * v).sum();
                sumOfSquares = values.Sum(n => n * n);
                double average = Average(values);
                sumOfSquares -= average * average * values.Length;
            }
            return sumOfSquares;
        }

        public static double Correlation(double[] x, double[] y)
        {
            double correlation = 0.0;
            if ((x != null) && (y != null) && (x.Length == y.Length) && (x.Length > 0))
            {
                for (int i = 0; i < x.Length; ++i)
                {
                    correlation += x[i] * y[i];
                }
                double xave = Average(x);
                double yave = Average(y);
                correlation -= xave * yave * x.Length;
            }
            return correlation;
        }

        /// <summary>
        /// Calculates the tilt using linear regression, which replicates the function SLOPE in Excel
        /// </summary>
        /// <param name="xArray"></param>
        /// <param name="yArray"></param>
        /// <returns></returns>
        public static double GetSlope(double[] xArray, double[] yArray)
        {
            double n = xArray.Length;
            double sumxy = 0, sumx = 0, sumy = 0, sumx2 = 0;
            for (int i = 0; i < xArray.Length; i++)
            {
                sumxy += xArray[i] * yArray[i];
                sumx += xArray[i];
                sumy += yArray[i];
                sumx2 += xArray[i] * xArray[i];
            }
            return ((sumxy - sumx * sumy / n) / (sumx2 - sumx * sumx / n));
        }
    }
}
