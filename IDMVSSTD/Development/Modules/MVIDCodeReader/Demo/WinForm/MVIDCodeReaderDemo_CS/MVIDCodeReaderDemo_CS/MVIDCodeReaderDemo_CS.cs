using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MVIDCodeReaderNet;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using System.Drawing.Imaging;

namespace MVIDCodeReaderDemo_CS
{
    public partial class MVIDCodeReaderDemo_CS : Form
    {
        MVIDCodeReader.MVID_CAMERA_INFO_LIST stDevList = new MVIDCodeReader.MVID_CAMERA_INFO_LIST();    // 设备信息列表
        MVIDCodeReader.MVID_PROC_PARAM       stProcParam = new MVIDCodeReader.MVID_PROC_PARAM();        // 图像读码参数
        MVIDCodeReader.MVID_CAM_OUTPUT_INFO  stOutput = new MVIDCodeReader.MVID_CAM_OUTPUT_INFO();      // 图像输出信息

        MVIDCodeReader  MyCodeReader = new MVIDCodeReader();    // 设备句柄
        Thread          hCamProcessThreadHandle = null;         // 读码线程句柄
        bool            m_bProcess = false;                     // 设备是否正在读码
        Int32           m_nDeviceIndex = -1;                    // 选中的设备序号
        string          filepath = null;                        // 本地文件路径
        System.DateTime CurrentTime = new System.DateTime();    // 当前时间
        bool            m_bIsEnum = false;                      // 是否枚举到有效设备
        bool            m_bIsInitRes = false;                   // 是否初始化本地图片读码资源
        bool            m_bIsLoadImg = false;                   // 是否加载了本地图片
        IntPtr          m_pProcParam = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(MVIDCodeReader.MVID_PROC_PARAM)));    // 读取本地文件

        private delegate void listresult(IntPtr ptr, Int32 ErrorCode);  // 读码结果列表委托
        private listresult ListResult;                                  // 显示读码结果列表
        Int32              m_nListNum = 0;                              // 读码列表数量

        byte[]      ImageBuffer = null;         // 图像缓存
        Int32       nLastImageLen = 0;          // 上一张图像长度

        // ch:显示图像 | en:Display
        Bitmap bmp = null;
        Bitmap bmpImage = null;
        Graphics gra = null;
        Pen pen = new Pen(Color.Blue, 3);           // ch:画笔颜色 | en:Brush color
        Point[] stPointList = new Point[4];         // ch:条码位置的4个点坐标 | en:Coordinates of four points on the barcode

        // ch:算法参数 | en:Algorithm parameters
        Dictionary<String, Int32> ParamDictionary = new Dictionary<String ,Int32>();
        List<string> ParamList;     // 参数列表

        // ch:当前算法参数值 | en:The current value of algorithm parameter
        Int32 nCurrentParamValue = 0;

        // ch:获得语言版本 | en:Get language version
        int nLCID = 0;

        MVIDCodeReaderDemoRaw_CS RawForm = new MVIDCodeReaderDemoRaw_CS();

        public MVIDCodeReaderDemo_CS()
        {
            InitializeComponent();
            ListResult = ShowList;
            pictureBoxDisplay.Show();
            gra = pictureBoxDisplay.CreateGraphics();

            ParamDictionary.Add(MVIDCodeReader.KEY_BCR_ABILITY, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_BCR_APPMODE, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_BCR_DELERRFLAG, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_BCR_DFKSIZELOWERLIMIT, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_BCR_DFKSIZEUPPERLIMIT, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_BCR_DISTORTION, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_BCR_IAMGEMORPH, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_BCR_LOCBARNUM, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_BCR_LOCWINSIZE, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_BCR_MAX_HEIGHT, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_BCR_MAX_WIDTH, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_BCR_ROI_HEIGHT, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_BCR_ROI_WIDTH, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_BCR_ROI_X, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_BCR_ROI_Y, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_BCR_SAMPLELEVEL, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_BCR_SAVEIMAGELEVEL, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_BCR_SEGQUIETW, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_BCR_SPOT, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_BCR_WAITINGTIME, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_BCR_WHITEGAP, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_TDCR_ABILITY, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_TDCR_ADVANCEPARAM, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_TDCR_ADVANCEPARAM2, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_TDCR_APPMODE, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_TDCR_CODECOLOR, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_TDCR_DEBUGFLAG, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_TDCR_DISCRETEFLAG, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_TDCR_DISTORTIONFLAG, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_TDCR_LOCCODENUM, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_TDCR_MAX_HEIGHT, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_TDCR_MAX_WIDTH, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_TDCR_MAXBARSIZE, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_TDCR_MINBARSIZE, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_TDCR_MIRRORMODE, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_TDCR_RECTANGLE, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_TDCR_ROI_HEIGHT, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_TDCR_ROI_WIDTH, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_TDCR_ROI_X, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_TDCR_ROI_Y, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_TDCR_SAMPLELEVEL, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_TDCR_WAITINGTIME, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_WAYBILL_ABILITY, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_WAYBILL_BINARYADAPTIVE, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_WAYBILL_BOUNDARYCOL, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_WAYBILL_BOUNDARYROW, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_WAYBILL_CODEBOUNDARYCOL, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_WAYBILL_CODEBOUNDARYROW, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_WAYBILL_ENHANCECLIPRATIOHIGH, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_WAYBILL_ENHANCECLIPRATIOLOW, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_WAYBILL_ENHANCECONTRASTFACTOR, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_WAYBILL_ENHANCEMETHOD, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_WAYBILL_ENHANCESHARPENFACTOR, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_WAYBILL_GRAYHIGH, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_WAYBILL_GRAYLOW, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_WAYBILL_GRAYMID, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_WAYBILL_JPGQUALITY, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_WAYBILL_MAX_HEIGHT, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_WAYBILL_MAX_WIDTH, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_WAYBILL_MAXBILLBARHEIGTHRATIO, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_WAYBILL_MAXBILLBARWIDTHRATIO, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_WAYBILL_MAXHEIGHT, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_WAYBILL_MAXWIDTH, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_WAYBILL_MINBILLBARHEIGTHRATIO, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_WAYBILL_MINBILLBARWIDTHRATIO, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_WAYBILL_MINHEIGHT, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_WAYBILL_MINWIDTH, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_WAYBILL_MORPHTIMES, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_WAYBILL_OUTPUTIMAGETYPE, 0);
            ParamDictionary.Add(MVIDCodeReader.KEY_WAYBILL_SHARPENKERNELSIZE, 0);

            ParamList = new List<string>(ParamDictionary.Keys);

            nLCID = System.Globalization.CultureInfo.CurrentCulture.LCID;

            EnableControl();
        }

        // 控件使能
        private void EnableControl()
        {
            ButtonEnum.Enabled          = !m_bProcess;
            ButtonStart.Enabled         = !m_bProcess && m_bIsEnum;
            ButtonStop.Enabled          = m_bProcess;
            buttonSetParam.Enabled      = m_bProcess;
            buttonGetParam.Enabled      = m_bProcess;
            textBoxExposure.Enabled     = m_bProcess;
            textBoxGain.Enabled         = m_bProcess;
            textBoxFrameRate.Enabled    = m_bProcess;

            ButtonInit.Enabled      = !m_bProcess && !m_bIsInitRes;
            ButtonLoad.Enabled      = !m_bProcess && m_bIsInitRes;
            ButtonProc.Enabled      = !m_bProcess && m_bIsLoadImg;
            ButtonCutout.Enabled    = !m_bProcess && m_bIsLoadImg;

            ButtonClean.Enabled = true;
        }

        // 搜索相机按钮
        private void ButtonEnum_Click(object sender, EventArgs e)
        {
            DeviceListAcq();
            EnableControl();
        }

        // 枚举相机
        private void DeviceListAcq()
        {
            Int32 nRet = MVIDCodeReader.MVID_CR_OK;
            System.GC.Collect();
            ComboBoxCamList.Items.Clear();
            ComboBoxCamList.Text = "";
            ComboBoxCamList.SelectedIndex = -1;

            // ch:枚举相机 | en:Enumerate cameras
            nRet = MVIDCodeReader.MVID_CR_CAM_EnumDevices_NET(ref stDevList);
            if (MVIDCodeReader.MVID_CR_OK != nRet)
            {
                string csMessage;
                csMessage = "enum device failed 0x" + String.Format("{0:X}", nRet);
                MessageBox.Show(csMessage);
                return;
            }

            MVIDCodeReader.MVID_CAMERA_INFO stDevInfo;      // ch:通用设备信息 | en:General device information
            for (int i = 0; i < stDevList.nDeviceNum; i++)
            {
                stDevInfo = (MVIDCodeReader.MVID_CAMERA_INFO)Marshal.PtrToStructure(stDevList.pstCamInfo[i], typeof(MVIDCodeReader.MVID_CAMERA_INFO));

                if (MVIDCodeReader.MVID_GIGE_CAM == stDevInfo.nCamType)
                {
                    if ('\0' != stDevInfo.chUserDefinedName[0])
                    {
                        ComboBoxCamList.Items.Add("[" + i + "] " + "GigE: " + Encoding.UTF8.GetString(stDevInfo.chUserDefinedName).TrimEnd('\0') + " (" + stDevInfo.chSerialNumber + ")");
                    }
                    else
                    {
                        ComboBoxCamList.Items.Add("[" + i + "] " + "GigE: " + stDevInfo.chManufacturerName + " " + stDevInfo.chModelName + " (" + stDevInfo.chSerialNumber + ")");
                    }
                }
                else if (MVIDCodeReader.MVID_USB_CAM == stDevInfo.nCamType)
                {
                    if ('\0' != stDevInfo.chUserDefinedName[0])
                    {
                        ComboBoxCamList.Items.Add("[" + i + "] " + "USB: " + Encoding.UTF8.GetString(stDevInfo.chUserDefinedName).TrimEnd('\0') + " (" + stDevInfo.chSerialNumber + ")");
                    }
                    else
                    {
                        ComboBoxCamList.Items.Add("[" + i + "] " + "USB: " + stDevInfo.chManufacturerName + " " + stDevInfo.chModelName + " (" + stDevInfo.chSerialNumber + ")");
                    }
                }
            }
            // ch:选择第一项 | en:Select the first item
            if (stDevList.nDeviceNum != 0)
            {
                ComboBoxCamList.SelectedIndex = 0;
                m_bIsEnum = true;
            }
            else
            {
                m_bIsEnum = false;
                MessageBox.Show("Find no device");
            }
        }

        // 开始读码
        private void ButtonStart_Click(object sender, EventArgs e)
        {
            if (stDevList.nDeviceNum == 0 || m_nDeviceIndex == -1)
            {
                MessageBox.Show("No device, please select");
                return;
            }

            int nIndex = m_nDeviceIndex;

            if (nIndex >= MVIDCodeReader.MVID_MAX_CAM_NUM)
            {
                MessageBox.Show("device num beyond limit");
                return;
            }

            // ch:由设备信息创建设备实例 | en:Device instance created by device information
            if (null == stDevList.pstCamInfo[nIndex])
            {
                MessageBox.Show("Device does not exist");
                return;
            }

            MVIDCodeReader.MVID_CAMERA_INFO stDevInfo;                            // ch:通用设备信息 | en:General device information
            stDevInfo = (MVIDCodeReader.MVID_CAMERA_INFO)Marshal.PtrToStructure(stDevList.pstCamInfo[nIndex], typeof(MVIDCodeReader.MVID_CAMERA_INFO));

            // ch:仅一维码识别：MVID_BCR | en:Recognize barcode only：MVID_BCR
            // ch:仅二维码识别：MVID_TDCR | en:Recognize Two-Dimension code only: MVID_TDCR
            // ch:一维码 + 二维码 识别：MVID_BCR | MVID_TDCR | en:Recognize Barcode + Two-Dimension code: MVID_BCR | MVID_TDCR
            int nRet = MyCodeReader.MVID_CR_CreateHandle_NET(MVIDCodeReader.MVID_BCR | MVIDCodeReader.MVID_TDCR);
            if (MVIDCodeReader.MVID_CR_OK != nRet)
            {
                string csMessage;
                csMessage = "MVID_CR_CreateHandle failed: 0x" + String.Format("{0:X}", nRet);
                MessageBox.Show(csMessage);
                return;
            }

            nRet = MyCodeReader.MVID_CR_CAM_BindDevice_NET(stDevList.pstCamInfo[nIndex]);
            if (MVIDCodeReader.MVID_CR_OK != nRet)
            {
                string csMessage;
                csMessage = "MVID_CR_CAM_BindDevice failed: 0x" + String.Format("{0:X}", nRet);
                MessageBox.Show(csMessage);
                return;
            }

            MVIDCodeReader.MVID_CAM_INTVALUE_EX nIntValue = new MVIDCodeReader.MVID_CAM_INTVALUE_EX();
            nRet = MyCodeReader.MVID_CR_CAM_GetIntValue_NET("Width", ref nIntValue);
            if (MVIDCodeReader.MVID_CR_OK != nRet)
            {
                string csMessage;
                csMessage = "MVID_CR_CAM_GetIntValue failed: 0x" + String.Format("{0:X}", nRet);
                MessageBox.Show(csMessage);
                return;
            }
            Int32 nWidth = (Int32)nIntValue.nCurValue;

            nRet = MyCodeReader.MVID_CR_CAM_GetIntValue_NET("Height", ref nIntValue);
            if (MVIDCodeReader.MVID_CR_OK != nRet)
            {
                string csMessage;
                csMessage = "MVID_CR_CAM_GetIntValue failed: 0x" + String.Format("{0:X}", nRet);
                MessageBox.Show(csMessage);
                return;
            }
            Int32 nHeight = (Int32)nIntValue.nCurValue;

            System.GC.Collect();

            if (nLastImageLen > nWidth * nHeight * 3 + 4096)
            {
                ImageBuffer = new byte[nLastImageLen];
            }
            else
            {
                nLastImageLen = nWidth * nHeight * 3 + 4096;
                ImageBuffer = new byte[nLastImageLen];
            }

            // 获取相机参数
            buttonGetParam_Click(sender, e);

            nRet = MyCodeReader.MVID_CR_CAM_StartGrabbing_NET();
            if (MVIDCodeReader.MVID_CR_OK != nRet)
            {
                string csMessage;
                csMessage = "MVID_CR_CAM_StartGrabbing failed: 0x" + String.Format("{0:X}", nRet);
                MessageBox.Show(csMessage);
                return;
            }

            UpdatelParamList();

            m_bProcess = true;
            hCamProcessThreadHandle = new Thread(CamProcessWorkThread);
            hCamProcessThreadHandle.Start();

            m_bIsInitRes = false;
            m_bIsLoadImg = false;
            EnableControl();
        }

        private void ComboBoxCamList_SelectedIndexChanged(object sender, EventArgs e)
        {
            m_nDeviceIndex = ComboBoxCamList.SelectedIndex;
        }

        // 读码线程
        public void CamProcessWorkThread(object obj)
        {
            Int32 nRet = MVIDCodeReader.MVID_CR_OK;
            IntPtr ptr = IntPtr.Zero;
            Int32 size = Marshal.SizeOf(typeof(MVIDCodeReader.MVID_CAM_OUTPUT_INFO));
            ptr = Marshal.AllocHGlobal(size);

            while (m_bProcess)
            {
                try
                {
                    nRet = MyCodeReader.MVID_CR_CAM_GetOneFrameTimeout_NET(ptr, 1000);
                    listBoxResult.Invoke(ListResult, new object[] { ptr, nRet });
                }
                catch
                {
                    continue;
                }
            }

            Marshal.FreeHGlobal(ptr);
        }

        // 停止读码
        private void ButtonStop_Click(object sender, EventArgs e)
        {
            string csMessage;
            m_bProcess = false;
            if (null != hCamProcessThreadHandle)
            {
                hCamProcessThreadHandle.Abort(1000);
            }

            int nRet = MyCodeReader.MVID_CR_CAM_StopGrabbing_NET();
            if (MVIDCodeReader.MVID_CR_OK != nRet)
            {
                csMessage = "stop grabbing failed: 0x" + String.Format("{0:X}", nRet);
                MessageBox.Show(csMessage);
            }
            else
            {
                csMessage = "stop grabbing success";
                MessageBox.Show(csMessage);
            }

            nRet = MyCodeReader.MVID_CR_DestroyHandle_NET();
            if (MVIDCodeReader.MVID_CR_OK != nRet)
            {
                csMessage = "destroy handle failed: 0x" + String.Format("{0:X}", nRet);
                MessageBox.Show(csMessage);
            }

            // 清空相机参数、算法参数、显示图像
            textBoxExposure.Text = "";
            textBoxGain.Text = "";
            textBoxFrameRate.Text = "";
            dataGridViewAlgorithm.Rows.Clear();
            if (null != pictureBoxDisplay.Image)
            {
                pictureBoxDisplay.Image.Dispose();
                pictureBoxDisplay.Image = null;
            }
            // 清空条码框
            stOutput.stCodeList.nCodeNum = 0;
            pictureBoxDisplay.Refresh();

            EnableControl();
        }

        // 加载本地图片
        private void ButtonLoad_Click(object sender, EventArgs e)
        {
            if (m_bProcess)
            {
                string csMessage;
                csMessage = "please stop camera barcode identify first";
                MessageBox.Show(csMessage);
                return;
            }

            System.GC.Collect();

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;                          //ch:该值确定是否可以选择多个文件 | en:This value determines whether it supports select multiple files

            if(0x0804 == nLCID)
            {
                dialog.Title = "请选择文件夹";
                dialog.Filter = "图片文件 (*.raw;*.RAW;*.bmp;*.BMP;*.jpg;*.JPG;*.jpeg;*.JPEG)|*.raw;*.RAW;*.bmp;*.BMP;*.jpg;*.JPG;*.jpeg;*.JPEG";
            }
            else
            {
                dialog.Title = "Select folder";
                dialog.Filter = "Picture Files (*.raw;*.RAW;*.bmp;*.BMP;*.jpg;*.JPG;*.jpeg;*.JPEG)|*.raw;*.RAW;*.bmp;*.BMP;*.jpg;*.JPG;*.jpeg;*.JPEG";
            }

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                filepath = dialog.FileName;
                TextFilePath.Text = filepath;
            }

            if (null == filepath)
            {
                return;
            }

            string strExtension = Path.GetExtension(filepath);
            if (".raw" == strExtension || ".RAW" == strExtension)
            {
                RawForm.ShowDialog();

                stProcParam.nWidth = UInt16.Parse(RawForm.textBoxImageWidth.Text);
                stProcParam.nHeight = UInt16.Parse(RawForm.textBoxImageHeight.Text);

                switch (RawForm.comboBoxConvertType.SelectedIndex)
                {
                    case 0:
                        stProcParam.enImageType = MVIDCodeReader.MVID_IMAGE_TYPE.MVID_IMAGE_MONO8;
                        break;
                    case 1:
                        stProcParam.enImageType = MVIDCodeReader.MVID_IMAGE_TYPE.MVID_IMAGE_BGR24;
                        break;
                    default:
                        return;
                }

                Marshal.StructureToPtr(stProcParam, m_pProcParam, true);
            }

            int nRet = MyCodeReader.MVID_CR_GetImageFileData_NET(filepath, m_pProcParam);
            if (MVIDCodeReader.MVID_CR_OK != nRet)
            {
                MessageBox.Show("MVID_CR_GetImageFileData failed: 0x" + String.Format("{0:X}", nRet));
                return;
            }

            stProcParam = (MVIDCodeReader.MVID_PROC_PARAM)Marshal.PtrToStructure(m_pProcParam, typeof(MVIDCodeReader.MVID_PROC_PARAM));
            if (nLastImageLen < stProcParam.nImageLen)
            {
                nLastImageLen = (int)stProcParam.nImageLen;
                ImageBuffer = new byte[nLastImageLen];
            }

            m_bIsLoadImg = true;
            EnableControl();
        }

        // 初始化资源
        private void ButtonInit_Click(object sender, EventArgs e)
        {
            if (m_bProcess)
            {
                string csMessage = "please stop camera barcode identify first";
                MessageBox.Show(csMessage);
                return;
            }

            IntPtr pVersion = Marshal.AllocHGlobal(1024);
            int nRet = MVIDCodeReader.MVID_CR_GetVersion_NET(pVersion);
            if (MVIDCodeReader.MVID_CR_OK != nRet)
            {
                Marshal.FreeHGlobal(pVersion);
                pVersion = IntPtr.Zero;

                string csMessage = "MVID_CR_GetVersion failed: 0x" + String.Format("{0:X}", nRet);
                MessageBox.Show(csMessage);
                return;
            }

            byte[] strVersion = new byte[1024];
            Marshal.Copy(pVersion, strVersion, 0, 1024);
            Marshal.FreeHGlobal(pVersion);
            pVersion = IntPtr.Zero;

            String strVersionGet = System.Text.Encoding.Default.GetString(strVersion, 0, strVersion.Length);
            listBoxResult.Items.Add(strVersionGet);
            listBoxResult.SelectedIndex = listBoxResult.Items.Count - 1;

            nRet = MyCodeReader.MVID_CR_CreateHandle_NET(MVIDCodeReader.MVID_BCR | MVIDCodeReader.MVID_TDCR | MVIDCodeReader.MVID_WAYBILL);
            if (MVIDCodeReader.MVID_CR_OK != nRet)
            {
                string csMessage = "MVID_CR_CreateHandle failed: 0x" + String.Format("{0:X}", nRet);
                MessageBox.Show(csMessage);
                return;
            }

            m_bIsInitRes = true;
            m_bIsLoadImg = false;
            EnableControl();
            UpdatelParamList();
        }

        // 本地图片读码
        private void ButtonProc_Click(object sender, EventArgs e)
        {
            if (m_bProcess)
            {
                string csMessage = "please stop camera barcode identify first";
                MessageBox.Show(csMessage);
                return;
            }

            Int32 nRet = MVIDCodeReader.MVID_CR_OK;
            IntPtr ptr = IntPtr.Zero;
            Int32 size = Marshal.SizeOf(typeof(MVIDCodeReader.MVID_PROC_PARAM));
            ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(stProcParam, ptr, true);
            nRet = MyCodeReader.MVID_CR_Process_NET(ptr, MVIDCodeReader.MVID_BCR | MVIDCodeReader.MVID_TDCR);
            if (MVIDCodeReader.MVID_CR_OK == nRet)
            {
                stProcParam = (MVIDCodeReader.MVID_PROC_PARAM)Marshal.PtrToStructure(ptr, typeof(MVIDCodeReader.MVID_PROC_PARAM));

                // ch:输出结果 | en:Output results
                if (0x0804 == nLCID)
                {
                    listBoxResult.Items.Add("已识别 " + stProcParam.stCodeList.nCodeNum + "个对象：");
                    for (int i = 0; i < stProcParam.stCodeList.nCodeNum; ++i)
                    {
                        listBoxResult.Items.Add("第" + i.ToString() + "个：[" + stProcParam.stCodeList.stCodeInfo[i].strCode + "], 码类型[" +
                        Convert.ToInt32(stProcParam.stCodeList.stCodeInfo[i].enBarType) + "], 是否被过滤[" +
                        stProcParam.stCodeList.stCodeInfo[i].nFilterFlag + "]");
                    }
                }
                else
                {
                    listBoxResult.Items.Add("Recognized" + stProcParam.stCodeList.nCodeNum + "Objects:");
                    for (int i = 0; i < stProcParam.stCodeList.nCodeNum; ++i)
                    {
                        listBoxResult.Items.Add("No." + i.ToString() + ": [" + stProcParam.stCodeList.stCodeInfo[i].strCode + "], code type[" +
                        Convert.ToInt32(stProcParam.stCodeList.stCodeInfo[i].enBarType) + "], filtered or not[" +
                        stProcParam.stCodeList.stCodeInfo[i].nFilterFlag + "]");
                    }
                }
            }
            else
            {
                string csMessage;
                csMessage = "identify barcode failed: 0x" + String.Format("{0:X}", nRet);
                listBoxResult.Items.Add(csMessage);

                stProcParam.stCodeList.nCodeNum = 0;
            }
            listBoxResult.SelectedIndex = listBoxResult.Items.Count - 1;

            stOutput.stImage.pImageBuf = stProcParam.pImageBuf;
            stOutput.stImage.nImageLen = stProcParam.nImageLen;
            stOutput.stImage.nWidth = stProcParam.nWidth;
            stOutput.stImage.nHeight = stProcParam.nHeight;
            stOutput.stImage.enImageType = stProcParam.enImageType;
            stOutput.stCodeList = stProcParam.stCodeList;

            Display(ref stOutput);
            pictureBoxDisplay.Invalidate();

            m_nListNum = listBoxResult.Items.Count;
            CheckListNum();

            Marshal.FreeHGlobal(ptr);
        }

        // 面单抠图
        private void ButtonCutout_Click(object sender, EventArgs e)
        {
            if (m_bProcess)
            {
                string csMessage;
                csMessage = "please stop camera barcode identify first";
                MessageBox.Show(csMessage);
                return;
            }

            Int32 nRet = MVIDCodeReader.MVID_CR_OK;
            IntPtr ptr = IntPtr.Zero;
            Int32 size = Marshal.SizeOf(typeof(MVIDCodeReader.MVID_PROC_PARAM));
            ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(stProcParam, ptr, true);
            Int32 nImageType = (Int32)MVIDCodeReader.MVID_IMAGE_TYPE.MVID_IMAGE_JPEG;

            // ch:目前只支持一维码的抠图 | en:Currently, only supports matting of bar code image
            nRet = MyCodeReader.MVID_CR_Process_NET(ptr, MVIDCodeReader.MVID_BCR | MVIDCodeReader.MVID_WAYBILL);
            if (MVIDCodeReader.MVID_CR_OK == nRet)
            {
                stProcParam = (MVIDCodeReader.MVID_PROC_PARAM)Marshal.PtrToStructure(ptr, typeof(MVIDCodeReader.MVID_PROC_PARAM));

                // ch:未识别到条码时不进行抠图 | en:Do not execute image matting if no barcode is recognized
                if (0 == stProcParam.nImageWaybillLen || null == stProcParam.pImageWaybill)
                {
                    MessageBox.Show("identify no barcode, waybill image is null");
                    return;
                }

                CurrentTime = System.DateTime.Now;

                nRet = MyCodeReader.MVID_CR_Algorithm_GetIntValue_NET(MVIDCodeReader.KEY_WAYBILL_OUTPUTIMAGETYPE, ref nImageType);
                if (MVIDCodeReader.MVID_CR_OK != nRet)
                {
                    string csMessage = "MVID_CR_Algorithm_GetIntValue_NET failed: 0x" + String.Format("{0:X}", nRet);
                    MessageBox.Show(csMessage);

                    Marshal.FreeHGlobal(ptr);
                    ptr = IntPtr.Zero;
                    return;
                }

                String FilePath = null;
                if ((Int32)MVIDCodeReader.MVID_IMAGE_TYPE.MVID_IMAGE_MONO8 == nImageType)
                {
                    FilePath = CurrentTime.ToString("yyyyMMddHHmmss") + ".raw";
                }
                else
                {
                    FilePath = CurrentTime.ToString("yyyyMMddHHmmss") + ".jpg";
                }

                // ch:保存图像 | en:Save image
                FileStream fs = new FileStream(FilePath, FileMode.Create);
                byte[] Jpgfile = new byte[stProcParam.nImageWaybillLen];
                Marshal.Copy(stProcParam.pImageWaybill, Jpgfile, 0, stProcParam.nImageWaybillLen);
                fs.Write(Jpgfile, 0, Jpgfile.Length);
                fs.Flush();
                fs.Close();
                MessageBox.Show("success save cutout image");
            }
            else
            {
                string csMessage;
                csMessage = "waybill process failed: 0x" + String.Format("{0:X}", nRet);
                MessageBox.Show(csMessage);
            }

            Marshal.FreeHGlobal(ptr);
            ptr = IntPtr.Zero;
        }

        // 清空读码消息列表
        private void ButtonClean_Click(object sender, EventArgs e)
        {
            listBoxResult.Items.Clear();
        }

        // 显示图片
        private void Display(ref MVIDCodeReader.MVID_CAM_OUTPUT_INFO stImageInfo)
        {
            if (stImageInfo.stImage.nWidth == 0 || stImageInfo.stImage.nHeight == 0)
            {
                return;
            }

            // ch:绘制图像 | en:Draw image
            if (MVIDCodeReader.MVID_IMAGE_TYPE.MVID_IMAGE_BMP == stImageInfo.stImage.enImageType)
            {
                pictureBoxDisplay.Image = (Image)bmpImage;
            }
            else if (MVIDCodeReader.MVID_IMAGE_TYPE.MVID_IMAGE_JPEG == stImageInfo.stImage.enImageType)
            {
                System.Drawing.Image img = System.Drawing.Image.FromFile(filepath);//双引号里是图片的路径
                pictureBoxDisplay.Image = img;
            }
            else
            {
                GCHandle handle = GCHandle.Alloc(ImageBuffer, GCHandleType.Pinned);
                Marshal.Copy(stImageInfo.stImage.pImageBuf, ImageBuffer, 0, (int)stImageInfo.stImage.nImageLen);
                IntPtr pImage = handle.AddrOfPinnedObject();

                if (MVIDCodeReader.MVID_IMAGE_TYPE.MVID_IMAGE_MONO8 == stImageInfo.stImage.enImageType)
                {
                    bmp = new Bitmap(stImageInfo.stImage.nWidth, stImageInfo.stImage.nHeight, stImageInfo.stImage.nWidth, PixelFormat.Format8bppIndexed, pImage);
                    ColorPalette cp = bmp.Palette;
                    for (int i = 0; i < 256; i++)
                    {
                        cp.Entries[i] = Color.FromArgb(i, i, i);
                    }
                    bmp.Palette = cp;
                }
                else
                {
                    bmp = new Bitmap(stImageInfo.stImage.nWidth, stImageInfo.stImage.nHeight, stImageInfo.stImage.nWidth * 3, PixelFormat.Format24bppRgb, pImage);
                }
                pictureBoxDisplay.Image = (Image)bmp;

                if (handle.IsAllocated)
                {
                    handle.Free();
                }
            }
        }

        // 更新算法参数列表
        private void UpdatelParamList()
        {
            int nParam = 0;
            for (int i = 0; i < ParamDictionary.Count; i++)
            {
                int nRet = MyCodeReader.MVID_CR_Algorithm_GetIntValue_NET(ParamList[i].ToString(), ref nParam);
                if (MVIDCodeReader.MVID_CR_OK != nRet)
                {
                    nParam = 0;
                }
                int index = this.dataGridViewAlgorithm.Rows.Add();
                dataGridViewAlgorithm.Rows[i].Cells[0].Value = ParamList[i];
                dataGridViewAlgorithm.Rows[i].Cells[1].Value = nParam.ToString();
            }
        }

        private void MVIDCodeReaderDemo_CS_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_bProcess = false;
            if (null != hCamProcessThreadHandle)
            {
                hCamProcessThreadHandle.Abort(1000);
            }

            int nRet = MyCodeReader.MVID_CR_DestroyHandle_NET();
            if (MVIDCodeReader.MVID_CR_OK != nRet)
            {
                string csMessage;
                csMessage = "MVID_CR_DestroyHandle_NET failed: 0x" + String.Format("{0:X}", nRet);
                listBoxResult.Items.Add(csMessage);
                listBoxResult.SelectedIndex = listBoxResult.Items.Count - 1;
            }

            if (IntPtr.Zero != m_pProcParam)
            {
                Marshal.FreeHGlobal(m_pProcParam);
                m_pProcParam = IntPtr.Zero;
            }
        }

        private void pictureBoxDisplay_Paint(object sender, PaintEventArgs e)
        {
            // ch:绘制结果 | en:Drawing results
            for (int i = 0; i < stOutput.stCodeList.nCodeNum; ++i)
            {
                // ch:绘制矩形框 | en:Draw ractangle frame
                for (int j = 0; j < 4; ++j)
                {
                    stPointList[j].X = (int)(stOutput.stCodeList.stCodeInfo[i].stCornerPt[j].nX * (float)(pictureBoxDisplay.Size.Width) / stOutput.stImage.nWidth);
                    stPointList[j].Y = (int)(stOutput.stCodeList.stCodeInfo[i].stCornerPt[j].nY * (float)(pictureBoxDisplay.Size.Height) / stOutput.stImage.nHeight);
                }
                e.Graphics.DrawPolygon(pen, stPointList);
            }
        }

        private void dataGridViewAlgorithm_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            nCurrentParamValue = Convert.ToInt32(dataGridViewAlgorithm.CurrentCell.Value.ToString());
        }

        private void dataGridViewAlgorithm_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            int nRet = MyCodeReader.MVID_CR_Algorithm_SetIntValue_NET(ParamList[dataGridViewAlgorithm.CurrentCell.RowIndex], Convert.ToInt32(dataGridViewAlgorithm.CurrentCell.Value.ToString()));
            if (MVIDCodeReader.MVID_CR_OK != nRet)
            {
                dataGridViewAlgorithm.CurrentCell.Value = nCurrentParamValue.ToString();
                string csMessage;
                csMessage = "MVID_CR_Algorithm_SetIntValue_NET failed: 0x" + String.Format("{0:X}", nRet);
                MessageBox.Show(csMessage);
            }
        }

        // 相机参数获取
        private void buttonGetParam_Click(object sender, EventArgs e)
        {
            string csMessage;
            MVIDCodeReader.MVID_CAM_FLOATVALUE stParam = new MVIDCodeReader.MVID_CAM_FLOATVALUE();
            int nRet = MyCodeReader.MVID_CR_CAM_GetFloatValue_NET("ExposureTime", ref stParam);
            if (MVIDCodeReader.MVID_CR_OK == nRet)
            {
                textBoxExposure.Text = stParam.fCurValue.ToString("F1");
            }
            else
            {
                csMessage = "MVID_CR_CAM_GetFloatValue_NET failed: 0x" + String.Format("{0:X}", nRet);
                MessageBox.Show(csMessage);
                return;
            }

            nRet = MyCodeReader.MVID_CR_CAM_GetFloatValue_NET("Gain", ref stParam);
            if (MVIDCodeReader.MVID_CR_OK == nRet)
            {
                textBoxGain.Text = stParam.fCurValue.ToString("F1");
            }
            else
            {
                csMessage = "MVID_CR_CAM_GetFloatValue_NET failed: 0x" + String.Format("{0:X}", nRet);
                MessageBox.Show(csMessage);
                return;
            }

            nRet = MyCodeReader.MVID_CR_CAM_GetFloatValue_NET("ResultingFrameRate", ref stParam);
            if (MVIDCodeReader.MVID_CR_OK == nRet)
            {
                textBoxFrameRate.Text = stParam.fCurValue.ToString("F1");
            }
            else
            {
                csMessage = "MVID_CR_CAM_GetFloatValue_NET failed: 0x" + String.Format("{0:X}", nRet);
                MessageBox.Show(csMessage);
                return;
            }
        }

        // 相机参数设置
        private void buttonSetParam_Click(object sender, EventArgs e)
        {
            int nRet = MVIDCodeReader.MVID_CR_OK;
            MyCodeReader.MVID_CR_CAM_SetEnumValue_NET("ExposureAuto", 0);

            try
            {
                float.Parse(textBoxExposure.Text);
                float.Parse(textBoxGain.Text);
                float.Parse(textBoxFrameRate.Text);
            }
            catch
            {
                MessageBox.Show("Please enter correct type", "PROMPT");
                return;
            }

            nRet = MyCodeReader.MVID_CR_CAM_SetFloatValue_NET("ExposureTime", float.Parse(textBoxExposure.Text));
            if (MVIDCodeReader.MVID_CR_OK != nRet)
            {
                MessageBox.Show("Set Exposure Time Fail!", "PROMPT");
            }

            MyCodeReader.MVID_CR_CAM_SetEnumValue_NET("GainAuto", 0);
            nRet = MyCodeReader.MVID_CR_CAM_SetFloatValue_NET("Gain", float.Parse(textBoxGain.Text));
            if (MVIDCodeReader.MVID_CR_OK != nRet)
            {
                MessageBox.Show("Set Gain Fail!", "PROMPT");
            }

            nRet = MyCodeReader.MVID_CR_CAM_SetFloatValue_NET("AcquisitionFrameRate", float.Parse(textBoxFrameRate.Text));
            if (MVIDCodeReader.MVID_CR_OK != nRet)
            {
                MessageBox.Show("Set Frame Rate Fail!", "PROMPT");
            }
        }

        // 展示读码信息列表
        private void ShowList(IntPtr ptr, Int32 ErrorCode)
        {
            if (MVIDCodeReader.MVID_CR_OK == ErrorCode)
            {
                if (0x0804 == nLCID)
                {
                    stOutput = (MVIDCodeReader.MVID_CAM_OUTPUT_INFO)Marshal.PtrToStructure(ptr, typeof(MVIDCodeReader.MVID_CAM_OUTPUT_INFO));
                    listBoxResult.Items.Add("已识别" + stOutput.stCodeList.nCodeNum + "个对象：");
                    for (int i = 0; i < stOutput.stCodeList.nCodeNum; ++i)
                    {
                        listBoxResult.Items.Add("第" + i.ToString() + "个：[" + stOutput.stCodeList.stCodeInfo[i].strCode + "], 码类型[" +
                        Convert.ToInt32(stOutput.stCodeList.stCodeInfo[i].enBarType) + "], 是否被过滤[" +
                        stOutput.stCodeList.stCodeInfo[i].nFilterFlag + "], 帧号[" + stOutput.stImage.nFrameNum + "]");
                    }
                }
                else
                {
                    stOutput = (MVIDCodeReader.MVID_CAM_OUTPUT_INFO)Marshal.PtrToStructure(ptr, typeof(MVIDCodeReader.MVID_CAM_OUTPUT_INFO));
                    listBoxResult.Items.Add("Recognized" + stOutput.stCodeList.nCodeNum + "Objects:");
                    for (int i = 0; i < stOutput.stCodeList.nCodeNum; ++i)
                    {
                        listBoxResult.Items.Add("No." + i.ToString() + ":[" + stOutput.stCodeList.stCodeInfo[i].strCode + "], code type[" +
                        Convert.ToInt32(stOutput.stCodeList.stCodeInfo[i].enBarType) + "], filtered or not[" +
                        stOutput.stCodeList.stCodeInfo[i].nFilterFlag + "], frame No.[" + stOutput.stImage.nFrameNum + "]");
                    }
                }
            }
            else
            {
                string csMessage;
                csMessage = "identify barcode failed: 0x" + String.Format("{0:X}", ErrorCode);
                listBoxResult.Items.Add(csMessage);

                stOutput.stCodeList.nCodeNum = 0;
            }
            listBoxResult.SelectedIndex = listBoxResult.Items.Count - 1;
            m_nListNum = listBoxResult.Items.Count;
            CheckListNum();

            Display(ref stOutput);
            pictureBoxDisplay.Invalidate();
        }

        private void CheckListNum()
        {
            // ch:对ListBox行数增加限制，避免内存一直上涨 | en:Number limit of ListBox lines, which avoids the continuous increase of memory
            if (100 < m_nListNum)
            {
                listBoxResult.Items.Clear();
            }
        }

        private void pictureBoxDisplay_DoubleClick(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Maximized)
            {
                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.WindowState = FormWindowState.Normal;
                this.pictureBoxDisplay.Dock = DockStyle.None;
                this.pictureBoxDisplay.Refresh();
                this.Refresh();
            }
            else
            {
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;
                this.pictureBoxDisplay.Dock = DockStyle.Fill;
                this.pictureBoxDisplay.BringToFront();
                this.pictureBoxDisplay.Refresh();
                this.Refresh();
            }
        }
    }
}
