using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;   //ObservableCollection
using System.Windows.Input; //ICommand
using System.Windows;
using System.Runtime.Serialization.Formatters.Binary;   //BinaryFormatter
using System.IO;    //MemoryStream
using Azure.ImagingSystem;
using Azure.WPF.Framework;
using Azure.Configuration.Settings;
using Azure.Image.Processing;

namespace Azure.LaserScanner.ViewModel
{
    [Serializable]
    class ProtocolViewModel : ViewModelBase
    {
        public delegate void ScanRegionChangedDelegate(object sender);
        public event ScanRegionChangedDelegate ScanRegionChanged;

        #region Private data...

        private string _ProtocolName = "New Protocol";
        private ObservableCollection<ScanRegionViewModel> _ScanRegions;
        private ScanRegionViewModel _SelectedScanRegion;

        private bool _IsModified = false;
        private ObservableCollection<ScanRegionViewModel> _ScanRegionsBkup;

        #endregion

        #region Constructors...

        public ProtocolViewModel()
        {
            _ScanRegions = new ObservableCollection<ScanRegionViewModel>();
        }

        public ProtocolViewModel(ProtocolViewModel otherProtocol)
        {
            _ProtocolName = otherProtocol._ProtocolName;
            _ScanRegions = new ObservableCollection<ScanRegionViewModel>(otherProtocol.ScanRegions);
            _SelectedScanRegion = _ScanRegions.First();
        }

        #endregion

        //public static T DeepClone<T>(this T obj)
        //{
        //    using (var ms = new System.IO.MemoryStream())
        //    {
        //        var formatter = new BinaryFormatter();
        //        formatter.Serialize(ms, obj);
        //        ms.Position = 0;
        //
        //        return (T)formatter.Deserialize(ms);
        //    }
        //}

        #region Public properties...

        public string ProtocolName
        {
            get { return _ProtocolName; }
            set
            {
                if (_ProtocolName != value)
                {
                    _ProtocolName = value;
                    RaisePropertyChanged("ProtocolName");
                }
            }
        }

        public bool IsModified
        {
            get { return _IsModified; }
            set
            {
                if (!_IsModified)
                {
                    BackupScanRegions();
                }
                else if (_IsModified && value == false)
                {
                    RestoreScanRegions();
                }
                _IsModified = value;
                RaisePropertyChanged("IsModified");
            }
        }
        public bool IsInitializing { get; set; } = false;

        public bool IsAlwaysVisible { get; set; } = false;

        /// <summary>
        /// Allow tab (or scan region) switching?
        /// Uses: see: SignalControl.xaml.cs
        /// </summary>
        //public bool IsAllowScanRegionChanged { get; set; } = false;

        public ObservableCollection<ScanRegionViewModel> ScanRegions
        {
            get { return _ScanRegions; }
            set
            {
                _ScanRegions = value;
                RaisePropertyChanged("ScanRegions");
            }
        }
        public ScanRegionViewModel SelectedScanRegion
        {
            get { return _SelectedScanRegion; }
            set
            {
                if (_SelectedScanRegion != value)
                {
                    bool bIsSelectionChanged = false;
                    if (_SelectedScanRegion != null)
                    {
                        bIsSelectionChanged = true;
                    }

                    _SelectedScanRegion = value;
                    RaisePropertyChanged("SelectedScanRegion");

                    if (bIsSelectionChanged && !IsInitializing)
                    {
                        // Notify subscriber of the selection changed
                        ScanRegionChanged?.Invoke(this);
                    }
                }
            }
        }

        #endregion

        public void BackupScanRegions()
        {
            if (_ScanRegionsBkup == null)
            {
                _ScanRegionsBkup = new ObservableCollection<ScanRegionViewModel>();
            }
            else
            {
                _ScanRegionsBkup.Clear();
            }

            foreach (var scanRegion in _ScanRegions)
            {
                _ScanRegionsBkup.Add((ScanRegionViewModel)scanRegion.Clone());
            }
        }
        public void RestoreScanRegions()
        {
            if (_ScanRegionsBkup != null && _ScanRegionsBkup.Count > 0)
            {
                IsInitializing = true;

                ScanRegions = new ObservableCollection<ScanRegionViewModel>(_ScanRegionsBkup);

                if (ScanRegions != null && ScanRegions.Count > 0)
                {
                    foreach (ScanRegionViewModel scanRegion in ScanRegions)
                    {
                        scanRegion.IsInitializing = true;
                        int index = scanRegion.PixelSizeOptions.ToList().FindIndex(x => x.Position == scanRegion.SelectedPixelSize.Position);
                        if (index >= 0)
                        {
                            scanRegion.SelectedPixelSize = scanRegion.PixelSizeOptions[index];
                        }
                        index = scanRegion.ScanSpeedOptions.ToList().FindIndex(x => x.Position == scanRegion.SelectedScanSpeed.Position);
                        if (index >= 0)
                        {
                            scanRegion.SelectedScanSpeed = scanRegion.ScanSpeedOptions[index];
                        }
                        // Phosphor Imaging tab no longer has the scan Quality selection option
                        if (scanRegion.SelectedScanQuality != null)
                        {
                            index = scanRegion.ScanQualityOptions.ToList().FindIndex(x => x.Position == scanRegion.SelectedScanQuality.Position);
                            if (index >= 0)
                            {
                                scanRegion.SelectedScanQuality = scanRegion.ScanQualityOptions[index];
                            }
                        }
                        index = scanRegion.SampleTypeOptions.ToList().FindIndex(x => x.Position == scanRegion.SelectedSampleType.Position);
                        if (index >= 0)
                        {
                            scanRegion.SelectedSampleType = scanRegion.SampleTypeOptions[index];
                        }

                        // Phosphor imaging selected signal level
                        if (scanRegion.SelectedSignalLevel != null)
                        {
                            index = scanRegion.SignalLevelOptions.ToList().FindIndex(x => x.IntensityLevel == scanRegion.SelectedSignalLevel.IntensityLevel);
                            if (index >= 0)
                            {
                                scanRegion.SelectedSignalLevel = scanRegion.SignalLevelOptions[index];
                            }
                        }

                        for (int i = 0; i < scanRegion.SignalList.Count; i++)
                        {
                            scanRegion.SignalList[i].IsInitializing = true;

                            LaserTypes laserItem = scanRegion.SignalList[i].LaserOptions.Single(item => item.LaserChannel == scanRegion.SignalList[i].SelectedLaser.LaserChannel);
                            index = scanRegion.SignalList[i].LaserOptions.IndexOf(laserItem);
                            if (index >= 0)
                            {
                                scanRegion.SignalList[i].SelectedLaser = scanRegion.SignalList[i].LaserOptions[index];
                            }
                            // point the selected intensity level to the intensities list
                            var intItem = scanRegion.SignalList[i].SignalLevelOptions.Single(item => item.IntensityLevel == scanRegion.SignalList[i].SelectedSignalLevel.IntensityLevel);
                            index = scanRegion.SignalList[i].SignalLevelOptions.IndexOf(intItem);
                            if (index >= 0)
                            {
                                scanRegion.SignalList[i].SelectedSignalLevel = scanRegion.SignalList[i].SignalLevelOptions[index];
                            }
                            // point the selected color channel to the color channels list
                            var colorChItem = scanRegion.SignalList[i].ColorChannelOptions.Single(item => item.ImageColorChannel == scanRegion.SignalList[i].SelectedColorChannel.ImageColorChannel);
                            index = scanRegion.SignalList[i].ColorChannelOptions.IndexOf(colorChItem);
                            if (index >= 0)
                            {
                                scanRegion.SignalList[i].SelectedColorChannel = scanRegion.SignalList[i].ColorChannelOptions[index];
                            }

                            scanRegion.SignalList[i].IsInitializing = false;
                        }
                        scanRegion.IsInitializing = false;
                    }
                }

                // Settings restored - reset the modified flag
                _IsModified = false;

                IsInitializing = false;

                SelectedScanRegion = ScanRegions.FirstOrDefault();
            }
        }

        public void AddScanRegion(ScanRegionViewModel scanRegionToAdd, bool bIsInitializing)
        {
            if (!bIsInitializing)
            {
                IsModified = true;
            }

            ScanRegions.Add(scanRegionToAdd);
        }
        public void RemoveScanRegion(ScanRegionViewModel scanRegionToRemove)
        {
            // backup the scan region(s) (if not already backed up) before removing (so it can be restore)
            IsModified = true;
            ScanRegions.Remove(scanRegionToRemove);
        }

    }

    //public static class ExtensionMethods
    //{
    //    // Deep clone
    //    public static T DeepClone<T>(this T a)
    //    {
    //        using (MemoryStream stream = new MemoryStream())
    //        {
    //            BinaryFormatter formatter = new BinaryFormatter();
    //            formatter.Serialize(stream, a);
    //            stream.Position = 0;
    //            return (T)formatter.Deserialize(stream);
    //        }
    //    }
    //}
}
