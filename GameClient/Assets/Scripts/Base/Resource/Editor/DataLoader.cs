//auth: Xiang ChunSong 2015/10/15
//purpose:

using System.IO;
using System;
using System.Reflection;
using Google.Protobuf.Collections;

namespace BuildBase
{
    public abstract class DataLoaderBase
    {
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

        public override bool Load(string file)
        {
            if (!File.Exists(file))
            {
                return false;
            }
            
            bool b = LoadBytes(File.ReadAllBytes(file));
            if (b)
            {
                return true;
            }
            
            Debugger.LogError("Load file : " + file + "fail!");
            File.Delete(file);
            return false;
        }

        public override bool Load(byte[] bytes)
        {
            if (bytes == null)
            {
                return false;
            }

            return LoadBytes(bytes);
        }

        public override bool Load(Stream stream)
        {
            if (stream == null)
            {
                return false;
            }

            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            return LoadBytes(bytes);
        }

        bool LoadBytes(byte[] bytes)
        {
            Clear();

            try
            {
                Rc4.rc4_go(ref bytes, bytes, bytes.Length, Rc4.key, Rc4.key.Length, 1);

                string name = typeof(MetaT).ToString() + "List";
                Type type = Type.GetType(name);
                object parser = type.GetProperty("Parser").GetValue(null, null);
                MethodInfo method = parser.GetType().GetMethod("ParseFrom", new Type[] { typeof(byte[]) });
                object obj = method.Invoke(parser, new object[] { bytes });
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
                Debugger.LogException(ex);
                return false;
            }

            return true;
        }

        protected virtual bool OnGetUnit(MetaT metaUnit) { return true; }
        protected virtual void Clear(){}
    }
}
