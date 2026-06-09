
// MVIDCodeReaderDemoDlg.h : 头文件
//

#pragma once
#include "afxwin.h"
#include "MVIDCodeReader.h"
#include "afxcmn.h"
#include "GdiPlus.h"
#include <string>
#include <map>
#include "MVIDCodeReaderDemoRawDlg.h"

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

// CMVIDCodeReaderDemoDlg 对话框
class CMVIDCodeReaderDemoDlg : public CDialog
{
// 构造
public:
	CMVIDCodeReaderDemoDlg(CWnd* pParent = NULL);	// 标准构造函数

// 对话框数据
	enum { IDD = IDD_MVIDCODEREADERDEMO_DIALOG };

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
    afx_msg BOOL PreTranslateMessage(MSG* pMsg);
    afx_msg void OnBnClickedBtnLoad();
    afx_msg void OnBnClickedBtnProc();
    afx_msg void OnNMClickListParam(NMHDR *pNMHDR, LRESULT *pResult);
    afx_msg void OnKillfocusEdit();//动态生成编辑框失去焦点响应函数
    afx_msg void OnBnClickedBtnInit();
    afx_msg void OnBnClickedBtnClear();
    afx_msg void OnBnClickedBtnCutout();
    afx_msg void OnBnClickedGetParameterButton();
    afx_msg void OnBnClickedSetParameterButton();
    afx_msg void OnClose();
    void Display(void* hWnd, MVID_CAM_OUTPUT_INFO* pstDisplayImage);
    void ReadBmp(CString pchBmpName, BmpFile* pstBmpFile);
    void UpdatelListCtrl();
    void ShowEdit( NM_LISTVIEW *pEditCtrl );
    void InitListCtrl();
    void ShowErrorMsg(CString csMessage, int nErrorNum);
    void Process();
    void CheckListNum();
    void SetHScroll();

    void EnableControls();

private:
    CListBox                m_ctrlListBoxResult;    // 读码结果列表
    CString                 m_strImageFilePath;     // 本地图像路径
    CListCtrl               m_list;                 // 算法参数编辑框
    CEdit                   m_Edit;                 // 算法参数编辑框
    CComboBox               m_comBox;               // 生产单元格下拉列表对象
    CComboBox               m_ctrlDeviceCombo;      // 枚举下拉框
    int                     m_nDeviceCombo;         // 设备序号
    BOOL                    m_bIsEnum;              // 是否枚举到有效设备
    int                     m_nListNum;             // ch:ListBox行数 | en:Number of lines
    CCriticalSection        m_criSection;           // ch:临界区 | en:Critical region

    MVID_CAMERA_INFO_LIST*  m_pstDevList;           // 相机枚举信息列表
    void*                   m_handle;               // SDK句柄
    BOOL                    m_bProcess;             // 是否正在读码
    HANDLE*                 m_hThread;              // 读码线程

    BmpFile                 m_stBmpFile;            // BMP文件数据
    MVID_PROC_PARAM*        m_pstProcParam;         // 图像读码参数
    MVID_CAM_OUTPUT_INFO*   m_pstOutput;            // 图像显示信息
    BOOL                    m_bIsInitRes;           // 是否初始化本地图片读码资源
    BOOL                    m_bIsLoadImg;           // 是否加载了本地图片

    std::map<CString, int>  m_mapParamList;         // 算法参数列表
    int                     m_nItem;                // 刚编辑的行
    int                     m_nSubItem;             // 刚编辑的列

    void*                   m_hWndDisplay;          // ch:显示窗口句柄 | en:Handle of display window
    BITMAPINFO*             m_bBitmapInfo;          // BMP图像信息

    CEdit                   m_ctrlExposureEdit;     // 曝光编辑框
    CString                 m_strExposureEdit;      // 曝光值
    CEdit                   m_ctrlGainEdit;         // 增益编辑框
    CString                 m_strGainEdit;          // 增益值
    CEdit                   m_ctrlFrameRateEdit;    // 帧率编辑框
    CString                 m_strFrameRateEdit;     // 帧率值

    LANGID                  m_nSystemLanguageId;    // 系统语言

    CMVIDCodeReaderDemoRawDlg m_DemoRawDlg;         // Raw图片信息采集
};
