using System;
using System.Threading;
using UniRx.Completables.InternalUtil;
using UniRx.InternalUtil;

namespace UniRx.Completables.Operators
{
    internal abstract class OperatorCompletableObserverBase : IDisposable, ICompletableObserver
    {
        protected internal volatile ICompletableObserver observer;
        private IDisposable cancel;

        protected OperatorCompletableObserverBase(ICompletableObserver observer, IDisposable cancel)
        {
            this.observer = observer;
            this.cancel = cancel;
        }

        public abstract void OnCompleted();

        public abstract void OnError(Exception error);

        public void Dispose()
        {
            observer = EmptyCompletableObserver.Instance;
            var target = Interlocked.Exchange(ref cancel, null);
            if (target != null)
                target.Dispose();
        }
    }

    internal abstract class OperatorObservableToCompletableObserverBase<T> : IDisposable, IObserver<T>
    {
        protected internal volatile ICompletableObserver observer;
        private IDisposable cancel;

        protected OperatorObservableToCompletableObserverBase(ICompletableObserver observer, IDisposable cancel)
        {
            this.observer = observer;
            this.cancel = cancel;
        }

        public abstract void OnNext(T value);

        public abstract void OnError(Exception error);

        public abstract void OnCompleted();

        public void Dispose()
        {
            observer = EmptyCompletableObserver.Instance;
            var target = Interlocked.Exchange(ref cancel, null);
            if (target != null)
                target.Dispose();
        }
    }

    internal abstract class OperatorCompletableToObservableObserverBase<T> : IDisposable, ICompletableObserver
    {
        protected internal volatile IObserver<T> observer;
        private IDisposable cancel;

        protected OperatorCompletableToObservableObserverBase(IObserver<T> observer, IDisposable cancel)
        {
            this.observer = observer;
            this.cancel = cancel;
        }

        public abstract void OnError(Exception error);

        public abstract void OnCompleted();

        public void Dispose()
        {
            observer = EmptyObserver<T>.Instance;
            var target = Interlocked.Exchange(ref cancel, null);
            if (target != null)
                target.Dispose();
        }
    }

    internal class PassthroughCompletableObserver : OperatorCompletableObserverBase
    {
        public PassthroughCompletableObserver(ICompletableObserver observer, IDisposable cancel)
            : base(observer, cancel) {}

        public override void OnError(Exception error)
        {
            try
            {
                observer.OnError(error);
            }
            finally
            {
                Dispose();
            }
        }

        public override void OnCompleted()
        {
            try
            {
                observer.OnCompleted();
            }
            finally
            {
                Dispose();
            }
        }
    }

    internal class PassthroughCompletableToObservableObserver<T> : OperatorCompletableToObservableObserverBase<T>
    {
        public PassthroughCompletableToObservableObserver(IObserver<T> observer, IDisposable cancel)
            : base(observer, cancel) {}

        public override void OnError(Exception error)
        {
            try
            {
                observer.OnError(error);
            }
            finally
            {
                Dispose();
            }
        }

        public override void OnCompleted()
        {
            try
            {
                observer.OnCompleted();
            }
            finally
            {
                Dispose();
            }
        }
    }

    internal class PassthroughObservableToCompletableObserver<T> : OperatorObservableToCompletableObserverBase<T>
    {
        public PassthroughObservableToCompletableObserver(ICompletableObserver observer, IDisposable cancel)
            : base(observer, cancel) {}

        public override void OnNext(T value)
        {
            // Ignore values
        }

        public override void OnError(Exception error)
        {
            try
            {
                observer.OnError(error);
            }
            finally
            {
                Dispose();
            }
        }

        public override void OnCompleted()
        {
            try
            {
                observer.OnCompleted();
            }
            finally
            {
                Dispose();
            }
        }
    }
}