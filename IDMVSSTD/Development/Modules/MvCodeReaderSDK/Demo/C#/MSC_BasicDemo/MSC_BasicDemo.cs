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

namespace MSC_BasicDemo
{
    public partial class Form1 : Form
    {
        MvCodeReader.MV_CODEREADER_DEVICE_INFO_LIST m_pstDeviceList = new MvCodeReader.MV_CODEREADER_DEVICE_INFO_LIST();
        private MvCodeReader m_MyCamera = new MvCodeReader();
        bool m_bGrabbing = false;
        Thread m_hRecvChannel0Thread = null;
        Thread m_hRecvChannel1Thread = null;
        MvCodeReader.MV_CODEREADER_IMAGE_OUT_INFO_EX2 m_stChannel0FrameInfo = new MvCodeReader.MV_CODEREADER_IMAGE_OUT_INFO_EX2();
        MvCodeReader.MV_CODEREADER_IMAGE_OUT_INFO_EX2 m_stChannel1FrameInfo = new MvCodeReader.MV_CODEREADER_IMAGE_OUT_INFO_EX2();

        // ch:用于从驱动获取图像的缓存 | en:Buffer for getting image from driver
        byte[] m_BufForChannel0Driver = new byte[1024 * 1024 * 20];
        byte[] m_BufForChannel1Driver = new byte[1024 * 1024 * 20];

        // 显示
        Bitmap bmp = null;
        Graphics graBox1 = null;
        Graphics graBox2 = null;
        Pen penChannel0 = new Pen(Color.Blue, 3);                   // 画笔颜色
        Pen penChannel1 = new Pen(Color.Blue, 3);                   // 画笔颜色
        Pen penOCR = new Pen(Color.Yellow, 3);
        Pen penWay = new Pen(Color.Red, 3);
        Point[] stPointChannel0List = new Point[4];                 // 条码位置的4个点坐标
        Point[] stPointChannel1List = new Point[4];                 // 条码位置的4个点坐标
        GraphicsPath WayShapePath = new GraphicsPath();           // 图形路径，内部变量 
        GraphicsPath OcrShapePath_0 = new GraphicsPath();           // 图形路径，内部变量 
        Matrix stRotateM_0 = new Matrix();
        Matrix stRotateWay = new Matrix();
        GraphicsPath OcrShapePath_1 = new GraphicsPath();           // 图形路径，内部变量 
        Matrix stRotateM_1 = new Matrix();

        public Form1()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;

            pictureBox1.Show();
            graBox1 = pictureBox1.CreateGraphics();

            pictureBox2.Show();
            graBox2 = pictureBox2.CreateGraphics();
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

        private void bnEnum_Click(object sender, EventArgs e)
        {
            DeviceListAcq();
        }

        private void DeviceListAcq()
        {
            // ch:创建设备列表 | en:Create Device List
            System.GC.Collect();
            cbDeviceList.Items.Clear();
            m_pstDeviceList.nDeviceNum = 0;
            int nRet = MvCodeReader.MV_CODEREADER_EnumDevices_NET(ref m_pstDeviceList, MvCodeReader.MV_CODEREADER_GIGE_DEVICE);
            if (0 != nRet)
            {
                ShowErrorMsg("Enumerate devices fail!",nRet);
                return;
            }

            if (0 == m_pstDeviceList.nDeviceNum)
            {
                ShowErrorMsg("None Device!", 0);
                return;
            }

            byte[] chUserDefinedName = null;

            // ch:在窗体列表中显示设备名 | en:Display stDevInfo name in the form list
            for (int i = 0; i < m_pstDeviceList.nDeviceNum; i++)
            {
                MvCodeReader.MV_CODEREADER_DEVICE_INFO stDevInfo = (MvCodeReader.MV_CODEREADER_DEVICE_INFO)Marshal.PtrToStructure(m_pstDeviceList.pDeviceInfo[i], typeof(MvCodeReader.MV_CODEREADER_DEVICE_INFO));
                if (stDevInfo.nTLayerType == MvCodeReader.MV_CODEREADER_GIGE_DEVICE)
                {
                    IntPtr buffer = Marshal.UnsafeAddrOfPinnedArrayElement(stDevInfo.SpecialInfo.stGigEInfo, 0);
                    MvCodeReader.MV_CODEREADER_GIGE_DEVICE_INFO stGigeInfo = (MvCodeReader.MV_CODEREADER_GIGE_DEVICE_INFO)Marshal.PtrToStructure(buffer, typeof(MvCodeReader.MV_CODEREADER_GIGE_DEVICE_INFO));
                    if (stGigeInfo.chUserDefinedName != "")
                    {
                        cbDeviceList.Items.Add("GEV: " + stGigeInfo.chUserDefinedName + " (" + stGigeInfo.chSerialNumber + ")");
                    }
                    else
                    {
                        cbDeviceList.Items.Add("GEV: " + stGigeInfo.chManufacturerName + " " + stGigeInfo.chModelName + " (" + stGigeInfo.chSerialNumber + ")");
                    }
                }
            }

            // ch:选择第一项 | en:Select the first item
            if (m_pstDeviceList.nDeviceNum != 0)
            {
                cbDeviceList.SelectedIndex = 0;
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

            tbExposure.Enabled = true;
            tbGain.Enabled = true;
            tbFrameRate.Enabled = true;
            bnGetParam.Enabled = true;
            bnSetParam.Enabled = true;
        }

        private void bnOpen_Click(object sender, EventArgs e)
        {
            if (m_pstDeviceList.nDeviceNum == 0 || cbDeviceList.SelectedIndex == -1)
            {
                ShowErrorMsg("No stDevInfo, please select", 0);
                return;
            }

            // ch:获取选择的设备信息 | en:Get selected stDevInfo information
            MvCodeReader.MV_CODEREADER_DEVICE_INFO stDevInfo =
                (MvCodeReader.MV_CODEREADER_DEVICE_INFO)Marshal.PtrToStructure(m_pstDeviceList.pDeviceInfo[cbDeviceList.SelectedIndex],
                                                              typeof(MvCodeReader.MV_CODEREADER_DEVICE_INFO));

            // ch:打开设备 | en:Open stDevInfo
            if (null == m_MyCamera)
            {
                m_MyCamera = new MvCodeReader();
                if (null == m_MyCamera)
                {
                    return;
                }
            }

            int nRet = m_MyCamera.MV_CODEREADER_CreateHandle_NET(ref stDevInfo);
            if (MvCodeReader.MV_CODEREADER_OK != nRet)
            {
                return;
            }

            nRet = m_MyCamera.MV_CODEREADER_OpenDevice_NET();
            if (MvCodeReader.MV_CODEREADER_OK != nRet)
            {
                m_MyCamera.MV_CODEREADER_DestroyHandle_NET();
                ShowErrorMsg("Device open fail!", nRet);
                return;
            }

            // ch:设置采集连续模式 | en:Set Continues Aquisition Mode
            m_MyCamera.MV_CODEREADER_SetEnumValue_NET("TriggerMode", (uint)MvCodeReader.MV_CODEREADER_TRIGGER_MODE.MV_CODEREADER_TRIGGER_MODE_OFF);

            bnGetParam_Click(null, null);// ch:获取参数 | en:Get parameters

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

            tbExposure.Enabled = false;
            tbGain.Enabled = false;
            tbFrameRate.Enabled = false;
            bnGetParam.Enabled = false;
            bnSetParam.Enabled = false;
        }

        private void bnClose_Click(object sender, EventArgs e)
        {
            // ch:取流标志位清零 | en:Reset flow flag bit
            if (m_bGrabbing == true)
            {
                m_bGrabbing = false;
                m_hRecvChannel0Thread.Join();
                m_hRecvChannel1Thread.Join();

                // ch:停止采集 | en:Stop Grabbing
                int nRet = m_MyCamera.MV_CODEREADER_StopGrabbing_NET();
                if (nRet != MvCodeReader.MV_CODEREADER_OK)
                {
                    ShowErrorMsg("Stop Grabbing Fail!", nRet);
                }
            }

            // ch:关闭设备 | en:Close Device
            m_MyCamera.MV_CODEREADER_CloseDevice_NET();
            m_MyCamera.MV_CODEREADER_DestroyHandle_NET();

            // ch:控件操作 | en:Control Operation
            SetCtrlWhenClose();
        }

        private void bnContinuesMode_CheckedChanged(object sender, EventArgs e)
        {
            if (bnContinuesMode.Checked)
            {
                int nRet = m_MyCamera.MV_CODEREADER_SetEnumValue_NET("TriggerMode", (uint)MvCodeReader.MV_CODEREADER_TRIGGER_MODE.MV_CODEREADER_TRIGGER_MODE_OFF);
                if (MvCodeReader.MV_CODEREADER_OK != nRet)
                {
                    ShowErrorMsg("Set TriggerMode Off Fail!", nRet);
                    return;
                }

                cbSoftTrigger.Enabled = false;
                bnTriggerExec.Enabled = false;
                cbSoftTrigger.Checked = false;
                bnStartGrab.Enabled = true;
                bnContinuesMode.Enabled = false;
                bnTriggerMode.Enabled = true;
            }
        }

        private void bnTriggerMode_CheckedChanged(object sender, EventArgs e)
        {
            // ch:打开触发模式 | en:Open Trigger Mode
            if (bnTriggerMode.Checked)
            {
                int nRet = m_MyCamera.MV_CODEREADER_SetEnumValue_NET("TriggerMode", (uint)MvCodeReader.MV_CODEREADER_TRIGGER_MODE.MV_CODEREADER_TRIGGER_MODE_ON);
                if (MvCodeReader.MV_CODEREADER_OK != nRet)
                {
                    ShowErrorMsg("Set TriggerMode On Fail!", nRet);
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
                    nRet = m_MyCamera.MV_CODEREADER_SetEnumValue_NET("TriggerSource", (uint)MvCodeReader.MV_CODEREADER_TRIGGER_SOURCE.MV_CODEREADER_TRIGGER_SOURCE_SOFTWARE);
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
                    nRet = m_MyCamera.MV_CODEREADER_SetEnumValue_NET("TriggerSource", (uint)MvCodeReader.MV_CODEREADER_TRIGGER_SOURCE.MV_CODEREADER_TRIGGER_SOURCE_LINE0);
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
            bnContinuesMode.Enabled = false;
            bnTriggerMode.Enabled = false;
            cbSoftTrigger.Enabled = false;
            if (bnTriggerMode.Checked && cbSoftTrigger.Checked)
            {
                bnTriggerExec.Enabled = true;
                cbSoftTrigger.Enabled = false;
            }
        }

        public void RecvChannel0Thread()
        {
            int nRet = MvCodeReader.MV_CODEREADER_OK;

            IntPtr pData = IntPtr.Zero;
            MvCodeReader.MV_CODEREADER_IMAGE_OUT_INFO_EX2 stFrameChannel0Info = new MvCodeReader.MV_CODEREADER_IMAGE_OUT_INFO_EX2();
            IntPtr pstChannel0InfoEx2 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(MvCodeReader.MV_CODEREADER_IMAGE_OUT_INFO_EX2)));
            Marshal.StructureToPtr(stFrameChannel0Info, pstChannel0InfoEx2, false);

            while (m_bGrabbing)
            {
                nRet = m_MyCamera.MV_CODEREADER_MSC_GetOneFrameTimeout_NET(ref pData, pstChannel0InfoEx2, 0, 1000);
                if (nRet == MvCodeReader.MV_CODEREADER_OK)
                {
                    stFrameChannel0Info = (MvCodeReader.MV_CODEREADER_IMAGE_OUT_INFO_EX2)Marshal.PtrToStructure(pstChannel0InfoEx2, typeof(MvCodeReader.MV_CODEREADER_IMAGE_OUT_INFO_EX2));
                    m_stChannel0FrameInfo = stFrameChannel0Info;
                }

                if (nRet == MvCodeReader.MV_CODEREADER_OK)
                {
                    if (0 >= stFrameChannel0Info.nFrameLen)
                    {
                        continue;
                    }

                    // 通道0绘制图像
                    Marshal.Copy(pData, m_BufForChannel0Driver, 0, (int)stFrameChannel0Info.nFrameLen);
                    if (stFrameChannel0Info.enPixelType == MvCodeReader.MvCodeReaderGvspPixelType.PixelType_CodeReader_Gvsp_Mono8)
                    {
                        IntPtr pImage = Marshal.UnsafeAddrOfPinnedArrayElement(m_BufForChannel0Driver, 0);
                        bmp = new Bitmap(stFrameChannel0Info.nWidth, stFrameChannel0Info.nHeight, stFrameChannel0Info.nWidth, PixelFormat.Format8bppIndexed, pImage);
                        ColorPalette cp = bmp.Palette;
                        for (int i = 0; i < 256; i++)
                        {
                            cp.Entries[i] = Color.FromArgb(i, i, i);
                        }
                        bmp.Palette = cp;

                        pictureBox1.Image = (Image)bmp;
                    }
                    else if (stFrameChannel0Info.enPixelType == MvCodeReader.MvCodeReaderGvspPixelType.PixelType_CodeReader_Gvsp_Jpeg)
                    {
                        GC.Collect();
                        MemoryStream ms = new MemoryStream();
                        ms.Write(m_BufForChannel0Driver, 0, (int)stFrameChannel0Info.nFrameLen);

                        pictureBox1.Image = Image.FromStream(ms);
                    }

                    MvCodeReader.MV_CODEREADER_RESULT_BCR_EX stBcrResult = (MvCodeReader.MV_CODEREADER_RESULT_BCR_EX)Marshal.PtrToStructure(stFrameChannel0Info.pstCodeListEx, typeof(MvCodeReader.MV_CODEREADER_RESULT_BCR_EX));

                    if (stFrameChannel0Info.bIsGetCode)
                    {
                        pictureBox1.Refresh();
                        for (int i = 0; i < stBcrResult.nCodeNum; ++i)
                        {
                            stPointChannel0List = new Point[4];
                            for (int j = 0; j < 4; ++j)
                            {
                                stPointChannel0List[j].X = (int)(stBcrResult.stBcrInfoEx[i].pt[j].x * (float)(pictureBox1.Size.Width) / stFrameChannel0Info.nWidth);
                                stPointChannel0List[j].Y = (int)(stBcrResult.stBcrInfoEx[i].pt[j].y * (float)(pictureBox1.Size.Height) / stFrameChannel0Info.nHeight);
                            }

                            graBox1.DrawPolygon(penChannel0, stPointChannel0List);
                        }
                    }

                    MvCodeReader.MV_CODEREADER_WAYBILL_LIST stWayList = (MvCodeReader.MV_CODEREADER_WAYBILL_LIST)Marshal.PtrToStructure(stFrameChannel0Info.pstWaybillList, typeof(MvCodeReader.MV_CODEREADER_WAYBILL_LIST));

                    for (int i = 0; i < stWayList.nWaybillNum; ++i)
                    {
                        float fWayX = (float)(stWayList.stWaybillInfo[i].fCenterX * (float)(pictureBox1.Size.Width) / stFrameChannel0Info.nWidth);
                        float fWayY = (float)(stWayList.stWaybillInfo[i].fCenterY * (float)(pictureBox1.Size.Height) / stFrameChannel0Info.nHeight);
                        float fWayW = (float)(stWayList.stWaybillInfo[i].fWidth * (float)(pictureBox1.Size.Width) / stFrameChannel0Info.nWidth);
                        float fWayH = (float)(stWayList.stWaybillInfo[i].fHeight * (float)(pictureBox1.Size.Height) / stFrameChannel0Info.nHeight);

                        WayShapePath.Reset();
                        WayShapePath.AddRectangle(new RectangleF(fWayX - fWayW / 2, fWayY - fWayH / 2, fWayW, fWayH));

                        stRotateWay.Reset();
                        PointF stCenPoint = new PointF(fWayX, fWayY);
                        stRotateWay.RotateAt(stWayList.stWaybillInfo[i].fAngle, stCenPoint);
                        WayShapePath.Transform(stRotateWay);
                        graBox1.DrawPath(penWay, WayShapePath);
                    }

                    MvCodeReader.MV_CODEREADER_OCR_INFO_LIST stOcrInfo = (MvCodeReader.MV_CODEREADER_OCR_INFO_LIST)Marshal.PtrToStructure(stFrameChannel0Info.UnparsedOcrList.pstOcrList, typeof(MvCodeReader.MV_CODEREADER_OCR_INFO_LIST));

                    for (int i = 0; i < stOcrInfo.nOCRAllNum; ++i)
                    {
                        float fOcrInfoX = (float)(stOcrInfo.stOcrRowInfo[i].nOcrRowCenterX * (float)(pictureBox1.Size.Width) / stFrameChannel0Info.nWidth);
                        float fOcrInfoY = (float)(stOcrInfo.stOcrRowInfo[i].nOcrRowCenterY * (float)(pictureBox1.Size.Height) / stFrameChannel0Info.nHeight);
                        float fOcrInfoW = (float)(stOcrInfo.stOcrRowInfo[i].nOcrRowWidth * (float)(pictureBox1.Size.Width) / stFrameChannel0Info.nWidth);
                        float fOcrInfoH = (float)(stOcrInfo.stOcrRowInfo[i].nOcrRowHeight * (float)(pictureBox1.Size.Height) / stFrameChannel0Info.nHeight);

                        OcrShapePath_0.Reset();
                        OcrShapePath_0.AddRectangle(new RectangleF(fOcrInfoX - fOcrInfoW / 2, fOcrInfoY - fOcrInfoH / 2, fOcrInfoW, fOcrInfoH));

                        stRotateM_0.Reset();
                        PointF stCenPoint = new PointF(fOcrInfoX, fOcrInfoY);
                        stRotateM_0.RotateAt(stOcrInfo.stOcrRowInfo[i].fOcrRowAngle, stCenPoint);
                        OcrShapePath_0.Transform(stRotateM_0);
                        graBox1.DrawPath(penOCR, OcrShapePath_0);

                    }
                }
                else
                {
                    if (MvCodeReader.MV_CODEREADER_E_PARAMETER == nRet)
                    {
                        break;
                    }

                    if (bnTriggerMode.Checked)
                    {
                        Thread.Sleep(5);
                    }
                    continue;
                }
            }
        }

        public void RecvChannel1Thread()
        {
            int nRet = MvCodeReader.MV_CODEREADER_OK;

            IntPtr pData = IntPtr.Zero;
            MvCodeReader.MV_CODEREADER_IMAGE_OUT_INFO_EX2 stFrameInfo = new MvCodeReader.MV_CODEREADER_IMAGE_OUT_INFO_EX2();
            IntPtr pstFrameInfoEx2 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(MvCodeReader.MV_CODEREADER_IMAGE_OUT_INFO_EX2)));
            Marshal.StructureToPtr(stFrameInfo, pstFrameInfoEx2, false);

            while (m_bGrabbing)
            {
                nRet = m_MyCamera.MV_CODEREADER_MSC_GetOneFrameTimeout_NET(ref pData, pstFrameInfoEx2, 1, 1000);
                if (nRet == MvCodeReader.MV_CODEREADER_OK)
                {
                    stFrameInfo = (MvCodeReader.MV_CODEREADER_IMAGE_OUT_INFO_EX2)Marshal.PtrToStructure(pstFrameInfoEx2, typeof(MvCodeReader.MV_CODEREADER_IMAGE_OUT_INFO_EX2));
                    m_stChannel0FrameInfo = stFrameInfo;
                }

                if (nRet == MvCodeReader.MV_CODEREADER_OK)
                {
                    if (0 >= stFrameInfo.nFrameLen)
                    {
                        continue;
                    }

                    // 通道1绘制图像
                    Marshal.Copy(pData, m_BufForChannel1Driver, 0, (int)stFrameInfo.nFrameLen);
                    if (stFrameInfo.enPixelType == MvCodeReader.MvCodeReaderGvspPixelType.PixelType_CodeReader_Gvsp_Mono8)
                    {
                        IntPtr pImage = Marshal.UnsafeAddrOfPinnedArrayElement(m_BufForChannel1Driver, 0);
                        bmp = new Bitmap(stFrameInfo.nWidth, stFrameInfo.nHeight, stFrameInfo.nWidth, PixelFormat.Format8bppIndexed, pImage);
                        ColorPalette cp = bmp.Palette;
                        for (int i = 0; i < 256; i++)
                        {
                            cp.Entries[i] = Color.FromArgb(i, i, i);
                        }
                        bmp.Palette = cp;

                        pictureBox2.Image = (Image)bmp;
                    }
                    else if (stFrameInfo.enPixelType == MvCodeReader.MvCodeReaderGvspPixelType.PixelType_CodeReader_Gvsp_Jpeg)
                    {
                        GC.Collect();
                        MemoryStream ms = new MemoryStream();
                        ms.Write(m_BufForChannel1Driver, 0, (int)stFrameInfo.nFrameLen);

                        pictureBox2.Image = Image.FromStream(ms);
                    }

                    MvCodeReader.MV_CODEREADER_RESULT_BCR_EX stBcrList = (MvCodeReader.MV_CODEREADER_RESULT_BCR_EX)Marshal.PtrToStructure(stFrameInfo.pstCodeListEx, typeof(MvCodeReader.MV_CODEREADER_RESULT_BCR_EX));

                    if (stFrameInfo.bIsGetCode)
                    {
                        pictureBox2.Refresh();
                        for (int i = 0; i < stBcrList.nCodeNum; ++i)
                        {
                            stPointChannel1List = new Point[4];
                            for (int j = 0; j < 4; ++j)
                            {
                                stPointChannel1List[j].X = (int)(stBcrList.stBcrInfoEx[i].pt[j].x * (float)(pictureBox2.Size.Width) / stFrameInfo.nWidth);
                                stPointChannel1List[j].Y = (int)(stBcrList.stBcrInfoEx[i].pt[j].y * (float)(pictureBox2.Size.Height) / stFrameInfo.nHeight);
                            }

                            graBox2.DrawPolygon(penChannel1, stPointChannel1List);
                        }
                    }

                    MvCodeReader.MV_CODEREADER_OCR_INFO_LIST stOcrInfo = (MvCodeReader.MV_CODEREADER_OCR_INFO_LIST)Marshal.PtrToStructure(stFrameInfo.UnparsedOcrList.pstOcrList, typeof(MvCodeReader.MV_CODEREADER_OCR_INFO_LIST));

                    for (int i = 0; i < stOcrInfo.nOCRAllNum; ++i)
                    {
                        float fOcrInfoX = (float)(stOcrInfo.stOcrRowInfo[i].nOcrRowCenterX * (float)(pictureBox2.Size.Width) / stFrameInfo.nWidth);
                        float fOcrInfoY = (float)(stOcrInfo.stOcrRowInfo[i].nOcrRowCenterY * (float)(pictureBox2.Size.Height) / stFrameInfo.nHeight);
                        float fOcrInfoW = (float)(stOcrInfo.stOcrRowInfo[i].nOcrRowWidth * (float)(pictureBox2.Size.Width) / stFrameInfo.nWidth);
                        float fOcrInfoH = (float)(stOcrInfo.stOcrRowInfo[i].nOcrRowHeight * (float)(pictureBox2.Size.Height) / stFrameInfo.nHeight);

                        OcrShapePath_1.Reset();
                        OcrShapePath_1.AddRectangle(new RectangleF(fOcrInfoX - fOcrInfoW / 2, fOcrInfoY - fOcrInfoH / 2, fOcrInfoW, fOcrInfoH));

                        stRotateM_1.Reset();
                        PointF stCenPoint = new PointF(fOcrInfoX, fOcrInfoY);
                        stRotateM_1.RotateAt(stOcrInfo.stOcrRowInfo[i].fOcrRowAngle, stCenPoint);
                        OcrShapePath_1.Transform(stRotateM_1);
                        graBox2.DrawPath(penOCR, OcrShapePath_1);
                    }
                }
                else
                {
                    if (MvCodeReader.MV_CODEREADER_E_PARAMETER == nRet)
                    {
                        break;
                    }
                    if (bnTriggerMode.Checked)
                    {
                        Thread.Sleep(5);
                    }
                    continue;
                }
            }
        }

        private void bnStartGrab_Click(object sender, EventArgs e)
        {
            // ch:标志位置位true | en:Set position bit true
            m_bGrabbing = true;

            m_hRecvChannel0Thread = new Thread(RecvChannel0Thread);
            m_hRecvChannel0Thread.Start();

            m_hRecvChannel1Thread = new Thread(RecvChannel1Thread);
            m_hRecvChannel1Thread.Start();

            m_stChannel0FrameInfo.nFrameLen = 0;//取流之前先清除帧长度
            m_stChannel0FrameInfo.enPixelType = MvCodeReader.MvCodeReaderGvspPixelType.PixelType_CodeReader_Gvsp_Undefined;

            m_stChannel1FrameInfo.nFrameLen = 0;//取流之前先清除帧长度
            m_stChannel1FrameInfo.enPixelType = MvCodeReader.MvCodeReaderGvspPixelType.PixelType_CodeReader_Gvsp_Undefined;

            // ch:开始采集 | en:Start Grabbing
            int nRet = m_MyCamera.MV_CODEREADER_StartGrabbing_NET();
            if (MvCodeReader.MV_CODEREADER_OK != nRet)
            {
                m_bGrabbing = false;
                m_hRecvChannel0Thread.Join();
                m_hRecvChannel1Thread.Join();
                ShowErrorMsg("Start Grabbing Fail!", nRet);
                return;
            }

            // ch:控件操作 | en:Control Operation
            SetCtrlWhenStartGrab();

        }

        private void cbSoftTrigger_CheckedChanged(object sender, EventArgs e)
        {
            if (cbSoftTrigger.Checked)
            {
                // ch:触发源设为软触发 | en:Set trigger source as Software
                m_MyCamera.MV_CODEREADER_SetEnumValue_NET("TriggerSource", (uint)MvCodeReader.MV_CODEREADER_TRIGGER_SOURCE.MV_CODEREADER_TRIGGER_SOURCE_SOFTWARE);
                if (m_bGrabbing)
                {
                    bnTriggerExec.Enabled = true;
                }
            }
            else
            {
                m_MyCamera.MV_CODEREADER_SetEnumValue_NET("TriggerSource", (uint)MvCodeReader.MV_CODEREADER_TRIGGER_SOURCE.MV_CODEREADER_TRIGGER_SOURCE_LINE0);
                bnTriggerExec.Enabled = false;
            }
        }

        private void bnTriggerExec_Click(object sender, EventArgs e)
        {
            // ch:触发命令 | en:Trigger command
            int nRet = m_MyCamera.MV_CODEREADER_SetCommandValue_NET("TriggerSoftware");
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
            if (bnTriggerMode.Checked)
            {
                cbSoftTrigger.Enabled = true;
                bnTriggerMode.Enabled = false;
                bnContinuesMode.Enabled = true;
            }
            else
            {
                bnContinuesMode.Enabled = false;
                bnTriggerMode.Enabled = true;
            }
        }

        private void bnStopGrab_Click(object sender, EventArgs e)
        {
            // ch:标志位设为false | en:Set flag bit false
            m_bGrabbing = false;

            if (null != m_hRecvChannel0Thread)
            {
                m_hRecvChannel0Thread.Join();
            }

            if (null != m_hRecvChannel1Thread)
            {
                m_hRecvChannel1Thread.Join();
            }

            // ch:停止采集 | en:Stop Grabbing
            int nRet = m_MyCamera.MV_CODEREADER_StopGrabbing_NET();
            if (nRet != MvCodeReader.MV_CODEREADER_OK)
            {
                ShowErrorMsg("Stop Grabbing Fail!" , nRet);
            }

            // ch:控件操作 | en:Control Operation
            SetCtrlWhenStopGrab();
        }

        private void bnGetParam_Click(object sender, EventArgs e)
        {
            MvCodeReader.MV_CODEREADER_FLOATVALUE stParam = new MvCodeReader.MV_CODEREADER_FLOATVALUE();
            int nRet = m_MyCamera.MV_CODEREADER_GetFloatValue_NET("ExposureTime", ref stParam);
            if (MvCodeReader.MV_CODEREADER_OK == nRet)
            {
                tbExposure.Text = stParam.fCurValue.ToString("F1");
            }
            else
            {
                ShowErrorMsg("Get ExposureTime Fail!", nRet);
            }

            nRet = m_MyCamera.MV_CODEREADER_GetFloatValue_NET("Gain", ref stParam);
            if (MvCodeReader.MV_CODEREADER_OK == nRet)
            {
                tbGain.Text = stParam.fCurValue.ToString("F1");
            }
            else
            {
                ShowErrorMsg("Get Gain Fail!", nRet);
            }

            nRet = m_MyCamera.MV_CODEREADER_GetFloatValue_NET("AcquisitionFrameRate", ref stParam);
            if (MvCodeReader.MV_CODEREADER_OK == nRet)
            {
                tbFrameRate.Text = stParam.fCurValue.ToString("F1");
            }
            else
            {
                ShowErrorMsg("Get FrameRate Fail!", nRet);
            }
        }

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
                ShowErrorMsg("Please enter correct type!", 0);
                return;
            }

            bool bIsSetted = true;
            m_MyCamera.MV_CODEREADER_SetEnumValue_NET("ExposureAuto", 0);
            int nRet = m_MyCamera.MV_CODEREADER_SetFloatValue_NET("ExposureTime", float.Parse(tbExposure.Text));
            if (nRet != MvCodeReader.MV_CODEREADER_OK)
            {
                bIsSetted = false;
                ShowErrorMsg("Set Exposure Time Fail!", nRet);
            }

            m_MyCamera.MV_CODEREADER_SetEnumValue_NET("GainAuto", 0);
            nRet = m_MyCamera.MV_CODEREADER_SetFloatValue_NET("Gain", float.Parse(tbGain.Text));
            if (nRet != MvCodeReader.MV_CODEREADER_OK)
            {
                bIsSetted = false;
                ShowErrorMsg("Set Gain Fail!", nRet);
            }

            nRet = m_MyCamera.MV_CODEREADER_SetFloatValue_NET("AcquisitionFrameRate", float.Parse(tbFrameRate.Text));
            if (nRet != MvCodeReader.MV_CODEREADER_OK)
            {
                bIsSetted = false;
                ShowErrorMsg("Set Frame Rate Fail!", nRet);
            }

            if (bIsSetted)
            {
                MessageBox.Show("Set Param Secceed");
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_bGrabbing = false;
            if (null != m_hRecvChannel0Thread)
            {
                m_hRecvChannel0Thread.Join();
            }

            if (null != m_hRecvChannel1Thread)
            {
                m_hRecvChannel1Thread.Join();
            }
        }

    }
}
