using System;

namespace UniRx.Completables.Operators
{
    internal class TimeoutCompletable : OperatorCompletableBase
    {
        private readonly ICompletable source;
        private readonly TimeSpan? dueTimeSpan;
        private readonly DateTimeOffset? dueDateTimeOffset;
        private readonly IScheduler scheduler;

        public TimeoutCompletable(ICompletable source, TimeSpan dueTimeSpan, IScheduler scheduler)
            : base(scheduler == Scheduler.CurrentThread || source.IsRequiredSubscribeOnCurrentThread())
        {
            this.source = source;
            this.dueTimeSpan = dueTimeSpan;
            this.scheduler = scheduler;
        }

        public TimeoutCompletable(ICompletable source, DateTimeOffset dueDateTimeOffset, IScheduler scheduler)
            : base(scheduler == Scheduler.CurrentThread || source.IsRequiredSubscribeOnCurrentThread())
        {
            this.source = source;
            this.dueDateTimeOffset = dueDateTimeOffset;
            this.scheduler = scheduler;
        }

        protected override IDisposable SubscribeCore(ICompletableObserver observer, IDisposable cancel)
        {
            var dueTime = dueDateTimeOffset ?? scheduler.Now + dueTimeSpan.Value;
            return new TimeoutObserver(this, observer, dueTime, cancel).Run();
        }

        private class TimeoutObserver : OperatorCompletableObserverBase
        {
            private readonly TimeoutCompletable parent;
            private readonly DateTimeOffset dueTime;
            private readonly object gate = new object();
            private bool isFinished;
            private SingleAssignmentDisposable sourceSubscription;
            private IDisposable timerSubscription;

            public TimeoutObserver(TimeoutCompletable parent,
                                   ICompletableObserver observer,
                                   DateTimeOffset dueTime,
                                   IDisposable cancel)
                : base(observer, cancel)
            {
                this.parent = parent;
                this.dueTime = dueTime;
            }

            public IDisposable Run()
            {
                sourceSubscription = new SingleAssignmentDisposable();

                timerSubscription = parent.scheduler.Schedule(dueTime, OnTimedOut);
                sourceSubscription.Disposable = parent.source.Subscribe(this);

                return StableCompositeDisposable.Create(timerSubscription, sourceSubscription);
            }

            private void OnTimedOut()
            {
                lock (gate)
                {
                    if (isFinished)
                        return;
                    isFinished = true;
                }

                sourceSubscription.Dispose();
                try
                {
                    observer.OnError(new TimeoutException());
                }
                finally
                {
                    Dispose();
                }
            }

            public override void OnError(Exception error)
            {
                lock (gate)
                {
                    if (isFinished)
                        return;
                    isFinished = true;
                    timerSubscription.Dispose();
                }

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
                lock (gate)
                {
                    if (!isFinished)
                    {
                        isFinished = true;
                        timerSubscription.Dispose();
                    }

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