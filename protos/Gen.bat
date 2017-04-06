cd ../tools
protoc.exe --csharp_out=../GameLogic/GameLogic/Proto/ --proto_path=../protos ../protos/*.proto
pause