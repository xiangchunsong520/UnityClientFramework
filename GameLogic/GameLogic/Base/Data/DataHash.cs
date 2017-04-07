//auth: Xiang ChunSong 2015/09/24
//purpose:

using System.Collections.Generic;
using System;
using System.Reflection;

namespace GameLogic
{
    public class DataHash<MetaT> : DataLoader<MetaT> where MetaT : new()
    {
        Dictionary<object, MetaT> _dataUnits = new Dictionary<object, MetaT>();
        string _mainKey;
        PropertyInfo _mainkeyProperty;

        public int Count { get { return _dataUnits.Count; } }

        public DataHash(string mainKey = "Id")
        {
            _mainKey = mainKey;
        }

        public bool ContainsKey(object key)
        {
            return _dataUnits.ContainsKey(key);
        }

        public MetaT this[object key]
        {
            get
            {
                return GetUnit(key);
            }
        }

        public MetaT GetUnit(object key)
        {
            if (_dataUnits.ContainsKey(key))
                return _dataUnits[key];
            throw new Exception(typeof(MetaT).ToString() + " has no key : " + key);
        }

        public Dictionary<object, MetaT> GetUnits()
        {
            return _dataUnits;
        }

        protected override bool OnGetUnit(MetaT metaUnit)
        {
            object key = OnGetKey(metaUnit);
            if (!_dataUnits.ContainsKey(key))
                _dataUnits.Add(key, metaUnit);
            else
                Debugger.LogError(typeof(MetaT).ToString() + " has key:" + key);
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

