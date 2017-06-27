/*
auth: Xiang ChunSong
purpose:
*/

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics;

[InitializeOnLoad]
public class AutoBuildGameLogic
{
    static AutoBuildGameLogic()
    {
        EditorUserBuildSettings.activeBuildTargetChanged += AutoBuild;
    }

    public static void AutoBuild()
    {
        switch (EditorUserBuildSettings.activeBuildTarget)
        {
            case BuildTarget.Android:
                ChangeGameLogicDefines(new string[] { "UNITY_EDITOR", "UNITY_ANDROID" });
                BuildGameLogic();
                break;
            case BuildTarget.iOS:
                ChangeGameLogicDefines(new string[] { "UNITY_EDITOR", "UNITY_IPHONE" });
                BuildGameLogic();
                break;
            case BuildTarget.StandaloneWindows:
                ChangeGameLogicDefines(new string[] { "UNITY_EDITOR", "UNITY_STANDALONE_WIN" });
                BuildGameLogic();
                break;
            default:
                break;
        }
    }

    public static void ChangeGameLogicDefines(string[] defines)
    {
        string[] matchSymbols = new string[] { "UNITY_EDITOR", "UNITY_ANDROID", "UNITY_IPHONE", "UNITY_STANDALONE_WIN" };
        string csprojPath = UnityEngine.Application.dataPath + "/../../GameLogic/GameLogic/GameLogic.csproj";

        string text = File.ReadAllText(csprojPath);
        var regex = new Regex(@"<DefineConstants>(?<define>.*?)</DefineConstants>");
        string result = regex.Replace(text, (match) =>
        {
            string define = match.Groups["define"].Value;
            string[] symbols = define.Split(';');
            List<string> list = new List<string>();
            for (int i = 0; i < symbols.Length; ++i)
            {
                bool find = false;
                for (int j = 0; j < matchSymbols.Length; ++j)
                {
                    if (symbols[i] == matchSymbols[j])
                    {
                        find = true;
                        break;
                    }
                }
                if (!find)
                    list.Add(symbols[i]);
            }
            list.AddRange(defines);
            string replace = string.Join(";", list.ToArray());
            return string.Format("<DefineConstants>{0}</DefineConstants>", replace);
        });
        File.WriteAllText(csprojPath, result);
    }

    public static void BuildGameLogic()
    {
        string slnPath = UnityEngine.Application.dataPath + "/../../GameLogic/GameLogic.sln";

        Process p = new Process();
        ProcessStartInfo pi = new ProcessStartInfo(Application.dataPath + "/../../tools/MSBuild/MSBuild.exe", slnPath + " /t:Rebuild /p:Configuration=Release");
        pi.UseShellExecute = false;
        pi.CreateNoWindow = true;
        p.StartInfo = pi;
        p.Start();
        p.WaitForExit();
    }
}
