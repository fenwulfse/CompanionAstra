@echo off
rem Double-click this after quitting Fallout 4 to collect the Papyrus log,
rem filter the Astra lines, and push them to GitHub for Claude to read.
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0collect_papyrus_log.ps1" -Push
echo.
pause
