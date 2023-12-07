using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.Avocado.AnalogDevices
{
    public class LaserModule
    {
        public uint SerialNumber { get; }
        public uint LaserWaveLength { get; }
        public double DriveCurrent { get; set; }

        public double CurrentAt5mW { get; set; }
        public double CurrentAt10mW { get; set; }
        public double CurrentAt15mW { get; set; }
        public double CurrentAt20mW { get; set; }
        public double CurrentAt25mW { get; set; }
        public double CurrentAt30mW { get; set; }

        public double TECActualTemper { get; set; }
        public double TECCommandedTemepr { get; set; }
        public double TECMaxPower { get; set; }
    }
}
