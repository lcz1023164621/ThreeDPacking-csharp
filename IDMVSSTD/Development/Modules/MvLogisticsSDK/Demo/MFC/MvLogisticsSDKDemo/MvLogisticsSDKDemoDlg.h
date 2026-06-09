
// MvLogisticsSDKDemoDlg.h : header file
//

#pragma once
#include "MvLogisticsSDK.h"
#include "MvLogisticsSDKDefine.h"
#include "GdiPlus.h"

#pragma comment(lib,"gdiplus.lib")

using namespace Gdiplus;



// CMvLogisticsSDKDemoDlg dialog
class CMvLogisticsSDKDemoDlg : public CDialog
{
// Construction
public:
	CMvLogisticsSDKDemoDlg(CWnd* pParent = NULL);	// standard constructor

// Dialog Data
	enum { IDD = IDD_MVLOGISTICSSDKDEMO_DIALOG };

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support


// Implementation
protected:
	HICON m_hIcon;

	// Generated message map functions
	virtual BOOL OnInitDialog();
	afx_msg void OnSysCommand(UINT nID, LPARAM lParam);
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	DECLARE_MESSAGE_MAP()

private:

    CString                         m_strCfgFilePath;
    CEdit                           m_editCfgFileBarcode;
    CEdit                           m_editCfgFileVolume;
    CEdit                           m_editCfgFileWeight;
    void*                           m_handle;
    void*                           m_hWndDisplay;
    BITMAPINFO*                     m_bBitmapInfo;
    CListCtrl                       m_ctrlDataHistory;
    int                             m_InsertRow;                                // list control ≤Â»Î––
    HGLOBAL                         m_hGlobal;

public:

    int Display(void* hWnd, MVLGS_CODE_LIST* pstLgsDisplay);
    void LogisticsCallBack(MVLGS_PACKAGE_INFO * pstPkgInfo);
    void InitListCtrl();
	void ExceptionCallbackFunc(MVLGS_EXCEPTION_INFO *pstEcptInfo);
	void TriggerCallbackFunc(MVLGS_TRIGGER_INFO *pstTriggerInfo);
public:
    afx_msg void OnBnClickedButton_Init();
    afx_msg void OnBnClickedButtonChoosefilepath();
    afx_msg void OnBnClickedButtonStart();
    afx_msg void OnBnClickedButtonStop();
    afx_msg void OnBnClickedButtonDestory();
    afx_msg void OnDestroy();
    afx_msg void OnBnClickedButtonTrigger();
};
