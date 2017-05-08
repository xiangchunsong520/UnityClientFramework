using UnityEngine;
using UnityEditor;
using BuildBase;
using System.IO;
using System;
using Google.Protobuf;

public class BuildProject : EditorWindow
{
    #region Propertys
    static BuildProject openWindow;
    static BuildProject Win
    {
        get
        {
            if (openWindow == null)
                openWindow = GetWindow(typeof(BuildProject)) as BuildProject;

            return openWindow;
        }
    }

    static BuildSettings sBuildSettings = null;
    static BuildSettings sSettings
    {
        get
        {
            if (sBuildSettings == null)
            {
                if (File.Exists(buildSettingPath))
                {
                    FileStream fs = new FileStream(buildSettingPath, FileMode.Open);
                    fs.Position = 0;
                    try
                    {
                        sBuildSettings = BuildSettings.Parser.ParseFrom(fs); //ProtoBuf.Serializer.Deserialize<BuildSettings>(fs);
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.Log(ex);
                        sBuildSettings = new BuildSettings();
                    }
                    fs.Close();
                }
                else
                    sBuildSettings = new BuildSettings();

                if (sBuildSettings.BuildGroups.Count == 0)
                {
                    for (int i = 0; i < 3; ++i)
                    {
                        BuildGroup group = new BuildGroup();
                        group.Platform = (BuildPlatform)i;
                        group.Active = false;
                        sBuildSettings.BuildGroups.Add(group);
                    }
                }
            }

            return sBuildSettings;
        }
    }
    static readonly string buildSettingPath = Application.dataPath + "/../buildSetting.bytes";
    static bool sILRuntimeDebug = false;
    static BuildChannel sBuildChannel = null;

    static readonly string resourceDir = Application.dataPath + "/Resources/";
    static readonly string streamingDir = Application.dataPath + "/StreamingAssets/GameResources/";
    static readonly string outputDir = Application.dataPath + "/../../Builds/";

    static readonly string androidPluginsPath = Application.dataPath + "/Plugins/Android/";
    static readonly string iosPluginsPath = Application.dataPath + "/Plugins/iOS/";
    static readonly string tempPluginsPath = Application.dataPath + "/../tempPlugins/";

    readonly string[] plantformNames = new string[] { "Android", "IOS", "Windows" };
    int selectIndex = 0;
    Vector2 scrollPos = Vector2.zero;
    #endregion

#region UI
    [MenuItem("BuildProject/Build")]
    static void Build()
    {
        Win.titleContent.text = "Build Projects";
        Win.Show();
    }

    public BuildProject()
    {
        Start();
    }

    void Start()
    {
        /*foreach (ChannelConfig cc in configs.GetUnits().Values)
        {
            BuildPlatform bp = BuildPlatform.BP_WINDOWS;
            if (cc.platform == "android")
                bp = BuildPlatform.BP_ANDROID;
            else if (cc.platform == "ios")
                bp = BuildPlatform.BP_IOS;

            BuildGroup bg = settings.buildGroups[(int)bp];
            bool exist = false;
            foreach (BuildChannel bc in bg.channels)
            {
                if (bc.channelName == cc.channelName)
                {
                    exist = true;
                    break;
                }
            }
            if (!exist)
            {
                BuildChannel bc = new BuildChannel();
                bc.channelName = cc.channelName;
                bg.channels.Add(bc);
            }
        }*/
        var a = sSettings;
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
        sSettings.WriteTo(fs);
        fs.Flush();
        fs.Close();
    }

    void OnGUI()
    {
        GUILayout.Label("Settings:");
        EditorGUILayout.BeginVertical("Box");
        EditorGUILayout.BeginHorizontal();
        selectIndex = GUILayout.SelectionGrid(selectIndex, plantformNames, 3);
        EditorGUILayout.EndHorizontal();

        float height = Win.position.height - 210;
        height = height < 100 ? 100 : height;
        scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(height));
        DrawPlatformDetails(selectIndex);
        GUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        GUILayout.Label("Build:");
        EditorGUILayout.BeginVertical("Box");
        GUILayout.Label("Platforms:");
        EditorGUILayout.BeginHorizontal("Box", GUILayout.Width(Win.position.width - 15));
        for (int i = 0; i < sSettings.BuildGroups.Count; ++i)
        {
            sSettings.BuildGroups[i].Active = GUILayout.Toggle(sSettings.BuildGroups[i].Active, plantformNames[i], GUILayout.Width(150));
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Label("Actions:");
        EditorGUILayout.BeginHorizontal("Box", GUILayout.Width(Win.position.width - 15));
        sSettings.ExportResource = GUILayout.Toggle(sSettings.ExportResource, "Export Resources", GUILayout.Width(150));
        sSettings.BuildProject = GUILayout.Toggle(sSettings.BuildProject, "Build Projects", GUILayout.Width(150));
        if (sSettings.BuildProject)
        {
            sILRuntimeDebug = GUILayout.Toggle(sILRuntimeDebug, "ILRuntimeDebug", GUILayout.Width(150));
        }
        EditorGUILayout.EndHorizontal();
        
        if (GUILayout.Button("Start", GUILayout.Height(35)))
        {
            OnClickStartBtn();
        }
        EditorGUILayout.EndVertical();
    }

    void DrawPlatformDetails(int index)
    {
        for (int i = 0; i < sSettings.BuildGroups[index].Channels.Count; ++i)
        {
            EditorGUILayout.BeginHorizontal("Box", GUILayout.Width(Win.position.width - 22));
            sSettings.BuildGroups[index].Channels[i].Active = GUILayout.Toggle(sSettings.BuildGroups[index].Channels[i].Active, sSettings.BuildGroups[index].Channels[i].ChannelName, GUILayout.Width(150));

            if (sSettings.BuildGroups[index].Platform != BuildPlatform.Windows)
            {
                GUILayout.Label("Plugins:", GUILayout.Width(55));
                GUILayout.TextField(sSettings.BuildGroups[index].Channels[i].PluginsPath);
                if (GUILayout.Button("...", GUILayout.Width(20)))
                {
                    string path = string.IsNullOrEmpty(sSettings.BuildGroups[index].Channels[i].PluginsPath) ? "" : Application.dataPath + sSettings.BuildGroups[index].Channels[i].PluginsPath;
                    path = EditorUtility.SaveFolderPanel("Plugins path", path, "");
                    string reativePath = GetRelativePath(path);
                    if (!string.IsNullOrEmpty(reativePath))
                        sSettings.BuildGroups[index].Channels[i].PluginsPath = reativePath;
                }
            }
            sSettings.BuildGroups[index].Channels[i].BuildMini = GUILayout.Toggle(sSettings.BuildGroups[index].Channels[i].BuildMini, "MINI", GUILayout.Width(45));
            sSettings.BuildGroups[index].Channels[i].BuildAll = GUILayout.Toggle(sSettings.BuildGroups[index].Channels[i].BuildAll, "ALL", GUILayout.Width(40));
            sSettings.BuildGroups[index].Channels[i].SelectIp = GUILayout.Toggle(sSettings.BuildGroups[index].Channels[i].SelectIp, "SelectIp", GUILayout.Width(65));
            EditorGUILayout.EndHorizontal();
        }
    }

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
            if (sSettings.ExportResource)
                ExportResources();
            if (sSettings.BuildProject)
                BuildProjects();
            UnityEngine.Debug.LogError("Finish : " + DateTime.Now + "\n------------------------------------------------------------");
            //HideProgressBar();
            building = false;
        }
    }
    #endregion

    #region ExportResource
    static void ExportResources()
    {

    }
    #endregion

    #region BuildProjects
    static void BuildProjects()
    {

    }
    #endregion
}