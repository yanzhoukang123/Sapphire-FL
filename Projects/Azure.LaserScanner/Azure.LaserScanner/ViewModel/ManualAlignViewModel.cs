using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Azure.Image.Processing;
using Azure.WPF.Framework;

namespace Azure.LaserScanner.ViewModel
{
    public class ManualAlignViewModel : ViewModelBase
    {
        public enum PixelShift
        {
            Left,
            Right,
            Up,
            Down,
        }

        private ImageChannelType _SelectedChannel = ImageChannelType.Red;
        private bool _Is4ChannelImage = false;
        private bool _IsShiftingPixel = false;
        private Task _ShiftImagePixelTask;
        public ImageChannelType SelectedChannel
        {
            get { return _SelectedChannel; }
            set
            {
                _SelectedChannel = value;
                RaisePropertyChanged("SelectedChannel");
            }
        }

        public bool Is4ChannelImage
        {
            get { return _Is4ChannelImage; }
            set
            {
                _Is4ChannelImage = value;
                RaisePropertyChanged("Is4ChannelImage");
            }
        }

        #region ManualAlignCommand

        private RelayCommand _ManualAlignCommand = null;
        public ICommand ManualAlignCommand
        {
            get
            {
                if (_ManualAlignCommand == null)
                {
                    _ManualAlignCommand = new RelayCommand((p) => ExecuteManualAlignCommand(p), (p) => CanExecuteManualAlignCommand(p));
                }

                return _ManualAlignCommand;
            }
        }

        private void ExecuteManualAlignCommand(object parameter)
        {
            if (SelectedChannel == ImageChannelType.None ||
                SelectedChannel == ImageChannelType.Mix ||
                Workspace.This.ActiveDocument == null ||
                _IsShiftingPixel)
            {
                return;
            }

            PixelShift shiftDir = (PixelShift)parameter;
            WriteableBitmap activeChannel = null;
            WriteableBitmap[] arrChannels = null;
            int chIndex = 0;
            _IsShiftingPixel = true;

            if (Workspace.This.ActiveDocument.IsRgbImage)
            {
                arrChannels = ImageProcessing.GetChannel(Workspace.This.ActiveDocument.Image);
                switch (SelectedChannel)
                {
                    case ImageChannelType.Red:
                        chIndex = 0;
                        activeChannel = arrChannels[chIndex];
                        break;
                    case ImageChannelType.Green:
                        chIndex = 1;
                        activeChannel = arrChannels[chIndex];
                        break;
                    case ImageChannelType.Blue:
                        chIndex = 2;
                        activeChannel = arrChannels[chIndex];
                        break;
                    case ImageChannelType.Gray:
                        chIndex = 3;
                        activeChannel = arrChannels[chIndex];
                        break;
                }
            }
            else
            {
                activeChannel = Workspace.This.ActiveDocument.Image;
            }

            if (shiftDir == PixelShift.Left || shiftDir == PixelShift.Right)
            {
                // Shift left or right
                int offSetX = (shiftDir == PixelShift.Right) ? 1 : -1;
                try
                {
                    //ImageProcessing.ShiftCol(ref activeChannel, offSetX);
                    //_IsShiftingPixel = false;
                    unsafe
                    {
                        int width = activeChannel.PixelWidth;
                        int height = activeChannel.PixelHeight;
                        int stride = activeChannel.BackBufferStride;
                        byte* pImageBuffer = (byte*)activeChannel.BackBuffer.ToPointer();
                        _ShiftImagePixelTask = Task.Run(() => ImageProcessing.ShiftCol(pImageBuffer, width, height, stride, offSetX));
                    }
                }
                catch
                {
                    _IsShiftingPixel = false;
                    throw;
                }
            }
            else if (shiftDir == PixelShift.Up || shiftDir == PixelShift.Down)
            {
                // Shift up or down
                int offSetY = (shiftDir == PixelShift.Down) ? 1 : -1;
                try
                {
                    //ImageProcessing.ShiftRow(ref activeChannel, offSetY);
                    //_IsShiftingPixel = false;
                    unsafe
                    {
                        int width = activeChannel.PixelWidth;
                        int height = activeChannel.PixelHeight;
                        int stride = activeChannel.BackBufferStride;
                        byte* pImageBuffer = (byte*)activeChannel.BackBuffer.ToPointer();
                        _ShiftImagePixelTask = Task.Run(() => ImageProcessing.ShiftRow(pImageBuffer, width, height, stride, offSetY));
                    }
                }
                catch
                {
                    _IsShiftingPixel = false;
                    throw;
                }
            }

            if (_IsShiftingPixel)
            {
                _ShiftImagePixelTask.Wait();
            }
            _IsShiftingPixel = false;

            if (Workspace.This.ActiveDocument.IsRgbImage)
            {
                arrChannels[chIndex] = activeChannel;
                Workspace.This.ActiveDocument.Image = ImageProcessing.SetChannel(arrChannels);
            }
            else
            {
                Workspace.This.ActiveDocument.Image = activeChannel;
            }
            Workspace.This.ActiveDocument.IsDirty = true;
            Workspace.This.ActiveDocument.UpdateDisplayImage();
        }

        private bool CanExecuteManualAlignCommand(object parameter)
        {
            //return (_ActiveDocument != null);
            return true;
        }

        #endregion 

        /*public unsafe void ShiftCol(ref WriteableBitmap srcImage, int offset)
        {
            int cols = srcImage.PixelWidth;
            int rows = srcImage.PixelHeight;
            int stride = srcImage.BackBufferStride;
            byte* pSrcBuffer = (byte*)(void*)srcImage.BackBuffer.ToPointer();
            ushort* pSrc16 = null;

            if (offset > 0)
            {
                // Shift RIGHT
                for (int j = cols - 1; j >= 0; j--)
                {
                    for (int i = 0; i < rows; i++)
                    {
                        pSrc16 = (ushort*)(pSrcBuffer + (i * stride));
                        *(pSrc16 + j) = (j - offset >= 0) ? *(pSrc16 + j - offset) : (ushort)0;
                    }
                }
            }
            else
            {
                // Shift LEFT
                for (int j = 0; j <= cols - 1; j++)
                {
                    for (int i = 0; i < rows; i++)
                    {
                        pSrc16 = (ushort*)(pSrcBuffer + (i * stride));
                        *(pSrc16 + j) = (j + offset < cols) ? *(pSrc16 + j - offset) : (ushort)0;
                    }
                }
            }
        }*/
        /*public unsafe void ShiftRow(ref WriteableBitmap srcImage, int offset)
        {
            int cols = srcImage.PixelWidth;
            int rows = srcImage.PixelHeight;
            int stride = srcImage.BackBufferStride;
            byte* pSrcBuffer = (byte*)(void*)srcImage.BackBuffer.ToPointer();
            ushort* pSrc16L1 = null;
            ushort* pSrc16L2 = null;

            if (offset > 0)
            {
                // Shift DOWN
                for (int j = rows - 1; j >= 0; j--)
                {
                    if (j - offset >= 0)
                    {
                        pSrc16L1 = (ushort*)(pSrcBuffer + ((j - 1) * stride));
                    }
                    pSrc16L2 = (ushort*)(pSrcBuffer + (j * stride));
                    for (int i = 0; i < cols; i++)
                    {
                        *(pSrc16L2 + i) = (j - offset >= 0) ? *(pSrc16L1 + i) : (ushort)0;
                    }
                }
            }
            else
            {
                // Shift UP
                for (int j = 0; j < rows - 1; j++)
                {
                    if (j + offset < rows)
                    {
                        pSrc16L1 = (ushort*)(pSrcBuffer + ((j + 1) * stride));
                    }
                    pSrc16L2 = (ushort*)(pSrcBuffer + (j * stride));
                    for (int i = 0; i < cols; i++)
                    {
                        *(pSrc16L2 + i) = (j + offset < rows) ? *(pSrc16L1 + i) : (ushort)0;
                    }
                }
            }
        }*/

        /*public unsafe void ShiftCol(byte* pImageBuffer, int width, int height, int stride, int offset)
        {
            int cols = width;
            int rows = height;
            ushort* pSrc16 = null;

            if (offset > 0)
            {
                // Shift RIGHT
                for (int j = cols - 1; j >= 0; j--)
                {
                    for (int i = 0; i < rows; i++)
                    {
                        pSrc16 = (ushort*)(pImageBuffer + (i * stride));
                        *(pSrc16 + j) = (j - offset >= 0) ? *(pSrc16 + j - offset) : (ushort)0;
                    }
                }
            }
            else
            {
                // Shift LEFT
                for (int j = 0; j <= cols - 1; j++)
                {
                    for (int i = 0; i < rows; i++)
                    {
                        pSrc16 = (ushort*)(pImageBuffer + (i * stride));
                        *(pSrc16 + j) = (j + offset < cols) ? *(pSrc16 + j - offset) : (ushort)0;
                    }
                }
            }
        }*/
        /*public unsafe void ShiftRow(byte* pImageBuffer, int width, int height, int stride, int offset)
        {
            int cols = width;
            int rows = height;
            ushort* pSrc16L1 = null;
            ushort* pSrc16L2 = null;

            if (offset > 0)
            {
                // Shift DOWN
                for (int j = rows - 1; j >= 0; j--)
                {
                    if (j - offset >= 0)
                    {
                        pSrc16L1 = (ushort*)(pImageBuffer + ((j - 1) * stride));
                    }
                    pSrc16L2 = (ushort*)(pImageBuffer + (j * stride));
                    for (int i = 0; i < cols; i++)
                    {
                        *(pSrc16L2 + i) = (j - offset >= 0) ? *(pSrc16L1 + i) : (ushort)0;
                    }
                }
            }
            else
            {
                // Shift UP
                for (int j = 0; j < rows - 1; j++)
                {
                    if (j + offset < rows)
                    {
                        pSrc16L1 = (ushort*)(pImageBuffer + ((j + 1) * stride));
                    }
                    pSrc16L2 = (ushort*)(pImageBuffer + (j * stride));
                    for (int i = 0; i < cols; i++)
                    {
                        *(pSrc16L2 + i) = (j + offset < rows) ? *(pSrc16L1 + i) : (ushort)0;
                    }
                }
            }
        }*/
    }
}
