@echo off
    setlocal
    call "%~dp0Utils\Utils.test-setup.bat" "%~f0"
    
    chcp 65001 > nul 2>&1
    powershell -NoProfile -ExecutionPolicy Bypass -file "%~dp0Test.ProgrammingLanguage.Powershell.ps1" "%KeePassCommandExe%"> "%KeePassCommand_TestOutput%"
    
    call "%~dp0Utils\Utils.diff.bat"
    if errorlevel 1 exit /b 1
    exit /b 0