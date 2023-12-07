using Azure.Avocado.EthernetCommLib;
using Azure.Avocado.MotionLib;
using Azure.WPF.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Hywire.Avocado.MotionTest
{
    class Workspace : ViewModelBase
    {
        #region Private Fields
        private MotionController _MotionController;
        private EthernetController _EthernetController;
        private bool _IsConnected;
        private MotorTypes _SelectedType;
        private MotionViewModel _SelectedMotionVm;
        private Dictionary<MotorTypes, MotionViewModel> _MotionViewModels;
        private string _SelectedTorque;
        private string _SelectedMode;
        #endregion Private Fields

        #region Constructor
        public Workspace()
        {
            _EthernetController = new EthernetController();
            MotionTypeOptions = new List<MotorTypes>();
            MotionTypeOptions.Add(MotorTypes.X);
            MotionTypeOptions.Add(MotorTypes.Y);
            MotionTypeOptions.Add(MotorTypes.Z);
            MotionTypeOptions.Add(MotorTypes.W);
            _SelectedType = MotionTypeOptions[0];
            _MotionViewModels = new Dictionary<MotorTypes, MotionViewModel>();
            _MotionViewModels.Add(MotorTypes.X, new MotionViewModel(MotorTypes.X));
            _MotionViewModels.Add(MotorTypes.Y, new MotionViewModel(MotorTypes.Y));
            _MotionViewModels.Add(MotorTypes.Z, new MotionViewModel(MotorTypes.Z));
            _MotionViewModels.Add(MotorTypes.W, new MotionViewModel(MotorTypes.W));
            _SelectedMotionVm = _MotionViewModels[MotorTypes.X];

            Initialize();
        }

        private void Initialize()
        {
            _MotionViewModels[MotorTypes.X].StartSpeed = 0.1;
            _MotionViewModels[MotorTypes.X].TopSpeed = 20;
            _MotionViewModels[MotorTypes.X].AccVal = 100;
            _MotionViewModels[MotorTypes.X].DccVal = 100;
            _MotionViewModels[MotorTypes.X].Coeff = 150;

            _MotionViewModels[MotorTypes.Y].StartSpeed = 0.1;
            _MotionViewModels[MotorTypes.Y].TopSpeed = 5;
            _MotionViewModels[MotorTypes.Y].AccVal = 5000;
            _MotionViewModels[MotorTypes.Y].DccVal = 5000;
            _MotionViewModels[MotorTypes.Y].Coeff = 1600;
        }

        private void _Controller_OnQueryUpdated()
        {
            _MotionViewModels[MotorTypes.X].IsEnabled = _MotionController.CrntState[MotorTypes.X].Enabled;
            _MotionViewModels[MotorTypes.X].AtFwdLimit = _MotionController.CrntState[MotorTypes.X].AtFwdLimit;
            _MotionViewModels[MotorTypes.X].AtBwdLimit = _MotionController.CrntState[MotorTypes.X].AtBwdLimit;
            _MotionViewModels[MotorTypes.X].AtHome = _MotionController.CrntState[MotorTypes.X].AtHome;
            _MotionViewModels[MotorTypes.X].IsBusy = _MotionController.CrntState[MotorTypes.X].IsBusy;
            _MotionViewModels[MotorTypes.Y].IsEnabled = _MotionController.CrntState[MotorTypes.Y].Enabled;
            _MotionViewModels[MotorTypes.Y].AtFwdLimit = _MotionController.CrntState[MotorTypes.Y].AtFwdLimit;
            _MotionViewModels[MotorTypes.Y].AtBwdLimit = _MotionController.CrntState[MotorTypes.Y].AtBwdLimit;
            _MotionViewModels[MotorTypes.Y].AtHome = _MotionController.CrntState[MotorTypes.Y].AtHome;
            _MotionViewModels[MotorTypes.Y].IsBusy = _MotionController.CrntState[MotorTypes.Y].IsBusy;
            _MotionViewModels[MotorTypes.Z].IsEnabled = _MotionController.CrntState[MotorTypes.Z].Enabled;
            _MotionViewModels[MotorTypes.Z].AtFwdLimit = _MotionController.CrntState[MotorTypes.Z].AtFwdLimit;
            _MotionViewModels[MotorTypes.Z].AtBwdLimit = _MotionController.CrntState[MotorTypes.Z].AtBwdLimit;
            _MotionViewModels[MotorTypes.Z].AtHome = _MotionController.CrntState[MotorTypes.Z].AtHome;
            _MotionViewModels[MotorTypes.Z].IsBusy = _MotionController.CrntState[MotorTypes.Z].IsBusy;
            _MotionViewModels[MotorTypes.W].IsEnabled = _MotionController.CrntState[MotorTypes.W].Enabled;
            _MotionViewModels[MotorTypes.W].AtFwdLimit = _MotionController.CrntState[MotorTypes.W].AtFwdLimit;
            _MotionViewModels[MotorTypes.W].AtBwdLimit = _MotionController.CrntState[MotorTypes.W].AtBwdLimit;
            _MotionViewModels[MotorTypes.W].AtHome = _MotionController.CrntState[MotorTypes.W].AtHome;
            _MotionViewModels[MotorTypes.W].IsBusy = _MotionController.CrntState[MotorTypes.W].IsBusy;

            _MotionViewModels[MotorTypes.X].CrntPos = Math.Round(_MotionController.CrntPositions[MotorTypes.X] / _MotionViewModels[MotorTypes.X].Coeff, 3);
            _MotionViewModels[MotorTypes.Y].CrntPos = Math.Round(_MotionController.CrntPositions[MotorTypes.Y] / _MotionViewModels[MotorTypes.Y].Coeff, 3);
            _MotionViewModels[MotorTypes.Z].CrntPos = Math.Round(_MotionController.CrntPositions[MotorTypes.Z] / _MotionViewModels[MotorTypes.Z].Coeff, 3);
            _MotionViewModels[MotorTypes.W].CrntPos = Math.Round(_MotionController.CrntPositions[MotorTypes.W] / _MotionViewModels[MotorTypes.W].Coeff, 3);
        }
        #endregion Constructor

        #region Public Properties
        public static Workspace This { get; } = new Workspace();
        public bool IsConnected
        {
            get { return _IsConnected; }
            set
            {
                if (_IsConnected != value)
                {
                    _IsConnected = value;
                    RaisePropertyChanged(nameof(IsConnected));
                }
            }
        }
        public List<MotorTypes> MotionTypeOptions { get; }
        public MotorTypes SelectedType
        {
            get { return _SelectedType; }
            set
            {
                if (_SelectedType != value)
                {
                    _SelectedType = value;
                    RaisePropertyChanged(nameof(SelectedType));
                    SelectedMotionVm = _MotionViewModels[SelectedType];
                }
            }
        }
        public MotionViewModel SelectedMotionVm
        {
            get { return _SelectedMotionVm; }
            set
            {
                if (_SelectedMotionVm != value)
                {
                    _SelectedMotionVm = value;
                    RaisePropertyChanged(nameof(SelectedMotionVm));
                }
            }
        }
        public List<string> TorqueOptions { get; }
        public List<string> ModeOptions { get; }
        public string SelectedTorque
        {
            get { return _SelectedTorque; }
            set
            {
                if (_SelectedTorque != value)
                {
                    _SelectedTorque = value;
                    RaisePropertyChanged(nameof(SelectedTorque));
                }
            }
        }
        public string SelectedMode
        {
            get { return _SelectedMode; }
            set
            {
                if (_SelectedMode != value)
                {
                    _SelectedMode = value;
                    RaisePropertyChanged(nameof(SelectedMode));
                }
            }
        }
        #endregion Public Properties

        #region Connect Command
        private RelayCommand _ConnectCmd;
        public ICommand ConnectCmd
        {
            get
            {
                if (_ConnectCmd == null)
                {
                    _ConnectCmd = new RelayCommand(ExecuteConnectCmd, CanExecuteConnectCmd);
                }
                return _ConnectCmd;
            }
        }

        private void ExecuteConnectCmd(object obj)
        {
            if (!IsConnected)
            {
                System.Net.IPAddress serverIp = new System.Net.IPAddress(new byte[] { 192, 168, 1, 110 });
                int portNum = 5000;
                System.Net.IPAddress localIp = new System.Net.IPAddress(new byte[] { 192, 168, 1, 100 });
                IsConnected = _EthernetController.Connect(serverIp, portNum, 8000, localIp);
                if (IsConnected)
                {
                    _MotionController = new MotionController(_EthernetController);
                    _MotionController.AutoQuery = true;
                    _MotionController.OnQueryUpdated += _Controller_OnQueryUpdated;
                    MessageBox.Show("Controller is connected.");
                }
                else
                {
                    MessageBox.Show("Connection attempt failed.\n" + _EthernetController.ErrorMessage);
                }
            }
            else
            {
                MessageBox.Show("Controller is already connected!");
            }
        }

        private bool CanExecuteConnectCmd(object obj)
        {
            return true;
        }
        #endregion Connect Command

        #region Set Command
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
            string parameter = obj.ToString();
            int val;
            switch (parameter)
            {
                case "Read":
                    if(_MotionController.GetMotionInfo(MotorTypes.X | MotorTypes.Y | MotorTypes.Z | MotorTypes.W) == false)
                    {
                        MessageBox.Show("Read failed");
                    }
                    break;
                case "Enable":
                    if(_MotionController.SetEnables(SelectedType, new bool[] { !SelectedMotionVm.IsEnabled }) == false)
                    {
                        MessageBox.Show("Failed");
                    }
                    break;
                case "Home":
                    if (SelectedMotionVm.IsBusy)
                    {
                        return;
                    }
                    int startSpd = (int)Math.Round(SelectedMotionVm.StartSpeed * SelectedMotionVm.Coeff);
                    int topSpd = (int)Math.Round(SelectedMotionVm.TopSpeed * SelectedMotionVm.Coeff);
                    int acc = (int)Math.Round(SelectedMotionVm.AccVal * SelectedMotionVm.Coeff);
                    int dcc = (int)Math.Round(SelectedMotionVm.DccVal * SelectedMotionVm.Coeff);
                    _MotionController.HomeMotion(SelectedType, new int[] { startSpd }, new int[] { topSpd }, new int[] { acc }, new int[] { dcc }, false);
                    break;
                case "Start":
                    if (SelectedMotionVm.IsBusy)
                    {
                        _MotionController.SetStart(SelectedType, new bool[] { false });
                    }
                    else
                    {
                        startSpd = (int)Math.Round(SelectedMotionVm.StartSpeed * SelectedMotionVm.Coeff);
                        topSpd = (int)Math.Round(SelectedMotionVm.TopSpeed * SelectedMotionVm.Coeff);
                        acc = (int)Math.Round(SelectedMotionVm.AccVal * SelectedMotionVm.Coeff);
                        dcc = (int)Math.Round(SelectedMotionVm.DccVal * SelectedMotionVm.Coeff);
                        int tgt = (int)Math.Round(SelectedMotionVm.TgtPos * SelectedMotionVm.Coeff);
                        _MotionController.AbsoluteMoveSingleMotion(SelectedType, startSpd, topSpd, acc, dcc, tgt, true, false);
                    }
                    break;
                case "Polar":
                    _MotionController.SetMotionPolarities(_MotionViewModels[MotorTypes.X].Polarities,
                        _MotionViewModels[MotorTypes.Y].Polarities,
                        _MotionViewModels[MotorTypes.Z].Polarities,
                        _MotionViewModels[MotorTypes.W].Polarities);
                    break;
                case "ScanX":
                    int leftPos = (int)(20 * _MotionViewModels[MotorTypes.X].Coeff);
                    int rightPos = (int)(220 * _MotionViewModels[MotorTypes.X].Coeff);
                    int topSpeed = (int)(400 * _MotionViewModels[MotorTypes.X].Coeff);
                    int accVal = (int)(4000 * _MotionViewModels[MotorTypes.X].Coeff);
                    int delays = 2000;
                    int repeats = 2000;
                    _MotionController.AbsoluteMoveSingleMotion(MotorTypes.X, 256, topSpeed, accVal, accVal, leftPos, rightPos, repeats, delays, true, false);

                    leftPos = (int)(20 * _MotionViewModels[MotorTypes.Y].Coeff);
                    rightPos = (int)(200 * _MotionViewModels[MotorTypes.Y].Coeff);
                    topSpeed = (int)(6 * _MotionViewModels[MotorTypes.Y].Coeff);
                    accVal = (int)(10 * _MotionViewModels[MotorTypes.Y].Coeff);
                    _MotionController.AbsoluteMoveSingleMotion(MotorTypes.Y, 256, topSpeed, accVal, accVal, leftPos, rightPos, repeats, delays, true, false);
                    break;
            }
        }

        private bool CanExecuteSetCmd(object obj)
        {
            return IsConnected;
        }
        #endregion Set Command
    }
}
