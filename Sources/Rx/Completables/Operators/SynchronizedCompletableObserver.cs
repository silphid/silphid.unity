using System;

namespace UniRx.Completables.Operators
{
    internal class SynchronizedCompletableObserver : ICompletableObserver
    {
        private readonly ICompletableObserver observer;
        private readonly object gate;

        public SynchronizedCompletableObserver(ICompletableObserver observer, object gate)
        {
            this.observer = observer;
            this.gate = gate;
        }

        public void OnError(Exception error)
        {
            lock (gate)
            {
                observer.OnError(error);
            }
        }

        public void OnCompleted()
        {
            lock (gate)
            {
                observer.OnCompleted();
            }
        }
    }
}