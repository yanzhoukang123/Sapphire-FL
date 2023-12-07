using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace Azure.LaserScanner
{
    /// <summary>
    /// Interaction logic for DeleteProtocol.xaml
    /// </summary>
    public partial class DeleteProtocol : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Private data...

        private string _Message = string.Empty;
        private string _ProtocolName = string.Empty;
        private bool _IsCancelButtonVisibled = true;

        #endregion

        #region Public properties...

        public string Message
        {
            get { return _Message; }
            set
            {
                _Message = value;
                OnPropertyChanged("Message");
            }
        }

        public string ProtocolName
        {
            get { return _ProtocolName; }
            set
            {
                _ProtocolName = value;
                OnPropertyChanged(nameof(ProtocolName));
            }
        }

        public bool IsCancelButtonVisibled
        {
            get { return _IsCancelButtonVisibled; }
            set
            {
                _IsCancelButtonVisibled = value;
                OnPropertyChanged(nameof(IsCancelButtonVisibled));
            }
        }

        #endregion

        public DeleteProtocol()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Set protocol name
        /// </summary>
        //private void Window_Loaded(object sender, RoutedEventArgs e)
        //{
        //    this.txtProtocolName.Text = this.ProtocolName.Trim();
        //}

        /// <summary>
        /// Close dialog box
        /// </summary>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        /// <summary>
        /// Delete protocol
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
