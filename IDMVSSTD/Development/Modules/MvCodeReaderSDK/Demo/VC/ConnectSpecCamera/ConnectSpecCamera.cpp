#include <stdio.h>
#include <Windows.h>
#include <process.h>
#include <conio.h>
#include "MvCodeReaderParams.h"
#include "MvCodeReaderErrorDefine.h"
#include "MvCodeReaderCtrl.h"

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

// 收包处理线程
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
            printf("Get One Frame: ChannelID[%d] Width[%d], Height[%d], nFrameNum[%d]\n", 
                stImageInfo.nChannelID, stImageInfo.nWidth, stImageInfo.nHeight, stImageInfo.nFrameNum);

            MV_CODEREADER_RESULT_BCR_EX2* stBcrResult = (MV_CODEREADER_RESULT_BCR_EX2*)stImageInfo.UnparsedBcrList.pstCodeListEx2;
			MV_CODEREADER_OCR_INFO_LIST* stOcrResult  = (MV_CODEREADER_OCR_INFO_LIST*)stImageInfo.UnparsedOcrList.pstOcrList;

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

// 主处理函数
int main()
{
    int nRet = MV_CODEREADER_OK;
    void* handle = NULL;
    MV_CODEREADER_DEVICE_INFO stDevInfo = {0};
    MV_CODEREADER_GIGE_DEVICE_INFO stGigEDev = {0};

    // ch:需要连接的相机ip(根据实际填充) | en:The camera IP that needs to be connected (based on actual padding)
    printf("Please input Current Camera Ip : ");
    char nCurrentIp[128];
    scanf("%s", &nCurrentIp);
    // ch:相机对应的网卡ip(根据实际填充) | en:The pc IP that needs to be connected (based on actual padding)
    printf("Please input Net Export Ip : ");
    char nNetExport[128];
    scanf("%s", &nNetExport);
    unsigned int nIp1, nIp2, nIp3, nIp4, nIp;

    sscanf(nCurrentIp, "%d.%d.%d.%d", &nIp1, &nIp2, &nIp3, &nIp4);
    nIp = (nIp1 << 24) | (nIp2 << 16) | (nIp3 << 8) | nIp4;
    stGigEDev.nCurrentIp = nIp;

    sscanf(nNetExport, "%d.%d.%d.%d", &nIp1, &nIp2, &nIp3, &nIp4);
    nIp = (nIp1 << 24) | (nIp2 << 16) | (nIp3 << 8) | nIp4;
    stGigEDev.nNetExport = nIp;

    stDevInfo.nTLayerType = MV_CODEREADER_GIGE_DEVICE;// ch:仅支持GigE相机 | en:Only support GigE camera
    stDevInfo.SpecialInfo.stGigEInfo = stGigEDev;

    unsigned int nThreadID = 0;
    void* hThreadHandle =   NULL;

    do 
    {
        // ch:选择设备并创建句柄 | en:Select device and create handle
        nRet = MV_CODEREADER_CreateHandle(&handle, &stDevInfo);
        if (MV_CODEREADER_OK != nRet)
        {
            printf("Create Handle fail! nRet[0x%x]\n", nRet);
            break;
        }

        // ch:打开设备 | en:Open device
        nRet = MV_CODEREADER_OpenDevice(handle);
        if (MV_CODEREADER_OK != nRet)
        {
            printf("Open Device fail! nRet [0x%x]\n", nRet);
            break;
        }

        // ch:设置触发模式为off | en:Set trigger mode as off
        nRet = MV_CODEREADER_SetEnumValue(handle, "TriggerMode", MV_CODEREADER_TRIGGER_MODE_OFF);
        if (MV_CODEREADER_OK != nRet)
        {
            printf("Set Trigger Mode fail! nRet [0x%x]\n", nRet);
            break;
        }

        // ch:开始取流 | en:Start grab image
        nRet = MV_CODEREADER_StartGrabbing(handle);
        if (MV_CODEREADER_OK != nRet)
        {
            printf("Start Grabbing fail! nRet [0x%x]\n", nRet);
            break;
        }

        hThreadHandle = (void*) _beginthreadex( NULL , 0 , WorkThread , handle, 0 , &nThreadID );
        if (NULL == hThreadHandle)
        {
            break;
        }

        printf("Press a key to stop grabbing.\n");
        WaitForKeyPress();

        g_bExit = true;
        Sleep(1000);

        // ch:停止取流 | en:Stop grab image
        nRet = MV_CODEREADER_StopGrabbing(handle);
        if (MV_CODEREADER_OK != nRet)
        {
            printf("Stop Grabbing fail! nRet [0x%x]\n", nRet);
            break;
        }

    } while (0);
    

    if (handle != NULL)
    {
        // ch:关闭设备 | en:Close device
        // ch:销毁句柄 | en:Destroy handle
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
