using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Azure.APDCalibrationBench
{
    public class CalibrationRecords
    {
        public class CalibrationData
        {
            public int APDGain { get; set; }
            public double? APDOutput { get; set; }
            public double? CalibVolt { get; set; }
            public double? CalibTemper { get; set; }
            public double VerifyAPDOutput { get; set; }
            public double VerifyAPDOutput2 { get; set; }
            public CalibrationData(int apdGain, double? apdOutput = null, double? calibVolt = null, double? calibTemper = null)
            {
                APDGain = apdGain;
                APDOutput = apdOutput;
                CalibVolt = calibVolt;
                CalibTemper = calibTemper;
            }
        }

        public List<CalibrationData> CalibrationRecordsCHA;
        //public List<CalibrationData> CalibrationRecordsCHB;

        public CalibrationRecords()
        {
            CalibrationRecordsCHA = new List<CalibrationData>();
            //CalibrationRecordsCHB = new List<CalibrationData>();

            CalibrationRecordsCHA.Add(new CalibrationData(50));         //
            CalibrationRecordsCHA.Add(new CalibrationData(100));        //
            CalibrationRecordsCHA.Add(new CalibrationData(150));
            CalibrationRecordsCHA.Add(new CalibrationData(200));        //
            CalibrationRecordsCHA.Add(new CalibrationData(250));
            CalibrationRecordsCHA.Add(new CalibrationData(300));        //
           // CalibrationRecordsCHA.Add(new CalibrationData(350));
            CalibrationRecordsCHA.Add(new CalibrationData(400));        //
           // CalibrationRecordsCHA.Add(new CalibrationData(450));
            CalibrationRecordsCHA.Add(new CalibrationData(500));        //

            //CalibrationRecordsCHB.Add(new CalibrationData(50));
            //CalibrationRecordsCHB.Add(new CalibrationData(100));
            //CalibrationRecordsCHB.Add(new CalibrationData(150));
            //CalibrationRecordsCHB.Add(new CalibrationData(200));
            //CalibrationRecordsCHB.Add(new CalibrationData(250));
            //CalibrationRecordsCHB.Add(new CalibrationData(300));
            //CalibrationRecordsCHB.Add(new CalibrationData(350));
            //CalibrationRecordsCHB.Add(new CalibrationData(400));
            //CalibrationRecordsCHB.Add(new CalibrationData(450));
            //CalibrationRecordsCHB.Add(new CalibrationData(500));
        }
    }
}
