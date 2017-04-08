/*
auth: Xiang ChunSong
purpose:
*/

using System.IO;

namespace Base
{
    public interface IPBChannel
    {
       byte[] Rc4Key { set; get; }
            
        bool Handle(MemoryStream stream);
    }
}