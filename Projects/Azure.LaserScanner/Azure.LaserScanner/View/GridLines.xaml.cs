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
//using System.Windows.Controls.Primitives;   //DataGridCellsPresenter
using Azure.LaserScanner.ViewModel;

namespace Azure.LaserScanner.View
{
    /// <summary>
    /// Interaction logic for GridLines.xaml
    /// </summary>
    public partial class GridLines : UserControl
    {
        List<int> dataList = new List<int>();

        public GridLines()
        {
            const int nRows = 26;
            dataList.Clear();

            for (int i = 0; i < nRows; i++)
            {
                dataList.Add(i + 1);
            }

            InitializeComponent();

            DataContext = Workspace.This.FluorescenceVM;

            dataGrid.ItemsSource = dataList;
        }

        private void dataGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var generator = dataGrid.ItemContainerGenerator;
            var selectedCells = dataGrid.SelectedCells;
            int count = selectedCells.Count;
            List<int> columnList = new List<int>();
            List<int> rowList = new List<int>();
            Dictionary<string, int> colDict = new Dictionary<string, int>();
            int prevRow = 1000;

            for (int i = 0; i < count; i++)
            {
                int colIndex = selectedCells[i].Column.DisplayIndex + 1;    // OK as long user can't reorder
                int rowIndex = generator.IndexFromContainer(generator.ContainerFromItem(selectedCells[i].Item)) + 1;
                //System.Diagnostics.Debug.WriteLine(string.Format("R:{0} C:{1}", rowIndex, colIndex));

                string currColHeader = selectedCells[i].Column.Header.ToString();

                if (!colDict.ContainsKey(currColHeader))
                {
                    colDict.Add(selectedCells[i].Column.Header.ToString(), selectedCells[i].Column.DisplayIndex);
                    columnList.Add(colIndex);
                }

                if (prevRow != rowIndex)
                {
                    rowList.Add(rowIndex);
                    prevRow = rowIndex;
                }
            }

            columnList.Sort();
            rowList.Sort();

            int startCol = columnList[0];
            int endCol = columnList[columnList.Count - 1];
            int startRow = rowList[0];
            int endRow = rowList[rowList.Count - 1];

            Rect region = new Rect(new Point(startCol, startRow), new Point(endCol, endRow));

            FluorescenceViewModel viewModel = Workspace.This.FluorescenceVM;
            if (viewModel != null)
            {
                viewModel.SelectedRegion = region;
            }
        }

        /*private static IEnumerable<T> FindVisualChildren<T>(DependencyObject dependencyObject)
                where T : DependencyObject
        {
            if (dependencyObject != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObject); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(dependencyObject, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }*/

        /*private void GridLines_Loaded(object sender, RoutedEventArgs e)
        {
            if (_GridLinesContainer != null)
            {
                if (_GrayDrawingBrush != null && _GridLinesContainer.ActualHeight != 0)
                {
                    _GrayDrawingBrush.Viewport = new Rect(0, _GridLinesContainer.ActualHeight, 20, 20);
                }
                if (_BlueDrawingBrush != null && _GridLinesContainer.ActualHeight != 0)
                {
                    _BlueDrawingBrush.Viewport = new Rect(0, _GridLinesContainer.ActualHeight, 100, 100);
                }
                if (_RedDrawingBrush != null && _GridLinesContainer.ActualHeight != 0)
                {
                    _RedDrawingBrush.Viewport = new Rect(0, _GridLinesContainer.ActualHeight, 200, 200);
                }
            }
        }*/

        /*private void _GridLinesContainer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_GridLinesContainer != null)
            {
                if (_GrayDrawingBrush != null && _GridLinesContainer.ActualHeight != 0)
                {
                    _GrayDrawingBrush.Viewport = new Rect(0, _GridLinesContainer.ActualHeight, 20, 20);
                }
                if (_BlueDrawingBrush != null && _GridLinesContainer.ActualHeight != 0)
                {
                    _BlueDrawingBrush.Viewport = new Rect(0, _GridLinesContainer.ActualHeight, 100, 100);
                }
                if (_RedDrawingBrush != null && _GridLinesContainer.ActualHeight != 0)
                {
                    _RedDrawingBrush.Viewport = new Rect(0, _GridLinesContainer.ActualHeight, 200, 200);
                }
            }
        }*/

    }

    public class RowNumberConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            DataGridRow row = value as DataGridRow;
            if (row == null)
                throw new InvalidOperationException("This converter class can only be used with DataGridRow elements.");

            return char.ConvertFromUtf32(row.GetIndex() + 65);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

}
