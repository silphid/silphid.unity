using System;

namespace UniRx
{
    public interface ICompletable
    {
        IDisposable Subscribe(ICompletableObserver observer);
    }
}