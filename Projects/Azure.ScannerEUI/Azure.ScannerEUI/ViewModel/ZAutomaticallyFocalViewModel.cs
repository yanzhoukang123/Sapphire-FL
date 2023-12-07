using Azure.Configuration.Settings;
using Azure.ImagingSystem;
using Azure.WPF.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.ScannerEUI.ViewModel
{
    class ZAutomaticallyFocalViewModel : ViewModelBase
    {
        #region privert
        private int ScanZ0 = 0;
        private int _ZMaxValue = 0;
        private double _ZMotorSubdivision = 0;
        private bool _IsCreateGif = false; 
        private bool _IsWorkEnabled = false;
        private bool _IsFoucsEnabled = true;
        private string _SelectedFocus = null;   //Channel
        public double _TopImage = 0;
        public double _DeltaFocus = 0;
        private int _Ofimages = 0;
        private int _numberStr = 0;
        public List<FocusType> _FocusOptionsList = null;
 
        #endregion
        public ZAutomaticallyFocalViewModel()
        {
            FocusOptions = new List<string>();
            FocusOptions.AddRange(new string[] { "None", "Z-Stacking" });
            SelectedFocus = FocusOptions[0];
        }
        public void Initialize()
        {
            ScanZ0 = (int)Workspace.This.EthernetController.DeviceProperties.ZFocusPosition;
            _ZMaxValue = SettingsManager.ConfigSettings.ZMaxValue;
            _ZMotorSubdivision = SettingsManager.ConfigSettings.ZMotorSubdivision;
            _FocusOptionsList = new List<FocusType>();
            //_FocusOptions = SettingsManager.ConfigSettings.Focus;
        }

        #region  attribute
        public delegate void SetScanRegionEvent(string name);
        public event SetScanRegionEvent OnScanRegionReceived;
        public List<string> FocusOptions { get; set; }
        public int NumberStr { get => _numberStr; set => _numberStr = value; }
        public string SelectedFocus
        {
            get { return _SelectedFocus; }
            set
            {
                if (_SelectedFocus != value)
                {
                    _SelectedFocus = value;
                    if (value == "None")
                    {
                        IsWorkEnabled = false;
                        IsCreateGif = false;
                    }
                    else {
                        IsWorkEnabled = true;
                    }
                    RaisePropertyChanged(nameof(SelectedFocus));
                    OnScanRegionReceived?.Invoke("SelectedFocus");
                }
            }
        }

        public double TopImage
        {

            get {
                double dRetVal = 0;

                if (_ZMotorSubdivision != 0)
                {
                    dRetVal = Math.Round((double)_TopImage / (double)_ZMotorSubdivision, 3);
                }
                return dRetVal;
            }
            set
            {
                if ((double)_TopImage / (double)_ZMotorSubdivision != value)
                {
                    _FocusOptionsList.Clear();
                    if (value >= 0 && ((value + ScanZ0) <= ((double)_ZMaxValue / (double)_ZMotorSubdivision)))
                    {
                        _TopImage = (int)value;
                        if (value < DeltaFocus)
                        {
                            value = 0;
                            DeltaFocus = 0;
                            Workspace.This.ScannerVM.ScanDynamicScopeString = String.Format("DeltaFocus should be less than or equal to TopImage");
                            Workspace.This.ScannerVM.ScanDynamicScopeStringType = "Warning";
                            RaisePropertyChanged("TopImage");
                            OnScanRegionReceived?.Invoke("TopImage");
                            return;
                        }
                        _TopImage = (int)(value * _ZMotorSubdivision);
                        if (DeltaFocus != 0)
                        {
                            int _temp = (int)(value / DeltaFocus);
                            for (int i = 0; i < _temp; i++)
                            {
                                int j = i + 1;
                                FocusType ft = new FocusType();
                                ft.DisplayName = j.ToString();
                                ft.Value = ScanZ0 + (DeltaFocus * j);
                                ft.Position = j;
                                _FocusOptionsList.Add(ft);
                            }
                            //将玻璃表面的焦点放到列表第一位
                            //Put the focus on the glass surface to the first place in the list
                            FocusType ftZcan0 = new FocusType();
                            ftZcan0.DisplayName = "0";
                            ftZcan0.Value = ScanZ0;
                            ftZcan0.Position = 0;
                            _FocusOptionsList.Insert(0, ftZcan0);
                        }
                        else
                        {

                            if (value != 0)
                            {   //将玻璃表面的焦点放到列表第一位
                                //Put the focus on the glass surface to the first place in the list
                                FocusType ftZcan0 = new FocusType();
                                ftZcan0.DisplayName = "0";
                                ftZcan0.Value = ScanZ0 + value;
                                ftZcan0.Position = 0;
                                _FocusOptionsList.Insert(0, ftZcan0);
                            }
                            else {
                                Ofimages = 0;
                            }

                        }
                    }
                    else
                    {
                        Ofimages = 0;
                        TopImage = 0;
                        Workspace.This.ScannerVM.ScanDynamicScopeString = String.Format("The TopImage should be TopImage<={0}", ((double)(_ZMaxValue) / (double)_ZMotorSubdivision)- ScanZ0);
                        Workspace.This.ScannerVM.ScanDynamicScopeStringType = "Warning";
                    }
                    RaisePropertyChanged("TopImage");
                    OnScanRegionReceived?.Invoke("TopImage");
                }

            }
        }

        public double DeltaFocus
        {
            get 
            {

                return _DeltaFocus; 
            }
            set
            {
                if (_DeltaFocus != value)
                {
                    _FocusOptionsList.Clear();
                    if (value >= 0 && ((value + ScanZ0) <= ((double)_ZMaxValue / (double)_ZMotorSubdivision)))
                    {
                        _DeltaFocus = value;
                        if (value != 0 && TopImage != 0)
                        {
                            double tmpTopImage = Math.Round(TopImage, 1);
                            int _temp = (int)(Math.Round(tmpTopImage, 1) / value);
                            for (int i = 0; i < _temp; i++)
                            {
                                int j = i + 1;
                                FocusType ft = new FocusType();
                                ft.DisplayName = j.ToString();
                                ft.Value = ScanZ0 + (DeltaFocus * j);
                                ft.Position = j;
                                _FocusOptionsList.Add(ft);
                            }
                            //将玻璃表面的焦点放到列表第一位
                            //Put the focus on the glass surface to the first place in the list
                            FocusType ftZcan0 = new FocusType();
                            ftZcan0.DisplayName = "0";
                            ftZcan0.Value = ScanZ0;
                            ftZcan0.Position = 0;
                            _FocusOptionsList.Insert(0, ftZcan0);
                        }
                        else
                        {

                                Ofimages =0;
                        }       
                    }
                    else
                    {
                        DeltaFocus = 0;
                        Workspace.This.ScannerVM.ScanDynamicScopeString = String.Format("The DeltaFocus should be DeltaFocus<={0}", ((double)(_ZMaxValue) / (double)_ZMotorSubdivision) - ScanZ0);
                        Workspace.This.ScannerVM.ScanDynamicScopeStringType = "Warning";
                    }
                }
                RaisePropertyChanged("DeltaFocus");
                OnScanRegionReceived?.Invoke("DeltaFocus");
            }
        }

        public int Ofimages
        {
            get { return _Ofimages; }
            set
            {
                if (_Ofimages != value)
                {
                    _Ofimages = value;
                }

                RaisePropertyChanged("Ofimages");
                OnScanRegionReceived?.Invoke("Ofimages");
            }
        }
        public bool IsWorkEnabled
        {
            get { return _IsWorkEnabled; }
            set
            {
                _IsWorkEnabled = value;
                RaisePropertyChanged("IsWorkEnabled");
            }
        }
        public bool IsFoucsEnabled
        {
            get { return _IsFoucsEnabled; }
            set
            {
                _IsFoucsEnabled = value;
                RaisePropertyChanged("IsFoucsEnabled");
            }
        }
        #endregion

        #region CreateGif
        public bool IsCreateGif
        {
            get { return _IsCreateGif; }
            set
            {
                if (_IsCreateGif != value)
                {
                    _IsCreateGif = value;
                }
                RaisePropertyChanged("IsCreateGif");
                OnScanRegionReceived?.Invoke("IsCreateGif");

            }
        }
        #endregion
    }
}
