using UnityEngine;
using System.Collections;
using Base;

public class GameClient : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        TimerManager.Instance.Update();
        UIManager.Instance.Update();
    }
}
