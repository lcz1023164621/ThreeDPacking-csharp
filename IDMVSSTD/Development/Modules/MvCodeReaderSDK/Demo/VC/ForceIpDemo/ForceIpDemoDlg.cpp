
// ForceIpDemoDlg.cpp : implementation file

#include "stdafx.h"
#include "ForceIpDemo.h"
#include "ForceIpDemoDlg.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

// used for CAboutDlg dialog in the "about" menu of the application 
class CAboutDlg : public CDialog
{
public:
	CAboutDlg();

// dialog data
	enum { IDD = IDD_ABOUTBOX };

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV supported

// implementation
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

// CForceIpDemoDlg dialog
CForceIpDemoDlg::CForceIpDemoDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CForceIpDemoDlg::IDD, pParent)
    , m_nDeviceCombo(0)
    , m_dwIpaddress(0)
    , m_dwSubNetMask(0)
    , m_dwDefaultGateWay(0)
{
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);
}

void CForceIpDemoDlg::DoDataExchange(CDataExchange* pDX)
{
    CDialog::DoDataExchange(pDX);
    DDX_Control(pDX, IDC_DEVICE_COMBO, m_ctrlDeviceCombo);
    DDX_CBIndex(pDX, IDC_DEVICE_COMBO, m_nDeviceCombo);
    DDX_Control(pDX, IDC_ENUM_DEVICE_BUTTON, m_ctrlEnumDeviceButton);
    DDX_Control(pDX, IDC_SET_IP_BUTTON, m_ctrlSetIpButton);

    DDX_Control(pDX, IDC_IPADDRESS, m_ctrlIpaddress);
    DDX_IPAddress(pDX, IDC_IPADDRESS, m_dwIpaddress); 

    DDX_Control(pDX, IDC_IPADDRESS2, m_ctrlSubNetMask);
    DDX_IPAddress(pDX, IDC_IPADDRESS2, m_dwSubNetMask);  

    DDX_Control(pDX, IDC_IPADDRESS3, m_ctrlDefaultGateWay);
    DDX_IPAddress(pDX, IDC_IPADDRESS3, m_dwDefaultGateWay);
}


BEGIN_MESSAGE_MAP(CForceIpDemoDlg, CDialog)
	ON_WM_SYSCOMMAND()
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	//}}AFX_MSG_MAP
    ON_BN_CLICKED(IDC_ENUM_DEVICE_BUTTON, &CForceIpDemoDlg::OnBnClickedEnumDeviceButton)
    ON_BN_CLICKED(IDC_SET_IP_BUTTON, &CForceIpDemoDlg::OnBnClickedSetIpButton)
    ON_CBN_SELCHANGE(IDC_DEVICE_COMBO, &CForceIpDemoDlg::OnCbnSelchangeDeviceCombo)
END_MESSAGE_MAP()


// CForceIpDemoDlg message handler
BOOL CForceIpDemoDlg::OnInitDialog()
{
	CDialog::OnInitDialog();

    // add "about" menu item to the menu of system

    // IDM_ABOUTBOX has to be in range of system's command
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

    // set the icon of this dialog. When main window of application is not dialog, the frame will
    // automatically perform this execution 
	SetIcon(m_hIcon, TRUE);			// set big icon
	SetIcon(m_hIcon, FALSE);		// set small icon 

	return TRUE;  // return TRUE unless the focus is on the widget
}

// ch:判断字符类型 | en:str type
bool CForceIpDemoDlg::IsStrUTF8(const char* pBuffer, int size)
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
bool CForceIpDemoDlg::Char2Wchar(const char *pStr, wchar_t *pOutWStr, int nOutStrSize)
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
bool CForceIpDemoDlg::Wchar2char(wchar_t *pOutWStr, char *pStr)
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

void CForceIpDemoDlg::OnSysCommand(UINT nID, LPARAM lParam)
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

// The code below is needed if the you want to add minimzing button to the dialog
//  in order to draw image of the button. For documentation/visual model of the MFC application,
//  this will be finished by frame automatically 

void CForceIpDemoDlg::OnPaint()
{
	if (IsIconic())
	{
		CPaintDC dc(this); // used to draw context of the device

		SendMessage(WM_ICONERASEBKGND, reinterpret_cast<WPARAM>(dc.GetSafeHdc()), 0);

		// make the icon in the middle of the metircs of working area
		int cxIcon = GetSystemMetrics(SM_CXICON);
		int cyIcon = GetSystemMetrics(SM_CYICON);
		CRect rect;
		GetClientRect(&rect);
		int x = (rect.Width() - cxIcon + 1) / 2;
		int y = (rect.Height() - cyIcon + 1) / 2;

		// draw the icon
		dc.DrawIcon(x, y, m_hIcon);
	}
	else
	{
		CDialog::OnPaint();
	}
}

//System call this function to get the diaplay of cursor, when user drag the minimized window
HCURSOR CForceIpDemoDlg::OnQueryDragIcon()
{
	return static_cast<HCURSOR>(m_hIcon);
}

// ch:显示错误信息 | en:Show error message
void CForceIpDemoDlg::ShowErrorMsg(CString csMessage, int nErrorNum)
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
    case MV_CODEREADER_E_HANDLE:           errorMsg += "Error or invalid handle ";                                         break;
    case MV_CODEREADER_E_SUPPORT:          errorMsg += "Not supported function ";                                          break;
    case MV_CODEREADER_E_BUFOVER:          errorMsg += "Cache is full ";                                                   break;
    case MV_CODEREADER_E_CALLORDER:        errorMsg += "Function calling order error ";                                    break;
    case MV_CODEREADER_E_PARAMETER:        errorMsg += "Incorrect parameter ";                                             break;
    case MV_CODEREADER_E_RESOURCE:         errorMsg += "Applying resource failed ";                                        break;
    case MV_CODEREADER_E_NODATA:           errorMsg += "No data ";                                                         break;
    case MV_CODEREADER_E_PRECONDITION:     errorMsg += "Precondition error, or running environment changed ";              break;
    case MV_CODEREADER_E_VERSION:          errorMsg += "Version mismatches ";                                              break;
    case MV_CODEREADER_E_NOENOUGH_BUF:     errorMsg += "Insufficient memory ";                                             break;
    case MV_CODEREADER_E_ABNORMAL_IMAGE:   errorMsg += "Abnormal image, maybe incomplete image because of lost packet ";   break;
    case MV_CODEREADER_E_UNKNOW:           errorMsg += "Unknown error ";                                                   break;
    case MV_CODEREADER_E_GC_GENERIC:       errorMsg += "General error ";                                                   break;
    case MV_CODEREADER_E_GC_ACCESS:        errorMsg += "Node accessing condition error ";                                  break;
    case MV_CODEREADER_E_ACCESS_DENIED:    errorMsg += "No permission ";                                                   break;
    case MV_CODEREADER_E_BUSY:             errorMsg += "Device is busy, or network disconnected ";                         break;
    case MV_CODEREADER_E_NETER:            errorMsg += "Network error ";                                                   break;
    }

    MessageBox(errorMsg, TEXT("PROMPT"), MB_OK | MB_ICONWARNING);
}

void CForceIpDemoDlg::OnBnClickedEnumDeviceButton()
{
    CString strMsg;

    // ch:清除设备列表框中的信息 | en:clear the information in the device list
    m_ctrlDeviceCombo.ResetContent();

    m_nDeviceCombo = 0; // 第一个

    // ch:初始化设备信息列表 | en:initialize device list
    memset(&m_stDevList, 0, sizeof(MV_CODEREADER_DEVICE_INFO_LIST));

    // ch:枚举子网内所有设备 | en:enumerate all the devices in the subnetworks
    int nRet = MV_CODEREADER_EnumDevices(&m_stDevList, MV_CODEREADER_GIGE_DEVICE);
    if (MV_CODEREADER_OK != nRet)
    {
        ShowErrorMsg(TEXT("Enum device error"), nRet);
        // ch:枚举设备失败  | en:fail to enumerate device 
        return;
    }

    // ch:将值加入到信息列表框中并显示出来 | en:add value to the information list and display it
    unsigned int i;
    int nIp1, nIp2, nIp3, nIp4;
    for (i = 0; i < m_stDevList.nDeviceNum; i++)
    {
        MV_CODEREADER_DEVICE_INFO* pDeviceInfo = m_stDevList.pDeviceInfo[i];
        if (NULL == pDeviceInfo)
        {
            continue;
        }
        if (pDeviceInfo->nTLayerType == MV_CODEREADER_GIGE_DEVICE)
        {
            nIp1 = ((m_stDevList.pDeviceInfo[i]->SpecialInfo.stGigEInfo.nCurrentIp & 0xff000000) >> 24);
            nIp2 = ((m_stDevList.pDeviceInfo[i]->SpecialInfo.stGigEInfo.nCurrentIp & 0x00ff0000) >> 16);
            nIp3 = ((m_stDevList.pDeviceInfo[i]->SpecialInfo.stGigEInfo.nCurrentIp & 0x0000ff00) >> 8);
            nIp4 = (m_stDevList.pDeviceInfo[i]->SpecialInfo.stGigEInfo.nCurrentIp & 0x000000ff);

            // 中文字体显示
            wchar_t strWchar[16] = {0};
            Char2Wchar((char*)pDeviceInfo->SpecialInfo.stGigEInfo.chUserDefinedName, strWchar, 16);
            Wchar2char(strWchar, (char*)pDeviceInfo->SpecialInfo.stGigEInfo.chUserDefinedName);

            strMsg.Format(_T("[%d]GigE:   %s %s (%d.%d.%d.%d)"), i, 
                CStringW(pDeviceInfo->SpecialInfo.stGigEInfo.chUserDefinedName), 
                CStringW(pDeviceInfo->SpecialInfo.stGigEInfo.chManufacturerName), nIp1, nIp2, nIp3, nIp4);

        }
        else if (pDeviceInfo->nTLayerType == MV_CODEREADER_USB_DEVICE)
        {
            // 中文字体显示
            wchar_t strWchar[16] = {0};
            Char2Wchar((char*)pDeviceInfo->SpecialInfo.stUsb3VInfo.chManufacturerName, strWchar, 16);
            Wchar2char(strWchar, (char*)pDeviceInfo->SpecialInfo.stUsb3VInfo.chManufacturerName);

            strMsg.Format(_T("[%d]UsbV3:  %s"), i, CStringW(pDeviceInfo->SpecialInfo.stUsb3VInfo.chManufacturerName));
        }
        else
        {
        }
        m_ctrlDeviceCombo.AddString(strMsg);
    }
    m_ctrlDeviceCombo.SetCurSel(CB_ERR);

    DisplayDeviceIp();

    UpdateData(FALSE);

    if (0 == m_stDevList.nDeviceNum)
    {
        ShowErrorMsg(TEXT("No device"), 0);
        return;
    }
    
    m_ctrlSetIpButton.EnableWindow(TRUE);

    return;
}

void CForceIpDemoDlg::OnBnClickedSetIpButton()
{
    // ch:先读取ip框数据 | en:Read ip data first
    UpdateData(TRUE);

    int nIndex = m_nDeviceCombo;
    if ((nIndex < 0) | (nIndex >= MV_CODEREADER_MAX_DEVICE_NUM))
    {
        ShowErrorMsg(TEXT("Please select device"), 0);
        return;
    }

    // ch:由设备信息创建设备实例 | en:create example of device from the device list
    if (NULL == m_stDevList.pDeviceInfo[nIndex])
    {
        ShowErrorMsg(TEXT("Device does not exist"), 0);
        return;
    }
    
    if (NULL !=  m_handle)
    {
        MV_CODEREADER_DestroyHandle(m_handle);
    }

    int nRet = MV_CODEREADER_OK ;

    try
    {
        nRet =  MV_CODEREADER_CreateHandle(&m_handle, m_stDevList.pDeviceInfo[nIndex]);
        if (MV_CODEREADER_OK != nRet)
        {
            ShowErrorMsg(TEXT("Create handle fail"), nRet);
            throw nRet;
        }

        nRet = MV_CODEREADER_GIGE_ForceIp(m_handle, m_dwIpaddress,m_dwSubNetMask, m_dwDefaultGateWay);
        if (MV_CODEREADER_OK != nRet)
        {
            ShowErrorMsg(TEXT("Set forceIp fail"), nRet);
            throw nRet;
        }

        ShowErrorMsg(TEXT("Set forceIp succeed"), 0);
    }
    catch (...)
    {
        if (NULL !=  m_handle)
        {
            MV_CODEREADER_DestroyHandle(m_handle);
            m_handle = NULL;
        }
        return ;
    }

      
    if (NULL !=  m_handle)
    {
        MV_CODEREADER_DestroyHandle(m_handle);
        m_handle = NULL;
    }

    return;
}

void CForceIpDemoDlg::OnCbnSelchangeDeviceCombo()
{
    UpdateData(TRUE);
    DisplayDeviceIp();
}

int CForceIpDemoDlg::DisplayDeviceIp()
{
    int nIndex = m_nDeviceCombo;
    if ((nIndex < 0) | (nIndex >= MV_CODEREADER_MAX_DEVICE_NUM))
    {
        ShowErrorMsg(TEXT("Please select device"), 0);
        return -1;
    }

    if ((0 ==  m_stDevList.nDeviceNum) ||
        (m_nDeviceCombo > m_stDevList.nDeviceNum))
    {
        UpdateData(FALSE);
        return MV_CODEREADER_OK;
    }

    m_ctrlIpaddress.SetAddress(m_stDevList.pDeviceInfo[nIndex]->SpecialInfo.stGigEInfo.nCurrentIp);
    m_ctrlSubNetMask.SetAddress(m_stDevList.pDeviceInfo[nIndex]->SpecialInfo.stGigEInfo.nCurrentSubNetMask);
    m_ctrlDefaultGateWay.SetAddress(m_stDevList.pDeviceInfo[nIndex]->SpecialInfo.stGigEInfo.nDefultGateWay);

    m_ctrlIpaddress.GetAddress(m_dwIpaddress);
    m_ctrlSubNetMask.GetAddress(m_dwSubNetMask);
    m_ctrlDefaultGateWay.GetAddress(m_dwDefaultGateWay);

    UpdateData(FALSE);
    return MV_CODEREADER_OK;
}

BOOL CForceIpDemoDlg::PreTranslateMessage(MSG* pMsg)
{
    if (pMsg->message == WM_KEYDOWN&&pMsg->wParam == VK_ESCAPE)
    {
        // ch:如果消息是键盘按下事件，且是Esc键，执行以下代码（什么都不做，你可以自己添加想要的代码）
        // en:If the message is a keyboard press event and a Esc key, execute the following code (nothing is done, you can add the code you want)
        return TRUE;
    }
    if (pMsg->message == WM_KEYDOWN && pMsg->wParam == VK_RETURN)
    {
        // ch:如果消息是键盘按下事件，且是Entert键，执行以下代码（什么都不做，你可以自己添加想要的代码）
        // en:If the message is a keyboard press event and a Esc key, execute the following code (nothing is done, you can add the code you want)
        return TRUE;
    }

    return CDialog::PreTranslateMessage(pMsg);
}

