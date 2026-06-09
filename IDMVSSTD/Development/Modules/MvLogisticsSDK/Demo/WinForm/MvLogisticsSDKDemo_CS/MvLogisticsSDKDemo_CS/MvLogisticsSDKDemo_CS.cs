using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MvLogisticsSDKNet; 
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using System.Drawing.Imaging;

namespace MvLogisticsSDKDemo_CS
{
    public partial class MvLogisticsSDKDemo_CS : Form
    {
        MvLogistics MyLogistics = new MvLogistics();
        string filepath = null;
        MvLogistics.MVLGS_CODE_LISTEx stOutput = new MvLogistics.MVLGS_CODE_LISTEx();
        MvLogistics.MVLGS_PACKAGE_INFOEx pstPkgInfo = new MvLogistics.MVLGS_PACKAGE_INFOEx();
        MvLogistics.cbOutputdelegate ResultCallback;        
        MvLogistics.cbExceptiondelegate ExcepCallbcak;
        MvLogistics.cbTriggerOutputdelegate TriggerCallback;
        private delegate void showresult(IntPtr ptr);
        
        private showresult ShowResult;
        byte[] ImageBuffer = null;
        byte[] JpgImageBuffer = null;

        // 显示
        Bitmap bmp = null;
        Graphics gra = null;
        Pen pen = new Pen(Color.Blue, 3);                   // 画笔颜色
        Point[] stPointList = new Point[4];                 // 条码位置的4个点坐标

        // 判断字符编码
        public static bool IsTextUTF8(byte[] inputStream)
        {
            int encodingBytesCount = 0;
            bool allTextsAreASCIIChars = true;

            for (int i = 0; i < inputStream.Length; i++)
            {
                byte current = inputStream[i];

                if ((current & 0x80) == 0x80)
                {
                    allTextsAreASCIIChars = false;
                }
                // First byte
                if (encodingBytesCount == 0)
                {
                    if ((current & 0x80) == 0)
                    {
                        // ASCII chars, from 0x00-0x7F
                        continue;
                    }

                    if ((current & 0xC0) == 0xC0)
                    {
                        encodingBytesCount = 1;
                        current <<= 2;

                        // More than two bytes used to encoding a unicode char.
                        // Calculate the real length.
                        while ((current & 0x80) == 0x80)
                        {
                            current <<= 1;
                            encodingBytesCount++;
                        }
                    }
                    else
                    {
                        // Invalid bits structure for UTF8 encoding rule.
                        return false;
                    }
                }
                else
                {
                    // Following bytes, must start with 10.
                    if ((current & 0xC0) == 0x80)
                    {
                        encodingBytesCount--;
                    }
                    else
                    {
                        // Invalid bits structure for UTF8 encoding rule.
                        return false;
                    }
                }
            }

            if (encodingBytesCount != 0)
            {
                // Invalid bits structure for UTF8 encoding rule.
                // Wrong following bytes count.
                return false;
            }

            // Although UTF8 supports encoding for ASCII chars, we regard as a input stream, whose contents are all ASCII as default encoding.
            return !allTextsAreASCIIChars;
        }

        public MvLogisticsSDKDemo_CS()
        {
            InitializeComponent();
            ShowResult = ShowList;
            Control.CheckForIllegalCrossThreadCalls = false;
            pictureBoxDisplay.Show();
            gra = pictureBoxDisplay.CreateGraphics();

            ButtonStart.Enabled = false;
            ButtonStop.Enabled = false;
            ButtonDeinit.Enabled = false;

            ConfigPath.Text = "MvLogisticsSDK.xml";
        }

        private void ShowList(IntPtr ptr)
        {
            pstPkgInfo = (MvLogistics.MVLGS_PACKAGE_INFOEx)Marshal.PtrToStructure(ptr, typeof(MvLogistics.MVLGS_PACKAGE_INFOEx));

            for (int i = 0; i < pstPkgInfo.nCodeListNum; i++)
            {
                BarcodeText.Text = "";
                WeightText.Text = "";
                VolumeText.Text = "";

                if (true == pstPkgInfo.bCodeEnable)
                {
                    // 渲染图像
                    Display(ref pstPkgInfo.stCodeList[i]);
                    stOutput = pstPkgInfo.stCodeList[i];
                    pictureBoxDisplay.Invalidate();

                    // 显示当前 条码结果
                    if (0 == pstPkgInfo.stCodeList[i].nCodeNum)
                    {
                        BarcodeText.Text += "NoRead";
                    }
                    else
                    {
                        for (int j = 0; j < pstPkgInfo.stCodeList[i].nCodeNum; j++)
                        {                      
                            bool bIsValidUTF8 = IsTextUTF8(pstPkgInfo.stCodeList[i].stCodeInfo[j].strCode);
                            if (bIsValidUTF8)
                            {
                                BarcodeText.Text += System.Text.Encoding.GetEncoding("UTF-8").GetString(pstPkgInfo.stCodeList[i].stCodeInfo[j].strCode, 
                                    0, pstPkgInfo.stCodeList[i].stCodeInfo[j].nLen);
                            }
                            else
                            {
                                BarcodeText.Text += System.Text.Encoding.GetEncoding("GB2312").GetString(pstPkgInfo.stCodeList[i].stCodeInfo[j].strCode, 
                                    0, pstPkgInfo.stCodeList[i].stCodeInfo[j].nLen);
                            }
                            if (j < pstPkgInfo.stCodeList[i].nCodeNum - 1)
                            {
                                BarcodeText.Text += ";";
                            }
                        }
                    }
                }

                // 显示当前: 体积结果
                if (true == pstPkgInfo.bVolumeEnable)
                {
                    VolumeText.Text += "长度:";
                    VolumeText.Text += pstPkgInfo.stVolumeInfo.fLength.ToString();
                    VolumeText.Text += "宽度:";
                    VolumeText.Text += pstPkgInfo.stVolumeInfo.fWidth.ToString();
                    VolumeText.Text += "高度:";
                    VolumeText.Text += pstPkgInfo.stVolumeInfo.fHeight.ToString();
                    VolumeText.Text += "计泡体积:";
                    VolumeText.Text += pstPkgInfo.stVolumeInfo.fVolume.ToString();
                }

                // 显示当前: 重量结果
                if (true == pstPkgInfo.bWeightEnable)
                {
                    WeightText.Text += pstPkgInfo.fWeight.ToString();
                    WeightText.Text += "KG";
                }

                // 添加整体的结果到列表中
                // 触发号 条码 体积 重量
                int index = this.DataHistory.Rows.Add();
                DataHistory.Rows[index].Cells[0].Value = pstPkgInfo.stCodeList[i].stImage.nTriggerIndex.ToString();
                DataHistory.Rows[index].Cells[1].Value = BarcodeText.Text.ToString();
                DataHistory.Rows[index].Cells[2].Value = VolumeText.Text.ToString();
                DataHistory.Rows[index].Cells[3].Value = WeightText.Text.ToString();

                DataHistory.CurrentCell = DataHistory.Rows[this.DataHistory.Rows.Count - 1].Cells[0];

                if (DataHistory.Rows.Count > 100)
                {
                    DataHistory.Rows.Clear();
                }
            }
        }

        //关闭窗口
        private void MvLogisticsSDKDemo_CS_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (null != MyLogistics)
            {
                MyLogistics.MV_LGS_DestroyHandle_NET();
            }
        }

        private void pictureBoxDisplay_Paint(object sender, PaintEventArgs e)
        {
            // 绘制结果
            for (int i = 0; i < stOutput.nCodeNum; ++i)
            {
                // 绘制矩形框
                for (int j = 0; j < 4; ++j)
                {
                    stPointList[j].X = (int)(stOutput.stCodeInfo[i].stCornerPt[j].nX * (float)(pictureBoxDisplay.Size.Width) / stOutput.stImage.nWidth);
                    stPointList[j].Y = (int)(stOutput.stCodeInfo[i].stCornerPt[j].nY * (float)(pictureBoxDisplay.Size.Height) / stOutput.stImage.nHeight);
                }
                e.Graphics.DrawPolygon(pen, stPointList);
            }
        }

        private void Display(ref MvLogistics.MVLGS_CODE_LISTEx stCodeList)
        {
            if (0 >= stCodeList.stImage.nImageLen)
            {
                return;
            }

            if (stCodeList.stImage.enImageType == MvLogistics.MVLGS_IMAGE_TYPE.MVLGS_IMAGE_JPEG)
            {
                try
                {
                    JpgImageBuffer = new byte[stCodeList.stImage.nImageLen];
                    Marshal.Copy(stCodeList.stImage.pImageBuf, JpgImageBuffer, 0, (int)stCodeList.stImage.nImageLen);
                    MemoryStream ms = new MemoryStream();
                    ms.Write(JpgImageBuffer, 0, (int)stCodeList.stImage.nImageLen);

                    pictureBoxDisplay.Image = Image.FromStream(ms);
                }
                catch
                {
                    return;
                }
            }
            else
            {
                ImageBuffer = new byte[stCodeList.stImage.nImageLen];
                GCHandle handle = GCHandle.Alloc(ImageBuffer, GCHandleType.Pinned);
                Marshal.Copy(stCodeList.stImage.pImageBuf, ImageBuffer, 0, (int)stCodeList.stImage.nImageLen);
                IntPtr pImage = handle.AddrOfPinnedObject();

                if (stCodeList.stImage.enImageType == MvLogistics.MVLGS_IMAGE_TYPE.MVLGS_IMAGE_MONO8)
                {
                    bmp = new Bitmap(stCodeList.stImage.nWidth, stCodeList.stImage.nHeight, stCodeList.stImage.nWidth, PixelFormat.Format8bppIndexed, pImage);
                    ColorPalette cp = bmp.Palette;
                    for (int i = 0; i < 256; i++)
                    {
                        cp.Entries[i] = Color.FromArgb(i, i, i);
                    }
                    bmp.Palette = cp;
                }
                else if (stCodeList.stImage.enImageType == MvLogistics.MVLGS_IMAGE_TYPE.MVLGS_IMAGE_BGR24)
                {
                    bmp = new Bitmap(stCodeList.stImage.nWidth, stCodeList.stImage.nHeight, stCodeList.stImage.nWidth * 3, PixelFormat.Format24bppRgb, pImage);
                }

                pictureBoxDisplay.Image = (Image)bmp;

                if (handle.IsAllocated)
                {
                    handle.Free();
                }
            }
        }
        public void ExceptionCallbackFunc(ref MvLogistics.MVLGS_EXCEPTION_INFO pstEcptInfo, IntPtr pUser)
        {
            // 断线通知
            if (pstEcptInfo.nExceptionID == MvLogistics.MV_LGS_EXCEPTION_DEV_DISCONNECT)
            {
                string csMessage;
                csMessage = "dev disconnect! CamSerialNum:" + pstEcptInfo.strCamSerialNum.ToString();
                MessageBox.Show(csMessage);
            }

            // 异常信息 重连成功
            if (pstEcptInfo.nExceptionID == MvLogistics.MV_LGS_EXCEPTION_RECONNECT_DEV_SUCCESS)
            {
                string csMessage;
                csMessage = "reconncet dev success! CamSerialNum:" + pstEcptInfo.strCamSerialNum.ToString();
                MessageBox.Show(csMessage);
            }
        }
        public void ResultCallbackFunc(IntPtr pstOutput, IntPtr pUser)
        {
            if (null == pstOutput || null == pUser)
            {
                return;
            }

            try
            {
                if (null != pstOutput)
                {
                    this.Invoke(ShowResult, new object[] { pstOutput });
                }
            }
            catch
            {
                return;
            }
        }

        public void TriggerCallbackFunc(ref MvLogistics.MVLGS_TRIGGER_INFO pstTriggerInfo, IntPtr pUser)
        {
            // 得到触发号和触发标记位 根据需求处理
            string csMessage;
            csMessage = "TriggerIndex,TriggerFlag:" + pstTriggerInfo.nTriggerIndex + "," + pstTriggerInfo.nTriggerFlag;
            MessageBox.Show(csMessage);
        }
        //开始工作
        private void ButtonStart_Click(object sender, EventArgs e)
        {
            if (null == MyLogistics)
            {
                string csMessage;
                csMessage = "Init failed: 0x" + String.Format("{0:X}", MvLogistics.MV_LGS_E_PRECONDITION);
                MessageBox.Show(csMessage);
                return;
            }

            Int32 nRet = MvLogistics.MV_LGS_OK;
            nRet = MyLogistics.MV_LGS_Start_NET();
            if (MvLogistics.MV_LGS_OK != nRet)
            {
                string csMessage;
                csMessage = "MV_LGS_Start_NET failed: 0x" + String.Format("{0:X}", nRet);
                MessageBox.Show(csMessage);
                return;
            }

            ButtonStart.Enabled = false;
            ButtonStop.Enabled = true;  // 停止取流按钮 正常显示
        }

        //停止工作
        private void ButtonStop_Click(object sender, EventArgs e)
        {
            if (null == MyLogistics)
            {
                string csMessage;
                csMessage = "Init failed: 0x" + String.Format("{0:X}", MvLogistics.MV_LGS_E_PRECONDITION);
                MessageBox.Show(csMessage);
                return;
            }

            int nRet = MyLogistics.MV_LGS_Stop_NET();
            if (MvLogistics.MV_LGS_OK != nRet)
            {
                string csMessage;
                csMessage = "stop process failed: 0x" + String.Format("{0:X}", nRet);
                MessageBox.Show(csMessage);
            }
            else
            {
                string csMessage;
                csMessage = "stop process success";
                MessageBox.Show(csMessage);
            }

            ButtonStart.Enabled = true;
            ButtonStop.Enabled = false;
         }

        //选择配置文件
        private void ButtonConfigLoad_Click(object sender, EventArgs e)
        {
            if (true == ButtonDeinit.Enabled)
            {
                string csMessage;
                csMessage = "请先销毁资源，在重新加载文件 ";
                MessageBox.Show(csMessage);
                return;
            }

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;//该值确定是否可以选择多个文件
            dialog.Title = "请选择文件夹";
            dialog.Filter = "文件(*.xml)|*.XML";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                filepath = dialog.FileName;
                ConfigPath.Text = filepath;
            }

            if (null == filepath)
            {
                return;
            }
        }

        // 初始化资源 
        private void ButtonInit_Click(object sender, EventArgs e)
        {
            int nRet = MvLogistics.MV_LGS_OK;

            int nVersion = MvLogistics.MV_LGS_GetVersion_NET();
            string strVersion = String.Format("{0}.{1}.{2}.{3}",
                (nVersion & 0xff000000) >> 24,
                (nVersion & 0x00ff0000) >> 16,
                (nVersion & 0x0000ff00) >> 8,
                (nVersion & 0x000000ff));

            nRet = MyLogistics.MV_LGS_CreateHandle_NET();
            if (MvLogistics.MV_LGS_OK != nRet)
            {
                string csMessage;
                csMessage = "MV_LGS_CreateHandle_NET failed: 0x" + String.Format("{0:X}", nRet);
                MessageBox.Show(csMessage);
                return;
            }

            string strCfgPath = "";
            if ("" == ConfigPath.Text)
            {
                strCfgPath = "MvLogisticsSDK.xml";
            }
            else
            {
                strCfgPath = ConfigPath.Text;
            }

            nRet = MyLogistics.MV_LGS_LoadDevCfg_NET(strCfgPath);
            if (MvLogistics.MV_LGS_OK != nRet)
            {
                string csMessage;
                csMessage = "MV_LGS_LoadDevCfg_NET failed: 0x" + String.Format("{0:X}", nRet);
                MessageBox.Show(csMessage);
                return;
            }

            // 注册异常回调函数
            ExcepCallbcak = new MvLogistics.cbExceptiondelegate(ExceptionCallbackFunc);
            nRet = MyLogistics.MV_LGS_RegisterExceptionCB_NET(ExcepCallbcak, IntPtr.Zero);
            if (MvLogistics.MV_LGS_OK != nRet)
            {
                string csMessage;
                csMessage = "MV_LGS_RegisterExceptionCB_NET failed: 0x" + String.Format("{0:X}", nRet);
                MessageBox.Show(csMessage);
                return;
            }
            // ch:注册回调函数 | en:Register image callback
            ResultCallback = new MvLogistics.cbOutputdelegate(ResultCallbackFunc);
            nRet = MyLogistics.MV_LGS_RegisterPackageCB_NET(ResultCallback, IntPtr.Zero);
            if (MvLogistics.MV_LGS_OK != nRet)
            {
                string csMessage;
                csMessage = "MVID_CR_CAM_RegisterImageCallBack failed: 0x" + String.Format("{0:X}", nRet);
                MessageBox.Show(csMessage);
                return;
            }

            // 注册触发回调函数
            TriggerCallback = new MvLogistics.cbTriggerOutputdelegate(TriggerCallbackFunc);
            nRet = MyLogistics.MV_LGS_RegisterTriggerInfoCB_NET(TriggerCallback, IntPtr.Zero);
            if (MvLogistics.MV_LGS_OK != nRet)
            {
                string csMessage;
                csMessage = "MV_LGS_RegisterTriggerInfoCB_NET failed: 0x" + String.Format("{0:X}", nRet);
                MessageBox.Show(csMessage);
                return;
            }
         
            ButtonInit.Enabled = false;
            ButtonStart.Enabled = true;
            ButtonDeinit.Enabled = true;  // 销毁句柄显示
        }
		
        // 销毁资源
        private void ButtonDeinit_Click(object sender, EventArgs e)
        {
            int nRet = MvLogistics.MV_LGS_OK;

            nRet = MyLogistics.MV_LGS_Stop_NET();
            if (MvLogistics.MV_LGS_OK != nRet)
            {
                // 不返回错误
            }
            else
            {
                string csMessage;
                csMessage = "stop success";
                MessageBox.Show(csMessage);
            }

            if (MvLogistics.MV_LGS_OK != MyLogistics.MV_LGS_DestroyHandle_NET())
            {
                string csMessage;
                csMessage = "Deinit failed: 0x" + String.Format("{0:X}", nRet);
                MessageBox.Show(csMessage);
            }
            else
            {
                string csMessage;
                csMessage = "Deinit success";
                MessageBox.Show(csMessage);
            }

            ButtonInit.Enabled = true;
            ButtonDeinit.Enabled = false;  // 销毁句柄灰色显示
            ButtonStart.Enabled = false;
            ButtonStop.Enabled = false;
        }
    }
}
