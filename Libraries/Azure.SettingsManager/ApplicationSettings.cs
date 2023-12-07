using System;
using System.Collections.Generic;
using System.Text;
using Azure.Common;
using Azure.ImagingSystem;
using Azure.Utilities;

namespace Azure.Configuration
{
    namespace Settings
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
            //private WindowStateInfo mainWindowStateInfo;
            // Satisfies MarkAllNonSerializableFields.
            //[NonSerializedAttribute]
            //private MruInfo recentFilesList;

            private string initialDirectory;
            private bool _IsAutoSavePubFileEnabled = false;
            private bool _IsAutoSavePubFile300dpi = false;
            private bool _IsAutoSavePubFile600dpi = false;
            private bool _IsAutoSavePubFileJpeg = false;
            private bool _IsHorizontalFlipEnabled = false;
            private bool _IsVerticalFlipEnabled = false;
            //private bool _IsExtDynamicRangeEnabled = false;
            private CMConnectionType _CMConnectionType = CMConnectionType.WIFI;
            private EdmundTargetType _EdmundTargetType = EdmundTargetType.BrandingOnBottom;

            private LaserModule _LaserModuleL1;
            private LaserModule _LaserModuleR1;
            private LaserModule _LaserModuleR2;
            private bool _IsChangeLaserModuleInitiated = false;
            private bool _IsAlwaysShowLaserModuleChanged = false;
            private bool _IsDespeckleSmartScan = false;
            //EL: for testing purposes:
            //private bool _IsPixelOffsetProcessing = true;
            //private bool _Is2LineAverageEnabled = false;
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
                //mainWindowStateInfo = new WindowStateInfo();
                //recentFilesList = new MruInfo();
                initialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                _LaserModuleL1 = new LaserModule();
                _LaserModuleR1 = new LaserModule();
                _LaserModuleR2 = new LaserModule();
            }

            #endregion //Constructors

            #region Properties...

            //public WindowStateInfo MainWindowStateInfo
            //{
            //    get { return mainWindowStateInfo; }
            //    set { mainWindowStateInfo = value; }
            //}

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
            public bool IsAutoSavePubFileEnabled
            {
                get { return _IsAutoSavePubFileEnabled; }
                set { _IsAutoSavePubFileEnabled = value; }
            }
            public bool IsAutoSavePubFile300dpi
            {
                get { return _IsAutoSavePubFile300dpi; }
                set { _IsAutoSavePubFile300dpi = value; }
            }
            public bool IsAutoSavePubFile600dpi
            {
                get { return _IsAutoSavePubFile600dpi; }
                set { _IsAutoSavePubFile600dpi = value; }
            }
            public bool IsAutoSavePubFileJpeg
            {
                get { return _IsAutoSavePubFileJpeg; }
                set { _IsAutoSavePubFileJpeg = value; }
            }
            public bool IsHorizontalFlipEnabled
            {
                get { return _IsHorizontalFlipEnabled; }
                set { _IsHorizontalFlipEnabled = value; }
            }
            public bool IsVerticalFlipEnabled
            {
                get { return _IsVerticalFlipEnabled; }
                set { _IsVerticalFlipEnabled = value; }
            }
            public CMConnectionType CMConnectionType
            {
                get { return _CMConnectionType; }
                set { _CMConnectionType = value; }
            }
            public EdmundTargetType EdmundTargetType
            {
                get { return _EdmundTargetType; }
                set { _EdmundTargetType = value; }
            }
            //public bool IsExtDynamicRangeEnabled
            //{
            //    get { return _IsExtDynamicRangeEnabled; }
            //    set { _IsExtDynamicRangeEnabled = value; }
            //}

            public LaserModule LaserModuleL1
            {
                get { return _LaserModuleL1; }
                set { _LaserModuleL1 = value; }
            }
            public LaserModule LaserModuleR1
            {
                get { return _LaserModuleR1; }
                set { _LaserModuleR1 = value; }
            }
            public LaserModule LaserModuleR2
            {
                get { return _LaserModuleR2; }
                set { _LaserModuleR2 = value; }
            }

            public bool IsChangeLaserModuleInitiated
            {
                get { return _IsChangeLaserModuleInitiated; }
                set { _IsChangeLaserModuleInitiated = value; }
            }

            public bool IsAlwaysShowLaserModuleChanged
            {
                get { return _IsAlwaysShowLaserModuleChanged; }
                set { _IsAlwaysShowLaserModuleChanged = value; }
            }
            public bool IsDespeckleSmartScan
            {
                get { return _IsDespeckleSmartScan; }
                set { _IsDespeckleSmartScan = value; }
            }

            //EL: for testing purposes:
            /// <summary>
            /// Pixel off proccessing (or sawtooth correction)
            /// </summary>
            //public bool IsPixelOffsetProcessing
            //{
            //    get { return _IsPixelOffsetProcessing; }
            //    set { _IsPixelOffsetProcessing = value; }
            //}
            /// <summary>
            /// 2-Line Average (ON/OFF)
            /// </summary>
            //public bool Is2LineAverageEnabled
            //{
            //    get { return _Is2LineAverageEnabled; }
            //    set { _Is2LineAverageEnabled = value; }
            //}

            #endregion //Properties
        }
    }
}
