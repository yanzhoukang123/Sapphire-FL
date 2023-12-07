using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input; //ICommand
using System.Windows.Media.Imaging; //WriteableBitmap
using System.ComponentModel;        //BackgroundWorker
using Azure.Image.Processing;
using Azure.WPF.Framework;  //RelayCommand
using Azure.ImagingSystem;
using Azure.Utilities;

namespace Azure.LaserScanner.ViewModel
{

    class ContrastViewModel : ViewModelBase
    {
        #region Private data...

        private int _BlackValue = 0;
        private int _WhiteValue = 65535;
        private double _GammaValue = 1.0;
        private ImageInfo _DisplayImageInfo = null;

        #endregion

        #region Constructors...

        public ContrastViewModel()
        {
            _DisplayImageInfo = new ImageInfo();
        }

        #endregion

        #region Public properties...

        public ImageInfo DisplayImageInfo
        {
            get { return _DisplayImageInfo; }
            set
            {
                _DisplayImageInfo = value;
                RaisePropertyChanged("DisplayImageInfo");
                RaisePropertyChanged("BlackValue");
                RaisePropertyChanged("WhiteValue");
                RaisePropertyChanged("GammaValue");
            }
        }

        public int BlackValue
        {
            get
            {
                if (DisplayImageInfo != null)
                {
                    if (!IsRgbImage || DisplayImageInfo.ContrastChannelFlags == ImageChannelFlag.None)
                    {
                        _BlackValue = DisplayImageInfo.MixChannel.BlackValue;
                    }
                    else
                    {
                        if (DisplayImageInfo.IsContrastRedChannel && DisplayImageInfo.IsDisplayRedChannel)
                            _BlackValue = DisplayImageInfo.RedChannel.BlackValue;
                        else if (DisplayImageInfo.IsContrastGreenChannel && DisplayImageInfo.IsDisplayGreenChannel)
                            _BlackValue = DisplayImageInfo.GreenChannel.BlackValue;
                        else if (DisplayImageInfo.IsContrastBlueChannel && DisplayImageInfo.IsDisplayBlueChannel)
                            _BlackValue = DisplayImageInfo.BlueChannel.BlackValue;
                        else if (DisplayImageInfo.IsContrastGrayChannel && DisplayImageInfo.IsDisplayGrayChannel)
                            _BlackValue = DisplayImageInfo.GrayChannel.BlackValue;
                        else if (DisplayImageInfo.ContrastChannelFlags == ImageChannelFlag.None)
                            _BlackValue = DisplayImageInfo.MixChannel.BlackValue;
                    }
                }
                return _BlackValue;
            }
            set
            {
                if (DisplayImageInfo != null)
                {
                    // Do not allow the black value to be >= to the white value.
                    if (value >= WhiteValue)
                    {
                        value = WhiteValue - 1;
                    }

                    if (!IsRgbImage || DisplayImageInfo.ContrastChannelFlags == ImageChannelFlag.None)
                    {
                        if (DisplayImageInfo.MixChannel.BlackValue != value)
                        {
                            DisplayImageInfo.MixChannel.BlackValue = value;
                            //IsContrastValueChanged = true;
                        }
                    }
                    else
                    {
                        if (DisplayImageInfo.IsContrastRedChannel && DisplayImageInfo.IsDisplayRedChannel)
                        {
                            if (DisplayImageInfo.RedChannel.BlackValue != value)
                            {
                                DisplayImageInfo.RedChannel.BlackValue = value;
                                //IsContrastValueChanged = true;
                            }
                        }
                        if (DisplayImageInfo.IsContrastGreenChannel && DisplayImageInfo.IsDisplayGreenChannel)
                        {
                            if (DisplayImageInfo.GreenChannel.BlackValue != value)
                            {
                                DisplayImageInfo.GreenChannel.BlackValue = value;
                                //IsContrastValueChanged = true;
                            }
                        }
                        if (DisplayImageInfo.IsContrastBlueChannel && DisplayImageInfo.IsDisplayBlueChannel)
                        {
                            if (DisplayImageInfo.BlueChannel.BlackValue != value)
                            {
                                DisplayImageInfo.BlueChannel.BlackValue = value;
                                //IsContrastValueChanged = true;
                            }
                        }
                        if (DisplayImageInfo.IsContrastGrayChannel && DisplayImageInfo.IsDisplayGrayChannel)
                        {
                            if (DisplayImageInfo.GrayChannel.BlackValue != value)
                            {
                                DisplayImageInfo.GrayChannel.BlackValue = value;
                                //IsContrastValueChanged = true;
                            }
                        }
                        if (DisplayImageInfo.ContrastChannelFlags == ImageChannelFlag.None)
                        {
                            if (DisplayImageInfo.MixChannel.BlackValue != value)
                            {
                                DisplayImageInfo.MixChannel.BlackValue = value;
                                //IsContrastValueChanged = true;
                            }
                        }
                    }
                    //_BlackValue = value;
                    RaisePropertyChanged("BlackValue");
                }
            }
        }

        public int WhiteValue
        {
            get
            {
                if (DisplayImageInfo != null)
                {
                    if (!IsRgbImage || DisplayImageInfo.ContrastChannelFlags == ImageChannelFlag.None)
                    {
                        _WhiteValue = DisplayImageInfo.MixChannel.WhiteValue;
                    }
                    else
                    {
                        if (DisplayImageInfo.IsContrastRedChannel && DisplayImageInfo.IsDisplayRedChannel)
                            _WhiteValue = DisplayImageInfo.RedChannel.WhiteValue;
                        else if (DisplayImageInfo.IsContrastGreenChannel && DisplayImageInfo.IsDisplayGreenChannel)
                            _WhiteValue = DisplayImageInfo.GreenChannel.WhiteValue;
                        else if (DisplayImageInfo.IsContrastBlueChannel && DisplayImageInfo.IsDisplayBlueChannel)
                            _WhiteValue = DisplayImageInfo.BlueChannel.WhiteValue;
                        else if (DisplayImageInfo.IsContrastGrayChannel && DisplayImageInfo.IsDisplayGrayChannel)
                            _WhiteValue = DisplayImageInfo.GrayChannel.WhiteValue;
                        else if (DisplayImageInfo.ContrastChannelFlags == ImageChannelFlag.None)
                            _WhiteValue = DisplayImageInfo.MixChannel.WhiteValue;
                    }
                }
                return _WhiteValue;
            }
            set
            {
                if (DisplayImageInfo != null)
                {
                    // Do not allow the white value to be <= the black value
                    if (value <= BlackValue)
                    {
                        value = BlackValue + 1;
                    }

                    if (!IsRgbImage || DisplayImageInfo.ContrastChannelFlags == ImageChannelFlag.None)
                    {
                        if (DisplayImageInfo.MixChannel.WhiteValue != value)
                        {
                            DisplayImageInfo.MixChannel.WhiteValue = value;
                            //IsContrastValueChanged = true;
                        }
                    }
                    else
                    {
                        if (DisplayImageInfo.IsContrastRedChannel && DisplayImageInfo.IsDisplayRedChannel)
                        {
                            if (DisplayImageInfo.RedChannel.WhiteValue != value)
                            {
                                DisplayImageInfo.RedChannel.WhiteValue = value;
                                //IsContrastValueChanged = true;
                            }
                        }
                        if (DisplayImageInfo.IsContrastGreenChannel && DisplayImageInfo.IsDisplayGreenChannel)
                        {
                            if (DisplayImageInfo.GreenChannel.WhiteValue != value)
                            {
                                DisplayImageInfo.GreenChannel.WhiteValue = value;
                                //IsContrastValueChanged = true;
                            }
                        }
                        if (DisplayImageInfo.IsContrastBlueChannel && DisplayImageInfo.IsDisplayBlueChannel)
                        {
                            if (DisplayImageInfo.BlueChannel.WhiteValue != value)
                            {
                                DisplayImageInfo.BlueChannel.WhiteValue = value;
                                //IsContrastValueChanged = true;
                            }
                        }
                        if (DisplayImageInfo.IsContrastGrayChannel && DisplayImageInfo.IsDisplayGrayChannel)
                        {
                            if (DisplayImageInfo.GrayChannel.WhiteValue != value)
                            {
                                DisplayImageInfo.GrayChannel.WhiteValue = value;
                                //IsContrastValueChanged = true;
                            }
                        }
                        if (DisplayImageInfo.ContrastChannelFlags == ImageChannelFlag.None)
                        {
                            if (DisplayImageInfo.MixChannel.WhiteValue != value)
                            {
                                DisplayImageInfo.MixChannel.WhiteValue = value;
                                //IsContrastValueChanged = true;
                            }
                        }
                    }
                    //_WhiteValue = value;
                    RaisePropertyChanged("WhiteValue");
                }
            }
        }

        public double GammaValue
        {
            get
            {
                if (DisplayImageInfo != null)
                {
                    // Convert Real gamma value to Slider value.
                    //
                    if (!IsRgbImage || DisplayImageInfo.ContrastChannelFlags == ImageChannelFlag.None)
                    {
                        _GammaValue = Math.Round(Math.Log10(DisplayImageInfo.MixChannel.GammaValue), 3);
                    }
                    else
                    {
                        if (DisplayImageInfo.IsContrastRedChannel && DisplayImageInfo.IsDisplayRedChannel)
                            _GammaValue = Math.Round(Math.Log10(DisplayImageInfo.RedChannel.GammaValue), 3);
                        else if (DisplayImageInfo.IsContrastGreenChannel && DisplayImageInfo.IsDisplayGreenChannel)
                            _GammaValue = Math.Round(Math.Log10(DisplayImageInfo.GreenChannel.GammaValue), 3);
                        else if (DisplayImageInfo.IsContrastBlueChannel && DisplayImageInfo.IsDisplayBlueChannel)
                            _GammaValue = Math.Round(Math.Log10(DisplayImageInfo.BlueChannel.GammaValue), 3);
                        else if (DisplayImageInfo.IsContrastGrayChannel && DisplayImageInfo.IsDisplayGrayChannel)
                            _GammaValue = Math.Round(Math.Log10(DisplayImageInfo.GrayChannel.GammaValue), 3);
                        else if (DisplayImageInfo.ContrastChannelFlags == ImageChannelFlag.None)
                            _GammaValue = Math.Round(Math.Log10(DisplayImageInfo.MixChannel.GammaValue), 3);
                    }
                }
                return _GammaValue;
            }
            set
            {
                if (DisplayImageInfo != null)
                {
                    if (!IsRgbImage || DisplayImageInfo.ContrastChannelFlags == ImageChannelFlag.None)
                    {
                        if (DisplayImageInfo.MixChannel.GammaValue != value)
                        {
                            double dGammaValue = Math.Round(Math.Pow(10, value), 3);    // true gamma value
                            DisplayImageInfo.MixChannel.GammaValue = dGammaValue;
                            //IsContrastValueChanged = true;
                        }
                    }
                    else
                    {
                        if (DisplayImageInfo.IsContrastRedChannel && DisplayImageInfo.IsDisplayRedChannel)
                        {
                            if (DisplayImageInfo.RedChannel.GammaValue != value)
                            {
                                double dGammaValue = Math.Round(Math.Pow(10, value), 3);
                                DisplayImageInfo.RedChannel.GammaValue = dGammaValue;
                                //IsContrastValueChanged = true;
                            }
                        }
                        if (DisplayImageInfo.IsContrastGreenChannel && DisplayImageInfo.IsDisplayGreenChannel)
                        {
                            if (DisplayImageInfo.GreenChannel.GammaValue != value)
                            {
                                double dGammaValue = Math.Round(Math.Pow(10, value), 3);
                                DisplayImageInfo.GreenChannel.GammaValue = dGammaValue;
                                //IsContrastValueChanged = true;
                            }
                        }
                        if (DisplayImageInfo.IsContrastBlueChannel && DisplayImageInfo.IsDisplayBlueChannel)
                        {
                            if (DisplayImageInfo.BlueChannel.GammaValue != value)
                            {
                                double dGammaValue = Math.Round(Math.Pow(10, value), 3);
                                DisplayImageInfo.BlueChannel.GammaValue = dGammaValue;
                                //IsContrastValueChanged = true;
                            }
                        }
                        if (DisplayImageInfo.IsContrastGrayChannel && DisplayImageInfo.IsDisplayGrayChannel)
                        {
                            if (DisplayImageInfo.GrayChannel.GammaValue != value)
                            {
                                double dGammaValue = Math.Round(Math.Pow(10, value), 3);
                                DisplayImageInfo.GrayChannel.GammaValue = dGammaValue;
                                //IsContrastValueChanged = true;
                            }
                        }
                        if (DisplayImageInfo.ContrastChannelFlags == ImageChannelFlag.None)
                        {
                            if (DisplayImageInfo.MixChannel.GammaValue != value)
                            {
                                double dGammaValue = Math.Round(Math.Pow(10, value), 3);    // true gamma value
                                DisplayImageInfo.MixChannel.GammaValue = dGammaValue;
                                //IsContrastValueChanged = true;
                            }
                        }
                    }
                    RaisePropertyChanged("GammaValue");
                }
            }
        }

        public int NumOfChannels { get; set; } = 0;

        public bool IsRgbImage
        {
            get
            {
                bool bIsRgbImage = false;
                if (NumOfChannels == 1 &&
                    (_DisplayImageInfo.IsContrastGrayChannel || DisplayImageInfo.ContrastChannelFlags == ImageChannelFlag.None))
                {
                    bIsRgbImage = false;
                }
                else
                {
                    bIsRgbImage = true;
                }
                return bIsRgbImage;
            }
        }

        #endregion

        #region Public methods...

        /// <summary>
        /// Reset contrast values (Black/White/Gamma) to default value
        /// </summary>
        public void ResetContrastValues()
        {
            this.BlackValue = 0;
            this.WhiteValue = 65535;
            this.GammaValue = 0.0;
        }

        /// <summary>
        /// Reset (or turn off) auto-contrast flags in all channels
        /// </summary>
        public void ResetAuto()
        {
            if (DisplayImageInfo != null)
            {
                if (DisplayImageInfo.RedChannel.IsAutoChecked ||
                    DisplayImageInfo.GreenChannel.IsAutoChecked ||
                    DisplayImageInfo.BlueChannel.IsAutoChecked ||
                    DisplayImageInfo.GrayChannel.IsAutoChecked)
                {
                    List<int> minValues = new List<int>();
                    List<int> maxValues = new List<int>();
                    if (DisplayImageInfo.RedChannel.IsAutoChecked)
                    {
                        minValues.Add(DisplayImageInfo.RedChannel.BlackValue);
                        maxValues.Add(DisplayImageInfo.RedChannel.WhiteValue);
                    }
                    if (DisplayImageInfo.GreenChannel.IsAutoChecked)
                    {
                        minValues.Add(DisplayImageInfo.GreenChannel.BlackValue);
                        maxValues.Add(DisplayImageInfo.GreenChannel.WhiteValue);
                    }
                    if (DisplayImageInfo.BlueChannel.IsAutoChecked)
                    {
                        minValues.Add(DisplayImageInfo.BlueChannel.BlackValue);
                        maxValues.Add(DisplayImageInfo.BlueChannel.WhiteValue);
                    }
                    if (DisplayImageInfo.GrayChannel.IsAutoChecked)
                    {
                        if (!IsRgbImage)
                        {
                            minValues.Add(DisplayImageInfo.MixChannel.BlackValue);
                            maxValues.Add(DisplayImageInfo.MixChannel.WhiteValue);
                        }
                        else
                        {
                            minValues.Add(DisplayImageInfo.GrayChannel.BlackValue);
                            maxValues.Add(DisplayImageInfo.GrayChannel.WhiteValue);
                        }
                    }
                    minValues.Sort();
                    DisplayImageInfo.MixChannel.BlackValue = minValues[0];
                    maxValues.Sort();
                    DisplayImageInfo.MixChannel.WhiteValue = maxValues[maxValues.Count - 1];
                }
                DisplayImageInfo.RedChannel.IsAutoChecked = false;
                DisplayImageInfo.GreenChannel.IsAutoChecked = false;
                DisplayImageInfo.BlueChannel.IsAutoChecked = false;
                DisplayImageInfo.GrayChannel.IsAutoChecked = false;
                DisplayImageInfo.MixChannel.IsAutoChecked = false;
                RaisePropertyChanged("BlackValue");
                RaisePropertyChanged("WhiteValue");
                RaisePropertyChanged("GammaValue");
            }
        }

        #endregion
    }
}
