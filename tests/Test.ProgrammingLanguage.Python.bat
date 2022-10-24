@echo off
    setlocal
    call "%~dp0Utils\Utils.test-setup.bat" "%~f0"

    chcp 65001 > nul 2>&1
    set PYTHONIOENCODING=utf-8
    set PYTHONPYCACHEPREFIX=%tmp
    set PYTHONDONTWRITEBYTECODE=1
    python "%~dp0Test.ProgrammingLanguage.Python.py" "%KeePassCommandExe%" > "%KeePassCommand_TestOutput%"
    
    call "%~dp0Utils\Utils.diff.bat"
    if errorlevel 1 exit /b 1
    exit /b 0