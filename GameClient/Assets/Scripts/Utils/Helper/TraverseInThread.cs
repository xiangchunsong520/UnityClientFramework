/*
auth: Xiang ChunSong
purpose:适用于遍历超大容器且要执行非常耗时的操作的情况,将操作分散在线程中以缩短遍历时间
*/

using System.Collections.Generic;
using System;
using System.Threading;
using Base;

class TraverseThrad
{
    bool _runing;
    List<object> _tempObjs = new List<object>();
    Action<object> _onAction;
    Thread _thread;
    ManualResetEvent _haveDataEvent;
    object _lockObj = new object();

    public bool finish
    {
        get
        {
            return _tempObjs.Count == 0;
        }
    }

    public TraverseThrad(Action<object> onAction)
    {
        _runing = true;
        _onAction = onAction;
        _haveDataEvent = new ManualResetEvent(false);
        _thread = new Thread(Run);
        _thread.Start();
    }

    public void DoAction(object obj)
    {
        if (obj == null)
        {
            Debugger.LogError("TraverseDictionaryInThread.TraverseThrad.DoAction obj is null!");
            return;
        }

        lock (_lockObj)
        {
            _tempObjs.Add(obj);
        }
        _haveDataEvent.Set();
    }

    public void Dispose()
    {
        _runing = false;
        _haveDataEvent.Set();
    }

    void Run()
    {
        while (_runing)
        {
            if (_haveDataEvent.WaitOne())
            {
                _haveDataEvent.Reset();

                int num = 0;
                lock (_lockObj)
                {
                    num = _tempObjs.Count;
                }
                for (int i = 0; i < num; ++i)
                {
                    try
                    {
                        _onAction(_tempObjs[0]);
                    }
                    catch (Exception ex)
                    {
                        Debugger.LogError(ex);
                    }
                    lock (_lockObj)
                    {
                        _tempObjs.RemoveAt(0);
                    }
                }
            }
            Thread.Sleep(1);
        }
    }
}

public class TraverseInThread<T>
{
    static int MAX_THREAD = 50;
    IEnumerable<T> _container;
    Action _onfinish;
    Action<object> _onAction;
    int _count;

    public TraverseInThread(IEnumerable<T> dict, int count, Action<object> onAction, Action onfinish)
    {
        _container = dict;
        _count = count;
        _onfinish = onfinish;
        _onAction = onAction;
        Thread thread = new Thread(Traverse);
        thread.Start();
    }

    void Traverse()
    {
        List<TraverseThrad> list = new List<TraverseThrad>();
        int num = _count > MAX_THREAD ? MAX_THREAD : _count;
        int i = 0;
        for (; i < num; ++i)
        {
            list.Add(new TraverseThrad(_onAction));
        }

        //Debugger.Log("Traverse 0");

        var d = _container.GetEnumerator();
        i = 0;
        while (d.MoveNext())
        {
            list[i].DoAction(d.Current);
            if (++i >= num)
            {
                i = 0;
            }
        }

        //Debugger.Log("Traverse 1");

        while (true)
        {
            bool allfinish = true;
            for (int j = 0; j < num; ++j)
            {
                if (!list[j].finish)
                {
                    allfinish = false;
                    break;
                }
            }

            if (allfinish)
            {
                break;
            }
            Thread.Sleep(1);
        }

        //Debugger.Log("Traverse 2");

        i = 0;
        for (; i < num; ++i)
        {
            try
            {
                list[0].Dispose();
            }
            catch (Exception ex)
            {
                Debugger.LogError(ex);
            }
        }

        //Debugger.Log("Traverse 3");

        TimerManager.Instance.AddFarmeTimer(1, () =>
        {
            _onfinish();
        });
    }
}