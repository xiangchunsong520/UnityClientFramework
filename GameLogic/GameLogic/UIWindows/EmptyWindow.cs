using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameLogic
{
    class EmptyWindow : UIWindow
    {
        protected override void OnSetWindow()
        {
            Settings.PrefabName = "UI/EmptyWindow";
        }
    }
}
