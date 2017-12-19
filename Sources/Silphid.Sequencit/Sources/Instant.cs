using System;
using UniRx;

namespace Silphid.Sequencit
{
    public class Instant : ICompletable
    {
        public static readonly ICompletable Default = new Instant();
        
        public IDisposable Subscribe(ICompletableObserver observer)
        {
            observer.OnCompleted();
            return Disposable.Empty;
        }
    }
}