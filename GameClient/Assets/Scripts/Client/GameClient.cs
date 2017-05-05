/*
auth: Xiang ChunSong
purpose:
*/

using UnityEngine;
using System.Collections;
using Base;

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
    }
}
