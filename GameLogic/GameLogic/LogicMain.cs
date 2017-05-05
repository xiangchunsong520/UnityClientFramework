using LitJson;
using UnityEngine;

namespace GameLogic
{
    public class LogicMain
    {
        public static readonly string VersionCode = "0.1.0";

        public static void Init()
        {
            PBChannel pbChannle = new PBChannel(GameClient.Instance.TcpClient);
            GameClient.Instance.TcpClient.SetPBChannel(pbChannle);
            GameNetHandler.Instance.Init();
            UIManager.Instance.Init();
            UIManager.OpenWindow("LaunchWindow");

            /*TestJson tj = new TestJson();
            tj.id = 1234;
            tj.name = "hahaha";

            string str = JsonMapper.ToJson(tj);
            Debugger.Log(str);

            TestJson outTj = JsonMapper.ToObject<TestJson>(str);
            Debugger.Log(outTj.id);
            Debugger.Log(outTj.name);*/
        }
    }

    class TestJson
    {
        public int id;
        public string name;
    }
}
