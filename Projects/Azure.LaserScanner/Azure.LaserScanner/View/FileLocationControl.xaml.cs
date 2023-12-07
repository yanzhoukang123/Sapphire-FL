using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;   // Regex
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Azure.LaserScanner.ViewModel;
using Azure.Configuration.Settings;

namespace Azure.LaserScanner.View
{
    /// <summary>
    /// Interaction logic for FileLocationControl.xaml
    /// </summary>
    public partial class FileLocationControl : UserControl
    {
        public FileLocationControl()
        {
            InitializeComponent();
            Loaded += new RoutedEventHandler(FileLocationControl_Loaded);
        }

        private void FileLocationControl_Loaded(object sender, RoutedEventArgs e)
        {
            string defaultFileName = Workspace.This.GenerateFileName(string.Empty, "");
            string destinationFolder = SettingsManager.ApplicationSettings.InitialDirectory;

            if (String.IsNullOrEmpty(destinationFolder))
            {
                string commonPictureFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string appCommonPictureFolder = commonPictureFolder + "\\" + Workspace.This.ProductName;
                destinationFolder = appCommonPictureFolder;
            }

            if (Workspace.This.SelectedImagingType == ImagingSystem.ImagingType.Fluorescence ||
                Workspace.This.SelectedImagingType == ImagingSystem.ImagingType.PhosphorImaging)
            {
                if (Workspace.This.SelectedImagingType == ImagingSystem.ImagingType.Fluorescence)
                {
                    if (Workspace.This.FluorescenceVM.SelectedAppProtocol != null && Workspace.This.FluorescenceVM.SelectedAppProtocol.SelectedScanRegion != null)
                    {
                        int nScanRegions = Workspace.This.FluorescenceVM.SelectedAppProtocol.ScanRegions.Count;
                        string fileName = defaultFileName;
                        for (int i = 0; i < nScanRegions; i++)
                        {
                            if (nScanRegions > 1)
                            {
                                fileName = string.Format("{0}_SR{1}", defaultFileName, Workspace.This.FluorescenceVM.SelectedAppProtocol.ScanRegions[i].ScanRegionNum);
                            }
                            Workspace.This.FluorescenceVM.SelectedAppProtocol.ScanRegions[i].FileLocationVm.FileName = fileName;
                            if (!Workspace.This.FluorescenceVM.SelectedAppProtocol.ScanRegions[i].FileLocationVm.IsAutoSave ||
                                string.IsNullOrEmpty(Workspace.This.FluorescenceVM.SelectedAppProtocol.ScanRegions[i].FileLocationVm.DestinationFolder))
                            {
                                Workspace.This.FluorescenceVM.SelectedAppProtocol.ScanRegions[i].FileLocationVm.DestinationFolder = destinationFolder;
                            }
                        }
                    }
                }
                else
                {
                    if (Workspace.This.PhosphorVM.SelectedAppProtocol != null && Workspace.This.PhosphorVM.SelectedAppProtocol.SelectedScanRegion != null)
                    {
                        int nScanRegions = Workspace.This.PhosphorVM.SelectedAppProtocol.ScanRegions.Count;
                        string fileName = defaultFileName;
                        for (int i = 0; i < Workspace.This.PhosphorVM.SelectedAppProtocol.ScanRegions.Count; i++)
                        {
                            if (nScanRegions > 1)
                            {
                                fileName = string.Format("{0}_SR{1}", defaultFileName, Workspace.This.PhosphorVM.SelectedAppProtocol.ScanRegions[i].ScanRegionNum);
                            }
                            Workspace.This.PhosphorVM.SelectedAppProtocol.ScanRegions[i].FileLocationVm.FileName = fileName;
                            if (!Workspace.This.PhosphorVM.SelectedAppProtocol.ScanRegions[i].FileLocationVm.IsAutoSave ||
                                string.IsNullOrEmpty(Workspace.This.PhosphorVM.SelectedAppProtocol.ScanRegions[i].FileLocationVm.DestinationFolder))
                            {
                                Workspace.This.PhosphorVM.SelectedAppProtocol.ScanRegions[i].FileLocationVm.DestinationFolder = destinationFolder;
                            }
                        }
                    }
                }
            }
            else
            {
                if (DataContext != null && DataContext is FileLocationViewModel)
                {
                    FileLocationViewModel viewModel = DataContext as FileLocationViewModel;
                    if (viewModel != null)
                    {
                        ((FileLocationViewModel)DataContext).DestinationFolder = destinationFolder;
                        ((FileLocationViewModel)DataContext).FileName = defaultFileName;
                    }
                }
            }

            //_FilePath = destinationFolder + "\\" + filename;
            //string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        /*private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            //string fileDir = System.IO.Path.GetDirectoryName(_FilePath);
            //Process.Start(@fileDir);

            // suppose that we have a test.txt at E:\
            //string filePath = @"E:\test.txt";

            FluorescenceViewModel viewModel = Workspace.This.FluorescenceVM;

            if (viewModel != null)
            {
                if (!System.IO.File.Exists(viewModel.FilePath))
                {
                    return;
                }

                // combine the arguments together
                // it doesn't matter if there is a space after ','
                string argument = "/select, \"" + viewModel.FilePath + "\"";

                System.Diagnostics.Process.Start("explorer.exe", argument);
            }
        }*/

        private void FileNameTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsValidFileName(e.Text);
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                TextBox tBox = (TextBox)sender;
                DependencyProperty prop = TextBox.TextProperty;

                BindingExpression binding = BindingOperations.GetBindingExpression(tBox, prop);
                if (binding != null)
                {
                    binding.UpdateSource();
                }
            }
        }

        /*private bool IsValidFileName(string text)
        {
            Regex containsABadCharacter = new Regex("["
                  + Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars())) + "]");
            if (containsABadCharacter.IsMatch(text)) { return false; };

            return true;
        }*/

        private bool IsValidFileName(string text)
        {
            string strTheseAreInvalidFileNameChars = new string(System.IO.Path.GetInvalidFileNameChars());
            Regex regInvalidFileName = new Regex("[" + Regex.Escape(strTheseAreInvalidFileNameChars) + "]");

            if (regInvalidFileName.IsMatch(text)) { return false; };

            return true;
        }

        /// <summary>
        ///  Update toggle image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonAutoSave_Checked(object sender, RoutedEventArgs e)
        {
            Uri source = null;
            BitmapImage img = null;

            if ((bool)buttonAutoSave.IsChecked)
            {
                source = new Uri("pack://application:,,,/Resources/Images/Imaging/btn_auto-save-selected.png");
            }
            else
            {
                source = new Uri("pack://application:,,,/Resources/Images/Imaging/btn_auto-save-default.png");
            }

            img = new BitmapImage(source);
            buttonAutoSave.Content = img;
        }
    }
}
