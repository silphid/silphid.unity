using System;
using UniRx.Completables;
using UniRx.Completables.InternalUtil;
using UniRx.InternalUtil;

namespace UniRx
{
    public sealed class CompletableSubject : ICompletableSubject, IDisposable, IOptimizedCompletable
    {
        private readonly object observerLock = new object();

        private bool isStopped;
        private bool isDisposed;
        private Exception lastError;
        private ICompletableObserver outObserver = EmptyCompletableObserver.Instance;

        public bool HasObservers
        {
            get { return !(outObserver is EmptyCompletableObserver) && !isStopped && !isDisposed; }
        }

        public void OnCompleted()
        {
            ICompletableObserver old;
            lock (observerLock)
            {
                ThrowIfDisposed();
                if (isStopped)
                    return;

                old = outObserver;
                outObserver = EmptyCompletableObserver.Instance;
                isStopped = true;
            }

            old.OnCompleted();
        }

        public void OnError(Exception error)
        {
            if (error == null)
                throw new ArgumentNullException("error");

            ICompletableObserver old;
            lock (observerLock)
            {
                ThrowIfDisposed();
                if (isStopped)
                    return;

                old = outObserver;
                outObserver = EmptyCompletableObserver.Instance;
                isStopped = true;
                lastError = error;
            }

            old.OnError(error);
        }

        public IDisposable Subscribe(ICompletableObserver observer)
        {
            if (observer == null)
                throw new ArgumentNullException("observer");

            Exception ex;

            lock (observerLock)
            {
                ThrowIfDisposed();
                if (!isStopped)
                {
                    var listObserver = outObserver as ListCompletableObserver;
                    if (listObserver != null)
                    {
                        outObserver = listObserver.Add(observer);
                    }
                    else
                    {
                        var current = outObserver;
                        if (current is EmptyCompletableObserver)
                        {
                            outObserver = observer;
                        }
                        else
                        {
                            outObserver = new ListCompletableObserver(
                                new ImmutableList<ICompletableObserver>(new[] { current, observer }));
                        }
                    }

                    return new Subscription(this, observer);
                }

                ex = lastError;
            }

            if (ex != null)
            {
                observer.OnError(ex);
            }
            else
            {
                observer.OnCompleted();
            }

            return Disposable.Empty;
        }

        public void Dispose()
        {
            lock (observerLock)
            {
                isDisposed = true;
                outObserver = DisposedCompletableObserver.Instance;
            }
        }

        private void ThrowIfDisposed()
        {
            if (isDisposed)
                throw new ObjectDisposedException("");
        }

        public bool IsRequiredSubscribeOnCurrentThread()
        {
            return false;
        }

        private class Subscription : IDisposable
        {
            readonly object gate = new object();
            CompletableSubject parent;
            ICompletableObserver unsubscribeTarget;

            public Subscription(CompletableSubject parent, ICompletableObserver unsubscribeTarget)
            {
                this.parent = parent;
                this.unsubscribeTarget = unsubscribeTarget;
            }

            public void Dispose()
            {
                lock (gate)
                {
                    if (parent != null)
                    {
                        lock (parent.observerLock)
                        {
                            var listObserver = parent.outObserver as ListCompletableObserver;
                            if (listObserver != null)
                            {
                                parent.outObserver = listObserver.Remove(unsubscribeTarget);
                            }
                            else
                            {
                                parent.outObserver = EmptyCompletableObserver.Instance;
                            }

                            unsubscribeTarget = null;
                            parent = null;
                        }
                    }
                }
            }
        }
    }
}