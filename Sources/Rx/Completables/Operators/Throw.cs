using System;

namespace UniRx.Completables.Operators
{
    internal class ThrowCompletable : OperatorCompletableBase
    {
        private readonly Exception error;
        private readonly IScheduler scheduler;

        public ThrowCompletable(Exception error, IScheduler scheduler)
            : base(scheduler == Scheduler.CurrentThread)
        {
            this.error = error;
            this.scheduler = scheduler;
        }

        protected override IDisposable SubscribeCore(ICompletableObserver observer, IDisposable cancel)
        {
            observer = new ThrowObserver(observer, cancel);

            if (scheduler == Scheduler.Immediate)
            {
                observer.OnError(error);
                return Disposable.Empty;
            }

            return scheduler.Schedule(
                () =>
                {
                    observer.OnError(error);
                    observer.OnCompleted();
                });
        }

        private class ThrowObserver : OperatorCompletableObserverBase
        {
            public ThrowObserver(ICompletableObserver observer, IDisposable cancel)
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