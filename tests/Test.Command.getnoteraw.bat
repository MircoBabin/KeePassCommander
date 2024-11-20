@echo off
    setlocal
    call "%~dp0Utils\Utils.test-setup.bat" "%~f0"

    "%KeePassCommandExe%" getnoteraw "Sample Entry" "-out-utf8:%KeePassCommand_TestOutput%"

    set diffErrorlevel=0
    call "%~dp0Utils\Utils.diff.bat"
    if errorlevel 1 set diffErrorlevel=1

    exit /b %diffErrorlevel%
