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
using System.Windows.Shapes;

namespace Azure.Avocado.FwUpgrader
{
    /// <summary>
    /// Interaction logic for PasswordPromptWindow.xaml
    /// </summary>
    public partial class PasswordPromptWindow : Window
    {
        public PasswordPromptWindow()
        {
            InitializeComponent();
        }

        private void _Btn_Click(object sender, RoutedEventArgs e)
        {
            if(_PwdBox.Password == "hywire128")
            {
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                _IndicatorText.Text = "Incorrect Password.";
            }
        }
    }
}
