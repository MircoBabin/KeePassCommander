@echo off
rem call "$(SolutionDir)CsharpExample_postbuild.bat" "$(TargetDir)" "$(ConfigurationName)"

set configuration=%~2
if "%configuration%" == "release" goto :release
if "%configuration%" == "Release" goto :release
if "%configuration%" == "RELEASE" goto :release

echo [SKIP] CsharpExample_postbuild.bat - configuration is not "release": %configuration%
goto :eof

:release
echo [START] CsharpExample_postbuild.bat

rem Make sure TargetDir is \ terminated
set targetdir=%~1
if not "%targetdir:~-1%" == "\" set targetdir=%targetdir%\
echo TargetDir: %targetdir%

rem Make sure ExampleDir is \ terminated
set exampledir=%~dp0
if not "%exampledir:~-1%" == "\" set exampledir=%exampledir%\
set exampledir=%exampledir%..\..\
echo ExampleDir: %exampledir%

rem Copy CsharpExample.exe to example directory
echo [STEP] Copy to example directory
del /q "%exampledir%CsharpExample.exe*" >nul 2>&1
copy /y "%targetdir%CsharpExample.exe*" "%exampledir%"

echo [END] CsharpExample_postbuild.bat
