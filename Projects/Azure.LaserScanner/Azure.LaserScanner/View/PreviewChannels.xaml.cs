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

namespace Azure.LaserScanner.View
{
    /// <summary>
    /// Interaction logic for PreviewChannels.xaml
    /// </summary>
    public partial class PreviewChannels : UserControl
    {
        public PreviewChannels()
        {
            InitializeComponent();
        }

        public Boolean IsContrastChannelAllowed
        {
            get { return (Boolean)GetValue(StateProperty); }
            set { this.SetValue(StateProperty, value); }
        }
        public static readonly DependencyProperty StateProperty = DependencyProperty.Register(
          "IsContrastChannelAllowed", typeof(Boolean), typeof(PreviewChannels), new PropertyMetadata(true));

        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
            "ButtonsOrientation", typeof(Orientation), typeof(PreviewChannels), new FrameworkPropertyMetadata(Orientation.Vertical));
        public Orientation ButtonsOrientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        public static readonly DependencyProperty ButtonsMarginProperty =
            DependencyProperty.Register("ButtonsMargin", typeof(Thickness), typeof(PreviewChannels), new PropertyMetadata(default(Thickness)));
        public Thickness ButtonsMargin
        {
            get { return (Thickness)GetValue(ButtonsMarginProperty); }
            set { SetValue(MarginProperty, value); }
        }
    }
}
