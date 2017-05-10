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
        public static readonly int CodeVersion = 0;    //客户端代码版本号

        public ResourceManager()
        {

        }

        public void Init()
        {

        }

        public void AfterInit()
        {
            Debugger.Log(ILRuntimeHelper.GetResourceUrl(), true);
        }

        public static ResourceDatas LoadResourceDatas(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }

            FileStream fs = new FileStream(path, FileMode.Open);
            ResourceDatas rds = LoadResourceDatas(fs);
            fs.Close();
            return rds;
        }

        public static ResourceDatas LoadResourceDatas(Stream stream)
        {
            return ResourceDatas.Parser.ParseFrom(stream);
        }

        public static void SaveResourceDatas(string path, ResourceDatas datas)
        {
            FileStream fs = new FileStream(path, FileMode.Create);
            datas.WriteTo(fs);
            fs.Flush();
            fs.Close();
        }
    }
}
