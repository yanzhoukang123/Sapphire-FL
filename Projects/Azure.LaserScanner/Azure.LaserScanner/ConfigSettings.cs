using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;   //ObservableCollection
using Azure.ImagingSystem;  //LightingType
using Azure.LaserScanner.ViewModel;

namespace Azure.LaserScanner
{
    class ConfigSettings
    {
        #region Private data...

        private ObservableCollection<BinningFactorType> _BinningFactorOptions = new ObservableCollection<BinningFactorType>();
        private List<PvCamSensitivity> _PvCamSensitivityOptions = new List<PvCamSensitivity>();
        private ObservableCollection<GainType> _GainOptions = new ObservableCollection<GainType>();
        private ObservableCollection<ResolutionType> _ResolutionOptions = new ObservableCollection<ResolutionType>();
        private ObservableCollection<QualityType> _QualityOptions = new ObservableCollection<QualityType>();
        private ObservableCollection<APDGainType> _APDGains = new ObservableCollection<APDGainType>();
        private ObservableCollection<APDPgaType> _APDPgas = new ObservableCollection<APDPgaType>();
        private int _XMaxValue = 0;
        private int _YMaxValue = 0;
        private int _ZMaxValue = 0;
        private int _XMotorSubdivision = 0;
        private int _YMotorSubdivision = 0;
        private int _ZMotorSubdivision = 0;
        private ObservableCollection<MotorSettingsType> _MotorSettings = new ObservableCollection<MotorSettingsType>();
        private int[] _LasersIntensities = new int[4];
        private List<LaserSettingsType> _LaserIntensitySettings = new List<LaserSettingsType>();
        private List<SampleTypeSetting> _SampleTypeSettings = new List<SampleTypeSetting>();
        private List<RgbLedIntensity> _RgbLedIntensities = new List<RgbLedIntensity>();
        private ChemiSetting _ChemiSettings = new ChemiSetting();

        #endregion

        #region Constructors...

        public ConfigSettings()
        {
            ComPort = 3;
            GalilAddress = "COM1 115200";
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
        public int XMotorSubdivision
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
        public int YMotorSubdivision
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
        public int ZMotorSubdivision
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

        public int ComPort { get; set; }
        public String GalilAddress { get; set; }
        public bool IsSimulationMode { get; set; }

        public ObservableCollection<BinningFactorType> BinningFactorOptions
        {
            get { return _BinningFactorOptions; }
        }

        public List<PvCamSensitivity> PvCamSensitivityOptions
        {
            get { return _PvCamSensitivityOptions; }
        }

        public ObservableCollection<GainType> GainOptions
        {
            get { return _GainOptions; }
        }

        public ObservableCollection<ResolutionType> ResolutionOptions
        {
            get { return _ResolutionOptions; }
        }

        public ObservableCollection<QualityType> QualityOptions
        {
            get { return _QualityOptions; }
        }

        public ObservableCollection<APDGainType> APDGains
        {
            get { return _APDGains; }
        }

        public ObservableCollection<APDPgaType> APDPgas
        {
            get { return _APDPgas; }
        }

        public ObservableCollection<MotorSettingsType> MotorSettings
        {
            get { return _MotorSettings; }
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

        public List<RgbLedIntensity> RgbLedIntensities
        {
            get { return _RgbLedIntensities; }
        }

        public ChemiSetting ChemiSettings
        {
            get { return _ChemiSettings; }
        }

        #endregion
    }
    
     
}
