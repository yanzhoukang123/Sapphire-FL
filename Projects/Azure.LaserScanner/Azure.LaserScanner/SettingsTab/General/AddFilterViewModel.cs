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
    class AddFilterViewModel : ViewModelBase
    {
        private ObservableCollection<FilterType> _FilterOptions;
        private FilterType _SelectedFilter = null;
        private bool _IsAddFilterMode = true;
        private bool _IsEditFilterMode = false;
        private int _Wavelength = 0;
        private string _Bandpass = string.Empty;
        private ICommand _ApplyCommand;
        private ICommand _DeleteFilterCommand;
        private ICommand _LaserFilterPairingCommand;
        private const string _SysSettingsConfigFile = "SysSettings.xml";

        public AddFilterViewModel()
        {
            if (SettingsManager.ConfigSettings.FilterOptions != null)
            {
                _FilterOptions = new ObservableCollection<FilterType>(SettingsManager.ConfigSettings.FilterOptions);
            }
        }

        public ObservableCollection<FilterType> FilterOptions
        {
            get
            {
                if (_FilterOptions == null)
                    _FilterOptions = new ObservableCollection<FilterType>(SettingsManager.ConfigSettings.FilterOptions);
                return _FilterOptions;
            }
            set
            {
                if (_FilterOptions != null)
                    _FilterOptions.Clear();

                _FilterOptions = value;
                RaisePropertyChanged("FilterOptions");
            }
        }

        public FilterType SelectedFilter
        {
            get { return _SelectedFilter; }
            set
            {
                if (_SelectedFilter != value)
                {
                    _SelectedFilter = value;
                    RaisePropertyChanged("_SelectedFilter");
                    RaisePropertyChanged("IsEnableFilterEdit");

                    if (_SelectedFilter != null && _IsEditFilterMode)
                    {
                        //this.Position = _SelectedSampleType.Position;
                        this.Wavelength = _SelectedFilter.Wavelength;
                        this.Bandpass = _SelectedFilter.Bandpass;
                    }
                }
            }
        }

        public bool IsAddFilterMode
        {
            get { return _IsAddFilterMode; }
            set
            {
                _IsAddFilterMode = value;
                RaisePropertyChanged("IsAddFilterMode");
            }
        }

        public bool IsEditFilterMode
        {
            get { return _IsEditFilterMode; }
            set
            {
                _IsEditFilterMode = value;
                RaisePropertyChanged("IsEditFilterMode");
                if (_IsEditFilterMode)
                {
                    if (_SelectedFilter != null)
                    {
                        //this.Position = _SelectedFilter.Position;
                        this.Wavelength = _SelectedFilter.Wavelength;
                        this.Bandpass = _SelectedFilter.Bandpass;
                    }
                }
            }
        }

        public bool IsEnableFilterEdit
        {
            get
            {
                if (_SelectedFilter != null)
                    return true;
                else
                    return false;
            }
        }

        public int Wavelength
        {
            get { return _Wavelength; }
            set
            {
                _Wavelength = value;
                RaisePropertyChanged("Wavelength");
            }
        }

        public string Bandpass
        {
            get { return _Bandpass; }
            set
            {
                _Bandpass = value;
                RaisePropertyChanged("Bandpass");
            }
        }

        /// <summary>
        /// Add or edit filter command
        /// </summary>
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
            if (IsAddFilterMode)
            {
                if (Wavelength > 0)
                {
                    string caption = string.Empty;
                    string message = string.Empty;

                    if (IsFilterExists(Wavelength, Bandpass))
                    {
                        caption = "Add emission filter...";
                        message = "The emission filter already exists.";
                        Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    FilterType filterType = new FilterType();

                    //int position = GetNextAvailablePosition(_SampleTypeFocusOptions);
                    //filterType.Position = position;
                    filterType.Wavelength = Wavelength;
                    filterType.Bandpass = Bandpass;

                    // Add new filter type to filter type options (current listview)
                    this.FilterOptions.Add(filterType);

                    // Add new filter type to the original filter type configuration settings
                    SettingsManager.ConfigSettings.FilterOptions.Add(filterType);
                    // Add to Imaging/Fluorescence sample type list
                    //Workspace.This.FluorescenceVM.SampleTypeOptions.Insert(sampleTypeSet.Position - 1, sampleTypeSet);

                    //foreach (var proto in Workspace.This.FluorescenceVM.AppProtocolOptions)
                    //{
                    //    foreach (var scanRegion in proto.ScanRegions)
                    //    {
                    //        scanRegion.SampleTypeOptions.Insert(filterType.Position - 1, filterType);
                    //    }
                    //}

                    RaisePropertyChanged("FilterOptions");

                    //Clear new entry
                    Wavelength = 0;
                    Bandpass = string.Empty;

                    //Save filter type to SysSettings.xml
                    AddFilter(filterType);

                    caption = "Add emission filter...";
                    message = "New emission filter added.";
                    Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else if (IsEditFilterMode)
            {
                if (_SelectedFilter != null)
                {
                    if (Wavelength > 0)
                    {
                        foreach (var filter in FilterOptions)
                        {
                            if (filter.Wavelength != Wavelength || filter.Bandpass != Bandpass)
                            {
                                // Update 'SysSettings.xml file
                                // Must be called before the selected filter changes value.
                                UpdateFilter(_SelectedFilter, new FilterType(Wavelength, Bandpass));

                                filter.Wavelength = Wavelength;
                                filter.Bandpass = Bandpass;
                                // Doing it this way because updating an element in the list does not update the UI
                                // Behave differently than add and remove to list
                                FilterOptions = new ObservableCollection<FilterType>(_FilterOptions);
                                //Workspace.This.FluorescenceVM.SampleTypeOptions = new ObservableCollection<SampleTypeSetting>(_SampleTypeFocusOptions);

                                //EL: TODO: laser + filter pairing
                                //
                                //foreach (var proto in Workspace.This.FluorescenceVM.AppProtocolOptions)
                                //{
                                //    foreach (var scanRegion in proto.ScanRegions)
                                //    {
                                //        scanRegion.FilterOptions = new ObservableCollection<FilterType>(_FilterOptions);
                                //    }
                                //}

                                // Add 'Custom' option to sample types list
                                //SampleTypeSetting customSampleType = new SampleTypeSetting();
                                //customSampleType.DisplayName = "Custom";
                                //customSampleType.FocusPosition = 0;
                                //Workspace.This.FluorescenceVM.SampleTypeOptions.Add(customSampleType);

                                //foreach (var proto in Workspace.This.FluorescenceVM.AppProtocolOptions)
                                //{
                                //    foreach (var scanRegion in proto.ScanRegions)
                                //    {
                                //        scanRegion.SampleTypeOptions.Add(customSampleType);
                                //    }
                                //}

                                string caption = "Edit emission filter...";
                                string message = "Selected emission filter updated.";
                                Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Information);
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

        public ICommand DeleteFilterCommand
        {
            get
            {
                if (_DeleteFilterCommand == null)
                {
                    _DeleteFilterCommand = new RelayCommand((p) => ExecuteDeleteFilterCommand(p), (p) => CanExecuteDeleteFilterCommand(p));
                }

                return _DeleteFilterCommand;
            }
        }
        private void ExecuteDeleteFilterCommand(object parameter)
        {
            if (_SelectedFilter != null)
            {
                //string protocolName = string.Empty;
                //string caption = string.Empty;
                //string message = string.Empty;
                //if (IsSampleTypeReferenced(_SelectedSampleType, ref protocolName))
                //{
                //    caption = "Delete sample type...";
                //    message = string.Format("The selected sample type is referenced by the protocol \"{0}\"\n" +
                //                            "Are you sure you want to delete the sample type \"{1}\"?",
                //                            protocolName, _SelectedSampleType.DisplayName);
                //}
                //else
                //{
                //    caption = "Delete sample type...";
                //    message = string.Format("Are you sure you want to delete the sample type \"{0}\"?", _SelectedSampleType.DisplayName);
                //}
                string caption = "Delete selected filter...";
                string message = "Are you sure you want to delete the selected emission filter.";
                MessageBoxResult dlgResult = Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (dlgResult == System.Windows.MessageBoxResult.Yes)
                {
                    // Remove selected filter from the SysSettings.xml file.
                    DeleteFilter(_SelectedFilter);

                    // Remove selected filter from the default filter options list.
                    SettingsManager.ConfigSettings.FilterOptions.Remove(_SelectedFilter);

                    // Remove from Fluorescence imaging tab.
                    //Workspace.This.FluorescenceVM.SampleTypeOptions.Remove(_SelectedSampleType);

                    //foreach (var proto in Workspace.This.FluorescenceVM.AppProtocolOptions)
                    //{
                    //    foreach (var scanRegion in proto.ScanRegions)
                    //    {
                    //        scanRegion.FilterOptions.Remove(_SelectedFilter);
                    //    }
                    //}

                    // Remove the selected filter from General settings tab filter list
                    _FilterOptions.Remove(_SelectedFilter);
                    RaisePropertyChanged("FilterOptions");

                    // Select none.
                    SelectedFilter = null;
                }
            }
        }
        private bool CanExecuteDeleteFilterCommand(object parameter)
        {
            return true;
        }

        /// <summary>
        /// Add filter to the configuration file (SysSettings.xml).
        /// </summary>
        /// <param name="samepleTypeTba"></param>
        private void AddFilter(FilterType filterTypeTba)
        {
            string filePath = System.IO.Path.Combine(Workspace.This.AppDataPath, _SysSettingsConfigFile);

            XmlDocument XDoc = new XmlDocument();
            //XDoc.PreserveWhitespace = true;   //this flag override auto indent settings
            XmlNode parentNode = null;
            XmlElement childElement = null;

            if (System.IO.File.Exists(filePath))
            {
                XDoc.Load(filePath);
                parentNode = XDoc.SelectSingleNode("SysSettings/EmissionFilters");
                if (parentNode == null)
                {
                    //Did not find the node, create a new node
                    parentNode = XDoc.SelectSingleNode("SysSettings");
                    childElement = XDoc.CreateElement("EmissionFilters");
                    parentNode.AppendChild(childElement);
                    parentNode = XDoc.SelectSingleNode("SysSettings/EmissionFilters");
                }
                childElement = XDoc.CreateElement("EmissionFilter");
                parentNode.AppendChild(childElement); // position is 1 index, list is zero index

                //childElement.SetAttribute("Position", samepleTypeTba.Position.ToString());
                childElement.SetAttribute("Wavelength", filterTypeTba.Wavelength.ToString());
                childElement.SetAttribute("Bandpass", filterTypeTba.Bandpass);

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
        /// Remove filter from configuration file (SysSettings.xml).
        /// </summary>
        /// <param name="sampleTypeTbd"></param>
        private bool DeleteFilter(FilterType filterTypeTbd)
        {
            if (filterTypeTbd == null) return false;

            bool bResult = false;
            string configFilePath = System.IO.Path.Combine(Workspace.This.AppDataPath, _SysSettingsConfigFile);
            bool bIsFilterTypeRemoved = false;

            // find and remove from 'SysSettings.xml'
            if (System.IO.File.Exists(configFilePath))
            {
                XmlDocument doc = new XmlDocument();
                //doc.PreserveWhitespace = true;
                doc.Load(configFilePath);

                XmlNodeList elemList = doc.GetElementsByTagName("EmissionFilter");
                for (int i = 0; i < elemList.Count; i++)
                {
                    string attrWavelength = elemList[i].Attributes["Wavelength"].Value;
                    string attrBandpass = elemList[i].Attributes["Bandpass"].Value;
                    if (attrWavelength == filterTypeTbd.Wavelength.ToString() &&
                        attrBandpass == filterTypeTbd.Bandpass)
                    {
                        elemList[i].ParentNode.RemoveChild(elemList[i]);
                        bIsFilterTypeRemoved = true;
                        break;
                    }
                }

                if (bIsFilterTypeRemoved)
                {
                    doc.Beautify();
                    doc.Save(configFilePath);
                    bResult = true;
                }
            }
            return bResult;
        }

        /// <summary>
        /// Update/modify selected filter on the configuration file (SysSettings.xml).
        /// </summary>
        /// <param name="selectSampleType"></param>
        /// <returns></returns>
        private bool UpdateFilter(FilterType selectedFilterType, FilterType updatingFilterType)
        {
            bool bResult = false;
            bool bSaveDoc = false;
            string configFilePath = System.IO.Path.Combine(Workspace.This.AppDataPath, _SysSettingsConfigFile);

            XmlDocument xdoc = new XmlDocument();
            xdoc.PreserveWhitespace = true;
            xdoc.Load(configFilePath);

            XmlNodeList filterTypeElements = xdoc.GetElementsByTagName("EmissionFilter");

            if (filterTypeElements != null)
            {
                for (int i = 0; i < filterTypeElements.Count; i++)
                {
                    if (filterTypeElements[i].NodeType == XmlNodeType.Element)
                    {
                        string attrWavelength = filterTypeElements[i].Attributes["Wavelength"].Value;
                        string attrBandpass = filterTypeElements[i].Attributes["Bandpass"].Value;
                        int nWavelength = int.Parse(attrWavelength);
                        //int nBandpass = int.Parse(attrBandpass);
                        if (nWavelength == selectedFilterType.Wavelength && attrBandpass == selectedFilterType.Bandpass)
                        {
                            try
                            {
                                filterTypeElements[i].Attributes["Wavelength"].Value = updatingFilterType.Wavelength.ToString();
                                filterTypeElements[i].Attributes["Bandpass"].Value = updatingFilterType.Bandpass;
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

        private bool IsFilterExists(int wavelength, string bandpass)
        {
            bool bResult = false;
            // check if the elements already exists.
            bResult = _FilterOptions.ToList().Exists(element => element.Wavelength == wavelength && element.Bandpass == bandpass);
            return bResult;
        }

        public ICommand LaserFilterPairingCommand
        {
            get
            {
                if (_LaserFilterPairingCommand == null)
                {
                    _LaserFilterPairingCommand = new RelayCommand((p) => ExecuteLaserFilterPairingCommand(p), (p) => CanExecuteLaserFilterPairingCommand(p));
                }

                return _LaserFilterPairingCommand;
            }
        }
        private void ExecuteLaserFilterPairingCommand(object parameter)
        {
            string message = message = "Laser and emission filter pairing. Please select the corresponding filter.";
            //if (SettingsManager.ApplicationSettings.IsChangeLaserModuleInitiated)
            //{
            //    message = "Laser and emission pairing. Please select the corresponding filter.";
            //    // Reset laser module change initiated flag 
            //    SettingsManager.ApplicationSettings.IsChangeLaserModuleInitiated = false;
            //}

            if (SettingsManager.ConfigSettings.FilterOptions == null)
            {
                string caption = "Laser and Filter Pairing...";
                message = "Filter list not found";
                Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            LaserModuleFilterPairingViewModel laserModuleFilterVm = new LaserModuleFilterPairingViewModel();
            laserModuleFilterVm.FilterOptions = new System.Collections.ObjectModel.ObservableCollection<FilterType>(SettingsManager.ConfigSettings.FilterOptions);

            if (Workspace.This.LaserL1 > 0)
            {
                laserModuleFilterVm.LaserModuleL1 = Workspace.This.LaserModuleL1;
                if (Workspace.This.LaserModuleL1.FilterWavelength > 0)
                {
                    int index = laserModuleFilterVm.FilterOptions.ToList().FindIndex(x => x.Wavelength == Workspace.This.LaserModuleL1.FilterWavelength && x.Bandpass == Workspace.This.LaserModuleL1.FilterBandpass);
                    if (index >= 0)
                    {
                        laserModuleFilterVm.SelectedFilterL1 = laserModuleFilterVm.FilterOptions[index];
                    }
                }
            }
            if (Workspace.This.LaserR1 > 0)
            {
                laserModuleFilterVm.LaserModuleR1 = Workspace.This.LaserModuleR1;
                if (Workspace.This.LaserModuleR1.FilterWavelength > 0)
                {
                    int index = laserModuleFilterVm.FilterOptions.ToList().FindIndex(x => x.Wavelength == Workspace.This.LaserModuleR1.FilterWavelength && x.Bandpass == Workspace.This.LaserModuleR1.FilterBandpass);
                    if (index >= 0)
                    {
                        laserModuleFilterVm.SelectedFilterR1 = laserModuleFilterVm.FilterOptions[index];
                    }
                }
            }
            if (Workspace.This.LaserR2 > 0)
            {
                laserModuleFilterVm.LaserModuleR2 = Workspace.This.LaserModuleR2;
                if (Workspace.This.LaserModuleR2.FilterWavelength > 0)
                {
                    int index = laserModuleFilterVm.FilterOptions.ToList().FindIndex(x => x.Wavelength == Workspace.This.LaserModuleR2.FilterWavelength && x.Bandpass == Workspace.This.LaserModuleR2.FilterBandpass);
                    if (index >= 0)
                    {
                        laserModuleFilterVm.SelectedFilterR2 = laserModuleFilterVm.FilterOptions[index];
                    }
                }
            }

            LaserModuleFilterPairing laserModuleFilterEditView = new LaserModuleFilterPairing();
            laserModuleFilterEditView.InfoText = message;
            laserModuleFilterEditView.DataContext = laserModuleFilterVm;
            var dlgResult = laserModuleFilterEditView.ShowDialog();
            if (dlgResult == true)
            {
                //OK selected...do something
                //
                if (Workspace.This.LaserL1 > 0 && laserModuleFilterVm.SelectedFilterL1 != null)
                {
                    Workspace.This.LaserModuleL1.FilterWavelength = laserModuleFilterVm.SelectedFilterL1.Wavelength;
                    Workspace.This.LaserModuleL1.FilterBandpass = laserModuleFilterVm.SelectedFilterL1.Bandpass;
                    foreach (var laser in Workspace.This.LaserOptions)
                    {
                        if (laser.LaserChannel == EthernetCommLib.LaserChannels.ChannelC)
                        {
                            laser.Filter = laserModuleFilterVm.SelectedFilterL1.Filter;
                            break;
                        }
                    }
                }
                if (Workspace.This.LaserR1 > 0 && laserModuleFilterVm.SelectedFilterR1 != null)
                {
                    Workspace.This.LaserModuleR1.FilterWavelength = laserModuleFilterVm.SelectedFilterR1.Wavelength;
                    Workspace.This.LaserModuleR1.FilterBandpass = laserModuleFilterVm.SelectedFilterR1.Bandpass;
                    foreach (var laser in Workspace.This.LaserOptions)
                    {
                        if (laser.LaserChannel == EthernetCommLib.LaserChannels.ChannelA)
                        {
                            laser.Filter = laserModuleFilterVm.SelectedFilterR1.Filter;
                            break;
                        }
                    }
                }
                if (Workspace.This.LaserR2 > 0 && laserModuleFilterVm.SelectedFilterR2 != null)
                {
                    Workspace.This.LaserModuleR2.FilterWavelength = laserModuleFilterVm.SelectedFilterR2.Wavelength;
                    Workspace.This.LaserModuleR2.FilterBandpass = laserModuleFilterVm.SelectedFilterR2.Bandpass;
                    foreach (var laser in Workspace.This.LaserOptions)
                    {
                        if (laser.LaserChannel == EthernetCommLib.LaserChannels.ChannelB)
                        {
                            laser.Filter = laserModuleFilterVm.SelectedFilterR2.Filter;
                            break;
                        }
                    }
                }
                Workspace.This.FluorescenceVM.LoadAppProtocols(SettingsManager.ConfigSettings.Protocols);
            }
        }
        private bool CanExecuteLaserFilterPairingCommand(object parameter)
        {
            return true;
        }


    }
}
