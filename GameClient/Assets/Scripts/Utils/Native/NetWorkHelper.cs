/*
auth: Xiang ChunSong
purpose:
*/

using UnityEngine;
using System.Net.Sockets;
using System;
using System.Runtime.InteropServices;

public class NetworkHelper
{
    public enum NetworkType
    {
        NT_NONE,
        NT_WIFI,
        NT_WWAN,
    }
    
#if UNITY_IPHONE && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern string getIPv6(string mHost);

    [DllImport("__Internal")]
    private static extern int getNetworkType();
#endif

    static string GetIPv6(string mHost)
    {
#if UNITY_IPHONE && !UNITY_EDITOR
		    return getIPv6(mHost);
#else
        return mHost + "&&ipv4";
#endif
    }

    public static void GetIPType(string serverIp, out string newServerIp, out AddressFamily mIPType)
    {
        mIPType = AddressFamily.InterNetwork;
        newServerIp = serverIp;
        try
        {
            string mIPv6 = GetIPv6(serverIp);
            if (!string.IsNullOrEmpty(mIPv6))
            {
                string[] m_StrTemp = System.Text.RegularExpressions.Regex.Split(mIPv6, "&&");
                if (m_StrTemp != null && m_StrTemp.Length >= 2)
                {
                    string IPType = m_StrTemp[1];
                    if (IPType == "ipv6")
                    {
                        newServerIp = m_StrTemp[0];
                        mIPType = AddressFamily.InterNetworkV6;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debugger.LogError("GetIPv6 error:" + e);
        }
    }

    public static NetworkType GetNetWorkType()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
            return NetworkType.NT_NONE;
        
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        return NetworkType.NT_WIFI;
#elif UNITY_IPHONE
        int type = getNetworkType();
        return (NetworkType)type;
#else
        AndroidJavaClass jc1 = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject jo = jc1.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaClass jc = new AndroidJavaClass("com.game.natives.NativeHelper");
        int type = jc.CallStatic<int>("GetNetworkType", jo);
        return (NetworkType)type;
#endif
    }
}
