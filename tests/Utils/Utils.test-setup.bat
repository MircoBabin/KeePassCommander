@echo off
    set "KeePassCommandExe=%~dp0..\..\bin\Debug\KeePassCommand.exe"
    set "KeePassCommand_TestBaseName=%~nx1"
    
    set "KeePassCommand_TestOutput=%tmp%\KeePassCommander.%KeePassCommand_TestBaseName%.output"
    set "KeePassCommand_TestExpected=%~dp0..\Expectations\%KeePassCommand_TestBaseName%.expected"
