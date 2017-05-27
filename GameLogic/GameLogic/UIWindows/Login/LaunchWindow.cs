using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Base;
using UnityEngine;

namespace GameLogic
{
    class LaunchWindow : UIWindow
    {
        UpdateStep _currentStep;
        protected override void OnOpen(object[] args)
        {
            //Invoke(1.5f, WaiteFinish);
            ClinetUpdateManager.Instance.Start(OnShowUpdateStep, OnShowUpdateProgress, OnShowUpdateStepFail);
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
