﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Base;
using UnityEngine;
using Google.Protobuf;

namespace GameLogic
{
    public class LogicMain
    {
        public static readonly string VersionCode = "0.1.0";

        public static void Init()
        {
            UIManager.Instance.Init();
            UIManager.OpenWindow("LaunchWindow");
        }
    }
}