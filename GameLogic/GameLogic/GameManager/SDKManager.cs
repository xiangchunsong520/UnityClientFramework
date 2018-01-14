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

        ChannelConfig _config = null;
        public ChannelConfig Config
        {
            get
            {
                if (_config == null)
                {
                    for (int i = 0; i < DataManager.Instance.channelConfigDatas.Count; ++i)
                    {
                        ChannelConfig cc = DataManager.Instance.channelConfigDatas[i];
                        if (cc.Sdk == _sdk && cc.Platform == _platform && cc.Source == _source)
                        {
                            _config = cc;
                            break;
                        }
                    }
                }

                return _config;
            }
        }

        public string ChannelName
        {
            get
            {
                if (Config == null)
                    return "";

                return Config.ChannelName;
            }
        }

        public bool UpdateInGame
        {
            get
            {
                if (Config == null)
                    return false;

                return Config.UpdateInGame;
            }
        } 

        public string DownloadName
        {
            get
            {
                if (Config == null)
                    return "";

                return Config.DownloadName;
            }
        }

        public void Init()
        {
#if !UNITY_EDITOR
#if UNITY_ANDROID
            _platform = "android";
#elif UNITY_IPHONE
            _platform = "ios";
            _source = "ios";
#endif
#endif
        }
    }
}
