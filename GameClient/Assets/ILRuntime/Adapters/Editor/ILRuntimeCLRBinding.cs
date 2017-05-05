#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Collections;

[System.Reflection.Obfuscation(Exclude = true)]
public class ILRuntimeCLRBinding
{
    
    [MenuItem("ILRuntime/Generate CLR Binding Code")]
    static void GenerateCLRBinding()
    {
        List<Type> types = new List<Type>();
        types.Add(typeof(int));
        types.Add(typeof(uint));
        types.Add(typeof(short));
        types.Add(typeof(ushort));
        types.Add(typeof(sbyte));
        types.Add(typeof(byte));
        types.Add(typeof(float));
        types.Add(typeof(double));
        types.Add(typeof(long));
        types.Add(typeof(ulong));
        types.Add(typeof(object));
        types.Add(typeof(string));
        types.Add(typeof(MemoryStream));
        types.Add(typeof(DateTime));
        //types.Add(typeof(TimeSpan));
        types.Add(typeof(Stopwatch));
        types.Add(typeof(Array));
        types.Add(typeof(Hashtable));
        types.Add(typeof(Vector2));
        types.Add(typeof(Vector3));
        types.Add(typeof(Vector4));
        types.Add(typeof(Quaternion));
        types.Add(typeof(GameObject));
        types.Add(typeof(UnityEngine.Object));
        types.Add(typeof(Transform));
        types.Add(typeof(MonoBehaviour));
        types.Add(typeof(Component));
        types.Add(typeof(RectTransform));
        types.Add(typeof(Time));
        types.Add(typeof(GUILayout));
        types.Add(typeof(GUIStyle));
        types.Add(typeof(GUI));
        types.Add(typeof(UnityEngine.Debug));
        types.Add(typeof(global::Debugger));
        //所有DLL内的类型的真实C#类型都是ILTypeInstance
        types.Add(typeof(List<ILRuntime.Runtime.Intepreter.ILTypeInstance>));
        types.Add(typeof(Dictionary<ILRuntime.Runtime.Intepreter.ILTypeInstance, ILRuntime.Runtime.Intepreter.ILTypeInstance>));

        ILRuntime.Runtime.CLRBinding.BindingCodeGenerator.GenerateBindingCode(types, "Assets/ILRuntime/Generated");
    }
}
#endif
