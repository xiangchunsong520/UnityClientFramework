using Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameLogic
{
    static class GameDataExtenson
    {
        public static bool IsMao(this Actor data)
        {
            return data.Isjumao || data.Isfeimao;
        }
    }
}
