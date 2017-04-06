using UnityEngine;
using System.Collections;
using System.IO;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Generated;
using System;

public class ILRuntimeManager
{
    public static ILRuntime.Runtime.Enviorment.AppDomain app = null;

    public static void Init()
    {
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

#endif
            app.RegisterCrossBindingAdaptor(new MonoBehaviourAdapter());
            app.RegisterCrossBindingAdaptor(new IDisposableAdaptor());
            app.RegisterCrossBindingAdaptor(new IEnumerableAdaptor<byte>());
            app.RegisterCrossBindingAdaptor(new IEnumerableAdaptor<ILTypeInstance>());
            app.RegisterCrossBindingAdaptor(new IEnumeratorAdaptor<ILTypeInstance>());
            app.RegisterCrossBindingAdaptor(new IOExceptionAdaptor());
            app.RegisterCrossBindingAdaptor(new IComparableAdaptor<ILTypeInstance>());

            app.DelegateManager.RegisterDelegateConvertor<Action>((action) =>
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

            CLRBindings.Initialize(app);
#if UNITY_EDITOR
            app.DebugService.StartDebugService(56000);
#endif
        }
        catch (System.Exception ex)
        {
            Debugger.LogError(ex);
        }
    }

    public static IType GetScriptType(string typeName)
    {
        try
        {
            return app.GetType(typeName);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex);
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
        catch (System.Exception ex)
        {
            Debug.LogError(ex);
            return null;
        }
    }

    public static object CallScriptMethod(string typeName, string methodName, object invokeObj = null, params object[] pars)
    {
        try
        {
            return app.Invoke(typeName, methodName, invokeObj, pars);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex);
            return null;
        }
    }
}
