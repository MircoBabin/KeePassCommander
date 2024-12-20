@echo off
    setlocal
    call "%~dp0Utils\Utils.test-setup.bat" "%~f0"

    rem Expect the same output as Test.Command.get.bat
    set "KeePassCommand_TestExpected=%~dp0Expectations\Test.Command.get.bat.expected"

    "%KeePassCommandExe%" "-filesystem:c:\incoming\KeePass" get "Sample Entry" > "%KeePassCommand_TestOutput%"

    set diffErrorlevel=0
    call "%~dp0Utils\Utils.diff.bat"
    if errorlevel 1 set diffErrorlevel=1

    exit /b %diffErrorlevel%
