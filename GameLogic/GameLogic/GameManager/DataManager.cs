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
        //------------------------------------------client config------------------------------------------
        public DataSingle<ClientConfig> clientConfig = new DataSingle<ClientConfig>();
        public DataVector<ChannelConfig> channelConfigDatas = new DataVector<ChannelConfig>();
        public DataHash<Language> languageDatas = new DataHash<Language>();
        public DataHash<Map> mapDatas = new DataHash<Map>("Name");
        public DataHash<Actor> actorDatas = new DataHash<Actor>("Name");
        public DataHash<Recipe> recipeDatas = new DataHash<Recipe>("Source");

        List<DataLoaderBase> clientConfigs = new List<DataLoaderBase>();

        //------------------------------------------server config------------------------------------------

        List<DataLoaderBase> serverConfigs = new List<DataLoaderBase>();

        public DataManager()
        {
            clientConfigs.Add(clientConfig);
            clientConfigs.Add(channelConfigDatas);
            clientConfigs.Add(languageDatas);
            clientConfigs.Add(mapDatas);
            clientConfigs.Add(actorDatas);
            clientConfigs.Add(recipeDatas);

            LoadClientData();
        }

        public bool LoadClientData()
        {
            System.Diagnostics.Stopwatch w = new System.Diagnostics.Stopwatch();
            w.Start();

            for (int i = 0; i < clientConfigs.Count; ++i)
            {
                if (!clientConfigs[i].Load())
                {
                    string name = clientConfigs[i].ToString();
                    int pos = name.LastIndexOf(".") + 1;
                    name = name.Substring(pos, name.LastIndexOf(">") - pos);
                    throw new Exception("The config : " + name + " load fail !");
                }
            }

            w.Stop();
            Debugger.Log("All client config load finish. Use time : " + w.ElapsedMilliseconds + " ms", true);
            return true;
        }
    }
}