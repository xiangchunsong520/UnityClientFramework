/*
auth: Xiang ChunSong
purpose:
*/

using Google.Protobuf;
using System.IO;
using Utils;

namespace Base
{
    public class ResourceManager : Singleton<ResourceManager>
    {
        public ResourceManager()
        {

        }

        public void Init()
        {

        }

        ResourceDatas LoadResourceDatas(string path)
        {
            return LoadResourceDatas(new FileStream(path, FileMode.Open));
        }

        ResourceDatas LoadResourceDatas(Stream stream)
        {
            return ResourceDatas.Parser.ParseFrom(stream);
        }
    }
}
