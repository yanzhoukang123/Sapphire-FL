using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Azure.CommandLib
{
    public class ThreadBase
    {
        /// <summary>
        /// Command completed delegate
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="exitState"></param>
        public delegate void CommandCompletedHandler(ThreadBase sender, ThreadExitStat exitState);

        /// <summary>
        /// Command completed event.
        /// </summary>
        public event CommandCompletedHandler Completed;

        public enum ThreadExitStat { Abort, Error, None };

        #region Protected field/data...

        protected Thread _ThreadHandle;
        protected ThreadExitStat _ExitStat = ThreadExitStat.None;
        protected AutoResetEvent _AutoResetEvent = new AutoResetEvent(false);
        protected Object _CommandSyncObject = new Object();
        protected bool _IsOutOfMemory = false;
        protected Exception _Error = null;

        #endregion

        #region Public properties...

        public ThreadExitStat ExitStat
        {
            get { return _ExitStat; }
            set { _ExitStat = value; }
        }
        public bool IsAlive
        {
            get { return _ThreadHandle.IsAlive; }
        }
        public Thread ThreadHandle
        {
            get { return _ThreadHandle; }
        }
        public bool IsOutOfMemory
        {
            get { return _IsOutOfMemory; }
            set { _IsOutOfMemory = value; }
        }
        public Exception Error
        {
            get { return _Error; }
            set { _Error = value; }
        }
        public bool IsSimulationMode { get; set; }

        #endregion

        #region Virtual functions...

        public virtual void ThreadFunction()
        {
        }
        public virtual void Initialize()
        {
        }
        public virtual void Finish()
        {
        }
        public virtual void AbortWork()
        {
        }

        /// <summary>
        /// This method is provided to simulate the task instead of actually performing the  task.
        /// </summary>
        protected virtual void SimulateThreadFunction()
        {
        }

        #endregion

        public ThreadBase()
        {
        }

        void ThreadProc()
        {
            try
            {
                if (IsSimulationMode)
                {
                    SimulateThreadFunction();
                }
                else
                {
                    ThreadFunction();
                }
            }
            catch (ThreadAbortException ex)
            {
                ExitStat = ThreadExitStat.Abort;
                Error = ex;
            }
            catch (OutOfMemoryException ex)
            {
                _IsOutOfMemory = true;
                Error = ex;
                ExitStat = ThreadExitStat.Error;
            }
            catch (Exception ex)
            {
                ExitStat = ThreadExitStat.Error;
                Error = ex;
            }
            finally
            {
                _AutoResetEvent.Set();

                // Do some clean up
                Finish();

                if (Completed != null)
                {
                    Completed(this, ExitStat);
                }
            }
        }

        public void Abort()
        {
            _ExitStat = ThreadExitStat.Abort;
            if (_ThreadHandle != null && _ThreadHandle.IsAlive)
            {
                AbortWork();
                _ThreadHandle.Abort();
            }
        }

        public void Start()
        {
            if (_ThreadHandle != null && _ThreadHandle.IsAlive)
            {
                throw new InvalidOperationException("Command instance is already executing asynchronously.");
            }

            _AutoResetEvent.Reset();

            Initialize();

            _ThreadHandle = new System.Threading.Thread(new System.Threading.ThreadStart(ThreadProc));
            _ThreadHandle.SetApartmentState(System.Threading.ApartmentState.STA);
            _ThreadHandle.Priority = ThreadPriority.Highest;
            _ThreadHandle.IsBackground = true;
            _ThreadHandle.Start();
        }

        public void Stop()
        {
            if (_ThreadHandle != null && _ThreadHandle.IsAlive)
            {
                AbortWork();
                _ThreadHandle.Abort();
            }
        }
    }
}
