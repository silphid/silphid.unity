using System;
using UniRx;

namespace Silphid.DataTypes
{
    public class RefCounter
    {
        private int _counter;
        private readonly Action _startAction;
        private readonly Action _endAction;

        public RefCounter(Action startAction, Action endAction)
        {
            _startAction = startAction;
            _endAction = endAction;
        }

        public IDisposable AddRef()
        {
            var isStart = false;

            lock (this)
            {
                if (_counter++ == 0)
                    isStart = true;
            }
            
            if (isStart)
                _startAction?.Invoke();
            
            return Disposable.Create(
                () =>
                {
                    var isEnd = false;
                    
                    lock (this)
                    {
                        if (--_counter == 0)
                            isEnd = true;
                    }
                    
                    if (isEnd)
                        _endAction?.Invoke();
                });
        }
    }
}