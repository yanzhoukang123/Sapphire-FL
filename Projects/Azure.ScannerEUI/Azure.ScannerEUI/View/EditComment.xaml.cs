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
using System.Windows.Shapes;
using Azure.ScannerEUI.ViewModel;

namespace Azure.ScannerEUI.View
{
    /// <summary>
    /// Interaction logic for EditComment.xaml
    /// </summary>
    public partial class EditComment : UserControl
    {
        public EditComment()
        {
            InitializeComponent();
        }

        private void EditComment_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.IsVisible == true)
            {
                FileViewModel activeDocument = Workspace.This.ActiveDocument;
                if (activeDocument != null)
                {
                    _CommentTextBox.Text = activeDocument.ImageInfoComment;
                }
            }

        }

        private void _OkButton_Click(object sender, RoutedEventArgs e)
        {
            FileViewModel activeDocument = Workspace.This.ActiveDocument;
            if (activeDocument != null)
            {
                activeDocument.ImageInfoComment = _CommentTextBox.Text;
                activeDocument.IsEditComment = false;
            }
        }

        private void _CancelButton_Click(object sender, RoutedEventArgs e)
        {
            FileViewModel activeDocument = Workspace.This.ActiveDocument;
            if (activeDocument != null)
            {
                activeDocument.IsEditComment = false;
            }
        }
    }
}
