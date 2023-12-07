using Azure.Image.Processing;
using Azure.ScannerEUI.ViewModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Azure.ScannerEUI.View
{
    /// <summary>
    /// ImageRotatingProcess.xaml 的交互逻辑
    /// </summary>
    public partial class ImageRotatingProcess : System.Windows.Window
    {
        private const int _SaturationThreshold = 62000;
        int singe = 0;
        public ImageRotatingProcess()
        {
            InitializeComponent();
           
            this.Loaded += new RoutedEventHandler(MotorControl_Loaded);
        }
        private void MotorControl_Loaded(object sender, RoutedEventArgs e)
        {
          
        }
        private void _ContrastBlackTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (singe == 1)
            {
                if (!string.IsNullOrEmpty(_ContrastBlackTextBox.Text) && _ContrastBlackTextBox.Text != "0")
                {
                    int Angle = 0;
                    if (int.TryParse(_ContrastBlackTextBox.Text, out Angle))
                    {
                        if (_Module1.Text != null)
                        {
                            if (_Module1.Text== "Detail")
                            {
                                img.Source = RotatProcess.Rotat.rotate(0 - Angle);
                            }
                            else
                            {
                                img.Source = RotatProcess.Rotat.rotateScrap(0 - Angle);
                            }
                        }
                    }
                    else
                    {
                        _ContrastBlackTextBox.Text = "0";
                    }
                }
                else
                {
                    img.Source = RotatProcess.Rotat.rotate(0);
                }
            }
        }

        private void _CloseButton_Click(object sender, RoutedEventArgs e)
        {
            //if (!string.IsNullOrEmpty(_ContrastBlackTextBox.Text) && _ContrastBlackTextBox.Text != "0")
            //{
            //    MessageBoxResult boxResult = MessageBoxResult.None;
            //    Workspace.This.Owner.Dispatcher.Invoke(new Action(() =>
            //    {
            //        boxResult = MessageBox.Show("Image has been modified, do you know to save it?", "warning", MessageBoxButton.YesNo);
            //    }));
            //    if (boxResult == MessageBoxResult.Yes)
            //    {
            //        Workspace.This.ImageRotatingPrcessVM.Angle = Convert.ToInt32(_ContrastBlackTextBox.Text);
            //        Workspace.This.ImageRotatingPrcessVM.ExecuteOkCommand(null);
            //        this.Close();
            //    }
            //    else
            //    {
            //        this.Close();
            //    }
            //}
            //else
            //{
            //    this.Close();
            //}
            this.Close();
        }

        private void _OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_ContrastBlackTextBox.Text))
            {
                if (Convert.ToInt32(_ContrastBlackTextBox.Text) == Workspace.This.ImageRotatingPrcessVM.Angle&&
                    _Module1.Text== Workspace.This.ActiveDocument.ImageTypePriview)
                {
                    this.Close();
                }
                else
                {
                    Workspace.This.StartWaitAnimation("Loading...");
                    Workspace.This.ImageRotatingPrcessVM.Angle = Convert.ToInt32(_ContrastBlackTextBox.Text);
                    Workspace.This.ImageRotatingPrcessVM.ExecuteOkCommand(null);
                    Workspace.This.StopWaitAnimation();
                    this.Close();
                }
            }
            else
            {
                this.Close();
            }
        }
        private static bool IsTextAllowed(string text)
        {
            Regex regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
            return !regex.IsMatch(text);
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        public void Init()
        {
            int nBlackValue = Workspace.This.ActiveDocument.BlackValue;
            int whiteValue = Workspace.This.ActiveDocument.WhiteValue;
            double gammaValue = Math.Round(Math.Pow(10, Workspace.This.ActiveDocument.GammaValue), 3);
            if (gammaValue==0) {
                gammaValue = 1;
            }
            bool IsSaturationChecked= Workspace.This.ActiveDocument.IsSaturation;
            bool IsInvertChecked= Workspace.This.ActiveDocument.IsInvert;
            singe = 1;
            DataContext = Workspace.This.ImageRotatingPrcessVM;
            ImageRotatingPrcessViewModel viewModel = DataContext as ImageRotatingPrcessViewModel;
            if (viewModel != null)
            {
                viewModel.Initialize();
            }
            WriteableBitmap wb = null;
            BitmapPalette palette = null;
            palette = new BitmapPalette(ImageProcessing.GetColorTableIndexed(false));
            PixelFormat dstPixelFormat = PixelFormats.Rgb24;
            dstPixelFormat = PixelFormats.Indexed8;
            int dynamicBit = Workspace.This.ActiveDocument.ImageInfo.DynamicBit;
            int maxInput = Workspace.This.ActiveDocument.ImageInfo.MaxPixelValue + 1;
            //将16位转为8位并显示图像
            //Converts 16 bits to 8 bits and displays the image
            wb = new WriteableBitmap(Workspace.This.ActiveDocument.Width,Workspace.This.ActiveDocument. Height, 96, 96, dstPixelFormat, palette);
            //if (Workspace.This.ActiveDocument.RotatingDisplayImage == null)
            {
                Workspace.This.StartWaitAnimation("Loading...");
                try
                {
                    //wb.Lock();
                    ImageProcessingHelper.UpdateRotatingDisplayImage(Workspace.This.ActiveDocument.RotatingImage, ref wb, nBlackValue, whiteValue, gammaValue, maxInput, IsSaturationChecked, _SaturationThreshold, IsInvertChecked, dynamicBit);
                }
                catch
                {
                }
                //将图像另存为一个png图像，用来实时显示旋转（这样不会感觉到有卡顿的）
                //Save the image as a PNG image and use it to show the rotation in real time (so it doesn't feel stuck)
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(wb));
                using (FileStream stream = new FileStream(viewModel.rotapath, FileMode.Create))
                encoder.Save(stream);
                //ImageProcessing.SaveBitmapImageIntoFile(ImageProcessing.ConvertWriteableBitmapToBitmapImage(Workspace.This.ActiveDocument.RotatingImage), viewModel.rotapath);
                wb = RotatProcess.Rotat.ImagePath(viewModel.rotapath, Workspace.This.ActiveDocument.RotatingImage.PixelWidth, Workspace.This.ActiveDocument.RotatingImage.PixelHeight);
                //Workspace.This.ActiveDocument.RotatingDisplayImage = wb;
                img.Source = wb;
                Workspace.This.StopWaitAnimation();
            }
            //else
            //{
            //    wb = Workspace.This.ActiveDocument.RotatingDisplayImage;
            //    img.Source = wb;
            //    RotatProcess.Rotat.toBitmapImage(wb);
            //}
       
            if (Workspace.This.ActiveDocument.ImageTypePriview == null)
            {
               Workspace.This.ImageRotatingPrcessVM.SelectedChannel = "Original";
            }
            else
            {
                Workspace.This.ImageRotatingPrcessVM.SelectedChannel = Workspace.This.ActiveDocument.ImageTypePriview;
            }
            _ContrastBlackTextBox.Text = (Workspace.This.ActiveDocument.Angle).ToString();
        }
        //输入要旋转的角度值执行相应的旋转
        //Enter the value of the Angle to be rotated and perform the corresponding rotation
        private void _PGAModule1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_Module1.SelectedValue.ToString() == "Detail")
            {
                img.Source = RotatProcess.Rotat.rotate(0 - Convert.ToInt32(_ContrastBlackTextBox.Text));
            }
            else
            {
                img.Source = RotatProcess.Rotat.rotateScrap(0 - Convert.ToInt32(_ContrastBlackTextBox.Text));
            }
        }
    }
}
