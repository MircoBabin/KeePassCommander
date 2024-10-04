@echo off
    setlocal
    rem must have same output as Test.Command.getnoteraw.bat
    call "%~dp0Utils\Utils.test-setup.bat" "%~f0" "%~dp0Test.Command.getnoteraw.bat"
    
    "%KeePassCommandExe%" getnoteraw -stdout-utf8 "Sample Entry" > "%KeePassCommand_TestOutput%"
    
    call "%~dp0Utils\Utils.diff.bat"
    if errorlevel 1 exit /b 1
    exit /b 0