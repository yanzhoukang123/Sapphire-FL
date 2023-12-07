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

namespace Azure.LaserScanner.View
{
    /// <summary>
    /// Interaction logic for ImageInfoControl.xaml
    /// </summary>
    public partial class ImageInfoControl : UserControl
    {
        public ImageInfoControl()
        {
            InitializeComponent();
        }

        private void addEditBtn_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (DataContext != null)
                {
                    if (DataContext is Azure.LaserScanner.ViewModel.Workspace)
                    {
                        if (Azure.LaserScanner.ViewModel.Workspace.This.ActiveDocument != null)
                        {
                            if (Azure.LaserScanner.ViewModel.Workspace.This.IsEditDynamicBitAllowed)
                                Azure.LaserScanner.ViewModel.Workspace.This.IsEditDynamicBitAllowed = false;
                            else
                                Azure.LaserScanner.ViewModel.Workspace.This.IsEditDynamicBitAllowed = true;
                        }
                    }
                }
            }
        }
    }
}
