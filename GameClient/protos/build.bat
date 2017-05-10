cd ../../tools
protoc.exe --csharp_out=../GameClient/protos --proto_path=../GameClient/protos ../GameClient/protos/*.proto

move ..\GameClient\protos\ProtocolResources.cs ..\GameClient\Assets\Scripts\Base\Resource\ProtocolResources.cs
move ..\GameClient\protos\ProtocolBuildProjectSettings.cs ..\GameClient\Assets\Scripts\Base\Resource\Editor\ProtocolBuildProjectSettings.cs

