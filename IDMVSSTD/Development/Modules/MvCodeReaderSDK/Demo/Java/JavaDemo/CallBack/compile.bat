@echo off
SET TargetDir=.\target

if not exist %TargetDir% (
		md %TargetDir%
		echo create target directory
)

javac com/CallBack/CallBackDemo.java -classpath libs\MvCodeReaderCtrlWrapper.jar -encoding utf-8 -d target
pause