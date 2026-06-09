
// MVIDCodeReaderDemoDlg.cpp : 实现文件
//

#include "stdafx.h"
#include "MVIDCodeReaderDemo.h"
#include "MVIDCodeReaderDemoDlg.h"
#include <io.h>


#ifdef _DEBUG
#define new DEBUG_NEW
#endif

#define BMP_BUF_SIZE    (5000*5000)

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


bool IsStrUTF8(const char* pBuffer, int size)
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

bool Char2Wchar(const char *pStr, wchar_t *pOutWStr, int nOutStrSize)
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

CMVIDCodeReaderDemoDlg::CMVIDCodeReaderDemoDlg(CWnd* pParent /*=NULL*/)
: CDialog(CMVIDCodeReaderDemoDlg::IDD, pParent)
, m_nDeviceCombo(0)
, m_nListNum(0)
, m_strImageFilePath(_T(""))
, m_bProcess(FALSE)
, m_handle(NULL)
, m_hThread(NULL)
, m_nItem(0)
, m_nSubItem(0)
, m_pstProcParam(NULL)
, m_pstOutput(NULL)
, m_bBitmapInfo(NULL)
, m_strExposureEdit(_T(""))
, m_strGainEdit(_T(""))
, m_strFrameRateEdit(_T(""))
, m_pstDevList(NULL)
, m_nSystemLanguageId(0)
, m_bIsEnum(FALSE)
, m_bIsInitRes(FALSE)
, m_bIsLoadImg(FALSE)
{
    m_hWndDisplay = NULL;
    memset(&m_stBmpFile, 0, sizeof(BmpFile));
    m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);
}

void CMVIDCodeReaderDemoDlg::DoDataExchange(CDataExchange* pDX)
{
    CDialog::DoDataExchange(pDX);
    DDX_Control(pDX, IDC_COMBO_CamList, m_ctrlDeviceCombo);
    DDX_CBIndex(pDX, IDC_COMBO_CamList, m_nDeviceCombo);
    DDX_Control(pDX, IDC_LIST_Result, m_ctrlListBoxResult);
    DDX_Text(pDX, IDC_EDIT_ImagePath, m_strImageFilePath);
    DDX_Control(pDX, IDC_LIST_Param, m_list);
    DDX_Control(pDX, IDC_EXPOSURE_EDIT, m_ctrlExposureEdit);
    DDX_Text(pDX, IDC_EXPOSURE_EDIT, m_strExposureEdit);
    DDX_Control(pDX, IDC_GAIN_EDIT, m_ctrlGainEdit);
    DDX_Text(pDX, IDC_GAIN_EDIT, m_strGainEdit);
    DDX_Control(pDX, IDC_FRAME_RATE_EDIT, m_ctrlFrameRateEdit);
    DDX_Text(pDX, IDC_FRAME_RATE_EDIT, m_strFrameRateEdit);
}

BEGIN_MESSAGE_MAP(CMVIDCodeReaderDemoDlg, CDialog)
    ON_WM_SYSCOMMAND()
    ON_WM_PAINT()
    ON_WM_QUERYDRAGICON()
    //}}AFX_MSG_MAP
    ON_BN_CLICKED(IDC_BTN_Enum, &CMVIDCodeReaderDemoDlg::OnBnClickedBtnEnum)
    ON_BN_CLICKED(IDC_BTN_Start, &CMVIDCodeReaderDemoDlg::OnBnClickedBtnStart)
    ON_BN_CLICKED(IDC_BTN_Stop, &CMVIDCodeReaderDemoDlg::OnBnClickedBtnStop)
    ON_BN_CLICKED(IDC_BTN_Load, &CMVIDCodeReaderDemoDlg::OnBnClickedBtnLoad)
    ON_BN_CLICKED(IDC_BTN_Proc, &CMVIDCodeReaderDemoDlg::OnBnClickedBtnProc)
    ON_NOTIFY(NM_CLICK, IDC_LIST_Param, &CMVIDCodeReaderDemoDlg::OnNMClickListParam)
    ON_EN_KILLFOCUS(IDC_EDIT_CREATEID, &CMVIDCodeReaderDemoDlg::OnKillfocusEdit)//添加动态生成编辑框的失去焦点响应函数
    ON_BN_CLICKED(IDC_BTN_Init, &CMVIDCodeReaderDemoDlg::OnBnClickedBtnInit)
    ON_BN_CLICKED(IDC_BTN_Clear, &CMVIDCodeReaderDemoDlg::OnBnClickedBtnClear)
    ON_BN_CLICKED(IDC_BTN_CUTOUT, &CMVIDCodeReaderDemoDlg::OnBnClickedBtnCutout)
    ON_WM_CLOSE()
    ON_BN_CLICKED(IDC_GET_PARAMETER_BUTTON, &CMVIDCodeReaderDemoDlg::OnBnClickedGetParameterButton)
    ON_BN_CLICKED(IDC_SET_PARAMETER_BUTTON, &CMVIDCodeReaderDemoDlg::OnBnClickedSetParameterButton)
END_MESSAGE_MAP()

// 控件使能
void CMVIDCodeReaderDemoDlg::EnableControls()
{
    GetDlgItem(IDC_BTN_Enum)->EnableWindow(!m_bProcess);
    GetDlgItem(IDC_BTN_Start)->EnableWindow(m_bIsEnum && !m_bProcess);
    GetDlgItem(IDC_BTN_Stop)->EnableWindow(m_bProcess);
    GetDlgItem(IDC_GET_PARAMETER_BUTTON)->EnableWindow(m_bProcess);
    GetDlgItem(IDC_SET_PARAMETER_BUTTON)->EnableWindow(m_bProcess);
    GetDlgItem(IDC_EXPOSURE_EDIT)->EnableWindow(m_bProcess);
    GetDlgItem(IDC_GAIN_EDIT)->EnableWindow(m_bProcess);
    GetDlgItem(IDC_FRAME_RATE_EDIT)->EnableWindow(m_bProcess);

    GetDlgItem(IDC_BTN_Init)->EnableWindow(!m_bProcess && !m_bIsInitRes);
    GetDlgItem(IDC_BTN_Load)->EnableWindow(!m_bProcess && m_bIsInitRes);
    GetDlgItem(IDC_BTN_Proc)->EnableWindow(!m_bProcess && m_bIsLoadImg);
    GetDlgItem(IDC_BTN_CUTOUT)->EnableWindow(!m_bProcess && m_bIsLoadImg);

    GetDlgItem(IDC_BTN_Clear)->EnableWindow(TRUE);
}

BOOL CMVIDCodeReaderDemoDlg::OnInitDialog()
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

    CWnd *pWnd = GetDlgItem(IDC_IMAGE_STATIC);
    if (NULL == pWnd)
    {
        return MVID_CR_E_RESOURCE;
    }

    m_hWndDisplay = pWnd->GetSafeHwnd();
    if (NULL == m_hWndDisplay)
    {
        return MVID_CR_E_RESOURCE;
    }

    GdiplusStartupInput gdiplusStartupInput;
    ULONG_PTR gdiplusToken;

    // ch:初始化GDI+ | en:InitializeGDI+
    GdiplusStartup(&gdiplusToken, &gdiplusStartupInput, NULL);

    InitListCtrl();

    m_nSystemLanguageId = GetUserDefaultUILanguage();

    if (NULL == m_pstProcParam)
    {
        m_pstProcParam = (MVID_PROC_PARAM *)malloc(sizeof(MVID_PROC_PARAM));
        if (NULL == m_pstProcParam)
        {
            return FALSE;
        }
    }
    memset(m_pstProcParam, 0, sizeof(MVID_PROC_PARAM));

    if (NULL == m_pstOutput)
    {
        m_pstOutput = (MVID_CAM_OUTPUT_INFO *)malloc(sizeof(MVID_CAM_OUTPUT_INFO));
        if (NULL == m_pstOutput)
        {
            return FALSE;
        }
    }
    memset(m_pstOutput, 0, sizeof(MVID_CAM_OUTPUT_INFO));

    memset(&m_stBmpFile, 0, sizeof(BmpFile));

    m_stBmpFile.pBuf    = (unsigned char*)malloc(BMP_BUF_SIZE);
    m_stBmpFile.nBufSize    = BMP_BUF_SIZE;

    m_mapParamList[_T(KEY_BCR_ABILITY)]   = 0;
    m_mapParamList[_T(KEY_BCR_ROI_X)]   = 0;
    m_mapParamList[_T(KEY_BCR_ROI_Y)]   = 0;
    m_mapParamList[_T(KEY_BCR_ROI_WIDTH)]   = 0;
    m_mapParamList[_T(KEY_BCR_ROI_HEIGHT)]   = 0;
    m_mapParamList[_T(KEY_BCR_MAX_WIDTH)]   = 0;
    m_mapParamList[_T(KEY_BCR_MAX_HEIGHT)]   = 0;
    m_mapParamList[_T(KEY_BCR_LOCBARNUM)]   = 0;
    m_mapParamList[_T(KEY_BCR_LOCWINSIZE)]   = 0;
    m_mapParamList[_T(KEY_BCR_WAITINGTIME)]   = 0;
    m_mapParamList[_T(KEY_BCR_SEGQUIETW)]   = 0;
    m_mapParamList[_T(KEY_BCR_DFKSIZELOWERLIMIT)]   = 0;
    m_mapParamList[_T(KEY_BCR_DFKSIZEUPPERLIMIT)]   = 0;
    m_mapParamList[_T(KEY_BCR_SAVEIMAGELEVEL)]  = 0;
    m_mapParamList[_T(KEY_BCR_APPMODE)]         = 0;
    m_mapParamList[_T(KEY_BCR_DISTORTION)]      = 0;
    m_mapParamList[_T(KEY_BCR_WHITEGAP)]        = 0;
    m_mapParamList[_T(KEY_BCR_SPOT)]            = 0;
    m_mapParamList[_T(KEY_BCR_SAMPLELEVEL)]     = 0;
    m_mapParamList[_T(KEY_BCR_IAMGEMORPH)]      = 0;
    m_mapParamList[_T(KEY_BCR_DELERRFLAG)]      = 0;

    m_mapParamList[_T(KEY_TDCR_ABILITY)]        = 0;
    m_mapParamList[_T(KEY_TDCR_ROI_X)]          = 0;
    m_mapParamList[_T(KEY_TDCR_ROI_Y)]          = 0;
    m_mapParamList[_T(KEY_TDCR_ROI_WIDTH)]      = 0;
    m_mapParamList[_T(KEY_TDCR_ROI_HEIGHT)]     = 0;
    m_mapParamList[_T(KEY_TDCR_MAX_WIDTH)]      = 0;
    m_mapParamList[_T(KEY_TDCR_MAX_HEIGHT)]     = 0;
    m_mapParamList[_T(KEY_TDCR_LOCCODENUM)]     = 0;
    m_mapParamList[_T(KEY_TDCR_MINBARSIZE)]     = 0;
    m_mapParamList[_T(KEY_TDCR_MAXBARSIZE)]     = 0;
    m_mapParamList[_T(KEY_TDCR_MIRRORMODE)]     = 0;
    m_mapParamList[_T(KEY_TDCR_SAMPLELEVEL)]    = 0;
    m_mapParamList[_T(KEY_TDCR_CODECOLOR)]      = 0;
    m_mapParamList[_T(KEY_TDCR_DISCRETEFLAG)]   = 0;
    m_mapParamList[_T(KEY_TDCR_DISTORTIONFLAG)] = 0;
    m_mapParamList[_T(KEY_TDCR_ADVANCEPARAM)]   = 0;
    m_mapParamList[_T(KEY_TDCR_ADVANCEPARAM2)]  = 0;
    m_mapParamList[_T(KEY_TDCR_WAITINGTIME)]    = 0;
    m_mapParamList[_T(KEY_TDCR_DEBUGFLAG)]      = 0;
    m_mapParamList[_T(KEY_TDCR_APPMODE)]        = 0;
    m_mapParamList[_T(KEY_TDCR_RECTANGLE)]      = 0;

    m_mapParamList[_T(KEY_WAYBILL_ABILITY)]         = 0;
    m_mapParamList[_T(KEY_WAYBILL_MAX_WIDTH)]       = 0;
    m_mapParamList[_T(KEY_WAYBILL_MAX_HEIGHT)]      = 0;
    m_mapParamList[_T(KEY_WAYBILL_OUTPUTIMAGETYPE)] = 0;
    m_mapParamList[_T(KEY_WAYBILL_JPGQUALITY)]      = 0;
    m_mapParamList[_T(KEY_WAYBILL_MINWIDTH)]        = 0;
    m_mapParamList[_T(KEY_WAYBILL_MINHEIGHT)]       = 0;
    m_mapParamList[_T(KEY_WAYBILL_MAXWIDTH)]        = 0;
    m_mapParamList[_T(KEY_WAYBILL_MAXHEIGHT)]       = 0;
    m_mapParamList[_T(KEY_WAYBILL_MORPHTIMES)]      = 0;
    m_mapParamList[_T(KEY_WAYBILL_GRAYLOW)]         = 0;
    m_mapParamList[_T(KEY_WAYBILL_GRAYMID)]         = 0;
    m_mapParamList[_T(KEY_WAYBILL_GRAYHIGH)]        = 0;
    m_mapParamList[_T(KEY_WAYBILL_BINARYADAPTIVE)]  = 0;
    m_mapParamList[_T(KEY_WAYBILL_BOUNDARYROW)]     = 0;
    m_mapParamList[_T(KEY_WAYBILL_BOUNDARYCOL)]     = 0;
    m_mapParamList[_T(KEY_WAYBILL_MAXBILLBARHEIGTHRATIO )]  = 0;
    m_mapParamList[_T(KEY_WAYBILL_MAXBILLBARWIDTHRATIO)]    = 0;
    m_mapParamList[_T(KEY_WAYBILL_MINBILLBARHEIGTHRATIO)]   = 0;
    m_mapParamList[_T(KEY_WAYBILL_MINBILLBARWIDTHRATIO)]    = 0;
    m_mapParamList[_T(KEY_WAYBILL_ENHANCEMETHOD)]           = 0;
    m_mapParamList[_T(KEY_WAYBILL_ENHANCECLIPRATIOLOW)]     = 0;
    m_mapParamList[_T(KEY_WAYBILL_ENHANCECLIPRATIOHIGH)]    = 0;
    m_mapParamList[_T(KEY_WAYBILL_ENHANCECONTRASTFACTOR)]   = 0;
    m_mapParamList[_T(KEY_WAYBILL_ENHANCESHARPENFACTOR)]    = 0;
    m_mapParamList[_T(KEY_WAYBILL_SHARPENKERNELSIZE)]       = 0;
    m_mapParamList[_T(KEY_WAYBILL_CODEBOUNDARYROW)]         = 0;
    m_mapParamList[_T(KEY_WAYBILL_CODEBOUNDARYCOL)]         = 0;

    EnableControls();
    return TRUE;
}

void CMVIDCodeReaderDemoDlg::OnSysCommand(UINT nID, LPARAM lParam)
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

void CMVIDCodeReaderDemoDlg::OnPaint()
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

HCURSOR CMVIDCodeReaderDemoDlg::OnQueryDragIcon()
{
    return static_cast<HCURSOR>(m_hIcon);
}

// 枚举相机
void CMVIDCodeReaderDemoDlg::OnBnClickedBtnEnum()
{
    CString strMsg = _T("");

    // ch:清除设备列表框中的信息 | en:Clear Device List Information
    m_ctrlDeviceCombo.ResetContent();
    m_nDeviceCombo = 0;

    // ch:初始化设备信息列表 | en:Device Information List Initialization
    if (NULL == m_pstDevList)
    {
        m_pstDevList = (MVID_CAMERA_INFO_LIST*)malloc(sizeof(MVID_CAMERA_INFO_LIST));
        if (NULL == m_pstDevList)
        {
            return;
        }
    }
    memset(m_pstDevList, 0, sizeof(MVID_CAMERA_INFO_LIST));

    // ch:枚举子网内所有设备 | en:Enumerate all devices within subnet  MVID_CR_CAM_EnumDevicesByCfg
    int nRet = MVID_CR_CAM_EnumDevices(m_pstDevList);
    if (MVID_CR_OK != nRet)
    {
        ShowErrorMsg(TEXT("MVID_CR_CAM_EnumDevices "), nRet);
        return;
    }

    int nIp1 = 0, nIp2 = 0, nIp3 = 0, nIp4 = 0;
    for (int i = 0; i < m_pstDevList->nCamNum; i++)
    {
        MVID_CAMERA_INFO* pDeviceInfo = m_pstDevList->pstCamInfo[i];
        if (NULL == pDeviceInfo)
        {
            continue;
        }

        if (pDeviceInfo->nCamType == MVID_GIGE_CAM)
        {
            nIp1 = ((pDeviceInfo->nCurrentIp & 0xff000000) >> 24);
            nIp2 = ((pDeviceInfo->nCurrentIp & 0x00ff0000) >> 16);
            nIp3 = ((pDeviceInfo->nCurrentIp & 0x0000ff00) >> 8);
            nIp4 = (pDeviceInfo->nCurrentIp & 0x000000ff);

            wchar_t* pUserName = NULL;
            if (strcmp("", (LPCSTR)(pDeviceInfo->chUserDefinedName)) != 0)
            {
                DWORD dwLenUserName = MultiByteToWideChar(CP_ACP, 0, (LPCSTR)(pDeviceInfo->chUserDefinedName), -1, NULL, 0);
                pUserName = new wchar_t[dwLenUserName];
                MultiByteToWideChar(CP_ACP, 0, (LPCSTR)(pDeviceInfo->chUserDefinedName), -1, pUserName, dwLenUserName);
            }
            else
            {
                char strUserName[256] = {0};
                sprintf_s(strUserName, "%s %s (%s)", pDeviceInfo->chManufacturerName,
                    pDeviceInfo->chModelName,
                    pDeviceInfo->chSerialNumber);
                DWORD dwLenUserName = MultiByteToWideChar(CP_ACP, 0, (LPCSTR)(strUserName), -1, NULL, 0);
                pUserName = new wchar_t[dwLenUserName];
                MultiByteToWideChar(CP_ACP, 0, (LPCSTR)(strUserName), -1, pUserName, dwLenUserName);
            }

            strMsg.Format(_T("[%d]GigE:    %s  (%d.%d.%d.%d)"), i, pUserName, nIp1, nIp2, nIp3, nIp4);
            if (NULL != pUserName)
            {
                delete[] pUserName;
                pUserName = NULL;
            }
        }
        else if (pDeviceInfo->nCamType == MVID_USB_CAM)
        {
            wchar_t* pUserName = NULL;
            if (strcmp("", (char*)pDeviceInfo->chUserDefinedName) != 0)
            {
                DWORD dwLenUserName = MultiByteToWideChar(CP_ACP, 0, (LPCSTR)(pDeviceInfo->chUserDefinedName), -1, NULL, 0);
                pUserName = new wchar_t[dwLenUserName];
                MultiByteToWideChar(CP_ACP, 0, (LPCSTR)(pDeviceInfo->chUserDefinedName), -1, pUserName, dwLenUserName);
            }
            else
            {
                char strUserName[256];
                sprintf_s(strUserName, "%s %s (%s)", pDeviceInfo->chManufacturerName,
                    pDeviceInfo->chModelName,
                    pDeviceInfo->chSerialNumber);
                DWORD dwLenUserName = MultiByteToWideChar(CP_ACP, 0, (LPCSTR)(strUserName), -1, NULL, 0);
                pUserName = new wchar_t[dwLenUserName];
                MultiByteToWideChar(CP_ACP, 0, (LPCSTR)(strUserName), -1, pUserName, dwLenUserName);
            }
            strMsg.Format(_T("[%d]UsbV3:  %s"), i, pUserName);
            if (NULL != pUserName)
            {
                delete[] pUserName;
                pUserName = NULL;
            }
        }
        else
        {
            ShowErrorMsg(TEXT("Unknown device enumerated"), 0);
            continue;
        }
        m_ctrlDeviceCombo.AddString(strMsg);
    }

    if (0 == m_pstDevList->nCamNum)
    {
        m_bIsEnum = FALSE;
        CString errorMsg = _T("");
        errorMsg.Format(_T("None Device"));
        MessageBox(errorMsg, TEXT("device"), MB_OK | MB_ICONWARNING);
        return;
    }

    m_bIsEnum = TRUE;
    m_ctrlDeviceCombo.SetCurSel(0);
    EnableControls();
    UpdateData(FALSE);
}

static  unsigned int __stdcall ProcessThread(void* pUser)
{
    CMVIDCodeReaderDemoDlg* pThis = (CMVIDCodeReaderDemoDlg*)pUser;
    if (pThis)
    {
        pThis->Process();
    }

    return 0;
}

void CMVIDCodeReaderDemoDlg::SetHScroll()
{
    CDC* dc = GetDC();
    CString str;

    try
    {
        for (int nIndex = 0; nIndex< m_ctrlListBoxResult.GetCount(); nIndex++)
        {
            m_ctrlListBoxResult.GetText(nIndex, str);

            SIZE szString = dc->GetTextExtent(str, str.GetLength() + 1);

            int temp = m_ctrlListBoxResult.GetHorizontalExtent();
            if (szString.cx > temp)
            {
                m_ctrlListBoxResult.SetHorizontalExtent(szString.cx);
            }
        }
    }
    catch (...)
    {
        return;
    }

    ReleaseDC(dc);
}

// 取图线程
void CMVIDCodeReaderDemoDlg::Process()
{
    int nRet = MVID_CR_OK;
    CString strMsg = _T("");
    memset(m_pstOutput, 0, sizeof(MVID_CAM_OUTPUT_INFO));

    while (m_bProcess)
    {
        nRet = MVID_CR_CAM_GetOneFrameTimeout(m_handle, m_pstOutput, 1000);
        if (0x0804 == m_nSystemLanguageId)
        {
            if (MVID_CR_OK == nRet)
            {
                //ch:输出结果 | en:Output results
                strMsg.Format(_T("已识别 %d 个对象："), m_pstOutput->stCodeList.nCodeNum);
                m_ctrlListBoxResult.AddString(strMsg);
                for (int i = 0; i < m_pstOutput->stCodeList.nCodeNum; ++i)
                {
                    wchar_t strWchar[MVID_MAX_CODECHARATERLEN] = {0};
                    Char2Wchar((char*)m_pstOutput->stCodeList.stCodeInfo[i].strCode, strWchar, MVID_MAX_CODECHARATERLEN);

                    strMsg.Format(_T("第%d个：[%s], 码类型[%d], 是否被过滤[%d], 帧号[%d]"), i+1,
                        strWchar,
                        (unsigned int)m_pstOutput->stCodeList.stCodeInfo[i].enBarType,
                        m_pstOutput->stCodeList.stCodeInfo[i].nFilterFlag,
                        m_pstOutput->stImage.nFrameNum);
                    m_ctrlListBoxResult.AddString(strMsg);
                }
                SetHScroll();
            }
            else
            {
                strMsg.Format(_T("识别失败，错误码 %x"), nRet);
                m_ctrlListBoxResult.AddString(strMsg);
            }
        }
        else
        {
            if (MVID_CR_OK == nRet)
            {
                //ch:输出结果 | en:Output results
                strMsg.Format(_T("%d objects have been identified："), m_pstOutput->stCodeList.nCodeNum);
                m_ctrlListBoxResult.AddString(strMsg);
                for (int i = 0; i < m_pstOutput->stCodeList.nCodeNum; ++i)
                {
                    wchar_t strWchar[MVID_MAX_CODECHARATERLEN] = {0};
                    Char2Wchar((char*)m_pstOutput->stCodeList.stCodeInfo[i].strCode, strWchar, MVID_MAX_CODECHARATERLEN);

                    strMsg.Format(_T("The number %d：[%s], Barcode Type [%d], Is it filtered ? [%d], Frame number [%d]"), i+1,
                        strWchar,
                        (unsigned int)m_pstOutput->stCodeList.stCodeInfo[i].enBarType,
                        m_pstOutput->stCodeList.stCodeInfo[i].nFilterFlag,
                        m_pstOutput->stImage.nFrameNum);
                    m_ctrlListBoxResult.AddString(strMsg);
                }
                SetHScroll();
            }
            else
            {
                strMsg.Format(_T("Recognition failure, error code: %x"), nRet);
                m_ctrlListBoxResult.AddString(strMsg);
            }
        }

        m_ctrlListBoxResult.SetTopIndex(m_ctrlListBoxResult.GetCount()-1);

        Display(m_hWndDisplay, m_pstOutput);

        m_nListNum = m_ctrlListBoxResult.GetCount();
        CheckListNum();
    }
}

// 开始读码
void CMVIDCodeReaderDemoDlg::OnBnClickedBtnStart()
{
    m_strImageFilePath = _T("");
    GetDlgItem(IDC_EDIT_ImagePath)->SetWindowText(m_strImageFilePath);
    UpdateData(TRUE);
    m_ctrlListBoxResult.ResetContent();

    int nIndex = m_nDeviceCombo;
    if ((nIndex < 0) | (nIndex >= MVID_MAX_CAM_NUM))
    {
        ShowErrorMsg(TEXT("Please select device"), 0);
        return ;
    }

    // ch:由设备信息创建设备实例 | en:Device instance created by device information
    if (NULL == m_pstDevList->pstCamInfo[nIndex])
    {
        ShowErrorMsg(TEXT("Device does not exist"), 0);
        return ;
    }

    if(m_handle)
    {
        MVID_CR_DestroyHandle(m_handle);
        m_handle = NULL;
        m_bIsLoadImg = FALSE;
    }

    // ch:仅一维码识别：MVID_BCR | en:Recognize barcode only：MVID_BCR
    // ch:仅二维码识别：MVID_TDCR | en:Recognize Two-Dimension code only: MVID_TDCR
    // ch:一维码 + 二维码 识别：MVID_BCR | MVID_TDCR | en:Recognize Barcode + Two-Dimension code: MVID_BCR | MVID_TDCR
    int nRet = MVID_CR_CreateHandle(&m_handle, MVID_BCR | MVID_TDCR);
    if (MVID_CR_OK != nRet)
    {
        ShowErrorMsg(TEXT("MVID_CR_CreateHandle "), nRet);
        return ;
    }

    nRet = MVID_CR_CAM_BindDevice(m_handle, m_pstDevList->pstCamInfo[nIndex]);
    if (MVID_CR_OK != nRet)
    {
        ShowErrorMsg(TEXT("MVID_CR_CAM_BindDevice "), nRet);
        return ;
    }

    OnBnClickedGetParameterButton();
    UpdatelListCtrl();

    nRet = MVID_CR_CAM_StartGrabbing(m_handle);
    if (MVID_CR_OK != nRet)
    {
        ShowErrorMsg(TEXT("MVID_CR_CAM_StartGrabbing "), nRet);
        return ;
    }

    m_bProcess = TRUE;
    unsigned int nThreadID = 0;
    m_hThread = (HANDLE*) _beginthreadex( NULL, 0, ProcessThread, this, 0 , &nThreadID );
    if (NULL == m_hThread)
    {
        ShowErrorMsg(TEXT("_beginthreadex fail"), 0);
        m_bProcess = FALSE;
        return ;
    }

    m_bIsInitRes = FALSE;
    m_bIsLoadImg = FALSE;
    EnableControls();
}


// ch:显示错误信息 | en:Show error message
void CMVIDCodeReaderDemoDlg::ShowErrorMsg(CString csMessage, int nErrorNum)
{
    CString errorMsg;
    if (nErrorNum == 0)
    {
        errorMsg.Format(_T("%s"), csMessage);
    }
    else
    {
        errorMsg.Format(_T("%s: Error = %x: "), csMessage, nErrorNum);
    }

    switch(nErrorNum)
    {
    case MVID_CR_E_ENCRYPT:         errorMsg += "encrypt error ";                                                   break;
    case MVID_CR_E_HANDLE:          errorMsg += "Error or invalid handle ";                                         break;
    case MVID_CR_E_SUPPORT:         errorMsg += "Not supported function ";                                          break;
    case MVID_CR_E_BUFOVER:         errorMsg += "Cache is full ";                                                   break;
    case MVID_CR_E_CALLORDER:       errorMsg += "Function calling order error ";                                    break;
    case MVID_CR_E_PARAMETER:       errorMsg += "Incorrect parameter ";                                             break;
    case MVID_CR_E_RESOURCE:        errorMsg += "Applying resource failed ";                                        break;
    case MVID_CR_E_NODATA:          errorMsg += "No data ";                                                         break;
    case MVID_CR_E_PRECONDITION:    errorMsg += "Precondition error, or running environment changed ";              break;
    case MVID_CR_E_ACCESS_DENIED:   errorMsg += "No permission ";                                                   break;
    case MVID_CR_E_BUSY:            errorMsg += "Device is busy, or network disconnected ";                         break;
    case MVID_CR_E_NETER:           errorMsg += "Network error ";                                                   break;
    case MVID_CR_E_UNKNOW:          errorMsg += "Unknown error ";                                                   break;
    }

    MessageBox(errorMsg, TEXT("PROMPT"), MB_OK | MB_ICONWARNING);
}

// 停止读码
void CMVIDCodeReaderDemoDlg::OnBnClickedBtnStop()
{
    CString strMsg = _T("");
    m_bProcess = FALSE;
    if (m_hThread)
    {
        MsgWaitForMultipleObjects(1, m_hThread, FALSE, INFINITE, QS_ALLINPUT);
        CloseHandle(m_hThread);
        m_hThread = NULL;
    }

    if(m_handle)
    {
        int nRet = MVID_CR_CAM_StopGrabbing(m_handle);
        if (MVID_CR_OK == nRet)
        {
            strMsg.Format(_T("stop grabbing success"));
            MessageBox(strMsg);
        }
        else
        {
            ShowErrorMsg(TEXT("stop grabbing failed "), nRet);
        }

        nRet = MVID_CR_DestroyHandle(m_handle);
        if (MVID_CR_OK != nRet)
        {
            ShowErrorMsg(TEXT("destroy handle failed "), nRet);
        }
        m_handle = NULL;
    }

    m_list.DeleteAllItems();
    this->RedrawWindow();

    EnableControls();
    UpdateData(FALSE);
}

// ch:读取BMP图像文件 | en:Read the BMP image file
void CMVIDCodeReaderDemoDlg::ReadBmp(CString pchBmpName, BmpFile* pstBmpFile)
{
    CString strMsg = _T("");
    //ch:二进制读方式打开指定的图像文件 | en:Opem specific image file by reading the binary data
    HANDLE hFile=CreateFile(pchBmpName,GENERIC_READ,FILE_SHARE_READ,NULL,OPEN_EXISTING,FILE_ATTRIBUTE_NORMAL,NULL);
    if (hFile == INVALID_HANDLE_VALUE)
    {
        strMsg.Format(_T("fopen error"));
        MessageBox(strMsg);
        return ;
    }

    DWORD dwRead = 0;
    ReadFile(hFile,&(pstBmpFile->stBmpFileHeader), sizeof(BITMAPFILEHEADER),&dwRead,NULL);
    ReadFile(hFile,&(pstBmpFile->stBmpInfoHeader), sizeof(BITMAPINFOHEADER),&dwRead,NULL);

    CloseHandle(hFile);

    UpdateData(FALSE);
}

// 加载本地图片
void CMVIDCodeReaderDemoDlg::OnBnClickedBtnLoad()
{
    if (m_bProcess)
    {
        MessageBox(_T("please stop camera barcode identify first"));
        return;
    }
    this->RedrawWindow();

    CFileDialog stFileDlg(TRUE, NULL, NULL, OFN_HIDEREADONLY, TEXT("All Files(*.*)|*.*|raw Files(*.raw)|*.raw;*.RAW|Bmp Files(*.bmp)|*.bmp;*.BMP|Jpeg Files(*.jpg;*.jpeg)|*.jpg;*.jpeg||"), this);
    if (IDOK != stFileDlg.DoModal())
    {
        return;
    }

    m_strImageFilePath = stFileDlg.GetPathName();
    char chImageFilePath[256] = {0};
    wchar_t* pBuffer = m_strImageFilePath.GetBuffer();
    WideCharToMultiByte(CP_OEMCP, NULL, pBuffer, -1, chImageFilePath, 256, NULL, FALSE);
    m_strImageFilePath.ReleaseBuffer();
    memset(m_pstProcParam, 0, sizeof(MVID_PROC_PARAM));

    // ch:获取文件后缀名 | en:Get file suffix name
    if (".raw" == (CString)strrchr(chImageFilePath,'.') || ".RAW" == (CString)strrchr(chImageFilePath, '.'))
    {
        m_DemoRawDlg.DoModal(); 

        m_pstProcParam->nWidth  = m_DemoRawDlg.m_nImageWidthEdit;
        m_pstProcParam->nHeight = m_DemoRawDlg.m_nImageHeightEdit;

        switch (m_DemoRawDlg.m_TypeCombo) 
        { 
        case 0:
            m_pstProcParam->enImageType = MVID_IMAGE_MONO8;
            break;
        case 1:
            m_pstProcParam->enImageType = MVID_IMAGE_BGR24;
            break;
        case 2:
            m_pstProcParam->enImageType = MVID_IMAGE_BayerGB10;
            break;
        case 3:
            m_pstProcParam->enImageType = MVID_IMAGE_YUV422_Packed;
            break;
        default:
            return;
        }
    }

    int nRet = MVID_CR_GetImageFileData(m_handle, chImageFilePath, m_pstProcParam);
    if (MVID_CR_OK == nRet)
    {
        m_bIsLoadImg = TRUE;
        EnableControls();
        UpdateData(FALSE);
    }
    else
    {
        CString strMsg = _T("");
        strMsg.Format(_T("Get image file data failed, nRet = %#x"), nRet);
        MessageBox(strMsg);
    }
}

// 本地图片识图读码
void CMVIDCodeReaderDemoDlg::OnBnClickedBtnProc()
{
    CString strMsg = _T("");

    if (m_bProcess)
    {
        strMsg.Format(_T("please stop camera barcode identify first"));
        MessageBox(strMsg);
        return ;
    }

    m_pstProcParam->stCodeList.nCodeNum = 0;
    memset(&m_pstProcParam->stCodeList, 0, sizeof(MVID_CODE_INFO_LIST));

    int nRet = MVID_CR_Process(m_handle, m_pstProcParam, MVID_BCR | MVID_TDCR);
    if (0x0804 == m_nSystemLanguageId)
    {
        if (MVID_CR_OK == nRet)
        {
            //ch:输出结果 | en:Output results
            strMsg.Format(_T("已识别 %d 个对象："), m_pstProcParam->stCodeList.nCodeNum);
            m_ctrlListBoxResult.AddString(strMsg);
            for (int i = 0; i < m_pstProcParam->stCodeList.nCodeNum; ++i)
            {
                wchar_t strWchar[MVID_MAX_CODECHARATERLEN] = {0};
                Char2Wchar((char*)m_pstProcParam->stCodeList.stCodeInfo[i].strCode, strWchar, MVID_MAX_CODECHARATERLEN);

                strMsg.Format(_T("第%d个：[%s], 码类型[%d], 是否被过滤[%d]"), i+1,
                    strWchar,
                    (unsigned int)m_pstProcParam->stCodeList.stCodeInfo[i].enBarType,
                    m_pstProcParam->stCodeList.stCodeInfo[i].nFilterFlag);
                m_ctrlListBoxResult.AddString(strMsg);
            }
            SetHScroll();
        }
        else
        {
            strMsg.Format(_T("识别失败，错误码 %x"), nRet);
            m_ctrlListBoxResult.AddString(strMsg);
        }
    }
    else
    {
        if (MVID_CR_OK == nRet)
        {
            //ch:输出结果 | en:Output results
            strMsg.Format(_T("%d objects have been identified："), m_pstProcParam->stCodeList.nCodeNum);
            m_ctrlListBoxResult.AddString(strMsg);
            for (int i = 0; i < m_pstProcParam->stCodeList.nCodeNum; ++i)
            {
                wchar_t strWchar[MVID_MAX_CODECHARATERLEN] = {0};
                Char2Wchar((char*)m_pstProcParam->stCodeList.stCodeInfo[i].strCode, strWchar, MVID_MAX_CODECHARATERLEN);

                strMsg.Format(_T("The number %d：[%s], Barcode Type [%d], Is it filtered ? [%d]"), i+1,
                    strWchar,
                    (unsigned int)m_pstProcParam->stCodeList.stCodeInfo[i].enBarType,
                    m_pstProcParam->stCodeList.stCodeInfo[i].nFilterFlag);
                m_ctrlListBoxResult.AddString(strMsg);
            }
            SetHScroll();
        }
        else
        {
            strMsg.Format(_T("Recognition failure, error code: %x"), nRet);
            m_ctrlListBoxResult.AddString(strMsg);
        }   
    }

    m_ctrlListBoxResult.SetTopIndex(m_ctrlListBoxResult.GetCount()-1);

    memset(m_pstOutput, 0, sizeof(MVID_CAM_OUTPUT_INFO));
    m_pstOutput->stImage.pImageBuf = m_pstProcParam->pImageBuf;
    m_pstOutput->stImage.enImageType = m_pstProcParam->enImageType;
    m_pstOutput->stImage.nWidth = m_pstProcParam->nWidth;
    m_pstOutput->stImage.nHeight = m_pstProcParam->nHeight;
    m_pstOutput->stImage.nImageLen = m_pstProcParam->nImageLen;
    memcpy(&m_pstOutput->stCodeList, &m_pstProcParam->stCodeList, sizeof(MVID_CODE_INFO_LIST));

    Display(m_hWndDisplay, m_pstOutput);

    m_nListNum = m_ctrlListBoxResult.GetCount();
    CheckListNum();

    EnableControls();
    UpdateData(FALSE);
}

// 初始化算法参数列表
void CMVIDCodeReaderDemoDlg::InitListCtrl()
{
    RECT  m_rect;
    m_list.GetClientRect(&m_rect);                                                  //ch:获取list的客户区,方便调节每一列的宽度 | en:Get the list frame for adjust the width of each column  
    m_list.SetExtendedStyle(LVS_EX_GRIDLINES | LVS_EX_FULLROWSELECT);               //ch:设置list风格，LVS_EX_GRIDLINES为网格线（只适用与report风格的listctrl）| en: Set the list style (only "report" style is supported), the LVS_EX_GRIDLINES indicates the grid line
    m_list.ModifyStyle(0, LVS_REPORT);

    //ch:LVS_EX_FULLROWSELECT为选中某行使整行高亮（只适用与report风格的listctrl）| en:The function of LVS_EX_FULLROWSELECT is to select and highlight a line (it is only supported by "report" style)
    m_list.InsertColumn(0, _T("ParamName"), LVCFMT_LEFT, m_rect.right * 0.5);
    m_list.InsertColumn(1, _T("ParamValue"), LVCFMT_LEFT, m_rect.right * 0.5);

    m_Edit.Create(ES_AUTOHSCROLL | WS_CHILD | ES_LEFT | ES_WANTRETURN, CRect(0, 0, 0, 0), this, IDC_EDIT_CREATEID);//ch:创建编辑框对象,IDC_EDIT_CREATEID为控件ID号3000，在文章开头定义 | en:Create edit box, the IDC_EDIT_CREATEID is the control ID (3000, which is defined at the header)
    m_Edit.SetFont(this->GetFont(), FALSE);                                         //ch:设置字体,不设置这里的话上面的字会很突兀的感觉 | en:Set the font
    m_Edit.SetParent(&m_list);                                                      //ch:将list control设置为父窗口,生成的Edit才能正确定位,这个也很重要 | en:Set the list control as the parent window for accurately locating the edit box 
    m_Edit.ShowWindow(SW_HIDE);                                                     //ch:隐藏编辑框 | en: Hide the edit box

    m_comBox.Create(WS_CHILD | WS_VISIBLE |  CBS_DROPDOWN | CBS_OEMCONVERT, CRect(0, 0, 0, 0), this, IDC_COMBOX_CREATEID);
    m_comBox.SetFont(this->GetFont(), FALSE);                                       //ch:设置字体,不设置这里的话上面的字会很突兀的感觉 | en:Set the font
    m_comBox.SetParent(&m_list);                                                    //ch:将list control设置为父窗口,生成的Edit才能正确定位,这个也很重要 | en:Set the list control as the parent window for accurately locating the edit box
    m_comBox.ShowWindow(SW_HIDE);                                                   //ch:隐藏编辑框 | en: Hide the edit box
}


void CMVIDCodeReaderDemoDlg::OnNMClickListParam(NMHDR *pNMHDR, LRESULT *pResult)
{
    LPNMITEMACTIVATE pNMItemActivate = reinterpret_cast<LPNMITEMACTIVATE>(pNMHDR);

    NM_LISTVIEW  *pEditCtrl = (NM_LISTVIEW *)pNMHDR;
    //PrintMessage("行：%d，列：%d\n", pEditCtrl->iItem, pEditCtrl->iSubItem);

    if (-1 != pEditCtrl->iItem && 1 == pEditCtrl->iSubItem) // ch:参数值选项 | en:Options of parameter values
    {
        CString strKeyName = m_list.GetItemText(pEditCtrl->iItem, 0);
        ShowEdit(pEditCtrl);
    }
    *pResult = 0;
}


void CMVIDCodeReaderDemoDlg::ShowEdit( NM_LISTVIEW *pEditCtrl )
{
    m_nItem = pEditCtrl->iItem;
    m_nSubItem = pEditCtrl->iSubItem;
    CRect  EditRect;
    m_list.GetSubItemRect(m_nItem, m_nSubItem, LVIR_LABEL, EditRect);   //ch:获取单元格的空间位置信息 | en:Get the position information of cells
    m_Edit.MoveWindow(&EditRect);                                       //ch:将编辑框位置放在相应单元格上 | en:Move the edit box to the corresponding cell
    m_Edit.SetFocus();                                                  //ch:设置为焦点 | en:Set it is the focus
    CString strItem = m_list.GetItemText(m_nItem, m_nSubItem);          //ch:获得相应单元格字符 | en:Get the character of the corresponding cell
    m_Edit.SetWindowText(strItem);                                      //ch:将单元格字符显示在编辑框上 | en:Display the character of the cell on the ecit box
    m_Edit.SetSel(0, -1);                                               //ch:设置光标在文本框文字的最后 | en:Move the cursor to the end of the last text in the text box
    m_Edit.ShowWindow(SW_SHOW);                                         //ch:显示编辑框在单元格上面 | en:Display the edit box over the cell
}

// 设置算法参数
void CMVIDCodeReaderDemoDlg::OnKillfocusEdit()
{
    CString strValue;
    m_Edit.GetWindowText(strValue);//ch:获得相应单元格字符 | en:Get the character of the corresponding cell
    CString strKeyName = m_list.GetItemText(m_nItem, 0);

    wchar_t* chwKeyName = strKeyName.GetBuffer();
    char  chKeyName[256] = {0};
    WideCharToMultiByte(CP_OEMCP, NULL, chwKeyName, -1, chKeyName, 256, NULL, FALSE);

    int nRet = MVID_CR_Algorithm_SetIntValue(m_handle, chKeyName, _ttoi(strValue));
    if (MVID_CR_OK != nRet)
    {
        CString strMsg;
        wchar_t strWchar[256] = {0};
        MultiByteToWideChar(CP_ACP, 0, (LPCSTR)(chKeyName), -1, strWchar, 256);
        strMsg.Format(_T("set (%s) failed (%#x)"), strWchar, nRet);
        MessageBox(strMsg);
        return;
    }
    m_list.SetItemText(m_nItem, m_nSubItem, strValue);
    m_Edit.ShowWindow(SW_HIDE);
}

// 获取算法参数
void CMVIDCodeReaderDemoDlg::UpdatelListCtrl()
{
    m_list.DeleteAllItems();
    int nRow = 0;
    CString strValue;

    std::map<CString, int>::iterator it = m_mapParamList.begin();
    for (; it != m_mapParamList.end(); ++it, ++nRow)
    {
        CString strFirstValue = it->first;
        wchar_t* chwKeyName = strFirstValue.GetBuffer();
        char  chKeyName[256] = {0};
        WideCharToMultiByte(CP_OEMCP, NULL, chwKeyName, -1, chKeyName, 256, NULL, FALSE);

        MVID_CR_Algorithm_GetIntValue(m_handle, chKeyName, &(it->second));

        m_list.InsertItem(nRow, it->first);

        strValue.Format(_T("%d"), it->second);
        m_list.SetItemText(nRow, 1, strValue);
    }

}

BOOL CMVIDCodeReaderDemoDlg::PreTranslateMessage(MSG* pMsg)
{
    CString cstrMessageOut;
    if (WM_KEYDOWN == pMsg->message) 
    {
        switch(pMsg->wParam)
        {
        case VK_ESCAPE:
            {
                OnOK();
            }
            break;
        case VK_RETURN:
            {
                UpdateData(TRUE);
                switch (GetFocus()->GetDlgCtrlID())
                {
                case IDC_EDIT_CREATEID: 
                case IDC_COMBOX_CREATEID:
                    {
                        m_list.SetFocus();
                    }
                    break;
                default:
                    {

                    }
                    break;
                }
                return TRUE;
            }
        default:
            break;
        }
    }

    return CDialog::PreTranslateMessage(pMsg);
}

// 初始化图片资源
void CMVIDCodeReaderDemoDlg::OnBnClickedBtnInit()
{
    CString strMsg = _T("");

    if (m_bProcess)
    {
        strMsg.Format(_T("please stop camera barcode identify first"));
        MessageBox(strMsg);
        return ;
    }

    if (m_handle)
    {
        MVID_CR_DestroyHandle(m_handle);
        m_handle = NULL;
    }

    char chSDKVersion[256] = {0};
    int nRet = MVID_CR_GetVersion(chSDKVersion);
    if (MVID_CR_OK != nRet)
    {
        ShowErrorMsg(TEXT("MVID_CR_GetVersion "), nRet);
        return ;
    }

    wchar_t strWchar[256] = {0};
    MultiByteToWideChar(CP_ACP, 0, (LPCSTR)(chSDKVersion), -1, strWchar, 256);
    m_ctrlListBoxResult.AddString(strWchar);
    m_ctrlListBoxResult.SetTopIndex(m_ctrlListBoxResult.GetCount()-1);

    // ch:仅一维码识别：MVID_BCR | en:Recognize barcode only: MVID_BCR
    // ch:仅二维码识别：MVID_TDCR | en:Recognize Two-Dimension code only: MVID_TDCR
    // ch:仅面单识别：MVID_WAYBILL | en:Recognize waybill only: MVID_WAYBILL
    // ch:一维码 + 二维码 + 面单 识别：MVID_BCR | MVID_TDCR | MVID_WAYBILL | en:Recognize Barcode + Two-Dimension code + waybill: MVID_BCR | MVID_TDCR | MVID_WAYBILL
    nRet = MVID_CR_CreateHandle(&m_handle, MVID_BCR | MVID_TDCR | MVID_WAYBILL);
    if (MVID_CR_OK != nRet)
    {
        ShowErrorMsg(TEXT("MVID_CR_CreateHandle "), nRet);
        return ;
    }

    m_bIsInitRes = TRUE;
    m_bIsLoadImg = FALSE;
    EnableControls();
    UpdatelListCtrl();
}

// 清空消息
void CMVIDCodeReaderDemoDlg::OnBnClickedBtnClear()
{
    m_ctrlListBoxResult.ResetContent();
}

void CMVIDCodeReaderDemoDlg::OnClose()
{
    m_bProcess = FALSE;
    if (m_hThread)
    {
        MsgWaitForMultipleObjects(1, m_hThread, FALSE, INFINITE, QS_ALLINPUT);
        CloseHandle(m_hThread);
        m_hThread = NULL;
    }

    if(m_handle)
    {
        MVID_CR_DestroyHandle(m_handle);
        m_handle = NULL;
    }

    if (m_pstDevList)
    {
        free(m_pstDevList);
        m_pstDevList = NULL;
    }

    if (m_pstProcParam)
    {
        free(m_pstProcParam);
        m_pstProcParam = NULL;
    }

    if (m_pstOutput)
    {
        free(m_pstOutput);
        m_pstOutput = NULL;
    }

    if (m_stBmpFile.pBuf)
    {
        free(m_stBmpFile.pBuf);
        m_stBmpFile.pBuf = NULL;
    }

    if (m_bBitmapInfo)
    {
        free(m_bBitmapInfo);
        m_bBitmapInfo = NULL;
    }

    CDialog::OnClose();
}

// 面单抠图
void CMVIDCodeReaderDemoDlg::OnBnClickedBtnCutout()
{
    CString strMsg;
    int nImageType = MVID_IMAGE_JPEG;

    if (m_bProcess)
    {
        strMsg.Format(_T("please stop camera barcode identify first"));
        MessageBox(strMsg);
        return ;
    }

    // ch:必须先经过算法处理且转换为Mono8才能抠图 | en:The image matting must be done after processing it by algorithm and transforming it to Mono8
    int nRet = MVID_CR_Process(m_handle, m_pstProcParam, MVID_BCR | MVID_WAYBILL );
    if (MVID_CR_OK == nRet)
    {
        // ch:未识别到条码时不进行抠图 | en:Do not perform image matting if there is no barcode is recognied图
        if (0 == m_pstProcParam->nImageWaybillLen || NULL == m_pstProcParam->pImageWaybill)
        {
            strMsg.Format(_T("identify no barcode, waybill image is null"));
            MessageBox(strMsg);
            return;
        }

        // ch:保存图像 | en:Save the imag
        FILE* pfile;
        char filename[256] = {0};
        CTime currTime;                      // ch:获取系统时间作为保存图片文件名 | en:Get the system time as the name of saved picture file
        currTime = CTime::GetCurrentTime(); 

        nRet = MVID_CR_Algorithm_GetIntValue(m_handle, KEY_WAYBILL_OUTPUTIMAGETYPE, &nImageType);
        if (MVID_CR_OK != nRet)
        {
            ShowErrorMsg(TEXT("MVID_CR_Algorithm_GetIntValue "), nRet);
            return ;
        }

        if (MVID_IMAGE_MONO8 == nImageType)
        {
            sprintf(filename,("%.4d%.2d%.2d%.2d%.2d%.2d.raw"), currTime.GetYear(), currTime.GetMonth(),
                currTime.GetDay(), currTime.GetHour(), currTime.GetMinute(), currTime.GetSecond());
        }
        else if (MVID_IMAGE_BMP == nImageType)
        {
            sprintf(filename,("%.4d%.2d%.2d%.2d%.2d%.2d.bmp"), currTime.GetYear(), currTime.GetMonth(),
                currTime.GetDay(), currTime.GetHour(), currTime.GetMinute(), currTime.GetSecond());
        }
        else
        {
            sprintf(filename,("%.4d%.2d%.2d%.2d%.2d%.2d.jpg"), currTime.GetYear(), currTime.GetMonth(),
                currTime.GetDay(), currTime.GetHour(), currTime.GetMinute(), currTime.GetSecond());
        }

        pfile = fopen(filename,"wb");
        if (pfile == NULL)
        {
            strMsg.Format(_T("failed open file"));
            MessageBox(strMsg);
            return ;
        }
        fwrite(m_pstProcParam->pImageWaybill, 1, m_pstProcParam->nImageWaybillLen, pfile);
        strMsg.Format(_T("success save cutout image"));
        MessageBox(strMsg);
        fclose (pfile);
        pfile = NULL;
    }

    EnableControls();
}

// 显示图片及识图结果
void CMVIDCodeReaderDemoDlg::Display(void* hWnd, MVID_CAM_OUTPUT_INFO* pstDisplayImage)
{
    if (pstDisplayImage->stImage.nWidth == 0 || pstDisplayImage->stImage.nHeight == 0 || NULL == hWnd)
    {
        return;
    }

    if (NULL == m_bBitmapInfo)
    {
        m_bBitmapInfo = (PBITMAPINFO)malloc(sizeof(BITMAPINFO) + 256 * sizeof(RGBQUAD));
        if (NULL == m_bBitmapInfo)
        {
            return;
        }
        memset(m_bBitmapInfo, 0, sizeof(BITMAPINFO) + 256 * sizeof(RGBQUAD));
    }

    int nRet = MVID_CR_OK;
    int nImageWidth = pstDisplayImage->stImage.nWidth;
    int nImageHeight = pstDisplayImage->stImage.nHeight;

    // ch:显示图像 | en:Display image
    HDC hDC  = ::GetDC((HWND)hWnd);
    SetStretchBltMode(hDC, COLORONCOLOR);
    RECT wndRect;
    ::GetClientRect((HWND)hWnd, &wndRect);

    int nWndRectWidth  = wndRect.right  - wndRect.left;
    int nWndRectHeight = wndRect.bottom - wndRect.top;

    int nDstX      = wndRect.left;
    int nDstY      = wndRect.top;
    int nDstWidth  = (int)(nWndRectWidth);
    int nDstHeight = (int)(nWndRectHeight);

    int nSrcX      = 0;
    int nSrcY      = 0;
    int nSrcWidth  = (int)(nImageWidth);
    int nSrcHeight = (int)(nImageHeight);

    switch(pstDisplayImage->stImage.enImageType)
    {
    case MVID_IMAGE_MONO8:
        {
            m_bBitmapInfo->bmiHeader.biBitCount = 8;

            for(int i = 0; i < 256; i++)
            {
                m_bBitmapInfo->bmiColors[i].rgbBlue = m_bBitmapInfo->bmiColors[i].rgbRed = m_bBitmapInfo->bmiColors[i].rgbGreen = i;
                m_bBitmapInfo->bmiColors[i].rgbReserved = 0;
            }
        }
        break;
    case MVID_IMAGE_BMP:
        {
            if (0 == m_stBmpFile.stBmpInfoHeader.biBitCount || 8 == m_stBmpFile.stBmpInfoHeader.biBitCount)
            {
                m_bBitmapInfo->bmiHeader.biBitCount = 8;

                for(int i = 0; i < 256; i++)
                {
                    m_bBitmapInfo->bmiColors[i].rgbBlue = m_bBitmapInfo->bmiColors[i].rgbRed = m_bBitmapInfo->bmiColors[i].rgbGreen = i;
                    m_bBitmapInfo->bmiColors[i].rgbReserved = 0;
                }
            }
            else if (24 == m_stBmpFile.stBmpInfoHeader.biBitCount)
            {
                m_bBitmapInfo->bmiHeader.biBitCount = 24;
            }
            else
            {
                return;
            }
        }
        break;
    case MVID_IMAGE_BGR24:
        {
            m_bBitmapInfo->bmiHeader.biBitCount = 24;
        }
        break;
    default:
        break;
    }

    // ch:位图信息头 | en:header of bitmap information
    m_bBitmapInfo->bmiHeader.biSize = sizeof(BITMAPINFOHEADER);             // ch:BITMAPINFOHEADER结构长度 | en:Size of structure BITMAPINFOHEADER
    m_bBitmapInfo->bmiHeader.biWidth = nImageWidth;                         // ch:图像宽度 | en:Image width
    m_bBitmapInfo->bmiHeader.biPlanes = 1;                                  // ch:位面数 | en:Number of bit planes
    m_bBitmapInfo->bmiHeader.biCompression = BI_RGB;                        // ch:图像数据压缩类型,BI_RGB表示不压缩 | en:Encoding type of image data, BI_RGB-nor encode
    m_bBitmapInfo->bmiHeader.biSizeImage = nImageWidth * nImageHeight;      // ch:图像大小 | en:Image size

    if (MVID_IMAGE_BMP == pstDisplayImage->stImage.enImageType)
    {
        int nOffset = 0;
        if (8 == m_bBitmapInfo->bmiHeader.biBitCount)
        {
            nOffset = sizeof(RGBQUAD) * 256 + sizeof(BITMAPFILEHEADER) + sizeof(BITMAPINFOHEADER);
        }
        else if (24 == m_bBitmapInfo->bmiHeader.biBitCount)
        {
            nOffset = sizeof(BITMAPFILEHEADER) + sizeof(BITMAPINFOHEADER);
        }
        else
        {
            return;
        }

        if (0 > m_stBmpFile.stBmpInfoHeader.biHeight)
        {
            m_bBitmapInfo->bmiHeader.biHeight = - nImageHeight;                      // ch:图像高度 |en:Image height
        }
        else
        {
            m_bBitmapInfo->bmiHeader.biHeight = nImageHeight;                       // ch:图像高度 |en:Image height
        }

        nRet = StretchDIBits(hDC,
            nDstX, nDstY, nDstWidth, nDstHeight,
            nSrcX, nSrcY, nSrcWidth, nSrcHeight, pstDisplayImage->stImage.pImageBuf + nOffset, m_bBitmapInfo, DIB_RGB_COLORS, SRCCOPY);
    }
    else if (MVID_IMAGE_JPEG == pstDisplayImage->stImage.enImageType)
    {
        //读取图片
        CImage CbmpImage;
        CbmpImage.Load(m_strImageFilePath);
        CbmpImage.Draw(hDC, wndRect);
    }
    else
    {
        m_bBitmapInfo->bmiHeader.biHeight = - nImageHeight;                       // ch:图像高度 |en:Image height
        nRet = StretchDIBits(hDC,
            nDstX, nDstY, nDstWidth, nDstHeight,
            nSrcX, nSrcY, nSrcWidth, nSrcHeight, pstDisplayImage->stImage.pImageBuf, m_bBitmapInfo, DIB_RGB_COLORS, SRCCOPY);
    }

    // 画框
    Graphics gCode((HWND)hWnd);

    Status nGdiStatus = Status::Ok;
    Pen pen(Color(0, 0, 255), 3);

    float fWidthProportion = (float)nWndRectWidth / nImageWidth;
    float fHeightProportion = (float)nWndRectHeight / nImageHeight;

    for (int i = 0; i < pstDisplayImage->stCodeList.nCodeNum; i++)
    {
        PointF point1((pstDisplayImage->stCodeList.stCodeInfo[i].stCornerPt[0].nX * fWidthProportion), (pstDisplayImage->stCodeList.stCodeInfo[i].stCornerPt[0].nY * fHeightProportion));
        PointF point2((pstDisplayImage->stCodeList.stCodeInfo[i].stCornerPt[1].nX * fWidthProportion), (pstDisplayImage->stCodeList.stCodeInfo[i].stCornerPt[1].nY * fHeightProportion));
        PointF point3((pstDisplayImage->stCodeList.stCodeInfo[i].stCornerPt[2].nX * fWidthProportion), (pstDisplayImage->stCodeList.stCodeInfo[i].stCornerPt[2].nY * fHeightProportion));
        PointF point4((pstDisplayImage->stCodeList.stCodeInfo[i].stCornerPt[3].nX * fWidthProportion), (pstDisplayImage->stCodeList.stCodeInfo[i].stCornerPt[3].nY * fHeightProportion));
        PointF points[4] = {point1, point2, point3, point4};
        PointF* pPoints = points;
        gCode.DrawPolygon(&pen, pPoints, 4);
    }

    ::ReleaseDC((HWND)hWnd, hDC);

    return;
}

// 相机参数获取
void CMVIDCodeReaderDemoDlg::OnBnClickedGetParameterButton()
{
    int nRet = MVID_CR_OK;
    CString strMsg;

    // ch:获取曝光时间 | en:Get exposure time
    MVID_CAM_FLOATVALUE stFloatValue = {0};
    nRet = MVID_CR_CAM_GetFloatValue(m_handle, "ExposureTime", &stFloatValue);
    if (MVID_CR_OK != nRet)
    {
        strMsg.Format(_T("failed get ExposureTime %#x"), nRet);
        MessageBox(strMsg);
        return ;
    }
    strMsg.Format(_T("%0.2f"), stFloatValue.fCurValue);
    m_ctrlExposureEdit.SetWindowText(strMsg);

    // ch:获取增益 | en: Get gain
    nRet = MVID_CR_CAM_GetFloatValue(m_handle, "Gain", &stFloatValue);
    if (MVID_CR_OK != nRet)
    {
        strMsg.Format(_T("failed get Gain %#x"), nRet);
        MessageBox(strMsg);
        return ;
    }
    strMsg.Format(_T("%0.2f"), stFloatValue.fCurValue);
    m_ctrlGainEdit.SetWindowText(strMsg);

    // ch:获取帧率 | en:Get frame rate
    nRet = MVID_CR_CAM_GetFloatValue(m_handle, "AcquisitionFrameRate", &stFloatValue);
    if (MVID_CR_OK != nRet)
    {
        strMsg.Format(_T("failed get AcquisitionFrameRate %#x"), nRet);
        MessageBox(strMsg);
        return ;
    }
    strMsg.Format(_T("%0.2f"), stFloatValue.fCurValue);
    m_ctrlFrameRateEdit.SetWindowText(strMsg);
}

// 设置相机参数
void CMVIDCodeReaderDemoDlg::OnBnClickedSetParameterButton()
{
    int nRet = MVID_CR_OK;
    CString strMsg;

    // ch:设置曝光时间 | en:Set exposure time
    float fExposureTime = 0.0f;
    m_ctrlExposureEdit.GetWindowText(strMsg);
    fExposureTime = atof(CStringA(strMsg));
    nRet = MVID_CR_CAM_SetFloatValue(m_handle, "ExposureTime", fExposureTime);
    if (MVID_CR_OK != nRet)
    {
        strMsg.Format(_T("failed set ExposureTime %#x"), nRet);
        MessageBox(strMsg);
        return ;
    }

    // ch:设置增益 | en: Set gain
    float fGain= 0.0f;
    m_ctrlGainEdit.GetWindowText(strMsg);
    fGain = atof(CStringA(strMsg));
    nRet = MVID_CR_CAM_SetFloatValue(m_handle, "Gain", fGain);
    if (MVID_CR_OK != nRet)
    {
        strMsg.Format(_T("failed set Gain %#x"), nRet);
        MessageBox(strMsg);
        return ;
    }

    // ch:设置帧率 | en:Set frame rate
    float fFrameRate= 0.0f;
    m_ctrlFrameRateEdit.GetWindowText(strMsg);
    fFrameRate = atof(CStringA(strMsg));
    nRet = MVID_CR_CAM_SetFloatValue(m_handle, "AcquisitionFrameRate", fFrameRate);
    if (MVID_CR_OK != nRet)
    {
        strMsg.Format(_T("failed set AcquisitionFrameRate %#x"), nRet);
        MessageBox(strMsg);
        return ;
    }
}

void CMVIDCodeReaderDemoDlg::CheckListNum()
{
    int nRet = MVID_CR_OK;

    // ch:对ListBox行数增加限制，避免内存一直上涨 | en:Number limit of ListBox lines, which avoids the continuous increase of memory
    if (100 < m_nListNum)
    {
        m_criSection.Lock();
        m_ctrlListBoxResult.ResetContent();
        m_criSection.Unlock();
    }

    return;
}
