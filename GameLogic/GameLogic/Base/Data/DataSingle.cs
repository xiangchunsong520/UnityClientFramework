using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace GameLogic
{
    public class DataSingle<MetaT> : DataLoader<MetaT> where MetaT : new()
    {
        public MetaT data;

        protected override bool LoadBytes(byte[] bytes)
        {
            Clear();

            try
            {
                Rc4.rc4_go(ref bytes, bytes, bytes.Length, Rc4.key, Rc4.key.Length, 1);

                string name = typeof(MetaT).ToString();
                Type type = Type.GetType(name);
                object parser = type.GetProperty("Parser").GetValue(null, null);
                MethodInfo method = parser.GetType().GetMethod("ParseFrom", new Type[] { typeof(byte[]) });
                data = (MetaT)method.Invoke(parser, new object[] { bytes });
            }
            catch (Exception ex)
            {
                Debugger.LogException(ex);
                return false;
            }

            return true;
        }
    }
}
