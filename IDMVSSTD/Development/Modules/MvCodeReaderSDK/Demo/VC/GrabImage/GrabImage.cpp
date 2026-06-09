#include <stdio.h>
#include <iostream>
#include <string>

#include <Windows.h>
#include <process.h>
#include <conio.h>
#include "MvCodeReaderParams.h"
#include "MvCodeReaderErrorDefine.h"
#include "MvCodeReaderCtrl.h"
#include "MvCodeReaderCtrlEx.h"
bool g_bExit = false;

// ch:等待按键输入 | en:Wait for key press
void WaitForKeyPress(void)
{
    while(!_kbhit())
    {
        Sleep(10);
    }
    _getch();
}

// ch:判断字符类型 | en:str type
bool IsStrUTF8(const char* pBuffer, int size)
{
    if (size < 0)
    {
        return false;
    }

    bool IsUTF8 = true;
    unsigned char* start = (unsigned char*)pBuffer;
    unsigned char* end = (unsigned char*)pBuffer + size;
    if (NULL == start ||
        NULL == end)
    {
        return false;
    }
    while (start < end)
    {
        if (*start < 0x80) // ch:(10000000): 值小于0x80的为ASCII字符 | en:(10000000): if the value is smaller than 0x80, it is the ASCII character
        {
            start++;
        }
        else if (*start < (0xC0)) // ch:(11000000): 值介于0x80与0xC0之间的为无效UTF-8字符 | en:(11000000): if the value is between 0x80 and 0xC0, it is the invalid UTF-8 character
        {
            IsUTF8 = false;
            break;
        }
        else if (*start < (0xE0)) // ch:(11100000): 此范围内为2字节UTF-8字符  | en: (11100000): if the value is between 0xc0 and 0xE0, it is the 2-byte UTF-8 character
        {
            if (start >= end - 1)
            {
                break;
            }

            if ((start[1] & (0xC0)) != 0x80)
            {
                IsUTF8 = false;
                break;
            }

            start += 2;
        }
        else if (*start < (0xF0)) // ch:(11110000): 此范围内为3字节UTF-8字符 | en: (11110000): if the value is between 0xE0 and 0xF0, it is the 3-byte UTF-8 character 
        {
            if (start >= end - 2)
            {
                break;
            }

            if ((start[1] & (0xC0)) != 0x80 || (start[2] & (0xC0)) != 0x80)
            {
                IsUTF8 = false;
                break;
            }

            start += 3;
        }
        else
        {
            IsUTF8 = false;
            break;
        }
    }

    return IsUTF8;
}

// ch: 单字节转宽字节 | en: char convert to Wchar
bool Char2Wchar(const char *pStr, wchar_t *pOutWStr, int nOutStrSize)
{
    if (!pStr || !pOutWStr)
    {
        return false;
    }

    bool bIsUTF = IsStrUTF8(pStr, strlen(pStr));
    UINT nChgType = bIsUTF ? CP_UTF8 : CP_ACP;

    int iLen = MultiByteToWideChar(nChgType, 0, (LPCSTR)pStr, -1, NULL, 0);

    memset(pOutWStr, 0, sizeof(wchar_t) * nOutStrSize);

    if (iLen >= nOutStrSize)
    {
        iLen = nOutStrSize - 1;
    }

    MultiByteToWideChar(nChgType, 0, (LPCSTR)pStr, -1, pOutWStr, iLen);

    pOutWStr[iLen] = 0;

    return true;
}

// ch: 宽字节转单字节 | en: Wchar convert to char
bool Wchar2char(wchar_t *pOutWStr, char *pStr)
{
    if (!pStr || !pOutWStr)
    {
        return false;
    }

    int nLen =  WideCharToMultiByte(CP_ACP, 0, pOutWStr, wcslen(pOutWStr), NULL, 0, NULL, NULL);

    WideCharToMultiByte(CP_ACP, 0 , pOutWStr, wcslen(pOutWStr), pStr, nLen, NULL, NULL);

    pStr[nLen] = '\0';

    return true;
}

// ch:打印设备的详细信息 | en:Print device Info
bool PrintDeviceInfo(MV_CODEREADER_DEVICE_INFO* pstMVDevInfo)
{
    if (NULL == pstMVDevInfo)
    {
        printf("The Pointer of pstMVDevInfo is NULL!\n");
        return false;
    }
    if (pstMVDevInfo->nTLayerType == MV_CODEREADER_GIGE_DEVICE)
    {
        int nIp1 = ((pstMVDevInfo->SpecialInfo.stGigEInfo.nCurrentIp & 0xff000000) >> 24);
        int nIp2 = ((pstMVDevInfo->SpecialInfo.stGigEInfo.nCurrentIp & 0x00ff0000) >> 16);
        int nIp3 = ((pstMVDevInfo->SpecialInfo.stGigEInfo.nCurrentIp & 0x0000ff00) >> 8);
        int nIp4 = (pstMVDevInfo->SpecialInfo.stGigEInfo.nCurrentIp & 0x000000ff);

        // ch:打印当前相机ip和用户自定义名字 | en:print current ip and user defined name
        printf("CurrentIp: %d.%d.%d.%d\n" , nIp1, nIp2, nIp3, nIp4);
        wchar_t strWchar[16] = {0};
        Char2Wchar((char*)pstMVDevInfo->SpecialInfo.stGigEInfo.chUserDefinedName, strWchar, 16);
        Wchar2char(strWchar, (char*)pstMVDevInfo->SpecialInfo.stGigEInfo.chUserDefinedName);
        printf("UserDefinedName: %s\n\n" , pstMVDevInfo->SpecialInfo.stGigEInfo.chUserDefinedName);
    }
    else if (pstMVDevInfo->nTLayerType == MV_CODEREADER_USB_DEVICE)
    {
        wchar_t strWchar[16] = {0};
        Char2Wchar((char*)pstMVDevInfo->SpecialInfo.stUsb3VInfo.chUserDefinedName, strWchar, 16);
        Wchar2char(strWchar, (char*)pstMVDevInfo->SpecialInfo.stUsb3VInfo.chUserDefinedName);
        printf("UserDefinedName: %s\n", pstMVDevInfo->SpecialInfo.stUsb3VInfo.chUserDefinedName);
        printf("Serial Number: %s\n", pstMVDevInfo->SpecialInfo.stUsb3VInfo.chSerialNumber);
        printf("Device Number: %d\n\n", pstMVDevInfo->SpecialInfo.stUsb3VInfo.nDeviceNumber);
    }
    else
    {
        printf("Not support.\n");
    }
    return true;
}

// ch:取流线程 | en:grab thread
static  unsigned int __stdcall WorkThread(void* pUser)
{
    int nRet = MV_CODEREADER_OK;

    MV_CODEREADER_IMAGE_OUT_INFO_EX2 stImageInfo = {0};
    memset(&stImageInfo, 0, sizeof(MV_CODEREADER_IMAGE_OUT_INFO_EX2));
    unsigned char * pData = NULL;

    while(1)
    {
        nRet = MV_CODEREADER_GetOneFrameTimeoutEx2(pUser, &pData, &stImageInfo, 1000);
        if (nRet == MV_CODEREADER_OK)
        {
            MV_CODEREADER_RESULT_BCR_EX2* stBcrResult = (MV_CODEREADER_RESULT_BCR_EX2*)stImageInfo.UnparsedBcrList.pstCodeListEx2;
			MV_CODEREADER_OCR_INFO_LIST* stOcrResult = (MV_CODEREADER_OCR_INFO_LIST*)stImageInfo.UnparsedOcrList.pstOcrList;
            MV_CODEREADER_AGV_INFO* stAgvResult = (MV_CODEREADER_AGV_INFO*)stImageInfo.UnparsedAgvInfo.pstAgvInfo;

            printf("Get One Frame: nChannelID[%d] Width[%d], Height[%d], nFrameNum[%d], nTriggerIndex[%d]\n", 
                stImageInfo.nChannelID, stImageInfo.nWidth, stImageInfo.nHeight, stImageInfo.nFrameNum, stImageInfo.nTriggerIndex);

            printf("CodeNum[%d]\n", stBcrResult->nCodeNum);

            for (int i = 0; i < stBcrResult->nCodeNum; i++)
            {
                wchar_t strWchar[MV_CODEREADER_MAX_BCR_CODE_LEN_EX] = {0};
                Char2Wchar((char*)stBcrResult->stBcrInfoEx2[i].chCode, strWchar, MV_CODEREADER_MAX_BCR_CODE_LEN_EX);
                Wchar2char(strWchar, (char*)stBcrResult->stBcrInfoEx2[i].chCode);
                printf("Get CodeInfo: CodeNum[%d] CodeEx[%s]\n", i, stBcrResult->stBcrInfoEx2[i].chCode);
            }

			printf("OcrAllNum[%d]\n", stOcrResult->nOCRAllNum);

			for (int i = 0; i < stOcrResult->nOCRAllNum; i ++)
			{
				printf("Get OcrInfo: OCRAllNum[%d] rowIndex[%d] chOcr[%s] ocrLen[%d]\r\n", 
				stOcrResult->nOCRAllNum, i, stOcrResult->stOcrRowInfo[i].chOcr, stOcrResult->stOcrRowInfo[i].nOcrLen);
			}

            if (stAgvResult)
            {
                printf("PointNum[%d] BoxNum[%d] String(%s)[%d]\n", stAgvResult->nPointNum, stAgvResult->nBoxNum, stAgvResult->chDrawString, stAgvResult->nStringLen);
                for (int i = 0; i < stAgvResult->nPointNum; i++)
                {
                    printf("Get AgvInfo: Point[%d]: XY(%d, %d) Color(%d, %d, %d, %d) Radius(%d)\n", 
                        i, stAgvResult->stPoint[i].stPoint.x, stAgvResult->stPoint[i].stPoint.y, stAgvResult->stPoint[i].stColor.nRed,
                        stAgvResult->stPoint[i].stColor.nGreen, stAgvResult->stPoint[i].stColor.nBlue, stAgvResult->stPoint[i].stColor.nAlpha,
                        stAgvResult->stPoint[i].nRadius);
                }

                for (int i = 0; i < stAgvResult->nBoxNum; i++)
                {
                    printf("Get AgvInfo: BoxInfo[%d]: XY(%d, %d)(%d, %d)(%d, %d)(%d, %d) Color(%d, %d, %d, %d) EdgeWidth(%d)\n", i,
                        stAgvResult->stBoxInfo[i].stPoint[0].x, stAgvResult->stBoxInfo[i].stPoint[0].y,
                        stAgvResult->stBoxInfo[i].stPoint[1].x, stAgvResult->stBoxInfo[i].stPoint[1].y,
                        stAgvResult->stBoxInfo[i].stPoint[2].x, stAgvResult->stBoxInfo[i].stPoint[2].y,
                        stAgvResult->stBoxInfo[i].stPoint[3].x, stAgvResult->stBoxInfo[i].stPoint[3].y,
                        stAgvResult->stBoxInfo[i].stColor.nRed, stAgvResult->stBoxInfo[i].stColor.nGreen,
                        stAgvResult->stBoxInfo[i].stColor.nBlue,stAgvResult->stBoxInfo[i].stColor.nAlpha,
                        stAgvResult->stBoxInfo[i].nEdgeWidth);
                }

                
            }
        }
        else
        {
            printf("No data[0x%x]\n", nRet);
        }

        if(g_bExit)
        {
            break;
        }
    }
    return 0;
}


// ch:主处理函数 | en:main process
int main()
{
    int nRet = MV_CODEREADER_OK;
    void* handle = NULL;
    unsigned int nThreadID = 0;
    void* hThreadHandle = NULL;

    do 
    {
        // ch:枚举设备 | en:Enum device
        MV_CODEREADER_DEVICE_INFO_LIST stDeviceList;
        memset(&stDeviceList, 0, sizeof(MV_CODEREADER_DEVICE_INFO_LIST));
        nRet = MV_CODEREADER_EnumDevices( &stDeviceList, MV_CODEREADER_GIGE_DEVICE);
        if (MV_CODEREADER_OK != nRet)
        {
            printf("Enum Devices fail! nRet [0x%x]\n", nRet);
            break;
        }

        if (stDeviceList.nDeviceNum > 0)
        {
            for (unsigned int i = 0; i < stDeviceList.nDeviceNum; i++)
            {
                printf("[device %d]:\n", i);
                MV_CODEREADER_DEVICE_INFO* pDeviceInfo = stDeviceList.pDeviceInfo[i];
                if (NULL == pDeviceInfo)
                {
                    break;
                } 
                PrintDeviceInfo(pDeviceInfo);
            }  
        } 
        else
        {
            printf("Find No Devices!\n");
            break;
        }

        printf("Please Intput camera index:");
        unsigned int nIndex = 0;
        scanf("%d", &nIndex);

        if (nIndex >= stDeviceList.nDeviceNum)
        {
            printf("Intput error!\n");
            break;
        }

		std::string path;
		std::cout << "请输入文件路径: ";
		std::cin >> path;
		std::cout << "输入的路径是: " << path << std::endl;

		printf("udp升级0 tcp升级1:\r\n");
		int nUpdateType = 0;
        scanf("%d", &nUpdateType);

		if(0 == nUpdateType)
		{
			nRet = MV_CODEREADER_CreateHandleForUpgradeAndEncryption(&handle);
			if (MV_CODEREADER_OK != nRet)
			{
				printf("MV_CODEREADER_CreateHandleForUpgradeAndEncryption fail! nRet [0x%x]\n", nRet);
				break;
			}

			nRet = MV_CODEREADER_StartUpgradeDeviceByHandle(handle,stDeviceList.pDeviceInfo[nIndex],(char*)path.c_str());
			if (MV_CODEREADER_OK != nRet)
			{
				printf("MV_CODEREADER_StartUpgradeDeviceByHandle fail! nRet [0x%x]\n", nRet);
				break;
			}
			unsigned int nProcess = 0;
			nRet  = MV_CODEREADER_GetUpgradeProcessByHandle(handle,&nProcess);
			if (MV_CODEREADER_OK != nRet)
			{
				printf("MV_CODEREADER_GetUpgradeProcessByHandle fail! nRet [0x%x]\n", nRet);
				break;
			}
			printf("process :%d",nProcess);
		}
		else
		{
			// ch:选择设备并创建句柄 | en:Select device and create handle
			nRet = MV_CODEREADER_CreateHandle(&handle, stDeviceList.pDeviceInfo[nIndex]);
			if (MV_CODEREADER_OK != nRet)
			{
				printf("Create Handle fail! nRet [0x%x]\n", nRet);
				break;
			}

			// ch:打开设备 | en:Open device
			nRet = MV_CODEREADER_OpenDevice(handle);
			if (MV_CODEREADER_OK != nRet)
			{
				printf("Open Device fail! nRet [0x%x]\n", nRet);
				break;
			}

			nRet = MV_CODEREADER_StartUpgradeDeviceByTcp(handle,(char*)path.c_str());
			if (MV_CODEREADER_OK != nRet)
			{
				printf("Open Device fail! nRet [0x%x]\n", nRet);
				break;
			}

		}


        CloseHandle(hThreadHandle);
        hThreadHandle = NULL;

    } while (0);
    

    if (handle != NULL)
    {
        MV_CODEREADER_CloseDevice(handle);
        MV_CODEREADER_DestroyHandle(handle);
        handle = NULL;
    }

     CloseHandle(hThreadHandle);
     hThreadHandle = NULL;

    printf("Press a key to exit.\n");
    WaitForKeyPress();

    return 0;
}
