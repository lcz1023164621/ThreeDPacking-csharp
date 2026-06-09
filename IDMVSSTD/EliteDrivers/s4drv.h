/*
// copyright(c) Beijing Senselock.
// All rights reserved.
//
// filename: s4drv.h
//
// briefs: driver setup functions declaration
//
// date:	2004-11-04
*/

#ifndef	__SENSE4_DRIVER_SETUP_H__
#define __SENSE4_DRIVER_SETUP_H__

#include <windows.h>

/* install flag	*/
#define DRV_FLAG_PCSC				0x01			// install PC/SC features
#define DRV_FLAG_CLEAR_OLD			0x02			// clear old drivers

/* error code	*/
#define ERR_SUCCESS					0x00000000		// success

#define ERR_SYSTEM_UNSUPPORTED		0x00000001		// system not supported 
#define ERR_LOAD_LIBRARY			0x00000002		// library not found
#define ERR_GETTING_FUNCENTRY		0x00000003		// function not found
#define ERR_INVALID_PARAMETER		0x00000004		// invalid parameter
#define ERR_RESOURCE_LACK			0x00000005		// insufficient system resource(such as memory)

#define ERR_CREATE_DIRECTORY		0x00000101		// create directory failed (check the destination path)
#define ERR_GET_DIRECTTORY			0x00000102		// fail to get the windows directory
#define ERR_CREATE_DRVFILE			0x00000103		// fail to create driver files in destination directory
#define ERR_DELETE_DRVFILE			0x00000104		// fail to delete driver files in system directory
#define ERR_COPY_DRVFILE			0x00000105		// fail to copy driver files to system directory
#define ERR_WRITE_DRVFILE			0x00000106		// fail to write driver files in destination directory
#define ERR_SET_FILETIME			0x00000107		// fail to set files writing time

#define ERR_OPEN_REGKEY				0x00000201		// fail to open registry key
#define ERR_DELETE_REGKEY			0x00000202		// fail to delete registry key
#define ERR_ENUM_REGVALUE			0x00000203		// fail to enumerate registry value
#define ERR_QUERY_REGVALUE			0x00000204		// fail to query registry value
#define ERR_SET_REGVALUE			0x00000205		// fail to set registy value
#define ERR_CREATE_REGKEY			0x00000206		// fail to create the registry key	//

#define ERR_GETTING_DEVPRO			0x00000301		// fail to get device property
#define ERR_SETTING_DEVPRO			0x00000302		// fail to set device property
#define ERR_FINDING_DEVICE			0x00000303		// error on finding an existing device
#define ERR_CREATING_DEVICE			0x00000304		// fail to create a new device node
#define ERR_REGISTER_DEVICE			0x00000305		// fail to register a new device node
#define ERR_CREATING_DEVSET			0x00000306		// fail to create a new device set
#define ERR_GETTING_DEVSET			0x00000307		// fail to get an existing device set
#define ERR_GETTING_CLASS			0x00000308		// fail to get device class from .inf file
#define ERR_UPDATE_DRIVER			0x00000309		// fail to update the drivers of existing devices
#define ERR_OEMCOPY_INF				0x0000030A		// fail to copy oem file from .inf file
#define ERR_REMOVER_DEVICE			0x0000030B		// fail to delete an existing device node
#define ERR_LOCATE_DEVNODE			0x0000030C		// fail to locate the device node
#define ERR_REENUMERATE_DEVNODE		0x0000030D		// fail to re-enumerate device node
#define ERR_GETTING_DEVINSPARAM		0x0000030E		// fail to get the device install parameter
#define ERR_SETTING_DEVINSPARAM		0x0000030F		// fail to set the device install parameter
#define ERR_BUILDING_DRIVERLIST		0x00000310		// fail to build the device the device driver list
#define ERR_ENUM_DRIVER				0x00000311		// fail to enumerate the device driver
#define ERR_SETTING_DRIVER			0x00000312		// fail to set the device driver
#define ERR_INSTALL_DEVICE			0x00000313		// fail to install the device

#define ERR_SERVICE_MANAGER			0x00000401		// fail to open the service manager
#define ERR_SERVICE_CREATED			0x00000402		// fail to ceate a new service or open a existing service
#define ERR_SERVICE_QUERY			0x00000403		// error on querying an existing service status
#define ERR_SERVICE_STARTED			0x00000404		// fail to start an existing service
#define ERR_SERVICE_DELETED			0x00000405		// fail to delete an existing service

#define ERR_FIND_RESOURCE			0x00000501		// can't find a resource needed
#define ERR_LOAD_RESOURCE			0x00000502		// can't load the resource
#define ERR_LOCK_RESOURCE			0x00000503		// can't lock the resource

#define ERR_ACCESS_DENIED			0x00000504		// Do not have administrator privileges
#define ERR_IN_WOW64				0x00000505		// setup can not run in wow64

typedef struct _DRIVER_VERSION
{
	DWORD DriverNum;								// the number of drviers installed in the system
	CHAR Version[16][16];							// the versions installed in the system
} DRIVER_INFO, *PDRIVER_INFO;

#ifdef	__cplusplus
extern	"C" {
#endif

/*
// get information of installed drivers in the system
//
// parameter:
//		pDrvInfo [out] return the information of installed driver  in the system
//
// return:
//		0: success
//		others: error code
//
*/
DWORD WINAPI s4drv_GetDriverInfo(PDRIVER_INFO pDrvInfo);

/*
// install Drivers
// 
// parameter:
//		lpszDestPath: [in]where the the driver file to be installed, if the path does not exist, it will be created.
//		dwCount: [in]when ulFlag specifies DRV_FLAG_PCSC and the system is WIN 2K/XP/2003, input the reader number needed(between 0 and 8)
//				 else this parameter are ignored
//		dwFlag: [in]Drivers flag(PC/SC or no PC/SC)
//
// return:
//		0: SUCCESS
//		others: error occured
//
*/
DWORD WINAPI s4drv_Install(LPCSTR lpszDestPath, DWORD dwCount, DWORD dwFlag);

/*
// uninstall drivers
//
// parameter:
//		none
//
// return:
//		0: success
//		other: error occured
//
*/
DWORD WINAPI s4drv_Uninstall(LPCSTR lpszDestPath);

/*
// check whether the system need to reboot
//
// parameter:
//		none
//
// return:
//		TRUE: need reboot system
//		FALSE: not need reboot system 
//
*/
BOOL WINAPI s4drv_IsNeedReboot();

BOOL WINAPI s4drv_Reboot();

#ifdef	__cplusplus
}
#endif

#endif	//__SENSE4_DRIVER_SETUP_H__