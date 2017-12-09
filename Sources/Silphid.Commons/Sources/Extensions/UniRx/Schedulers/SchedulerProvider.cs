using UniRx;

namespace Silphid.Extensions.UniRx.Schedulers
{
    public class SchedulerProvider : ISchedulerProvider
    {
        private static ISchedulerProvider _instance;
        public static ISchedulerProvider Instance => _instance ?? (_instance = new SchedulerProvider());

        public IScheduler CurrentThread => Scheduler.CurrentThread;
        public IScheduler Immediate => Scheduler.Immediate;
        public IScheduler MainThread => Scheduler.MainThread;
        public IScheduler MainThreadEndOfFrame => Scheduler.MainThreadEndOfFrame;
        public IScheduler MainThreadFixedUpdate => Scheduler.MainThreadFixedUpdate;
        public IScheduler MainThreadIgnoreTimeScale => Scheduler.MainThreadIgnoreTimeScale;
        public IScheduler ThreadPool => Scheduler.ThreadPool;
    }
}