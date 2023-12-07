using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Utilities;

namespace Azure.LaserScanner
{
    /// <summary>
    /// Application settings class.
    /// Loaded when application starts, saved when application ends.
    /// 
    /// Contains all persistent application settings.
    /// 
    /// All class members should meet XML serialization requirements.
    /// 
    /// Can be used directly, but recommended way is to use this class
    /// together with SettingsManager class.
    /// See also notes in SettingsManager.
    /// 
    /// Since class is serialized, it is public.
    /// </summary>
    [Serializable()]
    public class ApplicationSettings
    {
        #region Class Members...

        // Satisfies MarkAllNonSerializableFields.
        [NonSerializedAttribute]
        private WindowStateInfo mainWindowStateInfo;
        // Satisfies MarkAllNonSerializableFields.
        //[NonSerializedAttribute]
        //private MruInfo recentFilesList;

        private string initialDirectory;

        // Other members go here.
        // For every member create public property.
        // ...
        
        #endregion //Class Members

        #region Constructors...

        /// <summary>
        /// Default constructor.
        /// Creates instances of all internal classes and sets all default values.
        /// 
        /// This prevents exception when client cannot load Settings instance from
        /// XML file - in this case default Settings instance is created.
        /// Default Settings instance should always contain valid default values.
        /// </summary>
        public ApplicationSettings()
        {
            mainWindowStateInfo = new WindowStateInfo();
            //recentFilesList = new MruInfo();
            initialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        #endregion //Constructors

        #region Properties...

        public WindowStateInfo MainWindowStateInfo
        {
            get { return mainWindowStateInfo; }
            set { mainWindowStateInfo = value; }
        }

        //public MruInfo RecentFilesList
        //{
        //    get { return recentFilesList; }
        //    set { recentFilesList = value; }
        //}

        public string InitialDirectory
        {
            get { return initialDirectory; }
            set { initialDirectory = value; }
        }

        #endregion //Properties
    }
}
