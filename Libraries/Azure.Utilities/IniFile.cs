using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Azure.Utilities
{
    /// <summary>
    /// Create a New INI file to store or load data
    /// </summary>
    public class IniFile
    {
        private string _INIFilePath;

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        /// <summary>
        /// INIFile Constructor.
        /// </summary>
        /// <param name="INIPath"></param>
        public IniFile(string INIPath)
        {
            _INIFilePath = INIPath;
        }
        /// <summary>
        /// Write Data to the INI File
        /// </summary>
        /// <param name="Section">Section name</param>
        /// <param name="Key">Key name</param>
        /// <param name="Value">Value name</param>
        public void IniWriteValue(string Section, string Key, string Value)
        {
            WritePrivateProfileString(Section, Key, Value, _INIFilePath);
        }

        /// <summary>
        /// Read Data Value From the Ini File
        /// </summary>
        /// <param name="Section"></param>
        /// <param name="Key"></param>
        /// <param name="Path"></param>
        /// <returns></returns>
        public string IniReadValue(string Section, string Key)
        {
            StringBuilder temp = new StringBuilder(255);
            int i = GetPrivateProfileString(Section, Key, "", temp, 255, _INIFilePath);
            return temp.ToString();

        }

        public string INIFilePath
        {
            get { return _INIFilePath; }
        }
    }
}
