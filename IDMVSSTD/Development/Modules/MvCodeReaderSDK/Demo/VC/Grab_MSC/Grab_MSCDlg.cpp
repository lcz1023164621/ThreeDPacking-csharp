
// Grab_MSCDlg.cpp : implementation file 实现文件

#include "stdafx.h"
#include "Grab_MSC.h"
#include "Grab_MSCDlg.h"
#include <string.h> 
#include <comdef.h>
#include <gdiplus.h>
#include <WinGDI.h> 

using namespace Gdiplus;
#pragma comment( lib, "gdiplus.lib" ) 

#if (_MSC_VER >= 1900)
extern "C"
{
	FILE __iob_func[3] = { *stdin,*stdout,*stderr };
}
#endif

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

class CAboutDlg : public CDialog
{
public:
    CAboutDlg();

    // Dialog Data 对话框数据
    enum { IDD = IDD_ABOUTBOX };

protected:
    virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support

// Implementation
protected:
    DECLARE_MESSAGE_MAP()
};

CAboutDlg::CAboutDlg() : CDialog(CAboutDlg::IDD)
{
}

void CAboutDlg::DoDataExchange(CDataExchange* pDX)
{
    CDialog::DoDataExchange(pDX);
}

BEGIN_MESSAGE_MAP(CAboutDlg, CDialog)
END_MESSAGE_MAP()

CGrab_MSCDlg::CGrab_MSCDlg(CWnd* pParent /*=NULL*/)
    : CDialog(CGrab_MSCDlg::IDD, pParent)
    , m_bConnect(FALSE)
    , m_bStartJob(FALSE)
    , m_nChannelNum(0)
    , m_nRunMode(0)
    , m_nOutRawChannel(0)
    , m_bIsSoftTrigger(0)
    , m_bBitmapInfo(NULL)
    , m_handle(NULL)
    , m_hWndChannel0Display(NULL)
    , m_hWndChannel1Display(NULL)
    , m_hChannel0Thread(NULL)
    , m_hChannel1Thread(NULL)
    , m_bMSCCamera(FALSE)
{
    memset(&m_stDeviceInfoList, 0, sizeof(m_stDeviceInfoList));
    memset(&m_stDeviceInfo, 0, sizeof(m_stDeviceInfo));
    memset(&m_stParam, 0, sizeof(MV_CODEREADER_DRAW_PARAM) * ChannelNum);
    memset(&m_pstParam, 0, sizeof(MV_CODEREADER_TJPG_PARAM) * ChannelNum);

    for (int i = 0; i < ChannelNum; i++)
    {
        m_pcDataBuf[i]      = NULL;
        m_MaxImageSize[i]   = 0;
        m_pstImgInfoEx2[i]   = NULL;
    }

    m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);
}

void CGrab_MSCDlg::DoDataExchange(CDataExchange* pDX)
{
    CDialog::DoDataExchange(pDX);
    DDX_Control(pDX, IDC_DEVICE_COMBO, m_ctrlDeviceCombo);
    DDX_Control(pDX, IDC_OPEN_BUTTON, m_ctrlOpenButton);
    DDX_Control(pDX, IDC_CLOSE_BUTTON, m_ctrlCloseButton);
    DDX_Control(pDX, IDC_CONTINUS_MODE_RADIO, m_ctrlContinusModeRadio);
    DDX_Control(pDX, IDC_TRIGGER_MODE_RADIO, m_ctrlTriggerModeRadio);
    DDX_Control(pDX, IDC_SOFTWARE_ONCE_BUTTON, m_ctrlSoftwareOnceButton);
    DDX_Control(pDX, IDC_SOFTWARE_TRIGGER_CHECK, m_ctrlSoftwareTriggerCheck);
    DDX_Control(pDX, IDC_START_GRABBING_BUTTON, m_ctrlStartGrabbingButton);
    DDX_Control(pDX, IDC_STOP_GRABBING_BUTTON, m_ctrlStopGrabbingButton);
    DDX_Control(pDX, IDC_SAVE_BMP_BUTTON, m_ctrlSaveBmpButton);
    DDX_Control(pDX, IDC_SAVE_JPG_BUTTON, m_ctrlSaveJpgButton);
    DDX_Control(pDX, IDC_SAVE_RAW_BUTTON, m_ctrlSaveRawButton);
    DDX_Control(pDX, IDC_GET_PARAMETER_BUTTON, m_ctrlGetParameterButton);
    DDX_Control(pDX, IDC_SET_PARAMETER_BUTTON, m_ctrlSetParameterButton);
    DDX_Control(pDX, IDC_EXPOSURE_EDIT, m_ctrlExposureEdit);
    DDX_Control(pDX, IDC_GAIN_EDIT, m_ctrlGainEdit);
    DDX_Control(pDX, IDC_FRAME_RATE_EDIT, m_ctrlFrameRateEdit);

}

BEGIN_MESSAGE_MAP(CGrab_MSCDlg, CDialog)
    ON_WM_SYSCOMMAND()
    ON_WM_PAINT()
    ON_WM_QUERYDRAGICON()
    //}}AFX_MSG_MAP
    ON_BN_CLICKED(IDC_ENUM_BUTTON, &CGrab_MSCDlg::OnBnClickedEnumButton)
    ON_BN_CLICKED(IDC_OPEN_BUTTON, &CGrab_MSCDlg::OnBnClickedOpenButton)
    ON_BN_CLICKED(IDC_CLOSE_BUTTON, &CGrab_MSCDlg::OnBnClickedCloseButton)
    ON_BN_CLICKED(IDC_CONTINUS_MODE_RADIO, &CGrab_MSCDlg::OnBnClickedContinusModeRadio)
    ON_BN_CLICKED(IDC_TRIGGER_MODE_RADIO, &CGrab_MSCDlg::OnBnClickedTriggerModeRadio)
    ON_BN_CLICKED(IDC_START_GRABBING_BUTTON, &CGrab_MSCDlg::OnBnClickedStartGrabbingButton)
    ON_BN_CLICKED(IDC_STOP_GRABBING_BUTTON, &CGrab_MSCDlg::OnBnClickedStopGrabbingButton)
    ON_BN_CLICKED(IDC_SAVE_BMP_BUTTON, &CGrab_MSCDlg::OnBnClickedSaveBmpButton)
    ON_BN_CLICKED(IDC_SAVE_JPG_BUTTON, &CGrab_MSCDlg::OnBnClickedSaveJpgButton)
    ON_BN_CLICKED(IDC_SAVE_RAW_BUTTON, &CGrab_MSCDlg::OnBnClickedSaveRawButton)
    ON_BN_CLICKED(IDC_GET_PARAMETER_BUTTON, &CGrab_MSCDlg::OnBnClickedGetParameterButton)
    ON_BN_CLICKED(IDC_SET_PARAMETER_BUTTON, &CGrab_MSCDlg::OnBnClickedSetParameterButton)
    ON_BN_CLICKED(IDC_SOFTWARE_TRIGGER_CHECK, &CGrab_MSCDlg::OnBnClickedSoftwareTriggerCheck)
    ON_BN_CLICKED(IDC_SOFTWARE_ONCE_BUTTON, &CGrab_MSCDlg::OnBnClickedSoftwareOnceButton)
    ON_WM_CLOSE()
END_MESSAGE_MAP()


BOOL CGrab_MSCDlg::OnInitDialog()
{
    CDialog::OnInitDialog();

    ASSERT((IDM_ABOUTBOX & 0xFFF0) == IDM_ABOUTBOX);
    ASSERT(IDM_ABOUTBOX < 0xF000);

    CMenu* pSysMenu = GetSystemMenu(FALSE);
    if (pSysMenu != NULL)
    {
        BOOL bNameValid;
        CString strAboutMenu;
        bNameValid = strAboutMenu.LoadString(IDS_ABOUTBOX);
        ASSERT(bNameValid);
        if (!strAboutMenu.IsEmpty())
        {
            pSysMenu->AppendMenu(MF_SEPARATOR);
            pSysMenu->AppendMenu(MF_STRING, IDM_ABOUTBOX, strAboutMenu);
        }
        }

    SetIcon(m_hIcon, TRUE);
    SetIcon(m_hIcon, FALSE);

    // 创建多通道对应窗口句柄
    CWnd *pWndChannel0 = GetDlgItem(IDC_CHANNEL0_DISPLAY_STATIC);
    if (NULL == pWndChannel0)
    {
        return MV_CODEREADER_E_RESOURCE;
    }
    m_hWndChannel0Display = pWndChannel0->GetSafeHwnd();
    if (NULL == m_hWndChannel0Display)
    {
        return MV_CODEREADER_E_RESOURCE;
    }

    GdiplusStartupInput gdiplusStartupInput0;
    ULONG_PTR gdiplusToken0;

    // 初始化GDI+
    GdiplusStartup(&gdiplusToken0, &gdiplusStartupInput0, NULL);

    CWnd *pWndChannel1 = GetDlgItem(IDC_CHANNEL1_DISPLAY_STATIC);
    if (NULL == pWndChannel1)
    {
        return MV_CODEREADER_E_RESOURCE;
    }
    m_hWndChannel1Display = pWndChannel1->GetSafeHwnd();
    if (NULL == m_hWndChannel1Display)
    {
        return MV_CODEREADER_E_RESOURCE;
    }

    GdiplusStartupInput gdiplusStartupInput1;
    ULONG_PTR gdiplusToken1;

    // 初始化GDI+
    GdiplusStartup(&gdiplusToken1, &gdiplusStartupInput1, NULL);

    // 将按钮置灰
    GetDlgItem(IDC_OPEN_BUTTON)->EnableWindow(FALSE);
    GetDlgItem(IDC_CLOSE_BUTTON)->EnableWindow(FALSE);
    GetDlgItem(IDC_START_GRABBING_BUTTON)->EnableWindow(FALSE);
    GetDlgItem(IDC_STOP_GRABBING_BUTTON)->EnableWindow(FALSE);
    GetDlgItem(IDC_SAVE_BMP_BUTTON)->EnableWindow(FALSE);
    GetDlgItem(IDC_SAVE_JPG_BUTTON)->EnableWindow(FALSE);
    GetDlgItem(IDC_SAVE_RAW_BUTTON)->EnableWindow(FALSE);
    GetDlgItem(IDC_SOFTWARE_ONCE_BUTTON)->EnableWindow(FALSE);
    GetDlgItem(IDC_GET_PARAMETER_BUTTON)->EnableWindow(FALSE);
    GetDlgItem(IDC_SET_PARAMETER_BUTTON)->EnableWindow(FALSE);
    m_ctrlContinusModeRadio.EnableWindow(FALSE);
    m_ctrlTriggerModeRadio.EnableWindow(FALSE);
    m_ctrlSoftwareTriggerCheck.EnableWindow(FALSE);
    m_ctrlExposureEdit.EnableWindow(FALSE);
    m_ctrlGainEdit.EnableWindow(FALSE);
    m_ctrlFrameRateEdit.EnableWindow(FALSE);

    return TRUE;
}

// ch:判断字符类型 | en:str type
bool CGrab_MSCDlg::IsStrUTF8(const char* pBuffer, int size)
{
    if (size < 0)
    {
        return false;
    }

    bool IsUTF8 = true;
    unsigned char* start = (unsigned char*)pBuffer;
    unsigned char* end = (unsigned char*)pBuffer + size;
    if (NULL == start ||
        NULL == end)
    {
        return false;
    }
    while (start < end)
    {
        if (*start < 0x80) // ch:(10000000): 值小于0x80的为ASCII字符 | en:(10000000): if the value is smaller than 0x80, it is the ASCII character
        {
            start++;
        }
        else if (*start < (0xC0)) // ch:(11000000): 值介于0x80与0xC0之间的为无效UTF-8字符 | en:(11000000): if the value is between 0x80 and 0xC0, it is the invalid UTF-8 character
        {
            IsUTF8 = false;
            break;
        }
        else if (*start < (0xE0)) // ch:(11100000): 此范围内为2字节UTF-8字符  | en: (11100000): if the value is between 0xc0 and 0xE0, it is the 2-byte UTF-8 character
        {
            if (start >= end - 1)
            {
                break;
            }

            if ((start[1] & (0xC0)) != 0x80)
            {
                IsUTF8 = false;
                break;
            }

            start += 2;
        }
        else if (*start < (0xF0)) // ch:(11110000): 此范围内为3字节UTF-8字符 | en: (11110000): if the value is between 0xE0 and 0xF0, it is the 3-byte UTF-8 character 
        {
            if (start >= end - 2)
            {
                break;
            }

            if ((start[1] & (0xC0)) != 0x80 || (start[2] & (0xC0)) != 0x80)
            {
                IsUTF8 = false;
                break;
            }

            start += 3;
        }
        else
        {
            IsUTF8 = false;
            break;
        }
    }

    return IsUTF8;
}

// ch: 单字节转宽字节 | en: char convert to Wchar
bool CGrab_MSCDlg::Char2Wchar(const char *pStr, wchar_t *pOutWStr, int nOutStrSize)
{
    if (!pStr || !pOutWStr)
    {
        return false;
    }

    bool bIsUTF = IsStrUTF8(pStr, strlen(pStr));
    UINT nChgType = bIsUTF ? CP_UTF8 : CP_ACP;

    int iLen = MultiByteToWideChar(nChgType, 0, (LPCSTR)pStr, -1, NULL, 0);

    memset(pOutWStr, 0, sizeof(wchar_t) * nOutStrSize);

    if (iLen >= nOutStrSize)
    {
        iLen = nOutStrSize - 1;
    }

    MultiByteToWideChar(nChgType, 0, (LPCSTR)pStr, -1, pOutWStr, iLen);

    pOutWStr[iLen] = 0;

    return true;
}

// ch: 宽字节转单字节 | en: Wchar convert to char
bool CGrab_MSCDlg::Wchar2char(wchar_t *pOutWStr, char *pStr)
{
    if (!pStr || !pOutWStr)
    {
        return false;
    }

    int nLen =  WideCharToMultiByte(CP_ACP, 0, pOutWStr, wcslen(pOutWStr), NULL, 0, NULL, NULL);

    WideCharToMultiByte(CP_ACP, 0 , pOutWStr, wcslen(pOutWStr), pStr, nLen, NULL, NULL);

    pStr[nLen] = '\0';

    return true;
}

int CGrab_MSCDlg::InitResources()
{
    int nRet = MV_CODEREADER_OK;
    CString cstrInfo;

    try
    {
        int nSensorWidth = 0;
        int nSensorHight = 0;

        // 区分多通道固件与单通道固件
        MV_CODEREADER_INTVALUE_EX stValue = {0};
        nRet = MV_CODEREADER_GetIntValue(m_handle, CHANNEL_NUM, &stValue);
        if (MV_CODEREADER_OK != nRet && MV_CODEREADER_E_GC_GENERIC != nRet && MV_CODEREADER_E_UNKNOW != nRet)
        {
            cstrInfo.Format(_T("Get ChannelNum failed! err code:%#x"), nRet);
            MessageBox(cstrInfo);
            throw nRet;
        }

        if (MV_CODEREADER_E_GC_GENERIC == nRet || 1 == stValue.nCurValue || MV_CODEREADER_E_UNKNOW == nRet)
        {
            // 兼容原单通道设备无该节点信息 & 兼容虚拟相机
            m_nChannelNum = 1;
            m_bMSCCamera = FALSE;
        }
        else
        {
            m_nChannelNum = stValue.nCurValue;       // 得到通道个数
            m_bMSCCamera = TRUE;                     // 多通道系列设备
        }

        // 获取Camera_PayloadSize
        MV_CODEREADER_INTVALUE_EX stParam;
        memset(&stParam, 0, sizeof(MV_CODEREADER_INTVALUE_EX));
        nRet = MV_CODEREADER_GetIntValue(m_handle, Camera_PayloadSize, &stParam);
        if (MV_CODEREADER_OK != nRet)
        {
            // 无该节点则适用最大宽高为payloadSize
            memset(&stParam, 0, sizeof(MV_CODEREADER_INTVALUE_EX));
            nRet = MV_CODEREADER_GetIntValue(m_handle, Camera_Width, &stParam);
            if (MV_CODEREADER_OK != nRet)
            {
                cstrInfo.Format(_T("Get width failed! err code:%#x"), nRet);
                MessageBox(cstrInfo);
                throw nRet;
            }
            nSensorWidth = stParam.nCurValue;

            memset(&stParam, 0, sizeof(MV_CODEREADER_INTVALUE_EX));
            nRet = MV_CODEREADER_GetIntValue(m_handle, Camera_Height, &stParam);
            if (MV_CODEREADER_OK != nRet)
            {
                cstrInfo.Format(_T("Get height failed! err code:%#x"), nRet);
                MessageBox(cstrInfo);
                throw nRet;
            }
            nSensorHight = stParam.nCurValue;

            m_MaxImageSize[0] = nSensorWidth * nSensorHight + ImageExLen;
        }
        else
        {
            // 获取payloadSize成功
            m_MaxImageSize[0] = stParam.nCurValue + ImageExLen;
        }

        // 区分多通道固件与单通道固件
        if (true == m_bMSCCamera)
        {
            memset(&stParam, 0, sizeof(MV_CODEREADER_INTVALUE_EX));
            nRet = MV_CODEREADER_GetIntValue(m_handle, Camera_Width1, &stParam);
            if (MV_CODEREADER_OK != nRet && MV_CODEREADER_E_GC_GENERIC != nRet)
            {
                cstrInfo.Format(_T("Get width1 failed! err code:%#x"), nRet);
                MessageBox(cstrInfo);
                throw nRet;
            }
            nSensorWidth = stParam.nCurValue;

            memset(&stParam, 0, sizeof(MV_CODEREADER_INTVALUE_EX));
            nRet = MV_CODEREADER_GetIntValue(m_handle, Camera_Height1, &stParam);
            if (MV_CODEREADER_OK != nRet)
            {
                cstrInfo.Format(_T("Get height1 failed! err code:%#x"), nRet);
                MessageBox(cstrInfo);
                throw nRet;
            }
            nSensorHight = stParam.nCurValue;

            m_MaxImageSize[1] = nSensorWidth * nSensorHight + ImageExLen;
        }

        for (int i = 0; i < m_nChannelNum; i++)
        {
            m_pstParam[i].pBufOutput = (unsigned char*)malloc(m_MaxImageSize[i]);
            if (NULL == m_pstParam[i].pBufOutput)
            {
                nRet = MV_CODEREADER_E_RESOURCE;
                throw nRet;
            }
            memset(m_pstParam[i].pBufOutput, 0, m_MaxImageSize[i]);

            m_stParam[i].pData = (unsigned char*)malloc(m_MaxImageSize[i]);
            if (NULL == m_stParam[i].pData)
            {
                nRet = MV_CODEREADER_E_RESOURCE;
                throw nRet;
            }
            memset(m_stParam[i].pData, 0, m_MaxImageSize[i]);

            m_pcDataBuf[i] =  (unsigned char*)malloc(m_MaxImageSize[i]);
            if (NULL == m_pcDataBuf[i])
            {
                nRet = MV_CODEREADER_E_RESOURCE;
                throw nRet;
            }
            memset(m_pcDataBuf[i], 0, m_MaxImageSize[i]);

            // 存储图像信息
            m_pstImgInfoEx2[i] = (MV_CODEREADER_IMAGE_OUT_INFO_EX2*)malloc(sizeof(MV_CODEREADER_IMAGE_OUT_INFO_EX2)); 
            if (NULL == m_pstImgInfoEx2[i])
            {
                nRet = MV_CODEREADER_E_RESOURCE;
                throw nRet;
            }
            memset(m_pstImgInfoEx2[i], 0, sizeof(MV_CODEREADER_IMAGE_OUT_INFO_EX2));
        }

    }
    catch (...)
    {
        DeInitResources();
        return nRet;
    }

    return nRet;
}

void CGrab_MSCDlg::DestoryThreadHandle()
{
    if (NULL != m_hChannel0Thread)
    {
        //等待线程结束，关闭释放线程
        WaitForSingleObject(m_hChannel0Thread, 1000);
        CloseHandle(m_hChannel0Thread);
        m_hChannel0Thread = NULL;
    }

    if (NULL != m_hChannel1Thread)
    {
        //等待线程结束，关闭释放线程
        WaitForSingleObject(m_hChannel1Thread, 1000);
        CloseHandle(m_hChannel1Thread);
        m_hChannel1Thread = NULL;
    }
}


void CGrab_MSCDlg::DeInitResources()
{
    for (unsigned int i = 0; i < m_nChannelNum; i++)
    {
        if (NULL != m_pstParam[i].pBufOutput)
        {
            free(m_pstParam[i].pBufOutput);
            m_pstParam[i].pBufOutput = NULL;
        }

        if (NULL != m_stParam[i].pData)
        {
            free(m_stParam[i].pData);
            m_stParam[i].pData = NULL;
        }

        if (NULL != m_pcDataBuf[i])
        {
            free(m_pcDataBuf[i]);
            m_pcDataBuf[i] = NULL;
        }

        if (NULL != m_pstImgInfoEx2[i])
        {
            free(m_pstImgInfoEx2[i]);
            m_pstImgInfoEx2[i] = NULL;
        }
    }

}

void CGrab_MSCDlg::OnSysCommand(UINT nID, LPARAM lParam)
{
    if ((nID & 0xFFF0) == IDM_ABOUTBOX)
    {
        CAboutDlg dlgAbout;
        dlgAbout.DoModal();
    }
    else
    {
        CDialog::OnSysCommand(nID, lParam);
    }
}

void CGrab_MSCDlg::OnPaint()
{
    if (IsIconic())
    {
        CPaintDC dc(this); // 用于绘制的设备上下文

        SendMessage(WM_ICONERASEBKGND, reinterpret_cast<WPARAM>(dc.GetSafeHdc()), 0);

        // 使图标在工作区矩形中居中
        int cxIcon = GetSystemMetrics(SM_CXICON);
        int cyIcon = GetSystemMetrics(SM_CYICON);
        CRect rect;
        GetClientRect(&rect);
        int x = (rect.Width() - cxIcon + 1) / 2;
        int y = (rect.Height() - cyIcon + 1) / 2;

        // 绘制图标
        dc.DrawIcon(x, y, m_hIcon);
    }
    else
    {
        CDialog::OnPaint();
    }
}

HCURSOR CGrab_MSCDlg::OnQueryDragIcon()
{
    return static_cast<HCURSOR>(m_hIcon);
}

BOOL CGrab_MSCDlg::PreTranslateMessage(MSG* pMsg)
{
    // 屏蔽ESC和ENTER按键
    if (pMsg->message == WM_KEYDOWN && pMsg->wParam == VK_ESCAPE)
    {
        return TRUE;
    }
    if (pMsg->message == WM_KEYDOWN && pMsg->wParam == VK_RETURN) 
    {
        return TRUE;
    }
    else
    {
        return CDialog::PreTranslateMessage(pMsg);
    }
}

// 渲染流通道号为0的线程
void*  __stdcall CGrab_MSCDlg::Channel0ProcessThread(void* pUser)
{
    int nRet = MV_CODEREADER_OK;

    CGrab_MSCDlg* pThis = (CGrab_MSCDlg*)pUser;
    if (NULL == pThis)
    {
        return NULL;
    }

    MV_CODEREADER_IMAGE_OUT_INFO_EX2 stImageInfo = {0};
    unsigned char * pData = NULL;
    while (pThis->m_bStartJob)
    {
        // 获取图像数据
        nRet = MV_CODEREADER_MSC_GetOneFrameTimeout(pThis->m_handle, &pData, &stImageInfo, 0, 1000);
        if (MV_CODEREADER_OK == nRet)
        {
            if (NULL != pData)
            {
                // 输出图像结果
                nRet = pThis->Display(pThis->m_hWndChannel0Display, pData, &stImageInfo);
                if (MV_CODEREADER_OK != nRet)
                {
                    continue;
                }
            }
        }
        else
        {
            if (MV_CODEREADER_E_PARAMETER == nRet)
            {
                break;
            }
            continue;
        }
    }
    return NULL;
}

// 渲染流通道号为1的线程
void*  __stdcall CGrab_MSCDlg::Channel1ProcessThread(void* pUser)
{
    int nRet = MV_CODEREADER_OK;

    CGrab_MSCDlg* pThis = (CGrab_MSCDlg*)pUser;
    if (NULL == pThis)
    {
        return NULL;
    }

    MV_CODEREADER_IMAGE_OUT_INFO_EX2 stImageInfo = {0};
    unsigned char * pData = NULL;
    while (pThis->m_bStartJob)
    {
        // 获取图像数据
        nRet = MV_CODEREADER_MSC_GetOneFrameTimeout(pThis->m_handle, &pData, &stImageInfo, 1, 1000);
        if (MV_CODEREADER_OK == nRet)
        {
            if (NULL != pData)
            {
                // 输出图像结果
                nRet = pThis->Display(pThis->m_hWndChannel1Display, pData, &stImageInfo);
                if (MV_CODEREADER_OK != nRet)
                {
                    continue;
                }
            }
        }
        else
        {
            if (MV_CODEREADER_E_PARAMETER == nRet)
            {
                break;
            }
            continue;
        }
    }
    return NULL;
}

int CGrab_MSCDlg::Display(void* hWnd,  unsigned char *pData, MV_CODEREADER_IMAGE_OUT_INFO_EX2* pstDisplayImage)
{
    int nRet = MV_CODEREADER_OK;

    if ((NULL == pData) || (NULL == pstDisplayImage))
    {
        return MV_CODEREADER_E_PARAMETER;
    }

    if (pstDisplayImage->nWidth == 0 || pstDisplayImage->nHeight == 0 || NULL == hWnd)
    {
        return MV_CODEREADER_E_PARAMETER;
    }

    unsigned int ID = pstDisplayImage->nChannelID;
    memcpy(m_pstImgInfoEx2[ID], pstDisplayImage, sizeof(MV_CODEREADER_IMAGE_OUT_INFO_EX2));

    // 显示图像
    HDC hDC  = ::GetDC((HWND)hWnd);
    SetStretchBltMode(hDC, COLORONCOLOR);
    RECT wndRect;
    ::GetClientRect((HWND)hWnd, &wndRect);

    int nWndRectWidth  = wndRect.right  - wndRect.left;
    int nWndRectHeight = wndRect.bottom - wndRect.top;
    int nDstWidth  = (int)(nWndRectWidth);
    int nDstHeight = (int)(nWndRectHeight);
    int nDstX      = wndRect.left;
    int nDstY      = wndRect.top; 

    int nImageWidth = pstDisplayImage->nWidth;
    int nImageHeight = pstDisplayImage->nHeight;
    int nSrcX      = 0;
    int nSrcY      = 0;
    int nSrcWidth  = (int)(nImageWidth);
    int nSrcHeight = (int)(nImageHeight);

    // 给结构体赋值
    m_stParam[ID].hDC = hDC;
    m_stParam[ID].nDstX = nDstX;
    m_stParam[ID].nDstY = nDstY;
    m_stParam[ID].nImageHeight = nImageHeight;
    m_stParam[ID].nImageWidth = nImageWidth;
    m_stParam[ID].nWndRectHeight = nWndRectHeight;
    m_stParam[ID].nWndRectWidth = nWndRectWidth;

    // 将图像数据赋值给m_pcDataBuf，用于保存图像信息 
    memcpy(m_pcDataBuf[ID], pData, m_pstImgInfoEx2[ID]->nFrameLen);

    if (PixelType_CodeReader_Gvsp_Jpeg == pstDisplayImage->enPixelType)
    {
        memset(m_pstParam[ID].pBufOutput, 0, nImageWidth * nImageHeight);

        m_pstParam[ID].pBufInput    = pData;
        m_pstParam[ID].nBufInputLen = pstDisplayImage->nFrameLen;
        nRet = MvJpgDecompress(&m_pstParam[ID]);
        if (MV_CODEREADER_OK == nRet)
        {
            // 将Jpeg数据格式解压的数据封装为结构体，赋值作为参数传入，进行渲染
            memcpy(m_stParam[ID].pData, m_pstParam[ID].pBufOutput, m_pstParam[ID].nBufOutputLen);
            nRet = Draw(&m_stParam[ID]);
            if (MV_CODEREADER_OK != nRet)
            {
                return nRet = MV_CODEREADER_E_PARAMETER;
            }
        }
        else
        {
            // 解压失败
        }

    }
    else // 除Jpeg格式外的相机数据渲染
    {
        memset(m_stParam[ID].pData, 0, nImageWidth * nImageHeight);
        // 相机获取的数据直接渲染
        memcpy(m_stParam[ID].pData, pData, nImageWidth * nImageHeight);
        nRet = Draw(&m_stParam[ID]);
        if (MV_CODEREADER_OK != nRet)
        {
            return MV_CODEREADER_E_PARAMETER;
        }
    }

    // 识别到的条码进行画框
    Graphics gCode((HWND)hWnd);
    Status nGdiStatus = Status::Ok;
    Pen pen(Color(0, 0, 255), 3);

    float fWidthProportion = (float)nWndRectWidth / nImageWidth;
    float fHeightProportion = (float)nWndRectHeight / nImageHeight;

    MV_CODEREADER_RESULT_BCR_EX* stBcrResult = (MV_CODEREADER_RESULT_BCR_EX*)pstDisplayImage->pstCodeListEx;

    for (int i = 0; i < stBcrResult->nCodeNum; i++)
    {
        PointF point1((stBcrResult->stBcrInfoEx[i].pt[0].x * fWidthProportion), (stBcrResult->stBcrInfoEx[i].pt[0].y * fHeightProportion));
        PointF point2((stBcrResult->stBcrInfoEx[i].pt[1].x * fWidthProportion), (stBcrResult->stBcrInfoEx[i].pt[1].y * fHeightProportion));
        PointF point3((stBcrResult->stBcrInfoEx[i].pt[2].x * fWidthProportion), (stBcrResult->stBcrInfoEx[i].pt[2].y * fHeightProportion));
        PointF point4((stBcrResult->stBcrInfoEx[i].pt[3].x * fWidthProportion), (stBcrResult->stBcrInfoEx[i].pt[3].y * fHeightProportion));
        PointF points[4] = {point1, point2, point3, point4};
        PointF* pPoints = points;
        gCode.DrawPolygon(&pen, pPoints, 4);
    }

    Pen pen1(Color(255, 255, 0), 3);
    MV_CODEREADER_WAYBILL_LIST* pstWaybillList = (MV_CODEREADER_WAYBILL_LIST*)pstDisplayImage->pstWaybillList;
    for (int i = 0; i < pstWaybillList->nWaybillNum; i++)
    {
        int x = pstWaybillList->stWaybillInfo[i].fCenterX * fWidthProportion;
        int y = pstWaybillList->stWaybillInfo[i].fCenterY * fHeightProportion;
        int w = pstWaybillList->stWaybillInfo[i].fWidth * fWidthProportion;
        int h = pstWaybillList->stWaybillInfo[i].fHeight * fHeightProportion;
        InnerDrawShape(&gCode, x, y, w, h, pstWaybillList->stWaybillInfo[i].fAngle);
    }

	MV_CODEREADER_OCR_INFO_LIST* pOcrList = (MV_CODEREADER_OCR_INFO_LIST*)pstDisplayImage->UnparsedOcrList.pstOcrList;
	for (int i = 0; i < pOcrList->nOCRAllNum; i++)
	{
		int x = pOcrList->stOcrRowInfo[i].nOcrRowCenterX * fWidthProportion;
		int y = pOcrList->stOcrRowInfo[i].nOcrRowCenterY * fHeightProportion;
		int w = pOcrList->stOcrRowInfo[i].nOcrRowWidth * fWidthProportion;
		int h = pOcrList->stOcrRowInfo[i].nOcrRowHeight * fHeightProportion;

		InnerDrawShape(&gCode, x, y, w, h, pOcrList->stOcrRowInfo[i].fOcrRowAngle);
	}
    ::ReleaseDC((HWND)hWnd, hDC);
    return nRet;
}

unsigned int CGrab_MSCDlg::InnerDrawShape(Graphics* g, float x, float y, float w, float h, float fAngle)
{
    /* 路径初始化 */
    Status nGdiStatus = Status::Ok;
    GraphicsPath m_stShapePath;    ///< 图形路径，内部变量 
    nGdiStatus = m_stShapePath.Reset();
    if ( Status::Ok != nGdiStatus )
    {
        return 1;
    }

    float fGdiAngle = fAngle;
    if (fGdiAngle < 0)
    {
        fGdiAngle += 360;
    }

    /* 添加当前矩形至路径 */
    nGdiStatus = m_stShapePath.AddRectangle(RectF(x - w * 0.5, y - h*0.5, w, h));
    if ( Status::Ok != nGdiStatus )
    {
        return 1;
    }
    /* 根据角度旋转路径 */
    //Matrix* stRotateM = new Matrix();
    Matrix stRotateM;
    PointF stCenPoint( x, y );
    stRotateM.RotateAt( fAngle, stCenPoint );
    nGdiStatus = m_stShapePath.Transform(&stRotateM);

    if ( Status::Ok != nGdiStatus )
    {
        return 1;
    }

    /* 根据是否选中用不同画笔绘制图形 */
    Pen pen2(Color(255, 255, 0), 3);

    nGdiStatus = g->DrawPath(&pen2, &m_stShapePath);
    if ( Status::Ok != nGdiStatus )
    {
        return 1;
    }

    return 0;
}


int  CGrab_MSCDlg::Draw(MV_CODEREADER_DRAW_PARAM* pstParam)
{
    if (NULL == pstParam)
    {
        return MV_CODEREADER_E_PARAMETER;
    }   

    int nImageWidth = pstParam->nImageWidth;
    int nImageHeight = pstParam->nImageHeight;
    int nDstWidth  = (int)(pstParam->nWndRectWidth);
    int nDstHeight = (int)(pstParam->nWndRectHeight);
    int nSrcX      = 0;
    int nSrcY      = 0;
    int nSrcWidth  = (int)(nImageWidth);
    int nSrcHeight = (int)(nImageHeight);

    if (NULL == m_bBitmapInfo)
    {
        m_bBitmapInfo = (PBITMAPINFO)malloc(sizeof(BITMAPINFO) + 256 * sizeof(RGBQUAD));
        memset(m_bBitmapInfo, 0, sizeof(sizeof(BITMAPINFO) + 256 * sizeof(RGBQUAD)));
    }
    // 位图信息头
    m_bBitmapInfo->bmiHeader.biSize = sizeof(BITMAPINFOHEADER);             // BITMAPINFOHEADER结构长度
    m_bBitmapInfo->bmiHeader.biWidth = nImageWidth;                         // 图像宽度
    m_bBitmapInfo->bmiHeader.biPlanes = 1;                                  // 位面数
    m_bBitmapInfo->bmiHeader.biBitCount = 8;                                // 比特数/像素的颜色深度,2^8=256
    m_bBitmapInfo->bmiHeader.biCompression = BI_RGB;                        // 图像数据压缩类型,BI_RGB表示不压缩
    m_bBitmapInfo->bmiHeader.biSizeImage = nImageWidth * nImageHeight;      // 图像大小
    m_bBitmapInfo->bmiHeader.biHeight = - nImageHeight;                     // 图像高度

    for(int i = 0; i < 256; i++)
    {
        m_bBitmapInfo->bmiColors[i].rgbBlue = m_bBitmapInfo->bmiColors[i].rgbRed = m_bBitmapInfo->bmiColors[i].rgbGreen = i;
        m_bBitmapInfo->bmiColors[i].rgbReserved = 0;
    }

    int nRet = StretchDIBits(pstParam->hDC,
        pstParam->nDstX, pstParam->nDstY, nDstWidth, nDstHeight,
        nSrcX, nSrcY, nSrcWidth, nSrcHeight, pstParam->pData, m_bBitmapInfo, DIB_RGB_COLORS, SRCCOPY);

    return MV_CODEREADER_OK;
}

// jpg解码
int     CGrab_MSCDlg::MvJpgDecompress(IN OUT MV_CODEREADER_TJPG_PARAM* pstParam)
{
    if (NULL == pstParam)
    {
        return MV_CODEREADER_E_PARAMETER;
    }

    int nRet        = MV_CODEREADER_OK;
    int nWidth      = 0;
    int nHeight     = 0;
    int nSubsample  = 0;
    int nColorspace = 0;
    int nPixelfmt   = 0;      //TJPF_RGB;

    tjhandle handle = NULL;;
    handle = tjInitDecompress();
    if (NULL == handle)
    {
        return MV_CODEREADER_E_RESOURCE;
    }

    try
    {
        nRet = tjDecompressHeader3(handle, pstParam->pBufInput, pstParam->nBufInputLen, &nWidth, &nHeight, 
            &nSubsample, &nColorspace);
        if (nRet == -1)
        {
            nRet = MV_CODEREADER_E_PARAMETER;
            throw nRet;
        }

        if (TJSAMP_GRAY == nSubsample || TJSAMP_420 == nSubsample)
        {
            nPixelfmt = TJPF_GRAY;
        }

        nRet = tjDecompress2(handle, pstParam->pBufInput, pstParam->nBufInputLen, pstParam->pBufOutput, nWidth, 0,
            nHeight, nPixelfmt, 0);
        if (nRet == -1)
        {
            nRet = MV_CODEREADER_E_PARAMETER;
            throw nRet;
        }

        pstParam->nWidth = nWidth;
        pstParam->nHeight = nHeight;

        if (TJSAMP_GRAY == nSubsample || TJSAMP_420 == nSubsample)
        {
            pstParam->nBufOutputLen = pstParam->nWidth * pstParam->nHeight;
        }
        else
        {
            pstParam->nBufOutputLen = pstParam->nWidth * pstParam->nHeight * 3;
        }
    }
    catch (...)
    {
        if (handle)
        {
            tjDestroy(handle);
        }
        return nRet;
    }

    if (handle)
    {
        tjDestroy(handle);
    }

    return MV_CODEREADER_OK;
}

int CGrab_MSCDlg::GetCurrentConfigurationInformation()
{
    int nRet = MV_CODEREADER_OK;
    CString cstrInfo;

    // 获取触发模式
    MV_CODEREADER_ENUMVALUE stParam;
    unsigned int nTriggerMode = 0;
    memset(&stParam, 0, sizeof(MV_CODEREADER_ENUMVALUE));
    nRet = MV_CODEREADER_GetEnumValue(m_handle, TRIGGER_MODE, &stParam);
    if (MV_CODEREADER_OK != nRet)
    {
        cstrInfo.Format(_T("Get trigger mode failed! err code:%#x"), nRet);
        MessageBox(cstrInfo);
        return nRet;
    }
    nTriggerMode = stParam.nCurValue;

    if (MV_CODEREADER_TRIGGER_MODE_ON == nTriggerMode)
    {
        ((CButton *)GetDlgItem(IDC_CONTINUS_MODE_RADIO))->SetCheck(FALSE);
        ((CButton *)GetDlgItem(IDC_TRIGGER_MODE_RADIO))->SetCheck(TRUE);
        GetDlgItem(IDC_TRIGGER_MODE_RADIO)->EnableWindow(FALSE);
        GetDlgItem(IDC_CONTINUS_MODE_RADIO)->EnableWindow(TRUE);

    }
    else if (MV_CODEREADER_TRIGGER_MODE_OFF == nTriggerMode)
    {
        ((CButton *)GetDlgItem(IDC_CONTINUS_MODE_RADIO))->SetCheck(TRUE);
        ((CButton *)GetDlgItem(IDC_TRIGGER_MODE_RADIO))->SetCheck(FALSE);
        GetDlgItem(IDC_TRIGGER_MODE_RADIO)->EnableWindow(TRUE);
        GetDlgItem(IDC_CONTINUS_MODE_RADIO)->EnableWindow(FALSE);
    }
    else
    {
        return MV_CODEREADER_E_PARAMETER;
    }

    // 获取触发源,前置条件为已设为触发模式
    if(MV_CODEREADER_TRIGGER_MODE_OFF == nTriggerMode)
    {
        return nRet;
    }

    unsigned int nTriggerSource = 0;
    memset(&stParam, 0, sizeof(MV_CODEREADER_ENUMVALUE));
    nRet = MV_CODEREADER_GetEnumValue(m_handle, TRIGGER_SOURCE, &stParam);
    if (MV_CODEREADER_OK != nRet)
    {
        cstrInfo.Format(_T("Get trigger source failed! err code:%#x"), nRet);
        MessageBox(cstrInfo);
        return nRet;
    }
    nTriggerSource = stParam.nCurValue;

    if (MV_CODEREADER_TRIGGER_SOURCE_SOFTWARE == nTriggerSource)
    {
        ((CButton *)GetDlgItem(IDC_SOFTWARE_TRIGGER_CHECK))->SetCheck(TRUE);
    }
    else
    {
        ((CButton *)GetDlgItem(IDC_SOFTWARE_TRIGGER_CHECK))->SetCheck(FALSE);
    }

    return MV_CODEREADER_OK;
}

void CGrab_MSCDlg::OnBnClickedEnumButton()
{
    // TODO: Add your control notification handler code here
    // 清空设备列表框中的信息
    m_ctrlDeviceCombo.ResetContent();

    int nRet = MV_CODEREADER_OK;
    CString cstrInfo;

    // 查找设备
    memset(&m_stDeviceInfoList, 0, sizeof(MV_CODEREADER_DEVICE_INFO_LIST));
    nRet = MV_CODEREADER_EnumDevices( &m_stDeviceInfoList, MV_CODEREADER_GIGE_DEVICE);
    if (MV_CODEREADER_OK != nRet)
    {
        cstrInfo.Format(_T("Enum Device failed! err code:%#x"), nRet);
        MessageBox(cstrInfo);
        return;
    }
    
    // 枚举相机数量为0
    if (0 == m_stDeviceInfoList.nDeviceNum)
    {
        cstrInfo.Format(_T("None Device!"));
        MessageBox(cstrInfo);
        return;
    }
    
    // 显示查找到的设备信息
    for (unsigned int i = 0; i < m_stDeviceInfoList.nDeviceNum; i++)
    {
        unsigned char nIp1 = m_stDeviceInfoList.pDeviceInfo[i]->SpecialInfo.stGigEInfo.nCurrentIp & 0xff;
        unsigned char nIp2 = (m_stDeviceInfoList.pDeviceInfo[i]->SpecialInfo.stGigEInfo.nCurrentIp & 0xff00) >> 8;
        unsigned char nIp3 = (m_stDeviceInfoList.pDeviceInfo[i]->SpecialInfo.stGigEInfo.nCurrentIp & 0xff0000) >> 16;
        unsigned char nIp4 = (m_stDeviceInfoList.pDeviceInfo[i]->SpecialInfo.stGigEInfo.nCurrentIp & 0xff000000) >> 24;

        // 中文字体显示
        wchar_t strWchar[16] = {0};
        Char2Wchar((char*)m_stDeviceInfoList.pDeviceInfo[i]->SpecialInfo.stGigEInfo.chUserDefinedName, strWchar, 16);
        Wchar2char(strWchar, (char*)m_stDeviceInfoList.pDeviceInfo[i]->SpecialInfo.stGigEInfo.chUserDefinedName);

        cstrInfo.Format(_T("[%d] %s: %s (%d.%d.%d.%d)"), i, 
            CStringW(m_stDeviceInfoList.pDeviceInfo[i]->SpecialInfo.stGigEInfo.chUserDefinedName), 
            CStringW(m_stDeviceInfoList.pDeviceInfo[i]->SpecialInfo.stGigEInfo.chManufacturerName),
            nIp4, nIp3, nIp2, nIp1);
        m_ctrlDeviceCombo.AddString(cstrInfo);
    }
    
    if (m_stDeviceInfoList.nDeviceNum > 0)
    {
        m_ctrlDeviceCombo.SetCurSel(0);
    }
    
    GetDlgItem(IDC_OPEN_BUTTON)->EnableWindow(TRUE);
    UpdateData(FALSE);
}

void CGrab_MSCDlg::OnBnClickedOpenButton()
{
    // TODO: Add your control notification handler code here
    int nRet = MV_CODEREADER_OK;
    CString cstrInfo;

    UpdateData(TRUE);

    if (true ==  m_bConnect)
    {
        cstrInfo.Format(_T("The camera is already connect!"));
        MessageBox(cstrInfo);
        return ;
    }

    if (0 == m_stDeviceInfoList.nDeviceNum)
    {
        cstrInfo.Format(_T("Please discovery device first!"));
        MessageBox(cstrInfo);
        return ;
    }

    if (m_handle)
    {
        MV_CODEREADER_DestroyHandle(m_handle);
        m_handle = NULL;
    }

    // 获取当前选择的设备信息
    int nIndex = m_ctrlDeviceCombo.GetCurSel();
    memcpy(&m_stDeviceInfo, &m_stDeviceInfoList.pDeviceInfo[nIndex],sizeof(m_stDeviceInfoList.pDeviceInfo[nIndex]));

    // 创建设备句柄
    nRet = MV_CODEREADER_CreateHandle(&m_handle, m_stDeviceInfoList.pDeviceInfo[nIndex]);
    if (MV_CODEREADER_OK != nRet)
    {
        cstrInfo.Format(_T("Create handle failed! err code:%#x"), nRet);
        MessageBox(cstrInfo);
        return ;
    }

    // 打开设备
    nRet = MV_CODEREADER_OpenDevice(m_handle);
    if (MV_CODEREADER_OK != nRet)
    {
        cstrInfo.Format(_T("Open device failed! err code:%#x"), nRet);
        MessageBox(cstrInfo);
        MV_CODEREADER_DestroyHandle(m_handle);
        m_handle = NULL;
        return ;
    }

    MV_CODEREADER_SetWayBillEnable(m_handle, true);

    // 初始化必要的资源
    nRet = InitResources();
    if (MV_CODEREADER_OK != nRet)
    {
        cstrInfo.Format(_T("init resources failed! err code:%#x"), nRet);
        MessageBox(cstrInfo);
        return ;
    }

    nRet = MV_CODEREADER_SetEnumValue(m_handle, "RunningMode", 2);  
    if(MV_CODEREADER_OK != nRet)
    {
        cstrInfo.Format(_T("Set running mode failed! err code:%#x"), nRet);
        MessageBox(cstrInfo);
        return ;
    }

    // 获取运行模式以及触发模式
    nRet = GetCurrentConfigurationInformation();
    if (MV_CODEREADER_OK != nRet)
    {
        cstrInfo.Format(_T("Get param failed! err code:%#x"), nRet);
        MessageBox(cstrInfo);
        return ;
    }

    // 获取参数
    OnBnClickedGetParameterButton();
    m_bConnect = true;
    m_ctrlContinusModeRadio.EnableWindow(TRUE);
    m_ctrlTriggerModeRadio.EnableWindow(TRUE);
    GetDlgItem(IDC_OPEN_BUTTON)->EnableWindow(FALSE);
    GetDlgItem(IDC_CLOSE_BUTTON)->EnableWindow(TRUE);
    GetDlgItem(IDC_GET_PARAMETER_BUTTON)->EnableWindow(TRUE);
    GetDlgItem(IDC_SET_PARAMETER_BUTTON)->EnableWindow(TRUE);
    GetDlgItem(IDC_SAVE_JPG_BUTTON)->EnableWindow(FALSE);
    GetDlgItem(IDC_SAVE_BMP_BUTTON)->EnableWindow(FALSE);
    GetDlgItem(IDC_SAVE_RAW_BUTTON)->EnableWindow(FALSE);
    GetDlgItem(IDC_EXPOSURE_EDIT)->EnableWindow(TRUE);
    GetDlgItem(IDC_GAIN_EDIT)->EnableWindow(TRUE);
    GetDlgItem(IDC_FRAME_RATE_EDIT)->EnableWindow(TRUE);

    ((CButton *)GetDlgItem(IDC_SOFTWARE_TRIGGER_CHECK))->SetCheck(FALSE);
    GetDlgItem(IDC_START_GRABBING_BUTTON)->EnableWindow(TRUE);
    if(((CButton *)GetDlgItem(IDC_CONTINUS_MODE_RADIO))->GetCheck())
    {
        GetDlgItem(IDC_SOFTWARE_TRIGGER_CHECK)->EnableWindow(FALSE);
        GetDlgItem(IDC_CONTINUS_MODE_RADIO)->EnableWindow(FALSE);
        GetDlgItem(IDC_TRIGGER_MODE_RADIO)->EnableWindow(TRUE);
    }
    else
    {
        GetDlgItem(IDC_SOFTWARE_TRIGGER_CHECK)->EnableWindow(TRUE);
        GetDlgItem(IDC_CONTINUS_MODE_RADIO)->EnableWindow(TRUE);
        GetDlgItem(IDC_TRIGGER_MODE_RADIO)->EnableWindow(FALSE);
    }
}

void CGrab_MSCDlg::OnBnClickedCloseButton()
{
    // TODO: Add your control notification handler code here
    int nRet = MV_CODEREADER_OK;
    CString cstrInfo;

    // 销毁设备句柄 
    if (NULL != m_handle)
    {
        // 停止工作流程
        if (true ==  m_bStartJob)
        {
			m_bStartJob = false;
			DestoryThreadHandle();
            nRet = MV_CODEREADER_StopGrabbing(m_handle);
            if (MV_CODEREADER_OK != nRet)
            {
                cstrInfo.Format(_T("Stop grabbing failed! err code:%#x"), nRet);
                MessageBox(cstrInfo);
                m_bStartJob = false;
                return ;
            }

        }

        nRet = MV_CODEREADER_DestroyHandle(m_handle);
        if (MV_CODEREADER_OK != nRet)
        {
            cstrInfo.Format(_T("Destroy handle failed! err code:%#x"), nRet);
            MessageBox(cstrInfo);
            return ;
        }
        m_handle = NULL;
    }

    //销毁资源
    DeInitResources();
    DestoryThreadHandle();

    m_bStartJob = false;
    m_bConnect = false;

    // 关闭设备后清空各项参数数据
    ((CButton *)GetDlgItem(IDC_CONTINUS_MODE_RADIO))->SetCheck(FALSE);
    ((CButton *)GetDlgItem(IDC_TRIGGER_MODE_RADIO))->SetCheck(FALSE);
    ((CButton *)GetDlgItem(IDC_SOFTWARE_TRIGGER_CHECK))->SetCheck(FALSE);
    m_ctrlExposureEdit.SetWindowText(NULL);
    m_ctrlGainEdit.SetWindowText(NULL);
    m_ctrlFrameRateEdit.SetWindowText(NULL);

    GetDlgItem(IDC_OPEN_BUTTON)->EnableWindow(TRUE);
    GetDlgItem(IDC_CLOSE_BUTTON)->EnableWindow(FALSE);
    GetDlgItem(IDC_CONTINUS_MODE_RADIO)->EnableWindow(FALSE);
    GetDlgItem(IDC_TRIGGER_MODE_RADIO)->EnableWindow(FALSE);
    GetDlgItem(IDC_START_GRABBING_BUTTON)->EnableWindow(FALSE);
    GetDlgItem(IDC_STOP_GRABBING_BUTTON)->EnableWindow(FALSE);
    GetDlgItem(IDC_SOFTWARE_TRIGGER_CHECK)->EnableWindow(FALSE);
    GetDlgItem(IDC_SOFTWARE_ONCE_BUTTON)->EnableWindow(FALSE);
    GetDlgItem(IDC_SET_PARAMETER_BUTTON)->EnableWindow(FALSE);
    GetDlgItem(IDC_GET_PARAMETER_BUTTON)->EnableWindow(FALSE);
    GetDlgItem(IDC_SAVE_JPG_BUTTON)->EnableWindow(FALSE);
    GetDlgItem(IDC_SAVE_BMP_BUTTON)->EnableWindow(FALSE);
    GetDlgItem(IDC_SAVE_RAW_BUTTON)->EnableWindow(FALSE);
    GetDlgItem(IDC_EXPOSURE_EDIT)->EnableWindow(FALSE);
    GetDlgItem(IDC_GAIN_EDIT)->EnableWindow(FALSE);
    GetDlgItem(IDC_FRAME_RATE_EDIT)->EnableWindow(FALSE);
}

void CGrab_MSCDlg::OnBnClickedContinusModeRadio()
{
    // TODO: Add your control notification handler code here
    int nRet = MV_CODEREADER_OK;
    CString cstrInfo;

    // 设置触发模式
    nRet = MV_CODEREADER_SetEnumValue(m_handle, TRIGGER_MODE, MV_CODEREADER_TRIGGER_MODE_OFF);
    if (MV_CODEREADER_OK != nRet)
    {
        cstrInfo.Format(_T("Set Trigger off Mode failed! err code:%#x"), nRet);
        MessageBox(cstrInfo);
        ((CButton *)GetDlgItem(IDC_CONTINUS_MODE_RADIO))->SetCheck(FALSE);
        ((CButton *)GetDlgItem(IDC_TRIGGER_MODE_RADIO))->SetCheck(TRUE);
        return ;
    }

    ((CButton *)GetDlgItem(IDC_SOFTWARE_TRIGGER_CHECK))->SetCheck(FALSE);
    GetDlgItem(IDC_START_GRABBING_BUTTON)->EnableWindow(TRUE);
    GetDlgItem(IDC_SOFTWARE_TRIGGER_CHECK)->EnableWindow(FALSE);
    GetDlgItem(IDC_TRIGGER_MODE_RADIO)->EnableWindow(TRUE);
    ((CButton *)GetDlgItem(IDC_TRIGGER_MODE_RADIO))->SetCheck(FALSE);
    GetDlgItem(IDC_CONTINUS_MODE_RADIO)->EnableWindow(FALSE);
    ((CButton *)GetDlgItem(IDC_CONTINUS_MODE_RADIO))->SetCheck(TRUE);

    UpdateData(FALSE);
}

void CGrab_MSCDlg::OnBnClickedTriggerModeRadio()
{
    UpdateData(TRUE);

    // TODO: Add your control notification handler code here
    int nRet = MV_CODEREADER_OK;
    CString cstrInfo;

    // 设置触发模式
    nRet = MV_CODEREADER_SetEnumValue(m_handle, TRIGGER_MODE, MV_CODEREADER_TRIGGER_MODE_ON);
    if (MV_CODEREADER_OK != nRet)
    {
        cstrInfo.Format(_T("Set Trigger on Mode failed! err code:%#x"), nRet);
        MessageBox(cstrInfo);
        ((CButton *)GetDlgItem(IDC_CONTINUS_MODE_RADIO))->SetCheck(TRUE);
        ((CButton *)GetDlgItem(IDC_TRIGGER_MODE_RADIO))->SetCheck(FALSE);
        return ;
    }

    if (m_bIsSoftTrigger)
    {
        GetDlgItem(IDC_SOFTWARE_ONCE_BUTTON)->EnableWindow(FALSE);
    }
    else
    {
        ((CButton *)GetDlgItem(IDC_SOFTWARE_TRIGGER_CHECK))->SetCheck(FALSE);
        GetDlgItem(IDC_SOFTWARE_ONCE_BUTTON)->EnableWindow(FALSE);
    }
    GetDlgItem(IDC_SOFTWARE_TRIGGER_CHECK)->EnableWindow(TRUE);
    GetDlgItem(IDC_CONTINUS_MODE_RADIO)->EnableWindow(TRUE);
    ((CButton *)GetDlgItem(IDC_CONTINUS_MODE_RADIO))->SetCheck(FALSE);
    GetDlgItem(IDC_TRIGGER_MODE_RADIO)->EnableWindow(FALSE);
    ((CButton *)GetDlgItem(IDC_TRIGGER_MODE_RADIO))->SetCheck(TRUE);

    UpdateData(FALSE);
}

void CGrab_MSCDlg::OnBnClickedSoftwareTriggerCheck()
{
    UpdateData(TRUE);

    // TODO: Add your control notification handler code here

    int nRet = MV_CODEREADER_OK;
    CString cstrInfo;

    //  设置软触发模式
    if (m_ctrlSoftwareTriggerCheck.GetCheck())
    {
        nRet = MV_CODEREADER_SetEnumValue(m_handle, TRIGGER_SOURCE, MV_CODEREADER_TRIGGER_SOURCE_SOFTWARE);   // 选择软触发
        if (MV_CODEREADER_OK != nRet)
        {
            cstrInfo.Format(_T("Set Software Mode fialed! err code:%#x"), nRet);
            MessageBox(cstrInfo);
            ((CButton *)GetDlgItem(IDC_SOFTWARE_TRIGGER_CHECK))->SetCheck(FALSE);
            return ;
        }
        else
        {
            m_bIsSoftTrigger = true;
            ((CButton *)GetDlgItem(IDC_SOFTWARE_TRIGGER_CHECK))->SetCheck(TRUE);
            GetDlgItem(IDC_TRIGGER_MODE_RADIO)->EnableWindow(FALSE);

        }
    }
    else
    {
        nRet = MV_CODEREADER_SetEnumValue(m_handle, TRIGGER_SOURCE, MV_CODEREADER_TRIGGER_SOURCE_LINE0);
        if (MV_CODEREADER_OK != nRet)
        {
            cstrInfo.Format(_T("Set Line0 Mode failed! err code:%#x"), nRet);
            MessageBox(cstrInfo);
            ((CButton *)GetDlgItem(IDC_SOFTWARE_TRIGGER_CHECK))->SetCheck(FALSE);
            return ;
        }
        else
        {
            m_bIsSoftTrigger = false;
            ((CButton *)GetDlgItem(IDC_SOFTWARE_TRIGGER_CHECK))->SetCheck(FALSE);
            GetDlgItem(IDC_SOFTWARE_ONCE_BUTTON)->EnableWindow(FALSE);
        }
    }

    UpdateData(FALSE);

}

void CGrab_MSCDlg::OnBnClickedSoftwareOnceButton()
{
    // TODO: Add your control notification handler code here
    int nRet = MV_CODEREADER_OK;
    CString cstrInfo;

    // 软触发
    nRet = MV_CODEREADER_SetCommandValue(m_handle, TRIGGER_SOFTWARE);
    if (MV_CODEREADER_OK != nRet)
    {
        cstrInfo.Format(_T("Set Software Once failed! err code:%#x"), nRet);
        MessageBox(cstrInfo);
        return ;
    }

}

void CGrab_MSCDlg::OnBnClickedStartGrabbingButton()
{
    // TODO: Add your control notification handler code here
    int nRet = MV_CODEREADER_OK;
    CString cstrInfo;
    m_bStartJob = true;

    if (m_handle)
    {
        try
        {
            // 开始工作流程
            nRet = MV_CODEREADER_StartGrabbing(m_handle);
            if (MV_CODEREADER_OK != nRet)
            {
                cstrInfo.Format(_T("Start grabbing failed! err code:%#x"), nRet);
                MessageBox(cstrInfo);
                throw nRet;
            }

            // 获取运行模式
            MV_CODEREADER_ENUMVALUE stParam;
            memset(&stParam, 0, sizeof(MV_CODEREADER_ENUMVALUE));
            nRet = MV_CODEREADER_GetEnumValue(m_handle, RUNNING_MODE, &stParam);
            if (MV_CODEREADER_OK != nRet)
            {
                cstrInfo.Format(_T("Get Running mode failed! err code:%#x"), nRet);
                MessageBox(cstrInfo);
                throw nRet;
            }
            m_nRunMode = stParam.nCurValue;

            if (m_bMSCCamera && MSCRawRunMode == m_nRunMode)
            {
                // 获取多通道RAW模式下出图流通道
                MV_CODEREADER_ENUMVALUE stParam;
                memset(&stParam, 0, sizeof(MV_CODEREADER_ENUMVALUE));
                nRet = MV_CODEREADER_GetEnumValue(m_handle, OUT_CHANNEL, &stParam);
                if (MV_CODEREADER_OK != nRet)
                {
                    cstrInfo.Format(_T("Get Raw out channel selector failed! err code:%#x"), nRet);
                    MessageBox(cstrInfo);
                    throw nRet;
                }
                m_nOutRawChannel = stParam.nCurValue;
            }

            // 创建取流线程
            m_hChannel0Thread = CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)Channel0ProcessThread, this, 0, NULL);
            if (NULL == m_hChannel0Thread)
            {
                cstrInfo.Format(_T("Create channel0 proccess Thread failed!"));
                MessageBox(cstrInfo);
                throw MV_CODEREADER_E_RESOURCE;
            }

            m_ctrlContinusModeRadio.EnableWindow(FALSE);
            m_ctrlTriggerModeRadio.EnableWindow(FALSE);

            if (m_bMSCCamera)
            {
                m_hChannel1Thread = CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)Channel1ProcessThread, this, 0, NULL);
                if (NULL == m_hChannel1Thread)
                {
                    cstrInfo.Format(_T("Create channel1 proccess Thread failed!"));
                    MessageBox(cstrInfo);
                    throw MV_CODEREADER_E_RESOURCE;
                }

                GetDlgItem(IDC_SAVE_JPG_BUTTON)->EnableWindow(TRUE);

                if (MSCRawRunMode == m_nRunMode)
                {
                    GetDlgItem(IDC_SAVE_BMP_BUTTON)->EnableWindow(TRUE);
                    GetDlgItem(IDC_SAVE_RAW_BUTTON)->EnableWindow(TRUE);
                }
            }

            GetDlgItem(IDC_START_GRABBING_BUTTON)->EnableWindow(FALSE);
            GetDlgItem(IDC_STOP_GRABBING_BUTTON)->EnableWindow(TRUE);
            GetDlgItem(IDC_SOFTWARE_TRIGGER_CHECK)->EnableWindow(FALSE);

            if(((CButton *)GetDlgItem(IDC_SOFTWARE_TRIGGER_CHECK))->GetCheck())
            {
                GetDlgItem(IDC_SOFTWARE_ONCE_BUTTON)->EnableWindow(TRUE);
            }


            if (NormalRunMode == m_nRunMode || MSCTestRunMode == m_nRunMode)
            {
                GetDlgItem(IDC_SAVE_JPG_BUTTON)->EnableWindow(TRUE);
            }
            else
            {
                GetDlgItem(IDC_SAVE_JPG_BUTTON)->EnableWindow(TRUE);
                GetDlgItem(IDC_SAVE_BMP_BUTTON)->EnableWindow(TRUE);
                GetDlgItem(IDC_SAVE_RAW_BUTTON)->EnableWindow(TRUE);
            }
        }
        catch (...)
        {
            DestoryThreadHandle();
            return;
        }
    }

}

void CGrab_MSCDlg::OnBnClickedStopGrabbingButton()
{
    // TODO: Add your control notification handler code here
    int nRet = MV_CODEREADER_OK;
    CString cstrInfo;

    // 停止工作流程
    nRet = MV_CODEREADER_StopGrabbing(m_handle);
    if (MV_CODEREADER_OK != nRet)
    {
        cstrInfo.Format(_T("Stop grabbing failed! err code:%#x"), nRet);
        MessageBox(cstrInfo);
        m_bStartJob = false;
        return ;
    }

    m_bStartJob = false;
    DestoryThreadHandle();

    GetDlgItem(IDC_STOP_GRABBING_BUTTON)->EnableWindow(FALSE);
    GetDlgItem(IDC_SOFTWARE_ONCE_BUTTON)->EnableWindow(FALSE);
    GetDlgItem(IDC_SAVE_JPG_BUTTON)->EnableWindow(FALSE);
    GetDlgItem(IDC_SAVE_BMP_BUTTON)->EnableWindow(FALSE);
    GetDlgItem(IDC_SAVE_RAW_BUTTON)->EnableWindow(FALSE);
    GetDlgItem(IDC_START_GRABBING_BUTTON)->EnableWindow(TRUE);

    if(((CButton *)GetDlgItem(IDC_CONTINUS_MODE_RADIO))->GetCheck())
    {
        GetDlgItem(IDC_SOFTWARE_TRIGGER_CHECK)->EnableWindow(FALSE);
        ((CButton *)GetDlgItem(IDC_SOFTWARE_TRIGGER_CHECK))->SetCheck(FALSE);
        GetDlgItem(IDC_CONTINUS_MODE_RADIO)->EnableWindow(FALSE);
        ((CButton *)GetDlgItem(IDC_CONTINUS_MODE_RADIO))->SetCheck(TRUE);
        GetDlgItem(IDC_TRIGGER_MODE_RADIO)->EnableWindow(TRUE);
        ((CButton *)GetDlgItem(IDC_TRIGGER_MODE_RADIO))->SetCheck(FALSE);
    }
    else
    {
        GetDlgItem(IDC_SOFTWARE_TRIGGER_CHECK)->EnableWindow(TRUE);
        GetDlgItem(IDC_CONTINUS_MODE_RADIO)->EnableWindow(TRUE);
        ((CButton *)GetDlgItem(IDC_CONTINUS_MODE_RADIO))->SetCheck(FALSE);
        GetDlgItem(IDC_TRIGGER_MODE_RADIO)->EnableWindow(FALSE);
        ((CButton *)GetDlgItem(IDC_TRIGGER_MODE_RADIO))->SetCheck(TRUE);
    }

}

char* CGrab_MSCDlg::GetCurrentProGramPath(char* pFilePath, int nSize)
{
    if (NULL == pFilePath || MAX_PATH > nSize )
    {
        return pFilePath;
    }

    wchar_t chModuleFileName[MAX_PATH] = {0};
    char chPath[MAX_PATH] = {0};
    GetModuleFileName(NULL, chModuleFileName, MAX_PATH);
    Wchar2char(chModuleFileName, chPath);
    char *pFile = strrchr(chPath, '\\');
    if (pFile)
    {
        strncpy_s(pFilePath, nSize, chPath, (pFile - chPath));
    }
    else
    {
        strncpy_s(pFilePath, nSize, chPath, nSize - 1);
    }
    memset(chPath, 0, MAX_PATH);
    sprintf(chPath, "%s\\IMAGE", pFilePath);
    memset(chModuleFileName, 0, MAX_PATH);
    Char2Wchar(chPath, chModuleFileName, MAX_PATH);
    CreateDirectory(chModuleFileName, NULL);
    memset(pFilePath, 0, sizeof(char) * nSize);
    strncpy_s(pFilePath, nSize, chPath, nSize - 1);
    return pFilePath;
}

void CGrab_MSCDlg::OnBnClickedSaveBmpButton()
{
    // TODO: Add your control notification handler code here
    int nRet = MV_CODEREADER_OK;
    CString cstrInfo;

    // 判断是否开始取流
    if (!m_bConnect)
    {
        cstrInfo.Format(_T("No camera Connected! "));
        MessageBox(cstrInfo);
        return;
    }

    if (!m_bStartJob)
    {
        cstrInfo.Format(_T("The camera is not startJob!"));
        MessageBox(cstrInfo);
        return;
    }

    // 判断多通道RAW运行模式是否有效数据
    if (m_bMSCCamera && MSCRawRunMode == m_nRunMode)
    {
        if (NULL == m_pcDataBuf[m_nOutRawChannel])
        {
            cstrInfo.Format(_T("No data, Save BMP failed!"));
            MessageBox(cstrInfo);
            return;
        }

        // 判断是否可保存Bmp图
        if (PixelType_CodeReader_Gvsp_Mono8 != m_pstImgInfoEx2[m_nOutRawChannel]->enPixelType)
        {
            cstrInfo.Format(_T("Unable to save BMP image!"));
            MessageBox(cstrInfo);
            return;
        }

        FILE* pfile = NULL;
        char filename[256] = {0};

        MV_CODEREADER_SAVE_IMAGE_PARAM_EX* pstParam = new MV_CODEREADER_SAVE_IMAGE_PARAM_EX;
        memset(pstParam, 0, sizeof(MV_CODEREADER_SAVE_IMAGE_PARAM_EX));

        pstParam->pData = m_pcDataBuf[m_nOutRawChannel];
        pstParam->nDataLen = m_pstImgInfoEx2[m_nOutRawChannel]->nFrameLen;
        pstParam->nWidth = m_pstImgInfoEx2[m_nOutRawChannel]->nWidth;
        pstParam->nHeight = m_pstImgInfoEx2[m_nOutRawChannel]->nHeight;
        pstParam->enPixelType = m_pstImgInfoEx2[m_nOutRawChannel]->enPixelType;
        pstParam->nBufferSize = m_MaxImageSize[m_nOutRawChannel];
        pstParam->nImageLen = 0;
        pstParam->enImageType = MV_CODEREADER_Image_Bmp;
        pstParam->nJpgQuality = 60;
        nRet = MV_CODEREADER_SaveImage(m_handle, pstParam);
        if (MV_CODEREADER_OK == nRet)
        {
            CTime currTime;                                     // 获取系统时间作为保存图片文件名
            currTime = CTime::GetCurrentTime();
            char chCurDir[MAX_PATH] = {0};
            GetCurrentProGramPath(chCurDir, MAX_PATH);
            sprintf(filename,("%s\\%.4d%.2d%.2d%.2d%.2d%.2d.bmp"), chCurDir, currTime.GetYear(), currTime.GetMonth(),
                currTime.GetDay(), currTime.GetHour(), currTime.GetMinute(), currTime.GetSecond());
            pfile = fopen(filename,"wb");
            if(pfile == NULL)
            {
                cstrInfo.Format(_T("Open dispaly[%d] file failed!"), m_nOutRawChannel);
                MessageBox(cstrInfo);
                return ;
            }

            fwrite(pstParam->pImageBuffer, 1, pstParam->nImageLen, pfile);
            cstrInfo.Format(_T("Save Channel[%d] BMP image success!"), m_nOutRawChannel);
            MessageBox(cstrInfo);
        }
        else
        {
            cstrInfo.Format(_T("Save BMP image failed! err code:%#x"), nRet);
            MessageBox(cstrInfo);
        }

        if (NULL != pstParam)
        {
            delete pstParam;
            pstParam = NULL;
        }

        if (NULL != pfile)
        {
            fclose (pfile);
            pfile = NULL;
        }
    }
    else
    {
        for (int i = 0; i < m_nChannelNum; i++)
        {
            if (NULL == m_pcDataBuf[i])
            {
                cstrInfo.Format(_T("No data, Save BMP failed!"));
                MessageBox(cstrInfo);
            }

            // 判断是否可保存Bmp图
            if (PixelType_CodeReader_Gvsp_Mono8 != m_pstImgInfoEx2[i]->enPixelType)
            {
                cstrInfo.Format(_T("Unable to save BMP image!"));
                MessageBox(cstrInfo);
                return;
            }

            FILE* pfile = NULL;
            char filename[256] = {0};

            MV_CODEREADER_SAVE_IMAGE_PARAM_EX* pstParam = new MV_CODEREADER_SAVE_IMAGE_PARAM_EX;
            memset(pstParam, 0, sizeof(MV_CODEREADER_SAVE_IMAGE_PARAM_EX));

            pstParam->pData = m_pcDataBuf[i];
            pstParam->nDataLen = m_pstImgInfoEx2[i]->nFrameLen;
            pstParam->nWidth = m_pstImgInfoEx2[i]->nWidth;
            pstParam->nHeight = m_pstImgInfoEx2[i]->nHeight;
            pstParam->enPixelType = m_pstImgInfoEx2[i]->enPixelType;
            pstParam->nBufferSize = m_MaxImageSize[i];
            pstParam->nImageLen = 0;
            pstParam->enImageType = MV_CODEREADER_Image_Bmp;
            pstParam->nJpgQuality = 60;
            nRet = MV_CODEREADER_SaveImage(m_handle, pstParam);
            if (MV_CODEREADER_OK == nRet)
            {
                CTime currTime;                                     // 获取系统时间作为保存图片文件名
                currTime = CTime::GetCurrentTime();
                char chCurDir[MAX_PATH] = {0};
                GetCurrentProGramPath(chCurDir, MAX_PATH);
                sprintf(filename,("%s\\%.4d%.2d%.2d%.2d%.2d%.2d.bmp"), chCurDir, currTime.GetYear(), currTime.GetMonth(),
                    currTime.GetDay(), currTime.GetHour(), currTime.GetMinute(), currTime.GetSecond());
                pfile = fopen(filename,"wb");
                if(pfile == NULL)
                {
                    cstrInfo.Format(_T("Open file failed!"));
                    MessageBox(cstrInfo);
                    return ;
                }

                fwrite(pstParam->pImageBuffer, 1, pstParam->nImageLen, pfile);
                cstrInfo.Format(_T("Save Channel[%d] BMP image success!"), m_nOutRawChannel);
                MessageBox(cstrInfo);
            }
            else
            {
                cstrInfo.Format(_T("Save BMP image failed! err code:%#x"), nRet);
                MessageBox(cstrInfo);
            }

            if (NULL != pstParam)
            {
                delete pstParam;
                pstParam = NULL;
            }

            if (NULL != pfile)
            {
                fclose (pfile);
                pfile = NULL;
            }
        }
    }
}

void CGrab_MSCDlg::OnBnClickedSaveJpgButton()
{
    // TODO: Add your control notification handler code here
    int nRet = MV_CODEREADER_OK;
    CString cstrInfo;

    // 判断是否开始取流
    if (!m_bConnect)
    {
        cstrInfo.Format(_T("No camera Connected! "));
        MessageBox(cstrInfo);
        return;
    }

    if (!m_bStartJob)
    {
        cstrInfo.Format(_T("The camera is not startJob!"));
        MessageBox(cstrInfo);
        return;
    }

    if (m_bMSCCamera && MSCRawRunMode == m_nRunMode)
    {
        // 判断是否是有效数据
        if (NULL == m_pcDataBuf[m_nOutRawChannel])
        {
            cstrInfo.Format(_T("No valid image data，Save dispaly[%d] JPG image failed!"), m_nOutRawChannel);
            MessageBox(cstrInfo);
            return;
        }

        // 保存文件
        FILE* pfile = NULL;
        char filename[256] = {0};

        // 判断PixelType格式存图, 若Jpeg格式直接存图, Mono8格式转换存图
        if (PixelType_CodeReader_Gvsp_Jpeg == m_pstImgInfoEx2[m_nOutRawChannel]->enPixelType)
        {
            m_criSection.Lock();
            CTime currTime;                                     // 获取系统时间作为保存图片文件名
            currTime = CTime::GetCurrentTime(); 
            char chCurDir[MAX_PATH] = {0};
            GetCurrentProGramPath(chCurDir, MAX_PATH);
            sprintf(filename,("%s\\%.4d%.2d%.2d%.2d%.2d%.2d.jpg"), chCurDir, currTime.GetYear(), currTime.GetMonth(),
                currTime.GetDay(), currTime.GetHour(), currTime.GetMinute(), currTime.GetSecond());
            pfile = fopen(filename,"wb");
            if(pfile == NULL)
            {
                cstrInfo.Format(_T("Open file failed"));
                MessageBox(cstrInfo);
                return ;
            }

            fwrite(m_pcDataBuf[m_nOutRawChannel], 1, m_pstImgInfoEx2[m_nOutRawChannel]->nFrameLen, pfile);
            m_criSection.Unlock();
            cstrInfo.Format(_T("Save Channel[%d] JPG image success!"), m_nOutRawChannel);
            MessageBox(cstrInfo);
        }
        else
        {
            // 获取图像转换信息
            MV_CODEREADER_SAVE_IMAGE_PARAM_EX* pstParam = new MV_CODEREADER_SAVE_IMAGE_PARAM_EX;
            memset(pstParam, 0, sizeof(MV_CODEREADER_SAVE_IMAGE_PARAM_EX));
            pstParam->pData = m_pcDataBuf[m_nOutRawChannel];
            pstParam->nDataLen = m_pstImgInfoEx2[m_nOutRawChannel]->nFrameLen;
            pstParam->nWidth = m_pstImgInfoEx2[m_nOutRawChannel]->nWidth;
            pstParam->nHeight = m_pstImgInfoEx2[m_nOutRawChannel]->nHeight;
            pstParam->enPixelType = m_pstImgInfoEx2[m_nOutRawChannel]->enPixelType;//PixelType_CodeReader_Gvsp_Mono8;
            pstParam->nBufferSize = m_MaxImageSize[m_nOutRawChannel];
            pstParam->nImageLen = 0;
            pstParam->enImageType = MV_CODEREADER_Image_Jpeg;
            pstParam->nJpgQuality = 60;

            // 保存JPG图像
            nRet = MV_CODEREADER_SaveImage(m_handle, pstParam);
            if (MV_CODEREADER_OK == nRet)
            {
                CTime currTime;                                     // 获取系统时间作为保存图片文件名
                currTime = CTime::GetCurrentTime();
                char chCurDir[MAX_PATH] = {0};
                GetCurrentProGramPath(chCurDir, MAX_PATH);
                sprintf(filename,("%s\\%.4d%.2d%.2d%.2d%.2d%.2d.jpg"), chCurDir, currTime.GetYear(), currTime.GetMonth(),
                    currTime.GetDay(), currTime.GetHour(), currTime.GetMinute(), currTime.GetSecond());
                pfile = fopen(filename,"wb");
                if(pfile == NULL)
                {
                    cstrInfo.Format(_T("Open file failed"));
                    MessageBox(cstrInfo);
                    return ;
                }

                fwrite(pstParam->pImageBuffer, 1, pstParam->nImageLen, pfile);
                cstrInfo.Format(_T("Save Channel[%d] JPG image success!"), m_nOutRawChannel);
                MessageBox(cstrInfo);
            }
            else
            {
                cstrInfo.Format(_T("Save Channel[%d] JPG image failed! err code:%#x"), m_nOutRawChannel, nRet);
                MessageBox(cstrInfo);
            }

            if (NULL != pstParam)
            {
                delete pstParam;
                pstParam = NULL;
            }
        }

        if (NULL != pfile)
        {
            fclose (pfile);
            pfile = NULL;
        }

    }
    else
    {
        for (int i = 0; i < m_nChannelNum; i++)
        {
            // 判断是否是有效数据
            if (NULL == m_pcDataBuf[i])
            {
                cstrInfo.Format(_T("No valid image data，Save dispaly[%d] JPG image failed!"), i);
                MessageBox(cstrInfo);
            }

            // 保存文件
            FILE* pfile = NULL;
            char filename[256] = {0};

            // 判断PixelType格式存图, 若Jpeg格式直接存图, Mono8格式转换存图
            if (PixelType_CodeReader_Gvsp_Jpeg == m_pstImgInfoEx2[i]->enPixelType)
            {
                m_criSection.Lock();
                CTime currTime;                                     // 获取系统时间作为保存图片文件名
                currTime = CTime::GetCurrentTime();
                char chCurDir[MAX_PATH] = {0};
                GetCurrentProGramPath(chCurDir, MAX_PATH);
                sprintf(filename,("%s\\Channel%d%.4d%.2d%.2d%.2d%.2d%.2d.jpg"), chCurDir, m_pstImgInfoEx2[i]->nChannelID, currTime.GetYear(), currTime.GetMonth(),
                    currTime.GetDay(), currTime.GetHour(), currTime.GetMinute(), currTime.GetSecond());
                pfile = fopen(filename,"wb");
                if(pfile == NULL)
                {
                    cstrInfo.Format(_T("Open file failed"));
                    MessageBox(cstrInfo);
                    return ;
                }

                fwrite(m_pcDataBuf[i], 1, m_pstImgInfoEx2[i]->nFrameLen, pfile);
                m_criSection.Unlock();
                cstrInfo.Format(_T("Save dispaly[%d] JPG image success!"), i);
                MessageBox(cstrInfo);
            }
            else
            {
                // 获取图像转换信息
                MV_CODEREADER_SAVE_IMAGE_PARAM_EX* pstParam = new MV_CODEREADER_SAVE_IMAGE_PARAM_EX;
                memset(pstParam, 0, sizeof(MV_CODEREADER_SAVE_IMAGE_PARAM_EX));
                pstParam->pData = m_pcDataBuf[i];
                pstParam->nDataLen = m_pstImgInfoEx2[i]->nFrameLen;
                pstParam->nWidth = m_pstImgInfoEx2[i]->nWidth;
                pstParam->nHeight = m_pstImgInfoEx2[i]->nHeight;
                pstParam->enPixelType = m_pstImgInfoEx2[i]->enPixelType;//PixelType_CodeReader_Gvsp_Mono8;
                pstParam->nBufferSize = m_MaxImageSize[i];
                pstParam->nImageLen = 0;
                pstParam->enImageType = MV_CODEREADER_Image_Jpeg;
                pstParam->nJpgQuality = 60;

                // 保存JPG图像
                nRet = MV_CODEREADER_SaveImage(m_handle, pstParam);
                if (MV_CODEREADER_OK == nRet)
                {
                    CTime currTime;                                     // 获取系统时间作为保存图片文件名
                    currTime = CTime::GetCurrentTime();
                    char chCurDir[MAX_PATH] = {0};
                    GetCurrentProGramPath(chCurDir, MAX_PATH);
                    sprintf(filename,("%s\\Channel%d%.4d%.2d%.2d%.2d%.2d%.2d.jpg"), chCurDir, m_pstImgInfoEx2[i]->nChannelID, currTime.GetYear(), currTime.GetMonth(),
                        currTime.GetDay(), currTime.GetHour(), currTime.GetMinute(), currTime.GetSecond());
                    pfile = fopen(filename,"wb");
                    if(pfile == NULL)
                    {
                        cstrInfo.Format(_T("Open file failed"));
                        MessageBox(cstrInfo);
                        return ;
                    }

                    fwrite(pstParam->pImageBuffer, 1, pstParam->nImageLen, pfile);
                    cstrInfo.Format(_T("Save dispaly[%d] JPG image success!"), i);
                    MessageBox(cstrInfo);
                }
                else
                {
                    cstrInfo.Format(_T("Save dispaly[%d] JPG image failed! err code:%#x"), i, nRet);
                    MessageBox(cstrInfo);
                }

                if (NULL != pstParam)
                {
                    delete pstParam;
                    pstParam = NULL;
                }
            }

            if (NULL != pfile)
            {
                fclose (pfile);
                pfile = NULL;
            }
        }
    }

}

void CGrab_MSCDlg::OnBnClickedSaveRawButton()
{
    // TODO: Add your control notification handler code here
    int nRet = MV_CODEREADER_OK;
    CString cstrInfo;

    // 判断是否开始取流
    if (!m_bConnect)
    {
        cstrInfo.Format(_T("No camera Connected! "));
        MessageBox(cstrInfo);
        return;
    }

    if (!m_bStartJob)
    {
        cstrInfo.Format(_T("The camera is not startJob!"));
        MessageBox(cstrInfo);
        return;
    }

    if (m_bMSCCamera && MSCRawRunMode == m_nRunMode)
    {
        // 判断是否有有效数据
        if (NULL == m_pcDataBuf[m_nOutRawChannel])
        {
            cstrInfo.Format(_T("No valid image data，Save Channel[%d] RAW failed!"), m_nOutRawChannel);
            MessageBox(cstrInfo);
            return;
        }
        // 判断是否可保存Raw图
        if (PixelType_CodeReader_Gvsp_Mono8 != m_pstImgInfoEx2[m_nOutRawChannel]->enPixelType)
        {
            cstrInfo.Format(_T("Unable to save channel[%d] RAW image!"), m_nOutRawChannel);
            MessageBox(cstrInfo);
            return;
        }

        // 保存RAW图像
        FILE* pfile = NULL;
        char filename[256] = {0};
        CTime currTime;                                     // 获取系统时间作为保存图片文件名
        currTime = CTime::GetCurrentTime(); 
        char chCurDir[MAX_PATH] = {0};
        GetCurrentProGramPath(chCurDir, MAX_PATH);
		sprintf(filename,("%s\\%.4d%.2d%.2d%.2d%.2d%.2d.raw"), chCurDir, currTime.GetYear(), currTime.GetMonth(),
			currTime.GetDay(), currTime.GetHour(), currTime.GetMinute(), currTime.GetSecond());
        pfile = fopen(filename,"wb");
        if(pfile == NULL)
        {
            cstrInfo.Format(_T("Open file failed!"));
            MessageBox(cstrInfo);
            return ;
        }

        m_criSection.Lock();
        fwrite(m_pcDataBuf[m_nOutRawChannel], 1, m_pstImgInfoEx2[m_nOutRawChannel]->nFrameLen, pfile);
        m_criSection.Unlock();
        cstrInfo.Format(_T("Save Channle[%d] RAW image success!"), m_nOutRawChannel);
        MessageBox(cstrInfo);

        if (NULL != pfile)
        {
            fclose (pfile);
            pfile = NULL;
        }
    }

    else
    {
        for (int i = 0; i < m_nChannelNum; i ++)
        {
            // 判断是否有有效数据
            if (NULL == m_pcDataBuf[i])
            {
                cstrInfo.Format(_T("No valid image data，Save Channel[%d] RAW failed!"), i);
                MessageBox(cstrInfo);
                return;
            }

            // 判断是否可保存Raw图
            if (PixelType_CodeReader_Gvsp_Mono8 != m_pstImgInfoEx2[i]->enPixelType)
            {
                cstrInfo.Format(_T("Unable to save channel[%d] RAW image!"), i);
                MessageBox(cstrInfo);
                return;
            }

            // 保存RAW图像
            FILE* pfile = NULL;
            char filename[256] = {0};
            CTime currTime;                                     // 获取系统时间作为保存图片文件名
            currTime = CTime::GetCurrentTime(); 
            char chCurDir[MAX_PATH] = {0};
            GetCurrentProGramPath(chCurDir, MAX_PATH);
			sprintf(filename,("%s\\%.4d%.2d%.2d%.2d%.2d%.2d.raw"), chCurDir, currTime.GetYear(), currTime.GetMonth(),
				currTime.GetDay(), currTime.GetHour(), currTime.GetMinute(), currTime.GetSecond());
            pfile = fopen(filename,"wb");
            if(pfile == NULL)
            {
                cstrInfo.Format(_T("Open file failed!"));
                MessageBox(cstrInfo);
                return ;
            }
            m_criSection.Lock();
            fwrite(m_pcDataBuf[i], 1, m_pstImgInfoEx2[i]->nFrameLen, pfile);
            m_criSection.Unlock();
            cstrInfo.Format(_T("Save Channle[%d] RAW image success!"), i);
            MessageBox(cstrInfo);

            if (NULL != pfile)
            {
                fclose (pfile);
                pfile = NULL;
            }
        }
    }

}

void CGrab_MSCDlg::OnBnClickedGetParameterButton()
{
    // TODO: Add your control notification handler code here
    int nRet = MV_CODEREADER_OK;
    CString cstrInfo;

    // 获取曝光时间
    float fExposureTime = 0.0f;
    MV_CODEREADER_FLOATVALUE stParam;
    memset(&stParam, 0, sizeof(MV_CODEREADER_FLOATVALUE));
    nRet = MV_CODEREADER_GetFloatValue(m_handle, EXPOSURE_TIME, &stParam);
    if (MV_CODEREADER_OK != nRet)
    {
        cstrInfo.Format(_T("Set exposure time failed! err code:%#x"), nRet);
        MessageBox(cstrInfo);
    }
    else
    {
        fExposureTime = stParam.fCurValue;
        cstrInfo.Format(_T("%0.2f"), fExposureTime);
        m_ctrlExposureEdit.SetWindowText(cstrInfo);
    }

    // 获取增益
    float fGain= 0.0f;
    memset(&stParam, 0, sizeof(MV_CODEREADER_FLOATVALUE));
    nRet = MV_CODEREADER_GetFloatValue(m_handle, GAIN, &stParam);
    if (MV_CODEREADER_OK != nRet)
    {
        cstrInfo.Format(_T("Get gain failed! err code:%#x"), nRet);
        MessageBox(cstrInfo);
    }
    else
    {
        fGain = stParam.fCurValue;
        cstrInfo.Format(_T("%0.2f"), fGain);
        m_ctrlGainEdit.SetWindowText(cstrInfo);
    }

    // 获取帧率
    float fFrameRate= 0.0f;
    memset(&stParam, 0, sizeof(MV_CODEREADER_FLOATVALUE));
    nRet = MV_CODEREADER_GetFloatValue(m_handle, ACQUISITION_FRAME_RATE, &stParam);
    if (MV_CODEREADER_OK != nRet)
    {
        cstrInfo.Format(_T("Get acquisition rate failed! err code:%#x"), nRet);
        MessageBox(cstrInfo);
    }
    else
    {
        fFrameRate = stParam.fCurValue;
        cstrInfo.Format(_T("%0.2f"), fFrameRate);
        m_ctrlFrameRateEdit.SetWindowText(cstrInfo);
    }

    UpdateData(FALSE);
}

void CGrab_MSCDlg::OnClose()
{
    // TODO: Add your message handler code here and/or call default
    // 关闭程序，执行断开相机、销毁句柄操作
    PostQuitMessage(0);
    CloseDevice();

    DeInitResources();

    CDialog::OnClose();
}

int CGrab_MSCDlg::CloseDevice(void)
{

    if (m_handle)
    {
        MV_CODEREADER_DestroyHandle(m_handle);
        m_handle = NULL;
    }

    m_bConnect = FALSE;
    m_bStartJob = FALSE;

    return MV_CODEREADER_OK;
}

void CGrab_MSCDlg::OnBnClickedSetParameterButton()
{
    UpdateData(TRUE);

    // TODO: Add your control notification handler code here
    int nRet = MV_CODEREADER_OK;
    CString cstrInfo;

    bool bIsSetted = true;
    // 设置曝光时间
    float fExposureTime = 0.0f;
    m_ctrlExposureEdit.GetWindowText(cstrInfo);
    fExposureTime = atof(CStringA(cstrInfo));
    nRet = MV_CODEREADER_SetFloatValue(m_handle, EXPOSURE_TIME, fExposureTime);
    if (MV_CODEREADER_OK != nRet)
    {
        bIsSetted = false;
        cstrInfo.Format(_T("Set exposure time failed! err code:%#x"), nRet);
        MessageBox(cstrInfo);
    }

    // 设置增益
    float fGain= 0.0f;
    m_ctrlGainEdit.GetWindowText(cstrInfo);
    fGain = atof(CStringA(cstrInfo));
    nRet = MV_CODEREADER_SetFloatValue(m_handle, GAIN, fGain);
    if (MV_CODEREADER_OK != nRet)
    {
        bIsSetted = false;
        cstrInfo.Format(_T("Set gain failed! err code:%#x"), nRet);
        MessageBox(cstrInfo);
    }

    // 设置帧率
    float fFrameRate= 0.0f;
    m_ctrlFrameRateEdit.GetWindowText(cstrInfo);
    fFrameRate = atof(CStringA(cstrInfo));
    nRet = MV_CODEREADER_SetFloatValue(m_handle, ACQUISITION_FRAME_RATE, fFrameRate);
    if (MV_CODEREADER_OK != nRet)
    {
        bIsSetted = false;
        cstrInfo.Format(_T("Set acquisition rate failed! err code:%#x"), nRet);
        MessageBox(cstrInfo);
    }
    if(bIsSetted)
    {
        MessageBox(_T("Set Param Succeed"));
    }
}
