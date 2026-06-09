
#ifndef _MV_LOGISTICS_SDK_H_
#define _MV_LOGISTICS_SDK_H_

#include "MvLogisticsSDKDefine.h"

/**
*  @brief  动态库导入导出定义
*  @brief  Import and export definition of the dynamic library 
*/
#ifndef MV_LOGISTICS_API

    #if (defined (_WIN32) || defined(WIN64))
        #if defined(MV_LOGISTICS_EXPORTS)
            #define MV_LOGISTICS_API __declspec(dllexport)
        #else
            #define MV_LOGISTICS_API __declspec(dllimport)
        #endif
    #else
        #ifndef __stdcall
            #define __stdcall
        #endif

        #ifndef MV_LOGISTICS_API
            #define  MV_LOGISTICS_API
        #endif
    #endif

#endif

#ifndef IN
    #define IN
#endif

#ifndef OUT
    #define OUT
#endif

#ifdef __cplusplus
extern "C" {
#endif 

/************************************************************************
 *  @fn         MV_LGS_GetVersion()
 *  @brief      获取SDK版本号
 *  @return     返回4字节版本号 |主    |次    |修正  |  测试|
                                    8bints  8bits  8bits   8bits

 *  @fn         MV_LGS_GetVersion()
 *  @brief      Get SDK Version
 *  @return     Return 4 Byte of version number |Main   |Sub    |Rev  |  Test|
                                                 8bits  8bits   8bits   8bits
 ************************************************************************/
MV_LOGISTICS_API int __stdcall MV_LGS_GetVersion();

/************************************************************************
 *  @fn         MV_LGS_CreateHandle()
 *  @brief      创建句柄
 *  @param      handle          [IN][OUT]       句柄地址
 *  @return     成功，返回MV_LGS_OK；错误，返回错误码

 *  @fn         MV_LGS_CreateHandle()
 *  @brief      Create Handle
 *  @param      handle          [IN][OUT]       Handle Address
 *  @return     Success, return MV_LGS_OK. Failure, return error code
 ************************************************************************/
MV_LOGISTICS_API int __stdcall MV_LGS_CreateHandle(IN OUT void ** handle);

/***********************************************************************
*  @fn          MV_LGS_LoadDevCfg()
*  @brief       加载配置文件
*  @param       handle             [IN]          句柄
*  @param       strCfgPath         [IN]          配置文件路径
*  @return      成功，返回MV_LGS_OK；错误，返回错误码

*  @fn          MV_LGS_ScriptLoad()
*  @brief       Load Config File
*  @param       handle             [IN]          Handle
*  @param       strCfgPath         [IN]          FilePath of Config File
*  @return      Success, return MV_LGS_OK. Failure, return error code
***********************************************************************/
MV_LOGISTICS_API int __stdcall MV_LGS_LoadDevCfg(IN void * handle, IN const char* strCfgPath);

/************************************************************************
 *  @fn         MV_LGS_RegisterExceptionCB()
 *  @brief      注册异常消息回调
 *  @param      handle            [IN]      设备句柄
 *  @param      cbException       [IN]      异常回调函数指针
 *  @param      pUser             [IN]      用户自定义变量
 *  @return     成功，返回MV_LGS_OK；错误，返回错误码
 
 *  @fn         MV_LGS_RegisterExceptionCB()
 *  @brief      Register Exception Message CallBack
 *  @param      handle            [IN]      Device handle
 *  @param      cbException       [IN]      Exception Message CallBack Function Pointer
 *  @param      pUser             [IN]      User defined variable
 *  @return     Success, return MV_LGS_OK. Failure, return error code
************************************************************************/
MV_LOGISTICS_API int __stdcall MV_LGS_RegisterExceptionCB(IN void* handle, 
                                                                     IN void(__stdcall* cbException)(MVLGS_EXCEPTION_INFO * pstEcptInfo, void* pUser),
                                                                     IN void* pUser);

/************************************************************************
 *  @fn         MV_LGS_RegisterPackageCB()
 *  @brief      包裹消息回调
 *  @param      handle            [IN]      设备句柄
 *  @param      cbOutput          [IN]      包裹信息回调函数指针
 *  @param      pUser             [IN]      用户自定义变量
 *  @return     成功，返回MV_LGS_OK；错误，返回错误码
 
 *  @fn         MV_LGS_RegisterPackageCB()
 *  @brief      Register Exception Message CallBack
 *  @param      handle            [IN]      Device handle
 *  @param      cbOutput          [IN]      Pakcage Message CallBack Function Pointer
 *  @param      pUser             [IN]      User defined variable
 *  @return     Success, return MV_LGS_OK. Failure, return error code
************************************************************************/
MV_LOGISTICS_API int __stdcall MV_LGS_RegisterPackageCB(IN void* handle, 
                                                             IN void(__stdcall* cbOutput)(MVLGS_PACKAGE_INFO * pstPkgInfo, void* pUser),
                                                             IN void* pUser);



/************************************************************************
 *  @fn         MV_LGS_RegisterTriggerInfoCB();
 *  @brief      触发消息回调
 *  @param      handle            [IN]      设备句柄
 *  @param      cbOutput          [IN]      触发信息回调函数指针
 *  @param      pUser             [IN]      用户自定义变量
 *  @return     成功，返回MV_LGS_OK；错误，返回错误码
 
 *  @fn         MV_LGS_RegisterTriggerInfoCB();
 *  @brief      Register Trigger Message CallBack
 *  @param      handle            [IN]      Device handle
 *  @param      cbOutput          [IN]      Trigger Message CallBack Function Pointer
 *  @param      pUser             [IN]      User defined variable
 *  @return     Success, return MV_LGS_OK. Failure, return error code
************************************************************************/
MV_LOGISTICS_API int __stdcall MV_LGS_RegisterTriggerInfoCB(IN void* handle, 
                                                             IN void(__stdcall* cbOutput)(MVLGS_TRIGGER_INFO * pstTriggerInfo, void* pUser),
                                                             IN void* pUser);


/************************************************************************
 *  @fn         MV_LGS_RegisterNoReadImageCB()
 *  @brief      读码器NoRead图像回调
 *  @param      handle                  [IN]      设备句柄
 *  @param      cbNoReadImageOutput     [IN]      NoRead图像输出函数指针
 *  @param      pUser                   [IN]      用户自定义变量
 *  @return     成功，返回MV_LGS_OK；错误，返回错误码
 
 *  @fn         MV_LGS_RegisterNoReadImageCB()
 *  @brief      ReadCode Camera No ReadCode Image CallBack
 *  @param      handle                  [IN]      Device handle
 *  @param      cbNoReadImageOutput     [IN]      NoRead Image Output CallBack Function Pointer
 *  @param      pUser                   [IN]      User defined variable
 *  @return     Success, return MV_LGS_OK. Failure, return error code
************************************************************************/
MV_LOGISTICS_API int __stdcall MV_LGS_RegisterNoReadImageCB(IN void* handle,
                                                            IN void(__stdcall* cbNoReadImageOutput)(MVLGS_IMAGE_OUTPUT_INFO * pstImageOutPutInfo, void* pUser),
                                                            IN void* pUser);

/***********************************************************************
 *  @fn         MV_LGS_Start()
 *  @brief      开始取流
 *  @param      handle                 [IN]      句柄
 *  @return     成功，返回MV_LGS_OK；错误，返回错误码
 
 *  @fn         MV_LGS_Start()
 *  @brief      Start Grabbing
 *  @param      handle                 [IN]          Handle
 *  @return     Success, return MV_LGS_OK. Failure, return error code
 ***********************************************************************/
MV_LOGISTICS_API int __stdcall MV_LGS_Start(IN void * handle);

/***********************************************************************
 *  @fn         MV_LGS_Stop()
 *  @brief      结束取流
 *  @param      handle                 [IN]          句柄
 *  @return     成功，返回MV_LGS_OK；错误，返回错误码
 
 *  @fn         MV_LGS_Stop()
 *  @brief      Stop Grabbing
 *  @param      handle                 [IN]          Handle
 *  @return     Success, return MV_LGS_OK. Failure, return error code
 ***********************************************************************/
MV_LOGISTICS_API int __stdcall MV_LGS_Stop(IN void * handle);

/************************************************************************
 *  @fn         MV_LGS_DestroyHandle()
 *  @brief      销毁句柄
 *  @param      handle                 [IN]          句柄
 *  @return     成功，返回MV_LGS_OK；错误，返回错误码
 
 *  @fn         MV_LGS_DestroyHandle()
 *  @brief      Destroy Handle
 *  @param      handle                 [IN]          Handle
 *  @return     Success, return MV_LGS_OK. Failure, return error code
 ************************************************************************/
MV_LOGISTICS_API int __stdcall MV_LGS_DestroyHandle(IN void * handle);

/************************************************************************
 *  @fn         MV_LGS_SetTrigger()
 *  @brief      外部设置触发状态
 *  @param      handle                 [IN]          句柄
 *  @param      nTriggerSignal         [IN]          触发状态
 *  @return     成功，返回MV_LGS_OK；错误，返回错误码
 
 *  @fn         MV_LGS_SetTrigger()
 *  @brief      Set Trigger Signal
 *  @param      handle                 [IN]          Handle
 *  @param      nTriggerSignal         [IN]          Trigger Signal
 *  @return     Success, return MV_LGS_OK. Failure, return error code
 ************************************************************************/
MV_LOGISTICS_API int __stdcall MV_LGS_SetTrigger(IN void * handle, IN unsigned int nTriggerSignal);

/************************************************************************
 *  @fn         MV_LGS_SetRunNumber()
 *  @brief      设置流水号
 *  @param      handle                 [IN]          句柄
 *  @param      nRunNumber             [IN]          流水号
  * @param      unBindTime             [IN]          流水号和触发号绑定的时间区间
 *  @return     成功，返回MV_LGS_OK；错误，返回错误码
 
 *  @fn         MV_LGS_SetRunNumber()
 *  @brief      Set Running Number
 *  @param      handle                 [IN]          Handle
 *  @param      nRunNumber             [IN]          Running Number
 *  @param      unBindTime             [IN]          The time interval of the serial number and the trigger number binding
 *  @return     Success, return MV_LGS_OK. Failure, return error code
 ************************************************************************/
MV_LOGISTICS_API int __stdcall MV_LGS_SetRunNumber(IN void * handle, IN unsigned int nRunNumber, IN unsigned int unBindTime);

/************************************************************************
 *  @fn         MV_LGS_GetXmlCfgCamInfo()
 *  @brief      获取XML配置文件中的相机详细信息
 *  @param      pstXmlCfgCamInfo            [IN][OUT]           配置文件中对应相机信息列表
 *  @param      strCfgPath                  [IN]                配置文件路径
 *  @return     成功，返回MV_LGS_OK；错误，返回错误码

 *  @fn         MV_LGS_GetXmlCfgCamInfo()
 *  @brief      Get XML Config Camera Info
 *  @param      pstXmlCfgCamInfo            [IN][OUT]           Camera Info Lists
 *  @param      strCfgPath                  [IN]                Config File Path
 *  @return     Success, return MV_LGS_OK. Failure, return error code
 ************************************************************************/
MV_LOGISTICS_API int __stdcall MV_LGS_GetXmlCfgCamInfo(IN OUT MVLGS_XML_CFG_CAM_INFO_LIST * pstXmlCfgCamInfo, IN const char* strCfgPath);

#ifdef __cplusplus
}
#endif

#endif //_MV_LOGISTICS_SDK_H_