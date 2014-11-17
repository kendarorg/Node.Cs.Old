SET CURRENT_DIR=%CD%
SET NUGET_DIR=%CURRENT_DIR%\.nuget
VisualStudioIdentifier 11.0 VS2012 vs.bat
call vs.bat

cd %VS2012%
cd..
SET VS2012=%CD%
CD %CURRENT_DIR%
call "%VS2012%\Tools\VsDevCmd.bat"

mkdir tmp_nuget