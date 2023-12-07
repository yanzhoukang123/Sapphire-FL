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
    /// Interaction logic for FluorescencePanel.xaml
    /// </summary>
    public partial class FluorescencePanel : UserControl
    {
        //private bool _IsFluorescencePanelLoaded = false;

        public FluorescencePanel()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(FluorescencePanel_Loaded);
        }

        void FluorescencePanel_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext != null && DataContext is FluorescenceViewModel)
            {
                FluorescenceViewModel viewModel = DataContext as FluorescenceViewModel;
                if (viewModel != null)
                {
                    // Update the preview channel laser wavelength based on the detected/installed lasers.
                    if (Workspace.This.LaserChannelTypeList.ContainsKey(LaserChannels.ChannelC))
                        viewModel.ImagingVm.LaserL1Wavelength = Workspace.This.LaserChannelTypeList[LaserChannels.ChannelC].ToString(); //L1
                    if (Workspace.This.LaserChannelTypeList.ContainsKey(LaserChannels.ChannelA))
                        viewModel.ImagingVm.LaserR1Wavelength = Workspace.This.LaserChannelTypeList[LaserChannels.ChannelA].ToString(); //R1
                    if (Workspace.This.LaserChannelTypeList.ContainsKey(LaserChannels.ChannelB))
                        viewModel.ImagingVm.LaserR2Wavelength = Workspace.This.LaserChannelTypeList[LaserChannels.ChannelB].ToString(); //R2

                    //_IsFluorescencePanelLoaded = true;
                    // Load application protocols
                    viewModel.LoadAppProtocols(SettingsManager.ConfigSettings.Protocols);

                    // Set the current select protocol to the last item.
                    //if (viewModel.AppMethodOptions != null && viewModel.AppMethodOptions.Count > 0)
                    //{
                    //    viewModel.SelectedAppMethod = viewModel.AppMethodOptions.LastOrDefault();
                    //}
                }
            }
        }
    }
}
