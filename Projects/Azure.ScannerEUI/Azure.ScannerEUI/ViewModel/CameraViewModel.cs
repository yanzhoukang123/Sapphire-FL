using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input; //ICommand
using System.Collections.ObjectModel;   //ObservableCollection
using System.Windows.Media.Imaging; //WriteableBitmap
using System.Windows;   //Rect
using System.Windows.Threading; //Dispatcher
using System.Runtime.InteropServices;   //DLLImport
using Azure.CameraLib;  //PVCamCamera
using Azure.CommandLib; //ThreadBase
using Azure.Image.Processing;   //ImageInfo
using Azure.ImagingSystem;
using Azure.Configuration.Settings;
using Azure.WPF.Framework;  //RelayCommand

namespace Azure.ScannerEUI.ViewModel
{
    class CameraViewModel : ViewModelBase
    {
        #region Private data...

        //Photometrics camera object we will be working with
        //PVCamCamera _ActiveCamera;
        PhotometricsCamera _ActiveCamera;

        private double _ExposureTime = 0.001;   // exposure time in seconds

        private ObservableCollection<BinningFactorType> _BinningOptions = new ObservableCollection<BinningFactorType>();
        private ObservableCollection<GainType> _GainOptions = null;
        private ObservableCollection<ReadoutType> _ReadoutOptions = new ObservableCollection<ReadoutType>();
        private Dictionary<bool, string> _DarkFrameCorrOptions = new Dictionary<bool, string>();
        private BinningFactorType _SelectedBinning = null;
        private GainType _SelectedGain = null;
        private ReadoutType _SelectedReadout = null;
        private bool _IsDarkFrameCorrEnabled = false;
        private int _Left = 0;
        private int _Top = 0;
        private int _Width = 0;
        private int _Height = 0;

        private double _CcdTempSetPoint = 0;
        private double _CcdTemp = 0;

        private int _LedRedIntensity = 0;
        private int _LedGreenIntensity = 0;
        private int _LedBlueIntensity = 0;

        private bool _IsWhiteLEDOn = false;
        private bool _IsCameraConnected = false;
        private bool _IsLedRedSelected = false;
        private bool _IsLedGreenSelected = false;
        private bool _IsLedBlueSelected = false;

        private RelayCommand _ResetRoiCommand = null;
        private RelayCommand _SetCcdTempCommand = null;
        private RelayCommand _ReadCcdTempCommand = null;
        private RelayCommand _StartCaptureCommand = null;
        private RelayCommand _StopCaptureCommand = null;
        private RelayCommand _StartContinuousCommand = null;
        private RelayCommand _StopContinuousCommand = null;

        private ImageCaptureCommand _ImageCaptureCommand = null;
        private ImagingLiveCommand _LiveModeCommand = null;

        #endregion

        #region Constructors...

        public CameraViewModel()
        {
            _BinningOptions = SettingsManager.ConfigSettings.BinningFactorOptions;
            //if (_BinningOptions != null && _BinningOptions.Count > 0)
            //{
            //    SelectedBinning = _BinningOptions[0];  //select the first item
            //}

            _GainOptions = SettingsManager.ConfigSettings.GainOptions;
            //if (_GainOptions != null && _GainOptions.Count > 0)
            //{
            //    SelectedGain = _GainOptions[0];    //select the first item
            //}

            // populate readout options
            //ReadoutType readout1 = new ReadoutType(1, 0, "Normal");
            //ReadoutType readout2 = new ReadoutType(2, 1, "Fast");
            //_ReadoutOptions.Add(readout1);
            //_ReadoutOptions.Add(readout2);
            //_SelectedReadout = _ReadoutOptions[0]; // select the first item

            //CcdTempSetPoint = -10;

            _DarkFrameCorrOptions.Add(true, "Enable");
            _DarkFrameCorrOptions.Add(false, "Disabled");


            /*if (SettingsManager.ConfigSettings.RgbLedIntensities != null &&
                SettingsManager.ConfigSettings.RgbLedIntensities.Count >= 3)
            {
                LedRedIntensity = SettingsManager.ConfigSettings.RgbLedIntensities[0].Intensity;
                LedGreenIntensity = SettingsManager.ConfigSettings.RgbLedIntensities[1].Intensity;
                LedBlueIntensity = SettingsManager.ConfigSettings.RgbLedIntensities[1].Intensity;
            }*/

            //_ActiveCamera = new PVCamCamera();
            //_ActiveCamera.CamNotif += new PVCamCamera.CameraNotificationsHandler(_ActiveCamera_CamNotif);
            _ActiveCamera = new PhotometricsCamera();

        }

        #endregion

        #region Public properties...


        public int LedRedIntensity
        {
            get { return _LedRedIntensity; }
            set
            {
                if (_LedRedIntensity != value)
                {
                    _LedRedIntensity = value;
                    RaisePropertyChanged("LedRedIntensity");
                }
            }
        }

        public int LedGreenIntensity
        {
            get { return _LedGreenIntensity; }
            set
            {
                if (_LedGreenIntensity != value)
                {
                    _LedGreenIntensity = value;
                    RaisePropertyChanged("LedGreenIntensity");
                }
            }
        }

        public int LedBlueIntensity
        {
            get { return _LedBlueIntensity; }
            set
            {
                if (_LedBlueIntensity != value)
                {
                    _LedBlueIntensity = value;
                    RaisePropertyChanged("LedBlueIntensity");
                }
            }
        }

        public bool IsLedRedSelected
        {
            get { return _IsLedRedSelected; }
            set
            {
                if (_IsLedRedSelected != value)
                {
                    _IsLedRedSelected = value;
                    RaisePropertyChanged("IsLedRedSelected");
                    //if (_IsLedRedSelected == true)
                    //{
                    //    if (Workspace.This.ApdVM.APDTransfer.APDTransferIsAlive)
                    //    {
                    //        Workspace.This.ApdVM.APDTransfer.APDLaserLedRed(LedRedIntensity);
                    //    }
                    //}
                    //else
                    //{
                    //    if (Workspace.This.ApdVM.APDTransfer.APDTransferIsAlive)
                    //    {
                    //        Workspace.This.ApdVM.APDTransfer.APDLaserLedRed(0);
                    //    }
                    //}
                }
            }
        }

        public bool IsLedGreenSelected
        {
            get { return _IsLedGreenSelected; }
            set
            {
                if (_IsLedGreenSelected != value)
                {
                    _IsLedGreenSelected = value;
                    RaisePropertyChanged("IsLedGreenSelected");
                    //if (_IsLedGreenSelected == true)
                    //{
                    //    if (Workspace.This.ApdVM.APDTransfer.APDTransferIsAlive)
                    //    {
                    //        Workspace.This.ApdVM.APDTransfer.APDLaserLedGreen(LedGreenIntensity);
                    //    }
                    //}
                    //else
                    //{
                    //    if (Workspace.This.ApdVM.APDTransfer.APDTransferIsAlive)
                    //    {
                    //        Workspace.This.ApdVM.APDTransfer.APDLaserLedGreen(0);
                    //    }
                    //}
                }
            }
        }

        public bool IsLedBlueSelected
        {
            get { return _IsLedBlueSelected; }
            set
            {
                if (_IsLedBlueSelected != value)
                {
                    _IsLedBlueSelected = value;
                    RaisePropertyChanged("IsLedBlueSelected");
                    //if (_IsLedBlueSelected == true)
                    //{
                    //    if (Workspace.This.ApdVM.APDTransfer.APDTransferIsAlive)
                    //    {
                    //        Workspace.This.ApdVM.APDTransfer.APDLaserLedBlue(LedBlueIntensity);
                    //    }
                    //}
                    //else
                    //{
                    //    if (Workspace.This.ApdVM.APDTransfer.APDTransferIsAlive)
                    //    {
                    //        Workspace.This.ApdVM.APDTransfer.APDLaserLedBlue(0);
                    //    }
                    //}
                }
            }
        }

        public PhotometricsCamera ActiveCamera
        {
            get { return _ActiveCamera; }
        }

        public double ExposureTime
        {
            get { return _ExposureTime; }
            set
            {
                if (_ExposureTime != value)
                {
                    _ExposureTime = value;
                    RaisePropertyChanged("ExposureTime");
                }
            }
        }

        public ObservableCollection<BinningFactorType> BinningOptions
        {
            get { return _BinningOptions; }
        }

        public ObservableCollection<GainType> GainOptions
        {
            get { return _GainOptions; }
        }

        public ObservableCollection<ReadoutType> ReadoutOptions
        {
            get { return _ReadoutOptions; }
        }

        public BinningFactorType SelectedBinning
        {
            get { return _SelectedBinning; }
            set
            {
                if (_SelectedBinning != value)
                {
                    _SelectedBinning = value;
                    RaisePropertyChanged("SelectedBinning");
                    if (_SelectedBinning != null)
                    {
                        if (_ActiveCamera != null && _IsCameraConnected)
                        {
                            _ActiveCamera.HBin = _SelectedBinning.VerticalBins;
                            Left = 0;
                            Top = 0;
                            //int nWidth = (ActiveCamera.XSize / _SelectedBinning.VerticalBins) - 1;
                            //int nHeight = (ActiveCamera.YSize / _SelectedBinning.VerticalBins) - 1;
                            // Make image width and height even number
                            //Width = (nWidth % 2 == 0) ? nWidth - 1 : nWidth;
                            //Height = (nHeight % 2 == 0) ? nHeight - 1 : nHeight;
                            //Width = ActiveCamera.ImagingColumns - 1;
                            //Height = ActiveCamera.ImagingRows - 1;
                            Width = (_ActiveCamera.ImagingColumns / _SelectedBinning.VerticalBins) - 1;
                            if (Width < 0)
                                Width = 0;
                            Height = (_ActiveCamera.ImagingRows / _SelectedBinning.VerticalBins) - 1;
                            if (Height < 0)
                                Height = 0;
                        }
                    }
                }
            }
        }

        public GainType SelectedGain
        {
            get { return _SelectedGain; }
            set
            {
                if (_SelectedGain != value)
                {
                    _SelectedGain = value;
                    RaisePropertyChanged("SelectedGain");

                    if (_SelectedGain != null)
                    {
                        if (ActiveCamera != null)
                        {
                            ActiveCamera.Gain = _SelectedGain.Value;
                        }
                    }
                }
            }
        }

        public ReadoutType SelectedReadout
        {
            get { return _SelectedReadout; }
            set
            {
                if (_SelectedReadout != value)
                {
                    _SelectedReadout = value;
                    RaisePropertyChanged("SelectedReadout");

                    if (_SelectedReadout != null)
                    {
                        /*if (ActiveCamera != null)
                        {
                            //NOTE: readout speed value for Apogee and Photometrics are reverse.
                            //      Photometrics camera: 0 = Fast, 1 = Normal
                            //      Apogee camera: 0 = Normal, 1 = Fast
                            //      We're mirror the Apogee driver, so reverse it before setting
                            //      (then it get reverse again in Photometrics ReadoutSpeed property).
                            int readoutValue = (_SelectedReadout.Value == 0) ? 1 : 0; 
                            ActiveCamera.ReadoutSpeed = readoutValue;
                        }*/
                    }
                }
            }
        }

        public Dictionary<bool, string> DarkFrameCorrOptions
        {
            get { return _DarkFrameCorrOptions; }
        }

        public bool IsDarkFrameCorrEnabled
        {
            get { return _IsDarkFrameCorrEnabled; }
            set
            {
                if (_IsDarkFrameCorrEnabled != value)
                {
                    _IsDarkFrameCorrEnabled = value;
                    RaisePropertyChanged("IsDarkFrameCorrEnabled");
                    if (_ActiveCamera != null)
                    {
                        _ActiveCamera.IsDynamicDarkCorrection = _IsDarkFrameCorrEnabled;
                    }
                }
            }
        }

        /// <summary>
        /// The x-coordinate of the left edge of the rectangle
        /// </summary>
        public int Left
        {
            get { return _Left; }
            set
            {
                if (_Left != value)
                {
                    _Left = value;
                    RaisePropertyChanged("Left");
                    Width = Width - ((Width + Left) - (ActiveCamera.ImagingColumns - 1));
                }
            }
        }
        /// <summary>
        /// The y-coordinate of the top edge of the rectangle
        /// </summary>
        public int Top
        {
            get { return _Top; }
            set
            {
                if (_Top != value)
                {
                    _Top = value;
                    RaisePropertyChanged("Top");
                    Height = Height - ((Height + Top) - (ActiveCamera.ImagingRows - 1));
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
                    RaisePropertyChanged("Height");
                }
            }
        }

        public double CcdTempSetPoint
        {
            get { return _CcdTempSetPoint; }
            set
            {
                if (_CcdTempSetPoint != value)
                {
                    _CcdTempSetPoint = value;
                    RaisePropertyChanged("CcdTempSetPoint");
                }
            }
        }
        public double CcdTemp
        {
            get { return _CcdTemp; }
            set
            {
                if (_CcdTemp != value)
                {
                    _CcdTemp = value;
                    RaisePropertyChanged("CcdTemp");
                }
            }
        }

        public bool IsWhiteLEDOn
        {
            get { return _IsWhiteLEDOn; }
            set
            {
                if (_IsWhiteLEDOn != value)
                {
                    _IsWhiteLEDOn = value;
                    RaisePropertyChanged("IsWhiteLEDOn");
                    //if (_IsWhiteLEDOn == true)
                    //{
                    //    if (Workspace.This.ApdVM.APDTransfer.APDTransferIsAlive)
                    //    {
                    //        if (LedBlueIntensity == 0 || LedGreenIntensity == 0 || LedBlueIntensity == 0)
                    //        {
                    //            string caption = "White LED";
                    //            string message = "Please make sure Red, Green, or Blue intensity is not zero.";
                    //            MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                    //            _IsWhiteLEDOn = false;
                    //            RaisePropertyChanged("IsWhiteLEDOn");
                    //            return;
                    //        }
                    //        else
                    //        {
                    //            Workspace.This.ApdVM.APDTransfer.APDLaserLedWhite(LedRedIntensity, LedGreenIntensity, LedBlueIntensity);
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    if (Workspace.This.ApdVM.APDTransfer.APDTransferIsAlive)
                    //    {
                    //        Workspace.This.ApdVM.APDTransfer.APDLaserLedWhite(0, 0, 0);
                    //    }
                    //}
                }
            }
        }

        public bool IsCameraConnected
        {
            get { return _IsCameraConnected; }
            set
            {
                if (_IsCameraConnected != value)
                {
                    _IsCameraConnected = value;
                    RaisePropertyChanged("IsCameraConnected");
                    RaisePropertyChanged("IsEnabledControl");
                }
            }
        }

        public bool IsEnabledControl
        {
            get
            {
                if (IsCameraConnected &&
                    !Workspace.This.IsCapturing &&
                    !Workspace.This.IsContinuous)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        #endregion

        public bool InitializeCamera()
        {
            bool bResult = false;
            try
            {
                if (_ActiveCamera.Open())
                {
                    bResult = true;

                    ReadoutOptions.Clear();

                    for (int i = 0; i < _ActiveCamera.ReadoutOption.Count; i++)
                    {
                        string portDesc = string.Empty;
                        if (_ActiveCamera.ReadoutOption[i].Speed == 0)
                        {
                            portDesc = string.Format("Fast (Speed: {0}, Bit Depth: {1})",
                                _ActiveCamera.ReadoutOption[i].Speed,
                                _ActiveCamera.ReadoutOption[i].BitDepth);
                        }
                        else if (_ActiveCamera.ReadoutOption[i].Speed == 1)
                        {
                            portDesc = portDesc = string.Format("Normal (Speed: {0}, Bit Depth: {1})",
                                _ActiveCamera.ReadoutOption[i].Speed,
                                _ActiveCamera.ReadoutOption[i].BitDepth);
                        }
                        ReadoutType readout = new ReadoutType(i, _ActiveCamera.ReadoutOption[i].Speed, portDesc);
                        ReadoutOptions.Add(readout);
                    }

                    List<PP_Feature> ppList = ActiveCamera.PP_FeatureList;
                    if (ppList != null)
                    {
                        for (int i = 0; i < ppList.Count; i++)
                        {
                            if (ppList[i].Name.Contains("Dark Frame"))
                            {
                                if (ppList[i].FunctionList[0].CurrentVal == 0)
                                {
                                    IsDarkFrameCorrEnabled = false;
                                }
                                else if (ppList[i].FunctionList[0].CurrentVal == 1)
                                {
                                    IsDarkFrameCorrEnabled = true;
                                }
                            }
                        }
                    }

                    // Select normal readout
                    if (_ReadoutOptions != null && _ReadoutOptions.Count > 0)
                    {
                        SelectedReadout = _ReadoutOptions[1];
                    }

                    //Select binning 1x1
                    if (_BinningOptions != null && _BinningOptions.Count > 0)
                    {
                        SelectedBinning = _BinningOptions[0];
                    }
                    //Select gain: 1
                    if (_GainOptions != null && GainOptions.Count > 0)
                    {
                        SelectedGain = _GainOptions[0];
                    }
                }
                else
                {
                    bResult = false;
                }
            }
            catch
            {
                bResult = false;
            }

            IsCameraConnected = bResult;

            return bResult;
        }

        public void CloseCamera()
        {
            if (ActiveCamera != null)
            {
                ActiveCamera.Close();
                IsCameraConnected = false;
            }
        }

        #region ResetRoiCommand

        public ICommand ResetRoiCommand
        {
            get
            {
                if (_ResetRoiCommand == null)
                {
                    _ResetRoiCommand = new RelayCommand(ExecuteResetRoiCommand, CanExecuteResetRoiCommand);
                }

                return _ResetRoiCommand;
            }
        }
        public void ExecuteResetRoiCommand(object parameter)
        {
            if (_ActiveCamera != null)
            {
                Left = 0;
                Top = 0;
                Width = ActiveCamera.ImagingColumns - 1;
                Height = ActiveCamera.ImagingRows - 1;
                ActiveCamera.RoiStartX = 0;
                ActiveCamera.RoiWidth = (UInt16)(ActiveCamera.ImagingColumns - 1);
                ActiveCamera.RoiStartY = 0;
                ActiveCamera.RoiHeight = (UInt16)(ActiveCamera.ImagingRows - 1);
            }
        }

        public bool CanExecuteResetRoiCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region SetCcdTempCommand

        public ICommand SetCcdTempCommand
        {
            get
            {
                if (_SetCcdTempCommand == null)
                {
                    _SetCcdTempCommand = new RelayCommand(ExecuteSetCcdTempCommand, CanExecuteSetCcdTempCommand);
                }

                return _SetCcdTempCommand;
            }
        }
        public void ExecuteSetCcdTempCommand(object parameter)
        {
            if (_ActiveCamera != null)
            {
                //if (!_ActiveCamera.SetCcdTemp(_CcdTempSetPoint * 100))
                //{
                //    string caption = "CCD set temperature error";
                //    string message = string.Format("Error setting CDD temperature to {0}", _CcdTempSetPoint);
                //    MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Error);
                //}
                _ActiveCamera.CCDCoolerSetPoint = _CcdTempSetPoint;
            }
        }

        public bool CanExecuteSetCcdTempCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region ReadCcdTempCommand

        public ICommand ReadCcdTempCommand
        {
            get
            {
                if (_ReadCcdTempCommand == null)
                {
                    _ReadCcdTempCommand = new RelayCommand(ExecuteReadCcdTempCommand, CanExecuteReadCcdTempCommand);
                }

                return _ReadCcdTempCommand;
            }
        }
        public void ExecuteReadCcdTempCommand(object parameter)
        {
            if (_ActiveCamera != null)
            {
                CcdTemp = _ActiveCamera.CCDTemperature;
            }
        }

        public bool CanExecuteReadCcdTempCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region StartCaptureCommand

        public ICommand StartCaptureCommand
        {
            get
            {
                if (_StartCaptureCommand == null)
                {
                    _StartCaptureCommand = new RelayCommand(ExecuteStartCaptureCommand, CanExecuteStartCaptureCommand);
                }

                return _StartCaptureCommand;
            }
        }
        public void ExecuteStartCaptureCommand(object parameter)
        {
            //if (Workspace.This.ApdVM.APDTransfer.APDTransferIsAlive)
            //{
            //    Workspace.This.ApdVM.APDTransfer.APDLaserOpenEX();
            //}
            System.Threading.Thread.Sleep(1000);//Warning!This must be keep for opening lid of camera .

            ImageChannelSettings imagingChannel = new ImageChannelSettings();
            imagingChannel.Exposure = ExposureTime; // exposure time in seconds
            imagingChannel.BinningMode = SelectedBinning.VerticalBins;
            imagingChannel.AdGain = SelectedGain.Value;
            //imagingChannel.ReadoutSpeed = SelectedReadout.Value;
            //NOTE: readout speed value for Apogee and Photometrics are reverse.
            //      Photometrics camera: 0 = Fast, 1 = Normal
            //      Apogee camera: 0 = Normal, 1 = Fast
            //      We're mirror the Apogee driver, so reverse it before setting
            //      (then it get reverse again in Photometrics ReadoutSpeed property).
            int readoutValue = (_SelectedReadout.Value == 0) ? 1 : 0;
            imagingChannel.ReadoutSpeed = readoutValue;

            //Get ROI
            if ((Width + Left) > ActiveCamera.ImagingColumns - 1)
            {
                Width = Width - ((Width + Left) - (ActiveCamera.ImagingColumns - 1));
            }
            if ((Height + Top) > ActiveCamera.ImagingRows - 1)
            {
                Height = Height - ((Height + Top) - (ActiveCamera.ImagingRows - 1));
            }
            Rect roiRect = new Rect(Left, Top, Width, Height);

            _ImageCaptureCommand = new ImageCaptureCommand(Workspace.This.Owner.Dispatcher,
                                                           _ActiveCamera,
                                                           imagingChannel,
                                                           roiRect);
            _ImageCaptureCommand.Completed += new CommandLib.ThreadBase.CommandCompletedHandler(_ImageCaptureCommand_Completed);
            _ImageCaptureCommand.CommandStatus += new ImageCaptureCommand.CommandStatusHandler(_ImageCaptureCommand_CommandStatus);
            _ImageCaptureCommand.CompletionEstimate += new ImageCaptureCommand.CommandCompletionEstHandler(_ImageCaptureCommand_CompletionEstimate);
            _ImageCaptureCommand.Start();
            Workspace.This.IsCapturing = true;
            RaisePropertyChanged("IsEnabledControl");
        }

        private void _ImageCaptureCommand_CompletionEstimate(ThreadBase sender, DateTime dateTime, double estTime)
        {
            Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
            {
                Workspace.This.CaptureCountdownTimer.Start();
            });

            Workspace.This.CaptureStartTime = dateTime;
            Workspace.This.EstimatedCaptureTime = estTime;
        }

        private void _ImageCaptureCommand_CommandStatus(object sender, string status)
        {
            Workspace.This.Owner.Dispatcher.BeginInvoke((Action)delegate
            {
                Workspace.This.CapturingTopStatusText = status;
            });
        }

        private void _ImageCaptureCommand_Completed(CommandLib.ThreadBase sender, CommandLib.ThreadBase.ThreadExitStat exitState)
        {
            Workspace.This.Owner.Dispatcher.BeginInvoke((Action)delegate
            {
                Workspace.This.CaptureCountdownTimer.Stop();
                Workspace.This.IsCapturing = false;
                RaisePropertyChanged("IsEnabledControl");

                ImageCaptureCommand imageCaptureThread = (sender as ImageCaptureCommand);

                if (exitState == ThreadBase.ThreadExitStat.None)
                {
                    // Capture successful

                    WriteableBitmap capturedImage = imageCaptureThread.CapturedImage;
                    ImageInfo imageInfo = imageCaptureThread.ImageInfo;
                    if (imageInfo != null)
                    {
                        imageInfo.SoftwareVersion = Workspace.This.ProductVersion;
                    }

                    if (capturedImage != null)
                    {
                        string newTitle = String.Format("Image{0}", ++Workspace.This.FileNameCount);
                        Workspace.This.NewDocument(capturedImage, imageInfo, newTitle, false);
                        Workspace.This.SelectedTabIndex = (int)ApplicationTabType.Gallery;   // Switch to gallery tab
                    }
                }
                else if (exitState == ThreadBase.ThreadExitStat.Error)
                {
                    // Oh oh something went wrong - handle the error

                    if (imageCaptureThread != null && imageCaptureThread.Error != null)
                    {
                        string strCaption = "Image acquisition error...";
                        string strMessage = string.Empty;

                        if (imageCaptureThread.IsOutOfMemory)
                        {
                            strMessage = "System low on memory.\n" +
                                         "Please close some images before acquiring another image.\n" +
                                         "If this error persists, please restart the application.";
                            MessageBox.Show(strMessage, strCaption);
                        }
                        else
                        {
                            strMessage = "Image acquisition error: \n" + imageCaptureThread.Error.Message;
                            MessageBox.Show(strMessage, strCaption, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        }
                    }

                }

                _ImageCaptureCommand.Completed -= new CommandLib.ThreadBase.CommandCompletedHandler(_ImageCaptureCommand_Completed);
                _ImageCaptureCommand.CommandStatus -= new ImageCaptureCommand.CommandStatusHandler(_ImageCaptureCommand_CommandStatus);
                _ImageCaptureCommand = null;
            });
        }

        public bool CanExecuteStartCaptureCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region StopCaptureCommand

        public ICommand StopCaptureCommand
        {
            get
            {
                if (_StopCaptureCommand == null)
                {
                    _StopCaptureCommand = new RelayCommand(ExecuteStopCaptureCommand, CanExecuteStopCaptureCommand);
                }

                return _StopCaptureCommand;
            }
        }
        public void ExecuteStopCaptureCommand(object parameter)
        {
            // Abort image capture thread
            if (_ImageCaptureCommand != null)
            {
                _ImageCaptureCommand.Abort();
            }
        }

        public bool CanExecuteStopCaptureCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region StartContinuousCommand

        public ICommand StartContinuousCommand
        {
            get
            {
                if (_StartContinuousCommand == null)
                {
                    _StartContinuousCommand = new RelayCommand(ExecuteStartContinuousCommand, CanExecuteStartContinuousCommand);
                }

                return _StartContinuousCommand;
            }
        }

        public void ExecuteStartContinuousCommand(object parameter)
        {
            if (_ActiveCamera == null || _ActiveCamera.IsAcqRunning)
            {
                return;
            }

            ImageChannelSettings imagingChan = new ImageChannelSettings();
            imagingChan.BinningMode = _SelectedBinning.VerticalBins;
            imagingChan.Exposure = _ExposureTime;
            imagingChan.AdGain = _SelectedGain.Value;
            //NOTE: readout speed value for Apogee and Photometrics are reverse.
            //      Photometrics camera: 0 = Fast, 1 = Normal
            //      Apogee camera: 0 = Normal, 1 = Fast
            //      We're mirror the Apogee driver, so reverse it before setting
            //      (then it get reverse again in Photometrics ReadoutSpeed property).
            int readoutValue = (_SelectedReadout.Value == 0) ? 1 : 0;
            imagingChan.ReadoutSpeed = readoutValue;
            imagingChan.RgbIntensities = SettingsManager.ConfigSettings.RgbLedIntensities;

            Rect roiRect = new Rect();
            int nWidth = ActiveCamera.ImagingColumns / _SelectedBinning.VerticalBins;
            int nHeight = ActiveCamera.ImagingRows / _SelectedBinning.VerticalBins;

            if (Top != 0 && Left != 0 && Width != nWidth - 1 && Height != nHeight - 1)
            {
                // Get region of interest
                if ((Width + Left) > (nWidth - 1))
                {
                    Width = Width - ((Width + Left) - (nWidth - 1));
                }
                if ((Height + Top) > (nHeight - 1))
                {
                    Height = Height - ((Height + Top) - (nHeight - 1));
                }
                roiRect = new Rect(Left, Top, Width, Height);
            }

            //_LiveModeCommand = new ImagingLiveCommand(Workspace.This.Owner.Dispatcher,
            //                                          _ActiveCamera,
            //                                          Workspace.This.ApdVM.APDTransfer,
            //                                          imagingChan,
            //                                          roiRect,
            //                                          false);
            //_LiveModeCommand.CommandStatus += new ImagingLiveCommand.CommandStatusHandler(_LiveModeCommand_CommandStatus);
            //_LiveModeCommand.LiveImageReceived += new ImagingLiveCommand.ImageReceivedHandler(_LiveModeCommand_LiveImageReceived);
            //_LiveModeCommand.Completed += new ThreadBase.CommandCompletedHandler(_LiveModeCommand_Completed);
            //_LiveModeCommand.Start();
            //Workspace.This.IsContinuous = true;
            //RaisePropertyChanged("IsEnabledControl");
        }

        private void _LiveModeCommand_Completed(ThreadBase sender, ThreadBase.ThreadExitStat exitState)
        {
            Workspace.This.Owner.Dispatcher.BeginInvoke((Action)delegate
            {
                Workspace.This.DisplayImage = null;
            });

            Workspace.This.IsContinuous = false;
            RaisePropertyChanged("IsEnabledControl");

            _LiveModeCommand.CommandStatus -= new ImagingLiveCommand.CommandStatusHandler(_LiveModeCommand_CommandStatus);
            _LiveModeCommand.LiveImageReceived -= new ImagingLiveCommand.ImageReceivedHandler(_LiveModeCommand_LiveImageReceived);
            _LiveModeCommand.Completed -= new ThreadBase.CommandCompletedHandler(_LiveModeCommand_Completed);
            _LiveModeCommand = null;
        }

        private void _LiveModeCommand_LiveImageReceived(BitmapSource displayBitmap)
        {
            try
            {
                Workspace.This.Owner.Dispatcher.BeginInvoke((Action)delegate
                {
                    Workspace.This.DisplayImage = displayBitmap;
                });
            }
            catch (Exception ex)
            {
                ExecuteStopContinuousCommand(null);
                throw new Exception("Live mode error.", ex);
            }
        }

        private void _LiveModeCommand_CommandStatus(object sender, string status)
        {
            Workspace.This.Owner.Dispatcher.BeginInvoke((Action)delegate
            {
                Workspace.This.CapturingTopStatusText = status;
            });
        }

        public bool CanExecuteStartContinuousCommand(object parameter)
        {
            return true;
        }

        //[System.Runtime.InteropServices.DllImport("gdi32.dll")]
        //public static extern bool DeleteObject(IntPtr handle);
        //public static BitmapSource bs;
        //public static IntPtr ip;

        //public static BitmapSource BitmapToBitmapSource(System.Drawing.Bitmap source)
        //{
        //    ip = source.GetHbitmap();

        //    bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
        //            ip,
        //            IntPtr.Zero,
        //            System.Windows.Int32Rect.Empty,
        //            System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

        //    DeleteObject(ip);

        //    return bs;
        //}

        //WriteableBitmap wbDispBitmap;

        /*private void _ActiveCamera_CamNotif(PVCamCamera pvcc, ReportEvent e)
        {
            if (e.NotifEvent == CameraNotifications.ACQ_CONT_NEW_FRAME_RECEIVED)
            {
                //ActiveCamera.FrameToBMP(ActiveCamera.FrameDataShorts,
                //                        (ActiveCamera.Region[0].s2 - ActiveCamera.Region[0].s1 + 1) / ActiveCamera.Region[0].sbin,
                //                        (ActiveCamera.Region[0].p2 - ActiveCamera.Region[0].p1 + 1) / ActiveCamera.Region[0].pbin);
                //bs = BitmapToBitmapSource(ActiveCamera.LastBMP);
                //if (bs.CanFreeze) { bs.Freeze(); }
                //Workspace.This.DisplayImage = bs;
                //
                //WriteableBitmap wbDispBitmap = null;

                try
                {
                    ActiveCamera.FrameToBitmap(ActiveCamera.FrameDataShorts,
                                               (ActiveCamera.Region[0].s2 - ActiveCamera.Region[0].s1 + 1) / ActiveCamera.Region[0].sbin,
                                               (ActiveCamera.Region[0].p2 - ActiveCamera.Region[0].p1 + 1) / ActiveCamera.Region[0].pbin);

                    WriteableBitmap wbDispBitmap = ActiveCamera.LastBMP;

                    if (wbDispBitmap.CanFreeze)
                    {
                        wbDispBitmap.Freeze();
                    }

                    Workspace.This.Owner.Dispatcher.BeginInvoke((Action)delegate
                    {
                        Workspace.This.DisplayImage = wbDispBitmap;
                    });

                    ActiveCamera.FrameNumber++;
                }
                catch (Exception ex)
                {
                    ExecuteStopContinuousCommand(null);
                    throw ex;
                }
                finally
                {
                    // Forces a garbage collection
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                    GC.WaitForPendingFinalizers();
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                }
            }
            else if (e.NotifEvent == CameraNotifications.ACQ_CONT_FINISHED)
            {
                ExecuteStopContinuousCommand(null);
            }
        }*/

        #endregion

        #region StopContinuousCommand

        public ICommand StopContinuousCommand
        {
            get
            {
                if (_StopContinuousCommand == null)
                {
                    _StopContinuousCommand = new RelayCommand(ExecuteStopContinuousCommand, CanExecuteStopContinuousCommand);
                }

                return _StopContinuousCommand;
            }
        }
        public void ExecuteStopContinuousCommand(object parameter)
        {
            if (_LiveModeCommand != null)
            {
                _LiveModeCommand.Abort();
            }
        }

        public bool CanExecuteStopContinuousCommand(object parameter)
        {
            return true;
        }

        #endregion

    }

    public class DarkFrameCorrType
    {
        #region Public properties...

        public int Position { get; set; }

        public int Value { get; set; }

        public string DisplayName { get; set; }

        #endregion

        #region Constructors...

        public DarkFrameCorrType()
        {
        }

        public DarkFrameCorrType(int position, int value, string displayName)
        {
            this.Position = position;
            this.Value = value;
            this.DisplayName = displayName;
        }

        #endregion
    }

}
