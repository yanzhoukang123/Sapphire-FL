using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.ImagingSystem;
using Azure.WPF.Framework;

namespace Azure.LaserScanner.ViewModel
{
    class LaserModuleFilterPairingViewModel : ViewModelBase
    {
        private LaserModule _LaserModuleL1;
        private LaserModule _LaserModuleR1;
        private LaserModule _LaserModuleR2;

        private string _LaserModuleStatusL1 = string.Empty;
        private string _LaserModuleStatusR1 = string.Empty;
        private string _LaserModuleStatusR2 = string.Empty;

        private ObservableCollection<FilterType> _FilterOptions;
        private FilterType _SelectedFilterL1 = null;
        private FilterType _SelectedFilterR1 = null;
        private FilterType _SelectedFilterR2 = null;


        public LaserModule LaserModuleL1
        {
            get { return _LaserModuleL1; }
            set
            {
                _LaserModuleL1 = value;
                RaisePropertyChanged("LaserModuleL1");
            }
        }
        public LaserModule LaserModuleR1
        {
            get { return _LaserModuleR1; }
            set
            {
                _LaserModuleR1 = value;
                RaisePropertyChanged("LaserModuleR1");
            }
        }
        public LaserModule LaserModuleR2
        {
            get { return _LaserModuleR2; }
            set
            {
                _LaserModuleR2 = value;
                RaisePropertyChanged("LaserModuleR2");
            }
        }

        public ObservableCollection<FilterType> FilterOptions
        {
            get { return _FilterOptions; }
            set
            {
                _FilterOptions = value;
                RaisePropertyChanged("FilterOptions");
            }
        }

        public FilterType SelectedFilterL1
        {
            get { return _SelectedFilterL1; }
            set
            {
                _SelectedFilterL1 = value;
                RaisePropertyChanged("SelectedFilterL1");
            }
        }
        public FilterType SelectedFilterR1
        {
            get { return _SelectedFilterR1; }
            set
            {
                _SelectedFilterR1 = value;
                RaisePropertyChanged("SelectedFilterR1");
            }
        }
        public FilterType SelectedFilterR2
        {
            get { return _SelectedFilterR2; }
            set
            {
                _SelectedFilterR2 = value;
                RaisePropertyChanged("SelectedFilterR2");
            }
        }

        public string LaserModuleStatusL1
        {
            get { return _LaserModuleStatusL1; }
            set
            {
                _LaserModuleStatusL1 = value;
                RaisePropertyChanged("LaserModuleStatusL1");
            }
        }
        public string LaserModuleStatusR1
        {
            get { return _LaserModuleStatusR1; }
            set
            {
                _LaserModuleStatusR1 = value;
                RaisePropertyChanged("LaserModuleStatusR1");
            }
        }
        public string LaserModuleStatusR2
        {
            get { return _LaserModuleStatusR2; }
            set
            {
                _LaserModuleStatusR2 = value;
                RaisePropertyChanged("LaserModuleStatusR2");
            }
        }

        public bool IsEnableComboBoxL1
        {
            get
            {
                bool bIsEnabled = true;
                if (LaserModuleL1 == null || (LaserModuleL1 != null && LaserModuleL1.LaserWavelength == 0))
                {
                    bIsEnabled = false;
                }
                return bIsEnabled;
            }
        }
        public bool IsEnableComboBoxR1
        {
            get
            {
                bool bIsEnabled = true;
                if (LaserModuleR1 == null || (LaserModuleR1 != null && LaserModuleR1.LaserWavelength == 0))
                {
                    bIsEnabled = false;
                }
                return bIsEnabled;
            }
        }
        public bool IsEnableComboBoxR2
        {
            get
            {
                bool bIsEnabled = true;
                if (LaserModuleR2 == null || (LaserModuleR2 != null && LaserModuleR2.LaserWavelength == 0))
                {
                    bIsEnabled = false;
                }
                return bIsEnabled;
            }
        }

        public bool HasLasersAndFiltersPaired
        {
            get
            {
                bool bResult = false;
                bool bHasL1Filter = true;
                bool bHasR1Filter = true;
                bool bHasR2Filter = true;
                if (LaserModuleL1 != null)
                {
                    if (SelectedFilterL1 != null)
                    {
                        bHasL1Filter = true;
                    }
                    else
                    {
                        bHasL1Filter = false;
                    }
                }
                if (LaserModuleR1 != null)
                {
                    if (SelectedFilterR1 != null)
                    {
                        bHasR1Filter = true;
                    }
                    else
                    {
                        bHasR1Filter = false;
                    }
                }
                if (LaserModuleR2 != null)
                {
                    if (SelectedFilterR2 != null)
                    {
                        bHasR2Filter = true;
                    }
                    else
                    {
                        bHasR2Filter = false;
                    }
                }
                if (bHasL1Filter && bHasR1Filter && bHasR2Filter)
                {
                    bResult = true;
                }
                return bResult;
            }
        }

    }
}
