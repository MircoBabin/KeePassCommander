@echo off
    setlocal
    rem must have same output as Test.Command.getfieldraw.bat
    call "%~dp0Utils\Utils.test-setup.bat" "%~f0" "%~dp0Test.Command.getfieldraw.bat"

    "%KeePassCommandExe%" getfieldraw -stdout-utf8 "Sample Entry" "extra password 1" > "%KeePassCommand_TestOutput%"

    set diffErrorlevel=0
    call "%~dp0Utils\Utils.diff.bat"
    if errorlevel 1 set diffErrorlevel=1

    exit /b %diffErrorlevel%
