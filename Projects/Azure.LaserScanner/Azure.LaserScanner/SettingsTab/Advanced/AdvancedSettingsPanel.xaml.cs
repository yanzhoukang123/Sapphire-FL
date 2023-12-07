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
using System.Text.RegularExpressions;   // Regex
using System.Timers;

namespace Azure.LaserScanner
{
    /// <summary>
    /// Interaction logic for GeneralSettingsView.xaml
    /// </summary>
    public partial class AdvancedSettingsPanel : UserControl
    {
        public AdvancedSettingsPanel()
        {
            InitializeComponent();
            AddHandler(Keyboard.KeyDownEvent, (KeyEventHandler)HandleKeyDownEvent);
        }

        private void HandleKeyDownEvent(object sender, KeyEventArgs e)
        {
            // Trap CTRL + SHIFT + P
            if (e.Key == Key.P && (Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift)) == (ModifierKeys.Control | ModifierKeys.Shift))
            {
                if (this.DataContext != null)
                {
                    if (this.DataContext is AdvancedSettingsViewModel)
                    {
                        var viewModel = this.DataContext as AdvancedSettingsViewModel;
                        if (viewModel != null)
                        {
                            if (viewModel.IsEnablePhosphor)
                            {
                                viewModel.IsEnablePhosphor = false;
                            }
                            else
                            {
                                viewModel.IsEnablePhosphor = true;
                            }
                        }
                    }
                }

            }
        }
    }
}
