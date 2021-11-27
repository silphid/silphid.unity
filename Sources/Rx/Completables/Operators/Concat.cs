using System;
using System.Collections.Generic;
using System.Linq;

namespace UniRx.Completables.Operators
{
    internal class ConcatCompletable : OperatorCompletableBase
    {
        private readonly IEnumerable<ICompletable> sources;

        public ConcatCompletable(IEnumerable<ICompletable> sources)
            : base(true)
        {
            this.sources = sources;
        }

        public ICompletable Combine(IEnumerable<ICompletable> seconds)
        {
            return new ConcatCompletable(Completable.Combine(sources, seconds));
        }

        public ICompletable Combine(ICompletable second)
        {
            return new ConcatCompletable(Completable.Combine(sources, second));
        }

        protected override IDisposable SubscribeCore(ICompletableObserver observer, IDisposable cancel)
        {
            return new ConcatObserver(this, observer, cancel).Run();
        }

        private class ConcatObserver : OperatorCompletableObserverBase
        {
            private readonly ConcatCompletable parent;
            private readonly object gate = new object();

            private bool isDisposed;
            private IEnumerator<ICompletable> enumerator;
            private SerialDisposable subscription;
            private Action nextSelf;

            public ConcatObserver(ConcatCompletable parent, ICompletableObserver observer, IDisposable cancel)
                : base(observer, cancel)
            {
                this.parent = parent;
            }

            public IDisposable Run()
            {
                isDisposed = false;
                enumerator = parent.sources.GetEnumerator();
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
                                enumerator.Dispose();
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
                        hasNext = enumerator.MoveNext();
                        if (hasNext)
                        {
                            current = enumerator.Current;
                            if (current == null)
                                throw new InvalidOperationException("sequence is null.");
                        }
                        else
                        {
                            enumerator.Dispose();
                        }
                    }
                    catch (Exception exception)
                    {
                        ex = exception;
                        enumerator.Dispose();
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
                        try
                        {
                            observer.OnCompleted();
                        }
                        finally
                        {
                            Dispose();
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
                nextSelf();
            }
        }
    }
}