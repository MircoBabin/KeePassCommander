@echo off
    if not exist "%KeePassCommand_TestOutput%" echo Error: Output file does not exist %KeePassCommand_TestOutput%
    if not exist "%KeePassCommand_TestExpected%" echo Error: Expected file does not exist %KeePassCommand_TestExpected%
    
    if "%~1" == "sort" goto sortedCompare

    call :errorlevel_99
    fc /b "%KeePassCommand_TestOutput%" "%KeePassCommand_TestExpected%" >nul 2>&1
    if errorlevel 1 goto error
    
    exit /b 0

:sortedCompare
    set "KeePassCommand_TestOutputSorted=%tmp%\KeePassCommander.diff.sorted.1"
    set "KeePassCommand_TestExpectedSorted=%tmp%\KeePassCommander.diff.sorted.2"

    sort "%KeePassCommand_TestOutput%" /O "%KeePassCommand_TestOutputSorted%" >nul 2>&1
    if not exist "%KeePassCommand_TestOutputSorted%" echo Error: Sorted output file does not exist %KeePassCommand_TestOutputSorted%

    sort "%KeePassCommand_TestExpected%" /O "%KeePassCommand_TestExpectedSorted%" >nul 2>&1
    if not exist "%KeePassCommand_TestExpectedSorted%" echo Error: Sorted output file does not exist %KeePassCommand_TestExpectedSorted%

    call :errorlevel_99
    fc /b "%KeePassCommand_TestOutputSorted%" "%KeePassCommand_TestExpectedSorted%" >nul 2>&1
    if errorlevel 1 goto error

    exit /b 0

:Error    
    echo Error: output %KeePassCommand_TestOutput% does not match expectation %KeePassCommand_TestExpected%
    exit /b 1
    
:errorlevel_99
    exit /b 99    
