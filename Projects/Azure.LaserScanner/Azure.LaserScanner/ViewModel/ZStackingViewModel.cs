using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Azure.Image.Processing;
using Azure.WPF.Framework;
using AnimatedGif;
using System.Windows;

namespace Azure.LaserScanner.ViewModel
{
    public class ZStackingViewModel : ViewModelBase
    {
        public event EventHandler OnRequestClose;

        #region Private members...

        private ObservableCollection<FileViewModel> _SourceFiles;
        private FileViewModel _SelectedSourceFile;
        private ObservableCollection<FileViewModel> _ZStackingFiles;

        private BitmapImage _ZStackedImage = null;
        private Dictionary<string, ZProjector.ProjectionType> _ProjectionTypeOptions;
        private ZProjector.ProjectionType _SelectedProjectionType = ZProjector.ProjectionType.MAX_METHOD;

        #endregion

        #region Constructors...

        public ZStackingViewModel()
        {
            _ZStackingFiles = new ObservableCollection<FileViewModel>();
            _ProjectionTypeOptions = new Dictionary<string, ZProjector.ProjectionType>();
            _ProjectionTypeOptions.Add("Average Intensity", ZProjector.ProjectionType.AVG_METHOD);
            _ProjectionTypeOptions.Add("Max Intensity", ZProjector.ProjectionType.MAX_METHOD);
            _ProjectionTypeOptions.Add("Min Intensity", ZProjector.ProjectionType.MIN_METHOD);
            _ProjectionTypeOptions.Add("Sum Slices", ZProjector.ProjectionType.SUM_METHOD);
            _ProjectionTypeOptions.Add("Standard Deviation", ZProjector.ProjectionType.SD_METHOD);
            _ProjectionTypeOptions.Add("Median", ZProjector.ProjectionType.MEDIAN_METHOD);
        }

        #endregion

        #region Public properties...

        public ObservableCollection<FileViewModel> SourceFiles
        {
            get { return _SourceFiles; }
            set
            {
                _SourceFiles = value;
                RaisePropertyChanged("SourceFiles");
            }
        }

        public FileViewModel SelectedSourceFile
        {
            get { return _SelectedSourceFile; }
            set
            {
                _SelectedSourceFile = value;
                RaisePropertyChanged("SelectedSourceFile");
            }
        }

        public ObservableCollection<FileViewModel> ZStackingFiles
        {
            get { return _ZStackingFiles; }
            set
            {
                _ZStackingFiles = value;
                RaisePropertyChanged("ZStackingFiles");
            }
        }

        public FileViewModel SelectedZStackingFile
        {
            get;
            set;
        }

        public BitmapImage ZStackedImage
        {
            get { return _ZStackedImage; }
            set
            {
                _ZStackedImage = value;
                RaisePropertyChanged("ZStackedImage");
            }
        }

        public Dictionary<string, ZProjector.ProjectionType> ProjectionTypeOptions
        {
            get { return _ProjectionTypeOptions; }
            set
            {
                _ProjectionTypeOptions = value;
                RaisePropertyChanged("ProjectionTypeOptions");
            }
        }
        public ZProjector.ProjectionType SelectedProjectionType
        {
            get { return _SelectedProjectionType; }
            set
            {
                _SelectedProjectionType = value;
                RaisePropertyChanged("SelectedProjectionType");
            }
        }

        //public bool IsCloseDialog
        //{
        //    get { return _IsCloseDialog; }
        //    set
        //    {
        //        _IsCloseDialog = value;
        //        RaisePropertyChanged("IsCloseDialog");
        //    }
        //}

        #endregion

        #region AddFileCommand

        private RelayCommand _AddFileCommand = null;
        public ICommand AddFileCommand
        {
            get
            {
                if (_AddFileCommand == null)
                {
                    _AddFileCommand = new RelayCommand(ExecuteAddFileCommand, CanExecuteAddFileCommand);
                }

                return _AddFileCommand;
            }
        }
        protected void ExecuteAddFileCommand(object parameter)
        {
            if (SourceFiles != null && SourceFiles.Count > 0 && SelectedSourceFile != null)
            {
                if (SelectedSourceFile.Image.Format.BitsPerPixel == 16 || SelectedSourceFile.Image.Format.BitsPerPixel == 8)
                {
                    ZStackingFiles.Add(SelectedSourceFile);
                    SourceFiles.Remove(SelectedSourceFile);
                    SelectedSourceFile = SourceFiles.FirstOrDefault();
                }
                else
                {
                    string caption = "Z Stack";
                    string message = "The images format is not supported.\nThe image must be an 8 or 16 bits per pixel.";
                    Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                }
            }
        }
        protected bool CanExecuteAddFileCommand(object parameter)
        {
            //return true;
            return (SelectedSourceFile != null);
        }
        #endregion

        #region RemoveFileCommand

        private RelayCommand _RemoveFileCommand = null;
        public ICommand RemoveFileCommand
        {
            get
            {
                if (_RemoveFileCommand == null)
                {
                    _RemoveFileCommand = new RelayCommand(ExecuteRemoveFileCommand, CanExecuteRemoveFileCommand);
                }

                return _RemoveFileCommand;
            }
        }
        protected void ExecuteRemoveFileCommand(object parameter)
        {
            if (ZStackingFiles != null && ZStackingFiles.Count > 0 && SelectedZStackingFile != null)
            {
                SourceFiles.Add(SelectedZStackingFile);
                ZStackingFiles.Remove(SelectedZStackingFile);
            }
        }
        protected bool CanExecuteRemoveFileCommand(object parameter)
        {
            //return true;
            return (SelectedZStackingFile != null);
        }
        #endregion

        #region CreateZStackedImageCommand

        private RelayCommand _CreateZStackedImageCommand = null;
        public ICommand CreateZStackedImageCommand
        {
            get
            {
                if (_CreateZStackedImageCommand == null)
                {
                    _CreateZStackedImageCommand = new RelayCommand(ExecuteCreateZStackedImageCommand, CanExecuteCreateZStackedImageCommand);
                }

                return _CreateZStackedImageCommand;
            }
        }
        protected void ExecuteCreateZStackedImageCommand(object parameter)
        {
            if (ZStackingFiles != null && ZStackingFiles.Count > 1)
            {
                try
                {
                    // Validate images to be stacked
                    int width = ZStackingFiles[0].Image.PixelWidth;
                    int height = ZStackingFiles[0].Image.PixelHeight;
                    int bpp = ZStackingFiles[0].Image.Format.BitsPerPixel;
                    for (int i = 1; i< ZStackingFiles.Count; i++)
                    {
                        if (ZStackingFiles[i].Image.PixelWidth != width ||
                            ZStackingFiles[i].Image.PixelHeight != height ||
                            ZStackingFiles[i].Image.Format.BitsPerPixel != bpp)
                        {
                            string caption = "Z Stack";
                            string message = "The images to be stacked must be of the same size and bit depth.";
                            Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                            return;
                        }
                    }

                    ZProjector zproj = new ZProjector();
                    List<WriteableBitmap> projImages = new List<WriteableBitmap>();
                    foreach (var file in ZStackingFiles)
                    {
                        projImages.Add(file.Image);
                    }
                    zproj.SetImage(projImages);
                    zproj.ProjectionMethod = SelectedProjectionType;
                    zproj.DoProjection();

                    WriteableBitmap projImage = zproj.ProjImage;
                    ImageInfo imgInfo = (ImageInfo)ZStackingFiles[0].ImageInfo.Clone();
                    string docTitle = "Z-Projection";
                    switch(SelectedProjectionType)
                    {
                        case ZProjector.ProjectionType.AVG_METHOD:
                            docTitle += "_AVG";
                            break;
                        case ZProjector.ProjectionType.MAX_METHOD:
                            docTitle += "_MAX";
                            break;
                        case ZProjector.ProjectionType.MEDIAN_METHOD:
                            docTitle += "_MIDIAN";
                            break;
                        case ZProjector.ProjectionType.MIN_METHOD:
                            docTitle += "_MIN";
                            break;
                        case ZProjector.ProjectionType.SD_METHOD:
                            docTitle += "_SD";
                            break;
                        case ZProjector.ProjectionType.SUM_METHOD:
                            docTitle += "_SUM";
                            break;
                    }

                    foreach (var file in Workspace.This.Files)
                    {
                        if (file.FileName.Contains(docTitle))
                        {
                            docTitle = Workspace.This.GetUniqueFilename(docTitle);
                        }
                    }

                    // Close dialog window request
                    OnRequestClose(this, new EventArgs());

                    Workspace.This.NewDocument(projImage, imgInfo, docTitle, null, false, true);
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        protected bool CanExecuteCreateZStackedImageCommand(object parameter)
        {
            return (_ZStackingFiles.Count > 1);
        }

        #endregion
    }


}
