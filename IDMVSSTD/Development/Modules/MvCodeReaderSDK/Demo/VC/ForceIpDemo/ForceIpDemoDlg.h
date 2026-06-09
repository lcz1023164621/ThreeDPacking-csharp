
// ForceIpDemoDlg.h :  header file
#pragma once
#include "afxwin.h"
#include "afxcmn.h"
#include <stdio.h>
#include "MvCodeReaderCtrl.h"
#include "MvCodeReaderErrorDefine.h"
#include "MvCodeReaderParams.h"
#include "MvCodeReaderPixelType.h"

// CForceIpDemoDlg dialog
class CForceIpDemoDlg : public CDialog
{
// construction
public:
	CForceIpDemoDlg(CWnd* pParent = NULL);	// standard construction function

// dialog data
	enum { IDD = IDD_FORCEIPDEMO_DIALOG };

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV supported

// implementation
protected:
	HICON m_hIcon;

	// generated message mapping function
	virtual BOOL OnInitDialog();
	afx_msg void OnSysCommand(UINT nID, LPARAM lParam);
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	DECLARE_MESSAGE_MAP()
private:
    CComboBox m_ctrlDeviceCombo;
    int m_nDeviceCombo;
    CButton m_ctrlEnumDeviceButton;
    CButton m_ctrlSetIpButton;
    CIPAddressCtrl m_ctrlIpaddress;
    DWORD m_dwIpaddress;
    DWORD m_dwSubNetMask;
    DWORD m_dwDefaultGateWay;

    MV_CODEREADER_DEVICE_INFO_LIST m_stDevList;         // ch:设备信息列表结构体变量，用来存储设备列表

    void *                 m_handle;

public:
    afx_msg void OnBnClickedEnumDeviceButton();
    afx_msg void OnBnClickedSetIpButton();
    afx_msg void OnCbnSelchangeDeviceCombo();
    virtual BOOL PreTranslateMessage(MSG* pMsg);
    void ShowErrorMsg(CString csMessage, int nErrorNum);
    bool IsStrUTF8(const char* pBuffer, int size);
    bool Char2Wchar(const char *pStr, wchar_t *pOutWStr, int nOutStrSize);
    bool Wchar2char(wchar_t *pOutWStr, char *pStr);

private:
    int DisplayDeviceIp();
public:
    CIPAddressCtrl m_ctrlSubNetMask;
    CIPAddressCtrl m_ctrlDefaultGateWay;
};
