using Azure.Configuration.Settings;
using Azure.Image.Processing;
using Azure.WPF.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Azure.ScannerEUI.ViewModel
{
    class ImageRotatingPrcessViewModel : ViewModelBase
    {

        #region Public data
        public List<string> HistoryGifPath = new List<string>();
        //public string rotapath = System.Environment.CurrentDirectory+@"\CV2TESTIMAGE.png";
        //public string Gifpath = System.Environment.CurrentDirectory+ @"\TESTIMAGEGif.gif";
        //public string ImageGifPath = System.Environment.CurrentDirectory + @"\ImageGif";
        //public string LImageGifPath = System.Environment.CurrentDirectory + @"\ImageGif\L";
        //public string R1ImageGifPath = System.Environment.CurrentDirectory + @"\ImageGif\R1";
        //public string R2ImageGifPath = System.Environment.CurrentDirectory + @"\ImageGif\R2";
        public string rotapath;
        public string Gifpath;
        public string ImageGifPath;
        public string LImageGifPath;
        public string R1ImageGifPath;
        public string R2ImageGifPath;
        #endregion

        #region private
        private string _SelectedChannel = null;   //Channel
        private WriteableBitmap ImageSource = null;
        private AbstractPaneViewModel activePane = null;
        private GIFFileViewModel _GIFActiveDocument = null; 
        private RelayCommand _OkCommand = null;
        private RelayCommand _SaveCommand = null;
        private float _angle=0;
        private string _SourceGifPath;
        #endregion

        public ImageRotatingPrcessViewModel()
        {
            OptionsChannels = new List<string>();
            OptionsChannels.AddRange(new string[] { "Original", "Detail" });
            SelectedChannel = OptionsChannels[0];
            this.Files = new ObservableCollection<GIFFileViewModel>();
            this.Panes = new ObservableCollection<AbstractPaneViewModel>();

            // Create 'ImageGif' folder and write/save test images in 'ProgramData' (can't write to 'Program Files' (if run from the installation folder))
            string commonApplicationData = SettingsManager.ApplicationDataPath;
            rotapath = Path.Combine(commonApplicationData, "CV2TESTIMAGE.png");
            Gifpath = Path.Combine(commonApplicationData, "TESTIMAGEGif.gif");
            ImageGifPath = Path.Combine(commonApplicationData, "ImageGif");
            LImageGifPath = Path.Combine(commonApplicationData, "ImageGif", "L");
            R1ImageGifPath = Path.Combine(commonApplicationData, "ImageGif", "R1");
            R2ImageGifPath = Path.Combine(commonApplicationData, "ImageGif", "R2");
        }

        public void Initialize()
        {
            //将存GIF图像的文件夹清空
            // Empty the folder where GIF images are stored
            string imgtype = "*.gif";
            string[] dirs = Directory.GetFiles(ImageGifPath, imgtype);
            if (dirs.Length > 0)
            {
                foreach (string dir in dirs)
                {
                    if (HistoryGifPath.Count > 0)
                    {
                        if (!HistoryGifPath.Contains(dir))
                        {
                            Open(dir);
                        }
                    }
                    else
                    {
                        Open(dir);
                    }

                }
            }
            if (GIFFilel != null)
            {
                SourceGifPath = GIFFilel.FilePath;
            }
        }

        #region  attribute
        public float Angle
        {
            get { return _angle; }
            set
            {
                if (_angle != value)
                {
                    _angle = value;
                }
            }
        }
        public string SourceGifPath
        {
            get { return _SourceGifPath; }
            set
            {
                if (_SourceGifPath != value)
                {
                    _SourceGifPath = value;
                }
                RaisePropertyChanged("SourceGifPath");
            }
        }
        #endregion

        #region Preview 

        public List<string> OptionsChannels { get; }
        public string SelectedChannel
        {
            get { return _SelectedChannel; }
            set
            {
                if (_SelectedChannel != value)
                {
                    _SelectedChannel = value;
                    RaisePropertyChanged(nameof(SelectedChannel));
                }
            }
        }

        #endregion

        #region OkCommand

        public ICommand OkCommand
        {
            get
            {
                if (_OkCommand == null)
                {
                    _OkCommand = new RelayCommand(ExecuteOkCommand, CanExecuteOkCommand);
                }

                return _OkCommand;
            }
        }
        public void ExecuteOkCommand(object parameter)
        {
            WriteableBitmap wb = Workspace.This.ActiveDocument.RotatingImage;
            if (SelectedChannel == "Detail")
            {
                RotatProcess.Rotat.rotate(ref wb, 0 - Angle);
            }
            else
            {
                RotatProcess.Rotat.rotateScrap(ref wb, 0 - Angle);
            }
            Workspace.This.ActiveDocument.Image = wb.Clone();
            Workspace.This.ActiveDocument.IsDirty = true;
            Workspace.This.ActiveDocument.Rotating_UpdateDisplayImage();
            Workspace.This.ActiveDocument.Angle = (int)Angle;
            Workspace.This.ActiveDocument.ImageTypePriview = SelectedChannel;
        }
        public bool CanExecuteOkCommand(object parameter)
        {
            return true;
        }

        #endregion


        #region SaveCommand

        public ICommand SaveCommand
        {
            get
            {
                if (_SaveCommand == null)
                {
                    _SaveCommand = new RelayCommand(ExecuteSaveCommand, CanExecuteSaveCommand);
                }

                return _SaveCommand;
            }
        }
        public void ExecuteSaveCommand(object parameter)
        {
            if (GIFFilel != null)
            {
                if (!HistoryGifPath.Contains(GIFFilel.FilePath))
                {
                    SaveSync(GIFFilel);
                }

            }
        }
        public bool CanExecuteSaveCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region ActiveDocument

        //public event EventHandler ActiveDocumentChanged;

        public GIFFileViewModel GIFFilel
        {
            get { return _GIFActiveDocument; }
            set
            {
                if (_GIFActiveDocument != value)
                {
                    _GIFActiveDocument = value;
                    if (value != null)
                    {
                        SourceGifPath = value.FilePath;
                    }
                    RaisePropertyChanged("GIFFilel");
                }
            }
        }

        #endregion

        /// <summary>
        /// View-models for panes.
        /// </summary>
        public ObservableCollection<AbstractPaneViewModel> Panes
        {
            get;
            private set;
        }
        /// <summary>
        /// View-models for documents.
        /// </summary>
        public ObservableCollection<GIFFileViewModel> Files
        {
            get;
            private set;
        }

        /// <summary>
        /// View-model for the active pane.
        /// </summary>
        public AbstractPaneViewModel ActivePane
        {
            get
            {
                return activePane;
            }
            set
            {
                if (activePane == value)
                {
                    return;
                }

                activePane = value;

                RaisePropertyChanged("ActivePane");
            }
        }

        public GIFFileViewModel Open(string filePath)
        {
            var fileViewModel = Files.FirstOrDefault(fm => fm.FilePath == filePath);
            if (fileViewModel != null)
            {
                return fileViewModel;
            }

            if (!File.Exists(filePath))
            {
                return null;
            }

            fileViewModel = new GIFFileViewModel(filePath);
            if (Files != null)
            {
                Files.Add(fileViewModel);
                GIFFilel = Files[0];
                fileViewModel.IsDirty = true;
            }

            return fileViewModel;
        }

        internal bool Close(GIFFileViewModel fileToClose)
        {
            if (fileToClose.IsDirty)
            {
                //var productName = _Owner.ProductName;
                var productName = string.Empty;
                var res = System.Windows.MessageBox.Show(string.Format("Do you want to save changes to '{0}'?", fileToClose.Title), productName, System.Windows.MessageBoxButton.YesNoCancel, System.Windows.MessageBoxImage.Warning);
                if (res == System.Windows.MessageBoxResult.Cancel)
                    return false;
                if (res == System.Windows.MessageBoxResult.Yes)
                {
                    try
                    {
                        //SaveAsync(fileToClose, closeAfterSaved: true);
                        SaveSync(fileToClose);
                        Remove(fileToClose);
                    }
                    catch
                    {
                        // Rethrow to preserve stack details
                        // Satisfies the rule. 
                        throw;
                    }
                }
                else
                {
                    Remove(fileToClose);
                }
            }
            else
            {
                Remove(fileToClose);
            }
            return true;
        }
        /// <summary>
        /// Remove file and release memory.
        /// </summary>
        /// <param name="fileToRemove"></param>
        internal void Remove(GIFFileViewModel fileToRemove)
        {
            int nextItem = Files.IndexOf(fileToRemove) - 1;
            bool bIsRemoveActiveDoc = (fileToRemove == GIFFilel) ? true : false;

            Files.Remove(fileToRemove);
            HistoryGifPath.Add(fileToRemove.FilePath);
            SourceGifPath = null;
            // Forces a garbage collection
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);

            // Retrieves the number of bytes currently thought to be allocated (true: force full collection)
            GC.GetTotalMemory(true);

            if (Files.Count == 0)
            {
                GIFFilel = null;
            }
            else
            {
                if (bIsRemoveActiveDoc)
                {
                    if (nextItem < 0)
                    {
                        nextItem = 0;
                    }
                    GIFFilel = Files[nextItem];
                }
            }

            RaisePropertyChanged("GIFFilel");
        }

        internal void SaveSync(GIFFileViewModel fileToSave)
        {
            if (fileToSave != null)
            {
                var dlg = new SaveFileDialog();
                dlg.Filter = "TIF Files(.GIF)|*.gif";
                dlg.Title = "Save an Image File";
                if (fileToSave.FilePath != null)
                {
                    dlg.FileName = fileToSave.Title;
                   // dlg.FileName = GenerateFileName(fileToSave.Title, ".gif");
                }
                else
                {
                    dlg.FileName = fileToSave.Title;
                }
                DialogResult dr = dlg.ShowDialog();
                if (dr == DialogResult.OK)
                {
                    try
                    {
                        File.Copy(fileToSave.FilePath, dlg.FileName, true);
                        HistoryGifPath.Add(fileToSave.FilePath);
                        HistoryGifPath.Add(dlg.FileName);
                        fileToSave.FilePath = dlg.FileName;
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show("GIF Create fail!", "error");

                    }
               
                }
            }
            try
            {
            
                fileToSave.IsDirty = false;
                fileToSave.Title = fileToSave.FileName;
                
            }
            catch
            {
                throw;
            }
            finally
            {

            }
        }
        /// <summary>
        /// Generate default file name using timestamp (default file type is: .TIFF)
        /// </summary>
        /// <param name="headerTitle"></param>
        /// <param name="fileType"></param>
        /// <returns></returns>
        internal string GenerateFileName(string headerTitle, string fileType = ".gif")
        {
            string strFileName = string.Empty;
            string strFileFullPath = string.Empty;
            //int[] intArray = null;
            //bool bIsFramePartOfSet = false;

            //
            // Get set and frame number
            //
            headerTitle = Path.GetFileNameWithoutExtension(headerTitle);
            DateTime dt = DateTime.Now;
            string pattern = @"S\d";
            if (Regex.IsMatch(headerTitle, pattern))
            {
                //bIsFramePartOfSet = true;
                strFileName = headerTitle + "_" + string.Format("{0}-{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
            }
            else
            {
                strFileName = headerTitle + "_" + string.Format("{0}-{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
            }
            switch (fileType.ToLower())
            {
                case ".gif":
                    strFileName = strFileName + ".gif";
                    break;
            }

            return strFileName;
        }

        public void DirectoryUpdate()
        {
            if (Directory.Exists(ImageGifPath) == false)
            {
                Directory.CreateDirectory(ImageGifPath);
            }
            else
            {
                Directory.Delete(ImageGifPath, true);
                Directory.CreateDirectory(ImageGifPath);
            }
            while (true)
            {
                Thread.Sleep(100);
                if (Directory.Exists(ImageGifPath) == true)
                {
                    if (Directory.Exists(LImageGifPath) == false)
                    {
                        Directory.CreateDirectory(LImageGifPath);
                    }
                    if (Directory.Exists(R1ImageGifPath) == false)
                    {
                        Directory.CreateDirectory(R1ImageGifPath);
                    }
                    if (Directory.Exists(R2ImageGifPath) == false)
                    {
                        Directory.CreateDirectory(R2ImageGifPath);
                    }
                    return;
                }
                else
                {
                    Directory.CreateDirectory(ImageGifPath);
                }
            }
        }
    }
}
