@echo off
    if "%~1" == "set" goto set
    if "%~1" == "restore" goto restore
    
    echo Unknown command: "%~1"
    echo.
    echo Utils.chcp.bat set [codepage]
    echo Utils.chcp.bat restore
    goto :eof

:set
    for /f "tokens=2 delims=:" %%G in ('chcp') do set KeePassCommand_TestSaveChcp=%%G
    call :trim KeePassCommand_TestSaveChcp %KeePassCommand_TestSaveChcp%
    
    call :do_chcp %~2
    goto :eof

:restore
    call :do_chcp %KeePassCommand_TestSaveChcp%
    goto :eof

:do_chcp
    set KeePassCommand_TestSetChcp1=%~1
    call :trim KeePassCommand_TestSetChcp1 %KeePassCommand_TestSetChcp1%
    
    chcp %KeePassCommand_TestSetChcp1% >nul 2>&1

    for /f "tokens=2 delims=:" %%G in ('chcp') do set KeePassCommand_TestSetChcp2=%%G
    call :trim KeePassCommand_TestSetChcp2 %KeePassCommand_TestSetChcp2%
    
    if "%KeePassCommand_TestSetChcp2%" == "%KeePassCommand_TestSetChcp1%" goto do_chcp_end
    echo -------------------------------------------------------------------------------
    echo Error setting chcp "%KeePassCommand_TestSetChcp1%"
    echo Current codepage is "%KeePassCommand_TestSetChcp2%"
    chcp
    echo -------------------------------------------------------------------------------

:do_chcp_end    
    set KeePassCommand_TestSetChcp1=
    set KeePassCommand_TestSetChcp2=
    goto :eof
    
:trim
    set %1=%2
    goto :eof
   
