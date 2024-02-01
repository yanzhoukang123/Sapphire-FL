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

namespace Azure.ScannerTestJig.View.LightModeCalibration
{
    /// <summary>
    /// Login.xaml 的交互逻辑
    /// </summary>
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string txtPsw = pwd.Password;
            if (string.Empty == txtPsw)
            {
                return;
            }
            if (txtPsw != "123456")
            {
                MessageBox.Show("密码错误");
                return;
            }
            this.Hide();
            LightModeHideSettingSub lmcs = new LightModeHideSettingSub();
            lmcs.ShowDialog();
        }
    }
}
