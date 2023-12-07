using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace Azure.WPF.Framework
{
    public class RealGammaConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double val = 0;
            if (value != null)
                val = System.Convert.ToDouble(value);
            val = Math.Round(Math.Pow(10, val), 3);

            return val;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //throw new NotImplementedException();
            //double result = 0;
            //if (value != null)
            //{
            //    var strVal = value as string;
            //    var val = System.Convert.ToDouble(strVal);
            //    result = Math.Round(Math.Log10(val), 3);
            //}
            //return result;
            return Binding.DoNothing;
        }
    }
}
