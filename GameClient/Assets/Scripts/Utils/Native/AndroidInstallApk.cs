/*
auth: Xiang ChunSong
purpose:
*/

using System.Runtime.InteropServices;
using UnityEngine;

public class AndroidInstallApk
{
#if !UNITY_EDITOR && UNITY_ANDROID
    [DllImport("patch", CallingConvention = CallingConvention.StdCall)]
    public static extern int patch(string oldPath, string newPath, string patchPath);
#endif

    public static void InstallApk(string apkFile)
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        AndroidJavaClass jc1 = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject jo = jc1.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaClass jc = new AndroidJavaClass("com.game.natives.NativeHelper");
        jc.CallStatic("InstallApk", jo, apkFile);
#endif
    }

    public static int GreateNewApk(string newPath, string patchPath)
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        return patch(Application.dataPath, newPath, patchPath);
#endif
        return 1;
    }
}