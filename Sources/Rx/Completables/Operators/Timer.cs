using System;

namespace UniRx.Completables.Operators
{
    internal class TimerCompletable : OperatorCompletableBase
    {
        private readonly DateTimeOffset? dueTimeOffset;
        private readonly TimeSpan? dueTimeSpan;
        private readonly IScheduler scheduler;

        public TimerCompletable(DateTimeOffset dueTimeOffset, IScheduler scheduler)
            : base(scheduler == Scheduler.CurrentThread)
        {
            this.dueTimeOffset = dueTimeOffset;
            this.scheduler = scheduler;
        }

        public TimerCompletable(TimeSpan dueTimeSpan, IScheduler scheduler)
            : base(scheduler == Scheduler.CurrentThread)
        {
            this.dueTimeSpan = dueTimeSpan;
            this.scheduler = scheduler;
        }

        protected override IDisposable SubscribeCore(ICompletableObserver observer, IDisposable cancel)
        {
            var dueTime = dueTimeOffset != null
                              ? dueTimeOffset.Value - scheduler.Now
                              : dueTimeSpan.Value;

            var timerObserver = new PassthroughCompletableObserver(observer, cancel);
            return scheduler.Schedule(Scheduler.Normalize(dueTime), timerObserver.OnCompleted);
        }
    }
}