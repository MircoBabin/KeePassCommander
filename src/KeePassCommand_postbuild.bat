@echo off
rem call "$(SolutionDir)KeePassCommand_postbuild.bat" "$(TargetDir)"

echo [START] KeePassCommand_postbuild.bat

rem Make sure TargetDir is \ terminated
set targetdir=%~1
if not "%targetdir:~-1%" == "\" set targetdir=%targetdir%\
echo TargetDir: %targetdir%

rem Copy KeePassEntry.*
echo [STEP] Copy Programming Languages Connectors
del /q "%targetdir%KeePassEntry.*"
for %%f in ("%~dp0ProgrammingLanguagesConnectors\*") do (
    echo %%f
    copy /y "%%f" "%targetdir%"
)

echo [END] KeePassCommand_postbuild.bat
