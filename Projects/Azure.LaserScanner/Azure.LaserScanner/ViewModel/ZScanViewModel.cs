using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Azure.WPF.Framework;
using Azure.Configuration.Settings;

namespace Azure.LaserScanner.ViewModel
{
    [Serializable()]
    public class ZScanViewModel : ViewModelBase
    {
        private double _BottomImageFocus;
        private double _DeltaFocus;
        private int _NumOfImages = 2;
        //private bool _IsCreateAnimatedGif;

        public ZScanViewModel()
        {
        }

        public object Clone()
        {
            ZScanViewModel clone = (ZScanViewModel)this.MemberwiseClone();
            return clone;
        }

        public double BottomImageFocus
        {
            get { return _BottomImageFocus; }
            set
            {
                _BottomImageFocus = value;
                double focusPos = Workspace.This.AbsFocusPosition;
                if (SettingsManager.ConfigSettings.IsSimulationMode)
                        focusPos = 5.0;
                double focusMaxInMm = (double)SettingsManager.ConfigSettings.ZMaxValue / (double)SettingsManager.ConfigSettings.ZMotorSubdivision;
                double offset = 1;  // 1mm away from the min and max
                double nearDelta = focusPos - offset;
                double farDelta = focusPos - focusMaxInMm + offset;
                if (_BottomImageFocus < 0)
                {
                    if (_BottomImageFocus < farDelta)
                    {
                        _BottomImageFocus = farDelta;
                    }
                }
                else
                {
                    if (_BottomImageFocus > nearDelta)
                    {
                        _BottomImageFocus = nearDelta;
                    }
                }
                _BottomImageFocus = Math.Round(_BottomImageFocus, 2);
                RaisePropertyChanged("BottomImageFocus");
                // Trigger number of images update
                RaisePropertyChanged("NumOfImages");
            }
        }
        public double DeltaFocus
        {
            get { return _DeltaFocus; }
            set
            {
                _DeltaFocus = value;
                RaisePropertyChanged("DeltaFocus");
                // Trigger number of images update
                RaisePropertyChanged("NumOfImages");
            }
        }
        public int NumOfImages
        {
            get
            {
                if (_NumOfImages > 0)
                {
                    double focusPos = Workspace.This.AbsFocusPosition;

                    if (SettingsManager.ConfigSettings.IsSimulationMode)
                        focusPos = 5.0;

                    if (focusPos > 0)
                    {
                        double focusMaxInMm = (double)SettingsManager.ConfigSettings.ZMaxValue / (double)SettingsManager.ConfigSettings.ZMotorSubdivision;
                        double offset = 1;  // 1mm away from the min and max
                        double nearDelta = focusPos - offset;
                        var maxFocus = _BottomImageFocus + (_DeltaFocus * (_NumOfImages - 1));
                        if (maxFocus > nearDelta)
                        {
                            _NumOfImages = (int)((nearDelta - _BottomImageFocus) / _DeltaFocus) + 1;
                            string caption = "ZCan Setting...";
                            string message = "The focus range is greater than the maximum focus allowed.";
                            Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                    else
                    {
                        _NumOfImages = 0;
                    }
                }
                return _NumOfImages;
            }
            set
            {
                if (_DeltaFocus <= 0)
                {
                    string caption = "ZCan Setting...";
                    string message = "The delta value must be greater 0.";
                    Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                _NumOfImages = value;
                RaisePropertyChanged("NumOfImages");
            }
        }
        //public bool IsCreateAnimatedGif
        //{
        //    get { return _IsCreateAnimatedGif; }
        //    set
        //    {
        //        _IsCreateAnimatedGif = value;
        //        RaisePropertyChanged("IsCreateAnimatedGif");
        //    }
        //}
    }
}
