/*
auth: Xiang ChunSong
purpose:
*/

using Google.Protobuf;
using System;
using System.IO;
using Utils;
using System.Collections.Generic;
using UnityEngine;

namespace Base
{
    public enum UpdateState
    {
        CheckNetWork,
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
    }

    public class UpdateStep
    {
        public UpdateState State;
        public Action CallBack;
    }

    public class UpdateProgress
    {
        public float Progreess;
        public long TotalSize;
    }

    public class ResourceManager : Singleton<ResourceManager>
    {
        public static readonly int CodeVersion = 0;    //客户端代码版本号,用于判断版本是否需要升级

        string _dataPath;
        string _optionalPath;
        string _streamingPath;
        string _resourceUrl;

        UpdateStep _currentStep = new UpdateStep();
        UpdateProgress _currentProgress = new UpdateProgress();

        ResourceDatas _resources = null;
        ResourceDatas _newResources = null;
        Dictionary<string, List<WeakReference>> _resourceReferences = new Dictionary<string, List<WeakReference>>();
        Dictionary<string, AssetBundle> _loadedAssetBundles = new Dictionary<string, AssetBundle>();

        Action<UpdateStep> _onShowUpdateStep;
        Action<UpdateProgress> _onShowUpdateProgress;
        Action<object> _onShowUpdateStepFail;
        List<string> _needUpdateResources = new List<string>();
        long _totalUpdateSize;
        long _updateFinishSize;
        string _lastCodeVersion;

        public ResourceManager()
        {

        }

        public void Init()
        {

        }

        public void AfterInit()
        {
            Debugger.Log(ILRuntimeHelper.GetResourceUrl(), true);
        }

        public void Start(Action<UpdateStep> onShowUpdateStep, Action<UpdateProgress> onShowUpdateProgress, Action<object> onShowUpdateStepFail)
        {
            _onShowUpdateStep = onShowUpdateStep;
            _onShowUpdateProgress = onShowUpdateProgress;
            _onShowUpdateStepFail = onShowUpdateStepFail;
            ChangeCurrentUpdateState(UpdateState.CheckNetWork);
        }

        void ChangeCurrentUpdateState(UpdateState state, bool excute = true)
        {
            _currentStep.State = state;
            _currentStep.CallBack = ExcuteCurrentUpdateState;
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
                case UpdateState.ComparServerVersion:
                    ComparServerVersion();
                    break;
            }
        }

        void CheckNetWork()
        {
            _onShowUpdateStepFail(null);
        }

        void ComparServerVersion()
        {

        }

        static ResourceDatas LoadResourceDatas(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }

            FileStream fs = new FileStream(path, FileMode.Open);
            ResourceDatas rds = LoadResourceDatas(fs);
            fs.Close();
            return rds;
        }

        static ResourceDatas LoadResourceDatas(Stream stream)
        {
            return ResourceDatas.Parser.ParseFrom(stream);
        }

        static void SaveResourceDatas(string path, ResourceDatas datas)
        {
            FileStream fs = new FileStream(path, FileMode.Create);
            datas.WriteTo(fs);
            fs.Flush();
            fs.Close();
        }
    }
}
