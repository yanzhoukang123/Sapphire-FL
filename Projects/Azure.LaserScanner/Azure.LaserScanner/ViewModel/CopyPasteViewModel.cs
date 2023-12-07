using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Azure.WPF.Framework;
using Azure.Image.Processing;
//using Azure.UserManagement;

namespace Azure.LaserScanner.ViewModel
{
    public class CopyPasteViewModel : ViewModelBase
    {
        private ImageClipboard _ImageClipboard;

        public CopyPasteViewModel()
        {
            this._ImageClipboard = new ImageClipboard();
        }

        public bool IsImageClipboard
        {
            get
            {
                bool bResult = false;
                if (_ImageClipboard != null)
                {
                    if (_ImageClipboard.ClipImage != null)
                        bResult = true;
                }
                return bResult;
            }
        }

        public ImageClipboard ImageClipboard
        {
            get { return _ImageClipboard; }
            set
            {
                if (_ImageClipboard != value)
                {
                    _ImageClipboard = value;
                }
            }
        }


        #region CropCommand...
        private RelayCommand _CropCommand = null;
        public ICommand CropCommand
        {
            get
            {
                if (_CropCommand == null)
                {
                    _CropCommand = new RelayCommand(this.ExecuteCropCommand, CanExecuteCropCommand);
                }

                return _CropCommand;
            }
        }
        protected void ExecuteCropCommand(object parameter)
        {
            if (Workspace.This.ActiveDocument != null)
            {
                if (Workspace.This.ActiveDocument.IsSelectionToolChecked)
                {
                    int imgWidth = Workspace.This.ActiveDocument.Image.PixelWidth;
                    int imgHeight = Workspace.This.ActiveDocument.Image.PixelHeight;
                    Rect cropRect = Workspace.This.ActiveDocument.SelectedRegion;
                    cropRect.X = (int)cropRect.X;
                    cropRect.Y = (int)cropRect.Y;
                    cropRect.Width = (int)cropRect.Width;
                    cropRect.Height = (int)cropRect.Height;
                    if (cropRect.Width + cropRect.X > imgWidth) { cropRect.Width = imgWidth - cropRect.X; }
                    if (cropRect.Height + cropRect.Y > imgHeight) { cropRect.Height = imgHeight - cropRect.Y; }

                    Workspace.This.ActiveDocument.IsSelectionToolChecked = false;
                    ImageInfo newImageInfo = null;

                    if (cropRect.Width > 0 && cropRect.Height > 0)
                    {
                        if (Workspace.This.ActiveDocument.ImageInfo != null)
                        {
                            newImageInfo = (ImageInfo)Workspace.This.ActiveDocument.ImageInfo.Clone();
                            //newImageInfo = new ImageInfo(ActiveDocument.ImageInfo);
                        }

                        // Crop the selected region of the active document
                        WriteableBitmap cropImage = ImageProcessing.Crop(Workspace.This.ActiveDocument.Image, cropRect);

                        if (cropImage != null)
                        {
                            string newTitle = string.Empty;
                            if (Workspace.This.CheckSupportedFileType(Workspace.This.ActiveDocument.Title))
                                newTitle = Path.GetFileNameWithoutExtension(Workspace.This.ActiveDocument.Title) + "_crop";
                            else
                                newTitle = Workspace.This.ActiveDocument.Title + "_crop";

                            // check if document name already exists/opened
                            int docCounter = 0;
                            int result = 0;
                            foreach (var doc in Workspace.This.Files)
                            {
                                string title = doc.Title;
                                if (title.Contains(newTitle))
                                {
                                    char lastChar = title[title.Length - 1];
                                    if (!Char.IsDigit(lastChar))
                                    {
                                        docCounter = 1;
                                    }
                                    else
                                    {
                                        result = (int)Char.GetNumericValue(lastChar);
                                        if (result > docCounter)
                                        {
                                            docCounter = result;
                                        }
                                    }
                                }
                            }
                            if (docCounter > 0)
                            {
                                newTitle = newTitle + "_" + (docCounter + 1);
                            }

                            //if (_MainViewModel.Owner.ImagingSysSettings.IsQcVersion)
                            //    newImageInfo.DateTime = System.String.Format("{0:G} UTC {0:zz}", DateTime.Now);
                            //else
                            //    newImageInfo.DateTime = System.String.Format("{0:G}", DateTime.Now);
                            newImageInfo.DateTime = System.String.Format("{0:G} UTC{0:zz}", DateTime.Now);

                            //ActiveDocument.IsRgbImageCropped = true;
                            Workspace.This.NewDocument(cropImage, newImageInfo, newTitle, true, false);

                            //EL: TODO: add QC version
                            //if (Workspace.This.Owner.ImagingSysSettings.IsQcVersion)
                            //{
                            //    var srcFilePath = Workspace.This.ActiveDocument.FilePath;
                            //    if (string.IsNullOrEmpty(srcFilePath))
                            //    {
                            //        srcFilePath = Path.GetFileName(Workspace.This.ActiveDocument.Title);
                            //    }
                            //    ActivityLog newLog = new ActivityLog(Workspace.This.Owner.LoginUser.UserName);
                            //    newLog.LogEditImage(srcFilePath, "Crop");
                            //    Workspace.This.Owner.ManageUsersVM.LogActivity(newLog);
                            //}
                        }
                    }

                }
            }
        }
        protected bool CanExecuteCropCommand(object parameter)
        {
            return (Workspace.This.ActiveDocument != null && Workspace.This.ActiveDocument.IsSelectionToolChecked);
        }
        #endregion

        #region CopyCommand
        private RelayCommand _CopyCommand = null;
        public ICommand CopyCommand
        {
            get
            {
                if (_CopyCommand == null)
                {
                    _CopyCommand = new RelayCommand(this.ExecuteCopyCommand, CanExecuteCopyCommand);
                }

                return _CopyCommand;
            }
        }
        #region protected void ExecuteCopyCommand(object parameter)
        /// <summary>
        ///Region of interest copy command implementation.
        /// </summary>
        /// <param name="parameter"></param>
        protected void ExecuteCopyCommand(object parameter)
        {
            if (Workspace.This.ActiveDocument != null)
            {
                if (Workspace.This.ActiveDocument.IsSelectionToolChecked)
                {
                    // current only support 16-bit and 24-bit image
                    if (Workspace.This.ActiveDocument.Image.Format.BitsPerPixel != 16 &&
                        Workspace.This.ActiveDocument.Image.Format.BitsPerPixel != 24 &&
                        Workspace.This.ActiveDocument.Image.Format.BitsPerPixel != 48)
                    {
                        Workspace.This.ActiveDocument.IsSelectionToolChecked = false;
                        return;
                    }

                    Rect clipRect = Workspace.This.ActiveDocument.SelectedRegion;

                    if (clipRect.Width == 0 || clipRect.Height == 0)
                    {
                        string caption = "Region selection error...";
                        string message = "Please reselect your region of interest.";
                        Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                        return;
                    }

                    // Special case: when 16-bit image is saved as .JPG or .BMP; the image is not reloaded,
                    // the loaded data is still 16-bit, make copy & paste feature use the display image.
                    bool bIsActiveJpgOrBmp = false;
                    string fileExtension = Path.GetExtension(Workspace.This.ActiveDocument.FilePath);
                    if (!string.IsNullOrEmpty(fileExtension))
                    {
                        if (fileExtension.Contains(".jpg") || fileExtension.Contains(".bmp"))
                        {
                            bIsActiveJpgOrBmp = true;
                        }
                    }

                    if (Workspace.This.ActiveDocument.Image.Format.BitsPerPixel == 24 || bIsActiveJpgOrBmp)
                    {
                        // Crop the selected region of the display image
                        _ImageClipboard.ClipImage = ImageProcessing.Crop(Workspace.This.ActiveDocument.DisplayImage, clipRect);
                    }
                    else
                    {
                        try
                        {
                            Workspace.This.StartWaitAnimation("Copying...");
                            // Crop the selected region of the active document
                            _ImageClipboard.ClipImage = ImageProcessing.Crop(Workspace.This.ActiveDocument.Image, clipRect);
                            // Find the blobs (of the blot)
                            if (_ImageClipboard.ClipImage != null)
                            {
                                if (_ImageClipboard.ClipImage.Format.BitsPerPixel == 48)
                                {
                                    //extract rgb channels
                                    WriteableBitmap[] extractedImages = { null, null, null };
                                    extractedImages = ImageProcessing.GetChannel(Workspace.This.ActiveDocument.Image);
                                    if (extractedImages != null)
                                    {
                                        //Get the blobs of the green channel
                                        _ImageClipboard.BlobsImage = ImageProcessing.FindBlobs(extractedImages[1], new Size(20, 20), _ImageClipboard.ClipImage, clipRect);
                                        if (_ImageClipboard.BlobsImage == null)
                                        {
                                            Workspace.This.StopWaitAnimation();
                                            string caption = "Find blobs error...";
                                            string message = "Error finding the blobs on the selected region.\n" +
                                                             "Please select another region on the marker image.";
                                            Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                                        }
                                    }
                                }
                                else
                                {
                                    _ImageClipboard.BlobsImage = ImageProcessing.FindBlobs(Workspace.This.ActiveDocument.Image, new Size(20, 20), _ImageClipboard.ClipImage, clipRect);
                                    if (_ImageClipboard.BlobsImage == null)
                                    {
                                        Workspace.This.StopWaitAnimation();
                                        string caption = "Find blobs error...";
                                        string message = "Error finding the blobs on the selected region.\n" +
                                                         "Please select another region on the marker image.";
                                        Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(ex.Message);
                            throw;
                        }
                        finally
                        {
                            Workspace.This.StopWaitAnimation();
                        }
                    }

                    _ImageClipboard.Title = Workspace.This.ActiveDocument.Title;
                    clipRect.Width = _ImageClipboard.ClipImage.PixelWidth;
                    clipRect.Height = _ImageClipboard.ClipImage.PixelHeight;
                    _ImageClipboard.ClipRect = clipRect;
                    _ImageClipboard.OrigSize = new Size(Workspace.This.ActiveDocument.Image.PixelWidth, Workspace.This.ActiveDocument.Image.PixelHeight);
                    //_IsImageClipboard = true;
                    Workspace.This.ActiveDocument.IsSelectionToolChecked = false;

                    // Forces an immediate garbage collection.
                    //GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                    //GC.WaitForPendingFinalizers();
                    //GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                    // Force garbage collection.
                    //GC.Collect();
                    // Wait for all finalizers to complete before continuing.
                    //GC.WaitForPendingFinalizers();
                }
                else
                {
                    // Make duplicate image (copied image).
                    WriteableBitmap clonedImage = (WriteableBitmap)Workspace.This.ActiveDocument.Image.Clone();
                    ImageInfo clonedImageInfo = (ImageInfo)Workspace.This.ActiveDocument.ImageInfo.Clone();
                    int counter = 2;

                    string docTitle = string.Empty;
                    if (Workspace.This.CheckSupportedFileType(Workspace.This.ActiveDocument.Title))
                        docTitle = Path.GetFileNameWithoutExtension(Workspace.This.ActiveDocument.Title);
                    else
                        docTitle = Workspace.This.ActiveDocument.Title;

                    string newDocTitle = docTitle + " ( " + counter + " )";
                    // check if document title already
                    for (int i = 0; i < Workspace.This.Files.Count; i++)
                    {
                        string tmpTitle = string.Empty;
                        if (Workspace.This.CheckSupportedFileType(Workspace.This.Files[i].Title))
                            tmpTitle = Path.GetFileNameWithoutExtension(Workspace.This.Files[i].Title);
                        else
                            tmpTitle = Workspace.This.Files[i].Title;

                        if (tmpTitle == newDocTitle)
                        {
                            counter++;
                            newDocTitle = docTitle + " ( " + counter + " )";
                            i = 0;
                            continue;
                        }
                    }
                    Workspace.This.NewDocument(clonedImage, clonedImageInfo, newDocTitle, false, false);
                }
            }
        }
        #endregion
        protected bool CanExecuteCopyCommand(object parameter)
        {
            return (Workspace.This.ActiveDocument != null && Workspace.This.ActiveDocument.IsSelectionToolChecked);
        }
        #endregion

        #region PasteCommand
        private RelayCommand _PasteCommand = null;
        public ICommand PasteCommand
        {
            get
            {
                if (_PasteCommand == null)
                {
                    _PasteCommand = new RelayCommand(this.ExecutePasteCommand, CanExecutePasteCommand);
                }

                return _PasteCommand;
            }
        }
        protected void ExecutePasteCommand(object parameter)
        {
            if (Workspace.This.ActiveDocument != null)
            {
                if (IsImageClipboard)
                {
                    // current only support 16-bit and 24-bit image image
                    if (Workspace.This.ActiveDocument.Image.Format.BitsPerPixel != 16 &&
                        Workspace.This.ActiveDocument.Image.Format.BitsPerPixel != 24)
                    {
                        return;
                    }

                    // Special case: when 16-bit image is saved as .JPG or .BMP; the image is not reloaded,
                    // the loaded data is still 16-bit, make copy & paste feature use the display image.
                    bool bIsActiveJpgOrBmp = false;
                    string fileExtension = Path.GetExtension(Workspace.This.ActiveDocument.FilePath);
                    if (!string.IsNullOrEmpty(fileExtension))
                    {
                        if (fileExtension.Contains(".jpg") || fileExtension.Contains(".bmp"))
                        {
                            bIsActiveJpgOrBmp = true;
                        }
                    }

                    int clippboardBpp = _ImageClipboard.ClipImage.Format.BitsPerPixel;
                    int activeDocBpp = Workspace.This.ActiveDocument.Image.Format.BitsPerPixel;
                    bool bIs24bitCopyAndPaste = false;

                    if (clippboardBpp == activeDocBpp)
                    {
                        if (clippboardBpp == 24 && activeDocBpp == 24)
                        {
                            bIs24bitCopyAndPaste = true;
                        }
                        else
                        {
                            if (bIsActiveJpgOrBmp && activeDocBpp == 16)
                            {
                                string caption = "Bit depth mismatch...";
                                string message = "The chemi image and the clipboard image are of different bit depth.";
                                Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                                return;
                            }
                        }
                    }
                    else
                    {
                        if (bIsActiveJpgOrBmp && clippboardBpp == 24)
                        {
                            bIs24bitCopyAndPaste = true;
                        }
                        else if (clippboardBpp != 48)
                        {
                            string caption = "Bit depth mismatch...";
                            string message = "The chemi image and the clipboard image are of different bit depth.";
                            Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                            return;
                        }
                    }

                    // check image size
                    if (_ImageClipboard.OrigSize.Width != Workspace.This.ActiveDocument.Image.PixelWidth &&
                        _ImageClipboard.OrigSize.Height != Workspace.This.ActiveDocument.Image.PixelHeight)
                    {
                        string caption = "Size mismatch";
                        string message = "The marker image and the current image are of different size.";
                        Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                        return;
                    }

                    try
                    {
                        Workspace.This.StartWaitAnimation("Pasting...");

                        WriteableBitmap pastedImage = null;
                        ImageInfo imageInfo = null;
                        if (Workspace.This.ActiveDocument.ImageInfo != null)
                        {
                            imageInfo = (ImageInfo)Workspace.This.ActiveDocument.ImageInfo.Clone();
                        }
                        else
                        {
                            imageInfo = new ImageInfo();
                        }

                        if (bIs24bitCopyAndPaste)
                        {
                            try
                            {
                                pastedImage = ImageProcessingHelper.Paste24bppImage(Workspace.This.ActiveDocument.DisplayImage, _ImageClipboard.ClipImage, _ImageClipboard.ClipRect);
                                imageInfo.MixChannel.IsInvertChecked = false;
                                imageInfo.SelectedChannel = ImageChannelType.Mix;
                                imageInfo.MixChannel.BlackValue = 0;
                                imageInfo.MixChannel.WhiteValue = 255;
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }
                        }
                        else
                        {
                            try
                            {
                                if (clippboardBpp == 48)
                                {
                                    pastedImage = ImageProcessingHelper.PasteRgbMarkerImage(Workspace.This.ActiveDocument.Image,
                                                                                            _ImageClipboard.BlobsImage,
                                                                                            _ImageClipboard.ClipImage,
                                                                                            _ImageClipboard.ClipRect);
                                    if (pastedImage != null)
                                    {
                                        imageInfo.CaptureType = "Chemi + Marker";
                                        if (imageInfo.MixChannel.IsInvertChecked == true)
                                        {
                                            imageInfo.RedChannel.IsInvertChecked = true;
                                            imageInfo.GreenChannel.IsInvertChecked = true;
                                            imageInfo.BlueChannel.IsInvertChecked = true;
                                            imageInfo.GrayChannel.IsInvertChecked = true;
                                            imageInfo.SelectedChannel = ImageChannelType.Mix;
                                        }
                                    }
                                }
                                else
                                {
                                    pastedImage = ImageProcessingHelper.PasteMarkerImage(Workspace.This.ActiveDocument.Image,
                                                                                         _ImageClipboard.BlobsImage,
                                                                                         _ImageClipboard.ClipImage,
                                                                                         _ImageClipboard.ClipRect);
                                }
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }
                        }

                        if (pastedImage != null)
                        {
                            //if (pastedImage.CanFreeze) { pastedImage.Freeze(); }
                            var fileNameWithoutExt = string.Empty;
                            if (Workspace.This.CheckSupportedFileType(Workspace.This.ActiveDocument.Title))
                                fileNameWithoutExt = Path.GetFileNameWithoutExtension(Workspace.This.ActiveDocument.Title);
                            else
                                fileNameWithoutExt = Workspace.This.ActiveDocument.Title;

                            string newTitle = fileNameWithoutExt + "_+_Marker";

                            // check if document name already exists/opened
                            int docCounter = 0;
                            int result = 0;
                            foreach (var doc in Workspace.This.Files)
                            {
                                string title = doc.Title;
                                if (title.Contains(newTitle))
                                {
                                    char lastChar = title[title.Length - 1];
                                    if (!Char.IsDigit(lastChar))
                                    {
                                        docCounter = 1;
                                    }
                                    else
                                    {
                                        result = (int)Char.GetNumericValue(lastChar);
                                        if (result > docCounter)
                                        {
                                            docCounter = result;
                                        }
                                    }
                                }
                            }
                            if (docCounter > 0)
                            {
                                newTitle = newTitle + "_" + (docCounter + 1);
                            }

                            Workspace.This.NewDocument(pastedImage, imageInfo, newTitle, false);
                        }
                    }
                    catch (Exception ex)
                    {
                        Workspace.This.StopWaitAnimation();

                        string caption = "Chemi-marker paste error...";
                        string message = string.Format("Chemi-marker paste error: {0}", ex.Message);
                        Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                    }
                    finally
                    {
                        Workspace.This.StopWaitAnimation();
                    }
                }
            }
        }
        protected bool CanExecutePasteCommand(object parameter)
        {
            return (Workspace.This.ActiveDocument != null && IsImageClipboard);
        }
        #endregion

        #region ClearClipboardCommand...
        private RelayCommand _ClearClipboardCommand = null;
        public ICommand ClearClipboardCommand
        {
            get
            {
                if (_ClearClipboardCommand == null)
                {
                    _ClearClipboardCommand = new RelayCommand(this.ExecuteClearClipboardCommand, CanExecuteClearClipboardCommand);
                }

                return _ClearClipboardCommand;
            }
        }
        protected void ExecuteClearClipboardCommand(object parameter)
        {
            if (Workspace.This.ActiveDocument != null)
            {
                if (IsImageClipboard)
                {
                    _ImageClipboard.ClipImage = null;
                    _ImageClipboard.Title = string.Empty;
                    _ImageClipboard.ClipRect = new Rect();
                    _ImageClipboard.OrigSize = new Size();
                    RaisePropertyChanged("IsImageClipboard");
                }
            }
        }
        protected bool CanExecuteClearClipboardCommand(object parameter)
        {
            return (Workspace.This.ActiveDocument != null && IsImageClipboard);
        }
        #endregion

    }
}
