
// BasicDemoDlg.h : 头文件

#pragma once
#include "afxwin.h"
#include "MvCodeReaderCtrl.h"
#include "MvCodeReaderErrorDefine.h"
#include "MvCodeReaderParams.h"
#include "MvCodeReaderPixelType.h"
#include "turbojpeg.h"
#include "GdiPlus.h"
#include <map>
#include "afxcmn.h"
#include <string>

#if _MSC_VER >= 1900
#pragma comment(lib, "legacy_stdio_definitions.lib")
#endif

#pragma comment(lib,"gdiplus.lib")
using namespace std;
using namespace Gdiplus;

// 定义消息ID
#define WM_DISPALY_CHANGE   (WM_USER + 1)

#define BYTE_ALIGN              4                 // 4字节对齐
#define RUNNING_MODE            "RunningMode"
#define TRIGGER_MODE            "TriggerMode"
#define TRIGGER_SOURCE          "TriggerSource"
#define TRIGGER_SOFTWARE        "TriggerSoftware"
#define EXPOSURE_TIME           "ExposureTime"
#define GAIN                    "Gain"
#define ACQUISITION_FRAME_RATE  "AcquisitionFrameRate"

#define Camera_Width             "WidthMax"
#define Camera_Height            "HeightMax"
#define Camera_PayloadSize       "PayloadSize"
#define ImageExLen                4096

#define IMAGE_NAME_LEN          64

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

// CBasicDemoDlg 对话框
class CBasicDemoDlg : public CDialog
{
// 构造
public:
	CBasicDemoDlg(CWnd* pParent = NULL);	// 标准构造函数

// 对话框数据
	enum { IDD = IDD_BasicDemo_DIALOG };

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV 支持

    int InitResources();
    void DeInitResources();

    bool IsStrUTF8(const char* pBuffer, int size);
    bool Char2Wchar(const char *pStr, wchar_t *pOutWStr, int nOutStrSize);
    bool Wchar2char(wchar_t *pOutWStr, char *pStr);


// 实现
protected:
	HICON m_hIcon;

	// 生成的消息映射函数
	virtual BOOL OnInitDialog();
	afx_msg void OnSysCommand(UINT nID, LPARAM lParam);
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	DECLARE_MESSAGE_MAP()
public:
    afx_msg void OnBnClickedEnumButton();
    afx_msg void OnBnClickedOpenButton();
    afx_msg void OnBnClickedCloseButton();
    afx_msg void OnBnClickedStopGrabbingButton();
    afx_msg void OnBnClickedStartGrabbingButton();
    afx_msg void OnBnClickedContinusModeRadio();
    afx_msg void OnBnClickedTriggerModeRadio();
    afx_msg void OnBnClickedSoftwareTriggerCheck();
    afx_msg void OnBnClickedSoftwareOnceButton();
    afx_msg void OnBnClickedSaveBmpButton();
    afx_msg void OnBnClickedSaveJpgButton();
    afx_msg void OnBnClickedSaveRawButton();
    afx_msg void OnBnClickedGetParameterButton();
    afx_msg void OnBnClickedSetParameterButton();
    afx_msg void OnClose();
private:
    BOOL    PreTranslateMessage(MSG* pMsg);
    int     GetCurrentConfigurationInformation();                         // 获取相机运行状态信息(运行模式以及触发模式)
    static void*  __stdcall WINAPI ProcessThread(void* pUser);            // 图像显示线程
    int SaveJpg();
    int OpenDevice();
    int CloseDevice(void);
    string GetBarType(unsigned int nBarType);
    int Display(void* hWnd, unsigned char *pData, MV_CODEREADER_IMAGE_OUT_INFO_EX2* pstDisplayImage);
    int  MvJpgDecompress(IN OUT MV_CODEREADER_TJPG_PARAM* pstParam);
    int   Draw(MV_CODEREADER_DRAW_PARAM* pstParam);
    unsigned int InnerDrawShape(Graphics* g, float x, float y, float w, float h, float fAngle);
    char* GetCurrentProGramPath(char* pFilePath, int nSize);

private:
    MV_CODEREADER_DEVICE_INFO_LIST  m_stDeviceInfoList;             // 设备信息列表
    MV_CODEREADER_DEVICE_INFO       m_stDeviceInfo;                 // 设备信息

    void*                   m_handle;                               // 设备句柄
    void*                   m_hWndDisplay;                          // 显示窗口句柄
    bool                    m_bConnect;                             // 是否设备已连接
    bool                    m_bStartJob;                            // 是否工作线程已开启
    unsigned char*          m_pcDataBuf;                            // 存储图像数据
    int                     m_MaxImageSize;                         // 图像最大尺寸
    CCriticalSection        m_criBcrSection;                           // 临界区
    MV_CODEREADER_RESULT_BCR_EX2* m_pstBcrResultEx2;                    // 码制信息
    
    MV_CODEREADER_IMAGE_OUT_INFO_EX2*   m_pstImageInfoEx2;          // 图像信息
    CCriticalSection        m_criSection;                           // 临界区
    unsigned char*          m_pBufForDriver;                        // 用于从驱动获取图像的缓存
    unsigned int            m_nBufSizeForDriver;
    unsigned char*          m_pBufForSaveImage;                     // 用于保存图像的缓存
    unsigned int            m_nBufSizeForSaveImage;
    BITMAPINFO*             m_bBitmapInfo;
	HANDLE                  m_hProcessThread;                      // 取流线程

    MV_CODEREADER_DRAW_PARAM m_stParam;                             // 自己构建的结构体
    MV_CODEREADER_TJPG_PARAM m_pstParam ;                           // 解压JPG图像输出结构体
    bool                    m_bIsSoftTrigger;                       // 是否勾选软触发控件

private:
    CButton     m_ctrlOpenButton;
    CButton     m_ctrlCloseButton;
    CButton     m_ctrlContinusModeRadio;
    CButton     m_ctrlTriggerModeRadio;
    CButton     m_ctrlSoftwareTriggerCheck;
    CButton     m_ctrlStartGrabbingButton;
    CButton     m_ctrlStopGrabbingButton;
    CButton     m_ctrlSoftwareOnceButton;
    CButton     m_ctrlSaveBmpButton;
    CButton     m_ctrlSaveJpgButton;
    CButton     m_ctrlGetParameterButton;
    CButton     m_ctrlSetParameterButton;

private:
    CEdit       m_ctrlExposureEdit;
    CEdit       m_ctrlGainEdit;
    CEdit       m_ctrlFrameRateEdit;
    CComboBox   m_ctrlDeviceCombo;

    CListCtrl m_ctrlDisplayInfoList;
};
