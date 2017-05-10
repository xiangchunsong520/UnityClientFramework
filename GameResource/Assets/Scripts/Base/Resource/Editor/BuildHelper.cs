/*
auth: Xiang ChunSong
purpose:
*/

using Base;
using System.IO;
using Google.Protobuf;

public class BuildHelper
{
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
