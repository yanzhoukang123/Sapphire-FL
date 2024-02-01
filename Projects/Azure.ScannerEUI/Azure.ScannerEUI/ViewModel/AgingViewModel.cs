using Azure.Configuration.Settings;
using Azure.EthernetCommLib;
using Azure.ImagingSystem;
using Azure.WPF.Framework;
using Hywire.FileAccess;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Azure.ScannerEUI.ViewModel
{

    class AgingViewModel : ViewModelBase
    {
        public AgingViewModel()
        {
            TECRecordTimeInit();
        }

        #region privert data
        private RelayCommand _StartBatchProcessCommand = null;
        private RelayCommand _StopBatchProcessCommand = null;
        private bool _IsProcessing;
        private Thread _TECTempeProcess = null;
        private Thread _Process = null;
        private Thread _ProcessDoorState = null;
        private bool _IsNextBtnClicked = false;
        private bool _IsNextStepBtnEnabled = false;
        private RelayCommand _NextStepCommand = null;
        //private static readonly string[] _ChannelNames = { "NIR", "Green", "Red", "Blue" };

        private string _SubProcessName;
        private int _TotalProcessPercent;
        private int _SubProcessPercent;

        private string _DeviceSerialNum = string.Empty;
        private int _LoopTimes;
        private string _StoreFolder = string.Empty;
        private RelayCommand _StoreFolderCommand = null;
        private bool _IsProcessCanceled = false;
        private bool singe = false;
        private bool _IsTECTempeEnabled = true;
        private bool _IsTECTempeSelected = false;
        private bool _IsOldEnabled = true;

        #endregion

        #region Store Folder Command
        public ICommand StoreFolderCommand
        {
            get
            {
                if (_StoreFolderCommand == null)
                {
                    _StoreFolderCommand = new RelayCommand(ExecuteStoreFolderCommand, CanExecuteStoreFolderCommand);
                }
                return _StoreFolderCommand;
            }
        }
        public void ExecuteStoreFolderCommand(object parameter)
        {
            if (StoreFolder == string.Empty)
            {
                System.Windows.Forms.FolderBrowserDialog fdDlg = new System.Windows.Forms.FolderBrowserDialog();
                //string commonPictureFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                //string appCommonPictureFolder = commonPictureFolder + "\\" + "Scanner老化测试记录\\";
                fdDlg.ShowNewFolderButton = true;
                System.Windows.Forms.DialogResult dlgResult = fdDlg.ShowDialog();
                if (dlgResult == System.Windows.Forms.DialogResult.Cancel)
                {
                    return;
                }
                else
                {
                    StoreFolder = fdDlg.SelectedPath;
                }
            }
            else if (Directory.Exists(StoreFolder))
            {
                System.Diagnostics.Process.Start(StoreFolder);
            }
        }
        public bool CanExecuteStoreFolderCommand(object parameter)
        {
            return true;
        }
        #endregion Store Folder Command

        #region Start Batch Process Command
        public ICommand StartBatchProcessCommand
        {
            get
            {
                if (_StartBatchProcessCommand == null)
                {
                    _StartBatchProcessCommand = new RelayCommand(ExecuteStartBatchProcessCommand, CanExecuteBatchProcessCommand);
                }
                return _StartBatchProcessCommand;
            }
        }
        public void ExecuteStartBatchProcessCommand(object param)
        {

            if (!Workspace.This.MotorVM.IsNewFirmware)
            {
                MessageBox.Show("通讯异常！");
                return;
            }
            if (IsTECTempeSelected)
            {
                MessageBox.Show("TEC温度老化中，请先关闭TEC老化！");
                return;
            }
            //if(Workspace.This.IVVM.SensorML1==IvSensorType.NA&& Workspace.This.IVVM.SensorMR1 == IvSensorType.NA&&Workspace.This.IVVM.SensorMR2 == IvSensorType.NA)
            //{
            //    MessageBox.Show("至少有一个光学模块！");
            //    return;
            //}
            if (!Workspace.This.MotorVM.MotorAlreadyHome)
            {
                MessageBox.Show("电机尚未找到零点！");
                return;
            }
            if (DeviceSerialNum == string.Empty)
            {
                MessageBox.Show("请输入设备编号再重试！");
                return;
            }
            if (LoopTimes <= 0)
            {
                MessageBox.Show("请输入合法循环次数再重试！");
                return;
            }
            StoreFolder = string.Empty;         // prompt user to select folder
            if (StoreFolder == string.Empty)
            {
                System.Windows.Forms.FolderBrowserDialog fdDlg = new System.Windows.Forms.FolderBrowserDialog();
                //string commonPictureFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                //string appCommonPictureFolder = commonPictureFolder + "\\" + "Scanner老化测试记录\\";
                fdDlg.ShowNewFolderButton = true;
                System.Windows.Forms.DialogResult dlgResult = fdDlg.ShowDialog();
                if (dlgResult == System.Windows.Forms.DialogResult.Cancel)
                {
                    return;
                }
                else
                {
                    StoreFolder = fdDlg.SelectedPath;
                }
            }
            _ProcessDoorState = new Thread(CheckDoorState);
            _ProcessDoorState.IsBackground = true;
            _ProcessDoorState.Start();

            _Process = new Thread(BatchProcessMethod);
            _Process.IsBackground = true;
            _Process.Start();

            TotalProcessPercent = 0;
            SubProcessPercent = 0;
            SubProcessName = "漏光测试";
            IsProcessing = true;
        }

        private void CheckDoorState()
        {
            while (singe)
            {
                Thread.Sleep(500);
                if (Workspace.This.ApdVM.LIDIsOpen == true)
                {
                    Workspace.This.IVVM.GainTxtModuleL1 = 4000;
                    Workspace.This.IVVM.GainTxtModuleR1 = 4000;
                    Workspace.This.IVVM.GainTxtModuleR2 = 4000;
                    MessageBox.Show("检测到Door处于Open状态，已将Gain设置到4000。", "老化测试", MessageBoxButton.OK);
                }
            }
        }

        private void BatchProcessMethod()
        {
            int totalStep = 0;
            int singleLoopStep = 33;
            int totalStepDef = LoopTimes * singleLoopStep;

            int APDGain = 0;
            int PMTGain = 0;

            // setup storage folder
            string saveFolder = string.Empty;
            string storePath = string.Empty;
            string newStorePath = string.Empty;
            #region  get IV moule Count
            int IvWLL1 = Workspace.This.IVVM.WL1;
            int IvWLR1 = Workspace.This.IVVM.WR1;
            int IvWLR2 = Workspace.This.IVVM.WR2;
            int Power = ConfigPower;

            double ScanX0 = Workspace.This.ScannerVM.ScanX0;
            double ScanDeltaX = Workspace.This.ScannerVM.ScanDeltaX;
            double ScanY0 = Workspace.This.ScannerVM.ScanY0;
            double ScanDeltaY = Workspace.This.ScannerVM.ScanDeltaY;
            double PowerL = Workspace.This.IVVM.LaserAPower;
            double PowerR1 = Workspace.This.IVVM.LaserBPower;
            double PowerR2 = Workspace.This.IVVM.LaserCPower;
            ResolutionType res = Workspace.This.ScannerVM.SelectedResolution;
            double X1 = 10;
            double XD1 = 85;
            double X2 = 95;
            double XD2 = 85;
            double X3 = 180;
            double XD3 = 85;

            double Y1 = 10;
            double YD1 = 85;
            double Y2 = 95;
            double YD2 = 85;
            double Y3 = 180;
            double YD3 = 85;
            int icount = 0;
            if (IvWLL1 != 0)
            {
                Workspace.This.IVVM.IsCaptrueL1Selected = true;
            }
            if (IvWLR1 != 0)
            {
                Workspace.This.IVVM.IsCaptrueR1Selected = true;
            }
            if (IvWLR2 != 0)
            {
                Workspace.This.IVVM.IsCaptrueR2Selected = true;
            }
            #endregion
            for (int loopCount = 0; loopCount < LoopTimes; loopCount++)
            {
                saveFolder = string.Format(StoreFolder + "\\SN" + DeviceSerialNum + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + "\\");
                storePath = saveFolder;
                newStorePath = saveFolder;
                if (!Directory.Exists(saveFolder))
                {
                    Directory.CreateDirectory(saveFolder);
                }
                Thread.Sleep(500);
                if (loopCount == 0)
                {
                    #region resolution board scan confirmation
                    MessageBoxResult boxResult = MessageBoxResult.None;
                    Workspace.This.Owner.Dispatcher.Invoke(new Action(() =>
                    {
                        boxResult = MessageBox.Show("是否扫描分辨率板？", "梯度增益测试", MessageBoxButton.YesNo);
                    }));
                    if (boxResult == MessageBoxResult.Yes)
                    {
                        RemoveAllImages();
                        saveFolder = string.Format(newStorePath + "\\Resolution Board" + "\\");
                        storePath = saveFolder;
                        if (!Directory.Exists(saveFolder))
                        {
                            Directory.CreateDirectory(saveFolder);
                        }
                        Thread.Sleep(500);
                        SubProcessName = "分辨率板扫描";
                        SubProcessPercent = 0;
                        MessageBox.Show("请确认焦距，扫描区域\n准备好后，点击主页面“继续”按钮开始执行。",
                            "老化测试", MessageBoxButton.OK);
                        IsNextBtnClicked = false;
                        IsNextStepBtnEnabled = true;
                        while (!IsNextBtnClicked)
                        {
                            Thread.Sleep(10);
                        }
                        IsNextStepBtnEnabled = false;
                        // set resolution = 10um, Quality=1, PGA=1, Gain = 500(APD)/5000(PMT), 3 Channels Lasers 10mW 扫描1次
                        //select resolution 10um
                        for (int i = 0; i < Workspace.This.ScannerVM.ResolutionOptions.Count; i++)
                        {
                            if (Workspace.This.ScannerVM.ResolutionOptions[i].DisplayName == "10")
                            {
                                Workspace.This.ScannerVM.SelectedResolution = Workspace.This.ScannerVM.ResolutionOptions[i];
                                break;
                            }
                        }
                        //select Quality=1,
                        for (int i = 0; i < Workspace.This.ScannerVM.QualityOptions.Count; i++)
                        {
                            if (Workspace.This.ScannerVM.QualityOptions[i].DisplayName == "1")
                            {
                                Workspace.This.ScannerVM.SelectedQuality = Workspace.This.ScannerVM.QualityOptions[i];
                                break;
                            }
                        }
                        //select WL==784{ PGA=8  } 
                        if (IvWLL1 != 0 && IvWLL1 == 784)
                        {
                            Workspace.This.IVVM.SelectedMModuleL1 = Workspace.This.IVVM.PGAOptionsModule[3];
                        }
                        else if (IvWLL1 != 0 && IvWLL1 != 784)
                        {
                            Workspace.This.IVVM.SelectedMModuleL1 = Workspace.This.IVVM.PGAOptionsModule[0];
                        }

                        if (IvWLR1 != 0 && IvWLR1 == 784)
                        {
                            Workspace.This.IVVM.SelectedMModuleR1 = Workspace.This.IVVM.PGAOptionsModule[3];
                        }
                        else if (IvWLR1 != 0 && IvWLR1 != 784)
                        {
                            Workspace.This.IVVM.SelectedMModuleR1 = Workspace.This.IVVM.PGAOptionsModule[0];
                        }

                        if (IvWLR2 != 0 && IvWLR2 == 784)
                        {
                            Workspace.This.IVVM.SelectedMModuleR2 = Workspace.This.IVVM.PGAOptionsModule[3];
                        }
                        else if (IvWLR2 != 0 && IvWLR2 != 784)
                        {
                            Workspace.This.IVVM.SelectedMModuleR2 = Workspace.This.IVVM.PGAOptionsModule[0];
                        }
                        //for (int i = 0; i < Workspace.This.IVVM.PGAOptionsModule.Count; i++)
                        //{
                        //    if (Workspace.This.IVVM.PGAOptionsModule[i].DisplayName == "1")
                        //    {
                        //        if (IvWLL1 != 0)
                        //            Workspace.This.IVVM.SelectedMModuleL1 = Workspace.This.IVVM.PGAOptionsModule[i];
                        //        if (IvWLR1 != 0)
                        //            Workspace.This.IVVM.SelectedMModuleR1 = Workspace.This.IVVM.PGAOptionsModule[i];
                        //        if (IvWLR2 != 0)
                        //            Workspace.This.IVVM.SelectedMModuleR2 = Workspace.This.IVVM.PGAOptionsModule[i];
                        //        break;
                        //    }
                        //}
                        //Set APD Gain  500    //Set PMT Gain 5000       //Set lasers 10mw        //        // turn on the lasers
                        APDGain = 500;
                        PMTGain = 5000;
                        for (int k = 0; k < Workspace.This.IVVM.GainComModule.Count; k++)
                        {
                            if (Workspace.This.IVVM.GainComModule[k].DisplayName == APDGain.ToString())
                            {
                                if (IvWLL1 != 0)
                                {
                                    Workspace.This.IVVM.SelectedGainComModuleL1 = Workspace.This.IVVM.GainComModule[k];
                                    Workspace.This.IVVM.GainTxtModuleL1 = PMTGain;
                                    Workspace.This.IVVM.LaserAPower = Power;
                                    Workspace.This.IVVM.IsLaserL1Selected = true;
                                }
                                if (IvWLR1 != 0)
                                {
                                    Workspace.This.IVVM.SelectedGainComModuleR1 = Workspace.This.IVVM.GainComModule[k];
                                    Workspace.This.IVVM.GainTxtModuleR1 = PMTGain;
                                    Workspace.This.IVVM.LaserBPower = Power;
                                    Workspace.This.IVVM.IsLaserR1Selected = true;
                                }
                                if (IvWLR2 != 0)
                                {
                                    Workspace.This.IVVM.SelectedGainComModuleR2 = Workspace.This.IVVM.GainComModule[k];
                                    Workspace.This.IVVM.GainTxtModuleR2 = PMTGain;
                                    Workspace.This.IVVM.LaserCPower = Power;
                                    Workspace.This.IVVM.IsLaserR2Selected = true;
                                }
                                break;
                            }
                        }
                        Thread.Sleep(1000);
                        if (_IsProcessCanceled)
                        {
                            _IsProcessCanceled = false;
                            IsNextStepBtnEnabled = false;
                            IsNextBtnClicked = false;
                            IsProcessing = false;
                            return;
                        }
                        Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
                        {
                            //start Scan
                            Workspace.This.ScannerVM.ExecuteScanCommand(null);
                        });

                        try
                        {
                            WaitScanningEnded();
                            if (_IsProcessCanceled)
                            {
                                _IsProcessCanceled = false;
                                IsNextStepBtnEnabled = false;
                                IsNextBtnClicked = false;
                                IsProcessing = false;
                                return;
                            }
                        }
                        catch (ThreadAbortException e)
                        {
                            return;
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message);
                            return;
                        }
                        if (Workspace.This.Files.Count > 0)
                        {
                            try
                            {

                                for (int count = 0; count < Workspace.This.Files.Count; count++)
                                {
                                    string[] parts = Workspace.This.Files[count].FileName.Split('_');
                                    string NameType = parts[1];
                                    //if (IvWLL1 != 0 && Workspace.This.Files[count].FileName.Contains(IvWLL1.ToString()) && Workspace.This.Files[count].FileName.Contains("L"))
                                    //if (Workspace.This.Files[count].FileName.Contains("L"))
                                    if (IvWLL1 != 0 && NameType == "L")
                                    {
                                        int _pga = 1;
                                        if (IvWLL1 == 784)
                                        {
                                            _pga = 8;
                                        }
                                        DateTime dt = DateTime.Now;
                                        string strFileTime = "_" + string.Format("{0}-{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                                        if (Workspace.This.IVVM.SensorML1 == IvSensorType.APD)
                                        {

                                            Workspace.This.Files[count].FilePath = string.Format("{0}{1}-" + Power + "mW-Gain{2}-PGA" + _pga + "-Res{3}um-Quality1" + strFileTime + ".tif", storePath, IvWLL1 + Workspace.This.IVVM.WL1Sign + "-L", APDGain, Workspace.This.ScannerVM.SelectedResolution.Value);
                                            SavePicture(count, false);
                                        }
                                        else
                                        {
                                            Workspace.This.Files[count].FilePath = string.Format("{0}{1}-" + Power + "mW-Gain{2}-PGA" + _pga + "-Res{3}um-Quality1" + strFileTime + ".tif", storePath, IvWLL1 + Workspace.This.IVVM.WL1Sign + "-L", PMTGain, Workspace.This.ScannerVM.SelectedResolution.Value);
                                            SavePicture(count, false);
                                        }

                                    }
                                    //if (IvWLR2 != 0 && Workspace.This.Files[count].FileName.Contains(IvWLR2.ToString()) && Workspace.This.Files[count].FileName.Contains("R2"))
                                    //if (Workspace.This.Files[count].FileName.Contains("R2"))
                                    if (IvWLR2 != 0 && NameType == "R2")
                                    {
                                        int _pga = 1;
                                        if (IvWLR2 == 784)
                                        {
                                            _pga = 8;
                                        }
                                        DateTime dt = DateTime.Now;
                                        string strFileTime = "_" + string.Format("{0}-{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                                        if (Workspace.This.IVVM.SensorMR2 == IvSensorType.APD)
                                        {
                                            Workspace.This.Files[count].FilePath = string.Format("{0}{1}-" + Power + "mW-Gain{2}-PGA" + _pga + "-Res{3}um-Quality1" + strFileTime + ".tif", storePath, IvWLR2 + Workspace.This.IVVM.WR2Sign + "-R2", APDGain, Workspace.This.ScannerVM.SelectedResolution.Value);
                                            SavePicture(count, false);
                                        }
                                        else
                                        {
                                            Workspace.This.Files[count].FilePath = string.Format("{0}{1}-" + Power + "mW-Gain{2}-PGA" + _pga + "-Res{3}um-Quality1" + strFileTime + ".tif", storePath, IvWLR2 + Workspace.This.IVVM.WR2Sign + "-R2", PMTGain, Workspace.This.ScannerVM.SelectedResolution.Value);
                                            SavePicture(count, false);
                                        }

                                    }
                                    //if (IvWLR1 != 0 && Workspace.This.Files[count].FileName.Contains(IvWLR1.ToString()) && Workspace.This.Files[count].FileName.Contains("R1"))
                                    //if (Workspace.This.Files[count].FileName.Contains("R1"))
                                    if (IvWLR1 != 0 && NameType == "R1")
                                    {
                                        int _pga = 1;
                                        if (IvWLR1 == 784)
                                        {
                                            _pga = 8;
                                        }
                                        DateTime dt = DateTime.Now;
                                        string strFileTime = "_" + string.Format("{0}-{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                                        if (Workspace.This.IVVM.SensorMR1 == IvSensorType.APD)
                                        {
                                            Workspace.This.Files[count].FilePath = string.Format("{0}{1}-" + Power + "mW-Gain{2}-PGA" + _pga + "-Res{3}um-Quality1" + strFileTime + ".tif", storePath, IvWLR1 + Workspace.This.IVVM.WR1Sign + "-R1", APDGain, Workspace.This.ScannerVM.SelectedResolution.Value);
                                            SavePicture(count, false);
                                        }
                                        else
                                        {
                                            Workspace.This.Files[count].FilePath = string.Format("{0}{1}-" + Power + "mW-Gain{2}-PGA" + _pga + "-Res{3}um-Quality1" + strFileTime + ".tif", storePath, IvWLR1 + Workspace.This.IVVM.WR1Sign + "-R1", PMTGain, Workspace.This.ScannerVM.SelectedResolution.Value);
                                            SavePicture(count, false);
                                        }

                                    }
                                }
                                // clear all Images in Gallery Tab
                                Thread.Sleep(1000);
                                RemoveAllImages();
                                Workspace.This.SelectedTabIndex = (int)ApplicationTabType.Imaging;
                            }
                            catch (Exception exception)
                            {
                                // Rethrow to preserve stack details
                                // Satisfies the rule. 
                                //throw;
                                Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
                                {
                                    MessageBox.Show(exception.Message);
                                    IsNextStepBtnEnabled = false;
                                    IsNextBtnClicked = false;
                                    IsProcessing = false;
                                    return;
                                });
                            }
                        }
                        else        // failed to capture picture
                        {
                            Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
                            {
                                MessageBox.Show("采集图片失败！", "分辨率板扫描失败", MessageBoxButton.OK, MessageBoxImage.Error);
                            });

                            IsNextStepBtnEnabled = false;
                            IsNextBtnClicked = false;
                            IsProcessing = false;
                            return;
                        }
                    }
                    Workspace.This.SelectedTabIndex = 0;
                    #endregion

                    #region phosphor scan confirmation
                    boxResult = MessageBoxResult.None;
                    Workspace.This.Owner.Dispatcher.Invoke(new Action(() =>
                    {
                        boxResult = MessageBox.Show("是否进行Phosphor扫描？", "Phosphor Scan", MessageBoxButton.YesNo);
                    }));
                    if (boxResult == MessageBoxResult.Yes)
                    {
                        if (Workspace.This.IVVM.SensorML1 != IvSensorType.PMT && Workspace.This.IVVM.SensorMR1 != IvSensorType.PMT && Workspace.This.IVVM.SensorMR2 != IvSensorType.PMT)
                        {
                            MessageBox.Show("Phosphor扫描至少有一个PMT通道，点击主页面“继续”按钮继续执行。", "老化测试", MessageBoxButton.OK);
                            IsNextBtnClicked = false;
                            while (!IsNextBtnClicked)
                            {
                                Thread.Sleep(10);
                            }
                            Thread.Sleep(1000);
                            if (_IsProcessCanceled)
                            {
                                _IsProcessCanceled = false;
                                IsNextStepBtnEnabled = false;
                                IsNextBtnClicked = false;
                                IsProcessing = false;
                                return;
                            }
                        }
                        else
                        {
                            singe = true;
                            // setup storage folder
                            saveFolder = string.Format(newStorePath + "\\Phosphor" + "\\");
                            storePath = saveFolder;
                            // clear all Images in Gallery Tab
                            RemoveAllImages();
                            if (!Directory.Exists(saveFolder))
                            {
                                Directory.CreateDirectory(saveFolder);
                            }
                            Thread.Sleep(500);
                            SubProcessName = "Phosphor扫描";
                            SubProcessPercent = 0;
                            MessageBox.Show("请确认焦距，扫描区域，请注意避光！\n准备好后，点击主页面“继续”按钮开始执行。", "老化测试", MessageBoxButton.OK);
                            IsNextBtnClicked = false;
                            IsNextStepBtnEnabled = true;
                            while (!IsNextBtnClicked)
                            {
                                Thread.Sleep(10);
                            }
                            IsNextStepBtnEnabled = false;
                            //Res=200um,
                            for (int i = 0; i < Workspace.This.ScannerVM.ResolutionOptions.Count; i++)
                            {
                                if (Workspace.This.ScannerVM.ResolutionOptions[i].DisplayName == "200")
                                {
                                    Workspace.This.ScannerVM.SelectedResolution = Workspace.This.ScannerVM.ResolutionOptions[i];
                                    break;
                                }
                            }
                            //select Quality=1,
                            for (int i = 0; i < Workspace.This.ScannerVM.QualityOptions.Count; i++)
                            {
                                if (Workspace.This.ScannerVM.QualityOptions[i].DisplayName == "1")
                                {
                                    Workspace.This.ScannerVM.SelectedQuality = Workspace.This.ScannerVM.QualityOptions[i];
                                    break;
                                }
                            }
                            //select WL==784{ PGA=8  } 
                            if (IvWLL1 != 0 && IvWLL1 == 784)
                            {
                                Workspace.This.IVVM.SelectedMModuleL1 = Workspace.This.IVVM.PGAOptionsModule[3];
                            }
                            else if (IvWLL1 != 0 && IvWLL1 != 784)
                            {
                                Workspace.This.IVVM.SelectedMModuleL1 = Workspace.This.IVVM.PGAOptionsModule[3];
                            }

                            if (IvWLR1 != 0 && IvWLR1 == 784)
                            {
                                Workspace.This.IVVM.SelectedMModuleR1 = Workspace.This.IVVM.PGAOptionsModule[3];
                            }
                            else if (IvWLR1 != 0 && IvWLR1 != 784)
                            {
                                Workspace.This.IVVM.SelectedMModuleR1 = Workspace.This.IVVM.PGAOptionsModule[3];
                            }

                            if (IvWLR2 != 0 && IvWLR2 == 784)
                            {
                                Workspace.This.IVVM.SelectedMModuleR2 = Workspace.This.IVVM.PGAOptionsModule[3];
                            }
                            else if (IvWLR2 != 0 && IvWLR2 != 784)
                            {
                                Workspace.This.IVVM.SelectedMModuleR2 = Workspace.This.IVVM.PGAOptionsModule[3];
                            }
                            //for (int i = 0; i < Workspace.This.IVVM.PGAOptionsModule.Count; i++)
                            //{
                            //    if (Workspace.This.IVVM.PGAOptionsModule[i].DisplayName == "8")
                            //    {
                            //        if (IvWLL1 != 0)
                            //            Workspace.This.IVVM.SelectedMModuleL1 = Workspace.This.IVVM.PGAOptionsModule[i];
                            //        if (IvWLR1 != 0)
                            //            Workspace.This.IVVM.SelectedMModuleR1 = Workspace.This.IVVM.PGAOptionsModule[i];
                            //        if (IvWLR2 != 0)
                            //            Workspace.This.IVVM.SelectedMModuleR2 = Workspace.This.IVVM.PGAOptionsModule[i];
                            //        break;
                            //    }
                            //}
                            // turn on the lasers
                            Workspace.This.IVVM.IsLaserL1Selected = false;
                            Workspace.This.IVVM.IsLaserR1Selected = false;
                            Workspace.This.IVVM.IsLaserR2Selected = false;
                            for (int tempLoop = 0; tempLoop < 2; tempLoop++)
                            {
                                //if (tempLoop == 1)
                                {
                                    //Set lasers 0mw
                                    Workspace.This.IVVM.LaserAPower = 0;
                                    Workspace.This.IVVM.LaserBPower = 0;
                                    Workspace.This.IVVM.LaserCPower = 0;
                                }
                                // set gainD=10000 (1st), 11000 (2nd)
                                PMTGain = 10000 + tempLoop * 1000;
                                // move to (X0, Y0) position
                                Workspace.This.MotorVM.AbsXPos = Workspace.This.ScannerVM.ScanX0;
                                Workspace.This.MotorVM.ExecuteGoAbsPosCommand(MotorType.X);
                                Workspace.This.MotorVM.AbsYPos = Workspace.This.ScannerVM.ScanY0;
                                Workspace.This.MotorVM.ExecuteGoAbsPosCommand(MotorType.Y);
                                while (Workspace.This.MotorVM.CurrentXPos != Math.Round(Workspace.This.ScannerVM.ScanX0, 0))
                                {
                                    Thread.Sleep(100);
                                }
                                while (Workspace.This.MotorVM.CurrentYPos != Workspace.This.ScannerVM.ScanY0)
                                {
                                    Thread.Sleep(100);
                                }
                                // PMT Channel Laser 20mW  On Laser PMT Channel
                                if (Workspace.This.IVVM.SensorML1 == IvSensorType.PMT)
                                {
                                    Workspace.This.IVVM.GainTxtModuleL1 = PMTGain;
                                    Workspace.This.IVVM.LaserAPower = Power;
                                    Workspace.This.IVVM.IsLaserL1Selected = true;
                                }
                                if (Workspace.This.IVVM.SensorMR1 == IvSensorType.PMT)
                                {
                                    Workspace.This.IVVM.GainTxtModuleR1 = PMTGain;
                                    Workspace.This.IVVM.LaserBPower = Power;
                                    Workspace.This.IVVM.IsLaserR1Selected = true;
                                }
                                if (Workspace.This.IVVM.SensorMR2 == IvSensorType.PMT)
                                {
                                    Workspace.This.IVVM.GainTxtModuleR2 = PMTGain;
                                    Workspace.This.IVVM.LaserCPower = Power;
                                    Workspace.This.IVVM.IsLaserR2Selected = true;
                                }
                                Thread.Sleep(1000);
                                if (_IsProcessCanceled)
                                {
                                    _IsProcessCanceled = false;
                                    IsNextStepBtnEnabled = false;
                                    IsNextBtnClicked = false;
                                    IsProcessing = false;
                                    return;
                                }
                                Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
                                {
                                    Workspace.This.ScannerVM.ExecuteScanCommand(null);
                                });
                                try
                                {
                                    WaitScanningEnded();
                                    if (_IsProcessCanceled)
                                    {
                                        _IsProcessCanceled = false;
                                        IsNextStepBtnEnabled = false;
                                        IsNextBtnClicked = false;
                                        IsProcessing = false;
                                        return;
                                    }
                                    singe = false;
                                    //scan done PMT Gain 500//
                                    Workspace.This.IVVM.GainTxtModuleL1 = 4000;
                                    Workspace.This.IVVM.GainTxtModuleR1 = 4000;
                                    Workspace.This.IVVM.GainTxtModuleR2 = 4000;
                                }
                                catch (ThreadAbortException e)
                                {
                                    singe = false;
                                    Workspace.This.IVVM.GainTxtModuleL1 = 4000;
                                    Workspace.This.IVVM.GainTxtModuleR1 = 4000;
                                    Workspace.This.IVVM.GainTxtModuleR2 = 4000;
                                    return;
                                }
                                catch (Exception e)
                                {
                                    singe = false;
                                    Workspace.This.IVVM.GainTxtModuleL1 = 4000;
                                    Workspace.This.IVVM.GainTxtModuleR1 = 4000;
                                    Workspace.This.IVVM.GainTxtModuleR2 = 4000;
                                    MessageBox.Show(e.Message);
                                    return;
                                }
                                if (Workspace.This.Files.Count > 0)     // image captured successfully
                                {

                                    try
                                    {
                                        for (int count = 0; count < Workspace.This.Files.Count; count++)
                                        {
                                            string[] parts = Workspace.This.Files[count].FileName.Split('_');
                                            string NameType = parts[1];
                                            //if (IvWLL1 != 0 && Workspace.This.Files[count].FileName.Contains(IvWLL1.ToString()) && Workspace.This.Files[count].FileName.Contains("L"))
                                            //if (Workspace.This.Files[count].FileName.Contains("L"))
                                            if (IvWLL1 != 0 && NameType == "L")
                                            {
                                                int _pga = 1;
                                                if (IvWLL1 == 784)
                                                {
                                                    _pga = 8;
                                                }
                                                if (Workspace.This.IVVM.SensorML1 == IvSensorType.PMT)
                                                {
                                                    DateTime dt = DateTime.Now;
                                                    string strFileTime = "_" + string.Format("{0}-{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                                                    Workspace.This.Files[count].FilePath = string.Format("{0}{1}-" + Power + "mW-Gain{2}-PGA" + _pga + "-Res{3}um-Quality1" + strFileTime + ".tif", storePath, IvWLL1 + Workspace.This.IVVM.WL1Sign + "-L", PMTGain, Workspace.This.ScannerVM.SelectedResolution.Value);//C
                                                    SavePicture(count, false);
                                                }

                                            }
                                            //if (IvWLR1 != 0 && Workspace.This.Files[count].FileName.Contains(IvWLR1.ToString()) && Workspace.This.Files[count].FileName.Contains("R1"))
                                            //if (Workspace.This.Files[count].FileName.Contains("R1"))
                                            if (IvWLR1 != 0 && NameType == "R1")
                                            {
                                                int _pga = 1;
                                                if (IvWLR1 == 784)
                                                {
                                                    _pga = 8;
                                                }
                                                if (Workspace.This.IVVM.SensorMR1 == IvSensorType.PMT)
                                                {
                                                    DateTime dt = DateTime.Now;
                                                    string strFileTime = "_" + string.Format("{0}-{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                                                    Workspace.This.Files[count].FilePath = string.Format("{0}{1}-" + Power + "mW-Gain{2}-PGA" + _pga + "-Res{3}um-Quality1" + strFileTime + ".tif", storePath, IvWLR1 + Workspace.This.IVVM.WR1Sign + "-R1", PMTGain, Workspace.This.ScannerVM.SelectedResolution.Value);//B
                                                    SavePicture(count, false);
                                                }
                                            }
                                            //if (IvWLR2 != 0 && Workspace.This.Files[count].FileName.Contains(IvWLR2.ToString()) && Workspace.This.Files[count].FileName.Contains("R2"))
                                            //if (Workspace.This.Files[count].FileName.Contains("R2"))
                                            if (IvWLR2 != 0 && NameType == "R2")
                                            {
                                                int _pga = 1;
                                                if (IvWLR2 == 784)
                                                {
                                                    _pga = 8;
                                                }
                                                if (Workspace.This.IVVM.SensorMR2 == IvSensorType.PMT)
                                                {
                                                    DateTime dt = DateTime.Now;
                                                    string strFileTime = "_" + string.Format("{0}-{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                                                    Workspace.This.Files[count].FilePath = string.Format("{0}{1}-" + Power + "mW-Gain{2}-PGA" + _pga + "-Res{3}um-Quality1" + strFileTime + ".tif", storePath, IvWLR2 + Workspace.This.IVVM.WR2Sign + "-R2", PMTGain, Workspace.This.ScannerVM.SelectedResolution.Value);//A
                                                    SavePicture(count, false);
                                                }
                                            }
                                        }
                                        // clear all Images in Gallery Tab
                                        Thread.Sleep(1000);
                                        RemoveAllImages();
                                        Workspace.This.SelectedTabIndex = (int)ApplicationTabType.Imaging;
                                    }
                                    catch (Exception exception)
                                    {
                                        // Rethrow to preserve stack details
                                        // Satisfies the rule. 
                                        //throw;
                                        Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
                                        {
                                            MessageBox.Show(exception.Message);
                                            IsNextStepBtnEnabled = false;
                                            IsNextBtnClicked = false;
                                            IsProcessing = false;
                                            return;
                                        });
                                    }
                                }
                                else        // failed to capture picture
                                {
                                    Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
                                    {
                                        MessageBox.Show("采集图片失败！", "Phosphor扫描失败", MessageBoxButton.OK, MessageBoxImage.Error);
                                    });

                                    IsNextStepBtnEnabled = false;
                                    IsNextBtnClicked = false;
                                    IsProcessing = false;
                                    return;
                                }

                            }
                        }
                    }
                    #endregion

                    #region   darknoise scan configrmation
                    Workspace.This.Owner.Dispatcher.Invoke(new Action(() =>
                    {
                        //boxResult = MessageBox.Show("请确认焦距和扫描区域，以及设置激光功率！准备好后，点击主页面“继续”按钮执行。\n" +
                        //    "若想跳过测试，请点“否”！", "梯度增益测试", MessageBoxButton.YesNo);
                        MessageBox.Show("请确认焦距，分辨率，扫描区域！\n准备好后，点击主页面“继续”按钮开始执行。",
                            "老化测试", MessageBoxButton.OK);
                        Workspace.This.SelectedTabIndex = 0;
                    }));
                    #endregion
                }
                try
                {
                    Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
                    {
                        while (Workspace.This.Files.Count > 0)
                        {
                            Workspace.This.Remove(Workspace.This.Files[0]);
                        }
                    });
                    storePath = string.Empty;
                    if (!Directory.Exists(saveFolder))
                    {
                        Directory.CreateDirectory(saveFolder);
                    }
                    Thread.Sleep(500);

                    #region step 1: noise test without lasers on
                    SubProcessName = "噪声测试";
                    SubProcessPercent = 0;
                    storePath = newStorePath + "Dark Noise\\";
                    if (loopCount == 0)
                    {
                        IsNextBtnClicked = false;
                        IsNextStepBtnEnabled = true;
                    }
                    else
                    {
                        IsNextBtnClicked = true;
                        IsNextStepBtnEnabled = true;
                    }
                    IsNextStepBtnEnabled = true;
                    while (!IsNextBtnClicked)
                    {
                        Thread.Sleep(100);
                    }
                    IsNextStepBtnEnabled = false;

                    if (!Directory.Exists(storePath))
                    {
                        Directory.CreateDirectory(storePath);
                    }
                    Workspace.This.IVVM.LaserAPower = 0;
                    Workspace.This.IVVM.LaserBPower = 0;
                    Workspace.This.IVVM.LaserCPower = 0;
                    Workspace.This.IVVM.IsLaserL1Selected = false;
                    Workspace.This.IVVM.IsLaserR1Selected = false;
                    Workspace.This.IVVM.IsLaserR2Selected = false;
                    //select Quality=1,
                    for (int i = 0; i < Workspace.This.ScannerVM.QualityOptions.Count; i++)
                    {
                        if (Workspace.This.ScannerVM.QualityOptions[i].DisplayName == "1")
                        {
                            Workspace.This.ScannerVM.SelectedQuality = Workspace.This.ScannerVM.QualityOptions[i];
                            break;
                        }
                    }

                    //select WL==784{ PGA=8  } 
                    if (IvWLL1 != 0 && IvWLL1 == 784)
                    {
                        Workspace.This.IVVM.SelectedMModuleL1 = Workspace.This.IVVM.PGAOptionsModule[3];
                    }
                    else if (IvWLL1 != 0 && IvWLL1 != 784)
                    {
                        Workspace.This.IVVM.SelectedMModuleL1 = Workspace.This.IVVM.PGAOptionsModule[3];
                    }

                    if (IvWLR1 != 0 && IvWLR1 == 784)
                    {
                        Workspace.This.IVVM.SelectedMModuleR1 = Workspace.This.IVVM.PGAOptionsModule[3];
                    }
                    else if (IvWLR1 != 0 && IvWLR1 != 784)
                    {
                        Workspace.This.IVVM.SelectedMModuleR1 = Workspace.This.IVVM.PGAOptionsModule[3];
                    }

                    if (IvWLR2 != 0 && IvWLR2 == 784)
                    {
                        Workspace.This.IVVM.SelectedMModuleR2 = Workspace.This.IVVM.PGAOptionsModule[3];
                    }
                    else if (IvWLR2 != 0 && IvWLR2 != 784)
                    {
                        Workspace.This.IVVM.SelectedMModuleR2 = Workspace.This.IVVM.PGAOptionsModule[3];
                    }
                    //select PGA=8
                    //for (int i = 0; i < Workspace.This.IVVM.PGAOptionsModule.Count; i++)
                    //{
                    //    if (Workspace.This.IVVM.PGAOptionsModule[i].DisplayName == "8")
                    //    {
                    //        if (IvWLL1 != 0)
                    //            Workspace.This.IVVM.SelectedMModuleL1 = Workspace.This.IVVM.PGAOptionsModule[i];
                    //        if (IvWLR1 != 0)
                    //            Workspace.This.IVVM.SelectedMModuleR1 = Workspace.This.IVVM.PGAOptionsModule[i];
                    //        if (IvWLR2 != 0)
                    //            Workspace.This.IVVM.SelectedMModuleR2 = Workspace.This.IVVM.PGAOptionsModule[i];
                    //        break;
                    //    }
                    //}
                    //Set APD Gain  500             //Set PMT Gain 5000    
                    //Set lasers 10mw         //        // turn off the lasers  

                    APDGain = 500;
                    PMTGain = 4000;
                    for (int k = 0; k < Workspace.This.IVVM.GainComModule.Count; k++)
                    {
                        if (Workspace.This.IVVM.GainComModule[k].DisplayName == APDGain.ToString())
                        {
                            if (IvWLL1 != 0)
                            {
                                Workspace.This.IVVM.SelectedGainComModuleL1 = Workspace.This.IVVM.GainComModule[k];
                                Workspace.This.IVVM.GainTxtModuleL1 = PMTGain;
                            }
                            if (IvWLR1 != 0)
                            {
                                Workspace.This.IVVM.SelectedGainComModuleR1 = Workspace.This.IVVM.GainComModule[k];
                                Workspace.This.IVVM.GainTxtModuleR1 = PMTGain;
                            }
                            if (IvWLR2 != 0)
                            {
                                Workspace.This.IVVM.SelectedGainComModuleR2 = Workspace.This.IVVM.GainComModule[k];
                                Workspace.This.IVVM.GainTxtModuleR2 = PMTGain;
                            }
                            break;
                        }
                    }

                    Thread.Sleep(1000);
                    if (_IsProcessCanceled)
                    {
                        _IsProcessCanceled = false;
                        IsNextStepBtnEnabled = false;
                        IsNextBtnClicked = false;
                        IsProcessing = false;
                        return;
                    }
                    Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
                    {
                        Workspace.This.ScannerVM.ExecuteScanCommand(null);
                    });
                    try
                    {
                        WaitScanningEnded();
                        if (_IsProcessCanceled)
                        {
                            _IsProcessCanceled = false;
                            IsNextStepBtnEnabled = false;
                            IsNextBtnClicked = false;
                            IsProcessing = false;
                            return;
                        }
                    }
                    catch (ThreadAbortException e)
                    {
                        return;
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                        return;
                    }
                    icount = 0;
                    while (icount != 3)
                    {
                        if (Workspace.This.Files.Count > 0)
                        {
                            icount = 0;
                            break;
                        }
                        else
                        {
                            icount++;
                        }
                        Thread.Sleep(10000);
                    }
                    if (Workspace.This.Files.Count > 0)     // image captured successfully
                    {
                        try
                        {
                            for (int count = 0; count < Workspace.This.Files.Count; count++)
                            {
                                string[] parts = Workspace.This.Files[count].FileName.Split('_');
                                string NameType = parts[1];
                                //if (IvWLL1 != 0 && Workspace.This.Files[count].FileName.Contains(IvWLL1.ToString()) && Workspace.This.Files[count].FileName.Contains("L"))
                                //if (Workspace.This.Files[count].FileName.Contains("L"))
                                if (IvWLL1 != 0 && NameType == "L")
                                {
                                    DateTime dt = DateTime.Now;
                                    string strFileTime = "_" + string.Format("{0}-{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                                    if (Workspace.This.IVVM.SensorML1 == IvSensorType.APD)
                                    {
                                        Workspace.This.Files[count].FilePath = string.Format("{0}{1}-0mW-Gain{2}-PGA8-Res{3}um-Quality1" + strFileTime + ".tif", storePath, IvWLL1 + Workspace.This.IVVM.WL1Sign + "-L", APDGain, Workspace.This.ScannerVM.SelectedResolution.Value);
                                        SavePicture(count, false);
                                    }
                                    else
                                    {
                                        Workspace.This.Files[count].FilePath = string.Format("{0}{1}-0mW-Gain{2}-PGA8-Res{3}um-Quality1" + strFileTime + ".tif", storePath, IvWLL1 + Workspace.This.IVVM.WL1Sign + "-L", PMTGain, Workspace.This.ScannerVM.SelectedResolution.Value);
                                        SavePicture(count, false);
                                    }

                                }
                                //if (IvWLR1 != 0 && Workspace.This.Files[count].FileName.Contains(IvWLR1.ToString()) && Workspace.This.Files[count].FileName.Contains("R1"))
                                //if (Workspace.This.Files[count].FileName.Contains("R1"))
                                if (IvWLR1 != 0 && NameType == "R1")
                                {
                                    DateTime dt = DateTime.Now;
                                    string strFileTime = "_" + string.Format("{0}-{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                                    if (Workspace.This.IVVM.SensorMR1 == IvSensorType.APD)//B
                                    {
                                        Workspace.This.Files[count].FilePath = string.Format("{0}{1}-0mW-Gain{2}-PGA8-Res{3}um-Quality1" + strFileTime + ".tif", storePath, IvWLR1 + Workspace.This.IVVM.WR1Sign + "-R1", APDGain, Workspace.This.ScannerVM.SelectedResolution.Value);
                                        SavePicture(count, false);
                                    }
                                    else
                                    {
                                        Workspace.This.Files[count].FilePath = string.Format("{0}{1}-0mW-Gain{2}-PGA8-Res{3}um-Quality1" + strFileTime + ".tif", storePath, IvWLR1 + Workspace.This.IVVM.WR1Sign + "-R1", PMTGain, Workspace.This.ScannerVM.SelectedResolution.Value);
                                        SavePicture(count, false);
                                    }
                                }
                                //if (IvWLR2 != 0 && Workspace.This.Files[count].FileName.Contains(IvWLR2.ToString()) && Workspace.This.Files[count].FileName.Contains("R2"))
                                //if (Workspace.This.Files[count].FileName.Contains("R2"))
                                if (IvWLR2 != 0 && NameType == "R2")
                                {
                                    DateTime dt = DateTime.Now;
                                    string strFileTime = "_" + string.Format("{0}-{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                                    if (Workspace.This.IVVM.SensorMR2 == IvSensorType.APD)
                                    {
                                        Workspace.This.Files[count].FilePath = string.Format("{0}{1}-0mW-Gain{2}-PGA8-Res{3}um-Quality1" + strFileTime + ".tif", storePath, IvWLR2 + Workspace.This.IVVM.WR2Sign + "-R2", APDGain, Workspace.This.ScannerVM.SelectedResolution.Value);
                                        SavePicture(count, false);
                                    }
                                    else
                                    {
                                        Workspace.This.Files[count].FilePath = string.Format("{0}{1}-0mW-Gain{2}-PGA8-Res{3}um-Quality1" + strFileTime + ".tif", storePath, IvWLR2 + Workspace.This.IVVM.WR2Sign + "-R2", PMTGain, Workspace.This.ScannerVM.SelectedResolution.Value);
                                        SavePicture(count, false);
                                    }
                                }
                            }
                            // clear all Images in Gallery Tab
                            Thread.Sleep(1000);
                            RemoveAllImages();
                            Workspace.This.SelectedTabIndex = (int)ApplicationTabType.Imaging;
                        }
                        catch (Exception exception)
                        {
                            // Rethrow to preserve stack details
                            // Satisfies the rule. 
                            //throw;
                            Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
                            {
                                MessageBox.Show(exception.Message);
                                IsNextStepBtnEnabled = false;
                                IsNextBtnClicked = false;
                                IsProcessing = false;
                                return;
                            });
                        }
                        finally
                        {
                            SubProcessPercent = 100;
                            totalStep++;
                            TotalProcessPercent = totalStep * 100 / totalStepDef;
                        }
                    }
                    else        // failed to capture picture
                    {
                        Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
                        {
                            MessageBox.Show("采集图片失败！", "噪声测试失败", MessageBoxButton.OK, MessageBoxImage.Error);
                        });

                        IsNextStepBtnEnabled = false;
                        IsNextBtnClicked = false;
                        IsProcessing = false;
                        return;
                    }

                    #endregion

                    #region step 2: scan with different gains, 4 loops, 1st & 3rd use A, C; 2nd & 4th use B, D
                    SubProcessName = "梯度增益测试";
                    SubProcessPercent = 0;
                    storePath = newStorePath + "Gains\\";

                    if (!Directory.Exists(storePath))
                    {
                        Directory.CreateDirectory(storePath);
                    }

                    //select Quality=1,
                    for (int i = 0; i < Workspace.This.ScannerVM.QualityOptions.Count; i++)
                    {
                        if (Workspace.This.ScannerVM.QualityOptions[i].DisplayName == "1")
                        {
                            Workspace.This.ScannerVM.SelectedQuality = Workspace.This.ScannerVM.QualityOptions[i];
                            break;
                        }
                    }

                    //select WL==784{ PGA=8  } 
                    if (IvWLL1 != 0 && IvWLL1 == 784)
                    {
                        Workspace.This.IVVM.SelectedMModuleL1 = Workspace.This.IVVM.PGAOptionsModule[3];
                    }
                    else if (IvWLL1 != 0 && IvWLL1 != 784)
                    {
                        Workspace.This.IVVM.SelectedMModuleL1 = Workspace.This.IVVM.PGAOptionsModule[0];
                    }

                    if (IvWLR1 != 0 && IvWLR1 == 784)
                    {
                        Workspace.This.IVVM.SelectedMModuleR1 = Workspace.This.IVVM.PGAOptionsModule[3];
                    }
                    else if (IvWLR1 != 0 && IvWLR1 != 784)
                    {
                        Workspace.This.IVVM.SelectedMModuleR1 = Workspace.This.IVVM.PGAOptionsModule[0];
                    }

                    if (IvWLR2 != 0 && IvWLR2 == 784)
                    {
                        Workspace.This.IVVM.SelectedMModuleR2 = Workspace.This.IVVM.PGAOptionsModule[3];
                    }
                    else if (IvWLR2 != 0 && IvWLR2 != 784)
                    {
                        Workspace.This.IVVM.SelectedMModuleR2 = Workspace.This.IVVM.PGAOptionsModule[0];
                    }
                    //select PGA=1
                    //for (int i = 0; i < Workspace.This.IVVM.PGAOptionsModule.Count; i++)
                    //{
                    //    if (Workspace.This.IVVM.PGAOptionsModule[i].DisplayName == "1")
                    //    {
                    //        if (IvWLL1 != 0)
                    //            Workspace.This.IVVM.SelectedMModuleL1 = Workspace.This.IVVM.PGAOptionsModule[i];
                    //        if (IvWLR1 != 0)
                    //            Workspace.This.IVVM.SelectedMModuleR1 = Workspace.This.IVVM.PGAOptionsModule[i];
                    //        if (IvWLR2 != 0)
                    //            Workspace.This.IVVM.SelectedMModuleR2 = Workspace.This.IVVM.PGAOptionsModule[i];
                    //        break;
                    //    }
                    //}
                    //Set lasers 10mw
                    if (IvWLL1 != 0)
                        Workspace.This.IVVM.LaserAPower = Power;
                    if (IvWLR1 != 0)
                        Workspace.This.IVVM.LaserBPower = Power;
                    if (IvWLR2 != 0)
                        Workspace.This.IVVM.LaserCPower = Power;
                    for (int i = 0; i < 2; i++)     // 4 loops, gain differs from 500 to 50
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            switch (j)
                            {
                                case 0:
                                    APDGain = 500;
                                    PMTGain = 5500;
                                    break;
                                case 1:
                                    APDGain = 400;
                                    PMTGain = 5000;
                                    break;
                                case 2:
                                    APDGain = 300;
                                    PMTGain = 4500;
                                    break;
                                case 3:
                                    APDGain = 250;
                                    PMTGain = 4000;
                                    break;
                                case 4:
                                    APDGain = 200;
                                    PMTGain = 3500;
                                    break;
                                case 5:
                                    APDGain = 150;
                                    PMTGain = 3000;
                                    break;
                                case 6:
                                    APDGain = 100;
                                    PMTGain = 2500;
                                    break;
                                case 7:
                                    APDGain = 50;
                                    PMTGain = 2000;
                                    break;
                            }
                            //Set APD Gain          //Set PMT Gain  // turn off the lasers
                            for (int k = 0; k < Workspace.This.IVVM.GainComModule.Count; k++)
                            {
                                if (Workspace.This.IVVM.GainComModule[k].DisplayName == APDGain.ToString())
                                {
                                    if (IvWLL1 != 0)
                                    {
                                        Workspace.This.IVVM.SelectedGainComModuleL1 = Workspace.This.IVVM.GainComModule[k];
                                        Workspace.This.IVVM.GainTxtModuleL1 = PMTGain;
                                        Workspace.This.IVVM.IsLaserL1Selected = true;
                                        Thread.Sleep(2000);
                                        Workspace.This.IVVM.IsLaserL1Selected = true;
                                    }
                                    if (IvWLR1 != 0)
                                    {
                                        Workspace.This.IVVM.SelectedGainComModuleR1 = Workspace.This.IVVM.GainComModule[k];
                                        Workspace.This.IVVM.GainTxtModuleR1 = PMTGain;
                                        Workspace.This.IVVM.IsLaserR1Selected = true;
                                        Thread.Sleep(2000);
                                        Workspace.This.IVVM.IsLaserR1Selected = true;
                                    }
                                    if (IvWLR2 != 0)
                                    {
                                        Workspace.This.IVVM.SelectedGainComModuleR2 = Workspace.This.IVVM.GainComModule[k];
                                        Workspace.This.IVVM.GainTxtModuleR2 = PMTGain;
                                        Workspace.This.IVVM.IsLaserR2Selected = true;
                                        Thread.Sleep(2000);
                                        Workspace.This.IVVM.IsLaserR2Selected = true;
                                    }
                                }
                            }
                            Thread.Sleep(1000);
                            if (_IsProcessCanceled)
                            {
                                _IsProcessCanceled = false;
                                IsNextStepBtnEnabled = false;
                                IsNextBtnClicked = false;
                                IsProcessing = false;
                                return;
                            }
                            Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
                            {
                                Workspace.This.ScannerVM.ExecuteScanCommand(null);
                            });
                            try
                            {
                                WaitScanningEnded();
                                if (_IsProcessCanceled)
                                {
                                    _IsProcessCanceled = false;
                                    IsNextStepBtnEnabled = false;
                                    IsNextBtnClicked = false;
                                    IsProcessing = false;
                                    return;
                                }
                            }
                            catch (ThreadAbortException e)
                            {
                                return;
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.Message);
                                return;
                            }
                            icount = 0;
                            while (icount != 3)
                            {
                                if (Workspace.This.Files.Count > 0)
                                {
                                    icount = 0;
                                    break;
                                }
                                else
                                {
                                    icount++;
                                }
                                Thread.Sleep(10000);
                            }
                            if (Workspace.This.Files.Count > 0)     // image captured successfully
                            {
                                try
                                {
                                    for (int count = 0; count < Workspace.This.Files.Count; count++)
                                    {
                                        string[] parts = Workspace.This.Files[count].FileName.Split('_');
                                        string NameType = parts[1];
                                        //if (IvWLL1 != 0 && Workspace.This.Files[count].FileName.Contains(IvWLL1.ToString()) && Workspace.This.Files[count].FileName.Contains("L"))
                                        //if (Workspace.This.Files[count].FileName.Contains("L"))
                                        if (IvWLL1 != 0 && NameType == "L")
                                        {
                                            int _pga = 1;
                                            if (IvWLL1 == 784)
                                            {
                                                _pga = 8;
                                            }
                                            DateTime dt = DateTime.Now;
                                            string strFileTime = "_" + string.Format("{0}-{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                                            if (Workspace.This.IVVM.SensorML1 == IvSensorType.APD)
                                            {
                                                Workspace.This.Files[count].FilePath = string.Format("{0}{1}-" + Power + "mW-Gain{2}-PGA" + _pga + "-Res{3}um-Quality1--{4}" + strFileTime + ".tif", storePath, IvWLL1 + Workspace.This.IVVM.WL1Sign + "-L", APDGain, Workspace.This.ScannerVM.SelectedResolution.Value, i);
                                                SavePicture(count, false);
                                            }
                                            else
                                            {
                                                Workspace.This.Files[count].FilePath = string.Format("{0}{1}-" + Power + "mW-Gain{2}-PGA1-Res{3}um-Quality1--{4}" + strFileTime + ".tif", storePath, IvWLL1 + Workspace.This.IVVM.WL1Sign + "-L", PMTGain, Workspace.This.ScannerVM.SelectedResolution.Value, i);
                                                SavePicture(count, false);
                                            }

                                        }
                                        //if (IvWLR1 != 0 && Workspace.This.Files[count].FileName.Contains(IvWLR1.ToString()) && Workspace.This.Files[count].FileName.Contains("R1"))
                                        //if (Workspace.This.Files[count].FileName.Contains("R1"))
                                        if (IvWLR1 != 0 && NameType == "R1")
                                        {
                                            int _pga = 1;
                                            if (IvWLR1 == 784)
                                            {
                                                _pga = 8;
                                            }
                                            DateTime dt = DateTime.Now;
                                            string strFileTime = "_" + string.Format("{0}-{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                                            if (Workspace.This.IVVM.SensorMR1 == IvSensorType.APD)
                                            {
                                                Workspace.This.Files[count].FilePath = string.Format("{0}{1}-" + Power + "mW-Gain{2}-PGA" + _pga + "-Res{3}um-Quality1--{4}" + strFileTime + ".tif", storePath, IvWLR1 + Workspace.This.IVVM.WR1Sign + "-R1", APDGain, Workspace.This.ScannerVM.SelectedResolution.Value, i);
                                                SavePicture(count, false);
                                            }
                                            else
                                            {
                                                Workspace.This.Files[count].FilePath = string.Format("{0}{1}-" + Power + "mW-Gain{2}-PGA1-Res{3}um-Quality1--{4}" + strFileTime + ".tif", storePath, IvWLR1 + Workspace.This.IVVM.WR1Sign + "-R1", PMTGain, Workspace.This.ScannerVM.SelectedResolution.Value, i);
                                                SavePicture(count, false);
                                            }

                                        }
                                        //if (IvWLR2 != 0 && Workspace.This.Files[count].FileName.Contains(IvWLR2.ToString()) && Workspace.This.Files[count].FileName.Contains("R2"))
                                        //if (Workspace.This.Files[count].FileName.Contains("R2"))
                                        if (IvWLR2 != 0 && NameType == "R2")
                                        {
                                            int _pga = 1;
                                            if (IvWLR2 == 784)
                                            {
                                                _pga = 8;
                                            }
                                            DateTime dt = DateTime.Now;
                                            string strFileTime = "_" + string.Format("{0}-{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                                            if (Workspace.This.IVVM.SensorMR2 == IvSensorType.APD)
                                            {
                                                Workspace.This.Files[count].FilePath = string.Format("{0}{1}-" + Power + "mW-Gain{2}-PGA" + _pga + "-Res{3}um-Quality1--{4}" + strFileTime + ".tif", storePath, IvWLR2 + Workspace.This.IVVM.WR2Sign + "-R2", APDGain, Workspace.This.ScannerVM.SelectedResolution.Value, i);
                                                SavePicture(count, false);
                                            }
                                            else
                                            {
                                                Workspace.This.Files[count].FilePath = string.Format("{0}{1}-" + Power + "mW-Gain{2}-PGA1-Res{3}um-Quality1--{4}" + strFileTime + ".tif", storePath, IvWLR2 + Workspace.This.IVVM.WR2Sign + "-R2", PMTGain, Workspace.This.ScannerVM.SelectedResolution.Value, i);
                                                SavePicture(count, false);
                                            }

                                        }
                                    }
                                    // clear all Images in Gallery Tab
                                    Thread.Sleep(1000);
                                    RemoveAllImages();
                                    Workspace.This.SelectedTabIndex = (int)ApplicationTabType.Imaging;
                                }
                                catch (Exception exception)
                                {
                                    // Rethrow to preserve stack details
                                    // Satisfies the rule. 
                                    //throw;
                                    Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
                                    {
                                        MessageBox.Show(exception.Message);
                                        IsNextStepBtnEnabled = false;
                                        IsNextBtnClicked = false;
                                        IsProcessing = false;
                                        return;
                                    });
                                }
                                finally
                                {
                                    SubProcessPercent = i * 25 + (int)((j + 1) * 25.0 / 6.0);
                                    totalStep++;
                                    TotalProcessPercent = totalStep * 100 / totalStepDef;
                                }
                            }
                            else        // failed to capture picture
                            {
                                Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
                                {
                                    MessageBox.Show("采集图片失败！", "梯度增益测试失败", MessageBoxButton.OK, MessageBoxImage.Error);
                                });

                                IsNextStepBtnEnabled = false;
                                IsNextBtnClicked = false;
                                IsProcessing = false;
                                return;
                            }
                        }
                    }
                    #endregion

                    #region step 3: scan with maximum gain, 10x2 loops
                    //if (boxResult != MessageBoxResult.No)
                    {
                        SubProcessName = "增益稳定性测试";
                        SubProcessPercent = 0;

                        storePath = newStorePath + "Stability\\";
                        if (!Directory.Exists(storePath))
                        {
                            Directory.CreateDirectory(storePath);
                        }
                        //Set APD Gain  500     //Set PMT Gain 4000         //Set lasers 10mw
                        APDGain = 500;
                        PMTGain = 4000;
                        for (int k = 0; k < Workspace.This.IVVM.GainComModule.Count; k++)
                        {
                            if (Workspace.This.IVVM.GainComModule[k].DisplayName == APDGain.ToString())
                            {
                                if (IvWLL1 != 0)
                                {
                                    Workspace.This.IVVM.SelectedGainComModuleL1 = Workspace.This.IVVM.GainComModule[k];
                                    Workspace.This.IVVM.GainTxtModuleL1 = PMTGain;
                                    Workspace.This.IVVM.LaserAPower = Power;
                                }
                                if (IvWLR1 != 0)
                                {
                                    Workspace.This.IVVM.SelectedGainComModuleR1 = Workspace.This.IVVM.GainComModule[k];
                                    Workspace.This.IVVM.GainTxtModuleR1 = PMTGain;
                                    Workspace.This.IVVM.LaserBPower = Power;
                                }
                                if (IvWLR2 != 0)
                                {
                                    Workspace.This.IVVM.SelectedGainComModuleR2 = Workspace.This.IVVM.GainComModule[k];
                                    Workspace.This.IVVM.GainTxtModuleR2 = PMTGain;
                                    Workspace.This.IVVM.LaserCPower = Power;
                                }
                            }
                        }
                        for (int i = 0; i < 10; i++)
                        {

                            // turn on the lasers
                            if (IvWLL1 != 0)
                            {
                                Workspace.This.IVVM.IsLaserL1Selected = true;
                                Thread.Sleep(2000);
                                Workspace.This.IVVM.IsLaserL1Selected = true;
                            }
                            if (IvWLR1 != 0)
                            {
                                Workspace.This.IVVM.IsLaserR1Selected = true;
                                Thread.Sleep(2000);
                                Workspace.This.IVVM.IsLaserR1Selected = true;
                            }
                            if (IvWLR2 != 0)
                            {
                                Workspace.This.IVVM.IsLaserR2Selected = true;
                                Thread.Sleep(2000);
                                Workspace.This.IVVM.IsLaserR2Selected = true;
                            }

                            Thread.Sleep(1000);
                            if (_IsProcessCanceled)
                            {
                                _IsProcessCanceled = false;
                                IsNextStepBtnEnabled = false;
                                IsNextBtnClicked = false;
                                IsProcessing = false;
                                return;
                            }
                            Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
                            {
                                Workspace.This.ScannerVM.ExecuteScanCommand(null);
                            });

                            try
                            {
                                WaitScanningEnded();
                                if (_IsProcessCanceled)
                                {
                                    _IsProcessCanceled = false;
                                    IsNextStepBtnEnabled = false;
                                    IsNextBtnClicked = false;
                                    IsProcessing = false;
                                    return;
                                }
                            }
                            catch (ThreadAbortException e)
                            {
                                return;
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.Message);
                                return;
                            }
                            icount = 0;
                            while (icount != 3)
                            {
                                if (Workspace.This.Files.Count > 0)
                                {
                                    icount = 0;
                                    break;
                                }
                                else
                                {
                                    icount++;
                                }
                                Thread.Sleep(10000);
                            }
                            if (Workspace.This.Files.Count > 0)     // image captured successfully
                            {
                                try
                                {
                                    for (int count = 0; count < Workspace.This.Files.Count; count++)
                                    {
                                        string[] parts = Workspace.This.Files[count].FileName.Split('_');
                                        string NameType = parts[1];
                                        //if (IvWLL1 != 0 && Workspace.This.Files[count].FileName.Contains(IvWLL1.ToString()) && Workspace.This.Files[count].FileName.Contains("L"))
                                        //if (Workspace.This.Files[count].FileName.Contains("L"))
                                        if (IvWLL1 != 0 && NameType == "L")
                                        {
                                            int _pga = 1;
                                            if (IvWLL1 == 784)
                                            {
                                                _pga = 8;
                                            }
                                            DateTime dt = DateTime.Now;
                                            string strFileTime = "_" + string.Format("{0}-{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                                            if (Workspace.This.IVVM.SensorML1 == IvSensorType.APD)
                                            {
                                                Workspace.This.Files[count].FilePath = string.Format("{0}{1}-" + Power + "mW-Gain{2}-PGA" + _pga + "-Res{3}um-Quality1--{4}" + strFileTime + ".tif", storePath, IvWLL1 + Workspace.This.IVVM.WL1Sign + "-L", APDGain, Workspace.This.ScannerVM.SelectedResolution.Value, i);
                                                SavePicture(count, false);
                                            }
                                            else
                                            {
                                                Workspace.This.Files[count].FilePath = string.Format("{0}{1}-" + Power + "mW-Gain{2}-PGA1-Res{3}um-Quality1--{4}" + strFileTime + ".tif", storePath, IvWLL1 + Workspace.This.IVVM.WL1Sign + "-L", PMTGain, Workspace.This.ScannerVM.SelectedResolution.Value, i);
                                                SavePicture(count, false);
                                            }
                                        }
                                        //if (IvWLR1 != 0 && Workspace.This.Files[count].FileName.Contains(IvWLR1.ToString()) && Workspace.This.Files[count].FileName.Contains("R1"))
                                        //if (Workspace.This.Files[count].FileName.Contains("R1"))
                                        if (IvWLR1 != 0 && NameType == "R1")
                                        {
                                            int _pga = 1;
                                            if (IvWLR1 == 784)
                                            {
                                                _pga = 8;
                                            }
                                            DateTime dt = DateTime.Now;
                                            string strFileTime = "_" + string.Format("{0}-{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                                            if (Workspace.This.IVVM.SensorMR1 == IvSensorType.APD)
                                            {

                                                Workspace.This.Files[count].FilePath = string.Format("{0}{1}-" + Power + "mW-Gain{2}-PGA" + _pga + "-Res{3}um-Quality1--{4}" + strFileTime + ".tif", storePath, IvWLR1 + Workspace.This.IVVM.WR1Sign + "-R1", APDGain, Workspace.This.ScannerVM.SelectedResolution.Value, i);
                                                SavePicture(count, false);
                                            }
                                            else
                                            {
                                                Workspace.This.Files[count].FilePath = string.Format("{0}{1}-" + Power + "mW-Gain{2}-PGA1-Res{3}um-Quality1--{4}" + strFileTime + ".tif", storePath, IvWLR1 + Workspace.This.IVVM.WR1Sign + "-R1", PMTGain, Workspace.This.ScannerVM.SelectedResolution.Value, i);
                                                SavePicture(count, false);
                                            }
                                        }
                                        //if (IvWLR2 != 0 && Workspace.This.Files[count].FileName.Contains(IvWLR2.ToString()) && Workspace.This.Files[count].FileName.Contains("R2"))
                                        //if (Workspace.This.Files[count].FileName.Contains("R2"))
                                        if (IvWLR2 != 0 && NameType == "R2")
                                        {
                                            int _pga = 1;
                                            if (IvWLR2 == 784)
                                            {
                                                _pga = 8;
                                            }
                                            DateTime dt = DateTime.Now;
                                            string strFileTime = "_" + string.Format("{0}-{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                                            if (Workspace.This.IVVM.SensorMR2 == IvSensorType.APD)
                                            {
                                                Workspace.This.Files[count].FilePath = string.Format("{0}{1}-" + Power + "mW-Gain{2}-PGA" + _pga + "-Res{3}um-Quality1--{4}" + strFileTime + ".tif", storePath, IvWLR2 + Workspace.This.IVVM.WR2Sign + "-R2", APDGain, Workspace.This.ScannerVM.SelectedResolution.Value, i);
                                                SavePicture(count, false);
                                            }
                                            else
                                            {
                                                Workspace.This.Files[count].FilePath = string.Format("{0}{1}-" + Power + "mW-Gain{2}-PGA1-Res{3}um-Quality1--{4}" + strFileTime + ".tif", storePath, IvWLR2 + Workspace.This.IVVM.WR2Sign + "-R2", PMTGain, Workspace.This.ScannerVM.SelectedResolution.Value, i);
                                                SavePicture(count, false);
                                            }
                                        }
                                    }


                                    // clear all Images in Gallery Tab
                                    Thread.Sleep(1000);
                                    RemoveAllImages();
                                    Workspace.This.SelectedTabIndex = (int)ApplicationTabType.Imaging;
                                }
                                catch (Exception exception)
                                {
                                    // Rethrow to preserve stack details
                                    // Satisfies the rule. 
                                    //throw;
                                    Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
                                    {
                                        MessageBox.Show(exception.Message);
                                        IsNextStepBtnEnabled = false;
                                        IsNextBtnClicked = false;
                                        IsProcessing = false;
                                        return;
                                    });
                                }
                                finally
                                {
                                    SubProcessPercent = 5 + (i) * 10;
                                    totalStep++;
                                    TotalProcessPercent = totalStep * 100 / totalStepDef;
                                }
                            }
                            else        // failed to capture picture
                            {
                                Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
                                {
                                    MessageBox.Show("采集图片失败！", "最大增益测试失败", MessageBoxButton.OK, MessageBoxImage.Error);
                                });

                                IsNextStepBtnEnabled = false;
                                IsNextBtnClicked = false;
                                IsProcessing = false;
                                return;
                            }

                        }
                    }
                    #endregion step 3: scan with maximum gain, 10x2 loops

                    #region step 4: scan with diffrent qualities(2, 4), 4x2 loops
                    SubProcessName = "Quality测试";
                    SubProcessPercent = 0;
                    storePath = newStorePath + "Quality \\";
                    if (!Directory.Exists(storePath))
                    {
                        Directory.CreateDirectory(storePath);
                    }
                    //select WL==784{ PGA=8  } 
                    if (IvWLL1 != 0 && IvWLL1 == 784)
                    {
                        Workspace.This.IVVM.SelectedMModuleL1 = Workspace.This.IVVM.PGAOptionsModule[3];
                    }
                    else if (IvWLL1 != 0 && IvWLL1 != 784)
                    {
                        Workspace.This.IVVM.SelectedMModuleL1 = Workspace.This.IVVM.PGAOptionsModule[0];
                    }

                    if (IvWLR1 != 0 && IvWLR1 == 784)
                    {
                        Workspace.This.IVVM.SelectedMModuleR1 = Workspace.This.IVVM.PGAOptionsModule[3];
                    }
                    else if (IvWLR1 != 0 && IvWLR1 != 784)
                    {
                        Workspace.This.IVVM.SelectedMModuleR1 = Workspace.This.IVVM.PGAOptionsModule[0];
                    }

                    if (IvWLR2 != 0 && IvWLR2 == 784)
                    {
                        Workspace.This.IVVM.SelectedMModuleR2 = Workspace.This.IVVM.PGAOptionsModule[3];
                    }
                    else if (IvWLR2 != 0 && IvWLR2 != 784)
                    {
                        Workspace.This.IVVM.SelectedMModuleR2 = Workspace.This.IVVM.PGAOptionsModule[0];
                    }
                    //select PGA=1
                    //for (int i = 0; i < Workspace.This.IVVM.PGAOptionsModule.Count; i++)
                    //{
                    //    if (Workspace.This.IVVM.PGAOptionsModule[i].DisplayName == "1")
                    //    {
                    //        if (IvWLL1 != 0)
                    //            Workspace.This.IVVM.SelectedMModuleL1 = Workspace.This.IVVM.PGAOptionsModule[i];
                    //        if (IvWLR1 != 0)
                    //            Workspace.This.IVVM.SelectedMModuleR1 = Workspace.This.IVVM.PGAOptionsModule[i];
                    //        if (IvWLR2 != 0)
                    //            Workspace.This.IVVM.SelectedMModuleR2 = Workspace.This.IVVM.PGAOptionsModule[i];
                    //        break;
                    //    }
                    //}
                    //Set PMT Gain 4000      //Set lasers 10mw
                    APDGain = 500;
                    PMTGain = 4000;
                    for (int j = 0; j < Workspace.This.IVVM.GainComModule.Count; j++)
                    {
                        if (Workspace.This.IVVM.GainComModule[j].DisplayName == APDGain.ToString())
                        {
                            if (IvWLL1 != 0)
                            {
                                Workspace.This.IVVM.SelectedGainComModuleL1 = Workspace.This.IVVM.GainComModule[j];
                                Workspace.This.IVVM.GainTxtModuleL1 = PMTGain;
                                Workspace.This.IVVM.LaserAPower = Power;
                            }

                            if (IvWLR1 != 0)
                            {
                                Workspace.This.IVVM.SelectedGainComModuleR1 = Workspace.This.IVVM.GainComModule[j];
                                Workspace.This.IVVM.GainTxtModuleR1 = PMTGain;
                                Workspace.This.IVVM.LaserBPower = Power;
                            }

                            if (IvWLR2 != 0)
                            {
                                Workspace.This.IVVM.SelectedGainComModuleR2 = Workspace.This.IVVM.GainComModule[j];
                                Workspace.This.IVVM.GainTxtModuleR2 = PMTGain;
                                Workspace.This.IVVM.LaserCPower = Power;
                            }
                        }
                    }
                    for (int loopQualityNum = 0; loopQualityNum < 2; loopQualityNum++)
                    {
                        for (int loopQuality = 0; loopQuality < 2; loopQuality++)
                        {
                            if (loopQualityNum == 0)
                            {
                                for (int i = 0; i < Workspace.This.ScannerVM.QualityOptions.Count; i++)
                                {
                                    if (Workspace.This.ScannerVM.QualityOptions[i].DisplayName == "2")
                                    {
                                        Workspace.This.ScannerVM.SelectedQuality = Workspace.This.ScannerVM.QualityOptions[i];
                                        break;
                                    }

                                }
                            }
                            if (loopQualityNum == 1)
                            {
                                for (int i = 0; i < Workspace.This.ScannerVM.QualityOptions.Count; i++)
                                {
                                    if (Workspace.This.ScannerVM.QualityOptions[i].DisplayName == "4")
                                    {
                                        Workspace.This.ScannerVM.SelectedQuality = Workspace.This.ScannerVM.QualityOptions[i];
                                        break;
                                    }

                                }
                            }
                            // turn on the lasers
                            if (IvWLL1 != 0)
                            {
                                Workspace.This.IVVM.IsLaserL1Selected = true;
                                Thread.Sleep(2000);
                                Workspace.This.IVVM.IsLaserL1Selected = true;
                            }
                            if (IvWLR1 != 0)
                            {
                                Workspace.This.IVVM.IsLaserR1Selected = true;
                                Thread.Sleep(2000);
                                Workspace.This.IVVM.IsLaserR1Selected = true;
                            }
                            if (IvWLR2 != 0)
                            {
                                Workspace.This.IVVM.IsLaserR2Selected = true;
                                Thread.Sleep(2000);
                                Workspace.This.IVVM.IsLaserR2Selected = true;
                            }
                            Thread.Sleep(1000);
                            if (_IsProcessCanceled)
                            {
                                _IsProcessCanceled = false;
                                IsNextStepBtnEnabled = false;
                                IsNextBtnClicked = false;
                                IsProcessing = false;
                                return;
                            }
                            Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
                            {
                                Workspace.This.ScannerVM.ExecuteScanCommand(null);
                            });

                            try
                            {
                                WaitScanningEnded();
                                if (_IsProcessCanceled)
                                {
                                    _IsProcessCanceled = false;
                                    IsNextStepBtnEnabled = false;
                                    IsNextBtnClicked = false;
                                    IsProcessing = false;
                                    return;
                                }
                            }
                            catch (ThreadAbortException e)
                            {
                                return;
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.Message);
                                return;
                            }
                            icount = 0;
                            while (icount != 3)
                            {
                                if (Workspace.This.Files.Count > 0)
                                {
                                    icount = 0;
                                    break;
                                }
                                else
                                {
                                    icount++;
                                }
                                Thread.Sleep(10000);
                            }
                            if (Workspace.This.Files.Count > 0)     // image captured successfully
                            {
                                try
                                {
                                    for (int count = 0; count < Workspace.This.Files.Count; count++)
                                    {
                                        string[] parts = Workspace.This.Files[count].FileName.Split('_');
                                        string NameType = parts[1];
                                        //if (IvWLL1 != 0 && Workspace.This.Files[count].FileName.Contains(IvWLL1.ToString())&& Workspace.This.Files[count].FileName.Contains("L"))
                                        //if (Workspace.This.Files[count].FileName.Contains("L"))
                                        if (IvWLL1 != 0 && NameType == "L")
                                        {
                                            int _pga = 1;
                                            if (IvWLL1 == 784)
                                            {
                                                _pga = 8;
                                            }
                                            DateTime dt = DateTime.Now;
                                            string strFileTime = "_" + string.Format("{0}-{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                                            if (Workspace.This.IVVM.SensorML1 == IvSensorType.APD)
                                            {
                                                Workspace.This.Files[count].FilePath = string.Format("{0}{1}-" + Power + "mW-Gain{2}-PGA" + _pga + "-Res{3}um-Quality{4}--{5}" + strFileTime + ".tif", storePath, IvWLL1 + Workspace.This.IVVM.WL1Sign + "-L", APDGain, Workspace.This.ScannerVM.SelectedResolution.Value, Workspace.This.ScannerVM.SelectedQuality.Value, loopQuality);
                                                SavePicture(count, false);
                                            }
                                            else
                                            {
                                                Workspace.This.Files[count].FilePath = string.Format("{0}{1}-" + Power + "mW-Gain{2}-PGA1-Res{3}um-Quality{4}--{5}" + strFileTime + ".tif", storePath, IvWLL1 + Workspace.This.IVVM.WL1Sign + "-L", PMTGain, Workspace.This.ScannerVM.SelectedResolution.Value, Workspace.This.ScannerVM.SelectedQuality.Value, loopQuality);
                                                SavePicture(count, false);
                                            }

                                        }
                                        //if (IvWLR1 != 0 && Workspace.This.Files[count].FileName.Contains(IvWLR1.ToString())&& Workspace.This.Files[count].FileName.Contains("R1"))
                                        //if (Workspace.This.Files[count].FileName.Contains("R1"))
                                        if (IvWLR1 != 0 && NameType == "R1")
                                        {
                                            int _pga = 1;
                                            if (IvWLR1 == 784)
                                            {
                                                _pga = 8;
                                            }
                                            DateTime dt = DateTime.Now;
                                            string strFileTime = "_" + string.Format("{0}-{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                                            if (Workspace.This.IVVM.SensorMR1 == IvSensorType.APD)//B
                                            {
                                                Workspace.This.Files[count].FilePath = string.Format("{0}{1}-" + Power + "mW-Gain{2}-PGA" + _pga + "-Res{3}um-Quality{4}--{5}" + strFileTime + ".tif", storePath, IvWLR1 + Workspace.This.IVVM.WR1Sign + "-R1", APDGain, Workspace.This.ScannerVM.SelectedResolution.Value, Workspace.This.ScannerVM.SelectedQuality.Value, loopQuality);
                                                SavePicture(count, false);
                                            }
                                            else
                                            {
                                                Workspace.This.Files[count].FilePath = string.Format("{0}{1}-" + Power + "mW-Gain{2}-PGA1-Res{3}um-Quality{4}--{5}" + strFileTime + ".tif", storePath, IvWLR1 + Workspace.This.IVVM.WR1Sign + "-R1", PMTGain, Workspace.This.ScannerVM.SelectedResolution.Value, Workspace.This.ScannerVM.SelectedQuality.Value, loopQuality);
                                                SavePicture(count, false);
                                            }

                                        }
                                        //if (IvWLR2 != 0 && Workspace.This.Files[count].FileName.Contains(IvWLR2.ToString())&& Workspace.This.Files[count].FileName.Contains("R2"))
                                        //if (Workspace.This.Files[count].FileName.Contains("R2"))
                                        if (IvWLR2 != 0 && NameType == "R2")
                                        {
                                            int _pga = 1;
                                            if (IvWLR2 == 784)
                                            {
                                                _pga = 8;
                                            }
                                            DateTime dt = DateTime.Now;
                                            string strFileTime = "_" + string.Format("{0}-{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                                            if (Workspace.This.IVVM.SensorMR2 == IvSensorType.APD)
                                            {
                                                Workspace.This.Files[count].FilePath = string.Format("{0}{1}-" + Power + "mW-Gain{2}-PGA" + _pga + "-Res{3}um-Quality{4}--{5}" + strFileTime + ".tif", storePath, IvWLR2 + Workspace.This.IVVM.WR2Sign + "-R2", APDGain, Workspace.This.ScannerVM.SelectedResolution.Value, Workspace.This.ScannerVM.SelectedQuality.Value, loopQuality);
                                                SavePicture(count, false);
                                            }
                                            else
                                            {
                                                Workspace.This.Files[count].FilePath = string.Format("{0}{1}-" + Power + "mW-Gain{2}-PGA1-Res{3}um-Quality{4}--{5}" + strFileTime + ".tif", storePath, IvWLR2 + Workspace.This.IVVM.WR2Sign + "-R2", PMTGain, Workspace.This.ScannerVM.SelectedResolution.Value, Workspace.This.ScannerVM.SelectedQuality.Value, loopQuality);
                                                SavePicture(count, false);
                                            }

                                        }
                                    }
                                    // clear all Images in Gallery Tab
                                    Thread.Sleep(1000);
                                    RemoveAllImages();
                                    Workspace.This.SelectedTabIndex = (int)ApplicationTabType.Imaging;
                                }
                                catch (Exception exception)
                                {
                                    // Rethrow to preserve stack details
                                    // Satisfies the rule. 
                                    //throw;
                                    Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
                                    {
                                        MessageBox.Show(exception.Message);
                                        IsNextStepBtnEnabled = false;
                                        IsNextBtnClicked = false;
                                        IsProcessing = false;
                                        return;
                                    });
                                }
                                finally
                                {
                                    SubProcessPercent = (loopQualityNum) * 25 + (int)((loopQuality + 1) * 12.5);
                                    totalStep++;
                                    TotalProcessPercent = totalStep * 100 / totalStepDef;

                                }
                            }
                            else        // failed to capture picture
                            {
                                Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
                                {
                                    MessageBox.Show("采集图片失败！", "Quality测试失败", MessageBoxButton.OK, MessageBoxImage.Error);
                                });

                                IsNextStepBtnEnabled = false;
                                IsNextBtnClicked = false;
                                IsProcessing = false;
                                return;
                            }

                        }
                    }

                    #endregion

                    #region step 5  Multiple regions scan
                    SubProcessName = "多区域扫描";
                    SubProcessPercent = 0;
                    storePath = newStorePath + "Rois\\";

                    if (!Directory.Exists(storePath))
                    {
                        Directory.CreateDirectory(storePath);
                    }
                    //select Quality=1,
                    for (int i = 0; i < Workspace.This.ScannerVM.QualityOptions.Count; i++)
                    {
                        if (Workspace.This.ScannerVM.QualityOptions[i].DisplayName == "1")
                        {
                            Workspace.This.ScannerVM.SelectedQuality = Workspace.This.ScannerVM.QualityOptions[i];
                            break;
                        }
                    }

                    //select WL==784{ PGA=8  } 
                    if (IvWLL1 != 0 && IvWLL1 == 784)
                    {
                        Workspace.This.IVVM.SelectedMModuleL1 = Workspace.This.IVVM.PGAOptionsModule[3];
                    }
                    else if (IvWLL1 != 0 && IvWLL1 != 784)
                    {
                        Workspace.This.IVVM.SelectedMModuleL1 = Workspace.This.IVVM.PGAOptionsModule[0];
                    }

                    if (IvWLR1 != 0 && IvWLR1 == 784)
                    {
                        Workspace.This.IVVM.SelectedMModuleR1 = Workspace.This.IVVM.PGAOptionsModule[3];
                    }
                    else if (IvWLR1 != 0 && IvWLR1 != 784)
                    {
                        Workspace.This.IVVM.SelectedMModuleR1 = Workspace.This.IVVM.PGAOptionsModule[0];
                    }

                    if (IvWLR2 != 0 && IvWLR2 == 784)
                    {
                        Workspace.This.IVVM.SelectedMModuleR2 = Workspace.This.IVVM.PGAOptionsModule[3];
                    }
                    else if (IvWLR2 != 0 && IvWLR2 != 784)
                    {
                        Workspace.This.IVVM.SelectedMModuleR2 = Workspace.This.IVVM.PGAOptionsModule[0];
                    }
                    APDGain = 500;
                    PMTGain = 4000;
                    for (int k = 0; k < Workspace.This.IVVM.GainComModule.Count; k++)
                    {
                        if (Workspace.This.IVVM.GainComModule[k].DisplayName == APDGain.ToString())
                        {
                            if (IvWLL1 != 0)
                            {
                                Workspace.This.IVVM.SelectedGainComModuleL1 = Workspace.This.IVVM.GainComModule[k];
                                Workspace.This.IVVM.GainTxtModuleL1 = PMTGain;
                            }
                            if (IvWLR1 != 0)
                            {
                                Workspace.This.IVVM.SelectedGainComModuleR1 = Workspace.This.IVVM.GainComModule[k];
                                Workspace.This.IVVM.GainTxtModuleR1 = PMTGain;
                            }
                            if (IvWLR2 != 0)
                            {
                                Workspace.This.IVVM.SelectedGainComModuleR2 = Workspace.This.IVVM.GainComModule[k];
                                Workspace.This.IVVM.GainTxtModuleR2 = PMTGain;
                            }
                            break;
                        }
                    }
                    for (int a = 1; a < 10; a++)
                    {
                        if (a == 1)
                        {
                            Workspace.This.ScannerVM.ScanX0 = X1;
                            Workspace.This.ScannerVM.ScanDeltaX = XD1;
                            Workspace.This.ScannerVM.ScanY0 = Y1;
                            Workspace.This.ScannerVM.ScanDeltaY = YD1;
                        }
                        else if (a == 2)
                        {
                            Workspace.This.ScannerVM.ScanX0 = X2;
                            Workspace.This.ScannerVM.ScanDeltaX = XD2;
                            Workspace.This.ScannerVM.ScanY0 = Y1;
                            Workspace.This.ScannerVM.ScanDeltaY = YD1;
                        }
                        else if (a == 3)
                        {
                            Workspace.This.ScannerVM.ScanX0 = X3;
                            Workspace.This.ScannerVM.ScanDeltaX = XD3;
                            Workspace.This.ScannerVM.ScanY0 = Y1;
                            Workspace.This.ScannerVM.ScanDeltaY = YD1;
                        }
                        else if (a == 4)
                        {
                            Workspace.This.ScannerVM.ScanX0 = X1;
                            Workspace.This.ScannerVM.ScanDeltaX = XD1;
                            Workspace.This.ScannerVM.ScanY0 = Y2;
                            Workspace.This.ScannerVM.ScanDeltaY = YD2;
                        }
                        else if (a == 5)
                        {
                            Workspace.This.ScannerVM.ScanX0 = X2;
                            Workspace.This.ScannerVM.ScanDeltaX = XD2;
                            Workspace.This.ScannerVM.ScanY0 = Y2;
                            Workspace.This.ScannerVM.ScanDeltaY = YD2;
                        }
                        else if (a == 6)
                        {
                            Workspace.This.ScannerVM.ScanX0 = X3;
                            Workspace.This.ScannerVM.ScanDeltaX = XD3;
                            Workspace.This.ScannerVM.ScanY0 = Y2;
                            Workspace.This.ScannerVM.ScanDeltaY = YD2;
                        }
                        else if (a == 7)
                        {
                            Workspace.This.ScannerVM.ScanX0 = X1;
                            Workspace.This.ScannerVM.ScanDeltaX = XD1;
                            Workspace.This.ScannerVM.ScanY0 = Y3;
                            Workspace.This.ScannerVM.ScanDeltaY = YD3;
                        }
                        else if (a == 8)
                        {
                            Workspace.This.ScannerVM.ScanX0 = X2;
                            Workspace.This.ScannerVM.ScanDeltaX = XD2;
                            Workspace.This.ScannerVM.ScanY0 = Y3;
                            Workspace.This.ScannerVM.ScanDeltaY = YD3;
                        }
                        else if (a == 9)
                        {
                            Workspace.This.ScannerVM.ScanX0 = X3;
                            Workspace.This.ScannerVM.ScanDeltaX = XD3;
                            Workspace.This.ScannerVM.ScanY0 = Y3;
                            Workspace.This.ScannerVM.ScanDeltaY = YD3;
                        }
                        // turn on the lasers
                        if (IvWLL1 != 0)
                        {
                            Workspace.This.IVVM.IsLaserL1Selected = true;
                            Thread.Sleep(2000);
                            Workspace.This.IVVM.IsLaserL1Selected = true;
                        }
                        if (IvWLR1 != 0)
                        {
                            Workspace.This.IVVM.IsLaserR1Selected = true;
                            Thread.Sleep(2000);
                            Workspace.This.IVVM.IsLaserR1Selected = true;
                        }
                        if (IvWLR2 != 0)
                        {
                            Workspace.This.IVVM.IsLaserR2Selected = true;
                            Thread.Sleep(2000);
                            Workspace.This.IVVM.IsLaserR2Selected = true;
                        }
                        Thread.Sleep(1000);
                        if (_IsProcessCanceled)
                        {
                            _IsProcessCanceled = false;
                            IsNextStepBtnEnabled = false;
                            IsNextBtnClicked = false;
                            IsProcessing = false;
                            return;
                        }
                        Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
                        {
                            Workspace.This.ScannerVM.ExecuteScanCommand(null);
                        });
                        try
                        {
                            WaitScanningEnded();
                            if (_IsProcessCanceled)
                            {
                                _IsProcessCanceled = false;
                                IsNextStepBtnEnabled = false;
                                IsNextBtnClicked = false;
                                IsProcessing = false;
                                return;
                            }
                        }
                        catch (ThreadAbortException e)
                        {
                            return;
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message);
                            return;
                        }
                        icount = 0;
                        while (icount != 3)
                        {
                            if (Workspace.This.Files.Count > 0)
                            {
                                icount = 0;
                                break;
                            }
                            else
                            {
                                icount++;
                            }
                            Thread.Sleep(10000);
                        }
                        if (Workspace.This.Files.Count > 0)     // image captured successfully
                        {
                            try
                            {
                                for (int count = 0; count < Workspace.This.Files.Count; count++)
                                {
                                    string[] parts = Workspace.This.Files[count].FileName.Split('_');
                                    string NameType = parts[1];
                                    //if (IvWLL1 != 0 && Workspace.This.Files[count].FileName.Contains(IvWLL1.ToString()) && Workspace.This.Files[count].FileName.Contains("L"))
                                    //if (Workspace.This.Files[count].FileName.Contains("L"))
                                    if (IvWLL1 != 0 && NameType == "L")
                                    {
                                        int _pga = 1;
                                        if (IvWLL1 == 784)
                                        {
                                            _pga = 8;
                                        }
                                        DateTime dt = DateTime.Now;
                                        string strFileTime = "ROI" + a + "_" + string.Format("{0}-{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                                        if (Workspace.This.IVVM.SensorML1 == IvSensorType.APD)
                                        {
                                            Workspace.This.Files[count].FilePath = string.Format("{0}{1}-0mW-Gain{2}-PGA" + _pga + "-Res{3}um-Quality1" + strFileTime + ".tif", storePath, IvWLL1 + Workspace.This.IVVM.WL1Sign + "-L", APDGain, Workspace.This.ScannerVM.SelectedResolution.Value);
                                            SavePicture(count, false);
                                        }
                                        else
                                        {
                                            Workspace.This.Files[count].FilePath = string.Format("{0}{1}-0mW-Gain{2}-PGA8-Res{3}um-Quality1" + strFileTime + ".tif", storePath, IvWLL1 + Workspace.This.IVVM.WL1Sign + "-L", PMTGain, Workspace.This.ScannerVM.SelectedResolution.Value);
                                            SavePicture(count, false);
                                        }

                                    }
                                    //if (IvWLR1 != 0 && Workspace.This.Files[count].FileName.Contains(IvWLR1.ToString()) && Workspace.This.Files[count].FileName.Contains("R1"))
                                    //if (Workspace.This.Files[count].FileName.Contains("R1"))
                                    if (IvWLR1 != 0 && NameType == "R1")
                                    {
                                        int _pga = 1;
                                        if (IvWLR1 == 784)
                                        {
                                            _pga = 8;
                                        }
                                        DateTime dt = DateTime.Now;
                                        string strFileTime = "ROI" + a + "_" + string.Format("{0}-{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                                        if (Workspace.This.IVVM.SensorMR1 == IvSensorType.APD)//B
                                        {
                                            Workspace.This.Files[count].FilePath = string.Format("{0}{1}-0mW-Gain{2}-PGA" + _pga + "-Res{3}um-Quality1" + strFileTime + ".tif", storePath, IvWLR1 + Workspace.This.IVVM.WR1Sign + "-R1", APDGain, Workspace.This.ScannerVM.SelectedResolution.Value);
                                            SavePicture(count, false);
                                        }
                                        else
                                        {
                                            Workspace.This.Files[count].FilePath = string.Format("{0}{1}-0mW-Gain{2}-PGA8-Res{3}um-Quality1" + strFileTime + ".tif", storePath, IvWLR1 + Workspace.This.IVVM.WR1Sign + "-R1", PMTGain, Workspace.This.ScannerVM.SelectedResolution.Value);
                                            SavePicture(count, false);
                                        }
                                    }
                                    //if (IvWLR2 != 0 && Workspace.This.Files[count].FileName.Contains(IvWLR2.ToString()) && Workspace.This.Files[count].FileName.Contains("R2"))
                                    //if (Workspace.This.Files[count].FileName.Contains("R2"))
                                    if (IvWLR2 != 0 && NameType == "R2")
                                    {
                                        int _pga = 1;
                                        if (IvWLR2 == 784)
                                        {
                                            _pga = 8;
                                        }
                                        DateTime dt = DateTime.Now;
                                        string strFileTime = "ROI" + a + "_" + string.Format("{0}-{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                                        if (Workspace.This.IVVM.SensorMR2 == IvSensorType.APD)
                                        {
                                            Workspace.This.Files[count].FilePath = string.Format("{0}{1}-0mW-Gain{2}-PGA" + _pga + "-Res{3}um-Quality1" + strFileTime + ".tif", storePath, IvWLR2 + Workspace.This.IVVM.WR2Sign + "-R2", APDGain, Workspace.This.ScannerVM.SelectedResolution.Value);
                                            SavePicture(count, false);
                                        }
                                        else
                                        {
                                            Workspace.This.Files[count].FilePath = string.Format("{0}{1}-0mW-Gain{2}-PGA8-Res{3}um-Quality1" + strFileTime + ".tif", storePath, IvWLR2 + Workspace.This.IVVM.WR2Sign + "-R2", PMTGain, Workspace.This.ScannerVM.SelectedResolution.Value);
                                            SavePicture(count, false);
                                        }
                                    }
                                }
                                // clear all Images in Gallery Tab
                                Thread.Sleep(1000);
                                RemoveAllImages();
                                Workspace.This.SelectedTabIndex = (int)ApplicationTabType.Imaging;
                            }
                            catch (Exception exception)
                            {
                                // Rethrow to preserve stack details
                                // Satisfies the rule. 
                                //throw;
                                Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
                                {
                                    MessageBox.Show(exception.Message);
                                    IsNextStepBtnEnabled = false;
                                    IsNextBtnClicked = false;
                                    IsProcessing = false;
                                    return;
                                });
                            }
                            finally
                            {
                                //SubProcessPercent = 100;
                                //totalStep++;
                                //TotalProcessPercent = totalStep * 100 / totalStepDef;
                            }
                        }
                        else        // failed to capture picture
                        {
                            Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
                            {
                                MessageBox.Show("多区域扫描图片失败！", "多区域扫描失败", MessageBoxButton.OK, MessageBoxImage.Error);
                            });

                            IsNextStepBtnEnabled = false;
                            IsNextBtnClicked = false;
                            IsProcessing = false;
                            return;
                        }
                    }
                    //Workspace.This.ScannerVM.ScanX0 = ScanX0;
                    //Workspace.This.ScannerVM.ScanDeltaX= ScanDeltaX;
                    //Workspace.This.ScannerVM.ScanY0 = ScanY0;
                    //Workspace.This.ScannerVM.ScanDeltaY=ScanDeltaY;
                    //Workspace.This.IVVM.LaserAPower = PowerL;
                    //Workspace.This.IVVM.LaserBPower = PowerR1;
                    //Workspace.This.IVVM.LaserCPower = PowerR2;
                    // Workspace.This.ScannerVM.SelectedResolution = res;
                    #endregion

                    //#region step 6  Laser temperature difference
                    //if (loopCount == 0)
                    //{
                    //    //375模块不执行这个，因为375会模块执行这个会烧坏掉激光器
                    //    if (IvWLL1 != 375 && IvWLR1 != 375 && IvWLR2 != 375)
                    //    {
                    //        Workspace.This.NewParameterVM.FanReserveTemperature = 40;
                    //        Workspace.This.NewParameterVM.PublicExecuteParametersWriteCommand();
                    //        Thread.Sleep(2000);
                    //        Workspace.This.ScannerVM.ScanX0 = ScanX0;
                    //        Workspace.This.ScannerVM.ScanDeltaX = ScanDeltaX;
                    //        Workspace.This.ScannerVM.ScanY0 = 10;
                    //        Workspace.This.ScannerVM.ScanDeltaY = 250;
                    //        Workspace.This.IVVM.LaserAPower = 20;
                    //        Workspace.This.IVVM.LaserBPower = 20;
                    //        Workspace.This.IVVM.LaserCPower = 20;


                    //        //select resolution 100um
                    //        for (int i = 0; i < Workspace.This.ScannerVM.ResolutionOptions.Count; i++)
                    //        {
                    //            if (Workspace.This.ScannerVM.ResolutionOptions[i].DisplayName == "100")
                    //            {
                    //                Workspace.This.ScannerVM.SelectedResolution = Workspace.This.ScannerVM.ResolutionOptions[i];
                    //                break;
                    //            }
                    //        }
                    //        SubProcessName = "记录模块风扇温度";
                    //        SubProcessPercent = 0;
                    //        storePath = newStorePath + "ModeTemperaturediff\\";

                    //        if (!Directory.Exists(storePath))
                    //        {
                    //            Directory.CreateDirectory(storePath);
                    //        }
                    //        string timeNow = FormatStrDate();
                    //        _filePath = storePath + ((loopCount + 1) + "_TemperatureReport.csv");
                    //        GenerateReportTemperature(_filePath);
                    //        StrTemperature[0] = string.Format("\n{0},{1},{2},{3},{4},{5},{6}", Workspace.This.IVVM.SensorTemperatureL1, Workspace.This.IVVM.SensorTemperatureR1, Workspace.This.IVVM.SensorTemperatureR2, Workspace.This.IVVM.SensorRadTemperaTureL1, Workspace.This.IVVM.SensorRadTemperaTureR1, Workspace.This.IVVM.SensorRadTemperaTureR2, timeNow);
                    //        WriteReportData(StrTemperature);
                    //        //select Quality=4,
                    //        for (int i = 0; i < Workspace.This.ScannerVM.QualityOptions.Count; i++)
                    //        {
                    //            if (Workspace.This.ScannerVM.QualityOptions[i].DisplayName == "4")
                    //            {
                    //                Workspace.This.ScannerVM.SelectedQuality = Workspace.This.ScannerVM.QualityOptions[i];
                    //                break;
                    //            }
                    //        }

                    //        //select WL==784{ PGA=8  } 
                    //        if (IvWLL1 != 0 && IvWLL1 == 784)
                    //        {
                    //            Workspace.This.IVVM.SelectedMModuleL1 = Workspace.This.IVVM.PGAOptionsModule[3];
                    //        }
                    //        else if (IvWLL1 != 0 && IvWLL1 != 784)
                    //        {
                    //            Workspace.This.IVVM.SelectedMModuleL1 = Workspace.This.IVVM.PGAOptionsModule[0];
                    //        }

                    //        if (IvWLR1 != 0 && IvWLR1 == 784)
                    //        {
                    //            Workspace.This.IVVM.SelectedMModuleR1 = Workspace.This.IVVM.PGAOptionsModule[3];
                    //        }
                    //        else if (IvWLR1 != 0 && IvWLR1 != 784)
                    //        {
                    //            Workspace.This.IVVM.SelectedMModuleR1 = Workspace.This.IVVM.PGAOptionsModule[0];
                    //        }

                    //        if (IvWLR2 != 0 && IvWLR2 == 784)
                    //        {
                    //            Workspace.This.IVVM.SelectedMModuleR2 = Workspace.This.IVVM.PGAOptionsModule[3];
                    //        }
                    //        else if (IvWLR2 != 0 && IvWLR2 != 784)
                    //        {
                    //            Workspace.This.IVVM.SelectedMModuleR2 = Workspace.This.IVVM.PGAOptionsModule[0];
                    //        }
                    //        APDGain = 50;
                    //        PMTGain = 4000;
                    //        for (int k = 0; k < Workspace.This.IVVM.GainComModule.Count; k++)
                    //        {
                    //            if (Workspace.This.IVVM.GainComModule[k].DisplayName == APDGain.ToString())
                    //            {
                    //                if (IvWLL1 != 0)
                    //                {
                    //                    Workspace.This.IVVM.SelectedGainComModuleL1 = Workspace.This.IVVM.GainComModule[k];
                    //                    Workspace.This.IVVM.GainTxtModuleL1 = PMTGain;
                    //                }
                    //                if (IvWLR1 != 0)
                    //                {
                    //                    Workspace.This.IVVM.SelectedGainComModuleR1 = Workspace.This.IVVM.GainComModule[k];
                    //                    Workspace.This.IVVM.GainTxtModuleR1 = PMTGain;
                    //                }
                    //                if (IvWLR2 != 0)
                    //                {
                    //                    Workspace.This.IVVM.SelectedGainComModuleR2 = Workspace.This.IVVM.GainComModule[k];
                    //                    Workspace.This.IVVM.GainTxtModuleR2 = PMTGain;
                    //                }
                    //                break;
                    //            }
                    //        }
                    //        // turn on the lasers
                    //        if (IvWLL1 != 0)
                    //        {
                    //            Workspace.This.IVVM.IsLaserL1Selected = true;
                    //            Thread.Sleep(2000);
                    //            Workspace.This.IVVM.IsLaserL1Selected = true;
                    //        }
                    //        if (IvWLR1 != 0)
                    //        {
                    //            Workspace.This.IVVM.IsLaserR1Selected = true;
                    //            Thread.Sleep(2000);
                    //            Workspace.This.IVVM.IsLaserR1Selected = true;
                    //        }
                    //        if (IvWLR2 != 0)
                    //        {
                    //            Workspace.This.IVVM.IsLaserR2Selected = true;
                    //            Thread.Sleep(2000);
                    //            Workspace.This.IVVM.IsLaserR2Selected = true;
                    //        }
                    //        Thread.Sleep(1000);
                    //        if (_IsProcessCanceled)
                    //        {
                    //            _IsProcessCanceled = false;
                    //            IsNextStepBtnEnabled = false;
                    //            IsNextBtnClicked = false;
                    //            IsProcessing = false;
                    //            return;
                    //        }
                    //        Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
                    //        {
                    //            Workspace.This.ScannerVM.ExecuteScanCommand(null);
                    //        });
                    //        try
                    //        {
                    //            WaitScanningEnded();
                    //            if (_IsProcessCanceled)
                    //            {
                    //                _IsProcessCanceled = false;
                    //                IsNextStepBtnEnabled = false;
                    //                IsNextBtnClicked = false;
                    //                IsProcessing = false;
                    //                return;
                    //            }
                    //        }
                    //        catch (ThreadAbortException e)
                    //        {
                    //            return;
                    //        }
                    //        catch (Exception e)
                    //        {
                    //            MessageBox.Show(e.Message);
                    //            return;
                    //        }
                    //        icount = 0;
                    //        while (icount != 3)
                    //        {
                    //            if (Workspace.This.Files.Count > 0)
                    //            {
                    //                icount = 0;
                    //                break;
                    //            }
                    //            else
                    //            {
                    //                icount++;
                    //            }
                    //            Thread.Sleep(10000);
                    //        }
                    //        if (Workspace.This.Files.Count > 0)     // image captured successfully
                    //        {
                    //            try
                    //            {
                    //                for (int count = 0; count < Workspace.This.Files.Count; count++)
                    //                {
                    //                    string[] parts = Workspace.This.Files[count].FileName.Split('_');
                    //                    string NameType = parts[1];
                    //                    //if (IvWLL1 != 0 && Workspace.This.Files[count].FileName.Contains(IvWLL1.ToString()) && Workspace.This.Files[count].FileName.Contains("L"))
                    //                    //if (Workspace.This.Files[count].FileName.Contains("L"))
                    //                    if (IvWLL1 != 0 && NameType == "L")
                    //                    {
                    //                        DateTime dt = DateTime.Now;
                    //                        string strFileTime = "_" + string.Format("{0}-{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                    //                        if (Workspace.This.IVVM.SensorML1 == IvSensorType.APD)
                    //                        {
                    //                            Workspace.This.Files[count].FilePath = string.Format("{0}{1}-0mW-Gain{2}-PGA1-Res{3}um-Quality4" + strFileTime + ".tif", storePath, IvWLL1 + Workspace.This.IVVM.WL1Sign + "-L", APDGain, Workspace.This.ScannerVM.SelectedResolution.Value);
                    //                            SavePicture(count, false);
                    //                        }
                    //                        else
                    //                        {
                    //                            Workspace.This.Files[count].FilePath = string.Format("{0}{1}-0mW-Gain{2}-PGA1-Res{3}um-Quality4" + strFileTime + ".tif", storePath, IvWLL1 + Workspace.This.IVVM.WL1Sign + "-L", PMTGain, Workspace.This.ScannerVM.SelectedResolution.Value);
                    //                            SavePicture(count, false);
                    //                        }

                    //                    }
                    //                    //if (IvWLR1 != 0 && Workspace.This.Files[count].FileName.Contains(IvWLR1.ToString()) && Workspace.This.Files[count].FileName.Contains("R1"))
                    //                    //if (Workspace.This.Files[count].FileName.Contains("R1"))
                    //                    if (IvWLR1 != 0 && NameType == "R1")
                    //                    {
                    //                        DateTime dt = DateTime.Now;
                    //                        string strFileTime = "_" + string.Format("{0}-{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                    //                        if (Workspace.This.IVVM.SensorMR1 == IvSensorType.APD)//B
                    //                        {
                    //                            Workspace.This.Files[count].FilePath = string.Format("{0}{1}-0mW-Gain{2}-PGA1-Res{3}um-Quality4" + strFileTime + ".tif", storePath, IvWLR1 + Workspace.This.IVVM.WR1Sign + "-R1", APDGain, Workspace.This.ScannerVM.SelectedResolution.Value);
                    //                            SavePicture(count, false);
                    //                        }
                    //                        else
                    //                        {
                    //                            Workspace.This.Files[count].FilePath = string.Format("{0}{1}-0mW-Gain{2}-PGA1-Res{3}um-Quality4" + strFileTime + ".tif", storePath, IvWLR1 + Workspace.This.IVVM.WR1Sign + "-R1", PMTGain, Workspace.This.ScannerVM.SelectedResolution.Value);
                    //                            SavePicture(count, false);
                    //                        }
                    //                    }
                    //                    //if (IvWLR2 != 0 && Workspace.This.Files[count].FileName.Contains(IvWLR2.ToString()) && Workspace.This.Files[count].FileName.Contains("R2"))
                    //                    //if (Workspace.This.Files[count].FileName.Contains("R2"))
                    //                    if (IvWLR2 != 0 && NameType == "R2")
                    //                    {
                    //                        DateTime dt = DateTime.Now;
                    //                        string strFileTime = "_" + string.Format("{0}-{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                    //                        if (Workspace.This.IVVM.SensorMR2 == IvSensorType.APD)
                    //                        {
                    //                            Workspace.This.Files[count].FilePath = string.Format("{0}{1}-0mW-Gain{2}-PGA1-Res{3}um-Quality4" + strFileTime + ".tif", storePath, IvWLR2 + Workspace.This.IVVM.WR2Sign + "-R2", APDGain, Workspace.This.ScannerVM.SelectedResolution.Value);
                    //                            SavePicture(count, false);
                    //                        }
                    //                        else
                    //                        {
                    //                            Workspace.This.Files[count].FilePath = string.Format("{0}{1}-0mW-Gain{2}-PGA1-Res{3}um-Quality4" + strFileTime + ".tif", storePath, IvWLR2 + Workspace.This.IVVM.WR2Sign + "-R2", PMTGain, Workspace.This.ScannerVM.SelectedResolution.Value);
                    //                            SavePicture(count, false);
                    //                        }
                    //                    }
                    //                }
                    //                // clear all Images in Gallery Tab
                    //                Thread.Sleep(1000);
                    //                RemoveAllImages();
                    //                Workspace.This.SelectedTabIndex = (int)ApplicationTabType.Imaging;
                    //            }
                    //            catch (Exception exception)
                    //            {
                    //                // Rethrow to preserve stack details
                    //                // Satisfies the rule. 
                    //                //throw;
                    //                Workspace.This.NewParameterVM.FanReserveTemperature = 24;
                    //                Workspace.This.NewParameterVM.ExecuteParametersWriteCommand(null);
                    //                Thread.Sleep(2000);
                    //                Workspace.This.ScannerVM.ScanX0 = ScanX0;
                    //                Workspace.This.ScannerVM.ScanDeltaX = ScanDeltaX;
                    //                Workspace.This.ScannerVM.ScanY0 = ScanY0;
                    //                Workspace.This.ScannerVM.ScanDeltaY = ScanDeltaY;
                    //                Workspace.This.IVVM.LaserAPower = PowerL;
                    //                Workspace.This.IVVM.LaserBPower = PowerR1;
                    //                Workspace.This.IVVM.LaserCPower = PowerR2;
                    //                Workspace.This.ScannerVM.SelectedResolution = res;
                    //                Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
                    //                {
                    //                    MessageBox.Show(exception.Message);
                    //                    IsNextStepBtnEnabled = false;
                    //                    IsNextBtnClicked = false;
                    //                    IsProcessing = false;
                    //                    return;
                    //                });
                    //            }
                    //            finally
                    //            {
                    //                SubProcessPercent = 100;
                    //                totalStep++;
                    //                TotalProcessPercent = totalStep * 100 / totalStepDef;
                    //                Workspace.This.NewParameterVM.FanReserveTemperature = 24;
                    //                Workspace.This.NewParameterVM.ExecuteParametersWriteCommand(null);
                    //                Thread.Sleep(2000);
                    //                Workspace.This.ScannerVM.ScanX0 = ScanX0;
                    //                Workspace.This.ScannerVM.ScanDeltaX = ScanDeltaX;
                    //                Workspace.This.ScannerVM.ScanY0 = ScanY0;
                    //                Workspace.This.ScannerVM.ScanDeltaY = ScanDeltaY;
                    //                Workspace.This.IVVM.LaserAPower = PowerL;
                    //                Workspace.This.IVVM.LaserBPower = PowerR1;
                    //                Workspace.This.IVVM.LaserCPower = PowerR2;
                    //                Workspace.This.ScannerVM.SelectedResolution = res;
                    //            }
                    //        }
                    //        else        // failed to capture picture
                    //        {
                    //            Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
                    //            {
                    //                Workspace.This.NewParameterVM.FanReserveTemperature = 24;
                    //                Workspace.This.NewParameterVM.ExecuteParametersWriteCommand(null);
                    //                Thread.Sleep(2000);
                    //                Workspace.This.ScannerVM.ScanX0 = ScanX0;
                    //                Workspace.This.ScannerVM.ScanDeltaX = ScanDeltaX;
                    //                Workspace.This.ScannerVM.ScanY0 = ScanY0;
                    //                Workspace.This.ScannerVM.ScanDeltaY = ScanDeltaY;
                    //                Workspace.This.IVVM.LaserAPower = PowerL;
                    //                Workspace.This.IVVM.LaserBPower = PowerR1;
                    //                Workspace.This.IVVM.LaserCPower = PowerR2;
                    //                Workspace.This.ScannerVM.SelectedResolution = res;
                    //                MessageBox.Show("记录模块风扇温度失败！", "记录模块风扇温度失败", MessageBoxButton.OK, MessageBoxImage.Error);
                    //            });

                    //            IsNextStepBtnEnabled = false;
                    //            IsNextBtnClicked = false;
                    //            IsProcessing = false;
                    //            return;
                    //        }
                    //        string _timeNow = FormatStrDate();
                    //        StrTemperature[0] = string.Format("\n{0},{1},{2},{3},{4},{5},{6}", Workspace.This.IVVM.SensorTemperatureL1, Workspace.This.IVVM.SensorTemperatureR1, Workspace.This.IVVM.SensorTemperatureR2, Workspace.This.IVVM.SensorRadTemperaTureL1, Workspace.This.IVVM.SensorRadTemperaTureR1, Workspace.This.IVVM.SensorRadTemperaTureR2, _timeNow);
                    //        WriteReportData(StrTemperature);
                    //    }
                    //}
                    //#endregion

                }
                catch (Exception ex)
                {
                    Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
                    {
                        MessageBox.Show(ex.Message);
                        return;
                    });
                }

            }
            Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
            {
                //  _ProcessDoorState.Abort();
                MessageBox.Show("老化测试完毕！");
            });

            if (Directory.Exists(StoreFolder))
            {
                System.Diagnostics.Process.Start("Explorer.exe", StoreFolder);
            }
            IsNextStepBtnEnabled = false;
            IsNextBtnClicked = false;
            IsProcessing = false;

        }
        public bool CanExecuteBatchProcessCommand(object param)
        {
            return true;
        }
        #endregion Start Batch Process Command

        #region Stop Batch Process Command
        public ICommand StopBatchProcessCommand
        {
            get
            {
                if (_StopBatchProcessCommand == null)
                {
                    _StopBatchProcessCommand = new RelayCommand(ExecuteStopBatchProcessCommand, CanExecuteBatchProcessCommand);
                }
                return _StopBatchProcessCommand;
            }
        }
        public void ExecuteStopBatchProcessCommand(object param)
        {
            if (_Process != null)
            {
                if (Workspace.This.IsScanning || Workspace.This.IsPreparing)
                {
                    Workspace.This.Owner.Dispatcher.Invoke((Action)delegate ()
                    {
                        Workspace.This.ScannerVM.ExecuteStopScanCommand(null);
                    });
                    _IsProcessCanceled = true;
                }
                else
                {
                    _IsProcessCanceled = false;
                    IsNextStepBtnEnabled = false;
                    IsNextBtnClicked = false;
                    IsProcessing = false;
                }
                IsTECTempeEnabled = true;
            }
        }
        #endregion Stop Batch Process Command

        #region Next Step Command
        public ICommand NextStepCommand
        {
            get
            {
                if (_NextStepCommand == null)
                {
                    _NextStepCommand = new RelayCommand(ExecuteNextStepCommand, CanExecuteNextStepCommand);
                }
                return _NextStepCommand;
            }
        }

        public void ExecuteNextStepCommand(object param)
        {
            _IsNextBtnClicked = true;
            IsNextStepBtnEnabled = false;
        }
        public bool CanExecuteNextStepCommand(object param)
        {
            return true;
        }
        #endregion Next Step Command

        #region TEC温度测试
        private DispatcherTimer TECRecordTimer = null;
        APDPgaType LaPDPgaType = null;
        APDPgaType R1aPDPgaType = null;
        APDPgaType R2aPDPgaType = null;
        APDGainType LAPDGainType = null;
        APDGainType R1APDGainType = null;
        APDGainType R2APDGainType = null;
        float FanReserveTemperature = 0;
        double LTEControlTemperature = 0;
        double R1TEControlTemperature = 0;
        double R2TEControlTemperature = 0;
        double TEControlTemperature375 = 25;
        string SelectedIncrustation = "Auto";
        List<double> LTemperratureList = new List<double>();
        List<double> R1TemperratureList = new List<double>();
        List<double> R2TemperratureList = new List<double>();
        private void TemperatureShow(string Channel, string TECtemperature, double RadTemperature)
        {
            void msgSend()
            {
                Workspace.This.Owner.Dispatcher.BeginInvoke((Action)delegate
                {
                    Window window = new Window();
                    MessageBox.Show(window, Channel
                        + "激光器温度=" + TECtemperature +
                        "\n扇热器温度=" + RadTemperature +
                        "\nTime is " + FormatStrDate());
                });
            }
            Thread td_msg = new Thread(msgSend);
            td_msg.Start();
        }
        private void TECRecordTimeInit()
        {
            TECRecordTimer = new DispatcherTimer();
            TECRecordTimer.Tick += TECRecordTimer_Tick;
            TECRecordTimer.Interval = new TimeSpan(0, 0, 3);//3 sec

        }

        private void TECRecordTimer_Tick(object sender, EventArgs e)
        {
            int IvWLL1 = Workspace.This.IVVM.WL1;
            int IvWLR1 = Workspace.This.IVVM.WR1;
            int IvWLR2 = Workspace.This.IVVM.WR2;
            string timeNow = FormatStrDate();
            if (IvWLL1 != 0)
            {
                if (Workspace.This.IVVM.IsLaserL1Selected)
                {
                    LTemperratureList.Add(Convert.ToDouble(Workspace.This.IVVM.SensorTemperatureL1));//激光温度
                    if (LTemperratureList.Count == 5)
                    {
                        if (LTemperratureList.Average() >= 25.2)
                        {
                            Workspace.This.IVVM.IsLaserL1Selected = false;
                            TemperatureShow("L", Workspace.This.IVVM.SensorTemperatureL1, Workspace.This.IVVM.SensorRadTemperaTureL1);
                        }
                        else
                        {
                            LTemperratureList.Clear();
                        }
                    }
                }
            }
            if (IvWLR1 != 0)
            {
                if (Workspace.This.IVVM.IsLaserR1Selected)
                {
                    R1TemperratureList.Add(Convert.ToDouble(Workspace.This.IVVM.SensorTemperatureR1));//激光温度
                    if (R1TemperratureList.Count == 5)
                    {
                        if (R1TemperratureList.Average() >= 25.2)
                        {
                            Workspace.This.IVVM.IsLaserR1Selected = false;
                            TemperatureShow("R1", Workspace.This.IVVM.SensorTemperatureR1, Workspace.This.IVVM.SensorRadTemperaTureR1);
                        }
                        else
                        {
                            R1TemperratureList.Clear();
                        }
                    }
                }
            }
            if (IvWLR2 != 0)
            {
                if (Workspace.This.IVVM.IsLaserR2Selected)
                {
                    R2TemperratureList.Add(Convert.ToDouble(Workspace.This.IVVM.SensorTemperatureR2));//激光温度
                    if (R2TemperratureList.Count == 5)
                    {
                        if (R2TemperratureList.Average() >= 25.2)
                        {
                            Workspace.This.IVVM.IsLaserR2Selected = false;
                            TemperatureShow("R2", Workspace.This.IVVM.SensorTemperatureR2, Workspace.This.IVVM.SensorRadTemperaTureR2);
                        }
                        else
                        {
                            R2TemperratureList.Clear();
                        }
                    }
                }
            }
            StrTemperature[0] = string.Format("\n{0},{1},{2},{3},{4},{5},{6},{7}",
                Workspace.This.IVVM.SensorTemperatureL1, 
                Workspace.This.IVVM.SensorTemperatureR1, 
                Workspace.This.IVVM.SensorTemperatureR2, 
                Workspace.This.IVVM.SensorRadTemperaTureL1, 
                Workspace.This.IVVM.SensorRadTemperaTureR1, 
                Workspace.This.IVVM.SensorRadTemperaTureR2,
                Workspace.This.AmbientTemperatureCH1,
                timeNow);
            WriteReportData(StrTemperature);
            if (!Workspace.This.IVVM.IsLaserL1Selected && !Workspace.This.IVVM.IsLaserR1Selected && !Workspace.This.IVVM.IsLaserR2Selected)
            {
                IsTECTempeSelected = false;
                Reduction();
            }
        }

        public void TECTemperatureTest()
        {
            //step2,弹框自己选择路径保存温度，3秒记录一次所有模块的温度，机箱温度，当前时间
            SaveFileDialog openFileDialog = new SaveFileDialog();
            openFileDialog.Filter = "csv文件|*.csv|所有文件|*.*";
            openFileDialog.RestoreDirectory = true;
            openFileDialog.FilterIndex = 1;
            if (openFileDialog.ShowDialog() == true)
            {
                _filePath = openFileDialog.FileName;
                GenerateReportTemperature(_filePath);
            }
            //step 3,X,Y,Z回到Home，PGA设置成1，Gain设置成50
            Workspace.This.MotorVM.HomeXYZmotor();
            bool _tempCurrent = true;
            while (_tempCurrent)
            {
                Thread.Sleep(500);
                if (Workspace.This.MotorVM.MotionController.CrntState[EthernetCommLib.MotorTypes.X].AtHome &&
              Workspace.This.MotorVM.MotionController.CrntState[EthernetCommLib.MotorTypes.Y].AtHome &&
              Workspace.This.MotorVM.MotionController.CrntState[EthernetCommLib.MotorTypes.Z].AtHome)
                {
                    _tempCurrent = false;
                }
            }
            int IvWLL1 = Workspace.This.IVVM.WL1;
            int IvWLR1 = Workspace.This.IVVM.WR1;
            int IvWLR2 = Workspace.This.IVVM.WR2;
            if (IvWLL1 != 0)
            {
                LaPDPgaType = Workspace.This.IVVM.SelectedMModuleL1;
                Workspace.This.IVVM.SelectedMModuleL1 = Workspace.This.IVVM.PGAOptionsModule[0];

                LAPDGainType = Workspace.This.IVVM.SelectedGainComModuleL1;
                Workspace.This.IVVM.SelectedGainComModuleL1 = Workspace.This.IVVM.GainComModule[0];

                if (IvWLL1 == 375)  //375波长
                {
                    for (int i = 0; i < 2; i++)
                    {
                        Workspace.This.EthernetController.GetAllTECControlTTemperatures(LaserChannels.ChannelC);
                        LTEControlTemperature = EthernetController.TECControlTemperature[LaserChannels.ChannelC];
                        Thread.Sleep(2000);
                    }
                    if (LTEControlTemperature <= 0)
                    {
                        MessageBox.Show("L通道的LTEControlTemperature读取失败！");
                        _IsTECTempeSelected = false;
                        return;
                    }
                    if (Workspace.This.EthernetController.SetTECControlTemperature(LaserChannels.ChannelC, TEControlTemperature375) == false)
                    {
                        MessageBox.Show("LTEControlTemperature Write Failed.");
                        _IsTECTempeSelected = false;
                        return;
                    }
                }

                Workspace.This.IVVM.LaserCPower = 50;
                Thread.Sleep(1000);
                Workspace.This.IVVM.IsLaserL1Selected = true;
            }
            if (IvWLR1 != 0)
            {
                R1aPDPgaType = Workspace.This.IVVM.SelectedMModuleR1;
                Workspace.This.IVVM.SelectedMModuleR1 = Workspace.This.IVVM.PGAOptionsModule[0];

                R1APDGainType = Workspace.This.IVVM.SelectedGainComModuleR1;
                Workspace.This.IVVM.SelectedGainComModuleR1 = Workspace.This.IVVM.GainComModule[0];

                if (IvWLR1 == 375)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        Workspace.This.EthernetController.GetAllTECControlTTemperatures(LaserChannels.ChannelA);
                        R1TEControlTemperature = EthernetController.TECControlTemperature[LaserChannels.ChannelA];
                        Thread.Sleep(2000);
                    }
                    if (R1TEControlTemperature <= 0)
                    {
                        MessageBox.Show("R1通道的LTEControlTemperature读取失败！");
                        _IsTECTempeSelected = false;
                        return;
                    }
                    if (Workspace.This.EthernetController.SetTECControlTemperature(LaserChannels.ChannelA, TEControlTemperature375) == false)
                    {
                        MessageBox.Show("R1TEControlTemperature Write Failed.");
                        _IsTECTempeSelected = false;
                        return;
                    }
                }

                Workspace.This.IVVM.LaserAPower = 50;
                Thread.Sleep(1000);
                Workspace.This.IVVM.IsLaserR1Selected = true;
            }
            if (IvWLR2 != 0)
            {
                R2aPDPgaType = Workspace.This.IVVM.SelectedMModuleR2;
                Workspace.This.IVVM.SelectedMModuleR2 = Workspace.This.IVVM.PGAOptionsModule[0];

                R2APDGainType = Workspace.This.IVVM.SelectedGainComModuleR2;
                Workspace.This.IVVM.SelectedGainComModuleR2 = Workspace.This.IVVM.GainComModule[0];

                if (IvWLR2 == 375)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        Workspace.This.EthernetController.GetAllTECControlTTemperatures(LaserChannels.ChannelB);
                        R2TEControlTemperature = EthernetController.TECControlTemperature[LaserChannels.ChannelB];
                        Thread.Sleep(2000);
                    }
                    if (R2TEControlTemperature <= 0)
                    {
                        MessageBox.Show("R2通道的LTEControlTemperature读取失败！");
                        _IsTECTempeSelected = false;
                        return;
                    }
                    if (Workspace.This.EthernetController.SetTECControlTemperature(LaserChannels.ChannelB, TEControlTemperature375) == false)
                    {
                        MessageBox.Show("R2TEControlTemperature Write Failed.");
                        _IsTECTempeSelected = false;
                        return;
                    }
                }

                Workspace.This.IVVM.LaserBPower = 50;
                Thread.Sleep(1000);
                Workspace.This.IVVM.IsLaserR2Selected = true;
            }
            //Step 4，读取OtherSetting里的Modules Fan Operation(℃)里的参数并改为40°，读取后板风扇的等级，并设置为0
            FanReserveTemperature = Workspace.This.NewParameterVM.FanReserveTemperature;
            Workspace.This.NewParameterVM.FanReserveTemperature = 40;
            Workspace.This.NewParameterVM.ExecuteParametersWriteCommand(null);
            Thread.Sleep(2000);
            //后板风扇等级设置为0，停止转动
            SelectedIncrustation = Workspace.This.NewParameterVM.SelectedIncrustation;
            Workspace.This.NewParameterVM.SelectedIncrustation = "0";
            Thread.Sleep(300000); //等5分钟
            TECRecordTimer.Start();

        }

        public void Reduction()
        {
            TECRecordTimer.Stop();
            int IvWLL1 = Workspace.This.IVVM.WL1;
            int IvWLR1 = Workspace.This.IVVM.WR1;
            int IvWLR2 = Workspace.This.IVVM.WR2;
            if (IvWLL1 != 0)
            {
                Workspace.This.IVVM.SelectedMModuleL1 = LaPDPgaType;

                Workspace.This.IVVM.SelectedGainComModuleL1 = LAPDGainType;

                if (IvWLL1 == 375)  //375波长
                {
                    if (LTEControlTemperature > 0)
                    {
                        if (Workspace.This.EthernetController.SetTECControlTemperature(LaserChannels.ChannelC, LTEControlTemperature) == false)
                        {
                            MessageBox.Show("LTEControlTemperature Write Failed.");
                        }
                    }
                }

                Thread.Sleep(1000);
                Workspace.This.IVVM.IsLaserL1Selected = false;
            }
            if (IvWLR1 != 0)
            {
                Workspace.This.IVVM.SelectedMModuleR1 = R1aPDPgaType;

                Workspace.This.IVVM.SelectedGainComModuleR1 = R1APDGainType;

                if (IvWLR1 == 375)
                {
                    if (R1TEControlTemperature>0)
                    {
                        if (Workspace.This.EthernetController.SetTECControlTemperature(LaserChannels.ChannelA, R1TEControlTemperature) == false)
                        {
                            MessageBox.Show("R1TEControlTemperature Write Failed.");
                        }
                    }
                }

                Workspace.This.IVVM.IsLaserR1Selected = false;
                Thread.Sleep(1000);
            }
            if (IvWLR2 != 0)
            {
                Workspace.This.IVVM.SelectedMModuleR2 = R2aPDPgaType;

                Workspace.This.IVVM.SelectedGainComModuleR2 = R2APDGainType;

                if (IvWLR2 == 375)
                {
                    if (R2TEControlTemperature > 0)
                    {
                        if (Workspace.This.EthernetController.SetTECControlTemperature(LaserChannels.ChannelB, R2TEControlTemperature) == false)
                        {
                            MessageBox.Show("R2TEControlTemperature Write Failed.");
                        }
                    }
                }
                Thread.Sleep(1000);
                Workspace.This.IVVM.IsLaserR2Selected = false;
            }
            Workspace.This.NewParameterVM.SelectedIncrustation = SelectedIncrustation;
            Workspace.This.NewParameterVM.FanReserveTemperature = FanReserveTemperature;
            Workspace.This.NewParameterVM.ExecuteParametersWriteCommand(null);
            Thread.Sleep(2000);

        }
        #endregion

        #region  public property

        public bool IsOldEnabled
        {
            get
            {
                return _IsOldEnabled;
            }
            set
            {
                if (_IsOldEnabled != value)
                {
                    _IsOldEnabled = value;
                    RaisePropertyChanged("IsOldEnabled");
                }
            }
        }
        public bool IsTECTempeSelected
        {
            get
            {
                return _IsTECTempeSelected;
            }
            set
            {
                if (_IsTECTempeSelected != value)
                {
                    if (value)
                    {
                        if (IsProcessing)
                        {
                            MessageBox.Show("老化中，请先关闭老化！");
                            return;
                        }
                        else
                        {
                            if (!Workspace.This.MotorVM.IsNewFirmware)
                            {
                                MessageBox.Show("通讯异常！");
                                return;
                            }
                            if (Workspace.This.IVVM.SensorML1 == IvSensorType.NA && Workspace.This.IVVM.SensorMR1 == IvSensorType.NA && Workspace.This.IVVM.SensorMR2 == IvSensorType.NA)
                            {
                                MessageBox.Show("至少有一个光学模块！");
                                return;
                            }
                            //step 1,先判断机箱温度，>20或<30测试，如果不是这个温度范围，弹框提醒“请升温或者降温测试！机箱温度应该在20-30温度之间测试”如果在范围内，继续下一步。
                            if (Workspace.This.AmbientTemperatureCH1 < 20 || Workspace.This.AmbientTemperatureCH1 > 30)
                            {
                                MessageBox.Show("请升温或者降温测试！机箱温度应该在20-30温度之间测试！");
                                return;
                            }
                            MessageBoxResult boxResult = MessageBoxResult.None;
                            Workspace.This.Owner.Dispatcher.Invoke(new Action(() =>
                            {
                                boxResult = MessageBox.Show("测试中请勿对设备下电，或者先关闭该测试在对设备下电", "测试继续", MessageBoxButton.YesNo);
                            }));
                            if (boxResult == MessageBoxResult.No)
                            {
                                MessageBox.Show("取消了本次测试！");
                                return;
                            }
                            _IsTECTempeSelected = value;
                            RaisePropertyChanged("IsTECTempeSelected");
                            _TECTempeProcess = new Thread(TECTemperatureTest);
                            _TECTempeProcess.IsBackground = true;
                            _TECTempeProcess.Start();
                        }
                        IsOldEnabled = false;
                    }
                    else
                    {
                        Reduction();
                        _TECTempeProcess.Abort();
                        IsOldEnabled = true;
                        _IsTECTempeSelected = value;
                        RaisePropertyChanged("IsTECTempeSelected");
                    }
                }
            }
        }
        public bool IsTECTempeEnabled
        {
            get
            {
                return _IsTECTempeEnabled;
            }
            set
            {
                if (_IsTECTempeEnabled != value)
                {
                    _IsTECTempeEnabled = value;
                    RaisePropertyChanged("IsTECTempeEnabled");
                }
            }
        }
        private void SavePicture(int tabIndex, bool CloseAfterSave = true)
        {
            try
            {
                if (Workspace.This.Files[tabIndex] != null)
                {
                    //                    Thread.Sleep(1000);
                    Workspace.This.Owner.Dispatcher.Invoke(new Action(() =>
                    {
                        //Workspace.This.StartWaitAnimation("Saving...");
                        Workspace.This.SaveFile(Workspace.This.Files[tabIndex], false);

                        if (CloseAfterSave)
                        {
                            Workspace.This.Remove(Workspace.This.Files[tabIndex]);
                        }
                        //Workspace.This.StopWaitAnimation();
                    }));
                }
            }
            catch
            {
                // Rethrow to preserve stack details
                // Satisfies the rule. 
                throw;
            }
            finally
            {
                //Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
                //{
                //    Workspace.This.StopWaitAnimation();
                //});
            }
        }

        private void WaitScanningEnded()
        {
            Workspace.This.ScannerVM.ScanProcessingAgingThread.ThreadHandle.Join();
            if (_IsProcessCanceled)
            {
                return;
            }
            for (int temp = 0; temp < 500; temp++)
            {
                if (Workspace.This.Files.Count > 3)
                {
                    break;
                }
                Thread.Sleep(10);
            }
        }

        public bool IsNextBtnClicked
        {
            get
            {
                return _IsNextBtnClicked;
            }
            set
            {
                if (_IsNextBtnClicked != value)
                {
                    _IsNextBtnClicked = value;
                    RaisePropertyChanged("IsNextBtnClicked");
                }
            }
        }

        public bool IsNextStepBtnEnabled
        {
            get
            {
                return _IsNextStepBtnEnabled;
            }

            set
            {
                if (_IsNextStepBtnEnabled != value)
                {
                    _IsNextStepBtnEnabled = value;
                    RaisePropertyChanged("IsNextStepBtnEnabled");
                }
            }
        }

        public bool IsProcessing
        {
            get
            {
                return _IsProcessing;
            }
            set
            {
                if (_IsProcessing != value)
                {
                    _IsProcessing = value;
                    if (value)
                    {
                        IsTECTempeEnabled = false;
                    }
                    else
                    {
                        IsTECTempeEnabled = true;
                    }
                    RaisePropertyChanged("IsProcessing");
                }
            }
        }

        public string SubProcessName
        {
            get
            {
                return _SubProcessName;
            }
            set
            {
                if (_SubProcessName != value)
                {
                    _SubProcessName = value;
                    RaisePropertyChanged("SubProcessName");
                }
            }
        }

        public int TotalProcessPercent
        {
            get
            {
                return _TotalProcessPercent;
            }
            set
            {
                if (_TotalProcessPercent != value)
                {
                    _TotalProcessPercent = value;
                    RaisePropertyChanged("TotalProcessPercent");
                }
            }
        }

        public string StoreFolder
        {
            get
            {
                return _StoreFolder;
            }
            set
            {
                if (_StoreFolder != value)
                {
                    _StoreFolder = value;
                    RaisePropertyChanged("StoreFolder");
                }
            }
        }

        public int SubProcessPercent
        {
            get
            {
                return _SubProcessPercent;
            }
            set
            {
                if (_SubProcessPercent != value)
                {
                    _SubProcessPercent = value;
                    RaisePropertyChanged("SubProcessPercent");
                }
            }
        }

        public string DeviceSerialNum
        {
            get
            {
                return _DeviceSerialNum;
            }
            set
            {
                if (_DeviceSerialNum != value)
                {
                    _DeviceSerialNum = value;
                    RaisePropertyChanged("DeviceSerialNum");
                }
            }
        }

        public int LoopTimes
        {
            get
            {
                return _LoopTimes;
            }
            set
            {
                if (_LoopTimes != value)
                {
                    _LoopTimes = value;
                    RaisePropertyChanged("LoopTimes");
                }
            }
        }
        private int _ConfigPower;
        public int ConfigPower
        {
            get
            {
                return _ConfigPower;
            }
            set
            {
                if (_ConfigPower != value)
                {
                    _ConfigPower = value;
                    RaisePropertyChanged("ConfigPower");
                }
            }
        }
        #endregion

        private string FormatStrDate()
        {
            System.DateTime currentTime = DateTime.Now;//获取当前系统时间
            int Year = currentTime.Year;
            int month = currentTime.Month;
            int day = currentTime.Day;
            int hour = currentTime.Hour;
            int minute = currentTime.Minute;
            int Sec = currentTime.Second;
            //string newDay = "";
            //if (day.ToString().Length == 1)
            //{
            //    newDay = "0" + day.ToString();
            //}
            //else
            //{
            //    newDay = day.ToString();
            //}
            string timeNow = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}", hour, ":", minute,":", Sec," ", Year, "-", month, "-", day);
            return timeNow;
        }
        private void ReportFile_VisitError(object sender, FileAccessErrorArgs e)
        {
            MessageBox.Show("File access error！\n" + e.Message);
        }
        public void WriteReportData(string[] Temperature)
        {
            //if (ReportFile == null)
            //{
            //    return;
            //}
            //if (ReportFile.Open(System.IO.FileAccess.ReadWrite))
            //{
            //    Report.SetColumnContent(columnWriteCount++, Temperature);
            //    ReportFile.Write(Report.ToString());
            //}
            //ReportFile.Close();
            try
            {
                if (File.Exists(_filePath))
                {
                    // 打开文件并定位到末尾
                    using (StreamWriter writer = File.AppendText(_filePath))
                    {
                        // 写入新数据
                        writer.WriteLine(Temperature[0]);
                    }
                }
                else
                {
                    Console.WriteLine("CSV文件不存在。");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("\n" + e.Message);
            }

        }
        string _filePath = "";
        FileVisit ReportFile = null;
        CSVFile Report = null;
        string[] StrTemperature = new string[1];
        private void GenerateReportTemperature(string _filePath)
        {
            ReportFile = new FileVisit(_filePath);
            ReportFile.VisitError += ReportFile_VisitError; ;
            string LType = "NA";
            string R1Type = "NA";
            string R2Type = "NA";

            if (Workspace.This.IVVM.SensorML1 == IvSensorType.APD)
            {
                LType = "APD";
            }
            else if (Workspace.This.IVVM.SensorML1 == IvSensorType.PMT)
            {
                LType = "PMT";
            }
            else
            {
                LType = "NA";
            }
            if (Workspace.This.IVVM.SensorMR1 == IvSensorType.APD)
            {
                R1Type = "APD";
            }
            else if (Workspace.This.IVVM.SensorMR1 == IvSensorType.PMT)
            {
                R1Type = "PMT";
            }
            else
            {
                R1Type = "NA";
            }
            if (Workspace.This.IVVM.SensorMR2 == IvSensorType.APD)
            {
                R2Type = "APD";
            }
            else if (Workspace.This.IVVM.SensorMR2 == IvSensorType.PMT)
            {
                R2Type = "PMT";
            }
            else
            {
                R2Type = "NA";
            }
            if (ReportFile.Open(System.IO.FileAccess.ReadWrite))
            {
                StringBuilder _header = new StringBuilder("L:" + LType + ":" + Workspace.This.IVVM.WL1 + "    R1: " + R1Type + ":" + Workspace.This.IVVM.WR1 + "    R2: " + R2Type + ":" + Workspace.This.IVVM.WR2 + "\n", 1024);
                List<string> columHeaderList = new List<string>();
                columHeaderList.Add("L-LaserTemperature");
                columHeaderList.Add("R1-LaserTemperature");
                columHeaderList.Add("R2-LaserTemperature");
                columHeaderList.Add("L-RadiatorTemperature");
                columHeaderList.Add("R1-RadiatorTemperature");
                columHeaderList.Add("R2-RadiatorTemperature");
                columHeaderList.Add("CH1-Temperature");
                //if (Workspace.This.ScannerVM.HWversion == "1.1.0.0")
                //{
                //    columHeaderList.Add("CH1-Temperature");
                //    columHeaderList.Add("CH2-Temperature");
                //}
                columHeaderList.Add("Time");
                string[] _columnHeaders = columHeaderList.ToArray();
                Report = new CSVFile(_header.ToString(), _columnHeaders);
                ReportFile.Write(Report.ToString());
            }
            ReportFile.Close();
        }

        private void RemoveAllImages()
        {
            while (Workspace.This.Files.Count > 0)
            {
                Workspace.This.Owner.Dispatcher.Invoke(new Action(() =>
                {
                    Workspace.This.Remove(Workspace.This.Files[0]);
                }));
                Thread.Sleep(10);
            }
        }

    }
}
