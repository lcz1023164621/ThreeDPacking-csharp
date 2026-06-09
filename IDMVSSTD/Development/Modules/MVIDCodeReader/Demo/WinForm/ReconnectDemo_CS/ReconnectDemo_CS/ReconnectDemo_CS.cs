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

namespace ReconnectDemo_CS
{
    public partial class ReconnectDemo_CS : Form
    {
        MVIDCodeReader.MVID_CAMERA_INFO_LIST    stDevList = new MVIDCodeReader.MVID_CAMERA_INFO_LIST(); // 设备信息列表
        MVIDCodeReader.MVID_CAM_OUTPUT_INFO     stOutput = new MVIDCodeReader.MVID_CAM_OUTPUT_INFO();   // 图像输出信息
        MVIDCodeReader                          MyCodeReader = new MVIDCodeReader();                    // 设备句柄

        MVIDCodeReader.cbOutputdelegate     ImageCallback;      // 图像数据回调
        MVIDCodeReader.cbExceptiondelegate  ExceptionCallBack;  // 异常回调

        bool    bProcess = false;       // 设备是否正在读码
        Int32   m_nDeviceIndex = -1;    // 选中的设备序号
        Int32   m_nListNum = 0;         // 读码列表数量

        private delegate void listresult(IntPtr ptr);   // 读码结果列表委托
        private listresult ListResult;                  // 显示读码结果列表

        // ch:获得语言版本 | en:Get language version
        int nLCID = 0;

        // 图像处理函数
        public void ImageCallbackFunc(IntPtr pstOutput, IntPtr pUser)
        {
            if (bProcess && null != pstOutput)
            {
                listBoxResult.Invoke(ListResult, new object[] { pstOutput });
            }
        }

        // 异常处理函数
        public void ExceptionCallBackFunc(UInt32 nMsgType, IntPtr pUser)
        {
            bool bConnect = false;
            int nReConnectTime = 0;
            if (bProcess)
            {
                if (nMsgType == MVIDCodeReader.MVID_EXCEPTION_DEV_DISCONNECT)
                {
                    while (nReConnectTime < 10 && bConnect == false)
                    {
                        // ch:每500ms重连一次相机 | en:Reconnect to camera per 500 ms
                        Thread.Sleep(500);
                        int nReConnectTimeTemp = nReConnectTime + 1;

                        if (0x0804 == nLCID)
                        {
                            listBoxResult.Items.Add("相机掉线,尝试重连相机第" + nReConnectTimeTemp + "次");
                        }
                        else
                        {
                            listBoxResult.Items.Add("Camera is offline. Try to reconnect to camera for" + nReConnectTimeTemp + "times");
                        }
                        listBoxResult.SelectedIndex = listBoxResult.Items.Count - 1;

                        MyCodeReader.MVID_CR_DestroyHandle_NET();

                        int nRet = MyCodeReader.MVID_CR_CreateHandle_NET(MVIDCodeReader.MVID_BCR | MVIDCodeReader.MVID_TDCR);
                        if (MVIDCodeReader.MVID_CR_OK != nRet)
                        {
                            listBoxResult.Items.Add("MVID_CR_CreateHandle failed 0x" + String.Format("{0:X}", nRet));
                            listBoxResult.SelectedIndex = listBoxResult.Items.Count - 1;
                            nReConnectTime++;
                            continue;
                        }

                        nRet = MyCodeReader.MVID_CR_CAM_BindDevice_NET(stDevList.pstCamInfo[m_nDeviceIndex]);
                        if (MVIDCodeReader.MVID_CR_OK != nRet)
                        {
                            listBoxResult.Items.Add("MVID_CR_CAM_BindDevice failed 0x" + String.Format("{0:X}", nRet));
                            listBoxResult.SelectedIndex = listBoxResult.Items.Count - 1;
                            nReConnectTime++;
                            continue;
                        }

                        nRet = MyCodeReader.MVID_CR_CAM_RegisterImageCallBack_NET(ImageCallback, IntPtr.Zero);
                        if (MVIDCodeReader.MVID_CR_OK != nRet)
                        {
                            listBoxResult.Items.Add("MVID_CR_CAM_RegisterImageCallBack failed 0x" + String.Format("{0:X}", nRet));
                            listBoxResult.SelectedIndex = listBoxResult.Items.Count - 1;
                            nReConnectTime++;
                            continue;
                        }

                        nRet = MyCodeReader.MVID_CR_RegisterExceptionCallBack_NET(ExceptionCallBack, IntPtr.Zero);
                        if (MVIDCodeReader.MVID_CR_OK != nRet)
                        {
                            listBoxResult.Items.Add("MVID_CR_RegisterExceptionCallBack failed 0x" + String.Format("{0:X}", nRet));
                            listBoxResult.SelectedIndex = listBoxResult.Items.Count - 1;
                            nReConnectTime++;
                            continue;
                        }

                        nRet = MyCodeReader.MVID_CR_CAM_StartGrabbing_NET();
                        if (MVIDCodeReader.MVID_CR_OK != nRet)
                        {
                            listBoxResult.Items.Add("MVID_CR_CAM_StartGrabbing failed 0x" + String.Format("{0:X}", nRet));
                            listBoxResult.SelectedIndex = listBoxResult.Items.Count - 1;
                            nReConnectTime++;
                            continue;
                        }

                        bConnect = true;
                    }

                    if (0x0804 == nLCID)
                    {
                        if (bConnect == true)
                        {
                            listBoxResult.Items.Add("相机重连成功");
                            listBoxResult.SelectedIndex = listBoxResult.Items.Count - 1;
                        }
                        else
                        {
                            listBoxResult.Items.Add("相机重连失败");
                            listBoxResult.SelectedIndex = listBoxResult.Items.Count - 1;
                        }
                    }
                    else
                    {
                        if (bConnect == true)
                        {
                            listBoxResult.Items.Add("The camera is reconnected");
                            listBoxResult.SelectedIndex = listBoxResult.Items.Count - 1;
                        }
                        else
                        {
                            listBoxResult.Items.Add("Reconnecting to camera failed");
                            listBoxResult.SelectedIndex = listBoxResult.Items.Count - 1;
                        }
                    }
                }
                if (nMsgType == MVIDCodeReader.MVID_EXCEPTION_SOFTDOG_DISCONNECT)
                {
                    if (0x0804 == nLCID)
                    {
                        listBoxResult.Items.Add("加密狗掉线");
                    }
                    else
                    {
                        listBoxResult.Items.Add("The dongle is offline");
                    }
                    listBoxResult.SelectedIndex = listBoxResult.Items.Count - 1;
                }

                m_nListNum = listBoxResult.Items.Count;
                CheckListNum();
            }

        }

        public ReconnectDemo_CS()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            ListResult = ShowList;

            nLCID = System.Globalization.CultureInfo.CurrentCulture.LCID;

            ButtonStart.Enabled = true;
            ButtonStop.Enabled = false;
        }

        // 搜索相机按钮
        private void ButtonEnum_Click(object sender, EventArgs e)
        {
            DeviceListAcq();
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
            }
            else
            {
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

            bProcess = false;

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

            // ch:注册图像回调函数 | en:Register image callback
            ImageCallback = new MVIDCodeReader.cbOutputdelegate(ImageCallbackFunc);
            nRet = MyCodeReader.MVID_CR_CAM_RegisterImageCallBack_NET(ImageCallback, IntPtr.Zero);
            if (MVIDCodeReader.MVID_CR_OK != nRet)
            {
                string csMessage;
                csMessage = "MVID_CR_CAM_RegisterImageCallBack failed: 0x" + String.Format("{0:X}", nRet);
                MessageBox.Show(csMessage);
                return;
            }

            // 注册异常回调函数
            ExceptionCallBack = new MVIDCodeReader.cbExceptiondelegate(ExceptionCallBackFunc);
            nRet = MyCodeReader.MVID_CR_RegisterExceptionCallBack_NET(ExceptionCallBack, IntPtr.Zero);
            if (MVIDCodeReader.MVID_CR_OK != nRet)
            {
                string csMessage;
                csMessage = "MVID_CR_RegisterExceptionCallBack failed: 0x" + String.Format("{0:X}", nRet);
                MessageBox.Show(csMessage);
                return;
            }

            nRet = MyCodeReader.MVID_CR_CAM_StartGrabbing_NET();
            if (MVIDCodeReader.MVID_CR_OK != nRet)
            {
                string csMessage;
                csMessage = "MVID_CR_CAM_StartGrabbing failed: 0x" + String.Format("{0:X}", nRet);
                MessageBox.Show(csMessage);
                return;
            }

            bProcess = true;

            ButtonStart.Enabled = false;
            ButtonStop.Enabled = true;
        }

        private void ComboBoxCamList_SelectedIndexChanged(object sender, EventArgs e)
        {
            m_nDeviceIndex = ComboBoxCamList.SelectedIndex;
        }

        // 结束读码
        private void ButtonStop_Click(object sender, EventArgs e)
        {
            string csMessage;
            bProcess = false;

            int nRet = MyCodeReader.MVID_CR_DestroyHandle_NET();
            if (MVIDCodeReader.MVID_CR_OK != nRet)
            {
                csMessage = "stop identify failed: 0x" + String.Format("{0:X}", nRet);
                MessageBox.Show(csMessage);
            }
            else
            {
                csMessage = "stop identify success";
                MessageBox.Show(csMessage);
            }

            ButtonStart.Enabled = true;
            ButtonStop.Enabled = false;
        }

        // 清空消息
        private void ButtonClean_Click(object sender, EventArgs e)
        {
            listBoxResult.Items.Clear();
        }

        private void ReconnectDemo_CS_FormClosed(object sender, FormClosedEventArgs e)
        {
            bProcess = false;

            if (listBoxResult.IsDisposed)
            {
                int nRet = MyCodeReader.MVID_CR_DestroyHandle_NET();
                if (MVIDCodeReader.MVID_CR_OK != nRet)
                {
                    string csMessage;
                    csMessage = "MVID_CR_DestroyHandle_NET failed: 0x" + String.Format("{0:X}", nRet);
                    listBoxResult.Items.Add(csMessage);
                }
            }
        }

        // 展示读码信息列表
        private void ShowList(IntPtr ptr)
        {
            stOutput = (MVIDCodeReader.MVID_CAM_OUTPUT_INFO)Marshal.PtrToStructure(ptr, typeof(MVIDCodeReader.MVID_CAM_OUTPUT_INFO));

            if (0x0804 == nLCID)
            {
                if (0 != stOutput.stCodeList.nCodeNum)
                {
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
                    listBoxResult.Items.Add("已识别" + stOutput.stCodeList.nCodeNum + "个对象, " + "帧号[" + stOutput.stImage.nFrameNum + "]");
                }
            }
            else
            {
                if (0 != stOutput.stCodeList.nCodeNum)
                {
                    listBoxResult.Items.Add("Recognized" + stOutput.stCodeList.nCodeNum + "Objects:");
                    for (int i = 0; i < stOutput.stCodeList.nCodeNum; ++i)
                    {
                        listBoxResult.Items.Add("No." + i.ToString() + ": [" + stOutput.stCodeList.stCodeInfo[i].strCode + "], code type[" +
                        Convert.ToInt32(stOutput.stCodeList.stCodeInfo[i].enBarType) + "], filtered or not:[" +
                        stOutput.stCodeList.stCodeInfo[i].nFilterFlag + "], frame No.[" + stOutput.stImage.nFrameNum + "]");
                    }
                }
                else
                {
                    listBoxResult.Items.Add("Recognized" + stOutput.stCodeList.nCodeNum + "Objects," +"frame No.[" + stOutput.stImage.nFrameNum + "]");
                }
            }

            listBoxResult.SelectedIndex = listBoxResult.Items.Count - 1;
            m_nListNum = listBoxResult.Items.Count;
            CheckListNum();
        }

        private void CheckListNum()
        {
            // ch:对ListBox行数增加限制，避免内存一直上涨 | en:Number limit of ListBox lines, which avoids the continuous increase of memory
            if (100 < m_nListNum)
            {
                listBoxResult.Items.Clear();
            }
        }
    }
}
