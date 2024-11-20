@echo off
    setlocal

    rem [BEGIN] force codepage to 65001 (utf-8)
    call "%~dp0Utils\Utils.chcp.bat" set 65001
    rem [END]

    call "%~dp0Utils\Utils.test-setup.bat" "%~f0"

    chcp 65001 > nul 2>&1
    "%~dp0..\assets\lua\lua54.exe" "%~dp0Test.ProgrammingLanguage.Lua.lua" "%KeePassCommandExe%" > "%KeePassCommand_TestOutput%"

    set diffErrorlevel=0
    call "%~dp0Utils\Utils.diff.bat"
    if errorlevel 1 set diffErrorlevel=1

    rem [BEGIN] restore codepage before exit
    call "%~dp0Utils\Utils.chcp.bat" restore
    rem [END]

    exit /b %diffErrorlevel%
