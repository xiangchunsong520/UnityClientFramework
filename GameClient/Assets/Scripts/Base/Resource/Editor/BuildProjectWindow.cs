/*
auth: Xiang ChunSong
purpose:
*/

using UnityEngine;
using UnityEditor;
using BuildBase;
using System.IO;
using System;
using Google.Protobuf;
using System.Collections.Generic;

public class BuildProjectWindow : EditorWindow
{
    static BuildProjectWindow _openWindow;
    static BuildProjectWindow sWin
    {
        get
        {
            if (_openWindow == null)
                _openWindow = GetWindow(typeof(BuildProjectWindow)) as BuildProjectWindow;

            return _openWindow;
        }
    }

    static BuildSettings _buildSettings = null;
    static BuildSettings sBuildSettings
    {
        get
        {
            if (_buildSettings == null)
            {
                if (File.Exists(buildSettingPath))
                {
                    FileStream fs = new FileStream(buildSettingPath, FileMode.Open);
                    fs.Position = 0;
                    try
                    {
                        _buildSettings = BuildSettings.Parser.ParseFrom(fs); //ProtoBuf.Serializer.Deserialize<BuildSettings>(fs);
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.Log(ex);
                        _buildSettings = new BuildSettings();
                    }
                    fs.Close();
                }
                else
                    _buildSettings = new BuildSettings();

                if (_buildSettings.BuildGroups.Count == 0)
                {
                    for (int i = 0; i < 3; ++i)
                    {
                        BuildGroup group = new BuildGroup();
                        group.Platform = (BuildPlatform)i;
                        group.Active = false;
                        _buildSettings.BuildGroups.Add(group);
                    }
                }
            }

            return _buildSettings;
        }
    }
    static readonly string buildSettingPath = Application.dataPath + "/../buildSetting.bytes";
    public static bool sDebugBuild = false;
    public static bool sILRuntimeDebug = false;

#if !RECOURCE_CLIENT
    static DataHash<ChannelConfig> _channelConfigs = null;
    public static DataHash<ChannelConfig> sChannelConfigs
    {
        get
        {
            if (_channelConfigs == null)
            {
                _channelConfigs = new DataHash<ChannelConfig>("ChannelName");
                _channelConfigs.Load(Application.dataPath + "/Resources/Install/Unpackage/Data/ChannelConfig.bytes");
            }
            return _channelConfigs;
        }
    }
#endif

    readonly string[] plantformNames = new string[] { "Android", "IOS", "Windows" };
    int selectIndex = 0;
    Vector2 scrollPos = Vector2.zero;

    [MenuItem("BuildProject/Build")]
    static void Build()
    {
        sWin.titleContent.text = "Build Projects";
        sWin.Show();
    }

    public BuildProjectWindow()
    {
        Start();
    }

    void Start()
    {
#if !RECOURCE_CLIENT
        foreach (ChannelConfig cc in sChannelConfigs.GetUnits().Values)
        {
            BuildPlatform bp = BuildPlatform.Windows;
            if (cc.Platform == "android")
                bp = BuildPlatform.Android;
            else if (cc.Platform == "ios")
                bp = BuildPlatform.Ios;

            BuildGroup bg = sBuildSettings.BuildGroups[(int)bp];
            bool exist = false;
            foreach (BuildChannel bc in bg.Channels)
            {
                if (bc.ChannelName == cc.ChannelName)
                {
                    exist = true;
                    break;
                }
            }
            if (!exist)
            {
                BuildChannel bc = new BuildChannel();
                bc.ChannelName = cc.ChannelName;
                bg.Channels.Add(bc);
            }
        }
#endif
        var a = sBuildSettings;
        SaveSettings();
    }

    void OnDestroy()
    {
        SaveSettings();
    }

    void SaveSettings()
    {
        FileStream fs = new FileStream(buildSettingPath, FileMode.Create);
        fs.Position = 0;
        sBuildSettings.WriteTo(fs);
        fs.Flush();
        fs.Close();
    }

    void OnGUI()
    {
#if !RECOURCE_CLIENT
        GUILayout.Label("Settings:");
        EditorGUILayout.BeginVertical("Box");
        EditorGUILayout.BeginHorizontal();
        selectIndex = GUILayout.SelectionGrid(selectIndex, plantformNames, 3);
        EditorGUILayout.EndHorizontal();

        float height = sWin.position.height - 210;
        height = height < 100 ? 100 : height;
        scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(height));
        DrawPlatformDetails(selectIndex);
        GUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        GUILayout.Label("Build:");
#endif
        EditorGUILayout.BeginVertical("Box");
        GUILayout.Label("Platforms:");
        EditorGUILayout.BeginHorizontal("Box", GUILayout.Width(sWin.position.width - 15));
        for (int i = 0; i < sBuildSettings.BuildGroups.Count; ++i)
        {
            sBuildSettings.BuildGroups[i].Active = GUILayout.Toggle(sBuildSettings.BuildGroups[i].Active, plantformNames[i], GUILayout.Width(150));
        }
        EditorGUILayout.EndHorizontal();

#if !RECOURCE_CLIENT
        GUILayout.Label("Actions:");
        EditorGUILayout.BeginHorizontal("Box", GUILayout.Width(sWin.position.width - 15));
        sBuildSettings.ExportResource = GUILayout.Toggle(sBuildSettings.ExportResource, "Export Resources", GUILayout.Width(150));
        sBuildSettings.BuildProject = GUILayout.Toggle(sBuildSettings.BuildProject, "Build Projects", GUILayout.Width(150));
        if (sBuildSettings.BuildProject)
        {
            sDebugBuild = GUILayout.Toggle(sDebugBuild, "DebugBuild", GUILayout.Width(150));
            sILRuntimeDebug = GUILayout.Toggle(sILRuntimeDebug, "ILRuntimeDebug", GUILayout.Width(150));
        }
        EditorGUILayout.EndHorizontal();
#else
        sBuildSettings.ExportResource = true;
        sBuildSettings.BuildProject = false;
#endif

        if (GUILayout.Button("Start", GUILayout.Height(35)))
        {
            OnClickStartBtn();
        }
        EditorGUILayout.EndVertical();
    }

#if !RECOURCE_CLIENT
    void DrawPlatformDetails(int index)
    {
        for (int i = 0; i < sBuildSettings.BuildGroups[index].Channels.Count; ++i)
        {
            EditorGUILayout.BeginHorizontal("Box", GUILayout.Width(sWin.position.width - 22));
            sBuildSettings.BuildGroups[index].Channels[i].Active = GUILayout.Toggle(sBuildSettings.BuildGroups[index].Channels[i].Active, sBuildSettings.BuildGroups[index].Channels[i].ChannelName, GUILayout.Width(150));
            
            GUILayout.Label("Plugins:", GUILayout.Width(55));
            GUILayout.TextField(sBuildSettings.BuildGroups[index].Channels[i].PluginsPath);
            if (GUILayout.Button("...", GUILayout.Width(20)))
            {
                string path = string.IsNullOrEmpty(sBuildSettings.BuildGroups[index].Channels[i].PluginsPath) ? "" : Application.dataPath + sBuildSettings.BuildGroups[index].Channels[i].PluginsPath;
                path = EditorUtility.SaveFolderPanel("Plugins path", path, "");
                string reativePath = GetRelativePath(path);
                if (!string.IsNullOrEmpty(reativePath))
                    sBuildSettings.BuildGroups[index].Channels[i].PluginsPath = reativePath;
            }

            sBuildSettings.BuildGroups[index].Channels[i].BuildMini = GUILayout.Toggle(sBuildSettings.BuildGroups[index].Channels[i].BuildMini, "MINI", GUILayout.Width(45));
            sBuildSettings.BuildGroups[index].Channels[i].BuildAll = GUILayout.Toggle(sBuildSettings.BuildGroups[index].Channels[i].BuildAll, "ALL", GUILayout.Width(40));
            sBuildSettings.BuildGroups[index].Channels[i].SelectIp = GUILayout.Toggle(sBuildSettings.BuildGroups[index].Channels[i].SelectIp, "SelectIp", GUILayout.Width(65));
            sBuildSettings.BuildGroups[index].Channels[i].Debug = GUILayout.Toggle(sBuildSettings.BuildGroups[index].Channels[i].Debug, "Debug", GUILayout.Width(55));
            EditorGUILayout.EndHorizontal();
        }
    }
#endif

    string GetRelativePath(string absolutePath)
    {
        if (string.IsNullOrEmpty(absolutePath))
            return "";
        absolutePath = absolutePath.Replace("\\", "/");
        string[] str1s = absolutePath.Split('/');
        string dataPath = Application.dataPath.Replace("\\", "/");
        string[] str2s = dataPath.Split('/');
        int step = str2s.Length;
        for (int i = 0; i < str2s.Length; ++i)
        {
            if (!str1s[i].Equals(str2s[i]))
            {
                step = i;
                break;
            }
        }

        string relativePath = "/";
        for (int i = 0; i < str2s.Length - step; ++i)
        {
            relativePath += "../";
        }
        for (int i = step; i < str1s.Length; ++i)
        {
            relativePath += str1s[i];
            relativePath += "/";
        }
        return relativePath;
    }

    void OnClickStartBtn()
    {
        if (building)
        {
            return;
        }
        build = true;
    }

    static bool build = false;
    bool building = false;
    void Update()
    {
        if (build)
        {
            build = false;
            building = true;
            UnityEngine.Debug.LogError("------------------------------------------------------------\nStart : " + DateTime.Now);
            if (sBuildSettings.ExportResource)
                ExportResources();
#if !RECOURCE_CLIENT
            if (sBuildSettings.BuildProject)
                BuildProjects();
#endif
            UnityEngine.Debug.LogError("Finish : " + DateTime.Now + "\n------------------------------------------------------------");
            //HideProgressBar();
            building = false;
            SaveSettings();
        }
    }

    static void ExportResources()
    {
        List<BuildGroup> groups = new List<BuildGroup>();
        foreach (BuildGroup group in sBuildSettings.BuildGroups)
        {
            if (group.Active)
                groups.Add(group);
        }
        if (groups.Count == 0)
        {
            return;
        }

        ExportResource.ExportResources(groups);
    }

#if !RECOURCE_CLIENT
    static void BuildProjects()
    {
        List<BuildGroup> groups = new List<BuildGroup>();
        foreach (BuildGroup group in sBuildSettings.BuildGroups)
        {
            if (group.Active)
                groups.Add(group);
        }
        if (groups.Count == 0)
        {
            return;
        }

        BuildProject.BuildProjects(groups);
    }
#endif
}