using System;
using UniRx;

namespace Silphid.Sequencit
{
    public class Instant : ICompletable
    {
        private readonly Action _action;
        public static readonly ICompletable Default = new Instant();

        public Instant(Action action = null)
        {
            _action = action;
        }

        public IDisposable Subscribe(ICompletableObserver observer)
        {
            try
            {
                _action?.Invoke();
            }
            catch (Exception ex)
            {
                return Completable.Throw(ex)
                                  .Subscribe(observer);
            }

            observer.OnCompleted();
            return Disposable.Empty;
        }
    }
}