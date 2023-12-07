using Azure.Adorners;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace Azure.LaserScanner.View
{
    /// <summary>
    /// Interaction logic for RotateArbitraryWindow.xaml
    /// </summary>
    public partial class RotateArbitraryWindow : Window
    {
        private CroppingAdorner _ClpGrid;
        private FrameworkElement _GridBackground = null;
        private const double _ZoomStepSize = 0.1;

        public RotateArbitraryWindow()
        {
            InitializeComponent();
            Loaded += RotateArbitraryWindow_Loaded;
            rotateArbitraryControl.ApplyButtonClicked += RotateArbitraryControl_ApplyButtonClicked;
            _DisplayCanvas.MouseWheel += DisplayCanvas_MouseWheel;
        }

        private void RotateArbitraryWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Display the grid
            AddGridToElement(_DisplayCanvas, show:true);

            // Set the display image in the center of the display canvas
            double left = (_DisplayCanvas.ActualWidth - _DisplayImage.ActualWidth) / 2;
            Canvas.SetLeft(_DisplayImage, left);
            double top = (_DisplayCanvas.ActualHeight - _DisplayImage.ActualHeight) / 2;
            Canvas.SetTop(_DisplayImage, top);
        }

        private void RotateArbitraryControl_ApplyButtonClicked(object sender, EventArgs e)
        {
            this.DialogResult = true;
        }

        private void DisplayCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_DisplayImage.Source == null) { return; }

            if (e.Delta > 0)
            {
                ZoomIn();
            }
            else
            {
                ZoomOut();
            }
        }

        /// <summary>
        /// Fit display image to window
        /// </summary>
        public void RecoverTransform()
        {
            var transformGroup = new TransformGroup();
            var rotateTrans = myRotateTrans;
            var scaleTrans = myScaleTrans;
            myScaleTrans.ScaleX = 1;
            myScaleTrans.ScaleY = 1;
            transformGroup.Children.Add(rotateTrans);
            transformGroup.Children.Add(scaleTrans);
            _DisplayImage.RenderTransform = transformGroup;
        }

        private void ZoomOutBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_DisplayImage != null || _DisplayImage.Source != null)
            {
                ZoomOut();
            }
        }

        private void ZoomInBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_DisplayImage != null || _DisplayImage.Source != null)
            {
                ZoomIn();
            }
        }

        public void ZoomIn()
        {
            myScaleTrans.ScaleX += _ZoomStepSize;
            myScaleTrans.ScaleY += _ZoomStepSize;
        }

        public void ZoomOut()
        {
            if (myScaleTrans.ScaleX >= 1 + _ZoomStepSize)
            {
                myScaleTrans.ScaleX -= _ZoomStepSize;
                myScaleTrans.ScaleY -= _ZoomStepSize;
            }
        }

        private void AddGridToElement(FrameworkElement felem, bool show)
        {
            if (_GridBackground != null && !show)
            {
                RemoveGridFromCur();
            }

            if (_GridBackground == null && show && felem.ActualWidth > 0)
            {
                Rect rcInterior = new Rect(
                    0.0,
                    0.0,
                    felem.ActualWidth,
                    felem.ActualHeight);
                AdornerLayer aly = AdornerLayer.GetAdornerLayer(felem);
                _ClpGrid = new CroppingAdorner(felem, rcInterior, 1.0, true, 20);
                _ClpGrid.Margin = new Thickness(1, 1, 0, 0);
                if (aly != null)
                {
                    aly.Add(_ClpGrid);
                    _GridBackground = felem;
                }
                _ClpGrid.MouseWheel += ClpGrid_MouseWheel;
            }
        }
        private void RemoveGridFromCur()
        {
            AdornerLayer aly = AdornerLayer.GetAdornerLayer(_GridBackground);
            if (aly != null)
            {
                _ClpGrid.MouseWheel -= ClpGrid_MouseWheel;
                aly.Remove(_ClpGrid);
                _GridBackground = null;
            }
        }
        private void ClpGrid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_DisplayImage.Source == null) { return; }

            if (e.Delta > 0)
            {
                ZoomIn();
            }
            else
            {
                ZoomOut();
            }
        }

    }
}
