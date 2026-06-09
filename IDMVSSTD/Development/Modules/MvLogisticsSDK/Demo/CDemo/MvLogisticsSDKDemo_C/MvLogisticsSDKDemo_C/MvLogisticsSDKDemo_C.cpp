#include <stdio.h>
#include <stdlib.h>

#if defined __GNUC__
#include <unistd.h>
#define Sleep(x) usleep(x##000)

#elif defined _MSC_VER
#include <windows.h>
#include <conio.h>
#endif

#include "MvLogisticsSDKDefine.h"
#include "MvLogisticsSDK.h"

// ch:等待按键输入 | en:Wait for key press
#ifdef WIN32
void WaitForKeyPress(void)
{
    while(!_kbhit())
    {
        Sleep(10);
    }
    _getch();
}
#else
void PressEnterToExit(void)
{
    int c;
    while ( (c = getchar()) != '\n' && c != EOF );
    fprintf( stderr, "\nPress enter to exit.\n");
    while( getchar() != '\n');
}
#endif

// ch:异常消息回调 | en:Register Exception Message CallBack
void _stdcall ExceptionCallBack(MVLGS_EXCEPTION_INFO * pstEcptInfo, void* pUser)
{
    if (NULL != pstEcptInfo)
    {
        printf("Exception ID [%d]\r\n", pstEcptInfo->nExceptionID);
        printf("Exception Device Type [%d]\r\n", pstEcptInfo->enCamType);
        printf("Exception Device SerialNum [%s]\r\n", pstEcptInfo->strCamSerialNum);
        printf("Exception Description [%s]\r\n", pstEcptInfo->strExceptionDes);
    }
    else
    {
        printf("Exception Info is Null!\r\n");
    }
}

// ch:包裹回调/打印包裹相关信息 | en:Package CallBack/Print Package valied Information
void _stdcall PackageCallBack(MVLGS_PACKAGE_INFO * pstPkgInfo, void* pUser)
{
    if (NULL != pstPkgInfo)
    {
        printf("/**********************************************************/\r\n");
        if (false != pstPkgInfo->bCodeEnable)
        {
            printf("Package: Codelist Num [%d]\r\n", pstPkgInfo->nCodeListNum);

            // ch:显示当前条码信息 | en:Show Code Information
            for (int i = 0; i < pstPkgInfo->nCodeListNum; i++)
            {
                if (0 == pstPkgInfo->stCodeList[i].nCodeNum)
                {
                    printf("No Code Can Read!\r\n");
                }
                else
                {
                    printf("Code Num [%d], Trigger Index [%d], Running Number [%d]\r\n", pstPkgInfo->stCodeList[i].nCodeNum,
                        pstPkgInfo->stCodeList[i].stImage.nTriggerIndex, pstPkgInfo->stCodeList[i].nRunNumber);
                    for (int j = 0; j < pstPkgInfo->stCodeList[i].nCodeNum; j++)
                    {
                        printf("Code [%d] [%s]\r\n", j, pstPkgInfo->stCodeList[i].stCodeInfo[j].strCode);
                    }
                }
            }
        }

        if (pstPkgInfo->bPRMEnable)
        {
            printf("Can get PRMInfo!\r\n");
        }

        // ch:显示体积信息 | en:Show Volume Information
        if (pstPkgInfo->bVolumeEnable)
        {
            printf("Get VolumeInfo: VolumeInfo Length[%f] Width[%f] Height[%f] Volume[%f]\r\n", 
                pstPkgInfo->stVolumeInfo.fLength,
                pstPkgInfo->stVolumeInfo.fWidth,
                pstPkgInfo->stVolumeInfo.fHeight,
                pstPkgInfo->stVolumeInfo.fVolume);
        }

        // ch:显示重量信息 | en:Show Weigh Information
        if (pstPkgInfo->bWeightEnable)
        {
            printf("Get WeightInfo: WeightInfo Weight[%f]KG\r\n", pstPkgInfo->fWeight);
        }

        printf("/**********************************************************/\r\n");
    }
}

int main(void)
{
    void *handle    = NULL;
    int nRet        = MV_LGS_OK;

    do
    {
        // ch:获取物流SDK版本号 | en:Get LGS SDK Version
        int nVersion = MV_LGS_GetVersion();
        int nV1 = ((nVersion & 0xff000000) >> 24);
        int nV2 = ((nVersion & 0x00ff0000) >> 16);
        int nV3 = ((nVersion & 0x0000ff00) >> 8);
        int nV4 = ((nVersion & 0x000000ff));
        printf("MV LGS SDK Version: V%d.%d.%d.%d\r\n", nV1, nV2, nV3, nV4);

        // ch:选择设备并创建句柄 | en:Select device and create handle
        nRet = MV_LGS_CreateHandle(&handle);
        if (MV_LGS_OK != nRet)
        {
            printf("Create LGS handle failed! Error Code [%#x]\r\n", nRet);
            break;
        }
        else
        {
            printf("Create LGS handle Succeed!\r\n");
        }

        // ch:加载配置文件(该配置文件在当前工程路径) | en:Load Configuration file(The Cofiguration file in the current project path)
        nRet = MV_LGS_LoadDevCfg(handle, "MvLogisticsSDK.xml");
        if (MV_LGS_OK != nRet)
        {
            printf("Load Device Configuration Failed! Error Code [%#x]\r\n", nRet);
            break;
        }
        else
        {
            printf("Load Device Configuration Succeed!\r\n");
        }

        // ch:注册回调函数 | en:Register image callback
        nRet = MV_LGS_RegisterPackageCB(handle, PackageCallBack, handle);
        if (MV_LGS_OK != nRet)
        {
            printf("MV_LGS_RegisterPackageCB failed! Error Code [%#x]\r\n", nRet);
            break;
        }
        else
        {
            printf("MV_LGS_RegisterPackageCB Succeed!\r\n");
        }

        // ch:注册异常回调函数 | en:Register Exception Message CallBack
        nRet = MV_LGS_RegisterExceptionCB(handle, ExceptionCallBack, handle);
        if (MV_LGS_OK != nRet)
        {
            printf("MV_LGS_RegisterExceptionCB failed! Error Code [%#x]\r\n", nRet);
            break;
        }
        else
        {
            printf("MV_LGS_RegisterExceptionCB Succeed!\r\n");
        }

        // ch:开始取流 | en:Start grab image
        nRet = MV_LGS_Start(handle);
        if (MV_LGS_OK != nRet)
        {
            printf("Start Work failed! Error Code [%#x]\r\n", nRet);
            break;
        }
        else
        {
            printf("Start Work Succeed!\r\n");
        }

        nRet = MV_LGS_SetRunNumber(handle, 5);
        if (MV_LGS_OK != nRet)
        {
            printf("Set Running Number failed! Error Code [%#x]\r\n", nRet);
            break;
        }

        nRet = MV_LGS_SetTrigger(handle, MV_LGS_BEGIN_TRIGGER);
        if (MV_LGS_OK != nRet)
        {
            printf("Set Trigger Begin failed! Error Code [%#x]\r\n", nRet);
        }

        printf("Press Trigger to Get Package Information, or press a keyboard to stop grabbing!\r\n");

#ifdef WIN32
        WaitForKeyPress();
#else
        PressEnterToExit();
#endif

        nRet = MV_LGS_SetTrigger(handle, MV_LGS_STOP_TRIGGER);
        if (MV_LGS_OK != nRet)
        {
            printf("Set Trigger Stop failed! Error Code [%#x]\r\n", nRet);
        }

        // ch:停止取流 | en:Stop grab image
        nRet = MV_LGS_Stop(handle);
        if (MV_LGS_OK != nRet)
        {
            printf("Stop Grabbing fail! Error Code [%#x]\r\n", nRet);
            break;
        }
        else
        {
            printf("Stop Grabbing Succeed!\r\n");
        }

        // ch:销毁句柄 | en:Destroy handle
        nRet = MV_LGS_DestroyHandle(handle);
        if (MV_LGS_OK != nRet)
        {
            printf("Destroy Handle fail! Error Code [%#x]\r\n", nRet);
            break;
        }
        else
        {
            if (NULL != handle)
            {
                MV_LGS_DestroyHandle(handle);
                handle = NULL;
            }

            printf("Destroy Handle Succeed!\r\n");
        }

    } while (0);

    if (NULL != handle)
    {
        MV_LGS_Stop(handle);
        MV_LGS_DestroyHandle(handle);
        handle = NULL;
    }

    printf("Press a key to exit.\r\n");

#ifdef WIN32
    WaitForKeyPress();
#else
    PressEnterToExit();
#endif

    return MV_LGS_OK;

}