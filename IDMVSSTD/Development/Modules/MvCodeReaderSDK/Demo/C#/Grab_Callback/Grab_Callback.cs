using System;
using System.Collections.Generic;
using MvCodeReaderSDKNet;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;

namespace Grab_Callback
{
    class Grab_Callback
    {
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

        static MvCodeReader.cbOutputEx2delegate ImageCallback;
        static MvCodeReader cMyDevice = new MvCodeReader();
        static MvCodeReader.MV_CODEREADER_IMAGE_OUT_INFO_EX2 stFrameInfo = new MvCodeReader.MV_CODEREADER_IMAGE_OUT_INFO_EX2();
        public static void ImageCallbackFunc(IntPtr pData, IntPtr pstFrameInfoEx2, IntPtr pUser)
        {
            stFrameInfo = (MvCodeReader.MV_CODEREADER_IMAGE_OUT_INFO_EX2)Marshal.PtrToStructure(pstFrameInfoEx2, typeof(MvCodeReader.MV_CODEREADER_IMAGE_OUT_INFO_EX2));
            Console.WriteLine("Get one frame: ChannelID[" + Convert.ToString(stFrameInfo.nChannelID) + "] , Width[" + Convert.ToString(stFrameInfo.nWidth) + "] , Height[" + Convert.ToString(stFrameInfo.nHeight)
                                + "] , FrameNum[" + Convert.ToString(stFrameInfo.nFrameNum) + "], TriggerIndex[" + Convert.ToString(stFrameInfo.nTriggerIndex) + "]");

            MvCodeReader.MV_CODEREADER_RESULT_BCR_EX2 stBcrResult = (MvCodeReader.MV_CODEREADER_RESULT_BCR_EX2)Marshal.PtrToStructure(stFrameInfo.UnparsedBcrList.pstCodeListEx2, typeof(MvCodeReader.MV_CODEREADER_RESULT_BCR_EX2));

            Console.WriteLine("CodeNum[" + Convert.ToString(stBcrResult.nCodeNum) + "]");
            for (Int32 i = 0; i < stBcrResult.nCodeNum; i++ )
            {
                bool bIsValidUTF8 = IsTextUTF8(stBcrResult.stBcrInfoEx2[i].chCode);
                if (bIsValidUTF8)
                {
                    string strCode = Encoding.UTF8.GetString(stBcrResult.stBcrInfoEx2[i].chCode);
                    Console.WriteLine("Get CodeNum: " + "CodeNum[" + i.ToString() + "], CodeString[" + strCode.Trim().TrimEnd('\0') + "]");
                }
                else
                {
                    string strCode = Encoding.GetEncoding("GB2312").GetString(stBcrResult.stBcrInfoEx2[i].chCode);
                    Console.WriteLine("Get CodeNum: " + "CodeNum[" + i.ToString() + "], CodeString[" + strCode.Trim().TrimEnd('\0') + "]");
                }
            }

            MvCodeReader.MV_CODEREADER_OCR_INFO_LIST stOcrInfo = (MvCodeReader.MV_CODEREADER_OCR_INFO_LIST)Marshal.PtrToStructure(stFrameInfo.UnparsedOcrList.pstOcrList, typeof(MvCodeReader.MV_CODEREADER_OCR_INFO_LIST));

            Console.WriteLine("ocrAllNum[" + Convert.ToString(stOcrInfo.nOCRAllNum) + "]");
            for (int i = 0; i < stOcrInfo.nOCRAllNum; i++)
            {
                string strOcrCharCode = Encoding.UTF8.GetString(stOcrInfo.stOcrRowInfo[i].chOcr);
                Console.WriteLine("Get OcrInfo:" + "ocrNum[" + i.ToString() + "], ocrLen[" + Convert.ToString(stOcrInfo.stOcrRowInfo[i].nOcrLen) + "], ocrChar[" + strOcrCharCode.Trim().TrimEnd('\0') + "]");
            }
        }

        static void Main(string[] args)
        {
            int nRet = MvCodeReader.MV_CODEREADER_OK;

            do
            {
                // ch:枚举设备 | en:Enum cMyDevice
                MvCodeReader.MV_CODEREADER_DEVICE_INFO_LIST stDevList = new MvCodeReader.MV_CODEREADER_DEVICE_INFO_LIST();
                nRet = MvCodeReader.MV_CODEREADER_EnumDevices_NET(ref stDevList, MvCodeReader.MV_CODEREADER_GIGE_DEVICE);
                if (MvCodeReader.MV_CODEREADER_OK != nRet)
                {
                    Console.WriteLine("Enum cMyDevice failed:{0:x8}", nRet);
                    break;
                }
                Console.WriteLine("Enum cMyDevice count : " + Convert.ToString(stDevList.nDeviceNum));
                if (0 == stDevList.nDeviceNum)
                {
                    break;
                }

                MvCodeReader.MV_CODEREADER_DEVICE_INFO stDevInfo;                            // 通用设备信息
                string strUserDefinedName = "";

                // ch:打印设备信息 | en:Print cMyDevice info
                for (Int32 i = 0; i < stDevList.nDeviceNum; i++)
                {
                    stDevInfo = (MvCodeReader.MV_CODEREADER_DEVICE_INFO)Marshal.PtrToStructure(stDevList.pDeviceInfo[i], typeof(MvCodeReader.MV_CODEREADER_DEVICE_INFO));

                    if (MvCodeReader.MV_CODEREADER_GIGE_DEVICE == stDevInfo.nTLayerType)
                    {
                        MvCodeReader.MV_CODEREADER_GIGE_DEVICE_INFO stGigEDeviceInfo = (MvCodeReader.MV_CODEREADER_GIGE_DEVICE_INFO)MvCodeReader.ByteToStruct(stDevInfo.SpecialInfo.stGigEInfo, typeof(MvCodeReader.MV_CODEREADER_GIGE_DEVICE_INFO));
                        uint nIp1 = ((stGigEDeviceInfo.nCurrentIp & 0xff000000) >> 24);
                        uint nIp2 = ((stGigEDeviceInfo.nCurrentIp & 0x00ff0000) >> 16);
                        uint nIp3 = ((stGigEDeviceInfo.nCurrentIp & 0x0000ff00) >> 8);
                        uint nIp4 = (stGigEDeviceInfo.nCurrentIp & 0x000000ff);

                        strUserDefinedName = stGigEDeviceInfo.chUserDefinedName;
                        Console.WriteLine("\n" + i.ToString() + ": [GigE] User Define Name : " + strUserDefinedName);
                        Console.WriteLine("cMyDevice IP :" + nIp1 + "." + nIp2 + "." + nIp3 + "." + nIp4);
                    }
                }

                Int32 nDevIndex = 0;
                Console.Write("\nPlease input index （0 -- {0:d}） : ", stDevList.nDeviceNum - 1);
                try
                {
                    nDevIndex = Convert.ToInt32(Console.ReadLine());
                }
                catch
                {
                    Console.Write("Invalid Input!\n");
                    break;
                }

                if (nDevIndex > stDevList.nDeviceNum - 1 || nDevIndex < 0)
                {
                    Console.Write("Input Error!\n");
                    break;
                }
                stDevInfo = (MvCodeReader.MV_CODEREADER_DEVICE_INFO)Marshal.PtrToStructure(stDevList.pDeviceInfo[nDevIndex], typeof(MvCodeReader.MV_CODEREADER_DEVICE_INFO));

                // ch:创建设备 | en:Create cMyDevice
                nRet = cMyDevice.MV_CODEREADER_CreateHandle_NET(ref stDevInfo);
                if (MvCodeReader.MV_CODEREADER_OK != nRet)
                {
                    Console.WriteLine("Create cMyDevice failed:{0:x8}", nRet);
                    break;
                }

                // ch:打开设备 | en:Open cMyDevice
                nRet = cMyDevice.MV_CODEREADER_OpenDevice_NET();
                if (MvCodeReader.MV_CODEREADER_OK != nRet)
                {
                    Console.WriteLine("Open cMyDevice failed:{0:x8}", nRet);
                    break;
                }

                // ch:设置触发模式为off || en:set trigger mode as off
                if (MvCodeReader.MV_CODEREADER_OK != cMyDevice.MV_CODEREADER_SetEnumValue_NET("TriggerMode", 0))
                {
                    Console.WriteLine("Set TriggerMode failed!");
                    break;
                }

                // ch:注册回调函数 | en:Register image callback
                ImageCallback = new MvCodeReader.cbOutputEx2delegate(ImageCallbackFunc);
                nRet = cMyDevice.MV_CODEREADER_RegisterImageCallBackEx2_NET(ImageCallback, IntPtr.Zero);
                if (MvCodeReader.MV_CODEREADER_OK != nRet)
                {
                    Console.WriteLine("Register image callback failed!");
                    break;
                }

                // ch:开启抓图 || en: start grab image
                nRet = cMyDevice.MV_CODEREADER_StartGrabbing_NET();
                if (MvCodeReader.MV_CODEREADER_OK != nRet)
                {
                    Console.WriteLine("Start grabbing failed:{0:x8}", nRet);
                    break;
                }

                Console.WriteLine("Press enter to exit");
                Console.ReadLine();

                // ch:停止抓图 | en:Stop grabbing
                nRet = cMyDevice.MV_CODEREADER_StopGrabbing_NET();
                if (MvCodeReader.MV_CODEREADER_OK != nRet)
                {
                    Console.WriteLine("Stop grabbing failed{0:x8}", nRet);
                    break;
                }

                // ch:关闭设备 | en:Close cMyDevice
                nRet = cMyDevice.MV_CODEREADER_CloseDevice_NET();
                if (MvCodeReader.MV_CODEREADER_OK != nRet)
                {
                    Console.WriteLine("Close cMyDevice failed{0:x8}", nRet);
                    break;
                }

                // ch:销毁设备 | en:Destroy cMyDevice
                nRet = cMyDevice.MV_CODEREADER_DestroyHandle_NET();
                if (MvCodeReader.MV_CODEREADER_OK != nRet)
                {
                    Console.WriteLine("Destroy cMyDevice failed:{0:x8}", nRet);
                    break;
                }
        } while (false);

        if (MvCodeReader.MV_CODEREADER_OK != nRet)
        {
            // ch:销毁设备 | en:Destroy cMyDevice
            nRet = cMyDevice.MV_CODEREADER_DestroyHandle_NET();
            if (MvCodeReader.MV_CODEREADER_OK != nRet)
            {
                Console.WriteLine("Destroy cMyDevice failed:{0:x8}", nRet);
            }
        }

        Console.WriteLine("Press enter to exit");
        Console.ReadKey();
        }
    }
}
