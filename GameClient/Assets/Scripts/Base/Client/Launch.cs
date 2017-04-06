/*
auth: Xiang ChunSong
purpose:
*/

using UnityEngine;
using System.Collections;
using Base;

public class Launch : MonoBehaviour
{
    void Awake()
    {
        //Invoke("InitFinish", 2);
        ResourceManager.Instance.Install();
        ResourceManager.Instance.Init();
        ILRuntimeManager.Init();
        SceneLoader.LoadSceneAdditive("UI");
    }

    void Start()
    {
        ILRuntimeManager.CallScriptMethod("GameLogic.Main", "Init");
        //UIManager.Instance.Init();
    }

//     void InitFinish()
//     {
//         DestroyImmediate(gameObject);
//     }
}
