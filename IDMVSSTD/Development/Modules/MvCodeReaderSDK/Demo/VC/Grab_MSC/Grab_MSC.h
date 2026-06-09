
// Grab_MSC.h : main header file for the PROJECT_NAME application
//

#pragma once

#ifndef __AFXWIN_H__
	#error "include 'stdafx.h' before including this file for PCH"
#endif

#include "resource.h"		// main symbols


// CGrab_MSCApp:
// See Grab_MSC.cpp for the implementation of this class
//

class CGrab_MSCApp : public CWinAppEx
{
public:
	CGrab_MSCApp();

// Overrides
	public:
	virtual BOOL InitInstance();

// Implementation

	DECLARE_MESSAGE_MAP()
};

extern CGrab_MSCApp theApp;