using Microsoft.Research.DynamicDataDisplay.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Azure.ScannerTestJig.ViewModule._532LaserModel
{
    public class VoltagePointCollection532Laser : RingArray<Point>
    {
        private const int TOTAL_POINTS = 525000;

        public VoltagePointCollection532Laser()
            : base(TOTAL_POINTS) // here i set how much values to show 
        {
        }
    }
}
