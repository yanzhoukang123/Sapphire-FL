using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;   //MessageBox
using System.Windows.Input; //ICommand
using Azure.WPF.Framework;
using Azure.Configuration.Settings;

namespace Azure.LaserScanner.ViewModel
{
    class FileLocationViewModel : ViewModelBase
    {
        #region Private data...

        private string _DestinationFolder = string.Empty;
        private string _FileName = string.Empty;
        private string _FilePath = string.Empty;
        private string _Notes = string.Empty;
        private bool _IsAutoSave = false;
        private string _FileSize = string.Empty;

        private RelayCommand _EditFilePathCommand = null;
        private RelayCommand _BrowseLocationCommand = null;
        private RelayCommand _FileLocationCommand = null;

        #endregion

        #region Constructors...

        public FileLocationViewModel()
        {
        }

        #endregion

        #region Public properties...

        /// <summary>
        /// File full path (directory + filename)
        /// </summary>
        public string FilePath
        {
            get
            {
                return System.IO.Path.Combine(DestinationFolder, FileName);
            }
        }

        public string DestinationFolder
        {
            get { return _DestinationFolder; }
            set
            {
                if (_DestinationFolder != value)
                {
                    _DestinationFolder = value;
                    RaisePropertyChanged("DestinationFolder");
                }
            }
        }

        public string FileName
        {
            get
            {
                return _FileName;
            }
            set
            {
                if (_FileName != value)
                {
                    _FileName = value;
                    RaisePropertyChanged("FileName");
                }
            }
        }

        public string Notes
        {
            get { return _Notes; }
            set
            {
                if (_Notes != value)
                {
                    _Notes = value;
                    RaisePropertyChanged("Notes");
                }
            }
        }

        public bool IsAutoSave
        {
            get { return _IsAutoSave; }
            set
            {
                _IsAutoSave = value;
                RaisePropertyChanged("IsAutoSave");
                if (_IsAutoSave)
                {
                    if (string.IsNullOrEmpty(_FileName))
                    {
                        FileName = Workspace.This.GenerateFileName(string.Empty, string.Empty);   // generate default file name
                    }
                    if (String.IsNullOrEmpty(DestinationFolder))
                    {
                        DestinationFolder = SettingsManager.ApplicationSettings.InitialDirectory;
                        if (!System.IO.Directory.Exists(DestinationFolder))
                        {
                            string commonPictureFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                            string appCommonPictureFolder = commonPictureFolder + "\\" + Workspace.This.ProductName;
                            DestinationFolder = appCommonPictureFolder;
                        }
                    }
                }
            }
        }

        public string FileSize
        {
            get
            {
                return _FileSize;
            }
            set
            {
                if (_FileSize != value)
                {
                    _FileSize = value;
                    RaisePropertyChanged("FileSize");
                }
            }
        }

        #endregion

        #region public ICommand EditFilePathCommand

        public ICommand EditFilePathCommand
        {
            get
            {
                if (_EditFilePathCommand == null)
                {
                    _EditFilePathCommand = new RelayCommand(ExecuteEditFilePathCommand, CanExecuteEditFilePathCommand);
                }

                return _EditFilePathCommand;
            }
        }
        public void ExecuteEditFilePathCommand(object parameter)
        {
            // Set initial file name
            if (string.IsNullOrEmpty(_FileName))
            {
                FileName = Workspace.This.GenerateFileName(string.Empty, ".tif");
            }

            string tempFolder = DestinationFolder;

            // Set initial directory name
            if (String.IsNullOrEmpty(DestinationFolder))
            {
                string destinationFolder = SettingsManager.ApplicationSettings.InitialDirectory;
                if (String.IsNullOrEmpty(destinationFolder))
                {
                    string commonPictureFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    string appCommonPictureFolder = commonPictureFolder + "\\" + Workspace.This.ProductName;
                    destinationFolder = appCommonPictureFolder;
                }
                DestinationFolder = destinationFolder;
            }

            SelectFilePath selectPathWin = new SelectFilePath(FileName, DestinationFolder);
            selectPathWin.FileName = FileName;              // Initial file name
            selectPathWin.DestinationFolder = DestinationFolder;   // Initial destination folder
            bool? dialogResult = selectPathWin.ShowDialog();
            if (dialogResult == true)
            {
                FileName = selectPathWin.FileName;
                DestinationFolder = selectPathWin.DestinationFolder;

                if (!System.IO.Directory.Exists(DestinationFolder))
                {
                    // Create destination folder (if it doesn't exist)
                    try
                    {
                        System.IO.Directory.CreateDirectory(DestinationFolder);
                    }
                    catch (Exception ex)
                    {
                        Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, "ERROR: Creating the specified directory...",
                            MessageBoxButton.OK, MessageBoxImage.Stop);
                        return;
                    }
                }

                // Remember initial directory
                SettingsManager.ApplicationSettings.InitialDirectory = System.IO.Path.GetDirectoryName(DestinationFolder);
            }
            else
            {
                DestinationFolder = tempFolder;    // Restore preview saved destination folder
                return; // Cancel saving operation
            }
        }
        public bool CanExecuteEditFilePathCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region public ICommand BrowseLocationCommand

        public ICommand BrowseLocationCommand
        {
            get
            {
                if (_BrowseLocationCommand == null)
                {
                    _BrowseLocationCommand = new RelayCommand(ExecuteBrowseLocationCommand, CanExecuteBrowseLocationCommand);
                }

                return _BrowseLocationCommand;
            }
        }
        public void ExecuteBrowseLocationCommand(object parameter)
        {
            //if (!System.IO.Directory.Exists(DestinationFolder))
            //{
            //    return;
            //}

            // combine the arguments together
            // it doesn't matter if there is a space after ','
            //string argument = "/select, \"" + FilePath + "\"";    // Open destination folder and select the filename.
            //System.Diagnostics.Process.Start("explorer.exe", argument);

            // Open 'destinationFolder' in File Explorer
            //System.Diagnostics.Process.Start(@DestinationFolder);  // Open the destination folder in File Explorer

            WPFFolderBrowser.WPFFolderBrowserDialog wpfFBD = new WPFFolderBrowser.WPFFolderBrowserDialog();
            wpfFBD.InitialDirectory = DestinationFolder;
            wpfFBD.ShowPlacesList = true;
            bool? bResult = wpfFBD.ShowDialog();

            if (bResult == true)
            {
                if (System.IO.Directory.Exists(wpfFBD.FileName))
                {
                    this.DestinationFolder = wpfFBD.FileName;
                }
                else
                {
                    var parent = Application.Current.MainWindow;
                    string caption = "Select a folder...";
                    string message = string.Format("The folder \"{0}\" does not exists.\nPlease select another folder.", DestinationFolder);
                    Xceed.Wpf.Toolkit.MessageBox.Show(parent, message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                }
            }
        }
        public bool CanExecuteBrowseLocationCommand(object parameter)
        {
            return true;
        }

        #endregion

        #region public ICommand FileLocationCommand

        public ICommand FileLocationCommand
        {
            get
            {
                if (_FileLocationCommand == null)
                {
                    _FileLocationCommand = new RelayCommand(ExecuteFileLocationCommand, CanExecuteFileLocationCommand);
                }

                return _FileLocationCommand;
            }
        }
        public void ExecuteFileLocationCommand(object parameter)
        {
            //if (!System.IO.Directory.Exists(DestinationFolder))
            //{
            //    return;
            //}

            // combine the arguments together
            // it doesn't matter if there is a space after ','
            //string argument = "/select, \"" + FilePath + "\"";    // Open destination folder and select the filename.
            //System.Diagnostics.Process.Start("explorer.exe", argument);

            // Open 'destinationFolder' in File Explorer
            if (System.IO.Directory.Exists(DestinationFolder))
            {
                System.Diagnostics.Process.Start(@DestinationFolder);  // Open the destination folder in File Explorer
            }
            else
            {
                var parent = Application.Current.MainWindow;
                string caption = "File location...";
                string message = string.Format("The folder doesn't exists: {0}", DestinationFolder);
                Xceed.Wpf.Toolkit.MessageBox.Show(parent, message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
            }
        }
        public bool CanExecuteFileLocationCommand(object parameter)
        {
            return true;
        }

        #endregion


    }
}
