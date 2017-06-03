using UnityEngine;
using System.Collections;
using System.IO;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Generated;
using System;
using ILRuntime.Runtime.Stack;
using System.Collections.Generic;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.Utils;

public class ILRuntimeManager
{
    public static ILRuntime.Runtime.Enviorment.AppDomain app = null;

    public static void Init()
    {
        System.Diagnostics.Stopwatch w = new System.Diagnostics.Stopwatch();
        w.Start();

        try
        {
            app = new ILRuntime.Runtime.Enviorment.AppDomain();

            string dllname = "GameLogic";
#if UNITY_EDITOR
            string dllpath = Application.dataPath + "/../../output/";
            FileStream msDll = new FileStream(dllpath + dllname + ".dll", FileMode.Open);
            FileStream msPdb = new FileStream(dllpath + dllname + ".pdb", FileMode.Open);
            app.LoadAssembly(msDll, msPdb, new Mono.Cecil.Pdb.PdbReaderProvider());
            msDll.Close();
            msPdb.Close();
#else
#if ILRUNTIME_DEBUG
            string dllpath = Application.persistentDataPath + "/";
            Debugger.Log(dllpath, true);
            if (File.Exists(dllpath + dllname + ".dll") && File.Exists(dllpath + dllname + ".dll"))
            {
                FileStream msDll = new FileStream(dllpath + dllname + ".dll", FileMode.Open);
                FileStream msPdb = new FileStream(dllpath + dllname + ".pdb", FileMode.Open);
                app.LoadAssembly(msDll, msPdb, new Mono.Cecil.Pdb.PdbReaderProvider());
                msDll.Close();
                msPdb.Close();
            }
            else
            {
#endif
            byte[] bytes = Base.ResourceLoader.LoadUnpackageResBuffer("Install/Unpackage/GameLogic.bytes");
            Rc4.rc4_go(ref bytes, bytes, (long)bytes.Length, Rc4.key, Rc4.key.Length, 1);
            MemoryStream msDll = new MemoryStream(bytes);
            app.LoadAssembly(msDll, null, new Mono.Cecil.Pdb.PdbReaderProvider());
#if ILRUNTIME_DEBUG
            }
#endif
#endif
            SetupCrossBinding();
            SetupMethodDelegate();
            SetupCLRRedirection();
            CLRBindings.Initialize(app);

#if UNITY_EDITOR
            app.DebugService.StartDebugService(56000);
#endif
        }
        catch (Exception ex)
        {
            Debugger.LogException(ex);
        }

        w.Stop();
        Debugger.Log("Init ILRuntime finish. Use time : " + w.ElapsedMilliseconds + " ms", true);
    }

    static void SetupCrossBinding()
    {
        app.RegisterCrossBindingAdaptor(new MonoBehaviourAdapter());
        app.RegisterCrossBindingAdaptor(new IDisposableAdapter());
        app.RegisterCrossBindingAdaptor(new IEnumerableAdapter<byte>());
        app.RegisterCrossBindingAdaptor(new IEnumerableAdapter<int>());
        app.RegisterCrossBindingAdaptor(new IEnumerableAdapter<ILTypeInstance>());
        app.RegisterCrossBindingAdaptor(new IEnumeratorAdapter<ILTypeInstance>());
        app.RegisterCrossBindingAdaptor(new IEnumeratorAdapter<System.Object>());
        app.RegisterCrossBindingAdaptor(new IOExceptionAdapter());
        app.RegisterCrossBindingAdaptor(new IComparableAdapter<ILTypeInstance>());
        app.RegisterCrossBindingAdaptor(new IPBChannelAdapter());
    }

    static void SetupMethodDelegate()
    {
        app.DelegateManager.RegisterMethodDelegate<object>();
        app.DelegateManager.RegisterMethodDelegate<object[]>();
        app.DelegateManager.RegisterMethodDelegate<GameObject>();
        app.DelegateManager.RegisterMethodDelegate<MemoryStream>();

        /*app.DelegateManager.RegisterDelegateConvertor<Action>((action) =>
        {
            return new Action(() =>
            {
                ((Action)action)();
            });
        });
        app.DelegateManager.RegisterDelegateConvertor<Action<object[]>>((action) =>
        {
            return new Action<object[]>((args) =>
            {
                ((Action<object[]>)action)(args);
            });
        });
        app.DelegateManager.RegisterDelegateConvertor<Action<GameObject>>((action) =>
        {
            return new Action<GameObject>((go) =>
            {
                ((Action<GameObject>)action)(go);
            });
        });*/
    }

    unsafe static void SetupCLRRedirection()
    {
        var arr = typeof(GameObject).GetMethods();
        foreach (var i in arr)
        {
            if (i.Name == "AddComponent" && i.ContainsGenericParameters && i.IsGenericMethodDefinition && i.GetGenericArguments().Length == 1)
            {
                app.RegisterCLRMethodRedirection(i, AddComponent);
            }
            else if (i.Name == "GetComponent" && i.ContainsGenericParameters && i.IsGenericMethodDefinition && i.GetGenericArguments().Length == 1)
            {
                app.RegisterCLRMethodRedirection(i, GetComponent);
            }
        }
    }

    public static IType GetScriptType(string typeName)
    {
        try
        {
            return app.GetType(typeName);
        }
        catch (Exception ex)
        {
            Debugger.LogException(ex);
            return null;
        }
    }

    public static object GetScriptObj(string typeName)
    {
        try
        {
            IType type = GetScriptType(typeName);

            ILTypeInstance obj = ((ILType)type).Instantiate(true);

            return obj;
        }
        catch (Exception ex)
        {
            Debugger.LogException(ex);
            return null;
        }
    }

    public static object CallScriptMethod(string typeName, string methodName, object invokeObj = null, object[] pars = null)
    {
        try
        {
            return app.Invoke(typeName, methodName, invokeObj, pars);
        }
        catch (Exception ex)
        {
            Debugger.LogException(ex);
            return null;
        }
    }

    public static object GetScriptField(string typeName, string fieldName, object inastace = null)
    {
        try
        {
            return GetScriptType(typeName).ReflectionType.GetField(fieldName).GetValue(inastace);
        }
        catch (Exception ex)
        {
            Debugger.LogException(ex);
            return null;
        }
    }

    unsafe static StackObject* AddComponent(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
    {
        //CLR重定向的说明请看相关文档和教程，这里不多做解释
        ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;

        var ptr = __esp - 1;
        //成员方法的第一个参数为this
        GameObject instance = StackObject.ToObject(ptr, __domain, __mStack) as GameObject;
        if (instance == null)
            throw new System.NullReferenceException();
        __intp.Free(ptr);

        var genericArgument = __method.GenericArguments;
        //AddComponent应该有且只有1个泛型参数
        if (genericArgument != null && genericArgument.Length == 1)
        {
            var type = genericArgument[0];
            object res;
            if (type is CLRType)
            {
                //Unity主工程的类不需要任何特殊处理，直接调用Unity接口
                res = instance.AddComponent(type.TypeForCLR);
            }
            else
            {
                //热更DLL内的类型比较麻烦。首先我们得自己手动创建实例
                var ilInstance = new ILTypeInstance(type as ILType, false);//手动创建实例是因为默认方式会new MonoBehaviour，这在Unity里不允许
                //接下来创建Adapter实例
                var clrInstance = instance.AddComponent<MonoBehaviourAdapter.Adaptor>();
                //unity创建的实例并没有热更DLL里面的实例，所以需要手动赋值
                clrInstance.ILInstance = ilInstance;
                clrInstance.AppDomain = __domain;
                //这个实例默认创建的CLRInstance不是通过AddComponent出来的有效实例，所以得手动替换
                ilInstance.CLRInstance = clrInstance;

                res = clrInstance.ILInstance;//交给ILRuntime的实例应该为ILInstance

                if (clrInstance.gameObject.activeInHierarchy)
                {
                    clrInstance.Awake();//因为Unity调用这个方法时还没准备好所以这里补调一次
                    if (clrInstance.enabled)
                        clrInstance.OnEnable();
                }
            }

            return ILIntepreter.PushObject(ptr, __mStack, res);
        }

        return __esp;
    }

    unsafe static StackObject* GetComponent(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
    {
        //CLR重定向的说明请看相关文档和教程，这里不多做解释
        ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;

        var ptr = __esp - 1;
        //成员方法的第一个参数为this
        GameObject instance = StackObject.ToObject(ptr, __domain, __mStack) as GameObject;
        if (instance == null)
            throw new System.NullReferenceException();
        __intp.Free(ptr);

        var genericArgument = __method.GenericArguments;
        //AddComponent应该有且只有1个泛型参数
        if (genericArgument != null && genericArgument.Length == 1)
        {
            var type = genericArgument[0];
            object res = null;
            if (type is CLRType)
            {
                //Unity主工程的类不需要任何特殊处理，直接调用Unity接口
                res = instance.GetComponent(type.TypeForCLR);
            }
            else
            {
                //因为所有DLL里面的MonoBehaviour实际都是这个Component，所以我们只能全取出来遍历查找
                var clrInstances = instance.GetComponents<MonoBehaviourAdapter.Adaptor>();
                for (int i = 0; i < clrInstances.Length; i++)
                {
                    var clrInstance = clrInstances[i];
                    if (clrInstance.ILInstance != null)//ILInstance为null, 表示是无效的MonoBehaviour，要略过
                    {
                        if (clrInstance.ILInstance.Type == type)
                        {
                            res = clrInstance.ILInstance;//交给ILRuntime的实例应该为ILInstance
                            break;
                        }
                    }
                }
            }

            return ILIntepreter.PushObject(ptr, __mStack, res);
        }

        return __esp;
    }
}
