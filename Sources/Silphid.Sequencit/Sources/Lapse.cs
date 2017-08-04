using System;
using UniRx;
using Rx = UniRx;

namespace Silphid.Sequencit
{
    /// <summary>
    /// An observable that completes only when it is disposed.
    /// </summary>
    public class Lapse : IObservable<Unit>, IDisposable
    {
        public static Lapse Create(Action<IDisposable> action)
        {
            var lapse = new Lapse();
            action(lapse);
            return lapse;
        }

        private bool _isDisposed;
        private Subject<Unit> _subject;

        public IDisposable Subscribe(IObserver<Unit> observer)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(Lapse));

            if (_subject == null)
                _subject = new Subject<Unit>();

            return _subject.Subscribe(observer);
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            if (_subject != null)
            {
                _subject.OnCompleted();
                _subject = null;
            }

            _isDisposed = true;
        }
    }
}