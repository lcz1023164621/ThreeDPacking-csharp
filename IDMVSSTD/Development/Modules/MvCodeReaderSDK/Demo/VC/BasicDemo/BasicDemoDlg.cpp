
// BasicDemoDlg.cpp : 实现文件

#include "stdafx.h"
#include "BasicDemo.h"
#include "BasicDemoDlg.h"
#include <string.h>

#include <comdef.h>
#include <gdiplus.h>
#include <string>
using namespace std;
using namespace Gdiplus;
#pragma comment( lib, "gdiplus.lib" )
#include <WinGDI.h>
#include <windows.h>

#if (_MSC_VER >= 1900)
extern "C"
{
    FILE __iob_func[3] = { *stdin,*stdout,*stderr };
}
#endif

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

void    DebugInfo( char *szFormat, ...)
{
#ifdef WIN32

    char szInfo[8192];
    va_list ArgumentList;

    va_start(ArgumentList, szFormat); 
    vsprintf_s(szInfo, 8192, szFormat, ArgumentList);
    va_end(ArgumentList);

    OutputDebugStringA(szInfo);

#endif
}

// 用于应用程序“关于”菜单项的 CAboutDlg 对话框

class CAboutDlg : public CDialog
{
public:
	CAboutDlg();

// 对话框数据
	enum { IDD = IDD_ABOUTBOX };

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV 支持

// 实现
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


// CBasicDemoDlg 对话框

CBasicDemoDlg::CBasicDemoDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CBasicDemoDlg::IDD, pParent)
    , m_bConnect(0)
    , m_bStartJob(0)
    ,m_pBufForDriver(NULL)
    ,m_pBufForSaveImage(NULL)
    , m_nBufSizeForSaveImage(0)
    , m_bIsSoftTrigger(0)
    ,m_pcDataBuf(NULL)
    ,m_bBitmapInfo(NULL)
	,m_hProcessThread(NULL)
{
    memset(&m_stDeviceInfoList, 0, sizeof(m_stDeviceInfoList));
    memset(&m_stDeviceInfo, 0, sizeof(m_stDeviceInfo));
    memset(&m_stParam, 0, sizeof(MV_CODEREADER_DRAW_PARAM));
    memset(&m_pstParam, 0, sizeof(MV_CODEREADER_TJPG_PARAM));

    m_handle = NULL;
    m_hWndDisplay = NULL;
    m_pstImageInfoEx2 = NULL;
    m_nBufSizeForDriver = 0;
    m_MaxImageSize = 0;

    m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);
}

void CBasicDemoDlg::DoDataExchange(CDataExchange* pDX)
{
    CDialog::DoDataExchange(pDX);
    DDX_Control(pDX, IDC_OPEN_BUTTON, m_ctrlOpenButton);
    DDX_Control(pDX, IDC_CLOSE_BUTTON, m_ctrlCloseButton);
    DDX_Control(pDX, IDC_CONTINUS_MODE_RADIO, m_ctrlContinusModeRadio);
    DDX_Control(pDX, IDC_TRIGGER_MODE_RADIO, m_ctrlTriggerModeRadio);
    DDX_Control(pDX, IDC_SOFTWARE_TRIGGER_CHECK, m_ctrlSoftwareTriggerCheck);
    DDX_Control(pDX, IDC_START_GRABBING_BUTTON, m_ctrlStartGrabbingButton);
    DDX_Control(pDX, IDC_STOP_GRABBING_BUTTON, m_ctrlStopGrabbingButton);
    DDX_Control(pDX, IDC_SOFTWARE_ONCE_BUTTON, m_ctrlSoftwareOnceButton);
    DDX_Control(pDX, IDC_SAVE_BMP_BUTTON, m_ctrlSaveBmpButton);
    DDX_Control(pDX, IDC_SAVE_JPG_BUTTON, m_ctrlSaveJpgButton);
    DDX_Control(pDX, IDC_EXPOSURE_EDIT, m_ctrlExposureEdit);
    DDX_Control(pDX, IDC_GAIN_EDIT, m_ctrlGainEdit);
    DDX_Control(pDX, IDC_FRAME_RATE_EDIT, m_ctrlFrameRateEdit);
    DDX_Control(pDX, IDC_GET_PARAMETER_BUTTON, m_ctrlGetParameterButton);
    DDX_Control(pDX, IDC_SET_PARAMETER_BUTTON, m_ctrlSetParameterButton);
    DDX_Control(pDX, IDC_DEVICE_COMBO, m_ctrlDeviceCombo);
    DDX_Control(pDX, IDC_DISPLAY_INFO_LIST, m_ctrlDisplayInfoList);
}

BEGIN_MESSAGE_MAP(CBasicDemoDlg, CDialog)
	ON_WM_SYSCOMMAND()
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	//}}AFX_MSG_MAP
    ON_BN_CLICKED(IDC_ENUM_BUTTON, &CBasicDemoDlg::OnBnClickedEnumButton)
    ON_BN_CLICKED(IDC_OPEN_BUTTON, &CBasicDemoDlg::OnBnClickedOpenButton)
    ON_BN_CLICKED(IDC_START_GRABBING_BUTTON, &CBasicDemoDlg::OnBnClickedStartGrabbingButton)
    ON_BN_CLICKED(IDC_BTN_Start, &CBasicDemoDlg::OnBnClickedStartGrabbingButton)
    ON_BN_CLICKED(IDC_CONTINUS_MODE_RADIO, &CBasicDemoDlg::OnBnClickedContinusModeRadio)
    ON_BN_CLICKED(IDC_TRIGGER_MODE_RADIO, &CBasicDemoDlg::OnBnClickedTriggerModeRadio)
    ON_BN_CLICKED(IDC_SOFTWARE_TRIGGER_CHECK, &CBasicDemoDlg::OnBnClickedSoftwareTriggerCheck)
    ON_BN_CLICKED(IDC_SOFTWARE_ONCE_BUTTON, &CBasicDemoDlg::OnBnClickedSoftwareOnceButton)
    ON_BN_CLICKED(IDC_SAVE_JPG_BUTTON, &CBasicDemoDlg::OnBnClickedSaveJpgButton)
    ON_BN_CLICKED(IDC_GET_PARAMETER_BUTTON, &CBasicDemoDlg::OnBnClickedGetParameterButton)
    ON_BN_CLICKED(IDC_SET_PARAMETER_BUTTON, &CBasicDemoDlg::OnBnClickedSetParameterButton)
    ON_BN_CLICKED(IDC_SAVE_BMP_BUTTON, &CBasicDemoDlg::OnBnClickedSaveBmpButton)
    ON_BN_CLICKED(IDC_STOP_GRABBING_BUTTON, &CBasicDemoDlg::OnBnClickedStopGrabbingButton)
    ON_BN_CLICKED(IDC_CLOSE_BUTTON, &CBasicDemoDlg::OnBnClickedCloseButton)
    ON_WM_CLOSE()
    ON_BN_CLICKED(IDC_SAVE_RAW_BUTTON, &CBasicDemoDlg::OnBnClickedSaveRawButton)
END_MESSAGE_MAP()


// CBasicDemoDlg 消息处理程序

BOOL CBasicDemoDlg::OnInitDialog()
{
	CDialog::OnInitDialog();

	// 将“关于...”菜单项添加到系统菜单中。

	// IDM_ABOUTBOX 必须在系统命令范围内。
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

	// 设置此对话框的图标。当应用程序主窗口不是对话框时，框架将自动
	//  执行此操作
	SetIcon(m_hIcon, TRUE);			// 设置大图标
	SetIcon(m_hIcon, FALSE);		// 设置小图标

	// TODO: 在此添加额外的初始化代码

    // 创建窗口句柄
    CWnd *pWnd = GetDlgItem(IDC_DISPLAY_STATIC);
    if (NULL == pWnd)
    {
        return MV_CODEREADER_E_RESOURCE;
    }
    m_hWndDisplay = pWnd->GetSafeHwnd();
    if (NULL == m_hWndDisplay)
    {
        return MV_CODEREADER_E_RESOURCE;
    }

    GdiplusStartupInput gdiplusStartupInput;
    ULONG_PTR gdiplusToken;

    //初始化GDI+
    GdiplusStartup(&gdiplusToken, &gdiplusStartupInput, NULL);

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
    
    m_ctrlDisplayInfoList.ModifyStyle(0, LVS_REPORT);
    m_ctrlDisplayInfoList.SetExtendedStyle(m_ctrlDisplayInfoList.GetExtendedStyle() | LVS_EX_GRIDLINES | LVS_EX_FULLROWSELECT | LVS_EX_DOUBLEBUFFER);
    m_ctrlDisplayInfoList.InsertColumn(0,L"序号",LVCFMT_CENTER,40);
    m_ctrlDisplayInfoList.InsertColumn(1,L"识别时间",LVCFMT_CENTER,160);
    m_ctrlDisplayInfoList.InsertColumn(2,L"总体耗时",LVCFMT_CENTER,70);
    m_ctrlDisplayInfoList.InsertColumn(3,L"算法耗时",LVCFMT_CENTER,70);
    m_ctrlDisplayInfoList.InsertColumn(4,L"PPM",LVCFMT_CENTER,40);
    m_ctrlDisplayInfoList.InsertColumn(5,L"码制",LVCFMT_CENTER,80);
    m_ctrlDisplayInfoList.InsertColumn(6,L"码内容",LVCFMT_CENTER,180);
    m_ctrlDisplayInfoList.InsertColumn(7,L"总体评估",LVCFMT_CENTER,75);
    m_ctrlDisplayInfoList.InsertColumn(8,L"读码评分",LVCFMT_CENTER,75);

    return TRUE;  // 除非将焦点设置到控件，否则返回 TRUE
}

// ch:判断字符类型 | en:str type
bool CBasicDemoDlg::IsStrUTF8(const char* pBuffer, int size)
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
bool CBasicDemoDlg::Char2Wchar(const char *pStr, wchar_t *pOutWStr, int nOutStrSize)
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
bool CBasicDemoDlg::Wchar2char(wchar_t *pOutWStr, char *pStr)
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

int CBasicDemoDlg::InitResources()
{
    int nRet = MV_CODEREADER_OK;
    CString cstrInfo;

    try
    {
        int nSensorWidth = 0;
        int nSensorHight = 0;

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
                cstrInfo.Format(_T("Get hight failed! err code:%#x"), nRet);
                MessageBox(cstrInfo);
                throw nRet;
            }
            nSensorHight = stParam.nCurValue;

            m_MaxImageSize = nSensorWidth * nSensorHight + ImageExLen;
        }
        else
        {
            // 获取payloadSize成功
            m_MaxImageSize = stParam.nCurValue + ImageExLen;
        }

        m_pstParam.pBufOutput = (unsigned char*)malloc(m_MaxImageSize);
        if (NULL == m_pstParam.pBufOutput)
        {
            nRet = MV_CODEREADER_E_RESOURCE;
            throw nRet;
        }
        memset(m_pstParam.pBufOutput, 0, m_MaxImageSize);

        m_stParam.pData = (unsigned char*)malloc(m_MaxImageSize);
        if (NULL == m_stParam.pData)
        {
            nRet = MV_CODEREADER_E_RESOURCE;
            throw nRet;
        }
        memset(m_stParam.pData, 0, m_MaxImageSize);

        m_pcDataBuf =  (unsigned char*)malloc(m_MaxImageSize);
        if (NULL == m_pcDataBuf)
        {
            nRet = MV_CODEREADER_E_RESOURCE;
            throw nRet;
        }
        memset(m_pcDataBuf, 0, m_MaxImageSize);

        // 存储图像信息
        m_pstImageInfoEx2 = (MV_CODEREADER_IMAGE_OUT_INFO_EX2*)malloc(sizeof(MV_CODEREADER_IMAGE_OUT_INFO_EX2)); 
        if (NULL == m_pstImageInfoEx2)
        {
            nRet = MV_CODEREADER_E_RESOURCE;
            throw nRet;
        }
        memset(m_pstImageInfoEx2, 0, sizeof(MV_CODEREADER_IMAGE_OUT_INFO_EX2));
    }
    catch (...)
    {
        DeInitResources();
        return nRet;
    }

    return nRet;
}

void CBasicDemoDlg::DeInitResources()
{
	m_bStartJob = false;
	// 销毁取流线程
	if (NULL != m_hProcessThread)
	{
		//等待线程结束，关闭释放线程
		WaitForSingleObject(m_hProcessThread, 1000);
		CloseHandle(m_hProcessThread);
		m_hProcessThread = NULL;
	}

    if (NULL != m_pstParam.pBufOutput)
    {
        free(m_pstParam.pBufOutput);
        m_pstParam.pBufOutput = NULL;
    }

    if (NULL != m_stParam.pData)
    {
        free(m_stParam.pData);
        m_stParam.pData = NULL;
    }

    if (NULL != m_pcDataBuf)
    {
        free(m_pcDataBuf);
        m_pcDataBuf = NULL;
    }

    if (NULL != m_pstImageInfoEx2)
    {
        free(m_pstImageInfoEx2);
        m_pstImageInfoEx2 = NULL;
    }
}


void CBasicDemoDlg::OnSysCommand(UINT nID, LPARAM lParam)
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

// 如果向对话框添加最小化按钮，则需要下面的代码
//  来绘制该图标。对于使用文档/视图模型的 MFC 应用程序，
//  这将由框架自动完成。

void CBasicDemoDlg::OnPaint()
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

//当用户拖动最小化窗口时系统调用此函数取得光标
//显示。
HCURSOR CBasicDemoDlg::OnQueryDragIcon()
{
	return static_cast<HCURSOR>(m_hIcon);
}

BOOL CBasicDemoDlg::PreTranslateMessage(MSG* pMsg)
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

string CBasicDemoDlg::GetBarType(unsigned int nBarType)
{
    switch (nBarType)
    {
    case MV_CODEREADER_TDCR_DM:
        return "DM码";
    case MV_CODEREADER_TDCR_QR:
        return "QR码";
    case MV_CODEREADER_BCR_EAN8:
        return "EAN8码";
    case MV_CODEREADER_BCR_UPCE:
        return "UPCE码";
    case MV_CODEREADER_BCR_UPCA:
        return "UPCA码";
    case MV_CODEREADER_BCR_EAN13:
        return "EAN13码";
    case MV_CODEREADER_BCR_ISBN13:
        return "ISBN13码";
    case MV_CODEREADER_BCR_CODABAR:
        return "库德巴码";
    case MV_CODEREADER_BCR_ITF25:
        return "交叉25码";
    case MV_CODEREADER_BCR_CODE39:
        return " Code 39码";
    case MV_CODEREADER_BCR_CODE93:
        return "Code 93码";
    case MV_CODEREADER_BCR_CODE128:
        return "Code 128码";
    case MV_CODEREADER_TDCR_PDF417:
        return "PDF417码";
    case MV_CODEREADER_BCR_MATRIX25:
        return "MATRIX25码";
    case MV_CODEREADER_BCR_MSI:
        return "MSI码";
    case MV_CODEREADER_BCR_CODE11:
        return "Code 11码";
    case MV_CODEREADER_BCR_INDUSTRIAL25:
        return "industria125码";
    case MV_CODEREADER_BCR_CHINAPOST:
        return "中国邮政码";
    case MV_CODEREADER_BCR_ITF14:
        return "交叉14码";
    case MV_CODEREADER_TDCR_ECC140:
        return "ECC140码";
    default:
        return "/";
    }
}

// 渲染线程
void*  __stdcall CBasicDemoDlg::ProcessThread(void* pUser)
{
    int nRet = MV_CODEREADER_OK;

    CBasicDemoDlg* pThis = (CBasicDemoDlg*)pUser;
    if (NULL == pThis)
    {
        return NULL;
    }

    MV_CODEREADER_IMAGE_OUT_INFO_EX2 stImageInfo = {0};
    unsigned char * pData = NULL;
    while (pThis->m_bStartJob)
    {
        // 获取图像数据
        nRet = MV_CODEREADER_GetOneFrameTimeoutEx2(pThis->m_handle, &pData, &stImageInfo, 1000);
        if (nRet == MV_CODEREADER_OK)
        {
            if (NULL != pData)
            {
                //输出图像结果
                pThis->Display(pThis->m_hWndDisplay, pData, &stImageInfo);
            }
        }
        else
        {
            continue;
        }
    }
    return NULL;
}

int CBasicDemoDlg::Display(void* hWndDisplay, unsigned char *pData, MV_CODEREADER_IMAGE_OUT_INFO_EX2* pstDisplayImage)
{
    

    int nRet = MV_CODEREADER_OK;
    if ((NULL == pData) || (NULL == pstDisplayImage))
    {
        return MV_CODEREADER_E_PARAMETER;
    }

    if (pstDisplayImage->nWidth == 0 || pstDisplayImage->nHeight == 0 || NULL == hWndDisplay)
    {
        return MV_CODEREADER_E_PARAMETER;
    }

    MV_CODEREADER_RESULT_BCR_EX* stBcrResult = (MV_CODEREADER_RESULT_BCR_EX*)pstDisplayImage->pstCodeListEx;
    MV_CODEREADER_RESULT_BCR_EX stBcrParam = {0};
    stBcrParam.nCodeNum = stBcrResult->nCodeNum;
    memcpy(stBcrParam.stBcrInfoEx, stBcrResult->stBcrInfoEx, sizeof(stBcrResult->stBcrInfoEx));
    
    MV_CODEREADER_WAYBILL_LIST* pstWaybillList = (MV_CODEREADER_WAYBILL_LIST*)pstDisplayImage->pstWaybillList;
    MV_CODEREADER_WAYBILL_LIST stWaybill = {0};
    stWaybill.nOcrAllNum = pstWaybillList->nOcrAllNum;
    stWaybill.nWaybillNum = pstWaybillList->nWaybillNum;
    memcpy(stWaybill.stWaybillInfo, pstWaybillList->stWaybillInfo, sizeof(pstWaybillList->stWaybillInfo));
    
    MV_CODEREADER_OCR_INFO_LIST* pOcrList = (MV_CODEREADER_OCR_INFO_LIST*)pstDisplayImage->UnparsedOcrList.pstOcrList;
    MV_CODEREADER_OCR_INFO_LIST stOcr = {0};
    stOcr.nOCRAllNum = pOcrList->nOCRAllNum;
    memcpy(stOcr.stOcrRowInfo, pOcrList->stOcrRowInfo, sizeof(pOcrList->stOcrRowInfo));
    
    // 用于保存图像
    if (NULL != m_pstImageInfoEx2)
    {
        memcpy(m_pstImageInfoEx2, pstDisplayImage, sizeof(MV_CODEREADER_IMAGE_OUT_INFO_EX2));
    }
    
    if (NULL != m_pcDataBuf && m_pstImageInfoEx2->nFrameLen < m_MaxImageSize)
    {
        memcpy(m_pcDataBuf, pData, m_pstImageInfoEx2->nFrameLen);
    }

    // 显示图像
    HDC hDC  = ::GetDC((HWND)hWndDisplay);
    SetStretchBltMode(hDC, COLORONCOLOR);
    RECT wndRect;
    ::GetClientRect((HWND)hWndDisplay, &wndRect);

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
    m_stParam.hDC = hDC;
    m_stParam.nDstX = nDstX;
    m_stParam.nDstY = nDstY;
    m_stParam.nImageHeight = nImageHeight;
    m_stParam.nImageWidth = nImageWidth;
    m_stParam.nWndRectHeight = nWndRectHeight;
    m_stParam.nWndRectWidth = nWndRectWidth;

    if (PixelType_CodeReader_Gvsp_Jpeg == pstDisplayImage->enPixelType)
    {
        memset(m_pstParam.pBufOutput, 0, nImageWidth * nImageHeight);

        m_pstParam.pBufInput = pData;
        m_pstParam.nBufInputLen = nImageWidth * nImageHeight;
        nRet =  MvJpgDecompress(&m_pstParam);
        if (MV_CODEREADER_OK == nRet)
        {
            // 将Jpeg数据格式解压的数据封装为结构体，赋值作为参数传入，进行渲染
            memcpy(m_stParam.pData, m_pstParam.pBufOutput, m_pstParam.nBufOutputLen);
            nRet = Draw(&m_stParam);
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
        memset(m_stParam.pData, 0, nImageWidth * nImageHeight);
       // 相机获取的数据直接渲染
       memcpy(m_stParam.pData, pData, nImageWidth * nImageHeight);
       nRet = Draw(&m_stParam);
       if (MV_CODEREADER_OK != nRet)
       {
           return MV_CODEREADER_E_PARAMETER;
       }
    }

    // 识别到的条码进行画框
    Graphics gCode((HWND)m_hWndDisplay);
    Status nGdiStatus = Status::Ok;
    Pen pen(Color(0, 0, 255), 3);

    float fWidthProportion = (float)nWndRectWidth / nImageWidth;
    float fHeightProportion = (float)nWndRectHeight / nImageHeight;
    
    for (int i = 0; i < stBcrParam.nCodeNum; i++)
    {
        PointF point1((stBcrParam.stBcrInfoEx[i].pt[0].x * fWidthProportion), (stBcrParam.stBcrInfoEx[i].pt[0].y * fHeightProportion));
        PointF point2((stBcrParam.stBcrInfoEx[i].pt[1].x * fWidthProportion), (stBcrParam.stBcrInfoEx[i].pt[1].y * fHeightProportion));
        PointF point3((stBcrParam.stBcrInfoEx[i].pt[2].x * fWidthProportion), (stBcrParam.stBcrInfoEx[i].pt[2].y * fHeightProportion));
        PointF point4((stBcrParam.stBcrInfoEx[i].pt[3].x * fWidthProportion), (stBcrParam.stBcrInfoEx[i].pt[3].y * fHeightProportion));
        PointF points[4] = {point1, point2, point3, point4};
        PointF* pPoints = points;
        gCode.DrawPolygon(&pen, pPoints, 4);

        // 显示码制信息
        //系统时间
        SYSTEMTIME stTime = {0};
        GetLocalTime(&stTime);
        CString strParam("");
        CString strTime("");
        strTime.Format(L"%d/%d/%d %d:%d:%d:%d", stTime.wYear, stTime.wMonth, stTime.wDay, stTime.wHour, stTime.wMinute, stTime.wSecond, stTime.wMilliseconds);

        //序号
        int nNO = m_ctrlDisplayInfoList.GetItemCount();
        strParam.Format(L"%d", nNO);
        m_ctrlDisplayInfoList.InsertItem(0,(LPCTSTR)(strParam));
        //识别时间
        m_ctrlDisplayInfoList.SetItemText(0, 1, (LPCTSTR)(strTime));
        //总体耗时
        strParam.Format(L"%d", stBcrParam.stBcrInfoEx[i].nTotalProcCost);
        m_ctrlDisplayInfoList.SetItemText(0, 2, (LPCTSTR)(strParam));
        //算法耗时
        strParam.Format(L"%d", stBcrParam.stBcrInfoEx[i].sAlgoCost);
        m_ctrlDisplayInfoList.SetItemText(0, 3, (LPCTSTR)(strParam));
        //PPM
        strParam.Format(L"%d", stBcrParam.stBcrInfoEx[i].sPPM);
        m_ctrlDisplayInfoList.SetItemText(0, 4, (LPCTSTR)(strParam));
        //码制
        string strBarType = GetBarType(stBcrParam.stBcrInfoEx[i].nBarType);
        wchar_t chBarType[256] = {0};
        Char2Wchar(strBarType.c_str(), chBarType, 256);
        m_ctrlDisplayInfoList.SetItemText(0, 5, (LPCTSTR)(chBarType));

        //码内容
        wchar_t chCodeText[256] = {0};
        Char2Wchar(stBcrParam.stBcrInfoEx[i].chCode, chCodeText, 256);
        m_ctrlDisplayInfoList.SetItemText(0, 6, (LPCTSTR)(chCodeText));
        //总体评估
        strParam.Format(L"%d", stBcrParam.stBcrInfoEx[i].stCodeQuality.nOverQuality);
        m_ctrlDisplayInfoList.SetItemText(0, 7, (LPCTSTR)(strParam)); 
        //读码评分
        strParam.Format(L"%d", stBcrParam.stBcrInfoEx[i].nIDRScore);
        m_ctrlDisplayInfoList.SetItemText(0, 8, (LPCTSTR)(strParam));

    }
    

    Pen pen1(Color(255, 255, 0), 3);
 
    for (int i = 0; i < stWaybill.nWaybillNum; i++)
    {
        int x = stWaybill.stWaybillInfo[i].fCenterX * fWidthProportion;
        int y = stWaybill.stWaybillInfo[i].fCenterY * fHeightProportion;
        int w = stWaybill.stWaybillInfo[i].fWidth * fWidthProportion;
        int h = stWaybill.stWaybillInfo[i].fHeight * fHeightProportion;
        InnerDrawShape(&gCode, x, y, w, h, stWaybill.stWaybillInfo[i].fAngle);
    }

    for (int i = 0; i < stOcr.nOCRAllNum; i++)
    {
        int x = stOcr.stOcrRowInfo[i].nOcrRowCenterX * fWidthProportion;
        int y = stOcr.stOcrRowInfo[i].nOcrRowCenterY * fHeightProportion;
        int w = stOcr.stOcrRowInfo[i].nOcrRowWidth * fWidthProportion;
        int h = stOcr.stOcrRowInfo[i].nOcrRowHeight * fHeightProportion;

        InnerDrawShape(&gCode, x, y, w, h, stOcr.stOcrRowInfo[i].fOcrRowAngle);
    }

    ::ReleaseDC((HWND)hWndDisplay, hDC);


    return nRet;
}

unsigned int CBasicDemoDlg::InnerDrawShape(Graphics* g, float x, float y, float w, float h, float fAngle)
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


int  CBasicDemoDlg::Draw(MV_CODEREADER_DRAW_PARAM* pstParam)
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
int     CBasicDemoDlg::MvJpgDecompress(IN OUT MV_CODEREADER_TJPG_PARAM* pstParam)
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

int CBasicDemoDlg::GetCurrentConfigurationInformation()
{
    int nRet = MV_CODEREADER_OK;
    CString cstrInfo;

    // 获取触发模式
    unsigned int nTriggerMode = 0;
    MV_CODEREADER_ENUMVALUE stParam;
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
    }
    else if (MV_CODEREADER_TRIGGER_MODE_OFF == nTriggerMode)
    {
        ((CButton *)GetDlgItem(IDC_CONTINUS_MODE_RADIO))->SetCheck(TRUE);
        ((CButton *)GetDlgItem(IDC_TRIGGER_MODE_RADIO))->SetCheck(FALSE);
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

void CBasicDemoDlg::OnBnClickedEnumButton()
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

    if (0 == m_stDeviceInfoList.nDeviceNum)
    {
        CString errorMsg;
        errorMsg.Format(_T("None Device"));
        MessageBox(errorMsg, TEXT("device"), MB_OK | MB_ICONWARNING);
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
                                                         nIp4, 
                                                         nIp3, 
                                                         nIp2, 
                                                         nIp1);
        m_ctrlDeviceCombo.AddString(cstrInfo);
    }

    m_ctrlDeviceCombo.SetCurSel(0);
    GetDlgItem(IDC_OPEN_BUTTON)->EnableWindow(TRUE);
    UpdateData(FALSE);
}

void CBasicDemoDlg::OnBnClickedOpenButton()
{
    // TODO: Add your control notification handler code here
    int nRet = MV_CODEREADER_OK;
    CString cstrInfo;

    UpdateData(TRUE);

    if (true ==  m_bConnect)
    {
        cstrInfo.Format(_T("The camera is already connect"));
        MessageBox(cstrInfo);
        return ;
    }

    if (0 == m_stDeviceInfoList.nDeviceNum)
    {
        cstrInfo.Format(_T("Please discovery device first"));
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
    nRet = OpenDevice();
    if (MV_CODEREADER_OK != nRet)
    {
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

    // 初始化必要的资源
    InitResources();
    m_ctrlContinusModeRadio.EnableWindow(TRUE);
    m_ctrlTriggerModeRadio.EnableWindow(TRUE);
    GetDlgItem(IDC_OPEN_BUTTON)->EnableWindow(FALSE);
    GetDlgItem(IDC_CLOSE_BUTTON)->EnableWindow(TRUE);
    GetDlgItem(IDC_START_GRABBING_BUTTON)->EnableWindow(TRUE);
    GetDlgItem(IDC_GET_PARAMETER_BUTTON)->EnableWindow(TRUE);
    GetDlgItem(IDC_SET_PARAMETER_BUTTON)->EnableWindow(TRUE);
    GetDlgItem(IDC_SAVE_JPG_BUTTON)->EnableWindow(FALSE);
    GetDlgItem(IDC_SAVE_BMP_BUTTON)->EnableWindow(FALSE);
    GetDlgItem(IDC_SAVE_RAW_BUTTON)->EnableWindow(FALSE);
    GetDlgItem(IDC_EXPOSURE_EDIT)->EnableWindow(TRUE);
    GetDlgItem(IDC_GAIN_EDIT)->EnableWindow(TRUE);
    GetDlgItem(IDC_FRAME_RATE_EDIT)->EnableWindow(TRUE);


    ((CButton *)GetDlgItem(IDC_SOFTWARE_TRIGGER_CHECK))->SetCheck(FALSE);
    m_ctrlStartGrabbingButton.EnableWindow(TRUE);
    if(m_ctrlContinusModeRadio.GetCheck())
    {
        m_ctrlSoftwareTriggerCheck.EnableWindow(FALSE);
        m_ctrlContinusModeRadio.EnableWindow(FALSE);
        m_ctrlTriggerModeRadio.EnableWindow(TRUE);
    }
    else
    {
        m_ctrlSoftwareTriggerCheck.EnableWindow(TRUE);
        m_ctrlContinusModeRadio.EnableWindow(TRUE);
        m_ctrlTriggerModeRadio.EnableWindow(FALSE);
    }
    m_ctrlExposureEdit.EnableWindow(TRUE);
    m_ctrlGainEdit.EnableWindow(TRUE);
    m_ctrlFrameRateEdit.EnableWindow(TRUE);

}


void CBasicDemoDlg::OnBnClickedCloseButton()
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

            nRet = MV_CODEREADER_StopGrabbing(m_handle);
            if (MV_CODEREADER_OK != nRet)
            {
                cstrInfo.Format(_T("Stop grabbing failed! err code:%#x"), nRet);
                MessageBox(cstrInfo);
                m_bStartJob = false;
                return ;
            }

			// 销毁取流线程
			if (NULL != m_hProcessThread)
			{
				//等待线程结束，关闭释放线程
                nRet = WaitForSingleObject(m_hProcessThread, 1000);
				CloseHandle(m_hProcessThread);
				m_hProcessThread = NULL;
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

    if (m_pBufForDriver)
    {
        free(m_pBufForDriver);
        m_pBufForDriver = NULL;
    }
    if (m_pBufForSaveImage)
    {
        free(m_pBufForSaveImage);
        m_pBufForSaveImage = NULL;
    }
    m_nBufSizeForSaveImage  = 0;

	//销毁资源
	DeInitResources();

    // 关闭设备后清空各项参数数据
    ((CButton *)GetDlgItem(IDC_CONTINUS_MODE_RADIO))->SetCheck(FALSE);
    ((CButton *)GetDlgItem(IDC_TRIGGER_MODE_RADIO))->SetCheck(FALSE);
    ((CButton *)GetDlgItem(IDC_SOFTWARE_TRIGGER_CHECK))->SetCheck(FALSE);
    m_ctrlExposureEdit.SetWindowText(NULL);
    m_ctrlGainEdit.SetWindowText(NULL);
    m_ctrlFrameRateEdit.SetWindowText(NULL);

    m_bStartJob = false;
    m_bConnect = false;

    GetDlgItem(IDC_OPEN_BUTTON)->EnableWindow(TRUE);
    GetDlgItem(IDC_CLOSE_BUTTON)->EnableWindow(FALSE);
    GetDlgItem(IDC_START_GRABBING_BUTTON)->EnableWindow(FALSE);
    GetDlgItem(IDC_STOP_GRABBING_BUTTON)->EnableWindow(FALSE);
    GetDlgItem(IDC_SOFTWARE_ONCE_BUTTON)->EnableWindow(FALSE);
    GetDlgItem(IDC_TRIGGER_MODE_RADIO)->EnableWindow(FALSE);
    GetDlgItem(IDC_CONTINUS_MODE_RADIO)->EnableWindow(FALSE);
    GetDlgItem(IDC_SOFTWARE_TRIGGER_CHECK)->EnableWindow(FALSE);


    GetDlgItem(IDC_SET_PARAMETER_BUTTON)->EnableWindow(FALSE);
    GetDlgItem(IDC_GET_PARAMETER_BUTTON)->EnableWindow(FALSE);
    GetDlgItem(IDC_SAVE_JPG_BUTTON)->EnableWindow(FALSE);
    GetDlgItem(IDC_SAVE_BMP_BUTTON)->EnableWindow(FALSE);
    GetDlgItem(IDC_SAVE_RAW_BUTTON)->EnableWindow(FALSE);
    GetDlgItem(IDC_EXPOSURE_EDIT)->EnableWindow(FALSE);
    GetDlgItem(IDC_GAIN_EDIT)->EnableWindow(FALSE);
    GetDlgItem(IDC_FRAME_RATE_EDIT)->EnableWindow(FALSE);



}

void CBasicDemoDlg::OnBnClickedStartGrabbingButton()
{
    // TODO: Add your control notification handler code here
    int nRet = MV_CODEREADER_OK;
    CString cstrInfo;
    m_bStartJob = true;
    m_ctrlDisplayInfoList.DeleteAllItems();

    // 创建接收 处理线程
    m_hProcessThread = CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)ProcessThread, this, 0, NULL);
    if (NULL == m_hProcessThread)
    {
        cstrInfo.Format(_T("Create proccess Thread failed! "));
        MessageBox(cstrInfo);
        return;
    }

    // 开始工作流程
    nRet = MV_CODEREADER_StartGrabbing(m_handle);
    if (MV_CODEREADER_OK != nRet)
    {
        cstrInfo.Format(_T("Start grabbing failed! err code:%#x"), nRet);
        MessageBox(cstrInfo);
        return ;
    }

    m_ctrlContinusModeRadio.EnableWindow(FALSE);
    m_ctrlTriggerModeRadio.EnableWindow(FALSE);
    GetDlgItem(IDC_SAVE_BMP_BUTTON)->EnableWindow(TRUE);
    GetDlgItem(IDC_SAVE_RAW_BUTTON)->EnableWindow(TRUE);
    GetDlgItem(IDC_SAVE_JPG_BUTTON)->EnableWindow(TRUE);
    GetDlgItem(IDC_START_GRABBING_BUTTON)->EnableWindow(FALSE);
    GetDlgItem(IDC_STOP_GRABBING_BUTTON)->EnableWindow(TRUE);
    GetDlgItem(IDC_SOFTWARE_TRIGGER_CHECK)->EnableWindow(FALSE);
    if(((CButton *)GetDlgItem(IDC_SOFTWARE_TRIGGER_CHECK))->GetCheck())
    {
        GetDlgItem(IDC_SOFTWARE_ONCE_BUTTON)->EnableWindow(TRUE);
    }


    //GetDlgItem(IDC_CLOSE_BUTTON)->EnableWindow(FALSE);
}

void CBasicDemoDlg::OnBnClickedStopGrabbingButton()
{
    // TODO: Add your control notification handler code here
    int nRet = MV_CODEREADER_OK;
    CString cstrInfo;

    m_bStartJob = false;


    // 停止工作流程
    nRet = MV_CODEREADER_StopGrabbing(m_handle);
    if (MV_CODEREADER_OK != nRet)
    {
        cstrInfo.Format(_T("Stop grabbing failed! err code:%#x"), nRet);
        MessageBox(cstrInfo);
        m_bStartJob = false;
        return ;
    }
    // 销毁取流线程
    if (NULL != m_hProcessThread)
    {
        //等待线程结束，关闭释放线程
        WaitForSingleObject(m_hProcessThread, 1000);
        CloseHandle(m_hProcessThread);
        m_hProcessThread = NULL;
    }

    m_ctrlContinusModeRadio.EnableWindow(TRUE);
    m_ctrlTriggerModeRadio.EnableWindow(TRUE);
    GetDlgItem(IDC_START_GRABBING_BUTTON)->EnableWindow(TRUE);
    GetDlgItem(IDC_STOP_GRABBING_BUTTON)->EnableWindow(FALSE);
    GetDlgItem(IDC_SOFTWARE_ONCE_BUTTON)->EnableWindow(FALSE);
    GetDlgItem(IDC_SAVE_JPG_BUTTON)->EnableWindow(FALSE);
    GetDlgItem(IDC_SAVE_BMP_BUTTON)->EnableWindow(FALSE);
    GetDlgItem(IDC_SAVE_RAW_BUTTON)->EnableWindow(FALSE);
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
    //GetDlgItem(IDC_CLOSE_BUTTON)->EnableWindow(TRUE);
}


void CBasicDemoDlg::OnBnClickedContinusModeRadio()
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
    GetDlgItem(IDC_CONTINUS_MODE_RADIO)->EnableWindow(FALSE);
    ((CButton *)GetDlgItem(IDC_CONTINUS_MODE_RADIO))->SetCheck(TRUE);
    GetDlgItem(IDC_TRIGGER_MODE_RADIO)->EnableWindow(TRUE);
    ((CButton *)GetDlgItem(IDC_TRIGGER_MODE_RADIO))->SetCheck(FALSE);
    GetDlgItem(IDC_SOFTWARE_TRIGGER_CHECK)->EnableWindow(FALSE);
    ((CButton *)GetDlgItem(IDC_SOFTWARE_TRIGGER_CHECK))->SetCheck(FALSE);
    GetDlgItem(IDC_START_GRABBING_BUTTON)->EnableWindow(TRUE);



    


    UpdateData(FALSE);
}

void CBasicDemoDlg::OnBnClickedTriggerModeRadio()
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

    if (m_bIsSoftTrigger && m_bStartJob)
    {
        GetDlgItem(IDC_SOFTWARE_ONCE_BUTTON)->EnableWindow(TRUE);
    }
    else
    {
        ((CButton *)GetDlgItem(IDC_SOFTWARE_TRIGGER_CHECK))->SetCheck(FALSE);
        GetDlgItem(IDC_SOFTWARE_ONCE_BUTTON)->EnableWindow(FALSE);
    }
    m_ctrlTriggerModeRadio.EnableWindow(FALSE);
    m_ctrlTriggerModeRadio.SetCheck(TRUE);
    m_ctrlContinusModeRadio.EnableWindow(TRUE);
    m_ctrlContinusModeRadio.SetCheck(FALSE);
    m_ctrlSoftwareTriggerCheck.EnableWindow(TRUE);
   

    UpdateData(FALSE);
}

void CBasicDemoDlg::OnBnClickedSoftwareTriggerCheck()
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

void CBasicDemoDlg::OnBnClickedSoftwareOnceButton()
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

char* CBasicDemoDlg::GetCurrentProGramPath(char* pFilePath, int nSize)
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

void CBasicDemoDlg::OnBnClickedSaveBmpButton()
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

    // 判断是否有有效数据
    if (NULL == m_pcDataBuf)
    {
        cstrInfo.Format(_T("No data，Save BMP failed!"));
        MessageBox(cstrInfo);
        return;
    }

    // 判断是否可保存Bmp图
    if (PixelType_CodeReader_Gvsp_Mono8 != m_pstImageInfoEx2->enPixelType)
    {
        cstrInfo.Format(_T("Unable to save BMP image!"));
        MessageBox(cstrInfo);
        return;
    }

    FILE* pfile = NULL;
    char filename[MAX_PATH] = {0};

    MV_CODEREADER_SAVE_IMAGE_PARAM_EX* pstParam = new MV_CODEREADER_SAVE_IMAGE_PARAM_EX;
    memset(pstParam, 0, sizeof(MV_CODEREADER_SAVE_IMAGE_PARAM_EX));
    pstParam->pData = m_pcDataBuf;
    pstParam->nDataLen = m_pstImageInfoEx2->nFrameLen;
    pstParam->nWidth = m_pstImageInfoEx2->nWidth;
    pstParam->nHeight = m_pstImageInfoEx2->nHeight;
    pstParam->enPixelType = m_pstImageInfoEx2->enPixelType;
    pstParam->nBufferSize = m_MaxImageSize;
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
        pfile = fopen(filename,"wb+");
        if(pfile == NULL)
        {
			cstrInfo.Format(_T("Open file failed"));
            MessageBox(cstrInfo);
            return ;
        }

        fwrite(pstParam->pImageBuffer, 1, pstParam->nImageLen, pfile);
        cstrInfo.Format(_T("Save BMP image success!"));
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

void CBasicDemoDlg::OnBnClickedSaveJpgButton()
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

    // 判断是否有有效数据
    if (NULL == m_pcDataBuf)
    {
        cstrInfo.Format(_T("No valid image data，Save JPG image failed!"));
        MessageBox(cstrInfo);
        return;
    }

    // 保存文件
    FILE* pfile = NULL;
    char filename[MAX_PATH] = {0};

    // 判断PixelType格式存图, 若Jpeg格式直接存图, Mono8格式转换存图
    if (PixelType_CodeReader_Gvsp_Jpeg == m_pstImageInfoEx2->enPixelType)
    {
        m_criSection.Lock();
        CTime currTime;                                     // 获取系统时间作为保存图片文件名
        currTime = CTime::GetCurrentTime();
        char chCurDir[MAX_PATH] = {0};
        GetCurrentProGramPath(chCurDir, MAX_PATH);
        sprintf(filename,("%s\\%.4d%.2d%.2d%.2d%.2d%.2d.jpg"), chCurDir, currTime.GetYear(), currTime.GetMonth(),
            currTime.GetDay(), currTime.GetHour(), currTime.GetMinute(), currTime.GetSecond());
        pfile = fopen(filename,"wb+");

		
        if(pfile == NULL)
        {
			cstrInfo.Format(_T("Open file failed"));
            MessageBox(cstrInfo);
            return ;
        }
        fwrite(m_pcDataBuf, 1, m_pstImageInfoEx2->nFrameLen, pfile);
        m_criSection.Unlock();
        cstrInfo.Format(_T("Save JPG image success!"));
        MessageBox(cstrInfo);
    }
    else
    {
        // 获取图像转换信息
        MV_CODEREADER_SAVE_IMAGE_PARAM_EX* pstParam = new MV_CODEREADER_SAVE_IMAGE_PARAM_EX;
        memset(pstParam, 0, sizeof(MV_CODEREADER_SAVE_IMAGE_PARAM_EX));
        pstParam->pData = m_pcDataBuf;
        pstParam->nDataLen = m_pstImageInfoEx2->nFrameLen;
        pstParam->nWidth = m_pstImageInfoEx2->nWidth;
        pstParam->nHeight = m_pstImageInfoEx2->nHeight;
        pstParam->enPixelType = m_pstImageInfoEx2->enPixelType;//PixelType_CodeReader_Gvsp_Mono8;
        pstParam->nBufferSize = m_MaxImageSize;
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
            pfile = fopen(filename,"wb+");

            if(pfile == NULL)
            {
                cstrInfo.Format(_T("Open file failed"));
                MessageBox(cstrInfo);
                return ;
            }

            fwrite(pstParam->pImageBuffer, 1, pstParam->nImageLen, pfile);
            cstrInfo.Format(_T("Save JPG image success!"));
            MessageBox(cstrInfo);
        }
        else
        {
            cstrInfo.Format(_T("Save JPG image failed! err code:%#x"), nRet);
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


void CBasicDemoDlg::OnBnClickedSaveRawButton()
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

    // 判断是否有有效数据
    if (NULL == m_pcDataBuf)
    {
        cstrInfo.Format(_T("No valid image data，Save RAW failed!"));
        MessageBox(cstrInfo);
        return;
    }
    
    // 判断是否可保存Raw图
    if (PixelType_CodeReader_Gvsp_Mono8 != m_pstImageInfoEx2->enPixelType)
    {
        cstrInfo.Format(_T("Unable to save Raw image!"));
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
    pfile = fopen(filename,"wb+");
    if(pfile == NULL)
    {
        cstrInfo.Format(_T("Open file failed!"));
        MessageBox(cstrInfo);
        return ;
    }
    m_criSection.Lock();
    fwrite(m_pcDataBuf, 1, m_pstImageInfoEx2->nFrameLen, pfile);
    m_criSection.Unlock();
    cstrInfo.Format(_T("Save RAW image success!"));
    MessageBox(cstrInfo);

    if (NULL != pfile)
    {
        fclose (pfile);
        pfile = NULL;
    }
}

void CBasicDemoDlg::OnBnClickedGetParameterButton()
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


void CBasicDemoDlg::OnBnClickedSetParameterButton()
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

void CBasicDemoDlg::OnClose()
{
    // TODO: Add your message handler code here and/or call default
    // 关闭程序，执行断开相机、销毁句柄操作
    PostQuitMessage(0);

    CloseDevice();

    DeInitResources();
    
    CDialog::OnClose();
}
int CBasicDemoDlg::OpenDevice()
{
	int nRet = MV_CODEREADER_OK;
	CString cstrInfo;

	do
	{
		nRet = MV_CODEREADER_SetAreaInfoConfig(m_handle,0xa80,0x10000);
		if (MV_CODEREADER_OK != nRet)
		{
			cstrInfo.Format(_T("Set area failed! err code:%#x"), nRet);
			break;
		}

		nRet = MV_CODEREADER_OpenDevice(m_handle);
		if (MV_CODEREADER_E_REGION_VERIFICATION == nRet)
		{
			nRet = MV_CODEREADER_SetAreaInfoConfig(m_handle,0xa80,0x20000);
			if (MV_CODEREADER_OK != nRet)
			{
				cstrInfo.Format(_T("Set area failed! err code:%#x"), nRet);
				break;
			}
		
			nRet = MV_CODEREADER_OpenDevice(m_handle);
			if (MV_CODEREADER_OK != nRet)
			{		
				cstrInfo.Format(_T("Open device failed! err code:%#x"), nRet);
				break;
			}
		}
		
	}while (false);

	if (MV_CODEREADER_OK != nRet)
	{
		MessageBox(cstrInfo);
		MV_CODEREADER_DestroyHandle(m_handle);
		m_handle = NULL;
	}
	
	return nRet;

}
int CBasicDemoDlg::CloseDevice(void)
{
   
    if (m_handle)
    {
        MV_CODEREADER_DestroyHandle(m_handle);
        m_handle = NULL;
    }

    m_bConnect = FALSE;
    m_bStartJob = FALSE;

    if (m_pBufForDriver)
    {
        free(m_pBufForDriver);
        m_pBufForDriver = NULL;
    }
    m_nBufSizeForDriver = 0;

    if (m_pBufForSaveImage)
    {
        free(m_pBufForSaveImage);
        m_pBufForSaveImage = NULL;
    }
    m_nBufSizeForSaveImage  = 0;

    return MV_CODEREADER_OK;
}
