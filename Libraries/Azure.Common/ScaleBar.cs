using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Azure.Common
{
    [Serializable]
    public enum UnitOfLength
    {
        [Description("cm")]
        cm,
        [Description("inch")]
        inch,
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
    public class ScaleBar
    {
        #region Private fields...

        private double _Margin;
        private double _LineTextMargin;
        private double _DistanceInPixels;
        //private double _KnownDistance;
        private UnitOfLength _UnitOfLength = UnitOfLength.cm;

        private double _WidthInInchOrCm;
        private double _HeightInPixels;
        private double _FontSize;
        [NonSerialized]
        private Color _SelectedColor;
        [NonSerialized]
        private Color _SelectedBgColor;
        private string _ForegroundColor;
        private string _BackgroundColor;
        private SBLocation _Location = SBLocation.LowerRight;
        private bool _IsBoldText = false;
        private bool _IsHideText = false;
        private bool _IsOverlay = false;
        private bool _IsLabelAllChannels = false;
        private bool _IsShowScalebar = false;

        #endregion

        #region Constructors...
        public ScaleBar()
        {
            _Margin = 20;
            _LineTextMargin = 5;
            //_KnownDistance = 1.00;
            _WidthInInchOrCm = 1;
            _UnitOfLength = UnitOfLength.cm;
            _HeightInPixels = 2;
            _FontSize = 20;
            _SelectedColor = Colors.White;
            _SelectedBgColor = Colors.Transparent;
            _ForegroundColor = _SelectedColor.ToString();
            _BackgroundColor = _SelectedBgColor.ToString();
        }
        #endregion

        #region Public properties...

        public double Margin
        {
            get { return _Margin; }
            set { _Margin = value; }
        }
        public double LineTextMargin
        {
            get { return _LineTextMargin; }
            set { _LineTextMargin = value; }
        }

        public double DistanceInPixels
        {
            get { return _DistanceInPixels; }
            set { _DistanceInPixels = value; }
        }
        //public double KnownDistance
        //{
        //    get { return _KnownDistance; }
        //    set { _KnownDistance = value; }
        //}
        public UnitOfLength UnitOfLength
        {
            get { return _UnitOfLength; }
            set { _UnitOfLength = value; }
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
            set { _WidthInInchOrCm = value; }
        }

        public SBLocation Location
        {
            get { return _Location; }
            set { _Location = value; }
        }

        public double HeightInPixels
        {
            get { return _HeightInPixels; }
            set { _HeightInPixels = value; }
        }
        public double FontSize
        {
            get { return _FontSize; }
            set { _FontSize = value; }
        }

        public Color SelectedColor
        {
            get { return _SelectedColor; }
            set
            {
                _SelectedColor = value;
                if (_SelectedColor != null)
                    ForegroundColor = _SelectedColor.ToString();
            }
        }
        public string ForegroundColor
        {
            get { return _ForegroundColor; }
            set { _ForegroundColor = value; }
        }

        public Color SelectedBgColor
        {
            get { return _SelectedBgColor; }
            set
            {
                _SelectedBgColor = value;
                if (_SelectedBgColor != null)
                    BackgroundColor = _SelectedBgColor.ToString();
            }
        }
        public string BackgroundColor
        {
            get { return _BackgroundColor; }
            set { _BackgroundColor = value; }
        }

        public bool IsBoldText
        {
            get { return _IsBoldText; }
            set { _IsBoldText = value; }
        }
        public bool IsHideText
        {
            get { return _IsHideText; }
            set { _IsHideText = value; }
        }
        public bool IsOverlay
        {
            get { return _IsOverlay; }
            set { _IsOverlay = value; }
        }
        public bool IsLabelAllChannels
        {
            get { return _IsLabelAllChannels; }
            set { _IsLabelAllChannels = value; }
        }
        public bool IsShowScalebar
        {
            get { return _IsShowScalebar; }
            set { _IsShowScalebar = value; }
        }

        #endregion

        public object Clone()
        {
            ScaleBar clone = (ScaleBar)this.MemberwiseClone();
            clone._ForegroundColor = string.Copy(this._ForegroundColor);
            clone._BackgroundColor = string.Copy(this._BackgroundColor);
            clone._UnitOfLength = this._UnitOfLength;
            clone._SelectedColor = this._SelectedColor;
            clone._SelectedBgColor = this._SelectedBgColor;
            clone._Location = this._Location;
            return clone;
        }
    }

}
