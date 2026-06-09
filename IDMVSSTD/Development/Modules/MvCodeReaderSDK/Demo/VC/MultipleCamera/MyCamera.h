/************************************************************************/
/* 以C++接口为基础，对常用函数进行二次封装，方便用户使用                */
/************************************************************************/

#ifndef _MY_CAMERA_H_
#define _MY_CAMERA_H_

#include <stdio.h>
#include "MvCodeReaderCtrl.h"
#include "MvCodeReaderErrorDefine.h"
#include "MvCodeReaderParams.h"
#include "MvCodeReaderPixelType.h"
#include "turbojpeg.h"
#include "GdiPlus.h"
#include <map>

#pragma comment(lib,"gdiplus.lib")
using namespace std;
using namespace Gdiplus;

typedef struct _MV_CODEREADER_DRAW_PARAM_
{
    HDC hDC;
    unsigned char *pdata;

    int nImageWidth;
    int nImageHeight;

    int nWndRectWidth;
    int nWndRectHeight;
    int nDstX;
    int nDstY;

}MV_CODEREADER_DRAW_PARAM;

typedef struct _MV_CODEREADER_TJPG_PARAM_
{
    unsigned char*  pBufInput;
    unsigned int    nBufInputLen;

    unsigned int    nWidth;
    unsigned int    nHeight;

    unsigned int    nJpgQuality;

    unsigned char*  pBufOutput;
    unsigned int    nBufOutputLen;

}MV_CODEREADER_TJPG_PARAM;

class CMyCamera
{
public:
    CMyCamera();
    ~CMyCamera();

    static int EnumDevices(MV_CODEREADER_DEVICE_INFO_LIST* pstDevList);

    // ch:打开设备 | en:Open Device
    int     Open(char* chSerialNumber);

    // ch:关闭设备 | en:Close Device
    int     Close();

    int InitResources();
    void DeInitResources();

    // ch:开启抓图 | en:Start Grabbing
    int     StartGrabbing();

    // ch:停止抓图 | en:Stop Grabbing
    int     StopGrabbing();

    // ch:主动获取一帧图像数据 | en:Get one frame initiatively
    int     GetOneFrameTimeout(unsigned char** pData, unsigned int* pnDataLen, MV_CODEREADER_IMAGE_OUT_INFO_EX2* pFrameInfo, int nMsec);


    // ch:保存图片 | en:save image
    int     SaveImage(MV_CODEREADER_SAVE_IMAGE_PARAM_EX* pstParam);

    // ch:获取Int型参数，如 Width和Height，详细内容参考SDK安装目录下的 MvCameraNode.xlsx 文件
    // en:Get Int type parameters, such as Width and Height, for details please refer to MvCameraNode.xlsx file under SDK installation directory
    int     GetIntValue(IN const char* strKey, OUT unsigned int *pnValue);
    int     SetIntValue(IN const char* strKey, IN unsigned int nValue);

    // ch:获取Float型参数，如 ExposureTime和Gain，详细内容参考SDK安装目录下的 MvCameraNode.xlsx 文件
    // en:Get Float type parameters, such as ExposureTime and Gain, for details please refer to MvCameraNode.xlsx file under SDK installation directory
    int     GetFloatValue(IN const char* strKey, OUT float *pfValue);
    int     SetFloatValue(IN const char* strKey, IN float fValue);

    // ch:获取Enum型参数，如 PixelFormat，详细内容参考SDK安装目录下的 MvCameraNode.xlsx 文件
    // en:Get Enum type parameters, such as PixelFormat, for details please refer to MvCameraNode.xlsx file under SDK installation directory
    int     GetEnumValue(IN const char* strKey, OUT unsigned int *pnValue);
    int     SetEnumValue(IN const char* strKey, IN unsigned int nValue);

    // ch:获取Bool型参数，如 ReverseX，详细内容参考SDK安装目录下的 MvCameraNode.xlsx 文件
    // en:Get Bool type parameters, such as ReverseX, for details please refer to MvCameraNode.xlsx file under SDK installation directory
    int     GetBoolValue(IN const char* strKey, OUT bool *pbValue);
    int     SetBoolValue(IN const char* strKey, IN bool bValue);

    // ch:获取String型参数，如 DeviceUserID，详细内容参考SDK安装目录下的 MvCameraNode.xlsx 文件UserSetSave
    // en:Get String type parameters, such as DeviceUserID, for details please refer to MvCameraNode.xlsx file under SDK installation directory
    int     GetStringValue(IN const char* strKey, IN OUT char* strValue, IN unsigned int nSize);
    int     SetStringValue(IN const char* strKey, IN const char * strValue);

    // ch:执行一次Command型命令，如 UserSetSave，详细内容参考SDK安装目录下的 MvCameraNode.xlsx 文件
    // en:Execute Command once, such as UserSetSave, for details please refer to MvCameraNode.xlsx file under SDK installation directory
    int     CommandExecute(IN const char* strKey);

    // ch:探测网络最佳包大小(只对GigE相机有效) | en:Detection network optimal package size(It only works for the GigE camera)
    int     GetOptimalPacketSize();

    // 获取统计参数
    int     Process(HWND hDisplay, bool bIsStartGrab);
    static void*  __stdcall WINAPI ProcessThread(void* pUser);
    
    // ch:设置显示窗口句柄 | en:Set Display Window Handle
    int Display(void* hWnd,  unsigned char *pdata, MV_CODEREADER_IMAGE_OUT_INFO_EX2* pstDisplayImage);
    int  MvJpgDecompress(IN OUT MV_CODEREADER_TJPG_PARAM* pstParam);
    int   Draw(MV_CODEREADER_DRAW_PARAM* stParam);

	unsigned int InnerDrawShape(Graphics* g, float x, float y, float w, float h, float fAngle);
public:
    void*            m_hDevHandle;
public:
    unsigned char*   m_pBufForSaveImage;         // 用于保存图像的缓存
    unsigned int     m_nBufSizeForSaveImage;

    unsigned char*  m_pBufForDriver;            // 用于从驱动获取图像的缓存
    unsigned int    m_nBufSizeForDriver;
    HWND            m_hDisplay;
    bool            m_bStartGrabbing;
    BITMAPINFO*             m_bBitmapInfo;
    MV_CODEREADER_DRAW_PARAM m_stParam;    // 自己构建的结构体
    MV_CODEREADER_TJPG_PARAM m_pstParam ;  // 解压JPG图像输出结构体


};

#endif
