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
namespace BasicDemo
{
    public partial class Form1 : Form
    {
        MvCodeReader.MV_CODEREADER_DEVICE_INFO_LIST m_stDeviceList = new MvCodeReader.MV_CODEREADER_DEVICE_INFO_LIST();
        private MvCodeReader m_cMyDevice = new MvCodeReader();
        bool m_bGrabbing = false;
        Thread m_hReceiveThread = null;

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
        public Form1()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            pictureBox1.Show();
            gra = pictureBox1.CreateGraphics();
        }

        //将Byte转换为结构体类型
        public static object ByteToStruct(byte[] bytes, Type type)
        {
            int size = Marshal.SizeOf(type);
            if (size > bytes.Length)
            {
                return null;
            }
            //分配结构体内存空间
            IntPtr structPtr = Marshal.AllocHGlobal(size);
            //将byte数组拷贝到分配好的内存空间
            Marshal.Copy(bytes, 0, structPtr, size);
            //将内存空间转换为目标结构体
            object obj = Marshal.PtrToStructure(structPtr, type);
            //释放内存空间
            Marshal.FreeHGlobal(structPtr);
            return obj;
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
                        cbDeviceList.Items.Add("GEV: " + stGigEDeviceInfo.chUserDefinedName + " (" + stGigEDeviceInfo.chSerialNumber + ")");
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
                ShowErrorMsg("MV_CODEREADER_CreateHandle_NET fail!", nRet);
                return;
            }

            nRet = m_cMyDevice.MV_CODEREADER_OpenDevice_NET();
            if (MvCodeReader.MV_CODEREADER_OK != nRet)
            {
                m_cMyDevice.MV_CODEREADER_DestroyHandle_NET();
                ShowErrorMsg("Device open fail!", nRet);
                return;
            }

            // ch:设置采集连续模式 | en:Set Continues Aquisition Mode
            m_cMyDevice.MV_CODEREADER_SetEnumValue_NET("TriggerMode", (uint)MvCodeReader.MV_CODEREADER_TRIGGER_MODE.MV_CODEREADER_TRIGGER_MODE_OFF);

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

            bnContinuesMode.Checked = false;
            bnTriggerMode.Checked = false;
            cbSoftTrigger.Checked = false;
            tbExposure.Clear();
            tbGain.Clear();
            tbFrameRate.Clear();
        }

        private void bnClose_Click(object sender, EventArgs e)
        {
            // ch:取流标志位清零 | en:Reset flow flag bit
            if (m_bGrabbing == true)
            {
                m_bGrabbing = false;
                // ch:停止采集 | en:Stop Grabbing
                m_cMyDevice.MV_CODEREADER_StopGrabbing_NET();
                m_hReceiveThread.Join();
            }

            // ch:关闭设备 | en:Close Device
            m_cMyDevice.MV_CODEREADER_CloseDevice_NET();
            m_cMyDevice.MV_CODEREADER_DestroyHandle_NET();

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
                bnStartGrab.Enabled = true;

            }
        }

        private void SetCtrlWhenStartGrab()
        {
            bnStartGrab.Enabled = false;
            bnStopGrab.Enabled = true;
            cbSoftTrigger.Enabled = false;
            //bnClose.Enabled = false;

            if (bnTriggerMode.Checked && cbSoftTrigger.Checked)
            {
                bnTriggerExec.Enabled = true;
            }
        }

        public static String GetBarType(MvCodeReader.MV_CODEREADER_CODE_TYPE nBarType)
        {
            switch (nBarType)
            {
                case MvCodeReader.MV_CODEREADER_CODE_TYPE.MV_CODEREADER_TDCR_DM:
                    return "DM码";
                case MvCodeReader.MV_CODEREADER_CODE_TYPE.MV_CODEREADER_TDCR_QR:
                    return "QR码";
                case MvCodeReader.MV_CODEREADER_CODE_TYPE.MV_CODEREADER_BCR_EAN8:
                    return "EAN8码";
                case MvCodeReader.MV_CODEREADER_CODE_TYPE.MV_CODEREADER_BCR_UPCE:
                    return "UPCE码";
                case MvCodeReader.MV_CODEREADER_CODE_TYPE.MV_CODEREADER_BCR_UPCA:
                    return "UPCA码";
                case MvCodeReader.MV_CODEREADER_CODE_TYPE.MV_CODEREADER_BCR_EAN13:
                    return "EAN13码";
                case MvCodeReader.MV_CODEREADER_CODE_TYPE.MV_CODEREADER_BCR_ISBN13:
                    return "ISBN13码";
                case MvCodeReader.MV_CODEREADER_CODE_TYPE.MV_CODEREADER_BCR_CODABAR:
                    return "库德巴码";
                case MvCodeReader.MV_CODEREADER_CODE_TYPE.MV_CODEREADER_BCR_ITF25:
                    return "交叉25码";
                case MvCodeReader.MV_CODEREADER_CODE_TYPE.MV_CODEREADER_BCR_CODE39:
                    return " Code 39码";
                case MvCodeReader.MV_CODEREADER_CODE_TYPE.MV_CODEREADER_BCR_CODE93:
                    return "Code 93码";
                case MvCodeReader.MV_CODEREADER_CODE_TYPE.MV_CODEREADER_BCR_CODE128:
                    return "Code 128码";
                case MvCodeReader.MV_CODEREADER_CODE_TYPE.MV_CODEREADER_TDCR_PDF417:
                    return "PDF417码";
                case MvCodeReader.MV_CODEREADER_CODE_TYPE.MV_CODEREADER_BCR_MATRIX25:
                    return "MATRIX25码";
                case MvCodeReader.MV_CODEREADER_CODE_TYPE.MV_CODEREADER_BCR_MSI:
                    return "MSI码";
                case MvCodeReader.MV_CODEREADER_CODE_TYPE.MV_CODEREADER_BCR_CODE11:
                    return "Code 11码";
                case MvCodeReader.MV_CODEREADER_CODE_TYPE.MV_CODEREADER_BCR_INDUSTRIAL25:
                    return "industria125码";
                case MvCodeReader.MV_CODEREADER_CODE_TYPE.MV_CODEREADER_BCR_CHINAPOST:
                    return "中国邮政码";
                case MvCodeReader.MV_CODEREADER_CODE_TYPE.MV_CODEREADER_BCR_ITF14:
                    return "交叉14码";
                case MvCodeReader.MV_CODEREADER_CODE_TYPE.MV_CODEREADER_TDCR_ECC140:
                    return "ECC140码";
                default:
                    return "/";
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

                    MvCodeReader.MV_CODEREADER_RESULT_BCR_EX2 stBcrResultEx2 = (MvCodeReader.MV_CODEREADER_RESULT_BCR_EX2)Marshal.PtrToStructure(stFrameInfoEx2.UnparsedBcrList.pstCodeListEx2, typeof(MvCodeReader.MV_CODEREADER_RESULT_BCR_EX2));


                    pictureBox1.Refresh();
                    for (int i = 0; i < stBcrResultEx2.nCodeNum; ++i)
                    {
                        for (int j = 0; j < 4; ++j)
                        {
                            stPointList[j].X = (int)(stBcrResultEx2.stBcrInfoEx2[i].pt[j].x * (float)(pictureBox1.Size.Width) / stFrameInfoEx2.nWidth);
                            stPointList[j].Y = (int)(stBcrResultEx2.stBcrInfoEx2[i].pt[j].y * (float)(pictureBox1.Size.Height) / stFrameInfoEx2.nHeight);
                        }
                        gra.DrawPolygon(pen, stPointList);

                        DataGridViewRow cDataRow = new DataGridViewRow();
                        cDataRow.CreateCells(dataGridView1);
                        cDataRow.Cells[0].Value = this.dataGridView1.Rows.Count;
                        DateTime cDateTime = DateTime.Now;
                        cDataRow.Cells[1].Value = cDateTime.ToString();
                        cDataRow.Cells[2].Value = stBcrResultEx2.stBcrInfoEx2[i].nTotalProcCost.ToString();
                        cDataRow.Cells[3].Value = stBcrResultEx2.stBcrInfoEx2[i].sAlgoCost.ToString();
                        cDataRow.Cells[4].Value = stBcrResultEx2.stBcrInfoEx2[i].sPPM.ToString();
                        cDataRow.Cells[5].Value = GetBarType((MvCodeReader.MV_CODEREADER_CODE_TYPE)stBcrResultEx2.stBcrInfoEx2[i].nBarType);
                        String strCode = System.Text.Encoding.Default.GetString(stBcrResultEx2.stBcrInfoEx2[i].chCode);
                        cDataRow.Cells[6].Value = String.IsNullOrEmpty(strCode) ? "NoRead" : strCode;
                        cDataRow.Cells[7].Value = stBcrResultEx2.stBcrInfoEx2[i].stCodeQuality.nOverQuality.ToString();
                        cDataRow.Cells[8].Value = stBcrResultEx2.stBcrInfoEx2[i].nIDRScore.ToString();
                        this.dataGridView1.Rows.Insert(0, cDataRow);
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
            // 清空读码信息
            while (this.dataGridView1.Rows.Count != 0)
            {
                this.dataGridView1.Rows.RemoveAt(0);
            }

            // ch:标志位置位true | en:Set position bit true
            m_hReceiveThread = new Thread(ReceiveThreadProcess);
            m_hReceiveThread.Start();
            m_bGrabbing = true;

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
            if(cbSoftTrigger.Checked)
            {
                bnTriggerExec.Enabled = true;
                cbSoftTrigger.Enabled = false;
            }
            else
            {
                bnTriggerExec.Enabled = false;
            }
        }

        private void cbSoftTrigger_CheckedChanged(object sender, EventArgs e)
        {
            if (cbSoftTrigger.Checked)
            {
                // ch:触发源设为软触发 | en:Set trigger source as Software
                m_cMyDevice.MV_CODEREADER_SetEnumValue_NET("TriggerSource", (uint)MvCodeReader.MV_CODEREADER_TRIGGER_SOURCE.MV_CODEREADER_TRIGGER_SOURCE_SOFTWARE);
                if (m_bGrabbing)
                {
                    //bnTriggerExec.Enabled = true;
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
            //bnClose.Enabled = true;
            bnTriggerExec.Enabled = false;
            if (bnTriggerMode.Checked)
            {
                cbSoftTrigger.Enabled = true;
            }
        }

        private void bnStopGrab_Click(object sender, EventArgs e)
        {
            // ch:标志位设为false | en:Set flag bit false
            m_bGrabbing = false;

            // ch:停止采集 | en:Stop Grabbing
            int nRet = m_cMyDevice.MV_CODEREADER_StopGrabbing_NET();
            if (nRet != MvCodeReader.MV_CODEREADER_OK)
            {
                ShowErrorMsg("Stop Grabbing Fail!" , nRet);
            }

            if (null != m_hReceiveThread)
            {
                m_hReceiveThread.Join();
            }

            // ch:控件操作 | en:Control Operation
            SetCtrlWhenStopGrab();
            bnContinuesMode.Enabled = true;
            bnTriggerMode.Enabled = true;
            
        }

        private void bnGetParam_Click(object sender, EventArgs e)
        {
            MvCodeReader.MV_CODEREADER_FLOATVALUE stParam = new MvCodeReader.MV_CODEREADER_FLOATVALUE();
            int nRet = m_cMyDevice.MV_CODEREADER_GetFloatValue_NET("ExposureTime", ref stParam);
            if (MvCodeReader.MV_CODEREADER_OK == nRet)
            {
                tbExposure.Text = stParam.fCurValue.ToString("F1");
            }
            else
            {
                ShowErrorMsg("Get ExposureTime Fail!", nRet);
            }

            nRet = m_cMyDevice.MV_CODEREADER_GetFloatValue_NET("Gain", ref stParam);
            if (MvCodeReader.MV_CODEREADER_OK == nRet)
            {
                tbGain.Text = stParam.fCurValue.ToString("F1");
            }
            else
            {
                ShowErrorMsg("Get Gain Fail!", nRet);
            }

            nRet = m_cMyDevice.MV_CODEREADER_GetFloatValue_NET("AcquisitionFrameRate", ref stParam);
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

            m_cMyDevice.MV_CODEREADER_SetEnumValue_NET("ExposureAuto", 0);
            int nRet = m_cMyDevice.MV_CODEREADER_SetFloatValue_NET("ExposureTime", float.Parse(tbExposure.Text));
            if (nRet != MvCodeReader.MV_CODEREADER_OK)
            {
                bIsSetted = false;
                ShowErrorMsg("Set Exposure Time Fail!", nRet);
            }

            m_cMyDevice.MV_CODEREADER_SetEnumValue_NET("GainAuto", 0);
            nRet = m_cMyDevice.MV_CODEREADER_SetFloatValue_NET("Gain", float.Parse(tbGain.Text));
            if (nRet != MvCodeReader.MV_CODEREADER_OK)
            {
                bIsSetted = false;
                ShowErrorMsg("Set Gain Fail!", nRet);
            }

            nRet = m_cMyDevice.MV_CODEREADER_SetFloatValue_NET("AcquisitionFrameRate", float.Parse(tbFrameRate.Text));
            if (nRet != MvCodeReader.MV_CODEREADER_OK)
            {
                bIsSetted = false;
                ShowErrorMsg("Set Frame Rate Fail!", nRet);
            }
            if (bIsSetted)
            {
                MessageBox.Show("Set Param Succeed!");
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            bnClose_Click(sender, e);
        }
    }
}
