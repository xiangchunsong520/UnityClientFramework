using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Utils;

namespace Base
{
    public enum UpdateState
    {
        CheckNetWork,
        DownloadGateway,
        ComparServerVersion,
        DownloadResourceList,
        ComparServerResource,
        DownloadServerResource,
        CheckLoacalResource,
        DownloadPatchConfig,
        OpenNewClientUrl,
        DownloadNewClient,
        DownloadNewClientPatch,
        GenerateNewClient,
        InstallNewClient,
        UpdateFinish,
    }

    public class UpdateStep
    {
        public UpdateState State;
        public Action CallBack;
        public bool showTip;
        public object arg;
    }

    public class UpdateProgress
    {
        public float Progreess;
        public long TotalSize;
    }

    public class GatewayConfig
    {
        public class GameVersion
        {
            public class Channel
            {
                public string channelName;
                public bool isShenHe;
                public string resourceUrl;
                public List<string> ips;
                public List<int> ports;
            }

            public string version;
            public List<Channel> channels;
        }

        public string resourceTips;
        public string versionTips;
        public string versionUrl;
        public string patchUrl;
        public List<GameVersion> gameVersions;
    }

    public class SelfUpdateManager : Singleton<SelfUpdateManager>
    {
        UpdateStep _currentStep = new UpdateStep();
        UpdateProgress _currentProgress = new UpdateProgress();
        
        string _resourceUpdateTips;
        string _versionUpdateTips;
        string _versionUrl;
        string _patchUrl;
        uint _resourceCrc;
        
        ResourceDatas _newResources = null;

        Action<UpdateStep> _onShowUpdateStep;
        Action<UpdateProgress> _onShowUpdateProgress;
        Action<int> _onShowUpdateStepFail;
        List<string> _needUpdateResources = new List<string>();
        long _totalUpdateSize;
        long _updateFinishSize;
        string _lastCodeVersion;

        public void Start(Action<UpdateStep> onShowUpdateStep, Action<UpdateProgress> onShowUpdateProgress, Action<int> onShowUpdateStepFail)
        {
            _onShowUpdateStep = onShowUpdateStep;
            _onShowUpdateProgress = onShowUpdateProgress;
            _onShowUpdateStepFail = onShowUpdateStepFail;
            ChangeCurrentUpdateState(UpdateState.CheckNetWork);
        }

        void ChangeCurrentUpdateState(UpdateState state, bool excute = true, object arg = null)
        {
            _currentStep.State = state;
            _currentStep.CallBack = ExcuteCurrentUpdateState;
            _currentStep.showTip = !excute;
            _currentStep.arg = arg;
            _onShowUpdateStep(_currentStep);
            if (excute)
                ExcuteCurrentUpdateState();
        }

        void ExcuteCurrentUpdateState()
        {
            switch (_currentStep.State)
            {
                case UpdateState.CheckNetWork:
                    CheckNetWork();
                    break;
                case UpdateState.DownloadGateway:
                    DownloadGateway();
                    break;
                case UpdateState.ComparServerVersion:
                    ComparServerVersion();
                    break;
                case UpdateState.DownloadResourceList:
                    DownloadResourceList();
                    break;
                case UpdateState.ComparServerResource:
                    ComparServerResource();
                    break;
                case UpdateState.DownloadServerResource:
                    DownloadServerResource();
                    break;
                case UpdateState.CheckLoacalResource:
                    CheckLoacalResource();
                    break;
                case UpdateState.DownloadPatchConfig:
                    DownloadPatchConfig();
                    break;
                case UpdateState.OpenNewClientUrl:
                    OpenNewClientUrl();
                    break;
                case UpdateState.DownloadNewClient:
                    DownloadNewClient();
                    break;
                case UpdateState.DownloadNewClientPatch:
                    DownloadNewClientPatch();
                    break;
                case UpdateState.GenerateNewClient:
                    GenerateNewClient();
                    break;
                case UpdateState.InstallNewClient:
                    InstallNewClient();
                    break;
                case UpdateState.UpdateFinish:
                    UpdateFinish();
                    break;
            }
        }

        void CheckNetWork()
        {
            if (NetworkHelper.GetNetWorkType() == NetworkHelper.NetworkType.NT_NONE)
                _onShowUpdateStepFail(0);
            else
                ChangeCurrentUpdateState(UpdateState.DownloadGateway);
        }

        void DownloadGateway()
        {
            GameClient.Instance.ips.Clear();
            GameClient.Instance.ports.Clear();
#if UNITY_EDITOR
           /*string str = File.ReadAllText(Application.dataPath + "/../gateway.txt");
           Debug.Log(str);
           string[] strs = str.Split(' ');
           GameClient.Instance.ips.Add(strs[0]);
           GameClient.Instance.ports.Add(int.Parse(strs[1]));
           ChangeCurrentUpdateState(UpdateState.UpdateFinish);
#else*/
            string url = ILRuntimeHelper.GetGatewayUrl();
            Debugger.Log(url, true);
            string savepath = Application.persistentDataPath + "/gateway.txt";
            Downloader.DowloadFiles(new List<DownloadFile>() { new DownloadFile(url, savepath) },
            (obj) =>
            {
                if (!File.Exists(savepath))
                {
                    _onShowUpdateStepFail(0);
                    return;
                }

                string json = File.ReadAllText(savepath);
                GatewayConfig gatewayConfig = LitJson.JsonMapper.ToObject<GatewayConfig>(json);
                _resourceUpdateTips = gatewayConfig.resourceTips;
                _versionUpdateTips = gatewayConfig.versionTips;
                _versionUrl = gatewayConfig.versionUrl;
                _patchUrl = gatewayConfig.patchUrl;
                string version = ILRuntimeHelper.GetVersion();
                string channelName = ILRuntimeHelper.GetChannelName();
                bool find = false;
                for (int i = 0; i < gatewayConfig.gameVersions.Count; ++i)
                {
                    if (gatewayConfig.gameVersions[i].version == version)
                    {
                        for (int j = 0; j < gatewayConfig.gameVersions[i].channels.Count; ++j)
                        {
                            var channel = gatewayConfig.gameVersions[i].channels[j];
                            if (channel.channelName == channelName)
                            {
                                GameClient.Instance.isShenHe = channel.isShenHe;
                                ResourceManager.Instance.ResourceUrl = channel.resourceUrl;
                                Debugger.Log(ResourceManager.Instance.ResourceUrl, true);
                                GameClient.Instance.ips.AddRange(channel.ips);
                                GameClient.Instance.ports.AddRange(channel.ports);
                                find = true;
                                break;
                            }
                        }
                    }
                }
                if (!find)
                {
                    _onShowUpdateStepFail(1);
                    return;
                }
                if (GameClient.Instance.isShenHe)
                    ChangeCurrentUpdateState(UpdateState.UpdateFinish);
                else
                    ChangeCurrentUpdateState(UpdateState.ComparServerVersion);
            });
#endif
        }

        void ComparServerVersion()
        {
            string url = ResourceManager.Instance.ResourceUrl + "version.txt";
            Debugger.Log(url);
            string savepath = Application.persistentDataPath + "/version.txt";
            Downloader.DowloadFiles(new List<DownloadFile>() { new DownloadFile(url, savepath) },
            (obj) =>
            {
                if (!File.Exists(savepath))
                {
                    _onShowUpdateStepFail(0);
                    return;
                }

                string str = File.ReadAllText(savepath);
                string[] strs = str.Split(' ');
                if (strs.Length != 2)
                {
                    _onShowUpdateStepFail(1);
                    return;
                }

                int serverCodeVersion;
                if (!int.TryParse(strs[0], out serverCodeVersion))
                {
                    _onShowUpdateStepFail(2);
                    return;
                }

                uint crc;
                if (!uint.TryParse(strs[0], out crc))
                {
                    _onShowUpdateStepFail(3);
                    return;
                }

                if (serverCodeVersion > ResourceManager.CodeVersion)
                {
                    if (UnityDefine.UnityAndroid)
                    {
                        bool updateInGame = false;
                        Debugger.LogError("TODO:Check if update in game");
                        if (updateInGame)
                            ChangeCurrentUpdateState(UpdateState.DownloadPatchConfig);
                        else
                            ChangeCurrentUpdateState(UpdateState.OpenNewClientUrl, false);
                    }
                    else
                        ChangeCurrentUpdateState(UpdateState.OpenNewClientUrl, false);
                }
                else
                {
                    uint localCrc = FileHelper.GetFileCrc(ResourceManager.Instance.DataPath + "ResourceList.ab");
                    if (crc != localCrc)
                    {
                        _resourceCrc = crc;
                        ChangeCurrentUpdateState(UpdateState.DownloadResourceList);
                    }
                    else
                    {
                        ChangeCurrentUpdateState(UpdateState.CheckLoacalResource);
                    }
                }
            });
        }

        void DownloadResourceList()
        {
            string url = ResourceManager.Instance.ResourceUrl + "ResourceList.ab";
            Debugger.Log(url);
            string savepath = Application.persistentDataPath + "/ResourceList.ab";
            Downloader.DowloadFiles(new List<DownloadFile>() { new DownloadFile(url, savepath, 100, _resourceCrc, true) },
            (obj) =>
            {
                if (!File.Exists(savepath))
                {
                    _onShowUpdateStepFail(0);
                    return;
                }

                ChangeCurrentUpdateState(UpdateState.ComparServerResource);
            });
        }

        void ComparServerResource()
        {
            _newResources = ResourceManager.LoadResourceDatas(Application.persistentDataPath + "/ResourceList.ab");
            if (_newResources == null || _newResources.Resources.Count == 0)
            {
                _onShowUpdateStepFail(0);
                return;
            }

            Debugger.Log("new resource count : " + _newResources.Resources.Count);
            var a = new TraverseInThread<KeyValuePair<string, ResourceData>>(_newResources.Resources, _newResources.Resources.Count,
                (obj)=>
                {
                    KeyValuePair<string, ResourceData> pair = (KeyValuePair<string, ResourceData>)obj;
                    Debug.Log(pair.Key);
                },
                ()=>
                {

                });
        }

        void DownloadServerResource()
        {
        }

        void CheckLoacalResource()
        {
        }

        void DownloadPatchConfig()
        {
        }

        void OpenNewClientUrl()
        {
            string channelName = "game_windows";
            Debugger.LogError("TODO:Get channelName");
            string url = _versionUrl + "?channlname=" + channelName;
            Application.OpenURL(url);
        }

        void DownloadNewClient()
        {
        }

        void DownloadNewClientPatch()
        {
        }

        void GenerateNewClient()
        {
        }

        void InstallNewClient()
        {
        }

        void UpdateFinish()
        {
        }
    }
}
