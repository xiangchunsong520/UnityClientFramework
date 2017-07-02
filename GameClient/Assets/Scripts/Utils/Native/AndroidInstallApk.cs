/*
auth: Xiang ChunSong
purpose:
*/

using System.Runtime.InteropServices;
using UnityEngine;

public class AndroidInstallApk
{
#if UNITY_EDITOR && UNITY_ANDROID
    [DllImport("patch")]
    private static extern int patch(string oldPath, string newPath, string patchPath);
#endif

    public static void InstallApk(string apkfile)
    {
#if UNITY_EDITOR && UNITY_ANDROID
        AndroidJavaClass jc1 = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject jo = jc1.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaClass jc = new AndroidJavaClass("com.game.natives.NativeHelper");
        jc.CallStatic("InstallApk", jo, apkfile);
#endif
    }

    public static int GreateNewApk(string newApkPath, string patchPath)
    {
#if UNITY_EDITOR && UNITY_ANDROID
        return patch(Application.dataPath, newApkPath, patchPath);
#else
        return 1;
#endif
    }
}