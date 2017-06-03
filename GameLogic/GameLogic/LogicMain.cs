using LitJson;
using UnityEngine;

namespace GameLogic
{
    public class LogicMain
    {
        public static void Init()
        {
            PBChannel pbChannle = new PBChannel(GameClient.Instance.TcpClient);
            GameClient.Instance.TcpClient.SetPBChannel(pbChannle);
            GameNetHandler.Instance.Init();
            UIManager.Instance.Init();
            UIManager.OpenWindow("LaunchWindow");
        }
    }
}
