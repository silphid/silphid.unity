using System;
using UniRx.Completables;
using UniRx.Completables.InternalUtil;
using UniRx.InternalUtil;

#if (NET_4_6 || NET_STANDARD_2_0)
using System.Runtime.CompilerServices;
using System.Threading;
#endif

namespace UniRx
{
    public sealed class AsyncCompletableSubject : ICompletableSubject, IOptimizedCompletable, IDisposable
#if (NET_4_6 || NET_STANDARD_2_0)
        , INotifyCompletion
#endif
    {
        private readonly object _observerLock = new object();

        private bool _isDisposed;
        private Exception _lastError;
        private ICompletableObserver _outObserver = EmptyCompletableObserver.Instance;

        public bool HasObservers => !(_outObserver is EmptyCompletableObserver) && !IsCompleted && !_isDisposed;
        public bool IsCompleted { get; private set; }

        public void OnCompleted()
        {
            ICompletableObserver old;
            lock (_observerLock)
            {
                ThrowIfDisposed();
                if (IsCompleted) return;

                old = _outObserver;
                _outObserver = EmptyCompletableObserver.Instance;
                IsCompleted = true;
            }
            old.OnCompleted();
        }

        public void OnError(Exception error)
        {
            if (error == null) throw new ArgumentNullException(nameof(error));

            ICompletableObserver old;
            lock (_observerLock)
            {
                ThrowIfDisposed();
                if (IsCompleted) return;

                old = _outObserver;
                _outObserver = EmptyCompletableObserver.Instance;
                IsCompleted = true;
                _lastError = error;
            }

            old.OnError(error);
        }

        public IDisposable Subscribe(ICompletableObserver observer)
        {
            if (observer == null) throw new ArgumentNullException(nameof(observer));

            Exception ex;
            lock (_observerLock)
            {
                ThrowIfDisposed();
                if (!IsCompleted)
                {
                    if (_outObserver is ListCompletableObserver listObserver)
                    {
                        _outObserver = listObserver.Add(observer);
                    }
                    else
                    {
                        var current = _outObserver;
                        _outObserver = current is EmptyCompletableObserver
                            ? observer
                            : new ListCompletableObserver(
                        new ImmutableList<ICompletableObserver>(new[] { current, observer }));
                    }

                    return new Subscription(this, observer);
                }

                ex = _lastError;
            }

            if (ex != null)
            {
                observer.OnError(ex);
            }
            observer.OnCompleted();

            return Disposable.Empty;
        }

        public void Dispose()
        {
            lock (_observerLock)
            {
                _isDisposed = true;
                _outObserver = DisposedCompletableObserver.Instance;
                _lastError = null;
            }
        }

        internal void ThrowIfDisposed()
        {
            if (_isDisposed) throw new ObjectDisposedException("");
        }

        public bool IsRequiredSubscribeOnCurrentThread()
        {
            return false;
        }

        internal class Subscription : IDisposable
        {
            private readonly object _gate = new object();
            private AsyncCompletableSubject _parent;
            private ICompletableObserver _unsubscribeTarget;

            public Subscription(AsyncCompletableSubject parent, ICompletableObserver unsubscribeTarget)
            {
                _parent = parent;
                _unsubscribeTarget = unsubscribeTarget;
            }

            public void Dispose()
            {
                lock (_gate)
                {
                    if (_parent != null)
                    {
                        lock (_parent._observerLock)
                        {
                            _parent._outObserver = _parent._outObserver is ListCompletableObserver listObserver
                                ? listObserver.Remove(_unsubscribeTarget)
                                : EmptyCompletableObserver.Instance;

                            _unsubscribeTarget = null;
                            _parent = null;
                        }
                    }
                }
            }
        }


#if (NET_4_6 || NET_STANDARD_2_0)

        /// <summary>
        /// Gets an awaitable object for the current AsyncSubject.
        /// </summary>
        /// <returns>Object that can be awaited.</returns>
        public AsyncCompletableSubject GetAwaiter()
        {
            return this;
        }

        /// <summary>
        /// Specifies a callback action that will be invoked when the subject completes.
        /// </summary>
        /// <param name="continuation">Callback action that will be invoked when the subject completes.</param>
        /// <exception cref="ArgumentNullException"><paramref name="continuation"/> is null.</exception>
        public void OnCompleted(Action continuation)
        {
            if (continuation == null)
                throw new ArgumentNullException(nameof(continuation));

            OnCompleted(continuation, true);
        }

         void OnCompleted(Action continuation, bool originalContext)
        {
            //
            // [OK] Use of unsafe Subscribe: this type's Subscribe implementation is safe.
            //
            this.Subscribe/*Unsafe*/(new AwaitObserver(continuation, originalContext));
        }

        class AwaitObserver : ICompletableObserver
        {
            private readonly SynchronizationContext _context;
            private readonly Action _callback;

            public AwaitObserver(Action callback, bool originalContext)
            {
                if (originalContext)
                    _context = SynchronizationContext.Current;

                _callback = callback;
            }

            public void OnCompleted()
            {
                InvokeOnOriginalContext();
            }

            public void OnError(Exception error)
            {
                InvokeOnOriginalContext();
            }

            private void InvokeOnOriginalContext()
            {
                if (_context != null)
                {
                    //
                    // No need for OperationStarted and OperationCompleted calls here;
                    // this code is invoked through await support and will have a way
                    // to observe its start/complete behavior, either through returned
                    // Task objects or the async method builder's interaction with the
                    // SynchronizationContext object.
                    //
                    _context.Post(c => ((Action)c)(), _callback);
                }
                else
                {
                    _callback();
                }
            }
        }

        /// <summary>
        /// Ensure termination of completable, potentially blocking until the subject completes successfully or exceptionally.
        /// </summary>
        /// <exception cref="InvalidOperationException">The source sequence is empty.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Await pattern for C# and VB compilers.")]
        public void GetResult()
        {
            if (!IsCompleted)
            {
                var e = new ManualResetEvent(false);
                OnCompleted(() => e.Set(), false);
                e.WaitOne();
            }

            if (_lastError != null)
            {
                throw _lastError;
            }
        }
#endif
    }
}
