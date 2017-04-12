/*
auth: Xiang ChunSong
purpose:
*/

using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.CLR.TypeSystem;
using Base;

public class ILRuntimeMono : MonoBehaviour
{
    public string script = "";
    public List<ILRField> fields = new List<ILRField>();
    public List<ILRObjField> objFields = new List<ILRObjField>();

    [Serializable]
    public class ILRField
    {
        public string name;
        public string value;
    }

    [Serializable]
    public class ILRObjField
    {
        public string name;
        public UnityEngine.Object value;
    }

    void Awake()
    {
        IType itype = ILRuntimeManager.GetScriptType(script);
        if (itype != null)
        {
            ILTypeInstance instance = new ILTypeInstance(itype as ILType, false); ;
            if (instance != null)
            {
                for (int i = 0; i < fields.Count; ++i)
                {
                    int index;
                    var type = instance.Type.GetField(fields[i].name, out index);
                    if (type != null)
                    {
                        var cType = type.TypeForCLR;
                        if (cType.IsPrimitive)
                        {
                            if (cType == typeof(sbyte))
                            {
                                sbyte val;
                                if (!sbyte.TryParse(fields[i].value, out val))
                                    val = 0;
                                instance[index] = val;
                            }
                            else if (cType == typeof(short))
                            {
                                short val;
                                if (!short.TryParse(fields[i].value, out val))
                                    val = 0;
                                instance[index] = val;
                            }
                            else if (cType == typeof(int))
                            {
                                int val;
                                if (!int.TryParse(fields[i].value, out val))
                                    val = 0;
                                instance[index] = val;
                            }
                            else if (cType == typeof(long))
                            {
                                long val;
                                if (!long.TryParse(fields[i].value, out val))
                                    val = 0;
                                instance[index] = val;
                            }
                            else if (cType == typeof(byte))
                            {
                                byte val;
                                if (!byte.TryParse(fields[i].value, out val))
                                    val = 0;
                                instance[index] = val;
                            }
                            else if (cType == typeof(ushort))
                            {
                                ushort val;
                                if (!ushort.TryParse(fields[i].value, out val))
                                    val = 0;
                                instance[index] = val;
                            }
                            else if (cType == typeof(uint))
                            {
                                uint val;
                                if (!uint.TryParse(fields[i].value, out val))
                                    val = 0;
                                instance[index] = val;
                            }
                            else if (cType == typeof(ulong))
                            {
                                ulong val;
                                if (!ulong.TryParse(fields[i].value, out val))
                                    val = 0;
                                instance[index] = val;
                            }
                            else if (cType == typeof(float))
                            {
                                float val;
                                if (!float.TryParse(fields[i].value, out val))
                                    val = 0;
                                instance[index] = val;
                            }
                            else if (cType == typeof(double))
                            {
                                double val;
                                if (!double.TryParse(fields[i].value, out val))
                                    val = 0;
                                instance[index] = val;
                            }
                            else if (cType == typeof(bool))
                            {
                                bool val;
                                if (!bool.TryParse(fields[i].value, out val))
                                    val = false;
                                instance[index] = val;
                            }
                            else if (cType == typeof(char))
                            {
                                char val;
                                if (!char.TryParse(fields[i].value, out val))
                                    val = ' ';
                                instance[index] = val;
                            }
                            //else
                            //throw new System.NotImplementedException();
                        }
                        else
                        {
                            if (!typeof(UnityEngine.Object).IsAssignableFrom(cType))
                            {
                                if (cType == typeof(string))
                                {
                                    instance[index] = fields[i].value;
                                }
                                else if (cType == typeof(UnityEngine.Vector2))
                                {
                                    instance[index] = UnityHelper.ParseVector2(fields[i].value);
                                }
                                else if (cType == typeof(UnityEngine.Vector3))
                                {
                                    instance[index] = UnityHelper.ParseVector3(fields[i].value);
                                }
                                else if (cType == typeof(UnityEngine.Vector4))
                                {
                                    instance[index] = UnityHelper.ParseVector4(fields[i].value);
                                }
                            }
                        }
                    }
                }


                for (int i = 0; i < objFields.Count; ++i)
                {
                    int index;
                    var type = instance.Type.GetField(objFields[i].name, out index);
                    if (type != null)
                    {
                        var cType = type.TypeForCLR;
                        if (!cType.IsPrimitive)
                        {
                            if (typeof(UnityEngine.Object).IsAssignableFrom(cType))
                            {
                                instance[index] = objFields[i];
                            }
                        }
                    }
                }

                var adptor = gameObject.AddComponent<MonoBehaviourAdapter.Adaptor>();
                adptor.enabled = enabled;
                adptor.AppDomain = ILRuntimeManager.app;
                adptor.ILInstance = instance;
                if (gameObject.activeInHierarchy)
                {
                    adptor.Awake();
                    if (adptor.enabled)
                        adptor.OnEnable();
                }
            }
        }

        TimerManager.Instance.AddFarmeTimer(1, delayDestory);
    }

    void delayDestory()
    {
        DestroyImmediate(this);
    }
#if UNITY_EDITOR
    Dictionary<string, int> fieldMap = new Dictionary<string, int>();
    public ILRField GetField(string name)
    {
        ILRField f;
        int index;
        if (!fieldMap.TryGetValue(name, out index))
        {
            index = fields.FindIndex((a) => { return a.name == name; });
            if (index == -1)
            {
                f = new ILRField();
                f.name = name;
                index = fields.Count;
                fields.Add(f);
                fieldMap.Add(name, index);
            }
        }

        f = fields[index];
        return f;
    }
    Dictionary<string, int> objFieldMap = new Dictionary<string, int>();
    public ILRObjField GetObjField(string name)
    {
        ILRObjField f;
        int index;
        if (!objFieldMap.TryGetValue(name, out index))
        {
            index = fields.FindIndex((a) => { return a.name == name; });
            if (index == -1)
            {
                f = new ILRObjField();
                f.name = name;
                index = fields.Count;
                objFields.Add(f);
                objFieldMap.Add(name, index);
            }
        }

        f = objFields[index];
        return f;
    }
#endif
}
