using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Common;
using Azure.WPF.Framework;
using Azure.Configuration.Settings;

namespace Azure.LaserScanner
{
    public class GeneralAppSettingsViewModel : ViewModelBase
    {
        #region Private data/fields...

        private bool _IsAutoSavePubFileEnabled = false;
        private bool _IsAutoSavePubFile300dpi = false;
        private bool _IsAutoSavePubFile600dpi = false;
        private bool _IsAutoSavePubFileJpeg = false;
        private bool _IsHorizontalFlipEnabled = false;
        private bool _IsVerticalFlipEnabled = false;
        //private bool _IsExtDynamicRangeEnabled = false;
        private bool _IsAlwaysShowLaserModuleChanged = false;
        private bool _IsDespeckleSmartScan = false;
        private CMConnectionType _CMConnectionType = CMConnectionType.WIFI; 
        private EdmundTargetType _EdmundTargetType = EdmundTargetType.BrandingOnTop;
        //EL: for testing purposes:
        //private bool _IsPixelOffsetProcessing = false;
        //private bool _Is2LineAverageEnabled = false;

        #endregion

        #region Constructors...

        public GeneralAppSettingsViewModel()
        {
            _IsAutoSavePubFileEnabled = SettingsManager.ApplicationSettings.IsAutoSavePubFileEnabled;
            _IsAutoSavePubFile300dpi = SettingsManager.ApplicationSettings.IsAutoSavePubFile300dpi;
            _IsAutoSavePubFile600dpi = SettingsManager.ApplicationSettings.IsAutoSavePubFile600dpi;
            _IsAutoSavePubFileJpeg = SettingsManager.ApplicationSettings.IsAutoSavePubFileJpeg;
            _IsHorizontalFlipEnabled = SettingsManager.ApplicationSettings.IsHorizontalFlipEnabled;
            _IsVerticalFlipEnabled = SettingsManager.ApplicationSettings.IsVerticalFlipEnabled;
            _CMConnectionType = SettingsManager.ApplicationSettings.CMConnectionType;
            _EdmundTargetType = SettingsManager.ApplicationSettings.EdmundTargetType;
            //_IsExtDynamicRangeEnabled = SettingsManager.ApplicationSettings.IsExtDynamicRangeEnabled;
            _IsAlwaysShowLaserModuleChanged = SettingsManager.ApplicationSettings.IsAlwaysShowLaserModuleChanged;
            _IsDespeckleSmartScan = SettingsManager.ApplicationSettings.IsDespeckleSmartScan;
            //EL: testing...overriding the flag in config.xml by the flag in the Settings tab
            //_IsPixelOffsetProcessing = SettingsManager.ApplicationSettings.IsPixelOffsetProcessing;
            //_Is2LineAverageEnabled = SettingsManager.ApplicationSettings.Is2LineAverageEnabled;
            //SettingsManager.ConfigSettings.PixelOffsetProcessing = _IsPixelOffsetProcessing;
            //SettingsManager.ConfigSettings.PhosphorModuleProcessing = _Is2LineAverageEnabled;
            //SettingsManager.ConfigSettings.AllModuleProcessing = _Is2LineAverageEnabled;

            if (_IsAutoSavePubFileEnabled)
            {
                if (!_IsAutoSavePubFile300dpi && !_IsAutoSavePubFile600dpi && !_IsAutoSavePubFileJpeg)
                {
                    _IsAutoSavePubFile300dpi = true;
                }
            }
            else
            {
                _IsAutoSavePubFile300dpi = false;
                _IsAutoSavePubFile600dpi = false;
                _IsAutoSavePubFileJpeg = false;
            }
            if (_IsHorizontalFlipEnabled)
            {
                _IsVerticalFlipEnabled = false;
            }
            else if (_IsVerticalFlipEnabled)
            {
                _IsHorizontalFlipEnabled = false;
            }
        }

        #endregion

        #region Public properties...

        public bool IsAutoSavePubFileEnabled
        {
            get { return _IsAutoSavePubFileEnabled; }
            set
            {
                _IsAutoSavePubFileEnabled = value;
                RaisePropertyChanged("IsAutoSavePubFileEnabled");
                SettingsManager.ApplicationSettings.IsAutoSavePubFileEnabled = _IsAutoSavePubFileEnabled;
                if (_IsAutoSavePubFileEnabled)
                {
                    if (!_IsAutoSavePubFile300dpi && !_IsAutoSavePubFile600dpi && !_IsAutoSavePubFileJpeg)
                    {
                        IsAutoSavePubFile300dpi = true;
                    }
                }
                else
                {
                    IsAutoSavePubFile300dpi = false;
                    IsAutoSavePubFile600dpi = false;
                    IsAutoSavePubFileJpeg = false;
                }
            }
        }
        public bool IsAutoSavePubFile300dpi
        {
            get { return _IsAutoSavePubFile300dpi; }
            set
            {
                _IsAutoSavePubFile300dpi = value;
                RaisePropertyChanged("IsAutoSavePubFile300dpi");
                SettingsManager.ApplicationSettings.IsAutoSavePubFile300dpi = _IsAutoSavePubFile300dpi;
                if (!_IsAutoSavePubFile300dpi && !_IsAutoSavePubFile600dpi && !_IsAutoSavePubFileJpeg)
                {
                    SettingsManager.ApplicationSettings.IsAutoSavePubFileEnabled = false;
                    _IsAutoSavePubFileEnabled = false;
                    RaisePropertyChanged("IsAutoSavePubFileEnabled");
                }
            }
        }
        public bool IsAutoSavePubFile600dpi
        {
            get { return _IsAutoSavePubFile600dpi; }
            set
            {
                _IsAutoSavePubFile600dpi = value;
                RaisePropertyChanged("IsAutoSavePubFile600dpi");
                SettingsManager.ApplicationSettings.IsAutoSavePubFile600dpi = _IsAutoSavePubFile600dpi;
                if (!_IsAutoSavePubFile300dpi && !_IsAutoSavePubFile600dpi && !_IsAutoSavePubFileJpeg)
                {
                    SettingsManager.ApplicationSettings.IsAutoSavePubFileEnabled = false;
                    _IsAutoSavePubFileEnabled = false;
                    RaisePropertyChanged("IsAutoSavePubFileEnabled");
                }
            }
        }
        public bool IsAutoSavePubFileJpeg
        {
            get { return _IsAutoSavePubFileJpeg; }
            set
            {
                _IsAutoSavePubFileJpeg = value;
                RaisePropertyChanged("IsAutoSavePubFileJpeg");
                SettingsManager.ApplicationSettings.IsAutoSavePubFileJpeg = _IsAutoSavePubFileJpeg;
                if (!_IsAutoSavePubFile300dpi && !_IsAutoSavePubFile600dpi && !_IsAutoSavePubFileJpeg)
                {
                    SettingsManager.ApplicationSettings.IsAutoSavePubFileEnabled = false;
                    _IsAutoSavePubFileEnabled = false;
                    RaisePropertyChanged("IsAutoSavePubFileEnabled");
                }
            }
        }
        public bool IsHorizontalFlipEnabled
        {
            get { return _IsHorizontalFlipEnabled; }
            set
            {
                _IsHorizontalFlipEnabled = value;
                RaisePropertyChanged("IsHorizontalFlipEnabled");
                SettingsManager.ApplicationSettings.IsHorizontalFlipEnabled = _IsHorizontalFlipEnabled;
                if (_IsHorizontalFlipEnabled)
                {
                    IsVerticalFlipEnabled = false;
                }
            }
        }
        public bool IsVerticalFlipEnabled
        {
            get { return _IsVerticalFlipEnabled; }
            set
            {
                _IsVerticalFlipEnabled = value;
                RaisePropertyChanged("IsVerticalFlipEnabled");
                SettingsManager.ApplicationSettings.IsVerticalFlipEnabled = _IsVerticalFlipEnabled;
                if (_IsVerticalFlipEnabled)
                {
                    IsHorizontalFlipEnabled = false;
                }
            }
        }

        //public bool IsExtDynamicRangeEnabled
        //{
        //    get { return _IsExtDynamicRangeEnabled; }
        //    set
        //    {
        //        _IsExtDynamicRangeEnabled = value;
        //        RaisePropertyChanged("IsExtDynamicRangeEnabled");
        //        SettingsManager.ApplicationSettings.IsExtDynamicRangeEnabled = _IsExtDynamicRangeEnabled;
        //    }
        //}

        public bool IsAlwaysShowLaserModuleChanged
        {
            get { return _IsAlwaysShowLaserModuleChanged; }
            set
            {
                _IsAlwaysShowLaserModuleChanged = value;
                RaisePropertyChanged("IsAlwaysShowLaserModuleChanged");
                SettingsManager.ApplicationSettings.IsAlwaysShowLaserModuleChanged = _IsAlwaysShowLaserModuleChanged;
            }
        }

        public bool IsDespeckleSmartScan
        {
            get { return _IsDespeckleSmartScan; }
            set
            {
                _IsDespeckleSmartScan = value;
                RaisePropertyChanged("IsDespeckleSmartScan");
                SettingsManager.ApplicationSettings.IsDespeckleSmartScan = _IsDespeckleSmartScan;
            }
        }

        public CMConnectionType CMConnectionType
        {
            get { return _CMConnectionType; }
            set
            {
                _CMConnectionType = value;
                RaisePropertyChanged("CMConnectionType");
                SettingsManager.ApplicationSettings.CMConnectionType = _CMConnectionType;
            }
        }

        public EdmundTargetType EdmundTargetType
        {
            get { return _EdmundTargetType; }
            set
            {
                _EdmundTargetType = value;
                RaisePropertyChanged("EdmundTargetType");
                SettingsManager.ApplicationSettings.EdmundTargetType = _EdmundTargetType;
            }
        }
        //EL: for testing purposes:
        //public bool IsPixelOffsetProcessing
        //{
        //    get { return _IsPixelOffsetProcessing; }
        //    set
        //    {
        //        _IsPixelOffsetProcessing = value;
        //        RaisePropertyChanged("PixelOffsetProcessing");
        //        SettingsManager.ApplicationSettings.IsPixelOffsetProcessing = _IsPixelOffsetProcessing;
        //        //EL: testing...overriding the flag in config.xml by the flag in the Settings tab
        //        SettingsManager.ConfigSettings.PixelOffsetProcessing = _IsPixelOffsetProcessing;
        //    }
        //}
        //public bool Is2LineAverageEnabled
        //{
        //    get { return _Is2LineAverageEnabled; }
        //    set
        //    {
        //        _Is2LineAverageEnabled = value;
        //        RaisePropertyChanged("Is2LineAverageEnabled");
        //        SettingsManager.ApplicationSettings.Is2LineAverageEnabled = _Is2LineAverageEnabled;
        //        //EL: testing...overriding the flag in config.xml by the flag in the Settings tab
        //        SettingsManager.ConfigSettings.PhosphorModuleProcessing = _Is2LineAverageEnabled;
        //        SettingsManager.ConfigSettings.AllModuleProcessing = _Is2LineAverageEnabled;
        //    }
        //}

        #endregion
    }
}
