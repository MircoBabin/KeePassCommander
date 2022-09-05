@echo off
setlocal EnableDelayedExpansion
set "totalStartTime=%time: =0%"

    set TOTAL=0
    set COUNTER=0
    if "%~1" == "" goto all

:commandline    
    call :getargc TOTAL %*
:commandlineNext
    call :runtest %1
    shift
    if "%~1" == "" goto end
    goto commandlineNext

:all
    SET /P AREYOUSURE=Run ALL tests ? (Y/[N])?
    IF "%AREYOUSURE%" == "Y" GOTO all_yes
    IF "%AREYOUSURE%" == "y" GOTO all_yes
    goto :eof
    
:all_yes    
    for %%a in ("%~dp0Test.*.bat") do set /A TOTAL=TOTAL+1
    for %%a in ("%~dp0Test.*.bat") do call :runtest "%%a"
    goto end
    
:end
set "totalEndTime=%time: =0%"

echo [Total time]
call :showelapsed "%totalStartTime%" "%totalEndTime%"

goto:eof

:runtest
    set /A COUNTER=COUNTER+1
    echo [%COUNTER%/%TOTAL%] %~1
    echo [start]
    
    set "testStartTime=%time: =0%"
    call "%~1"
    set "testEndTime=%time: =0%"
    
    echo [done] - there should be no output between [start] and [done]
    call :showelapsed "%testStartTime%" "%testEndTime%"
    echo.
    echo.
    
    goto :eof
    
:showelapsed
    set "startTime=%~1"
    set "endTime=%~2"
    
    rem Get elapsed time:
    set "end=!endTime:%time:~8,1%=%%100)*100+1!"  &  set "start=!startTime:%time:~8,1%=%%100)*100+1!"
    set /A "elap=((((10!end:%time:~2,1%=%%100)*60+1!%%100)-((((10!start:%time:~2,1%=%%100)*60+1!%%100), elap-=(elap>>31)*24*60*60*100"

    rem Convert elapsed time to HH:MM:SS:CC format:
    set /A "cc=elap%%100+100,elap/=100,ss=elap%%60+100,elap/=60,mm=elap%%60+100,hh=elap/60+100"

    echo Start:    %startTime%
    echo End:      %endTime%
    echo Elapsed:  %hh:~1%%time:~2,1%%mm:~1%%time:~2,1%%ss:~1%%time:~8,1%%cc:~1%
    goto :eof
    
:getargc
    set getargc_v0=%1
    set /a "%getargc_v0% = 0"
:getargc_l0
    if not x%2x==xx (
        shift
        set /a "%getargc_v0% = %getargc_v0% + 1"
        goto :getargc_l0
    )
    set getargc_v0=
    goto :eof    
    