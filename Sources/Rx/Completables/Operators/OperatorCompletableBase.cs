using System;

namespace UniRx.Completables.Operators
{
    public abstract class OperatorCompletableBase : ICompletable
    {
        private readonly bool isRequiredSubscribeOnCurrentThread;

        protected OperatorCompletableBase(bool isRequiredSubscribeOnCurrentThread)
        {
            this.isRequiredSubscribeOnCurrentThread = isRequiredSubscribeOnCurrentThread;
        }

        public bool IsRequiredSubscribeOnCurrentThread()
        {
            return isRequiredSubscribeOnCurrentThread;
        }

        public IDisposable Subscribe(ICompletableObserver observer)
        {
            var subscription = new SingleAssignmentDisposable();

            // note:
            // does not make the safe observer, it breaks exception durability.
            // var safeObserver = Observer.CreateAutoDetachObserver<T>(observer, subscription);

            if (isRequiredSubscribeOnCurrentThread && Scheduler.IsCurrentThreadSchedulerScheduleRequired)
            {
                Scheduler.CurrentThread.Schedule(() => subscription.Disposable = SubscribeCore(observer, subscription));
            }
            else
            {
                subscription.Disposable = SubscribeCore(observer, subscription);
            }

            return subscription;
        }

        protected abstract IDisposable SubscribeCore(ICompletableObserver observer, IDisposable cancel);
    }
}