/*
auth: Xiang ChunSong
purpose:
*/

using System.Collections.Generic;
using ILRuntime.Other;
using System;
using System.Collections;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.CLR.Method;
using Base;
using System.IO;

public class IPBChannelAdaptor : CrossBindingAdaptor
{
    public override Type BaseCLRType
    {
        get
        {
            return typeof(IPBChannel);
        }
    }

    public override Type AdaptorType
    {
        get
        {
            return typeof(Adaptor);
        }
    }

    public override object CreateCLRInstance(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
    {
        return new Adaptor(appdomain, instance);
    }

    internal class Adaptor : IPBChannel, CrossBindingAdaptorType
    {
        ILTypeInstance instance;
        ILRuntime.Runtime.Enviorment.AppDomain appdomain;

        public Adaptor()
        {

        }

        public Adaptor(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
        {
            this.appdomain = appdomain;
            this.instance = instance;
        }

        public ILTypeInstance ILInstance { get { return instance; } }

        object[] param = new object[1];

        IMethod mGetRc4Key;
        IMethod mSetRc4Key;
        public byte[] Rc4Key
        {
            get
            {
                if (mGetRc4Key == null)
                {
                    mGetRc4Key = instance.Type.GetMethod("get_Rc4Key", 0);
                }

                if (mGetRc4Key != null)
                {
                    return appdomain.Invoke(mGetRc4Key, instance) as byte[];
                }
                else
                {
                    return null;
                }
            }

            set
            {
                if (mSetRc4Key == null)
                {
                    mSetRc4Key = instance.Type.GetMethod("set_Rc4Key", 1);
                }

                if (mSetRc4Key != null)
                {
                    param[0] = value;
                    appdomain.Invoke(mSetRc4Key, instance, param);
                }
            }
        }

        IMethod mHandle;
        public bool Handle(MemoryStream stream)
        {
            if (mHandle == null)
            {
                mHandle = instance.Type.GetMethod("Handle", 1);
            }

            if (mHandle != null)
            {
                param[0] = stream;
                return (bool)appdomain.Invoke(mHandle, instance, param);
            }
            return false;
        }

        public override string ToString()
        {
            IMethod m = appdomain.ObjectType.GetMethod("ToString", 0);
            m = instance.Type.GetVirtualMethod(m);
            if (m == null || m is ILMethod)
            {
                return instance.ToString();
            }
            else
                return instance.Type.FullName;
        }
    }
}

