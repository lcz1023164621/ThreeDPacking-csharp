#include <stdio.h>
#include <Windows.h>
#include <process.h>
#include <conio.h>
#include "MvCodeReaderCtrl.h"

unsigned int g_nMode = 0;  // 读写模式：1 读取 2 写入

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

// ch:等待按键输入 | en:Wait for key press
void WaitForKeyPress(void)
{
    while(!_kbhit())
    {
        Sleep(10);
    }
    _getch();
}

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
        // 中文字体显示
        wchar_t strWchar[16] = {0};
        Char2Wchar((char*)pstMVDevInfo->SpecialInfo.stGigEInfo.chUserDefinedName, strWchar, 16);
        Wchar2char(strWchar, (char*)pstMVDevInfo->SpecialInfo.stGigEInfo.chUserDefinedName);
        printf("CurrentIp: %d.%d.%d.%d\n" , nIp1, nIp2, nIp3, nIp4);
        printf("UserDefinedName: %s\n\n" , pstMVDevInfo->SpecialInfo.stGigEInfo.chUserDefinedName);
    }
    else if (pstMVDevInfo->nTLayerType == MV_CODEREADER_USB_DEVICE)
    {
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

static  unsigned int __stdcall ProgressThread(void* pUser)
{
    int nRet = MV_CODEREADER_OK;
    MV_CODEREADER_FILE_ACCESS_PROGRESS stFileAccessProgress = {0};

    while(1)
    {
        //ch:获取文件存取进度 |en:Get progress of file access
        nRet = MV_CODEREADER_GetFileAccessProgress(pUser, &stFileAccessProgress);
        printf("State = 0x%x,Completed = %I64d,Total = %I64d\r\n", nRet, stFileAccessProgress.nCompleted,stFileAccessProgress.nTotal);
        if (nRet != MV_CODEREADER_OK || (stFileAccessProgress.nCompleted != 0 && stFileAccessProgress.nCompleted == stFileAccessProgress.nTotal))
        {
            break;
        }
    }
    return nRet;
}

char* GetCurrentProGramPath(char* pFilePath, int nSize)
{
    if (NULL == pFilePath || MAX_PATH > nSize )
    {
        return pFilePath;
    }

    char chPath[MAX_PATH] = {0};
    GetModuleFileName(NULL, chPath, MAX_PATH);
    char *pFile = strrchr(chPath, '\\');
    if (pFile)
    {
        strncpy_s(pFilePath, nSize, chPath, (pFile - chPath));
    }
    else
    {
        strncpy_s(pFilePath, nSize, chPath, nSize - 1);
    }
    return pFilePath;
}

static  unsigned int __stdcall FileAccessThread(void* pUser)
{
    MV_CODEREADER_FILE_ACCESS stFileAccess = {0};
    char chFile[MAX_PATH] = {0};
    char chCurDir[MAX_PATH] = {0};
    GetCurrentProGramPath(chCurDir, MAX_PATH);
    sprintf(chFile, "%s\\UserSet1.mfa", chCurDir);
    stFileAccess.pUserFileName = chFile;
    stFileAccess.pDevFileName = "UserSet1";
    int nRet = MV_CODEREADER_OK;
    if (1 == g_nMode)
    {
        //ch:读模式 |en:Read mode
        nRet = MV_CODEREADER_FileAccessRead(pUser, &stFileAccess);
        if (MV_CODEREADER_OK != nRet)
        {
            printf("File Access Read fail! nRet [0x%x]\n", nRet);
        }
    }
    else if (2 == g_nMode)
    {
        //ch:写模式 |en:Write mode
        nRet = MV_CODEREADER_FileAccessWrite(pUser, &stFileAccess);
        if (MV_CODEREADER_OK != nRet)
        {
            printf("File Access Write fail! nRet [0x%x]\n", nRet);
        }
    }
    return nRet;
}

int main()
{
    int nRet = MV_CODEREADER_OK;
    void* handle = NULL;

    do 
    {
        // ch:枚举设备 | en:Enum device
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

        printf("Please Input camera index(0-%d):", stDeviceList.nDeviceNum-1);
        unsigned int nIndex = 0;
        scanf_s("%d", &nIndex);

        if (nIndex >= stDeviceList.nDeviceNum)
        {
            printf("Input error!\n");
            break;
        }

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

        //ch:读模式 |en:Read mode
        g_nMode = 1;
        printf("Read to file.\n");

        unsigned int nThreadID = 0;
        void* hReadHandle = (void*) _beginthreadex( NULL , 0 , FileAccessThread , handle, 0 , &nThreadID );
        if (NULL == hReadHandle)
        {
            break;
        }

        Sleep(5);

        nThreadID = 0;
        void* hReadProgressHandle = (void*) _beginthreadex( NULL , 0 , ProgressThread , handle, 0 , &nThreadID );
        if (NULL == hReadProgressHandle)
        {
            break;
        }

        nRet = WaitForMultipleObjects(1, &hReadHandle, TRUE, INFINITE);
        if (MV_CODEREADER_OK != nRet)
        {
            printf("WaitForMultipleObjects hReadHandle failed!\n");
        }

        nRet = WaitForMultipleObjects(1, &hReadProgressHandle, TRUE, INFINITE);
        if (MV_CODEREADER_OK != nRet)
        {
            printf("WaitForMultipleObjects hReadProgressHandle failed!\n");
        }

        printf("File Access Read Success!\n");
        printf("\n");

        //ch:写模式 |en:Write mode
        g_nMode = 2;
        printf("Write from file.\n");

        nThreadID = 0;
        void* hWriteHandle = (void*) _beginthreadex( NULL , 0 , FileAccessThread , handle, 0 , &nThreadID );
        if (NULL == hWriteHandle)
        {
            break;
        }

        Sleep(5);

        nThreadID = 0;
        void* hWriteProgressHandle = (void*) _beginthreadex( NULL , 0 , ProgressThread , handle, 0 , &nThreadID );
        if (NULL == hWriteProgressHandle)
        {
            break;
        }

        nRet = WaitForMultipleObjects(1, &hWriteHandle, TRUE, INFINITE);
        if (MV_CODEREADER_OK != nRet)
        {
            printf("WaitForMultipleObjects hWriteHandle failed!\n");
        }

        nRet = WaitForMultipleObjects(1, &hWriteProgressHandle, TRUE, INFINITE);
        if (MV_CODEREADER_OK != nRet)
        {
            printf("WaitForMultipleObjects hWriteProgressHandle failed!\n");
        }
        
        printf("File Access Write Success!\n");

        // ch:关闭设备 | Close device
        nRet = MV_CODEREADER_CloseDevice(handle);
        if (MV_CODEREADER_OK != nRet)
        {
            printf("ClosDevice fail! nRet [0x%x]\n", nRet);
            break;
        }

        // ch:销毁句柄 | Destroy handle
        nRet = MV_CODEREADER_DestroyHandle(handle);
        if (MV_CODEREADER_OK != nRet)
        {
            printf("Destroy Handle fail! nRet [0x%x]\n", nRet);
            break;
        }
    } while (0);

    if (nRet != MV_CODEREADER_OK)
    {
        if (handle != NULL)
        {
            MV_CODEREADER_CloseDevice(handle);
            MV_CODEREADER_DestroyHandle(handle);
            handle = NULL;
        }
    }

    printf("Press a key to exit.\n");
    WaitForKeyPress();
    return 0;
}
