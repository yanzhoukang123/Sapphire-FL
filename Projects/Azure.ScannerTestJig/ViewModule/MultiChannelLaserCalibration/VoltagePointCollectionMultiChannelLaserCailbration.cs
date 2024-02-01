using Microsoft.Research.DynamicDataDisplay.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Azure.ScannerTestJig.ViewModule.MultiChannelLaserCalibration
{
   public class VoltagePointCollectionMultiChannelLaserCailbration : RingArray<Point>
    {
        private const int TOTAL_POINTS = 525000;
        public VoltagePointCollectionMultiChannelLaserCailbration()
           : base(TOTAL_POINTS) // here i set how much values to show 
        {
        }
    }
}
