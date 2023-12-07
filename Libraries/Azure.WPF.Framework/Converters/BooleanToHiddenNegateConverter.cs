using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace Azure.WPF.Framework
{
    public class BooleanToHiddenNegateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var boolVal = false;
            if (value is bool)
            {
                boolVal = (bool)value;
            }

            return boolVal ? Visibility.Hidden : Visibility.Visible;
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value is Visibility && (Visibility)value == Visibility.Visible;
        }
    }
}


