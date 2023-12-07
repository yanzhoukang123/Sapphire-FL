using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Azure.WPF.Framework;
using Azure.Configuration.Settings;

namespace Azure.LaserScanner.ViewModel
{
    [Serializable()]
    public class ZScanSetting : ViewModelBase
    {
        private double _AbsFocus = 0;
        private double _MinFocus = 0;
        private double _MaxFocus = 0;
        private double _BottomImageFocus;
        private double _DeltaFocus = 1;
        private int _NumOfImages = 2;

        public ZScanSetting()
        {
        }

        public ZScanSetting(double absFocus, double minFocus, double maxFocus)
        {
            AbsFocus = absFocus;
            MinFocus = minFocus;
            MaxFocus = maxFocus;
        }

        public object Clone()
        {
            ZScanSetting clone = (ZScanSetting)this.MemberwiseClone();
            return clone;
        }

        public double AbsFocus
        {
            get { return _AbsFocus; }
            set { _AbsFocus = value; }
        }
        public double MinFocus
        {
            get { return _MinFocus; }
            set { _MinFocus = value; }
        }
        public double MaxFocus
        {
            get { return _MaxFocus; }
            set { _MaxFocus = value; }
        }

        public double BottomImageFocus
        {
            get { return _BottomImageFocus; }
            set
            {
                _BottomImageFocus = value;
                if (SettingsManager.ConfigSettings.IsSimulationMode)
                {
                    _AbsFocus = 1.0; //Sapphire FL focus position is around 1
                }
                if (_BottomImageFocus < MinFocus)
                {
                    _BottomImageFocus = MinFocus;
                }
                else
                {
                    if (_BottomImageFocus > MaxFocus)
                    {
                        _BottomImageFocus = MaxFocus;
                        _DeltaFocus = 0;
                        _NumOfImages = 1;
                    }
                }
                _BottomImageFocus = Math.Round(_BottomImageFocus, 2);
                RaisePropertyChanged("BottomImageFocus");
                // Trigger number of images update
                RaisePropertyChanged("DeltaFocus");
                RaisePropertyChanged("NumOfImages");
            }
        }
        public double DeltaFocus
        {
            get { return _DeltaFocus; }
            set
            {
                _DeltaFocus = value;
                if (_DeltaFocus < 0.01)
                {
                    _DeltaFocus = 0.01;
                    string caption = "Z Scan Settings";
                    string message = "The mininum focus delta is 0.01 mm.";
                    Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Information);
                }
                if ((_BottomImageFocus + _DeltaFocus) > MaxFocus)
                {
                    _DeltaFocus = MaxFocus - BottomImageFocus;
                    _NumOfImages = 1;
                }
                _DeltaFocus = Math.Round(_DeltaFocus, 2);
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
                    if (SettingsManager.ConfigSettings.IsSimulationMode)
                    {
                        AbsFocus = 1.0; //Sapphire FL focus position is around 1
                    }

                    if (AbsFocus > 0)
                    {
                        var maxFocus = _BottomImageFocus + (_DeltaFocus * _NumOfImages);
                        if (maxFocus > MaxFocus)
                        {
                            string caption = "ZCan Setting...";
                            string message = "The focus range is greater than the maximum focus allowed.";
                            Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                            _NumOfImages = (int)((MaxFocus - _BottomImageFocus) / _DeltaFocus);
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

        private RelayCommand _UpdateTextBoxBindingOnEnterCommand = null;
        public ICommand UpdateTextBoxBindingOnEnterCommand
        {
            get
            {
                if (_UpdateTextBoxBindingOnEnterCommand == null)
                {
                    _UpdateTextBoxBindingOnEnterCommand = new RelayCommand(ExecuteUpdateTextBoxBindingOnEnterCommand, CanExecuteUpdateTextBoxBindingOnEnterCommand);
                }

                return _UpdateTextBoxBindingOnEnterCommand;
            }
        }
        protected void ExecuteUpdateTextBoxBindingOnEnterCommand(object parameter)
        {
            TextBox tBox = parameter as TextBox;
            if (tBox != null)
            {
                DependencyProperty prop = TextBox.TextProperty;
                System.Windows.Data.BindingExpression binding = System.Windows.Data.BindingOperations.GetBindingExpression(tBox, prop);
                if (binding != null)
                    binding.UpdateSource();
            }
        }
        protected bool CanExecuteUpdateTextBoxBindingOnEnterCommand(object parameter)
        {
            return true;
        }
    }
}
