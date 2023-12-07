using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.EthernetCommLib;
using Azure.WPF.Framework;

namespace Azure.LaserScanner.ViewModel
{
    public class LaserTemperature
    {
        private System.Timers.Timer _TemperatureTimer;
        private EthernetController _EthernetController;
        private Logger _Logger = null;
        private string _LogFilePath;
        private string _LogFileName = "Temperature.log";
        private string _AppDataPath;

        public LaserTemperature()
        {
        }

        public EthernetController EthernetDevice
        {
            get { return _EthernetController; }
            set { _EthernetController = value; }
        }
        public double SensorTemperatureL1 { get; set; }
        public double SensorTemperatureR1 { get; set; }
        public double SensorTemperatureR2 { get; set; }
        public double SensorRadTemperaTureL1 { get; set; }
        public double SensorRadTemperaTureR1 { get; set; }
        public double SensorRadTemperaTureR2 { get; set; }
        public bool IsWriteToLogFileEnabled { get; set; }
        public int Interval { get; set; } = 30000;
        public bool IsLaserSensorLoggingEnabled { get; set; }
        public string LogFileName
        {
            get { return _LogFileName; }
            set { _LogFileName = value; }
        }
        public string AppDataPath
        {
            get { return _AppDataPath; }
            set { _AppDataPath = value; }
        }

        public bool Start()
        {
            if (_EthernetController != null && _EthernetController.IsConnected)
            {
                if (IsWriteToLogFileEnabled)
                {
                    LoggerSetup();

                    LogMessage("***** Laser Sensor Temperature *****");
                    LogMessage("************************************");
                }
                if (_TemperatureTimer == null)
                {
                    _TemperatureTimer = new System.Timers.Timer();
                    _TemperatureTimer.Interval = Interval;
                    _TemperatureTimer.Elapsed += TemperatureTimer_Elapsed;
                }
                _TemperatureTimer.Start();
                IsLaserSensorLoggingEnabled = true;
                return true;
            }
            else
            {
                return false;
            }
        }
        public void Stop()
        {
            if (_TemperatureTimer != null)
            {
                _TemperatureTimer.Elapsed -= TemperatureTimer_Elapsed;
                _TemperatureTimer.Stop();
                IsLaserSensorLoggingEnabled = false;
            }
        }

        private void TemperatureTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                _EthernetController.GetAllLaserTemperatures();
                SensorTemperatureL1 = _EthernetController.LaserTemperatures[LaserChannels.ChannelC];
                SensorTemperatureR1 = _EthernetController.LaserTemperatures[LaserChannels.ChannelA];
                SensorTemperatureR2 = _EthernetController.LaserTemperatures[LaserChannels.ChannelB];
                _EthernetController.GetAllRadiatorTemperatures();
                SensorRadTemperaTureL1 = _EthernetController.RadiatorTemperatures[LaserChannels.ChannelC];
                SensorRadTemperaTureR1 = _EthernetController.RadiatorTemperatures[LaserChannels.ChannelA];
                SensorRadTemperaTureR2 = _EthernetController.RadiatorTemperatures[LaserChannels.ChannelB];

                if (IsWriteToLogFileEnabled)
                {
                    string laserTemp = string.Format("Laser Temperature:    L: {0,4}, R1: {1,4}, R2: {2,4}", SensorTemperatureL1, SensorTemperatureR1, SensorTemperatureR2);
                    string radiatorTemp = string.Format("Radiator Temperature: L: {0,4}, R1: {1,4}, R2: {2,4}", SensorRadTemperaTureL1, SensorRadTemperaTureR1, SensorRadTemperaTureR2);
                    //string temp = laserTemp + "\n" + radiatorTemp;
                    LogMessage(laserTemp);
                    LogMessage(radiatorTemp);
                }
            }
            catch (Exception ex)
            {
                LogException("Laser Temperature", ex);
            }
        }

        public void LoggerSetup()
        {
            //
            // Create logger
            //
            _LogFilePath = Path.Combine(AppDataPath, _LogFileName);
            _Logger = new Logger();
            _Logger.Open(_LogFilePath);
            _Logger.SuppressLoggingExceptionEvents = true;
            //_CommandMediator.SetLogger(_Logger);
            //_CommandMediator.Exception += new ExceptionDelegate(_CommandMediator_Exception);
            //_CommandMediator.StateChanged += new StateChangedDelegate(_CommandMediator_StateChanged);
            _Logger.LogAppConfigSettings();
        }
        public Logger Logger
        {
            get { return _Logger; }
        }

        /// <summary>
        /// Log a message to the attached logger. See method <see cref="SetLogger"/>
        /// to attach a logger.
        /// </summary>
        /// <param name="msg"></param>
        public void LogMessage(string msg)
        {
            if (_Logger == null) return;

            _Logger.LogMessage(msg);
        }

        /// <summary>
        /// Log an exception to the attached logger. See method <see cref="SetLogger"/>
        /// to attach a logger.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="ex"></param>
        internal void LogException(string header, Exception ex)
        {
            if (_Logger == null) return;

            _Logger.LogException(header, ex);
        }

    }
}
