using System;

namespace UniRx.Completables.Operators
{
    internal class EmptyCompletable : OperatorCompletableBase
    {
        private readonly IScheduler scheduler;

        public EmptyCompletable(IScheduler scheduler)
            : base(false)
        {
            this.scheduler = scheduler;
        }

        protected override IDisposable SubscribeCore(ICompletableObserver observer, IDisposable cancel)
        {
            observer = new PassthroughCompletableObserver(observer, cancel);
            return scheduler.Schedule(observer.OnCompleted);
        }
    }

    internal class ImmutableEmptyCompletable : IOptimizedCompletable
    {
        internal static ImmutableEmptyCompletable Instance = new ImmutableEmptyCompletable();

        public bool IsRequiredSubscribeOnCurrentThread()
        {
            return false;
        }

        public IDisposable Subscribe(ICompletableObserver observer)
        {
            observer.OnCompleted();
            return Disposable.Empty;
        }
    }
}