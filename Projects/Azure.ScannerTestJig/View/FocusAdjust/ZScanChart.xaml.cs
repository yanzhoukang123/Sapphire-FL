using Azure.ScannerTestJig.ViewModule;
using Azure.ScannerTestJig.ViewModule.FocusAdjust;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Azure.ScannerTestJig.View.FocusAdjust
{
    /// <summary>
    /// ZScanChart.xaml 的交互逻辑
    /// </summary>
    public partial class ZScanChart : UserControl
    {
        public ZScanChart()
        {
            InitializeComponent();
        }
        private void plotter_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ChartPlotter chart = sender as ChartPlotter;
            Point p = e.GetPosition(this).ScreenToData(chart.Transform);
            if (Workspace.This.PramaSetteVM.ZParam1 == 0)
            {
                Workspace.This.PramaSetteVM.ZParam1 = Math.Round(p.Y, 2);
            }
            else if (Workspace.This.PramaSetteVM.ZParam2 == 0)
            {
                Workspace.This.PramaSetteVM.ZParam2 = Math.Round(p.Y, 2);

            }
            else if (Workspace.This.PramaSetteVM.ZParam1>0&& Workspace.This.PramaSetteVM.ZParam2>0) 
            {
                Workspace.This.PramaSetteVM.ZParam1 = Math.Round(p.Y, 2);
                Workspace.This.PramaSetteVM.ZParam2 = 0;
            }
        }
    }
}
