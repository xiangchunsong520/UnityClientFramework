/*
auth: Xiang ChunSong
purpose:Singleton base class
*/

namespace Base
{
    public class Singleton<T> where T : class, new()
    {
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new T();
                return _instance;
            }
        }
    }
}
