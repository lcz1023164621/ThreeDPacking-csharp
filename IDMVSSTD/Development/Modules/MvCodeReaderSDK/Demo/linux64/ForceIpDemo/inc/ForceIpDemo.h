#ifndef ForceIpDemo_H
#define ForceIpDemo_H

#include <QWidget>
#include <QMessageBox>
#include <QPushButton>
#include <arpa/inet.h>
#include <QString>
#include <QDebug>
#include <QThread>
#include "MvCodeReaderCtrl.h"
#include "MvCodeReaderErrorDefine.h"
#include "MvCodeReaderParams.h"

#define IP_CONFIG_STATIC 5
#define IP_CONFIG_DHCP   6
#define IP_CONFIG_LLA    4

namespace Ui {
class ForceIpDemo;
}

class CForceIpDemo : public QWidget
{
    Q_OBJECT

public:
    /******************************************************************
     * @fn      CForceIpDemo()
     * @brief   CForceIpDemo类的构造函数
     * param    QWidget *parent = 0      [IN]    父类指针

     * @fn      CForceIpDemo()
     * @brief   Constructor of CForceIpDemo class
     * param    QWidget *parent = 0      [IN]    Parent class pointer
    ******************************************************************/
    explicit CForceIpDemo(QWidget *parent = 0);

    /******************************************************************
     * @fn      ~CForceIpDemo()
     * @brief   CForceIpDemo类的析构函数

     * @fn      ~CForceIpDemo()
     * @brief   Destructor of CForceIpDemo class
    ******************************************************************/
    ~CForceIpDemo();

private slots:

    /******************************************************************
     * @fn      on_BtnEnumDevice_clicked()
     * @brief   枚举设备

     * @fn      on_BtnEnumDevice_clicked()
     * @brief   Enumerate devices
    ******************************************************************/
    void on_BtnEnumDevice_clicked();

    /******************************************************************
     * @fn      on_BtnSetIp_clicked()
     * @brief   设置IP

     * @fn      on_BtnSetIp_clicked()
     * @brief   Set IP
    ******************************************************************/
    void on_BtnSetIp_clicked();

    /******************************************************************
     * @fn      on_comBoxDevice_currentIndexChanged()
     * @brief   当前设备切换

     * @fn      on_comBoxDevice_currentIndexChanged()
     * @brief   Current device switching
    ******************************************************************/
    void on_comBoxDevice_currentIndexChanged();

    /******************************************************************
     * @fn      on_rBtnStatic_clicked()
     * @brief   静态IP

     * @fn      on_rBtnStatic_clicked()
     * @brief   Static IP
    ******************************************************************/
    void on_rBtnStatic_clicked();

    /******************************************************************
     * @fn      on_rBtnDHCP_clicked()
     * @brief   自动分配IP(DHCP)

     * @fn      on_rBtnDHCP_clicked()
     * @brief   Auto assign IP(DHCP)
    ******************************************************************/
    void on_rBtnDHCP_clicked();

    /******************************************************************
     * @fn      on_rBtnLLA_clicked()
     * @brief   自动分配IP(LLA)

     * @fn      on_rBtnLLA_clicked()
     * @brief   Auto assign IP(LLA)
    ******************************************************************/
    void on_rBtnLLA_clicked();


private:

    /******************************************************************
     * @fn      DisplayDeviceIp()
     * @brief   显示IP信息

     * @fn      DisplayDeviceIp()
     * @brief   Display IP information
    ******************************************************************/
    void DisplayDeviceIp();

    /******************************************************************
     * @fn      CurrentIpConfig()
     * @brief   更新当前IP配置方式

     * @fn      CurrentIpConfig()
     * @brief   Update the current IP configuration mode
    ******************************************************************/
    void CurrentIpConfig();

    MV_CODEREADER_DEVICE_INFO_LIST m_stDevList;       // 设备信息列表
    void*                          m_pHandle;         // 设备句柄
    unsigned int                   m_nIndex;          // 设备索引
    QString                        m_strIpRange;      //Ip范围
    Ui::ForceIpDemo *ui;
};

#endif // ForceIpDemo_H
