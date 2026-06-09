
// ReconnectDemoDlg.h : 头文件
//

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

// CReconnectDemoDlg 对话框
class CReconnectDemoDlg : public CDialog
{
// 构造
public:
	CReconnectDemoDlg(CWnd* pParent = NULL);	// 标准构造函数

// 对话框数据
	enum { IDD = IDD_ReconnectDemo_DIALOG };

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV 支持


// 实现
protected:
	HICON m_hIcon;

	// 生成的消息映射函数
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
    void ExceptionCallBack(unsigned int nMsgType);
    void CheckListNum();
    void SetHScroll();

private:

    CListBox                m_ctrlListBoxResult;    // 读码结果列表
    CComboBox               m_ctrlDeviceCombo;      // 枚举下拉框
    int                     m_nDeviceCombo;         // 设备序号
    int                     m_nIndex;               // 设备序号
    int                     m_nListNum;             // ch:ListBox行数 | en:Number of lines
    CCriticalSection        m_criSection;           // ch:临界区 | en:Critical region

    MVID_CAMERA_INFO_LIST*  m_pstDevList;           // 相机枚举信息列表
    void*                   m_handle;               // SDK句柄
    BOOL                    m_bProcess;             // 是否正在读码

    LANGID                  m_nSystemLanguageId;    // 系统语言
};
