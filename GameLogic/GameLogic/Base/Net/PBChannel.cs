using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Base;
using CW;
using Google.Protobuf;
using System.Reflection;

namespace GameLogic
{
    class PBChannel : IPBChannel
    {

        private TCPClient _tcpClient;
        private MsgDispatcher _msgDispatcher;
        private MemoryStream _serializeStream;
        private MemoryStream _deserializeStream;
        private MemoryStream _tempStream;
        private byte[] _tempBuf;

        public byte _msgIndex = 0;
        byte[] _rc4Key;
        public byte[] Rc4Key
        {
            get
            {
                return _rc4Key;
            }

            set
            {
                _rc4Key = value;
            }
        }

        public MsgDispatcher Dispatcher
        {
            get
            {
                return _msgDispatcher;
            }
        }

        public PBChannel(TCPClient tcpClient)
        {
            _tcpClient = tcpClient;
            _msgDispatcher = new MsgDispatcher();
            _serializeStream = new MemoryStream();
            _deserializeStream = new MemoryStream();
            _tempStream = new MemoryStream();
        }

        public bool Handle(MemoryStream stream)
        {
            int totalMsgLen = (int)stream.Length;
            if (totalMsgLen < 2)
            {
                Debugger.LogError("Invalid msg[len=" + Convert.ToString(totalMsgLen) + "]");
                return false;
            }

            stream.Read(_tempBuf, 0, 2);
            ushort lenth = BitConverter.ToUInt16(_tempBuf, 0);
            if (lenth != totalMsgLen)
            {
                Debugger.LogError("msg lenth different");
                return false;
            }

            _tempStream.Position = 0;
            _tempStream.SetLength(0);
            _tempStream.Write(stream.GetBuffer(), 2, totalMsgLen - 2);
            _tempStream.Position = 0;

            _tempBuf = _tempStream.GetBuffer();
            Rc4.rc4_go(ref _tempBuf, _tempBuf, totalMsgLen - 2, _rc4Key, _rc4Key.Length, 1);

            _deserializeStream.Position = 0;
            _deserializeStream.SetLength(0);
            _deserializeStream.Write(_tempBuf, 0, totalMsgLen - 2);
            _deserializeStream.Position = 0;
            try
            {
                PacketHeader head = PacketHeader.Parser.ParseFrom(_deserializeStream);
                
                if (UnityDefine.UnityEditor)
                {
                    Debugger.Log("Rcv msg id1:" + head.Id1 + " id2:" + head.Id2);
                }

                _deserializeStream.Position = 0;
                _msgDispatcher.Dispatch(head, _deserializeStream);

            }
            catch (Exception ex)
            {
                Debugger.LogError("Invalid msg");
                Debugger.LogException(ex);
                return false;
            }

            return true;
        }

        void SerializeTotalLen(ushort totalLen)
        {
            byte[] totalLenBytes = BitConverter.GetBytes(totalLen);
            _serializeStream.Position = 0;
            _serializeStream.Write(totalLenBytes, 0, sizeof(ushort));
        }

        public bool Send<MsgT>(MsgT msg) where MsgT : IMessage
        {
            try
            {
                _tempStream.SetLength(0);
                _tempStream.Position = 0;
                msg.WriteTo(_tempStream);
                _tempStream.WriteByte(_msgIndex++);
                _tempStream.Position = 0;
                _tempBuf = _tempStream.GetBuffer();

                Rc4.rc4_go(ref _tempBuf, _tempBuf, _tempStream.Length, _rc4Key, _rc4Key.Length, 0);
                
                _serializeStream.SetLength(0);
                _serializeStream.Position = 2;
                _serializeStream.Write(_tempBuf, 0, (int)_tempStream.Length);
                // write total msg len to stream
                SerializeTotalLen((ushort)_serializeStream.Length);

                if (_tcpClient.SendStream(_serializeStream))
                {
                    if (UnityDefine.UnityEditor)
                    {
                        Type type = msg.GetType();
                        PropertyInfo id1 = type.GetProperty("Id1");
                        object val1 = id1.GetValue(msg, null);
                        PropertyInfo id2 = type.GetProperty("Id2");
                        object val2 = id2.GetValue(msg, null);
                        Debugger.Log("Send msg id1:" + (PacketID)val1 + " id2:" + (PacketID2)val2);
                    }

                    return true;
                }

                return false;
            }
            catch (System.Exception ex)
            {
                Debugger.LogException(ex);
                return false;
            }
        }
    }

    static class IPBChannelExtension
    {
        public static void Register(this IPBChannel pbChannel, PacketID id1, PacketID2 id2, HandleMsgCallback onHandleMsg)
        {
            ((PBChannel)pbChannel).Dispatcher.Register(id1, id2, onHandleMsg);
        }

        public static void Unregister(this IPBChannel pbChannel, PacketID id1, PacketID2 id2, HandleMsgCallback onHandleMsg)
        {
            ((PBChannel)pbChannel).Dispatcher.Unregister(id1, id2, onHandleMsg);
        }

        public static bool Send<MsgT>(this IPBChannel pbChannel, MsgT msg) where MsgT : IMessage
        {
            return ((PBChannel)pbChannel).Send(msg);
        }
    }
}
