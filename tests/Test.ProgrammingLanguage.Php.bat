@echo off
    setlocal
    call "%~dp0Utils\Utils.test-setup.bat" "%~f0"

    "%~dp0..\assets\php\php.exe" "%~dp0Test.ProgrammingLanguage.Php.php" "%KeePassCommandExe%" > "%KeePassCommand_TestOutput%"

    set diffErrorlevel=0
    call "%~dp0Utils\Utils.diff.bat"
    if errorlevel 1 set diffErrorlevel=1

    exit /b %diffErrorlevel%
