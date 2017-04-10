using UnityEngine;

namespace GameLogic
{
    public class LogicMain
    {
        public static readonly string VersionCode = "0.1.0";

        public static void Init()
        {
            GameObject go = new GameObject("aaaaa");
            go.AddComponent<TestMono>();

            var t = go.GetComponent<TestMono>();
            Debugger.LogError(t);

            PBChannel pbChannle = new PBChannel(GameClient.Instance.TcpClient);
            GameClient.Instance.TcpClient.SetPBChannel(pbChannle);
            GameNetHandler.Instance.Init();
            UIManager.Instance.Init();
            UIManager.OpenWindow("LaunchWindow");
        }
    }

    public class TestMono : UnityEngine.MonoBehaviour
    {
        public int a = 19;
        public static float test;
        int last;

        void Awake()
        {
            a = 19;
            Debugger.LogError("TestMono:Awake " + a);
        }

        void Start()
        {
            Debugger.LogError("TestMono:Start " + a);
            last = a;
        }

        void Update()
        {
            if (last != a)
            {
                Debugger.Log(last + " --> " + a);
                last = a;
            }
        }
    }
}
