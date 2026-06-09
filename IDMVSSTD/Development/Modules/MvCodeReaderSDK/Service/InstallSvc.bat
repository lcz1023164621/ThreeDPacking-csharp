@echo off
set base_dir=%~dp0
%base_dir:~0,2%
pushd %base_dir%
echo %cd%

cd.>Path.ini
echo %systemroot%\Temp>Path.ini

echo install LogServer...
MvSmtLogServer.exe install

@Xcopy /Y LogServer.ini %systemroot%\Temp\MvSmtLog\

pushd %systemroot%\system32
echo %cd%

echo start LogServer...
sc start MvSmtLogServer

@ping 127.0.0.1 -n 3 >nul