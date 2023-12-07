using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.AccessControl;
using Microsoft.Win32;
using System.Reflection;

namespace Azure.Utilities
{
    public class RegistryModify
    {
        #region Private fields...

        private string _CompanyName = string.Empty;
        private string _ProductName = string.Empty;
        private string _SubKey = string.Empty;
        //private RegistryKey _BaseRegistryKey = Registry.LocalMachine;
        private RegistryKey _BaseRegistryKey = Registry.CurrentUser;

        #endregion

        #region Constructor...

        public RegistryModify()
        {
            try
            {
                string companyName = string.Empty;
                string productName = string.Empty;
                object[] customAttributes = null;

                //System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                System.Reflection.Assembly assembly = System.Windows.Application.ResourceAssembly;

                if (assembly != null)
                {
                    customAttributes = assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                    if ((customAttributes != null) && (customAttributes.Length > 0))
                    {
                        companyName = ((AssemblyCompanyAttribute)customAttributes[0]).Company;
                    }

                    if (string.IsNullOrEmpty(companyName))
                    {
                        companyName = "Azure Biosystems";
                    }

                    customAttributes = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                    if ((customAttributes != null) && (customAttributes.Length > 0))
                    {
                        productName = ((AssemblyProductAttribute)customAttributes[0]).Product;
                    }

                    if (string.IsNullOrEmpty(productName))
                    {
                        productName = "Sapphire";
                    }

                    _CompanyName = companyName;
                    _ProductName = productName;
                    //_SubKey = "SOFTWARE\\" + companyName + "\\" + productName;
                    _SubKey = "SOFTWARE\\" + companyName + "\\" + productName;
                }
            }
            catch
            {
            }
        }
        #endregion

        #region Public properties...

        /// <summary>
        /// A property to set the BaseRegistryKey value.
        /// (default = Registry.LocalMachine)
        /// </summary>
        public RegistryKey BaseRegistryKey
        {
            get { return _BaseRegistryKey; }
            set { _BaseRegistryKey = value; }
        }

        /// <summary>
        /// A property to set the SubKey value
        /// (default = "SOFTWARE\\" + CompanyName + ProductName)
        /// </summary>
        public string SubKey
        {
            get { return _SubKey; }
            set { _SubKey = value; }
        }

        public string CompanyName
        {
            get { return _CompanyName; }
            set { _CompanyName = value; }
        }

        public string ProductName
        {
            get { return _ProductName; }
            set { _ProductName = value; }
        }

        #endregion

        /// <summary>
        /// To read a registry key.
        /// Input: keyName (string)
        /// Output: value (string) 
        /// </summary>
        public string Read(string keyName)
        {
            // Opening the registry key
            RegistryKey baseRegKey = _BaseRegistryKey;
            // Open a subKey as read-only
            RegistryKey subKey = baseRegKey.OpenSubKey(_SubKey);
            // If the RegistrySubKey doesn't exist -> (null)
            if (subKey == null)
            {
                return null;
            }
            else
            {
                try
                {
                    // If the RegistryKey exists I get its value
                    // or null is returned.
                    return subKey.GetValue(keyName).ToString();
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// To write into a registry key.
        /// Input: keyName (string) , value (object)
        /// Output: true or false 
        /// </summary>
        public bool Write(string keyName, object value)
        {
            try
            {
                // Setting
                RegistryKey baseRegKey = _BaseRegistryKey;
                // I have to use CreateSubKey
                // (create or open it if already exits), 
                //  'cause OpenSubKey open a subKey as read-only
                RegistryKey subKey = baseRegKey.CreateSubKey(_SubKey);
                // Save the value
                subKey.SetValue(keyName, value);

                return true;
            }
            catch
            {
                return false;
            }
        }


        /// <summary>
        /// To delete a registry key.
        /// input: KeyName (string)
        /// output: true or false 
        /// </summary>
        public bool DeleteKey(string keyName)
        {
            try
            {
                // Setting
                RegistryKey baseRegKey = _BaseRegistryKey;
                RegistryKey subKey = baseRegKey.CreateSubKey(_SubKey);
                // If the RegistrySubKey doesn't exists -> (true)
                if (subKey == null)
                    return true;
                else
                    subKey.DeleteValue(keyName);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// To delete a sub key and any child.
        /// Input: void
        /// Output: true or false 
        /// </summary>
        public bool DeleteSubKeyTree()
        {
            try
            {
                // Setting
                RegistryKey baseRegKey = _BaseRegistryKey;
                RegistryKey subKey = baseRegKey.OpenSubKey(_SubKey);
                // If the RegistryKey exists, I delete it
                if (subKey != null)
                    baseRegKey.DeleteSubKeyTree(_SubKey);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
