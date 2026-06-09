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

namespace ReconnectDemo
{
    public partial class ReconnectDemo : Form
    {
        MvCodeReader.MV_CODEREADER_DEVICE_INFO_LIST m_stDeviceList = new MvCodeReader.MV_CODEREADER_DEVICE_INFO_LIST();
        private MvCodeReader m_cMyDevice = new MvCodeReader();
        bool m_bGrabbing = false;
        Thread m_hReceiveThread = null;
        MvCodeReader.MV_CODEREADER_IMAGE_OUT_INFO_EX2 m_stFrameInfo = new MvCodeReader.MV_CODEREADER_IMAGE_OUT_INFO_EX2();
        MvCodeReader.cbExceptiondelegate pCallBackFunc;

        // ch:用于从驱动获取图像的缓存 | en:Buffer for getting image from driver
        byte[] m_BufForDriver = new byte[1024 * 1024 * 20];

        // 显示
        Bitmap bmp = null;
        Graphics gra = null;
        Pen pen = new Pen(Color.Blue, 3);                   // 画笔颜色
        Point[] stPointList = new Point[4];                 // 条码位置的4个点坐标
        GraphicsPath WayShapePath = new GraphicsPath();     // 图形路径，内部变量 
        GraphicsPath OcrShapePath = new GraphicsPath();     // 图形路径，内部变量
        Matrix stRotateWay = new Matrix();
        Matrix stRotateM = new Matrix();
        Pen penOcr = new Pen(Color.Yellow, 3);
        Pen penWay = new Pen(Color.Red, 3);
        public ReconnectDemo()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            pictureBox1.Show();
            gra = pictureBox1.CreateGraphics();
            pCallBackFunc = new MvCodeReader.cbExceptiondelegate(cbExceptiondelegate);
        }

        // ch:显示错误信息 | en:Show error message
        private void ShowErrorMsg(string csMessage, int nErrorNum)
        {
            string errorMsg;
            if (nErrorNum == 0)
            {
                errorMsg = csMessage;
            }
            else
            {
                errorMsg = csMessage + ": Error =" + String.Format("{0:X}", nErrorNum);
            }

            switch (nErrorNum)
            {
                case MvCodeReader.MV_CODEREADER_E_HANDLE: errorMsg += " Error or invalid handle "; break;
                case MvCodeReader.MV_CODEREADER_E_SUPPORT: errorMsg += " Not supported function "; break;
                case MvCodeReader.MV_CODEREADER_E_BUFOVER: errorMsg += " Cache is full "; break;
                case MvCodeReader.MV_CODEREADER_E_CALLORDER: errorMsg += " Function calling order error "; break;
                case MvCodeReader.MV_CODEREADER_E_PARAMETER: errorMsg += " Incorrect parameter "; break;
                case MvCodeReader.MV_CODEREADER_E_RESOURCE: errorMsg += " Applying resource failed "; break;
                case MvCodeReader.MV_CODEREADER_E_NODATA: errorMsg += " No data "; break;
                case MvCodeReader.MV_CODEREADER_E_PRECONDITION: errorMsg += " Precondition error, or running environment changed "; break;
                case MvCodeReader.MV_CODEREADER_E_VERSION: errorMsg += " Version mismatches "; break;
                case MvCodeReader.MV_CODEREADER_E_NOENOUGH_BUF: errorMsg += " Insufficient memory "; break;
                case MvCodeReader.MV_CODEREADER_E_UNKNOW: errorMsg += " Unknown error "; break;
                case MvCodeReader.MV_CODEREADER_E_GC_GENERIC: errorMsg += " General error "; break;
                case MvCodeReader.MV_CODEREADER_E_GC_ACCESS: errorMsg += " Node accessing condition error "; break;
                case MvCodeReader.MV_CODEREADER_E_ACCESS_DENIED: errorMsg += " No permission "; break;
                case MvCodeReader.MV_CODEREADER_E_BUSY: errorMsg += " Device is busy, or network disconnected "; break;
                case MvCodeReader.MV_CODEREADER_E_NETER: errorMsg += " Network error "; break;
            }

            MessageBox.Show(errorMsg, "PROMPT");
        }
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

        private void bnEnum_Click(object sender, EventArgs e)
        {
            DeviceListAcq();
        }

        private void DeviceListAcq()
        {
            // ch:创建设备列表 | en:Create Device List
            System.GC.Collect();
            cbDeviceList.Items.Clear();
            m_stDeviceList.nDeviceNum = 0;
            int nRet = MvCodeReader.MV_CODEREADER_EnumDevices_NET(ref m_stDeviceList, MvCodeReader.MV_CODEREADER_GIGE_DEVICE);
            if (0 != nRet)
            {
                ShowErrorMsg("Enumerate devices fail!",nRet);
                return;
            }

            if (0 == m_stDeviceList.nDeviceNum)
            {
                ShowErrorMsg("None Device!", 0);
                return;
            }

            string strUserDefinedName = "";
            // ch:在窗体列表中显示设备名 | en:Display stDevInfo name in the form list
            for (int i = 0; i < m_stDeviceList.nDeviceNum; i++)
            {
                MvCodeReader.MV_CODEREADER_DEVICE_INFO stDevInfo = (MvCodeReader.MV_CODEREADER_DEVICE_INFO)Marshal.PtrToStructure(m_stDeviceList.pDeviceInfo[i], typeof(MvCodeReader.MV_CODEREADER_DEVICE_INFO));
                if (stDevInfo.nTLayerType == MvCodeReader.MV_CODEREADER_GIGE_DEVICE)
                {
                    IntPtr buffer = Marshal.UnsafeAddrOfPinnedArrayElement(stDevInfo.SpecialInfo.stGigEInfo, 0);
                    MvCodeReader.MV_CODEREADER_GIGE_DEVICE_INFO stGigEDeviceInfo = (MvCodeReader.MV_CODEREADER_GIGE_DEVICE_INFO)Marshal.PtrToStructure(buffer, typeof(MvCodeReader.MV_CODEREADER_GIGE_DEVICE_INFO));
                    if (stGigEDeviceInfo.chUserDefinedName != "")
                    {
                        byte[] byteUserDefinedName = Encoding.GetEncoding("GB2312").GetBytes(stGigEDeviceInfo.chUserDefinedName);
                        bool bIsValidUTF8 = IsTextUTF8(byteUserDefinedName);
                        if (bIsValidUTF8)
                        {
                            strUserDefinedName = Encoding.UTF8.GetString(byteUserDefinedName);
                        }
                        else
                        {
                            strUserDefinedName = Encoding.GetEncoding("GB2312").GetString(byteUserDefinedName);
                        }
                        cbDeviceList.Items.Add("GEV: " + strUserDefinedName + " (" + stGigEDeviceInfo.chSerialNumber + ")");
 
                    }
                    else
                    {
                        cbDeviceList.Items.Add("GEV: " + stGigEDeviceInfo.chManufacturerName + " " + stGigEDeviceInfo.chModelName + " (" + stGigEDeviceInfo.chSerialNumber + ")");
                    }
                }
            }

            // ch:选择第一项 | en:Select the first item
            if (m_stDeviceList.nDeviceNum != 0)
            {
                cbDeviceList.SelectedIndex = 0;
            }
        }

        private Int32 InitDevice()
        {
            Int32 nRet = m_cMyDevice.MV_CODEREADER_RegisterExceptionCallBack_NET(pCallBackFunc, IntPtr.Zero);
            if (MvCodeReader.MV_CODEREADER_OK != nRet)
            {
                return nRet;
            }
            GC.KeepAlive(pCallBackFunc);

            // 打开设备控件操作
            SetCtrlWhenOpen();

            // 开始取流
            m_bGrabbing = true;

            m_hReceiveThread = new Thread(ReceiveThreadProcess);
            m_hReceiveThread.Start();

            m_stFrameInfo.nFrameLen = 0;//取流之前先清除帧长度
            m_stFrameInfo.enPixelType = MvCodeReader.MvCodeReaderGvspPixelType.PixelType_CodeReader_Gvsp_Undefined;
            
            // ch:开始采集 | en:Start Grabbing
            nRet = m_cMyDevice.MV_CODEREADER_StartGrabbing_NET();
            if (MvCodeReader.MV_CODEREADER_OK != nRet)
            {
                m_bGrabbing = false;
                m_hReceiveThread.Join();
                ShowErrorMsg("Start Grabbing Fail!", nRet);
                return nRet;
            }

            // ch:控件操作 | en:Control Operation
            SetCtrlWhenStartGrab();
            return nRet;
        }

        private void DeInitDevice()
        {
            // ch:取流标志位清零 | en:Reset flow flag bit
            if (m_bGrabbing == true)
            {
                m_bGrabbing = false;
                m_hReceiveThread.Join();

                // ch:停止采集 | en:Stop Grabbing
                int nRet = m_cMyDevice.MV_CODEREADER_StopGrabbing_NET();
                if (nRet != MvCodeReader.MV_CODEREADER_OK)
                {
                    ShowErrorMsg("Stop Grabbing Fail!", nRet);
                }
            }

            // ch:关闭设备 | en:Close Device
            m_cMyDevice.MV_CODEREADER_CloseDevice_NET();
            m_cMyDevice.MV_CODEREADER_DestroyHandle_NET();
        }

        // ch:回调函数 | en:Callback function
        private void cbExceptiondelegate(UInt32 nMsgType, IntPtr pUser)
        {
            if (MvCodeReader.MV_CODEREADER_EXCEPTION_DEV_DISCONNECT == nMsgType)
            {
                DeInitDevice();

                // ch:获取选择的设备信息 | en:Get Used Device Info
                MvCodeReader.MV_CODEREADER_DEVICE_INFO stDevInfo =
                (MvCodeReader.MV_CODEREADER_DEVICE_INFO)Marshal.PtrToStructure(m_stDeviceList.pDeviceInfo[cbDeviceList.SelectedIndex],
                                                              typeof(MvCodeReader.MV_CODEREADER_DEVICE_INFO));

                // ch:打开设备 | en:Open Device
                while (true)
                {
                    int nRet = m_cMyDevice.MV_CODEREADER_CreateHandle_NET(ref stDevInfo);
                    if (MvCodeReader.MV_CODEREADER_OK != nRet)
                    {
                        Thread.Sleep(5);
                        continue;
                    }

                    nRet = m_cMyDevice.MV_CODEREADER_OpenDevice_NET();
                    if (MvCodeReader.MV_CODEREADER_OK != nRet)
                    {
                        m_cMyDevice.MV_CODEREADER_DestroyHandle_NET();
                        continue;
                    }
                    else
                    {
                        nRet = InitDevice();
                        if (MvCodeReader.MV_CODEREADER_OK != nRet)
                        {
                            Thread.Sleep(5);
                            m_cMyDevice.MV_CODEREADER_DestroyHandle_NET();
                            continue;
                        }
                        break;
                    }
                }
            }
        }

        private void SetCtrlWhenOpen()
        {
            bnOpen.Enabled = false;

            bnClose.Enabled = true;
            bnStartGrab.Enabled = true;
            bnStopGrab.Enabled = false;
            bnContinuesMode.Enabled = true;
            bnContinuesMode.Checked = true;
            bnTriggerMode.Enabled = true;
            cbSoftTrigger.Enabled = false;
            bnTriggerExec.Enabled = false;
            if (bnContinuesMode.Checked)
            {
                cbSoftTrigger.Enabled = false;
                bnStartGrab.Enabled = true;
            }
            else
            {
                cbSoftTrigger.Enabled = true;
                bnStartGrab.Enabled = false;
            }
        }

        private void bnOpen_Click(object sender, EventArgs e)
        {
            if (m_stDeviceList.nDeviceNum == 0 || cbDeviceList.SelectedIndex == -1)
            {
                ShowErrorMsg("No stDevInfo, please select", 0);
                return;
            }

            // ch:获取选择的设备信息 | en:Get selected stDevInfo information
            MvCodeReader.MV_CODEREADER_DEVICE_INFO stDevInfo =
                (MvCodeReader.MV_CODEREADER_DEVICE_INFO)Marshal.PtrToStructure(m_stDeviceList.pDeviceInfo[cbDeviceList.SelectedIndex],
                                                              typeof(MvCodeReader.MV_CODEREADER_DEVICE_INFO));

            // ch:打开设备 | en:Open stDevInfo
            if (null == m_cMyDevice)
            {
                m_cMyDevice = new MvCodeReader();
                if (null == m_cMyDevice)
                {
                    return;
                }
            }

            int nRet = m_cMyDevice.MV_CODEREADER_CreateHandle_NET(ref stDevInfo);
            if (MvCodeReader.MV_CODEREADER_OK != nRet)
            {
                return;
            }

            nRet = m_cMyDevice.MV_CODEREADER_OpenDevice_NET();
            if (MvCodeReader.MV_CODEREADER_OK != nRet)
            {
                m_cMyDevice.MV_CODEREADER_DestroyHandle_NET();
                ShowErrorMsg("Device open fail!", nRet);
                return;
            }

            nRet = m_cMyDevice.MV_CODEREADER_RegisterExceptionCallBack_NET(pCallBackFunc, IntPtr.Zero);
            if (MvCodeReader.MV_CODEREADER_OK != nRet)
            {
                ShowErrorMsg("Register expection callback failed!", nRet);
            }
            GC.KeepAlive(pCallBackFunc);

            // ch:设置采集连续模式 | en:Set Continues Aquisition Mode
            m_cMyDevice.MV_CODEREADER_SetEnumValue_NET("TriggerMode", (uint)MvCodeReader.MV_CODEREADER_TRIGGER_MODE.MV_CODEREADER_TRIGGER_MODE_OFF);

            // ch:控件操作 | en:Control operation
            SetCtrlWhenOpen();
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
            bnContinuesMode.Checked = false;
            bnTriggerMode.Checked = false;
            cbSoftTrigger.Checked = false;
        }

        private void bnClose_Click(object sender, EventArgs e)
        {
            // 断开设备
            DeInitDevice();

            // ch:控件操作 | en:Control Operation
            SetCtrlWhenClose();
        }

        private void bnContinuesMode_CheckedChanged(object sender, EventArgs e)
        {
            if (bnContinuesMode.Checked)
            {
                int nRet = m_cMyDevice.MV_CODEREADER_SetEnumValue_NET("TriggerMode", (uint)MvCodeReader.MV_CODEREADER_TRIGGER_MODE.MV_CODEREADER_TRIGGER_MODE_OFF);
                if (MvCodeReader.MV_CODEREADER_OK != nRet)
                {
                    ShowErrorMsg("Set TriggerMode Off Fail!", nRet);
                    return;
                }

                cbSoftTrigger.Enabled = false;
                cbSoftTrigger.Checked = false;
                bnTriggerExec.Enabled = false;
                bnContinuesMode.Enabled = false;
                bnTriggerMode.Enabled = true;
                bnStartGrab.Enabled = true;
                
            }
        }

        private void bnTriggerMode_CheckedChanged(object sender, EventArgs e)
        {
            // ch:打开触发模式 | en:Open Trigger Mode
            if (bnTriggerMode.Checked)
            {
                int nRet = m_cMyDevice.MV_CODEREADER_SetEnumValue_NET("TriggerMode", (uint)MvCodeReader.MV_CODEREADER_TRIGGER_MODE.MV_CODEREADER_TRIGGER_MODE_ON);
                if (MvCodeReader.MV_CODEREADER_OK != nRet)
                {
                    ShowErrorMsg("Set TriggerMode On Fail!", nRet);
                    bnContinuesMode.Checked = true;
                    return;
                }

                // ch:触发源选择:0 - Line0; | en:Trigger source select:0 - Line0;
                //           1 - Line1;
                //           2 - Line2;
                //           3 - Line3;
                //           4 - Counter;
                //           7 - Software;
                if (cbSoftTrigger.Checked)
                {
                    nRet = m_cMyDevice.MV_CODEREADER_SetEnumValue_NET("TriggerSource", (uint)MvCodeReader.MV_CODEREADER_TRIGGER_SOURCE.MV_CODEREADER_TRIGGER_SOURCE_SOFTWARE);
                    if (MvCodeReader.MV_CODEREADER_OK != nRet)
                    {
                        ShowErrorMsg("Set TriggerMode Source SoftWare Fail!", nRet);
                        return;
                    }

                    if (m_bGrabbing)
                    {
                        bnTriggerExec.Enabled = true;
                    }
                }
                else
                {
                    nRet = m_cMyDevice.MV_CODEREADER_SetEnumValue_NET("TriggerSource", (uint)MvCodeReader.MV_CODEREADER_TRIGGER_SOURCE.MV_CODEREADER_TRIGGER_SOURCE_LINE0);
                    if (MvCodeReader.MV_CODEREADER_OK != nRet)
                    {
                        ShowErrorMsg("Set TriggerMode Source Line0 Fail!", nRet);
                        return;

                    }
                }
                cbSoftTrigger.Enabled = true;
                bnTriggerMode.Enabled = false;
                bnContinuesMode.Enabled = true;
            }
        }

        private void SetCtrlWhenStartGrab()
        {
            bnStartGrab.Enabled = false;
            bnStopGrab.Enabled = true;
            cbSoftTrigger.Enabled = false;
            if (bnTriggerMode.Checked && cbSoftTrigger.Checked)
            {
                bnTriggerExec.Enabled = true;
            }

        }

        public void ReceiveThreadProcess()
        {

            int nRet = MvCodeReader.MV_CODEREADER_OK;

            IntPtr pData = IntPtr.Zero;
            MvCodeReader.MV_CODEREADER_IMAGE_OUT_INFO_EX2 stFrameInfoEx2 = new MvCodeReader.MV_CODEREADER_IMAGE_OUT_INFO_EX2();
            IntPtr pstFrameInfoEx2 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(MvCodeReader.MV_CODEREADER_IMAGE_OUT_INFO_EX2)));
            Marshal.StructureToPtr(stFrameInfoEx2, pstFrameInfoEx2, false);

            while (m_bGrabbing)
            {
                nRet = m_cMyDevice.MV_CODEREADER_GetOneFrameTimeoutEx2_NET(ref pData, pstFrameInfoEx2, 1000);
                if (nRet == MvCodeReader.MV_CODEREADER_OK)
                {
                    stFrameInfoEx2 = (MvCodeReader.MV_CODEREADER_IMAGE_OUT_INFO_EX2)Marshal.PtrToStructure(pstFrameInfoEx2, typeof(MvCodeReader.MV_CODEREADER_IMAGE_OUT_INFO_EX2));
                    m_stFrameInfo = stFrameInfoEx2;
                }

                if (nRet == MvCodeReader.MV_CODEREADER_OK)
                {
                    if (0 >= stFrameInfoEx2.nFrameLen)
                    {
                        continue;
                    }

                    // 绘制图像
                    Marshal.Copy(pData, m_BufForDriver, 0, (int)stFrameInfoEx2.nFrameLen);
                    if (stFrameInfoEx2.enPixelType == MvCodeReader.MvCodeReaderGvspPixelType.PixelType_CodeReader_Gvsp_Mono8)
                    {
                        IntPtr pImage = Marshal.UnsafeAddrOfPinnedArrayElement(m_BufForDriver, 0);
                        bmp = new Bitmap(stFrameInfoEx2.nWidth, stFrameInfoEx2.nHeight, stFrameInfoEx2.nWidth, PixelFormat.Format8bppIndexed, pImage);
                        ColorPalette cp = bmp.Palette;
                        for (int i = 0; i < 256; i++)
                        {
                            cp.Entries[i] = Color.FromArgb(i, i, i);
                        }
                        bmp.Palette = cp;

                        pictureBox1.Image = (Image)bmp;
                    }
                    else if (stFrameInfoEx2.enPixelType == MvCodeReader.MvCodeReaderGvspPixelType.PixelType_CodeReader_Gvsp_Jpeg)
                    {
                        GC.Collect();
                        MemoryStream ms = new MemoryStream();
                        ms.Write(m_BufForDriver, 0, (int)stFrameInfoEx2.nFrameLen);

                        pictureBox1.Image = Image.FromStream(ms);
                    }

                    MvCodeReader.MV_CODEREADER_RESULT_BCR_EX2 stBcrResult = (MvCodeReader.MV_CODEREADER_RESULT_BCR_EX2)Marshal.PtrToStructure(stFrameInfoEx2.UnparsedBcrList.pstCodeListEx2, typeof(MvCodeReader.MV_CODEREADER_RESULT_BCR_EX2));

                    pictureBox1.Refresh();
                    for (int i = 0; i < stBcrResult.nCodeNum; ++i)
                    {
                        for (int j = 0; j < 4; ++j)
                        {
                            stPointList[j].X = (int)(stBcrResult.stBcrInfoEx2[i].pt[j].x * (float)(pictureBox1.Size.Width) / stFrameInfoEx2.nWidth);
                            stPointList[j].Y = (int)(stBcrResult.stBcrInfoEx2[i].pt[j].y * (float)(pictureBox1.Size.Height) / stFrameInfoEx2.nHeight);
                        }
                        gra.DrawPolygon(pen, stPointList);
                    }

                    MvCodeReader.MV_CODEREADER_WAYBILL_LIST stWayList = (MvCodeReader.MV_CODEREADER_WAYBILL_LIST)Marshal.PtrToStructure(stFrameInfoEx2.pstWaybillList, typeof(MvCodeReader.MV_CODEREADER_WAYBILL_LIST));

                    for (int i = 0; i < stWayList.nWaybillNum; ++i)
                    {
                        float fWayX = (float)(stWayList.stWaybillInfo[i].fCenterX * (float)(pictureBox1.Size.Width) / stFrameInfoEx2.nWidth);
                        float fWayY = (float)(stWayList.stWaybillInfo[i].fCenterY * (float)(pictureBox1.Size.Height) / stFrameInfoEx2.nHeight);
                        float fWayW = (float)(stWayList.stWaybillInfo[i].fWidth * (float)(pictureBox1.Size.Width) / stFrameInfoEx2.nWidth);
                        float fWayH = (float)(stWayList.stWaybillInfo[i].fHeight * (float)(pictureBox1.Size.Height) / stFrameInfoEx2.nHeight);

                        WayShapePath.Reset();
                        WayShapePath.AddRectangle(new RectangleF(fWayX - fWayW / 2, fWayY - fWayH / 2, fWayW, fWayH));

                        stRotateWay.Reset();
                        PointF stCenPoint = new PointF(fWayX, fWayY);
                        stRotateWay.RotateAt(stWayList.stWaybillInfo[i].fAngle, stCenPoint);
                        WayShapePath.Transform(stRotateWay);
                        gra.DrawPath(penWay, WayShapePath);
                    }

                    MvCodeReader.MV_CODEREADER_OCR_INFO_LIST stOcrInfo = (MvCodeReader.MV_CODEREADER_OCR_INFO_LIST)Marshal.PtrToStructure(stFrameInfoEx2.UnparsedOcrList.pstOcrList, typeof(MvCodeReader.MV_CODEREADER_OCR_INFO_LIST));

                    for (int i = 0; i < stOcrInfo.nOCRAllNum; ++i)
                    {
                        float fOcrInfoX = (float)(stOcrInfo.stOcrRowInfo[i].nOcrRowCenterX * (float)(pictureBox1.Size.Width) / stFrameInfoEx2.nWidth);
                        float fOcrInfoY = (float)(stOcrInfo.stOcrRowInfo[i].nOcrRowCenterY * (float)(pictureBox1.Size.Height) / stFrameInfoEx2.nHeight);
                        float fOcrInfoW = (float)(stOcrInfo.stOcrRowInfo[i].nOcrRowWidth * (float)(pictureBox1.Size.Width) / stFrameInfoEx2.nWidth);
                        float fOcrInfoH = (float)(stOcrInfo.stOcrRowInfo[i].nOcrRowHeight * (float)(pictureBox1.Size.Height) / stFrameInfoEx2.nHeight);

                        OcrShapePath.Reset();
                        OcrShapePath.AddRectangle(new RectangleF(fOcrInfoX - fOcrInfoW / 2, fOcrInfoY - fOcrInfoH / 2, fOcrInfoW, fOcrInfoH));

                        stRotateM.Reset();
                        PointF stCenPoint = new PointF(fOcrInfoX, fOcrInfoY);
                        stRotateM.RotateAt(stOcrInfo.stOcrRowInfo[i].fOcrRowAngle, stCenPoint);
                        OcrShapePath.Transform(stRotateM);
                        gra.DrawPath(penOcr, OcrShapePath);
                    }
                }
            }
        }

        private void bnStartGrab_Click(object sender, EventArgs e)
        {
            // ch:标志位置位true | en:Set position bit true
            m_bGrabbing = true;

            m_hReceiveThread = new Thread(ReceiveThreadProcess);
            m_hReceiveThread.Start();

            m_stFrameInfo.nFrameLen = 0;//取流之前先清除帧长度
            m_stFrameInfo.enPixelType = MvCodeReader.MvCodeReaderGvspPixelType.PixelType_CodeReader_Gvsp_Undefined;
            // ch:开始采集 | en:Start Grabbing
            int nRet = m_cMyDevice.MV_CODEREADER_StartGrabbing_NET();
            if (MvCodeReader.MV_CODEREADER_OK != nRet)
            {
                m_bGrabbing = false;
                m_hReceiveThread.Join();
                ShowErrorMsg("Start Grabbing Fail!", nRet);
                return;
            }

            // ch:控件操作 | en:Control Operation
            SetCtrlWhenStartGrab();
            bnContinuesMode.Enabled = false;
            bnTriggerMode.Enabled = false;
        }

        private void cbSoftTrigger_CheckedChanged(object sender, EventArgs e)
        {
            if (cbSoftTrigger.Checked)
            {
                // ch:触发源设为软触发 | en:Set trigger source as Software
                m_cMyDevice.MV_CODEREADER_SetEnumValue_NET("TriggerSource", (uint)MvCodeReader.MV_CODEREADER_TRIGGER_SOURCE.MV_CODEREADER_TRIGGER_SOURCE_SOFTWARE);
                if (m_bGrabbing)
                {
                    bnTriggerExec.Enabled = true;
                }
            }
            else
            {
                m_cMyDevice.MV_CODEREADER_SetEnumValue_NET("TriggerSource", (uint)MvCodeReader.MV_CODEREADER_TRIGGER_SOURCE.MV_CODEREADER_TRIGGER_SOURCE_LINE0);
                bnTriggerExec.Enabled = false;
            }
        }

        private void bnTriggerExec_Click(object sender, EventArgs e)
        {
            // ch:触发命令 | en:Trigger command
            int nRet = m_cMyDevice.MV_CODEREADER_SetCommandValue_NET("TriggerSoftware");
            if (MvCodeReader.MV_CODEREADER_OK != nRet)
            {
                ShowErrorMsg("Trigger Software Fail!", nRet);
            }
        }

        private void SetCtrlWhenStopGrab()
        {
            bnStartGrab.Enabled = true;
            bnStopGrab.Enabled = false;

            bnTriggerExec.Enabled = false;
            if (bnContinuesMode.Checked)
            {
                cbSoftTrigger.Enabled = false;
                bnContinuesMode.Enabled = false;
                bnTriggerMode.Enabled = true;
            }
            else
            {
                cbSoftTrigger.Enabled = true;
                bnContinuesMode.Enabled = true;
                bnTriggerMode.Enabled = false;
            }
        }

        private void bnStopGrab_Click(object sender, EventArgs e)
        {
            // ch:标志位设为false | en:Set flag bit false
            m_bGrabbing = false;
            if (null != m_hReceiveThread)
            {
                m_hReceiveThread.Join();
            }

            // ch:停止采集 | en:Stop Grabbing
            int nRet = m_cMyDevice.MV_CODEREADER_StopGrabbing_NET();
            if (nRet != MvCodeReader.MV_CODEREADER_OK)
            {
                ShowErrorMsg("Stop Grabbing Fail!" , nRet);
            }

            // ch:控件操作 | en:Control Operation
            SetCtrlWhenStopGrab();
        }

        private void ReconnectDemo_FormClosing(object sender, FormClosingEventArgs e)
        {
            bnClose_Click(sender, e);
        }
    }
}
