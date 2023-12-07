using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Azure.Adorners;
using Azure.Configuration.Settings;
using Azure.LaserScanner.ViewModel;

namespace Azure.LaserScanner.View
{
    /// <summary>
    /// Interaction logic for GridLineControl.xaml
    /// </summary>
    public partial class GridLineControl : UserControl
    {
        #region Private data...

        Point? lastCenterPositionOnTarget;
        Point? lastMousePositionOnTarget;
        Point? lastDragPoint;

        private Point origin;  // Original Offset of image
        private Point start;   // Original Position of the mouse

        private double _ZoomRate = 1;
        //private double _ImageZoomRate = 1;

        //private bool _IsCropImage = false;
        //private double _AdornerMargin = 0;
        //private AdornerLayer _AdornerLayer;

        private const double _RateStep = 1.1;
        private double dShiftX = 0;
        private double dShiftY = 0;

        //CroppingAdorner _ClpGrid;
        //FrameworkElement _GridBackground = null;

        private double _CellsMaxWidth = 0;
        private double _CellsMaxHeight = 0;
        private const int _NumCells = 25;
        private const int _NumHorzCellsChemi = 17;
        private const int _NumVertCellsChemi = 14;
        private double _CellSize = 20.0;
        private const string _Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        //private bool _IsAdornerVisibled = false;
        //private bool _IsScrollViewerSizeChanged = false;

        #endregion

        public GridLineControl()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(GridLineControl_Loaded);
            //this.Unloaded += new RoutedEventHandler(GridLineControl_Unloaded);

            //displayCanvas.MouseLeftButtonDown += displayCanvas_MouseLeftButtonDown;
            //displayCanvas.MouseLeftButtonUp += displayCanvas_MouseLeftButtonUp;
            //displayCanvas.MouseMove += displayCanvas_MouseMove;

            //displayCanvas.MouseLeftButtonDown += new MouseButtonEventHandler(displayCanvas_MouseLeftButtonDown);
            //displayCanvas.MouseLeftButtonUp += new MouseButtonEventHandler(displayCanvas_MouseLeftButtonUp);
            //displayCanvas.MouseMove += new MouseEventHandler(displayCanvas_MouseMove);
            //displayCanvas.MouseWheel += new MouseWheelEventHandler(displayCanvas_MouseWheel);

            scrollViewer.ScrollChanged += OnScrollViewerScrollChanged;
            scrollViewer.SizeChanged += scrollViewer_SizeChanged;
            scrollViewer.MouseLeftButtonUp += OnMouseLeftButtonUp;
            //scrollViewer.PreviewMouseLeftButtonUp += OnMouseLeftButtonUp;
            scrollViewer.PreviewMouseWheel += OnPreviewMouseWheel;
            //scrollViewer.MouseLeftButtonUp += OnMouseLeftButtonUp;
            //scrollViewer.MouseWheel += OnPreviewMouseWheel;

            //scrollViewer.PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
            scrollViewer.MouseLeftButtonDown += OnMouseLeftButtonDown;
            scrollViewer.MouseMove += OnMouseMove;

            zoomSlider.SmallChange = 0.1;
            zoomSlider.LargeChange = 0.2;
            zoomSlider.ValueChanged += OnSliderValueChanged;
        }

        #region Public properties...
        #endregion


        private void GridLineControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (scrollViewer != null)
            {
                if (scrollViewer.ActualHeight > 0)
                {
                    //_CellSize = borderContainer.ActualHeight / (_NumCells + 1);  // 25 cells + header cell.
                    _CellSize = Math.Round( (scrollViewer.ActualHeight - horizontalHeaderBot.ActualHeight) / (_NumCells + 1), 2 );
                    _CellsMaxWidth = _CellSize * _NumCells;
                    _CellsMaxHeight = _CellsMaxWidth;

                    this.UpdateGrid();
                    this.UpdateGridHeaders();

                    if (DataContext != null)
                    {
                        if (DataContext is ImagingViewModel)
                        {
                            ImagingViewModel viewModel = DataContext as ImagingViewModel;
                            if (viewModel != null)
                            {
                                viewModel.CellSize = _CellSize;
                                viewModel.NumOfCells = _NumCells;

                                //if (viewModel.ScanRegionAdornerLayer == null)
                                //{
                                //    viewModel.DisplayCanvas = displayCanvas;
                                //    viewModel.ScanRegionAdornerLayer = AdornerLayer.GetAdornerLayer(displayCanvas);
                                //    if (viewModel.DisplayCanvas != null)
                                //    {
                                //        viewModel.RemoveAllScanRegions();   // remove scan rect adorner
                                //    }
                                //}

                                if (viewModel.DisplayCanvas == null)
                                {
                                    viewModel.DisplayCanvas = displayCanvas;
                                }
                            }
                        }
                    }
                }
            }
        }

        //private void GridLineControl_Unloaded(object sender, RoutedEventArgs e)
        //{
        //    GetImagingRegion(); // save thumb location
        //}

        void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (sender is ScrollViewer)
            {
                if (lastDragPoint.HasValue)
                {
                    Point posNow = e.GetPosition(scrollViewer);

                    double dX = posNow.X - lastDragPoint.Value.X;
                    double dY = posNow.Y - lastDragPoint.Value.Y;

                    lastDragPoint = posNow;

                    scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - dX);
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - dY);
                }
            }
        }

        void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is ScrollViewer)
            {
                var mousePos = e.GetPosition(scrollViewer);
                if (mousePos.X <= scrollViewer.ViewportWidth && mousePos.Y < scrollViewer.ViewportHeight) //make sure we still can use the scrollbars
                {
                    scrollViewer.Cursor = Cursors.SizeAll;
                    lastDragPoint = mousePos;
                    Mouse.Capture(scrollViewer);
                }
            }
        }

        void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            lastMousePositionOnTarget = Mouse.GetPosition(gridContainer);

            if (e.Delta > 0)
            {
                zoomSlider.Value += 0.1;
            }
            if (e.Delta < 0)
            {
                zoomSlider.Value -= 0.1;
            }

            e.Handled = true;
        }

        void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is ScrollViewer)
            {
                scrollViewer.Cursor = Cursors.Arrow;
                scrollViewer.ReleaseMouseCapture();
                lastDragPoint = null;
            }
        }

        void OnSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            myGridScaleTransform.ScaleX = e.NewValue;
            myGridScaleTransform.ScaleY = e.NewValue;

            var centerOfViewport = new Point(scrollViewer.ViewportWidth / 2, scrollViewer.ViewportHeight / 2);
            lastCenterPositionOnTarget = scrollViewer.TranslatePoint(centerOfViewport, gridContainer);
        }

        void OnScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.ExtentHeightChange != 0 || e.ExtentWidthChange != 0)
            {
                Point? targetBefore = null;
                Point? targetNow = null;

                if (!lastMousePositionOnTarget.HasValue)
                {
                    if (lastCenterPositionOnTarget.HasValue)
                    {
                        var centerOfViewport = new Point(scrollViewer.ViewportWidth / 2, scrollViewer.ViewportHeight / 2);
                        Point centerOfTargetNow = scrollViewer.TranslatePoint(centerOfViewport, gridContainer);

                        targetBefore = lastCenterPositionOnTarget;
                        targetNow = centerOfTargetNow;
                    }
                }
                else
                {
                    targetBefore = lastMousePositionOnTarget;
                    targetNow = Mouse.GetPosition(gridContainer);

                    lastMousePositionOnTarget = null;
                }

                if (targetBefore.HasValue)
                {
                    double dXInTargetPixels = targetNow.Value.X - targetBefore.Value.X;
                    double dYInTargetPixels = targetNow.Value.Y - targetBefore.Value.Y;

                    //double multiplicatorX = e.ExtentWidth / gridContainer.Width;
                    //double multiplicatorY = e.ExtentHeight / gridContainer.Height;
                    double multiplicatorX = e.ExtentWidth / gridContainer.ActualWidth;
                    double multiplicatorY = e.ExtentHeight / gridContainer.ActualHeight;

                    double newOffsetX = scrollViewer.HorizontalOffset - dXInTargetPixels * multiplicatorX;
                    double newOffsetY = scrollViewer.VerticalOffset - dYInTargetPixels * multiplicatorY;

                    if (double.IsNaN(newOffsetX) || double.IsNaN(newOffsetY))
                    {
                        return;
                    }

                    scrollViewer.ScrollToHorizontalOffset(newOffsetX);
                    scrollViewer.ScrollToVerticalOffset(newOffsetY);
                }
            }
        }

        private void scrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Ignore resizing if currently scanning or when going from minimize to maximize and vice versa
            if (Workspace.This.IsScanning ||
                (Workspace.This.CurWindowState == WindowState.Maximized && Workspace.This.PrevWindowState == WindowState.Minimized) ||
                (Workspace.This.CurWindowState == WindowState.Minimized && Workspace.This.PrevWindowState == WindowState.Maximized))
            {
                return;
            }

            if (scrollViewer != null)
            {
                if (scrollViewer.ActualHeight > 0)
                {
                    zoomSlider.Value = 1.0; // reset imaging region scaling to 100%

                    _CellSize = Math.Round( (scrollViewer.ActualHeight - horizontalHeaderBot.ActualHeight) / (_NumCells + 1), 2 );
                    _CellsMaxWidth = _CellSize * _NumCells;
                    _CellsMaxHeight = _CellsMaxWidth;

                    UpdateGrid();
                    UpdateGridHeaders();

                    if (DataContext != null)
                    {
                        if (this.DataContext is ImagingViewModel)
                        {
                            ImagingViewModel viewModel = DataContext as ImagingViewModel;
                            if (viewModel != null)
                            {
                                if (viewModel.CellSize != _CellSize)
                                {
                                    viewModel.CellSize = _CellSize;
                                    viewModel.NumOfCells = _NumCells;
                                    if (Workspace.This.SelectedImagingType == ImagingSystem.ImagingType.Fluorescence)
                                    {
                                        Workspace.This.FluorescenceVM.LoadAppProtocols(SettingsManager.ConfigSettings.Protocols);
                                    }
                                    else if (Workspace.This.SelectedImagingType == ImagingSystem.ImagingType.PhosphorImaging)
                                    {
                                        Workspace.This.PhosphorVM.LoadAppProtocols(SettingsManager.ConfigSettings.PhosphorProtocols);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void UpdateGrid()
        {
            if (scrollViewer != null)
            {
                if (scrollViewer.ViewportHeight > 0)
                {
                    // Rotate 180
                    myCanvasScaleTransform.ScaleX = 1.0;
                    myCanvasScaleTransform.ScaleY = -1.0;

                    borderContainer.Padding = new Thickness(_CellSize / 2.25, _CellSize / 2.25, _CellSize / 2.25, _CellSize / 2.25);
                    borderContainer.Width = (_NumCells * _CellSize) + _CellSize;
                    borderContainer.HorizontalAlignment = HorizontalAlignment.Left;

                    myGrayDrawingBrush.Viewport = new Rect(0, 0, _CellSize, _CellSize);
                    myGrayLineGeo1.EndPoint = new Point(0, _CellSize);
                    myGrayLineGeo2.EndPoint = new Point(_CellSize, 0);

                    //blueDrawingBrush.Viewport = new Rect(blockSize, blockSize, blockSize * 5.0, blockSize * 5.0);
                    //blueLineGeo1.StartPoint = new Point(blockSize, blockSize);
                    //blueLineGeo1.EndPoint = new Point(blockSize, blockSize * 6.0);
                    //blueLineGeo2.StartPoint = new Point(blockSize, blockSize);
                    //blueLineGeo2.EndPoint = new Point(blockSize * 6.0, blockSize);

                    //blueDrawingBrush.Viewport = new Rect(0, 0, _CellSize * 5.0, _CellSize * 5.0);
                    //blueLineGeo1.StartPoint = new Point(0, 0);
                    //blueLineGeo1.EndPoint = new Point(0, _CellSize * 5.0);
                    //blueLineGeo2.StartPoint = new Point(0, 0);
                    //blueLineGeo2.EndPoint = new Point(_CellSize * 5.0, 0);

                    //redDrawingBrush.Viewport = new Rect(blockSize, blockSize, blockSize * 10.0, blockSize * 10.0);
                    //redLineGeo1.StartPoint = new Point(blockSize, blockSize);
                    //redLineGeo1.EndPoint = new Point(0, blockSize * 10.0);
                    //redLineGeo2.StartPoint = new Point(blockSize, blockSize);
                    //redLineGeo2.EndPoint = new Point(blockSize * 10.0, 0);
                }
            }
        }

        private void UpdateGridHeaders()
        {
            try
            {
                this.UpdateVerticalHeader();
                this.UpdateHorizontalHeaderBottom();
            }
            catch
            {
            }
        }

        private void UpdateHorizontalHeaderTop()
        {
            horizontalHeaderTop.Width = _NumCells * _CellSize;
            horizontalHeaderTop.Height = _CellSize;
            horizontalHeaderTop.HorizontalAlignment = HorizontalAlignment.Left;
            horizontalHeaderTop.VerticalAlignment = VerticalAlignment.Bottom;
            //horizontalHeaderTop.ShowGridLines = true;
            //horizontalHeaderTop.Background = new SolidColorBrush(Colors.LightSteelBlue);

            horizontalHeaderTop.ColumnDefinitions.Clear();
            horizontalHeaderTop.Children.Clear();

            for (int index = 0; index < _NumCells; index++)
            {
                ColumnDefinition gridCol = new ColumnDefinition();

                horizontalHeaderTop.ColumnDefinitions.Add(gridCol);

                TextBlock txtBlock = new TextBlock();
                txtBlock.Text = index.ToString();
                txtBlock.FontSize = 12;
                txtBlock.FontWeight = FontWeights.SemiBold;
                txtBlock.Foreground = new SolidColorBrush(Colors.Black);
                txtBlock.VerticalAlignment = VerticalAlignment.Center;
                txtBlock.HorizontalAlignment = HorizontalAlignment.Center;
                Grid.SetRow(txtBlock, 0);
                Grid.SetColumn(txtBlock, index);

                Rectangle rec = new Rectangle();
                rec.Height = 10;
                rec.Stroke = new SolidColorBrush(Colors.Black);
                rec.HorizontalAlignment = HorizontalAlignment.Center;
                rec.VerticalAlignment = VerticalAlignment.Top;
                Grid.SetRow(txtBlock, 0);
                Grid.SetColumn(rec, index);

                horizontalHeaderTop.Children.Add(txtBlock);
            }
        }

        private void UpdateHorizontalHeaderBottom()
        {
            horizontalHeaderBot.Width = _NumCells * _CellSize + _CellSize;
            horizontalHeaderBot.Height = _CellSize;
            horizontalHeaderBot.HorizontalAlignment = HorizontalAlignment.Left;
            horizontalHeaderBot.VerticalAlignment = VerticalAlignment.Bottom;
            //horizontalHeaderBot.ShowGridLines = true;
            //horizontalHeaderBot.Background = new SolidColorBrush(Colors.LightSteelBlue);

            horizontalHeaderBot.ColumnDefinitions.Clear();
            horizontalHeaderBot.Children.Clear();

            for (int index = 0; index < _NumCells + 1; index++)
            {
                ColumnDefinition gridCol = new ColumnDefinition();

                horizontalHeaderBot.ColumnDefinitions.Add(gridCol);

                TextBlock txtBlock = new TextBlock();
                txtBlock.Text = index.ToString();
                txtBlock.FontSize = 12;
                txtBlock.FontWeight = FontWeights.SemiBold;
                txtBlock.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#A8B0BA");
                txtBlock.VerticalAlignment = VerticalAlignment.Bottom;
                txtBlock.HorizontalAlignment = HorizontalAlignment.Center;
                //txtBlock.Margin = new Thickness(0, 2, 0, 0);
                Grid.SetRow(txtBlock, 0);
                Grid.SetColumn(txtBlock, index);

                Rectangle rec = new Rectangle();
                rec.Height = 10;
                rec.Stroke = new SolidColorBrush(Colors.Black);
                rec.HorizontalAlignment = HorizontalAlignment.Center;
                rec.VerticalAlignment = VerticalAlignment.Top;
                Grid.SetColumn(rec, index);

                horizontalHeaderBot.Children.Add(txtBlock);
                horizontalHeaderBot.Children.Add(rec);
            }
        }

        private void UpdateVerticalHeader()
        {
            verticalHeader.Height = _NumCells * _CellSize + _CellSize;
            verticalHeader.Width = _CellSize;
            verticalHeader.HorizontalAlignment = HorizontalAlignment.Left;
            verticalHeader.VerticalAlignment = VerticalAlignment.Bottom;
            //verticalHeader.ShowGridLines = true;
            //verticalHeader.Background = new SolidColorBrush(Colors.LightSteelBlue);

            verticalHeader.RowDefinitions.Clear();
            verticalHeader.Children.Clear();

            // A to Z
            for (int index = 0; index < _NumCells + 1; index++)
            {
                RowDefinition gridRow = new RowDefinition();

                verticalHeader.RowDefinitions.Add(gridRow);

                TextBlock txtBlock = new TextBlock();
                txtBlock.Text = _Alphabet[_NumCells - index].ToString();
                txtBlock.FontSize = 12;
                txtBlock.FontWeight = FontWeights.SemiBold;
                txtBlock.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#A8B0BA");
                txtBlock.VerticalAlignment = VerticalAlignment.Center;
                txtBlock.HorizontalAlignment = HorizontalAlignment.Left;
                //txtBlock.Margin = new Thickness(6,0,0,0);
                Grid.SetRow(txtBlock, index);
                Grid.SetColumn(txtBlock, 0);

                Rectangle rec = new Rectangle();
                rec.Width = 10;
                rec.Stroke = new SolidColorBrush(Colors.Black);
                rec.HorizontalAlignment = HorizontalAlignment.Right;
                rec.VerticalAlignment = VerticalAlignment.Center;
                Grid.SetRow(rec, index);
                Grid.SetColumn(rec, 0);

                verticalHeader.Children.Add(txtBlock);
                verticalHeader.Children.Add(rec);
            }
        }

        /*private void gridContainer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.UpdateGrid();
            this.UpdateHorizontalHeader();
            this.UpdateVerticalHeader();

            ImagingViewModel viewModel = (ImagingViewModel)DataContext;
            if (viewModel != null)
            {
                viewModel.SelectedImagingRegion = GetImagingRegion();
            }
        }*/

        /*private void Text(double x, double y, string text, Color color)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.Text = text;
            textBlock.Foreground = new SolidColorBrush(color);
            Canvas.SetLeft(textBlock, x);
            Canvas.SetTop(textBlock, y);
            displayCanvas.Children.Add(textBlock);
        }*/


        private void displayCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (displayCanvas.IsMouseCaptured)
            {
                //Point p = e.MouseDevice.GetPosition(gridContainer);
                //
                //Matrix m = displayCanvas.RenderTransform.Value;
                ////m.OffsetX = origin.X + (p.X - start.X);
                ////m.OffsetY = origin.Y + (p.Y - start.Y);
                //double deltaX = origin.X + (p.X - start.X);
                //double deltaY = origin.Y + (p.Y - start.Y);
                //
                //if (p.X > 0 && p.Y > 0 && p.X < DisplayCanvasMaxWidth && p.Y < DisplayCanvasMaxHeight)
                //{
                //    //m.OffsetX = deltaX;
                //    //Matrix m = _DisplayImage.RenderTransform.Value;
                //    //m.Translate(deltaX, deltaY);
                //    m.OffsetX = deltaX;
                //    m.OffsetY = deltaY;
                //
                //    displayCanvas.RenderTransform = new MatrixTransform(m);
                //    //_DisplayImage.ReleaseMouseCapture();
                //    //viewModel.Matrix = _DisplayImage.RenderTransform.Value;
                //}

                Point p = e.MouseDevice.GetPosition(gridContainer);

                Matrix m = displayCanvas.RenderTransform.Value;
                m.OffsetX = origin.X + (p.X - start.X);
                m.OffsetY = origin.Y + (p.Y - start.Y);

                displayCanvas.RenderTransform = new MatrixTransform(m);

            }
        }

        private void displayCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (displayCanvas.IsMouseCaptured) return;

            displayCanvas.CaptureMouse();

            start = e.GetPosition(gridContainer);
            origin.X = displayCanvas.RenderTransform.Value.OffsetX;
            origin.Y = displayCanvas.RenderTransform.Value.OffsetY;
        }

        private void displayCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            displayCanvas.ReleaseMouseCapture();
        }

        private void displayCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (displayCanvas == null) { return; }

            Point p = e.MouseDevice.GetPosition(displayCanvas);

            Matrix m = displayCanvas.RenderTransform.Value;
            if (e.Delta > 0)
            {
                _ZoomRate *= _RateStep;
                m.ScaleAtPrepend(_RateStep, _RateStep, p.X, p.Y);
                displayCanvas.RenderTransform = new MatrixTransform(m);
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
                    displayCanvas.RenderTransform = new MatrixTransform(m);
                }
            }
        }

        public void RecoverTransform()
        {
            displayCanvas.RenderTransform = new MatrixTransform(_ZoomRate, 0, 0, _ZoomRate, -dShiftX, -dShiftY);
            dShiftX = 0;
            dShiftY = 0;
            displayCanvas.RenderTransform = new MatrixTransform(1, 0, 0, 1, -dShiftX, -dShiftY);
        }

        private void _DisplayImage_ManipulationStarting(object sender, ManipulationStartingEventArgs e)
        {
            e.ManipulationContainer = displayCanvas;

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
            Point ImageInCanvasPtLT = gridContainer.TranslatePoint(new Point(0, 0), displayCanvas);
            Point ImageInCanvasPtRB = gridContainer.TranslatePoint(new Point(gridContainer.ActualWidth, gridContainer.ActualHeight), displayCanvas);
            if (gridContainer.ActualWidth * _ZoomRate < displayCanvas.ActualWidth)
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
                    if (ImageInCanvasPtRB.X + dTanslationX > displayCanvas.ActualWidth)
                    {
                        dTanslationX = displayCanvas.ActualWidth - ImageInCanvasPtRB.X;
                    }
                }
            }
            else
            {
                if (dTanslationX < 0)
                {
                    if (ImageInCanvasPtRB.X + dTanslationX < displayCanvas.ActualWidth)
                    {
                        dTanslationX = displayCanvas.ActualWidth - ImageInCanvasPtRB.X;
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

            if (displayCanvas.ActualHeight * _ZoomRate < displayCanvas.ActualHeight)
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
                    if (ImageInCanvasPtRB.Y + dTanslationY > displayCanvas.ActualHeight)
                    {
                        dTanslationY = displayCanvas.ActualHeight - ImageInCanvasPtRB.Y;
                    }
                }
            }
            else
            {
                if (dTanslationY < 0)
                {
                    if (ImageInCanvasPtRB.Y + dTanslationY < displayCanvas.ActualHeight)
                    {
                        dTanslationY = displayCanvas.ActualHeight - ImageInCanvasPtRB.Y;
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

            Point center = new Point(gridContainer.ActualWidth / 2, gridContainer.ActualHeight / 2);
            center = gridContainer.TranslatePoint(center, displayCanvas);

            matrix.ScaleAt(dZoomStep, dZoomStep, center.X, center.Y);
            matrix.Translate(dTanslationX, dTanslationY);

            ((MatrixTransform)element.RenderTransform).Matrix = matrix;
        }


        /*public void RegionAdornerInit()
        {
            if (_Thumb == null || _IsAdornerVisibled) { return; }

            const double numBlocks = _NumCells;
            double top = 0.0;
            double left = 0.0;
            double width = _CellSize * 10.0;
            double height = _CellSize * 10.0;

            width = _CellSize * 10.0;
            height = _CellSize * 10.0;

            if (!_IsScrollViewerSizeChanged)
            {
                if (this.DataContext != null)
                {
                    if (this.DataContext is ImagingViewModel)
                    {
                        ImagingViewModel viewModel = DataContext as ImagingViewModel;
                        if (viewModel != null)
                        {
                            if (viewModel.ThumbRect.Width > 0.0 || viewModel.ThumbRect.Height > 0.0)
                            {
                                //top = viewModel.ThumbRect.Top * _ViewPortChangedFactor;
                                //left = viewModel.ThumbRect.Left * _ViewPortChangedFactor;
                                //width = viewModel.ThumbRect.Width * _ViewPortChangedFactor;
                                //height = viewModel.ThumbRect.Height * _ViewPortChangedFactor;
                                top = viewModel.ThumbRect.Top;
                                left = viewModel.ThumbRect.Left;
                                width = viewModel.ThumbRect.Width;
                                height = viewModel.ThumbRect.Height;
                            }
                            else
                            {
                                viewModel.ThumbRect = new Rect(left, top, width, height);
                            }
                            //viewModel.CellSize = _CellSize;
                        }
                    }
                }
            }

            Canvas.SetLeft(_Thumb, left);
            Canvas.SetTop(_Thumb, top);
            _Thumb.Width = width;
            _Thumb.Height = height;

            _AdornerLayer = AdornerLayer.GetAdornerLayer(_Thumb);

            width = _CellSize * numBlocks;
            height = _CellSize * numBlocks;

            // Comment out for a bug fix (artf156968): changing the application window from maximized
            // to normal and back to maiximized window causes the region selection adorner to be not
            // be able to  selection full scan area
            // ('displayCanvas' size didn't get updated when the application went full screen)
            //
            //if (width > displayCanvas.ActualWidth)
            //{
            //    width = displayCanvas.ActualWidth;
            //}
            //if (height > displayCanvas.ActualHeight)
            //{
            //    height = displayCanvas.ActualHeight;
            //}

            if (_AdornerLayer != null)
            {
                _Thumb.MaxWidth = _CellSize * _NumCells;
                _Thumb.MaxHeight = _CellSize * _NumCells;
                _AdornerLayer.Add(new MyCanvasAdorner(_Thumb, width, height, true));
                _Thumb.Visibility = System.Windows.Visibility.Visible;
                _IsAdornerVisibled = true;
            }
        }*/

        /*public void RegionAdornerFini()
        {
            if (!_IsAdornerVisibled) { return; }

            Adorner[] toRemoveArray = _AdornerLayer.GetAdorners(_Thumb);
            Adorner toRemove;
            if (toRemoveArray != null)
            {
                toRemove = toRemoveArray[0];
                _AdornerLayer.Remove(toRemove);
            }
            _Thumb.Visibility = System.Windows.Visibility.Hidden;
            _IsAdornerVisibled = false;
        }*/

        /*public Rect GetImagingRegion()
        {
            if (!_IsAdornerVisibled) { return new Rect(); }

            Rect imagingRegion = new Rect();
            Point ptThumbLT = new Point();
            ptThumbLT.X = Canvas.GetLeft(_Thumb);
            ptThumbLT.Y = Canvas.GetTop(_Thumb);
            Point ptThumbRB = new Point();

            ptThumbLT.X = (ptThumbLT.X < 0 || ptThumbLT.X < 1) ? 0 : ptThumbLT.X;
            ptThumbLT.Y = (ptThumbLT.Y < 0 || ptThumbLT.Y < 1) ? 0 : ptThumbLT.Y;
            ptThumbRB.X = ptThumbLT.X + _Thumb.Width;
            ptThumbRB.Y = ptThumbLT.Y + _Thumb.Height;

            //Point ptThumbInImageLT = new Point();
            //ptThumbInImageLT = displayCanvas.TranslatePoint(ptThumbLT, displayCanvas);
            //Point ptThumbInImageRB = new Point();
            //ptThumbInImageRB = displayCanvas.TranslatePoint(ptThumbRB, displayCanvas);

            //if (ptThumbInImageLT.X < 0) { imagingRegion.X = 0; } else { imagingRegion.X = ptThumbInImageLT.X; }
            //if (ptThumbInImageLT.Y < 0) { imagingRegion.Y = 0; } else { imagingRegion.Y = ptThumbInImageLT.Y; }
            //if (ptThumbInImageRB.X > displayCanvas.ActualWidth) { imagingRegion.Width = displayCanvas.ActualWidth - imagingRegion.X; }
            //else { imagingRegion.Width = ptThumbInImageRB.X - imagingRegion.X; }
            //if (ptThumbInImageRB.Y > displayCanvas.ActualHeight) { imagingRegion.Height = displayCanvas.ActualHeight - imagingRegion.Y; }
            //else { imagingRegion.Height = ptThumbInImageRB.Y - imagingRegion.Y; }

            imagingRegion.X = ptThumbLT.X;
            imagingRegion.Y = ptThumbLT.Y;
            if (ptThumbRB.X > _CellsMaxWidth) { imagingRegion.Width = _CellsMaxWidth - imagingRegion.X; }
            else { imagingRegion.Width = ptThumbRB.X - imagingRegion.X; }
            if (ptThumbRB.Y > _CellsMaxHeight) { imagingRegion.Height = _CellsMaxHeight - imagingRegion.Y; }
            else { imagingRegion.Height = ptThumbRB.Y - imagingRegion.Y; }

            //imagingRegion.X *= _ImageZoomRate;
            //imagingRegion.Y *= _ImageZoomRate;
            //imagingRegion.Width *= _ImageZoomRate;
            //imagingRegion.Height *= _ImageZoomRate;

            if (imagingRegion.X == 0)
            {
                if (imagingRegion.Width / _CellSize != _NumCells)
                {
                    if ((imagingRegion.Width / _CellSize > _NumCells) ||
                        (imagingRegion.Width / _CellSize > _NumCells - 0.025))
                    {
                        imagingRegion.Width = _CellsMaxWidth;
                    }
                }
            }
            if (imagingRegion.Y == 0)
            {
                if (imagingRegion.Height / _CellSize != _NumCells)
                {
                    if ((imagingRegion.Height / _CellSize > _NumCells) ||
                        (imagingRegion.Height / _CellSize > _NumCells - 0.025))
                    {
                        imagingRegion.Height = _CellsMaxHeight;
                    }
                }
            }

            if (this.DataContext != null)
            {
                if (this.DataContext is ImagingViewModel)
                {
                    ImagingViewModel viewModel = DataContext as ImagingViewModel;
                    if (viewModel != null)
                    {
                        viewModel.ThumbRect = imagingRegion;
                    }
                }
            }

            return imagingRegion;
        }*/


        /*private void _Thumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
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

            //if (nLeft > displayCanvas.ActualWidth - thumb.Width - _AdornerMargin)
            //{
            //    nLeft = displayCanvas.ActualWidth - thumb.Width - _AdornerMargin;
            //}
            //if (nTop > displayCanvas.ActualHeight - thumb.Height - _AdornerMargin)
            //{
            //    nTop = displayCanvas.ActualHeight - thumb.Height - _AdornerMargin;
            //}

            if (nLeft + thumb.ActualWidth > _CellsMaxWidth)
                nLeft = _CellsMaxWidth - thumb.ActualWidth;
            if (nTop + thumb.ActualHeight > _CellsMaxHeight)
                nTop = _CellsMaxHeight - thumb.ActualHeight;

            Canvas.SetTop(thumb, nTop);
            Canvas.SetLeft(thumb, nLeft);

            MatrixTransform lFx = new MatrixTransform(((MatrixTransform)displayCanvas.RenderTransform).Matrix);
            displayCanvas.RenderTransform = lFx;
        }*/

        /*private void _Thumb_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //
            // WORK-AROUND:
            // ClipToBounds for some reason doesn't restrict the adorner resizing within the boundary.
            //

            if (displayCanvas == null || _Thumb == null) { return; }

            // Top left
            Point ptThumbTL = new Point();
            ptThumbTL.X = Canvas.GetLeft(_Thumb);
            ptThumbTL.Y = Canvas.GetTop(_Thumb);

            if (ptThumbTL.X < 0)
            {
                if (_Thumb.Width + ptThumbTL.X < _CellsMaxWidth)
                    _Thumb.Width += ptThumbTL.X;
                else
                    _Thumb.Width = _CellsMaxWidth;
                Canvas.SetLeft(_Thumb, 0);

            }
            if (ptThumbTL.Y < 0)
            {
                if (_Thumb.Height + ptThumbTL.Y < _CellsMaxHeight)
                    _Thumb.Height += ptThumbTL.Y;
                else
                    _Thumb.Height = _CellsMaxHeight;
                Canvas.SetTop(_Thumb, 0);
            }

            // Bottom right
            Point ptThumbBR = new Point();
            ptThumbBR.X = ptThumbTL.X + _Thumb.Width;
            ptThumbBR.Y = ptThumbTL.Y + _Thumb.Height;
            if (ptThumbBR.X > _CellsMaxWidth)
            {
                _Thumb.Width -= (ptThumbBR.X - _CellsMaxWidth);
            }
            if (ptThumbBR.Y > _CellsMaxHeight)
            {
                _Thumb.Height -= (ptThumbBR.Y - _CellsMaxHeight);
            }

            if (this.DataContext != null)
            {
                if (this.DataContext is ImagingViewModel)
                {
                    ImagingViewModel viewModel = DataContext as ImagingViewModel;
                    if (viewModel != null)
                    {
                        viewModel.SelectedImagingRegion = GetImagingRegion();
                    }
                }
            }
        }*/

        /*private void _Thumb_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            if (this.DataContext != null)
            {
                if (this.DataContext is ImagingViewModel)
                {
                    ImagingViewModel viewModel = DataContext as ImagingViewModel;
                    if (viewModel != null)
                    {
                        viewModel.SelectedImagingRegion = GetImagingRegion();
                    }
                }
            }
        }*/


        private void ZoomDecrement_Click(object sender, RoutedEventArgs e)
        {
            zoomSlider.Value -= 0.1;
        }

        private void ZoomIncrement_Click(object sender, RoutedEventArgs e)
        {
            zoomSlider.Value += 0.1;
        }

    }
}
