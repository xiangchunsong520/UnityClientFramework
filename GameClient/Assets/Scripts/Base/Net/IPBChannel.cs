/*
auth: Xiang ChunSong
purpose:
*/

using Google.Protobuf;
using System.IO;

namespace Base
{
    public interface IPBChannel
    {
        bool Handle(MemoryStream stream);
    }
}