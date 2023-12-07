using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Azure.Resources
{

   public class TabCloseRoutedEventHelper
   {
      // Using a DependencyProperty as the backing store for EnableRoutedClick.
      // This enables animation, styling, binding, etc...
      public static readonly RoutedEvent CloseTabEvent = EventManager.RegisterRoutedEvent("CloseTab",
         RoutingStrategy.Bubble, typeof(RoutedEvent), typeof(TabCloseRoutedEventHelper));

      public static readonly DependencyProperty EnableRoutedClickProperty = DependencyProperty.RegisterAttached(
          "EnableRoutedClick",
          typeof(bool),
          typeof(TabCloseRoutedEventHelper),
          new System.Windows.PropertyMetadata(OnEnableRoutedClickChanged));

      /// <summary>
      /// Add an attached property:
      /// </summary>
      /// <param name="obj"></param>
      /// <returns></returns>
      public static bool GetEnableRoutedClick(DependencyObject obj)
      {
         return (bool)obj.GetValue(EnableRoutedClickProperty);
      }

      public static void SetEnableRoutedClick(DependencyObject obj, bool value)
      {
         obj.SetValue(EnableRoutedClickProperty, value);
      }

      private static void OnEnableRoutedClickChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
      {
         var newValue = (bool)e.NewValue;
         var button = sender as Button;
         if (button == null)
            return;
         if (newValue)
            button.Click += new RoutedEventHandler(OnButtonClick);
      }

      static void OnButtonClick(object sender, RoutedEventArgs e)
      {
         var control = sender as Control;
         if (control != null)
         {
            control.RaiseEvent(new RoutedEventArgs(TabCloseRoutedEventHelper.CloseTabEvent, control));
         }
      }
   }


}
