
// Grab_MSCDlg.h : header file

#pragma once
#include "afxwin.h"
#include "MvCodeReaderCtrl.h"
#include "MvCodeReaderErrorDefine.h"
#include "MvCodeReaderParams.h"
#include "MvCodeReaderPixelType.h"
#include "turbojpeg.h"
#include "GdiPlus.h"
#include <map>

#if _MSC_VER >= 1900
#pragma comment(lib, "legacy_stdio_definitions.lib")
#endif

#pragma comment(lib,"gdiplus.lib")

using namespace std;
using namespace Gdiplus;

// 节点宏定义
#define RUNNING_MODE                "RunningMode"
#define TRIGGER_MODE                "TriggerMode"
#define TRIGGER_SOURCE              "TriggerSource"
#define TRIGGER_SOFTWARE            "TriggerSoftware"
#define EXPOSURE_TIME               "ExposureTime"
#define GAIN                        "Gain"
#define ACQUISITION_FRAME_RATE      "AcquisitionFrameRate"
#define CHANNEL_NUM                 "GevMessageChannelCnt"
#define OUT_CHANNEL                 "OutputChannelSelector"

#define Camera_Width                "WidthMax"
#define Camera_Height               "HeightMax"
#define Camera_Width1               "WidthMax1"
#define Camera_Height1              "HeightMax1"
#define Camera_PayloadSize          "PayloadSize"

#define ChannelNum                  4
#define MSCRawRunMode               1
#define NormalRunMode               0
#define MSCTestRunMode              2
#define ImageExLen                  4096

// 解压JPG图像输出结构体
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

// 图像展示结构体
typedef struct _MV_CODEREADER_DRAW_PARAM_
{
    HDC hDC;
    unsigned char *pData;

    int nImageWidth;
    int nImageHeight;

    int nWndRectWidth;
    int nWndRectHeight;
    int nDstX;
    int nDstY;

}MV_CODEREADER_DRAW_PARAM;


// CGrab_MSCDlg dialog 对话框
class CGrab_MSCDlg : public CDialog
{
// Construction 构造
public:
    CGrab_MSCDlg(CWnd* pParent = NULL); // standard constructor 标准构造函数

// Dialog Data 对话框数据
    enum { IDD = IDD_GRAB_MSC_DIALOG };

protected:
    virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support 支持

// 实现
protected:
    HICON m_hIcon;

    // Generated message map functions 生成的消息映射函数
    virtual BOOL OnInitDialog();
    afx_msg void OnSysCommand(UINT nID, LPARAM lParam);
    afx_msg void OnPaint();
    afx_msg HCURSOR OnQueryDragIcon();
    DECLARE_MESSAGE_MAP()

public:
    CComboBox m_ctrlDeviceCombo;
    afx_msg void OnBnClickedEnumButton();
    afx_msg void OnBnClickedOpenButton();
    afx_msg void OnBnClickedCloseButton();
    afx_msg void OnBnClickedContinusModeRadio();
    afx_msg void OnBnClickedTriggerModeRadio();
    afx_msg void OnBnClickedSoftwareTriggerCheck();
    afx_msg void OnBnClickedSoftwareOnceButton();
    afx_msg void OnBnClickedStartGrabbingButton();
    afx_msg void OnBnClickedStopGrabbingButton();
    
    afx_msg void OnBnClickedSaveBmpButton();
    afx_msg void OnBnClickedSaveJpgButton();
    afx_msg void OnBnClickedSaveRawButton();
    afx_msg void OnBnClickedGetParameterButton();
    afx_msg void OnBnClickedSetParameterButton();
    afx_msg void OnClose();

    int InitResources();
    void DeInitResources();
    void DestoryThreadHandle();

    bool IsStrUTF8(const char* pBuffer, int size);
    bool Char2Wchar(const char *pStr, wchar_t *pOutWStr, int nOutStrSize);
    bool Wchar2char(wchar_t *pOutWStr, char *pStr);

private:
    BOOL    PreTranslateMessage(MSG* pMsg);
    int     CloseDevice(void);
    int     GetCurrentConfigurationInformation();                             // 获取相机运行模式状态信息
    static void*  __stdcall WINAPI Channel0ProcessThread(void* pUser);        // 流通道0图像显示线程
    static void*  __stdcall WINAPI Channel1ProcessThread(void* pUser);        // 流通道1图像显示线程

    int             Display(void* hWnd, unsigned char *pData, MV_CODEREADER_IMAGE_OUT_INFO_EX2* pstDisplayImage);
    unsigned int    InnerDrawShape(Graphics* g, float x, float y, float w, float h, float fAngle);
    int             MvJpgDecompress(IN OUT MV_CODEREADER_TJPG_PARAM* pstParam);
    int             Draw(MV_CODEREADER_DRAW_PARAM* pstParam);
    int             SaveJpg();
    char* GetCurrentProGramPath(char* pFilePath, int nSize);

private:
    MV_CODEREADER_DEVICE_INFO_LIST      m_stDeviceInfoList;           // 设备信息列表
    MV_CODEREADER_DEVICE_INFO           m_stDeviceInfo;               // 设备信息
    MV_CODEREADER_IMAGE_OUT_INFO_EX2*   m_pstImgInfoEx2[ChannelNum];  // 图像信息
    MV_CODEREADER_DRAW_PARAM            m_stParam[ChannelNum];        // 输出图像的结构体
    MV_CODEREADER_TJPG_PARAM            m_pstParam[ChannelNum];       // 解压JPG图像输出结构体

    void*                   m_handle;                               // 设备句柄
    void*                   m_hWndChannel0Display;                  // 显示流通道号0的窗口句柄
    void*                   m_hWndChannel1Display;                  // 显示流通道号1的窗口句柄
    HANDLE                  m_hChannel0Thread;                      // 通道0的取流线程
    HANDLE                  m_hChannel1Thread;                      // 通道1的取流线程

    bool                    m_bConnect;                             // 是否设备已连接
    bool                    m_bStartJob;                            // 是否工作线程已开启
    unsigned char*          m_pcDataBuf[ChannelNum];                // 存储图像数据
    int                     m_MaxImageSize[ChannelNum];             // 图像最大尺寸
    CCriticalSection        m_criSection;                           // 临界区
    BITMAPINFO*             m_bBitmapInfo;                          // 图像数据转换BIT结构体
    bool                    m_bMSCCamera;                           // 是否是多通道相机
    unsigned int            m_nChannelNum;                          // 流通道个数
    unsigned int            m_nRunMode;                             // 相机运行模式
    unsigned int            m_nOutRawChannel;                       // RAW模式出图的流通道
    bool                    m_bIsSoftTrigger;                       // 是否勾选软触发控件

    // 界面按钮信息
    CButton     m_ctrlOpenButton;
    CButton     m_ctrlCloseButton;
    CButton     m_ctrlContinusModeRadio;
    CButton     m_ctrlTriggerModeRadio;
    CButton     m_ctrlSoftwareOnceButton;
    CButton     m_ctrlSoftwareTriggerCheck;
    CButton     m_ctrlStartGrabbingButton;
    CButton     m_ctrlStopGrabbingButton;
    CButton     m_ctrlSaveBmpButton;
    CButton     m_ctrlSaveJpgButton;
    CButton     m_ctrlSaveRawButton;
    CButton     m_ctrlGetParameterButton;
    CButton     m_ctrlSetParameterButton;
    CEdit       m_ctrlExposureEdit;
    CEdit       m_ctrlGainEdit;
    CEdit       m_ctrlFrameRateEdit;

};
