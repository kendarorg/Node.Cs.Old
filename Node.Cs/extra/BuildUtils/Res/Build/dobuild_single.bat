SET PROJECT_NAME=%1
SET FRAMEWORK_VERSION=%2
SET FRAMEWORK_NUGET_VERSION=%3
mkdir tmp_nuget\%PROJECT_NAME%
mkdir tmp_nuget\%PROJECT_NAME%\bin
mkdir tmp_nuget\%PROJECT_NAME%\bin\net%FRAMEWORK_NUGET_VERSION%

msbuild %PROJECT_NAME%/%PROJECT_NAME%.csproj /p:TargetFrameworkVersion=v%FRAMEWORK_VERSION%;Configuration=Release
copy /Y %PROJECT_NAME%\bin\Release\*.dll tmp_nuget\%PROJECT_NAME%\bin\net%FRAMEWORK_NUGET_VERSION%