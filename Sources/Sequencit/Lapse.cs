using System;
using UniRx;
using Rx = UniRx;

namespace Silphid.Sequencit
{
    /// <summary>
    /// An observable that completes only when it is disposed.
    /// </summary>
    public class Lapse : ICompletable, IDisposable
    {
        private readonly Action<IDisposable> _action;
        private bool _isSubscribed;
        private bool _isDisposed;
        private CompletableSubject _subject;

        public static Lapse Create(Action<IDisposable> action = null) =>
            new Lapse(action);

        protected Lapse() {}

        protected Lapse(Action<IDisposable> action)
        {
            _action = action;
        }

        public IDisposable Subscribe(ICompletableObserver observer)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(Lapse));

            if (_subject == null)
                _subject = new CompletableSubject();

            if (!_isSubscribed)
            {
                _isSubscribed = true;
                _action?.Invoke(this);

                if (_isDisposed)
                {
                    observer.OnCompleted();
                    return Disposable.Empty;
                }
            }

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