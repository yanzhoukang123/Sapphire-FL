using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// Interaction logic for RotateArbitraryControl.xaml
    /// </summary>
    public partial class RotateArbitraryControl : UserControl
    {
        public event EventHandler ApplyButtonClicked;

        public RotateArbitraryControl()
        {
            InitializeComponent();
        }

        protected virtual void OnApplyButtonClicked(EventArgs e)
        {
            ApplyButtonClicked?.Invoke(this, e);
        }
        private void ApplyButton_Click(object sender, EventArgs e)
        {
            OnApplyButtonClicked(e);
        }
    }
}
