using System;
using UniRx;

namespace Silphid.Sequencit
{
    public class Instant : IObservable<Unit>
    {
        private readonly Action _action;
        public static readonly IObservable<Unit> Default = new Instant();

        public Instant(Action action = null)
        {
            _action = action;
        }

        public IDisposable Subscribe(IObserver<Unit> observer)
        {
            try
            {
                _action?.Invoke();
            }
            catch (Exception ex)
            {
                return Observable
                    .Throw<Unit>(ex)
                    .Subscribe(observer);
            }

            observer.OnCompleted();
            return Disposable.Empty;
        }
    }
}