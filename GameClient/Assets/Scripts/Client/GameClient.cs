﻿/*
auth: Xiang ChunSong
purpose:
*/

using UnityEngine;
using System.Collections;
using Base;
using LitJson;
using System.IO;

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
#else
                throw new NotImplementedException();
#endif
            }
            return _buildSettings;
        }
    }

    void Awake()
    {
        _instance = this;
        DontDestroyOnLoad(gameObject);
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Application.targetFrameRate = 30;

        ResourceManager.Instance.Init();
        ILRuntimeManager.Init();
        ResourceManager.Instance.AfterInit();
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
}
