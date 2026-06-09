#pragma once
#include "afxwin.h"
#include "MVIDCodeReader.h"
#include "afxcmn.h"
#include "GdiPlus.h"

#include <string>
#include <map>

#pragma comment(lib,"gdiplus.lib")

using namespace std;
using namespace Gdiplus;

#pragma pack(2)
typedef struct tagBmpFile
{
    BITMAPFILEHEADER    stBmpFileHeader;    // ch:文件头结构 | en:File header structure
    BITMAPINFOHEADER    stBmpInfoHeader;    // ch:信息头结构 | en:Information header structure
    unsigned int        nLineBytes;         // ch:行字节宽度 | en:Width of line byte
    RGBQUAD*            pstPalette;         // ch:调色板 | en:Palette
    unsigned char*      pBuf;               // ch:图像数据缓存 | en:Buffer for saving image data
    unsigned int        nBufSize;           // ch:缓存空间大小 | en:Buffer size
}BmpFile;
#pragma pack()

class CGrab_CallbackDlg : public CDialog
{
public:
	CGrab_CallbackDlg(CWnd* pParent = NULL);

	enum { IDD = IDD_Grab_Callback_DIALOG };

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	

protected:
	HICON m_hIcon;

	virtual BOOL OnInitDialog();
	afx_msg void OnSysCommand(UINT nID, LPARAM lParam);
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	DECLARE_MESSAGE_MAP()
public:
    afx_msg void OnBnClickedBtnEnum();
    afx_msg void OnBnClickedBtnStart();
    afx_msg void OnBnClickedBtnStop();
    afx_msg void OnBnClickedBtnClear();
    afx_msg void OnClose();

    void ShowErrorMsg(CString csMessage, int nErrorNum);
    void ImageCallBack(MVID_CAM_OUTPUT_INFO* pFrameInfo);
    void Display(void* hWnd, MVID_CAM_OUTPUT_INFO* pstDisplayImage);
    void CheckListNum();
    void SetHScroll();

private:

    CListBox                m_ctrlListBoxResult;
    CComboBox               m_ctrlDeviceCombo;
    int                     m_nDeviceCombo;
    int                     m_nListNum;         // ch:ListBox行数 | en:Number of lines
    CCriticalSection        m_criSection;       // ch:临界区 | en:Critical region

    MVID_CAMERA_INFO_LIST*  m_pstDevList;       // 相机列表
    void*                   m_handle;           // 相机句柄
    BOOL                    m_bProcess;         // 是否正在读码

    void*                   m_hWndDisplay;      // ch:显示窗口句柄 | en:Handle of display window
    BITMAPINFO*             m_pstBitmapInfo;      // 图像信息

    LANGID                  m_nSystemLanguageId;
};
