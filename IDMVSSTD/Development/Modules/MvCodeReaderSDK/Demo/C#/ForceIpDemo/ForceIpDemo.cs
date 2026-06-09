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
using System.Net;

namespace MvCamera_SDK_CS_Demo
{
    public partial class ForceIpDemo : Form
    {
        MvCodeReader.MV_CODEREADER_DEVICE_INFO_LIST m_stDeviceList = new MvCodeReader.MV_CODEREADER_DEVICE_INFO_LIST();
        private MvCodeReader m_MyCamera = new MvCodeReader();

        public ForceIpDemo()
        {
            InitializeComponent();
            DeviceListAcq();
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
                ShowErrorMsg("DeviceList Acquire Failed!", nRet);
                return;
            }

            // ch:在窗体列表中显示设备名 | en:Display device name in the form list
            if (0 == m_stDeviceList.nDeviceNum)
            {
                ShowErrorMsg("None Device!", 0);
                return;
            }

            // ch:在窗体列表中显示设备名 | en:Display device name in the form list
            for (int i = 0; i < m_stDeviceList.nDeviceNum; i++)
            {
                MvCodeReader.MV_CODEREADER_DEVICE_INFO device = (MvCodeReader.MV_CODEREADER_DEVICE_INFO)Marshal.PtrToStructure(m_stDeviceList.pDeviceInfo[i], typeof(MvCodeReader.MV_CODEREADER_DEVICE_INFO));
                if (device.nTLayerType == MvCodeReader.MV_CODEREADER_GIGE_DEVICE)
                {
                    IntPtr buffer = Marshal.UnsafeAddrOfPinnedArrayElement(device.SpecialInfo.stGigEInfo, 0);
                    MvCodeReader.MV_CODEREADER_GIGE_DEVICE_INFO gigeInfo = (MvCodeReader.MV_CODEREADER_GIGE_DEVICE_INFO)Marshal.PtrToStructure(buffer, typeof(MvCodeReader.MV_CODEREADER_GIGE_DEVICE_INFO));
                    if (gigeInfo.chUserDefinedName != "")
                    {
                        cbDeviceList.Items.Add("GEV: " + gigeInfo.chUserDefinedName + " (" + gigeInfo.chSerialNumber + ")");
                    }
                    else
                    {
                        cbDeviceList.Items.Add("GEV: " + gigeInfo.chManufacturerName + " " + gigeInfo.chModelName + " (" + gigeInfo.chSerialNumber + ")");
                    }
                }
            }

            // ch:选择第一项 | en:Select the first item
            if (m_stDeviceList.nDeviceNum != 0)
            {
                cbDeviceList.SelectedIndex = 0;
            }
        }

        private void cbDeviceList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (m_stDeviceList.nDeviceNum == 0)
            {
                ShowErrorMsg("No Device", 0);
                return;
            }

            MvCodeReader.MV_CODEREADER_DEVICE_INFO device = 
                (MvCodeReader.MV_CODEREADER_DEVICE_INFO)Marshal.PtrToStructure(m_stDeviceList.pDeviceInfo[cbDeviceList.SelectedIndex], typeof(MvCodeReader.MV_CODEREADER_DEVICE_INFO));
            IntPtr buffer = Marshal.UnsafeAddrOfPinnedArrayElement(device.SpecialInfo.stGigEInfo, 0);
            MvCodeReader.MV_CODEREADER_GIGE_DEVICE_INFO gigeInfo = 
                (MvCodeReader.MV_CODEREADER_GIGE_DEVICE_INFO)Marshal.PtrToStructure(buffer, typeof(MvCodeReader.MV_CODEREADER_GIGE_DEVICE_INFO));
            UInt32 nNetIp1 = (gigeInfo.nNetExport & 0xFF000000) >> 24;
            UInt32 nNetIp2 = (gigeInfo.nNetExport & 0x00FF0000) >> 16;
            UInt32 nNetIp3 = (gigeInfo.nNetExport & 0x0000FF00) >> 8;
            UInt32 nNetIp4 = (gigeInfo.nNetExport & 0x000000FF);

            // ch:显示IP | en:Display IP
            UInt32 nIp1 = (gigeInfo.nCurrentIp & 0xFF000000) >> 24;
            UInt32 nIp2 = (gigeInfo.nCurrentIp & 0x00FF0000) >> 16;
            UInt32 nIp3 = (gigeInfo.nCurrentIp & 0x0000FF00) >> 8;
            UInt32 nIp4 = (gigeInfo.nCurrentIp & 0x000000FF);

            label4.Text = nNetIp1.ToString() + "." + nNetIp2.ToString() + "." + nNetIp3.ToString() + "." + "0" + " - " + nNetIp1.ToString() + "." + nNetIp2.ToString() + "." + nNetIp3.ToString() + "." + "255";

            tbIP.Text = nIp1.ToString() + "." + nIp2.ToString() + "." + nIp3.ToString() + "." + nIp4.ToString();

            // ch:显示掩码 | en:Display mask
            nIp1 = (gigeInfo.nCurrentSubNetMask & 0xFF000000) >> 24;
            nIp2 = (gigeInfo.nCurrentSubNetMask & 0x00FF0000) >> 16;
            nIp3 = (gigeInfo.nCurrentSubNetMask & 0x0000FF00) >> 8;
            nIp4 = (gigeInfo.nCurrentSubNetMask & 0x000000FF);

            tbMask.Text = nIp1.ToString() + "." + nIp2.ToString() + "." + nIp3.ToString() + "." + nIp4.ToString();

            // ch:显示网关 | en:Display gateway
            nIp1 = (gigeInfo.nDefultGateWay & 0xFF000000) >> 24;
            nIp2 = (gigeInfo.nDefultGateWay & 0x00FF0000) >> 16;
            nIp3 = (gigeInfo.nDefultGateWay & 0x0000FF00) >> 8;
            nIp4 = (gigeInfo.nDefultGateWay & 0x000000FF);

            tbDefaultWay.Text = nIp1.ToString() + "." + nIp2.ToString() + "." + nIp3.ToString() + "." + nIp4.ToString();
        }

        private void bnSetIp_Click(object sender, EventArgs e)
        {
            if (m_stDeviceList.nDeviceNum == 0)
            {
                ShowErrorMsg("No Device", 0);
                return;
            }

            // ch:IP转换 | en:IP conversion
            IPAddress clsIpAddr;
            if (false == IPAddress.TryParse(tbIP.Text,out clsIpAddr))
            {
                ShowErrorMsg("Please enter correct IP",0);
                return;
            }
            long nIp = IPAddress.NetworkToHostOrder(clsIpAddr.Address);

            // ch:掩码转换 | en:Mask conversion
            IPAddress clsSubMask;
            if (false == IPAddress.TryParse(tbMask.Text, out clsSubMask))
            {
                ShowErrorMsg("Please enter correct IP",0);
                return;
            }
            long nSubMask = IPAddress.NetworkToHostOrder(clsSubMask.Address);

            // ch:网关转换 | en:Gateway conversion
            IPAddress clsDefaultWay;
            if (false == IPAddress.TryParse(tbDefaultWay.Text, out clsDefaultWay))
            {
                ShowErrorMsg("Please enter correct IP",0);
                return;
            }
            long nDefaultWay = IPAddress.NetworkToHostOrder(clsDefaultWay.Address);

            if (m_stDeviceList.pDeviceInfo[cbDeviceList.SelectedIndex] == IntPtr.Zero)
            {
                return ;
            }

            // ch:打开设备 | en:Open device
            MvCodeReader.MV_CODEREADER_DEVICE_INFO device =
                (MvCodeReader.MV_CODEREADER_DEVICE_INFO)Marshal.PtrToStructure(m_stDeviceList.pDeviceInfo[cbDeviceList.SelectedIndex],
                                                              typeof(MvCodeReader.MV_CODEREADER_DEVICE_INFO));

            // ch:打开设备 | en:Open device
            if (null == m_MyCamera)
            {
                m_MyCamera = new MvCodeReader();
                if (null == m_MyCamera)
                {
                    return;
                }
            }

            int nRet = m_MyCamera.MV_CODEREADER_CreateHandle_NET(ref device);
            if (MvCodeReader.MV_CODEREADER_OK != nRet)
            {
                return;
            }

            // ch:判断设备IP是否可达
            bool bAccessible = MvCodeReader.MV_CODEREADER_IsDeviceAccessible_NET(ref device, MvCodeReader.MV_CODEREADER_ACCESS_Exclusive);
            if (bAccessible)
            {
                nRet = m_MyCamera.MV_CODEREADER_GIGE_SetIpConfig_NET(MvCodeReader.MV_CODEREADER_IP_CFG_STATIC);
                if (MvCodeReader.MV_CODEREADER_OK != nRet)
                {
                    ShowErrorMsg("Set Ip config fail", nRet);
                    m_MyCamera.MV_CODEREADER_DestroyHandle_NET();
                    m_MyCamera = null;
                    return;
                }
                nRet = m_MyCamera.MV_CODEREADER_GIGE_ForceIp_NET((uint)(nIp >> 32), (uint)(nSubMask >> 32), (uint)(nDefaultWay >> 32));
                if (MvCodeReader.MV_CODEREADER_OK != nRet)
                {
                    ShowErrorMsg("ForceIp fail", nRet);
                    m_MyCamera.MV_CODEREADER_DestroyHandle_NET();
                    m_MyCamera = null;
                    return;
                }
            }
            else
            {
                nRet = m_MyCamera.MV_CODEREADER_GIGE_ForceIp_NET((uint)(nIp >> 32), (uint)(nSubMask >> 32), (uint)(nDefaultWay >> 32));
                if (MvCodeReader.MV_CODEREADER_OK != nRet)
                {
                    ShowErrorMsg("ForceIp fail", nRet);
                    m_MyCamera.MV_CODEREADER_DestroyHandle_NET();
                    m_MyCamera = null;
                    return;
                }
                m_MyCamera.MV_CODEREADER_DestroyHandle_NET();

                //ch:需要重新创建句柄，设置为静态IP方式进行保存
                IntPtr buffer = Marshal.UnsafeAddrOfPinnedArrayElement(device.SpecialInfo.stGigEInfo, 0);
                MvCodeReader.MV_CODEREADER_GIGE_DEVICE_INFO stGigEDev =
                    (MvCodeReader.MV_CODEREADER_GIGE_DEVICE_INFO)Marshal.PtrToStructure(buffer, typeof(MvCodeReader.MV_CODEREADER_GIGE_DEVICE_INFO));
                stGigEDev.nCurrentIp = (uint)(nIp >> 32);
                stGigEDev.nCurrentSubNetMask = (uint)(nSubMask >> 32);
                stGigEDev.nDefultGateWay = (uint)(nDefaultWay >> 32);

                // stGigEDev结构体转为stDevInfo.SpecialInfo.stGigEInfo(Byte[])
                IntPtr stGigeInfoPtr = Marshal.AllocHGlobal(Marshal.SizeOf(stGigEDev));
                Marshal.StructureToPtr(stGigEDev, stGigeInfoPtr, false);
                device.SpecialInfo.stGigEInfo = new Byte[Marshal.SizeOf(device.SpecialInfo)];
                Marshal.Copy(stGigeInfoPtr, device.SpecialInfo.stGigEInfo, 0, Marshal.SizeOf(device.SpecialInfo));
                Marshal.Release(stGigeInfoPtr);

                nRet = m_MyCamera.MV_CODEREADER_CreateHandle_NET(ref device);
                if (MvCodeReader.MV_CODEREADER_OK != nRet)
                {
                    ShowErrorMsg("Create handle fail", nRet);
                    return;
                }

                nRet = m_MyCamera.MV_CODEREADER_GIGE_SetIpConfig_NET(MvCodeReader.MV_CODEREADER_IP_CFG_STATIC);
                if (MvCodeReader.MV_CODEREADER_OK != nRet)
                {
                    ShowErrorMsg("Set Ip config fail", nRet);
                    m_MyCamera.MV_CODEREADER_DestroyHandle_NET();
                    m_MyCamera = null;
                    return;
                }
            }

            ShowErrorMsg("IP Set Succeed!", 0);
            m_MyCamera.MV_CODEREADER_DestroyHandle_NET();
            m_MyCamera = null;
            GC.Collect();
        }

    }
}
