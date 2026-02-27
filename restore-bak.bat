@echo off
REM Restaura AgroSolutions.bak no container fas-sqlserver.
REM Uso: restore-bak.bat

cd /d "%~dp0"
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0restore-bak.ps1"
exit /b %ERRORLEVEL%
