using Azure.EthernetCommLib;
using Azure.ScannerEUI.ViewModel;
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

namespace Azure.ScannerEUI.View
{
    /// <summary>
    /// ScanWorkUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class ScanWorkUserControl : UserControl
    {
        public ScanRegionUserControl OverAllRegion = null;
        ScanRegionUserControl SRUl = null;
        public ScanWorkUserControl()
        { 
            InitializeComponent();
            Workspace.This.ScannerVM.OnScanEnbledRegionReceived += ZAutomaticallyFocalVM_OnScanEnbledRegionReceived1;
            Workspace.This.ScannerVM.OnScanRegionReceived += ScannerVM_OnScanRegionReceived;
            Workspace.This.ScannerVM.OnScanWorkReceived += SetCurrentRegion;
            Workspace.This.ScannerVM.GetRegionScanWorkReceived += ScannerVM_GetRegionScanWorkReceived;
            Workspace.This.IVVM.OnScanRegionReceived += IVVM_OnScanRegionReceived;
            Workspace.This.ScannerVM.SetRegionScanWorkLaserReceived += ScannerVM_SetRegionScanWorkLaserReceived;
            Workspace.This.ZAutomaticallyFocalVM.OnScanRegionReceived += ZAutomaticallyFocalVM_OnScanRegionZStackReceived;
        }

        /// <summary>
        /// 关闭当前ROI的激光
        /// Turn off the current ROI laser
        /// </summary>
        /// <param name="Current">RIO index</param>
        private void ScannerVM_SetRegionScanWorkLaserReceived(int Current)
        {
            Workspace.This.Owner.Dispatcher.BeginInvoke((Action)delegate
            {
                foreach (ScanRegionUserControl uc in canvas1.Children)
                {
                    if ((int)uc.Tag == Current)
                    {
                        OverAllRegion = uc;
                        IsSetRegion();
                    }
                }
            });
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="Current"></param>
        private void ScannerVM_GetRegionScanWorkReceived(int Current)
        {
            Workspace.This.Owner.Dispatcher.BeginInvoke((Action)delegate
            {
                foreach (ScanRegionUserControl uc in canvas1.Children)
                {
                    if ((int)uc.Tag == Current)
                    {
                        OverAllRegion = uc;
                        IsGetRegion();
                    }
                }
            });
        }

        /// <summary>
        /// 是否启用当前ROI，Z-Stack
        /// Turn off the current ROI laser
        /// </summary>
        /// <param name="isEnable"></param>
        private void ZAutomaticallyFocalVM_OnScanEnbledRegionReceived1(bool isEnable)
        {
            Workspace.This.Owner.Dispatcher.BeginInvoke((Action)delegate
            {
                this.IsEnabled = isEnable;
                btnDown.IsEnabled = isEnable;
                btnUp.IsEnabled = isEnable;

            });

        }
        /// <summary>
        /// 创建ROI到列表最后
        /// Create the ROI to the end of the list
        /// </summary>
        int index = 0;
        private void CreateControl()
        {
            int Count = canvas1.Children.Count;
            if (Count > 9)
            {
                return;
            }
            bool IsScanScopelimitations = false;
            //Calculate the scanning range of X and Y, and if it exceeds the range, do not proceed further
            //计算X和Y的扫描范围，如果超出范围就不往下执行了
            Workspace.This.ScannerVM.ScanScopelimitations(ref IsScanScopelimitations);
            if (!IsScanScopelimitations)
            {
                return;
            }
            index++;
            SRUl = new ScanRegionUserControl();
            SRUl.Width = 55;
            SRUl.Height = 55;
            if (canvas1.Children.Count > 0)
                SRUl.IndexCount = canvas1.Children.Count + 1;
            else
                SRUl.IndexCount = index;
            SRUl.SetLbNumber();
            SRUl.OnScanDataReceived += Bt_OnScanDataReceived;
            canvas1.Children.Add(SRUl);
            index++;
            Workspace.This.ScannerVM.ScanWorkCount = canvas1.Children.Count;
        }
        /// <summary>
        /// 删除最后一个ROI
        /// Delete the last ROI
        /// </summary>
        private void RemoveControl()
        {
            if (canvas1.Children.Count > 0)
            {
                canvas1.Children.RemoveAt(canvas1.Children.Count - 1);
            }
            if (canvas1.Children.Count == 1)
            { 
                index = 0;
            }
            if (OverAllRegion != null) 
            {
                if ((int)OverAllRegion.Tag == canvas1.Children.Count + 1 || canvas1.Children.Count == 0)
                {
                    Workspace.This.WorkIndexTitleVisBility = Visibility.Hidden;
                }
            }
            Workspace.This.ScannerVM.ScanWorkCount = canvas1.Children.Count;
        }
        /// <summary>
        /// 获取当前ROI里所有属性的参数
        /// Gets parameters for all attributes in the current ROI
        /// </summary>
        /// <param name="Current"></param>
        public void Bt_OnScanDataReceived(int Current)
        {
            //Workspace.This.Owner.Dispatcher.BeginInvoke((Action)delegate
            //{
            Workspace.This.ScannerVM.CurrentScanWorkIndex = Current;
            Workspace.This.ScannerVM.ClickScanWork = true;
            foreach (ScanRegionUserControl uc in canvas1.Children)
            {
                if ((int)uc.Tag != Current)
                {
                    uc.SetBackGrondColor();
                }
                else
                {
                    OverAllRegion = uc;
                    GetRegion();
                    Workspace.This.CurrentScanWorkIndexTitle = OverAllRegion.NumberStr;
                    Workspace.This.WorkIndexTitleVisBility = Visibility.Visible;
                }
            }
            Workspace.This.ScannerVM.ClickScanWork = false;
            //});
        }

        private void Uc_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 获取所有参数，如果参数都已经获取返回True
        /// Returns True if all parameters have been obtained
        /// </summary>
        /// <param name="Current"></param>
        /// <param name="loaded"></param>
        public void SetCurrentRegion(int Current, ref bool loaded)
        {
            //Workspace.This.Owner.Dispatcher.BeginInvoke((Action)delegate
            //{
                foreach (ScanRegionUserControl uc in canvas1.Children)
                {
                    if ((int)uc.Tag == Current)
                    {
                        uc.CurretIndexCount = Current;
                        uc.GridPanel_MouseUp(null, null);
                    }
                }
            //});
            loaded = true;
        }
        private void CreateControl_Click(object sender, RoutedEventArgs e)
        {
            CreateControl();
        }
        private void RemoveControl_Click(object sender, RoutedEventArgs e)
        {
            //Workspace.This.WorkIndexTitleVisBility = Visibility.Hidden;
            RemoveControl();
        }
        //down
        private void BtnDown_Click(object sender, RoutedEventArgs e)
        {
            double ii = scrolls.VerticalOffset;
            scrolls.ScrollToVerticalOffset(ii + 1);
        }
        //up
        private void BtnUp_Click(object sender, RoutedEventArgs e)
        {
            double ii = scrolls.VerticalOffset;
            if (ii > 0)
            {
                scrolls.ScrollToVerticalOffset(ii - 1);
            }
        }
        #region SetLaser
        public void IsSetRegion()
        {
            if (OverAllRegion == null)
            {
                return;
            }
            ScanRegionUserControl _Region = OverAllRegion;
            _Region.LLaser = false;
            _Region.R1Laser = false;
            _Region.R2Laser = false;
        }
        #endregion

        #region GetRegionParamete
        public void IsGetRegion()
        {
            if (OverAllRegion != null)
            {
                ScanRegionUserControl _Region = OverAllRegion;
                Workspace.This.ScannerVM._RangeDx = _Region.Dx;
                Workspace.This.ScannerVM._RangeDy = _Region.Dy;
            }
        }
        /// <summary>
        /// 获取当前ROI需要的参数
        /// Gets the parameters required for the current ROI
        /// </summary>
        public void GetRegion()
        {
            if (OverAllRegion != null)
            {
                ScanRegionUserControl _Region = OverAllRegion;
                Workspace.This.ScannerVM.ScanX0 = _Region.X0;
                Workspace.This.ScannerVM.ScanDeltaX = _Region.Dx;
                Workspace.This.ScannerVM.ScanY0 = _Region.Y0;
                Workspace.This.ScannerVM.ScanDeltaY = _Region.Dy;
                Workspace.This.ScannerVM.ScanZ0 = _Region.Z0;
                Workspace.This.ScannerVM.ScanDeltaZ = _Region.Dz;
                for (int i = 0; i < Workspace.This.ScannerVM.ResolutionOptions.Count; i++)
                {
                    if (Workspace.This.ScannerVM.ResolutionOptions[i].Value == _Region.Reslution)
                    {
                        Workspace.This.ScannerVM.SelectedResolution = Workspace.This.ScannerVM.ResolutionOptions[i];
                    }
                }
                for (int i = 0; i < Workspace.This.ScannerVM.QualityOptions.Count; i++)
                {
                    if (Workspace.This.ScannerVM.QualityOptions[i].Value == _Region.Quality)
                    {
                        Workspace.This.ScannerVM.SelectedQuality = Workspace.This.ScannerVM.QualityOptions[i];
                    }
                }
                for (int i = 0; i < Workspace.This.IVVM.PGAOptionsModule.Count; i++)
                {

                    if (Workspace.This.IVVM.PGAOptionsModule[i].Value == _Region.LPag)
                    {
                        Workspace.This.IVVM.SelectedMModuleL1 = Workspace.This.IVVM.PGAOptionsModule[i];
                    }
                    if (Workspace.This.IVVM.PGAOptionsModule[i].Value == _Region.R1Pag)
                    {
                        Workspace.This.IVVM.SelectedMModuleR1 = Workspace.This.IVVM.PGAOptionsModule[i];
                    }
                    if (Workspace.This.IVVM.PGAOptionsModule[i].Value == _Region.R2Pag)
                    {
                        Workspace.This.IVVM.SelectedMModuleR2 = Workspace.This.IVVM.PGAOptionsModule[i];
                    }
                }
                for (int i = 0; i < Workspace.This.IVVM.GainComModule.Count; i++)
                {

                    if (Workspace.This.IVVM.GainComModule[i].Value == _Region.LApdGain)
                    {
                        Workspace.This.IVVM.SelectedGainComModuleL1 = Workspace.This.IVVM.GainComModule[i];
                    }
                    if (Workspace.This.IVVM.GainComModule[i].Value == _Region.R1ApdGain)
                    {
                        Workspace.This.IVVM.SelectedGainComModuleR1 = Workspace.This.IVVM.GainComModule[i];

                    }
                    if (Workspace.This.IVVM.GainComModule[i].Value == _Region.R2ApdGain)
                    {
                        Workspace.This.IVVM.SelectedGainComModuleR2 = Workspace.This.IVVM.GainComModule[i];
                    }
                }
                for (int i=0;i<Workspace.This.ZAutomaticallyFocalVM.FocusOptions.Count;i++) {

                    if (Workspace.This.ZAutomaticallyFocalVM.FocusOptions[i] == _Region.SelectedZStackFocus)
                    {
                        Workspace.This.ZAutomaticallyFocalVM.SelectedFocus = Workspace.This.ZAutomaticallyFocalVM.FocusOptions[i];
                    }
                }
                //Workspace.This.ZAutomaticallyFocalVM._TopImage = 0;
                //Workspace.This.ZAutomaticallyFocalVM._DeltaFocus = 0;
                Workspace.This.ZAutomaticallyFocalVM.Ofimages = _Region.Ofimages;
                Workspace.This.ZAutomaticallyFocalVM.DeltaFocus = _Region.DeltaFocus;
                Workspace.This.ZAutomaticallyFocalVM.TopImage = _Region.BottomImage;
                Workspace.This.ZAutomaticallyFocalVM.IsCreateGif = _Region.IsCreateGif;
                Workspace.This.IVVM.GainTxtModuleL1 = _Region.LPmtGain;
                Workspace.This.IVVM.GainTxtModuleR1 = _Region.R1PmtGain;
                Workspace.This.IVVM.GainTxtModuleR2 = _Region.R2PmtGain;
                Workspace.This.IVVM.LaserCPower = _Region.LPower;
                Workspace.This.IVVM.LaserAPower = _Region.R1Power;
                Workspace.This.IVVM.LaserBPower = _Region.R2Power;
                Workspace.This.IVVM.IsLaserL1Selected = _Region.LLaser;
                Workspace.This.IVVM.IsLaserR1Selected = _Region.R1Laser;
                Workspace.This.IVVM.IsLaserR2Selected = _Region.R2Laser;
                Workspace.This.IVVM.IsCaptrueL1Selected = _Region.LCaptrue;
                Workspace.This.IVVM.IsCaptrueR1Selected = _Region.R1Captrue;
                Workspace.This.IVVM.IsCaptrueR2Selected = _Region.R2Captrue;
            }
        }

        #endregion

        /// <summary>
        /// 将参数放入到指定的ROI里
        /// Puts the parameter into the specified ROI
        /// </summary>
        /// <param name="name"></param>
        #region SetRegionParamete
        private void ScannerVM_OnScanRegionReceived(string name)
        {
            if (OverAllRegion == null)
            {
                return;
            }
            ScanRegionUserControl _Region = OverAllRegion;
            switch (name)
            {
                case "ScanX0":
                    _Region.X0 = Workspace.This.ScannerVM.ScanX0;
                    break;
                case "ScanDeltaX":
                    _Region.Dx = Workspace.This.ScannerVM.ScanDeltaX;
                    break;
                case "ScanY0":
                    _Region.Y0 = Workspace.This.ScannerVM.ScanY0;
                    break;
                case "ScanDeltaY":
                    _Region.Dy = Workspace.This.ScannerVM.ScanDeltaY;
                    break;
                case "ScanZ0":
                    _Region.Z0 = Workspace.This.ScannerVM.ScanZ0;
                    break;
                case "ScanDeltaZ":
                    _Region.Dz = Workspace.This.ScannerVM.ScanDeltaZ;
                    break;
                case "SelectedResolution":
                    _Region.Reslution = Workspace.This.ScannerVM.SelectedResolution.Value;
                    break;
                case "SelectedQuality":
                    _Region.Quality = Workspace.This.ScannerVM.SelectedQuality.Value;
                    break;
            }
        }
        /// <summary>
        /// 将参数放入到指定的ROI里
        /// Puts the parameter into the specified ROI
        /// </summary>
        /// <param name="name"></param>
        private void IVVM_OnScanRegionReceived(string name)
        {
            if (OverAllRegion == null)
            {
                return;
            }
            ScanRegionUserControl _Region = OverAllRegion;
            switch (name)
            {
                case "SelectedMModuleL1":
                    _Region.LPag = Workspace.This.IVVM.SelectedMModuleL1.Value;
                    break;
                case "SelectedMModuleR1":
                    _Region.R1Pag = Workspace.This.IVVM.SelectedMModuleR1.Value;
                    break;
                case "SelectedMModuleR2":
                    _Region.R2Pag = Workspace.This.IVVM.SelectedMModuleR2.Value;
                    break;
                case "SelectedGainComModuleL1":
                    _Region.LApdGain = Workspace.This.IVVM.SelectedGainComModuleL1.Value;
                    break;
                case "SelectedGainComModuleR1":
                    _Region.R1ApdGain = Workspace.This.IVVM.SelectedGainComModuleR1.Value;
                    break;
                case "SelectedGainComModuleR2":
                    _Region.R2ApdGain = Workspace.This.IVVM.SelectedGainComModuleR2.Value;
                    break;
                case "GainTxtModuleL1":
                    _Region.LPmtGain = Workspace.This.IVVM.GainTxtModuleL1;
                    break;
                case "GainTxtModuleR1":
                    _Region.R1PmtGain = Workspace.This.IVVM.GainTxtModuleR1;
                    break;
                case "GainTxtModuleR2":
                    _Region.R2PmtGain = Workspace.This.IVVM.GainTxtModuleR2;
                    break;
                case "LaserCPower":
                    _Region.LPower = Workspace.This.IVVM.LaserCPower;
                    break;
                case "LaserAPower":
                    _Region.R1Power = Workspace.This.IVVM.LaserAPower;
                    break;
                case "LaserBPower":
                    _Region.R2Power = Workspace.This.IVVM.LaserBPower;
                    break;
                case "IsLaserL1Selected":
                    _Region.LLaser = Workspace.This.IVVM.IsLaserL1Selected;
                    break;
                case "IsLaserR1Selected":
                    _Region.R1Laser = Workspace.This.IVVM.IsLaserR1Selected;
                    break;
                case "IsLaserR2Selected":
                    _Region.R2Laser = Workspace.This.IVVM.IsLaserR2Selected;
                    break;
                case "IsCaptrueL1Selected":
                    _Region.LCaptrue = Workspace.This.IVVM.IsCaptrueL1Selected;
                    break;
                case "IsCaptrueR1Selected":
                    _Region.R1Captrue = Workspace.This.IVVM.IsCaptrueR1Selected;
                    break;
                case "IsCaptrueR2Selected":
                    _Region.R2Captrue = Workspace.This.IVVM.IsCaptrueR2Selected;
                    break;
            }
        }
        /// <summary>
        /// 将参数放入到指定的ROI里
        /// Puts the parameter into the specified ROI
        /// </summary>
        /// <param name="name"></param>
        private void ZAutomaticallyFocalVM_OnScanRegionZStackReceived(string name)
        {
            if (OverAllRegion == null)
            {
                return;
            }
            ScanRegionUserControl _Region = OverAllRegion;
            switch (name)
            {
                case "IsCreateGif":
                    _Region.IsCreateGif = Workspace.This.ZAutomaticallyFocalVM.IsCreateGif;
                    break;
                case "TopImage":
                    _Region.BottomImage = Workspace.This.ZAutomaticallyFocalVM.TopImage;
                    break;
                case "SelectedFocus":
                    _Region.SelectedZStackFocus = Workspace.This.ZAutomaticallyFocalVM.SelectedFocus;
                    break;
                case "Ofimages":
                    _Region.Ofimages = Workspace.This.ZAutomaticallyFocalVM.Ofimages;
                    break;
                case "DeltaFocus":
                    _Region.DeltaFocus = Workspace.This.ZAutomaticallyFocalVM.DeltaFocus;
                    break;
            }
        }

        #endregion
    }
}
