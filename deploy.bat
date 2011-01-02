@echo off

lib\NAnt\NAnt.exe deploy

if %ERRORLEVEL% NEQ 0 goto errors

goto finish

:errors
pause

:finish