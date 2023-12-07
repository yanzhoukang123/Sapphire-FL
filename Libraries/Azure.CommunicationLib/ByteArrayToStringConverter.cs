
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.CommunicationLib
{
    public static class ByteArrayToStringConverter
    {
        public static string Run(byte[] srcArray, string seperator, bool toHexFormat)
        {
            StringBuilder builder = new StringBuilder();
            int last = srcArray.Length;
            for(int i = 0; i < last; i++)
            {
                if (toHexFormat)
                {
                    builder.Append(string.Format("{0:X2}{1}", srcArray[i], seperator));
                }
                else
                {
                    builder.Append(string.Format("{0:d3}{1}", srcArray[i], seperator));
                }
            }
            return builder.Remove(builder.Length - 1 - seperator.Length, seperator.Length).ToString();
        }
    }
}
