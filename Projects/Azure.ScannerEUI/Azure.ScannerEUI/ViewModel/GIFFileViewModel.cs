using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.ScannerEUI.ViewModel
{
    
    class GIFFileViewModel : PaneViewModel
    {
        private DirtyType _DocDirtyType = DirtyType.None;
        public GIFFileViewModel()
        {
            Title = FileName;
            IsDirty = true;
        }
        public GIFFileViewModel(string filePath)
        {
            FilePath = filePath;
            Title = FileName;
            string extension = System.IO.Path.GetExtension(filePath).ToLower();
            try
            {
            //
            }
            catch (Exception)
            {
                //string strMessage = string.Format("Error loading: {0}\n{1}", filePath, ex.Message);
                //string strCaption = "File loading error....";
                //Xceed.Wpf.Toolkit.MessageBox.Show(strMessage, strCaption, MessageBoxButton.OK, MessageBoxImage.Stop);
                //string message = string.Format("File loading error: {0}", ex.Message);
                //throw new Exception(message);
                throw;
            }
        }

        #region IsDirty

        private bool _isDirty = false;
        public bool IsDirty
        {
            get { return _isDirty; }
            set
            {
                if (_isDirty != value)
                {
                    _isDirty = value;
                    RaisePropertyChanged("IsDirty");
                    RaisePropertyChanged("FileName");
                    if (!_isDirty)
                    {
                        _DocDirtyType = DirtyType.None;
                    }
                }
            }
        }

        #endregion
        #region public string FileName
        public string FileName
        {
            get
            {
                if (FilePath == null)
                {
                    //return "Untitled" + (IsDirty ? "*" : "");
                    return Title + (IsDirty ? "*" : "");
                }

                return System.IO.Path.GetFileName(FilePath) + (IsDirty ? "*" : "");
            }
        }
        #endregion

        #region FilePath
        private string _filePath = null;
        public string FilePath
        {
            get { return _filePath; }
            set
            {
                if (_filePath != value)
                {
                    _filePath = value;
                    RaisePropertyChanged("FilePath");
                    RaisePropertyChanged("FileName");
                    RaisePropertyChanged("Title");

                    //if (File.Exists(_filePath))
                    //{
                    //    //_textContent = File.ReadAllText(_filePath);
                    //    try
                    //    {
                    //        ImageContent = Load(_filePath);
                    //        IsDirty = false;
                    //    }
                    //    catch
                    //    {
                    //    }
                    //    ContentId = _filePath;
                    //}
                }
            }
        }
        #endregion
    }
}
