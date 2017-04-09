//auth: Xiang ChunSong 2015/10/15
//purpose:

using System.IO;
using System;
using System.Reflection;
using Google.Protobuf.Collections;
using Base;

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
        string GetConfigFileName()
        {
            string name = typeof(MetaT).ToString();
            return name.Substring(name.LastIndexOf(".") + 1) + ".bytes";
        }

        public override bool Load()
        {
            System.Diagnostics.Stopwatch w = new System.Diagnostics.Stopwatch();
            w.Start();
            bool rsl = Load(ResourceLoader.LoadUnpackageResStream("Install/Unpackage/Data/" + GetConfigFileName()));
            w.Stop();
            if (UnityDefine.UnityEditor)
            {
                string name = typeof(MetaT).ToString();
                name = name.Substring(name.LastIndexOf(".") + 1);
                Debugger.Log("load config : " + name + " finish. Use time : " + w.ElapsedMilliseconds + " ms");
            }
            return rsl;
        }

        public override bool Load(string file)
        {
            Clear();

            if (!File.Exists(file))
            {
                return false;
            }

            MemoryStream ms = new MemoryStream(File.ReadAllBytes(file));
            if (LoadStream(ms))
            { 
                return true;
            }
            
            Debugger.LogError("Load file : " + file + "fail!");
            File.Delete(file);
            return false;
        }

        public override bool Load(byte[] bytes)
        {
            Clear();

            if (bytes == null)
            {
                return false;
            }

            MemoryStream ms = new MemoryStream(bytes);
            return LoadStream(ms);
        }

        public override bool Load(Stream stream)
        {
            Clear();

            if (stream == null)
            {
                return false;
            }

            return LoadStream(stream);
        }

        bool LoadStream(Stream ms)
        {
            try
            {
                ms.Position = 4;

                string name = typeof(MetaT).ToString() + "List";
                Type type = Type.GetType(name);
                object parser = type.GetProperty("Parser").GetValue(null, null);
                MethodInfo method = parser.GetType().GetMethod("ParseFrom", new Type[] { typeof(Stream) });
                object obj = method.Invoke(parser, new object[] { ms });
                RepeatedField<MetaT> datas = obj.GetType().GetProperty("Datas").GetValue(obj, null) as RepeatedField<MetaT>;
                
                for (int i = 0; i < datas.Count; ++i)
                {
                    if (!OnGetUnit(datas[i]))
                    {
                        return false;
                    }
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
