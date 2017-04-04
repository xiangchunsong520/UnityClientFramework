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
        Invoke("InitFinish", 3);
    }

    void Start()
    {
        ResourceManager.Instance.Install();
        ResourceManager.Instance.Init();
        SceneLoader.LoadSceneAdditive("UI");
        UIManager.Instance.Init();
    }

    void InitFinish()
    {
        DestroyImmediate(gameObject);
    }
}
