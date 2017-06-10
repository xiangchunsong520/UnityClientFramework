//auth: Xiang ChunSong 2015/09/24
//purpose:

using System.Collections.Generic;
using System;

namespace GameLogic
{
    public class DataVector<MetaT> : DataLoader<MetaT> where MetaT : new()
    {
        List<MetaT> _units = new List<MetaT>();

        public int Count
        {
            get
            {
                return _units.Count;
            }
        }

        public MetaT this[int index]
        {
            get
            {
                return GetUnit(index);
            }
        }

        public MetaT GetUnit(int index)
        {
            if (index < _units.Count && index >= 0)
                return _units[index];
            throw new Exception("Get data : " + typeof(MetaT) + " outof range, length : " + _units.Count + " index : " + index);
        }

        public List<MetaT> GetUnits()
        {
            return _units;
        }

        protected override bool OnGetUnit(MetaT metaUnit)
        {
            _units.Add(metaUnit);
            return true;
        }

        protected override void Clear()
        {
            _units.Clear();
        }
    }
}
