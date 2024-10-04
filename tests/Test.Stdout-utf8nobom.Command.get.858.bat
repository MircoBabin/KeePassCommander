@echo off
    rem This file is saved in Western Europe OEM-858 codepage.
    setlocal
    
    rem [BEGIN] force codepage to 858
    call "%~dp0Utils\Utils.chcp.bat" set 858
    rem [END]
    
    rem must have same output as Test.Stdout-utf8nobom.Command.get.65001.bat
    call "%~dp0Utils\Utils.test-setup.bat" "%~f0" "%~dp0Test.Stdout-utf8nobom.Command.get.65001.bat"
    
    "%KeePassCommandExe%" get -stdout-utf8nobom "Unicode Entry@Notes ‚n Õ–r•" > "%KeePassCommand_TestOutput%"
    
    call "%~dp0Utils\Utils.diff.bat"
    
    rem [BEGIN] restore codepage before exit
    call "%~dp0Utils\Utils.chcp.bat" restore
    rem [END]

    if errorlevel 1 exit /b 1
    exit /b 0    