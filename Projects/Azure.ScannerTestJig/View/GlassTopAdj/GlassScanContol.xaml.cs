using Azure.ScannerTestJig.ViewModule;
using Azure.ScannerTestJig.ViewModule.GlassTopAdj;
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

namespace Azure.ScannerTestJig.View.GlassTopAdj
{
    /// <summary>
    /// GlassScanContol.xaml 的交互逻辑
    /// </summary>
    public partial class GlassScanContol : UserControl
    {
        public GlassScanContol()
        {
            InitializeComponent();
            GlassTopAdjustViewModel vm = Workspace.This.GlassAdjustWindVM;
            if (vm != null)
            {
                this.DataContext = vm;
            }
        }
    }
}
