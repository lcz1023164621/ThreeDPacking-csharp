
// BasicDemoDlg.cpp : implementation file
//

#include "stdafx.h"
#include "ReconnectDemo.h"
#include "ReconnectDemoDlg.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

#if (_MSC_VER >= 1900)
extern "C"
{
    FILE __iob_func[3] = { *stdin,*stdout,*stderr };
}
#endif

// CAboutDlg dialog used for App About
class CAboutDlg : public CDialog
{
public:
	CAboutDlg();

// Dialog Data
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


// CBasicDemoDlg dialog
CBasicDemoDlg::CBasicDemoDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CBasicDemoDlg::IDD, pParent)
    , m_pcMyCamera(NULL)
    , m_nDeviceCombo(0)
    , m_bRetState(MV_CODEREADER_E_UNKNOW)
    , m_bCreateDevice(FALSE)
    , m_bOpenDevice(FALSE)
    , m_bStartGrabbing(FALSE)
    , m_nTriggerMode(MV_CODEREADER_TRIGGER_MODE_OFF)
    , m_pBufForSaveImage(NULL)
    , m_nBufSizeForSaveImage(0)
    , m_pBufForDriver(NULL)
    , m_nBufSizeForDriver(0)
{
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);
}

void CBasicDemoDlg::DoDataExchange(CDataExchange* pDX)
{
    CDialog::DoDataExchange(pDX);
    DDX_Control(pDX, IDC_OPEN_BUTTON, m_ctrlOpenButton);
    DDX_Control(pDX, IDC_CLOSE_BUTTON, m_ctrlCloseButton);
    DDX_Control(pDX, IDC_START_GRABBING_BUTTON, m_ctrlStartGrabbingButton);
    DDX_Control(pDX, IDC_STOP_GRABBING_BUTTON, m_ctrlStopGrabbingButton);
    DDX_Control(pDX, IDC_CONTINUS_MODE_RADIO, m_ctrlContinusModeRadio);
    DDX_Control(pDX, IDC_TRIGGER_MODE_RADIO, m_ctrlTriggerModeRadio);
    DDX_Control(pDX, IDC_DEVICE_COMBO, m_ctrlDeviceCombo);
    DDX_CBIndex(pDX, IDC_DEVICE_COMBO, m_nDeviceCombo);
    DDX_Control(pDX, IDC_SOFTWARE_TRIGGER_CHECK, m_ctrlSoftwareTriggerCheck);
    DDX_Control(pDX, IDC_SOFTWARE_TRIGGER_ONCE_BUTTON, m_ctrlSoftwareTriggerOnceButton);

}

BEGIN_MESSAGE_MAP(CBasicDemoDlg, CDialog)
	ON_WM_SYSCOMMAND()
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	//}}AFX_MSG_MAP
    ON_BN_CLICKED(IDC_ENUM_BUTTON, &CBasicDemoDlg::OnBnClickedEnumButton)
    ON_BN_CLICKED(IDC_OPEN_BUTTON, &CBasicDemoDlg::OnBnClickedOpenButton)
    ON_BN_CLICKED(IDC_CLOSE_BUTTON, &CBasicDemoDlg::OnBnClickedCloseButton)
    ON_BN_CLICKED(IDC_CONTINUS_MODE_RADIO, &CBasicDemoDlg::OnBnClickedContinusModeRadio)
    ON_BN_CLICKED(IDC_TRIGGER_MODE_RADIO, &CBasicDemoDlg::OnBnClickedTriggerModeRadio)
    ON_BN_CLICKED(IDC_START_GRABBING_BUTTON, &CBasicDemoDlg::OnBnClickedStartGrabbingButton)
    ON_BN_CLICKED(IDC_STOP_GRABBING_BUTTON, &CBasicDemoDlg::OnBnClickedStopGrabbingButton)
    ON_WM_CLOSE()
    ON_BN_CLICKED(IDC_SOFTWARE_TRIGGER_ONCE_BUTTON, &CBasicDemoDlg::OnBnClickedSoftwareTriggerOnceButton)
    ON_BN_CLICKED(IDC_SOFTWARE_TRIGGER_CHECK, &CBasicDemoDlg::OnBnClickedSoftwareTriggerCheck)
END_MESSAGE_MAP()


// CBasicDemoDlg message handlers

BOOL CBasicDemoDlg::OnInitDialog()
{
	CDialog::OnInitDialog();

    // Add "About..." menu item to system menu.

    // IDM_ABOUTBOX must be in the system command range.
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

    // Set the icon for this dialog.  The framework does this automatically
    //  when the application's main window is not a dialog
	SetIcon(m_hIcon, TRUE);			// Set big icon
	SetIcon(m_hIcon, FALSE);		// Set small icon
   
    GdiplusStartupInput gdiplusStartupInput;
    ULONG_PTR gdiplusToken;
    //初始化GDI+
    GdiplusStartup(&gdiplusToken, &gdiplusStartupInput, NULL);


	DisplayWindowInitial();

	return TRUE;  // return TRUE  unless you set the focus to a control
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

// If you add a minimize button to your dialog, you will need the code below
//  to draw the icon.  For MFC applications using the document/view model,
//  this is automatically done for you by the framework.

void CBasicDemoDlg::OnPaint()
{
	if (IsIconic())
	{
		CPaintDC dc(this); // device context for painting

		SendMessage(WM_ICONERASEBKGND, reinterpret_cast<WPARAM>(dc.GetSafeHdc()), 0);

		// Center icon in client rectangle
		int cxIcon = GetSystemMetrics(SM_CXICON);
		int cyIcon = GetSystemMetrics(SM_CYICON);
		CRect rect;
		GetClientRect(&rect);
		int x = (rect.Width() - cxIcon + 1) / 2;
		int y = (rect.Height() - cyIcon + 1) / 2;

		// Draw the icon
		dc.DrawIcon(x, y, m_hIcon);
	}
	else
	{
		CDialog::OnPaint();
	}
}

// The system calls this function to obtain the cursor to display while the user drags
//  the minimized window.
HCURSOR CBasicDemoDlg::OnQueryDragIcon()
{
	return static_cast<HCURSOR>(m_hIcon);
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


// ch:关闭设备时的窗口显示 | en:Window display when closing device
State CBasicDemoDlg::EnableWindowWhenClose(void)
{
    m_ctrlOpenButton.EnableWindow(FALSE);
    m_ctrlCloseButton.EnableWindow(FALSE);
    m_ctrlStartGrabbingButton.EnableWindow(FALSE);
    m_ctrlStopGrabbingButton.EnableWindow(FALSE);
    m_ctrlContinusModeRadio.EnableWindow(FALSE);
    m_ctrlTriggerModeRadio.EnableWindow(FALSE);
    m_ctrlSoftwareTriggerCheck.EnableWindow(FALSE);
    m_ctrlSoftwareTriggerOnceButton.EnableWindow(FALSE);
    m_ctrlContinusModeRadio.SetCheck(FALSE);
    m_ctrlTriggerModeRadio.SetCheck(FALSE);
    return MV_CODEREADER_OK;
}

// ch:打开设备但不开始抓图 | en:Open device but does not start grabbing*/
State CBasicDemoDlg::EnableWindowWhenOpenNotStart(void)
{
    m_ctrlOpenButton.EnableWindow(FALSE);
    m_ctrlCloseButton.EnableWindow(TRUE);
    m_ctrlStartGrabbingButton.EnableWindow(TRUE);
    m_ctrlContinusModeRadio.EnableWindow(TRUE);
    m_ctrlTriggerModeRadio.EnableWindow(TRUE);
    if (m_ctrlTriggerModeRadio.GetCheck())
    {
        OnBnClickedTriggerModeRadio();
    }
    else
    {
        OnBnClickedContinusModeRadio();
    }

    return MV_CODEREADER_OK;
}

// ch:按下开始采集按钮时的按钮颜色 | en:Button color when the start grabbing button is pressed
State CBasicDemoDlg::EnableWindowWhenStart(void)
{
    m_ctrlStopGrabbingButton.EnableWindow(TRUE);
    m_ctrlStartGrabbingButton.EnableWindow(FALSE);

    m_ctrlContinusModeRadio.EnableWindow(FALSE);
    m_ctrlTriggerModeRadio.EnableWindow(FALSE);

    if(m_ctrlSoftwareTriggerCheck.GetCheck())
    {
        m_ctrlSoftwareTriggerCheck.EnableWindow(FALSE);
        m_ctrlSoftwareTriggerOnceButton.EnableWindow(TRUE);
    }
    else
    {
        m_ctrlSoftwareTriggerCheck.EnableWindow(FALSE);
        m_ctrlSoftwareTriggerOnceButton.EnableWindow(FALSE);

    }

    return MV_CODEREADER_OK;
}

// ch:按下结束采集时的按钮颜色 | en:Button color when the stop grabbing button is pressed
State CBasicDemoDlg::EnableWindowWhenStop(void)
{
    m_ctrlStopGrabbingButton.EnableWindow(FALSE);
    m_ctrlStartGrabbingButton.EnableWindow(TRUE);

    m_ctrlContinusModeRadio.EnableWindow(TRUE);
    m_ctrlTriggerModeRadio.EnableWindow(TRUE);

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
    m_ctrlSoftwareTriggerOnceButton.EnableWindow(FALSE);

    return MV_CODEREADER_OK;
}

// ch:最开始时的窗口初始化 | en:Initial window initialization
State CBasicDemoDlg::DisplayWindowInitial(void)
{

    CWnd *pWnd = GetDlgItem(IDC_DISPLAY_STATIC);
    if (NULL == pWnd)
    {
        return MV_CODEREADER_OK;
    }
    m_hwndDisplay = pWnd->GetSafeHwnd();
    if (NULL == m_hwndDisplay)
    {
        return MV_CODEREADER_E_RESOURCE;
    }

    EnableWindowWhenClose();
    return MV_CODEREADER_OK;
}

// ch:显示错误信息 | en:Show error message
void CBasicDemoDlg::ShowErrorMsg(CString csMessage, int nErrorNum)
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
    case MV_CODEREADER_E_ACCESS_DENIED:	errorMsg += "No permission ";                                                   break;
    case MV_CODEREADER_E_BUSY:             errorMsg += "Device is busy, or network disconnected ";                         break;
    case MV_CODEREADER_E_NETER:            errorMsg += "Network error ";                                                   break;
    }

    MessageBox(errorMsg, TEXT("PROMPT"), MB_OK | MB_ICONWARNING);
}


// ch:打开设备 | en:Open Device
State CBasicDemoDlg::OpenDevice(void)
{
    if (TRUE == m_bOpenDevice)
    {
        return MV_CODEREADER_E_CALLORDER;
    }
    UpdateData(TRUE);
    if(TRUE == m_bCreateDevice)
    {
        return MV_CODEREADER_E_CALLORDER;
    }
    int nIndex = m_nDeviceCombo;
    if ((nIndex < 0) | (nIndex >= MV_CODEREADER_MAX_DEVICE_NUM))
    {
        ShowErrorMsg(TEXT("Please select devic"), 0);
        return MV_CODEREADER_E_CALLORDER;
    }

    // ch:由设备信息创建设备实例 | en:Device instance created by device information
    if (NULL == m_stDevList.pDeviceInfo[nIndex])
    {
        ShowErrorMsg(TEXT("Device does not exist!"), 0);
        return MV_CODEREADER_E_CALLORDER;
    }

    if (NULL != m_pcMyCamera)
    {
        m_pcMyCamera->Close();
        delete m_pcMyCamera;
    }

    m_pcMyCamera = new CMyCamera;
    if (NULL == m_pcMyCamera)
    {
        m_bRetState = MV_CODEREADER_E_RESOURCE;
        return MV_CODEREADER_E_RESOURCE;
    }

    int nRet = m_pcMyCamera->Open(m_stDevList.pDeviceInfo[nIndex]);
    if (MV_CODEREADER_OK != nRet)
    {
        delete m_pcMyCamera;
        m_pcMyCamera = NULL;
        m_bRetState = nRet;
        return nRet;
    }

    m_bCreateDevice = TRUE;

    m_bRetState = nRet;

    m_bOpenDevice = TRUE;
    memcpy(&m_stDevInfo, m_stDevList.pDeviceInfo[nIndex], sizeof(MV_CODEREADER_DEVICE_INFO));
    m_pcMyCamera->RegisterExceptionCallBack(ReconnectDevice, this);

    return nRet;
}

// ch:关闭设备 | en:Close Device
State CBasicDemoDlg::CloseDevice(void)
{   
    int nRet = MV_CODEREADER_OK;
    if (FALSE == m_bCreateDevice || FALSE == m_bOpenDevice)
    {
        return MV_CODEREADER_E_CALLORDER;
    }
    m_bRetState = MV_CODEREADER_OK;

    if (m_pcMyCamera)
    {
        m_pcMyCamera->Close();
    }

    m_bOpenDevice = FALSE;
    m_bStartGrabbing = FALSE;

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

// ch:销毁设备 | en:Destroy device
State CBasicDemoDlg::DestroyHandle(void)
{
    m_bRetState = MV_CODEREADER_OK;
    int nRet = MV_CODEREADER_OK;

    if (m_pcMyCamera)
    {
        m_pcMyCamera->Close();
        delete m_pcMyCamera;
        m_pcMyCamera = NULL;
    }

    m_bCreateDevice = FALSE;
    m_bOpenDevice = FALSE;
    m_bStartGrabbing = FALSE;
    return MV_CODEREADER_OK;
}

// ch:获取触发模式 | en:Get Trigger Mode
State CBasicDemoDlg::GetTriggerMode(void)
{
    CString strFeature;
    unsigned int nEnumValue = 0;

    int nRet = m_pcMyCamera->GetEnumValue(TRIGGER_MODE, &nEnumValue);
    if (MV_CODEREADER_OK != nRet)
    {
        return nRet;
    }

    m_nTriggerMode = nEnumValue;
    if (MV_CODEREADER_TRIGGER_MODE_ON ==  m_nTriggerMode)
    {
        OnBnClickedTriggerModeRadio();
    }
    else if (MV_CODEREADER_TRIGGER_MODE_OFF == m_nTriggerMode)
    {
        OnBnClickedContinusModeRadio();
    }
    else
    {
        return MV_CODEREADER_E_SUPPORT;
    }
    UpdateData(FALSE);

    return MV_CODEREADER_OK;
}

// ch:设置触发模式 | en:Set Trigger Mode
State CBasicDemoDlg::SetTriggerMode(void)
{
    int nRet = m_pcMyCamera->SetEnumValue("TriggerMode", m_nTriggerMode);
    if (MV_CODEREADER_OK != nRet)
    {
        return nRet;
    }

    return MV_CODEREADER_OK;
}

// ch:获取触发模式 | en:Get Trigger Mode
State CBasicDemoDlg::SetTestMode(void)
{
    CString strFeature;
    unsigned int nEnumValue = 0;

    int nRet = m_pcMyCamera->SetEnumValue("RunningMode", 2);     //normal 0 ; test 1
    if (MV_CODEREADER_OK != nRet)
    {
        return nRet;
    }

    return MV_CODEREADER_OK;
}


// ch:按下查找设备按钮 | en:Click Find Device button:Enumeration
void CBasicDemoDlg::OnBnClickedEnumButton()
{
    m_bRetState = MV_CODEREADER_OK;
    CString strMsg;

    // ch:清除设备列表框中的信息 | en:Clear Device List Information
    m_ctrlDeviceCombo.ResetContent();

    // ch:初始化设备信息列表 | en:Device Information List Initialization
    memset(&m_stDevList, 0, sizeof(MV_CODEREADER_DEVICE_INFO_LIST));

    // ch:枚举子网内所有设备 | en:Enumerate all devices within subnet
    int nRet = CMyCamera::EnumDevices(&m_stDevList);
    if (MV_CODEREADER_OK != nRet)
    {
        m_bRetState = nRet;
        return;
    }

    // ch:将值加入到信息列表框中并显示出来 | en:Add value to the information list box and display
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
            Char2Wchar((char*)pDeviceInfo->SpecialInfo.stUsb3VInfo.chUserDefinedName, strWchar, 16);
            Wchar2char(strWchar, (char*)pDeviceInfo->SpecialInfo.stUsb3VInfo.chUserDefinedName);

            strMsg.Format(_T("[%d]UsbV3:  %s"), i, CStringW(pDeviceInfo->SpecialInfo.stUsb3VInfo.chUserDefinedName));
        }

        m_ctrlDeviceCombo.AddString(strMsg);
    }
    m_ctrlDeviceCombo.SetCurSel(CB_ERR);

    // ch:枚举到设备之后要显示出来 | en:Display the device after enumeration
    UpdateData(FALSE);
    
    if (0 == m_stDevList.nDeviceNum)
    {
        ShowErrorMsg(TEXT("No device"), 0);
        return;
    }
    // ch:将打开设备的按钮显现出来 | en:Turn on the Open button
    m_ctrlOpenButton.EnableWindow(TRUE);
    return;
}

// ch:按下打开设备按钮：打开设备 | en:Click Open button: Open Device
void CBasicDemoDlg::OnBnClickedOpenButton()
{
    int nRet = OpenDevice();
    if (MV_CODEREADER_OK != nRet)
    {
        ShowErrorMsg(TEXT("Open Fail"), nRet);
        return;
    }

    GetTriggerMode();

    SetTestMode();
    EnableWindowWhenOpenNotStart();


    return;
}

// ch:按下关闭设备按钮：关闭设备 | en:Click Close button: Close Device
void CBasicDemoDlg::OnBnClickedCloseButton()
{
    CloseDevice();
    DestroyHandle();
    EnableWindowWhenClose();
    m_ctrlOpenButton.EnableWindow(TRUE);
    m_ctrlSoftwareTriggerCheck.SetCheck(FALSE);
}

// ch:按下连续模式按钮 | en:Click Continues button
void CBasicDemoDlg::OnBnClickedContinusModeRadio()
{
    ((CButton *)GetDlgItem(IDC_CONTINUS_MODE_RADIO))->SetCheck(TRUE);
    ((CButton *)GetDlgItem(IDC_TRIGGER_MODE_RADIO))->SetCheck(FALSE);
    ((CButton *)GetDlgItem(IDC_SOFTWARE_TRIGGER_CHECK))->SetCheck(FALSE);
    ((CButton *)GetDlgItem(IDC_SOFTWARE_TRIGGER_CHECK))->EnableWindow(FALSE);
    ((CButton *)GetDlgItem(IDC_SOFTWARE_TRIGGER_ONCE_BUTTON))->EnableWindow(FALSE);
    ((CButton *)GetDlgItem(IDC_START_GRABBING_BUTTON))->EnableWindow(TRUE);
    ((CButton *)GetDlgItem(IDC_TRIGGER_MODE_RADIO))->EnableWindow(TRUE);
    ((CButton *)GetDlgItem(IDC_CONTINUS_MODE_RADIO))->EnableWindow(FALSE);


    m_nTriggerMode = MV_CODEREADER_TRIGGER_MODE_OFF;
    int nRet = SetTriggerMode();
    if (MV_CODEREADER_OK != nRet)
    {
        return;
    }

    return;
}

// ch:按下触发模式按钮 | en:Click Trigger Mode button
void CBasicDemoDlg::OnBnClickedTriggerModeRadio()
{
    ((CButton *)GetDlgItem(IDC_CONTINUS_MODE_RADIO))->SetCheck(FALSE);
    ((CButton *)GetDlgItem(IDC_TRIGGER_MODE_RADIO))->SetCheck(TRUE);
    ((CButton *)GetDlgItem(IDC_SOFTWARE_TRIGGER_CHECK))->EnableWindow(TRUE);
    ((CButton *)GetDlgItem(IDC_SOFTWARE_TRIGGER_ONCE_BUTTON))->EnableWindow(FALSE);
    ((CButton *)GetDlgItem(IDC_START_GRABBING_BUTTON))->EnableWindow(TRUE);
    ((CButton *)GetDlgItem(IDC_TRIGGER_MODE_RADIO))->EnableWindow(FALSE);
    ((CButton *)GetDlgItem(IDC_CONTINUS_MODE_RADIO))->EnableWindow(TRUE);


    m_nTriggerMode = MV_CODEREADER_TRIGGER_MODE_ON;
    int nRet = SetTriggerMode();
    if (MV_CODEREADER_OK != nRet)
    {
        ShowErrorMsg(TEXT("Set Trigger Mode Fail"), nRet);
        return;
    }

    return;
}

// ch:按下开始采集按钮 | en:Click Start button
void CBasicDemoDlg::OnBnClickedStartGrabbingButton()
{
     CString cstrInfo;
    if (FALSE == m_bCreateDevice || FALSE == m_bOpenDevice || TRUE == m_bStartGrabbing)
    {
        return;
    }

    int nRet = MV_CODEREADER_OK;

    if (NULL != m_pcMyCamera)
    {
        nRet = m_pcMyCamera->StartGrabbing();
        if (nRet == MV_CODEREADER_OK)
        {
            m_bStartGrabbing = TRUE;
            m_pcMyCamera->Process(m_hwndDisplay, m_bStartGrabbing );

        }
    }
    else
    {
        m_bRetState = nRet;
        return ;
    }

    if (MV_CODEREADER_OK != nRet)
    {
        return ;
    }
    EnableWindowWhenStart();

    return;
}

// ch:按下结束采集按钮 | en:Click Stop button
void CBasicDemoDlg::OnBnClickedStopGrabbingButton()
{
    if (FALSE == m_bCreateDevice || FALSE == m_bOpenDevice || FALSE == m_bStartGrabbing)
    {
        return;
    }

    int nRet = MV_CODEREADER_OK;
    if (NULL != m_pcMyCamera)
    {
        nRet = m_pcMyCamera->StopGrabbing();
    }
    else
    {
        m_bRetState = nRet;
        return ;
    }

    if (MV_CODEREADER_OK != nRet)
    {
        return ;
    }
    m_bStartGrabbing = FALSE;

    EnableWindowWhenStop();

    return;
}


// ch:右上角退出 | en:Exit from upper right corner
void CBasicDemoDlg::OnClose()
{
    PostQuitMessage(0);
    DestroyHandle();
    CDialog::OnClose();
}

void __stdcall CBasicDemoDlg::ReconnectDevice(unsigned int nMsgType, void* pUser)
{
    if(nMsgType == MV_CODEREADER_EXCEPTION_DEV_DISCONNECT)
    {
        CBasicDemoDlg* pThis = (CBasicDemoDlg*)pUser;

        pThis->EnableWindowWhenClose();
        pThis->m_ctrlOpenButton.EnableWindow(TRUE);
        if (pThis->m_bOpenDevice)
        {
            pThis->m_pcMyCamera->Close();

            BOOL bConnected = FALSE;
            while (1)
            {
                int nRet = MV_CODEREADER_OK;
                nRet = pThis->m_pcMyCamera->Open(&pThis->m_stDevInfo);
                if (MV_CODEREADER_OK == nRet)
                {
                    pThis->m_pcMyCamera->RegisterExceptionCallBack(ReconnectDevice, pUser);
                    bConnected = TRUE;
                    pThis->EnableWindowWhenOpenNotStart();
                    break;
                }
                else
                {
                    Sleep(100);
                }
            }

            if (bConnected && pThis->m_bStartGrabbing)
            {
                Sleep(500);
                pThis->m_pcMyCamera->StartGrabbing();
                pThis->EnableWindowWhenStart();
            }
        }
    }
}

BOOL CBasicDemoDlg::PreTranslateMessage(MSG* pMsg)
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


// ch:软触发一次 | en: Software Trigger Once
void CBasicDemoDlg::OnBnClickedSoftwareTriggerOnceButton()
{
    int nRet = MV_CODEREADER_OK;
    CString cstrInfo;

    // 软触发
    nRet = m_pcMyCamera->SetCommandValue("TriggerSoftware");
    if (MV_CODEREADER_OK != nRet)
    {
        cstrInfo.Format(_T("Set Software Once failed! err code:%#x"), nRet);
        MessageBox(cstrInfo);
        return;
    }
}

void CBasicDemoDlg::OnBnClickedSoftwareTriggerCheck()
{
    int nRet = MV_CODEREADER_OK;

    if(m_ctrlSoftwareTriggerCheck.GetCheck())
    {
        nRet = m_pcMyCamera->SetEnumValue("TriggerSource", MV_CODEREADER_TRIGGER_SOURCE_SOFTWARE);
        if (MV_CODEREADER_OK != nRet)
        {
            ShowErrorMsg(TEXT("Set Software Mode fialed"), nRet);
            m_ctrlSoftwareTriggerCheck.SetCheck(FALSE);
            return ;
        }
    }
    else
    {
        nRet = m_pcMyCamera->SetEnumValue("TriggerSource", MV_CODEREADER_TRIGGER_SOURCE_LINE0);
        if (MV_CODEREADER_OK != nRet)
        {
            ShowErrorMsg(TEXT("Set TriggerSource fialed"), nRet);
            m_ctrlSoftwareTriggerCheck.SetCheck(TRUE);
        }
    }
    
    
}
