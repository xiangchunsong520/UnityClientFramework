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
    public class UIObject
    {
        protected GameObject _gameObject;
        MonoBehaviour _mono = null;
        Canvas _canvas = null;
        Canvas _parentCanvas = null;
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

        protected void Show()
        {

        }

        protected void Hide()
        {
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

        void Update()
        {
            if (OnUpdate())
            {
                CancelInvoke(_updateTimer);
            }
        }

        public virtual bool OnUpdate()
        {
            return true;
        }

        protected void SetGameObject(GameObject go)
        {
            _gameObject = go;
            _canvas = GetComponen<Canvas>();
            _parentCanvas = GetComponentInParent<Canvas>();
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

    public class UIWindow : UIObject
    {
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
            return OnOpen(args);
        }

        public bool Close()
        {
            return OnClose();
        }

        public virtual void OnInit()
        {

        }

        public virtual void OnRelease()
        {

        }

        public virtual bool OnOpen(object[] args)
        {
            return true;
        }

        public virtual bool OnClose()
        {
            return true;
        }

        protected void ReturnWindow()
        {

        }
    }
}