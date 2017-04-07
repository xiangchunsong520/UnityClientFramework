using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    class ConnectServerWindow : UIWindow
    {
        InputField _input;
        Dropdown _dropdpwn;

        protected override void OnInit()
        {
            _input = GetChildComponent<InputField>("GameObject/InputField");
            _dropdpwn = GetChildComponent<Dropdown>("GameObject/Dropdown");
            EventTriggerListener.Get(GetChildGameObject("GameObject/Button")).onClick = OnClickConnect;
        }

        protected override void OnRelease()
        {
            
        }

        void OnClickConnect(GameObject go)
        {
            Debugger.Log(_dropdpwn.options[_dropdpwn.value].text);
            Debugger.Log(_input.text);
        }
    }
}
