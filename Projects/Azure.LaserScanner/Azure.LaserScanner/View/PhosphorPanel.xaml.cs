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
using Azure.Configuration.Settings;
using Azure.EthernetCommLib;
using Azure.LaserScanner.ViewModel;

namespace Azure.LaserScanner.View
{
    /// <summary>
    /// Interaction logic for PhosphorPanel.xaml
    /// </summary>
    public partial class PhosphorPanel : UserControl
    {
        //private bool _IsPhosphorPanelLoaded = false;

        public PhosphorPanel()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(PhosphorPanel_Loaded);
        }

        private void PhosphorPanel_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext != null && DataContext is PhosphorViewModel)
            {
                PhosphorViewModel viewModel = DataContext as PhosphorViewModel;
                if (viewModel != null)
                {
                    // Update the preview channel laser wavelength based on the detected/installed lasers.
                    if (Workspace.This.LaserChannelTypeList.ContainsKey(LaserChannels.ChannelC))
                        viewModel.ImagingVm.LaserL1Wavelength = Workspace.This.LaserChannelTypeList[LaserChannels.ChannelC].ToString(); //L1
                    if (Workspace.This.LaserChannelTypeList.ContainsKey(LaserChannels.ChannelA))
                        viewModel.ImagingVm.LaserR1Wavelength = Workspace.This.LaserChannelTypeList[LaserChannels.ChannelA].ToString(); //R1
                    if (Workspace.This.LaserChannelTypeList.ContainsKey(LaserChannels.ChannelB))
                        viewModel.ImagingVm.LaserR2Wavelength = Workspace.This.LaserChannelTypeList[LaserChannels.ChannelB].ToString(); //R2

                    //_IsPhosphorPanelLoaded = true;
                    // Load application protocols
                    viewModel.LoadAppProtocols(SettingsManager.ConfigSettings.PhosphorProtocols);
                }
            }
        }
    }
}
