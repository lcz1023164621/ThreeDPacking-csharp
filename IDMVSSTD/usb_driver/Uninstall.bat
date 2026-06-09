@echo off

:: Uninstall ethernet.inf and serial.inf
if %PROCESSOR_ARCHITECTURE% == x86 (
    dpinst_32.exe /U ethernet.inf
    dpinst_32.exe /U serial.inf
) else (
    dpinst.exe /U ethernet.inf
    dpinst.exe /U serial.inf
)

