using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;               // Application.Current.Dispatcher
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Windows.Threading;     //Dispatcher
using Azure.Image.Processing;
using Azure.Configuration.Settings;
using Azure.EthernetCommLib;
using Azure.LaserScanner.ViewModel;
using Azure.Ipp.Imaging;
using Azure.ImagingSystem;

namespace Azure.LaserScanner.ViewModel
{
    public class ImagingSystemHelper
    {
        /// <summary>
        /// Convert time in seconds to hour, minutes, seconds (long format)
        /// </summary>
        /// <param name="timeInSeconds">Time in seconds</param>
        /// <returns>Formatted time</returns>
        public static string FormatTime(double timeInSeconds)
        {
            if (double.IsNaN(timeInSeconds) || double.IsInfinity(timeInSeconds) || timeInSeconds <= 0)
            {
                return string.Empty;
            }

            TimeSpan timeSpan = TimeSpan.FromSeconds(timeInSeconds);
            int nHourInDays = 0;

            if (timeSpan.Days > 0)
                nHourInDays = timeSpan.Days * 24;  // Days into hours
            int nTotalHours = nHourInDays + timeSpan.Hours;
            string strHour = (nTotalHours > 0) ? string.Format("{0} hour", nTotalHours) : string.Empty;
            strHour += (nTotalHours > 0 && (timeSpan.Minutes > 0 || timeSpan.Seconds > 0)) ? ", " : "";
            string strMin = string.Empty;
            if (nTotalHours > 0)
            {
                strMin = (timeSpan.Minutes > 0 || timeSpan.Seconds > 0) ? string.Format("{0} min", timeSpan.Minutes) : string.Empty;
                strMin += (timeSpan.Seconds > 0) ? ", " : "";
            }
            else
            {
                strMin = (timeSpan.Minutes > 0) ? string.Format("{0} min", timeSpan.Minutes) : string.Empty;
                strMin += (timeSpan.Minutes > 0 && timeSpan.Seconds > 0) ? ", " : "";
            }
            //strMin = (timeSpan.Minutes > 0 || timeSpan.Seconds > 0) ? string.Format("{0} min", timeSpan.Minutes) : string.Empty;
            //strMin += (timeSpan.Seconds > 0) ? ", " : "";
            string strSec = (timeSpan.Seconds > 0) ? string.Format("{0} sec", timeSpan.Seconds) : string.Empty;
            //string strMsec = (timeSpan.Milliseconds > 0) ? string.Format("{0}ms", timeSpan.Milliseconds) : string.Empty;
            //string strFormattedTime = string.Format("{0}{1}{2}", strMin, strSec, strMsec);
            string strFormattedTime = string.Empty;
            if (nTotalHours > 0 && timeSpan.Seconds > 0)
            {
                strFormattedTime = string.Format("{0:D2}:{1:D2}:{2:D2}", nTotalHours, timeSpan.Minutes, timeSpan.Seconds);
            }
            else
            {
                strFormattedTime = string.Format("{0}{1}{2}", strHour, strMin, strSec);
            }
            return strFormattedTime;
        }

        /// <summary>
        /// Convert time in seconds to day.hour:minutes:seconds
        /// </summary>
        /// <param name="timeInSeconds">Time in seconds</param>
        /// <returns>Formatted time</returns>
        public static string TotalScanTime(double timeInSeconds)
        {
            if (double.IsNaN(timeInSeconds) || double.IsInfinity(timeInSeconds) || timeInSeconds <= 0)
            {
                return string.Empty;
            }

            TimeSpan duration = TimeSpan.FromSeconds(timeInSeconds);
            string strFormattedTime = string.Empty;
            if (duration.Days > 0)
            {
                //strFormattedTime = string.Format("{0:dd\\.hh\\:mm\\:ss}", duration);
                strFormattedTime = duration.ToString(@"dd\.hh\:mm\:ss");
            }
            else
            {
                //strFormattedTime = string.Format("{0:hh\\:mm\\:ss}", duration);
                strFormattedTime = duration.ToString(@"hh\:mm\:ss");
            }
            return strFormattedTime;
        }

        /*public static string GetLaserWaveLength(ImagingSystem.LaserType laserType)
        {
            string result = string.Empty;
            // No longer reading from the config file
            //if (SettingsManager.ConfigSettings.LasersWavelength != null && SettingsManager.ConfigSettings.LasersWavelength.Count > 0)
            //{
            //    foreach (var wavelength in SettingsManager.ConfigSettings.LasersWavelength)
            //    {
            //        if (wavelength.LaserType == laserType)
            //        {
            //            string[] values = wavelength.Wavelength.Split('/');
            //            result = values[0].Trim();
            //            break;
            //        }
            //    }
            //}
            //else if (SettingsManager.ConfigSettings.DyeOptions != null && SettingsManager.ConfigSettings.DyeOptions.Count > 0)
            //{
            //    foreach (var dye in SettingsManager.ConfigSettings.DyeOptions)
            //    {
            //        if (dye.LaserType == laserType)
            //        {
            //            string[] values = dye.WaveLength.Split('/');
            //            result = values[0].Trim();
            //            break;
            //        }
            //    }
            //}
            int nLaserWavelength = (int)laserType;
            result = nLaserWavelength.ToString();

            if (Workspace.This != null)
            {
                int nWavelength_N100 = ((int)(nLaserWavelength / 100)) * 100;
                int nWavelengthL1 = Workspace.This.LaserL1;
                int nWavelengthR1 = Workspace.This.LaserR1;
                int nWavelengthR2 = Workspace.This.LaserR2;
                int nWavelengthL1_N100 = ((int)(nWavelengthL1 / 100)) * 100;
                int nWavelengthR1_N100 = ((int)(nWavelengthR1 / 100)) * 100;
                int nWavelengthR2_N100 = ((int)(nWavelengthR2 / 100)) * 100;
                if (nWavelength_N100 == nWavelengthL1_N100)
                    result = nWavelengthL1.ToString();
                else if (nWavelength_N100 == nWavelengthR1_N100)
                    result = nWavelengthR1.ToString();
                else if (nWavelength_N100 == nWavelengthR2_N100)
                    result = nWavelengthR2.ToString();
            }

            return result;
        }*/

        /// <summary>
        /// L1 = Channnel A, R1 = Channel B, R2 = Channel C
        /// </summary>
        /// <param name="waveLength"></param>
        /// <returns></returns>
        public static LaserChannels GetLaserChannel(int waveLength)
        {
            LaserChannels laserCH = LaserChannels.ChannelA;

            if (Workspace.This.LaserL1 == waveLength)
                laserCH = LaserChannels.ChannelA;
            else if (Workspace.This.LaserR1 == waveLength)
                laserCH = LaserChannels.ChannelB;
            else if (Workspace.This.LaserR2 == waveLength)
                laserCH = LaserChannels.ChannelC;

            return laserCH;
        }

        /// <summary>
        /// L1 = Laser channel C
        /// R1 = Laser channel A
        /// R2 = laser channel B
        /// </summary>
        /// <param name="waveLength"></param>
        /// <returns></returns>
        public static LaserChannels GetLaserChannel(string waveLength)
        {
            LaserChannels laserCH = LaserChannels.ChannelA;
            int nWaveLength = 0;
            int nWaveLength_N100 = 0;
            // Get wavelength from wavelength + bandpass
            var strWavelength = waveLength.Split('/');
            if (strWavelength != null && strWavelength.Length > 0)
            {
                int.TryParse(strWavelength[0], out nWaveLength);
                nWaveLength_N100 = ((int)(nWaveLength / 100.0)) * 100;
            }

            int laserL1_N100 = ((int)(Workspace.This.LaserL1 / 100.0)) * 100;   //C channel
            int laserR1_N100 = ((int)(Workspace.This.LaserR1 / 100.0)) * 100;   //A channel
            int laserR2_N100 = ((int)(Workspace.This.LaserR2 / 100.0)) * 100;   //B channel

            if (laserL1_N100 == nWaveLength_N100)
                laserCH = LaserChannels.ChannelC;
            else if (laserR1_N100 == nWaveLength_N100)
                laserCH = LaserChannels.ChannelA;
            else if (laserR2_N100 == nWaveLength_N100)
                laserCH = LaserChannels.ChannelB;

            return laserCH;
        }

        public static bool IsPhosphorModule(LaserModule laserModule)
        {
            bool bIsPhosphorModule = false;

            if (laserModule.LaserWavelength == 638 ||
                laserModule.LaserWavelength == 658 ||
                laserModule.LaserWavelength == 685)
            {
                if (laserModule.SensorType == IvSensorType.PMT)
                {
                    bIsPhosphorModule = true;
                }
            }
                return bIsPhosphorModule;
        }

        /// <summary>
        /// Get available RAM in megabytes.
        /// </summary>
        /// <returns></returns>
        public static double GetAvailableRAM()
        {
            System.Diagnostics.PerformanceCounter ramCounter = new System.Diagnostics.PerformanceCounter("Memory", "Available MBytes");
            return ramCounter.NextValue();
        }

        public static string GetFileSize(int scanRes, int deltaX, int deltaY)
        {
            string retValue = string.Empty;
            double dEstFileSize = 0;

            double dWidth = deltaX * 1000.0 / scanRes;
            double dHeight = deltaY * 1000.0 / scanRes;

            double dNumBytes = 2.0;
            dEstFileSize = dWidth * dHeight * dNumBytes;

            // Return in KB if less a MB
            // 1KB = 1024 bytes 
            // 1MB = 1024 * 1024
            if (dEstFileSize < (1024 * 1024))
            {
                int fileSizeInKB = (int)(dEstFileSize / 1024);
                retValue = string.Format("{0}KB", fileSizeInKB);
            }
            else
            {
                int fileSizeInMB = (int)(dEstFileSize / (1024 * 1024));
                retValue = string.Format("{0}MB", fileSizeInMB);
            }

            return retValue;
        }

    }
}
