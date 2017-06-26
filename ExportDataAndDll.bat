@echo off
cd data
md temp
ConfigConvertor.exe
cd ../tools
protoc.exe --csharp_out=../GameLogic/GameLogic/Proto/ --proto_path=../data/temp ../data/temp/*.proto
cd ../data/temp
del ProtocolDatas.proto
cd ..
rd temp
cd ..
%~dp0tools/MSBuild/MSBuild %~dp0GameLogic/GameLogic.sln /t:Rebuild /p:Configuration=Release
%~dp0tools/ExportDataAndDll.exe %~dp0