using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Base;
using System.IO;

public class UIAtlas : MonoBehaviour
{
    [SerializeField]
    [HideInInspector]
    List<Sprite> sprites = new List<Sprite>();
    Dictionary<string, Sprite> dic = new Dictionary<string, Sprite>();
    bool hasinit = false;

    public int Count
    {
        get
        {
            return sprites.Count;
        }
    }

    public Sprite this[string spriteName]
    {
        get
        {
            if (!hasinit)
                Init();

            if (!dic.ContainsKey(spriteName))
                return null;

            return dic[spriteName];
        }
    }

    public static Sprite GetSprite(string atlasName, string spriteName)
    {
#if UNITY_EDITOR && !RECOURCE_CLIENT
        Object[] objs = UnityEditor.AssetDatabase.LoadAllAssetsAtPath("Assets/UI_Atlas/" + atlasName + "/" + spriteName + ".png");
        for (int i = 0; i < objs.Length; ++i)
        {
            if (objs[i].GetType() == typeof(Sprite) && objs[i].name == spriteName)
            {
                return objs[i] as Sprite;
            }
        }

        DirectoryInfo dirInfo = new DirectoryInfo("Assets/UI_Atlas/" + atlasName + "/");
        FileInfo[] files = dirInfo.GetFiles("*.png");
        for (int i = 0; i < files.Length; ++i)
        {
            if (Path.GetFileNameWithoutExtension(files[i].Name) == spriteName)
                continue;

            objs = UnityEditor.AssetDatabase.LoadAllAssetsAtPath("Assets/UI_Atlas/" + atlasName + "/" + files[i].Name);
            for (int j = 0; j < objs.Length; ++j)
            {
                if (objs[j].GetType() == typeof(Sprite) && objs[j].name == spriteName)
                {
                    return objs[j] as Sprite;
                }
            }
        }

        Debugger.LogError("the atlas : " + atlasName + " can't find sprite : " + spriteName);
        return null;
#endif
        GameObject go = ResourceLoader.Load<GameObject>("UI/Atlas/" + atlasName + ".prefab");
        if (go != null)
        {
            UIAtlas atlas = go.GetComponent<UIAtlas>();
            if (atlas != null)
            {
                return atlas[spriteName];
            }
        }

        return null;
    }

    void Awake()
    {
        if (!hasinit)
            Init();
    }

    void Init()
    {

        for (int i = 0; i < sprites.Count; ++i)
        {
            if (sprites[i] != null)
            {
                dic.Add(sprites[i].name, sprites[i]);
            }
        }
        hasinit = true;
    }
#if UNITY_EDITOR
    public void SetSprites(List<Sprite> list)
    {
        sprites.Clear();
        sprites.AddRange(list);
    }
#endif
}
