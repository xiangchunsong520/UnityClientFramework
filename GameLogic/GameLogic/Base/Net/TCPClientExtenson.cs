using Base;
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
            Debugger.Log("Connect to server    " + ip + ":" + port);
            ((PBChannel)tcpClient.Channel).Rc4Key = key;
            tcpClient.Connect(ip, port);
        }
    }
}
