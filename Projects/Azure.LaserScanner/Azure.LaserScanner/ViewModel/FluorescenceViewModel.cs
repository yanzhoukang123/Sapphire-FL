using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;   //Rect
using System.Windows.Input; //ICommand
using System.Collections.ObjectModel;   //ObservableCollection
using System.ComponentModel;    //BackgroundWorker
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Threading;
using Azure.Common;
using Azure.WPF.Framework;
using Azure.ImagingSystem;
using Azure.CommandLib;
using Azure.Image.Processing;
using Azure.Configuration.Settings;
using Azure.EthernetCommLib;
using Azure.LaserScanner.View;
using Azure.Ipp.Imaging;
using CroppingImageLibrary.Services;

namespace Azure.LaserScanner.ViewModel
{
    /*public enum ScanResolution
    {
        SR10M = 10,
        SR20M = 20,
        SR50M = 50,
        SR100M = 100,
        SR150M = 150,
        SR200M = 200,
        SR500M = 500,
        SR1000M = 1000,
    }*/

    /*public enum ScanQuality
    {
        Low,
        Medium,
        High,
        Highest,
    }*/

    public enum ScanType
    {
        Auto,
        Normal,
        Preview
    }

    class FluorescenceViewModel : ViewModelBase
    {
        #region Private data...

        //private int _ImageYOffsetRecord=0;
        private WriteableBitmap _PreviewImage = null;
        private WriteableBitmap _ChannelL1PrevImage = null;
        private WriteableBitmap _ChannelR1PrevImage = null;
        private WriteableBitmap _ChannelR2PrevImage = null;
        private WriteableBitmap _ChannelL1PrevImageUnAligned = null;
        private WriteableBitmap _ChannelR1PrevImageUnAligned = null;
        private WriteableBitmap _ChannelR2PrevImageUnAligned = null;

        //BackgroundWorker _ContrastWorker = null;
        // Preview image alignment delta X and delta Y offset
        // Channel order A, B, D
        //private int[] _DeltaXAlignOffset = new int[4];
        //private int[] _DeltaYAlignOffset = new int[4];

        private double _XMotorSubdivision = 0;
        private double _YMotorSubdivision = 0;
        private double _ZMotorSubdivision = 0;
        private int _XMaxValue = 0;
        private int _YMaxValue = 0;
        private int _ZMaxValue = 0;
        private double _ScanX0 = 0;
        private double _ScanY0 = 0;
        private double _ScanZ0 = 0;
        private double _ScanDeltaX = 0;
        private double _ScanDeltaY = 0;
        private double _ScanDeltaZ = 0;

        private const double _ScanWidthInMm = 250.0;    // scan width in millimeters
        private const double _ScanHeightInMm = 250.0;   // scan height in millimeters

        private int _Time = 0;
        private string _ScanTime = string.Empty;
        private int _RemainingTime = 0;

        private bool _IsUpdatingPreviewImage = false;
        private bool _IsAligningPreviewImage = false;
        //private bool _IsEdrScanning = false;

        private ImageChannelType _LaserL1ColorChannel = ImageChannelType.None;
        private ImageChannelType _LaserR1ColorChannel = ImageChannelType.None;
        private ImageChannelType _LaserR2ColorChannel = ImageChannelType.None;

        private LaserScanCommand _ImageScanCommand = null;

        private bool _IsSaveScanDataOnAborted = false;

        private RelayCommand _SaveProtocolCommand = null;
        private RelayCommand _DeleteProtocolCommand = null;

        private RelayCommand _StartScanCommand = null;
        private RelayCommand _StopScanCommand = null;
        private RelayCommand _ResolutionChangedCommand = null;

        private Nullable<ScanType> _CurrentScanType = ScanType.Normal;
        private string _ScanType = string.Empty;    // Scan type to be saved in ImageInfo

        private bool _IsLaserL1On = false;
        private bool _IsLaserR1On = false;
        private bool _IsLaserR2On = false;

        private double _PercentCompleted = 0.0;
        private string _EstTimeRemaining = string.Empty;

        //private FileLocationViewModel _FileLocationVm = null;
        private ImagingViewModel _ImagingVm = null;
        private bool _IsSmartScanning = false;

        private const string _ProtocolConfigFile = "Protocols.xml";
        private ObservableCollection<ProtocolViewModel> _AppProtocolOptions = null;
        private ProtocolViewModel _SelectedAppProtocol = null;

        private bool _IsPreviewChannels = false;
        private ContrastSettingsWindow _PreviewContrastWindow = null;
        private bool _IsPrescanCompleted = false;
        private static AutoResetEvent _ContrastThreadEvent = new AutoResetEvent(false);
        private static AutoResetEvent _CroppingThreadEvent = new AutoResetEvent(false);

        private bool _IsCropPreviewImage = false;
        private bool _Is4channelImage = false;
        private bool _IsGrayscaleImage = false;
        //private bool _IsAutoMergeImages = true;
        private bool _IsAutoScan = false;
        private List<string> _ScanImageBaseFilename = new List<string>();
        private bool _IsAutoScanCompleted = false;
        private bool _IsPreviewSetupCompleted = false;
        private bool _IsAbortedOnLidOpened = false;
        private int _CurrentScanRegion = int.MaxValue;
        private bool _IsScanRegionChanged = false;  //Did scan region changed while scanning?
        private bool _IsPreviewImageCleared = false;

        private bool _IsYCompensationEnabled = false;
        private double _YCompensationOffset = 0;

        private Dictionary<string, LaserChannels> _LaserChannelDict;

        #endregion

        #region Constructors...

        public FluorescenceViewModel()
        {
            this.ProgressMin = 0.0;
            this.ProgressMax = 100.0;

            _AppProtocolOptions = new ObservableCollection<ProtocolViewModel>();

            _XMotorSubdivision = SettingsManager.ConfigSettings.XMotorSubdivision;
            _YMotorSubdivision = SettingsManager.ConfigSettings.YMotorSubdivision;
            _ZMotorSubdivision = SettingsManager.ConfigSettings.ZMotorSubdivision;
            _XMaxValue = SettingsManager.ConfigSettings.XMaxValue;
            _YMaxValue = SettingsManager.ConfigSettings.YMaxValue;
            _ZMaxValue = SettingsManager.ConfigSettings.ZMaxValue;

            //获取是否启用了Y轴图像处理（避免Y轴刚启动时照成的前几行有倾斜）
            //Get whether the Y-axis image processing is enabled (to avoid the tilt of the first few lines when the Y-axis is first started)
            _IsYCompensationEnabled = SettingsManager.ConfigSettings.YCompenSationBitAt;
            //截取掉Y轴的行数，（YCompensationOffset * 1000 / res）
            //Number of rows to chop off on the Y-axis
            _YCompensationOffset = SettingsManager.ConfigSettings.YCompenOffset;

            //_FileLocationVm = new FileLocationViewModel();
            //_FileLocationVm.DestinationFolder = SettingsManager.ApplicationSettings.InitialDirectory;

            _ImagingVm = new ImagingViewModel();
            _ImagingVm.ContrastVm.DisplayImageInfo.IsSaturationChecked = true;  // Enable saturation display (default)
            _ImagingVm.ChannelChanged += new ImagingViewModel.ChannelChangedHandler(ImagingVm_ChannelChanged);
            _ImagingVm.ShowPreviewChannelsClicked += new ImagingViewModel.ShowPreviewChannelsClickedHandler(ImagingVm_ShowPreviewChannelsClicked);
            _ImagingVm.UpdateDisplayImage += new ImagingViewModel.UpdateDisplayImagedHandler(ImagingVm_UpdateDisplayImage);
            _ImagingVm.IsContrastChannelAllowed = true;
            _ImagingVm.ScanRegionChanged += ImagingVm_ScanRectChanged;

            _LaserChannelDict = new Dictionary<string, LaserChannels>();
            _LaserChannelDict.Add("L", LaserChannels.ChannelC);
            _LaserChannelDict.Add("R1", LaserChannels.ChannelA);
            _LaserChannelDict.Add("R2", LaserChannels.ChannelB);
        }

        #endregion

        private void ImagingVm_UpdateDisplayImage(object sender, ImageInfo imageInfo)
        {
            // Contrast the preview scanned image
            if (!Workspace.This.IsScanning)
            {
                if ((_ChannelL1PrevImage != null || _ChannelR1PrevImage != null ||
                    _ChannelR2PrevImage != null) && _IsPrescanCompleted)
                {
                    // Contrast the image....
                    UpdatePreviewDisplayImage();
                }
            }
        }

        private void ImagingVm_ChannelChanged(object sender, LaserChannels laserChannel, bool bOnOffFlag)
        {
            if (!Workspace.This.IsScanning && _ImagingVm.NumOfDisplayChannels > 0)
            {
                if ((_ChannelL1PrevImage != null || _ChannelR1PrevImage != null ||
                    _ChannelR2PrevImage != null) && _IsPrescanCompleted)
                {
                    // Contrast the image
                    UpdatePreviewDisplayImage();
                }
            }
            else if (_ImagingVm.NumOfDisplayChannels == 0)
            {
                this._ImagingVm.PreviewImage = null;
            }
        }

        private void ImagingVm_ShowPreviewChannelsClicked(object sender)
        {
            IsPreviewChannels = !_IsPreviewChannels;
        }

        private void ImagingVm_ScanRectChanged(object sender)
        {
            CropService cropService = (CropService)sender;
            if (cropService != null)
            {
                // Selected the added scan region
                if (SelectedAppProtocol.ScanRegions != null && !Workspace.This.IsScanning)
                {
                    if (_ImagingVm.CellSize > 0)
                    {
                        SelectedAppProtocol.SelectedScanRegion = SelectedAppProtocol.ScanRegions.Where(z => z.ScanRegionNum == cropService.AdornerID).FirstOrDefault();

                        if (SelectedAppProtocol.SelectedScanRegion != null)
                        {
                            SelectedAppProtocol.SelectedScanRegion.IsScanRectDragged = true;  // dragged vs manually entered value
                            var cropArea = cropService.GetCroppedArea();
                            double x = Math.Round(cropArea.CroppedRectAbsolute.X / _ImagingVm.CellSize, 2);
                            double y = Math.Round(cropArea.CroppedRectAbsolute.Y / _ImagingVm.CellSize, 2);
                            double w = Math.Round(cropArea.CroppedRectAbsolute.Width / _ImagingVm.CellSize, 2);
                            double h = Math.Round(cropArea.CroppedRectAbsolute.Height / _ImagingVm.CellSize, 2);
                            if (x < 0) { x = 0; }
                            if (y < 0) { y = 0; }
                            if (w > 25) { w = 25; }
                            if (h > 25) { h = 25; }
                            if (x + w > 25) { x -= (x + w) - 25; }
                            if (y + h > 25) { y -= (y + h) - 25; }
                            SelectedAppProtocol.SelectedScanRegion.X = x;
                            SelectedAppProtocol.SelectedScanRegion.Y = y;
                            SelectedAppProtocol.SelectedScanRegion.Width = w;
                            SelectedAppProtocol.SelectedScanRegion.Height = h;
                            SelectedAppProtocol.SelectedScanRegion.IsScanRectDragged = false;
                        }
                    }
                }
                else if (Workspace.This.IsScanning)
                {
                    _IsScanRegionChanged = true;
                }
            }
        }

        private void Protocol_SignalChanged(object sender, bool bIsSelectionChanged)
        {
            if (_SelectedAppProtocol == null) { return; }

            SignalViewModel selectSignal = sender as SignalViewModel;

            if (selectSignal != null && !_IsSmartScanning)
            {
                // Don't check for duplicate selection if single scan channel.
                if (_SelectedAppProtocol.SelectedScanRegion.SignalList.Count > 1)
                {
                    if (selectSignal.SelectingColorChannel != null)
                    {
                        var colorChannelFound = _SelectedAppProtocol.SelectedScanRegion.SignalList.Any(item => item.SelectedColorChannel.ImageColorChannel == selectSignal.SelectingColorChannel.ImageColorChannel);
                        if (colorChannelFound)
                        {
                            string caption = "Color channel already selected...";
                            string message = "The color channel is already selected in the current scan region.\nPlease select another color channel.";
                            Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Information);

                            Workspace.This.Owner.Dispatcher.BeginInvoke((Action)delegate
                            {
                                // Prevent triggering of a selection changed
                                // when reverting back to the previous selected color channel.
                                selectSignal.IsInitializing = true;
                                // Revert back to the previous selected color channel.
                                selectSignal.SelectedColorChannel = selectSignal.PrevSelectedColorChannel;
                                selectSignal.IsInitializing = false;
                            });
                            return;
                        }
                    }
                    else if (selectSignal.SelectingLaser != null)
                    {
                        var laserFound = _SelectedAppProtocol.SelectedScanRegion.SignalList.Any(item => item.SelectedLaser.LaserChannel == selectSignal.SelectingLaser.LaserChannel);
                        if (laserFound)
                        {
                            if (selectSignal.SelectedLaser.LaserChannel != selectSignal.SelectingLaser.LaserChannel)
                            {
                                string caption = "Laser already selected...";
                                string message = "The laser type is already selected in the current protocol.\nPlease select another laser.";
                                Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Information);

                                Workspace.This.Owner.Dispatcher.BeginInvoke((Action)delegate
                                {
                                    // Prevent triggering of a selection changed
                                    // when reverting back to the previous selected dye.
                                    selectSignal.IsInitializing = true;
                                    // Revert back to the previous selected dye.
                                    selectSignal.SelectedLaser = selectSignal.PrevSelectedLaser;
                                    selectSignal.IsInitializing = false;
                                });
                                return;
                            }
                        }
                    }
                }

                if (!_SelectedAppProtocol.IsModified)
                {
                    // Save the protocol default settings
                    _SelectedAppProtocol.IsModified = true;
                }

                if (selectSignal.SelectingColorChannel != null)
                {
                    // Reset previous selected color channel
                    if (selectSignal.SelectedColorChannel.ImageColorChannel == ImageChannelType.Red)
                    {
                        _ImagingVm.ContrastVm.DisplayImageInfo.IsContrastRedChannel = false;
                        _ImagingVm.ContrastVm.DisplayImageInfo.IsDisplayRedChannel = false;
                    }
                    else if (selectSignal.SelectedColorChannel.ImageColorChannel == ImageChannelType.Green)
                    {
                        _ImagingVm.ContrastVm.DisplayImageInfo.IsContrastGreenChannel = false;
                        _ImagingVm.ContrastVm.DisplayImageInfo.IsDisplayGreenChannel = false;
                    }
                    else if (selectSignal.SelectedColorChannel.ImageColorChannel == ImageChannelType.Blue)
                    {
                        _ImagingVm.ContrastVm.DisplayImageInfo.IsContrastBlueChannel = false;
                        _ImagingVm.ContrastVm.DisplayImageInfo.IsDisplayBlueChannel = false;
                    }
                    else if (selectSignal.SelectedColorChannel.ImageColorChannel == ImageChannelType.Gray)
                    {
                        _ImagingVm.ContrastVm.DisplayImageInfo.IsContrastGrayChannel = false;
                        _ImagingVm.ContrastVm.DisplayImageInfo.IsDisplayGrayChannel = false;
                    }
                    //_ImagingVm.SetContrastColorChannel(selectSignal.SelectedColorChannel.ImageColorChannel, false);

                    // Imaging color channel changed
                    SetPreviewColorChannel(selectSignal, selectSignal.SelectingColorChannel.ImageColorChannel);

                    // Set the new selected color channel
                    if (selectSignal.SelectingColorChannel.ImageColorChannel == ImageChannelType.Red)
                    {
                        _ImagingVm.ContrastVm.DisplayImageInfo.IsContrastRedChannel = true;
                        _ImagingVm.ContrastVm.DisplayImageInfo.IsDisplayRedChannel = true;
                    }
                    else if (selectSignal.SelectingColorChannel.ImageColorChannel == ImageChannelType.Green)
                    {
                        _ImagingVm.ContrastVm.DisplayImageInfo.IsContrastGreenChannel = true;
                        _ImagingVm.ContrastVm.DisplayImageInfo.IsDisplayGreenChannel = true;
                    }
                    else if (selectSignal.SelectingColorChannel.ImageColorChannel == ImageChannelType.Blue)
                    {
                        _ImagingVm.ContrastVm.DisplayImageInfo.IsContrastBlueChannel = true;
                        _ImagingVm.ContrastVm.DisplayImageInfo.IsDisplayBlueChannel = true;
                    }
                    else if (selectSignal.SelectingColorChannel.ImageColorChannel == ImageChannelType.Gray)
                    {
                        _ImagingVm.ContrastVm.DisplayImageInfo.IsContrastGrayChannel = true;
                        _ImagingVm.ContrastVm.DisplayImageInfo.IsDisplayGrayChannel = true;
                    }
                    //_ImagingVm.SetContrastColorChannel(selectSignal.SelectingColorChannel.ImageColorChannel, true);
                }
                else if (selectSignal.SelectingLaser != null)
                {
                    // Dye (or laser type) changed
                    if (selectSignal.SelectedLaser != null && selectSignal.SelectingLaser != null)
                    {
                        // Hide previous selected preview laser type (dye)
                        SetPreviewLaserVisibility(selectSignal.SelectedLaser.LaserChannel, false);
                        // Reset the selected laser type (dye) color channel
                        SetPreviewColorChannel(selectSignal.SelectedLaser, ImageChannelType.None);
                        // Set color channel to newly selected laser type (dye)
                        SetPreviewColorChannel(selectSignal.SelectingLaser, selectSignal.SelectedColorChannel.ImageColorChannel); //EL: TODO: backup previous selected laser
                        // Show the newly selected laser type (or dye)
                        SetPreviewLaserVisibility(selectSignal.SelectingLaser.LaserChannel, true); //EL: TODO: backup previous selected laser
                    }
                }
            }
        }

        private void ScanRegion_ScanRectChanged(object sender, bool bIsScanRectChanged, bool bIsScanRectDragged)
        {
            if (_SelectedAppProtocol == null) { return; }

            if (!bIsScanRectChanged)
            {
                // The selection is changing, backup settings of not already backed up
                _SelectedAppProtocol.IsModified = true;
            }

            if (bIsScanRectChanged && !bIsScanRectDragged)
            {
                // Clear preview image(s)
                //if (_ImagingVm.PreviewImages != null && _ImagingVm.PreviewImages.Count > 0)
                //{
                //    _ImagingVm.PreviewImages.Clear();
                //}

                if (_ImagingVm.RemoveAllScanRegions())   // remove scan rect adorner
                {
                    // Start from the end of the list to select the first tab item
                    for (int index = 0; index < _SelectedAppProtocol.ScanRegions.Count; index++)
                    {
                        int nScanRegionId = (_SelectedAppProtocol.ScanRegions[index].ScanRegionNum == 0) ? index + 1 : _SelectedAppProtocol.ScanRegions[index].ScanRegionNum;
                        _ImagingVm.IsInitializing = true;   // Don't trigger scan region changed
                        _ImagingVm.AddScanRegionSelection(nScanRegionId, _SelectedAppProtocol.ScanRegions[index].ScanRect, _SelectedAppProtocol.ScanRegions[index].Color);
                        _ImagingVm.IsInitializing = false;
                    }
                }
            }

            ScanTime = ImagingSystemHelper.FormatTime(_SelectedAppProtocol.SelectedScanRegion.GetScanTime());
            _SelectedAppProtocol.SelectedScanRegion.FileLocationVm.FileSize = GetFileSize();
        }

        private void ScanRegion_ScanRegionSettingChanged(object sender, bool bIsSelectionChanged, bool bIsSignalChanged)
        {
            if (_SelectedAppProtocol == null) { return; }

            if (!bIsSelectionChanged)
            {
                // The selection (or property value) is changing, backup settings of not already backed up
                _SelectedAppProtocol.IsModified = true;
            }
            else
            {
                // The selection/property has changed, calculate estimate scan time and file size

                if (_SelectedAppProtocol.SelectedScanRegion != null && bIsSignalChanged)
                {

                    if (_SelectedAppProtocol.SelectedScanRegion.SignalList != null && SelectedAppProtocol.SelectedScanRegion.SignalCount > 0)
                    {
                        if (_ImagingVm != null)
                        {
                            _ImagingVm.IsLaserL1PrvVisible = false;
                            _ImagingVm.IsLaserR1PrvVisible = false;
                            _ImagingVm.IsLaserR2PrvVisible = false;
                            _ImagingVm.IsContrastLaserL1Channel = false;
                            _ImagingVm.IsContrastLaserR1Channel = false;
                            _ImagingVm.IsContrastLaserR2Channel = false;
                            _ImagingVm.ContrastVm.NumOfChannels = _SelectedAppProtocol.SelectedScanRegion.SignalList.Count;
                            if (_SelectedAppProtocol.SelectedScanRegion.SignalList.Count == 1)
                                _ImagingVm.IsContrastChannelAllowed = false;
                            else
                                _ImagingVm.IsContrastChannelAllowed = true;

                            // image color channel
                            _LaserL1ColorChannel = ImageChannelType.None;
                            _LaserR1ColorChannel = ImageChannelType.None;
                            _LaserR2ColorChannel = ImageChannelType.None;

                            foreach (var signal in _SelectedAppProtocol.SelectedScanRegion.SignalList)
                            {
                                if (signal != null && signal.SelectedLaser != null)
                                {
                                    SetPreviewColorChannel(signal);
                                    SetPreviewLaserVisibility(signal.SelectedLaser.LaserChannel, true);
                                }
                            }
                        }
                    }
                }
                else
                {
                    ScanTime = ImagingSystemHelper.FormatTime(_SelectedAppProtocol.SelectedScanRegion.GetScanTime());
                    _SelectedAppProtocol.SelectedScanRegion.FileLocationVm.FileSize = GetFileSize();
                }
                RaisePropertyChanged("IsEdrScanning");
            }

        }


        #region Public properties...

        //public FileLocationViewModel FileLocationVm
        //{
        //    get { return _FileLocationVm; }
        //}

        public ImagingViewModel ImagingVm
        {
            get { return _ImagingVm; }
        }

        public ObservableCollection<ProtocolViewModel> AppProtocolOptions
        {
            get { return _AppProtocolOptions; }
            set
            {
                if (_AppProtocolOptions != value)
                {
                    _AppProtocolOptions = value;
                    RaisePropertyChanged("AppProtocolOptions");
                }
            }
        }

        public ProtocolViewModel SelectedAppProtocol
        {
            get { return _SelectedAppProtocol; }
            set
            {
                if (_SelectedAppProtocol != value)
                {
                    // Restore default the current selected protocol if modified not and not saved.
                    if (_SelectedAppProtocol != null)
                    {
                        // Protocol selection changed (restore default settings)
                        if (_SelectedAppProtocol.IsModified)
                        {
                            _SelectedAppProtocol.IsModified = false;
                        }
                    }

                    _SelectedAppProtocol = value;
                    RaisePropertyChanged("SelectedAppProtocol");
                    RaisePropertyChanged("IsEdrScanning");

                    if (_SelectedAppProtocol != null)
                    {
                        if (_SelectedAppProtocol.SelectedScanRegion != null)
                        {
                            ScanTime = ImagingSystemHelper.FormatTime(_SelectedAppProtocol.SelectedScanRegion.GetScanTime());
                            _SelectedAppProtocol.SelectedScanRegion.FileLocationVm.FileSize = GetFileSize();

                            UpdateScanRegionPreviewChannels();
                        }
                    }
                }
            }
        }

        public double ScanX0
        {
            get
            {
                double scanX0 = 0.0;
                if (_XMotorSubdivision != 0)
                {
                    scanX0 = (_ScanX0 - (Workspace.This.MotorVM.AbsXPos * _XMotorSubdivision)) / (double)_XMotorSubdivision;
                }
                return scanX0;
            }
            set
            {
                // Workspace.This.MotorVM.AbsXPos = X logical home position
                _ScanX0 = value * _XMotorSubdivision + (Workspace.This.MotorVM.AbsXPos * _XMotorSubdivision);
            }
        }

        public double ScanY0
        {
            get
            {
                double scanY0 = 0.0;
                if (_YMotorSubdivision != 0)
                {
                    scanY0 = (_ScanY0 - (Workspace.This.MotorVM.AbsYPos * _YMotorSubdivision)) / _YMotorSubdivision;
                }
                return scanY0;
            }
            set
            {
                // Workspace.This.MotorVM.AbsYPos = Y logical home position
                _ScanY0 = value * _YMotorSubdivision + (Workspace.This.MotorVM.AbsYPos * _YMotorSubdivision); 
            }
        }

        public double ScanZ0
        {
            get { return _ScanZ0 / _ZMotorSubdivision; }
            set
            {
                if ((ScanZ0 / _ZMotorSubdivision) != value)
                {
                    _ScanZ0 = value * _ZMotorSubdivision;
                }
            }
        }

        public double ScanDeltaX
        {
            get
            {
                double scanDeltaX = 0.0;
                if (_XMotorSubdivision != 0)
                {
                    scanDeltaX = _ScanDeltaX / _XMotorSubdivision;
                }
                return scanDeltaX;
            }
            set
            {
                if ((_ScanDeltaX / _XMotorSubdivision) != value)
                {
                    _ScanDeltaX = value * _XMotorSubdivision;
                }
            }
        }

        public double ScanDeltaY
        {
            get
            {
                double scanDeltaY = 0.0;
                if (_YMotorSubdivision != 0)
                {
                    scanDeltaY = _ScanDeltaY / _YMotorSubdivision;
                }
                return scanDeltaY;
            }
            set
            {
                if ((_ScanDeltaY / _YMotorSubdivision) != value)
                {
                    _ScanDeltaY = value * _YMotorSubdivision;
                }
            }
        }

        public double ScanDeltaZ
        {
            get
            {
                double scanDeltaZ = 0.0;
                if (_ZMotorSubdivision != 0)
                {
                    scanDeltaZ = _ScanDeltaZ / _ZMotorSubdivision;
                }
                return scanDeltaZ;
            }
            set
            {
                if ((_ScanDeltaZ / _ZMotorSubdivision) != value)
                {
                    _ScanDeltaZ = value * _ZMotorSubdivision;
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
                    EstTimeRemaining = "Scan: " + ImagingSystemHelper.FormatTime(_RemainingTime);
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
                }
            }
        }

        /// <summary>
        /// Selected scan region scan time
        /// </summary>
        public string ScanTime
        {
            get
            {
                return _ScanTime;
            }
            set
            {
                _ScanTime = value;
                RaisePropertyChanged("ScanTime");
                RaisePropertyChanged("TotalScanTime");
            }
        }

        /// <summary>
        /// Total scan time (if there's multiple scan regions)
        /// </summary>
        public string TotalScanTime
        {
            get
            {
                string strTotalScanTime = string.Empty;
                if (SelectedAppProtocol != null)
                {
                    if (SelectedAppProtocol.ScanRegions != null && SelectedAppProtocol.ScanRegions.Count > 1)
                    {
                        double dTotalScanTime = 0;
                        foreach (var scanRegion in SelectedAppProtocol.ScanRegions)
                        {
                            dTotalScanTime += scanRegion.GetScanTime();
                        }
                        strTotalScanTime = "[Total: " + ImagingSystemHelper.TotalScanTime(dTotalScanTime) + "]";
                    }
                }

                return strTotalScanTime;
            }
        }


        /// <summary>
        /// Calculate estimate scan time
        /// </summary>
        /// <returns></returns>
        /*private double GetScanTime()
        {
            double estScanTime = 0;
            if (_SelectedAppProtocol != null && _SelectedAppProtocol.SelectedScanRegion != null)
            {
                if (_SelectedAppProtocol.SelectedScanRegion.SelectedPixelSize != null &&
                    _SelectedAppProtocol.SelectedScanRegion.SelectedScanSpeed != null)
                {
                    double deltaY = _ScanHeightInMm * _SelectedAppProtocol.SelectedScanRegion.ScanRect.Height / (_ImagingVm.CellSize * _ImagingVm.NumOfCells);    //250: Total scan area in mm
                    double height = deltaY * 1000.0 / (double)_SelectedAppProtocol.SelectedScanRegion.SelectedPixelSize.Value;
                    estScanTime = height * (double)_SelectedAppProtocol.SelectedScanRegion.SelectedScanSpeed.Value / 2.0;

                    if (SettingsManager.ConfigSettings.IsFluorescence2LinesAvgScan)
                    {
                        estScanTime *= 2;
                    }
                }
            }
            return estScanTime;
        }*/

        internal string GetFileSize()
        {
            string retValue = string.Empty;

            if (_SelectedAppProtocol != null && _SelectedAppProtocol.SelectedScanRegion != null)
            {
                if (_SelectedAppProtocol.SelectedScanRegion.SelectedPixelSize != null)
                {
                    double deltaX = _ScanWidthInMm * SelectedAppProtocol.SelectedScanRegion.ScanRect.Width / (_ImagingVm.CellSize * _ImagingVm.NumOfCells);   //250: Total scan area in mm
                    double deltaY = _ScanHeightInMm * SelectedAppProtocol.SelectedScanRegion.ScanRect.Height / (_ImagingVm.CellSize * _ImagingVm.NumOfCells); //250: Total scan area in mm
                    double width = deltaX * 1000.0 / (double)_SelectedAppProtocol.SelectedScanRegion.SelectedPixelSize.Value;
                    double height = deltaY * 1000.0 / (double)_SelectedAppProtocol.SelectedScanRegion.SelectedPixelSize.Value;

                    double dNumBytes = 2.0; //16-bit image - 2 bytes per pixel
                    double sizePerFile = width * height * dNumBytes;
                    double sizeTotal = 0;

                    string strSizePerFile = string.Empty;
                    string strSizeTotal = string.Empty;

                    #region Get number of imaging channels ===
                    int nNumOfChannels = 1;
                    bool bIsGrayscaleImage = false;
                    if (_SelectedAppProtocol.SelectedScanRegion.SignalList.Count == 1)
                    {
                        if (_SelectedAppProtocol.SelectedScanRegion.SignalList[0].SelectedColorChannel.ImageColorChannel == ImageChannelType.Gray)
                        {
                            bIsGrayscaleImage = true;
                        }
                    }

                    if (bIsGrayscaleImage)
                    {
                        nNumOfChannels = 1;
                    }
                    else
                    {
                        bool bIs4channelImage = false;
                        foreach (var signal in _SelectedAppProtocol.SelectedScanRegion.SignalList)
                        {
                            var colorChannel = signal?.SelectedColorChannel?.ImageColorChannel;
                            if (colorChannel == ImageChannelType.Gray)
                            {
                                bIs4channelImage = true;
                                break;
                            }
                            //if (signal != null)
                            //{
                            //    if (signal.SelectedColorChannel.ImageColorChannel == ImageChannelType.Gray)
                            //    {
                            //        bIs4channelImage = true;
                            //        break;
                            //    }
                            //}
                        }

                        if (bIs4channelImage)
                            nNumOfChannels = 4;
                        else
                            nNumOfChannels = 3;
                    }
                    #endregion

                    if (nNumOfChannels > 1)
                    {
                        // Return in KB if less a MB
                        // 1KB = 1024 bytes 
                        // 1MB = 1024 * 1024
                        //
                        sizeTotal = sizePerFile * nNumOfChannels;

                        // Each channel
                        if (sizePerFile < (1024 * 1024))
                        {
                            double fileSizeInKB = Math.Round((sizePerFile / 1024), 2);
                            strSizePerFile = string.Format("{0} KB Each", fileSizeInKB);
                        }
                        else
                        {
                            double fileSizeInMB = Math.Round((sizePerFile / (1024 * 1024)), 2);
                            strSizePerFile = string.Format("{0} MB Each", fileSizeInMB);
                        }
                        // Total size
                        if (sizeTotal < (1024 * 1024))
                        {
                            double fileSizeInKB = Math.Round((sizeTotal / 1024), 2);
                            strSizeTotal = string.Format(" (Total: {0} KB)", fileSizeInKB);
                        }
                        else
                        {
                            double fileSizeInMB = Math.Round((sizeTotal / (1024 * 1024)), 2);
                            strSizeTotal = string.Format(" (Total: {0} MB)", fileSizeInMB);
                        }
                        retValue = string.Format("{0}{1}", strSizePerFile, strSizeTotal);
                    }
                    else
                    {
                        if (sizePerFile < (1024 * 1024))
                        {
                            double fileSizeInKB = Math.Round((sizePerFile / 1024), 2);
                            strSizePerFile = string.Format("{0} KB", fileSizeInKB);
                        }
                        else
                        {
                            double fileSizeInMB = Math.Round((sizePerFile / (1024 * 1024)), 2);
                            strSizePerFile = string.Format("{0} MB", fileSizeInMB);
                        }
                        retValue = strSizePerFile;
                    }
                }
            }
            return retValue;
        }

        private double GetFileSizeInBytes()
        {
            double sizeTotal = 0;

            if (_SelectedAppProtocol != null && _SelectedAppProtocol.SelectedScanRegion != null)
            {
                if (_SelectedAppProtocol.SelectedScanRegion.SelectedPixelSize != null)
                {
                    double deltaX = _ScanWidthInMm * _SelectedAppProtocol.SelectedScanRegion.ScanRect.Width / (_ImagingVm.CellSize * _ImagingVm.NumOfCells);   //250: Total scan area in mm
                    double deltaY = _ScanHeightInMm * _SelectedAppProtocol.SelectedScanRegion.ScanRect.Height / (_ImagingVm.CellSize * _ImagingVm.NumOfCells);   //250: Total scan area in mm
                    double width = deltaX * 1000.0 / (double)_SelectedAppProtocol.SelectedScanRegion.SelectedPixelSize.Value;
                    double height = deltaY * 1000.0 / (double)_SelectedAppProtocol.SelectedScanRegion.SelectedPixelSize.Value;

                    #region Get number of imaging channels ===

                    int nNumOfChannels = 1;
                    bool bIsGrayscaleImage = false;
                    if (_SelectedAppProtocol.SelectedScanRegion.SignalList.Count == 1)
                    {
                        if (_SelectedAppProtocol.SelectedScanRegion.SignalList[0].SelectedColorChannel.ImageColorChannel == ImageChannelType.Gray)
                        {
                            bIsGrayscaleImage = true;
                        }
                    }

                    if (bIsGrayscaleImage)
                    {
                        nNumOfChannels = 1;
                    }
                    else
                    {
                        bool bIs4channelImage = false;
                        foreach (var signal in _SelectedAppProtocol.SelectedScanRegion.SignalList)
                        {
                            if (signal != null)
                            {
                                if (signal.SelectedColorChannel.ImageColorChannel == ImageChannelType.Gray)
                                {
                                    bIs4channelImage = true;
                                    break;
                                }
                            }
                        }

                        if (bIs4channelImage)
                            nNumOfChannels = 4;
                        else
                            nNumOfChannels = 3;
                    }

                    #endregion

                    double dNumBytes = 2.0; //16-bit image - 2 bytes per pixel
                    double sizePerFile = width * height * dNumBytes;
                    sizeTotal = sizePerFile * nNumOfChannels;
                }
            }
            return sizeTotal;
        }

        private double GetFileSizeInBytes(ScanRegionViewModel scanRegion)
        {
            double sizeTotal = 0;

            if (scanRegion != null)
            {
                if (scanRegion.SelectedPixelSize != null)
                {
                    double deltaX = _ScanWidthInMm * scanRegion.ScanRect.Width / (_ImagingVm.CellSize * _ImagingVm.NumOfCells);   //250: Total scan area in mm
                    double deltaY = _ScanHeightInMm * scanRegion.ScanRect.Height / (_ImagingVm.CellSize * _ImagingVm.NumOfCells);   //250: Total scan area in mm
                    double width = deltaX * 1000.0 / (double)scanRegion.SelectedPixelSize.Value;
                    double height = deltaY * 1000.0 / (double)scanRegion.SelectedPixelSize.Value;

                    #region Get number of imaging channels ===

                    int nNumOfChannels = 1;
                    bool bIsGrayscaleImage = false;
                    if (scanRegion.SignalList.Count == 1)
                    {
                        if (scanRegion.SignalList[0].SelectedColorChannel.ImageColorChannel == ImageChannelType.Gray)
                        {
                            bIsGrayscaleImage = true;
                        }
                    }

                    if (bIsGrayscaleImage)
                    {
                        nNumOfChannels = 1;
                    }
                    else
                    {
                        bool bIs4channelImage = false;
                        foreach (var signal in scanRegion.SignalList)
                        {
                            if (signal != null)
                            {
                                if (signal.SelectedColorChannel.ImageColorChannel == ImageChannelType.Gray)
                                {
                                    bIs4channelImage = true;
                                    break;
                                }
                            }
                        }

                        if (bIs4channelImage)
                            nNumOfChannels = 4;
                        else
                            nNumOfChannels = 3;
                    }

                    #endregion

                    double dNumBytes = 2.0; //16-bit image - 2 bytes per pixel
                    double sizePerFile = width * height * dNumBytes;
                    sizeTotal = sizePerFile * nNumOfChannels;
                }
            }
            return sizeTotal;
        }

        public bool IsLaserL1On
        {
            get
            {
                return _IsLaserL1On;
            }
            set
            {
                if (_IsLaserL1On != value)
                {
                    _IsLaserL1On = value;
                    RaisePropertyChanged("IsLaserL1On");
                }
            }
        }

        public bool IsLaserR1On
        {
            get
            {
                //_IsLaserBOn = (ChannelBSignal > 0) ? true : false;
                return _IsLaserR1On;
            }
            set
            {
                if (_IsLaserR2On != value)
                {
                    _IsLaserR2On = value;
                    RaisePropertyChanged("IsLaserR1On");
                }
            }
        }

        public bool IsLaserR2On
        {
            get
            {
                return _IsLaserR2On;
            }
            set
            {
                if (_IsLaserR2On != value)
                {
                    _IsLaserR2On = value;
                    RaisePropertyChanged("IsLaserR2On");
                }
            }
        }

        public double ProgressMin { get; set; }
        public double ProgressMax { get; set; }

        public string EstTimeRemaining
        {
            get { return _EstTimeRemaining; }
            set
            {
                if (_EstTimeRemaining != value)
                {
                    _EstTimeRemaining = value;
                    RaisePropertyChanged("EstTimeRemaining");
                }
            }
        }

        public double PercentCompleted
        {
            get { return _PercentCompleted; }
            set
            {
                if (_PercentCompleted != value)
                {
                    _PercentCompleted = value;
                    RaisePropertyChanged("PercentCompleted");
                }
            }
        }

        public bool IsPreviewChannels
        {
            get { return _IsPreviewChannels; }
            set
            {
                _IsPreviewChannels = value;
                RaisePropertyChanged("IsPreviewChannels");
                if (_IsPreviewChannels)
                {
                    if (_PreviewContrastWindow == null)
                    {
                        // Display preview channels/contrast window
                        _PreviewContrastWindow = new ContrastSettingsWindow();
                        _PreviewContrastWindow.DataContext = Workspace.This.FluorescenceVM;
                        _PreviewContrastWindow.Owner = Workspace.This.Owner;
                        //_PreviewContrastWindow.Topmost = true;
                        _PreviewContrastWindow.Show();
                    }
                    _ImagingVm.ContrastVm.NumOfChannels = SelectedAppProtocol.SelectedScanRegion.SignalCount;
                    if (_ImagingVm.ContrastVm.NumOfChannels > 1)
                    {
                        _ImagingVm.IsContrastChannelAllowed = true;
                    }
                    else
                    {
                        _ImagingVm.IsContrastChannelAllowed = false;
                    }
                }
                else
                {
                    if (_PreviewContrastWindow != null)
                    {
                        if (_PreviewContrastWindow.IsLoaded)
                        {
                            // Close preview channels/contrast window
                            _PreviewContrastWindow.Close();
                            _PreviewContrastWindow = null;
                        }
                    }
                }
            }
        }

        public bool IsEdrScanning
        {
            get
            {
                bool bResult = false;
                if (SelectedAppProtocol != null)
                {
                    if (SelectedAppProtocol.ScanRegions != null)
                    {
                        for (int i = 0; i < SelectedAppProtocol.ScanRegions.Count; i++)
                        {
                            if (SelectedAppProtocol.ScanRegions[i].IsEdrScanning)
                            {
                                bResult = true;
                                break;
                            }
                        }
                    }
                }
                return bResult;
            }
        }

        //public List<int> DynamicBitsOptions { get; }
        //private int _SelectedDynamicBits;
        //public int SelectedDynamicBits
        //{
        //    get { return _SelectedDynamicBits; }
        //    set
        //    {
        //        if (_SelectedDynamicBits != value)
        //        {
        //            _SelectedDynamicBits = value;
        //            RaisePropertyChanged(nameof(SelectedDynamicBits));
        //        }
        //    }
        //}

        #endregion


        #region AddScanSignalCommand

        private RelayCommand _AddScanSignalCommand = null;
        public ICommand AddScanSignalCommand
        {
            get
            {
                if (_AddScanSignalCommand == null)
                {
                    _AddScanSignalCommand = new RelayCommand(ExecuteAddScanSignalCommand, CanExecuteAddScanSignalCommand);
                }

                return _AddScanSignalCommand;
            }
        }

        public void ExecuteAddScanSignalCommand(object parameter)
        {
            if (SelectedAppProtocol.SelectedScanRegion.SignalCount < 4)
            {
                if (!SelectedAppProtocol.IsModified)
                {
                    SelectedAppProtocol.IsModified = true;
                }

                int nLaserIndex = 0;
                int nColorChannelIndex = 0;
                bool bAvailLaserFound = false;
                var signalList = new ObservableCollection<SignalViewModel>(SelectedAppProtocol.SelectedScanRegion.SignalList);
                if (signalList.Count > 0)
                {
                    // Automatically select an un-use laser type
                    var bIsLaserL1InUse = signalList.Any(item => item.SelectedLaser.LaserChannel == LaserChannels.ChannelC);
                    var bIsLaserR1InUse = signalList.Any(item => item.SelectedLaser.LaserChannel == LaserChannels.ChannelA);
                    var bIsLaserR2InUse = signalList.Any(item => item.SelectedLaser.LaserChannel == LaserChannels.ChannelB);
                    var bIsLaserL1Avail = signalList[0].LaserOptions.Any(item => item.LaserChannel == LaserChannels.ChannelC);
                    var bIsLaserR1Avail = signalList[0].LaserOptions.Any(item => item.LaserChannel == LaserChannels.ChannelA);
                    var bIsLaserR2Avail = signalList[0].LaserOptions.Any(item => item.LaserChannel == LaserChannels.ChannelB);
                    if (!bIsLaserL1InUse && bIsLaserL1Avail)
                    {
                        var itemsFound = signalList[0].LaserOptions.Where(item => item.LaserChannel == LaserChannels.ChannelC).ToList();
                        if (itemsFound != null && itemsFound.Count > 0)
                        {
                            nLaserIndex = (int)signalList[0].LaserOptions.IndexOf(itemsFound[0]);
                            bAvailLaserFound = true;
                        }
                    }
                    else if (!bIsLaserR1InUse && bIsLaserR1Avail)
                    {
                        var itemsFound = signalList[0].LaserOptions.Where(item => item.LaserChannel == LaserChannels.ChannelA).ToList();
                        if (itemsFound != null && itemsFound.Count > 0)
                        {
                            nLaserIndex = (int)signalList[0].LaserOptions.IndexOf(itemsFound[0]);
                            bAvailLaserFound = true;
                        }
                    }
                    else if (!bIsLaserR2InUse && bIsLaserR2Avail)
                    {
                        var itemsFound = signalList[0].LaserOptions.Where(item => item.LaserChannel == LaserChannels.ChannelB).ToList();
                        if (itemsFound != null && itemsFound.Count > 0)
                        {
                            nLaserIndex = (int)signalList[0].LaserOptions.IndexOf(itemsFound[0]);
                            bAvailLaserFound = true;
                        }
                    }

                    if (bAvailLaserFound)
                    {
                        // Automatically select an un-use color channel
                        var bIsRedChannelInUse = signalList.Any(item => item.SelectedColorChannel.ImageColorChannel == ImageChannelType.Red);
                        var bIsGreenChannelInUse = signalList.Any(item => item.SelectedColorChannel.ImageColorChannel == ImageChannelType.Green);
                        var bIsBlueChannelInUse = signalList.Any(item => item.SelectedColorChannel.ImageColorChannel == ImageChannelType.Blue);
                        var bIsGrayChannelInUse = signalList.Any(item => item.SelectedColorChannel.ImageColorChannel == ImageChannelType.Gray);
                        if (!bIsRedChannelInUse)
                            nColorChannelIndex = 0;
                        else if (!bIsGreenChannelInUse)
                            nColorChannelIndex = 1;
                        else if (!bIsBlueChannelInUse)
                            nColorChannelIndex = 2;
                        else if (!bIsGrayChannelInUse)
                            nColorChannelIndex = 3;
                    }
                }

                if (bAvailLaserFound)
                {
                    List<int> keyList = new List<int>(SettingsManager.ConfigSettings.LasersSignalList.Keys);
                    SignalViewModel signal = new SignalViewModel(SettingsManager.ConfigSettings.LasersSignalList[keyList[0]].Count);
                    //signal.LaserOptions = new ObservableCollection<LaserTypes>(Workspace.This.LaserOptions);
                    signal.SelectedLaser = signal.LaserOptions[nLaserIndex];
                    signal.SelectedColorChannel = signal.ColorChannelOptions[nColorChannelIndex];
                    signal.SelectedSignalLevel = signal.SignalLevelOptions[4];
                    signal.SignalChanged += new SignalViewModel.SignalChangedDelegate(Protocol_SignalChanged);
                    signalList.Add(signal);
                    SelectedAppProtocol.SelectedScanRegion.SignalList = signalList;
                    RaisePropertyChanged("SelectedAppProtocol");

                    // Setup preview contrast channel
                    // Set color channel to newly selected laser type (dye)
                    SetPreviewColorChannel(signal.SelectedLaser, signal.SelectedColorChannel.ImageColorChannel);
                    // Show the newly selected laser type (or dye)
                    SetPreviewLaserVisibility(signal.SelectedLaser.LaserChannel, true);
                }
                else
                {
                    string caption = "Sapphire FL Biomolecular Imager";
                    string message = "Cannot add another scan channel. All available lasers are in used";
                    Xceed.Wpf.Toolkit.MessageBox.Show(Workspace.This.Owner, message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                    return;
                }
            }
        }

        public bool CanExecuteAddScanSignalCommand(object parameter)
        {
            bool bResult = false;
            if (SelectedAppProtocol != null)
            {
                if (SelectedAppProtocol.SelectedScanRegion.SignalCount < 4)
                {
                    bResult = true;
                }
            }
            return bResult;
        }

        #endregion

        #region DeleteScanSignalCommand

        private RelayCommand _DeleteScanSignalCommand = null;
        public ICommand DeleteScanSignalCommand
        {
            get
            {
                if (_DeleteScanSignalCommand == null)
                {
                    _DeleteScanSignalCommand = new RelayCommand(ExecuteDeleteScanSignalCommand, CanExecuteDeleteScanSignalCommand);
                }

                return _DeleteScanSignalCommand;
            }
        }
        private void ExecuteDeleteScanSignalCommand(object parameter)
        {
            if (SelectedAppProtocol.SelectedScanRegion.SignalList != null)
            {
                if (SelectedAppProtocol.SelectedScanRegion.SignalList.Count > 1)
                {
                    SignalViewModel selectedSignal = parameter as SignalViewModel;
                    if (selectedSignal != null)
                    {
                        if (!SelectedAppProtocol.IsModified)
                        {
                            // Mark as modified (and backup default settings)
                            SelectedAppProtocol.IsModified = true;
                        }

                        ObservableCollection<SignalViewModel> signalList = new ObservableCollection<SignalViewModel>(SelectedAppProtocol.SelectedScanRegion.SignalList);
                        selectedSignal.SignalChanged -= new SignalViewModel.SignalChangedDelegate(Protocol_SignalChanged);
                        signalList.Remove(selectedSignal);
                        SelectedAppProtocol.SelectedScanRegion.SignalList = signalList;

                        if (selectedSignal.SelectedLaser != null)
                        {
                            ResetPreviewImages();
                            // Hide the preview laser type of the deleted signal
                            SetPreviewLaserVisibility(selectedSignal.SelectedLaser.LaserChannel, false);
                        }
                    }
                }
            }
        }
        private bool CanExecuteDeleteScanSignalCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region SaveProtocolCommand

        public ICommand SaveProtocolCommand
        {
            get
            {
                if (_SaveProtocolCommand == null)
                {
                    _SaveProtocolCommand = new RelayCommand(ExecuteSaveProtocolCommand, CanExecuteSaveProtocolCommand);
                }

                return _SaveProtocolCommand;
            }
        }
        public void ExecuteSaveProtocolCommand(object parameter)
        {
            // Now allowing custom focus to be save without first creating a custom sample type
            //if (SelectedAppProtocol.SelectedSampleType.DisplayName.Equals("Custom", StringComparison.OrdinalIgnoreCase))
            //{
            //    string caption = "Create new protocol...";
            //    string message = "Please select a sample type or create a new sample type with the current focus value.";
            //    Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
            //    return;
            //}

            SaveProtocolWindow saveProtocolWin = new SaveProtocolWindow();
            saveProtocolWin.Owner = Workspace.This.Owner;
            saveProtocolWin.DataContext = this;
            bool? dialogResult = saveProtocolWin.ShowDialog();
            if (dialogResult == true)
            {
                // save item to the config file
                if (SaveAppProtocol(SelectedAppProtocol, saveProtocolWin.ProtocolName))
                {
                    // Add item to protocol options list
                    ProtocolViewModel protocol = new ProtocolViewModel(SelectedAppProtocol);
                    //protocol.ProtocolOptionChanged += new ProtocolViewModel.ProtocolOptionChangedDelegate(Protocol_ProtocolOptionChanged);
                    //protocol.PixelSizeOptions = new List<ResolutionType>(SettingsManager.ConfigSettings.ResolutionOptions);
                    //protocol.ScanSpeedOptions = new List<ScanSpeedType>(SettingsManager.ConfigSettings.ScanSpeedOptions);
                    //protocol.SampleTypeOptions = new List<SampleTypeSetting>(SettingsManager.ConfigSettings.SampleTypeSettings);
                    protocol.ProtocolName = saveProtocolWin.ProtocolName;  //new protocol name
                    ObservableCollection<ProtocolViewModel> tmpProtocols = new ObservableCollection<ProtocolViewModel>(_AppProtocolOptions);
                    tmpProtocols.Add(protocol);
                    AppProtocolOptions = tmpProtocols;
                    SelectedAppProtocol = protocol;
                    SelectedAppProtocol.IsModified = false;
                }
            }
        }
        public bool CanExecuteSaveProtocolCommand(object parameter)
        {
            return true;
        }

        public bool ValidateProtocolName(string protocolName)
        {
            bool bResult = true;

            // Check if the protocol name is already exists
            if (AppProtocolOptions != null)
            {
                foreach (var protocol in AppProtocolOptions)
                {
                    if (string.Equals(protocol.ProtocolName, protocolName, StringComparison.OrdinalIgnoreCase))
                    {
                        string caption = "Protocol name already exists.";
                        string message = "The protocol name already exists. Please enter a different name.";
                        Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                        //saveProtocolWin.DialogResult = false;
                        bResult = false;
                    }
                }
            }

            return bResult;
        }
        #endregion

        #region DeleteProtocolCommand

        public ICommand DeleteProtocolCommand
        {
            get
            {
                if (_DeleteProtocolCommand == null)
                {
                    _DeleteProtocolCommand = new RelayCommand(ExecuteDeleteProtocolCommand, CanExecuteDeleteProtocolCommand);
                }

                return _DeleteProtocolCommand;
            }
        }
        public void ExecuteDeleteProtocolCommand(object parameter)
        {
            if (SelectedAppProtocol != null)
            {
                //string caption = "Delete protocol...";
                //string message = string.Format("Are you sure you want to delete the protocol \"{0}\".", SelectedAppProtocol.ProtocolName);
                //MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.YesNo, MessageBoxImage.Question);
                //if (result == MessageBoxResult.No)
                //{
                //    return;
                //}
                if (SelectedAppProtocol.IsAlwaysVisible)
                {
                    DeleteProtocol deleteProto = new DeleteProtocol();
                    deleteProto.Message = "Default protocols cannot be deleted.";
                    deleteProto.ProtocolName = SelectedAppProtocol.ProtocolName;
                    deleteProto.IsCancelButtonVisibled = false;
                    var dlgResult = deleteProto.ShowDialog();
                    return;
                }
                else
                {
                    DeleteProtocol deleteProto = new DeleteProtocol();
                    deleteProto.Message = "Are you sure you want to delete the protocol?";
                    deleteProto.ProtocolName = SelectedAppProtocol.ProtocolName;
                    deleteProto.IsCancelButtonVisibled = true;
                    var dlgResult = deleteProto.ShowDialog();
                    if (dlgResult == false) { return; }

                    // Delete selected item from the config file
                    DeleteAppProtocol(SelectedAppProtocol.ProtocolName);

                    // Select the next item after removing the selected item
                    int nextItem = _AppProtocolOptions.IndexOf(SelectedAppProtocol) - 1;

                    // Delete selected item from protocol options list
                    ObservableCollection<ProtocolViewModel> tmpProtocols = new ObservableCollection<ProtocolViewModel>(_AppProtocolOptions);
                    tmpProtocols.Remove(SelectedAppProtocol);
                    AppProtocolOptions = tmpProtocols;

                    if (nextItem < 0) { nextItem = 0; }
                    SelectedAppProtocol = _AppProtocolOptions[nextItem];
                    ResetPreviewImages();
                }
            }
        }
        public bool CanExecuteDeleteProtocolCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region AddScanRegionCommand

        private RelayCommand _AddScanRegionCommand = null;
        public ICommand AddScanRegionCommand
        {
            get
            {
                if (_AddScanRegionCommand == null)
                {
                    _AddScanRegionCommand = new RelayCommand(ExecuteAddScanRegionCommand, CanExecuteAddScanRegionCommand);
                }
                return _AddScanRegionCommand;
            }
        }
        private void ExecuteAddScanRegionCommand(object parameter)
        {
            if (Workspace.This.IsScanning) { return; }

            if (SelectedAppProtocol != null)
            {
                if (!SelectedAppProtocol.IsModified)
                {
                    // Mark as modified (and backup default settings)
                    SelectedAppProtocol.IsModified = true;
                }

                // Add a new scan region
                int nScanRegionId = _ImagingVm.GetAdornerId();
                ScanRegionViewModel scanRegionVm = new ScanRegionViewModel
                {
                    ScanRegionNum = nScanRegionId,
                    CellSize = _ImagingVm.CellSize,
                    NumOfCells = _ImagingVm.NumOfCells
                };
                ScanRegionRect scanRect = null;

                if (SelectedAppProtocol.SelectedScanRegion != null)
                {
                    // Duplicate the scan settings from the current selected scan region
                    //
                    //var signalList = new ObservableCollection<SignalViewModel>(SelectedAppProtocol.SelectedScanRegion.SignalList);
                    scanRect = (ScanRegionRect)SelectedAppProtocol.SelectedScanRegion.ScanRect.Clone();
                    int pixelSize = SelectedAppProtocol.SelectedScanRegion.SelectedPixelSize.Value;
                    int scanSpeed = SelectedAppProtocol.SelectedScanRegion.SelectedScanSpeed.Value;
                    int scanQuality = SelectedAppProtocol.SelectedScanRegion.SelectedScanQuality.Value;
                    scanRegionVm.SelectedPixelSize = scanRegionVm.PixelSizeOptions.FirstOrDefault(x => x.Value == pixelSize);
                    scanRegionVm.SelectedScanSpeed = scanRegionVm.ScanSpeedOptions.FirstOrDefault(x => x.Value == scanSpeed);
                    scanRegionVm.SelectedScanQuality = scanRegionVm.ScanQualityOptions.FirstOrDefault(x => x.Value == scanQuality);
                    // Focus settings
                    if (SelectedAppProtocol.SelectedScanRegion.IsZScan)
                    {
                        scanRegionVm.IsCustomFocus = true;
                        scanRegionVm.ZScanSetting = (ZScanSetting)SelectedAppProtocol.SelectedScanRegion.ZScanSetting.Clone();
                    }

                    var sampleType = SelectedAppProtocol.SelectedScanRegion.SelectedSampleType;
                    scanRegionVm.SelectedSampleType = scanRegionVm.SampleTypeOptions.FirstOrDefault(x => x.DisplayName == sampleType.DisplayName);
                    if (SelectedAppProtocol.SelectedScanRegion.IsCustomFocus)
                    {
                        scanRegionVm.IsCustomFocus = true;
                        scanRegionVm.CustomFocusValue = SelectedAppProtocol.SelectedScanRegion.CustomFocusValue;
                    }

                    scanRegionVm.IsEdrScanning = SelectedAppProtocol.SelectedScanRegion.IsEdrScanning;
                    if (SelectedAppProtocol.SelectedScanRegion.IsSequentialScanAllowed)
                    {
                        scanRegionVm.IsSequentialScanning = SelectedAppProtocol.SelectedScanRegion.IsSequentialScanning;
                    }

                    // Duplicate current selected scann region scan signal (laser/intensity/color channel
                    List<int> keyList = new List<int>(SettingsManager.ConfigSettings.LasersSignalList.Keys);
                    foreach (var selectedRegionSignal in SelectedAppProtocol.SelectedScanRegion.SignalList)
                    {
                        var signal = new SignalViewModel(SettingsManager.ConfigSettings.LasersSignalList[keyList[0]].Count);
                        // Find and select laser type
                        int index = signal.LaserOptions.ToList().FindIndex(x => x.LaserChannel == selectedRegionSignal.SelectedLaser.LaserChannel);
                        signal.SelectedLaser = signal.LaserOptions[index];

                        //int laser = selectedRegionSignal.SelectedLaser.;
                        //signal.SelectedLaser = signal.LaserOptions.FirstOrDefault(x => x.Value == pixelSize);
                        // Find and set signal level
                        signal.SelectedSignalLevel = signal.SignalLevelOptions.Where(x => x.DisplayName == selectedRegionSignal.SelectedSignalLevel.DisplayName).FirstOrDefault();

                        // Select the 'red' channel by default
                        //signal.SelectedColorChannel = signal.ColorChannelOptions[0];
                        // Find and select the color channel
                        signal.SelectedColorChannel = signal.ColorChannelOptions.Where(x => x.ImageColorChannel == selectedRegionSignal.SelectedColorChannel.ImageColorChannel).FirstOrDefault();

                        signal.SignalChanged += new SignalViewModel.SignalChangedDelegate(Protocol_SignalChanged);
                        scanRegionVm.SignalList.Add(signal);
                    }

                    scanRegionVm.FileLocationVm.IsAutoSave = SelectedAppProtocol.SelectedScanRegion.FileLocationVm.IsAutoSave;
                    if (scanRegionVm.FileLocationVm.IsAutoSave)
                    {
                        string fileName = SelectedAppProtocol.SelectedScanRegion.FileLocationVm.FileName;
                        if (string.IsNullOrEmpty(fileName))
                        {
                            fileName = Workspace.This.GenerateFileName(string.Empty, string.Empty);
                        }
                        else
                        {
                            fileName = string.Format("{0} ({1})", fileName, nScanRegionId);
                        }
                        scanRegionVm.FileLocationVm.FileName = fileName;
                        scanRegionVm.FileLocationVm.DestinationFolder = SelectedAppProtocol.SelectedScanRegion.FileLocationVm.DestinationFolder;
                    }
                }
                else
                {
                    List<int> keyList = new List<int>(SettingsManager.ConfigSettings.LasersSignalList.Keys);
                    var signal = new SignalViewModel(SettingsManager.ConfigSettings.LasersSignalList[keyList[0]].Count);
                    //signal.IsInitializing = true;
                    if (signal != null && signal.LaserOptions != null && signal.LaserOptions.Count > 0)
                    {
                        signal.SelectedLaser = signal.LaserOptions[0];

                        // Select signal level 1 by default
                        if (signal.SignalLevelOptions != null)
                        {
                            signal.SelectedSignalLevel = signal.SignalLevelOptions.Where(x => x.DisplayName == "1").FirstOrDefault();
                        }

                        // Select the 'red' channel by default
                        signal.SelectedColorChannel = signal.ColorChannelOptions[0];
                    }
                    //signal.IsInitializing = false;
                    signal.SignalChanged += new SignalViewModel.SignalChangedDelegate(Protocol_SignalChanged);
                    scanRegionVm.SignalList.Add(signal);

                    scanRegionVm.SelectedPixelSize = scanRegionVm.PixelSizeOptions.FirstOrDefault(x => x.Value == 200);
                    scanRegionVm.SelectedScanSpeed = scanRegionVm.ScanSpeedOptions.FirstOrDefault(x => x.DisplayName == "Highest");
                    scanRegionVm.SelectedScanQuality = scanRegionVm.ScanQualityOptions.FirstOrDefault(x => x.Position == 1);
                    scanRegionVm.SelectedSampleType = scanRegionVm.SampleTypeOptions.FirstOrDefault(x => x.DisplayName.Contains("Membrane"));

                    double left = 0;
                    double top = 0; //NOTE: region is flipped vertically
                    if (SelectedAppProtocol.ScanRegions.Count > 0)
                    {
                        switch (nScanRegionId)
                        {
                            case 2:
                                left = 10 * _ImagingVm.CellSize;
                                top = 0;
                                break;
                            case 3:
                                left = 0;
                                top = 10 * _ImagingVm.CellSize;
                                break;
                            case 4:
                                left = 10 * _ImagingVm.CellSize;
                                top = 10 * _ImagingVm.CellSize;
                                break;
                            default:
                                if (SelectedAppProtocol.ScanRegions.Count >= 4 && SelectedAppProtocol.ScanRegions.Count < 19)
                                {
                                    int offset = SelectedAppProtocol.ScanRegions.Count - 3;
                                    left = offset * _ImagingVm.CellSize;
                                    top = offset * _ImagingVm.CellSize;
                                }
                                else if (SelectedAppProtocol.ScanRegions.Count >= 19)
                                {
                                    int offset = SelectedAppProtocol.ScanRegions.Count % 19;
                                    left = offset * _ImagingVm.CellSize;
                                    top = offset * _ImagingVm.CellSize;
                                }
                                break;
                        }
                    }
                    double width = _ImagingVm.CellSize * 10;
                    double height = _ImagingVm.CellSize * 10;
                    scanRect = new ScanRegionRect(left, top, width, height);
                }

                scanRegionVm.ScanRect = scanRect;
                scanRegionVm.ScanRectChanged += ScanRegion_ScanRectChanged;
                scanRegionVm.ScanRegionSettingChanged += ScanRegion_ScanRegionSettingChanged;

                SelectedAppProtocol.AddScanRegion(scanRegionVm, false);

                // Add scan region adorner
                _ImagingVm.IsInitializing = true;   // don't trigger scan rect changed event
                _ImagingVm.AddScanRegionSelection(nScanRegionId, scanRect, scanRegionVm.Color);
                _ImagingVm.IsInitializing = false;

                // Selected the added scan region
                if (SelectedAppProtocol.ScanRegions != null)
                {
                    SelectedAppProtocol.SelectedScanRegion = SelectedAppProtocol.ScanRegions.Where(x => x.ScanRegionNum == nScanRegionId).FirstOrDefault();
                }
            }
        }
        private bool CanExecuteAddScanRegionCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region RemoveScanRegionCommand
        private RelayCommand _RemoveScanRegionCommand = null;
        public ICommand RemoveScanRegionCommand
        {
            get
            {
                if (_RemoveScanRegionCommand == null)
                {
                    _RemoveScanRegionCommand = new RelayCommand(ExecuteRemoveScanRegionCommand, CanExecuteRemoveScanRegionCommand);
                }
                return _RemoveScanRegionCommand;
            }
        }
        private void ExecuteRemoveScanRegionCommand(object parameter)
        {
            if (Workspace.This.IsScanning) { return; }

            if (SelectedAppProtocol != null)
            {
                var scanRegion = parameter as ScanRegionViewModel;
                if (scanRegion != null)
                {
                    foreach (var signal in scanRegion.SignalList)
                    {
                        signal.SignalChanged -= new SignalViewModel.SignalChangedDelegate(Protocol_SignalChanged);
                    }
                    scanRegion.ScanRectChanged -= ScanRegion_ScanRectChanged;
                    scanRegion.ScanRegionSettingChanged -= ScanRegion_ScanRegionSettingChanged;
                    _ImagingVm.RemoveScanRegionSelection(scanRegion);   // remove scan region adorner
                    SelectedAppProtocol.RemoveScanRegion(scanRegion);
                    if (_SelectedAppProtocol.SelectedScanRegion != null)
                    {
                        ScanTime = ImagingSystemHelper.FormatTime(_SelectedAppProtocol.SelectedScanRegion.GetScanTime());
                    }
                    // Remove preview image
                    if (_ImagingVm.PreviewImages != null &&
                        _ImagingVm.PreviewImages.Count >= scanRegion.ScanRegionNum)
                    {
                        for (int i= _ImagingVm.PreviewImages.Count -1; i >= 0; i--)
                        {
                            if (scanRegion.ScanRegionNum == _ImagingVm.PreviewImages[i].RegionNumber)
                            {
                                _ImagingVm.PreviewImages.Remove(_ImagingVm.PreviewImages[i]);
                                break;
                            }
                        }
                    }

                    // Update preview channels
                    if (_SelectedAppProtocol.SelectedScanRegion != null)
                    {
                        ScanTime = ImagingSystemHelper.FormatTime(_SelectedAppProtocol.SelectedScanRegion.GetScanTime());
                        _SelectedAppProtocol.SelectedScanRegion.FileLocationVm.FileSize = GetFileSize();
                        UpdateScanRegionPreviewChannels();
                    }
                }
            }
        }
        private bool CanExecuteRemoveScanRegionCommand(object parameter)
        {
            return true;
        }
        #endregion


        #region StartScanCommand

        public ICommand StartScanCommand
        {
            get
            {
                if (_StartScanCommand == null)
                {
                    _StartScanCommand = new RelayCommand(ExecuteStartScanCommand, CanExecuteStartScanCommand);
                }

                return _StartScanCommand;
            }
        }
        public void ExecuteStartScanCommand(object parameter)
        {
            if (_SelectedAppProtocol == null)
            {
                string caption = "No scan protocol selected...";
                string message = "No scan protocol selected\nPlease select a scan protocol and try again.";
                Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _CurrentScanType = (ScanType)parameter;
            if (_CurrentScanType == null)
            {
                _CurrentScanType = ScanType.Normal;
            }
            if (_CurrentScanType == ScanType.Auto)
            {
                _IsAutoScan = true;
            }


            if (!SettingsManager.ConfigSettings.IsSimulationMode && Workspace.This.EthernetController.IsConnected)
            {
                Workspace.This.NewParameterVM.ExecuteParametersReadCommand(null);
            }

            List<ScanParameterStruct> scanParams = new List<ScanParameterStruct>();

            // Validate each scan region selected signals settings
            foreach (var scanRegion in SelectedAppProtocol.ScanRegions)
            {
                if (scanRegion.SelectedPixelSize == null || scanRegion.SelectedScanSpeed == null || scanRegion.SelectedSampleType == null ||
                    (scanRegion.SignalCount > 0 && (scanRegion.SignalList[0].SelectedLaser == null || scanRegion.SignalList[0].SelectedColorChannel == null)))
                {
                    string caption = "Incorrect scan parameters...";
                    string message = "Please make sure the scan parameters are correctly set up and try again.";
                    Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Don't allow duplicate dye/laser type or duplicate color channel
                if (scanRegion.SignalList.Count > 1)
                {
                    List<string> colorChannels = new List<string>();
                    //List<LaserChannels> lasersChannels = new List<LaserChannels>();
                    foreach (var signal in scanRegion.SignalList)
                    {
                        colorChannels.Add(signal.SelectedColorChannel.DisplayName);
                        //lasersChannels.Add(signal.SelectedLaser.LaserChannel);
                    }
                    // Find duplicate color channels
                    var duplicatedColors = colorChannels.GroupBy(x => x)
                          .Where(g => g.Count() > 1)
                          .Select(y => y.Key)
                          .ToList();
                    if (duplicatedColors != null && duplicatedColors.Count > 0)
                    {
                        string caption = "Sapphire FL Biomolecular Imager";
                        string message = string.Format("Duplicate color channels [{0}] in {1}.\nPlease make sure there's no duplicate imaging color channels.", duplicatedColors[0], scanRegion.ScanRegionHeader);
                        Xceed.Wpf.Toolkit.MessageBox.Show(Workspace.This.Owner, message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                        return;
                    }
                    // Don't check if there are duplicate lasers type; we're now allowing the lasers with the same wavelength to be scanned at the same time.
                    // Find duplicate laser types
                    //var duplicatedLasers = lasersChannels.GroupBy(x => x)
                    //  .Where(g => g.Count() > 1)
                    //  .Select(y => y.Key)
                    //  .ToList();
                    //if (duplicatedLasers != null && duplicatedLasers.Count > 0)
                    //{
                    //    string caption = "Sapphire FL Biomolecular Imager";
                    //
                    //    string message = string.Format("Duplicate laser type selected [Laser: {0}] in {1}.\nPlease make sure there's no duplicate laser selected.", (int)duplicatedLasers[0], scanRegion.ScanRegionHeader);
                    //    Xceed.Wpf.Toolkit.MessageBox.Show(Workspace.This.Owner, message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                    //    return;
                    //}
                }

                // Set unidrectional scan (using the NEW 'Quality' selection option)
                // Unidirection (or 2-line average scan)
                bool bIsUnidirectionalScan = false;
                // Sawtooth correction
                bool bIsPixelOffsetProcessing = SettingsManager.ConfigSettings.PixelOffsetProcessing;
                // 1 = unidrectional OFF ('High'), 2 = unidirectional ON ('Highest') (see: ScanQualities table in config.xml)
                if (scanRegion.SelectedScanQuality.Position == 2)
                {
                    // Enable unidirectional scan
                    bIsUnidirectionalScan = true;
                    // Don't turn on sawtooth correction if unidirectional (or 2-line average is) ON
                    bIsPixelOffsetProcessing = false;
                }

                ScanParameterStruct scanParam = new ScanParameterStruct();

                int scanResolution = scanRegion.SelectedPixelSize.Value;
                int scanSpeed = scanRegion.SelectedScanSpeed.Value;
                int width = 0;
                int height = 0;
                bool bIsEdrScanning = scanRegion.IsEdrScanning;
                // Preview scan
                if (_CurrentScanType == ScanType.Preview)
                {
                    scanResolution = 1000;          // Preview scans at 1000 micron
                    scanSpeed = 1;                  // Preview scans at highest speed (1 = Highest)
                    bIsUnidirectionalScan = false;  // Turn off unidirectional scan

                    if (_CurrentScanType == ScanType.Preview && _SelectedAppProtocol.SelectedScanRegion.IsZScan)
                    {
                        _ScanZ0 = _SelectedAppProtocol.SelectedScanRegion.ZScanSetting.BottomImageFocus * _ZMotorSubdivision;
                    }
                    // Turn off EDR scanning when doing preview scan
                    if (bIsEdrScanning)
                    {
                        bIsEdrScanning = false;
                    }
                }

                if (scanRegion.IsCustomFocus)
                {
                    // Sapphire FL: add to move up (original Sapphire subtract to move up)
                    _ScanZ0 = (Workspace.This.MotorVM.AbsZPos + scanRegion.CustomFocusValue) * _ZMotorSubdivision;
                }
                else
                {
                    // Focus position of the selected sample type.
                    // SampleType focus position now store focus position relative to the AbsFocusPosition (absolute focus position).
                    // Sapphire FL (+ to move up, SOG - to move up)
                    _ScanZ0 = (Workspace.This.MotorVM.AbsZPos + scanRegion.SelectedSampleType.FocusPosition) * _ZMotorSubdivision;
                }

                if (_ScanZ0 < 0 || _ScanZ0 > _ZMaxValue)
                {
                    if (_ScanZ0 < 0)
                        _ScanZ0 = Workspace.This.MotorVM.AbsZPos * _ZMotorSubdivision;
                    else if (_ScanZ0 * _ZMotorSubdivision > _ZMaxValue)
                        _ScanZ0 = _ZMaxValue;
                }

                // Set scan parameters
                //

                if (ImagingVm != null && ImagingVm.CellSize > 0 &&
                    scanRegion.ScanRect.Width > 0 && scanRegion.ScanRect.Height > 0)
                {
                    ScanDeltaX = (int)(_ScanWidthInMm * Math.Round(scanRegion.ScanRect.Width, 2) / (ImagingVm.CellSize * ImagingVm.NumOfCells));
                    ScanDeltaY = (int)(_ScanHeightInMm * Math.Round(scanRegion.ScanRect.Height, 2) / (ImagingVm.CellSize * ImagingVm.NumOfCells));
                    ScanX0 = (int)(_ScanWidthInMm * Math.Round(scanRegion.ScanRect.X, 2) / (ImagingVm.CellSize * ImagingVm.NumOfCells));
                    ScanY0 = (int)(_ScanHeightInMm * Math.Round(scanRegion.ScanRect.Y, 2) / (ImagingVm.CellSize * ImagingVm.NumOfCells));
                    //_ScanDeltaX = (int)Math.Round(_XMotorSubdivision * _ScanWidthInMm * scanRegion.ScanRect.Width / (ImagingVm.CellSize * ImagingVm.NumOfCells));
                    //_ScanDeltaY = (int)Math.Round(_YMotorSubdivision * _ScanHeightInMm * scanRegion.ScanRect.Height / (ImagingVm.CellSize * ImagingVm.NumOfCells));
                    //_ScanX0 = (int)Math.Round(_XMotorSubdivision * (_ScanWidthInMm * scanRegion.ScanRect.X / (ImagingVm.CellSize * ImagingVm.NumOfCells)) + (Workspace.This.MotorVM.AbsXPos * _XMotorSubdivision));
                    //_ScanY0 = (int)Math.Round(_YMotorSubdivision * (_ScanHeightInMm * scanRegion.ScanRect.Y / (ImagingVm.CellSize * ImagingVm.NumOfCells)) + (Workspace.This.MotorVM.AbsYPos * _YMotorSubdivision));

                    // Add the overscan width and height (using the same amount of overscan for X and Y direction)
                    _ScanDeltaX += (int)Math.Round(SettingsManager.ConfigSettings.YMotionExtraMoveLength * _XMotorSubdivision);
                    _ScanDeltaY += (int)Math.Round(SettingsManager.ConfigSettings.YMotionExtraMoveLength * _YMotorSubdivision);

                    if (scanResolution > 0)
                    {
                        width = (int)(ScanDeltaX * 1000.0 / scanResolution);
                        height = (int)(ScanDeltaY * 1000.0 / scanResolution);
                        if (bIsUnidirectionalScan)
                        {
                            Time = height * scanRegion.SelectedScanSpeed.Value;
                        }
                        else
                        {
                            Time = (int)(height * scanRegion.SelectedScanSpeed.Value / 2.0);
                        }
                    }
                }
                else
                {
                    string caption = "Sapphire FL Biomolecular Imager";
                    string message = string.Format("Scan Region #{0}\nInvalid scan ROI. Please re-select the scan area.", scanRegion.ScanRegionHeader);
                    Xceed.Wpf.Toolkit.MessageBox.Show(Workspace.This.Owner, message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                    return;
                }

                //启用了Y轴补偿
                //Y-axis compensation is enabled
                if (_IsYCompensationEnabled && (int)((_YCompensationOffset * 1000) / scanResolution) >= 1)
                {
                    _ScanY0 = _ScanY0 - (int)(_YCompensationOffset * _YMotorSubdivision);
                    _ScanDeltaY = (int)_ScanDeltaY + (int)(_YCompensationOffset * _YMotorSubdivision);
                }

                // Scan 5um at 10um and at the speed of 2
                scanParam.Is5micronScan = false;
                if (scanResolution == 5)
                {
                    scanResolution = 10;
                    if (scanSpeed < 2)
                    {
                        scanSpeed = 2;  // high (1 = highest)
                    }
                    scanParam.Is5micronScan = true;
                    width = (int)(ScanDeltaX * 1000.0 / scanResolution);
                    height = (int)(ScanDeltaY * 1000.0 / scanResolution);
                }

                if (bIsUnidirectionalScan)
                {
                    // Double the height on unidirectional scan to synchronize the scanner's LED progress bar
                    height *= 2;
                }

                scanParam.Width = width;
                scanParam.Height = height;
                scanParam.ScanDeltaX = (int)_ScanDeltaX;
                scanParam.ScanDeltaY = (int)_ScanDeltaY;
                scanParam.ScanDeltaZ = (int)_ScanDeltaZ;
                scanParam.ScanX0 = (int)_ScanX0;
                scanParam.ScanY0 = (int)_ScanY0;
                scanParam.ScanZ0 = (int)_ScanZ0;
                scanParam.AbsFocusPosition = Workspace.This.MotorVM.AbsZPos;
                scanParam.Res = scanResolution; // scan resolution in micron
                scanParam.Quality = scanSpeed;  // scan quality/speed (NOT Quality that refers to unidirectional scan)
                scanParam.Time = Time;
                scanParam.XMotorSubdivision = _XMotorSubdivision;
                scanParam.YMotorSubdivision = _YMotorSubdivision;
                scanParam.ZMotorSubdivision = _ZMotorSubdivision;
                //EL: TODO: do we need these for horizontal scan
                //scanParam.DataRate = DataRate;
                //scanParam.LineCounts = LineCounts;
                scanParam.DataRate = 25;
                scanParam.LineCounts = 1000;
                scanParam.XMotorSpeed = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[0].Speed * SettingsManager.ConfigSettings.XMotorSubdivision);
                scanParam.XMotionAccVal = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[0].Accel * SettingsManager.ConfigSettings.XMotorSubdivision);
                scanParam.XMotionDccVal = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[0].Dccel * SettingsManager.ConfigSettings.XMotorSubdivision);
                scanParam.YMotorSpeed = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[1].Speed * SettingsManager.ConfigSettings.YMotorSubdivision);
                scanParam.YMotionAccVal = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[1].Accel * SettingsManager.ConfigSettings.YMotorSubdivision);
                scanParam.YMotionDccVal = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[1].Dccel * SettingsManager.ConfigSettings.YMotorSubdivision);
                scanParam.ZMotorSpeed = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[2].Speed * SettingsManager.ConfigSettings.ZMotorSubdivision);
                scanParam.ZMotionAccVal = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[2].Accel * SettingsManager.ConfigSettings.ZMotorSubdivision);
                scanParam.ZMotionDccVal = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[2].Dccel * SettingsManager.ConfigSettings.ZMotorSubdivision);
                scanParam.IsNewFirmwire = Workspace.This.MotorVM.IsNewFirmware;
                scanParam.XmotionTurnAroundDelay = SettingsManager.ConfigSettings.XMotionTurnDelay;
                scanParam.XMotionExtraMoveLength = SettingsManager.ConfigSettings.XMotionExtraMoveLength;
                scanParam.YMotionExtraMoveLength = SettingsManager.ConfigSettings.YMotionExtraMoveLength;
                scanParam.LCoefficient = Workspace.This.NewParameterVM.LCoefficient;
                scanParam.L375Coefficient = Workspace.This.NewParameterVM.L375Coefficient;
                scanParam.R1Coefficient = Workspace.This.NewParameterVM.R1Coefficient;
                scanParam.R2Coefficient = Workspace.This.NewParameterVM.R2Coefficient;
                scanParam.R2532Coefficient = Workspace.This.NewParameterVM.R2532Coefficient;
                scanParam.IsIgnoreCompCoefficient = SettingsManager.ConfigSettings.IsIgnoreCompCoefficient;
                //scanParam.DynamicBits = SelectedDynamicBits;
                //Grating ruler pulse
                scanParam.XEncoderSubdivision = Workspace.This.NewParameterVM.XEncoderSubdivision;
                if (SettingsManager.ConfigSettings.IsSimulationMode)
                {
                    scanParam.XEncoderSubdivision = SettingsManager.ConfigSettings.XEncoderSubdivision;
                }
                //scanParam.HorizontalCalibrationSpeed = HorizontalCalibrationSpeed;    //EL: TODO: do we need this for horizontal scan
                //是否启用动态为补偿   Whether to enable dynamic as compensation
                //scanParam.DynamicBitsAt =  SettingsManager.ApplicationSettings.IsExtDynamicRangeEnabled;
                //scanParam.IsUnidirectionalScan = SettingsManager.ConfigSettings.AllModuleProcessing;    // 2 lines average
                scanParam.IsUnidirectionalScan = bIsUnidirectionalScan;
                //scanParam.IsUnidirectionalScan = SettingsManager.ConfigSettings.IsFluorescence2LinesAvgScan;
                scanParam.EdrScaleFactor = SettingsManager.ConfigSettings.EdrScaleFactor;
                scanParam.IsEdrScanning = bIsEdrScanning;
                //_IsEdrScanning = bIsEdrScanning;

                //（因为在设计中将R1透镜的位置和R2透镜的位置更换了，然后参数名没有更改成对应的通道，所以下面参数名R1其实是R2,R2其实是R1）....
                // (in the design, the position of R1 lens and R2 lens is changed, and the parameter name is not changed to the corresponding channel, so the following parameter name R1 is actually R2, R2 is actually R1)....
                //L透镜到R2透镜之间的距离（mm），毫米为单位
                //The distance between L lens and R2 lens (mm), in millimeters
                int OpticalL_R1Distance = (int)Workspace.This.NewParameterVM.OpticalL_R1Distance;
                if (SettingsManager.ConfigSettings.IsSimulationMode)
                {
                    OpticalL_R1Distance = 48;
                    Workspace.This.NewParameterVM.OpticalL_R1Distance = OpticalL_R1Distance;
                }
                //根据当前选择的分辨率计算L透镜到R2透镜之间的实际像素宽度
                //// Calculate the actual pixel width between L lens and R2 lens based on the currently selected resolution
                int OffsetWidth = (int)(OpticalL_R1Distance * 1000.0 / scanParam.Res);
                //扫描图像宽度加上补偿透镜之间距离的宽度（像素）
                // The width of the scanned image plus the width of the distance between the compensating lenses (pixels)
                int currentRangeWidth = scanParam.Width + OffsetWidth;
                int currentRangeHeight = scanParam.Height;
                scanParam.Width = currentRangeWidth;
                //将L到R2透镜之间的距离（mm）累加到ScanDeltaX上
                //Add the distance (mm) between L and R2 lens to ScanDeltaX
                int offsetDeltaXPulse = OpticalL_R1Distance;
                //if (Workspace.This.NewParameterVM.OpticalL_R1Distance > 0 && scanParam.Res == 150)
                //{
                //    offsetDeltaXPulse = (int)Workspace.This.NewParameterVM.OpticalL_R1Distance - 1;
                //}
                //else
                //{
                //    offsetDeltaXPulse = (int)Workspace.This.NewParameterVM.OpticalL_R1Distance;
                //}
                offsetDeltaXPulse = (int)(offsetDeltaXPulse * _XMotorSubdivision);
                scanParam.ScanDeltaX += offsetDeltaXPulse;

                //scanParam.Width = (int)(scanParam.ScanDeltaX / scanParam.XMotorSubdivision * 1000.0 / scanParam.Res);
                //TODO: temporary work-around: make sure the width is an even value to avoid the skewed on the scanned image
                if (scanParam.Width % 2 != 0)
                {
                    //int deltaX = scanParam.ScanDeltaX - (int)Math.Round(scanParam.Res / 1000.0 * scanParam.XMotorSubdivision);
                    //scanParam.ScanDeltaX = deltaX;
                    //scanParam.Width = (int)(scanParam.ScanDeltaX / scanParam.XMotorSubdivision * 1000.0 / scanParam.Res);
                    scanParam.Width--;
                    scanParam.ScanDeltaX = (int)(scanParam.Width * scanParam.Res / 1000.0 * _XMotorSubdivision);
                }

                scanParam.AlignmentParam = new ImageAlignParam();
                scanParam.AlignmentParam.Resolution = scanParam.Res;
                scanParam.AlignmentParam.PixelOddX = SettingsManager.ConfigSettings.XOddNumberedLine;   //X odd Line
                scanParam.AlignmentParam.PixelEvenX = SettingsManager.ConfigSettings.XEvenNumberedLine; //X even Line
                scanParam.AlignmentParam.PixelOddY = SettingsManager.ConfigSettings.YOddNumberedLine;   //Y odd Line
                scanParam.AlignmentParam.PixelEvenY = SettingsManager.ConfigSettings.YEvenNumberedLine; //Y Even Line
                scanParam.AlignmentParam.YCompOffset = SettingsManager.ConfigSettings.YCompenOffset;
                scanParam.AlignmentParam.OpticalL_R1Distance = Workspace.This.NewParameterVM.OpticalL_R1Distance;
                scanParam.AlignmentParam.Pixel_10_L_DX = Workspace.This.NewParameterVM.Pixel_10_L_DX;
                scanParam.AlignmentParam.Pixel_10_L_DY = Workspace.This.NewParameterVM.Pixel_10_L_DY;
                scanParam.AlignmentParam.OpticalR2_R1Distance = Workspace.This.NewParameterVM.OpticalR2_R1Distance;
                scanParam.AlignmentParam.Pixel_10_R2_DX = Workspace.This.NewParameterVM.Pixel_10_R2_DX;
                scanParam.AlignmentParam.Pixel_10_R2_DY = Workspace.This.NewParameterVM.Pixel_10_R2_DY;
                scanParam.AlignmentParam.IsImageOffsetProcessing = SettingsManager.ConfigSettings.ImageOffsetProcessing;
                //scanParam.AlignmentParam.IsPixelOffsetProcessing = SettingsManager.ConfigSettings.PixelOffsetProcessing;
                scanParam.AlignmentParam.IsPixelOffsetProcessing = bIsPixelOffsetProcessing;
                scanParam.AlignmentParam.PixelOffsetProcessingRes = SettingsManager.ConfigSettings.PixelOffsetProcessingRes;
                scanParam.AlignmentParam.IsYCompensationBitAt = SettingsManager.ConfigSettings.YCompenSationBitAt;
                scanParam.AlignmentParam.XMotionExtraMoveLength = SettingsManager.ConfigSettings.XMotionExtraMoveLength;
                scanParam.AlignmentParam.YMotionExtraMoveLength = SettingsManager.ConfigSettings.YMotionExtraMoveLength;

                // Motor speed settings
                //if (SettingsManager.ConfigSettings.MotorSettings != null)
                //{
                //    foreach (var motorSetting in SettingsManager.ConfigSettings.MotorSettings)
                //    {
                //        switch (motorSetting.MotorType)
                //        {
                //            case MotorType.X:
                //                scanParam.XMotorSpeed = (int)Math.Round(motorSetting.Speed * _XMotorSubdivision);
                //                scanParam.XMotionAccVal = (int)Math.Round(motorSetting.Accel * _XMotorSubdivision);
                //                scanParam.XMotionDccVal = (int)Math.Round(motorSetting.Dccel * _XMotorSubdivision);
                //                break;
                //            case MotorType.Y:
                //                scanParam.YMotorSpeed = (int)Math.Round(motorSetting.Speed * _YMotorSubdivision);
                //                scanParam.YMotionAccVal = (int)Math.Round(motorSetting.Accel * _YMotorSubdivision);
                //                scanParam.YMotionDccVal = (int)Math.Round(motorSetting.Dccel * _YMotorSubdivision);
                //                break;
                //            case MotorType.Z:
                //                scanParam.ZMotorSpeed = (int)Math.Round(motorSetting.Speed * _ZMotorSubdivision);
                //                scanParam.ZMotionAccVal = (int)Math.Round(motorSetting.Accel * _ZMotorSubdivision);
                //                scanParam.ZMotionDccVal = (int)Math.Round(motorSetting.Dccel * _ZMotorSubdivision);
                //                break;
                //        }
                //    }
                //}
                //scanParam.IsChannelALightShadeFix = SettingsManager.ConfigSettings.IsChannelALightShadeFix;
                //scanParam.IsChannelBLightShadeFix = SettingsManager.ConfigSettings.IsChannelBLightShadeFix;
                //scanParam.IsChannelCLightShadeFix = SettingsManager.ConfigSettings.IsChannelCLightShadeFix;
                //scanParam.IsChannelDLightShadeFix = SettingsManager.ConfigSettings.IsChannelDLightShadeFix;

                if (_CurrentScanType == ScanType.Auto || scanParam.IsEdrScanning)   //EDR scanning uses SmartScan
                {
                    scanParam.IsSmartScanning = true;
                    scanParam.SmartScanResolution = SettingsManager.ConfigSettings.AutoScanSettings.Resolution;
                    if (scanParam.Res <= 50)
                    {
                        // Default High Resolution smart test scan resolution: 100
                        // High resolution: 10/20/25 micron scan
                        if (SettingsManager.ConfigSettings.AutoScanSettings.HighResolution == 0)
                        {
                            SettingsManager.ConfigSettings.AutoScanSettings.HighResolution = 100;
                        }
                        if (scanParam.Res <= 50)
                        {
                            scanParam.SmartScanResolution = SettingsManager.ConfigSettings.AutoScanSettings.HighResolution;
                        }
                    }
                    List<int> keyList = new List<int>(SettingsManager.ConfigSettings.LasersSignalList.Keys);
                    scanParam.SmartScanSignalLevels = SettingsManager.ConfigSettings.LasersSignalList[keyList[0]].Count;
                    scanParam.SmartScanFloor = SettingsManager.ConfigSettings.AutoScanSettings.Floor;
                    scanParam.SmartScanCeiling = SettingsManager.ConfigSettings.AutoScanSettings.Ceiling;
                    scanParam.SmartScanOptimalVal = SettingsManager.ConfigSettings.AutoScanSettings.OptimalVal;
                    scanParam.SmartScanOptimalDelta = SettingsManager.ConfigSettings.AutoScanSettings.OptimalDelta;
                    scanParam.SmartScanAlpha488 = SettingsManager.ConfigSettings.AutoScanSettings.Alpha488;
                    scanParam.SmartScanInitSignalLevel = SettingsManager.ConfigSettings.AutoScanSettings.StartingSignalLevel;
                    scanParam.SmartScanSignalStepdownLevel = SettingsManager.ConfigSettings.AutoScanSettings.HighSignalStepdownLevel;
                    if (SettingsManager.ConfigSettings.LasersSignalList.ContainsKey(Workspace.This.LaserL1))
                        scanParam.LaserL1SignalOptions = new List<Signal>(SettingsManager.ConfigSettings.LasersSignalList[Workspace.This.LaserL1]);
                    if (SettingsManager.ConfigSettings.LasersSignalList.ContainsKey(Workspace.This.LaserR1))
                        scanParam.LaserR1SignalOptions = new List<Signal>(SettingsManager.ConfigSettings.LasersSignalList[Workspace.This.LaserR1]);
                    if (SettingsManager.ConfigSettings.LasersSignalList.ContainsKey(Workspace.This.LaserR2))
                        scanParam.LaserR2SignalOptions = new List<Signal>(SettingsManager.ConfigSettings.LasersSignalList[Workspace.This.LaserR2]);
                }

                Workspace.This.LogMessage("==========================================");
                Workspace.This.LogMessage(string.Format("{0}", scanRegion.ScanRegionHeader));
                Workspace.This.LogMessage(string.Format("Pixel Size: {0}", scanParam.Res));
                Workspace.This.LogMessage(string.Format("Scan Speed: {0}", scanParam.Quality));
                Workspace.This.LogMessage(string.Format("Focus: {0}", scanParam.ScanZ0));
                Workspace.This.LogMessage(string.Format("EDR Scan: {0}", scanParam.DynamicBitsAt.ToString()));

                #region === Get prefined signal settings ===

                List<Signal> scanRegionSignals = new List<Signal>();
                IsLaserL1On = false;
                IsLaserR1On = false;
                IsLaserR2On = false;
                //IsLaserDOn = false;

                // Signal validation
                if (scanRegion.SignalList.Count > 0)
                {
                    //Signal scanSignal = null;
                    foreach (var signal in scanRegion.SignalList)
                    {
                        if (_CurrentScanType == ScanType.Auto)
                        {
                            // Auto-scan: don't check for intensity signal level selection
                            //
                            if (signal.SelectedLaser == null ||
                                signal.SelectedColorChannel == null)
                            {
                                string caption = "Signal options error...";
                                string message = string.Empty;
                                if (signal.SelectedLaser == null)
                                {
                                    message = string.Format("A laser not selected [{0}].\nPlease select a laser.", scanRegion.ScanRegionHeader);
                                    Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                                    return;
                                }
                                if (signal.SelectedColorChannel == null)
                                {
                                    message = string.Format("Color channel not selected [{0}].\nPlease select a color channel.", scanRegion.ScanRegionHeader);
                                    Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                                    return;
                                }
                            }
                        }
                        else
                        {
                            // Check to make sure all the signal options are selected.
                            //
                            if (signal.SelectedLaser == null ||
                                signal.SelectedSignalLevel == null ||
                                signal.SelectedColorChannel == null)
                            {
                                string caption = "Signal options error...";
                                string message = string.Empty;
                                if (signal.SelectedLaser == null)
                                {
                                    message = string.Format("A laser not selected [{0}].\nPlease select a laser.", scanRegion.ScanRegionHeader);
                                    Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                                    return;
                                }
                                if (signal.SelectedSignalLevel == null)
                                {
                                    message = string.Format("Intensity level not selected [{0}].\nPlease select an intensity.", scanRegion.ScanRegionHeader);
                                    Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                                    return;
                                }
                                if (signal.SelectedColorChannel == null)
                                {
                                    message = string.Format("Color channel not selected [{0}].\nPlease select a color channel.", scanRegion.ScanRegionHeader);
                                    Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                                    return;
                                }
                            }
                        }

                        // Laser modules
                        // L1 = laser channel C
                        // R1 = laser channel A
                        // R2 = laser channel B
                        Signal scanSignal = null;
                        if (signal.SelectedLaser.LaserChannel ==  LaserChannels.ChannelC)
                        {
                            scanSignal = (Signal)SettingsManager.ConfigSettings.LasersSignalList[Workspace.This.LaserL1][signal.SelectedSignalLevel.IntensityLevel - 1].Clone();  // config file 1 index
                            IsLaserL1On = true;
                        }
                        else if (signal.SelectedLaser.LaserChannel ==  LaserChannels.ChannelA)
                        {
                            scanSignal = (Signal)SettingsManager.ConfigSettings.LasersSignalList[Workspace.This.LaserR1][signal.SelectedSignalLevel.IntensityLevel - 1].Clone();  // config file 1 index
                            IsLaserR1On = true;
                        }
                        else if (signal.SelectedLaser.LaserChannel == LaserChannels.ChannelB)
                        {
                            scanSignal = (Signal)SettingsManager.ConfigSettings.LasersSignalList[Workspace.This.LaserR2][signal.SelectedSignalLevel.IntensityLevel - 1].Clone();  // config file 1 index
                            IsLaserR2On = true;
                        }

                        scanSignal.ColorChannel = (int)signal.SelectedColorChannel.ImageColorChannel;
                        //scanSignal.LaserWavelength = ImagingSystemHelper.GetLaserWaveLength(signal.SelectedDye.LaserType);
                        //scanSignal.LaserChannel = ImagingSystemHelper.GetLaserChannel(Int32.Parse(scanSignal.LaserWavelength));
                        // Get the laser channel corresponding to the wave length
                        //scanSignal.LaserChannel = ImagingSystemHelper.GetLaserChannel(scanSignal.LaserWavelength);
                        scanSignal.LaserChannel = signal.SelectedLaser.LaserChannel;
                        scanSignal.LaserWavelength = signal.SelectedLaser.Wavelength;
                        scanSignal.SensorType = signal.SelectedLaser.SensorType;
                        scanSignal.SignalLevel = signal.SelectedSignalLevel.IntensityLevel;
                        //if (scanSignal.LaserChannel == LaserChannels.ChannelA)      //R1
                        //    scanSignal.LaserWavelength = Workspace.This.LaserR1.ToString();
                        //else if (scanSignal.LaserChannel == LaserChannels.ChannelB) //R2
                        //    scanSignal.LaserWavelength = Workspace.This.LaserR2.ToString();
                        //else if (scanSignal.LaserChannel == LaserChannels.ChannelC) //L1
                        //    scanSignal.LaserWavelength = Workspace.This.LaserL1.ToString();

                        scanRegionSignals.Add(scanSignal);

                        Workspace.This.LogMessage(string.Format("Laser Channel: {0}", scanSignal.LaserChannel.ToString()));
                        Workspace.This.LogMessage(string.Format("Laser Wavelength: {0}", scanSignal.LaserWavelength));
                        Workspace.This.LogMessage(string.Format("Sensor Type: {0}", scanSignal.SensorType.ToString()));
                        Workspace.This.LogMessage(string.Format("Signal Level: {0}", scanSignal.SignalLevel));
                        Workspace.This.LogMessage("==========================================");
                    }   //foreach scanRegion.SignalList
                }

                //_ScanChannelCount = scanRegionSignals.Count;

                //check laser status
                if (IsLaserL1On == false && IsLaserR1On == false &&
                    IsLaserR2On == false)
                {
                    string caption = "No laser selected...";
                    string message = string.Format("No laser selected [{0}].\n Do you want to continue scanning?", scanRegion.ScanRegionHeader);
                    if (Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.YesNo, MessageBoxImage.Information) != MessageBoxResult.Yes)
                    {
                        return;
                    }
                }

                #endregion

                if (scanRegionSignals.Count > 1)
                {
                    scanParam.IsSequentialScanning = scanRegion.IsSequentialScanning;
                    if (scanRegion.IsSequentialScanning && !scanRegion.IsZScan)
                    {
                        _ScanImageBaseFilename.Clear();
                        _ScanImageBaseFilename.Add(Workspace.This.GenerateFileName(string.Empty, string.Empty));
                    }
                }
                else
                {
                    // Scanning channels must be greater 1, otherwise turn OFF sequential scanning.
                    scanRegion.IsSequentialScanning = false;
                    scanParam.IsSequentialScanning = false;
                }

                // Z-Scanning setup
                if ((_CurrentScanType == ScanType.Normal || _CurrentScanType == ScanType.Auto) && scanRegion.IsZScan)
                {
                    if (scanRegion.IsZScan)
                    {
                        if (scanRegion.ZScanSetting.DeltaFocus <= 0 || scanRegion.ZScanSetting.NumOfImages < 2)
                        {
                            string caption = "Z-Scanning...";
                            string message = string.Format("Z-Scan: The Focus Delta must be greater 0 [{0}].", scanRegion.ScanRegionHeader);
                            if (scanRegion.ZScanSetting.NumOfImages < 2)
                                message = string.Format("Z-Scan: The number of images must be 2 or greater [{0}].", scanRegion.ScanRegionHeader);
                            Xceed.Wpf.Toolkit.MessageBox.Show(Workspace.This.Owner, message, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }

                    scanParam.IsZScanning = scanRegion.IsZScan;
                    scanParam.BottomImageFocus = scanRegion.ZScanSetting.BottomImageFocus;
                    scanParam.DeltaFocus = scanRegion.ZScanSetting.DeltaFocus;
                    scanParam.NumOfImages = scanRegion.ZScanSetting.NumOfImages;

                    _ScanImageBaseFilename.Clear();
                    if (scanRegion.IsSequentialScanning) // Z-Scanning + Sequential
                    {
                        // Common name for each set
                        for (int i = 0; i < scanParam.NumOfImages; i++)
                        {
                            _ScanImageBaseFilename.Add(Workspace.This.GenerateFileName(string.Empty, string.Empty));
                        }
                    }
                    else
                    {
                        _ScanImageBaseFilename.Add(Workspace.This.GenerateFileName(string.Empty, string.Empty));
                    }
                }
                else
                {
                    // Number of images must be greater 1, otherwise turn OFF Z-scanning.
                    scanParam.IsZScanning = false;
                }

                scanParam.Signals = scanRegionSignals;
                scanParams.Add(scanParam);

            } // foreach scan regions

            // check file size (if > 2GB display warning, and turn off auto merge)
            for (int index = 0; index < _SelectedAppProtocol.ScanRegions.Count; index++)
            {
                bool bIsAutoMergeImages = true;

                if (_SelectedAppProtocol.ScanRegions[index].SignalCount > 1)
                {
                    //
                    // Don't merge the images if total image size after a channel merges is > 2GB
                    //
                    double totalFileSize = GetFileSizeInBytes(_SelectedAppProtocol.ScanRegions[index]);
                    if (totalFileSize > 2100000000)
                    {
                        bIsAutoMergeImages = false;

                        string caption = "File size to large...";
                        string message = string.Format("Image size in scan region #{0} is too large. The scanned images will appear and save as individual channnels.",
                            _SelectedAppProtocol.ScanRegions[index].ScanRegionNum);
                        var parent = Application.Current.MainWindow;
                        var dlgResult = Xceed.Wpf.Toolkit.MessageBox.Show(parent, message, caption, MessageBoxButton.OKCancel);
                        if (dlgResult == MessageBoxResult.Cancel) { return; }
                    }
                }
                _SelectedAppProtocol.ScanRegions[index].IsAutoMergeImages = bIsAutoMergeImages;
            }

            // Setting this flag here to avoid null reference on preview channels changed.
            _IsPrescanCompleted = false;

            //
            // Clear previous preview image buffers
            //
            _PreviewImage = null;
            _ChannelL1PrevImage = null;
            _ChannelR1PrevImage = null;
            _ChannelR2PrevImage = null;
            _ChannelL1PrevImageUnAligned = null;
            _ChannelR1PrevImageUnAligned = null;
            _ChannelR2PrevImageUnAligned = null;

            if (this._ImagingVm != null && this._ImagingVm.PreviewImage != null)
            {
                this.ImagingVm.PreviewImage = null;     //clear preview image
            }

            if (this._ImagingVm != null && this._ImagingVm.PreviewImages != null)
            {
                // Clear preview image(s)
                _ImagingVm.PreviewImages.Clear();
            }

            _SelectedAppProtocol.SelectedScanRegion = _SelectedAppProtocol.ScanRegions[0];

            #region === Auto save file location verification ===

            for (int index = 0; index < _SelectedAppProtocol.ScanRegions.Count; index++)
            {
                // Auto-save verification
                if (_SelectedAppProtocol.ScanRegions[index].FileLocationVm.IsAutoSave)
                {
                    if (string.IsNullOrEmpty(_SelectedAppProtocol.ScanRegions[index].FileLocationVm.DestinationFolder))
                    {
                        string strCaption = "File saving location...";
                        string strMessage = "Please specify the destination folder.";
                        Xceed.Wpf.Toolkit.MessageBox.Show(strMessage, strCaption, MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    else if (!System.IO.Directory.Exists(_SelectedAppProtocol.ScanRegions[index].FileLocationVm.DestinationFolder))
                    {
                        string strCaption = "File saving location...";
                        string strMessage = string.Format("The folder \"{0}\" does not exists\nPlease select another folder.",
                            _SelectedAppProtocol.ScanRegions[index].FileLocationVm.DestinationFolder);
                        Xceed.Wpf.Toolkit.MessageBox.Show(strMessage, strCaption, MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    else
                    {
                        //Check if file already exists
                        /*string fileName = _SelectedAppProtocol.ScanRegions[index].FileLocationVm.FileName.Trim();
                        string targetDirectory = _SelectedAppProtocol.ScanRegions[index].FileLocationVm.DestinationFolder;
                        if (!Workspace.This.CheckSupportedFileType(fileName))
                        {
                            fileName += ".tif";
                        }
                        string filePath = System.IO.Path.Combine(targetDirectory, fileName);
                        if (System.IO.File.Exists(filePath))
                        {
                            string strCaption = "File already exists...";
                            string strMessage = string.Format("The specified file \"{0}\" already exists in the selected folder [Scan Region #{1}]",
                                _SelectedAppProtocol.ScanRegions[index].FileLocationVm.FileName, _SelectedAppProtocol.ScanRegions[index].ScanRegionNum);
                            Xceed.Wpf.Toolkit.MessageBox.Show(strMessage, strCaption, MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }*/

                        string fileName = _SelectedAppProtocol.ScanRegions[index].FileLocationVm.FileName.Trim();
                        if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) > 0)
                        {
                            string strCaption = "Auto-save: Invalid file name...";
                            string strMessage = "Please make sure you've entered a valid file name.";
                            Xceed.Wpf.Toolkit.MessageBox.Show(strMessage, strCaption, MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        string targetDirectory = _SelectedAppProtocol.ScanRegions[index].FileLocationVm.DestinationFolder;
                        System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo(targetDirectory);
                        System.IO.FileInfo[] fileEntries = dirInfo.GetFiles();
                        foreach (System.IO.FileInfo fileInfo in fileEntries)
                        {
                            string fileNameWithoutExt = System.IO.Path.GetFileNameWithoutExtension(fileInfo.Name);
                            if (fileNameWithoutExt.StartsWith(fileName, StringComparison.OrdinalIgnoreCase))
                            {
                                string strCaption = "File already exists...";
                                string strMessage = string.Format("A file name starting with \"{0}\" already exists in the selected folder [Scan Region #{1}]",
                                    _SelectedAppProtocol.ScanRegions[index].FileLocationVm.FileName, _SelectedAppProtocol.ScanRegions[index].ScanRegionNum);
                                Xceed.Wpf.Toolkit.MessageBox.Show(strMessage, strCaption, MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                            }
                        }
                    }
                }
            }

            #endregion

            if (!SettingsManager.ConfigSettings.IsSimulationMode)
            {
                if (!Workspace.This.EthernetController.IsConnected)
                {
                    string caption = "Sapphire FL Biomolecular Imager";
                    string message = "Scanner is not connected.\nPlease make sure the system power is turned on, \nand the USB cable is securely connected.";
                    Xceed.Wpf.Toolkit.MessageBox.Show(Workspace.This.Owner, message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                    return;
                }
                //if (!Workspace.This.MotorVM.IsMotorAlreadyHome)
                //{
                //    Xceed.Wpf.Toolkit.MessageBox.Show("Please wait. Motor is being initialized.");
                //    return;
                //}

                //Turn off all the lasers before scanning just in case they were left on from previous scan.
                Workspace.This.TurnOffAllLasers();

                //EL: TODO: check if the stage is homed?
                // Is the scan head in the locked position?
                //if (Workspace.This.MotorVM.GalilMotor.IsScanheadLocked)
                //{
                //    string caption = "Sapphire Biomolecular Imager";
                //    string message = "The scan head is in the locked position.\n" +
                //                     "Please power cycle the Sapphire scanner then relaunch the application to unlock the scan head.";
                //    Xceed.Wpf.Toolkit.MessageBox.Show(Workspace.This.Owner, message, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                //    return;
                //}

                // Is the scanner lid opened?
                if (Workspace.This.EthernetController.LidIsOpen)
                {
                    string caption = "Sapphire FL Biomolecular Imager";
                    string message = "The scanner lid is opened.\n" +
                                        "Please close the lid, then try scanning again.";
                    Xceed.Wpf.Toolkit.MessageBox.Show(Workspace.This.Owner, message, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            ImageProcessing.FlipAxis flipAxis = ImageProcessing.FlipAxis.None;
            if (SettingsManager.ApplicationSettings.IsHorizontalFlipEnabled)
            {
                flipAxis = ImageProcessing.FlipAxis.Horizontal;
            }
            else if (SettingsManager.ApplicationSettings.IsVerticalFlipEnabled)
            {
                flipAxis = ImageProcessing.FlipAxis.Vertical;
            }

            bool bIsSaveDebuggingImages = SettingsManager.ConfigSettings.IsSaveDebuggingImages;
            bool bIsSaveEdrAs24bit = SettingsManager.ConfigSettings.IsSaveEdrAs24bit;
            _ImageScanCommand = new LaserScanCommand(Application.Current.Dispatcher,
                                                     Workspace.This.EthernetController,
                                                     Workspace.This.MotorVM.MotionController,
                                                     scanParams,
                                                     false,
                                                     SettingsManager.ConfigSettings.IsApplyImageSmoothing,
                                                     Workspace.This.AppDataPath,
                                                     flipAxis,
                                                     bIsSaveDebuggingImages: bIsSaveDebuggingImages,
                                                     bIsSaveEdrAs24bit: bIsSaveEdrAs24bit);
            _ImageScanCommand.Completed += ImageScanCommand_Completed;
            _ImageScanCommand.CommandStatus += ImageScanCommand_CommandStatus;
            //_ImageScanCommand.ReceiveTransfer += ImageScanCommand_ReceiveTransfer;
            _ImageScanCommand.DataReceived += ImageScanCommand_DataReceived;
            _ImageScanCommand.ScanRegionCompleted += ImageScanCommand_ScanRegionCompleted;
            _ImageScanCommand.IsSimulationMode = SettingsManager.ConfigSettings.IsSimulationMode;
            _ImageScanCommand.IsKeepRawImages = SettingsManager.ConfigSettings.IsKeepRawImages;
            _ImageScanCommand.OnScanDataReceived += ImageScanCommand_OnScanDataReceived;
            _ImageScanCommand.Logger = Workspace.This.Logger;
            _ImageScanCommand.IsSimulationMode = SettingsManager.ConfigSettings.IsSimulationMode;
            _ImageScanCommand.ApplicationDataPath = SettingsManager.ApplicationDataPath;
            _ImageScanCommand.IsDespeckleSmartScan = SettingsManager.ApplicationSettings.IsDespeckleSmartScan;
            if (SettingsManager.ConfigSettings.IsSimulationMode)
            {
                _ImageScanCommand.CompletionEstimate += ImageScanCommand_CompletionEstimate;
            }
            // SmartScan (formally 'AutoScan')
            if (_CurrentScanType == ScanType.Auto || scanParams[0].IsEdrScanning)
            {
                _IsAutoScanCompleted = false;
                _ImageScanCommand.SmartScanStarting += ImageScanCommand_SmartScanStarting;
                _ImageScanCommand.SmartScanUpdated += ImageScanCommand_SmartScanUpdated;
                _ImageScanCommand.SmartScanCompleted += ImageScanCommand_SmartScanCompleted;
            }

            // Z-Scan/Sequential/EDR scan settings
            foreach (var scanParam in scanParams)
            {
                if (scanParam.IsZScanning)
                {
                    _ImageScanCommand.ZScanningCompleted -= ImageScanCommand_ZScanningCompleted;
                    _ImageScanCommand.ZScanningCompleted += ImageScanCommand_ZScanningCompleted;
                }
                if (scanParam.IsSequentialScanning)
                {
                    _ImageScanCommand.SequentialChannelStarting -= ImageScanCommand_SequentialChannelStarting;
                    _ImageScanCommand.SequentialChannelStarting += ImageScanCommand_SequentialChannelStarting;
                    //_ImageScanCommand.SequentialChannelCompleted -= ImageScanCommand_SequentialChannelCompleted;
                    //_ImageScanCommand.SequentialChannelCompleted += ImageScanCommand_SequentialChannelCompleted;
                }
                if (scanParam.IsEdrScanning)
                {
                    _IsAutoScanCompleted = false;
                    _ImageScanCommand.SmartScanStarting -= ImageScanCommand_SmartScanStarting;
                    _ImageScanCommand.SmartScanStarting += ImageScanCommand_SmartScanStarting;
                    _ImageScanCommand.SmartScanUpdated -= ImageScanCommand_SmartScanUpdated;
                    _ImageScanCommand.SmartScanUpdated += ImageScanCommand_SmartScanUpdated;
                    _ImageScanCommand.SmartScanCompleted -= ImageScanCommand_SmartScanCompleted;
                    _ImageScanCommand.SmartScanCompleted += ImageScanCommand_SmartScanCompleted;
                    //_ImageScanCommand.EDRTestScanStarting -= ImageScanCommand_EDRTestScanStarting;
                    //_ImageScanCommand.EDRTestScanStarting += ImageScanCommand_EDRTestScanStarting;
                    _ImageScanCommand.EDRTestScanUpdating -= ImageScanCommand_EDRTestScanUpdating;
                    _ImageScanCommand.EDRTestScanUpdating += ImageScanCommand_EDRTestScanUpdating;
                    //_ImageScanCommand.EDRTestScanCompleted -= ImageScanCommand_EDRTestScanCompleted;
                    //_ImageScanCommand.EDRTestScanCompleted += ImageScanCommand_EDRTestScanCompleted;
                }
            }

            Workspace.This.IsScanning = true;

            if (!SettingsManager.ConfigSettings.IsSimulationMode)
                Workspace.This.IsPreparingToScan = true;

            _IsSaveScanDataOnAborted = false;
            _IsAbortedOnLidOpened = false;
            if (_SelectedAppProtocol.ScanRegions.Count > 1)
            {
                // Disable scan region adorner while scanning.
                _ImagingVm.IsAdornerEnabled = false;
            }

            if (!_IsSmartScanning)
            {
                _ImagingVm.ContrastVm.ResetContrastValues();  // Reset preview window contrast values.
            }

            if (_CurrentScanType != ScanType.Auto && !scanParams[0].IsEdrScanning)
            {
                PreviewImageSetup(scanParams[0], 0);
            }

            // Start the image scanning thread
            _ImageScanCommand.Start();

            /*if (SettingsManager.ConfigSettings.IsSimulationMode)
            {
                _ImageScanCommand.Start();
            }
            else
            {
                System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    Workspace.This.StatusTextProgress = "Scanning setup in progress...please wait....";

                    //EL: TODO: move the motor to position
                    //
                    //first,make the motor goes to start position
                    //Workspace.This.MotorVM.GalilMotor.SendCommand("HX");
                    //Workspace.This.MotorVM.GalilMotor.SendCommand("ST");
                    //#region motor program one
                    //string temp = "#SCAN\r" +
                    //              "SHXYZ\r" +
                    //              "STXYZ\r" +
                    //              "SPX=50000\r" +
                    //              "SPY=5000\r" +
                    //              "SPZ=2000\r" +
                    //              "PAB=" + (scanParams[0].ScanY0).ToString() + "\r" +
                    //              "BGY\r" +
                    //              "PAA=" + (scanParams[0].ScanX0).ToString() + "\r" +
                    //              "BGX\r" +
                    //              "PAC=" + (scanParams[0].ScanZ0).ToString() + "\r" +
                    //              "BGZ\r" +
                    //              "AMX\r" +
                    //              "AMY\r" +
                    //              "AMZ\r" +
                    //              "EN";
                    //#endregion motor program one
                    //Workspace.This.MotorVM.GalilMotor.ProgramDownload(temp);
                    ////waitting for the motor goes to the position
                    //while (!(Workspace.This.MotorVM.GalilMotor.XCurrentP == scanParams[0].ScanX0 &&
                    //        Workspace.This.MotorVM.GalilMotor.YCurrentP == scanParams[0].ScanY0 &&
                    //        Workspace.This.MotorVM.GalilMotor.ZCurrentP == scanParams[0].ScanZ0))
                    //{
                    //    Thread.Sleep(1);
                    //}

                    //while (!Workspace.This.ApdVM.IsPMTGainRecovered)
                    //{
                    //    Thread.Sleep(1);
                    //}

                    if (_CurrentScanType == ScanType.Auto)
                    {
                        Workspace.This.StatusTextProgress = "SMARTSCAN in progress....";
                    }

                    // Start the image scanning thread
                    _ImageScanCommand.Start();
                });
            }*/

            //Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
            //{
            //    Workspace.This.CaptureDispatcherTimer.Start();
            //});
        }
        public bool CanExecuteStartScanCommand(object parameter)
        {
            //return true;
            return (_SelectedAppProtocol != null);
        }

        #endregion

        private void ImageScanCommand_CommandStatus(object sender, string statusText)
        {
            Workspace.This.Owner.Dispatcher.BeginInvoke((Action)delegate
            {
                Workspace.This.StatusTextProgress = statusText;
            });
        }

        private void ImageScanCommand_CompletionEstimate(ThreadBase sender, DateTime dateTime, double estTime, double percentCompleted)
        {
            Workspace.This.Owner.Dispatcher.BeginInvoke((Action)delegate
            {
                this.Time = (int)estTime;
            });
        }

        private void ImageScanCommand_Completed(CommandLib.ThreadBase sender, CommandLib.ThreadBase.ThreadExitStat exitState)
        {
            Workspace.This.Owner.Dispatcher.BeginInvoke((Action)delegate
            {
                //Workspace.This.CaptureDispatcherTimer.Stop();
                Workspace.This.IsPreparingToScan = false;
                Workspace.This.IsReadyScanning = false;
                Workspace.This.IsScanning = false;
                _CurrentScanRegion = int.MaxValue;
                _ImagingVm.IsAdornerEnabled = true;
                // Enable saturation display (in case it's turned off by EDR scanning)
                _ImagingVm.ContrastVm.DisplayImageInfo.IsSaturationChecked = true;
                _IsUpdatingPreviewImage = false;
                _IsAligningPreviewImage = false;

                //Reset status
                Time = 0;
                RemainingTime = 0;
                Workspace.This.StatusTextProgress = string.Empty;
                Workspace.This.PercentCompleted = 0;
                //_IsSmartScanning = false;

                //
                // Clear the unaligned/uncropped image buffer.
                //
                _ChannelL1PrevImageUnAligned = null;
                _ChannelR1PrevImageUnAligned = null;
                _ChannelR2PrevImageUnAligned = null;
                //
                // Clear preview image buffers
                //
                //_ImagingVm.PreviewImage = null;
                //_PreviewImage = null;
                //_ChannelL1PrevImage = null;
                //_ChannelR1PrevImage = null;
                //_ChannelR2PrevImage = null;

                if (_IsAbortedOnLidOpened)
                {
                    IsPreviewChannels = false;  // close preview image contrast window.
                    string caption = "Lid open detected...";
                    string message = "Lid open detected. The scan was terminated.\n" + 
                                     "Would you like to save your scanned data?";
                    var result = Xceed.Wpf.Toolkit.MessageBox.Show(Workspace.This.Owner, message, caption, MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Yes)
                    {
                        _IsSaveScanDataOnAborted = true;
                        _ImageScanCommand.IsSaveDataOnScanAbort = _IsSaveScanDataOnAborted;
                    }
                    else
                    {
                        _IsSaveScanDataOnAborted = false;
                        _ImageScanCommand.IsSaveDataOnScanAbort = _IsSaveScanDataOnAborted;
                    }
                }

                ThreadBase scannedThread = (sender as LaserScanCommand);
                bool bIsZScanning = false;
                bool bIsSequentialScanning = false;
                if (((LaserScanCommand)scannedThread).CurrentScanParam != null)
                {
                    bIsZScanning = ((LaserScanCommand)scannedThread).CurrentScanParam.IsZScanning;
                    bIsSequentialScanning = ((LaserScanCommand)scannedThread).CurrentScanParam.IsSequentialScanning;
                }

                if (exitState == ThreadBase.ThreadExitStat.None)
                {
                    #region === Scan Sucessfully Completed ===

                    if (_CurrentScanType == ScanType.Preview)
                    {
                        #region === Preview Scan ===

                        // Preview scan : get unfrozen copy
                        // This is just in case the preview channel(s) is turned off during the Prescan
                        // before the Prescan completed.
                        //
                        //EL: TODO:
                        //if (((ScanProcessing)scannedThread).ChannelAImage != null)
                        //{
                        //    _ChannelAPrevImage = ((ScanProcessing)scannedThread).ChannelAImage.Clone();
                        //}
                        //if (((ScanProcessing)scannedThread).ChannelBImage != null)
                        //{
                        //    _ChannelBPrevImage = ((ScanProcessing)scannedThread).ChannelBImage.Clone();
                        //}
                        //if (((ScanProcessing)scannedThread).ChannelCImage != null)
                        //{
                        //    _ChannelCPrevImage = ((ScanProcessing)scannedThread).ChannelCImage.Clone();
                        //}
                        //if (((ScanProcessing)scannedThread).ChannelDImage != null)
                        //{
                        //    _ChannelDPrevImage = ((ScanProcessing)scannedThread).ChannelDImage.Clone();
                        //}

                        _IsPrescanCompleted = true;

                        #endregion
                    }
                    else
                    {
                        // Clear preview image(s)
                        //if (_ImagingVm.PreviewImages != null && _ImagingVm.PreviewImages.Count > 0)
                        //{
                        //    _ImagingVm.PreviewImages.Clear();
                        //}

                        // Switch to gallery tab
                        Workspace.This.SelectedApplicationTab = ApplicationTabType.Gallery;

                        // Generate new default file name
                        if (_SelectedAppProtocol != null && _SelectedAppProtocol.ScanRegions.Count > 0)
                        {
                            int nScanRegions = _SelectedAppProtocol.ScanRegions.Count;
                            string generatedFileName = Workspace.This.GenerateFileName(string.Empty, string.Empty);

                            foreach (var scanRegion in _SelectedAppProtocol.ScanRegions)
                            {
                                string fileName = scanRegion.FileLocationVm.FileName;
                                if (!string.IsNullOrEmpty(fileName))
                                {
                                    if (Workspace.This.IsGeneratedFileName(fileName))
                                    {
                                        if (nScanRegions > 1)
                                        {
                                            scanRegion.FileLocationVm.FileName = string.Format("{0}_SR{1}", generatedFileName, scanRegion.ScanRegionNum);
                                        }
                                        else
                                        {
                                            scanRegion.FileLocationVm.FileName = generatedFileName;
                                        }
                                    }
                                    else
                                    {
                                        string fileExt = ".tif";
                                        if (!Workspace.This.CheckSupportedFileType(fileName))
                                        {
                                            fileName += fileExt;
                                        }
                                        // User's specified file name - add suffix to avoid duplicate file name.
                                        string directoryName = scanRegion.FileLocationVm.DestinationFolder;
                                        string filePath = System.IO.Path.Combine(directoryName, fileName);
                                        if (System.IO.File.Exists(filePath))
                                        {
                                            // Get a unique file name in the destination folder
                                            scanRegion.FileLocationVm.FileName = Workspace.This.GetUniqueFilenameInFolder(directoryName, fileName);
                                        }
                                    }
                                }
                                else
                                {
                                    if (nScanRegions > 1)
                                    {
                                        scanRegion.FileLocationVm.FileName = string.Format("{0}_SR{1}", generatedFileName, scanRegion.ScanRegionNum);
                                    }
                                    else
                                    {
                                        scanRegion.FileLocationVm.FileName = generatedFileName;
                                    }
                                }
                            }
                        }
                    }

                    #endregion
                }
                else if ((exitState == ThreadBase.ThreadExitStat.Abort && _IsSaveScanDataOnAborted) || (_IsAbortedOnLidOpened && _IsSaveScanDataOnAborted))
                {
                    #region === Scan Aborted (Save Data) ===

                    if (_CurrentScanType != ScanType.Preview && 
                        (!bIsZScanning || (bIsZScanning && exitState == ThreadBase.ThreadExitStat.Abort && _IsSaveScanDataOnAborted)))
                    {
                        // Save data on abort
                        //    previously save the scan data here
                        //    now save in 'ImageScanCommand_ScanRegionCompleted' or 'ImageScanCommand_ZScanningCompleted' for Z-Scan)
                        //
                        if (scannedThread != null)
                        {
                            if (((LaserScanCommand)scannedThread).IsScanAborted && ((LaserScanCommand)scannedThread).IsSaveDataOnScanAbort)
                            {
                                ((LaserScanCommand)scannedThread).KeepDataOnAbort();
                            }
                        }

                        // Clear preview image(s)
                        //if (_ImagingVm.PreviewImages != null && _ImagingVm.PreviewImages.Count > 0)
                        //{
                        //    _ImagingVm.PreviewImages.Clear();
                        //}

                        // Switch to gallery tab
                        Workspace.This.SelectedApplicationTab = ApplicationTabType.Gallery;
                    }   // Normal scan
                    else if (_CurrentScanType == ScanType.Preview)
                    {
                        #region === Preview Scan ===

                        // Preview scan : get unfrozen copy
                        // This is just in case the preview channel(s) is turned off during the Prescan
                        // before the Prescan completed.
                        //
                        //EL: TODO:
                        //if (((ScanProcessing)scannedThread).ChannelAImage != null)
                        //{
                        //    _ChannelAPrevImage = ((ScanProcessing)scannedThread).ChannelAImage.Clone();
                        //}
                        //if (((ScanProcessing)scannedThread).ChannelBImage != null)
                        //{
                        //    _ChannelBPrevImage = ((ScanProcessing)scannedThread).ChannelBImage.Clone();
                        //}
                        //if (((ScanProcessing)scannedThread).ChannelCImage != null)
                        //{
                        //    _ChannelCPrevImage = ((ScanProcessing)scannedThread).ChannelCImage.Clone();
                        //}
                        //if (((ScanProcessing)scannedThread).ChannelDImage != null)
                        //{
                        //    _ChannelDPrevImage = ((ScanProcessing)scannedThread).ChannelDImage.Clone();
                        //}

                        _IsPrescanCompleted = true;

                        #endregion
                    }

                    #endregion
                }
                else if (exitState == ThreadBase.ThreadExitStat.Abort && ((LaserScanCommand)scannedThread).IsEdrSaturationAbort)
                {
                    string L1LaserWavelength = string.Empty;
                    string R1LaserWavelength = string.Empty;
                    string R2LaserWavelength = string.Empty;
                    string lineBreak1 = string.Empty;
                    string lineBreak2 = string.Empty;
                    string lineBreak3 = string.Empty;
                    var saturatedChannels = ((LaserScanCommand)scannedThread).EdrSaturatedChannels;
                    foreach (var saturatedChannel in saturatedChannels)
                    {
                        if (saturatedChannel == LaserChannels.ChannelC)         //L1 channel
                        {
                            L1LaserWavelength = "Laser: " + Workspace.This.LaserModuleL1.LaserWavelength.ToString();
                        }
                        else if (saturatedChannel == LaserChannels.ChannelA)    //R1 channel
                        {
                            R1LaserWavelength = "Laser: " + Workspace.This.LaserModuleR1.LaserWavelength.ToString();
                            if (!string.IsNullOrEmpty(L1LaserWavelength))
                                lineBreak1 = "\n";
                        }
                        else if (saturatedChannel == LaserChannels.ChannelB)    //R2 channel
                        {
                            R2LaserWavelength = "Laser: " + Workspace.This.LaserModuleR2.LaserWavelength.ToString();
                            if (!string.IsNullOrEmpty(L1LaserWavelength) || !string.IsNullOrEmpty(R1LaserWavelength))
                                lineBreak2 = "\n";
                        }
                    }
                    string caption = "Signal too strong...";
                    string message = string.Format("The scan was terminated. The following laser's signal was too strong for EDR scan :\n    {0}{1}    {2}{3}    {4}",
                        L1LaserWavelength, lineBreak1, R1LaserWavelength, lineBreak2, R2LaserWavelength);
                    var parent = Application.Current.MainWindow;
                    Xceed.Wpf.Toolkit.MessageBox.Show(parent, message, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else if (exitState == ThreadBase.ThreadExitStat.Error)
                {
                    #region === Scan Error ===

                    // Oh oh something went wrong - handle the error

                    //
                    // Clear preview image buffers
                    //
                    _ImagingVm.PreviewImage = null;
                    _PreviewImage = null;
                    _ChannelL1PrevImage = null;
                    _ChannelR1PrevImage = null;
                    _ChannelR2PrevImage = null;

                    // Clear preview image(s)
                    //if (_ImagingVm.PreviewImages != null && _ImagingVm.PreviewImages.Count > 0)
                    //{
                    //    _ImagingVm.PreviewImages.Clear();
                    //}

                    if (_PreviewContrastWindow != null && _PreviewContrastWindow.IsLoaded)
                    {
                        // Close preview channels/contrast window
                        _PreviewContrastWindow.Close();
                        _PreviewContrastWindow = null;
                    }

                    // Error occurred but not 'lid opened' error (lid opened error handle above, to allow the option to save the scanned data)
                    if (!_IsAbortedOnLidOpened)
                    {
                        string caption = "Scanning error...";
                        string message = scannedThread.Error.Message + "\n\nStack Trace:\n" + scannedThread.Error.StackTrace;
                        Workspace.This.LogMessage(message);
                        var parent = Application.Current.MainWindow;
                        Xceed.Wpf.Toolkit.MessageBox.Show(parent, message, caption, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }

                    #endregion
                }

                _ScanImageBaseFilename.Clear();
                _IsSaveScanDataOnAborted = false;
                _IsAbortedOnLidOpened = false;
                if (_CurrentScanType != ScanType.Preview)
                    _IsPrescanCompleted = false;

                //Turn off all the lasers after scanning is completed
                Workspace.This.TurnOffAllLasers();

                _ImageScanCommand.Completed -= ImageScanCommand_Completed;
                _ImageScanCommand.CommandStatus -= ImageScanCommand_CommandStatus;
                //_ImageScanCommand.ReceiveTransfer -= ImageScanCommand_ReceiveTransfer;
                _ImageScanCommand.DataReceived -= ImageScanCommand_DataReceived;
                _ImageScanCommand.SmartScanStarting -= ImageScanCommand_SmartScanStarting;
                _ImageScanCommand.SmartScanUpdated -= ImageScanCommand_SmartScanUpdated;
                _ImageScanCommand.SmartScanCompleted -= ImageScanCommand_SmartScanCompleted;
                _ImageScanCommand.ScanRegionCompleted -= ImageScanCommand_ScanRegionCompleted;
                //_ImageScanCommand.EDRTestScanStarting -= ImageScanCommand_EDRTestScanStarting;
                _ImageScanCommand.EDRTestScanUpdating -= ImageScanCommand_EDRTestScanUpdating;
                //_ImageScanCommand.EDRTestScanCompleted -= ImageScanCommand_EDRTestScanCompleted;
                if (SettingsManager.ConfigSettings.IsSimulationMode)
                {
                    _ImageScanCommand.CompletionEstimate -= ImageScanCommand_CompletionEstimate;
                }

                if (bIsZScanning)
                {
                    _ImageScanCommand.ZScanningCompleted -= ImageScanCommand_ZScanningCompleted;
                    if (exitState == ThreadBase.ThreadExitStat.None ||
                        (exitState == ThreadBase.ThreadExitStat.Abort && _IsSaveScanDataOnAborted))
                    {
                        Workspace.This.SelectedApplicationTab = ApplicationTabType.Gallery;
                    }
                }
                if (bIsSequentialScanning)
                {
                    _ImageScanCommand.SequentialChannelStarting -= ImageScanCommand_SequentialChannelStarting;
                    //_ImageScanCommand.SequentialChannelCompleted -= ImageScanCommand_SequentialChannelCompleted;
                    if (exitState == ThreadBase.ThreadExitStat.None ||
                        (exitState == ThreadBase.ThreadExitStat.Abort && _IsSaveScanDataOnAborted))
                    {
                        Workspace.This.SelectedApplicationTab = ApplicationTabType.Gallery;
                    }
                }

                // Reset Smartscan type
                _IsAutoScan = false;
                _IsSmartScanning = false;
                _ImageScanCommand = null;

                // Did scan region changed while scanning? Get the modified scan region
                if (_ImagingVm.CropServices != null && _IsScanRegionChanged)
                {
                    if (_ImagingVm.CellSize > 0)
                    {
                        foreach (var scanRegion in SelectedAppProtocol.ScanRegions)
                        {
                            if (scanRegion != null)
                            {
                                foreach (var cropService in _ImagingVm.CropServices)
                                {
                                    if (cropService.AdornerID == scanRegion.ScanRegionNum)
                                    {
                                        scanRegion.IsScanRectDragged = true;  // dragged vs manually entered value
                                        var cropArea = cropService.GetCroppedArea();
                                        double x = Math.Round(cropArea.CroppedRectAbsolute.X / _ImagingVm.CellSize, 2);
                                        double y = Math.Round(cropArea.CroppedRectAbsolute.Y / _ImagingVm.CellSize, 2);
                                        double w = Math.Round(cropArea.CroppedRectAbsolute.Width / _ImagingVm.CellSize, 2);
                                        double h = Math.Round(cropArea.CroppedRectAbsolute.Height / _ImagingVm.CellSize, 2);
                                        if (x < 0) { x = 0; }
                                        if (y < 0) { y = 0; }
                                        if (w > 25) { w = 25; }
                                        if (h > 25) { h = 25; }
                                        if (x + w > 25) { x -= (x + w) - 25; }
                                        if (y + h > 25) { y -= (y + h) - 25; }
                                        scanRegion.X = x;
                                        scanRegion.Y = y;
                                        scanRegion.Width = w;
                                        scanRegion.Height = h;
                                        scanRegion.IsScanRectDragged = false;
                                        break;
                                    }
                                }
                            }
                        }
                        _IsScanRegionChanged = false;
                    }
                }

                // Clear the unaligned/uncropped image buffer.
                //_ChannelAPrevImageUnAligned = null;
                //_ChannelBPrevImageUnAligned = null;
                //_ChannelCPrevImageUnAligned = null;

                // force a garbage collection to free 
                // up memory as quickly as possible.
                //GC.Collect();
            });
        }

        private void ImageScanCommand_SmartScanStarting(ThreadBase sender)
        {
            Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
            {
                var imageScanCommand = sender as LaserScanCommand;
                var currentScanParam = imageScanCommand.CurrentScanParam;
                int nScanRegion = imageScanCommand.CurrentScanRegion;

                Workspace.This.LogMessage(string.Format("Smartscan starting [Scan Region #{0}]", nScanRegion + 1));
                Workspace.This.LogMessage(string.Format("Smartscan starting signal level: {0}", SettingsManager.ConfigSettings.AutoScanSettings.StartingSignalLevel));

                _IsAutoScanCompleted = false;
                _CurrentScanType = ScanType.Auto;

                //if (nScanRegion != _CurrentScanRegion)
                //{
                //    PreviewImageSetup(currentScanParam, nScanRegion);
                //}
                PreviewImageSetup(currentScanParam, nScanRegion);
                _CurrentScanRegion = nScanRegion;

                // Update signal levels
                int signalLevel = SettingsManager.ConfigSettings.AutoScanSettings.StartingSignalLevel - 1;
                // Backup scan region settings
                _SelectedAppProtocol.IsModified = true;
                _SelectedAppProtocol.SelectedScanRegion = _SelectedAppProtocol.ScanRegions[nScanRegion];
                for (int i = 0; i < _SelectedAppProtocol.SelectedScanRegion.SignalCount; i++)
                {
                    _SelectedAppProtocol.SelectedScanRegion.SignalList[i].IsInitializing = true;    //Don't trigger selection changed event
                    _SelectedAppProtocol.SelectedScanRegion.SignalList[i].SelectedSignalLevel = _SelectedAppProtocol.SelectedScanRegion.SignalList[i].SignalLevelOptions[signalLevel];
                    _SelectedAppProtocol.SelectedScanRegion.SignalList[i].IsInitializing = false;
                }
            });
        }

        /// <summary>
        /// Update the selected scanning channels' signal level and clear the preview display image.
        /// </summary>
        /// <param name="sender"></param>
        private void ImageScanCommand_SmartScanUpdated(ThreadBase sender)
        {
            Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
            {
                var scanThread = sender as LaserScanCommand;
                var updatedSignals = scanThread.UpdatedSignalLevel;
                List<string> signalLevels = new List<string>();
                var nScanRegion = scanThread.CurrentScanRegion;

                // Update signal levels
                _SelectedAppProtocol.IsModified = true;
                for (int i = 0; i < _SelectedAppProtocol.ScanRegions[nScanRegion].SignalCount; i++)
                {
                    _SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].IsInitializing = true;    //Don't trigger selection changed event
                    _SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].SelectedSignalLevel = _SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].SignalLevelOptions[updatedSignals[i].Position - 1];
                    _SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].IsInitializing = false;
                    signalLevels.Add(_SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].SignalLevelOptions[updatedSignals[i].Position - 1].DisplayName);
                    // Clear the display image buffers
                    if (_SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].SelectedLaser.LaserChannel == LaserChannels.ChannelC)   //L1
                    {
                        ImageProcessingHelper.FastClear(ref _ChannelL1PrevImageUnAligned);
                        ImageProcessingHelper.FastClear(ref _ChannelL1PrevImage);
                    }
                    if (_SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].SelectedLaser.LaserChannel == LaserChannels.ChannelA)   //R1
                    {
                        ImageProcessingHelper.FastClear(ref _ChannelR1PrevImageUnAligned);
                        ImageProcessingHelper.FastClear(ref _ChannelR1PrevImage);
                    }
                    if (_SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].SelectedLaser.LaserChannel == LaserChannels.ChannelB)   //R2
                    {
                        ImageProcessingHelper.FastClear(ref _ChannelR2PrevImageUnAligned);
                        ImageProcessingHelper.FastClear(ref _ChannelR2PrevImage);
                    }
                }

                // Clear preview display image
                _ImagingVm.PreviewImage = null;

                string scanLevels = string.Join(",", signalLevels);
                Workspace.This.LogMessage(string.Format("Signal levels: {0}", scanLevels));
            });
        }

        private void ImageScanCommand_SmartScanCompleted(ThreadBase sender)
        {
            Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
            {
                _IsAutoScanCompleted = true;
                _CurrentScanType = ScanType.Normal;
                var scanThread = sender as LaserScanCommand;
                var updatedSignals = scanThread.UpdatedSignalLevel;
                var scanParam = scanThread.CurrentScanParam;
                int nScanRegion = scanThread.CurrentScanRegion;

                Workspace.This.LogMessage(string.Format("Smartscan completed [Scan Region #{0}]", nScanRegion + 1));

                // Update signal levels
                _SelectedAppProtocol.IsModified = true;
                for (int i = 0; i < _SelectedAppProtocol.ScanRegions[nScanRegion].SignalCount; i++)
                {
                    _SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].IsInitializing = true;    //Don't trigger selection changed event
                    _SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].SelectedSignalLevel = _SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].SignalLevelOptions[updatedSignals[i].Position - 1];
                    _SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].IsInitializing = false;
                }

                Workspace.This.LogMessage(string.Format("Setup preview images for the actual scan [Scan Region #{0}]", nScanRegion + 1));

                // Setup preview images with the selected pixel size
                PreviewImageSetup(scanParam, nScanRegion);

                Workspace.This.LogMessage("PreviewImageSetup completed.");
            });
        }

        private void ImageScanCommand_SequentialChannelStarting(ThreadBase sender, LaserChannels channel)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)delegate
            {
                _ImagingVm.IsLaserR1PrvSelected = (channel == LaserChannels.ChannelA) ? true : false;
                _ImagingVm.IsLaserR2PrvSelected = (channel == LaserChannels.ChannelB) ? true : false;
                _ImagingVm.IsLaserL1PrvSelected = (channel == LaserChannels.ChannelC) ? true : false;
            });
        }

        /*private void ImageScanCommand_SequentialChannelCompleted(ThreadBase sender, LaserChannels channel)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)delegate
            {
                // Deselect the preview channel
                if (channel == LaserChannels.ChannelA)
                {
                    _ImagingVm.IsLaserR1PrvSelected = false;
                }
                if (channel == LaserChannels.ChannelB)
                {
                    _ImagingVm.IsLaserR2PrvSelected = false;
                }
                if (channel == LaserChannels.ChannelC)
                {
                    _ImagingVm.IsLaserL1PrvSelected = false;
                }
            });
        }*/

        private void ImageScanCommand_ZScanningCompleted(ThreadBase sender, ImageInfo imageInfo, int nScanRegion)
        {
            Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
            {
                LaserScanCommand scanThread = sender as LaserScanCommand;
                bool bIsZScanning = scanThread.CurrentScanParam.IsZScanning;
                bool bIsSequentialScanning = scanThread.CurrentScanParam.IsSequentialScanning;
                WriteableBitmap scannedImageChA = null;
                WriteableBitmap scannedImageChB = null;
                WriteableBitmap scannedImageChC = null;
                WriteableBitmap laserL1RawImage = null;
                WriteableBitmap laserR1RawImage = null;
                WriteableBitmap laserR2RawImage = null;

                bool bIsAutoMergeImages = _SelectedAppProtocol.ScanRegions[nScanRegion].IsAutoMergeImages; ;

                for (int i = 0; i < _SelectedAppProtocol.ScanRegions[nScanRegion].SignalCount; i++)
                {
                    if (_SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].SelectedLaser.LaserChannel == LaserChannels.ChannelC)   //L1
                    {
                        scannedImageChC = scanThread.ChannelCImage.Clone();
                        if (SettingsManager.ConfigSettings.IsKeepRawImages)
                        {
                            laserL1RawImage = scanThread.ChannelCRawImage.Clone();
                        }
                        ImageProcessingHelper.FastClear(ref _ChannelL1PrevImageUnAligned);
                        ImageProcessingHelper.FastClear(ref _ChannelL1PrevImage);
                    }
                    if (_SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].SelectedLaser.LaserChannel == LaserChannels.ChannelA)   //R1
                    {
                        scannedImageChA = scanThread.ChannelAImage.Clone();
                        if (SettingsManager.ConfigSettings.IsKeepRawImages)
                        {
                            laserR1RawImage = scanThread.ChannelARawImage.Clone();
                        }
                        ImageProcessingHelper.FastClear(ref _ChannelR1PrevImageUnAligned);
                        ImageProcessingHelper.FastClear(ref _ChannelR1PrevImage);
                    }
                    if (_SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].SelectedLaser.LaserChannel == LaserChannels.ChannelB)   //R2
                    {
                        scannedImageChB = scanThread.ChannelBImage.Clone();
                        if (SettingsManager.ConfigSettings.IsKeepRawImages)
                        {
                            laserR2RawImage = scanThread.ChannelBRawImage.Clone();
                        }
                        ImageProcessingHelper.FastClear(ref _ChannelR2PrevImageUnAligned);
                        ImageProcessingHelper.FastClear(ref _ChannelR2PrevImage);
                    }
                }

                if (imageInfo != null)
                {
                    imageInfo.CaptureType = "Fluorescence";
                    imageInfo.SoftwareVersion = Workspace.This.ProductVersion;
                    imageInfo.FWVersion = Workspace.This.FWVersion;
                    imageInfo.SystemSN = Workspace.This.SystemSN;
                    imageInfo.Software = "Sapphire FL";
                    string smartScan = (_IsAutoScan) ? "Smartscan" : string.Empty;
                    string seqScan = (bIsSequentialScanning) ? "Sequential" : string.Empty;
                    string zScan = (bIsZScanning) ? "Z-Scan" : string.Empty;
                    string separator1 = (_IsAutoScan && bIsSequentialScanning) ? " + " : string.Empty;
                    string separator2 = ((_IsAutoScan || bIsSequentialScanning) && bIsZScanning) ? " + " : string.Empty;
                    imageInfo.ScanType = string.Format("{0}{1}{2}{3}{4}", smartScan, separator1, seqScan, separator2, zScan);
                    imageInfo.ScanZ0Abs = Workspace.This.MotorVM.AbsZPos;
                    //string signOfNumber = (Workspace.This.MotorVM.AbsZPos <= Math.Round(imageInfo.ScanZ0, 2)) ? "+" : "-";
                    //    Math.Abs(imageInfo.ScanZ0 - Workspace.This.MotorVM.AbsZPos).ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
                    int x1 = (int)Math.Round(_SelectedAppProtocol.ScanRegions[nScanRegion].ScanRect.X / _ImagingVm.CellSize);
                    int y1 = (int)Math.Round(_SelectedAppProtocol.ScanRegions[nScanRegion].ScanRect.Y / _ImagingVm.CellSize);
                    int x2 = (int)Math.Round(x1 + (_SelectedAppProtocol.ScanRegions[nScanRegion].ScanRect.Width / _ImagingVm.CellSize));
                    int y2 = (int)Math.Round(y1 + (_SelectedAppProtocol.ScanRegions[nScanRegion].ScanRect.Height / _ImagingVm.CellSize));
                    string row = string.Format("{0}-{1}", Workspace.This.IndexToRow(y1), Workspace.This.IndexToRow(y2));
                    string col = string.Format("{0}-{1}", x1, x2);
                    imageInfo.ScanRegion = row + ", " + col;

                    #region === Filter wavelength ===
                    if (imageInfo.RedChannel.LaserIntensity > 0)
                    {
                        int wavelength = int.Parse(imageInfo.RedChannel.LaserWavelength);
                        foreach (var laserType in Workspace.This.LaserOptions)
                        {
                            if (!Workspace.This.IsPhosphorModule(laserType))
                            {
                                if (laserType.Wavelength == wavelength)
                                {
                                    imageInfo.RedChannel.FilterWavelength = laserType.Filter;
                                    break;
                                }
                            }
                        }
                        // Can sometimes have multiple lasers with the save wavelength (638/APD and 638/PMT)
                        //var laserType = Workspace.This.LaserOptions.SingleOrDefault(item => item.Wavelength == int.Parse(imageInfo.RedChannel.LaserWavelength));
                        //imageInfo.RedChannel.FilterWavelength = laserType.Filter;
                    }
                    if (imageInfo.GreenChannel.LaserIntensity > 0)
                    {
                        int wavelength = int.Parse(imageInfo.GreenChannel.LaserWavelength);
                        foreach (var laserType in Workspace.This.LaserOptions)
                        {
                            if (!Workspace.This.IsPhosphorModule(laserType))
                            {
                                if (laserType.Wavelength == wavelength)
                                {
                                    imageInfo.GreenChannel.FilterWavelength = laserType.Filter;
                                    break;
                                }
                            }
                        }
                        // Can sometimes have multiple lasers with the save wavelength (638/APD and 638/PMT)
                        //var laserType = Workspace.This.LaserOptions.SingleOrDefault(item => item.Wavelength == int.Parse(imageInfo.GreenChannel.LaserWavelength));
                        //imageInfo.GreenChannel.FilterWavelength = laserType.Filter;
                    }
                    if (imageInfo.BlueChannel.LaserIntensity > 0)
                    {
                        int wavelength = int.Parse(imageInfo.BlueChannel.LaserWavelength);
                        foreach (var laserType in Workspace.This.LaserOptions)
                        {
                            if (!Workspace.This.IsPhosphorModule(laserType))
                            {
                                if (laserType.Wavelength == wavelength)
                                {
                                    imageInfo.BlueChannel.FilterWavelength = laserType.Filter;
                                    break;
                                }
                            }
                        }
                        // Can sometimes have multiple lasers with the save wavelength (638/APD and 638/PMT)
                        //var laserType = Workspace.This.LaserOptions.SingleOrDefault(item => item.Wavelength == int.Parse(imageInfo.BlueChannel.LaserWavelength));
                        //imageInfo.BlueChannel.FilterWavelength = laserType.Filter;
                    }
                    if (imageInfo.GrayChannel.LaserIntensity > 0)
                    {
                        int wavelength = int.Parse(imageInfo.GrayChannel.LaserWavelength);
                        foreach (var laserType in Workspace.This.LaserOptions)
                        {
                            if (!Workspace.This.IsPhosphorModule(laserType))
                            {
                                if (laserType.Wavelength == wavelength)
                                {
                                    imageInfo.GrayChannel.FilterWavelength = laserType.Filter;
                                    break;
                                }
                            }
                        }
                        // Can sometimes have multiple lasers with the save wavelength (638/APD and 638/PMT)
                        //var laserType = Workspace.This.LaserOptions.SingleOrDefault(item => item.Wavelength == int.Parse(imageInfo.GrayChannel.LaserWavelength));
                        //imageInfo.GrayChannel.FilterWavelength = laserType.Filter;
                    }
                    if (imageInfo.MixChannel.LaserIntensity > 0)
                    {
                        int wavelength = int.Parse(imageInfo.MixChannel.LaserWavelength);
                        foreach (var laserType in Workspace.This.LaserOptions)
                        {
                            if (!Workspace.This.IsPhosphorModule(laserType))
                            {
                                if (laserType.Wavelength == wavelength)
                                {
                                    imageInfo.MixChannel.FilterWavelength = laserType.Filter;
                                    break;
                                }
                            }
                        }
                        // Can sometimes have multiple lasers with the save wavelength (638/APD and 638/PMT)
                        //var laserType = Workspace.This.LaserOptions.SingleOrDefault(item => item.Wavelength == int.Parse(imageInfo.MixChannel.LaserWavelength));
                        //imageInfo.MixChannel.FilterWavelength = laserType.Filter;
                    }
                    #endregion

                    // Get scan region
                    if (imageInfo.ScanX0 > 0)
                    {
                        imageInfo.ScanX0 = (int)((imageInfo.ScanX0 - Workspace.This.MotorVM.AbsXPos) / (double)_XMotorSubdivision);
                    }
                    if (imageInfo.ScanY0 > 0)
                    {
                        imageInfo.ScanY0 = (int)((imageInfo.ScanY0 - Workspace.This.MotorVM.AbsYPos) / (double)_YMotorSubdivision);
                    }
                    if (_SelectedAppProtocol.ScanRegions[nScanRegion].IsCustomFocus)
                    {
                        string valueSign = (_SelectedAppProtocol.ScanRegions[nScanRegion].CustomFocusValue > 0) ? "+" : string.Empty;
                        imageInfo.SampleType = string.Format("{0} ({1}{2})",
                            _SelectedAppProtocol.ScanRegions[nScanRegion].SelectedSampleType.DisplayName,
                            valueSign,
                            _SelectedAppProtocol.ScanRegions[nScanRegion].CustomFocusValue.ToString("0.00"));
                    }
                    else
                    {
                        imageInfo.SampleType = _SelectedAppProtocol.ScanRegions[nScanRegion].SelectedSampleType.DisplayName;
                    }
                    imageInfo.ScanSpeed = _SelectedAppProtocol.ScanRegions[nScanRegion].SelectedScanSpeed.DisplayName;
                    imageInfo.ScanQuality = _SelectedAppProtocol.ScanRegions[nScanRegion].SelectedScanQuality.DisplayName;
                    imageInfo.Comment = _SelectedAppProtocol.ScanRegions[nScanRegion].FileLocationVm.Notes;
                }

                try
                {
                    //Add image to Gallery
                    //

                    string docTitle = string.Empty;
                    string fileNameWithoutExt = string.Empty;
                    string fileExtension = ".tif";  // default file extension
                    string destinationFolder = _SelectedAppProtocol.ScanRegions[nScanRegion].FileLocationVm.DestinationFolder;
                    string fileFullPath = string.Empty;
                    bool bIsSaveAsPUB = false;

                    string tmpFileName = string.Empty;
                    if (_SelectedAppProtocol.ScanRegions[nScanRegion].FileLocationVm.IsAutoSave)
                    {
                        bIsSaveAsPUB = SettingsManager.ApplicationSettings.IsAutoSavePubFileEnabled;

                        if (string.IsNullOrEmpty(_SelectedAppProtocol.ScanRegions[nScanRegion].FileLocationVm.FileName.Trim()))
                        {
                            // file name not specified, generate a new file name
                            tmpFileName = Workspace.This.GenerateFileName(string.Empty, string.Empty);
                        }
                        else
                        {
                            var flFileName = _SelectedAppProtocol.ScanRegions[nScanRegion].FileLocationVm.FileName.Trim();
                            if (Workspace.This.CheckSupportedFileType(flFileName))
                                tmpFileName = System.IO.Path.GetFileNameWithoutExtension(flFileName);
                            else
                                tmpFileName = flFileName;
                        }
                    }
                    else
                    {
                        if (_ScanImageBaseFilename == null || _ScanImageBaseFilename.Count == 0)
                        {
                            tmpFileName = Workspace.This.GenerateFileName(string.Empty, string.Empty);
                        }
                        else
                        {
                            // file name not specified, generate a new file name
                            if (string.IsNullOrEmpty(_ScanImageBaseFilename[scanThread.ZSequentialSetIndex]))
                            {
                                tmpFileName = Workspace.This.GenerateFileName(string.Empty, string.Empty);
                            }
                            else
                            {
                                tmpFileName = _ScanImageBaseFilename[scanThread.ZSequentialSetIndex];
                            }
                        }
                    }
                    string signOfNumber = (Workspace.This.MotorVM.AbsZPos <= Math.Round(imageInfo.ScanZ0, 2)) ? "+" : "-";
                    fileNameWithoutExt = string.Format("{0}_({1}{2})", tmpFileName, signOfNumber,
                        Math.Abs(imageInfo.ScanZ0 - Workspace.This.MotorVM.AbsZPos).ToString("F2", System.Globalization.CultureInfo.InvariantCulture));

                    WriteableBitmap[] scannedImages;
                    if (_Is4channelImage)
                        scannedImages = new WriteableBitmap[] { null, null, null, null };
                    else
                        scannedImages = new WriteableBitmap[] { null, null, null };

                    int nLaserSignalLevel = 0;

                    for (int i = 0; i < _SelectedAppProtocol.ScanRegions[nScanRegion].SignalCount; i++)
                    {
                        if (_SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].SelectedLaser.LaserChannel == LaserChannels.ChannelC)       //L1
                        {
                            nLaserSignalLevel = _SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].SelectedSignalLevel.IntensityLevel;
                            if (_IsGrayscaleImage)
                            {
                                scannedImages[0] = scannedImageChC;
                            }
                            else
                            {
                                SetImageChannel(ref scannedImages,
                                                ref scannedImageChC,
                                                _SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].SelectedColorChannel.ImageColorChannel);
                                if (!bIsAutoMergeImages)
                                {
                                    docTitle = string.Format("{0}_{1}{2}", fileNameWithoutExt, Workspace.This.LaserChannelTypeList[LaserChannels.ChannelC], fileExtension);
                                    if (_SelectedAppProtocol.ScanRegions[nScanRegion].FileLocationVm.IsAutoSave)
                                    {
                                        fileFullPath = System.IO.Path.Combine(destinationFolder, docTitle);
                                        var clonedImage = scannedImageChC.Clone();
                                        clonedImage.Freeze();
                                        Workspace.This.SaveAsync(clonedImage, imageInfo, fileFullPath, false);
                                    }
                                    else
                                    {
                                        fileFullPath = string.Empty;
                                    }
                                    Workspace.This.NewDocument(scannedImageChC, imageInfo, docTitle, fileFullPath, bIsSaveAsPUB, true);
                                }
                            }
                            // Keep raw image
                            if (SettingsManager.ConfigSettings.IsKeepRawImages && laserL1RawImage != null)
                            {
                                string rawDocTitle = string.Format("{0}_{1}_RAW{2}", fileNameWithoutExt, Workspace.This.LaserChannelTypeList[LaserChannels.ChannelC], fileExtension);
                                Workspace.This.NewDocument(laserL1RawImage, imageInfo, rawDocTitle, fileFullPath, false, true);
                            }
                        }
                        if (_SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].SelectedLaser.LaserChannel == LaserChannels.ChannelA)   //R1
                        {
                            nLaserSignalLevel = _SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].SelectedSignalLevel.IntensityLevel;
                            if (_IsGrayscaleImage)
                            {
                                scannedImages[0] = scannedImageChA;
                            }
                            else
                            {
                                SetImageChannel(ref scannedImages,
                                                ref scannedImageChA,
                                                _SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].SelectedColorChannel.ImageColorChannel);
                                if (!bIsAutoMergeImages)
                                {
                                    docTitle = string.Format("{0}_{1}{2}", fileNameWithoutExt, Workspace.This.LaserChannelTypeList[LaserChannels.ChannelA], fileExtension);
                                    if (_SelectedAppProtocol.ScanRegions[nScanRegion].FileLocationVm.IsAutoSave)
                                    {
                                        fileFullPath = System.IO.Path.Combine(destinationFolder, docTitle);
                                        var clonedImage = scannedImageChA.Clone();
                                        clonedImage.Freeze();
                                        Workspace.This.SaveAsync(clonedImage, imageInfo, fileFullPath, false);
                                    }
                                    else
                                    {
                                        fileFullPath = string.Empty;
                                    }
                                    Workspace.This.NewDocument(scannedImageChA, imageInfo, docTitle, fileFullPath, bIsSaveAsPUB, true);
                                }
                            }
                            // Keep raw image
                            if (SettingsManager.ConfigSettings.IsKeepRawImages && laserR1RawImage != null)
                            {
                                string rawDocTitle = string.Format("{0}_{1}_RAW{2}", fileNameWithoutExt, Workspace.This.LaserChannelTypeList[LaserChannels.ChannelA], fileExtension);
                                Workspace.This.NewDocument(laserR1RawImage, imageInfo, rawDocTitle, fileFullPath, false, true);
                            }
                        }
                        if (_SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].SelectedLaser.LaserChannel ==  LaserChannels.ChannelB)  //R2
                        {
                            nLaserSignalLevel = _SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].SelectedSignalLevel.IntensityLevel;
                            if (_IsGrayscaleImage)
                            {
                                scannedImages[0] = scannedImageChB;
                            }
                            else
                            {
                                SetImageChannel(ref scannedImages,
                                                ref scannedImageChB,
                                                _SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].SelectedColorChannel.ImageColorChannel);
                                if (!bIsAutoMergeImages)
                                {
                                    docTitle = string.Format("{0}_{1}{2}", fileNameWithoutExt, Workspace.This.LaserChannelTypeList[LaserChannels.ChannelB], fileExtension);
                                    if (_SelectedAppProtocol.ScanRegions[nScanRegion].FileLocationVm.IsAutoSave)
                                    {
                                        fileFullPath = System.IO.Path.Combine(destinationFolder, docTitle);
                                        var clonedImage = scannedImageChB.Clone();
                                        clonedImage.Freeze();
                                        Workspace.This.SaveAsync(clonedImage, imageInfo, fileFullPath, false);
                                    }
                                    else
                                    {
                                        fileFullPath = string.Empty;
                                    }
                                    Workspace.This.NewDocument(scannedImageChB, imageInfo, docTitle, fileFullPath, bIsSaveAsPUB, true);
                                }
                            }
                            // Keep raw image
                            if (SettingsManager.ConfigSettings.IsKeepRawImages && laserR2RawImage != null)
                            {
                                string rawDocTitle = string.Format("{0}_{1}_RAW{2}", fileNameWithoutExt, Workspace.This.LaserChannelTypeList[LaserChannels.ChannelB], fileExtension);
                                Workspace.This.NewDocument(laserR2RawImage, imageInfo, rawDocTitle, fileFullPath, false, true);
                            }
                        }
                    }
                    // Now using the accending order of laser's wavelength instead of using laser A/B/C/D (swapping laser A and laser D)
                    //imageInfo.IntensityLevel = string.Format("{0} / {1} / {2} / {3}", laserDSignalLevelDN, laserBSignalLevelDN, laserCSignalLevelDN, laserASignalLevelDN);  //EL: TODO:

                    if (bIsAutoMergeImages || _IsGrayscaleImage)
                    {
                        WriteableBitmap scannedImage = null;
                        if (_IsGrayscaleImage)
                        {
                            scannedImage = scannedImages[0];
                        }
                        else
                        {
                            // MERGE color channels
                            //
                            // Make sure we have a scanned image
                            bool hasImages = false;
                            foreach (var image in scannedImages)
                            {
                                if (image != null)
                                {
                                    hasImages = true;
                                    break;
                                }
                            }
                            if (hasImages)
                            {
                                scannedImage = ImageProcessing.SetChannel(scannedImages);
                                if (scannedImage != null)
                                {
                                    if (scannedImage.CanFreeze)
                                        scannedImage.Freeze();
                                }
                            }
                        }

                        if (scannedImage != null)
                        {
                            docTitle = string.Format("{0}{1}", fileNameWithoutExt, fileExtension);
                            if (_SelectedAppProtocol.ScanRegions[nScanRegion].FileLocationVm.IsAutoSave)
                            {
                                fileFullPath = System.IO.Path.Combine(destinationFolder, docTitle);
                                var clonedImage = scannedImage.Clone();
                                clonedImage.Freeze();
                                Workspace.This.SaveAsync(clonedImage, imageInfo, fileFullPath, false);
                            }
                            else
                            {
                                fileFullPath = string.Empty;
                            }
                            // Add the new scanned image to Gallery
                            Workspace.This.NewDocument(scannedImage, imageInfo, docTitle, fileFullPath, bIsSaveAsPUB, true);
                        }
                    }

                    //clear preivew view image
                    _ImagingVm.PreviewImage = null;

                }
                catch (Exception)
                {
                    throw;
                }
            });
        }

        /*private void ImageScanCommand_EDRTestScanStarting(ThreadBase sender)
        {
            Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
            {
                var imageScanCommand = sender as LaserScanCommand;
                var currentScanParam = imageScanCommand.CurrentScanParam;
                int nScanRegion = imageScanCommand.CurrentScanRegion;
                List<string> signalLevels = new List<string>();

                Workspace.This.LogMessage("EDR scan (PART2) PreviewImageSetup");

                PreviewImageSetup(currentScanParam, nScanRegion);
                
                Workspace.This.LogMessage(string.Format("EDR scan (PART2) starting [Scan Region #{0}]", nScanRegion + 1));
                foreach (var signal in currentScanParam.Signals)
                {
                    Workspace.This.LogMessage(string.Format("EDR scan (PART2) signal level: {0}", signal.DisplayName));
                }

                //_IsAutoScanCompleted = false;
                //_CurrentScanType = ScanType.Auto;

                //if (nScanRegion != _CurrentScanRegion)
                //{
                //    PreviewImageSetup(currentScanParam);
                //}
                //_CurrentScanRegion = nScanRegion;

                // Update signal levels
                //_SelectedAppProtocol.IsModified = true;

                Workspace.This.LogMessage("EDR scan (PART2) update UI signal level");

                try
                {
                    for (int i = 0; i < _SelectedAppProtocol.ScanRegions[nScanRegion].SignalCount; i++)
                    {
                        _SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].IsInitializing = true;    //Don't trigger selection changed event
                        _SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].SelectedSignalLevel = _SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].SignalLevelOptions[currentScanParam.Signals[i].Position - 1];
                        _SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].IsInitializing = false;
                        signalLevels.Add(_SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].SignalLevelOptions[currentScanParam.Signals[i].Position - 1].DisplayName);
                        // Clear the display image buffers
                        //if (_SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].SelectedLaser.LaserChannel == LaserChannels.ChannelC)   //L1
                        //{
                        //    ImageProcessingHelper.FastClear(ref _ChannelL1PrevImageUnAligned);
                        //}
                        //if (_SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].SelectedLaser.LaserChannel == LaserChannels.ChannelA)   //R1
                        //{
                        //    ImageProcessingHelper.FastClear(ref _ChannelR1PrevImageUnAligned);
                        //}
                        //if (_SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].SelectedLaser.LaserChannel == LaserChannels.ChannelB)   //R2
                        //{
                        //    ImageProcessingHelper.FastClear(ref _ChannelR2PrevImageUnAligned);
                        //}
                    }
                }
                catch
                {
                }

                // Clear preview display image
                _ImagingVm.PreviewImage = null;
                // Don't show saturation when doing EDR scanning
                _ImagingVm.ContrastVm.DisplayImageInfo.IsSaturationChecked = false;

                string scanLevels = string.Join(",", signalLevels);
                Workspace.This.LogMessage(string.Format("Signal levels: {0}", scanLevels));

            });
        }*/

        private void ImageScanCommand_EDRTestScanUpdating(ThreadBase sender)
        {
            Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
            {
                var imageScanCommand = sender as LaserScanCommand;
                var currentScanParam = imageScanCommand.CurrentScanParam;
                int nScanRegion = imageScanCommand.CurrentScanRegion;
                var updatedSignals = imageScanCommand.UpdatedSignalLevel;
                List<string> signalLevels = new List<string>();

                //PreviewImageSetup(currentScanParam);

                Workspace.This.LogMessage(string.Format("EDR scan updating signal level [Scan Region #{0}]", nScanRegion + 1));
                foreach (var signal in currentScanParam.Signals)
                {
                    Workspace.This.LogMessage(string.Format("EDR scan updating UI signal level: {0}", signal.DisplayName));
                }

                //_IsAutoScanCompleted = false;
                //_CurrentScanType = ScanType.Auto;

                for (int i = 0; i < _SelectedAppProtocol.ScanRegions[nScanRegion].SignalCount; i++)
                {
                    // Clear the display image buffers
                    if (_SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].SelectedLaser.LaserChannel == LaserChannels.ChannelC)   //L1
                    {
                        ImageProcessingHelper.FastClear(ref _ChannelL1PrevImageUnAligned);
                        _ImagingVm.IsLaserL1PrvSelected = true;
                    }
                    if (_SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].SelectedLaser.LaserChannel == LaserChannels.ChannelA)   //R1
                    {
                        ImageProcessingHelper.FastClear(ref _ChannelR1PrevImageUnAligned);
                        _ImagingVm.IsLaserR1PrvSelected = true;
                    }
                    if (_SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].SelectedLaser.LaserChannel == LaserChannels.ChannelB)   //R2
                    {
                        ImageProcessingHelper.FastClear(ref _ChannelR2PrevImageUnAligned);
                        _ImagingVm.IsLaserR2PrvSelected = true;
                    }
                }

                if (nScanRegion != _CurrentScanRegion)
                {
                    PreviewImageSetup(currentScanParam, nScanRegion);
                }
                _CurrentScanRegion = nScanRegion;

                // Update signal levels
                for (int i = 0; i < updatedSignals.Count; i++)
                {
                    for (int j = 0; j < _SelectedAppProtocol.ScanRegions[nScanRegion].SignalList.Count; j++)
                    {
                        if (updatedSignals[i].LaserChannel == _SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[j].SelectedLaser.LaserChannel)
                        {
                            _SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[j].IsInitializing = true;    //Don't trigger selection changed event
                            _SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[j].SelectedSignalLevel = _SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].SignalLevelOptions[updatedSignals[i].Position - 1];
                            _SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[j].IsInitializing = false;
                            signalLevels.Add(_SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[j].SignalLevelOptions[updatedSignals[i].Position - 1].DisplayName);

                            // Reset the Black and White contrast value
                            if (_SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].SelectedColorChannel.ImageColorChannel == ImageChannelType.Red)
                            {
                                _ImagingVm.ContrastVm.DisplayImageInfo.RedChannel.BlackValue = 0;
                                _ImagingVm.ContrastVm.DisplayImageInfo.RedChannel.WhiteValue = 65535;
                            }
                            if (_SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].SelectedColorChannel.ImageColorChannel == ImageChannelType.Green)
                            {
                                _ImagingVm.ContrastVm.DisplayImageInfo.GreenChannel.BlackValue = 0;
                                _ImagingVm.ContrastVm.DisplayImageInfo.GreenChannel.WhiteValue = 65535;
                            }
                            if (_SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].SelectedColorChannel.ImageColorChannel == ImageChannelType.Blue)
                            {
                                _ImagingVm.ContrastVm.DisplayImageInfo.BlueChannel.BlackValue = 0;
                                _ImagingVm.ContrastVm.DisplayImageInfo.BlueChannel.WhiteValue = 65535;
                            }
                            if (_SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].SelectedColorChannel.ImageColorChannel == ImageChannelType.Gray)
                            {
                                _ImagingVm.ContrastVm.DisplayImageInfo.GrayChannel.BlackValue = 0;
                                _ImagingVm.ContrastVm.DisplayImageInfo.GrayChannel.WhiteValue = 65535;
                            }
                            if (_SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].SelectedColorChannel.ImageColorChannel == ImageChannelType.Mix)
                            {
                                _ImagingVm.ContrastVm.DisplayImageInfo.MixChannel.BlackValue = 0;
                                _ImagingVm.ContrastVm.DisplayImageInfo.MixChannel.WhiteValue = 65535;
                            }
                        }
                    }
                }

                // Reset the Black and White contrast value
                _ImagingVm.ContrastVm.BlackValue = 0;
                _ImagingVm.ContrastVm.WhiteValue = 65535;

                // Clear preview display image
                _ImagingVm.PreviewImage = null;
                // Don't show saturation when doing EDR scanning
                _ImagingVm.ContrastVm.DisplayImageInfo.IsSaturationChecked = false;

                string scanLevels = string.Join(",", signalLevels);
                Workspace.This.LogMessage(string.Format("Scanning At Signal levels: {0}", scanLevels));
            });
        }

        /*private void ImageScanCommand_EDRTestScanCompleted(ThreadBase sender)
        {
            Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
            {
                var imageScanCommand = sender as LaserScanCommand;
                var currentScanParam = imageScanCommand.CurrentScanParam;
                int nScanRegion = imageScanCommand.CurrentScanRegion;
                List<string> signalLevels = new List<string>();

                PreviewImageSetup(currentScanParam, nScanRegion);

                Workspace.This.LogMessage(string.Format("EDR test scan completed [Scan Region #{0}]", nScanRegion + 1));
                foreach (var signal in currentScanParam.Signals)
                {
                    Workspace.This.LogMessage(string.Format("EDR scan signal level: {0}", signal.DisplayName));
                }

                //_IsAutoScanCompleted = false;
                //_CurrentScanType = ScanType.Auto;

                //if (nScanRegion != _CurrentScanRegion)
                //{
                //    PreviewImageSetup(currentScanParam);
                //}
                //_CurrentScanRegion = nScanRegion;

                // Update signal levels
                //_SelectedAppProtocol.IsModified = true;
                for (int i = 0; i < _SelectedAppProtocol.ScanRegions[nScanRegion].SignalCount; i++)
                {
                    _SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].IsInitializing = true;    //Don't trigger selection changed event
                    _SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].SelectedSignalLevel = _SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].SignalLevelOptions[currentScanParam.Signals[i].Position - 1];
                    _SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].IsInitializing = false;
                    signalLevels.Add(_SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].SignalLevelOptions[currentScanParam.Signals[i].Position - 1].DisplayName);

                    // Reset the Black and White contrast value
                    if (_SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].SelectedColorChannel.ImageColorChannel == ImageChannelType.Red)
                    {
                        _ImagingVm.ContrastVm.DisplayImageInfo.RedChannel.BlackValue = 0;
                        _ImagingVm.ContrastVm.DisplayImageInfo.RedChannel.WhiteValue = 65535;
                    }
                    if (_SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].SelectedColorChannel.ImageColorChannel == ImageChannelType.Green)
                    {
                        _ImagingVm.ContrastVm.DisplayImageInfo.GreenChannel.BlackValue = 0;
                        _ImagingVm.ContrastVm.DisplayImageInfo.GreenChannel.WhiteValue = 65535;
                    }
                    if (_SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].SelectedColorChannel.ImageColorChannel == ImageChannelType.Blue)
                    {
                        _ImagingVm.ContrastVm.DisplayImageInfo.BlueChannel.BlackValue = 0;
                        _ImagingVm.ContrastVm.DisplayImageInfo.BlueChannel.WhiteValue = 65535;
                    }
                    if (_SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].SelectedColorChannel.ImageColorChannel == ImageChannelType.Gray)
                    {
                        _ImagingVm.ContrastVm.DisplayImageInfo.GrayChannel.BlackValue = 0;
                        _ImagingVm.ContrastVm.DisplayImageInfo.GrayChannel.WhiteValue = 65535;
                    }
                    if (_SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].SelectedColorChannel.ImageColorChannel == ImageChannelType.Mix)
                    {
                        _ImagingVm.ContrastVm.DisplayImageInfo.MixChannel.BlackValue = 0;
                        _ImagingVm.ContrastVm.DisplayImageInfo.MixChannel.WhiteValue = 65535;
                    }
                    if (_SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].SelectedLaser.LaserChannel == LaserChannels.ChannelC)
                    {
                        _ImagingVm.IsLaserL1PrvSelected = true;
                    }
                    if (_SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].SelectedLaser.LaserChannel == LaserChannels.ChannelA)   //R1
                    {
                        _ImagingVm.IsLaserR1PrvSelected = true;
                    }
                    if (_SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].SelectedLaser.LaserChannel == LaserChannels.ChannelB)   //R2
                    {
                        _ImagingVm.IsLaserR2PrvSelected = true;
                    }

                    // Clear the display image buffers
                    //if (_SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].SelectedLaser.LaserChannel == LaserChannels.ChannelC)   //L1
                    //{
                    //    ImageProcessingHelper.FastClear(ref _ChannelL1PrevImageUnAligned);
                    //}
                    //if (_SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].SelectedLaser.LaserChannel == LaserChannels.ChannelA)   //R1
                    //{
                    //    ImageProcessingHelper.FastClear(ref _ChannelR1PrevImageUnAligned);
                    //}
                    //if (_SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].SelectedLaser.LaserChannel == LaserChannels.ChannelB)   //R2
                    //{
                    //    ImageProcessingHelper.FastClear(ref _ChannelR2PrevImageUnAligned);
                    //}
                }

                // Clear preview display image
                _ImagingVm.PreviewImage = null;

                string scanLevels = string.Join(",", signalLevels);
                Workspace.This.LogMessage(string.Format("Signal levels: {0}", scanLevels));

            });
        }*/

        private void SetImageChannel(ref WriteableBitmap[] arrSrcImages,
                                     ref WriteableBitmap srcimg,
                                     ImageChannelType imgChannelType)
        {
            switch (imgChannelType)
            {
                case ImageChannelType.Red:
                    arrSrcImages[0] = srcimg;
                    break;
                case ImageChannelType.Green:
                    arrSrcImages[1] = srcimg;
                    break;
                case ImageChannelType.Blue:
                    arrSrcImages[2] = srcimg;
                    break;
                case ImageChannelType.Gray:
                    arrSrcImages[3] = srcimg;
                    break;
            }
        }

        /*private void ImageScanCommand_ReceiveTransfer(ThreadBase sender, string scanType) //display received data in real time
        {
            LaserScanCommand currentScanThread = (sender as LaserScanCommand);

            if (scanType == "ScannerIsReady")
            {
                Workspace.This.IsReadyScanning = true;
                Workspace.This.IsPreparingToScan = false;
                //if (!_UiUpdateTimer.Enabled)
                //{
                //    _UiUpdateTimer.Start();
                //}
            }

            if (scanType == "LIDStatus")
            {
                //EL: TODO:
                //Workspace.This.ApdVM.LIDIsOpen = Workspace.This.ApdVM.APDTransfer.LIDIsOpen;
                //if (Workspace.This.ApdVM.APDTransfer.LIDIsOpen)
                //{
                //    _IsAbortedOnLidOpened = true;
                //    ExecuteStopScanCommand("LIDOpened");
                //}
            }

            if (scanType == "RemainingTime")
            {
                if (_CurrentScanType != ScanType.Auto || (_CurrentScanType == ScanType.Auto && _IsAutoScanCompleted))
                {
                    RemainingTime = currentScanThread.RemainingTime;
                    double timeElapsed = Time - RemainingTime;
                    //Workspace.This.StatusTextProgress = ImagingSystemHelper.FormatTime(timeElapsed);
                    //Workspace.This.PercentCompleted = (int)((timeElapsed / (double)Time) * 100.0);
                    Workspace.This.StatusTextProgress = ImagingSystemHelper.FormatTime(RemainingTime);
                    Workspace.This.PercentCompleted = (int)((timeElapsed / (double)Time) * 100.0);
                }
            }
        }*/

        /*private void ImageScanCommand_DataReceived(ushort[] apdChannelA, ushort[] apdChannelB, ushort[] apdChannelC, ushort[] apdChannelD)
        {
            if (!_IsPreviewSetupCompleted) { return; }

            if (apdChannelA == null && apdChannelB == null &&
                apdChannelC == null && apdChannelD == null)
            {
                return;
            }

            // Unaligned image with and height
            int imageWidth = 0;
            int imageHeight = 0;

            if (_ImagingVm.PreviewChannels == null || _ImagingVm.NumOfDisplayChannels == 0)
            {
                if (this.ImagingVm.PreviewImage != null)
                    this.ImagingVm.PreviewImage = null;     //Clear prievew image
                return;
            }

            //_IsScanDataReceived = true;

            if (_IsUpdatingPreviewImage)
            {
                return;
            }

            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                _IsUpdatingPreviewImage = true;
                //_SelectedChannels.Clear();

                //if (ImagingVm.PreviewChannels.Count > 0)
                //{
                //    foreach (var channel in _ImagingVm.PreviewChannels)
                //    {
                //        if (channel == LaserType.LaserA)        // 784
                //        {
                //            if (apdChannelA != null && (_ChannelAPrevImage != null || _ChannelAPrevImageUnAligned != null))
                //            {
                //                if (_IsCropPreviewImage)
                //                {
                //                    if (_ChannelAPrevImageUnAligned != null)
                //                    {
                //                        ImageProcessing.FrameToBitmap(apdChannelA, ref _ChannelAPrevImageUnAligned);
                //                        imageWidth = _ChannelAPrevImageUnAligned.PixelWidth;
                //                        imageHeight = _ChannelAPrevImageUnAligned.PixelHeight;
                //                    }
                //                }
                //                else
                //                {
                //                    if (_ChannelAPrevImage != null)
                //                        ImageProcessing.FrameToBitmap(apdChannelA, ref _ChannelAPrevImage);
                //                }
                //                _SelectedChannels.Add(new LaserTypeColorChannel(LaserType.LaserA, _LaserAColorChannel));
                //            }
                //        }
                //        else if (channel == LaserType.LaserB)   // 520
                //        {
                //            if (apdChannelB != null && (_ChannelBPrevImage != null || _ChannelBPrevImageUnAligned != null))
                //            {
                //                if (_IsCropPreviewImage)
                //                {
                //                    if (_ChannelBPrevImageUnAligned != null)
                //                    {
                //                        ImageProcessing.FrameToBitmap(apdChannelB, ref _ChannelBPrevImageUnAligned);
                //                        imageWidth = _ChannelBPrevImageUnAligned.PixelWidth;
                //                        imageHeight = _ChannelBPrevImageUnAligned.PixelHeight;
                //                    }
                //                }
                //                else
                //                {
                //                    if (_ChannelBPrevImage != null)
                //                        ImageProcessing.FrameToBitmap(apdChannelB, ref _ChannelBPrevImage);
                //                }
                //                _SelectedChannels.Add(new LaserTypeColorChannel(LaserType.LaserB, _LaserBColorChannel));
                //            }
                //        }
                //        else if (channel == LaserType.LaserC)   // 658
                //        {
                //            if (apdChannelC != null && (_ChannelCPrevImage != null || _ChannelCPrevImageUnAligned != null))
                //            {
                //                if (_IsCropPreviewImage)
                //                {
                //                    if (_ChannelCPrevImageUnAligned != null)
                //                    {
                //                        ImageProcessing.FrameToBitmap(apdChannelC, ref _ChannelCPrevImageUnAligned);
                //                        imageWidth = _ChannelCPrevImageUnAligned.PixelWidth;
                //                        imageHeight = _ChannelCPrevImageUnAligned.PixelHeight;
                //                    }
                //                }
                //                else
                //                {
                //                    if (_ChannelCPrevImage != null)
                //                        ImageProcessing.FrameToBitmap(apdChannelC, ref _ChannelCPrevImage);
                //                }
                //                _SelectedChannels.Add(new LaserTypeColorChannel(LaserType.LaserC, _LaserCColorChannel));
                //            }
                //        }
                //        else if (channel == LaserType.LaserD)   // 488
                //        {
                //            if (apdChannelD != null && (_ChannelDPrevImage != null || _ChannelDPrevImageUnAligned != null))
                //            {
                //                if (_IsCropPreviewImage)
                //                {
                //                    if (_ChannelDPrevImageUnAligned != null)
                //                    {
                //                        ImageProcessing.FrameToBitmap(apdChannelD, ref _ChannelDPrevImageUnAligned);
                //                        imageWidth = _ChannelDPrevImageUnAligned.PixelWidth;
                //                        imageHeight = _ChannelDPrevImageUnAligned.PixelHeight;
                //                    }
                //                }
                //                else
                //                {
                //                    if (_ChannelDPrevImage != null)
                //                        ImageProcessing.FrameToBitmap(apdChannelD, ref _ChannelDPrevImage);
                //                }
                //                _SelectedChannels.Add(new LaserTypeColorChannel(LaserType.LaserD, _LaserDColorChannel));
                //            }
                //        }
                //    }
                //}

                if (_ImagingVm.IsLaserAPrvSelected)
                {
                    // 784
                    if (apdChannelA != null && (_ChannelAPrevImage != null || _ChannelAPrevImageUnAligned != null))
                    {
                        if (_IsCropPreviewImage)
                        {
                            if (_ChannelAPrevImageUnAligned != null)
                            {
                                ImageProcessing.FrameToBitmap(apdChannelA, ref _ChannelAPrevImageUnAligned);
                                imageWidth = _ChannelAPrevImageUnAligned.PixelWidth;
                                imageHeight = _ChannelAPrevImageUnAligned.PixelHeight;
                            }
                        }
                        else
                        {
                            if (_ChannelAPrevImage != null)
                                ImageProcessing.FrameToBitmap(apdChannelA, ref _ChannelAPrevImage);
                        }
                    }
                }
                if (_ImagingVm.IsLaserBPrvSelected)  // 520
                {
                    if (apdChannelB != null && (_ChannelBPrevImage != null || _ChannelBPrevImageUnAligned != null))
                    {
                        if (_IsCropPreviewImage)
                        {
                            if (_ChannelBPrevImageUnAligned != null)
                            {
                                ImageProcessing.FrameToBitmap(apdChannelB, ref _ChannelBPrevImageUnAligned);
                                imageWidth = _ChannelBPrevImageUnAligned.PixelWidth;
                                imageHeight = _ChannelBPrevImageUnAligned.PixelHeight;
                            }
                        }
                        else
                        {
                            if (_ChannelBPrevImage != null)
                                ImageProcessing.FrameToBitmap(apdChannelB, ref _ChannelBPrevImage);
                        }
                    }
                }
                if (_ImagingVm.IsLaserCPrvSelected)   // 658
                {
                    if (apdChannelC != null && (_ChannelCPrevImage != null || _ChannelCPrevImageUnAligned != null))
                    {
                        if (_IsCropPreviewImage)
                        {
                            if (_ChannelCPrevImageUnAligned != null)
                            {
                                ImageProcessing.FrameToBitmap(apdChannelC, ref _ChannelCPrevImageUnAligned);
                                imageWidth = _ChannelCPrevImageUnAligned.PixelWidth;
                                imageHeight = _ChannelCPrevImageUnAligned.PixelHeight;
                            }
                        }
                        else
                        {
                            if (_ChannelCPrevImage != null)
                                ImageProcessing.FrameToBitmap(apdChannelC, ref _ChannelCPrevImage);
                        }
                    }
                }
                if (_ImagingVm.IsLaserDPrvSelected)   // 488
                {
                    if (apdChannelD != null && (_ChannelDPrevImage != null || _ChannelDPrevImageUnAligned != null))
                    {
                        if (_IsCropPreviewImage)
                        {
                            if (_ChannelDPrevImageUnAligned != null)
                            {
                                ImageProcessing.FrameToBitmap(apdChannelD, ref _ChannelDPrevImageUnAligned);
                                imageWidth = _ChannelDPrevImageUnAligned.PixelWidth;
                                imageHeight = _ChannelDPrevImageUnAligned.PixelHeight;
                            }
                        }
                        else
                        {
                            if (_ChannelDPrevImage != null)
                                ImageProcessing.FrameToBitmap(apdChannelD, ref _ChannelDPrevImage);
                        }
                    }
                }

                if (_IsCropPreviewImage)
                {
                    if (_IsAligningPreviewImage) return;

                    unsafe
                    {
                        // Align the images using laser C (red) channel as reference.
                        //
                        byte*[] imagesUnaligned = new byte*[4];
                        imagesUnaligned[0] = (_ChannelAPrevImageUnAligned != null) ? (byte*)_ChannelAPrevImageUnAligned.BackBuffer.ToPointer() : null;
                        imagesUnaligned[1] = (_ChannelBPrevImageUnAligned != null) ? (byte*)_ChannelBPrevImageUnAligned.BackBuffer.ToPointer() : null;
                        imagesUnaligned[2] = (_ChannelCPrevImageUnAligned != null) ? (byte*)_ChannelCPrevImageUnAligned.BackBuffer.ToPointer() : null;
                        imagesUnaligned[3] = (_ChannelDPrevImageUnAligned != null) ? (byte*)_ChannelDPrevImageUnAligned.BackBuffer.ToPointer() : null;
                        byte*[] imagesAligned = new byte*[4];
                        imagesAligned[0] = (_ChannelAPrevImage != null) ? (byte*)_ChannelAPrevImage.BackBuffer.ToPointer() : null;
                        imagesAligned[1] = (_ChannelBPrevImage != null) ? (byte*)_ChannelBPrevImage.BackBuffer.ToPointer() : null;
                        imagesAligned[2] = (_ChannelCPrevImage != null) ? (byte*)_ChannelCPrevImage.BackBuffer.ToPointer() : null;
                        imagesAligned[3] = (_ChannelDPrevImage != null) ? (byte*)_ChannelDPrevImage.BackBuffer.ToPointer() : null;

                        System.Threading.Tasks.Task taskAlign = System.Threading.Tasks.Task.Factory.StartNew(() =>
                        {
                            _IsAligningPreviewImage = true;
                            ImagingHelper.AlignImage(imagesUnaligned, imagesAligned, _DeltaXAlignOffset, _DeltaYAlignOffset, imageWidth, imageHeight);
                            _IsAligningPreviewImage = false;
                        });
                    }
                }

                //this._ContrastVm.SelectedChannels = _SelectedChannels;
                //UpdatePreviewDisplayImage(_ContrastVm.SelectedChannels, _ContrastVm.DisplayImageInfo);
                UpdatePreviewDisplayImage();
            }));
        }*/

        private void ImageScanCommand_OnScanDataReceived(object sender, string dataName)
        {
            //switch (dataName)
            //{
            //    case "RemainingTime":
            //        RemainingTime = scanCommand.RemainingTime;
            //        break;
            //    //case "OnXRemainingTime":
            //    //    TdOnXRemaining = new Thread(OnXRemainingTime);
            //    //    TdOnXRemaining.IsBackground = true;
            //    //    TdOnXRemaining.Start();
            //    //    break;
            //}
            LaserScanCommand laserScanCommand = sender as LaserScanCommand;
            Application.Current.Dispatcher.BeginInvoke((Action)delegate
            {
                if (dataName == "ScanningPrepStarted" || dataName == "ScanningPrepCompleted")
                {
                    if (dataName == "ScanningPrepStarted")
                    {
                        Workspace.This.IsPreparingToScan = true;
                    }
                    else if (dataName == "ScanningPrepCompleted")
                    {
                        Workspace.This.IsPreparingToScan = false;
                    }
                }
                else if (dataName == "LIDOpened")
                {
                    if (!_IsAbortedOnLidOpened)
                    {
                        //System.Diagnostics.Trace.WriteLine("LID is opened");
                        _IsAbortedOnLidOpened = true;
                        ExecuteStopScanCommand("LIDOpened");
                        return;
                    }
                }

                if (_CurrentScanType == ScanType.Auto && laserScanCommand.IsSmartScanCalc || laserScanCommand.IsEdrScanCalc)
                {
                    //Don't display the time remaining countdown while calculating the signal level.
                    //Workspace.This.StatusTextProgress = "SMARTSCAN in progress....";
                }
                else
                {
                    if (dataName == "RemainingTime")
                    {
                        //Application.Current.Dispatcher.BeginInvoke((Action)delegate
                        //{
                        //    if (_CurrentScanType != ScanType.Auto || (_CurrentScanType == ScanType.Auto && _IsAutoScanCompleted))
                        //    {
                        //        RemainingTime = scanCommand.RemainingTime;
                        //        double timeElapsed = Time - RemainingTime;
                        //        Workspace.This.StatusTextProgress = ImagingSystemHelper.FormatTime(RemainingTime);
                        //        double percentCompleted = (timeElapsed / (double)Time) * 100.0;
                        //        Workspace.This.PercentCompleted = (int)percentCompleted;
                        //    }
                        //});
                        //Workspace.This.EstimatedCaptureRemainingTime = scanCommand.RemainingTime;

                        RemainingTime = _ImageScanCommand.RemainingTime;
                        double timeElapsed = Time - RemainingTime;
                        Workspace.This.StatusTextProgress = ImagingSystemHelper.FormatTime(RemainingTime);
                        double percentCompleted = (timeElapsed / (double)Time) * 100.0;
                        Workspace.This.PercentCompleted = (int)percentCompleted;
                    }
                }
            });
        }

        private void ImageScanCommand_DataReceived(object sender)
        {
            if (!_IsPreviewSetupCompleted || _IsAligningPreviewImage || _IsUpdatingPreviewImage) { return; }

            var scanCommand = sender as LaserScanCommand;
            if (scanCommand != null)
            {
                if (scanCommand.IsScanAborted) { return; }
            }

            if (_ImagingVm.PreviewChannels == null || _ImagingVm.NumOfDisplayChannels == 0)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (_PreviewImage != null && !_IsPreviewImageCleared)
                    {
                        try
                        {
                            // Clear the preview image if no display channel is selected.
                            _PreviewImage.Lock();
                            ImageProcessingHelper.FastClear(ref _PreviewImage);
                            // Specify the area of the bitmap that changed.
                            _PreviewImage.AddDirtyRect(new Int32Rect(0, 0, _PreviewImage.PixelWidth, _PreviewImage.PixelHeight));
                            _PreviewImage.Unlock();
                            _IsPreviewImageCleared = true;
                        }
                        catch
                        {
                        }
                    }
                }));
                return;
            }
            else
            {
                _IsPreviewImageCleared = false;
            }

            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    #region Convert ushort to WriteableBitmap

                    /*if (_ImagingVm.IsLaserL1PrvSelected)
                    {
                        if (apdChannelC != null && _ChannelL1PrevImageUnAligned != null)
                        {
                            if (_IsCropPreviewImage)
                            {
                                ImageProcessing.FrameToBitmap(apdChannelC, ref _ChannelL1PrevImageUnAligned);
                            }
                            else
                            {
                                ImageProcessing.FrameToBitmap(apdChannelC, ref _ChannelL1PrevImageUnAligned);
                                _ChannelL1PrevImage = _ChannelL1PrevImageUnAligned;
                            }
                        }
                    }
                    if (_ImagingVm.IsLaserR1PrvSelected)
                    {
                        if (apdChannelA != null && _ChannelR1PrevImageUnAligned != null)
                        {
                            if (_IsCropPreviewImage)
                            {
                                ImageProcessing.FrameToBitmap(apdChannelA, ref _ChannelR1PrevImageUnAligned);
                            }
                            else
                            {
                                ImageProcessing.FrameToBitmap(apdChannelA, ref _ChannelR1PrevImageUnAligned);
                                _ChannelR1PrevImage = _ChannelR1PrevImageUnAligned;
                            }
                        }
                    }
                    if (_ImagingVm.IsLaserR2PrvSelected)
                    {
                        if (apdChannelB != null && _ChannelR2PrevImageUnAligned != null)
                        {
                            if (_IsCropPreviewImage)
                            {
                                ImageProcessing.FrameToBitmap(apdChannelB, ref _ChannelR2PrevImageUnAligned);
                            }
                            else
                            {
                                ImageProcessing.FrameToBitmap(apdChannelB, ref _ChannelR2PrevImageUnAligned);
                                _ChannelR2PrevImage = _ChannelR2PrevImageUnAligned;
                            }
                        }
                    }*/

                    #endregion

                    if (_IsCropPreviewImage)
                    {
                        if (_IsAligningPreviewImage) return;

                        unsafe
                        {
                            #region Image Alignment Setup

                            byte* psrcimgL1 = null;
                            byte* psrcimgR1 = null;
                            byte* psrcimgR2 = null;
                            byte* pdstimgL1 = null;
                            byte* pdstimgR1 = null;
                            byte* pdstimgR2 = null;
                            byte* psrcimgTemp = null;
                            byte* pdstimgTemp = null;
                            int srcwidth = 0;
                            int srcheight = 0;
                            int srcstride = 0;
                            int dstwidth = 0;
                            int dstheight = 0;
                            int dststride = 0;
                            PixelFormat format = PixelFormats.Gray16;

                            if (_ImagingVm.IsLaserL1PrvSelected && _ChannelL1PrevImageUnAligned != null)
                            {
                                srcwidth = _ChannelL1PrevImageUnAligned.PixelWidth;
                                srcheight = _ChannelL1PrevImageUnAligned.PixelHeight;
                                srcstride = _ChannelL1PrevImageUnAligned.BackBufferStride;
                                dstwidth = _ChannelL1PrevImage.PixelWidth;
                                dstheight = _ChannelL1PrevImage.PixelHeight;
                                dststride = _ChannelL1PrevImage.BackBufferStride;
                                psrcimgL1 = (byte*)_ChannelL1PrevImageUnAligned.BackBuffer.ToPointer();
                                pdstimgL1 = (byte*)_ChannelL1PrevImage.BackBuffer.ToPointer();
                                format = _ChannelL1PrevImageUnAligned.Format;
                            }
                            if (_ImagingVm.IsLaserR1PrvSelected && _ChannelR1PrevImageUnAligned != null)
                            {
                                srcwidth = _ChannelR1PrevImageUnAligned.PixelWidth;
                                srcheight = _ChannelR1PrevImageUnAligned.PixelHeight;
                                srcstride = _ChannelR1PrevImageUnAligned.BackBufferStride;
                                dstwidth = _ChannelR1PrevImage.PixelWidth;
                                dstheight = _ChannelR1PrevImage.PixelHeight;
                                dststride = _ChannelR1PrevImage.BackBufferStride;
                                psrcimgR1 = (byte*)_ChannelR1PrevImageUnAligned.BackBuffer.ToPointer();
                                pdstimgR1 = (byte*)_ChannelR1PrevImage.BackBuffer.ToPointer();
                                format = _ChannelR1PrevImageUnAligned.Format;
                            }
                            if (_ImagingVm.IsLaserR2PrvSelected && _ChannelR2PrevImageUnAligned != null)
                            {
                                srcwidth = _ChannelR2PrevImageUnAligned.PixelWidth;
                                srcheight = _ChannelR2PrevImageUnAligned.PixelHeight;
                                srcstride = _ChannelR2PrevImageUnAligned.BackBufferStride;
                                dstwidth = _ChannelR2PrevImage.PixelWidth;
                                dstheight = _ChannelR2PrevImage.PixelHeight;
                                dststride = _ChannelR2PrevImage.BackBufferStride;
                                psrcimgR2 = (byte*)_ChannelR2PrevImageUnAligned.BackBuffer.ToPointer();
                                pdstimgR2 = (byte*)_ChannelR2PrevImage.BackBuffer.ToPointer();
                                format = _ChannelR2PrevImageUnAligned.Format;
                            }

                            #endregion

                            #region Image Alignment

                            System.Threading.Tasks.Task taskAlign = System.Threading.Tasks.Task.Factory.StartNew(() =>
                            {
                                if (_ImageScanCommand != null)
                                {
                                    if (!_IsAligningPreviewImage)
                                    {
                                        _IsAligningPreviewImage = true;

                                        if (_ImagingVm.IsLaserL1PrvSelected && psrcimgL1 != null && pdstimgL1 != null)
                                        {
                                            if (_ImageScanCommand != null)
                                            {
                                                var alignParam = _ImageScanCommand.CurrentScanParam.AlignmentParam;
                                                alignParam.LaserChannel = LaserChannels.ChannelC;
                                                try
                                                {
                                                    ImagingHelper.SFLImageAlign(psrcimgL1, srcwidth, srcheight, srcstride,
                                                                                pdstimgL1, dstwidth, dstheight, dststride,
                                                                                format, alignParam);
                                                 }
                                                catch (Exception ex)
                                                {
                                                    _IsAligningPreviewImage = false;
                                                    Workspace.This.LogException("ImageScanCommand_DataReceived: AlignImage(): L1.", ex);
                                                    return;
                                                }
                                            }
                                        }
                                        if (_ImagingVm.IsLaserR1PrvSelected && psrcimgR1 != null && pdstimgR1 != null)
                                        {
                                            if (_ImageScanCommand != null)
                                            {
                                                var alignParam = _ImageScanCommand.CurrentScanParam.AlignmentParam;
                                                alignParam.LaserChannel = LaserChannels.ChannelA;
                                                try
                                                {
                                                    ImagingHelper.SFLImageAlign(psrcimgR1, srcwidth, srcheight, srcstride,
                                                                                pdstimgR1, dstwidth, dstheight, dststride,
                                                                                format, alignParam);
                                                }
                                                catch (Exception ex)
                                                {
                                                    _IsAligningPreviewImage = false;
                                                    Workspace.This.LogException("ImageScanCommand_DataReceived: AlignImage(): R1.", ex);
                                                    return;
                                                }
                                            }
                                        }
                                        if (_ImagingVm.IsLaserR2PrvSelected && psrcimgR2 != null && pdstimgR2 != null)
                                        {
                                            if (_ImageScanCommand != null)
                                            {
                                                var alignParam = _ImageScanCommand.CurrentScanParam.AlignmentParam;
                                                alignParam.LaserChannel = LaserChannels.ChannelB;
                                                try
                                                {
                                                    ImagingHelper.SFLImageAlign(psrcimgR2, srcwidth, srcheight, srcstride,
                                                                                pdstimgR2, dstwidth, dstheight, dststride,
                                                                                format, alignParam);
                                                }
                                                catch (Exception ex)
                                                {
                                                    _IsAligningPreviewImage = false;
                                                    Workspace.This.LogException("ImageScanCommand_DataReceived: AlignImage(): R2.", ex);
                                                    return;
                                                }
                                            }
                                        }
                                        _IsAligningPreviewImage = false;
                                    }
                                }
                            });

                            #endregion
                        }
                    }

                    if (!_IsAligningPreviewImage)
                    {
                        if (_SelectedAppProtocol.SelectedScanRegion.IsEdrScanning)
                        {
                            _ImagingVm.ContrastVm.DisplayImageInfo.IsSaturationChecked = false;
                        }
                        UpdatePreviewDisplayImage();
                    }
                }
                catch (Exception ex)
                {
                    _IsAligningPreviewImage = false;
                    _IsUpdatingPreviewImage = false;
                    Workspace.This.LogException("ImageScanCommand_DataReceived().", ex);
                    //_ImagingVm.PreviewImage = null;     //Clear prievew image
                    _ContrastThreadEvent.Set();
                }
            }));
        }

        private void ImageScanCommand_ScanRegionCompleted(ThreadBase sender, ImageInfo imageInfo, int nScanRegion)
        {
            LaserScanCommand scanningCommand = sender as LaserScanCommand;
            
            if (scanningCommand == null) { return; }

            var currScanParam = scanningCommand.ScanParams[nScanRegion];
            bool bIsZScanning = currScanParam.IsZScanning;
            bool bIsAutoMergeImages = _SelectedAppProtocol.ScanRegions[nScanRegion].IsAutoMergeImages;

            if (bIsZScanning)
            {
                // Z-Scan multi scan regions: just switch the scan region
                //   the scan region image(s) is saved in 'ImageScanCommand_ZScanningCompleted'

                // Select the next scan region and setup the next scan region's preview images.
                if (nScanRegion + 1 < scanningCommand.ScanParams.Count)
                {
                    Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
                    {
                        nScanRegion++;
                        // Set up the next scan region preview image settings
                        PreviewImageSetup(scanningCommand.ScanParams[nScanRegion], nScanRegion);
                        // Switch scan region
                        _SelectedAppProtocol.SelectedScanRegion = _SelectedAppProtocol.ScanRegions[nScanRegion];
                    });
                }

                return;
            }

            if (_CurrentScanType != ScanType.Preview)
            {
                Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
                {
                    //L1 = ChannelC
                    //R1 = ChannelA
                    //R2 = ChannelB
                    WriteableBitmap laserL1scannedImage = null;
                    WriteableBitmap laserR1scannedImage = null;
                    WriteableBitmap laserR2scannedImage = null;
                    WriteableBitmap laserL1RawImage = null;
                    WriteableBitmap laserR1RawImage = null;
                    WriteableBitmap laserR2RawImage = null;

                    var signals = scanningCommand.CurrentScanParam.Signals;

                    for (int i = 0; i < signals.Count; i++)
                    {
                        if (signals[i].LaserChannel == LaserChannels.ChannelC)      //L1
                        {
                            if (scanningCommand.ChannelCImage != null)
                            {
                                laserL1scannedImage = scanningCommand.ChannelCImage.Clone();
                                if (SettingsManager.ConfigSettings.IsKeepRawImages)
                                {
                                    laserL1RawImage = scanningCommand.ChannelCRawImage.Clone();
                                }
                            }
                        }
                        else if (signals[i].LaserChannel == LaserChannels.ChannelA) //R1
                        {
                            if (scanningCommand.ChannelAImage != null)
                            {
                                laserR1scannedImage = scanningCommand.ChannelAImage.Clone();
                                if (SettingsManager.ConfigSettings.IsKeepRawImages)
                                {
                                    laserR1RawImage = scanningCommand.ChannelARawImage.Clone();
                                }
                            }
                        }
                        else if (signals[i].LaserChannel == LaserChannels.ChannelB) //R2
                        {
                            if (scanningCommand.ChannelBImage != null)
                            {
                                laserR2scannedImage = scanningCommand.ChannelBImage.Clone();
                                if (SettingsManager.ConfigSettings.IsKeepRawImages)
                                {
                                    laserR2RawImage = scanningCommand.ChannelBRawImage.Clone();
                                }
                            }
                        }
                    }

                    if (imageInfo != null)
                    {
                        imageInfo.CaptureType = "Fluorescence";
                        imageInfo.SoftwareVersion = Workspace.This.ProductVersion;
                        imageInfo.FWVersion = Workspace.This.FWVersion;
                        imageInfo.SystemSN = Workspace.This.SystemSN;
                        imageInfo.Software = "Sapphire FL";
                        string smartScan = (currScanParam.IsSmartScanning) ? "Smartscan" : string.Empty;
                        string seqScan = (currScanParam.IsSequentialScanning) ? "Sequential" : string.Empty;
                        string zScan = (bIsZScanning) ? "Z-Scan" : string.Empty;
                        if (currScanParam.IsEdrScanning)
                        {
                            smartScan = "EDR";
                        }
                        string separator1 = (currScanParam.IsSmartScanning && currScanParam.IsSequentialScanning) ? " + " : string.Empty;
                        string separator2 = ((currScanParam.IsSmartScanning || currScanParam.IsSequentialScanning) && currScanParam.IsZScanning) ? " + " : string.Empty;
                        imageInfo.ScanType = string.Format("{0}{1}{2}{3}{4}", smartScan, separator1, seqScan, separator2, zScan);
                        imageInfo.ScanZ0Abs = Workspace.This.MotorVM.AbsZPos;
                        int x1 = (int)Math.Round(_SelectedAppProtocol.ScanRegions[nScanRegion].ScanRect.X / _ImagingVm.CellSize);
                        int y1 = (int)Math.Round(_SelectedAppProtocol.ScanRegions[nScanRegion].ScanRect.Y / _ImagingVm.CellSize);
                        int x2 = (int)Math.Round(x1 + (_SelectedAppProtocol.ScanRegions[nScanRegion].ScanRect.Width / _ImagingVm.CellSize));
                        int y2 = (int)Math.Round(y1 + (_SelectedAppProtocol.ScanRegions[nScanRegion].ScanRect.Height / _ImagingVm.CellSize));
                        string row = string.Format("{0}-{1}", Workspace.This.IndexToRow(y1), Workspace.This.IndexToRow(y2));
                        string col = string.Format("{0}-{1}", x1, x2);
                        imageInfo.ScanRegion = row + ", " + col;
                        // User selected signal level
                        if (imageInfo.RedChannel.LaserIntensity > 0) { imageInfo.RedChannel.SignalLevel = Workspace.IntensityLevels[imageInfo.RedChannel.LaserIntensityLevel]; }
                        if (imageInfo.GreenChannel.LaserIntensity > 0) { imageInfo.GreenChannel.SignalLevel = Workspace.IntensityLevels[imageInfo.GreenChannel.LaserIntensityLevel]; }
                        if (imageInfo.BlueChannel.LaserIntensity > 0) { imageInfo.BlueChannel.SignalLevel = Workspace.IntensityLevels[imageInfo.BlueChannel.LaserIntensityLevel]; }
                        if (imageInfo.GrayChannel.LaserIntensity > 0) { imageInfo.GrayChannel.SignalLevel = Workspace.IntensityLevels[imageInfo.GrayChannel.LaserIntensityLevel]; }
                        if (imageInfo.MixChannel.LaserIntensity > 0) { imageInfo.MixChannel.SignalLevel = Workspace.IntensityLevels[imageInfo.MixChannel.LaserIntensityLevel]; }

                        #region === Filter wavelength ===
                        if (imageInfo.RedChannel.LaserIntensity > 0)
                        {
                            int wavelength = 0;
                            if (imageInfo.RedChannel.LaserWavelength == "0")
                            {
                                var laserChannel = _LaserChannelDict[imageInfo.RedChannel.LaserChannel];
                                wavelength = Workspace.This.LaserChannelTypeList[laserChannel];
                                imageInfo.RedChannel.LaserWavelength = wavelength.ToString();
                            }
                            else
                            {
                                wavelength = int.Parse(imageInfo.RedChannel.LaserWavelength);
                            }
                            foreach (var laserType in Workspace.This.LaserOptions)
                            {
                                if (!Workspace.This.IsPhosphorModule(laserType))
                                {
                                    if (laserType.Wavelength == wavelength)
                                    {
                                        imageInfo.RedChannel.FilterWavelength = laserType.Filter;
                                    }
                                }
                            }
                            // Can sometimes have multiple lasers with the save wavelength (638/APD and 638/PMT)
                            //var laserType = Workspace.This.LaserOptions.SingleOrDefault(item => item.Wavelength == int.Parse(imageInfo.RedChannel.LaserWavelength));
                            //imageInfo.RedChannel.FilterWavelength = laserType.Filter;
                        }
                        if (imageInfo.GreenChannel.LaserIntensity > 0)
                        {
                            int wavelength = 0;
                            if (imageInfo.GreenChannel.LaserWavelength == "0")
                            {
                                var laserChannel = _LaserChannelDict[imageInfo.GreenChannel.LaserChannel];
                                wavelength = Workspace.This.LaserChannelTypeList[laserChannel];
                                imageInfo.GreenChannel.LaserWavelength = wavelength.ToString();
                            }
                            else
                            {
                                wavelength = int.Parse(imageInfo.GreenChannel.LaserWavelength);
                            }
                            foreach (var laserType in Workspace.This.LaserOptions)
                            {
                                if (!Workspace.This.IsPhosphorModule(laserType))
                                {
                                    if (laserType.Wavelength == wavelength)
                                    {
                                        imageInfo.GreenChannel.FilterWavelength = laserType.Filter;
                                    }
                                }
                            }
                            // Can sometimes have multiple lasers with the save wavelength (638/APD and 638/PMT)
                            //var laserType = Workspace.This.LaserOptions.SingleOrDefault(item => item.Wavelength == int.Parse(imageInfo.GreenChannel.LaserWavelength));
                            //imageInfo.GreenChannel.FilterWavelength = laserType.Filter;
                        }
                        if (imageInfo.BlueChannel.LaserIntensity > 0)
                        {
                            int wavelength = 0;
                            if (imageInfo.BlueChannel.LaserWavelength == "0")
                            {
                                var laserChannel = _LaserChannelDict[imageInfo.BlueChannel.LaserChannel];
                                wavelength = Workspace.This.LaserChannelTypeList[laserChannel];
                                imageInfo.BlueChannel.LaserWavelength = wavelength.ToString();
                            }
                            else
                            {
                                wavelength = int.Parse(imageInfo.BlueChannel.LaserWavelength);
                            }
                            foreach (var laserType in Workspace.This.LaserOptions)
                            {
                                if (!Workspace.This.IsPhosphorModule(laserType))
                                {
                                    if (laserType.Wavelength == wavelength)
                                    {
                                        imageInfo.BlueChannel.FilterWavelength = laserType.Filter;
                                    }
                                }
                            }
                            // Can sometimes have multiple lasers with the save wavelength (638/APD and 638/PMT)
                            //var laserType = Workspace.This.LaserOptions.SingleOrDefault(item => item.Wavelength == int.Parse(imageInfo.BlueChannel.LaserWavelength));
                            //imageInfo.BlueChannel.FilterWavelength = laserType.Filter;
                        }
                        if (imageInfo.GrayChannel.LaserIntensity > 0)
                        {
                            int wavelength = 0;
                            if (imageInfo.GrayChannel.LaserWavelength == "0")
                            {
                                var laserChannel = _LaserChannelDict[imageInfo.GrayChannel.LaserChannel];
                                wavelength = Workspace.This.LaserChannelTypeList[laserChannel];
                                imageInfo.GrayChannel.LaserWavelength = wavelength.ToString();
                            }
                            else
                            {
                                wavelength = int.Parse(imageInfo.GrayChannel.LaserWavelength);
                            }
                            foreach (var laserType in Workspace.This.LaserOptions)
                            {
                                if (!Workspace.This.IsPhosphorModule(laserType))
                                {
                                    if (laserType.Wavelength == wavelength)
                                    {
                                        imageInfo.GrayChannel.FilterWavelength = laserType.Filter;
                                    }
                                }
                            }
                            // Can sometimes have multiple lasers with the save wavelength (638/APD and 638/PMT)
                            //var laserType = Workspace.This.LaserOptions.SingleOrDefault(item => item.Wavelength == int.Parse(imageInfo.GrayChannel.LaserWavelength));
                            //imageInfo.GrayChannel.FilterWavelength = laserType.Filter;
                        }
                        if (imageInfo.MixChannel.LaserIntensity > 0)
                        {
                            int wavelength = 0;
                            if (imageInfo.MixChannel.LaserWavelength == "0")
                            {
                                var laserChannel = _LaserChannelDict[imageInfo.MixChannel.LaserChannel];
                                wavelength = Workspace.This.LaserChannelTypeList[laserChannel];
                                imageInfo.MixChannel.LaserWavelength = wavelength.ToString();
                            }
                            else
                            {
                                wavelength = int.Parse(imageInfo.MixChannel.LaserWavelength);
                            }
                            foreach (var laserType in Workspace.This.LaserOptions)
                            {
                                if (!Workspace.This.IsPhosphorModule(laserType))
                                {
                                    if (laserType.Wavelength == wavelength)
                                    {
                                        imageInfo.MixChannel.FilterWavelength = laserType.Filter;
                                    }
                                }
                            }
                            // Can sometimes have multiple lasers with the save wavelength (638/APD and 638/PMT)
                            //var laserType = Workspace.This.LaserOptions.SingleOrDefault(item => item.Wavelength == int.Parse(imageInfo.MixChannel.LaserWavelength));
                            //imageInfo.MixChannel.FilterWavelength = laserType.Filter;
                        }
                        #endregion

                        // Get scan region
                        //if (imageInfo.ScanX0 > 0)
                        //{
                        //    imageInfo.ScanX0 = (int)((imageInfo.ScanX0 - Workspace.This.ApdVM.APDTransfer.DeviceProperties.XHome) / (double)_XMotorSubdivision);
                        //}
                        //if (imageInfo.ScanY0 > 0)
                        //{
                        //    imageInfo.ScanY0 = (int)((imageInfo.ScanY0 - Workspace.This.ApdVM.APDTransfer.DeviceProperties.YHome) / (double)_YMotorSubdivision);
                        //}
                        if (_SelectedAppProtocol.ScanRegions[nScanRegion].IsCustomFocus)
                        {
                            string symbol = (_SelectedAppProtocol.ScanRegions[nScanRegion].CustomFocusValue > 0) ? "+" : string.Empty;
                            imageInfo.SampleType = string.Format("{0} ({1}{2})",
                                _SelectedAppProtocol.ScanRegions[nScanRegion].SelectedSampleType.DisplayName,
                                symbol,
                                _SelectedAppProtocol.ScanRegions[nScanRegion].CustomFocusValue.ToString("0.00"));
                        }
                        else
                        {
                            imageInfo.SampleType = _SelectedAppProtocol.ScanRegions[nScanRegion].SelectedSampleType.DisplayName;
                        }
                        imageInfo.ScanSpeed = _SelectedAppProtocol.ScanRegions[nScanRegion].SelectedScanSpeed.DisplayName;
                        imageInfo.ScanQuality = _SelectedAppProtocol.ScanRegions[nScanRegion].SelectedScanQuality.DisplayName;
                        imageInfo.Comment = _SelectedAppProtocol.ScanRegions[nScanRegion].FileLocationVm.Notes;
                    }

                    try
                    {
                        //Add image to Gallery
                        //

                        string docTitle = string.Empty;
                        string fileNameWithoutExt = string.Empty;
                        string fileExtension = ".tif";  // default file extension
                        string destinationFolder = _SelectedAppProtocol.ScanRegions[nScanRegion].FileLocationVm.DestinationFolder;
                        string fileFullPath = string.Empty;
                        bool bIsSaveAsPUB = false;

                        string tmpFileName = string.Empty;
                        if (_SelectedAppProtocol.ScanRegions[nScanRegion].FileLocationVm.IsAutoSave)
                        {
                            bIsSaveAsPUB = SettingsManager.ApplicationSettings.IsAutoSavePubFileEnabled;

                            if (string.IsNullOrEmpty(_SelectedAppProtocol.ScanRegions[nScanRegion].FileLocationVm.FileName.Trim()))
                            {
                                // file name not specified, generate a new file name
                                tmpFileName = Workspace.This.GenerateFileName(string.Empty, string.Empty);
                            }
                            else
                            {
                                var flFileName = _SelectedAppProtocol.ScanRegions[nScanRegion].FileLocationVm.FileName.Trim();
                                if (Workspace.This.CheckSupportedFileType(flFileName))
                                    tmpFileName = System.IO.Path.GetFileNameWithoutExtension(flFileName);
                                else
                                    tmpFileName = flFileName;
                            }
                        }
                        else
                        {
                            if (_ScanImageBaseFilename == null || _ScanImageBaseFilename.Count == 0)
                            {
                                tmpFileName = Workspace.This.GenerateFileName(string.Empty, string.Empty);
                            }
                            else
                            {
                                // file name not specified, generate a new file name
                                if (string.IsNullOrEmpty(_ScanImageBaseFilename[scanningCommand.ZSequentialSetIndex]))
                                {
                                    tmpFileName = Workspace.This.GenerateFileName(string.Empty, string.Empty);
                                }
                                else
                                {
                                    tmpFileName = _ScanImageBaseFilename[scanningCommand.ZSequentialSetIndex];
                                }
                            }
                        }

                        //string signOfNumber = (Workspace.This.AbsFocusPosition > imageInfo.ScanZ0) ? "+" : "";
                        //fileNameWithoutExt = string.Format("{0}_({1}{2})", tmpFileName, signOfNumber,
                        //    (Workspace.This.AbsFocusPosition - imageInfo.ScanZ0).ToString("F2", System.Globalization.CultureInfo.InvariantCulture));

                        //string strScanRegion = (_SelectedAppProtocol.ScanRegions.Count > 1) ? string.Format(" (SR{0})", nScanRegion + 1) : string.Empty;
                        //fileNameWithoutExt = string.Format("{0}{1}", tmpFileName, strScanRegion);
                        fileNameWithoutExt = tmpFileName;

                        WriteableBitmap[] scannedImages;
                        if (_Is4channelImage)
                        {
                            scannedImages = new WriteableBitmap[] { null, null, null, null };
                        }
                        else
                        {
                            scannedImages = new WriteableBitmap[] { null, null, null };
                        }

                        int nLaserSignalLevel = 0;

                        for (int i = 0; i < _SelectedAppProtocol.ScanRegions[nScanRegion].SignalCount; i++)
                        {
                            if (_SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].SelectedLaser.LaserChannel == LaserChannels.ChannelC)   //L1
                            {
                                nLaserSignalLevel = _SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].SelectedSignalLevel.IntensityLevel;

                                if (_IsGrayscaleImage)
                                {
                                    scannedImages[0] = laserL1scannedImage;
                                }
                                else
                                {
                                    SetImageChannel(ref scannedImages,
                                                    ref laserL1scannedImage,
                                                    _SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].SelectedColorChannel.ImageColorChannel);
                                    if (!bIsAutoMergeImages)
                                    {
                                        docTitle = string.Format("{0}_{1}{2}", fileNameWithoutExt, Workspace.This.LaserChannelTypeList[LaserChannels.ChannelC], fileExtension);
                                        if (_SelectedAppProtocol.ScanRegions[nScanRegion].FileLocationVm.IsAutoSave)
                                        {
                                            fileFullPath = System.IO.Path.Combine(destinationFolder, docTitle);
                                            var clonedImage = laserL1scannedImage.Clone();
                                            clonedImage.Freeze();
                                            Workspace.This.SaveAsync(clonedImage, imageInfo, fileFullPath, false);
                                        }
                                        else
                                        {
                                            fileFullPath = string.Empty;
                                        }
                                        Workspace.This.NewDocument(laserL1scannedImage, imageInfo, docTitle, fileFullPath, bIsSaveAsPUB, true);
                                    }
                                }
                                // Keep raw image
                                if (SettingsManager.ConfigSettings.IsKeepRawImages && laserL1RawImage != null)
                                {
                                    string rawDocTitle = string.Format("{0}_{1}_RAW{2}", fileNameWithoutExt, Workspace.This.LaserChannelTypeList[LaserChannels.ChannelC], fileExtension);
                                    Workspace.This.NewDocument(laserL1RawImage, imageInfo, rawDocTitle, fileFullPath, false, true);
                                }
                            }
                            if (_SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].SelectedLaser.LaserChannel == LaserChannels.ChannelA)   //R1
                            {
                                nLaserSignalLevel = _SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].SelectedSignalLevel.IntensityLevel;
                                if (_IsGrayscaleImage)
                                {
                                    scannedImages[0] = laserR1scannedImage;
                                }
                                else
                                {
                                    SetImageChannel(ref scannedImages,
                                                    ref laserR1scannedImage,
                                                    _SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].SelectedColorChannel.ImageColorChannel);
                                    if (!bIsAutoMergeImages)
                                    {
                                        docTitle = string.Format("{0}_{1}{2}", fileNameWithoutExt, Workspace.This.LaserChannelTypeList[LaserChannels.ChannelA], fileExtension);
                                        if (_SelectedAppProtocol.ScanRegions[nScanRegion].FileLocationVm.IsAutoSave)
                                        {
                                            fileFullPath = System.IO.Path.Combine(destinationFolder, docTitle);
                                            var clonedImage = laserR1scannedImage.Clone();
                                            clonedImage.Freeze();
                                            Workspace.This.SaveAsync(clonedImage, imageInfo, fileFullPath, false);
                                        }
                                        else
                                        {
                                            fileFullPath = string.Empty;
                                        }
                                        Workspace.This.NewDocument(laserR1scannedImage, imageInfo, docTitle, fileFullPath, bIsSaveAsPUB, true);
                                    }
                                }
                                // Keep raw image
                                if (SettingsManager.ConfigSettings.IsKeepRawImages && laserR1RawImage != null)
                                {
                                    string rawDocTitle = string.Format("{0}_{1}_RAW{2}", fileNameWithoutExt, Workspace.This.LaserChannelTypeList[LaserChannels.ChannelA], fileExtension);
                                    Workspace.This.NewDocument(laserR1RawImage, imageInfo, rawDocTitle, fileFullPath, false, true);
                                }
                            }
                            if (_SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].SelectedLaser.LaserChannel == LaserChannels.ChannelB)   //R2
                            {
                                nLaserSignalLevel = _SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].SelectedSignalLevel.IntensityLevel;
                                if (_IsGrayscaleImage)
                                {
                                    scannedImages[0] = laserR2scannedImage;
                                }
                                else
                                {
                                    SetImageChannel(ref scannedImages,
                                                    ref laserR2scannedImage,
                                                    _SelectedAppProtocol.ScanRegions[nScanRegion].SignalList[i].SelectedColorChannel.ImageColorChannel);
                                    if (!bIsAutoMergeImages)
                                    {
                                        docTitle = string.Format("{0}_{1}{2}", fileNameWithoutExt, Workspace.This.LaserChannelTypeList[LaserChannels.ChannelB], fileExtension);
                                        if (_SelectedAppProtocol.ScanRegions[nScanRegion].FileLocationVm.IsAutoSave)
                                        {
                                            fileFullPath = System.IO.Path.Combine(destinationFolder, docTitle);
                                            var clonedImage = laserR2scannedImage.Clone();
                                            clonedImage.Freeze();
                                            Workspace.This.SaveAsync(clonedImage, imageInfo, fileFullPath, false);
                                        }
                                        else
                                        {
                                            fileFullPath = string.Empty;
                                        }
                                        Workspace.This.NewDocument(laserR2scannedImage, imageInfo, docTitle, fileFullPath, bIsSaveAsPUB, true);
                                    }
                                }
                                // Keep raw image
                                if (SettingsManager.ConfigSettings.IsKeepRawImages && laserR2RawImage != null)
                                {
                                    string rawDocTitle = string.Format("{0}_{1}_RAW{2}", fileNameWithoutExt, Workspace.This.LaserChannelTypeList[LaserChannels.ChannelB], fileExtension);
                                    Workspace.This.NewDocument(laserR2RawImage, imageInfo, rawDocTitle, fileFullPath, false, true);
                                }
                            }
                        }
                        // Now using the accending order of laser's wavelength instead of using laser A/B/C/D (swapping laser A and laser D)
                        //imageInfo.IntensityLevel = string.Format("{0} / {1} / {2} / {3}", laserDSignalLevelDN, laserBSignalLevelDN, laserCSignalLevelDN, laserASignalLevelDN);

                        if (bIsAutoMergeImages || _IsGrayscaleImage)
                        {
                            WriteableBitmap scannedImage = null;
                            if (_IsGrayscaleImage)
                            {
                                scannedImage = scannedImages[0];
                            }
                            else
                            {
                                // MERGE color channels
                                //
                                // Make sure we have a scanned image
                                bool hasImages = false;
                                foreach (var image in scannedImages)
                                {
                                    if (image != null)
                                    {
                                        hasImages = true;
                                        break;
                                    }
                                }
                                if (hasImages)
                                {
                                    scannedImage = ImageProcessing.SetChannel(scannedImages);
                                    if (scannedImage != null)
                                    {
                                        if (scannedImage.CanFreeze)
                                            scannedImage.Freeze();
                                    }
                                }
                            }

                            if (scannedImage != null)
                            {
                                docTitle = string.Format("{0}{1}", fileNameWithoutExt, fileExtension);
                                if (_SelectedAppProtocol.ScanRegions[nScanRegion].FileLocationVm.IsAutoSave)
                                {
                                    fileFullPath = System.IO.Path.Combine(destinationFolder, docTitle);
                                    var clonedImage = scannedImage.Clone();
                                    clonedImage.Freeze();
                                    Workspace.This.SaveAsync(clonedImage, imageInfo, fileFullPath, false);
                                }
                                else
                                {
                                    fileFullPath = string.Empty;
                                }
                                // Add the new scanned image to Gallery
                                Workspace.This.NewDocument(scannedImage, imageInfo, docTitle, fileFullPath, bIsSaveAsPUB, true);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                });
            }

            if (!scanningCommand.IsSaveDataOnScanAbort)
            {
                // Select the next scan region and setup the next scan region's preview images.
                if (nScanRegion + 1 < scanningCommand.ScanParams.Count)
                {
                    // reset image buffer
                    //EL: TODO:
                    //scanningCommand.ChannelAImage = null;
                    //scanningCommand.ChannelBImage = null;
                    //scanningCommand.ChannelCImage = null;
                    //scanningCommand.ChannelDImage = null;

                    Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
                    {
                        nScanRegion++;
                        // Set up the next scan region preview image settings
                        PreviewImageSetup(scanningCommand.ScanParams[nScanRegion], nScanRegion);
                        // Switch scan region
                        _SelectedAppProtocol.SelectedScanRegion = _SelectedAppProtocol.ScanRegions[nScanRegion];
                    });
                }
            }
        }

        private unsafe void UpdatePreviewDisplayImage()
        {
            if (_PreviewImage == null) { return; }

            var imageInfo = _ImagingVm.ContrastVm.DisplayImageInfo;
            if (imageInfo != null)
            {
                // Now turning off saturation display on EDR scan
                //imageInfo.IsSaturationChecked = true;
                imageInfo.SelectedChannel = ImageChannelType.Mix;

                // color order R-G-B-K(gray)
                WriteableBitmap[] displayChannels = { null, null, null, null };

                bool bIsGrayscale = false;
                int nDstStride = 0;
                if (_ImagingVm.PreviewChannels.Count == 1 && _SelectedAppProtocol.SelectedScanRegion.SignalList.Count == 1)
                {
                    if (_SelectedAppProtocol.SelectedScanRegion.SignalList[0].SelectedColorChannel.ImageColorChannel == ImageChannelType.Gray)
                    {
                        bIsGrayscale = true;
                        imageInfo.NumOfChannels = 1;
                        if (_ImagingVm.IsLaserL1PrvSelected)
                            displayChannels[0] = _ChannelL1PrevImage;
                        if (_ImagingVm.IsLaserR1PrvSelected)
                            displayChannels[0] = _ChannelR1PrevImage;
                        if (_ImagingVm.IsLaserR2PrvSelected)
                            displayChannels[0] = _ChannelR2PrevImage;

                        if (_PreviewImage != null)
                            nDstStride = _PreviewImage.BackBufferStride;
                    }
                }

                if (!bIsGrayscale)
                {
                    if (_ImagingVm.IsLaserL1PrvSelected)
                    {
                        if (_ImagingVm.LaserL1ColorChannel == ImageChannelType.Red)
                            displayChannels[0] = _ChannelL1PrevImage;
                        else if (_ImagingVm.LaserL1ColorChannel == ImageChannelType.Green)
                            displayChannels[1] = _ChannelL1PrevImage;
                        else if (_ImagingVm.LaserL1ColorChannel == ImageChannelType.Blue)
                            displayChannels[2] = _ChannelL1PrevImage;
                        else if (_ImagingVm.LaserL1ColorChannel == ImageChannelType.Gray)
                            displayChannels[3] = _ChannelL1PrevImage;
                    }
                    if (_ImagingVm.IsLaserR1PrvSelected)
                    {
                        if (_ImagingVm.LaserR1ColorChannel == ImageChannelType.Red)
                            displayChannels[0] = _ChannelR1PrevImage;
                        else if (_ImagingVm.LaserR1ColorChannel == ImageChannelType.Green)
                            displayChannels[1] = _ChannelR1PrevImage;
                        else if (_ImagingVm.LaserR1ColorChannel == ImageChannelType.Blue)
                            displayChannels[2] = _ChannelR1PrevImage;
                        else if (_ImagingVm.LaserR1ColorChannel == ImageChannelType.Gray)
                            displayChannels[3] = _ChannelR1PrevImage;
                    }
                    if (_ImagingVm.IsLaserR2PrvSelected)
                    {
                        if (_ImagingVm.LaserR2ColorChannel == ImageChannelType.Red)
                            displayChannels[0] = _ChannelR2PrevImage;
                        else if (_ImagingVm.LaserR2ColorChannel == ImageChannelType.Green)
                            displayChannels[1] = _ChannelR2PrevImage;
                        else if (_ImagingVm.LaserR2ColorChannel == ImageChannelType.Blue)
                            displayChannels[2] = _ChannelR2PrevImage;
                        else if (_ImagingVm.LaserR2ColorChannel == ImageChannelType.Gray)
                            displayChannels[3] = _ChannelR2PrevImage;
                    }
                }

                //Contrast in another thread
                //
                int nWidth = 0;
                int nHeight = 0;
                int nStride = 0;
                foreach (var dispChan in displayChannels)
                {
                    if (dispChan != null)
                    {
                        nWidth = dispChan.PixelWidth;
                        nHeight = dispChan.PixelHeight;
                        nStride = dispChan.BackBufferStride;
                        break;
                    }
                }

                //byte* pPreviewImageData = null;
                byte* pRchImageData = null;
                byte* pGchImageData = null;
                byte* pBchImageData = null;
                byte* pKchImageData = null; //gray channel
                byte*[] pDisplayImageData = null;

                if (_ChannelL1PrevImage != null)
                    _ChannelL1PrevImage.Lock();
                if (_ChannelR1PrevImage != null)
                    _ChannelR1PrevImage.Lock();
                if (_ChannelR2PrevImage != null)
                    _ChannelR2PrevImage.Lock();

                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    if (displayChannels[0] != null)
                    {
                        pRchImageData = (byte*)displayChannels[0].BackBuffer.ToPointer();
                    }
                    if (displayChannels[1] != null)
                    {
                        pGchImageData = (byte*)displayChannels[1].BackBuffer.ToPointer();
                    }
                    if (displayChannels[2] != null)
                    {
                        pBchImageData = (byte*)displayChannels[2].BackBuffer.ToPointer();
                    }
                    if (displayChannels[3] != null)
                    {
                        pKchImageData = (byte*)displayChannels[3].BackBuffer.ToPointer();
                    }

                    pDisplayImageData = new byte*[4] { pRchImageData, pGchImageData, pBchImageData, pKchImageData };
                });

                // Contrast in its own thread.
                //
                _IsUpdatingPreviewImage = true;
                _ContrastThreadEvent.Set();
                Thread contrastThread = new Thread(() => ContrastTask(pDisplayImageData, nWidth, nHeight, nStride,
                                                                      _ImagingVm.ContrastVm.DisplayImageInfo, bIsGrayscale));
                contrastThread.Start();
            }

        }

        private unsafe void ContrastTask(byte*[] pSrcImageData, int nWidth, int nHeight, int nSrcStride,
                                         ImageInfo imageInfo, bool bIsGrayscale)
        {
            _ContrastThreadEvent.WaitOne();

            int nDstStride = 0;
            byte* pDstImageData = null;
            bool bIsAutoProcessed = false;

            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                if (_PreviewImage != null)
                {
                    nDstStride = _PreviewImage.BackBufferStride;
                    _PreviewImage.Lock();
                    pDstImageData = (byte*)_PreviewImage.BackBuffer.ToPointer();
                }
            });

            try
            {
                PixelFormatType srcPixelFormat = PixelFormatType.P16u_C1;
                if (bIsGrayscale)
                {
                    // scale single channel grayscale image
                    ImageProcessingHelper.UpdateDisplayImage(pSrcImageData[0], nWidth, nHeight, nSrcStride, srcPixelFormat,
                                                             imageInfo, pDstImageData, nDstStride, ref bIsAutoProcessed);
                }
                else
                {
                    // scale multi-channel image
                    ImageProcessingHelper.UpdateDisplayImage(pSrcImageData, nWidth, nHeight, nSrcStride, srcPixelFormat,
                                                             imageInfo, pDstImageData, nDstStride, ref bIsAutoProcessed);
                }
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    Workspace.This.LogException("Error updating display image.", ex);

                    if (_PreviewImage != null)
                        _PreviewImage.Unlock();
                    if (_ChannelL1PrevImage != null)
                        _ChannelL1PrevImage.Unlock();
                    if (_ChannelR1PrevImage != null)
                        _ChannelR1PrevImage.Unlock();
                    if (_ChannelR2PrevImage != null)
                        _ChannelR2PrevImage.Unlock();
                });
                _IsUpdatingPreviewImage = false;
                _ContrastThreadEvent.Set();
                return;
            }

            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                // This is in case the user deselected all the preview channels.
                if (_ImagingVm.PreviewImage == null)
                    _ImagingVm.PreviewImage = _PreviewImage;

                if (_PreviewImage != null)
                {
                    // Specify the area of the bitmap that changed.
                    _PreviewImage.AddDirtyRect(new Int32Rect(0, 0, _PreviewImage.PixelWidth, _PreviewImage.PixelHeight));
                }
                if (_ImagingVm.ContrastVm.DisplayImageInfo.RedChannel.IsAutoChecked ||
                    _ImagingVm.ContrastVm.DisplayImageInfo.GreenChannel.IsAutoChecked ||
                    _ImagingVm.ContrastVm.DisplayImageInfo.BlueChannel.IsAutoChecked ||
                    _ImagingVm.ContrastVm.DisplayImageInfo.GrayChannel.IsAutoChecked ||
                    _ImagingVm.ContrastVm.DisplayImageInfo.MixChannel.IsAutoChecked)
                {
                    if (bIsAutoProcessed)
                    {
                        // Reset auto-contrast flags.
                        _ImagingVm.ContrastVm.ResetAuto();
                    }
                }

                if (_PreviewImage != null)
                    _PreviewImage.Unlock();
                if (_ChannelL1PrevImage != null)
                    _ChannelL1PrevImage.Unlock();
                if (_ChannelR1PrevImage != null)
                    _ChannelR1PrevImage.Unlock();
                if (_ChannelR2PrevImage != null)
                    _ChannelR2PrevImage.Unlock();
            });

            _IsUpdatingPreviewImage = false;
            _ContrastThreadEvent.Set();
        }

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
            if (_ImageScanCommand != null)
            {
                // Close preview contrast window to avoid it from overlapping messagebox below.
                IsPreviewChannels = false;
                MessageBoxResult result;

                string lidStatus = string.Empty;
                if (parameter != null && parameter is string)
                {
                    lidStatus = parameter as string;
                }

                if (!string.IsNullOrEmpty(lidStatus) && lidStatus.Equals("LIDOpened"))
                {
                    if (_CurrentScanType != ScanType.Preview)
                    {
                        if (_CurrentScanType != ScanType.Auto || (_CurrentScanType == ScanType.Auto && _IsAutoScanCompleted))
                        {
                            // Save the scan data
                            _IsSaveScanDataOnAborted = true;
                            _ImageScanCommand.IsSaveDataOnScanAbort = _IsSaveScanDataOnAborted;
                        }
                    }

                    _ImageScanCommand.DataReceived -= ImageScanCommand_DataReceived;
                    _ImageScanCommand.Abort();  // Abort the scanning thread
                }
                else
                {
                    if (_CurrentScanType == ScanType.Auto)
                    {
                        result = MessageBoxResult.No;
                    }
                    else
                    {
                        if (_CurrentScanType == ScanType.Preview)
                        {
                            // Don't prompt to keep the scanned data - automatically keep the scanned data
                            result = MessageBoxResult.Yes;
                            _IsSaveScanDataOnAborted = true;
                            _ImageScanCommand.IsSaveDataOnScanAbort = _IsSaveScanDataOnAborted;

                        }
                        else
                        {
                            // If EDR scanning, don't prompt to save the scan data
                            // Don't prompt to keep the scanned data - automatically keep the scanned data
                            result = MessageBoxResult.Yes;
                            if (_ImageScanCommand.CurrentScanParam != null && !_ImageScanCommand.CurrentScanParam.IsEdrScanning)
                            {
                                string caption = "Abort scanning...";
                                string message = "Would you like to save your scanned data?";
                                result = Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.YesNoCancel, MessageBoxImage.Information);
                                if (result == MessageBoxResult.Yes)
                                {
                                    if (Workspace.This.IsScanning)  // Scanning completed?
                                    {
                                        if (_ImageScanCommand != null)
                                        {
                                            _IsSaveScanDataOnAborted = true;
                                            _ImageScanCommand.IsSaveDataOnScanAbort = _IsSaveScanDataOnAborted;
                                        }
                                    }
                                    else
                                    {
                                        // Scan already completed
                                        result = MessageBoxResult.Cancel;
                                    }
                                }
                                else
                                {
                                    if (_ImageScanCommand != null)
                                    {
                                        _IsSaveScanDataOnAborted = false;
                                        _ImageScanCommand.IsSaveDataOnScanAbort = _IsSaveScanDataOnAborted;
                                    }
                                }
                            }
                        }
                    }

                    if (result != MessageBoxResult.Cancel)
                    {
                        if (_ImageScanCommand != null)
                        {
                            _ImageScanCommand.DataReceived -= ImageScanCommand_DataReceived;
                            _ImageScanCommand.Abort();  // Abort the scanning thread
                        }
                    }
                }
            }
            else if (_AutoAlignScanCommand != null)
            {
                _AutoAlignScanCommand.Abort();
            }
        }

        public bool CanExecuteStopScanCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region ResolutionChangedCommand

        public ICommand ResolutionChangedCommand
        {
            get
            {
                if (_ResolutionChangedCommand == null)
                {
                    _ResolutionChangedCommand = new RelayCommand(ExecuteResolutionChangedCommand, CanExecuteResolutionChangedCommand);
                }

                return _ResolutionChangedCommand;
            }
        }

        public void ExecuteResolutionChangedCommand(object parameter)
        {
            int resolution = (int)parameter;
        }

        public bool CanExecuteResolutionChangedCommand(object parameter)
        {
            return true;
        }

        #endregion


        #region ClosePreviewWinCommand
        private RelayCommand _ClosePreviewWinCommand = null;
        public ICommand ClosePreviewWinCommand
        {
            get
            {
                if (_ClosePreviewWinCommand == null)
                {
                    _ClosePreviewWinCommand = new RelayCommand((p) => OnClosePreviewWin(p), (p) => CanClosePreviewWin(p));
                }
                return _ClosePreviewWinCommand;
            }
        }

        private void OnClosePreviewWin(object parameter)
        {
            IsPreviewChannels = false;
        }

        private bool CanClosePreviewWin(object parameter)
        {
            return IsPreviewChannels;
        }

        #endregion


        #region Auto Alignment

        private RelayCommand _ShowAutoAlignCommand = null;
        public ICommand ShowAutoAlignCommand
        {
            get
            {
                if (_ShowAutoAlignCommand == null)
                {
                    _ShowAutoAlignCommand = new RelayCommand((p) => OnShowAutoAlignWindow(p), (p) => CanShowAutoAlignWindow(p));
                }
                return _ShowAutoAlignCommand;
            }
        }
        private void OnShowAutoAlignWindow(object parameter)
        {
            if (Workspace.This.IsScanning) { return; }
            var autoAlign = new AutoAlignmentWindow();
            var dlgResult = autoAlign.ShowDialog();

            if (dlgResult == true)
            {
                //Do something...
                StartAutoAlignment();
            }
        }
        private bool CanShowAutoAlignWindow(object parameter)
        {
            return true;
        }

        private LaserScanCommand _AutoAlignScanCommand = null;
        private void StartAutoAlignment()
        {
            if (Workspace.This.LaserOptions.Count < 2)
            {
                string caption = "Auto Alignment";
                string message = "Auto Alignment requires 2 or 3 optimal modules installed to perform the auto alignment function. Please SHUT DOWN THE INSTRUMENT before loading another optical module.\n" +
                                 "Click OK to home the scan head and close the application.";
                var parent = Application.Current.MainWindow;
                var dlgResult = Xceed.Wpf.Toolkit.MessageBox.Show(parent, message, caption, MessageBoxButton.OKCancel, MessageBoxImage.Stop);
                if (dlgResult == MessageBoxResult.OK)
                {
                    Workspace.This.ExecuteChangeLaserModuleCommand(null);
                }
                return;
            }
            else if (Workspace.This.LaserOptions.Count == 2)
            {
                // 2 laser modules installed and one of them is a Phosphor module.
                // Not possible to do auto alignment with the Phosphor module and another laser module.

                if (Workspace.This.LaserModuleL1 != null)
                {
                    if (ImagingSystemHelper.IsPhosphorModule(Workspace.This.LaserModuleL1))
                    {
                        string caption = "Auto Alignment";
                        string message = string.Format("Detected a Phosphor module in Port #1.\nPlease SHUT DOWN THE INSTRUMENT before replacing it with another optical module.\nClick OK to home the scan head and close the application.");
                        var parent = Application.Current.MainWindow;
                        var dlgResult = Xceed.Wpf.Toolkit.MessageBox.Show(parent, message, caption, MessageBoxButton.OKCancel, MessageBoxImage.Stop);
                        if (dlgResult == MessageBoxResult.OK)
                        {
                            Workspace.This.ExecuteChangeLaserModuleCommand(null);
                        }
                        return;
                    }
                }
            }

            // Is Phosphor module installed?
            /*bool bIsPhosphorModuleInstalled = false;
            LaserTypes phosphorLaser = null;
            var itemsFound = Workspace.This.LaserOptions.Where(xx => xx.SensorType == IvSensorType.PMT).ToList();
            if (itemsFound != null && itemsFound.Count > 0)
            {
                foreach (var item in itemsFound)
                {
                    if (item.Wavelength == 638 || item.Wavelength == 658 || item.Wavelength == 685)
                    {
                        bIsPhosphorModuleInstalled = true;
                        phosphorLaser = item;
                        break;
                    }
                }
                if (bIsPhosphorModuleInstalled)
                {
                    string portNumber = string.Empty;
                    if (phosphorLaser != null)
                    {
                        if (phosphorLaser.LaserChannel == LaserChannels.ChannelC)
                            portNumber = "1";
                        else if (phosphorLaser.LaserChannel == LaserChannels.ChannelA)
                            portNumber = "2";
                        else if (phosphorLaser.LaserChannel == LaserChannels.ChannelB)
                            portNumber = "3";
                    }
                    string caption = "Auto Alignment";
                    string message = string.Format("Detected a Phosphor module in Port #{0}.\nPlease SHUT DOWN THE INSTRUMENT before replacing it with another optical module.\nClick OK to home the scan head and close the application.", portNumber);
                    var parent = Application.Current.MainWindow;
                    var dlgResult = Xceed.Wpf.Toolkit.MessageBox.Show(parent, message, caption, MessageBoxButton.OKCancel, MessageBoxImage.Stop);
                    if (dlgResult == MessageBoxResult.OK)
                    {
                        Workspace.This.ExecuteChangeLaserModuleCommand(null);
                    }
                    return;
                }
            }*/

            _CurrentScanType = ScanType.Auto;
            _IsAutoScan = true;

            if (!SettingsManager.ConfigSettings.IsSimulationMode && Workspace.This.EthernetController.IsConnected)
            {
                Workspace.This.NewParameterVM.ExecuteParametersReadCommand(null);
            }

            bool bIsUnidirectionalScan = false;     // 2-line average
            bool bIsPixelOffsetProcessing = true;   // Sawtooth correction
            int scanResolution = 10;                // 10 micron
            int scanSpeed = 1;                      // 1 = Highest
            // scan region coordinate
            double x = SettingsManager.ConfigSettings.AutoAlignScanRegion.X;
            double y = SettingsManager.ConfigSettings.AutoAlignScanRegion.Y;
            double w = SettingsManager.ConfigSettings.AutoAlignScanRegion.Width;
            double h = SettingsManager.ConfigSettings.AutoAlignScanRegion.Height;
            if (SettingsManager.ApplicationSettings.EdmundTargetType == EdmundTargetType.BrandingOnTop)
            {
                y = 0.0;
            }
            x = Math.Round(x * _ImagingVm.CellSize, 2);
            y = Math.Round(y * _ImagingVm.CellSize, 2);
            w = Math.Round(w * _ImagingVm.CellSize, 2);
            h = Math.Round(h * _ImagingVm.CellSize, 2);

            ScanDeltaX = (int)(_ScanWidthInMm * w / (ImagingVm.CellSize * ImagingVm.NumOfCells));
            ScanDeltaY = (int)(_ScanHeightInMm * h / (ImagingVm.CellSize * ImagingVm.NumOfCells));
            ScanX0 = (int)(_ScanWidthInMm * x / (ImagingVm.CellSize * ImagingVm.NumOfCells));
            ScanY0 = (int)(_ScanHeightInMm * y / (ImagingVm.CellSize * ImagingVm.NumOfCells));
            _ScanZ0 = Workspace.This.MotorVM.AbsZPos * _ZMotorSubdivision;

            // Add the overscan width and height
            //_ScanDeltaX += (int)Math.Round(SettingsManager.ConfigSettings.XMotionExtraMoveLength * _XMotorSubdivision);
            _ScanDeltaY += (int)Math.Round(SettingsManager.ConfigSettings.YMotionExtraMoveLength * _YMotorSubdivision);

            int width = (int)(ScanDeltaX * 1000.0 / scanResolution);
            int height = (int)(ScanDeltaY * 1000.0 / scanResolution);
            if (scanResolution > 0)
            {
                width = (int)(ScanDeltaX * 1000.0 / scanResolution);
                height = (int)(ScanDeltaY * 1000.0 / scanResolution);
                if (bIsUnidirectionalScan)
                {
                    Time = height * scanSpeed;
                }
                else
                {
                    Time = height * scanSpeed / 2;
                }
            }

            ScanParameterStruct scanParam = new ScanParameterStruct();
            List<ScanParameterStruct> scanParams = new List<ScanParameterStruct>();

            scanParam.Width = width;
            scanParam.Height = height;
            scanParam.ScanDeltaX = (int)_ScanDeltaX;
            scanParam.ScanDeltaY = (int)_ScanDeltaY;
            scanParam.ScanDeltaZ = (int)_ScanDeltaZ;
            scanParam.ScanX0 = (int)_ScanX0;
            scanParam.ScanY0 = (int)_ScanY0;
            scanParam.ScanZ0 = (int)_ScanZ0;
            scanParam.AbsFocusPosition = Workspace.This.MotorVM.AbsZPos;
            scanParam.Res = scanResolution; // scan resolution in micron
            scanParam.Quality = scanSpeed;  // scan quality/speed (NOT Quality that refers to unidirectional scan)
            scanParam.Time = Time;
            scanParam.XMotorSubdivision = _XMotorSubdivision;
            scanParam.YMotorSubdivision = _YMotorSubdivision;
            scanParam.ZMotorSubdivision = _ZMotorSubdivision;
            //EL: TODO: do we need these for horizontal scan
            //scanParam.DataRate = DataRate;
            //scanParam.LineCounts = LineCounts;
            scanParam.DataRate = 25;
            scanParam.LineCounts = 1000;
            scanParam.XMotorSpeed = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[0].Speed * SettingsManager.ConfigSettings.XMotorSubdivision);
            scanParam.XMotionAccVal = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[0].Accel * SettingsManager.ConfigSettings.XMotorSubdivision);
            scanParam.XMotionDccVal = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[0].Dccel * SettingsManager.ConfigSettings.XMotorSubdivision);
            scanParam.YMotorSpeed = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[1].Speed * SettingsManager.ConfigSettings.YMotorSubdivision);
            scanParam.YMotionAccVal = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[1].Accel * SettingsManager.ConfigSettings.YMotorSubdivision);
            scanParam.YMotionDccVal = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[1].Dccel * SettingsManager.ConfigSettings.YMotorSubdivision);
            scanParam.ZMotorSpeed = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[2].Speed * SettingsManager.ConfigSettings.ZMotorSubdivision);
            scanParam.ZMotionAccVal = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[2].Accel * SettingsManager.ConfigSettings.ZMotorSubdivision);
            scanParam.ZMotionDccVal = (int)Math.Round(SettingsManager.ConfigSettings.MotorSettings[2].Dccel * SettingsManager.ConfigSettings.ZMotorSubdivision);
            scanParam.IsNewFirmwire = Workspace.This.MotorVM.IsNewFirmware;
            scanParam.XmotionTurnAroundDelay = SettingsManager.ConfigSettings.XMotionTurnDelay;
            scanParam.XMotionExtraMoveLength = SettingsManager.ConfigSettings.XMotionExtraMoveLength;
            scanParam.YMotionExtraMoveLength = SettingsManager.ConfigSettings.YMotionExtraMoveLength;
            scanParam.LCoefficient = Workspace.This.NewParameterVM.LCoefficient;
            scanParam.L375Coefficient = Workspace.This.NewParameterVM.L375Coefficient;
            scanParam.R1Coefficient = Workspace.This.NewParameterVM.R1Coefficient;
            scanParam.R2Coefficient = Workspace.This.NewParameterVM.R2Coefficient;
            scanParam.R2532Coefficient = Workspace.This.NewParameterVM.R2532Coefficient;
            scanParam.IsIgnoreCompCoefficient = SettingsManager.ConfigSettings.IsIgnoreCompCoefficient;
            //Grating ruler pulse
            scanParam.XEncoderSubdivision = Workspace.This.NewParameterVM.XEncoderSubdivision;
            if (SettingsManager.ConfigSettings.IsSimulationMode)
            {
                scanParam.XEncoderSubdivision = SettingsManager.ConfigSettings.XEncoderSubdivision;
            }
            scanParam.IsUnidirectionalScan = bIsUnidirectionalScan;
            scanParam.EdrScaleFactor = SettingsManager.ConfigSettings.EdrScaleFactor;
            scanParam.IsEdrScanning = false;

            //（因为在设计中将R1透镜的位置和R2透镜的位置更换了，然后参数名没有更改成对应的通道，所以下面参数名R1其实是R2,R2其实是R1）
            // (in the design, the position of R1 lens and R2 lens is changed, and the parameter name is not changed to the corresponding channel, so the following parameter name R1 is actually R2, R2 is actually R1)
            //L透镜到R2透镜之间的距离（mm），毫米为单位
            //The distance between L lens and R2 lens (mm), in millimeters
            int OpticalL_R1Distance = (int)Workspace.This.NewParameterVM.OpticalL_R1Distance;
            if (SettingsManager.ConfigSettings.IsSimulationMode)
            {
                OpticalL_R1Distance = 48;
                Workspace.This.NewParameterVM.OpticalL_R1Distance = OpticalL_R1Distance;
            }
            //根据当前选择的分辨率计算L透镜到R2透镜之间的实际像素宽度
            //// Calculate the actual pixel width between L lens and R2 lens based on the currently selected resolution
            int OffsetWidth = (int)(OpticalL_R1Distance * 1000.0 / scanParam.Res);
            //扫描图像宽度加上补偿透镜之间距离的宽度（像素）
            // The width of the scanned image plus the width of the distance between the compensating lenses (pixels)
            int currentRangeWidth = scanParam.Width + OffsetWidth;
            int currentRangeHeight = scanParam.Height;
            scanParam.Width = currentRangeWidth;
            int offsetDeltaXPulse = 0;
            //将L到R2透镜之间的距离（mm）累加到ScanDeltaX上
            //Add the distance (mm) between L and R2 lens to ScanDeltaX
            //if (Workspace.This.NewParameterVM.OpticalL_R1Distance > 0 && scanParam.Res == 150)
            //{
            //    offsetDeltaXPulse = (int)Workspace.This.NewParameterVM.OpticalL_R1Distance - 1;
            //}
            //else
            //{
            //    offsetDeltaXPulse = (int)Workspace.This.NewParameterVM.OpticalL_R1Distance;
            //}
            offsetDeltaXPulse = (int)Workspace.This.NewParameterVM.OpticalL_R1Distance;
            offsetDeltaXPulse = (int)(offsetDeltaXPulse * _XMotorSubdivision);
            scanParam.ScanDeltaX += offsetDeltaXPulse;

            //scanParam.Width = (int)(scanParam.ScanDeltaX / scanParam.XMotorSubdivision * 1000.0 / scanParam.Res);
            //TODO: temporary work-around: make sure the width is an even value to avoid the skewed on the scanned image
            if (scanParam.Width % 2 != 0)
            {
                //int deltaX = scanParam.ScanDeltaX - (int)Math.Round(scanParam.Res / 1000.0 * scanParam.XMotorSubdivision);
                //scanParam.ScanDeltaX = deltaX;
                //scanParam.Width = (int)(scanParam.ScanDeltaX / scanParam.XMotorSubdivision * 1000.0 / scanParam.Res);
                scanParam.Width--;
                scanParam.ScanDeltaX = (int)(scanParam.Width * scanParam.Res / 1000.0 * _XMotorSubdivision);
            }

            scanParam.AlignmentParam = new ImageAlignParam();
            scanParam.AlignmentParam.Resolution = scanParam.Res;
            scanParam.AlignmentParam.PixelOddX = SettingsManager.ConfigSettings.XOddNumberedLine;   //X odd Line
            scanParam.AlignmentParam.PixelEvenX = SettingsManager.ConfigSettings.XEvenNumberedLine; //X even Line
            scanParam.AlignmentParam.PixelOddY = SettingsManager.ConfigSettings.YOddNumberedLine;   //Y odd Line
            scanParam.AlignmentParam.PixelEvenY = SettingsManager.ConfigSettings.YEvenNumberedLine; //Y Even Line
            scanParam.AlignmentParam.YCompOffset = SettingsManager.ConfigSettings.YCompenOffset;
            scanParam.AlignmentParam.OpticalL_R1Distance = Workspace.This.NewParameterVM.OpticalL_R1Distance;
            scanParam.AlignmentParam.Pixel_10_L_DX = Workspace.This.NewParameterVM.Pixel_10_L_DX;
            scanParam.AlignmentParam.Pixel_10_L_DY = Workspace.This.NewParameterVM.Pixel_10_L_DY;
            scanParam.AlignmentParam.OpticalR2_R1Distance = Workspace.This.NewParameterVM.OpticalR2_R1Distance;
            scanParam.AlignmentParam.Pixel_10_R2_DX = Workspace.This.NewParameterVM.Pixel_10_R2_DX;
            scanParam.AlignmentParam.Pixel_10_R2_DY = Workspace.This.NewParameterVM.Pixel_10_R2_DY;
            scanParam.AlignmentParam.IsImageOffsetProcessing = SettingsManager.ConfigSettings.ImageOffsetProcessing;
            scanParam.AlignmentParam.IsPixelOffsetProcessing = bIsPixelOffsetProcessing;
            scanParam.AlignmentParam.PixelOffsetProcessingRes = SettingsManager.ConfigSettings.PixelOffsetProcessingRes;
            scanParam.AlignmentParam.IsYCompensationBitAt = SettingsManager.ConfigSettings.YCompenSationBitAt;
            scanParam.AlignmentParam.XMotionExtraMoveLength = SettingsManager.ConfigSettings.XMotionExtraMoveLength;
            scanParam.AlignmentParam.YMotionExtraMoveLength = SettingsManager.ConfigSettings.YMotionExtraMoveLength;

            // SmartScan settings
            scanParam.IsSmartScanning = true;
            scanParam.SmartScanResolution = SettingsManager.ConfigSettings.AutoScanSettings.Resolution;
            List<int> keyList = new List<int>(SettingsManager.ConfigSettings.LasersSignalList.Keys);
            scanParam.SmartScanSignalLevels = SettingsManager.ConfigSettings.LasersSignalList[keyList[0]].Count;
            scanParam.SmartScanFloor = SettingsManager.ConfigSettings.AutoScanSettings.Floor;
            scanParam.SmartScanCeiling = SettingsManager.ConfigSettings.AutoScanSettings.Ceiling;
            scanParam.SmartScanOptimalVal = SettingsManager.ConfigSettings.AutoScanSettings.OptimalVal;
            scanParam.SmartScanOptimalDelta = SettingsManager.ConfigSettings.AutoScanSettings.OptimalDelta;
            scanParam.SmartScanAlpha488 = SettingsManager.ConfigSettings.AutoScanSettings.Alpha488;
            scanParam.SmartScanInitSignalLevel = SettingsManager.ConfigSettings.AutoScanSettings.StartingSignalLevel;
            scanParam.SmartScanSignalStepdownLevel = SettingsManager.ConfigSettings.AutoScanSettings.HighSignalStepdownLevel;
            // Laser modules
            // L1 = laser channel C
            // R1 = laser channel A
            // R2 = laser channel B
            Signal signal = null;
            List<Signal> scanSignals = new List<Signal>();

            if (Workspace.This.LaserModuleL1 != null)
            {
                if (!ImagingSystemHelper.IsPhosphorModule(Workspace.This.LaserModuleL1))
                {
                    if (SettingsManager.ConfigSettings.LasersSignalList.ContainsKey(Workspace.This.LaserL1))
                    {
                        scanParam.LaserL1SignalOptions = new List<Signal>(SettingsManager.ConfigSettings.LasersSignalList[Workspace.This.LaserL1]);
                        if (scanParam.LaserL1SignalOptions != null)
                        {
                            var tempSignal = scanParam.LaserL1SignalOptions.Where(xx => xx.DisplayName == "5").FirstOrDefault();
                            if (tempSignal != null)
                                signal = (Signal)tempSignal.Clone();
                        }
                        if (signal == null)
                        {
                            signal = (Signal)scanParam.LaserL1SignalOptions[4].Clone();
                        }
                        signal.ColorChannel = (int)ImageChannelType.Red;
                        signal.LaserChannel = LaserChannels.ChannelC;
                        signal.LaserWavelength = Workspace.This.LaserModuleL1.LaserWavelength;
                        signal.SensorType = Workspace.This.LaserModuleL1.SensorType;
                        signal.SignalLevel = signal.Position;
                        scanSignals.Add(signal);
                    }
                }
            }
            if (SettingsManager.ConfigSettings.LasersSignalList.ContainsKey(Workspace.This.LaserR1))
            {
                scanParam.LaserR1SignalOptions = new List<Signal>(SettingsManager.ConfigSettings.LasersSignalList[Workspace.This.LaserR1]);
                if (scanParam.LaserR1SignalOptions != null)
                {
                    var tempSignal = scanParam.LaserR1SignalOptions.Where(xx => xx.DisplayName == "5").FirstOrDefault();
                    if (tempSignal != null)
                        signal = (Signal)tempSignal.Clone();
                }
                if (signal == null)
                {
                    signal = (Signal)scanParam.LaserR1SignalOptions[4].Clone();
                }
                signal.ColorChannel = (int)ImageChannelType.Green;
                signal.LaserChannel = LaserChannels.ChannelA;
                signal.LaserWavelength = Workspace.This.LaserModuleR1.LaserWavelength;
                signal.SensorType = Workspace.This.LaserModuleR1.SensorType;
                signal.SignalLevel = signal.Position;
                scanSignals.Add(signal);
            }
            if (SettingsManager.ConfigSettings.LasersSignalList.ContainsKey(Workspace.This.LaserR2))
            {
                scanParam.LaserR2SignalOptions = new List<Signal>(SettingsManager.ConfigSettings.LasersSignalList[Workspace.This.LaserR2]);
                if (scanParam.LaserR2SignalOptions != null)
                {
                    var tempSignal = scanParam.LaserR2SignalOptions.Where(xx => xx.DisplayName == "5").FirstOrDefault();
                    if (tempSignal != null)
                        signal = (Signal)tempSignal.Clone();
                }
                if (signal == null)
                {
                    signal = (Signal)scanParam.LaserR2SignalOptions[4].Clone();
                }
                signal.ColorChannel = (int)ImageChannelType.Blue;
                signal.LaserChannel = LaserChannels.ChannelB;
                signal.LaserWavelength = Workspace.This.LaserModuleR2.LaserWavelength;
                signal.SensorType = Workspace.This.LaserModuleR2.SensorType;
                signal.SignalLevel = signal.Position;
                scanSignals.Add(signal);
            }
            //else
            //{
            //    string caption = "Auto Alignment";
            //    string message = "No optical module detected in Port #3. Port #3 must contain an optical module in order to perform the auto alignment function. Please SHUT DOWN THE INSTRUMENT before loading an optical module into Port #3.";
            //    var parent = Application.Current.MainWindow;
            //    var dlgResult = Xceed.Wpf.Toolkit.MessageBox.Show(parent, message, caption, MessageBoxButton.OKCancel, MessageBoxImage.Stop);
            //    if (dlgResult == MessageBoxResult.OK)
            //    {
            //        Workspace.This.ExecuteChangeLaserModuleCommand(null);
            //    }
            //    return;
            //}

            scanParam.Signals = scanSignals;
            scanParams.Add(scanParam);

            Workspace.This.LogMessage(string.Format("Pixel Size: {0}", scanParam.Res));
            Workspace.This.LogMessage(string.Format("Scan Speed: {0}", scanParam.Quality));
            Workspace.This.LogMessage(string.Format("Focus: {0}", scanParam.ScanZ0));

            // Setting this flag here to avoid null reference on preview channels changed.
            _IsPrescanCompleted = false;

            //
            // Clear previous preview image buffers
            //
            _PreviewImage = null;
            _ChannelL1PrevImage = null;
            _ChannelR1PrevImage = null;
            _ChannelR2PrevImage = null;
            _ChannelL1PrevImageUnAligned = null;
            _ChannelR1PrevImageUnAligned = null;
            _ChannelR2PrevImageUnAligned = null;

            if (this._ImagingVm != null && this._ImagingVm.PreviewImage != null)
            {
                this.ImagingVm.PreviewImage = null;     //clear preview image
            }

            if (this._ImagingVm != null && this._ImagingVm.PreviewImages != null)
            {
                // Clear preview image(s)
                _ImagingVm.PreviewImages.Clear();
            }

            _SelectedAppProtocol.SelectedScanRegion = _SelectedAppProtocol.ScanRegions[0];

            if (!SettingsManager.ConfigSettings.IsSimulationMode)
            {
                if (!Workspace.This.EthernetController.IsConnected)
                {
                    string caption = "Sapphire FL Biomolecular Imager";
                    string message = "Scanner is not connected.\nPlease make sure the system power is turned on, \nand the USB cable is securely connected.";
                    Xceed.Wpf.Toolkit.MessageBox.Show(Workspace.This.Owner, message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                    return;
                }

                //Turn off all the lasers before scanning just in case they were left on from previous scan.
                Workspace.This.TurnOffAllLasers();

                // Is the scanner lid opened?
                if (Workspace.This.EthernetController.LidIsOpen)
                {
                    string caption = "Sapphire FL Biomolecular Imager";
                    string message = "The scanner lid is opened.\n" +
                                        "Please close the lid, then try scanning again.";
                    Xceed.Wpf.Toolkit.MessageBox.Show(Workspace.This.Owner, message, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            ImageProcessing.FlipAxis flipAxis = ImageProcessing.FlipAxis.None;
            if (SettingsManager.ApplicationSettings.IsHorizontalFlipEnabled)
            {
                flipAxis = ImageProcessing.FlipAxis.Horizontal;
            }
            else if (SettingsManager.ApplicationSettings.IsVerticalFlipEnabled)
            {
                flipAxis = ImageProcessing.FlipAxis.Vertical;
            }

            bool bIsSaveDebuggingImages = SettingsManager.ConfigSettings.IsSaveDebuggingImages;
            _AutoAlignScanCommand = new LaserScanCommand(Application.Current.Dispatcher,
                                                     Workspace.This.EthernetController,
                                                     Workspace.This.MotorVM.MotionController,
                                                     scanParams,
                                                     false,
                                                     SettingsManager.ConfigSettings.IsApplyImageSmoothing,
                                                     Workspace.This.AppDataPath,
                                                     flipAxis,
                                                     bIsSaveDebuggingImages);
            _AutoAlignScanCommand.Logger = Workspace.This.Logger;
            _AutoAlignScanCommand.ApplicationDataPath = SettingsManager.ApplicationDataPath;
            _AutoAlignScanCommand.OnScanDataReceived += AutoAlignScanCommand_OnScanDataReceived;
            _AutoAlignScanCommand.Completed += AutoAlignScanCommand_Completed;
            _IsAutoScanCompleted = false;
            _AutoAlignScanCommand.SmartScanStarting += AutoAlignScanCommand_SmartScanStarting;
            _AutoAlignScanCommand.SmartScanUpdated += AutoAlignScanCommand_SmartScanUpdated;
            _AutoAlignScanCommand.SmartScanCompleted += AutoAlignScanCommand_SmartScanCompleted;
            //_AutoAlignScanCommand.IsKeepRawImages = SettingsManager.ConfigSettings.IsKeepRawImages;

            Workspace.This.IsScanning = true;

            if (!SettingsManager.ConfigSettings.IsSimulationMode)
                Workspace.This.IsPreparingToScan = true;

            Workspace.This.StartWaitAnimation("Auto alignment scan in progress.\nPlease wait...");

            // Start the image scanning thread
            _AutoAlignScanCommand.Start();
        }

        private void AutoAlignScanCommand_OnScanDataReceived(object sender, string dataName)
        {
            LaserScanCommand laserScanCommand = sender as LaserScanCommand;
            Application.Current.Dispatcher.BeginInvoke((Action)delegate
            {
                if (dataName == "PreparingToScan" || dataName == "ScanningPrepCompleted")
                {
                    if (dataName == "PreparingToScan")
                    {
                        Workspace.This.IsPreparingToScan = true;
                    }
                    else if (dataName == "ScanningPrepCompleted")
                    {
                        Workspace.This.IsPreparingToScan = false;
                    }
                }
                else if (dataName == "LIDOpened")
                {
                    if (!_IsAbortedOnLidOpened)
                    {
                        //System.Diagnostics.Trace.WriteLine("LID is opened");
                        _IsAbortedOnLidOpened = true;
                        ExecuteStopScanCommand("LIDOpened");
                        return;
                    }
                }

                if (_CurrentScanType == ScanType.Auto && laserScanCommand.IsSmartScanCalc || laserScanCommand.IsEdrScanCalc)
                {
                    //Don't display the time remaining countdown while calculating the signal level.
                    //Workspace.This.StatusTextProgress = "SMARTSCAN in progress....";
                }
                else
                {
                    if (dataName == "RemainingTime")
                    {
                        RemainingTime = laserScanCommand.RemainingTime;
                        double timeElapsed = Time - RemainingTime;
                        Workspace.This.StatusTextProgress = ImagingSystemHelper.FormatTime(RemainingTime);
                        double percentCompleted = (timeElapsed / (double)Time) * 100.0;
                        Workspace.This.PercentCompleted = (int)percentCompleted;
                    }
                }
            });
        }

        private void AutoAlignScanCommand_SmartScanCompleted(ThreadBase sender)
        {
            Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
            {
                _IsAutoScanCompleted = true;
                _CurrentScanType = ScanType.Normal;
                var scanThread = sender as LaserScanCommand;
                var updatedSignals = scanThread.UpdatedSignalLevel;

                Workspace.This.LogMessage("Auto Alignment: SmartScan completed.");

                List<string> signalLevels = new List<string>();
                foreach (var signal in updatedSignals)
                {
                    signalLevels.Add(signal.DisplayName);
                }
                string scanLevels = string.Join(",", signalLevels);
                Workspace.This.LogMessage(string.Format("Auto Alignment: Signal levels: {0}", scanLevels));
            });
        }

        private void AutoAlignScanCommand_SmartScanUpdated(ThreadBase sender)
        {
            Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
            {
                var scanThread = sender as LaserScanCommand;
                var updatedSignals = scanThread.UpdatedSignalLevel;
                List<string> signalLevels = new List<string>();
                foreach (var signal in updatedSignals)
                {
                    signalLevels.Add(signal.DisplayName);
                }

                string scanLevels = string.Join(",", signalLevels);
                Workspace.This.LogMessage(string.Format("Auto Alignment: Signal levels: {0}", scanLevels));
            });
        }

        private void AutoAlignScanCommand_SmartScanStarting(ThreadBase sender)
        {
            Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
            {
                var imageScanCommand = sender as LaserScanCommand;
                var currentScanParam = imageScanCommand.CurrentScanParam;

                _IsAutoScanCompleted = false;
                _CurrentScanType = ScanType.Auto;

                List<string> signalLevels = new List<string>();
                foreach (var signal in currentScanParam.Signals)
                {
                    signalLevels.Add(signal.DisplayName);
                }

                string scanLevels = string.Join(",", signalLevels);
                Workspace.This.LogMessage(string.Format("Auto Alignment: Starting signal levels: {0}", scanLevels));

            });

        }

        private void AutoAlignScanCommand_Completed(ThreadBase sender, ThreadBase.ThreadExitStat exitState)
        {
            Workspace.This.Owner.Dispatcher.BeginInvoke((Action)delegate
            {
                Workspace.This.StopWaitAnimation();
                Workspace.This.IsPreparingToScan = false;
                Workspace.This.IsReadyScanning = false;
                Workspace.This.IsScanning = false;
                _CurrentScanRegion = int.MaxValue;
                _ImagingVm.IsAdornerEnabled = true;
                // Enable saturation display (in case it's turned off by EDR scanning)
                _ImagingVm.ContrastVm.DisplayImageInfo.IsSaturationChecked = true;
                _IsUpdatingPreviewImage = false;
                _IsAligningPreviewImage = false;

                //Reset status
                Time = 0;
                RemainingTime = 0;
                Workspace.This.StatusTextProgress = string.Empty;
                Workspace.This.PercentCompleted = 0;
                //_IsSmartScanning = false;

                //
                // Clear the unaligned/uncropped image buffer.
                //
                _ChannelL1PrevImageUnAligned = null;
                _ChannelR1PrevImageUnAligned = null;
                _ChannelR2PrevImageUnAligned = null;
                //
                // Clear preview image buffers
                //
                //_ImagingVm.PreviewImage = null;
                //_PreviewImage = null;
                //_ChannelL1PrevImage = null;
                //_ChannelR1PrevImage = null;
                //_ChannelR2PrevImage = null;

                if (_IsAbortedOnLidOpened)
                {
                    IsPreviewChannels = false;  // close preview image contrast window.
                    string caption = "Lid open detected...";
                    string message = "Lid open detected. The scan was terminated.";
                    Xceed.Wpf.Toolkit.MessageBox.Show(Workspace.This.Owner, message, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                var scanningCommand = (sender as LaserScanCommand);
                bool bIsZScanning = false;
                bool bIsSequentialScanning = false;
                if (scanningCommand.CurrentScanParam != null)
                {
                    bIsZScanning = scanningCommand.CurrentScanParam.IsZScanning;
                    bIsSequentialScanning = scanningCommand.CurrentScanParam.IsSequentialScanning;
                }

                if (exitState == ThreadBase.ThreadExitStat.None)
                {
                    #region === Scan Sucessfully Completed ===

                    //L1 = ChannelC
                    //R1 = ChannelA
                    //R2 = ChannelB
                    WriteableBitmap laserL1scannedImage = null;
                    WriteableBitmap laserR1scannedImage = null;
                    WriteableBitmap laserR2scannedImage = null;
                    //WriteableBitmap laserL1RawImage = null;
                    //WriteableBitmap laserR1RawImage = null;
                    //WriteableBitmap laserR2RawImage = null;

                    var signals = scanningCommand.CurrentScanParam.Signals;

                    for (int i = 0; i < signals.Count; i++)
                    {
                        if (signals[i].LaserChannel == LaserChannels.ChannelC)      //L1
                        {
                            laserL1scannedImage = scanningCommand.ChannelCImage.Clone();
                            //if (SettingsManager.ConfigSettings.IsKeepRawImages)
                            //{
                            //    if (scanningCommand.ChannelCRawImage != null)
                            //    {
                            //        laserL1RawImage = scanningCommand.ChannelCRawImage.Clone();
                            //    }
                            //}
                        }
                        else if (signals[i].LaserChannel == LaserChannels.ChannelA) //R1
                        {
                            laserR1scannedImage = scanningCommand.ChannelAImage.Clone();
                            //if (SettingsManager.ConfigSettings.IsKeepRawImages)
                            //{
                            //    if (scanningCommand.ChannelARawImage != null)
                            //    {
                            //        laserR1RawImage = scanningCommand.ChannelARawImage.Clone();
                            //    }
                            //}
                        }
                        else if (signals[i].LaserChannel == LaserChannels.ChannelB) //R2
                        {
                            laserR2scannedImage = scanningCommand.ChannelBImage.Clone();
                            //if (SettingsManager.ConfigSettings.IsKeepRawImages)
                            //{
                            //    if (scanningCommand.ChannelBRawImage != null)
                            //    {
                            //        laserR2RawImage = scanningCommand.ChannelBRawImage.Clone();
                            //    }
                            //}
                        }
                    }

                    var imageInfo = scanningCommand.ImageInfo;
                    var currScanParam = scanningCommand.ScanParams[0];
                    if (imageInfo != null)
                    {
                        imageInfo.CaptureType = "Fluorescence";
                        imageInfo.SoftwareVersion = Workspace.This.ProductVersion;
                        imageInfo.FWVersion = Workspace.This.FWVersion;
                        imageInfo.SystemSN = Workspace.This.SystemSN;
                        string smartScan = (currScanParam.IsSmartScanning) ? "Smartscan" : string.Empty;
                        string seqScan = (currScanParam.IsSequentialScanning) ? "Sequential" : string.Empty;
                        string zScan = (bIsZScanning) ? "Z-Scan" : string.Empty;
                        if (currScanParam.IsEdrScanning)
                        {
                            smartScan = "EDR";
                        }
                        string separator1 = (currScanParam.IsSmartScanning && currScanParam.IsSequentialScanning) ? " + " : string.Empty;
                        string separator2 = ((currScanParam.IsSmartScanning || currScanParam.IsSequentialScanning) && currScanParam.IsZScanning) ? " + " : string.Empty;
                        imageInfo.ScanType = string.Format("{0}{1}{2}{3}{4}", smartScan, separator1, seqScan, separator2, zScan);
                        imageInfo.ScanZ0Abs = Workspace.This.MotorVM.AbsZPos;
                        imageInfo.ScanSpeed = "Highest";
                        // scan region coordinate
                        double x = Math.Round(4.0 * _ImagingVm.CellSize, 2);
                        double y = Math.Round(0.5 * _ImagingVm.CellSize, 2);
                        double w = Math.Round(4.0 * _ImagingVm.CellSize, 2);
                        double h = Math.Round(1.5 * _ImagingVm.CellSize, 2);
                        int x1 = (int)Math.Round(x / _ImagingVm.CellSize);
                        int y1 = (int)Math.Round(y / _ImagingVm.CellSize);
                        int x2 = (int)Math.Round(x1 + (w / _ImagingVm.CellSize));
                        int y2 = (int)Math.Round(y1 + (h / _ImagingVm.CellSize));
                        string row = string.Format("{0}-{1}", Workspace.This.IndexToRow(y1), Workspace.This.IndexToRow(y2));
                        string col = string.Format("{0}-{1}", x1, x2);
                        imageInfo.ScanRegion = row + ", " + col;
                        // User selected signal level
                        if (imageInfo.RedChannel.LaserIntensity > 0) { imageInfo.RedChannel.SignalLevel = Workspace.IntensityLevels[imageInfo.RedChannel.LaserIntensityLevel]; }
                        if (imageInfo.GreenChannel.LaserIntensity > 0) { imageInfo.GreenChannel.SignalLevel = Workspace.IntensityLevels[imageInfo.GreenChannel.LaserIntensityLevel]; }
                        if (imageInfo.BlueChannel.LaserIntensity > 0) { imageInfo.BlueChannel.SignalLevel = Workspace.IntensityLevels[imageInfo.BlueChannel.LaserIntensityLevel]; }
                        if (imageInfo.GrayChannel.LaserIntensity > 0) { imageInfo.GrayChannel.SignalLevel = Workspace.IntensityLevels[imageInfo.GrayChannel.LaserIntensityLevel]; }
                        if (imageInfo.MixChannel.LaserIntensity > 0) { imageInfo.MixChannel.SignalLevel = Workspace.IntensityLevels[imageInfo.MixChannel.LaserIntensityLevel]; }

                        #region === Filter wavelength ===
                        if (imageInfo.RedChannel.LaserIntensity > 0)
                        {
                            int wavelength = 0;
                            if (imageInfo.RedChannel.LaserWavelength == "0")
                            {
                                var laserChannel = _LaserChannelDict[imageInfo.RedChannel.LaserChannel];
                                wavelength = Workspace.This.LaserChannelTypeList[laserChannel];
                                imageInfo.RedChannel.LaserWavelength = wavelength.ToString();
                            }
                            else
                            {
                                wavelength = int.Parse(imageInfo.RedChannel.LaserWavelength);
                            }
                            foreach (var laserType in Workspace.This.LaserOptions)
                            {
                                if (!Workspace.This.IsPhosphorModule(laserType))
                                {
                                    if (laserType.Wavelength == wavelength)
                                    {
                                        imageInfo.RedChannel.FilterWavelength = laserType.Filter;
                                    }
                                }
                            }
                            // Can sometimes have multiple lasers with the save wavelength (638/APD and 638/PMT)
                            //var laserType = Workspace.This.LaserOptions.SingleOrDefault(item => item.Wavelength == int.Parse(imageInfo.RedChannel.LaserWavelength));
                            //imageInfo.RedChannel.FilterWavelength = laserType.Filter;
                        }
                        if (imageInfo.GreenChannel.LaserIntensity > 0)
                        {
                            int wavelength = 0;
                            if (imageInfo.GreenChannel.LaserWavelength == "0")
                            {
                                var laserChannel = _LaserChannelDict[imageInfo.GreenChannel.LaserChannel];
                                wavelength = Workspace.This.LaserChannelTypeList[laserChannel];
                                imageInfo.GreenChannel.LaserWavelength = wavelength.ToString();
                            }
                            else
                            {
                                wavelength = int.Parse(imageInfo.GreenChannel.LaserWavelength);
                            }
                            foreach (var laserType in Workspace.This.LaserOptions)
                            {
                                if (!Workspace.This.IsPhosphorModule(laserType))
                                {
                                    if (laserType.Wavelength == wavelength)
                                    {
                                        imageInfo.GreenChannel.FilterWavelength = laserType.Filter;
                                    }
                                }
                            }
                            // Can sometimes have multiple lasers with the save wavelength (638/APD and 638/PMT)
                            //var laserType = Workspace.This.LaserOptions.SingleOrDefault(item => item.Wavelength == int.Parse(imageInfo.GreenChannel.LaserWavelength));
                            //imageInfo.GreenChannel.FilterWavelength = laserType.Filter;
                        }
                        if (imageInfo.BlueChannel.LaserIntensity > 0)
                        {
                            int wavelength = 0;
                            if (imageInfo.BlueChannel.LaserWavelength == "0")
                            {
                                var laserChannel = _LaserChannelDict[imageInfo.BlueChannel.LaserChannel];
                                wavelength = Workspace.This.LaserChannelTypeList[laserChannel];
                                imageInfo.BlueChannel.LaserWavelength = wavelength.ToString();
                            }
                            else
                            {
                                wavelength = int.Parse(imageInfo.BlueChannel.LaserWavelength);
                            }
                            foreach (var laserType in Workspace.This.LaserOptions)
                            {
                                if (!Workspace.This.IsPhosphorModule(laserType))
                                {
                                    if (laserType.Wavelength == wavelength)
                                    {
                                        imageInfo.BlueChannel.FilterWavelength = laserType.Filter;
                                    }
                                }
                            }
                            // Can sometimes have multiple lasers with the save wavelength (638/APD and 638/PMT)
                            //var laserType = Workspace.This.LaserOptions.SingleOrDefault(item => item.Wavelength == int.Parse(imageInfo.BlueChannel.LaserWavelength));
                            //imageInfo.BlueChannel.FilterWavelength = laserType.Filter;
                        }
                        if (imageInfo.GrayChannel.LaserIntensity > 0)
                        {
                            int wavelength = 0;
                            if (imageInfo.GrayChannel.LaserWavelength == "0")
                            {
                                var laserChannel = _LaserChannelDict[imageInfo.GrayChannel.LaserChannel];
                                wavelength = Workspace.This.LaserChannelTypeList[laserChannel];
                                imageInfo.GrayChannel.LaserWavelength = wavelength.ToString();
                            }
                            else
                            {
                                wavelength = int.Parse(imageInfo.GrayChannel.LaserWavelength);
                            }
                            foreach (var laserType in Workspace.This.LaserOptions)
                            {
                                if (!Workspace.This.IsPhosphorModule(laserType))
                                {
                                    if (laserType.Wavelength == wavelength)
                                    {
                                        imageInfo.GrayChannel.FilterWavelength = laserType.Filter;
                                    }
                                }
                            }
                            // Can sometimes have multiple lasers with the save wavelength (638/APD and 638/PMT)
                            //var laserType = Workspace.This.LaserOptions.SingleOrDefault(item => item.Wavelength == int.Parse(imageInfo.GrayChannel.LaserWavelength));
                            //imageInfo.GrayChannel.FilterWavelength = laserType.Filter;
                        }
                        if (imageInfo.MixChannel.LaserIntensity > 0)
                        {
                            int wavelength = 0;
                            if (imageInfo.MixChannel.LaserWavelength == "0")
                            {
                                var laserChannel = _LaserChannelDict[imageInfo.MixChannel.LaserChannel];
                                wavelength = Workspace.This.LaserChannelTypeList[laserChannel];
                                imageInfo.MixChannel.LaserWavelength = wavelength.ToString();
                            }
                            else
                            {
                                wavelength = int.Parse(imageInfo.MixChannel.LaserWavelength);
                            }
                            foreach (var laserType in Workspace.This.LaserOptions)
                            {
                                if (!Workspace.This.IsPhosphorModule(laserType))
                                {
                                    if (laserType.Wavelength == wavelength)
                                    {
                                        imageInfo.MixChannel.FilterWavelength = laserType.Filter;
                                    }
                                }
                            }
                            // Can sometimes have multiple lasers with the save wavelength (638/APD and 638/PMT)
                            //var laserType = Workspace.This.LaserOptions.SingleOrDefault(item => item.Wavelength == int.Parse(imageInfo.MixChannel.LaserWavelength));
                            //imageInfo.MixChannel.FilterWavelength = laserType.Filter;
                        }
                        #endregion
                    }

                    // Save test images?
                    if (SettingsManager.ConfigSettings.IsSaveAutoAlignImages && !string.IsNullOrEmpty(Workspace.This.AppDataPath))
                    {
                        string dbgTIPath = System.IO.Path.Combine(Workspace.This.AppDataPath, "DBTI");
                        if (System.IO.Directory.Exists(dbgTIPath))
                        {
                            //var dir = new System.IO.DirectoryInfo(dbgTIPath);
                            //foreach (var file in dir.EnumerateFiles("AA*.tif"))
                            //{
                            //    try
                            //    {
                            //        file.Delete();
                            //    }
                            //    catch
                            //    {
                            //    }
                            //}
                            // Delete files that are a month old
                            string[] files = Directory.GetFiles(dbgTIPath);
                            foreach (string file in files)
                            {
                                FileInfo fi = new FileInfo(file);
                                if (fi.CreationTime < DateTime.Now.AddMonths(-1))
                                {
                                    try
                                    {
                                        fi.Delete();
                                    }
                                    catch
                                    {
                                    }
                                }
                            }
                        }
                        else
                        {
                            try
                            {
                                System.IO.Directory.CreateDirectory(dbgTIPath);
                            }
                            catch
                            {
                            }
                        }
                        try
                        {
                            DateTime dt = DateTime.Now;
                            string suffix = string.Format("{0}.{1:D2}.{2:D2}_{3:D2}.{4:D2}.{5:D2}", dt.ToString("yy"), dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                            if (laserL1scannedImage != null)
                            {
                                string fileName = "AA-L1_" + suffix + ".tif";
                                string filePath = Path.Combine(dbgTIPath, fileName);
                                imageInfo.MixChannel = (ImageChannel)imageInfo.RedChannel.Clone();
                                ImagingHelper.SaveFile(filePath, laserL1scannedImage, imageInfo);
                            }
                            if (laserR1scannedImage != null)
                            {
                                string fileName = "AA-R1_" + suffix + ".tif";
                                string filePath = Path.Combine(dbgTIPath, fileName);
                                imageInfo.MixChannel = (ImageChannel)imageInfo.GreenChannel.Clone();
                                ImagingHelper.SaveFile(filePath, laserR1scannedImage, imageInfo);
                            }
                            if (laserR2scannedImage != null)
                            {
                                string fileName = "AA-R2_" + suffix + ".tif";
                                string filePath = Path.Combine(dbgTIPath, fileName);
                                imageInfo.MixChannel = (ImageChannel)imageInfo.BlueChannel.Clone();
                                ImagingHelper.SaveFile(filePath, laserR2scannedImage, imageInfo);
                            }
                        }
                        catch
                        {
                        }
                    }

                    int lcIndexL1 = 0;  //L1 = Red 
                    int lcIndexR1 = 1;  //R1 = Green
                    int lcIndexR2 = 2;  //R2 = Blue
                    int refChanIndex = lcIndexR2;
                    // Now allowing R1 as the reference channel; no longer assuming R2 is always the reference channel
                    if (laserR2scannedImage != null)
                    {
                        refChanIndex = lcIndexR2;   // set R2 as the reference channel
                    }
                    else if (laserR1scannedImage != null)
                    {
                        refChanIndex = lcIndexR1;   // set R1 as the reference channel
                    }
                    int[] laserChannels = new int[] { lcIndexL1, lcIndexR1, lcIndexR2 };
                    WriteableBitmap alignedImage = null;
                    float[] matrixL1 = null;
                    float[] matrixR1 = null;
                    WriteableBitmap[] arrScannedImages = new WriteableBitmap[] { laserL1scannedImage, laserR1scannedImage, laserR2scannedImage };

                    try
                    {
                        Workspace.This.StartWaitAnimation("Calculating alignment parameters.\nPlease wait...");

                        alignedImage = ImageProcessing.GetAlignment(arrScannedImages, laserChannels, refChanIndex, ref matrixL1, ref matrixR1);

                        // Get the 3rd and 6th value in the 2x3 matrix
                        int ldx = 0, ldy = 0, rdx = 0, rdy = 0;
                        if (matrixL1 != null)
                        {
                            ldx = (int)matrixL1[2];
                            ldy = (int)matrixL1[5];
                            //ldx = (int)Math.Round(matrixL1[2]);
                            //ldy = (int)Math.Round(matrixL1[5]);
                        }
                        if (matrixR1 != null)
                        {
                            rdx = (int)matrixR1[2];
                            rdy = (int)matrixR1[5];
                            //rdx = (int)Math.Round(matrixR1[2]);
                            //rdy = (int)Math.Round(matrixR1[5]);
                        }

                        // Send the automatic aligned image to Gallery
                        if (alignedImage != null && SettingsManager.ConfigSettings.IsSendAutoAlignedImageToGallery)
                        {
                            string title = Workspace.This.GetUniqueFilename("Auto-Aligned-Image");
                            Workspace.This.NewDocument(alignedImage, imageInfo, title, false, false);
                        }

                        if (ldx == 0 && ldy == 0 && rdx == 0 && rdy == 0)
                        {
                            Workspace.This.StopWaitAnimation();

                            string caption = "Auto Align...";
                            string message = "The laser modules appeared to be already aligned.";
                            var parent = Application.Current.MainWindow;
                            Xceed.Wpf.Toolkit.MessageBox.Show(parent, message, caption, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                            return;
                        }

                        // Reverse sign of auto-align value
                        int nPixel_10_L_DX = ldx * (-1);
                        int nPixel_10_L_DY = ldy * (-1);
                        int nPixel_10_R2_DX = rdx * (-1);
                        int nPixel_10_R2_DY = rdy * (-1);
                        // reverse sign (the scanned image is automatically flipped vertically by default)
                        if (!SettingsManager.ApplicationSettings.IsVerticalFlipEnabled)
                        {
                            nPixel_10_L_DY *= -1;
                            nPixel_10_R2_DY *= -1;
                        }

                        Workspace.This.LogMessage(string.Format("Pixel_10_L_DX: {0}", nPixel_10_L_DX));
                        Workspace.This.LogMessage(string.Format("Pixel_10_L_DY: {0}", nPixel_10_L_DY));
                        Workspace.This.LogMessage(string.Format("Pixel_10_R1_DX: {0}", nPixel_10_R2_DX));
                        Workspace.This.LogMessage(string.Format("Pixel_10_R1_DY: {0}", nPixel_10_R2_DY));

                        //System.Diagnostics.Trace.WriteLine("[{0}]", string.Join(", ", matrixL1));
                        //System.Diagnostics.Trace.WriteLine("[{0}]", string.Join(", ", matrixR1));

                        bool hasDeviceProperties = true;
                        if (Workspace.This.EthernetController.GetDeviceProperties() == false)
                        {
                            Thread.Sleep(1000);
                            // Get device properties faled. Try again
                            if (Workspace.This.EthernetController.GetDeviceProperties() == false)
                            {
                                Workspace.This.StopWaitAnimation();

                                hasDeviceProperties = false;
                                string caption = "Saving image alignment parameters...";
                                string message = "Unable to read the current system setting parameters from the scanner.";
                                var parent = Application.Current.MainWindow;
                                Xceed.Wpf.Toolkit.MessageBox.Show(parent, message, caption, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                            }
                        }

                        if (hasDeviceProperties)
                        {
                            // Get current params
                            var deviceProperties = Workspace.This.EthernetController.DeviceProperties;
                            deviceProperties.PixelOffsetDxCHL += nPixel_10_L_DX;
                            deviceProperties.PixelOffsetDyCHL += nPixel_10_L_DY;
                            deviceProperties.PixelOffsetDxCHR2 += nPixel_10_R2_DX;
                            deviceProperties.PixelOffsetDyCHR2 += nPixel_10_R2_DY;

                            int tempPixel10LDx = deviceProperties.PixelOffsetDxCHL;
                            int tempPixel10LDy = deviceProperties.PixelOffsetDyCHL;
                            int tempPixel10R2Dx = deviceProperties.PixelOffsetDxCHR2;
                            int tempPixel10R2Dy = deviceProperties.PixelOffsetDyCHR2;

                            try
                            {
                                Workspace.This.StopWaitAnimation();
                                Workspace.This.StartWaitAnimation("Saving image alignment parameters...");
                                if (Workspace.This.EthernetController.SetDeviceProperties(deviceProperties) == false)
                                {
                                    Workspace.This.StopWaitAnimation();

                                    string caption = "Saving image alignment parameters...";
                                    string message = "Unable to write the alignment parameters to the scanner.";
                                    var parent = Application.Current.MainWindow;
                                    Xceed.Wpf.Toolkit.MessageBox.Show(parent, message, caption, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                                }
                                else
                                {
                                    // Wait 3 seconds before reading back the written values.
                                    ImagingSystem.ImagingHelper.Delay(3000);

                                    // Read back and verify the saved parameters.
                                    if (Workspace.This.EthernetController.GetDeviceProperties() == false)
                                    {
                                        Workspace.This.StopWaitAnimation();

                                        string caption = "Image alignment parameters...";
                                        string message = "Unable to read back and verify the image alignment parameters.";
                                        var parent = Application.Current.MainWindow;
                                        Xceed.Wpf.Toolkit.MessageBox.Show(parent, message, caption, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                                        return;
                                    }
                                    else
                                    {
                                        Workspace.This.StopWaitAnimation();

                                        // Verify the saved parameters
                                        if (tempPixel10LDx == Workspace.This.EthernetController.DeviceProperties.PixelOffsetDxCHL &&
                                            tempPixel10LDy == Workspace.This.EthernetController.DeviceProperties.PixelOffsetDyCHL &&
                                            tempPixel10R2Dx == Workspace.This.EthernetController.DeviceProperties.PixelOffsetDxCHR2 &&
                                            tempPixel10R2Dy == Workspace.This.EthernetController.DeviceProperties.PixelOffsetDyCHR2)
                                        {
                                            // Verified: parameters successfully saved
                                            string caption = "Save image alignment parameters...";
                                            string message = "Successfully saved the alignment parameters.";
                                            var parent = Application.Current.MainWindow;
                                            Xceed.Wpf.Toolkit.MessageBox.Show(parent, message, caption, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                                        }
                                        else
                                        {
                                            string caption = "Save image alignment parameters...";
                                            string message = "Unable to read back and verify the alignment parameters.";
                                            var parent = Application.Current.MainWindow;
                                            Xceed.Wpf.Toolkit.MessageBox.Show(parent, message, caption, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Workspace.This.StopWaitAnimation();
                                string caption = "Save image alignment parameters...";
                                string message = "Alignment parameters saving error:\n" + ex.Message;
                                var parent = Application.Current.MainWindow;
                                Xceed.Wpf.Toolkit.MessageBox.Show(parent, message, caption, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                            }
                            finally
                            {
                                Workspace.This.StopWaitAnimation();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Workspace.This.StopWaitAnimation();
                        string caption = "Get image alignment parameters";
                        string message = "Error calculating alignment parameters.\n" + ex.Message;
                        var parent = Application.Current.MainWindow;
                        Xceed.Wpf.Toolkit.MessageBox.Show(parent, message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                        return;
                    }

                    #endregion
                }
                else if (exitState == ThreadBase.ThreadExitStat.Error)
                {
                    #region === Scan Error ===

                    // Oh oh something went wrong - handle the error

                    //
                    // Clear preview image buffers
                    //
                    _ImagingVm.PreviewImage = null;
                    _PreviewImage = null;
                    _ChannelL1PrevImage = null;
                    _ChannelR1PrevImage = null;
                    _ChannelR2PrevImage = null;

                    if (_PreviewContrastWindow != null && _PreviewContrastWindow.IsLoaded)
                    {
                        // Close preview channels/contrast window
                        _PreviewContrastWindow.Close();
                        _PreviewContrastWindow = null;
                    }

                    // Error occurred but not 'lid opened' error (lid opened error handle above, to allow the option to save the scanned data)
                    if (!_IsAbortedOnLidOpened)
                    {
                        string caption = "Auto Alignment: Scanning error...";
                        string message = ((ThreadBase)scanningCommand).Error.Message + "\n\nStack Trace:\n" + ((ThreadBase)scanningCommand).Error.StackTrace;
                        Workspace.This.LogMessage(message);
                        var parent = Application.Current.MainWindow;
                        Xceed.Wpf.Toolkit.MessageBox.Show(parent, message, caption, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }

                    #endregion
                }

                _ScanImageBaseFilename.Clear();
                _IsSaveScanDataOnAborted = false;
                _IsAbortedOnLidOpened = false;
                if (_CurrentScanType != ScanType.Preview)
                {
                    _IsPrescanCompleted = false;
                }

                //Turn off all the lasers after scanning is completed
                Workspace.This.TurnOffAllLasers();

                _AutoAlignScanCommand.OnScanDataReceived -= AutoAlignScanCommand_OnScanDataReceived;
                _AutoAlignScanCommand.Completed -= AutoAlignScanCommand_Completed;
                _AutoAlignScanCommand.SmartScanStarting -= AutoAlignScanCommand_SmartScanStarting;
                _AutoAlignScanCommand.SmartScanUpdated -= AutoAlignScanCommand_SmartScanUpdated;
                _AutoAlignScanCommand.SmartScanCompleted -= AutoAlignScanCommand_SmartScanCompleted;
                if (SettingsManager.ConfigSettings.IsSimulationMode)
                {
                    _ImageScanCommand.CompletionEstimate -= ImageScanCommand_CompletionEstimate;
                }

                // Reset Smartscan type
                _IsAutoScan = false;
                _IsSmartScanning = false;
                _ImageScanCommand = null;

                // Did scan region changed while scanning? Get the modified scan region
                if (_ImagingVm.CropServices != null && _IsScanRegionChanged)
                {
                    if (_ImagingVm.CellSize > 0)
                    {
                        foreach (var scanRegion in SelectedAppProtocol.ScanRegions)
                        {
                            if (scanRegion != null)
                            {
                                foreach (var cropService in _ImagingVm.CropServices)
                                {
                                    if (cropService.AdornerID == scanRegion.ScanRegionNum)
                                    {
                                        scanRegion.IsScanRectDragged = true;  // dragged vs manually entered value
                                        var cropArea = cropService.GetCroppedArea();
                                        double x = Math.Round(cropArea.CroppedRectAbsolute.X / _ImagingVm.CellSize, 2);
                                        double y = Math.Round(cropArea.CroppedRectAbsolute.Y / _ImagingVm.CellSize, 2);
                                        double w = Math.Round(cropArea.CroppedRectAbsolute.Width / _ImagingVm.CellSize, 2);
                                        double h = Math.Round(cropArea.CroppedRectAbsolute.Height / _ImagingVm.CellSize, 2);
                                        if (x < 0) { x = 0; }
                                        if (y < 0) { y = 0; }
                                        if (w > 25) { w = 25; }
                                        if (h > 25) { h = 25; }
                                        if (x + w > 25) { x -= (x + w) - 25; }
                                        if (y + h > 25) { y -= (y + h) - 25; }
                                        scanRegion.X = x;
                                        scanRegion.Y = y;
                                        scanRegion.Width = w;
                                        scanRegion.Height = h;
                                        scanRegion.IsScanRectDragged = false;
                                        break;
                                    }
                                }
                            }
                        }
                        _IsScanRegionChanged = false;
                    }
                }

                // Clear the unaligned/uncropped image buffer.
                //_ChannelAPrevImageUnAligned = null;
                //_ChannelBPrevImageUnAligned = null;
                //_ChannelCPrevImageUnAligned = null;

                // force a garbage collection to free 
                // up memory as quickly as possible.
                //GC.Collect();
            });
        }

        #endregion


        #region === Helper methods ===

        /// <summary>
        /// Returns sample type based on sample type position.
        /// </summary>
        /// <param name="nSampleTypePos"></param>
        /// <returns></returns>
        //private SampleTypeSetting GetSampleType(int nSampleTypePos)
        //{
        //    SampleTypeSetting result = null;
        //    if (this.SampleTypeOptions != null && this.SampleTypeOptions.Count > 0)
        //    {
        //        for (int i = 0; i < this.SampleTypeOptions.Count; i++)
        //        {
        //            if (this.SampleTypeOptions[i].Position == nSampleTypePos)
        //            {
        //                result = this.SampleTypeOptions[i];
        //                break;
        //            }
        //        }
        //    }
        //    return result;
        //}

        /// <summary>
        /// Returns dye type based on dye type position.
        /// </summary>
        /// <param name="nSampleTypePos"></param>
        /// <returns></returns>
        /*private DyeType GetDyeType(ObservableCollection<DyeType> dyeTypeOptions, int nDyeTypePos)
        {
            DyeType result = null;
            if (dyeTypeOptions != null && dyeTypeOptions.Count > 0)
            {
                for (int i = 0; i < dyeTypeOptions.Count; i++)
                {
                    if (dyeTypeOptions[i].Position == nDyeTypePos)
                    {
                        result = dyeTypeOptions[i];
                        break;
                    }
                }
            }
            return result;
        }*/

        /// <summary>
        /// Save the selected protocol to config file (protocols.xml)
        /// </summary>
        /// <param name="protocolToBeSave"></param>
        /// <param name="saveasProtocolName"></param>
        public bool SaveAppProtocol(ProtocolViewModel protocolToBeSave, string saveasProtocolName)
        {
            string filePath = System.IO.Path.Combine(Workspace.This.AppDataPath, _ProtocolConfigFile);

            XmlDocument XDoc = new XmlDocument();
            //XDoc.PreserveWhitespace = true;   //this flag override auto indent settings
            XmlNode protocolsParentNode = null;
            bool bUserConfigExists = false;

            if (System.IO.File.Exists(filePath))
            {
                XDoc.Load(filePath);
                XmlNodeList nodeList = XDoc.GetElementsByTagName("Protocols");
                protocolsParentNode = nodeList.Item(0);
                bUserConfigExists = true;
            }

            if (!bUserConfigExists)
            {
                var comment = XDoc.CreateComment("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
                XmlNode rootNode = XDoc.CreateElement("Config");
                XDoc.InsertBefore(comment, rootNode);
                XDoc.AppendChild(rootNode);

                protocolsParentNode = XDoc.CreateElement("Protocols");
                rootNode.AppendChild(protocolsParentNode);
            }

            XmlNode protocolNode = XDoc.CreateElement("Protocol");
            protocolsParentNode.AppendChild(protocolNode);

            XmlAttribute xmlAttrib = XDoc.CreateAttribute("DisplayName");
            xmlAttrib.Value = saveasProtocolName; //new protocol name
            protocolNode.Attributes.Append(xmlAttrib);

            foreach (var scanRegion in protocolToBeSave.ScanRegions)
            {
                if (scanRegion.SelectedPixelSize == null ||
                    scanRegion.SelectedSampleType == null ||
                    scanRegion.SelectedScanSpeed == null)
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show(string.Format("Please make sure all the scanning options are selected [{0}].", scanRegion.ScanRegionHeader));
                    return false;
                }

                XmlNode scanRegionNode = XDoc.CreateElement("ScanRegion");
                protocolNode.AppendChild(scanRegionNode);

                xmlAttrib = XDoc.CreateAttribute("SampleType");
                xmlAttrib.Value = scanRegion.SelectedSampleType.Position.ToString();
                scanRegionNode.Attributes.Append(xmlAttrib);

                xmlAttrib = XDoc.CreateAttribute("PixelSize");
                xmlAttrib.Value = scanRegion.SelectedPixelSize.Position.ToString();
                scanRegionNode.Attributes.Append(xmlAttrib);

                xmlAttrib = XDoc.CreateAttribute("ScanSpeed");
                xmlAttrib.Value = scanRegion.SelectedScanSpeed.Position.ToString();
                scanRegionNode.Attributes.Append(xmlAttrib);

                xmlAttrib = XDoc.CreateAttribute("ScanQuality");
                xmlAttrib.Value = scanRegion.SelectedScanQuality.Position.ToString();
                scanRegionNode.Attributes.Append(xmlAttrib);

                xmlAttrib = XDoc.CreateAttribute("ScanRegionRect");
                double x = Math.Round(scanRegion.ScanRect.X / _ImagingVm.CellSize, 2);
                double y = Math.Round(scanRegion.ScanRect.Y / _ImagingVm.CellSize, 2);
                double w = Math.Round(scanRegion.ScanRect.Width / _ImagingVm.CellSize, 2);
                double h = Math.Round(scanRegion.ScanRect.Height / _ImagingVm.CellSize, 2);
                ScanRegionRect scanRect = new ScanRegionRect(x, y, w, h);
                xmlAttrib.Value = scanRect.ToString();
                scanRegionNode.Attributes.Append(xmlAttrib);

                if (scanRegion.FileLocationVm.IsAutoSave)
                {
                    xmlAttrib = XDoc.CreateAttribute("IsAutoSave");
                    xmlAttrib.Value = scanRegion.FileLocationVm.IsAutoSave.ToString();
                    scanRegionNode.Attributes.Append(xmlAttrib);
                }

                if (scanRegion.IsCustomFocus)
                {
                    xmlAttrib = XDoc.CreateAttribute("IsCustomFocus");
                    xmlAttrib.Value = scanRegion.IsCustomFocus.ToString();
                    scanRegionNode.Attributes.Append(xmlAttrib);
                    xmlAttrib = XDoc.CreateAttribute("CustomFocus");
                    xmlAttrib.Value = scanRegion.CustomFocusValue.ToString();
                    scanRegionNode.Attributes.Append(xmlAttrib);
                }
                else if (scanRegion.IsZScan)
                {
                    xmlAttrib = XDoc.CreateAttribute("IsZScan");
                    xmlAttrib.Value = scanRegion.IsZScan.ToString();
                    scanRegionNode.Attributes.Append(xmlAttrib);
                }

                for (int index = 0; index < scanRegion.SignalList.Count; index++)
                {
                    SignalViewModel signal = scanRegion.SignalList[index];

                    if (signal != null && signal.SelectedLaser != null &&
                                          signal.SelectedSignalLevel != null &&
                                          signal.SelectedColorChannel != null)
                    {
                        XmlNode dyeNode = XDoc.CreateElement("Laser");
                        scanRegionNode.AppendChild(dyeNode);
                        xmlAttrib = XDoc.CreateAttribute("LaserType");
                        xmlAttrib.Value = signal.SelectedLaser.Wavelength.ToString();
                        dyeNode.Attributes.Append(xmlAttrib);
                        xmlAttrib = XDoc.CreateAttribute("SignalIntensity");
                        xmlAttrib.Value = signal.SelectedSignalLevel.IntensityLevel.ToString();
                        dyeNode.Attributes.Append(xmlAttrib);
                        xmlAttrib = XDoc.CreateAttribute("ColorChannel");
                        xmlAttrib.Value = signal.SelectedColorChannel.DisplayName;
                        dyeNode.Attributes.Append(xmlAttrib);
                    }
                    else
                    {
                        Xceed.Wpf.Toolkit.MessageBox.Show(string.Format("Please make sure all the options are selected [{0}].", scanRegion.ScanRegionHeader));
                        return false;
                    }
                }

                // Save AutoSave destination folder/path
                if (scanRegion.FileLocationVm.IsAutoSave)
                {
                    XmlNode nodeAutoSave = XDoc.CreateElement("AutoSave");
                    scanRegionNode.AppendChild(nodeAutoSave);
                    xmlAttrib = XDoc.CreateAttribute("Path");
                    xmlAttrib.Value = scanRegion.FileLocationVm.DestinationFolder;
                    nodeAutoSave.Attributes.Append(xmlAttrib);
                }

                // Save z-scanning settings
                if (scanRegion.IsZScan)
                {
                    XmlNode dyeNode = XDoc.CreateElement("ZScan");
                    scanRegionNode.AppendChild(dyeNode);
                    xmlAttrib = XDoc.CreateAttribute("BottomImageFocus");
                    xmlAttrib.Value = scanRegion.ZScanSetting.BottomImageFocus.ToString();
                    dyeNode.Attributes.Append(xmlAttrib);
                    xmlAttrib = XDoc.CreateAttribute("DeltaFocus");
                    xmlAttrib.Value = scanRegion.ZScanSetting.DeltaFocus.ToString();
                    dyeNode.Attributes.Append(xmlAttrib);
                    xmlAttrib = XDoc.CreateAttribute("NumOfImages");
                    xmlAttrib.Value = scanRegion.ZScanSetting.NumOfImages.ToString();
                    dyeNode.Attributes.Append(xmlAttrib);
                }
            }   //closing: foreach

            XDoc.Beautify();
            XDoc.Save(filePath);

            return true;
        }

        /// <summary>
        /// Delete selected protocol from config file (protocols.xml)
        /// </summary>
        /// <param name="selectedProtocol"></param>
        public void DeleteAppProtocol(string selectedProtocol)
        {
            string configFilePath = System.IO.Path.Combine(Workspace.This.AppDataPath, _ProtocolConfigFile);
            bool bIsProtocolRemoved = false;

            // find and remove from 'config.xml'
            if (System.IO.File.Exists(configFilePath))
            {
                XmlDocument doc = new XmlDocument();
                //doc.PreserveWhitespace = true;    //this flag override auto indent settings
                doc.Load(configFilePath);

                XmlNodeList elemList = doc.GetElementsByTagName("Protocol");
                for (int i = 0; i < elemList.Count; i++)
                {
                    string attrVal = elemList[i].Attributes["DisplayName"].Value;
                    if (attrVal == selectedProtocol)
                    {
                        elemList[i].ParentNode.RemoveChild(elemList[i]);
                        bIsProtocolRemoved = true;
                        break;
                    }
                }

                if (bIsProtocolRemoved)
                {
                    doc.Beautify();
                    doc.Save(configFilePath);
                    string caption = "Protocol deleted...";
                    string message = string.Format("Protocol \"{0}\" deleted.", selectedProtocol);
                    Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        /// <summary>
        /// Set the preview channels and allocate preview image buffers
        /// </summary>
        /// <param name="scanParam"></param>
        /// <param name="nScanRegion"></param>
        private void PreviewImageSetup(ScanParameterStruct scanParam, int nScanRegion)
        {
            // Calcuate preview image (aligned and unalighned) width and height.
            //
            int previewImageWidth = 0;
            int previewImageHeight = 0;
            int width = scanParam.Width;
            int height = scanParam.Height;

            if (scanParam.IsUnidirectionalScan)
            {
                // The height was doubled on unidirectional scan to synchronize the scanner's LED progress bar
                height /= 2;
            }

            //int width = (int)(scanParam.ScanDeltaX / scanParam.XMotorSubdivision * 1000.0 / scanParam.Res);
            //int height = (int)(scanParam.ScanDeltaY / scanParam.YMotorSubdivision * 1000.0 / scanParam.Res);
            //TODO: temporary work-around: make sure the width is an even value to avoid the skewed on the scanned image
            //if (width % 2 != 0)
            //{
            //    int deltaX = scanParam.ScanDeltaX - (int)(scanParam.Res / 1000.0 * scanParam.XMotorSubdivision);
            //    scanParam.ScanDeltaX = deltaX;
            //    scanParam.Width = (int)(scanParam.ScanDeltaX / scanParam.XMotorSubdivision * 1000.0 / scanParam.Res);
            //    scanParam.Width--;
            //    scanParam.ScanDeltaX = (int)(scanParam.Width * scanParam.Res / 1000.0 * _XMotorSubdivision);
            //}
            //int offsetDeltaXPulse = 0;
            _IsPreviewSetupCompleted = false;
            _IsAligningPreviewImage = false;
            _IsUpdatingPreviewImage = false;
            bool bIsZScanning = scanParam.IsZScanning;
            bool bIsSequentialScanning = scanParam.IsSequentialScanning;

            /*if (Workspace.This.EthernetController != null && Workspace.This.EthernetController.IsConnected)
            {
                //（因为在设计中将R1透镜的位置和R2透镜的位置更换了，然后参数名没有更改成对应的通道，所以下面参数名R1其实是R2,R2其实是R1）....
                // (in the design, the position of R1 lens and R2 lens is changed, and the parameter name is not changed to the corresponding channel, so the following parameter name R1 is actually R2, R2 is actually R1)....
                //L透镜到R2透镜之间的距离（mm），毫米为单位
                //The distance between L lens and R2 lens (mm), in millimeters
                //int OpticalL_R1Distance = (int)Workspace.This.NewParameterVM.OpticalL_R1Distance;
                //int opticalL_R1Distance = (int)Workspace.This.EthernetController.DeviceProperties.OpticalLR1Distance;

                //nOffsetWidth = opticalL_R1Distance * 1000 / scanParam.Res;

                //根据当前选择的分辨率计算L透镜到R2透镜之间的实际像素宽度
                // Calculate the actual pixel width between L lens and R2 lens based on the currently selected resolution
                double offsetWidth = scanParam.AlignmentParam.OpticalL_R1Distance * 1000.0 / scanParam.Res;

                previewImageWidth = width - (int)Math.Round(offsetWidth);
                previewImageHeight = height;

                //ScanDeltaX adjustment for the distance (mm) between L and R2 lens
                //if (scanParam.AlignmentParam.OpticalL_R1Distance > 0 && scanParam.Res == 150)
                //{
                //    offsetDeltaXPulse = (int)scanParam.AlignmentParam.OpticalL_R1Distance - 1;
                //}
                //else
                //{
                //    offsetDeltaXPulse = (int)scanParam.AlignmentParam.OpticalL_R1Distance;
                //}
                //offsetDeltaXPulse = (int)(offsetDeltaXPulse * scanParam.XMotorSubdivision);

            }
            else if (SettingsManager.ConfigSettings.IsSimulationMode)
            {
                previewImageWidth = width;
                previewImageHeight = height;
            }
            else
            {
                throw new Exception("Scanner not connected.");
            }*/

            previewImageWidth = width;
            previewImageHeight = height;
            if (!SettingsManager.ConfigSettings.IsSimulationMode)
            {
                // Calculate the actual pixel width between L lens and R2 lens based on the currently selected resolution
                int opticalDist = (int)(scanParam.AlignmentParam.OpticalL_R1Distance * 1000.0 / scanParam.Res);
                // Calculate the preview image width and height
                // With X overscan compensation (using the same amount of overscan for X and Y direction)
                previewImageWidth = width - opticalDist - (int)(scanParam.AlignmentParam.YMotionExtraMoveLength * 1000.0 / scanParam.Res);
                // Without X overscan compensation
                //previewImageWidth = width - opticalDist;
                // With Y overscan compensation
                previewImageHeight = height - (int)(scanParam.AlignmentParam.YMotionExtraMoveLength * 1000.0 / scanParam.Res);
            }

            // If actual and aligned/cropped image size are the same; don't align/crop the images.
            //
            _IsCropPreviewImage = false;
            if (previewImageWidth != width || previewImageHeight != height)
            {
                _IsCropPreviewImage = true;
            }

            //scanParam.Width = width;
            //scanParam.Height = height;

            if (width <= 0 || height <= 0 || previewImageWidth <= 0 || previewImageHeight <= 0)
            {
                string caption = "Sapphire FL Biomolecular Imager";
                string message = string.Format("Scan Region #{0}\nInvalid scan ROI. Please re-select the scan area.", nScanRegion + 1);
                Xceed.Wpf.Toolkit.MessageBox.Show(Workspace.This.Owner, message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                throw new Exception("Invalid scan region ROI.");
            }

            // Automatically check all preview the channels
            _ImagingVm.IsLaserL1PrvSelected = false;
            _ImagingVm.IsLaserR1PrvSelected = false;
            _ImagingVm.IsLaserR2PrvSelected = false;
            _ImagingVm.IsContrastLaserL1Channel = false;
            _ImagingVm.IsContrastLaserR1Channel = false;
            _ImagingVm.IsContrastLaserR2Channel = false;
            _IsUpdatingPreviewImage = false;
            _ImagingVm.ContrastVm.DisplayImageInfo.IsContrastRedChannel = false;
            _ImagingVm.ContrastVm.DisplayImageInfo.IsContrastGreenChannel = false;
            _ImagingVm.ContrastVm.DisplayImageInfo.IsContrastBlueChannel = false;
            _ImagingVm.ContrastVm.DisplayImageInfo.IsContrastGrayChannel = false;
            _ImagingVm.ContrastVm.DisplayImageInfo.IsDisplayRedChannel = false;
            _ImagingVm.ContrastVm.DisplayImageInfo.IsDisplayGreenChannel = false;
            _ImagingVm.ContrastVm.DisplayImageInfo.IsDisplayBlueChannel = false;
            _ImagingVm.ContrastVm.DisplayImageInfo.IsDisplayGrayChannel = false;

            _ChannelL1PrevImage = null;
            _ChannelL1PrevImageUnAligned = null;
            _ChannelR1PrevImage = null;
            _ChannelR1PrevImageUnAligned = null;
            _ChannelR2PrevImage = null;
            _ChannelR2PrevImageUnAligned = null;
            _PreviewImage = null;

            _Is4channelImage = false;
            _IsGrayscaleImage = false;

            foreach (var signal in scanParam.Signals)
            {
                if (signal.ColorChannel == (int)ImageChannelType.Red)
                {
                    _ImagingVm.ContrastVm.DisplayImageInfo.IsContrastRedChannel = true;
                    _ImagingVm.ContrastVm.DisplayImageInfo.IsDisplayRedChannel = true;
                }
                else if (signal.ColorChannel == (int)ImageChannelType.Green)
                {
                    _ImagingVm.ContrastVm.DisplayImageInfo.IsContrastGreenChannel = true;
                    _ImagingVm.ContrastVm.DisplayImageInfo.IsDisplayGreenChannel = true;
                }
                else if (signal.ColorChannel == (int)ImageChannelType.Blue)
                {
                    _ImagingVm.ContrastVm.DisplayImageInfo.IsContrastBlueChannel = true;
                    _ImagingVm.ContrastVm.DisplayImageInfo.IsDisplayBlueChannel = true;
                }
                else if (signal.ColorChannel == (int)ImageChannelType.Gray)
                {
                    _ImagingVm.ContrastVm.DisplayImageInfo.IsContrastGrayChannel = true;
                    _ImagingVm.ContrastVm.DisplayImageInfo.IsDisplayGrayChannel = true;
                    if (scanParam.Signals.Count > 1)
                    {
                        _Is4channelImage = true;
                    }
                    else
                    {
                        _IsGrayscaleImage = true;
                    }
                }

                if (signal.LaserChannel == LaserChannels.ChannelC)      //L1
                {
                    //_ImagingVm.PreviewChannels.Add(LaserType.LaserA);
                    _LaserL1ColorChannel = (ImageChannelType)signal.ColorChannel;
                    _ImagingVm.LaserL1ColorChannel = (ImageChannelType)signal.ColorChannel;
                    _ImagingVm.IsLaserL1PrvSelected = true;
                    _ImagingVm.IsContrastLaserL1Channel = true;
                    if (_IsCropPreviewImage)
                    {
                        _ChannelL1PrevImage = new WriteableBitmap(previewImageWidth, previewImageHeight, 96, 96, PixelFormats.Gray16, null);
                        _ChannelL1PrevImageUnAligned = new WriteableBitmap(width, height, 96, 96, PixelFormats.Gray16, null);
                        scanParam.BackBufferStride = _ChannelL1PrevImageUnAligned.BackBufferStride;
                        _ImageScanCommand.ChannelCBackBuffer = _ChannelL1PrevImageUnAligned.BackBuffer;
                    }
                    else
                    {
                        _ChannelL1PrevImage = new WriteableBitmap(width, height, 96, 96, PixelFormats.Gray16, null);
                    }
                }
                else if (signal.LaserChannel == LaserChannels.ChannelA)  //R1
                {
                    //_ImagingVm.PreviewChannels.Add(LaserType.LaserB);
                    _LaserR1ColorChannel = (ImageChannelType)signal.ColorChannel;
                    _ImagingVm.LaserR1ColorChannel = (ImageChannelType)signal.ColorChannel;
                    _ImagingVm.IsLaserR1PrvSelected = true;
                    _ImagingVm.IsContrastLaserR1Channel = true;
                    if (_IsCropPreviewImage)
                    {
                        _ChannelR1PrevImage = new WriteableBitmap(previewImageWidth, previewImageHeight, 96, 96, PixelFormats.Gray16, null);
                        _ChannelR1PrevImageUnAligned = new WriteableBitmap(width, height, 96, 96, PixelFormats.Gray16, null);
                        scanParam.BackBufferStride = _ChannelR1PrevImageUnAligned.BackBufferStride;
                        _ImageScanCommand.ChannelABackBuffer = _ChannelR1PrevImageUnAligned.BackBuffer;
                    }
                    else
                    {
                        _ChannelR1PrevImage = new WriteableBitmap(width, height, 96, 96, PixelFormats.Gray16, null);
                    }
                }
                else if (signal.LaserChannel == LaserChannels.ChannelB)  //R2
                {
                    //_ImagingVm.PreviewChannels.Add(LaserType.LaserC);
                    _LaserR2ColorChannel = (ImageChannelType)signal.ColorChannel;
                    _ImagingVm.LaserR2ColorChannel = (ImageChannelType)signal.ColorChannel;
                    _ImagingVm.IsLaserR2PrvSelected = true;
                    _ImagingVm.IsContrastLaserR2Channel = true;
                    if (_IsCropPreviewImage)
                    {
                        _ChannelR2PrevImage = new WriteableBitmap(previewImageWidth, previewImageHeight, 96, 96, PixelFormats.Gray16, null);
                        _ChannelR2PrevImageUnAligned = new WriteableBitmap(width, height, 96, 96, PixelFormats.Gray16, null);
                        scanParam.BackBufferStride = _ChannelR2PrevImageUnAligned.BackBufferStride;
                        _ImageScanCommand.ChannelBBackBuffer = _ChannelR2PrevImageUnAligned.BackBuffer;
                    }
                    else
                    {
                        _ChannelR2PrevImage = new WriteableBitmap(width, height, 96, 96, PixelFormats.Gray16, null);
                    }
                }
            }   //foreach Signals

            // Allocate preview display image buffer
            if (_IsGrayscaleImage)
            {
                //if ((previewImageWidth * previewImageHeight) > (20000 * 20000))
                //{
                //    //_PreviewImage = null;
                //    BitmapPalette pal = new BitmapPalette(ImageProcessing.GetColorTableIndexed(true));
                //    _PreviewImage = new WriteableBitmap(previewImageWidth, previewImageHeight, 96, 96, PixelFormats.Indexed8, pal);
                //}
                //else
                //{
                //    _PreviewImage = new WriteableBitmap(previewImageWidth, previewImageHeight, 96, 96, PixelFormats.Rgb24, null);
                //}
                BitmapPalette pal = new BitmapPalette(ImageProcessing.GetColorTableIndexed(true));
                _PreviewImage = new WriteableBitmap(previewImageWidth, previewImageHeight, 96, 96, PixelFormats.Indexed8, pal);
                _ImagingVm.ContrastVm.NumOfChannels = 1;
                _ImagingVm.ContrastVm.DisplayImageInfo.NumOfChannels = 1;
            }
            else
            {
                //if (_PreviewImage == null || _PreviewImage.Format != PixelFormats.Rgb24)
                //{
                //    _PreviewImage = null;
                //    _PreviewImage = new WriteableBitmap(previewImageWidth, previewImageHeight, 96, 96, PixelFormats.Rgb24, null);
                //}
                _PreviewImage = new WriteableBitmap(previewImageWidth, previewImageHeight, 96, 96, PixelFormats.Rgb24, null);

                if (_Is4channelImage)
                {
                    _ImagingVm.ContrastVm.NumOfChannels = 4;
                    _ImagingVm.ContrastVm.DisplayImageInfo.NumOfChannels = 4;
                }
                else
                {
                    _ImagingVm.ContrastVm.NumOfChannels = 3;
                    _ImagingVm.ContrastVm.DisplayImageInfo.NumOfChannels = 3;
                }

                //
                // Don't merge the images if total image size after a channel merges is > 2GB
                //
                //double totalFileSize = GetFileSizeInBytes();
                //_IsAutoMergeImages = true;
                //if (totalFileSize > 2100000000)
                //{
                //    _IsAutoMergeImages = false;
                //
                //    string caption = "File size to large...";
                //    string message = "Image size is too large. Scanned image will appear and save as individual channnels.";
                //    Xceed.Wpf.Toolkit.MessageBox.Show(message, caption);
                //}
            }

            // Preview contrast window settings.
            if (scanParam.Signals.Count == 1)
            {
                _ImagingVm.IsContrastChannelAllowed = false;    // Single channel don't display the contrast channel checkbox
            }
            else
            {
                _ImagingVm.IsContrastChannelAllowed = true;
            }
            _ImagingVm.ContrastVm.NumOfChannels = scanParam.Signals.Count;
            _ImagingVm.PreviewImage = _PreviewImage;
            //_ImagingVm.PreviewImages.Clear();

            // Set preview image coordinate
            //double x = (scanParam.ScanX0 - (int)(Workspace.This.MotorVM.AbsXPos * scanParam.XMotorSubdivision)) / (double)scanParam.XMotorSubdivision * ImagingVm.CellSize * ImagingVm.NumOfCells / _ScanWidthInMm;
            //double y = (scanParam.ScanY0 - (int)(Workspace.This.MotorVM.AbsYPos * scanParam.YMotorSubdivision)) / (double)scanParam.YMotorSubdivision * ImagingVm.CellSize * ImagingVm.NumOfCells / _ScanHeightInMm;
            //double w = (scanParam.ScanDeltaX - offsetDeltaXPulse) * ImagingVm.CellSize * ImagingVm.NumOfCells / _ScanWidthInMm / scanParam.XMotorSubdivision;
            //double h = scanParam.ScanDeltaY * ImagingVm.CellSize * ImagingVm.NumOfCells / _ScanHeightInMm / scanParam.YMotorSubdivision;
            //var imageItem = new ImageItem();
            //imageItem.Top = y;
            //imageItem.Left = x;
            //imageItem.Width = w;
            //imageItem.Height = h;
            //imageItem.Image = _PreviewImage;
            //imageItem.RegionNumber = nScanRegion;
            var imageItem = new ImageItem();
            var sr = _SelectedAppProtocol.ScanRegions[nScanRegion];
            imageItem.Top = sr.ScanRect.Y;
            imageItem.Left = sr.ScanRect.X;
            imageItem.Width = sr.ScanRect.Width;
            imageItem.Height = sr.ScanRect.Height;
            imageItem.Image = _PreviewImage;
            imageItem.RegionNumber = sr.ScanRegionNum;
            for (int index = _ImagingVm.PreviewImages.Count - 1; index >= 0; index--)
            {
                //if (_ImagingVm.PreviewImages[index].Top == y && _ImagingVm.PreviewImages[index].Left == x &&
                //    _ImagingVm.PreviewImages[index].Width == w && _ImagingVm.PreviewImages[index].Height == h)
                //{
                //    _ImagingVm.PreviewImages.Remove(_ImagingVm.PreviewImages[index]);
                //    break;
                //}
                //if (_ImagingVm.PreviewImages[index].RegionNumber == nScanRegion &&
                //    _ImagingVm.PreviewImages[index].Top == sr.ScanRect.Y && _ImagingVm.PreviewImages[index].Left == sr.ScanRect.X &&
                //    _ImagingVm.PreviewImages[index].Width == sr.ScanRect.Width && _ImagingVm.PreviewImages[index].Height == sr.ScanRect.Height)
                //{
                //    _ImagingVm.PreviewImages.Remove(_ImagingVm.PreviewImages[index]);
                //    break;
                //}
                // Scan region tab's region number is 1 based
                if (_ImagingVm.PreviewImages[index].RegionNumber == nScanRegion + 1)
                {
                    _ImagingVm.PreviewImages.Remove(_ImagingVm.PreviewImages[index]);
                    break;
                }
            }
            _ImagingVm.PreviewImages.Add(imageItem);

            Workspace.This.EstimatedCaptureTime = Time = scanParam.Time;  // Estimated scan time for the scan region

            _IsPreviewSetupCompleted = true;
        }

        internal void UpdateScanRegionPreviewChannels()
        {
            if (_SelectedAppProtocol != null)
            {
                // Add scan rect adorner
                if (_ImagingVm.DisplayCanvas != null)
                {
                    // Clear preview image(s)
                    if (_ImagingVm.PreviewImages != null && _ImagingVm.PreviewImages.Count > 0)
                    {
                        _ImagingVm.PreviewImages.Clear();
                    }

                    if (_ImagingVm.RemoveAllScanRegions())   // remove scan rect adorner
                    {
                        // Start from the end of the list to select the first tab item
                        for (int index = _SelectedAppProtocol.ScanRegions.Count - 1; index >= 0; index--)
                        {
                            int nScanRegionId = (_SelectedAppProtocol.ScanRegions[index].ScanRegionNum == 0) ? index + 1 : _SelectedAppProtocol.ScanRegions[index].ScanRegionNum;
                            _ImagingVm.IsInitializing = true;   // Don't trigger scan region changed
                            _ImagingVm.AddScanRegionSelection(nScanRegionId, _SelectedAppProtocol.ScanRegions[index].ScanRect, _SelectedAppProtocol.ScanRegions[index].Color);
                            _ImagingVm.IsInitializing = false;
                        }
                    }
                }

                if (_SelectedAppProtocol.SelectedScanRegion != null)
                {
                    if (_SelectedAppProtocol.SelectedScanRegion.SignalList != null && SelectedAppProtocol.SelectedScanRegion.SignalCount > 0)
                    {
                        if (_ImagingVm != null)
                        {
                            _ImagingVm.IsLaserL1PrvVisible = false;
                            _ImagingVm.IsLaserR1PrvVisible = false;
                            _ImagingVm.IsLaserR2PrvVisible = false;
                            //_ImagingVm.IsLaserDPrvVisible = false;
                            _ImagingVm.IsContrastLaserL1Channel = false;
                            _ImagingVm.IsContrastLaserR1Channel = false;
                            _ImagingVm.IsContrastLaserR2Channel = false;
                            //_ImagingVm.IsContrastLaserDChannel = false;
                            _ImagingVm.ContrastVm.NumOfChannels = _SelectedAppProtocol.SelectedScanRegion.SignalList.Count;
                            if (_SelectedAppProtocol.SelectedScanRegion.SignalList.Count == 1)
                                _ImagingVm.IsContrastChannelAllowed = false;
                            else
                                _ImagingVm.IsContrastChannelAllowed = true;

                            // image color channel
                            _LaserL1ColorChannel = ImageChannelType.None;
                            _LaserR1ColorChannel = ImageChannelType.None;
                            _LaserR2ColorChannel = ImageChannelType.None;
                            //_LaserDColorChannel = ImageChannelType.None;

                            foreach (var signal in _SelectedAppProtocol.SelectedScanRegion.SignalList)
                            {
                                if (signal != null && signal.SelectedLaser != null)
                                {
                                    SetPreviewColorChannel(signal);
                                    SetPreviewLaserVisibility(signal.SelectedLaser.LaserChannel, true);
                                }
                            }
                        }
                    }
                }
            }

        }

        /// <summary>
        /// Show/hide the preview channel
        /// </summary>
        /// <param name="laserType"></param>
        /// <param name="bOnOffFlag"></param>
        private void SetPreviewLaserVisibility(LaserChannels laserChannel, bool bOnOffFlag)
        {
            if (laserChannel == LaserChannels.ChannelC) //L1
            {
                _ImagingVm.IsLaserL1PrvVisible = bOnOffFlag;
                _ImagingVm.IsLaserL1PrvSelected = bOnOffFlag;
                //if (_ImagingVm.IsContrastChannelAllowed)
                //    _ImagingVm.IsContrastLaserAChannel = bOnOffFlag;
            }
            else if (laserChannel == LaserChannels.ChannelA)
            {
                _ImagingVm.IsLaserR1PrvVisible = bOnOffFlag;
                _ImagingVm.IsLaserR1PrvSelected = bOnOffFlag;
                //if (_ImagingVm.IsContrastChannelAllowed)
                //    _ImagingVm.IsContrastLaserBChannel = bOnOffFlag;
            }
            else if (laserChannel == LaserChannels.ChannelB)
            {
                _ImagingVm.IsLaserR2PrvVisible = bOnOffFlag;
                _ImagingVm.IsLaserR2PrvSelected = bOnOffFlag;
                //if (_ImagingVm.IsContrastChannelAllowed)
                //    _ImagingVm.IsContrastLaserCChannel = bOnOffFlag;
            }
            if (_ImagingVm != null)
                _ImagingVm.ContrastVm.NumOfChannels = _SelectedAppProtocol.SelectedScanRegion.SignalList.Count;
        }

        private void SetPreviewColorChannel(SignalViewModel signal)
        {
            if (signal.SelectedColorChannel != null)
            {
                if (signal.SelectedLaser != null)
                {
                    if (signal.SelectedLaser.LaserChannel ==  LaserChannels.ChannelC)       //L1
                    {
                        _LaserL1ColorChannel = signal.SelectedColorChannel.ImageColorChannel;
                        _ImagingVm.LaserL1ColorChannel = _LaserL1ColorChannel;
                    }
                    else if (signal.SelectedLaser.LaserChannel == LaserChannels.ChannelA)  //R1
                    {
                        _LaserR1ColorChannel = signal.SelectedColorChannel.ImageColorChannel;
                        _ImagingVm.LaserR1ColorChannel = _LaserR1ColorChannel;
                    }
                    else if (signal.SelectedLaser.LaserChannel == LaserChannels.ChannelB)  //R2
                    {
                        _LaserR2ColorChannel = signal.SelectedColorChannel.ImageColorChannel;
                        _ImagingVm.LaserR2ColorChannel = _LaserR2ColorChannel;
                    }
                }
            }
        }

        private void SetPreviewColorChannel(SignalViewModel signal, ImageChannelType colorChannelType)
        {
            if (signal.SelectedLaser != null)
            {
                if (signal.SelectedLaser.LaserChannel == LaserChannels.ChannelC)        //L1
                {
                    _LaserL1ColorChannel = colorChannelType;
                    _ImagingVm.LaserL1ColorChannel = _LaserL1ColorChannel;
                }
                else if (signal.SelectedLaser.LaserChannel ==  LaserChannels.ChannelA)  //R1
                {
                    _LaserR1ColorChannel = colorChannelType;
                    _ImagingVm.LaserR1ColorChannel = _LaserR1ColorChannel;
                }
                else if (signal.SelectedLaser.LaserChannel == LaserChannels.ChannelB)   //R2
                {
                    _LaserR2ColorChannel = colorChannelType;
                    _ImagingVm.LaserR2ColorChannel = _LaserR2ColorChannel;
                }
            }
        }

        /*private void SetPreviewColorChannel(DyeType dye, ImageChannelType colorChannelType)
        {
            //EL: TODO: set color channel base on laser channel (we're removing the dye)
            //
            if (dye != null)
            {
                if (dye.LaserType == LaserType.LaserA)  //EL: TODO:
                {
                    _LaserL1ColorChannel = colorChannelType;
                    _ImagingVm.LaserL1ColorChannel = _LaserL1ColorChannel;
                }
                else if (dye.LaserType == LaserType.LaserB)   //EL: TODO:
                {
                    _LaserR1ColorChannel = colorChannelType;
                    _ImagingVm.LaserR1ColorChannel = _LaserR1ColorChannel;
                }
                else if (dye.LaserType == LaserType.LaserC)   //EL: TODO:
                {
                    _LaserR2ColorChannel = colorChannelType;
                    _ImagingVm.LaserR2ColorChannel = _LaserR2ColorChannel;
                }
                //else if (dye.LaserType == LaserType.LaserD)
                //{
                //    _LaserDColorChannel = colorChannelType;
                //    _ImagingVm.LaserDColorChannel = _LaserDColorChannel;
                //}
            }
        }*/
        private void SetPreviewColorChannel(LaserTypes laserType, ImageChannelType colorChannelType)
        {
            if (laserType != null)
            {
                if (laserType.LaserChannel == LaserChannels.ChannelC)
                {
                    _LaserL1ColorChannel = colorChannelType;
                    _ImagingVm.LaserL1ColorChannel = _LaserL1ColorChannel;
                }
                else if (laserType.LaserChannel == LaserChannels.ChannelA)
                {
                    _LaserR1ColorChannel = colorChannelType;
                    _ImagingVm.LaserR1ColorChannel = _LaserR1ColorChannel;
                }
                else if (laserType.LaserChannel == LaserChannels.ChannelB)
                {
                    _LaserR2ColorChannel = colorChannelType;
                    _ImagingVm.LaserR2ColorChannel = _LaserR2ColorChannel;
                }
            }
        }

        /*private ImageChannelFlag ImageChannelTypeToImageChannelFlag(ImageChannelType ict)
        {
            ImageChannelFlag retVal = ImageChannelFlag.None;

            ImageChannelFlag result;
            if (Enum.TryParse(ict.ToString(), out result))
            {
                switch (result)
                {
                    case ImageChannelFlag.Red:
                        retVal = ImageChannelFlag.Red;
                        break;
                    case ImageChannelFlag.Green:
                        retVal = ImageChannelFlag.Green;
                        break;
                    case ImageChannelFlag.Blue:
                        retVal = ImageChannelFlag.Blue;
                        break;
                    case ImageChannelFlag.Gray:
                        retVal = ImageChannelFlag.Gray;
                        break;
                }
            }

            return retVal;
        }*/

        //private string GetLaserWaveLength(LaserType laserType)
        //{
        //    string result = string.Empty;
        //    if (SettingsManager.ConfigSettings.DyeOptions != null && SettingsManager.ConfigSettings.DyeOptions.Count > 0)
        //    {
        //        foreach (var dye in SettingsManager.ConfigSettings.DyeOptions)
        //        {
        //            if (dye.LaserType == laserType)
        //            {
        //                string[] values = dye.WaveLength.Split('/');
        //                result = values[0].Trim();
        //                break;
        //            }
        //        }
        //    }
        //    return result;
        //}

        private void ResetPreviewImages()
        {
            _ChannelL1PrevImage = null;
            _ChannelL1PrevImageUnAligned = null;
            _ChannelR1PrevImage = null;
            _ChannelR1PrevImageUnAligned = null;
            _ChannelR2PrevImage = null;
            _ChannelR2PrevImageUnAligned = null;
            _PreviewImage = null;
            _ImagingVm.PreviewImage = _PreviewImage;
        }

        /// <summary>
        /// Update AppProtocolOptions from existing protocols.
        /// </summary>
        /// <param name="protocols"></param>
        public void LoadAppProtocols(List<Protocol> protocols)
        {
            if (protocols == null || protocols.Count == 0)
            {
                return;
            }

            // Clear application protocols (starting at the end)
            for (int i = 0, j = AppProtocolOptions.Count; i < j; i++)
            {
                AppProtocolOptions.RemoveAt(0);
            }

            foreach (var protocol in protocols)
            {
                if (protocol != null)
                {
                    ProtocolViewModel protocolVm = new ProtocolViewModel();
                    protocolVm.ProtocolName = protocol.Name;
                    protocolVm.IsAlwaysVisible = protocol.IsAlwaysVisible;
                    protocolVm.ScanRegions.Clear();
                    // Add scan regions
                    foreach (var protocolScanRegion in protocol.ScanRegions)
                    {
                        ScanRegionViewModel scanRegionVm = new ScanRegionViewModel();
                        scanRegionVm.ScanRegionNum = protocolScanRegion.RegionNumber;
                        scanRegionVm.CellSize = _ImagingVm.CellSize;
                        scanRegionVm.NumOfCells = _ImagingVm.NumOfCells;
                        //scanRegionVm.IsInitializing = true;

                        if (scanRegionVm.PixelSizeOptions != null && scanRegionVm.PixelSizeOptions.Count > 0)
                            scanRegionVm.SelectedPixelSize = scanRegionVm.PixelSizeOptions[protocolScanRegion.PixelSize - 1];     // config file is 1 index
                        if (scanRegionVm.ScanSpeedOptions != null && scanRegionVm.ScanSpeedOptions.Count > 0)
                            scanRegionVm.SelectedScanSpeed = scanRegionVm.ScanSpeedOptions[protocolScanRegion.ScanSpeed - 1];     // config file is 1 index
                        if (scanRegionVm.ScanQualityOptions != null && scanRegionVm.ScanQualityOptions.Count > 0)
                        {
                            scanRegionVm.SelectedScanQuality = scanRegionVm.ScanQualityOptions.FirstOrDefault(x => x.Position == protocolScanRegion.ScanQuality);
                            if (scanRegionVm.SelectedScanQuality == null)
                            {
                                scanRegionVm.SelectedScanQuality = scanRegionVm.ScanQualityOptions[0];  //select the first item
                            }
                        }
                        if (scanRegionVm.SampleTypeOptions != null && scanRegionVm.SampleTypeOptions.Count > 0)
                        {
                            if (protocolScanRegion.IsZScan)
                            {
                                scanRegionVm.IsZScan = protocolScanRegion.IsZScan;
                                scanRegionVm.ZScanSetting.BottomImageFocus = protocolScanRegion.BottomImageFocus;
                                scanRegionVm.ZScanSetting.DeltaFocus = protocolScanRegion.DeltaFocus;
                                scanRegionVm.ZScanSetting.NumOfImages = protocolScanRegion.NumOfImages;
                                int index = scanRegionVm.SampleTypeOptions.ToList().FindIndex(x => x.DisplayName == "Z-Scan");
                                if (index >= 0 && index < scanRegionVm.SampleTypeOptions.Count)
                                {
                                    scanRegionVm.SelectedSampleType = scanRegionVm.SampleTypeOptions[index];
                                }
                            }
                            else if (protocolScanRegion.IsCustomFocus)
                            {
                                scanRegionVm.IsCustomFocus = protocolScanRegion.IsCustomFocus;
                                scanRegionVm.CustomFocusValue = protocolScanRegion.CustomFocusPosition;
                                int index = scanRegionVm.SampleTypeOptions.ToList().FindIndex(x => x.DisplayName == "Custom");
                                if (index >= 0 && index < scanRegionVm.SampleTypeOptions.Count)
                                {
                                    scanRegionVm.SelectedSampleType = scanRegionVm.SampleTypeOptions[index];
                                }
                            }
                            else
                            {
                                int index = scanRegionVm.SampleTypeOptions.ToList().FindIndex(x => x.Position == protocolScanRegion.SampleType);
                                if (index >= 0 && index < scanRegionVm.SampleTypeOptions.Count)
                                {
                                    scanRegionVm.SelectedSampleType = scanRegionVm.SampleTypeOptions[index];
                                }
                            }
                        }

                        if (protocolScanRegion.Lasers != null)
                        {
                            foreach (var laser in protocolScanRegion.Lasers)
                            {
                                List<int> keyList = new List<int>(SettingsManager.ConfigSettings.LasersSignalList.Keys);
                                SignalViewModel signal = new SignalViewModel(SettingsManager.ConfigSettings.LasersSignalList[keyList[0]].Count);
                                //if (Workspace.This.ContainsPhosphorModule(Workspace.This.LaserOptions))
                                //    signal.LaserOptions = Workspace.This.RemovePhosphorModule(Workspace.This.LaserOptions);
                                //else
                                //    signal.LaserOptions = new ObservableCollection<LaserTypes>(Workspace.This.LaserOptions);
                                if (signal != null && signal.LaserOptions != null && signal.LaserOptions.Count > 0)
                                {
                                    var itemsFound = signal.LaserOptions.Where(item => item.LaserChannel == laser.LaserChannel).ToList();
                                    if (itemsFound != null && itemsFound.Count > 0)
                                    {
                                        signal.SelectedLaser = itemsFound[0];
                                    }
                                    signal.SelectedSignalLevel = signal.SignalLevelOptions[laser.SignalIntensity - 1];
                                    for (int i = 0; i < signal.ColorChannelOptions.Count; i++)
                                    {
                                        if (laser.ColorChannel == signal.ColorChannelOptions[i].ImageColorChannel)
                                        {
                                            signal.SelectedColorChannel = signal.ColorChannelOptions[i];
                                            break;
                                        }
                                    }
                                    signal.SignalChanged += new SignalViewModel.SignalChangedDelegate(Protocol_SignalChanged);
                                    scanRegionVm.SignalList.Add(signal);
                                }
                            }
                        }

                        Rect rect = new Rect(
                            protocolScanRegion.ScanRect.X * _ImagingVm.CellSize, protocolScanRegion.ScanRect.Y * _ImagingVm.CellSize,
                            protocolScanRegion.ScanRect.Width * _ImagingVm.CellSize, protocolScanRegion.ScanRect.Height * _ImagingVm.CellSize);
                        scanRegionVm.ScanRect = new ScanRegionRect(rect);

                        if (protocolScanRegion.IsAutoSaveEnabled)
                        {
                            scanRegionVm.FileLocationVm.IsAutoSave = protocolScanRegion.IsAutoSaveEnabled;
                            scanRegionVm.FileLocationVm.DestinationFolder = protocolScanRegion.AutoSavePath;
                        }

                        // Subcribe to scan rect property changed
                        scanRegionVm.ScanRectChanged += ScanRegion_ScanRectChanged;
                        // Subcribe to scan region settings property changed
                        scanRegionVm.ScanRegionSettingChanged += ScanRegion_ScanRegionSettingChanged;
                        
                        protocolVm.AddScanRegion(scanRegionVm, true);
                    }

                    // Subcribe to the protocol's scan region changed notification
                    protocolVm.ScanRegionChanged += Protocol_ScanRegionChanged;

                    if (protocolVm.SelectedScanRegion == null)
                    {
                        // Make sure the tab header on each scan region is selected to update scan region settings UI.
                        protocolVm.SelectedScanRegion = protocolVm.ScanRegions.First();
                    }

                    _AppProtocolOptions.Add(protocolVm);
                }
            }

            if (AppProtocolOptions.Count > 0)
            {
                SelectedAppProtocol = AppProtocolOptions.First();
            }
            else
            {
                SelectedAppProtocol = null;
            }
        }

        private void Protocol_ScanRegionChanged(object sender)
        {
            if (_SelectedAppProtocol != null)
            {
                if (_SelectedAppProtocol.SelectedScanRegion != null)
                {
                    Workspace.This.Owner.Dispatcher.BeginInvoke((Action)delegate
                    {
                        _SelectedAppProtocol.SelectedScanRegion.FileLocationVm.FileSize = GetFileSize();
                        ScanTime = ImagingSystemHelper.FormatTime(_SelectedAppProtocol.SelectedScanRegion.GetScanTime());

                        // Preveiw channels visibility setup
                        if (_SelectedAppProtocol.SelectedScanRegion != null)
                        {
                            if (_SelectedAppProtocol.SelectedScanRegion.SignalList != null && SelectedAppProtocol.SelectedScanRegion.SignalCount > 0)
                            {
                                if (_ImagingVm != null)
                                {
                                    _ImagingVm.IsLaserL1PrvVisible = false;
                                    _ImagingVm.IsLaserR1PrvVisible = false;
                                    _ImagingVm.IsLaserR2PrvVisible = false;
                                    //_ImagingVm.IsLaserDPrvVisible = false;
                                    _ImagingVm.IsContrastLaserL1Channel = false;
                                    _ImagingVm.IsContrastLaserR1Channel = false;
                                    _ImagingVm.IsContrastLaserR2Channel = false;
                                    //_ImagingVm.IsContrastLaserDChannel = false;
                                    _ImagingVm.ContrastVm.NumOfChannels = _SelectedAppProtocol.SelectedScanRegion.SignalList.Count;
                                    if (_SelectedAppProtocol.SelectedScanRegion.SignalList.Count == 1)
                                        _ImagingVm.IsContrastChannelAllowed = false;
                                    else
                                        _ImagingVm.IsContrastChannelAllowed = true;

                                    // image color channel
                                    _LaserL1ColorChannel = ImageChannelType.None;
                                    _LaserR1ColorChannel = ImageChannelType.None;
                                    _LaserR2ColorChannel = ImageChannelType.None;
                                    //_LaserDColorChannel = ImageChannelType.None;

                                    foreach (var signal in _SelectedAppProtocol.SelectedScanRegion.SignalList)
                                    {
                                        if (signal != null && signal.SelectedLaser != null)
                                        {
                                            SetPreviewColorChannel(signal);
                                            SetPreviewLaserVisibility(signal.SelectedLaser.LaserChannel, true);
                                        }
                                    }
                                }
                            }
                        }
                    });
                }
            }
        }

        #endregion

    }

    #region Helper classes

    public class ScanChannelData
    {
        public WriteableBitmap ImageData { get; set; }
        public ImageChannelType Channel { get; set; }

        public ScanChannelData(WriteableBitmap image, ImageChannelType channel)
        {
            this.ImageData = image;
            this.Channel = channel;
        }

        // 784: A channel : Gray
        // 520: B channel : Green
        // 658: C channel : Red
        // 488: D channel : Blue
        public string ChannelToSignal()
        {
            string retVal = string.Empty;
            switch (this.Channel)
            {
                case ImageChannelType.Red:
                    retVal = "658";
                    break;
                case ImageChannelType.Green:
                    retVal = "520";
                    break;
                case ImageChannelType.Blue:
                    retVal = "488";
                    break;
                case ImageChannelType.Gray:
                    retVal = "784";
                    break;
            }
            return retVal;
        }
    }

    public class LaserTypeColorChannel
    {
        public LaserType LaserType { get; set; }
        public ImageChannelType ImageColorChannel { get; set; }

        public LaserTypeColorChannel(LaserType laserType, ImageChannelType imageColorChannel)
        {
            this.LaserType = laserType;
            this.ImageColorChannel = imageColorChannel;
        }
    }

    #endregion

}
