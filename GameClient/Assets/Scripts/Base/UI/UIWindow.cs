/*
auth: Xiang ChunSong
purpose:
*/

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class UIObject
{
    protected GameObject _gameObject;
    MonoBehaviour _mono = null;
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
    Canvas _canvas;
    Canvas _parentCanvas;

    public void Init()
    {
        OnInit();
    }

    public void Release()
    {
        OnRelease();
    }

    public bool Open(params object[] args)
    {
        return OnOpen(args);
    }

    public bool Close()
    {
        return OnClose();
    }

    public bool Update(float deltaTime)
    {
        return OnUpdate(deltaTime);
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

    public virtual bool OnUpdate(float deltaTime)
    {
        return true;
    }

    protected void SetGameObject(GameObject go)
    {
        _gameObject = go;
        _canvas = GetComponen<Canvas>();
        _parentCanvas = GetComponentInParent<Canvas>();
    }

    protected void StartUpdate()
    {
        throw new NotImplementedException();
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

    public UIChildWindow(GameObject go)
    {
        SetGameObject(go);
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
}