using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Security.Cryptography;
using System.Xml;
//using Azure.WindowsAPI.Interop;
using Azure.WPF.Framework;
using Azure.Utilities;    //WindowsInvoke

namespace Azure.ScannerEUI
{
    /// <summary>
    /// Interaction logic for SaveAllPrompt.xaml
    /// </summary>
    public partial class PasswordPrompt : Window
    {
        #region Private data...
        private string _SecureXmlPath = string.Empty;
        #endregion

        #region Public properties...
        #endregion

        #region Constructors...
        public PasswordPrompt(string strSecureXmlPath)
        {
            InitializeComponent();

            this._SecureXmlPath = strSecureXmlPath;

            //this.Closed += new EventHandler(Window_Closed);
            this._PasswordBox.PasswordChanged += new RoutedEventHandler(_PasswordBox_PasswordChanged);
        }
        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _PasswordBox.Focus();
            //_KeyboardBtn_Click(sender, e);
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void _OKBtn_Click(object sender, RoutedEventArgs e)
        {
            _StatusTextBlock.Text = string.Empty;
            string strPasswordHash = SecureSettings.GetPassword(_SecureXmlPath);
            string strEnterPasswordHash = EncryptDecrypt.EncryptString(_PasswordBox.Password);

            if (!string.IsNullOrEmpty(strPasswordHash) && strPasswordHash == strEnterPasswordHash)
            {
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                _StatusTextBlock.Text = "Invalid password";
            }
        }

        private void _Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void _PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            _StatusTextBlock.Text = string.Empty;
        }

    }

    /*#region Helper Class : Encrypt/Decrypt
    class EncryptDecrypt
    {
        private static byte[] entropy = { 65, 34, 87, 33 };
        private static string strPassphrase = entropy.ToString();

        #region public static string EncryptString(string strToBeEncrypt)
        public static string EncryptString(string strToBeEncrypt)
        {
            if (string.IsNullOrEmpty(strToBeEncrypt) || string.IsNullOrWhiteSpace(strToBeEncrypt))
            {
                return string.Empty;
            }

            byte[] Results;
            //byte[] entropy = { 65, 34, 87, 33 };
            //string strPassphrase = entropy.ToString();
            System.Text.UTF8Encoding UTF8 = new System.Text.UTF8Encoding();

            // Step 1. Hash the passphrase using MD5
            // We use the MD5 hash generator as the result is a 128 bit byte array
            // which is a valid length for the TripleDES encoder we use below

            MD5CryptoServiceProvider HashProvider = new MD5CryptoServiceProvider();
            byte[] TDESKey = HashProvider.ComputeHash(UTF8.GetBytes(strPassphrase));

            // Step 2. Create a new TripleDESCryptoServiceProvider object
            TripleDESCryptoServiceProvider TDESAlgorithm = new TripleDESCryptoServiceProvider();

            // Step 3. Setup the encoder
            TDESAlgorithm.Key = TDESKey;
            TDESAlgorithm.Mode = CipherMode.ECB;
            TDESAlgorithm.Padding = PaddingMode.PKCS7;

            // Step 4. Convert the input string to a byte[]
            byte[] DataToEncrypt = UTF8.GetBytes(strToBeEncrypt);

            // Step 5. Attempt to encrypt the string
            try
            {
                ICryptoTransform Encryptor = TDESAlgorithm.CreateEncryptor();
                Results = Encryptor.TransformFinalBlock(DataToEncrypt, 0, DataToEncrypt.Length);
            }
            finally
            {
                // Clear the TripleDes and Hashprovider services of any sensitive information
                TDESAlgorithm.Clear();
                HashProvider.Clear();
            }

            // Step 6. Return the encrypted string as a base64 encoded string
            return Convert.ToBase64String(Results);
        }
        #endregion

        #region public static string DecryptString(string strToBeDecrypt, bool bBlankAllowed = true)
        public static string DecryptString(string strToBeDecrypt, bool bBlankAllowed = true)
        {
            byte[] Results;
            //byte[] entropy = { 65, 34, 87, 33 };
            //string strPassphrase = entropy.ToString();
            System.Text.UTF8Encoding UTF8 = new System.Text.UTF8Encoding();

            if (bBlankAllowed)
            {
                // Blank/empty password allowed
                if (string.IsNullOrWhiteSpace(strToBeDecrypt))
                {
                    return string.Empty;
                }
            }

            // Step 1. We hash the passphrase using MD5
            // We use the MD5 hash generator as the result is a 128 bit byte array
            // which is a valid length for the TripleDES encoder we use below

            MD5CryptoServiceProvider HashProvider = new MD5CryptoServiceProvider();
            byte[] TDESKey = HashProvider.ComputeHash(UTF8.GetBytes(strPassphrase));

            // Step 2. Create a new TripleDESCryptoServiceProvider object
            TripleDESCryptoServiceProvider TDESAlgorithm = new TripleDESCryptoServiceProvider();

            // Step 3. Setup the decoder
            TDESAlgorithm.Key = TDESKey;
            TDESAlgorithm.Mode = CipherMode.ECB;
            TDESAlgorithm.Padding = PaddingMode.PKCS7;

            //Fix for base64 error when the string strToBeDecrypt has a space, This is the line of code to add
            strToBeDecrypt = strToBeDecrypt.Replace(" ", "+");

            // Step 4. Convert the input string to a byte[]
            byte[] DataToDecrypt = Convert.FromBase64String(strToBeDecrypt);

            // Step 5. Attempt to decrypt the string
            try
            {
                ICryptoTransform Decryptor = TDESAlgorithm.CreateDecryptor();
                Results = Decryptor.TransformFinalBlock(DataToDecrypt, 0, DataToDecrypt.Length);
            }
            finally
            {
                // Clear the TripleDes and Hashprovider services of any sensitive information
                TDESAlgorithm.Clear();
                HashProvider.Clear();
            }

            // Step 6. Return the decrypted string in UTF8 format
            return UTF8.GetString(Results);
        }
        #endregion

    }
    #endregion*/

    #region Helper Class : Secure settings
    class SecureSettings
    {
        public static void CreateSecureXml(string strXmlFilePath)
        {
            const string element = "PasswordHash";
            const string elementHash = "gXfn9vzTcXc=";

            XmlTextWriter writer = new XmlTextWriter(strXmlFilePath, Encoding.UTF8);
            writer.Formatting = Formatting.Indented;

            writer.WriteStartDocument();
            writer.WriteStartElement("Config");

            writer.WriteElementString(element, elementHash);

            writer.WriteEndElement();
            writer.WriteEndDocument();

            writer.Close();
            writer = null;
        }

        public static string GetPassword(string strXmlFilePath)
        {
            string strResult = string.Empty;
            const string element = "AuthenHash";

            XmlDocument doc = new XmlDocument();
            doc.Load(strXmlFilePath);
            XmlNode node = doc.SelectSingleNode("/Config/" + element);
            if (node != null)
            {
                strResult = node.InnerText;
            }

            return strResult;
        }

        public static void UpdateSecureXml(string strXmlFilePath, string value)
        {
            const string element = "PasswordHash";

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(strXmlFilePath);
                XmlNode node = doc.SelectSingleNode("/Config/" + element);
                if (node != null)
                {
                    //node.InnerText = EncryptDecrypt.EncryptString(value);
                    node.InnerText = value;
                }
                else
                {
                    XmlNode root = doc.DocumentElement;
                    XmlElement elem;
                    elem = doc.CreateElement(element);
                    elem.InnerText = value;
                    root.AppendChild(elem);
                }
                doc.Save(strXmlFilePath);
                doc = null;
            }
            catch (Exception)
            {
                //
                // Possible Exceptions:
                //   System.ArgumentException
                //   System.ArgumentNullException
                //   System.InvalidOperationException
                //   System.IO.DirectoryNotFoundException
                //   System.IO.FileNotFoundException
                //   System.IO.IOException
                //   System.IO.PathTooLongException
                //   System.NotSupportedException
                //   System.Security.SecurityException
                //   System.UnauthorizedAccessException
                //   System.UriFormatException
                //   System.Xml.XmlException
                //   System.Xml.XPath.XPathException
                //
            }
        }
    }
    #endregion

}
