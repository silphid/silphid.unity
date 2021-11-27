using System;
using System.Collections.Generic;

namespace UniRx.Completables.Operators
{
    internal class CatchCompletable<TException> : OperatorCompletableBase where TException : Exception
    {
        private readonly ICompletable source;
        private readonly Func<TException, ICompletable> errorHandler;

        public CatchCompletable(ICompletable source, Func<TException, ICompletable> errorHandler)
            : base(source.IsRequiredSubscribeOnCurrentThread())
        {
            this.source = source;
            this.errorHandler = errorHandler;
        }

        protected override IDisposable SubscribeCore(ICompletableObserver observer, IDisposable cancel)
        {
            return new CatchObserver(this, observer, cancel).Run();
        }

        private class CatchObserver : OperatorCompletableObserverBase
        {
            private readonly CatchCompletable<TException> parent;
            private SingleAssignmentDisposable sourceSubscription;
            private SingleAssignmentDisposable exceptionSubscription;

            public CatchObserver(CatchCompletable<TException> parent, ICompletableObserver observer, IDisposable cancel)
                : base(observer, cancel)
            {
                this.parent = parent;
            }

            public IDisposable Run()
            {
                sourceSubscription = new SingleAssignmentDisposable();
                exceptionSubscription = new SingleAssignmentDisposable();

                sourceSubscription.Disposable = parent.source.Subscribe(this);
                return StableCompositeDisposable.Create(sourceSubscription, exceptionSubscription);
            }

            public override void OnError(Exception error)
            {
                var e = error as TException;
                if (e != null)
                {
                    ICompletable next;
                    try
                    {
                        next = parent.errorHandler == Stubs.CatchIgnore
                                   ? Completable.Empty()
                                   : parent.errorHandler(e);
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            observer.OnError(ex);
                        }
                        finally
                        {
                            Dispose();
                        }

                        return;
                    }

                    exceptionSubscription.Disposable = next.Subscribe(observer);
                }
                else
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

    internal class CatchCompletable : OperatorCompletableBase
    {
        private readonly IEnumerable<ICompletable> sources;

        public CatchCompletable(IEnumerable<ICompletable> sources)
            : base(true)
        {
            this.sources = sources;
        }

        protected override IDisposable SubscribeCore(ICompletableObserver observer, IDisposable cancel)
        {
            return new CatchObserver(this, observer, cancel).Run();
        }

        private class CatchObserver : OperatorCompletableObserverBase
        {
            private readonly CatchCompletable parent;
            private readonly object gate = new object();
            private bool isDisposed;
            private IEnumerator<ICompletable> e;
            private SerialDisposable subscription;
            private Exception lastException;
            private Action nextSelf;

            public CatchObserver(CatchCompletable parent, ICompletableObserver observer, IDisposable cancel)
                : base(observer, cancel)
            {
                this.parent = parent;
            }

            public IDisposable Run()
            {
                isDisposed = false;
                e = parent.sources.GetEnumerator();
                subscription = new SerialDisposable();

                var schedule = Scheduler.DefaultSchedulers.TailRecursion.Schedule(RecursiveRun);

                return StableCompositeDisposable.Create(
                    schedule,
                    subscription,
                    Disposable.Create(
                        () =>
                        {
                            lock (gate)
                            {
                                isDisposed = true;
                                e.Dispose();
                            }
                        }));
            }

            private void RecursiveRun(Action self)
            {
                lock (gate)
                {
                    nextSelf = self;
                    if (isDisposed)
                        return;

                    var current = default(ICompletable);
                    var hasNext = false;
                    var ex = default(Exception);

                    try
                    {
                        hasNext = e.MoveNext();
                        if (hasNext)
                        {
                            current = e.Current;
                            if (current == null)
                                throw new InvalidOperationException("sequence is null.");
                        }
                        else
                        {
                            e.Dispose();
                        }
                    }
                    catch (Exception exception)
                    {
                        ex = exception;
                        e.Dispose();
                    }

                    if (ex != null)
                    {
                        try
                        {
                            observer.OnError(ex);
                        }
                        finally
                        {
                            Dispose();
                        }

                        return;
                    }

                    if (!hasNext)
                    {
                        if (lastException != null)
                        {
                            try
                            {
                                observer.OnError(lastException);
                            }
                            finally
                            {
                                Dispose();
                            }
                        }
                        else
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

                        return;
                    }

                    var source = current;
                    var d = new SingleAssignmentDisposable();
                    subscription.Disposable = d;
                    d.Disposable = source.Subscribe(this);
                }
            }

            public override void OnError(Exception error)
            {
                lastException = error;
                nextSelf();
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
}