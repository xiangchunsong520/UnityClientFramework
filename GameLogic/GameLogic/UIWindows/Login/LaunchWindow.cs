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
        RawImage image;
        UpdateStep _currentStep;
        SceneAsyncLoader loader;
        protected override void OnOpen(object[] args)
        {
            //Invoke(1.5f, WaiteFinish);
            ClinetUpdateManager.Instance.Start(OnShowUpdateStep, OnShowUpdateProgress, OnShowUpdateStepFail);
            image = GetChildComponent<RawImage>("RawImage");
            //image.texture = ResourceLoader.Load<Texture>("chongzhi_1.png");
            loader = SceneLoader.LoadSceneAsync("Community2");
            StartUpdate();
        }

        protected override bool OnUpdate()
        {
            //Debugger.Log(loader.Progress);
            if (loader.IsDone)
            {
                Debugger.LogError(1);
                //GameObject go = ResourceLoader.Load<GameObject>("Main Camera.prefab");
                //Debugger.LogError(go);
                //GameObject.Instantiate(go);
                CloseSelf();
            }
            return loader.IsDone;
        }

        void OnShowUpdateStep(UpdateStep step)
        {
            _currentStep = step;
            Debugger.Log(_currentStep.State);
            switch (_currentStep.State)
            {
                case UpdateState.CheckNetWork:
                    break;
            }
        }

        void OnShowUpdateProgress(UpdateProgress progress)
        {
            switch (_currentStep.State)
            {
                case UpdateState.CheckNetWork:
                    break;
            }
        }

        void OnShowUpdateStepFail(object obj)
        {
            Debugger.LogError(_currentStep.State);
            switch (_currentStep.State)
            {
                case UpdateState.CheckNetWork:
                    break;
            }
        }

        void WaiteFinish()
        {
            UIManager.OpenWindow("ConnectServerWindow");
        }
    }
}
