using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Azure.LaserScanner.ViewModel;

namespace Azure.LaserScanner.View
{
    /// <summary>
    /// Interaction logic for SignalControl.xaml
    /// </summary>
    public partial class SignalControl : UserControl
    {
        public SignalControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Update icon
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /*private void btnSequentialScanning_Checked(object sender, RoutedEventArgs e)
        {
            Uri source = null;
            BitmapImage img = null;
            ToggleButton btn = (ToggleButton)sender;

            if ((bool)btn.IsChecked)
            {
                source = new Uri("pack://application:,,,/Resources/Images/Imaging/btn_auto-save-selected.png");
            }
            else
            {
                source = new Uri("pack://application:,,,/Resources/Images/Imaging/btn_auto-save-default.png");
            }

            img = new BitmapImage(source);
            btn.Content = img;
        }*/

        /*private void btnEdrScanning_Checked(object sender, RoutedEventArgs e)
        {
            Uri source = null;
            BitmapImage img = null;
            ToggleButton btn = (ToggleButton)sender;

            if ((bool)btn.IsChecked)
            {
                source = new Uri("pack://application:,,,/Resources/Images/Imaging/btn_auto-save-selected.png");
            }
            else
            {
                source = new Uri("pack://application:,,,/Resources/Images/Imaging/btn_auto-save-default.png");
            }

            img = new BitmapImage(source);
            btn.Content = img;
        }*/

        // This is work-around for not allowing tab (scan region) switching while scanning.
        // Disabling the TabControl will also disable all its child control -
        // we want to allow the user to scroll the dyes and the scan settings while scanning.
        //private int _TabItemIndex = 0;
        /*private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.OriginalSource == tabControl)
            {
                if (!Workspace.This.IsScanning)
                {
                    _TabItemIndex = tabControl.SelectedIndex;
                }
                else
                {
                    bool bIsAllowTabItemChanged = false;
                    if (DataContext is FluorescenceViewModel)
                    {
                        bIsAllowTabItemChanged = ((FluorescenceViewModel)DataContext).SelectedAppMethod.IsAllowScanRegionChanged;
                    }
                    else if (DataContext is PhosphorViewModel)
                    {
                        bIsAllowTabItemChanged = ((FluorescenceViewModel)DataContext).SelectedAppMethod.IsAllowScanRegionChanged;
                    }

                    if (Workspace.This.IsScanning && bIsAllowTabItemChanged)
                    {
                        _TabItemIndex = tabControl.SelectedIndex;
                        if (DataContext is FluorescenceViewModel)
                        {
                            ((FluorescenceViewModel)DataContext).SelectedAppMethod.IsAllowScanRegionChanged = false;
                        }
                        else if (DataContext is PhosphorViewModel)
                        {
                            ((FluorescenceViewModel)DataContext).SelectedAppMethod.IsAllowScanRegionChanged = false;
                        }
                    }
                    else
                    {
                        // Don't allow tab switching while scanning
                        //
                        if (tabControl.SelectedIndex != _TabItemIndex)
                        {
                            e.Handled = true;
                            tabControl.SelectedIndex = _TabItemIndex;
                            //MessageBox.Show("Please Save or Cancel your work first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }*/
    }
}
