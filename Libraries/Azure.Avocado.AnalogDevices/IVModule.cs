using Azure.EthernetCommLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.Avocado.AnalogDevices
{
    public enum SensorTypes
    {
        APD,
        PMT,
    }

    public enum PGAOptions
    {
        G1 = 1,
        G2 = 2,
        G4 = 4,
        G8 = 8,
        G16 = 16,
        G32 = 32,
        G64 = 64,
        G128 = 128,
    }

    public enum APDGainOptions
    {
        G50 = 50,
        G100 = 100,
        G150 = 150,
        G200 = 200,
        G250 = 250,
        G300 = 300,
        G400 = 400,
        G500 = 500,
    }

    public interface IADModule
    {
        SensorTypes SensorType { get; }
        uint SerialNumber { get; }
        PGAOptions PGAValue { get; set; }

    }

    public class APDConverter : IADModule
    {
        public APDConverter(uint serialNum)
        {
            SensorType = SensorTypes.APD;
            SerialNumber = serialNum;
        }

        public SensorTypes SensorType { get; }
        public uint SerialNumber { get; }
        public PGAOptions PGAValue { get; set; }

        public APDGainOptions APDGain { get; set; }

        public double Temperature { get; set; }
        public double HighVoltage { get; set; }
        public double CalibrationTemper { get; set; }
        public double CalibrationVoltageAtGain50 { get; set; }
        public double CalibrationVoltageAtGain100 { get; set; }
        public double CalibrationVoltageAtGain150 { get; set; }
        public double CalibrationVoltageAtGain200 { get; set; }
        public double CalibrationVoltageAtGain250 { get; set; }
        public double CalibrationVoltageAtGain300 { get; set; }
        public double CalibrationVoltageAtGain400 { get; set; }
        public double CalibrationVoltageAtGain500 { get; set; }
    }

    public class PMTConverter : IADModule
    {
        public PMTConverter(uint serialNum)
        {
            SensorType = SensorTypes.PMT;
            SerialNumber = serialNum;
        }

        public SensorTypes SensorType { get; }
        public uint SerialNumber { get; }
        public PGAOptions PGAValue { get; set; }

        /// <summary>
        /// Range: 0 to 12000
        /// </summary>
        public uint PMTGain { get; set; }
        /// <summary>
        /// Range: 0 to 12000
        /// </summary>
        public uint Compensation { get; set; }
    }
}
