#!/bin/bash

# 设置加载库路径
# export LD_LIBRARY_PATH="../../../../:$LD_LIBRARY_PATH"

cd target

# 运行Java程序

java -classpath ".:../libs/MvCodeReaderCtrlWrapper.jar" com.MvID.JavaDemo


# 暂停等待用户按键

read -p "Press any key to continue..." -n1 -s

echo
