#include "stdafx.h"
#include "MyCamera.h"
#include <string.h>

CMyCamera::CMyCamera()
{
    m_hDevHandle        = NULL;
    m_bBitmapInfo       = NULL;
    m_MaxImageSize      = 0;
    memset(&m_pstParam, 0, sizeof(MV_CODEREADER_TJPG_PARAM));
    memset(&m_stParam, 0, sizeof(MV_CODEREADER_DRAW_PARAM));
}

CMyCamera::~CMyCamera()
{
    if (m_hDevHandle)
    {
        MV_CODEREADER_DestroyHandle(m_hDevHandle);
        m_hDevHandle    = NULL;
    }
}

int CMyCamera::EnumDevices(MV_CODEREADER_DEVICE_INFO_LIST* pstDevList)
{
    int nRet = MV_CODEREADER_EnumDevices(pstDevList, MV_CODEREADER_GIGE_DEVICE);
    if (MV_CODEREADER_OK != nRet)
    {
        return nRet;
    }

    return MV_CODEREADER_OK;
}

int CMyCamera::InitResources()
{
    int nRet = MV_CODEREADER_OK;

    try
    {
        int nSensorWidth = 0;
        int nSensorHight = 0;

        MV_CODEREADER_INTVALUE_EX stParam;
        memset(&stParam, 0, sizeof(MV_CODEREADER_INTVALUE_EX));
        nRet = MV_CODEREADER_GetIntValue(m_hDevHandle, Camera_Width, &stParam);
        if (MV_CODEREADER_OK != nRet)
        {
            nRet = MV_CODEREADER_E_UNKNOW;
            throw nRet;
        }
        nSensorWidth = stParam.nCurValue;

        memset(&stParam, 0, sizeof(MV_CODEREADER_INTVALUE_EX));
        nRet = MV_CODEREADER_GetIntValue(m_hDevHandle, Camera_Height, &stParam);
        if (MV_CODEREADER_OK != nRet)
        {
            throw nRet;
        }
        nSensorHight = stParam.nCurValue;

        m_MaxImageSize = nSensorWidth * nSensorHight + 4096;

        m_pstParam.pBufOutput = (unsigned char*)malloc(m_MaxImageSize);
        if (NULL == m_pstParam.pBufOutput)
        {
            nRet = MV_CODEREADER_E_RESOURCE;
            throw nRet;
        }
        memset(m_pstParam.pBufOutput, 0, m_MaxImageSize);

        m_stParam.pData = (unsigned char*)malloc(m_MaxImageSize);
        if (NULL == m_stParam.pData)
        {
            nRet = MV_CODEREADER_E_RESOURCE;
            throw nRet;
        }
        memset(m_stParam.pData, 0, m_MaxImageSize);

    }
    catch (...)
    {
        DeInitResources();
        return nRet;
    }

    return MV_CODEREADER_OK;
}

void CMyCamera::DeInitResources()
{
    if (NULL != m_pstParam.pBufOutput)
    {
        free(m_pstParam.pBufOutput);
        m_pstParam.pBufOutput = NULL;
    }

    if (NULL != m_stParam.pData)
    {
        free(m_stParam.pData);
        m_stParam.pData = NULL;
    }
}

// ch:打开设备 | en:Open Device
int     CMyCamera::Open(MV_CODEREADER_DEVICE_INFO* pstDeviceInfo)
{
    if (NULL == pstDeviceInfo)
    {
        return MV_CODEREADER_E_PARAMETER;
    }

    int nRet = MV_CODEREADER_OK;
    if(m_hDevHandle == NULL)
    {
        nRet  = MV_CODEREADER_CreateHandle(&m_hDevHandle, pstDeviceInfo);
        if (MV_CODEREADER_OK != nRet)
        {
            return nRet;
        }
    }

    nRet = MV_CODEREADER_OpenDevice(m_hDevHandle);
    if (MV_CODEREADER_OK != nRet)
    {
        MV_CODEREADER_DestroyHandle(m_hDevHandle);
        m_hDevHandle = NULL;

        return nRet;
    }

    // 初始化必要的资源
    InitResources();

    return MV_CODEREADER_OK;
}


// ch:关闭设备 | en:Close Device
int     CMyCamera::Close()
{
    int nRet = MV_CODEREADER_OK;

    if (NULL == m_hDevHandle)
    {
        return MV_CODEREADER_E_PARAMETER;
    }

    nRet = MV_CODEREADER_CloseDevice(m_hDevHandle);
    nRet = MV_CODEREADER_DestroyHandle(m_hDevHandle);
    m_hDevHandle = NULL;

    return nRet;
}

// jpg解码
int     CMyCamera::MvJpgDecompress(IN OUT MV_CODEREADER_TJPG_PARAM* pstParam)
{
    if (NULL == pstParam)
    {
        return MV_CODEREADER_E_PARAMETER;
    }

    int nRet        = MV_CODEREADER_OK;
    int nWidth      = 0;
    int nHeight     = 0;
    int nSubsample  = 0;
    int nColorspace = 0;
    int nPixelfmt   = 0;      //TJPF_RGB;

    tjhandle handle = NULL;;
    handle = tjInitDecompress();
    if (NULL == handle)
    {
        return MV_CODEREADER_E_RESOURCE;
    }

    try
    {
        nRet = tjDecompressHeader3(handle, pstParam->pBufInput, pstParam->nBufInputLen, &nWidth, &nHeight, 
            &nSubsample, &nColorspace);
        if (nRet == -1)
        {
            nRet = MV_CODEREADER_E_PARAMETER;
            throw nRet;
        }

        if (TJSAMP_GRAY == nSubsample || TJSAMP_420 == nSubsample)
        {
            nPixelfmt = TJPF_GRAY;
        }

        nRet = tjDecompress2(handle, pstParam->pBufInput, pstParam->nBufInputLen, pstParam->pBufOutput, nWidth, 0,
            nHeight, nPixelfmt, 0);
        if (nRet == -1)
        {
            nRet = MV_CODEREADER_E_PARAMETER;
            throw nRet;
        }

        pstParam->nWidth = nWidth;
        pstParam->nHeight = nHeight;

        if (TJSAMP_GRAY == nSubsample || TJSAMP_420 == nSubsample)
        {
            pstParam->nBufOutputLen = pstParam->nWidth * pstParam->nHeight;
        }
        else
        {
            pstParam->nBufOutputLen = pstParam->nWidth * pstParam->nHeight * 3;
        }
    }
    catch (...)
    {
        if (handle)
        {
            tjDestroy(handle);
			handle = NULL;
        }
        return nRet;
    }

    if (handle)
    {
        tjDestroy(handle);
		handle = NULL;
    }

    return MV_CODEREADER_OK;
}


// ch:开启抓图 | en:Start Grabbing
int     CMyCamera::StartGrabbing()
{
    return MV_CODEREADER_StartGrabbing(m_hDevHandle);
}


// ch:停止抓图 | en:Stop Grabbing
int     CMyCamera::StopGrabbing()
{
    return MV_CODEREADER_StopGrabbing(m_hDevHandle);
}


int  CMyCamera::Process(HWND hDisplay, bool bIsStartGrab)
{
    m_hDisplay = hDisplay;
    m_bStartGrabbing = bIsStartGrab;
    HANDLE hProcessThread = CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)ProcessThread, this, 0, NULL);
    if (NULL == hProcessThread)
    {
        return MV_CODEREADER_E_RESOURCE;
    }

    CloseHandle(hProcessThread);

    return MV_CODEREADER_OK;
}

void*  __stdcall CMyCamera::ProcessThread(void* pUser)
{
    int nRet = MV_CODEREADER_OK;

    CMyCamera* pThis = (CMyCamera*)pUser;
    if (NULL == pThis)
    {
        return NULL;
    }

    MV_CODEREADER_IMAGE_OUT_INFO_EX2 stImageInfo = {0};
    memset(&stImageInfo, 0, sizeof(MV_CODEREADER_IMAGE_OUT_INFO_EX2));
    unsigned char * pData = NULL;

    while (pThis->m_bStartGrabbing)
    {
        nRet = MV_CODEREADER_GetOneFrameTimeoutEx2(pThis->m_hDevHandle, &pData, &stImageInfo, 1000);
        if (nRet == MV_CODEREADER_OK)
        {
            if (NULL != pData)
            {
                //输出图像结果
                nRet = pThis->Display(pThis->m_hDisplay, pData, &stImageInfo);
                if (MV_CODEREADER_OK != nRet)
                {
                    continue;
                }
            }
            Sleep(5);
        }
        else
        {
            continue;
        }
    }
    return NULL;
}

int CMyCamera::Display(void* hWnd,  unsigned char *pdata, MV_CODEREADER_IMAGE_OUT_INFO_EX2* pstDisplayImage)
{
    int nRet = MV_CODEREADER_OK;

    if ((NULL == pdata)|| (NULL == pstDisplayImage))
    {
        return MV_CODEREADER_E_PARAMETER;
    }

    if ((0 == pstDisplayImage->nWidth ) ||
        (0 == pstDisplayImage->nHeight ) || 
        (NULL == hWnd))
    {
        return MV_CODEREADER_E_PARAMETER;
    }

    int nImageWidth = pstDisplayImage->nWidth;
    int nImageHeight = pstDisplayImage->nHeight;

    // 显示图像
    HDC hDC  = ::GetDC((HWND)hWnd);
    SetStretchBltMode(hDC, COLORONCOLOR);
    RECT wndRect;
    ::GetClientRect((HWND)hWnd, &wndRect);

    int nWndRectWidth  = wndRect.right  - wndRect.left;
    int nWndRectHeight = wndRect.bottom - wndRect.top;

    int nDstX      = wndRect.left;
    int nDstY      = wndRect.top;
    int nDstWidth  = (int)(nWndRectWidth);
    int nDstHeight = (int)(nWndRectHeight);

    int nSrcX      = 0;
    int nSrcY      = 0;
    int nSrcWidth  = (int)(nImageWidth);
    int nSrcHeight = (int)(nImageHeight);

    // 给结构体赋值
    m_stParam.hDC = hDC;
    m_stParam.nDstX = nDstX;
    m_stParam.nDstY = nDstY;
    m_stParam.nImageHeight = nImageHeight;
    m_stParam.nImageWidth = nImageWidth;
    m_stParam.nWndRectHeight = nWndRectHeight;
    m_stParam.nWndRectWidth = nWndRectWidth;

    // 判断图像格式
    if (PixelType_CodeReader_Gvsp_Jpeg == pstDisplayImage->enPixelType)
    {
        memset(m_pstParam.pBufOutput, 0, nImageWidth * nImageHeight);
        m_pstParam.pBufInput = pdata;
        m_pstParam.nBufInputLen = nImageWidth * nImageHeight;
        nRet =  MvJpgDecompress(&m_pstParam);
        if (MV_CODEREADER_OK == nRet)
        {
            // 将Jpeg数据格式解压的数据封装为结构体，赋值作为参数传入，进行渲染
            memcpy(m_stParam.pData, m_pstParam.pBufOutput, m_pstParam.nBufOutputLen);
            nRet = Draw(&m_stParam);
            if (MV_CODEREADER_OK != nRet)
            {
                return nRet = MV_CODEREADER_E_PARAMETER;
            }
        }
        else
        {
            // 解压失败
			return nRet = MV_CODEREADER_E_ABNORMAL_IMAGE;
        }
    }
    else
    {
        // 除Jpeg格式外的相机数据渲染
        memset(m_stParam.pData, 0, nImageWidth * nImageHeight);
        // 相机获取的数据直接渲染
        memcpy(m_stParam.pData, pdata, nImageWidth * nImageHeight);
        nRet = Draw(&m_stParam);
        if (MV_CODEREADER_OK != nRet)
        {
            return MV_CODEREADER_E_PARAMETER;
        }
    }

    // 画框
    Graphics gCode((HWND)hWnd);

    Status nGdiStatus = Status::Ok;
    Pen pen(Color(0, 0, 255), 3);

    float fWidthProportion = (float)nWndRectWidth / nImageWidth;
    float fHeightProportion = (float)nWndRectHeight / nImageHeight;

    MV_CODEREADER_RESULT_BCR_EX* stBcrResult = (MV_CODEREADER_RESULT_BCR_EX*)pstDisplayImage->pstCodeListEx;

    for (int i = 0; i < stBcrResult->nCodeNum; i++)
    {
        PointF point1((stBcrResult->stBcrInfoEx[i].pt[0].x * fWidthProportion), (stBcrResult->stBcrInfoEx[i].pt[0].y * fHeightProportion));
        PointF point2((stBcrResult->stBcrInfoEx[i].pt[1].x * fWidthProportion), (stBcrResult->stBcrInfoEx[i].pt[1].y * fHeightProportion));
        PointF point3((stBcrResult->stBcrInfoEx[i].pt[2].x * fWidthProportion), (stBcrResult->stBcrInfoEx[i].pt[2].y * fHeightProportion));
        PointF point4((stBcrResult->stBcrInfoEx[i].pt[3].x * fWidthProportion), (stBcrResult->stBcrInfoEx[i].pt[3].y * fHeightProportion));
        PointF points[4] = {point1, point2, point3, point4};
        PointF* pPoints = points;
        gCode.DrawPolygon(&pen, pPoints, 4);
    }

	MV_CODEREADER_OCR_INFO_LIST* pOcrList = (MV_CODEREADER_OCR_INFO_LIST*)pstDisplayImage->UnparsedOcrList.pstOcrList;
	for (int i = 0; i < pOcrList->nOCRAllNum; i++)
	{
		int x = pOcrList->stOcrRowInfo[i].nOcrRowCenterX * fWidthProportion;
		int y = pOcrList->stOcrRowInfo[i].nOcrRowCenterY * fHeightProportion;
		int w = pOcrList->stOcrRowInfo[i].nOcrRowWidth * fWidthProportion;
		int h = pOcrList->stOcrRowInfo[i].nOcrRowHeight * fHeightProportion;

		InnerDrawShape(&gCode, x, y, w, h, pOcrList->stOcrRowInfo[i].fOcrRowAngle);
	}
    ::ReleaseDC((HWND)hWnd, hDC);

    return nRet;
}

unsigned int CMyCamera::InnerDrawShape(Graphics* g, float x, float y, float w, float h, float fAngle)
{
	/* 路径初始化 */
	Status nGdiStatus = Status::Ok;
	GraphicsPath m_stShapePath;    ///< 图形路径，内部变量 
	nGdiStatus = m_stShapePath.Reset();
	if ( Status::Ok != nGdiStatus )
	{
		return MV_CODEREADER_E_SUPPORT;
	}

	float fGdiAngle = fAngle;
	if (fGdiAngle < 0)
	{
		fGdiAngle += 360;
	}

	/* 添加当前矩形至路径 */
	nGdiStatus = m_stShapePath.AddRectangle(RectF(x - w * 0.5, y - h*0.5, w, h));
	if ( Status::Ok != nGdiStatus )
	{
		return MV_CODEREADER_E_SUPPORT;
	}
	/* 根据角度旋转路径 */
	//Matrix* stRotateM = new Matrix();
	Matrix stRotateM;
	PointF stCenPoint( x, y );
	stRotateM.RotateAt( fAngle, stCenPoint );
	nGdiStatus = m_stShapePath.Transform(&stRotateM);

	if ( Status::Ok != nGdiStatus )
	{
		return MV_CODEREADER_E_SUPPORT;
	}

	/* 根据是否选中用不同画笔绘制图形 */
	Pen pen2(Color(255, 255, 0), 3);

	nGdiStatus = g->DrawPath(&pen2, &m_stShapePath);
	if ( Status::Ok != nGdiStatus )
	{
		return MV_CODEREADER_E_SUPPORT;
	}

	return MV_CODEREADER_OK;
}

int  CMyCamera::Draw(MV_CODEREADER_DRAW_PARAM* pstParam)
{
    if (NULL == pstParam)
    {
        return MV_CODEREADER_E_PARAMETER;
    }   

    int nImageWidth = pstParam->nImageWidth;
    int nImageHeight = pstParam->nImageHeight;
    int nDstWidth  = (int)(pstParam->nWndRectWidth);
    int nDstHeight = (int)(pstParam->nWndRectHeight);
    int nSrcX      = 0;
    int nSrcY      = 0;
    int nSrcWidth  = (int)(nImageWidth);
    int nSrcHeight = (int)(nImageHeight);

    if (NULL == m_bBitmapInfo)
    {
        m_bBitmapInfo = (PBITMAPINFO)malloc(sizeof(BITMAPINFO) + 256 * sizeof(RGBQUAD));
        memset(m_bBitmapInfo, 0, sizeof(sizeof(BITMAPINFO) + 256 * sizeof(RGBQUAD)));
    }
    // 位图信息头
    m_bBitmapInfo->bmiHeader.biSize = sizeof(BITMAPINFOHEADER);             // BITMAPINFOHEADER结构长度
    m_bBitmapInfo->bmiHeader.biWidth = nImageWidth;                         // 图像宽度
    m_bBitmapInfo->bmiHeader.biPlanes = 1;                                  // 位面数
    m_bBitmapInfo->bmiHeader.biBitCount = 8;                                // 比特数/像素的颜色深度,2^8=256
    m_bBitmapInfo->bmiHeader.biCompression = BI_RGB;                        // 图像数据压缩类型,BI_RGB表示不压缩
    m_bBitmapInfo->bmiHeader.biSizeImage = nImageWidth * nImageHeight;      // 图像大小
    m_bBitmapInfo->bmiHeader.biHeight = - nImageHeight;                     // 图像高度

    for(int i = 0; i < 256; i++)
    {
        m_bBitmapInfo->bmiColors[i].rgbBlue = m_bBitmapInfo->bmiColors[i].rgbRed = m_bBitmapInfo->bmiColors[i].rgbGreen = i;
        m_bBitmapInfo->bmiColors[i].rgbReserved = 0;
    }

    int nRet = StretchDIBits(pstParam->hDC,
        pstParam->nDstX, pstParam->nDstY, nDstWidth, nDstHeight,
        nSrcX, nSrcY, nSrcWidth, nSrcHeight, pstParam->pData, m_bBitmapInfo, DIB_RGB_COLORS, SRCCOPY);

    return MV_CODEREADER_OK;
}

int     CMyCamera::GetOneFrameTimeout(unsigned char* pData, unsigned int* pnDataLen, unsigned int nDataSize, MV_CODEREADER_IMAGE_OUT_INFO_EX2* pFrameInfo, int nMsec)
{
    if (NULL == pnDataLen)
    {
        return MV_CODEREADER_E_PARAMETER;
    }

    int nRet = MV_CODEREADER_OK;
    *pnDataLen  = 0;
    nRet = MV_CODEREADER_GetOneFrameTimeoutEx2(m_hDevHandle, &pData, pFrameInfo, nMsec);
    if (MV_CODEREADER_OK != nRet)
    {
        return nRet;
    }

    *pnDataLen = pFrameInfo->nFrameLen;
    return nRet;
}


int CMyCamera::SaveImage(MV_CODEREADER_SAVE_IMAGE_PARAM_EX* pstParam)
{
    if (NULL == pstParam)
    {
        return MV_CODEREADER_E_PARAMETER;
    }

    return MV_CODEREADER_SaveImage(m_hDevHandle, pstParam);
}


// ch:注册消息异常回调 | en:Register Message Exception CallBack
int     CMyCamera::RegisterExceptionCallBack(void(__stdcall* cbException)(unsigned int nMsgType, void* pUser),void* pUser)
{
    return MV_CODEREADER_RegisterExceptionCallBack(m_hDevHandle, cbException, pUser);
}


// ch:获取Int型参数，如 Width和Height，详细内容参考SDK安装目录下的 MvCameraNode.xlsx 文件
// en:Get Int type parameters, such as Width and Height, for details please refer to MvCameraNode.xlsx file under SDK installation directory
int     CMyCamera::GetIntValue(IN const char* strKey, OUT unsigned int *pnValue)
{
    if (NULL == strKey || NULL == pnValue)
    {
        return MV_CODEREADER_E_PARAMETER;
    }

    MV_CODEREADER_INTVALUE_EX stParam;
    memset(&stParam, 0, sizeof(MV_CODEREADER_INTVALUE_EX));
    int nRet = MV_CODEREADER_GetIntValue(m_hDevHandle, strKey, &stParam);
    if (MV_CODEREADER_OK != nRet)
    {
        return nRet;
    }
    *pnValue = stParam.nCurValue;

    return MV_CODEREADER_OK;
}


// ch:设置Int型参数，如 Width和Height，详细内容参考SDK安装目录下的 MvCameraNode.xlsx 文件
// en:Set Int type parameters, such as Width and Height, for details please refer to MvCameraNode.xlsx file under SDK installation directory
int     CMyCamera::SetIntValue(IN const char* strKey, IN unsigned int nValue)
{
    if (NULL == strKey)
    {
        return MV_CODEREADER_E_PARAMETER;
    }

    return MV_CODEREADER_SetIntValue(m_hDevHandle, strKey, nValue);
}


// ch:获取Float型参数，如 ExposureTime和Gain，详细内容参考SDK安装目录下的 MvCameraNode.xlsx 文件
// en:Get Float type parameters, such as ExposureTime and Gain, for details please refer to MvCameraNode.xlsx file under SDK installation directory
int     CMyCamera::GetFloatValue(IN const char* strKey, OUT float *pfValue)
{
    if (NULL == strKey || NULL == pfValue)
    {
        return MV_CODEREADER_E_PARAMETER;
    }

    MV_CODEREADER_FLOATVALUE stParam;
    memset(&stParam, 0, sizeof(MV_CODEREADER_FLOATVALUE));
    int nRet = MV_CODEREADER_GetFloatValue(m_hDevHandle, strKey, &stParam);
    if (MV_CODEREADER_OK != nRet)
    {
        return nRet;
    }

    *pfValue = stParam.fCurValue;

    return MV_CODEREADER_OK;
}


// ch:设置Float型参数，如 ExposureTime和Gain，详细内容参考SDK安装目录下的 MvCameraNode.xlsx 文件
// en:Set Float type parameters, such as ExposureTime and Gain, for details please refer to MvCameraNode.xlsx file under SDK installation directory
int     CMyCamera::SetFloatValue(IN const char* strKey, IN float fValue)
{
    if (NULL == strKey)
    {
        return MV_CODEREADER_E_PARAMETER;
    }
    return MV_CODEREADER_SetFloatValue(m_hDevHandle, strKey, fValue);
}


// ch:获取Enum型参数，如 PixelFormat，详细内容参考SDK安装目录下的 MvCameraNode.xlsx 文件
// en:Get Enum type parameters, such as PixelFormat, for details please refer to MvCameraNode.xlsx file under SDK installation directory
int     CMyCamera::GetEnumValue(IN const char* strKey, OUT unsigned int *pnValue)
{
    if (NULL == strKey || NULL == pnValue)
    {
        return MV_CODEREADER_E_PARAMETER;
    }

    MV_CODEREADER_ENUMVALUE stParam;
    memset(&stParam, 0, sizeof(MV_CODEREADER_ENUMVALUE));
    int nRet = MV_CODEREADER_GetEnumValue(m_hDevHandle, strKey, &stParam);
    if (MV_CODEREADER_OK != nRet)
    {
        return nRet;
    }

    *pnValue = stParam.nCurValue;
    return MV_CODEREADER_OK;
}


// ch:设置Enum型参数，如 PixelFormat，详细内容参考SDK安装目录下的 MvCameraNode.xlsx 文件
// en:Set Enum type parameters, such as PixelFormat, for details please refer to MvCameraNode.xlsx file under SDK installation directory
int     CMyCamera::SetEnumValue(IN const char* strKey, IN unsigned int nValue)
{
    if (NULL == strKey)
    {
        return MV_CODEREADER_E_PARAMETER;
    }

    return MV_CODEREADER_SetEnumValue(m_hDevHandle, strKey, nValue);
}


// ch:获取Bool型参数，如 ReverseX，详细内容参考SDK安装目录下的 MvCameraNode.xlsx 文件
// en:Get Bool type parameters, such as ReverseX, for details please refer to MvCameraNode.xlsx file under SDK installation directory
int     CMyCamera::GetBoolValue(IN const char* strKey, OUT bool *pbValue)
{
    if (NULL == strKey || NULL == pbValue)
    {
        return MV_CODEREADER_E_PARAMETER;
    }

    return MV_CODEREADER_GetBoolValue(m_hDevHandle, strKey, pbValue);
}


// ch:设置Bool型参数，如 ReverseX，详细内容参考SDK安装目录下的 MvCameraNode.xlsx 文件
// en:Set Bool type parameters, such as ReverseX, for details please refer to MvCameraNode.xlsx file under SDK installation directory
int     CMyCamera::SetBoolValue(IN const char* strKey, IN bool bValue)
{
    if (NULL == strKey)
    {
        return MV_CODEREADER_E_PARAMETER;
    }

    return MV_CODEREADER_SetBoolValue(m_hDevHandle, strKey, bValue);
}


// ch:获取String型参数，如 DeviceUserID，详细内容参考SDK安装目录下的 MvCameraNode.xlsx 文件UserSetSave
// en:Get String type parameters, such as DeviceUserID, for details please refer to MvCameraNode.xlsx file under SDK installation directory
int     CMyCamera::GetStringValue(IN const char* strKey, IN OUT char* strValue, IN unsigned int nSize)
{
    if (NULL == strKey || NULL == strValue)
    {
        return MV_CODEREADER_E_PARAMETER;
    }

    MV_CODEREADER_STRINGVALUE stParam;
    memset(&stParam, 0, sizeof(MV_CODEREADER_STRINGVALUE));
    int nRet = MV_CODEREADER_GetStringValue(m_hDevHandle, strKey, &stParam);
    if (MV_CODEREADER_OK != nRet)
    {
        return nRet;
    }

    strcpy_s(strValue, nSize, stParam.chCurValue);

    return MV_CODEREADER_OK;
}


// ch:设置String型参数，如 DeviceUserID，详细内容参考SDK安装目录下的 MvCameraNode.xlsx 文件UserSetSave
// en:Set String type parameters, such as DeviceUserID, for details please refer to MvCameraNode.xlsx file under SDK installation directory
int     CMyCamera::SetStringValue(IN const char* strKey, IN const char* strValue)
{
    if (NULL == strKey)
    {
        return MV_CODEREADER_E_PARAMETER;
    }

    return MV_CODEREADER_SetStringValue(m_hDevHandle, strKey, strValue);
}


int CMyCamera::GetOptimalPacketSize()
{
    return MV_CODEREADER_GetOptimalPacketSize(m_hDevHandle);
}

int     CMyCamera::SetCommandValue(IN const char* strKey)
{
    if (NULL == strKey)
    {
        return MV_CODEREADER_E_PARAMETER;
    }
    return MV_CODEREADER_SetCommandValue(m_hDevHandle, "TriggerSoftware");
}
