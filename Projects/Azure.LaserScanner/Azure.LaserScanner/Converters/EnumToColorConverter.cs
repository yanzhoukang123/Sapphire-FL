using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using Azure.Common;

namespace Azure.LaserScanner.Converters
{
    public class EnumToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var scbColor = new SolidColorBrush(Colors.Gray);
            if (value != null && value.GetType().IsEnum)
            {
                ChannelType imageChannel = (ChannelType)value;
                if (imageChannel != ChannelType.GRAY && imageChannel != ChannelType.MONO)
                {
                    var clr = (Color)ColorConverter.ConvertFromString(imageChannel.ToString());
                    scbColor = new SolidColorBrush(clr);
                }
            }
            return scbColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
