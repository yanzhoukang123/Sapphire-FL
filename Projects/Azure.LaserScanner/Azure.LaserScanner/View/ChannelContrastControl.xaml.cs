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
using Azure.LaserScanner.ViewModel;

namespace Azure.LaserScanner.View
{
    /// <summary>
    /// Interaction logic for ChannelContrastControl.xaml
    /// </summary>
    public partial class ChannelContrastControl : UserControl
    {
        public ChannelContrastControl()
        {
            InitializeComponent();
        }

        public static DependencyProperty ChannelSelectHeaderProperty = DependencyProperty.Register("ChannelSelectHeader", typeof(string), typeof(ChannelContrastControl));

        public string ChannelSelectHeader
        {
            get { return (string)GetValue(ChannelSelectHeaderProperty); }
            set { SetValue(ChannelSelectHeaderProperty, value); }
        }

        public static DependencyProperty ChannelSelectVisibilityProperty = DependencyProperty.Register("ChannelSelectVisibility", typeof(Visibility), typeof(ChannelContrastControl));

        public Visibility ChannelSelectVisibility
        {
            get { return (Visibility)GetValue(ChannelSelectVisibilityProperty); }
            set { SetValue(ChannelSelectVisibilityProperty, value); }
        }

        //public static DependencyProperty ChannelSelectedProperty = DependencyProperty.Register("ChannelSelected", typeof(ColorChannel), typeof(ChannelContrastControl));

        //public ColorChannel ChannelSelected
        //{
        //    get { return (ColorChannel)GetValue(ChannelSelectedProperty); }
        //    set { SetValue(ChannelSelectedProperty, value); }
        //}

        /*public static readonly DependencyProperty ShowChannelSelectProperty =
            DependencyProperty.Register("IsShowChannelSelect",
                typeof(Boolean),
                typeof(ChannelContrastControl),
                new FrameworkPropertyMetadata(false));

        public bool IsShowChannelSelect
        {
            get { return (bool)GetValue(ShowChannelSelectProperty); }
            set { SetValue(ShowChannelSelectProperty, value); }
        }*/

    }
}
