using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using System.Windows;

namespace Azure.ScannerEUI
{
    class LoadingAnimationManager
    {
        private Thread _Thread;
        private AnimationWindow _AnimationWindow;
        private Point _Location;
        private Size _Size;
        private string _DisplayText = string.Empty;

        public Thread Thread
        {
            get { return _Thread; }
        }

        public LoadingAnimationManager(Point location, Size size, string strDisplayText)
        {
            _Location = location;
            _Size = size;
            _DisplayText = strDisplayText;
        }

        public void BeginWaiting()
        {
            _Thread = new Thread(RunThread);
            _Thread.IsBackground = true;
            _Thread.SetApartmentState(ApartmentState.STA);
            _Thread.Name = "LoadingManager";
            _Thread.Start();

            // Loop until worker thread activates
            while (!_Thread.IsAlive) ;
        }

        public void EndWaiting()
        {
            if (this._AnimationWindow != null)
            {
                _AnimationWindow.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (Action)(() => { _AnimationWindow.Close(); }));
            }
        }

        public void RunThread()
        {
            // Create our context, and install it:
            SynchronizationContext.SetSynchronizationContext(
                new System.Windows.Threading.DispatcherSynchronizationContext(
                    System.Windows.Threading.Dispatcher.CurrentDispatcher));

            _AnimationWindow = new AnimationWindow();

            double childWidth = _AnimationWindow.Width;
            double childHeight = _AnimationWindow.Height;
            int x = (int)(((_Size.Width - childWidth) / 2) + _Location.X);
            int y = (int)(((_Size.Height - childHeight) / 2) + _Location.Y);

            _AnimationWindow.Left = x;
            _AnimationWindow.Top = y;

            _AnimationWindow.LoadingText = _DisplayText;

            // When the window closes, shut down the dispatcher
            _AnimationWindow.Closed += (s, e) =>
               Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.Background);

            _AnimationWindow.ShowDialog();

            // Start the Dispatcher Processing
            System.Windows.Threading.Dispatcher.Run();
        }
    }
}
