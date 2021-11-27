namespace UniRx
{
    public interface IOptimizedCompletable : ICompletable
    {
        bool IsRequiredSubscribeOnCurrentThread();
    }

    public static class OptimizedCompletableExtensions
    {
        public static bool IsRequiredSubscribeOnCurrentThread(this ICompletable source)
        {
            var obs = source as IOptimizedCompletable;
            if (obs == null)
                return true;

            return obs.IsRequiredSubscribeOnCurrentThread();
        }

        public static bool IsRequiredSubscribeOnCurrentThread(this ICompletable source, IScheduler scheduler)
        {
            if (scheduler == Scheduler.CurrentThread)
                return true;

            return IsRequiredSubscribeOnCurrentThread(source);
        }
    }
}