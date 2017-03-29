cd ../tools
protoc.exe --proto_path=../protos/ --csharp_out=../GameClient/Assets/Scripts/Base/Resource/ --proto_path=../protos ../protos/*.proto
