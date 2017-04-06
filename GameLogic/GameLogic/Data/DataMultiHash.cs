//auth: Xiang ChunSong 2015/09/24
//purpose:

using System.Collections.Generic;
using System;
using System.Reflection;

namespace GameLogic
{
    public class DataMultiHash<MetaT> : DataLoader<MetaT> where MetaT : new()
    {
        Dictionary<object, List<MetaT>> _dataUnits = new Dictionary<object, List<MetaT>>();
        string _mainKey;
        PropertyInfo _mainkeyProperty;

        public DataMultiHash(string mainKey = "Id")
        {
            _mainKey = mainKey;
        }

        public List<MetaT> GetUnit(object key)
        {
            if (_dataUnits.ContainsKey(key))
                return _dataUnits[key];
            return default(List<MetaT>);
        }

        protected override bool OnGetUnit(MetaT metaUnit)
        {
            object key = OnGetKey(metaUnit);
            if (_dataUnits.ContainsKey(key))
                _dataUnits[key].Add(metaUnit);
            else
            {
                List<MetaT> list = new List<MetaT>();
                list.Add(metaUnit);
                _dataUnits.Add(key, list);
            }
            return true;
        }

        protected override void Clear()
        {
            _dataUnits.Clear();
        }

        protected object OnGetKey(MetaT metaUnit)
        {
            if (_mainkeyProperty == null)
            {
                _mainkeyProperty = metaUnit.GetType().GetProperty(_mainKey);
                if (_mainkeyProperty == null)
                {
                    throw new Exception(typeof(MetaT).ToString() + " has no property : " + _mainKey);
                }
            }
            return _mainkeyProperty.GetValue(metaUnit, null);
        }
    }
}
