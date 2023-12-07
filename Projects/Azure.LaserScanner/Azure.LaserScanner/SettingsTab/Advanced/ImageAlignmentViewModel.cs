using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Input; //ICommand
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Azure.Configuration.Settings;
using Azure.EthernetCommLib;
using Azure.Image.Processing;
using Azure.LaserScanner.View;
using Azure.LaserScanner.ViewModel;
using Azure.WPF.Framework;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;

namespace Azure.LaserScanner
{
    class ImageAlignmentViewModel : ViewModelBase
    {
        #region Private field/data...

        // Calibration scan must be done in 10 micron
        private int _Pixel_10_L_DX;
        private int _Pixel_10_L_DY;
        private int _Pixel_10_R2_DX;
        private int _Pixel_10_R2_DY;

        private RelayCommand _ClearAlignmentParamCommand = null;
        private RelayCommand _GetAlignmentParamCommand = null;
        private RelayCommand _AlignmentParamWriteCommand = null;

        private bool _IsWritingParameters = false;

        #endregion

        #region Construcors...

        public ImageAlignmentViewModel()
        {
        }

        #endregion

        /// <summary>
        /// Occurs when a close call should be performed.
        /// </summary>
        public event EventHandler RequestClose;
        /// <summary>
        /// Raises the <see cref="E:RequestClose"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void OnRequestClose(EventArgs e)
        {
            if (RequestClose != null)
            {
                RequestClose(this, e);
            }
        }
        /// <summary>
        /// Raises the <see cref="E:RequestClose"/> event.
        /// </summary>
        protected void RaiseRequestCloseEvent()
        {
            OnRequestClose(EventArgs.Empty);
        }


        #region Public properties...

        public int Pixel_10_L_DX
        {
            get { return _Pixel_10_L_DX; }
            set
            {
                if (_Pixel_10_L_DX != value)
                {
                    _Pixel_10_L_DX = value;
                    RaisePropertyChanged("Pixel_10_L_DX");
                }
            }
        }

        public int Pixel_10_L_DY
        {
            get { return _Pixel_10_L_DY; }
            set
            {
                if (_Pixel_10_L_DY != value)
                {
                    _Pixel_10_L_DY = value;
                    RaisePropertyChanged("Pixel_10_L_DY");
                }
            }
        }

        public int Pixel_10_R2_DX
        {
            get { return _Pixel_10_R2_DX; }
            set
            {
                if (_Pixel_10_R2_DX != value)
                {
                    _Pixel_10_R2_DX = value;
                    RaisePropertyChanged("Pixel_10_R2_DX");
                }
            }
        }

        public int Pixel_10_R2_DY
        {
            get { return _Pixel_10_R2_DY; }
            set
            {
                if (_Pixel_10_R2_DY != value)
                {
                    _Pixel_10_R2_DY = value;
                    RaisePropertyChanged("Pixel_10_R2_DY");
                }
            }
        }

        #endregion


        #region public ICommand ClearAlignmentParamCommand
        public ICommand ClearAlignmentParamCommand
        {
            get
            {
                if (_ClearAlignmentParamCommand == null)
                {
                    _ClearAlignmentParamCommand = new RelayCommand(ExecuteClearAlignmentParamCommand, CanExecuteClearAlignmentParamCommand);
                }

                return _ClearAlignmentParamCommand;
            }
        }
        public void ExecuteClearAlignmentParamCommand(object parameter)
        {
            Pixel_10_L_DX = 0;
            Pixel_10_L_DY = 0;
            Pixel_10_R2_DX = 0;
            Pixel_10_R2_DY = 0;
        }
        public bool CanExecuteClearAlignmentParamCommand(object parameter)
        {
            return true;
        }
        #endregion

        #region public ICommand GetAlignmentParamCommand
        public ICommand GetAlignmentParamCommand
        {
            get
            {
                if (_GetAlignmentParamCommand == null)
                {
                    _GetAlignmentParamCommand = new RelayCommand(ExecuteGetAlignmentParamCommand, CanExecuteGetAlignmentParamCommand);
                }

                return _GetAlignmentParamCommand;
            }
        }
        public void ExecuteGetAlignmentParamCommand(object parameter)
        {
            if (!Workspace.This.IsActiveDocument)
            {
                string caption = "Get image alignment parameters...";
                string message = "Please make sure there's a 10 micron scanned image in Gallery.";
                var parent = Application.Current.MainWindow;
                Xceed.Wpf.Toolkit.MessageBox.Show(parent, message, caption, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            WriteableBitmap activeImage = Workspace.This.ActiveDocument.Image;
            int nImageChannels = Workspace.This.ActiveDocument.ImageInfo.NumOfChannels;
            if (activeImage.Format != PixelFormats.Rgb48)
            {
                string caption = "Get image alignment parameters...";
                string message = "This operation requires the active image in Gallery to be a 48-bit image.";
                var parent = Application.Current.MainWindow;
                Xceed.Wpf.Toolkit.MessageBox.Show(parent, message, caption, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            string[] laserModules;
            if (!string.IsNullOrEmpty(Workspace.This.ActiveDocument.LaserChannel))
            {
                laserModules = Workspace.This.ActiveDocument.LaserChannel.Split('/');
            }
            else
            {
                string caption = "Get image alignment parameters";
                string message = "Could find laser modules info in the image info";
                var parent = Application.Current.MainWindow;
                Xceed.Wpf.Toolkit.MessageBox.Show(parent, message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

            int lcIndexL1 = -1, lcIndexR1 = -1, lcIndexR2 = -1;
            if (laserModules != null)
            {
                string searchLaser = "L";
                lcIndexL1 = Array.FindIndex(laserModules, x => x.Contains(searchLaser));
                searchLaser = "R1";
                lcIndexR1 = Array.FindIndex(laserModules, x => x.Contains(searchLaser));
                searchLaser = "R2";
                lcIndexR2 = Array.FindIndex(laserModules, x => x.Contains(searchLaser));
            }
            else
            {
                string caption = "Get image alignment parameters";
                string message = "Could find laser modules info in the image info";
                var parent = Application.Current.MainWindow;
                Xceed.Wpf.Toolkit.MessageBox.Show(parent, message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

            // R2 is our reference channel.
            // Make sure we have a reference image in R2
            if (lcIndexR2 >= 0 || lcIndexR1 >= 0)
            {
                // Either R1 or R2 has to exists
                // We're now allowing R1 or R2 to be the reference channel
            }
            else
            {
                string caption = "Get image alignment parameters";
                string message = "Could find the reference image." +
                    "\nWe're using the image from Port #3 (R2) or Port #2 (R1) as the reference image." +
                    "\nPlease make sure the scanned image includes the image from a laser module in Port #3 or Port #2.";
                var parent = Application.Current.MainWindow;
                Xceed.Wpf.Toolkit.MessageBox.Show(parent, message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

            int refChanIndex = lcIndexR2;
            // Now allowing R1 as the reference channel; no longer assuming R2 is always the reference channel
            if (lcIndexR2 >= 0)
            {
                refChanIndex = lcIndexR2;   // set R2 as the reference channel
            }
            else if (lcIndexR1 >= 0)
            {
                refChanIndex = lcIndexR1;   // set R1 as the reference channel
            }

            int[] laserChannels = new int[] { lcIndexL1, lcIndexR1, lcIndexR2 };
            WriteableBitmap alignedImage = null;
            float[] matrixL1 = null;
            float[] matrixR1 = null;
            if (nImageChannels > 1)
            {
                try
                {
                    Workspace.This.StartWaitAnimation("Calculating alignment parameters.\nPlease wait...");

                    alignedImage = ImageProcessing.GetAlignment(activeImage, laserChannels, refChanIndex, ref matrixL1, ref matrixR1);

                    // Get the 3rd and 6th value in the 2x3 matrix
                    int ldx = 0, ldy = 0, rdx = 0, rdy = 0;
                    if (matrixL1 != null)
                    {
                        ldx = (int)matrixL1[2];
                        ldy = (int)matrixL1[5];
                        //ldx = (int)Math.Round(matrixL1[2]);
                        //ldy = (int)Math.Round(matrixL1[5]);
                    }
                    if (matrixR1 != null)
                    {
                        rdx = (int)matrixR1[2];
                        rdy = (int)matrixR1[5];
                        //rdx = (int)Math.Round(matrixR1[2]);
                        //rdy = (int)Math.Round(matrixR1[5]);
                    }

                    // Reset the alignment parameters entries.
                    Pixel_10_L_DX = 0;
                    Pixel_10_L_DY = 0;
                    Pixel_10_R2_DX = 0;
                    Pixel_10_R2_DY = 0;

                    if (alignedImage != null && SettingsManager.ConfigSettings.IsSendAutoAlignedImageToGallery)
                    {
                        string title = Workspace.This.GetUniqueFilename("Auto-Aligned-Image");
                        Workspace.This.NewDocument(alignedImage, (ImageInfo)Workspace.This.ActiveDocument.ImageInfo.Clone(), title, false, false);
                    }

                    if (ldx == 0 && ldy == 0 && rdx == 0 && rdy == 0)
                    {
                        Workspace.This.StopWaitAnimation();
                        string caption = "Get alignment parameters...";
                        string message = "The laser modules appeared to be already aligned.";
                        var parent = Application.Current.MainWindow;
                        Xceed.Wpf.Toolkit.MessageBox.Show(parent, message, caption, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                        return;
                    }

                    // reverse sign of auto-align value
                    Pixel_10_L_DX = ldx * (-1);
                    Pixel_10_L_DY = ldy * (-1);
                    Pixel_10_R2_DX = rdx * (-1);
                    Pixel_10_R2_DY = rdy * (-1);
                    // This reverse sign below is not needed here; the 'write' command will reverse the sign
                    //
                    // reverse sign (the scanned image is automatically flipped vertically by default)
                    //if (!SettingsManager.ApplicationSettings.IsVerticalFlipEnabled)
                    //{
                    //    Pixel_10_L_DY *= -1;
                    //    Pixel_10_R2_DY *= -1;
                    //}

                    //System.Diagnostics.Trace.WriteLine("[{0}]", string.Join(", ", matrixL1));
                    //System.Diagnostics.Trace.WriteLine("[{0}]", string.Join(", ", matrixR1));

                    Workspace.This.StopWaitAnimation();
                }
                catch (Exception ex)
                {
                    Workspace.This.StopWaitAnimation();
                    string caption = "Get image alignment parameters";
                    string message = "Error calculating alignment parameters.\n" + ex.Message;
                    var parent = Application.Current.MainWindow;
                    Xceed.Wpf.Toolkit.MessageBox.Show(parent, message, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                }
            }
        }
        public bool CanExecuteGetAlignmentParamCommand(object parameter)
        {
            return (Workspace.This.IsActiveDocument);
            //return true;
        }

        #endregion

        #region public ICommand AlignmentParamWriteCommand

        public ICommand AlignmentParamWriteCommand
        {
            get
            {
                if (_AlignmentParamWriteCommand == null)
                {
                    _AlignmentParamWriteCommand = new RelayCommand(ExecuteAlignmentParamWriteCommand, CanExecuteAlignmentParamWriteCommand);
                }

                return _AlignmentParamWriteCommand;
            }
        }
        public void ExecuteAlignmentParamWriteCommand(object parameter)
        {
            if (_IsWritingParameters) { return; }

            if (_Pixel_10_L_DX == 0 && _Pixel_10_L_DY == 0 &&
                _Pixel_10_R2_DX == 0 && _Pixel_10_R2_DY == 0)
            {
                return;
            }

            if (Workspace.This.EthernetController.IsConnected)
            {
                if (Workspace.This.EthernetController.GetDeviceProperties() == false)
                {
                    string caption = "Image alignment parameters...";
                    string message = "Unable to read the system setting parameters.";
                    Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }

                /*AvocadoDeviceProperties deviceProperties;
                var ptr = Marshal.AllocHGlobal(256);
                deviceProperties = (AvocadoDeviceProperties)Marshal.PtrToStructure(ptr, typeof(AvocadoDeviceProperties));
                Marshal.FreeHGlobal(ptr);

                deviceProperties.LogicalHomeX = Workspace.This.EthernetController.DeviceProperties.LogicalHomeX;
                deviceProperties.LogicalHomeY = Workspace.This.EthernetController.DeviceProperties.LogicalHomeY;
                deviceProperties.OpticalLR1Distance = Workspace.This.EthernetController.DeviceProperties.OpticalLR1Distance;
                deviceProperties.PixelOffsetR1 = Workspace.This.EthernetController.DeviceProperties.PixelOffsetR1;
                deviceProperties.OpticalLR2Distance = Workspace.This.EthernetController.DeviceProperties.OpticalLR2Distance;
                deviceProperties.PixelOffsetR2 = Workspace.This.EthernetController.DeviceProperties.PixelOffsetR2;
                deviceProperties.PixelOffsetDxCHR1 = Workspace.This.EthernetController.DeviceProperties.PixelOffsetDxCHR1;
                deviceProperties.PixelOffsetDyCHR1 = Workspace.This.EthernetController.DeviceProperties.PixelOffsetDyCHR1;
                deviceProperties.PixelOffsetDxCHR2 = Workspace.This.EthernetController.DeviceProperties.PixelOffsetDxCHR2;
                deviceProperties.PixelOffsetDyCHR2 = Workspace.This.EthernetController.DeviceProperties.PixelOffsetDyCHR2;
                deviceProperties.ZFocusPosition = Workspace.This.EthernetController.DeviceProperties.ZFocusPosition;
                deviceProperties.SysSN = Workspace.This.EthernetController.DeviceProperties.SysSN;
                deviceProperties.PixelOffsetDxCHL = Workspace.This.EthernetController.DeviceProperties.PixelOffsetDxCHL;
                deviceProperties.PixelOffsetDyCHL = Workspace.This.EthernetController.DeviceProperties.PixelOffsetDyCHL;
                deviceProperties.XEncoderSubdivision = Workspace.This.EthernetController.DeviceProperties.XEncoderSubdivision;
                deviceProperties.FanSwitchInterval = Workspace.This.EthernetController.DeviceProperties.FanSwitchInterval;
                deviceProperties.FanReserveTemperature = Workspace.This.EthernetController.DeviceProperties.FanReserveTemperature;*/

                var deviceProperties = Workspace.This.EthernetController.DeviceProperties;

                // Add value entered to the existing value
                //deviceProperties.PixelOffsetDxCHL +=  (2 * Pixel_20_L_DX);
                //deviceProperties.PixelOffsetDyCHL += (2 * Pixel_20_L_DY);
                //deviceProperties.PixelOffsetDxCHR2 += (2 * Pixel_20_R2_DX);
                //deviceProperties.PixelOffsetDyCHR2 += (2 * Pixel_20_R2_DY);

                // The scanned image is automatically flipped vertically by default
                // If this flag in Settings tab is not set, reverse the sign
                if (!SettingsManager.ApplicationSettings.IsVerticalFlipEnabled)
                {
                    _Pixel_10_L_DY *= -1;
                    _Pixel_10_R2_DY *= -1;
                }

                //Now expecting the alignment scan to be scanned at 10um (per Wei).
                deviceProperties.PixelOffsetDxCHL += _Pixel_10_L_DX;
                deviceProperties.PixelOffsetDyCHL += _Pixel_10_L_DY;
                deviceProperties.PixelOffsetDxCHR2 += _Pixel_10_R2_DX;
                deviceProperties.PixelOffsetDyCHR2 += _Pixel_10_R2_DY;

                int tempPixel10LDx = deviceProperties.PixelOffsetDxCHL;
                int tempPixel10LDy = deviceProperties.PixelOffsetDyCHL;
                int tempPixel10R2Dx = deviceProperties.PixelOffsetDxCHR2;
                int tempPixel10R2Dy = deviceProperties.PixelOffsetDyCHR2;

                try
                {
                    Workspace.This.StartWaitAnimation("Saving image alignment parameters...");
                    _IsWritingParameters = true;
                    if (Workspace.This.EthernetController.SetDeviceProperties(deviceProperties) == false)
                    {
                        Workspace.This.StopWaitAnimation();

                        string caption = "Image alignment parameters...";
                        string message = "Unable to write the alignment parameters.";
                        Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    }
                    else
                    {
                        // Wait 3 seconds before reading back the written values.
                        ImagingSystem.ImagingHelper.Delay(3000);

                        // Read back and verify the saved parameters.
                        if (Workspace.This.EthernetController.GetDeviceProperties() == false)
                        {
                            Workspace.This.StopWaitAnimation();

                            string caption = "Image alignment parameters...";
                            string message = "Unable to read back and verify the image alignment parameters.";
                            Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                            return;
                        }
                        else
                        {
                            Workspace.This.StopWaitAnimation();

                            // Verify the saved parameters
                            if (tempPixel10LDx == Workspace.This.EthernetController.DeviceProperties.PixelOffsetDxCHL &&
                                tempPixel10LDy == Workspace.This.EthernetController.DeviceProperties.PixelOffsetDyCHL &&
                                tempPixel10R2Dx == Workspace.This.EthernetController.DeviceProperties.PixelOffsetDxCHR2 &&
                                tempPixel10R2Dy == Workspace.This.EthernetController.DeviceProperties.PixelOffsetDyCHR2)
                            {
                                // Verified: parameters successfully saved
                                string caption = "Image alignment parameters...";
                                string message = "Successfully written the alignment parameters.";
                                Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                            }
                            else
                            {
                                string caption = "Image alignment parameters...";
                                string message = "Unable to read back and verify the alignment parameters.";
                                Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                            }
                        }
                    }
                }
                finally
                {
                    _IsWritingParameters = false;
                    Workspace.This.StopWaitAnimation();
                }
            }
            else
            {
                string caption = "Image alignment Settings...";
                string message = "The scanner is not connected.";
                Xceed.Wpf.Toolkit.MessageBox.Show(message, caption, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);

            }
        }
        public bool CanExecuteAlignmentParamWriteCommand(object parameter)
        {
            return (!_IsWritingParameters);
        }

        #endregion

        /*private WriteableBitmap GetAlignment(WriteableBitmap srcimg, int[] laserChannels, ref float[] matrixL1, ref float[] matrixR1)
        {
            int width = srcimg.PixelWidth;
            int height = srcimg.PixelHeight;
            var wbImgChannels = ImageProcessing.GetChannel(srcimg);
            // Convert WriteableBitmap to OpenCvSharp.Mat
            OpenCvSharp.Mat[] matChannels = new OpenCvSharp.Mat[wbImgChannels.Length];
            OpenCvSharp.Mat[] matAlignedChannels = new OpenCvSharp.Mat[wbImgChannels.Length];

            // Convert WriteableBitmap to OpenCvSharp.Mat
            // Convert 16U image to 32F image
            //for (int i = 0; i < matChannels.Length; i++)
            //{
            //    matChannels[i] = new Mat(height, width, MatType.CV_32FC1);
            //    matAlignedChannels[i] = new Mat(height, width, MatType.CV_32FC1);
            //    var mat = wbImgChannels[i].ToMat();
            //    mat.ConvertTo(matChannels[i], MatType.CV_32FC1);
            //    matAlignedChannels[i] = matChannels[i].Clone();
            //}

            // Convert WriteableBitmap to OpenCvSharp.Mat
            for (int i = 0; i < matChannels.Length; i++)
            {
                matChannels[i] = wbImgChannels[i].ToMat();
                matAlignedChannels[i] = matChannels[i].Clone();
                //matAlignedChannels[i] = new Mat(height, width, MatType.CV_16UC1);
            }

            // Define motion model
            OpenCvSharp.MotionTypes warp_mode = OpenCvSharp.MotionTypes.Translation;

            // Set a 2x3 or 3x3 warp matrix depending on the motion model.
            Mat warp_matrix;
            if (warp_mode == OpenCvSharp.MotionTypes.Homography)
            {
                warp_matrix = Mat.Eye(3, 3, MatType.CV_32F);
            }
            else
            {
                warp_matrix = Mat.Eye(2, 3, MatType.CV_32F);
            }

            // Specify the number of iterations.
            int number_of_iterations = 5000;

            // Specify the threshold of the increment
            // in the correlation coefficient between two iterations
            double termination_eps = 1e-10;

            // Define termination criteria
            TermCriteria criteria = new TermCriteria(CriteriaTypes.Count | CriteriaTypes.Eps, number_of_iterations, termination_eps);

            //float[] dataR1;
            //float[] dataL1;
            int lcIndexL1 = laserChannels[0];
            int lcIndexR1 = laserChannels[1];
            int lcIndexR2 = laserChannels[2];   // Reference channel
            double cc;
            for (int i = 0; i < matChannels.Length; i++)
            {
                if (i != lcIndexR2)
                {
                    if (i == lcIndexL1 || i == lcIndexR1)
                    {
                        cc = OpenCvSharp.Cv2.FindTransformECC(
                            GetGradient(matChannels[lcIndexR2]),
                            GetGradient(matChannels[i]),
                            warp_matrix,
                            warp_mode,
                            criteria);

                        float[] data = new float[warp_matrix.Width * warp_matrix.Height];
                        Marshal.Copy(warp_matrix.Data, data, 0, warp_matrix.Width * warp_matrix.Height);

                        if ((int)data[2] != 0 || (int)data[5] != 0)
                        {
                            if (warp_mode == OpenCvSharp.MotionTypes.Homography)
                            {
                                // Use Perspective warp when the transformation is a Homography
                                OpenCvSharp.Cv2.WarpPerspective(matChannels[i], matAlignedChannels[i], warp_matrix, matChannels[0].Size(), InterpolationFlags.Linear | InterpolationFlags.WarpInverseMap);
                            }
                            else
                            {
                                // Use Affine warp when the transformation is not a Homography
                                OpenCvSharp.Cv2.WarpAffine(matChannels[i], matAlignedChannels[i], warp_matrix, matAlignedChannels[0].Size(), InterpolationFlags.Linear | InterpolationFlags.WarpInverseMap);
                            }
                        }

                        //float[] data = new float[warp_matrix.Width * warp_matrix.Height];
                        //Marshal.Copy(warp_matrix.Data, data, 0, warp_matrix.Width * warp_matrix.Height);
                        if (i == lcIndexR1)
                        {
                            matrixR1 = data;
                            //System.Diagnostics.Trace.WriteLine("[{0}]", string.Join(", ", matrixR1));
                        }
                        else if (i == lcIndexL1)
                        {
                            matrixL1 = data;
                            //System.Diagnostics.Trace.WriteLine("[{0}]", string.Join(", ", matrixL1));
                        }
                    }
                }
            }

            // Calculated image
            if (lcIndexR2 >= 0 && (lcIndexL1 >= 0 || lcIndexR1 >= 0))
            {
                for (int i = 0; i < matChannels.Length; i++)
                {
                    //Convert CV_32FC1 to CV_16UC1
                    //var mat16U = new Mat(height, width, MatType.CV_16UC1);
                    //matAlignedChannels[i].ConvertTo(mat16U, MatType.CV_16UC1);
                    //matAlignedChannels[i] = mat16U;

                    wbImgChannels[i] = matAlignedChannels[i].ToWriteableBitmap();
                }
            }

            WriteableBitmap alignedImage = ImageProcessing.SetChannel(wbImgChannels);
            return alignedImage;
        }*/
        /*private Mat GetGradient(Mat src_gray)
        {
            int rows = src_gray.Rows;
            int cols = src_gray.Cols;
            Mat grad_x = new Mat(rows, cols, MatType.CV_16UC1);
            Mat grad_y = new Mat(rows, cols, MatType.CV_16UC1);
            Mat abs_grad_x = new Mat(rows, cols, MatType.CV_16UC1);
            Mat abs_grad_y = new Mat(rows, cols, MatType.CV_16UC1);
            int scale = 1;
            int delta = 0;
            int ddepth = MatType.CV_16UC1;
            //int ddepth = MatType.CV_32FC1;

            // Calculate the x and y gradients using Sobel operator
            OpenCvSharp.Cv2.Sobel(src_gray, grad_x, ddepth, 1, 0, 3, scale, delta, BorderTypes.Default);
            OpenCvSharp.Cv2.ConvertScaleAbs(grad_x, abs_grad_x);

            OpenCvSharp.Cv2.Sobel(src_gray, grad_y, ddepth, 0, 1, 3, scale, delta, BorderTypes.Default);
            OpenCvSharp.Cv2.ConvertScaleAbs(grad_y, abs_grad_y);
            // Combine the two gradients
            Mat grad = new Mat(rows, cols, MatType.CV_16UC1);
            //Mat grad = new Mat(rows, cols, MatType.CV_32FC1);
            OpenCvSharp.Cv2.AddWeighted(abs_grad_x, 0.5, abs_grad_y, 0.5, 0, grad);

            return grad;
        }*/
    }
}
