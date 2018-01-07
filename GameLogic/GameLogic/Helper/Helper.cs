/*
auth: Xiang ChunSong
purpose:
*/

using Data;
using System;
using System.Globalization;
using UnityEngine;

namespace GameLogic
{
    public class Helper
    {
        public static string GetLanguage(string id)
        {
            try
            {
                if (DataManager.Instance.languageDatas.ContainsKey(id))
                {
                    Language language = DataManager.Instance.languageDatas.GetUnit(id);
                    string str = language.GetType().GetProperty(LogicMain.language).GetValue(language, null) as string;
                    str = str.Replace("\\n", "\n");
                    return str;
                }
            }
            catch(Exception ex)
            {
                Debugger.LogException(ex);
            }

            return string.Format("<color=#ff0000>{0}</color>", id);
        }

        public static string GetGatewayUrl()
        {
            return DataManager.Instance.clientConfig.Gateways;
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
            UIManager.OpenWindow<MessageBox>(GetLanguage("COMMON_TIP"), content, callback);
        }

        public static void ShowMessageBox(string title, string content, Action callback = null)
        {
            UIManager.OpenWindow<MessageBox>(title, content, callback);
        }

        public static void ShowMessageBox(string content, Action<bool> callback)
        {
            UIManager.OpenWindow<MessageBox>(GetLanguage("COMMON_TIP"), content, callback);
        }

        public static void ShowMessageBox(string title, string content, Action<bool> callback)
        {
            UIManager.OpenWindow<MessageBox>(title, content, callback);
        }

        public static Color PraseColor(string str)
        {
            Color color = Color.white;
            int num;
            if (int.TryParse(str, NumberStyles.HexNumber, null, out num))
            {
                if (str.Length == 8)
                {
                    int f = 0xff;
                    int a = num & f;
                    int b = (num >> 8) & f;
                    int g = (num >> 16) & f;
                    int r = (num >> 24) & f;
                    color.r = (float)r / f;
                    color.g = (float)g / f;
                    color.b = (float)b / f;
                    color.a = (float)a / f;
                }
                else if (str.Length == 6)
                {
                    int f = 0xff;
                    int b = num & f;
                    int g = (num >> 8) & f;
                    int r = (num >> 16) & f;
                    color.r = (float)r / f;
                    color.g = (float)g / f;
                    color.b = (float)b / f;
                }
                else if (str.Length == 4)
                {
                    int f = 0xf;
                    int a = num & f;
                    int b = (num >> 4) & f;
                    int g = (num >> 8) & f;
                    int r = (num >> 12) & f;
                    color.r = (float)r / f;
                    color.g = (float)g / f;
                    color.b = (float)b / f;
                    color.a = (float)a / f;
                }
                else if (str.Length == 3)
                {
                    int f = 0xf;
                    int b = num & f;
                    int g = (num >> 4) & f;
                    int r = (num >> 8) & f;
                    color.r = (float)r / f;
                    color.g = (float)g / f;
                    color.b = (float)b / f;
                }
            }
            return color;
        }
    }
}
