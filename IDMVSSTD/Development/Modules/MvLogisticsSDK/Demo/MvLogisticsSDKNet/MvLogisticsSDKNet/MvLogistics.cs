using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;

namespace MvLogisticsSDKNet
{
    /// <summary>
    /// MvLogisticsSDK
    /// </summary>
    public class MvLogistics
    {
        // 回调函数声明

        /// <summary>
        /// ch:异常消息回调 | en:Exception message callBack
        /// </summary>
        /// <param name="pstEcptInfo">ch:异常回调参数结构体 | en:Exception message structure</param>
        /// <param name="pUser">ch:用户自定义变量 | en:User defined variable</param>
        public delegate void cbExceptiondelegate(ref MVLGS_EXCEPTION_INFO pstEcptInfo, IntPtr pUser);

        /// <summary>
        /// ch:包裹回调 | en:package callback
        /// </summary>
        /// <param name="pstOutput">ch:包裹信息指针 | en:package infomation pointer</param>
        /// <param name="pUser">ch:用户自定义变量 | en:User defined variable</param>
        public delegate void cbOutputdelegate(IntPtr pstOutput, IntPtr pUser);

        /// <summary>
        /// ch:NoRead图像回调 | en:NoRead Image data callback
        /// </summary>
        /// <param name="pstImageOutPutInfo">ch:NoRead图像回调参数指针 | en:NoRead image callback parameter pointer</param>
        /// <param name="pUser">ch:用户自定义变量 | en:User defined variable</param>
        public delegate void cbNoReaddelegate(IntPtr pstImageOutPutInfo, IntPtr pUser);

        /// <summary>
        /// ch:触发回调 | en:Trigger callback
        /// </summary>
        /// <param name="pstTriggerInfo">ch:触发回调参数指针 | en:Trigger callback parameter pointer</param>
        /// <param name="pUser">ch:用户自定义变量 | en:User defined variable</param>
		public delegate void cbTriggerOutputdelegate(ref MVLGS_TRIGGER_INFO pstTriggerInfo, IntPtr pUser);

        /// <summary>
        /// ch:获取SDK版本号 | en:Get SDK Version
        /// </summary>
        /// <returns>ch:返回SDK版本号 | en:SDK Version</returns>
        public static Int32 MV_LGS_GetVersion_NET()
        {
            return MV_LGS_GetVersion();
        }

        /// <summary>
        /// ch:构造函数 | en:Constructor
        /// </summary>
        public MvLogistics()
        {
            handle = IntPtr.Zero;
        }

        /// <summary>
        /// ch:析构函数 | en:Destructor
        /// </summary>
        ~MvLogistics()
        {
            MV_LGS_DestroyHandle_NET();
        }

        /// <summary>
        /// ch:创建句柄 | en:Create Handle
        /// </summary>
        /// <returns>ch:成功, 返回MV_LGS_OK; 错误, 返回错误码 | en:Success, return MV_LGS_OK. Failure, return error code</returns>
        public Int32 MV_LGS_CreateHandle_NET()
        {
            if (IntPtr.Zero != handle)
            {
                MV_LGS_DestroyHandle(handle);
                handle = IntPtr.Zero;
            }

            return MV_LGS_CreateHandle(ref handle);
        }

        /// <summary>
        /// ch:销毁句柄 | en:Destroy Handle
        /// </summary>
        /// <returns>ch:成功, 返回MV_LGS_OK; 错误, 返回错误码 | en:Success, return MV_LGS_OK. Failure, return error code</returns>
        public Int32 MV_LGS_DestroyHandle_NET()
        {
            Int32 nRet = MV_LGS_DestroyHandle(handle);
            handle = IntPtr.Zero;
            return nRet;
        }

        /// <summary>
        /// ch:加载配置文件 | en:Load Config file
        /// </summary>
        /// <param name="strCfgPath">ch:配置文件路径 | en:FilePath of Config File</param>
        /// <returns>ch:成功, 返回MV_LGS_OK; 错误, 返回错误码 | en:Success, return MV_LGS_OK. Failure, return error code</returns>
        public Int32 MV_LGS_LoadDevCfg_NET(String strCfgPath)
        {
            return MV_LGS_LoadDevCfg(handle, strCfgPath);
        }

        /// <summary>
        /// ch:注册异常消息回调 | en:Register Exception Message CallBack
        /// </summary>
        /// <param name="cbException">ch:异常回调函数指针 | en:Exception Message CallBack Function Pointer</param>
        /// <param name="pUser">ch:用户自定义变量 | en:User defined variable</param>
        /// <returns>ch:成功, 返回MV_LGS_OK; 错误, 返回错误码 | en:Success, return MV_LGS_OK. Failure, return error code</returns>
        public Int32 MV_LGS_RegisterExceptionCB_NET(cbExceptiondelegate cbException, IntPtr pUser)
        {
            return MV_LGS_RegisterExceptionCB(handle, cbException, pUser);
        }

        /// <summary>
        /// ch:包裹消息回调 | en:Register Pakcage Message CallBack
        /// </summary>
        /// <param name="cbOutput">ch:包裹信息回调函数指针 | en:Pakcage Message CallBack Function Pointer</param>
        /// <param name="pUser">ch:用户自定义变量 | en:User defined variable</param>
        /// <returns>ch:成功, 返回MV_LGS_OK; 错误, 返回错误码 | en:Success, return MV_LGS_OK. Failure, return error code</returns>
        public Int32 MV_LGS_RegisterPackageCB_NET(cbOutputdelegate cbOutput, IntPtr pUser)
        {
            return MV_LGS_RegisterPackageCB(handle, cbOutput, pUser);
        }
        /// <summary>
        /// ch:触发消息回调 | en:Register Trigger Message CallBack
        /// </summary>
        /// <param name="cbTriggerInfoOutput">ch:触发消息回调函数指针 | en:Trigger Message CallBack Function Pointer</param>
        /// <param name="pUser">ch:用户自定义变量 | en:User defined variable</param>
        /// <returns>ch:成功, 返回MV_LGS_OK; 错误, 返回错误码 | en:Success, return MV_LGS_OK. Failure, return error code</returns>
        public Int32 MV_LGS_RegisterTriggerInfoCB_NET(cbTriggerOutputdelegate cbTriggerInfoOutput, IntPtr pUser)
        {
            return MV_LGS_RegisterTriggerInfoCB(handle, cbTriggerInfoOutput, pUser);
        }
        /// <summary>
        /// ch:开始取流 | en:Start Grabbing
        /// </summary>
        /// <returns>ch:成功, 返回MV_LGS_OK; 错误, 返回错误码 | en:Success, return MV_LGS_OK. Failure, return error code</returns>
        public Int32 MV_LGS_Start_NET()
        {
            return MV_LGS_Start(handle);
        }

        /// <summary>
        /// ch:结束取流 | en:Stop Grabbing
        /// </summary>
        /// <returns>ch:成功, 返回MV_LGS_OK; 错误, 返回错误码 | en:Success, return MV_LGS_OK. Failure, return error code</returns>
        public Int32 MV_LGS_Stop_NET()
        {
            return MV_LGS_Stop(handle);
        }

        /// <summary>
        /// ch:外部设置触发状态 | en:Set Trigger Signal
        /// </summary>
        /// <param name="nTriggerSignal">ch:触发状态 | en:Trigger Signal</param>
        /// <returns>ch:成功, 返回MV_LGS_OK; 错误, 返回错误码 | en:Success, return MV_LGS_OK. Failure, return error code</returns>
        public Int32 MV_LGS_SetTrigger_NET(UInt32 nTriggerSignal)
        {
            return MV_LGS_SetTrigger(handle, nTriggerSignal);
        }

        /// <summary>
        /// ch:读码器NoRead图像回调 | en:NoRead Image CallBack
        /// </summary>
        /// <param name="cbNoReadImageOutput">ch:NoRead图像输出回调函数指针 | en:NoRead Image Output CallBack Function Pointer</param>
        /// <param name="pUser">ch:用户自定义变量 | en:User defined variable</param>
        /// <returns>ch:成功, 返回MV_LGS_OK; 错误, 返回错误码 | en:Success, return MV_LGS_OK. Failure, return error code</returns>
        public Int32 MV_LGS_RegisterNoReadImageCB_NET(cbNoReaddelegate cbNoReadImageOutput, IntPtr pUser)
        {
            return MV_LGS_RegisterNoReadImageCB(handle, cbNoReadImageOutput, pUser);
        }

        /// <summary>
        /// ch:获取XML配置中的相机信息 | en:Get XML Config Camera Info
        /// </summary>
        /// <param name="pstXmlCfgCamInfo">ch:配置文件中对应相机信息列表 | en:Camera Info Lists</param>
        /// <param name="strCfgPath">ch:配置文件路径 | en:Config File Path</param>
        /// <returns>ch:成功, 返回MV_LGS_OK; 错误, 返回错误码 | en:Success, return MV_LGS_OK. Failure, return error code</returns>
        public Int32 MV_LGS_GetXmlCfgCamInfo_NET(ref MVLGS_XML_CFG_CAM_INFO_LIST pstXmlCfgCamInfo, String strCfgPath)
        {
            return MV_LGS_GetXmlCfgCamInfo(ref pstXmlCfgCamInfo, strCfgPath);
        }

        /// <summary>
        /// ch:获取相机句柄 | en:Get Camera Handle
        /// </summary>
        /// <returns>ch:返回相机句柄 | en:return camera handle</returns>
        public IntPtr GetCameraHandle()
        {
            return handle;
        }

        /// <summary>
        /// ch:设置流水号 | en:Set Running Number
        /// </summary>
        /// <param name="nRunNumber">ch:流水号 | en:Running Number</param>
        /// <param name="unBindTime">ch:流水号和触发号绑定的时间区间 | en:The time interval of the serial number and the trigger number binding</param>
        /// <returns>ch:成功, 返回MV_LGS_OK; 错误, 返回错误码 | en:Success, return MV_LGS_OK. Failure, return error code</returns>
        public Int32 MV_LGS_SetRunNumber_NET(UInt32 nRunNumber, UInt32 unBindTime)
        {
            return MV_LGS_SetRunNumber(handle, nRunNumber, unBindTime);
        }

        // 正确码定义

        /// <summary>ch:成功, 无错误 | en:Successed, no error</summary>
        public const Int32 MV_LGS_OK                    = unchecked((Int32)0x00000000);

        //通用错误码定义:范围0x80011000-0x801100FF

        /// <summary> ch:错误或无效的句柄 | en:Error or invalid handle</summary>
        public const Int32 MV_LGS_E_HANDLE              = unchecked((Int32)0x80110000);
        /// <summary> ch:不支持的功能 | en:The function is not supported</summary>
        public const Int32 MV_LGS_E_SUPPORT             = unchecked((Int32)0x80110001);
        /// <summary> ch:缓存已满 | en:Buffer is full</summary>
        public const Int32 MV_LGS_E_BUFOVER             = unchecked((Int32)0x80110002);
        /// <summary> ch:函数调用顺序错误 | en:Incorrect calling sequence</summary>
        public const Int32 MV_LGS_E_CALLORDER           = unchecked((Int32)0x80110003);
        /// <summary> ch:错误的参数 | en:Incorrect parameter</summary>
        public const Int32 MV_LGS_E_PARAMETER           = unchecked((Int32)0x80110004);
        /// <summary> ch:资源申请失败 | en:Applying resource failed</summary>
        public const Int32 MV_LGS_E_RESOURCE            = unchecked((Int32)0x80110005);
        /// <summary> ch:无数据 | en:No data</summary>
        public const Int32 MV_LGS_E_NODATA              = unchecked((Int32)0x80110006);
        /// <summary> ch:前置条件有误，或运行环境已发生变化 | en:Precondition error, or running environment changed</summary>
        public const Int32 MV_LGS_E_PRECONDITION        = unchecked((Int32)0x80110007);
        /// <summary> ch:凭证错误，可能是未插加密狗，或加密狗过期 | en:Credential error, possibly because the dongle was not installed or expired</summary>
        public const Int32 MV_LGS_E_ENCRYPT             = unchecked((Int32)0x80110008);
        /// <summary> ch:过滤规则相关的错误 | en:Filter rule error</summary>
        public const Int32 MV_LGS_E_RULE                = unchecked((Int32)0x8011000a);
        /// <summary> ch:jpg编码相关错误 | en:Jpg encoding error</summary>
        public const Int32 MV_LGS_E_JPGENC              = unchecked((Int32)0x80110012);
        /// <summary> ch:输入的图像数据有损或图像格式,宽高错误 | en:Abnormal image. Incomplete image caused by packet loss or incorrect image format, width, or height</summary>
        public const Int32 MV_LGS_E_IMAGE               = unchecked((Int32)0x80110013);
        /// <summary> ch:配置文件有误 | en:Config file error</summary>
        public const Int32 MV_LGS_E_CONFIG              = unchecked((Int32)0x80110014);
        /// <summary> ch:未知的错误 | en:Unknown error</summary>
        public const Int32 MV_LGS_E_UNKNOW              = unchecked((Int32)0x801100FF);

        /// <summary> ch:相机相关的错误 | en:Camera error</summary>
        public const Int32 MV_LGS_E_CAMERA              = unchecked((Int32)0x80112100);
        /// <summary> ch:一维码相关错误 | en:1D barcode error</summary>
        public const Int32 MV_LGS_E_BCR                 = unchecked((Int32)0x80112200);
        /// <summary> ch:二维码相关错误 | en:2D barcode error</summary>
        public const Int32 MV_LGS_E_TDCR                = unchecked((Int32)0x80112300);
        /// <summary> ch:抠图相关错误 | en:Matting error</summary>
        public const Int32 MV_LGS_E_WAYBILL             = unchecked((Int32)0x80112400);
        /// <summary> ch:脚本规则相关错误 | en:Script rule error</summary>
        public const Int32 MV_LGS_E_SCRIPT              = unchecked((Int32)0x80112500);

        // GenICam系列错误:范围0x80110100-0x801101FF

        /// <summary> ch:通用错误 | en:General error</summary>
        public const Int32 MV_LGS_E_GC_GENERIC          = unchecked((Int32)0x80110100);
        /// <summary> ch:参数非法 | en:Invalid parameter</summary>
        public const Int32 MV_LGS_E_GC_ARGUMENT         = unchecked((Int32)0x80110101);
        /// <summary> ch:值超出范围 | en:The value is out of range</summary>
        public const Int32 MV_LGS_E_GC_RANGE            = unchecked((Int32)0x80110102);
        /// <summary> ch:属性错误 | en:Attribute error</summary>
        public const Int32 MV_LGS_E_GC_PROPERTY         = unchecked((Int32)0x80110103);
        /// <summary> ch:运行环境有问题 | en:Running environment error</summary>
        public const Int32 MV_LGS_E_GC_RUNTIME          = unchecked((Int32)0x80110104);
        /// <summary> ch:逻辑错误 | en:Incorrect logic</summary>
        public const Int32 MV_LGS_E_GC_LOGICAL          = unchecked((Int32)0x80110105);
        /// <summary> ch:节点访问条件有误 | en:Node accessing condition error</summary>
        public const Int32 MV_LGS_E_GC_ACCESS           = unchecked((Int32)0x80110106);
        /// <summary> ch:超时 | en:Timeout</summary>
        public const Int32 MV_LGS_E_GC_TIMEOUT          = unchecked((Int32)0x80110107);
        /// <summary> ch:转换异常 | en:Transformation exception</summary>
        public const Int32 MV_LGS_E_GC_DYNAMICCAST      = unchecked((Int32)0x80110108);
        /// <summary> ch:GenICam未知错误 | en:GenICam unknown error</summary>
        public const Int32 MV_LGS_E_GC_UNKNOW           = unchecked((Int32)0x801101FF);

        //GigE_STATUS对应的错误码:范围0x80110200-0x801102FF

        /// <summary> ch:命令不被设备支持 | en:The command is not supported by device</summary>
        public const Int32 MV_LGS_E_NOT_IMPLEMENTED     = unchecked((Int32)0x80110200);
        /// <summary> ch:访问的目标地址不存在 | en:Target address does not exist</summary>
        public const Int32 MV_LGS_E_INVALID_ADDRESS     = unchecked((Int32)0x80110201);
        /// <summary> ch:目标地址不可写 | en:The target address is not writable</summary>
        public const Int32 MV_LGS_E_WRITE_PROTECT       = unchecked((Int32)0x80110202);
        /// <summary> ch:设备无访问权限 | en:No access permission</summary>
        public const Int32 MV_LGS_E_ACCESS_DENIED       = unchecked((Int32)0x80110203);
        /// <summary> ch:设备忙，或网络断开 | en:Device is busy, or network is disconnected</summary>
        public const Int32 MV_LGS_E_BUSY                = unchecked((Int32)0x80110204);
        /// <summary> ch:网络包数据错误 | en:Network packet error</summary>
        public const Int32 MV_LGS_E_PACKET              = unchecked((Int32)0x80110205);
        /// <summary> ch:网络相关错误 | en:Network error</summary>
        public const Int32 MV_LGS_E_NETER               = unchecked((Int32)0x80110206);

        // GigE相机特有的错误码

        /// <summary> ch:设备IP冲突 | en:IP address conflicted</summary>
        public const Int32 MV_LGS_E_IP_CONFLICT         = unchecked((Int32)0x80110221);

        //USB_STATUS对应的错误码:范围0x80110300-0x801103FF

        /// <summary> ch:读usb出错 | en:USB read error</summary>
        public const Int32 MV_LGS_E_USB_READ            = unchecked((Int32)0x80110300);
        /// <summary> ch:写usb出错 | en:USB write error</summary>
        public const Int32 MV_LGS_E_USB_WRITE           = unchecked((Int32)0x80110301);
        /// <summary> ch:设备异常 | en:Device exception</summary>
        public const Int32 MV_LGS_E_USB_DEVICE          = unchecked((Int32)0x80110302);
        /// <summary> ch:GenICam相关错误 | en:GenICam error</summary>
        public const Int32 MV_LGS_E_USB_GENICAM         = unchecked((Int32)0x80110303);
        /// <summary> ch:带宽不足  该错误码新增 | en:Insufficient bandwidth, this error code is newly added</summary>
        public const Int32 MV_LGS_E_USB_BANDWIDTH       = unchecked((Int32)0x80110304);
        /// <summary> ch:驱动不匹配或者未装驱动 | en:Driver is mismatched, or is not installed</summary>
        public const Int32 MV_LGS_E_USB_DRIVER          = unchecked((Int32)0x80110305);
        /// <summary> ch:USB未知的错误 | en:USB unknown error</summary>
        public const Int32 MV_LGS_E_USB_UNKNOW          = unchecked((Int32)0x801103FF);

        // 融合模块错误码

        /// <summary> ch:融合模块参数错误 | en:Fusion module parameter error</summary>
        public const Int32 MV_LGS_E_FUSION_PARAM        = unchecked((Int32)0x80112600);
        /// <summary> ch:融合模块内存分配失败 | en:Fusion module memory allocation failed</summary>
        public const Int32 MV_LGS_E_FUSION_MALLOC       = unchecked((Int32)0x80112601);
        /// <summary> ch:融合模块调用顺序错误 | en:Fusion module call sequence is wrong</summary>
        public const Int32 MV_LGS_E_FUSION_CALLORDER    = unchecked((Int32)0x80112602);
        /// <summary> ch:融合模块配置文件错误 | en:Fusion module configuration file error</summary>
        public const Int32 MV_LGS_E_FUSION_CFGFILE      = unchecked((Int32)0x80112603);
        /// <summary> ch:融合模块未知错误 | en:Unknown error of fusion module</summary>
        public const Int32 MV_LGS_E_FUSION_UNKNOWN      = unchecked((Int32)0x80112604);
        /// <summary> ch:融合模块缓存不足 | en:Insufficient Fusion Module Cache</summary>
        public const Int32 MV_LGS_E_FUSION_LACKBUF      = unchecked((Int32)0x80112605);
        /// <summary> ch:融合模块不支持 | en:Fusion module does not support</summary>
        public const Int32 MV_LGS_E_FUSION_SUPPORT      = unchecked((Int32)0x80112606);

        // 体积模块错误码

        /// <summary> ch:体积模块参数错误 | en:Volume module parameter error</summary>
        public const Int32 MV_LGS_E_VOLMEASURE_PARAM        = unchecked((Int32)0x80112700);
        /// <summary> ch:体积模块内存分配失败 | en:Volume module memory allocation failed</summary>
        public const Int32 MV_LGS_E_VOLMEASURE_MALLOC       = unchecked((Int32)0x80112701);
        /// <summary> ch:体积模块调用顺序错误 | en:Volume module calling sequence is wrong</summary>
        public const Int32 MV_LGS_E_VOLMEASURE_CALLORDER    = unchecked((Int32)0x80112702);
        /// <summary> ch:体积模块无数据 | en:No data for volume module</summary>
        public const Int32 MV_LGS_E_VOLMEASURE_NODATA       = unchecked((Int32)0x80112703);
        /// <summary> ch:体积模块配置文件错误 | en:Volume module configuration file error</summary>
        public const Int32 MV_LGS_E_VOLMEASURE_CFGFILE      = unchecked((Int32)0x80112704);
        /// <summary> ch:体积模块无包裹 | en:Volume module without package</summary>
        public const Int32 MV_LGS_E_VOLMEASURE_NOPKG        = unchecked((Int32)0x80112705);
        /// <summary> ch:体积模块未知错误 | en:Volume module unknown error</summary>
        public const Int32 MV_LGS_E_VOLMEASURE_UNKNOWN      = unchecked((Int32)0x80112706);
        /// <summary> ch:体积模块缓存不足 | en:Insufficient volume module cache</summary>
        public const Int32 MV_LGS_E_VOLMEASURE_LACKBUF      = unchecked((Int32)0x80112707);
        /// <summary> ch:体积模块不支持 | en:Volume module does not support</summary>
        public const Int32 MV_LGS_E_VOLMEASURE_SUPPORT      = unchecked((Int32)0x80112708);

        // 称重模块错误码

        /// <summary> ch:称重模块称重设备打开失败 | en:Weighing module weighing equipment failed to open</summary>
        public const Int32 MV_LGS_E_WGHT_OPEN           = unchecked((Int32)0x80112800);
        /// <summary> ch:称重模块加密错误 | en:Weighing module encryption error</summary>
        public const Int32 MV_LGS_E_WGHT_ENC            = unchecked((Int32)0x80112801);
        /// <summary> ch:称重模块资源初始化失败 | en:Weighing module resource initialization failed</summary>
        public const Int32 MV_LGS_E_WGHT_RESOURCE       = unchecked((Int32)0x80112802);
        /// <summary> ch:称重模块调用顺序错误 | en:Weighing module call sequence error</summary>
        public const Int32 MV_LGS_E_WGHT_CALLORDER      = unchecked((Int32)0x80112803);
        /// <summary> ch:称重模块指针类型参数为空 | en:Weighing module pointer type parameter is null</summary>
        public const Int32 MV_LGS_E_WGHT_NULL           = unchecked((Int32)0x80112804);
        /// <summary> ch:称重模块数值类型参数范围错误 | en:Weighing module numerical type parameter range error</summary>
        public const Int32 MV_LGS_E_WGHT_RANGE          = unchecked((Int32)0x80112805);
        /// <summary> ch:称重模块能力集错误 | en:Weighing module capability set error</summary>
        public const Int32 MV_LGS_E_WGHT_ENABLE         = unchecked((Int32)0x80112806);
        /// <summary> ch:称重模块其他内部错误 | en:Weighing module other internal errors</summary>
        public const Int32 MV_LGS_E_WGHT_UNKNOW         = unchecked((Int32)0x80112807);

        /// <summary>ch:设备重连成功 | en:The device is reconnect success</summary>
        public const Int32  MV_LGS_EXCEPTION_RECONNECT_DEV_SUCCESS  = 0x00008000;      

        /// <summary>ch:设备断开连接 | en:The device is disconnected</summary>
        public const Int32  MV_LGS_EXCEPTION_DEV_DISCONNECT         = 0x00008001;
        /// <summary>ch:加密狗掉线 | en:The softdog is disconnected</summary>
        public const Int32  MV_LGS_EXCEPTION_SOFTDOG_DISCONNECT     = 0x00008002;

        /// <summary>ch:开始触发 | en:Start trigger</summary>
        public const Int32 MV_LGS_BEGIN_TRIGGER = 1;
        /// <summary>ch:结束触发 | en:Stop trigger</summary>
        public const Int32 MV_LGS_STOP_TRIGGER  = 0;

        /// <summary> ch:条码类型 | en:Code type</summary>
        public enum MVLGS_CODE_TYPE
        {
            /// <summary>ch:无可识别条码 | en:No recognizable bar code</summary>
            MVLGS_CODE_NONE         = 0,

            // 二维码
            /// <summary>ch:DM码 | en:DM code</summary>
            MVLGS_CODE_TDCR_DM      = 1,
            /// <summary>ch:QR码 | en:QR code</summary>
            MVLGS_CODE_TDCR_QR      = 2,

            // 一维码
            /// <summary>ch:EAN8码 | en:EAN8 code</summary>
            MVLGS_CODE_BCR_EAN8     = 8,
            /// <summary>ch:UPCE码 | en:UPCE code</summary>
            MVLGS_CODE_BCR_UPCE     = 9,
            /// <summary>ch:UPCA码 | en:UPCA code</summary>
            MVLGS_CODE_BCR_UPCA     = 12,
            /// <summary>ch:EAN13码 | en:EAN13 code</summary>
            MVLGS_CODE_BCR_EAN13    = 13,
            /// <summary>ch:ISBN13码 | en:ISBN13 code</summary>
            MVLGS_CODE_BCR_ISBN13   = 14,
            /// <summary>ch:库德巴码 | en:Codabar code</summary>
            MVLGS_CODE_BCR_CODABAR  = 20,
            /// <summary>ch:交叉25码 | en:ITF25 code</summary>
            MVLGS_CODE_BCR_ITF25    = 25,
            /// <summary>ch:Code 39 | en:Code 39</summary>
            MVLGS_CODE_BCR_CODE39   = 39,
            /// <summary>ch:Code 93 | en:Code 93</summary>
            MVLGS_CODE_BCR_CODE93   = 93,
            /// <summary>ch:Code 128 | en:Code 128</summary>
            MVLGS_CODE_BCR_CODE128  = 128,
        };

        // 图像格式
        /// <summary>ch:图像格式 | en:Image Type</summary>
        public enum MVLGS_IMAGE_TYPE
        {
            /// <summary>ch:未定义 | en:Undefined format</summary>
            MVLGS_IMAGE_Undefined   = 0,
            /// <summary>ch:Mono8 | en:MONO8 format</summary>
            MVLGS_IMAGE_MONO8       = 1,
            /// <summary>ch:JPEG | en:JPEG format</summary>
            MVLGS_IMAGE_JPEG        = 2,
            /// <summary>ch:Bmp | en:BMP format</summary>
            MVLGS_IMAGE_BMP         = 3,
            /// <summary>ch:RGB24 | en:RGB format</summary>
            MVLGS_IMAGE_RGB24       = 4,
            /// <summary>ch:BGR24 | en:BGR format</summary>
            MVLGS_IMAGE_BGR24       = 5,
        };

        // 设备类型
        /// <summary> ch:设备类型 | en:Device type</summary>
        public enum MVLGS_DEVICE_TYPE
        {
            /// <summary> ch:工业相机 | en:industrial camera</summary>
            MVLGS_IDCAM                 = 0,
            /// <summary> ch:读码器 | en:Barcode reader</summary>
            MVLGS_CODEREADER            = 1,
            /// <summary> ch:线扫相机 | en:Line Scan Camera</summary>
            MVLGS_LINESCAN              = 2,
            /// <summary> ch:全景相机 | en:Panoramic camera</summary>
            MVLGS_PANORAMIC             = 3,
            /// <summary> ch:体积相机 | en:Volume Camera</summary>
            MVLGS_VOLUME                = 4,
            /// <summary> ch:称重 | en:Weighting</summary>
            MVLGS_WEIGHT                = 5,
        };

        /// <summary> ch:条码位置坐标 | en:Barcode position coordinates</summary>
        public struct MVLGS_POINT_I
        {
            /// <summary> ch:X坐标 | en:X coordinate</summary>
            public Int32 nX;
            /// <summary> ch:Y坐标 | en:Y coordinate</summary>
            public Int32 nY;
        };

        /// <summary> ch:最大条码长度 | en:Maximum barcode length</summary>
        public const Int32 MVLGS_MAX_CODECHARATERLEN       = 4096;
        /// <summary> ch:单张图片内最大条码个数 | en:Maximum number of barcodes in a single picture</summary>
        public const Int32 MVLGS_MAX_CODENUM               = 256;
        /// <summary> ch:最大条码信息列表个数 | en:Maximum number of barcode information lists</summary>
        public const Int32 MVLGS_MAX_CODELISTNUM           = 24;

        /// <summary>ch:条码信息 | en:Code information</summary>
        public struct MVLGS_CODE_INFO
        {
            /// <summary>ch:字符 | en:Character, maximum size: 4096</summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MVLGS_MAX_CODECHARATERLEN)]
            public string strCode;
            /// <summary>ch:字符长度 | en:Character size</summary>
            public Int32 nLen;
            /// <summary>ch:条码类型 | en:Bar code type</summary>
            public MVLGS_CODE_TYPE enBarType;
            /// <summary>ch:条码位置 | en:Bar code location</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public MVLGS_POINT_I[] stCornerPt;
            /// <summary>ch:条码角度（0~3600） | en:Bar code angle, range: [0, 3600°]</summary>
            public Int32 nAngle;
            /// <summary>ch:过滤码标识(0为正常码, 1为过滤码) | en:Filter identifier: 0- normal code, 1-filter code</summary>
            public Int32 nFilterFlag;
            /// <summary> ch:相机序列号 | en:Camera serial number</summary>
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string strSerialNumber;
            /// <summary>ch:保留 | en:Reserved</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 23)]
			public UInt32[] nReserved;                              // 保留
        };

        /// <summary>ch:条码信息 | en:Code information</summary>
        public struct MVLGS_CODE_INFOEx
        {
            /// <summary>ch:字符 | en:Character, maximum size: 4096</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MVLGS_MAX_CODECHARATERLEN)]           
            public byte[] strCode;                                  
            /// <summary>ch:字符长度 | en:Character size</summary>
            public Int32 nLen;
            /// <summary>ch:条码类型 | en:Bar code type</summary>         
            public MVLGS_CODE_TYPE enBarType;                       
            /// <summary>ch:条码位置 | en:Bar code location</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public MVLGS_POINT_I[] stCornerPt;                      
            /// <summary>ch:条码角度（0~3600） | en:Bar code angle, range: [0, 3600°]</summary>
            public Int32 nAngle;                                    
            /// <summary>ch:过滤码标识(0为正常码, 1为过滤码) | en:Filter identifier: 0- normal code, 1-filter code</summary>           
            public Int32 nFilterFlag;                               
            /// <summary> ch:相机序列号 | en:Camera serial number</summary>
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]    
            public string strSerialNumber;
            /// <summary>ch:保留 | en:Reserved</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 23)]
            public UInt32[] nReserved;                              
        };

        /// <summary>ch:输出图像的信息 | en:Output Frame Information</summary>
        public struct MVLGS_IMAGE_INFO
        {
            /// <summary> ch:原始图像缓存，由用户传入 | en:Original image buffer</summary>
            public IntPtr pImageBuf;
            /// <summary>ch:原始图像长度 | en:Original image size</summary>
            public UInt32 nImageLen;
            /// <summary>ch:图像格式 | en:Image Type</summary>
            public MVLGS_IMAGE_TYPE enImageType;
            /// <summary>ch:图像宽 | en:Image Width</summary>
            public UInt16 nWidth;
            /// <summary>ch:图像高 | en:Image Height</summary>
            public UInt16 nHeight;

            /// <summary> ch:触发序号，特殊场合可当做包裹号使用 | en:Trigger serial number, can be used as a parcel number in special occasions</summary>
            public UInt32 nTriggerIndex;
            /// <summary>ch:帧号 | en:Frame No.</summary>
            public UInt32 nFrameNum;
            /// <summary>ch:时间戳高32位 | en:Timestamp high 32 bits</summary>
            public UInt32 nDevTimeStampHigh;
            /// <summary>ch:时间戳低32位 | en:Timestamp low 32 bits</summary>
            public UInt32 nDevTimeStampLow;
            /// <summary>ch:用户自定义名字 | en:Custom name</summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
            public string chUserDefinedName;
            /// <summary>ch:保留 | en:Reserved</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public UInt32[] nReserved;
        };

        /// <summary> ch:条码信息列表 | en:List of barcode information</summary>
        public struct MVLGS_CODE_LIST
        {
            /// <summary> ch:条码数量 | en:Number of barcodes</summary>
            public Int32 nCodeNum;
            /// <summary> ch:条码信息 | en:Barcode information</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MVLGS_MAX_CODENUM)]
            public MVLGS_CODE_INFO[] stCodeInfo;
            /// <summary> ch:原始图像信息 | en:Original image information</summary>
            public MVLGS_IMAGE_INFO stImage;

            /// <summary> ch:抠图缓存，由SDK内部分配 | en:Cutout cache, allocated internally by the SDK</summary>
            public IntPtr pImageWaybill;
            /// <summary> ch:图像大小 | en:Image size</summary>
            public UInt32 nImageWaybillLen;
            /// <summary> ch:抠图图像格式 | en:Matte image format</summary>
            public MVLGS_IMAGE_TYPE enWaybillImageType;

            /// <summary>ch:保留 | en:Reserved</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
            public UInt32[] nReserved;
        };
        /// <summary> ch:条码信息列表 | en:List of barcode information</summary>
        public struct MVLGS_CODE_LISTEx
        {
            /// <summary> ch:条码数量 | en:Number of barcodes</summary>
            public Int32 nCodeNum;
            /// <summary> ch:条码信息 | en:Barcode information</summary>       
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MVLGS_MAX_CODENUM)]
            public MVLGS_CODE_INFOEx[] stCodeInfo;
            /// <summary> ch:原始图像信息 | en:Original image information</summary>
            public MVLGS_IMAGE_INFO stImage;
            /// <summary> ch:抠图缓存，由SDK内部分配 | en:Cutout cache, allocated internally by the SDK</summary>
            public IntPtr pImageWaybill;
            /// <summary> ch:图像大小 | en:Image size</summary> 
            public UInt32 nImageWaybillLen;
            /// <summary> ch:抠图图像格式 | en:Matte image format</summary>
            public MVLGS_IMAGE_TYPE enWaybillImageType;
            /// <summary>ch:保留 | en:Reserved</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
            public UInt32[] nReserved;                  // 保留
        };

        /// <summary> ch:包裹体积字段信息 | en:Package volume field information</summary>
        public struct MVLGS_VOLUME_INFO
        {
            /// <summary> ch:包裹ID | en:Package ID</summary>
            public UInt32 nPkgID;
            /// <summary> ch:体积 | en: volume</summary>
            public Single fVolume;
            /// <summary> ch:长度 | en: length</summary>
            public Single fLength;
            /// <summary> ch:宽度 | en:Width</summary>
            public Single fWidth;
            /// <summary> ch:高度 | en:Height</summary>
            public Single fHeight;
            /// <summary> ch:体积测量开始时间戳 | en:Volume measurement start timestamp</summary>
            public Int64  nObjEnterSysTime;
            /// <summary> ch:体积测量结束时间戳 | en:Volume measurement end timestamp</summary>
            public Int64  nObjLeaveSysTime;

            /// <summary>ch:保留 | en:Reserved</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 60)]
            public UInt32[] nReserved;
        };

        /// <summary> ch:包裹信息 | en:Package information</summary>
        public struct MVLGS_PACKAGE_INFO
        {
            /// <summary> ch:是否包含条码信息,非0：包含条码信息 | en:Whether to include barcode information, non-zero: contains barcode information</summary>
            public Boolean bCodeEnable;
            /// <summary> ch:条码列表数量 | en:Number of barcode lists</summary>
            public UInt32 nCodeListNum;
            /// <summary> ch:条码列表信息 | en:Bar code list information</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MVLGS_MAX_CODELISTNUM)]
            public MVLGS_CODE_LIST[] stCodeList;

            /// <summary> ch:是否包含体积信息,非0：包含体积信息 | en:Whether to include volume information, non-zero: contains volume information</summary>
            public Boolean bVolumeEnable;
            /// <summary> ch:体积信息 | en:Volume information</summary>
            public MVLGS_VOLUME_INFO stVolumeInfo;

            /// <summary> ch:是否包含重量信息,非0：包含重量信息 | en:Whether to include weight information, non-zero: contains weight information</summary>
            public Boolean bWeightEnable;
            /// <summary> ch:重量信息 | en:Weight information</summary>
            public Single fWeight;

            /// <summary> ch:是否包含全景图像,非0：包含全景图像 | en:Whether to include panoramic images, non-zero: include panoramic images</summary>
            public Boolean bPRMEnable;
            /// <summary> ch:原始图像信息 | en:Original image information</summary>
            public MVLGS_IMAGE_INFO stPRMImage;

            /// <summary> ch:是否为条码先输出 (1为第一次输出，0为第二次输出) | en:Whether to output the barcode first (1 is the first output, 0 is the second output)</summary>
            public Boolean bIsFirstCodeOut;

             /// <summary> ch:触发开始时间戳(13位) | en:Trigger start timestamp (13 bits)</summary>
            public Int64 llTriggerStartTimeStamp;
            /// <summary> ch:触发结束时间戳(13位) | en:Trigger end timestamp (13 bits)</summary>
            public Int64 llTriggerEndTimeStamp;

            /// <summary>ch:保留 | en:Reserved</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 27)]
            public UInt32[] nReserved;
        };
        /// <summary> ch:包裹信息 | en:Package information</summary>
        public struct MVLGS_PACKAGE_INFOEx
        {
            /// <summary> ch:是否包含条码信息,非0：包含条码信息 | en:Whether to include barcode information, non-zero: contains barcode information</summary>
            public Boolean bCodeEnable;
            /// <summary> ch:条码列表数量 | en:Number of barcode lists</summary>
            public UInt32 nCodeListNum;
            /// <summary> ch:条码列表信息 | en:Bar code list information</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MVLGS_MAX_CODELISTNUM)]
            public MVLGS_CODE_LISTEx[] stCodeList;
            /// <summary> ch:是否包含体积信息,非0：包含体积信息 | en:Whether to include volume information, non-zero: contains volume information</summary>
            public Boolean bVolumeEnable;
            /// <summary> ch:体积信息 | en:Volume information</summary>
            public MVLGS_VOLUME_INFO stVolumeInfo;
            /// <summary> ch:是否包含重量信息,非0：包含重量信息 | en:Whether to include weight information, non-zero: contains weight information</summary>
            public Boolean bWeightEnable;
            /// <summary> ch:重量信息 | en:Weight information</summary>
            public Single fWeight;
            /// <summary> ch:是否包含全景图像,非0：包含全景图像 | en:Whether to include panoramic images, non-zero: include panoramic images</summary>
            public Boolean bPRMEnable;
            /// <summary> ch:是否包含全景图像,非0：包含全景图像 | en:Whether to include panoramic images, non-zero: include panoramic images</summary>
            public MVLGS_IMAGE_INFO stPRMImage;
            /// <summary> ch:是否为条码先输出 (1为第一次输出，0为第二次输出) | en:Whether to output the barcode first (1 is the first output, 0 is the second output)</summary>
            public Boolean bIsFirstCodeOut;         // 是否为条码先输出
            /// <summary> ch:触发开始时间戳(13位) | en:Trigger start timestamp (13 bits)</summary>
			public Int64 llTriggerStartTimeStamp;
            /// <summary> ch:触发结束时间戳(13位) | en:Trigger end timestamp (13 bits)</summary>
			public Int64 llTriggerEndTimeStamp;      // 触发结束时间戳
            /// <summary>ch:保留 | en:Reserved</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 27)]
            public UInt32[] nReserved;              // 保留
        };

        /// <summary> ch:异常信息 | en:Exception information</summary>
        public struct MVLGS_EXCEPTION_INFO
        {
            /// <summary> ch:异常ID | en:Exception ID</summary>
            public UInt32 nExceptionID;
            /// <summary> ch:异常设备类型 | en:Abnormal device type</summary>
            public MVLGS_DEVICE_TYPE enCamType;

            /// <summary> ch:异常设备序列号(体积相机此处为MAC地址) | en:Abnormal device serial number (the volume camera here is the MAC address)</summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string strCamSerialNum;

            /// <summary> ch:异常描述 | en:Exception description</summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string strExceptionDes;

            /// <summary>ch:保留 | en:Reserved</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public UInt32[] nReserved;
        };

         /// <summary> ch:触发信息 | en:Trigger information</summary>
        public struct MVLGS_TRIGGER_INFO
        {
            /// <summary> ch:触发ID | en:Trigger ID</summary>
	        public UInt32    nTriggerIndex;
            /// <summary> ch:触发标识 与头文件触发开始为1(MV_LGS_BEGIN_TRIGGER) 触发结束为0(MV_LGS_STOP_TRIGGER) 对应映射| en:Trigger description</summary>          
	        public UInt32    nTriggerFlag;                            

             /// <summary>ch:保留 | en:Reserved</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public UInt32[] nReserved;                   
        }

        /// <summary> ch:相机输出图像信息 | en:Camera output image information</summary>
        public struct MVLGS_IMAGE_OUTPUT_INFO
        {
            /// <summary> ch:图像信息 | en:Image information</summary>
            public MVLGS_IMAGE_INFO stImage;
            /// <summary> ch:相机序列号 | en:Camera serial number</summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string strSerialNumber;

            /// <summary>ch:保留 | en:Reserved</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public UInt32[] nReserved;
        };

        /// <summary> ch:设备信息 | en:Device Information</summary>
        public struct MVLGS_XML_CFG_CAM_INFO
        {
            /// <summary> ch:相机类型 | en:Camera type</summary>
            public MVLGS_DEVICE_TYPE enXmlCamType;

            ///<summary>ch:MAC 地址 高位| en:High MAC address</summary>
            public UInt32 nMacAddrHigh;
            /// <summary>ch:MAC 地址 低位| en:Low MAC address</summary>
            public UInt32 nMacAddrLow;
            /// <summary>ch:当前IP | en:Current IP address</summary> 
            public UInt32 nCurrentIp;

            /// <summary>ch:制造商名字 | en:Manufacturer Name</summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string strManufacturerName;
            /// <summary>ch:型号名字 | en:Model Name</summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string strModelName;
            /// <summary>ch:设备版本号 | en:Device version No.</summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string strDeviceVersion;
            /// <summary>ch:序列号 | en:Device serial No.</summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string strSerialNumber;
            /// <summary>ch:用户自定义名字 | en:Custom name</summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
            public string strUserDefinedName;

            /// <summary> ch:设备在线状态，true在线，false离线 | en:Device online status, true online, false offline</summary>
            [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.I1)]
            public bool bDeviceOnline;

            /// <summary> ch:是否为主相机 | en:Whether the main camera</summary>
            [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.I1)]
            public bool bMainCamera;

            /// <summary>ch:保留 | en:Reserved</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public UInt32[] nReserved;
        };

        /// <summary> ch:XML配置文件中相机信息列表 | en:List of camera information in XML configuration file</summary>
        public struct MVLGS_XML_CFG_CAM_INFO_LIST
        {
            /// <summary> ch:XML配置文件中的相机数量 | en:Number of cameras in XML configuration file</summary>
            public UInt32 nXmlCfgCamNum;

            /// <summary> ch:相机信息结构体数组 | en:Camera information structure array</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public IntPtr[] pstXmlCamInfo;
        };



        // 私有成员变量
        /// <summary> ch:设备句柄 | en:Device handle</summary>
        IntPtr handle;

        // 从C/C++接口库导出的函数
        [DllImport("MvLogisticsSDK.dll", EntryPoint = "MV_LGS_GetVersion")]
        private static extern Int32 MV_LGS_GetVersion();

        [DllImport("MvLogisticsSDK.dll", EntryPoint = "MV_LGS_CreateHandle")]
        private static extern Int32 MV_LGS_CreateHandle(ref IntPtr handle);

        [DllImport("MvLogisticsSDK.dll", EntryPoint = "MV_LGS_DestroyHandle")]
        private static extern Int32 MV_LGS_DestroyHandle(IntPtr handle);

        [DllImport("MvLogisticsSDK.dll", EntryPoint = "MV_LGS_LoadDevCfg")]
        private static extern Int32 MV_LGS_LoadDevCfg(IntPtr handle, String strCfgPath);

        [DllImport("MvLogisticsSDK.dll", EntryPoint = "MV_LGS_RegisterExceptionCB")]
        private static extern Int32 MV_LGS_RegisterExceptionCB(IntPtr handle, cbExceptiondelegate cbException, IntPtr pUser);

        [DllImport("MvLogisticsSDK.dll", EntryPoint = "MV_LGS_RegisterPackageCB")]
        private static extern Int32 MV_LGS_RegisterPackageCB(IntPtr handle, cbOutputdelegate cbOutput, IntPtr pUser);

        [DllImport("MvLogisticsSDK.dll", EntryPoint = "MV_LGS_RegisterTriggerInfoCB")]
        private static extern Int32 MV_LGS_RegisterTriggerInfoCB(IntPtr handle, cbTriggerOutputdelegate cbTriggerInfoOutput, IntPtr pUser);
        
        [DllImport("MvLogisticsSDK.dll", EntryPoint = "MV_LGS_Start")]
        private static extern Int32 MV_LGS_Start(IntPtr handle);

        [DllImport("MvLogisticsSDK.dll", EntryPoint = "MV_LGS_Stop")]
        private static extern Int32 MV_LGS_Stop(IntPtr handle);

        [DllImport("MvLogisticsSDK.dll", EntryPoint = "MV_LGS_SetTrigger")]
        private static extern Int32 MV_LGS_SetTrigger(IntPtr handle, UInt32 nTriggerSignal);

        [DllImport("MvLogisticsSDK.dll", EntryPoint = "MV_LGS_RegisterNoReadImageCB")]
        private static extern Int32 MV_LGS_RegisterNoReadImageCB(IntPtr handle, cbNoReaddelegate cbNoReadImageOutput, IntPtr pUser);

        [DllImport("MvLogisticsSDK.dll", EntryPoint = "MV_LGS_GetXmlCfgCamInfo")]
        private static extern Int32 MV_LGS_GetXmlCfgCamInfo(ref MVLGS_XML_CFG_CAM_INFO_LIST pstXmlCfgCamInfo, String strCfgPath);

        [DllImport("MvLogisticsSDK.dll", EntryPoint = "MV_LGS_SetRunNumber")]
        private static extern Int32 MV_LGS_SetRunNumber(IntPtr handle, UInt32 nRunNumber, UInt32 unBindTime);
    }
}
