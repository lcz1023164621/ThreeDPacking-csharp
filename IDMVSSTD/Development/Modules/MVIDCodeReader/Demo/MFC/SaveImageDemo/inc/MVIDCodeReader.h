
#ifndef _MVID_CODEREADER_H_
#define _MVID_CODEREADER_H_

#include "MVIDCodeReaderDefine.h"

#ifdef __cplusplus
extern "C" {
#endif 


/************************************************************************
 *  @~chinese
 *  @brief  获取SDK版本号
 *  @param  chVersion              [IN][OUT]       SDK版本号
 *  @return 成功，返回MVID_CR_OK；错误，返回错误码 
 *  @remarks chVersion包含SDK，一维码，二维码版本号，以'_'分隔，如"1.2.0.1_3.6.2_2.2.5"
 
 *  @~english
 *  @brief  Get SDK Version
 *  @param  chVersion              [IN][OUT]       SDK Version
 *  @return Success, return MVID_CR_OK. Failure, return error code
 *  @remarks chVersion contains the SDK, BCR, and TDCR version numbers, separation with '_', such as "1.2.0.1_3.6.2_2.2.5"
 ************************************************************************/
MVID_CODEREADER_API int __stdcall MVID_CR_GetVersion(IN OUT char* const chVersion);

/************************************************************************
 *  @~chinese
 *  @brief  创建句柄
 *  @param  handle                 [IN][OUT]      句柄
 *  @param  nCodeAbility           [IN]           读码能力集，一维码-MVID_BCR，二维码-MVID_TDCR，面单抠图-MVID_WAYBILL，使用或运算获取多种能力
 *  @return 成功，返回MVID_CR_OK；错误，返回错误码 
 
 *  @~english
 *  @brief  Create Handle
 *  @param  handle                 [IN][OUT]      Handle
 *  @param  nCodeAbility           [IN]           Code Ability, BCR-MVID_BCR, TDCR-MVID_TDCR, Waybill-MVID_WAYBILL, use or operations to acquire multiple abilities
 *  @return Success, return MVID_CR_OK. Failure, return error code
 ************************************************************************/
MVID_CODEREADER_API int __stdcall MVID_CR_CreateHandle(IN OUT void ** handle, IN unsigned int nCodeAbility);

/************************************************************************
 *  @~chinese
 *  @brief  销毁句柄
 *  @param  handle                 [IN]          句柄
 *  @return 成功，返回MVID_CR_OK；错误，返回错误码 
 
 *  @~english
 *  @brief  Destroy Handle
 *  @param  handle                 [IN]          Handle
 *  @return Success, return MVID_CR_OK. Failure, return error code
 ************************************************************************/
MVID_CODEREADER_API int __stdcall MVID_CR_DestroyHandle(IN void * handle);

/************************************************************************
 *  @~chinese
 *  @brief  设置算法整型参数
 *  @param  handle                 [IN]          句柄
 *  @param  strParamKeyName        [IN]          属性键值
 *  @param  nValue                 [IN]          参数值
 *  @return 成功，返回MVID_CR_OK；错误，返回错误码 
 
 *  @~english
 *  @brief  Set Algorithm Integer Value
 *  @param  handle                 [IN]          Handle
 *  @param  strParamKeyName        [IN]          Param KeyName
 *  @param  nValue                 [IN]          Value
 *  @return Success, return MVID_CR_OK. Failure, return error code
 ************************************************************************/
MVID_CODEREADER_API int __stdcall MVID_CR_Algorithm_SetIntValue(IN void* handle, IN const char* const strParamKeyName, IN const int nValue);

/************************************************************************
 *  @~chinese
 *  @brief  获取算法整型参数
 *  @param  handle                 [IN]          句柄
 *  @param  strParamKeyName        [IN]          属性键值
 *  @param  pnValue                [IN][OUT]     参数值
 *  @return 成功，返回MVID_CR_OK；错误，返回错误码 
 
 *  @~english
 *  @brief  Get Algorithm Integer Value
 *  @param  handle                 [IN]          Handle
 *  @param  strParamKeyName        [IN]          Param KeyName
 *  @param  pnValue                [IN][OUT]     Value
 *  @return Success, return MVID_CR_OK. Failure, return error code
 ************************************************************************/
MVID_CODEREADER_API int __stdcall MVID_CR_Algorithm_GetIntValue(IN void* handle, IN const char* const strParamKeyName, IN OUT int* const pnValue);

/************************************************************************
 *  @~chinese
 *  @brief  设置算法浮点型参数
 *  @param  handle                 [IN]          句柄
 *  @param  strParamKeyName        [IN]          属性键值
 *  @param  fValue                 [IN]          参数值
 *  @return 成功，返回MVID_CR_OK；错误，返回错误码 
 
 *  @~english
 *  @brief  Set Algorithm Float Value
 *  @param  handle                 [IN]          Handle
 *  @param  strParamKeyName        [IN]          Param KeyName
 *  @param  fValue                 [IN]          Value
 *  @return Success, return MVID_CR_OK. Failure, return error code
 ************************************************************************/
MVID_CODEREADER_API int __stdcall MVID_CR_Algorithm_SetFloatValue(IN void* handle, IN const char* const strParamKeyName, IN const float fValue);

/************************************************************************
 *  @~chinese
 *  @brief  获取算法浮点型参数
 *  @param  handle                 [IN]          句柄
 *  @param  strParamKeyName        [IN]          属性键值
 *  @param  pfValue                [IN][OUT]     参数值
 *  @return 成功，返回MVID_CR_OK；错误，返回错误码 
 
 *  @~english
 *  @brief  Get Algorithm Float Value
 *  @param  handle                 [IN]          Handle
 *  @param  strParamKeyName        [IN]          Param KeyName
 *  @param  pfValue                [IN][OUT]     Value
 *  @return Success, return MVID_CR_OK. Failure, return error code
 ************************************************************************/
MVID_CODEREADER_API int __stdcall MVID_CR_Algorithm_GetFloatValue(IN void* handle, IN const char* const strParamKeyName, IN OUT float* const pfValue);

/************************************************************************
 *  @~chinese
 *  @brief  设置算法字符串型参数
 *  @param  handle                 [IN]          句柄
 *  @param  strParamKeyName        [IN]          属性键值
 *  @param  strValue               [IN]          参数值
 *  @return 成功，返回MVID_CR_OK；错误，返回错误码 
 
 *  @~english
 *  @brief  Set Algorithm String Value
 *  @param  handle                 [IN]          Handle
 *  @param  strParamKeyName        [IN]          Param KeyName
 *  @param  strValue               [IN]          Value
 *  @return Success, return MVID_CR_OK. Failure, return error code
 ************************************************************************/
MVID_CODEREADER_API int __stdcall MVID_CR_Algorithm_SetStringValue(IN void* handle, IN const char* const strParamKeyName, IN const char* const strValue);

/************************************************************************
 *  @~chinese
 *  @brief  获取算法字符串型参数
 *  @param  handle                 [IN]          句柄
 *  @param  strParamKeyName        [IN]          属性键值
 *  @param  strValue               [IN][OUT]     参数值
 *  @return 成功，返回MVID_CR_OK；错误，返回错误码 
 
 *  @~english
 *  @brief  Get Algorithm String Value
 *  @param  handle                 [IN]          Handle
 *  @param  strParamKeyName        [IN]          Param KeyName
 *  @param  strValue               [IN][OUT]     Value
 *  @return Success, return MVID_CR_OK. Failure, return error code
 ************************************************************************/
MVID_CODEREADER_API int __stdcall MVID_CR_Algorithm_GetStringValue(IN void* handle, IN const char* const strParamKeyName, IN OUT char* const strValue);

/************************************************************************
 *  @~chinese
 *  @brief  读码
 *  @param  handle                 [IN]          句柄
 *  @param  pstParam               [IN][OUT]     图片信息结构体
 *  @param  nCodeAbility           [IN]          读码能力集，一维码-MVID_BCR，二维码-MVID_TDCR，面单抠图-MVID_WAYBILL，使用或运算获取多种能力
 *  @return 成功，返回MVID_CR_OK；错误，返回错误码 
 
 *  @~english
 *  @brief  Barcode identification
 *  @param  handle                 [IN]          Handle
 *  @param  pstParam               [IN][OUT]     Param of input Image
 *  @param  nCodeAbility           [IN]          Code Ability, BCR-MVID_BCR, TDCR-MVID_TDCR, Waybill-MVID_WAYBILL, use or operations to acquire multiple abilities
 *  @return Success, return MVID_CR_OK. Failure, return error code
 ************************************************************************/
MVID_CODEREADER_API int __stdcall MVID_CR_Process(IN void* handle, IN OUT MVID_PROC_PARAM* pstParam , IN unsigned int nCodeAbility);

/************************************************************************
 *  @~chinese
 *  @brief  枚举设备
 *  @param  pstCamList             [IN][OUT]     设备列表
 *  @return 成功，返回MVID_CR_OK；错误，返回错误码 
 
 *  @~english
 *  @brief  Enumerate Device
 *  @param  pstCamList             [IN][OUT]     Device List
 *  @return Success, return MVID_CR_OK. Failure, return error code 
************************************************************************/
MVID_CODEREADER_API int __stdcall MVID_CR_CAM_EnumDevices(IN OUT MVID_CAMERA_INFO_LIST* pstCamList);

/************************************************************************
 *  @~chinese
 *  @brief  根据配置文件枚举指定设备
 *  @param  pstCamList             [IN][OUT]     设备列表
 *  @return 成功，返回MVID_CR_OK；错误，返回错误码 
 
 *  @~english
 *  @brief  Enumerate Specified Series Device
 *  @param  pstCamList             [IN][OUT]     Device List
 *  @return Success, return MVID_CR_OK. Failure, return error code 
************************************************************************/
MVID_CODEREADER_API int __stdcall MVID_CR_CAM_EnumDevicesByCfg(IN OUT MVID_CAMERA_INFO_LIST* pstCamList);

/************************************************************************
 *  @~chinese
 *  @brief  绑定设备
 *  @param  handle                 [IN]           句柄
 *  @param  pstCamInfo             [IN]           设备信息结构体
 *  @return 成功，返回MVID_CR_OK；错误，返回错误码 
 
 *  @~english
 *  @brief  Bind Device
 *  @param  handle                 [IN]           Handle
 *  @param  pstCamInfo             [IN]           Camera Information
 *  @return Success, return MVID_CR_OK. Failure, return error code
 ************************************************************************/
MVID_CODEREADER_API int __stdcall MVID_CR_CAM_BindDevice(IN void * handle, IN const MVID_CAMERA_INFO* pstCamInfo);

/************************************************************************
 *  @~chinese
 *  @brief  通过IP绑定设备
 *  @param  handle                 [IN]           句柄
 *  @param  chCurrentIp            [IN]           相机IP
 *  @param  chNetExport            [IN]           当前PC IP
 *  @return 成功，返回MVID_CR_OK；错误，返回错误码 
 
 *  @~english
 *  @brief  Bind Device By IP
 *  @param  handle                 [IN]           Handle
 *  @param  chCurrentIp            [IN]           Camera IP
 *  @param  chNetExport            [IN]           Current PC IP
 *  @return Success, return MVID_CR_OK. Failure, return error code
 ************************************************************************/
MVID_CODEREADER_API int __stdcall MVID_CR_CAM_BindDeviceByIP(IN void * handle, IN const char* chCurrentIp, IN const char* chNetExport);

/************************************************************************
 *  @~chinese
 *  @brief  通过序列号绑定设备
 *  @param  handle                 [IN]           句柄
 *  @param  chSerialNumber         [IN]           相机序列号
 *  @return 成功，返回MVID_CR_OK；错误，返回错误码 
 
 *  @~english
 *  @brief  Bind Device By SerialNumber
 *  @param  handle                 [IN]           Handle
 *  @param  chSerialNumber         [IN]           Camera Serial Number
 *  @return Success, return MVID_CR_OK. Failure, return error code
 ************************************************************************/
MVID_CODEREADER_API int __stdcall MVID_CR_CAM_BindDeviceBySerialNumber(IN void * handle, IN const char* chSerialNumber);

/***********************************************************************
 *  @~chinese
 *  @brief  注册图像数据回调，包含解码信息
 *  @param  handle                 [IN]          句柄
 *  @param  cbOutput               [IN]          回调函数指针
 *  @param  pUser                  [IN]          用户自定义变量
 *  @return 成功，返回MVID_CR_OK；错误，返回错误码
 
 *  @~english
 *  @brief  register image data callback, include barcode info
 *  @param  handle                 [IN]          Handle
 *  @param  cbOutput               [IN]          Callback function pointer
 *  @param  pUser                  [IN]          User defined variable
 *  @return Success, return MVID_CR_OK. Failure, return error code
 ***********************************************************************/
MVID_CODEREADER_API int __stdcall MVID_CR_CAM_RegisterImageCallBack(IN void* handle, 
                                                         IN void(__stdcall* cbOutput)(MVID_CAM_OUTPUT_INFO* pstOutput, void* pUser),
                                                         IN void* pUser);

/***********************************************************************
 *  @~chinese
 *  @brief  注册图像数据回调，不包含解码信息
 *  @param  handle                 [IN]          句柄
 *  @param  cbOutput               [IN]          回调函数指针
 *  @param  pUser                  [IN]          用户自定义变量
 *  @return 成功，返回MVID_CR_OK；错误，返回错误码
 
 *  @~english
 *  @brief  register image data callback, without barcode info
 *  @param  handle                 [IN]          Handle
 *  @param  cbOutput               [IN]          Callback function pointer
 *  @param  pUser                  [IN]          User defined variable
 *  @return Success, return MVID_CR_OK. Failure, return error code
 ***********************************************************************/
MVID_CODEREADER_API int __stdcall MVID_CR_CAM_RegisterImageBufferCallBack(IN void* handle, 
                                                         IN void(__stdcall* cbOutput)(MVID_IMAGE_INFO* pstOutput, void* pUser),
                                                         IN void* pUser);

/***********************************************************************
 *  @~chinese
 *  @brief  注册预处理图像数据回调
 *  @param  handle                 [IN]          句柄
 *  @param  cbPreOutput            [IN]          回调函数指针，预处理输入参数内存由SDK内部分配
 *  @param  pUser                  [IN]          用户自定义变量
 *  @return 成功，返回MVID_CR_OK；错误，返回错误码
 
 *  @~english
 *  @brief  register pretreatment image data callback
 *  @param  handle                 [IN]          Handle
 *  @param  cbPreOutput            [IN]          Callback function pointer，Preprocessing input parameter memory is allocated internally by the SDK
 *  @param  pUser                  [IN]          User defined variable
 *  @return Success, return MVID_CR_OK. Failure, return error code
 ***********************************************************************/
MVID_CODEREADER_API int __stdcall MVID_CR_CAM_RegisterPreImageCallBack(IN void* handle, 
                                                         IN void(__stdcall* cbPreOutput)(MVID_IMAGE_INFO* pstPreOutput, MVID_IMAGE_INFO* pstProcInput, void* pUser),
                                                         IN void* pUser);

/************************************************************************
 *  @~chinese
 *  @brief  注册全部事件回调，在打开设备之后调用,只支持GIGE
 *  @param  handle                 [IN]         设备句柄
 *  @param  cbEvent                [IN]         用户注册事件回调函数
 *  @param  pUser                  [IN]         用户自定义变量
 *  @return 成功，返回MVID_CR_OK；错误，返回错误码
 
 *  @~english
 *  @brief  Register event callback, which is called after the device is opened
 *  @param  handle:                [IN]         Device Handle
 *  @param  cbEvent                [IN]         Event CallBack Function Pointer
 *  @param  pUser                  [IN]         User defined variable
 *  @return Success, return MVID_CR_OK. Failure, return error code
************************************************************************/
MVID_CODEREADER_API int __stdcall MVID_CR_CAM_RegisterAllEventCallBack(IN void* handle, 
                                                         IN void(__stdcall* cbEvent)(MVID_EVENT_OUT_INFO * pEventInfo, void* pUser),
                                                         IN void* pUser);

/***********************************************************************
 *  @~chinese
 *  @brief  开始取流
 *  @param  handle                 [IN]          句柄
 *  @return 成功，返回MVID_CR_OK；错误，返回错误码
 
 *  @~english
 *  @brief  Start Grabbing
 *  @param  handle                 [IN]          Handle
 *  @return Success, return MVID_CR_OK. Failure, return error code
 ***********************************************************************/
MVID_CODEREADER_API int __stdcall MVID_CR_CAM_StartGrabbing(IN void* handle);

/***********************************************************************
 *  @~chinese
 *  @brief  停止取流
 *  @param  handle                 [IN]          句柄
 *  @return 成功，返回MVID_CR_OK；错误，返回错误码
 
 *  @~english
 *  @brief  Stop Grabbing
 *  @param  handle                 [IN]          Handle
 *  @return Success, return MVID_CR_OK. Failure, return error code
 ***********************************************************************/
MVID_CODEREADER_API int __stdcall MVID_CR_CAM_StopGrabbing(IN void* handle);

/***********************************************************************
 *  @~chinese
 *  @brief  采用超时机制获取一帧图片，SDK内部等待直到有数据时返回，包含解码信息
 *  @param  handle                 [IN]          句柄
 *  @param  pstFrameInfo           [IN][OUT]     图像信息结构体
 *  @param  nMsec                  [IN]          等待超时时间，单位：ms
 *  @return 成功，返回MVID_CR_OK；错误，返回错误码
 
 *  @~english
 *  @brief  Timeout mechanism is used to get image, and the SDK waits inside until the data is returned, include barcode info
 *  @param  handle                 [IN]          Handle
 *  @param  pstFrameInfo           [IN][OUT]     Image information structure
 *  @param  nMsec                  [IN]          Waiting timeout, Unit: ms
 *  @return Success, return MVID_CR_OK. Failure, return error code
 ***********************************************************************/
MVID_CODEREADER_API int __stdcall MVID_CR_CAM_GetOneFrameTimeout(IN void* handle, IN OUT MVID_CAM_OUTPUT_INFO* pstFrameInfo, IN unsigned int nMsec);

/***********************************************************************
 *  @~chinese
 *  @brief  采用超时机制获取一帧图片，SDK内部等待直到有数据时返回，不包含解码信息
 *  @param  handle                 [IN]          句柄
 *  @param  pImageInfo             [IN][OUT]     图像信息结构体
 *  @param  nMsec                  [IN]          等待超时时间
 *  @return 成功，返回MVID_CR_OK；错误，返回错误码
 
 *  @~english
 *  @brief  Timeout mechanism is used to get image, and the SDK waits inside until the data is returned, without barcode info
 *  @param  handle                 [IN]          Handle
 *  @param  pImageInfo             [IN][OUT]     Image information structure
 *  @param  nMsec                  [IN]          Waiting timeout
 *  @return Success, return MVID_CR_OK. Failure, return error code
 ***********************************************************************/
MVID_CODEREADER_API int __stdcall MVID_CR_CAM_GetImageBuffer(IN void* handle, IN OUT MVID_IMAGE_INFO* pImageInfo, IN unsigned int nMsec);

/************************************************************************
 *  @~chinese
 *  @brief  设置相机Int型属性值
 *  @param  handle                [IN]          相机句柄
 *  @param  strKey                [IN]          属性键值，如获取宽度信息则为"Width"
 *  @param  nValue                [IN]          想要设置的相机的属性值
 *  @return 成功,返回MVID_CR_OK,失败,返回错误码
 
 *  @~english
 *  @brief  Set Camera Int value
 *  @param  handle                [IN]          Handle
 *  @param  strKey                [IN]          Key value, for example, using "Width" to set width
 *  @param  nValue                [IN]          Feature value to set
 *  @return Success, return MVID_CR_OK. Failure, return error code
 ************************************************************************/
MVID_CODEREADER_API int __stdcall MVID_CR_CAM_SetIntValue(IN void* handle, IN const char* strKey, IN int64_t nValue);

/************************************************************************
 *  @~chinese
 *  @brief  获取Int属性值
 *  @param  handle                [IN]              相机句柄
 *  @param  strKey                [IN]              属性键值，如获取宽度信息则为"Width"
 *  @param  pIntValue             [IN][OUT]         返回给调用者有关相机属性结构体指针
 *  @return 成功,返回MVID_CR_OK,失败,返回错误码
 
 *  @~english
 *  @brief  Get Int value
 *  @param  handle                [IN]              Handle
 *  @param  strKey                [IN]              Key value, for example, using "Width" to get width
 *  @param  pIntValue             [IN][OUT]         Structure pointer of camera features
 *  @return Success, return MVID_CR_OK. Failure, return error code
 ************************************************************************/
MVID_CODEREADER_API int __stdcall MVID_CR_CAM_GetIntValue(IN void* handle, IN const char* strKey, IN OUT MVID_CAM_INTVALUE_EX *pIntValue);

/************************************************************************
 *  @~chinese
 *  @brief  设置相机Enum型属性值
 *  @param  handle                [IN]        相机句柄
 *  @param  strKey                [IN]        属性键值，如获取像素格式信息则为"PixelFormat"
 *  @param  nValue                [IN]        想要设置的相机的属性值
 *  @return 成功,返回MVID_CR_OK,失败,返回错误码
 
 *  @~english
 *  @brief  Set Camera Enum value
 *  @param  handle                [IN]        Handle
 *  @param  strKey                [IN]        Key value, for example, using "PixelFormat" to set pixel format
 *  @param  nValue                [IN]        Feature value to set
 *  @return Success, return MVID_CR_OK. Failure, return error code
 ************************************************************************/
MVID_CODEREADER_API int __stdcall MVID_CR_CAM_SetEnumValue(IN void* handle, IN const char* strKey, IN unsigned int nValue);

/************************************************************************
 *  @~chinese
 *  @brief  通过字符串设置相机Enum型属性值
 *  @param  handle                [IN]        相机句柄
 *  @param  strKey                [IN]        属性键值，如获取像素格式信息则为"PixelFormat"
 *  @param  strValue              [IN]        想要设置的相机的属性字符串
 *  @return 成功,返回MVID_CR_OK,失败,返回错误码
 
 *  @~english
 *  @brief  Set Camera Enum value by string
 *  @param  handle                [IN]        Handle
 *  @param  strKey                [IN]        Key value, for example, using "PixelFormat" to set pixel format
 *  @param  strValue              [IN]        Feature String to set
 *  @return Success, return MVID_CR_OK. Failure, return error code
 ************************************************************************/
MVID_CODEREADER_API int __stdcall MVID_CR_CAM_SetEnumValueByString(IN void* handle, IN const char* strKey, IN const char* strValue);

/************************************************************************
 *  @~chinese
 *  @brief  获取相机Enum属性值
 *  @param  handle                   [IN]        相机句柄
 *  @param  strKey                   [IN]        属性键值，如获取像素格式信息则为"PixelFormat"
 *  @param  pEnumValue               [IN][OUT]   返回给调用者有关相机属性结构体指针
 *  @return 成功,返回MVID_CR_OK,失败,返回错误码
 
 *  @~english
 *  @brief  Get Camera Enum value
 *  @param  handle                   [IN]        Handle
 *  @param  strKey                   [IN]        Key value, for example, using "PixelFormat" to get pixel format
 *  @param  pEnumValue               [IN][OUT]   Structure pointer of camera features
 *  @return Success, return MVID_CR_OK. Failure, return error code
 ************************************************************************/
MVID_CODEREADER_API int __stdcall MVID_CR_CAM_GetEnumValue(IN void* handle, IN const char* strKey, IN OUT MVID_CAM_ENUMVALUE *pEnumValue);

/************************************************************************
 *  @~chinese
 *  @brief  设置相机Float型属性值
 *  @param  handle                [IN]        相机句柄
 *  @param  strKey                [IN]        属性键值
 *  @param  fValue                [IN]        想要设置的相机的属性值
 *  @return 成功,返回MVID_CR_OK,失败,返回错误码
 
 *  @~english
 *  @brief  Set Camera Float value
 *  @param  handle                [IN]        Handle
 *  @param  strKey                [IN]        Key value
 *  @param  fValue                [IN]        Feature value to set
 *  @return Success, return MVID_CR_OK. Failure, return error code
 ************************************************************************/
MVID_CODEREADER_API int __stdcall MVID_CR_CAM_SetFloatValue(IN void* handle, IN const char* strKey, IN float fValue);

/************************************************************************
 *  @~chinese
 *  @brief  获取相机Float属性值
 *  @param  handle                     [IN]        相机句柄
 *  @param  strKey                     [IN]        属性键值
 *  @param  pFloatValue                [IN][OUT]   返回给调用者有关相机属性结构体指针
 *  @return 成功,返回MVID_CR_OK,失败,返回错误码
 
 *  @~english
 *  @brief  Get Cmera Float value
 *  @param  handle                     [IN]        Handle
 *  @param  strKey                     [IN]        Key value
 *  @param  pFloatValue                [IN][OUT]   Structure pointer of camera features
 *  @return Success, return MVID_CR_OK. Failure, return error code
 ************************************************************************/
MVID_CODEREADER_API int __stdcall MVID_CR_CAM_GetFloatValue(IN void* handle, IN const char* strKey, IN OUT MVID_CAM_FLOATVALUE *pFloatValue);

/************************************************************************
 *  @~chinese
 *  @brief  设置相机String型属性值
 *  @param  handle                  [IN]        相机句柄
 *  @param  strKey                  [IN]        属性键值
 *  @param  sValue                  [IN]        想要设置的相机的属性值
 *  @return 成功,返回MVID_CR_OK,失败,返回错误码
 
 *  @~english
 *  @brief  Set Camera String value
 *  @param  handle                  [IN]        Handle
 *  @param  strKey                  [IN]        Key value
 *  @param  sValue                  [IN]        Feature value to set
 *  @return Success, return MVID_CR_OK. Failure, return error code
 ************************************************************************/
MVID_CODEREADER_API int __stdcall MVID_CR_CAM_SetStringValue(IN void* handle, IN const char* strKey, IN const char * sValue);

/************************************************************************
 *  @~chinese
 *  @brief  获取相机String属性值
 *  @param  handle                       [IN]        相机句柄
 *  @param  strKey                       [IN]        属性键值
 *  @param  pStringValue                 [IN][OUT]   返回给调用者有关相机属性结构体指针
 *  @return 成功,返回MVID_CR_OK,失败,返回错误码
 
 *  @~english
 *  @brief  Get Camera String value
 *  @param  handle                       [IN]        Handle
 *  @param  strKey                       [IN]        Key value
 *  @param  pStringValue                 [IN][OUT]   Structure pointer of camera features
 *  @return Success, return MVID_CR_OK. Failure, return error code
 ************************************************************************/
MVID_CODEREADER_API int __stdcall MVID_CR_CAM_GetStringValue(IN void* handle, IN const char* strKey, IN OUT MVID_CAM_STRINGVALUE *pStringValue);

/************************************************************************
 *  @~chinese
 *  @brief  设置相机Boolean型属性值
 *  @param  handle                [IN]        相机句柄
 *  @param  strKey                [IN]        属性键值
 *  @param  bValue                [IN]        想要设置的相机的属性值
 *  @return 成功,返回MVID_CR_OK,失败,返回错误码
 
 *  @~english
 *  @brief  Set Camera Boolean value
 *  @param  handle                [IN]        Handle
 *  @param  strKey                [IN]        Key value
 *  @param  bValue                [IN]        Feature value to set
 *  @return Success, return MVID_CR_OK. Failure, return error code
 ************************************************************************/
MVID_CODEREADER_API int __stdcall MVID_CR_CAM_SetBoolValue(IN void* handle, IN const char* strKey, IN bool bValue);

/************************************************************************
 *  @~chinese
 *  @brief  获取相机Boolean属性值
 *  @param  handle                     [IN]        相机句柄
 *  @param  strKey                     [IN]        属性键值
 *  @param  pBoolValue                 [IN][OUT]   返回给调用者有关相机属性值
 *  @return 成功,返回MVID_CR_OK,失败,返回错误码
 
 *  @~english
 *  @brief  Get Camera Boolean value
 *  @param  handle                     [IN]        Handle
 *  @param  strKey                     [IN]        Key value
 *  @param  pBoolValue                 [IN][OUT]   Structure pointer of camera features
 *  @return Success, return MVID_CR_OK. Failure, return error code
 ************************************************************************/
MVID_CODEREADER_API int __stdcall MVID_CR_CAM_GetBoolValue(IN void* handle, IN const char* strKey, IN OUT bool *pBoolValue);

/************************************************************************
 *  @~chinese
 *  @brief  设置相机Command型属性值
 *  @param  handle                  [IN]        相机句柄
 *  @param  strKey                  [IN]        属性键值
 *  @return 成功,返回MVID_CR_OK,失败,返回错误码
 
 *  @~english
 *  @brief  Set Camera Command value
 *  @param  handle                  [IN]        Handle
 *  @param  strKey                  [IN]        Key value
 *  @return Success, return MVID_CR_OK. Failure, return error code
 ************************************************************************/
MVID_CODEREADER_API int __stdcall MVID_CR_CAM_SetCommandValue(IN void* handle, IN const char* strKey);

/***********************************************************************
 *  @~chinese
 *  @brief  设置SDK内部图像缓存节点个数，大于等于1，小于等于20，在抓图前调用
 *  @param  handle                      [IN]            设备句柄
 *  @param  nNum                        [IN]            缓存节点个数
 *  @return 成功，返回MVID_CR_OK；错误，返回错误码
 
 *  @~english
 *  @brief  Set the number of the internal image cache nodes in SDK, Greater than or equal to 1, less than or equal 20, to be called before the capture
 *  @param  handle                      [IN]            Device handle
 *  @param  nNum                        [IN]            Image Node Number
 *  @return Success, return MVID_CR_OK. Failure, return error code
 ***********************************************************************/
MVID_CODEREADER_API int __stdcall MVID_CR_CAM_SetImageNodeNum(IN void* handle, unsigned int nNum);

/***********************************************************************
 *  @~chinese
 *  @brief  设置图像输出模式(MVID_OUTPUT_NORMAL - 非MONO8均转换为MVID_IMAGE_BGR24, MVID_OUTPUT_RAW - 以原始图像格式输出)
 *  @param  handle                      [IN]            设备句柄
 *  @param  nNum                        [IN]            图像输出模式
 *  @return 成功，返回MVID_CR_OK；错误，返回错误码
 
 *  @~english
 *  @brief  Set Image OutPutMode
 *  @param  handle                      [IN]            Device handle
 *  @param  nNum                        [IN]            Image OutPut Mode
 *  @return Success, return MVID_CR_OK. Failure, return error code
 ***********************************************************************/
MVID_CODEREADER_API int __stdcall MVID_CR_CAM_SetImageOutPutMode(IN void* handle, IN MVID_IMAGE_OUTPUT_MODE enImageOutPutMode);

/************************************************************************
 *  @~chinese
 *  @brief  注册异常消息回调，在打开设备之后调用
 *  @param  handle            [IN]      设备句柄
 *  @param  cbException       [IN]      异常回调函数指针
 *  @param  pUser             [IN]      用户自定义变量
 *  @return 见返回错误码
 
 *  @~english
 *  @brief  Register Exception Message CallBack, call after open device
 *  @param  handle            [IN]       Device handle
 *  @param  cbException       [IN]       Exception Message CallBack Function Pointer
 *  @param  pUser             [IN]       User defined variable
 *  @return Refer to error code
************************************************************************/
MVID_CODEREADER_API int __stdcall MVID_CR_RegisterExceptionCallBack(IN void* handle, 
                                                                    IN void(__stdcall* cbException)(unsigned int nMsgType, void* pUser),
                                                                    IN void* pUser);

/************************************************************************
 *  @~chinese
 *  @brief  保存图片，支持Bmp和Jpeg.编码质量在50-99之间
 *  @param  handle                 [IN]           句柄
 *  @param  pstInputImage          [IN]           输入图片参数结构体,支持Mono8/BGR24格式图像
 *  @param  enImageType            [IN]           目标转换类型，默认为Jpeg
 *  @param  pstOutputImage         [IN][OUT]      输出图片参数结构体
 *  @param  nJpgQuality            [IN]           JPG压缩质量，默认为80，若目标转换类型为BMP则该参数无效
 *  @return 成功，返回MVID_CR_OK；错误，返回错误码 
 
 *  @~english
 *  @brief  Save image, support Bmp and Jpeg. Encoding quality, [50-99]
 *  @param  handle                 [IN]           Handle
 *  @param  pstInputImage          [IN]           Input image parameters structure
 *  @param  enImageType            [IN]           Convery image type，default Jpeg
 *  @param  pstOutputImage         [IN][OUT]      OutPut image parameters structure
 *  @param  nJpgQuality            [IN]           Jpg quality，default quality 80, no use for Bmp image
 *  @return Success, return MVID_CR_OK. Failure, return error code
 ************************************************************************/
#ifndef __cplusplus
MVID_CODEREADER_API int __stdcall MVID_CR_SaveImage(IN void* handle, IN MVID_IMAGE_INFO* pstInputImage, IN MVID_IMAGE_TYPE enImageType, IN OUT MVID_IMAGE_INFO* pstOutputImage, IN unsigned int nJpgQuality);
#else
MVID_CODEREADER_API int __stdcall MVID_CR_SaveImage(IN void* handle, IN MVID_IMAGE_INFO* pstInputImage, IN MVID_IMAGE_TYPE enImageType, IN OUT MVID_IMAGE_INFO* pstOutputImage, IN unsigned int nJpgQuality = 80);
#endif

/************************************************************************
 *  @~chinese
 *  @brief  读取图片文件，转换为MVID_IMAGE_MONO8格式，可用于MVID_CR_Process
 *  @param  handle                 [IN]           句柄
 *  @param  pFilePath              [IN]           输入图片文件路径
 *  @param  pstImageParam          [IN][OUT]      输出图片参数结构体
 *  @return 成功，返回MVID_CR_OK；错误，返回错误码 
 *  @remarks 支持Raw、Bmp、Jpeg格式图片，Raw图片需要传入宽、高及像素格式
 
 *  @~english
 *  @brief  Read image file, convert to MVID_IMAGE_MONO8 format, can be used for MVID_CR_Process
 *  @param  handle                 [IN]           Handle
 *  @param  pFilePath              [IN]           Input image parameters structure
 *  @param  pstImageParam          [IN][OUT]      OutPut image parameters structure
 *  @return Success, return MVID_CR_OK. Failure, return error code
 *  @remarks Support Raw, Bmp, Jpeg format image, Raw image require incoming width, height, and pixel format
 ************************************************************************/
MVID_CODEREADER_API int __stdcall MVID_CR_GetImageFileData(IN void* handle, IN const char* pFilePath, IN OUT MVID_PROC_PARAM* pstImageParam);

/***********************************************************************
 *  @~chinese
 *  @brief      过滤规则导入
 *  @param      handle                 [IN]          句柄
 *  @param      pFilePath              [IN]          过滤规则文件路径，设置NULL为取消过滤
 *  @return     成功，返回MVID_CR_OK；错误，返回错误码
 
 *  @~english
 *  @brief      Load Rule File
 *  @param      handle                 [IN]          Handle
 *  @param      pFilePath              [IN]          FileName of Rule
 *  @return     Success, return MVID_CR_OK. Failure, return error code
 ***********************************************************************/
MVID_CODEREADER_API int __stdcall MVID_CR_RuleLoad(IN void* handle, IN const char* pFilePath);

/***********************************************************************
 *  @~chinese
 *  @brief      脚本文件导入
 *  @param      handle                 [IN]          句柄
 *  @param      pstrFilePath           [IN]          脚本文件路径，设置NULL为取消过滤
 *  @param      pstrFuncName           [IN]          过滤函数名称
 *  @return     成功，返回MVID_CR_OK；错误，返回错误码
 
 *  @~english
 *  @brief      Load Script File
 *  @param      handle                 [IN]          Handle
 *  @param      pstrFilePath           [IN]          FilePath of Script
 *  @param      pstrFuncName           [IN]          Filter Function Name
 *  @return     Success, return MVID_CR_OK. Failure, return error code
 ***********************************************************************/
MVID_CODEREADER_API int __stdcall MVID_CR_ScriptLoad(IN void* handle, IN const char* pstrFilePath, IN const char* pstrFuncName);

/***********************************************************************
 *  @~chinese
 *  @brief  获取各种类型的信息
 *  @param  handle                      [IN]            设备句柄
 *  @param  pstInfo                     [IN][OUT]       返回给调用者有关设备各种类型的信息结构体指针
 *  @return 成功，返回MVID_CR_OK；错误，返回错误码
 *  @remarks 接口里面输入需要获取的信息类型（指定MVID_ALL_MATCH_INFO结构体中的nType类型），获取对应的信息（在MVID_ALL_MATCH_INFO结构体中pInfo里返回）
    该接口的调用前置条件取决于所获取的信息类型，获取GigE设备的MVID_MATCH_TYPE_NET_DETECT信息需在开启抓图之后调用，
    获取U3V设备的MVID_MATCH_TYPE_USB_DETECT信息需在打开设备之后调用。该接口不支持CameraLink设备。
 
 *  @~english
 *  @brief  Get various type of information
 *  @param  handle                      [IN]            Device handle
 *  @param  pstInfo                     [IN][OUT]       Structure pointer of various type of information
 *  @return Success, return MVID_CR_OK. Failure, return error code
 *  @remarks Input required information type (specify nType in structure MVID_ALL_MATCH_INFO) in the interface and get corresponding information (return in pInfo of structure MVID_ALL_MATCH_INFO). 
    The calling precondition of this interface is determined by obtained information type. Call after enabling capture to get MVID_MATCH_TYPE_NET_DETECT information of GigE device, 
    and call after starting device to get MV_MATCH_TYPE_USB_DETECT information of USB3Vision device. This API is not supported by CameraLink device. 
***********************************************************************/
MVID_CODEREADER_API int __stdcall MVID_CR_CAM_GetAllMatchInfo(IN void* handle, IN OUT MVID_ALL_MATCH_INFO* pstInfo);

#ifdef __cplusplus
}
#endif 

#endif //_MVID_CODEREADER_H_
