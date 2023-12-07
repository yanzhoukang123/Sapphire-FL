using Azure.ScannerEUI.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Azure.ScannerEUI.View
{
    /// <summary>
    /// ScanRegionUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class ScanRegionUserControl : UserControl
    {
        #region private data
        BrushConverter bc = new BrushConverter();
        private string _numberStr;
        private double _x0;
        private double _dx=0;
        private double _y0=0;
        private double _dy=0;
        private double _z0=0;
        private double _dz=0;
        private int _Reslution = 10;
        private int _Quality=1;
        private int _lPag=3;
        private int _r1Pag=3;
        private int _r2Pag=3;
        private int _lApdGain=300;
        private int _r1ApdGain=300;
        private int _r2ApdGain=300;
        private int _lPmtGain=4000;
        private int _r1PmtGain=4000;
        private int _r2PmtGain=4000;
        private double _lPower =0;
        private double _r1Power =0;
        private double _r2Power =0;
        private bool _lLaser = false;
        private bool _r1Laser= false;
        private bool _r2Laser= false;
        private bool _lCaptrue = false;
        private bool _r1Captrue = false;
        private bool _r2Captrue= false;
        private double _BottomImage = 0;
        private double _DeltaFocus = 0;
        private int _Ofimages=0;
        private bool _IsCreateGif = false;
        private string _SelectedFocus = "None";

        #endregion

        #region public data
        public delegate void ReceivedScanDataHandle(int currentIndex);
        public event ReceivedScanDataHandle OnScanDataReceived;
        public int IndexCount = 0;
        public int CurretIndexCount = 0;
        private string backColor = "#FFEFEFEF";
        private string foreColor = "#FFEFEFEF";
        public string BackColor { get => backColor; set => backColor = value; }
        public string ForeColor { get => foreColor; set => foreColor = value; }
        public double X0 { get => _x0; set => _x0 = value; }
        public double Dx { get => _dx; set => _dx = value; }
        public double Y0 { get => _y0; set => _y0 = value; }
        public double Dy { get => _dy; set => _dy = value; }
        public double Z0 { get => _z0; set => _z0 = value; }
        public double Dz { get => _dz; set => _dz = value; }
        public int Reslution { get => _Reslution; set => _Reslution = value; }
        public int Quality { get => _Quality; set => _Quality = value; }
        public int LPag { get => _lPag; set => _lPag = value; }
        public int R1Pag { get => _r1Pag; set => _r1Pag = value; }
        public int R2Pag { get => _r2Pag; set => _r2Pag = value; }
        public int LApdGain { get => _lApdGain; set => _lApdGain = value; }
        public int R1ApdGain { get => _r1ApdGain; set => _r1ApdGain = value; }
        public int R2ApdGain { get => _r2ApdGain; set => _r2ApdGain = value; }
        public int LPmtGain { get => _lPmtGain; set => _lPmtGain = value; }
        public int R1PmtGain { get => _r1PmtGain; set => _r1PmtGain = value; }
        public int R2PmtGain { get => _r2PmtGain; set => _r2PmtGain = value; }
        public double LPower { get => _lPower; set => _lPower = value; }
        public double R1Power { get => _r1Power; set => _r1Power = value; }
        public double R2Power { get => _r2Power; set => _r2Power = value; }
        public bool LLaser { get => _lLaser; set => _lLaser = value; }
        public bool R1Laser { get => _r1Laser; set => _r1Laser = value; }
        public bool R2Laser { get => _r2Laser; set => _r2Laser = value; }
        public bool LCaptrue { get => _lCaptrue; set => _lCaptrue = value; }
        public bool R1Captrue { get => _r1Captrue; set => _r1Captrue = value; }
        public bool R2Captrue { get => _r2Captrue; set => _r2Captrue = value; }
        public string SelectedZStackFocus { get => _SelectedFocus; set => _SelectedFocus = value; }
        public bool IsCreateGif { get => _IsCreateGif; set => _IsCreateGif = value; }
        public double BottomImage { get => _BottomImage; set => _BottomImage = value; }
        public double DeltaFocus { get => _DeltaFocus; set => _DeltaFocus = value; }
        public int Ofimages { get => _Ofimages; set => _Ofimages = value; }
        public string NumberStr { get => _numberStr; set => _numberStr = value; }

        #endregion

        /// <summary>
        /// 获取软件启动后XYZ默认位置
        /// Gets XYZ default location after software startup
        /// </summary>
        public ScanRegionUserControl()
        {
            InitializeComponent();
            X0 = (int)Workspace.This.EthernetController.DeviceProperties.LogicalHomeX;
            Dx = 250;
            Y0 = (int)Workspace.This.EthernetController.DeviceProperties.LogicalHomeY;
            Dy = 250;
            Z0 = (int)Workspace.This.EthernetController.DeviceProperties.ZFocusPosition;
        }
        //鼠标离开当前ROI时更改它的样式，并加载属于它的参数
        // When the mouse moves away from the current ROI, change its style and load its parameters
        public void GridPanel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            BackColor = "#006EB3";
            ForeColor = "#FFEFEFEF";
            _ScanStr.Foreground= (SolidColorBrush)bc.ConvertFrom(ForeColor);
            _Region.Foreground = (SolidColorBrush)bc.ConvertFrom(ForeColor);
            _LbNumber.Foreground = (SolidColorBrush)bc.ConvertFrom(ForeColor);
            GridPanel.Background = (SolidColorBrush)bc.ConvertFrom(BackColor);
            CurretIndexCount = (int)this.Tag;
            OnScanDataReceived(CurretIndexCount);
        }
        public void SetLbNumber()
        {
            this.Tag = IndexCount;
            _LbNumber.Content = "# " + IndexCount;
            _numberStr = _ScanStr.Content + " "+_Region.Content + " "+_LbNumber.Content;
        }
        public void SetBackGrondColor()
        {
            BackColor = "#FFEFEFEF";
            ForeColor = "#0F0E0E";
            _ScanStr.Foreground = (SolidColorBrush)bc.ConvertFrom(ForeColor);
            _Region.Foreground = (SolidColorBrush)bc.ConvertFrom(ForeColor);
            _LbNumber.Foreground = (SolidColorBrush)bc.ConvertFrom(ForeColor);
            GridPanel.Background = (SolidColorBrush)bc.ConvertFrom(BackColor);
        }
    }
}
