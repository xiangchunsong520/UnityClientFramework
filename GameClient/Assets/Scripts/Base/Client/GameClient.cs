/*
auth: Xiang ChunSong
purpose:
*/

using UnityEngine;
using System.Collections;
using Base;

public class GameClient : MonoBehaviour
{

    // Use this for initialization
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        TimerManager.Instance.Update();
    }
}
