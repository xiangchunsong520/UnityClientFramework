using CW;
using System;
using System.Collections.Generic;
using System.IO;
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
            ((PBChannel)GameClient.Instance.TcpClient.Channel).Dispatcher.Register(PacketID.LoginProtocol, PacketID2.LoginKey, LoginKeyRes);

            _input = GetChildComponent<InputField>("GameObject/InputField");
            _dropdpwn = GetChildComponent<Dropdown>("GameObject/Dropdown");
            EventTriggerListener.Get(GetChildGameObject("GameObject/Button")).onClick = OnClickConnect;
        }

        protected override void OnRelease()
        {
            ((PBChannel)GameClient.Instance.TcpClient.Channel).Dispatcher.Unregister(PacketID.LoginProtocol, PacketID2.LoginKey, LoginKeyRes);
        }

        void OnClickConnect(GameObject go)
        {
            string ip = _dropdpwn.options[_dropdpwn.value].text;
            int port = int.Parse(_input.text);
            Debugger.Log(_dropdpwn.options[_dropdpwn.value].text);
            Debugger.Log(_input.text);
            GameClient.Instance.TcpClient.Connect(ip, port);
            GameNetHandler.Instance.LoginKeyReq();
        }

        void LoginKeyRes(MemoryStream ms)
        {
            Debugger.LogError("ConnectServerWindow:LoginKeyRes");
            GameNetHandler.Instance.LoginLoginReq("xcs001", "1");
        }
    }
}
