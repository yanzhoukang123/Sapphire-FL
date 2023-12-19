using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Windows.Media;
using DrawToolsLib;
using Azure.Common;

namespace Azure.Image.Processing
{
    public enum ImageChannelFlag
    {
        None = 0x00,
        Red = 0x01,
        Green = 0x02,
        Blue = 0x04,
        Gray = 0x08,   //following ImageJ image color channel naming scheme (see ImageJ color merge)
        OverAll = 0x10,
    }

    [Serializable()]
    public enum ImageChannelType
    {
        None,
        Red,
        Green,
        Blue,
        Gray,
        Mix,    //All channels
    }

    [Serializable()]
    public class ImageChannel : ICloneable
    {
        // Laser scanner
        [OptionalField(VersionAdded = 6)]
        private int _ApdGain = 0;
        [OptionalField(VersionAdded = 6)]
        private int _ApdPga = 0;
        [OptionalField(VersionAdded = 6)]
        private int _LaserIntensityLevel = 0;
        [OptionalField(VersionAdded = 6)]
        private int _LaserIntensity = 0;
        [OptionalField(VersionAdded = 6)]
        private string _LaserWavelength = string.Empty;
        [OptionalField(VersionAdded = 7)]
        private double _ScanZ0 = 0;
        [OptionalField(VersionAdded = 8)]
        private string _LaserChannel = string.Empty;
        private string _SignalLevel = string.Empty;
        [OptionalField(VersionAdded = 8)]
        private string _FilterWavelength = string.Empty;

        public ImageChannelType ColorChannel { get; set; }
        public double Exposure { get; set; }
        public int LightSource { get; set; }
        public int FilterPosition { get; set; }
        public int FocusPosition { get; set; }
        public int BlackValue { get; set; }
        public int WhiteValue { get; set; }
        public double GammaValue { get; set; }
        public bool IsAutoChecked { get; set; }
        public bool IsInvertChecked { get; set; }
        public bool IsSaturationChecked { get; set; }
        public int PrevBlackValue { get; set; }
        public int PrevWhiteValue { get; set; }
        public double PrevGammaValue { get; set; }
        // Laser scanner
        public int ApdGain
        {
            get { return _ApdGain; }
            set { _ApdGain = value; }
        }
        public int ApdPga
        {
            get { return _ApdPga; }
            set { _ApdPga = value; }
        }
        public int LaserIntensityLevel
        {
            get { return _LaserIntensityLevel; }
            set { _LaserIntensityLevel = value; }
        }
        public int LaserIntensity
        {
            get { return _LaserIntensity; }
            set { _LaserIntensity = value; }
        }
        /// <summary>
        /// Selected signal level on the signal intensity lookup level
        /// </summary>
        public string SignalLevel
        {
            get { return _SignalLevel; }
            set { _SignalLevel = value; }
        }
        public string LaserWavelength
        {
            get { return _LaserWavelength; }
            set { _LaserWavelength = value; }
        }
        public string FilterWavelength
        {
            get { return _FilterWavelength; }
            set { _FilterWavelength = value; }
        }
        /// <summary>
        /// Scanned focus position
        /// </summary>
        public double ScanZ0
        {
            get { return _ScanZ0; }
            set { _ScanZ0 = value; }
        }

        /// <summary>
        /// Laser channel (Laser Module Slot: L1 = ChannelC, R1 = ChannelA, R2 = ChannelB)
        /// </summary>
        public string LaserChannel
        {
            get { return _LaserChannel; }
            set { _LaserChannel = value; }
        }

        public ImageChannel(ImageChannelType colorChannelType = ImageChannelType.Mix)
        {
            this.ColorChannel = colorChannelType;
            this.BlackValue = 0;
            this.WhiteValue = 65535;
            this.GammaValue = 1.0;
            this.PrevBlackValue = 0;
            this.PrevWhiteValue = 65535;
            this.PrevGammaValue = 1.0;
        }

        // Copy constructor (deep copy)
        public ImageChannel(ImageChannel otherImageChannel)
        {
            this.ColorChannel = otherImageChannel.ColorChannel;
            this.Exposure = otherImageChannel.Exposure;
            this.LightSource = otherImageChannel.LightSource;
            this.FilterPosition = otherImageChannel.FilterPosition;
            this.FocusPosition = otherImageChannel.FocusPosition;
            this.BlackValue = otherImageChannel.BlackValue;
            this.WhiteValue = otherImageChannel.WhiteValue;
            this.GammaValue = otherImageChannel.GammaValue;
            this.IsAutoChecked = otherImageChannel.IsAutoChecked;
            this.IsInvertChecked = otherImageChannel.IsInvertChecked;
            this.IsSaturationChecked = otherImageChannel.IsSaturationChecked;
            this.PrevBlackValue = otherImageChannel.PrevBlackValue;
            this.PrevWhiteValue = otherImageChannel.PrevWhiteValue;
            this.PrevGammaValue = otherImageChannel.PrevGammaValue;
            this.ApdGain = otherImageChannel.ApdGain;
            this.ApdPga = otherImageChannel.ApdPga;
            this.LaserIntensityLevel = otherImageChannel.LaserIntensityLevel;
            this.LaserIntensity = otherImageChannel.LaserIntensity;
            this.LaserWavelength = otherImageChannel.LaserWavelength;
            //this._IntensityLevelDN = otherImageChannel._IntensityLevelDN;
    }

        public object Clone()
        {
            ImageChannel clone = (ImageChannel)this.MemberwiseClone();
            clone.ColorChannel = new ImageChannelType();
            clone.ColorChannel = this.ColorChannel;
            return clone;
        }
    }

    [Serializable()]
    public class ImageInfo : ICloneable
    {
        #region Private fields/data...

        [OptionalField(VersionAdded = 2)]
        private ImageChannel _RedChannel = null;    // Channel 1
        [OptionalField(VersionAdded = 2)]
        private ImageChannel _GreenChannel = null;  // Channel 2
        [OptionalField(VersionAdded = 2)]
        private ImageChannel _BlueChannel = null;   // Channel 3
        [OptionalField(VersionAdded = 2)]
        private ImageChannel _GrayChannel = null;   // Channel 4 - gray channel (4-channel image)
        [OptionalField(VersionAdded = 2)]
        private ImageChannel _MixChannel = null;   // RGB composite

        [OptionalField(VersionAdded = 4)]
        private string _ScanType = string.Empty;

        [OptionalField(VersionAdded = 5)]
        ImageChannelFlag _AvailChannelFlags;
        [OptionalField(VersionAdded = 5)]
        private string _LaserAWavelength = string.Empty;
        [OptionalField(VersionAdded = 5)]
        private string _LaserBWavelength = string.Empty;
        [OptionalField(VersionAdded = 5)]
        private string _LaserCWavelength = string.Empty;
        [OptionalField(VersionAdded = 5)]
        private string _LaserDWavelength = string.Empty;

        [OptionalField(VersionAdded = 6)]
        private bool _IsSaturationChecked = false;
        [OptionalField(VersionAdded = 6)]
        ImageChannelFlag _DisplayChannelFlags;  // selected channels to display
        [OptionalField(VersionAdded = 6)]
        ImageChannelFlag _ContrastChannelFlags; // selected channels to contrast
        [OptionalField(VersionAdded = 7)]
        private double _ScanZ0Abs = 0;          // system absolute focus position
        [OptionalField(VersionAdded = 7)]
        private Guid _GroupID;  // Group or set ID.

        [OptionalField(VersionAdded = 8)]
        private string _SystemSN = string.Empty;
        [OptionalField(VersionAdded = 8)]
        private string _ModifiedDate = string.Empty;
        // Collection contains instances of GraphicsBase-derived classes.
        [OptionalField(VersionAdded = 8)]
        private PropertiesGraphicsBase[] _DrawingGraphics;
        [OptionalField(VersionAdded = 8)]
        private bool _IsShowScalebar;
        [OptionalField(VersionAdded = 8)]
        private ScaleBar _Scalebar;
        [OptionalField(VersionAdded = 9)]
        private string _ChannelRemark = string.Empty;
        //[OptionalField(VersionAdded = 9)]
        //private int _DynamicBit = 0;
        [OptionalField(VersionAdded = 9)]
        private string _ScanQuality = string.Empty;
        [OptionalField(VersionAdded = 9)]
        private bool _IsSmartscanDespeckled = false;

        #endregion

        #region Public properties...

        // Capture software
        public string Software { get; set; } = string.Empty;
        // Capture software version string
        public string SoftwareVersion { get; set; } = string.Empty;
        public int MajorVersion
        {
            get
            {
                int nMajorVersion = 0;
                if (SoftwareVersion != null)
                {
                    var version = SoftwareVersion.Split('.');
                    if (!string.IsNullOrEmpty(version[0]))
                    {
                        //nMajorVersion = int.Parse(version[0]);
                        int.TryParse(version[0], out nMajorVersion);
                        //minorVersion = int.Parse(version[1]);
                    }
                }
                return nMajorVersion;
            }
        }
        public int MinorVersion
        {
            get
            {
                int nMinorVersion = 0;
                if (SoftwareVersion != null)
                {
                    var version = SoftwareVersion.Split('.');
                    if (version != null)
                    {
                        //nMajorVersion = int.Parse(version[0]);
                        nMinorVersion = int.Parse(version[1]);
                    }
                }
                return nMinorVersion;
            }
        }

        public string CameraFirmware { get; set; } = string.Empty;

        // Imager type/model
        public string Model { get; set; } = string.Empty;
        // User's comment
        public string Comment { get; set; } = string.Empty;

        // NOTE: Included for backward compatibility. Use to determine
        //       if an image is a chemi image on file open.
        public int LightSourceChan1 { get; set; }

        // Camera readout speed
        public string ReadoutSpeed { get; set; } = string.Empty;

        // Non-color channel contrast values
        public int BlackValue { get; set; }
        public int WhiteValue { get; set; }
        public double GammaValue { get; set; }
        public int PrevBlackValue { get; set; }
        public int PrevWhiteValue { get; set; }
        public double PrevGammaValue { get; set; }

        public bool IsSaturationChecked
        {
            get { return _IsSaturationChecked; }
            set { _IsSaturationChecked = value; }
        }

        // Color image channels
        public ImageChannel RedChannel
        {
            get { return _RedChannel; }
            set { _RedChannel = value; }
        }
        public ImageChannel GreenChannel
        {
            get { return _GreenChannel; }
            set { _GreenChannel = value; }
        }
        public ImageChannel BlueChannel
        {
            get { return _BlueChannel; }
            set { _BlueChannel = value; }
        }
        public ImageChannel GrayChannel
        {
            get { return _GrayChannel; }
            set { _GrayChannel = value; }
        }
        // Composite image
        public ImageChannel MixChannel
        {
            get { return _MixChannel; }
            set { _MixChannel = value; }
        }

        public int NumOfChannels { get; set; }
        // Handle multi-channel (4-channel) image
        //public ImageChannelFlag SelectedChannelFlags { get; set; }

        public ImageChannelFlag AvailChannelFlags
        {
            get { return _AvailChannelFlags; }
            set { _AvailChannelFlags = value; }
        }
        public ImageChannelFlag DisplayChannelFlags
        {
            get { return _DisplayChannelFlags; }
            set { _DisplayChannelFlags = value; }
        }
        public ImageChannelFlag ContrastChannelFlags
        {
            get { return _ContrastChannelFlags; }
            set { _ContrastChannelFlags = value; }
        }

        /// <summary>
        /// Helper function to set or clear a bit in the flags field.
        /// </summary>
        /// <param name="flag">The Flag bit to set or clear.</param>
        /// <param name="value">True to set, false to clear the bit in the flags field.</param>
        private void SetFlag(ImageChannelFlag flag, bool value)
        {
            if (value)
            {
                _AvailChannelFlags |= flag;
            }
            else
            {
                _AvailChannelFlags &= ~flag;
            }
        }
        public bool IsRedChannelAvail
        {
            get { return (_AvailChannelFlags & ImageChannelFlag.Red) != 0; }
            set { this.SetFlag(ImageChannelFlag.Red, value); }
        }
        public bool IsGreenChannelAvail
        {
            get { return (_AvailChannelFlags & ImageChannelFlag.Green) != 0; }
            set { this.SetFlag(ImageChannelFlag.Green, value); }
        }
        public bool IsBlueChannelAvail
        {
            get { return (_AvailChannelFlags & ImageChannelFlag.Blue) != 0; }
            set { this.SetFlag(ImageChannelFlag.Blue, value); }
        }
        public bool IsGrayChannelAvail
        {
            get { return (_AvailChannelFlags & ImageChannelFlag.Gray) != 0; }
            set { this.SetFlag(ImageChannelFlag.Gray, value); }
        }

        private void SetDisplayFlag(ImageChannelFlag flag, bool value)
        {
            if (value)
            {
                _DisplayChannelFlags |= flag;
            }
            else
            {
                _DisplayChannelFlags &= ~flag;
            }
        }
        public bool IsDisplayRedChannel
        {
            get { return (_DisplayChannelFlags & ImageChannelFlag.Red) != 0; }
            set { this.SetDisplayFlag(ImageChannelFlag.Red, value); }
        }
        public bool IsDisplayGreenChannel
        {
            get { return (_DisplayChannelFlags & ImageChannelFlag.Green) != 0; }
            set { this.SetDisplayFlag(ImageChannelFlag.Green, value); }
        }
        public bool IsDisplayBlueChannel
        {
            get { return (_DisplayChannelFlags & ImageChannelFlag.Blue) != 0; }
            set { this.SetDisplayFlag(ImageChannelFlag.Blue, value); }
        }
        public bool IsDisplayGrayChannel
        {
            get { return (_DisplayChannelFlags & ImageChannelFlag.Gray) != 0; }
            set { this.SetDisplayFlag(ImageChannelFlag.Gray, value); }
        }
        public bool IsDisplayOverAll
        {
            get { return (_DisplayChannelFlags & ImageChannelFlag.OverAll) != 0; }
            set { this.SetDisplayFlag(ImageChannelFlag.OverAll, value); }
        }

        private void SetContrastFlag(ImageChannelFlag flag, bool value)
        {
            if (value)
            {
                _ContrastChannelFlags |= flag;
            }
            else
            {
                _ContrastChannelFlags &= ~flag;
            }
        }
        public bool IsContrastRedChannel
        {
            get { return (_ContrastChannelFlags & ImageChannelFlag.Red) != 0; }
            set { this.SetContrastFlag(ImageChannelFlag.Red, value); }
        }
        public bool IsContrastGreenChannel
        {
            get { return (_ContrastChannelFlags & ImageChannelFlag.Green) != 0; }
            set { this.SetContrastFlag(ImageChannelFlag.Green, value); }
        }
        public bool IsContrastBlueChannel
        {
            get { return (_ContrastChannelFlags & ImageChannelFlag.Blue) != 0; }
            set { this.SetContrastFlag(ImageChannelFlag.Blue, value); }
        }
        public bool IsContrastGrayChannel
        {
            get { return (_ContrastChannelFlags & ImageChannelFlag.Gray) != 0; }
            set { this.SetContrastFlag(ImageChannelFlag.Gray, value); }
        }

        public string LaserAWavelength
        {
            get { return _LaserAWavelength; }
            set { _LaserAWavelength = value; }
        }
        public string LaserBWavelength
        {
            get { return _LaserBWavelength; }
            set { _LaserBWavelength = value; }
        }
        public string LaserCWavelength
        {
            get { return _LaserCWavelength; }
            set { _LaserCWavelength = value; }
        }
        public string LaserDWavelength
        {
            get { return _LaserDWavelength; }
            set { _LaserDWavelength = value; }
        }

        public int Aperture { get; set; }
        public int BinFactor { get; set; }
        public string DateTime { get; set; } = string.Empty;
        public string CaptureType { get; set; } = string.Empty;
        public string Calibration { get; set; } = string.Empty;

        // Current selected color image channel
        public ImageChannelType SelectedChannel { get; set; }

        public int SaturationThreshold { get; set; }
        public int GainValue { get; set; }  //Camera gain
        public ushort PhotometricInterpretation { get; set; }
        public bool IsPixelInverted { get; set; }
        public bool IsChemiImage { get; set; }
        public bool IsScannedImage { get; set; }
        public Guid GroupID
        {
            get { return _GroupID; }
            set { _GroupID = value; }
        }

        // Scanner image info
        public string FpgaFirmware { get; set; }
        public string FWVersion { get; set; } = string.Empty;
        public int ApdAGain { get; set; }
        public int ApdBGain { get; set; }
        public int ApdCGain { get; set; }
        public int ApdDGain { get; set; }
        public int ApdAPga { get; set; }
        public int ApdBPga { get; set; }
        public int ApdCPga { get; set; }
        public int ApdDPga { get; set; }
        public int LaserAIntensity { get; set; }
        public int LaserBIntensity { get; set; }
        public int LaserCIntensity { get; set; }
        public int LaserDIntensity { get; set; }
        //public int ScanQuality { get; set; }
        /// <summary>
        /// Quality = Unidirectional (2-line average) ON/OFF (NOT the same as 'Quality' in ScannerModeSettings)
        ///           Highest = unidirectional scan ON
        ///           High = unidirectional scan OFF
        /// </summary>
        public string ScanQuality
        {
            get { return _ScanQuality; }
            set { _ScanQuality = value; }
        }
        public int ScanResolution { get; set; }
        public int ScanX0 { get; set; }
        public int ScanY0 { get; set; }
        public int DeltaX { get; set; }
        public int DeltaY { get; set; }
        public double ScanZ0 { get; set; }
        /// <summary>
        /// System absolute focus position.
        /// </summary>
        public double ScanZ0Abs
        {
            get { return _ScanZ0Abs; }
            set { _ScanZ0Abs = value; }
        }
        [OptionalField(VersionAdded = 2)]
        public string SampleType = string.Empty;
        [OptionalField(VersionAdded = 2)]
        public string ScanSpeed = string.Empty;
        [OptionalField(VersionAdded = 2)]
        public string IntensityLevel = string.Empty;
        public string ScanRegion { get; set; }

        [OptionalField(VersionAdded = 3)]
        public string AutoExposureType = string.Empty;
        public string ScanType
        {
            get { return _ScanType; }
            set { _ScanType = value; }
        }

        public string ChannelRemark
        {
            get { return _ChannelRemark; }
            set { _ChannelRemark = value; }
        }
        //public int DynamicBit
        //{
        //    get { return _DynamicBit; }
        //    set { _DynamicBit = value; }
        //}
        public int DynamicBit { get; set; } = 0;
        public int EdrBitDepth { get; set; } = 0;
        public int MaxPixelValue { get; set; } = 65535;
        public string SystemSN
        {
            get { return _SystemSN; }
            set { _SystemSN = value; }
        }
        public string ModifiedDate
        {
            get { return _ModifiedDate; }
            set { _ModifiedDate = value; }
        }

        public bool IsSmartscanDespeckled
        {
            get { return _IsSmartscanDespeckled; }
            set { _IsSmartscanDespeckled = value; }
        }

        public PropertiesGraphicsBase[] DrawingGraphics
        {
            get { return _DrawingGraphics; }
            set { _DrawingGraphics = value; }
        }
        public bool IsShowScalebar
        {
            get { return _IsShowScalebar; }
            set { _IsShowScalebar = value; }
        }
        public ScaleBar Scalebar
        {
            get { return _Scalebar; }
            set { _Scalebar = value; }
        }

        #endregion

        #region Constructors...

        public ImageInfo()
        {
            _RedChannel = new ImageChannel(ImageChannelType.Red);       // Channel 1
            _GreenChannel = new ImageChannel(ImageChannelType.Green);   // Channel 2
            _BlueChannel = new ImageChannel(ImageChannelType.Blue);     // Channel 3
            _GrayChannel = new ImageChannel(ImageChannelType.Gray);     // Channel 4 - gray channel (4-channel image)
            _MixChannel = new ImageChannel(ImageChannelType.Mix);       // composite

            this.NumOfChannels = 1;

            this.SelectedChannel = ImageChannelType.Mix;

            this.DateTime = string.Empty;
            this.CaptureType = string.Empty;
            this.Calibration = string.Empty;
            this.GainValue = -1;
            this.SaturationThreshold = 62000;
            // 0 = WhiteIsZero; 1 = BlackIsZero; 2 = RGB
            this.PhotometricInterpretation = 1;
            _Scalebar = new ScaleBar();
        }

        #endregion

        #region Public methods...
        public object Clone()
        {
            ImageInfo clone = new ImageInfo();
            clone = this.MemberwiseClone() as ImageInfo;
            clone.SelectedChannel = this.SelectedChannel;
            clone.RedChannel = (this._RedChannel != null) ? (ImageChannel)this._RedChannel.Clone() : null;
            clone.GreenChannel = (this._GreenChannel != null) ? (ImageChannel)this._GreenChannel.Clone() : null;
            clone.BlueChannel = (this._BlueChannel != null) ? (ImageChannel)this._BlueChannel.Clone() : null;
            clone.GrayChannel = (this._GrayChannel != null) ? (ImageChannel)this._GrayChannel.Clone() : null;
            clone.MixChannel = (this._MixChannel != null) ? (ImageChannel)this._MixChannel.Clone() : null;
            if (this.DateTime != null)
                clone.DateTime = string.Copy(this.DateTime);
            if (this.CaptureType != null)
                clone.CaptureType = string.Copy(this.CaptureType);
            if (this.Calibration != null)
                clone.Calibration = string.Copy(this.Calibration);
            if (this.AutoExposureType != null)
                clone.AutoExposureType = string.Copy(this.AutoExposureType);
            if (this.SampleType != null)
                clone.SampleType = string.Copy(this.SampleType);
            //if (this.Protocol != null)
            //    clone.Protocol = string.Copy(this.Protocol);
            //if (this.UserName != null)
            //    clone.UserName = string.Copy(this.UserName);
            if (this.ScanSpeed != null)
                clone.ScanSpeed = string.Copy(this.ScanSpeed);
            if (this.IntensityLevel != null)
                clone.IntensityLevel = string.Copy(this.IntensityLevel);
            if (this.FWVersion != null)
                clone.FWVersion = string.Copy(this.FWVersion);
            if (this._ScanType != null)
                clone.ScanType = string.Copy(this._ScanType);
            if (this._LaserAWavelength != null)
                clone._LaserAWavelength = string.Copy(this._LaserAWavelength);
            if (this._LaserBWavelength != null)
                clone._LaserBWavelength = string.Copy(this._LaserBWavelength);
            if (this._LaserCWavelength != null)
                clone._LaserCWavelength = string.Copy(this._LaserCWavelength);
            if (this._LaserDWavelength != null)
                clone._LaserDWavelength = string.Copy(this._LaserDWavelength);
            if (this._SystemSN != null)
                clone._SystemSN = string.Copy(this._SystemSN);
            if (this._ModifiedDate != null)
                clone._ModifiedDate = string.Copy(this._ModifiedDate);
            if (this.Software != null)
                clone.Software = string.Copy(this.Software);
            if (this.SoftwareVersion != null)
                clone.SoftwareVersion = string.Copy(this.SoftwareVersion);
            if (this.CameraFirmware != null)
                clone.CameraFirmware = string.Copy(this.CameraFirmware);
            if (this.Model != null)
                clone.Model = string.Copy(this.Model);
            if (this.Comment != null)
                clone.Comment = string.Copy(this.Comment);
            if (this._Scalebar != null)
                clone.Scalebar = (ScaleBar)this._Scalebar.Clone();
            if (this._DrawingGraphics != null)
                clone._DrawingGraphics = (PropertiesGraphicsBase[])this._DrawingGraphics.Clone();
            return clone;
        }

        //public object Clone()
        //{
        //    using (System.IO.MemoryStream memStream = new System.IO.MemoryStream())
        //    {
        //        if (this.GetType().IsSerializable)
        //        {
        //            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
        //            formatter.Serialize(memStream, this);
        //            memStream.Flush();
        //            memStream.Position = 0;
        //            return formatter.Deserialize(memStream);
        //        }
        //        return null;
        //    }
        //}

        #endregion

    }
}
