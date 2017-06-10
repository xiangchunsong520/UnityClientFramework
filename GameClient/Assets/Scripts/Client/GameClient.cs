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

public class ClientBuildSettings
{
    public bool MiniBuild;
    public bool SelectIp;
    public bool Debug;
}

public class GameClient : MonoBehaviour
{
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

    public List<string> ips = new List<string>();
    public List<int> ports = new List<int>();
    public bool isShenHe;

    void Awake()
    {
        _instance = this;
        DontDestroyOnLoad(gameObject);
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Application.targetFrameRate = 30;

#if !UNITY_EDITOR && !UNITY_STANDALONE_WIN
        Debugger.NotEditor();
        Debugger.SetWriter(new LogWriter(Application.persistentDataPath + "/log.txt"), new LogWriter(Application.persistentDataPath + "/error.txt"));
        Application.logMessageReceived += LogCallback;
        Application.logMessageReceivedThreaded += LogCallback;
#endif

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

#if !UNITY_EDITOR && !UNITY_STANDALONE_WIN
    void LogCallback(string condition, string stackTrace, LogType type)
    {
        switch (type)
        {
            case LogType.Log:
                Debugger.Log(condition, true);
                break;
            case LogType.Assert:
                Debugger.LogAssertion(condition);
                break;
            case LogType.Warning:
                Debugger.LogWarning(condition, true);
                break;
            case LogType.Error:
                Debugger.LogError(condition);
                break;
            case LogType.Exception:
                Debugger.LogException(condition);
                break;
        }
    }
#endif
}
