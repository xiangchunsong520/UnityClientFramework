@echo off
%~dp0MSBuild/MSBuild %~dp0../GameLogic/GameLogic.sln /t:Build /p:Configuration=Release
pause