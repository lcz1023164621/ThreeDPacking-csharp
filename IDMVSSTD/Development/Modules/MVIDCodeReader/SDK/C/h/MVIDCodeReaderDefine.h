
#ifndef _MVID_CODEREADER_DEFINE_H_
#define _MVID_CODEREADER_DEFINE_H_

#ifndef MVID_CODEREADER_API

    #if (defined (_WIN32) || defined(WIN64))
        #if defined(MVID_CODEREADER_EXPORTS)
            #define MVID_CODEREADER_API __declspec(dllexport)
        #else
            #define MVID_CODEREADER_API __declspec(dllimport)
        #endif
    #else
        #ifndef __stdcall
            #define __stdcall
        #endif

        #if defined(MVID_CODEREADER_EXPORTS)
            #define MVID_CODEREADER_API __attribute__((visibility("default")))
        #else
            #define MVID_CODEREADER_API
        #endif
    #endif

#endif

#ifdef _WIN32
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

#ifndef IN
    #define IN
#endif

#ifndef OUT
    #define OUT
#endif


/********************************************************************/
///  \~chinese
///  \name 正确码定义
///  @{
///  \~english
///  \name Definition of correct code
///  @{
#define MVID_CR_OK                  0x00000000  ///< \~chinese 成功，无错误    \~english Successed, no error
///  @}

/********************************************************************/
///  \~chinese
///  \name 通用错误码定义:范围0x80000000-0x800000FF
///  @{
///  \~english
///  \name Definition of General Error Codes: Range from 0x80000000 to 0x800000FF
///  @{
#define MVID_CR_E_HANDLE            0x80000000  ///< \~chinese 错误或无效的句柄   \~english Error or invalid handle
#define MVID_CR_E_SUPPORT           0x80000001  ///< \~chinese 不支持的功能       \~english The function is not supported
#define MVID_CR_E_BUFOVER           0x80000002  ///< \~chinese 缓存已满           \~english Buffer is full
#define MVID_CR_E_CALLORDER         0x80000003  ///< \~chinese 函数调用顺序错误   \~english Incorrect calling sequence
#define MVID_CR_E_PARAMETER         0x80000004  ///< \~chinese 错误的参数         \~english Incorrect parameter
#define MVID_CR_E_RESOURCE          0x80000005  ///< \~chinese 资源申请失败       \~english Applying resource failed
#define MVID_CR_E_NODATA            0x80000006  ///< \~chinese 无数据             \~english No data
#define MVID_CR_E_PRECONDITION      0x80000007  ///< \~chinese 前置条件有误，或运行环境已发生变化       \~english Precondition error, or running environment changed
#define MVID_CR_E_ENCRYPT           0x80000008  ///< \~chinese 凭证错误，可能是未插加密狗，或加密狗过期 \~english Credential error, possibly because the dongle was not installed or expired.
#define MVID_CR_E_RULE              0x8000000A  ///< \~chinese 过滤规则相关的错误 \~english Filter rule error.
#define MVID_CR_E_LOAD_LIBRARY      0x8000000B  ///< \~chinese 动态导入DLL失败     \~english Dynamically importing the DLL file failed.
#define MVID_CR_E_JPGENC            0x80000012  ///< \~chinese jpg编码相关错误     \~english Jpg encoding error.
#define MVID_CR_E_IMAGE             0x80000013  ///< \~chinese 输入的图像数据有损或图像格式,宽高错误    \~english Abnormal image. Incomplete image caused by packet loss or incorrect image format, width, or height.
#define MVID_CR_E_CONVERT           0x80000014  ///< \~chinese 格式转换错误       \~english Format conversion error
#define MVID_CR_E_UNKNOW            0x800000FF  ///< \~chinese 未知的错误         \~english Unknown error
///  @}

/********************************************************************/
///  \~chinese
///  \name GenICam系列错误:范围0x80000100-0x800301FF
///  @{
///  \~english
///  \name GenICam Series Error Codes: Range from 0x80000100 to 0x800001FF
///  @{
#define MVID_CR_E_GC_GENERIC        0x80000100  ///< \~chinese 通用错误           \~english General error
#define MVID_CR_E_GC_ARGUMENT       0x80000101  ///< \~chinese 参数非法           \~english Invalid parameter
#define MVID_CR_E_GC_RANGE          0x80000102  ///< \~chinese 值超出范围         \~english The value is out of range
#define MVID_CR_E_GC_PROPERTY       0x80000103  ///< \~chinese 属性错误           \~english Attribute error
#define MVID_CR_E_GC_RUNTIME        0x80000104  ///< \~chinese 运行环境有问题     \~english Running environment error
#define MVID_CR_E_GC_LOGICAL        0x80000105  ///< \~chinese 逻辑错误           \~english Incorrect logic
#define MVID_CR_E_GC_ACCESS         0x80000106  ///< \~chinese 节点访问条件有误   \~english Node accessing condition error
#define MVID_CR_E_GC_TIMEOUT        0x80000107  ///< \~chinese 超时               \~english Timeout
#define MVID_CR_E_GC_DYNAMICCAST    0x80000108  ///< \~chinese 转换异常           \~english Transformation exception
#define MVID_CR_E_GC_UNKNOW         0x800001FF  ///< \~chinese GenICam未知错误     \~english GenICam unknown error
///  @}

/********************************************************************/
///  \~chinese
///  \name GigE_STATUS对应的错误码:范围0x80000200-0x800002FF
///  @{
///  \~english
///  \name GigE_STATUS Error Codes: Range from 0x80000200 to 0x800002FF
///  @{
#define MVID_CR_E_NOT_IMPLEMENTED   0x80000200  ///< \~chinese 命令不被设备支持       \~english The command is not supported by device
#define MVID_CR_E_INVALID_ADDRESS   0x80000201  ///< \~chinese 访问的目标地址不存在   \~english Target address does not exist
#define MVID_CR_E_WRITE_PROTECT     0x80000202  ///< \~chinese 目标地址不可写         \~english The target address is not writable
#define MVID_CR_E_ACCESS_DENIED     0x80000203  ///< \~chinese 设备无访问权限         \~english No access permission
#define MVID_CR_E_BUSY              0x80000204  ///< \~chinese 设备忙，或网络断开     \~english Device is busy, or network is disconnected
#define MVID_CR_E_PACKET            0x80000205  ///< \~chinese 网络包数据错误         \~english Network packet error
#define MVID_CR_E_NETER             0x80000206  ///< \~chinese 网络相关错误           \~english Network error
#define MVID_CR_E_IP_CONFLICT       0x80000221  ///< \~chinese 设备IP冲突             \~english IP address conflicted
///  @}

/********************************************************************/
///  \~chinese
///  \name USB_STATUS对应的错误码:范围0x80000300-0x800003FF
///  @{
///  \~english
///  \name USB_STATUS Error Codes: Range from 0x80000300 to 0x800003FF
///  @{
#define MVID_CR_E_USB_READ          0x80000300  ///< \~chinese 读usb出错                \~english Reading USB error
#define MVID_CR_E_USB_WRITE         0x80000301  ///< \~chinese 写usb出错                \~english Writing USB error
#define MVID_CR_E_USB_DEVICE        0x80000302  ///< \~chinese 设备异常                 \~english Device exception
#define MVID_CR_E_USB_GENICAM       0x80000303  ///< \~chinese GenICam相关错误          \~english GenICam error
#define MVID_CR_E_USB_BANDWIDTH     0x80000304  ///< \~chinese 带宽不足                 \~english Insufficient bandwidth
#define MVID_CR_E_USB_DRIVER        0x80000305  ///< \~chinese 驱动不匹配或者未装驱动   \~english Driver mismatch or unmounted drive
#define MVID_CR_E_USB_UNKNOW        0x800003FF  ///< \~chinese USB未知的错误            \~english USB unknown error
///  @}

/********************************************************************/
///  \~chinese
///  \name 模块错误码:范围0x80002100-0x800025FF
///  @{
///  \~english
///  \name Module Error Codes: Range from 0x80000300 to 0x800003FF
///  @{
#define MVID_CR_E_CAMERA            0x80002100  ///< \~chinese 相机相关的错误     \~english Camera error
#define MVID_CR_E_BCR               0x80002200  ///< \~chinese 一维码相关错误     \~english 1D barcode error
#define MVID_CR_E_TDCR              0x80002300  ///< \~chinese 二维码相关错误     \~english 2D barcode error
#define MVID_CR_E_WAYBILL           0x80002400  ///< \~chinese 抠图相关错误       \~english Matting error
#define MVID_CR_E_SCRIPT            0x80002500  ///< \~chinese 脚本规则相关错误   \~english Script rule error
///  @}


/// \~chinese 相机类型定义     \~english Camera type definition
#define MVID_GIGE_CAM       0x00000001      ///< \~chinese GigE设备    \~english GigE Device
#define MVID_USB_CAM        0x00000004      ///< \~chinese USB3.0 设备 \~english USB3.0 Device

/// \~chinese 异常消息类型     \~english Exception message type
#define MVID_EXCEPTION_DEV_DISCONNECT          0x00008001      ///< \~chinese 设备断开连接 \~english The device is disconnected
#define MVID_EXCEPTION_SOFTDOG_DISCONNECT      0x00008002      ///< \~chinese 加密狗掉线   \~english The softdog is disconnected

/// \~chinese 相机信息     \~english Camera information
typedef struct _MVID_CAMERA_INFO_
{
    /// \~chinese 相机通用属性     \~english Camera general properties
    unsigned int        nCamType;                       ///< [OUT] \~chinese 相机类型，GigE或USB \~english Camera type: USB, GigE
    unsigned char       chManufacturerName[32];         ///< [OUT] \~chinese 制造商名字         \~english Manufacturer Name
    unsigned char       chModelName[32];                ///< [OUT] \~chinese 型号名字           \~english Model Name
    unsigned char       chDeviceVersion[32];            ///< [OUT] \~chinese 设备版本号         \~english Device version No.
    unsigned char       chManufacturerSpecificInfo[48]; ///< [OUT] \~chinese 制造商特定的信息   \~english Manufacturer specific information
    unsigned char       chSerialNumber[16];             ///< [OUT] \~chinese 序列号             \~english Device serial No.
    unsigned char       chUserDefinedName[16];          ///< [OUT] \~chinese 用户自定义名字     \~english Custom name 
    unsigned int        nMacAddrHigh;                   ///< [OUT] \~chinese MAC地址 高位       \~english High MAC address
    unsigned int        nMacAddrLow;                    ///< [OUT] \~chinese MAC地址 低位       \~english Low MAC address
    unsigned int        nCommomReaserved[8];            ///<       \~chinese 保留               \~english Reserved 

    /// \~chinese 网口相机属性     \~english GigE camera properties
    unsigned int        nCurrentIp;                     ///< [OUT] \~chinese 当前IP       \~english Current IP address 
    unsigned int        nNetExport;                     ///< [OUT] \~chinese 与设备连接的网口IP地址  \~english IP address of the network port connected to the device
	unsigned int        nUsedReserved[4];                   ///< [OUT] \~chinese 自研网卡下预留                 
    unsigned int        nGigEReserved[28];              ///<       \~chinese GigE保留    \~english GigE Reserved

    /// \~chinese USB相机属性      \~english USB camera properties
    unsigned char       CrtlInEndPoint;                 ///< [OUT] \~chinese 控制输入端点    \~english Control input endpoint
    unsigned char       CrtlOutEndPoint;                ///< [OUT] \~chinese 控制输出端点    \~english Control output endpoint
    unsigned char       StreamEndPoint;                 ///< [OUT] \~chinese 流端点          \~english Stream endpoint
    unsigned char       EventEndPoint;                  ///< [OUT] \~chinese 事件端点        \~english Event endpoint
    unsigned short      idVendor;                       ///< [OUT] \~chinese 供应商ID号      \~english Vendor ID 
    unsigned short      idProduct;                      ///< [OUT] \~chinese 产品ID号        \~english Product ID 
    unsigned int        nDeviceNumber;                  ///< [OUT] \~chinese 设备序列号      \~english Device ID 
    unsigned int        nUsbReserved[31];               ///<       \~chinese USB保留         \~english Usb Reserved

    bool                bSelectDevice;                  ///< [OUT] \~chinese 是否为指定系列型号相机  \~english Whether to specify a series model camera for the profile

    unsigned int        nReserved[63];                  ///<       \~chinese 保留    \~english Reserved 

}MVID_CAMERA_INFO;

#define MVID_MAX_CAM_NUM        256         ///< \~chinese 最大支持的设备个数  \~english The maximum number of supported devices

/// \~chinese 设备信息列表     \~english Device Information List
typedef struct _MVID_CAMERA_INFO_LIST_
{
    unsigned int            nCamNum;                            ///< [OUT] \~chinese 在线设备数量     \~english The number of online devices
    MVID_CAMERA_INFO*       pstCamInfo[MVID_MAX_CAM_NUM];       ///< [OUT] \~chinese 相机信息，支持最多256个设备 \~english Device information, up to 256 devices can be supported

}MVID_CAMERA_INFO_LIST;

/// \~chinese 算法处理能力集  \~english Algorithm processing capability set
#define MVID_BCR            0x00000001          ///< \~chinese 一维码   \~english One-dimensional code
#define MVID_TDCR           0x00000002          ///< \~chinese 二维码   \~english Two-dimensional code
#define MVID_WAYBILL        0x00000004          ///< \~chinese 面单抠图 \~english Image Matting

/// \~chinese 条码类型     \~english Code type
typedef enum _MVID_CODE_TYPE_
{
    MVID_CODE_NONE          = 0,            ///< \~chinese 无可识别条码 \~english No recognizable bar code

    MVID_CODE_TDCR_DM       = 1,            ///< \~chinese DM码  \~english DM code
    MVID_CODE_TDCR_QR       = 2,            ///< \~chinese QR码  \~english QR code

    MVID_CODE_BCR_EAN8      = 8,            ///< \~chinese EAN8码    \~english EAN8 code
    MVID_CODE_BCR_UPCE      = 9,            ///< \~chinese UPCE码    \~english UPCE code
    MVID_CODE_BCR_UPCA      = 12,           ///< \~chinese UPCA码    \~english UPCA code
    MVID_CODE_BCR_EAN13     = 13,           ///< \~chinese EAN13码   \~english EAN13 code
    MVID_CODE_BCR_ISBN13    = 14,           ///< \~chinese ISBN13码  \~english ISBN13 code
    MVID_CODE_BCR_CODABAR   = 20,           ///< \~chinese 库德巴码 \~english Codabar code
    MVID_CODE_BCR_ITF25     = 25,           ///< \~chinese 交叉25码 \~english ITF25 code
    MVID_CODE_BCR_CODE39    = 39,           ///< \~chinese Code 39   \~english Code 39
    MVID_CODE_BCR_CODE93    = 93,           ///< \~chinese Code 93   \~english Code 93
    MVID_CODE_BCR_CODE128   = 128,          ///< \~chinese Code 128  \~english Code 128

}MVID_CODE_TYPE;

/// \~chinese 图像格式     \~english Image type
typedef enum _MVID_IMAGE_TYPE_
{
    MVID_IMAGE_Undefined            = 0,    ///< \~chinese 未定义   \~english Undefined format
    MVID_IMAGE_MONO8                = 1,    ///< \~chinese Mono8     \~english MONO8 format
    MVID_IMAGE_JPEG                 = 2,    ///< \~chinese JPEG      \~english JPEG format
    MVID_IMAGE_BMP                  = 3,    ///< \~chinese Bmp       \~english BMP format
    MVID_IMAGE_RGB24                = 4,    ///< \~chinese RGB24     \~english RGB format
    MVID_IMAGE_BGR24                = 5,    ///< \~chinese BGR24     \~english BGR format

    MVID_IMAGE_MONO10               = 6,    ///< \~chinese Mono10        \~english Mono10 format
    MVID_IMAGE_MONO10_Packed        = 7,    ///< \~chinese Mono10_Packed \~english Mono10_Packed format
    MVID_IMAGE_MONO12               = 8,    ///< \~chinese Mono12        \~english Mono12 format
    MVID_IMAGE_MONO12_Packed        = 9,    ///< \~chinese Mono12_Packed \~english Mono12_Packed format
    MVID_IMAGE_MONO16               = 10,   ///< \~chinese Mono16        \~english Mono16 format
    MVID_IMAGE_BayerGR8             = 11,   ///< \~chinese BGR8          \~english BGR8 format
    MVID_IMAGE_BayerRG8             = 12,   ///< \~chinese BRG8          \~english BRG8 format
    MVID_IMAGE_BayerGB8             = 13,   ///< \~chinese BGB8          \~english BGB8 format
    MVID_IMAGE_BayerBG8             = 14,   ///< \~chinese BBG8          \~english BBG8 format
    MVID_IMAGE_BayerGR10            = 15,   ///< \~chinese BGR10         \~english BGR10 format
    MVID_IMAGE_BayerRG10            = 16,   ///< \~chinese BRG10         \~english BRG10 format
    MVID_IMAGE_BayerGB10            = 17,   ///< \~chinese BGB10         \~english BGB10 format
    MVID_IMAGE_BayerBG10            = 18,   ///< \~chinese BBG10         \~english BBG10 format
    MVID_IMAGE_BayerGR12            = 19,   ///< \~chinese BGR12         \~english BGR12 format
    MVID_IMAGE_BayerRG12            = 20,   ///< \~chinese BRG12         \~english BRG12 format
    MVID_IMAGE_BayerGB12            = 21,   ///< \~chinese BGB12         \~english BGB12 format
    MVID_IMAGE_BayerBG12            = 22,   ///< \~chinese BBG12         \~english BBG12 format
    MVID_IMAGE_BayerGR10_Packed     = 23,   ///< \~chinese BGR10_Packed  \~english BGR10_Packed format
    MVID_IMAGE_BayerRG10_Packed     = 24,   ///< \~chinese BRG10_Packed  \~english BRG10_Packed format
    MVID_IMAGE_BayerGB10_Packed     = 25,   ///< \~chinese BGB10_Packed  \~english BGB10_Packed format
    MVID_IMAGE_BayerBG10_Packed     = 26,   ///< \~chinese BBG10_Packed  \~english BBG10_Packed format
    MVID_IMAGE_BayerGR12_Packed     = 27,   ///< \~chinese BGR12_Packed  \~english BGR12_Packed format
    MVID_IMAGE_BayerRG12_Packed     = 28,   ///< \~chinese BRG12_Packed  \~english BRG12_Packed format
    MVID_IMAGE_BayerGB12_Packed     = 29,   ///< \~chinese BGB12_Packed  \~english BGB12_Packed format
    MVID_IMAGE_BayerBG12_Packed     = 30,   ///< \~chinese BBG12_Packed  \~english BBG12_Packed format
    MVID_IMAGE_YUV422_Packed        = 31,   ///< \~chinese YUV422_Packed \~english YUV422_Packed format
    MVID_IMAGE_YUV422_YUYV_Packed   = 32,   ///< \~chinese YUV422_YUYV_Packed \~english YUV422_YUYV_Packed format
    MVID_IMAGE_RGB8_Packed          = 33,   ///< \~chinese RGB8_Packed   \~english RGB8_Packed format
    MVID_IMAGE_BGR8_Packed          = 34,   ///< \~chinese BGR8_Packed   \~english BGR8_Packed format
    MVID_IMAGE_RGBA8_Packed         = 35,   ///< \~chinese RGBA8_Packed  \~english RGBA8_Packed format
    MVID_IMAGE_BGRA8_Packed         = 36,   ///< \~chinese BGRA8_Packed  \~english BGRA8_Packed format

}MVID_IMAGE_TYPE;

/// \~chinese 条码标识     \~english Code flag
typedef enum _MVID_CODE_FLAG_
{
    MVID_CODE_CORRECT           = 0,    ///< \~chinese 正常条码 \~english Normal bar code
    MVID_CODE_FILTERED          = 1,    ///< \~chinese 过滤条码 \~english Filter bar code

}MVID_CODE_FLAG;

/// \~chinese 图像输出模式     \~english Image output mode
typedef enum _MVID_IMAGE_OUTPUT_MODE_
{
    MVID_OUTPUT_NORMAL          = 0,    ///< \~chinese 常规输出 \~english normal output
    MVID_OUTPUT_RAW             = 1,    ///< \~chinese 原图输出 \~english raw output

}MVID_IMAGE_OUTPUT_MODE;

/// \~chinese 输出帧图像信息   \~english Output frame information
typedef struct _MVID_IMAGE_INFO_
{
    unsigned char*      pImageBuf;          ///< [OUT] \~chinese 原始图像缓存，由SDK内部分配    \~english Original image buffer
    unsigned int        nImageLen;          ///< [OUT] \~chinese 原始图像长度                   \~english Original image size
    MVID_IMAGE_TYPE     enImageType;        ///< [OUT] \~chinese 图像格式 \~english Image Type
    unsigned short      nWidth;             ///< [OUT] \~chinese 图像宽   \~english Image Width
    unsigned short      nHeight;            ///< [OUT] \~chinese 图像高   \~english Image Height

    unsigned int        nFrameNum;          ///< [OUT] \~chinese 帧号           \~english Frame No.
    unsigned int        nDevTimeStampHigh;  ///< [OUT] \~chinese 时间戳高32位   \~english Timestamp high 32 bits
    unsigned int        nDevTimeStampLow;   ///< [OUT] \~chinese 时间戳低32位   \~english Timestamp low 32 bits

    unsigned int        nFrameCounter;      ///< [OUT] \~chinese 帧触发号       \~english frame Trigger Counting 32 bits
    unsigned int        nTriggerIndex;      ///< [OUT] \~chinese 触发计数       \~english Trigger Counting
    unsigned long long  nHostTimeStamp;     ///< [OUT] \~chinese 主机时间戳低位 \~english host Timestamp 

    unsigned int        nDecodingTime;      ///< [OUT] \~chinese 解码时间 \~english DecodingTime

    unsigned int        nReserved[27];      ///<       \~chinese 保留     \~english Reserved

}MVID_IMAGE_INFO;

/// \~chinese 坐标点   \~english Coordinate point
typedef struct _MVID_POINT_I_
{
    int             nX;             ///< [OUT] \~chinese X坐标 \~english X-coordinate
    int             nY;             ///< [OUT] \~chinese Y坐标 \~english Y-coordinate

}MVID_POINT_I;

#define MVID_MAX_CODECHARATERLEN        4096        ///< \~chinese 最大条码长度 \~english Maximum barcode length
#define MVID_MAX_CODENUM                256         ///< \~chinese 最大条码数量 \~english Maximum number of barcodes

/// \~chinese 条码信息     \~english Code information
typedef struct _MVID_CODE_INFO_
{
    unsigned char       strCode[MVID_MAX_CODECHARATERLEN];      ///< [OUT] \~chinese 字符     \~english Character, maximum size: 4096
    int                 nLen;                                   ///< [OUT] \~chinese 字符长度 \~english Character size
    MVID_CODE_TYPE      enBarType;                              ///< [OUT] \~chinese 条码类型 \~english Bar code type
    MVID_POINT_I        stCornerPt[4];                          ///< [OUT] \~chinese 条码位置 \~english Bar code location
    int                 nAngle;                                 ///< [OUT] \~chinese 条码角度[0~3600°]  \~english Bar code angle, range: [0, 3600°]
    int                 nFilterFlag;                            ///< [OUT] \~chinese 过滤码标识(0为正常码，1为过滤码) \~english Filter identifier: 0- normal code, 1-filter code
    int                 nPPM;                                   ///< [OUT] \~chinese 估计ppm \~english ppm

    unsigned int        nReserved[30];                          ///<       \~chinese 保留 \~english Reserved

}MVID_CODE_INFO;

/// \~chinese 条码信息列表     \~english Code information list
typedef struct _MVID_CODE_INFO_LIST_
{
    int                 nCodeNum;                       ///< [OUT] \~chinese 条码数量 \~english The number of bar codes
    MVID_CODE_INFO      stCodeInfo[MVID_MAX_CODENUM];   ///< [OUT] \~chinese 条码信息 \~english Bar code information, maximum size: 256

    unsigned int        nReserved[32];                  ///<       \~chinese 保留     \~english Reserved

}MVID_CODE_INFO_LIST;

/// \~chinese 相机输出信息     \~english Camera output information
typedef struct _MVID_CAM_OUTPUT_INFO_
{
    MVID_IMAGE_INFO         stImage;            ///< [OUT] \~chinese 输出图像的信息 \~english Image information
    MVID_CODE_INFO_LIST     stCodeList;         ///< [OUT] \~chinese 条码信息列表   \~english Bar code information

    unsigned char*          pImageWaybill;      ///< [OUT] \~chinese 抠图缓存，由SDK内部分配 \~english Image matting buffer
    unsigned int            nImageWaybillLen;   ///< [OUT] \~chinese 图像大小                \~english Image size
    MVID_IMAGE_TYPE         enWaybillImageType; ///< [OUT] \~chinese 抠图图像格式            \~english Image format

    unsigned int            nReserved[31];      ///<       \~chinese 保留    \~english Reserved

}MVID_CAM_OUTPUT_INFO;

/// \~chinese 读码图像参数     \~english Code reading image parameters
typedef struct _MVID_PROC_PARAM_
{
    unsigned char*      pImageBuf;              ///< [IN]  \~chinese 原始图像缓存，由用户传入  \~english Original image buffer
    unsigned int        nImageLen;              ///< [IN]  \~chinese 原始图像长度              \~english Original image size
    MVID_IMAGE_TYPE     enImageType;            ///< [IN]  \~chinese 输入图像的格式            \~english Image type
    unsigned short      nWidth;                 ///< [IN]  \~chinese 图像宽                    \~english Image width
    unsigned short      nHeight;                ///< [IN]  \~chinese 图像高                    \~english Image height

    MVID_CODE_INFO_LIST stCodeList;             ///< [OUT] \~chinese 条码信息                  \~english Bar code information
    unsigned char*      pImageWaybill;          ///< [OUT] \~chinese 抠图缓存，由SDK内部分配   \~english Matting buffer, which is allocated by SDK
    unsigned int        nImageWaybillLen;       ///< [OUT] \~chinese 图像大小                  \~english Image size
    MVID_IMAGE_TYPE     enWaybillImageType;     ///< [OUT] \~chinese 抠图图像格式              \~english The format of the matted image

    unsigned int        nReserved[31];          ///<       \~chinese 保留    \~english Reserved

}MVID_PROC_PARAM;

/// \~chinese Int类型节点值       \~english Int node value
typedef struct _MVID_CR_CAM_INTVALUE_
{
    unsigned int    nCurValue;      ///< [OUT] \~chinese 当前值 \~english Current Value
    unsigned int    nMax;           ///< [OUT] \~chinese 最大值 \~english The maximum value
    unsigned int    nMin;           ///< [OUT] \~chinese 最小值 \~english The minimum value
    unsigned int    nInc;           ///< [OUT] \~chinese 增量值 \~english Increment

    unsigned int    nReserved[4];   ///<       \~chinese 保留    \~english Reserved

}MVID_CAM_INTVALUE;

/// \~chinese Int类型节点值Ex      \~english Int node value Ex
typedef struct _MVID_CAM_INTVALUE_EX_
{
    int64_t         nCurValue;          ///< [OUT] \~chinese 当前值 \~english Current Value
    int64_t         nMax;               ///< [OUT] \~chinese 最大值 \~english The maximum value
    int64_t         nMin;               ///< [OUT] \~chinese 最小值 \~english The minimum value
    int64_t         nInc;               ///< [OUT] \~chinese 增量值 \~english Increment

    unsigned int    nReserved[16];      ///<       \~chinese 保留    \~english Reserved

}MVID_CAM_INTVALUE_EX;

#define MVID_MAX_XML_SYMBOLIC_NUM       64      ///< \~chinese 最大枚举条目对应的符号长度 \~english Max Enum Entry Symbolic Number

/// \~chinese Enum类型节点值       \~english Enum node value
typedef struct _MVID_CAM_ENUMVALUE_
{
    unsigned int    nCurValue;                                  ///< [OUT] \~chinese 当前值 \~english Current Value
    unsigned int    nSupportedNum;                              ///< [OUT] \~chinese 数据的有效数据个数 \~english The number of valid data
    unsigned int    nSupportValue[MVID_MAX_XML_SYMBOLIC_NUM];   ///< [OUT] \~chinese 支持的枚举类型，每个数组表示一种类型，最大大小为：64 \~english Supported enumeration types, each array indicates one type, , maximum size: 64

    unsigned int    nReserved[4];                               ///<       \~chinese 保留 \~english Reserved

}MVID_CAM_ENUMVALUE;

/// \~chinese Float类型节点值      \~english Float node value
typedef struct _MVID_CAM_FLOATVALUE_
{
    float           fCurValue;      ///< [OUT] \~chinese 当前值 \~english Current Value
    float           fMax;           ///< [OUT] \~chinese 最大值 \~english The maximum value
    float           fMin;           ///< [OUT] \~chinese 最小值 \~english The minimum value

    unsigned int    nReserved[4];   ///<       \~chinese 保留 \~english Reserved

}MVID_CAM_FLOATVALUE;

/// \~chinese String类型节点值     \~english String node value
typedef struct _MVID_CAM_STRINGVALUE_
{
    char            chCurValue[256];    ///< [OUT] \~chinese 当前值   \~english Current Value
    int64_t         nMaxLength;         ///< [OUT] \~chinese 最大长度 \~english The maximum size

    unsigned int    nReserved[2];       ///<       \~chinese 保留 \~english Reserved

}MVID_CAM_STRINGVALUE;

#define MVID_MAX_EVENT_NAME_SIZE        128     ///< \~chinese Event事件名称最大长度 \~english Max length of event name

/// \~chinese Event事件回调信息    \~english Event callback infomation
typedef struct _MVID_EVENT_OUT_INFO_
{
    char            EventName[MVID_MAX_EVENT_NAME_SIZE];    ///< [OUT] \~chinese Event名称     \~english Event name

    unsigned short  nEventID;                               ///< [OUT] \~chinese Event号       \~english Event ID
    unsigned short  nStreamChannel;                         ///< [OUT] \~chinese 流通道序号    \~english Circulation number

    unsigned int    nBlockIdHigh;                           ///< [OUT] \~chinese 帧号高位      \~english BlockId high
    unsigned int    nBlockIdLow;                            ///< [OUT] \~chinese 帧号低位      \~english BlockId low

    unsigned int    nTimestampHigh;                         ///< [OUT] \~chinese 时间戳高位    \~english Timestramp high
    unsigned int    nTimestampLow;                          ///< [OUT] \~chinese 时间戳低位    \~english Timestramp low

    void *          pEventData;                             ///< [OUT] \~chinese Event数据      \~english Event data
    unsigned int    nEventDataSize;                         ///< [OUT] \~chinese Event数据长度  \~english Event data len

    unsigned int    nReserved[16];                          ///<       \~chinese 保留 \~english Reserved

}MVID_EVENT_OUT_INFO;

/// \~chinese 信息类型     \~english Information Type
#define MVID_MATCH_TYPE_NET_DETECT      0x00000001      ///< \~chinese 网络流量和丢包信息                \~english Network traffic and packet loss information
#define MVID_MATCH_TYPE_USB_DETECT      0x00000002      ///< \~chinese host接收到来自U3V设备的字节总数    \~english The total number of bytes host received from U3V device

/// \~chinese 全匹配的信息结构体     \~english Fully matched information structure
typedef struct _MVID_ALL_MATCH_INFO_
{
    unsigned int    nType;          ///< [IN]       \~chinese 需要输出的信息类型，e.g. MVID_MATCH_TYPE_NET_DETECT \~english Information type need to output, e.g. MVID_MATCH_TYPE_NET_DETECT

    void*           pInfo;          ///< [IN][OUT]  \~chinese 输出的信息缓存，由调用者分配 \~english Output information cache, which is allocated by the caller
    unsigned int    nInfoSize;      ///< [IN]       \~chinese 信息缓存的大小               \~english Information cache size

}MVID_ALL_MATCH_INFO;

/// \~chinese 网络流量和丢包信息反馈结构体，对应类型为 MVID_MATCH_TYPE_NET_DETECT  \~english Network traffic and packet loss feedback structure, the corresponding type is MVID_MATCH_TYPE_NET_DETECT
typedef struct _MVID_MATCH_INFO_NET_DETECT_
{
    int64_t         nReviceDataSize;            ///< [OUT] \~chinese 已接收数据大小[Start和Stop之间]  \~english Received data size
    int64_t         nLostPacketCount;           ///< [OUT] \~chinese 丢失的包数量  \~english Number of packets lost
    unsigned int    nLostFrameCount;            ///< [OUT] \~chinese 丢帧数量      \~english Number of frames lost
    unsigned int    nNetRecvFrameCount;         ///< [OUT] \~chinese 接收帧数量    \~english Received Frame Count
    int64_t         nRequestResendPacketCount;  ///< [OUT] \~chinese 请求重发包数  \~english Request Resend Packet Count
    int64_t         nResendPacketCount;         ///< [OUT] \~chinese 重发包数      \~english Resend Packet Count

}MVID_MATCH_INFO_NET_DETECT;

/// \~chinese host收到从u3v设备端的总字节数，对应类型为 MVID_MATCH_TYPE_USB_DETECT  \~english The total number of bytes host received from the u3v device side, the corresponding type is MV_MATCH_TYPE_USB_DETECT
typedef struct _MVID_MATCH_INFO_USB_DETECT_
{
    int64_t         nReviceDataSize;      ///< [OUT] \~chinese 已接收数据大小[Bind和Stop之间] \~english Received data size
    unsigned int    nRevicedFrameCount;   ///< [OUT] \~chinese 已收到的帧数                  \~english Number of frames received
    unsigned int    nErrorFrameCount;     ///< [OUT] \~chinese 错误帧数                      \~english Number of error frames

    unsigned int    nReserved[2];         ///<       \~chinese 保留 \~english Reserved

}MVID_MATCH_INFO_USB_DETECT;

#define MVID_ALGORITHM_MIN_WIDTH        128         ///< \~chinese 算法支持最小宽度   \~english Algorithm supports minimum width
#define MVID_ALGORITHM_MIN_HEIGHT       128         ///< \~chinese 算法支持最小高度   \~english Algorithm supports minimum height


/// \~chinese 一维码参数，内部有默认值，可以不设置  \~english Bcr code parameter, with default values inside, which can not be set
#define KEY_BCR_ABILITY                 "BCR_Ability"               ///< \~chinese 算法能力集，含Code39[1]，Code128[2]，CodeBar[4]，EAN[8]，ITF25[16]，CODE93[32]，默认值63，范围[0,63] \~english Algorithm capability set.contain:Code39[1]，Code128[2]，CodeBar[4]，EAN[8]，ITF25[16]，CODE93[32],Range: [0, 63], default: 63
#define KEY_BCR_ROI_X                   "BCR_PositionXROI"          ///< \~chinese 图像ROI X方向偏移，默认值0，范围[0,65535]  \~english Image ROI X direction offset,Range: [0,65535], default: 0
#define KEY_BCR_ROI_Y                   "BCR_PositionYROI"          ///< \~chinese 图像ROI Y方向偏移，默认值0，范围[0,65535]  \~english Image ROI Y direction offset,Range: [0,65535], default: 0
#define KEY_BCR_ROI_WIDTH               "BCR_WidthROI"              ///< \~chinese 图像ROI宽度，默认值65535，范围[100,65535]  \~english Image ROI width. Range: [100,65535], default: 65535
#define KEY_BCR_ROI_HEIGHT              "BCR_HeightROI"             ///< \~chinese 图像ROI高度，默认值65535，范围[40,65535]   \~english Image ROI height. Range: [40,65535], default: 65535
#define KEY_BCR_MAX_WIDTH               "BCR_MaxWidth"              ///< \~chinese 算法最大宽度，默认3840，范围[0,65535]      \~english Maximum width of the algorithm. Range: [0,65535], default: 3840
#define KEY_BCR_MAX_HEIGHT              "BCR_MaxHeight"             ///< \~chinese 算法最大高度，默认2748，范围[0,65535]      \~english Maximum height of the algorithm. Range: [0,65535], default: 2748 

#define KEY_BCR_LOCBARNUM               "BCR_LocBarNum"             ///< \~chinese 条形码区域定位模块条码区域个数，默认值4，范围[1,200] \~english The number of barcodes to read, Range: [1, 200], default: 4
#define KEY_BCR_LOCWINSIZE              "BCR_LocWinSize"            ///< \~chinese 条形码区域定位模块窗口大小，默认值4，范围[4,65]      \~english Barcode height, Range: [4, 65], default: 4
#define KEY_BCR_WAITINGTIME             "BCR_WaitingTime"           ///< \~chinese 算法库Process最大运行时间，超过限定时间强行return，默认值500，范围[0,5000] \~english Algorithm library's maximum running time. The current image processing will end when exceeding the specified running time., Range: [0, 5000], default: 500
#define KEY_BCR_SEGQUIETW               "BCR_SegQuietW"             ///< \~chinese 条码静区宽度，默认值30，范围[0,200]    \~english Width of the barcode quiet zone Range: [0, 200], default: 30
#define KEY_BCR_DFKSIZELOWERLIMIT       "BCR_DfkSizeLowerLimit"     ///< \~chinese 去伪过滤尺寸下限（宽不足此数的条码删掉），默认值30，范围[0,4000]   \~english Lower limit of barcode width, below which the bar code will be filtered out.Range: [0, 4000], default: 30
#define KEY_BCR_DFKSIZEUPPERLIMIT       "BCR_DfkSizeUpperLimit"     ///< \~chinese 去伪过滤尺寸上限（宽超过此数的条码删掉），默认值2400，范围[0,4000] \~english Upper limit of barcode width, above which the barcode will be filtered out.Range: [0, 4000], default: 2400
#define KEY_BCR_SAVEIMAGELEVEL          "BCR_SaveImageLevel"        ///< \~chinese 保存未译出图片的灵敏度，默认值1，范围[1,3]   \~english Sensitivity of saving untranslated images, Range: [1, 3], default: 1
#define KEY_BCR_APPMODE                 "BCR_AppMode"               ///< \~chinese 算法库运行模式（动态模式/预留模式），默认值1，范围[0,2147483646] \~english Algorithm library operating mode in dynamic mode or reservation mode. Range: [0, 2147483646], default: 1
#define KEY_BCR_DISTORTION              "BCR_Distortion"            ///< \~chinese 动态模式下 透视畸变开关，默认值0，范围[0,2147483646] \~english Perspective distortion switch in expert mode. Range: [0, 2147483646], default: 0
#define KEY_BCR_WHITEGAP                "BCR_WhiteGap"              ///< \~chinese 动态模式下 印刷质量开关，默认值0，范围[0,2147483646] \~english Printing quality switch in dynamic mode, Range: [0, 2147483646], default: 0
#define KEY_BCR_SPOT                    "BCR_Spot"                  ///< \~chinese 动态模式下 镜面反光开关，默认值0，范围[0,2147483646] \~english Specular reflective switch in dynamic mode. Range: [0, 2147483646], default: 0
#define KEY_BCR_SAMPLELEVEL             "BCR_SampleLevel"           ///< \~chinese 图像采样尺度，默认值1，范围[1,8]       \~english Image sampling scale. Range: [1, 8], default: 1
#define KEY_BCR_IAMGEMORPH              "BCR_ImageMorph"            ///< \~chinese 图像形态学预处理，默认值0，范围[0，2] \~english Image morphology preprocessing. Range: [0, 2], default: 0
#define KEY_BCR_DELERRFLAG              "BCR_DelErrFlag"            ///< \~chinese 降错识开关，默认值1，范围[0,1]         \~english Lower error identification switch. Range: [0, 1], default: 1


/// \~chinese 二维码参数，内部有默认值，可以不设置  \~english Tdcr code parameter, with default values inside, which can not be set
#define KEY_TDCR_ABILITY                 "TDCR_Ability"             ///< \~chinese 算法能力集，含QR[1]，DM[2]，默认值3，范围[0,3]    \~english Algorithm capability set.contain:QR[1]，DM[2], Range: [0, 3], default: 3
#define KEY_TDCR_ROI_X                   "TDCR_PositionXROI"        ///< \~chinese 图像ROI X方向偏移，默认值0，范围[0,65535] \~english Image ROI X direction offset,Range: [0,65535], default: 0
#define KEY_TDCR_ROI_Y                   "TDCR_PositionYROI"        ///< \~chinese 图像ROI Y方向偏移，默认值0，范围[0,65535] \~english Image ROI Y direction offset,Range: [0,65535], default: 0
#define KEY_TDCR_ROI_WIDTH               "TDCR_WidthROI"            ///< \~chinese 图像ROI宽度，默认值65535，范围[128,65535] \~english Image ROI width. Range: [128,65535], default: 65535
#define KEY_TDCR_ROI_HEIGHT              "TDCR_HeightROI"           ///< \~chinese 图像ROI高度，默认值65535，范围[128,65535] \~english Image ROI height. Range: [128,65535], default: 65535
#define KEY_TDCR_MAX_WIDTH               "TDCR_MaxWidth"            ///< \~chinese 算法最大宽度，默认3840，范围[0,65535]     \~english Maximum width of the algorithm. Range: [0,65535], default: 3840
#define KEY_TDCR_MAX_HEIGHT              "TDCR_MaxHeight"           ///< \~chinese 算法最大高度，默认2748，范围[0,65535]     \~english Maximum height of the algorithm. Range: [0,65535], default: 2748 

#define KEY_TDCR_LOCCODENUM              "TDCR_LocCodeNum"          ///< \~chinese 检测模块输出ROI个数，默认值5，范围[1,1000]    \~english The number of ROIs outputted by detection module. Range: [1, 1000], default: 5
#define KEY_TDCR_MINBARSIZE              "TDCR_MinBarSize"          ///< \~chinese blob筛选时，最小宽高，默认值40，范围[20,1000] \~english The minimum width and height when filtering blobs. Range: [20, 1000], default: 40
#define KEY_TDCR_MAXBARSIZE              "TDCR_MaxBarSize"          ///< \~chinese blob筛选时，最大宽高，默认值300，范围[20,1000] \~english The maximum width and height when filtering blobs. Range: [20, 1000], default: 300
#define KEY_TDCR_MIRRORMODE              "TDCR_MirrorMode"          ///< \~chinese 镜像模式是否打开，默认值0，范围[0,2]  \~english Whether to enable mirror mode. Range: [0, 2], default: 0
#define KEY_TDCR_SAMPLELEVEL             "TDCR_SampleLevel"         ///< \~chinese 图像降采样倍数，默认值1，范围[1,8]    \~english Image downsampling ratio. Range: [1, 8], default: 1
#define KEY_TDCR_CODECOLOR               "TDCR_CodeColor"           ///< \~chinese 白底黒码标识，默认值0，范围[0,2]      \~english Identifier of the black bar code on white background. Range: [0, 2], default: 0
#define KEY_TDCR_DISCRETEFLAG            "TDCR_DiscreteFlag"        ///< \~chinese 连续与离散码标志，0-连续码 1-离散码，默认值0，范围[0,2] \~english Code flag: "0"-continuous code, "1"-discrete code, "2"-self-adaptive Range: [0, 2], default: 0
#define KEY_TDCR_DISTORTIONFLAG          "TDCR_DistortionFlag"      ///< \~chinese QR畸变配置参数，默认值0，范围[0,1]    \~english QR distortion configuration parameter. Range: [0, 1], default: 0
#define KEY_TDCR_ADVANCEPARAM            "TDCR_AdvanceParam"        ///< \~chinese 高级参数，默认值0，范围[0,2147483640]  \~english Advanced parameters. Range: [0, 2147483640], default: 0
#define KEY_TDCR_ADVANCEPARAM2           "TDCR_AdvanceParam2"       ///< \~chinese 高级参数2，默认值0，范围[0,2147483640] \~english Advanced parameters 2. Range: [0, 2147483640], default: 0
#define KEY_TDCR_WAITINGTIME             "TDCR_WaitingTime"         ///< \~chinese 超时退出时间，默认值1000，范围[0,5000] \~english Timeout exit time. Range: [0, 5000], default: 1000
#define KEY_TDCR_DEBUGFLAG               "TDCR_DebugFlag"           ///< \~chinese debug信息是否打开，默认值0，范围[0,1] \~english Whether to enable debug information. Range: [0, 1], default: 0
#define KEY_TDCR_RECTANGLE               "TDCR_Rectangle"           ///< \~chinese dm正方形长方形码类型，0 正方形 1 长方形 2 兼容模式，默认值0，范围[0,2] \~english Code types: 0-sqaure, 1-rectangle, 2-compatible mode. Range: [0, 2], default: 0
#define KEY_TDCR_APPMODE                 "TDCR_AppMode"             ///< \~chinese 算法库运行模式（普通模式/专业模式/极速模式），默认值0，范围[0,2]       \~english Algorithm library operation mode


/// \~chinese 抠图参数，内部有默认值，可以不设置    \~english Waybill parameter, with default values inside, which can not be set
#define KEY_WAYBILL_ABILITY                     "WAYBILL_Ability"                   ///< \~chinese 算法能力集，含面单提取[1]，图像增强[2]，码提取[4]，默认7，范围[1,7] \~english Algorithm capability set. contain:Waybill [1], image enhancement [2], code extraction [4]. Range: [1, 7], default: 7
#define KEY_WAYBILL_MAX_WIDTH                   "WAYBILL_Max_Width"                 ///< \~chinese 算法最大宽度，默认3840，范围[0,65535] \~english Maximum width of the algorithm. Range: [0,65535], default: 3840
#define KEY_WAYBILL_MAX_HEIGHT                  "WAYBILL_Max_Height"                ///< \~chinese 算法最大高度，默认2748，范围[0,65535] \~english Maximum height of the algorithm. Range: [0,65535], default: 2748 
#define KEY_WAYBILL_OUTPUTIMAGETYPE             "WAYBILL_OutputImageType"           ///< \~chinese 面单抠图输出的图片格式，默认Jpg，范围[1,3],1为Mono8，2为Jpg，3为BMP \~english Waybill Image format of the image output，1-Mono8,2-Jpg,3-BMP,Range: [1,3], default: jpg
#define KEY_WAYBILL_JPGQUALITY                  "WAYBILL_JpgQuality"                ///< \~chinese jpg编码质量，默认80，范围[1,100] \~english Jpp encoding quality. Range: [1,100], default: 80

#define KEY_WAYBILL_MINWIDTH                    "WAYBILL_MinWidth"                  ///< \~chinese waybill最小宽, 宽是长边, 高是短边，默认100，范围[15,2592] \~english Minimum width of waybill. Range: [15, 2592], default: 100
#define KEY_WAYBILL_MINHEIGHT                   "WAYBILL_MinHeight"                 ///< \~chinese waybill最小高，默认100，范围[10,2048] \~english Minimum height of waybill. Range: [10, 2048], default: 100
#define KEY_WAYBILL_MAXWIDTH                    "WAYBILL_MaxWidth"                  ///< \~chinese waybill最大宽, 宽是长边, 高是短边，默认3072，范围[15,3072] \~english Maximum width of waybill. Range: [15, 3072], default: 3072
#define KEY_WAYBILL_MAXHEIGHT                   "WAYBILL_MaxHeight"                 ///< \~chinese waybill最大高，默认2048，范围[10,2048] \~english Maximum height of waybill. Range: [10, 2048], default: 2048
#define KEY_WAYBILL_MORPHTIMES                  "WAYBILL_MorphTimes"                ///< \~chinese 膨胀次数，默认0，范围[0,10] \~english Expansion times. Range: [0, 10], default: 0
#define KEY_WAYBILL_GRAYLOW                     "WAYBILL_GrayLow"                   ///< \~chinese 面单上条码和字符灰度最小值，默认0，范围[0,255] \~english Minimum gray value of the bar code and character gray on the waybill. Range: [0, 255], default: 0
#define KEY_WAYBILL_GRAYMID                     "WAYBILL_GrayMid"                   ///< \~chinese 面单上灰度中间值，用于区分条码和背景，默认70，范围[0,255] \~english Median gray value of waybill which is used to distinguish barcode from background. Range: [0, 255], default: 70
#define KEY_WAYBILL_GRAYHIGH                    "WAYBILL_GrayHigh"                  ///< \~chinese 面单上背景灰度最大值，默认130，范围[0,255] \~english Maximum gray value of waybill background. Range: [0, 255], default: 130
#define KEY_WAYBILL_BINARYADAPTIVE              "WAYBILL_BinaryAdaptive"            ///< \~chinese 自适应二值化，默认1，范围[0,1] \~english Adaptive binarization. Range: [0, 1], default: 1
#define KEY_WAYBILL_BOUNDARYROW                 "WAYBILL_BoundaryRow"               ///< \~chinese 面单抠图行方向扩边，默认0，范围[0,2000] \~english Expand the edge in row direction when matting waybill. Range: [0, 2000], default: 0
#define KEY_WAYBILL_BOUNDARYCOL                 "WAYBILL_BoundaryCol"               ///< \~chinese 面单抠图列方向扩边，默认0，范围[0,2000] \~english Expand the edge in column direction when matting waybill. Range: [0, 2000], default: 0
#define KEY_WAYBILL_MAXBILLBARHEIGTHRATIO       "WAYBILL_MaxBillBarHightRatio"      ///< \~chinese 最大面单和条码高度比例，默认20，范围[1,100] \~english Maximum height ratio of waybill to barcode. Range: [1, 100], default: 20
#define KEY_WAYBILL_MAXBILLBARWIDTHRATIO        "WAYBILL_MaxBillBarWidthRatio"      ///< \~chinese 最大面单和条码宽度比例，默认5，范围[1,100] \~english Maximum width ratio of waybill to barcode. Range: [1, 100], default: 5
#define KEY_WAYBILL_MINBILLBARHEIGTHRATIO       "WAYBILL_MinBillBarHightRatio"      ///< \~chinese 最小面单和条码高度比例，默认5，范围[1,100] \~english Minimum height ratio of waybill to barcode. Range: [1, 100], default: 5
#define KEY_WAYBILL_MINBILLBARWIDTHRATIO        "WAYBILL_MinBillBarWidthRatio"      ///< \~chinese 最小面单和条码宽度比例，默认2，范围[1,100] \~english Minimum width ratio of waybill to barcode. Range: [1, 100], default: 2
#define KEY_WAYBILL_ENHANCEMETHOD               "WAYBILL_EnhanceMethod"             ///< \~chinese 增强方法，默认2，范围[1,4] \~english Enhancement method. Range: [1, 4], default: 2
#define KEY_WAYBILL_ENHANCECLIPRATIOLOW         "WAYBILL_ClipRatioLow"              ///< \~chinese 增强拉伸低阈值比例，默认1，范围[0,100] \~english Enhance the low threshold ratio of stretching. Range: [0, 100], default: 1
#define KEY_WAYBILL_ENHANCECLIPRATIOHIGH        "WAYBILL_ClipRatioHigh"             ///< \~chinese 增强拉伸高阈值比例，默认99，范围[0,100] \~english Enhance the high threshold ratio of stretching. Range: [0, 100], default: 99
#define KEY_WAYBILL_ENHANCECONTRASTFACTOR       "WAYBILL_ContrastFactor"            ///< \~chinese 对比度系数，默认100，范围[1,10000] \~english Contrast ratio. Range: [1, 10000], default: 100
#define KEY_WAYBILL_ENHANCESHARPENFACTOR        "WAYBILL_SharpenFactor"             ///< \~chinese 锐化系数，默认0，范围[0,10000] \~english Sharpness. Range: [0,10000], default: 0
#define KEY_WAYBILL_SHARPENKERNELSIZE           "WAYBILL_KernelSize"                ///< \~chinese 锐化滤波核大小，默认3，范围[3,15] \~english Size of sharpening filter core. Range: [3, 15], default: 3
#define KEY_WAYBILL_CODEBOUNDARYROW             "WAYBILL_CodeBoundaryRow"           ///< \~chinese 码单抠图行方向扩边，默认0，范围[0,2000] \~english Expand the edge in row direction when matting weight memo. Range: [0, 2000], default: 0
#define KEY_WAYBILL_CODEBOUNDARYCOL             "WAYBILL_CodeBoundaryCol"           ///< \~chinese 码单抠图列方向扩边，默认0，范围[0,2000] \~english Expand the edge in column direction when matting weight memo. Range: [0, 2000], default: 0


#endif //_MVID_CODEREADER_DEFINE_H_
