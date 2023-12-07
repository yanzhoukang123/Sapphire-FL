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

namespace Azure.LaserScanner.ViewModel
{
    public class AnimatedGifViewModel : ViewModelBase
    {
        #region Private members...

        private ObservableCollection<FileViewModel> _SourceFiles;
        private ObservableCollection<FileViewModel> _AnimatedFiles;

        private BitmapImage _AnimatedImage = null;
        //private bool _IsAnimationCompleted = false;
        
        private GifQuality _SelectedGifQuality = GifQuality.Default;

        #endregion

        #region Constructors...

        public AnimatedGifViewModel()
        {
            _AnimatedFiles = new ObservableCollection<FileViewModel>();
            AnimationSpeed = 500;
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
            get;
            set;
        }

        public ObservableCollection<FileViewModel> AnimatedFiles
        {
            get { return _AnimatedFiles; }
            set
            {
                _AnimatedFiles = value;
                RaisePropertyChanged("AnimatedFiles");
            }
        }

        public FileViewModel SelectedAnimatedFile
        {
            get;
            set;
        }

        public BitmapImage AnimatedImage
        {
            get { return _AnimatedImage; }
            set
            {
                _AnimatedImage = value;
                RaisePropertyChanged("AnimatedImage");
            }
        }

        /*public bool IsAnimationCompleted
        {
            get { return _IsAnimationCompleted; }
            set
            {
                _IsAnimationCompleted = value;
                RaisePropertyChanged("IsAnimationCompleted");
            }
        }*/

        /// <summary>
        /// Animation speed (or delay time) in milliseconds
        /// </summary>
        public int AnimationSpeed
        {
            get;
            set;
        }

        /// <summary>
        /// Looping/repeat counter (0 = Infinite loop)
        /// </summary>
        public int RepeatCount
        {
            get;
            set;
        }

        public GifQuality SelectedGifQuality
        {
            get { return _SelectedGifQuality; }
            set
            {
                _SelectedGifQuality = value;
                RaisePropertyChanged("SelectedGifQuality");
            }
        }

        public IEnumerable<GifQuality> GifQualityOptions
        {
            get
            {
                return Enum.GetValues(typeof(GifQuality))
                    .Cast<GifQuality>();
            }
        }

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
                AnimatedFiles.Add(SelectedSourceFile);
                SourceFiles.Remove(SelectedSourceFile);
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
            if (AnimatedFiles != null && AnimatedFiles.Count > 0 && SelectedAnimatedFile != null)
            {
                SourceFiles.Add(SelectedAnimatedFile);
                AnimatedFiles.Remove(SelectedAnimatedFile);
            }
        }
        protected bool CanExecuteRemoveFileCommand(object parameter)
        {
            //return true;
            return (SelectedAnimatedFile != null);
        }
        #endregion

        #region LoadAnimatedGifCommand

        private RelayCommand _LoadAnimatedGifCommand = null;
        public ICommand LoadAnimatedGifCommand
        {
            get
            {
                if (_LoadAnimatedGifCommand == null)
                {
                    _LoadAnimatedGifCommand = new RelayCommand(ExecuteLoadAnimatedGifCommand, CanExecuteLoadAnimatedGifCommand);
                }

                return _LoadAnimatedGifCommand;
            }
        }
        protected void ExecuteLoadAnimatedGifCommand(object parameter)
        {
            try
            {
                var dlg = new OpenFileDialog();
                dlg.Filter = "GIF Files (*.gif)|*.gif";
                dlg.Title = "Open File";
                dlg.CheckFileExists = true;
                dlg.CheckPathExists = true;
                dlg.Multiselect = false;
                if (dlg.ShowDialog().GetValueOrDefault())
                {
                    var animated = new BitmapImage();
                    animated.BeginInit();
                    animated.UriSource = new Uri(dlg.FileName, UriKind.RelativeOrAbsolute);
                    animated.CacheOption = BitmapCacheOption.None;
                    animated.CreateOptions = BitmapCreateOptions.None | BitmapCreateOptions.PreservePixelFormat | BitmapCreateOptions.IgnoreImageCache;
                    animated.EndInit();
                    animated.Freeze();
                    AnimatedImage = animated.Clone();
                }
                else
                {
                    return;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        protected bool CanExecuteLoadAnimatedGifCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region CreateAnimatedGifCommand

        private RelayCommand _CreateAnimatedGifCommand = null;
        public ICommand CreateAnimatedGifCommand
        {
            get
            {
                if (_CreateAnimatedGifCommand == null)
                {
                    _CreateAnimatedGifCommand = new RelayCommand(ExecuteCreateAnimatedGifCommand, CanExecuteCreateAnimatedGifCommand);
                }

                return _CreateAnimatedGifCommand;
            }
        }
        protected void ExecuteCreateAnimatedGifCommand(object parameter)
        {
            if (AnimatedFiles != null && AnimatedFiles.Count > 1)
            {
                try
                {
                    var dlg = new SaveFileDialog();
                    dlg.Filter = "GIF (*.gif)|*.gif";
                    dlg.Title = "Save image";
                    dlg.FileName = Workspace.This.GenerateFileName(string.Empty, ".gif");
                    string filePath = string.Empty;

                    if (dlg.ShowDialog().GetValueOrDefault())
                    {
                        filePath = dlg.FileName;
                        if (Path.GetExtension(filePath).ToLower() != ".gif")
                        {
                            filePath = filePath + ".gif";
                        }
                    }
                    else
                    {
                        return;
                    }

                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }

                    using (var gif = AnimatedGif.AnimatedGif.Create(filePath, AnimationSpeed, RepeatCount))
                    {
                        GifQuality gifQuality = SelectedGifQuality;
                        // When quality is set to Default causes an error to occur in GDI+.
                        // Default is equivalent to Bit8 (set to Bit8 to avoid GDI+ error.
                        if (SelectedGifQuality == GifQuality.Default)
                            gifQuality = GifQuality.Bit8;
                        foreach (var file in _AnimatedFiles)
                        {
                            gif.AddFrame(file.DisplayImage.ToBitmap(), delay: AnimationSpeed, quality: gifQuality);
                        }
                    }

                    var animated = new BitmapImage();
                    animated.BeginInit();
                    animated.UriSource = new Uri(@filePath, UriKind.RelativeOrAbsolute);
                    animated.CacheOption = BitmapCacheOption.None;
                    animated.CreateOptions = BitmapCreateOptions.None | BitmapCreateOptions.PreservePixelFormat | BitmapCreateOptions.IgnoreImageCache;
                    animated.EndInit();
                    animated.Freeze();
                    AnimatedImage = animated.Clone();
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        protected bool CanExecuteCreateAnimatedGifCommand(object parameter)
        {
            return (_AnimatedFiles.Count > 1);
        }

        #endregion
    }
}
