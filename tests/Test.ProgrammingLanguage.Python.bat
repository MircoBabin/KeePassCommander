@echo off
    setlocal

    rem [BEGIN] force codepage to 65001 (utf-8)
    call "%~dp0Utils\Utils.chcp.bat" set 65001
    rem [END]

    call "%~dp0Utils\Utils.test-setup.bat" "%~f0"

    set PYTHONIOENCODING=utf-8
    set PYTHONPYCACHEPREFIX=%tmp%
    set PYTHONDONTWRITEBYTECODE=1
    python "%~dp0Test.ProgrammingLanguage.Python.py" "%KeePassCommandExe%" > "%KeePassCommand_TestOutput%"

    set diffErrorlevel=0
    call "%~dp0Utils\Utils.diff.bat"
    if errorlevel 1 set diffErrorlevel=1

    rem [BEGIN] restore codepage before exit
    call "%~dp0Utils\Utils.chcp.bat" restore
    rem [END]

    exit /b %diffErrorlevel%
