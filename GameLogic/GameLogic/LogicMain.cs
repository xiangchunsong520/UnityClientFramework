using System.Collections.Generic;

namespace GameLogic
{
    public class LogicMain : Singleton<LogicMain>
    {
        public List<string> ips = new List<string>();
        public List<int> ports = new List<int>();
        public bool isShenHe;

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
