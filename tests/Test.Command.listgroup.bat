@echo off
    setlocal
    call "%~dp0Utils\Utils.test-setup.bat" "%~f0"
    
    "%KeePassCommandExe%" listgroup "All Entries" > "%KeePassCommand_TestOutput%"
    
    call "%~dp0Utils\Utils.diff.bat" "sort"
    if errorlevel 1 exit /b 1
    exit /b 0    