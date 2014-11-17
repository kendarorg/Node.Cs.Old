@echo off
SET SOLUTION_DIR=%1
SET OUT_DIR=%2
SET TARGET_DIR=%3

echo Node.Cs main
copy /Y "%SOLUTION_DIR%src\nodecs\Node.Cs\%OUT_DIR%*.dll" "%TARGET_DIR%"
copy /Y "%SOLUTION_DIR%src\nodecs\Node.Cs\%OUT_DIR%*.exe" "%TARGET_DIR%"

echo Http commons
copy /Y "%SOLUTION_DIR%src\modules\Http\%OUT_DIR%Http.dll" "%TARGET_DIR%"
copy /Y "%SOLUTION_DIR%src\modules\Http\%OUT_DIR%Http.Contexts.dll" "%TARGET_DIR%"
copy /Y "%SOLUTION_DIR%src\modules\Http\%OUT_DIR%Http.Shared.dll" "%TARGET_DIR%"
REM copy /Y "%SOLUTION_DIR%src\modules\Http.Shared\%OUT_DIR%*.Json.dll" "%TARGET_DIR%"

echo Routing engine
copy /Y "%SOLUTION_DIR%src\modules\Http.Routing\%OUT_DIR%http.routing.dll" "%TARGET_DIR%"

echo MVC
copy /Y "%SOLUTION_DIR%src\modules\Http.Mvc\%OUT_DIR%http.Mvc.dll" "%TARGET_DIR%"

echo Path providers
copy /Y "%SOLUTION_DIR%src\modules\Http.PathProvider.StaticContent\%OUT_DIR%Http.PathProvider.StaticContent.dll" "%TARGET_DIR%"

echo Renderers
copy /Y "%SOLUTION_DIR%src\modules\Http.Renderer.Markdown\%OUT_DIR%*Markdown*.dll" "%TARGET_DIR%"
copy /Y "%SOLUTION_DIR%src\modules\Http.Renderer.Razor\%OUT_DIR%*Razor*.dll" "%TARGET_DIR%"

echo Common utilities
copy /Y "%SOLUTION_DIR%src\modules\Node.Caching\%OUT_DIR%Node.Caching.dll" "%TARGET_DIR%"
copy /Y "%SOLUTION_DIR%src\modules\Curl\%OUT_DIR%Curl.dll" "%TARGET_DIR%"
copy /Y "%SOLUTION_DIR%src\modules\Log4Net\%OUT_DIR%*Log4Net*.dll" "%TARGET_DIR%"
copy /Y "%SOLUTION_DIR%src\modules\ConsoleLogger\%OUT_DIR%ConsoleLogger.dll" "%TARGET_DIR%"