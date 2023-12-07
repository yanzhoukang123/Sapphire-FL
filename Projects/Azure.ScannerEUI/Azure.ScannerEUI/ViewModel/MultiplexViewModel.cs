using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Azure.WPF.Framework;

namespace Azure.ScannerEUI.ViewModel
{
    class MultiplexViewModel : ViewModelBase
    {
        #region Private field/data...

        private ObservableCollection<FileViewModel> _Files = new ObservableCollection<FileViewModel>();
        private FileViewModel _SelectedImageC1 = null;
        private FileViewModel _SelectedImageC2 = null;
        private FileViewModel _SelectedImageC3 = null;
        private FileViewModel _SelectedImageC4 = null;

        #endregion

        #region Public properties...

        public FileViewModel SelectedImageC1
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
            WriteableBitmap[] srcImages = { null, null, null };
            List<WriteableBitmap> imageList = new List<WriteableBitmap>();

            if (SelectedImageC1 != null)
            {
                srcImages[0] = SelectedImageC1.Image;
                imageList.Add(SelectedImageC1.Image);
            }
            if (SelectedImageC2 != null)
            {
                srcImages[1] = SelectedImageC2.Image;
                imageList.Add(SelectedImageC2.Image);
            }
            if (SelectedImageC3 != null)
            {
                srcImages[2] = SelectedImageC3.Image;
                imageList.Add(SelectedImageC3.Image);
            }

            if (imageList.Count < 2)
            {
                srcImages = null;
                imageList.Clear();
                imageList = null;
                string caption = "Merge image...";
                string message = "Please select more than 1 image to merge.";
                Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                return;
            }

            // Validate image with, height and bit depth
            WriteableBitmap firstImage = imageList[0];
            bool bIsValidFiles = false;
            for (int i = 1; i < imageList.Count; i++)
            {
                if (firstImage.PixelWidth == imageList[i].PixelWidth &&
                    firstImage.PixelHeight == imageList[i].PixelHeight &&
                    firstImage.Format.BitsPerPixel == imageList[i].Format.BitsPerPixel)
                {
                    bIsValidFiles = true;
                }
                else
                {
                    bIsValidFiles = false;
                    break;
                }
            }

            if (!bIsValidFiles)
            {
                srcImages = null;
                imageList.Clear();
                imageList = null;
                string caption = "Invalid files...";
                string message = "The source images must have the same width, height and bit depth";
                Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                return;
            }

            Azure.Image.Processing.ImageInfo imageInfo = null;
            foreach (var file in Files)
            {
                if (file.ImageInfo != null)
                {
                    imageInfo = (Azure.Image.Processing.ImageInfo)file.ImageInfo.Clone();
                    break;
                }
            }

            if (imageInfo == null)
                imageInfo = new Image.Processing.ImageInfo();

            imageInfo.SelectedChannel = Image.Processing.ImageChannelType.Mix;
            imageInfo.ChannelRemark = "";
            if (SelectedImageC1 != null)
                imageInfo.ChannelRemark += "Red_" + SelectedImageC1.ImageInfo.ChannelRemark + "_";
            if (SelectedImageC2 != null)
                imageInfo.ChannelRemark += "Green_" + SelectedImageC2.ImageInfo.ChannelRemark + "_";
            if (SelectedImageC3 != null)
                imageInfo.ChannelRemark += "Blue_" + SelectedImageC3.ImageInfo.ChannelRemark + "_";
            // merge image channels
            WriteableBitmap mergedImage = Azure.Image.Processing.ImageProcessing.SetChannel(srcImages[0], srcImages[1], srcImages[2]);
            // Add to Gallery
            string title = GetUniqueFilename("Composite");
            Workspace.This.NewDocument(mergedImage, imageInfo, title, false, true);
            srcImages = null;
            imageList.Clear();
            imageList = null;
            // Close/hide multiplex control
            Workspace.This.IsMultiplexChecked = false;
        }

        private bool CanExecuteMergeChannelsCommand(object parameter)
        {
            return true;
        }

        #endregion 

        private string GetUniqueFilename(string fileName)
        {
            int count = 1;

            string tempFileName = string.Format("{0}{1}", fileName, count);
            string fileNameWithoutExt = string.Empty;
            for (int i = 0; i < Workspace.This.Files.Count; i++)
            {
                fileNameWithoutExt = System.IO.Path.GetFileNameWithoutExtension(Workspace.This.Files[i].Title);
                if (tempFileName.Equals(fileNameWithoutExt, StringComparison.InvariantCultureIgnoreCase))
                {
                    tempFileName = string.Format("{0}{1}", fileName, count++);
                    i = -1; // Reset i to 0; setting it to -1 here because the for loop will do an increment
                }
            }

            return tempFileName;
        }

        public void ResetSelection()
        {
            SelectedImageC1 = null;
            SelectedImageC2 = null;
            SelectedImageC3 = null;
            SelectedImageC4 = null;
        }
    }
}
