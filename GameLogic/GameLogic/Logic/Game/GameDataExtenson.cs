using Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameLogic
{
    static class GameDataExtenson
    {
        public static Actor sDefaultActor = new Actor();

        public static bool IsMao(this Actor data)
        {
            return data.Isjumao || data.Isfeimao;
        }

        public static Actor GetActorData(this DataHash<Actor> dataHash, string type)
        {
            if (dataHash.ContainsKey(type))
                return dataHash.GetUnit(type);
            return sDefaultActor;
        }
    }
}
