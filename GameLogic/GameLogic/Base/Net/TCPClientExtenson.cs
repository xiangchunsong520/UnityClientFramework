using Base;
using CW;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameLogic
{
    static class TCPClientExtenson
    {
        static byte[] key = { 1, 9, 8, 1, 1, 0, 3, 1 };

        public static void ConnectServer(this TCPClient tcpClient, string ip, int port)
        {
            Debugger.Log("Connect to server    " + ip + ":" + port, true);
            ((PBChannel)tcpClient.Channel).Rc4Key = key;
            tcpClient.Connect(ip, port);
        }

        public static void Register(this TCPClient tcpClient, PacketID id1, PacketID2 id2, HandleMsgCallback onHandleMsg)
        {
            ((PBChannel)tcpClient.Channel).Dispatcher.Register(id1, id2, onHandleMsg);
        }

        public static void Unregister(this TCPClient tcpClient, PacketID id1, PacketID2 id2, HandleMsgCallback onHandleMsg)
        {
            ((PBChannel)tcpClient.Channel).Dispatcher.Unregister(id1, id2, onHandleMsg);
        }

        public static bool Send<MsgT>(this TCPClient tcpClient, MsgT msg) where MsgT : IMessage
        {
            return ((PBChannel)tcpClient.Channel).Send(msg);
        }
    }
}
