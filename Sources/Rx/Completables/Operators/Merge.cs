using System;
using System.Collections.Generic;

namespace UniRx.Completables.Operators
{
    internal class MergeCompletable : OperatorCompletableBase
    {
        private readonly IObservable<ICompletable> sources;
        private readonly int maxConcurrent;

        public MergeCompletable(IObservable<ICompletable> sources, bool isRequiredSubscribeOnCurrentThread)
            : base(isRequiredSubscribeOnCurrentThread)
        {
            this.sources = sources;
        }

        public MergeCompletable(IObservable<ICompletable> sources,
                                int maxConcurrent,
                                bool isRequiredSubscribeOnCurrentThread)
            : base(isRequiredSubscribeOnCurrentThread)
        {
            this.sources = sources;
            this.maxConcurrent = maxConcurrent;
        }

        protected override IDisposable SubscribeCore(ICompletableObserver observer, IDisposable cancel)
        {
            return maxConcurrent > 0
                       ? new LimitedConcurrentMergeObserver(this, observer, cancel).Run()
                       : new UnlimitedConcurrentMergeObserver(this, observer, cancel).Run();
        }

        #region UnlimitedConcurrentMergeObserver

        private class UnlimitedConcurrentMergeObserver : OperatorObservableToCompletableObserverBase<ICompletable>
        {
            private readonly object gate = new object();
            private readonly MergeCompletable parent;
            private SingleAssignmentDisposable sourceDisposable;
            private CompositeDisposable collectionDisposable;
            private bool isStopped;

            public UnlimitedConcurrentMergeObserver(MergeCompletable parent,
                                                    ICompletableObserver observer,
                                                    IDisposable cancel)
                : base(observer, cancel)
            {
                this.parent = parent;
            }

            public IDisposable Run()
            {
                collectionDisposable = new CompositeDisposable();
                sourceDisposable = new SingleAssignmentDisposable();
                collectionDisposable.Add(sourceDisposable);

                sourceDisposable.Disposable = parent.sources.Subscribe(this);
                return collectionDisposable;
            }

            public override void OnNext(ICompletable value)
            {
                var disposable = new SingleAssignmentDisposable();
                collectionDisposable.Add(disposable);
                var innerObserver = new InnerObserver(this, disposable);
                disposable.Disposable = value.Subscribe(innerObserver);
            }

            public override void OnError(Exception error)
            {
                lock (gate)
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
                isStopped = true;
                if (collectionDisposable.Count == 1)
                {
                    lock (gate)
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
                else
                {
                    sourceDisposable.Dispose();
                }
            }

            #region InnerObserver

            private class InnerObserver : OperatorCompletableObserverBase
            {
                private readonly UnlimitedConcurrentMergeObserver parent;
                private readonly IDisposable cancel;

                public InnerObserver(UnlimitedConcurrentMergeObserver parent, IDisposable cancel)
                    : base(parent.observer, cancel)
                {
                    this.parent = parent;
                    this.cancel = cancel;
                }

                public override void OnError(Exception error)
                {
                    lock (parent.gate)
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
                    parent.collectionDisposable.Remove(cancel);
                    if (parent.isStopped && parent.collectionDisposable.Count == 1)
                    {
                        lock (parent.gate)
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

            #endregion
        }

        #endregion

        #region LimitedConcurrentMergeObserver

        private class LimitedConcurrentMergeObserver : OperatorObservableToCompletableObserverBase<ICompletable>
        {
            private readonly object gate = new object();
            private readonly MergeCompletable parent;
            private Queue<ICompletable> queue;
            private CompositeDisposable collectionDisposable;
            private SingleAssignmentDisposable sourceDisposable;
            private bool isStopped;
            private int activeCount;

            public LimitedConcurrentMergeObserver(MergeCompletable parent,
                                                  ICompletableObserver observer,
                                                  IDisposable cancel)
                : base(observer, cancel)
            {
                this.parent = parent;
            }

            public IDisposable Run()
            {
                queue = new Queue<ICompletable>();
                activeCount = 0;

                collectionDisposable = new CompositeDisposable();
                sourceDisposable = new SingleAssignmentDisposable();
                collectionDisposable.Add(sourceDisposable);

                sourceDisposable.Disposable = parent.sources.Subscribe(this);
                return collectionDisposable;
            }

            public override void OnNext(ICompletable value)
            {
                lock (gate)
                {
                    if (activeCount < parent.maxConcurrent)
                    {
                        activeCount++;
                        Subscribe(value);
                    }
                    else
                    {
                        queue.Enqueue(value);
                    }
                }
            }

            public override void OnError(Exception error)
            {
                lock (gate)
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
                lock (gate)
                {
                    isStopped = true;
                    if (activeCount == 0)
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
                    else
                    {
                        sourceDisposable.Dispose();
                    }
                }
            }

            private void Subscribe(ICompletable innerSource)
            {
                var disposable = new SingleAssignmentDisposable();
                collectionDisposable.Add(disposable);
                var innerObserver = new InnerObserver(this, disposable);
                disposable.Disposable = innerSource.Subscribe(innerObserver);
            }

            #region InnerObserver

            private class InnerObserver : OperatorCompletableObserverBase
            {
                private readonly LimitedConcurrentMergeObserver parent;
                private readonly IDisposable cancel;

                public InnerObserver(LimitedConcurrentMergeObserver parent, IDisposable cancel)
                    : base(parent.observer, cancel)
                {
                    this.parent = parent;
                    this.cancel = cancel;
                }

                public override void OnError(Exception error)
                {
                    lock (parent.gate)
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
                    parent.collectionDisposable.Remove(cancel);
                    lock (parent.gate)
                    {
                        if (parent.queue.Count > 0)
                        {
                            var source = parent.queue.Dequeue();
                            parent.Subscribe(source);
                        }
                        else
                        {
                            parent.activeCount--;
                            if (parent.isStopped && parent.activeCount == 0)
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
            }

            #endregion
        }

        #endregion
    }
}