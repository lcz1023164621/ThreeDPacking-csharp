#include <stdio.h>
#include <Windows.h>
#include <conio.h>
#include "MvCodeReaderParams.h"
#include "MvCodeReaderErrorDefine.h"
#include "MvCodeReaderCtrl.h"
#include <iostream>
#include <process.h>
#include <list>
#include "Lock.h"

using namespace std;

typedef struct _CODEREADER_IMAGE_OUT_INFO_
{
    unsigned char* pFrameData;
    MV_CODEREADER_IMAGE_OUT_INFO_EX2 stFrameInfo;
}CODEREADER_IMAGE_OUT_INFO;

CMutex g_Lock;
list <CODEREADER_IMAGE_OUT_INFO> g_lstFrameInfoList;
bool  g_bRunState = false;
unsigned int g_nSaveNum = 0;

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
        printf("UserDefinedName: %s\n\n", pstMVDevInfo->SpecialInfo.stUsb3VInfo.chUserDefinedName);
    }
    else
    {
        printf("Not support.\n");
    }

    return true;
}

char* GetCurrentProGramPath(char* pFilePath, int nSize)
{
    if (NULL == pFilePath || MAX_PATH > nSize )
    {
        return pFilePath;
    }

    wchar_t chModuleFileName[MAX_PATH] = {0};
    char chPath[MAX_PATH] = {0};
    GetModuleFileName(NULL, chModuleFileName, MAX_PATH);
    Wchar2char(chModuleFileName, chPath);
    char *pFile = strrchr(chPath, '\\');
    if (pFile)
    {
        strncpy_s(pFilePath, nSize, chPath, (pFile - chPath));
    }
    else
    {
        strncpy_s(pFilePath, nSize, chPath, nSize - 1);
    }
    memset(chPath, 0, MAX_PATH);
    sprintf(chPath, "%s\\IMAGE", pFilePath);
    memset(chModuleFileName, 0, MAX_PATH);
    Char2Wchar(chPath, chModuleFileName, MAX_PATH);
    CreateDirectory(chModuleFileName, NULL);
    memset(pFilePath, 0, sizeof(char) * nSize);
    strncpy_s(pFilePath, nSize, chPath, nSize - 1);
    return pFilePath;
}

void SaveImageFile(MV_CODEREADER_SAVE_IMAGE_PARAM_EX* pstSaveParam)
{
    if (NULL == pstSaveParam && NULL == pstSaveParam->pImageBuffer)
    {
        return;
    }
    
    char chFile[MAX_PATH] = {0};
    char chCurDir[MAX_PATH] = {0};
    GetCurrentProGramPath(chCurDir, MAX_PATH);
    sprintf_s(chFile, MAX_PATH, "%s\\Image_h%d_w%d_%d.bmp", chCurDir, pstSaveParam->nHeight, pstSaveParam->nWidth, g_nSaveNum);
    FILE* pFile = NULL;
    fopen_s(&pFile, chFile, "wb");
    if (NULL == pFile)
    {
        printf("fopen_s failed, chFile[%s]\n", chFile);
        return;
    }
    
    fwrite(pstSaveParam->pImageBuffer, 1, pstSaveParam->nImageLen, pFile);
    fclose(pFile);
    return;
}

void SaveJpegFile(unsigned char* pFrameData, MV_CODEREADER_IMAGE_OUT_INFO_EX2* pstFrameInfo)
{
    if (NULL == pFrameData && NULL == pstFrameInfo)
    {
        return;
    }

    char chFile[MAX_PATH] = {0};
    char chCurDir[MAX_PATH] = {0};
    GetCurrentProGramPath(chCurDir, MAX_PATH);
    sprintf_s(chFile, MAX_PATH, "%s\\Image_h%d_w%d_%d.jpeg", chCurDir, pstFrameInfo->nHeight, pstFrameInfo->nWidth, g_nSaveNum);
    FILE* pFile = NULL;
    fopen_s(&pFile, chFile, "wb");
    if (NULL == pFile)
    {
        printf("fopen_s failed, chFile[%s]\n", chFile);
        return;
    }

    fwrite(pFrameData, 1, pstFrameInfo->nFrameLen, pFile);
    fclose(pFile);
}

unsigned int __stdcall SaveImageThread(void* pParam)
{
    if (NULL == pParam)
    {
        return -1;
    }
    
    void* hDevHandle = pParam;
    unsigned char* pOutImageBuffer = NULL;
    unsigned int   nOutBufferSize = 0;
    int nRet = MV_CODEREADER_OK;
    CODEREADER_IMAGE_OUT_INFO stCoderFrameInfo = {0};
    memset(&stCoderFrameInfo, 0, sizeof(CODEREADER_IMAGE_OUT_INFO));
    MV_CODEREADER_RESULT_BCR_EX2* pstBcrResult = NULL;
    MV_CODEREADER_OCR_INFO_LIST* pstOcrResult = NULL;
    while(g_bRunState)
    {
        CLock lock(g_Lock);
        if (g_lstFrameInfoList.empty())
        {
            Sleep(50);
            continue;
        }
        stCoderFrameInfo = g_lstFrameInfoList.front();
        g_lstFrameInfoList.pop_front();
        
        pstBcrResult = (MV_CODEREADER_RESULT_BCR_EX2*)stCoderFrameInfo.stFrameInfo.UnparsedBcrList.pstCodeListEx2;
        pstOcrResult = (MV_CODEREADER_OCR_INFO_LIST*)stCoderFrameInfo.stFrameInfo.UnparsedOcrList.pstOcrList;
        
        unsigned int nSize = 0;
        if (stCoderFrameInfo.stFrameInfo.enPixelType == PixelType_CodeReader_Gvsp_Jpeg)
        {
            SaveJpegFile(stCoderFrameInfo.pFrameData, &stCoderFrameInfo.stFrameInfo);
        }
        else
        {
            nSize = stCoderFrameInfo.stFrameInfo.nWidth * stCoderFrameInfo.stFrameInfo.nHeight * 4;
            if (NULL == pOutImageBuffer || nOutBufferSize < nSize)
            {
                if (pOutImageBuffer)
                {
                    free(pOutImageBuffer);
                    pOutImageBuffer = NULL;
                    nOutBufferSize = 0;
                }

                pOutImageBuffer = (unsigned char*)malloc(nSize);
                if (NULL == pOutImageBuffer)
                {
                    printf("malloc failed\n");
                    break;
                }
                nOutBufferSize = nSize;
            }
            memset(pOutImageBuffer, 0, nOutBufferSize);
            
            // 存图格式为BMP图像缓存
            MV_CODEREADER_SAVE_IMAGE_PARAM_EX stSaveParam = {0};
            memset(&stSaveParam, 0, sizeof(MV_CODEREADER_SAVE_IMAGE_PARAM_EX));
            stSaveParam.pData = stCoderFrameInfo.pFrameData;
            stSaveParam.nDataLen = stCoderFrameInfo.stFrameInfo.nFrameLen;
            stSaveParam.enPixelType = stCoderFrameInfo.stFrameInfo.enPixelType;
            stSaveParam.nWidth = stCoderFrameInfo.stFrameInfo.nWidth;
            stSaveParam.nHeight = stCoderFrameInfo.stFrameInfo.nHeight;
            
            stSaveParam.pImageBuffer = pOutImageBuffer;
            stSaveParam.nBufferSize = nOutBufferSize;
            stSaveParam.enImageType = MV_CODEREADER_Image_Bmp;
            
            nRet = MV_CODEREADER_SaveImage(hDevHandle, &stSaveParam);
            if (nRet != MV_CODEREADER_OK)
            {
                if (MV_CODEREADER_E_BUFOVER == nRet)
                {
                    if (stSaveParam.pImageBuffer)
                    {
                        free(stSaveParam.pImageBuffer);
                        stSaveParam.pImageBuffer = NULL;
                        stSaveParam.nBufferSize = 0;
                    }
                    
                    stSaveParam.pImageBuffer = (unsigned char*)malloc(stSaveParam.nImageLen);
                    if (NULL == stSaveParam.pImageBuffer)
                    {
                        printf("malloc failed\n");
                        break;
                    }
                    stSaveParam.nBufferSize = stSaveParam.nImageLen;
                    memset(stSaveParam.pImageBuffer, 0, stSaveParam.nBufferSize);
                    
                    nRet = MV_CODEREADER_SaveImage(hDevHandle, &stSaveParam);
                    if (nRet != MV_CODEREADER_OK)
                    {
                        printf("Save Image failed, nRet[%#X]\n", nRet);
                    }
                    else
                    {
                        SaveImageFile(&stSaveParam);
                    }
                }
                else
                {
                    printf("Save Image failed, nRet[%#X]\n", nRet);
                }
            }
            else
            {
                SaveImageFile(&stSaveParam);
            }
        }
        
        printf("CodeNum[%d]\n", pstBcrResult->nCodeNum);
        /*for (int i = 0; i < pstBcrResult->nCodeNum; i++)
        {
            wchar_t strWchar[MV_CODEREADER_MAX_BCR_CODE_LEN_EX] = {0};
            Char2Wchar((char*)pstBcrResult->stBcrInfoEx2[i].chCode, strWchar, MV_CODEREADER_MAX_BCR_CODE_LEN_EX);
            Wchar2char(strWchar, (char*)pstBcrResult->stBcrInfoEx2[i].chCode);
            printf("Get CodeInfo: CodeNum[%d] CodeEx[%s]\n", i, pstBcrResult->stBcrInfoEx2[i].chCode);
        }*/

        printf("OcrAllNum[%d]\n", pstOcrResult->nOCRAllNum);

        /*for (int i = 0; i < pstOcrResult->nOCRAllNum; i ++)
        {
            printf("Get OcrInfo: OCRAllNum[%d] rowIndex[%d] chOcr[%s] ocrLen[%d]\r\n", 
                pstOcrResult->nOCRAllNum, i, 
                pstOcrResult->stOcrRowInfo[i].chOcr, 
                pstOcrResult->stOcrRowInfo[i].nOcrLen);
        }*/

        if (stCoderFrameInfo.pFrameData)
        {
            free(stCoderFrameInfo.pFrameData);
            stCoderFrameInfo.pFrameData = NULL;
        }
        if (pstBcrResult)
        {
            free(pstBcrResult);
            pstBcrResult = NULL;
        }
        if (pstOcrResult)
        {
            free(pstOcrResult);
            pstOcrResult = NULL;
        }
    }
    
    if (stCoderFrameInfo.pFrameData)
    {
        free(stCoderFrameInfo.pFrameData);
        stCoderFrameInfo.pFrameData = NULL;
    }
    if (pstBcrResult)
    {
        free(pstBcrResult);
        pstBcrResult = NULL;
    }
    if (pstOcrResult)
    {
        free(pstOcrResult);
        pstOcrResult = NULL;
    }

    if (pOutImageBuffer)
    {
        free(pOutImageBuffer);
        pOutImageBuffer = NULL;
        nOutBufferSize = 0;
    }
    return 0;
}

// ch:注册图像回调处理函数 | en:Register CallBack
void __stdcall ImageCallBack(unsigned char * pData, MV_CODEREADER_IMAGE_OUT_INFO_EX2* pFrameInfo, void* pUser)
{
    if (pFrameInfo)
    {
        MV_CODEREADER_RESULT_BCR_EX2* stBcrResult = (MV_CODEREADER_RESULT_BCR_EX2*)pFrameInfo->UnparsedBcrList.pstCodeListEx2;
        MV_CODEREADER_OCR_INFO_LIST* stOcrResult = (MV_CODEREADER_OCR_INFO_LIST*)pFrameInfo->UnparsedOcrList.pstOcrList;
        MV_CODEREADER_RESULT_BCR_EX2* pstCodeListEx2 = NULL;
        MV_CODEREADER_OCR_INFO_LIST* pstOcrList = NULL;
        printf("Get One Frame: nChannelID[%d] Width[%d], Height[%d], nFrameNum[%d], nTriggerIndex[%d]\n", 
            pFrameInfo->nChannelID, pFrameInfo->nWidth, pFrameInfo->nHeight, pFrameInfo->nFrameNum, pFrameInfo->nTriggerIndex);
        
        if (g_nSaveNum++ < 10)
        {
            CODEREADER_IMAGE_OUT_INFO stCoderFrameInfo = {0};
            memset(&stCoderFrameInfo, 0, sizeof(CODEREADER_IMAGE_OUT_INFO));
            // 分配图像数据缓存
            if (NULL == stCoderFrameInfo.pFrameData)
            {
                stCoderFrameInfo.pFrameData = (unsigned char*)malloc(pFrameInfo->nFrameLen);
                if (NULL == stCoderFrameInfo.pFrameData)
                {
                    printf("[ImageCallBack] malloc failed\n");
                    return;
                }
                memset(stCoderFrameInfo.pFrameData, 0, pFrameInfo->nFrameLen);
            }
            
            memcpy_s(stCoderFrameInfo.pFrameData, pFrameInfo->nFrameLen, pData, pFrameInfo->nFrameLen);
            
            // 分配条码信息缓存
            if (NULL == pstCodeListEx2)
            {
                pstCodeListEx2 = (MV_CODEREADER_RESULT_BCR_EX2*)malloc(sizeof(MV_CODEREADER_RESULT_BCR_EX2));
                if (NULL == pstCodeListEx2)
                {
                    printf("[ImageCallBack] malloc failed\n");
                    return;
                }
            }
            memcpy_s(pstCodeListEx2, sizeof(MV_CODEREADER_RESULT_BCR_EX2), stBcrResult, sizeof(MV_CODEREADER_RESULT_BCR_EX2));
            
            // 分配OCR信息缓存
            if (NULL == pstOcrList)
            {
                pstOcrList = (MV_CODEREADER_OCR_INFO_LIST*)malloc(sizeof(MV_CODEREADER_OCR_INFO_LIST));
                if (NULL == pstOcrList)
                {
                    printf("[ImageCallBack] malloc failed\n");
                    return;
                }
            }
            memcpy_s(pstOcrList, sizeof(MV_CODEREADER_OCR_INFO_LIST), stOcrResult, sizeof(MV_CODEREADER_OCR_INFO_LIST));
            
            // 拷贝帧数据其他数据
            memcpy_s(&stCoderFrameInfo.stFrameInfo, sizeof(MV_CODEREADER_IMAGE_OUT_INFO_EX2), pFrameInfo, sizeof(MV_CODEREADER_IMAGE_OUT_INFO_EX2));
            stCoderFrameInfo.stFrameInfo.UnparsedOcrList.pstOcrList = pstOcrList;
            stCoderFrameInfo.stFrameInfo.UnparsedBcrList.pstCodeListEx2 = pstCodeListEx2;
            
            CLock lock(g_Lock);
            g_lstFrameInfoList.push_back(stCoderFrameInfo);
        }
    }
}

// ch:主处理函数 | en:main process
int main()
{
    int nRet = MV_CODEREADER_OK;
    void* handle = NULL;
    HANDLE hThread = NULL;
    unsigned int nThreadId = 0;

    do 
    {
        // ch:枚举设备 | Enum device
        MV_CODEREADER_DEVICE_INFO_LIST stDeviceList;
        memset(&stDeviceList, 0, sizeof(MV_CODEREADER_DEVICE_INFO_LIST));
        nRet = MV_CODEREADER_EnumDevices(&stDeviceList, MV_CODEREADER_GIGE_DEVICE);
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

        // ch:选择设备并创建句柄 | Select device and create handle
        nRet = MV_CODEREADER_CreateHandle(&handle, stDeviceList.pDeviceInfo[nIndex]);
        if (MV_CODEREADER_OK != nRet)
        {
            printf("Create Handle fail! nRet [0x%x]\n", nRet);
            break;
        }

        // ch:打开设备 | Open device
        nRet = MV_CODEREADER_OpenDevice(handle);
        if (MV_CODEREADER_OK != nRet)
        {
            printf("Open Device fail! nRet [0x%x]\n", nRet);
            break;
        }

        // ch:设置触发模式为off | eb:Set trigger mode as off
        nRet = MV_CODEREADER_SetEnumValue(handle, "TriggerMode", MV_CODEREADER_TRIGGER_MODE_OFF);
        if (MV_CODEREADER_OK != nRet)
        {
            printf("Set Trigger Mode fail! nRet [0x%x]\n", nRet);
            break;
        }

        // ch:注册抓图回调 | en:Register image callback
        nRet = MV_CODEREADER_RegisterImageCallBackEx2(handle, ImageCallBack, handle);
        if (MV_CODEREADER_OK != nRet)
        {
            printf("Register Image CallBackEx2 fail! nRet [0x%x]\n", nRet);
            break;
        }
        
        g_nSaveNum = 0;
        g_bRunState = true;
        hThread = (HANDLE)_beginthreadex(NULL, 0, &SaveImageThread, handle, 0, &nThreadId);
        if (NULL == hThread)
        {
            printf("creat SaveImage Thread failed\n");
            break;
        }
        
        // ch:开始取流 | en:Start grab image
        nRet = MV_CODEREADER_StartGrabbing(handle);
        if (MV_CODEREADER_OK != nRet)
        {
            printf("Start Grabbing fail! nRet [0x%x]\n", nRet);
            break;
        }

        printf("Press a key to stop grabbing.\n");
        WaitForKeyPress();

        // ch:停止取流 | en:Stop grab image
        nRet = MV_CODEREADER_StopGrabbing(handle);
        if (MV_CODEREADER_OK != nRet)
        {
            printf("Stop Grabbing fail! nRet [0x%x]\n", nRet);
            break;
        }

        g_bRunState = false;
        Sleep(100);
        
        if (hThread)
        {
            DWORD dwRet = WaitForSingleObject(hThread, INFINITE);
            if (dwRet == WAIT_TIMEOUT)
            {
                TerminateThread(hThread, 0);
            }
        }
        CloseHandle(hThread);
    } while (0);

    if (handle != NULL)
    {
        // ch:关闭设备 | en:Close device
        // ch:销毁句柄 | en:Destroy handle
        MV_CODEREADER_CloseDevice(handle);
        MV_CODEREADER_DestroyHandle(handle);
        handle = NULL;
    }
    
    CLock lock(g_Lock);
    if (!g_lstFrameInfoList.empty())
    {
        CODEREADER_IMAGE_OUT_INFO stCoderFrameInfo = g_lstFrameInfoList.front();
        g_lstFrameInfoList.pop_front();

        MV_CODEREADER_RESULT_BCR_EX2* pstBcrResult = (MV_CODEREADER_RESULT_BCR_EX2*)stCoderFrameInfo.stFrameInfo.UnparsedBcrList.pstCodeListEx2;
        MV_CODEREADER_OCR_INFO_LIST* pstOcrResult = (MV_CODEREADER_OCR_INFO_LIST*)stCoderFrameInfo.stFrameInfo.UnparsedOcrList.pstOcrList;
        
        if (stCoderFrameInfo.pFrameData)
        {
            free(stCoderFrameInfo.pFrameData);
            stCoderFrameInfo.pFrameData = NULL;
        }
        if (pstBcrResult)
        {
            free(pstBcrResult);
            pstBcrResult = NULL;
        }
        if (pstOcrResult)
        {
            free(pstOcrResult);
            pstOcrResult = NULL;
        }
    }

    printf("Press a key to exit.\n");
    WaitForKeyPress();

    return 0;
}
