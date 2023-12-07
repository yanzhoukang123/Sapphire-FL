using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Azure.LaserScanner.SplashScreen
{
    /// <summary>
    /// Helper to show or close given splash window
    /// </summary>
    public static class Splasher
    {
        /// <summary>
        /// 
        /// </summary>
        private static Window mSplash;
        private static bool mIsSplashClosed;

        /// <summary>
        /// Get or set the splash screen window
        /// </summary>
        public static Window Splash
        {
            get
            {
                return mSplash;
            }
            set
            {
                mSplash = value;

                if (mSplash != null)
                {
                    mIsSplashClosed = false;
                }
            }
        }

        public static bool IsSplashClosed
        {
            get { return mIsSplashClosed; }
            set { mIsSplashClosed = value; }
        }

        /// <summary>
        /// Show splash screen
        /// </summary>
        public static void ShowSplash()
        {
            if (mSplash != null)
            {
                mSplash.Show();
            }
        }
        /// <summary>
        /// Close splash screen
        /// </summary>
        public static void CloseSplash()
        {
            if (mSplash != null)
            {
                mSplash.Close();

                if (mSplash is IDisposable)
                {
                    (mSplash as IDisposable).Dispose();
                }

                mIsSplashClosed = true;
            }
        }
    }
}
