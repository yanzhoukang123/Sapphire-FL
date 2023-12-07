using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DrawToolsLib
{
    namespace ColorExtensions
    {
        public static class RGBExtensions
        {
            public static System.Windows.Media.Color ColorToColor(this System.Drawing.Color color)
            {
                return Color.FromArgb(color.A, color.R, color.G, color.B);
            }

            public static System.Drawing.Color ColorToColor(this System.Windows.Media.Color color)
            {
                return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
            }
        }
    }
}
