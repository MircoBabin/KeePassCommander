@echo off
    set "KeePassCommandExe=%~dp0..\..\bin\Debug\KeePassCommand.exe"
    set "KeePassCommandDllDll=%~dp0..\..\bin\Debug\KeePassCommandDll.dll"
    set "KeePassCommandConfigXml=%~dp0..\..\bin\Debug\KeePassCommand.config.xml"
    set "KeePassCommand_TestBaseName=%~nx1"
    
    set "KeePassCommand_TestOutput=%tmp%\KeePassCommander.%KeePassCommand_TestBaseName%.output"
    set "KeePassCommand_TestExpected=%~dp0..\Expectations\%KeePassCommand_TestBaseName%.expected"
    
    set "KeePassCommand_FileSystemDirectory=c:\incoming\KeePass"
    md c:\incoming >nul 2>&1
    md c:\incoming\KeePass >nul 2>&1
    
