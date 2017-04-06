cd ../../tools
protoc.exe --csharp_out=../GameClient/Assets/Scripts/Base/Resource/ --proto_path=../GameClient/protos ../GameClient/protos/*.proto
