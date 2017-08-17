using System;
using UniRx;

namespace Silphid.Sequencit
{
    public class Instant : IObservable<Unit>
    {
        public static readonly IObservable<Unit> Default = new Instant();
        
        public IDisposable Subscribe(IObserver<Unit> observer)
        {
            observer.OnCompleted();
            return Disposable.Empty;
        }
    }
}