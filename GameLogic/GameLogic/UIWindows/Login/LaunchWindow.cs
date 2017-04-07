using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Base;

namespace GameLogic
{
    class LaunchWindow : UIWindow
    {
        protected override void OnOpen(object[] args)
        {
            Debugger.Log("LaunchWindow:OnOpen");
            Invoke(1.5f, WaiteFinish);
        }

        void WaiteFinish()
        {
            Debugger.Log("LaunchWindow:WaiteFinish");
            UIManager.OpenWindow("ConnectServerWindow");
        }
    }
}
