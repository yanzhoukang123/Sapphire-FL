using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;
using System.Globalization;

namespace Azure.WPF.Framework
{
    public class LogicalOrNegateConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            //bool val1 = (bool)values[0];
            //bool val2 = (bool)values[1];
            //return !(val1 || val2);

            foreach (object value in values)
            {
                if ((value is bool) && (bool)value == true)
                {
                    return false;
                }
            }
            return true;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }

}
