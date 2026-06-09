#pragma once
#include "afxwin.h"
#include "MVIDCodeReader.h"
#include "afxcmn.h"

#include <string>
#include <map>
using namespace std;

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

class CSaveImageDemoDlg : public CDialog
{
public:
	CSaveImageDemoDlg(CWnd* pParent = NULL);

	enum { IDD = IDD_SaveImageDemo_DIALOG };

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

    afx_msg void OnBnClickedBtnLoad();
    afx_msg void OnBnClickedBtnInit();
    afx_msg void OnClose();
    afx_msg void OnBnClickedBtnSave();
    void ShowErrorMsg(CString csMessage, int nErrorNum);

private:

    CString                 m_strImageFilePath;     // 图像文件路径
    void*                   m_handle;               // SDK句柄
    MVID_PROC_PARAM*        m_pstProcParam;         // 读码图像参数
    int                     m_nImageWidthEdit;      // 图像宽
    int                     m_nImageHeightEdit;     // 图像高
    CComboBox               m_ctrlImageTypeCombo;   // 图像格式下拉框
    CComboBox               m_ctrlConvertTypeCombo; // 原图格式下拉框
};
