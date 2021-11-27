using UniRx;

namespace Silphid.Extensions.UniRx.Schedulers
{
    public interface ISchedulerProvider
    {
        IScheduler CurrentThread { get; }
        IScheduler Immediate { get; }
        IScheduler MainThread { get; }
        IScheduler MainThreadEndOfFrame { get; }
        IScheduler MainThreadFixedUpdate { get; }
        IScheduler MainThreadIgnoreTimeScale { get; }
        IScheduler ThreadPool { get; }
    }
}