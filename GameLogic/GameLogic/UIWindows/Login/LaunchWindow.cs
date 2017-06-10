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
