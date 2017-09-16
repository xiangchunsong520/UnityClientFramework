using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Xml;
using System.IO;
using System.Text.RegularExpressions;

public class BMFontCreator : EditorWindow
{
    TextAsset fontData;
    Texture2D fontTexture;
    string fontName;

    [MenuItem("UI/BMFont")]
    static void Build()
    {
        BMFontCreator window = GetWindow(typeof(BMFontCreator)) as BMFontCreator;
        window.titleContent.text = "BMFont Creator";
        window.Show();
    }

    void OnGUI()
    {
        fontData = EditorGUILayout.ObjectField("Font Data", fontData, typeof(TextAsset), false) as TextAsset;
        fontTexture = EditorGUILayout.ObjectField("Texture", fontTexture, typeof(Texture2D), false) as Texture2D;

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Font name");
        fontName = EditorGUILayout.TextField(fontName);
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Create"))
        {
            if (string.IsNullOrEmpty(fontName))
            {
                if (EditorUtility.DisplayDialog("提示", "字体名为空", "ok"))
                {
                    return;
                }
            }

            if (File.Exists(Application.dataPath + "/UI_Fonts/" + fontName + ".fontsettings"))
            {
                if (EditorUtility.DisplayDialog("提示", "已经存在'" + fontName + "'字体,是否替换?", "ok", "cancel"))
                {
                    File.Delete(Application.dataPath + "/UI_Fonts/" + fontName + ".mat");
                    File.Delete(Application.dataPath + "/UI_Fonts/" + fontName + ".fontsettings");
                }
                else
                {
                    return;
                }
            }
            CreateFont();
        }
    }

    void CreateFont()
    {
        Material material = new Material(Shader.Find("UI/Default Font"));
        material.mainTexture = fontTexture;
        AssetDatabase.CreateAsset(material, "Assets/UI_Fonts/" + fontName + ".mat");
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        material = AssetDatabase.LoadAssetAtPath<Material>("Assets/UI_Fonts/" + fontName + ".mat");

        Font font = new Font();
        font.material = material;

        List<CharacterInfo> chtInfoList = GetCharacterInfos();
        font.characterInfo = chtInfoList.ToArray();

        AssetDatabase.CreateAsset(font, "Assets/UI_Fonts/" + fontName + ".fontsettings");
        AssetDatabase.Refresh();
    }

    Vector2 GetTextureSize()
    {
        Vector2 v2 = new Vector2(0, 0);
        Regex regex = new Regex(@"scaleW=(?<width>[\d]+)\s+?scaleH=(?<height>[\d]+)");
        var match = regex.Match(fontData.text);
        if (match.Success)
        {
            if (!string.IsNullOrEmpty(match.Groups["width"].Value))
            {
                int w;
                if (int.TryParse(match.Groups["width"].Value, out w))
                {
                    v2.x = w;
                }
            }
            if (!string.IsNullOrEmpty(match.Groups["height"].Value))
            {
                int h;
                if (int.TryParse(match.Groups["height"].Value, out h))
                {
                    v2.y = h;
                }
            }
        }
        return v2;
    }

    List<CharacterInfo> GetCharacterInfos()
    {
        Vector2 textureSize = GetTextureSize();

        List<CharacterInfo> list = new List<CharacterInfo>();
        Regex regex = new Regex(@"char\s+?id=(?<id>[\d]+)\s+?x=(?<x>[\d]+)\s+?y=(?<y>[\d]+)\s+?width=(?<width>[\d]+)\s+?height=(?<height>[\d]+)\s+?xoffset=(?<xoffset>[\d]+)\s+?yoffset=(?<yoffset>[\d]+)\s+?xadvance=(?<xadvance>[\d]+)\s+?page=(?<page>[\d]+)\s+?chnl=(?<chnl>[\d]+)");
        int index = 0;
        var match = regex.Match(fontData.text, index);
        while (match.Success)
        {
            int id = int.Parse(match.Groups["id"].Value);
            int x = int.Parse(match.Groups["x"].Value);
            int y = int.Parse(match.Groups["y"].Value);
            int width = int.Parse(match.Groups["width"].Value);
            int height = int.Parse(match.Groups["height"].Value);
            int xoffset = int.Parse(match.Groups["xoffset"].Value);
            int yoffset = int.Parse(match.Groups["yoffset"].Value);
            int xadvance = int.Parse(match.Groups["xadvance"].Value);
            int page = int.Parse(match.Groups["page"].Value);
            int chnl = int.Parse(match.Groups["chnl"].Value);

            CharacterInfo chtInfo = new CharacterInfo();
            float texWidth = textureSize.x;
            float texHeight = textureSize.y;

            chtInfo.glyphWidth = (int)texWidth;
            chtInfo.glyphHeight = (int)texHeight;
            chtInfo.index = id;
            
            chtInfo.uvTopLeft = new Vector2((float)x / texWidth, 1 - (float)y / texHeight);
            chtInfo.uvTopRight = new Vector2((float)(x + width) / texWidth, 1 - (float)y / texHeight);
            chtInfo.uvBottomLeft = new Vector2((float)x / texWidth, 1 - (float)(y + height) / texHeight);
            chtInfo.uvBottomRight = new Vector2((float)(x + width) / texWidth, 1 - (float)(y + height) / texHeight);

            chtInfo.minX = xoffset;
            chtInfo.minY = - yoffset - height;
            chtInfo.maxX = xoffset + width;
            chtInfo.maxY = -yoffset;

            chtInfo.advance = xadvance;

            list.Add(chtInfo);

            index = match.Index + match.Length;
            match = regex.Match(fontData.text, index);
        }
        return list;
    }
}
