using Azure.APDCalibrationBench;
using Azure.ScannerTestJig.View.MultiChannelLaserCalibration;
using Azure.WPF.Framework;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Azure.ScannerTestJig.ViewModule.MultiChannelLaserCalibration
{
    class ViewModuleChartMultiChannelLaserCailbration : ViewModelBase
    {
        #region Private data
        private string[] _AvailablePorts;
        private ObservableCollection<string> _COMNumberCoeffOptions = new ObservableCollection<string>();
        private ObservableCollection<int> _ChannelNumberCoeffOptions = new ObservableCollection<int>();
        private ObservableCollection<int> _PhotodiodeVoltageCorrespondingToLaserPowerOptions = new ObservableCollection<int>();
        private string _SelectedCOMNumberCoeff;
        private int _SelectedChannelNumberCoeff;
        private int _SelectedPhotodiodeVoltageCorrespondingToLaserPower;
        public MultiChannelLaserCalibrationSub _WaveView532LaserSubWind = null;
        public DisplayMultiChannelLaserCalibrationData _Display532LaserData = null;
        private MultChannelLaserPort _Laser532ModelViewPort = null;
        public Thread HistoryThread;
        public Thread DataProcessThread;
        private RelayCommand _StartWaveCommand = null;
        private RelayCommand _Data532WaveCommand = null;
        private RelayCommand _Out532WaveCommand = null;
        private RelayCommand _WriteInteriorPDCommand = null;
        private RelayCommand _ReadInteriorPDCommand = null;
        private RelayCommand _WriteLaserOpticalPowerCommand = null;
        private RelayCommand _ReadLaserOpticalPowerCommand = null;
        private RelayCommand _WriteLaserCurrentCommand = null;
        private RelayCommand _ReadLaserCurrentCommand = null;
        private RelayCommand _WritePD532Command = null;
        private RelayCommand _ReadPD532Command = null;
        private RelayCommand _WritePhotodiodeVoltageCorrespondingToLaserPowerCommand = null;
        private RelayCommand _ReadPhotodiodeVoltageCorrespondingToLaserPowerCommand = null;
        private RelayCommand _ReadLaserTecTempCommand = null;
        private RelayCommand _WriteInstruct485Command = null;
        private EnumerableDataSource<Point> _LaserPowerCH1 = null;
        private EnumerableDataSource<Point> _LaserPowerCH2 = null;
        private EnumerableDataSource<Point> _LaserPowerCH3 = null;
        private EnumerableDataSource<Point> _LaserPowerCH4 = null;
        private EnumerableDataSource<Point> _LaserPowerCH5 = null;
        private EnumerableDataSource<Point> _LaserPowerCH6 = null;
        private EnumerableDataSource<Point> _LaserPowerCH7 = null;

        private EnumerableDataSource<Point> _HistoryLaserPowerCH1 = null;
        private EnumerableDataSource<Point> _HistoryLaserPowerCH2 = null;
        private EnumerableDataSource<Point> _HistoryLaserPowerCH3 = null;
        private EnumerableDataSource<Point> _HistoryLaserPowerCH4 = null;
        private EnumerableDataSource<Point> _HistoryLaserPowerCH5 = null;
        private EnumerableDataSource<Point> _HistoryLaserPowerCH6 = null;
        private EnumerableDataSource<Point> _HistoryLaserPowerCH7 = null;

        public VoltagePointCollectionMultiChannelLaserCailbration LaserPowerPointCollectionCH1;
        public VoltagePointCollectionMultiChannelLaserCailbration LaserPowerPointCollectionCH2;
        public VoltagePointCollectionMultiChannelLaserCailbration LaserPowerPointCollectionCH3;
        public VoltagePointCollectionMultiChannelLaserCailbration LaserPowerPointCollectionCH4;
        public VoltagePointCollectionMultiChannelLaserCailbration LaserPowerPointCollectionCH5;
        public VoltagePointCollectionMultiChannelLaserCailbration LaserPowerPointCollectionCH6;
        public VoltagePointCollectionMultiChannelLaserCailbration LaserPowerPointCollectionCH7;

        public VoltagePointCollectionMultiChannelLaserCailbration HistoryLaserPowerPointCollectionCH1;
        public VoltagePointCollectionMultiChannelLaserCailbration HistoryLaserPowerPointCollectionCH2;
        public VoltagePointCollectionMultiChannelLaserCailbration HistoryLaserPowerPointCollectionCH3;
        public VoltagePointCollectionMultiChannelLaserCailbration HistoryLaserPowerPointCollectionCH4;
        public VoltagePointCollectionMultiChannelLaserCailbration HistoryLaserPowerPointCollectionCH5;
        public VoltagePointCollectionMultiChannelLaserCailbration HistoryLaserPowerPointCollectionCH6;
        public VoltagePointCollectionMultiChannelLaserCailbration HistoryLaserPowerPointCollectionCH7;
        private string _LaserNumber;
        private double _GetCurrentTecTempValue = 0;
        private double _LaserOpticalPowerValue = 0;
        private double _InteriorPDValue = 0;
        private double _PD532Value = 0;
        private double _LaserCurrentValue = 0;
        private double _LaserElectricValue = 0;
        private double _PhotodiodeVoltageCorrespondingToLaserPowerValue = 0;
        private string _SetInstruct485 = "";
        private string _GetInstruct485 = "";
        string path = Directory.GetCurrentDirectory() + @"\测试报告\";
        public int isStartButton = 0;
        BrushConverter bc = new BrushConverter();
        public bool IsStart = false;
        public int index = 0;
        public int PeriodIndex = 0;
        //List<Dictionary<double, double>> CalList = new List<Dictionary<double, double>>();
        List<List<double>> CalList = new List<List<double>>();
        private double InteriorPDValueCH1 = 0;
        private double InteriorPDValueCH2 = 0;
        private double InteriorPDValueCH3 = 0;
        private double InteriorPDValueCH4 = 0;
        private double InteriorPDValueCH5 = 0;
        private double InteriorPDValueCH6 = 0;
        private double InteriorPDValueCH7 = 0;

        private double LaserOpticalPowerValueCH1 = 0;
        private double LaserOpticalPowerValueCH2 = 0;
        private double LaserOpticalPowerValueCH3 = 0;
        private double LaserOpticalPowerValueCH4 = 0;
        private double LaserOpticalPowerValueCH5 = 0;
        private double LaserOpticalPowerValueCH6 = 0;
        private double LaserOpticalPowerValueCH7 = 0;

        private double GetCurrentTecTempValueCH1 = 0;
        private double GetCurrentTecTempValueCH2 = 0;
        private double GetCurrentTecTempValueCH3 = 0;
        private double GetCurrentTecTempValueCH4 = 0;
        private double GetCurrentTecTempValueCH5 = 0;
        private double GetCurrentTecTempValueCH6 = 0;
        private double GetCurrentTecTempValueCH7 = 0;

        private double PD532ValueCH1 = 0;
        private double PD532ValueCH2 = 0;
        private double PD532ValueCH3 = 0;
        private double PD532ValueCH4 = 0;
        private double PD532ValueCH5 = 0;
        private double PD532ValueCH6 = 0;
        private double PD532ValueCH7 = 0;

        private double LaserElectricValueCH1 = 0;
        private double LaserElectricValueCH2 = 0;
        private double LaserElectricValueCH3 = 0;
        private double LaserElectricValueCH4 = 0;
        private double LaserElectricValueCH5 = 0;
        private double LaserElectricValueCH6 = 0;
        private double LaserElectricValueCH7 = 0;

        private double PhotodiodeVoltageCorrespondingToLaserPowerValueCH1 = 0;
        private double PhotodiodeVoltageCorrespondingToLaserPowerValueCH2 = 0;
        private double PhotodiodeVoltageCorrespondingToLaserPowerValueCH3 = 0;
        private double PhotodiodeVoltageCorrespondingToLaserPowerValueCH4 = 0;
        private double PhotodiodeVoltageCorrespondingToLaserPowerValueCH5 = 0;
        private double PhotodiodeVoltageCorrespondingToLaserPowerValueCH6 = 0;
        private double PhotodiodeVoltageCorrespondingToLaserPowerValueCH7 = 0;

        private bool _CH1Check = true;
        private bool _CH2Check = true;
        private bool _CH3Check = true;
        private bool _CH4Check = true;
        private bool _CH5Check = true;
        private bool _CH6Check = true;
        private bool _CH7Check = true;

        private double _CH1OpticalPower;
        private double _CH2OpticalPower;
        private double _CH3OpticalPower;
        private double _CH4OpticalPower;
        private double _CH5OpticalPower;
        private double _CH6OpticalPower;
        private double _CH7OpticalPower;

        private double _CH1Time;
        private double _CH2Time;
        private double _CH3Time;
        private double _CH4Time;
        private double _CH5Time;
        private double _CH6Time;
        private double _CH7Time;

        private string _CH1Txt="";
        private string _CH2Txt = "";
        private string _CH3Txt = "";
        private string _CH4Txt = "";
        private string _CH5Txt = "";
        private string _CH6Txt = "";
        private string _CH7Txt = "";
        #endregion
        public void ExcutePortConnectCommand()
        {
            try
            {
                if (_PhotodiodeVoltageCorrespondingToLaserPowerOptions.Count == 0)
                {
                    _PhotodiodeVoltageCorrespondingToLaserPowerOptions.Add(5);
                    _PhotodiodeVoltageCorrespondingToLaserPowerOptions.Add(10);
                    _PhotodiodeVoltageCorrespondingToLaserPowerOptions.Add(15);
                    _PhotodiodeVoltageCorrespondingToLaserPowerOptions.Add(20);
                    _PhotodiodeVoltageCorrespondingToLaserPowerOptions.Add(25);
                    _PhotodiodeVoltageCorrespondingToLaserPowerOptions.Add(30);
                    _PhotodiodeVoltageCorrespondingToLaserPowerOptions.Add(35);
                    _PhotodiodeVoltageCorrespondingToLaserPowerOptions.Add(40);
                    _PhotodiodeVoltageCorrespondingToLaserPowerOptions.Add(45);
                    _PhotodiodeVoltageCorrespondingToLaserPowerOptions.Add(50);
                    _PhotodiodeVoltageCorrespondingToLaserPowerOptions.Add(55);
                    _PhotodiodeVoltageCorrespondingToLaserPowerOptions.Add(60);
                }
                _AvailablePorts = SerialPort.GetPortNames();
                if (_AvailablePorts.Length == 0)
                {
                    MessageBox.Show("未找到串口设备！", "连接错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    _WaveView532LaserSubWind = new MultiChannelLaserCalibrationSub();
                    _WaveView532LaserSubWind.Title = Workspace.This.Owner.Title + "-532 WaveView Kit";
                    _WaveView532LaserSubWind.ShowDialog();
                    _WaveView532LaserSubWind = null;
                    return;
                }
                _COMNumberCoeffOptions.Clear();
                if (_COMNumberCoeffOptions.Count <= 0)
                {

                    for (int i = 0; i < _AvailablePorts.Length; i++)
                    {
                        _COMNumberCoeffOptions.Add(_AvailablePorts[i]);
                    }

                }
                if (_ChannelNumberCoeffOptions.Count <= 0)
                {
                    _ChannelNumberCoeffOptions.Add(1);
                    _ChannelNumberCoeffOptions.Add(2);
                    _ChannelNumberCoeffOptions.Add(3);
                    _ChannelNumberCoeffOptions.Add(4);
                    _ChannelNumberCoeffOptions.Add(5);
                    _ChannelNumberCoeffOptions.Add(6);
                    _ChannelNumberCoeffOptions.Add(7);

                }
                _SelectedChannelNumberCoeff = _ChannelNumberCoeffOptions[0];
                if (_AvailablePorts.Length > 0)
                {
                    if (_Laser532ModelViewPort == null || _Laser532ModelViewPort.IsConnected == false)
                    {
                        _Laser532ModelViewPort = new MultChannelLaserPort();
                        _Laser532ModelViewPort.PUBAddress = 0x01;
                        _Laser532ModelViewPort.Instruct485UpdateCommOutput += _Laser532ModelViewPort_Instruct485UpdateCommOutput;
                        //_Laser532ModelViewPort.InteriorPDUpdateCommOutputCH1 += _Laser532ModelViewPort_InteriorPDUpdateCommOutput1CH1;
                        //_Laser532ModelViewPort.InteriorPDUpdateCommOutputCH2 += _Laser532ModelViewPort_InteriorPDUpdateCommOutputCH2;
                        //_Laser532ModelViewPort.InteriorPDUpdateCommOutputCH3 += _Laser532ModelViewPort_InteriorPDUpdateCommOutputCH3;
                        //_Laser532ModelViewPort.InteriorPDUpdateCommOutputCH4 += _Laser532ModelViewPort_InteriorPDUpdateCommOutputCH4;
                        //_Laser532ModelViewPort.InteriorPDUpdateCommOutputCH5 += _Laser532ModelViewPort_InteriorPDUpdateCommOutputCH5;
                        //_Laser532ModelViewPort.InteriorPDUpdateCommOutputCH6 += _Laser532ModelViewPort_InteriorPDUpdateCommOutputCH6;
                        //_Laser532ModelViewPort.InteriorPDUpdateCommOutputCH7 += _Laser532ModelViewPort_InteriorPDUpdateCommOutputCH7;

                        _Laser532ModelViewPort.LaserOpticalPowerUpdateCommOutputCH1 += _Laser532ModelViewPort_LaserOpticalPowerUpdateCommOutputCH1;
                        _Laser532ModelViewPort.LaserOpticalPowerUpdateCommOutputCH2 += _Laser532ModelViewPort_LaserOpticalPowerUpdateCommOutputCH2;
                        _Laser532ModelViewPort.LaserOpticalPowerUpdateCommOutputCH3 += _Laser532ModelViewPort_LaserOpticalPowerUpdateCommOutputCH3;
                        _Laser532ModelViewPort.LaserOpticalPowerUpdateCommOutputCH4 += _Laser532ModelViewPort_LaserOpticalPowerUpdateCommOutputCH4;
                        _Laser532ModelViewPort.LaserOpticalPowerUpdateCommOutputCH5 += _Laser532ModelViewPort_LaserOpticalPowerUpdateCommOutputCH5;
                        _Laser532ModelViewPort.LaserOpticalPowerUpdateCommOutputCH6 += _Laser532ModelViewPort_LaserOpticalPowerUpdateCommOutputCH6;
                        _Laser532ModelViewPort.LaserOpticalPowerUpdateCommOutputCH7 += _Laser532ModelViewPort_LaserOpticalPowerUpdateCommOutputCH7;


                        _Laser532ModelViewPort.LaserPowerUpdateCommOutputCH1 += _Laser532ModelViewPort_LaserPowerUpdateCommOutputCH1;
                        _Laser532ModelViewPort.LaserPowerUpdateCommOutputCH2 += _Laser532ModelViewPort_LaserPowerUpdateCommOutputCH2;
                        _Laser532ModelViewPort.LaserPowerUpdateCommOutputCH3 += _Laser532ModelViewPort_LaserPowerUpdateCommOutputCH3;
                        _Laser532ModelViewPort.LaserPowerUpdateCommOutputCH4 += _Laser532ModelViewPort_LaserPowerUpdateCommOutputCH4;
                        _Laser532ModelViewPort.LaserPowerUpdateCommOutputCH5 += _Laser532ModelViewPort_LaserPowerUpdateCommOutputCH5;
                        _Laser532ModelViewPort.LaserPowerUpdateCommOutputCH6 += _Laser532ModelViewPort_LaserPowerUpdateCommOutputCH6;
                        _Laser532ModelViewPort.LaserPowerUpdateCommOutputCH7 += _Laser532ModelViewPort_LaserPowerUpdateCommOutputCH7;


                        _Laser532ModelViewPort.PDUpdateCommOutputCH1 += _Laser532ModelViewPort_InteriorPDUpdateCommOutputCH1;
                        _Laser532ModelViewPort.PDUpdateCommOutputCH2 += _Laser532ModelViewPort_InteriorPDUpdateCommOutputCH2;
                        _Laser532ModelViewPort.PDUpdateCommOutputCH3 += _Laser532ModelViewPort_InteriorPDUpdateCommOutputCH3;
                        _Laser532ModelViewPort.PDUpdateCommOutputCH4 += _Laser532ModelViewPort_InteriorPDUpdateCommOutputCH4;
                        _Laser532ModelViewPort.PDUpdateCommOutputCH5 += _Laser532ModelViewPort_InteriorPDUpdateCommOutputCH5;
                        _Laser532ModelViewPort.PDUpdateCommOutputCH6 += _Laser532ModelViewPort_InteriorPDUpdateCommOutputCH6;
                        _Laser532ModelViewPort.PDUpdateCommOutputCH7 += _Laser532ModelViewPort_InteriorPDUpdateCommOutputCH7;

                        _Laser532ModelViewPort.LaserElectricUpdateCommOutputCH1 += _Laser532ModelViewPort_LaserElectricUpdateCommOutputCH1;
                        _Laser532ModelViewPort.LaserElectricUpdateCommOutputCH2 += _Laser532ModelViewPort_LaserElectricUpdateCommOutputCH2;
                        _Laser532ModelViewPort.LaserElectricUpdateCommOutputCH3 += _Laser532ModelViewPort_LaserElectricUpdateCommOutputCH3;
                        _Laser532ModelViewPort.LaserElectricUpdateCommOutputCH4 += _Laser532ModelViewPort_LaserElectricUpdateCommOutputCH4;
                        _Laser532ModelViewPort.LaserElectricUpdateCommOutputCH5 += _Laser532ModelViewPort_LaserElectricUpdateCommOutputCH5;
                        _Laser532ModelViewPort.LaserElectricUpdateCommOutputCH6 += _Laser532ModelViewPort_LaserElectricUpdateCommOutputCH6;
                        _Laser532ModelViewPort.LaserElectricUpdateCommOutputCH7 += _Laser532ModelViewPort_LaserElectricUpdateCommOutputCH7;


                        _Laser532ModelViewPort.PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH1 += _Laser532ModelViewPort_PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH1;
                        _Laser532ModelViewPort.PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH2 += _Laser532ModelViewPort_PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH2;
                        _Laser532ModelViewPort.PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH3 += _Laser532ModelViewPort_PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH3;
                        _Laser532ModelViewPort.PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH4 += _Laser532ModelViewPort_PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH4;
                        _Laser532ModelViewPort.PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH5 += _Laser532ModelViewPort_PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH5;
                        _Laser532ModelViewPort.PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH6 += _Laser532ModelViewPort_PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH6;
                        _Laser532ModelViewPort.PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH7 += _Laser532ModelViewPort_PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH7;

                    }
                    //SampleValueLaserElectric = new double[step2];
                    LaserPowerPointCollectionCH1 = new VoltagePointCollectionMultiChannelLaserCailbration();
                    LaserPowerPointCollectionCH2 = new VoltagePointCollectionMultiChannelLaserCailbration();
                    LaserPowerPointCollectionCH3 = new VoltagePointCollectionMultiChannelLaserCailbration();
                    LaserPowerPointCollectionCH4 = new VoltagePointCollectionMultiChannelLaserCailbration();
                    LaserPowerPointCollectionCH5 = new VoltagePointCollectionMultiChannelLaserCailbration();
                    LaserPowerPointCollectionCH6 = new VoltagePointCollectionMultiChannelLaserCailbration();
                    LaserPowerPointCollectionCH7 = new VoltagePointCollectionMultiChannelLaserCailbration();

                    HistoryLaserPowerPointCollectionCH1 = new VoltagePointCollectionMultiChannelLaserCailbration();
                    HistoryLaserPowerPointCollectionCH2 = new VoltagePointCollectionMultiChannelLaserCailbration();
                    HistoryLaserPowerPointCollectionCH3 = new VoltagePointCollectionMultiChannelLaserCailbration();
                    HistoryLaserPowerPointCollectionCH4 = new VoltagePointCollectionMultiChannelLaserCailbration();
                    HistoryLaserPowerPointCollectionCH5 = new VoltagePointCollectionMultiChannelLaserCailbration();
                    HistoryLaserPowerPointCollectionCH6 = new VoltagePointCollectionMultiChannelLaserCailbration();
                    HistoryLaserPowerPointCollectionCH7 = new VoltagePointCollectionMultiChannelLaserCailbration();

                    _WaveView532LaserSubWind = new MultiChannelLaserCalibrationSub();
                    LaserPowerCH1 = new EnumerableDataSource<Point>(LaserPowerPointCollectionCH1);
                    LaserPowerCH1.SetXMapping(x => x.X);
                    LaserPowerCH1.SetYMapping(y => y.Y);

                    LaserPowerCH2 = new EnumerableDataSource<Point>(LaserPowerPointCollectionCH2);
                    LaserPowerCH2.SetXMapping(x => x.X);
                    LaserPowerCH2.SetYMapping(y => y.Y);

                    LaserPowerCH3 = new EnumerableDataSource<Point>(LaserPowerPointCollectionCH3);
                    LaserPowerCH3.SetXMapping(x => x.X);
                    LaserPowerCH3.SetYMapping(y => y.Y);

                    LaserPowerCH4 = new EnumerableDataSource<Point>(LaserPowerPointCollectionCH4);
                    LaserPowerCH4.SetXMapping(x => x.X);
                    LaserPowerCH4.SetYMapping(y => y.Y);

                    LaserPowerCH5 = new EnumerableDataSource<Point>(LaserPowerPointCollectionCH5);
                    LaserPowerCH5.SetXMapping(x => x.X);
                    LaserPowerCH5.SetYMapping(y => y.Y);

                    LaserPowerCH6 = new EnumerableDataSource<Point>(LaserPowerPointCollectionCH6);
                    LaserPowerCH6.SetXMapping(x => x.X);
                    LaserPowerCH6.SetYMapping(y => y.Y);

                    LaserPowerCH7 = new EnumerableDataSource<Point>(LaserPowerPointCollectionCH7);
                    LaserPowerCH7.SetXMapping(x => x.X);
                    LaserPowerCH7.SetYMapping(y => y.Y);


                    HistoryLaserPowerCH1 = new EnumerableDataSource<Point>(HistoryLaserPowerPointCollectionCH1);
                    HistoryLaserPowerCH1.SetXMapping(x => x.X);
                    HistoryLaserPowerCH1.SetYMapping(y => y.Y);

                    HistoryLaserPowerCH2 = new EnumerableDataSource<Point>(HistoryLaserPowerPointCollectionCH2);
                    HistoryLaserPowerCH2.SetXMapping(x => x.X);
                    HistoryLaserPowerCH2.SetYMapping(y => y.Y);

                    HistoryLaserPowerCH3 = new EnumerableDataSource<Point>(HistoryLaserPowerPointCollectionCH3);
                    HistoryLaserPowerCH3.SetXMapping(x => x.X);
                    HistoryLaserPowerCH3.SetYMapping(y => y.Y);

                    HistoryLaserPowerCH4 = new EnumerableDataSource<Point>(HistoryLaserPowerPointCollectionCH4);
                    HistoryLaserPowerCH4.SetXMapping(x => x.X);
                    HistoryLaserPowerCH4.SetYMapping(y => y.Y);

                    HistoryLaserPowerCH5 = new EnumerableDataSource<Point>(HistoryLaserPowerPointCollectionCH5);
                    HistoryLaserPowerCH5.SetXMapping(x => x.X);
                    HistoryLaserPowerCH5.SetYMapping(y => y.Y);

                    HistoryLaserPowerCH6 = new EnumerableDataSource<Point>(HistoryLaserPowerPointCollectionCH6);
                    HistoryLaserPowerCH6.SetXMapping(x => x.X);
                    HistoryLaserPowerCH6.SetYMapping(y => y.Y);

                    HistoryLaserPowerCH7 = new EnumerableDataSource<Point>(HistoryLaserPowerPointCollectionCH7);
                    HistoryLaserPowerCH7.SetXMapping(x => x.X);
                    HistoryLaserPowerCH7.SetYMapping(y => y.Y);

                    DataProcessThread = new Thread(DataProcessThreadMetoh);
                    DataProcessThread.Priority = ThreadPriority.Highest;
                    DataProcessThread.IsBackground = true;
                    DataProcessThread.Start();

                    if (AbnormalTimer == null)
                    {
                        OnTimeInit(1);
                        AbnormalTimer.Start();
                    }
                    _WaveView532LaserSubWind.Title = Workspace.This.Owner.Title + "-Multi Laser Model Kit";
                    _WaveView532LaserSubWind.ShowDialog();
                    _WaveView532LaserSubWind = null;


                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }
        }
        private void _Laser532ModelViewPort_Instruct485UpdateCommOutput()
        {
            GetInstruct485 = _Laser532ModelViewPort.GetByte;
        }
        private void _Laser532ModelViewPort_InteriorPDUpdateCommOutputCH7()
        {
            InteriorPDValue = _Laser532ModelViewPort.PDVoltageCH7;
            InteriorPDValueCH7 = _Laser532ModelViewPort.PDVoltageCH7;
            PD532Value = _Laser532ModelViewPort.PDVoltageCH7;
            PD532ValueCH7 = _Laser532ModelViewPort.PDVoltageCH7;
        }

        private void _Laser532ModelViewPort_InteriorPDUpdateCommOutputCH6()
        {
            InteriorPDValue = _Laser532ModelViewPort.PDVoltageCH6;
            InteriorPDValueCH6 = _Laser532ModelViewPort.PDVoltageCH6;
            PD532Value = _Laser532ModelViewPort.PDVoltageCH6;
            PD532ValueCH6 = _Laser532ModelViewPort.PDVoltageCH6;
        }

        private void _Laser532ModelViewPort_InteriorPDUpdateCommOutputCH5()
        {
            InteriorPDValue = _Laser532ModelViewPort.PDVoltageCH5;
            InteriorPDValueCH5 = _Laser532ModelViewPort.PDVoltageCH5;
            PD532Value = _Laser532ModelViewPort.PDVoltageCH5;
            PD532ValueCH5 = _Laser532ModelViewPort.PDVoltageCH5;
        }

        private void _Laser532ModelViewPort_InteriorPDUpdateCommOutputCH4()
        {
            InteriorPDValue = _Laser532ModelViewPort.PDVoltageCH4;
            InteriorPDValueCH4 = _Laser532ModelViewPort.PDVoltageCH4;
            PD532Value = _Laser532ModelViewPort.PDVoltageCH4;
            PD532ValueCH4 = _Laser532ModelViewPort.PDVoltageCH4;
        }

        private void _Laser532ModelViewPort_InteriorPDUpdateCommOutputCH3()
        {
            InteriorPDValue = _Laser532ModelViewPort.PDVoltageCH3;
            InteriorPDValueCH3 = _Laser532ModelViewPort.PDVoltageCH3;
            PD532Value = _Laser532ModelViewPort.PDVoltageCH3;
            PD532ValueCH3 = _Laser532ModelViewPort.PDVoltageCH3;
        }

        private void _Laser532ModelViewPort_InteriorPDUpdateCommOutputCH2()
        {
            InteriorPDValue = _Laser532ModelViewPort.PDVoltageCH2;
            InteriorPDValueCH2 = _Laser532ModelViewPort.PDVoltageCH2;
            PD532Value = _Laser532ModelViewPort.PDVoltageCH2;
            PD532ValueCH2 = _Laser532ModelViewPort.PDVoltageCH2;
        }

        private void _Laser532ModelViewPort_InteriorPDUpdateCommOutputCH1()
        {
            InteriorPDValue = _Laser532ModelViewPort.PDVoltageCH1;
            InteriorPDValueCH1 = _Laser532ModelViewPort.PDVoltageCH1;
            PD532Value = _Laser532ModelViewPort.PDVoltageCH1;
            PD532ValueCH1 = _Laser532ModelViewPort.PDVoltageCH1;
        }

        private void _Laser532ModelViewPort_LaserOpticalPowerUpdateCommOutputCH1()
        {
            LaserOpticalPowerValue = _Laser532ModelViewPort.LaserPowerValueCH1;
            LaserOpticalPowerValueCH1 = _Laser532ModelViewPort.LaserPowerValueCH1;
        }
        private void _Laser532ModelViewPort_LaserOpticalPowerUpdateCommOutputCH2()
        {
            LaserOpticalPowerValue = _Laser532ModelViewPort.LaserPowerValueCH2;
            LaserOpticalPowerValueCH2 = _Laser532ModelViewPort.LaserPowerValueCH2;
        }
        private void _Laser532ModelViewPort_LaserOpticalPowerUpdateCommOutputCH3()
        {
            LaserOpticalPowerValue = _Laser532ModelViewPort.LaserPowerValueCH3;
            LaserOpticalPowerValueCH3 = _Laser532ModelViewPort.LaserPowerValueCH3;
        }
        private void _Laser532ModelViewPort_LaserOpticalPowerUpdateCommOutputCH4()
        {
            LaserOpticalPowerValue = _Laser532ModelViewPort.LaserPowerValueCH4;
            LaserOpticalPowerValueCH4 = _Laser532ModelViewPort.LaserPowerValueCH4;
        }
        private void _Laser532ModelViewPort_LaserOpticalPowerUpdateCommOutputCH5()
        {
            LaserOpticalPowerValue = _Laser532ModelViewPort.LaserPowerValueCH5;
            LaserOpticalPowerValueCH5 = _Laser532ModelViewPort.LaserPowerValueCH5;
        }
        private void _Laser532ModelViewPort_LaserOpticalPowerUpdateCommOutputCH6()
        {
            LaserOpticalPowerValue = _Laser532ModelViewPort.LaserPowerValueCH6;
            LaserOpticalPowerValueCH6 = _Laser532ModelViewPort.LaserPowerValueCH6;
        }
        private void _Laser532ModelViewPort_LaserOpticalPowerUpdateCommOutputCH7()
        {
            LaserOpticalPowerValue = _Laser532ModelViewPort.LaserPowerValueCH7;
            LaserOpticalPowerValueCH7 = _Laser532ModelViewPort.LaserPowerValueCH7;
        }

        private void _Laser532ModelViewPort_PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH1()
        {
            PhotodiodeVoltageCorrespondingToLaserPowerValue = _Laser532ModelViewPort.PhotodiodeVoltageCorrespondingToLaserPowerCH1;
            PhotodiodeVoltageCorrespondingToLaserPowerValueCH1 = _Laser532ModelViewPort.PhotodiodeVoltageCorrespondingToLaserPowerCH1;
        }
        private void _Laser532ModelViewPort_PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH2()
        {
            PhotodiodeVoltageCorrespondingToLaserPowerValue = _Laser532ModelViewPort.PhotodiodeVoltageCorrespondingToLaserPowerCH2;
            PhotodiodeVoltageCorrespondingToLaserPowerValueCH2 = _Laser532ModelViewPort.PhotodiodeVoltageCorrespondingToLaserPowerCH2;
        }
        private void _Laser532ModelViewPort_PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH3()
        {
            PhotodiodeVoltageCorrespondingToLaserPowerValue = _Laser532ModelViewPort.PhotodiodeVoltageCorrespondingToLaserPowerCH3;
            PhotodiodeVoltageCorrespondingToLaserPowerValueCH3 = _Laser532ModelViewPort.PhotodiodeVoltageCorrespondingToLaserPowerCH3;
        }
        private void _Laser532ModelViewPort_PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH4()
        {
            PhotodiodeVoltageCorrespondingToLaserPowerValue = _Laser532ModelViewPort.PhotodiodeVoltageCorrespondingToLaserPowerCH4;
            PhotodiodeVoltageCorrespondingToLaserPowerValueCH4 = _Laser532ModelViewPort.PhotodiodeVoltageCorrespondingToLaserPowerCH4;
        }
        private void _Laser532ModelViewPort_PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH5()
        {
            PhotodiodeVoltageCorrespondingToLaserPowerValue = _Laser532ModelViewPort.PhotodiodeVoltageCorrespondingToLaserPowerCH5;
            PhotodiodeVoltageCorrespondingToLaserPowerValueCH5 = _Laser532ModelViewPort.PhotodiodeVoltageCorrespondingToLaserPowerCH5;
        }
        private void _Laser532ModelViewPort_PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH6()
        {
            PhotodiodeVoltageCorrespondingToLaserPowerValue = _Laser532ModelViewPort.PhotodiodeVoltageCorrespondingToLaserPowerCH6;
            PhotodiodeVoltageCorrespondingToLaserPowerValueCH6 = _Laser532ModelViewPort.PhotodiodeVoltageCorrespondingToLaserPowerCH6;
        }
        private void _Laser532ModelViewPort_PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutputCH7()
        {
            PhotodiodeVoltageCorrespondingToLaserPowerValue = _Laser532ModelViewPort.PhotodiodeVoltageCorrespondingToLaserPowerCH7;
            PhotodiodeVoltageCorrespondingToLaserPowerValueCH7 = _Laser532ModelViewPort.PhotodiodeVoltageCorrespondingToLaserPowerCH7;
        }

        private void _Laser532ModelViewPort_LaserElectricUpdateCommOutputCH1()
        {
            LaserElectricValue = _Laser532ModelViewPort.LaserElectricValueCH1;
            LaserElectricValueCH1 = _Laser532ModelViewPort.LaserElectricValueCH1;
        }
        private void _Laser532ModelViewPort_LaserElectricUpdateCommOutputCH2()
        {
            LaserElectricValue = _Laser532ModelViewPort.LaserElectricValueCH2;
            LaserElectricValueCH2 = _Laser532ModelViewPort.LaserElectricValueCH2;
        }
        private void _Laser532ModelViewPort_LaserElectricUpdateCommOutputCH3()
        {
            LaserElectricValue = _Laser532ModelViewPort.LaserElectricValueCH3;
            LaserElectricValueCH3 = _Laser532ModelViewPort.LaserElectricValueCH3;
        }
        private void _Laser532ModelViewPort_LaserElectricUpdateCommOutputCH4()
        {
            LaserElectricValue = _Laser532ModelViewPort.LaserElectricValueCH4;
            LaserElectricValueCH4 = _Laser532ModelViewPort.LaserElectricValueCH4;
        }
        private void _Laser532ModelViewPort_LaserElectricUpdateCommOutputCH5()
        {
            LaserElectricValue = _Laser532ModelViewPort.LaserElectricValueCH5;
            LaserElectricValueCH5 = _Laser532ModelViewPort.LaserElectricValueCH5;
        }
        private void _Laser532ModelViewPort_LaserElectricUpdateCommOutputCH6()
        {
            LaserElectricValue = _Laser532ModelViewPort.LaserElectricValueCH6;
            LaserElectricValueCH6 = _Laser532ModelViewPort.LaserElectricValueCH6;
        }
        private void _Laser532ModelViewPort_LaserElectricUpdateCommOutputCH7()
        {
            LaserElectricValue = _Laser532ModelViewPort.LaserElectricValueCH7;
            LaserElectricValueCH7 = _Laser532ModelViewPort.LaserElectricValueCH7;
        }


        private void _Laser532ModelViewPort_LaserPowerUpdateCommOutputCH1()
        {
            GetCurrentTecTempValue = _Laser532ModelViewPort.TecTemperValueCH1 * 0.1;
            GetCurrentTecTempValueCH1 = _Laser532ModelViewPort.TecTemperValueCH1 * 0.1;
        }
        private void _Laser532ModelViewPort_LaserPowerUpdateCommOutputCH2()
        {
            GetCurrentTecTempValue = _Laser532ModelViewPort.TecTemperValueCH2 * 0.1;
            GetCurrentTecTempValueCH2 = _Laser532ModelViewPort.TecTemperValueCH2 * 0.1;
        }
        private void _Laser532ModelViewPort_LaserPowerUpdateCommOutputCH3()
        {
            GetCurrentTecTempValue = _Laser532ModelViewPort.TecTemperValueCH3 * 0.1;
            GetCurrentTecTempValueCH3 = _Laser532ModelViewPort.TecTemperValueCH3 * 0.1;
        }
        private void _Laser532ModelViewPort_LaserPowerUpdateCommOutputCH4()
        {
            GetCurrentTecTempValue = _Laser532ModelViewPort.TecTemperValueCH4 * 0.1;
            GetCurrentTecTempValueCH4 = _Laser532ModelViewPort.TecTemperValueCH4 * 0.1;
        }
        private void _Laser532ModelViewPort_LaserPowerUpdateCommOutputCH5()
        {
            GetCurrentTecTempValue = _Laser532ModelViewPort.TecTemperValueCH5 * 0.1;
            GetCurrentTecTempValueCH5 = _Laser532ModelViewPort.TecTemperValueCH5 * 0.1;
        }
        private void _Laser532ModelViewPort_LaserPowerUpdateCommOutputCH6()
        {
            GetCurrentTecTempValue = _Laser532ModelViewPort.TecTemperValueCH6 * 0.1;
            GetCurrentTecTempValueCH6 = _Laser532ModelViewPort.TecTemperValueCH6 * 0.1;
        }
        private void _Laser532ModelViewPort_LaserPowerUpdateCommOutputCH7()
        {
            GetCurrentTecTempValue = _Laser532ModelViewPort.TecTemperValueCH7 * 0.1;
            GetCurrentTecTempValueCH7 = _Laser532ModelViewPort.TecTemperValueCH7 * 0.1;
        }

        #region Data532WaveCommand
        public ICommand Data532WaveCommand
        {
            get
            {
                if (_Data532WaveCommand == null)
                {
                    _Data532WaveCommand = new RelayCommand(ExecuteData532WaveCommand, CanExecuteData532WaveCommand);
                }
                return _Data532WaveCommand;
            }
        }
        public void ExecuteData532WaveCommand(object parameter)
        {
            try
            {
                //HistoryLaserPowerPointCollectionCH1.Clear();
                //HistoryLaserPowerPointCollectionCH2.Clear();
                //HistoryLaserPowerPointCollectionCH3.Clear();
                //HistoryLaserPowerPointCollectionCH4.Clear();
                //HistoryLaserPowerPointCollectionCH5.Clear();
                //HistoryLaserPowerPointCollectionCH6.Clear();
                //HistoryLaserPowerPointCollectionCH7.Clear();
                string fName;
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "csv文件|*.csv|所有文件|*.*";
                openFileDialog.RestoreDirectory = true;
                openFileDialog.FilterIndex = 1;
                if (openFileDialog.ShowDialog() == true)
                {
                    fName = openFileDialog.FileName;
                    string strData = File.ReadAllText(fName);
                    if (string.IsNullOrEmpty(strData))
                    {
                        MessageBox.Show("无效的数据");
                        return;
                    }
                    HistoryThread = new Thread(new ParameterizedThreadStart(HostoryDataDispaly));
                    HistoryThread.IsBackground = true;
                    HistoryThread.Start((object)strData);
                    _Display532LaserData = new DisplayMultiChannelLaserCalibrationData();
                    _Display532LaserData.fileName.Content = fName;
                    _Display532LaserData.Title = Workspace.This.Owner.Title + "-532 History WaveView Kit";
                    _Display532LaserData.Show();
                    //_Display532LaserData = null;

                }
            }
            catch
            {
                MessageBox.Show("文件被占用");

            }
        }

        void HostoryDataDispaly(object message)
        {
            string strData = message.ToString();
            string[] DataValue = strData.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            if (DataValue.Length > 0)
            {
                string[] Data = DataValue[0].Split(new string[] { "," }, StringSplitOptions.None);
                if (Data.Length > 0)
                {
                    Thread.Sleep(50);
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        if (Data.Length == 1)
                        {
                            HistoryLaserPowerPointCollectionCH1.Clear();
                            _Display532LaserData.TabCH1.Focus();
                        }
                        else if (Data.Length == 2)
                        {
                            HistoryLaserPowerPointCollectionCH2.Clear();
                            _Display532LaserData.TabCH2.Focus();
                        }
                        else if (Data.Length == 3)
                        {
                            HistoryLaserPowerPointCollectionCH3.Clear();
                            _Display532LaserData.TabCH3.Focus();
                        }
                        else if (Data.Length == 4)
                        {
                            HistoryLaserPowerPointCollectionCH4.Clear();
                            _Display532LaserData.TabCH4.Focus();
                        }
                        else if (Data.Length == 5)
                        {
                            HistoryLaserPowerPointCollectionCH5.Clear();
                            _Display532LaserData.TabCH5.Focus();
                        }
                        else if (Data.Length == 6)
                        {
                            HistoryLaserPowerPointCollectionCH6.Clear();
                            _Display532LaserData.TabCH6.Focus();

                        }
                        else if (Data.Length == 7)
                        {
                            HistoryLaserPowerPointCollectionCH7.Clear();
                            _Display532LaserData.TabCH7.Focus();
                        }
                    });

                }
            }
            for (int h = 0; h < DataValue.Length-1; h++)
            {
                string[] Data = DataValue[h].Split(new string[] { "," }, StringSplitOptions.None);
                if (Data.Length > 0)
                {
                    Thread.Sleep(50);
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        if (Data.Length == 1)
                        {
                            double laserPowerCH1 = Convert.ToDouble(Data[0]);
                            Point LaserPowerPointCH1 = new Point(h, laserPowerCH1);
                            HistoryLaserPowerPointCollectionCH1.Add(LaserPowerPointCH1);
                        }
                        else if (Data.Length == 2)
                        {
                            double laserPowerCH2 = Convert.ToDouble(Data[1]);
                            Point LaserPowerPointCH2 = new Point(h, laserPowerCH2);
                            HistoryLaserPowerPointCollectionCH2.Add(LaserPowerPointCH2);
                        }
                        else if (Data.Length == 3)
                        {
                            double laserPowerCH3 = Convert.ToDouble(Data[2]);
                            Point LaserPowerPointCH3 = new Point(h, laserPowerCH3);
                            HistoryLaserPowerPointCollectionCH3.Add(LaserPowerPointCH3);
                        }
                        else if (Data.Length == 4)
                        {
                            double laserPowerCH4 = Convert.ToDouble(Data[3]);
                            Point LaserPowerPointCH4 = new Point(h, laserPowerCH4);
                            HistoryLaserPowerPointCollectionCH4.Add(LaserPowerPointCH4);
                        }
                        else if (Data.Length == 5)
                        {
                            double laserPowerCH5 = Convert.ToDouble(Data[4]);
                            Point LaserPowerPointCH5 = new Point(h, laserPowerCH5);
                            HistoryLaserPowerPointCollectionCH5.Add(LaserPowerPointCH5);
                        }
                        else if (Data.Length == 6)
                        {
                            double laserPowerCH6 = Convert.ToDouble(Data[5]);
                            Point LaserPowerPointCH6 = new Point(h, laserPowerCH6);
                            HistoryLaserPowerPointCollectionCH6.Add(LaserPowerPointCH6);

                        }
                        else if (Data.Length == 7)
                        {
                            double laserPowerCH7 = Convert.ToDouble(Data[6]);
                            Point LaserPowerPointCH7 = new Point(h, laserPowerCH7);
                            HistoryLaserPowerPointCollectionCH7.Add(LaserPowerPointCH7);
                        }
                    });

                }
            }
        }

        public bool CanExecuteData532WaveCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region Out532WaveCommand
        public ICommand Out532WaveCommand
        {
            get
            {
                if (_Out532WaveCommand == null)
                {
                    _Out532WaveCommand = new RelayCommand(ExecuteOut532WaveCommand, CanExecuteOut532WaveCommand);
                }
                return _Out532WaveCommand;
            }
        }
        string time = "";
        public void ExecuteOut532WaveCommand(object parameter)
        {
            time = StrDate();
            path = Workspace.This.GetManualFolder();
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            int count = 0;
            if (CH1Check)
            {
                count = LaserPowerPointCollectionCH1.Count;
                string[] testdata = new string[count];
                for (int y = 0; y < count; y++)
                {
                    testdata[y] = LaserPowerPointCollectionCH1[y].Y.ToString();          
                }
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);//创建新路径
                }
                File.WriteAllLines(path + LaserNumber + "_CH1_"+CH1Txt+"_" + "658_" + time + ".csv", testdata, System.Text.Encoding.UTF8);
            }
            if (CH2Check)
            {
                count = LaserPowerPointCollectionCH2.Count;
                string[] testdata = new string[count];
                for (int y = 0; y < count; y++)
                {
                    testdata[y] = ","+LaserPowerPointCollectionCH2[y].Y.ToString();
                }
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);//创建新路径
                }
                File.WriteAllLines(path + LaserNumber + "_CH2_" +  CH2Txt + "_" + "685_" + time + ".csv", testdata, System.Text.Encoding.UTF8);
            }
            if (CH3Check)
            {
                count = LaserPowerPointCollectionCH3.Count;
                string[] testdata = new string[count];
                for (int y = 0; y < count; y++)
                {
                    testdata[y] = "," + "," + LaserPowerPointCollectionCH3[y].Y.ToString();
                }
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);//创建新路径
                }
                File.WriteAllLines(path + LaserNumber + "_CH3_" + CH3Txt + "_" + "784_" + time + ".csv", testdata, System.Text.Encoding.UTF8);
            }
            if (CH4Check)
            {
                count = LaserPowerPointCollectionCH4.Count;
                string[] testdata = new string[count];
                for (int y = 0; y < count; y++)
                {
                    testdata[y] = "," + "," + "," + LaserPowerPointCollectionCH4[y].Y.ToString();
                }
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);//创建新路径
                }
                File.WriteAllLines(path + LaserNumber + "_CH4_" +  CH4Txt + "_" + "488_" + time + ".csv", testdata, System.Text.Encoding.UTF8);
            }
            if (CH5Check)
            {
                count = LaserPowerPointCollectionCH5.Count;
                string[] testdata = new string[count];
                for (int y = 0; y < count; y++)
                {
                    testdata[y] = "," + "," + "," + "," + LaserPowerPointCollectionCH5[y].Y.ToString();
                }
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);//创建新路径
                }
                File.WriteAllLines(path + LaserNumber + "_CH5_" +  CH5Txt + "_" + "638_" + time + ".csv", testdata, System.Text.Encoding.UTF8);
            }
            if (CH6Check)
            {
                count = LaserPowerPointCollectionCH6.Count;
                string[] testdata = new string[count];
                for (int y = 0; y < count; y++)
                {
                    testdata[y] = "," + "," + "," + "," + "," + LaserPowerPointCollectionCH6[y].Y.ToString();
                }
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);//创建新路径
                }
                File.WriteAllLines(path + LaserNumber + "_CH6_" + CH6Txt + "_" + "450_" + time + ".csv", testdata, System.Text.Encoding.UTF8);
            }
            if (CH7Check)
            {
                count = LaserPowerPointCollectionCH7.Count;
                string[] testdata = new string[count];
                for (int y = 0; y < count; y++)
                {
                    testdata[y] = "," + "," + "," + "," + "," + "," + LaserPowerPointCollectionCH7[y].Y.ToString();
                }
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);//创建新路径
                }
                File.WriteAllLines(path + LaserNumber + "_CH7_" + CH7Txt + "_" + "730_" + time + ".csv", testdata, System.Text.Encoding.UTF8);
            }
            bool isPngSave=_WaveView532LaserSubWind.SavePng(path + LaserNumber,time + ".png");
            Thread _temp = new Thread(newProcess);
            _temp.IsBackground = true;
            if(isPngSave)
               _temp.Start();


        }
        void newProcess()
        {
            MessageBox.Show("导出成功\n");
            //System.Diagnostics.Process.Start("explorer.exe", path);
        }
        private string StrDate()
        {
            System.DateTime currentTime = DateTime.Now;//获取当前系统时间
            int month = currentTime.Month;
            int day = currentTime.Day;
            int hour = currentTime.Hour;
            int minute = currentTime.Minute;
            string newDay = "";
            string newMonth = "";
            if (day.ToString().Length == 1)
            {
                newDay = "0" + day.ToString();
            }
            else
            {
                newDay = day.ToString();
            }
            if (month.ToString().Length == 1)
            {
                newMonth = "0" + month.ToString();
            }
            else
            {
                newMonth = month.ToString();
            }
            string timeNow = newMonth + newDay + hour.ToString() + minute.ToString();
            return timeNow;
        }
        public bool CanExecuteOut532WaveCommand(object parameter)
        {
            return true;
        }
        #endregion

        #region StartWaveCommand
        private bool isSettingClose;
        public ICommand StartWaveCommand
        {
            get
            {
                if (_StartWaveCommand == null)
                {
                    _StartWaveCommand = new RelayCommand(ExecuteStartWaveCommand, CanExecuteStartWaveCommand);
                }
                return _StartWaveCommand;
            }
        }
        public void ExecuteStartWaveCommand(object parameter)
        {
            if (!_Laser532ModelViewPort.IsConnected)
            {
                MessageBox.Show("请先选择端口");
                return;
            }
            isStartButton++;
            index = 0;
            IsStart = false;
            if (isStartButton == 1)
            {
                try
                {
                    PeriodIndex = 0;
                    CalList.Clear();
                    LaserPowerPointCollectionCH1.Clear();
                    LaserPowerPointCollectionCH2.Clear();
                    LaserPowerPointCollectionCH3.Clear();
                    LaserPowerPointCollectionCH4.Clear();
                    LaserPowerPointCollectionCH5.Clear();
                    LaserPowerPointCollectionCH6.Clear();
                    LaserPowerPointCollectionCH7.Clear();
                    MutiChannelLaserCalibrationSetting setting = new MutiChannelLaserCalibrationSetting();
                    setting.ShowDialog();
                    if (!IsSettingClose)
                    {
                        isStartButton = 0;
                        return;
                    }
                    if (!CH1Check && !CH2Check && !CH3Check && !CH4Check && !CH5Check && !CH6Check && !CH7Check)
                    {
                        isStartButton = 0;
                        return;
                    }
                    _WaveView532LaserSubWind.charttt.TabCH1.Header = "CH1";
                    _WaveView532LaserSubWind.charttt.TabCH2.Header = "CH2";
                    _WaveView532LaserSubWind.charttt.TabCH3.Header = "CH3";
                    _WaveView532LaserSubWind.charttt.TabCH4.Header = "CH4";
                    _WaveView532LaserSubWind.charttt.TabCH5.Header = "CH5";
                    if (CH1Check)
                    {
                        if (string.IsNullOrEmpty(CH1Txt))
                        {
                            MessageBox.Show("CH1 没有写波长");
                            isStartButton = 0;
                            return;
                        }
                        _WaveView532LaserSubWind.charttt.TabCH1.Focus();
                        _WaveView532LaserSubWind.charttt.TabCH1.Header += "-" + CH1Txt;
                    }
                    if (CH2Check)
                    {
                        if (string.IsNullOrEmpty(CH2Txt))
                        {
                            MessageBox.Show("CH2 没有写波长");
                            isStartButton = 0;
                            return;
                        }
                        _WaveView532LaserSubWind.charttt.TabCH2.Focus();
                        _WaveView532LaserSubWind.charttt.TabCH2.Header += "-" + CH2Txt;
                    }
                    if (CH3Check)
                    {
                        if (string.IsNullOrEmpty(CH3Txt))
                        {
                            MessageBox.Show("CH3 没有写波长");
                            isStartButton = 0;
                            return;
                        }
                        _WaveView532LaserSubWind.charttt.TabCH3.Focus();
                        _WaveView532LaserSubWind.charttt.TabCH3.Header += "-" + CH3Txt;
                    }
                    if (CH4Check)
                    {
                        if (string.IsNullOrEmpty(CH4Txt))
                        {
                            MessageBox.Show("CH4 没有写波长");
                            isStartButton = 0;
                            return;
                        }
                        _WaveView532LaserSubWind.charttt.TabCH4.Focus();
                        _WaveView532LaserSubWind.charttt.TabCH4.Header += "-" + CH4Txt;
                    }
                    if (CH5Check)
                    {
                        if (string.IsNullOrEmpty(CH5Txt))
                        {
                            MessageBox.Show("CH5 没有写波长");
                            isStartButton = 0;
                            return;
                        }
                        _WaveView532LaserSubWind.charttt.TabCH5.Focus();
                        _WaveView532LaserSubWind.charttt.TabCH5.Header += "-" + CH5Txt;
                    }
                    if (CH6Check)
                    {
                        if (string.IsNullOrEmpty(CH6Txt))
                        {
                            MessageBox.Show("CH6 没有写波长");
                            isStartButton = 0;
                            return;
                        }
                        _WaveView532LaserSubWind.charttt.TabCH6.Focus();
                        _WaveView532LaserSubWind.charttt.TabCH6.Header += "-" + CH6Txt;
                    }
                    if (CH7Check)
                    {
                        if (string.IsNullOrEmpty(CH7Txt))
                        {
                            MessageBox.Show("CH7 没有写波长");
                            isStartButton = 0;
                            return;
                        }
                        _WaveView532LaserSubWind.charttt.TabCH7.Focus();
                        _WaveView532LaserSubWind.charttt.TabCH7.Header += "-" + CH7Txt;
                    }
                    _WaveView532LaserSubWind.Fit();
                    string fName;
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    openFileDialog.Filter = "csv文件|*.csv|所有文件|*.*";
                    openFileDialog.RestoreDirectory = true;
                    openFileDialog.FilterIndex = 1;
                    if (openFileDialog.ShowDialog() == true)
                    {
                        fName = openFileDialog.FileName;
                        string strData = File.ReadAllText(fName);
                        if (string.IsNullOrEmpty(strData))
                        {
                            isStartButton = 0;
                            MessageBox.Show("无效的数据");
                            return;
                        }
                        string[] DataValue = strData.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        List<double> listCH0 = new List<double>();
                        List<double> listCH1 = new List<double>();
                        List<double> listCH2 = new List<double>();
                        List<double> listCH3 = new List<double>();
                        List<double> listCH4 = new List<double>();
                        List<double> listCH5 = new List<double>();
                        List<double> listCH6 = new List<double>();
                        List<double> listTime = new List<double>();
                        for (int h = 0; h < DataValue.Length; h++)
                        {
                            string[] Data = DataValue[h].Split(new string[] { "," }, StringSplitOptions.None);
                            if (Data.Length > 1)
                            {
                                if (Data.Length < 5)
                                {
                                    isStartButton = 0;
                                    MessageBox.Show("请选择多通道配置文件");
                                    return;
                                }
                                string sPowerCH0 = Data[0];
                                string sPowerCH1 = Data[1];
                                string sPowerCH2 = Data[2];
                                string sPowerCH3 = Data[3];
                                string sPowerCH4 = Data[4];
                                string sPowerCH5 = Data[5];
                                string sPowerCH6 = Data[6];
                                string sTime = Data[7];
                                if (string.IsNullOrEmpty(sPowerCH0))
                                {
                                    sPowerCH0 = "0";
                                }
                                if (string.IsNullOrEmpty(sPowerCH1))
                                {
                                    sPowerCH1 = "0";
                                }
                                if (string.IsNullOrEmpty(sPowerCH2))
                                {
                                    sPowerCH2 = "0";
                                }
                                if (string.IsNullOrEmpty(sPowerCH3))
                                {
                                    sPowerCH3 = "0";
                                }
                                if (string.IsNullOrEmpty(sPowerCH4))
                                {
                                    sPowerCH4 = "0";
                                }
                                if (string.IsNullOrEmpty(sPowerCH5))
                                {
                                    sPowerCH5 = "0";
                                }
                                if (string.IsNullOrEmpty(sPowerCH6))
                                {
                                    sPowerCH6 = "0";
                                }
                                double f2;
                                if (!double.TryParse(sPowerCH0, out f2))
                                {
                                    continue;
                                }
                                double powerCH0 = Convert.ToDouble(sPowerCH0);
                                double powerCH1 = Convert.ToDouble(sPowerCH1);
                                double powerCH2 = Convert.ToDouble(sPowerCH2);
                                double powerCH3 = Convert.ToDouble(sPowerCH3);
                                double powerCH4 = Convert.ToDouble(sPowerCH4);
                                double powerCH5 = Convert.ToDouble(sPowerCH5);
                                double powerCH6 = Convert.ToDouble(sPowerCH6);
                                double time = Convert.ToDouble(sTime);

                                //Dictionary<double, double> caldata = new Dictionary<double, double>();
                                listCH0.Add(powerCH0);
                                listCH1.Add(powerCH1);
                                listCH2.Add(powerCH2);
                                listCH3.Add(powerCH3);
                                listCH4.Add(powerCH4);
                                listCH5.Add(powerCH5);
                                listCH6.Add(powerCH6);
                                listTime.Add(time);
                            }
                        }
                        CalList.Add(listTime);
                        CalList.Add(listCH0);
                        CalList.Add(listCH1);
                        CalList.Add(listCH2);
                        CalList.Add(listCH3);
                        CalList.Add(listCH4);
                        CalList.Add(listCH5);
                        CalList.Add(listCH6);
                    }
                    else
                    {
                        isStartButton = 0;
                        MessageBox.Show("缺少532校准.csv文件");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    isStartButton = 0;
                    MessageBox.Show("文件被占用");
                    return;
                }
                LaserPowerPointCollectionCH1.Clear();
                LaserPowerPointCollectionCH2.Clear();
                LaserPowerPointCollectionCH3.Clear();
                LaserPowerPointCollectionCH4.Clear();
                LaserPowerPointCollectionCH5.Clear();
                LaserPowerPointCollectionCH6.Clear();
                LaserPowerPointCollectionCH7.Clear();

                _WaveView532LaserSubWind.jiguangWaveButton55.IsEnabled = false;
                _WaveView532LaserSubWind.jiguangWave1Button11.IsEnabled = false;
                _WaveView532LaserSubWind.jiguangWaveButton2.IsEnabled = false;
                _WaveView532LaserSubWind.jiguangWave1Button2.IsEnabled = false;
                _WaveView532LaserSubWind.jiguangWaveButton.IsEnabled = false;
                _WaveView532LaserSubWind.jiguangWave1Button.IsEnabled = false;
                _WaveView532LaserSubWind.jiguangWaveButton1.IsEnabled = false;
                _WaveView532LaserSubWind.jiguangWave1Button1.IsEnabled = false;
                _WaveView532LaserSubWind.jiguangWaveButton3.IsEnabled = false;
                _WaveView532LaserSubWind.jiguangWave1Button3.IsEnabled = false;
                _WaveView532LaserSubWind.COMNumberCoeffComboBox1.IsEnabled = false;
                _WaveView532LaserSubWind.jiguangWaveButton4.IsEnabled = false;
                _WaveView532LaserSubWind.jiguangWave1Button4.IsEnabled = false;
                IsStart = true;
                _WaveView532LaserSubWind.COMNumberCoeffComboBox.IsEnabled = false;
                _WaveView532LaserSubWind.StartWaveButton.Content = "停止";
                _WaveView532LaserSubWind.StartWaveButton.Foreground = Brushes.Red;

            }
            else
            {
                _WaveView532LaserSubWind.jiguangWaveButton55.IsEnabled = true;
                _WaveView532LaserSubWind.jiguangWave1Button11.IsEnabled = true;
                _WaveView532LaserSubWind.jiguangWaveButton2.IsEnabled = true;
                _WaveView532LaserSubWind.jiguangWave1Button2.IsEnabled = true;
                _WaveView532LaserSubWind.jiguangWaveButton.IsEnabled = true;
                _WaveView532LaserSubWind.jiguangWave1Button.IsEnabled = true;
                _WaveView532LaserSubWind.jiguangWaveButton1.IsEnabled = true;
                _WaveView532LaserSubWind.jiguangWave1Button1.IsEnabled = true;
                _WaveView532LaserSubWind.jiguangWaveButton3.IsEnabled = true;
                _WaveView532LaserSubWind.jiguangWave1Button3.IsEnabled = true;
                _WaveView532LaserSubWind.COMNumberCoeffComboBox1.IsEnabled = true;
                _WaveView532LaserSubWind.jiguangWaveButton4.IsEnabled = true;
                _WaveView532LaserSubWind.jiguangWave1Button4.IsEnabled = true;
                IsStart = false;
                isStartButton = 0;
                PeriodIndex = 10000;//终止当前的扫描循环条件
                _WaveView532LaserSubWind.COMNumberCoeffComboBox.IsEnabled = true;
                _WaveView532LaserSubWind.StartWaveButton.Content = "开始";
                _WaveView532LaserSubWind.StartWaveButton.Foreground = Brushes.White;

            }

        }
        public bool CanExecuteStartWaveCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region WriteInteriorPDCommand
        public ICommand WriteInteriorPDCommand
        {
            get
            {
                if (_WriteInteriorPDCommand == null)
                {
                    _WriteInteriorPDCommand = new RelayCommand(ExecuteWriteInteriorPDCommand, CanExecuteWriteInteriorPDCommand);
                }
                return _WriteInteriorPDCommand;
            }
        }
        public void ExecuteWriteInteriorPDCommand(object parameter)
        {
            if (_Laser532ModelViewPort.IsConnected)
            {
                _Laser532ModelViewPort.SetInternalPDVoltage(InteriorPDValue);
            }

        }
        public bool CanExecuteWriteInteriorPDCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region ReadInteriorPDCommand
        public ICommand ReadInteriorPDCommand
        {
            get
            {
                if (_ReadInteriorPDCommand == null)
                {
                    _ReadInteriorPDCommand = new RelayCommand(ExecuteReadInteriorPDCommand, CanExecuteReadInteriorPDCommand);
                }
                return _ReadInteriorPDCommand;
            }
        }
        public void ExecuteReadInteriorPDCommand(object parameter)
        {
            if (_Laser532ModelViewPort.IsConnected)
            {
                _Laser532ModelViewPort.GetInternalPDVoltage();
            }

        }
        public bool CanExecuteReadInteriorPDCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region WriteLaserOpticalPowerCommand
        public ICommand WriteLaserOpticalPowerCommand
        {
            get
            {
                if (_WriteLaserOpticalPowerCommand == null)
                {
                    _WriteLaserOpticalPowerCommand = new RelayCommand(ExecuteWriteLaserOpticalPowerCommand, CanExecuteWriteLaserOpticalPowerCommand);
                }
                return _WriteLaserOpticalPowerCommand;
            }
        }
        public void ExecuteWriteLaserOpticalPowerCommand(object parameter)
        {
            if (_Laser532ModelViewPort.IsConnected)
            {
                _Laser532ModelViewPort.SetCurrentLaserLightPowerValueValue(LaserOpticalPowerValue);
            }

        }
        public bool CanExecuteWriteLaserOpticalPowerCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region ReadLaserOpticalPowerCommand
        public ICommand ReadLaserOpticalPowerCommand
        {
            get
            {
                if (_ReadLaserOpticalPowerCommand == null)
                {
                    _ReadLaserOpticalPowerCommand = new RelayCommand(ExecuteReadLaserOpticalPowerCommand, CanExecuteReadLaserOpticalPowerCommand);
                }
                return _ReadLaserOpticalPowerCommand;
            }
        }
        public void ExecuteReadLaserOpticalPowerCommand(object parameter)
        {
            if (_Laser532ModelViewPort.IsConnected)
            {
                _Laser532ModelViewPort.GetCurrentLaserLightPowerValueValue();
            }

        }
        public bool CanExecuteReadLaserOpticalPowerCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region WriteLaserCurrentCommand
        public ICommand WriteLaserCurrentCommand
        {
            get
            {
                if (_WriteLaserCurrentCommand == null)
                {
                    _WriteLaserCurrentCommand = new RelayCommand(ExecuteWriteLaserCurrentCommand, CanExecuteWriteLaserCurrentCommand);
                }
                return _WriteLaserCurrentCommand;
            }
        }
        public void ExecuteWriteLaserCurrentCommand(object parameter)
        {
            if (_Laser532ModelViewPort.IsConnected)
            {
                _Laser532ModelViewPort.SetCurrentLaserCurrentValue(LaserElectricValue);
            }

        }
        public bool CanExecuteWriteLaserCurrentCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region ReadLaserCurrentCommand
        public ICommand ReadLaserCurrentCommand
        {
            get
            {
                if (_ReadLaserCurrentCommand == null)
                {
                    _ReadLaserCurrentCommand = new RelayCommand(ExecuteReadLaserCurrentCommand, CanExecuteReadLaserCurrentCommand);
                }
                return _ReadLaserCurrentCommand;
            }
        }
        public void ExecuteReadLaserCurrentCommand(object parameter)
        {
            if (_Laser532ModelViewPort.IsConnected)
            {
                _Laser532ModelViewPort.GetCurrentLaserCurrentValue();
            }

        }
        public bool CanExecuteReadLaserCurrentCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region WritePD532Command
        public ICommand WritePD532Command
        {
            get
            {
                if (_WritePD532Command == null)
                {
                    _WritePD532Command = new RelayCommand(ExecuteWritePD532Command, CanExecuteWritePD532Command);
                }
                return _WritePD532Command;
            }
        }
        public void ExecuteWritePD532Command(object parameter)
        {
            if (_Laser532ModelViewPort.IsConnected)
            {
                _Laser532ModelViewPort.SetPDVoltage(PD532Value);
            }

        }
        public bool CanExecuteWritePD532Command(object parameter)
        {
            return true;
        }

        #endregion

        #region ReadPD532Command
        public ICommand ReadPD532Command
        {
            get
            {
                if (_ReadPD532Command == null)
                {
                    _ReadPD532Command = new RelayCommand(ExecuteReadPD532Command, CanExecuteReadPD532Command);
                }
                return _ReadPD532Command;
            }
        }
        public void ExecuteReadPD532Command(object parameter)
        {
            if (_Laser532ModelViewPort.IsConnected)
            {
                _Laser532ModelViewPort.GetPDVoltage();
            }

        }
        public bool CanExecuteReadPD532Command(object parameter)
        {
            return true;
        }

        #endregion

        #region WritePhotodiodeVoltageCorrespondingToLaserPowerCommand
        public ICommand WritePhotodiodeVoltageCorrespondingToLaserPowerCommand
        {
            get
            {
                if (_WritePhotodiodeVoltageCorrespondingToLaserPowerCommand == null)
                {
                    _WritePhotodiodeVoltageCorrespondingToLaserPowerCommand = new RelayCommand(ExecuteWritePhotodiodeVoltageCorrespondingToLaserPowerCommand, CanExecuteWritePhotodiodeVoltageCorrespondingToLaserPowerCommand);
                }
                return _WritePhotodiodeVoltageCorrespondingToLaserPowerCommand;
            }
        }
        public void ExecuteWritePhotodiodeVoltageCorrespondingToLaserPowerCommand(object parameter)
        {
            if (_Laser532ModelViewPort.IsConnected)
            {
                _Laser532ModelViewPort.SetCurrentPhotodiodeVoltageCorrespondingToLaserPowerValue(SelectedPhotodiodeVoltageCorrespondingToLaserPower, PhotodiodeVoltageCorrespondingToLaserPowerValue);
            }

        }
        public bool CanExecuteWritePhotodiodeVoltageCorrespondingToLaserPowerCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region ReadPhotodiodeVoltageCorrespondingToLaserPowerCommand
        public ICommand ReadPhotodiodeVoltageCorrespondingToLaserPowerCommand
        {
            get
            {
                if (_ReadPhotodiodeVoltageCorrespondingToLaserPowerCommand == null)
                {
                    _ReadPhotodiodeVoltageCorrespondingToLaserPowerCommand = new RelayCommand(ExecuteReadPhotodiodeVoltageCorrespondingToLaserPowerCommand, CanExecuteReadPhotodiodeVoltageCorrespondingToLaserPowerCommand);
                }
                return _ReadPhotodiodeVoltageCorrespondingToLaserPowerCommand;
            }
        }
        public void ExecuteReadPhotodiodeVoltageCorrespondingToLaserPowerCommand(object parameter)
        {
            if (_Laser532ModelViewPort.IsConnected)
            {
                _Laser532ModelViewPort.GetCurrentPhotodiodeVoltageCorrespondingToLaserPowerValue(SelectedPhotodiodeVoltageCorrespondingToLaserPower);
            }

        }
        public bool CanExecuteReadPhotodiodeVoltageCorrespondingToLaserPowerCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region ReadLaserTecTempCommand
        public ICommand ReadLaserTecTempCommand
        {
            get
            {
                if (_ReadLaserTecTempCommand == null)
                {
                    _ReadLaserTecTempCommand = new RelayCommand(ExecuteReadLaserTecTempCommandCommand, CanExecuteReadLaserTecTempCommandCommand);
                }
                return _ReadLaserTecTempCommand;
            }
        }
        public void ExecuteReadLaserTecTempCommandCommand(object parameter)
        {
            if (_Laser532ModelViewPort.IsConnected)
            {
                _Laser532ModelViewPort.GetCurrentTECActualTemperatureValue();
            }

        }
        public bool CanExecuteReadLaserTecTempCommandCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region WriteInstruct485Command
        public ICommand WriteInstruct485Command
        {
            get
            {
                if (_WriteInstruct485Command == null)
                {
                    _WriteInstruct485Command = new RelayCommand(ExecuteWriteInstruct485Command, CanExecuteWriteInstruct485Command);
                }
                return _WriteInstruct485Command;
            }
        }
        public void ExecuteWriteInstruct485Command(object parameter)
        {
            if (_Laser532ModelViewPort.IsConnected)
            {
                _Laser532ModelViewPort.SetInstruct485(SetInstruct485);
            }

        }
        public bool CanExecuteWriteInstruct485Command(object parameter)
        {
            return true;
        }

        #endregion

        private void SetMultChannleCurrentLaserLightPowerValue(int CH, double data)
        {
            if (CH1Check && CH == 1)
            {
                _Laser532ModelViewPort.SetCurrentLaserCurrentValue(data, CH);//关闭激光
                Thread.Sleep(200);
            }
            if (CH2Check && CH == 2)
            {
                _Laser532ModelViewPort.SetCurrentLaserCurrentValue(data, CH);//关闭激光
                Thread.Sleep(200);
            }
            if (CH3Check && CH == 3)
            {
                _Laser532ModelViewPort.SetCurrentLaserCurrentValue(data, CH);//关闭激光
                Thread.Sleep(200);
            }
            if (CH4Check && CH == 4)
            {
                _Laser532ModelViewPort.SetCurrentLaserCurrentValue(data, CH);//关闭激光
                Thread.Sleep(200);
            }
            if (CH5Check && CH == 5)
            {
                _Laser532ModelViewPort.SetCurrentLaserCurrentValue(data, CH);//关闭激光
                Thread.Sleep(200);
            }
            if (CH6Check && CH == 6)
            {
                _Laser532ModelViewPort.SetCurrentLaserCurrentValue(data, CH);//关闭激光
                Thread.Sleep(200);
            }
            if (CH7Check && CH == 7)
            {
                _Laser532ModelViewPort.SetCurrentLaserCurrentValue(data, CH);//关闭激光
                Thread.Sleep(200);
            }
        }
        private void GetMultChannleCurrentLaserLightPowerValue()
        {
            if (CH1Check)
            {
                _Laser532ModelViewPort.GetCurrentLaserLightPowerValueValue(1);//关闭激光
                Thread.Sleep(140);
            }
            if (CH2Check)
            {
                _Laser532ModelViewPort.GetCurrentLaserLightPowerValueValue(2);//关闭激光
                Thread.Sleep(140);
            }
            if (CH3Check)
            {
                _Laser532ModelViewPort.GetCurrentLaserLightPowerValueValue(3);//关闭激光
                Thread.Sleep(140);
            }
            if (CH4Check)
            {
                _Laser532ModelViewPort.GetCurrentLaserLightPowerValueValue(4);//关闭激光
                Thread.Sleep(140);
            }
            if (CH5Check)
            {
                _Laser532ModelViewPort.GetCurrentLaserLightPowerValueValue(5);//关闭激光
                Thread.Sleep(140);
            }
            if (CH6Check)
            {
                _Laser532ModelViewPort.GetCurrentLaserLightPowerValueValue(6);//关闭激光
                Thread.Sleep(140);
            }
            if (CH7Check)
            {
                _Laser532ModelViewPort.GetCurrentLaserLightPowerValueValue(7);//关闭激光
                Thread.Sleep(140);
            }
        }
        private void PrintData()
        {
            if (CH1Check)
            {
                Point LaserPowerPoint = new Point(index, LaserOpticalPowerValueCH1);
                LaserPowerPointCollectionCH1.Add(LaserPowerPoint);
            }
            if (CH2Check)
            {
                Point LaserPowerPoint = new Point(index, LaserOpticalPowerValueCH2);
                LaserPowerPointCollectionCH2.Add(LaserPowerPoint);
            }
            if (CH3Check)
            {
                Point LaserPowerPoint = new Point(index, LaserOpticalPowerValueCH3);
                LaserPowerPointCollectionCH3.Add(LaserPowerPoint);
            }
            if (CH4Check)
            {
                Point LaserPowerPoint = new Point(index, LaserOpticalPowerValueCH4);
                LaserPowerPointCollectionCH4.Add(LaserPowerPoint);
            }
            if (CH5Check)
            {
                Point LaserPowerPoint = new Point(index, LaserOpticalPowerValueCH5);
                LaserPowerPointCollectionCH5.Add(LaserPowerPoint);
            }
            if (CH6Check)
            {
                Point LaserPowerPoint = new Point(index, LaserOpticalPowerValueCH6);
                LaserPowerPointCollectionCH6.Add(LaserPowerPoint);
            }
            if (CH7Check)
            {
                Point LaserPowerPoint = new Point(index, LaserOpticalPowerValueCH7);
                LaserPowerPointCollectionCH7.Add(LaserPowerPoint);
            }
        }

        private DispatcherTimer AbnormalTimer = null;

        private void OnTimeInit(int _Time)
        {
            AbnormalTimer = new DispatcherTimer();
            AbnormalTimer.Tick += TerminationTime;
            AbnormalTimer.Interval = new TimeSpan(0, 0, _Time);//1 sec

        }
        private void TerminationTime(object sender, EventArgs e)
        {
            if (IsStart)
            {
                PeriodIndex++;
            }
        }
        void DataProcessThreadMetoh()
        {
            while (true)
            {
                Thread.Sleep(500);
                //if (_Laser532ModelViewPort.IsConnected)
                //{
                //    _Laser532ModelViewPort.GetCurrentTECActualTemperatureValue();
                //}
                if (IsStart) //点击了开始
                {

                    for (int j = 0; j < CalList[0].Count; j++)
                    {

                        if (PeriodIndex == 10000)
                        {
                            break;
                        }
                        SetMultChannleCurrentLaserLightPowerValue(1, 0);
                        SetMultChannleCurrentLaserLightPowerValue(2, 0);
                        SetMultChannleCurrentLaserLightPowerValue(3, 0);
                        SetMultChannleCurrentLaserLightPowerValue(4, 0);
                        SetMultChannleCurrentLaserLightPowerValue(5, 0);
                        SetMultChannleCurrentLaserLightPowerValue(6, 0);
                        SetMultChannleCurrentLaserLightPowerValue(7, 0);
                        Thread.Sleep(2500);


                        //double LaserPower = CalList[i].ElementAt(0).Key;
                        //double Time = CalList[i].ElementAt(0).Value;


                        double Time = CalList[0][j];
                        
                        double LaserPowerCH0 = CalList[1][j];
                        double LaserPowerCH1 = CalList[2][j];
                        double LaserPowerCH2 = CalList[3][j];
                        double LaserPowerCH3 = CalList[4][j];
                        double LaserPowerCH4 = CalList[5][j];
                        double LaserPowerCH5 = CalList[6][j];
                        double LaserPowerCH6 = CalList[7][j];
                        SetMultChannleCurrentLaserLightPowerValue(1, LaserPowerCH0 * 100);
                        SetMultChannleCurrentLaserLightPowerValue(2, LaserPowerCH1 * 100);
                        SetMultChannleCurrentLaserLightPowerValue(3, LaserPowerCH2 * 100);
                        SetMultChannleCurrentLaserLightPowerValue(4, LaserPowerCH3 * 100);
                        SetMultChannleCurrentLaserLightPowerValue(5, LaserPowerCH4 * 100);
                        SetMultChannleCurrentLaserLightPowerValue(6, LaserPowerCH5 * 100);
                        SetMultChannleCurrentLaserLightPowerValue(7, LaserPowerCH6 * 100);
                        //_Laser532ModelViewPort.SetCurrentLaserLightPowerValueValue(LaserPower);
                        //PeriodIndex = 0;
                        //while (!_Laser532ModelViewPort.DataComplete)
                        //{
                        //    Thread.Sleep(1000);
                        //    SetMultChannleCurrentLaserLightPowerValue(LaserPower);
                        //    //_Laser532ModelViewPort.SetCurrentLaserLightPowerValueValue(LaserPower);
                        //    PeriodIndex++;
                        //    if (PeriodIndex > 5)
                        //    {
                        //        MessageBox.Show("光功率 " + LaserPower + " mW连续5次下发失败");
                        //        //return;
                        //    }
                        //}
                        PeriodIndex = 0;
                        int _second = (int)Time * 60;
                        while (PeriodIndex <= _second) //不满足条件时代表当前周期完成了 
                        {
                            index++;
                            //PeriodIndex++;
                            if (PeriodIndex == 10000)
                            {
                                return;
                            }
                            //_Laser532ModelViewPort.GetCurrentLaserLightPowerValueValue();
                            GetMultChannleCurrentLaserLightPowerValue();
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                PrintData();
                            });
                        }
                    }
                    SetMultChannleCurrentLaserLightPowerValue(1, 0);
                    SetMultChannleCurrentLaserLightPowerValue(2, 0);
                    SetMultChannleCurrentLaserLightPowerValue(3, 0);
                    SetMultChannleCurrentLaserLightPowerValue(4, 0);
                    SetMultChannleCurrentLaserLightPowerValue(5, 0);
                    SetMultChannleCurrentLaserLightPowerValue(6, 0);
                    SetMultChannleCurrentLaserLightPowerValue(7, 0); ;//关闭激光
                    //扫描结束
                    index = 0;
                    IsStart = false;
                    isStartButton = 0;
                    PeriodIndex = 0;
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        _WaveView532LaserSubWind.jiguangWaveButton55.IsEnabled = true;
                        _WaveView532LaserSubWind.jiguangWave1Button11.IsEnabled = true;
                        _WaveView532LaserSubWind.jiguangWaveButton2.IsEnabled = true;
                        _WaveView532LaserSubWind.jiguangWave1Button2.IsEnabled = true;
                        _WaveView532LaserSubWind.jiguangWaveButton.IsEnabled = true;
                        _WaveView532LaserSubWind.jiguangWave1Button.IsEnabled = true;
                        _WaveView532LaserSubWind.jiguangWaveButton1.IsEnabled = true;
                        _WaveView532LaserSubWind.jiguangWave1Button1.IsEnabled = true;
                        _WaveView532LaserSubWind.jiguangWaveButton3.IsEnabled = true;
                        _WaveView532LaserSubWind.jiguangWave1Button3.IsEnabled = true;
                        _WaveView532LaserSubWind.COMNumberCoeffComboBox1.IsEnabled = true;
                        _WaveView532LaserSubWind.jiguangWaveButton4.IsEnabled = true;
                        _WaveView532LaserSubWind.jiguangWave1Button4.IsEnabled = true;
                        WriteFile();
                        _WaveView532LaserSubWind.StartWaveButton.Content = "开始";
                        _WaveView532LaserSubWind.StartWaveButton.Foreground = Brushes.White;
                    });

                }
            }
        }

        void WriteFile()
        {
            int count;
            path = Workspace.This.GetManualFolder();
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            if (CH1Check)
            {
                count = LaserPowerPointCollectionCH1.Count;
                string[] testdata = new string[count];
                for (int y = 0; y < count; y++)
                {
                    testdata[y] = LaserPowerPointCollectionCH1[y].Y.ToString();
                }
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);//创建新路径
                }
                File.WriteAllLines(path + LaserNumber + "_CH1_" + CH1Txt + "_" + "658_" + time + ".csv", testdata, System.Text.Encoding.UTF8);
            }
            if (CH2Check)
            {
                count = LaserPowerPointCollectionCH2.Count;
                string[] testdata = new string[count];
                for (int y = 0; y < count; y++)
                {
                    testdata[y] = "," + LaserPowerPointCollectionCH2[y].Y.ToString();
                }
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);//创建新路径
                }
                File.WriteAllLines(path + LaserNumber + "_CH2_" + CH2Txt + "_" + "685_" + time + ".csv", testdata, System.Text.Encoding.UTF8);
            }
            if (CH3Check)
            {
                count = LaserPowerPointCollectionCH3.Count;
                string[] testdata = new string[count];
                for (int y = 0; y < count; y++)
                {
                    testdata[y] = "," + "," + LaserPowerPointCollectionCH3[y].Y.ToString();
                }
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);//创建新路径
                }
                File.WriteAllLines(path + LaserNumber + "_CH3_" + CH3Txt + "_" + "784_" + time + ".csv", testdata, System.Text.Encoding.UTF8);
            }
            if (CH4Check)
            {
                count = LaserPowerPointCollectionCH4.Count;
                string[] testdata = new string[count];
                for (int y = 0; y < count; y++)
                {
                    testdata[y] = "," + "," + "," + LaserPowerPointCollectionCH4[y].Y.ToString();
                }
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);//创建新路径
                }
                File.WriteAllLines(path + LaserNumber + "_CH4_" + CH4Txt + "_" + "488_" + time + ".csv", testdata, System.Text.Encoding.UTF8);
            }
            if (CH5Check)
            {
                count = LaserPowerPointCollectionCH5.Count;
                string[] testdata = new string[count];
                for (int y = 0; y < count; y++)
                {
                    testdata[y] = "," + "," + "," + "," + LaserPowerPointCollectionCH5[y].Y.ToString();
                }
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);//创建新路径
                }
                File.WriteAllLines(path + LaserNumber + "_CH5_" + CH5Txt + "_" + "638_" + time + ".csv", testdata, System.Text.Encoding.UTF8);
            }
            if (CH6Check)
            {
                count = LaserPowerPointCollectionCH6.Count;
                string[] testdata = new string[count];
                for (int y = 0; y < count; y++)
                {
                    testdata[y] = "," + "," + "," + "," + "," + LaserPowerPointCollectionCH6[y].Y.ToString();
                }
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);//创建新路径
                }
                File.WriteAllLines(path + LaserNumber + "_CH6_" + CH6Txt + "_" + "450_" + time + ".csv", testdata, System.Text.Encoding.UTF8);
            }
            if (CH7Check)
            {
                count = LaserPowerPointCollectionCH7.Count;
                string[] testdata = new string[count];
                for (int y = 0; y < count; y++)
                {
                    testdata[y] = "," + "," + "," + "," + "," + "," + LaserPowerPointCollectionCH7[y].Y.ToString();
                }
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);//创建新路径
                }
                File.WriteAllLines(path + LaserNumber + "_CH7_" + CH7Txt + "_" + "730_" + time + ".csv", testdata, System.Text.Encoding.UTF8);
            }
            _WaveView532LaserSubWind.SavePng(path + LaserNumber, time + ".png");
            MessageBox.Show("记录完成，数据保存成功\n");
            //System.Diagnostics.Process.Start("explorer.exe", path);
        }


        public string CH1Txt
        {
            get
            {
                return _CH1Txt;
            }
            set
            {
                if (_CH1Txt != value)
                {
                    _CH1Txt = value;
                    RaisePropertyChanged("CH1Txt");
                }
            }

        }
        public string CH2Txt
        {
            get
            {
                return _CH2Txt;
            }
            set
            {
                if (_CH2Txt != value)
                {
                    _CH2Txt = value;
                    RaisePropertyChanged("CH2Txt");
                }
            }

        }
        public string CH3Txt
        {
            get
            {
                return _CH3Txt;
            }
            set
            {
                if (_CH3Txt != value)
                {
                    _CH3Txt = value;
                    RaisePropertyChanged("CH3Txt");
                }
            }

        }
        public string CH4Txt
        {
            get
            {
                return _CH4Txt;
            }
            set
            {
                if (_CH4Txt != value)
                {
                    _CH4Txt = value;
                    RaisePropertyChanged("CH4Txt");
                }
            }

        }
        public string CH5Txt
        {
            get
            {
                return _CH5Txt;
            }
            set
            {
                if (_CH5Txt != value)
                {
                    _CH5Txt = value;
                    RaisePropertyChanged("CH5Txt");
                }
            }

        }

        public string CH6Txt
        {
            get
            {
                return _CH6Txt;
            }
            set
            {
                if (_CH6Txt != value)
                {
                    _CH6Txt = value;
                    RaisePropertyChanged("CH6Txt");
                }
            }

        }

        public string CH7Txt
        {
            get
            {
                return _CH7Txt;
            }
            set
            {
                if (_CH7Txt != value)
                {
                    _CH7Txt = value;
                    RaisePropertyChanged("CH7Txt");
                }
            }

        }
        public bool CH1Check
        {
            get
            {
                return _CH1Check;
            }
            set
            {
                if (_CH1Check != value)
                {
                    _CH1Check = value;
                    RaisePropertyChanged("CH1Check");
                }
            }

        }
        public bool CH2Check
        {
            get
            {
                return _CH2Check;
            }
            set
            {
                if (_CH2Check != value)
                {
                    _CH2Check = value;
                    RaisePropertyChanged("CH2Check");
                }
            }

        }
        public bool CH3Check
        {
            get
            {
                return _CH3Check;
            }
            set
            {
                if (_CH3Check != value)
                {
                    _CH3Check = value;
                    RaisePropertyChanged("CH3Check");
                }
            }

        }
        public bool CH4Check
        {
            get
            {
                return _CH4Check;
            }
            set
            {
                if (_CH4Check != value)
                {
                    _CH4Check = value;
                    RaisePropertyChanged("CH4Check");
                }
            }

        }
        public bool CH5Check
        {
            get
            {
                return _CH5Check;
            }
            set
            {
                if (_CH5Check != value)
                {
                    _CH5Check = value;
                    RaisePropertyChanged("CH5Check");
                }
            }

        }
        public bool CH6Check
        {
            get
            {
                return _CH6Check;
            }
            set
            {
                if (_CH6Check != value)
                {
                    _CH6Check = value;
                    RaisePropertyChanged("CH6Check");
                }
            }

        }
        public bool CH7Check
        {
            get
            {
                return _CH7Check;
            }
            set
            {
                if (_CH7Check != value)
                {
                    _CH7Check = value;
                    RaisePropertyChanged("CH7Check");
                }
            }

        }

        public double CH1OpticalPower
        {
            get
            {
                return _CH1OpticalPower;
            }
            set
            {
                if (_CH1OpticalPower != value)
                {
                    _CH1OpticalPower = value;
                    RaisePropertyChanged("CH1OpticalPower");
                }
            }

        }
        public double CH2OpticalPower
        {
            get
            {
                return _CH2OpticalPower;
            }
            set
            {
                if (_CH2OpticalPower != value)
                {
                    _CH2OpticalPower = value;
                    RaisePropertyChanged("CH2OpticalPower");
                }
            }

        }
        public double CH3OpticalPower
        {
            get
            {
                return _CH3OpticalPower;
            }
            set
            {
                if (_CH3OpticalPower != value)
                {
                    _CH3OpticalPower = value;
                    RaisePropertyChanged("CH3OpticalPower");
                }
            }

        }

        public double CH4OpticalPower
        {
            get
            {
                return _CH4OpticalPower;
            }
            set
            {
                if (_CH4OpticalPower != value)
                {
                    _CH4OpticalPower = value;
                    RaisePropertyChanged("CH4OpticalPower");
                }
            }

        }

        public double CH5OpticalPower
        {
            get
            {
                return _CH5OpticalPower;
            }
            set
            {
                if (_CH5OpticalPower != value)
                {
                    _CH5OpticalPower = value;
                    RaisePropertyChanged("CH5OpticalPower");
                }
            }

        }

        public double CH6OpticalPower
        {
            get
            {
                return _CH6OpticalPower;
            }
            set
            {
                if (_CH6OpticalPower != value)
                {
                    _CH6OpticalPower = value;
                    RaisePropertyChanged("CH6OpticalPower");
                }
            }

        }

        public double CH7OpticalPower
        {
            get
            {
                return _CH7OpticalPower;
            }
            set
            {
                if (_CH7OpticalPower != value)
                {
                    _CH7OpticalPower = value;
                    RaisePropertyChanged("CH7OpticalPower");
                }
            }

        }


        public double CH1Time
        {
            get
            {
                return _CH1Time;
            }
            set
            {
                if (_CH1Time != value)
                {
                    _CH1Time = value;
                    RaisePropertyChanged("CH1Time");
                }
            }

        }
        public double CH2Time
        {
            get
            {
                return _CH2Time;
            }
            set
            {
                if (_CH2Time != value)
                {
                    _CH2Time = value;
                    RaisePropertyChanged("CH2Time");
                }
            }

        }
        public double CH3Time
        {
            get
            {
                return _CH3Time;
            }
            set
            {
                if (_CH3Time != value)
                {
                    _CH3Time = value;
                    RaisePropertyChanged("CH3Time");
                }
            }

        }
        public double CH4Time
        {
            get
            {
                return _CH4Time;
            }
            set
            {
                if (_CH4Time != value)
                {
                    _CH4Time = value;
                    RaisePropertyChanged("CH4Time");
                }
            }

        }
        public double CH5Time
        {
            get
            {
                return _CH5Time;
            }
            set
            {
                if (_CH5Time != value)
                {
                    _CH5Time = value;
                    RaisePropertyChanged("CH5Time");
                }
            }

        }
        public double CH6Time
        {
            get
            {
                return _CH6Time;
            }
            set
            {
                if (_CH6Time != value)
                {
                    _CH6Time = value;
                    RaisePropertyChanged("CH6Time");
                }
            }

        }
        public double CH7Time
        {
            get
            {
                return _CH7Time;
            }
            set
            {
                if (_CH7Time != value)
                {
                    _CH7Time = value;
                    RaisePropertyChanged("CH7Time");
                }
            }

        }
        public MultChannelLaserPort Laser532ModelViewPort
        {
            get
            {
                return _Laser532ModelViewPort;
            }
            set
            {
                _Laser532ModelViewPort = value;
            }

        }

        public string LaserNumber
        {
            get
            {
                return _LaserNumber;
            }
            set
            {
                _LaserNumber = value;
                RaisePropertyChanged("LaserNumber");
            }
        }
        public string SetInstruct485
        {
            get
            {
                return _SetInstruct485;
            }
            set
            {
                _SetInstruct485 = value;
                RaisePropertyChanged("SetInstruct485");
            }
        }

        public string GetInstruct485
        {
            get
            {
                return _GetInstruct485;
            }
            set
            {
                _GetInstruct485 = value;
                RaisePropertyChanged("GetInstruct485");
            }
        }
        public double GetCurrentTecTempValue
        {
            get
            {
                return _GetCurrentTecTempValue;
            }
            set
            {
                _GetCurrentTecTempValue = value;
                RaisePropertyChanged("GetCurrentTecTempValue");
            }
        }
        public ObservableCollection<string> COMNumberCoeffOptions
        {
            get
            {
                return _COMNumberCoeffOptions;
            }
        }
        public ObservableCollection<int> ChannelNumberCoeffOptions
        {
            get
            {
                return _ChannelNumberCoeffOptions;
            }
        }
        public string SelectedCOMNumberCoeff
        {
            get
            {
                return _SelectedCOMNumberCoeff;
            }
            set
            {
                if (_SelectedCOMNumberCoeff != value)
                {
                    _SelectedCOMNumberCoeff = value;
                    if (string.IsNullOrEmpty(value))
                    {
                        return;
                    }
                    if (_Laser532ModelViewPort.IsConnected == false)
                    {
                        _Laser532ModelViewPort.SearchPort(value, 115200);
                        Thread.Sleep(1000);
                        // _Laser532ModelViewPort.SetCurrentLaserCurrentValue(0);//关闭激光
                    }
                    else
                    {
                        _Laser532ModelViewPort.Dispose();
                        _Laser532ModelViewPort.SearchPort(value, 115200);
                    }
                    RaisePropertyChanged("SelectedCOMNumberCoeff");
                }
            }
        }
        public int  SelectedChannelNumberCoeff
        {
            get
            {
                return _SelectedChannelNumberCoeff;
            }
            set
            {
                if (_SelectedChannelNumberCoeff != value)
                {
                    _SelectedChannelNumberCoeff = value;
                    if (_Laser532ModelViewPort != null)
                    {
                        _Laser532ModelViewPort.ChannelAddress = Convert.ToByte(value);

                    }
                    RaisePropertyChanged("SelectedChannelNumberCoeff");
                }
            }
        }

        public ObservableCollection<int> PhotodiodeVoltageCorrespondingToLaserPowerOptions
        {
            get
            {
                return _PhotodiodeVoltageCorrespondingToLaserPowerOptions;
            }
        }
        public int SelectedPhotodiodeVoltageCorrespondingToLaserPower
        {
            get
            {
                return _SelectedPhotodiodeVoltageCorrespondingToLaserPower;
            }
            set
            {
                if (_SelectedPhotodiodeVoltageCorrespondingToLaserPower != value)
                {
                    _SelectedPhotodiodeVoltageCorrespondingToLaserPower = value;
                    RaisePropertyChanged("SelectedPhotodiodeVoltageCorrespondingToLaserPower");
                }
            }
        }
        public double LaserOpticalPowerValue
        {
            get
            {
                return _LaserOpticalPowerValue;
            }
            set
            {
                _LaserOpticalPowerValue = value;
                RaisePropertyChanged("LaserOpticalPowerValue");
            }
        }
        public double LaserElectricValue
        {
            get
            {
                return _LaserElectricValue;
            }
            set
            {
                _LaserElectricValue = value;
                RaisePropertyChanged("LaserElectricValue");
            }
        }
        public double LaserCurrentValue
        {
            get
            {
                return _LaserCurrentValue;
            }
            set
            {
                _LaserCurrentValue = value;
                RaisePropertyChanged("LaserCurrentValue");
            }
        }
        public double InteriorPDValue
        {
            get
            {
                return _InteriorPDValue;
            }
            set
            {
                _InteriorPDValue = value;
                RaisePropertyChanged("InteriorPDValue");
            }
        }
        public double PD532Value
        {
            get
            {
                return _PD532Value;
            }
            set
            {
                _PD532Value = value;
                RaisePropertyChanged("PD532Value");
            }
        }
        public double PhotodiodeVoltageCorrespondingToLaserPowerValue
        {
            get
            {
                return _PhotodiodeVoltageCorrespondingToLaserPowerValue;
            }
            set
            {
                _PhotodiodeVoltageCorrespondingToLaserPowerValue = value;
                RaisePropertyChanged("PhotodiodeVoltageCorrespondingToLaserPowerValue");
            }
        }

        public EnumerableDataSource<Point> LaserPowerCH1
        {
            get
            {
                return _LaserPowerCH1;
            }
            set
            {
                _LaserPowerCH1 = value;
                RaisePropertyChanged("LaserPowerCH1");
            }

        }
        public EnumerableDataSource<Point> LaserPowerCH2
        {
            get
            {
                return _LaserPowerCH2;
            }
            set
            {
                _LaserPowerCH2 = value;
                RaisePropertyChanged("LaserPowerCH2");
            }

        }
        public EnumerableDataSource<Point> LaserPowerCH3
        {
            get
            {
                return _LaserPowerCH3;
            }
            set
            {
                _LaserPowerCH3 = value;
                RaisePropertyChanged("LaserPowerCH3");
            }

        }
        public EnumerableDataSource<Point> LaserPowerCH4
        {
            get
            {
                return _LaserPowerCH4;
            }
            set
            {
                _LaserPowerCH4 = value;
                RaisePropertyChanged("LaserPowerCH4");
            }

        }
        public EnumerableDataSource<Point> LaserPowerCH5
        {
            get
            {
                return _LaserPowerCH5;
            }
            set
            {
                _LaserPowerCH5 = value;
                RaisePropertyChanged("LaserPowerCH5");
            }

        }
        public EnumerableDataSource<Point> LaserPowerCH6
        {
            get
            {
                return _LaserPowerCH6;
            }
            set
            {
                _LaserPowerCH6 = value;
                RaisePropertyChanged("LaserPowerCH6");
            }

        }
        public EnumerableDataSource<Point> LaserPowerCH7
        {
            get
            {
                return _LaserPowerCH7;
            }
            set
            {
                _LaserPowerCH7 = value;
                RaisePropertyChanged("LaserPowerCH7");
            }

        }

        public EnumerableDataSource<Point> HistoryLaserPowerCH1
        {
            get
            {
                return _HistoryLaserPowerCH1;
            }
            set
            {
                _HistoryLaserPowerCH1 = value;
                RaisePropertyChanged("HistoryLaserPowerCH1");
            }

        }

        public EnumerableDataSource<Point> HistoryLaserPowerCH2
        {
            get
            {
                return _HistoryLaserPowerCH2;
            }
            set
            {
                _HistoryLaserPowerCH2 = value;
                RaisePropertyChanged("HistoryLaserPowerCH2");
            }

        }
        public EnumerableDataSource<Point> HistoryLaserPowerCH3
        {
            get
            {
                return _HistoryLaserPowerCH3;
            }
            set
            {
                _HistoryLaserPowerCH3 = value;
                RaisePropertyChanged("HistoryLaserPowerCH3");
            }

        }
        public EnumerableDataSource<Point> HistoryLaserPowerCH4
        {
            get
            {
                return _HistoryLaserPowerCH4;
            }
            set
            {
                _HistoryLaserPowerCH4 = value;
                RaisePropertyChanged("HistoryLaserPowerCH4");
            }

        }
        public EnumerableDataSource<Point> HistoryLaserPowerCH5
        {
            get
            {
                return _HistoryLaserPowerCH5;
            }
            set
            {
                _HistoryLaserPowerCH5 = value;
                RaisePropertyChanged("HistoryLaserPowerCH5");
            }

        }
        public EnumerableDataSource<Point> HistoryLaserPowerCH6
        {
            get
            {
                return _HistoryLaserPowerCH6;
            }
            set
            {
                _HistoryLaserPowerCH6 = value;
                RaisePropertyChanged("HistoryLaserPowerCH6");
            }

        }
        public EnumerableDataSource<Point> HistoryLaserPowerCH7
        {
            get
            {
                return _HistoryLaserPowerCH7;
            }
            set
            {
                _HistoryLaserPowerCH7 = value;
                RaisePropertyChanged("HistoryLaserPowerCH7");
            }

        }

        public bool IsSettingClose { get => isSettingClose; set => isSettingClose = value; }
    }
}
