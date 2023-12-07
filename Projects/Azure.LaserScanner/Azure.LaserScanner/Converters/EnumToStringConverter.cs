using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Azure.LaserScanner.Converters
{
    public class EnumToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string enumString;
            try
            {
                enumString =  Enum.GetName((value.GetType()), value);
                return enumString;
            }
            catch
            {
                return string.Empty;
                // Returns empty string , if a exception is thrown then the view doesnt load up 
            }
        }

        //   Convert back is generally not require, we can leave it saying DoNothing of Binding type.
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}

