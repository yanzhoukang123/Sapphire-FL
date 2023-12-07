using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace Azure.WPF.Framework
{
    public class GainValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool bVal = false;

            if (value is bool)
                bVal = (bool)value;

            if (bVal == true)
            {
                return "APD Gain:";
            }
            else
            {
                return "Gain:";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
