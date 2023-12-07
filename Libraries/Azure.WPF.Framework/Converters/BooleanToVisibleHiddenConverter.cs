using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace Azure.WPF.Framework
{
    public class BooleanToVisibileHiddenConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var boolean = false;
            if (value is bool) { boolean = (bool)value; }

            return boolean ? Visibility.Visible : Visibility.Hidden;
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value is Visibility && (Visibility)value == Visibility.Visible;
        }
    }
}


