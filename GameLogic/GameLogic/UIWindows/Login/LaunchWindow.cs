using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Base;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    class LaunchWindow : UIWindow
    {
        UpdateStep _currentStep;

        Slider _progress;
        Text _progressText;
        Text _stepText;

        protected override void OnInit()
        {
            _progress = GetChildComponent<Slider>("Progress");
            _progressText = GetChildComponent<Text>("ProgressText");
            _stepText = GetChildComponent<Text>("StepText");
        }

        protected override void OnOpen(object[] args)
        {
            SelfUpdateManager.Instance.Start(OnShowUpdateStep, OnShowUpdateProgress, OnShowUpdateStepFail);
        }

        void OnShowUpdateStep(UpdateStep step)
        {
            _currentStep = step;
            Debugger.Log("Current state : " + _currentStep.State);
            Debugger.Log("Show tip : " + _currentStep.showTip);
            switch (_currentStep.State)
            {
                case UpdateState.CheckNetWork:
                    _stepText.text = "Checking......";
                    break;
                case UpdateState.DownloadServerResource:
                    string tip = _currentStep.arg as string;
                    Helper.ShowMessageBox(tip, () =>
                    {
                        _progress.gameObject.SetActive(true);
                        _progress.value = 0;
                        _progressText.text = "0%";
                        _stepText.text = "downloading......";
                        _currentStep.CallBack();
                    });
                    break;
                case UpdateState.RestartClient:
                    Helper.ShowMessageBox("Restart game", () =>
                    {
                        _currentStep.CallBack();
                    });
                    break;
                case UpdateState.UpdateFinish:
                    UIManager.OpenWindow("ConnectServerWindow");
                    break;
            }
        }

        void OnShowUpdateProgress(UpdateProgress progress)
        {
            switch (_currentStep.State)
            {
                case UpdateState.CheckNetWork:
                    break;
                case UpdateState.DownloadServerResource:
                    _progress.value = progress.Progreess;
                    int num = (int)(progress.Progreess * 100);
                    _progressText.text = string.Format("{0}%", num);
                    break;
            }
        }

        void OnShowUpdateStepFail(int errorCode)
        {
            Debugger.LogError("Error on state :" + _currentStep.State + " error code : " + errorCode);
            switch (_currentStep.State)
            {
                case UpdateState.CheckNetWork:
                    break;
            }
        }
    }
}
