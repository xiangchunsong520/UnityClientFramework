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

        protected override void OnSetWindow()
        {
            Settings.PrefabName = "UI/ConnectServerWindow";
        }

        protected override void OnInit()
        {
            GameClient.Instance.TcpClient.Channel.Register(PacketID.LoginProtocol, PacketID2.LoginKey, LoginKeyRes);

            _input = GetChildComponent<InputField>("GameObject/InputField");
            _dropdpwn = GetChildComponent<Dropdown>("GameObject/Dropdown");
            EventTriggerListener.Get(GetChildGameObject("GameObject/Button")).onClick = OnClickConnect;
        }

        protected override void OnRelease()
        {
            GameClient.Instance.TcpClient.Channel.Unregister(PacketID.LoginProtocol, PacketID2.LoginKey, LoginKeyRes);
        }

        protected override void OnOpen(object[] args)
        {
            //exit = false;
            //StartUpdate();
            //Debugger.Log("SelectServer : " + DataManager.Instance.clientConfig.SelectServer.ToString());
        }

        /*
        bool exit = false;
        protected override bool OnUpdate()
        {
            //Debugger.Log(Time.realtimeSinceStartup);
            return exit;
        }
        */
        void OnClickConnect(GameObject go)
        {
            
            string ip = _dropdpwn.options[_dropdpwn.value].text;
            int port = int.Parse(_input.text);
            GameClient.Instance.TcpClient.ConnectServer(ip, port);
            GameNetHandler.Instance.LoginKeyReq();
            //exit = true;
        }

        void LoginKeyRes(MemoryStream ms)
        {
            Debugger.Log("ConnectServerWindow:LoginKeyRes");
            GameNetHandler.Instance.LoginLoginReq("xcs001", "1");
        }
    }
}
