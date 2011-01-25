@echo off

lib\NAnt\NAnt.exe -buildfile:build\Colombo.build -logger:NAnt.LoggerExtension.ConsoleColorLogger package

if %ERRORLEVEL% NEQ 0 goto errors

goto finish

:errors
pause

:finish