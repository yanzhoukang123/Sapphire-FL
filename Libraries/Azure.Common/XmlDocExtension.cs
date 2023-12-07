using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Azure.Common
{
    public static class XmlDocExtension
    {
        public static string ToIndentedString(this XmlDocument doc)
        {
            var stringWriter = new System.IO.StringWriter(new StringBuilder());
            var xmlTextWriter = new XmlTextWriter(stringWriter) { Formatting = Formatting.Indented, IndentChar = ' ' };
            doc.Save(xmlTextWriter);
            return stringWriter.ToString();
        }

        public static string Beautify(this XmlDocument doc)
        {
            StringBuilder sb = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                NewLineChars = "\r\n",
                NewLineHandling = NewLineHandling.Replace
            };
            using (XmlWriter writer = XmlWriter.Create(sb, settings))
            {
                doc.Save(writer);
            }
            return sb.ToString();
        }

    }
}
