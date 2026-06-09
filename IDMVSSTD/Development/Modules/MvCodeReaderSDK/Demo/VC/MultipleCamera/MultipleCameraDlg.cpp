
// MultipleCameraDlg.cpp : implementation file
// 

#include "stdafx.h"
#include "MultipleCamera.h"
#include "MultipleCameraDlg.h"

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
public:
    afx_msg void OnEnChangeExpouseEdit();
};

CAboutDlg::CAboutDlg() : CDialog(CAboutDlg::IDD)
{
}

void CAboutDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
}

BEGIN_MESSAGE_MAP(CAboutDlg, CDialog)
    //ON_EN_CHANGE(IDC_EXPOUSE_EDIT, &CAboutDlg::OnEnChangeExpouseEdit)
END_MESSAGE_MAP()


// CMultipleCameraDlg dialog
CMultipleCameraDlg::CMultipleCameraDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CMultipleCameraDlg::IDD, pParent)
    // ch:按钮对应的变量初始化 | en:Variable initialization corresponding to button
    , m_nOnlineNumEdit(0)
    , m_nUseNumEdit(0)
    // ch:自己定义的变量初始化 | en:User defined variable*
    , m_bCreateDevice(0)                          // 是否创建设备 | en:Create
    , m_bOpenDevice(FALSE)                        // 是否打开 | en:Open
    , m_bStartGrabbing(FALSE)                     // 是否开始抓图 | en:Start grabbing
    , m_nTriggerMode(TRIGGER_NOT_SET)             // 触发模式 | en:Trigger mode
    , m_serialNum1(_T(""))
    , m_serialNum2(_T(""))
    , m_serialNum3(_T(""))
    , m_serialNum4(_T(""))
{
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);
    memset(&m_stDevList, 0, sizeof(MV_CODEREADER_DEVICE_INFO_LIST));
    for (int i =0; i< 4; i++)
    {
        memset(&m_stCamSerial[i], 0, sizeof(MV_CODEREADER_CODEREADER_SERIAL));
    }
    
    
    for (int i = 0; i < MAX_DEVICE_NUM; i++)
    {
        m_hwndDisplay[i] = NULL;
        m_cwmdDisplay[i] = NULL;
        m_pcMyCamera[i] = NULL;
        m_hGetOneFrameHandle[i] = NULL;
    }
}

void CMultipleCameraDlg::DoDataExchange(CDataExchange* pDX)
{
    CDialog::DoDataExchange(pDX);
    DDX_Text(pDX, IDC_ONLINE_NUM_EDIT, m_nOnlineNumEdit);
  
    DDX_Control(pDX, IDC_CONTINUS_MODE_RADIO, m_bContinusModeRadio);
    DDX_Control(pDX, IDC_TRIGGER_MODE_RADIO, m_bTriggerModeRadio);

    DDX_Text(pDX, IDC_EDIT1, m_serialNum1);
    DDX_Text(pDX, IDC_EDIT2, m_serialNum2);
    DDX_Text(pDX, IDC_EDIT3, m_serialNum3);
    DDX_Text(pDX, IDC_EDIT4, m_serialNum4);
}

BEGIN_MESSAGE_MAP(CMultipleCameraDlg, CDialog)
	ON_WM_SYSCOMMAND()
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	// }}AFX_MSG_MAP
    ON_BN_CLICKED(IDC_INIT_DEVICE_BUTTON, &CMultipleCameraDlg::OnBnClickedInitDeviceButton)
    //ON_BN_CLICKED(IDC_SET_EXPOUSE_GAIN_BUTTON, &CMultipleCameraDlg::OnBnClickedSetExpouseGainButton)
    ON_BN_CLICKED(IDC_START_GRABBING_BUTTON, &CMultipleCameraDlg::OnBnClickedStartGrabbingButton)
    ON_BN_CLICKED(IDC_STOP_GRABBING_BUTTON, &CMultipleCameraDlg::OnBnClickedStopGrabbingButton)
    ON_BN_CLICKED(IDC_SAVE_IMAGE_BUTTON, &CMultipleCameraDlg::OnBnClickedSaveImageButton)
    ON_BN_CLICKED(IDC_CONTINUS_MODE_RADIO, &CMultipleCameraDlg::OnBnClickedContinusModeRadio)
    ON_BN_CLICKED(IDC_TRIGGER_MODE_RADIO, &CMultipleCameraDlg::OnBnClickedTriggerModeRadio)
    ON_BN_CLICKED(IDC_SOFTWARE_MODE_CHECK, &CMultipleCameraDlg::OnBnClickedSoftwareModeCheck)
    ON_BN_CLICKED(IDC_HARDWARE_MODE_CHECK, &CMultipleCameraDlg::OnBnClickedHardwareModeCheck)
    ON_BN_CLICKED(IDC_SOFTWARE_ONCE_BUTTON, &CMultipleCameraDlg::OnBnClickedSoftwareOnceButton)
    ON_BN_CLICKED(IDC_CLOSE_DEVICE_BUTTON, &CMultipleCameraDlg::OnBnClickedCloseDeviceButton)
    ON_WM_CLOSE()

END_MESSAGE_MAP()

// CMultipleCameraDlg message handlers

BOOL CMultipleCameraDlg::OnInitDialog()
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

    GdiplusStartupInput gdiplusStartupInput;
    ULONG_PTR gdiplusToken;
    //初始化GDI+
    GdiplusStartup(&gdiplusToken, &gdiplusStartupInput, NULL);

    // Set the icon for this dialog.  The framework does this automatically
    //  when the application's main window is not a dialog
	SetIcon(m_hIcon, TRUE);			// Set big icon
	SetIcon(m_hIcon, FALSE);		// Set small icon

	int nRet = DisplayWindowInitial();
    if (MV_CODEREADER_OK != nRet)
    {
        return nRet;
    }

    nRet = HwndHandleInit();
    if (MV_CODEREADER_OK != nRet)
    {
        return nRet;
    }

    m_bContinusModeRadio.EnableWindow(FALSE);
    m_bTriggerModeRadio.EnableWindow(FALSE);
    GetDlgItem(IDC_CLOSE_DEVICE_BUTTON)->EnableWindow(FALSE);
	return TRUE;  // return TRUE  unless you set the focus to a control
}

void CMultipleCameraDlg::OnSysCommand(UINT nID, LPARAM lParam)
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

void CMultipleCameraDlg::OnPaint()
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
HCURSOR CMultipleCameraDlg::OnQueryDragIcon()
{
	return static_cast<HCURSOR>(m_hIcon);
}


// Get window handle  
int CMultipleCameraDlg::HwndHandleInit(void)
{
    m_cwmdDisplay[0] = GetDlgItem(IDC_DISPLAY1_STATIC);
    m_cwmdDisplay[1] = GetDlgItem(IDC_DISPLAY2_STATIC);
    m_cwmdDisplay[2] = GetDlgItem(IDC_DISPLAY3_STATIC);
    m_cwmdDisplay[3] = GetDlgItem(IDC_DISPLAY4_STATIC);
    if (NULL == m_cwmdDisplay[0] || 
        NULL == m_cwmdDisplay[1] || 
        NULL == m_cwmdDisplay[2] ||
        NULL == m_cwmdDisplay[3])
    {
        return STATUS_ERROR;
    }

    for (int i = 0; i < MAX_DEVICE_NUM; i++)
    {
        m_hwndDisplay[i] = m_cwmdDisplay[i]->GetSafeHwnd();
        if (NULL == m_hwndDisplay[i])
        {
            return STATUS_ERROR;
        }
    }
    return MV_CODEREADER_OK;
}

// ch:判断字符类型 | en:str type
bool CMultipleCameraDlg::IsStrUTF8(const char* pBuffer, int size)
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
bool CMultipleCameraDlg::Char2Wchar(const char *pStr, wchar_t *pOutWStr, int nOutStrSize)
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
bool CMultipleCameraDlg::Wchar2char(wchar_t *pOutWStr, char *pStr)
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

// ch:显示框初始化,最开始的初始化 | en:Window initialization
int CMultipleCameraDlg::DisplayWindowInitial(void)
{
    //EnableWindowWhenClose();
    EnableControls(TRUE);

    int nRet = CMyCamera::EnumDevices(&m_stDevList);
    if (MV_CODEREADER_OK != nRet)
    {
        return STATUS_ERROR;
    }
    m_nOnlineNumEdit = m_stDevList.nDeviceNum;
    UpdateData(FALSE);
    return nRet;
}

// ch:按钮使能 | en:Enable control
int CMultipleCameraDlg::EnableControls(BOOL bIsCameraReady)
{
    // ch:相机初始化区域 | en:Initialization area
    GetDlgItem(IDC_ONLINE_NUM_EDIT)->EnableWindow(FALSE);                                        // 在线个数 | en:Online number
   
    // ch:采集控制区域 | en:Acquisition control area
    GetDlgItem(IDC_START_GRABBING_BUTTON)->EnableWindow(m_bStartGrabbing ? FALSE : m_bOpenDevice);// 开始采集 | en:Start grabbing
    GetDlgItem(IDC_STOP_GRABBING_BUTTON)->EnableWindow(m_bStartGrabbing ? m_bOpenDevice : FALSE); // 停止采集 | en:Stop grabbing
    GetDlgItem(IDC_SAVE_IMAGE_BUTTON)->EnableWindow(m_bStartGrabbing && m_nTriggerMode == TRIGGER_OFF ? m_bOpenDevice : FALSE);    // 保存图片 | en:Save image
    GetDlgItem(IDC_SOFTWARE_MODE_CHECK)->EnableWindow(m_nTriggerMode ? m_bOpenDevice : FALSE);   // 软触发采集 | en:Software trigger
    GetDlgItem(IDC_HARDWARE_MODE_CHECK)->EnableWindow(m_nTriggerMode ? m_bOpenDevice : FALSE);   // 硬触发采集 | en:Hardware trigger
    GetDlgItem(IDC_SOFTWARE_ONCE_BUTTON)->EnableWindow(m_nTriggerSource == SOFTWAREMODE && m_nTriggerMode == TRIGGER_ON ? m_bOpenDevice : FALSE);// 软触发一次 | en:Software trigger once
    GetDlgItem(IDC_CONTINUS_MODE_RADIO)->EnableWindow(m_bOpenDevice ? m_bOpenDevice : FALSE);     // 连续模式 | en:Continuous mode
    GetDlgItem(IDC_TRIGGER_MODE_RADIO)->EnableWindow(m_bOpenDevice ? m_bOpenDevice : FALSE);      // 触发模式 | en:Trigger mode

    // ch:采集帧数、丢帧和图像显示控制区域 | en:Display control area
    if (!m_bOpenDevice)
    {
       // GetDlgItem(IDC_LOST_FRAME4_EDIT)->EnableWindow(FALSE);
    }
 
    return MV_CODEREADER_OK;
}

// ch:按下初始化按钮时的按钮颜色 | en:Button color after press initialization
int CMultipleCameraDlg::EnableWindowWhenInitCamera(void)
{
    EnableControls(TRUE);
    UpdateData(FALSE);

    return MV_CODEREADER_OK;
}

// ch:显示错误信息 | en:Show error message
void CMultipleCameraDlg::ShowErrorMsg(CString csMessage, int nErrorNum)
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
int CMultipleCameraDlg::OpenDevice(void)
{
    if (TRUE == m_bOpenDevice)
    {
        return MV_CODEREADER_E_CALLORDER;
    }

    // ch:参数检测 | en:Parameter check
    int nCameUseNum = 0;
    USES_CONVERSION;  
    char * pSerialNum1 = T2A(m_serialNum1);
    int nSerialLen1 = wcslen(m_serialNum1);
    if (nSerialLen1 != 0)
    {
        m_stCamSerial[nCameUseNum].nIndex = nCameUseNum;
        m_stCamSerial[nCameUseNum].pSerial = pSerialNum1;
        nCameUseNum++;
    }

    char * pSerialNum2 = T2A(m_serialNum2);
    int nSerialLen2 = wcslen(m_serialNum2);
    if (nSerialLen2 != 0)
    {
        m_stCamSerial[nCameUseNum].nIndex = nCameUseNum;
        m_stCamSerial[nCameUseNum].pSerial = pSerialNum2;
        nCameUseNum++;
    }

    char * pSerialNum3 = T2A(m_serialNum3);
    int nSerialLen3 = wcslen(m_serialNum3);
    if (nSerialLen3 != 0)
    {
        m_stCamSerial[nCameUseNum].nIndex = nCameUseNum;
        m_stCamSerial[nCameUseNum].pSerial = pSerialNum3;
        nCameUseNum++;
    }

    char * pSerialNum4 = T2A(m_serialNum4);
    int nSerialLen4 = wcslen(m_serialNum4);
    if (nSerialLen4 != 0)
    {
        m_stCamSerial[nCameUseNum].nIndex = nCameUseNum;
        m_stCamSerial[nCameUseNum].pSerial = pSerialNum4;
        nCameUseNum++;
    }

    if (nCameUseNum == 0)
    {
        ShowErrorMsg(TEXT("Please add camera serialNum"), 0);
        return MV_CODEREADER_E_NODATA;
    }
    // ch:添加序列号的相机数目 | en:Camera Serial num
    int nCanOpenDeviceNum = 0;
    int nRet = MV_CODEREADER_OK;

    for (unsigned int i = 0, j = 0; j < nCameUseNum; j++, i++)
    {
        m_pcMyCamera[i] = new  CMyCamera;
        if (NULL == m_pcMyCamera[i])
        {
            ShowErrorMsg(TEXT("Please create camera failed"), 0);
            return MV_CODEREADER_E_RESOURCE;
        }


        m_pcMyCamera[i]->m_pBufForDriver = NULL;
        m_pcMyCamera[i]->m_pBufForSaveImage = NULL;
        m_pcMyCamera[i]->m_nBufSizeForDriver = 0;
        m_pcMyCamera[i]->m_nBufSizeForSaveImage = 0;

        nRet = m_pcMyCamera[i]->Open(m_stCamSerial[j].pSerial);
        if (MV_CODEREADER_OK != nRet)
        {
            delete m_pcMyCamera[i];
            m_pcMyCamera[i] = NULL;
            i--;
            ShowErrorMsg(TEXT("Warning: Get Camera control privilige fail!"), nRet);
            return MV_CODEREADER_E_RESOURCE;
        }
        else
        {
            nCanOpenDeviceNum++;
        }
    }

    m_nUseNumEdit = nCanOpenDeviceNum;
    m_bCreateDevice = TRUE;
    m_bOpenDevice = TRUE;

    UpdateData(FALSE);
    return MV_CODEREADER_OK;
}

// ch:设置触发模式 | en:Set trigger mode
int CMultipleCameraDlg::SetTriggerMode(void)
{
    if (FALSE == m_bOpenDevice)
    {        
        return STATUS_ERROR;
    }
    int nRet = MV_CODEREADER_OK;
    int i = 0;
    CString cstrInfo;

    for (i = 0; i < m_nUseNumEdit; i++)
    {
        nRet = m_pcMyCamera[i]->SetEnumValue("TriggerMode", m_nTriggerMode);
        if (MV_CODEREADER_OK != nRet)
        {
            cstrInfo.Format(_T("Set CamNum[%d] Trigger[%d] on Mode failed! err code:%#x"), i, m_nTriggerMode,nRet);
            MessageBox(cstrInfo);
            return nRet;
        }
    }
    
    return MV_CODEREADER_OK;
}


// ch:设置触发模式 | en:Set trigger mode
int CMultipleCameraDlg::SetRunningModeTest(void)
{
    if (FALSE == m_bOpenDevice)
    {        
        return STATUS_ERROR;
    }
    int nRet = MV_CODEREADER_OK;
    int i = 0;

    for (i = 0; i < m_nUseNumEdit; i++)
    {
        nRet = m_pcMyCamera[i]->SetEnumValue("RunningMode", 2);       //test 1 ; normal 0
    }

    return MV_CODEREADER_OK;
}



// ch:设置曝光时间 | en:Set exposure time
int CMultipleCameraDlg::SetExposureMode(void)
{
    if (FALSE == m_bOpenDevice)
    {        
        return STATUS_ERROR;
    }
    int nRet = MV_CODEREADER_OK;
    int i = 0;

    for (i = 0; i < m_nUseNumEdit; i++)
    {
        nRet = m_pcMyCamera[i]->SetEnumValue("ExposureMode", 0);
    }
    return MV_CODEREADER_OK;
}

// ch:设置增益 | en:Set gain
int CMultipleCameraDlg::SetGain(void)
{
    if (FALSE == m_bOpenDevice)
    {        
        return STATUS_ERROR;
    }
    int nRet = MV_CODEREADER_OK;
    int i = 0;

    for (i = 0; i < m_nUseNumEdit; i++)
    {
        nRet = m_pcMyCamera[i]->SetEnumValue("GainAuto", 0);
    }
    return MV_CODEREADER_OK;
}

// ch:设置触发源 | en:Set trigger source
int CMultipleCameraDlg::SetTriggerSource(void)
{
    if (FALSE == m_bOpenDevice)
    {
        return STATUS_ERROR;
    }
    int nRet = MV_CODEREADER_OK;
    int i = 0;
    CString cstrInfo;

    for (i = 0; i < m_nUseNumEdit; i++)
    {
        nRet = m_pcMyCamera[i]->SetEnumValue("TriggerSource", m_nTriggerSource);
        if (MV_CODEREADER_OK != nRet)
        {
            cstrInfo.Format(_T("Set TriggerSource Mode fialed! err code:%#x"), nRet);
            MessageBox(cstrInfo);
            return nRet;
        }
    }
    return MV_CODEREADER_OK;
}

// ch:软触发一次 | en:Software trigger once
int CMultipleCameraDlg::DoSoftwareOnce(void)
{
    if (FALSE == m_bOpenDevice)
    {        
        return STATUS_ERROR;
    }
    int nRet = MV_CODEREADER_OK;
    int i = 0;

    for (i = 0; i < m_nUseNumEdit; i++)
    {
        nRet = m_pcMyCamera[i]->CommandExecute("TriggerSoftware");
        if (MV_CODEREADER_OK != nRet)
        {
            return nRet;
        }
    }

    return nRet;
}

// ch:开始采集 | en:Start grabbing
int CMultipleCameraDlg::StartGrabbing(void)
{
    if (FALSE == m_bOpenDevice || TRUE == m_bStartGrabbing)
    {        
        return STATUS_ERROR;
    }
    int nRet = MV_CODEREADER_OK;
    int i = 0;
    

    for (i = 0; i < m_nUseNumEdit; i++)
    {
       nRet = m_pcMyCamera[i]->StartGrabbing();
       if (MV_CODEREADER_OK == nRet)
        {
            m_bStartGrabbing = TRUE;
            m_pcMyCamera[i]->Process(m_hwndDisplay[i], m_bStartGrabbing);
        }
        UpdateData(FALSE);
    }

    return MV_CODEREADER_OK;
}

// ch:结束采集 | en:Stop grabbing
int CMultipleCameraDlg::StopGrabbing(void)
{
    int nRet = MV_CODEREADER_OK;
    if (FALSE == m_bOpenDevice || FALSE == m_bStartGrabbing)
    {        
        return STATUS_ERROR;
    }

    for (int i = 0; i < m_nUseNumEdit; i++)
    {
        nRet = m_pcMyCamera[i]->StopGrabbing();
    }

    m_bStartGrabbing = FALSE;
    for (int i = 0; i < m_nUseNumEdit; i++)
    {
        if (m_hGetOneFrameHandle[i])
        {
            CloseHandle(m_hGetOneFrameHandle[i]);
            m_hGetOneFrameHandle[i] = NULL;
        }
    }
    return nRet;
}

char* CMultipleCameraDlg::GetCurrentProGramPath(char* pFilePath, int nSize)
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

// ch:保存图片 | en:Save image
int CMultipleCameraDlg::SaveImage(void)
{
    if (FALSE == m_bStartGrabbing)
    {
        return STATUS_ERROR;
    }
    // ch:获取1张图 | en:Get one frame
    MV_CODEREADER_IMAGE_OUT_INFO_EX2 stImageInfo = {0};
    memset(&stImageInfo, 0, sizeof(MV_CODEREADER_IMAGE_OUT_INFO_EX2));
    unsigned int nDataLen = 0;
    int nRet = MV_CODEREADER_OK;
    for (int i = 0; i < m_nUseNumEdit; i++)
    {
        m_pcMyCamera[i]->m_pBufForDriver = NULL;

        nRet = m_pcMyCamera[i]->GetOneFrameTimeout(&m_pcMyCamera[i]->m_pBufForDriver, &nDataLen,  &stImageInfo, 1000);
        if (MV_CODEREADER_OK == nRet)
        {
            // ch:BMP图片大小：width * height * 3 + 2048(预留BMP头大小)
            m_pcMyCamera[i]->m_nBufSizeForSaveImage = stImageInfo.nWidth * stImageInfo.nHeight * 3 + 2048;

            // ch:设置对应的相机参数 | en:Set camera parameter
            MV_CODEREADER_SAVE_IMAGE_PARAM_EX stParam = {0};
            stParam.enImageType = MV_CODEREADER_Image_Bmp;
            stParam.enPixelType = stImageInfo.enPixelType;                  // 相机对应的像素格式 | en:Pixel format
            stParam.nBufferSize = m_pcMyCamera[i]->m_nBufSizeForSaveImage;  // 存储节点的大小 | en:Buffer node size
            stParam.nWidth      = stImageInfo.nWidth;                       // 相机对应的宽 | en:Width
            stParam.nHeight     = stImageInfo.nHeight;                      // 相机对应的高 | en:Height
            stParam.nDataLen    = stImageInfo.nFrameLen;
            stParam.pData       = m_pcMyCamera[i]->m_pBufForDriver;

            nRet = m_pcMyCamera[i]->SaveImage(&stParam);
            if(MV_CODEREADER_OK != nRet)
            {
                ShowErrorMsg(TEXT("One camera fail to save image"), nRet);
                return nRet;
            }
            char chImageName[IMAGE_NAME_LEN] = {0};
            char chCurDir[MAX_PATH] = {0};
            GetCurrentProGramPath(chCurDir, MAX_PATH);

            if (MV_CODEREADER_Image_Bmp == stParam.enImageType)
            {
                sprintf_s(chImageName, IMAGE_NAME_LEN, "%s\\Image_w%d_h%d_fn%03d.bmp", chCurDir, stImageInfo.nWidth, stImageInfo.nHeight, stImageInfo.nFrameNum);
            }
            else if (MV_CODEREADER_Image_Jpeg == stParam.enImageType)
            {
                sprintf_s(chImageName, IMAGE_NAME_LEN, "%s\\Image_w%d_h%d_fn%03d.jpg", chCurDir, stImageInfo.nWidth, stImageInfo.nHeight, stImageInfo.nFrameNum);
            }

            FILE* fp = fopen(chImageName, "wb");
            if (NULL == fp)
            {
                ShowErrorMsg(TEXT("write image failed, maybe you have no privilege"), 0);
                return STATUS_ERROR;
            }
            fwrite(stParam.pImageBuffer, 1, stParam.nImageLen, fp);
            fclose(fp);
        }
        else
        {
            ShowErrorMsg(TEXT("No data, can not save image"), nRet);
        }
    }
    return nRet;
}


// ch:关闭设备 | en:Close device
int CMultipleCameraDlg::CloseDevice(void)
{
    int nRet = MV_CODEREADER_OK;

    for (int i = 0; i < MAX_DEVICE_NUM; i++)
    {
        if (m_pcMyCamera[i])
        {
            m_pcMyCamera[i]->StopGrabbing();
            m_pcMyCamera[i]->Close();
            delete m_pcMyCamera[i];
            m_pcMyCamera[i] = NULL;
        }
    }

    m_bStartGrabbing = FALSE;
    m_bOpenDevice = FALSE;
    m_nUseNumEdit = 0;
    return nRet;
}

// ch:销毁句柄 | en:Destroy handle
int CMultipleCameraDlg::DestroyDevice(void)
{
    for (int i = 0; i < MAX_DEVICE_NUM; i++)
    {
        if (m_pcMyCamera[i] && m_pcMyCamera[i]->m_pBufForSaveImage)
        {
            free(m_pcMyCamera[i]->m_pBufForSaveImage);
            m_pcMyCamera[i]->m_pBufForSaveImage = NULL;
        }

        if (m_pcMyCamera[i])
        {
            delete m_pcMyCamera[i];
            m_pcMyCamera[i] = NULL;
        }
    }

    EnableControls(TRUE);
    m_bCreateDevice = FALSE;
    return MV_CODEREADER_OK;
}

// ch:初始化相机，有打开相机操作 | en:Initialzation, include opening device
void CMultipleCameraDlg::OnBnClickedInitDeviceButton()
{
    UpdateData(TRUE);

    CloseDevice();

    int nRet = OpenDevice();
    if (MV_CODEREADER_OK != nRet)
    {
        CloseDevice(); // 关闭所有打开的设备

        EnableControls(FALSE);
        UpdateData(FALSE);
        return;
    }

    EnableWindowWhenInitCamera();
    OnBnClickedContinusModeRadio();

    GetDlgItem(IDC_CLOSE_DEVICE_BUTTON)->EnableWindow(TRUE);
    GetDlgItem(IDC_INIT_DEVICE_BUTTON)->EnableWindow(FALSE);
    GetDlgItem(IDC_EDIT1)->EnableWindow(FALSE);
    GetDlgItem(IDC_EDIT2)->EnableWindow(FALSE);
    GetDlgItem(IDC_EDIT3)->EnableWindow(FALSE);
    GetDlgItem(IDC_EDIT4)->EnableWindow(FALSE);
    if(((CButton *)GetDlgItem(IDC_CONTINUS_MODE_RADIO))->GetCheck())
    {
        GetDlgItem(IDC_START_GRABBING_BUTTON)->EnableWindow(TRUE);
    }
    else
    {
        GetDlgItem(IDC_START_GRABBING_BUTTON)->EnableWindow(FALSE);
        GetDlgItem(IDC_SOFTWARE_MODE_CHECK)->EnableWindow(TRUE);
        GetDlgItem(IDC_HARDWARE_MODE_CHECK)->EnableWindow(TRUE);
    }

    return;
}

// ch:设置曝光和增益 | en:Set exposure and gain
void CMultipleCameraDlg::OnBnClickedSetExpouseGainButton()
{
    UpdateData(TRUE);
    bool bIsSetOK = true;

    int nRet = SetExposureMode();
    if (MV_CODEREADER_OK != nRet)
    {
        bIsSetOK = false;
        ShowErrorMsg(TEXT("Set exposure fail"), nRet);
    }
    nRet = SetGain();
    if (MV_CODEREADER_OK != nRet)
    {
        bIsSetOK = false;
        ShowErrorMsg(TEXT("Set gain fail"), nRet);
    }
    if (true == bIsSetOK)
    {
        ShowErrorMsg(TEXT("Set exposure and gain succeed"), nRet);
    }
    return;
}

// ch:开始抓图 | en:Start grabbing
void CMultipleCameraDlg::OnBnClickedStartGrabbingButton()
{
    UpdateData(TRUE);

    int nRet = StartGrabbing();
    if (MV_CODEREADER_OK != nRet)
    {
        ShowErrorMsg(TEXT("Start grabbing fail"), nRet);
    }

    m_bStartGrabbing = true;
    //EnableControls(TRUE);
    GetDlgItem(IDC_START_GRABBING_BUTTON)->EnableWindow(FALSE);
    GetDlgItem(IDC_STOP_GRABBING_BUTTON)->EnableWindow(TRUE);
    GetDlgItem(IDC_SOFTWARE_MODE_CHECK)->EnableWindow(FALSE);
    GetDlgItem(IDC_HARDWARE_MODE_CHECK)->EnableWindow(FALSE);

    m_bContinusModeRadio.EnableWindow(FALSE);
    m_bTriggerModeRadio.EnableWindow(FALSE);
    if(((CButton *)GetDlgItem(IDC_SOFTWARE_MODE_CHECK))->GetCheck())
    {
        GetDlgItem(IDC_SOFTWARE_ONCE_BUTTON)->EnableWindow(TRUE);
    }
    GetDlgItem(IDC_SAVE_IMAGE_BUTTON)->EnableWindow(m_bStartGrabbing && m_nTriggerMode == TRIGGER_OFF ? m_bOpenDevice : FALSE);    // 保存图片 | en:Save image

    return;
}

// ch:结束抓图 | en:Stop grabbing
void CMultipleCameraDlg::OnBnClickedStopGrabbingButton()
{
    int nRet = StopGrabbing();
    if (MV_CODEREADER_OK != nRet)
    {
        ShowErrorMsg(TEXT("Stop Grabbing Failed"), 0);
    }
    //EnableControls(TRUE);
    GetDlgItem(IDC_SOFTWARE_ONCE_BUTTON)->EnableWindow(FALSE);

    GetDlgItem(IDC_START_GRABBING_BUTTON)->EnableWindow(TRUE);
    GetDlgItem(IDC_STOP_GRABBING_BUTTON)->EnableWindow(FALSE);
    GetDlgItem(IDC_SAVE_IMAGE_BUTTON)->EnableWindow(FALSE);

    GetDlgItem(IDC_CONTINUS_MODE_RADIO)->EnableWindow(m_bOpenDevice ? m_bOpenDevice : FALSE);     // 连续模式 | en:Continuous mode
    GetDlgItem(IDC_TRIGGER_MODE_RADIO)->EnableWindow(m_bOpenDevice ? m_bOpenDevice : FALSE);      // 触发模式 | en:Trigger mode
    if(((CButton *)GetDlgItem(IDC_TRIGGER_MODE_RADIO))->GetCheck())
    {
        GetDlgItem(IDC_TRIGGER_MODE_RADIO)->EnableWindow(FALSE);
        if(((CButton *)GetDlgItem(IDC_SOFTWARE_MODE_CHECK))->GetCheck())
        {
            GetDlgItem(IDC_HARDWARE_MODE_CHECK)->EnableWindow(TRUE);
        }
        else
        {
            GetDlgItem(IDC_SOFTWARE_MODE_CHECK)->EnableWindow(TRUE);
        }
    }
    else
    {
        GetDlgItem(IDC_CONTINUS_MODE_RADIO)->EnableWindow(FALSE);
    }

}

// ch:保存图片 | en:Save image
void CMultipleCameraDlg::OnBnClickedSaveImageButton()
{
    int nRet = SaveImage();
    if (MV_CODEREADER_OK == nRet)
    {
        ShowErrorMsg(TEXT("Save Image Succeed"), 0);
    }

    return;
}

// ch:设置连续模式 | en:Set continuous mode
void CMultipleCameraDlg::OnBnClickedContinusModeRadio()
{
    ((CButton *)GetDlgItem(IDC_CONTINUS_MODE_RADIO))->SetCheck(TRUE);
    ((CButton *)GetDlgItem(IDC_TRIGGER_MODE_RADIO))->SetCheck(FALSE);

    SetRunningModeTest();

    m_nTriggerMode = TRIGGER_OFF;
    int nRet = SetTriggerMode();
    if (MV_CODEREADER_OK != nRet)
    {
        ((CButton *)GetDlgItem(IDC_CONTINUS_MODE_RADIO))->SetCheck(FALSE);
        ((CButton *)GetDlgItem(IDC_TRIGGER_MODE_RADIO))->SetCheck(TRUE);
        return;
    }
    GetDlgItem(IDC_CONTINUS_MODE_RADIO)->EnableWindow(FALSE);
    GetDlgItem(IDC_TRIGGER_MODE_RADIO)->EnableWindow(TRUE);

    GetDlgItem(IDC_SOFTWARE_MODE_CHECK)->EnableWindow(FALSE);
    GetDlgItem(IDC_HARDWARE_MODE_CHECK)->EnableWindow(FALSE);
    ((CButton *)GetDlgItem(IDC_SOFTWARE_MODE_CHECK))->SetCheck(FALSE);
    ((CButton *)GetDlgItem(IDC_HARDWARE_MODE_CHECK))->SetCheck(FALSE);
    GetDlgItem(IDC_START_GRABBING_BUTTON)->EnableWindow(TRUE);
    GetDlgItem(IDC_SOFTWARE_ONCE_BUTTON)->EnableWindow(FALSE);
    if (m_bStartGrabbing)
    {
        GetDlgItem(IDC_SAVE_IMAGE_BUTTON)->EnableWindow(TRUE);
    }
    UpdateData(TRUE);
    UpdateData(FALSE);
    return;
}

// ch:设置触发模式 | en:Set trigger mode
void CMultipleCameraDlg::OnBnClickedTriggerModeRadio()
{
    ((CButton *)GetDlgItem(IDC_CONTINUS_MODE_RADIO))->SetCheck(FALSE);
    ((CButton *)GetDlgItem(IDC_TRIGGER_MODE_RADIO))->SetCheck(TRUE);
    m_nTriggerMode = TRIGGER_ON;
    int nRet = SetTriggerMode();
    if (MV_CODEREADER_OK != nRet)
    {
        ((CButton *)GetDlgItem(IDC_CONTINUS_MODE_RADIO))->SetCheck(TRUE);
        ((CButton *)GetDlgItem(IDC_TRIGGER_MODE_RADIO))->SetCheck(FALSE);
        return;
    }
    GetDlgItem(IDC_SOFTWARE_MODE_CHECK)->EnableWindow(TRUE);
    GetDlgItem(IDC_HARDWARE_MODE_CHECK)->EnableWindow(TRUE);
    GetDlgItem(IDC_START_GRABBING_BUTTON)->EnableWindow(TRUE);
    GetDlgItem(IDC_CONTINUS_MODE_RADIO)->EnableWindow(TRUE);
    ((CButton *)GetDlgItem(IDC_CONTINUS_MODE_RADIO))->SetCheck(FALSE);
    GetDlgItem(IDC_TRIGGER_MODE_RADIO)->EnableWindow(FALSE);
    ((CButton *)GetDlgItem(IDC_TRIGGER_MODE_RADIO))->SetCheck(TRUE);
    GetDlgItem(IDC_SOFTWARE_ONCE_BUTTON)->EnableWindow(FALSE);
    ((CButton *)GetDlgItem(IDC_SOFTWARE_MODE_CHECK))->SetCheck(TRUE);
    OnBnClickedSoftwareModeCheck();


    GetDlgItem(IDC_SAVE_IMAGE_BUTTON)->EnableWindow(FALSE);
    UpdateData(TRUE);
    UpdateData(FALSE);
    return;
}

// ch:触发模式为软件触发 | en:Software trigger
void CMultipleCameraDlg::OnBnClickedSoftwareModeCheck()
{
    m_nTriggerSource = SOFTWAREMODE;
    // ch:设置为软触发模式 | en:Set trigger mode as software trigger
    int nRet = SetTriggerSource();
    if (MV_CODEREADER_OK != nRet)
    {
        ShowErrorMsg(TEXT("Set software trigger fail"), nRet);
    }

    //EnableControls(TRUE);


    if(((CButton *)GetDlgItem(IDC_SOFTWARE_MODE_CHECK))->GetCheck())
    {
        ((CButton *)GetDlgItem(IDC_HARDWARE_MODE_CHECK))->SetCheck(FALSE);
        GetDlgItem(IDC_HARDWARE_MODE_CHECK)->EnableWindow(TRUE);        
        GetDlgItem(IDC_SOFTWARE_MODE_CHECK)->EnableWindow(FALSE);
        if(m_bStartGrabbing)
        {
            GetDlgItem(IDC_SOFTWARE_ONCE_BUTTON)->EnableWindow(TRUE);
        }
    }

    return;
}

// ch:触发模式为硬件触发 | en:Hardware trigger
void CMultipleCameraDlg::OnBnClickedHardwareModeCheck()
{
    m_nTriggerSource = HAREWAREMODE;
    // ch:设置为硬触发模式 | en:Set trigger mode as hardware trigger
    int nRet = SetTriggerSource();
    if (MV_CODEREADER_OK != nRet)
    {
        ShowErrorMsg(TEXT("Set hardware trigger fail"), nRet);
    }

    //EnableControls(TRUE);
    if(((CButton *)GetDlgItem(IDC_HARDWARE_MODE_CHECK))->GetCheck())
    {
        ((CButton *)GetDlgItem(IDC_SOFTWARE_MODE_CHECK))->SetCheck(FALSE);
        GetDlgItem(IDC_SOFTWARE_MODE_CHECK)->EnableWindow(TRUE);
        GetDlgItem(IDC_HARDWARE_MODE_CHECK)->EnableWindow(FALSE);
        GetDlgItem(IDC_SOFTWARE_ONCE_BUTTON)->EnableWindow(FALSE);
    }
    return;
}

// ch:软触发一次 | en:Software trigger
void CMultipleCameraDlg::OnBnClickedSoftwareOnceButton()
{
    if (FALSE == m_bStartGrabbing)
    {
    	ShowErrorMsg(TEXT("Please start grabbing first!"), 0);
        return;
    }

    // ch:软触发一次 | en:Sftware trigger
    int nRet = DoSoftwareOnce();


    return;
}

// ch:关闭，包含销毁句柄 | en:Close, include destroy handle
void CMultipleCameraDlg::OnClose()
{
    m_bStartGrabbing = FALSE;
    CloseDevice(); // 关闭所有打开的设备
    DestroyDevice();
    CDialog::OnClose();
}

BOOL CMultipleCameraDlg::PreTranslateMessage(MSG* pMsg)
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


void CMultipleCameraDlg::OnBnClickedCloseDeviceButton()
{
    // TODO: Add your control notification handler code here
    m_bStartGrabbing = FALSE;
    CloseDevice(); // 关闭所有打开的设备

    EnableControls(FALSE);
    UpdateData(FALSE);
    GetDlgItem(IDC_CLOSE_DEVICE_BUTTON)->EnableWindow(FALSE);
    GetDlgItem(IDC_INIT_DEVICE_BUTTON)->EnableWindow(TRUE);
    ((CButton *)GetDlgItem(IDC_TRIGGER_MODE_RADIO))->SetCheck(FALSE);
    ((CButton *)GetDlgItem(IDC_CONTINUS_MODE_RADIO))->SetCheck(FALSE);
    ((CButton *)GetDlgItem(IDC_SOFTWARE_MODE_CHECK))->SetCheck(FALSE);
    ((CButton *)GetDlgItem(IDC_HARDWARE_MODE_CHECK))->SetCheck(FALSE);
    GetDlgItem(IDC_EDIT1)->EnableWindow(TRUE);
    GetDlgItem(IDC_EDIT2)->EnableWindow(TRUE);
    GetDlgItem(IDC_EDIT3)->EnableWindow(TRUE);
    GetDlgItem(IDC_EDIT4)->EnableWindow(TRUE);

    for(int i = 0; i < 4; i++) 
    {
        if(NULL != m_cwmdDisplay[i])
        {
            m_cwmdDisplay[i]->ShowWindow(FALSE);
            m_cwmdDisplay[i]->ShowWindow(TRUE);
        }
    }

    DestroyDevice();
}
