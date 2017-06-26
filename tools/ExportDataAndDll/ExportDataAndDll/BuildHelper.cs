/*
auth: Xiang ChunSong
purpose:
*/

using Base;
using System.IO;
using Google.Protobuf;
using BuildBase;
//using ICSharpCode.SharpZipLib.Zip;
using System;
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

    /*public static void CreateZipFile(string filesPath, string zipFilePath, string password = "")
    {
        if (!Directory.Exists(filesPath))
        {
            //UnityEngine.Debug.LogError("Cannot find directory " + filesPath);

            return;
        }

        if (!Directory.Exists(Path.GetDirectoryName(zipFilePath)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(zipFilePath));
        }

        try
        {
            string[] filenames = Directory.GetFiles(filesPath, "*.*", SearchOption.AllDirectories);

            using (ZipOutputStream s = new ZipOutputStream(File.Create(zipFilePath)))
            {
                s.SetLevel(0);
                s.Password = password;
                byte[] buffer = new byte[4096];

                if (filesPath.EndsWith("/") || filesPath.EndsWith("\\"))
                    filesPath = filesPath.Substring(0, filesPath.Length - 1);
                int index = Path.GetDirectoryName(filesPath).Length + 1;

                foreach (string file in filenames)
                {
                    string path = file.Substring(index);

                    ZipEntry entry = new ZipEntry(path);
                    entry.CompressionMethod = CompressionMethod.Stored;
                    entry.DateTime = new DateTime(2017, 1, 1);

                    s.PutNextEntry(entry);

                    using (FileStream fs = File.OpenRead(file))
                    {
                        int sourceBytes;

                        do
                        {
                            sourceBytes = fs.Read(buffer, 0, buffer.Length);
                            s.Write(buffer, 0, sourceBytes);
                        } while (sourceBytes > 0);
                    }
                }

                s.Finish();
                s.Close();
            }
        }
        catch (Exception ex)
        {
            //UnityEngine.Debug.LogError("Exception during processing " + ex);
        }
    }*/

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
