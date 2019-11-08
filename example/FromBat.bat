@echo off
    setlocal
    
rem find KeePassEntry.bat
    set KeePassEntry_bats=
    
    set KeePassEntry_bat=%~dp0KeePassEntry.bat
    if exist "%KeePassEntry_bat%" goto example
    set KeePassEntry_bats=%KeePassEntry_bats%%KeePassEntry_bat%;
    
    set KeePassEntry_bat=%~dp0..\bin\release\KeePassEntry.bat
    if exist "%KeePassEntry_bat%" goto example
    set KeePassEntry_bats=%KeePassEntry_bats%%KeePassEntry_bat%;
    
    echo KeePassEntry.bat not found, tried the following paths:
    for %%f in ("%KeePassEntry_bats:;=";"%") do @if not "%%~f"=="" echo.%%~f
    exit /b 1

:example    
rem BEGIN example
    call "%KeePassEntry_bat%" "Sample Entry"
    if errorlevel 1 goto :eof
    if "%keepasscommand_entryname%" == "" goto KeePassNotStarted
    
    echo entryname:  %keepasscommand_entryname%
    echo username:   %keepasscommand_username%
    echo password:   %keepasscommand_password%
    echo url:        %keepasscommand_url%
    echo url-scheme: %keepasscommand_urlscheme%
    echo url-host:   %keepasscommand_urlhost%
    echo url-port:   %keepasscommand_urlport%
    echo url-path:   %keepasscommand_urlpath%
    echo notes:      %keepasscommand_notes%
rem END example
    exit /b 0

:KeePassNotStarted
    echo KeePass is not started
    echo Has KeePassCommander.dll been copied to the directory containing KeePass.exe ?

    exit /b 2
    
