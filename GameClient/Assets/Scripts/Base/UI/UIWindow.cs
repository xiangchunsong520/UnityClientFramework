/*
auth: Xiang ChunSong
purpose:
*/

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIObject
{
    GameObject _gameObject;

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

    protected T GetComponen<T>() where T : Component
    {
        return _gameObject.GetComponent<T>();
    }

    protected T GetChildComponent<T>(string childPath) where T : Component
    {
        Transform ts = GetChildTransform(childPath);
        if (ts)
            return ts.GetComponent<T>();

        return null;
    }
}

public class UIChildWindow
{

}

public class UIWindow
{

}