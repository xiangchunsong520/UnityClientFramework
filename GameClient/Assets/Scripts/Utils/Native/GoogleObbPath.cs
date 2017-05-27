/*
auth: Xiang ChunSong
purpose:
*/

using UnityEngine;
using System;

public class GoogleObbPath
{
    private static String EXP_PATH = @"/Android/obb/";

    static String GetExtSDPath()
    {
        AndroidJavaClass jc1 = new AndroidJavaClass("android.os.Environment");
        AndroidJavaObject jo = jc1.CallStatic<AndroidJavaObject>("getExternalStorageDirectory");
        String path = jo.Call<String>("toString");
        return path;
    }
    static String GetApkPackName()
    {
        AndroidJavaClass jc1 = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject jo = jc1.GetStatic<AndroidJavaObject>("currentActivity");
        String packname = jo.Call<String>("getPackageName");
        return packname;
    }
    static int GetApkVerCode()
    {
        AndroidJavaClass jc1 = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject jo = jc1.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject jpm = jo.Call<AndroidJavaObject>("getPackageManager");
        String packname = GetApkPackName();
        AndroidJavaObject jpi = jpm.Call<AndroidJavaObject>("getPackageInfo", packname, 0);
        int vercode = jpi.Get<int>("versionCode");
        return vercode;
    }
    public static string GetMainObbPath()
    {
        String obbpath = GetExtSDPath() + EXP_PATH + GetApkPackName() + @"/" + "main." + GetApkVerCode() + @"." + GetApkPackName() + @".obb";
        return obbpath;
    }
}
