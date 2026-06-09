using System;
using System.Collections.Generic;
using MvCodeReaderSDKNet;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;
using System.Text;

namespace FileAccessDemo
{
    class Program
    {
        public static MvCodeReader.MV_CODEREADER_DEVICE_INFO_LIST m_stDeviceList = new MvCodeReader.MV_CODEREADER_DEVICE_INFO_LIST();
        public static MvCodeReader m_cMyDevice  = new MvCodeReader();
        public static UInt32 m_nMode = 0;
        public static UInt32 m_nDevIndex = 0;
        public static Int32 m_nRet = MvCodeReader.MV_CODEREADER_OK;

        
        static void FileAccessProgress()
        {
            Int32 nRet = MvCodeReader.MV_CODEREADER_OK;
            MvCodeReader.MV_CODEREADER_FILE_ACCESS_PROGRESS stFileAccessProgress = new MvCodeReader.MV_CODEREADER_FILE_ACCESS_PROGRESS();

            while (true)
            {
                //ch:获取文件存取进度 |en:Get progress of file access
                nRet = m_cMyDevice.MV_CODEREADER_GetFileAccessProgress_NET(ref stFileAccessProgress);
                Console.WriteLine("State = {0:x8},Completed = {1},Total = {2}", nRet , stFileAccessProgress.nCompleted , stFileAccessProgress.nTotal);
                if (nRet != MvCodeReader.MV_CODEREADER_OK || (stFileAccessProgress.nCompleted != 0 && stFileAccessProgress.nCompleted == stFileAccessProgress.nTotal))
                {
                    break;
                }

                Thread.Sleep(50);
            }
        }

        static void FileAccessThread()
        {
            MvCodeReader.MV_CODEREADER_FILE_ACCESS stFileAccess = new MvCodeReader.MV_CODEREADER_FILE_ACCESS();
            string str = System.Windows.Forms.Application.StartupPath;
            stFileAccess.pUserFileName = str + "\\UserSet1.mfa";
            stFileAccess.pDevFileName = "UserSet1";
            if (1 == m_nMode)
            {
                //ch:读模式 |en:Read mode
                m_nRet = m_cMyDevice.MV_CODEREADER_FileAccessRead_NET(ref stFileAccess);
                if (MvCodeReader.MV_CODEREADER_OK != m_nRet)
                {
                    Console.WriteLine("File Access Read failed:{0:x8}", m_nRet);
                }
            }
            else if (2 == m_nMode)
            {
                //ch:写模式 |en:Write mode
                m_nRet = m_cMyDevice.MV_CODEREADER_FileAccessWrite_NET(ref stFileAccess);
                if (MvCodeReader.MV_CODEREADER_OK != m_nRet)
                {
                    Console.WriteLine("File Access Write failed:{0:x8}", m_nRet);
                }
            }
        }

        static void Main(string[] args)
        {
            Int32 nRet = MvCodeReader.MV_CODEREADER_OK;
            do
            {
                // 枚举设备
                nRet = MvCodeReader.MV_CODEREADER_EnumDevices_NET(ref m_stDeviceList, MvCodeReader.MV_CODEREADER_GIGE_DEVICE);
                if (MvCodeReader.MV_CODEREADER_OK != nRet)
                {
                    Console.WriteLine("Enum m_cMyDevice failed:{0:x8}", nRet);
                    break;
                }
                Console.WriteLine("Enum m_cMyDevice count : " + Convert.ToString(m_stDeviceList.nDeviceNum));
                if (0 == m_stDeviceList.nDeviceNum)
                {
                    break;
                }

                // ch:打印设备信息 en:Print m_cMyDevice info
                for (UInt32 i = 0; i < m_stDeviceList.nDeviceNum; i++)
                {
                    MvCodeReader.MV_CODEREADER_DEVICE_INFO stDevInfo = (MvCodeReader.MV_CODEREADER_DEVICE_INFO)Marshal.PtrToStructure(m_stDeviceList.pDeviceInfo[i], typeof(MvCodeReader.MV_CODEREADER_DEVICE_INFO));

                    if (stDevInfo.nTLayerType == MvCodeReader.MV_CODEREADER_GIGE_DEVICE)
                    {
                        IntPtr buffer = Marshal.UnsafeAddrOfPinnedArrayElement(stDevInfo.SpecialInfo.stGigEInfo, 0);
                        MvCodeReader.MV_CODEREADER_GIGE_DEVICE_INFO stGigEDeviceInfo = (MvCodeReader.MV_CODEREADER_GIGE_DEVICE_INFO)Marshal.PtrToStructure(buffer, typeof(MvCodeReader.MV_CODEREADER_GIGE_DEVICE_INFO));

                        uint nIp1 = ((stGigEDeviceInfo.nCurrentIp & 0xff000000) >> 24);
                        uint nIp2 = ((stGigEDeviceInfo.nCurrentIp & 0x00ff0000) >> 16);
                        uint nIp3 = ((stGigEDeviceInfo.nCurrentIp & 0x0000ff00) >> 8);
                        uint nIp4 = (stGigEDeviceInfo.nCurrentIp & 0x000000ff);

                        Console.WriteLine("\n" + i.ToString() + ": [GigE] User Define Name : " + stGigEDeviceInfo.chUserDefinedName);
                        Console.WriteLine("cMyDevice IP :" + nIp1 + "." + nIp2 + "." + nIp3 + "." + nIp4);
                    }
                }

                Console.Write("Please input index(0-{0:d}):", m_stDeviceList.nDeviceNum - 1);
                try
                {
                    m_nDevIndex = Convert.ToUInt32(Console.ReadLine());
                }
                catch
                {
                    Console.Write("Invalid Input!\n");
                    break;
                }

                if (m_nDevIndex > m_stDeviceList.nDeviceNum - 1 || m_nDevIndex < 0)
                {
                    Console.Write("Input Error!\n");
                    break;
                }
                MvCodeReader.MV_CODEREADER_DEVICE_INFO stDeviceInfo = (MvCodeReader.MV_CODEREADER_DEVICE_INFO)Marshal.PtrToStructure(m_stDeviceList.pDeviceInfo[m_nDevIndex], typeof(MvCodeReader.MV_CODEREADER_DEVICE_INFO));

                if (null == m_cMyDevice)
                {
                    m_cMyDevice = new MvCodeReader();
                    if (null == m_cMyDevice)
                    {
                        Console.Write("new MvCoderRead failed!\n");
                        break;
                    }
                }

                // 创建设备
                nRet = m_cMyDevice.MV_CODEREADER_CreateHandle_NET(ref stDeviceInfo);
                if (MvCodeReader.MV_CODEREADER_OK != nRet)
                {
                    Console.WriteLine("Create m_cMyDevice failed:{0:x8}", nRet);
                    break;
                }

                // 打开设备
                nRet = m_cMyDevice.MV_CODEREADER_OpenDevice_NET();
                if (MvCodeReader.MV_CODEREADER_OK != nRet)
                {
                    Console.WriteLine("Open m_cMyDevice failed:{0:x8}", nRet);
                    break;
                }

                //ch:读模式 |en:Read mode
                Console.WriteLine("Read to file");
                m_nMode = 1;

                Thread hReadHandle = new Thread(FileAccessThread);
                hReadHandle.Start();

                Thread.Sleep(5);

                Thread hReadProgressHandle = new Thread(FileAccessProgress);
                hReadProgressHandle.Start();

                hReadProgressHandle.Join();
                hReadHandle.Join();
                if (MvCodeReader.MV_CODEREADER_OK == m_nRet)
                {
                    Console.WriteLine("File Access Read Success");
                }

                Console.WriteLine("");

                //ch:写模式 |en:Write mode
                Console.WriteLine("Write to file");
                m_nMode = 2;

                Thread hWriteHandle = new Thread(FileAccessThread);
                hWriteHandle.Start();
                
                Thread.Sleep(5);

                Thread hWriteProgressHandle = new Thread(FileAccessProgress);
                hWriteProgressHandle.Start();

                hWriteProgressHandle.Join();
                hWriteHandle.Join();
                if (MvCodeReader.MV_CODEREADER_OK == m_nRet)
                {
                    Console.WriteLine("File Access Write Success");
                }

                // 关闭设备
                nRet = m_cMyDevice.MV_CODEREADER_CloseDevice_NET();
                if (MvCodeReader.MV_CODEREADER_OK != nRet)
                {
                    Console.WriteLine("Close m_cMyDevice failed{0:x8}", nRet);
                    break;
                }

                // 销毁设备
                nRet = m_cMyDevice.MV_CODEREADER_DestroyHandle_NET();
                if (MvCodeReader.MV_CODEREADER_OK != nRet)
                {
                    Console.WriteLine("Destroy m_cMyDevice failed:{0:x8}", nRet);
                    break;
                }
            } while (false);

            if (MvCodeReader.MV_CODEREADER_OK != nRet)
            {
                // ch:销毁设备 | en:Destroy m_cMyDevice
                nRet = m_cMyDevice.MV_CODEREADER_DestroyHandle_NET();
                if (MvCodeReader.MV_CODEREADER_OK != nRet)
                {
                    Console.WriteLine("Destroy m_cMyDevice failed:{0:x8}", nRet);
                }
            }

            Console.WriteLine("Press enter to exit");
            Console.ReadKey();
        }
    }
}
