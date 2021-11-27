using System;

namespace UniRx.Completables.Operators
{
    internal class UntilCompletable : OperatorCompletableBase
    {
        private readonly ICompletable source;
        private readonly ICompletable other;

        public UntilCompletable(ICompletable source, ICompletable other)
            : base(source.IsRequiredSubscribeOnCurrentThread() || other.IsRequiredSubscribeOnCurrentThread())
        {
            this.source = source;
            this.other = other;
        }

        protected override IDisposable SubscribeCore(ICompletableObserver observer, IDisposable cancel)
        {
            return new UntilObserver(this, observer, cancel).Run();
        }

        private class UntilObserver : OperatorCompletableObserverBase
        {
            private readonly UntilCompletable parent;
            private readonly object gate = new object();
            private bool open;

            public UntilObserver(UntilCompletable parent, ICompletableObserver observer, IDisposable cancel)
                : base(observer, cancel)
            {
                this.parent = parent;
            }

            public IDisposable Run()
            {
                var otherSubscription = new SingleAssignmentDisposable();
                var otherObserver = new UntilOtherObserver(this, otherSubscription);
                otherSubscription.Disposable = parent.other.Subscribe(otherObserver);

                var sourceSubscription = parent.source.Subscribe(this);

                return StableCompositeDisposable.Create(otherSubscription, sourceSubscription);
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

            private class UntilOtherObserver : ICompletableObserver
            {
                private readonly UntilObserver sourceObserver;
                private readonly IDisposable subscription;

                public UntilOtherObserver(UntilObserver sourceObserver, IDisposable subscription)
                {
                    this.sourceObserver = sourceObserver;
                    this.subscription = subscription;
                }

                public void OnError(Exception error)
                {
                    lock (sourceObserver.gate)
                    {
                        try
                        {
                            sourceObserver.observer.OnError(error);
                        }
                        finally
                        {
                            sourceObserver.Dispose();
                            subscription.Dispose();
                        }
                    }
                }

                public void OnCompleted()
                {
                    lock (sourceObserver.gate)
                    {
                        try
                        {
                            sourceObserver.observer.OnCompleted();
                        }
                        finally
                        {
                            sourceObserver.Dispose();
                            subscription.Dispose();
                        }
                    }
                }
            }
        }
    }
}