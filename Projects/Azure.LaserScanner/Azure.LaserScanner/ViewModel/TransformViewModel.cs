using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Azure.WPF.Framework;
using Azure.Image.Processing;
//using Azure.UserManagement;
using Azure.LaserScanner.View;

namespace Azure.LaserScanner.ViewModel
{
    public class TransformViewModel : ViewModelBase
    {
        private bool _IsRotateArbitraryChecked = false;
        //private bool _IsImageRotating = false;
        //private bool _IsImageRotated = false;
        private double _ArbitraryRotationAngle = 0.0;
        private WriteableBitmap _DisplayImage = null;

        // Image shifting
        private bool _IsImageShiftChecked = false;
        private double _ShiftOffsetX = 0;
        private double _ShiftOffsetY = 0;
        private int _ShiftStepSizeInPixels = 5;
        private bool _IsImageShifting = false;

        public TransformViewModel()
        {
        }

        #region Public properties...

        public bool IsRotateArbitraryChecked
        {
            get { return _IsRotateArbitraryChecked; }
            set
            {
                if (_IsRotateArbitraryChecked != value)
                {
                    _IsRotateArbitraryChecked = value;
                    RaisePropertyChanged("IsRotateArbitraryChecked");
                }
            }
        }

        //public bool IsImageRotating
        //{
        //    get { return _IsImageRotating; }
        //    set
        //    {
        //        if (_IsImageRotating != value)
        //        {
        //            _IsImageRotating = value;
        //            RaisePropertyChanged("IsImageRotating");
        //        }
        //    }
        //}

        //public bool IsImageRotated
        //{
        //    get { return _IsImageRotated; }
        //    set
        //    {
        //        if (_IsImageRotated != value)
        //        {
        //            _IsImageRotated = value;
        //            RaisePropertyChanged("IsImageRotated");
        //        }
        //    }
        //}

        public double ArbitraryRotationAngle
        {
            get { return _ArbitraryRotationAngle; }
            set
            {
                if (_ArbitraryRotationAngle != value)
                {
                    _ArbitraryRotationAngle = value;
                    if (_ArbitraryRotationAngle > -0.10 && _ArbitraryRotationAngle < 0.10)
                    {
                        _ArbitraryRotationAngle = 0.00;
                    }
                    RaisePropertyChanged("ArbitraryRotationAngle");
                }
            }
        }

        public WriteableBitmap DisplayImage
        {
            get { return _DisplayImage; }
            set
            {
                _DisplayImage = value;
                RaisePropertyChanged("DisplayImage");
            }
        }

        public bool IsImageShiftChecked
        {
            get { return _IsImageShiftChecked; }
            set
            {
                _IsImageShiftChecked = value;
                RaisePropertyChanged("IsImageShiftChecked");
            }
        }
        public double ShiftOffsetX
        {
            get { return _ShiftOffsetX; }
            set
            {
                _ShiftOffsetX = value;
                RaisePropertyChanged("ShiftOffsetX");
            }
        }
        public double ShiftOffsetY
        {
            get { return _ShiftOffsetY; }
            set
            {
                _ShiftOffsetY = value;
                RaisePropertyChanged("ShiftOffsetY");
            }
        }
        public int ShiftStepSizeInPixels
        {
            get { return _ShiftStepSizeInPixels; }
            set
            {
                _ShiftStepSizeInPixels = value;
                RaisePropertyChanged("ShiftStepSizeInPixels");
            }
        }
        public bool IsImageShifting
        {
            get { return _IsImageShifting; }
            set
            {
                _IsImageShifting = value;
                RaisePropertyChanged("IsImageShifting");
            }
        }

        #endregion

        #region RotateRight90Command
        private RelayCommand _RotateRight90Command = null;
        /// <summary>
        /// Rotate right 90 degree
        /// </summary>
        public ICommand RotateRight90Command
        {
            get
            {
                if (_RotateRight90Command == null)
                {
                    _RotateRight90Command = new RelayCommand(this.ExecuteRotateRight90Command, CanExecuteRotateRight90Command);
                }

                return _RotateRight90Command;
            }
        }
        protected void ExecuteRotateRight90Command(object parameter)
        {
            if (!Workspace.This.IsActiveDocument) { return; }

            double dAngle = 90.0;
            Workspace.This.ActiveDocument.Image = ImageProcessing.WpfRotate(Workspace.This.ActiveDocument.Image, dAngle);

            // Create a new display image buffer is width or height changed after rotation
            if (Workspace.This.ActiveDocument.Image.PixelWidth != Workspace.This.ActiveDocument.DisplayImage.PixelWidth &&
                Workspace.This.ActiveDocument.Image.PixelHeight != Workspace.This.ActiveDocument.DisplayImage.PixelHeight)
            {
                PixelFormat currDspPixelFormat = Workspace.This.ActiveDocument.DisplayImage.Format;
                int width = Workspace.This.ActiveDocument.Image.PixelWidth;
                int height = Workspace.This.ActiveDocument.Image.PixelHeight;
                double dpiX = Workspace.This.ActiveDocument.DisplayImage.DpiX;
                double dpiY = Workspace.This.ActiveDocument.DisplayImage.DpiY;
                BitmapPalette palette = Workspace.This.ActiveDocument.DisplayImage.Palette;
                Workspace.This.ActiveDocument.DisplayImage = null;
                Workspace.This.ActiveDocument.DisplayImage = new WriteableBitmap(width, height, dpiX, dpiY, currDspPixelFormat, palette);
            }

            if (Workspace.This.ActiveDocument.DocDirtyType != DirtyType.NewCreate)
            {
                Workspace.This.ActiveDocument.DocDirtyType = DirtyType.Modified;
            }

            /*if (_MainViewModel.ActiveDocument.ImageInfo.NumOfChannels > 1)
            {
                int nColorGradation = _MainViewModel.ActiveDocument.ImageInfo.NumOfChannels;
                // Is an individual channel currently selected?
                if (_MainViewModel.ActiveDocument.ImageInfo.SelectedChannel != ImageChannelType.Mix)
                {
                    if (_MainViewModel.ActiveDocument.ImageInfo.NumOfChannels > 1)
                    {
                        nColorGradation = ((int)_MainViewModel.ActiveDocument.SelectedChannel) - 1;
                    }
                }
                _MainViewModel.ActiveDocument.UpdateDisplayImage(nColorGradation, true);
            }
            else
            {
                _MainViewModel.ActiveDocument.UpdateDisplayImage();
            }*/
            Workspace.This.ActiveDocument.UpdateDisplayImage();

            //EL: TODO: add QC version
            //if (Workspace.This.Owner.ImagingSysSettings.IsQcVersion)
            //{
            //    var srcFilePath = Workspace.This.ActiveDocument.FilePath;
            //    if (string.IsNullOrEmpty(srcFilePath))
            //    {
            //        srcFilePath = Path.GetFileName(Workspace.This.ActiveDocument.Title);
            //    }
            //    ActivityLog newLog = new ActivityLog(Workspace.This.Owner.LoginUser.UserName);
            //    newLog.LogEditImage(srcFilePath, "Rotate Right");
            //    Workspace.This.Owner.ManageUsersVM.LogActivity(newLog);
            //}
        }
        protected bool CanExecuteRotateRight90Command(object parameter)
        {
            return (Workspace.This.ActiveDocument != null);
        }
        #endregion

        #region RotateLeft90Command
        private RelayCommand _RotateLeft90Command = null;
        /// <summary>
        /// Rotate left 90 degree
        /// </summary>
        public ICommand RotateLeft90Command
        {
            get
            {
                if (_RotateLeft90Command == null)
                {
                    _RotateLeft90Command = new RelayCommand(ExecuteRotateLeft90Command, CanExecuteRotateLeft90Command);
                }

                return _RotateLeft90Command;
            }
        }

        /// <summary>
        /// Rotate left 90 degree
        /// </summary>
        /// <param name="parameter"></param>
        protected void ExecuteRotateLeft90Command(object parameter)
        {
            if (!Workspace.This.IsActiveDocument) { return; }

            double dAngle = -90.0;
            Workspace.This.ActiveDocument.Image = ImageProcessing.WpfRotate(Workspace.This.ActiveDocument.Image, dAngle);

            if (Workspace.This.ActiveDocument.DocDirtyType != DirtyType.NewCreate)
            {
                Workspace.This.ActiveDocument.DocDirtyType = DirtyType.Modified;
            }

            /*if (_MainViewModel.ActiveDocument.ImageInfo.NumOfChannels > 1)
            {
                int nColorGradation = _MainViewModel.ActiveDocument.ImageInfo.NumOfChannels;
                // Is an individual channel currently selected?
                if (_MainViewModel.ActiveDocument.ImageInfo.SelectedChannel != ImageChannelType.Mix)
                {
                    if (_MainViewModel.ActiveDocument.ImageInfo.NumOfChannels > 1)
                    {
                        nColorGradation = ((int)_MainViewModel.ActiveDocument.SelectedChannel) - 1;
                    }
                }
                _MainViewModel.ActiveDocument.UpdateDisplayImage(nColorGradation, true);
            }
            else
            {
                _MainViewModel.ActiveDocument.UpdateDisplayImage();
            }*/
            Workspace.This.ActiveDocument.UpdateDisplayImage();

            //EL: TODO: add QC version
            //if (Workspace.This.Owner.ImagingSysSettings.IsQcVersion)
            //{
            //    var srcFilePath = Workspace.This.ActiveDocument.FilePath;
            //    if (string.IsNullOrEmpty(srcFilePath))
            //    {
            //        srcFilePath = Path.GetFileName(Workspace.This.ActiveDocument.Title);
            //    }
            //    ActivityLog newLog = new ActivityLog(Workspace.This.Owner.LoginUser.UserName);
            //    newLog.LogEditImage(srcFilePath, "Rotate Left");
            //    Workspace.This.Owner.ManageUsersVM.LogActivity(newLog);
            //}
        }
        protected bool CanExecuteRotateLeft90Command(object parameter)
        {
            return (Workspace.This.ActiveDocument != null);
        }
        #endregion

        #region ShowRotateArbitraryWinCommand

        private RelayCommand _ShowRotateArbitraryWinCommand = null;
        public ICommand ShowRotateArbitraryWinCommand
        {
            get
            {
                if (_ShowRotateArbitraryWinCommand == null)
                {
                    _ShowRotateArbitraryWinCommand = new RelayCommand(ExecuteShowRotateArbitraryWinCommand, CanExecuteShowRotateArbitraryWinCommand);
                }

                return _ShowRotateArbitraryWinCommand;
            }
        }

        /// <summary>
        /// Show rotate arbitrary window (rotation preview)
        /// </summary>
        protected void ExecuteShowRotateArbitraryWinCommand(object parameter)
        {
            if (!Workspace.This.IsActiveDocument) { return; }

            try
            {
                ArbitraryRotationAngle = 0.0;
                var arbitraryRotateWin = new RotateArbitraryWindow();
                arbitraryRotateWin.Owner = Application.Current.MainWindow;
                arbitraryRotateWin.DataContext = this;
                DisplayImage = Workspace.This.ActiveDocument.DisplayImage;
                var dlgResult = arbitraryRotateWin.ShowDialog();
                if (dlgResult == true)
                {
                    // Apply the rotation
                    //

                    WriteableBitmap rotatedImage = null;
                    double factor = 0.0;

                    if (ArbitraryRotationAngle != 0.0)
                    {
                        try
                        {
                            Workspace.This.StartWaitAnimation("Rotating the image...please wait");
                            if (ArbitraryRotationAngle != 90 && ArbitraryRotationAngle != -90)
                            {
                                // Rotate the image
                                rotatedImage = ImageProcessing.Rotate(Workspace.This.ActiveDocument.Image, ArbitraryRotationAngle);
                            }
                            else
                            {
                                if (ArbitraryRotationAngle == 90)
                                {
                                    ExecuteRotateRight90Command(null);
                                    IsRotateArbitraryChecked = false;
                                    return;
                                }
                                else if (ArbitraryRotationAngle == -90)
                                {
                                    ExecuteRotateLeft90Command(null);
                                    IsRotateArbitraryChecked = false;
                                    return;
                                }
                            }
                        }
                        finally
                        {
                            Workspace.This.StopWaitAnimation();
                        }
                        if (rotatedImage != null)
                        {
                            if (rotatedImage.PixelWidth > Workspace.This.ActiveDocument.Image.PixelWidth)
                            {
                                factor = (double)rotatedImage.PixelWidth / (double)Workspace.This.ActiveDocument.Image.PixelWidth;
                            }
                            else if (rotatedImage.PixelHeight > Workspace.This.ActiveDocument.Image.PixelHeight)
                            {
                                factor = (double)rotatedImage.PixelHeight / (double)Workspace.This.ActiveDocument.Image.PixelHeight;
                            }
                        }
                    }

                    if (rotatedImage != null && ArbitraryRotationAngle != 0.0)
                    {
                        Workspace.This.ActiveDocument.Image = rotatedImage;
                        Workspace.This.ActiveDocument.DocDirtyType = DirtyType.Modified;
                        // Manually trigger image size changed to set new zoom rate/factor,
                        // because rotate arbitrary changes the size of the image.
                        if (factor != 0.0)
                        {
                            Workspace.This.ActiveDocument.TriggerImageSizeChanged(factor);
                        }
                        Workspace.This.ActiveDocument.UpdateDisplayImage();

                        //EL: TODO: add QC version
                        //if (Workspace.This.Owner.ImagingSysSettings.IsQcVersion)
                        //{
                        //    var srcFilePath = Workspace.This.ActiveDocument.FilePath;
                        //    if (string.IsNullOrEmpty(srcFilePath))
                        //    {
                        //        srcFilePath = Path.GetFileName(Workspace.This.ActiveDocument.Title);
                        //    }
                        //    ActivityLog newLog = new ActivityLog(Workspace.This.Owner.LoginUser.UserName);
                        //    newLog.LogEditImage(srcFilePath, string.Format("Rotate {0} deg", Math.Round(ArbitraryRotationAngle, 2)));
                        //    Workspace.This.Owner.ManageUsersVM.LogActivity(newLog);
                        //}
                    }
                }
            }
            finally
            {
                IsRotateArbitraryChecked = false;
            }
        }
        protected bool CanExecuteShowRotateArbitraryWinCommand(object parameter)
        {
            return true;
        }

        #endregion

        /*#region RotateArbitraryCommand

        private RelayCommand _RotateArbitraryCommand = null;
        public ICommand RotateArbitraryCommand
        {
            get
            {
                if (_RotateArbitraryCommand == null)
                {
                    _RotateArbitraryCommand = new RelayCommand(ExecuteRotateArbitraryCommand, CanExecuteRotateArbitraryCommand);
                }

                return _RotateArbitraryCommand;
            }
        }

        /// <summary>
        /// Rotate an image at arbitrary angle (rotation preview)
        /// </summary>
        /// <param name="parameter"></param>
        //protected void ExecuteRotateArbitraryCommand(object parameter)
        //{
        //    if (!_MainViewModel.IsActiveDocument || IsImageRotating) { return; }
        //
        //    if (_MainViewModel.ActiveDocument.Image.CanFreeze)
        //    {
        //        _MainViewModel.ActiveDocument.Image.Freeze();
        //    }
        //
        //    string oldTitle = Path.GetFileName(_MainViewModel.ActiveDocument.Title);
        //    var sc = System.Threading.SynchronizationContext.Current;
        //    new System.Threading.Thread(new System.Threading.ThreadStart(() =>
        //    {
        //        WriteableBitmap tempImage = null;
        //
        //        if (ArbitraryRotationAngle != 0.0)
        //        {
        //            tempImage = ImageProcessing.Rotate(_MainViewModel.ActiveDocument.Image, ArbitraryRotationAngle);
        //        }
        //        else
        //        {
        //            tempImage = _MainViewModel.ActiveDocument.Image;
        //        }
        //
        //        if (tempImage != null)
        //        {
        //            if (tempImage.CanFreeze)
        //            {
        //                tempImage.Freeze();
        //            }
        //        }
        //
        //        // Update display image
        //        sc.Post(p =>
        //        {
        //            if (tempImage != null)
        //            {
        //                //ImageInfo imageInfo = Workspace.This.ActiveDocument.ImageInfo;
        //                //
        //                //if (imageInfo == null)
        //                //{
        //                //    imageInfo = new ImageInfo();
        //                //}
        //                //WriteableBitmap newDisplayImage = ImageProcessingHelper.UpdateDisplayImage(temp, imageInfo);
        //                //
        //                //if (newDisplayImage != null)
        //                //{
        //                //    Workspace.This.ActiveDocument.DisplayImage = newDisplayImage;
        //                //}
        //
        //                _MainViewModel.ActiveDocument.UpdateDisplayImage(tempImage);
        //
        //            }
        //            IsImageRotating = false;
        //
        //        }, null);
        //
        //    })).Start();
        //
        //    IsImageRotating = true;
        //    IsImageRotated = true;
        //    // Force WPF to render UI changes immediately with this magic line of code...
        //    Application.Current.Dispatcher.Invoke(new Action(() => { }), System.Windows.Threading.DispatcherPriority.ContextIdle);
        //
        //    if (_MainViewModel.Owner.ImagingSysSettings.IsQcVersion)
        //    {
        //        ActivityLog newLog = new ActivityLog(_MainViewModel.Owner.LoginUser.UserName);
        //        newLog.LogEditImage(oldTitle, string.Format("Rotate {0} deg", ArbitraryRotationAngle));
        //        _MainViewModel.Owner.ManageUsersVM.LogActivity(newLog);
        //    }
        //}
        protected void ExecuteRotateArbitraryCommand(object parameter)
        {
            if (!Workspace.This.IsActiveDocument || IsImageRotating) { return; }

            var origWidth = Workspace.This.ActiveDocument.Image.PixelWidth;
            var origHeight = Workspace.This.ActiveDocument.Image.PixelHeight;
            var srcImageCloned = (WriteableBitmap)Workspace.This.ActiveDocument.Image.Clone();
            srcImageCloned.Freeze();
            WriteableBitmap rotatedImage = null;

            BackgroundWorker worker = new BackgroundWorker();
            // Open the document in a different thread
            worker.DoWork += (o, ea) =>
            {
                if (ArbitraryRotationAngle != 0.0)
                {
                    IsImageRotating = true;
                    rotatedImage = ImageProcessing.Rotate(srcImageCloned, ArbitraryRotationAngle);
                }
                else
                {
                    rotatedImage = srcImageCloned;
                }

                if (rotatedImage != null)
                {
                    //var dstWidth = rotatedImage.PixelWidth;
                    //var dstHeight = rotatedImage.PixelHeight;
                    //var offsetX = (double)dstWidth / 2.0 - (double)origWidth / 2.0;
                    //var offsetY = (double)dstHeight / 2.0 - (double)origHeight / 2.0;
                    //Rect cropROI = new Rect(offsetX, offsetY, origWidth, origHeight);
                    //if (ArbitraryRotationAngle != 0.0)
                    //{
                    //    rotatedImage = ImageProcessing.Crop(rotatedImage, cropROI);
                    //}
                    //
                    //if (ArbitraryRotationAngle != 0.0)
                    //{
                    //    if (rotatedImage.PixelWidth != origWidth || rotatedImage.PixelHeight != origHeight)
                    //    {
                    //        // Keeping the aspect ratio to avoid distortion.
                    //        double scaleFactor = 1;
                    //        int width = 0;
                    //        int height = 0;
                    //        if (origWidth > origHeight)
                    //        {
                    //            // Keep original width
                    //            scaleFactor =  (double)origWidth / (double)rotatedImage.PixelWidth;
                    //            width = origWidth;
                    //            height = (int)((double)rotatedImage.PixelHeight * scaleFactor);
                    //        }
                    //        else
                    //        {
                    //            // Keep original height
                    //            scaleFactor = (double)origHeight / (double)rotatedImage.PixelHeight;
                    //            height = origHeight;
                    //            width = (int)((double)rotatedImage.PixelWidth * scaleFactor);
                    //        }
                    //        rotatedImage = ImageProcessing.Resize(rotatedImage, new Size(width, height));
                    //    }
                    //}

                    if (rotatedImage.CanFreeze)
                    {
                        rotatedImage.Freeze();
                    }
                }
            };
            worker.RunWorkerCompleted += (o, ea) =>
            {
                Application.Current.Dispatcher.BeginInvoke((Action)delegate
                {
                    IsImageRotating = false;

                    if (rotatedImage != null)
                    {
                        IsImageRotated = true;
                        Workspace.This.ActiveDocument.UpdateDisplayImage(rotatedImage);
                    }

                    srcImageCloned = null;
                });

            };

            worker.RunWorkerAsync();

            //var threadRotate = new Thread(() =>
            //{
            //    if (ArbitraryRotationAngle != 0.0)
            //    {
            //        IsImageRotating = true;
            //        rotatedImage = ImageProcessing.Rotate(srcImageCloned, ArbitraryRotationAngle);
            //    }
            //
            //    if (rotatedImage != null)
            //    {
            //        var origWidth = _MainViewModel.ActiveDocument.Image.PixelWidth;
            //        var origHeight = _MainViewModel.ActiveDocument.Image.PixelHeight;
            //        var dstWidth = rotatedImage.PixelWidth;
            //        var dstHeight = rotatedImage.PixelHeight;
            //        var offsetX = (double)dstWidth / 2.0 - (double)origWidth / 2.0;
            //        var offsetY = (double)dstHeight / 2.0 - (double)origHeight / 2.0;
            //        Rect cropROI = new Rect(offsetX, offsetY, origWidth, origHeight);
            //        rotatedImage = ImageProcessing.Crop(rotatedImage, cropROI);
            //
            //        IsImageRotated = true;
            //
            //        if (rotatedImage.CanFreeze)
            //        {
            //            rotatedImage.Freeze();
            //        }
            //    }
            //    IsImageRotating = false;
            //});
            //threadRotate.Start();
            //threadRotate.Join();

            //IsImageRotated = true;
            //IsImageRotating = false;

            //_MainViewModel.ActiveDocument.UpdateDisplayImage(rotatedImage);
            //

            //Rotation is not applied until the user select 'Apply' (this command is use for previewing the rotation angle).
            //if (_MainViewModel.Owner.ImagingSysSettings.IsQcVersion)
            //{
            //    string oldTitle = Path.GetFileName(_MainViewModel.ActiveDocument.Title);
            //    ActivityLog newLog = new ActivityLog(_MainViewModel.Owner.LoginUser.UserName);
            //    newLog.LogEditImage(oldTitle, string.Format("Rotate {0} deg", ArbitraryRotationAngle));
            //    _MainViewModel.Owner.ManageUsersVM.LogActivity(newLog);
            //}
        }
        protected bool CanExecuteRotateArbitraryCommand(object parameter)
        {
            return true;
        }

        #endregion*/

        #region UpdateAngleBindingOnEnterCommand

        private RelayCommand _UpdateAngleBindingOnEnterCommand = null;
        public ICommand UpdateAngleBindingOnEnterCommand
        {
            get
            {
                if (_UpdateAngleBindingOnEnterCommand == null)
                {
                    _UpdateAngleBindingOnEnterCommand = new RelayCommand(ExecuteUpdateAngleBindingOnEnterCommand, CanExecuteUpdateAngleBindingOnEnterCommand);
                }

                return _UpdateAngleBindingOnEnterCommand;
            }
        }
        protected void ExecuteUpdateAngleBindingOnEnterCommand(object parameter)
        {
            if (!Workspace.This.IsActiveDocument) { return; }
            System.Windows.Controls.TextBox tBox = parameter as System.Windows.Controls.TextBox;
            if (tBox != null)
            {
                //DependencyProperty prop = TextBox.TextProperty;
                //BindingExpression binding = BindingOperations.GetBindingExpression(tBox, prop);
                //if (binding != null)
                //    binding.UpdateSource();

                double angle = 0.0;
                if (double.TryParse(tBox.Text, out angle))
                {
                    if (ArbitraryRotationAngle != angle)
                    {
                        ArbitraryRotationAngle = angle;
                    }
                }
                else
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show("Please entry a valid value.");
                }
            }
        }
        protected bool CanExecuteUpdateAngleBindingOnEnterCommand(object parameter)
        {
            return true;
        }

        #endregion

        /*#region RotateArbitApplyCommand

        private RelayCommand _RotateArbitApplyCommand = null;
        public ICommand RotateArbitApplyCommand
        {
            get
            {
                if (_RotateArbitApplyCommand == null)
                {
                    _RotateArbitApplyCommand = new RelayCommand(ExecuteRotateArbitApplyCommand, CanExecuteRotateArbitApplyCommand);
                }
                return _RotateArbitApplyCommand;
            }
        }

        protected void ExecuteRotateArbitApplyCommand(object parameter)
        {
            if (!Workspace.This.IsActiveDocument) { return; }

            //CloseAction();

            //_MainViewModel.IsArbitraryRotate = false;

            WriteableBitmap rotatedImage = null;
            double factor = 0.0;

            if (ArbitraryRotationAngle != 0.0)
            {
                rotatedImage = ImageProcessing.Rotate(Workspace.This.ActiveDocument.Image, ArbitraryRotationAngle);
                if (rotatedImage != null)
                {
                    var origWidth = Workspace.This.ActiveDocument.Image.PixelWidth;
                    var origHeight = Workspace.This.ActiveDocument.Image.PixelHeight;
                    //var dstWidth = rotatedImage.PixelWidth;
                    //var dstHeight = rotatedImage.PixelHeight;
                    //var offsetX = (double)dstWidth / 2.0 - (double)origWidth / 2.0;
                    //var offsetY = (double)dstHeight / 2.0 - (double)origHeight / 2.0;
                    //Rect cropROI = new Rect(offsetX, offsetY, origWidth, origHeight);
                    //rotatedImage = ImageProcessing.Crop(rotatedImage, cropROI);
                    //if (rotatedImage.PixelWidth != origWidth || rotatedImage.PixelHeight != origHeight)
                    //{
                    //    // Keeping the aspect ratio to avoid distortion.
                    //    double scaleFactor = 1;
                    //    int width = 0;
                    //    int height = 0;
                    //    if (origWidth > origHeight)
                    //    {
                    //        // Keep original width
                    //        scaleFactor = (double)origWidth / (double)rotatedImage.PixelWidth;
                    //        width = origWidth;
                    //        height = (int)((double)rotatedImage.PixelHeight * scaleFactor);
                    //    }
                    //    else
                    //    {
                    //        // Keep original height
                    //        scaleFactor = (double)origHeight / (double)rotatedImage.PixelHeight;
                    //        height = origHeight;
                    //        width = (int)((double)rotatedImage.PixelWidth * scaleFactor);
                    //    }
                    //    rotatedImage = ImageProcessing.Resize(rotatedImage, new Size(width, height));
                    //}

                    if (rotatedImage.PixelWidth > Workspace.This.ActiveDocument.Image.PixelWidth)
                    {
                        factor = (double)rotatedImage.PixelWidth / (double)Workspace.This.ActiveDocument.Image.PixelWidth;
                    }
                    else if (rotatedImage.PixelHeight > Workspace.This.ActiveDocument.Image.PixelHeight)
                    {
                        factor = (double)rotatedImage.PixelHeight / (double)Workspace.This.ActiveDocument.Image.PixelHeight;
                    }
                }
            }

            if (rotatedImage != null && ArbitraryRotationAngle != 0.0)
            {
                Workspace.This.ActiveDocument.Image = rotatedImage;
                Workspace.This.ActiveDocument.DocDirtyType = DirtyType.Modified;
                Workspace.This.ActiveDocument.UpdateDisplayImage();
                // Manually trigger image size changed to set new zoom rate/factor,
                // because rotate arbitrary changes the size of the image.
                if (factor != 0.0)
                    Workspace.This.ActiveDocument.TriggerImageSizeChanged(factor);
                IsRotateArbitraryChecked = false;

                //EL: TODO: add QC version
                //if (Workspace.This.Owner.ImagingSysSettings.IsQcVersion)
                //{
                //    var srcFilePath = Workspace.This.ActiveDocument.FilePath;
                //    if (string.IsNullOrEmpty(srcFilePath))
                //    {
                //        srcFilePath = Path.GetFileName(Workspace.This.ActiveDocument.Title);
                //    }
                //    ActivityLog newLog = new ActivityLog(Workspace.This.Owner.LoginUser.UserName);
                //    newLog.LogEditImage(srcFilePath, string.Format("Rotate {0} deg", Math.Round(ArbitraryRotationAngle, 2)));
                //    Workspace.This.Owner.ManageUsersVM.LogActivity(newLog);
                //}
            }
            //_MainViewModel.IsShowGrid = false;
        }

        protected bool CanExecuteRotateArbitApplyCommand(object parameter)
        {
            return true;
        }

        #endregion*/

        /*#region RotateArbitCancelCommand

        private RelayCommand _RotateArbitCancelCommand = null;
        public ICommand RotateArbitCancelCommand
        {
            get
            {
                if (_RotateArbitCancelCommand == null)
                {
                    _RotateArbitCancelCommand = new RelayCommand(ExecuteRotateArbitCancelCommand, CanExecuteRotateArbitCancelCommand);
                }

                return _RotateArbitCancelCommand;
            }
        }

        protected void ExecuteRotateArbitCancelCommand(object parameter)
        {
            if (!Workspace.This.IsActiveDocument) { return; }

            //CloseAction();

            IsRotateArbitraryChecked = false;
            //_MainViewModel.IsShowGrid = false;
            if (_IsImageRotated)
            {
                Workspace.This.ActiveDocument.UpdateDisplayImage();
                _IsImageRotated = false;
            }

            // Forces an immediate garbage collection.
            //GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            //GC.WaitForPendingFinalizers();
            //GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            // Force garbage collection.
            //GC.Collect();
            // Wait for all finalizers to complete before continuing.
            //GC.WaitForPendingFinalizers();
        }

        protected bool CanExecuteRotateArbitCancelCommand(object parameter)
        {
            return true;
        }

        #endregion*/


        #region FlipHorizontalCommand
        private RelayCommand _FlipHorizontalCommand = null;
        public ICommand FlipHorizontalCommand
        {
            get
            {
                if (_FlipHorizontalCommand == null)
                {
                    _FlipHorizontalCommand = new RelayCommand(ExecuteFlipHorizontalCommand, CanExecuteFlipHorizontalCommand);
                }

                return _FlipHorizontalCommand;
            }
        }
        protected void ExecuteFlipHorizontalCommand(object parameter)
        {
            if (!Workspace.This.IsActiveDocument) { return; }

            if (Workspace.This.ActiveDocument.Image.Format.BitsPerPixel == 32)
            {
                string caption = "Image type not supported...";
                string message = "This operation is current not supported for 32-bit image.";
                Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

            // Intel IPP flip
            //ActiveDocument.Image = ImageProcessing.Flip(ActiveDocument.Image, IppiAxis.ippAxsHorizontal);
            // WPF flip
            Workspace.This.ActiveDocument.Image = ImageProcessing.WpfFlip(Workspace.This.ActiveDocument.Image, ImageProcessing.FlipAxis.Horizontal);

            if (Workspace.This.ActiveDocument.DocDirtyType != DirtyType.NewCreate)
            {
                Workspace.This.ActiveDocument.DocDirtyType = DirtyType.Modified;
            }

            /*if (_MainViewModel.ActiveDocument.ImageInfo.NumOfChannels > 1)
            {
                int nColorGradation = _MainViewModel.ActiveDocument.ImageInfo.NumOfChannels;
                // Is an individual channel currently selected?
                if (_MainViewModel.ActiveDocument.ImageInfo.SelectedChannel != ImageChannelType.Mix)
                {
                    if (_MainViewModel.ActiveDocument.ImageInfo.NumOfChannels > 1)
                    {
                        nColorGradation = ((int)_MainViewModel.ActiveDocument.SelectedChannel) - 1;
                    }
                }
                _MainViewModel.ActiveDocument.UpdateDisplayImage(nColorGradation, true);
            }
            else
            {
                _MainViewModel.ActiveDocument.UpdateDisplayImage();
            }*/
            Workspace.This.ActiveDocument.UpdateDisplayImage();

            //EL: TODO: add QC version
            //if (Workspace.This.Owner.ImagingSysSettings.IsQcVersion)
            //{
            //    var srcFilePath = Workspace.This.ActiveDocument.FilePath;
            //    if (string.IsNullOrEmpty(srcFilePath))
            //    {
            //        srcFilePath = Path.GetFileName(Workspace.This.ActiveDocument.Title);
            //    }
            //    ActivityLog newLog = new ActivityLog(Workspace.This.Owner.LoginUser.UserName);
            //    newLog.LogEditImage(srcFilePath, "Flip Horizontal");
            //    Workspace.This.Owner.ManageUsersVM.LogActivity(newLog);
            //}
        }
        protected bool CanExecuteFlipHorizontalCommand(object parameter)
        {
            return (Workspace.This.ActiveDocument != null);
        }
        #endregion

        #region FlipVerticalCommand
        private RelayCommand _FlipVerticalCommand = null;
        public ICommand FlipVerticalCommand
        {
            get
            {
                if (_FlipVerticalCommand == null)
                {
                    _FlipVerticalCommand = new RelayCommand(ExecuteFlipVerticalCommand, CanExecuteFlipVerticalCommand);
                }

                return _FlipVerticalCommand;
            }
        }
        protected void ExecuteFlipVerticalCommand(object parameter)
        {
            if (!Workspace.This.IsActiveDocument) { return; }

            if (Workspace.This.ActiveDocument.Image.Format.BitsPerPixel == 32)
            {
                string caption = "Image type not supported...";
                string message = "This operation is current not supported for 32-bit image.";
                Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

            //ActiveDocument.Image = ImageProcessing.Flip(ActiveDocument.Image, IppiAxis.ippAxsVertical);
            Workspace.This.ActiveDocument.Image = ImageProcessing.WpfFlip(Workspace.This.ActiveDocument.Image, ImageProcessing.FlipAxis.Vertical);

            if (Workspace.This.ActiveDocument.DocDirtyType != DirtyType.NewCreate)
            {
                Workspace.This.ActiveDocument.DocDirtyType = DirtyType.Modified;
            }

            /*if (_MainViewModel.ActiveDocument.ImageInfo.NumOfChannels > 1)
            {
                int nColorGradation = _MainViewModel.ActiveDocument.ImageInfo.NumOfChannels;
                // Is an individual channel currently selected?
                if (_MainViewModel.ActiveDocument.ImageInfo.SelectedChannel != ImageChannelType.Mix)
                {
                    if (_MainViewModel.ActiveDocument.ImageInfo.NumOfChannels > 1)
                    {
                        nColorGradation = ((int)_MainViewModel.ActiveDocument.SelectedChannel) - 1;
                    }
                }
                _MainViewModel.ActiveDocument.UpdateDisplayImage(nColorGradation, true);
            }
            else
            {
                _MainViewModel.ActiveDocument.UpdateDisplayImage();
            }*/
            Workspace.This.ActiveDocument.UpdateDisplayImage();

            //EL: TODO: add QC version
            //if (Workspace.This.Owner.ImagingSysSettings.IsQcVersion)
            //{
            //    var filePath = Workspace.This.ActiveDocument.FilePath;
            //    if (string.IsNullOrEmpty(filePath))
            //    {
            //        filePath = Path.GetFileName(Workspace.This.ActiveDocument.Title);
            //    }
            //    ActivityLog newLog = new ActivityLog(Workspace.This.Owner.LoginUser.UserName);
            //    newLog.LogEditImage(filePath, "Flip Vertical");
            //    Workspace.This.Owner.ManageUsersVM.LogActivity(newLog);
            //}
        }
        protected bool CanExecuteFlipVerticalCommand(object parameter)
        {
            return (Workspace.This.ActiveDocument != null);
        }
        #endregion

        #region ShiftLeftCommand
        private RelayCommand _ShiftLeftCommand = null;
        /// <summary>
        /// Shift/move image pixels to the left
        /// </summary>
        public ICommand ShiftLeftCommand
        {
            get
            {
                if (_ShiftLeftCommand == null)
                {
                    _ShiftLeftCommand = new RelayCommand(this.ExecuteShiftLeftCommand, CanExecuteShiftLeftCommand);
                }

                return _ShiftLeftCommand;
            }
        }
        protected void ExecuteShiftLeftCommand(object parameter)
        {
            throw new NotImplementedException();
        }
        protected bool CanExecuteShiftLeftCommand(object parameter)
        {
            return (Workspace.This.ActiveDocument != null);
        }
        #endregion

        #region ShiftRightCommand
        private RelayCommand _ShiftRightCommand = null;
        /// <summary>
        /// Shift/move image pixels to the right
        /// </summary>
        public ICommand ShiftRightCommand
        {
            get
            {
                if (_ShiftRightCommand == null)
                {
                    _ShiftRightCommand = new RelayCommand(this.ExecuteShiftRightCommand, CanExecuteShiftRightCommand);
                }

                return _ShiftRightCommand;
            }
        }
        protected void ExecuteShiftRightCommand(object parameter)
        {
            throw new NotImplementedException();
        }
        protected bool CanExecuteShiftRightCommand(object parameter)
        {
            return (Workspace.This.ActiveDocument != null);
        }
        #endregion

    }
}
