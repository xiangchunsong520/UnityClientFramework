#if !RECOURCE_CLIENT
/*
auth: Xiang ChunSong
purpose:
*/

using UnityEngine;
using UnityEditor;
using System.Collections;
using BuildBase;
using System.Collections.Generic;
using System;
using System.IO;
using Base;
using UnityEditor.Callbacks;
using LitJson;
using System.Diagnostics;
using System.Yaml.Serialization;
using System.Text.RegularExpressions;

public class BuildProject : Editor
{
    static List<BuildChannel> sCurBuildChannels = null;
    static string exportDir;

    public static void BuildProjects(List<BuildGroup> groups)
    {
        UnityEngine.Debug.Log("----------Start Build Projects " + DateTime.Now);
        //UpdateProgressBar("正在移除Resources文件夹");
        string resourceAsset = "Assets/Resources";
        string tempResourceAsset = "Assets/ResourcesTemp";
        AssetDatabase.MoveAsset(resourceAsset, tempResourceAsset);
        //UpdateProgressBar();
        AssetDatabase.Refresh();
        //UpdateProgressBar();

        foreach (BuildGroup buildGroup in groups)
        {
            if (buildGroup.Active)
                Build(BuildHelper.GetBuildTarget(buildGroup.Platform), new List<BuildChannel>(buildGroup.Channels));
        }

        //UpdateProgressBar("正在恢复Resources文件夹");
        AssetDatabase.MoveAsset(tempResourceAsset, resourceAsset);
        //UpdateProgressBar();
        AssetDatabase.Refresh();
        //UpdateProgressBar();
        UnityEngine.Debug.Log("----------Finish Build Projects " + DateTime.Now);
    }

    static void Build(BuildTarget target, List<BuildChannel> channels)
    {
        sCurBuildChannels = channels;
        try
        {
            string pluginsDir;
            string tempPluginsDir = Application.dataPath + "/../tempPlugins/";
            string buildPath = Application.dataPath + "/../../Builds/";
            buildPath = BuildHelper.GetRealPath(buildPath);
            if (target == BuildTarget.Android)
            {
                PlayerSettings.Android.bundleVersionCode = PlayerSettings.Android.bundleVersionCode + 1;
                exportDir = Application.dataPath + "/../../Builds/ExportResources/Android/";
                pluginsDir = Application.dataPath + "/Plugins/Android/";
            }
            else if (target == BuildTarget.iOS)
            {
                int num = int.Parse(PlayerSettings.iOS.buildNumber);
                PlayerSettings.iOS.buildNumber = (num + 1).ToString();
                exportDir = Application.dataPath + "/../../Builds/ExportResources/IOS/";
                pluginsDir = Application.dataPath + "/Plugins/iOS/";
            }
            else
            {
                PlayerSettings.defaultIsFullScreen = false;
                PlayerSettings.SetAspectRatio(AspectRatio.AspectOthers, false);
                PlayerSettings.SetAspectRatio(AspectRatio.Aspect4by3, false);
                PlayerSettings.SetAspectRatio(AspectRatio.Aspect5by4, false);
                PlayerSettings.SetAspectRatio(AspectRatio.Aspect16by10, false);
                PlayerSettings.SetAspectRatio(AspectRatio.Aspect16by9, true);
                PlayerSettings.defaultScreenWidth = 1280;
                PlayerSettings.defaultScreenHeight = 720;
                //PlayerSettings.allowFullscreenSwitch = false;
                PlayerSettings.displayResolutionDialog = ResolutionDialogSetting.Disabled;
                exportDir = Application.dataPath + "/../../Builds/ExportResources/Windows/";
                pluginsDir = Application.dataPath + "/Plugins/x86/";
            }

            if (!Directory.Exists(exportDir))
            {
                UnityEngine.Debug.LogError("You should Export the resources first!!!   :" + target);
                return;
            }

            if (!File.Exists(exportDir + "_ResourceList.ab"))
            {
                UnityEngine.Debug.LogError("You should Export the resources first!!!   :" + target);
                return;
            }

            /*if (Directory.Exists(tempPluginsDir))
                Directory.Delete(tempPluginsDir);
            if (Directory.Exists(pluginsDir))
                Directory.Move(pluginsDir, tempPluginsDir);*/

            string csfile = Application.dataPath + "/../../GameLogic/GameLogic/LogicMain.cs";
            if (!File.Exists(csfile))
            {
                UnityEngine.Debug.LogError("Can't find CS file : GameLogic/GameLogic/LogicMain.cs");
                return;
            }
            string cscode = File.ReadAllText(csfile);
            Regex zhushi = new Regex(@"(\/\*[\w\W]*?\*\/)|([\/]{2,}?.*)");
            cscode = zhushi.Replace(cscode, (m) => { return ""; });
            Regex regex = new Regex("version\\s*?=\\s*?\"(?<version>[\\S]*?)\"");
            var match = regex.Match(cscode);
            if (!match.Success)
            {
                UnityEngine.Debug.LogError("The CS file : GameLogic/GameLogic/LogicMain.cs don't contains 'version'");
                return;
            }
            string version = match.Groups["version"].Value;
            if (string.IsNullOrEmpty(version))
            {
                UnityEngine.Debug.LogError("The CS file : GameLogic/GameLogic/LogicMain.cs 's value of 'version' is null");
                return;
            }
            PlayerSettings.bundleVersion = version;

            UnityEngine.Debug.Log("Start Build Projects " + target + " " + DateTime.Now);
            string oldSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildHelper.GetBuildTargetGroup(target));
            string symbols = BuildProjectWindow.sILRuntimeDebug ? "ILRUNTIME_DEBUG;FOCE_ENABLE_ILRUNTIME" : "FOCE_ENABLE_ILRUNTIME";
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildHelper.GetBuildTargetGroup(target), symbols);
            BuildOptions op = target == BuildTarget.iOS ? BuildOptions.AcceptExternalModificationsToPlayer : BuildOptions.None;
            if (BuildProjectWindow.sDebugBuild)
                op |= BuildOptions.Development;

            List<string> levels = new List<string>();
            int i = 0;
            while (levels.Count < 1)
            {
                if (EditorBuildSettings.scenes[i].enabled)
                {
                    levels.Add(EditorBuildSettings.scenes[i].path);
                }
                ++i;
            }

            bool hasBuild = false;
            foreach (BuildChannel buildChannel in channels)
            {
                if (!buildChannel.Active)
                    continue;
                if (!buildChannel.BuildMini && !buildChannel.BuildAll)
                    continue;
                hasBuild = true;
                break;
            }

            if (hasBuild)
            {
                string saveName = "";
                if (target == BuildTarget.Android)
                    saveName = "__android_empty.apk";
                else if (target == BuildTarget.iOS)
                    saveName = "__ios_empty";
                else if (target == BuildTarget.StandaloneWindows)
                    saveName = "__windows_empty/game.exe";

                BuildPipeline.BuildPlayer(levels.ToArray(), buildPath + saveName, target, op);
            }

            /*if (Directory.Exists(tempPluginsDir))
                Directory.Move(tempPluginsDir, pluginsDir);*/

            if (hasBuild)
            {
                string versions = ResourceManager.CodeVersion.ToString();
                versions += " ";
                versions += FileHelper.GetFileCrc(exportDir + "_ResourceList.ab");
                byte[] buf = System.Text.Encoding.Default.GetBytes(versions);
                File.WriteAllBytes(exportDir + "version.txt", buf);
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildHelper.GetBuildTargetGroup(target), oldSymbols);
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogException(ex);
        }

        sCurBuildChannels = null;
        UnityEngine.Debug.Log("Finish Build Projects " + target + " " + DateTime.Now);
    }

    [PostProcessBuild(100)]
    public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (sCurBuildChannels == null)
            return;

        string buildPath = Application.dataPath + "/../../Builds/";
        string outPutPath = Path.GetDirectoryName(pathToBuiltProject);
        string outFolderName = Path.GetFileNameWithoutExtension(pathToBuiltProject);
        string tempPath = "";
        switch (target)
        {
            case BuildTarget.Android:
                tempPath = Path.Combine(outPutPath, outFolderName);
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
                break;
            case BuildTarget.iOS:
                tempPath = pathToBuiltProject;
                break;
            case BuildTarget.StandaloneWindows:
                tempPath = Path.GetDirectoryName(pathToBuiltProject);
                break;
        }

        Process p = new Process();
        ProcessStartInfo pi;
        if (target == BuildTarget.Android)
        {
            pi = new ProcessStartInfo(Application.dataPath + "/../../tools/apktool.bat", "d " + pathToBuiltProject);
            pi.WorkingDirectory = outPutPath;
            pi.UseShellExecute = false;
            pi.CreateNoWindow = true;
            p.StartInfo = pi;
            p.Start();
            p.WaitForExit();

            File.Delete(pathToBuiltProject);

            string ymlPath = Path.Combine(tempPath, "apktool.yml");
            SetApktoolYmlFile(ymlPath);
        }

        string streamingDir = "";
        switch (target)
        {
            case BuildTarget.Android:
                streamingDir = tempPath + "/assets/";
                break;
            case BuildTarget.iOS:
                streamingDir = tempPath + "/Data/Raw/";
                break;
            case BuildTarget.StandaloneWindows:
                streamingDir = tempPath + "/game_Data/StreamingAssets/";
                break;
        }
        if (!Directory.Exists(streamingDir))
            Directory.CreateDirectory(streamingDir);

        CopyGameResources(target, streamingDir + "GameResources/", true);
        ClientBuildSettings setting = new ClientBuildSettings();
        foreach (BuildChannel buildChannel in sCurBuildChannels)
        {
            if (!buildChannel.Active)
                continue;

            if (!buildChannel.BuildMini)
                continue;

            if (!BuildProjectWindow.sChannelConfigs.ContainsKey(buildChannel.ChannelName))
            {
                UnityEngine.Debug.LogError("Build channel : " + buildChannel.ChannelName + " fail!   the ChannelConfig don't have key : " + buildChannel.ChannelName);
                continue;
            }

            ChannelConfig channelConfig = BuildProjectWindow.sChannelConfigs.GetUnit(buildChannel.ChannelName);

            if (string.IsNullOrEmpty(channelConfig.BundleID))
            {
                UnityEngine.Debug.LogError("Build channel : " + buildChannel.ChannelName + " fail!   the ChannelConfig bundleID is null!");
                continue;
            }

            if (string.IsNullOrEmpty(channelConfig.DownloadName))
            {
                UnityEngine.Debug.LogError("Build channel : " + buildChannel.ChannelName + " fail!   the ChannelConfig downloadName is null!");
                continue;
            }

            setting.SelectIp = buildChannel.SelectIp;
            setting.Debug = buildChannel.Debug;
            setting.MiniBuild = true;
            File.WriteAllText(streamingDir + "setting.txt", JsonMapper.ToJson(setting));
            
            CopyPlugins(target, buildChannel.PluginsPath);

            string miniDir = buildPath + "_" + channelConfig.DownloadName;

            if (target == BuildTarget.Android)
            {
                miniDir = miniDir + ".apk";
                p = new Process();
                pi = new ProcessStartInfo(Application.dataPath + "/../../tools/apktool.bat", "b " + outFolderName);
                pi.WorkingDirectory = outPutPath;
                pi.UseShellExecute = false;
                pi.CreateNoWindow = true;
                p.StartInfo = pi;
                p.Start();
                p.WaitForExit();

                if (File.Exists(miniDir))
                    File.Delete(miniDir);
                File.Move(tempPath + "/dist/" + Path.GetFileName(pathToBuiltProject), miniDir);

                p = new Process();
                pi = new ProcessStartInfo(Application.dataPath + "/../../tools/sign.bat", miniDir.Replace("/", "\\"));
                pi.UseShellExecute = false;
                pi.CreateNoWindow = true;
                p.StartInfo = pi;
                p.Start();
                p.WaitForExit();
            }
            else
            {
                if (Directory.Exists(miniDir))
                    Directory.Delete(miniDir, true);
                FileHelper.CopyFolder(tempPath, miniDir, true);
            }
        }


        CopyGameResources(target, streamingDir + "GameResources/", false);
        foreach (BuildChannel buildChannel in sCurBuildChannels)
        {
            if (!buildChannel.Active)
                continue;

            if (!buildChannel.BuildAll)
                continue;

            if (!BuildProjectWindow.sChannelConfigs.ContainsKey(buildChannel.ChannelName))
            {
                UnityEngine.Debug.LogError("Build channel : " + buildChannel.ChannelName + " fail!   the ChannelConfig don't have key : " + buildChannel.ChannelName);
                continue;
            }

            ChannelConfig channelConfig = BuildProjectWindow.sChannelConfigs.GetUnit(buildChannel.ChannelName);

            if (string.IsNullOrEmpty(channelConfig.BundleID))
            {
                UnityEngine.Debug.LogError("Build channel : " + buildChannel.ChannelName + " fail!   the ChannelConfig bundleID is null!");
                continue;
            }

            if (string.IsNullOrEmpty(channelConfig.DownloadName))
            {
                UnityEngine.Debug.LogError("Build channel : " + buildChannel.ChannelName + " fail!   the ChannelConfig downloadName is null!");
                continue;
            }

            setting.SelectIp = buildChannel.SelectIp;
            setting.Debug = buildChannel.Debug;
            setting.MiniBuild = false;
            File.WriteAllText(streamingDir + "setting.txt", JsonMapper.ToJson(setting));

            CopyPlugins(target, buildChannel.PluginsPath);

            string allDir = buildPath + channelConfig.DownloadName;

            if (target == BuildTarget.Android)
            {
                allDir = allDir + ".apk";
                p = new Process();
                pi = new ProcessStartInfo(Application.dataPath + "/../../tools/apktool.bat", "b " + outFolderName);
                pi.WorkingDirectory = outPutPath;
                pi.UseShellExecute = false;
                pi.CreateNoWindow = true;
                p.StartInfo = pi;
                p.Start();
                p.WaitForExit();

                if (File.Exists(allDir))
                    File.Delete(allDir);
                File.Move(tempPath + "/dist/" + Path.GetFileName(pathToBuiltProject), allDir);

                p = new Process();
                pi = new ProcessStartInfo(Application.dataPath + "/../../tools/sign.bat", allDir.Replace("/", "\\"));
                pi.UseShellExecute = false;
                pi.CreateNoWindow = true;
                p.StartInfo = pi;
                p.Start();
                p.WaitForExit();
            }
            else
            {
                if (Directory.Exists(allDir))
                    Directory.Delete(allDir, true);
                FileHelper.CopyFolder(tempPath, allDir, true);
            }
        }

        Directory.Delete(tempPath, true);
    }

    static void SetApktoolYmlFile(string ymlPath)
    {
        YamlSerializer serializer = new YamlSerializer();
        string[] strs = File.ReadAllLines(ymlPath);
        string line1 = strs[0];
        string[] strs2 = new string[strs.Length - 1];
        Array.Copy(strs, 1, strs2, 0, strs2.Length);
        string str = string.Join("\n", strs2);
        object[] obj = serializer.Deserialize(str);
        Dictionary<object, object> dir = obj[0] as Dictionary<object, object>;
        object[] childs = dir["doNotCompress"] as object[];
        bool hastxt = false;
        bool hasab = false;
        int needsize = 2;
        int index = childs.Length;
        for (int i = 0; i < index; ++i)
        {
            if (childs[i].Equals("txt"))
            {
                hastxt = true;
                --needsize;
            }
            else if (childs[i].Equals("ab"))
            {
                hasab = true;
                --needsize;
            }
        }
        Array.Resize(ref childs, index + needsize);
        if (!hastxt)
            childs[index++] = "txt";
        if (!hasab)
            childs[index++] = "ab";
        dir["doNotCompress"] = childs;
        string output = serializer.Serialize(dir);
        string[] outLines = output.Split('\n');
        string[] outlines2 = new string[outLines.Length - 3];
        outlines2[0] = line1;
        Array.Copy(outLines, 2, outlines2, 1, outlines2.Length - 1);
        string writestr = string.Join("\n", outlines2);
        File.WriteAllText(ymlPath, writestr);
    }

    static void CopyGameResources(BuildTarget target, string path, bool miniBuid)
    {
        string exportPath = "";
        switch (target)
        {
            case BuildTarget.Android:
                exportPath = Application.dataPath + "/../../Builds/ExportResources/Android/";
                break;
            case BuildTarget.iOS:
                exportPath = Application.dataPath + "/../../Builds/ExportResources/IOS/";
                break;
            case BuildTarget.StandaloneWindows:
                exportPath = Application.dataPath + "/../../Builds/ExportResources/Windows/";
                break;
        }
        ResourceDatas resourceList = BuildHelper.LoadResourceDatas(exportPath + "_ResourceList.ab");
        if (resourceList == null)
        {
            EditorUtility.DisplayDialog("提示", "你应该先导出资源", "好的");
            return;
        }

        ResourceDatas miniList = new ResourceDatas();

        var e = resourceList.Resources.GetEnumerator();
        while (e.MoveNext())
        {
            if ((miniBuid && e.Current.Value.IsInstall()) || (!miniBuid && !e.Current.Value.IsOptional()))
            {
                string key = e.Current.Key;
                if(miniBuid)
                    miniList.Resources.Add(key, e.Current.Value);
                string targetPath = path + key.Substring(0, 2) + "/";
                if (!Directory.Exists(targetPath))
                    Directory.CreateDirectory(targetPath);

                File.Copy(exportPath + key + ".ab", targetPath + key + ".ab", true);
            }
        }

        if (miniBuid)
            BuildHelper.SaveResourceDatas(path + "ResourceList.ab", miniList);
        else
            File.Copy(exportPath + "_ResourceList.ab", path + "ResourceList.ab", true);
    }

    static void CopyPlugins(BuildTarget target, string path)
    {
        UnityEngine.Debug.LogError("TODO:Copy Plugins");
        if (string.IsNullOrEmpty(path))
            return;

        if (!Directory.Exists(path))
            return;

        switch (target)
        {
            case BuildTarget.Android:
                
                break;
            case BuildTarget.iOS:
                
                break;
            case BuildTarget.StandaloneWindows:
                
                break;
        }
    }

    [MenuItem("BuildProject/CreateGameResources/Android/Mini")]
    static void CreateGameResourcesAndroidMini()
    {
        string path = Application.dataPath + "/../../Builds/GameResources_Android_Mini/";
        CopyGameResources(BuildTarget.Android, path, true);
    }

    [MenuItem("BuildProject/CreateGameResources/Android/All")]
    static void CreateGameResourcesAndroidAll()
    {
        string path = Application.dataPath + "/../../Builds/GameResources_Android_All/";
        CopyGameResources(BuildTarget.Android, path, false);
    }

    [MenuItem("BuildProject/CreateGameResources/IOS/Mini")]
    static void CreateGameResourcesIOSMini()
    {
        string path = Application.dataPath + "/../../Builds/GameResources_IOS_Mini/";
        CopyGameResources(BuildTarget.iOS, path, true);
    }

    [MenuItem("BuildProject/CreateGameResources/IOS/All")]
    static void CreateGameResourcesIOSAll()
    {
        string path = Application.dataPath + "/../../Builds/GameResources_IOS_All/";
        CopyGameResources(BuildTarget.iOS, path, false);
    }

    [MenuItem("BuildProject/CreateGameResources/Windows/Mini")]
    static void CreateGameResourcesWindowsMini()
    {
        string path = Application.dataPath + "/../../Builds/GameResources_Windows_Mini/";
        CopyGameResources(BuildTarget.StandaloneWindows, path, true);
    }

    [MenuItem("BuildProject/CreateGameResources/Windows/All")]
    static void CreateGameResourcesWindowsAll()
    {
        string path = Application.dataPath + "/../../Builds/GameResources_Windows_All/";
        CopyGameResources(BuildTarget.StandaloneWindows, path, false);
    }
}
#endif