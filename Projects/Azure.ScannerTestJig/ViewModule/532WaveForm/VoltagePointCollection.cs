using Microsoft.Research.DynamicDataDisplay.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Azure.ScannerTestJig.ViewModule._532WaveForm
{
    public class VoltagePointCollection : RingArray<Point>
    {
        private const int TOTAL_POINTS = 5250;

        public VoltagePointCollection()
            : base(TOTAL_POINTS) // here i set how much values to show 
        {
        }
    }

    //public class Point
    //{
    //    public double index { get; set; }

    //    public double Temper { get; set; }

    //    public double LaserPower { get; set; }

    //    public double LaserElectric { get; set; }

    //    public Point(double Temper, double index)
    //    {
    //        this.index = index;
    //        this.Temper = Temper;
    //    }
    //}
}
