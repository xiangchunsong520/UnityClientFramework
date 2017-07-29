using System;
using System.IO;

namespace Base
{
    class CircularBuffer
    {
        private byte[] _buf;
        private uint _size;
        private uint _write;
        private uint _read;
        private MemoryStream _stream;

        public int length
        {
            get
            {
                return Length();
            }
        }

        public CircularBuffer(int size)
        {
            // size must be a power of 2! 
            if (!IsPowerOf2(size))
                size = RoundUpPowerOf2(size);

            if (size == 0)
                size = 2;

            _buf = new byte[size];
            _size = (uint)size;
            _write = _read = 0;
            _stream = new MemoryStream(size);
        }

        public void Clear()
        {
            Array.Clear(_buf, 0, (int)_size);
            _write = _read = 0;
            _stream.Position = 0;
            _stream.SetLength(0);
        }

        // Peek only read data, but don't move the _read index
        public int PeekInt()
        {
            int ret = 0;
            MemoryStream stream = DoPeek(sizeof(int));
            if (stream.Length == sizeof(int))
            {
                byte[] retBytes = stream.ToArray();
                ret = BitConverter.ToInt32(retBytes, 0);
            }

            return ret;
        }

        public ushort PeekUshot()
        {
            ushort ret = 0;
            MemoryStream stream = DoPeek(sizeof(ushort));
            if (stream.Length == sizeof(ushort))
            {
                byte[] retBytes = stream.ToArray();
                ret = BitConverter.ToUInt16(retBytes, 0);
            }

            return ret;
        }

        public MemoryStream Fetch(int len)
        {
            MemoryStream stream = DoPeek(len);
            _read += (uint)stream.Length;

            return stream;
        }

        public bool Push(byte[] buf, int len)
        {
            if (Space() < len)
                return false;

            // first put the data starting from _write to buffer end 
            uint firstLen = Math.Min((uint)len, _size - (_write & (_size - 1)));
            Array.Copy(buf, 0, _buf, (int)(_write & (_size - 1)), (int)firstLen);
            // then put the rest (if any) at the beginning of the buffer 
            Array.Copy(buf, firstLen, _buf, 0, len - firstLen);
            _write += (uint)len;

            return true;
        }

        MemoryStream DoPeek(int len)
        {
            _stream.Position = 0;
            _stream.SetLength(0);
            uint size = _write - _read;
            if (size < len)
                return _stream;

            // first get the data from _read until the end of the buffer 
            uint firstLen = Math.Min((uint)len, _size - (_read & (_size - 1)));
            _stream.Write(_buf, (int)(_read & (_size - 1)), (int)firstLen);
            // then get the rest (if any) from the beginning of the buffer 
            _stream.Write(_buf, 0, len - (int)firstLen);
            _stream.Position = 0;

            return _stream;
        }

        int Length()
        {
            return (int)(_write - _read);
        }

        int Space()
        {
            return (int)_size - Length();
        }

        bool IsPowerOf2(int x)
        {
            return (x != 0) && ((x & (x - 1)) == 0);
        }

        int RoundUpPowerOf2(int x)
        {
            if (x == 0)
                return 0;

            double d = Math.Log((double)x) / Math.Log((double)2);
            int i = (int)d;
            i++;

            return (int)(Math.Pow((double)2, (double)i));
        }
    }
}
