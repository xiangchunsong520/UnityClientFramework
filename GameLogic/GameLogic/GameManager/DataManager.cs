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
        public ClientConfig clientConfig;
        DataVector<ClientConfig> _clientConfigDatas = new DataVector<ClientConfig>();
        public DataHash<Language> languageDatas = new DataHash<Language>();
        public DataVector<ChannelConfig> channelConfigDatas = new DataVector<ChannelConfig>();

        List<DataLoaderBase> clientConfigs = new List<DataLoaderBase>();

        //------------------------------------------server config------------------------------------------

        List<DataLoaderBase> serverConfigs = new List<DataLoaderBase>();

        public DataManager()
        {
            clientConfigs.Add(_clientConfigDatas);
            clientConfigs.Add(languageDatas);
            clientConfigs.Add(channelConfigDatas);

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
            clientConfig = _clientConfigDatas[0];
            w.Stop();
            Debugger.Log("All client config load finish. Use time : " + w.ElapsedMilliseconds + " ms", true);
            return true;
        }
    }
}