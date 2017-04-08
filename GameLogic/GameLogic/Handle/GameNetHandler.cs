using CW;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GameLogic
{
    class GameNetHandler : Singleton<GameNetHandler>
    {
        public GameNetHandler()
        {
            ((PBChannel)GameClient.Instance.TcpClient.Channel).Dispatcher.Register(PacketID.LoginProtocol, PacketID2.LoginKey, LoginKeyRes);
            ((PBChannel)GameClient.Instance.TcpClient.Channel).Dispatcher.Register(PacketID.LoginProtocol, PacketID2.LoginLogin, LoginLoginRes);
        }

        public void Init()
        {

        }

        public void LoginKeyReq()
        {
            CS_PacketLoginKeyReq req = new CS_PacketLoginKeyReq();
            req.Id1 = PacketID.LoginProtocol;
            req.Id2 = PacketID2.LoginKey;
            ((PBChannel)GameClient.Instance.TcpClient.Channel)._msgIndex = 0;
            ((PBChannel)GameClient.Instance.TcpClient.Channel).Send(req);
        }

        void LoginKeyRes(MemoryStream ms)
        {
            Debugger.LogError("GameNetHandler:LoginKeyRes");
            CS_PacketLoginKeyRes res = CS_PacketLoginKeyRes.Parser.ParseFrom(ms);
            byte[] key = new byte[8];
            int index = 0;
            for (int i = 0; i < res.Key.Count; ++i)
            {
                if (i % 32 == 5)
                {
                    key[index++] = (byte)res.Key[i];
                }
            }
            GameClient.Instance.TcpClient.Channel.Rc4Key = key;
        }

        public void LoginLoginReq(string uname, string pwd)
        {
            CS_PacketLoginLoginReq req = new CS_PacketLoginLoginReq();
            req.Id1 = PacketID.LoginProtocol;
            req.Id2 = PacketID2.LoginLogin;
            req.Username = uname;
            req.Passwd = pwd;
            req.Json = "{\"platform\":\"longyou\",\"uname\":\"xcs001\"}";
            req.Ver = "cn.1.4.0";
            ((PBChannel)GameClient.Instance.TcpClient.Channel).Send(req);
        }

        void LoginLoginRes(MemoryStream ms)
        {
            Debugger.Log("Login finish!");
        }
    }
}
