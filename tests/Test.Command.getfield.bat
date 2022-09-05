@echo off
    setlocal
    call "%~dp0Utils\Utils.test-setup.bat" "%~f0"
    
    "%KeePassCommandExe%" getfield "Sample Entry" "extra field 1" "extra password 1"> "%KeePassCommand_TestOutput%"
    
    call "%~dp0Utils\Utils.diff.bat"
    if errorlevel 1 exit /b 1
    exit /b 0    