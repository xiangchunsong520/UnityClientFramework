/*
auth: Xiang ChunSong
purpose:
*/

using System;
using System.Collections.Generic;
using Utils;

namespace Base
{
    public class TimerManager : Singleton<TimerManager>
    {
        List<Timer> _timers = new List<Timer>();
        List<Timer> _removed = new List<Timer>();

        static object threadLoack = new object();

        public void Update()
        {
            for (int i = 0; i < _timers.Count;)
            {
                Timer timer = _timers[i];
                if (timer == null)
                {
                    _timers.RemoveAt(i);
                }
                else if (_removed.Contains(timer) || timer.CheckExecute())
                {
                    _timers.RemoveAt(i);
                    lock (threadLoack)
                    {
                        _removed.Remove(timer);
                    }
                }
                else
                {
                    ++i;
                }
            }
        }

        public Timer AddDelayTimer(float delayTime, Action callback)
        {
            Timer timer = new DelayTimer(delayTime, (objs)=>callback(), null);
            _timers.Add(timer);
            return timer;
        }

        public Timer AddDelayTimer(float delayTime, Action<object[]> callback, params object[] args)
        {
            Timer timer = new DelayTimer(delayTime, callback, args);
            _timers.Add(timer);
            return timer;
        }

        public Timer AddRepeatTimer(float delayTime, float repeatTime, Action callback)
        {
            Timer timer = new RepeatTimer(delayTime, repeatTime, (objs)=>callback(), null);
            _timers.Add(timer);
            return timer;
        }

        public Timer AddRepeatTimer(float delayTime, float repeatTime, Action<object[]> callback, params object[] args)
        {
            Timer timer = new RepeatTimer(delayTime, repeatTime, callback, args);
            _timers.Add(timer);
            return timer;
        }

        public Timer AddFarmeTimer(int farmeCount, Action callback)
        {
            Timer timer = new FrameTimer(farmeCount, (objs)=>callback(), null);
            _timers.Add(timer);
            return timer;
        }

        public Timer AddFarmeTimer(int farmeCount, Action<object[]> callback, params object[] args)
        {
            Timer timer = new FrameTimer(farmeCount, callback, args);
            _timers.Add(timer);
            return timer;
        }

        public Timer AddFarmeRepeatTimer(int delayFrame, int repeatFrame, Action callback)
        {
            Timer timer = new FrameRepeatTimer(delayFrame, repeatFrame, (objs) => callback(), null);
            _timers.Add(timer);
            return timer;
        }

        public Timer AddFarmeRepeatTimer(int delayFrame, int repeatFrame, Action<object[]> callback, params object[] args)
        {
            Timer timer = new FrameRepeatTimer(delayFrame, repeatFrame, callback, args);
            _timers.Add(timer);
            return timer;
        }

        public void RemoveTimer(Timer timer)
        {
            lock (threadLoack)
            {
                _removed.Add(timer);
            }
        }
    }
}