using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class GameStates : MonoBehaviour
{
    float _updateInterval = 1f;
    float _accum = .0f;
    int _frames = 0;
    float _timeLeft = 1f;
    string _fpsFormat = "";

    void Update()
    {
        float _deltaTime = Time.deltaTime;

        _timeLeft -= _deltaTime;
        _accum += _deltaTime;
        ++_frames;
        if (_timeLeft <= 0)
        {
            float fps = _frames / _accum;
            _fpsFormat = string.Format("{0:F2}FPS", fps);

            _timeLeft = _updateInterval;
            _accum = .0f;
            _frames = 0;
        }
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.green;
        GUI.Label(new Rect(.0f, .0f, Screen.width, Screen.height), _fpsFormat, style);
    }
}
