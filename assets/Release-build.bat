@echo off
setlocal
cls

cd /D "%~dp0"

set sz_exe=C:\Program Files\7-Zip\7z.exe
if exist "%sz_exe%" goto build

set sz_exe=C:\Program Files (x86)\7-Zip\7z.exe
if exist "%sz_exe%" goto build

echo !!! 7-Zip 18.06 - 7z.exe not found
pause
goto:eof

:build 
echo 7-Zip 18.06: %sz_exe%

FOR /F "tokens=* USEBACKQ" %%g IN (`"%~dp0..\bin\Release\KeePassCommand.exe" --version`) do (SET KeepassCommandReleaseVersion=%%g)
FOR /F "tokens=* USEBACKQ" %%g IN (`"%~dp0..\bin\Debug\KeePassCommand.exe" --version`) do (SET KeepassCommandDebugVersion=%%g)

if "%KeepassCommandReleaseVersion%" == "%KeepassCommandDebugVersion%" goto build_zip
echo.
echo Release version: %KeepassCommandReleaseVersion%
echo Debug version..: %KeepassCommandDebugVersion%
echo.
echo !!! Versions do not match.
pause
goto :eof


:build_zip
echo.
echo Release version: %KeepassCommandReleaseVersion%
echo.
echo.


del /q "Release\*" >nul 2>&1

set files="%~dp0..\bin\Release\KeePassCommand.exe"
set files=%files% "%~dp0..\bin\Release\KeePassCommand.exe.config"
set files=%files% "%~dp0..\bin\Release\KeePassCommandDll.dll"
set files=%files% "%~dp0..\bin\Release\KeePassCommander.dll"
set files=%files% "%~dp0..\bin\Release\KeePassEntry.bat"
set files=%files% "%~dp0..\bin\Release\KeePassEntry.php"
set files=%files% "%~dp0..\bin\Release\KeePassEntry.ps1"
set files=%files% "%~dp0..\bin\Release\KeePassEntry.py"

"%sz_exe%" a -tzip -mx7 "Release\KeePassCommander-%KeepassCommandReleaseVersion%.zip" %files%
"%sz_exe%" a -tzip -mx7 "Release\KeePassCommander-%KeepassCommandReleaseVersion%-debugpack.zip" "%~dp0..\bin"

echo.
echo.
echo Created "Release\KeePassCommander-%KeepassCommandReleaseVersion%.zip"
echo Created "Release\KeePassCommander-%KeepassCommandReleaseVersion%-debugpack.zip" 

rem https://github.com/MircoBabin/KeePassCommander/releases/latest/download/release.download.zip.url-location
rem Don't output trailing newline (CRLF)
<NUL >"Release\release.download.zip.url-location" set /p="https://github.com/MircoBabin/KeePassCommander/releases/download/%KeepassCommandReleaseVersion%/KeePassCommander-%KeepassCommandReleaseVersion%.zip"

echo.
echo Created "Release\release.download.zip.url-location" 
echo.

rem https://github.com/MircoBabin/KeePassCommander/releases/latest/download/keepass.plugin.version.txt
echo :>"Release\keepass.plugin.version.txt"
echo KeePassCommander:%KeepassCommandReleaseVersion%>>"Release\keepass.plugin.version.txt"
echo :>>"Release\keepass.plugin.version.txt"

echo.
echo Created "Release\keepass.plugin.version.txt" 
echo.

pause
