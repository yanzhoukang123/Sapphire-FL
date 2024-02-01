using Azure.WPF.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Azure.ScannerTestJig.View._532WaveForm;
using Azure.APDCalibrationBench;
using System.Windows;
using System.Threading;
using System.Windows.Threading;
using System.Windows.Media;
using System.IO;
using System.Diagnostics;
using System.Windows.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.ComponentModel;

namespace Azure.ScannerTestJig.ViewModule._532WaveForm
{
    class WaveViewChartViewModel : ViewModelBase
    {
        #region Private data
        private string[] _AvailablePorts;
        private ObservableCollection<string> _COMNumberCoeffOptions = new ObservableCollection<string>();
        private ObservableCollection<int> _BAUDNumberCoeffOptions = new ObservableCollection<int>();
        private string _SelectedCOMNumberCoeff;
        private int _SelectedBAUDNumberCoeff;
        private RelayCommand _Data532WaveCommand = null;
        private RelayCommand _Out532WaveCommand = null;
        private RelayCommand _LaserCurrentCommand = null;
        private RelayCommand _StartWaveCommand = null;
        private RelayCommand _GetLaserPowerCommand = null;
        
        private double _LaserPowerValue;
        private double _LaserElectricValue;
        private double _TecTemperValue;
        private EnumerableDataSource<Point> _LaserPower = null;
        private EnumerableDataSource<Point> _LaserElectric = null;
        private EnumerableDataSource<Point> _TecTemper = null;
        private EnumerableDataSource<Point> _HistoryLaserPower = null;
        private EnumerableDataSource<Point> _HistoryLaserElectric = null;
        private EnumerableDataSource<Point> _HistoryTecTemper = null;
        private Laser532WaveViewPort _Laser532WaveViewPort = null;
        private double _GetCurrentTecTempValue = 0;
        private double _GetLaserPowerValue = 0;
        private double _LaserCurrentValue = 0;
        private int _StartLaserNumber = 0;
        private int _EndLaserNumber = 0;
        private string _LaserNumber;
        public WaveViewSubWind _WaveViewSubWind = null;
        public Display532Data _Display532Data = null;
        public bool IsStart=false;
        public bool IsDone = false;
        public bool IsStepBoFeng = false;
        public int index = 0;
        public double MaxLaseCurrentindex = 0;
        public double MinLaseCurrentindex = 0;
        public VoltagePointCollection TecTemperPointCollection;
        public VoltagePointCollection LaserPowerPointCollection;
        public VoltagePointCollection LaserElectricPointCollection;
        public VoltagePointCollection HistoryTecTemperPointCollection;
        public VoltagePointCollection HistoryLaserPowerPointCollection;
        public VoltagePointCollection HistoryLaserElectricPointCollection;
        DispatcherTimer updateCollectionTimer;
        DispatcherTimer GetLaserPowerTimer;
        DispatcherTimer SetLaseCurrentTimer;
        DispatcherTimer GetTECTemperTimer;
        //public int MaxLaserPower = 421;
        //public int MaxLaserPower = 140;
        //public int MinLaserPower = 120;
        public int step1 = 1122;
        public int step2 = 1216;
        //public int MaxLaserPower = 150;
        //public int MinLaserPower = 120;
        //public int step1 = 200;
        //public int step2 = 550;
        public int isStartButton = 0;
        BrushConverter  bc = new BrushConverter();
        public Thread HistoryThread;
        public Thread DataProcessThread;
        public Thread StepThread;
        int frequency = 0;
        int _LaserNumberTime = 1;
        double _LaserNumberStep = 1;
        //private int i = 0;
        //public double[] SampleValueTecTemper { get; private set; }
        //public double[] SampleValueLaserPower { get; private set; }
        //public double[] SampleValueLaserElectric { get; private set; }

        private Thread ProcessThread;
        //private double StepPoint;
        private int sleepTime =100;

        string path = Directory.GetCurrentDirectory() + @"\测试报告\";
        #endregion

        public void ExcutePortConnectCommand()
        {
            try
            {
                LaserPowerValue = 0;
                LaserElectricValue = 0;
                GetCurrentTecTempValue = 0;
                _AvailablePorts =SerialPort.GetPortNames();
                if (_AvailablePorts.Length == 0)
                {
                    MessageBox.Show("未找到串口设备！", "连接错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    _WaveViewSubWind = new WaveViewSubWind();
                    _WaveViewSubWind.Title = Workspace.This.Owner.Title + "-532 WaveView Kit";
                    _WaveViewSubWind.Data532WaveButton.IsEnabled = true;
                    _WaveViewSubWind.Out532WaveButton.IsEnabled = false;
                    _WaveViewSubWind.ji1guangWaveButton.IsEnabled = false;
                    _WaveViewSubWind.jiguangWaveButton.IsEnabled = false;
                    _WaveViewSubWind.StartWaveButton.IsEnabled = false;
                    _WaveViewSubWind.ShowDialog();
                    _WaveViewSubWind = null;
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
                _BAUDNumberCoeffOptions.Clear();
                if (_BAUDNumberCoeffOptions.Count <= 0)
                {
                    _BAUDNumberCoeffOptions.Add(9600);
                    _BAUDNumberCoeffOptions.Add(115200);
                }
                if (SelectedBAUDNumberCoeff==0)
                {
                    SelectedBAUDNumberCoeff = _BAUDNumberCoeffOptions[0];
                }

                //GetCurrentTecTempValue = 0;
                frequency = 0;
                isStartButton = 0;
                IsStart = false;
                IsDone = false;
                IsStepBoFeng = false;
                index = 0;
                //i = 0;
                _LaserCurrentValue = 0;
                MaxLaseCurrentindex = 0;
                MinLaseCurrentindex = 0;
                if (_AvailablePorts.Length > 0)
                {
                    if (_Laser532WaveViewPort == null || _Laser532WaveViewPort.IsConnected == false)
                    {
                        _Laser532WaveViewPort = new Laser532WaveViewPort();
                        _Laser532WaveViewPort.UpdateCommOutput += _Laser532WaveViewPort_UpdateCommOutput; ;
                    }
                    //SampleValueTecTemper = new double[step2];
                    //SampleValueLaserPower = new double[step2];
                    //SampleValueLaserElectric = new double[step2];
                    TecTemperPointCollection = new VoltagePointCollection();
                    LaserPowerPointCollection = new VoltagePointCollection();
                    LaserElectricPointCollection = new VoltagePointCollection();
                    HistoryTecTemperPointCollection = new VoltagePointCollection();
                    HistoryLaserPowerPointCollection = new VoltagePointCollection();
                    HistoryLaserElectricPointCollection = new VoltagePointCollection();
                    DataProcessThread = new Thread(DataProcessThreadMetoh);
                    DataProcessThread.Priority = ThreadPriority.Highest;
                    DataProcessThread.IsBackground = true;
                    DataProcessThread.Start();
                    StepThread = new Thread(StepThreadMetoh);
                    StepThread.IsBackground = true;
                    //StepThread.Start();
                    //读取TEC实际温度
                    //GetTECTemperTimer = new DispatcherTimer();
                    //GetTECTemperTimer.Interval = TimeSpan.FromMilliseconds(800);
                    //GetTECTemperTimer.Tick += GetTECTemperTimer_Tick;
                    ////GetTECTemperTimer.Start();
                    ////读取激光功率
                    ////GetLaserPowerTimer = new DispatcherTimer();
                    ////GetLaserPowerTimer.Interval = TimeSpan.FromMilliseconds(600);
                    ////GetLaserPowerTimer.Tick += GetLaserPowerTimer_Tick;
                    ////GetLaserPowerTimer.Stop();
                    ////写入激光电流
                    //SetLaseCurrentTimer = new DispatcherTimer();
                    //SetLaseCurrentTimer.Interval = TimeSpan.FromMilliseconds(800);
                    //SetLaseCurrentTimer.Tick += SetLaseCurrentTimer_Tick;
                    //SetLaseCurrentTimer.Stop();
                    //打印数据到波形
                    //updateCollectionTimer = new DispatcherTimer();
                    //updateCollectionTimer.Interval = TimeSpan.FromMilliseconds(500);
                    //updateCollectionTimer.Tick += new EventHandler(updateCollectionTimer_Tick);
                    //updateCollectionTimer.Stop();
                    TecTemper = new EnumerableDataSource<Point>(TecTemperPointCollection);
                    TecTemper.SetXMapping(x => x.X);
                    TecTemper.SetYMapping(y => y.Y);
                    LaserPower = new EnumerableDataSource<Point>(LaserPowerPointCollection);
                    LaserPower.SetXMapping(x => x.X);
                    LaserPower.SetYMapping(y => y.Y);
                    LaserElectric = new EnumerableDataSource<Point>(LaserElectricPointCollection);
                    LaserElectric.SetXMapping(x => x.X);
                    LaserElectric.SetYMapping(y => y.Y);
                    HistoryTecTemper = new EnumerableDataSource<Point>(HistoryTecTemperPointCollection);
                    HistoryTecTemper.SetXMapping(x => x.X);
                    HistoryTecTemper.SetYMapping(y => y.Y);
                    HistoryLaserPower = new EnumerableDataSource<Point>(HistoryLaserPowerPointCollection);
                    HistoryLaserPower.SetXMapping(x => x.X);
                    HistoryLaserPower.SetYMapping(y => y.Y);
                    HistoryLaserElectric = new EnumerableDataSource<Point>(HistoryLaserElectricPointCollection);
                    HistoryLaserElectric.SetXMapping(x => x.X);
                    HistoryLaserElectric.SetYMapping(y => y.Y);
                    _WaveViewSubWind = new WaveViewSubWind();
                    _WaveViewSubWind.Title = Workspace.This.Owner.Title + "-532 WaveView Kit";
                    _WaveViewSubWind.ShowDialog();
                    _WaveViewSubWind = null;
                }

            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }

        }
        void StepThreadMetoh()
        { 
        
        }
        void DataProcessThreadMetoh()
        {

            while (true)
            {
                Thread.Sleep(10);
                if (_Laser532WaveViewPort.IsConnected)
                {
                    while (_Laser532WaveViewPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _Laser532WaveViewPort.GetCurrentTECActualTemperatureValue();
                    Thread.Sleep(sleepTime);
                    if (isStartButton == 1)//点击了开始按钮
                    {
                        if (!IsStart)
                        {
                            if (GetCurrentTecTempValue >= 24.8 && GetCurrentTecTempValue <= 25.2)
                            {
                                MaxLaseCurrentindex = 0;
                                IsStart = true;
                                IsDone = true;
                            }
                            else
                            {
                                Thread.Sleep(1000);
                            }

                        }

                    }
                    if (IsDone)
                    {
                        index++;
                        while (_Laser532WaveViewPort.IsBusy)
                        {
                                Thread.Sleep(1);
                        }
                        _Laser532WaveViewPort.GetCurrentLaserLightPowerValueValue();
                        Thread.Sleep(sleepTime);
                        double __MaxLaseCurrentindex = StartLaserNumber + MaxLaseCurrentindex;
                        double __MinLaseCurrentindex = EndLaserNumber + MaxLaseCurrentindex;
                        //从起点到终点设置激光功率
                        if (__MaxLaseCurrentindex < EndLaserNumber)
                        {
                            while (_Laser532WaveViewPort.IsBusy)
                            {
                                Thread.Sleep(1);
                            }
                            _Laser532WaveViewPort.GetCurrentLaserCurrentValue();
                            Thread.Sleep(sleepTime);
                            while (_Laser532WaveViewPort.IsBusy)
                            {
                                Thread.Sleep(1);
                            }
                            _Laser532WaveViewPort.SetCurrentLaserCurrentValue(__MaxLaseCurrentindex);
                            Thread.Sleep(sleepTime);
                            if (index % LaserNumberTime == 0)
                            {
                                MaxLaseCurrentindex = MaxLaseCurrentindex + LaserNumberStep;
                                MinLaseCurrentindex = MaxLaseCurrentindex;
                            }
                        }
                        else
                        {
                            //从终点到起点设置激光功率
                            while (_Laser532WaveViewPort.IsBusy)
                            {
                                Thread.Sleep(1);
                            }
                            _Laser532WaveViewPort.GetCurrentLaserCurrentValue();
                            Thread.Sleep(sleepTime);
                            while (_Laser532WaveViewPort.IsBusy)
                            {
                                Thread.Sleep(1);
                            }
                            _Laser532WaveViewPort.SetCurrentLaserCurrentValue(__MinLaseCurrentindex - MinLaseCurrentindex);
                            Thread.Sleep(sleepTime);
                            if (index % LaserNumberTime == 0)
                            {
                                MinLaseCurrentindex = MinLaseCurrentindex + LaserNumberStep;
                            }
                            if (__MinLaseCurrentindex - MinLaseCurrentindex <= StartLaserNumber) //当一个周期完成时
                            {
                                frequency++; //记录当前的周期
                                MaxLaseCurrentindex = 0;
                            }
                          
                        }
                        double tectemper = TecTemperValue;
                        double laserPower = LaserPowerValue;
                        double laserelectric = LaserElectricValue;
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            Point TemperPoint = new Point(index, tectemper);
                            TecTemperPointCollection.Add(TemperPoint);
                            Point LaserPowerPoint = new Point(index, laserPower);
                            LaserPowerPointCollection.Add(LaserPowerPoint);
                            Point LaserElectricPoint = new Point(index, laserelectric);
                            LaserElectricPointCollection.Add(LaserElectricPoint);
                        });

                        for (int i = 0; i < 2; i++)
                        {
                            index++;
                            while (_Laser532WaveViewPort.IsBusy)
                            {
                                Thread.Sleep(1);
                            }
                            _Laser532WaveViewPort.GetCurrentLaserLightPowerValueValue();
                            Thread.Sleep(sleepTime);
                            while (_Laser532WaveViewPort.IsBusy)
                            {
                                Thread.Sleep(1);
                            }
                            _Laser532WaveViewPort.GetCurrentLaserCurrentValue();
                            Thread.Sleep(sleepTime);
                            while (_Laser532WaveViewPort.IsBusy)
                            {
                                Thread.Sleep(1);
                            }
                            Thread.Sleep(sleepTime);
                            tectemper = TecTemperValue;
                            laserPower = LaserPowerValue;
                            laserelectric = LaserElectricValue;
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                Point TemperPoint = new Point(index, tectemper);
                                TecTemperPointCollection.Add(TemperPoint);
                                Point LaserPowerPoint = new Point(index, laserPower);
                                LaserPowerPointCollection.Add(LaserPowerPoint);
                                Point LaserElectricPoint = new Point(index, laserelectric);
                                LaserElectricPointCollection.Add(LaserElectricPoint);
                            });
                        }
                        if (frequency == 1)
                        {
                            while (_Laser532WaveViewPort.IsBusy)
                            {
                                Thread.Sleep(1);
                            }

                            _Laser532WaveViewPort.SetCurrentLaserCurrentValue(0);//关闭激光
                            IsDone = false;
                            frequency = 0;
                            index = 0;
                            isStartButton = 0;
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                WriteFile();
                                _WaveViewSubWind.COMNumberCoeffComboBox.IsEnabled = true;
                                _WaveViewSubWind.FileNameTextBox11.IsEnabled = true;
                                _WaveViewSubWind.FileNameTextBox1111.IsEnabled = true;
                                _WaveViewSubWind.FileNameTextBox13.IsEnabled = true;
                                _WaveViewSubWind.FileNameTextBox13_Copy.IsEnabled = true;
                                _WaveViewSubWind.StartWaveButton.Content = "开始";
                                _WaveViewSubWind.StartWaveButton.Foreground = Brushes.White;
                            });
                        }

                        //Thread.Sleep();
                    }
                }

            }
        }

        private void GetTECTemperTimer_Tick(object sender, EventArgs e)
        {
            if (_Laser532WaveViewPort.IsConnected)
            {
                while (_Laser532WaveViewPort.IsBusy)
                {
                    Thread.Sleep(1);
                }
                _Laser532WaveViewPort.GetCurrentTECActualTemperatureValue();
                //Thread.Sleep(20);
                //for (int i = 0; i < 4; i++)
                //{
                //    _Laser532WaveViewPort.GetCurrentLaserLightPowerValueValue();
                //    Thread.Sleep(20);
                //}
            }
        }

        private void GetLaserPowerTimer_Tick(object sender, EventArgs e)
        {
            if (_Laser532WaveViewPort.IsConnected)
            {
                while (_Laser532WaveViewPort.IsBusy)
                {
                    Thread.Sleep(1);
                }
                _Laser532WaveViewPort.GetCurrentLaserLightPowerValueValue();
            }
        }

        private void SetLaseCurrentTimer_Tick(object sender, EventArgs e)
        {
            if (_Laser532WaveViewPort.IsConnected)
            {
                while (_Laser532WaveViewPort.IsBusy)
                {
                    Thread.Sleep(10);
                }
                //int __MaxLaseCurrentindex = MinLaserPower + MaxLaseCurrentindex;
                //int __MinLaseCurrentindex = MaxLaserPower + MaxLaseCurrentindex;
                //if (__MaxLaseCurrentindex < MaxLaserPower)
                double __MaxLaseCurrentindex = StartLaserNumber + MaxLaseCurrentindex;
                double __MinLaseCurrentindex = EndLaserNumber + MaxLaseCurrentindex;
                if (__MaxLaseCurrentindex < EndLaserNumber)
                {
                    _Laser532WaveViewPort.GetCurrentLaserCurrentValue();
                    Thread.Sleep(10);
                    _Laser532WaveViewPort.SetCurrentLaserCurrentValue(__MaxLaseCurrentindex);
                    //LaserCurrentValue = __LaseCurrentindex;
                    MaxLaseCurrentindex++;
                    MinLaseCurrentindex = MaxLaseCurrentindex;
                }
                else
                {
                    //当温度升到30度开始设置激光功率
                    //if (__MinLaseCurrentindex - MinLaseCurrentindex < MinLaserPower)
                    //{
                    //    IsStepBoFeng = false;
                    //    return;
                    //}
                    if (__MinLaseCurrentindex - MinLaseCurrentindex < StartLaserNumber)
                    {
                        IsStepBoFeng = false;
                        return;
                    }
                    _Laser532WaveViewPort.GetCurrentLaserCurrentValue();
                    Thread.Sleep(10);
                    _Laser532WaveViewPort.SetCurrentLaserCurrentValue(__MinLaseCurrentindex- MinLaseCurrentindex);
                    //LaserCurrentValue = __LaseCurrentindex;
                    MinLaseCurrentindex++;
                    if (__MinLaseCurrentindex - MinLaseCurrentindex == StartLaserNumber)
                    {
                        if (GetCurrentTecTempValue < 29.9 || GetCurrentTecTempValue > 30.1)
                        {
                            _Laser532WaveViewPort.SetCurrentTEControlTemperatureValue(30);
                            Thread.Sleep(10);
                            _Laser532WaveViewPort.SetCurrentTEControlTemperatureValue(30);
                        }
                        frequency++;
                    }
                    //if (__MinLaseCurrentindex - MinLaseCurrentindex == MinLaserPower)
                    //{
                    //    if (GetCurrentTecTempValue < 29.9 || GetCurrentTecTempValue > 30.1)
                    //    {
                    //        _Laser532WaveViewPort.SetCurrentTEControlTemperatureValue(30);
                    //        Thread.Sleep(10);
                    //        _Laser532WaveViewPort.SetCurrentTEControlTemperatureValue(30);
                    //    }
                    //    frequency++;
                    //}
                }
            }
        }

        void updateCollectionTimer_Tick(object sender, EventArgs e)
        {
            //i++;
            if (IsStart)
            {
                index++;
                double tectemper = TecTemperValue;
                double laserPower = LaserPowerValue;
                double laserelectric = LaserElectricValue;
                Point TemperPoint = new Point(index, tectemper);
                TecTemperPointCollection.Add(TemperPoint);
                Point LaserPowerPoint = new Point(index, laserPower);
                LaserPowerPointCollection.Add(LaserPowerPoint);
                Point LaserElectricPoint = new Point(index, laserelectric);
                LaserElectricPointCollection.Add(LaserElectricPoint);
                //SampleValueTecTemper[index-1]=tectemper;
                //SampleValueLaserPower[index-1]=laserPower;
                //SampleValueLaserElectric[index-1]=laserelectric;
                if (frequency == 3)
                {
                    _Laser532WaveViewPort.SetCurrentTEControlTemperatureValue(25);
                    Thread.Sleep(100);
                    _Laser532WaveViewPort.SetCurrentLaserCurrentValue(0);//关闭激光
                    //GetLaserPowerTimer.Stop();
                    //SetLaseCurrentTimer.Stop();
                    //updateCollectionTimer.Stop();
                    index = 0;
                    WriteFile();
                    _WaveViewSubWind.StartWaveButton.Content = "开始";
                    _WaveViewSubWind.StartWaveButton.Foreground = Brushes.White;
                }
            }
        }

        #region 导入波形和导出数据
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
                HistoryTecTemperPointCollection.Clear();
                HistoryLaserPowerPointCollection.Clear();
                HistoryLaserElectricPointCollection.Clear();
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
                    _Display532Data = new Display532Data();
                    _Display532Data.fileName.Content = fName;
                    _Display532Data.Title = Workspace.This.Owner.Title + "-532 History WaveView Kit";
                    _Display532Data.Show();
                    _Display532Data = null;
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
            for (int h = 0; h < DataValue.Length; h++)
            {
                string[] Data = DataValue[h].Split(new string[] { "," }, StringSplitOptions.None);
                if (Data.Length >1)
                {
                    Thread.Sleep(50);
                    if (Data[0] == "")
                    {
                        continue;
                    }
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        double tectemper = Convert.ToDouble(Data[2]);
                        double laserPower = Convert.ToDouble(Data[0]);
                        double laserelectric = Convert.ToDouble(Data[1]);
                        Point TemperPoint = new Point(h, tectemper);
                        HistoryTecTemperPointCollection.Add(TemperPoint);
                        Point LaserPowerPoint = new Point(h, laserPower);
                        HistoryLaserPowerPointCollection.Add(LaserPowerPoint);
                        Point LaserElectricPoint = new Point(h, laserelectric);
                        HistoryLaserElectricPointCollection.Add(LaserElectricPoint);
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
            string[] testdata = new string[LaserPowerPointCollection.Count];
            for (int y = 0; y < testdata.Length; y++)
            {
                //testdata[y] = LaserPowerPointCollection[y].Y.ToString();
                testdata[y] = LaserPowerPointCollection[y].Y.ToString() + "," +
                LaserElectricPointCollection[y].Y.ToString() + "," +
                TecTemperPointCollection[y].Y.ToString();
            }
            //if (!Directory.Exists(path))
            //{
            //    Directory.CreateDirectory(path);//创建新路径
            //}
            time = StrDate();
            string csvname = LaserNumber + "_" + time + ".csv";
            string csvPath = Workspace.This.GetManualPath(csvname);
            if (string.IsNullOrEmpty(csvPath))
            {
                return;
            }
            string pngPath = System.IO.Path.ChangeExtension(csvPath, ".png");
            File.WriteAllLines(csvPath, testdata, System.Text.Encoding.UTF8);
            _WaveViewSubWind.SavePng(pngPath);
            Thread _temp = new Thread(newProcess);
            _temp.IsBackground = true;
            _temp.Start();


        }
        void newProcess()
        {
            MessageBox.Show("导出成功");
            //System.Diagnostics.Process.Start("explorer.exe", path);
        }
        public bool CanExecuteOut532WaveCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region LaserCurrentCommand
        public ICommand LaserCurrentCommand
        {
            get
            {
                if (_LaserCurrentCommand == null)
                {
                    _LaserCurrentCommand = new RelayCommand(ExecuteLaserCurrentCommand, CanExecuteLaserCurrentCommand);
                }
                return _LaserCurrentCommand;
            }
        }
        public void ExecuteLaserCurrentCommand(object parameter)
        {
            if (_Laser532WaveViewPort.IsConnected)
            {
                while (_Laser532WaveViewPort.IsBusy)
                {
                    Thread.Sleep(1);
                }
                _Laser532WaveViewPort.SetCurrentLaserCurrentValue(LaserCurrentValue);
            }

        }
        public bool CanExecuteLaserCurrentCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region StartWaveCommand
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
            if (StartLaserNumber <= 0)
            {
                MessageBox.Show("起始激光功率不允许为0");
                return;
            }
            if (EndLaserNumber <= StartLaserNumber)
            {
                MessageBox.Show("起始激光功率不允许小于开始激光功率");
                return;
            }
            if (!_Laser532WaveViewPort.IsConnected)
            {
                MessageBox.Show("请先选择端口");
                return;
            }
            IsStepBoFeng = false;
            isStartButton++;
            MaxLaseCurrentindex = 0;
            MinLaseCurrentindex = 0;
            if (isStartButton == 1)
            {
                if (_Laser532WaveViewPort.IsConnected)
                {
                    while (_Laser532WaveViewPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _Laser532WaveViewPort.SetCurrentTEControlTemperatureValue(25);
                    Thread.Sleep(1000);
                   // _Laser532WaveViewPort.SetCurrentTEControlTemperatureValue(25);
                    while (_Laser532WaveViewPort.IsBusy)
                    {
                        Thread.Sleep(1);
                    }
                    _Laser532WaveViewPort.SetCurrentLaserCurrentValue(0);
                }
                frequency = 0;
                //GetCurrentTecTempValue = 0;
                IsStart = false;
                IsDone = false;
                index = 0;
                //i = 0;
                _LaserCurrentValue = 0;
                //updateCollectionTimer.Start();
                //GetTECTemperTimer.Start();
                TecTemperPointCollection.Clear();
                LaserPowerPointCollection.Clear();
                LaserElectricPointCollection.Clear();
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    Point TemperPoint = new Point(index, 250);
                    TecTemperPointCollection.Add(TemperPoint);
                    Point LaserPowerPoint = new Point(index, 0);
                    LaserPowerPointCollection.Add(LaserPowerPoint);
                    Point LaserElectricPoint = new Point(index, StartLaserNumber);
                    LaserElectricPointCollection.Add(LaserElectricPoint);
                });
                //GetLaserPowerTimer.Start();
                //SetLaseCurrentTimer.Start();
                //ProcessThread = new Thread(ProcessData);
                //ProcessThread.IsBackground = true;
                //ProcessThread.Priority = ThreadPriority.Lowest;
                //if (!ProcessThread.IsAlive)
                //{
                //    ProcessThread.Start();
                //}
                 //StepPoint = Math.Abs(LaserNumberTime / 1000);
                _WaveViewSubWind.COMNumberCoeffComboBox.IsEnabled = false;
                _WaveViewSubWind.FileNameTextBox11.IsEnabled = false;
                _WaveViewSubWind.FileNameTextBox1111.IsEnabled = false;
                _WaveViewSubWind.FileNameTextBox13.IsEnabled = false;
                _WaveViewSubWind.FileNameTextBox13_Copy.IsEnabled = false;
                _WaveViewSubWind.StartWaveButton.Content = "停止";
                _WaveViewSubWind.StartWaveButton.Foreground = Brushes.Red;
            }
            else {
                isStartButton = 0;
                frequency = 0;
                IsDone = false;
                //GetLaserPowerTimer.Stop();
                //SetLaseCurrentTimer.Stop();
                //updateCollectionTimer.Stop();
                //if (ProcessThread.IsAlive)
                //{
                //    ProcessThread.Abort();
                //}
                _WaveViewSubWind.COMNumberCoeffComboBox.IsEnabled = true;
                _WaveViewSubWind.FileNameTextBox11.IsEnabled = true;
                _WaveViewSubWind.FileNameTextBox1111.IsEnabled = true;
                _WaveViewSubWind.FileNameTextBox13.IsEnabled = true;
                _WaveViewSubWind.FileNameTextBox13_Copy.IsEnabled = true;
                _WaveViewSubWind.StartWaveButton.Content = "开始";
                _WaveViewSubWind.StartWaveButton.Foreground = Brushes.White;
            }
            

        }
        public bool CanExecuteStartWaveCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region GetLaserPowerCommand
        public ICommand GetLaserPowerCommand
        {
            get
            {
                if (_GetLaserPowerCommand == null)
                {
                    _GetLaserPowerCommand = new RelayCommand(ExecuteGetLaserPowerCommand, CanExecuteGetLaserPowerCommand);
                }
                return _GetLaserPowerCommand;
            }
        }
        public void ExecuteGetLaserPowerCommand(object parameter)
        {
            if (_Laser532WaveViewPort.IsConnected)
            {
                while (_Laser532WaveViewPort.IsBusy)
                {
                    Thread.Sleep(1);
                }
                _Laser532WaveViewPort.GetCurrentLaserLightPowerValueValue();
                GetLaserPowerValue = _Laser532WaveViewPort.LaserPowerValue * 0.1;
            }

        }
        public bool CanExecuteGetLaserPowerCommand(object parameter)
        {
            return true;
        }

        #endregion
        #endregion


        private void _Laser532WaveViewPort_UpdateCommOutput()
        {
            LaserPowerValue = _Laser532WaveViewPort.LaserPowerValue;
            LaserElectricValue = _Laser532WaveViewPort.LaserElectricValue;
            TecTemperValue = _Laser532WaveViewPort.TecTemperValue;
            GetCurrentTecTempValue = TecTemperValue * 0.1;
            Thread.Sleep(10);
            Console.WriteLine(LaserPowerValue);
            Console.WriteLine(LaserElectricValue);
            Console.WriteLine(GetCurrentTecTempValue);
        }

        void WriteFile()
        {
            string[] testdata = new string[LaserPowerPointCollection.Count];
            for (int y = 0; y < testdata.Length; y++)
            {
                //testdata[y] = LaserPowerPointCollection[y].Y.ToString();
                testdata[y] = LaserPowerPointCollection[y].Y.ToString() + "," +
                LaserElectricPointCollection[y].Y.ToString() + "," +
                TecTemperPointCollection[y].Y.ToString();
            }
            //if (!Directory.Exists(path))
            //{
            //    Directory.CreateDirectory(path);//创建新路径
            //}
            string time = StrDate();
            string csvname = LaserNumber + "_" + time + ".csv";
            string csvPath = Workspace.This.GetManualPath(csvname);
            if (string.IsNullOrEmpty(csvPath))
            {
                return;
            }
            string pngPath = System.IO.Path.ChangeExtension(csvPath, ".png");
            File.WriteAllLines(csvPath, testdata, System.Text.Encoding.UTF8);
            _WaveViewSubWind.SavePng(pngPath);
            MessageBox.Show("记录完成，数据保存成功");
            //System.Diagnostics.Process.Start("explorer.exe", path);
        }

        public void ProcessData()
        {
            while (true)
            {
                Thread.Sleep(100);
                if (!IsStart)
                {
                    //if (GetCurrentTecTempValue < 24.9 || GetCurrentTecTempValue > 25.1)
                    //{
                    //    _Laser532WaveViewPort.SetCurrentTEControlTemperatureValue(25);
                    //    Thread.Sleep(5000);
                    //}
                    //else 
                    if (GetCurrentTecTempValue >= 24.8 && GetCurrentTecTempValue <= 25.2)
                    {
                        //updateCollectionTimer.Start();
                        //GetLaserPowerTimer.Start();
                        //SetLaseCurrentTimer.Start();
                        MaxLaseCurrentindex = 0;
                        IsStart = true;
                        IsDone = true;
                    }
                    else
                    {
                        _Laser532WaveViewPort.SetCurrentTEControlTemperatureValue(25);
                        Thread.Sleep(100);
                    }

                }
                //else
                //{
                //    //if (!IsStepBoFeng)
                //    //{
                //    //    if (MaxLaseCurrentindex > 1)
                //    //    {
                //    //        MaxLaseCurrentindex = 0;
                //    //        IsStepBoFeng = true;

                //    //    }
                //    //}
                //    //else 
                //    //{
                //    //    if (index > step1)
                //    //    {
                //    //        if (GetCurrentTecTempValue < 29.9 || GetCurrentTecTempValue > 30.1)
                //    //        {
                //    //            _Laser532WaveViewPort.SetCurrentTEControlTemperatureValue(30);
                //    //            Thread.Sleep(1000);
                //    //        }
                //    //        //else if (GetCurrentTecTempValue > 29.9 || GetCurrentTecTempValue < 30.1)
                //    //        //{
                //    //        //    if (!IsStepBoFeng)
                //    //        //    {
                //    //        //        if (MaxLaseCurrentindex > 1)
                //    //        //        {
                //    //        //            MaxLaseCurrentindex = 0;
                //    //        //            IsStepBoFeng = true;
                //    //        //        }
                //    //        //    }
                //    //        //}
                //    //    }

                //    //}
                   
                //}
            }
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
        public Laser532WaveViewPort Laser532WaveViewPort
        {
            get
            {
                return _Laser532WaveViewPort;
            }
            set
            {
                _Laser532WaveViewPort = value;
            }

        }

        public EnumerableDataSource<Point> LaserPower
        {
            get
            {
                return _LaserPower;
            }
            set
            {
                _LaserPower = value;
                RaisePropertyChanged("LaserPower");
            }

        }

        public EnumerableDataSource<Point> LaserElectric
        {
            get
            {
                return _LaserElectric;
            }
            set
            {
                _LaserElectric = value;
                RaisePropertyChanged("LaserElectric");
            }
        }

        public EnumerableDataSource<Point> TecTemper
        {
            get
            {
                return _TecTemper;
            }
            set
            {
                _TecTemper = value;
                RaisePropertyChanged("TecTemper");
            }
        }

        public EnumerableDataSource<Point> HistoryLaserPower
        {
            get
            {
                return _HistoryLaserPower;
            }
            set
            {
                _HistoryLaserPower = value;
                RaisePropertyChanged("HistoryLaserPower");
            }

        }

        public EnumerableDataSource<Point> HistoryLaserElectric
        {
            get
            {
                return _HistoryLaserElectric;
            }
            set
            {
                _HistoryLaserElectric = value;
                RaisePropertyChanged("HistoryLaserElectric");
            }
        }

        public EnumerableDataSource<Point> HistoryTecTemper
        {
            get
            {
                return _HistoryTecTemper;
            }
            set
            {
                _HistoryTecTemper = value;
                RaisePropertyChanged("HistoryTecTemper");
            }
        }

        public double LaserPowerValue { get => _LaserPowerValue; set => _LaserPowerValue = value; }
        public double LaserElectricValue { get => _LaserElectricValue; set => _LaserElectricValue = value; }
        public double TecTemperValue { get => _TecTemperValue; set => _TecTemperValue = value; }
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
        public double GetLaserPowerValue
        {
            get
            {
                return _GetLaserPowerValue;
            }
            set
            {
                _GetLaserPowerValue = value;
                RaisePropertyChanged("GetLaserPowerValue");
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
        public int EndLaserNumber
        {
            get
            {
                return _EndLaserNumber;
            }
            set
            {
                _EndLaserNumber = value;
                RaisePropertyChanged("EndLaserNumber");
            }
        }
        public int StartLaserNumber
        {
            get
            {
                return _StartLaserNumber;
            }
            set
            {
                _StartLaserNumber = value;
                RaisePropertyChanged("StartLaserNumber");
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

        public double LaserNumberStep
        {
            get
            {
                return _LaserNumberStep;
            }
            set
            {
                _LaserNumberStep = value;
                RaisePropertyChanged("LaserNumberStep");
            }
        }

        public int LaserNumberTime
        {
            get
            {
                return _LaserNumberTime;
            }
            set
            {
                _LaserNumberTime = value;
                RaisePropertyChanged("LaserNumberTime");
            }
        }

        public ObservableCollection<string> COMNumberCoeffOptions
        {
            get
            {
                return _COMNumberCoeffOptions;
            }
        }
        public ObservableCollection<int> BAUDNumberCoeffOptions
        {
            get
            {
                return _BAUDNumberCoeffOptions;
            }
        }
        public int SelectedBAUDNumberCoeff
        {
            get
            {
                return _SelectedBAUDNumberCoeff;
            }
            set
            {
                if (_SelectedBAUDNumberCoeff != value)
                {
                    _SelectedBAUDNumberCoeff = value;
                    RaisePropertyChanged("SelectedBAUDNumberCoeff");
                }
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
                    MaxLaseCurrentindex = 0;
                    MinLaseCurrentindex = 0;
                    isStartButton = 0;
                    frequency = 0;
                    IsStart = false;
                    IsDone = false;
                    IsStepBoFeng = false;
                    index = 0;
                    //i = 0;
                    _LaserCurrentValue = 0;
                    if (_Laser532WaveViewPort.IsConnected == false)
                    {
                        _Laser532WaveViewPort.SearchPort(value, SelectedBAUDNumberCoeff);
                    }
                    else
                    {
                        _Laser532WaveViewPort.Dispose();
                        _Laser532WaveViewPort.SearchPort(value, SelectedBAUDNumberCoeff);
                    }
                    if (_Laser532WaveViewPort.IsConnected)
                    {
                        while (_Laser532WaveViewPort.IsBusy)
                        {
                            Thread.Sleep(1);
                        }
                        _Laser532WaveViewPort.SetCurrentLaserCurrentValue(0);
                    }
                    RaisePropertyChanged("SelectedCOMNumberCoeff");
                }
            }
        }
    }
}
