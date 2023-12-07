using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CroppingImageLibrary.Services;

namespace CroppingImageLibrary
{
    /// <summary>
    /// Interaction logic for CropToolControl.xaml
    /// </summary>
    public partial class CropToolControl : UserControl
    {
        //public CropService CropService { get; private set; }
        //public ObservableCollection<CropService> CropService { get; set; }

        public static readonly DependencyProperty CropServiceProperty =
        DependencyProperty.Register(
            name: "CropService",
            propertyType: typeof(List<CropService>),
            ownerType: typeof(CropToolControl),
            typeMetadata: new FrameworkPropertyMetadata(defaultValue: null));

        public List<CropService> CropService
        {
            get { return (List<CropService>)GetValue(CropServiceProperty); }
            set { SetValue(CropServiceProperty, value); }
        }

        public CropToolControl()
        {
            InitializeComponent();
        }

        private void RootGrid_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //CropService.Adorner.RaiseEvent(e);
        }

        private void RootGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //CropService.Adorner.RaiseEvent(e);
        }

        private void RootGrid_Loaded(object sender, RoutedEventArgs e)
        {
            //CropService = new CropService(this);
        }

        public void SetImage(BitmapImage bitmapImage)
        {
            //SourceImage.Source = bitmapImage;
            //RootGrid.Height = bitmapImage.Height;
            //RootGrid.Width = bitmapImage.Width;
        }
    }
}
