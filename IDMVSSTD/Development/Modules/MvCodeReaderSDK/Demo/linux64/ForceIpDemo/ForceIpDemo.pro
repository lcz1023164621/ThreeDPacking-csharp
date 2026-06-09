#-------------------------------------------------
#
# Project created by QtCreator 2022-07-29T10:08:38
#
#-------------------------------------------------

QT       += core gui

greaterThan(QT_MAJOR_VERSION, 4): QT += widgets

TARGET = ForceIpDemo
TEMPLATE = app

#DESTDIR     = bin
#OBJECTS_DIR = tmp/obj
#UI_DIR      = tmp/ui
#RCC_DIR     = tmp/rcc
#MOC_DIR     = tmp/moc

SOURCES += src/main.cpp            \
           src/ForceIpDemo.cpp

HEADERS  += inc/ForceIpDemo.h \
    ../Include/MvCodeReaderCtrl.h \
    ../Include/MvCodeReaderErrorDefine.h \
    ../Include/MvCodeReaderParams.h \
    ../Include/MvCodeReaderPixelType.h \
    ../Include/MvCodeReaderCtrl.h


FORMS       += res/ForceIpDemo.ui

INCLUDEPATH += inc/ \
   ../Include/

INCLUDEPATH += $$PWD/inc/
DEPENDPATH += $$PWD/lib

LIBS += -L$$PWD/bin/ -lFormatConversion \
    -L$$PWD/bin/ -lMediaProcess \
    -L$$PWD/bin/ -lMvCameraControl \
    -L$$PWD/bin/ -lMvCodeReaderCtrl \
    -L$$PWD/bin/ -lMVGigEVisionSDK \
    -L$$PWD/bin/ -lMVRender \
    -L$$PWD/bin/ -lMvUsb3vTL \
	-L$$PWD/bin/ -lhlog \
    -L$$PWD/bin/ -lhpr
