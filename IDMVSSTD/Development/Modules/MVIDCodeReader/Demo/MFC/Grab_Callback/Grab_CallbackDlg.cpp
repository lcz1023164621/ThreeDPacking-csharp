#include "stdafx.h"
#include "Grab_Callback.h"
#include "Grab_CallbackDlg.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

#define BMP_BUF_SIZE    (5000*5000)

class CAboutDlg : public CDialog
{
public:
	CAboutDlg();

	enum { IDD = IDD_ABOUTBOX };

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV 支持

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

CGrab_CallbackDlg::CGrab_CallbackDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CGrab_CallbackDlg::IDD, pParent)
    , m_nDeviceCombo(0)
    , m_nListNum(0)
    , m_bProcess(FALSE)
    , m_handle(NULL)
    , m_pstBitmapInfo(NULL)
    , m_pstDevList(NULL)
    , m_nSystemLanguageId(0)
{
    m_hWndDisplay = NULL;
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);
}

void CGrab_CallbackDlg::DoDataExchange(CDataExchange* pDX)
{
    CDialog::DoDataExchange(pDX);
    DDX_Control(pDX, IDC_COMBO_CamList, m_ctrlDeviceCombo);
    DDX_CBIndex(pDX, IDC_COMBO_CamList, m_nDeviceCombo);
    DDX_Control(pDX, IDC_LIST_Result, m_ctrlListBoxResult);
}

BEGIN_MESSAGE_MAP(CGrab_CallbackDlg, CDialog)
	ON_WM_SYSCOMMAND()
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()

    ON_BN_CLICKED(IDC_BTN_Enum, &CGrab_CallbackDlg::OnBnClickedBtnEnum)
    ON_BN_CLICKED(IDC_BTN_Start, &CGrab_CallbackDlg::OnBnClickedBtnStart)
    ON_BN_CLICKED(IDC_BTN_Stop, &CGrab_CallbackDlg::OnBnClickedBtnStop)
    ON_BN_CLICKED(IDC_BTN_Clear, &CGrab_CallbackDlg::OnBnClickedBtnClear)
    ON_WM_CLOSE()
END_MESSAGE_MAP()



BOOL CGrab_CallbackDlg::OnInitDialog()
{
	CDialog::OnInitDialog();

    m_nSystemLanguageId = GetUserDefaultUILanguage();

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
    GetDlgItem(IDC_BTN_Start)->EnableWindow(TRUE);
    GetDlgItem(IDC_BTN_Stop)->EnableWindow(FALSE);

	return TRUE; 
}

void CGrab_CallbackDlg::OnSysCommand(UINT nID, LPARAM lParam)
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

void CGrab_CallbackDlg::OnPaint()
{
	if (IsIconic())
	{
		CPaintDC dc(this);

		SendMessage(WM_ICONERASEBKGND, reinterpret_cast<WPARAM>(dc.GetSafeHdc()), 0);

		int cxIcon = GetSystemMetrics(SM_CXICON);
		int cyIcon = GetSystemMetrics(SM_CYICON);
		CRect rect;
		GetClientRect(&rect);
		int x = (rect.Width() - cxIcon + 1) / 2;
		int y = (rect.Height() - cyIcon + 1) / 2;

		dc.DrawIcon(x, y, m_hIcon);
	}
	else
	{
		CDialog::OnPaint();
	}
}

HCURSOR CGrab_CallbackDlg::OnQueryDragIcon()
{
	return static_cast<HCURSOR>(m_hIcon);
}


void CGrab_CallbackDlg::OnBnClickedBtnEnum()
{
    CString strMsg;

    // ch:清除设备列表框中的信息 | en:Clear Device List Information
    m_nDeviceCombo = 0;
    m_ctrlDeviceCombo.ResetContent();

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

    // ch:枚举子网内所有设备 | en:Enumerate all devices within subnet
    int nRet = MVID_CR_CAM_EnumDevices(m_pstDevList);
    if (MVID_CR_OK != nRet)
    {
        return;
    }

    int nIp1, nIp2, nIp3, nIp4;
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
            nIp4 = ( pDeviceInfo->nCurrentIp & 0x000000ff);

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
            strMsg.Format(_T("[%d]GigE:  %s  (%d.%d.%d.%d)"), i, 
                pUserName, nIp1, nIp2, nIp3, nIp4);
            if (NULL != pUserName)
            {
                delete(pUserName);
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
                delete(pUserName);
                pUserName = NULL;
            }
        }
        else
        {
            ShowErrorMsg(TEXT("Unknown device enumerated"), 0);
        }
        m_ctrlDeviceCombo.AddString(strMsg);
    }

    if (0 == m_pstDevList->nCamNum)
    {
        CString strMsg;
        strMsg.Format(_T("None Device"));
        MessageBox(strMsg, TEXT("device"), MB_OK | MB_ICONWARNING);
        return;
    }
    m_ctrlDeviceCombo.SetCurSel(0);
    UpdateData(FALSE);
}

void __stdcall VirtualImageCallBack(MVID_CAM_OUTPUT_INFO* pFrameInfo, void* pUser)
{
    CGrab_CallbackDlg * pThis = (CGrab_CallbackDlg *)pUser;
    if (pThis)
    {
        pThis->ImageCallBack(pFrameInfo);
    }
}

void CGrab_CallbackDlg::OnBnClickedBtnStart()
{
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

    // ch:仅一维码识别：MVID_BCR | en:Recognize barcode only：MVID_BCR
    // ch:仅二维码识别：MVID_TDCR | en:Recognize Two-Dimension code only: MVID_TDCR
    // ch:一维码 + 二维码 识别：MVID_BCR | MVID_TDCR | en:Recognize Barcode + Two-Dimension code: MVID_BCR | MVID_TDCR
    m_bProcess = FALSE;
    if(m_handle)
    {
        MVID_CR_DestroyHandle(m_handle);
        m_handle = NULL;
    }

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

    nRet = MVID_CR_CAM_RegisterImageCallBack(m_handle, VirtualImageCallBack, this);
    if (MVID_CR_OK != nRet)
    {
        ShowErrorMsg(TEXT("MVID_CR_CAM_RegisterImageCallBack "), nRet);
        return ;
    }

    nRet = MVID_CR_CAM_StartGrabbing(m_handle);
    if (MVID_CR_OK != nRet)
    {
        ShowErrorMsg(TEXT("MVID_CR_CAM_StartGrabbing "), nRet);
        return ;
    }

    m_bProcess = TRUE;

    GetDlgItem(IDC_BTN_Start)->EnableWindow(FALSE);
    GetDlgItem(IDC_BTN_Stop)->EnableWindow(TRUE);
}

void CGrab_CallbackDlg::SetHScroll()
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

void CGrab_CallbackDlg::ImageCallBack(MVID_CAM_OUTPUT_INFO* pstOutput)
{
    int nRet = MVID_CR_OK;
    CString strMsg;
    if (m_bProcess && NULL != pstOutput)
    {
        m_ctrlListBoxResult.SetRedraw(FALSE);

        if (0x0804 == m_nSystemLanguageId)
        {
            if (0 != pstOutput->stCodeList.nCodeNum)
            {
                // ch:输出结果 | en:Output result
                strMsg.Format(_T("已识别 %d 个对象："), pstOutput->stCodeList.nCodeNum);
                m_ctrlListBoxResult.AddString(strMsg);
                for (int i = 0; i < pstOutput->stCodeList.nCodeNum; ++i)
                {
                    wchar_t strWchar[MVID_MAX_CODECHARATERLEN] = {0};
                    Char2Wchar((char*)pstOutput->stCodeList.stCodeInfo[i].strCode, strWchar, MVID_MAX_CODECHARATERLEN);

                    strMsg.Format(_T("第%d个：[%s], 码类型[%d], 是否被过滤[%d], 帧号[%d]"), i+1,
                        strWchar,
                        (unsigned int)pstOutput->stCodeList.stCodeInfo[i].enBarType,
                        pstOutput->stCodeList.stCodeInfo[i].nFilterFlag,
                        pstOutput->stImage.nFrameNum);
                    m_ctrlListBoxResult.AddString(strMsg);
                }
                SetHScroll();
            }
            else
            {
                strMsg.Format(_T("已识别 0 个对象, 帧号[%d]"), pstOutput->stImage.nFrameNum);
                m_ctrlListBoxResult.AddString(strMsg);
            }
        }
        else
        {
            if (0 != pstOutput->stCodeList.nCodeNum)
            {
                // ch:输出结果 | en:Output result
                strMsg.Format(_T("Recognized %d Objects:"), pstOutput->stCodeList.nCodeNum);
                m_ctrlListBoxResult.AddString(strMsg);
                for (int i = 0; i < pstOutput->stCodeList.nCodeNum; ++i)
                {
                    wchar_t strWchar[MVID_MAX_CODECHARATERLEN] = {0};
                    Char2Wchar((char*)pstOutput->stCodeList.stCodeInfo[i].strCode, strWchar, MVID_MAX_CODECHARATERLEN);

                    strMsg.Format(_T("No.%d：[%s], Code Type[%d], Filtered or Not[%d], Frame No.[%d]"), i+1,
                        strWchar,
                        (unsigned int)pstOutput->stCodeList.stCodeInfo[i].enBarType,
                        pstOutput->stCodeList.stCodeInfo[i].nFilterFlag,
                        pstOutput->stImage.nFrameNum);
                    m_ctrlListBoxResult.AddString(strMsg);
                }
                SetHScroll();
            }
            else
            {
                strMsg.Format(_T("No object is recognized, frame No.[%d]"), pstOutput->stImage.nFrameNum);
                m_ctrlListBoxResult.AddString(strMsg);
            }
        }

        m_ctrlListBoxResult.SetTopIndex(m_ctrlListBoxResult.GetCount()-1);
        m_ctrlListBoxResult.SetRedraw(TRUE);

        Display(m_hWndDisplay, pstOutput);

        m_nListNum = m_ctrlListBoxResult.GetCount();
        CheckListNum();
    }
}

// ch:显示错误信息 | en:Show error message
void CGrab_CallbackDlg::ShowErrorMsg(CString csMessage, int nErrorNum)
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

void CGrab_CallbackDlg::OnBnClickedBtnStop()
{
    CString strMsg;
    m_bProcess = FALSE;
    if(m_handle)
    {
        int nRet = MVID_CR_CAM_StopGrabbing(m_handle);
        if (MVID_CR_OK != nRet)
        {
            ShowErrorMsg(TEXT("stop grabbing failed "), nRet);
        }

        nRet = MVID_CR_DestroyHandle(m_handle);
        if (MVID_CR_OK == nRet)
        {
            strMsg.Format(_T("stop identify success"));
            MessageBox(strMsg);
        }
        else
        {
            ShowErrorMsg(TEXT("stop identify failed "), nRet);
        }

        m_handle = NULL;
    }

    this->RedrawWindow();

    GetDlgItem(IDC_BTN_Start)->EnableWindow(TRUE);
    GetDlgItem(IDC_BTN_Stop)->EnableWindow(FALSE);
}

void CGrab_CallbackDlg::OnBnClickedBtnClear()
{
    m_ctrlListBoxResult.ResetContent();
}

void CGrab_CallbackDlg::OnClose()
{
    m_bProcess = FALSE;
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

    if (m_pstBitmapInfo)
    {
        free(m_pstBitmapInfo);
        m_pstBitmapInfo = NULL;
    }

    CDialog::OnClose();
}

void CGrab_CallbackDlg::Display(void* hWnd, MVID_CAM_OUTPUT_INFO* pstDisplayImage)
{
    int nRet = MVID_CR_OK;

    if (pstDisplayImage->stImage.nWidth == 0 || pstDisplayImage->stImage.nHeight == 0 || NULL == hWnd)
    {
        return;
    }
    if (NULL == m_pstBitmapInfo)
    {
        m_pstBitmapInfo = (PBITMAPINFO)malloc(sizeof(BITMAPINFO) + 256 * sizeof(RGBQUAD));
        if (NULL == m_pstBitmapInfo)
        {
            return;
        }
        memset(m_pstBitmapInfo, 0, sizeof(BITMAPINFO) + 256 * sizeof(RGBQUAD));
    }

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
            m_pstBitmapInfo->bmiHeader.biBitCount = 8;                                // 比特数/像素的颜色深度,2^8=256

            for(int i = 0; i < 256; i++)
            {
                m_pstBitmapInfo->bmiColors[i].rgbBlue = m_pstBitmapInfo->bmiColors[i].rgbRed = m_pstBitmapInfo->bmiColors[i].rgbGreen = i;
                m_pstBitmapInfo->bmiColors[i].rgbReserved = 0;
            }
        }
        break;
    case MVID_IMAGE_BGR24:
        {
            m_pstBitmapInfo->bmiHeader.biBitCount = 24;
        }
        break;
    default:
        break;
    }

    // ch:位图信息头 | en:header of bitmap information
    m_pstBitmapInfo->bmiHeader.biSize = sizeof(BITMAPINFOHEADER);             // ch:BITMAPINFOHEADER结构长度 | en:Size of structure BITMAPINFOHEADER
    m_pstBitmapInfo->bmiHeader.biWidth = nImageWidth;                         // ch:图像宽度 | en:Image width
    m_pstBitmapInfo->bmiHeader.biPlanes = 1;                                  // ch:位面数 | en:Number of bit planes
    m_pstBitmapInfo->bmiHeader.biCompression = BI_RGB;                        // ch:图像数据压缩类型,BI_RGB表示不压缩 | en:Encoding type of image data, BI_RGB-nor encode
    m_pstBitmapInfo->bmiHeader.biSizeImage = nImageWidth * nImageHeight;      // ch:图像大小 | en:Image size

    if (MVID_IMAGE_BMP == pstDisplayImage->stImage.enImageType)
    {
        int nOffset = sizeof(RGBQUAD) * 256 + sizeof(BITMAPFILEHEADER) + sizeof(BITMAPINFOHEADER);
        m_pstBitmapInfo->bmiHeader.biHeight = nImageHeight;                       // ch:图像高度 |en:Image height

        nRet = StretchDIBits(hDC,
            nDstX, nDstY, nDstWidth, nDstHeight,
            nSrcX, nSrcY, nSrcWidth, nSrcHeight, pstDisplayImage->stImage.pImageBuf + nOffset, m_pstBitmapInfo, DIB_RGB_COLORS, SRCCOPY);
    }
    else
    {
        m_pstBitmapInfo->bmiHeader.biHeight = - nImageHeight;                       // ch:图像高度 |en:Image height
        nRet = StretchDIBits(hDC,
            nDstX, nDstY, nDstWidth, nDstHeight,
            nSrcX, nSrcY, nSrcWidth, nSrcHeight, pstDisplayImage->stImage.pImageBuf, m_pstBitmapInfo, DIB_RGB_COLORS, SRCCOPY);
    }

    // ch:画框 | en: Image frame
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

void CGrab_CallbackDlg::CheckListNum()
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