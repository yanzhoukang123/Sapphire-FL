using Azure.APDCalibrationBench;
using Azure.ScannerTestJig.View._532LaserModel;
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

namespace Azure.ScannerTestJig.ViewModule._532LaserModel
{
    class WaveViewChart532LaserViewModel:ViewModelBase
    {
        #region Private data
        private string[] _AvailablePorts;
        private ObservableCollection<string> _COMNumberCoeffOptions = new ObservableCollection<string>();
        private ObservableCollection<int> _PhotodiodeVoltageCorrespondingToLaserPowerOptions = new ObservableCollection<int>();
        private string _SelectedCOMNumberCoeff;
        private int _SelectedPhotodiodeVoltageCorrespondingToLaserPower;
        public WaveView532LaserSubWind _WaveView532LaserSubWind = null;
        public Display532LaserData _Display532LaserData = null;
        private Laser532ModelViewPort _Laser532ModelViewPort = null;
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
        private EnumerableDataSource<Point> _LaserPower = null;
        private EnumerableDataSource<Point> _PDVoltage = null;
        private EnumerableDataSource<Point> _InternalPDVoltage = null;
        private EnumerableDataSource<Point> _TecTemper = null;
        private EnumerableDataSource<Point> _HistoryLaserPower = null;
        private EnumerableDataSource<Point> _HistoryPDVoltage = null;
        private EnumerableDataSource<Point> _HistoryInternalPDVoltage = null;
        private EnumerableDataSource<Point> _HistoryTecTemper = null;
        public VoltagePointCollection532Laser LaserPowerPointCollection;
        public VoltagePointCollection532Laser PDVoltagePointCollection;
        public VoltagePointCollection532Laser InternalPDPointCollection;
        public VoltagePointCollection532Laser TecTemperPointCollection;
        public VoltagePointCollection532Laser HistoryLaserPowerPointCollection;
        public VoltagePointCollection532Laser HistoryPDVoltagePointCollection;
        public VoltagePointCollection532Laser HistoryInternalPDPointCollection;
        public VoltagePointCollection532Laser HistoryTecTemperPointCollection;
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
        List<Dictionary<double, double>> CalList = new List<Dictionary<double, double>>();
        #endregion
        public void ExcutePortConnectCommand()
        {
            try
            {
                if(_PhotodiodeVoltageCorrespondingToLaserPowerOptions.Count==0)
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
                    _WaveView532LaserSubWind = new WaveView532LaserSubWind();
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
                if (_AvailablePorts.Length > 0)
                {
                    if (_Laser532ModelViewPort == null || _Laser532ModelViewPort.IsConnected == false)
                    {
                        _Laser532ModelViewPort = new Laser532ModelViewPort();
                        _Laser532ModelViewPort.PUBAddress = 0x01;
                        _Laser532ModelViewPort.Instruct485UpdateCommOutput += _Laser532ModelViewPort_Instruct485UpdateCommOutput;
                        _Laser532ModelViewPort.InteriorPDUpdateCommOutput += _Laser532ModelViewPort_InteriorPDUpdateCommOutput1;
                        _Laser532ModelViewPort.LaserOpticalPowerUpdateCommOutput += _Laser532ModelViewPort_LaserOpticalPowerUpdateCommOutput;
                        _Laser532ModelViewPort.LaserPowerUpdateCommOutput += _Laser532ModelViewPort_LaserPowerUpdateCommOutput;
                        _Laser532ModelViewPort.PDUpdateCommOutput += _Laser532ModelViewPort_InteriorPDUpdateCommOutput;
                        _Laser532ModelViewPort.LaserElectricUpdateCommOutput += _Laser532ModelViewPort_LaserElectricUpdateCommOutput;
                        _Laser532ModelViewPort.PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutput += _Laser532ModelViewPort_PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutput; ;

                    }
                    //SampleValueLaserElectric = new double[step2];
                    LaserPowerPointCollection = new VoltagePointCollection532Laser();
                    PDVoltagePointCollection = new VoltagePointCollection532Laser();
                    InternalPDPointCollection = new VoltagePointCollection532Laser();
                    TecTemperPointCollection = new VoltagePointCollection532Laser();
                    HistoryLaserPowerPointCollection = new VoltagePointCollection532Laser();
                    HistoryPDVoltagePointCollection = new VoltagePointCollection532Laser();
                    HistoryInternalPDPointCollection = new VoltagePointCollection532Laser();
                    HistoryTecTemperPointCollection = new VoltagePointCollection532Laser();
                    _WaveView532LaserSubWind = new WaveView532LaserSubWind();
                    LaserPower = new EnumerableDataSource<Point>(LaserPowerPointCollection);
                    LaserPower.SetXMapping(x => x.X);
                    LaserPower.SetYMapping(y => y.Y);
                    PDVoltage = new EnumerableDataSource<Point>(PDVoltagePointCollection);
                    PDVoltage.SetXMapping(x => x.X);
                    PDVoltage.SetYMapping(y => y.Y);
                    InternalPDVoltage = new EnumerableDataSource<Point>(InternalPDPointCollection);
                    InternalPDVoltage.SetXMapping(x => x.X);
                    InternalPDVoltage.SetYMapping(y => y.Y);
                    TecTemper = new EnumerableDataSource<Point>(TecTemperPointCollection);
                    TecTemper.SetXMapping(x => x.X);
                    TecTemper.SetYMapping(y => y.Y);

                    HistoryLaserPower = new EnumerableDataSource<Point>(HistoryLaserPowerPointCollection);
                    HistoryLaserPower.SetXMapping(x => x.X);
                    HistoryLaserPower.SetYMapping(y => y.Y);
                    HistoryPDVoltage = new EnumerableDataSource<Point>(HistoryPDVoltagePointCollection);
                    HistoryPDVoltage.SetXMapping(x => x.X);
                    HistoryPDVoltage.SetYMapping(y => y.Y);
                    HistoryInternalPDVoltage = new EnumerableDataSource<Point>(HistoryInternalPDPointCollection);
                    HistoryInternalPDVoltage.SetXMapping(x => x.X);
                    HistoryInternalPDVoltage.SetYMapping(y => y.Y);
                    HistoryTecTemper = new EnumerableDataSource<Point>(HistoryTecTemperPointCollection);
                    HistoryTecTemper.SetXMapping(x => x.X);
                    HistoryTecTemper.SetYMapping(y => y.Y);
                    DataProcessThread = new Thread(DataProcessThreadMetoh);
                    DataProcessThread.Priority = ThreadPriority.Highest;
                    DataProcessThread.IsBackground = true;
                    DataProcessThread.Start();


                    _WaveView532LaserSubWind.Title = Workspace.This.Owner.Title + "-532 Laser Model Kit";
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

        private void _Laser532ModelViewPort_InteriorPDUpdateCommOutput1()
        {
            InteriorPDValue = _Laser532ModelViewPort.InternalPDVoltage;
        }

        private void _Laser532ModelViewPort_LaserOpticalPowerUpdateCommOutput()
        {
            LaserOpticalPowerValue = _Laser532ModelViewPort.LaserPowerValue;
        }

        private void _Laser532ModelViewPort_PhotodiodeVoltageCorrespondingToLaserPowerUpdateCommOutput()
        {
            PhotodiodeVoltageCorrespondingToLaserPowerValue = _Laser532ModelViewPort.PhotodiodeVoltageCorrespondingToLaserPower;
        }

        private void _Laser532ModelViewPort_LaserElectricUpdateCommOutput()
        {
            //LaserElectricValue = _Laser532ModelViewPort.LaserElectricValue;
           PD532Value = _Laser532ModelViewPort.LaserElectricValue;
        }

        private void _Laser532ModelViewPort_InteriorPDUpdateCommOutput()
        {
            PD532Value = _Laser532ModelViewPort.PDVoltage;
        }

        private void _Laser532ModelViewPort_LaserPowerUpdateCommOutput()
        {
            GetCurrentTecTempValue = _Laser532ModelViewPort.TecTemperValue*0.1;
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
                HistoryLaserPowerPointCollection.Clear();
                HistoryPDVoltagePointCollection.Clear();
                HistoryInternalPDPointCollection.Clear();
                HistoryTecTemperPointCollection.Clear();
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
                    _Display532LaserData = new Display532LaserData();
                    _Display532LaserData.fileName.Content = fName;
                    _Display532LaserData.Title = Workspace.This.Owner.Title + "-532 History WaveView Kit";
                    _Display532LaserData.Show();
                    _Display532LaserData = null;

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
                if (Data.Length > 1)
                {
                    Thread.Sleep(50);
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        double laserPower = Convert.ToDouble(Data[0]);
                        double pdvoltage = Convert.ToDouble(Data[1]);
                        double internalpd = Convert.ToDouble(Data[2]);
                        double tectemper = Convert.ToDouble(Data[3]);

                        Point LaserPowerPoint = new Point(h, laserPower);
                        HistoryLaserPowerPointCollection.Add(LaserPowerPoint);
                        Point PdvoltagePoint = new Point(h, pdvoltage);
                        HistoryPDVoltagePointCollection.Add(PdvoltagePoint);
                        Point InternalpdPoint = new Point(h, internalpd);
                        HistoryInternalPDPointCollection.Add(InternalpdPoint);
                        Point TectemperPoint = new Point(h, tectemper);
                        HistoryTecTemperPointCollection.Add(TectemperPoint);
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
                testdata[y] = LaserPowerPointCollection[y].Y.ToString() + "," +
                PDVoltagePointCollection[y].Y.ToString() + "," +
                InternalPDPointCollection[y].Y.ToString() + "," +
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
            _WaveView532LaserSubWind.SavePng(pngPath);
            Thread _temp = new Thread(newProcess);
            _temp.IsBackground = true;
            _temp.Start();


            //string time = StrDate();
            //string csvname = LaserNumber + "_" + time + ".csv";
            //string pngname = LaserNumber + "_" + time + ".png";
            //string pathname = Workspace.This.GetManualPath(csvname);
            //File.WriteAllLines(path + pathname, testdata, System.Text.Encoding.UTF8);
            //_WaveView532LaserSubWind.SavePng(path + pngname);
            //MessageBox.Show("记录完成，数据保存成功\n" + pathname);
            //System.Diagnostics.Process.Start("explorer.exe", path);

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
            CalList.Clear();
            isStartButton++;
            index = 0;
            IsStart = false;
            if (isStartButton == 1)
            {
                try
                {
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
                        string[] DataValue = strData.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        for (int h = 0; h < DataValue.Length; h++)
                        {
                            string[] Data = DataValue[h].Split(new string[] { "," }, StringSplitOptions.None);
                            if (Data.Length > 1)
                            {
                                string sPower = Data[0];
                                string sTime = Data[1];
                                double f2;
                                if (!double.TryParse(sPower, out f2))
                                {
                                    continue;
                                }
                                double power = Convert.ToDouble(sPower);
                                double time = Convert.ToDouble(sTime);
                                Dictionary<double, double> caldata = new Dictionary<double, double>();
                                caldata.Add(power, time);
                                CalList.Add(caldata);
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("缺少532校准.csv文件");
                        return;
                    }
                }
                catch
                {
                    MessageBox.Show("文件被占用");
                    return;
                }
                LaserPowerPointCollection.Clear();
                PDVoltagePointCollection.Clear();
                InternalPDPointCollection.Clear();
                TecTemperPointCollection.Clear();
                //Application.Current.Dispatcher.Invoke((Action)delegate
                //{
                //    Point LaserPowerPoint = new Point(index, 0);
                //    LaserPowerPointCollection.Add(LaserPowerPoint);
                //    Point PDVoltagePoint = new Point();
                //    PDVoltagePointCollection.Add(PDVoltagePoint);
                //    Point InternalPDPoint = new Point();
                //    InternalPDPointCollection.Add(InternalPDPoint);
                //    Point TecTemperPoint = new Point(index, 250);
                //    TecTemperPointCollection.Add(TecTemperPoint);
                //});
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
                CalList.Clear();
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
                _Laser532ModelViewPort.SetCurrentLaserCurrentValue(LaserCurrentValue);
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
                    for (int i = 0; i < CalList.Count; i++)
                    {
                        // Dictionary<double, double> _temp = new Dictionary<double, double>();
                        _Laser532ModelViewPort.SetCurrentLaserCurrentValue(0);//关闭激光
                        Thread.Sleep(2500);
                        double LaserPower = CalList[i].ElementAt(0).Key;
                        double Time = CalList[i].ElementAt(0).Value;
                        _Laser532ModelViewPort.SetCurrentLaserLightPowerValueValue(LaserPower);
                        PeriodIndex = 0;
                        Thread.Sleep(100);
                        while (!_Laser532ModelViewPort.DataComplete)
                        {
                            Thread.Sleep(1000);
                            _Laser532ModelViewPort.SetCurrentLaserLightPowerValueValue(LaserPower);
                            PeriodIndex++;
                            if (PeriodIndex > 5)
                            {
                                MessageBox.Show("光功率 "+ LaserPower + " mW连续5次下发失败");
                                //return;
                            }
                        }
                        PeriodIndex = 0;
                        while (PeriodIndex <= Time * 60) //不满足条件时代表当前周期完成了 
                        {
                            index++;
                            PeriodIndex++;
                            _Laser532ModelViewPort.GetCurrentLaserLightPowerValueValue();
                            Thread.Sleep(200);
                            _Laser532ModelViewPort.GetCurrentTECActualTemperatureValue();
                            Thread.Sleep(200);
                            _Laser532ModelViewPort.GetPDVoltage();
                            Thread.Sleep(200);
                            _Laser532ModelViewPort.GetInternalPDVoltage();
                            Thread.Sleep(400);
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                Point LaserPowerPoint = new Point(index, LaserOpticalPowerValue);
                                LaserPowerPointCollection.Add(LaserPowerPoint);
                                Point PDVoltagePoint = new Point(index, PD532Value);
                                PDVoltagePointCollection.Add(PDVoltagePoint);
                                Point InternalPDPoint = new Point(index, InteriorPDValue);
                                InternalPDPointCollection.Add(InternalPDPoint);
                                Point TecTemperPoint = new Point(index, GetCurrentTecTempValue);
                                TecTemperPointCollection.Add(TecTemperPoint);
                            });
                        }
                    }

                    _Laser532ModelViewPort.SetCurrentLaserCurrentValue(0);//关闭激光
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
            string[] testdata = new string[LaserPowerPointCollection.Count];
            for (int y = 0; y < testdata.Length; y++)
            {
                testdata[y] = LaserPowerPointCollection[y].Y.ToString() + "," +
                 PDVoltagePointCollection[y].Y.ToString() + "," +
                 InternalPDPointCollection[y].Y.ToString() + "," +
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
            _WaveView532LaserSubWind.SavePng(pngPath);
            MessageBox.Show("记录完成，数据保存成功");
            //System.Diagnostics.Process.Start("explorer.exe", path);
        }
        public Laser532ModelViewPort Laser532ModelViewPort
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
                        _Laser532ModelViewPort.SearchPort(value,115200);
                        Thread.Sleep(1000);
                       // _Laser532ModelViewPort.SetCurrentLaserCurrentValue(0);//关闭激光
                    }
                    else
                    {
                        _Laser532ModelViewPort.Dispose();
                        _Laser532ModelViewPort.SearchPort(value,115200);
                    }
                    RaisePropertyChanged("SelectedCOMNumberCoeff");
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

        public EnumerableDataSource<Point> PDVoltage
        {
            get
            {
                return _PDVoltage;
            }
            set
            {
                _PDVoltage = value;
                RaisePropertyChanged("PDVoltage");
            }
        }

        public EnumerableDataSource<Point> InternalPDVoltage
        {
            get
            {
                return _InternalPDVoltage;
            }
            set
            {
                _InternalPDVoltage = value;
                RaisePropertyChanged("InternalPDVoltage");
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

        public EnumerableDataSource<Point> HistoryPDVoltage
        {
            get
            {
                return _HistoryPDVoltage;
            }
            set
            {
                _HistoryPDVoltage = value;
                RaisePropertyChanged("HistoryPDVoltage");
            }
        }

        public EnumerableDataSource<Point> HistoryInternalPDVoltage
        {
            get
            {
                return _HistoryInternalPDVoltage;
            }
            set
            {
                _HistoryInternalPDVoltage = value;
                RaisePropertyChanged("HistoryInternalPDVoltage");
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
    } 
}
