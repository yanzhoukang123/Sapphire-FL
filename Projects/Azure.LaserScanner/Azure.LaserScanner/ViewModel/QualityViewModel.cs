using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Azure.WPF.Framework;

namespace Azure.LaserScanner.ViewModel
{
    class QualityViewModel : ViewModelBase
    {
        //private ScanQualityData _SelectedScanQuality = new ScanQualityData();
        //private List<ScanQualityData> _ScanQualityOptions = new List<ScanQualityData>();
        private ScanQualityData _SelectedScanQuality = null;
        private List<ScanQualityData> _ScanQualityOptions = null;


        public QualityViewModel()
        {
            //_ScanQualityOptions.Add(new ScanQualityData { Position = 1, Image = "../Resources/Images/circle_blue16x16.png", DisplayName = "Low", Value = ScanQuality.Low });
            //_ScanQualityOptions.Add(new ScanQualityData { Position = 2, Image = "../Resources/Images/circle_blue16x16.png", DisplayName = "Medium", Value = ScanQuality.Medium });
            //_ScanQualityOptions.Add(new ScanQualityData { Position = 3, Image = "../Resources/Images/circle_blue16x16.png", DisplayName = "High", Value = ScanQuality.High });
            //SelectedScanQuality = _ScanQualityOptions[0];
        }

        public delegate void SelectedScanQualityChangedHandle(object sender, EventArgs e);
        public event SelectedScanQualityChangedHandle SelectedScanQualityChanged;

        public List<ScanQualityData> ScanQualityOptions
        {
            get { return _ScanQualityOptions; }
            set
            {
                _ScanQualityOptions = value;
                RaisePropertyChanged("ScanQualityOptions");
            }
        }

        public ScanQualityData SelectedScanQuality
        {
            get { return _SelectedScanQuality; }
            set
            {
                if (_SelectedScanQuality != value)
                {
                    _SelectedScanQuality = value;
                    if (this.SelectedScanQualityChanged != null)
                    {
                        SelectedScanQualityChanged(this,new EventArgs());
                    }
                    RaisePropertyChanged("SelectedScanQuality");
                }
            }
        }

    }

    public class ScanQualityData
    {
        public int Position { get; set; }
        public string Image { get; set; }
        public string DisplayName { get; set; }
        //public ScanQuality Value { get; set; }
    }

}
