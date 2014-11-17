@echo off
call dobuild_env.bat

call dobuild_single ExpressionBuilder 4.5 45
call dobuild_single ExpressionBuilder 4.0 40
call dobuild_nuget ExpressionBuilder

call dobuild_single ClassWrapper 4.5 45
call dobuild_single ClassWrapper 4.0 40
call dobuild_nuget ClassWrapper

pause
