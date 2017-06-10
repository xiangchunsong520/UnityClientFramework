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
            SDKManager.Instance.Init();
            PBChannel pbChannle = new PBChannel(GameClient.Instance.TcpClient);
            GameClient.Instance.TcpClient.SetPBChannel(pbChannle);
            GameNetHandler.Instance.Init();
            UIManager.Instance.Init();
            UIManager.OpenWindow("LaunchWindow");
        }
    }
}
