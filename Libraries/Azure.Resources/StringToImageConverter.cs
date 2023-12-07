using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Azure.Resources
{
    public class StringToImageConverter : IValueConverter
    {
        /// <summary>
        /// Convert string (content) to image source
        /// </summary>
        /// <returns>BitmapImage</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return new BitmapImage(new Uri((string)value));
            }
            catch
            {
                return new BitmapImage();
            }
        }

        /// <summary>
        /// NotImplementedException
        /// </summary>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
