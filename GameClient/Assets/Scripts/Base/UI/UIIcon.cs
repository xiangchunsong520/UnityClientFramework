using Base;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIIcon : MonoBehaviour
{
    [HideInInspector]
    public Sprite sprite;

    public static Sprite GetIcon(string path)
    {
#if UNITY_EDITOR && !RECOURCE_CLIENT
        Sprite sp = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI_Icons/" + path + ".png");
        if (sp != null)
            return sp;

        Debugger.LogError(" can't find icon : " + path);
        return null;
#endif
        GameObject go = ResourceLoader.Load<GameObject>("UI/Icons/" + path + ".prefab");
        if (go != null)
        {
            UIIcon icon = go.GetComponent<UIIcon>();
            if (icon != null)
            {
                return icon.sprite;
            }
        }

        return null;
    }
}
