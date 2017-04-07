/*
auth: Xiang ChunSong
purpose:
*/

using UnityEngine;
//using UnityEngine.UI;
using System.Collections;
using System;
using Data;
using Base;
using System.Collections.Generic;

namespace GameLogic
{
    #region UIObject
    public class UIObject
    {
        protected GameObject _gameObject;
        MonoBehaviour _mono = null;
        Canvas _canvas = null;
        Camera _camera = null;
        Timer _updateTimer;
        List<Timer> _invokTimers = new List<Timer>();

        MonoBehaviour Mono
        {
            get
            {
                if (!_mono)
                {
                    _mono = _gameObject.GetComponent<MonoBehaviour>();
                }

                if (!_mono)
                {
                    _mono = _gameObject.AddComponent<MonoBehaviour>();
                }

                return _mono;
            }
        }

        public UIObject()
        {

        }

        public UIObject(GameObject go)
        {
            SetGameObject(go);
        }

        public virtual bool OnUpdate()
        {
            throw new NotImplementedException();
        }

        public void SetCanvas(Canvas canvas)
        {
            _canvas = canvas;
        }

        public void SetCamera(Camera camera)
        {
            _camera = camera;
        }

        void Update()
        {
            if (OnUpdate())
            {
                CancelInvoke(_updateTimer);
            }
        }

        protected void Show()
        {
            if (_canvas)
            {
                if (_camera)
                {
                    _canvas.worldCamera = _camera;
                }
                else
                {
                    _canvas.enabled = true;
                }
            }
            else
            {
                _gameObject.SetActive(true);
            }
        }

        protected void Hide()
        {
            if (_canvas)
            {
                if (_camera)
                {
                    _canvas.worldCamera = UIManager.Instance.HideCamera;
                }
                else
                {
                    _canvas.enabled = false;
                }
            }
            else
            {
                _gameObject.SetActive(false);
            }

            if (_updateTimer != null)
            {
                CancelInvoke(_updateTimer);
                _updateTimer = null;
            }
            for (int i = 0; i < _invokTimers.Count; ++i)
            {
                CancelInvoke(_invokTimers[i]);
            }
            _invokTimers.Clear();
        }

        protected void SetGameObject(GameObject go)
        {
            _gameObject = go;
            _canvas = GetComponen<Canvas>();
        }

        protected void StartUpdate(int frameCount = 1)
        {
            _updateTimer = TimerManager.Instance.AddFarmeRepeatTimer(1, frameCount, Update);
        }

        protected Transform GetChildTransform(string childPath)
        {
            return _gameObject.transform.FindChild(childPath);
        }

        protected GameObject GetChildGameObject(string childPath)
        {
            Transform ts = GetChildTransform(childPath);
            if (ts)
                return ts.gameObject;

            return null;
        }

        protected T GetComponen<T>()
        {
            return _gameObject.GetComponent<T>();
        }

        protected T GetChildComponent<T>(string childPath)
        {
            Transform ts = GetChildTransform(childPath);
            if (ts)
                return ts.GetComponent<T>();

            return default(T);
        }

        protected T GetComponentInParent<T>()
        {
            return _gameObject.GetComponentInParent<T>();
        }

        protected Coroutine StartCorention(IEnumerator routine)
        {
            return Mono.StartCoroutine(routine);
        }

        protected void StopCoroutine(IEnumerator routine)
        {
            Mono.StopCoroutine(routine);
        }

        protected void StopCoroutine(Coroutine routine)
        {
            Mono.StopCoroutine(routine);
        }

        protected void StopAllCoroutines()
        {
            Mono.StopAllCoroutines();
        }

        protected Timer Invoke(float delayTime, Action callback)
        {
            Timer timer = TimerManager.Instance.AddDelayTimer(delayTime, callback);
            _invokTimers.Add(timer);
            return timer;
        }

        protected Timer InvokeRepeat(float delayTime, float repeatTime, Action callback)
        {
            Timer timer = TimerManager.Instance.AddRepeatTimer(delayTime, repeatTime, callback);
            _invokTimers.Add(timer);
            return timer;
        }

        protected void CancelInvoke(Timer timer)
        {
            TimerManager.Instance.RemoveTimer(timer);
            _invokTimers.Remove(timer);
        }
    }
    #endregion

    #region UIChildWindow
    public class UIChildWindow : UIObject
    {
        public GameObject Root
        {
            get
            {
                return _gameObject;
            }
        }

        public UIChildWindow(GameObject go) : base(go)
        {

        }
    }
    #endregion

    #region UIWindwo
    public class UIWindow : UIObject
    {
        bool _onopeninit = false;

        public GameObject Root
        {
            set
            {
                SetGameObject(value);
            }
            get
            {
                return _gameObject;
            }
        }

        public WindowConfig ConfigData
        {
            set;
            get;
        }

        public object[] Param
        {
            get;
            set;
        }

        protected virtual void OnInit()
        {

        }

        protected virtual void OnRelease()
        {

        }

        protected virtual void OnOpen(object[] args)
        {

        }

        protected virtual void OnClose()
        {

        }

        public void Init()
        {
            OnInit();
        }

        public void Release()
        {
            OnRelease();
        }

        public bool Open(object[] args)
        {
            if (!Root)
            {
                Debugger.LogError("Open window " + ConfigData.WinName + " fail!!");
                return false;
            }

            //bool waiteOpen = false;
            Show();
            if (!_onopeninit)
            {
                Param = args;
                OnOpen(args);
                _onopeninit = true;
                //waiteOpen = true;
                //if (GuideManager.Instance.IsGuiding && !_configData.openEffect)
                //    TimerManager.Instance.AddFarmeTimer(2, WaiteOpen);
            }
            if (ConfigData.OpenEffect)
            {
                Animation anim = Root.GetComponent<Animation>();
                if (anim == null)
                {
                    anim = Root.AddComponent<Animation>();
                    //anim.AddClip(Common.ResourceManager.Load<AnimationClip>("UI/winopen1.anim"), "winopen1");
                }
                else
                {

                }
                anim.Play("winopen1");
                //if (GuideManager.Instance.IsGuiding && waiteOpen)
                //    TimerManager.Instance.AddTimer(anim.GetClip("winopen1").length /*+ .1f*/, WaiteOpen);
            }
            //TimerManager.Instance.DelTimer(TweenFinish);
            //GL.Clear(false, true, Color.black);
            return true;
        }

        public bool Close()
        {
            if (!Root)
            {
                Debugger.LogError("Close window " + ConfigData.WinName + " fail!!");
                return false;
            }
            _onopeninit = false;
            //TimerManager.Instance.DelTimer(WaiteOpen);
            if (!ConfigData.OpenEffect)
            {
                OnClose();
                //if (GuideManager.Instance.IsGuiding)
                //    ClientMsgDispatcher.Instance.Dispatch(MsgHandle.MH_Guide, MsgAction.MA_GuideTrigger, GuideTriggerType.GTT_CloseWindow, _configData.winName);
                Hide();
                if (ConfigData.CloseDelete)
                    GameObject.DestroyImmediate(Root);
            }
            else
            {
                Animation anim = Root.GetComponent<Animation>();
                if (anim == null)
                {
                    anim = Root.AddComponent<Animation>();
                    //anim.AddClip(Common.ResourceManager.Load<AnimationClip>("UI/winclose.anim"), "winclose");
                }
                else
                {
                    //anim.AddClip(Common.ResourceManager.Load<AnimationClip>("UI/winclose.anim"), "winclose");
                }
                //anim.Play("winclose");
                //TimerManager.Instance.AddTimer(anim.GetClip("winclose").length /*+ .1f*/, TweenFinish);
            }
            //GL.Clear(false, true, Color.black);
            return true;
        }

        protected void CloseSelf()
        {
            if (ConfigData.IsRecord && !ConfigData.IsHover)
                UIManager.ReturnOpenWindow();
            else
                UIManager.CloseWindow(ConfigData.WinName);
        }
    }
    #endregion
}