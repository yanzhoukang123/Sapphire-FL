using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;   //MessageBox
using System.Windows.Input; //ICommand
using System.Collections.ObjectModel;   //ObservableCollection
using System.Windows.Media.Imaging; //BitmapSource
using Azure.WPF.Framework;
using Azure.ImagingSystem;
using Azure.CommandLib;
using Azure.Image.Processing;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
//using Azure.ImagingSystem;
using Azure.Configuration.Settings;
using System.Windows.Threading;
using Azure.ScannerEUI.View;
using System.Threading;
using System.IO;
using Azure.EthernetCommLib;
using LogW;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Management;

namespace Azure.ScannerEUI.ViewModel
{
    class ScannerViewModel : ViewModelBase
    {
        #region Private data...
        private double YCompenOffset = 0;
        private double _XMotorSubdivision = 0;
        private double _YMotorSubdivision = 0;
        private double _ZMotorSubdivision = 0;
        private int _XMaxValue = 0;
        private int _YMaxValue = 0;
        private int _ZMaxValue = 0;
        private int _ScanX0 = 0;
        private int _ScanY0 = 0;
        private int _ScanZ0 = 0;
        private double _ScanDeltaX = 0;
        private double _ScanDeltaY = 0;
        private double _ScanDeltaZ = 0;
        private int _Width = 0;
        private int _Height = 0;
        private int _Time = 0;

        private int _RemainingTime = 0;
        private int _DataRate = 25;
        private int _LineCounts = 1000;
        private int XHomecoefficient = 0;
        private enum ScanChannel
        { A,B,C,D}
        private ScanChannel _NowShowImage;
        private List<ResolutionType> _ResolutionOptions = null;
        private List<QualityType> _QualityOptions = null;
        private ResolutionType _SelectedResolution = null;
        private QualityType _SelectedQuality = null;

        private int _LaserAIntensity = 0;
        private int _LaserBIntensity = 0;
        private int _LaserCIntensity = 0;
        private int _LaserDIntensity = 0;

        private double _LaserAPower = 0;
        private double _LaserBPower = 0;
        private double _LaserCPower = 0;
        private double _LaserDPower = 0;

        private bool _IsLaserASelected = false;
        private bool _IsLaserBSelected = false;
        private bool _IsLaserCSelected = false;
        private bool _IsLaserDSelected = false;

        private RelayCommand _UseCurrentPosCommand = null;
        private RelayCommand _ScanCommand = null;
        private RelayCommand _StopScanCommand = null;
        private RelayCommand _ChACommand = null;
        private RelayCommand _ChBCommand = null;
        private RelayCommand _ChCCommand = null;
        private RelayCommand _ChDCommand = null;
        //private ImageScanCommand _ImageScanCommand = null;

        private ScanProcessing _ScanningProcess;

        private EnumerableDataSource<Point> _ChannelA = null;
        private EnumerableDataSource<Point> _ChannelB = null;
        private EnumerableDataSource<Point> _ChannelC = null;
        private EnumerableDataSource<Point> _ChannelD = null;
        private EnumerableDataSource<Point> _Commond = null;
        private EnumerableDataSource<Point> _Feedback = null;
        public WriteableBitmap _ChannelAImage = null;
        public WriteableBitmap _ChannelBImage = null;
        public WriteableBitmap _ChannelCImage = null;
        private WriteableBitmap _ChannelDImage = null;
        String _NowChannel = null;
        string _FPGAVersion = null;
        string _HWversion = null;
        string _LEDVersion = null;
        private const string LaserSetErrorMessage = "Laser Power should be set to 0 or above 5 mW";
        private int _Spenttime= 1;
        private string _SelectedChannel = null;   //Channel
        private int _SelectedDynamicBits;
        private string _CurrentScanHeaderTitle = string.Empty;   
        private string _CurrentScanVerticaTitle = string.Empty;
        private string _CurrentScanHorizontalTitle = string.Empty;
        private DispatcherTimer ShowTimer = null;
        private Thread dynamThread = null;
        private bool dynamSwitch = false;
        private Visibility _ChannelVisibility = Visibility.Visible;
        private Visibility _CommondFeedback = Visibility.Hidden;
        private int _ScanWorkCount = 0;
        private bool _FirstDx = false;
        private Thread ScanDynamicScopeThread = null;
        public string ScanDynamicScopeString = "";
        public string ScanDynamicScopeStringType = "";
        private int _StackNum = 0;
        private int _RangeNum = 0;
        private bool _IsScanTest = false;
        private  int _CurrentRangeWidth = 0;
        private int _CurrentRangeHeight = 0;
        public double _RangeDx = 0;
        public double _RangeDy = 0;
        private bool ycompenSationBitAt = false;
		private string ROIWarning = string.Empty;
        private int _CurrentScanWorkIndex = 0;
        private bool _ClickScanWork = false;
        #endregion

        #region Constructors...


        public ScannerViewModel()
        {
            //获取是否启用了Y轴图像处理（避免Y轴刚启动时照成的前几行有倾斜）
            //Get whether the Y-axis image processing is enabled (to avoid the tilt of the first few lines when the Y-axis is first started)
            ycompenSationBitAt = SettingsManager.ConfigSettings.YCompenSationBitAt;
            //截取掉Y轴的行数，（YCompenOffset*1000/res）
            YCompenOffset = SettingsManager.ConfigSettings.YCompenOffset;
            /*_ResolutionOptions = SettingsManager.ConfigSettings.ResolutionOptions;
            _XMotorSubdivision = SettingsManager.ConfigSettings.XMotorSubdivision;
            _YMotorSubdivision = SettingsManager.ConfigSettings.YMotorSubdivision;
            _ZMotorSubdivision = SettingsManager.ConfigSettings.ZMotorSubdivision;
            _XMaxValue = SettingsManager.ConfigSettings.XMaxValue;
            _YMaxValue = SettingsManager.ConfigSettings.YMaxValue;
            _ZMaxValue = SettingsManager.ConfigSettings.ZMaxValue;
            NowChannel = "APD CHA";
            if (_ResolutionOptions != null && _ResolutionOptions.Count > 0)
            {
                _SelectedResolution = _ResolutionOptions[0];    // select the first item
            }

            _QualityOptions = SettingsManager.ConfigSettings.QualityOptions;
            if (_QualityOptions != null && _QualityOptions.Count > 0)
            {
                _SelectedQuality = _QualityOptions[0];  // select the first item
            }*/
            DynamicBitsOptions = new List<int>();
            DynamicBitsOptions.AddRange(new int[] { 0, 1, 2 });
            SelectedDynamicBits = DynamicBitsOptions[0];
            OptionsChannels = new List<string>();
            OptionsChannels.AddRange(new string[] { "L", "R1", "R2" });
            //实时显示当前扫描的图像
            ///Display the currently scanned image in real time
            dynamThread = new Thread(new System.Threading.ThreadStart(ScanDynamicDisplay));
            dynamThread.IsBackground = true;
            dynamThread.Start();
            ScanDynamicScopeThread = new Thread(new System.Threading.ThreadStart(ScanDynamicScopeSettins));
            ScanDynamicScopeThread.IsBackground = true;
            ScanDynamicScopeThread.Start();
        }
        #endregion

        #region Public properties...
        public delegate void SetScanWorkLaserEvent(int Current);
        public event SetScanWorkLaserEvent SetRegionScanWorkLaserReceived;
        public delegate void GetScanWorkEvent(int Current);
        public event GetScanWorkEvent GetRegionScanWorkReceived;
        public delegate void SetScanWorkEvent(int workIndex,ref bool loaded);
        public event SetScanWorkEvent OnScanWorkReceived;
        public delegate void SetScanRegionEvent(string name);
        public event SetScanRegionEvent OnScanRegionReceived;
        public delegate void SetScanEnabledRegionEvent(bool isEnable);
        public event SetScanEnabledRegionEvent OnScanEnbledRegionReceived;
        //开放线程
        public ScanProcessing ScanProcessingAgingThread
        {
            get { return _ScanningProcess; }
        }
        //Total number of multi region tasks
        public int ScanWorkCount 
        {
            get { return _ScanWorkCount; }
            set
            {
                if (_ScanWorkCount != value)
                {
                    _ScanWorkCount = value;
                }
            }
        }
        //Current scan work index
        public int CurrentScanWorkIndex
        {
            get { return _CurrentScanWorkIndex; }
            set
            {
                if (_CurrentScanWorkIndex != value)
                {
                    _CurrentScanWorkIndex = value;
                }
            }
        }

        public bool ClickScanWork
        {
            get { return _ClickScanWork; }
            set
            {
                if (_ClickScanWork != value)
                {
                    _ClickScanWork = value;
                }
            }
        }

        public int SpentTime
        {
            get { return _Spenttime; }
            set
            {
                if (_Spenttime != value)
                {
                    _Spenttime = value;
                    RaisePropertyChanged("SpentTime");
                 
                }
            }
        }
        //HW
        public string HWversion
        {
            get
            {
                return _HWversion;
            }
            set
            {
                _HWversion = value;
                RaisePropertyChanged("HWversion");
            }
        }
        //FW
        public string FPGAVersion
        {
            get
            {
                return _FPGAVersion;
            }
            set
            {
                _FPGAVersion = value;
                RaisePropertyChanged("FPGAVersion");
            }
        }
        //LED HW
        public string LEDVersion
        {
            get
            {
                return _LEDVersion;
            }
            set
            {
                _LEDVersion = value;
            }
        }
        public String NowChannel
        {
            get
            {
                return _NowChannel;
            }
            set
            {
                if (_NowChannel != value)
                {
                    _NowChannel = value;
                    RaisePropertyChanged("NowChannel");
                }
            }

        }
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

        public EnumerableDataSource<Point> ChannelD
        {
            get
            {
                return _ChannelD;
            }
            set
            {
                _ChannelD = value;
                RaisePropertyChanged("ChannelD");
            }

        }
        public EnumerableDataSource<Point> Commond
        {
            get
            {
                return _Commond;
            }
            set
            {
                _Commond = value;
                RaisePropertyChanged("Commond");
            }

        }
        public EnumerableDataSource<Point> Feedback
        {
            get
            {
                return _Feedback;
            }
            set
            {
                _Feedback = value;
                RaisePropertyChanged("Feedback");
            }

        }
        public Visibility ChannelVisibility
        {
            get
            {
                return _ChannelVisibility;

            }
            set
            {
                _ChannelVisibility = value;
                RaisePropertyChanged("ChannelVisibility");
            }


        }
        public Visibility CommondFeedbackVisibility
        {
            get
            {
                return _CommondFeedback;

            }
            set
            {
                _CommondFeedback = value;
                RaisePropertyChanged("CommondFeedbackVisibility");
            }
        }
        public double ScanX0
        {
            get
            {
                double dRetVal = 0;
                if (_XMotorSubdivision != 0)
                {
                    dRetVal = Math.Round((double)_ScanX0 / (double)_XMotorSubdivision, 3);
                }
                return dRetVal;
            }
            set
            {
                //No need to calculate whether or not to pop-up reminders when selecting tasks
                if (ClickScanWork)
                {
                    if ((double)(_ScanX0 / (double)_XMotorSubdivision) != value)
                    {
                        _ScanX0 = (int)(value * _XMotorSubdivision);
                        RaisePropertyChanged("ScanX0");
                        OnScanRegionReceived("ScanX0");
                    }
                }
                else
                {
                    if (((double)_ScanX0 / (double)_XMotorSubdivision) != value)
                    {
                        if (value >= 0 && value <= (XHomecoefficient - (Workspace.This.EthernetController.DeviceProperties.OpticalLR2Distance)))
                        {
                            _ScanX0 = (int)(value * _XMotorSubdivision);
                        }
                        if (value + (_ScanDeltaX / _XMotorSubdivision) > (XHomecoefficient - (Workspace.This.EthernetController.DeviceProperties.OpticalLR2Distance)))
                        {
                            _ScanX0 = (int)(value * _XMotorSubdivision);
                            ScanDynamicScopeString = String.Format("The X should be 0=<X0+DX<={0}", XHomecoefficient - (Workspace.This.EthernetController.DeviceProperties.OpticalLR2Distance));
                            ScanDynamicScopeStringType = "Error";
                            //Xceed.Wpf.Toolkit.MessageBox.Show(String.Format("you should type value 0-{0}", 310 - (Workspace.This.EthernetController.DeviceProperties.OpticalLR2Distance)), "Error");
                            //System.Windows.Forms.MessageBox.Show(String.Format("you should type value 0-{0}", 310 - (Workspace.This.EthernetController.DeviceProperties.OpticalLR2Distance)), "Error");
                        }
                        RaisePropertyChanged("ScanX0");
                        OnScanRegionReceived("ScanX0");
                    }
                }
            }
        }

        public double ScanY0
        {
            get
            {
                double dRetVal = 0;
                if (_YMotorSubdivision != 0)
                {
                    dRetVal = Math.Round((double)_ScanY0 / (double)_YMotorSubdivision, 3);
                }
                return dRetVal;
            }
            set
            {
                //No need to calculate whether or not to pop-up reminders when selecting tasks
                if (ClickScanWork)
                {
                    if ((double)(_ScanY0 / (double)_YMotorSubdivision) != value)
                    {
                        _ScanY0 = (int)(value * _YMotorSubdivision);
                        RaisePropertyChanged("ScanY0");
                        OnScanRegionReceived("ScanY0");
                    }
                }
                else
                {
                    if ((double)(_ScanY0 / (double)_YMotorSubdivision) != value)
                    {
                        if (value >= 0 && value <= ((double)_YMaxValue / (double)_YMotorSubdivision) - 10)
                        {
                            _ScanY0 = (int)(value * _YMotorSubdivision);
                        }
                        if (value + (_ScanDeltaY / _YMotorSubdivision) > ((double)_YMaxValue / (double)_YMotorSubdivision) - 5)
                        {
                            _ScanY0 = (int)(value * _YMotorSubdivision);
                            //ScanDynamicScopeString = String.Format("you should type value 0-{0}", ((double)_YMaxValue / (double)_YMotorSubdivision) - 10);
                            ScanDynamicScopeString = String.Format("The Y should be 0=<Y0+DY<={0}", ((double)_YMaxValue / (double)_YMotorSubdivision) - 5);
                            ScanDynamicScopeStringType = "Error";
                            //Xceed.Wpf.Toolkit.MessageBox.Show(String.Format("you should type value 0-{0}", ((double)_YMaxValue / (double)_YMotorSubdivision) - 10), "Error");
                            //System.Windows.Forms.MessageBox.Show(String.Format("you should type value 0-{0}", ((double)_YMaxValue / (double)_YMotorSubdivision)-10), "Error");
                        }
                        RaisePropertyChanged("ScanY0");
                        OnScanRegionReceived("ScanY0");
                    }
                }

            }
        }

        public double ScanZ0
        {
            get
            {
                double dRetVal = 0;
                if (_ZMotorSubdivision != 0)
                {
                    dRetVal = Math.Round((double)_ScanZ0 / (double)_ZMotorSubdivision, 3);
                }
                return dRetVal;
            }
            set
            {
                //No need to calculate whether or not to pop-up reminders when selecting tasks
                if (ClickScanWork)
                {
                    if ((double)(_ScanZ0 / (double)_ZMotorSubdivision) != value)
                    {
                        _ScanZ0 = (int)(value * _ZMotorSubdivision);
                        RaisePropertyChanged("ScanZ0");
                        OnScanRegionReceived("ScanZ0");
                    }
                }
                else
                {
                    if ((double)_ScanZ0 / (double)_ZMotorSubdivision != value)
                    {
                        if (value >= 0 && value <= ((double)_ZMaxValue / (double)_ZMotorSubdivision))
                        {
                            _ScanZ0 = (int)(value * _ZMotorSubdivision);
                        }
                        if (value + (_ScanDeltaZ / _ZMotorSubdivision) > (double)_ZMaxValue / (double)_ZMotorSubdivision)
                        {
                            _ScanZ0 = (int)(value * _ZMotorSubdivision);
                            //ScanDynamicScopeString = String.Format("The Z you should type value 0-{0}", (double)_ZMaxValue / (double)_ZMotorSubdivision);
                            ScanDynamicScopeString = String.Format("The Z should be 0=<Z0+DZ<={0}", (double)_ZMaxValue / (double)_ZMotorSubdivision);
                            ScanDynamicScopeStringType = "Error";
                            //Xceed.Wpf.Toolkit.MessageBox.Show(String.Format("you should type value 0-{0}", (double)_ZMaxValue / (double)_ZMotorSubdivision), "Error");
                            //System.Windows.Forms.MessageBox.Show(String.Format("you should type value 0-{0}", (double)_ZMaxValue / (double)_ZMotorSubdivision), "Error");
                        }
                        RaisePropertyChanged("ScanZ0");
                        OnScanRegionReceived("ScanZ0");
                    }
                }
            }
        }
        private int indexDeltaX = 0;

        public double ScanDeltaX
        {
            get
            {
                double dRetVal = 0;
                if (_XMotorSubdivision != 0)
                {
                    dRetVal = _ScanDeltaX / _XMotorSubdivision;
                }
                return dRetVal;
            }
            set
            {
                /**/
                ;
                if ((_ScanDeltaX / _XMotorSubdivision) != value)
                {
                    double SDValue = value;
                    if (_FirstDx)
                    {
                        SDValue += Workspace.This.EthernetController.DeviceProperties.OpticalLR2Distance;
                    }
                    if (SDValue < 0 || SDValue > XHomecoefficient)
                    {
                        indexDeltaX++;
                        value = XHomecoefficient - (Workspace.This.EthernetController.DeviceProperties.OpticalLR2Distance);
                        if (indexDeltaX != 2)
                        {
                            ScanDynamicScopeString = "The DX ranges from 0 to " + (int)value;
                            ScanDynamicScopeStringType = "Warning";
                            //Xceed.Wpf.Toolkit.MessageBox.Show(String.Format("The DX ranges from 0 to " + (int)value + ""), "Warning");
                            //System.Windows.Forms.MessageBox.Show(String.Format("The DX ranges from 0 to " + (int)value + ""), "Warning");
                            // MessageBox.Show(String.Format("The DX ranges from 0 to " + (int)value + "", "Warning"));
                        }
                        else
                        {
                            indexDeltaX = 0;
                        }
                    }
                    if (value >= 0 && ((value + ScanX0) <= XHomecoefficient - (Workspace.This.EthernetController.DeviceProperties.OpticalLR2Distance)))
                    {
                        _ScanDeltaX = value * _XMotorSubdivision;
                        Width = (int)((ScanDeltaX * 1000) / SelectedResolution.Value);
                    }
                    else
                    {
                        _ScanDeltaX = 0;
                        //ScanDynamicScopeString = String.Format("The DX should be 0=<X0+DX<={0}", XHomecoefficient - (Workspace.This.EthernetController.DeviceProperties.OpticalLR2Distance));
                        ScanDynamicScopeString = String.Format("The X should be 0=<X0+DX<={0}", XHomecoefficient - (Workspace.This.EthernetController.DeviceProperties.OpticalLR2Distance));
                        ScanDynamicScopeStringType = "Error";
                        //Xceed.Wpf.Toolkit.MessageBox.Show(String.Format("The DX should be 0=<X0+DX<={0}", 310 - (Workspace.This.EthernetController.DeviceProperties.OpticalLR2Distance)), "Error");
                        //System.Windows.Forms.MessageBox.Show(String.Format("The DX should be 0=<X0+DX<={0}", 310 - (Workspace.This.EthernetController.DeviceProperties.OpticalLR2Distance)), "Error");
                        //MessageBox.Show(String.Format("The DX should be 0=<X0+DX<={0}", 310 - (Workspace.This.EthernetController.DeviceProperties.OpticalLR2Distance)), "Error");
                    }
                    RaisePropertyChanged("ScanDeltaX");
                    OnScanRegionReceived("ScanDeltaX");
                }
            }
        }
        public double ScanDeltaY
        {
            get
            {
                double dRetVal = 0;
                if (_YMotorSubdivision > 0)
                {
                    dRetVal = _ScanDeltaY / _YMotorSubdivision;
                }
                return dRetVal;
            }
            set
            {
                if ((_ScanDeltaY / _YMotorSubdivision) != value)
                {
                    if (value >= 0 && ((value + ScanY0) <= ((double)_YMaxValue / (double)_YMotorSubdivision) - 5))
                    {
                        _ScanDeltaY = value * _YMotorSubdivision;
                    }
                    else
                    {
                        _ScanDeltaY = 0;
                        //ScanDynamicScopeString = String.Format("The DY should be 0=<Y0+DY<={0}", ((double)_YMaxValue / (double)_YMotorSubdivision) - 5);
                        ScanDynamicScopeString = String.Format("The Y should be 0=<Y0+DY<={0}", ((double)_YMaxValue / (double)_YMotorSubdivision) - 5);
                        ScanDynamicScopeStringType = "Error";
                        // Xceed.Wpf.Toolkit.MessageBox.Show(String.Format("The DY should be 0=<Y0+DY<={0}", ((double)_YMaxValue / (double)_YMotorSubdivision) - 10), "Error");
                        //System.Windows.Forms.MessageBox.Show(String.Format("The DY should be 0=<Y0+DY<={0}", ((double)_YMaxValue / (double)_YMotorSubdivision)-10), "Error");
                    }
                    Height = (int)((ScanDeltaY * 1000) / SelectedResolution.Value);
                    if (ycompenSationBitAt && (int)((YCompenOffset * 1000) / SelectedResolution.Value) >= 1)
                    {
                        Height += (int)((YCompenOffset * 1000) / SelectedResolution.Value);
                    }
                    RaisePropertyChanged("ScanDeltaY");
                    OnScanRegionReceived("ScanDeltaY");
                }
            }
        }

        public double ScanDeltaZ
        {
            get
            {
                double dRetVal = 0.0;
                if (_ZMotorSubdivision != 0)
                {
                    dRetVal = Math.Round((double)_ScanDeltaZ / (double)_ZMotorSubdivision, 3);
                }
                return dRetVal;
            }
            set
            {
                if (((double)_ScanDeltaZ / (double)_ZMotorSubdivision) != value)
                {
                    if (value >= 0 && ((value + ScanZ0) <= ((double)_ZMaxValue / (double)_ZMotorSubdivision)))
                    {
                        _ScanDeltaZ = (int)(value * _ZMotorSubdivision);
                    }
                    else
                    {
                        _ScanDeltaZ = 0;
                        //ScanDynamicScopeString = String.Format("The DZ should be 0=<Z0+DZ<={0}", (double)_ZMaxValue / (double)_ZMotorSubdivision);
                        ScanDynamicScopeString = String.Format("The Z should be 0=<Z0+DZ<={0}", (double)_ZMaxValue / (double)_ZMotorSubdivision);
                        ScanDynamicScopeStringType = "Error";
                        //Xceed.Wpf.Toolkit.MessageBox.Show(String.Format("The DZ should be 0=<Z0+DZ<={0}", (double)_ZMaxValue / (double)_ZMotorSubdivision), "Error");
                        //System.Windows.Forms.MessageBox.Show(String.Format("The DZ should be 0=<Z0+DZ<={0}", (double)_ZMaxValue / (double)_ZMotorSubdivision), "Error");
                    }
                    RaisePropertyChanged("ScanDeltaZ");
                    OnScanRegionReceived("ScanDeltaZ");
                }
            }
        }

        public int Width
        {
            get { return _Width; }
            set
            {
                if (_Width != value)
                {
                    _Width = value;
                    RaisePropertyChanged("Width");
                }
            }
        }

        public int Height
        {
            get { return _Height; }
            set
            {
                if (_Height != value)
                {
                    _Height = value;
                    if (SettingsManager.ConfigSettings.AllModuleProcessing)
                    {
                        Time = _Height * SelectedQuality.Value;
                    }
                    else
                    {
                        Time = _Height * SelectedQuality.Value / 2;
                    }
                    RaisePropertyChanged("Height");
                }
            }
        }

        public int Time
        {
            get { return _Time; }
            set
            {
                if (_Time != value)
                {
                    _Time = value;
                    RaisePropertyChanged("Time");
                }
            }
        }

        public int RemainingTime
        {
            get { return _RemainingTime; }
            set
            {
                if (_RemainingTime != value)
                {
                    _RemainingTime = value;
                    RaisePropertyChanged("RemainingTime");
                }
            }
        }

        public int DataRate
        {
            get { return _DataRate; }
            set
            {
                if (_DataRate != value)
                {
                    _DataRate = value;
                    if (_DataRate < 25)
                    {
                        _DataRate = 25;
                    }
                    RaisePropertyChanged("DataRate");
                    //if (Workspace.This.ApdVM.APDTransfer.APDTransferIsAlive == true)
                    //{
                    //    Workspace.This.ApdVM.APDTransfer.APDLaserScanDataRateSet(_DataRate);
                    //}
                }
            }
        }

        public int LineCounts
        {
            get { return _LineCounts; }
            set
            {
                if (_LineCounts != value)
                {
                    _LineCounts = value;
                    RaisePropertyChanged("Lines");
                    //if (Workspace.This.ApdVM.APDTransfer.APDTransferIsAlive == true)
                    //{
                    //    Workspace.This.ApdVM.APDTransfer.APDLaserScanLineCountsSet(_LineCounts);
                    //}
                }
            }
        }

        public List<ResolutionType> ResolutionOptions
        {
            
            get { return _ResolutionOptions; }
        }

        public List<QualityType> QualityOptions
        {
            get { return _QualityOptions; }
        }

        public ResolutionType SelectedResolution
        {
            get { return _SelectedResolution; }
            set
            {                
                if (_SelectedResolution != value)
                {
                    _SelectedResolution = value;
                    Width = (int)((ScanDeltaX * 1000 ) / SelectedResolution.Value);
                    Height = (int)((ScanDeltaY * 1000) / SelectedResolution.Value);
                    if (ycompenSationBitAt && (int)((YCompenOffset * 1000) / SelectedResolution.Value) >= 1)
                    {
                        Height += (int)((YCompenOffset * 1000) / SelectedResolution.Value);
                    }
                    RaisePropertyChanged("SelectedResolution");
                    OnScanRegionReceived("SelectedResolution");
                    //System.Windows.MessageBox.Show(string.Format("Resolution selected: {0}", _SelectedResolution.Value));                                 
                    //if (Workspace.This.ApdVM.APDTransfer.APDTransferIsAlive == true)
                    //{
                    //    Workspace.This.ApdVM.APDTransfer.APDLaserScanResSet(_SelectedResolution.Value);
                    //}
                }
            }
        }

        public QualityType SelectedQuality
        {
            get { return _SelectedQuality; }
            set
            {
                if (_SelectedQuality != value)
                {
                    _SelectedQuality = value;
                    Time = _Height * SelectedQuality.Value / 2;
                    RaisePropertyChanged("SelectedQuality");
                    OnScanRegionReceived("SelectedQuality");
                    //System.Windows.MessageBox.Show(string.Format("Quality selected: {0}", _SelectedQuality.Value));
                    //if (Workspace.This.ApdVM.APDTransfer.APDTransferIsAlive == true)
                    //{
                    //    Workspace.This.ApdVM.APDTransfer.APDLaserScanQualitySet(_SelectedQuality.Value);
                    //}
                }
            }
        }
        public List<int> DynamicBitsOptions { get; }
        public int SelectedDynamicBits
        {
            get { return _SelectedDynamicBits; }
            set
            {
                if (_SelectedDynamicBits != value)
                {
                    _SelectedDynamicBits = value;
                    RaisePropertyChanged(nameof(SelectedDynamicBits));
                }
            }
        }

        public int LaserAIntensity
        {
            get { return _LaserAIntensity; }
            set
            {
                if (_LaserAIntensity != value)
                {
                    _LaserAIntensity = value;

                    if (_LaserAIntensity > SettingsManager.ConfigSettings.LaserAMaxIntensity || _LaserAIntensity < 0)
                    {
                        string caption = "Laser A...";
                        string message = string.Format("Laser A intensity range: 0 - {0}", SettingsManager.ConfigSettings.LaserAMaxIntensity);
                        MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                        if (_LaserAIntensity < 0)
                        {
                            _LaserAIntensity = 0;
                        }
                        else if (_LaserAIntensity > SettingsManager.ConfigSettings.LaserAMaxIntensity)
                        {
                            _LaserAIntensity = SettingsManager.ConfigSettings.LaserAMaxIntensity;
                            //LaserAPower = Workspace.This.ApdVM.APDTransfer.LaserIntensityToPower(ScannerDataStruct.APDLaserChannelType.A, _LaserAIntensity);
                        }
                    }

                    RaisePropertyChanged("LaserAIntensity");

                    //if (Workspace.This.ApdVM.APDTransfer.APDTransferIsAlive == true)
                    //{
                    //    Workspace.This.ApdVM.APDTransfer.LaserSetA(_LaserAIntensity);
                    //    //Setting the laser's intensity turns on the laser.
                    //    //Trigger laser on/off button update
                    //    if (_LaserAIntensity > 0)
                    //    {
                    //        _IsLaserASelected = true;
                    //    }
                    //    else
                    //    {
                    //        _IsLaserASelected = false;
                    //    }
                    //    RaisePropertyChanged("IsLaserASelected");
                    //} 
                }
            }
        }

        public int LaserBIntensity
        {
            get { return _LaserBIntensity; }
            set
            {
                if (_LaserBIntensity != value)
                {
                    _LaserBIntensity = value;

                    if (_LaserBIntensity > SettingsManager.ConfigSettings.LaserBMaxIntensity || _LaserBIntensity < 0)
                    {
                        string caption = "Laser B...";
                        string message = string.Format("Laser B intensity range: 0 - {0}", SettingsManager.ConfigSettings.LaserBMaxIntensity);
                        MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                        if (_LaserBIntensity < 0)
                        {
                            _LaserBIntensity = 0;
                        }
                        else if (_LaserBIntensity > SettingsManager.ConfigSettings.LaserBMaxIntensity)
                        {
                            _LaserBIntensity = SettingsManager.ConfigSettings.LaserBMaxIntensity;
                            //LaserBPower = Workspace.This.ApdVM.APDTransfer.LaserIntensityToPower(ScannerDataStruct.APDLaserChannelType.B, _LaserBIntensity);
                        }
                    }

                    RaisePropertyChanged("LaserBIntensity");

                    //if (Workspace.This.ApdVM.APDTransfer.APDTransferIsAlive == true)
                    //{
                    //    Workspace.This.ApdVM.APDTransfer.LaserSetB(_LaserBIntensity);
                    //    //Setting the laser's intensity turns on the laser.
                    //    //Trigger laser on/off button update
                    //    if (_LaserBIntensity > 0)
                    //    {
                    //        _IsLaserBSelected = true;
                    //    }
                    //    else
                    //    {
                    //        _IsLaserBSelected = false;
                    //    }
                    //    RaisePropertyChanged("IsLaserBSelected");
                    //} 

                }
            }
        }

        public int LaserCIntensity
        {
            get { return _LaserCIntensity; }
            set
            {
                if (_LaserCIntensity != value)
                {
                    _LaserCIntensity = value;

                    if (_LaserCIntensity > SettingsManager.ConfigSettings.LaserCMaxIntensity || _LaserCIntensity < 0)
                    {
                        string caption = "Laser C...";
                        string message = string.Format("Laser C intensity range: 0 - {0}", SettingsManager.ConfigSettings.LaserCMaxIntensity);
                        MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                        if (_LaserCIntensity < 0)
                        {
                            _LaserCIntensity = 0;
                        }
                        else if (_LaserCIntensity > SettingsManager.ConfigSettings.LaserCMaxIntensity)
                        {
                            _LaserCIntensity = SettingsManager.ConfigSettings.LaserCMaxIntensity;
                            //LaserCPower = Workspace.This.ApdVM.APDTransfer.LaserIntensityToPower(ScannerDataStruct.APDLaserChannelType.C, _LaserCIntensity);
                        }
                    }

                    RaisePropertyChanged("LaserCIntensity");

                    //if (Workspace.This.ApdVM.APDTransfer.APDTransferIsAlive == true)
                    //{
                    //    Workspace.This.ApdVM.APDTransfer.LaserSetC(_LaserCIntensity);
                    //    //Setting the laser's intensity turns on the laser.
                    //    //Trigger laser on/off button update
                    //    if (_LaserCIntensity > 0)
                    //    {
                    //        _IsLaserCSelected = true;
                    //    }
                    //    else
                    //    {
                    //        _IsLaserCSelected = false;
                    //    }
                    //    RaisePropertyChanged("IsLaserCSelected");
                    //}  
                }
            }
        }

        public int LaserDIntensity
        {
            get { return _LaserDIntensity; }
            set
            {
                if (_LaserDIntensity != value)
                {
                    _LaserDIntensity = value;

                    if (_LaserDIntensity > SettingsManager.ConfigSettings.LaserDMaxIntensity || _LaserDIntensity < 0)
                    {
                        string caption = "Laser D...";
                        string message = string.Format("Laser D intensity range: 0 - {0}", SettingsManager.ConfigSettings.LaserDMaxIntensity);
                        MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                        if (_LaserDIntensity < 0)
                        {
                            _LaserDIntensity = 0;
                        }
                        else if (_LaserDIntensity > SettingsManager.ConfigSettings.LaserDMaxIntensity)
                        {
                            _LaserDIntensity = SettingsManager.ConfigSettings.LaserDMaxIntensity;
                            //LaserDPower = Workspace.This.ApdVM.APDTransfer.LaserIntensityToPower(ScannerDataStruct.APDLaserChannelType.D, _LaserDIntensity);
                        }
                    }

                    RaisePropertyChanged("LaserDIntensity");

                    //if (Workspace.This.ApdVM.APDTransfer.APDTransferIsAlive == true)
                    //{
                    //    Workspace.This.ApdVM.APDTransfer.LaserSetD(_LaserDIntensity);
                    //    //Setting the laser's intensity turns on the laser.
                    //    //Trigger laser on/off button update
                    //    if (_LaserDIntensity > 0)
                    //    {
                    //        _IsLaserDSelected = true;
                    //    }
                    //    else
                    //    {
                    //        _IsLaserDSelected = false;
                    //    }
                    //    RaisePropertyChanged("IsLaserDSelected");
                    //}  
                }
            }
        }

        public double LaserAPower
        {
            get { return _LaserAPower; }
            set
            {
                if (_LaserAPower != value)
                {
                    if ((value < 0) || (value > 0 && value < 5))
                    {
                        string caption = "Laser A...";
                        string message = LaserSetErrorMessage;
                        MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        _LaserAPower = value;
                        //LaserAIntensity = Workspace.This.ApdVM.APDTransfer.LaserPowerToIntensity(ScannerDataStruct.APDLaserChannelType.A, _LaserAPower);
                        RaisePropertyChanged("LaserAPower");
                    }
                }
            }
        }

        public double LaserBPower
        {
            get { return _LaserBPower; }
            set
            {
                if (_LaserBPower != value)
                {
                    if ((value < 0) || (value > 0 && value < 5))
                    {
                        string caption = "Laser B...";
                        string message = LaserSetErrorMessage;
                        MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        _LaserBPower = value;
                        //LaserBIntensity = Workspace.This.ApdVM.APDTransfer.LaserPowerToIntensity(ScannerDataStruct.APDLaserChannelType.B, _LaserBPower);
                        RaisePropertyChanged("LaserBPower");
                    }
                }
            }
        }

        public double LaserCPower
        {
            get { return _LaserCPower; }
            set
            {
                if (_LaserCPower != value)
                {
                    if ((value < 0) || (value > 0 && value < 5))
                    {
                        string caption = "Laser C...";
                        string message = LaserSetErrorMessage;
                        MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        _LaserCPower = value;
                        //LaserCIntensity = Workspace.This.ApdVM.APDTransfer.LaserPowerToIntensity(ScannerDataStruct.APDLaserChannelType.C, _LaserCPower);
                        RaisePropertyChanged("LaserCPower");
                    }
                }
            }
        }

        public double LaserDPower
        {
            get { return _LaserDPower; }
            set
            {
                if (_LaserDPower != value)
                {
                    if ((value < 0) || (value > 0 && value < 5))
                    {
                        string caption = "Laser D...";
                        string message = LaserSetErrorMessage;
                        MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        _LaserDPower = value;
                        //LaserDIntensity = Workspace.This.ApdVM.APDTransfer.LaserPowerToIntensity(ScannerDataStruct.APDLaserChannelType.D, _LaserDPower);
                        RaisePropertyChanged("LaserDPower");
                    }
                }
            }
        }

        public bool IsLaserASelected
        {
            get { return _IsLaserASelected; }
            set
            {
                //if (_IsLaserASelected != value)
                //{
                //}
                _IsLaserASelected = value;
                RaisePropertyChanged("IsLaserASelected");

                if (!_IsLaserASelected)
                {
                    Workspace.This.EthernetController.SetLaserPower(EthernetCommLib.LaserChannels.ChannelA, 0);
                }
                else
                {
                    Workspace.This.EthernetController.SetLaserPower(EthernetCommLib.LaserChannels.ChannelA, LaserAPower);
                }
            }
        }

        public bool IsLaserBSelected
        {
            get { return _IsLaserBSelected; }
            set
            {
                //if (_IsLaserBSelected != value)
                //{
                //}
                _IsLaserBSelected = value;
                RaisePropertyChanged("IsLaserBSelected");

                if (!_IsLaserBSelected)
                {
                    Workspace.This.EthernetController.SetLaserPower(EthernetCommLib.LaserChannels.ChannelB, 0);
                }
                else
                {
                    Workspace.This.EthernetController.SetLaserPower(EthernetCommLib.LaserChannels.ChannelB, LaserBPower);
                }
            }
        }

        public bool IsLaserCSelected
        {
            get { return _IsLaserCSelected; }
            set
            {
                //if (_IsLaserCSelected != value)
                //{
                //}
                _IsLaserCSelected = value;
                RaisePropertyChanged("IsLaserCSelected");

                if (!_IsLaserCSelected)
                {
                    Workspace.This.EthernetController.SetLaserPower(EthernetCommLib.LaserChannels.ChannelC, 0);
                }
                else
                {
                    Workspace.This.EthernetController.SetLaserPower(EthernetCommLib.LaserChannels.ChannelC, LaserCPower);
                }
            }
        }

        public bool IsLaserDSelected
        {
            get { return _IsLaserDSelected; }
            set
            {
                //if (_IsLaserDSelected != value)
                //{
                //}
                _IsLaserDSelected = value;
                RaisePropertyChanged("IsLaserDSelected");

                //if (Workspace.This.ApdVM.APDTransfer.APDTransferIsAlive == true)
                //{
                //    if (!_IsLaserDSelected)
                //    {
                //        Workspace.This.ApdVM.APDTransfer.LaserSetD(0x00);
                //    }
                //    else
                //    {
                //        Workspace.This.ApdVM.APDTransfer.LaserSetD(LaserDIntensity);
                //    }
                //}
            }
        }

        public int HorizontalCalibrationSpeed { get; set; }

        public void TurnOffAllLasers()
        {
            System.Threading.Thread.Sleep(200);
            IsLaserBSelected = false;
            System.Threading.Thread.Sleep(200);
            IsLaserCSelected = false;
            System.Threading.Thread.Sleep(200);
            IsLaserDSelected = false;
        }

        #region CurrentScanTitle
        //HeaderTitle
        public string CurrentScanHeaderTitle
        {
            get
            {
                return _CurrentScanHeaderTitle;
            }
            set
            {
                _CurrentScanHeaderTitle = value;
                RaisePropertyChanged("CurrentScanHeaderTitle");
            }
        }
        //VerticaTitle
        public string CurrentScanVerticaTitle
        {
            get
            {
                return _CurrentScanVerticaTitle;
            }
            set
            {
                _CurrentScanVerticaTitle = value;
                RaisePropertyChanged("CurrentScanVerticaTitle");
            }
        }
        //HorizontalTitle
        public string CurrentScanHorizontalTitle
        {
            get
            {
                return _CurrentScanHorizontalTitle;
            }
            set
            {
                _CurrentScanHorizontalTitle = value;
                RaisePropertyChanged("CurrentScanHorizontalTitle");
            }
        }
        //HeaderTitle
        #endregion

        #endregion

        public void Initialize()
        {
            _ResolutionOptions = SettingsManager.ConfigSettings.ResolutionOptions;
            _XMotorSubdivision = SettingsManager.ConfigSettings.XMotorSubdivision;
            _YMotorSubdivision = SettingsManager.ConfigSettings.YMotorSubdivision;
            _ZMotorSubdivision = SettingsManager.ConfigSettings.ZMotorSubdivision;
            _XMaxValue = SettingsManager.ConfigSettings.XMaxValue;
            _YMaxValue = SettingsManager.ConfigSettings.YMaxValue;
            _ZMaxValue = SettingsManager.ConfigSettings.ZMaxValue;
            RaisePropertyChanged("ResolutionOptions");
            NowChannel = "APD CHA";

            if (_ResolutionOptions != null && _ResolutionOptions.Count > 0)
            {
                _SelectedResolution = _ResolutionOptions[0];    // select the first item
                RaisePropertyChanged("SelectedResolution");
            }

            _QualityOptions = SettingsManager.ConfigSettings.QualityOptions;
            RaisePropertyChanged("QualityOptions");
            if (_QualityOptions != null && _QualityOptions.Count > 0)
            {
                _SelectedQuality = _QualityOptions[0];  // select the first item
                RaisePropertyChanged("SelectedQuality");
            }
            OnSpentTimeInit();//SpentTime
            if (_XMaxValue <= 45833)  //Max range of X-axis,  Distinguish whether the maximum range of the X-axis is 250mm or 500mm
            {
                XHomecoefficient = 310;
                try
                {
                    if (Workspace.This != null)
                    {
                        if (Workspace.This.EthernetController.DeviceProperties.LogicalHomeX == 11)
                        {
                            XHomecoefficient = 311;
                        }
                        else if (Workspace.This.EthernetController.DeviceProperties.LogicalHomeX == 12)
                        {
                            XHomecoefficient = 312;
                        }
                        else if (Workspace.This.EthernetController.DeviceProperties.LogicalHomeX == 13)
                        {
                            XHomecoefficient = 313;
                        }
                        //else if (Workspace.This.EthernetController.DeviceProperties.LogicalHomeX == 14)
                        //{
                        //    _XHomecoefficient = 314;
                        //}
                        //else if (Workspace.This.EthernetController.DeviceProperties.LogicalHomeX == 15)
                        //{
                        //    _XHomecoefficient = 315;
                        //}
                    }
                }
                catch
                {
                }
            }
            else
            {
                XHomecoefficient = 550;
                try
                {
                    if (Workspace.This != null)
                    {
                        if (Workspace.This.EthernetController.DeviceProperties.LogicalHomeX == 11)
                        {
                            XHomecoefficient = 551;
                        }
                        else if (Workspace.This.EthernetController.DeviceProperties.LogicalHomeX == 12)
                        {
                            XHomecoefficient = 552;
                        }
                        else if (Workspace.This.EthernetController.DeviceProperties.LogicalHomeX == 13)
                        {
                            XHomecoefficient = 553;
                        }
                        //else if (Workspace.This.EthernetController.DeviceProperties.LogicalHomeX == 14)
                        //{
                        //    _XHomecoefficient = 554;
                        //}
                        //else if (Workspace.This.EthernetController.DeviceProperties.LogicalHomeX == 15)
                        //{
                        //    _XHomecoefficient = 555;
                        //}
                    }
                }
                catch
                {
                }
            }
            ScanX0 = (int)Workspace.This.EthernetController.DeviceProperties.LogicalHomeX;
            ScanY0 = (int)Workspace.This.EthernetController.DeviceProperties.LogicalHomeY;
            ScanZ0 = (int)Workspace.This.EthernetController.DeviceProperties.ZFocusPosition;
            double xmax = ((int)Workspace.This.EthernetController.DeviceProperties.OpticalLR2Distance);
            ScanDeltaX = 100;
            double Ymax = ((double)_YMaxValue / (double)_YMotorSubdivision);
            ScanDeltaY = 100;
            _FirstDx = true;
        }

        /// <summary>
        /// Initialize the lasers intensity without setting the lasers intensity
        /// </summary>
        /// 
        public void InitLasersIntensity()
        {
           /*Workspace.This.IVVM */
            _LaserAIntensity = SettingsManager.ConfigSettings.LaserAIntensity;
            RaisePropertyChanged("LaserAIntensity");                                //Trigger UI update
            _LaserBIntensity = SettingsManager.ConfigSettings.LaserBIntensity;
            RaisePropertyChanged("LaserBIntensity");
            _LaserCIntensity = SettingsManager.ConfigSettings.LaserCIntensity;
            RaisePropertyChanged("LaserCIntensity");
            _LaserDIntensity = SettingsManager.ConfigSettings.LaserDIntensity;
            RaisePropertyChanged("LaserDIntensity");
        }

        #region DynamicWindown
        void ScanDynamicScopeSettins()
        {
            while (true)
            {
                Thread.Sleep(200);
                if (ScanDynamicScopeString != "" && ScanDynamicScopeStringType != "")
                {
                    Workspace.This.Owner.Dispatcher.BeginInvoke((Action)delegate
                    {
                        Window window = new Window();
                        MessageBox.Show(window, ScanDynamicScopeString, ScanDynamicScopeStringType);
                    });
                    Thread.Sleep(200);
                    ScanDynamicScopeString = "";
                    ScanDynamicScopeStringType = "";
                }
            }
        }
        #endregion

        #region UseCurrentPosCommand

        public ICommand UseCurrentPosCommand
        {
            get
            {
                if (_UseCurrentPosCommand == null)
                {
                    _UseCurrentPosCommand = new RelayCommand(ExecuteUseCurrentPosCommand, CanExecuteUseCurrentPosCommand);
                }

                return _UseCurrentPosCommand;
            }
        }
        public void ExecuteUseCurrentPosCommand(object parameter)
        {
            //TODO: implement the command
            ScanX0 = Workspace.This.MotorVM.CurrentXPos;
            ScanY0 = Workspace.This.MotorVM.CurrentYPos;
            //if (Workspace.This.MotorVM.GalilMotor != null && Workspace.This.MotorVM.GalilMotor.IsAlive)
            //{
            //    //ScanZ0 = (int)Workspace.This.MotorVM.GalilMotor.ZCurrentP/_ZMotorSubdivision;
            //}
        }
        public bool CanExecuteUseCurrentPosCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region ScanCommand

        public ICommand ScanCommand
        {
            get
            {
                if (_ScanCommand == null)
                {
                    _ScanCommand = new RelayCommand(ExecuteScanCommand, CanExecuteScanCommand);
                }

                return _ScanCommand;
            }
        }
        public void ExecuteScanCommand(object parameter)
        {
            //选择ROI扫描或者水平扫描，或者Z-Stack扫描
            //Multi-area scanning and Z-stacking scanning and horizontal scanning
            //Calculate the scanning range of X and Y, and if it exceeds the range, do not proceed further
            //计算X和Y的扫描范围，如果超出范围就不往下执行了
            bool IsScanScopelimitations = false;
            ScanScopelimitations(ref IsScanScopelimitations);
            if (!IsScanScopelimitations)
            {
                return;
            }
            if (ScanWorkCount > 0 || Workspace.This.ZAutomaticallyFocalVM.SelectedFocus == "Z-Stacking"&& ScanDeltaX > 0 && ScanDeltaY > 0)
            {
                _StackNum = 0;
                _RangeNum = 0;
                _IsScanTest = true;
                Workspace.This.LGifImageTitle.Clear();
                Workspace.This.R1GifImageTitle.Clear();
                Workspace.This.R2GifImageTitle.Clear();
                OnScanEnbledRegionReceived(false);
                Workspace.This.ZAutomaticallyFocalVM.IsWorkEnabled = false;
                Workspace.This.ZAutomaticallyFocalVM.IsFoucsEnabled = false;
                ScanCommondList(_StackNum, _RangeNum);
            }
            else
            {
                _StackNum = 0;
                _RangeNum = 0;
                _IsScanTest = true;
                Workspace.This.LGifImageTitle.Clear();
                Workspace.This.R1GifImageTitle.Clear();
                Workspace.This.R2GifImageTitle.Clear();
                OnScanEnbledRegionReceived(false);
                Workspace.This.ZAutomaticallyFocalVM.IsWorkEnabled = false;
                Workspace.This.ZAutomaticallyFocalVM.IsFoucsEnabled = false;
                ScanCommond();

            }
        }
    
        private void ScanCommondList(int _Stacking,int _RangeCount)
        {
            bool loaded = false;
            OnScanWorkReceived(_RangeCount + 1, ref loaded);
            if (Workspace.This.ZAutomaticallyFocalVM.SelectedFocus == "Z-Stacking" && ScanWorkCount <= 0)//选择了Z-stack扫描  Testing only Z-Stacking
            {
                if (_IsScanTest == true)
                {
                    //Z-scan扫描集合为空
                    //Z-scan the scan set is empty
                    if (Workspace.This.ZAutomaticallyFocalVM._FocusOptionsList.Count == 0)
                    {
                        ScanDynamicScopeString = String.Format("The Z-Stack collection is empty，TopImage and DeltaFocus are equal to 0");
                        ScanDynamicScopeStringType = "Error";
                        WorkDone();
                    }
                    //Z-scan没有扫描完
                    // The z-scan is not complete
                    if (_ScanningProcess == null && _Stacking < Workspace.This.ZAutomaticallyFocalVM._FocusOptionsList.Count)
                    {
                        double _CurrenFocus = Workspace.This.ZAutomaticallyFocalVM._FocusOptionsList[_Stacking].Value;
                        ScanZ0 = _CurrenFocus;
                        Workspace.This.ZAutomaticallyFocalVM.Ofimages = _Stacking + 1;
                        ScanCommond();
                    }
                }
            }
            else if (Workspace.This.ZAutomaticallyFocalVM.SelectedFocus == "None" && ScanWorkCount > 0)//选择了ROI扫描 Select only multiple regions
            {
                if (_IsScanTest == true)
                {
                    if (_ScanningProcess == null && _RangeCount < ScanWorkCount)
                    {
                        try
                        {
                            if (loaded)
                            {
                                ScanCommond();
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("An error occurred in selecting the region" + ex.Message);
                        }
                    }
                }
            }
            else if (Workspace.This.ZAutomaticallyFocalVM.SelectedFocus == "Z-Stacking" && ScanWorkCount > 0)//选择了ROI和Z-Stack扫描  //Multiregion is selected and Z-Stacking is selected 
            {
                if (_IsScanTest == true)
                {
                    if (_ScanningProcess == null)
                    {
                        //bool loaded = false;
                        //OnScanWorkReceived(_RangeCount + 1, ref loaded);
                        if (loaded)
                        {
                            //没有选择Z-Scan
                            //Z-scan is not selected
                            if (Workspace.This.ZAutomaticallyFocalVM._FocusOptionsList.Count == 0)
                            {
                                if (_StackNum >= Workspace.This.ZAutomaticallyFocalVM._FocusOptionsList.Count && _RangeNum < ScanWorkCount)
                                {
                                    _StackNum = 0;
                                    _RangeNum++;
                                    //已经扫描到最后一个区域
                                    //The last area has been scanned
                                    if (_RangeNum >= ScanWorkCount)
                                    {
                                        //扫描终止
                                        //Scan is terminated
                                        WorkDone();
                                        Workspace.This.SelectedTabIndex = (int)ApplicationTabType.Gallery;   // Switch to gallery tab

                                    }
                                    //Z-scan扫描并且TopImage和DeltaFocus都是0时开始弹框提醒
                                    ////Z-scan scans and the TopImage and DeltaFocus are both 0, the pop-up reminder starts
                                    ROIWarning += string.Format("ROI{0}, ",_RangeNum);
                                    ScanDynamicScopeString = String.Format("{0}...TopImage and DeltaFocus are equal to 0", ROIWarning);
                                    ScanDynamicScopeStringType = "Warning";
                                    ScanCommondList(_StackNum, _RangeNum);
                                }
                            }
                            //Z-scan扫描
                            //Z - scan to scan
                            if (_Stacking < Workspace.This.ZAutomaticallyFocalVM._FocusOptionsList.Count)
                            {
                                double _CurrenFocus = Workspace.This.ZAutomaticallyFocalVM._FocusOptionsList[_Stacking].Value;
                                ScanZ0 = _CurrenFocus;
                                Workspace.This.ZAutomaticallyFocalVM.Ofimages = _Stacking + 1;
                                ScanCommond();
                            }
                        }
                    }

                }
            }
            else if (Workspace.This.ZAutomaticallyFocalVM.SelectedFocus == "None" && ScanWorkCount <= 0)
            {
                ScanCommond();
            }
        }

        private void ScanCommond()
        {
            if (!Workspace.This.EthernetController.IsConnected)
            {
                string caption = "Start scanning...";
                string message = "ERROR: The scanner is not connected!";
                Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }
            if (Workspace.This.ScannerVM.HWversion == Workspace.This.DefaultHWversion)//（FW Version 1.1.0.0）
            {
                //关闭RGB灯光，Turn off RGB lights
                if (Workspace.This.IsRGBLightSelected)
                {
                    Workspace.This.IsRGBLightSelected = false;
                }
            }
            CurrentScanDirection();
            Width = (int)Math.Ceiling(ScanDeltaX * 1000.0 / SelectedResolution.Value);
            Height = (int)(ScanDeltaY * 1000.0 / SelectedResolution.Value);
            Time = (int)(_Height * SelectedQuality.Value / 2.0);
            ScanParameterStruct scanParameter = new ScanParameterStruct();  //Set scan parameter
            scanParameter.Height = Height;
            long? SysMemory = GetMemoryAvailable();
            long? ImageMemory = GetImageMemory(Width, Height);
            if (ImageMemory > SysMemory)
            {
                string message = "Out of memory";
                Window window = new Window();
                MessageBox.Show(window, message);
                return;
            }
            if (SettingsManager.ConfigSettings.AllModuleProcessing)
            {
                Time *= 2;
                scanParameter.Height *= 2;
            }
            RemainingTime = Time;
            //Set Scan parameter for Scanning
            Log.Info(this, "Set scan parameter…");
            scanParameter.Width = Width;
            scanParameter.ScanX0 = _ScanX0;
            scanParameter.ScanDeltaX = (int)_ScanDeltaX;
            //启用了Y轴补偿
            //Y-axis compensation is enabled
            if (ycompenSationBitAt && (int)(YCompenOffset * 1000 / SelectedResolution.Value) >= 1)
            {
                scanParameter.ScanY0 = _ScanY0 - (int)(YCompenOffset * _YMotorSubdivision);
                scanParameter.ScanDeltaY = (int)_ScanDeltaY + (int)(YCompenOffset * _YMotorSubdivision);
            }
            else
            {
                scanParameter.ScanY0 = _ScanY0;
                scanParameter.ScanDeltaY = (int)_ScanDeltaY;
            }
            scanParameter.ScanZ0 = _ScanZ0;
            scanParameter.ScanDeltaZ = (int)_ScanDeltaZ;
            scanParameter.Res = SelectedResolution.Value;
            scanParameter.Quality = SelectedQuality.Value;
            scanParameter.DataRate = DataRate;
            scanParameter.LineCounts = LineCounts;
            scanParameter.Time = Time;
            scanParameter.XMotorSubdivision = _XMotorSubdivision;
            scanParameter.YMotorSubdivision = _YMotorSubdivision;
            scanParameter.ZMotorSubdivision = _ZMotorSubdivision;
            scanParameter.XMotorSpeed = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[0].Speed * SettingsManager.ConfigSettings.XMotorSubdivision);
            scanParameter.XMotionAccVal = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[0].Accel * SettingsManager.ConfigSettings.XMotorSubdivision);
            scanParameter.XMotionDccVal = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[0].Dccel * SettingsManager.ConfigSettings.XMotorSubdivision);
            scanParameter.YMotorSpeed = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[1].Speed * SettingsManager.ConfigSettings.YMotorSubdivision);
            scanParameter.YMotionAccVal = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[1].Accel * SettingsManager.ConfigSettings.YMotorSubdivision);
            scanParameter.YMotionDccVal = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[1].Dccel * SettingsManager.ConfigSettings.YMotorSubdivision);
            scanParameter.ZMotorSpeed = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[2].Speed * SettingsManager.ConfigSettings.ZMotorSubdivision);
            scanParameter.ZMotionAccVal = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[2].Accel * SettingsManager.ConfigSettings.ZMotorSubdivision);
            scanParameter.ZMotionDccVal = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[2].Dccel * SettingsManager.ConfigSettings.ZMotorSubdivision);
            scanParameter.IsNewFirmwire = Workspace.This.MotorVM.IsNewFirmware;
            scanParameter.XmotionTurnAroundDelay = SettingsManager.ConfigSettings.XMotionTurnDelay;
            scanParameter.XMotionExtraMoveLength = SettingsManager.ConfigSettings.XMotionExtraMoveLength;
            scanParameter.DynamicBits = SelectedDynamicBits;
            //Grating ruler pulse
            scanParameter.XEncoderSubdivision = Workspace.This.NewParameterVM.XEncoderSubdivision;
            //scanParameter.XEncoderSubdivision = SettingsManager.ConfigSettings.XEncoderSubdivision;
            scanParameter.HorizontalCalibrationSpeed = HorizontalCalibrationSpeed;
            //是否启用动态为补偿   Whether to enable dynamic as compensation
            scanParameter.DynamicBitsAt = SettingsManager.ConfigSettings.ScanDynamicBitAt;
            scanParameter.IsUnidirectionalScan = SettingsManager.ConfigSettings.AllModuleProcessing;
            IntPtr[] _Channel = new IntPtr[3];
            if (_ScanDeltaX > 0 && _ScanDeltaY > 0)
            {
                //（因为在设计中将R1透镜的位置和R2透镜的位置更换了，然后参数名没有更改成对应的通道，所以下面参数名R1其实是R2,R2其实是R1）....
                // (in the design, the position of R1 lens and R2 lens is changed, and the parameter name is not changed to the corresponding channel, so the following parameter name R1 is actually R2, R2 is actually R1)....
                //L透镜到R2透镜之间的距离（mm），毫米为单位
                //The distance between L lens and R2 lens (mm), in millimeters
                int OpticalL_R1Distance = (int)Workspace.This.NewParameterVM.OpticalL_R1Distance;
                //根据当前选择的分辨率计算L透镜到R2透镜之间的实际像素宽度
                //// Calculate the actual pixel width between L lens and R2 lens based on the currently selected resolution
                int OffsetWidth = (int)(OpticalL_R1Distance * 1000.0 / SelectedResolution.Value);
                //扫描图像宽度加上补偿透镜之间距离的宽度（像素）
                // The width of the scanned image plus the width of the distance between the compensating lenses (pixels)
                _CurrentRangeWidth = Width + OffsetWidth;
                _CurrentRangeHeight = Height;
                scanParameter.Width = _CurrentRangeWidth;
                //将L到R2透镜之间的距离（mm）累加到ScanDeltaX上
                //Add the distance (mm) between L and R2 lens to ScanDeltaX
                int offsetDeltaXPulse = OpticalL_R1Distance;
                //if (Workspace.This.NewParameterVM.OpticalL_R1Distance > 0 && SelectedResolution.Value == 150)
                //{
                //    offsetDeltaXPulse = (int)Workspace.This.NewParameterVM.OpticalL_R1Distance - 1;
                //}
                //else
                //{
                //    offsetDeltaXPulse = (int)Workspace.This.NewParameterVM.OpticalL_R1Distance;
                //}
                offsetDeltaXPulse = (int)(offsetDeltaXPulse * _XMotorSubdivision);
                scanParameter.ScanDeltaX += (int)offsetDeltaXPulse;
                try
                {
                    _ChannelAImage = new WriteableBitmap(scanParameter.Width, Height, 90, 90, System.Windows.Media.PixelFormats.Gray16, null);
                    _ChannelBImage = new WriteableBitmap(scanParameter.Width, Height, 90, 90, System.Windows.Media.PixelFormats.Gray16, null);
                    _ChannelCImage = new WriteableBitmap(scanParameter.Width, Height, 90, 90, System.Windows.Media.PixelFormats.Gray16, null);
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
                _Channel[0] = _ChannelAImage.BackBuffer;
                _Channel[1] = _ChannelBImage.BackBuffer;
                _Channel[2] = _ChannelCImage.BackBuffer;
                dynamSwitch = true;
                scanParameter.BackBufferStride = _ChannelAImage.BackBufferStride;
            }
            _ScanningProcess = new ScanProcessing(Workspace.This.EthernetController, Workspace.This.MotorVM.MotionController, scanParameter, _Channel);
            _ScanningProcess.Completed += _ImageScanCommand_Completed;
            _ScanningProcess.OnScanDataReceived += _ScanningProcess_OnScanDataReceived;
            _ScanningProcess.OnSpentTimeScanDataReceived += _ScanningProcess_OnSpentTimeScanDataReceived;
            _ScanningProcess.ApplicationDataPath = SettingsManager.ApplicationDataPath;
            OnSpentTimeStart();//SpentTime 
            _ScanningProcess.Start();
            Workspace.This.MotorIsAlive = false;
            Workspace.This.IsScanning = true;
            //Optical module button disabled
            Workspace.This.DisconnectDeviceEnable = false;  
        }
        private void _ScanningProcess_OnSpentTimeScanDataReceived()
        {
            _packcountTime = 0;
            _IsOnSpentTimeScanDataReceivedCheck = true;
        }
        /// <summary>
        /// X轴水平扫描和扫图倒计时
        /// X axis horizontal scan and scan countdown
        /// </summary>
        Thread TdOnXRemaining = null;
        private void _ScanningProcess_OnScanDataReceived(string dataName)
        {
            switch (dataName)
            {
                case "RemainingTime":
                    RemainingTime = _ScanningProcess.RemainingTime;
                    break;
                case "OnXRemainingTime":
                    TdOnXRemaining = new Thread(OnXRemainingTime);
                    TdOnXRemaining.IsBackground = true;
                    TdOnXRemaining.Start();
                    break;
            }
        }
        /// <summary>
        ///X水平模式扫描倒计时
        /// X moudel horizontal scan countdown
        /// </summary>
        void OnXRemainingTime()
        {
            while (true)
            {
                Thread.Sleep(1000);
                RemainingTime = RemainingTime - 1;
                if (RemainingTime == 0)
                {
                    return;
                }
            }

        }
        private void _ImageScanCommand_CommandStatus(object sender, string status)
        {
            Workspace.This.Owner.Dispatcher.BeginInvoke((Action)delegate
            {
                Workspace.This.CapturingTopStatusText = status;
            });
        }

        private void _ImageScanCommand_Completed(CommandLib.ThreadBase sender, CommandLib.ThreadBase.ThreadExitStat exitState)
        {
            Workspace.This.Owner.Dispatcher.BeginInvoke((Action)delegate
            {
                Workspace.This.CaptureCountdownTimer.Stop();
                OnSpentTimeStop(); //SpentTimeStop
                if (TdOnXRemaining != null)
                    TdOnXRemaining.Abort();//X horizontal scan countdown stop
                dynamSwitch = false;
                Workspace.This.IsScanning = false;
                Workspace.This.IsPreparing = false;
                //Enable Motor control
                Workspace.This.MotorIsAlive = true;
                ScanProcessing scannedThread = (sender as ScanProcessing);

                if (exitState == ThreadBase.ThreadExitStat.None)
                {
                    // Capture successful

                    scannedThread = (sender as ScanProcessing);
                    //Three ImageInfos need to be applied for. Previously, applying for only one ImageInfo would result in the values of other channels being cloned as well. Only ENU GUI
                    //需要申请三个ImageInfo，之前只申请一个ImageInfo时会导致其它通道的值也被克隆了，只需要更改ENU GUI
                    ImageInfo imageInfoL = null;
                    ImageInfo imageInfoR1 = null;
                    ImageInfo imageInfoR2 = null;
                    if (scannedThread.ImageInfo != null)
                    {
                        imageInfoL = (ImageInfo)scannedThread.ImageInfo.Clone();
                        imageInfoR1 = (ImageInfo)scannedThread.ImageInfo.Clone();
                        imageInfoR2 = (ImageInfo)scannedThread.ImageInfo.Clone();
                    }
                    if (imageInfoL != null)
                    {
                        Log.Info(this, "Set imageInfo…");
                        imageInfoL.LaserAIntensity = (Workspace.This.IVVM.IsLaserL1Selected) ? LaserAIntensity : 0;
                        imageInfoL.LaserBIntensity = (Workspace.This.IVVM.IsLaserR1Selected) ? LaserBIntensity : 0;
                        imageInfoL.LaserCIntensity = (Workspace.This.IVVM.IsLaserR2Selected) ? LaserCIntensity : 0;
                        //imageInfo.LaserDIntensity = (IsLaserDSelected) ? LaserDIntensity : 0;
                        if (Workspace.This.IVVM.SensorML1 == IvSensorType.APD)//C
                        {
                            imageInfoL.ApdCGain = (Workspace.This.IVVM.IsLaserL1Selected) ? Workspace.This.IVVM.SelectedGainComModuleL1.Value : 0;
                        }
                        else
                        {
                            imageInfoL.ApdCGain = (Workspace.This.IVVM.IsLaserL1Selected) ? Workspace.This.IVVM.GainTxtModuleL1 : 0;
                        }
                        if (Workspace.This.IVVM.SensorMR1 == IvSensorType.APD)//A
                        {
                            imageInfoL.ApdAGain = (Workspace.This.IVVM.IsLaserR1Selected) ? Workspace.This.IVVM.SelectedGainComModuleR1.Value : 0;
                        }
                        else
                        {
                            imageInfoL.ApdAGain = (Workspace.This.IVVM.IsLaserR1Selected) ? Workspace.This.IVVM.GainTxtModuleR1 : 0;
                        }
                        if (Workspace.This.IVVM.SensorMR2 == IvSensorType.APD)//B
                        {
                            imageInfoL.ApdBGain = (Workspace.This.IVVM.IsLaserR2Selected) ? Workspace.This.IVVM.SelectedGainComModuleR2.Value : 0;
                        }
                        else
                        {
                            imageInfoL.ApdBGain = (Workspace.This.IVVM.IsLaserR2Selected) ? Workspace.This.IVVM.GainTxtModuleR2 : 0;
                        }
                        imageInfoL.ScanZ0 = (int)Workspace.This.MotorVM.CurrentZPos;
                        imageInfoL.SoftwareVersion = Workspace.This.ProductVersion;
                        imageInfoL.FpgaFirmware = Workspace.This.ScannerVM.FPGAVersion;
                    }


                    if (imageInfoR1 != null)
                    {
                        Log.Info(this, "Set imageInfo…");
                        imageInfoR1.LaserAIntensity = (Workspace.This.IVVM.IsLaserL1Selected) ? LaserAIntensity : 0;
                        imageInfoR1.LaserBIntensity = (Workspace.This.IVVM.IsLaserR1Selected) ? LaserBIntensity : 0;
                        imageInfoR1.LaserCIntensity = (Workspace.This.IVVM.IsLaserR2Selected) ? LaserCIntensity : 0;
                        //imageInfo.LaserDIntensity = (IsLaserDSelected) ? LaserDIntensity : 0;
                        if (Workspace.This.IVVM.SensorML1 == IvSensorType.APD)//C
                        {
                            imageInfoR1.ApdCGain = (Workspace.This.IVVM.IsLaserL1Selected) ? Workspace.This.IVVM.SelectedGainComModuleL1.Value : 0;
                        }
                        else
                        {
                            imageInfoR1.ApdCGain = (Workspace.This.IVVM.IsLaserL1Selected) ? Workspace.This.IVVM.GainTxtModuleL1 : 0;
                        }
                        if (Workspace.This.IVVM.SensorMR1 == IvSensorType.APD)//A
                        {
                            imageInfoR1.ApdAGain = (Workspace.This.IVVM.IsLaserR1Selected) ? Workspace.This.IVVM.SelectedGainComModuleR1.Value : 0;
                        }
                        else
                        {
                            imageInfoR1.ApdAGain = (Workspace.This.IVVM.IsLaserR1Selected) ? Workspace.This.IVVM.GainTxtModuleR1 : 0;
                        }
                        if (Workspace.This.IVVM.SensorMR2 == IvSensorType.APD)//B
                        {
                            imageInfoR1.ApdBGain = (Workspace.This.IVVM.IsLaserR2Selected) ? Workspace.This.IVVM.SelectedGainComModuleR2.Value : 0;
                        }
                        else
                        {
                            imageInfoR1.ApdBGain = (Workspace.This.IVVM.IsLaserR2Selected) ? Workspace.This.IVVM.GainTxtModuleR2 : 0;
                        }
                        imageInfoR1.ScanZ0 = (int)Workspace.This.MotorVM.CurrentZPos;
                        imageInfoR1.SoftwareVersion = Workspace.This.ProductVersion;
                        imageInfoR1.FpgaFirmware = Workspace.This.ScannerVM.FPGAVersion;
                    }


                    if (imageInfoR2 != null)
                    {
                        Log.Info(this, "Set imageInfo…");
                        imageInfoR2.LaserAIntensity = (Workspace.This.IVVM.IsLaserL1Selected) ? LaserAIntensity : 0;
                        imageInfoR2.LaserBIntensity = (Workspace.This.IVVM.IsLaserR1Selected) ? LaserBIntensity : 0;
                        imageInfoR2.LaserCIntensity = (Workspace.This.IVVM.IsLaserR2Selected) ? LaserCIntensity : 0;
                        //imageInfo.LaserDIntensity = (IsLaserDSelected) ? LaserDIntensity : 0;
                        if (Workspace.This.IVVM.SensorML1 == IvSensorType.APD)//C
                        {
                            imageInfoR2.ApdCGain = (Workspace.This.IVVM.IsLaserL1Selected) ? Workspace.This.IVVM.SelectedGainComModuleL1.Value : 0;
                        }
                        else
                        {
                            imageInfoR2.ApdCGain = (Workspace.This.IVVM.IsLaserL1Selected) ? Workspace.This.IVVM.GainTxtModuleL1 : 0;
                        }
                        if (Workspace.This.IVVM.SensorMR1 == IvSensorType.APD)//A
                        {
                            imageInfoR2.ApdAGain = (Workspace.This.IVVM.IsLaserR1Selected) ? Workspace.This.IVVM.SelectedGainComModuleR1.Value : 0;
                        }
                        else
                        {
                            imageInfoR2.ApdAGain = (Workspace.This.IVVM.IsLaserR1Selected) ? Workspace.This.IVVM.GainTxtModuleR1 : 0;
                        }
                        if (Workspace.This.IVVM.SensorMR2 == IvSensorType.APD)//B
                        {
                            imageInfoR2.ApdBGain = (Workspace.This.IVVM.IsLaserR2Selected) ? Workspace.This.IVVM.SelectedGainComModuleR2.Value : 0;
                        }
                        else
                        {
                            imageInfoR2.ApdBGain = (Workspace.This.IVVM.IsLaserR2Selected) ? Workspace.This.IVVM.GainTxtModuleR2 : 0;
                        }
                        imageInfoR2.ScanZ0 = (int)Workspace.This.MotorVM.CurrentZPos;
                        imageInfoR2.SoftwareVersion = Workspace.This.ProductVersion;
                        imageInfoR2.FpgaFirmware = Workspace.This.ScannerVM.FPGAVersion;
                    }

                    if (scannedThread.ScanType == ScanTypes.Horizontal)
                    {
                        try
                        {
                            _ChannelAImage.Freeze();
                            _ChannelBImage.Freeze();
                            //Phosphor Laser Module software filter
                            if (SettingsManager.ConfigSettings.ENGGUI_PhosphorModuleProcessing && !SettingsManager.ConfigSettings.AllModuleProcessing)
                            {
                                _ChannelCImage = OpenCvSharpHelper.Filter2D(_ChannelCImage);
                            }
                            _ChannelCImage.Freeze();
                            int _LGain = 0;
                            int _R1Gain = 0;
                            int _R2Gain = 0;
                            if (Workspace.This.IVVM.SensorML1 == IvSensorType.APD)
                            {
                                _LGain = Workspace.This.IVVM.SelectedGainComModuleL1.Value;//Apd
                            }
                            else
                            {
                                _LGain = Workspace.This.IVVM.GainTxtModuleL1;//PMT
                            }
                            if (Workspace.This.IVVM.SensorMR1 == IvSensorType.APD)
                            {
                                _R1Gain = Workspace.This.IVVM.SelectedGainComModuleR1.Value;
                            }
                            else
                            {
                                _R1Gain = Workspace.This.IVVM.GainTxtModuleR1;
                            }
                            if (Workspace.This.IVVM.SensorMR2 == IvSensorType.APD)
                            {
                                _R2Gain = Workspace.This.IVVM.SelectedGainComModuleR2.Value;
                            }
                            else
                            {
                                _R2Gain = Workspace.This.IVVM.GainTxtModuleR2;
                            }
                            System.DateTime currentTime = DateTime.Now;//获取当前系统时间
                            int month = currentTime.Month;
                            int day = currentTime.Day;
                            int hour = currentTime.Hour;
                            int minute = currentTime.Minute;
                            string newDay = "";
                            if (day.ToString().Length == 1)
                            {
                                newDay = "0" + day.ToString();
                            }
                            else
                            {
                                newDay = day.ToString();
                            }
                            string timeNow = month.ToString() + newDay + hour.ToString() + minute.ToString();
                            //Add image to Gallery
                            if (Workspace.This.IVVM.WL1 > 0)
                            {
                                //Record which bin the current image is in, so that even if merged into RGB, we can still distinguish which bin the image belongs to
                                //记录当前图像是哪个仓位，这样即使合并成RGB我们也能分别出图像来是哪个仓位
                                imageInfoL.ChannelRemark = "L";
                                double APower = Workspace.This.IVVM.LaserAPower;
                                if (!Workspace.This.IVVM.IsLaserL1Selected)
                                {
                                    APower = 0;
                                }
                                string LimageSet = string.Format("{0}_{1}_{2}mw_Gain{3}_PGA{4}_Quality{5}_{6}um", Workspace.This.IVVM.WL1 + Workspace.This.IVVM.WL1Sign, "L", APower,
                                       _LGain, Workspace.This.IVVM.SelectedMModuleL1.DisplayName, Workspace.This.ScannerVM.SelectedQuality.Value, Workspace.This.ScannerVM.SelectedResolution.Value);

                                if (Workspace.This.IVVM.WR1 > 0)
                                {
                                    double R1Power = Workspace.This.IVVM.LaserBPower;
                                    if (!Workspace.This.IVVM.IsLaserR1Selected)
                                    {
                                        R1Power = 0;
                                    }
                                    LimageSet += string.Format("_{0}-{1}-{2}mW", "R1", Workspace.This.IVVM.WR1 + Workspace.This.IVVM.WR1Sign, R1Power);
                                }

                                if (Workspace.This.IVVM.WR2 > 0)
                                {
                                    double R2Power = Workspace.This.IVVM.LaserCPower;
                                    if (!Workspace.This.IVVM.IsLaserR2Selected)
                                    {
                                        R2Power = 0;
                                    }
                                    LimageSet += string.Format("_{0}-{1}-{2}mW", "R2", Workspace.This.IVVM.WR2 + Workspace.This.IVVM.WR2Sign, R2Power);
                                }
                                if (_RangeNum >= 0 && ScanWorkCount > 0)
                                {
                                    int tempRangeNum = _RangeNum + 1;
                                    LimageSet += string.Format("_ROI{0}", tempRangeNum);
                                }
                                if (Workspace.This.ZAutomaticallyFocalVM.SelectedFocus != "None" && Workspace.This.ZAutomaticallyFocalVM.Ofimages <= Workspace.This.ZAutomaticallyFocalVM._FocusOptionsList.Count)//
                                {
                                    double focus = Workspace.This.EthernetController.DeviceProperties.ZFocusPosition +
                                        (Workspace.This.ZAutomaticallyFocalVM._FocusOptionsList[_StackNum].Value - Workspace.This.EthernetController.DeviceProperties.ZFocusPosition);
                                    LimageSet += string.Format("_Focus{0}", Math.Round(focus, 2));
                                }
                                if (Workspace.This.IVVM.IsCaptrueL1Selected)
                                {
                                    Workspace.This.NewDocument(_ChannelCImage, imageInfoL, LimageSet + "_Date" + timeNow, false, true, true);
                                }
                            }
                            if (Workspace.This.IVVM.WR1 > 0)
                            {
                                //Record which bin the current image is in, so that even if merged into RGB, we can still distinguish which bin the image belongs to
                                //记录当前图像是哪个仓位，这样即使合并成RGB我们也能分别出图像来是哪个仓位
                                imageInfoR1.ChannelRemark = "R1";
                                double BPower = Workspace.This.IVVM.LaserBPower;
                                if (!Workspace.This.IVVM.IsLaserR1Selected)
                                {
                                    BPower = 0;
                                }

                                string R1imageSet = string.Format("{0}_{1}_{2}mw_Gain{3}_PGA{4}_Quality{5}_{6}um", Workspace.This.IVVM.WR1 + Workspace.This.IVVM.WR1Sign, "R1", BPower,
                                  _R1Gain, Workspace.This.IVVM.SelectedMModuleR1.DisplayName, Workspace.This.ScannerVM.SelectedQuality.Value, Workspace.This.ScannerVM.SelectedResolution.Value);

                                if (Workspace.This.IVVM.WL1 > 0)
                                {
                                    double LPower = Workspace.This.IVVM.LaserAPower;
                                    if (!Workspace.This.IVVM.IsLaserL1Selected)
                                    {
                                        LPower = 0;
                                    }
                                    R1imageSet += string.Format("_{0}-{1}-{2}mW", "L", Workspace.This.IVVM.WL1 + Workspace.This.IVVM.WL1Sign, LPower);
                                }

                                if (Workspace.This.IVVM.WR2 > 0)
                                {
                                    double R2Power = Workspace.This.IVVM.LaserCPower;
                                    if (!Workspace.This.IVVM.IsLaserR2Selected)
                                    {
                                        R2Power = 0;
                                    }
                                    R1imageSet += string.Format("_{0}-{1}-{2}mW", "R2", Workspace.This.IVVM.WR2 + Workspace.This.IVVM.WR2Sign, R2Power);
                                }
                                if (_RangeNum >= 0 && ScanWorkCount > 0)
                                {
                                    int tempRangeNum = _RangeNum + 1;
                                    R1imageSet += string.Format("_ROI{0}", tempRangeNum);
                                }
                                if (Workspace.This.ZAutomaticallyFocalVM.SelectedFocus != "None" && Workspace.This.ZAutomaticallyFocalVM.Ofimages <= Workspace.This.ZAutomaticallyFocalVM._FocusOptionsList.Count)//
                                {
                                    double focus = Workspace.This.EthernetController.DeviceProperties.ZFocusPosition +
                                        (Workspace.This.ZAutomaticallyFocalVM._FocusOptionsList[_StackNum].Value - Workspace.This.EthernetController.DeviceProperties.ZFocusPosition);
                                    R1imageSet += string.Format("_Focus{0}", Math.Round(focus, 2));
                                }
                                if (Workspace.This.IVVM.IsCaptrueR1Selected)
                                {
                                    Workspace.This.NewDocument(_ChannelAImage, imageInfoR1, R1imageSet + "_Date" + timeNow, false, true, true);
                                }

                            }
                            if (Workspace.This.IVVM.WR2 > 0)
                            {
                                //Record which bin the current image is in, so that even if merged into RGB, we can still distinguish which bin the image belongs to
                                //记录当前图像是哪个仓位，这样即使合并成RGB我们也能分别出图像来是哪个仓位
                                imageInfoR2.ChannelRemark = "R2";
                                double CPower = Workspace.This.IVVM.LaserCPower;
                                if (!Workspace.This.IVVM.IsLaserR2Selected)
                                {
                                    CPower = 0;
                                }
                                string R2imageSet = string.Format("{0}_{1}_{2}mw_Gain{3}_PGA{4}_Quality{5}_{6}um", Workspace.This.IVVM.WR2 + Workspace.This.IVVM.WR2Sign, "R2", CPower,
                                _R2Gain, Workspace.This.IVVM.SelectedMModuleR2.DisplayName, Workspace.This.ScannerVM.SelectedQuality.Value, Workspace.This.ScannerVM.SelectedResolution.Value);

                                if (Workspace.This.IVVM.WR1 > 0)
                                {
                                    double R1Power = Workspace.This.IVVM.LaserBPower;
                                    if (!Workspace.This.IVVM.IsLaserR1Selected)
                                    {
                                        R1Power = 0;
                                    }
                                    R2imageSet += string.Format("_{0}-{1}-{2}mW", "R1", Workspace.This.IVVM.WR1 + Workspace.This.IVVM.WR1Sign, R1Power);
                                }

                                if (Workspace.This.IVVM.WL1 > 0)
                                {
                                    double LPower = Workspace.This.IVVM.LaserAPower;
                                    if (!Workspace.This.IVVM.IsLaserL1Selected)
                                    {
                                        LPower = 0;
                                    }
                                    R2imageSet += string.Format("_{0}-{1}-{2}mW", "L", Workspace.This.IVVM.WL1 + Workspace.This.IVVM.WL1Sign, LPower);
                                }
                                if (_RangeNum >= 0 && ScanWorkCount > 0)
                                {
                                    int tempRangeNum = _RangeNum + 1;
                                    R2imageSet += string.Format("_ROI{0}", tempRangeNum);
                                }
                                if (Workspace.This.ZAutomaticallyFocalVM.SelectedFocus != "None" && Workspace.This.ZAutomaticallyFocalVM.Ofimages <= Workspace.This.ZAutomaticallyFocalVM._FocusOptionsList.Count)//
                                {
                                    double focus = Workspace.This.EthernetController.DeviceProperties.ZFocusPosition +
                                        (Workspace.This.ZAutomaticallyFocalVM._FocusOptionsList[_StackNum].Value - Workspace.This.EthernetController.DeviceProperties.ZFocusPosition);
                                    R2imageSet += string.Format("_Focus{0}", Math.Round(focus, 2));
                                }
                                if (Workspace.This.IVVM.IsCaptrueR2Selected)
                                {
                                    Workspace.This.NewDocument(_ChannelBImage, imageInfoR2, R2imageSet + "_Date" + timeNow, false, true, true);
                                }
                            }

                            // Oh oh something went wrong - handle the error
                            if (_ChannelAImage != null || _ChannelBImage != null || _ChannelCImage != null)
                            {
                                _ChannelAImage = null;
                                _ChannelBImage = null;
                                _ChannelCImage = null;
                                GC.Collect();
                            }
                        }
                        catch
                        {
                        }
                    }
                    else if (scannedThread.ScanType == ScanTypes.Static || scannedThread.ScanType == ScanTypes.Vertical || scannedThread.ScanType == ScanTypes.XAxis)
                    {
                        try
                        {
                            Point[] chA = new Point[scannedThread.SampleValueChannelA.Length];
                            Point[] chB = new Point[scannedThread.SampleValueChannelB.Length];
                            Point[] chC = new Point[scannedThread.SampleValueChannelC.Length];

                            double coeffX = DataRate;
                            double coeffChA = 1;
                            double coeffChB = 1;
                            double coeffChC = 1;
                            switch (scannedThread.ScanType)
                            {
                                case ScanTypes.Static:
                                    coeffX = DataRate;
                                    coeffChA = 1;
                                    coeffChB = 1;
                                    coeffChC = 1;
                                    break;
                                case ScanTypes.Vertical:
                                    coeffX = 8 / SettingsManager.ConfigSettings.ZMotorSubdivision;
                                    coeffChA = 1;
                                    coeffChB = 1;
                                    coeffChC = 1;
                                    break;
                                case ScanTypes.XAxis:
                                    coeffX = 1;
                                    coeffChA = -1 / SettingsManager.ConfigSettings.XMotorSubdivision;
                                    //coeffChB = 1 / SettingsManager.ConfigSettings.XEncoderSubdivision;
                                    coeffChB = 1 / Workspace.This.NewParameterVM.XEncoderSubdivision;
                                    coeffChC = 1;
                                    break;
                            }
                            if (scannedThread.ScanType == ScanTypes.XAxis)
                            {
                                for (int i = 0; i < chA.Length; i++)
                                {
                                    chA[i].X = scannedThread.SampleIndex[i] * coeffX;
                                    chA[i].Y = scannedThread.SampleValueChannelA[i] * coeffChA;
                                    chB[i].X = chA[i].X;
                                    chB[i].Y = scannedThread.SampleValueChannelB[i] * coeffChB;
                                    chC[i].X = chA[i].X;
                                    chC[i].Y = scannedThread.SampleValueChannelC[i] * coeffChC;
                                }
                                Commond = new EnumerableDataSource<Point>(chA);
                                Feedback = new EnumerableDataSource<Point>(chB);
                                //ChannelC = new EnumerableDataSource<Point>(chC);
                                Commond.SetXYMapping(p => p);
                                Feedback.SetXYMapping(p => p);
                                // ChannelC.SetXYMapping(p => p);
                                Workspace.This.SelectedTabIndex = (int)ApplicationTabType.ScanChart; // Switch to ScanChart tab
                            }
                            else
                            {

                                if (scannedThread.ScanType == ScanTypes.Static)
                                {
                                    Thread ThreadStatic = new Thread(StaticData);
                                    ThreadStatic.IsBackground = true;
                                    ThreadStatic.Start(new object[] { chA, chB, chC, scannedThread.SampleValueChannelA, scannedThread.SampleValueChannelB, scannedThread.SampleValueChannelC });
                                }
                                else
                                {
                                    int[] _tempA = MovingAverage(scannedThread.SampleValueChannelA, 5);
                                    int[] _tempB = MovingAverage(scannedThread.SampleValueChannelB, 5);
                                    int[] _tempC = MovingAverage(scannedThread.SampleValueChannelC, 5);
                                    for (int i = 0; i < chA.Length; i++)
                                    {
                                        chA[i].X = scannedThread.SampleIndex[i] * coeffX;
                                        chA[i].Y = _tempA[i] * coeffChA;
                                        chB[i].X = chA[i].X;
                                        chB[i].Y = _tempB[i] * coeffChB;
                                        chC[i].X = chA[i].X;
                                        chC[i].Y = _tempC[i] * coeffChC;
                                    }
                                    ChannelA = new EnumerableDataSource<Point>(chA);
                                    ChannelB = new EnumerableDataSource<Point>(chB);
                                    ChannelC = new EnumerableDataSource<Point>(chC);
                                    ChannelA.SetXYMapping(p => p);
                                    ChannelB.SetXYMapping(p => p);
                                    ChannelC.SetXYMapping(p => p);
                                }
                                Workspace.This.SelectedTabIndex = (int)ApplicationTabType.ScanChart; // Switch to ScanChart tab
                            }

                        }
                        catch (Exception ex)
                        {
                        }
                    }

                    // Remove scanned preview image
                    Workspace.This.Owner.Dispatcher.BeginInvoke((Action)delegate
                    {
                        Workspace.This.DisplayImage = null;
                    });
                }
                else if (exitState == ThreadBase.ThreadExitStat.Error)
                {
                    // Oh oh something went wrong - handle the error
                    if (_ChannelAImage != null || _ChannelBImage != null || _ChannelCImage != null)
                    {
                        _ChannelAImage = null;
                        _ChannelBImage = null;
                        _ChannelCImage = null;
                        GC.Collect();
                    }
                    WorkDone();
                    string caption = "Scanning error...";
                    Log.Error(this, scannedThread.Error.Message);
                    string message = string.Format("Scanning error: {0}", scannedThread.Error.Message);
                    MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }

                _ScanningProcess.Completed -= new CommandLib.ThreadBase.CommandCompletedHandler(_ImageScanCommand_Completed);
                //_ImageScanCommand.CommandStatus -= new ImageScanCommand.CommandStatusHandler(_ImageScanCommand_CommandStatus);
                _ScanningProcess = null;
                //Not selecting multi-region only Z stack scan is selected
                if (Workspace.This.ZAutomaticallyFocalVM.SelectedFocus == "Z-Stacking" && ScanWorkCount <= 0)
                {
                    _StackNum++;
                    ScanCommondList(_StackNum, _RangeNum);
                    if (_StackNum >= Workspace.This.ZAutomaticallyFocalVM._FocusOptionsList.Count)
                    {
                        // The scan is complete
                        WorkDone();
                        Workspace.This.SelectedTabIndex = (int)ApplicationTabType.Gallery;   // Switch to gallery tab
                    }
                }
                else if (Workspace.This.ZAutomaticallyFocalVM.SelectedFocus == "None" && ScanWorkCount > 0)// Only multi-area scanning is selected
                {

                    _RangeNum++;
                    ScanCommondList(_StackNum, _RangeNum);
                    if (_RangeNum >= ScanWorkCount)
                    {
                        // The scan is complete
                        WorkDone();
                        Workspace.This.SelectedTabIndex = (int)ApplicationTabType.Gallery;   // Switch to gallery tab
                    }
                }
                else if (Workspace.This.ZAutomaticallyFocalVM.SelectedFocus == "Z-Stacking" && ScanWorkCount > 0)// Only multi-area scanning is selected and Z-Stacking
                {
                    _StackNum++;
                    ScanCommondList(_StackNum, _RangeNum);
                    if (_StackNum >= Workspace.This.ZAutomaticallyFocalVM._FocusOptionsList.Count && _RangeNum < ScanWorkCount)
                    {
                        _StackNum = 0;
                        _RangeNum++;
                        if (_RangeNum >= ScanWorkCount)
                        {
                            // The scan is complete
                            WorkDone();
                            Workspace.This.SelectedTabIndex = (int)ApplicationTabType.Gallery;   // Switch to gallery tab

                        }
                        ScanCommondList(_StackNum, _RangeNum);
                    }
                }
                else
                {
                    // The scan is complete
                    WorkDone();
                    if (scannedThread.ScanType == ScanTypes.Static || scannedThread.ScanType == ScanTypes.Vertical || scannedThread.ScanType == ScanTypes.XAxis)
                    {
                        Workspace.This.SelectedTabIndex = (int)ApplicationTabType.ScanChart;
                    }
                    else
                    {
                        Workspace.This.SelectedTabIndex = (int)ApplicationTabType.Gallery;   // Switch to gallery tab
                    }
                }
                Workspace.This.DisconnectDeviceEnable = true;  //Optical module button enable
            });
        }
        /// <summary>
        /// 静态模式扫描数据处理
        /// Static mode scanning data processing
        /// </summary>
        /// <param name="Parameter"></param>
        private void StaticData(object Parameter)
        {
            object[] domain = (object[])Parameter;
            Point[] chA = (Point[])domain[0];
            Point[] chB = (Point[])domain[1];
            Point[] chC = (Point[])domain[2];
            int[] SampleValueChannelA = (int[])domain[3];
            int[] SampleValueChannelB = (int[])domain[4];
            int[] SampleValueChannelC = (int[])domain[5];
            double detaX = DataRate;
            for (int i = 0; i < chA.Length; i++)
            {
                chA[i].X = i * detaX;
                chA[i].Y = SampleValueChannelA[i];
                chB[i].X = i * detaX;
                chB[i].Y = SampleValueChannelB[i];
                chC[i].X = i * detaX;
                chC[i].Y = SampleValueChannelC[i];
            }
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                ChannelA = new EnumerableDataSource<Point>(chA);
                ChannelB = new EnumerableDataSource<Point>(chB);
                ChannelC = new EnumerableDataSource<Point>(chC);
                ChannelA.SetXYMapping(p => p);
                ChannelB.SetXYMapping(p => p);
                ChannelC.SetXYMapping(p => p);
                Workspace.This.SelectedTabIndex = (int)ApplicationTabType.ScanChart; // Switch to ScanChart tab
            });
        }

        public bool CanExecuteScanCommand(object parameter)
        {
            return true;
        }
        /// <summary>
        /// 通道实时预览
        /// Channel Live Preview
        /// </summary>
        private void ScanDynamicDisplay()
        {
            while (true)
            {
                Thread.Sleep(2000);
                if (Workspace.This.IsScanning && dynamSwitch)
                {
                    try
                    {
                        WriteableBitmap temp;
                        // int OffsetWidth = (int)((Workspace.This.NewParameterVM.OpticalL_R1Distance * 1000) / SelectedResolution.Value);
                        switch (_NowShowImage)
                        {
                            case ScanChannel.A:
                                ImageProcessing.FrameToBitmap(out temp, _ScanningProcess._APDChannelA, _CurrentRangeWidth, _CurrentRangeHeight);
                                break;
                            case ScanChannel.B:
                                ImageProcessing.FrameToBitmap(out temp, _ScanningProcess._APDChannelB, _CurrentRangeWidth, _CurrentRangeHeight);
                                break;
                            case ScanChannel.C:
                                ImageProcessing.FrameToBitmap(out temp, _ScanningProcess._APDChannelC, _CurrentRangeWidth, _CurrentRangeHeight);
                                break;
                            default:
                                ImageProcessing.FrameToBitmap(out temp, _ScanningProcess._APDChannelA, _CurrentRangeWidth, _CurrentRangeHeight);
                                break;
                        }
                        if (temp != null)
                        {
                            if (temp.CanFreeze) { temp.Freeze(); }
                            BitmapImage UiUpdate = ImageProcessing.ConvertWriteableBitmapToBitmapImage(temp, _CurrentRangeWidth, _CurrentRangeHeight);
                            //System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(UiUpdate.PixelWidth, UiUpdate.PixelHeight, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                            //System.Drawing.Imaging.BitmapData data = bmp.LockBits(
                            //new System.Drawing.Rectangle(System.Drawing.Point.Empty, bmp.Size), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                            //UiUpdate.CopyPixels(Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride);
                            //bmp.UnlockBits(data);
                            //UiUpdate = ImageProcessing.BitmapToBitmapImage(bmp);
                            //if (UiUpdate.CanFreeze) { UiUpdate.Freeze(); }
                            Workspace.This.Owner.Dispatcher.BeginInvoke((Action)delegate
                            {
                                Workspace.This.DisplayImage = UiUpdate.Clone();
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        ExecuteStopScanCommand(null);
                        throw new Exception(ex.Message);
                    }
                }

            }
        }
        #endregion


        #region   ScanDirectionSample
        private void CurrentScanDirection()
        {

            if (ScanDeltaX == 0 && ScanDeltaY == 0 && ScanDeltaZ == 0)
            {
                CurrentScanHeaderTitle = "Static Sample";
                CurrentScanHorizontalTitle = "Time (s)";
                CurrentScanVerticaTitle = "ADC Value";
                //ScanTypes.Static;
                ChannelVisibility = Visibility.Visible;
                CommondFeedbackVisibility = Visibility.Hidden;
            }
            else if (ScanDeltaX == 0 && ScanDeltaY == 0)
            {
                CurrentScanHeaderTitle = "Z-Scan";
                CurrentScanHorizontalTitle = "Z-Pos(mm)";
                CurrentScanVerticaTitle = "ADC Value";
                //ScanTypes.Vertical;
                ChannelVisibility = Visibility.Visible;
                CommondFeedbackVisibility = Visibility.Hidden;
            }
            else if (ScanDeltaX > 0 && ScanDeltaY > 0)
            {

                //ScanTypes.Horizontal;
            }
            else if (ScanDeltaX > 0 && DataRate > 0)
            {
                CurrentScanHeaderTitle = "X-Scan";
                CurrentScanHorizontalTitle = "Time (s)";
                CurrentScanVerticaTitle = "X-Pos (um)";
                ChannelVisibility = Visibility.Hidden;
                CommondFeedbackVisibility = Visibility.Visible;
                //ScanTypes.XAxis;
            }

        }
        #endregion

        #region StopScanCommand

        public ICommand StopScanCommand
        {
            get
            {
                if (_StopScanCommand == null)
                {
                    _StopScanCommand = new RelayCommand(ExecuteStopScanCommand, CanExecuteStopScanCommand);
                }

                return _StopScanCommand;
            }
        }
        public void ExecuteStopScanCommand(object parameter)
        {
            if (_ScanningProcess != null)
            {
                //终止当前扫描
                //Terminate the current scan
                WorkDone();
                //Enable Motor control
                Workspace.This.MotorIsAlive = true;
                //光学模块按钮启用  Optical module button enable
                Workspace.This.DisconnectDeviceEnable = true; 
                _ScanningProcess.Abort();  // Abort the scanning thread
                if (Workspace.This.MotorVM.IsNewFirmware)
                {
                    Workspace.This.MotorVM.MotionController.SetStart(EthernetCommLib.MotorTypes.X | EthernetCommLib.MotorTypes.Y | EthernetCommLib.MotorTypes.Z,
                        new bool[] { false, false, false });
                }
            }
            else
            { //终止当前扫描
                //Terminate the current scan
                WorkDone();
                //Enable Motor control
                Workspace.This.MotorIsAlive = true;
                Workspace.This.DisconnectDeviceEnable = true; //光学模块按钮启用  Optical module button enable
            }
        }
        public bool CanExecuteStopScanCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region ChACommand

        public ICommand ChACommand
        {
            get
            {
                if (_ChACommand == null)
                {
                    _ChACommand = new RelayCommand(ExecuteChACommand, CanExecuteChACommand);
                }

                return _ChACommand;
            }
        }
        public void ExecuteChACommand(object parameter)
        {
            if (_ChACommand != null)
            {
                _NowShowImage = ScanChannel.A;
                NowChannel = "APD CHA";
            }
        }
        public bool CanExecuteChACommand(object parameter)
        {
            return true;
        }

        #endregion ChACommand

        #region ChBCommand

        public ICommand ChBCommand
        {
            get
            {
                if (_ChBCommand == null)
                {
                    _ChBCommand = new RelayCommand(ExecuteChBCommand, CanExecuteChBCommand);
                }

                return _ChBCommand;
            }
        }
        public void ExecuteChBCommand(object parameter)
        {
            if (_ChBCommand != null)
            {
                _NowShowImage = ScanChannel.B;
                NowChannel = "APD CHB";
            }
        }
        public bool CanExecuteChBCommand(object parameter)
        {
            return true;
        }

        #endregion ChBCommand

        #region ChCCommand

        public ICommand ChCCommand
        {
            get
            {
                if (_ChCCommand == null)
                {
                    _ChCCommand = new RelayCommand(ExecuteChCCommand, CanExecuteChCCommand);
                }

                return _ChCCommand;
            }
        }
        public void ExecuteChCCommand(object parameter)
        {
            if (_ChCCommand != null)
            {
                _NowShowImage = ScanChannel.C;
                NowChannel = "APD CHC";
            }
        }
        public bool CanExecuteChCCommand(object parameter)
        {
            return true;
        }

        #endregion ChCCommand

        #region ChDCommand

        public ICommand ChDCommand
        {
            get
            {
                if (_ChDCommand == null)
                {
                    _ChDCommand = new RelayCommand(ExecuteChDCommand, CanExecuteChDCommand);
                }

                return _ChDCommand;
            }
        }
        public void ExecuteChDCommand(object parameter)
        {
            if (_ChDCommand != null)
            {
                _NowShowImage = ScanChannel.D;
                NowChannel = "APD CHD";
            }
        }
        public bool CanExecuteChDCommand(object parameter)
        {
            return true;
        }

        #endregion ChDCommand

        #region SettingsCommand
        private RelayCommand _SettingsCommand = null;
        public ICommand SettingsCommand
        {
            get
            {
                if (_SettingsCommand == null)
                {
                    _SettingsCommand = new RelayCommand(ExecuteSettingsCommand, CanExecuteSettingsCommand);
                }

                return _SettingsCommand;
            }
        }
        public void ExecuteSettingsCommand(object parameter)
        {
            //if (Workspace.This.IsCapturing || Workspace.This.IsContinuous)
            //{
            //    string caption = "Camera Mode";
            //    string message = "Camera mode is busy.\nWould you like to terminate the current operation?";
            //    System.Windows.MessageBoxResult dlgResult = System.Windows.MessageBox.Show(message, caption, System.Windows.MessageBoxButton.YesNo);
            //    if (dlgResult == System.Windows.MessageBoxResult.No)
            //    {
            //        return;
            //    }

            //    CameraViewModel viewModel = Workspace.This.CameraVM;
            //    if (Workspace.This.IsCapturing)
            //    {
            //        viewModel.ExecuteStopCaptureCommand(null);
            //    }
            //    else
            //    {
            //        viewModel.ExecuteStopContinuousCommand(null);
            //    }
            //}

            //ParameterSetup paramSetupWin = new ParameterSetup();
            NewParameterSetup newParameterSetup = new NewParameterSetup();
            // Needed for centering this dialogbox in the center of the parent window
            newParameterSetup.Owner = Workspace.This.Owner;
            newParameterSetup.DataContext = Workspace.This.NewParameterVM;
            newParameterSetup.ShowDialog();
        }

        public bool CanExecuteSettingsCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region Preview Channel

        public List<string> OptionsChannels { get; }
        public string SelectedChannel
        {
            get { return _SelectedChannel; }
            set
            {
                if (_SelectedChannel != value)
                {
                    _SelectedChannel = value;
                    RaisePropertyChanged(nameof(SelectedChannel));
                    switch (_SelectedChannel)
                    {
                        case "R1":
                            _NowShowImage = ScanChannel.A;
                            NowChannel = "APD R1";
                            break;
                        case "R2":
                            _NowShowImage = ScanChannel.B;
                            NowChannel = "APD R2";
                            break;
                        case "L":
                            _NowShowImage = ScanChannel.C;
                            NowChannel = "APD L";
                            break;
                    }
                }
            }
        }

        #endregion

        #region Spent time Method

        private void OnSpentTimeInit()
        {
            SpentTime = 0;
            ShowTimer = new DispatcherTimer();
            ShowTimer.Tick += OnTimeLoadData;
            ShowTimer.Interval = new TimeSpan(0, 0, 1);//1 sec

        }

        private void OnSpentTimeStop()
        {
            _IsOnSpentTimeScanDataReceivedCheck = false;
            ShowTimer.Stop();
        }

        private void OnSpentTimeStart()
        {
            SpentTime = 0;
            ShowTimer.Start();
        }

        bool _IsOnSpentTimeScanDataReceivedCheck = false;
        int _packcountTime = 0;
        int _GetScanninPackCount = 0;
        /// <summary>
        /// 判断当前的设备状态是否已经触碰驱动器而收不到数据
        /// Determine if the current device state has touched the drive and no data is received
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTimeLoadData(object sender, EventArgs e)
        {
            SpentTime++;
            if (_ScanDeltaX > 0 && _ScanDeltaY > 0)
            {
                if (_IsOnSpentTimeScanDataReceivedCheck)
                {
                    _packcountTime++;
                    if (_packcountTime == 10)
                    {
                        _GetScanninPackCount = RemainingTime;
                    }
                    if (_packcountTime == 45)
                    {
                        if (_GetScanninPackCount == RemainingTime)
                        {
                            Log.Fatal(this, "Serious error, no data received!");
                            throw new Exception("Serious error, no data received!");

                        }
                        _packcountTime = 0;
                    }
                }
            }
        }
        #endregion

        #region Horizontal scan X speed calibration
        private RelayCommand _HorizontalScanSpeedCalibrationCmd;
        private Thread _CalibrateThread;
        public RelayCommand HorizontalScanSpeedCalibrationCmd
        {
            get
            {
                if (_HorizontalScanSpeedCalibrationCmd == null)
                {
                    _HorizontalScanSpeedCalibrationCmd = new RelayCommand(ExecuteHorizontalScanSpeedCalibrationCmd, CanExecuteHorizontalScanSpeedCalibrationCmd);
                }
                return _HorizontalScanSpeedCalibrationCmd;
            }
        }

        private void ExecuteHorizontalScanSpeedCalibrationCmd(object obj)
        {
            if (Workspace.This.IsScanning)
            {
                if(_CalibrateThread!=null && _CalibrateThread.IsAlive)
                {
                    _CalibrateThread.Abort();
                    _CalibrateThread.Join();
                }
                return;
            }

            _CalibrateThread = new Thread(CalibrateXSpeed);
            _CalibrateThread.IsBackground = true;
            _CalibrateThread.Start();
        }

        private bool CanExecuteHorizontalScanSpeedCalibrationCmd(object obj)
        {
            return true;
        }
        /// <summary>
        /// X axis motor speed calibration
        /// </summary>
        private void CalibrateXSpeed()
        {
            //Workspace.This.MotorVM.HomeCommand.Execute(MotorType.X);
            //Workspace.This.MotorVM.HomeCommand.Execute(MotorType.Y);
            //do
            //{
            //    Thread.Sleep(100);
            //}
            //while (!Workspace.This.MotorVM.MotionController.CrntState[Avocado.EthernetCommLib.MotorTypes.X].AtHome ||
            //!Workspace.This.MotorVM.MotionController.CrntState[Avocado.EthernetCommLib.MotorTypes.Y].AtHome);

            Workspace.This.MotorVM.AbsXPos = 10;
            Workspace.This.MotorVM.AbsYPos = 50;
            Workspace.This.MotorVM.GoAbsPosCommand.Execute(MotorType.X);
            Workspace.This.MotorVM.GoAbsPosCommand.Execute(MotorType.Y);
            Workspace.This.MotorVM.MotionController.GetMotionInfo(MotorTypes.X | MotorTypes.Y);
            do
            {
                Thread.Sleep(100);
            }
            while(Workspace.This.MotorVM.MotionController.CrntState[MotorTypes.X].IsBusy ||
            Workspace.This.MotorVM.MotionController.CrntState[MotorTypes.Y].IsBusy);

            ScanX0 = Workspace.This.MotorVM.CurrentXPos;
            ScanY0 = Workspace.This.MotorVM.CurrentYPos;
            ScanZ0 = Workspace.This.MotorVM.CurrentZPos;
            double startDx = ScanDeltaX;
            if(startDx < 30)        // minimum dx is 30 mm (10 mm scan + 20 mm extra move)
            {
                startDx = 30;
            }
            ScanDeltaY = 0;
            ScanDeltaZ = 0;

            // sample 10000 points @ 5ms/points, total time is 50 sec
            DataRate = 5;
            LineCounts = 10000;

            int[,] calibratedSpeedArray = new int[311, 4];

            string q1_fileName = Path.Combine(SettingsManager.ApplicationDataPath, "Quality1_Calibrated X Speeds.csv");
            string q2_fileName = Path.Combine(SettingsManager.ApplicationDataPath, "Quality2_Calibrated X Speeds.csv");
            string q4_fileName = Path.Combine(SettingsManager.ApplicationDataPath, "Quality4_Calibrated X Speeds.csv");
            string q8_fileName = Path.Combine(SettingsManager.ApplicationDataPath, "Quality8_Calibrated X Speeds.csv");
            string processingFileName = Path.Combine(SettingsManager.ApplicationDataPath, "Calibration Process Record.csv");
            File.WriteAllText(q1_fileName, "dx, Q=1\r\n");
            File.WriteAllText(q2_fileName, "dx, Q=2\r\n");
            File.WriteAllText(q4_fileName, "dx, Q=4\r\n");
            File.WriteAllText(q8_fileName, "dx, Q=8\r\n");

            for (int qualityLoop = 0;qualityLoop < 4; qualityLoop++)         // quality 1,2,4,8
            {
                for(ScanDeltaX = startDx; ScanDeltaX < 320; ScanDeltaX++)
                {
                    SelectedQuality = QualityOptions[qualityLoop];
                    int singleTrip = (int)Math.Round(ScanDeltaX * SettingsManager.ConfigSettings.XMotorSubdivision);
                    double singleTripTime = SelectedQuality.Value / 2.0 - SettingsManager.ConfigSettings.XMotionTurnDelay / 1000.0;
                    int accVal = (int)Math.Round(SettingsManager.ConfigSettings.XMotorSubdivision * SettingsManager.ConfigSettings.MotorSettings[0].Accel);
                    HorizontalCalibrationSpeed = XMotorSpeedCalibration.GetSpeed(accVal, 256, singleTrip, singleTripTime);

                    bool calibrateFinished = false;
                    int tryCounts = 0;
                    while(calibrateFinished==false)
                    {
                        if(tryCounts++ > 20)
                        {
                            File.AppendAllText(processingFileName, string.Format("Calibrate failed: dx={0}, Q={1}\r\n", ScanDeltaX, SelectedQuality.Value));
                            break;
                        }
                        ScanCommand.Execute(null);
                        while (Workspace.This.IsScanning)
                        {
                            Thread.Sleep(100);
                        }
                        Thread.Sleep(100);
                        // find moving start position around 48th second
                        Point[] cmdPositions = (Point[])ChannelA.Data;
                        int startIndex = 100;
                        int startCounts = 0;
                        do
                        {
                            if (cmdPositions[startIndex].Y == 0 && cmdPositions[startIndex + 1].Y > 0)
                            {
                                startCounts++;
                                if (startCounts == 48 / SelectedQuality.Value)
                                {
                                    break;
                                }
                                startIndex += 100;
                            }
                            else if (cmdPositions[startIndex].Y > 0 && cmdPositions[startIndex + 1].Y > 0)
                            {
                                startIndex += 10;
                            }
                            else
                            {
                                startIndex++;
                            }
                        }
                        while (startCounts < 48 / SelectedQuality.Value);

                        int tolerance = 10;
                        if (tryCounts > 10)
                        {
                            tolerance = 20;     // maximum tolerance is +/-100ms
                        }
                        if(Math.Abs(startIndex - 48000/5) < tolerance)
                        {
                            calibrateFinished = true;
                            calibratedSpeedArray[(int)(ScanDeltaX - 10), qualityLoop] = HorizontalCalibrationSpeed;
                            File.AppendAllText(processingFileName, string.Format("Calibrated: dx={0}, Q={1}, Speed={2}\r\n", ScanDeltaX, SelectedQuality.Value, HorizontalCalibrationSpeed));
                            switch (qualityLoop)
                            {
                                case 0:
                                    File.AppendAllText(q1_fileName, string.Format("{0},{1}\r\n", ScanDeltaX, HorizontalCalibrationSpeed));
                                    break;
                                case 1:
                                    File.AppendAllText(q2_fileName, string.Format("{0},{1}\r\n", ScanDeltaX, HorizontalCalibrationSpeed));
                                    break;
                                case 2:
                                    File.AppendAllText(q4_fileName, string.Format("{0},{1}\r\n", ScanDeltaX, HorizontalCalibrationSpeed));
                                    break;
                                case 3:
                                    File.AppendAllText(q8_fileName, string.Format("{0},{1}\r\n", ScanDeltaX, HorizontalCalibrationSpeed));
                                    break;
                            }
                        }
                        else
                        {
                            // calibrate the speed
                            var newSpeed = HorizontalCalibrationSpeed * startIndex / (48000 / 5);
                            var speedDif = newSpeed - HorizontalCalibrationSpeed;
                            if (tryCounts > 14)
                            {
                                if(speedDif > 0) { speedDif = 10; }
                                else { speedDif = -10; }
                            }
                            HorizontalCalibrationSpeed += speedDif;
                            File.AppendAllText(processingFileName,
                                string.Format("Calibrating: dx={0}, Q={1}, 48th moving at{2} msec, calibrated speed={3}\r\n",
                                ScanDeltaX, SelectedQuality.Value, startIndex * 5, HorizontalCalibrationSpeed));
                        }
                        while (_ScanningProcess != null)
                        {
                            Thread.Sleep(100);
                        }
                    }
                }
            }

            using (FileStream fs = new FileStream(Path.Combine(SettingsManager.ApplicationDataPath, "Calibrated X Speeds.csv"), FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(fs))
                {
                    writer.WriteLine("dx, Q=1, Q=2, Q=4, Q=8");
                    for(int i = 0; i < 311; i++)
                    {
                        writer.WriteLine("{0},{1},{2},{3},{4}", i + 10, calibratedSpeedArray[i, 0], calibratedSpeedArray[i, 1], calibratedSpeedArray[i, 2], calibratedSpeedArray[i, 3]);
                    }
                }
            }
        }
        #endregion Horizontal scan X speed calibration

        #region MovingAverage
        /// <summary>
        /// MovingAverage smooth
        /// </summary>
        /// <param name="data"></param>
        /// <param name="span"></param>
        /// <returns></returns>
        public static int[] MovingAverage(int[] data, int span)
        {
            int b = 0;
            if (span % 2 == 1)
                b = (span - 1) / 2;
            else
            {
                span -= 1;
                b = (span - 1) / 2;
            }
            int[] smoothArray = new int[data.Length];
            if (data.Length > span)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    if (i < b)
                    {
                        smoothArray[i] = 0;
                        for (int j = -i; j < i + 1; j++)
                        {
                            smoothArray[i] += data[i + j];
                        }
                        smoothArray[i] /= (2 * i + 1);
                    }
                    else if (i >= b && (data.Length - i) > b)
                    {
                        smoothArray[i] = 0;
                        for (int j = -b; j < b + 1; j++)
                        {
                            smoothArray[i] += data[i + j];
                        }
                        smoothArray[i] /= span;
                    }
                    else
                    {
                        smoothArray[i] = 0;
                        int c = data.Length - i - 1;
                        for (int j = -c; j < c + 1; j++)
                        {
                            smoothArray[i] += data[i + j];
                        }
                        smoothArray[i] /= (2 * c + 1);

                    }
                }
            }
            else
            {
                Log.Error("MovingAverage", "span is bigger than data's count");
                throw new ArgumentException("span is bigger than data's count");
              
            }

            return smoothArray;
        }
        #endregion

        #region
        private void WorkDone()
        {
            for (int RangeCount = 0; RangeCount < ScanWorkCount; RangeCount++)
            {
                //关闭所有ROI里打开的激光
                //Turn off all lasers turned on in ROI
                SetRegionScanWorkLaserReceived(RangeCount + 1);
            }
            _IsScanTest = false;
            _StackNum = 0;
            _RangeNum = 0;
            //Turn off all the lasers after scanning is completed
            Workspace.This.IVVM.TurnOffAllLasers();
            OnScanEnbledRegionReceived(true);
            if (Workspace.This.ZAutomaticallyFocalVM.SelectedFocus == "Z-Stacking")
                Workspace.This.ZAutomaticallyFocalVM.IsWorkEnabled = true;
            Workspace.This.ZAutomaticallyFocalVM.IsFoucsEnabled = true;
        }
        #endregion

        public void ScanScopelimitations(ref bool value)
        {
            value = true;
            ScanDynamicScopeString = "";
            ScanDynamicScopeStringType = "";
            if ((_ScanX0 + _ScanDeltaX) / _XMotorSubdivision > (XHomecoefficient - (Workspace.This.EthernetController.DeviceProperties.OpticalLR2Distance)))
            {
                ExecuteStopScanCommand(null);
                value = false;
                string message = String.Format("The X should be 0=<X0+DX<={0}", XHomecoefficient - (Workspace.This.EthernetController.DeviceProperties.OpticalLR2Distance));
                Window window = new Window();
                MessageBox.Show(window, message);
                return;
            }
            if ((_ScanY0 + _ScanDeltaY) / (double)_YMotorSubdivision > ((double)_YMaxValue / (double)_YMotorSubdivision) - 5)
            {
                ExecuteStopScanCommand(null);
                value = false;
                string message = String.Format("The Y should be 0=<Y0+DY<={0}", ((double)_YMaxValue / (double)_YMotorSubdivision) - 5);
                Window window = new Window();
                MessageBox.Show(window, message);
                return;
            }
            if ((_ScanZ0 + _ScanDeltaZ) / (double)_ZMotorSubdivision > ((double)_ZMaxValue / (double)_ZMotorSubdivision))
            {
                ExecuteStopScanCommand(null);
                value = false;
                string message = String.Format("The Z should be 0=<Z0+DZ<={0}", (double)_ZMaxValue / (double)_ZMotorSubdivision);
                Window window = new Window();
                MessageBox.Show(window, message);
                return;
            }
        }

        #region unit conversion

        private const int KbDiv = 1024;
        private const int MbDiv = 1024 * 1024;
        private const int GbDiv = 1024 * 1024 * 1024;

        #endregion

        /// <summary>
        ///     MemoryAvailable
        /// </summary>
        internal static long? GetMemoryAvailable()
        {
            long availablebytes = 0;
            var managementClassOs = new ManagementClass("Win32_OperatingSystem");
            foreach (var managementBaseObject in managementClassOs.GetInstances())
                if (managementBaseObject["FreePhysicalMemory"] != null)
                    availablebytes = 1024 * long.Parse(managementBaseObject["FreePhysicalMemory"].ToString());
            return availablebytes / MbDiv;
        }
        internal static long? GetImageMemory(int width, int height)
        {

            return (((width * height) * 3) / MbDiv) + 100;  //3 channel  Accumulate the image space calculation by 100M to avoid the same memory space

        }
    }

    /*public class ResolutionType
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
    }*/

    /*public class QualityType
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

    }*/

}
