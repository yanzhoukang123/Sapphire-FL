using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Diagnostics;

namespace Azure.LaserScanner.SplashScreen
{
    /// <summary>
    /// Message listener, singlton pattern.
    /// Inherit from DependencyObject to implement DataBinding.
    /// </summary>
    public class MessageListener : DependencyObject
    {
        /// <summary>
        /// 
        /// </summary>
        private static MessageListener mInstance;

        /// <summary>
        /// 
        /// </summary>
        private MessageListener()
        {
        }

        /// <summary>
        /// Get MessageListener instance
        /// </summary>
        public static MessageListener Instance
        {
            get
            {
                if (mInstance == null)
                {
                    mInstance = new MessageListener();
                }

                return mInstance;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void ReceiveMessage(string message)
        {
            Message = message;
            Debug.WriteLine(Message);
            DispatcherHelper.DoEvents();
        }

        /// <summary>
        /// Get or set received message
        /// </summary>
        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(string), typeof(MessageListener), new UIPropertyMetadata(null));

        /// <summary>
        /// Get or set received progress message
        /// </summary>
        public string ProgressMessage
        {
            get { return (string)GetValue(ProgressMessageProperty); }
            set { SetValue(ProgressMessageProperty, value); }
        }

        public int ProgressPercentCompleted
        {
            get { return (int)GetValue(ProgressPercentCompletedProperty); }
            set { SetValue(ProgressPercentCompletedProperty, value); }
        }

        public void ReceiveProgressMessage(string message)
        {
            ProgressMessage = message;
            Debug.WriteLine(ProgressMessage);
            DispatcherHelper.DoEvents();
        }

        public void ReceiveProgressMessageAppend(string message)
        {
            ProgressMessage += Environment.NewLine + message;
            Debug.WriteLine(ProgressMessage);
            DispatcherHelper.DoEvents();
        }

        public void ReceiveProgressPercentCompleted(int percentCompleted)
        {
            ProgressPercentCompleted = percentCompleted;
            Debug.WriteLine(ProgressPercentCompleted);
            DispatcherHelper.DoEvents();
        }

        public static readonly DependencyProperty ProgressMessageProperty =
            DependencyProperty.Register("ProgressMessage", typeof(string), typeof(MessageListener), new UIPropertyMetadata(null));
        public static readonly DependencyProperty ProgressPercentCompletedProperty =
     DependencyProperty.Register("ProgressPercentCompleted", typeof(string), typeof(MessageListener), new UIPropertyMetadata(null));
    }
}
