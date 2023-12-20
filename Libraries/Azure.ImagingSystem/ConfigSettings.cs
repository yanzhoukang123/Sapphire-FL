using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;   //ObservableCollection
using Azure.ImagingSystem;  //LightingType

namespace Azure.Configuration
{
    namespace Settings
    {
        public class ConfigSettings
        {
            #region Private data...

            private List<ResolutionType> _ResolutionOptions = new List<ResolutionType>();
            private List<QualityType> _QualityOptions = new List<QualityType>();
            private List<ScanSpeedType> _ScanSpeedOptions = new List<ScanSpeedType>();
            // New scan option: NOT the same as scan speed (Marketing name for (2-line average or unidirectional scan)
            private List<ScanQualityType> _ScanQualityOptions = new List<ScanQualityType>();
            private ObservableCollection<APDGainType> _APDGains = new ObservableCollection<APDGainType>();
            private ObservableCollection<FocusType> _Focus = new ObservableCollection<FocusType>();
            private ObservableCollection<APDPgaType> _APDPgas = new ObservableCollection<APDPgaType>();
            private ObservableCollection<PhosphorLaserModules> _PhosphorLaserModules = new ObservableCollection<PhosphorLaserModules>();
            private ObservableCollection<FilterType> _FilterOptions = new ObservableCollection<FilterType>();
            private int _XMaxValue = 0;
            private int _YMaxValue = 0;
            private int _ZMaxValue = 0;
            private double _XMotorSubdivision = 0;
            private double _YMotorSubdivision = 0;
            private double _ZMotorSubdivision = 0;
            private ObservableCollection<MotorSettingsType> _MotorSettings = new ObservableCollection<MotorSettingsType>();
            private int[] _LasersIntensities = new int[4];
            private List<LaserSettingsType> _LaserIntensitySettings = new List<LaserSettingsType>();
            private List<SampleTypeSetting> _SampleTypeSettings = new List<SampleTypeSetting>();

            // Phosphor signal options
            private List<Signal> _PhosphorSignalOptions = new List<Signal>();

            private Dictionary<int, List<Signal>> _LasersSignalList = new Dictionary<int, List<Signal>>();

            private List<LaserTypes> _LaserOptions = new List<LaserTypes>();
            private List<Protocol> _Protocols = new List<Protocol>();
            private List<Protocol> _PhosphorProtocols = new List<Protocol>();

            private List<ImagingSettings> _ImagingSettings = new List<ImagingSettings>();

            private AutoScan _AutoScanSettings = new AutoScan();

            #endregion

            #region Constructors...

            public ConfigSettings()
            {
            }

            #endregion

            #region Public properties...

            public int XMaxValue
            {
                get
                {
                    return _XMaxValue;
                }
                set
                {
                    _XMaxValue = value;
                }
            }
            public int YMaxValue
            {
                get
                {
                    return _YMaxValue;
                }
                set
                {
                    _YMaxValue = value;
                }
            }
            public int ZMaxValue
            {
                get
                {
                    return _ZMaxValue;
                }
                set
                {
                    _ZMaxValue = value;
                }
            }
            public int WMaxValue { get; set; }
            public int WMinValue { get; set; }
            public int WMediumValue { get; set; }
            public double XMotorSubdivision
            {
                get
                {
                    return _XMotorSubdivision;
                }

                set
                {
                    _XMotorSubdivision = value;
                }

            }
            public double YMotorSubdivision
            {
                get
                {
                    return _YMotorSubdivision;
                }

                set
                {
                    _YMotorSubdivision = value;
                }

            }
            public double ZMotorSubdivision
            {
                get
                {
                    return _ZMotorSubdivision;
                }

                set
                {
                    _ZMotorSubdivision = value;
                }

            }
            public double WMotorSubdivision { get; set; }
            public double XEncoderSubdivision { get; set; }
            // Non-EUI: Lasers default intensity
            public int LaserAIntensity { get; set; }
            public int LaserBIntensity { get; set; }
            public int LaserCIntensity { get; set; }
            public int LaserDIntensity { get; set; }

            // Non-EUI: Lasers maximum intensity
            public int LaserAMaxIntensity { get; set; }
            public int LaserBMaxIntensity { get; set; }
            public int LaserCMaxIntensity { get; set; }
            public int LaserDMaxIntensity { get; set; }

            public bool IsSimulationMode { get; set; }
            public bool IsApplyImageSmoothing { get; set; }

            public bool IsChannelALightShadeFix { get; set; }
            public bool IsChannelBLightShadeFix { get; set; }
            public bool IsChannelCLightShadeFix { get; set; }
            public bool IsChannelDLightShadeFix { get; set; }

            // Replaced with 'IsFluorescence2LinesAvgScan' and 'IsPhosphor2LinesAvgScan'
            // to be able to apply the correction only for Fluorescence or Phosphor scan or both.
            //public bool IsUnidirectionalScan { get; set; }

            //public bool IsFluorescence2LinesAvgScan { get; set; }
            //public bool IsPhosphor2LinesAvgScan { get; set; }
            public int EdrScaleFactor { get; set; }
            public bool IsSaveDebuggingImages { get; set; }
            public bool IsSendAutoAlignedImageToGallery { get; set; }
            public bool IsSaveAutoAlignImages { get; set; }
            public bool IsSaveEdrAs24bit { get; set; } = true;
            public bool IsKeepRawImages { get; set; }
            public string BitmapScalingMode { get; set; } = "HighQuality";
            public string ChemiModuleWiFiUrl { get; set; } = "http://192.168.12.1/home";
            public string ChemiModuleLANUrl { get; set; } = "http://192.168.1.40/home";
            public bool IsLaserTempLogging { get; set; }
            public int LaserTempLoggingInterval { get; set; } = 30;
            public int XMotionTurnDelay { get; set; }

            public double XMotionExtraMoveLength { get; set; }
            public double YMotionExtraMoveLength { get; set; }

            public bool MotionPolarityClkX { get; set; }
            public bool MotionPolarityDirX { get; set; }
            public bool MotionPolarityEnableX { get; set; }
            public bool MotionPolarityHomeX { get; set; }
            public bool MotionPolarityFwdX { get; set; }
            public bool MotionPolarityBwdX { get; set; }
            public bool MotionPolarityClkY { get; set; }
            public bool MotionPolarityDirY { get; set; }
            public bool MotionPolarityEnableY { get; set; }
            public bool MotionPolarityHomeY { get; set; }
            public bool MotionPolarityFwdY { get; set; }
            public bool MotionPolarityBwdY { get; set; }
            public bool MotionPolarityClkZ { get; set; }
            public bool MotionPolarityDirZ { get; set; }
            public bool MotionPolarityEnableZ { get; set; }
            public bool MotionPolarityHomeZ { get; set; }
            public bool MotionPolarityFwdZ { get; set; }
            public bool MotionPolarityBwdZ { get; set; }
            public bool MotionPolarityClkW { get; set; }
            public bool MotionPolarityDirW { get; set; }
            public bool MotionPolarityEnableW { get; set; }
            public bool MotionPolarityHomeW { get; set; }
            public bool MotionPolarityFwdW { get; set; }
            public bool MotionPolarityBwdW { get; set; }
            public bool HomeMotionsAtLaunchTime { get; set; }
            public double YCompenOffset { get; set; }
            public bool YCompenSationBitAt { get; set; }
            public bool ScanDynamicBitAt { get; set; }
            public int RadiatorTemperatureL { get; set; }
            public int RadiatorTemperatureR1 { get; set; }
            public int RadiatorTemperatureR2 { get; set; }
            public bool ImageOffsetProcessing { get; set; }
            public bool PixelOffsetProcessing { get; set; }
            public int PixelOffsetProcessingRes { get; set; } = 50;
            public int XOddNumberedLine { get; set; }
            public int XEvenNumberedLine { get; set; }
            public int YOddNumberedLine { get; set; }
            public int YEvenNumberedLine { get; set; }
            //比如将100微米的图像取平均新图为50微米
            public bool PhosphorModuleProcessing { get; set; }
            public bool AllModuleProcessing { get; set; }

            //Control conditions for the fan speed 
            public double InternalLowTemperature { get; set; }
            public double InternalModerateTemperature { get; set; }
            public double InternalHighTemperature { get; set; }
            public double ModuleLowTemperature { get; set; }
            public double ModuleModerateTemperature { get; set; }
            public double ModuleHighTemperature { get; set; }


            public List<ResolutionType> ResolutionOptions
            {
                get { return _ResolutionOptions; }
            }

            public List<QualityType> QualityOptions
            {
                get { return _QualityOptions; }
            }

            public List<ScanSpeedType> ScanSpeedOptions
            {
                get { return _ScanSpeedOptions; }
            }

            public List<ScanQualityType> ScanQualityOptions
            {
                get { return _ScanQualityOptions; }
            }

            public ObservableCollection<APDGainType> APDGains
            {
                get { return _APDGains; }
            }
            public ObservableCollection<FocusType> Focus
            {
                get { return _Focus; }
            }

            public ObservableCollection<APDPgaType> APDPgas
            {
                get { return _APDPgas; }
            }
            public ObservableCollection<PhosphorLaserModules> PhosphorLaserModules
            {
                get { return _PhosphorLaserModules; }
            }
            public ObservableCollection<MotorSettingsType> MotorSettings
            {
                get { return _MotorSettings; }
            }

            public ObservableCollection<FilterType> FilterOptions
            {
                get { return _FilterOptions; }
            }

            public int[] LasersIntensities
            {
                get { return _LasersIntensities; }
            }

            public List<LaserSettingsType> LasersIntensitySettings
            {
                get { return _LaserIntensitySettings; }
            }

            public List<SampleTypeSetting> SampleTypeSettings
            {
                get { return _SampleTypeSettings; }
            }

            public List<Signal> PhosphorSignalOptions
            {
                get { return _PhosphorSignalOptions; }
            }

            public Dictionary<int, List<Signal>> LasersSignalList
            {
                get { return _LasersSignalList; }
                set { _LasersSignalList = value; }
            }

            public List<LaserTypes> LaserTypes
            {
                get { return _LaserOptions; }
                set { _LaserOptions = value; }
            }

            public List<Protocol> Protocols
            {
                get { return _Protocols; }
                set { _Protocols = value; }
            }

            public List<Protocol> PhosphorProtocols
            {
                get { return _PhosphorProtocols; }
                set { _PhosphorProtocols = value; }
            }

            public List<ImagingSettings> ImagingSettings
            {
                get { return _ImagingSettings; }
            }

            public AutoScan AutoScanSettings
            {
                get { return _AutoScanSettings; }
            }

            public System.Windows.Rect AutoAlignScanRegion { get; set; }

            public bool IsChemiModule { get; set; } = false;

            public bool IsIgnoreCompCoefficient {  get; set; } = false;

            #endregion
        } 

    }

}
