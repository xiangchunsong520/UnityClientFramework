using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class UnityHelper
{
    public static string Vector2ToString(Vector2 v2)
    {
        return string.Format("{0},{1}", v2.x, v2.y);
    }

    public static Vector2 ParseVector2(string str)
    {
        if (string.IsNullOrEmpty(str))
            return Vector2.zero;

        str = str.Replace(" ", "");
        string[] strs = str.Split(',');
        if (strs.Length == 2)
        {
            float x, y;
            if (float.TryParse(strs[0], out x) && float.TryParse(strs[1], out y))
            {
                return new Vector2(x, y);
            }
        }
        return Vector2.zero;
    }

    public static string Vector3ToString(Vector3 v3)
    {
        return string.Format("{0},{1},{2}", v3.x, v3.y, v3.z);
    }

    public static Vector3 ParseVector3(string str)
    {
        if (string.IsNullOrEmpty(str))
            return Vector3.zero;

        str = str.Replace(" ", "");
        string[] strs = str.Split(',');
        if (strs.Length == 3)
        {
            float x, y, z;
            if (float.TryParse(strs[0], out x) && float.TryParse(strs[1], out y) && float.TryParse(strs[2], out z))
            {
                return new Vector3(x, y, z);
            }
        }
        return Vector3.zero;
    }

    public static string Vector4ToString(Vector4 v4)
    {
        return string.Format("{0},{1},{2},{3}", v4.x, v4.y, v4.z, v4.w);
    }

    public static Vector4 ParseVector4(string str)
    {
        if (string.IsNullOrEmpty(str))
            return Vector4.zero;

        str = str.Replace(" ", "");
        string[] strs = str.Split(',');
        if (strs.Length == 4)
        {
            float x, y, z, w;
            if (float.TryParse(strs[0], out x) && float.TryParse(strs[1], out y) && float.TryParse(strs[2], out z) && float.TryParse(strs[3], out w))
            {
                return new Vector4(x, y, z, w);
            }
        }
        return Vector4.zero;
    }
}
