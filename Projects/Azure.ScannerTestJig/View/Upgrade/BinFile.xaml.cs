using Azure.ScannerTestJig.ViewModule;
using Azure.ScannerTestJig.ViewModule.Upgrade;
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

namespace Azure.ScannerTestJig.View.Upgrade
{
    /// <summary>
    /// BinFileMerge.xaml 的交互逻辑
    /// </summary>
    public partial class BinFile : Window
    {
        public BinFile()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(IVContorl_Loaded);
        }
        void IVContorl_Loaded(object sender, EventArgs e)
        {

            UpgradeViewModule vm = Workspace.This.UpgradeViewModule;
            if (vm != null)
            {
                this.DataContext = vm;
                vm.InitIVControls();
            }
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // 只允许输入十六进制字符（0-9, A-F, a-f）
            if (!IsHexCharacter(e.Text))
            {
                e.Handled = true; // 拦截非法输入
            }
        }
        private bool IsHexCharacter(string text)
        {
            foreach (char c in text)
            {
                if (!Uri.IsHexDigit(c))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
