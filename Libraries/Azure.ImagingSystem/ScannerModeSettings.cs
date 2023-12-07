using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Azure.Image.Processing;
using Azure.EthernetCommLib;

namespace Azure.ImagingSystem
{
    /// <summary>
    /// Blue  = 488nm = LaserD
    /// Green = 520nm = LaserB
    /// IR700 = 658nm = LaserC
    /// IR800 = 784nm = LaserA
    /// </summary>
    public enum LaserType
    {
        None = 0,
        LaserA = 780,
        LaserB = 532,
        LaserC = 685,
        LaserD = 488
    }

    public enum ApdType
    {
        None,
        ApdA,
        ApdB,
        ApdC,
        ApdD
    }

    public enum MotorType
    {
        X = 1,
        Y = 2,
        Z = 3,
        W
    }

    public enum SampleType
    {
        Gel,
        Plate,
        Membrane,
        Custom,
    }

    public enum ScanTypes
    {
        Unknown,
        Static,
        Vertical,
        Horizontal,
        XAxis,
    }

    public class MotorSettingsType
    {
        #region Public properties...

        public MotorType MotorType { get; set; }
        public double Position { get; set; }
        public double Speed { get; set; }
        public double Accel { get; set; }
        public double Dccel { get; set; }

        #endregion  //Public properties

        #region Constructors...

        public MotorSettingsType()
        {
        }

        public MotorSettingsType(MotorType motorType, int position, int speed)
        {
            this.MotorType = motorType;
            this.Position = position;
            this.Speed = speed;
        }

        #endregion  //Constructors
    }

    public class APDGainType
    {
        #region Public properties...

        public int Position { get; set; }

        public int Value { get; set; }

        public string DisplayName { get; set; }

        #endregion

        #region Constructors...

        public APDGainType()
        {
        }

        public APDGainType(int position, int value, string displayName)
        {
            this.Position = position;
            this.Value = value;
            this.DisplayName = displayName;
        }

        #endregion
    }

    public class FocusType
    {
        #region Public properties...

        public int Position { get; set; }

        public double Value { get; set; }

        public string DisplayName { get; set; }

        #endregion

        #region Constructors...

        public FocusType()
        {
        }

        public FocusType(int position, int value, string displayName)
        {
            this.Position = position;
            this.Value = value;
            this.DisplayName = displayName;
        }

        #endregion
    }

    public class APDPgaType
    {
        #region Public properties...

        public int Position { get; set; }

        public int Value { get; set; }

        public string DisplayName { get; set; }

        #endregion

        #region Constructors...

        public APDPgaType()
        {
        }

        public APDPgaType(int position, int value, string displayName)
        {
            this.Position = position;
            this.Value = value;
            this.DisplayName = displayName;
        }

        #endregion
    }
    public class PhosphorLaserModules
    {
        #region Public properties...

        public string DisplayName { get; set; }

        #endregion

        #region Constructors...

        public PhosphorLaserModules()
        {
        }

        public PhosphorLaserModules(string displayName)
        {
            this.DisplayName = displayName;
        }

        #endregion
    }

    [Serializable]
    public class ResolutionType
    {
        #region Public properties...

        public int Position { get; set; }

        public int Value { get; set; }

        public string DisplayName { get; set; }

        #endregion

        #region Constructors...

        public ResolutionType()
        {
        }

        public ResolutionType(int position, int value, string displayName)
        {
            this.Position = position;
            this.Value = value;
            this.DisplayName = displayName;
        }

        #endregion

        public object Clone()
        {
            ResolutionType clone = (ResolutionType)this.MemberwiseClone();
            return clone;
        }
    }

    public class QualityType
    {
        #region Public properties...

        public int Position { get; set; }

        public int Value { get; set; }

        public string DisplayName { get; set; }

        #endregion

        #region Constructors...

        public QualityType()
        {
        }

        public QualityType(int position, int value, string displayName)
        {
            this.Position = position;
            this.Value = value;
            this.DisplayName = displayName;
        }

        #endregion

    }


    [Serializable]
    public class ScanSpeedType
    {
        #region Public properties...

        public int Position { get; set; }

        public int Value { get; set; }

        public string DisplayName { get; set; }

        #endregion

        #region Constructors...

        public ScanSpeedType()
        {
        }

        public ScanSpeedType(int position, int value, string displayName)
        {
            this.Position = position;
            this.Value = value;
            this.DisplayName = displayName;
        }

        #endregion

        public object Clone()
        {
            ScanSpeedType clone = (ScanSpeedType)this.MemberwiseClone();
            return clone;
        }
    }

    [Serializable]
    public class ScanQualityType
    {
        #region Public properties...

        public int Position { get; set; }

        public int Value { get; set; }

        public string DisplayName { get; set; }

        #endregion

        #region Constructors...

        public ScanQualityType()
        {
        }

        public ScanQualityType(int position, int value, string displayName)
        {
            this.Position = position;
            this.Value = value;
            this.DisplayName = displayName;
        }

        #endregion

        public object Clone()
        {
            ScanSpeedType clone = (ScanSpeedType)this.MemberwiseClone();
            return clone;
        }
    }

    /// <summary>
    /// Laser intensities setting
    /// </summary>
    public class LaserSettingsType
    {
        #region Public properties...

        public int Position { get; set; }
        public LaserType LaserType { get; set; }
        public List<int> Intensities { get; set; }
        public int MaxIntensity { get; set; }

        #endregion

        #region Constructors...

        public LaserSettingsType()
        {
            this.Intensities = new List<int>();
        }

        public LaserSettingsType(int position, LaserType laserType, List<int> intensities, int maxIntensity)
        {
            this.Position = position;
            this.LaserType = laserType;
            this.Intensities = intensities;
            this.MaxIntensity = maxIntensity;
        }

        #endregion
    }

    [Serializable]
    public class SampleTypeSetting
    {
        #region Public properties...

        public int Position { get; set; }
        public string DisplayName { get; set; }
        //public SampleType SampleType { get; set; }
        public double FocusPosition { get; set; }
        public bool IsDefaultSampleType { get; set; }

        #endregion

        #region Constructors...

        public SampleTypeSetting()
        {
        }

        /*public SampleTypeSetting(string displayName, SampleType sampleType, double focusPosition)
        {
            this.DisplayName = displayName;
            this.SampleType = sampleType;
            this.FocusPosition = focusPosition;
        }*/

        public SampleTypeSetting(int position, string displayName, double focusPosition)
        {
            this.Position = position;
            this.DisplayName = displayName;
            this.FocusPosition = focusPosition;
        }

        #endregion

        public object Clone()
        {
            SampleTypeSetting clone = (SampleTypeSetting)this.MemberwiseClone();
            return clone;
        }

        /*public bool Equals(SampleTypeSetting otherSampleType)
        {
            bool bResult = false;

            if (otherSampleType == null) return bResult;

            if (this.DisplayName.Equals(otherSampleType.DisplayName, StringComparison.OrdinalIgnoreCase) &&
                this.FocusPosition == otherSampleType.FocusPosition)
            {
                bResult = true;
            }

            return bResult;
        }*/
    }

    public class Signal
    {
        public int Position { get; set; }
        public string DisplayName { get; set; }
        public LaserType LaserType { get; set; }
        public int LaserIntensity { get; set; }
        public int ApdGain { get; set; }
        public int ApdPga { get; set; }
        public int ColorChannel { get; set; }
        public int LaserIntInmW { get; set; }
        public int LaserWavelength { get; set; }
        public LaserChannels LaserChannel { get; set; }
        public IvSensorType SensorType { get; set; } = IvSensorType.APD;
        public int SignalLevel { get; set; }

        public Signal()
        {
        }

        public Signal(int position, LaserType laserType, int laserIntInmW, int laserInt, int gain, int pga, int laserWavelength, LaserChannels laserChannel, IvSensorType sensorType)
        {
            this.Position = position;
            this.LaserType = laserType;
            this.LaserIntInmW = laserIntInmW;
            this.LaserIntensity = laserInt;
            this.ApdGain = gain;
            this.ApdPga = pga;
            this.LaserWavelength = laserWavelength;
            this.LaserChannel = laserChannel;
            this.SensorType = sensorType;
        }
        public object Clone()
        {
            Signal clone = (Signal)this.MemberwiseClone();
            return clone;
        }
    }

    //[Serializable]
    /*public class DyeType
    {
        #region Public properties...

        public int Position { get; set; }
        public string DisplayName { get; set; }
        public LaserType LaserType { get; set; }
        public string WaveLength { get; set; }
        public bool IsCustomDye { get; set; }

        #endregion

        #region Constructors...

        public DyeType()
        {
        }

        public DyeType(int position, string displayName, LaserType laserType, string waveLength, bool bIsCustomDye)
        {
            this.Position = position;
            this.DisplayName = displayName;
            this.LaserType = laserType;
            this.WaveLength = waveLength;
            this.IsCustomDye = bIsCustomDye;
        }

        #endregion

        public object Clone()
        {
            DyeType clone = (DyeType)this.MemberwiseClone();
            return clone;
        }
    }*/

    [Serializable]
    public class FilterType
    {
        #region Public properties...

        public int Position { get; set; }
        public int Wavelength { get; set; }
        public string Bandpass { get; set; }

        #endregion

        #region Constructors...

        public FilterType()
        {
        }

        public FilterType(int waveLength, string bandpass)
        {
            this.Wavelength = waveLength;
            this.Bandpass = bandpass;
        }

        public FilterType(int position, int waveLength, string bandpass)
        {
            this.Position = position;
            this.Wavelength = waveLength;
            this.Bandpass = bandpass;
        }

        #endregion

        public string Filter
        {
            get
            {
                string filter = string.Empty;
                if (Wavelength > 0)
                {
                    filter = string.Format("{0}BP{1}", Wavelength, Bandpass);
                }
                return filter;
            }
            set
            {
                if (value != null)
                {
                    if (value.Contains("BP"))
                    {
                        string[] numbers = System.Text.RegularExpressions.Regex.Split(value, @"\D+");
                        if (numbers.Length == 2)
                        {
                            if (!string.IsNullOrEmpty(numbers[0]))
                            {
                                Wavelength = int.Parse(numbers[0]);
                            }
                            if (!string.IsNullOrEmpty(numbers[1]))
                            {
                                Bandpass = numbers[1];
                            }
                        }
                    }
                }
            }
        }

        public object Clone()
        {
            FilterType clone = (FilterType)this.MemberwiseClone();
            return clone;
        }

    }

    [Serializable]
    public class LaserTypes
    {
        #region Public properties...

        public string DisplayName
        {
            get
            {
                string displayName = string.Empty;
                if (!string.IsNullOrEmpty(Filter))
                {
                    displayName = Wavelength + " / " + Filter;
                }
                else
                {
                    displayName = Wavelength.ToString();
                }
                return displayName;
            }
        }
        public LaserChannels LaserChannel;
        public IvSensorType SensorType;
        public int Wavelength { get; set; }
        public string Filter { get; set; }
        public int SignalIntensity { get; set; }
        public ImageChannelType ColorChannel { get; set; }

        #endregion

        #region Constructors...

        public LaserTypes()
        {
        }

        public LaserTypes(LaserChannels laserChannel, IvSensorType sensorType, int waveLength, string filter, int laserIntensity, ImageChannelType colorChannel)
        {
            this.LaserChannel = laserChannel;
            this.SensorType = sensorType;
            this.Wavelength = waveLength;
            this.Filter = filter;
            this.SignalIntensity = laserIntensity;
            this.ColorChannel = colorChannel;
        }

        #endregion

        public object Clone()
        {
            LaserTypes clone = (LaserTypes)this.MemberwiseClone();
            return clone;
        }
    }

    public class Protocol
    {
        public string Name { get; set; }
        public bool IsAlwaysVisible { get; set; }
        public List<ScanRegion> ScanRegions { get; set; }

        public Protocol()
        {
            ScanRegions = new List<ScanRegion>();
        }
    }

    public class ScanRegion
    {
        //public string DisplayName { get; set; }

        /// <summary>
        /// Scan region number
        /// </summary>
        public int RegionNumber { get; set; } = 1;
        public int SampleType { get; set; }
        public int PixelSize { get; set; }
        public int ScanSpeed { get; set; }
        public int ScanQuality { get; set; } = 1;
        public Rect ScanRect { get; set; }
        public int IntensityLevel { get; set; } // currently only being used for Phosphor imaging.

        // Custom focus
        public bool IsCustomFocus { get; set; }
        public double CustomFocusPosition { get; set; }
        public bool IsAutoSaveEnabled { get; set; }
        public string AutoSavePath { get; set; } = string.Empty;

        // Z-Scan parameters
        public bool IsZScan { get; set; }
        public double BottomImageFocus { get; set; }
        public double DeltaFocus { get; set; }
        public int NumOfImages { get; set; } = 2;

        public List<LaserTypes> Lasers = null;

        public ScanRegion()
        {
            Lasers = new List<LaserTypes>();
        }
    }

    public class AppDyeData
    {
        public int DyerType { get; set; }
        public int SignalIntensity { get; set; }
        public ImageChannelType ColorChannel { get; set; }

        public AppDyeData()
        {
        }
    }

    public class AutoScan
    {
        public int Resolution;
        public int HighResolution;
        public int OptimalVal;
        public double OptimalDelta;
        public int Ceiling;
        public int Floor;
        public double Alpha488;
        public int StartingSignalLevel;
        public int HighSignalStepdownLevel;

        public Signal LaserASignalLevel;
        public Signal LaserBSignalLevel;
        public Signal LaserCSignalLevel;
        public Signal LaserDSignalLevel;

        public AutoScan()
        {
        }
    }

    public class PreviewChannel

    {
        #region Public properties...

        public int Position { get; set; }

        public int Value { get; set; }

        public string DisplayName { get; set; }

        #endregion

        #region Constructors...

        public PreviewChannel()
        {
        }

        public PreviewChannel(int position, int value, string displayName)
        {
            this.Position = position;
            this.Value = value;
            this.DisplayName = displayName;
        }

        #endregion

    }

    public class ScanParameterStruct
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int XMotorSpeed { get; set; }
        public int YMotorSpeed { get; set; }
        public int ZMotorSpeed { get; set; }
        public int ScanX0 { get; set; }
        public int ScanY0 { get; set; }
        public int ScanZ0 { get; set; }
        public int ScanDeltaX { get; set; }
        public int ScanDeltaY { get; set; }
        public int ScanDeltaZ { get; set; }
        public int Res { get; set; }
        /// <summary>
        /// Quality = Scan speed (NOT the same as 'Quality' in ImageInfo)
        /// </summary>
        public int Quality { get; set; }
        public int DataRate { get; set; }
        public int LineCounts { get; set; }
        public int Time { get; set; }
        public double XMotorSubdivision { get; set; }
        public double YMotorSubdivision { get; set; }
        public double ZMotorSubdivision { get; set; }
        public double XEncoderSubdivision { get; set; }
        /// <summary>
        /// new firmware is version after 2.0.1.1 (included 2.0.1.1);
        /// new firmware takes control of X,Y,Z motion;
        /// </summary>
        public bool IsNewFirmwire { get; set; }
        /// <summary>
        /// Unit of msec
        /// 
        /// </summary>
        public int XmotionTurnAroundDelay { get; set; }
        /// <summary>
        /// Unit of mm
        /// </summary>
        public double XMotionExtraMoveLength { get; set; }
        public double YMotionExtraMoveLength { get; set; }
        public double LCoefficient { get; set; } = 0;
        public double L375Coefficient { get; set; } = 0;
        public double R1Coefficient { get; set; } = 0;
        public double R2Coefficient { get; set; } = 0;
        public double R2532Coefficient { get; set; } = 0;
        public bool IsIgnoreCompCoefficient { get; set; } = false;
        public int XMotionAccVal { get; set; }
        public int YMotionAccVal { get; set; }
        public int ZMotionAccVal { get; set; }
        public int XMotionDccVal { get; set; }
        public int YMotionDccVal { get; set; }
        public int ZMotionDccVal { get; set; }
        public int DynamicBits { get; set; }
        public int HorizontalCalibrationSpeed { get; set; }
        public bool DynamicBitsAt { get; set; }
        public int BackBufferStride { get; set; }
        public bool IsUnidirectionalScan { get; set; }

        // Smart scan parameters
        public bool IsSmartScanning;
        public int SmartScanResolution;
        public int SmartScanSignalLevels;
        public int SmartScanFloor;
        public int SmartScanCeiling;
        public int SmartScanOptimalVal;
        public double SmartScanOptimalDelta;
        public double SmartScanAlpha488;
        public int SmartScanInitSignalLevel;
        public int SmartScanSignalStepdownLevel;
        public List<Signal> LaserL1SignalOptions;
        public List<Signal> LaserR1SignalOptions;
        public List<Signal> LaserR2SignalOptions;
        public List<Signal> Signals;

        public bool IsSequentialScanning;
        // Z-Scanning (Z-Stacking) parameters
        public bool IsZScanning = false;
        public double BottomImageFocus;
        public double DeltaFocus;
        public int NumOfImages;
        public double AbsFocusPosition;
        // Extended Dynamic Range
        public bool IsEdrScanning = false;
        public int EdrScaleFactor = 1;

        public ImageAlignParam AlignmentParam;
        public bool Is5micronScan { get; set; }
    }

    public class ImageAlignParam
    {
        public int Resolution { get; set; }
        public int PixelOddX { get; set; }
        public int PixelEvenX { get; set; }
        public int PixelOddY { get; set; }
        public int PixelEvenY { get; set; }
        public double YCompOffset { get; set; }
        public double OpticalL_R1Distance { get; set; }
        public double OpticalR2_R1Distance { get; set; }
        public int Pixel_10_L_DX { get; set; }
        public int Pixel_10_L_DY { get; set; }
        public int Pixel_10_R2_DX { get; set; }
        public int Pixel_10_R2_DY { get; set; }
        public bool IsYCompensationBitAt { get; set; }
        public bool IsImageOffsetProcessing { get; set; }
        public bool IsPixelOffsetProcessing { get; set; }
        public int PixelOffsetProcessingRes { get; set; } = 50;
        public LaserChannels LaserChannel { get; set; }   //L1/A, R1/B, R2/C
        public double XMotionExtraMoveLength { get; set; }
        public double YMotionExtraMoveLength { get; set; }

        public ImageAlignParam()
        {
        }

        public object Clone()
        {
            ImageAlignParam clone = (ImageAlignParam)this.MemberwiseClone();
            return clone;
        }
    }

    /*public class LaserImageChannel
    {
        public System.Windows.Media.Imaging.WriteableBitmap ImageBuffer { get; set; }
        public LaserType LaserChannel { get; set; }
        public bool IsSelected { get; set; }

        public LaserImageChannel()
        {
        }
    }*/

}
