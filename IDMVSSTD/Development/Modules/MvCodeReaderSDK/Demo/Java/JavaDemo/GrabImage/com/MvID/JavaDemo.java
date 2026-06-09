package com.MvID;

import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.OutputStream;
import java.util.ArrayList;

import MvCodeReaderCtrlWrapper.*;
import MvCodeReaderCtrlWrapper.MvCodeReaderCtrl.*;

import MvCodeReaderCtrlWrapper.MvCodeReaderCtrlDefine.*;

public class JavaDemo {
	
	static Handle hHandle = null;

	// Save Image API
	public static void saveDataToFile(byte[] dataToSave, int dataSize, String fileName)
    {
        OutputStream os = null;

        try
        {
            // Create directory
            File tempFile = new File("ImageSave");
            if (!tempFile.exists()) 
            {
                tempFile.mkdirs();
            }

            os = new FileOutputStream(tempFile.getPath() + File.separator + fileName);
            os.write(dataToSave, 0, dataSize);
            os.flush();
            os.close();
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

	// Print Device Info
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
	
	// Grabbing thread API
	public static class GrabbingThread extends Thread
	{
		public GrabbingThread()
		{			
		}
						
		public void run()
		{
			MV_CODEREADER_IMAGE_OUT_INFO_EX2 stOutInfo = new MV_CODEREADER_IMAGE_OUT_INFO_EX2();
			MV_CODEREADER_CAM_INTVALUE stValue = new MV_CODEREADER_CAM_INTVALUE();
			int nRet = 0;
			long payloadSize = 0;
			nRet = MvCodeReaderCtrl.MV_CODEREADER_GetIntValue(hHandle, "PayloadSize", stValue);
			if (0 == nRet)
			{
				payloadSize = stValue.nCurValue;
				System.out.print("GetOneFrame GetOptimalPacketSize[" + String.format("%d", payloadSize) + "]\r\n");
			}
			else
			{
				System.out.print("GetIntValue Failed, nRet[" + String.format("0x%x", nRet) + "]\r\n");
			}			

            
			MV_CODEREADER_SAVE_IMAGE_PARAM_EX stParam = new MV_CODEREADER_SAVE_IMAGE_PARAM_EX();
			byte[] pdata = new byte[(int)payloadSize];
			
			for(int i = 0; i < 5; i++)
			{
				if(null != hHandle)
				{
					nRet = MvCodeReaderCtrl.MV_CODEREADER_GetOneFrameTimeoutEx2(hHandle, pdata, stOutInfo, 1000);
					if(0 != nRet)
					{
						System.out.print("GetOneFrame Failed, nRet[" + String.format("0x%x", nRet) + "]\r\n");
					}
					else
					{
						// save buffer to file follow Image Type to Save 
						// saveDataToFile(pdata, stOutInfo.nFrameLen, "Image.jpg");
											 										
						System.out.print(String.format("Get One Frame: nEventID[%d], nChannelID[%d], nWidth[%d], nHeight[%d], nFrameNum[%d], nTriggerIndex[%d], nFrameLen[%d]"
								+ " nCodeNumber[%d]\r\n", 
								stOutInfo.nEventID, stOutInfo.nChannelID, stOutInfo.nWidth, stOutInfo.nHeight, stOutInfo.nFrameNum,
									stOutInfo.nTriggerIndex, stOutInfo.nFrameLen, stOutInfo.pstCodeListEx.nCodeNum));
								
						System.out.print("Get One Code: bIsGetCode[" + stOutInfo.bIsGetCode + "]success\r\n"); 
						
						System.out.print("Get GvspPixelType: MvCodeReaderGvspPixelType[" + stOutInfo.enPixelType + "]success\r\n"); 
						
						// print code info
						for(int a = 0; a < stOutInfo.pstCodeListEx.nCodeNum; a++)
						{
							System.out.print(String.format("CodeInfo: TheCodeID[%d], CodeString[%s], nCodeLen[%d], nAngle[%d], nBarType[%d]\r\n", 
									a, stOutInfo.pstCodeListEx.stBcrInfoEx.get(a).chCode, stOutInfo.pstCodeListEx.stBcrInfoEx.get(a).nLen, 
									stOutInfo.pstCodeListEx.stBcrInfoEx.get(a).nAngle, stOutInfo.pstCodeListEx.stBcrInfoEx.get(a).nBarType));
							
							System.out.print(String.format("CodePointInfo: stCornerLeftTop.X[%d], stCornerLeftTop.Y[%d], stCornerRightTop.X[%d], stCornerRightTop.Y[%d],"
									+ "stCornerRightBottom.X[%d], stCornerRightBottom.Y[%d], stCornerLeftBottom.X[%d], stCornerLeftBottom.Y[%d]\r\n", 
									stOutInfo.pstCodeListEx.stBcrInfoEx.get(a).stCornerLeftTop.nX, stOutInfo.pstCodeListEx.stBcrInfoEx.get(a).stCornerLeftTop.nY, 
									stOutInfo.pstCodeListEx.stBcrInfoEx.get(a).stCornerRightTop.nX, stOutInfo.pstCodeListEx.stBcrInfoEx.get(a).stCornerRightTop.nY, 
									stOutInfo.pstCodeListEx.stBcrInfoEx.get(a).stCornerRightBottom.nX, stOutInfo.pstCodeListEx.stBcrInfoEx.get(a).stCornerRightBottom.nY, 
									stOutInfo.pstCodeListEx.stBcrInfoEx.get(a).stCornerLeftBottom.nX, stOutInfo.pstCodeListEx.stBcrInfoEx.get(a).stCornerLeftBottom.nY));
							
						}
						
					}
				}
			}			
		}
	}
	
	
	public static void main(String[] args) throws InterruptedException {
	
		int nRet = 0;
		int i = 0;
		
		System.out.print(" *****************Begin*******************  \r\n");
				
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
			
			System.out.print("input camera index is [" + nIndex + "]\r\n" );
			
			try
			{
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
				System.out.print("Create handle failed\r\n");
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
						
			// register exception cb
			nRet = MvCodeReaderCtrl.MV_CODEREADER_RegisterExceptionCallBack(hHandle, new MvCodeReaderCtrlDefine.ExceptionCallBack()
			{
				@Override
				public int OnExceptionCallBack(int nMsg)
				{
					System.out.print("Open Device MV_CODEREADER_RegisterExceptionCallBack! nMsg[" + String.format("0x%x", nMsg) + "]\r\n");
					return 0;
				}	
			});
			
			if(0 != nRet)
			{
				System.out.print("MVID_CR_CAM_RegisterImageCallBack Failed! nRet[" + String.format("0x%x", nRet) + "]\r\n");
				break;
			}		
			
			nRet = MvCodeReaderCtrl.MV_CODEREADER_StartGrabbing(hHandle);
			if(0 != nRet){
				System.out.print("StartGrabbing Failed! nRet[" + String.format("0x%x", nRet) + "]\r\n");
				break;
			}
			else
			{
				System.out.print("Device startGrabbing success!\r\n");	
			}
			
			// Create recv thread
			try
			{
				GrabbingThread grabThread = new JavaDemo.GrabbingThread();
				grabThread.start();
				Thread.sleep(12000);
				grabThread.interrupt();
	        } catch (InterruptedException e) {
	            e.printStackTrace();
	        }
			
			nRet = MvCodeReaderCtrl.MV_CODEREADER_StopGrabbing(hHandle); 
			if(0 != nRet){
			    System.out.print("StopGrabbing failed! nRet[" + String.format("0x%x", nRet) + "]\r\n");
				break;
			}	
			else
			{
				System.out.print("\r\nDevice stopGrabbing success!\r\n");	
			}			
	        
		}while(false);
		
		if (null != hHandle)
		{
			MvCodeReaderCtrl.MV_CODEREADER_DestroyHandle(hHandle);
		}
		
		System.out.print("Exit.\r\n");
	}

}
