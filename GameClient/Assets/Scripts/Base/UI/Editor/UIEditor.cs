#if !RECOURCE_CLIENT
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Yaml.Serialization;

public class UIEditor : Editor
{
    static readonly string metastring =
@"fileFormatVersion: 2
guid: #GUID#
timeCreated: #TC#
licenseType: Pro
TextureImporter:
  fileIDToRecycleName: {}
  serializedVersion: 4
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
    sRGBTexture: 1
    linearTexture: 0
    fadeOut: 0
    borderMipMap: 0
    mipMapFadeDistanceStart: 1
    mipMapFadeDistanceEnd: 3
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapFilter: 0
  isReadable: 0
  grayScaleToAlpha: 0
  generateCubemap: 6
  cubemapConvolution: 0
  seamlessCubemap: 0
  textureFormat: 1
  maxTextureSize: 2048
  textureSettings:
    filterMode: -1
    aniso: -1
    mipBias: -1
    wrapMode: #WM#
  nPOTScale: 0
  lightmap: 0
  compressionQuality: 50
  spriteMode: 1
  spriteExtrude: 1
  spriteMeshType: 1
  alignment: 0
  spritePivot: {x: #SPX#, y: #SPY#}
  spriteBorder: {x: #SBX#, y: #SBY#, z: #SBZ#, w: #SBW#}
  spritePixelsToUnits: 100
  alphaUsage: 1
  alphaIsTransparency: 1
  spriteTessellationDetail: -1
  textureType: 8
  textureShape: 1
  maxTextureSizeSet: 0
  compressionQualitySet: 0
  textureFormatSet: 0
  platformSettings:
  - buildTarget: DefaultTexturePlatform
    maxTextureSize: 2048
    textureFormat: -1
    textureCompression: 1
    compressionQuality: 50
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: 0
  - buildTarget: Standalone
    maxTextureSize: 2048
    textureFormat: 12
    textureCompression: 1
    compressionQuality: 50
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: 1
  - buildTarget: iPhone
    maxTextureSize: 2048
    textureFormat: 33
    textureCompression: 1
    compressionQuality: 50
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: 1
  - buildTarget: Android
    maxTextureSize: 2048
    textureFormat: 34
    textureCompression: 1
    compressionQuality: 50
    crunchedCompression: 0
    allowsAlphaSplitting: 1
    overridden: 1
  spriteSheet:
    serializedVersion: 2
    sprites: []
    outline: []
  spritePackingTag: #PT#
  userData: 
  assetBundleName: #AN#
  assetBundleVariant: #AV#
"; 

    //[MenuItem("UI/Create Atlas")]
    public static void CreateUIAtlas()
    {
        string rootFloder = Application.dataPath + "/UI_Atlas/";
        if (!Directory.Exists(rootFloder))
            return;

        string targetFloder = Application.dataPath + "/Resources/UI/Atlas/";
        if (!Directory.Exists(targetFloder))
            Directory.CreateDirectory(targetFloder);

        List<string> lastnames = new List<string>();
        DirectoryInfo last = new DirectoryInfo(targetFloder);
        FileInfo[] pfbs = last.GetFiles("*.prefab", SearchOption.AllDirectories);
        foreach(FileInfo pfb in pfbs)
        {
            string name = pfb.FullName.Substring(targetFloder.Length);
            name = name.Substring(0, name.IndexOf("."));
            name = name.Replace("\\", "/");
            lastnames.Add(name);
        }

        DirectoryInfo dir = new DirectoryInfo(rootFloder);
        DirectoryInfo[] childs = dir.GetDirectories("*", SearchOption.AllDirectories);
        foreach (DirectoryInfo child in childs)
        {
            string subpath = child.FullName.Substring(Application.dataPath.IndexOf("Assets"));
            subpath = subpath.Replace("\\", "/");
            subpath = subpath.Substring("Assets/UI_Atlas/".Length);
            lastnames.Remove(subpath);

            string floder = Path.GetDirectoryName(targetFloder + subpath);
            if (!Directory.Exists(floder))
                Directory.CreateDirectory(floder);

            string atlasName = "Atlas/" + subpath;

            List<Sprite> sprites = new List<Sprite>();
            FileInfo[] files = child.GetFiles("*.png");
            foreach (FileInfo file in files)
            {
                string old = File.ReadAllText(file.FullName + ".meta");
                YamlSerializer serializer = new YamlSerializer();
                object[] rsl = serializer.Deserialize(old);
                Dictionary<object, object> settings = rsl[0] as Dictionary<object, object>;
                string createTime = settings["timeCreated"].ToString();
                Dictionary<object, object> TextureImporter = settings["TextureImporter"] as Dictionary<object, object>;
                string wm = "1";
                string spx = "0";
                string spy = "0";
                string sbx = "0";
                string sby = "0";
                string sbz = "0";
                string sbw = "0";
                string an = "";
                string av = "";
                if (TextureImporter != null)
                {
                    int textureType = (int)TextureImporter["textureType"];
                    if (textureType == 8)
                    {
                        Dictionary<object, object> sp = TextureImporter["spritePivot"] as Dictionary<object, object>;
                        spx = sp["x"].ToString();
                        spy = sp[true].ToString();

                        Dictionary<object, object> sb = TextureImporter["spriteBorder"] as Dictionary<object, object>;
                        sbx = sb["x"].ToString();
                        sby = sb[true].ToString();
                        sbz = sb["z"].ToString();
                        sbw = sb["w"].ToString();

                        Dictionary<object, object> textureSettings = TextureImporter["textureSettings"] as Dictionary<object, object>;
                        wm = textureSettings["wrapMode"].ToString();
                    }
                    an = TextureImporter["assetBundleName"] == null ? "" : TextureImporter["assetBundleName"].ToString();
                    av = TextureImporter["assetBundleVariant"] == null ? "" : TextureImporter["assetBundleVariant"].ToString();
                }

                string guid = AssetDatabase.AssetPathToGUID("Assets/UI_Atlas/" + subpath + "/" + file.Name);
                string str = metastring.Replace("#GUID#", guid);
                str = str.Replace("#TC#", createTime);
                str = str.Replace("#WM#", wm);
                str = str.Replace("#SPX#", spx);
                str = str.Replace("#SPY#", spy);
                str = str.Replace("#SBX#", sbx);
                str = str.Replace("#SBY#", sby);
                str = str.Replace("#SBZ#", sbz);
                str = str.Replace("#SBW#", sbw);
                str = str.Replace("#PT#", atlasName);
                str = str.Replace("#AN#", an);
                str = str.Replace("#AV#", av);
                File.WriteAllText(file.FullName + ".meta", str);
                AssetDatabase.Refresh();
                Object[] objs = AssetDatabase.LoadAllAssetsAtPath("Assets/UI_Atlas/" + subpath + "/" + file.Name);
                foreach (Object obj in objs)
                {
                    if (obj.GetType() == typeof(Sprite))
                    {
                        sprites.Add(obj as Sprite);
                    }
                }
            }
            
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/UI/Atlas/" + subpath + ".prefab");
            if (go == null)
            {
                go = new GameObject();
                UIAtlas atlas = go.AddComponent<UIAtlas>();
                atlas.SetSprites(sprites);
                PrefabUtility.CreatePrefab("Assets/Resources/UI/Atlas/" + subpath + ".prefab", go);
                DestroyImmediate(go);
            }
            else
            {
                GameObject go2 = PrefabUtility.InstantiatePrefab(go) as GameObject;
                UIAtlas atlas = go2.GetComponent<UIAtlas>();
                if (atlas == null)
                {
                    atlas = go2.AddComponent<UIAtlas>();
                    atlas.SetSprites(sprites);
                    PrefabUtility.ReplacePrefab(go2, go, ReplacePrefabOptions.ConnectToPrefab);
                }
                else
                {
                    bool b = false;
                    for (int i = 0; i < sprites.Count; ++i)
                    {
                        if (sprites[i] != null && atlas[sprites[i].name] != sprites[i])
                        {
                            b = true;
                            break;
                        }
                    }
                    if (b || sprites.Count != atlas.Count)
                    {
                        atlas.SetSprites(sprites);
                        PrefabUtility.ReplacePrefab(go2, go, ReplacePrefabOptions.ConnectToPrefab);
                    }
                }
                DestroyImmediate(go2);
            }
        }

        foreach (string str in lastnames)
        {
            File.Delete(targetFloder + str + ".prefab");
        }

        AssetDatabase.Refresh();
    }

    //[MenuItem("UI/Create Icons")]
    public static void CreateUIIcons()
    {
        string rootFloder = Application.dataPath + "/UI_Icons/";
        if (!Directory.Exists(rootFloder))
            return;

        string targetFloder = Application.dataPath + "/Resources/UI/Icons/";
        if (!Directory.Exists(targetFloder))
            Directory.CreateDirectory(targetFloder);

        List<string> lastnames = new List<string>();
        DirectoryInfo last = new DirectoryInfo(targetFloder);
        FileInfo[] pfbs = last.GetFiles("*.prefab", SearchOption.AllDirectories);
        foreach (FileInfo pfb in pfbs)
        {
            string name = pfb.FullName.Substring(targetFloder.Length);
            name = name.Substring(0, name.IndexOf("."));
            name = name.Replace("\\", "/");
            lastnames.Add(name);
        }

        DirectoryInfo dir = new DirectoryInfo(rootFloder);
        FileInfo[] files = dir.GetFiles("*.png", SearchOption.AllDirectories);
        foreach (FileInfo file in files)
        {
            string assetPath = file.FullName.Substring(Application.dataPath.IndexOf("Assets"));
            assetPath = assetPath.Replace("\\", "/");
            string subpath = assetPath.Substring("Assets/UI_Icons/".Length);
            subpath = subpath.Substring(0, subpath.IndexOf("."));
            lastnames.Remove(subpath);

            string old = File.ReadAllText(file.FullName + ".meta");
            YamlSerializer serializer = new YamlSerializer();
            object[] rsl = serializer.Deserialize(old);
            Dictionary<object, object> settings = rsl[0] as Dictionary<object, object>;
            string createTime = settings["timeCreated"].ToString();
            Dictionary<object, object> TextureImporter = settings["TextureImporter"] as Dictionary<object, object>;
            string an = "";
            string av = "";
            if (TextureImporter != null)
            {
                an = TextureImporter["assetBundleName"] == null ? "" : TextureImporter["assetBundleName"].ToString();
                av = TextureImporter["assetBundleVariant"] == null ? "" : TextureImporter["assetBundleVariant"].ToString();
            }

            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            string tagName = "Icons/" + subpath;
            string meta = metastring.Replace("#GUID#", guid);
            meta = meta.Replace("#TC#", createTime);
            meta = meta.Replace("#WM#", "1");
            meta = meta.Replace("#SPX#", "0");
            meta = meta.Replace("#SPY#", "0");
            meta = meta.Replace("#SBX#", "0");
            meta = meta.Replace("#SBY#", "0");
            meta = meta.Replace("#SBZ#", "0");
            meta = meta.Replace("#SBW#", "0");
            meta = meta.Replace("#PT#", tagName);
            meta = meta.Replace("#AN#", an);
            meta = meta.Replace("#AV#", av);
            File.WriteAllText(file.FullName + ".meta", meta);
            AssetDatabase.Refresh();

            string floder = Path.GetDirectoryName(targetFloder + subpath);
            if (!Directory.Exists(floder))
                Directory.CreateDirectory(floder);

            string pfbName = "Assets/Resources/UI/Icons/" + subpath + ".prefab";
            Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);

            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(pfbName);
            if (go == null)
            {
                go = new GameObject();
                UIIcon icon = go.AddComponent<UIIcon>();
                icon.sprite = sp;
                PrefabUtility.CreatePrefab(pfbName, go);
                DestroyImmediate(go);
            }
            else
            {
                GameObject go2 = PrefabUtility.InstantiatePrefab(go) as GameObject;
                UIIcon icon = go2.GetComponent<UIIcon>();
                if (icon == null)
                {
                    icon = go2.AddComponent<UIIcon>();
                    icon.sprite = sp;
                    PrefabUtility.ReplacePrefab(go2, go, ReplacePrefabOptions.ConnectToPrefab);
                }
                else if (icon.sprite != sp)
                {
                    icon.sprite = sp;
                    PrefabUtility.ReplacePrefab(go2, go, ReplacePrefabOptions.ConnectToPrefab);
                }
                DestroyImmediate(go2);
            }
        }

        foreach (string str in lastnames)
        {
            File.Delete(targetFloder + str + ".prefab");
        }

        AssetDatabase.Refresh();
    }
}
#endif