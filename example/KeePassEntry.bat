@echo off
rem This file is put in the same directory as KeePassCommand.exe.
rem When KeePassCommand.exe is located somewhere else, adjust line 6 call :keepasscommand "%~dp0KeePassCommand.exe" "Sample Entry"

rem BEGIN example
    call :keepasscommand "%~dp0KeePassCommand.exe" "Sample Entry"
    
    echo entryname:  %keepasscommand_entryname%
    echo username:   %keepasscommand_username%
    echo password:   %keepasscommand_password%
    echo url:        %keepasscommand_url%
    echo url-scheme: %keepasscommand_urlscheme%
    echo url-host:   %keepasscommand_urlhost%
    echo url-port:   %keepasscommand_urlport%
    echo url-path:   %keepasscommand_urlpath%
    echo notes:      %keepasscommand_notes%
    goto :eof
rem END example
    
:keepasscommand
    rem "%1" should be full path to KeePassCommand.exe e.g. "%~dp0KeePassCommand.exe"
    rem "%2" should be entryname to be fetched
    
    set keepasscommand_state_exe="%~1" get "%~2"
    set keepasscommand_state=0
    
    set keepasscommand_entryname=
    set keepasscommand_username=
    set keepasscommand_password=
    set keepasscommand_url=
    set keepasscommand_urlscheme=
    set keepasscommand_urlhost=
    set keepasscommand_urlport=
    set keepasscommand_urlpath=
    set keepasscommand_notes=
    
    for /f "tokens=*" %%i in ('"%keepasscommand_state_exe%"') do call :keepasscommand_parse "%%i"
    
    set keepasscommand_state_exe=
    set keepasscommand_state=
    set keepasscommand_state_line=
    set keepasscommand_state_1=
    set keepasscommand_state_2=
    goto :eof
    
:keepasscommand_parse
    set keepasscommand_state_line=%~1
    set keepasscommand_state_1=%keepasscommand_state_line:~0,1%
    set keepasscommand_state_2=%keepasscommand_state_line:~2%
    
    if "%keepasscommand_state%" == "0" goto keepasscommand_state0
    goto keepasscommand_state1
    
:keepasscommand_state0
    if "%keepasscommand_state_1%" == "B" set keepasscommand_state=1
    goto :eof    
    
:keepasscommand_state1
    if "%keepasscommand_state_1%" == "I" (
        if "%keepasscommand_state%" == "1" set keepasscommand_entryname=%keepasscommand_state_2%
        if "%keepasscommand_state%" == "2" set keepasscommand_username=%keepasscommand_state_2%
        if "%keepasscommand_state%" == "3" set keepasscommand_password=%keepasscommand_state_2%
        if "%keepasscommand_state%" == "4" set keepasscommand_url=%keepasscommand_state_2%
        if "%keepasscommand_state%" == "5" set keepasscommand_urlscheme=%keepasscommand_state_2%
        if "%keepasscommand_state%" == "6" set keepasscommand_urlhost=%keepasscommand_state_2%
        if "%keepasscommand_state%" == "7" set keepasscommand_urlport=%keepasscommand_state_2%
        if "%keepasscommand_state%" == "8" set keepasscommand_urlpath=%keepasscommand_state_2%
        if "%keepasscommand_state%" == "9" set keepasscommand_notes=%keepasscommand_state_2%
        
        set /a keepasscommand_state=keepasscommand_state+1
    )
    
    if "%keepasscommand_state_1%" == "E" (
        set keepasscommand_state=0
    )
    
    goto :eof
