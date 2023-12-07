using Azure.Avocado.EthernetCommLib;
using Azure.CommunicationLib;
using Azure.WPF.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Hywire.TCPClientTest
{
    class Workspace : ViewModelBase
    {
        public static Workspace This { get; } = new Workspace();

        private byte _SourceIPHH = 192;
        private byte _SourceIPHL = 168;
        private byte _SourceIPLH = 1;
        private byte _SourceIPLL = 110;
        private int _PortNum = 5000;
        private EthernetController _Controller;
        private System.Timers.Timer _UpdateTimer;
        private double _ReceivingRate;
        public Workspace()
        {
            _Controller = new EthernetController();
            _Controller.OnReceivedScanningData += _Controller_OnReceivedData;
            _ReceivedData = new StringBuilder();

            _UpdateTimer = new System.Timers.Timer();
            _UpdateTimer.AutoReset = true;
            _UpdateTimer.Elapsed += _UpdateTimer_Elapsed;
            _UpdateTimer.Interval = 1000;
            _UpdateTimer.Start();
        }

        private void _UpdateTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            ReceivingRate = _Controller.ReadingRate;
        }

        private void _Controller_OnReceivedData()
        {
            byte[] buf = new byte[_Controller.ReceivingBuf.StoredSize];
            _Controller.ReceivingBuf.ReadDataOut(buf, 0, buf.Length);

            //_ReceivedData.Append(ByteArrayToStringConverter.Run(buf, " ", true));
            //RaisePropertyChanged(nameof(ReceivedData));
        }

        public byte SourceIPHH
        {
            get { return _SourceIPHH; }
            set
            {
                if (_SourceIPHH != value)
                {
                    _SourceIPHH = value;
                    RaisePropertyChanged(nameof(SourceIPHH));
                }
            }
        }
        public byte SourceIPHL
        {
            get { return _SourceIPHL; }
            set
            {
                if (_SourceIPHL != value)
                {
                    _SourceIPHL = value;
                    RaisePropertyChanged(nameof(SourceIPHL));
                }
            }
        }
        public byte SourceIPLH
        {
            get { return _SourceIPLH; }
            set
            {
                if (_SourceIPLH != value)
                {
                    _SourceIPLH = value;
                    RaisePropertyChanged(nameof(SourceIPLH));
                }
            }
        }
        public byte SourceIPLL
        {
            get { return _SourceIPLL; }
            set
            {
                if (_SourceIPLL != value)
                {
                    _SourceIPLL = value;
                    RaisePropertyChanged(nameof(SourceIPLL));
                }
            }
        }
        public int PortNum
        {
            get { return _PortNum; }
            set
            {
                if (_PortNum != value)
                {
                    _PortNum = value;
                    RaisePropertyChanged(nameof(PortNum));
                }
            }
        }

        private string _SendingData;
        private StringBuilder _ReceivedData;

        public string SendingData
        {
            get { return _SendingData; }
            set
            {
                if (_SendingData != value)
                {
                    _SendingData = value;
                    RaisePropertyChanged(nameof(SendingData));
                }
            }
        }
        public string ReceivedData
        {
            get { return _ReceivedData.ToString(); }
        }
        public double ReceivingRate
        {
            get { return _ReceivingRate; }
            set
            {
                if (_ReceivingRate != value)
                {
                    _ReceivingRate = value;
                    RaisePropertyChanged(nameof(ReceivingRate));
                }
            }
        }
        private RelayCommand _SetCmd;
        public ICommand SetCmd
        {
            get
            {
                if (_SetCmd == null)
                {
                    _SetCmd = new RelayCommand(ExecuteSetCmd, CanExecuteSetCmd);
                }
                return _SetCmd;
            }
        }

        private void ExecuteSetCmd(object obj)
        {
            switch (obj.ToString())
            {
                case "Connect":
                    if (_Controller.IsConnected) { return; }
                    System.Net.IPAddress ip = new System.Net.IPAddress(new byte[] { SourceIPHH, SourceIPHL, SourceIPLH, SourceIPLL });
                    _Controller.Connect(ip, PortNum,8000, new System.Net.IPAddress(new byte[] { 192, 168, 1, 100 }));
                    if (_Controller.IsConnected)
                    {
                        MessageBox.Show("Connected");
                    }
                    else
                    {
                        MessageBox.Show("Not Connected");
                    }
                    break;
                case "Send":
                    if (!_Controller.IsConnected)
                    {
                        MessageBox.Show("Connect Ethernet first!");
                        return;
                    }
                    string[] chars = SendingData.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    byte[] b = new byte[chars.Length];
                    for(int i=0;i<chars.Length;i++)
                    {
                        b[i] = Convert.ToByte(chars[i], 16);
                    }
                    _Controller.SendBytes(b);
                    break;
                case "Clear":
                    break;
            }
        }

        private bool CanExecuteSetCmd(object obj)
        {
            return true;
        }
    }
}
