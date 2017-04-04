﻿/*
auth: Xiang ChunSong
purpose:
*/

using Google.Protobuf;
using System.IO;

namespace Base
{
    public class ResourceManager : Singleton<ResourceManager>
    {
        public ResourceManager()
        {

        }

        public void Install()
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
