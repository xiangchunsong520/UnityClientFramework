using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Base;
using UnityEngine;

namespace GameLogic
{
    class LaunchWindow : UIWindow
    {
        protected override void OnOpen(object[] args)
        {
            Invoke(1.5f, WaiteFinish);
        }

        void WaiteFinish()
        {
            if (DataManager.Instance.clientConfig.ShowState)
            {
                GameObject go = new GameObject("GameStates");
                go.AddComponent<GameStates>();
            }
            UIManager.OpenWindow("ConnectServerWindow");
        }
    }
}
