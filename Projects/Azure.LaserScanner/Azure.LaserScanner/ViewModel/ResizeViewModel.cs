using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Azure.WPF.Framework;
using Azure.Configuration.Settings;
using Azure.Image.Processing;

namespace Azure.LaserScanner.ViewModel
{
    public class ResizeViewModel : ViewModelBase
    {
        private FileViewModel _ActiveDocument;
        private int _ActualWidthInPixels;
        private int _ActualHeightInPixels;
        private string _SelectedPixelsUnit = "pixels";
        private double _ActualWidthInInchCm;
        private double _ActualHeightInInchCm;
        private double _ActualImagePPI;
        private string _ActualSelectedUnit;
        private double _WidthInInchCm;
        private double _HeightInInchCm;
        private string _SelectedConvUnit = "inch";  //inch or cm
        private string _DocSelectedUnit = "dots/inch";     //dots/inch or dots/cm
        private int _CurrentWidthInPixels;
        private int _CurrentHeightInPixels;
        private double _CurrentImageDpi;
        private bool _IsConvToCm = false;

        public ResizeViewModel()
        {
        }

        public FileViewModel ActiveDocument
        {
            get { return _ActiveDocument; }
            set
            {
                _ActiveDocument = value;

                if (_ActiveDocument != null && _ActiveDocument.Image != null)
                {
                    _ActualWidthInPixels = _CurrentWidthInPixels = _ActiveDocument.Image.PixelWidth;
                    _ActualHeightInPixels = _CurrentHeightInPixels = _ActiveDocument.Image.PixelHeight;
                    _ActualImagePPI = _ActiveDocument.Image.DpiX;
                    _CurrentImageDpi = _ActiveDocument.Image.DpiX;
                    _IsConvToCm = false;

                    if (_DocSelectedUnit == "dots/inch")
                    {
                        _WidthInInchCm = (double)_ActualWidthInPixels / _CurrentImageDpi;
                        _HeightInInchCm = (double)_ActualHeightInPixels / _CurrentImageDpi;
                        _SelectedConvUnit = "inch";
                        _ActualWidthInInchCm = (double)_ActualWidthInPixels / _ActualImagePPI;
                        _ActualHeightInInchCm = (double)_ActualHeightInPixels / _ActualImagePPI;
                        _ActualSelectedUnit = "pixels/inch";
                    }
                    else if (_DocSelectedUnit == "dots/cm")
                    {
                        _CurrentImageDpi = _CurrentImageDpi / 2.54;
                        _IsConvToCm = true;
                        _WidthInInchCm = (double)_ActualWidthInPixels / _CurrentImageDpi;
                        _HeightInInchCm = (double)_ActualHeightInPixels / _CurrentImageDpi;
                        _SelectedConvUnit = "cm";
                        _ActualImagePPI /= 2.54;
                        _ActualWidthInInchCm = (double)_ActualWidthInPixels / _ActualImagePPI;
                        _ActualHeightInInchCm = (double)_ActualHeightInPixels / _ActualImagePPI;
                        _ActualSelectedUnit = "pixels/cm";
                    }
                }
                else
                {
                    _ActualWidthInPixels = 0;
                    _ActualHeightInPixels = 0;
                    _ActualWidthInInchCm = 0;
                    _ActualHeightInInchCm = 0;
                    _ActualImagePPI = 0;
                    _WidthInInchCm = 0;
                    _HeightInInchCm = 0;
                    _CurrentWidthInPixels = 0;
                    _CurrentHeightInPixels = 0;
                    _CurrentImageDpi = 0;
                }
                RaisePropertyChanged("ActualWidthInPixels");
                RaisePropertyChanged("ActualHeightInPixels");
                RaisePropertyChanged("ActualWidthInInchCm");
                RaisePropertyChanged("ActualHeightInInchCm");
                RaisePropertyChanged("ActualImagePPI");
                RaisePropertyChanged("ActualSelectedUnit");
                RaisePropertyChanged("WidthInInchCm");
                RaisePropertyChanged("HeightInInchCm");
                RaisePropertyChanged("SelectedConvUnit");
                RaisePropertyChanged("CurrentWidthInPixels");
                RaisePropertyChanged("CurrentHeightInPixels");
                RaisePropertyChanged("CurrentImageDpi");
            }
        }

        public int ActualWidthInPixels
        {
            get
            {
                return _ActualWidthInPixels;
            }
            set
            {
                if (_ActualWidthInPixels != value)
                {
                    _ActualWidthInPixels = value;
                    RaisePropertyChanged("ActualWidthInPixels");
                }
            }
        }
        public int ActualHeightInPixels
        {
            get
            {
                return _ActualHeightInPixels;
            }
            set
            {
                if (_ActualHeightInPixels != value)
                {
                    _ActualHeightInPixels = value;
                    RaisePropertyChanged("ActualHeightInPixels");
                }
            }
        }
        public List<string> PixelsUnitOptions
        {
            get
            {
                return new List<string>()
                {
                    "pixels",
                    "percent",
                };
            }
        }
        public string SelectedPixelsUnit
        {
            get { return _SelectedPixelsUnit; }
            set
            {
                _SelectedPixelsUnit = value;
                RaisePropertyChanged("SelectedPixelsUnit");
            }
        }

        public double ActualWidthInInchCm
        {
            get { return _ActualWidthInInchCm; }
            set
            {
                _ActualWidthInInchCm = value;
                RaisePropertyChanged("ActualWidthInInchCm");
            }
        }
        public double ActualHeightInInchCm
        {
            get { return _ActualHeightInInchCm; }
            set
            {
                _ActualHeightInInchCm = value;
                RaisePropertyChanged("ActualHeightInInchCm");
            }
        }
        public double ActualImagePPI
        {
            get { return _ActualImagePPI; }
            set
            {
                _ActualImagePPI = value;
                RaisePropertyChanged("ActualImagePPI");
            }
        }
        public string ActualSelectedUnit
        {
            get { return _ActualSelectedUnit; }
            set
            {
                _ActualSelectedUnit = value;
                RaisePropertyChanged("ActualSelectedUnit");
            }
        }

        public double WidthInInchCm
        {
            get
            {
                return _WidthInInchCm;
            }
            set
            {
                if (_WidthInInchCm != value)
                {
                    _WidthInInchCm = value;
                    RaisePropertyChanged("WidthInInchCm");
                }
            }
        }
        public double HeightInInchCm
        {
            get
            {
                return _HeightInInchCm;
            }
            set
            {
                if (_HeightInInchCm != value)
                {
                    _HeightInInchCm = value;
                    RaisePropertyChanged("HeightInInchCm");
                }
            }
        }

        public List<string> ConvUnitOptions
        {
            get
            {
                return new List<string>()
                {
                    "inch",
                    "cm",
                };
            }
        }
        public string SelectedConvUnit
        {
            get { return _SelectedConvUnit; }
            set
            {
                _SelectedConvUnit = value;
                RaisePropertyChanged("SelectedConvUnit");
            }
        }

        public List<string> DocUnitOptions
        {
            get
            {
                return new List<string>()
                {
                    "dots/inch",
                    "dots/cm",
                };
            }
        }
        public string DocSelectedUnit
        {
            get { return _DocSelectedUnit; }
            set
            {
                if (_DocSelectedUnit != value)
                {
                    _DocSelectedUnit = value;
                    RaisePropertyChanged("DocSelectedUnit");

                    if (ActiveDocument != null && ActiveDocument.Image != null)
                    {
                        if (_DocSelectedUnit == "dots/inch")
                        {
                            if (_IsConvToCm)
                            {
                                _CurrentImageDpi = _CurrentImageDpi * 2.54;
                                _ActualImagePPI = _ActualImagePPI * 2.54;
                                _IsConvToCm = false;
                            }

                            _WidthInInchCm = (double)_CurrentWidthInPixels / _CurrentImageDpi;
                            _HeightInInchCm = (double)_CurrentHeightInPixels / _CurrentImageDpi;
                            _ActualWidthInInchCm = (double)_ActualWidthInPixels / _ActualImagePPI;
                            _ActualHeightInInchCm = (double)_ActualHeightInPixels / _ActualImagePPI;
                            _SelectedConvUnit = "inch";
                            _ActualSelectedUnit = "pixels/inch";
                        }
                        else if (_DocSelectedUnit == "dots/cm")
                        {
                            if (!_IsConvToCm)
                            {
                                _CurrentImageDpi = _CurrentImageDpi / 2.54;
                                _ActualImagePPI = _ActualImagePPI / 2.54;
                                _IsConvToCm = true;
                            }
                            _WidthInInchCm = (double)_CurrentWidthInPixels / _CurrentImageDpi;
                            _HeightInInchCm = (double)_CurrentHeightInPixels / _CurrentImageDpi;
                            _ActualWidthInInchCm = (double)_ActualWidthInPixels / _ActualImagePPI;
                            _ActualHeightInInchCm = (double)_ActualHeightInPixels / _ActualImagePPI;
                            _SelectedConvUnit = "cm";
                            _ActualSelectedUnit = "pixels/cm";
                        }
                        RaisePropertyChanged("SelectedConvUnit");
                        RaisePropertyChanged("WidthInInchCm");
                        RaisePropertyChanged("HeightInInchCm");
                        RaisePropertyChanged("CurrentImageDpi");
                        RaisePropertyChanged("ActualWidthInInchCm");
                        RaisePropertyChanged("ActualHeightInInchCm");
                        RaisePropertyChanged("ActualImagePPI");
                        RaisePropertyChanged("ActualSelectedUnit");
                    }
                }
            }
        }

        public int CurrentWidthInPixels
        {
            get
            {
                return _CurrentWidthInPixels;
            }
            set
            {
                if (_CurrentWidthInPixels != value)
                {
                    _CurrentWidthInPixels = value;
                    RaisePropertyChanged("CurrentWidthInPixels");

                    // Calculate the height (keep aspect ratio).
                    _CurrentHeightInPixels = (int)Math.Round((double)ActiveDocument.Image.PixelHeight * (double)_CurrentWidthInPixels / (double)ActiveDocument.Image.PixelWidth);
                    RaisePropertyChanged("CurrentHeightInPixels");

                    _WidthInInchCm = (double)_CurrentWidthInPixels / _CurrentImageDpi;
                    _HeightInInchCm = (double)_CurrentHeightInPixels / _CurrentImageDpi;
                    RaisePropertyChanged("WidthInInchCm");
                    RaisePropertyChanged("HeightInInchCm");
                }
            }
        }
        public int CurrentHeightInPixels
        {
            get
            {
                return _CurrentHeightInPixels;
            }
            set
            {
                if (_CurrentHeightInPixels != value)
                {
                    _CurrentHeightInPixels = value;
                    RaisePropertyChanged("CurrentHeightInPixels");

                    // Calculate the width (keep aspect ratio).
                    _CurrentWidthInPixels = (int)Math.Round((double)ActiveDocument.Image.PixelWidth * (double)_CurrentHeightInPixels / (double)ActiveDocument.Image.PixelHeight);
                    RaisePropertyChanged("CurrentWidthInPixels");

                    _WidthInInchCm = (double)_CurrentWidthInPixels / _CurrentImageDpi;
                    _HeightInInchCm = (double)_CurrentHeightInPixels / _CurrentImageDpi;
                    RaisePropertyChanged("WidthInInchCm");
                    RaisePropertyChanged("HeightInInchCm");
                }
            }
        }

        public double CurrentImageDpi
        {
            get
            {
                return _CurrentImageDpi;
            }
            set
            {
                if (_CurrentImageDpi != value)
                {
                    _CurrentImageDpi = value;
                    RaisePropertyChanged("CurrentImageDpi");
                    if (_CurrentImageDpi > 0)
                    {
                        _WidthInInchCm = (double)_ActualWidthInPixels / _CurrentImageDpi;
                        _HeightInInchCm = (double)_ActualHeightInPixels / _CurrentImageDpi;
                        RaisePropertyChanged("WidthInInchCm");
                        RaisePropertyChanged("HeightInInchCm");
                    }
                }
            }
        }

        #region ResizeImageCommand
        private RelayCommand _ResizeImageCommand = null;
        public ICommand ResizeImageCommand
        {
            get
            {
                if (_ResizeImageCommand == null)
                {
                    _ResizeImageCommand = new RelayCommand(this.ExecuteResizeImageCommand, this.CanExecuteResizeImageCommand);
                }
                return _ResizeImageCommand;
            }
        }
        protected void ExecuteResizeImageCommand(object parameter)
        {
            if (ActiveDocument != null)
            {
                try
                {
                    string oldTitle = Path.GetFileName(ActiveDocument.Title);

                    int nImageWidth = ActiveDocument.Image.PixelWidth;
                    int nImageHeight = ActiveDocument.Image.PixelHeight;
                    double dImageDpi = ActiveDocument.Image.DpiX;
                    double dpiToBeSet = _CurrentImageDpi;

                    if (_IsConvToCm)
                    {
                        dpiToBeSet = _CurrentImageDpi * 2.54;
                    }

                    if (_CurrentImageDpi != dImageDpi)
                    {
                        ActiveDocument.ChangeImageDpi(dpiToBeSet, dpiToBeSet);
                        Xceed.Wpf.Toolkit.MessageBox.Show("Image DPI changed.", "Change Image DPI...", MessageBoxButton.OK, MessageBoxImage.Information);

                        //if (_MainViewModel.Owner.ImagingSysSettings.IsQcVersion)
                        //{
                        //    ActivityLog newLog = new ActivityLog(_MainViewModel.Owner.LoginUser.UserName);
                        //    newLog.LogEditImage(oldTitle, string.Format("Resize {0}x{1}, DPI={2},", nImageWidth, nImageHeight, dpiToBeSet));
                        //    _MainViewModel.Owner.ManageUsersVM.LogActivity(newLog);
                        //}
                    }
                }
                catch (Exception ex)
                {
                    string caption = "Image resize...";
                    string message = string.Format("Error resizing the image.\nERROR: {0}", ex.Message);
                    Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                }
            }
        }
        protected bool CanExecuteResizeImageCommand(object parameter)
        {
            return (ActiveDocument != null);
        }
        #endregion

        #region SaveAsPubFileCommand
        private RelayCommand _SaveAsPubFileCommand = null;
        public ICommand SaveAsPubFileCommand
        {
            get
            {
                if (_SaveAsPubFileCommand == null)
                {
                    _SaveAsPubFileCommand = new RelayCommand(this.ExecuteSaveAsPubFileCommand, this.CanExecuteSaveAsPubFileCommand);
                }
                return _SaveAsPubFileCommand;
            }
        }
        protected void ExecuteSaveAsPubFileCommand(object parameter)
        {
            if (ActiveDocument != null)
            {
                try
                {
                    //string oldTitle = Path.GetFileName(ActiveDocument.Title);
                    int nImageWidth = ActiveDocument.Image.PixelWidth;
                    int nImageHeight = ActiveDocument.Image.PixelHeight;
                    double dImageDpi = ActiveDocument.Image.DpiX;
                    double dpiToBeSet = _CurrentImageDpi;
                    string fileName = string.Empty;
                    string filePath = string.Empty;
                    int fileType = ImageProcessing.TIFF_FILE;

                    if (_IsConvToCm)
                    {
                        dpiToBeSet = _CurrentImageDpi * 2.54;
                    }
                    //else
                    //    dpiToBeSet = _CurrentImageDpi / 2.54;

                    var dlg = new SaveFileDialog();
                    dlg.Filter = "TIFF (PUB) | *.tif; *.tiff | JPEG (*.jpg; *.jpeg) | *.jpg; *.jpeg | BMP (*.bmp) | *.bmp";    // Filter files by extension
                    dlg.Title = "Save an image as PUB file";
                    dlg.DefaultExt = ".tif"; // Default file extension
                    dlg.FilterIndex = 1;
                    //dlg.AddExtension = true;
                    //dlg.CheckFileExists = true;
                    //dlg.OverwritePrompt = false;

                    // Get/set file name in SaveFileDialog
                    if (Workspace.This.CheckSupportedFileType(ActiveDocument.Title))
                    {
                        fileName = Path.GetFileNameWithoutExtension(ActiveDocument.Title);
                    }
                    else
                    {
                        fileName = ActiveDocument.Title;
                    }
                    fileName = fileName +"_PUB_" + Math.Round(dpiToBeSet, 2);

                    // Get/set initial destination folder in SaveFileDialog
                    string initialDirectory = string.Empty;
                    if (ActiveDocument.FilePath == null || string.IsNullOrEmpty(ActiveDocument.FilePath))
                    {
                        initialDirectory = SettingsManager.ApplicationSettings.InitialDirectory;
                    }
                    else
                    {
                        initialDirectory = Path.GetDirectoryName(ActiveDocument.FilePath);
                    }
                    if (string.IsNullOrEmpty(initialDirectory) || !System.IO.Directory.Exists(initialDirectory))
                    {
                        initialDirectory = "D:\\";
                    }
                    dlg.FileName = fileName;
                    dlg.InitialDirectory = initialDirectory;
                    if (dlg.ShowDialog().GetValueOrDefault())
                    {
                        filePath = dlg.FileName;
                        if (dlg.FilterIndex == 1)
                            fileType = ImageProcessing.TIFF_FILE;
                        else if (dlg.FilterIndex == 2)
                            fileType = ImageProcessing.JPG_FILE;
                        else if (dlg.FilterIndex == 3)
                            fileType = ImageProcessing.BMP_FILE;
                    }
                    else
                    {
                        return;
                    }

                    try
                    {
                        Workspace.This.StartWaitAnimation("Saving PUB file...");

                        WriteableBitmap imageToBeSaved = ActiveDocument.DisplayImage.Clone();
                        if (nImageWidth != _CurrentWidthInPixels || nImageHeight != _CurrentHeightInPixels)
                        {
                            imageToBeSaved = ImageProcessing.Resize(imageToBeSaved, new Size(_CurrentWidthInPixels, _CurrentHeightInPixels));
                        }

                        if (imageToBeSaved != null)
                        {
                            SaveFileAsPub(imageToBeSaved, (ImageInfo)ActiveDocument.ImageInfo.Clone(), filePath, fileType, dpiToBeSet);
                        }
                    }
                    catch (Exception ex)
                    {
                        Workspace.This.StopWaitAnimation();
                        throw new Exception("Image Resize Error: " + ex.Message);
                    }
                    finally
                    {
                        //CurrentImageDpi = dImageDpi;    // restore actual DPI value;
                        Workspace.This.StopWaitAnimation();
                    }

                    //if (_MainViewModel.Owner.ImagingSysSettings.IsQcVersion)
                    //{
                    //    ActivityLog newLog = new ActivityLog(_MainViewModel.Owner.LoginUser.UserName);
                    //    newLog.LogEditImage(ActiveDocument.Title, string.Format("Resize {0}x{1}, DPI={2},", nImageWidth, nImageHeight, dpiToBeSet));
                    //    _MainViewModel.Owner.ManageUsersVM.LogActivity(newLog);
                    //}

                    string caption = "Image Resize...";
                    string message = string.Format("The image is successfully saved as PUB file. (DPI: {0})", dpiToBeSet);
                    Xceed.Wpf.Toolkit.MessageBox.Show(Workspace.This.Owner, message, caption, MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    string caption = "Image resize (8-bit)...";
                    string message = string.Format("Error resizing the image.\nERROR: {0}", ex.Message);
                    Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                }
            }
        }
        protected bool CanExecuteSaveAsPubFileCommand(object parameter)
        {
            return (ActiveDocument != null);
        }
        #endregion

        private void SaveFileAsPub(WriteableBitmap imageToSave, ImageInfo imgInfo, string filePath, int fileType, double dpi)
        {
            //var imgInfo = (ImageInfo)fileToSave.ImageInfo.Clone();
            //var imageToSave = (WriteableBitmap)fileToSave.DisplayImage.Clone();

            if (imageToSave.CanFreeze)
                imageToSave.Freeze();

            // Reset the contrast value to 8-bit per channel compatible
            imgInfo.RedChannel.BlackValue = 0;
            imgInfo.RedChannel.WhiteValue = 255;
            imgInfo.GreenChannel.BlackValue = 0;
            imgInfo.GreenChannel.WhiteValue = 255;
            imgInfo.BlueChannel.BlackValue = 0;
            imgInfo.BlueChannel.WhiteValue = 255;
            imgInfo.MixChannel.BlackValue = 0;
            imgInfo.MixChannel.WhiteValue = 255;
            imgInfo.IsChemiImage = false;
            imgInfo.MixChannel.IsInvertChecked = false;
            imgInfo.SelectedChannel = ImageChannelType.Mix;
            imgInfo.IsGrayChannelAvail = false;

            bool bIs4ChannelImage = (imageToSave.Format.BitsPerPixel == 32 || imageToSave.Format.BitsPerPixel == 64);
            if (bIs4ChannelImage)
            {
                imgInfo.RedChannel.FilterPosition = 0;
                imgInfo.GreenChannel.FilterPosition = 0;
                imgInfo.BlueChannel.FilterPosition = 0;
                imgInfo.GrayChannel.FilterPosition = 0;
                imgInfo.MixChannel.FilterPosition = 0;
                imgInfo.RedChannel.LightSource = 0;
                imgInfo.GreenChannel.LightSource = 0;
                imgInfo.BlueChannel.LightSource = 0;
                imgInfo.GrayChannel.LightSource = 0;
                imgInfo.MixChannel.LightSource = 0;
            }

            if (imageToSave.DpiX != dpi)
            {
                imageToSave = ImageProcessing.SetBitmapDpi(imageToSave, dpi, dpi);
                if (imageToSave.CanFreeze)
                    imageToSave.Freeze();
            }
            ImageProcessing.Save(filePath, imageToSave, imgInfo, fileType, false, true);

            /*BackgroundWorker saveFileWorker = new BackgroundWorker();

            // Save the document in a different thread
            saveFileWorker.DoWork += (o, ea) =>
            {
                if (imageToSave.DpiX != dpi)
                {
                    imageToSave = ImageProcessing.SetBitmapDpi(imageToSave, dpi, dpi);
                    if (imageToSave.CanFreeze)
                        imageToSave.Freeze();
                }
                ImageProcessing.Save(filePath, imageToSave, imgInfo, ImageProcessing.TIFF_FILE, false, true);
            };
            saveFileWorker.RunWorkerCompleted += (o, ea) =>
            {
                if (ea.Cancelled)
                {
                    // operation cancelled
                }
                else if (ea.Error != null)
                {
                    // error occurred
                    throw ea.Error;
                }
                else
                {
                    // Remember initial directory
                    SettingsManager.ApplicationSettings.InitialDirectory = Path.GetDirectoryName(filePath);
                }
            };

            saveFileWorker.RunWorkerAsync();*/
        }

    }
}
