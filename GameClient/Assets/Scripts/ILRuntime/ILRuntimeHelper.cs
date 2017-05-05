/*
auth: Xiang ChunSong
purpose:
*/

using UnityEngine;
using System.Collections;

public class ILRuntimeHelper
{
    static object[] param1 = new object[1];

    public static string GetClientVersion()
    {
        return ILRuntimeManager.GetScriptField("GameLogic.LogicMain", "VersionCode") as string;
    }
    
    public static string GetResourceUrl()
    {
        return ILRuntimeManager.CallScriptMethod("GameLogic.Helper", "GetResourceUrl", null, null) as string;
    }

    public static string GetLanguage(int id)
    {
        param1[0] = id;
        return ILRuntimeManager.CallScriptMethod("GameLogic.Helper", "GetLanguage", null, param1) as string;
    }
}
