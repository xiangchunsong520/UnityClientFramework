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
        private UdpClient _udpClient;
        private IPEndPoint _SelfIp;
        private IPEndPoint _targetIp;
        private Thread _recvThread;

        public delegate void ErrorCallBack(string error);
        private ErrorCallBack _errorCallBack = null;
        public delegate void ReceiveCallBack(byte[] buffer, int length);
        private ReceiveCallBack _receiveCallBack = null;

        private bool _isReceive = false;

        public string localEndPoint { get { return _udpClient.Client.LocalEndPoint.ToString(); } }

        public UDPClient()
        {
            _SelfIp = null;
        }

        public UDPClient(string ip, int port)
        {
            _SelfIp = new IPEndPoint(IPAddress.Parse(ip), port);
        }

        public void StartReceive(string ip, int port, ReceiveCallBack onReceive, ErrorCallBack onError = null)
        {
            Debugger.Log("UDPClient::StartReceive");

            AddressFamily addressFamily;
            string connectIP;
            NetworkHelper.GetIPType(ip, out connectIP, out addressFamily);
            _targetIp = new IPEndPoint(IPAddress.Parse(connectIP), port);
            _udpClient = new UdpClient(addressFamily);
            if (_SelfIp != null)
                _udpClient.Client.Bind(_SelfIp);
            _udpClient.Client.SendBufferSize = 1024 * 128;
            _udpClient.Client.ReceiveBufferSize = 1024 * 128;
            _receiveCallBack = onReceive;
            _errorCallBack = onError;
            _isReceive = true;
            _recvThread = new Thread(Receive);
            _recvThread.Start();
        }

        public void StopReceive()
        {
            Debugger.Log("UDPClient::StopReceive");
            _isReceive = false;
            if (_udpClient != null)
            {
                _recvThread.Abort();
                _udpClient.Close();
            }
        }

        public void Send(byte[] buffer, int length)
        {
            try
            {
                if (_udpClient != null)
                {
                    int sendlen = _udpClient.Send(buffer, length, _targetIp);
                    if (sendlen != length)
                    {
                        _errorCallBack("send buffer fail!!" + length + "  send:" + sendlen);
                    }
                }
                else
                {
                    _errorCallBack("send buffer fail!  The udpClient is null!");
                }
            }
            catch (System.Exception ex)
            {
                if (_errorCallBack != null)
                    _errorCallBack(ex.ToString());
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
                    _receiveCallBack(bytRecv, bytRecv.Length);
                }
                catch (Exception ex)
                {
                    if (_errorCallBack != null)
                        _errorCallBack(ex.ToString());
                }
            }
        }
    }
}
