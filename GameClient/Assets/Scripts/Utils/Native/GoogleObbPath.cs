/*
auth: Xiang ChunSong
purpose:
*/

using UnityEngine;

public class GoogleObbPath
{
    private static string EXP_PATH = @"/Android/obb/";

    static string GetExtSDPath()
    {
        AndroidJavaClass jc1 = new AndroidJavaClass("android.os.Environment");
        AndroidJavaObject jo = jc1.CallStatic<AndroidJavaObject>("getExternalStorageDirectory");
        string path = jo.Call<string>("toString");
        return path;
    }
    static string GetApkPackName()
    {
        AndroidJavaClass jc1 = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject jo = jc1.GetStatic<AndroidJavaObject>("currentActivity");
        string packname = jo.Call<string>("getPackageName");
        return packname;
    }
    public static int GetApkVerCode()
    {
        AndroidJavaClass jc1 = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject jo = jc1.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject jpm = jo.Call<AndroidJavaObject>("getPackageManager");
        string packname = GetApkPackName();
        AndroidJavaObject jpi = jpm.Call<AndroidJavaObject>("getPackageInfo", packname, 0);
        int vercode = jpi.Get<int>("versionCode");
        return vercode;
    }
    public static string GetMainObbPath()
    {
        string obbpath = GetExtSDPath() + EXP_PATH + GetApkPackName() + @"/" + "main." + GetApkVerCode() + @"." + GetApkPackName() + @".obb";
        return obbpath;
    }
}
