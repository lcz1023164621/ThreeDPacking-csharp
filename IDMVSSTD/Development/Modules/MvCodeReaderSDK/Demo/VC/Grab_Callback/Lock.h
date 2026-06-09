#pragma once
#ifndef _LOCK_H_
#define _LOCK_H_

#include <Windows.h>
#include <stdio.h>

class CMyLock
{
public:
    virtual ~CMyLock() {}
    virtual void Lock() const = 0;
    virtual void UnLock() const = 0;
};

class CMutex: public CMyLock
{
public:
    CMutex();
    ~CMutex();

    virtual void Lock() const;
    virtual void UnLock() const;

private:
    HANDLE m_mutex;
};

// Ëø
class CLock
{
public:
    CLock(const CMyLock&);
    ~CLock(void);

private:
    const CMyLock& m_lock;
};



#endif
