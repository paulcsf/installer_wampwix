REM Should be called in Visual Studio pre-build as: call $(ProjectDir)\Websocket_PreBuild.bat "$(ProjectDir)" "$(SolutionDir)"
echo Project Directory:
echo %1
echo Solution Directory:
echo %2

REM Delete all of the files in the cache directory
DEL /F /Q /S "%2Websocket\node_cache\*"
REM Delete all of the subdirectories in the cache directory
for /D %%i in ("%2Websocket\node_cache\*") do RD /S /Q "%%i"
