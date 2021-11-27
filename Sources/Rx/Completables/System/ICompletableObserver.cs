using System;

namespace UniRx
{
    public interface ICompletableObserver
    {
        void OnError(Exception error);
        void OnCompleted();
    }
}