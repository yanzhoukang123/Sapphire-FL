using Azure.ScannerTestJig.View.Camera;
using Azure.WPF.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Azure.ScannerTestJig.ViewModule.Camera
{
    class CameraViewModule : ViewModelBase
    {
        public CameraPage CP = null;
        public Toupcam cam_ = null;
        private WriteableBitmap bmp_ = null;
        private bool started_ = false;
        internal void ExcuteCameraConnectCommand()
        {
            //if (cam_ != null)
            //    return;
            //Toupcam.GigeEnable(null);
            //Thread.Sleep(2000);
            //Toupcam.DeviceV2[] arr = Toupcam.EnumV2();
            //if (arr.Length <= 0)
            //    MessageBox.Show("No camera found.");
            if (CP == null)
            {
                CP = new CameraPage();
                CP.Title = Workspace.This.Owner.Title + "-Camera Kit";
            }
            CP.ShowDialog();
        }

    }
}
