using Azure.WPF.Framework;
using Hywire.FileAccess;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Azure.ScannerTestJig.ViewModule.FocusAdjust
{
    class ZScanChartViewModel : ViewModelBase
    {
        #region Private data
        private EnumerableDataSource<Point> _ChannelA = null;
        private EnumerableDataSource<Point> _ChannelB = null;
        private EnumerableDataSource<Point> _ChannelC = null;
        //private EnumerableDataSource<Point> _ChannelD = null;
        private EnumerableDataSource<Point> _LeftDown = null;
        private EnumerableDataSource<Point> _LeftTop = null;
        private EnumerableDataSource<Point> _RightTop = null;
        private EnumerableDataSource<Point> _RightDown = null;
        private EnumerableDataSource<Point> _Center = null;
        private EnumerableDataSource<Point> _CenterDown = null;
        private EnumerableDataSource<Point> _CenterTop = null;
        private EnumerableDataSource<Point> _Light = null;
        private Visibility _IsFocusVisibility = Visibility.Hidden;
        private Visibility _IsGlassVisibility = Visibility.Hidden;
        private Visibility _CenterVis = Visibility.Hidden;
        private double _ChAMaxPosition = 0;
        private double _ChBMaxPosition = 0;
        private double _ChCMaxPosition = 0;
        //private double _ChDMaxPosition = 0;

        private string _NameOfChannelA = "ChannelA";
        private string _NameOfChannelB = "ChannelB";
        private string _NameOfChannelC = "ChannelC";
        //private string _NameOfChannelD = "ChannelD";

        private int _PointsCountCHA = 0;
        private int _PointsCountCHB = 0;
        private int _PointsCountCHC = 0;

        private int _PointsCount_LeftDown = 0;
        private int _PointsCount_LeftTop = 0;
        private int _PointsCount_RightTop = 0;
        private int _PointsCount_RightDown = 0;
        private int _PointsCount_Light = 0;
        private int _PointsCount_Center = 0;
        private int _PointsCount_CenterDown = 0;
        private int _PointsCount_CenterTop = 0;
        private FileVisit _FileVisit;
        #endregion
        public EnumerableDataSource<Point> ChannelA
        {
            get
            {
                return _ChannelA;
            }
            set
            {
                _ChannelA = value;
                RaisePropertyChanged("ChannelA");
            }

        }
        public EnumerableDataSource<Point> ChannelB
        {
            get
            {
                return _ChannelB;
            }
            set
            {

                _ChannelB = value;
                RaisePropertyChanged("ChannelB");
            }

        }
        public EnumerableDataSource<Point> ChannelC
        {
            get
            {
                return _ChannelC;
            }
            set
            {
                _ChannelC = value;
                RaisePropertyChanged("ChannelC");
            }

        }
        //public EnumerableDataSource<Point> ChannelD
        //{
        //    get
        //    {
        //        return _ChannelD;
        //    }
        //    set
        //    {
        //        _ChannelD = value;
        //        RaisePropertyChanged("ChannelD");
        //    }

        //}
        public EnumerableDataSource<Point> LeftDown
        {
            get
            {
                return _LeftDown;
            }
            set
            {
                _LeftDown = value;
                RaisePropertyChanged("LeftDown");
            }

        }
        public EnumerableDataSource<Point> LeftTop
        {
            get
            {
                return _LeftTop;
            }
            set
            {

                _LeftTop = value;
                RaisePropertyChanged("LeftTop");
            }

        }
        public EnumerableDataSource<Point> RightTop
        {
            get
            {
                return _RightTop;
            }
            set
            {
                _RightTop = value;
                RaisePropertyChanged("RightTop");
            }

        }
        public EnumerableDataSource<Point> RightDown
        {
            get
            {
                return _RightDown;
            }
            set
            {
                _RightDown = value;
                RaisePropertyChanged("RightDown");
            }

        }
        public EnumerableDataSource<Point> Center
        {
            get
            {
                return _Center;
            }
            set
            {
                _Center = value;
                RaisePropertyChanged("Center");
            }

        }
        public EnumerableDataSource<Point> CenterDown
        {
            get
            {
                return _CenterDown;
            }
            set
            {
                _CenterDown = value;
                RaisePropertyChanged("CenterDown");
            }

        }
        public EnumerableDataSource<Point> CenterTop
        {
            get
            {
                return _CenterTop;
            }
            set
            {
                _CenterTop = value;
                RaisePropertyChanged("CenterTop");
            }

        }
        public EnumerableDataSource<Point> Light
        {
            get
            {
                return _Light;
            }
            set
            {

                _Light = value;
                RaisePropertyChanged("Light");
            }

        }
        public double ChAMaxPosition
        {
            get
            {
                return _ChAMaxPosition;
            }
            set
            {
                if (_ChAMaxPosition != value)
                {
                    _ChAMaxPosition = value;
                    RaisePropertyChanged("ChAMaxPosition");
                }
            }
        }
        public double ChBMaxPosition
        {
            get
            {
                return _ChBMaxPosition;
            }
            set
            {
                if (_ChBMaxPosition != value)
                {
                    _ChBMaxPosition = value;
                    RaisePropertyChanged("ChBMaxPosition");
                }
            }
        }
        public double ChCMaxPosition
        {
            get
            {
                return _ChCMaxPosition;
            }
            set
            {
                if (_ChCMaxPosition != value)
                {
                    _ChCMaxPosition = value;
                    RaisePropertyChanged("ChCMaxPosition");
                }
            }
        }
        //public double ChDMaxPosition
        //{
        //    get
        //    {
        //        return _ChDMaxPosition;
        //    }
        //    set
        //    {
        //        if (_ChDMaxPosition != value)
        //        {
        //            _ChDMaxPosition = value;
        //            RaisePropertyChanged("ChDMaxPosition");
        //        }
        //    }
        //}
        public string NameOfChannelA
        {
            get
            {
                return _NameOfChannelA;
            }
            set
            {
                if (_NameOfChannelA != value)
                {
                    _NameOfChannelA = value;
                    RaisePropertyChanged("NameOfChannelA");
                }
            }
        }
        public string NameOfChannelB
        {
            get
            {
                return _NameOfChannelB;
            }
            set
            {
                if (_NameOfChannelB != value)
                {
                    _NameOfChannelB = value;
                    RaisePropertyChanged("NameOfChannelB");
                }
            }
        }
        public string NameOfChannelC
        {
            get
            {
                return _NameOfChannelC;
            }
            set
            {
                if (_NameOfChannelC != value)
                {
                    _NameOfChannelC = value;
                    RaisePropertyChanged("NameOfChannelC");
                }
            }
        }
        //public string NameOfChannelD
        //{
        //    get
        //    {
        //        return _NameOfChannelD;
        //    }
        //    set
        //    {
        //        if (_NameOfChannelD != value)
        //        {
        //            _NameOfChannelD = value;
        //            RaisePropertyChanged("NameOfChannelD");
        //        }
        //    }
        //}
        public int PointsCountCHA
        {
            get
            {
                _PointsCountCHA = 0;
                if (ChannelA != null)
                {
                    foreach (Point point in ChannelA.Data)
                    {
                        _PointsCountCHA++;
                    }
                }
                return _PointsCountCHA;
            }
        }
        public int PointsCountCHB
        {
            get
            {
                _PointsCountCHB = 0;
                if (ChannelB != null)
                {
                    foreach (Point point in ChannelB.Data)
                    {
                        _PointsCountCHB++;
                    }
                }
                return _PointsCountCHB;
            }
        }
        public int PointsCountCHC
        {
            get
            {
                _PointsCountCHC = 0;
                if (ChannelC != null)
                {
                    foreach (Point point in ChannelC.Data)
                    {
                        _PointsCountCHC++;
                    }
                }
                return _PointsCountCHC;
            }
        }

        public int PointsCount_LeftDown
        {
            get
            {
                _PointsCount_LeftDown = 0;
                if (LeftDown != null)
                {
                    foreach (Point point in LeftDown.Data)
                    {
                        _PointsCount_LeftDown++;
                    }
                }
                return _PointsCount_LeftDown;
            }
        }
        public int PointsCount_LeftTop
        {
            get
            {
                _PointsCount_LeftTop = 0;
                if (LeftTop != null)
                {
                    foreach (Point point in LeftTop.Data)
                    {
                        _PointsCount_LeftTop++;
                    }
                }
                return _PointsCount_LeftTop;
            }
        }
        public int PointsCount_RightTop
        {
            get
            {
                _PointsCount_RightTop = 0;
                if (RightTop != null)
                {
                    foreach (Point point in RightTop.Data)
                    {
                        _PointsCount_RightTop++;
                    }
                }
                return _PointsCount_RightTop;
            }
        }
        public int PointsCount_RightDown
        {
            get
            {
                _PointsCount_RightDown = 0;
                if (RightDown != null)
                {
                    foreach (Point point in RightDown.Data)
                    {
                        _PointsCount_RightDown++;
                    }
                }
                return _PointsCount_RightDown;
            }
        }

        public int PointsCount_Center
        {
            get
            {
                _PointsCount_Center = 0;
                if (Center != null)
                {
                    foreach (Point point in Center.Data)
                    {
                        _PointsCount_Center++;
                    }
                }
                return _PointsCount_Center;
            }
        }

        public int PointsCount_CenterDown
        {
            get
            {
                _PointsCount_CenterDown = 0;
                if (CenterDown != null)
                {
                    foreach (Point point in CenterDown.Data)
                    {
                        _PointsCount_CenterDown++;
                    }
                }
                return _PointsCount_CenterDown;
            }
        }

        public int PointsCount_CenterTop
        {
            get
            {
                _PointsCount_CenterTop = 0;
                if (CenterTop != null)
                {
                    foreach (Point point in CenterTop.Data)
                    {
                        _PointsCount_CenterTop++;
                    }
                }
                return _PointsCount_CenterTop;
            }
        }

        public int PointsCount_Light
        {
            get
            {
                _PointsCount_Light = 0;
                if (Light != null)
                {
                    foreach (Point point in Light.Data)
                    {
                        _PointsCount_Light++;
                    }
                }
                return _PointsCount_Light;
            }
        }
        public Visibility FocusVisibility
        {
            get { return _IsFocusVisibility; }
            set
            {
                if (_IsFocusVisibility != value)
                {
                    _IsFocusVisibility = value;
                    RaisePropertyChanged("FocusVisibility");

                }
            }
        }
        public Visibility GlassVisibility
        {
            get { return _IsGlassVisibility; }
            set
            {
                if (_IsGlassVisibility != value)
                {
                    _IsGlassVisibility = value;
                    RaisePropertyChanged("GlassVisibility");

                }
            }
        }
        public Visibility CenterVis
        {
            get { return _CenterVis; }
            set
            {
                if (_CenterVis != value)
                {
                    _CenterVis = value;
                    RaisePropertyChanged("CenterVis");

                }
            }
        }
        public void InitContorVis()
        {
            FocusVisibility = Visibility.Visible;
            //GlassVisibility = Visibility.Hidden;


        }
        public void SaveFocusData(string savePath)
        {
            IEnumerator<Point> _VisitChannelA = null;
            IEnumerator<Point> _VisitChannelB = null;
            IEnumerator<Point> _VisitChannelC = null;
           // IEnumerator<Point> _VisitChannelD = null;

            int _pointsCount = 0;

            if (ChannelA != null)
            {
                _VisitChannelA = ChannelA.GetPoints().GetEnumerator();
                _pointsCount = PointsCountCHA;
            }
            if (ChannelB != null)
            {
                _VisitChannelB = ChannelB.GetPoints().GetEnumerator();
                if (_pointsCount < PointsCountCHB)
                {
                    _pointsCount = PointsCountCHB;
                }
            }
            if (ChannelC != null)
            {
                _VisitChannelC = ChannelC.GetPoints().GetEnumerator();
                if (_pointsCount < PointsCountCHC)
                {
                    _pointsCount = PointsCountCHC;
                }
            }
            //if (ChannelD != null)
            //{
            //    _VisitChannelD = ChannelD.GetPoints().GetEnumerator();
            //    if (_pointsCount < PointsCountCHD)
            //    {
            //        _pointsCount = PointsCountCHD;
            //    }
            //}

            string[] testdata = new string[_pointsCount];
            for (int y = 0; y < _pointsCount; y++)
            {
                if (_VisitChannelA != null)
                {
                    _VisitChannelA.MoveNext();
                }
                if (_VisitChannelB != null)
                {
                    _VisitChannelB.MoveNext();
                }
                if (_VisitChannelC != null)
                {
                    _VisitChannelC.MoveNext();
                }
                //if (_VisitChannelD != null)
                //{
                //    _VisitChannelD.MoveNext();
                //}

                string _ValueStrCHA = _VisitChannelA == null ? "NULL" : _VisitChannelA.Current.Y.ToString();
                string _ValueStrCHB = _VisitChannelB == null ? "NULL" : _VisitChannelB.Current.Y.ToString();
                string _ValueStrCHC = _VisitChannelC == null ? "NULL" : _VisitChannelC.Current.Y.ToString();
                //string _ValueStrCHD = _VisitChannelD == null ? "NULL" : _VisitChannelD.Current.Y.ToString();
                testdata[y] =
                _ValueStrCHA + "," +
                _ValueStrCHB + "," +
                _ValueStrCHC + ",";
               // _ValueStrCHD + ",";
            }
            _FileVisit = new FileVisit(savePath);
            _FileVisit.Open(FileAccess.ReadWrite);
            _FileVisit.Close();
            File.WriteAllLines(savePath, testdata, Encoding.Unicode);
        }


        public void SaveGlassData(string savePath)
        {
            IEnumerator<Point> _VisitLeftDown = null;
            IEnumerator<Point> _VisitLeftTop = null;
            IEnumerator<Point> _VisitRightTop = null;
            IEnumerator<Point> _VisitRightDown = null;
            IEnumerator<Point> _VisitCenter = null;
            IEnumerator<Point> _VisitCenterDown = null;
            IEnumerator<Point> _VisitCenterTop = null;

            int _pointsCount = 0;

            if (LeftDown != null)
            {
                _VisitLeftDown = LeftDown.GetPoints().GetEnumerator();
                _pointsCount = PointsCount_LeftDown;
            }
            if (LeftTop != null)
            {
                _VisitLeftTop = LeftTop.GetPoints().GetEnumerator();
                if (_pointsCount < PointsCount_LeftTop)
                {
                    _pointsCount = PointsCount_LeftTop;
                }
            }
            if (RightTop != null)
            {
                _VisitRightTop = RightTop.GetPoints().GetEnumerator();
                if (_pointsCount < PointsCount_RightTop)
                {
                    _pointsCount = PointsCount_RightTop;
                }
            }
            if (RightDown != null)
            {
                _VisitRightDown = RightDown.GetPoints().GetEnumerator();
                if (_pointsCount < PointsCount_RightDown)
                {
                    _pointsCount = PointsCount_RightDown;
                }
            }
            if (Center != null)
            {
                _VisitCenter = Center.GetPoints().GetEnumerator();
                if (_pointsCount < PointsCount_Center)
                {
                    _pointsCount = PointsCount_Center;
                }
            }

            if (CenterDown != null)
            {
                _VisitCenterDown = CenterDown.GetPoints().GetEnumerator();
                if (_pointsCount < PointsCount_CenterDown)
                {
                    _pointsCount = PointsCount_CenterDown;
                }
            }

            if (CenterTop != null)
            {
                _VisitCenterTop = CenterTop.GetPoints().GetEnumerator();
                if (_pointsCount < PointsCount_CenterTop)
                {
                    _pointsCount = PointsCount_CenterTop;
                }
            }

            string[] testdata = new string[_pointsCount];
            for (int y = 0; y < _pointsCount; y++)
            {
                if (_VisitLeftDown != null)
                {
                    _VisitLeftDown.MoveNext();
                }
                if (_VisitLeftTop != null)
                {
                    _VisitLeftTop.MoveNext();
                }
                if (_VisitRightTop != null)
                {
                    _VisitRightTop.MoveNext();
                }
                if (_VisitRightDown != null)
                {
                    _VisitRightDown.MoveNext();
                }
                if (_VisitCenter != null)
                {
                    _VisitCenter.MoveNext();
                }
                if (_VisitCenterDown != null)
                {
                    _VisitCenterDown.MoveNext();
                }
                if (_VisitCenterTop != null)
                {
                    _VisitCenterTop.MoveNext();
                }

                string _ValueStrLeftDown = _VisitLeftDown == null ? "NULL" : _VisitLeftDown.Current.Y.ToString();
                string _ValueStrLeftTop = _VisitLeftTop == null ? "NULL" : _VisitLeftTop.Current.Y.ToString();
                string _ValueStrRightTop = _VisitRightTop == null ? "NULL" : _VisitRightTop.Current.Y.ToString();
                string _ValueStrRightDown = _VisitRightDown == null ? "NULL" : _VisitRightDown.Current.Y.ToString();
                string _ValueStrCenter = _VisitCenter == null ? "NULL" : _VisitCenter.Current.Y.ToString();
                string _ValueStrCenterDown = _VisitCenterDown == null ? "NULL" : _VisitCenterDown.Current.Y.ToString();
                string _ValueStrCenterTop = _VisitCenterTop == null ? "NULL" : _VisitCenterTop.Current.Y.ToString();
                testdata[y] =
                _ValueStrLeftDown + "," +
                _ValueStrLeftTop + "," +
                _ValueStrRightTop + "," +
                 _ValueStrRightDown + "," +
                   _ValueStrCenter + "," +
                       _ValueStrCenterDown + "," +
                           _ValueStrCenterTop + ",";
            }
            _FileVisit = new FileVisit(savePath);
            _FileVisit.Open(FileAccess.ReadWrite);
            _FileVisit.Close();
            File.WriteAllLines(savePath, testdata, Encoding.Unicode);
        }

        public void SaveLightData(string savePath)
        {
            IEnumerator<Point> _VisitLight = null;
            //IEnumerator<Point> _VisitChannelB = null;
            //IEnumerator<Point> _VisitChannelC = null;
            // IEnumerator<Point> _VisitChannelD = null;

            int _pointsCount = 0;

            if (_VisitLight != null)
            {
                _VisitLight = Light.GetPoints().GetEnumerator();
                _pointsCount = PointsCount_Light;
            }
            //if (ChannelB != null)
            //{
            //    _VisitChannelB = ChannelB.GetPoints().GetEnumerator();
            //    if (_pointsCount < PointsCountCHB)
            //    {
            //        _pointsCount = PointsCountCHB;
            //    }
            //}
            //if (ChannelC != null)
            //{
            //    _VisitChannelC = ChannelC.GetPoints().GetEnumerator();
            //    if (_pointsCount < PointsCountCHC)
            //    {
            //        _pointsCount = PointsCountCHC;
            //    }
            //}
            //if (ChannelD != null)
            //{
            //    _VisitChannelD = ChannelD.GetPoints().GetEnumerator();
            //    if (_pointsCount < PointsCountCHD)
            //    {
            //        _pointsCount = PointsCountCHD;
            //    }
            //}

            string[] testdata = new string[_pointsCount];
            for (int y = 0; y < _pointsCount; y++)
            {
                if (Light != null)
                {
                    _VisitLight.MoveNext();
                }
                //if (_VisitChannelB != null)
                //{
                //    _VisitChannelB.MoveNext();
                //}
                //if (_VisitChannelC != null)
                //{
                //    _VisitChannelC.MoveNext();
                //}
                //if (_VisitChannelD != null)
                //{
                //    _VisitChannelD.MoveNext();
                //}

                string _ValueStrLight = _VisitLight == null ? "NULL" : _VisitLight.Current.Y.ToString();
                //string _ValueStrCHB = _VisitChannelB == null ? "NULL" : _VisitChannelB.Current.Y.ToString();
                //string _ValueStrCHC = _VisitChannelC == null ? "NULL" : _VisitChannelC.Current.Y.ToString();
                //string _ValueStrCHD = _VisitChannelD == null ? "NULL" : _VisitChannelD.Current.Y.ToString();
                testdata[y] =
                _ValueStrLight + ",";
                //_ValueStrCHB + "," +
                //_ValueStrCHC + ",";
                // _ValueStrCHD + ",";
            }
            _FileVisit = new FileVisit(savePath);
            _FileVisit.Open(FileAccess.ReadWrite);
            _FileVisit.Close();
            File.WriteAllLines(savePath, testdata, Encoding.Unicode);
        }
    }
}
