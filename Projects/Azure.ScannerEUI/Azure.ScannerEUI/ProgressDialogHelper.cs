using Azure.ScannerEUI.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;

namespace Azure.ScannerEUI
{
    public  class ProgressDialogHelper
    {
        public SplashScreenWindow _ProDialog;
        public Thread _Worker;

        public  void CloseProgressDialog()
        {
            _ProDialog.Dispatcher.BeginInvoke(new Action(() =>
            {
                _ProDialog.Visibility=Visibility.Hidden;
                Workspace .This.Owner.Visibility= Visibility.Visible;
            }));
        }

        public void WorkerThreadAbort()
        {
            if (_Worker.IsAlive)
                _Worker.Abort();
        }

        public void SetValueAndMessage(double value, string color, string mess)
        {
            _ProDialog.Dispatcher.BeginInvoke(new Action(() =>
            {
                _ProDialog.SetProgressValue(value);
                _ProDialog.SetMessage(color, mess);
            }));
        }

        public void SetValue(double value)
        {
            _ProDialog.Dispatcher.BeginInvoke(new Action(() =>
            {
                _ProDialog.SetProgressValue(value);
            }));
        }
        /// <summary>
        ///  Red,Black
        /// </summary>
        /// <param name="color"></param>
        /// <param name="mess"></param>
        public void SetMessage(string color, string mess)
        {
            _ProDialog.Dispatcher.BeginInvoke(new Action(() =>
            {
                _ProDialog.SetMessage(color, mess);
            }));
        }
        /// <summary>
        /// show Window
        /// </summary>
        /// <param name="workAction"></param>
        public void Show(Action workAction)
        {
            
            this._Worker = new Thread(new ThreadStart(workAction));
            this._Worker.IsBackground = true;
            this._ProDialog = new SplashScreenWindow();
            _Worker.Start();
            _ProDialog.ShowDialog();
        }
    }
}
