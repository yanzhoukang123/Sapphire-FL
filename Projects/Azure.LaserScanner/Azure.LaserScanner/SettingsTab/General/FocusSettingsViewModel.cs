using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input; //ICommand
using System.Xml;
using System.Collections.ObjectModel;   //ObservableCollection
using Azure.Common;
using Azure.WPF.Framework;
using Azure.ImagingSystem;
using Azure.Configuration.Settings;
using Azure.LaserScanner.ViewModel;

namespace Azure.LaserScanner
{
    //public enum FocusEditMode
    //{
    //    Add,
    //    Edit,
    //}

    public class FocusSettingsViewModel : ViewModelBase
    {
        private ObservableCollection<SampleTypeSetting> _SampleTypeFocusOptions;
        private SampleTypeSetting _SelectedSampleType;
        private bool _IsAddFocusMode = true;
        private bool _IsEditFocusMode = false;

        //private int _Position = 1;
        private string _SampleType = string.Empty;
        private double? _FocusPosition;
        
        private ICommand _ApplyCommand;
        private ICommand _DeleteSampleTypeCommand;

        private const string _SysSettingsConfigFile = "SysSettings.xml";

        public FocusSettingsViewModel()
        {
            _SampleTypeFocusOptions = new ObservableCollection<SampleTypeSetting>(SettingsManager.ConfigSettings.SampleTypeSettings);
            if (_SampleTypeFocusOptions != null)
            {
                for (int i = 0; i < _SampleTypeFocusOptions.Count; i++)
                {
                    if (_SampleTypeFocusOptions[i].DisplayName.ToLower() == "custom")
                    {
                        _SampleTypeFocusOptions.Remove(_SampleTypeFocusOptions[i]);
                    }
                }
            }
        }

        public ObservableCollection<SampleTypeSetting> SampleTypeFocusOptions
        {
            get { return _SampleTypeFocusOptions; }
            set
            {
                _SampleTypeFocusOptions = value;
                RaisePropertyChanged("SampleTypeFocusOptions");

            }
        }

        public SampleTypeSetting SelectedSampleType
        {
            get { return _SelectedSampleType; }
            set
            {
                if (_SelectedSampleType != value)
                {
                    _SelectedSampleType = value;
                    RaisePropertyChanged("SelectedSampleType");
                    RaisePropertyChanged("IsEnableEdit");

                    if (_SelectedSampleType != null && _IsEditFocusMode)
                    {
                        //this.Position = _SelectedSampleType.Position;
                        this.SampleType = _SelectedSampleType.DisplayName;
                        this.FocusPosition = _SelectedSampleType.FocusPosition;
                    }
                }
            }
        }

        public bool IsAddFocusMode
        {
            get { return _IsAddFocusMode; }
            set
            {
                _IsAddFocusMode = value;
                RaisePropertyChanged("IsAddFocusMode");
            }
        }

        public bool IsEditFocusMode
        {
            get { return _IsEditFocusMode; }
            set
            {
                _IsEditFocusMode = value;
                RaisePropertyChanged("IsEditFocusMode");
                if (_IsEditFocusMode)
                {
                    if (_SelectedSampleType != null)
                    {
                        //this.Position = _SelectedSampleType.Position;
                        this.SampleType = _SelectedSampleType.DisplayName;
                        this.FocusPosition = _SelectedSampleType.FocusPosition;
                    }
                }
            }
        }

        public bool IsEnableEdit
        {
            get
            {
                if (_SelectedSampleType != null)
                    return true;
                else
                    return false;
            }
        }

        //public int Position
        //{
        //    get { return _Position; }
        //    set
        //    {
        //        _Position = value;
        //        RaisePropertyChanged("Position");
        //    }
        //}

        public string SampleType
        {
            get { return _SampleType; }
            set
            {
                _SampleType = value;
                RaisePropertyChanged("SampleType");
            }
        }

        public double? FocusPosition
        {
            get { return _FocusPosition; }
            set
            {
                _FocusPosition = value;
                RaisePropertyChanged("FocusPosition");
            }
        }

        public ICommand ApplyCommand
        {
            get
            {
                if (_ApplyCommand == null)
                {
                    _ApplyCommand = new RelayCommand((p) => ExecuteApplyCommand(p), (p) => CanExecuteApplyCommand(p));
                }

                return _ApplyCommand;
            }
        }

        private void ExecuteApplyCommand(object parameter)
        {
            if (IsAddFocusMode)
            {
                if (!string.IsNullOrEmpty(SampleType.Trim()) && FocusPosition >= 0)
                {
                    if (IsSampleTypeExists(_SampleType))
                    {
                        string caption = "Add focus type...";
                        string message = "The focus type already exists";
                        Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    SampleTypeSetting sampleTypeSet = new SampleTypeSetting();

                    int position = GetNextAvailablePosition(_SampleTypeFocusOptions);
                    sampleTypeSet.Position = position;
                    sampleTypeSet.DisplayName = _SampleType;
                    sampleTypeSet.FocusPosition = (double)_FocusPosition;

                    // Add new sample type to sample type options (current listview)
                    this.SampleTypeFocusOptions.Insert(sampleTypeSet.Position - 1, sampleTypeSet); // (-1) : zero index

                    // Add new sample type to the original sample type configuration settings
                    SettingsManager.ConfigSettings.SampleTypeSettings.Insert(sampleTypeSet.Position - 1, sampleTypeSet);
                    // Add to Imaging/Fluorescence sample type list
                    //Workspace.This.FluorescenceVM.SampleTypeOptions.Insert(sampleTypeSet.Position - 1, sampleTypeSet);

                    foreach (var proto in Workspace.This.FluorescenceVM.AppProtocolOptions)
                    {
                        foreach (var scanRegion in proto.ScanRegions)
                        {
                            scanRegion.SampleTypeOptions.Insert(sampleTypeSet.Position - 1, sampleTypeSet);
                        }
                    }

                    RaisePropertyChanged("SampleTypeFocusOptions");

                    //Clear new entry
                    this.SampleType = string.Empty;
                    this.FocusPosition = null;

                    //Save sample type to SysSettings.xml
                    AddSampleType(position, sampleTypeSet);

                    Xceed.Wpf.Toolkit.MessageBox.Show("New sample type and focus position added.");
                }
            }
            else if (IsEditFocusMode)
            {
                if (_SelectedSampleType != null)
                {
                    if (!string.IsNullOrEmpty(SampleType.Trim()) && FocusPosition >= 0)
                    {
                        foreach (var sampleType in _SampleTypeFocusOptions)
                        {
                            if (sampleType.DisplayName.Equals(_SelectedSampleType.DisplayName, StringComparison.OrdinalIgnoreCase) &&
                                sampleType.FocusPosition == _SelectedSampleType.FocusPosition)
                            {
                                if (!string.IsNullOrEmpty(SampleType.Trim()) && FocusPosition >= 0)
                                {
                                    if (!sampleType.DisplayName.Equals(this._SampleType, StringComparison.OrdinalIgnoreCase) ||
                                        sampleType.FocusPosition != this._FocusPosition)
                                    {
                                        // Update 'SysSettings.xml file
                                        // Must be called before the selected sample type changes value.
                                        UpdateSampleType(_SelectedSampleType, new SampleTypeSetting(_SelectedSampleType.Position, _SampleType, (double)_FocusPosition));

                                        sampleType.DisplayName = this.SampleType;
                                        sampleType.FocusPosition = (double)this._FocusPosition;
                                        // Doing it this way because updating an element in the list does not update the UI
                                        // Behave differently than add and remove to list
                                        SampleTypeFocusOptions = new ObservableCollection<SampleTypeSetting>(_SampleTypeFocusOptions);
                                        //Workspace.This.FluorescenceVM.SampleTypeOptions = new ObservableCollection<SampleTypeSetting>(_SampleTypeFocusOptions);

                                        foreach (var proto in Workspace.This.FluorescenceVM.AppProtocolOptions)
                                        {
                                            foreach (var scanRegion in proto.ScanRegions)
                                            {
                                                scanRegion.SampleTypeOptions = new ObservableCollection<SampleTypeSetting>(_SampleTypeFocusOptions);
                                            }
                                        }
                                        // Add 'Custom' option to sample types list
                                        SampleTypeSetting customSampleType = new SampleTypeSetting();
                                        customSampleType.DisplayName = "Custom";
                                        customSampleType.FocusPosition = 0;
                                        //Workspace.This.FluorescenceVM.SampleTypeOptions.Add(customSampleType);

                                        foreach (var proto in Workspace.This.FluorescenceVM.AppProtocolOptions)
                                        {
                                            foreach (var scanRegion in proto.ScanRegions)
                                            {
                                                scanRegion.SampleTypeOptions.Add(customSampleType);
                                            }
                                        }

                                        Xceed.Wpf.Toolkit.MessageBox.Show("Selected sample type updated.");
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
            }
        }

        private bool CanExecuteApplyCommand(object parameter)
        {
            return true;
        }


        public ICommand DeleteSampleTypeCommand
        {
            get
            {
                if (_DeleteSampleTypeCommand == null)
                {
                    _DeleteSampleTypeCommand = new RelayCommand((p) => ExecuteDeleteSampleTypeCommand(p), (p) => CanExecuteDeleteSampleTypeCommand(p));
                }

                return _DeleteSampleTypeCommand;
            }
        }

        private void ExecuteDeleteSampleTypeCommand(object parameter)
        {
            if (_SelectedSampleType != null)
            {
                string protocolName = string.Empty;
                string caption = string.Empty;
                string message = string.Empty;
                if (IsSampleTypeReferenced(_SelectedSampleType, ref protocolName))
                {
                    caption = "Delete focus type...";
                    message = string.Format("The selected focus type is referenced by the protocol \"{0}\"\n" +  
                                            "Are you sure you want to delete the focus type \"{1}\"?",
                                            protocolName, _SelectedSampleType.DisplayName);
                }
                else
                {
                    caption = "Delete focus type...";
                    message = string.Format("Are you sure you want to delete the focus type \"{0}\"?", _SelectedSampleType.DisplayName);
                }
                MessageBoxResult dlgResult = Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (dlgResult == System.Windows.MessageBoxResult.Yes)
                {
                    // Remove selected sample type from the SysSettings.xml file.
                    DeleteSampleType(_SelectedSampleType);

                    // Remove selected sample type from the default sample type list.
                    SettingsManager.ConfigSettings.SampleTypeSettings.Remove(_SelectedSampleType);

                    // Remove from Fluorescence imaging tab.
                    //Workspace.This.FluorescenceVM.SampleTypeOptions.Remove(_SelectedSampleType);

                    foreach (var proto in Workspace.This.FluorescenceVM.AppProtocolOptions)
                    {
                        foreach (var scanRegion in proto.ScanRegions)
                        {
                            scanRegion.SampleTypeOptions.Remove(_SelectedSampleType);
                        }
                    }

                    // Remove the selected sample type from General settings tab
                    _SampleTypeFocusOptions.Remove(_SelectedSampleType);
                    RaisePropertyChanged("SampleTypeFocusOptions");

                    // Select none.
                    SelectedSampleType = null;
                }
            }
        }

        private bool CanExecuteDeleteSampleTypeCommand(object parameter)
        {
            return true;
        }


        #region === Helper Methods ===

        /// <summary>
        /// Add sample type to the configuration file (SysSettings.xml).
        /// </summary>
        /// <param name="samepleTypeTba"></param>
        private void AddSampleType(int position, SampleTypeSetting samepleTypeTba)
        {
            string filePath = System.IO.Path.Combine(Workspace.This.AppDataPath, _SysSettingsConfigFile);

            XmlDocument XDoc = new XmlDocument();
            //XDoc.PreserveWhitespace = true;   //this flag override auto indent settings
            XmlNode parentNode = null;
            XmlElement childElement = null;

            if (System.IO.File.Exists(filePath))
            {
                XDoc.Load(filePath);
                parentNode = XDoc.SelectSingleNode("SysSettings/SampleTypes");
                childElement = XDoc.CreateElement("SampleType");
                parentNode.InsertBefore(childElement, parentNode.ChildNodes[position - 1]); // position is 1 index, list is zero index

                childElement.SetAttribute("Position", samepleTypeTba.Position.ToString());
                childElement.SetAttribute("DisplayName", samepleTypeTba.DisplayName);
                childElement.SetAttribute("FocusPosition", samepleTypeTba.FocusPosition.ToString());

                //XmlWriterSettings settings = new XmlWriterSettings();
                //settings.Encoding = Encoding.UTF8;
                //settings.Indent = true;
                //settings.IndentChars = "  ";  //indent 2 spaces

                //using (XmlWriter writer = XmlTextWriter.Create(filePath, settings))
                //{
                //    XDoc.Save(writer);
                //}

                //XDoc.ToIndentedString();
                XDoc.Beautify();
                XDoc.Save(filePath);
            }
        }

        /// <summary>
        /// Remove sample type from configuration file (SysSettings.xml).
        /// </summary>
        /// <param name="sampleTypeTbd"></param>
        private bool DeleteSampleType(SampleTypeSetting sampleTypeTbd)
        {
            if (sampleTypeTbd == null) return false;

            bool bResult = false;
            string configFilePath = System.IO.Path.Combine(Workspace.This.AppDataPath, _SysSettingsConfigFile);
            bool bIsSampleTypeRemoved = false;

            // find and remove from 'SysSettings.xml'
            if (System.IO.File.Exists(configFilePath))
            {
                XmlDocument doc = new XmlDocument();
                //doc.PreserveWhitespace = true;
                doc.Load(configFilePath);

                XmlNodeList elemList = doc.GetElementsByTagName("SampleType");
                for (int i = 0; i < elemList.Count; i++)
                {
                    string attrDispName = elemList[i].Attributes["DisplayName"].Value;
                    string attrFocusPos = elemList[i].Attributes["FocusPosition"].Value;
                    if (attrDispName == sampleTypeTbd.DisplayName &&
                        attrFocusPos == sampleTypeTbd.FocusPosition.ToString())
                    {
                        elemList[i].ParentNode.RemoveChild(elemList[i]);
                        bIsSampleTypeRemoved = true;
                        break;
                    }
                }

                if (bIsSampleTypeRemoved)
                {
                    doc.Beautify();
                    doc.Save(configFilePath);
                    bResult = true;
                }
            }
            return bResult;
        }

        /// <summary>
        /// Update/modify an existing sample type on the configuration file (SysSettings.xml).
        /// </summary>
        /// <param name="selectSampleType"></param>
        /// <returns></returns>
        private bool UpdateSampleType(SampleTypeSetting selectSampleType, SampleTypeSetting updatingSampleType)
        {
            bool bResult = false;
            bool bSaveDoc = false;
            string configFilePath = System.IO.Path.Combine(Workspace.This.AppDataPath, _SysSettingsConfigFile);

            XmlDocument xdoc = new XmlDocument();
            xdoc.PreserveWhitespace = true;
            xdoc.Load(configFilePath);

            XmlNodeList sampleTypeElements = xdoc.GetElementsByTagName("SampleType");

            if (sampleTypeElements != null)
            {
                for (int i = 0; i < sampleTypeElements.Count; i++)
                {
                    if (sampleTypeElements[i].NodeType == XmlNodeType.Element)
                    {
                        string attrDispName = sampleTypeElements[i].Attributes["DisplayName"].Value;
                        string attrFocusPos = sampleTypeElements[i].Attributes["FocusPosition"].Value;
                        double dFocusPos = double.Parse(attrFocusPos);
                        if (attrDispName.Equals(selectSampleType.DisplayName, StringComparison.OrdinalIgnoreCase) &&
                            dFocusPos == selectSampleType.FocusPosition)
                        {
                            try
                            {
                                sampleTypeElements[i].Attributes["DisplayName"].Value = updatingSampleType.DisplayName;
                                sampleTypeElements[i].Attributes["FocusPosition"].Value = updatingSampleType.FocusPosition.ToString();
                                bSaveDoc = true;
                                break;
                            }
                            catch
                            {
                                bResult = false;
                                bSaveDoc = false;
                            }
                        }
                    }
                }
            }

            if (bSaveDoc)
            {
                try
                {
                    //xdoc.ToIndentedString();
                    xdoc.Save(configFilePath);
                    bResult = true;
                }
                catch (Exception e)
                {
                    bResult = false;
                    string message = string.Format("Error updating the config file: {0}\nERROR: {1}", _SysSettingsConfigFile, e.Message);
                    Xceed.Wpf.Toolkit.MessageBox.Show(message);
                }

            }

            return bResult;
        }

        private bool IsSampleTypeExists(string sampleType)
        {
            bool bResult = false;
            // check if the elements already exists.
            bResult = _SampleTypeFocusOptions.ToList().Exists(element => element.DisplayName.Equals(sampleType, StringComparison.OrdinalIgnoreCase));
            return bResult;
        }

        private bool IsSampleTypeReferenced(SampleTypeSetting sampleType, ref string protocolName)
        {
            bool bResult = false;

            if (sampleType != null)
            {
                if (Workspace.This.FluorescenceVM.AppProtocolOptions != null &&
                    Workspace.This.FluorescenceVM.AppProtocolOptions.Count > 0)
                {
                    foreach (var appMethod in Workspace.This.FluorescenceVM.AppProtocolOptions)
                    {
                        if (appMethod.SelectedScanRegion.SelectedSampleType.Position == sampleType.Position && 
                            appMethod.SelectedScanRegion.SelectedSampleType.DisplayName.Equals(sampleType.DisplayName, StringComparison.OrdinalIgnoreCase))
                        {
                            protocolName = appMethod.ProtocolName;
                            bResult = true;
                        }
                    }
                }
            }

            return bResult;
        }

        /// <summary>
        /// Get the index of the item called "Custom" in the list
        /// </summary>
        /// <param name="sampleTypeList"></param>
        /// <returns></returns>
        private int GetItemCustomIndex(List<SampleTypeSetting> sampleTypeList)
        {
            int nResult = -1;
            foreach (var item in sampleTypeList)
            {
                if (item.DisplayName.Equals("Custom", StringComparison.OrdinalIgnoreCase) &&
                    item.FocusPosition == 0)
                {
                    nResult = sampleTypeList.IndexOf(item);
                    break;
                }
            }
            return nResult;
        }

        /*private int GetNextAvailablePosition(List<SampleTypeSetting> theList)
        {
            if (theList == null || theList.Count < 1) { return 1; }

            int nResult = theList[theList.Count - 1].Position + 1;

            if (theList != null)
            {
                for (int index = 1; index < theList.Count + 1; index++)
                {
                    if (index < theList.Count + 1)
                    {
                        if (theList[index - 1].Position == index)
                        {
                            continue;
                        }
                        else
                        {
                            nResult = index;
                            break;
                        }
                    }
                }
            }

            return nResult;
        }*/

        private int GetPositionMax(ObservableCollection<SampleTypeSetting> theList)
        {
            int nMaxPos = theList[0].Position;
            for (int i = 0; i < theList.Count; i++)
            {
                if (theList[i].Position > nMaxPos)
                {
                    nMaxPos = theList[i].Position;
                }
            }

            return nMaxPos;
        }

        private int GetNextAvailablePosition(ObservableCollection<SampleTypeSetting> theList)
        {
            if (theList == null || theList.Count < 1) { return 1; }

            // this is just in case the position are not in ascending order.
            int nResult = GetPositionMax(theList) + 1;

            if (theList != null)
            {
                bool bIsPositionExists = false;
                for (int index = 1; index < theList.Count + 1; index++)
                {
                    bIsPositionExists = false;
                    for (int j = 0; j < theList.Count; j++)
                    {
                        if (index == theList[j].Position)
                        {
                            bIsPositionExists = true;
                            break;
                        }
                    }
                    if (!bIsPositionExists)
                    {
                        if (index < theList.Count + 1)
                            nResult = index;
                        break;
                    }
                }
            }

            return nResult;
        }

        #endregion

    }

    /*public static class XmlDocExtension
    {
        public static string ToIndentedString(this XmlDocument doc)
        {
            var stringWriter = new System.IO.StringWriter(new StringBuilder());
            var xmlTextWriter = new XmlTextWriter(stringWriter) { Formatting = Formatting.Indented, IndentChar=' ' };
            doc.Save(xmlTextWriter);
            return stringWriter.ToString();
        }

        public static string Beautify(this XmlDocument doc)
        {
            StringBuilder sb = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                NewLineChars = "\r\n",
                NewLineHandling = NewLineHandling.Replace
            };
            using (XmlWriter writer = XmlWriter.Create(sb, settings))
            {
                doc.Save(writer);
            }
            return sb.ToString();
        }

    }*/

}
