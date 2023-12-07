using Azure.EthernetCommLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Azure.MotionLib
{
    public class MotionController
    {
        #region Events
        public delegate void UpdateQueryHandle();
        public event UpdateQueryHandle OnQueryUpdated;
        #endregion Events

        #region Private Fields
        private EthernetController _CommController;
        //private Thread _QueryThread;
        private System.Timers.Timer _QueryTimer;
        #endregion Private Fields

        public MotionController(EthernetController commController)
        {
            _CommController = commController;
            _CommController.OnReceivedMotionData += _CommController_OnReceivedMotionData;

            if (_CommController.IsConnected)
            {
                //_QueryThread = new Thread(_QueryTimer_Elapsed);
                //_QueryThread.IsBackground = true;
                //_QueryThread.Start();
                _QueryTimer = new System.Timers.Timer(1000);
                _QueryTimer.Elapsed += new System.Timers.ElapsedEventHandler(QueryTimer_Elapsed);
                _QueryTimer.Start();
            }

            CrntPositions = new Dictionary<MotorTypes, int>();
            CrntState = new Dictionary<MotorTypes, MotionState>();
            StartSpeeds = new Dictionary<MotorTypes, int>();
            TopSpeeds = new Dictionary<MotorTypes, int>();
            Accelerations = new Dictionary<MotorTypes, int>();
            Decelerations = new Dictionary<MotorTypes, int>();

            CrntPositions.Add(MotorTypes.X, 0);
            CrntPositions.Add(MotorTypes.Y, 0);
            CrntPositions.Add(MotorTypes.Z, 0);
            CrntPositions.Add(MotorTypes.W, 0);
            CrntState.Add(MotorTypes.X, new MotionState());
            CrntState.Add(MotorTypes.Y, new MotionState());
            CrntState.Add(MotorTypes.Z, new MotionState());
            CrntState.Add(MotorTypes.W, new MotionState());
            StartSpeeds.Add(MotorTypes.X, 0);
            StartSpeeds.Add(MotorTypes.Y, 0);
            StartSpeeds.Add(MotorTypes.Z, 0);
            StartSpeeds.Add(MotorTypes.W, 0);
            TopSpeeds.Add(MotorTypes.X, 0);
            TopSpeeds.Add(MotorTypes.Y, 0);
            TopSpeeds.Add(MotorTypes.Z, 0);
            TopSpeeds.Add(MotorTypes.W, 0);
            Accelerations.Add(MotorTypes.X, 0);
            Accelerations.Add(MotorTypes.Y, 0);
            Accelerations.Add(MotorTypes.Z, 0);
            Accelerations.Add(MotorTypes.W, 0);
            Decelerations.Add(MotorTypes.X, 0);
            Decelerations.Add(MotorTypes.Y, 0);
            Decelerations.Add(MotorTypes.Z, 0);
            Decelerations.Add(MotorTypes.W, 0);
        }

        //private void _QueryTimer_Elapsed()
        //{
        //    while (_CommController.IsConnected)
        //    {
        //        if (AutoQuery)
        //        {
        //            GetMotionInfo(MotorTypes.X | MotorTypes.Y | MotorTypes.Z | MotorTypes.W);
        //        }
        //        Thread.Sleep(1000);
        //    }
        //}

        void QueryTimer_Elapsed(object sender, EventArgs e)
        {
            if (_CommController.IsConnected)
            {
                if (AutoQuery)
                {
                    GetMotionInfo(MotorTypes.X | MotorTypes.Y | MotorTypes.Z | MotorTypes.W);
                }
            }
        }

        private void _CommController_OnReceivedMotionData()
        {
            CrntPositions[MotorTypes.X] = _CommController.MotionCrntPositions[MotorTypes.X];
            CrntPositions[MotorTypes.Y] = _CommController.MotionCrntPositions[MotorTypes.Y];
            CrntPositions[MotorTypes.Z] = _CommController.MotionCrntPositions[MotorTypes.Z];
            CrntPositions[MotorTypes.W] = _CommController.MotionCrntPositions[MotorTypes.W];
            CrntState[MotorTypes.X] = _CommController.MotionStates[MotorTypes.X];
            CrntState[MotorTypes.Y] = _CommController.MotionStates[MotorTypes.Y];
            CrntState[MotorTypes.Z] = _CommController.MotionStates[MotorTypes.Z];
            CrntState[MotorTypes.W] = _CommController.MotionStates[MotorTypes.W];

            OnQueryUpdated?.Invoke();
        }

        #region Public Properties
        public Dictionary<MotorTypes, int> CrntPositions { get; }
        public Dictionary<MotorTypes, MotionState> CrntState { get; }
        public Dictionary<MotorTypes, int> StartSpeeds { get; }
        public Dictionary<MotorTypes, int> TopSpeeds { get; }
        public Dictionary<MotorTypes, int> Accelerations { get; }
        public Dictionary<MotorTypes, int> Decelerations { get; }
        public bool AutoQuery { get; set; }
        #endregion Public Properties

        #region Public Functions
        /// <summary>
        /// enable and home specified motions. typically used in initialization
        /// </summary>
        /// <param name="startSpeeds"></param>
        /// <param name="topSpeeds"></param>
        /// <param name="accVals"></param>
        /// <param name="dccVals"></param>
        /// <returns></returns>
        public bool HomeMotions(MotorTypes motions, int[] startSpeeds, int[] topSpeeds, int[] accVals, int[] dccVals)
        {
            if (SetEnables(motions, new bool[] { true, true, true, true }) == false) { return false; }
            if (SetMotionSpeedsAndAccs(motions, startSpeeds, topSpeeds, accVals, dccVals) == false)
            {
                return false;
            }
            if (SetHome(motions) == false) { return false; }
            return true;
        }

        /// <summary>
        /// get motion current position & status
        /// </summary>
        /// <param name="motions"></param>
        /// <returns></returns>
        public bool GetMotionInfo(MotorTypes motions)
        {
            try
            {
                byte motionVal = (byte)motions;
                AvocadoProtocol.SubSys motionSys = (AvocadoProtocol.SubSys)motionVal;
                byte[] frame = AvocadoProtocol.GetMotionPosAndState(motionSys);
                if (_CommController.SendBytes(frame) == false)
                {
                    return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool SetMotionPolarities(MotionSignalPolarity x, MotionSignalPolarity y, MotionSignalPolarity z, MotionSignalPolarity w)
        {
            try
            {
                byte[] frame = AvocadoProtocol.SetMotionPolarities(x.MapToByte(), y.MapToByte(), z.MapToByte(), w.MapToByte());
                if (_CommController.SendBytes(frame) == false) { return false; }
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool SetMotionSpeedsAndAccs(MotorTypes motions, int[] startSpeeds, int[] topSpeeds, int[] accVals, int[] dccVals)
        {
            try
            {
                byte motionVal = (byte)motions;
                AvocadoProtocol.SubSys motionSys = (AvocadoProtocol.SubSys)motionVal;
                byte[] frame = AvocadoProtocol.SetMotionSpeedsAndAccs(motionSys, startSpeeds, topSpeeds, accVals, dccVals);
                if (_CommController.SendBytes(frame) == false) { return false; }
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool SetEnables(MotorTypes motions, bool[] enables)
        {
            try
            {
                byte motionVal = (byte)motions;
                AvocadoProtocol.SubSys motionSys = (AvocadoProtocol.SubSys)motionVal;
                byte[] frame = AvocadoProtocol.SetMotionsEnable(motionSys, enables);
                if (_CommController.SendBytes(frame) == false) { return false; }
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool SetStart(MotorTypes motions, bool[] starts)
        {
            try
            {
                byte motionVal = (byte)motions;
                AvocadoProtocol.SubSys motionSys = (AvocadoProtocol.SubSys)motionVal;
                byte[] frame = AvocadoProtocol.SetMotionsStart(motionSys, starts);
                if (_CommController.SendBytes(frame) == false) { return false; }
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool SetHome(MotorTypes motions)
        {
            try
            {
                byte motionVal = (byte)motions;
                AvocadoProtocol.SubSys motionSys = (AvocadoProtocol.SubSys)motionVal;
                byte[] frame = AvocadoProtocol.HomeMotions(motionSys);
                if (_CommController.SendBytes(frame) == false) { return false; }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool SetMotionParameters(MotorTypes motions, int[] startSpeeds, int[] topSpeeds, int[] accVals, int[] dccVals,
                                                        int[] dccPosL, int[] tgtPosL, int[] dccPosR, int[] tgtPosR, int[] singleTripTimes, int[] repeats)
        {
            try
            {
                byte motionVal = (byte)motions;
                AvocadoProtocol.SubSys motionSys = (AvocadoProtocol.SubSys)motionVal;
                byte[] frame = AvocadoProtocol.SetMotionParameters(motionSys, startSpeeds, topSpeeds, accVals, dccVals,
                                                    dccPosL, tgtPosL, dccPosR, tgtPosR, singleTripTimes, repeats);
                if (_CommController.SendBytes(frame) == false) { return false; }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool HomeMotion(MotorTypes motions, int[] startSpeeds, int[] topSpeeds, int[] accVals, int[] dccVals, bool waitForcomplete)
        {
            bool containsX = (motions & MotorTypes.X) == MotorTypes.X;
            bool containsY = (motions & MotorTypes.Y) == MotorTypes.Y;
            bool containsZ = (motions & MotorTypes.Z) == MotorTypes.Z;
            bool containsW = (motions & MotorTypes.W) == MotorTypes.W;
            SetEnables(motions, new bool[] { true, true, true, true });
            // 1. stop the motion if possible
            SetStart(motions, new bool[] { false, false, false, false });
            if (containsX)
            {
                if(WaitMotionIdle(MotorTypes.X, 1000) == false) { return false; }
            }
            if (containsY)
            {
                if (WaitMotionIdle(MotorTypes.Y, 1000) == false) { return false; }
            }
            if (containsZ)
            {
                if (WaitMotionIdle(MotorTypes.Z, 1000) == false) { return false; }
            }
            if (containsW)
            {
                if (WaitMotionIdle(MotorTypes.W, 1000) == false) { return false; }
            }

            // 2. set motion parameters
            if (SetMotionSpeedsAndAccs(motions, startSpeeds, topSpeeds, accVals, dccVals) == false) { return false; }
            // 3. set home
            if (SetHome(motions) == false) { return false; }

            // 4. wait for motion complete if needed
            if (waitForcomplete)
            {
                int count = 0;
                if (GetMotionInfo(motions) == false) { return false; }
                bool completed = false;
                do
                {
                    if (count++ > 30000)        // wait 30 seconds before throwing exceptions
                    {
                        throw new Exception(string.Format("{0} waiting too long", motions.ToString()));
                    }

                    completed = true;
                    if (containsX && CrntState[MotorTypes.X].IsBusy) { completed = false; }
                    if (containsY && CrntState[MotorTypes.Y].IsBusy) { completed = false; }
                    if (containsZ && CrntState[MotorTypes.Z].IsBusy) { completed = false; }
                    if (containsW && CrntState[MotorTypes.W].IsBusy) { completed = false; }
                    Thread.Sleep(1);
                }
                while (completed == false);
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="motion"></param>
        /// <param name="startSpeed">unit: pulse/sec</param>
        /// <param name="topSpeed">unit: pulse/sec</param>
        /// <param name="accVal">unit: pulse/sec^2</param>
        /// <param name="dccVal">unit: pulse/sec^2</param>
        /// <param name="tgtPos">unit: pulse</param>
        /// <param name="waitForcomplete"></param>
        /// <returns></returns>
        public bool AbsoluteMoveSingleMotion(MotorTypes motion, int startSpeed, int topSpeed, int accVal, int dccVal, int tgtPos, bool startNow, bool waitForcomplete)
        {
            if (SetEnables(motion, new bool[] { true }) == false) { return false; }
            // 1. stop the motion if possible
            if (SetStart(motion, new bool[] { false }) == false) { return false; }
            int timeout = 1000;
            while (CrntState[motion].IsBusy)
            {
                if(timeout<= 0)
                {
                    return false;
                }
                Thread.Sleep(1);	//ONLY Thread.Sleep(1) does context switching
                timeout--;
            }
            if (CrntPositions[motion] == tgtPos) { return true; }

            // 2. calculate and set dcc position
            if (GetMotionInfo(motion) == false) { return false; }
            double crntPos = CrntPositions[motion];
            double accPos, dccPos;
            double speedSqureDiff = 1.0 * topSpeed * topSpeed - 1.0 * startSpeed * startSpeed;
            double dir = tgtPos > crntPos ? 1.0 : -1.0;
            accPos = crntPos + dir * speedSqureDiff / 2 / accVal;
            dccPos = tgtPos - dir * speedSqureDiff / 2 / dccVal;
            bool noTopSpeed = false;
            if ((tgtPos > crntPos) && (accPos > dccPos))
            {
                noTopSpeed = true;
            }
            else if ((tgtPos < crntPos) && (accPos < dccPos))
            {
                noTopSpeed = true;
            }
            if (noTopSpeed)
            {
                dccPos = (tgtPos * dccVal + CrntPositions[motion] * accVal) / (accVal + dccVal);
            }

            // 3. set parameters and start motion if startNow is true
            if (SetMotionParameters(motion, new int[] { startSpeed }, new int[] { topSpeed }, new int[] { accVal }, new int[] { dccVal },
                new int[] { (int)dccPos }, new int[] { tgtPos }, new int[] { 0 }, new int[] { 0 }, new int[] { 0 }, new int[] { 0 }) == false)
            {
                return false;
            }
            if (startNow)
            {
                if (SetStart(motion, new bool[] { true }) == false) { return false; }
                if (waitForcomplete)
                {
                    int count = 0;
                    if (GetMotionInfo(motion) == false) { return false; }
                    bool completed = false;
                    do
                    {
                        if (count++ > 30000)        // wait 30 seconds before throwing exceptions
                        {
                            throw new Exception(string.Format("{0} waiting too long", motion.ToString()));
                        }

                        completed = true;
                        if (CrntState[motion].IsBusy) { completed = false; }
                        Thread.Sleep(1);
                    }
                    while (completed == false);
                }
            }

            return true;
        }

        public bool AbsoluteMoveSingleMotion(MotorTypes motion, int startSpeed, int topSpeed, int accVal, int dccVal, int tgtPos1, int tgtPos2, int repeats, int singleTripTime, bool startNow, bool waitForComplete)
        {
            SetEnables(motion, new bool[] { true });
            // 1. stop the motion if possible
            SetStart(motion, new bool[] { false });
            while (CrntState[motion].IsBusy) { Thread.Sleep(1); }


            GetMotionInfo(motion);
            Thread.Sleep(100);
            double crntPos = CrntPositions[motion];

            // 2. calculate and set dcc position
            double accPos, dccPos1, dccPos2;
            double speedSqureDiff = 1.0 * topSpeed * topSpeed - 1.0 * startSpeed * startSpeed;
            double dir = tgtPos1 > crntPos ? 1.0 : -1.0;
            accPos = crntPos + dir * speedSqureDiff / 2 / accVal;
            dccPos1 = tgtPos1 - dir * speedSqureDiff / 2 / dccVal;
            bool noTopSpeed = false;
            if ((tgtPos1 > crntPos) && (accPos > dccPos1))
            {
                noTopSpeed = true;
            }
            else if ((tgtPos1 < crntPos) && (accPos < dccPos1))
            {
                noTopSpeed = true;
            }
            if (noTopSpeed)
            {
                dccPos1 = ((double)tgtPos1 * dccVal + (double)CrntPositions[motion] * accVal) / (accVal + dccVal);
            }
            // 3. set turn back dcc position and target position
            dir = tgtPos2 > tgtPos1 ? 1.0 : -1.0;
            accPos = tgtPos1 + dir * speedSqureDiff / 2 / accVal;
            dccPos2 = tgtPos2 - dir * speedSqureDiff / 2 / dccVal;
            noTopSpeed = false;
            if ((tgtPos2 > tgtPos1) && (accPos > dccPos2))
            {
                noTopSpeed = true;
            }
            else if ((tgtPos2 < tgtPos1) && (accPos < dccPos2))
            {
                noTopSpeed = true;
            }
            if (noTopSpeed)
            {
                dccPos2 = ((double)tgtPos2 * dccVal + (double)tgtPos1 * accVal) / (accVal + dccVal);
            }
            // 4. set parameters
            SetMotionParameters(motion, new int[] { startSpeed }, new int[] { topSpeed }, new int[] { accVal }, new int[] { dccVal },
                new int[] { (int)dccPos1 }, new int[] { tgtPos1 }, new int[] { (int)dccPos2 }, new int[] { tgtPos2 }, new int[] { singleTripTime }, new int[] { repeats });

            // 7. start motion
            if (startNow)
            {
                if (SetStart(motion, new bool[] { true }) == false) { return false; }

                // 8. wait for motion complete if needed
                if (waitForComplete)
                {
                    GetMotionInfo(motion);
                    int tryCounts = 0;
                    do
                    {
                        if (tryCounts++ > 100000)
                        {
                            return false;
                        }
                        Thread.Sleep(1);
                    }
                    while (CrntState[motion].IsBusy);
                }
            }
            return true;
        }
        #endregion Public Functions

        #region Private Functions
        private bool WaitMotionIdle(MotorTypes motion, int timeoutInMsec)
        {
            int cnt = 0;
            do
            {
                if (++cnt > timeoutInMsec)
                {
                    return false;
                }
                Thread.Sleep(1);
            }
            while (CrntState[motion].IsBusy);
            return true;
        }
        #endregion Private Functions
    }

    public class MotionSignalPolarity
    {
        public bool ClkPolar { get; set; }
        public bool DirPolar { get; set; }
        public bool EnaPolar { get; set; }
        public bool FwdLmtPolar { get; set; }
        public bool BwdLmtPolar { get; set; }
        public bool HomePolar { get; set; }

        public byte MapToByte()
        {
            byte result = 0;
            result = (byte)(result | (ClkPolar ? 0x20 : 0x00));
            result = (byte)(result | (DirPolar ? 0x10 : 0x00));
            result = (byte)(result | (EnaPolar ? 0x08 : 0x00));
            result = (byte)(result | (FwdLmtPolar ? 0x04 : 0x00));
            result = (byte)(result | (BwdLmtPolar ? 0x02 : 0x00));
            result = (byte)(result | (HomePolar ? 0x01 : 0x00));
            return result;
        }

        public void MapFromByte(byte polar)
        {
            ClkPolar = (polar & 0x20) == 0x20 ? true : false;
            DirPolar = (polar & 0x10) == 0x10 ? true : false;
            EnaPolar = (polar & 0x08) == 0x08 ? true : false;
            FwdLmtPolar = (polar & 0x04) == 0x04 ? true : false;
            BwdLmtPolar = (polar & 0x02) == 0x02 ? true : false;
            HomePolar = (polar & 0x01) == 0x01 ? true : false;
        }
    }
}
