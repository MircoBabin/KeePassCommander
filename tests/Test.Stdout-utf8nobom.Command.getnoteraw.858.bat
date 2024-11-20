@echo off
    rem This file is saved in Western Europe OEM-858 codepage.
    setlocal

    rem [BEGIN] force codepage to 858
    call "%~dp0Utils\Utils.chcp.bat" set 858
    rem [END]

    rem must have same output as Test.Stdout-utf8nobom.Command.getnoteraw.65001.bat
    call "%~dp0Utils\Utils.test-setup.bat" "%~f0" "%~dp0Test.Stdout-utf8nobom.Command.getnoteraw.65001.bat"

    "%KeePassCommandExe%" getnoteraw -stdout-utf8nobom "Unicode Entry@Notes ‚n Õ–r•" > "%KeePassCommand_TestOutput%"

    set diffErrorlevel=0
    call "%~dp0Utils\Utils.diff.bat"
    if errorlevel 1 set diffErrorlevel=1

    rem [BEGIN] restore codepage before exit
    call "%~dp0Utils\Utils.chcp.bat" restore
    rem [END]

    exit /b %diffErrorlevel%
