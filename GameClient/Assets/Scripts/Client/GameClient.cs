/*
auth: Xiang ChunSong
purpose:
*/

using UnityEngine;
using System.Collections;
using Base;
using LitJson;
using System.IO;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;

public class ClientBuildSettings
{
    public bool MiniBuild;
    public bool SelectIp;
    public bool Debug;
}

public class GameClient : MonoBehaviour
{
    static bool _hasInit = false;
    static GameClient _instance;
    public static GameClient Instance
    {
        get
        {
            return _instance;
        }
    }

    TCPClient _tcpClient = new TCPClient();
    public TCPClient TcpClient
    {
        get
        {
            return _tcpClient;
        }
    }

    ClientBuildSettings _buildSettings = null;
    public ClientBuildSettings BuildSettings
    {
        get
        {
            if (_buildSettings == null)
            {
#if UNITY_EDITOR
                _buildSettings = JsonMapper.ToObject<ClientBuildSettings>(File.ReadAllText(Application.dataPath + "/../setting.txt"));
#elif UNITY_STANDALONE_WIN
                _buildSettings = JsonMapper.ToObject<ClientBuildSettings>(File.ReadAllText(Application.streamingAssetsPath + "/setting.txt"));
#elif UNITY_ANDROID
                Stream stream = StreamingAssetLoad.GetFile("setting.txt");
                StreamReader sr = new StreamReader(stream);
                string json = sr.ReadToEnd();
                sr.Close();
                stream.Close();
                _buildSettings = JsonMapper.ToObject<ClientBuildSettings>(json);
#elif UNITY_IPHONE
                _buildSettings = JsonMapper.ToObject<ClientBuildSettings>(File.ReadAllText(Application.streamingAssetsPath + "/setting.txt"));
#else
                throw new NotImplementedException();
#endif
            }
            return _buildSettings;
        }
    }

    [HideInInspector]
    public List<string> ips = new List<string>();
    [HideInInspector]
    public List<int> ports = new List<int>();
    [HideInInspector]
    public bool isShenHe;

    void Awake()
    {
#if !UNITY_EDITOR && !UNITY_STANDALONE_WIN
        Debugger.Init(Application.persistentDataPath);
#endif

        _instance = this;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Application.targetFrameRate = 30;

        ResourceManager.Instance.Init();
        ILRuntimeManager.Init();
        SceneLoader.LoadSceneAdditive("UI");
        if (BuildSettings.Debug)
        {
            GameObject go = new GameObject("GameStates");
            go.AddComponent<GameStates>();
        }
    }
    
    void Start()
    {
        ILRuntimeManager.CallScriptMethod("GameLogic.LogicMain", "Init");
    }

    void OnDestroy()
    {
        _tcpClient.Close();
    }

    void Update()
    {
        TimerManager.Instance.Update();
        _tcpClient.Run();
        ++Debugger.frameCount;
    }

    public void RestartGame()
    {
        try
        {
            _tcpClient.Close();
            SceneManager.LoadScene("Game");
            ResourceManager.UnloadUnusedAssets();
        }
        catch (Exception ex)
        {
            Debugger.LogException(ex);
        }
    }

    void OnGUI()
    {
        if (GUILayout.Button("restart"))
        {
            RestartGame();
        }
    }
}
