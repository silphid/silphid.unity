using System;

namespace UniRx.Completables.Operators
{
    internal class ImmutableNeverCompletable : IOptimizedCompletable
    {
        internal static ImmutableNeverCompletable Instance = new ImmutableNeverCompletable();

        public bool IsRequiredSubscribeOnCurrentThread()
        {
            return false;
        }

        public IDisposable Subscribe(ICompletableObserver observer)
        {
            return Disposable.Empty;
        }
    }
}