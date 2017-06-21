/*
auth: Xiang ChunSong
purpose:
*/

using System;
using System.Runtime.InteropServices;

public class IOSVersionCode
{
#if UNITY_IPHONE && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern string getVersionCode();
#endif

    public static int GetIOSVersionCode()
    {
#if UNITY_IPHONE && !UNITY_EDITOR
        string verstr = getVersionCode();
        string[] strs = verstr.Split('.');
        if (strs.Length == 3)
        {
            int ver = 0;
            for (int i = 0; i < 3; ++i)
            {
                int num;
                if (!int.TryParse(strs[i], out num))
                {
                    Debugger.LogError("GetIOSVersionCode error : " + verstr);
                    return 0;
                }
                ver += num * (int)Math.Pow(100, 2 - i);
            }

            return ver;
        }

        Debugger.LogError("GetIOSVersionCode error : " + verstr);
#endif
        return 0;
    }
}

