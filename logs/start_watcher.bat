@echo off
title Astra Watcher
rem Double-click once and leave the window open. It pulls Claude's changes,
rem rebuilds/deploys the mod while the game is closed, and uploads the
rem Papyrus log automatically whenever you quit Fallout 4.
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0astra_watcher.ps1"
pause
