@echo off
    if not exist "%KeePassCommand_TestOutput%" echo Error: Output file does not exist %KeePassCommand_TestOutput%
    if not exist "%KeePassCommand_TestExpected%" echo Error: Expected file does not exist %KeePassCommand_TestExpected%
    
    call :errorlevel_99
    fc /b "%KeePassCommand_TestOutput%" "%KeePassCommand_TestExpected%" >nul 2>&1
    if errorlevel 1 goto error
    
    exit /b 0

:Error    
    echo Error: output %KeePassCommand_TestOutput% does not match expectation %KeePassCommand_TestExpected%
    exit /b 1
    
:errorlevel_99
    exit /b 99    
