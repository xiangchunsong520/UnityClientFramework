/*
auth: Xiang ChunSong
purpose:
*/

using System;

namespace GameLogic
{
    public class Helper
    {
        public static string GetLanguage(int id)
        {
            if (DataManager.Instance.languageDatas.ContainsKey(id))
            {
                string str = DataManager.Instance.languageDatas.GetUnit(id).Text;
                str = str.Replace("\\n", "\n");
                return str;
            }
            return string.Format("[ff0000]id:{0}[-]", id);
        }

        public static string GetGatewayUrl()
        {
            return DataManager.Instance.clientConfig.Gateways;
        }

        public static string GetVersion()
        {
            return DataManager.Instance.clientConfig.Version;
        }

        public static string GetChannelName()
        {
            return SDKManager.Instance.ChannelName;
        }

        public static bool GetUpdateInGame()
        {
            return SDKManager.Instance.UpdateInGame;
        }

        public static string GetDownladName()
        {
            return SDKManager.Instance.DownloadName;
        }

        public static void ShowMessageBox(string content, Action callback = null)
        {
            UIManager.OpenWindow<MessageBox>(GetLanguage(3), content, callback);
        }

        public static void ShowMessageBox(string title, string content, Action callback = null)
        {
            UIManager.OpenWindow<MessageBox>(title, content, callback);
        }

        public static void ShowMessageBox(string content, Action<bool> callback)
        {
            UIManager.OpenWindow<MessageBox>(GetLanguage(3), content, callback);
        }

        public static void ShowMessageBox(string title, string content, Action<bool> callback)
        {
            UIManager.OpenWindow<MessageBox>(title, content, callback);
        }
    }
}
