/*
auth: Xiang ChunSong
purpose:
*/

using System.IO;

namespace Base
{
    public interface IPBChannel
    {
        bool Handle(MemoryStream stream);
    }
}