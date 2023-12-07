using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.EthernetCommLib;
using Azure.WPF.Framework;

namespace Azure.ImagingSystem
{
    [Serializable]
    public class LaserModule : ViewModelBase
    {
        private int _LaserWavelength = 0;
        private IvSensorType _SensorType = IvSensorType.NA;
        private LaserChannels _LaserChannel = LaserChannels.None;   // Avoid XML serialization error
        private string _LaserSN = string.Empty;
        private int _FilterWavelength = 0;
        private string _FilterBandpass = string.Empty;

        public LaserModule()
        {
        }

        /// <summary>
        /// Laser wavelength
        /// </summary>
        public int LaserWavelength
        {
            get { return _LaserWavelength; }
            set
            {
                _LaserWavelength = value;
                RaisePropertyChanged("LaserWavelength");
            }
        }
        /// <summary>
        /// Sensor type (APD or PMT)
        /// </summary>
        public IvSensorType SensorType
        {
            get { return _SensorType; }
            set
            {
                _SensorType = value;
                RaisePropertyChanged("SensorType");
            }
        }
        public LaserChannels LaserChannel
        {
            get { return _LaserChannel; }
            set { _LaserChannel = value; }
        }
        /// <summary>
        /// Laser serial number
        /// </summary>
        public string LaserSN
        {
            get { return _LaserSN; }
            set { _LaserSN = value; }
        }
        public int FilterWavelength
        {
            get { return _FilterWavelength; }
            set
            {
                _FilterWavelength = value;
                RaisePropertyChanged("FilterWavelength");
                RaisePropertyChanged("Filter");
            }
        }
        public string FilterBandpass
        {
            get { return _FilterBandpass; }
            set
            {
                _FilterBandpass = value;
                RaisePropertyChanged("FilterBandpass");
                RaisePropertyChanged("Filter");
            }
        }
        public string Laser
        {
            get
            {
                string laser = string.Empty;
                if (_LaserWavelength > 0)
                {
                    laser = _FilterWavelength.ToString();
                }
                else
                {
                    laser = "--";
                }
                RaisePropertyChanged("LaserWavelength");
                return laser;
            }
        }
        /// <summary>
        /// Emission filter in Wavelength and Bandpass format (i.e., 518BP22)
        /// </summary>
        public string Filter
        {
            get
            {
                string filter = string.Empty;
                if (_FilterWavelength > 0)
                {
                    filter = string.Format("{0}BP{1}", _FilterWavelength, _FilterBandpass);
                }
                else
                {
                    filter = "--";
                }
                return filter;
            }
            set
            {
                if (value != null)
                {
                    if (value.Contains("BP"))
                    {
                        string[] numbers = System.Text.RegularExpressions.Regex.Split(value, @"\D+");
                        if (numbers.Length == 2)
                        {
                            if (!string.IsNullOrEmpty(numbers[0]))
                            {
                                _FilterWavelength = int.Parse(numbers[0]);
                            }
                            if (!string.IsNullOrEmpty(numbers[1]))
                            {
                                _FilterBandpass = numbers[1];
                            }
                        }
                    }
                }
            }
        }
    }
}
