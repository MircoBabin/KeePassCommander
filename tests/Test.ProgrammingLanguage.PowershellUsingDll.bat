@echo off
    setlocal
    call "%~dp0Utils\Utils.test-setup.bat" "%~f0"
    
    rem Expect the same output as Test.ProgrammingLanguage.Powershell.bat
    set "KeePassCommand_TestExpected=%~dp0Expectations\Test.ProgrammingLanguage.Powershell.bat.expected"
    
    powershell -NoProfile -ExecutionPolicy Bypass -file "%~dp0Test.ProgrammingLanguage.PowershellUsingDll.ps1" "%KeePassCommandExe%" > "%KeePassCommand_TestOutput%"
    
    call "%~dp0Utils\Utils.diff.bat"
    if errorlevel 1 exit /b 1
    exit /b 0