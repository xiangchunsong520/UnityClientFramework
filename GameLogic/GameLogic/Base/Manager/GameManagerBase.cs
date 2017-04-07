/*
auth: Xiang ChunSong
purpose: Game Manager base class
*/

using System;

namespace GameLogic
{
    public class GameManagerBase<T> : Singleton<T> where T : class, new()
    {
        public GameManagerBase()
        {
            Init();
        }

        public virtual void Init()
        {
            throw new NotImplementedException();
        }

        public virtual void Reset()
        {
            throw new NotImplementedException();
        }
    }
}
