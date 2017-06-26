/*
auth: Xiang ChunSong
purpose:
*/

using UnityEngine;
using UnityEditor;


[InitializeOnLoad]
public class AutoBuildGameLogic
{
    static AutoBuildGameLogic()
    {
        EditorUserBuildSettings.activeBuildTargetChanged += OnChangePlatform;
    }

    static void OnChangePlatform()
    {
        //Debug.Log("Has Pro Licence : " + Application.HasProLicense());
        Debug.Log("Platform : " + EditorUserBuildSettings.activeBuildTarget);
    }
}
