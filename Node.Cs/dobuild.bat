@echo off
call dobuild_env.bat

call dobuild_single ConcurrencyHelpers 4.5 net45 %CD%\src\libs\ConcurrencyHelpers
call dobuild_single ConcurrencyHelpers 4.0 net40 %CD%\src\libs\ConcurrencyHelpers

call dobuild_nuget ConcurrencyHelpers %CD%\src\libs\ConcurrencyHelpers


call dobuild_single CoroutinesLib.Shared 4.5 net45 %CD%\src\libs\CoroutinesLib.Shared
call dobuild_single CoroutinesLib.Shared 4.0 net40 %CD%\src\libs\CoroutinesLib.Shared

call dobuild_single CoroutinesLib 4.5 net45 %CD%\src\libs\CoroutinesLib
call dobuild_single CoroutinesLib 4.0 net40 %CD%\src\libs\CoroutinesLib

call dobuild_nuget CoroutinesLib %CD%\src\libs\CoroutinesLib

pause
