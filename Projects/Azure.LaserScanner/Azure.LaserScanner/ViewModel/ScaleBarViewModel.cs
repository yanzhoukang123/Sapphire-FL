using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
//using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Azure.WPF.Framework;
using Azure.Common;
using DrawToolsLib;

namespace Azure.LaserScanner.ViewModel
{
    public class ScaleBarViewModel : ViewModelBase
    {
        private FileViewModel _ActiveDocument;
        private ScaleBar _Scalebar;
        private double _ImageWidth;
        private double _ImageHeight;
        //private double _Margin;
        //private double _LineTextMargin;

        public ScaleBarViewModel()
        {
            //_Margin = 20;
            //_LineTextMargin = 5;
        }

        //public double Margin
        //{
        //    get { return _Margin; }
        //}
        //public double LineTextMargin
        //{
        //    get { return _LineTextMargin; }
        //}

        public FileViewModel ActiveDocument
        {
            get { return _ActiveDocument; }
            set
            {
                _ActiveDocument = value;

                if (_ActiveDocument != null && _ActiveDocument.Image != null)
                {
                    _ImageWidth = _ActiveDocument.Image.PixelWidth;
                    _ImageHeight = _ActiveDocument.Image.PixelHeight;
                    _Scalebar = _ActiveDocument.Scalebar;
                    double dpi = _ActiveDocument.Image.DpiX;
                    _Scalebar.DistanceInPixels = (_Scalebar.DistanceInPixels == 0) ? dpi : _Scalebar.DistanceInPixels;
                    if (_Scalebar.DistanceInPixels == dpi && _Scalebar.UnitOfLength == UnitOfLength.cm)
                    {
                        _Scalebar.DistanceInPixels = dpi / 2.54;
                    }
                    UpdateScaleBar();
                }
                else
                {
                    _Scalebar = new ScaleBar();
                    _Scalebar.DistanceInPixels = 0;
                }
                RaisePropertyChanged("SelectedUnitOfLength");
                RaisePropertyChanged("SelectedColor");
                RaisePropertyChanged("DistanceInPixels");
                RaisePropertyChanged("WidthInInchOrCm");
                RaisePropertyChanged("Scalebar");
            }
        }

        public ScaleBar Scalebar
        {
            get { return _Scalebar; }
            set
            {
                _Scalebar = value;
            }
        }

        //public double DistanceInPixels
        //{
        //    get { return _Scalebar.DistanceInPixels; }
        //    set
        //    {
        //        _Scalebar.DistanceInPixels = value;
        //        RaisePropertyChanged("DistanceInPixels");
        //    }
        //}
        //public double KnownDistance
        //{
        //    get { return _Scalebar.KnownDistance; }
        //    set
        //    {
        //        _Scalebar.KnownDistance = value;
        //        RaisePropertyChanged("KnownDistance");
        //    }
        //}

        //public ObservableCollection<string> UnitOfLengthOptions
        //{
        //    get
        //    {
        //        return new ObservableCollection<string>()
        //        {
        //            "inch",
        //            "cm",
        //        };
        //    }
        //}
        public Dictionary<UnitOfLength, string> UnitOfLengthOptions { get; } =
            new Dictionary<UnitOfLength, string>()
            {
                { UnitOfLength.cm, "cm"},
                { UnitOfLength.inch, "inch"},
            };
        public UnitOfLength SelectedUnitOfLength
        {
            get { return _Scalebar.UnitOfLength; }
            set
            {
                if (_Scalebar.UnitOfLength != value)
                {
                    if (_ActiveDocument != null && _ActiveDocument.Image != null)
                    {
                        var dpi = _ActiveDocument.Image.DpiX;
                        if (_Scalebar.UnitOfLength == UnitOfLength.inch && value == UnitOfLength.cm)
                        {
                            // inch to cm
                            _Scalebar.DistanceInPixels = dpi / 2.54;
                            _Scalebar.WidthInInchOrCm *= 2.54;
                        }
                        else if (_Scalebar.UnitOfLength == UnitOfLength.cm && value == UnitOfLength.inch)
                        {
                            // cm to inch
                            _Scalebar.DistanceInPixels = dpi;
                            _Scalebar.WidthInInchOrCm /= 2.54;
                        }
                    }

                    _Scalebar.UnitOfLength = value;
                    RaisePropertyChanged("SelectedUnitOfLength");
                    //RaisePropertyChanged("DistanceInPixels");
                    RaisePropertyChanged("WidthInInchOrCm");
                    RaisePropertyChanged("UnitOfLengthText");
                    UpdateScaleBar();
                }
            }
        }

        public string UnitOfLengthText
        {
            get
            {
                string unitOfLength = string.Empty;
                if (_Scalebar.UnitOfLength == UnitOfLength.cm)
                    unitOfLength = "centimeters :";
                else
                {
                    unitOfLength = "inches:";
                }
                return unitOfLength;
            }
        }

        public double WidthInInchOrCm
        {
            get { return _Scalebar.WidthInInchOrCm; }
            set
            {
                _Scalebar.WidthInInchOrCm = value;
                var scale = _ActiveDocument.ZoomLevel;
                var imageWidth = _ImageWidth * scale;
                double margin = _Scalebar.Margin * scale;
                var lineWidth = _Scalebar.WidthInInchOrCm * _Scalebar.DistanceInPixels * scale;
                if (lineWidth > imageWidth - margin)
                {
                    _Scalebar.WidthInInchOrCm = Math.Round((imageWidth - margin) / _Scalebar.DistanceInPixels / scale, 2);
                }
                RaisePropertyChanged("WidthInInchOrCm");
                if (_ActiveDocument.DrawingCanvas != null && _Scalebar.IsShowScalebar)
                {
                    UpdateScaleBar();
                }
            }
        }

        public Dictionary<SBLocation, string> LocationOptions { get; } =
            new Dictionary<SBLocation, string>()
            {
                { SBLocation.UpperRight, "Upper Right"},
                { SBLocation.LowerRight, "Lower Right"},
                { SBLocation.UpperLeft,  "Upper Left"},
                { SBLocation.LowerLeft,  "Lower Left"},
            };
        /// <summary>
        /// Scale bar location (Upper Right, Lower Right, Upper Left, Lower Left)
        /// </summary>
        public SBLocation SelectedLocation
        {
            get { return _Scalebar.Location; }
            set
            {
                _Scalebar.Location = value;
                RaisePropertyChanged("SelectedLocation");
                if (_ActiveDocument.DrawingCanvas != null && _Scalebar.IsShowScalebar)
                {
                    UpdateScaleBar();
                }
            }
        }

        public double HeightInPixels
        {
            get { return _Scalebar.HeightInPixels; }
            set
            {
                _Scalebar.HeightInPixels = value;
                RaisePropertyChanged("HeightInPixels");
                if (_ActiveDocument.DrawingCanvas != null && _Scalebar.IsShowScalebar)
                {
                    UpdateScaleBar();
                }
            }
        }
        public double FontSize
        {
            get { return _Scalebar.FontSize; }
            set
            {
                _Scalebar.FontSize = value;
                RaisePropertyChanged("FontSize");
                if (_ActiveDocument.DrawingCanvas != null && _Scalebar.IsShowScalebar)
                {
                    UpdateScaleBar();
                }
            }
        }

        public Dictionary<Color, string> ColorOptions { get; } =
            new Dictionary<Color, string>()
            {
                { Colors.White, "White"},
                { Colors.Black, "Black"},
                { Colors.LightGray, "Light Gray"},
                { Colors.Gray, "Gray"},
                { Colors.DarkGray, "Dark Gray"},
                { Colors.Red, "Red"},
                { Colors.Green, "Green"},
                { Colors.Blue, "Blue"},
                { Colors.Yellow, "Yellow"},
            };
        public Color SelectedColor
        {
            get { return _Scalebar.SelectedColor; }
            set
            {
                _Scalebar.SelectedColor = value;
                RaisePropertyChanged("SelectedColor");
                if (_ActiveDocument.DrawingCanvas != null && _Scalebar.IsShowScalebar)
                {
                    _ActiveDocument.DrawingCanvas.ObjectColor = _Scalebar.SelectedColor;
                    foreach (var drawObj in _ActiveDocument.DrawingCanvas.GraphicsList)
                    {
                        if (drawObj is GraphicsLine)
                        {
                            ((GraphicsLine)drawObj).ObjectColor = _Scalebar.SelectedColor;
                        }
                        else if (drawObj is GraphicsText)
                        {
                            ((GraphicsText)drawObj).ObjectColor = _Scalebar.SelectedColor;
                        }
                    }
                    _ActiveDocument.DrawingCanvas.RefreshClip();
                }
            }
        }
        public Dictionary<Color, string> BgColorOptions { get; } =
            new Dictionary<Color, string>()
            {
                { Colors.Transparent, "None"},
                { Colors.Black, "Black"},
                { Colors.White, "White"},
                { Colors.Gray, "Gray"},
                { Colors.DarkGray, "Dark Gray"},
                { Colors.LightGray, "Light Gray"},
                { Colors.Red, "Red"},
                { Colors.Green, "Green"},
                { Colors.Blue, "Blue"},
                { Colors.Yellow, "Yellow"},
            };
        public Color SelectedBgColor
        {
            get { return _Scalebar.SelectedBgColor; }
            set
            {
                _Scalebar.SelectedBgColor = value;
                RaisePropertyChanged("SelectedBgColor");
            }
        }

        public bool IsBoldText
        {
            get { return _Scalebar.IsBoldText; }
            set
            {
                _Scalebar.IsBoldText = value;
                RaisePropertyChanged("IsBoldText");
                if (_ActiveDocument.DrawingCanvas != null && _Scalebar.IsShowScalebar)
                {
                    UpdateScaleBar();
                }
            }
        }
        public bool IsHideText
        {
            get { return _Scalebar.IsHideText; }
            set
            {
                if (_Scalebar.IsHideText != value)
                {
                    _Scalebar.IsHideText = value;
                    RaisePropertyChanged("IsHideText");
                    if (_ActiveDocument.DrawingCanvas != null && _Scalebar.IsShowScalebar)
                    {
                        UpdateScaleBar();
                    }
                }
            }
        }
        //public bool IsOverlay
        //{
        //    get { return _Scalebar.IsOverlay; }
        //    set
        //    {
        //        _Scalebar.IsOverlay = value;
        //        RaisePropertyChanged("IsOverlay");
        //    }
        //}
        //public bool IsLabelAllChannels
        //{
        //    get { return _Scalebar.IsLabelAllChannels; }
        //    set
        //    {
        //        _Scalebar.IsLabelAllChannels = value;
        //        RaisePropertyChanged("IsLabelAllChannels");
        //    }
        //}
        public bool IsShowScalebar
        {
            get { return _Scalebar.IsShowScalebar; }
            set
            {
                _Scalebar.IsShowScalebar = value;
                RaisePropertyChanged("IsShowScalebar");

                _ActiveDocument.IsShowScalebar = _Scalebar.IsShowScalebar;

                if (_Scalebar.IsShowScalebar)
                {
                    UpdateScaleBar();
                    _ActiveDocument.DrawingCanvas.RefreshClip();
                }
            }
        }

        public void UpdateScaleBar()
        {
            if (_ActiveDocument == null) { return; }

            var scale = _ActiveDocument.ZoomLevel;
            var imageWidth = _ImageWidth * scale;
            var imageHeight = _ImageHeight * scale;
            double margin = _Scalebar.Margin * scale;
            Point start = new Point(margin, margin);
            Point end = new Point(margin, margin);
            var lineWidth = _Scalebar.WidthInInchOrCm * _Scalebar.DistanceInPixels * scale;
            if (lineWidth > imageWidth)
            {
                _Scalebar.WidthInInchOrCm = Math.Round((0.50 * ((imageWidth - margin) / _Scalebar.DistanceInPixels / scale)), 2);
            }
            var lineTextVertMargin = _ActiveDocument.ImageInfo.Scalebar.LineTextMargin * scale;

            GraphicsBase baseText = new GraphicsText();
            string textFontFamilyName = "Trebuchet MS";
            var textUnitString = string.Format("{0} {1}", Math.Round(_Scalebar.WidthInInchOrCm, 2), _Scalebar.UnitOfLength.ToString());
            var fontWeight = (_Scalebar.IsBoldText) ? FontWeights.Bold : FontWeights.Normal;
            var textSize = Workspace.This.MeasureString(textUnitString, textFontFamilyName, _Scalebar.FontSize * scale, fontWeight);

            if (_Scalebar.Location == SBLocation.UpperLeft)
            {
                start = new Point(margin, margin);
                end = new Point(margin + lineWidth, margin);
            }
            else if (_Scalebar.Location == SBLocation.UpperRight)
            {
                start = new Point(imageWidth - margin - lineWidth, margin);
                end = new Point(start.X + lineWidth, margin);
            }
            else if (_Scalebar.Location == SBLocation.LowerLeft)
            {
                if (_Scalebar.IsHideText)
                {
                    start = new Point(margin, imageHeight - margin);
                    end = new Point(margin + lineWidth, imageHeight - margin);
                }
                else
                {
                    start = new Point(margin, imageHeight - margin - lineTextVertMargin - textSize.Height);
                    end = new Point(margin + lineWidth, imageHeight - margin - lineTextVertMargin - textSize.Height);
                }
            }
            else if (_Scalebar.Location == SBLocation.LowerRight)
            {
                if (_Scalebar.IsHideText)
                {
                    start = new Point(imageWidth - margin - lineWidth, imageHeight - margin);
                    end = new Point(start.X + lineWidth, imageHeight - margin);
                }
                else
                {
                    start = new Point(imageWidth - margin - lineWidth, imageHeight - margin - lineTextVertMargin - textSize.Height);
                    end = new Point(start.X + lineWidth, imageHeight - margin - lineTextVertMargin - textSize.Height);
                }
            }

            // Show text
            if (!_Scalebar.IsHideText)
            {
                _ActiveDocument.DrawingCanvas.ObjectColor = _Scalebar.SelectedColor;
                _ActiveDocument.DrawingCanvas.TextFontSize = _Scalebar.FontSize;
                _ActiveDocument.DrawingCanvas.TextFontFamilyName = textFontFamilyName;
                _ActiveDocument.DrawingCanvas.TextFontStyle = FontStyles.Normal;
                _ActiveDocument.DrawingCanvas.TextFontWeight = fontWeight;
                _ActiveDocument.DrawingCanvas.TextFontStretch = FontStretches.ExtraExpanded;
                _ActiveDocument.DrawingCanvas.ActualScale = scale;

                if (_Scalebar.Location == SBLocation.UpperLeft)
                {
                    start = new Point(margin, margin);
                    end = new Point(margin + lineWidth, margin);

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
                        _ActiveDocument.DrawingCanvas.ObjectColor,
                        _ActiveDocument.DrawingCanvas.TextFontSize * scale,
                        _ActiveDocument.DrawingCanvas.TextFontFamilyName,
                        _ActiveDocument.DrawingCanvas.TextFontStyle,
                        _ActiveDocument.DrawingCanvas.TextFontWeight,
                        _ActiveDocument.DrawingCanvas.TextFontStretch,
                        _ActiveDocument.DrawingCanvas.ActualScale);
                }
                else if (_Scalebar.Location == SBLocation.UpperRight)
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
                        _ActiveDocument.DrawingCanvas.ObjectColor,
                        _ActiveDocument.DrawingCanvas.TextFontSize * scale,
                        _ActiveDocument.DrawingCanvas.TextFontFamilyName,
                        _ActiveDocument.DrawingCanvas.TextFontStyle,
                        _ActiveDocument.DrawingCanvas.TextFontWeight,
                        _ActiveDocument.DrawingCanvas.TextFontStretch,
                        _ActiveDocument.DrawingCanvas.ActualScale);
                }
                else if (_Scalebar.Location == SBLocation.LowerLeft)
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
                        _ActiveDocument.DrawingCanvas.ObjectColor,
                        _ActiveDocument.DrawingCanvas.TextFontSize * scale,
                        _ActiveDocument.DrawingCanvas.TextFontFamilyName,
                        _ActiveDocument.DrawingCanvas.TextFontStyle,
                        _ActiveDocument.DrawingCanvas.TextFontWeight,
                        _ActiveDocument.DrawingCanvas.TextFontStretch,
                        _ActiveDocument.DrawingCanvas.ActualScale);
                }
                else if (_Scalebar.Location == SBLocation.LowerRight)
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
                        _ActiveDocument.DrawingCanvas.ObjectColor,
                        _ActiveDocument.DrawingCanvas.TextFontSize * scale,
                        _ActiveDocument.DrawingCanvas.TextFontFamilyName,
                        _ActiveDocument.DrawingCanvas.TextFontStyle,
                        _ActiveDocument.DrawingCanvas.TextFontWeight,
                        _ActiveDocument.DrawingCanvas.TextFontStretch,
                        _ActiveDocument.DrawingCanvas.ActualScale);
                }
            }

            Color color = (Color)ColorConverter.ConvertFromString("#00000000");
            if (_Scalebar.SelectedColor == color)
            {
                _Scalebar.SelectedColor = Colors.White;
            }

            _ActiveDocument.IsShowScalebar = _Scalebar.IsShowScalebar;

            GraphicsBase baseLine = new GraphicsLine(start, end, _Scalebar.HeightInPixels, _Scalebar.SelectedColor, scale);
            _ActiveDocument.DrawingCanvas.Tool = ToolType.None;

            if (_ActiveDocument.DrawingCanvas.Count > 0)
                _ActiveDocument.DrawingCanvas.GraphicsList.Clear();

            // Add scale bar
            _ActiveDocument.DrawingCanvas.GraphicsList.Add(baseLine);

            if (!_Scalebar.IsHideText)
            {
                // Add text
                _ActiveDocument.DrawingCanvas.GraphicsList.Add(baseText);
            }

            _ActiveDocument.DrawingCanvas.RefreshClip();
        }

        #region ApplyScaleBarCommand
        private RelayCommand _ApplyScaleBarCommand = null;
        public ICommand ApplyScaleBarCommand
        {
            get
            {
                if (_ApplyScaleBarCommand == null)
                {
                    _ApplyScaleBarCommand = new RelayCommand(this.ExecuteApplyScaleBarCommand, this.CanExecuteApplyScaleBarCommand);
                }
                return _ApplyScaleBarCommand;
            }
        }
        protected void ExecuteApplyScaleBarCommand(object parameter)
        {
            if (_ActiveDocument != null)
            {
                try
                {
                    _ActiveDocument.IsShowScalebar = _Scalebar.IsShowScalebar;

                    SerializationHelper helper = new SerializationHelper(ActiveDocument.DrawingCanvas.GraphicsList);
                    _ActiveDocument.ImageInfo.DrawingGraphics = helper.Graphics;
                    _ActiveDocument.Scalebar = _Scalebar;

                    //System.Xml.Serialization.XmlSerializer xml = new System.Xml.Serialization.XmlSerializer(typeof(SerializationHelper));
                    //xml.Serialize(helper);
                    //_ActiveDocument.ImageInfo.DrawingGraphics = ConvertXmlToByteArray(helper.Graphics;

                    //foreach (var graphic in helper.Graphics)
                    //{
                    //    _ActiveDocument.ImageInfo.DrawingGraphics = helper.Graphics;
                    //}
                }
                catch (Exception ex)
                {
                    string caption = "Scale Bar...";
                    string message = string.Format("Error setting scale bar parameters.\nERROR: {0}", ex.Message);
                    Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                }
            }
        }
        protected bool CanExecuteApplyScaleBarCommand(object parameter)
        {
            return true;
        }
        #endregion

        private byte[] ConvertXmlToByteArray(System.Xml.XmlDocument xml, Encoding encoding)
        {
            using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
            {
                System.Xml.XmlWriterSettings settings = new System.Xml.XmlWriterSettings();

                settings.Encoding = encoding;
                settings.OmitXmlDeclaration = true;

                using (System.Xml.XmlWriter writer = System.Xml.XmlWriter.Create(stream, settings))
                {
                    xml.Save(writer);
                }

                return stream.ToArray();
            }
        }

        private System.Xml.Linq.XElement ConvertByteArrayToXml(byte[] data)
        {
            System.Xml.XmlReaderSettings settings = new System.Xml.XmlReaderSettings();

            using (System.IO.MemoryStream stream = new System.IO.MemoryStream(data))
            using (System.Xml.XmlReader reader = System.Xml.XmlReader.Create(stream, settings))
            {
                return System.Xml.Linq.XElement.Load(reader);
            }
        }


    }
}
