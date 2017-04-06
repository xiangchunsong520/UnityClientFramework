//auth: Xiang ChunSong 2015/10/15
//purpose:

using System.IO;
using System;
using System.Reflection;
using UnityEngine;
using Google.Protobuf.Collections;

namespace GameLogic
{
    public abstract class DataLoaderBase
    {
        public abstract bool Load();
        public abstract bool Load(string file);
        public abstract bool Load(byte[] bytes);
        public abstract bool Load(Stream stream);
    }

    public class DataLoader<MetaT> : DataLoaderBase where MetaT : new()
    {
        public static string GetConfigFileName()
        {
            string name = typeof(MetaT).ToString();
            return name.Substring(name.LastIndexOf(".") + 1) + ".bytes";
        }

        public override bool Load()
        {
            string dataPath = Application.dataPath + "/Resources/Install/Unpackage/Data/" + GetConfigFileName();
            Debugger.LogError("TODO:Get path in ResourceManager!");
            return Load(dataPath);
        }

        public override bool Load(string file)
        {
            Clear();

            if (!File.Exists(file))
                return false;

            FileStream ms = new FileStream(file, FileMode.Open);
            if (LoadStream(ms))
            {
                ms.Close();
                return true;
            }

            ms.Close();
            Debugger.LogError("Load file : " + file + "fail!");
            File.Delete(file);
            return false;
        }

        public override bool Load(byte[] bytes)
        {
            Clear();

            MemoryStream ms = new MemoryStream(bytes);
            return LoadStream(ms);
        }

        public override bool Load(Stream stream)
        {
            Clear();
            return LoadStream(stream);
        }

        bool LoadStream(Stream ms)
        {
            ms.Position = 4;

            try
            {
                string name = typeof(MetaT).ToString() + "List";
                Type type = Type.GetType(name);
                object parser = type.GetProperty("Parser").GetValue(null, null);
                MethodInfo method = parser.GetType().GetMethod("ParseFrom", new Type[] { typeof(Stream) });
                object obj = method.Invoke(parser, new object[] { ms });
                RepeatedField<MetaT> datas = obj.GetType().GetProperty("Datas").GetValue(obj, null) as RepeatedField<MetaT>;
                
                for (int i = 0; i < datas.Count; ++i)
                {
                    if (!OnGetUnit(datas[i]))
                        return false;
                }
            }
            catch (Exception ex)
            {
                Debugger.LogError(ex);
                return false;
            }

            return true;
        }

        protected virtual bool OnGetUnit(MetaT metaUnit) { return true; }
        protected virtual void Clear(){}
    }
}
