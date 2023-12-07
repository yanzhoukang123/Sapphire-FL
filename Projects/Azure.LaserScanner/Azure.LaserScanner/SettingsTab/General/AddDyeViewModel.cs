using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input; //ICommand
using System.Collections.ObjectModel;   //ObservableCollection
using System.Xml;
using Azure.Common;
using Azure.WPF.Framework;
using Azure.ImagingSystem;
using Azure.Configuration.Settings;
using Azure.LaserScanner.ViewModel;

namespace Azure.LaserScanner
{
    class AddDyeViewModel : ViewModelBase
    {
        private ObservableCollection<DyeType> _DyeOptions;
        private DyeType _SelectedDye = null;
        private List<DyeType> _LasersChannelOptions;
        private DyeType _SelectedLaserChannel;
        private string _DyeName = string.Empty;
        private ICommand _AddDyeCommand;
        private ICommand _DeleteDyeCommand;
        private const string _SettingsConfigFilename = "CustSettings.xml";

        public AddDyeViewModel()
        {
            /*if (SettingsManager.ConfigSettings.DyeOptions != null)
            {
                // Populate laser channel wavelength
                if (_LasersChannelOptions == null)
                {
                    _LasersChannelOptions = new List<DyeType>();
                }
                else
                {
                    _LasersChannelOptions.Clear();
                }

                // Get the 4 lasers wavelength
                bool bFoundLaserA = false;
                bool bFoundLaserB = false;
                bool bFoundLaserC = false;
                bool bFoundLaserD = false;

                foreach (var dye in SettingsManager.ConfigSettings.DyeOptions)
                {
                    if (dye.LaserType == LaserType.LaserD && !bFoundLaserD) // 488nm
                    {
                        _LasersChannelOptions.Add(dye);
                        bFoundLaserD = true;
                    }
                    else if (dye.LaserType == LaserType.LaserB && !bFoundLaserB) // 520nm
                    {
                        _LasersChannelOptions.Add(dye);
                        bFoundLaserB = true;
                    }
                    else if (dye.LaserType == LaserType.LaserC && !bFoundLaserC) // 658nm
                    {
                        _LasersChannelOptions.Add(dye);
                        bFoundLaserC = true;
                    }
                    else if (dye.LaserType == LaserType.LaserA && !bFoundLaserA) // 784nm
                    {
                        _LasersChannelOptions.Add(dye);
                        bFoundLaserA = true;
                    }
                    if (bFoundLaserA && bFoundLaserB &&
                        bFoundLaserC && bFoundLaserD)
                    {
                        // Found the 4 lasers' wavelength
                        //
                        // sort list based on laser type
                        _LasersChannelOptions.Sort((x, y) =>
                            x.LaserType.CompareTo(y.LaserType));

                        break;
                    }

                }
            }*/
        }

        /*public ObservableCollection<DyeType> DyeOptions
        {
            get
            {
                if (_DyeOptions == null)
                    _DyeOptions = new ObservableCollection<DyeType>(SettingsManager.ConfigSettings.DyeOptions);
                return _DyeOptions;
            }
            set
            {
                if (_DyeOptions != null)
                    _DyeOptions.Clear();

                _DyeOptions = value;
                RaisePropertyChanged("DyeOptions");
            }
        }*/

        /*public DyeType SelectedDye
        {
            get { return _SelectedDye; }
            set
            {
                _SelectedDye = value;
                RaisePropertyChanged("SelectedDye");
            }
        }*/

        public List<DyeType> LasersChannelOptions
        {
            get { return _LasersChannelOptions; }
        }

        public DyeType SelectedLaserChannel
        {
            get { return _SelectedLaserChannel; }
            set
            {
                _SelectedLaserChannel = value;
                RaisePropertyChanged("SelectedLaserChannel");
            }
        }

        /*public string DyeName
        {
            get { return _DyeName; }
            set
            {
                _DyeName = value;
                RaisePropertyChanged("DyeName");
            }
        }*/

        /*public ICommand AddDyeCommand
        {
            get
            {
                if (_AddDyeCommand == null)
                {
                    _AddDyeCommand = new RelayCommand((p) => ExecuteAddDyeCommand(p), (p) => CanExecuteAddDyeCommand(p));
                }

                return _AddDyeCommand;
            }
        }

        private void ExecuteAddDyeCommand(object parameter)
        {
            if (!string.IsNullOrEmpty(_DyeName.Trim()) && _SelectedLaserChannel != null)
            {
                DyeType theNewDye = new DyeType();
                theNewDye.DisplayName = _DyeName;
                theNewDye.LaserType = _SelectedLaserChannel.LaserType;
                theNewDye.WaveLength = _SelectedLaserChannel.WaveLength;
                theNewDye.IsCustomDye = true;

                if (!IsDyeExists(SettingsManager.ConfigSettings.CustDyeOptions, theNewDye) &&
                    !IsDyeExists(SettingsManager.ConfigSettings.DyeOptions, theNewDye))
                {
                    int position = GetNextAvailablePosition(_DyeOptions);
                    theNewDye.Position = position;

                    // Add dye to the default dye list
                    SettingsManager.ConfigSettings.DyeOptions.Insert(position - 1, theNewDye);  // dyes list is zero index
                    // Update dye listview
                    this.DyeOptions = new ObservableCollection<DyeType>(SettingsManager.ConfigSettings.DyeOptions);

                    // Update dye type list for all methods.
                    // TODO: find a more efficient way of display dye options on each method
                    if (Workspace.This.FluorescenceVM.AppProtocolOptions != null &&
                        Workspace.This.FluorescenceVM.AppProtocolOptions.Count > 0)
                    {
                        foreach (var appMethod in Workspace.This.FluorescenceVM.AppProtocolOptions)
                        {
                            foreach (var signal in appMethod.SelectedScanRegion.SignalList)
                            {
                                signal.DyeOptions = new ObservableCollection<DyeType>(SettingsManager.ConfigSettings.DyeOptions);
                            }
                        }
                    }

                    // Add the new dye to 'CustSettings.xml'
                    AddDyeType(position, theNewDye);

                    DyeName = string.Empty; // clear dye name textbox
                    //SelectedDye = null;

                    Xceed.Wpf.Toolkit.MessageBox.Show("New dye added.");
                }
                else
                {
                    string caption = "Add dye...";
                    string message = "The dye already exists.";
                    Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            else
            {
                string caption = "Error adding a dye...";
                string message = string.Empty;
                if (string.IsNullOrEmpty(_DyeName.Trim()))
                {
                    message = "Please enter a valid dye name.";
                }
                else if (_SelectedLaserChannel == null)
                {
                    message = "Please select a laser channel.";
                }
                Xceed.Wpf.Toolkit.MessageBox.Show(message, caption);
            }
        }

        private bool CanExecuteAddDyeCommand(object parameter)
        {
            //return (!string.IsNullOrEmpty(_DyeName) && _SelectedLaserChannel != null);
            return true;
        }*/

        /*public ICommand DeleteDyeCommand
        {
            get
            {
                if (_DeleteDyeCommand == null)
                {
                    _DeleteDyeCommand = new RelayCommand((p) => ExecuteDeleteDyeCommand(p), (p) => CanExecuteDeleteDyeCommand(p));
                }

                return _DeleteDyeCommand;
            }
        }

        private void ExecuteDeleteDyeCommand(object parameter)
        {
            if (_SelectedDye != null)
            {
                string protocolName = string.Empty;
                string caption = string.Empty;
                string message = string.Empty;
                if (IsDyeTypeReferenced(_SelectedDye, ref protocolName))
                {
                    caption = "Delete sample type...";
                    message = string.Format("The selected dye is referenced by the protocol \"{0}\"\n" +
                                            "Are you sure you want to delete the dye \"{1}\"?",
                                            protocolName, _SelectedDye.DisplayName);
                }
                else
                {
                    caption = "Delete selected dye...";
                    message = string.Format("Are you sure you want to delete the dye \"{0}\"?", _SelectedDye.DisplayName);
                }
                MessageBoxResult dlgResult = Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (dlgResult == System.Windows.MessageBoxResult.Yes)
                {
                    // Remove the selected dye from the configuration file (CustSettings.xml).
                    DeleteDyeType(_SelectedDye);

                    // Remove the selected dye from main dye list
                    SettingsManager.ConfigSettings.DyeOptions.Remove(_SelectedDye);
                    // Remove the selected dye from listview dye options
                    this.DyeOptions.Remove(_SelectedDye);
                    //RaisePropertyChanged("DyeOptions"); //Trigger UI update

                    // Update dye type list for all methods.
                    // TODO: find a more efficient way of display dye options on each method
                    if (Workspace.This.FluorescenceVM.AppProtocolOptions != null &&
                        Workspace.This.FluorescenceVM.AppProtocolOptions.Count > 0)
                    {
                        foreach (var appMethod in Workspace.This.FluorescenceVM.AppProtocolOptions)
                        {
                            foreach (var signal in appMethod.SelectedScanRegion.SignalList)
                            {
                                signal.DyeOptions = new ObservableCollection<DyeType>(SettingsManager.ConfigSettings.DyeOptions);
                            }
                        }
                    }

                    SelectedDye = null;

                    Xceed.Wpf.Toolkit.MessageBox.Show("The selected dye deleted.");
                }
            }
        }

        private bool CanExecuteDeleteDyeCommand(object parameter)
        {
            return (_SelectedDye != null && _SelectedDye.IsCustomDye);
        }*/

        /*private void AddDyeToDyeOptions(DyeType theNewDye)
        {
            // Add the dye to default dye options list.
            if (SettingsManager.ConfigSettings.DyeOptions != null)
            {
                if (theNewDye != null)
                {
                    int position = GetNextAvailablePosition(_DyeOptions);
                    theNewDye.Position = position;
                    // Add dye to current listview
                    //_DyeOptions.Insert(position - 1, theNewDye);
                    // Add dye to the default dye list
                    SettingsManager.ConfigSettings.DyeOptions.Insert(position-1, theNewDye);
                    this.DyeOptions = new ObservableCollection<DyeType>(SettingsManager.ConfigSettings.DyeOptions);

                    // Update dye type list for all methods.
                    // TODO: find a more efficient way of display dye options on each method
                    if (Workspace.This.FluorescenceVM.AppProtocolOptions != null &&
                        Workspace.This.FluorescenceVM.AppProtocolOptions.Count > 0)
                    {
                        foreach (var appMethod in Workspace.This.FluorescenceVM.AppProtocolOptions)
                        {
                            foreach (var scanRegion in appMethod.ScanRegions)
                            {
                                foreach (var signal in scanRegion.SignalList)
                                {
                                    signal.DyeOptions = new ObservableCollection<DyeType>(SettingsManager.ConfigSettings.DyeOptions);
                                }
                            }
                        }
                    }
                }
            }

        }*/

        /*private void DeleteDyeFromDyeOptions(DyeType otherDye)
        {
            // Remove the dye from the default dye options list.
            if (SettingsManager.ConfigSettings.DyeOptions != null)
            {
                if (otherDye != null)
                {
                    // Remove the dye from the main default list
                    SettingsManager.ConfigSettings.DyeOptions.Remove(otherDye);
                    this.DyeOptions = new ObservableCollection<DyeType>(SettingsManager.ConfigSettings.DyeOptions);

                    // Update dye type list for all methods.
                    // TODO: find a more efficient way of display dye options on each method
                    if (Workspace.This.FluorescenceVM.AppProtocolOptions != null &&
                        Workspace.This.FluorescenceVM.AppProtocolOptions.Count > 0)
                    {
                        foreach (var appMethod in Workspace.This.FluorescenceVM.AppProtocolOptions)
                        {
                            foreach (var scanRegion in appMethod.ScanRegions)
                            {
                                foreach (var signal in scanRegion.SignalList)
                                {
                                    signal.DyeOptions = new ObservableCollection<DyeType>(SettingsManager.ConfigSettings.DyeOptions);
                                }
                            }
                        }
                    }
                }
            }
        }*/

        private int GetPositionMax(ObservableCollection<DyeType> theList)
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

        private int GetNextAvailablePosition(ObservableCollection<DyeType> theList)
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

        /// <summary>
        /// Add dye to the configuration file (CustSettings.xml).
        /// </summary>
        /// <param name="dyeTypeTba"></param>
        /*private void AddDyeType(int mainListPosition, DyeType dyeTypeTba)
        {
            string filePath = System.IO.Path.Combine(Workspace.This.AppDataPath, _SettingsConfigFilename);

            XmlDocument XDoc = new XmlDocument();
            //XDoc.PreserveWhitespace = true;   //this flag override auto indent settings
            XmlNode parentNode = null;
            XmlElement childElement = null;

            if (System.IO.File.Exists(filePath))
            {
                XDoc.Load(filePath);
                parentNode = XDoc.SelectSingleNode("CustSettings/Dyes");
                childElement = XDoc.CreateElement("Dye");

                int custPosition = GetPosition(mainListPosition); // get position where to add/insert the new item
                parentNode.InsertBefore(childElement, parentNode.ChildNodes[custPosition]);
            }
            else
            {
                // CustSettings.xml doesn't exist create an new file.
                //
                // Create an XML declaration. 
                XmlDeclaration xmldecl;
                xmldecl = XDoc.CreateXmlDeclaration("1.0", null, null);
                xmldecl.Encoding = "UTF-8";

                // Add the new node to the document.
                XmlElement root = XDoc.DocumentElement;
                XDoc.InsertBefore(xmldecl, root);

                //XDoc.CreateComment("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
                XmlNode rootNode = XDoc.CreateElement("CustSettings");
                XDoc.AppendChild(rootNode);
                parentNode = XDoc.CreateElement("Dyes");
                rootNode.AppendChild(parentNode);
                childElement = XDoc.CreateElement("Dye");
                parentNode.AppendChild(childElement);
            }

            childElement.SetAttribute("Position", dyeTypeTba.Position.ToString());
            childElement.SetAttribute("DisplayName", dyeTypeTba.DisplayName);
            childElement.SetAttribute("Laser", dyeTypeTba.LaserType.ToString());
            childElement.SetAttribute("WaveLength", dyeTypeTba.WaveLength);

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
        }*/

        /// <summary>
        /// Get new dye position to insert in the custom dye list
        /// </summary>
        /// <param name="globalListPos"></param>
        /// <returns></returns>
        /*private int GetPosition(int globalListPos)
        {
            int nResult = SettingsManager.ConfigSettings.CustDyeOptions.Count + 1;

            for (int i = 0; i < SettingsManager.ConfigSettings.CustDyeOptions.Count; i++)
            {
                if (globalListPos > SettingsManager.ConfigSettings.CustDyeOptions[i].Position)
                {
                    continue;
                }
                else
                {
                    nResult = i;
                }
            }

            return nResult;
        }*/

        /// <summary>
        /// Remove the dye from the configuration file (CustSettings.xml).
        /// </summary>
        /// <param name="dyeTypeTbd"></param>
        public bool DeleteDyeType(DyeType dyeTbd)
        {
            if (dyeTbd == null) return false;

            bool bResult = false;
            string configFilePath = System.IO.Path.Combine(Workspace.This.AppDataPath, _SettingsConfigFilename);
            bool bIsSampleTypeRemoved = false;

            // Find and remove from 'CustSettings.xml'
            if (System.IO.File.Exists(configFilePath))
            {
                XmlDocument doc = new XmlDocument();
                //doc.PreserveWhitespace = true;
                doc.Load(configFilePath);

                XmlNodeList elemList = doc.GetElementsByTagName("Dye");
                for (int i = 0; i < elemList.Count; i++)
                {
                    string attrDispName = elemList[i].Attributes["DisplayName"].Value;
                    string attrLaser = elemList[i].Attributes["Laser"].Value;
                    string attrWavelength = elemList[i].Attributes["WaveLength"].Value;
                    if (attrDispName.Equals(dyeTbd.DisplayName, StringComparison.OrdinalIgnoreCase) && 
                        attrLaser == dyeTbd.LaserType.ToString() &&
                        attrWavelength == dyeTbd.WaveLength)
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

        private bool IsDyeExists(List<DyeType> dyeList, DyeType otherDye)
        {
            bool bResult = false;
            if (dyeList != null)
            {
                foreach (var dye in dyeList)
                {
                    if (dye.DisplayName.ToLower() == otherDye.DisplayName.ToLower() &&
                        dye.LaserType == otherDye.LaserType)
                    {
                        bResult = true;
                        break;
                    }
                }
            }

            return bResult;
        }

        /*private bool IsDyeTypeReferenced(DyeType dyeType, ref string protocolName)
        {
            bool bResult = false;

            if (dyeType != null)
            {
                if (Workspace.This.FluorescenceVM.AppProtocolOptions != null &&
                    Workspace.This.FluorescenceVM.AppProtocolOptions.Count > 0)
                {
                    foreach (var appProtocol in Workspace.This.FluorescenceVM.AppProtocolOptions)
                    {
                        foreach (var signal in appProtocol.SelectedScanRegion.SignalList)
                        {
                            if (signal.SelectedDye.DisplayName.Equals(dyeType.DisplayName, StringComparison.OrdinalIgnoreCase) &&
                                signal.SelectedDye.Position == dyeType.Position)
                            {
                                protocolName = appProtocol.ProtocolName;
                                return true;
                            }
                        }
                    }
                }
            }

            return bResult;
        }*/

    }
}
