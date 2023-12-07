using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml.Serialization;

namespace DrawToolsLib
{
    public class XmlColor
    {
        private Color _Color;

        public XmlColor() { }
        public XmlColor(Color color) { _Color = color; }

        public static implicit operator Color(XmlColor xmlColor)
        {
            return xmlColor._Color;
        }

        public static implicit operator XmlColor(Color color)
        {
            return new XmlColor(color);
        }

        [XmlText]
        public string Default
        {
            get { return _Color.ToString(); }
            set { _Color = (Color)ColorConverter.ConvertFromString(value); }
        }
    }
}
