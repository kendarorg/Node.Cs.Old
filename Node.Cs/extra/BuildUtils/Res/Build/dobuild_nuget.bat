SET PROJECT_NAME=%1

"%NUGET_DIR%\NuGet.exe" pack "%PROJECT_NAME%/%PROJECT_NAME%.nuspec" -Verbosity detailed -basepath "tmp_nuget/%PROJECT_NAME%" -OutputDirectory tmp_nuget