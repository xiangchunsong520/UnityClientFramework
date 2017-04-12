using UnityEngine;
using UnityEditor;
using UnityEditor.UI;
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
using System;

[CustomEditor(typeof(ILRuntimeMono), true)]
public class ILRuntimeMonoInspector : Editor
{
    static string _dllname = "GameLogic";
    static string _dllpath = Application.dataPath + "/../../output/";
    static ILRuntime.Runtime.Enviorment.AppDomain _app = null;
    static DateTime _checkTime = DateTime.Now;
    static uint _crc;
    static bool _dllChange;

    static ILRuntime.Runtime.Enviorment.AppDomain app
    {
        get
        {
            if (_app == null || ((DateTime.Now - _checkTime).Seconds > 5 && _crc != FileHelper.GetCrc(_dllpath + _dllname + ".dll")))
            {
                _app = new ILRuntime.Runtime.Enviorment.AppDomain();

                FileStream msDll = new FileStream(_dllpath + _dllname + ".dll", FileMode.Open);
                FileStream msPdb = new FileStream(_dllpath + _dllname + ".pdb", FileMode.Open);
                _app.LoadAssembly(msDll, msPdb, new Mono.Cecil.Pdb.PdbReaderProvider());
                msDll.Close();
                msPdb.Close();

                _app.RegisterCrossBindingAdaptor(new MonoBehaviourAdapter());

                _checkTime = DateTime.Now;
                _crc = FileHelper.GetCrc(_dllpath + _dllname + ".dll");
                _dllChange = true;
            }
            return _app;
        }
    }
    
    string _lastScrpt = "";
    bool _typeChange;
    ILType _type;
    ILTypeInstance _instance;
    ILTypeInstance instance
    {
        get
        {
            if (_dllChange || _typeChange)
            {
                _instance = _type.Instantiate(true);
                _dllChange = false;
                _typeChange = false;
            }
            return _instance;
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        ILRuntimeMono clr = target as ILRuntimeMono;
        EditorGUILayout.BeginHorizontal();
        clr.script = EditorGUILayout.TextField("Script", clr.script);
        _typeChange = !_lastScrpt.Equals(clr.script);
        _lastScrpt = clr.script;
        EditorGUILayout.EndHorizontal();
        if (!string.IsNullOrEmpty(clr.script))
        {
            if (_type == null || _typeChange)
            {
                _type = app.GetType(clr.script) as ILType;
                if (_type == null)
                {
                    _type = app.GetType("GameLogic." + clr.script) as ILType;
                    if (_type != null)
                    {
                        clr.script = "GameLogic." + clr.script;
                    }
                }
            }
            if (_type == null)
            {
                GUI.color = Color.yellow;
                EditorGUILayout.LabelField("Can't find '" + clr.script + "' in ILRuntime dll!");
            }
            else
            {
                foreach (var f in instance.Type.FieldMapping)
                {
                    var name = f.Key;
                    var type = instance.Type.FieldTypes[f.Value];
                    FieldDefinition fd;
                    instance.Type.GetField(f.Value, out fd);
                    if (!fd.IsPublic)
                        continue;

                    var cType = type.TypeForCLR;
                    if (cType.IsPrimitive)//如果是基础类型
                    {
                        ILRuntimeMono.ILRField field = clr.GetField(name);
                        if (cType == typeof(sbyte))
                        {
                            sbyte val;
                            if (!sbyte.TryParse(field.value, out val))
                                val = 0;
                            field.value = ((sbyte)EditorGUILayout.IntField(name, val)).ToString();
                        }
                        else if (cType == typeof(short))
                        {
                            short val;
                            if (!short.TryParse(field.value, out val))
                                val = 0;
                            field.value = ((short)EditorGUILayout.IntField(name, val)).ToString();
                        }
                        else if (cType == typeof(int))
                        {
                            int val;
                            if (!int.TryParse(field.value, out val))
                                val = 0;
                            field.value = EditorGUILayout.IntField(name, val).ToString();
                        }
                        else if (cType == typeof(long))
                        {
                            sbyte val;
                            if (!sbyte.TryParse(field.value, out val))
                                val = 0;
                            field.value = EditorGUILayout.LongField(name, val).ToString();
                        }
                        else if (cType == typeof(byte))
                        {
                            byte val;
                            if (!byte.TryParse(field.value, out val))
                                val = 0;
                            field.value = ((byte)EditorGUILayout.IntField(name, val)).ToString();
                        }
                        else if (cType == typeof(ushort))
                        {
                            ushort val;
                            if (!ushort.TryParse(field.value, out val))
                                val = 0;
                            field.value = ((ushort)EditorGUILayout.IntField(name, val)).ToString();
                        }
                        else if (cType == typeof(uint))
                        {
                            uint val;
                            if (!uint.TryParse(field.value, out val))
                                val = 0;
                            field.value = ((uint)EditorGUILayout.LongField(name, val)).ToString();
                        }
                        else if (cType == typeof(ulong))
                        {
                            ulong val;
                            if (!ulong.TryParse(field.value, out val))
                                val = 0;
                            field.value = ((ulong)EditorGUILayout.LongField(name, (long)val)).ToString();
                        }
                        else if (cType == typeof(float))
                        {
                            float val;
                            if (!float.TryParse(field.value, out val))
                                val = 0;
                            field.value = EditorGUILayout.FloatField(name, val).ToString();
                        }
                        else if (cType == typeof(double))
                        {
                            double val;
                            if (!double.TryParse(field.value, out val))
                                val = 0;
                            field.value = EditorGUILayout.DoubleField(name, val).ToString();
                        }
                        else if (cType == typeof(bool))
                        {
                            bool val;
                            if (!bool.TryParse(field.value, out val))
                                val = false;
                            field.value = EditorGUILayout.Toggle(name, val).ToString();
                        }
                        else if (cType == typeof(char))
                        {
                            char val;
                            if (!char.TryParse(field.value, out val))
                                val = ' ';
                            
                            var v = EditorGUILayout.TextField(name, val.ToString());
                            if (string.IsNullOrEmpty(v))
                                field.value = " ";
                            else
                                field.value = v[0].ToString();
                        }
                    }
                    else
                    {
                        if (typeof(UnityEngine.Object).IsAssignableFrom(cType))
                        {
                            ILRuntimeMono.ILRObjField field = clr.GetObjField(name);
                            field.value = EditorGUILayout.ObjectField(name, field.value, cType, true);
                        }
                        else
                        {
                            ILRuntimeMono.ILRField field = clr.GetField(name);
                            if (cType == typeof(string))
                            {
                                field.value = EditorGUILayout.TextField(name, field.value);
                            }
                            else if (cType == typeof(UnityEngine.Vector2))
                            {
                                field.value = UnityHelper.Vector2ToString(EditorGUILayout.Vector2Field(name, UnityHelper.ParseVector2(field.value)));
                            }
                            else if (cType == typeof(UnityEngine.Vector3))
                            {
                                field.value = UnityHelper.Vector3ToString(EditorGUILayout.Vector3Field(name, UnityHelper.ParseVector3(field.value)));
                            }
                            else if (cType == typeof(UnityEngine.Vector4))
                            {
                                field.value = UnityHelper.Vector4ToString(EditorGUILayout.Vector4Field(name, UnityHelper.ParseVector4(field.value)));
                            }
                            //else
                            //    EditorGUILayout.LabelField(name + " : " + cType);
                        }
                    }
                }
            }
        }
    }
}