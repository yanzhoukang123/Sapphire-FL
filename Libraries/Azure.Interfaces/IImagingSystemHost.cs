using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;

namespace Azure.Interfaces
{
   /// <summary>
   /// The IImagingSystemHost interface definition.
   /// </summary>
   public interface IImagingSystemHost 
   {
      /// <summary>
      /// Get the UI host's dispatcher object.
      /// </summary>
      Dispatcher HostDispatcher { get; }

      /// <summary>
      /// Show error dialog.
      /// </summary>
      /// <param name="header"></param>
      /// <param name="message"></param>
      void ShowErrorMessage(string header, string message);

      /// <summary>
      /// Invoke Action on UI thread (blocking call).
      /// </summary>
      /// <param name="action"></param>
      void InvokeOnUI(Action action);

      /// <summary>
      /// Invoke Action asynchronously on UI thread.
      /// </summary>
      /// <param name="action"></param>
      void BeginInvokeOnUI(Action action);

      /// <summary>
      /// Exit the application.
      /// </summary>
      void ExitApp();

   }


}
