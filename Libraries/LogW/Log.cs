using log4net;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace LogW
{
    public class Log
    {
        //LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        //public static log4net.ILog log4 = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static log4net.ILog log4;
        private static string GetLog(object obj, string detail)
        {
            string title = obj.GetType().ToString();
            int index = title.LastIndexOf('.');

            if (index > 0)
            {
                title = title.Substring(++index);
            }

            //return "[" + obj.GetType().ToString() + "]" + " " + detail;
            return "[" + title + "]" + " " + detail;
        }
        public static void Debug(object obj, string detail)
        {
            log4.Debug(GetLog(obj, detail));
        }

        public static void Info(object obj, string detail)
        {
            log4.Info(GetLog(obj, detail));
        }

        public static void Warn(object obj, string detail)
        {
            log4.Warn(GetLog(obj, detail));
        }

        public static void Error(object obj, string detail)
        {
            log4.Error(GetLog(obj, detail));
        }

        public static void Fatal(object obj, string detail)
        {
            log4.Fatal(GetLog(obj, detail));
        }
    }
}
