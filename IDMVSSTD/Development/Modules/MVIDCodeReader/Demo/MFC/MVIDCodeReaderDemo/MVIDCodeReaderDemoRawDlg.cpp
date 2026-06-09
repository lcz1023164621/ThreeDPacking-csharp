// CMVIDCodeReaderDemoRawDlg.cpp : implementation file
//

#include "stdafx.h"
#include "MVIDCodeReaderDemo.h"
#include "MVIDCodeReaderDemoRawDlg.h"


// CMVIDCodeReaderDemoRawDlg dialog

IMPLEMENT_DYNAMIC(CMVIDCodeReaderDemoRawDlg, CDialog)

CMVIDCodeReaderDemoRawDlg::CMVIDCodeReaderDemoRawDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CMVIDCodeReaderDemoRawDlg::IDD, pParent)
    , m_nImageWidthEdit(0)
    , m_nImageHeightEdit(0)
    , m_TypeCombo(0)
{
}

CMVIDCodeReaderDemoRawDlg::~CMVIDCodeReaderDemoRawDlg()
{
}

void CMVIDCodeReaderDemoRawDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);

    DDX_Text(pDX, IDC_IMAGE_WIDTH, m_nImageWidthEdit);
    DDX_Text(pDX, IDC_IMAGE_HEIGHT, m_nImageHeightEdit);
    DDX_Control(pDX, IDC_COMBO_CONVERTTYPE, m_ctrlConvertTypeCombo);
}

BOOL CMVIDCodeReaderDemoRawDlg::OnInitDialog()
{
    CDialog::OnInitDialog();

    // TODO:  Add extra initialization here
    ComboList();

    return TRUE;  // return TRUE unless you set the focus to a control
    // EXCEPTION: OCX Property Pages should return FALSE
}

BEGIN_MESSAGE_MAP(CMVIDCodeReaderDemoRawDlg, CDialog)
    ON_BN_CLICKED(IDC_BUTTON1, &CMVIDCodeReaderDemoRawDlg::OnBnClickedButton1)
    ON_CBN_SELCHANGE(IDC_COMBO_CONVERTTYPE, &CMVIDCodeReaderDemoRawDlg::OnCbnSelchangeComboConverttype)
END_MESSAGE_MAP()


// CMVIDCodeReaderDemoRawDlg message handlers

void CMVIDCodeReaderDemoRawDlg::OnBnClickedButton1()
{
    // TODO: Add your control notification handler code here
    CDialog::OnOK();
}

void CMVIDCodeReaderDemoRawDlg::ComboList()
{
    m_ctrlConvertTypeCombo.AddString(_T("MONO8"));
    m_ctrlConvertTypeCombo.AddString(_T("BGR24"));
    m_ctrlConvertTypeCombo.AddString(_T("BayerGB10"));
    m_ctrlConvertTypeCombo.AddString(_T("YUV422_Packed"));
    m_ctrlConvertTypeCombo.SetCurSel(0);
    m_TypeCombo = m_ctrlConvertTypeCombo.GetCurSel();
}

void CMVIDCodeReaderDemoRawDlg::OnCbnSelchangeComboConverttype()
{
    // TODO: Add your control notification handler code here
    m_TypeCombo = m_ctrlConvertTypeCombo.GetCurSel();
}
