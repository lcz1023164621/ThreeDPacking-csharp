@echo off

:: Install ethernet.inf and serial.inf
if %PROCESSOR_ARCHITECTURE% == x86 (
     dpinst_32.exe /LM
) else (
     dpinst.exe /LM
)
