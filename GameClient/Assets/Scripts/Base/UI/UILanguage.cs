using Base;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UI/UILanguage", 1)]
public class UILanguage : MonoBehaviour
{
    public string id;

    void Awake()
    {
        string str = ILRuntimeHelper.GetLanguage(id);
        Text text = GetComponent<Text>();
        if (text != null)
        {
            text.text = str;
            return;
        }

        InputField input = GetComponent<InputField>();
        if (input != null)
        {
            input.text = str;
            return;
        }

        Image img = GetComponent<Image>();
        if (img != null)
        {
            try
            {
                string[] strs = str.Split(':');
                img.sprite = UIAtlas.GetSprite(strs[0], strs[1]);
                img.SetNativeSize();
                return;
            }
            catch(Exception ex)
            {
                Debugger.LogException(ex);
            }
        }

        RawImage rawImg = GetComponent<RawImage>();
        if (rawImg != null)
        {
            rawImg.texture = ResourceLoader.Load<Texture>(str);
            rawImg.SetNativeSize();
            return;
        }
    }
}
