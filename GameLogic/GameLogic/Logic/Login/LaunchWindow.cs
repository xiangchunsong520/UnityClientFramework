﻿using System;
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
        Text _TextVer;
        GameObject _button;
        
        protected override void OnSetWindowDetail()
        {
            Settings.PrefabName = "UI/Install/LaunchWindow";
        }

        protected override void OnInit()
        {
            _progress = GetChildComponent<Slider>("Progress");
            _progressText = GetChildComponent<Text>("ProgressText");
            _stepText = GetChildComponent<Text>("StepText");
            _TextVer = GetChildComponent<Text>("TextVer");
            _button = GetChildGameObject("Button");
            EventTriggerListener.Get(_button).onClick = OnClickStart;
            _button.SetActive(false);
        }

        protected override void OnOpen(object[] args)
        {
            _TextVer.text = LogicMain.version + "." + DataManager.Instance.clientConfig.data.ResVersion;
            _progress.gameObject.SetActive(true);
            _progress.value = 0;
            _progressText.text = "0%";
            _stepText.text = "Checking......";
            /*
            UpdateStep us = new UpdateStep();
            us.State = UpdateState.UpdateFinish;
            OnShowUpdateStep(us);
            return;
            //*/
            TimerManager.Instance.AddDelayTimer(2, () =>
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
                    _progress.gameObject.SetActive(true);
                    _progress.value = 0;
                    _progressText.text = "0%";
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
                    _progress.value = 1;
                    _progressText.text = "100%";
                    _stepText.text = "Checking finish!";
                    TimerManager.Instance.AddFarmeTimer(1, () =>
                    {
                        //UIManager.OpenWindow<ConnectServerWindow>();
                        _progress.gameObject.SetActive(false);
                        _progressText.text = "";
                        _stepText.text = "";
                        _button.SetActive(true);
                    });
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

            //*
            UpdateStep us = new UpdateStep();
            us.State = UpdateState.UpdateFinish;
            OnShowUpdateStep(us);
            return;
            //*/

            Helper.ShowMessageBox(tip, () =>
            {
                _currentStep.CallBack();
            });
        }

        void OnClickStart(GameObject go)
        {
            UIManager.OpenWindow<MainWindow>();
        }

        string GetSizeString(int size)
        {
            if (size > 1024 * 1024)
            {
                float gb = (float)size / 1024f / 1024f;
                return gb.ToString("F2") + "G";
            }

            if (size > 1024)
            {
                float mb = (float)size / 1024f;
                return mb.ToString("F2") + "M";
            }

            float kb = size;

            return ((int)(kb + 1)).ToString() + "K";
        }
    }
}
