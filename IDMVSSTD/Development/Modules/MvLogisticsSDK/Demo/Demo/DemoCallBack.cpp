// 回调
#include <stdio.h>
#include <Windows.h>
#include <conio.h>
#include "MvLogisticsSDK.h"
#include "MvLogisticsSDKDefine.h"

// ch:等待按键输入 | en:Wait for key press
void WaitForKeyPress(void)
{
    while(!_kbhit())
    {
        Sleep(10);
    }
    _getch();
}

void __stdcall ImageCallBackEx(MVLGS_PACKAGE_INFO * pstPkgInfo, void* pUser)
{
    if (pstPkgInfo)
    {
        printf("/**********************************************************/\n");
        if (pstPkgInfo->bCodeEnable)
        {
            printf("Get One Frame: codelistnum[%d]\n", pstPkgInfo->nCodeListNum);

            for (int i = 0; i < pstPkgInfo->nCodeListNum; i++)
            {
                printf("Get One Frame: codenum[%d]\n", pstPkgInfo->stCodeList[i].nCodeNum);
                printf("包裹: %d\n", pstPkgInfo->stCodeList[i].stImage.nTriggerIndex);
                for (int j = 0; j < pstPkgInfo->stCodeList[i].nCodeNum; j++)
                {
                    printf("条码[%d] [%s]\n", j, pstPkgInfo->stCodeList[i].stCodeInfo[j].strCode);
                }
            }
        }

        if (pstPkgInfo->bVolumeEnable)
        {
            printf("Get One Frame: VolumeInfo Length[%f] Width[%f] Height[%f]\n", pstPkgInfo->stVolumeInfo.fLength, pstPkgInfo->stVolumeInfo.fWidth, pstPkgInfo->stVolumeInfo.fHeight);
        }

        if (pstPkgInfo->bWeightEnable)
        {
            printf("Get One Frame: WeightInfo Weight[%f]\n", pstPkgInfo->fWeight);
        }

        printf("/**********************************************************/\n");
    }
}

int main()
{
    int nRet = MV_LGS_OK;
    void* handle = NULL;

    do 
    {

        // ch:选择设备并创建句柄 | en:Select device and create handle
        nRet = MV_LGS_CreateHandle(&handle);
        if (MV_LGS_OK != nRet)
        {
            printf("Create Handle fail! nRet [%#x]\n", nRet);
            break;
        }

        nRet = MV_LGS_LoadDevCfg(handle, "MvLogisticsSDK.xml");
        if (MV_LGS_OK != nRet)
        {
            printf("LoadDevCfg fail! nRet [%#x]\n", nRet);
            break;
        }
        else
        {
            printf("LoadDevCfg success! nRet [%#x]\n", nRet);
        }

        // ch:注册抓图回调 | en:Register image callback
        nRet = MV_LGS_RegisterPackageCB(handle, ImageCallBackEx, handle);
        if (MV_LGS_OK != nRet)
        {
            printf("Register Image CallBack fail! nRet [%#x]\n", nRet);
            break;
        }


        // ch:开始取流 | en:Start grab image
        nRet = MV_LGS_Start(handle);
        if (MV_LGS_OK != nRet)
        {
            printf("MV_LGS_Start fail! nRet [%#x]\n", nRet);
            break;
        }

        printf("Press a key to stop grabbing.\n");
        WaitForKeyPress();

        // ch:停止取流 | en:Stop grab image
        nRet = MV_LGS_Stop(handle);
        if (MV_LGS_OK != nRet)
        {
            printf("Stop Grabbing fail! nRet [%#x]\n", nRet);
            break;
        }

        // ch:销毁句柄 | en:Destroy handle
        nRet = MV_LGS_DestroyHandle(handle);
        if (MV_LGS_OK != nRet)
        {
            printf("Destroy Handle fail! nRet [%#x]\n", nRet);
            break;
        }
    } while (0);

    if (nRet != MV_LGS_OK)
    {
        if (handle != NULL)
        {
            MV_LGS_DestroyHandle(handle);
            handle = NULL;
        }
    }

    printf("Press a key to exit.\n");
    WaitForKeyPress();

    return 0;
}