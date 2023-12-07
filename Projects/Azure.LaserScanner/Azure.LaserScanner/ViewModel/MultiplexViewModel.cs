using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Azure.Image.Processing;
using Azure.WPF.Framework;

namespace Azure.LaserScanner.ViewModel
{
    class MultiplexViewModel : ViewModelBase
    {
        #region Private field/data...

        private ObservableCollection<FileViewModel> _Files = new ObservableCollection<FileViewModel>();
        private ObservableCollection<MultiplexData> _FilesTitle = new ObservableCollection<MultiplexData>();
        //private FileViewModel _SelectedImageC1 = null;
        //private FileViewModel _SelectedImageC2 = null;
        //private FileViewModel _SelectedImageC3 = null;
        //private FileViewModel _SelectedImageC4 = null;
        private MultiplexData _SelectedImageC1 = null;
        private MultiplexData _SelectedImageC2 = null;
        private MultiplexData _SelectedImageC3 = null;
        private MultiplexData _SelectedImageC4 = null;
        private bool _IsKeepSourceImages = false;

        #endregion

        #region Public properties...

        /*public FileViewModel SelectedImageC1
        {
            get { return _SelectedImageC1; }
            set
            {
                _SelectedImageC1 = value;
                RaisePropertyChanged("SelectedImageC1");
            }
        }

        public FileViewModel SelectedImageC2
        {
            get { return _SelectedImageC2; }
            set
            {
                _SelectedImageC2 = value;
                RaisePropertyChanged("SelectedImageC2");
            }
        }

        public FileViewModel SelectedImageC3
        {
            get { return _SelectedImageC3; }
            set
            {
                _SelectedImageC3 = value;
                RaisePropertyChanged("SelectedImageC3");
            }
        }

        public FileViewModel SelectedImageC4
        {
            get { return _SelectedImageC4; }
            set
            {
                _SelectedImageC4 = value;
                RaisePropertyChanged("SelectedImageC4");
            }
        }*/

        public MultiplexData SelectedImageC1
        {
            get { return _SelectedImageC1; }
            set
            {
                _SelectedImageC1 = value;
                RaisePropertyChanged("SelectedImageC1");
            }
        }

        public MultiplexData SelectedImageC2
        {
            get { return _SelectedImageC2; }
            set
            {
                _SelectedImageC2 = value;
                RaisePropertyChanged("SelectedImageC2");
            }
        }

        public MultiplexData SelectedImageC3
        {
            get { return _SelectedImageC3; }
            set
            {
                _SelectedImageC3 = value;
                RaisePropertyChanged("SelectedImageC3");
            }
        }

        public MultiplexData SelectedImageC4
        {
            get { return _SelectedImageC4; }
            set
            {
                _SelectedImageC4 = value;
                RaisePropertyChanged("SelectedImageC4");
            }
        }

        public ObservableCollection<FileViewModel> Files
        {
            get { return _Files; }
            set
            {
                _Files = value;
                RaisePropertyChanged("Files");
            }
        }

        public ObservableCollection<MultiplexData> FilesTitle
        {
            get { return _FilesTitle; }
            set
            {
                _FilesTitle = value;
                RaisePropertyChanged("FilesTitle");
            }
        }

        public bool IsKeepSourceImages
        {
            get { return _IsKeepSourceImages; }
            set
            {
                _IsKeepSourceImages = value;
                RaisePropertyChanged("IsKeepSourceImages");
            }
        }

        #endregion

        #region Constructors...

        public MultiplexViewModel()
        {
        }

        #endregion

        #region CloseCommand
        private RelayCommand _CloseCommand = null;
        public ICommand CloseCommand
        {
            get
            {
                if (_CloseCommand == null)
                {
                    _CloseCommand = new RelayCommand((p) => ExecuteCloseCommand(p), (p) => CanExecuteCloseCommand(p));
                }
                return _CloseCommand;
            }
        }

        private void ExecuteCloseCommand(object parameter)
        {
            Workspace.This.IsMultiplexChecked = false;
        }

        private bool CanExecuteCloseCommand(object parameter)
        {
            return (Workspace.This.IsMultiplexChecked);
        }

        #endregion

        #region MergeChannelsCommand
        private RelayCommand _MergeChannelsCommand = null;
        public ICommand MergeChannelsCommand
        {
            get
            {
                if (_MergeChannelsCommand == null)
                {
                    _MergeChannelsCommand = new RelayCommand((p) => ExecuteMergeChannelsCommand(p), (p) => CanExecuteMergeChannelsCommand(p));
                }

                return _MergeChannelsCommand;
            }
        }

        private void ExecuteMergeChannelsCommand(object parameter)
        {
            FileViewModel[] fileList = { null, null, null, null };
            int fileCount = 0;
            ImageInfo imageInfo = null;
            // laser power intensity
            int laserAIntensity = 0;
            int laserBIntensity = 0;
            int laserCIntensity = 0;
            int laserDIntensity = 0;
            // scan intensity level
            int laserAIntensityLevel = 0;
            int laserBIntensityLevel = 0;
            int laserCIntensityLevel = 0;
            int laserDIntensityLevel = 0;
            string strLaserAIntensityLevel = "-";
            string strLaserBIntensityLevel = "-";
            string strLaserCIntensityLevel = "-";
            string strLaserDIntensityLevel = "-";
            int apdAGain = 0;
            int apdBGain = 0;
            int apdCGain = 0;
            int apdDGain = 0;
            int apdAPga = 0;
            int apdBPga = 0;
            int apdCPga = 0;
            int apdDPga = 0;

            if (SelectedImageC1 != null && SelectedImageC1.Title != "None" && SelectedImageC1.FileID != "0")
            {
                #region === Red / CH1 ===

                FileViewModel selectedFile = GetFile(SelectedImageC1);
                if (selectedFile != null)
                {
                    if (imageInfo == null)
                    {
                        imageInfo = (ImageInfo)selectedFile.ImageInfo.Clone();
                    }
                    if (selectedFile.Image.Format.BitsPerPixel == 8 || selectedFile.Image.Format.BitsPerPixel == 16)
                    {
                        fileList[0] = selectedFile;
                        imageInfo.RedChannel = (ImageChannel)selectedFile.ImageInfo.MixChannel.Clone();
                        imageInfo.IsRedChannelAvail = true;
                        if (imageInfo.RedChannel.ScanZ0 == 0)
                            imageInfo.RedChannel.ScanZ0 = imageInfo.ScanZ0;
                        ImagingSystem.LaserType laserType = (ImagingSystem.LaserType)selectedFile.ImageInfo.MixChannel.LightSource;
                        if (laserType == ImagingSystem.LaserType.LaserA)
                        {
                            if (imageInfo.RedChannel.LaserIntensityLevel != 0)
                            {
                                laserAIntensityLevel = imageInfo.RedChannel.LaserIntensityLevel;
                                laserAIntensity = imageInfo.RedChannel.LaserIntensity;
                                apdAGain = imageInfo.RedChannel.ApdGain;
                                apdAPga = imageInfo.RedChannel.ApdPga;
                            }
                            else
                            {
                                // Get laser info from image extracted from older version of the software
                                laserAIntensity = 0;
                                GetLaserInfo(imageInfo, laserType, ref apdAGain, ref apdAPga, ref laserAIntensityLevel, ref laserAIntensity);
                                imageInfo.RedChannel.LaserIntensityLevel = laserAIntensityLevel;
                                imageInfo.RedChannel.LaserIntensity = laserAIntensity;
                                imageInfo.RedChannel.ApdGain = apdAGain;
                                imageInfo.RedChannel.ApdPga = apdAPga;
                            }
                            if (imageInfo.MajorVersion >= 1 && imageInfo.MinorVersion >= 5)
                            {
                                strLaserAIntensityLevel = Workspace.IntensityLevels[laserAIntensityLevel];
                            }
                            else
                            {
                                strLaserAIntensityLevel = laserAIntensityLevel.ToString();
                            }
                        }
                        else if (laserType == ImagingSystem.LaserType.LaserB)
                        {
                            if (imageInfo.RedChannel.LaserIntensityLevel != 0)
                            {
                                laserBIntensityLevel = imageInfo.RedChannel.LaserIntensityLevel;
                                laserBIntensity = imageInfo.RedChannel.LaserIntensity;
                                apdBGain = imageInfo.RedChannel.ApdGain;
                                apdBPga = imageInfo.RedChannel.ApdPga;
                            }
                            else
                            {
                                // Get laser info from image extracted from older version of the software
                                laserBIntensity = 0;
                                GetLaserInfo(imageInfo, laserType, ref apdBGain, ref apdBPga, ref laserBIntensityLevel, ref laserBIntensity);
                                imageInfo.RedChannel.LaserIntensityLevel = laserBIntensityLevel;
                                imageInfo.RedChannel.LaserIntensity = laserBIntensity;
                                imageInfo.RedChannel.ApdGain = apdBGain;
                                imageInfo.RedChannel.ApdPga = apdBPga;
                            }
                            if (imageInfo.MajorVersion >= 1 && imageInfo.MinorVersion >= 5)
                            {
                                strLaserBIntensityLevel = Workspace.IntensityLevels[laserBIntensityLevel];
                            }
                            else
                            {
                                strLaserBIntensityLevel = laserBIntensityLevel.ToString();
                            }
                        }
                        else if (laserType == ImagingSystem.LaserType.LaserC)
                        {
                            if (imageInfo.RedChannel.LaserIntensityLevel != 0)
                            {
                                laserCIntensityLevel = imageInfo.RedChannel.LaserIntensityLevel;
                                laserCIntensity = imageInfo.RedChannel.LaserIntensity;
                                apdCGain = imageInfo.RedChannel.ApdGain;
                                apdCPga = imageInfo.RedChannel.ApdPga;
                            }
                            else
                            {
                                // Get laser info from image extracted from older version of the software
                                laserCIntensity = 0;
                                GetLaserInfo(imageInfo, laserType, ref apdCGain, ref apdCPga, ref laserCIntensityLevel, ref laserCIntensity);
                                imageInfo.RedChannel.LaserIntensityLevel = laserCIntensityLevel;
                                imageInfo.RedChannel.LaserIntensity = laserCIntensity;
                                imageInfo.RedChannel.ApdGain = apdCGain;
                                imageInfo.RedChannel.ApdPga = apdCPga;
                            }
                            if (imageInfo.MajorVersion >= 1 && imageInfo.MinorVersion >= 5)
                            {
                                strLaserCIntensityLevel = Workspace.IntensityLevels[laserCIntensityLevel];
                            }
                            else
                            {
                                strLaserCIntensityLevel = laserCIntensityLevel.ToString();
                            }
                        }
                        else if (laserType == ImagingSystem.LaserType.LaserD)
                        {
                            if (imageInfo.RedChannel.LaserIntensityLevel != 0)
                            {
                                laserDIntensityLevel = imageInfo.RedChannel.LaserIntensityLevel;
                                laserDIntensity = imageInfo.RedChannel.LaserIntensity;
                                apdDGain = imageInfo.RedChannel.ApdGain;
                                apdDPga = imageInfo.RedChannel.ApdPga;
                            }
                            else
                            {
                                // Get laser info from image extracted from older version of the software
                                laserDIntensity = 0;
                                GetLaserInfo(imageInfo, laserType, ref apdDGain, ref apdDPga, ref laserDIntensityLevel, ref laserDIntensity);
                                imageInfo.RedChannel.LaserIntensityLevel = laserDIntensityLevel;
                                imageInfo.RedChannel.LaserIntensity = laserDIntensity;
                                imageInfo.RedChannel.ApdGain = apdDGain;
                                imageInfo.RedChannel.ApdPga = apdDPga;
                            }
                            if (imageInfo.MajorVersion >= 1 && imageInfo.MinorVersion >= 5)
                            {
                                strLaserDIntensityLevel = Workspace.IntensityLevels[laserDIntensityLevel];
                            }
                            else
                            {
                                strLaserDIntensityLevel = laserDIntensityLevel.ToString();
                            }
                        }
                        imageInfo.IsRedChannelAvail = true;
                        fileCount++;
                    }
                    else
                    {
                        Xceed.Wpf.Toolkit.MessageBox.Show("The selected image must be grayscale image (invalid channel: red).", "Invalid image type",
                                                          System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                        return;
                    }
                }

                #endregion
            }
            if (SelectedImageC2 != null && SelectedImageC2.Title != "None" && SelectedImageC2.FileID != "0")
            {
                #region === Green / CH2 ===

                FileViewModel selectedFile = GetFile(SelectedImageC2);
                if (selectedFile != null)
                {
                    if (imageInfo == null)
                    {
                        imageInfo = (ImageInfo)selectedFile.ImageInfo.Clone();
                    }
                    if (selectedFile.Image.Format.BitsPerPixel == 8 || selectedFile.Image.Format.BitsPerPixel == 16)
                    {
                        fileList[1] = selectedFile;
                        if (imageInfo == null)
                            imageInfo = (ImageInfo)selectedFile.ImageInfo.Clone();
                        imageInfo.GreenChannel = (ImageChannel)selectedFile.ImageInfo.MixChannel.Clone();
                        imageInfo.IsGreenChannelAvail = true;
                        if (imageInfo.GreenChannel.ScanZ0 == 0)
                            imageInfo.GreenChannel.ScanZ0 = imageInfo.ScanZ0;
                        ImagingSystem.LaserType laserType = (ImagingSystem.LaserType)imageInfo.GreenChannel.LightSource;

                        if (laserType == ImagingSystem.LaserType.LaserA)
                        {
                            if (imageInfo.GreenChannel.LaserIntensityLevel != 0)
                            {
                                laserAIntensityLevel = imageInfo.GreenChannel.LaserIntensityLevel;
                                laserAIntensity = imageInfo.GreenChannel.LaserIntensity;
                                apdAGain = imageInfo.GreenChannel.ApdGain;
                                apdAPga = imageInfo.GreenChannel.ApdPga;
                            }
                            else
                            {
                                // Get laser info from image extracted from older version of the software
                                laserAIntensity = 0;
                                GetLaserInfo(imageInfo, laserType, ref apdAGain, ref apdAPga, ref laserAIntensityLevel, ref laserAIntensity);
                                imageInfo.GreenChannel.LaserIntensityLevel = laserAIntensityLevel;
                                imageInfo.GreenChannel.LaserIntensity = laserAIntensity;
                                imageInfo.GreenChannel.ApdGain = apdAGain;
                                imageInfo.GreenChannel.ApdPga = apdAPga;
                            }
                            if (imageInfo.MajorVersion >= 1 && imageInfo.MinorVersion >= 5)
                            {
                                strLaserAIntensityLevel = Workspace.IntensityLevels[laserAIntensityLevel];
                            }
                            else
                            {
                                strLaserAIntensityLevel = laserAIntensityLevel.ToString();
                            }
                        }
                        else if (laserType == ImagingSystem.LaserType.LaserB)
                        {
                            if (imageInfo.GreenChannel.LaserIntensityLevel != 0)
                            {
                                laserBIntensityLevel = imageInfo.GreenChannel.LaserIntensityLevel;
                                laserBIntensity = imageInfo.GreenChannel.LaserIntensity;
                                apdBGain = imageInfo.GreenChannel.ApdGain;
                                apdBPga = imageInfo.GreenChannel.ApdPga;
                            }
                            else
                            {
                                // Get laser info from image extracted from older version of the software
                                laserBIntensity = 0;
                                GetLaserInfo(imageInfo, laserType, ref apdBGain, ref apdBPga, ref laserBIntensityLevel, ref laserBIntensity);
                                imageInfo.GreenChannel.LaserIntensityLevel = laserBIntensityLevel;
                                imageInfo.GreenChannel.LaserIntensity = laserBIntensity;
                                imageInfo.GreenChannel.ApdGain = apdBGain;
                                imageInfo.GreenChannel.ApdPga = apdBPga;
                            }
                            if (imageInfo.MajorVersion >= 1 && imageInfo.MinorVersion >= 5)
                            {
                                strLaserBIntensityLevel = Workspace.IntensityLevels[laserBIntensityLevel];
                            }
                            else
                            {
                                strLaserBIntensityLevel = laserBIntensityLevel.ToString();
                            }
                        }
                        else if (laserType == ImagingSystem.LaserType.LaserC)
                        {
                            if (imageInfo.GreenChannel.LaserIntensityLevel != 0)
                            {
                                laserCIntensityLevel = imageInfo.GreenChannel.LaserIntensityLevel;
                                laserCIntensity = imageInfo.GreenChannel.LaserIntensity;
                                apdCGain = imageInfo.GreenChannel.ApdGain;
                                apdCPga = imageInfo.GreenChannel.ApdPga;
                            }
                            else
                            {
                                // Get laser info from image extracted from older version of the software
                                laserCIntensity = 0;
                                GetLaserInfo(imageInfo, laserType, ref apdCGain, ref apdCPga, ref laserCIntensityLevel, ref laserCIntensity);
                                imageInfo.GreenChannel.LaserIntensityLevel = laserCIntensityLevel;
                                imageInfo.GreenChannel.LaserIntensity = laserCIntensity;
                                imageInfo.GreenChannel.ApdGain = apdCGain;
                                imageInfo.GreenChannel.ApdPga = apdCPga;
                            }
                            if (imageInfo.MajorVersion >= 1 && imageInfo.MinorVersion >= 5)
                            {
                                strLaserCIntensityLevel = Workspace.IntensityLevels[laserCIntensityLevel];
                            }
                            else
                            {
                                strLaserCIntensityLevel = laserCIntensityLevel.ToString();
                            }
                        }
                        else if (laserType == ImagingSystem.LaserType.LaserD)
                        {
                            if (imageInfo.GreenChannel.LaserIntensityLevel != 0)
                            {
                                laserDIntensityLevel = imageInfo.GreenChannel.LaserIntensityLevel;
                                laserDIntensity = imageInfo.GreenChannel.LaserIntensity;
                                apdDGain = imageInfo.GreenChannel.ApdGain;
                                apdDPga = imageInfo.GreenChannel.ApdPga;
                            }
                            else
                            {
                                // Get laser info from image extracted from older version of the software
                                laserDIntensity = 0;
                                GetLaserInfo(imageInfo, laserType, ref apdDGain, ref apdDPga, ref laserDIntensityLevel, ref laserDIntensity);
                                imageInfo.GreenChannel.LaserIntensityLevel = laserDIntensityLevel;
                                imageInfo.GreenChannel.LaserIntensity = laserDIntensity;
                                imageInfo.GreenChannel.ApdGain = apdDGain;
                                imageInfo.GreenChannel.ApdPga = apdDPga;
                            }
                            if (imageInfo.MajorVersion >= 1 && imageInfo.MinorVersion >= 5)
                            {
                                strLaserDIntensityLevel = Workspace.IntensityLevels[laserDIntensityLevel];
                            }
                            else
                            {
                                strLaserDIntensityLevel = laserDIntensityLevel.ToString();
                            }
                        }
                        imageInfo.IsGreenChannelAvail = true;
                        fileCount++;
                    }
                    else
                    {
                        Xceed.Wpf.Toolkit.MessageBox.Show("The selected image must be grayscale image (invalid channel: green).", "Invalid image type",
                                                          System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                        return;
                    }
                }

                #endregion
            }
            if (SelectedImageC3 != null && SelectedImageC3.Title != "None" && SelectedImageC3.FileID != "0")
            {
                #region === Blue / CH3 ===

                FileViewModel selectedFile = GetFile(SelectedImageC3);
                if (selectedFile != null)
                {
                    if (imageInfo == null)
                    {
                        imageInfo = (ImageInfo)selectedFile.ImageInfo.Clone();
                    }
                    if (selectedFile.Image.Format.BitsPerPixel == 8 || selectedFile.Image.Format.BitsPerPixel == 16)
                    {
                        fileList[2] = selectedFile;
                        if (imageInfo == null)
                            imageInfo = (ImageInfo)selectedFile.ImageInfo.Clone();
                        imageInfo.BlueChannel = (ImageChannel)selectedFile.ImageInfo.MixChannel.Clone();
                        imageInfo.IsBlueChannelAvail = true;
                        if (imageInfo.BlueChannel.ScanZ0 == 0)
                            imageInfo.BlueChannel.ScanZ0 = imageInfo.ScanZ0;
                        ImagingSystem.LaserType laserType = (ImagingSystem.LaserType)imageInfo.BlueChannel.LightSource;
                        if (laserType == ImagingSystem.LaserType.LaserA)
                        {
                            if (imageInfo.BlueChannel.LaserIntensityLevel != 0)
                            {
                                laserAIntensityLevel = imageInfo.BlueChannel.LaserIntensityLevel;
                                laserAIntensity = imageInfo.BlueChannel.LaserIntensity;
                                apdAGain = imageInfo.BlueChannel.ApdGain;
                                apdAPga = imageInfo.BlueChannel.ApdPga;
                            }
                            else
                            {
                                // Get laser info from image extracted from older version of the software
                                laserAIntensity = 0;
                                GetLaserInfo(imageInfo, laserType, ref apdAGain, ref apdAPga, ref laserAIntensityLevel, ref laserAIntensity);
                                imageInfo.BlueChannel.LaserIntensityLevel = laserAIntensityLevel;
                                imageInfo.BlueChannel.LaserIntensity = laserAIntensity;
                                imageInfo.BlueChannel.ApdGain = apdAGain;
                                imageInfo.BlueChannel.ApdPga = apdAPga;
                            }
                            if (imageInfo.MajorVersion >= 1 && imageInfo.MinorVersion >= 5)
                            {
                                strLaserAIntensityLevel = Workspace.IntensityLevels[laserAIntensityLevel];
                            }
                            else
                            {
                                strLaserAIntensityLevel = laserAIntensityLevel.ToString();
                            }
                        }
                        else if (laserType == ImagingSystem.LaserType.LaserB)
                        {
                            if (imageInfo.BlueChannel.LaserIntensityLevel != 0)
                            {
                                laserBIntensityLevel = imageInfo.BlueChannel.LaserIntensityLevel;
                                laserBIntensity = imageInfo.BlueChannel.LaserIntensity;
                                apdBGain = imageInfo.BlueChannel.ApdGain;
                                apdBPga = imageInfo.BlueChannel.ApdPga;
                            }
                            else
                            {
                                // Get laser info from image extracted from older version of the software
                                laserBIntensity = 0;
                                GetLaserInfo(imageInfo, laserType, ref apdBGain, ref apdBPga, ref laserBIntensityLevel, ref laserBIntensity);
                                imageInfo.BlueChannel.LaserIntensityLevel = laserBIntensityLevel;
                                imageInfo.BlueChannel.LaserIntensity = laserBIntensity;
                                imageInfo.BlueChannel.ApdGain = apdBGain;
                                imageInfo.BlueChannel.ApdPga = apdBPga;
                            }
                            if (imageInfo.MajorVersion >= 1 && imageInfo.MinorVersion >= 5)
                            {
                                strLaserBIntensityLevel = Workspace.IntensityLevels[laserBIntensityLevel];
                            }
                            else
                            {
                                strLaserBIntensityLevel = laserBIntensityLevel.ToString();
                            }
                        }
                        else if (laserType == ImagingSystem.LaserType.LaserC)
                        {
                            if (imageInfo.BlueChannel.LaserIntensityLevel != 0)
                            {
                                laserCIntensityLevel = imageInfo.BlueChannel.LaserIntensityLevel;
                                laserCIntensity = imageInfo.BlueChannel.LaserIntensity;
                                apdCGain = imageInfo.BlueChannel.ApdGain;
                                apdCPga = imageInfo.BlueChannel.ApdPga;
                            }
                            else
                            {
                                // Get laser info from image extracted from older version of the software
                                laserCIntensity = 0;
                                GetLaserInfo(imageInfo, laserType, ref apdCGain, ref apdCPga, ref laserCIntensityLevel, ref laserCIntensity);
                                imageInfo.BlueChannel.LaserIntensityLevel = laserCIntensityLevel;
                                imageInfo.BlueChannel.LaserIntensity = laserCIntensity;
                                imageInfo.BlueChannel.ApdGain = apdCGain;
                                imageInfo.BlueChannel.ApdPga = apdCPga;
                            }
                            if (imageInfo.MajorVersion >= 1 && imageInfo.MinorVersion >= 5)
                            {
                                strLaserCIntensityLevel = Workspace.IntensityLevels[laserCIntensityLevel];
                            }
                            else
                            {
                                strLaserCIntensityLevel = laserCIntensityLevel.ToString();
                            }
                        }
                        else if (laserType == ImagingSystem.LaserType.LaserD)
                        {
                            if (imageInfo.BlueChannel.LaserIntensityLevel != 0)
                            {
                                laserDIntensityLevel = imageInfo.BlueChannel.LaserIntensityLevel;
                                laserDIntensity = imageInfo.BlueChannel.LaserIntensity;
                                apdDGain = imageInfo.BlueChannel.ApdGain;
                                apdDPga = imageInfo.BlueChannel.ApdPga;
                            }
                            else
                            {
                                // Get laser info from image extracted from older version of the software
                                laserDIntensity = 0;
                                GetLaserInfo(imageInfo, laserType, ref apdDGain, ref apdDPga, ref laserDIntensityLevel, ref laserDIntensity);
                                imageInfo.BlueChannel.LaserIntensityLevel = laserDIntensityLevel;
                                imageInfo.BlueChannel.LaserIntensity = laserDIntensity;
                                imageInfo.BlueChannel.ApdGain = apdDGain;
                                imageInfo.BlueChannel.ApdPga = apdDPga;
                            }
                            if (imageInfo.MajorVersion >= 1 && imageInfo.MinorVersion >= 5)
                            {
                                strLaserDIntensityLevel = Workspace.IntensityLevels[laserDIntensityLevel];
                            }
                            else
                            {
                                strLaserDIntensityLevel = laserDIntensityLevel.ToString();
                            }
                        }
                        imageInfo.IsBlueChannelAvail = true;
                        fileCount++;
                    }
                    else
                    {
                        Xceed.Wpf.Toolkit.MessageBox.Show("The selected image must be grayscale image (invalid channel: blue).", "Invalid image type",
                                                          System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                        return;
                    }
                }

                #endregion
            }
            if (SelectedImageC4 != null && SelectedImageC4.Title != "None" && SelectedImageC4.FileID != "0")
            {
                #region === Gray / CH4 ===

                FileViewModel selectedFile = GetFile(SelectedImageC4);
                if (selectedFile != null)
                {
                    if (imageInfo == null)
                    {
                        imageInfo = (ImageInfo)selectedFile.ImageInfo.Clone();
                    }
                    if (selectedFile.Image.Format.BitsPerPixel == 8 || selectedFile.Image.Format.BitsPerPixel == 16)
                    {
                        fileList[3] = selectedFile;
                        if (imageInfo == null)
                            imageInfo = (ImageInfo)selectedFile.ImageInfo.Clone();
                        imageInfo.GrayChannel = (ImageChannel)selectedFile.ImageInfo.MixChannel.Clone();
                        imageInfo.IsGrayChannelAvail = true;
                        if (imageInfo.GrayChannel.ScanZ0 == 0)
                            imageInfo.GrayChannel.ScanZ0 = imageInfo.ScanZ0;
                        ImagingSystem.LaserType laserType = (ImagingSystem.LaserType)imageInfo.GrayChannel.LightSource;
                        if (laserType == ImagingSystem.LaserType.LaserA)
                        {
                            if (imageInfo.GrayChannel.LaserIntensityLevel != 0)
                            {
                                laserAIntensityLevel = imageInfo.GrayChannel.LaserIntensityLevel;
                                laserAIntensity = imageInfo.GrayChannel.LaserIntensity;
                                apdAGain = imageInfo.GrayChannel.ApdGain;
                                apdAPga = imageInfo.GrayChannel.ApdPga;
                            }
                            else
                            {
                                // Get laser info from image extracted from older version of the software
                                laserAIntensity = 0;
                                GetLaserInfo(imageInfo, laserType, ref apdAGain, ref apdAPga, ref laserAIntensityLevel, ref laserAIntensity);
                                imageInfo.GrayChannel.LaserIntensityLevel = laserAIntensityLevel;
                                imageInfo.GrayChannel.LaserIntensity = laserAIntensity;
                                imageInfo.GrayChannel.ApdGain = apdAGain;
                                imageInfo.GrayChannel.ApdPga = apdAPga;
                            }
                            if (imageInfo.MajorVersion >= 1 && imageInfo.MinorVersion >= 5)
                            {
                                strLaserAIntensityLevel = Workspace.IntensityLevels[laserAIntensityLevel];
                            }
                            else
                            {
                                strLaserAIntensityLevel = laserAIntensityLevel.ToString();
                            }
                        }
                        else if (laserType == ImagingSystem.LaserType.LaserB)
                        {
                            if (imageInfo.GrayChannel.LaserIntensityLevel != 0)
                            {
                                laserBIntensityLevel = imageInfo.GrayChannel.LaserIntensityLevel;
                                laserBIntensity = imageInfo.GrayChannel.LaserIntensity;
                                apdBGain = imageInfo.GrayChannel.ApdGain;
                                apdBPga = imageInfo.GrayChannel.ApdPga;
                            }
                            else
                            {
                                // Get laser info from image extracted from older version of the software
                                laserBIntensity = 0;
                                GetLaserInfo(imageInfo, laserType, ref apdBGain, ref apdBPga, ref laserBIntensityLevel, ref laserBIntensity);
                                imageInfo.GrayChannel.LaserIntensityLevel = laserBIntensityLevel;
                                imageInfo.GrayChannel.LaserIntensity = laserBIntensity;
                                imageInfo.GrayChannel.ApdGain = apdBGain;
                                imageInfo.GrayChannel.ApdPga = apdBPga;
                            }
                            if (imageInfo.MajorVersion >= 1 && imageInfo.MinorVersion >= 5)
                            {
                                strLaserBIntensityLevel = Workspace.IntensityLevels[laserBIntensityLevel];
                            }
                            else
                            {
                                strLaserBIntensityLevel = laserBIntensityLevel.ToString();
                            }
                        }
                        else if (laserType == ImagingSystem.LaserType.LaserC)
                        {
                            if (imageInfo.GrayChannel.LaserIntensityLevel != 0)
                            {
                                laserCIntensityLevel = imageInfo.GrayChannel.LaserIntensityLevel;
                                laserCIntensity = imageInfo.GrayChannel.LaserIntensity;
                                apdCGain = imageInfo.GrayChannel.ApdGain;
                                apdCPga = imageInfo.GrayChannel.ApdPga;
                            }
                            else
                            {
                                // Get laser info from image extracted from older version of the software
                                laserCIntensity = 0;
                                GetLaserInfo(imageInfo, laserType, ref apdCGain, ref apdCPga, ref laserCIntensityLevel, ref laserCIntensity);
                                imageInfo.GrayChannel.LaserIntensityLevel = laserCIntensityLevel;
                                imageInfo.GrayChannel.LaserIntensity = laserCIntensity;
                                imageInfo.GrayChannel.ApdGain = apdCGain;
                                imageInfo.GrayChannel.ApdPga = apdCPga;
                            }
                            if (imageInfo.MajorVersion >= 1 && imageInfo.MinorVersion >= 5)
                            {
                                strLaserCIntensityLevel = Workspace.IntensityLevels[laserCIntensityLevel];
                            }
                            else
                            {
                                strLaserCIntensityLevel = laserCIntensityLevel.ToString();
                            }
                        }
                        else if (laserType == ImagingSystem.LaserType.LaserD)
                        {
                            if (imageInfo.GrayChannel.LaserIntensityLevel != 0)
                            {
                                laserDIntensityLevel = imageInfo.GrayChannel.LaserIntensityLevel;
                                laserDIntensity = imageInfo.GrayChannel.LaserIntensity;
                                apdDGain = imageInfo.GrayChannel.ApdGain;
                                apdDPga = imageInfo.GrayChannel.ApdPga;
                            }
                            else
                            {
                                // Get laser info from image extracted from older version of the software
                                laserDIntensity = 0;
                                GetLaserInfo(imageInfo, laserType, ref apdDGain, ref apdDPga, ref laserDIntensityLevel, ref laserDIntensity);
                                imageInfo.GrayChannel.LaserIntensityLevel = laserDIntensityLevel;
                                imageInfo.GrayChannel.LaserIntensity = laserDIntensity;
                                imageInfo.GrayChannel.ApdGain = apdDGain;
                                imageInfo.GrayChannel.ApdPga = apdDPga;
                            }
                            if (imageInfo.MajorVersion >= 1 && imageInfo.MinorVersion >= 5)
                            {
                                strLaserDIntensityLevel = Workspace.IntensityLevels[laserDIntensityLevel];
                            }
                            else
                            {
                                strLaserDIntensityLevel = laserDIntensityLevel.ToString();
                            }
                        }
                        fileCount++;
                    }
                    else
                    {
                        Xceed.Wpf.Toolkit.MessageBox.Show("The selected image must be grayscale image (invalid channel: gray).", "Invalid image type",
                                                          System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                        return;
                    }
                }

                #endregion
            }

            if (fileCount < 1)
            {
                fileList = null;
                string caption = "Merge image...";
                string message = "Please select 1 or more image to merge.";
                Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                return;
            }

            WriteableBitmap[] srcFiles = null;
            if (SelectedImageC4 != null)
            {
                srcFiles = new WriteableBitmap[4];
                if (fileCount == 1)
                {
                    fileList = null;
                    string caption = "Merge image...";
                    string message = "Invalid operation. Please select more than one image to merge (merging a single grayscale image to the gray channel not allowed).";
                    Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                    return;
                }
            }
            else
            {
                srcFiles = new WriteableBitmap[3];
            }

            int dynamicBitMax = 0;
            int edrBitDepthMax = 0;

            if (fileCount > 0)
            {
                if (fileCount > 1)
                {
                    // Validate image width, height and bit depth

                    // Get the first image
                    FileViewModel firstImage = null;
                    for (int i = 1; i < fileList.Length; i++)
                    {
                        if (fileList[i] != null)
                        {
                            firstImage = fileList[0];
                            break;
                        }
                    }

                    dynamicBitMax = firstImage.ImageInfo.DynamicBit;
                    edrBitDepthMax = firstImage.ImageInfo.EdrBitDepth;
                    // Validate image width, height and bit depth
                    for (int i = 1; i < fileList.Length; i++)
                    {
                        if (fileList[i] != null)
                        {
                            if (fileList[i].ImageInfo.DynamicBit > dynamicBitMax)
                                dynamicBitMax = fileList[i].ImageInfo.DynamicBit;
                            if (fileList[i].ImageInfo.EdrBitDepth > edrBitDepthMax)
                                edrBitDepthMax = fileList[i].ImageInfo.EdrBitDepth;

                                if (firstImage.Image.PixelWidth == fileList[i].Image.PixelWidth &&
                                firstImage.Image.PixelHeight == fileList[i].Image.PixelHeight &&
                                firstImage.Image.Format.BitsPerPixel == fileList[i].Image.Format.BitsPerPixel)
                            {
                                // Do nothing...valid image
                            }
                            else
                            {
                                fileList = null;
                                string caption = "Invalid files...";
                                string message = "The selected images must have the same width, height and bit depth";
                                Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                                return;
                            }
                        }
                    }
                }

                for (int i = 0; i < fileList.Length; i++)
                {
                    if (fileList[i] != null)
                    {
                        if (dynamicBitMax > 16)
                        {
                            // Convert to highest dynamicbit 
                            int dynamicBit = (fileList[i].ImageInfo.DynamicBit < 16) ? 16 : fileList[i].ImageInfo.DynamicBit;
                            if (dynamicBit < dynamicBitMax)
                            {
                                try
                                {
                                    WriteableBitmap tempImage = fileList[i].Image.Clone();
                                    tempImage = ImageProcessing.ConvertDynamicBit(tempImage, dynamicBit, dynamicBitMax);
                                    srcFiles[i] = tempImage;
                                }
                                catch (Exception ex)
                                {
                                    //throw;
                                    Workspace.This.StopWaitAnimation();
                                    string caption = "Create Multiplex Image...";
                                    string message = "Error creating a multiplex image.\nError: " + ex.Message;
                                    Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                                }
                            }
                            else
                            {
                                // Same dynamicbit as max dynamic bit (no conversion required)
                                srcFiles[i] = fileList[i].Image.Clone();
                            }
                        }
                        else
                        {
                            // Not EDR images
                            srcFiles[i] = fileList[i].Image.Clone();
                        }
                    }
                }
            }

            if (imageInfo == null)
                imageInfo = new Image.Processing.ImageInfo();

            if (dynamicBitMax > 16)
                imageInfo.DynamicBit = dynamicBitMax;
            if (edrBitDepthMax > 16)
                imageInfo.EdrBitDepth = edrBitDepthMax;

            // We're now uisng the existence of lasers' intensity to display scan info
            imageInfo.LaserDIntensity = laserDIntensity;
            imageInfo.LaserBIntensity = laserBIntensity;
            imageInfo.LaserCIntensity = laserCIntensity;
            imageInfo.LaserAIntensity = laserAIntensity;

            imageInfo.SelectedChannel = Image.Processing.ImageChannelType.Mix;
            //imageInfo.IntensityLevel = string.Format("{0} / {1} / {2} / {3}", laserAIntensityLevel, laserBIntensityLevel, laserCIntensityLevel, laserDIntensityLevel);
            // Now using the accending order of laser's wavelength instead of using laser A/B/C/D (swapping laser A and laser D)
            imageInfo.IntensityLevel = string.Format("{0} / {1} / {2} / {3}", strLaserDIntensityLevel, strLaserBIntensityLevel, strLaserCIntensityLevel, strLaserAIntensityLevel);

            try
            {
                Workspace.This.StartWaitAnimation("Merging image channels...");

                WriteableBitmap mergedImage = null;
                if (SelectedImageC4 != null && SelectedImageC4.Title != "None" && SelectedImageC4.FileID != "0")
                {
                    // Merge images as 4-channel image (rgba)
                    mergedImage = Azure.Image.Processing.ImageProcessing.SetChannel(srcFiles);
                }
                else
                {
                    // Merge images as 3-channel image (rgb)
                    mergedImage = Azure.Image.Processing.ImageProcessing.SetChannel(srcFiles);
                }

                string[] channels = { "None", "None", "None", "None" };
                for (int i = 0; i < 4; i++)
                {
                    if (fileList[i] != null)
                    {
                        channels[i] = fileList[i].FileName;
                    }
                }

                /*if (_ImagingSys.ImagingSysSettings.IsQcVersion)
                {
                    ActivityLog newActivityLog = new ActivityLog(_ImagingSys.LoginUser.UserName);
                    newActivityLog.LogCreateMultiplex(channels[0], channels[1], channels[2], channels[3]);
                    _ImagingSys.ManageUsersVM.LogActivity(newActivityLog);
                }*/

                if (!_IsKeepSourceImages)
                {
                    bool bIsUnsaved = false;
                    for (int i = 0; i < fileList.Length; i++)
                    {
                        if (fileList[i] != null)
                        {
                            if (fileList[i].IsDirty)
                            {
                                bIsUnsaved = true;
                                break;
                            }
                        }
                    }

                    // Close wait animation if prompt to save
                    // Saving an unsaved image, will cause the wait animation to stay opened.
                    if (bIsUnsaved)
                    {
                        Workspace.This.StopWaitAnimation();
                    }

                    // Close selected image(s)
                    foreach (var file in fileList)
                    {
                        if (file != null)
                            Workspace.This.Close(file);
                    }

                    // Start the wait animation again, just in case it's a very large image
                    if (bIsUnsaved)
                    {
                        Workspace.This.StartWaitAnimation("Adding multiplex image to Gallery...");
                    }
                }

                // Add to Gallery
                string title = Workspace.This.GetUniqueFilename("Composite");
                Workspace.This.NewDocument(mergedImage, imageInfo, title, false, true);
                fileList = null;
                // Close/hide multiplex control
                Workspace.This.IsMultiplexChecked = false;
            }
            catch (Exception ex)
            {
                //throw;
                Workspace.This.StopWaitAnimation();
                string caption = "Create Multiplex Image...";
                string message = "Error creating a multiplex image.\nError: " + ex.Message;
                Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
            finally
            {
                Workspace.This.StopWaitAnimation();
            }
        }

        private bool CanExecuteMergeChannelsCommand(object parameter)
        {
            return true;
        }

        #endregion 

        private FileViewModel GetFile(MultiplexData selectedFile)
        {
            FileViewModel fvmResult = null;
            foreach (var file in Workspace.This.Files)
            {
                if (file.Title == selectedFile.Title && file.FileID == selectedFile.FileID)
                {
                    fvmResult = file;
                }
            }

            return fvmResult;
        }

        public void ResetSelection()
        {
            SelectedImageC1 = null;
            SelectedImageC2 = null;
            SelectedImageC3 = null;
            SelectedImageC4 = null;
        }

        /// <summary>
        /// Get laser info from image extracted from older version of the software
        /// </summary>
        /// <param name="imgInfo"></param>
        /// <param name="laserType"></param>
        /// <param name="apdGain"></param>
        /// <param name="apdPga"></param>
        /// <param name="laserSignalLevel"></param>
        /// <param name="laserIntensity"></param>
        private void GetLaserInfo(ImageInfo imgInfo, ImagingSystem.LaserType laserType, ref int apdGain, ref int apdPga, ref int laserSignalLevel, ref int laserIntensity)
        {
            var intLevels = imgInfo.IntensityLevel.Split('/');
            //ImagingSystem.LaserType enumLaserType;
            //Enum.TryParse(laserType, out enumLaserType);
            if (laserType == ImagingSystem.LaserType.LaserA)
            {
                apdGain = imgInfo.ApdAGain;
                apdPga = imgInfo.ApdAPga;
                laserIntensity = imgInfo.LaserAIntensity;
                //laserSignalLevel = int.Parse(intLevels[0]);
                int nLevel = 0;
                bool bIsSuccessful = Int32.TryParse(intLevels[0], out nLevel);
                if (bIsSuccessful)
                {
                    laserSignalLevel = nLevel;
                }
            }
            else if (laserType == ImagingSystem.LaserType.LaserB)
            {
                apdGain = imgInfo.ApdBGain;
                apdPga = imgInfo.ApdBPga;
                laserIntensity = imgInfo.LaserBIntensity;
                //laserSignalLevel = int.Parse(intLevels[1]);
                int nLevel = 0;
                bool bIsSuccessful = Int32.TryParse(intLevels[0], out nLevel);
                if (bIsSuccessful)
                {
                    laserSignalLevel = nLevel;
                }
            }
            else if (laserType == ImagingSystem.LaserType.LaserC)
            {
                apdGain = imgInfo.ApdCGain;
                apdPga = imgInfo.ApdCPga;
                laserIntensity = imgInfo.LaserCIntensity;
                //laserSignalLevel = int.Parse(intLevels[2]);
                int nLevel = 0;
                bool bIsSuccessful = Int32.TryParse(intLevels[0], out nLevel);
                if (bIsSuccessful)
                {
                    laserSignalLevel = nLevel;
                }
            }
            else if (laserType == ImagingSystem.LaserType.LaserD)
            {
                apdGain = imgInfo.ApdDGain;
                apdPga = imgInfo.ApdDPga;
                laserIntensity = imgInfo.LaserDIntensity;
                //laserSignalLevel = int.Parse(intLevels[3]);
                int nLevel = 0;
                bool bIsSuccessful = Int32.TryParse(intLevels[0], out nLevel);
                if (bIsSuccessful)
                {
                    laserSignalLevel = nLevel;
                }
            }
        }
    }
}
