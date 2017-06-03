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

public class BuildProject : Editor
{
    static ChannelConfig sCurChannelConfigs = null;
    static BuildChannel sCurBuildChannel = null;
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
        try
        {
            string pluginsDir;
            string tempPluginsDir = Application.dataPath + "/../tempPlugins/";
            string buildPath = Application.dataPath + "/../../Builds/";
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
                exportDir = Application.dataPath + "/../../Builds/ExportResources/Windows/";
                pluginsDir = Application.dataPath + "/Plugins/x86/";
            }

            if (!Directory.Exists(exportDir))
            {
                UnityEngine.Debug.LogError("You should Export the resources first!!!   :" + target);
                return;
            }

            string name = FileHelper.GetStringMd5("Install/Unpackage/Data/ClientConfig.bytes".ToLower()) + ".ab";
            ClientConfig config = BuildHelper.LoadClientConfig(exportDir + name);
            if (config == null)
            {
                UnityEngine.Debug.LogError("You should Export the resources first!!!   :" + target);
                return;
            }
            UnityEngine.Debug.Log("Start Build Projects " + target + " " + DateTime.Now);

            if (Directory.Exists(tempPluginsDir))
                Directory.Delete(tempPluginsDir);
            if (Directory.Exists(pluginsDir))
                Directory.Move(pluginsDir, tempPluginsDir);

            PlayerSettings.bundleVersion = config.Version;
            string oldSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildHelper.GetBuildTargetGroup(target));
            string symbols = BuildProjectWindow.sILRuntimeDebug ? "ILRUNTIME_DEBUG" : "";
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

                if (!BuildProjectWindow.sChannelConfigs.ContainsKey(buildChannel.ChannelName))
                {
                    UnityEngine.Debug.LogError("Build channel : " + buildChannel.ChannelName + " fail!   the ChannelConfig don't have key : " + buildChannel.ChannelName);
                    continue;
                }
                sCurChannelConfigs = BuildProjectWindow.sChannelConfigs.GetUnit(buildChannel.ChannelName);

                if (string.IsNullOrEmpty(sCurChannelConfigs.BundleID))
                {
                    UnityEngine.Debug.LogError("Build channel : " + buildChannel.ChannelName + " fail!   the ChannelConfig bundleID is null!");
                    continue;
                }

                if (string.IsNullOrEmpty(sCurChannelConfigs.DownloadName))
                {
                    UnityEngine.Debug.LogError("Build channel : " + buildChannel.ChannelName + " fail!   the ChannelConfig downloadName is null!");
                    continue;
                }

                hasBuild = true;
                sCurBuildChannel = buildChannel;

                PlayerSettings.productName = sCurChannelConfigs.ProductName;
                PlayerSettings.bundleIdentifier = sCurChannelConfigs.BundleID;

                string channelPluginsDir = string.IsNullOrEmpty(buildChannel.PluginsPath) ? "" : Application.dataPath + buildChannel.PluginsPath;
                if (Directory.Exists(channelPluginsDir))
                    Directory.Move(channelPluginsDir, pluginsDir);

                AssetDatabase.Refresh();

                string saveName = sCurChannelConfigs.DownloadName;
                if (target == BuildTarget.Android)
                    saveName = saveName + ".apk";
                else if (target == BuildTarget.iOS)
                    saveName = saveName + "_temp";
                else if (target == BuildTarget.StandaloneWindows)
                    saveName = saveName + "_temp/game.exe";

                BuildPipeline.BuildPlayer(levels.ToArray(), buildPath + saveName, target, op);

                if (Directory.Exists(pluginsDir))
                    Directory.Move(pluginsDir, channelPluginsDir);
            }

            if (Directory.Exists(tempPluginsDir))
                Directory.Move(tempPluginsDir, pluginsDir);

            sCurChannelConfigs = null;
            sCurBuildChannel = null;

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

        UnityEngine.Debug.Log("Finish Build Projects " + target + " " + DateTime.Now);
    }

    [PostProcessBuild(100)]
    public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (sCurChannelConfigs == null || sCurBuildChannel == null)
            return;
        switch (target)
        {
            case BuildTarget.Android:
                OnPostProcessAndroidBuild(pathToBuiltProject);
                break;
            case BuildTarget.iOS:
                OnPostProcessIOSBuild(pathToBuiltProject);
                break;
            case BuildTarget.StandaloneWindows:
                OnPostProcessWindowsBuild(pathToBuiltProject);
                break;
        }
    }

    static void OnPostProcessAndroidBuild(string path)
    {
        UnityEngine.Debug.Log(path);
    }

    static void OnPostProcessIOSBuild(string path)
    {
        path = path.Replace("\\", "/");

        string buildPath = Application.dataPath + "/../../Builds/";
        string tempPath = path;
        string streamingDir = tempPath + "/Data/Raw/";
        if (!Directory.Exists(streamingDir))
            Directory.CreateDirectory(streamingDir);

        ResourceDatas resourceList = BuildHelper.LoadResourceDatas(exportDir + "_ResourceList.ab");

        ClientBuildSettings setting = new ClientBuildSettings();
        setting.SelectIp = sCurBuildChannel.SelectIp;
        setting.Debug = sCurBuildChannel.Debug;
        if (sCurBuildChannel.BuildMini)
        {
            setting.MiniBuild = true;
            File.WriteAllText(streamingDir + "setting.txt", JsonMapper.ToJson(setting));

            CopyGameResources(BuildTarget.iOS, streamingDir + "GameResources/", true);

            string miniDir = buildPath + "_" + sCurChannelConfigs.DownloadName;
            if (Directory.Exists(miniDir))
                Directory.Delete(miniDir, true);
            FileHelper.CopyFolder(tempPath, miniDir, true);
        }

        if (sCurBuildChannel.BuildAll)
        {
            setting.MiniBuild = false;
            File.WriteAllText(streamingDir + "setting.txt", JsonMapper.ToJson(setting));

            CopyGameResources(BuildTarget.iOS, streamingDir + "GameResources/", false);

            string allDir = buildPath + sCurChannelConfigs.DownloadName;
            if (Directory.Exists(allDir))
                Directory.Delete(allDir, true);
            FileHelper.CopyFolder(tempPath, allDir, true);
        }
        Directory.Delete(tempPath, true);
    }

    static void OnPostProcessWindowsBuild(string path)
    {
        path = path.Replace("\\", "/");

        string buildPath = Application.dataPath + "/../../Builds/";
        string tempPath = path.Substring(0, path.LastIndexOf("/"));
        string streamingDir = tempPath + "/game_Data/StreamingAssets/";
        if (!Directory.Exists(streamingDir))
            Directory.CreateDirectory(streamingDir);

        ClientBuildSettings setting = new ClientBuildSettings();
        setting.SelectIp = sCurBuildChannel.SelectIp;
        setting.Debug = sCurBuildChannel.Debug;
        if (sCurBuildChannel.BuildMini)
        {
            setting.MiniBuild = true;
            File.WriteAllText(streamingDir + "setting.txt", JsonMapper.ToJson(setting));

            CopyGameResources(BuildTarget.StandaloneWindows, streamingDir + "GameResources/", true);

            string miniDir = buildPath + "_" + sCurChannelConfigs.DownloadName;
            if (Directory.Exists(miniDir))
                Directory.Delete(miniDir, true);
            FileHelper.CopyFolder(tempPath, miniDir, true);
        }

        if (sCurBuildChannel.BuildAll)
        {
            setting.MiniBuild = false;
            File.WriteAllText(streamingDir + "setting.txt", JsonMapper.ToJson(setting));

            CopyGameResources(BuildTarget.StandaloneWindows, streamingDir + "GameResources/", false);

            string allDir = buildPath + sCurChannelConfigs.DownloadName;
            if (Directory.Exists(allDir))
                Directory.Delete(allDir, true);
            FileHelper.CopyFolder(tempPath, allDir, true);
        }
        Directory.Delete(tempPath, true);
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
