//auth: Xiang ChunSong 2015/09/24
//purpose:
using System.Collections.Generic;
using System;
using System.Reflection;

namespace GameLogic
{
    public class DataDekeyHash<MetaT> : DataLoader<MetaT> where MetaT : new()
    {
        Dictionary<object, Dictionary<object, MetaT>> _dataUnits = new Dictionary<object, Dictionary<object, MetaT>>();
        string _mainKey;
        PropertyInfo _mainkeyProperty;
        string _key2;
        PropertyInfo _key2Property;

        public DataDekeyHash(string mainKey = "Id", string key2 = "Id2")
        {
            _mainKey = mainKey;
            _key2 = key2;
        }

        public Dictionary<object, MetaT> this[object key1]
        {
            get
            {
                if (_dataUnits.ContainsKey(key1))
                    return _dataUnits[key1];
                else
                {
                    Debugger.LogError(typeof(MetaT).ToString() + " has no key : " + key1);
                    return null;
                }
                   
               // throw new System.Exception(typeof(MetaT).ToString() + " has no key : " + key1);
            }
        }

        public MetaT this[object key1, object key2]
        {
            get
            {
                return GetUnit(key1, key2);
            }
        }

        public MetaT GetUnit(object key1, object key2)
        {
            if (_dataUnits.ContainsKey(key1))
            {
                Dictionary<object, MetaT> dic = _dataUnits[key1];
                if (dic.ContainsKey(key2))
                    return dic[key2];
            }

            return default(MetaT);
        }
       
        protected override bool OnGetUnit(MetaT metaUnit)
        {
            object key = OnGetKey(metaUnit);
            object key2 = OnGetKey2(metaUnit);
            if (_dataUnits.ContainsKey(key))
            {
                if (!_dataUnits[key].ContainsKey(key2))
                {
                    _dataUnits[key].Add(key2, metaUnit);
                }
                else
                {
                    Debugger.LogError(typeof(MetaT).ToString() + " has key:" + key + " key2:" + key2);
                }
            }
            else
            {
                Dictionary<object, MetaT> dic = new Dictionary<object, MetaT>();
                dic.Add(key2, metaUnit);
                _dataUnits.Add(key, dic);
            }

            return true;
        }

        protected override void Clear()
        {
            _dataUnits.Clear();
        }

        public Dictionary<object, Dictionary<object, MetaT>> GetUnits()
        {
            return _dataUnits;
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

        protected virtual object OnGetKey2(MetaT metaUnit)
        {
            if (_key2Property == null)
            {
                _key2Property = metaUnit.GetType().GetProperty(_key2);
                if (_key2Property == null)
                {
                    throw new Exception(typeof(MetaT).ToString() + " has no property : " + _key2);
                }
            }
            return _key2Property.GetValue(metaUnit, null);
        }
    }
}
