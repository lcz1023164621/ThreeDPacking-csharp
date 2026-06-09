#include "stdafx.h"
#include "SaveImageDemo.h"
#include "SaveImageDemoDlg.h"
#include <io.h>

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

CSaveImageDemoDlg::CSaveImageDemoDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CSaveImageDemoDlg::IDD, pParent)
    , m_strImageFilePath(_T(""))
    , m_handle(NULL)
    , m_nImageWidthEdit(0)
    , m_nImageHeightEdit(0)
    , m_pstProcParam(NULL)
{
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);
}

void CSaveImageDemoDlg::DoDataExchange(CDataExchange* pDX)
{
    CDialog::DoDataExchange(pDX);
    DDX_Text(pDX, IDC_EDIT_ImagePath, m_strImageFilePath);
    DDX_Text(pDX, IDC_IMAGE_WIDTH, m_nImageWidthEdit);
    DDX_Text(pDX, IDC_IMAGE_HEIGHT, m_nImageHeightEdit);
    DDX_Control(pDX, IDC_COMBO_TYPE, m_ctrlImageTypeCombo);
    DDX_Control(pDX, IDC_COMBO_CONVERTTYPE, m_ctrlConvertTypeCombo);
}

BEGIN_MESSAGE_MAP(CSaveImageDemoDlg, CDialog)
	ON_WM_SYSCOMMAND()
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	//}}AFX_MSG_MAP
    ON_BN_CLICKED(IDC_BTN_Load, &CSaveImageDemoDlg::OnBnClickedBtnLoad)
    ON_BN_CLICKED(IDC_BTN_Init, &CSaveImageDemoDlg::OnBnClickedBtnInit)
    ON_WM_CLOSE()
    ON_BN_CLICKED(IDC_BTN_SAVE, &CSaveImageDemoDlg::OnBnClickedBtnSave)
END_MESSAGE_MAP()

BOOL CSaveImageDemoDlg::OnInitDialog()
{
	CDialog::OnInitDialog();

	// ch:IDM_ABOUTBOX 必须在系统命令范围内。| en:The IDM_ABOUTBOX must be in the range of system command
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

    m_ctrlImageTypeCombo.AddString(_T("JPG"));
    m_ctrlImageTypeCombo.AddString(_T("BMP"));

    // ch:默认选择第一项 | en:By default, the first item is selected
    m_ctrlImageTypeCombo.SetCurSel(0);

    m_ctrlConvertTypeCombo.AddString(_T("MONO8"));
    m_ctrlConvertTypeCombo.AddString(_T("BGR24"));
    m_ctrlConvertTypeCombo.SetCurSel(0);

    if (NULL == m_pstProcParam)
    {
        m_pstProcParam = (MVID_PROC_PARAM *)malloc(sizeof(MVID_PROC_PARAM));
        if (NULL == m_pstProcParam)
        {
            return FALSE;
        }
    }
    memset(m_pstProcParam, 0, sizeof(MVID_PROC_PARAM));

	return TRUE;  // ch:除非将焦点设置到控件，否则返回 TRUE | en:It will return "TRUE" unless the focus is set on the control
}

void CSaveImageDemoDlg::OnSysCommand(UINT nID, LPARAM lParam)
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

void CSaveImageDemoDlg::OnPaint()
{
	if (IsIconic())
	{
		CPaintDC dc(this); // ch:用于绘制的设备上下文 | en:Device context for drawin

		SendMessage(WM_ICONERASEBKGND, reinterpret_cast<WPARAM>(dc.GetSafeHdc()), 0);

		// ch:使图标在工作区矩形中居中 | en:Move the icon to the center of work region (rectangle)
		int cxIcon = GetSystemMetrics(SM_CXICON);
		int cyIcon = GetSystemMetrics(SM_CYICON);
		CRect rect;
		GetClientRect(&rect);
		int x = (rect.Width() - cxIcon + 1) / 2;
		int y = (rect.Height() - cyIcon + 1) / 2;

		// ch:绘制图标 | en: Draw the icon
		dc.DrawIcon(x, y, m_hIcon);
	}
	else
	{
		CDialog::OnPaint();
	}
}

HCURSOR CSaveImageDemoDlg::OnQueryDragIcon()
{
	return static_cast<HCURSOR>(m_hIcon);
}

// ch:显示错误信息 | en:Show error message
void CSaveImageDemoDlg::ShowErrorMsg(CString csMessage, int nErrorNum)
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
    default:                        errorMsg += "unknow error ";                                                    break;
    }

    MessageBox(errorMsg, TEXT("PROMPT"), MB_OK | MB_ICONWARNING);
}

// 选择本地图片
void CSaveImageDemoDlg::OnBnClickedBtnLoad()
{
    CFileDialog stFileDlg(TRUE, NULL, NULL, OFN_HIDEREADONLY, TEXT("RAW Files(*.raw)|*.raw;*.RAW||"), this);
    if (IDOK != stFileDlg.DoModal())
    {
        return;
    }

    m_strImageFilePath  = stFileDlg.GetPathName();

    UpdateData(FALSE);
}

// 初始化资源
void CSaveImageDemoDlg::OnBnClickedBtnInit()
{
    if(m_handle)
    {
        MVID_CR_DestroyHandle(m_handle);
        m_handle = NULL;
    }

    // ch:仅一维码识别：MVID_BCR | en:Recognize barcode only：MVID_BCR
    // ch:仅二维码识别：MVID_TDCR | en:Recognize Two-Dimension code only: MVID_TDCR
    // ch:一维码 + 二维码 识别：MVID_BCR | MVID_TDCR | en:Recognize Barcode + Two-Dimension code: MVID_BCR | MVID_TDCR
    int nRet = MVID_CR_CreateHandle(&m_handle, 0);
    if (MVID_CR_OK != nRet)
    {
        ShowErrorMsg(TEXT("MVID_CR_CreateHandle "), nRet);
        return ;
    }
}

void CSaveImageDemoDlg::OnClose()
{
    if (m_handle)
    {
        MVID_CR_DestroyHandle(m_handle);
        m_handle = NULL;
    }

    if (m_pstProcParam)
    {
        if (m_pstProcParam->pImageBuf)
        {
            free(m_pstProcParam->pImageBuf);
            m_pstProcParam->pImageBuf = NULL;
        }

        free(m_pstProcParam);
        m_pstProcParam = NULL;
    }

    CDialog::OnClose();
}

// 保存图像
void CSaveImageDemoDlg::OnBnClickedBtnSave()
{
    int nRet = MVID_CR_OK;
    CString strMsg = _T("");
    if (NULL == m_handle)
    {
        strMsg.Format(_T("wrong handle !"));
        MessageBox(strMsg);
    }

    UpdateData(TRUE);

    if (m_nImageWidthEdit <= 0 || m_nImageHeightEdit <= 0)
    {
        strMsg.Format(_T("error image width/height"));
        MessageBox(strMsg);
        return ;
    }

    // ch:加载图像 | en: Load the image
    char chImageFilePath[1024] = {0};
    WideCharToMultiByte(CP_OEMCP, NULL, m_strImageFilePath.GetBuffer(), -1, chImageFilePath, 256, NULL, FALSE);

    FILE* pfile = fopen(chImageFilePath, "rb");
    if (NULL == pfile)
    {
        strMsg.Format(_T("open file fail"));
        MessageBox(strMsg);
        return ;
    }

    m_pstProcParam->nWidth      = m_nImageWidthEdit;
    m_pstProcParam->nHeight     = m_nImageHeightEdit;
    m_pstProcParam->nImageLen = filelength(fileno(pfile));

    switch (m_ctrlConvertTypeCombo.GetCurSel()) 
    { 
    case 0:
        m_pstProcParam->enImageType = MVID_IMAGE_MONO8;

        if (m_pstProcParam->nWidth * m_pstProcParam->nHeight != m_pstProcParam->nImageLen)
        {
            memset(m_pstProcParam, 0, sizeof(MVID_PROC_PARAM));
            strMsg.Format(_T("Input Width/Height Error or the Picture is not Mono8!"));
            MessageBox(strMsg);
            fclose(pfile);
            pfile = NULL;
            return ;
        }

        break;
    case 1:
        m_pstProcParam->enImageType = MVID_IMAGE_BGR24;

        if (3 * m_pstProcParam->nWidth * m_pstProcParam->nHeight != m_pstProcParam->nImageLen)
        {
            memset(m_pstProcParam, 0, sizeof(MVID_PROC_PARAM));
            strMsg.Format(_T("Input Width/Height Error or the Picture is not Mono8!"));
            MessageBox(strMsg);
            fclose(pfile);
            pfile = NULL;
            return ;
        }

        break; 
    default:
        fclose(pfile);
        pfile = NULL;
        return;
    }

    if (m_pstProcParam->pImageBuf)
    {
        free(m_pstProcParam->pImageBuf);
        m_pstProcParam->pImageBuf = NULL;
    }
    m_pstProcParam->pImageBuf = (unsigned char*)malloc(m_pstProcParam->nImageLen);
    if (NULL == m_pstProcParam->pImageBuf)
    {
        strMsg.Format(_T("malloc fail"));
        MessageBox(strMsg);
        fclose(pfile);
        pfile = NULL;
        return ;
    }

    if (m_pstProcParam->nImageLen > fread(m_pstProcParam->pImageBuf, 1, m_pstProcParam->nImageLen, pfile))
    {
        strMsg.Format(_T("read fail"));
        MessageBox(strMsg);
        fclose(pfile);
        pfile = NULL;
        return;
    }
    fclose(pfile);
    pfile = NULL;

    MVID_IMAGE_INFO pstInputImage = {0};
    MVID_IMAGE_INFO pstOutputImage = {0};
    MVID_IMAGE_TYPE enImageType = MVID_IMAGE_JPEG;

    switch (m_ctrlImageTypeCombo.GetCurSel()) 
    { 
    case 0:
        enImageType = MVID_IMAGE_JPEG;
        break; 
    case 1:
        enImageType = MVID_IMAGE_BMP;
        break;
    default:
        return;
    }

    pstInputImage.pImageBuf = m_pstProcParam->pImageBuf;
    pstInputImage.nImageLen = m_pstProcParam->nImageLen;
    pstInputImage.enImageType = m_pstProcParam->enImageType;
    pstInputImage.nWidth = m_pstProcParam->nWidth;
    pstInputImage.nHeight = m_pstProcParam->nHeight;

    nRet = MVID_CR_SaveImage(m_handle, &pstInputImage, enImageType, &pstOutputImage, 80);
    if (MVID_CR_OK == nRet)
    {
        // ch:保存图像 | en:Save image
        char filename[256] = {0};
        CTime currTime;                                     // ch:获取系统时间作为保存图片文件名 | en:Get the system time as the name of saved picture file
        currTime = CTime::GetCurrentTime();

        if (MVID_IMAGE_JPEG == enImageType)
        {
            sprintf(filename,("%.4d%.2d%.2d%.2d%.2d%.2d.jpg"), currTime.GetYear(), currTime.GetMonth(),
                currTime.GetDay(), currTime.GetHour(), currTime.GetMinute(), currTime.GetSecond());
        }
        else
        {
            sprintf(filename,("%.4d%.2d%.2d%.2d%.2d%.2d.bmp"), currTime.GetYear(), currTime.GetMonth(),
            currTime.GetDay(), currTime.GetHour(), currTime.GetMinute(), currTime.GetSecond());
        }

        pfile = fopen(filename,"wb");
        if(pfile == NULL)
        {
            strMsg.Format(_T("failed open file"));
            MessageBox(strMsg);
            return ;
        }
        fwrite(pstOutputImage.pImageBuf, 1, pstOutputImage.nImageLen, pfile);
        strMsg.Format(_T("success save image"));
        MessageBox(strMsg);
        fclose (pfile);
        pfile = NULL;
    }
    else
    {
        strMsg.Format(_T("fail save image %#x"), nRet);
        MessageBox(strMsg);
    }
}
