using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Base;
using UnityEngine;
using Google.Protobuf;

namespace GameLogic
{
    public class Main
    {
        public static void Init()
        {
            UIManager.Instance.Init();
            TimerManager.Instance.AddDelayTimer(2, LaunchFinish);
        }

        static void LaunchFinish()
        {
            Debugger.Log("LaunchFinish");
        }
    }
}
