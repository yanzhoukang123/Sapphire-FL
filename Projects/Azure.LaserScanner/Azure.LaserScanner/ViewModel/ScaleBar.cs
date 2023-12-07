using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Azure.WPF.Framework;

namespace Azure.LaserScanner.ViewModel
{
    [Serializable]
    public enum UnitOfLength
    {
        [Description("inch")]
        inch,
        [Description("cm")]
        cm,
    }
    [Serializable]
    public enum SBLocation
    {
        [Description("Upper Right")]
        UpperRight,
        [Description("Lower Right")]
        LowerRight,
        [Description("Lower Left")]
        LowerLeft,
        [Description("Upper Left")]
        UpperLeft,
    }

    [Serializable]
    public class ScaleBar : ViewModelBase
    {
        public delegate void ScalebarChangedHandler(ScaleBar sender, string changedType);
        public event ScalebarChangedHandler ScalebarChanged;

        #region Private fields...

        private double _DistanceInPixels;
        private double _KnownDistance;
        private UnitOfLength _UnitOfLength = UnitOfLength.inch;

        private double _WidthInInchOrCm;
        private double _HeightInPixels;
        private double _FontSize;
        private Color _SelectedColor;
        private Color _SelectedBgColor;
        private SBLocation _Location = SBLocation.LowerRight;
        private bool _IsBoldText = false;
        private bool _IsHideText = false;
        private bool _IsOverlay = false;
        private bool _IsLabelAllChannels = false;
        private bool _IsShowScalebar = false;

        //private double _ImageWidth;
        //private double _ImageHeight;
        //private double _Margin;
        //private double _LineTextMargin;

        #endregion

        #region Constructors...
        public ScaleBar()
        {
            _KnownDistance = 1.00;
            _UnitOfLength = UnitOfLength.inch;
            _HeightInPixels = 2;
            _FontSize = 20;
            //_Margin = 20;
            //_LineTextMargin = 5;
            _SelectedColor = Colors.White;
            _SelectedBgColor = Colors.Transparent;
        }
        #endregion

        #region Public properties...

        public double DistanceInPixels
        {
            get { return _DistanceInPixels; }
            set
            {
                if (_DistanceInPixels != value)
                {
                    _DistanceInPixels = value;
                    RaisePropertyChanged("DistanceInPixels");
                    if (ScalebarChanged != null)
                    {
                        ScalebarChanged(this, "DistanceInPixels");
                    }
                }
            }
        }
        public double KnownDistance
        {
            get { return _KnownDistance; }
            set
            {
                if (_KnownDistance != value)
                {
                    _KnownDistance = value;
                    RaisePropertyChanged("KnownDistance");
                    if (ScalebarChanged != null)
                    {
                        ScalebarChanged(this, "KnownDistance");
                    }
                }
            }
        }
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
                { UnitOfLength.inch, "inch"},
                { UnitOfLength.cm, "cm"},
            };
        public UnitOfLength UnitOfLength
        {
            get { return _UnitOfLength; }
            set
            {
                if (_UnitOfLength != value)
                {
                    _UnitOfLength = value;
                    RaisePropertyChanged("UnitOfLength");
                    RaisePropertyChanged("UnitOfLengthText");
                    if (ScalebarChanged != null)
                    {
                        ScalebarChanged(this, "UnitOfLength");
                    }
                    /*if (_ActiveDocument != null && _ActiveDocument.Image != null)
                    {
                        var dpi = _ActiveDocument.Image.DpiX;
                        if (_UnitOfLength == UnitOfLength.cm)
                        {
                            DistanceInPixels = dpi / 2.54;
                            WidthInInchOrCm *= 2.54;
                        }
                        else
                        {
                            DistanceInPixels = dpi;
                            WidthInInchOrCm /= 2.54;
                        }
                    }

                    RaisePropertyChanged("UnitOfLengthText");
                    */
                }
            }
        }

        public string UnitOfLengthText
        {
            get
            {
                string unitOfLength = string.Empty;
                if (_UnitOfLength == UnitOfLength.cm)
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
            get { return _WidthInInchOrCm; }
            set
            {
                if (_WidthInInchOrCm != value)
                {
                    _WidthInInchOrCm = value;
                    RaisePropertyChanged("WidthInInchOrCm");
                    if (ScalebarChanged != null)
                    {
                        ScalebarChanged(this, "WidthInInchOrCm");
                    }
                    //if (_ActiveDocument != null && _ActiveDocument.DrawingCanvas != null && _IsShowScalebar)
                    //if (_ActiveDrawingCanvas != null && _IsShowScalebar)
                    //{
                    //    UpdateScaleBar();
                    //}
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
        public SBLocation Location
        {
            get { return _Location; }
            set
            {
                if (_Location != value)
                {
                    if (_Location != value)
                    {
                        _Location = value;
                        RaisePropertyChanged("SelectedLocation");
                        if (ScalebarChanged != null)
                        {
                            ScalebarChanged(this, "Location");
                        }
                        //if (_ActiveDocument != null && _ActiveDocument.DrawingCanvas != null && _IsShowScalebar)
                        //if (_ActiveDocument != null && _ActiveDocument.DrawingCanvas != null && _IsShowScalebar)
                        //if (_ActiveDrawingCanvas != null && _IsShowScalebar)
                        //{
                        //    UpdateScaleBar();
                        //}
                    }
                }
            }
        }

        public double HeightInPixels
        {
            get { return _HeightInPixels; }
            set
            {
                if (_HeightInPixels != value)
                {
                    _HeightInPixels = value;
                    RaisePropertyChanged("HeightInPixels");
                    if (ScalebarChanged != null)
                    {
                        ScalebarChanged(this, "HeightInPixels");
                    }
                    //if (_ActiveDocument != null && _ActiveDocument.DrawingCanvas != null && _IsShowScalebar)
                    //{
                    //    UpdateScaleBar();
                    //}
                }
            }
        }
        public double FontSize
        {
            get { return _FontSize; }
            set
            {
                if (_FontSize != value)
                {
                    _FontSize = value;
                    RaisePropertyChanged("FontSize");
                    if (ScalebarChanged != null)
                    {
                        ScalebarChanged(this, "FontSize");
                    }
                    //if (_ActiveDocument != null && _ActiveDocument.DrawingCanvas != null && _IsShowScalebar)
                    //if (_ActiveDrawingCanvas != null && _IsShowScalebar)
                    //{
                    //    UpdateScaleBar();
                    //}
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
            get { return _SelectedColor; }
            set
            {
                if (_SelectedColor != value)
                {
                    _SelectedColor = value;
                    RaisePropertyChanged("SelectedColor");
                    if (ScalebarChanged != null)
                    {
                        ScalebarChanged(this, "SelectedColor");
                    }
                    //if (_ActiveDocument != null && _ActiveDocument.DrawingCanvas != null && _IsShowScalebar)
                    //{
                    //    _ActiveDocument.DrawingCanvas.ObjectColor = _SelectedColor;
                    //    foreach (var drawObj in _ActiveDocument.DrawingCanvas.GraphicsList)
                    //    {
                    //        if (drawObj is GraphicsLine)
                    //        {
                    //            ((GraphicsLine)drawObj).ObjectColor = _SelectedColor;
                    //        }
                    //        else if (drawObj is GraphicsText)
                    //        {
                    //            ((GraphicsText)drawObj).ObjectColor = _SelectedColor;
                    //        }
                    //    }
                    //    _ActiveDocument.DrawingCanvas.RefreshClip();
                    //}
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
            get { return _SelectedBgColor; }
            set
            {
                _SelectedBgColor = value;
                RaisePropertyChanged("SelectedBgColor");
                if (ScalebarChanged != null)
                {
                    ScalebarChanged(this, "SelectedBgColor");
                }
            }
        }

        public bool IsBoldText
        {
            get { return _IsBoldText; }
            set
            {
                if (_IsBoldText != value)
                {
                    _IsBoldText = value;
                    RaisePropertyChanged("IsBoldText");
                    if (ScalebarChanged != null)
                    {
                        ScalebarChanged(this, "IsBoldText");
                    }
                    //if (_ActiveDocument != null && _ActiveDocument.DrawingCanvas != null && _IsShowScalebar)
                    //if (_ActiveDrawingCanvas != null && _IsShowScalebar)
                    //{
                    //    UpdateScaleBar();
                    //}
                }
            }
        }
        public bool IsHideText
        {
            get { return _IsHideText; }
            set
            {
                if (_IsHideText != value)
                {
                    _IsHideText = value;
                    RaisePropertyChanged("IsHideText");
                    if (ScalebarChanged != null)
                    {
                        ScalebarChanged(this, "IsHideText");
                    }
                    //if (_ActiveDocument != null && _ActiveDocument.DrawingCanvas != null && _IsShowScalebar)
                    //if (_ActiveDrawingCanvas != null && _IsShowScalebar)
                    //{
                    //    UpdateScaleBar();
                    //}
                }
            }
        }
        public bool IsOverlay
        {
            get { return _IsOverlay; }
            set
            {
                _IsOverlay = value;
                RaisePropertyChanged("IsOverlay");
                if (ScalebarChanged != null)
                {
                    ScalebarChanged(this, "IsOverlay");
                }
            }
        }
        public bool IsLabelAllChannels
        {
            get { return _IsLabelAllChannels; }
            set
            {
                _IsLabelAllChannels = value;
                RaisePropertyChanged("IsLabelAllChannels");
                if (ScalebarChanged != null)
                {
                    ScalebarChanged(this, "IsLabelAllChannels");
                }
            }
        }
        public bool IsShowScalebar
        {
            get { return _IsShowScalebar; }
            set
            {
                if (_IsShowScalebar != value)
                {
                    _IsShowScalebar = value;
                    RaisePropertyChanged("IsShowScalebar");
                    if (ScalebarChanged != null)
                    {
                        ScalebarChanged(this, "IsShowScalebar");
                    }
                    //if (_ActiveDocument != null && _IsShowScalebar)
                    //if (_ActiveDrawingCanvas != null && _IsShowScalebar)
                    //{
                    //    UpdateScaleBar();
                    //    _ActiveDocument.IsShowScalebar = _IsShowScalebar;
                    //}
                }
            }
        }

        #endregion
    }

}
