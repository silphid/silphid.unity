using System;

namespace UniRx.Completables.Operators
{
    internal class DeferCompletable : OperatorCompletableBase
    {
        private readonly Func<ICompletable> completableFactory;

        public DeferCompletable(Func<ICompletable> completableFactory)
            : base(false)
        {
            this.completableFactory = completableFactory;
        }

        protected override IDisposable SubscribeCore(ICompletableObserver observer, IDisposable cancel)
        {
            observer = new DeferObserver(observer, cancel);

            ICompletable source;
            try
            {
                source = completableFactory();
            }
            catch (Exception ex)
            {
                source = Completable.Throw(ex);
            }

            return source.Subscribe(observer);
        }

        private class DeferObserver : OperatorCompletableObserverBase
        {
            public DeferObserver(ICompletableObserver observer, IDisposable cancel)
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
    }
}