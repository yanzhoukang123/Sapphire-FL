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
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Windows.Input;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media;
//using System.Windows.Controls;    //Image
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
//using Azure.Controller;
using Azure.Image.Processing;
using Azure.WPF.Framework;  //RelayCommand
using DrawToolsLib;
using Azure.Ipp.Imaging;
using Azure.Configuration.Settings;
using Azure.Common;

namespace Azure.LaserScanner.ViewModel
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
        // Flag: Has Dispose already been called?
        bool disposed = false;
        // Instantiate a SafeHandle instance.
        SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);

        // Protected implementation of Dispose pattern.
        protected override void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                handle.Dispose();
                // Free any other managed objects here.
                //

                if (_Image != null)
                {
                    try
                    {
                        // In extreme situations force a garbage collection to free 
                        // up memory as quickly as possible.
                        if (_Image != null &&
                            _Image.PixelHeight * _Image.PixelWidth > (10000 * 10000))
                        {
                            _Image = null;
                            _DisplayImage = null;
                            // Forces an immediate garbage collection.
                            //GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                            //GC.WaitForPendingFinalizers();
                            //GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                            // Force garbage collection.
                            GC.Collect();
                            // Wait for all finalizers to complete before continuing.
                            GC.WaitForPendingFinalizers();
                            GC.Collect();
                        }
                        else
                        {
                            _Image = null;
                            _DisplayImage = null;
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            // Free any unmanaged objects here.
            //

            disposed = true;
            // Call base class implementation.
            base.Dispose(disposing);
            GC.SuppressFinalize(this);
        }

        //public delegate void ZoomUpdateDelegate(ZoomType zoomType);
        //public event ZoomUpdateDelegate ZoomUpdateEvent;
        //public delegate void CropAdornerDelegate(bool bIsVisible);
        //public event CropAdornerDelegate CropAdornerEvent;
        //public delegate void CropAdornerRectDelegate();
        //public event CropAdornerRectDelegate CropAdornerRectEvent;

        public delegate void RegionAdornerDelegate(bool bIsVisible);
        public event RegionAdornerDelegate RegionAdornerChanged;
        public delegate void ImageResizedDelegate(object sender, double percent);
        public event ImageResizedDelegate ImageSizeChanged;

        #region Private data....

        string _FileID = string.Empty;
        //static ImageSourceConverter ISC = new ImageSourceConverter();
        private double _ZoomLevel = 1.0;
        //private double _MinimumZoom = 1.0;
        //private double _MaximumZoom = 10.0;
        private string _PixelX = string.Empty;
        private string _PixelY = string.Empty;
        private string _PixelIntensity = string.Empty;

        private WriteableBitmap _Image = null;
        private WriteableBitmap _DisplayImage = null;
        private ImageInfo _ImageInfo = null;
        private bool _IsShowImageInfo = false;
        //private ImageViewer _ImageViewer = null;
        private DrawingCanvas _DrawingCanvas = null;
        private ToolType _SelectedDrawingTool = ToolType.None;

        private const int _SaturationThreshold = 62000;

        //private const int MaxPixelValue = 65535;
        //private int _MaxWhiteValue = 65535;
        //private int _LargeChange = 10;
        //private int _SmallChange = 1;
        //private int _WhiteValue = 65535;
        //private int _BlackValue = 0;
        //private double _GammaValue = 0.0;
        //private ImageChannelType _SelectedChannelType = ImageChannelType.Mix;

        //private bool _IsRedChannelChecked = false;
        //private bool _IsGreenChannelChecked = false;
        //private bool _IsBlueChannelChecked = false;
        //private bool _IsGrayChannelChecked = false;
        //private bool _IsMixChannelsChecked = true;
        //private ImageChannelType _SelectedColorChannel = ImageChannelType.None;

        //private DrawingMode _DrawingMode = DrawingMode.Select;

        //private bool _IsCropping = false;
        //private string _CropX;
        //private string _CropY;
        //private string _CropWidth;
        //private string _CropHeight;

        // Use to distinguish between newly created image, and a modified image
        private DirtyType _DocDirtyType = DirtyType.None;
        private bool _IsImageChannelChanged = false;

        //private Rect _CropRect;
        //private bool _IsTriggerGetCropRect = false;

        private ZoomType _ZoomingType = ZoomType.ZoomFit;
        //private bool _IsRgbImageCropped = false;    // Work-around for RGB image cropping crash
        private bool _IsManualContrast = false;
        private bool _IsEditComment = false;
        private bool _IsContrasting = false;

        // manual contrast
        //private ChannelSelectViewModel _RedChannelContrastVm = null;
        //private ChannelSelectViewModel _GreenChannelContrastVm = null;
        //private ChannelSelectViewModel _BlueChannelContrastVm = null;
        //private ChannelSelectViewModel _GrayChannelContrastVm = null;

        #endregion

        #region Constructors...

        public FileViewModel()
        {
            FileID = GenerateID();
            Title = FileName;
            IsDirty = true;

            //_RedChannelContrastVm = new ChannelSelectViewModel(this, ImageChannelType.Red);
            //_GreenChannelContrastVm = new ChannelSelectViewModel(this, ImageChannelType.Green);
            //_BlueChannelContrastVm = new ChannelSelectViewModel(this, ImageChannelType.Blue);
            //_GrayChannelContrastVm = new ChannelSelectViewModel(this, ImageChannelType.Gray);
        }

        public FileViewModel(string filePath)
        {
            FileID = GenerateID();
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
                //imageContent = ImageProcessing.Load(filePath);
                imageContent = ImageProcessing.LoadImage(filePath);
                bool bIsPixelInverted = false;

                if (imageContent != null)
                {
                    try
                    {
                        // get image info from the comments section of the image metadata
                        _ImageInfo = ImageProcessing.ReadMetadata(filePath);

                        #region === Backward compatibility ===
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
                            // For compatibility with other software version (or capture software)
                            if (string.IsNullOrEmpty(_ImageInfo.Software))
                            {
                                _ImageInfo.Software = GetSoftware(filePath);
                                if (!string.IsNullOrEmpty(_ImageInfo.Software))
                                {
                                    // chemiSOLO's image stored its capture info in the GrayChannel (and the Azure Imager's and the SOG stores it in the MixChannel).
                                    if (_ImageInfo.Software.ToLower() == "chemisolo")
                                    {
                                        _ImageInfo.MixChannel = (ImageChannel)_ImageInfo.GrayChannel.Clone();
                                    }
                                }
                            }

                            if (extension.Equals(".tif") || extension.Equals(".tiff"))
                                _ImageInfo.PhotometricInterpretation = GetPhotometricInterpretation(filePath);

                            if (_ImageInfo.Scalebar == null) { _ImageInfo.Scalebar = new ScaleBar(); }

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
                        if ((extension.Equals(".tif") || extension.Equals(".tiff")) && imageContent.Format.BitsPerPixel == 16)
                        {
                            if (bIsPixelInverted || (_ImageInfo.IsChemiImage && _ImageInfo.IsPixelInverted) || _ImageInfo.PhotometricInterpretation == 0)
                            {
                                imageContent = ImageProcessing.Invert(imageContent);

                                // chemiSOLO's TIFF image set PhotometricInterpretation to 0 (WhiteIsZero)
                                //   and it also store its capture info in the GrayChannel (and the Azure Imager's and the SOG stores it in the MixChannel).
                                if (_ImageInfo.PhotometricInterpretation == 0 ||
                                    (!string.IsNullOrEmpty(_ImageInfo.Software) && _ImageInfo.Software.ToLower() == "chemisolo"))
                                {
                                    _ImageInfo.MixChannel = (ImageChannel)_ImageInfo.GrayChannel.Clone();
                                }

                                _ImageInfo.MixChannel.IsInvertChecked = true;
                                _ImageInfo.IsPixelInverted = false;
                            }
                        }
                    }
                    else
                    {
                        _ImageInfo = new ImageInfo();
                    }

                    if (imageContent.CanFreeze) { imageContent.Freeze(); }

                    Image = imageContent;

                    // Set max pixel value
                    int bpp = _Image.Format.BitsPerPixel;
                    nWidth = _Image.PixelWidth;
                    nHeight = _Image.PixelHeight;
                    int maxPixelValue = 65535;
                    int dynamicBpp = _ImageInfo.DynamicBit;
                    if (dynamicBpp > 16)
                    {
                        //maxPixelValue = (1 << _ImageInfo.DynamicBit) - 1;
                        //maxPixelValue = (int)Math.Pow(2, _ImageInfo.DynamicBit) - 1;
                        // EDR image set display range from 0 to MAX
                        maxPixelValue = ImageProcessing.Max(Image, new Rect(0, 0, nWidth, nHeight));
                        double cnvFactor = dynamicBpp / 16.0;
                        maxPixelValue = (int)Math.Pow(maxPixelValue, cnvFactor);
                        int actualBitDepth = _ImageInfo.EdrBitDepth > 0 ? _ImageInfo.EdrBitDepth : _ImageInfo.DynamicBit;
                        int actualMaxPixelValue = (int)Math.Pow(2, actualBitDepth) - 1;
                        _ImageInfo.SaturationThreshold = (int)(actualMaxPixelValue * ((double)_SaturationThreshold / 65535.0));
                    }
                    else
                    {
                        maxPixelValue = (bpp == 16 || bpp == 48 || bpp == 64) ? 65535 : 255;
                        if (maxPixelValue > 255)
                            _ImageInfo.SaturationThreshold = (int)(maxPixelValue * ((double)_SaturationThreshold / 65535.0));
                        else
                            _ImageInfo.SaturationThreshold = 254;
                    }

                    MaxPixelValue = maxPixelValue;
                    MaxWhiteValue = maxPixelValue;

                    _ImageInfo.MixChannel.WhiteValue = maxPixelValue;
                    _ImageInfo.RedChannel.WhiteValue = maxPixelValue;
                    _ImageInfo.GreenChannel.WhiteValue = maxPixelValue;
                    _ImageInfo.BlueChannel.WhiteValue = maxPixelValue;
                    _ImageInfo.GrayChannel.WhiteValue = maxPixelValue;
                    _ImageInfo.MaxPixelValue = maxPixelValue;

                    // Default to mix/overall (grayscale image default type is Mix)
                    _ImageInfo.SelectedChannel = ImageChannelType.Mix;
                    // Check 'Display Overall' (or 'All Channels') by default
                    _ImageInfo.IsDisplayOverAll = true;

                    BitmapPalette palette = null;
                    PixelFormat dstPixelFormat = PixelFormats.Rgb24;
                    PixelFormatType pixelFormatType = IppImaging.GetPixelFormatType(_Image.Format);

                    if (pixelFormatType == PixelFormatType.P8u_C1 ||
                        pixelFormatType == PixelFormatType.P16u_C1 ||
                        pixelFormatType == PixelFormatType.P32f_C1)
                    {
                        _ImageInfo.NumOfChannels = 1;
                        _ImageInfo.DisplayChannelFlags = ImageChannelFlag.None;
                        _ImageInfo.ContrastChannelFlags = ImageChannelFlag.None;
                        // Work-around: very large image > 20000 x 20000 pixels use 8-bit indexed display image
                        //              Windows has issue displaying image > 20000 x 20000 pixels non-Indexed display image.
                        if (nWidth * nHeight > (20000 * 20000))
                        {
                            // Create 8-bit indexed display image buffer.
                            bool bIsSaturation = _ImageInfo.IsSaturationChecked;
                            dstPixelFormat = PixelFormats.Indexed8;
                            palette = new BitmapPalette(ImageProcessing.GetColorTableIndexed(bIsSaturation));
                            _DisplayImage = new WriteableBitmap(nWidth, nHeight, _Image.DpiX, _Image.DpiY, dstPixelFormat, palette);
                        }
                        else
                        {
                            // Create 24-bit display image buffer.
                            _DisplayImage = new WriteableBitmap(nWidth, nHeight, _Image.DpiX, _Image.DpiY, dstPixelFormat, palette);
                        }

                        // chemiSOLO compatibility, chemiSOLO single channel grayscale save its channel info in GrayChannel
                        // SFL saves it in MixChannel
                        if (_ImageInfo.MixChannel.Exposure == 0 && _ImageInfo.GrayChannel.Exposure != 0)
                        {
                            _ImageInfo.MixChannel = (ImageChannel)_ImageInfo.GrayChannel.Clone();
                        }
                    }
                    else if (pixelFormatType == PixelFormatType.P8u_C3 || pixelFormatType == PixelFormatType.P8u_C4 ||
                             pixelFormatType == PixelFormatType.P16u_C3 || pixelFormatType == PixelFormatType.P16u_C4)
                    {
                        // Create 24-bit display image buffer.
                        dstPixelFormat = PixelFormats.Rgb24;
                        palette = _Image.Palette;
                        _DisplayImage = new WriteableBitmap(nWidth, nHeight, _Image.DpiX, _Image.DpiY, dstPixelFormat, palette);
                        if (pixelFormatType == PixelFormatType.P8u_C4 ||
                            pixelFormatType == PixelFormatType.P16u_C4)
                        {
                            _ImageInfo.NumOfChannels = 4;
                            // Check all constrast and display channels
                            _ImageInfo.DisplayChannelFlags |= ImageChannelFlag.Red | ImageChannelFlag.Green | ImageChannelFlag.Blue | ImageChannelFlag.Gray;
                            _ImageInfo.ContrastChannelFlags |= ImageChannelFlag.Red | ImageChannelFlag.Green | ImageChannelFlag.Blue | ImageChannelFlag.Gray;
                            if (!_ImageInfo.IsRedChannelAvail && !_ImageInfo.IsGreenChannelAvail && !_ImageInfo.IsBlueChannelAvail && !_ImageInfo.IsGrayChannelAvail)
                            {
                                _ImageInfo.IsRedChannelAvail = true;
                                _ImageInfo.IsGreenChannelAvail = true;
                                _ImageInfo.IsBlueChannelAvail = true;
                                _ImageInfo.IsGrayChannelAvail = true;
                                if (_ImageInfo.SelectedChannel == ImageChannelType.Mix && _ImageInfo.MixChannel.IsAutoChecked)
                                {
                                    _ImageInfo.RedChannel.IsAutoChecked = true;
                                    _ImageInfo.GreenChannel.IsAutoChecked = true;
                                    _ImageInfo.BlueChannel.IsAutoChecked = true;
                                    _ImageInfo.GrayChannel.IsAutoChecked = true;
                                }
                            }
                            _ImageInfo.IsGrayChannelAvail = true;   // assume the gray channel exists/available on a 4-channel image.
                        }
                        else
                        {
                            _ImageInfo.NumOfChannels = 3;
                            // Check all constrast and display channels
                            _ImageInfo.DisplayChannelFlags |= ImageChannelFlag.Red | ImageChannelFlag.Green | ImageChannelFlag.Blue;
                            _ImageInfo.ContrastChannelFlags |= ImageChannelFlag.Red | ImageChannelFlag.Green | ImageChannelFlag.Blue;
                            if (!_ImageInfo.IsRedChannelAvail && !_ImageInfo.IsGreenChannelAvail && !_ImageInfo.IsBlueChannelAvail)
                            {
                                _ImageInfo.IsRedChannelAvail = true;
                                _ImageInfo.IsGreenChannelAvail = true;
                                _ImageInfo.IsBlueChannelAvail = true;
                                if (_ImageInfo.SelectedChannel == ImageChannelType.Mix && _ImageInfo.MixChannel.IsAutoChecked)
                                {
                                    _ImageInfo.RedChannel.IsAutoChecked = true;
                                    _ImageInfo.GreenChannel.IsAutoChecked = true;
                                    _ImageInfo.BlueChannel.IsAutoChecked = true;
                                }
                            }
                        }
                    }

                    DateTime modifiedDate = File.GetLastWriteTime(@filePath);
                    _ImageInfo.ModifiedDate = System.String.Format("{0:G}", modifiedDate.ToString());

                    // Set saturation threshold
                    //_ImageInfo.SaturationThreshold = (int)(maxPixelValue * ((double)_SaturationThreshold / 65535.0));

                    if (bpp == 24 || bpp == 32)
                    {
                        _ImageInfo.IsGrayChannelAvail = false;
                    }

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
            FileID = GenerateID();
            FilePath = null;
            Title = newImageTitle;

            this.Image = newImage;
            this.ImageInfo = newImageInfo;
            int bpp = newImage.Format.BitsPerPixel;

            int maxPixelValue = 65535;
            if (newImageInfo.DynamicBit > 16)
            {
                //maxPixelValue = (1 << _ImageInfo.ActualBitDepth) - 1;
                //maxPixelValue = (int)Math.Pow(2, _ImageInfo.DynamicBit) - 1;
                //ImageInfo.SaturationThreshold = (int)(maxPixelValue * ((double)_SaturationThreshold / 65535.0));
                maxPixelValue = ImageProcessing.Max(newImage, new Rect(0, 0, newImage.PixelWidth, newImage.PixelHeight));
                double cnvFactor = newImageInfo.DynamicBit / 16.0;
                maxPixelValue = (int)Math.Pow(maxPixelValue, cnvFactor);
                int actualBitDepth = _ImageInfo.EdrBitDepth > 0 ? _ImageInfo.EdrBitDepth : _ImageInfo.DynamicBit;
                int actualMaxPixelValue = (int)Math.Pow(2, actualBitDepth) - 1;
                _ImageInfo.SaturationThreshold = (int)(actualMaxPixelValue * ((double)_SaturationThreshold / 65535.0));
            }
            else
            {
                maxPixelValue = (bpp == 16 || bpp == 48 || bpp == 64) ? 65535 : 255;
                if (maxPixelValue > 255)
                    _ImageInfo.SaturationThreshold = (int)(maxPixelValue * ((double)_SaturationThreshold / 65535.0));
                else
                    _ImageInfo.SaturationThreshold = 254;
            }

            MaxPixelValue = maxPixelValue;
            MaxWhiteValue = maxPixelValue;
            _ImageInfo.MaxPixelValue = maxPixelValue;

            BitmapPalette palette = _Image.Palette;
            PixelFormat dstPixelFormat = PixelFormats.Rgb24;
            _ImageInfo.NumOfChannels = 1;
            var dpiX = _Image.DpiX;
            var dpiY = _Image.DpiY;
            _DisplayImage = new WriteableBitmap(Width, Height, dpiX, dpiY, dstPixelFormat, palette);

            // Should we set contrast value to min and max or autocontrast?
            if (bpp == 16 || bpp == 48 || bpp == 64)
            {
                // Chemi cumulative: use the same initial black and white value.
                if (bIsGetMinMax)
                {
                    uint minValue = 0;
                    uint maxValue = 0;
                    uint[] minArray = null;
                    uint[] maxArray = null;
                    Rect roiRect = new Rect(0, 0, this.Image.PixelWidth, this.Image.PixelHeight);
                    if (bpp == 16)
                    {
                        ImageProcessing.MinMax(this.Image, roiRect, ref minValue, ref maxValue);
                    }
                    else
                    {
                        ImageProcessing.MinMaxMC(this.Image, roiRect, ref minValue, ref maxValue, ref minArray, ref maxArray);
                    }
                    if (newImageInfo.DynamicBit > 16)
                    {
                        double cnvFactor = newImageInfo.DynamicBit / 16.0;
                        int tmpMinValue = (int)Math.Pow(minValue, cnvFactor);
                        int tmpMaxValue = (int)Math.Pow(maxValue, cnvFactor);
                        if (bpp == 48)
                        {
                            if (minArray != null && maxArray != null)
                            {
                                tmpMinValue = (int)Math.Pow(minArray[0], cnvFactor);
                                tmpMaxValue = (int)Math.Pow(maxArray[0], cnvFactor);
                                ImageInfo.RedChannel.BlackValue = tmpMinValue;
                                ImageInfo.RedChannel.WhiteValue = tmpMaxValue;
                                tmpMinValue = (int)Math.Pow(minArray[1], cnvFactor);
                                tmpMaxValue = (int)Math.Pow(maxArray[1], cnvFactor);
                                ImageInfo.GreenChannel.BlackValue = tmpMinValue;
                                ImageInfo.GreenChannel.WhiteValue = tmpMaxValue;
                                tmpMinValue = (int)Math.Pow(minArray[2], cnvFactor);
                                tmpMaxValue = (int)Math.Pow(maxArray[2], cnvFactor);
                                ImageInfo.BlueChannel.BlackValue = tmpMinValue;
                                ImageInfo.BlueChannel.WhiteValue = tmpMaxValue;
                            }
                        }
                        else if (bpp == 64)
                        {
                            if (minArray != null && maxArray != null)
                            {
                                tmpMinValue = (int)Math.Pow(minArray[0], cnvFactor);
                                tmpMaxValue = (int)Math.Pow(maxArray[0], cnvFactor);
                                ImageInfo.RedChannel.BlackValue = tmpMinValue;
                                ImageInfo.RedChannel.WhiteValue = tmpMaxValue;
                                tmpMinValue = (int)Math.Pow(minArray[1], cnvFactor);
                                tmpMaxValue = (int)Math.Pow(maxArray[1], cnvFactor);
                                ImageInfo.GreenChannel.BlackValue = tmpMinValue;
                                ImageInfo.GreenChannel.WhiteValue = tmpMaxValue;
                                tmpMinValue = (int)Math.Pow(minArray[2], cnvFactor);
                                tmpMaxValue = (int)Math.Pow(maxArray[2], cnvFactor);
                                ImageInfo.BlueChannel.BlackValue = tmpMinValue;
                                ImageInfo.BlueChannel.WhiteValue = tmpMaxValue;
                                tmpMinValue = (int)Math.Pow(minArray[3], cnvFactor);
                                tmpMaxValue = (int)Math.Pow(maxArray[3], cnvFactor);
                                ImageInfo.GrayChannel.BlackValue = tmpMinValue;
                                ImageInfo.GrayChannel.WhiteValue = tmpMaxValue;
                            }
                        }
                        else
                        {
                            ImageInfo.MixChannel.BlackValue = tmpMinValue;
                            ImageInfo.MixChannel.WhiteValue = tmpMaxValue;
                        }
                    }
                    else
                    {
                        ImageInfo.MixChannel.BlackValue = (int)minValue;
                        ImageInfo.MixChannel.WhiteValue = (int)maxValue;
                        ImageInfo.MixChannel.GammaValue = 1.0;
                    }
                }
            }

            // If cropped image, carry over the contrast value.
            if (!bIsCropped)
            {
                if (IsRgbImage && _ImageInfo.SelectedChannel != ImageChannelType.Mix)
                {
                    _ImageInfo.SelectedChannel = ImageChannelType.Mix;
                }
            }

            // Set number of image channel.
            if (bpp == 8 || bpp == 16 || (bpp == 32 && newImage.Format == PixelFormats.Gray32Float))
            {
                _ImageInfo.NumOfChannels = 1;
                _ImageInfo.DisplayChannelFlags = ImageChannelFlag.None;
                _ImageInfo.ContrastChannelFlags = ImageChannelFlag.None;
            }
            if (bpp == 24 || bpp == 48)         //3-channel image
            {
                _ImageInfo.NumOfChannels = 3;
                _ImageInfo.DisplayChannelFlags |= ImageChannelFlag.Red | ImageChannelFlag.Green | ImageChannelFlag.Blue;
                _ImageInfo.ContrastChannelFlags |= ImageChannelFlag.Red | ImageChannelFlag.Green | ImageChannelFlag.Blue;
                if (!_ImageInfo.IsRedChannelAvail && !_ImageInfo.IsGreenChannelAvail && !_ImageInfo.IsBlueChannelAvail)
                {
                    _ImageInfo.IsRedChannelAvail = true;
                    _ImageInfo.IsGreenChannelAvail = true;
                    _ImageInfo.IsBlueChannelAvail = true;
                }
            }
            else if ((bpp == 32 && newImage.Format != PixelFormats.Gray32Float) || bpp == 64)    //4-channel image
            {
                _ImageInfo.NumOfChannels = 4;
                _ImageInfo.DisplayChannelFlags |= ImageChannelFlag.Red | ImageChannelFlag.Green | ImageChannelFlag.Blue | ImageChannelFlag.Gray;
                _ImageInfo.ContrastChannelFlags |= ImageChannelFlag.Red | ImageChannelFlag.Green | ImageChannelFlag.Blue | ImageChannelFlag.Gray;
                if (!_ImageInfo.IsRedChannelAvail && !_ImageInfo.IsGreenChannelAvail && !_ImageInfo.IsBlueChannelAvail && !_ImageInfo.IsGrayChannelAvail)
                {
                    _ImageInfo.IsRedChannelAvail = true;
                    _ImageInfo.IsGreenChannelAvail = true;
                    _ImageInfo.IsBlueChannelAvail = true;
                    _ImageInfo.IsGrayChannelAvail = true;
                }
            }

            UpdateDisplayImage();
        }

        #endregion

        #region Public properties....

        public string FileID
        {
            get { return _FileID; }
            set
            {
                _FileID = value;
                RaisePropertyChanged("FileID");
            }
        }

        //public bool IsInitialized { get; set; }
        //public bool IsFileLoaded { get; set; }
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

        #endregion

        private string GenerateID()
        {
            return Guid.NewGuid().ToString("N");
        }

        #region public double MinimumZoom
        /*public double MinimumZoom
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
        }*/
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
                    //if (_DrawingCanvas != null && _DrawingCanvas.Count > 0)
                    //{
                    //    _DrawingCanvas.ActualScale = _ZoomLevel;
                    //}
                }
            }
        }
        #endregion

        #region public DrawingCanvas DrawingCanvas
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
        #endregion

        #region public ToolType SelectedDrawingTool
        public ToolType SelectedDrawingTool
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
        #endregion

        public ScaleBar Scalebar
        {
            get { return _ImageInfo.Scalebar; }
            set { _ImageInfo.Scalebar = value; }
        }

        public bool IsInitialized { get; set; }
        public bool IsShowScalebar
        {
            get { return _ImageInfo.IsShowScalebar; }
            set
            {
                _ImageInfo.IsShowScalebar = value;
                RaisePropertyChanged("IsShowScalebar");
            }
        }

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
                        _ImageInfo.CaptureType.Contains("Fluorescence") ||
                        _ImageInfo.CaptureType.Contains("Phosphor") ||
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
                    if (_Image.Format.BitsPerPixel == 24 && _ImageInfo.IsChemiImage)
                    {
                        exposureTime = GetFormattedExposureTime(_ImageInfo.MixChannel.Exposure);
                    }
                    else
                    {
                        string strExpTimeCh1 = (_ImageInfo.RedChannel.Exposure > 0) ? GetFormattedExposureTime(_ImageInfo.RedChannel.Exposure) : "--";
                        string strExpTimeCh2 = (_ImageInfo.GreenChannel.Exposure > 0) ? string.Format(" / {0}", GetFormattedExposureTime(_ImageInfo.GreenChannel.Exposure)) : " / --";
                        string strExpTimeCh3 = string.Empty;
                        if (_ImageInfo.CaptureType.IndexOf("RGB", StringComparison.OrdinalIgnoreCase) >= 0 || _ImageInfo.NumOfChannels == 4)
                        {
                            strExpTimeCh3 = (_ImageInfo.BlueChannel.Exposure > 0) ? string.Format(" / {0}", GetFormattedExposureTime(_ImageInfo.BlueChannel.Exposure)) : " / --";
                        }
                        else
                        {
                            strExpTimeCh3 = (_ImageInfo.BlueChannel.Exposure > 0) ? string.Format(" / {0}", GetFormattedExposureTime(_ImageInfo.BlueChannel.Exposure)) : "";
                        }
                        string strExpTimeCh4 = string.Empty;
                        if (_ImageInfo.NumOfChannels == 4)
                        {
                            strExpTimeCh4 = (_ImageInfo.GrayChannel.Exposure > 0) ? string.Format(" / {0}", GetFormattedExposureTime(_ImageInfo.GrayChannel.Exposure)) : "";
                        }
                        exposureTime = string.Format("{0}{1}{2}{3}", strExpTimeCh1, strExpTimeCh2, strExpTimeCh3, strExpTimeCh4);
                    }
                }
                else
                {
                    if (_ImageInfo.MixChannel.Exposure > 0)
                    {
                        exposureTime = GetFormattedExposureTime(_ImageInfo.MixChannel.Exposure);
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

        public string AutoExposureType
        {
            get
            {
                string result = string.Empty;
                if (_ImageInfo != null)
                {
                    if (!string.IsNullOrEmpty(_ImageInfo.AutoExposureType))
                    {
                        result = _ImageInfo.AutoExposureType;
                    }
                    else
                    {
                        result = string.Empty;
                    }
                }
                return result;
            }
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
                if (IsRgbImage)
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

        #region ImageInfo public properties...

        public string ApdGain
        {
            get
            {
                string result = string.Empty;
                if (_ImageInfo == null)
                {
                    result = string.Empty;
                }
                else
                {
                    if (_ImageInfo.IsScannedImage)
                    {
                        if (_ImageInfo.NumOfChannels > 1)
                        {
                            // Now using the ascending order of laser's wavelength instead of using the internal labeling of laser A/B/C/D (swapping laser A and laser D)
                            result = string.Format("{0} / {1} / {2} / {3}",
                                (_ImageInfo.RedChannel.LaserIntensity > 0) ? _ImageInfo.RedChannel.ApdGain.ToString() : "-",
                                (_ImageInfo.GreenChannel.LaserIntensity > 0) ? _ImageInfo.GreenChannel.ApdGain.ToString() : "-",
                                (_ImageInfo.BlueChannel.LaserIntensity > 0) ? _ImageInfo.BlueChannel.ApdGain.ToString() : "-",
                                (_ImageInfo.GrayChannel.LaserIntensity > 0) ? _ImageInfo.GrayChannel.ApdGain.ToString() : "-");
                        }
                        else
                        {
                            //EL: TODO: add backward compatibility?
                            //var laserType = GetLaserType();
                            //if (laserType == ImagingSystem.LaserType.LaserA)
                            //    result = _ImageInfo.ApdAGain.ToString();
                            //else if (laserType == ImagingSystem.LaserType.LaserB)
                            //    result = _ImageInfo.ApdBGain.ToString();
                            //else if (laserType == ImagingSystem.LaserType.LaserC)
                            //    result = _ImageInfo.ApdCGain.ToString();
                            //else if (laserType == ImagingSystem.LaserType.LaserD)
                            //    result = _ImageInfo.ApdDGain.ToString();
                            ////result = _ImageInfo.MixChannel.ApdGain.ToString();
                            if (_ImageInfo.MixChannel.ApdGain > 0)
                                result = _ImageInfo.MixChannel.ApdGain.ToString();
                        }
                    }
                }
                return result;
            }
        }

        public string ApdPga
        {
            get
            {
                string result = string.Empty;
                if (_ImageInfo == null)
                {
                    result = string.Empty;
                }
                else
                {
                    if (_ImageInfo.IsScannedImage)
                    {
                        if (_ImageInfo.NumOfChannels > 1)
                        {
                            // Now using the ascending order of laser's wavelength instead of using the internal labeling of laser A/B/C/D (swapping laser A and laser D)
                            result = string.Format("{0} / {1} / {2} / {3}",
                                (_ImageInfo.RedChannel.LaserIntensity > 0) ? _ImageInfo.RedChannel.ApdPga.ToString() : "-",
                                (_ImageInfo.GreenChannel.LaserIntensity > 0) ? _ImageInfo.GreenChannel.ApdPga.ToString() : "-",
                                (_ImageInfo.BlueChannel.LaserIntensity > 0) ? _ImageInfo.BlueChannel.ApdPga.ToString() : "-",
                                (_ImageInfo.GrayChannel.LaserIntensity > 0) ? _ImageInfo.GrayChannel.ApdPga.ToString() : "-");
                        }
                        else
                        {
                            //EL: TODO: add backward compatibility?
                            //var laserType = GetLaserType();
                            //if (laserType == ImagingSystem.LaserType.LaserA)
                            //    result = _ImageInfo.ApdAPga.ToString();
                            //else if (laserType == ImagingSystem.LaserType.LaserB)
                            //    result = _ImageInfo.ApdBPga.ToString();
                            //else if (laserType == ImagingSystem.LaserType.LaserC)
                            //    result = _ImageInfo.ApdCPga.ToString();
                            //else if (laserType == ImagingSystem.LaserType.LaserD)
                            //    result = _ImageInfo.ApdDPga.ToString();
                            ////result = _ImageInfo.MixChannel.ApdPga.ToString();
                            if (_ImageInfo.MixChannel.ApdPga > 0)
                                result = _ImageInfo.MixChannel.ApdPga.ToString();
                        }
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// Laser intensity in milliwatts
        /// </summary>
        public string LasersIntensity
        {
            get
            {
                string lasersIntensity = string.Empty;
                if (_ImageInfo != null && _ImageInfo.IsScannedImage)
                {
                    if (_ImageInfo.NumOfChannels > 1)
                    {
                        // Now using the ascending order of laser's wavelength instead of using the internal labeling of laser A/B/C/D (swapping laser A and laser D)
                        lasersIntensity = string.Format("{0} / {1} / {2} / {3}",
                            (_ImageInfo.RedChannel.LaserIntensity > 0) ? _ImageInfo.RedChannel.LaserIntensity.ToString() : "-",
                            (_ImageInfo.GreenChannel.LaserIntensity > 0) ? _ImageInfo.GreenChannel.LaserIntensity.ToString() : "-",
                            (_ImageInfo.BlueChannel.LaserIntensity > 0) ? _ImageInfo.BlueChannel.LaserIntensity.ToString() : "-",
                            (_ImageInfo.GrayChannel.LaserIntensity > 0) ? _ImageInfo.GrayChannel.LaserIntensity.ToString() : "-");
                    }
                    else
                    {
                        //EL: TODO: add backward compatibility?
                        //var laserType = GetLaserType();
                        //if (laserType == ImagingSystem.LaserType.LaserA)
                        //    lasersIntensity = _ImageInfo.LaserAIntensity.ToString();
                        //else if (laserType == ImagingSystem.LaserType.LaserB)
                        //    lasersIntensity = _ImageInfo.LaserBIntensity.ToString();
                        //else if (laserType == ImagingSystem.LaserType.LaserC)
                        //    lasersIntensity = _ImageInfo.LaserCIntensity.ToString();
                        //else if (laserType == ImagingSystem.LaserType.LaserD)
                        //    lasersIntensity = _ImageInfo.LaserDIntensity.ToString();
                        if (_ImageInfo.MixChannel.LaserIntensity > 0)
                            lasersIntensity = _ImageInfo.MixChannel.LaserIntensity.ToString();
                    }
                }
                return lasersIntensity;
            }
        }

        public string LaserWavelength
        {
            get
            {
                string laserWavelength = string.Empty;
                if (_ImageInfo != null && _ImageInfo.IsScannedImage)
                {
                    if (_ImageInfo.NumOfChannels > 1)
                    {
                        laserWavelength = string.Format("{0} / {1} / {2} / {3}",
                            (_ImageInfo.RedChannel.LaserIntensity > 0) ? (!string.IsNullOrEmpty(_ImageInfo.RedChannel.LaserWavelength) ? _ImageInfo.RedChannel.LaserWavelength + " (R)" : "-") : "-",
                            (_ImageInfo.GreenChannel.LaserIntensity > 0) ? (!string.IsNullOrEmpty(_ImageInfo.GreenChannel.LaserWavelength) ? _ImageInfo.GreenChannel.LaserWavelength + " (G)" : "-") : "-",
                            (_ImageInfo.BlueChannel.LaserIntensity > 0) ? (!string.IsNullOrEmpty(_ImageInfo.BlueChannel.LaserWavelength) ? _ImageInfo.BlueChannel.LaserWavelength + " (B)" : "-") : "-",
                            (_ImageInfo.GrayChannel.LaserIntensity > 0) ? (!string.IsNullOrEmpty(_ImageInfo.GrayChannel.LaserWavelength) ? _ImageInfo.GrayChannel.LaserWavelength + " (K)" : "-") : "-");
                    }
                    else
                    {
                        laserWavelength = _ImageInfo.MixChannel.LaserWavelength;
                    }
                }
                return laserWavelength;
            }
        }

        public string FilterWavelength
        {
            get
            {
                string wavelength = string.Empty;
                if (_ImageInfo != null && _ImageInfo.IsScannedImage)
                {
                    if (_ImageInfo.NumOfChannels > 1)
                    {
                        wavelength = string.Format("{0} / {1} / {2} / {3}",
                            (_ImageInfo.RedChannel.LaserIntensity > 0) ? (!string.IsNullOrEmpty(_ImageInfo.RedChannel.FilterWavelength) ? _ImageInfo.RedChannel.FilterWavelength + " (R)" : "-") : "-",
                            (_ImageInfo.GreenChannel.LaserIntensity > 0) ? (!string.IsNullOrEmpty(_ImageInfo.GreenChannel.FilterWavelength) ? _ImageInfo.GreenChannel.FilterWavelength + " (G)" : "-") : "-",
                            (_ImageInfo.BlueChannel.LaserIntensity > 0) ? (!string.IsNullOrEmpty(_ImageInfo.BlueChannel.FilterWavelength) ? _ImageInfo.BlueChannel.FilterWavelength + " (B)" : "-") : "-",
                            (_ImageInfo.GrayChannel.LaserIntensity > 0) ? (!string.IsNullOrEmpty(_ImageInfo.GrayChannel.FilterWavelength) ? _ImageInfo.GrayChannel.FilterWavelength + " (K)" : "-") : "-");
                    }
                    else
                    {
                        wavelength = _ImageInfo.MixChannel.FilterWavelength;
                    }
                }
                return wavelength;
            }
        }

        public string LaserChannel
        {
            get
            {
                string laserWavelength = string.Empty;
                if (_ImageInfo != null && _ImageInfo.IsScannedImage)
                {
                    if (_ImageInfo.NumOfChannels > 1)
                    {
                        laserWavelength = string.Format("{0} / {1} / {2} / {3}",
                            (_ImageInfo.RedChannel.LaserIntensity > 0) ? (!string.IsNullOrEmpty(_ImageInfo.RedChannel.LaserChannel) ? _ImageInfo.RedChannel.LaserChannel : "-") : "-",
                            (_ImageInfo.GreenChannel.LaserIntensity > 0) ? (!string.IsNullOrEmpty(_ImageInfo.GreenChannel.LaserChannel) ? _ImageInfo.GreenChannel.LaserChannel : "-") : "-",
                            (_ImageInfo.BlueChannel.LaserIntensity > 0) ? (!string.IsNullOrEmpty(_ImageInfo.BlueChannel.LaserChannel) ? _ImageInfo.BlueChannel.LaserChannel : "-") : "-",
                            (_ImageInfo.GrayChannel.LaserIntensity > 0) ? (!string.IsNullOrEmpty(_ImageInfo.GrayChannel.LaserChannel) ? _ImageInfo.GrayChannel.LaserChannel : "-") : "-");
                    }
                    else
                    {
                        laserWavelength = _ImageInfo.MixChannel.LaserChannel;
                    }
                }
                return laserWavelength;
            }
        }

        public string ScanFocusPosition
        {
            get
            {
                string focusPositions = string.Empty;
                if (_ImageInfo != null && _ImageInfo.IsScannedImage)
                {
                    string strSoftware = _ImageInfo.Software;
                    //if (!string.IsNullOrEmpty(FilePath))
                    //{
                    //    strSoftware = GetSoftware(FilePath);
                    //}
                    if (_ImageInfo.NumOfChannels > 1)
                    {
                        if (strSoftware == "Sapphire FL")
                        {
                            focusPositions = string.Format("{0} / {1} / {2} / {3}",
                                (_ImageInfo.RedChannel.LaserIntensity > 0) ? GetScannedFocusPosition(_ImageInfo, ImageChannelType.Red) : "-",
                                (_ImageInfo.GreenChannel.LaserIntensity > 0) ? GetScannedFocusPosition(_ImageInfo, ImageChannelType.Green) : "-",
                                (_ImageInfo.BlueChannel.LaserIntensity > 0) ? GetScannedFocusPosition(_ImageInfo, ImageChannelType.Blue) : "-",
                                (_ImageInfo.GrayChannel.LaserIntensity > 0) ? GetScannedFocusPosition(_ImageInfo, ImageChannelType.Gray) : "-");
                        }
                        else
                        {
                            //EL: TODO: Add backward compatibility?
                            // Now using the ascending order of laser's wavelength instead of using laser A/B/C/D (swapping laser A and laser D)
                            focusPositions = string.Format("{0} / {1} / {2} / {3}",
                                (_ImageInfo.LaserDIntensity > 0) ? GetScannedFocusPosition(_ImageInfo, ImagingSystem.LaserType.LaserD).ToString() : "-",
                                (_ImageInfo.LaserBIntensity > 0) ? GetScannedFocusPosition(_ImageInfo, ImagingSystem.LaserType.LaserB).ToString() : "-",
                                (_ImageInfo.LaserCIntensity > 0) ? GetScannedFocusPosition(_ImageInfo, ImagingSystem.LaserType.LaserC).ToString() : "-",
                                (_ImageInfo.LaserAIntensity > 0) ? GetScannedFocusPosition(_ImageInfo, ImagingSystem.LaserType.LaserA).ToString() : "-");
                        }
                    }
                    else
                    {
                        if (strSoftware == "Sapphire FL")
                        {
                            focusPositions = GetScannedFocusPosition(_ImageInfo, ImageChannelType.Mix);
                        }
                        else
                        {
                            //EL: TODO: Add backward compatibility?
                            var laserType = GetLaserType();
                            if (laserType == ImagingSystem.LaserType.LaserA)
                                focusPositions = GetScannedFocusPosition(_ImageInfo, ImagingSystem.LaserType.LaserA).ToString();
                            else if (laserType == ImagingSystem.LaserType.LaserB)
                                focusPositions = GetScannedFocusPosition(_ImageInfo, ImagingSystem.LaserType.LaserB).ToString();
                            else if (laserType == ImagingSystem.LaserType.LaserC)
                                focusPositions = GetScannedFocusPosition(_ImageInfo, ImagingSystem.LaserType.LaserC).ToString();
                            else if (laserType == ImagingSystem.LaserType.LaserD)
                                focusPositions = GetScannedFocusPosition(_ImageInfo, ImagingSystem.LaserType.LaserD).ToString();
                        }
                    }
                }
                return focusPositions;
            }
        }

        /// <summary>
        /// Get the capture software name from the image metadata (305/Software)
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private string GetSoftware(string filename)
        {
            string strSoftware = string.Empty;

            try
            {
                BitmapSource srcBitmap = BitmapFrame.Create(new Uri(filename));
                if (srcBitmap != null)
                {
                    BitmapMetadata metadata = (BitmapMetadata)srcBitmap.Metadata;
                    if (metadata != null)
                    {
                        strSoftware = metadata.GetQuery("/ifd/{ushort=305}") as string;
                    }
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }

            return strSoftware;
        }

        /// <summary>
        /// Get TIFF Tag PhotometricInterpretation value
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private ushort GetPhotometricInterpretation(string filename)
        {
            ushort photometricInt = 1;

            try
            {
                BitmapSource srcBitmap = BitmapFrame.Create(new Uri(filename));
                if (srcBitmap != null)
                {
                    BitmapMetadata metadata = (BitmapMetadata)srcBitmap.Metadata;
                    if (metadata != null)
                    {
                        // Get photometricInterpolation
                        photometricInt = (ushort)metadata.GetQuery("/ifd/exif:{ushort=262}");
                    }
                }
            }
            catch (Exception)
            {
                return (ushort)PhotometricInterpretationTag.PHOTOMETRIC_MINISBLACK;
            }

            return photometricInt;
        }


        private double GetScannedFocusPosition(ImageInfo imgInfo, ImagingSystem.LaserType laserType)
        {
            double result = 0;
            var colorChannel = GetColorChannel(imgInfo, laserType);
            if (colorChannel == ImageChannelType.Red)
            {
                if (imgInfo.RedChannel.ScanZ0 == 0)
                    result = imgInfo.ScanZ0;
                else
                {
                    if (imgInfo.ScanZ0Abs != 0)
                        result = imgInfo.ScanZ0Abs - imgInfo.RedChannel.ScanZ0;
                }
            }
            else if (colorChannel == ImageChannelType.Green)
            {
                if (imgInfo.GreenChannel.ScanZ0 == 0)
                    result = imgInfo.ScanZ0;
                else
                {
                    if (imgInfo.ScanZ0Abs != 0)
                        result = imgInfo.ScanZ0Abs - imgInfo.GreenChannel.ScanZ0;
                }
            }
            else if (colorChannel == ImageChannelType.Blue)
            {
                if (imgInfo.BlueChannel.ScanZ0 == 0)
                    result = imgInfo.ScanZ0;
                else
                {
                    if (imgInfo.ScanZ0Abs != 0)
                        result = imgInfo.ScanZ0Abs - imgInfo.BlueChannel.ScanZ0;
                }
            }
            else if (colorChannel == ImageChannelType.Gray)
            {
                if (imgInfo.GrayChannel.ScanZ0 == 0)
                    result = imgInfo.ScanZ0;
                else
                {
                    if (imgInfo.ScanZ0Abs != 0)
                        result = imgInfo.ScanZ0Abs - imgInfo.GrayChannel.ScanZ0;
                }
            }
            else
            {
                if (imgInfo.ScanZ0 != 0 && imgInfo.ScanZ0Abs != 0)
                {
                    result = imgInfo.ScanZ0Abs - imgInfo.ScanZ0;
                }
                else if (imgInfo.MixChannel.ScanZ0 != 0 && imgInfo.ScanZ0Abs != 0)
                {
                    result = imgInfo.ScanZ0Abs - imgInfo.MixChannel.ScanZ0;
                }
                else
                {
                    if (imgInfo.ScanZ0 != 0)
                        result = imgInfo.ScanZ0;
                    else if (imgInfo.MixChannel.ScanZ0 != 0)
                        result = imgInfo.MixChannel.ScanZ0;
                }
            }
            return Math.Round(result, 3);
        }
        /// <summary>
        /// Focus position of 0 (zero) is the focus position on the (top) of the glass
        /// </summary>
        /// <param name="imgInfo"></param>
        /// <param name="colorChannel"></param>
        /// <returns></returns>
        private string GetScannedFocusPosition(ImageInfo imgInfo, ImageChannelType colorChannel)
        {
            double focusPosition = 0;

            if (colorChannel == ImageChannelType.Red)
            {
                if (imgInfo.RedChannel.ScanZ0 == 0)
                {
                    if (imgInfo.ScanZ0Abs != 0 && imgInfo.ScanZ0 != 0)
                    {
                        focusPosition =  imgInfo.ScanZ0 - imgInfo.ScanZ0Abs;
                    }
                    else
                    {
                        focusPosition = imgInfo.ScanZ0;
                    }
                }
                else
                {
                    if (imgInfo.ScanZ0Abs != 0)
                    {
                        focusPosition = imgInfo.RedChannel.ScanZ0 - imgInfo.ScanZ0Abs;
                    }
                    else
                    {
                        focusPosition = imgInfo.RedChannel.ScanZ0;
                    }
                }
            }
            else if (colorChannel == ImageChannelType.Green)
            {
                if (imgInfo.GreenChannel.ScanZ0 == 0)
                {
                    if (imgInfo.ScanZ0Abs != 0 && imgInfo.ScanZ0 != 0)
                    {
                        focusPosition = +imgInfo.ScanZ0 - imgInfo.ScanZ0Abs;
                    }
                    else
                    {
                        focusPosition = imgInfo.ScanZ0;
                    }
                }
                else
                {
                    if (imgInfo.ScanZ0Abs != 0)
                    {
                        focusPosition = imgInfo.GreenChannel.ScanZ0 - imgInfo.ScanZ0Abs;
                    }
                    else
                    {
                        focusPosition = imgInfo.GreenChannel.ScanZ0;
                    }
                }
            }
            else if (colorChannel == ImageChannelType.Blue)
            {
                if (imgInfo.BlueChannel.ScanZ0 == 0)
                {
                    if (imgInfo.ScanZ0Abs != 0 && imgInfo.ScanZ0 != 0)
                    {
                        focusPosition = imgInfo.ScanZ0 - imgInfo.ScanZ0Abs;
                    }
                    else
                    {
                        focusPosition = imgInfo.ScanZ0;
                    }
                }
                else
                {
                    if (imgInfo.ScanZ0Abs != 0)
                    {
                        focusPosition = imgInfo.BlueChannel.ScanZ0 - imgInfo.ScanZ0Abs;
                    }
                    else
                    {
                        focusPosition = imgInfo.BlueChannel.ScanZ0;
                    }
                }
            }
            else if (colorChannel == ImageChannelType.Gray)
            {
                if (imgInfo.GrayChannel.ScanZ0 == 0)
                {
                    if (imgInfo.ScanZ0Abs != 0 && imgInfo.ScanZ0 != 0)
                    {
                        focusPosition = imgInfo.ScanZ0 - imgInfo.ScanZ0Abs;
                    }
                    else
                    {
                        focusPosition = imgInfo.ScanZ0;
                    }
                }
                else
                {
                    if (imgInfo.ScanZ0Abs != 0)
                    {
                        focusPosition = imgInfo.GrayChannel.ScanZ0 - imgInfo.ScanZ0Abs;
                    }
                    else
                    {
                        focusPosition = imgInfo.GrayChannel.ScanZ0;
                    }
                }
            }
            else
            {
                if (imgInfo.MixChannel.ScanZ0 != 0 && imgInfo.ScanZ0Abs != 0)
                {
                    focusPosition = imgInfo.MixChannel.ScanZ0 - imgInfo.ScanZ0Abs;
                }
                else if (imgInfo.ScanZ0 != 0 && imgInfo.ScanZ0Abs != 0)
                {
                    focusPosition = imgInfo.ScanZ0 - imgInfo.ScanZ0Abs;
                }
                else
                {
                    if (imgInfo.MixChannel.ScanZ0 != 0)
                        focusPosition = imgInfo.MixChannel.ScanZ0;
                    else if (imgInfo.ScanZ0 != 0)
                        focusPosition = imgInfo.ScanZ0;
                }
            }

            string signOfNumber = string.Empty;
            focusPosition = Math.Round(focusPosition, 2);
            if (focusPosition != 0)
            {
                signOfNumber = (focusPosition > 0) ? "+" : "-";
            }
            //focusPosition = string.Format("{0}{1}", signOfNumber, Math.Abs(focusPosition).ToString("F2", System.Globalization.CultureInfo.InvariantCulture));

            return string.Format("{0}{1}", signOfNumber, Math.Abs(focusPosition).ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
        }

        /*public string ScanRegion
        {
            get
            {
                string scanRegion = string.Empty;
                if (_ImageInfo != null && _ImageInfo.IsScannedImage)
                {
                    int y1 = (int)Math.Round(_ImageInfo.ScanY0 / 10.0);
                    int y2 = (int)Math.Round((_ImageInfo.ScanY0 + _ImageInfo.DeltaY) / 10.0);
                    string row = string.Format("{0}-{1}", IndexToRow(y1), IndexToRow(y2));

                    int x1 = (int)Math.Round(_ImageInfo.ScanX0 / 10.0);
                    int x2 = (int)Math.Round((_ImageInfo.ScanX0 + _ImageInfo.DeltaX) / 10.0);

                    string col = string.Format("{0}-{1}", x1, x2);
                    scanRegion = row + ", " + col;
                }
                return scanRegion;
            }
        }*/
        public string ScanRegion
        {
            get
            {
                string scanRegion = string.Empty;
                if (_ImageInfo != null && _ImageInfo.IsScannedImage)
                {
                    //int y1 = (int)Math.Round(_ImageInfo.ScanRegion.Y);
                    //int y2 = (int)Math.Round(_ImageInfo.ScanRegion.Y + _ImageInfo.ScanRegion.Height);
                    //string row = string.Format("{0}-{1}", IndexToRow(y1), IndexToRow(y2));

                    //int x1 = (int)Math.Round(_ImageInfo.ScanRegion.X);
                    //int x2 = (int)Math.Round(_ImageInfo.ScanRegion.X + _ImageInfo.ScanRegion.Width);

                    //string col = string.Format("{0}-{1}", x1, x2);
                    //scanRegion = row + ", " + col;
                    scanRegion = _ImageInfo.ScanRegion;
                }
                return scanRegion;
            }
        }

        public string SampleType
        {
            get
            {
                string sampleType = string.Empty;
                if (_ImageInfo != null)
                {
                    sampleType = _ImageInfo.SampleType;
                }
                return sampleType;
            }
        }

        public string ScanSpeed
        {
            get
            {
                string scanSpeed = string.Empty;
                if (_ImageInfo != null)
                {
                    scanSpeed = _ImageInfo.ScanSpeed;
                }
                return scanSpeed;
            }
        }

        /// <summary>
        /// User selected laser's intensity level (config.xml lookup table).
        /// </summary>
        public string IntensityLevel
        {
            get
            {
                string intensityLevel = string.Empty;
                if (_ImageInfo != null)
                {
                    if (_ImageInfo.NumOfChannels > 1)
                    {
                        // v1.5 includes 14 intensity levels (with L1 - L4, and 1-10) [use lookup table]
                        //if (_ImageInfo.MajorVersion > 1 || (_ImageInfo.MajorVersion == 1 && _ImageInfo.MinorVersion >= 5) ||
                        //    (!string.IsNullOrEmpty(_ImageInfo.IntensityLevel) && _ImageInfo.IntensityLevel.Contains('L')))
                        //{
                        //    // Now using the order of laser's wavelength instead of using internal labeling of laser A/B/C/D (swapping laser A and laser D)
                        //    intensityLevel = string.Format("{0} / {1} / {2} / {3}",
                        //        (_ImageInfo.LaserDIntensity > 0) ? IntensityLevels[GetIntensityLevel(_ImageInfo, ImagingSystem.LaserType.LaserD)] : "-",
                        //        (_ImageInfo.LaserBIntensity > 0) ? IntensityLevels[GetIntensityLevel(_ImageInfo, ImagingSystem.LaserType.LaserB)] : "-",
                        //        (_ImageInfo.LaserCIntensity > 0) ? IntensityLevels[GetIntensityLevel(_ImageInfo, ImagingSystem.LaserType.LaserC)] : "-",
                        //        (_ImageInfo.LaserAIntensity > 0) ? IntensityLevels[GetIntensityLevel(_ImageInfo, ImagingSystem.LaserType.LaserA)] : "-");
                        //}
                        //else
                        //{
                        //    // Now using the order of laser's wavelength instead of using internal labeling of laser A/B/C/D (swapping laser A and laser D)
                        //    intensityLevel = string.Format("{0} / {1} / {2} / {3}",
                        //        (_ImageInfo.LaserDIntensity > 0) ? GetIntensityLevel(_ImageInfo, ImagingSystem.LaserType.LaserD).ToString() : "-",
                        //        (_ImageInfo.LaserBIntensity > 0) ? GetIntensityLevel(_ImageInfo, ImagingSystem.LaserType.LaserB).ToString() : "-",
                        //        (_ImageInfo.LaserCIntensity > 0) ? GetIntensityLevel(_ImageInfo, ImagingSystem.LaserType.LaserC).ToString() : "-",
                        //        (_ImageInfo.LaserAIntensity > 0) ? GetIntensityLevel(_ImageInfo, ImagingSystem.LaserType.LaserA).ToString() : "-");
                        //}

                        if (!string.IsNullOrEmpty(_ImageInfo.RedChannel.SignalLevel) ||
                            !string.IsNullOrEmpty(_ImageInfo.GreenChannel.SignalLevel) ||
                            !string.IsNullOrEmpty(_ImageInfo.BlueChannel.SignalLevel))
                        {
                            intensityLevel = string.Format("{0} / {1} / {2} / {3}",
                                (_ImageInfo.RedChannel.LaserIntensity > 0) ? ((_ImageInfo.RedChannel.SignalLevel != null) ? _ImageInfo.RedChannel.SignalLevel : "-") : "-",
                                (_ImageInfo.GreenChannel.LaserIntensity > 0) ? ((_ImageInfo.GreenChannel.SignalLevel != null) ? _ImageInfo.GreenChannel.SignalLevel : "-") : "-",
                                (_ImageInfo.BlueChannel.LaserIntensity > 0) ? ((_ImageInfo.BlueChannel.SignalLevel != null) ? _ImageInfo.BlueChannel.SignalLevel : "-") : "-",
                                (_ImageInfo.GrayChannel.LaserIntensity > 0) ? ((_ImageInfo.GrayChannel.SignalLevel != null) ? _ImageInfo.GrayChannel.SignalLevel : "-") : "-");
                        }
                        else
                        {
                            intensityLevel = string.Format("{0} / {1} / {2} / {3}",
                                (_ImageInfo.RedChannel.LaserIntensity > 0) ? IntensityLevels[_ImageInfo.RedChannel.LaserIntensityLevel] : "-",
                                (_ImageInfo.GreenChannel.LaserIntensity > 0) ? IntensityLevels[_ImageInfo.GreenChannel.LaserIntensityLevel] : "-",
                                (_ImageInfo.BlueChannel.LaserIntensity > 0) ? IntensityLevels[_ImageInfo.BlueChannel.LaserIntensityLevel] : "-",
                                (_ImageInfo.GrayChannel.LaserIntensity > 0) ? IntensityLevels[_ImageInfo.GrayChannel.LaserIntensityLevel] : "-");
                        }
                    }
                    else
                    {
                        //if (_ImageInfo.CaptureType.ToLower().Contains("phosphor"))
                        //{
                        //    // Special case: not using the 10 or 14 signal levels (Phosphor intensity levels: 1-5)
                        //    intensityLevel = _ImageInfo.IntensityLevel;
                        //}
                        //else
                        //{
                        //    intensityLevel = _ImageInfo.IntensityLevel;
                        //    var laserType = GetLaserType();
                        //    // v1.5 includes 14 intensity levels (with L1 - L4 and 1-10) [use lookup table]
                        //    if (_ImageInfo.MajorVersion > 1 || (_ImageInfo.MajorVersion == 1 && _ImageInfo.MinorVersion >= 5) ||
                        //        (!string.IsNullOrEmpty(_ImageInfo.IntensityLevel) && _ImageInfo.IntensityLevel.Contains('L')))
                        //    {
                        //        if (laserType == ImagingSystem.LaserType.LaserA)
                        //            intensityLevel = IntensityLevels[GetIntensityLevel(_ImageInfo, ImagingSystem.LaserType.LaserA)];
                        //        else if (laserType == ImagingSystem.LaserType.LaserB)
                        //            intensityLevel = IntensityLevels[GetIntensityLevel(_ImageInfo, ImagingSystem.LaserType.LaserB)];
                        //        else if (laserType == ImagingSystem.LaserType.LaserC)
                        //            intensityLevel = IntensityLevels[GetIntensityLevel(_ImageInfo, ImagingSystem.LaserType.LaserC)];
                        //        else if (laserType == ImagingSystem.LaserType.LaserD)
                        //            intensityLevel = IntensityLevels[GetIntensityLevel(_ImageInfo, ImagingSystem.LaserType.LaserD)];
                        //    }
                        //    else
                        //    {
                        //        if (laserType == ImagingSystem.LaserType.LaserA)
                        //            intensityLevel = GetIntensityLevel(_ImageInfo, ImagingSystem.LaserType.LaserA).ToString();
                        //        else if (laserType == ImagingSystem.LaserType.LaserB)
                        //            intensityLevel = GetIntensityLevel(_ImageInfo, ImagingSystem.LaserType.LaserB).ToString();
                        //        else if (laserType == ImagingSystem.LaserType.LaserC)
                        //            intensityLevel = GetIntensityLevel(_ImageInfo, ImagingSystem.LaserType.LaserC).ToString();
                        //        else if (laserType == ImagingSystem.LaserType.LaserD)
                        //            intensityLevel = GetIntensityLevel(_ImageInfo, ImagingSystem.LaserType.LaserD).ToString();
                        //    }
                        //}
                        if (_ImageInfo.MixChannel.LaserIntensity > 0)
                        {
                            if (!string.IsNullOrEmpty(_ImageInfo.MixChannel.SignalLevel))
                            {
                                intensityLevel = _ImageInfo.MixChannel.SignalLevel;
                            }
                            else
                            {
                                intensityLevel = (_ImageInfo.MixChannel.LaserIntensity > 0) ? IntensityLevels[_ImageInfo.MixChannel.LaserIntensityLevel] : "-";
                            }
                        }
                    }
                }
                return intensityLevel;
            }
        }
        private int GetIntensityLevel(ImageInfo imgInfo, ImagingSystem.LaserType laserType)
        {
            int result = 0;
            var colorChannel = GetColorChannel(imgInfo, laserType);
            if (colorChannel == ImageChannelType.Red)
            {
                result = imgInfo.RedChannel.LaserIntensityLevel;
            }
            else if (colorChannel == ImageChannelType.Green)
            {
                result = imgInfo.GreenChannel.LaserIntensityLevel;
            }
            else if (colorChannel == ImageChannelType.Blue)
            {
                result = imgInfo.BlueChannel.LaserIntensityLevel;
            }
            else if (colorChannel == ImageChannelType.Gray)
            {
                result = imgInfo.GrayChannel.LaserIntensityLevel;
            }
            else
            {
                if (imgInfo.MixChannel.LaserIntensityLevel == 0)
                {
                    if (!string.IsNullOrEmpty(imgInfo.IntensityLevel))
                    {
                        int nIntLevel = 0;
                        int.TryParse(imgInfo.IntensityLevel, out nIntLevel);
                        result = nIntLevel;
                    }
                }
                else
                {
                    result = imgInfo.MixChannel.LaserIntensityLevel;
                }
            }
            return result;
        }
        private ImageChannelType GetColorChannel(ImageInfo imgInfo, ImagingSystem.LaserType laserType)
        {
            ImageChannelType result = ImageChannelType.Mix;
            if ((int)laserType == imgInfo.RedChannel.LightSource)
            {
                result = ImageChannelType.Red;
            }
            else if ((int)laserType == imgInfo.GreenChannel.LightSource)
            {
                result = ImageChannelType.Green;
            }
            else if ((int)laserType == imgInfo.BlueChannel.LightSource)
            {
                result = ImageChannelType.Blue;
            }
            else if ((int)laserType == imgInfo.GrayChannel.LightSource)
            {
                result = ImageChannelType.Gray;
            }
            return result;
        }


        static readonly string[] IntensityLevels = new[] { "0", "L1", "L2", "L3", "L4", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };
        static readonly string[] Rows = new[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
        public static string IndexToRow(double index)
        {
            index = Math.Round(index);
            if (index > Rows.Length - 1) { index = Rows.Length - 1; }
            if (index < 0)
                throw new IndexOutOfRangeException("index must be a positive number");

            return Rows[(int)index];
        }

        private string GetColorChannel(ImagingSystem.LaserType laserType)
        {
            string result = string.Empty;
            if ((int)laserType == _ImageInfo.RedChannel.LightSource)
            {
                result = " (R)";
            }
            else if ((int)laserType == _ImageInfo.GreenChannel.LightSource)
            {
                result = " (G)";
            }
            else if ((int)laserType == _ImageInfo.BlueChannel.LightSource)
            {
                result = " (B)";
            }
            else if ((int)laserType == _ImageInfo.GrayChannel.LightSource)
            {
                result = " (K)";
            }
            return result;
        }

        /// <summary>
        /// Get a single channel grayscale image laser's type.
        /// </summary>
        /// <returns></returns>
        private ImagingSystem.LaserType GetLaserType()
        {
            ImagingSystem.LaserType result = ImagingSystem.LaserType.None;

            if (_ImageInfo == null)
                return result;

            result = (ImagingSystem.LaserType)_ImageInfo.MixChannel.LightSource;
            if (result == ImagingSystem.LaserType.None)
            {
                int nChannelCount = 0;
                if (_ImageInfo.LaserAIntensity > 0)
                {
                    result = ImagingSystem.LaserType.LaserA;
                    nChannelCount++;
                }
                if (_ImageInfo.LaserBIntensity > 0)
                {
                    result = ImagingSystem.LaserType.LaserB;
                    nChannelCount++;
                }
                if (_ImageInfo.LaserCIntensity > 0)
                {
                    result = ImagingSystem.LaserType.LaserC;
                    nChannelCount++;
                }
                if (_ImageInfo.LaserDIntensity > 0)
                {
                    result = ImagingSystem.LaserType.LaserD;
                    nChannelCount++;
                }
                if (nChannelCount > 1)
                {
                    // Not a single channel scan
                    // Probably an extracted channel using an older version of the software
                    // Assume laser type not found.
                    //
                    result = ImagingSystem.LaserType.None;
                }
            }
            return result;
        }

        public string FirmwareVersion
        {
            get
            {
                string fwVersion = string.Empty;
                if (_ImageInfo != null)
                {
                    if (_ImageInfo.IsScannedImage)
                    {
                        fwVersion = _ImageInfo.FWVersion;
                    }
                    else
                    {
                        fwVersion = _ImageInfo.CameraFirmware;
                    }
                }
                return fwVersion;
            }
        }

        public string EdrDynamicBit
        {
            get
            {
                string strDynamicBit = string.Empty;
                if (_ImageInfo != null)
                {
                    if (ImageInfo.DynamicBit > 16)
                    {
                        //int mod = _ImageInfo.DynamicBit % 16;
                        int mod = _ImageInfo.DynamicBit % 16;
                        if (_ImageInfo.EdrBitDepth > 0)
                        {
                            // EDR image always compressed using 24-bit compression; display the actual bit depth
                            mod = _ImageInfo.EdrBitDepth % 16;
                        }
                        strDynamicBit = mod.ToString();
                    }
                }
                return strDynamicBit;
            }
        }

        public bool IsSmartScan
        {
            get
            {
                bool bResult = false;
                if (_ImageInfo != null)
                {
                    if (_ImageInfo.IsScannedImage)
                    {
                        if (!string.IsNullOrEmpty(_ImageInfo.ScanType))
                        {
                            if (_ImageInfo.ScanType.ToLower().Contains("smartscan"))
                            {
                                bResult = true;
                            }
                        }
                    }
                }
                return bResult;
            }
        }
        #endregion

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

        //public bool IsCropping
        //{
        //    get { return _IsCropping; }
        //    set
        //    {
        //        if (_IsCropping != value)
        //        {
        //            if (Image != null)
        //            {
        //                if (Image.Format.BitsPerPixel == 32)
        //                {
        //                    string caption = "Image type not supported...";
        //                    string message = "This operation is current not supported for 32-bit image.";
        //                    Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
        //                    return;
        //                }
        //            }

        //            _IsCropping = value;
        //            RaisePropertyChanged("IsCropping");
        //            RaisePropertyChanged("IsAutoExposureToBand");
        //        }
        //    }
        //}

        /*public bool IsTriggerGetCropRect
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
        }*/

        /*public Rect CropRect
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
        }*/


        /*public string CropX
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
        }*/

        /*public string CropY
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
        }*/

        /*public string CropWidth
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
        }*/

        /*public string CropHeight
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
        }*/

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
                if (FilePath == null || string.IsNullOrEmpty(_filePath))
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
                _DisplayImage = value;
                RaisePropertyChanged("DisplayImage");

                //if (IsAutoContrast)
                //{
                //    RaisePropertyChanged("BlackValue");
                //    RaisePropertyChanged("WhiteValue");
                //    RaisePropertyChanged("GammaValue");
                //}

                // In extreme situations force a garbage collection to free 
                // up memory as quickly as possible.
                //if (_DisplayImage != null &&
                //    _DisplayImage.PixelHeight * _DisplayImage.PixelWidth > (10000 * 10000))
                //{
                //    GC.Collect();
                //}
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

        #region IsRgbImage

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

        public bool Is64bitImage
        {
            get
            {
                bool bIs64bit = false;
                if (_Image != null)
                {
                    bIs64bit = (_Image.Format.BitsPerPixel == 64);
                }
                return bIs64bit;
            }
        }

        public bool Is4ChannelImage
        {
            get
            {
                bool bIs4ChannelImage = false;
                if (_Image != null)
                {
                    bIs4ChannelImage = (_Image.Format.BitsPerPixel == 32 || _Image.Format.BitsPerPixel == 64);
                }
                return bIs4ChannelImage;
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
        /*public bool IsRgbImageCropped
        {
            get { return _IsRgbImageCropped; }
            set
            {
                if (_IsRgbImageCropped != value)
                {
                    _IsRgbImageCropped = value;
                    RaisePropertyChanged("IsRgbImageCropped");
                }
            }
        }*/

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
                    _LargeChange = (MaxPixelValue > 255) ? ((MaxPixelValue + 1) / 256) : 20;
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
                    _SmallChange = (MaxPixelValue > 255) ? ((MaxPixelValue + 1) / 512) : 10;
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

        #region public ImageChannelType SelectedChannel
        public ImageChannelType SelectedChannel
        {
            get
            {
                ImageChannelType ictSelectedCh = ImageChannelType.Red;

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
                        RaisePropertyChanged("SelectedChannel");
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

        /*public bool IsRedChannelChecked
        {
            get { return _IsRedChannelChecked; }
            set
            {
                if (_IsRedChannelChecked != value)
                {
                    _IsRedChannelChecked = value;
                    if (_IsRedChannelChecked)
                    {
                        // R-G-B buttons are mutually exclusive.
                        if (IsGreenChannelChecked)
                        {
                            IsGreenChannelChecked = false;
                        }
                        if (IsBlueChannelChecked)
                        {
                            IsBlueChannelChecked = false;
                        }

                        _SelectedColorChannel = ImageChannelType.Red;

                        //this.CurrentDisplayChannel = ColorTonalAdjustment.Channels.DisplayChannelRed;
                        //this.updateImage.CurrentChannel = this.CurrentDisplayChannel;

                        // update display image
                        //BlackWhiteGammaCommand();
                    }
                    else
                    {
                        SetOverall();
                    }

                    RaisePropertyChanged("IsRedChannelChecked");
                }
            }
        }*/

        /*public bool IsGreenChannelChecked
        {
            get { return _IsGreenChannelChecked; }
            set
            {
                if (_IsGreenChannelChecked != value)
                {
                    // R-G-B buttons are mutually exclusive.
                    _IsGreenChannelChecked = value;
                    if (_IsGreenChannelChecked)
                    {
                        if (IsRedChannelChecked)
                        {
                            IsRedChannelChecked = false;
                        }
                        if (IsBlueChannelChecked)
                        {
                            IsBlueChannelChecked = false;
                        }

                        _SelectedColorChannel = ImageChannelType.Green;

                        //this.CurrentDisplayChannel = ColorTonalAdjustment.Channels.DisplayChannelGreen;
                        //this.updateImage.CurrentChannel = this.CurrentDisplayChannel;

                        // Update display image
                        //BlackWhiteGammaCommand();
                    }
                    else
                    {
                        SetOverall();
                    }

                    RaisePropertyChanged("IsGreenChannelChecked");
                }
            }

        }*/

        /*public bool IsBlueChannelChecked
        {
            get { return _IsBlueChannelChecked; }
            set
            {
                if (_IsBlueChannelChecked != value)
                {
                    _IsBlueChannelChecked = value;
                    if (_IsBlueChannelChecked)
                    {
                        // R-G-B buttons are mutually exclusive.
                        if (IsRedChannelChecked)
                        {
                            IsRedChannelChecked = false;
                        }
                        if (IsGreenChannelChecked)
                        {
                            IsGreenChannelChecked = false;
                        }

                        _SelectedColorChannel = ImageChannelType.Blue;

                        //this.CurrentDisplayChannel = ColorTonalAdjustment.Channels.DisplayChannelBlue;
                        //this.updateImage.CurrentChannel = this.CurrentDisplayChannel;

                        // Update display image
                        //BlackWhiteGammaCommand();
                    }
                    else
                    {
                        SetOverall();
                    }

                    RaisePropertyChanged("IsBlueChannelChecked");
                }
            }
        }*/

        /*public bool IsMixChannelsChecked
        {
            get { return _IsMixChannelsChecked; }
            set
            {
                if (_IsMixChannelsChecked != value)
                {
                    bool bIsComposite = value;
                    if (!IsRedChannelChecked && !IsGreenChannelChecked && !IsBlueChannelChecked)
                    {
                        _IsMixChannelsChecked = true;
                        bIsComposite = true;
                    }

                    _IsMixChannelsChecked = bIsComposite;
                    //this.updateImage.DisplayCompositeImage = bIsComposite;

                    //if (CurrentDisplayChannel == ColorTonalAdjustment.Channels.DisplayChannelOverall &&
                    //    _Owner.ImageGalleryWDG.ActiveDocument.iImage.Channels > 1)
                    //{
                    //    // do nothing...
                    //}
                    //else
                    //{
                    //    // Update display image
                    //    BlackWhiteGammaCommand();
                    //}

                    RaisePropertyChanged("IsMixChannelsChecked");
                }
            }
        }*/

        /*private bool SetOverall()
        {
            if (IsRedChannelChecked || IsGreenChannelChecked || IsBlueChannelChecked)
            {
                return false;
            }

            IsMixChannelsChecked = true;

            //this.updateImage.DisplayCompositeImage = true;
            //this.updateImage.CurrentChannel = ColorTonalAdjustment.Channels.DisplayChannelOverall;

            //this.CurrentDisplayChannel = ColorTonalAdjustment.Channels.DisplayChannelOverall;

            if (IsAutoContrast)
            {
                IsAutoContrast = false;
            }

            //BlackWhiteGammaCommand();

            return true;
        }*/


        #region public int BlackValue
        private int _BlackValue = 0;
        public int BlackValue
        {
            get
            {
                if (_Image != null && _ImageInfo != null)
                {
                    //if (_ImageInfo.IsDisplayOverAll)
                    if (!IsRgbImage || _ImageInfo.ContrastChannelFlags == ImageChannelFlag.None)
                    {
                        _BlackValue = _ImageInfo.MixChannel.BlackValue;
                    }
                    else
                    {
                        if (_ImageInfo.IsContrastRedChannel && _ImageInfo.IsDisplayRedChannel)
                            _BlackValue = _ImageInfo.RedChannel.BlackValue;
                        else if (_ImageInfo.IsContrastGreenChannel && _ImageInfo.IsDisplayGreenChannel)
                            _BlackValue = _ImageInfo.GreenChannel.BlackValue;
                        else if (_ImageInfo.IsContrastBlueChannel && _ImageInfo.IsDisplayBlueChannel)
                            _BlackValue = _ImageInfo.BlueChannel.BlackValue;
                        else if (_ImageInfo.IsContrastGrayChannel && _ImageInfo.IsDisplayGrayChannel)
                            _BlackValue = _ImageInfo.GrayChannel.BlackValue;
                        else if (_ImageInfo.ContrastChannelFlags == ImageChannelFlag.None)
                            _BlackValue = _ImageInfo.MixChannel.BlackValue;
                    }
                }
                return _BlackValue;
            }   // get
            set
            {
                if (_Image != null && _ImageInfo != null)
                {
                    // Do not allow the black value to be >= to the white value.
                    if (value >= WhiteValue && WhiteValue >= 1)
                    {
                        value = WhiteValue - 1;
                    }

                    //if (_ImageInfo.IsDisplayOverAll)
                    if (!IsRgbImage || _ImageInfo.ContrastChannelFlags == ImageChannelFlag.None)
                    {
                        if (_ImageInfo.MixChannel.BlackValue != value)
                        {
                            _ImageInfo.MixChannel.BlackValue = value;
                            IsContrastValueChanged = true;
                        }
                    }
                    else
                    {
                        if (_ImageInfo.IsContrastRedChannel && _ImageInfo.IsDisplayRedChannel)
                        {
                            if (_ImageInfo.RedChannel.BlackValue != value)
                            {
                                _ImageInfo.RedChannel.BlackValue = value;
                                IsContrastValueChanged = true;
                            }
                        }
                        if (_ImageInfo.IsContrastGreenChannel && _ImageInfo.IsDisplayGreenChannel)
                        {
                            if (_ImageInfo.GreenChannel.BlackValue != value)
                            {
                                _ImageInfo.GreenChannel.BlackValue = value;
                                IsContrastValueChanged = true;
                            }
                        }
                        if (_ImageInfo.IsContrastBlueChannel && _ImageInfo.IsDisplayBlueChannel)
                        {
                            if (_ImageInfo.BlueChannel.BlackValue != value)
                            {
                                _ImageInfo.BlueChannel.BlackValue = value;
                                IsContrastValueChanged = true;
                            }
                        }
                        if (_ImageInfo.IsContrastGrayChannel && _ImageInfo.IsDisplayGrayChannel)
                        {
                            if (_ImageInfo.GrayChannel.BlackValue != value)
                            {
                                _ImageInfo.GrayChannel.BlackValue = value;
                                IsContrastValueChanged = true;
                            }
                        }
                        if (_ImageInfo.ContrastChannelFlags == ImageChannelFlag.None)
                        {
                            if (_ImageInfo.MixChannel.BlackValue != value)
                            {
                                _ImageInfo.MixChannel.BlackValue = value;
                                IsContrastValueChanged = true;
                            }
                        }
                    }
                    RaisePropertyChanged("BlackValue");
                }
            }   // set

        }
        #endregion

        #region public int WhiteValue
        private int _WhiteValue = 65535;
        public int WhiteValue
        {
            get
            {
                if (_Image != null && _ImageInfo != null)
                {
                    //if (_ImageInfo.IsDisplayOverAll)
                    if (!IsRgbImage || _ImageInfo.ContrastChannelFlags == ImageChannelFlag.None)
                    {
                        _WhiteValue = _ImageInfo.MixChannel.WhiteValue;
                    }
                    else
                    {
                        if (_ImageInfo.IsContrastRedChannel && _ImageInfo.IsDisplayRedChannel)
                            _WhiteValue = _ImageInfo.RedChannel.WhiteValue;
                        else if (_ImageInfo.IsContrastGreenChannel && _ImageInfo.IsDisplayGreenChannel)
                            _WhiteValue = _ImageInfo.GreenChannel.WhiteValue;
                        else if (_ImageInfo.IsContrastBlueChannel && _ImageInfo.IsDisplayBlueChannel)
                            _WhiteValue = _ImageInfo.BlueChannel.WhiteValue;
                        else if (_ImageInfo.IsContrastGrayChannel && _ImageInfo.IsDisplayGrayChannel)
                            _WhiteValue = _ImageInfo.GrayChannel.WhiteValue;
                        else if (_ImageInfo.ContrastChannelFlags == ImageChannelFlag.None)
                            _WhiteValue = _ImageInfo.MixChannel.WhiteValue;
                    }
                }
                return _WhiteValue;
            }
            set
            {
                if (_Image != null && _ImageInfo != null)
                {
                    // Do not allow the white value to be <= the black value
                    if (value <= BlackValue)
                    {
                        value = BlackValue + 1;
                    }

                    //if (_ImageInfo.IsDisplayOverAll)
                    if (!IsRgbImage || _ImageInfo.ContrastChannelFlags == ImageChannelFlag.None)
                    {
                        if (_ImageInfo.MixChannel.WhiteValue != value)
                        {
                            _ImageInfo.MixChannel.WhiteValue = value;
                            IsContrastValueChanged = true;
                            //OnPropertyChanged("WhiteValue");
                        }
                    }
                    else
                    {
                        if (_ImageInfo.IsContrastRedChannel && _ImageInfo.IsDisplayRedChannel)
                        {
                            if (_ImageInfo.RedChannel.WhiteValue != value)
                            {
                                _ImageInfo.RedChannel.WhiteValue = value;
                                IsContrastValueChanged = true;
                            }
                        }
                        if (_ImageInfo.IsContrastGreenChannel && _ImageInfo.IsDisplayGreenChannel)
                        {
                            if (_ImageInfo.GreenChannel.WhiteValue != value)
                            {
                                _ImageInfo.GreenChannel.WhiteValue = value;
                                IsContrastValueChanged = true;
                            }
                        }
                        if (_ImageInfo.IsContrastBlueChannel && _ImageInfo.IsDisplayBlueChannel)
                        {
                            if (_ImageInfo.BlueChannel.WhiteValue != value)
                            {
                                _ImageInfo.BlueChannel.WhiteValue = value;
                                IsContrastValueChanged = true;
                            }
                        }
                        if (_ImageInfo.IsContrastGrayChannel && _ImageInfo.IsDisplayGrayChannel)
                        {
                            if (_ImageInfo.GrayChannel.WhiteValue != value)
                            {
                                _ImageInfo.GrayChannel.WhiteValue = value;
                                IsContrastValueChanged = true;
                            }
                        }
                        if (_ImageInfo.ContrastChannelFlags == ImageChannelFlag.None)
                        {
                            if (_ImageInfo.MixChannel.WhiteValue != value)
                            {
                                _ImageInfo.MixChannel.WhiteValue = value;
                                IsContrastValueChanged = true;
                            }
                        }
                    }
                    RaisePropertyChanged("WhiteValue");
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
                    //if (_ImageInfo.IsDisplayOverAll)
                    if (!IsRgbImage || _ImageInfo.ContrastChannelFlags == ImageChannelFlag.None)
                    {
                        dGammaValue = Math.Round(Math.Log10(_ImageInfo.MixChannel.GammaValue), 3);
                    }
                    else
                    {
                        if (_ImageInfo.IsContrastRedChannel && _ImageInfo.IsDisplayRedChannel)
                            dGammaValue = Math.Round(Math.Log10(_ImageInfo.RedChannel.GammaValue), 3);
                        else if (_ImageInfo.IsContrastGreenChannel && _ImageInfo.IsDisplayGreenChannel)
                            dGammaValue = Math.Round(Math.Log10(_ImageInfo.GreenChannel.GammaValue), 3);
                        else if (_ImageInfo.IsContrastBlueChannel && _ImageInfo.IsDisplayBlueChannel)
                            dGammaValue = Math.Round(Math.Log10(_ImageInfo.BlueChannel.GammaValue), 3);
                        else if (_ImageInfo.IsContrastGrayChannel && _ImageInfo.IsDisplayGrayChannel)
                            dGammaValue = Math.Round(Math.Log10(_ImageInfo.GrayChannel.GammaValue), 3);
                        else if (_ImageInfo.ContrastChannelFlags == ImageChannelFlag.None)
                            dGammaValue = Math.Round(Math.Log10(_ImageInfo.MixChannel.GammaValue), 3);
                    }
                }
                return dGammaValue;
            }
            set
            {
                if (_Image != null && _ImageInfo != null)
                {
                    //if (_ImageInfo.IsDisplayOverAll)
                    if (!IsRgbImage || _ImageInfo.ContrastChannelFlags == ImageChannelFlag.None)
                    {
                        if (_ImageInfo.MixChannel.GammaValue != value)
                        {
                            double dGammaValue = Math.Round(Math.Pow(10, value), 3);    // true gamma value
                            _ImageInfo.MixChannel.GammaValue = dGammaValue;
                            IsContrastValueChanged = true;
                        }
                    }
                    else
                    {
                        if (_ImageInfo.IsContrastRedChannel && _ImageInfo.IsDisplayRedChannel)
                        {
                            if (_ImageInfo.RedChannel.GammaValue != value)
                            {
                                double dGammaValue = Math.Round(Math.Pow(10, value), 3);
                                _ImageInfo.RedChannel.GammaValue = dGammaValue;
                                IsContrastValueChanged = true;
                            }
                        }
                        if (_ImageInfo.IsContrastGreenChannel && _ImageInfo.IsDisplayGreenChannel)
                        {
                            if (_ImageInfo.GreenChannel.GammaValue != value)
                            {
                                double dGammaValue = Math.Round(Math.Pow(10, value), 3);
                                _ImageInfo.GreenChannel.GammaValue = dGammaValue;
                                IsContrastValueChanged = true;
                            }
                        }
                        if (_ImageInfo.IsContrastBlueChannel && _ImageInfo.IsDisplayBlueChannel)
                        {
                            if (_ImageInfo.BlueChannel.GammaValue != value)
                            {
                                double dGammaValue = Math.Round(Math.Pow(10, value), 3);
                                _ImageInfo.BlueChannel.GammaValue = dGammaValue;
                                IsContrastValueChanged = true;
                            }
                        }
                        if (_ImageInfo.IsContrastGrayChannel && _ImageInfo.IsDisplayGrayChannel)
                        {
                            if (_ImageInfo.GrayChannel.GammaValue != value)
                            {
                                double dGammaValue = Math.Round(Math.Pow(10, value), 3);
                                _ImageInfo.GrayChannel.GammaValue = dGammaValue;
                                IsContrastValueChanged = true;
                            }
                        }
                        if (_ImageInfo.ContrastChannelFlags == ImageChannelFlag.None)
                        {
                            if (_ImageInfo.MixChannel.GammaValue != value)
                            {
                                double dGammaValue = Math.Round(Math.Pow(10, value), 3);    // true gamma value
                                _ImageInfo.MixChannel.GammaValue = dGammaValue;
                                IsContrastValueChanged = true;
                            }
                        }
                    }
                    RaisePropertyChanged("GammaValue");
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
                ImageChannelFlag autoContrastFlags = ImageChannelFlag.None;
                if (_Image != null && _ImageInfo != null)
                {
                    //if (_ImageInfo.IsDisplayOverAll)
                    if (_ImageInfo.ContrastChannelFlags == ImageChannelFlag.None)
                    {
                        bIsAutoContrast = _ImageInfo.MixChannel.IsAutoChecked;
                    }
                    else
                    {
                        if (_ImageInfo.IsContrastRedChannel)
                        {
                            bIsAutoContrast = _ImageInfo.RedChannel.IsAutoChecked;
                            if (bIsAutoContrast)
                                autoContrastFlags |= ImageChannelFlag.Red;
                            else
                                autoContrastFlags &= ~ImageChannelFlag.Red;
                        }
                        if (_ImageInfo.IsContrastGreenChannel)
                        {
                            bIsAutoContrast = _ImageInfo.GreenChannel.IsAutoChecked;
                            if (bIsAutoContrast)
                                autoContrastFlags |= ImageChannelFlag.Green;
                            else
                                autoContrastFlags &= ~ImageChannelFlag.Green;
                        }
                        if (_ImageInfo.IsContrastBlueChannel)
                        {
                            bIsAutoContrast = _ImageInfo.BlueChannel.IsAutoChecked;
                            if (bIsAutoContrast)
                                autoContrastFlags |= ImageChannelFlag.Blue;
                            else
                                autoContrastFlags &= ~ImageChannelFlag.Blue;
                        }
                        if (_ImageInfo.IsContrastGrayChannel)
                        {
                            bIsAutoContrast = _ImageInfo.GrayChannel.IsAutoChecked;
                            if (bIsAutoContrast)
                                autoContrastFlags |= ImageChannelFlag.Gray;
                            else
                                autoContrastFlags &= ~ImageChannelFlag.Gray;
                        }
                        else if (_ImageInfo.ContrastChannelFlags == ImageChannelFlag.None)
                        {
                            bIsAutoContrast = _ImageInfo.MixChannel.IsAutoChecked;
                        }
                        if (_ImageInfo.ContrastChannelFlags != ImageChannelFlag.None)
                            bIsAutoContrast = (autoContrastFlags == _ImageInfo.ContrastChannelFlags);
                    }
                }

                return bIsAutoContrast;
            }
            set
            {
                if (_Image != null && _ImageInfo != null)
                {
                    //if (_ImageInfo.IsDisplayOverAll)
                    if (_ImageInfo.ContrastChannelFlags == ImageChannelFlag.None)
                    {
                        #region === Composite/Mix ===
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
                                    if (_ImageInfo.MixChannel.PrevWhiteValue == 0)
                                        _ImageInfo.MixChannel.PrevWhiteValue = MaxWhiteValue;
                                    _ImageInfo.MixChannel.BlackValue = _ImageInfo.MixChannel.PrevBlackValue;
                                    _ImageInfo.MixChannel.WhiteValue = _ImageInfo.MixChannel.PrevWhiteValue;
                                    _ImageInfo.MixChannel.GammaValue = _ImageInfo.MixChannel.PrevGammaValue;
                                    RaisePropertyChanged("BlackValue");
                                    RaisePropertyChanged("WhiteValue");
                                    RaisePropertyChanged("GammaValue");
                                }
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        if (_ImageInfo.IsContrastRedChannel)
                        {
                            #region === Red channel ===
                            if (_ImageInfo.RedChannel.IsAutoChecked != value)
                            {
                                _ImageInfo.RedChannel.IsAutoChecked = value;
                                //OnPropertyChanged("IsAutoContrast");

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
                                        if (_ImageInfo.RedChannel.PrevWhiteValue == 0)
                                            _ImageInfo.RedChannel.PrevWhiteValue = MaxWhiteValue;
                                        _ImageInfo.RedChannel.BlackValue = _ImageInfo.RedChannel.PrevBlackValue;
                                        _ImageInfo.RedChannel.WhiteValue = _ImageInfo.RedChannel.PrevWhiteValue;
                                        _ImageInfo.RedChannel.GammaValue = _ImageInfo.RedChannel.PrevGammaValue;
                                        RaisePropertyChanged("BlackValue");
                                        RaisePropertyChanged("WhiteValue");
                                        RaisePropertyChanged("GammaValue");
                                    }
                                }
                            }
                            #endregion
                        }
                        if (_ImageInfo.IsContrastGreenChannel)
                        {
                            #region === Green channel ===
                            if (_ImageInfo.GreenChannel.IsAutoChecked != value)
                            {
                                _ImageInfo.GreenChannel.IsAutoChecked = value;
                                //OnPropertyChanged("IsAutoContrast");

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
                                        if (_ImageInfo.GreenChannel.PrevWhiteValue == 0)
                                            _ImageInfo.GreenChannel.PrevWhiteValue = MaxWhiteValue;
                                        _ImageInfo.GreenChannel.BlackValue = _ImageInfo.GreenChannel.PrevBlackValue;
                                        _ImageInfo.GreenChannel.WhiteValue = _ImageInfo.GreenChannel.PrevWhiteValue;
                                        _ImageInfo.GreenChannel.GammaValue = _ImageInfo.GreenChannel.PrevGammaValue;
                                        RaisePropertyChanged("BlackValue");
                                        RaisePropertyChanged("WhiteValue");
                                        RaisePropertyChanged("GammaValue");
                                    }
                                }
                            }
                            #endregion
                        }
                        if (_ImageInfo.IsContrastBlueChannel)
                        {
                            #region === Blue channel ===
                            if (_ImageInfo.BlueChannel.IsAutoChecked != value)
                            {
                                _ImageInfo.BlueChannel.IsAutoChecked = value;
                                //OnPropertyChanged("IsAutoContrast");

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
                                        if (_ImageInfo.BlueChannel.PrevWhiteValue == 0)
                                            _ImageInfo.BlueChannel.PrevWhiteValue = MaxWhiteValue;
                                        _ImageInfo.BlueChannel.BlackValue = _ImageInfo.BlueChannel.PrevBlackValue;
                                        _ImageInfo.BlueChannel.WhiteValue = _ImageInfo.BlueChannel.PrevWhiteValue;
                                        _ImageInfo.BlueChannel.GammaValue = _ImageInfo.BlueChannel.PrevGammaValue;
                                        RaisePropertyChanged("BlackValue");
                                        RaisePropertyChanged("WhiteValue");
                                        RaisePropertyChanged("GammaValue");
                                    }
                                }
                            }
                            #endregion
                        }
                        if (_ImageInfo.IsContrastGrayChannel)
                        {
                            #region === Gray channel ===
                            if (_ImageInfo.GrayChannel.IsAutoChecked != value)
                            {
                                _ImageInfo.GrayChannel.IsAutoChecked = value;
                                //OnPropertyChanged("IsAutoContrast");

                                if (_ImageInfo.GrayChannel.IsAutoChecked)
                                {
                                    // Save previous Black, White, and Gamma values
                                    _ImageInfo.GrayChannel.PrevBlackValue = _ImageInfo.GrayChannel.BlackValue;
                                    _ImageInfo.GrayChannel.PrevWhiteValue = _ImageInfo.GrayChannel.WhiteValue;
                                    _ImageInfo.GrayChannel.PrevGammaValue = _ImageInfo.GrayChannel.GammaValue;
                                    IsManualContrast = false;
                                }
                                else
                                {
                                    if (!IsManualContrast)
                                    {
                                        // Restore previous Black, White, and Gamma values
                                        if (_ImageInfo.GrayChannel.PrevWhiteValue == 0)
                                            _ImageInfo.GrayChannel.PrevWhiteValue = MaxWhiteValue;
                                        _ImageInfo.GrayChannel.BlackValue = _ImageInfo.GrayChannel.PrevBlackValue;
                                        _ImageInfo.GrayChannel.WhiteValue = _ImageInfo.GrayChannel.PrevWhiteValue;
                                        _ImageInfo.GrayChannel.GammaValue = _ImageInfo.GrayChannel.PrevGammaValue;
                                        RaisePropertyChanged("BlackValue");
                                        RaisePropertyChanged("WhiteValue");
                                        RaisePropertyChanged("GammaValue");
                                    }
                                }
                            }
                            #endregion
                        }
                        RaisePropertyChanged("IsAutoContrast");
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
                ImageChannelFlag invertFlags = ImageChannelFlag.None;
                if (_Image != null && _ImageInfo != null)
                {
                    //if (_ImageInfo.IsDisplayOverAll)
                    if (_ImageInfo.ContrastChannelFlags == ImageChannelFlag.None)
                    {
                        bIsInvert = _ImageInfo.MixChannel.IsInvertChecked;
                    }
                    else
                    {
                        if (_ImageInfo.IsContrastRedChannel)
                        {
                            bIsInvert = _ImageInfo.RedChannel.IsInvertChecked;
                            if (bIsInvert)
                                invertFlags |= ImageChannelFlag.Red;
                            else
                                invertFlags &= ~ImageChannelFlag.Red;
                        }
                        if (_ImageInfo.IsContrastGreenChannel)
                        {
                            bIsInvert = _ImageInfo.GreenChannel.IsInvertChecked;
                            if (bIsInvert)
                                invertFlags |= ImageChannelFlag.Green;
                            else
                                invertFlags &= ~ImageChannelFlag.Green;
                        }
                        if (_ImageInfo.IsContrastBlueChannel)
                        {
                            bIsInvert = _ImageInfo.BlueChannel.IsInvertChecked;
                            if (bIsInvert)
                                invertFlags |= ImageChannelFlag.Blue;
                            else
                                invertFlags &= ~ImageChannelFlag.Blue;
                        }
                        if (_ImageInfo.IsContrastGrayChannel)
                        {
                            bIsInvert = _ImageInfo.GrayChannel.IsInvertChecked;
                            if (bIsInvert)
                                invertFlags |= ImageChannelFlag.Gray;
                            else
                                invertFlags &= ~ImageChannelFlag.Gray;
                        }
                        else if (_ImageInfo.ContrastChannelFlags == ImageChannelFlag.None)
                        {
                            bIsInvert = _ImageInfo.MixChannel.IsInvertChecked;
                        }
                        if (_ImageInfo.ContrastChannelFlags != ImageChannelFlag.None)
                            bIsInvert = (invertFlags == _ImageInfo.ContrastChannelFlags);
                    }
                }
                return bIsInvert;
            }   // get
            set
            {
                if (_Image != null && _ImageInfo != null)
                {
                    //if (_ImageInfo.IsDisplayOverAll)
                    if (_ImageInfo.ContrastChannelFlags == ImageChannelFlag.None)
                    {
                        if (_ImageInfo.MixChannel.IsInvertChecked != value)
                        {
                            //_IsInvertChecked = value;
                            _ImageInfo.MixChannel.IsInvertChecked = value;
                        }
                    }
                    else
                    {
                        if (_ImageInfo.IsContrastRedChannel)
                        {
                            if (_ImageInfo.RedChannel.IsInvertChecked != value)
                            {
                                //_IsInvertChecked = value;
                                _ImageInfo.RedChannel.IsInvertChecked = value;
                            }
                        }
                        if (_ImageInfo.IsContrastGreenChannel)
                        {
                            if (_ImageInfo.GreenChannel.IsInvertChecked != value)
                            {
                                //_IsInvertChecked = value;
                                _ImageInfo.GreenChannel.IsInvertChecked = value;
                            }
                        }
                        if (_ImageInfo.IsContrastBlueChannel)
                        {
                            if (_ImageInfo.BlueChannel.IsInvertChecked != value)
                            {
                                //_IsInvertChecked = value;
                                _ImageInfo.BlueChannel.IsInvertChecked = value;
                            }
                        }
                        if (_ImageInfo.IsContrastGrayChannel)
                        {
                            if (_ImageInfo.GrayChannel.IsInvertChecked != value)
                            {
                                //_IsInvertChecked = value;
                                _ImageInfo.GrayChannel.IsInvertChecked = value;
                            }
                        }
                        else if (_ImageInfo.ContrastChannelFlags == ImageChannelFlag.None)
                        {
                            if (_ImageInfo.MixChannel.IsInvertChecked != value)
                            {
                                //_IsInvertChecked = value;
                                _ImageInfo.MixChannel.IsInvertChecked = value;
                            }
                        }
                    }
                    RaisePropertyChanged("IsInvert");
                }
            }   // set
        }
        #endregion

        #region public bool IsSaturation
        /// <summary>
        /// Saturation flag is now 'global', no longer image color channel specific.
        /// </summary>
        public bool IsSaturation
        {
            get
            {
                bool bIsSaturation = false;
                if (_Image != null && _ImageInfo != null)
                {
                    bIsSaturation = _ImageInfo.IsSaturationChecked;
                }
                return bIsSaturation;
            }
            set
            {
                if (_Image != null && _ImageInfo != null)
                {
                    _ImageInfo.IsSaturationChecked = value;
                    RaisePropertyChanged("IsSaturation");
                }
            }

        }
        #endregion

        #region Color channel selection

        public bool IsContrastRedChannel
        {
            get
            {
                bool bIsContrastRedChannel = false;
                if (_ImageInfo != null)
                    bIsContrastRedChannel = _ImageInfo.IsContrastRedChannel;
                return bIsContrastRedChannel;
            }
            set
            {
                _ImageInfo.IsContrastRedChannel = value;
                RaisePropertyChanged("IsContrastRedChannel");
                RaisePropertyChanged("BlackValue");
                RaisePropertyChanged("WhiteValue");
                RaisePropertyChanged("GammaValue");
                RaisePropertyChanged("IsAutoContrast");
                RaisePropertyChanged("IsInvert");
                if (_ImageInfo.IsContrastRedChannel)
                {
                    if (!_ImageInfo.IsDisplayRedChannel)
                    {
                        _ImageInfo.IsDisplayRedChannel = true;
                        RaisePropertyChanged("IsDisplayRedChannel");
                    }
                }
                else 
                {
                    _ImageInfo.IsDisplayOverAll = false;
                    RaisePropertyChanged("IsDisplayOverAll");
                }
            }
        }
        public bool IsContrastGreenChannel
        {
            get
            {
                bool bIsContrastGreenChannel = false;
                if (_ImageInfo != null)
                    bIsContrastGreenChannel = _ImageInfo.IsContrastGreenChannel;
                return bIsContrastGreenChannel;
            }
            set
            {
                _ImageInfo.IsContrastGreenChannel = value;
                RaisePropertyChanged("IsContrastGreenChannel");
                RaisePropertyChanged("BlackValue");
                RaisePropertyChanged("WhiteValue");
                RaisePropertyChanged("GammaValue");
                RaisePropertyChanged("IsAutoContrast");
                RaisePropertyChanged("IsInvert");
                if (_ImageInfo.IsContrastGreenChannel)
                {
                    if (!_ImageInfo.IsDisplayGreenChannel)
                    {
                        _ImageInfo.IsDisplayGreenChannel = true;
                        RaisePropertyChanged("IsDisplayGreenChannel");
                    }
                }
                else
                {
                    _ImageInfo.IsDisplayOverAll = false;
                    RaisePropertyChanged("IsDisplayOverAll");
                }
            }
        }
        public bool IsContrastBlueChannel
        {
            get
            {
                bool bIsContrastBlueChannel = false;
                if (_ImageInfo != null)
                    bIsContrastBlueChannel = _ImageInfo.IsContrastBlueChannel;
                return bIsContrastBlueChannel;
            }
            set
            {
                _ImageInfo.IsContrastBlueChannel = value;
                RaisePropertyChanged("IsContrastBlueChannel");
                RaisePropertyChanged("BlackValue");
                RaisePropertyChanged("WhiteValue");
                RaisePropertyChanged("GammaValue");
                RaisePropertyChanged("IsAutoContrast");
                RaisePropertyChanged("IsInvert");
                if (_ImageInfo.IsContrastBlueChannel)
                {
                    if (!_ImageInfo.IsDisplayBlueChannel)
                    {
                        _ImageInfo.IsDisplayBlueChannel = true;
                        RaisePropertyChanged("IsDisplayBlueChannel");
                    }
                }
                else
                {
                    _ImageInfo.IsDisplayOverAll = false;
                    RaisePropertyChanged("IsDisplayOverAll");
                }
            }
        }
        public bool IsContrastGrayChannel
        {
            get
            {
                bool bIsContrastGrayChannel = false;
                if (_ImageInfo != null)
                    bIsContrastGrayChannel = _ImageInfo.IsContrastGrayChannel;
                return bIsContrastGrayChannel;
            }
            set
            {
                _ImageInfo.IsContrastGrayChannel = value;
                RaisePropertyChanged("IsContrastGrayChannel");
                RaisePropertyChanged("BlackValue");
                RaisePropertyChanged("WhiteValue");
                RaisePropertyChanged("GammaValue");
                RaisePropertyChanged("IsAutoContrast");
                RaisePropertyChanged("IsInvert");
                if (_ImageInfo.IsContrastGrayChannel)
                {
                    if (!_ImageInfo.IsDisplayGrayChannel)
                    {
                        _ImageInfo.IsDisplayGrayChannel = true;
                        RaisePropertyChanged("IsDisplayGrayChannel");
                    }
                }
                else
                {
                    _ImageInfo.IsDisplayOverAll = false;
                    RaisePropertyChanged("IsDisplayOverAll");
                }
            }
        }

        private bool _IsContrastValueChanged = false;
        public bool IsContrastValueChanged
        {
            get { return _IsContrastValueChanged; }
            set
            {
                if (_IsContrastValueChanged != value)
                {
                    _IsContrastValueChanged = value;
                    RaisePropertyChanged("IsContrastValueChanged");
                }
            }
        }

        public bool IsDisplayRedChannel
        {
            get
            {
                bool bIsDisplayRedChannel = false;
                if (_ImageInfo != null)
                    bIsDisplayRedChannel = _ImageInfo.IsDisplayRedChannel;
                return bIsDisplayRedChannel;
            }
            set
            {
                _ImageInfo.IsDisplayRedChannel = value;
                RaisePropertyChanged("IsDisplayRedChannel");
                if (!_ImageInfo.IsDisplayRedChannel)
                {
                    _ImageInfo.IsContrastRedChannel = false;
                    RaisePropertyChanged("IsContrastRedChannel");
                    _ImageInfo.IsDisplayOverAll = false;
                    RaisePropertyChanged("IsDisplayOverAll");
                }
                else
                {
                    if (!_ImageInfo.IsContrastRedChannel)
                    {
                        _ImageInfo.IsContrastRedChannel = true;
                        RaisePropertyChanged("IsContrastRedChannel");
                    }
                }
                RaisePropertyChanged("BlackValue");
                RaisePropertyChanged("WhiteValue");
                RaisePropertyChanged("GammaValue");
                RaisePropertyChanged("IsAutoContrast");
                RaisePropertyChanged("IsInvert");
                UpdateDisplayImage();
            }
        }
        public bool IsDisplayGreenChannel
        {
            get
            {
                bool bIsDisplayGreenChannel = false;
                if (_ImageInfo != null)
                    bIsDisplayGreenChannel = _ImageInfo.IsDisplayGreenChannel;
                return bIsDisplayGreenChannel;
            }
            set
            {
                _ImageInfo.IsDisplayGreenChannel = value;
                RaisePropertyChanged("IsDisplayGreenChannel");
                if (!_ImageInfo.IsDisplayGreenChannel)
                {
                    _ImageInfo.IsContrastGreenChannel = false;
                    RaisePropertyChanged("IsContrastGreenChannel");
                    _ImageInfo.IsDisplayOverAll = false;
                    RaisePropertyChanged("IsDisplayOverAll");
                }
                else
                {
                    _ImageInfo.IsContrastGreenChannel = true;
                    RaisePropertyChanged("IsContrastGreenChannel");
                }
                RaisePropertyChanged("BlackValue");
                RaisePropertyChanged("WhiteValue");
                RaisePropertyChanged("GammaValue");
                RaisePropertyChanged("IsAutoContrast");
                RaisePropertyChanged("IsInvert");
                UpdateDisplayImage();
            }
        }
        public bool IsDisplayBlueChannel
        {
            get
            {
                bool bIsDisplayBlueChannel = false;
                if (_ImageInfo != null)
                    bIsDisplayBlueChannel = _ImageInfo.IsDisplayBlueChannel;
                return bIsDisplayBlueChannel;
            }
            set
            {
                _ImageInfo.IsDisplayBlueChannel = value;
                RaisePropertyChanged("IsDisplayBlueChannel");
                if (!_ImageInfo.IsDisplayBlueChannel)
                {
                    _ImageInfo.IsContrastBlueChannel = false;
                    RaisePropertyChanged("IsContrastBlueChannel");
                    _ImageInfo.IsDisplayOverAll = false;
                    RaisePropertyChanged("IsDisplayOverAll");
                }
                else
                {
                    _ImageInfo.IsContrastBlueChannel = true;
                    RaisePropertyChanged("IsContrastBlueChannel");
                }
                RaisePropertyChanged("BlackValue");
                RaisePropertyChanged("WhiteValue");
                RaisePropertyChanged("GammaValue");
                RaisePropertyChanged("IsAutoContrast");
                RaisePropertyChanged("IsInvert");
                UpdateDisplayImage();
            }
        }
        public bool IsDisplayGrayChannel
        {
            get
            {
                bool bIsDisplayGrayChannel = false;
                if (_ImageInfo != null)
                    bIsDisplayGrayChannel = _ImageInfo.IsDisplayGrayChannel;
                return bIsDisplayGrayChannel;
            }
            set
            {
                _ImageInfo.IsDisplayGrayChannel = value;
                RaisePropertyChanged("IsDisplayGrayChannel");
                if (!_ImageInfo.IsDisplayGrayChannel)
                {
                    _ImageInfo.IsContrastGrayChannel = false;
                    RaisePropertyChanged("IsContrastGrayChannel");
                    _ImageInfo.IsDisplayOverAll = false;
                    RaisePropertyChanged("IsDisplayOverAll");
                }
                else
                {
                    _ImageInfo.IsContrastGrayChannel = true;
                    RaisePropertyChanged("IsContrastGrayChannel");
                }
                RaisePropertyChanged("BlackValue");
                RaisePropertyChanged("WhiteValue");
                RaisePropertyChanged("GammaValue");
                RaisePropertyChanged("IsAutoContrast");
                RaisePropertyChanged("IsInvert");
                UpdateDisplayImage();
            }
        }
        public bool IsDisplayOverAll
        {
            get
            {
                bool bIsDisplayOverAll = false;
                if (_ImageInfo != null)
                    bIsDisplayOverAll = _ImageInfo.IsDisplayOverAll;
                return bIsDisplayOverAll;
            }
            set
            {
                _ImageInfo.IsDisplayOverAll = value;
                RaisePropertyChanged("IsDisplayOverAll");
                if (_ImageInfo.IsDisplayOverAll)
                {
                    if (IsRgbImage)
                    {
                        _ImageInfo.IsContrastRedChannel = true;
                        _ImageInfo.IsContrastGreenChannel = true;
                        _ImageInfo.IsContrastBlueChannel = true;
                        _ImageInfo.IsDisplayRedChannel = true;
                        _ImageInfo.IsDisplayGreenChannel = true;
                        _ImageInfo.IsDisplayBlueChannel = true;
                        RaisePropertyChanged("IsContrastRedChannel");
                        RaisePropertyChanged("IsContrastGreenChannel");
                        RaisePropertyChanged("IsContrastBlueChannel");
                        RaisePropertyChanged("IsDisplayRedChannel");
                        RaisePropertyChanged("IsDisplayGreenChannel");
                        RaisePropertyChanged("IsDisplayBlueChannel");

                        if (Is4ChannelImage)
                        {
                            _ImageInfo.IsContrastGrayChannel = true;
                            _ImageInfo.IsDisplayGrayChannel = true;
                            RaisePropertyChanged("IsContrastGrayChannel");
                            RaisePropertyChanged("IsDisplayGrayChannel");
                        }
                    }
                }
                else
                {
                    if (IsRgbImage)
                    {
                        _ImageInfo.IsContrastRedChannel = false;
                        _ImageInfo.IsContrastGreenChannel = false;
                        _ImageInfo.IsContrastBlueChannel = false;
                        _ImageInfo.IsDisplayRedChannel = false;
                        _ImageInfo.IsDisplayGreenChannel = false;
                        _ImageInfo.IsDisplayBlueChannel = false;
                        RaisePropertyChanged("IsContrastRedChannel");
                        RaisePropertyChanged("IsContrastGreenChannel");
                        RaisePropertyChanged("IsContrastBlueChannel");
                        RaisePropertyChanged("IsDisplayRedChannel");
                        RaisePropertyChanged("IsDisplayGreenChannel");
                        RaisePropertyChanged("IsDisplayBlueChannel");

                        if (Is4ChannelImage)
                        {
                            _ImageInfo.IsContrastGrayChannel = false;
                            _ImageInfo.IsDisplayGrayChannel = false;
                            RaisePropertyChanged("IsContrastGrayChannel");
                            RaisePropertyChanged("IsDisplayGrayChannel");
                        }
                    }
                }
                RaisePropertyChanged("BlackValue");
                RaisePropertyChanged("WhiteValue");
                RaisePropertyChanged("GammaValue");
                RaisePropertyChanged("IsAutoContrast");
                RaisePropertyChanged("IsInvert");
            }
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
                        //Workspace.This.ShowOnscreenKeyboard();
                    }
                    else
                    {
                        // hide the onscreen keyboard
                        //Workspace.This.HideOnscreenKeyboard();
                    }
                }
            }
        }

        /// <summary>
        /// Is image contrasting in progressing?
        /// </summary>
        public bool IsContrasting
        {
            get { return _IsContrasting; }
        }

        public string ModifiedDate
        {
            get { return _ImageInfo.ModifiedDate; }
            set
            {
                _ImageInfo.ModifiedDate = value;
                RaisePropertyChanged("ModifiedDate");
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

        private bool _IsSelectionToolChecked = false;
        public bool IsSelectionToolChecked
        {
            get { return _IsSelectionToolChecked; }
            set
            {
                if (_IsSelectionToolChecked != value)
                {
                    if (Image != null)
                    {
                        if (Image.Format.BitsPerPixel == 32)
                        {
                            string caption = "Image type not supported...";
                            string message = "This operation is current not supported for 32-bit image.";
                            Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                            return;
                        }

                        if (RegionAdornerChanged != null)
                        {
                            RegionAdornerChanged(value);
                        }
                    }

                    _IsSelectionToolChecked = value;
                    RaisePropertyChanged("IsSelectionToolChecked");
                    //RaisePropertyChanged("IsAutoExposureToBand");
                }
            }
        }

        private Rect _SelectedRegion = new Rect();
        public Rect SelectedRegion
        {
            get
            {
                return _SelectedRegion;
            }
            set
            {
                _SelectedRegion = value;
                RaisePropertyChanged("SelectedRegion");
            }
        }

        // Override the default DPI value when specifically specified by the user on image resize.
        private bool _IsOverrideDefaultDpi = false;
        public bool IsOverrideDefaultDpi
        {
            get { return _IsOverrideDefaultDpi; }
            set
            {
                if (_IsOverrideDefaultDpi != value)
                {
                    _IsOverrideDefaultDpi = value;
                }
            }
        }

        public void ResizeImage(System.Windows.Size dstSize, double factor)
        {
            _Image = ImageProcessing.Resize(_Image, dstSize);
            IsDirty = true;

            if (ImageSizeChanged != null)
            {
                ImageSizeChanged(this, factor);
            }
        }

        /// <summary>
        /// Resize the image pixels and set the image DPI to the specify value.
        /// </summary>
        /// <param name="dstSize"></param>
        /// <param name="factor"></param>
        /// <param name="dDpiX"></param>
        /// <param name="dDpiY"></param>
        public void ResizeImage(System.Windows.Size dstSize, double factor, double dDpiX, double dDpiY)
        {
            if (dDpiX != _Image.DpiX || dDpiY != _Image.DpiY)
            {
                _Image = ImageProcessing.Resize(_Image, dstSize, dDpiX, dDpiY);
            }
            else
            {
                _Image = ImageProcessing.Resize(_Image, dstSize);
            }

            PixelFormat format = _DisplayImage.Format;
            BitmapPalette palette = _DisplayImage.Palette;
            _DisplayImage = new WriteableBitmap(_Image.PixelWidth, _Image.PixelHeight, 96, 96, format, palette);
            RaisePropertyChanged("DisplayImage");

            IsDirty = true;

            if (ImageSizeChanged != null)
            {
                ImageSizeChanged(this, factor);
            }
        }

        public void ChangeImageDpi(double dDpiX, double dDpiY)
        {
            if (dDpiX != _Image.DpiX || dDpiY != _Image.DpiY)
            {
                int width = _Image.PixelWidth;
                int height = _Image.PixelHeight;
                int stride = _Image.BackBufferStride;
                PixelFormat format = _Image.Format;
                BitmapPalette palette = _Image.Palette;
                byte[] data = new byte[(long)stride * (long)height];

                _Image.CopyPixels(data, stride, 0);
                WriteableBitmap target = new WriteableBitmap(width, height,
                                                             dDpiX, dDpiY,
                                                             format, palette);
                target.WritePixels(new Int32Rect(0, 0, width, height), data, stride, 0);
                data = null;
                IsDirty = true;
                Image = target;
            }
        }

        private AutoResetEvent _UpdateDisplayImageEvent = new AutoResetEvent(false);

        public unsafe void UpdateDisplayImage()
        {
            _UpdateDisplayImageEvent.Set();

            try
            {
                int srcWidth = _Image.PixelWidth;
                int srcHeight = _Image.PixelHeight;

                // Re-create display image buffer if source image pixel dimensions changed.
                if (srcWidth != _DisplayImage.PixelWidth &&
                    srcHeight != _DisplayImage.PixelHeight)
                {
                    // Work-around: very large image > 20000 x 20000 pixels; use 8-bit display image
                    //              Windows has issue displaying an image > 20000 x 20000 pixels.
                    //
                    if (_Image != null && (_Image.Format.BitsPerPixel == 16 || _Image.Format.BitsPerPixel == 8) &&
                        srcHeight * srcWidth > (20000 * 20000))
                    {
                        // Create 8-bit indexed display image buffer.
                        bool bIsSaturation = _ImageInfo.IsSaturationChecked;
                        var palette = new BitmapPalette(ImageProcessing.GetColorTableIndexed(bIsSaturation));
                        DisplayImage = new WriteableBitmap(srcWidth, srcHeight, _Image.DpiX, _Image.DpiY, PixelFormats.Indexed8, palette);
                    }
                    else
                    {
                        // Create 24-bit display image buffer.
                        DisplayImage = new WriteableBitmap(srcWidth, srcHeight, _Image.DpiX, _Image.DpiY, PixelFormats.Rgb24, _Image.Palette);
                    }
                }

                int srcStride = _Image.BackBufferStride;
                int dstStride = _DisplayImage.BackBufferStride;
                var pixelFormat = IppImaging.GetPixelFormatType(_Image.Format);
                byte* pSrcImageData = null;
                byte* pDstImageData = null;

                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    // lock bitmap in UI thread
                    if (!_Image.IsFrozen)
                        _Image.Lock();
                    if (!_DisplayImage.IsFrozen)
                        _DisplayImage.Lock();

                    pSrcImageData = (byte*)_Image.BackBuffer.ToPointer();
                    pDstImageData = (byte*)_DisplayImage.BackBuffer.ToPointer();
                });

                // run a method in another thread
                Thread updateDisplayImageThread = new Thread(() => UpdateDisplayImageEx(pSrcImageData, srcWidth, srcHeight, srcStride, pixelFormat, pDstImageData, dstStride, _ImageInfo));

                updateDisplayImageThread.Start();
                updateDisplayImageThread.Join();

                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    if (!_Image.IsFrozen)
                        _Image.Unlock();

                    _DisplayImage.AddDirtyRect(new Int32Rect(0, 0, Width, Height));

                    if (!_DisplayImage.IsFrozen)
                        _DisplayImage.Unlock();
                });

                //Reset flags: image contrasted.
                _IsContrasting = false;
                IsContrastValueChanged = false;

                // Trigger contrast values changed.
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
            //finally
            //{
            //    //Reset flags: image contrasted.
            //    //
            //    _IsContrasting = false;
            //    IsContrastValueChanged = false;
            //    //_UpdateDisplayImageEvent.Set();
            //}
        }

        public unsafe void UpdateDisplayImage(WriteableBitmap srcBitmap)
        {
            _UpdateDisplayImageEvent.Set();

            try
            {
                _IsContrasting = true;
                int srcWidth = srcBitmap.PixelWidth;
                int srcHeight = srcBitmap.PixelHeight;
                PixelFormatType pixelFormat = IppImaging.GetPixelFormatType(srcBitmap.Format);

                if (srcWidth != _DisplayImage.PixelWidth || srcHeight != _DisplayImage.PixelHeight)
                {
                    BitmapPalette palette = null;
                    PixelFormat dstPixelFormat = PixelFormats.Rgb24;
                    DisplayImage = new WriteableBitmap(srcWidth, srcHeight, 96, 96, dstPixelFormat, palette);
                }

                if (pixelFormat == PixelFormatType.P8u_C1 ||
                    pixelFormat == PixelFormatType.P16u_C1)
                {
                    _ImageInfo.NumOfChannels = 1;
                }
                else if (pixelFormat == PixelFormatType.P8u_C3 || pixelFormat == PixelFormatType.P8u_C4 ||
                         pixelFormat == PixelFormatType.P16u_C3 || pixelFormat == PixelFormatType.P16u_C4)
                {
                    if (pixelFormat == PixelFormatType.P8u_C4 ||
                        pixelFormat == PixelFormatType.P16u_C4)
                    {
                        _ImageInfo.NumOfChannels = 4;
                    }
                    else
                    {
                        _ImageInfo.NumOfChannels = 3;
                    }
                }

                int srcStride = srcBitmap.BackBufferStride;
                int dstStride = _DisplayImage.BackBufferStride;
                byte* pSrcImageData = null;
                byte* pDstImageData = null;
                //_ImageInfo.IsMultipleGrayscaleChannels = IsMultipleGrayChannels;

                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    // lock bitmap in UI thread
                    if (!srcBitmap.IsFrozen)
                        srcBitmap.Lock();
                    if (!_DisplayImage.IsFrozen)
                        _DisplayImage.Lock();

                    pSrcImageData = (byte*)srcBitmap.BackBuffer.ToPointer();
                    pDstImageData = (byte*)_DisplayImage.BackBuffer.ToPointer();
                });

                //run a method in another thread
                Thread updateDisplayImageThread = new Thread(() => UpdateDisplayImageEx(pSrcImageData, srcWidth, srcHeight, srcStride, pixelFormat, pDstImageData, dstStride, _ImageInfo));

                updateDisplayImageThread.Start();
                updateDisplayImageThread.Join();

                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    if (!srcBitmap.IsFrozen)
                        srcBitmap.Unlock();

                    _DisplayImage.AddDirtyRect(new Int32Rect(0, 0, srcWidth, srcHeight));

                    if (!_DisplayImage.IsFrozen)
                        _DisplayImage.Unlock();
                });

                //Reset flags: image contrasted.
                _IsContrasting = false;
                IsContrastValueChanged = false;

                // Trigger contrast values changed.
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
            //finally
            //{
            //    //Reset flags: image contrasted.
            //    //
            //    _IsContrasting = false;
            //    IsContrastValueChanged = false;
            //}
        }

        public unsafe void UpdateDisplayImageEx(byte* psrcimg, int width, int height, int srcstep, PixelFormatType pixelformat, byte* pdstimg, int dststep, ImageInfo imginfo)
        {
            _UpdateDisplayImageEvent.WaitOne();

            if (psrcimg == null) { return; }

            try
            {
                if (pixelformat == PixelFormatType.P16u_C3 || pixelformat == PixelFormatType.P16u_C4 ||
                    pixelformat == PixelFormatType.P8u_C3 || pixelformat == PixelFormatType.P8u_C4)
                {
                    #region === Individual channel(s) contrast ===

                    int nBlackValue = 0;
                    int nWhiteValue = 0;
                    int nDynamicBit = imginfo.DynamicBit;

                    if (imginfo.IsRedChannelAvail)
                    {
                        if (imginfo.RedChannel.IsAutoChecked)
                        {
                            //ImageProcessing.GetAutoScaleValues(srcimg, ref nWhiteValue, ref nBlackValue, ImageChannelType.Red);
                            ImageProcessing.GetAutoScaleValues(psrcimg, width, height, srcstep, pixelformat, ref nWhiteValue, ref nBlackValue, nDynamicBit, ImageChannelType.Red);
                            imginfo.RedChannel.BlackValue = nBlackValue;
                            imginfo.RedChannel.WhiteValue = nWhiteValue;
                            imginfo.RedChannel.GammaValue = 1.0;
                        }
                    }
                    if (imginfo.IsGreenChannelAvail)
                    {
                        if (imginfo.GreenChannel.IsAutoChecked)
                        {
                            //ImageProcessing.GetAutoScaleValues(srcimg, ref nWhiteValue, ref nBlackValue, ImageChannelType.Green);
                            ImageProcessing.GetAutoScaleValues(psrcimg, width, height, srcstep, pixelformat, ref nWhiteValue, ref nBlackValue, nDynamicBit, ImageChannelType.Green);
                            imginfo.GreenChannel.BlackValue = nBlackValue;
                            imginfo.GreenChannel.WhiteValue = nWhiteValue;
                            imginfo.GreenChannel.GammaValue = 1.0;
                        }
                    }
                    if (imginfo.IsBlueChannelAvail)
                    {
                        if (imginfo.BlueChannel.IsAutoChecked)
                        {
                            //ImageProcessing.GetAutoScaleValues(srcimg, ref nWhiteValue, ref nBlackValue, ImageChannelType.Blue);
                            ImageProcessing.GetAutoScaleValues(psrcimg, width, height, srcstep, pixelformat, ref nWhiteValue, ref nBlackValue, nDynamicBit, ImageChannelType.Blue);
                            imginfo.BlueChannel.BlackValue = nBlackValue;
                            imginfo.BlueChannel.WhiteValue = nWhiteValue;
                            imginfo.BlueChannel.GammaValue = 1.0;
                        }
                    }
                    if (imginfo.IsGrayChannelAvail)
                    {
                        if (imginfo.GrayChannel.IsAutoChecked)
                        {
                            //ImageProcessing.GetAutoScaleValues(srcimg, ref nWhiteValue, ref nBlackValue, ImageChannelType.Gray);
                            ImageProcessing.GetAutoScaleValues(psrcimg, width, height, srcstep, pixelformat, ref nWhiteValue, ref nBlackValue, nDynamicBit, ImageChannelType.Gray);
                            imginfo.GrayChannel.BlackValue = nBlackValue;
                            imginfo.GrayChannel.WhiteValue = nWhiteValue;
                            imginfo.GrayChannel.GammaValue = 1.0;
                        }
                    }

                    if (pixelformat == PixelFormatType.P8u_C4 || pixelformat == PixelFormatType.P16u_C4)
                    {
                        ImageProcessing.Scale_16u8u_C4C3Ex(psrcimg, width, height, srcstep, pixelformat, pdstimg, dststep, imginfo);
                    }
                    else
                    {
                        //System.Diagnostics.Stopwatch stopwatch1 = new System.Diagnostics.Stopwatch();
                        //stopwatch1.Start();

                        ImageProcessing.Scale_16u8u_C3C3Ex(psrcimg, width, height, srcstep, pixelformat, pdstimg, dststep, imginfo);

                        //stopwatch1.Stop();
                        //string elapsed = stopwatch1.ElapsedMilliseconds.ToString();
                        //System.Diagnostics.Debug.WriteLine(elapsed);
                    }

                    #endregion
                } // rgb image
                else
                {
                    #region === single channel image contrast ===

                    int nBlackValue = 0;
                    int nWhiteValue = MaxPixelValue;
                    double dGammaValue = 1.0;
                    int nDynamicBit = imginfo.DynamicBit;

                    if (imginfo.MixChannel.IsAutoChecked == true)
                    {
                        //ImageProcessing.GetAutoScaleValues(srcimg, ref nWhiteValue, ref nBlackValue);
                        ImageProcessing.GetAutoScaleValues(psrcimg, width, height, srcstep, pixelformat, ref nWhiteValue, ref nBlackValue, nDynamicBit);
                        imginfo.MixChannel.BlackValue = nBlackValue;
                        imginfo.MixChannel.WhiteValue = nWhiteValue;
                        imginfo.MixChannel.GammaValue = dGammaValue = 1.0;
                    }
                    else
                    {
                        nBlackValue = imginfo.MixChannel.BlackValue;
                        nWhiteValue = imginfo.MixChannel.WhiteValue;
                        dGammaValue = imginfo.MixChannel.GammaValue;
                    }

                    PixelFormat srcPixelFormat = (pixelformat == PixelFormatType.P16u_C1) ? PixelFormats.Gray16 : PixelFormats.Gray8;
                    if (width * height > (20000 * 20000))
                    {
                        // Work-around: very large image > 20000 x 20000 use 8-bit display image
                        //              Windows has issue displaying image > 20000 x 20000 pixels images.
                        // Use an indexed bitmap format.
                        ImageProcessing.Scale_16u8u_C1_Indexed(psrcimg, width, height, srcstep, srcPixelFormat,
                                                               pdstimg, dststep,
                                                               nBlackValue, nWhiteValue, dGammaValue,
                                                               imginfo.MaxPixelValue + 1,
                                                               imginfo.IsSaturationChecked,
                                                               imginfo.SaturationThreshold,
                                                               imginfo.MixChannel.IsInvertChecked,
                                                               imginfo.DynamicBit);
                    }
                    else
                    {
                        int nDynamicBpp = imginfo.DynamicBit;
                        int nMaxInput = imginfo.MaxPixelValue + 1;
                        srcPixelFormat = (pixelformat == PixelFormatType.P16u_C1) ? PixelFormats.Gray16 : PixelFormats.Gray8;
                        ImageProcessing.Scale_16u8u_C1(psrcimg, width, height, srcstep, srcPixelFormat, pdstimg, dststep,
                                                       nBlackValue, nWhiteValue, dGammaValue, nMaxInput,
                                                       imginfo.IsSaturationChecked,
                                                       imginfo.SaturationThreshold,
                                                       imginfo.MixChannel.IsInvertChecked, nDynamicBpp);
                    }
                    #endregion
                }

                _UpdateDisplayImageEvent.Set();
            }
            catch (OutOfMemoryException oome)
            {
                //GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                //GC.WaitForPendingFinalizers();
                //GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                _UpdateDisplayImageEvent.Set();
                throw new Exception(string.Format("{0}\nPlease close some images to clear up some memory.", oome.Message));
            }
            catch (Exception)
            {
                _UpdateDisplayImageEvent.Set();
                throw;
            }
            //finally
            //{
            //    _UpdateDisplayImageEvent.Set();
            //
            //    //GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            //    //GC.WaitForPendingFinalizers();
            //    //GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            //
            //    // Retrieves the number of bytes currently thought to be allocated (true: force full collection)
            //    //GC.GetTotalMemory(true);
            //}
        }


        public void UpdateDisplayImage(int nColorGradation, bool bIsMergeChannels)
        {
            if (_IsContrasting) { return; }

            try
            {
                _IsContrasting = true;

                _DisplayImage.Lock();
                if (nColorGradation > 3)
                    ImageProcessing.Scale_16u8u_C4C3(ref _Image, ref _DisplayImage, _ImageInfo, nColorGradation, bIsMergeChannels);
                else
                    ImageProcessing.Scale_16u8u_C3C3(ref _Image, ref _DisplayImage, _ImageInfo, nColorGradation, bIsMergeChannels);
                _DisplayImage.AddDirtyRect(new Int32Rect(0, 0, Width, Height));
                _DisplayImage.Unlock();

                _IsContrasting = false;

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
                _IsContrasting = false;
                throw;
            }
            //finally
            //{
            //    _IsContrasting = false;
            //}
        }

        public void TriggerImageSizeChanged(double factor)
        {
            if (factor != 1.0)
            {
                PixelFormat format = _DisplayImage.Format;
                BitmapPalette palette = _DisplayImage.Palette;
                _DisplayImage = new WriteableBitmap(_Image.PixelWidth, _Image.PixelHeight, 96, 96, format, palette);
                RaisePropertyChanged("DisplayImage");
            }
            ImageSizeChanged?.Invoke(this, factor);
        }

        public void DrawScaleBar()
        {
            var scale = ZoomLevel;
            var imageWidth = _Image.PixelWidth * scale;
            var imageHeight = _Image.PixelHeight * scale;
            double margin = _ImageInfo.Scalebar.Margin * scale;
            System.Windows.Point start = new System.Windows.Point(margin, margin);
            System.Windows.Point end = new System.Windows.Point(margin, margin);
            var lineWidth = _ImageInfo.Scalebar.WidthInInchOrCm * _ImageInfo.Scalebar.DistanceInPixels * scale;
            var lineTextVertMargin = _ImageInfo.Scalebar.LineTextMargin * scale;

            GraphicsBase baseText = new GraphicsText();
            string textFontFamilyName = "Trebuchet MS";
            var textUnitString = string.Format("{0} {1}", Math.Round(_ImageInfo.Scalebar.WidthInInchOrCm, 2), _ImageInfo.Scalebar.UnitOfLength.ToString());
            var fontWeight = (_ImageInfo.Scalebar.IsBoldText) ? FontWeights.Bold : FontWeights.Normal;
            var textSize = Workspace.This.MeasureString(textUnitString, textFontFamilyName, _ImageInfo.Scalebar.FontSize * scale, fontWeight);

            if (_ImageInfo.Scalebar.Location == SBLocation.UpperLeft)
            {
                start = new System.Windows.Point(margin, margin);
                end = new System.Windows.Point(margin + lineWidth, margin);
            }
            else if (_ImageInfo.Scalebar.Location == SBLocation.UpperRight)
            {
                start = new System.Windows.Point(imageWidth - margin - lineWidth, margin);
                end = new System.Windows.Point(start.X + lineWidth, margin);
            }
            else if (_ImageInfo.Scalebar.Location == SBLocation.LowerLeft)
            {
                if (_ImageInfo.Scalebar.IsHideText)
                {
                    start = new System.Windows.Point(margin, imageHeight - margin);
                    end = new System.Windows.Point(margin + lineWidth, imageHeight - margin);
                }
                else
                {
                    start = new System.Windows.Point(margin, imageHeight - margin - lineTextVertMargin - textSize.Height);
                    end = new System.Windows.Point(margin + lineWidth, imageHeight - margin - lineTextVertMargin - textSize.Height);
                }
            }
            else if (_ImageInfo.Scalebar.Location == SBLocation.LowerRight)
            {
                if (_ImageInfo.Scalebar.IsHideText)
                {
                    start = new System.Windows.Point(imageWidth - margin - lineWidth, imageHeight - margin);
                    end = new System.Windows.Point(start.X + lineWidth, imageHeight - margin);
                }
                else
                {
                    start = new System.Windows.Point(imageWidth - margin - lineWidth, imageHeight - margin - lineTextVertMargin - textSize.Height);
                    end = new System.Windows.Point(start.X + lineWidth, imageHeight - margin - lineTextVertMargin - textSize.Height);
                }
            }

            // Show text
            if (!_ImageInfo.Scalebar.IsHideText)
            {
                _DrawingCanvas.ObjectColor = _ImageInfo.Scalebar.SelectedColor;
                _DrawingCanvas.TextFontSize = _ImageInfo.Scalebar.FontSize;
                _DrawingCanvas.TextFontFamilyName = textFontFamilyName;
                _DrawingCanvas.TextFontStyle = FontStyles.Normal;
                _DrawingCanvas.TextFontWeight = fontWeight;
                _DrawingCanvas.TextFontStretch = FontStretches.ExtraExpanded;
                _DrawingCanvas.ActualScale = scale;

                if (_ImageInfo.Scalebar.Location == SBLocation.UpperLeft)
                {
                    start = new System.Windows.Point(margin, margin);
                    end = new System.Windows.Point(margin + lineWidth, margin);

                    double startX = margin + (lineWidth / 2) - (textSize.Width / 2);
                    if (startX < 0)
                        startX = 0;
                    double startY = margin + lineTextVertMargin;
                    double endX = startX + textSize.Width + 1;
                    double endY = margin + lineTextVertMargin + textSize.Height;

                    baseText = new GraphicsText(
                        textUnitString,
                        startX,
                        startY,
                        endX,
                        endY,
                        _DrawingCanvas.ObjectColor,
                        _DrawingCanvas.TextFontSize * scale,
                        _DrawingCanvas.TextFontFamilyName,
                        _DrawingCanvas.TextFontStyle,
                        _DrawingCanvas.TextFontWeight,
                        _DrawingCanvas.TextFontStretch,
                        _DrawingCanvas.ActualScale);
                }
                else if (_ImageInfo.Scalebar.Location == SBLocation.UpperRight)
                {
                    //start = new Point((imageWidth - _Margin - lineWidth) * scale, _Margin * scale);
                    //end = new Point(start.X + (lineWidth * scale), _Margin * scale);

                    double startX = imageWidth - margin - lineWidth + (lineWidth / 2) - (textSize.Width / 2);
                    if (startX < 0)
                        startX = 0;
                    double startY = margin + lineTextVertMargin;
                    double endX = startX + textSize.Width + 1;
                    double endY = margin + lineTextVertMargin + textSize.Height;
                    if (endX > imageWidth - margin)
                    {
                        endX -= endX - imageWidth - margin;
                        startX = endX - textSize.Width;
                    }

                    baseText = new GraphicsText(
                        textUnitString,
                        startX,
                        startY,
                        endX,
                        endY,
                        _DrawingCanvas.ObjectColor,
                        _DrawingCanvas.TextFontSize * scale,
                        _DrawingCanvas.TextFontFamilyName,
                        _DrawingCanvas.TextFontStyle,
                        _DrawingCanvas.TextFontWeight,
                        _DrawingCanvas.TextFontStretch,
                        _DrawingCanvas.ActualScale);
                }
                else if (_ImageInfo.Scalebar.Location == SBLocation.LowerLeft)
                {
                    //start = new Point(_Margin * scale, (imageHeight - _Margin) * scale);
                    //end = new Point((_Margin + lineWidth) * scale, (imageHeight - _Margin) * scale);

                    double startX = margin + (lineWidth / 2) - (textSize.Width / 2);
                    if (startX < 0)
                        startX = 0;
                    double startY = imageHeight - margin - textSize.Height;
                    double endX = startX + textSize.Width + 1;
                    double endY = imageHeight - margin;

                    baseText = new GraphicsText(
                        textUnitString,
                        startX,
                        startY,
                        endX,
                        endY,
                        _DrawingCanvas.ObjectColor,
                        _DrawingCanvas.TextFontSize * scale,
                        _DrawingCanvas.TextFontFamilyName,
                        _DrawingCanvas.TextFontStyle,
                        _DrawingCanvas.TextFontWeight,
                        _DrawingCanvas.TextFontStretch,
                        _DrawingCanvas.ActualScale);
                }
                else if (_ImageInfo.Scalebar.Location == SBLocation.LowerRight)
                {
                    //start = new Point((imageWidth - _Margin - lineWidth) * scale, (imageHeight - _Margin) * scale);
                    //end = new Point(start.X + (lineWidth * scale), (imageHeight - _Margin) * scale);

                    double startX = imageWidth - margin - lineWidth + (lineWidth / 2) - (textSize.Width / 2);
                    if (startX < 0)
                        startX = 0;
                    double startY = imageHeight - margin - textSize.Height;
                    double endX = startX + textSize.Width + 1;
                    double endY = imageHeight - margin;

                    baseText = new GraphicsText(
                        textUnitString,
                        startX,
                        startY,
                        endX,
                        endY,
                        _DrawingCanvas.ObjectColor,
                        _DrawingCanvas.TextFontSize * scale,
                        _DrawingCanvas.TextFontFamilyName,
                        _DrawingCanvas.TextFontStyle,
                        _DrawingCanvas.TextFontWeight,
                        _DrawingCanvas.TextFontStretch,
                        _DrawingCanvas.ActualScale);
                }
            }

            IsShowScalebar = _ImageInfo.Scalebar.IsShowScalebar;

            GraphicsBase baseLine = new GraphicsLine(start, end, _ImageInfo.Scalebar.HeightInPixels, _ImageInfo.Scalebar.SelectedColor, scale);
            _DrawingCanvas.Tool = ToolType.None;

            if (_DrawingCanvas.Count > 0)
                _DrawingCanvas.GraphicsList.Clear();

            // Add scale bar
            _DrawingCanvas.GraphicsList.Add(baseLine);

            if (!_ImageInfo.Scalebar.IsHideText)
            {
                // Add text
                _DrawingCanvas.GraphicsList.Add(baseText);
            }

            _DrawingCanvas.RefreshClip();
        }

        //private bool _IsImageResized = false;
        //public bool IsImageResized
        //{
        //    get { return _IsImageResized; }
        //    set
        //    {
        //        if (_IsImageResized != value)
        //        {
        //            _IsImageResized = value;
        //            RaisePropertyChanged("IsImageResized");
        //            if (_IsImageResized)
        //            {
        //                if (ImageSizeChanged != null)
        //                {
        //                    ImageSizeChanged(10);
        //                }
        //            }
        //        }
        //    }
        //}


        /*public ChannelSelectViewModel RedChannelContrastVm
        {
            get { return _RedChannelContrastVm; }
            set
            {
                if (_RedChannelContrastVm != value)
                {
                    _RedChannelContrastVm = value;
                    RaisePropertyChanged("RedChannelContrastVm");
                }
            }
        }*/

        /*public ChannelSelectViewModel GreenChannelContrastVm
        {
            get { return _GreenChannelContrastVm; }
            set
            {
                if (_GreenChannelContrastVm != value)
                {
                    _GreenChannelContrastVm = value;
                    RaisePropertyChanged("GreenChannelContrastVm");
                }
            }
        }*/

        /*public ChannelSelectViewModel BlueChannelContrastVm
        {
            get { return _BlueChannelContrastVm; }
            set
            {
                if (_BlueChannelContrastVm != value)
                {
                    _BlueChannelContrastVm = value;
                    RaisePropertyChanged("BlueChannelContrastVm");
                }
            }
        }*/

        /*public ChannelSelectViewModel GrayChannelContrastVm
        {
            get { return _GrayChannelContrastVm; }
            set
            {
                if (_GrayChannelContrastVm != value)
                {
                    _GrayChannelContrastVm = value;
                    RaisePropertyChanged("GrayChannelContrastVm");
                }
            }
        }*/

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

    class MultiplexData : ViewModelBase
    {
        private string _Title;
        private string _FileID;

        public MultiplexData()
        {
        }

        public MultiplexData(string title, string fileId)
        {
            this._Title = title;
            this._FileID = fileId;
        }

        public string Title
        {
            get { return _Title; }
            set
            {
                _Title = value;
                RaisePropertyChanged("Title");
            }
        }

        public string FileID
        {
            get { return _FileID; }
            set
            {
                _FileID = value;
                RaisePropertyChanged("FileID");
            }
        }
    }

}
