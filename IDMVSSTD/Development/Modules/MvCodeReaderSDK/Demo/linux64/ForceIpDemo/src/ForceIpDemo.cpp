#include "ForceIpDemo.h"
#include "ui_ForceIpDemo.h"

CForceIpDemo::CForceIpDemo(QWidget *parent) :
    QWidget(parent),
    ui(new Ui::ForceIpDemo)
{
    ui->setupUi(this);
    memset(&m_stDevList, 0, sizeof(MV_CODEREADER_DEVICE_INFO_LIST));
    m_pHandle = nullptr;
    m_nIndex = 0;
}

CForceIpDemo::~CForceIpDemo()
{
    delete ui;
}

// ch: 枚举设备 | en: Enumerate devices
void CForceIpDemo::on_BtnEnumDevice_clicked()
{
    QString strDevInfo = "";
    QString strErrInfo = "";

    // ch: 清除设备列表框中的信息 | en: Clear the information in the device list
    ui->comBoxDevice->clear();

    // ch: 初始化设备信息列表 | en: initialize device list
    memset(&m_stDevList, 0, sizeof(MV_CODEREADER_DEVICE_INFO_LIST));

    // ch: 枚举子网内所有设备 | en: Enumerate all the devices in the subnetworks
    int nRet = MV_CODEREADER_EnumDevices(&m_stDevList, MV_CODEREADER_GIGE_DEVICE);
    if (MV_CODEREADER_OK != nRet)
    {
        // ch: 枚举设备失败  | en:Failed to enumerate devices
        strErrInfo = tr("Failed to enumerate devices ") + QString::number(nRet, 16).mid(8, 8);
        QMessageBox::critical(this, tr("ERROR"), strErrInfo, QMessageBox::Ok);
        return;
    }
    // ch: 将设备信息显示在列表框 | en: Display device information in the list box
    int nIp1 = 0;
    int nIp2 = 0;
    int nIp3 = 0;
    int nIp4 = 0;
    for (unsigned int i = 0; i < m_stDevList.nDeviceNum; i++)
    {
        MV_CODEREADER_DEVICE_INFO* pDeviceInfo = m_stDevList.pDeviceInfo[i];
        if (NULL == pDeviceInfo)
        {
            continue;
        }
        if (pDeviceInfo->nTLayerType == MV_CODEREADER_GIGE_DEVICE)
        {
            nIp1 = ((m_stDevList.pDeviceInfo[i]->SpecialInfo.stGigEInfo.nCurrentIp & 0xff000000) >> 24);
            nIp2 = ((m_stDevList.pDeviceInfo[i]->SpecialInfo.stGigEInfo.nCurrentIp & 0x00ff0000) >> 16);
            nIp3 = ((m_stDevList.pDeviceInfo[i]->SpecialInfo.stGigEInfo.nCurrentIp & 0x0000ff00) >> 8);
            nIp4 = (m_stDevList.pDeviceInfo[i]->SpecialInfo.stGigEInfo.nCurrentIp & 0x000000ff);

            if (0 != strlen((char*)pDeviceInfo->SpecialInfo.stGigEInfo.chUserDefinedName))
            {
                strDevInfo = QString(tr("[%1]GigE: %2 %3 (%4.%5.%6.%7)")).arg(i).arg((char*)pDeviceInfo->SpecialInfo.stGigEInfo.chManufacturerName)
                        .arg((char*)pDeviceInfo->SpecialInfo.stGigEInfo.chUserDefinedName).arg(nIp1).arg(nIp2).arg(nIp3).arg(nIp4);
            }
            else
            {
                strDevInfo = QString(tr("[%1]GigE: %2 %3 (%4.%5.%6.%7)")).arg(i).arg((char*)pDeviceInfo->SpecialInfo.stGigEInfo.chManufacturerName)
                        .arg((char*)pDeviceInfo->SpecialInfo.stGigEInfo.chSerialNumber).arg(nIp1).arg(nIp2).arg(nIp3).arg(nIp4);
            }
        }
        else
        {
        }
        ui->comBoxDevice->addItem(strDevInfo);
    }
    // ch: 没有查找到设备 | en: No devices found
    if (0 == m_stDevList.nDeviceNum)
    {
        strErrInfo = tr("No device found ") + QString::number(nRet, 16).mid(8, 8);
        QMessageBox::critical(this, tr("ERROR"), strErrInfo, QMessageBox::Ok);
        return;
    }
    m_nIndex = 0;
    DisplayDeviceIp();

}
// ch: 当前设备切换 | en: Current device switching
void CForceIpDemo::on_comBoxDevice_currentIndexChanged()
{
    int nRet = MV_CODEREADER_OK;
    QString strErrInfo = "";
    m_nIndex = ui->comBoxDevice->currentIndex();
    if (0 == ui->comBoxDevice->count())
    {
        return;
    }
    // ch: 创建设备句柄 | en: Creat device handle
    nRet = MV_CODEREADER_CreateHandle(&m_pHandle, m_stDevList.pDeviceInfo[m_nIndex]);
    if (MV_CODEREADER_OK != nRet)
    {
        strErrInfo = tr("Creat device handle failed ") + QString::number(nRet, 16).mid(8, 8);
        QMessageBox::critical(this, tr("ERROR"), strErrInfo, QMessageBox::Ok);
        return;
    }
    DisplayDeviceIp();
    CurrentIpConfig();

    ui->lblIPTips->setText(m_strIpRange);
}
// ch: 显示IP信息 | en: Display IP information
void CForceIpDemo::DisplayDeviceIp()
{
    // ch: IP 地址 | en: IP address
    int nParam1 = ((m_stDevList.pDeviceInfo[m_nIndex]->SpecialInfo.stGigEInfo.nCurrentIp & 0xff000000) >> 24);
    int nParam2 = ((m_stDevList.pDeviceInfo[m_nIndex]->SpecialInfo.stGigEInfo.nCurrentIp & 0x00ff0000) >> 16);
    int nParam3 = ((m_stDevList.pDeviceInfo[m_nIndex]->SpecialInfo.stGigEInfo.nCurrentIp & 0x0000ff00) >> 8);
    int nParam4 = (m_stDevList.pDeviceInfo[m_nIndex]->SpecialInfo.stGigEInfo.nCurrentIp & 0x000000ff);
    QString strIP = QString("%1.%2.%3.%4").arg(nParam1).arg(nParam2).arg(nParam3).arg(nParam4);
    ui->lineEditIPAddress->setText(strIP);
    m_strIpRange = QString("修改IP地址使设备可达.\n%1.%2.%3.1 - %1.%2.%3.254").arg(nParam1).arg(nParam2).arg(nParam3);
    // ch: 子网掩码 | en: Subnet mask
    nParam1 = (m_stDevList.pDeviceInfo[m_nIndex]->SpecialInfo.stGigEInfo.nCurrentSubNetMask & 0xff000000) >> 24;
    nParam2 = (m_stDevList.pDeviceInfo[m_nIndex]->SpecialInfo.stGigEInfo.nCurrentSubNetMask & 0x00ff0000) >> 16;
    nParam3 = (m_stDevList.pDeviceInfo[m_nIndex]->SpecialInfo.stGigEInfo.nCurrentSubNetMask & 0x0000ff00) >> 8;
    nParam4 = (m_stDevList.pDeviceInfo[m_nIndex]->SpecialInfo.stGigEInfo.nCurrentSubNetMask & 0x000000ff);
    QString strSubNetMask = QString("%1.%2.%3.%4").arg(nParam1).arg(nParam2).arg(nParam3).arg(nParam4);
    ui->lineEditMask->setText(strSubNetMask);
    // ch: 默认网关 | en: Default gateway
    nParam1 = (m_stDevList.pDeviceInfo[m_nIndex]->SpecialInfo.stGigEInfo.nDefultGateWay & 0xff000000) >> 24;
    nParam2 = (m_stDevList.pDeviceInfo[m_nIndex]->SpecialInfo.stGigEInfo.nDefultGateWay & 0x00ff0000) >> 16;
    nParam3 = (m_stDevList.pDeviceInfo[m_nIndex]->SpecialInfo.stGigEInfo.nDefultGateWay & 0x0000ff00) >> 8;
    nParam4 = (m_stDevList.pDeviceInfo[m_nIndex]->SpecialInfo.stGigEInfo.nDefultGateWay & 0x000000ff);
    QString strDefaultGateway = QString("%1.%2.%3.%4").arg(nParam1).arg(nParam2).arg(nParam3).arg(nParam4);
    ui->lineEditGateway->setText(strDefaultGateway);
}
// ch: 静态IP | en: Static IP
void CForceIpDemo::on_rBtnStatic_clicked()
{
    int nRet = MV_CODEREADER_OK;
    QString strErrInfo = "";
    nRet = MV_CODEREADER_GIGE_SetIpConfig(m_pHandle, MV_CODEREADER_IP_CFG_STATIC);
    if (MV_CODEREADER_OK != nRet)
    {
        CurrentIpConfig();
        strErrInfo = tr("Set IP config static failed ") + QString::number(nRet, 16).mid(8, 8);
        QMessageBox::critical(this, tr("ERROR"), strErrInfo, QMessageBox::Ok);
        return;
    }
    m_stDevList.pDeviceInfo[m_nIndex]->SpecialInfo.stGigEInfo.nIpCfgCurrent = IP_CONFIG_STATIC;
    DisplayDeviceIp();
    CurrentIpConfig();
}
// ch: 设置IP | en: Set IP
void CForceIpDemo::on_BtnSetIp_clicked()
{
    int nRet = MV_CODEREADER_OK;
    QString strErrInfo = "";
    unsigned int nIp = htonl(inet_addr(ui->lineEditIPAddress->text().toLatin1().data()));
    unsigned int nSubNetMask = htonl(inet_addr(ui->lineEditMask->text().toLatin1().data()));
    unsigned int nGateway = htonl(inet_addr(ui->lineEditGateway->text().toLatin1().data()));

    nRet = MV_CODEREADER_GIGE_ForceIp(m_pHandle, nIp, nSubNetMask, nGateway);
    if (MV_CODEREADER_OK != nRet)
    {
        strErrInfo = tr("Set ForceIp failed ") + QString::number(nRet, 16).mid(8, 8);
        QMessageBox::critical(this, tr("ERROR"), strErrInfo, QMessageBox::Ok);
        return;
    }
    else
    {
        strErrInfo = tr("Set ForceIp success ");
        QMessageBox::information(this, tr("Information"), strErrInfo, QMessageBox::Ok, NULL);
        return;
    }
}
// ch:自动分配IP(DHCP) | en: Auto assign IP(DHCP)
void CForceIpDemo::on_rBtnDHCP_clicked()
{
    int nRet = MV_CODEREADER_OK;
    QString strErrInfo = "";
    nRet = MV_CODEREADER_GIGE_SetIpConfig(m_pHandle, MV_CODEREADER_IP_CFG_DHCP);
    if (MV_CODEREADER_OK != nRet)
    {
        CurrentIpConfig();
        strErrInfo = tr("Set IP config DHCP failed ") + QString::number(nRet, 16).mid(8, 8);
        QMessageBox::critical(this, tr("ERROR"), strErrInfo, QMessageBox::Ok);
        return;
    }
    m_stDevList.pDeviceInfo[m_nIndex]->SpecialInfo.stGigEInfo.nIpCfgCurrent = IP_CONFIG_DHCP;
    DisplayDeviceIp();
    CurrentIpConfig();
}
// ch:自动分配IP(LLA) | en: Auto assign IP(LLA)
void CForceIpDemo::on_rBtnLLA_clicked()
{
    int nRet = MV_CODEREADER_OK;
    QString strErrInfo = "";
    nRet = MV_CODEREADER_GIGE_SetIpConfig(m_pHandle, MV_CODEREADER_IP_CFG_LLA);
    if (MV_CODEREADER_OK != nRet)
    {
        CurrentIpConfig();
        strErrInfo = tr("Set IP config LLA failed ") + QString::number(nRet, 16).mid(8, 8);
        QMessageBox::critical(this, tr("ERROR"), strErrInfo, QMessageBox::Ok);
        return;
    }
    m_stDevList.pDeviceInfo[m_nIndex]->SpecialInfo.stGigEInfo.nIpCfgCurrent = IP_CONFIG_LLA;
    DisplayDeviceIp();
    CurrentIpConfig();
}
// ch: 当前IP配置方式 | en: Current IP configuration mode
void CForceIpDemo::CurrentIpConfig()
{
    unsigned int nCurrentIpConfig = m_stDevList.pDeviceInfo[m_nIndex]->SpecialInfo.stGigEInfo.nIpCfgCurrent;
    switch (nCurrentIpConfig)
    {
    case IP_CONFIG_STATIC:
        ui->rBtnStatic->setEnabled(false);
        ui->rBtnStatic->setChecked(true);
        ui->rBtnDHCP->setEnabled(true);
        ui->rBtnLLA->setEnabled(true);
        ui->lineEditIPAddress->setEnabled(true);
        ui->lineEditMask->setEnabled(true);
        ui->lineEditGateway->setEnabled(true);
        ui->BtnSetIp->setEnabled(true);
        break;
    case IP_CONFIG_DHCP:
        ui->rBtnDHCP->setEnabled(false);
        ui->rBtnDHCP->setChecked(true);
        ui->rBtnStatic->setEnabled(true);
        ui->rBtnLLA->setEnabled(true);
        ui->lineEditIPAddress->setEnabled(false);
        ui->lineEditMask->setEnabled(false);
        ui->lineEditGateway->setEnabled(false);
        ui->BtnSetIp->setEnabled(false);
        break;
    case IP_CONFIG_LLA:
        ui->rBtnLLA->setEnabled(false);
        ui->rBtnLLA->setChecked(true);
        ui->rBtnStatic->setEnabled(true);
        ui->rBtnDHCP->setEnabled(true);
        ui->lineEditIPAddress->setEnabled(false);
        ui->lineEditMask->setEnabled(false);
        ui->lineEditGateway->setEnabled(false);
        ui->BtnSetIp->setEnabled(false);
        break;
    default:
        ui->rBtnStatic->setEnabled(true);
        ui->rBtnStatic->setChecked(false);
        ui->rBtnDHCP->setEnabled(true);
        ui->rBtnDHCP->setChecked(false);
        ui->rBtnLLA->setEnabled(true);
        ui->rBtnLLA->setChecked(false);
        ui->lineEditIPAddress->setEnabled(false);
        ui->lineEditMask->setEnabled(false);
        ui->lineEditGateway->setEnabled(false);
        ui->BtnSetIp->setEnabled(false);
        break;
    }
}

