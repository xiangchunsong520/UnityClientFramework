using UnityEngine;
using UnityEditor;

public class BuildProject : ScriptableObject
{
    [MenuItem("Tools/BuildProject/Build")]
    static void Build()
    {
        EditorUtility.DisplayDialog("MyTool", "Do It in C# !", "OK", "");
    }
}