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

namespace Grab_Callback_CS
{
    public partial class Grab_Callback_CS : Form
    {
        MVIDCodeReader.MVID_CAMERA_INFO_LIST stDevList = new MVIDCodeReader.MVID_CAMERA_INFO_LIST();
        MVIDCodeReader.MVID_CAM_OUTPUT_INFO stOutput = new MVIDCodeReader.MVID_CAM_OUTPUT_INFO();
        MVIDCodeReader MyCodeReader = new MVIDCodeReader();
        MVIDCodeReader.cbOutputdelegate ImageCallback;
        bool bProcess = false;
        Int32 m_nDeviceIndex = -1;
        Int32 m_nListNum = 0;
        private delegate void listresult(IntPtr ptr);
        private listresult ListResult;
        byte[] ImageBuffer = null;

        // ch:显示 | en:Display
        Graphics gra;
        Pen pen = new Pen(Color.Blue, 3);                   // ch:画笔颜色 | en:Brush color
        Point[] stPointList = new Point[4];                 // ch:条码位置的4个点坐标 | en:Coordinates of four points on the barcode
        Bitmap bmp;

        // ch:获得语言版本 | en:Get language version
        int nLCID = 0;

        public void ImageCallbackFunc(IntPtr pstOutput, IntPtr pUser)
        {
            if (bProcess && null != pstOutput)
            {
                listBoxResult.Invoke(ListResult, new object[] { pstOutput });
            }
        }

        public Grab_Callback_CS()
        {
            InitializeComponent();
            ListResult = ShowList;
            gra = pictureBoxDisplay.CreateGraphics();

            nLCID = System.Globalization.CultureInfo.CurrentCulture.LCID;

            ButtonStart.Enabled = true;
            ButtonStop.Enabled = false;
        }

        private void ButtonEnum_Click(object sender, EventArgs e)
        {
            DeviceListAcq();
        }

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

            // ch:注册回调函数 | en:Register image callback
            ImageCallback = new MVIDCodeReader.cbOutputdelegate(ImageCallbackFunc);
            nRet = MyCodeReader.MVID_CR_CAM_RegisterImageCallBack_NET(ImageCallback, IntPtr.Zero);
            if (MVIDCodeReader.MVID_CR_OK != nRet)
            {
                string csMessage;
                csMessage = "MVID_CR_CAM_RegisterImageCallBack failed: 0x" + String.Format("{0:X}", nRet);
                MessageBox.Show(csMessage);
                return;
            }

            Int32 nWidth, nHeight = 0;
            MVIDCodeReader.MVID_CAM_INTVALUE_EX nIntValue = new MVIDCodeReader.MVID_CAM_INTVALUE_EX();
            nRet = MyCodeReader.MVID_CR_CAM_GetIntValue_NET("Width", ref nIntValue);
            if (MVIDCodeReader.MVID_CR_OK != nRet)
            {
                string csMessage;
                csMessage = "MVID_CR_CAM_GetIntValue failed: 0x" + String.Format("{0:X}", nRet);
                MessageBox.Show(csMessage);
                return;
            }

            nWidth = (Int32)nIntValue.nCurValue;

            nRet = MyCodeReader.MVID_CR_CAM_GetIntValue_NET("Height", ref nIntValue);
            if (MVIDCodeReader.MVID_CR_OK != nRet)
            {
                string csMessage;
                csMessage = "MVID_CR_CAM_GetIntValue failed: 0x" + String.Format("{0:X}", nRet);
                MessageBox.Show(csMessage);
                return;
            }

            nHeight = (Int32)nIntValue.nCurValue;

            ImageBuffer = new byte[nWidth * nHeight * 3 + 4096];

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

        private void ButtonStop_Click(object sender, EventArgs e)
        {
            string csMessage;
            bProcess = false;

            int nRet = MyCodeReader.MVID_CR_CAM_StopGrabbing_NET();
            if (MVIDCodeReader.MVID_CR_OK != nRet)
            {
                csMessage = "stop grabbing failed: 0x" + String.Format("{0:X}", nRet);
                MessageBox.Show(csMessage);
            }

            nRet = MyCodeReader.MVID_CR_DestroyHandle_NET();
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

        private void ButtonClean_Click(object sender, EventArgs e)
        {
            listBoxResult.Items.Clear();
        }

        private void Grab_Callback_CS_FormClosed(object sender, FormClosedEventArgs e)
        {
            bProcess = false;
            if (pictureBoxDisplay.IsDisposed)
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

        private void ShowList(IntPtr ptr)
        {
            stOutput = (MVIDCodeReader.MVID_CAM_OUTPUT_INFO)Marshal.PtrToStructure(ptr, typeof(MVIDCodeReader.MVID_CAM_OUTPUT_INFO));
            if (0 != stOutput.stCodeList.nCodeNum)
            {
                if (0x0804 == nLCID)
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
                    listBoxResult.Items.Add("Recognized" + stOutput.stCodeList.nCodeNum + "Objects:");
                    for (int i = 0; i < stOutput.stCodeList.nCodeNum; ++i)
                    {
                        listBoxResult.Items.Add("No." + i.ToString() + ": [" + stOutput.stCodeList.stCodeInfo[i].strCode + "], code type[" +
                        Convert.ToInt32(stOutput.stCodeList.stCodeInfo[i].enBarType) + "], filtered or not:[" +
                        stOutput.stCodeList.stCodeInfo[i].nFilterFlag + "], frame No.[" + stOutput.stImage.nFrameNum + "]");
                    }
                }
            }
            else
            {
                if (0x0804 == nLCID)
                {
                    listBoxResult.Items.Add("已识别" + stOutput.stCodeList.nCodeNum + "个对象, " + "帧号[" + stOutput.stImage.nFrameNum + "]");
                }
                else
                {
                    listBoxResult.Items.Add("Recognized" + stOutput.stCodeList.nCodeNum + "Objects," +"frame No.[" + stOutput.stImage.nFrameNum + "]");
                }
            }
            listBoxResult.SelectedIndex = listBoxResult.Items.Count - 1;
            m_nListNum = listBoxResult.Items.Count;
            CheckListNum();

            // ch:绘制图像 | en:Draw image
            if (MVIDCodeReader.MVID_IMAGE_TYPE.MVID_IMAGE_BMP == stOutput.stImage.enImageType)
            {
                pictureBoxDisplay.Image = (Image)bmp;
            }
            else
            {
                GCHandle handle = GCHandle.Alloc(ImageBuffer, GCHandleType.Pinned);
                Marshal.Copy(stOutput.stImage.pImageBuf, ImageBuffer, 0, (int)stOutput.stImage.nImageLen);

                IntPtr pImage = handle.AddrOfPinnedObject();

                if (MVIDCodeReader.MVID_IMAGE_TYPE.MVID_IMAGE_MONO8 == stOutput.stImage.enImageType)
                {
                    bmp = new Bitmap(stOutput.stImage.nWidth, stOutput.stImage.nHeight, stOutput.stImage.nWidth, PixelFormat.Format8bppIndexed, pImage);
                    ColorPalette cp = bmp.Palette;
                    for (int i = 0; i < 256; i++)
                    {
                        cp.Entries[i] = Color.FromArgb(i, i, i);
                    }
                    bmp.Palette = cp;
                }
                else
                {
                    bmp = new Bitmap(stOutput.stImage.nWidth, stOutput.stImage.nHeight, stOutput.stImage.nWidth * 3, PixelFormat.Format24bppRgb, pImage);
                }
                pictureBoxDisplay.Image = (Image)bmp;

                if (handle.IsAllocated)
                {
                    handle.Free();
                }
            }
            pictureBoxDisplay.Invalidate();
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
