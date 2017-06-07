/*
auth: Xiang ChunSong
purpose:
*/

using UnityEngine;
using System.Collections;
using System.IO;
using System;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

public class ObbAssetLoad
{
#if UNITY_ANDROID
    static AssetZip ApkFile = null;
#endif

    public static void Init()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (ApkFile == null)
        {
            ApkFile = new AssetZip();
            ApkFile.Init(GoogleObbPath.GetMainObbPath());
        }
#endif
    }

    public static void Release()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (ApkFile != null)
        {
            ApkFile.Release();
            ApkFile = null;

            GC.Collect();
        }
#endif
    }

    public static bool GetFile(string file, out string dstfile, out int offset)
    {
#if UNITY_EDITOR || UNITY_IPHONE || UNITY_STANDALONE_WIN
        dstfile = file;
        offset = 0;
        return false;
#elif UNITY_ANDROID // 安卓平台的，数据存储在obb包当中
        Init();
        ZipFile.PartialInputStream stream = ApkFile.FindFileStream(file) as ZipFile.PartialInputStream;
        if (stream == null)
        {
            dstfile = string.Empty;
            offset = 0;
            return false;
        }
        else
        {
            dstfile = Application.dataPath;
            offset = (int)stream.Position;
            return true;
        }
#endif
    }

    public static Stream GetFile(string file)
    {
#if UNITY_EDITOR || UNITY_IPHONE || UNITY_STANDALONE_WIN
        return null;
#elif UNITY_ANDROID // 安卓平台的，数据存储在obb包当中
        Init();
        return ApkFile.FindFileStream(file);
#endif
    }

    public static void EachAllFile(System.Action<string> fun)
    {
#if UNITY_EDITOR || UNITY_IPHONE || UNITY_STANDALONE_WIN

#elif UNITY_ANDROID // 安卓平台的，数据存储在obb包当中
        Init();
        ApkFile.EachAllFile(
            (string file) => 
            {
                fun(file);
            });
#endif
    }

    public static bool ExistsFile(string file)
    {
#if UNITY_EDITOR || UNITY_IPHONE || UNITY_STANDALONE_WIN
        return false;
#elif UNITY_ANDROID // 安卓平台的，数据存储在obb包当中
        Init();
        return ApkFile.ExistsFile(file);
#endif
    }
}
