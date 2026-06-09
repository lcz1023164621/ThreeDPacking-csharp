#!/bin/bash

TargetDir="./target"

# 检查目录是否存在，不存在则创建
if [ ! -d "$TargetDir" ]; then
    mkdir -p "$TargetDir"
    echo "create target directory"
fi

# 编译Java文件
javac com/MvID/JavaDemo.java -classpath libs/MvCodeReaderCtrlWrapper.jar -encoding utf-8 -d target

# 暂停等待用户按键
read -p "Press any key to continue..." -n1 -s
echo

