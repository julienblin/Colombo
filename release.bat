@echo off

lib\NAnt\NAnt.exe release

if %ERRORLEVEL% NEQ 0 goto errors

goto finish

:errors
pause

:finish