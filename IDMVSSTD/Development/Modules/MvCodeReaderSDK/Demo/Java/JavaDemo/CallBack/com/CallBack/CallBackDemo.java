package com.CallBack;

import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.OutputStream;
import java.util.ArrayList;

import MvCodeReaderCtrlWrapper.*;
import MvCodeReaderCtrlWrapper.MvCodeReaderCtrl.*;
import MvCodeReaderCtrlWrapper.MvCodeReaderCtrlDefine.*;
import MvCodeReaderCtrlWrapper.ParameterException.*;

public class CallBackDemo {

	static Handle hHandle = null;

	public static void saveDataToFile(byte[] dataToSave, int dataSize, String fileName)
    {
        OutputStream os = null;

        try
        {
            // Create saveImg directory
            File tempFile = new File("saveImg");
            if (!tempFile.exists()) 
            {
                tempFile.mkdirs();
            }

            os = new FileOutputStream(tempFile.getPath() + File.separator + fileName);
            os.write(dataToSave, 0, dataSize);
            System.out.println("SaveImage succeed.");
        }
        catch (IOException e)
        {
            e.printStackTrace();
        }
        finally
        {
            // Close file stream
            try 
            {
                os.close();
            } 
            catch (IOException e) 
            {
                e.printStackTrace();
            }
        }
    }
	
	// CallBack function 
	private static void printImgCBInfo(byte[] pdata, MV_CODEREADER_IMAGE_OUT_INFO_EX2 stOutInfo)
	{
		if (null == stOutInfo)
		{
			System.out.println("stOutInfo is null");
			return ;
		}
		
		System.out.print("/**CBpstOutInfo***************************************/\n");
		
		// save buffer to file follow Image Type to Save 
		// saveDataToFile(pdata, stOutInfo.nFrameLen, "Image.jpg");
		// saveDataToFile(pdata, stOutInfo.nFrameLen, "Image.raw");
						
		System.out.print(String.format("Get One Frame: nEventID[%d], nChannelID[%d], nWidth[%d], nHeight[%d], nFrameNum[%d], nTriggerIndex[%d], nFrameLen[%d], "
				+ " nCodeNumber[%d] \r\n", 
				stOutInfo.nEventID, stOutInfo.nChannelID, stOutInfo.nWidth, stOutInfo.nHeight, stOutInfo.nFrameNum,
					stOutInfo.nTriggerIndex, stOutInfo.nFrameLen, stOutInfo.pstCodeListEx.nCodeNum));
		
		System.out.print("Get One Code: bIsGetCode[" + stOutInfo.bIsGetCode + "]success\r\n"); 
		
		System.out.print("Get GvspPixelType: MvCodeReaderGvspPixelType[" + stOutInfo.enPixelType + "]success\r\n");
		
		// print code info
		for(int a = 0; a < stOutInfo.pstCodeListEx.nCodeNum; a++)
		{
			System.out.print(String.format("CodeInfo: TheCodeID[%d], CodeString[%s], nCodeLen[%d], nAngle[%d], nBarType[%d],"
					+ "sAlgoCost[%d], nIDRScore[%d], n1DIsGetQuality[%d]\r\n", 
					a, stOutInfo.pstCodeListEx.stBcrInfoEx.get(a).chCode, stOutInfo.pstCodeListEx.stBcrInfoEx.get(a).nLen, 
					stOutInfo.pstCodeListEx.stBcrInfoEx.get(a).nAngle, stOutInfo.pstCodeListEx.stBcrInfoEx.get(a).nBarType,
					stOutInfo.pstCodeListEx.stBcrInfoEx.get(a).sAlgoCost, stOutInfo.pstCodeListEx.stBcrInfoEx.get(a).nIDRScore,
					stOutInfo.pstCodeListEx.stBcrInfoEx.get(a).n1DIsGetQuality));
			
			System.out.print(String.format("CodePointInfo: stCornerLeftTop.X[%d], stCornerLeftTop.Y[%d], stCornerRightTop.X[%d], stCornerRightTop.Y[%d],"
					+ "stCornerRightBottom.X[%d], stCornerRightBottom.Y[%d], stCornerLeftBottom.X[%d], stCornerLeftBottom.Y[%d]\r\n", 
					stOutInfo.pstCodeListEx.stBcrInfoEx.get(a).stCornerLeftTop.nX, stOutInfo.pstCodeListEx.stBcrInfoEx.get(a).stCornerLeftTop.nY, 
					stOutInfo.pstCodeListEx.stBcrInfoEx.get(a).stCornerRightTop.nX, stOutInfo.pstCodeListEx.stBcrInfoEx.get(a).stCornerRightTop.nY, 
					stOutInfo.pstCodeListEx.stBcrInfoEx.get(a).stCornerRightBottom.nX, stOutInfo.pstCodeListEx.stBcrInfoEx.get(a).stCornerRightBottom.nY, 
					stOutInfo.pstCodeListEx.stBcrInfoEx.get(a).stCornerLeftBottom.nX, stOutInfo.pstCodeListEx.stBcrInfoEx.get(a).stCornerLeftBottom.nY));
		
			System.out.print("Get CodeQuality: bIsGetQuality[" + stOutInfo.pstCodeListEx.stBcrInfoEx.get(a).bIsGetQuality + "]success\r\n"); 
		} 			
	}
	
	private static void PrintDeviceInfo(MV_CODEREADER_DEVICE_INFO stCamInfo)
	{
		if(stCamInfo.nTLayerType == MvCodeReaderCtrlDefine.MV_CODEREADER_GIGE_DEVICE)
		{
			int nIp1 = ((stCamInfo.stGigEInfo.nCurrentIp & 0xff000000) >> 24) & 0xff;
	        int nIp2 = ((stCamInfo.stGigEInfo.nCurrentIp & 0x00ff0000) >> 16);
	        int nIp3 = ((stCamInfo.stGigEInfo.nCurrentIp & 0x0000ff00) >> 8);
	        int nIp4 = (stCamInfo.stGigEInfo.nCurrentIp & 0x000000ff);
	                
	        System.out.print("CurrentIp: " + nIp1 + "." + nIp2 + "." + nIp3 + "." + nIp4 + "\r\n");
			
			System.out.print(String.format("GiGEInfo: UserDefinedName:[%s], chSerialNumber:[%s] \r\n\r\n", 
					stCamInfo.stGigEInfo.chUserDefinedName, stCamInfo.stGigEInfo.chSerialNumber));
		}
	}
	
		
	public static void main(String[] args) throws InterruptedException {
		
		int nRet = 0;
		int i = 0;
		
		System.out.print(" ***************Begin******************  \r\n");
		
		String strVersion = MvCodeReaderCtrl.MV_CODEREADER_GetSDKVersion();
		System.out.print("Get version " + strVersion + "\r\n");
		
		do
		{
			// Enum device
			ArrayList<MV_CODEREADER_DEVICE_INFO> stCamList = MvCodeReaderCtrl.MV_CODEREADER_EnumDevices();
			
			if (stCamList == null)
			{
				System.out.print("Find No Device!\r\n");
				break;		
			}
			
			for(MV_CODEREADER_DEVICE_INFO stCamInfo:stCamList)
			{
			
				System.out.print("[device " + i + "]:\r\n");
				PrintDeviceInfo(stCamInfo);
				i++;
			}
			
			int nIndex = 0;
			System.out.print("Please input camera index: ");
			
			try 
			{
				nIndex = System.in.read() - 48;	
				
			} catch (IOException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
				System.out.print("Input Error!\r\n");
				break;
			}
			
			try
			{
				MV_CODEREADER_DEVICE_INFO deviceInfo = new MV_CODEREADER_DEVICE_INFO();
				
				hHandle = MvCodeReaderCtrl.MV_CODEREADER_CreateHandle(stCamList.get(nIndex));
				if(null == hHandle)
				{
					System.out.print("Create handle failed! \r\n");
					break;
				}
			}
			catch(Exception e)
			{
				e.printStackTrace();
				System.out.print("Create Handle Failed \r\n");
				break;
			}
			
			nRet = MvCodeReaderCtrl.MV_CODEREADER_OpenDevice(hHandle);
			if(0 != nRet){
				System.out.print("Open Device fail! nRet[" + String.format("0x%x", nRet) + "]\r\n");
				break;
			}
			else
			{
				System.out.print("Open device success!\r\n");	
			}
	        
			// Register MvCodeReaderCtrl.MV_CODEREADER_RegisterImageCallBackEx2   
			nRet = MvCodeReaderCtrl.MV_CODEREADER_RegisterImageCallBackEx2(hHandle, new ImageCallBack()
			{
				@Override
				public int OnImageCallBack(byte[] pdata, MV_CODEREADER_IMAGE_OUT_INFO_EX2 stOutInfo) {
					// TODO Auto-generated method stub
					printImgCBInfo( pdata, stOutInfo);

					return 0;
				}
			});

			if(0 != nRet)
			{
				System.out.print("MV_CODEREADER_RegisterImageCallBackEx2 Failed! nRet[" + String.format("0x%x", nRet) + "]\r\n");
				break;
			}
			
			System.out.print("MV_CODEREADER_RegisterImageCallBackEx2 ! nRet[" + String.format("0x%x", nRet) + "]\r\n");
			
			nRet = MvCodeReaderCtrl.MV_CODEREADER_StartGrabbing(hHandle);
			if(0 != nRet){
				System.out.print("StartGrabbing Failed! nRet[" + String.format("0x%x", nRet) + "]\r\n");
				break;
			}
			else
			{
				System.out.print("Device startGrabbing success!\r\n");	
			}
			
			// Wait CallBack Data
			try
            {
                Thread.sleep(1000 * 1000);
            }
            catch (InterruptedException e)
            {
                e.printStackTrace();
            }
			
			MvCodeReaderCtrl.MV_CODEREADER_StopGrabbing(hHandle);   
			if(0 != nRet){
			    System.out.print("StopGrabbing failed! nRet[" + String.format("0x%x", nRet) + "]\r\n");
				break;
			}	
			else
			{
				System.out.print("Device stopGrabbing success!\r\n");	
			}
	        
		}while(false);
		
		if (null != hHandle)
		{
			MvCodeReaderCtrl.MV_CODEREADER_DestroyHandle(hHandle);
		}
		
		System.out.print("exit.\r\n");
	}

}

