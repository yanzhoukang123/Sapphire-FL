using System;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using Azure.Image.Processing;
using Azure.LaserScanner.ViewModel;

namespace Azure.LaserScanner.View
{
    public partial class ImageInfoPanel : UserControl
    {
        public ImageInfoPanel()
        {
            InitializeComponent();
        }

        private void _ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            FileViewModel activeDocument = Workspace.This.ActiveDocument;
            if (activeDocument != null)
            {
                activeDocument.IsShowImageInfo = false;
                activeDocument.IsEditComment = false;
            }
        }
    }
}
