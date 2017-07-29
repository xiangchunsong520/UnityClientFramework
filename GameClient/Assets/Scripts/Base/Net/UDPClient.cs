//auth: Xiang ChunSong 2015/12/07
//purpose:

using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;
using System;

namespace Base
{
    public class UDPClient
    {
        UdpClient _udpClient;
        IPEndPoint _SelfIp;
        IUDPHandle _hander;

        bool _isReceive = false;

        public UDPClient()
        {
            _SelfIp = null;
        }

        public UDPClient(string ip, int port)
        {
            _SelfIp = new IPEndPoint(IPAddress.Parse(ip), port);
        }

        public void Start(IUDPHandle hander)
        {
            _hander = hander;

            _udpClient = new UdpClient();
            if (_SelfIp != null)
                _udpClient.Client.Bind(_SelfIp);
            _udpClient.Client.SendBufferSize = 65536;
            _udpClient.Client.ReceiveBufferSize = 65536;
            _isReceive = true;

            Thread th = new Thread(Receive);
            th.Start();
        }

        public void Stop()
        {
            _isReceive = false;
            if (_udpClient != null)
            {
                _udpClient.Close();
            }
        }

        public void Send(IPEndPoint target, byte[] buffer, int length)
        {
            try
            {
                if (_udpClient != null)
                {
                    int sendlen = _udpClient.Send(buffer, length, target);
                    if (sendlen != length)
                    {
                        Debugger.LogColor("FF0000FF", "send buffer fail!!" + length + "  send:" + sendlen, true);
                    }
                }
                else
                {
                    Debugger.LogColor("FF0000FF", "send buffer fail!  The udpClient is null!", true);
                }
            }
            catch (System.Exception ex)
            {
                Debugger.LogException(ex);
            }
        }

        void Receive()
        {
            IPEndPoint remoteIpep = new IPEndPoint(IPAddress.Any, 0);
            while (_isReceive)
            {
                try
                {
                    if (_udpClient == null || _udpClient.Available < 1)
                    {
                        Thread.Sleep(10);
                        continue;
                    }

                    byte[] bytRecv = _udpClient.Receive(ref remoteIpep);
                    _hander.Handle(remoteIpep, bytRecv, bytRecv.Length);
                }
                catch (Exception ex)
                {
                    Debugger.LogException(ex);
                }
            }
        }
    }
}
