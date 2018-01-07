using System.Collections.Generic;
using UnityEngine;

namespace GameLogic
{
    public class LogicMain : Singleton<LogicMain>
    {
        public static readonly string version = "0.0.1.0";
        public static string language = "Ch";

        public static void Init()
        {
#if UNITY_EDITOR
            Debugger.Log("UNITY_EDITOR");
#endif
#if UNITY_ANDROID
            Debugger.Log("UNITY_ANDROID");
#endif
#if UNITY_IPHONE
            Debugger.Log("UNITY_IPHONE");
#endif
#if UNITY_STANDALONE_WIN
            Debugger.Log("UNITY_STANDALONE_WIN");
#endif
            SystemLanguage sl = Application.systemLanguage;
            switch (sl)
            {
                case SystemLanguage.Chinese:
                case SystemLanguage.ChineseSimplified:
                    language = "Ch";
                    break;
                case SystemLanguage.ChineseTraditional:
                    language = "Cht";
                    break;
                default:
                    language = "En";
                    break;
            }
            Debugger.Log("Language : " + language, true);

            SDKManager.Instance.Init();
            PBChannel pbChannle = new PBChannel(GameClient.Instance.TcpClient);
            GameClient.Instance.TcpClient.SetPBChannel(pbChannle);
            GameNetHandler.Instance.Init();
            UIManager.Instance.Init();
            UIManager.OpenWindow<LaunchWindow>();
        }

        public static void Update()
        {

        }
    }
}
