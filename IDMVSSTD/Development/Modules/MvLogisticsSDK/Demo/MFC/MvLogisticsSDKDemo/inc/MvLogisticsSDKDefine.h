
#ifndef _MV_LOGISTICS_DEFINE_H_
#define _MV_LOGISTICS_DEFINE_H_


// 正确码定义
#define MV_LGS_OK                       0x00000000      ///< ch:成功, 无错误 | en:Successed, no error

// 通用错误码定义:范围0x80110000-0x801100FF
#define MV_LGS_E_HANDLE                 0x80110000      ///< ch:错误或无效的句柄 | en:Error or invalid handle
#define MV_LGS_E_SUPPORT                0x80110001      ///< ch:不支持的功能 | en:The function is not supported
#define MV_LGS_E_BUFOVER                0x80110002      ///< ch:缓存已满 | en:Buffer is full
#define MV_LGS_E_CALLORDER              0x80110003      ///< ch:函数调用顺序错误 | en:Incorrect calling sequence
#define MV_LGS_E_PARAMETER              0x80110004      ///< ch:错误的参数 | en:Incorrect parameter
#define MV_LGS_E_RESOURCE               0x80110005      ///< ch:资源申请失败 | en:Applying resource failed
#define MV_LGS_E_NODATA                 0x80110006      ///< ch:无数据 | en:No data
#define MV_LGS_E_PRECONDITION           0x80110007      ///< ch:前置条件有误，或运行环境已发生变化 | en:Precondition error, or running environment changed
#define MV_LGS_E_ENCRYPT                0x80110008      ///< ch:凭证错误，可能是未插加密狗，或加密狗过期 | en:Credential error, possibly because the dongle was not installed or expired
#define MV_LGS_E_RULE                   0x8011000a      ///< ch:过滤规则相关的错误 | en:Filter rule error
#define MV_LGS_E_JPGENC                 0x80110012      ///< ch:jpg编码相关错误 | en:Jpg encoding error
#define MV_LGS_E_IMAGE                  0x80110013      ///< ch:输入的图像数据有损或图像格式,宽高错误 | en:Abnormal image. Incomplete image caused by packet loss or incorrect image format, width, or height
#define MV_LGS_E_CONFIG                 0x80110014      ///< ch:配置文件有误 | en:Config file error
#define MV_LGS_E_UNKNOW                 0x801100FF      ///< ch:未知的错误 | en:Unknown error

#define MV_LGS_E_CAMERA                 0x80112100      ///< ch:相机相关的错误 | en:Camera error
#define MV_LGS_E_BCR                    0x80112200      ///< ch:一维码相关错误 | en:1D barcode error
#define MV_LGS_E_TDCR                   0x80112300      ///< ch:二维码相关错误 | en:2D barcode error
#define MV_LGS_E_WAYBILL                0x80112400      ///< ch:抠图相关错误 | en:Matting error
#define MV_LGS_E_SCRIPT                 0x80112500      ///< ch:脚本规则相关错误 | en:Script rule error

// GenICam系列错误:范围0x80110100-0x801101FF
#define MV_LGS_E_GC_GENERIC             0x80110100      ///< ch:通用错误 | en:General error
#define MV_LGS_E_GC_ARGUMENT            0x80110101      ///< ch:参数非法 | en:Invalid parameter
#define MV_LGS_E_GC_RANGE               0x80110102      ///< ch:值超出范围 | en:The value is out of range
#define MV_LGS_E_GC_PROPERTY            0x80110103      ///< ch:属性错误 | en:Attribute error
#define MV_LGS_E_GC_RUNTIME             0x80110104      ///< ch:运行环境有问题 | en:Running environment error
#define MV_LGS_E_GC_LOGICAL             0x80110105      ///< ch:逻辑错误 | en:Incorrect logic
#define MV_LGS_E_GC_ACCESS              0x80110106      ///< ch:节点访问条件有误 | en:Node accessing condition error
#define MV_LGS_E_GC_TIMEOUT             0x80110107      ///< ch:超时 | en:Timeout
#define MV_LGS_E_GC_DYNAMICCAST         0x80110108      ///< ch:转换异常 | en:Transformation exception
#define MV_LGS_E_GC_UNKNOW              0x801101FF      ///< ch:GenICam未知错误 | en:GenICam unknown error

// GigE_STATUS对应的错误码:范围0x80110200-0x801102FF
#define MV_LGS_E_NOT_IMPLEMENTED        0x80110200      ///< ch:命令不被设备支持 | en:The command is not supported by device
#define MV_LGS_E_INVALID_ADDRESS        0x80110201      ///< ch:访问的目标地址不存在 | en:Target address does not exist
#define MV_LGS_E_WRITE_PROTECT          0x80110202      ///< ch:目标地址不可写 | en:The target address is not writable
#define MV_LGS_E_ACCESS_DENIED          0x80110203      ///< ch:设备无访问权限 | en:No access permission
#define MV_LGS_E_BUSY                   0x80110204      ///< ch:设备忙，或网络断开 | en:Device is busy, or network is disconnected
#define MV_LGS_E_PACKET                 0x80110205      ///< ch:网络包数据错误 | en:Network packet error
#define MV_LGS_E_NETER                  0x80110206      ///< ch:网络相关错误 | en:Network error

// GigE相机特有的错误码
#define MV_LGS_E_IP_CONFLICT            0x80110221      ///< ch:设备IP冲突 | en:IP address conflicted

// USB_STATUS对应的错误码:范围0x80110300-0x801103FF
#define MV_LGS_E_USB_READ               0x80110300      ///< ch:读usb出错 | en:USB read error
#define MV_LGS_E_USB_WRITE              0x80110301      ///< ch:写usb出错 | en:USB write error
#define MV_LGS_E_USB_DEVICE             0x80110302      ///< ch:设备异常 | en:Device exception
#define MV_LGS_E_USB_GENICAM            0x80110303      ///< ch:GenICam相关错误 | en:GenICam error
#define MV_LGS_E_USB_BANDWIDTH          0x80110304      ///< ch:带宽不足  该错误码新增 | en:Insufficient bandwidth, this error code is newly added
#define MV_LGS_E_USB_DRIVER             0x80110305      ///< ch:驱动不匹配或者未装驱动 | en:Driver is mismatched, or is not installed
#define MV_LGS_E_USB_UNKNOW             0x801103FF      ///< ch:USB未知的错误 | en:USB unknown error

// 融合模块错误码
#define MV_LGS_E_FUSION_PARAM           0x80112600      ///< ch:融合模块参数错误 | en:Fusion module parameter error
#define MV_LGS_E_FUSION_MALLOC          0x80112601      ///< ch:融合模块内存分配失败 | en:Fusion module memory allocation failed
#define MV_LGS_E_FUSION_CALLORDER       0x80112602      ///< ch:融合模块调用顺序错误 | en:Fusion module call sequence is wrong
#define MV_LGS_E_FUSION_CFGFILE         0x80112603      ///< ch:融合模块配置文件错误 | en:Fusion module configuration file error
#define MV_LGS_E_FUSION_UNKNOWN         0x80112604      ///< ch:融合模块未知错误 | en:Unknown error of fusion module
#define MV_LGS_E_FUSION_LACKBUF         0x80112605      ///< ch:融合模块缓存不足 | en:Insufficient Fusion Module Cache
#define MV_LGS_E_FUSION_SUPPORT         0x80112606      ///< ch:融合模块不支持 | en:Fusion module does not support

// 体积模块错误码
#define MV_LGS_E_VOLMEASURE_PARAM       0x80112700      ///< ch:体积模块参数错误 | en:Volume module parameter error
#define MV_LGS_E_VOLMEASURE_MALLOC      0x80112701      ///< ch:体积模块内存分配失败 | en:Volume module memory allocation failed
#define MV_LGS_E_VOLMEASURE_CALLORDER   0x80112702      ///< ch:体积模块调用顺序错误 | en:Volume module calling sequence is wrong
#define MV_LGS_E_VOLMEASURE_NODATA      0x80112703      ///< ch:体积模块无数据 | en:No data for volume module
#define MV_LGS_E_VOLMEASURE_CFGFILE     0x80112704      ///< ch:体积模块配置文件错误 | en:Volume module configuration file error
#define MV_LGS_E_VOLMEASURE_NOPKG       0x80112705      ///< ch:体积模块无包裹 | en:Volume module without package
#define MV_LGS_E_VOLMEASURE_UNKNOWN     0x80112706      ///< ch:体积模块未知错误 | en:Volume module unknown error
#define MV_LGS_E_VOLMEASURE_LACKBUF     0x80112707      ///< ch:体积模块缓存不足 | en:Insufficient volume module cache
#define MV_LGS_E_VOLMEASURE_SUPPORT     0x80112708      ///< ch:体积模块不支持 | en:Volume module does not support

// 称重模块错误码
#define MV_LGS_E_WGHT_OPEN              0x80112800      ///< ch:称重模块称重设备打开失败 | en:Weighing module weighing equipment failed to open
#define MV_LGS_E_WGHT_ENC               0x80112801      ///< ch:称重模块加密错误 | en:Weighing module encryption error
#define MV_LGS_E_WGHT_RESOURCE          0x80112802      ///< ch:称重模块资源初始化失败 | en:Weighing module resource initialization failed
#define MV_LGS_E_WGHT_CALLORDER         0x80112803      ///< ch:称重模块调用顺序错误 | en:Weighing module call sequence error
#define MV_LGS_E_WGHT_NULL              0x80112804      ///< ch:称重模块指针类型参数为空 | en:Weighing module pointer type parameter is null
#define MV_LGS_E_WGHT_RANGE             0x80112805      ///< ch:称重模块数值类型参数范围错误 | en:Weighing module numerical type parameter range error
#define MV_LGS_E_WGHT_ENABLE            0x80112806      ///< ch:称重模块能力集错误 | en:Weighing module capability set error
#define MV_LGS_E_WGHT_UNKNOW            0x80112807      ///< ch:称重模块其他内部错误 | en:Weighing module other internal errors


#ifndef IN
    #define IN
#endif

#ifndef OUT
    #define OUT
#endif

//跨平台定义
//Cross Platform Definition
#ifdef WIN32
typedef __int64 int64_t;
typedef unsigned __int64 uint64_t;
#else
#include <stdint.h>
#endif

#ifndef __cplusplus
typedef char    bool;
#define true    1
#define false   0
#endif


// 异常消息类型
#define MV_LGS_EXCEPTION_DEV_DISCONNECT          0x00008001      // ch:设备断开连接 | en:The device is disconnected
#define MV_LGS_EXCEPTION_SOFTDOG_DISCONNECT      0x00008002      // ch:加密狗掉线 | en:The softdog is disconnected
#define MV_LGS_EXCEPTION_RECONNECT_DEV_SUCCESS   0x00008000      // 相机重连成功

#define MVLGS_MAX_CODECHARATERLEN               4096    // ch:最大条码长度 | en:Maximum barcode length
#define MVLGS_MAX_CODENUM                       256     // ch:单张图片内最大条码个数 | en:Maximum number of barcodes in a single picture
#define MVLGS_MAX_CODELISTNUM                   24      // ch:最大条码信息列表个数 | en:Maximum number of barcode information lists

#define MV_LGS_BEGIN_TRIGGER                    1       // ch:开始触发 | en:Start trigger
#define MV_LGS_STOP_TRIGGER                     0       // ch:结束触发 | en:Stop trigger

// ch:条码类型 | en:Code type
typedef enum _MVLGS_CODE_TYPE_
{
    MVLGS_CODE_NONE             = 0,          // ch:无可识别条码 | en:No recognizable bar code

    // 二维码
    MVLGS_CODE_TDCR_DM          = 1,          // ch:DM码 | en:DM code
    MVLGS_CODE_TDCR_QR          = 2,          // ch:QR码 | en:QR code

    // 一维码
    MVLGS_CODE_BCR_EAN8         = 8,          // ch:EAN8码 | en:EAN8 code
    MVLGS_CODE_BCR_UPCE         = 9,          // ch:UPCE码 | en:UPCE code
    MVLGS_CODE_BCR_UPCA         = 12,         // ch:UPCA码 | en:UPCA code
    MVLGS_CODE_BCR_EAN13        = 13,         // ch:EAN13码 | en:EAN13 code
    MVLGS_CODE_BCR_ISBN13       = 14,         // ch:ISBN13码 | en:ISBN13 code
    MVLGS_CODE_BCR_CODABAR      = 20,         // ch:库德巴码 | en:Codabar code
    MVLGS_CODE_BCR_ITF25        = 25,         // ch:交叉25码 | en:ITF25 code
    MVLGS_CODE_BCR_CODE39       = 39,         // ch:Code 39 | en:Code 39
    MVLGS_CODE_BCR_CODE93       = 93,         // ch:Code 93 | en:Code 93
    MVLGS_CODE_BCR_CODE128      = 128,        // ch:Code 128 | en:Code 128
} MVLGS_CODE_TYPE;

// ch:图像格式 | en:Image Type
typedef enum _MVLGS_IMAGE_TYPE_
{
    MVLGS_IMAGE_Undefined        = 0,         // ch:未定义 | en:Undefined format
    MVLGS_IMAGE_MONO8            = 1,         // ch:Mono8 | en:MONO8 format
    MVLGS_IMAGE_JPEG             = 2,         // ch:JPEG | en:JPEG format
    MVLGS_IMAGE_BMP              = 3,         // ch:Bmp | en:BMP format
    MVLGS_IMAGE_RGB24            = 4,         // ch:RGB24 | en:RGB format
    MVLGS_IMAGE_BGR24            = 5,         // ch:BGR24 | en:BGR format
}MVLGS_IMAGE_TYPE;

// ch:设备类型 | en:Device type
typedef enum _MVLGS_DEVICE_TYPE_
{
    MVLGS_IDCAM                 = 0,         // ch:工业相机 | en:industrial camera
    MVLGS_CODEREADER            = 1,         // ch:读码器 | en:Barcode reader
    MVLGS_LINESCAN              = 2,         // ch:线扫相机 | en:Line Scan Camera
    MVLGS_PANORAMIC             = 3,         // ch:全景相机 | en:Panoramic camera
    MVLGS_VOLUME                = 4,         // ch:体积相机 | en:Volume Camera
    MVLGS_WEIGHT                = 5,         // ch:称重 | en:Weighting(暂不支持)
}MVLGS_DEVICE_TYPE;

// ch:条码位置坐标 | en:Barcode position coordinates
typedef struct _MVLGS_POINT_I_
{
    int             nX;                     // ch:X坐标 | en:X coordinate
    int             nY;                     // ch:Y坐标 | en:Y coordinate
} MVLGS_POINT_I;

// ch:条码信息 | en:Code information
typedef struct _MVLGS_CODE_INFO_
{
    unsigned char           strCode[MVLGS_MAX_CODECHARATERLEN];     // ch:字符 | en:Character, maximum size: 4096
    int                     nLen;                                   // ch:字符长度 | en:Character size
    MVLGS_CODE_TYPE         enBarType;                              // ch:条码类型 | en:Bar code type
    MVLGS_POINT_I           stCornerPt[4];                          // ch:条码位置 | en:Bar code location
    int                     nAngle;                                 // ch:条码角度（0~3600） | en:Bar code angle, range: [0, 3600°]
    int                     nFilterFlag;                            // ch:过滤码标识(0为正常码, 1为过滤码) | en:Filter identifier: 0- normal code, 1-filter code
    char                    strSerialNumber[32];                    // ch:相机序列号 | en:Camera serial number

    unsigned int            nReserved[23];                          // ch:保留 | en:Reserved
}MVLGS_CODE_INFO;

// ch:输出图像的信息 | en:Output Frame Information
typedef struct _MVLGS_IMAGE_INFO_
{
    unsigned char*      pImageBuf;              // ch:原始图像缓存，由用户传入 | en:Original image buffer
    unsigned int        nImageLen;              // ch:原始图像长度 | en:Original image size
    MVLGS_IMAGE_TYPE    enImageType;            // ch:图像格式 | en:Image Type
    unsigned short      nWidth;                 // ch:图像宽 | en:Image Width
    unsigned short      nHeight;                // ch:图像高 | en:Image Height

    unsigned int        nTriggerIndex;          // ch:触发序号，特殊场合可当做包裹号使用 | en:Trigger serial number, can be used as a parcel number in special occasions
    unsigned int        nFrameNum;              // ch:帧号 | en:Frame No.
    unsigned int        nDevTimeStampHigh;      // ch:时间戳高32位 | en:Timestamp high 32 bits
    unsigned int        nDevTimeStampLow;       // ch:时间戳低32位 | en:Timestamp low 32 bits

    unsigned int        nReserved[8];           // ch:保留 | en:Reserved
}MVLGS_IMAGE_INFO;

// ch:条码信息列表 | en:List of barcode information
typedef struct _MVLGS_CODE_LIST_
{
    int                 nCodeNum;                       // ch:条码数量 | en:Number of barcodes
    MVLGS_CODE_INFO     stCodeInfo[MVLGS_MAX_CODENUM];  // ch:条码信息 | en:Barcode information

    MVLGS_IMAGE_INFO    stImage;                        // ch:原始图像信息 | en:Original image information

    unsigned char*      pImageWaybill;                  // ch:抠图缓存，由SDK内部分配 | en:Cutout cache, allocated internally by the SDK
    unsigned int        nImageWaybillLen;               // ch:图像大小 | en:Image size
    MVLGS_IMAGE_TYPE    enWaybillImageType;             // ch:抠图图像格式 | en:Matte image format

    unsigned int        nReserved[7];                   // ch:保留 | en:Reserved
}MVLGS_CODE_LIST;

// ch:包裹体积字段信息 | en:Package volume field information
typedef struct _MVLGS_VOLUME_INFO_
{
    unsigned int        nPkgID;                 // ch:包裹ID | en:Package ID
    float               fVolume;                // ch:体积 | en: volume
    float               fLength;                // ch:长度 | en: length
    float               fWidth;                 // ch:宽度 | en:Width
    float               fHeight;                // ch:高度 | en:Height
    int64_t             nObjEnterSysTime;       // ch:体积测量开始时间戳 | en:Volume measurement start timestamp
    int64_t             nObjLeaveSysTime;       // ch:体积测量结束时间戳 | en:Volume measurement end timestamp

    unsigned int        nReserved[60];          // ch:保留 | en:Reserved
}MVLGS_VOLUME_INFO;

// ch:包裹信息 | en:Package information
typedef struct _MVLGS_PACKAGE_INFO_
{
    bool                bCodeEnable;                        // ch:是否包含条码信息,非0：包含条码信息 | en:Whether to include barcode information, non-zero: contains barcode information
    unsigned int        nCodeListNum;                       // ch:条码列表数量 | en:Number of barcode lists
    MVLGS_CODE_LIST     stCodeList[MVLGS_MAX_CODELISTNUM];  // ch:条码列表信息 | en:Bar code list information

    bool                bVolumeEnable;                      // ch:是否包含体积信息,非0：包含体积信息 | en:Whether to include volume information, non-zero: contains volume information
    MVLGS_VOLUME_INFO   stVolumeInfo;                       // ch:体积信息 | en:Volume information

    bool                bWeightEnable;                      // ch:是否包含重量信息,非0：包含重量信息 | en:Whether to include weight information, non-zero: contains weight information
    float               fWeight;                            // ch:重量信息 | en:Weight information

    bool                bPRMEnable;                         // ch:是否包含全景图像,非0：包含全景图像 | en:Whether to include panoramic images, non-zero: include panoramic images
    MVLGS_IMAGE_INFO    stPRMImage;                         // ch:原始图像信息 | en:Original image information

    bool                bIsFirstCodeOut;                    // ch:是否为条码先输出 (1为第一次输出，0为第二次输出) | en:Whether to output the barcode first (1 is the first output, 0 is the second output)

    long long           llTriggerStartTimeStamp;            // ch:触发开始时间戳(13位) | en:Trigger start timestamp (13 bits)
    long long           llTriggerEndTimeStamp;              // ch:触发结束时间戳(13位) | en:Trigger end timestamp (13 bits)

    unsigned int        nReserved[27];                      // ch:保留 | en:Reserved

}MVLGS_PACKAGE_INFO;

// ch:异常信息 | en:Exception information
typedef struct _MVLGS_EXCEPTION_INFO_
{
    unsigned int        nExceptionID;                       // ch:异常ID | en:Exception ID
    MVLGS_DEVICE_TYPE   enCamType;                          // ch:异常设备类型 | en:Abnormal device type
    char                strCamSerialNum[32];                // ch:异常设备序列号(体积相机此处为MAC地址) | en:Abnormal device serial number (the volume camera here is the MAC address)
    char                strExceptionDes[256];               // ch:异常描述 | en:Exception description

    unsigned int        nReserved[16];                      // ch:保留 | en:Reserved

}MVLGS_EXCEPTION_INFO;

// 触发信息
typedef struct _MVLGS_TRIGGER_INFO_
{
	unsigned int    nTriggerIndex;                           // 触发ID
	unsigned int    nTriggerFlag;                            // 与头文件触发开始为1(MV_LGS_BEGIN_TRIGGER) 触发结束为0(MV_LGS_STOP_TRIGGER) 对应映射

	unsigned int    nReserved[16];                          //预留

}MVLGS_TRIGGER_INFO;

// ch:相机输出图像信息 | en:Camera output image information
typedef struct _MVLGS_IMAGE_OUTPUT_INFO_
{
    MVLGS_IMAGE_INFO            stImage;                // ch:图像信息 | en:Image information
    char                        strSerialNumber[32];    // ch:相机序列号 | en:Camera serial number

    unsigned int                nReserved[32];          // ch:保留 | en:Reserved
}MVLGS_IMAGE_OUTPUT_INFO;

// ch:设备信息 | en:Device Information
typedef struct _MVLGS_XML_CFG_CAM_INFO_
{
    MVLGS_DEVICE_TYPE           enXmlCamType;                   // ch:相机类型 | en:Camera type

    // 相机信息
    unsigned int                nMacAddrHigh;                   // ch:MAC 地址 高位| en:High MAC address
    unsigned int                nMacAddrLow;                    // ch:MAC 地址 低位| en:Low MAC address
    unsigned int                nCurrentIp;                     // ch:当前IP | en:Current IP address
    unsigned char               strManufacturerName[32];        // ch:制造商名字 | en:Manufacturer Name
    unsigned char               strModelName[32];               // ch:型号名字 | en:Model Name
    unsigned char               strDeviceVersion[32];           // ch:设备版本号 | en:Device version No.
    unsigned char               strSerialNumber[32];            // ch:序列号 | en:Device serial No.
    unsigned char               strUserDefinedName[16];         // ch:用户自定义名字 | en:Custom name

    bool                        bDeviceOnline;                  // ch:设备在线状态，true在线，false离线 | en:Device online status, true online, false offline
    bool                        bMainCamera;                    // ch:是否为主相机 | en:Whether the main camera

    unsigned int                nReserved[16];                  // ch:保留 | en:Reserved
}MVLGS_XML_CFG_CAM_INFO;

// ch:XML配置文件中相机信息列表 | en:List of camera information in XML configuration file
typedef struct _MVLGS_XML_CFG_CAM_INFO_LIST_
{
    unsigned int                nXmlCfgCamNum;               // ch:XML配置文件中的相机数量 | en:Number of cameras in XML configuration file
    MVLGS_XML_CFG_CAM_INFO*     pstXmlCamInfo[256];          // ch:相机信息结构体数组 | en:Camera information structure array
}MVLGS_XML_CFG_CAM_INFO_LIST;

#endif //_MV_LOGISTICS_DEFINE_H_