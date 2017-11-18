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
        
        protected override void OnSetWindowDetail()
        {
            Settings.PrefabName = "UI/Install/LaunchWindow";
        }

        protected override void OnInit()
        {
            _progress = GetChildComponent<Slider>("Progress");
            _progressText = GetChildComponent<Text>("ProgressText");
            _stepText = GetChildComponent<Text>("StepText");
        }

        protected override void OnOpen(object[] args)
        {
            TimerManager.Instance.AddDelayTimer(3, () =>
            {
                SelfUpdateManager.Instance.Start(OnShowUpdateStep, OnShowUpdateProgress, OnShowUpdateStepFail);
            });
        }

        void OnShowUpdateStep(UpdateStep step)
        {
            _currentStep = step;
            Debugger.Log("Current Step : " + _currentStep.State, true);
            //Debugger.Log("Show tip : " + _currentStep.showTip);
            switch (_currentStep.State)
            {
                case UpdateState.CheckNetWork:
                    _stepText.text = "Checking......";
                    break;
                case UpdateState.DownloadServerResource:
                    object[] args = _currentStep.arg as object[];
                    int totalSize = (int)args[0];
                    string str = args[1] as string;
                    if (NetworkHelper.GetNetWorkType() == NetworkHelper.NetworkType.NT_WIFI || totalSize < 5 * 1024)
                    {
                        _progress.gameObject.SetActive(true);
                        _progress.value = 0;
                        _progressText.text = "0%";
                        _stepText.text = "downloading......";
                        _currentStep.CallBack();
                    }
                    else
                    {
                        string tip = string.Format(str, GetSizeString(totalSize));
                        Helper.ShowMessageBox(tip, () =>
                        {
                            _progress.gameObject.SetActive(true);
                            _progress.value = 0;
                            _progressText.text = "0%";
                            _stepText.text = "downloading......";
                            _currentStep.CallBack();
                        });
                    }
                    break;
                case UpdateState.RestartClient:
                    Helper.ShowMessageBox("Restart game", () =>
                    {
                        _currentStep.CallBack();
                    });
                    break;
                case UpdateState.ReloadConfigs:
                    DataManager.Instance.LoadClientData();
                    break;
                case UpdateState.UpdateFinish:
                    UIManager.OpenWindow<ConnectServerWindow>();
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
            string tip = "Error on state :" + _currentStep.State + "\nError code : " + errorCode;
            Debugger.LogError(tip);
            switch (_currentStep.State)
            {
                case UpdateState.CheckNetWork:
                    tip = "No network!";
                    break;
            }
            Helper.ShowMessageBox(tip, () =>
            {
                _currentStep.CallBack();
            });
        }

        string GetSizeString(int size)
        {
            if (size > 1024 * 1024)
            {
                float gb = (float)size / 1024f / 1024f;
                return gb.ToString("F2") + "G";
            }

            if (size > 5 * 1024)
            {
                float mb = (float)size / 1024f;
                if (mb <= 10f)
                    mb /= 3f;
                else if (mb <= 20f)
                    mb /= 2.5f;
                else if (mb <= 50f)
                    mb /= 2f;
                else if (mb <= 150f)
                    mb /= 1.5f;
                else if (mb <= 250f)
                    mb /= 1.3f;
                else
                    mb /= 1.2f;
                return mb.ToString("F2") + "M";
            }

            float kb = size / 5;

            return ((int)(kb + 1)).ToString() + "K";
        }
    }
}
