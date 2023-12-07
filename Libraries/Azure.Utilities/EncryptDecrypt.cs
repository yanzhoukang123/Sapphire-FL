using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography; //MD5CryptoServiceProvider

namespace Azure.Utilities
{
    #region Helper Class : Encrypt/Decrypt
    public class EncryptDecrypt
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
    #endregion
}
