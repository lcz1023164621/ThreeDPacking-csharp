#include "Lock.h"

// 创建一个匿名互斥对象
CMutex::CMutex()
{
    m_mutex = NULL;
    m_mutex = CreateMutex(NULL, FALSE, NULL);
}

// 销毁互斥对象
CMutex::~CMutex()
{
    if (m_mutex)
    {
        CloseHandle(m_mutex);
        m_mutex = NULL;
    }
}

void CMutex::Lock() const
{
    DWORD d = WaitForSingleObject(m_mutex, INFINITE);
}

void CMutex::UnLock() const
{
    ReleaseMutex(m_mutex);
}

CLock::CLock(const CMyLock& m):m_lock(m)
{
    m_lock.Lock();
}

CLock::~CLock(void)
{
    m_lock.UnLock();
}


