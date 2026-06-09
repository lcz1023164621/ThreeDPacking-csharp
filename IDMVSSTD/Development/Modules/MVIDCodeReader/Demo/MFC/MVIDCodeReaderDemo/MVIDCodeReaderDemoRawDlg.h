#pragma once


// CMVIDCodeReaderDemoRawDlg dialog

class CMVIDCodeReaderDemoRawDlg : public CDialog
{
	DECLARE_DYNAMIC(CMVIDCodeReaderDemoRawDlg)

public:
	CMVIDCodeReaderDemoRawDlg(CWnd* pParent = NULL);   // standard constructor
	virtual ~CMVIDCodeReaderDemoRawDlg();

// Dialog Data
	enum { IDD = IDD_MVIDCODEREADERDEMO_RAWDIALOG };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support

    virtual BOOL OnInitDialog();
	DECLARE_MESSAGE_MAP()

public:

    int                     m_nImageWidthEdit;
    int                     m_nImageHeightEdit;
    CComboBox               m_ctrlConvertTypeCombo;
    int                     m_TypeCombo;

    afx_msg void OnBnClickedButton1();
    void ComboList();
    afx_msg void OnCbnSelchangeComboConverttype();
};
