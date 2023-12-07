using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;

namespace Azure.LaserScanner.CollapsiblePanel
{
    public class MultiplyConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double result = 1.0;
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] is double)
                    result *= (double)values[i];
            }

            return result;
        }
       
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new Exception("Not implemented");
        }
    }  

}
