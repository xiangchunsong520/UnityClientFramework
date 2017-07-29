/*
auth: Xiang ChunSong
purpose:
*/

using System.Net;

namespace Base
{
    public interface IUDPHandle
    {
        void Handle(IPEndPoint ip, byte[] buffer, int length);
    }
}