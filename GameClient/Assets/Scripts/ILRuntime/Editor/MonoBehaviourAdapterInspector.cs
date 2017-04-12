using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.Utils;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using ILRuntime.Runtime.Enviorment;
using Mono.Cecil;

[CustomEditor(typeof(MonoBehaviourAdapter.Adaptor), true)]
public class MonoBehaviourAdapterInspector: Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        MonoBehaviourAdapter.Adaptor clr = target as MonoBehaviourAdapter.Adaptor;
        var instance = clr.ILInstance;
        if (instance != null)
        {
            EditorGUILayout.LabelField("Script", clr.ILInstance.Type.FullName);
            foreach (var i in instance.Type.FieldMapping)
            {
                var name = i.Key;
                var type = instance.Type.FieldTypes[i.Value];
                FieldDefinition fd;
                instance.Type.GetField(i.Value, out fd);
                if (!fd.IsPublic)
                    continue;

                var cType = type.TypeForCLR;
                if (cType.IsPrimitive)//如果是基础类型
                {
                    if (cType == typeof(sbyte))
                    {
                        instance[i.Value] = (sbyte)EditorGUILayout.IntField(name, (sbyte)(int)instance[i.Value]);
                    }
                    else if (cType == typeof(short))
                    {
                        instance[i.Value] = (short)EditorGUILayout.IntField(name, (short)(int)instance[i.Value]);
                    }
                    else if (cType == typeof(int))
                    {
                        instance[i.Value] = EditorGUILayout.IntField(name, (int)instance[i.Value]);
                    }
                    else if (cType == typeof(long))
                    {
                        instance[i.Value] = EditorGUILayout.LongField(name, (long)instance[i.Value]);
                    }
                    else if (cType == typeof(byte))
                    {
                        instance[i.Value] = (byte)EditorGUILayout.IntField(name, (byte)(int)instance[i.Value]);
                    }
                    else if (cType == typeof(ushort))
                    {
                        instance[i.Value] = (ushort)EditorGUILayout.IntField(name, (ushort)(int)instance[i.Value]);
                    }
                    else if (cType == typeof(uint))
                    {
                        instance[i.Value] = (uint)EditorGUILayout.LongField(name, (uint)(int)instance[i.Value]);
                    }
                    else if (cType == typeof(ulong))
                    {
                        instance[i.Value] = (ulong)EditorGUILayout.LongField(name, (long)instance[i.Value]);
                    }
                    else if (cType == typeof(float))
                    {
                        instance[i.Value] = EditorGUILayout.FloatField(name, (float)instance[i.Value]);
                    }
                    else if (cType == typeof(double))
                    {
                        instance[i.Value] = EditorGUILayout.DoubleField(name, (double)instance[i.Value]);
                    }
                    else if (cType == typeof(bool))
                    {
                        instance[i.Value] = EditorGUILayout.Toggle(name, (int)instance[i.Value] == 1);
                    }
                    else if (cType == typeof(char))
                    {
                        var val = EditorGUILayout.TextField(name, ((char)(int)instance[i.Value]).ToString());
                        if (string.IsNullOrEmpty(val))
                            instance[i.Value] = ' ';
                        else
                            instance[i.Value] = val[0];
                    }
                    else
                        throw new System.NotImplementedException();
                }
                else
                {
                    object obj = instance[i.Value];
                    if (typeof(UnityEngine.Object).IsAssignableFrom(cType))
                    {
                        //处理Unity类型
                        var res = EditorGUILayout.ObjectField(name, obj as UnityEngine.Object, cType, true);
                        instance[i.Value] = res;
                    }
                    else
                    {
                        if (cType == typeof(string))
                        {
                            instance[i.Value] = EditorGUILayout.TextField(name, (string)instance[i.Value]);
                        }
                        else if (cType == typeof(UnityEngine.Vector2))
                        {
                            instance[i.Value] = EditorGUILayout.Vector2Field(name, (Vector2)instance[i.Value]);
                        }
                        else if (cType == typeof(UnityEngine.Vector3))
                        {
                            instance[i.Value] = EditorGUILayout.Vector3Field(name, (Vector3)instance[i.Value]);
                        }
                        else if (cType == typeof(UnityEngine.Vector4))
                        {
                            instance[i.Value] = EditorGUILayout.Vector4Field(name, (Vector4)instance[i.Value]);
                        }
                        /*else
                        {
                            if (obj != null)
                                EditorGUILayout.LabelField(name, obj.ToString());
                            else
                                EditorGUILayout.LabelField(name, "(null)");
                        }*/
                    }
                }
            }
        }
    }
}
