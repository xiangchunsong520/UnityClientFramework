using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;

public class AssetZip
{
#if !UNITY_IPHONE || UNITY_EDITOR
    ZipFile mZipfile = null;

    Dictionary<string, long> mZipEntrys = new Dictionary<string, long>();
#endif
    // 释放资源
    public void Release()
    {
#if !UNITY_IPHONE || UNITY_EDITOR
        if (mZipfile != null)
        {
            mZipfile.Close();
            mZipfile = null;
        }
#endif
    }

    public void Init(string filepath, string password = "")
    {
#if !UNITY_IPHONE || UNITY_EDITOR
        if (!File.Exists(filepath))
        {
            return;
        }
        try
        {
            mZipfile = new ZipFile(filepath);
            mZipfile.Password = password;
            InitFileList();
        }
        catch (System.Exception e)
        {
            Debugger.LogError("AssetZip.init error!" + e.Message + ",StackTrace:" + e.StackTrace);
            return;
        }
#endif
    }

    // 初始化文件列表
    void InitFileList()
    {
#if !UNITY_IPHONE || UNITY_EDITOR
        // zip包当中的所有文件列表
        IEnumerator itor = mZipfile.GetEnumerator();
        while (itor.MoveNext())
        {
            ZipEntry entry = itor.Current as ZipEntry;
            if (entry.IsFile)
                mZipEntrys.Add(entry.Name.ToLower(), entry.ZipFileIndex);
        }
#endif
    }

    public Stream FindFileStream(string file)
    {
#if !UNITY_IPHONE || UNITY_EDITOR
        long entryIndex = -1;
        if (!mZipEntrys.TryGetValue(file.ToLower(), out entryIndex))
        {
            Debugger.LogError(string.Format("file: {0} not find!", file));
            return null;
        }

        return mZipfile.GetInputStream(entryIndex);
#else
        return null;
#endif
    }

    public void EachAllFile(System.Action<string> fun)
    {
#if !UNITY_IPHONE || UNITY_EDITOR
        foreach (KeyValuePair<string, long> itor in mZipEntrys)
            fun(itor.Key);
#endif
    }

    public bool ExistsFile(string file)
    {
#if !UNITY_IPHONE || UNITY_EDITOR
        return mZipEntrys.ContainsKey(file.ToLower());
#else
        return false;
#endif
    }
}
