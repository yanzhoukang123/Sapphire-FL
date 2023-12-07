using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Azure.Adorners;
using Azure.Image.Processing;
using Azure.ScannerEUI.ViewModel;

namespace Azure.ScannerEUI.View
{
    public partial class ImagingViewer : UserControl
    {
        #region Private data...

        private Point origin;  // Original Offset of image
        private Point start;   // Original Position of the mouse

        private bool _IsCropImage = false;
        private double _AdornerMargin = 10.0;
        private AdornerLayer _AdornerLayer;

        private const double _RateStep = 1.1;
        private double dShiftX = 0;
        private double dShiftY = 0;

        private double _ZoomRate = 1;
        private double _ImageZoomRate;

        //private MatrixTransform _MatrixTransform;

        #endregion

        #region Constructors...

        public ImagingViewer()
        {
            InitializeComponent();

            //DataContext = Workspace.This.LiveViewModel;
            //((LiveViewModel)DataContext).CropAdornerEvent += new LiveViewModel.CropAdornerDelegate(LiveViewer_CropAdornerEvent);
            //((LiveViewModel)DataContext).CropAdornerRectEvent += new LiveViewModel.CropAdornerRectDelegate(LiveViewer_CropAdornerRectEvent);
        }

        #endregion

        public double ZoomRate
        {
            get { return _ZoomRate; }
            set { _ZoomRate = value; }
        }

        public double ImageZoomRate
        {
            get { return _ImageZoomRate; }
            set { _ImageZoomRate = value; }
        }

        //private void LiveViewer_Loaded(object sender, RoutedEventArgs e)
        //{
        //    if (_ScrollViewer == null || _DisplayImage.Source == null)
        //    {
        //        return;
        //    }
        //}

        private void _DisplayImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (_DisplayImage.IsMouseCaptured)
            {
                Point p = e.MouseDevice.GetPosition(_DisplayCanvas);

                Matrix m = _DisplayImage.RenderTransform.Value;
                //m.OffsetX = origin.X + (p.X - start.X);
                //m.OffsetY = origin.Y + (p.Y - start.Y);
                double deltaX = origin.X + (p.X - start.X);
                double deltaY = origin.Y + (p.Y - start.Y);

                if (p.X > 0 && p.Y > 0 && p.X < _DisplayCanvas.ActualWidth && p.Y < _DisplayCanvas.ActualHeight)
                {
                    //m.OffsetX = deltaX;
                    //Matrix m = _DisplayImage.RenderTransform.Value;
                    //m.Translate(deltaX, deltaY);
                    m.OffsetX = deltaX;
                    m.OffsetY = deltaY;

                    _DisplayImage.RenderTransform = new MatrixTransform(m);
                    //_DisplayImage.ReleaseMouseCapture();
                }
            }
        }

        private void _DisplayImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_DisplayImage.IsMouseCaptured) return;

            _DisplayImage.CaptureMouse();

            start = e.GetPosition(_DisplayCanvas);
            origin.X = _DisplayImage.RenderTransform.Value.OffsetX;
            origin.Y = _DisplayImage.RenderTransform.Value.OffsetY;
        }

        private void _DisplayImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _DisplayImage.ReleaseMouseCapture();
        }

        private void _DisplayImage_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Point p = e.MouseDevice.GetPosition(_DisplayImage);

            Matrix m = _DisplayImage.RenderTransform.Value;
            if (e.Delta > 0)
            {
                _ZoomRate *= _RateStep;
                m.ScaleAtPrepend(_RateStep, _RateStep, p.X, p.Y);
                _DisplayImage.RenderTransform = new MatrixTransform(m);
            }
            else
            {
                if (_ZoomRate / _RateStep < 1)
                {
                    _ZoomRate = 1;
                }
                else
                {
                    _ZoomRate /= _RateStep;
                }

                if (_ZoomRate == 1)
                {
                    RecoverTransform();
                }
                else
                {
                    m.ScaleAtPrepend(1 / _RateStep, 1 / _RateStep, p.X, p.Y);
                    _DisplayImage.RenderTransform = new MatrixTransform(m);
                }
            }
        }

        private void _DisplayImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //LiveViewModel viewModel = Workspace.This.LiveViewModel;
            //if (viewModel == null) { return; }

            if (_DisplayImage.Source == null)
            {
                ImageZoomRate = 0;
                return;
            }
            if (_DisplayImage.ActualWidth < _DisplayImage.Width)
            {
                //ImageZoomRate = viewModel.DisplayImage.PixelHeight / _DisplayImage.ActualHeight;
            }
            else if (_DisplayImage.ActualHeight < _DisplayImage.Height)
            {
                //ImageZoomRate = viewModel.DisplayImage.PixelWidth / _DisplayImage.ActualWidth;
            }
        }

        public void ZoomIn()
        {
            //FileViewModel viewModel = Workspace.This.ActiveDocument;
            //if (viewModel == null) { return; }

            Point center = new Point(_ScrollViewer.ActualWidth / 2, _ScrollViewer.ActualHeight / 2);
            Matrix matrix = ((MatrixTransform)_DisplayImage.RenderTransform).Matrix;
            center = _ScrollViewer.TranslatePoint(center, _DisplayImage);
            _ZoomRate *= _RateStep;
            matrix.ScaleAt(_RateStep, _RateStep, center.X, center.Y);
            ((MatrixTransform)_DisplayImage.RenderTransform).Matrix = matrix;
        }

        public void ZoomOut()
        {
            //FileViewModel viewModel = Workspace.This.ActiveDocument;
            //if (viewModel == null) { return; }

            if (_ZoomRate / _RateStep < 1)
            {
                _ZoomRate = 1;
            }
            else
            {
                _ZoomRate /= _RateStep;
            }

            Point center;
            Matrix matrix = ((MatrixTransform)_DisplayImage.RenderTransform).Matrix;
            if (_ZoomRate == 1)
            {
                RecoverTransform();
            }
            else
            {
                center = new Point(_ScrollViewer.ActualWidth / 2, _ScrollViewer.ActualHeight / 2);
                center = _ScrollViewer.TranslatePoint(center, _DisplayImage);
                matrix.ScaleAt(1 / _RateStep, 1 / _RateStep, center.X, center.Y);
                ((MatrixTransform)_DisplayImage.RenderTransform).Matrix = matrix;
            }
        }

        #region public void RecoverTransform()
        /// <summary>
        /// Fit display image to window
        /// </summary>
        public void RecoverTransform()
        {
            _DisplayImage.RenderTransform = new MatrixTransform(ZoomRate, 0, 0, ZoomRate, -dShiftX, -dShiftY);
            dShiftX = 0;
            dShiftY = 0;
            _DisplayImage.RenderTransform = new MatrixTransform(1, 0, 0, 1, -dShiftX, -dShiftY);
        }
        #endregion

        private void _Thumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            System.Windows.Controls.Primitives.Thumb thumb = sender as System.Windows.Controls.Primitives.Thumb;
            double nTop = Canvas.GetTop(thumb) + e.VerticalChange;
            double nLeft = Canvas.GetLeft(thumb) + e.HorizontalChange;

            if (nLeft < _AdornerMargin)
            {
                nLeft = _AdornerMargin;
            }
            if (nTop < _AdornerMargin)
            {
                nTop = _AdornerMargin;
            }
            if (nLeft > _DisplayCanvas.ActualWidth - thumb.Width - _AdornerMargin)
            {
                nLeft = _DisplayCanvas.ActualWidth - thumb.Width - _AdornerMargin;
            }
            if (nTop > _DisplayCanvas.ActualHeight - thumb.Height - _AdornerMargin)
            {
                nTop = _DisplayCanvas.ActualHeight - thumb.Height - _AdornerMargin;
            }
            Canvas.SetTop(thumb, nTop);
            Canvas.SetLeft(thumb, nLeft);

            MatrixTransform lFx = new MatrixTransform(((MatrixTransform)_DisplayImage.RenderTransform).Matrix);
            _DisplayImage.RenderTransform = lFx;
        }

        private void _DisplayImage_ManipulationStarting(object sender, ManipulationStartingEventArgs e)
        {
            e.ManipulationContainer = _DisplayCanvas;

            e.Mode = ManipulationModes.Scale | ManipulationModes.Translate;
        }

        private void _DisplayImage_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)e.Source;
            element.Opacity = 1;
        }

        private void _DisplayImage_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            //promised the miniscale is 1
            double dZoomStep = 1;
            if (_ZoomRate * e.DeltaManipulation.Scale.X < 1)
            {
                dZoomStep = 1 / _ZoomRate;
                _ZoomRate = 1;
            }
            else
            {
                dZoomStep = e.DeltaManipulation.Scale.X;
                _ZoomRate *= e.DeltaManipulation.Scale.X;
            }

            double dTanslationX = e.DeltaManipulation.Translation.X;
            double dTanslationY = e.DeltaManipulation.Translation.Y;

            //manipulate the image is in canvas when image is shifted
            Point ImageInCanvasPtLT = _DisplayImage.TranslatePoint(new Point(0, 0), _DisplayCanvas);
            Point ImageInCanvasPtRB = _DisplayImage.TranslatePoint(new Point(_DisplayImage.ActualWidth, _DisplayImage.ActualHeight), _DisplayCanvas);
            if (_DisplayImage.ActualWidth * ZoomRate < _DisplayCanvas.ActualWidth)
            {
                if (dTanslationX < 0)
                {
                    if (ImageInCanvasPtLT.X + dTanslationX < 0)
                    {
                        dTanslationX = -ImageInCanvasPtLT.X;
                    }
                }
                else
                {
                    if (ImageInCanvasPtRB.X + dTanslationX > _DisplayCanvas.ActualWidth)
                    {
                        dTanslationX = _DisplayCanvas.ActualWidth - ImageInCanvasPtRB.X;
                    }
                }
            }
            else
            {
                if (dTanslationX < 0)
                {
                    if (ImageInCanvasPtRB.X + dTanslationX < _DisplayCanvas.ActualWidth)
                    {
                        dTanslationX = _DisplayCanvas.ActualWidth - ImageInCanvasPtRB.X;
                    }
                }
                else
                {
                    if (ImageInCanvasPtLT.X + dTanslationX > 0)
                    {
                        dTanslationX = -ImageInCanvasPtLT.X;
                    }
                }
            }

            if (_DisplayCanvas.ActualHeight * ZoomRate < _DisplayCanvas.ActualHeight)
            {
                if (dTanslationY < 0)
                {
                    if (ImageInCanvasPtLT.Y + dTanslationY < 0)
                    {
                        dTanslationY = -ImageInCanvasPtLT.Y;
                    }
                }
                else
                {
                    if (ImageInCanvasPtRB.Y + dTanslationY > _DisplayCanvas.ActualHeight)
                    {
                        dTanslationY = _DisplayCanvas.ActualHeight - ImageInCanvasPtRB.Y;
                    }
                }
            }
            else
            {
                if (dTanslationY < 0)
                {
                    if (ImageInCanvasPtRB.Y + dTanslationY < _DisplayCanvas.ActualHeight)
                    {
                        dTanslationY = _DisplayCanvas.ActualHeight - ImageInCanvasPtRB.Y;
                    }
                }
                else
                {
                    if (ImageInCanvasPtLT.Y + dTanslationY > 0)
                    {
                        dTanslationY = -ImageInCanvasPtLT.Y;
                    }
                }
            }

            //shift
            dShiftX += dTanslationX;
            dShiftY += dTanslationY;

            FrameworkElement element = (FrameworkElement)e.Source;
            //element.Opacity = 0.5;

            Matrix matrix = ((MatrixTransform)element.RenderTransform).Matrix;

            var deltaManipulation = e.DeltaManipulation;

            Point center = new Point(_ImageBorder.ActualWidth / 2, _ImageBorder.ActualHeight / 2);
            center = _ImageBorder.TranslatePoint(center, _DisplayImage);

            matrix.ScaleAt(dZoomStep, dZoomStep, center.X, center.Y);
            matrix.Translate(dTanslationX, dTanslationY);

            ((MatrixTransform)element.RenderTransform).Matrix = matrix;
        }

        #region === Image cropping interface ===

        public void CropInit()
        {
            if (_IsCropImage) { return; }

            Canvas.SetLeft(_Thumb, _AdornerMargin);
            Canvas.SetTop(_Thumb, _AdornerMargin);
            _Thumb.Width = _DisplayCanvas.ActualWidth - _AdornerMargin * 2;
            _Thumb.Height = _DisplayCanvas.ActualHeight - _AdornerMargin * 2;
            _AdornerLayer = AdornerLayer.GetAdornerLayer(_Thumb);
            _AdornerLayer.Add(new MyCanvasAdorner(_Thumb, _DisplayCanvas.ActualWidth - _AdornerMargin * 2, _DisplayCanvas.ActualHeight - _AdornerMargin * 2));
            _Thumb.Visibility = System.Windows.Visibility.Visible;
            _IsCropImage = true;
        }

        public void CropFini()
        {
            if (!_IsCropImage)
                return;
            Adorner[] toRemoveArray = _AdornerLayer.GetAdorners(_Thumb);
            Adorner toRemove;
            if (toRemoveArray != null)
            {
                toRemove = toRemoveArray[0];
                _AdornerLayer.Remove(toRemove);
            }
            _Thumb.Visibility = System.Windows.Visibility.Hidden;
            _IsCropImage = false;
        }

        public Rect GetCropRect()
        {
            if (!_IsCropImage)
                return new Rect();
            Rect CropRect = new Rect();
            Point ptThumbLT = new Point();
            ptThumbLT.X = Canvas.GetLeft(_Thumb);
            ptThumbLT.Y = Canvas.GetTop(_Thumb);
            Point ptThumbRB = new Point();
            ptThumbRB.X = ptThumbLT.X + _Thumb.Width;
            ptThumbRB.Y = ptThumbLT.Y + _Thumb.Height;

            Point ptThumbInImageLT = new Point();
            ptThumbInImageLT = _DisplayCanvas.TranslatePoint(ptThumbLT, _DisplayImage);
            Point ptThumbInImageRB = new Point();
            ptThumbInImageRB = _DisplayCanvas.TranslatePoint(ptThumbRB, _DisplayImage);

            if (ptThumbInImageLT.X < 0)
            {
                CropRect.X = 0;
            }
            else
            {
                CropRect.X = ptThumbInImageLT.X;
            }

            if (ptThumbInImageLT.Y < 0)
            {
                CropRect.Y = 0;
            }
            else
            {
                CropRect.Y = ptThumbInImageLT.Y;
            }

            if (ptThumbInImageRB.X > _DisplayImage.ActualWidth)
            {
                CropRect.Width = _DisplayImage.ActualWidth - CropRect.X;
            }
            else
            {
                CropRect.Width = ptThumbInImageRB.X - CropRect.X;
            }

            if (ptThumbInImageRB.Y > _DisplayImage.ActualHeight)
            {
                CropRect.Height = _DisplayImage.ActualHeight - CropRect.Y;
            }
            else
            {
                CropRect.Height = ptThumbInImageRB.Y - CropRect.Y;
            }

            CropRect.X *= ImageZoomRate;
            CropRect.Y *= ImageZoomRate;
            CropRect.Width *= ImageZoomRate;
            CropRect.Height *= ImageZoomRate;

            return CropRect;
        }

        #endregion

        //private void LiveViewer_CropAdornerEvent(bool bIsVisible)
        //{
        //    if (bIsVisible)
        //    {
        //        CropInit();
        //    }
        //    else
        //    {
        //        CropFini();
        //    }
        //}

        //void LiveViewer_CropAdornerRectEvent()
        //{
        //    LiveViewModel viewModel = Workspace.This.LiveViewModel;
        //    if (viewModel == null)
        //    {
        //        return;
        //    }

        //    viewModel.CropRect = GetCropRect();
        //}

    }
}
