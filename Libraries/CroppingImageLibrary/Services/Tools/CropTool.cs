using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CroppingImageLibrary.Services.Tools
{
    public delegate void CropChangedHandler(Object sender);

    public class CropTool
    {
        public event CropChangedHandler CropChanged;

        private readonly Canvas _canvas;
        private readonly CropShape _cropShape;
        private readonly ShadeTool _shadeService;
        private readonly ThumbTool _thumbService;
        private readonly TextTool _textService;

        public double TopLeftX => Canvas.GetLeft(_cropShape.Shape);
        public double TopLeftY => Canvas.GetTop(_cropShape.Shape);
        public double BottomRightX => Canvas.GetLeft(_cropShape.Shape) + _cropShape.Shape.Width;
        public double BottomRightY => Canvas.GetTop(_cropShape.Shape) + _cropShape.Shape.Height;
        public double Height => _cropShape.Shape.Height;
        public double Width => _cropShape.Shape.Width;


        #region CropChanged

        /// <summary>
        /// CropChanged Routed Event
        /// </summary>
        //public static readonly RoutedEvent CropChangedEvent = EventManager.RegisterRoutedEvent("CropChanged",
        //    RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CropTool));

        /// <summary>
        /// Occurs when State property changes
        /// </summary>
        //public event RoutedEventHandler CropChanged
        //{
        //    add { AddHandler(CropChangedEvent, value); }
        //    remove { RemoveHandler(CropChangedEvent, value); }
        //}

        /// <summary>
        /// A helper method to raise the StateChanged event.
        /// </summary>
        //protected RoutedEventArgs RaiseCropChangedEvent()
        //{
        //    return RaiseCropChangedEvent(this);
        //}

        /// <summary>
        /// A static helper method to raise the StateChanged event on a target element.
        /// </summary>
        /// <param name="target">UIElement or ContentElement on which to raise the event</param>
        //static RoutedEventArgs RaiseCropChangedEvent(DependencyObject target)
        //{
        //    if (target == null) return null;

        //    RoutedEventArgs args = new RoutedEventArgs();
        //    args.RoutedEvent = CropChangedEvent;
        //    RoutedEventHelper.RaiseEvent(target, args);
        //    return args;
        //}

        #endregion

        public CropTool(Canvas canvas, Color clr)
        {
            _canvas = canvas;
            // crop rectangle with dash rectangle
            //_cropShape = new CropShape(new Rectangle {
            //        Height = 0,
            //        Width = 0,
            //        Stroke = Brushes.Green,
            //        StrokeThickness = 3.5
            //    },
            //    new Rectangle {
            //        Stroke = Brushes.White,
            //        StrokeDashArray = new DoubleCollection(new double[] { 4, 4 })
            //    });

            // remove the dash rectangle
            Color clrShaded = clr;
            clrShaded.A = 35;
            _cropShape = new CropShape(new Rectangle
            {
                Height = 0,
                Width = 0,
                Stroke = new SolidColorBrush(clr),
                Fill = new SolidColorBrush(clrShaded),
                StrokeThickness = 2
            },
            null
            //new Rectangle
            //{
            //    Stroke = Brushes.White,
            //    StrokeDashArray = new DoubleCollection(new double[] { 4, 4 })
            //}
            );

            _shadeService = new ShadeTool(canvas, this);
            _thumbService = new ThumbTool(canvas, this, clr, true);
            _textService = new TextTool(this);

            _canvas.Children.Add(_cropShape.Shape);
            if (_cropShape.DashedShape != null)
            {
                _canvas.Children.Add(_cropShape.DashedShape);
            }

            // background shading
            //_canvas.Children.Add(_shadeService.ShadeOverlay);

            _canvas.Children.Add(_thumbService.BottomMiddle);
            _canvas.Children.Add(_thumbService.LeftMiddle);
            _canvas.Children.Add(_thumbService.TopMiddle);
            _canvas.Children.Add(_thumbService.RightMiddle);
            _canvas.Children.Add(_thumbService.TopLeft);
            _canvas.Children.Add(_thumbService.TopRight);
            _canvas.Children.Add(_thumbService.BottomLeft);
            _canvas.Children.Add(_thumbService.BottomRight);

            // width and height adorner
            //_canvas.Children.Add(_textService.TextBlock);
        }

        public void Redraw(double newX, double newY, double newWidth, double newHeight)
        {
            _cropShape.Redraw(newX, newY, newWidth, newHeight);
            _shadeService.Redraw();
            _thumbService.Redraw();
            _textService.Redraw();

            //Trigger crop changed event here????
            CropChanged?.Invoke(this);
        }
    }
}
