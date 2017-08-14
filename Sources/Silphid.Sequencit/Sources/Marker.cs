using System;
using UniRx;

namespace Silphid.Sequencit
{
    public class Marker : IObservable<Unit>
    {
        public IDisposable Subscribe(IObserver<Unit> observer)
        {
            observer.OnCompleted();
            return Disposable.Empty;
        }
    }
}