/*
auth: Xiang ChunSong
purpose:
*/

using System;

namespace Base
{
    public class Timer
    {
        Action<object[]> _callback;
        object[] _args;
        
        public virtual bool CheckExecute()
        {
            throw new NotImplementedException();
        }

        public Timer(Action<object[]> callback, object[] args)
        {
            _callback = callback;
            _args = args;
        }

        protected void Execute()
        {
            if (_callback != null)
            {
                try
                {
                    _callback(_args);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
    }

    public class DelayTimer : Timer
    {
        DateTime _executeTime;

        public DelayTimer(float delayTime, Action<object[]> callback, object[] args) : base(callback, args)
        {
            _executeTime = DateTime.Now + new TimeSpan((long)(TimeSpan.TicksPerSecond * delayTime));
        }

        public override bool CheckExecute()
        {
            if (DateTime.Now >= _executeTime)
            {
                Execute();
                return true;
            }
            return false;
        }
    }

    public class RepeatTimer : Timer
    {
        DateTime _executeTime;
        TimeSpan _repeatTime;

        public RepeatTimer(float delayTime, float repeatTime, Action<object[]> callback, object[] args) : base(callback, args)
        {
            _executeTime = DateTime.Now + new TimeSpan((long)(TimeSpan.TicksPerSecond * delayTime));
            _repeatTime = new TimeSpan((long)(TimeSpan.TicksPerSecond * repeatTime)); ;
        }

        public override bool CheckExecute()
        {
            if (DateTime.Now >= _executeTime)
            {
                Execute();
                _executeTime = _executeTime + _repeatTime;
            }
            return false;
        }
    }

    public class FrameTimer : Timer
    {
        int _executeFrame;
        int _curFrame;

        public FrameTimer(int delayFrame, Action<object[]> callback, object[] args) : base(callback, args)
        {
            _executeFrame = delayFrame;
            _curFrame = 0;
        }

        public override bool CheckExecute()
        {
            if (++_curFrame >= _executeFrame)
            {
                Execute();
                return true;
            }
            return false;
        }
    }

    public class FrameRepeatTimer : Timer
    {
        int _executeFrame;
        int _repeatFrame;
        int _curFrame;

        public FrameRepeatTimer(int delayFrame, int repeatFrame, Action<object[]> callback, object[] args) : base(callback, args)
        {
            _executeFrame = delayFrame;
            _repeatFrame = repeatFrame;
            _curFrame = 0;
        }

        public override bool CheckExecute()
        {
            if (++_curFrame >= _executeFrame)
            {
                Execute();
                _executeFrame = _repeatFrame;
                _curFrame = 0;
            }
            return false;
        }
    }
}