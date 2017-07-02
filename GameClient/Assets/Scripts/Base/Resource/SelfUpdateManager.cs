using Google.Protobuf;
using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

namespace Base
{
    public enum UpdateState
    {
        CheckNetWork,
        DownloadGateway,
        ComparServerVersion,
        CheckLoacalResource,
        DownloadResourceList,
        ComparServerResource,
        DownloadServerResource,
        OpenNewClientUrl,
        DownloadPatchConfig,
        DownloadNewClientPatch,
        GenerateNewClient,
        DownloadNewClient,
        InstallNewClient,
        RestartClient,
        ReloadConfigs,
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

    public class PatchConfig
    {
        public class Channel
        {
            public class Patch
            {
                public string oldVersion;
                public string patch_all;
                public string oldmd5_all;
                public int size_all;
                public string patch_mini;
                public string oldmd5_mini;
                public int size_mini;
            }

            public string name;
            public string md5_all;
            public int size_all;
            public string md5_mini;
            public int size_mini;
            public List<Patch> patchs;
        }

        public string newVersion;
        public List<Channel> channels;
    }

    public class SelfUpdateManager : Singleton<SelfUpdateManager>
    {
        Action<UpdateStep> _onShowUpdateStep;
        Action<UpdateProgress> _onShowUpdateProgress;
        Action<int> _onShowUpdateStepFail;
        UpdateStep _currentStep = new UpdateStep();
        UpdateProgress _currentProgress = new UpdateProgress();
        
        ResourceDatas _newResources = null;
        
        string _resourceUpdateTips;
        string _versionUpdateTips;
        string _versionUrl;
        string _patchUrl;
        string _patchFileUrl;
        int _targetVersion;
        string _targetMd5;
        
        uint _resourceCrc;
        List<string> _needUpdateResources = new List<string>();
        Downloader _currentDownloader = null;
        bool _isSaving;
        DateTime _lastSaveTime;
        List<KeyValuePair<string, ResourceData>> _finishDatas = new List<KeyValuePair<string, ResourceData>>();
        ResourceDatas _currentResourclist = new ResourceDatas();
        bool _needRestart = false;
        bool _needReload = false;

        static object _threadLock = new object();
        static object _saveThreadLock = new object();

        public void Start(Action<UpdateStep> onShowUpdateStep, Action<UpdateProgress> onShowUpdateProgress, Action<int> onShowUpdateStepFail)
        {
            _onShowUpdateStep = onShowUpdateStep;
            _onShowUpdateProgress = onShowUpdateProgress;
            _onShowUpdateStepFail = onShowUpdateStepFail;

#if !UNITY_EDITOR && UNITY_ANDROID
            Debugger.LogError("Application.persistentDataPath : " + Application.persistentDataPath);
            DirectoryInfo dir = new DirectoryInfo(Application.persistentDataPath);
            Debugger.LogError(dir);
            FileInfo[] files = dir.GetFiles("*.apk");
            Debugger.LogError("files.Length : " + files.Length);
            for (int i = 0; i < files.Length; ++i)
            {
                string filename = Path.GetFileNameWithoutExtension(files[i].Name);
                Debugger.LogError("filename : " + filename);
                string[] strs = filename.Split('_');
                Debugger.LogError(strs.Length);
                float version;
                if (float.TryParse(strs[0], out version))
                {
                    Debugger.LogError("version : " + version);
                    if ((int)version > (int)ResourceManager.CodeVersion)
                    {
                        _targetVersion = (int)version;
                        ChangeCurrentUpdateState(UpdateState.InstallNewClient);
                        return;
                    }
                }

                File.Delete(files[i].FullName);
            }
#endif

            ChangeCurrentUpdateState(UpdateState.CheckNetWork);
        }

        void ChangeCurrentUpdateState(UpdateState state, bool autoexcute = true, object arg = null)
        {
            _currentStep.State = state;
            _currentStep.CallBack = ExcuteCurrentUpdateState;
            _currentStep.showTip = !autoexcute;
            _currentStep.arg = arg;
            _onShowUpdateStep(_currentStep);
            if (autoexcute)
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
                case UpdateState.CheckLoacalResource:
                    CheckLoacalResource();
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
                case UpdateState.OpenNewClientUrl:
                    OpenNewClientUrl();
                    break;
                case UpdateState.DownloadPatchConfig:
                    DownloadPatchConfig();
                    break;
                case UpdateState.DownloadNewClientPatch:
                    DownloadNewClientPatch();
                    break;
                case UpdateState.GenerateNewClient:
                    GenerateNewClient();
                    break;
                case UpdateState.DownloadNewClient:
                    DownloadNewClient();
                    break;
                case UpdateState.InstallNewClient:
                    InstallNewClient();
                    break;
                case UpdateState.RestartClient:
                    RestartClient();
                    break;
                case UpdateState.ReloadConfigs:
                    ReloadConfigs();
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
            if (File.Exists(Application.dataPath + "/../gateway.txt"))
            {
                string str = File.ReadAllText(Application.dataPath + "/../gateway.txt");
                string[] strs = str.Split(' ');
                GameClient.Instance.ips.Add(strs[0]);
                GameClient.Instance.ports.Add(int.Parse(strs[1]));
            }
            ChangeCurrentUpdateState(UpdateState.UpdateFinish);
#else
#if ILRUNTIME_DEBUG && UNITY_STANDALONE_WIN
            if (ResourceManager.IsILRuntimeDebug)
            {
                if (File.Exists(Application.dataPath + "/gateway.txt"))
                {
                    string str = File.ReadAllText(Application.dataPath + "/gateway.txt");
                    string[] strs = str.Split(' ');
                    GameClient.Instance.ips.Add(strs[0]);
                    GameClient.Instance.ports.Add(int.Parse(strs[1]));
                }
                ChangeCurrentUpdateState(UpdateState.UpdateFinish);
            }
            else
            {
#endif
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
                GatewayConfig gatewayConfig = JsonMapper.ToObject<GatewayConfig>(json);
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
                                ResourceManager.ResourceUrl = channel.resourceUrl;
                                Debugger.Log(ResourceManager.ResourceUrl, true);
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

                File.Delete(savepath);

                if (GameClient.Instance.isShenHe && !GameClient.Instance.BuildSettings.MiniBuild)
                    ChangeCurrentUpdateState(UpdateState.UpdateFinish);
                else
                    ChangeCurrentUpdateState(UpdateState.ComparServerVersion);
            });
#if ILRUNTIME_DEBUG && UNITY_STANDALONE_WIN
            }
#endif
#endif
        }

        void ComparServerVersion()
        {
            string url = ResourceManager.ResourceUrl + "version.txt";
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

                float serverCodeVersion;
                if (!float.TryParse(strs[0], out serverCodeVersion))
                {
                    _onShowUpdateStepFail(2);
                    return;
                }

                uint crc;
                if (!uint.TryParse(strs[1], out crc))
                {
                    _onShowUpdateStepFail(3);
                    return;
                }

                File.Delete(savepath);

                if ((int)serverCodeVersion > (int)ResourceManager.CodeVersion)
                {
#if UNITY_ANDROID && !UNITY_EDITOR
                    bool updateInGame = ILRuntimeHelper.GetUpdateInGame();
                    if (updateInGame)
                    {
                        _targetVersion = (int)serverCodeVersion;
                        ChangeCurrentUpdateState(UpdateState.DownloadPatchConfig);
                    }
                    else
                        ChangeCurrentUpdateState(UpdateState.OpenNewClientUrl, false);
#else
                    ChangeCurrentUpdateState(UpdateState.OpenNewClientUrl, false);
#endif
                }
                else
                {
                    uint localCrc = FileHelper.GetFileCrc(ResourceManager.DataPath + "ResourceList.ab");
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

        void CheckLoacalResource()
        {
            _newResources = ResourceManager.LoadResourceDatas(ResourceManager.DataPath + "ResourceList.ab");
            NeedDownloadResource();
        }

        void DownloadResourceList()
        {
            string url = ResourceManager.ResourceUrl + "ResourceList.ab";
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
            NeedDownloadResource();
        }

        void NeedDownloadResource()
        {
            _needUpdateResources.Clear();
            int totalSize = 0;

#if UNITY_ANDROID && !UNITY_EDITOR
            ResourceDatas streamingResources = ResourceManager.LoadResourceDatas(ResourceManager.GetStreamingFile("ResourceList.ab"));
#else
            ResourceDatas streamingResources = ResourceManager.LoadResourceDatas(ResourceManager.StreamingPath + "ResourceList.ab");
#endif
            var a = new TraverseInThread<KeyValuePair<string, ResourceData>>(_newResources.Resources, _newResources.Resources.Count,
            (obj) =>
            {
                KeyValuePair<string, ResourceData> pair = (KeyValuePair<string, ResourceData>)obj;
                string key = pair.Key;
                ResourceData rd = pair.Value;
                string filename = ResourceManager.GetResourceFileName(key);
                if (!rd.IsOptional())
                {
                    bool streaminghas = false;
                    if (streamingResources.Resources.ContainsKey(key))
                    {
                        ResourceData srd = streamingResources.Resources[key];
                        if (rd.Crc == srd.Crc)
                            streaminghas = true;
                    }

                    if (FileHelper.GetFileCrc(ResourceManager.DataPath + filename) != rd.Crc && !streaminghas)
                    {
                        lock (_threadLock)
                        {
                            _needUpdateResources.Add(key);
                        }
                        totalSize += rd.Size;
                    }
                }
                else
                {
                    if (File.Exists(ResourceManager.OptionalPath + filename))
                    {
                        if (FileHelper.GetFileCrc(ResourceManager.OptionalPath + filename) != rd.Crc)
                        {
                            lock (_threadLock)
                            {
                                _needUpdateResources.Add(key);
                            }
                            totalSize += rd.Size;
                        }
                    }
                }
            },
            () =>
            {
                if (_needUpdateResources.Count == 0)
                {
                    if (File.Exists(Application.persistentDataPath + "/ResourceList.ab"))
                    {
                        try
                        {
                            File.Copy(Application.persistentDataPath + "/ResourceList.ab", ResourceManager.DataPath + "ResourceList.ab", true);
                            File.Delete(Application.persistentDataPath + "/ResourceList.ab");
                        }
                        catch (Exception ex)
                        {
                            Debugger.LogException(ex);
                        }
                    }
                    ChangeCurrentUpdateState(UpdateState.UpdateFinish);
                }
                else
                {
                    _currentProgress.TotalSize = totalSize;
                    object[] obj = new object[2];
                    obj[0] = totalSize;
                    obj[1] = _resourceUpdateTips;
                    ChangeCurrentUpdateState(UpdateState.DownloadServerResource, false, obj);
                }
            });
        }

        void DownloadServerResource()
        {
            if (_currentDownloader != null)
            {
                _currentDownloader.PauseDownload(false);
                GameClient.Instance.StartCoroutine(CheckDownloadingNetWork());
                return;
            }

            _currentProgress.Progreess = 0f;
            _onShowUpdateProgress(_currentProgress);

            string newResourceListPath = Application.persistentDataPath + "/ResourceList.ab";
            if (File.Exists(newResourceListPath))
            {
                _finishDatas.Clear();
                var itor = ResourceManager.Instance.ResourceList.Resources.GetEnumerator();
                while (itor.MoveNext())
                {
                    _currentResourclist.Resources.Add(itor.Current.Key, itor.Current.Value);
                }
            }

            int toltalSize = 0;
            List<DownloadFile> downloadfiles = new List<DownloadFile>(_needUpdateResources.Count);
            var a = new TraverseInThread<string>(_needUpdateResources, _needUpdateResources.Count,
            (arg) =>
            {
                string key = arg as string;
                ResourceData rd = _newResources.Resources[key];
                string filename = ResourceManager.GetResourceFileName(key);
                string saveName = rd.IsOptional() ? ResourceManager.OptionalPath + filename : ResourceManager.DataPath + filename;
                DownloadFile downloadFile = new DownloadFile(ResourceManager.ResourceUrl + key + ".ab", saveName, rd.Size, rd.Crc);
                toltalSize += rd.Size;
                lock (_threadLock)
                {
                    downloadfiles.Add(downloadFile);
                }

                if (rd.Path.Contains("Install/Unpackage/GameLogic.bytes") || rd.Path.Contains("Scenes/Install/UI"))
                {
                    _needRestart = true;
                }
                else if (rd.Path.Contains("Install/Unpackage/Data/"))
                {
                    _needReload = true;
                }
            },
            () =>
            {
                _currentProgress.Progreess = 0f;
                _currentProgress.TotalSize = toltalSize;
                _onShowUpdateProgress(_currentProgress);

                _lastSaveTime = DateTime.Now;

                _currentDownloader = Downloader.DowloadFiles(downloadfiles,
                (arg) =>    //onFinish
                {
                    _currentDownloader = null;
                    _currentResourclist.Resources.Clear();
                    _finishDatas.Clear();
                    if (_needUpdateResources.Count > 0)
                    {
                        _onShowUpdateStepFail(0);
                        return;
                    }

                    if (File.Exists(newResourceListPath))
                    {
                        try
                        {
                            lock (_threadLock)
                            {
                                File.Copy(newResourceListPath, ResourceManager.DataPath + "ResourceList.ab", true);
                            }
                            File.Delete(newResourceListPath);
                        }
                        catch (Exception ex)
                        {
                            Debugger.LogException(ex);
                        }
                        ResourceManager.Instance.LoadResourceList();
                    }
                    
                    if (_needRestart)
                    {
                        ChangeCurrentUpdateState(UpdateState.RestartClient, false);
                    }
                    else
                    {
                        _newResources.Resources.Clear();
                        _newResources = null;
                        if (_needReload)
                        {
                            ChangeCurrentUpdateState(UpdateState.ReloadConfigs);
                        }
                        else
                        {
                            ChangeCurrentUpdateState(UpdateState.UpdateFinish);
                        }
                    }
                },
                (arg) =>    //onProgress;
                {
                    object[] args = arg as object[];
                    _currentProgress.Progreess = (float)args[0];
                    _currentProgress.TotalSize = toltalSize;
                    _onShowUpdateProgress(_currentProgress);
                },
                (arg) =>    //onSingleFileFinish
                {
                    if (!File.Exists(newResourceListPath))
                        return;

                    DownloadFile file = arg as DownloadFile;
                    string key = Path.GetFileNameWithoutExtension(file.savePath);
                    if (File.Exists(file.savePath))
                    {
                        ResourceData rd = _newResources.Resources[key];
                        KeyValuePair<string, ResourceData> pair = new KeyValuePair<string, ResourceData>(key, rd);
                        lock (_saveThreadLock)
                        {
                            _finishDatas.Add(pair);
                        }
                        _needUpdateResources.Remove(key);
                    }

                    if (_currentDownloader != null && !_isSaving && (_lastSaveTime - DateTime.Now).Seconds > 20)
                    {
                        _isSaving = true;
                        _lastSaveTime = DateTime.Now;
                        Thread thread = new Thread(SaveResourceList);
                        thread.Start();
                        _isSaving = false;
                    }
                }
                );
                GameClient.Instance.StartCoroutine(CheckDownloadingNetWork());
            });
        }

        void SaveResourceList()
        {
            int num = 0;
            lock(_saveThreadLock)
            {
                num = _finishDatas.Count;
            }
            for (int i = 0; i < num; ++i)
            {
                _currentResourclist.Resources[_finishDatas[0].Key] = _finishDatas[0].Value;
                lock(_saveThreadLock)
                {
                    _finishDatas.RemoveAt(0);
                }
            }
            FileStream fs = new FileStream(ResourceManager.DataPath + "ResourceList.ab.ab", FileMode.Create);
            _currentResourclist.WriteTo(fs);
            fs.Flush();
            fs.Close();
            try
            {
                lock (_threadLock)
                {
                    File.Copy(ResourceManager.DataPath + "ResourceList.ab.ab", ResourceManager.DataPath + "ResourceList.ab", true);
                }
                File.Delete(ResourceManager.DataPath + "ResourceList.ab.ab");
            }
            catch (Exception ex)
            {
                Debugger.LogException(ex);
            }
        }

        IEnumerator CheckDownloadingNetWork()
        {
            while (_currentDownloader != null)
            {
                if (NetworkHelper.GetNetWorkType() == NetworkHelper.NetworkType.NT_NONE)
                {
                    _onShowUpdateStepFail(1);
                    _currentDownloader.PauseDownload(true);
                    break;
                }
                yield return 0;
            }
        }

        void OpenNewClientUrl()
        {
            string url = _versionUrl + "?channlname=" + ILRuntimeHelper.GetChannelName();
            Application.OpenURL(url);
        }

        float GetVersionValue(string version)
        {
            float value;
            if (float.TryParse(version, out value))
                return value;

            return -1f;
        }

        void DownloadPatchConfig()
        {
            string url = _patchUrl + "patchs.txt";
            string savepath = Application.persistentDataPath + "/patchs.txt";
            Downloader.DowloadFiles(new List<DownloadFile>() { new DownloadFile(url, savepath) },
            (arg) =>
            {
                if (!File.Exists(savepath))
                {
                    object[] obj = new object[2];
                    obj[0] = 50000;
                    obj[1] = _versionUpdateTips;
                    _currentProgress.TotalSize = 50000;
                    ChangeCurrentUpdateState(UpdateState.DownloadNewClient, false, obj);
                }
                else
                {
                    string str = File.ReadAllText(savepath);
                    bool find = false;
                    int totalSize = 50000;
                    PatchConfig config = JsonMapper.ToObject<PatchConfig>(str);
                    if (_targetVersion == (int)GetVersionValue(config.newVersion))
                    {
                        string channelName = ILRuntimeHelper.GetChannelName();
                        for (int i = 0; i < config.channels.Count; ++i)
                        {
                            var channel = config.channels[i];
                            if (channelName == channel.name)
                            {
                                totalSize = channel.size_mini;
                                _targetMd5 = GameClient.Instance.BuildSettings.MiniBuild ? channel.md5_mini : channel.md5_all;
                                for (int j = 0; j < channel.patchs.Count; ++j)
                                {
                                    var patch = channel.patchs[j];
                                    if (ResourceManager.CodeVersion == GetVersionValue(patch.oldVersion))
                                    {
                                        string md5 = FileHelper.GetFileMd5(Application.dataPath);
                                        if ((GameClient.Instance.BuildSettings.MiniBuild && md5 == patch.oldmd5_mini) || (!GameClient.Instance.BuildSettings.MiniBuild && md5 == patch.oldmd5_all))
                                        {
                                            _patchFileUrl = GameClient.Instance.BuildSettings.MiniBuild ? _patchUrl + patch.patch_mini : _patchUrl + patch.patch_all;
                                            totalSize = GameClient.Instance.BuildSettings.MiniBuild ? patch.size_mini : patch.size_all;
                                            find = true;
                                        }

                                        break;
                                    }
                                }

                                if (find)
                                {
                                    break;
                                }
                            }
                        }
                    }

                    File.Delete(savepath);
                    _currentProgress.TotalSize = totalSize;
                    if (find)
                    {
                        object[] obj = new object[2];
                        obj[0] = totalSize;
                        obj[1] = _versionUpdateTips;
                        ChangeCurrentUpdateState(UpdateState.DownloadNewClientPatch, false, obj);
                    }
                    else
                    {
                        object[] obj = new object[2];
                        obj[0] = totalSize;
                        obj[1] = _versionUpdateTips;
                        ChangeCurrentUpdateState(UpdateState.DownloadNewClient, false, obj);
                    }
                }

            });
        }

        void DownloadNewClientPatch()
        {
            if (_currentDownloader != null)
            {
                _currentProgress.TotalSize = 50000;
                ChangeCurrentUpdateState(UpdateState.DownloadNewClient);
                _currentDownloader = null;
                return;
            }

            string savepath = Application.persistentDataPath + "/" + _targetVersion + "_" + _targetMd5 + ".patch";
            _currentDownloader = Downloader.DowloadFiles(new List<DownloadFile>() { new DownloadFile(_patchFileUrl, savepath) },
            (arg) =>
            {
                if (!File.Exists(savepath))
                {
                    _onShowUpdateStepFail(0);
                }
                else
                {
                    _currentDownloader = null;
                    ChangeCurrentUpdateState(UpdateState.GenerateNewClient);
                }
            },
            (arg) =>
            {
                object[] args = arg as object[];
                _currentProgress.Progreess = (float)args[0];
                _onShowUpdateProgress(_currentProgress);
            });
        }

        void GenerateNewClient()
        {
            string newApk = Application.persistentDataPath + "/" + _targetVersion + "_game.apk";
            string patchPath = Application.persistentDataPath + "/" + _targetVersion +"_" + _targetMd5 + ".patch";
            if (!File.Exists(patchPath))
            {
                _currentProgress.TotalSize = 50000;
                ChangeCurrentUpdateState(UpdateState.DownloadNewClient);
                return;
            }
            int res = AndroidInstallApk.GreateNewApk(newApk, patchPath);
            File.Delete(patchPath);
            if (res == 0)
            {
                if (FileHelper.GetFileMd5(newApk) == _targetMd5)
                {
                    ChangeCurrentUpdateState(UpdateState.InstallNewClient);
                }
                else
                {
                    if (File.Exists(newApk))
                        File.Delete(newApk);
                    _onShowUpdateStepFail(100);
                }
            }
            else
            {
                if (File.Exists(newApk))
                    File.Delete(newApk);
                _onShowUpdateStepFail(res);
            }
        }

        void DownloadNewClient()
        {
            string url = _patchUrl + "_" + ILRuntimeHelper.GetDownladName();
            string savepath = Application.persistentDataPath + "/" + _targetVersion + "_game.apk";
            Downloader.DowloadFiles(new List<DownloadFile>() { new DownloadFile(url, savepath) },
            (obj) =>
            {
                if (!File.Exists(savepath))
                {
                    ChangeCurrentUpdateState(UpdateState.OpenNewClientUrl);
                }
                else
                {
                    ChangeCurrentUpdateState(UpdateState.InstallNewClient);
                }
            },
            (arg)=>
            {
                object[] args = arg as object[];
                _currentProgress.Progreess = (float)args[0];
                _onShowUpdateProgress(_currentProgress);
            });
        }

        void InstallNewClient()
        {
            AndroidInstallApk.InstallApk(Application.persistentDataPath + "/" + _targetVersion + "_game.apk");
        }

        void RestartClient()
        {
            GameClient.Instance.RestartGame();
        }

        void ReloadConfigs()
        {
            ChangeCurrentUpdateState(UpdateState.UpdateFinish);
        }

        void UpdateFinish()
        {
            Debugger.Log("self update finish!", true);
        }
    }
}
