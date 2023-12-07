using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Text.RegularExpressions;   // Regex
using System.ComponentModel;            // INotifyPropertyChanged
using Azure.LaserScanner.ViewModel;

namespace Azure.LaserScanner
{
    public enum ResizeByType
    {
        Percentage,
        Pixels
    }

    /// <summary>
    /// Interaction logic for ImageResizeWindow.xaml
    /// Binding using RelativeSource Self and DataContext
    /// </summary>
    public partial class ImageResizeWindow : Window, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Private data...

        private int _ImageWidth = 0;
        private int _ImageHeight = 0;
        private int _PixelWidth = 0;
        private int _PixelHeight = 0;
        private int _HorizontalPercentage = 100;
        private int _VerticalPercentage = 100;
        private ResizeByType _SelectedResizeBy = ResizeByType.Percentage;
        private bool _IsMaintainAspectRatio = true;
        private bool _IsRaiseTextChangedEvent = true;
        #endregion

        #region Public properties...

        public int PixelWidth
        {
            get { return _PixelWidth; }
            set
            {
                if (_PixelWidth != value)
                {
                    _PixelWidth = value;
                    OnPropertyChanged("PixelWidth");
                }
            }
        }

        public int PixelHeight
        {
            get { return _PixelHeight; }
            set
            {
                if (_PixelHeight != value)
                {
                    _PixelHeight = value;
                    OnPropertyChanged("PixelHeight");
                }
            }
        }

        public int HorizontalPercentage
        {
            get { return _HorizontalPercentage; }
            set
            {
                if (_HorizontalPercentage != value)
                {
                    _HorizontalPercentage = value;
                    OnPropertyChanged("HorizontalPercentage");
                }
            }
        }

        public int VerticalPercentage
        {
            get { return _VerticalPercentage; }
            set
            {
                if (_VerticalPercentage != value)
                {
                    _VerticalPercentage = value;
                    OnPropertyChanged("VerticalPercentage");
                }
            }
        }

        public ResizeByType SelectedResizeBy
        {
            get { return _SelectedResizeBy; }
            set
            {
                if (_SelectedResizeBy != value)
                {
                    _SelectedResizeBy = value;
                    if (_SelectedResizeBy == ResizeByType.Percentage)
                    {
                        //OnPropertyChanged("HorizontalPercentage");
                        //OnPropertyChanged("VerticalPercentage");
                        _HorizontalPercentage = 100;
                        _VerticalPercentage = 100;
                        _HorizontalTextBox.Text = _HorizontalPercentage.ToString();
                        _IsRaiseTextChangedEvent = false;
                        _VerticalTextBox.Text = _VerticalPercentage.ToString();
                        _IsRaiseTextChangedEvent = true;
                    }
                    else
                    {
                        //OnPropertyChanged("PixelWidth");
                        //OnPropertyChanged("PixelHeight");
                        _PixelWidth = _ImageWidth;
                        _PixelHeight = _ImageHeight;
                        _HorizontalTextBox.Text = _PixelWidth.ToString();
                        _IsRaiseTextChangedEvent = false;
                        _VerticalTextBox.Text = _PixelHeight.ToString();
                        _IsRaiseTextChangedEvent = true;
                    }

                    OnPropertyChanged("SelectedResizeBy");
                }
            }
        }

        public bool IsMaintainAspectRatio
        {
            get { return _IsMaintainAspectRatio; }
            set
            {
                if (_IsMaintainAspectRatio != value)
                {
                    _IsMaintainAspectRatio = value;
                    OnPropertyChanged("IsMaintainAspectRatio");
                }
            }
        }

        #endregion

        #region Constructors...

        public ImageResizeWindow()
        {
            InitializeComponent();
            DataContext = Workspace.This;
        }

        #endregion

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private static bool IsTextAllowed(string text)
        {
            Regex regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
            return !regex.IsMatch(text);
        }

        // If you want to prevent pasting of incorrect data hook up the DataObject.Pasting event
        // DataObject.Pasting="TextBoxPasting"
        // Use the DataObject.Pasting Handler 
        private void TextBoxPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));
                if (!IsTextAllowed(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void ImageResizeWindow_Loaded(object sender, RoutedEventArgs e)
        {
            FileViewModel activeDocument = Workspace.This.ActiveDocument;

            if (activeDocument == null) { return; }

            if (activeDocument.Image != null)
            {
                _ImageWidth = _PixelWidth = activeDocument.Image.PixelWidth;
                _ImageHeight = _PixelHeight = activeDocument.Image.PixelHeight;
            }

            if (_SelectedResizeBy == ResizeByType.Percentage)
            {
                _HorizontalTextBox.Text = _HorizontalPercentage.ToString();
                if (_IsMaintainAspectRatio)
                {
                    _VerticalPercentage = _HorizontalPercentage;
                    _IsRaiseTextChangedEvent = false;   // don't trigger textchanged event
                    _VerticalTextBox.Text = _VerticalPercentage.ToString();
                    _IsRaiseTextChangedEvent = true;
                }
            }
            else if (_SelectedResizeBy == ResizeByType.Pixels)
            {
                _HorizontalTextBox.Text = _PixelWidth.ToString();
                _IsRaiseTextChangedEvent = false;   // don't trigger textchanged event
                _VerticalTextBox.Text = _PixelHeight.ToString();
                _IsRaiseTextChangedEvent = true;
            }
        }

        private void HorizontalTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_HorizontalTextBox == null || string.IsNullOrEmpty(_HorizontalTextBox.Text.Trim()))
            {
                return;
            }

            if (_SelectedResizeBy == ResizeByType.Percentage)
            {
                _HorizontalPercentage = int.Parse(_HorizontalTextBox.Text.Trim());
                if (_IsRaiseTextChangedEvent && _IsMaintainAspectRatio)
                {
                    _VerticalPercentage = _HorizontalPercentage;
                    _IsRaiseTextChangedEvent = false;   // don't trigger textchanged event
                    _VerticalTextBox.Text = _VerticalPercentage.ToString();
                    _IsRaiseTextChangedEvent = true;
                }
            }
            else if (_SelectedResizeBy == ResizeByType.Pixels)
            {
                _PixelWidth = int.Parse(_HorizontalTextBox.Text.Trim());
                if (_IsRaiseTextChangedEvent && _IsMaintainAspectRatio)
                {
                    _PixelHeight = (int)(_PixelWidth * _ImageHeight / _ImageWidth);
                    _IsRaiseTextChangedEvent = false;   // don't trigger textchanged event
                    _VerticalTextBox.Text = _PixelHeight.ToString();
                    _IsRaiseTextChangedEvent = true;
                }
            }

        }

        private void VerticalTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_VerticalTextBox == null || string.IsNullOrEmpty(_VerticalTextBox.Text.Trim()))
            {
                return;
            }

            if (_SelectedResizeBy == ResizeByType.Percentage)
            {
                _VerticalPercentage = int.Parse(_VerticalTextBox.Text.Trim());
                if (_IsRaiseTextChangedEvent && _IsMaintainAspectRatio)
                {
                    _HorizontalPercentage = _VerticalPercentage;
                    _IsRaiseTextChangedEvent = false;   // don't trigger textchanged event
                    _HorizontalTextBox.Text = _HorizontalPercentage.ToString();
                    _IsRaiseTextChangedEvent = true;
                }
            }
            else if (_SelectedResizeBy == ResizeByType.Pixels)
            {
                _PixelHeight = int.Parse(_VerticalTextBox.Text.Trim());
                if (_IsRaiseTextChangedEvent && _IsMaintainAspectRatio)
                {
                    _PixelHeight = (int)(_PixelHeight * _ImageWidth / _ImageHeight);
                    _IsRaiseTextChangedEvent = false;   // don't trigger textchanged event
                    _HorizontalTextBox.Text = _PixelHeight.ToString();
                    _IsRaiseTextChangedEvent = true;
                }
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

    }

    public class PixelsPercentageConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType,
               object parameter, System.Globalization.CultureInfo culture)
        {
            // To get around the stupid WPF designer bug
            if (values != null)
            {
                int result = 0;

                // To get around the stupid WPF designer bug
                if (values[0] != null)
                {
                    if ((ResizeByType)values[0] == ResizeByType.Percentage)
                    {
                        result = Int32.Parse(values[1].ToString());
                        //result = (int)values[1];
                    }
                    else
                    {
                        result = Int32.Parse(values[2].ToString());
                        //result = (int)values[2];
                    }
                }

                return result.ToString();
            }

            return string.Empty;
        }

        public object[] ConvertBack(object value, Type[] targetTypes,
               object parameter, System.Globalization.CultureInfo culture)
        {
            //throw new NotSupportedException("Cannot convert back");

            //string[] splitValues = ((string)value).Split(' ');
            //return splitValues;

            return Array.ConvertAll<Type, Object>(targetTypes, t => value);
        }
    }

}
