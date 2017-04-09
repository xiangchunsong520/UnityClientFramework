

public class UnityDefine
{
    public static bool UnityEditor
    {
        get
        {
#if UNITY_EDITOR
            return true;
#endif
            return false;
        }
    }

    public static bool UnityAndroid
    {
        get
        {
#if UNITY_ANDROID
            return true;
#endif
            return false;
        }
    }

    public static bool UnityIOS
    {
        get
        {
#if UNITY_IPHONE
            return true;
#endif
            return false;
        }
    }

    public static bool UnityWindows
    {
        get
        {
#if UNITY_STANDALONE_WIN
            return true;
#endif
            return false;
        }
    }
}