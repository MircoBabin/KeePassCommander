@echo off
    cd /d "%~dp0"
    chcp 65001 > nul 2>&1
    "%~dp0..\assets\lua\lua54.exe" FromLua.lua
