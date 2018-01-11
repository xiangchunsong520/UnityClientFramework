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
        bool b = gameObject.activeInHierarchy;
        if (b)
        {
            gameObject.SetActive(false);
        }

#if (UNITY_EDITOR && !DISABLE_ILRUNTIME) || (!UNITY_EDITOR && !UNITY_STANDALONE_WIN) || FOCE_ENABLE_ILRUNTIME
        IType type = ILRuntimeManager.GetScriptType(script);
        if (type != null)
        {
            ILTypeInstance instance = new ILTypeInstance(type as ILType, false); ;
            if (instance != null)
            {
                for (int i = 0; i < fields.Count; ++i)
                {
                    int index;
                    var field = instance.Type.GetField(fields[i].name, out index);
                    if (field != null)
                    {
                        var fieldType = field.TypeForCLR;
                        if (fieldType.IsPrimitive)
                        {
                            if (fieldType == typeof(sbyte))
                            {
                                sbyte val;
                                if (!sbyte.TryParse(fields[i].value, out val))
                                    val = 0;
                                instance[index] = val;
                            }
                            else if (fieldType == typeof(short))
                            {
                                short val;
                                if (!short.TryParse(fields[i].value, out val))
                                    val = 0;
                                instance[index] = val;
                            }
                            else if (fieldType == typeof(int))
                            {
                                int val;
                                if (!int.TryParse(fields[i].value, out val))
                                    val = 0;
                                instance[index] = val;
                            }
                            else if (fieldType == typeof(long))
                            {
                                long val;
                                if (!long.TryParse(fields[i].value, out val))
                                    val = 0;
                                instance[index] = val;
                            }
                            else if (fieldType == typeof(byte))
                            {
                                byte val;
                                if (!byte.TryParse(fields[i].value, out val))
                                    val = 0;
                                instance[index] = val;
                            }
                            else if (fieldType == typeof(ushort))
                            {
                                ushort val;
                                if (!ushort.TryParse(fields[i].value, out val))
                                    val = 0;
                                instance[index] = val;
                            }
                            else if (fieldType == typeof(uint))
                            {
                                uint val;
                                if (!uint.TryParse(fields[i].value, out val))
                                    val = 0;
                                instance[index] = val;
                            }
                            else if (fieldType == typeof(ulong))
                            {
                                ulong val;
                                if (!ulong.TryParse(fields[i].value, out val))
                                    val = 0;
                                instance[index] = val;
                            }
                            else if (fieldType == typeof(float))
                            {
                                float val;
                                if (!float.TryParse(fields[i].value, out val))
                                    val = 0;
                                instance[index] = val;
                            }
                            else if (fieldType == typeof(double))
                            {
                                double val;
                                if (!double.TryParse(fields[i].value, out val))
                                    val = 0;
                                instance[index] = val;
                            }
                            else if (fieldType == typeof(bool))
                            {
                                bool val;
                                if (!bool.TryParse(fields[i].value, out val))
                                    val = false;
                                instance[index] = val;
                            }
                            else if (fieldType == typeof(char))
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
                            if (!typeof(UnityEngine.Object).IsAssignableFrom(fieldType))
                            {
                                if (fieldType == typeof(string))
                                {
                                    instance[index] = fields[i].value;
                                }
                                else if (fieldType == typeof(UnityEngine.Vector2))
                                {
                                    instance[index] = UnityHelper.ParseVector2(fields[i].value);
                                }
                                else if (fieldType == typeof(UnityEngine.Vector3))
                                {
                                    instance[index] = UnityHelper.ParseVector3(fields[i].value);
                                }
                                else if (fieldType == typeof(UnityEngine.Vector4))
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
                    var field = instance.Type.GetField(objFields[i].name, out index);
                    if (field != null)
                    {
                        var fieldType = field.TypeForCLR;
                        if (!fieldType.IsPrimitive)
                        {
                            if (typeof(UnityEngine.Object).IsAssignableFrom(fieldType))
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
            }
        }
#else
        Type type = ILRuntimeManager.assembly.GetType(script);
        Component instance = gameObject.AddComponent(type);

        if (instance != null)
        {
            for (int i = 0; i < fields.Count; ++i)
            {
                var field = type.GetField(fields[i].name);
                if (field != null)
                {
                    var fieldType = field.FieldType;
                    if (fieldType.IsPrimitive)
                    {
                        if (fieldType == typeof(sbyte))
                        {
                            sbyte val;
                            if (!sbyte.TryParse(fields[i].value, out val))
                                val = 0;
                            field.SetValue(instance, val);
                        }
                        else if (fieldType == typeof(short))
                        {
                            short val;
                            if (!short.TryParse(fields[i].value, out val))
                                val = 0;
                            field.SetValue(instance, val);
                        }
                        else if (fieldType == typeof(int))
                        {
                            int val;
                            if (!int.TryParse(fields[i].value, out val))
                                val = 0;
                            field.SetValue(instance, val);
                        }
                        else if (fieldType == typeof(long))
                        {
                            long val;
                            if (!long.TryParse(fields[i].value, out val))
                                val = 0;
                            field.SetValue(instance, val);
                        }
                        else if (fieldType == typeof(byte))
                        {
                            byte val;
                            if (!byte.TryParse(fields[i].value, out val))
                                val = 0;
                            field.SetValue(instance, val);
                        }
                        else if (fieldType == typeof(ushort))
                        {
                            ushort val;
                            if (!ushort.TryParse(fields[i].value, out val))
                                val = 0;
                            field.SetValue(instance, val);
                        }
                        else if (fieldType == typeof(uint))
                        {
                            uint val;
                            if (!uint.TryParse(fields[i].value, out val))
                                val = 0;
                            field.SetValue(instance, val);
                        }
                        else if (fieldType == typeof(ulong))
                        {
                            ulong val;
                            if (!ulong.TryParse(fields[i].value, out val))
                                val = 0;
                            field.SetValue(instance, val);
                        }
                        else if (fieldType == typeof(float))
                        {
                            float val;
                            if (!float.TryParse(fields[i].value, out val))
                                val = 0;
                            field.SetValue(instance, val);
                        }
                        else if (fieldType == typeof(double))
                        {
                            double val;
                            if (!double.TryParse(fields[i].value, out val))
                                val = 0;
                            field.SetValue(instance, val);
                        }
                        else if (fieldType == typeof(bool))
                        {
                            bool val;
                            if (!bool.TryParse(fields[i].value, out val))
                                val = false;
                            field.SetValue(instance, val);
                        }
                        else if (fieldType == typeof(char))
                        {
                            char val;
                            if (!char.TryParse(fields[i].value, out val))
                                val = ' ';
                            field.SetValue(instance, val);
                        }
                        //else
                        //throw new System.NotImplementedException();
                    }
                    else
                    {
                        if (!typeof(UnityEngine.Object).IsAssignableFrom(fieldType))
                        {
                            if (fieldType == typeof(string))
                            {
                                field.SetValue(instance, fields[i].value);
                            }
                            else if (fieldType == typeof(UnityEngine.Vector2))
                            {
                                field.SetValue(instance, UnityHelper.ParseVector2(fields[i].value));
                            }
                            else if (fieldType == typeof(UnityEngine.Vector3))
                            {
                                field.SetValue(instance, UnityHelper.ParseVector2(fields[i].value));
                            }
                            else if (fieldType == typeof(UnityEngine.Vector4))
                            {
                                field.SetValue(instance, UnityHelper.ParseVector2(fields[i].value));
                            }
                        }
                    }
                }
            }


            for (int i = 0; i < objFields.Count; ++i)
            {
                var field = type.GetField(objFields[i].name);
                if (field != null)
                {
                    var fieldType = field.FieldType;
                    if (!fieldType.IsPrimitive)
                    {
                        if (typeof(UnityEngine.Object).IsAssignableFrom(fieldType))
                        {
                            field.SetValue(instance, objFields[i]);
                        }
                    }
                }
            }

            type.GetProperty("enabled").SetValue(instance, enabled, null);
        }
#endif
        if (b)
        {
            gameObject.SetActive(true);
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
