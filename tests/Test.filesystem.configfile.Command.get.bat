@echo off
    setlocal
    call "%~dp0Utils\Utils.test-setup.bat" "%~f0"
    
    set "Org_KeePassCommand_TestExpected=%KeePassCommand_TestExpected%"

:test1
    del /q "%KeePassCommandConfigXml%" >nul 2>&1
    copy "%~dp0Test.filesystem.KeePassCommand.config.xml" "%KeePassCommandConfigXml%" >nul 2>&1
    
    "%KeePassCommandExe%" get "Sample Entry" > "%KeePassCommand_TestOutput%"

    del /q "%KeePassCommandConfigXml%" >nul 2>&1

    rem Expect the same output as Test.Command.get.bat
    set "KeePassCommand_TestExpected=%~dp0Expectations\Test.Command.get.bat.expected"

    call "%~dp0Utils\Utils.diff.bat"
    if errorlevel 1 exit /b 1

:test2    
    rem get Another Entry via Named Pipe
    "%KeePassCommandExe%" get -namedpipe "Another Entry" > "%KeePassCommand_TestOutput%"
    
    set "KeePassCommand_TestExpected=%Org_KeePassCommand_TestExpected%.AnotherEntry"

    call "%~dp0Utils\Utils.diff.bat"
    if errorlevel 1 exit /b 1

:test3    
    rem get Another Entry via FileSystem, must fail because it is not allowed via the entry "KeePassCommander.FileSystem - example for git in a VM"

    del /q "%KeePassCommandConfigXml%" >nul 2>&1
    copy "%~dp0Test.filesystem.KeePassCommand.config.xml" "%KeePassCommandConfigXml%" >nul 2>&1
    
    "%KeePassCommandExe%" get "Another Entry" > "%KeePassCommand_TestOutput%"
    
    del /q "%KeePassCommandConfigXml%" >nul 2>&1
    
    set "KeePassCommand_TestExpected=%Org_KeePassCommand_TestExpected%.AnotherEntry"
    call "%~dp0Utils\Utils.diff.bat" "" "not-equal"
    if errorlevel 1 exit /b 1

    set "KeePassCommand_TestExpected=%Org_KeePassCommand_TestExpected%.AnotherEntry-filesystem"
    call "%~dp0Utils\Utils.diff.bat"
    if errorlevel 1 exit /b 1

:test4
    rem run CsharpExample.exe with KeePassCommand.config.xml in directory of KeePassCommandDll.dll

    del /q "%KeePassCommandConfigXml%" >nul 2>&1
    copy "%~dp0Test.filesystem.KeePassCommand.config.xml" "%KeePassCommandConfigXml%" >nul 2>&1

    "%~dp0\..\Example\CsharpExample.exe" "%KeePassCommandDllDll%" > "%KeePassCommand_TestOutput%"

    del /q "%KeePassCommandConfigXml%" >nul 2>&1

    set "KeePassCommand_TestExpected=%Org_KeePassCommand_TestExpected%.CsharpExample.auto"
    call "%~dp0Utils\Utils.diff.bat"
    if errorlevel 1 exit /b 1

:test5
    rem run CsharpExample.exe with filesystem

    "%~dp0\..\Example\CsharpExample.exe" "%KeePassCommandDllDll%" "%KeePassCommand_FileSystemDirectory%" > "%KeePassCommand_TestOutput%"

    set "KeePassCommand_TestExpected=%Org_KeePassCommand_TestExpected%.CsharpExample.filesystem"
    call "%~dp0Utils\Utils.diff.bat"
    if errorlevel 1 exit /b 1

:test6
    rem run CsharpExample.exe with namedpipe

    "%~dp0\..\Example\CsharpExample.exe" "%KeePassCommandDllDll%" "namedpipe" > "%KeePassCommand_TestOutput%"

    set "KeePassCommand_TestExpected=%Org_KeePassCommand_TestExpected%.CsharpExample.namedpipe"
    call "%~dp0Utils\Utils.diff.bat"
    if errorlevel 1 exit /b 1


:end    
    exit /b 0