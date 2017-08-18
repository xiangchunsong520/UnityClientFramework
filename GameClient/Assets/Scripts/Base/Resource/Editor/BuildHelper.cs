/*
auth: Xiang ChunSong
purpose:
*/

using Base;
using System.IO;
using Google.Protobuf;
using UnityEditor;
using BuildBase;
using System.Collections.Generic;

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

    public static ClientConfig LoadClientConfig(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        FileStream fs = new FileStream(path, FileMode.Open);
        fs.Position = 4;
        ClientConfigList config = ClientConfigList.Parser.ParseFrom(fs);
        fs.Close();
        return config.Datas[0];
    }

    public static BuildTarget GetBuildTarget(BuildPlatform platform)
    {
        if (platform == BuildPlatform.Android)
            return BuildTarget.Android;

        if (platform == BuildPlatform.Ios)
            return BuildTarget.iOS;

        return BuildTarget.StandaloneWindows;
    }

    public static BuildTargetGroup GetBuildTargetGroup(BuildTarget target)
    {
        if (target == BuildTarget.Android)
            return BuildTargetGroup.Android;

        if (target == BuildTarget.iOS)
            return BuildTargetGroup.iOS;

        return BuildTargetGroup.Standalone;
    }

    public static string GetRealPath(string path)
    {
        path = path.Replace("\\", "/");
        string[] strs = path.Split('/');
        List<string> list = new List<string>();
        for (int i = 0; i < strs.Length; ++i)
        {
            string str = strs[i];
            if (str != "..")
            {
                list.Add(str);
            }
            else
            {
                list.RemoveAt(list.Count - 1);
            }
        }
        return string.Join("/", list.ToArray());
    }
}
