/*
auth: Xiang ChunSong
purpose:
*/

using UnityEngine;
using System.Collections;
using Base;

public class GameClient : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        ResourceManager.Instance.Init();
        ILRuntimeManager.Init();
        SceneLoader.LoadSceneAdditive("UI");
    }

    void Start()
    {
        ILRuntimeManager.CallScriptMethod("GameLogic.LogicMain", "Init");
    }

    void Update()
    {
        TimerManager.Instance.Update();
    }
}
