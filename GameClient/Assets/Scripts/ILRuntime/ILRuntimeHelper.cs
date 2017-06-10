/*
auth: Xiang ChunSong
purpose:
*/

using UnityEngine;
using System.Collections;

public class ILRuntimeHelper
{
    static object[] param1 = new object[1];

    public static string GetLanguage(int id)
    {
        param1[0] = id;
        return ILRuntimeManager.CallScriptMethod("GameLogic.Helper", "GetLanguage", null, param1) as string;
    }

    public static string GetGatewayUrl()
    {
        return ILRuntimeManager.CallScriptMethod("GameLogic.Helper", "GetGatewayUrl", null, null) as string;
    }

    public static string GetVersion()
    {
        return ILRuntimeManager.CallScriptMethod("GameLogic.Helper", "GetVersion", null, null) as string;
    }

    public static string GetChannelName()
    {
        return ILRuntimeManager.CallScriptMethod("GameLogic.Helper", "GetChannelName", null, null) as string;
    }
}
