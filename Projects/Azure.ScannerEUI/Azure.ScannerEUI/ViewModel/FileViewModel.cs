/************************************************************************

   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the New BSD
   License (BSD) as published at http://avalondock.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up AvalonDock in Extended WPF Toolkit Plus at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like facebook.com/datagrids

  **********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Input;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media;
//using System.Windows;
//using System.Windows.Controls;    //Image
using System.Drawing;
//using Azure.Controller;
using Azure.Image.Processing;
using Azure.WPF.Framework;  //RelayCommand
//using DrawToolsLib;
using Azure.Ipp.Imaging;
using Azure.Configuration.Settings;

namespace Azure.ScannerEUI.ViewModel
{
    public enum ZoomType
    {
        ZoomIn,
        ZoomOut,
        ZoomFit
    }

    //public enum DrawingMode
    //{
    //    Select = 0,
    //    DrawRectangle = 1,
    //    DrawLine = 2,
    //    DrawTextBox = 3
    //}

    public enum DirtyType { NewCreate, Modified, None }

    public class FileViewModel : PaneViewModel
    {
        //public delegate void ZoomUpdateDelegate(ZoomType zoomType);
        //public event ZoomUpdateDelegate ZoomUpdateEvent;
        //public delegate void CropAdornerDelegate(bool bIsVisible);
        //public event CropAdornerDelegate CropAdornerEvent;
        //public delegate void CropAdornerRectDelegate();
        //public event CropAdornerRectDelegate CropAdornerRectEvent;

        #region Private data....

        static ImageSourceConverter ISC = new ImageSourceConverter();
        private double _ZoomLevel = 1.0;
        private double _MinimumZoom = 1.0;
        private string _PixelX = string.Empty;
        private string _PixelY = string.Empty;
        private string _PixelIntensity = string.Empty;
        //private WriteableBitmap _RotatingDisplayImage = null;
        private WriteableBitmap _RotatingImage = null;
        private WriteableBitmap _Image = null;
        private WriteableBitmap _DisplayImage = null;
        private ImageInfo _ImageInfo = null;
        private bool _IsShowImageInfo = false;
        //private ImageViewer _ImageViewer = null;
        //private DrawingCanvas _DrawingCanvas = null;
        //private DrawToolsLib.ToolType _SelectedDrawingTool = ToolType.None;

        private const int _SaturationThreshold = 62000;

        //private const int MaxPixelValue = 65535;
        //private int _MaxWhiteValue = 65535;
        //private int _LargeChange = 10;
        //private int _SmallChange = 1;
        //private int _WhiteValue = 65535;
        //private int _BlackValue = 0;
        //private double _GammaValue = 0.0;
        //private ImageChannelType _SelectedChannelType = ImageChannelType.ChannelMix;

        //private DrawingMode _DrawingMode = DrawingMode.Select;

        private bool _IsCropping = false;
        private string _CropX;
        private string _CropY;
        private string _CropWidth;
        private string _CropHeight;

        // Use to distinguish between newly created image, and a modified image
        private DirtyType _DocDirtyType = DirtyType.None;
        private bool _IsImageChannelChanged = false;

        private Rect _CropRect;
        private bool _IsTriggerGetCropRect = false;

        private ZoomType _ZoomingType = ZoomType.ZoomFit;
        private bool _IsRGBImageCropped = false;    // Work-around for RGB image cropping crash
        private bool _IsManualContrast = false;
        private bool _IsEditComment = false;

        #endregion

        #region Public properties....

        public bool IsInitialized { get; set; }
        public bool IsFileLoaded { get; set; }
        //public double CanvasWidth { get; set; }
        //public double CanvasHeight { get; set; }

        //public int MaxPixelValue { get; set; }
        //public int MaxWhiteValue { get; set; }

        //public int SavedBlackValue { get; set; }
        //public int SavedWhiteValue { get; set; }
        //public double SavedGammaValue { get; set; }

        public int Width
        {
            get
            {
                int width = 0;
                if (_Image != null)
                {
                    width = _Image.PixelWidth;
                }
                return width;
            }
        }

        public int Height
        {
            get
            {
                int height = 0;
                if (_Image != null)
                {
                    height = _Image.PixelHeight;
                }
                return height;
            }
        }

        public int Angle
        {
            get;
            set;
        }

        public string ArrowLineTime
        {
            get;
            set;
        }

        public string ImageTypePriview
        {
            get;
            set;
        }
        #endregion

        #region Constructors...

        public FileViewModel()
        {
            Title = FileName;
            IsDirty = true;
        }

        public FileViewModel(string filePath)
        {
            FilePath = filePath;
            Title = FileName;
            string extension = System.IO.Path.GetExtension(filePath).ToLower();

            //Set the icon only for open documents (just a test)
            //iIconSource = ISC.ConvertFromInvariantString(@"pack://application:,,/Images/document.png") as ImageSource;

            WriteableBitmap imageContent = null;
            int nWidth = 0;
            int nHeight = 0;

            try
            {
                imageContent = ImageProcessing.LoadImage(filePath);
                bool bIsPixelInverted = false;

                if (imageContent != null)
                {
                    try
                    {
                        // get image info from the comments section of the image metadata
                        _ImageInfo = ImageProcessing.ReadMetadata(filePath);

                        #region Backward compatibility
                        if (_ImageInfo != null)
                        {
                            string[] version = null;
                            int iMajorVersion = 0;
                            int iMinorVersion = 0;
                            if (!string.IsNullOrEmpty(_ImageInfo.SoftwareVersion))
                            {
                                version = _ImageInfo.SoftwareVersion.Split('.');
                                if (version.Length > 1)
                                {
                                    int.TryParse(version[0], out iMajorVersion);
                                    int.TryParse(version[1], out iMinorVersion);
                                }
                            }

                            if (_ImageInfo.RedChannel == null) { _ImageInfo.RedChannel = new ImageChannel(ImageChannelType.Red); }
                            if (_ImageInfo.GreenChannel == null) { _ImageInfo.GreenChannel = new ImageChannel(ImageChannelType.Green); }
                            if (_ImageInfo.BlueChannel == null) { _ImageInfo.BlueChannel = new ImageChannel(ImageChannelType.Blue); }
                            if (_ImageInfo.GrayChannel == null) { _ImageInfo.GrayChannel = new ImageChannel(ImageChannelType.Gray); }
                            if (_ImageInfo.MixChannel == null)
                            {
                                _ImageInfo.MixChannel = new ImageChannel(ImageChannelType.Mix);

                                if (extension.Equals(".tif") || extension.Equals(".tiff") || extension.Equals(".jpg"))
                                {
                                    bool bIsChemiImage = false;
                                    if (_ImageInfo.CaptureType.ToLower().Contains("chemi") &&
                                        _ImageInfo.LightSourceChan1 == 0x0A)    //0x0A is defined as None in cSeries
                                    {
                                        bIsChemiImage = true;
                                    }

                                    if (bIsChemiImage && (iMajorVersion > 1 || (iMajorVersion >= 1 && iMinorVersion >= 6)))
                                    {
                                        // Assume the Chemi image pixel were saved inverted.
                                        bIsPixelInverted = true;
                                    }
                                }
                            }
                        }
                        #endregion
                    }
                    catch
                    {
                        //try
                        //{
                        //    // backward compatibility (read old image information)
                        //    imageInfo = SW.Control.ImageTabControl.ReadMetadata(filePath);
                        //}
                        //catch
                        //{
                        //}
                    }

                    if (_ImageInfo != null)
                    {
                        //
                        // Invert the source data if chemi.
                        // The chemi image is saved with the pixels inverted starting with v1.6.8
                        //
                        // Chemi capture type:
                        //Chemi: Normal
                        //Chemi: Single Exposure
                        //Chemi: Cumulative
                        //Chemi: Multiple
                        //Chemi with marker: Chemi
                        if (extension.Equals(".tif") || extension.Equals(".tiff") || extension.Equals(".jpg"))
                        {
                            if (bIsPixelInverted || (_ImageInfo.IsChemiImage && _ImageInfo.IsPixelInverted))
                            {
                                imageContent = ImageProcessing.Invert(imageContent);
                                _ImageInfo.MixChannel.IsInvertChecked = true;
                            }
                        }
                    }
                    else
                    {
                        _ImageInfo = new ImageInfo();
                    }

                    if (imageContent.CanFreeze) { imageContent.Freeze(); }

                    Image = imageContent;
                    RotatingImage = imageContent;
                    // Set max pixel value
                    int bpp = _Image.Format.BitsPerPixel;
                    nWidth = _Image.PixelWidth;
                    nHeight = _Image.PixelHeight;
                    int maxPixelValue = 65535;
                    int dynamicBpp = _ImageInfo.DynamicBit;
                    if (dynamicBpp > 16)
                    {
                        //maxPixelValue = (1 << _ImageInfo.DynamicBit) - 1;
                        maxPixelValue = (int)Math.Pow(2, _ImageInfo.DynamicBit) - 1;
                    }
                    else
                    {
                        maxPixelValue = (bpp == 16 || bpp == 48 || bpp == 64) ? 65535 : 255;
                    }
                    MaxPixelValue = maxPixelValue;
                    MaxWhiteValue = maxPixelValue;

                    _ImageInfo.MixChannel.WhiteValue = maxPixelValue;
                    _ImageInfo.RedChannel.WhiteValue = maxPixelValue;
                    _ImageInfo.GreenChannel.WhiteValue = maxPixelValue;
                    _ImageInfo.BlueChannel.WhiteValue = maxPixelValue;
                    _ImageInfo.GrayChannel.WhiteValue = maxPixelValue;

                    // Default to mix/overall (grayscale image default type is Mix)
                    _ImageInfo.SelectedChannel = ImageChannelType.Mix;

                    BitmapPalette palette = null;
                    PixelFormat dstPixelFormat = PixelFormats.Rgb24;
                    if (bpp == 8 || bpp == 16)
                    {
                        _ImageInfo.NumOfChannels = 1;
                        bool bIsSaturation = _ImageInfo.MixChannel.IsSaturationChecked;
                        dstPixelFormat = PixelFormats.Indexed8;
                        palette = new BitmapPalette(ImageProcessing.GetColorTableIndexed(bIsSaturation));
                    }
                    else if (bpp == 24 || bpp == 48)
                    {
                        _ImageInfo.NumOfChannels = 3;
                        dstPixelFormat = PixelFormats.Rgb24;
                        palette = null;
                        _DisplayImage = new WriteableBitmap(nWidth, nHeight, 96, 96, dstPixelFormat, palette);
                    }
                    else if (bpp == 32 && Image.Format == PixelFormats.Gray32Float)
                    {
                        _ImageInfo.NumOfChannels = 1;
                    }
                    else if (bpp == 64)
                    {
                        _ImageInfo.NumOfChannels = 4;
                    }

                    //BGR to RGB color order
                    if (_Image.Format == PixelFormats.Bgr24)
                    {
                        // Swap red and blue channel
                        WriteableBitmap[] imageChannels = { null, null, null };
                        imageChannels = ImageProcessing.GetChannel(_Image); // GetChannel returns BGR color order
                        if (imageChannels != null)
                        {
                            // SetChannel expect BGR color order??
                            _Image = ImageProcessing.SetChannel(imageChannels[0], imageChannels[1], imageChannels[2]);
                            imageChannels = null;
                        }
                    }

                    // Set saturation threshold
                    _ImageInfo.SaturationThreshold = (int)(maxPixelValue * ((double)_SaturationThreshold / 65535.0));

                    // Create display image buffer.
                    var dpiX = _Image.DpiX;
                    var dpiY = _Image.DpiY;
                    _DisplayImage = new WriteableBitmap(nWidth, nHeight, dpiX, dpiY, dstPixelFormat, palette);

                    UpdateDisplayImage();
                }
            }
            catch (Exception)
            {
                //string strMessage = string.Format("Error loading: {0}\n{1}", filePath, ex.Message);
                //string strCaption = "File loading error....";
                //Xceed.Wpf.Toolkit.MessageBox.Show(strMessage, strCaption, MessageBoxButton.OK, MessageBoxImage.Stop);
                //string message = string.Format("File loading error: {0}", ex.Message);
                //throw new Exception(message);
                throw;
            }
        }

        public FileViewModel(WriteableBitmap newImage, ImageInfo newImageInfo, string newImageTitle, bool bIsCropped, bool bIsGetMinMax = true)
        {
            //FilePath = newImageTitle;
            //Title = FileName;
            FilePath = null;
            Title = newImageTitle;
            RotatingImage = newImage;
            this.Image = newImage;
            this.ImageInfo = newImageInfo;
            int bpp = newImage.Format.BitsPerPixel;

            int maxPixelValue = 65535;
            // Extended dynamic range image?
            if (newImageInfo.DynamicBit > 16)
            {
                //maxPixelValue = (1 << _ImageInfo.DynamicBit) - 1;
                maxPixelValue = (int)Math.Pow(2, _ImageInfo.DynamicBit) - 1;
            }
            else
            {
                maxPixelValue = (bpp == 16 || bpp == 48 || bpp == 64) ? 65535 : 255;
            }

            MaxPixelValue = maxPixelValue;
            MaxWhiteValue = maxPixelValue;

            BitmapPalette palette = null;
            PixelFormat dstPixelFormat = PixelFormats.Rgb24;
            if (bpp == 8 || bpp == 16)
            {
                _ImageInfo.NumOfChannels = 1;
                bool bIsSaturation = _ImageInfo.MixChannel.IsSaturationChecked;
                dstPixelFormat = PixelFormats.Indexed8;
                palette = new BitmapPalette(ImageProcessing.GetColorTableIndexed(bIsSaturation));
                _DisplayImage = new WriteableBitmap(Width, Height, 96, 96, dstPixelFormat, palette);
            }
            else if (bpp == 32 && newImage.Format == PixelFormats.Gray32Float)
            {
                _ImageInfo.NumOfChannels = 1;
                dstPixelFormat = PixelFormats.Rgb24;
                palette = null;
                _DisplayImage = new WriteableBitmap(Width, Height, 96, 96, dstPixelFormat, palette);
            }
            else if (bpp == 24 || bpp == 48 || bpp == 64)
            {
                //_ImageInfo.NumOfChannels = 3;
                dstPixelFormat = PixelFormats.Rgb24;
                palette = null;
                _DisplayImage = new WriteableBitmap(Width, Height, 96, 96, dstPixelFormat, palette);

                // Set number of image channel.
                if (bpp == 24 || bpp == 48)         //3-channel image
                {
                    _ImageInfo.NumOfChannels = 3;
                }
                else if ((bpp == 32 && newImage.Format != PixelFormats.Gray32Float) || bpp == 64)    //4-channel image
                {
                    _ImageInfo.NumOfChannels = 4;
                }
            }

            if (bpp == 16 || bpp == 48 || bpp == 64)
            {
                // Chemi cumulative: use the same initial black and white value.
                if (bIsGetMinMax)
                {
                    uint minValue = 0;
                    uint maxValue = 0;
                    Rect roiRect = new Rect(0, 0, this.Image.PixelWidth, this.Image.PixelHeight);
                    ImageProcessing.MinMax(this.Image, roiRect, ref minValue, ref maxValue);

                    if (newImageInfo.DynamicBit > 16)
                    {
                        double cnvFactor = newImageInfo.DynamicBit / 16.0;
                        int tmpMinValue = (int)Math.Pow(minValue, cnvFactor);
                        int tmpMaxValue = (int)Math.Pow(maxValue, cnvFactor);
                        ImageInfo.MixChannel.BlackValue = tmpMinValue;
                        ImageInfo.MixChannel.WhiteValue = tmpMaxValue;
                    }
                    else
                    {
                        ImageInfo.MixChannel.BlackValue = (int)minValue;
                        ImageInfo.MixChannel.WhiteValue = (int)maxValue;
                        ImageInfo.MixChannel.GammaValue = 1.0;
                    }
                }
            }

            if (IsRgbImage && _ImageInfo.SelectedChannel != ImageChannelType.Mix)
            {
                _ImageInfo.SelectedChannel = ImageChannelType.Mix;
            }

            UpdateDisplayImage();
        }

        #endregion

        #region public double MinimumZoom
        public double MinimumZoom
        {
            get { return _MinimumZoom; }
            set
            {
                if (_MinimumZoom != value)
                {
                    _MinimumZoom = value;
                    if (_MinimumZoom > 1.0)
                    {
                        _MinimumZoom = 1.0;
                    }
                }
            }
        }
        #endregion

        #region public double ZoomLevel
        /// <summary>
        /// Get/set the gallery ZoomLevel display string.
        /// </summary>
        public double ZoomLevel
        {
            get { return _ZoomLevel; }
            set
            {
                if (_ZoomLevel != value)
                {
                    _ZoomLevel = value;
                    RaisePropertyChanged("ZoomLevel");
                }
            }
        }
        #endregion

        /*#region public DrawingCanvas DrawingCanvas
        public DrawingCanvas DrawingCanvas
        {
            get { return _DrawingCanvas; }
            set
            {
                if (_DrawingCanvas != value)
                {
                    _DrawingCanvas = value;
                    RaisePropertyChanged("DrawingCanvas");
                }
            }
        }
        #endregion*/

        /*#region public DrawToolsLib.ToolType SelectedDrawingTool
        public DrawToolsLib.ToolType SelectedDrawingTool
        {
            get
            {
                return _SelectedDrawingTool;
            }
            set
            {
                if (_SelectedDrawingTool != value)
                {
                    _SelectedDrawingTool = value;
                    _DrawingCanvas.Tool = _SelectedDrawingTool;
                    RaisePropertyChanged("SelectedDrawingTool");
                }
            }
        }
        #endregion*/

        #region public ImageInfo ImageInfo
        public ImageInfo ImageInfo
        {
            get { return _ImageInfo; }
            set
            {
                _ImageInfo = value;
            }
        }
        #endregion

        #region public bool IsShowImageInfo
        public bool IsShowImageInfo
        {
            get { return _IsShowImageInfo; }
            set
            {
                if (_Image != null)
                {
                    if (_IsShowImageInfo != value)
                    {
                        _IsShowImageInfo = value;
                        RaisePropertyChanged("IsShowImageInfo");
                        //RaisePropertyChanged("IsScannedImage");
                    }
                }
            }
        }
        #endregion

        public bool IsScannedImage
        {
            get
            {
                bool bResult = false;
                if (_ImageInfo != null)
                {
                    if (_ImageInfo.CaptureType.Contains("Scan") ||
                            _ImageInfo.IsScannedImage)
                    {
                        bResult = true;
                    }
                }
                return bResult;
            }
        }

        public string FormattedExposureTime
        {
            get
            {
                if (_ImageInfo == null)
                {
                    return string.Empty;
                }

                string exposureTime = string.Empty;
                if (IsRgbImage)
                {
                    string strExpTimeCh1 = (_ImageInfo.RedChannel.Exposure > 0) ? GetFormattedExposureTime(_ImageInfo.RedChannel.Exposure) : "--";
                    string strExpTimeCh2 = (_ImageInfo.GreenChannel.Exposure > 0) ? string.Format(" / {0}", GetFormattedExposureTime(_ImageInfo.GreenChannel.Exposure)) : " / --";
                    string strExpTimeCh3 = string.Empty;
                    if (_ImageInfo.CaptureType.IndexOf("RGB", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        strExpTimeCh3 = (_ImageInfo.BlueChannel.Exposure > 0) ? string.Format(" / {0}", GetFormattedExposureTime(_ImageInfo.BlueChannel.Exposure)) : " / --";
                    }
                    else
                    {
                        strExpTimeCh3 = (_ImageInfo.BlueChannel.Exposure > 0) ? string.Format(" / {0}", GetFormattedExposureTime(_ImageInfo.BlueChannel.Exposure)) : "";
                    }
                    exposureTime = string.Format("{0}{1}{2}", strExpTimeCh1, strExpTimeCh2, strExpTimeCh3);
                }
                else
                {
                    if (_ImageInfo.RedChannel.Exposure > 0)
                    {
                        exposureTime = GetFormattedExposureTime(_ImageInfo.RedChannel.Exposure);
                    }
                }

                return exposureTime;
            }
        }

        private string GetFormattedExposureTime(double dExposureTime)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(dExposureTime);

            string strMin = (timeSpan.Minutes > 0) ? string.Format("{0}m", timeSpan.Minutes) : string.Empty;
            string strSec = (timeSpan.Seconds > 0) ? string.Format("{0}s", timeSpan.Seconds) : string.Empty;
            string strMsec = (timeSpan.Milliseconds > 0) ? string.Format("{0}ms", timeSpan.Milliseconds) : string.Empty;
            string strExposureTime = string.Format("{0}{1}{2}", strMin, strSec, strMsec);
            return strExposureTime;
        }

        /*public string FormattedLightSource
        {
            get
            {
                if (_ImageInfo == null)
                {
                    return string.Empty;
                }

                string lightSource = string.Empty;
                if (IsRGBImage)
                {
                    string strLightCh1 = (_ImageInfo.LightSourceChan1 > 0) ? GetFormattedLightSource(_ImageInfo.LightSourceChan1) : "--";
                    string strLightCh2 = (_ImageInfo.LightSourceChan2 > 0) ? string.Format(" / {0}", GetFormattedLightSource(_ImageInfo.LightSourceChan2)) : " / --";
                    string strLightCh3 = string.Empty;
                    if (_ImageInfo.CaptureType.IndexOf("RGB", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        strLightCh3 = (_ImageInfo.LightSourceChan3 > 0) ? string.Format(" / {0}", GetFormattedLightSource(_ImageInfo.LightSourceChan3)) : " / --";
                    }
                    else
                    {
                        strLightCh3 = (_ImageInfo.LightSourceChan3 > 0) ? string.Format(" / {0}", GetFormattedExposureTime(_ImageInfo.LightSourceChan3)) : "";
                    }
                    lightSource = string.Format("{0}{1}{2}", strLightCh1, strLightCh2, strLightCh3);
                }
                else
                {
                    if (_ImageInfo.LightSourceChan1 > 0)
                    {
                        lightSource = GetFormattedLightSource(_ImageInfo.LightSourceChan1);
                    }
                }

                return lightSource;
            }
        }*/

        /*private string GetFormattedLightSource(int lightPosition)
        {
            string result = string.Empty;

            if (Workspace.This.Owner != null)
            {
                System.Collections.ObjectModel.ObservableCollection<cSeries.UI.LightingType> lightTypeOptions = Workspace.This.Owner.LightingTypeOptions;

                if (lightTypeOptions != null)
                {
                    foreach (var lightType in lightTypeOptions)
                    {
                        if (lightType.Position == lightPosition)
                        {
                            // Display 'Blue' as 'Epi Blue'
                            if (lightType.DisplayName.Contains("Blue") && !lightType.DisplayName.Contains("Epi Blue"))
                            {
                                result = "Epi Blue";
                                break;
                            }
                            else if (lightType.DisplayName.Contains("White") && !lightType.DisplayName.Contains("Epi White"))
                            {
                                result = "Epi White";
                                break;
                            }
                            else
                            {
                                result = lightType.DisplayName;
                                break;
                            }
                        }
                    }
                }
            }
            return result;
        }*/

        public string GainValue
        {
            get
            {
                string result = string.Empty;
                if (_ImageInfo == null)
                {
                    result = string.Empty;
                }

                if (_ImageInfo.IsScannedImage)
                {
                    result = string.Format("{0} / {1} / {2} / {3}", _ImageInfo.ApdAGain,
                                                                    _ImageInfo.ApdBGain,
                                                                    _ImageInfo.ApdCGain,
                                                                    _ImageInfo.ApdCGain);
                }
                else
                {
                    if (_ImageInfo.GainValue > 0)
                    {
                        result = _ImageInfo.GainValue.ToString();
                    }
                    else
                    {
                        result = string.Empty;
                    }
                }
                return result;
            }
        }

        public string LasersIntensity
        {
            get
            {
                string lasersIntensity = string.Empty;
                if (_ImageInfo != null && _ImageInfo.IsScannedImage)
                {
                    lasersIntensity = string.Format("{0} / {1} / {2} / {3}",
                        _ImageInfo.LaserAIntensity,
                        _ImageInfo.LaserBIntensity,
                        _ImageInfo.LaserCIntensity,
                        _ImageInfo.LaserDIntensity);
                }
                return lasersIntensity;
            }
        }

        public string ScanRegion
        {
            get
            {
                string scanRegion = string.Empty;

                if (_ImageInfo != null && _ImageInfo.IsScannedImage)
                {
                    scanRegion = string.Format("{0}, {1}, {2}, {3}",
                            Math.Round(_ImageInfo.ScanX0 / 10.0),
                            Math.Round(_ImageInfo.ScanY0 / 10.0),
                            _ImageInfo.DeltaX, _ImageInfo.DeltaY);
                }
                return scanRegion;
            }
        }


        public bool IsImageChannelChanged
        {
            get { return _IsImageChannelChanged; }
            set
            {
                if (_IsImageChannelChanged != value)
                {
                    _IsImageChannelChanged = value;
                    RaisePropertyChanged("IsImageChannelChanged");
                }
            }
        }

        #region Cropping public properties...

        public bool IsCropping
        {
            get { return _IsCropping; }
            set
            {
                if (_IsCropping != value)
                {
                    if (Image != null)
                    {
                        if (Image.Format.BitsPerPixel == 32)
                        {
                            string caption = "Image type not supported...";
                            string message = "This operation is current not supported for 32-bit image.";
                            System.Windows.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                            return;
                        }
                    }

                    _IsCropping = value;
                    RaisePropertyChanged("IsCropping");
                    RaisePropertyChanged("IsAutoExposureToBand");
                }
            }
        }

        public bool IsTriggerGetCropRect
        {
            get { return _IsTriggerGetCropRect; }
            set
            {
                if (_IsTriggerGetCropRect != value)
                {
                    _IsTriggerGetCropRect = value;
                    RaisePropertyChanged("IsTriggerGetCropRect");
                }
            }
        }

        public Rect CropRect
        {
            get
            {
                //if (CropAdornerRectEvent != null)
                //{
                //    CropAdornerRectEvent();
                //}
                IsTriggerGetCropRect = true;
                return _CropRect;
            }
            set
            {
                if (_CropRect != value)
                {
                    _CropRect = value;
                    RaisePropertyChanged("CropRect");
                    IsTriggerGetCropRect = false;
                }
            }
        }


        public string CropX
        {
            get { return _CropX; }
            set
            {
                if (_CropX != value)
                {
                    _CropX = value;
                    RaisePropertyChanged("CropX");
                }
            }
        }

        public string CropY
        {
            get { return _CropY; }
            set
            {
                if (_CropY != value)
                {
                    _CropY = value;
                    RaisePropertyChanged("CropY");
                }
            }
        }

        public string CropWidth
        {
            get { return _CropWidth; }
            set
            {
                if (_CropWidth != value)
                {
                    _CropWidth = value;
                    RaisePropertyChanged("CropWidth");
                }
            }
        }

        public string CropHeight
        {
            get { return _CropHeight; }
            set
            {
                if (_CropHeight != value)
                {
                    _CropHeight = value;
                    RaisePropertyChanged("CropHeight");
                }
            }
        }

        #endregion


        #region FilePath
        private string _filePath = null;
        public string FilePath
        {
            get { return _filePath; }
            set
            {
                if (_filePath != value)
                {
                    _filePath = value;
                    RaisePropertyChanged("FilePath");
                    RaisePropertyChanged("FileName");
                    RaisePropertyChanged("Title");

                    //if (File.Exists(_filePath))
                    //{
                    //    //_textContent = File.ReadAllText(_filePath);
                    //    try
                    //    {
                    //        ImageContent = Load(_filePath);
                    //        IsDirty = false;
                    //    }
                    //    catch
                    //    {
                    //    }
                    //    ContentId = _filePath;
                    //}
                }
            }
        }
        #endregion

        #region public string FileName
        public string FileName
        {
            get 
            {
                if (FilePath == null)
                {
                    //return "Untitled" + (IsDirty ? "*" : "");
                    return Title + (IsDirty ? "*" : "");
                }

                return System.IO.Path.GetFileName(FilePath) + (IsDirty ? "*" : ""); 
            }
        }
        #endregion

        #region public WriteableBitmap Image
        public WriteableBitmap Image
        {
            get { return _Image; }
            set
            {
                //if (_Image != value)
                //{
                _Image = value;
                //RaisePropertyChanged("Image");
                //}
            }
        }
        /// <summary>
        /// 旋转后的图像
        /// Rotated image
        /// </summary>
        public WriteableBitmap RotatingImage
        {
            get { return _RotatingImage; }
            set
            {
                //if (_Image != value)
                //{
                _RotatingImage = value;
                //RaisePropertyChanged("Image");
                //}
            }
        }

        //public WriteableBitmap RotatingDisplayImage
        //{
        //    get { return _RotatingDisplayImage; }
        //    set
        //    {
        //        //if (_Image != value)
        //        //{
        //        _RotatingDisplayImage = value;
        //        //RaisePropertyChanged("Image");
        //        //}
        //    }
        //}

        #endregion

        #region public WriteableBitmap DisplayImage

        public WriteableBitmap DisplayImage
        {
            get
            {
                return _DisplayImage;
            }
            set
            {
                if (_DisplayImage != value)
                {
                    _DisplayImage = value;

                    if (_DisplayImage != null)
                    {
                        if (_DisplayImage.CanFreeze)
                        {
                            _DisplayImage.Freeze();
                        }
                    }

                    RaisePropertyChanged("DisplayImage");

                    if (IsAutoContrast)
                    {
                        RaisePropertyChanged("BlackValue");
                        RaisePropertyChanged("WhiteValue");
                        RaisePropertyChanged("GammaValue");
                    }

                    // In extreme situations force a garbage collection to free 
                    // up memory as quickly as possible.
                    if (_DisplayImage != null &&
                        _DisplayImage.PixelHeight * _DisplayImage.PixelWidth > (10000 * 10000))
                    {
                        GC.Collect();
                    }
                }
            }
        }

        #endregion

        #region public BitmapSource ImageSource
        private BitmapSource _ImageSource;
        /// <summary>
        /// Get/set WPF display image
        /// </summary>
        public BitmapSource ImageSource
        {
            get
            {
                /*if (_ZoomLevel == 1)
                {
                   _ZoomLevel = _WindowWidth / imageSource.Width; //Initial zoom level, fit to width only
                   _minimumZoom = _ZoomLevel;
                   if (_WindowHeight / imageSource.Height < _ZoomLevel)
                   {
                      _minimumZoom = _WindowHeight / imageSource.Height;
                      if (!newImage)
                         _ZoomLevel = _minimumZoom;
                   }
                   ZoomLevel = _ZoomLevel;
                   ZoomLevelString = _ZoomLevel.ToString();
                }*/

                return _ImageSource;
            }
            set
            {
                if (_ImageSource != value)
                {
                    _ImageSource = value;
                    RaisePropertyChanged("ImageSource");
                }
            }
        }
        #endregion


        public ZoomType ZoomingType
        {
            get { return _ZoomingType; }
            set
            {
                _ZoomingType = value;
                RaisePropertyChanged("ZoomingType");
            }
        }

        public void ZoomIn()
        {
            ZoomingType = ZoomType.ZoomIn;
        }

        public void ZoomOut()
        {
            ZoomingType = ZoomType.ZoomOut;
        }

        #region public string PixelX
        public string PixelX
        {
            get { return _PixelX; }
            set
            {
                if (_PixelX != value)
                {
                    _PixelX = value;
                    RaisePropertyChanged("PixelX");
                }
            }
        }
        #endregion

        #region public string PixelY
        public string PixelY
        {
            get { return _PixelY; }
            set
            {
                if (_PixelY != value)
                {
                    _PixelY = value;
                    RaisePropertyChanged("PixelY");
                }
            }
        }
        #endregion

        #region public string PixelIntensity
        public string PixelIntensity
        {
            get { return _PixelIntensity; }
            set
            {
                if (_PixelIntensity != value)
                {
                    _PixelIntensity = value;
                    RaisePropertyChanged("PixelIntensity");
                }
            }
        }
        #endregion

        #region public bool IsRgbImage

        public bool IsRgbImage
        {
            get
            {
                bool bResult = false;
                if (_Image != null)
                {
                    // We're using Gray32Float pixel format for the extended dynamic range image
                    if (_Image.Format.BitsPerPixel == 24 ||
                        (_Image.Format.BitsPerPixel == 32 && Image.Format != PixelFormats.Gray32Float) ||
                        _Image.Format.BitsPerPixel == 48 ||
                        _Image.Format.BitsPerPixel == 64)
                    {
                        bResult = true;
                    }
                    else
                    {
                        bResult = false;
                    }
                }

                return bResult;
            }
        }

        #endregion

        #region IsDirty

        private bool _isDirty = false;
        public bool IsDirty
        {
            get { return _isDirty; }
            set
            {
                if (_isDirty != value)
                {
                    _isDirty = value;
                    RaisePropertyChanged("IsDirty");
                    RaisePropertyChanged("FileName");
                    if (!_isDirty)
                    {
                        _DocDirtyType = DirtyType.None;
                    }
                }
            }
        }

        #endregion

        #region public string ToolTip
        /// <summary>
        /// Tooltip to display in the UI.
        /// </summary>
        public string ToolTip
        {
            get
            {
                /*
                var toolTip = new StringBuilder();
                if (string.IsNullOrEmpty(this.FilePath))
                {
                    toolTip.Append(UntitledFileName);
                }
                else
                {
                    toolTip.Append(this.FilePath);
                }

                if (this.IsModified)
                {
                    toolTip.Append("*");
                }

                return toolTip.ToString();
                */
                return this.FileName;
            }
        }
        #endregion

        public bool Is16BitImage
        {
            get
            {
                bool bIs16BitImage = (Image.Format.BitsPerPixel == 16) ? true : false;
                return bIs16BitImage;
            }
        }

        public DirtyType DocDirtyType
        {
            get { return _DocDirtyType; }
            set
            {
                if (_DocDirtyType != value)
                {
                    _DocDirtyType = value;
                    RaisePropertyChanged("FileDirtyType");
                    if (_DocDirtyType == DirtyType.NewCreate ||
                        _DocDirtyType == DirtyType.Modified)
                    {
                        IsDirty = true;
                    }
                    else
                    {
                        IsDirty = false;
                    }
                }
            }
        }

        /// <summary>
        /// Work-around for RGB image crop (crashes when contrasting individual channel)
        /// TODO: fine a better solution.
        /// </summary>
        public bool IsRGBImageCropped
        {
            get { return _IsRGBImageCropped; }
            set
            {
                if (_IsRGBImageCropped != value)
                {
                    _IsRGBImageCropped = value;
                    RaisePropertyChanged("IsRGBImageCropped");
                }
            }
        }

        /***#region SaveCommand
        RelayCommand _saveCommand = null;
        public ICommand SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                {
                    _saveCommand = new RelayCommand((p) => OnSave(p), (p) => CanSave(p));
                }

                return _saveCommand;
            }
        }

        private bool CanSave(object parameter)
        {
            return IsDirty;
        }

        private void OnSave(object parameter)
        {
            //Workspace.This.SaveAsync(this, false);
            Workspace.This.SaveSync(this, false);
        }

        #endregion***/

        /***#region SaveAsCommand
        RelayCommand _saveAsCommand = null;
        public ICommand SaveAsCommand
        {
            get
            {
                if (_saveAsCommand == null)
                {
                    _saveAsCommand = new RelayCommand((p) => OnSaveAs(p), (p) => CanSaveAs(p));
                }

                return _saveAsCommand;
            }
        }

        private bool CanSaveAs(object parameter)
        {
            return IsDirty;
        }

        private void OnSaveAs(object parameter)
        {
            //Workspace.This.SaveAsync(this, true);
            Workspace.This.SaveSync(this, true);
        }

        #endregion***/


        #region CloseCommand
        RelayCommand _closeCommand = null;
        public ICommand CloseCommand
        {
            get
            {
                if (_closeCommand == null)
                {
                    _closeCommand = new RelayCommand((p) => OnClose(), (p) => CanClose());
                }

                return _closeCommand;
            }
        }

        private bool CanClose()
        {
            return true;
        }

        private void OnClose()
        {
            Workspace.This.Close(this);
        }

        public void ReleaseMemory()
        {
            if (_Image != null)
            {
                Image = null;
            }

            if (_DisplayImage != null)
            {
                _DisplayImage = null;
            }

            if (_ImageInfo != null)
            {
                _ImageInfo = null;
            }

            //if (_ImageViewer != null)
            //{
            //    _ImageViewer = null;
            //}

            //if (_DrawingCanvas != null)
            //{
            //    _DrawingCanvas = null;
            //}
        }

        #endregion


        #region public int MaxWhiteValue
        private int _MaxWhiteValue = 65535;
        public int MaxWhiteValue
        {
            get { return _MaxWhiteValue; }
            set
            {
                if (_MaxWhiteValue != value)
                {
                    _MaxWhiteValue = value;
                    RaisePropertyChanged("MaxWhiteValue");
                }
            }
        }
        #endregion

        #region public int MaxPixelValue
        private int _MaxPixelValue = 65535;
        public int MaxPixelValue
        {
            get { return _MaxPixelValue; }
            set
            {
                if (_MaxPixelValue != value)
                {
                    _MaxPixelValue = value;
                    RaisePropertyChanged("MaxPixelValue");
                }
            }
        }
        #endregion

        #region public int LargeChange
        private int _LargeChange = 10;
        public int LargeChange
        {
            get
            {
                if (_Image != null)
                {
                    _LargeChange = (MaxPixelValue > 255) ? ((MaxPixelValue + 1) / 256) : 10;
                }

                return _LargeChange;
            }
            set
            {
                if (_LargeChange != value)
                {
                    _LargeChange = value;
                    RaisePropertyChanged("LargeChange");
                }
            }
        }
        #endregion

        #region public int SmallChange
        private int _SmallChange = 1;
        public int SmallChange
        {
            get
            {
                if (_Image != null)
                {
                    _SmallChange = (MaxPixelValue > 255) ? ((MaxPixelValue + 1) / 256) : 10;
                }

                return _SmallChange;
            }
            set
            {
                if (_SmallChange != value)
                {
                    _SmallChange = value;
                    RaisePropertyChanged("SmallChange");
                }
            }
        }
        #endregion

        #region public ImageChannelType SelectedChannelType
        public ImageChannelType SelectedChannelType
        {
            get
            {
                ImageChannelType ictSelectedCh = ImageChannelType.Mix;

                if (_ImageInfo != null)
                {
                    ictSelectedCh = _ImageInfo.SelectedChannel;
                }

                return ictSelectedCh;
            }
            set
            {
                if (_ImageInfo != null)
                {
                    IsImageChannelChanged = false;
                    if (_ImageInfo.SelectedChannel != value && !Workspace.This.IsProcessingContrast)
                    {
                        _ImageInfo.SelectedChannel = value;
                        RaisePropertyChanged("SelectedChannelType");
                        RaisePropertyChanged("BlackValue");
                        RaisePropertyChanged("WhiteValue");
                        RaisePropertyChanged("GammaValue");
                        RaisePropertyChanged("IsAutoContrast");
                        RaisePropertyChanged("IsInvert");
                        RaisePropertyChanged("IsSaturation");
                        IsImageChannelChanged = true;   // ignore the button pressed if already selected
                    }
                }
            }
        }
        #endregion

        #region public int BlackValue
        public int BlackValue
        {
            get
            {
                int iBlackValue = 0;
                if (_Image != null && _ImageInfo != null)
                {
                    if (_ImageInfo.SelectedChannel == ImageChannelType.Red)
                    {
                        iBlackValue = _ImageInfo.RedChannel.BlackValue;
                    }
                    else if (_ImageInfo.SelectedChannel == ImageChannelType.Green)
                    {
                        iBlackValue = _ImageInfo.GreenChannel.BlackValue;
                    }
                    else if (_ImageInfo.SelectedChannel == ImageChannelType.Blue)
                    {
                        iBlackValue = _ImageInfo.BlueChannel.BlackValue;
                    }
                    else if (_ImageInfo.SelectedChannel == ImageChannelType.Mix)
                    {
                        iBlackValue = _ImageInfo.MixChannel.BlackValue;
                    }
                }
                
                return iBlackValue;
            }   // get
            set
            {
                if (_Image != null && _ImageInfo != null)
                {
                    // Do not allow the black value to be >= to the white value.
                    if (value >= WhiteValue)
                    {
                        value = WhiteValue - 1;
                    }

                    if (_ImageInfo.SelectedChannel == ImageChannelType.Red)
                    {
                        if (_ImageInfo.RedChannel.BlackValue != value)
                        {
                            _ImageInfo.RedChannel.BlackValue = value;
                            RaisePropertyChanged("BlackValue");
                        }
                    }
                    else if (_ImageInfo.SelectedChannel == ImageChannelType.Green)
                    {
                        if (_ImageInfo.GreenChannel.BlackValue != value)
                        {
                            _ImageInfo.GreenChannel.BlackValue = value;
                            RaisePropertyChanged("BlackValue");
                        }
                    }
                    else if (_ImageInfo.SelectedChannel == ImageChannelType.Blue)
                    {
                        if (_ImageInfo.BlueChannel.BlackValue != value)
                        {
                            _ImageInfo.BlueChannel.BlackValue = value;
                            RaisePropertyChanged("BlackValue");
                        }
                    }
                    else if (_ImageInfo.SelectedChannel == ImageChannelType.Mix)
                    {
                        if (_ImageInfo.MixChannel.BlackValue != value)
                        {
                            _ImageInfo.MixChannel.BlackValue = value;
                            RaisePropertyChanged("BlackValue");
                        }
                    }
                }
            }   // set

        }
        #endregion

        #region public int WhiteValue
        public int WhiteValue
        {
            get
            {
                int iWhiteValue = 65535;
                if (_Image != null && _ImageInfo != null)
                {
                    if (_ImageInfo.SelectedChannel == ImageChannelType.Red)
                    {
                        iWhiteValue = _ImageInfo.RedChannel.WhiteValue;
                    }
                    else if (_ImageInfo.SelectedChannel == ImageChannelType.Green)
                    {
                        iWhiteValue = _ImageInfo.GreenChannel.WhiteValue;
                    }
                    else if (_ImageInfo.SelectedChannel == ImageChannelType.Blue)
                    {
                        iWhiteValue = _ImageInfo.BlueChannel.WhiteValue;
                    }
                    else if (_ImageInfo.SelectedChannel == ImageChannelType.Mix)
                    {
                        iWhiteValue = _ImageInfo.MixChannel.WhiteValue;
                    }
                }

                return iWhiteValue;
            }
            set
            {
                if (_Image != null)
                {
                    if (_ImageInfo != null)
                    {
                        // Do not allow the white value to be <= the black value
                        if (value <= BlackValue)
                        {
                            value = BlackValue + 1;
                        }

                        if (_ImageInfo.SelectedChannel == ImageChannelType.Red)
                        {
                            if (_ImageInfo.RedChannel.WhiteValue != value)
                            {
                                _ImageInfo.RedChannel.WhiteValue = value;
                                RaisePropertyChanged("WhiteValue");
                            }
                        }
                        else if (_ImageInfo.SelectedChannel == ImageChannelType.Green)
                        {
                            if (_ImageInfo.GreenChannel.WhiteValue != value)
                            {
                                _ImageInfo.GreenChannel.WhiteValue = value;
                                RaisePropertyChanged("WhiteValue");
                            }
                        }
                        else if (_ImageInfo.SelectedChannel == ImageChannelType.Blue)
                        {
                            if (_ImageInfo.BlueChannel.WhiteValue != value)
                            {
                                _ImageInfo.BlueChannel.WhiteValue = value;
                                RaisePropertyChanged("WhiteValue");
                            }
                        }
                        else if (_ImageInfo.SelectedChannel == ImageChannelType.Mix)
                        {
                            if (_ImageInfo.MixChannel.WhiteValue != value)
                            {
                                _ImageInfo.MixChannel.WhiteValue = value;
                                RaisePropertyChanged("WhiteValue");
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region public double GammaValue
        public double GammaValue
        {
            get
            {
                double dGammaValue = 1.0;
                if (_Image != null && _ImageInfo != null)
                {
                    if (_ImageInfo.SelectedChannel == ImageChannelType.Red)
                    {
                        dGammaValue = Math.Round(Math.Log10(_ImageInfo.RedChannel.GammaValue), 3);
                    }
                    else if (_ImageInfo.SelectedChannel == ImageChannelType.Green)
                    {
                        dGammaValue = Math.Round(Math.Log10(_ImageInfo.GreenChannel.GammaValue), 3);
                    }
                    else if (_ImageInfo.SelectedChannel == ImageChannelType.Blue)
                    {
                        dGammaValue = Math.Round(Math.Log10(_ImageInfo.BlueChannel.GammaValue), 3);
                    }
                    else if (_ImageInfo.SelectedChannel == ImageChannelType.Mix)
                    {
                        dGammaValue = Math.Round(Math.Log10(_ImageInfo.MixChannel.GammaValue), 3);
                    }
                }

                return dGammaValue;
            }
            set
            {
                if (_Image != null && _ImageInfo != null)
                {
                    if (_ImageInfo.SelectedChannel == ImageChannelType.Red)
                    {
                        if (_ImageInfo.RedChannel.GammaValue != value)
                        {
                            double dGammaValue = Math.Round(Math.Pow(10, value), 3);
                            _ImageInfo.RedChannel.GammaValue = dGammaValue;
                            RaisePropertyChanged("GammaValue");
                        }
                    }
                    else if (_ImageInfo.SelectedChannel == ImageChannelType.Green)
                    {
                        if (_ImageInfo.GreenChannel.GammaValue != value)
                        {
                            double dGammaValue = Math.Round(Math.Pow(10, value), 3);
                            _ImageInfo.GreenChannel.GammaValue = dGammaValue;
                            RaisePropertyChanged("GammaValue");
                        }
                    }
                    else if (_ImageInfo.SelectedChannel == ImageChannelType.Blue)
                    {
                        if (_ImageInfo.BlueChannel.GammaValue != value)
                        {
                            //_GammaValue = value;
                            double dGammaValue = Math.Round(Math.Pow(10, value), 3);
                            _ImageInfo.BlueChannel.GammaValue = dGammaValue;
                            RaisePropertyChanged("GammaValue");
                        }
                    }
                    else if (_ImageInfo.SelectedChannel == ImageChannelType.Mix)
                    {
                        if (_ImageInfo.MixChannel.GammaValue != value)
                        {
                            double dGammaValue = Math.Round(Math.Pow(10, value), 2);    // true gamma value
                            _ImageInfo.MixChannel.GammaValue = dGammaValue;
                            RaisePropertyChanged("GammaValue");
                        }
                    }
                }

            }

        }
        #endregion

        public bool IsManualContrast
        {
            get { return _IsManualContrast; }
            set
            {
                if (_IsManualContrast != value)
                {
                    _IsManualContrast = value;
                    RaisePropertyChanged("IsManualContrast");
                }
            }
        }

        #region public bool IsAutoContrast
        public bool IsAutoContrast
        {
            get
            {
                bool bIsAutoContrast = false;
                if (_Image != null && _ImageInfo != null)
                {
                    if (_ImageInfo.SelectedChannel == ImageChannelType.Red)
                    {
                        bIsAutoContrast = _ImageInfo.RedChannel.IsAutoChecked;
                    }
                    else if (_ImageInfo.SelectedChannel == ImageChannelType.Green)
                    {
                        bIsAutoContrast = _ImageInfo.GreenChannel.IsAutoChecked;
                    }
                    else if (_ImageInfo.SelectedChannel == ImageChannelType.Blue)
                    {
                        bIsAutoContrast = _ImageInfo.BlueChannel.IsAutoChecked;
                    }
                    else if (_ImageInfo.SelectedChannel == ImageChannelType.Mix)
                    {
                        bIsAutoContrast = _ImageInfo.MixChannel.IsAutoChecked;
                    }
                }
                
                return bIsAutoContrast;
            }
            set
            {
                if (_Image != null && _ImageInfo != null)
                {
                    if (_ImageInfo.SelectedChannel == ImageChannelType.Red)
                    {
                        if (_ImageInfo.RedChannel.IsAutoChecked != value)
                        {
                            _ImageInfo.RedChannel.IsAutoChecked = value;
                            RaisePropertyChanged("IsAutoContrast");

                            if (_ImageInfo.RedChannel.IsAutoChecked)
                            {
                                // Save previous Black, White, and Gamma values
                                _ImageInfo.RedChannel.PrevBlackValue = _ImageInfo.RedChannel.BlackValue;
                                _ImageInfo.RedChannel.PrevWhiteValue = _ImageInfo.RedChannel.WhiteValue;
                                _ImageInfo.RedChannel.PrevGammaValue = _ImageInfo.RedChannel.GammaValue;
                                IsManualContrast = false;
                            }
                            else
                            {
                                if (!IsManualContrast)
                                {
                                    // Restore previous Black, White, and Gamma values
                                    _ImageInfo.RedChannel.BlackValue = _ImageInfo.RedChannel.PrevBlackValue;
                                    _ImageInfo.RedChannel.WhiteValue = _ImageInfo.RedChannel.PrevWhiteValue;
                                    _ImageInfo.RedChannel.GammaValue = _ImageInfo.RedChannel.PrevGammaValue;
                                    RaisePropertyChanged("BlackValue");
                                    RaisePropertyChanged("WhiteValue");
                                    RaisePropertyChanged("GammaValue");
                                }
                            }
                        }
                    }
                    else if (_ImageInfo.SelectedChannel == ImageChannelType.Green)
                    {
                        if (_ImageInfo.GreenChannel.IsAutoChecked != value)
                        {
                            _ImageInfo.GreenChannel.IsAutoChecked = value;
                            RaisePropertyChanged("IsAutoContrast");

                            // Save previous Black, White, and Gamma values
                            if (_ImageInfo.GreenChannel.IsAutoChecked)
                            {
                                _ImageInfo.GreenChannel.PrevBlackValue = _ImageInfo.GreenChannel.BlackValue;
                                _ImageInfo.GreenChannel.PrevWhiteValue = _ImageInfo.GreenChannel.WhiteValue;
                                _ImageInfo.GreenChannel.PrevGammaValue = _ImageInfo.GreenChannel.GammaValue;
                                IsManualContrast = false;
                            }
                            else
                            {
                                if (!IsManualContrast)
                                {
                                    // Restore previous Black, White, and Gamma values
                                    _ImageInfo.GreenChannel.BlackValue = _ImageInfo.GreenChannel.PrevBlackValue;
                                    _ImageInfo.GreenChannel.WhiteValue = _ImageInfo.GreenChannel.PrevWhiteValue;
                                    _ImageInfo.GreenChannel.GammaValue = _ImageInfo.GreenChannel.PrevGammaValue;
                                    RaisePropertyChanged("BlackValue");
                                    RaisePropertyChanged("WhiteValue");
                                    RaisePropertyChanged("GammaValue");
                                }
                            }
                        }
                    }
                    else if (_ImageInfo.SelectedChannel == ImageChannelType.Blue)
                    {
                        if (_ImageInfo.BlueChannel.IsAutoChecked != value)
                        {
                            _ImageInfo.BlueChannel.IsAutoChecked = value;
                            RaisePropertyChanged("IsAutoContrast");

                            if (_ImageInfo.BlueChannel.IsAutoChecked)
                            {
                                // Save previous Black, White, and Gamma values
                                _ImageInfo.BlueChannel.PrevBlackValue = _ImageInfo.BlueChannel.BlackValue;
                                _ImageInfo.BlueChannel.PrevWhiteValue = _ImageInfo.BlueChannel.WhiteValue;
                                _ImageInfo.BlueChannel.PrevGammaValue = _ImageInfo.BlueChannel.GammaValue;
                                IsManualContrast = false;
                            }
                            else
                            {
                                if (!IsManualContrast)
                                {
                                    // Restore previous Black, White, and Gamma values
                                    _ImageInfo.BlueChannel.BlackValue = _ImageInfo.BlueChannel.PrevBlackValue;
                                    _ImageInfo.BlueChannel.WhiteValue = _ImageInfo.BlueChannel.PrevWhiteValue;
                                    _ImageInfo.BlueChannel.GammaValue = _ImageInfo.BlueChannel.PrevGammaValue;
                                    RaisePropertyChanged("BlackValue");
                                    RaisePropertyChanged("WhiteValue");
                                    RaisePropertyChanged("GammaValue");
                                }
                            }
                        }
                    }
                    else if (_ImageInfo.SelectedChannel == ImageChannelType.Mix)
                    {
                        if (_ImageInfo.MixChannel.IsAutoChecked != value)
                        {
                            _ImageInfo.MixChannel.IsAutoChecked = value;
                            RaisePropertyChanged("IsAutoContrast");

                            if (_ImageInfo.MixChannel.IsAutoChecked)
                            {
                                // Save previous Black, White, and Gamma values
                                _ImageInfo.MixChannel.PrevBlackValue = _ImageInfo.MixChannel.BlackValue;
                                _ImageInfo.MixChannel.PrevWhiteValue = _ImageInfo.MixChannel.WhiteValue;
                                _ImageInfo.MixChannel.PrevGammaValue = _ImageInfo.MixChannel.GammaValue;
                                IsManualContrast = false;
                            }
                            else
                            {
                                if (!IsManualContrast)
                                {
                                    // Restore previous Black, White, and Gamma values
                                    _ImageInfo.MixChannel.BlackValue = _ImageInfo.MixChannel.PrevBlackValue;
                                    _ImageInfo.MixChannel.WhiteValue = _ImageInfo.MixChannel.PrevWhiteValue;
                                    _ImageInfo.MixChannel.GammaValue = _ImageInfo.MixChannel.PrevGammaValue;
                                    RaisePropertyChanged("BlackValue");
                                    RaisePropertyChanged("WhiteValue");
                                    RaisePropertyChanged("GammaValue");
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region public bool IsInvert
        public bool IsInvert
        {
            get
            {
                bool bIsInvert = false;
                if (_Image != null && _ImageInfo != null)
                {
                    if (_ImageInfo.SelectedChannel == ImageChannelType.Red)
                    {
                        bIsInvert = _ImageInfo.RedChannel.IsInvertChecked;
                    }
                    else if (_ImageInfo.SelectedChannel == ImageChannelType.Green)
                    {
                        bIsInvert = _ImageInfo.GreenChannel.IsInvertChecked;
                    }
                    else if (_ImageInfo.SelectedChannel == ImageChannelType.Blue)
                    {
                        bIsInvert = _ImageInfo.BlueChannel.IsInvertChecked;
                    }
                    else if (_ImageInfo.SelectedChannel == ImageChannelType.Mix)
                    {
                        bIsInvert = _ImageInfo.MixChannel.IsInvertChecked;
                    }
                }

                return bIsInvert;
            }   // get
            set
            {
                if (_Image != null && _ImageInfo != null)
                {
                    if (_ImageInfo.SelectedChannel == ImageChannelType.Red)
                    {
                        if (_ImageInfo.RedChannel.IsInvertChecked != value)
                        {
                            //_IsInvertChecked = value;
                            _ImageInfo.RedChannel.IsInvertChecked = value;
                            RaisePropertyChanged("IsInvert");
                        }
                    }
                    else if (_ImageInfo.SelectedChannel == ImageChannelType.Green)
                    {
                        if (_ImageInfo.GreenChannel.IsInvertChecked != value)
                        {
                            //_IsInvertChecked = value;
                            _ImageInfo.GreenChannel.IsInvertChecked = value;
                            RaisePropertyChanged("IsInvert");
                        }
                    }
                    else if (_ImageInfo.SelectedChannel == ImageChannelType.Blue)
                    {
                        if (_ImageInfo.BlueChannel.IsInvertChecked != value)
                        {
                            //_IsInvertChecked = value;
                            _ImageInfo.BlueChannel.IsInvertChecked = value;
                            RaisePropertyChanged("IsInvert");
                        }
                    }
                    else if (_ImageInfo.SelectedChannel == ImageChannelType.Mix)
                    {
                        if (_ImageInfo.MixChannel.IsInvertChecked != value)
                        {
                            //_IsInvertChecked = value;
                            _ImageInfo.MixChannel.IsInvertChecked = value;
                            RaisePropertyChanged("IsInvert");
                        }
                    }
                }

            }   // set
        }
        #endregion

        #region public bool IsSaturation
        public bool IsSaturation
        {
            get
            {
                bool bIsSaturation = false;
                if (_Image != null && _ImageInfo != null)
                {
                    if (_ImageInfo.SelectedChannel == ImageChannelType.Red)
                    {
                        bIsSaturation = _ImageInfo.RedChannel.IsSaturationChecked;
                    }
                    else if (_ImageInfo.SelectedChannel == ImageChannelType.Green)
                    {
                        bIsSaturation = _ImageInfo.GreenChannel.IsSaturationChecked;
                    }
                    else if (_ImageInfo.SelectedChannel == ImageChannelType.Blue)
                    {
                        bIsSaturation = _ImageInfo.BlueChannel.IsSaturationChecked;
                    }
                    else if (_ImageInfo.SelectedChannel == ImageChannelType.Mix)
                    {
                        bIsSaturation = _ImageInfo.MixChannel.IsSaturationChecked;
                    }
                }
                return bIsSaturation;
            }   // get
            set
            {
                if (_Image != null && _ImageInfo != null)
                {
                    if (_ImageInfo.SelectedChannel == ImageChannelType.Red)
                    {
                        if (_ImageInfo.RedChannel.IsSaturationChecked != value)
                        {
                            _ImageInfo.RedChannel.IsSaturationChecked = value;
                            RaisePropertyChanged("IsSaturation");
                        }
                    }
                    else if (_ImageInfo.SelectedChannel == ImageChannelType.Green)
                    {
                        if (_ImageInfo.GreenChannel.IsSaturationChecked != value)
                        {
                            _ImageInfo.GreenChannel.IsSaturationChecked = value;
                            RaisePropertyChanged("IsSaturation");
                        }
                    }
                    else if (_ImageInfo.SelectedChannel == ImageChannelType.Blue)
                    {
                        if (_ImageInfo.BlueChannel.IsSaturationChecked != value)
                        {
                            _ImageInfo.BlueChannel.IsSaturationChecked = value;
                            RaisePropertyChanged("IsSaturation");
                        }
                    }
                    else if (_ImageInfo.SelectedChannel == ImageChannelType.Mix)
                    {
                        if (_ImageInfo.MixChannel.IsSaturationChecked != value)
                        {
                            _ImageInfo.MixChannel.IsSaturationChecked = value;
                            RaisePropertyChanged("IsSaturation");
                        }
                    }
                }
            }   // set

        }
        #endregion


        public bool IsEditComment
        {
            get { return _IsEditComment; }
            set
            {
                if (_IsEditComment != value)
                {
                    _IsEditComment = value;
                    RaisePropertyChanged("IsEditComment");

                    if (_IsEditComment)
                    {
                        // bring up the onscreen keyboard
                        Workspace.This.ShowOnscreenKeyboard();
                    }
                    else
                    {
                        // hide the onscreen keyboard
                        Workspace.This.HideOnscreenKeyboard();
                    }
                }
            }
        }

        public string ImageInfoComment
        {
            get
            {
                string comment = string.Empty;
                if (ImageInfo != null)
                {
                    comment = ImageInfo.Comment;
                }
                return comment;
            }
            set
            {
                if (ImageInfo != null)
                {
                    if (ImageInfo.Comment != value)
                    {
                        ImageInfo.Comment = value;
                        RaisePropertyChanged("ImageInfoComment");
                        this.IsDirty = true;
                    }
                }
            }
        }

        public int ImageInfoDynamicBit
        {
            get
            {
                int dynamicBit =0;
                if (ImageInfo != null)
                {
                    dynamicBit = ImageInfo.DynamicBit;
                }
                return dynamicBit;
            }
            set
            {
                if (ImageInfo != null)
                {
                    if (ImageInfo.DynamicBit != value)
                    {
                        ImageInfo.DynamicBit = value;
                        RaisePropertyChanged("ImageInfoDynamicBit");
                        this.IsDirty = true;
                    }
                }
            }
        }

        #region EditCommentCommand
        private RelayCommand _EditCommentCommand = null;
        public ICommand EditCommentCommand
        {
            get
            {
                if (_EditCommentCommand == null)
                {
                    _EditCommentCommand = new RelayCommand(ExecuteEditCommentCommand, CanExecuteEditCommentCommand);
                }

                return _EditCommentCommand;
            }
        }

        protected void ExecuteEditCommentCommand(object parameter)
        {
            IsEditComment = !IsEditComment;
        }

        protected bool CanExecuteEditCommentCommand(object parameter)
        {
            return (!IsEditComment);
        }

        #endregion

        public void UpdateDisplayImage()
        {
            try
            {
                _DisplayImage.Lock();
                ImageProcessingHelper.UpdateDisplayImage(ref _Image, _ImageInfo, ref _DisplayImage);
                if (_DisplayImage.CanFreeze)
                {
                    Workspace.This.StartWaitAnimation("Loading...");
                    //_DisplayImage.Freeze();
                    _DisplayImage.Lock();
                    _DisplayImage.AddDirtyRect(new Int32Rect(0, 0, Width, Height));
                    _DisplayImage.Unlock();
                    if (IsAutoContrast)
                    {
                        RaisePropertyChanged("BlackValue");
                        RaisePropertyChanged("WhiteValue");
                        RaisePropertyChanged("GammaValue");
                    }
                    RaisePropertyChanged("DisplayImage");
                    Workspace.ClearMemory();
                    Workspace.This.StopWaitAnimation();
                    return;
                }
                _DisplayImage.AddDirtyRect(new Int32Rect(0, 0, Width, Height));
                _DisplayImage.Unlock();

                // Trigger value changed.
                if (IsAutoContrast)
                {
                    RaisePropertyChanged("BlackValue");
                    RaisePropertyChanged("WhiteValue");
                    RaisePropertyChanged("GammaValue");
                }
           
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        ///Rotating Reserves the back buffer for updates.
        /// </summary>
        public void Rotating_UpdateDisplayImage()
        {
            try
            {
                _DisplayImage.Lock();
                if (_DisplayImage != null)
                if (_DisplayImage.CanFreeze) { _DisplayImage.Freeze(); }
                BitmapPalette palette = null;
                palette = new BitmapPalette(ImageProcessing.GetColorTableIndexed(false));
                PixelFormat dstPixelFormat = PixelFormats.Rgb24;
                dstPixelFormat = PixelFormats.Indexed8;
                _DisplayImage = new WriteableBitmap(Width, Height, 96, 96, dstPixelFormat, palette);
                ImageProcessingHelper.UpdateDisplayImage(ref _Image, _ImageInfo, ref _DisplayImage);
                _DisplayImage.Lock();
                _DisplayImage.AddDirtyRect(new Int32Rect(0, 0, Width, Height));
                _DisplayImage.Unlock();
                // Trigger value changed.
                if (IsAutoContrast)
                {
                    RaisePropertyChanged("BlackValue");
                    RaisePropertyChanged("WhiteValue");
                    RaisePropertyChanged("GammaValue");
                }
                RaisePropertyChanged("DisplayImage");
                Workspace.ClearMemory();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public void UpdatePixelMoveDisplayImage(ref WriteableBitmap img,int xMove1,int XMove2, int yMove1, int yMove2)
        {
            try
            {
                ImageProcessingHelper.UpdatePixelMoveDisplayImage(ref img, xMove1, XMove2, yMove1, yMove2);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void UpdatePixelMoveDisplayImage(int xMove1, int XMove2, int yMove1, int yMove2)
        {
            try
            {
                Workspace.This._tempSrcImg = _Image;
                ImageProcessingHelper.UpdatePixelMoveDisplayImage(ref _Image, xMove1, XMove2, yMove1, yMove2);
                UpdateDisplayImage();
                _Image = Workspace.This._tempSrcImg;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static void UpdatePixelSingleMoveDisplayImage(ref WriteableBitmap img, int xMove1, int XMove2, int yMove1, int yMove2)
        {
            try
            {
                ImageProcessingHelper.UpdatePixelSingleMoveDisplayImage(ref img, xMove1, XMove2, yMove1, yMove2);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static void ImageToPngSave(WriteableBitmap img,string Path)
        {
            try
            {
                ImageProcessingHelper.ImageToPngSave(img,Path);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public static void CreateGif(string SavePath, List<string> imageList)
        {
            try
            {
                ImageProcessingHelper.CreateGif(SavePath, imageList);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static void UpdatePixeleffectiveMoveDisplayImage(ref WriteableBitmap img, int pixeloffset)
        {
            try
            {
                ImageProcessingHelper.UpdatePixeleffectiveMoveDisplayImage(ref img, pixeloffset);
            }
            catch (Exception)
            {
                throw;
            }
        }
        
        public static void YAxisUpdatePixeleffectiveMoveDisplayImage(ref WriteableBitmap img, int pixeloffset)
        {
            try
            {
                ImageProcessingHelper.YAxisUpdatePixeleffectiveMoveDisplayImage(ref img, pixeloffset);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static void CoupletAverageProcessing(ref WriteableBitmap img)
        {
            try
            {
                ImageProcessingHelper.CoupletAverageProcessing(ref img);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void UpdateDisplayImage(int nColorGradation, bool bIsMergeChannels)
        {
            try
            {
                _DisplayImage.Lock();
                if (nColorGradation > 3)
                    ImageProcessing.Scale_16u8u_C4C3(ref _Image, ref _DisplayImage, _ImageInfo, nColorGradation, bIsMergeChannels);
                else
                    ImageProcessing.Scale_16u8u_C3C3(ref _Image, ref _DisplayImage, _ImageInfo, nColorGradation);
                _DisplayImage.AddDirtyRect(new Int32Rect(0, 0, Width, Height));
                _DisplayImage.Unlock();

                // Trigger value changed.
                if (IsAutoContrast)
                {
                    RaisePropertyChanged("BlackValue");
                    RaisePropertyChanged("WhiteValue");
                    RaisePropertyChanged("GammaValue");
                }
            }
            catch (Exception)
            {
                throw;
            }
        }


        /*#region public int PrevBlackValue

        public int PrevBlackValue
        {
            get
            {
                int iBlackValue = 0;
                if (_Image != null && _ImageInfo != null)
                {
                    if (_ImageInfo.SelectedChannel == ImageChannelType.ChannelR)
                    {
                        iBlackValue = _ImageInfo.PrevRchBlackVal;
                    }
                    else if (_ImageInfo.SelectedChannel == ImageChannelType.ChannelG)
                    {
                        iBlackValue = _ImageInfo.PrevGchBlackVal;
                    }
                    else if (_ImageInfo.SelectedChannel == ImageChannelType.ChannelB)
                    {
                        iBlackValue = _ImageInfo.PrevBchBlackVal;
                    }
                    else if (_ImageInfo.SelectedChannel == ImageChannelType.ChannelMix)
                    {
                        iBlackValue = _ImageInfo.PrevMixBlackVal;
                    }
                }
                return iBlackValue;
            }

            set
            {
                if (_Image != null && _ImageInfo != null)
                {
                    if (_ImageInfo.SelectedChannel == ImageChannelType.ChannelR)
                    {
                        _ImageInfo.PrevRchBlackVal = value;
                    }
                    else if (_ImageInfo.SelectedChannel == ImageChannelType.ChannelG)
                    {
                        _ImageInfo.PrevGchBlackVal = value;
                    }
                    else if (_ImageInfo.SelectedChannel == ImageChannelType.ChannelB)
                    {
                        _ImageInfo.PrevBchBlackVal = value;
                    }
                    else if (_ImageInfo.SelectedChannel == ImageChannelType.ChannelMix)
                    {
                        _ImageInfo.PrevMixBlackVal = value;
                    }
                }
            }

        }
        
        #endregion*/

        /*#region public int PrevWhiteValue

        public int PrevWhiteValue
        {
            get
            {
                int iWhiteValue = 0;
                if (_Image != null && _ImageInfo != null)
                {
                    if (_ImageInfo.SelectedChannel == ImageChannelType.ChannelR)
                    {
                        iWhiteValue = _ImageInfo.PrevRchWhiteVal;
                    }
                    else if (_ImageInfo.SelectedChannel == ImageChannelType.ChannelG)
                    {
                        iWhiteValue = _ImageInfo.PrevGchWhiteVal;
                    }
                    else if (_ImageInfo.SelectedChannel == ImageChannelType.ChannelB)
                    {
                        iWhiteValue = _ImageInfo.PrevBchWhiteVal;
                    }
                    else if (_ImageInfo.SelectedChannel == ImageChannelType.ChannelMix)
                    {
                        iWhiteValue = _ImageInfo.PrevMixWhiteVal;
                    }
                }
                return iWhiteValue;
            }

            set
            {
                if (_Image != null && _ImageInfo != null)
                {
                    if (_ImageInfo.SelectedChannel == ImageChannelType.ChannelR)
                    {
                        _ImageInfo.PrevRchWhiteVal = value;
                    }
                    else if (_ImageInfo.SelectedChannel == ImageChannelType.ChannelG)
                    {
                        _ImageInfo.PrevGchWhiteVal = value;
                    }
                    else if (_ImageInfo.SelectedChannel == ImageChannelType.ChannelB)
                    {
                        _ImageInfo.PrevBchWhiteVal = value;
                    }
                    else if (_ImageInfo.SelectedChannel == ImageChannelType.ChannelMix)
                    {
                        _ImageInfo.PrevMixWhiteVal = value;
                    }
                }
            }

        }

        #endregion*/

        /*#region public int PrevGammaValue

        public double PrevGammaValue
        {
            get
            {
                double dGammaValue = 0;
                if (_Image != null && _ImageInfo != null)
                {
                    if (_ImageInfo.SelectedChannel == ImageChannelType.ChannelR)
                    {
                        dGammaValue = _ImageInfo.PrevRchGammaVal;
                    }
                    else if (_ImageInfo.SelectedChannel == ImageChannelType.ChannelG)
                    {
                        dGammaValue = _ImageInfo.PrevGchGammaVal;
                    }
                    else if (_ImageInfo.SelectedChannel == ImageChannelType.ChannelB)
                    {
                        dGammaValue = _ImageInfo.PrevBchGammaVal;
                    }
                    else if (_ImageInfo.SelectedChannel == ImageChannelType.ChannelMix)
                    {
                        dGammaValue = _ImageInfo.PrevMixGammaVal;
                    }
                }
                return dGammaValue;
            }

            set
            {
                if (_Image != null && _ImageInfo != null)
                {
                    if (_ImageInfo.SelectedChannel == ImageChannelType.ChannelR)
                    {
                        _ImageInfo.PrevRchGammaVal = value;
                    }
                    else if (_ImageInfo.SelectedChannel == ImageChannelType.ChannelG)
                    {
                        _ImageInfo.PrevGchGammaVal = value;
                    }
                    else if (_ImageInfo.SelectedChannel == ImageChannelType.ChannelB)
                    {
                        _ImageInfo.PrevBchGammaVal = value;
                    }
                    else if (_ImageInfo.SelectedChannel == ImageChannelType.ChannelMix)
                    {
                        _ImageInfo.PrevMixGammaVal = value;
                    }
                }
            }

        }

        #endregion*/

    }
}
