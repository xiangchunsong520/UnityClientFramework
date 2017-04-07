/*
auth: Xiang ChunSong
purpose:
*/

using Data;
using System;
using System.Collections.Generic;

namespace GameLogic
{
    public class DataManager : Singleton<DataManager>
    {
        public DataHash<ClientConfig> clientConfigDatas = new DataHash<ClientConfig>();
        public DataHash<Language> languageDatas = new DataHash<Language>();
        public DataHash<WindowConfig> windowConfigDatas = new DataHash<WindowConfig>("WinName");

        List<DataLoaderBase> clientConfigs = new List<DataLoaderBase>();

        public DataManager()
        {
            clientConfigs.Add(clientConfigDatas);
            clientConfigs.Add(languageDatas);
            clientConfigs.Add(windowConfigDatas);

            LoadClientData();
        }

        public bool LoadClientData()
        {
            for (int i = 0; i < clientConfigs.Count; ++i)
            {
                System.Diagnostics.Stopwatch w = new System.Diagnostics.Stopwatch();
                w.Start();
                if (!clientConfigs[i].Load())
                {
                    throw new Exception("The config : " + clientConfigs[i].ToString() + " load fail !");
                }
                w.Stop();
                Debugger.Log("load config : " + clientConfigs[i].ToString() + " finish. Use time : " + w.ElapsedMilliseconds);
            }
            return true;
        }
    }
}