using System;
using System.Collections.Generic;
using System.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MvCodeReaderSDKNet;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Drawing.Drawing2D;

namespace MultipleDemo
{
    public partial class MultipleDemo : Form
    {
        MvCodeReader.cbOutputEx2delegate m_cbImageOutput1;
        MvCodeReader.cbOutputEx2delegate m_cbImageOutput2;
        MvCodeReader.cbOutputEx2delegate m_cbImageOutput3;
        MvCodeReader.cbOutputEx2delegate m_cbImageOutput4;
        MvCodeReader.MV_CODEREADER_DEVICE_INFO_LIST m_pstDeviceList;

        private MvCodeReader[] m_pcDevice;
        MvCodeReader.MV_CODEREADER_DEVICE_INFO[] m_pstDeviceInfo;

        // 第一个窗口显示
        Graphics m_pcGraBox1 = null;
        byte[] m_BufForDriver1 = new byte[1024 * 1024 * 20];
        Bitmap m_BitMap1 = null;

        // 第二个窗口显示
        Graphics m_pcGraBox2 = null;
        byte[] m_BufForDriver2 = new byte[1024 * 1024 * 20];
        Bitmap m_BitMap2 = null;

        // 第三个窗口显示
        Graphics m_pcGraBox3 = null;
        byte[] m_BufForDriver3 = new byte[1024 * 1024 * 20];
        Bitmap m_BitMap3 = null;

        // 第四个窗口显示
        Graphics m_pcGraBox4 = null;
        byte[] m_BufForDriver4 = new byte[1024 * 1024 * 20];
        Bitmap m_BitMap4 = null;

        bool m_bGrabbing;
        int m_nCanOpenDeviceNum;        // ch:设备使用数量 | en:Used Device Number
        int m_nDevNum;                  // ch:在线设备数量 | en:Online Device Number

        public MultipleDemo()
        {
            InitializeComponent();
            m_pstDeviceList = new MvCodeReader.MV_CODEREADER_DEVICE_INFO_LIST();
            m_bGrabbing = false;
            m_nCanOpenDeviceNum = 0;
            m_nDevNum = 0;
            DeviceListAcq();
            m_pcDevice = new MvCodeReader[4];
            m_pstDeviceInfo = new MvCodeReader.MV_CODEREADER_DEVICE_INFO[4];
            m_cbImageOutput1 = new MvCodeReader.cbOutputEx2delegate(ImageCallBack);
            pictureBox1.Show();
            m_pcGraBox1 = pictureBox1.CreateGraphics();

            m_cbImageOutput2 = new MvCodeReader.cbOutputEx2delegate(ImageCallBack1);
            pictureBox2.Show();
            m_pcGraBox2 = pictureBox2.CreateGraphics();

            m_cbImageOutput3 = new MvCodeReader.cbOutputEx2delegate(ImageCallBack2);
            pictureBox3.Show();
            m_pcGraBox3 = pictureBox3.CreateGraphics();

            m_cbImageOutput4 = new MvCodeReader.cbOutputEx2delegate(ImageCallBack3);
            pictureBox4.Show();
            m_pcGraBox4 = pictureBox4.CreateGraphics();
        }

        public void ResetMember()
        {
            m_pstDeviceList = new MvCodeReader.MV_CODEREADER_DEVICE_INFO_LIST();
            m_bGrabbing = false;
            m_nCanOpenDeviceNum = 0;
            m_nDevNum = 0;
            DeviceListAcq();

            m_cbImageOutput1 = new MvCodeReader.cbOutputEx2delegate(ImageCallBack);
            m_cbImageOutput2 = new MvCodeReader.cbOutputEx2delegate(ImageCallBack1);
            m_cbImageOutput3 = new MvCodeReader.cbOutputEx2delegate(ImageCallBack2);
            m_cbImageOutput4 = new MvCodeReader.cbOutputEx2delegate(ImageCallBack3);
            m_pstDeviceInfo = new MvCodeReader.MV_CODEREADER_DEVICE_INFO[4];

            pictureBox1.Image = null;
            pictureBox2.Image = null;
            pictureBox3.Image = null;
            pictureBox4.Image = null;
        }

        // ch:枚举设备 | en:Create Device List
        private void DeviceListAcq()
        {
            System.GC.Collect();
            int nRet = MvCodeReader.MV_CODEREADER_EnumDevices_NET(ref m_pstDeviceList, MvCodeReader.MV_CODEREADER_GIGE_DEVICE);
            if (MvCodeReader.MV_CODEREADER_OK != nRet)
            {
                richTextBox.Text += "Enumerate devices fail!\r\n";
                return;
            }

            m_nDevNum = (int)m_pstDeviceList.nDeviceNum;
            tbDevNum.Text = m_nDevNum.ToString("d");
        }

        private void SetCtrlWhenOpen()
        {
            bnOpen.Enabled = false;
            bnClose.Enabled = true;
            bnStartGrab.Enabled = true;
            bnStopGrab.Enabled = false;
            bnContinuesMode.Enabled = false;
            bnContinuesMode.Checked = true;
            bnTriggerMode.Enabled = true;
            cbSoftTrigger.Enabled = false;
            bnTriggerExec.Enabled = false;

            tbExposure.Enabled = true;
            tbGain.Enabled = true;
            tbFrameRate.Enabled = true;
            bnSetParam.Enabled = true;
            tbUseNum.Enabled = false;
        }

        // ch:初始化、打开相机 | en:Initialization and open devices
        private void bnOpen_Click(object sender, EventArgs e)
        {
            bool bOpened =false;
            // ch:判断输入格式是否正确 | en:Determine whether the input format is correct
            try
            {
                int.Parse(tbUseNum.Text);
            }
            catch
            {
                richTextBox.Text += "Please enter correct format!\r\n";
                return;
            }
            // ch:获取使用设备的数量 | en:Get Used Device Number
            int nCameraUsingNum = int.Parse(tbUseNum.Text);
            // ch:参数检测 | en:Parameters inspection
            if (nCameraUsingNum <= 0)
            {
                nCameraUsingNum = 1;
            }
            if (nCameraUsingNum > 4)
            {
                nCameraUsingNum = 4;
            }

            byte[] chUserDefinedName = null;
            for (int i = 0, j = 0; j < m_nDevNum; j++)
            {
                //ch:获取选择的设备信息 | en:Get Selected Device Information
                MvCodeReader.MV_CODEREADER_DEVICE_INFO stDevInfo = 
                    (MvCodeReader.MV_CODEREADER_DEVICE_INFO)Marshal.PtrToStructure(m_pstDeviceList.pDeviceInfo[j], typeof(MvCodeReader.MV_CODEREADER_DEVICE_INFO));

                String StrTemp = "";
                if (stDevInfo.nTLayerType == MvCodeReader.MV_CODEREADER_GIGE_DEVICE)
                {
                    IntPtr buffer = Marshal.UnsafeAddrOfPinnedArrayElement(stDevInfo.SpecialInfo.stGigEInfo, 0);
                    MvCodeReader.MV_CODEREADER_GIGE_DEVICE_INFO stGigeInfo = (MvCodeReader.MV_CODEREADER_GIGE_DEVICE_INFO)Marshal.PtrToStructure(buffer, typeof(MvCodeReader.MV_CODEREADER_GIGE_DEVICE_INFO));
                    if (stGigeInfo.chUserDefinedName != "")
                    {
                        StrTemp = "GEV: " + stGigeInfo.chUserDefinedName + " (" + stGigeInfo.chSerialNumber + ")";
                    }
                    else
                    {
                        StrTemp = "GEV: " + stGigeInfo.chManufacturerName + " " + stGigeInfo.chModelName + " (" + stGigeInfo.chSerialNumber + ")";
                    }
                }

                //ch:打开设备 | en:Open Device
                if (null == m_pcDevice[i])
                {
                    m_pcDevice[i] = new MvCodeReader();
                    if (null == m_pcDevice[i])
                    {
                        return ;
                    }
                }

                int nRet = m_pcDevice[i].MV_CODEREADER_CreateHandle_NET(ref stDevInfo);
                if (MvCodeReader.MV_CODEREADER_OK != nRet)
                {
                    richTextBox.Text += String.Format("Create Handle[{0}] failed! nRet=0x{1}\r\n", StrTemp, nRet.ToString("X"));
                    return;
                }

                nRet = m_pcDevice[i].MV_CODEREADER_OpenDevice_NET();
                if (MvCodeReader.MV_CODEREADER_OK != nRet)
                {
                    richTextBox.Text += String.Format("Open Device[{0}] failed! nRet=0x{1}\r\n", StrTemp, nRet.ToString("X"));
                    continue;
                }
                else
                {
                    richTextBox.Text += String.Format("Open Device[{0}] success!\r\n", StrTemp);
                    
                    m_nCanOpenDeviceNum++;
                    m_pstDeviceInfo[i] = stDevInfo;
                    
                    if (stDevInfo.nTLayerType == MvCodeReader.MV_CODEREADER_GIGE_DEVICE)
                    {
                        int nPacketSize = m_pcDevice[i].MV_CODEREADER_GetOptimalPacketSize_NET();
                        if (nPacketSize > 0)
                        {
                            nRet = m_pcDevice[i].MV_CODEREADER_SetIntValue_NET("GevSCPSPacketSize", nPacketSize);
                            if (nRet != MvCodeReader.MV_CODEREADER_OK)
                            {
                                richTextBox.Text += String.Format("Set Packet Size failed! nRet=0x{0}\r\n", nRet.ToString("X"));
                            }
                        }
                        else
                        {
                            richTextBox.Text += String.Format("Get Packet Size failed! nPacketSize=0x{0}\r\n", nPacketSize.ToString("X"));
                        }
                    }

                    m_pcDevice[i].MV_CODEREADER_SetEnumValue_NET("TriggerMode", (uint)MvCodeReader.MV_CODEREADER_TRIGGER_MODE.MV_CODEREADER_TRIGGER_MODE_OFF);
                    bOpened = true;
                    if (m_nCanOpenDeviceNum == nCameraUsingNum)
                    {
                        break;
                    }
                    i++;
                }
            }

            if (null != m_pcDevice[0])
            {
                m_pcDevice[0].MV_CODEREADER_RegisterImageCallBackEx2_NET(m_cbImageOutput1, (IntPtr)0);
            }

            if (null != m_pcDevice[1])
            {
                m_pcDevice[1].MV_CODEREADER_RegisterImageCallBackEx2_NET(m_cbImageOutput2, (IntPtr)1);
            }

            if (null != m_pcDevice[2])
            {
                m_pcDevice[2].MV_CODEREADER_RegisterImageCallBackEx2_NET(m_cbImageOutput3, (IntPtr)2);
            }

            if (null != m_pcDevice[3])
            {
                m_pcDevice[3].MV_CODEREADER_RegisterImageCallBackEx2_NET(m_cbImageOutput4, (IntPtr)3);
            }

            // ch:只要有一台设备成功打开 | en:As long as there is a stDevInfo successfully opened
            if (bOpened)
            {
                tbUseNum.Text = m_nCanOpenDeviceNum.ToString();
                SetCtrlWhenOpen();
            }
        }

        private void SetCtrlWhenClose()
        {
            bnOpen.Enabled = true;
            bnClose.Enabled = false;
            bnStartGrab.Enabled = false;
            bnStopGrab.Enabled = false;
            bnContinuesMode.Enabled = false;
            bnTriggerMode.Enabled = false;
            cbSoftTrigger.Enabled = false;
            bnTriggerExec.Enabled = false;

            tbExposure.Enabled = false;
            tbGain.Enabled = false;
            tbFrameRate.Enabled = false;
            bnSetParam.Enabled = false;
            tbUseNum.Enabled = true;
            bnContinuesMode.Checked = false;
            bnTriggerMode.Checked = false;
            cbSoftTrigger.Checked = false;
            tbExposure.Clear();
            tbGain.Clear();
            tbFrameRate.Clear();
        }

        // ch:关闭相机 | en:Close Device
        private void bnClose_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < m_nCanOpenDeviceNum; ++i)
            {
                int nRet;

                nRet = m_pcDevice[i].MV_CODEREADER_CloseDevice_NET();
                if (MvCodeReader.MV_CODEREADER_OK != nRet)
                {
                    return;
                }

                nRet = m_pcDevice[i].MV_CODEREADER_DestroyHandle_NET();
                if (MvCodeReader.MV_CODEREADER_OK != nRet)
                {
                    return;
                }
            }

            //控件操作 ch: | en:Control Operation
            SetCtrlWhenClose();
            // ch:取流标志位清零 | en:Zero setting grabbing flag bit
            m_bGrabbing = false;
            // ch:重置成员变量 | en:Reset member variable
            ResetMember();
        }

        // ch:连续采集 | en:
        private void bnContinuesMode_CheckedChanged(object sender, EventArgs e)
        {
            if (bnContinuesMode.Checked)
            {
                for (int i = 0; i < m_nCanOpenDeviceNum; ++i)
                {
                    m_pcDevice[i].MV_CODEREADER_SetEnumValue_NET("TriggerMode", (uint)MvCodeReader.MV_CODEREADER_TRIGGER_MODE.MV_CODEREADER_TRIGGER_MODE_OFF);

                }
                cbSoftTrigger.Enabled = false;
                bnTriggerExec.Enabled = false;
                bnContinuesMode.Enabled = false;
                bnTriggerMode.Enabled = true;
                cbSoftTrigger.Checked = false;
                bnStartGrab.Enabled = true;
            }
        }

        // ch:打开触发模式 | en:Open Trigger Mode
        private void bnTriggerMode_CheckedChanged(object sender, EventArgs e)
        {
            if (bnTriggerMode.Checked)
            {
                for (int i = 0; i < m_nCanOpenDeviceNum; ++i)
                {
                    m_pcDevice[i].MV_CODEREADER_SetEnumValue_NET("TriggerMode", (uint)MvCodeReader.MV_CODEREADER_TRIGGER_MODE.MV_CODEREADER_TRIGGER_MODE_ON);

                    // ch:触发源选择:0 - Line0; | en:Trigger source select:0 - Line0;
                    if (cbSoftTrigger.Checked)
                    {
                        m_pcDevice[i].MV_CODEREADER_SetEnumValue_NET("TriggerSource", (uint)MvCodeReader.MV_CODEREADER_TRIGGER_SOURCE.MV_CODEREADER_TRIGGER_SOURCE_SOFTWARE);
                        if (m_bGrabbing)
                        {
                            bnTriggerExec.Enabled = true;
                        }
                    }
                    else
                    {
                        m_pcDevice[i].MV_CODEREADER_SetEnumValue_NET("TriggerSource", (uint)MvCodeReader.MV_CODEREADER_TRIGGER_SOURCE.MV_CODEREADER_TRIGGER_SOURCE_LINE0);
                    }
                    cbSoftTrigger.Enabled = true;
                    bnContinuesMode.Enabled = true;
                    bnTriggerMode.Enabled = false;
                }
            }
        }

        private void SetCtrlWhenStartGrab()
        {
            bnStartGrab.Enabled = false;
            bnStopGrab.Enabled = true;
            bnClose.Enabled = false;
            cbSoftTrigger.Enabled = false;
            bnContinuesMode.Enabled = false;
            bnTriggerMode.Enabled = false;

            if (bnTriggerMode.Checked && cbSoftTrigger.Checked)
            {
                bnTriggerExec.Enabled = true;
            }
        }

        private void bnStartGrab_Click(object sender, EventArgs e)
        {
            int nRet = MvCodeReader.MV_CODEREADER_OK;

            // ch:开始采集 | en:Start Grabbing
            for (int i = 0; i < m_nCanOpenDeviceNum; i++)
            {
                nRet = m_pcDevice[i].MV_CODEREADER_StartGrabbing_NET();
                if (MvCodeReader.MV_CODEREADER_OK != nRet)
                {
                    richTextBox.Text += String.Format("No.{0} Start Grabbing failed! nRet=0x{1}\r\n", (i + 1).ToString(), nRet.ToString("X"));
                }
            }

            // ch:控件操作 | en:Control Operation
            SetCtrlWhenStartGrab();
            // ch:标志位置位true | en:Set Position Bit true
            m_bGrabbing = true;
        }

        private void cbSoftTrigger_CheckedChanged(object sender, EventArgs e)
        {
            if (cbSoftTrigger.Checked)
            {
                // ch:触发源设为软触发 | en:Set Trigger Source As Software
                for (int i = 0; i < m_nCanOpenDeviceNum; ++i)
                {
                    m_pcDevice[i].MV_CODEREADER_SetEnumValue_NET("TriggerSource", (uint)MvCodeReader.MV_CODEREADER_TRIGGER_SOURCE.MV_CODEREADER_TRIGGER_SOURCE_SOFTWARE);
                }
                if (m_bGrabbing)
                {
                    bnTriggerExec.Enabled = true;
                }

            }
            else
            {
                bnTriggerExec.Enabled = false;
            }
        }

        // ch:触发命令 | en:Trigger Command
        private void bnTriggerExec_Click(object sender, EventArgs e)
        {
            int nRet;

            for (int i = 0; i < m_nCanOpenDeviceNum; ++i)
            {
                nRet = m_pcDevice[i].MV_CODEREADER_SetCommandValue_NET("TriggerSoftware");
                if (MvCodeReader.MV_CODEREADER_OK != nRet)
                {
                    richTextBox.Text += String.Format("No.{0} Set software trigger failed! nRet=0x{1}\r\n", (i + 1).ToString(), nRet.ToString("X"));
                }
            }
        }

        private void SetCtrlWhenStopGrab()
        {
            bnStartGrab.Enabled = true;
            bnStopGrab.Enabled = false;
            bnClose.Enabled = true;

            bnTriggerExec.Enabled = false;
            if (bnTriggerMode.Checked)
            {
                cbSoftTrigger.Enabled = true;
                bnContinuesMode.Enabled = true;
                bnTriggerMode.Enabled = false;
            }
            else
            {
                bnContinuesMode.Enabled = false;
                bnTriggerMode.Enabled = true;

            }
        }

        //停止采集 ch: | en:Stop Grabbing
        private void bnStopGrab_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < m_nCanOpenDeviceNum; ++i)
            {
                m_pcDevice[i].MV_CODEREADER_StopGrabbing_NET();
            }
            //ch:标志位设为false  | en:Set Flag Bit false
            m_bGrabbing = false;

            // ch:控件操作 | en:Control Operation
            SetCtrlWhenStopGrab();
        }

        // ch:设置曝光时间和增益 | en:Set Exposure Time and Gain
        private void bnSetParam_Click(object sender, EventArgs e)
        {
            try
            {
                float.Parse(tbExposure.Text);
                float.Parse(tbGain.Text);
                float.Parse(tbFrameRate.Text);
            }
            catch
            {
                richTextBox.Text += String.Format("Please Enter Correct Type.\r\n");
                return;
            }

            if (float.Parse(tbExposure.Text) < 0 || float.Parse(tbGain.Text) < 0 || float.Parse(tbFrameRate.Text) < 0)
            {
                richTextBox.Text += String.Format("Set ExposureTime or Gain fail,Because ExposureTime or Gain or FrameRate less than zero.\r\n");
                return;
            }

            int nRet;
            for (int i = 0; i < m_nCanOpenDeviceNum; ++i)
            {
                bool bSuccess = true;
                m_pcDevice[i].MV_CODEREADER_SetEnumValue_NET("ExposureAuto", 0);

                nRet = m_pcDevice[i].MV_CODEREADER_SetFloatValue_NET("ExposureTime", float.Parse(tbExposure.Text));
                if (nRet != MvCodeReader.MV_CODEREADER_OK)
                {
                    richTextBox.Text += String.Format("No.{0} Set Exposure Time Failed! nRet=0x{1}\r\n", (i + 1).ToString(), nRet.ToString("X"));
                    bSuccess = false;
                }

                m_pcDevice[i].MV_CODEREADER_SetEnumValue_NET("GainAuto", 0);
                nRet = m_pcDevice[i].MV_CODEREADER_SetFloatValue_NET("Gain", float.Parse(tbGain.Text));
                if (nRet != MvCodeReader.MV_CODEREADER_OK)
                {
                    richTextBox.Text += String.Format("No.{0} Set Gain Failed! nRet=0x{1}\r\n", (i + 1).ToString(), nRet.ToString("X"));
                    bSuccess = false;
                }

                nRet = m_pcDevice[i].MV_CODEREADER_SetFloatValue_NET("AcquisitionFrameRate", float.Parse(tbFrameRate.Text));
                if (nRet != MvCodeReader.MV_CODEREADER_OK)
                {
                    richTextBox.Text += String.Format("No.{0} Set FrameRate Failed! nRet=0x{1}\r\n", (i + 1).ToString(), nRet.ToString("X"));
                    bSuccess = false;
                }

                if (bSuccess)
                {
                    richTextBox.Text += String.Format("No.{0} Set Parameters Succeed!\r\n", (i + 1).ToString());
                }
            }
        }

        private void MultipleDemo_FormClosing(object sender, FormClosingEventArgs e)
        {
            bnClose_Click(sender, e);
        }

        private void richTextBox_DoubleClick(object sender, EventArgs e)
        {
            richTextBox.Clear();
        }

        private void richTextBox_TextChanged(object sender, EventArgs e)
        {
            richTextBox.SelectionStart = richTextBox.Text.Length;
            richTextBox.ScrollToCaret();
        }

        // ch:取流回调函数 | en:Aquisition Callback Function
        private void ImageCallBack(IntPtr pData, IntPtr pstFrameInfoEx2, IntPtr pUser)
        {
            if (null == pstFrameInfoEx2 || null == pData)
            {
                return;
            }

            MvCodeReader.MV_CODEREADER_IMAGE_OUT_INFO_EX2 stFrameInfoEx2 = new MvCodeReader.MV_CODEREADER_IMAGE_OUT_INFO_EX2();
            stFrameInfoEx2 = (MvCodeReader.MV_CODEREADER_IMAGE_OUT_INFO_EX2)Marshal.PtrToStructure(pstFrameInfoEx2, typeof(MvCodeReader.MV_CODEREADER_IMAGE_OUT_INFO_EX2));

            if (0 >= stFrameInfoEx2.nFrameLen)
            {
                return;
            }

            Marshal.Copy(pData, m_BufForDriver1, 0, (int)stFrameInfoEx2.nFrameLen);
            if (stFrameInfoEx2.enPixelType == MvCodeReader.MvCodeReaderGvspPixelType.PixelType_CodeReader_Gvsp_Mono8)
            {
                IntPtr pImage = Marshal.UnsafeAddrOfPinnedArrayElement(m_BufForDriver1, 0);
                m_BitMap1 = new Bitmap(stFrameInfoEx2.nWidth, stFrameInfoEx2.nHeight, stFrameInfoEx2.nWidth, PixelFormat.Format8bppIndexed, pImage);
                ColorPalette cp = m_BitMap1.Palette;
                for (int i = 0; i < 256; i++)
                {
                    cp.Entries[i] = Color.FromArgb(i, i, i);
                }
                m_BitMap1.Palette = cp;

                pictureBox1.Image = (Image)m_BitMap1;
            }
            else if (stFrameInfoEx2.enPixelType == MvCodeReader.MvCodeReaderGvspPixelType.PixelType_CodeReader_Gvsp_Jpeg)
            {
                GC.Collect();
                MemoryStream ms = new MemoryStream();
                ms.Write(m_BufForDriver1, 0, (int)stFrameInfoEx2.nFrameLen);

                pictureBox1.Image = Image.FromStream(ms);
            }
        }

        private void ImageCallBack1(IntPtr pData, IntPtr pstFrameInfoEx2, IntPtr pUser)
        {
            if (null == pstFrameInfoEx2 || null == pData)
            {
                return;
            }

            MvCodeReader.MV_CODEREADER_IMAGE_OUT_INFO_EX2 stFrameInfoEx2 = new MvCodeReader.MV_CODEREADER_IMAGE_OUT_INFO_EX2();
            stFrameInfoEx2 = (MvCodeReader.MV_CODEREADER_IMAGE_OUT_INFO_EX2)Marshal.PtrToStructure(pstFrameInfoEx2, typeof(MvCodeReader.MV_CODEREADER_IMAGE_OUT_INFO_EX2));

            if (0 >= stFrameInfoEx2.nFrameLen)
            {
                return;
            }

            // 绘制图像
            Marshal.Copy(pData, m_BufForDriver2, 0, (int)stFrameInfoEx2.nFrameLen);
            if (stFrameInfoEx2.enPixelType == MvCodeReader.MvCodeReaderGvspPixelType.PixelType_CodeReader_Gvsp_Mono8)
            {
                IntPtr pImage = Marshal.UnsafeAddrOfPinnedArrayElement(m_BufForDriver2, 0);
                m_BitMap2 = new Bitmap(stFrameInfoEx2.nWidth, stFrameInfoEx2.nHeight, stFrameInfoEx2.nWidth, PixelFormat.Format8bppIndexed, pImage);
                ColorPalette cp = m_BitMap2.Palette;
                for (int i = 0; i < 256; i++)
                {
                    cp.Entries[i] = Color.FromArgb(i, i, i);
                }
                m_BitMap2.Palette = cp;

                pictureBox2.Image = (Image)m_BitMap2;
            }
            else if (stFrameInfoEx2.enPixelType == MvCodeReader.MvCodeReaderGvspPixelType.PixelType_CodeReader_Gvsp_Jpeg)
            {
                GC.Collect();
                MemoryStream ms = new MemoryStream();
                ms.Write(m_BufForDriver2, 0, (int)stFrameInfoEx2.nFrameLen);

                pictureBox2.Image = Image.FromStream(ms);
            }
        }

        private void ImageCallBack2(IntPtr pData, IntPtr pstFrameInfoEx2, IntPtr pUser)
        {
            if (null == pstFrameInfoEx2 || null == pData)
            {
                return;
            }

            MvCodeReader.MV_CODEREADER_IMAGE_OUT_INFO_EX2 stFrameInfoEx2 = new MvCodeReader.MV_CODEREADER_IMAGE_OUT_INFO_EX2();
            stFrameInfoEx2 = (MvCodeReader.MV_CODEREADER_IMAGE_OUT_INFO_EX2)Marshal.PtrToStructure(pstFrameInfoEx2, typeof(MvCodeReader.MV_CODEREADER_IMAGE_OUT_INFO_EX2));

            if (0 >= stFrameInfoEx2.nFrameLen)
            {
                return;
            }

            // 绘制图像
            Marshal.Copy(pData, m_BufForDriver3, 0, (int)stFrameInfoEx2.nFrameLen);
            if (stFrameInfoEx2.enPixelType == MvCodeReader.MvCodeReaderGvspPixelType.PixelType_CodeReader_Gvsp_Mono8)
            {
                IntPtr pImage = Marshal.UnsafeAddrOfPinnedArrayElement(m_BufForDriver3, 0);
                m_BitMap3 = new Bitmap(stFrameInfoEx2.nWidth, stFrameInfoEx2.nHeight, stFrameInfoEx2.nWidth, PixelFormat.Format8bppIndexed, pImage);
                ColorPalette cp = m_BitMap3.Palette;
                for (int i = 0; i < 256; i++)
                {
                    cp.Entries[i] = Color.FromArgb(i, i, i);
                }
                m_BitMap3.Palette = cp;

                pictureBox3.Image = (Image)m_BitMap3;
            }
            else if (stFrameInfoEx2.enPixelType == MvCodeReader.MvCodeReaderGvspPixelType.PixelType_CodeReader_Gvsp_Jpeg)
            {
                GC.Collect();
                MemoryStream ms = new MemoryStream();
                ms.Write(m_BufForDriver3, 0, (int)stFrameInfoEx2.nFrameLen);

                pictureBox3.Image = Image.FromStream(ms);
            }
        }

        private void ImageCallBack3(IntPtr pData, IntPtr pstFrameInfoEx2, IntPtr pUser)
        {
            if (null == pstFrameInfoEx2 || null == pData)
            {
                return;
            }

            MvCodeReader.MV_CODEREADER_IMAGE_OUT_INFO_EX2 stFrameInfoEx2 = new MvCodeReader.MV_CODEREADER_IMAGE_OUT_INFO_EX2();
            stFrameInfoEx2 = (MvCodeReader.MV_CODEREADER_IMAGE_OUT_INFO_EX2)Marshal.PtrToStructure(pstFrameInfoEx2, typeof(MvCodeReader.MV_CODEREADER_IMAGE_OUT_INFO_EX2));

            if (0 >= stFrameInfoEx2.nFrameLen)
            {
                return;
            }

            // 绘制图像
            Marshal.Copy(pData, m_BufForDriver4, 0, (int)stFrameInfoEx2.nFrameLen);
            if (stFrameInfoEx2.enPixelType == MvCodeReader.MvCodeReaderGvspPixelType.PixelType_CodeReader_Gvsp_Mono8)
            {
                IntPtr pImage = Marshal.UnsafeAddrOfPinnedArrayElement(m_BufForDriver4, 0);
                m_BitMap4 = new Bitmap(stFrameInfoEx2.nWidth, stFrameInfoEx2.nHeight, stFrameInfoEx2.nWidth, PixelFormat.Format8bppIndexed, pImage);
                ColorPalette cp = m_BitMap3.Palette;
                for (int i = 0; i < 256; i++)
                {
                    cp.Entries[i] = Color.FromArgb(i, i, i);
                }
                m_BitMap4.Palette = cp;

                pictureBox4.Image = (Image)m_BitMap4;
            }
            else if (stFrameInfoEx2.enPixelType == MvCodeReader.MvCodeReaderGvspPixelType.PixelType_CodeReader_Gvsp_Jpeg)
            {
                GC.Collect();
                MemoryStream ms = new MemoryStream();
                ms.Write(m_BufForDriver4, 0, (int)stFrameInfoEx2.nFrameLen);

                pictureBox4.Image = Image.FromStream(ms);
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }
    }
}
