using Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameLogic
{
    class SDKManager : Singleton<SDKManager>
    {
        string _sdk = "nosdk";
        string _platform = "win";
        string _source = "standard";

        ChannelConfig config = null;

        public string ChannelName
        {
            get
            {
                if (config == null)
                {
                    for (int i = 0; i < DataManager.Instance.channelConfigDatas.Count; ++i)
                    {
                        ChannelConfig cc = DataManager.Instance.channelConfigDatas[i];
                        if (cc.Sdk == _sdk && cc.Platform == _platform && cc.Source == _source)
                        {
                            config = cc;
                            break;
                        }
                    }
                }

                if (config == null)
                    return "";

                return config.ChannelName;
            }
        }

        public void Init()
        {

        }
    }
}
