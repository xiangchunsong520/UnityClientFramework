using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;

namespace Base
{
    public class TCPClient
    {
        public enum State
        {
            OK = 0,
            Init = 1,
            Close = 2,
            Timeout = 3,
            InvalidMsg = 4,
            Error = 5
        }
        public delegate void ErrorCallback(State state, string errMsg);

        int MsgLenSize = 4;
        int TryCountOfRecvBufferFull = 100;

        Socket _socket;
        State _state = State.Init;
        ErrorCallback _errCallback;
        string _errMsg;
        byte[] _recvBuffer = new byte[65535];
        CircularBuffer _handleBuffer = new CircularBuffer(65535*10);
        IPBChannel _pbChannel;

        bool _circularBufferFull = false;

        public bool Connected
        {
            get
            {
                if (_socket == null)
                    return false;

                return _socket.Connected && _state == State.OK;
            }
        }

        public IPBChannel Channel
        {
            get { return _pbChannel; }
        }

        public string ErrorMsg
        {
            get { return _errMsg; }
        }

        public TCPClient()
        {
        }

        public void SetPBChannel(IPBChannel pbChannel)
        {
            _pbChannel = pbChannel;
        }

        public void Connect(string servIP, int port, ErrorCallback callback = null)
        {
            Close();

            _circularBufferFull = false;
            _handleBuffer.Clear();
            _errCallback = callback;
            //AddressFamily addressFamily;
            //string connectIP;
            //NetworkHelper.GetIPType(servIP, out connectIP, out addressFamily);
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Connect(servIP, port);

            _state = State.OK;
            _pbChannel.Rc4Key = Rc4.key;
            Receive();
        }

        public void Close()
        {
            if (_socket != null)
            {
                _socket.Close();
                _state = State.Close;
                _socket = null;
            }
        }

        private void Receive()
        {
            try
            {
                if (_state != State.OK)
                {
                    Debugger.LogError("TCPClient state is not OK!");
                    return;
                }
                _socket.BeginReceive(_recvBuffer, 0, _recvBuffer.Length, SocketFlags.None, OnReceive, null);
            }
            catch (Exception e)
            {
                Debugger.LogError(e);
            }
        }

        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                int num = _socket.EndReceive(ar);
                if (num <= 0)
                {
                    _state = State.Close;
                    Debugger.LogError("TCPClient receive failed!!");
                }
                else
                {
                    for (int i = 0; i < TryCountOfRecvBufferFull; i++)
                    {
                        if (_handleBuffer.Push(_recvBuffer, num))
                            break;
                        Debugger.LogError("wait msg handle!");
                        _circularBufferFull = true;
                        System.Threading.Thread.Sleep(100);
                        if (i == TryCountOfRecvBufferFull - 1)
                            Debugger.LogError("wait msg time out!");
                    }
                    Receive();
                }
            }
            catch (SocketException e)
            {
                _state = State.Error;
                _errMsg = e.Message;
                Debugger.LogError("TCPClient error : " + _state + "\n" + _errMsg);
            }
        }

        public void Run()
        {
            if (_state != State.OK)
            {
                if (_errCallback != null)
                {
                    _errCallback(_state, _errMsg);
                }
                return;
            }

            int i = 5;
            while (i > 0 || _circularBufferFull)
            {
                ushort totalMsgLen = _handleBuffer.PeekUshot();
                if (totalMsgLen == 0)
                    return;

                MemoryStream stream = _handleBuffer.Fetch(totalMsgLen);
                if (stream.Length == 0)
                {
                    return;
                }

                _pbChannel.Handle(stream);

                --i;
            }
            _circularBufferFull = false;
        }

        public bool SendStream(MemoryStream stream)
        {
            if (_state != State.OK)
                return false;

            bool ret = true;
            try
            {
                byte[] buffer = stream.GetBuffer();
                for (int total = 0, num = 0; total < stream.Length; total += num)
                    num = _socket.Send(buffer, total, (int)stream.Length - total, SocketFlags.None);
            }
            catch (SocketException e)
            {
                ret = false;
                _errMsg = e.Message;
                Debugger.LogError("TCPClient send fail : " + _errMsg);
            }

            return ret;
        }
    }
}