set WORKSPACE=..
set LUBAN_DLL=%WORKSPACE%\Config\Luban\Luban.dll
set CONF_ROOT=.

dotnet %LUBAN_DLL% ^
    -t all ^
    -d json ^
    -c cs-simple-json ^
    --conf %CONF_ROOT%\luban.conf ^
    -xoutputCodeDir=..\Assets\Scripts\Luban ^
    -x outputDataDir=..\Assets\Resources\Config

pause