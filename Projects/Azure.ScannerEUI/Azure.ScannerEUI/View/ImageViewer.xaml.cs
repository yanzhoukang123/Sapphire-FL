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
using System.Windows.Media.Animation;
using System.Windows.Controls.Primitives;   //Thumb
using Azure.Adorners;
using Azure.Image.Processing;
using Azure.ScannerEUI.ViewModel;
using Azure.Configuration.Settings;
//using DrawToolsLib;

namespace Azure.ScannerEUI.View
{
    /// <summary>
    /// Interaction logic for ImageViewer.xaml
    /// </summary>
    public partial class ImageViewer : UserControl
    {
        #region Public

        #endregion

        #region Private data...

        private Point origin;  // Original Offset of image
        private Point start;   // Original Position of the mouse

        private double _ZoomRate = 1;
        //private double _LastZoomRate = 1;
        private double _ImageZoomRate = 1;
        //private MatrixTransform _MatrixTransform;

        private bool _IsCropImage = false;
        private double _AdornerMargin = 10;
        private AdornerLayer _AdornerLayer;

        private const double _RateStep = 1.1;
        private double dShiftX = 0;
        private double dShiftY = 0;
        Arrow[] ArrowHead = new Arrow[255];
        Arrow[] ArrowTrail = new Arrow[255];
        int i_ArrowLine = 0;//箭头
        string temo_ArrowLinetime;
        Point[] p_Arrow = new Point[255];
        TextBlock[] Ruter = new TextBlock[255];
        private int BeginPixelX = 0;
        private int BeginPixelY = 0;
        private int CurrentPixelX = 0;
        private int CurrentPixelY = 0;
        bool IsMove = false;
        bool IsDisplayImage = false;
        #endregion

        #region Constructors...

        public ImageViewer()
        {
            InitializeComponent();
            //DataContext = Workspace.This.ActiveDocument;
            //((FileViewModel)DataContext).ZoomUpdateEvent += new FileViewModel.ZoomUpdateDelegate(ImageViewer_ZoomUpdateEvent);
            //((FileViewModel)DataContext).CropAdornerEvent += new FileViewModel.CropAdornerDelegate(ImageViewer_CropAdornerEvent);
            //((FileViewModel)DataContext).CropAdornerRectEvent += new FileViewModel.CropAdornerRectDelegate(ImageViewer_CropAdornerRectEvent);

            //scrollViewer.ScrollChanged += OnScrollViewerScrollChanged;
            //scrollViewer.MouseLeftButtonUp += OnScrollViewerMouseLeftButtonUp;
            //scrollViewer.PreviewMouseLeftButtonUp += OnMouseLeftButtonUp;
            //scrollViewer.PreviewMouseWheel += OnScrollViewerPreviewMouseWheel;

            //gridContainer.PreviewMouseWheel += OnPreviewMouseWheel;
            //_ScrollViewer.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
            //_ScrollViewer.MouseLeftButtonUp += OnMouseLeftButtonUp;
            //_ScrollViewer.MouseMove += OnMouseMove;
            //_ScrollViewer.ScrollChanged += OnScrollViewerScrollChanged;

            //drawingCanvas.ToolChanged += drawingCanvas_ToolChanged;

            //InitializeDrawingCanvas();
            //InitializePropertiesControls();

            //Setup();
            Workspace.ArrowClearEvent += Workspace_ArrowClear;
            Workspace.ArrowBackWardEvent += Workspace_ArrowBackWardEvent;
            Workspace.ArrowForWardEvent += Workspace_ArrowForWardEvent;
            _DisplayImage.MouseLeftButtonDown += _DisplayImage_MouseLeftButtonDown;
            _DisplayImage.MouseLeftButtonUp += _DisplayImage_MouseLeftButtonUp;
            _DisplayImage.MouseMove += _DisplayImage_MouseMove;
            _DisplayImage.MouseWheel += new MouseWheelEventHandler(_DisplayImage_MouseWheel);
        }

        private void Workspace_ArrowForWardEvent(object sender, EventArgs e)
        {
            if (temo_ArrowLinetime == Workspace.This.ActiveDocument.ArrowLineTime)
            {
                if (ArrowHead[i_ArrowLine] != null)
                {
                    this._DisplayCanvas.Children.Add(ArrowHead[i_ArrowLine]);
                    this._DisplayCanvas.Children.Add(ArrowTrail[i_ArrowLine]);
                    this._DisplayCanvas.Children.Add(Ruter[i_ArrowLine]);
                    i_ArrowLine++;
                }
            }
        }

        private void Workspace_ArrowBackWardEvent(object sender, EventArgs e)
        {
            if (temo_ArrowLinetime == Workspace.This.ActiveDocument.ArrowLineTime)
            {
                if (i_ArrowLine > 0)
                {
                    i_ArrowLine--;
                    this._DisplayCanvas.Children.Remove(ArrowHead[i_ArrowLine]);
                    this._DisplayCanvas.Children.Remove(ArrowTrail[i_ArrowLine]);
                    this._DisplayCanvas.Children.Remove(Ruter[i_ArrowLine]);
                }
            }
        }

        private void Workspace_ArrowClear(object sender, EventArgs e)
        {
            if (temo_ArrowLinetime == Workspace.This.ActiveDocument.ArrowLineTime)
            {

                for (int k = 0; k <= i_ArrowLine; k++)
                {
                    this._DisplayCanvas.Children.Remove(ArrowHead[k]);
                    this._DisplayCanvas.Children.Remove(ArrowTrail[k]);
                    this._DisplayCanvas.Children.Remove(Ruter[k]);
                }
                i_ArrowLine = 0;
                ArrowHead = new Arrow[255];
                ArrowTrail = new Arrow[255];
                Ruter = new TextBlock[255];
            }
            //ArrowClera();
        }

        #endregion

        private void _DisplayImage_MouseMove(object sender, MouseEventArgs e)
        {
            //FileViewModel viewModel = Workspace.This.ActiveDocument;
            FileViewModel viewModel = (FileViewModel)DataContext;
            if (viewModel == null) { return; }
            Point point = new Point((e.GetPosition(_DisplayImage).X * _ImageZoomRate), (e.GetPosition(_DisplayImage).Y * _ImageZoomRate));

            viewModel.PixelX = ((int)point.X).ToString();
            viewModel.PixelY = ((int)point.Y).ToString();
            CurrentPixelX = (int)point.X;
            CurrentPixelY = (int)point.Y;
            int iRedData = 0;
            int iGreenData = 0;
            int iBlueData = 0;
            int iGrayData = 0;
            int iDynamicBit = viewModel.ImageInfo.DynamicBit;

            ImageProcessingHelper.GetPixelIntensity(viewModel.Image, point, ref iRedData, ref iGreenData, ref iBlueData, ref iGrayData, iDynamicBit);

            if (viewModel.Image.Format == PixelFormats.Gray8 ||
                viewModel.Image.Format == PixelFormats.Indexed8 ||
                viewModel.Image.Format == PixelFormats.Gray16 ||
                viewModel.Image.Format == PixelFormats.Gray32Float)
            {
                viewModel.PixelIntensity = string.Format("{0}", iRedData);
            }
            else if (viewModel.Image.Format == PixelFormats.Bgr24 ||
                viewModel.Image.Format == PixelFormats.Rgb24 ||
                viewModel.Image.Format == PixelFormats.Rgb48)
            {
                viewModel.PixelIntensity = string.Format("(R: {0} G: {1} B: {2})", iRedData, iGreenData, iBlueData);
            }
            else if (viewModel.Image.Format == PixelFormats.Rgba64)
            {
                viewModel.PixelIntensity = string.Format("(R: {0} G: {1} B: {2} K: {3})", iRedData, iGreenData, iBlueData, iGrayData);
            }

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
                    //viewModel.Matrix = _DisplayImage.RenderTransform.Value;
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
            if (_DisplayImage.Source == null) { return; }

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
                if (_ZoomRate / _RateStep < 1.0)
                {
                    _ZoomRate = 1.0;
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
            //_LastZoomRate = _ZoomRate;
        }


        /// <summary>
        /// Set initial properties of drawing canvas
        /// </summary>
        /*private void InitializeDrawingCanvas()
        {
            drawingCanvas.LineWidth = SettingsManager.ApplicationSettings.LineWidth;
            drawingCanvas.ObjectColor = SettingsManager.ApplicationSettings.ObjectColor;

            drawingCanvas.TextFontSize = SettingsManager.ApplicationSettings.TextFontSize;
            drawingCanvas.TextFontFamilyName = SettingsManager.ApplicationSettings.TextFontFamilyName;
            drawingCanvas.TextFontStyle = FontConversions.FontStyleFromString(SettingsManager.ApplicationSettings.TextFontStyle);
            drawingCanvas.TextFontWeight = FontConversions.FontWeightFromString(SettingsManager.ApplicationSettings.TextFontWeight);
            drawingCanvas.TextFontStretch = FontConversions.FontStretchFromString(SettingsManager.ApplicationSettings.TextFontStretch);
        }*/

        /*private void drawingCanvas_ToolChanged(object sender)
        {
            FileViewModel viewModel = Workspace.This.ActiveDocument;

            if (viewModel.SelectedDrawingTool != drawingCanvas.Tool)
            {
                // Update the drawing tool selection
                viewModel.SelectedDrawingTool = drawingCanvas.Tool;
            }
        }*/

        private void _DisplayImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //FileViewModel viewModel = Workspace.This.ActiveDocument;
            FileViewModel viewModel = (FileViewModel)DataContext;
            if (viewModel == null || viewModel.Image == null) { return; }

            if (_DisplayImage.Source == null)
            {
                _ImageZoomRate = 1.0;
                //viewModel.ImageZoomRate = _ImageZoomRate;
                return;
            }
            if (_DisplayImage.ActualWidth < _DisplayImage.Width)
            {
                _ImageZoomRate = viewModel.Image.PixelHeight / _DisplayImage.ActualHeight;
                //viewModel.ImageZoomRate = _ImageZoomRate;
            }
            else if (_DisplayImage.ActualHeight < _DisplayImage.Height)
            {
                _ImageZoomRate = viewModel.Image.PixelWidth / _DisplayImage.ActualWidth;
                //viewModel.ImageZoomRate = _ImageZoomRate;
            }
        }

        private void OnZoomUpdated(Object sender, DataTransferEventArgs args)
        {
            if (_DisplayImage.Source == null) { return; }

            //FileViewModel viewModel = (FileViewModel)DataContext;
            //if (viewModel == null) { return; }
            //ZoomType zoomingType = viewModel.ZoomingType;

            ZoomType zoomingType = (ZoomType)Enum.Parse(typeof(ZoomType), _ZoomUpdate.Text);

            if (zoomingType == ZoomType.ZoomIn)
            {
                ZoomIn();
            }
            else if (zoomingType == ZoomType.ZoomOut)
            {
                ZoomOut();
            }
            //else if (zoomingType == ZoomType.ZoomFit)
            //{
            //    ZoomFit();
            //}
        }

        public void ZoomIn()
        {
            Point center = new Point(_ImageBorder.ActualWidth / 2, _ImageBorder.ActualHeight / 2);
            Matrix matrix = ((MatrixTransform)_DisplayImage.RenderTransform).Matrix;
            center = _ImageBorder.TranslatePoint(center, _DisplayImage);
            _ZoomRate *= _RateStep;
            matrix.ScaleAt(_RateStep, _RateStep, center.X, center.Y);
            ((MatrixTransform)_DisplayImage.RenderTransform).Matrix = matrix;
            //_LastZoomRate = _ZoomRate;
        }

        public void ZoomOut()
        {
            if (_ZoomRate / _RateStep < 1)
            {
                _ZoomRate = 1;
            }
            else
            {
                _ZoomRate /= _RateStep;
            }

            Matrix matrix = ((MatrixTransform)_DisplayImage.RenderTransform).Matrix;
            if (_ZoomRate == 1)
            {
                RecoverTransform();
            }
            else
            {
                Point center = new Point(_ImageBorder.ActualWidth / 2, _ImageBorder.ActualHeight / 2);
                center = _ImageBorder.TranslatePoint(center, _DisplayImage);
                matrix.ScaleAt(1 / _RateStep, 1 / _RateStep, center.X, center.Y);
                ((MatrixTransform)_DisplayImage.RenderTransform).Matrix = matrix;
            }
            //_LastZoomRate = _ZoomRate;
        }

        public void ZoomFit()
        {
            _ZoomRate = 1;
            RecoverTransform();
        }

        #region public void RecoverTransform()
        /// <summary>
        /// Fit display image to window
        /// </summary>
        public void RecoverTransform()
        {
            _DisplayImage.RenderTransform = new MatrixTransform(_ZoomRate, 0, 0, _ZoomRate, -dShiftX, -dShiftY);
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
            if (_DisplayImage.ActualWidth * _ZoomRate < _DisplayCanvas.ActualWidth)
            {
                //dTanslationX = 0;
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

            if (_DisplayImage.ActualHeight * _ZoomRate < _DisplayCanvas.ActualHeight)
            {
                //dTanslationY = 0;
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
            //_LastZoomRate = _ZoomRate;
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
            if (!_IsCropImage) { return; }

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
            if (!_IsCropImage) { return new Rect(); }

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

            if (ptThumbInImageLT.X < 0) { CropRect.X = 0; } else { CropRect.X = ptThumbInImageLT.X; }
            if (ptThumbInImageLT.Y < 0) { CropRect.Y = 0; } else { CropRect.Y = ptThumbInImageLT.Y; }
            if (ptThumbInImageRB.X > _DisplayImage.ActualWidth) { CropRect.Width = _DisplayImage.ActualWidth - CropRect.X; }
            else { CropRect.Width = ptThumbInImageRB.X - CropRect.X; }
            if (ptThumbInImageRB.Y > _DisplayImage.ActualHeight) { CropRect.Height = _DisplayImage.ActualHeight - CropRect.Y; }
            else { CropRect.Height = ptThumbInImageRB.Y - CropRect.Y; }

            CropRect.X *= _ImageZoomRate;
            CropRect.Y *= _ImageZoomRate;
            CropRect.Width *= _ImageZoomRate;
            CropRect.Height *= _ImageZoomRate;

            return CropRect;
        }

        private void InkCanvas_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            #region//箭头
            ++i_ArrowLine;
            string temptime = DateTime.Now.ToString("yyyy_MM_dd_h_mm_ss_fff");//2018-3-31 1:30:10.033
            Workspace.This.ActiveDocument.ArrowLineTime = temptime;
            temo_ArrowLinetime = temptime;
            this.Cursor = Cursors.Hand;
            IsMove = false;
            #endregion
        }

        private void InkCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            #region//箭头
            if (Mouse.RightButton == MouseButtonState.Pressed && IsMove)
            {
                if (CurrentPixelX <=PixelWidth-5 && CurrentPixelX >=0 && CurrentPixelY <=PixelHeight-5 && CurrentPixelY>=0)
                {
                    _DisplayImage_MouseMove(null, e);
                    this.ArrowHead[i_ArrowLine].StrokeThickness = 3;
                    this.ArrowHead[i_ArrowLine].HeadHeight = 8;
                    this.ArrowHead[i_ArrowLine].HeadWidth = 8;
                    this.ArrowHead[i_ArrowLine].Opacity = 1;
                    this.ArrowTrail[i_ArrowLine].StrokeThickness = 3;
                    this.ArrowTrail[i_ArrowLine].HeadHeight = 8;
                    this.ArrowTrail[i_ArrowLine].HeadWidth = 8;
                    this.ArrowTrail[i_ArrowLine].Opacity = 1;
                    p_Arrow[i_ArrowLine] = Mouse.GetPosition(this._DisplayCanvas);
                    double X = p_Arrow[i_ArrowLine].X;
                    double Y = p_Arrow[i_ArrowLine].Y;
                    this.ArrowHead[i_ArrowLine].X2 = X;
                    this.ArrowHead[i_ArrowLine].Y2 = Y;
                    this.ArrowTrail[i_ArrowLine].X1 = X;
                    this.ArrowTrail[i_ArrowLine].Y1 = Y;
                    Ruter[i_ArrowLine].Text = "X," + BeginPixelX + "     Y," + BeginPixelY + "     " + PixelTranForm(BeginPixelX, BeginPixelY, CurrentPixelX, CurrentPixelY) + "mm     " + "DX," + CurrentPixelX + "     DY," + CurrentPixelY;
                    Ruter[i_ArrowLine].SetValue(Canvas.LeftProperty, (X + this.ArrowHead[i_ArrowLine].X1) / 2);
                    Ruter[i_ArrowLine].SetValue(Canvas.TopProperty, (Y + this.ArrowHead[i_ArrowLine].Y1) / 2 - 25);
                }
            }
            #endregion
        }
        int PixelWidth = 0;
        int PixelHeight = 0;
        private void InkCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsDisplayImage)
            {
                return;
            }
            #region//箭头
            WriteableBitmap IS = (WriteableBitmap)_DisplayImage.Source;
            PixelWidth = IS.PixelWidth;
            PixelHeight = IS.PixelHeight;
            p_Arrow[i_ArrowLine] = new Point();
            this.ArrowHead[i_ArrowLine] = new Arrow();
            this.ArrowHead[i_ArrowLine].Stroke = new SolidColorBrush(Colors.Red);
            this.ArrowTrail[i_ArrowLine] = new Arrow();
            this.ArrowTrail[i_ArrowLine].Stroke = new SolidColorBrush(Colors.Red);
            Point p = new Point();
            p = Mouse.GetPosition(this._DisplayCanvas);
            this.ArrowHead[i_ArrowLine].X1 = p.X;
            this.ArrowHead[i_ArrowLine].Y1 = p.Y;
            this.ArrowTrail[i_ArrowLine].X1 = p.X;
            this.ArrowTrail[i_ArrowLine].Y1 = p.Y;
            Ruter[i_ArrowLine] = new TextBlock()
            {
                Width = 1000,
                Height = 40,
                FontSize = 15,

            };
            Ruter[i_ArrowLine].Foreground = new SolidColorBrush(Colors.Bisque);
            if (p_Arrow[i_ArrowLine].X == 0 && p_Arrow[i_ArrowLine].Y == 0)
            {
                this.ArrowHead[i_ArrowLine].X2 = p.X;
                this.ArrowHead[i_ArrowLine].Y2 = p.Y;
                this.ArrowTrail[i_ArrowLine].X2 = p.X;
                this.ArrowTrail[i_ArrowLine].Y2 = p.Y;
            }
            BeginPixelX = CurrentPixelX;
            BeginPixelY = CurrentPixelY;
            this._DisplayCanvas.Children.Add(Ruter[i_ArrowLine]);
            this._DisplayCanvas.Children.Add(ArrowTrail[i_ArrowLine]);
            this._DisplayCanvas.Children.Add(ArrowHead[i_ArrowLine]);
            this.Cursor = Cursors.Cross;
            IsMove = true;

            #endregion
        }

        #region Pixel TransForm

        private double PixelTranForm(int beginPixelX, int beginPixelY, int endPixelX, int endPixelY)
        {           
            //√【（X1 - X2）²+（Y1 - Y2）²】=?
            int Reslution = Workspace.This.ActiveDocument.ImageInfo.ScanResolution;
            double X1 = Math.Round(Math.Pow(Math.Abs(beginPixelX - endPixelX),2),2);
            double Y1 = Math.Round(Math.Pow(Math.Abs(beginPixelY - endPixelY),2),2);
            double Result = Math.Round((Math.Sqrt(X1 + Y1)* Reslution/1000),2);
            return Result;
        }

        private void _DisplayImage_MouseEnter(object sender, MouseEventArgs e)
        {
            IsDisplayImage = true;
        }

        private void _DisplayImage_MouseLeave(object sender, MouseEventArgs e)
        {
            IsDisplayImage = false;
        }

        //public  void ArrowClera()
        //{
        //    for (int k = 0; k <= i_ArrowLine; k++)
        //    {
        //        this._DisplayCanvas.Children.Remove(ArrowHead[k]);
        //        this._DisplayCanvas.Children.Remove(ArrowTrail[k]);
        //        this._DisplayCanvas.Children.Remove(Ruter[k]);
        //    }
        //    i_ArrowLine = 0;
        //    ArrowHead = new Arrow[255];
        //    ArrowTrail = new Arrow[255];
        //    Ruter =new TextBlock[255];
        //    //this.InkCanvas.EditingMode = InkCanvasEditingMode.None;

        //}

        #endregion

        /*private void _CropVisibility_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            if (_CropVisibility.Visibility == System.Windows.Visibility.Visible && _DisplayImage != null)
            {
                CropInit();
            }
            else
            {
                CropFini();
            }
        }*/

        /*private void _TriggerGetCropRect_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            //FileViewModel viewModel = Workspace.This.ActiveDocument;
            FileViewModel viewModel = (FileViewModel)DataContext;
            if (viewModel == null) { return; }

            if (_TriggerGetRect.Visibility == System.Windows.Visibility.Visible && _DisplayImage != null)
            {
                viewModel.CropRect = GetCropRect();
            }
        }*/

        #endregion

        /// <summary>
        /// Initialize Properties controls on the toolbar
        /// </summary>
        /*void InitializePropertiesControls()
        {
            for (int i = 1; i <= 10; i++)
            {
                comboPropertiesLineWidth.Items.Add(i.ToString(CultureInfo.InvariantCulture));
            }

            // Fill line width combo and set initial selection
            int lineWidth = (int)(Workspace.This.ActiveDocument.DrawingCanvas.LineWidth + 0.5);

            if (lineWidth < 1)
            {
                lineWidth = 1;
            }

            if (lineWidth > 10)
            {
                lineWidth = 10;
            }

            comboPropertiesLineWidth.SelectedIndex = lineWidth - 1;

            buttonPropertiesFont.Click += new RoutedEventHandler(PropertiesFont_Click);
            //buttonPropertiesColor.Click += new RoutedEventHandler(PropertiesColor_Click);
            comboPropertiesLineWidth.SelectionChanged += new SelectionChangedEventHandler(PropertiesLineWidth_SelectionChanged);
        }*/

    }
}
