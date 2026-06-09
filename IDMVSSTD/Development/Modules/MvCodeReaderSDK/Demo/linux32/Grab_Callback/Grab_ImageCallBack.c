#include <stdio.h>
#include <string.h>
#include <unistd.h>
#include <stdlib.h>
#include <string.h>
#include <termios.h>
#include <iconv.h>

#include "MvCodeReaderParams.h"
#include "MvCodeReaderErrorDefine.h"
#include "MvCodeReaderCtrl.h"

// ch:中文转换条码长度定义 | en：Chinese coding format len
#define MAX_BCR_LEN  512

// ch:中文编码GB2312格式转换UTF_8 | en: Chinese coding format GB2312 to utf_8
int GB2312ToUTF8(char* szSrc, size_t iSrcLen, char* szDst, size_t iDstLen)
{
    iconv_t cd = iconv_open("utf-8//IGNORE", "gb2312//IGNORE");
	if(0 == cd)
    {
		return -2;  
	}
		  
    memset(szDst, 0, iDstLen);
    char **src = &szSrc;
    char **dst = &szDst;
    if(-1 == (int)iconv(cd, src, &iSrcLen, dst, &iDstLen))
    {
		return -1; 
	}
		  
    iconv_close(cd);
    return 0;
}

// ch:等待用户输入enter键来结束取流或结束程序
// en:wait for user to input enter to stop grabbing or end the sample program
void PressEnterToExit(void)
{
    int c;
    while ( (c = getchar()) != '\n' && c != EOF );
    fprintf( stderr, "\nPress Enter to exit.\n");
    while( getchar() != '\n');
}

// ch:打印设备信息 | en:Print device Info
bool PrintDeviceInfo(MV_CODEREADER_DEVICE_INFO* pstMVDevInfo)
{
    if (NULL == pstMVDevInfo)
    {
        printf("The Pointer of pstMVDevInfo is NULL!\r\n");
        return false;
    }
    if (MV_CODEREADER_GIGE_DEVICE == pstMVDevInfo->nTLayerType)
    {
        int nIp1 = ((pstMVDevInfo->SpecialInfo.stGigEInfo.nCurrentIp & 0xff000000) >> 24);
        int nIp2 = ((pstMVDevInfo->SpecialInfo.stGigEInfo.nCurrentIp & 0x00ff0000) >> 16);
        int nIp3 = ((pstMVDevInfo->SpecialInfo.stGigEInfo.nCurrentIp & 0x0000ff00) >> 8);
        int nIp4 = (pstMVDevInfo->SpecialInfo.stGigEInfo.nCurrentIp & 0x000000ff);

        // ch:打印当前相机ip和用户自定义名字 | en:print current ip and user defined name
        printf("CurrentIp: %d.%d.%d.%d\r\n" , nIp1, nIp2, nIp3, nIp4);
        printf("UserDefinedName: %s\r\n\n" , pstMVDevInfo->SpecialInfo.stGigEInfo.chUserDefinedName);
    }
    else if (MV_CODEREADER_USB_DEVICE == pstMVDevInfo->nTLayerType)
    {
        printf("UserDefinedName: %s\r\n\n", pstMVDevInfo->SpecialInfo.stUsb3VInfo.chUserDefinedName);
    }
    else
    {
        printf("Not support.\r\n");
    }

    return true;
}

// ch: 回调图像信息 | en:Register Image Call Back
void __stdcall ImageCallBack(unsigned char * pData, MV_CODEREADER_IMAGE_OUT_INFO_EX2* pFrameInfo, void* pUser)
{
	int nRet = MV_CODEREADER_OK;
	
    if (pFrameInfo)
    {
        MV_CODEREADER_RESULT_BCR_EX* stBcrResult = (MV_CODEREADER_RESULT_BCR_EX*)pFrameInfo->pstCodeListEx;

        printf("Get One Frame: nChannelID[%d] Width[%d], Height[%d], nFrameNum[%d], nTriggerIndex[%d]\n", 
            pFrameInfo->nChannelID, pFrameInfo->nWidth, pFrameInfo->nHeight, pFrameInfo->nFrameNum, pFrameInfo->nTriggerIndex);

        printf("CodeNum[%d]\n", stBcrResult->nCodeNum);

		char strChar[MAX_BCR_LEN] = {0};
        for (int i = 0; i < stBcrResult->nCodeNum; i++)
        {
            memset(strChar, 0, MAX_BCR_LEN);
			nRet = GB2312ToUTF8(stBcrResult->stBcrInfoEx[i].chCode, strlen(stBcrResult->stBcrInfoEx[i].chCode), strChar, MAX_BCR_LEN);
			if (nRet == MV_CODEREADER_OK)
			{
				printf("CodeNum[%d] Code[%s]\r\n", i, strChar);
			}
			else
			{
				printf("CodeNum[%d] Code[%s]\r\n", i, stBcrResult->stBcrInfoEx[i].chCode);
			}
        }
    }
}

// ch:主处理函数 | en:main process
int main()
{
    int nRet = MV_CODEREADER_OK;
    void* handle = NULL;
	bool bIsNormalRun = true;
	
    do 
    {
        // ch:枚举设备 | Enum device
        MV_CODEREADER_DEVICE_INFO_LIST stDeviceList;
        memset(&stDeviceList, 0, sizeof(MV_CODEREADER_DEVICE_INFO_LIST));

        nRet = MV_CODEREADER_EnumDevices(&stDeviceList, MV_CODEREADER_GIGE_DEVICE);
        if (MV_CODEREADER_OK != nRet)
        {
            printf("Enum Devices fail! nRet [0x%x]\r\n", nRet);
            break;
        }
        else
        {
            printf("Enum Devices succeed!\r\n");
        }

        if (stDeviceList.nDeviceNum > 0)
        {
            for (unsigned int i = 0; i < stDeviceList.nDeviceNum; i++)
            {
                printf("[device %d]:\r\n", i);
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
            printf("Find No Devices!\r\n");
            break;
        }

        printf("Please Intput camera index:");
        unsigned int nIndex = 0;
        scanf("%d", &nIndex);

        if (nIndex >= stDeviceList.nDeviceNum)
        {
            printf("Intput error!\r\n");
            break;
        }

        // ch:选择设备并创建句柄 | Select device and create handle
        nRet = MV_CODEREADER_CreateHandle(&handle, stDeviceList.pDeviceInfo[nIndex]);
        if (MV_CODEREADER_OK != nRet)
        {
            printf("Create Handle fail! nRet [0x%x]\r\n", nRet);
            break;
        }
        else
        {
            printf("Create Handle succeed!\r\n");
        }

        // ch:打开设备 | Open device
        nRet = MV_CODEREADER_OpenDevice(handle);
        if (MV_CODEREADER_OK != nRet)
        {
            printf("Open Device fail! nRet [0x%x]\r\n", nRet);
            break;
        }
        else
        {
            printf("Open Device succeed!\r\n");
        }

        // ch:设置触发模式为off | eb:Set trigger mode as off
        nRet = MV_CODEREADER_SetEnumValue(handle, "TriggerMode", MV_CODEREADER_TRIGGER_MODE_OFF);
        if (MV_CODEREADER_OK != nRet)
        {
            printf("Set Trigger Mode fail! nRet [0x%x]\r\n", nRet);
            break;
        }
        else
        {
            printf("Set Trigger Mode succeed!\r\n");
        }

        // ch:注册图像回调 | en:Register image callback
        nRet = MV_CODEREADER_RegisterImageCallBackEx2(handle, ImageCallBack, handle);
        if (MV_CODEREADER_OK != nRet)
        {
            printf("Register Image CallBack fail! nRet [0x%x]\r\n", nRet);
            break;
        }
        else
        {
            printf("Register Image CallBack succeed!\r\n");
        }

        // ch:开始取流 | en:Start grab image
        nRet = MV_CODEREADER_StartGrabbing(handle);
        if (MV_CODEREADER_OK != nRet)
        {
            printf("Start Grabbing fail! nRet [0x%x]\r\n", nRet);
            break;
        }
        else
        {
            printf("Start Grabbing succeed!\r\n");
        }

        printf("Press Enter to stop grabbing.\r\n");
        PressEnterToExit();

        // ch:停止取流 | en:Stop grab image
        nRet = MV_CODEREADER_StopGrabbing(handle);
        if (MV_CODEREADER_OK != nRet)
        {
            printf("Stop Grabbing fail! nRet [0x%x]\r\n", nRet);
			bIsNormalRun = false;
            break;
        }
        else
        {
            printf("Stop Grabbing succeed!\r\n");
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

	if (bIsNormalRun)
	{
		printf("Exit!\r\n");		
	}
	
	if (false == bIsNormalRun)
	{
		PressEnterToExit();
		printf("Exit!\r\n");
	}

    return 0;
}