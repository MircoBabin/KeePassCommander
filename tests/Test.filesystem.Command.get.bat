@echo off
    setlocal
    call "%~dp0Utils\Utils.test-setup.bat" "%~f0"
    
    rem Expect the same output as Test.Command.get.bat
    set "KeePassCommand_TestExpected=%~dp0Expectations\Test.Command.get.bat.expected"
    
    "%KeePassCommandExe%" "-filesystem:c:\incoming\KeePass" get "Sample Entry" > "%KeePassCommand_TestOutput%"
    
    call "%~dp0Utils\Utils.diff.bat"
    if errorlevel 1 exit /b 1
    exit /b 0    