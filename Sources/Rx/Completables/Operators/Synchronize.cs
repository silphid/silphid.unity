using System;

namespace UniRx.Completables.Operators
{
    internal class SynchronizeCompletable : OperatorCompletableBase
    {
        private readonly ICompletable source;
        private readonly object gate;

        public SynchronizeCompletable(ICompletable source, object gate)
            : base(source.IsRequiredSubscribeOnCurrentThread())
        {
            this.source = source;
            this.gate = gate;
        }

        protected override IDisposable SubscribeCore(ICompletableObserver observer, IDisposable cancel)
        {
            return source.Subscribe(new SynchronizeObserver(this, observer, cancel));
        }

        private class SynchronizeObserver : OperatorCompletableObserverBase
        {
            private readonly SynchronizeCompletable parent;

            public SynchronizeObserver(SynchronizeCompletable parent, ICompletableObserver observer, IDisposable cancel)
                : base(observer, cancel)
            {
                this.parent = parent;
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
}