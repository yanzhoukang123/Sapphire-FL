using Azure.ScannerTestJig.View.Upgrade;
using Azure.WPF.Framework;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ConvetLib;
using System.Diagnostics;
using System.Collections.ObjectModel;
using Azure.APDCalibrationBench;
using System.IO.Ports;
using System.Threading;
//hex文件格式详细说明 
//Hex文件的INTEL格式:这是Intel公司提出的按地址排列的数据信息,数据宽度为字节,所有数
//据使用16进制数字表示.
//这是一个例子:
//:10008000AF5F67F0602703E0322CFA92007780C361
//:1000900089001C6B7EA7CA9200FE10D2AA00477D81
//:0B00A00080FA92006F3600C3A00076CB
//:00000001FF
//第一行,":"符号表明记录的开始. 后面的两个字符表明记录的长度,这里是10h. 后面的四个字
//符给出调入的地址,这里是0080h. 后面的两个字符表明记录的类型;
//0 数据记录 1 记录文件结束 2 扩展段地址记录 3 开始段地址记录 4 扩展线性地址记录 5
//开始线性地址记录
//后面则是真正的数据记录, 最后两位是校验和检查,它加上前面所有的数据和为0.
//最后一行特殊,总是写成这个样子.
//扩展Intel Hex的格式(最大1M): 由于普通的Intel的Hex记录文件只能记录64K的地址范围,
//所以大于64K的地址数据要靠扩展Intel Hex格式的文件来记录.对于扩展形式Hex文件,在每
//一个64K段的开始加上扩展的段地址规定,下面的数据地址均在这个段内,除非出现新的段地址
//定义.
//一个段地址 定义的格式如下:
//起始符 长度 起始地址 扩展段标示 扩展段序号 无用 累加和
//: 02 0000 02 3000 EC
//段地址的标识符是第四组数据02,表示扩展地址段的定义,再后面的以为HEX数表示段的数目,
//上面的定义为3,表示段地址是3,所以下面的数据地址是3 + XX(XX是64K段内的地址)  
namespace Azure.ScannerTestJig.ViewModule.Upgrade
{
    class UpgradeViewModule : ViewModelBase
    {

        #region Private data
        private string[] _AvailablePorts;
        private ObservableCollection<string> _COMNumberCoeffOptions = new ObservableCollection<string>();
        private ObservableCollection<int> _BAUDNumberCoeffOptions = new ObservableCollection<int>();
        private ObservableCollection<string> _AddressNumberCoeffOptions = new ObservableCollection<string>();
        private string _SelectedCOMNumberCoeff;
        private int _SelectedBAUDNumberCoeff;
        private string _SelectedAddressNumberCoeff;
        private UpgradePort _UpgradePort = null;
        public UpgradeSub _upgradeSub = null;
        private RelayCommand _WriteInstruct485Command = null;
        private RelayCommand _WriteInstructCleanCommand = null;
        private RelayCommand _WriteFrist1Command = null;
        private RelayCommand _WriteFrist2Command = null;
        private RelayCommand _WriteFrist3Command = null;
        private RelayCommand _StartCommand = null;
        #endregion
        private RelayCommand _ConvertCommand = null;
        private RelayCommand _MergeCommand = null;
        private RelayCommand _SplitCommand = null;
        private RelayCommand _OpenBin1Command = null;
        private RelayCommand _OpenBin2Command = null;
        private RelayCommand _MergeBinCommand = null;
        private RelayCommand _SplitBinCommand = null;
        private string _BinPath1 = "";
        private string _BinPath2 = "";
        private string _StartAddress = "0";
        private string _Bin1Lenth = "";
        private string _Bin2Lenth = "";
        private string _GetInstruct485 = "";
        private string _Address485 = "22";
        private string _SetInstruct485 = "";
        private string _StrCurrentState = "";
        private string _StartAddress485 = "0";
        private string _EndAddress485 = "7400";
        private bool _UpgradeProcess = true;
        string path = Directory.GetCurrentDirectory() + @"\File\";
        private ConvertHelp convertHelp = new ConvertHelp();
        public UpgradeViewModule()
        {

        }
        internal void ExcutePortConnectCommand()
        {
            _StrCurrentState = "";
            _AvailablePorts = SerialPort.GetPortNames();
            if (_AvailablePorts.Length == 0)
            {
                MessageBox.Show("未找到串口设备！", "连接错误", MessageBoxButton.OK, MessageBoxImage.Error);
                _upgradeSub = new UpgradeSub();
                _upgradeSub.Title = Workspace.This.Owner.Title + "-Upgrade Kit";
                _upgradeSub.ShowDialog();
                _upgradeSub = null;
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
            if (SelectedBAUDNumberCoeff == 0)
            {
                SelectedBAUDNumberCoeff = _BAUDNumberCoeffOptions[0];
            }

            if (_AvailablePorts.Length > 0)
            {
                if (_UpgradePort == null || _UpgradePort.IsConnected == false)
                {
                    _UpgradePort = new UpgradePort();
                    _UpgradePort.DataProcessUpdateCommOutput += _UpgradePort_DataProcessUpdateCommOutput;
                }
            }
            _upgradeSub = new UpgradeSub();
            _upgradeSub.Title = Workspace.This.Owner.Title + "-Upgrade Kit";
            _upgradeSub.ShowDialog();
            _upgradeSub = null;
        }

        private void _UpgradePort_DataProcessUpdateCommOutput()
        {
            GetInstruct485 = _UpgradePort.GetByte;
            // Console.WriteLine(_UpgradePort.GetByte);
        }

        public void InitIVControls()
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);//创建新路径
            }
        }

        #region pri
        public string BinPath1
        {
            get
            {
                return _BinPath1;
            }
            set
            {
                if (_BinPath1 != value)
                {
                    _BinPath1 = value;
                    RaisePropertyChanged("BinPath1");
                }
            }

        }

        public string BinPath2
        {
            get
            {
                return _BinPath2;
            }
            set
            {
                if (_BinPath2 != value)
                {
                    _BinPath2 = value;
                    RaisePropertyChanged("BinPath2");
                }
            }

        }
        public string StartAddress
        {
            get
            {
                return _StartAddress;
            }
            set
            {
                if (_StartAddress != value)
                {
                    _StartAddress = value;
                    RaisePropertyChanged("StartAddress");
                }
            }

        }
        public string Bin1Lenth
        {
            get
            {
                return _Bin1Lenth;
            }
            set
            {
                if (_Bin1Lenth != value)
                {
                    _Bin1Lenth = value;
                    RaisePropertyChanged("Bin1Lenth");
                }
            }

        }
        public string Bin2Lenth
        {
            get
            {
                return _Bin2Lenth;
            }
            set
            {
                if (_Bin2Lenth != value)
                {
                    _Bin2Lenth = value;
                    RaisePropertyChanged("Bin2Lenth");
                }
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
                    if (_UpgradePort.IsConnected == false)
                    {
                        _UpgradePort.SearchPort(value, SelectedBAUDNumberCoeff);

                    }
                    else
                    {
                        _UpgradePort.Dispose();
                        _UpgradePort.SearchPort(value, SelectedBAUDNumberCoeff);
                    }
                    if (!string.IsNullOrEmpty(Address485) && Address485 != "0")
                    {
                        int offset = convertHelp.String2Hex(Address485);
                        byte off = (byte)offset;
                        _UpgradePort.PUBAddress = off;
                    }
                    RaisePropertyChanged("SelectedCOMNumberCoeff");
                }
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

        public string StrCurrentState
        {
            get
            {
                return _StrCurrentState;
            }
            set
            {
                _StrCurrentState = value;
                RaisePropertyChanged("StrCurrentState");
            }
        }

        public UpgradePort UpgradePort
        {
            get
            {
                return _UpgradePort;
            }
            set
            {
                _UpgradePort = value;
            }

        }

        public string Address485
        {
            get
            {
                return _Address485;
            }
            set
            {
                _Address485 = value;
                if (!string.IsNullOrEmpty(value) && value != "0")
                {
                    int offset = convertHelp.String2Hex(value);
                    byte off = (byte)offset;
                    _UpgradePort.PUBAddress = off;
                }
                RaisePropertyChanged("Address485");
            }
        }

        public bool UpgradeProcess
        {
            get
            {
                return _UpgradeProcess;
            }
            set
            {
                _UpgradeProcess = value;
                RaisePropertyChanged("UpgradeProcess");
            }
        }

        public string StartAddress485
        {
            get
            {
                return _StartAddress485;
            }
            set
            {
                _StartAddress485 = value;
                RaisePropertyChanged("StartAddress485");
            }
        }

        public string EndAddress485
        {
            get
            {
                return _EndAddress485;
            }
            set
            {
                _EndAddress485 = value;
                RaisePropertyChanged("EndAddress485");
            }
        }
        #endregion

        #region ConvertCommand 
        public ICommand ConvertCommand
        {
            get
            {
                if (_ConvertCommand == null)
                {
                    _ConvertCommand = new RelayCommand(ExecuteConvertCommand, CanExecuteConvertCommand);
                }

                return _ConvertCommand;
            }
        }
        public void ExecuteConvertCommand(object parameter)
        {
            // 创建路径选择对话框实例
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Hex文件 (*.hex)|*.hex|Bin 文件 (*.bin)|*.bin"; // 设置过滤器，仅显示指定类型的文件
            // 显示对话框，并判断用户是否点击了确定按钮
            bool? result = openFileDialog.ShowDialog();
            if (result == true)
            {
                string time = StrDate();
                string selectedFilePath = openFileDialog.FileName;
                // 在这里处理选中的文件路径
                string fristName = Path.GetFileName(selectedFilePath);
                string LastName = Path.GetExtension(fristName);
                if (LastName == ".hex") //16进制文件
                {
                    string szBinPath = Path.ChangeExtension(fristName, "bin");
                    if (!convertHelp.Hex2Bin(selectedFilePath, path + szBinPath))
                    {
                        MessageBox.Show("转换失败,请检查文件");
                    }

                }
                else    //二进制文件
                {

                    string szBinPath = Path.ChangeExtension(fristName, "hex");
                    if (!convertHelp.Bin2Hex(path + szBinPath, selectedFilePath))
                    {
                        MessageBox.Show("转换失败,请检查文件");
                    }

                }
                MessageBox.Show("文件转换完成!", "提示");
                System.Diagnostics.Process.Start("explorer.exe", path);
            }
        }

        public bool CanExecuteConvertCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region SplitCommand 
        public ICommand SplitCommand
        {
            get
            {
                if (_SplitCommand == null)
                {
                    _SplitCommand = new RelayCommand(ExecuteSplitCommand, CanExecuteSplitCommand);
                }

                return _SplitCommand;
            }
        }
        public void ExecuteSplitCommand(object parameter)
        {
            BinFile binFileMerge = new BinFile();
            binFileMerge.GSplit.Visibility = Visibility.Visible;
            binFileMerge.GMerge.Visibility = Visibility.Hidden;
            binFileMerge.ShowDialog();
        }

        public bool CanExecuteSplitCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region MergeCommand 
        public ICommand MergeCommand
        {
            get
            {
                if (_MergeCommand == null)
                {
                    _MergeCommand = new RelayCommand(ExecuteMergeCommand, CanExecuteMergeCommand);
                }

                return _MergeCommand;
            }
        }
        public void ExecuteMergeCommand(object parameter)
        {
            BinFile binFileMerge = new BinFile();
            binFileMerge.GSplit.Visibility = Visibility.Hidden;
            binFileMerge.GMerge.Visibility = Visibility.Visible;
            binFileMerge.ShowDialog();
        }

        public bool CanExecuteMergeCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region OpenBin1Command 
        public ICommand OpenBin1Command
        {
            get
            {
                if (_OpenBin1Command == null)
                {
                    _OpenBin1Command = new RelayCommand(ExecuteOpenBin1Command, CanExecuteOpenBin1Command);
                }

                return _OpenBin1Command;
            }
        }
        long fileLength1 = 0;
        public void ExecuteOpenBin1Command(object parameter)
        {
            // 创建路径选择对话框实例
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Bin 文件 (*.bin)|*.bin"; // 设置过滤器，仅显示指定类型的文件
            // 显示对话框，并判断用户是否点击了确定按钮
            bool? result = openFileDialog.ShowDialog();
            if (result == true)
            {
                BinPath1 = openFileDialog.FileName;
                FileInfo fileInfo = new FileInfo(BinPath1);
                fileLength1 = fileInfo.Length;
                Bin1Lenth = fileLength1.ToString("X");

            }
        }

        public bool CanExecuteOpenBin1Command(object parameter)
        {
            return true;
        }

        #endregion

        #region OpenBin2Command 
        public ICommand OpenBin2Command
        {
            get
            {
                if (_OpenBin2Command == null)
                {
                    _OpenBin2Command = new RelayCommand(ExecuteOpenBin2Command, CanExecuteOpenBin2Command);
                }

                return _OpenBin2Command;
            }
        }
        public void ExecuteOpenBin2Command(object parameter)
        {
            // 创建路径选择对话框实例
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Bin 文件 (*.bin)|*.bin"; // 设置过滤器，仅显示指定类型的文件
            // 显示对话框，并判断用户是否点击了确定按钮
            bool? result = openFileDialog.ShowDialog();
            if (result == true)
            {
                BinPath2 = openFileDialog.FileName;
                FileInfo fileInfo = new FileInfo(BinPath2);
                long fileLength = fileInfo.Length;
                Bin2Lenth = fileLength.ToString("X");
            }
        }

        public bool CanExecuteOpenBin2Command(object parameter)
        {
            return true;
        }

        #endregion

        #region MergeBinCommand 
        public ICommand MergeBinCommand
        {
            get
            {
                if (_MergeBinCommand == null)
                {
                    _MergeBinCommand = new RelayCommand(ExecuteMergeBinCommand, CanExecuteMergeBinCommand);
                }

                return _MergeBinCommand;
            }
        }
        public void ExecuteMergeBinCommand(object parameter)
        {
            if (string.IsNullOrEmpty(BinPath1) || !File.Exists(BinPath1))
            {
                MessageBox.Show("Bin1路径错误，请选择正确的文件");
                return;
            }
            if (string.IsNullOrEmpty(BinPath2) || !File.Exists(BinPath2))
            {
                MessageBox.Show("Bin2路径错误，请选择正确的文件");
                return;
            }
            string LastName1 = Path.GetFileName(BinPath1);
            string LastName2 = Path.GetFileName(BinPath2);
            byte[] buffer = convertHelp.MergeFile(BinPath1, BinPath2);
            FileStream fBin = new FileStream(path + "Merge_" + LastName1 + "+" + LastName2, FileMode.Create); //创建文件BIN文件
            BinaryWriter BinWrite = new BinaryWriter(fBin); //二进制方式打开文件
            BinWrite.Write(buffer, 0, buffer.Length); //写入数据
            BinWrite.Flush();//释放缓存
            BinWrite.Close();//关闭文件
            MessageBox.Show("文件合并完成!", "提示");
            System.Diagnostics.Process.Start("explorer.exe", path);
        }

        public bool CanExecuteMergeBinCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region SplitBinCommand 
        public ICommand SplitBinCommand
        {
            get
            {
                if (_SplitBinCommand == null)
                {
                    _SplitBinCommand = new RelayCommand(ExecuteSplitBinCommand, CanExecuteSplitBinCommand);
                }

                return _SplitBinCommand;
            }
        }
        public void ExecuteSplitBinCommand(object parameter)
        {
            if (string.IsNullOrEmpty(BinPath1) || !File.Exists(BinPath1))
            {
                MessageBox.Show("Bin路径错误，请选择正确的文件");
                return;
            }
            string LastName = Path.GetFileName(BinPath1);
            int offset = convertHelp.String2Hex(StartAddress);
            if (offset > fileLength1)
            {
                MessageBox.Show("起始位置不应大于结束位置！");
                return;
            }
            byte[] buffer = convertHelp.SplitFile(BinPath1, offset);
            FileStream fBin = new FileStream(path + "Split_" + LastName, FileMode.Create); //创建文件BIN文件
            BinaryWriter BinWrite = new BinaryWriter(fBin); //二进制方式打开文件
            BinWrite.Write(buffer, 0, buffer.Length); //写入数据
            BinWrite.Flush();//释放缓存
            BinWrite.Close();//关闭文件
            MessageBox.Show("文件分割完成!", "提示");
            System.Diagnostics.Process.Start("explorer.exe", path);
        }

        public bool CanExecuteSplitBinCommand(object parameter)
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
            if (_UpgradePort.IsConnected)
            {
                if (!string.IsNullOrEmpty(SetInstruct485))
                {
                    _UpgradePort.SetInstruct485(SetInstruct485);
                }
            }

        }
        public bool CanExecuteWriteInstruct485Command(object parameter)
        {
            return true;
        }

        #endregion

        #region StartCommand
        public ICommand StartCommand
        {
            get
            {
                if (_StartCommand == null)
                {
                    _StartCommand = new RelayCommand(ExecuteStartCommand, CanExecuteStartCommand);
                }
                return _StartCommand;
            }
        }
        Thread WorkThread = null;
        public void ExecuteStartCommand(object parameter)
        {
            WorkThread = new Thread(DataProcessThreadMetoh);
            WorkThread.Priority = ThreadPriority.Highest;
            WorkThread.IsBackground = true;
            WorkThread.Start();
        }

        void DataProcessThreadMetoh()
        {
            int timesleep = 1000;
            if (_UpgradePort.IsConnected)
            {
                int StartAddressbyte485, EndAddressbyte485;

                if (!string.IsNullOrEmpty(EndAddress485))
                {
                    EndAddressbyte485 = convertHelp.String2Hex(EndAddress485);
                }
                else
                {
                    StrCurrentState = "数据长度不正确";
                    return;
                }

                UpgradeProcess = false;
                byte[] Reset = new byte[] { 0x3a, UpgradePort.PUBAddress, 0x01, 0xFC, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
                byte[] Clean = new byte[] { 0x3a, UpgradePort.PUBAddress, 0x01, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
                byte[] Flash = new byte[] { 0x3a, UpgradePort.PUBAddress, 0x01, 0xFE, 0x01, 0x00, 0x00, 0x00, 0x00, 0x3b };
                byte[] Check = new byte[] { 0x3a, UpgradePort.PUBAddress, 0x01, 0xFD, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
                UpgradePort.SetByteNull();
                ///
                //step 5.1 开始复位
                ///
                StrCurrentState = "开始复位...";
                Thread.Sleep(timesleep);
                UpgradePort.SetInstruct485(Reset);
                while (UpgradePort.ReResetByte == null)
                {
                    StrCurrentState = "等待复位...";
                    Thread.Sleep(timesleep);
                }


                ///
                //step 5.2 清除扇区
                ///
                StrCurrentState = "开始清除扇区...";
                Thread.Sleep(timesleep);
                UpgradePort.SetInstruct485(Clean);
                while (UpgradePort.ReCleanByte == null)
                {
                    StrCurrentState = "等待清除扇区...";
                    Thread.Sleep(timesleep);
                }
                //延时1000ms
                Thread.Sleep(1000);

                ///
                //step5.3 写APP.bin程序到Flash
                ///
                StrCurrentState = "选择Bin程序...";
                Thread.Sleep(timesleep);
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Bin 文件 (*.bin)|*.bin";
                bool? result = openFileDialog.ShowDialog();
                string binpath = "";
                if (result == true)
                {
                    binpath = openFileDialog.FileName;
                }
                else
                {
                    UpgradeProcess = true;
                    StrCurrentState = "取消了Bin文件选择，升级终止";
                    return;
                }
                int CRC = 0;
                int splitPackLength = 512;//从0开始计算的,数据长度
                int Length = 0;
                byte poly = 0x07;
                List<byte[]> list = convertHelp.GetPackList(binpath, splitPackLength, "0", ref CRC, ref Length, poly);
                if (Length > EndAddressbyte485)
                {
                    UpgradeProcess = true;
                    StrCurrentState = "文件的长度:0x" + Length.ToString("X") + " > 数据长度:0x" + EndAddress485 + "，升级终止";
                    return;
                }
                int count = list.Count;
                int DataLength = 0;
                for (int i = 0; i < count; i++)
                {
                    byte[] _Flash = Flash;
                    byte[] Pack = list[i];
                    DataLength += Pack.Length;

                    //写入数据长度
                    byte[] LengthbyteArray = BitConverter.GetBytes(Pack.Length);
                    Array.Reverse(LengthbyteArray);
                    byte Length1 = LengthbyteArray[2];
                    byte Length2 = LengthbyteArray[3];
                    _Flash[5] = Length1;
                    _Flash[6] = Length2;

                    //写入地址
                    byte[] DatabyteArray = BitConverter.GetBytes(DataLength);
                    Array.Reverse(DatabyteArray);
                    byte Address1 = DatabyteArray[2];
                    byte Address2 = DatabyteArray[3];
                    _Flash[7] = Address1;
                    _Flash[8] = Address2;


                    // 写入数据
                    byte[] newPack = InsertByteArray(_Flash, Pack, 9);
                    UpgradePort.SetInstruct485(newPack);
                    while (UpgradePort.ReWriteFlashByte == null)
                    {
                        //StrCurrentState = "等待写入Bin程序到扇区...";
                        Thread.Sleep(timesleep);
                    }
                    UpgradePort.ReWriteFlashByte = null;//清除上次收到的回复
                    Thread.Sleep(1500);
                    StrCurrentState = string.Format("正在写入包 {0}/{1}...", i, count);
                }



                ///
                //step6 校验程序
                ///
                StrCurrentState = "开始校验程序...";
                Thread.Sleep(timesleep);

                //程序总长度
                byte[] Totallength = BitConverter.GetBytes(Length);
                Array.Reverse(Totallength);
                byte Totallength1 = Totallength[2];
                byte Totallength2 = Totallength[3];
                Check[5] = Totallength1;
                Check[6] = Totallength2;

                //CRC
                byte[] crc = BitConverter.GetBytes(CRC);
                Array.Reverse(crc);
                byte crc1 = crc[2];
                byte crc2 = crc[3];
                Check[8] = crc2;


                UpgradePort.SetInstruct485(Check);
                while (UpgradePort.ReCheckByte == null)
                {
                    StrCurrentState = "等待写APP.bin程序扇区...";
                    Thread.Sleep(timesleep);
                }
                if (UpgradePort.ReCheckByte[8] == 0x01)
                {
                    StrCurrentState = "校验通过...";
                }
                else
                {
                    StrCurrentState = string.Format("校验失败，校验码={0}...", "PC计算的校验码");
                    UpgradeProcess = true;
                    return;
                }
                Thread.Sleep(timesleep);


                ///
                //step7 重新复位
                ///
                StrCurrentState = "重新复位...";
                Thread.Sleep(timesleep);
                UpgradePort.SetInstruct485(Reset);
                while (UpgradePort.ReResetByte == null)
                {
                    StrCurrentState = "等待复位...";
                    Thread.Sleep(timesleep);
                }
                StrCurrentState = "升级成功...";
                Thread.Sleep(timesleep);
                UpgradeProcess = true;
            }
            else
            {
                StrCurrentState = "端口没有打开...";
            }
        }
        public bool CanExecuteStartCommand(object parameter)
        {
            return true;
        }

        #endregion

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

        public byte[] InsertByteArray(byte[] sourceArray, byte[] insertArray, int index)
        {
            byte[] result = new byte[sourceArray.Length + insertArray.Length];

            // 将源数组的前半部分复制到结果数组中
            Array.Copy(sourceArray, 0, result, 0, index);

            // 将要插入的数组复制到结果数组中
            Array.Copy(insertArray, 0, result, index, insertArray.Length);

            // 将源数组的后半部分复制到结果数组中
            Array.Copy(sourceArray, index, result, index + insertArray.Length, sourceArray.Length - index);

            return result;
        }


        #region
        public ICommand WriteInstructCleanCommand
        {
            get
            {
                if (_WriteInstructCleanCommand == null)
                {
                    _WriteInstructCleanCommand = new RelayCommand(ExecuteWriteInstructCleanCommand, CanExecuteWriteInstructCleanCommand);
                }
                return _WriteInstructCleanCommand;
            }
        }
        public void ExecuteWriteInstructCleanCommand(object parameter)
        {
            try
            {
                if (_UpgradePort == null)
                {
                    MessageBox.Show("先选择串口");
                    return;
                }
                type = "Clean";
                WorkThread = new Thread(DataProcessThreadMetoh1);
                WorkThread.Priority = ThreadPriority.Highest;
                WorkThread.IsBackground = true;
                WorkThread.Start();

            }
            catch
            {
                MessageBox.Show("先选择串口");
            }
        }
        public bool CanExecuteWriteInstructCleanCommand(object parameter)
        {
            return true;
        }


        public ICommand WriteFrist1Command
        {
            get
            {
                if (_WriteFrist1Command == null)
                {
                    _WriteFrist1Command = new RelayCommand(ExecuteWriteFrist1Command, CanExecuteWriteFrist1Command);
                }
                return _WriteFrist1Command;
            }
        }
        public void ExecuteWriteFrist1Command(object parameter)
        {
            try
            {
                if (_UpgradePort == null)
                {
                    MessageBox.Show("先选择串口");
                    return;
                }
                type = "1";
                WorkThread = new Thread(DataProcessThreadMetoh1);
                WorkThread.Priority = ThreadPriority.Highest;
                WorkThread.IsBackground = true;
                WorkThread.Start();


            }
            catch
            {
                MessageBox.Show("先选择串口");
            }

        }
        public bool CanExecuteWriteFrist1Command(object parameter)
        {
            return true;
        }


        public ICommand WriteFrist2Command
        {
            get
            {
                if (_WriteFrist2Command == null)
                {
                    _WriteFrist2Command = new RelayCommand(ExecuteWriteFrist2Command, CanExecuteWriteFrist2Command);
                }
                return _WriteFrist2Command;
            }
        }
        public void ExecuteWriteFrist2Command(object parameter)
        {
            try
            {
                if (_UpgradePort == null)
                {
                    MessageBox.Show("先选择串口");
                    return;
                }
                type = "2";
                WorkThread = new Thread(DataProcessThreadMetoh1);
                WorkThread.Priority = ThreadPriority.Highest;
                WorkThread.IsBackground = true;
                WorkThread.Start();

            }
            catch
            {
                MessageBox.Show("先选择串口");
            }

        }
        public bool CanExecuteWriteFrist2Command(object parameter)
        {
            return true;
        }

        public ICommand WriteFrist3Command
        {
            get
            {
                if (_WriteFrist3Command == null)
                {
                    _WriteFrist3Command = new RelayCommand(ExecuteWriteFrist3Command, CanExecuteWriteFrist3Command);
                }
                return _WriteFrist3Command;
            }
        }
        public void ExecuteWriteFrist3Command(object parameter)
        {
            try
            {
                if (_UpgradePort == null)
                {
                    MessageBox.Show("先选择串口");
                    return;
                }
                type = "3";
                WorkThread = new Thread(DataProcessThreadMetoh1);
                WorkThread.Priority = ThreadPriority.Highest;
                WorkThread.IsBackground = true;
                WorkThread.Start();

            }
            catch
            {
                MessageBox.Show("先选择串口");
            }
        }
        public bool CanExecuteWriteFrist3Command(object parameter)
        {
            return true;
        }

        string type="";
        void DataProcessThreadMetoh1()
        {
            int timesleep = 1000;
            if (_UpgradePort.IsConnected)
            {
                int StartAddressbyte485, EndAddressbyte485;

                if (!string.IsNullOrEmpty(EndAddress485))
                {
                    EndAddressbyte485 = convertHelp.String2Hex(EndAddress485);
                }
                else
                {
                    StrCurrentState = "数据长度不正确";
                    return;
                }

                UpgradeProcess = false;
                byte[] Reset = new byte[] { 0x3a, UpgradePort.PUBAddress, 0x01, 0xFC, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
                byte[] Clean = new byte[] { 0x3a, UpgradePort.PUBAddress, 0x01, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
                byte[] Flash = new byte[] { 0x3a, UpgradePort.PUBAddress, 0x01, 0xFE, 0x01, 0x00, 0x00, 0x00, 0x00, 0x3b };
                byte[] Check = new byte[] { 0x3a, UpgradePort.PUBAddress, 0x01, 0xFD, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3b };
                UpgradePort.SetByteNull();


                if (type == "Clean")
                {
                    ///
                    //step 5.2 清除扇区
                    ///
                    StrCurrentState = "开始清除扇区...";
                    Thread.Sleep(timesleep);
                    UpgradePort.SetInstruct485(Clean);
                    while (UpgradePort.ReCleanByte == null)
                    {
                        StrCurrentState = "等待清除扇区...";
                        Thread.Sleep(timesleep);
                    }
                    //延时1000ms
                    Thread.Sleep(1000);

                }
                else
                {
                    ///
                    //step5.3 写APP.bin程序到Flash
                    ///
                    StrCurrentState = "选择Bin程序...";
                    Thread.Sleep(timesleep);
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    openFileDialog.Filter = "Bin 文件 (*.bin)|*.bin";
                    bool? result = openFileDialog.ShowDialog();
                    string binpath = "";
                    if (result == true)
                    {
                        binpath = openFileDialog.FileName;
                    }
                    else
                    {
                        UpgradeProcess = true;
                        StrCurrentState = "取消了Bin文件选择，升级终止";
                        return;
                    }
                    int CRC = 0;
                    int splitPackLength = 512;//从0开始计算的,数据长度
                    int Length = 0;
                    byte poly = 0x07;
                    List<byte[]> list = convertHelp.GetPackList(binpath, splitPackLength, "0", ref CRC, ref Length, poly);
                    if (Length > EndAddressbyte485)
                    {
                        UpgradeProcess = true;
                        StrCurrentState = "文件的长度:0x" + Length.ToString("X") + " > 数据长度:0x" + EndAddress485 + "，升级终止";
                        return;
                    }
                    int count = list.Count;
                    int DataLength = 0;
                    for (int i = 0; i < count; i++)
                    {
                        byte[] _Flash = Flash;
                        byte[] Pack = list[i];
                        DataLength += Pack.Length;

                        //写入数据长度
                        byte[] LengthbyteArray = BitConverter.GetBytes(Pack.Length);
                        Array.Reverse(LengthbyteArray);
                        byte Length1 = LengthbyteArray[2];
                        byte Length2 = LengthbyteArray[3];
                        _Flash[5] = Length1;
                        _Flash[6] = Length2;

                        //写入地址
                        byte[] DatabyteArray = BitConverter.GetBytes(DataLength);
                        Array.Reverse(DatabyteArray);
                        byte Address1 = DatabyteArray[2];
                        byte Address2 = DatabyteArray[3];
                        _Flash[7] = Address1;
                        _Flash[8] = Address2;

                        if (type == "1")
                        {
                            if (i == 0)
                            {
                                // 写入数据
                                byte[] newPack = InsertByteArray(_Flash, Pack, 9);
                                UpgradePort.SetInstruct485(newPack);
                                while (UpgradePort.ReWriteFlashByte == null)
                                {
                                    StrCurrentState = "等待写入Bin程序到扇区...";
                                    Thread.Sleep(timesleep);
                                }
                                UpgradePort.ReWriteFlashByte = null;//清除上次收到的回复
                                Thread.Sleep(1500);
                                StrCurrentState = string.Format("第一包写入成功...");
                                return;
                            }


                        }
                        if (type == "2")
                        {
                            if (i == 1)
                            {
                                // 写入数据
                                byte[] newPack = InsertByteArray(_Flash, Pack, 9);
                                UpgradePort.SetInstruct485(newPack);
                                while (UpgradePort.ReWriteFlashByte == null)
                                {
                                    StrCurrentState = "等待写入Bin程序到扇区...";
                                    Thread.Sleep(timesleep);
                                }
                                UpgradePort.ReWriteFlashByte = null;//清除上次收到的回复
                                Thread.Sleep(1500);
                                StrCurrentState = string.Format("第二包写入成功...");
                                return;
                            }


                        }

                        if (type == "3")
                        {
                            if (i == 2)
                            {
                                // 写入数据
                                byte[] newPack = InsertByteArray(_Flash, Pack, 9);
                                UpgradePort.SetInstruct485(newPack);
                                while (UpgradePort.ReWriteFlashByte == null)
                                {
                                    StrCurrentState = "等待写入Bin程序到扇区...";
                                    Thread.Sleep(timesleep);
                                }
                                UpgradePort.ReWriteFlashByte = null;//清除上次收到的回复
                                Thread.Sleep(1500);
                                StrCurrentState = string.Format("第三包写入成功...");
                                return;
                            }

                        }
                    }

                }
            }
            else
            {
                StrCurrentState = "端口没有打开...";
            }
        }
        #endregion
        //GetPackList(string BinPath, int SplitPackConst, string StratAddress, ref int crc, byte poly)
    }
}
