using Azure.Avocado.EthernetCommLib;
using Azure.WPF.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Hywire.Avocado.AnalogTest
{
    class Workspace : ViewModelBase
    {
        public static Workspace This { get; } = new Workspace();

        private EthernetController _CommController;

        private Workspace()
        {
            _CommController = new EthernetController();
            _CommController.OnRecievedSingleSampleData += _CommController_OnRecievedSingleSampleData;
        }

        private void _CommController_OnRecievedSingleSampleData()
        {
            SampleValueChA = _CommController.SampleValueChA;
            SampleValueChB = _CommController.SampleValueChB;
            SampleValueChC = _CommController.SampleValueChC;
        }

        private uint _SampleValueChA;
        private uint _SampleValueChB;
        private uint _SampleValueChC;

        public uint SampleValueChA
        {
            get { return _SampleValueChA; }
            set
            {
                if (_SampleValueChA != value)
                {
                    _SampleValueChA = value;
                    RaisePropertyChanged(nameof(SampleValueChA));
                }
            }
        }
        public uint SampleValueChB
        {
            get { return _SampleValueChB; }
            set
            {
                if (_SampleValueChB != value)
                {
                    _SampleValueChB = value;
                    RaisePropertyChanged(nameof(SampleValueChB));
                }
            }
        }
        public uint SampleValueChC
        {
            get { return _SampleValueChC; }
            set
            {
                if (_SampleValueChC != value)
                {
                    _SampleValueChC = value;
                    RaisePropertyChanged(nameof(SampleValueChC));
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
            switch(obj.ToString())
            {
                case "Connect":
                    _CommController.Connect(new System.Net.IPAddress(new byte[] { 192, 168, 1, 110 }), 5000,8000, new System.Net.IPAddress(new byte[] { 192, 168, 1, 100 }));
                    if (_CommController.IsConnected)
                    {
                        MessageBox.Show("Connected");
                    }
                    else
                    {
                        MessageBox.Show("Failed\n" + _CommController.ErrorMessage);
                    }
                    break;
                case "Start":
                    _CommController.TriggerSingleScan();
                    break;
            }
        }

        private bool CanExecuteSetCmd(object obj)
        {
            switch (obj.ToString())
            {
                case "Connect":
                    return !_CommController.IsConnected;
                case "Start":
                    return _CommController.IsConnected;
                default:
                    return false;
            }
        }
    }
}
