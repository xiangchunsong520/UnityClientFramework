/*
auth: Xiang ChunSong
purpose:
*/

using UnityEngine;
using System.Collections;

public class ILRuntimeHelper
{
    static object[] param1 = new object[1];

    public static string GetLanguage(string id)
    {
        param1[0] = id;
        return ILRuntimeManager.CallScriptMethod("GameLogic.Helper", "GetLanguage", null, param1) as string;
    }

    public static string GetGatewayUrl()
    {
        return ILRuntimeManager.CallScriptMethod("GameLogic.Helper", "GetGatewayUrl", null, null) as string;
    }

    public static string GetChannelName()
    {
        return ILRuntimeManager.CallScriptMethod("GameLogic.Helper", "GetChannelName", null, null) as string;
    }

    public static bool GetUpdateInGame()
    {
        return (bool)ILRuntimeManager.CallScriptMethod("GameLogic.Helper", "GetUpdateInGame", null, null);
    }

    public static string GetDownladName()
    {
        return ILRuntimeManager.CallScriptMethod("GameLogic.Helper", "GetDownladName", null, null) as string;
    }
}
