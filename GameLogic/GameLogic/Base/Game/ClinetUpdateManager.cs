using Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameLogic
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

    class ClinetUpdateManager : Singleton<ClinetUpdateManager>
    {
        UpdateStep _currentStep = new UpdateStep();
        UpdateProgress _currentProgress = new UpdateProgress();

        ResourceDatas _newResources = null;

        Action<UpdateStep> _onShowUpdateStep;
        Action<UpdateProgress> _onShowUpdateProgress;
        Action<object> _onShowUpdateStepFail;
        List<string> _needUpdateResources = new List<string>();
        long _totalUpdateSize;
        long _updateFinishSize;
        string _lastCodeVersion;

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
    }
}
