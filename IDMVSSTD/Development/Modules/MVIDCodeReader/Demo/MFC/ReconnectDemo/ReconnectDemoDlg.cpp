#include "stdafx.h"
#include "ReconnectDemo.h"
#include "ReconnectDemoDlg.h"

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

CReconnectDemoDlg::CReconnectDemoDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CReconnectDemoDlg::IDD, pParent)
    , m_nDeviceCombo(0)
    , m_nIndex(0)
    , m_nListNum(0)
    , m_bProcess(FALSE)
    , m_handle(NULL)
    , m_pstDevList(NULL)
    , m_nSystemLanguageId(0)
{
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);
}

void CReconnectDemoDlg::DoDataExchange(CDataExchange* pDX)
{
    CDialog::DoDataExchange(pDX);
    DDX_Control(pDX, IDC_COMBO_CamList, m_ctrlDeviceCombo);
    DDX_CBIndex(pDX, IDC_COMBO_CamList, m_nDeviceCombo);
    DDX_Control(pDX, IDC_LIST_Result, m_ctrlListBoxResult);
}

BEGIN_MESSAGE_MAP(CReconnectDemoDlg, CDialog)
	ON_WM_SYSCOMMAND()
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	//}}AFX_MSG_MAP
    ON_BN_CLICKED(IDC_BTN_Enum, &CReconnectDemoDlg::OnBnClickedBtnEnum)
    ON_BN_CLICKED(IDC_BTN_Start, &CReconnectDemoDlg::OnBnClickedBtnStart)
    ON_BN_CLICKED(IDC_BTN_Stop, &CReconnectDemoDlg::OnBnClickedBtnStop)
    ON_BN_CLICKED(IDC_BTN_Clear, &CReconnectDemoDlg::OnBnClickedBtnClear)
    ON_WM_CLOSE()
END_MESSAGE_MAP()

BOOL CReconnectDemoDlg::OnInitDialog()
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

    GetDlgItem(IDC_BTN_Start)->EnableWindow(TRUE);
    GetDlgItem(IDC_BTN_Stop)->EnableWindow(FALSE);

    // 获取语言id 中文简体：0x0804, 美国英语：0x0409
    m_nSystemLanguageId = GetUserDefaultUILanguage();

	return TRUE;
}

void CReconnectDemoDlg::OnSysCommand(UINT nID, LPARAM lParam)
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

void CReconnectDemoDlg::OnPaint()
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

HCURSOR CReconnectDemoDlg::OnQueryDragIcon()
{
	return static_cast<HCURSOR>(m_hIcon);
}

// 枚举相机
void CReconnectDemoDlg::OnBnClickedBtnEnum()
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
            strMsg.Format(_T("[%d]GigE:    %s  (%d.%d.%d.%d)"), i, 
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
        CString errorMsg;
        errorMsg.Format(_T("None Device"));
        MessageBox(errorMsg, TEXT("device"), MB_OK | MB_ICONWARNING);
        return;
    }
    m_ctrlDeviceCombo.SetCurSel(0);
    UpdateData(FALSE);
}

void __stdcall VirtualImageCallBack(MVID_CAM_OUTPUT_INFO* pFrameInfo, void* pUser)
{
    CReconnectDemoDlg * pThis = (CReconnectDemoDlg *)pUser;
    if (pThis)
    {
        pThis->ImageCallBack(pFrameInfo);
    }
}

void __stdcall VirtualExceptionCallBack(unsigned int nMsgType, void* pUser)
{
    CReconnectDemoDlg * pThis = (CReconnectDemoDlg *)pUser;
    if (pThis)
    {
        pThis->ExceptionCallBack(nMsgType);
    }
}

// 开始读码
void CReconnectDemoDlg::OnBnClickedBtnStart()
{
    UpdateData(TRUE);
    m_ctrlListBoxResult.ResetContent();

    m_nIndex = m_nDeviceCombo;
    if ((m_nIndex < 0) | (m_nIndex >= MVID_MAX_CAM_NUM))
    {
        ShowErrorMsg(TEXT("Please select device"), 0);
        return ;
    }

    // ch:由设备信息创建设备实例 | en:Device instance created by device information
    if (NULL == m_pstDevList->pstCamInfo[m_nIndex])
    {
        ShowErrorMsg(TEXT("Device does not exist"), 0);
        return ;
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

    nRet = MVID_CR_CAM_BindDevice(m_handle, m_pstDevList->pstCamInfo[m_nIndex]);
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

    nRet = MVID_CR_RegisterExceptionCallBack(m_handle, VirtualExceptionCallBack, this);
    if (MVID_CR_OK != nRet)
    {
        ShowErrorMsg(TEXT("MVID_CR_RegisterExceptionCallBack "), nRet);
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

void CReconnectDemoDlg::SetHScroll()
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
    catch (CMemoryException* e)
    {
        return;
    }

    ReleaseDC(dc);
}

// 图像回调
void CReconnectDemoDlg::ImageCallBack(MVID_CAM_OUTPUT_INFO* pstOutput)
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
                //ch:输出结果 | en:Output result
                strMsg.Format(_T("已识别 %d 个对象："), pstOutput->stCodeList.nCodeNum);
                m_ctrlListBoxResult.AddString(strMsg);
                for (int i = 0; i < pstOutput->stCodeList.nCodeNum; ++i)
                {
                    wchar_t strWchar[MVID_MAX_CODECHARATERLEN] = {0};
                    Char2Wchar((char*)pstOutput->stCodeList.stCodeInfo[i].strCode, strWchar, MVID_MAX_CODECHARATERLEN);

                    strMsg.Format(_T("第%d个：[%s], 码类型[%d]"), i+1,
                        strWchar,
                        (unsigned int)pstOutput->stCodeList.stCodeInfo[i].enBarType);
                    m_ctrlListBoxResult.AddString(strMsg);
                }
                SetHScroll();
            }
            else
            {
                strMsg.Format(_T("已识别 0 个对象"), nRet);
                m_ctrlListBoxResult.AddString(strMsg);
            }
        }
        else
        {
            if (0 != pstOutput->stCodeList.nCodeNum)
            {
                //ch:输出结果 | en:Output result
                strMsg.Format(_T("Recognized %d Objects:"), pstOutput->stCodeList.nCodeNum);
                m_ctrlListBoxResult.AddString(strMsg);
                for (int i = 0; i < pstOutput->stCodeList.nCodeNum; ++i)
                {
                    wchar_t strWchar[MVID_MAX_CODECHARATERLEN] = {0};
                    Char2Wchar((char*)pstOutput->stCodeList.stCodeInfo[i].strCode, strWchar, MVID_MAX_CODECHARATERLEN);

                    strMsg.Format(_T("No.%d: [%s], Code Type [%d]"), i+1,
                        strWchar,
                        (unsigned int)pstOutput->stCodeList.stCodeInfo[i].enBarType);
                    m_ctrlListBoxResult.AddString(strMsg);
                }
                SetHScroll();
            }
            else
            {
                strMsg.Format(_T("No object is recognized"), nRet);
                m_ctrlListBoxResult.AddString(strMsg);
            }
        }

        m_ctrlListBoxResult.SetTopIndex(m_ctrlListBoxResult.GetCount()-1);
        m_ctrlListBoxResult.SetRedraw(TRUE);
        m_nListNum = m_ctrlListBoxResult.GetCount();
        CheckListNum();
    }
}

// 异常回调
void     CReconnectDemoDlg::ExceptionCallBack(unsigned int nMsgType)
{
    int nRet = MVID_CR_OK;
    BOOL bConnect = FALSE;
    int nReConnectTime = 0;
    CString strMsg;
    if (m_bProcess)
    {
        if (nMsgType == MVID_EXCEPTION_DEV_DISCONNECT)
        {
            while (nReConnectTime < 10 && bConnect == FALSE)
            {
                // ch:每500ms重连一次相机 | en:Reconnect to the camera per 500ms
                Sleep(500);

                // 语言版本区分
                if (0x0804 == m_nSystemLanguageId)
                {
                    strMsg.Format(_T("相机掉线,尝试重连相机第(%d)次"), nReConnectTime + 1);
                }
                else
                {
                    strMsg.Format(_T("The camera is offline. Try to reconnect for (%d) times"), nReConnectTime + 1);
                }

                m_ctrlListBoxResult.AddString(strMsg);
                m_ctrlListBoxResult.SetTopIndex(m_ctrlListBoxResult.GetCount()-1);
                if(m_handle)
                {
                    MVID_CR_DestroyHandle(m_handle);
                    m_handle = NULL;
                }

                int nRet = MVID_CR_CreateHandle(&m_handle, MVID_BCR | MVID_TDCR);
                if (MVID_CR_OK != nRet)
                {
                    strMsg.Format(_T("MVID_CR_CreateHandle failed(%#x)"), nRet);
                    m_ctrlListBoxResult.AddString(strMsg);
                    m_ctrlListBoxResult.SetTopIndex(m_ctrlListBoxResult.GetCount()-1);
                    nReConnectTime ++;
                    continue;
                }

                nRet = MVID_CR_CAM_BindDevice(m_handle, m_pstDevList->pstCamInfo[m_nIndex]);
                if (MVID_CR_OK != nRet)
                {
                    strMsg.Format(_T("MVID_CR_CAM_BindDevice failed(%#x)"), nRet);
                    m_ctrlListBoxResult.AddString(strMsg);
                    m_ctrlListBoxResult.SetTopIndex(m_ctrlListBoxResult.GetCount()-1);
                    nReConnectTime ++;
                    continue;
                }

                nRet = MVID_CR_CAM_RegisterImageCallBack(m_handle, VirtualImageCallBack, this);
                if (MVID_CR_OK != nRet)
                {
                    strMsg.Format(_T("MVID_CR_CAM_RegisterImageCallBack failed(%#x)"), nRet);
                    m_ctrlListBoxResult.AddString(strMsg);
                    m_ctrlListBoxResult.SetTopIndex(m_ctrlListBoxResult.GetCount()-1);
                    nReConnectTime ++;
                    continue;
                }

                nRet = MVID_CR_RegisterExceptionCallBack(m_handle, VirtualExceptionCallBack, this);
                if (MVID_CR_OK != nRet)
                {
                    strMsg.Format(_T("MVID_CR_RegisterExceptionCallBack failed(%#x)"), nRet);
                    m_ctrlListBoxResult.AddString(strMsg);
                    m_ctrlListBoxResult.SetTopIndex(m_ctrlListBoxResult.GetCount()-1);
                    nReConnectTime ++;
                    continue;
                }

                nRet = MVID_CR_CAM_StartGrabbing(m_handle);
                if (MVID_CR_OK != nRet)
                {
                    strMsg.Format(_T("MVID_CR_CAM_StartGrabbing failed(%#x)"), nRet);
                    m_ctrlListBoxResult.AddString(strMsg);
                    m_ctrlListBoxResult.SetTopIndex(m_ctrlListBoxResult.GetCount()-1);
                    nReConnectTime ++;
                    continue;
                }

                bConnect = TRUE;
            }

            if(bConnect == TRUE)
            {
                if (0x0804 == m_nSystemLanguageId)
                {
                    strMsg.Format(_T("相机重连成功"));
                }
                else
                {
                    strMsg.Format(_T("The camera is reconnected"));
                }

                m_ctrlListBoxResult.AddString(strMsg);
                m_ctrlListBoxResult.SetTopIndex(m_ctrlListBoxResult.GetCount()-1);
            }
            else
            {
                if (0x0804 == m_nSystemLanguageId)
                {
                    strMsg.Format(_T("相机重连失败"));
                }
                else
                {
                    strMsg.Format(_T("Reconnecting to camera failed"));
                }

                m_ctrlListBoxResult.AddString(strMsg);
                m_ctrlListBoxResult.SetTopIndex(m_ctrlListBoxResult.GetCount()-1);
            }
        }
        if (nMsgType == MVID_EXCEPTION_SOFTDOG_DISCONNECT)
        {
            if (0x0804 == m_nSystemLanguageId)
            {
                strMsg.Format(_T("加密狗掉线"));
            }
            else
            {
                strMsg.Format(_T("The dongle is offline"));
            }

           m_ctrlListBoxResult.AddString(strMsg);
           m_ctrlListBoxResult.SetTopIndex(m_ctrlListBoxResult.GetCount()-1);
        }

        m_nListNum = m_ctrlListBoxResult.GetCount();
        CheckListNum();
    }
}

// ch:显示错误信息 | en:Show error message
void CReconnectDemoDlg::ShowErrorMsg(CString csMessage, int nErrorNum)
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
void CReconnectDemoDlg::OnBnClickedBtnStop()
{
    CString strMsg = _T("");

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

    GetDlgItem(IDC_BTN_Start)->EnableWindow(TRUE);
    GetDlgItem(IDC_BTN_Stop)->EnableWindow(FALSE);
}

// 清空消息
void CReconnectDemoDlg::OnBnClickedBtnClear()
{
    m_ctrlListBoxResult.ResetContent();
}

void CReconnectDemoDlg::OnClose()
{
    m_bProcess = FALSE;
    if (m_handle)
    {
        MVID_CR_DestroyHandle(m_handle);
    }

    if (m_pstDevList)
    {
        free(m_pstDevList);
        m_pstDevList = NULL;
    }

    CDialog::OnClose();
}

void CReconnectDemoDlg::CheckListNum()
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
