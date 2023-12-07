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
using System.ComponentModel;
using Azure.Adorners;
using Azure.Image.Processing;
using Azure.LaserScanner.ViewModel;
using DrawToolsLib;

namespace Azure.LaserScanner.View
{
    /// <summary>
    /// Interaction logic for ImageViewer.xaml
    /// </summary>
    public partial class ImageViewer : UserControl, INotifyPropertyChanged
    {
        // Declare the event
        public event PropertyChangedEventHandler PropertyChanged;

        #region Private data...

        private Point origin;  // Original Offset of image
        private Point start;   // Original Position of the mouse

        private double _ZoomRate = 1;   // MatrixTransform zoom rate
        //private double _LastZoomRate = 1;
        private double _ImageZoomRate = 1;  // Image size and display window ratio
        //private MatrixTransform _MatrixTransform;
        //private int _ZoomPercent = 100;
        private double _ZoomLevel = 1;  // zoom percentage of the display image to the real image size.

        private bool _IsRegionAdornerVisible = false;
        private double _AdornerMargin = 0;
        private AdornerLayer _AdornerLayer;

        private const double _ZoomRateStep = 1.1;
        private double dShiftX = 0;
        private double dShiftY = 0;

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

            //_DisplayImage.MouseLeftButtonDown += _DisplayImage_MouseLeftButtonDown;
            //_DisplayImage.MouseLeftButtonUp += _DisplayImage_MouseLeftButtonUp;
            //_DisplayImage.MouseMove += _DisplayImage_MouseMove;
            //_DisplayImage.MouseWheel += new MouseWheelEventHandler(_DisplayImage_MouseWheel);

            _DisplayCanvas.MouseLeftButtonDown += _DisplayCanvas_MouseLeftButtonDown;
            _DisplayCanvas.MouseLeftButtonUp += _DisplayCanvas_MouseLeftButtonUp;
            _DisplayCanvas.MouseMove += _DisplayCanvas_MouseMove;
            _DisplayCanvas.MouseWheel += _DisplayCanvas_MouseWheel;

            this.Loaded += new RoutedEventHandler(ImageViewer_Loaded);
        }

        #endregion

        // Create the OnPropertyChanged method to raise the event
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        private void ImageViewer_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext != null)
            {
                if (this.DataContext is FileViewModel)
                {
                    FileViewModel viewModel = DataContext as FileViewModel;
                    if (viewModel != null && !viewModel.IsInitialized)
                    {
                        viewModel.RegionAdornerChanged += new FileViewModel.RegionAdornerDelegate(viewModel_RegionAdornerChanged);
                        viewModel.ImageSizeChanged += ViewModel_ImageSizeChanged;
                        // Load scalebar
                        _DrawingCanvas.Tool = DrawToolsLib.ToolType.None;
                        viewModel.DrawingCanvas = _DrawingCanvas;
                        if (viewModel.ImageInfo != null && viewModel.ImageInfo.DrawingGraphics != null &&
                            viewModel.ImageInfo.DrawingGraphics.Length > 0 && viewModel.ImageInfo.IsShowScalebar)
                        {
                            foreach (var drawingObj in viewModel.ImageInfo.DrawingGraphics)
                            {
                                if (drawingObj is PropertiesGraphicsEllipse)
                                    ((PropertiesGraphicsEllipse)drawingObj).ObjectColor = (Color)ColorConverter.ConvertFromString(((PropertiesGraphicsEllipse)drawingObj).DrawingObjectColor);
                                else if (drawingObj is PropertiesGraphicsLine)
                                    ((PropertiesGraphicsLine)drawingObj).ObjectColor = (Color)ColorConverter.ConvertFromString(((PropertiesGraphicsLine)drawingObj).DrawingObjectColor);
                                else if (drawingObj is PropertiesGraphicsPolyLine)
                                    ((PropertiesGraphicsPolyLine)drawingObj).ObjectColor = (Color)ColorConverter.ConvertFromString(((PropertiesGraphicsPolyLine)drawingObj).DrawingObjectColor);
                                else if (drawingObj is PropertiesGraphicsRectangle)
                                    ((PropertiesGraphicsRectangle)drawingObj).ObjectColor = (Color)ColorConverter.ConvertFromString(((PropertiesGraphicsRectangle)drawingObj).DrawingObjectColor);
                                else if (drawingObj is PropertiesGraphicsText)
                                    ((PropertiesGraphicsText)drawingObj).ObjectColor = (Color)ColorConverter.ConvertFromString(((PropertiesGraphicsText)drawingObj).DrawingObjectColor);
                                viewModel.DrawingCanvas.GraphicsList.Add(drawingObj.CreateGraphics());
                            }
                            viewModel.ImageInfo.Scalebar.SelectedColor = (Color)ColorConverter.ConvertFromString(viewModel.ImageInfo.Scalebar.ForegroundColor);
                            //viewModel.ImageInfo.Scalebar.SelectedBgColor = (Color)ColorConverter.ConvertFromString(viewModel.ImageInfo.Scalebar.BackgroundColor);
                        }
                        viewModel.IsInitialized = true;
                    }
                }
            }
        }

        private void ViewModel_ImageSizeChanged(object sender, double percent)
        {
            RecoverTransform();
            _DisplayImage_SizeChanged(null, null);
        }

        private void viewModel_RegionAdornerChanged(bool bIsVisible)
        {
            if (bIsVisible)
            {
                RegionAdornerInit();
            }
            else
            {
                RegionAdornerFini();
            }
        }

        private void _DisplayCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            FileViewModel viewModel = (FileViewModel)DataContext;
            if (viewModel == null || viewModel.Image == null) { return; }

            //Point point = new Point((e.GetPosition(_DisplayCanvas).X * _ImageZoomRate), (e.GetPosition(_DisplayCanvas).Y * _ImageZoomRate));
            Point point = new Point((e.GetPosition(_DisplayCanvas)).X, (e.GetPosition(_DisplayCanvas)).Y);
            point = _DisplayCanvas.TranslatePoint(point, _DisplayImage);
            point.X = (int)(point.X * _ImageZoomRate);
            point.Y = (int)(point.Y * _ImageZoomRate);

            var width = viewModel.Width;
            var height = viewModel.Height;
            if (point.X < 0 || point.Y < 0 || point.X >= width || point.Y >= height)
                return;

            viewModel.PixelX = ((int)point.X).ToString();
            viewModel.PixelY = ((int)point.Y).ToString();
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

            if (_DisplayCanvas.IsMouseCaptured)
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
                    _DrawingCanvas.RenderTransform = new MatrixTransform(m);
                    //_DisplayImage.ReleaseMouseCapture();
                    //viewModel.Matrix = _DisplayImage.RenderTransform.Value;
                }
            }
        }

        private void _DisplayCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_DisplayCanvas.IsMouseCaptured) return;

            start = e.GetPosition(_DisplayCanvas);

            origin.X = _DisplayImage.RenderTransform.Value.OffsetX;
            origin.Y = _DisplayImage.RenderTransform.Value.OffsetY;

            _DisplayCanvas.CaptureMouse();
        }

        private void _DisplayCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _DisplayCanvas.ReleaseMouseCapture();
        }

        private void _DisplayCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_DisplayImage.Source == null) { return; }

            FileViewModel viewModel = (FileViewModel)DataContext;
            if (viewModel == null || viewModel.Image == null) { return; }

            Point p = new Point((e.GetPosition(_DisplayCanvas)).X, (e.GetPosition(_DisplayCanvas)).Y);
            p = _DisplayCanvas.TranslatePoint(p, _DisplayImage);
            p.X = (int)(p.X * _ImageZoomRate);
            p.Y = (int)(p.Y * _ImageZoomRate);

            var width = viewModel.Width;
            var height = viewModel.Height;
            if (p.X < 0 || p.Y < 0 || p.X >= width || p.Y >= height)
                return;

            if (e.Delta > 0)
            {
                // Zoom in
                //
                Matrix m = _DisplayImage.RenderTransform.Value;
                p = e.MouseDevice.GetPosition(_DisplayCanvas);
                p = _DisplayCanvas.TranslatePoint(p, _DisplayImage);

                _ZoomRate *= _ZoomRateStep;
                m.ScaleAtPrepend(_ZoomRateStep, _ZoomRateStep, p.X, p.Y);
                _DisplayImage.RenderTransform = new MatrixTransform(m);
                _DrawingCanvas.RenderTransform = new MatrixTransform(m);
            }
            else
            {
                if (_ZoomRate / _ZoomRateStep < 1.0)
                {
                    _ZoomRate = 1.0;
                }
                else
                {
                    _ZoomRate /= _ZoomRateStep;
                }

                if (_ZoomRate == 1)
                {
                    RecoverTransform();
                }
                else
                {
                    // Zoom out
                    //
                    Matrix m = _DisplayImage.RenderTransform.Value;
                    p = e.MouseDevice.GetPosition(_DisplayCanvas);
                    p = _DisplayCanvas.TranslatePoint(p, _DisplayImage);

                    m.ScaleAtPrepend(1 / _ZoomRateStep, 1 / _ZoomRateStep, p.X, p.Y);
                    _DisplayImage.RenderTransform = new MatrixTransform(m);
                    _DrawingCanvas.RenderTransform = new MatrixTransform(m);
                }
            }

            if (_DisplayImage.ActualWidth < _DisplayImage.Width)
            {
                _ZoomLevel = _DisplayImage.ActualHeight / viewModel.Image.PixelHeight;
            }
            else if (_DisplayImage.ActualHeight < _DisplayImage.Height)
            {
                _ZoomLevel = _DisplayImage.ActualWidth / viewModel.Image.PixelWidth;
            }
            viewModel.ZoomLevel = _ZoomLevel + _ZoomRate - 1;
            //ZoomPercent = (int)(viewModel.ZoomLevel * 100);
        }


        private void _DisplayImage_MouseMove(object sender, MouseEventArgs e)
        {
            //FileViewModel viewModel = Workspace.This.ActiveDocument;
            FileViewModel viewModel = (FileViewModel)DataContext;
            if (viewModel == null) { return; }

            Point point = new Point((e.GetPosition(_DisplayImage).X * _ImageZoomRate), (e.GetPosition(_DisplayImage).Y * _ImageZoomRate));

            viewModel.PixelX = ((int)point.X).ToString();
            viewModel.PixelY = ((int)point.Y).ToString();
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

            start = e.GetPosition(_DisplayCanvas);

            origin.X = _DisplayImage.RenderTransform.Value.OffsetX;
            origin.Y = _DisplayImage.RenderTransform.Value.OffsetY;

            _DisplayImage.CaptureMouse();
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
                _ZoomRate *= _ZoomRateStep;
                m.ScaleAtPrepend(_ZoomRateStep, _ZoomRateStep, p.X, p.Y);
                _DisplayImage.RenderTransform = new MatrixTransform(m);
            }
            else
            {
                if (_ZoomRate / _ZoomRateStep < 1.0)
                {
                    _ZoomRate = 1.0;
                }
                else
                {
                    _ZoomRate /= _ZoomRateStep;
                }

                if (_ZoomRate == 1)
                {
                    RecoverTransform();
                }
                else
                {
                    m.ScaleAtPrepend(1 / _ZoomRateStep, 1 / _ZoomRateStep, p.X, p.Y);
                    _DisplayImage.RenderTransform = new MatrixTransform(m);
                }
            }
            //_LastZoomRate = _ZoomRate;
            //ZoomPercent = (int)((_ZoomLevel + Math.Abs(_ZoomRate - 1)) * 100);
            //var scale = _DisplayImage.ActualWidth / _DisplayImage.Source.Width;

            FileViewModel viewModel = DataContext as FileViewModel;
            if (viewModel == null || viewModel.Image == null) { return; }
            if (_DisplayImage.ActualWidth < _DisplayImage.Width)
            {
                _ZoomLevel = _DisplayImage.ActualHeight / viewModel.Image.PixelHeight;
            }
            else if (_DisplayImage.ActualHeight < _DisplayImage.Height)
            {
                _ZoomLevel = _DisplayImage.ActualWidth / viewModel.Image.PixelWidth;
            }
            viewModel.ZoomLevel = _ZoomLevel + _ZoomRate - 1;
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
            if (DataContext != null)
            {
                if (DataContext is FileViewModel)
                {
                    FileViewModel viewModel = DataContext as FileViewModel;
                    if (viewModel == null || viewModel.Image == null) { return; }

                    if (_DisplayImage.Source == null)
                    {
                        _ImageZoomRate = 1.0;
                        _ZoomLevel = 1.0;
                        return;
                    }
                    if (_DisplayImage.ActualWidth < _DisplayImage.Width)
                    {
                        _ImageZoomRate = viewModel.Image.PixelHeight / _DisplayImage.ActualHeight;
                        _ZoomLevel = _DisplayImage.ActualHeight / viewModel.Image.PixelHeight;
                    }
                    else if (_DisplayImage.ActualHeight < _DisplayImage.Height)
                    {
                        _ImageZoomRate = viewModel.Image.PixelWidth / _DisplayImage.ActualWidth;
                        _ZoomLevel = _DisplayImage.ActualWidth / viewModel.Image.PixelWidth;
                    }

                    viewModel.ZoomLevel = _ZoomLevel;
                    //ZoomPercent = (int)(_ZoomLevel * 100);
                    if (viewModel.ImageInfo.IsShowScalebar &&
                        viewModel.DrawingCanvas != null && viewModel.DrawingCanvas.Count > 0)
                    {
                        viewModel.DrawScaleBar();
                    }
                }
            }
        }

        /*private void OnZoomUpdated(Object sender, DataTransferEventArgs args)
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
        }*/

        private void zoomOutBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_DisplayImage != null || _DisplayImage.Source != null)
            {
                ZoomOut();
            }
        }

        private void zoomInBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_DisplayImage != null || _DisplayImage.Source != null)
            {
                ZoomIn();
            }
        }

        public void ZoomIn()
        {
            Point center = new Point(_ImageBorder.ActualWidth / 2, _ImageBorder.ActualHeight / 2);
            Matrix matrix = ((MatrixTransform)_DisplayImage.RenderTransform).Matrix;
            center = _ImageBorder.TranslatePoint(center, _DisplayImage);
            _ZoomRate *= _ZoomRateStep;
            matrix.ScaleAt(_ZoomRateStep, _ZoomRateStep, center.X, center.Y);
            ((MatrixTransform)_DisplayImage.RenderTransform).Matrix = matrix;

            Matrix m = _DrawingCanvas.RenderTransform.Value;
            m.ScaleAt(_ZoomRateStep, _ZoomRateStep, center.X, center.Y);
            _DrawingCanvas.RenderTransform = new MatrixTransform(m);

            FileViewModel viewModel = DataContext as FileViewModel;
            if (viewModel == null || viewModel.Image == null) { return; }

            if (_DisplayImage.ActualWidth < _DisplayImage.Width)
            {
                _ImageZoomRate = viewModel.Image.PixelHeight / _DisplayImage.ActualHeight;
                _ZoomLevel = _DisplayImage.ActualHeight / viewModel.Image.PixelHeight;
            }
            else if (_DisplayImage.ActualHeight < _DisplayImage.Height)
            {
                _ImageZoomRate = viewModel.Image.PixelWidth / _DisplayImage.ActualWidth;
                _ZoomLevel = _DisplayImage.ActualWidth / viewModel.Image.PixelWidth;
            }
            viewModel.ZoomLevel = _ZoomLevel + _ZoomRate - 1;
        }

        public void ZoomOut()
        {
            if (_ZoomRate / _ZoomRateStep < 1)
            {
                _ZoomRate = 1;
            }
            else
            {
                _ZoomRate /= _ZoomRateStep;
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
                matrix.ScaleAt(1 / _ZoomRateStep, 1 / _ZoomRateStep, center.X, center.Y);
                ((MatrixTransform)_DisplayImage.RenderTransform).Matrix = matrix;

                //Matrix m = ((MatrixTransform)_DrawingCanvas.RenderTransform).Matrix;
                Matrix m = _DrawingCanvas.RenderTransform.Value;
                m.ScaleAt(1 / _ZoomRateStep, 1 / _ZoomRateStep, center.X, center.Y);
                _DrawingCanvas.RenderTransform = new MatrixTransform(m);
            }
            //_LastZoomRate = _ZoomRate;
            //ZoomPercent = (int)((_ZoomLevel + Math.Abs(_ZoomRate - 1)) * 100);

            FileViewModel viewModel = DataContext as FileViewModel;
            if (viewModel == null || viewModel.Image == null) { return; }

            if (_DisplayImage.ActualWidth < _DisplayImage.Width)
            {
                _ImageZoomRate = viewModel.Image.PixelHeight / _DisplayImage.ActualHeight;
                _ZoomLevel = _DisplayImage.ActualHeight / viewModel.Image.PixelHeight;
            }
            else if (_DisplayImage.ActualHeight < _DisplayImage.Height)
            {
                _ImageZoomRate = viewModel.Image.PixelWidth / _DisplayImage.ActualWidth;
                _ZoomLevel = _DisplayImage.ActualWidth / viewModel.Image.PixelWidth;
            }
            viewModel.ZoomLevel = _ZoomLevel + _ZoomRate - 1;
            //ZoomPercent = (int)((viewModel.ZoomLevel * 100);
        }

        public void ZoomFit()
        {
            _ZoomRate = 1;
            RecoverTransform();
            //ZoomPercent = (int)((_ZoomLevel + Math.Abs(_ZoomRate - 1)) * 100);

            //FileViewModel viewModel = DataContext as FileViewModel;
            //if (viewModel == null || viewModel.Image == null) { return; }
            //
            //if (_DisplayImage.ActualWidth < _DisplayImage.Width)
            //{
            //    _ImageZoomRate = viewModel.Image.PixelHeight / _DisplayImage.ActualHeight;
            //    _ZoomLevel = _DisplayImage.ActualHeight / viewModel.Image.PixelHeight;
            //}
            //else if (_DisplayImage.ActualHeight < _DisplayImage.Height)
            //{
            //    _ImageZoomRate = viewModel.Image.PixelWidth / _DisplayImage.ActualWidth;
            //    _ZoomLevel = _DisplayImage.ActualWidth / viewModel.Image.PixelWidth;
            //}
            //viewModel.ZoomLevel = _ZoomLevel + _ZoomRate - 1;
            //ZoomPercent = (int)((viewModel.ZoomLevel * 100);
        }

        //public int ZoomPercent
        //{
        //    get { return _ZoomPercent; }
        //    set
        //    {
        //        _ZoomPercent = value;
        //        OnPropertyChanged("ZoomPercent");
        //    }
        //}

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

            _DrawingCanvas.RenderTransform = new MatrixTransform(_ZoomRate, 0, 0, _ZoomRate, -dShiftX, -dShiftY);
            dShiftX = 0;
            dShiftY = 0;
            _DrawingCanvas.RenderTransform = new MatrixTransform(1, 0, 0, 1, -dShiftX, -dShiftY);

            FileViewModel viewModel = DataContext as FileViewModel;
            if (viewModel == null || viewModel.Image == null) { return; }

            if (_DisplayImage.ActualWidth < _DisplayImage.Width)
            {
                _ImageZoomRate = viewModel.Image.PixelHeight / _DisplayImage.ActualHeight;
                _ZoomLevel = _DisplayImage.ActualHeight / viewModel.Image.PixelHeight;
            }
            else if (_DisplayImage.ActualHeight < _DisplayImage.Height)
            {
                _ImageZoomRate = viewModel.Image.PixelWidth / _DisplayImage.ActualWidth;
                _ZoomLevel = _DisplayImage.ActualWidth / viewModel.Image.PixelWidth;
            }
            viewModel.ZoomLevel = _ZoomLevel;
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

        public void RegionAdornerInit()
        {
            if (_IsRegionAdornerVisible) { return; }

            Canvas.SetLeft(_Thumb, _AdornerMargin);
            Canvas.SetTop(_Thumb, _AdornerMargin);
            _Thumb.Width = _DisplayCanvas.ActualWidth - _AdornerMargin * 2;
            _Thumb.Height = _DisplayCanvas.ActualHeight - _AdornerMargin * 2;
            _AdornerLayer = AdornerLayer.GetAdornerLayer(_Thumb);
            _AdornerLayer.Add(new MyCanvasAdorner(_Thumb, _DisplayCanvas.ActualWidth - _AdornerMargin * 2, _DisplayCanvas.ActualHeight - _AdornerMargin * 2));
            _Thumb.Visibility = System.Windows.Visibility.Visible;
            _IsRegionAdornerVisible = true;
        }

        public void RegionAdornerFini()
        {
            if (!_IsRegionAdornerVisible) { return; }

            Adorner[] toRemoveArray = _AdornerLayer.GetAdorners(_Thumb);
            Adorner toRemove;
            if (toRemoveArray != null)
            {
                toRemove = toRemoveArray[0];
                _AdornerLayer.Remove(toRemove);
            }
            _Thumb.Visibility = System.Windows.Visibility.Hidden;
            _IsRegionAdornerVisible = false;
        }

        public Rect GetSelectedRegion()
        {
            if (!_IsRegionAdornerVisible) { return new Rect(); }

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

        private void _Thumb_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //
            // WORK-AROUND:
            // ClipToBounds for some reason doesn't restrict the adorner resizing within the boundary.
            //
            // Top left
            /*
            Point ptThumbTL = new Point();
            ptThumbTL.X = Canvas.GetLeft(_Thumb);
            ptThumbTL.Y = Canvas.GetTop(_Thumb);

            if (ptThumbTL.X < 0)
            {
                Canvas.SetLeft(_Thumb, 0);
                _Thumb.Width += ptThumbTL.X;

            }
            if (ptThumbTL.Y < 0)
            {
                Canvas.SetTop(_Thumb, 0);
                _Thumb.Height += ptThumbTL.Y;
            }

            // Bottom right
            Point ptThumbBR = new Point();
            ptThumbBR.X = ptThumbTL.X + _Thumb.Width;
            ptThumbBR.Y = ptThumbTL.Y + _Thumb.Height;
            if (ptThumbBR.X > _DisplayImage.ActualWidth)
            {
                _Thumb.Width -= (ptThumbBR.X - _DisplayImage.ActualWidth);
            }
            if (ptThumbBR.Y > _DisplayImage.ActualHeight)
            {
                _Thumb.Height -= (ptThumbBR.Y - _DisplayImage.ActualHeight);
            }*/

            if (this.DataContext != null)
            {
                if (this.DataContext is FileViewModel)
                {
                    FileViewModel viewModel = DataContext as FileViewModel;
                    if (viewModel != null)
                    {
                        viewModel.SelectedRegion = GetSelectedRegion();
                    }
                }
            }
        }

        private void _Thumb_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            if (this.DataContext != null)
            {
                if (this.DataContext is FileViewModel)
                {
                    FileViewModel viewModel = DataContext as FileViewModel;
                    if (viewModel != null)
                    {
                        viewModel.SelectedRegion = GetSelectedRegion();
                    }
                }
            }
        }

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
