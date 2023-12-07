using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;

namespace DrawToolsLib
{
    /// <summary>
    /// Base class for rectangle-based tools
    /// </summary>
    abstract public class ToolRectangleBase : ToolObject
    {
        /// <summary>
        /// Set cursor and resize new object.
        /// </summary>
        public override void OnMouseMove(DrawingCanvas drawingCanvas, MouseEventArgs e)
        {
            drawingCanvas.Cursor = ToolCursor;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (drawingCanvas.IsMouseCaptured)
                {
                    if ( drawingCanvas.Count > 0 )
                    {
                        drawingCanvas[drawingCanvas.Count - 1].MoveHandleTo(
                            e.GetPosition(drawingCanvas), 5);

                        //Point p = e.GetPosition(drawingCanvas);
                        //p.X = p.X / drawingCanvas.ActualScale;
                        //p.Y = p.Y / drawingCanvas.ActualScale;
                        //drawingCanvas[drawingCanvas.Count - 1].MoveHandleTo(p, 5);

                    }
                }

            }
        }

    }
}
