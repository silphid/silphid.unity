using System;
using System.Threading;

namespace UniRx.Completables.Operators
{
    internal class WaitCompletableObserver : ICompletableObserver
    {
        private readonly ICompletable source;
        private readonly TimeSpan? timeoutDuration;
        private ManualResetEvent semaphore;
        private Exception exception;

        public WaitCompletableObserver(ICompletable source, TimeSpan? timeoutDuration)
        {
            this.source = source;
            this.timeoutDuration = timeoutDuration;
        }

        public void Run()
        {
            semaphore = new ManualResetEvent(false);
            using (source.Subscribe(this))
            {
                var isTimedOut = timeoutDuration.HasValue
                                     ? !semaphore.WaitOne(timeoutDuration.Value)
                                     : !semaphore.WaitOne();

                if (isTimedOut)
                    throw new TimeoutException("OnCompleted not fired.");
            }

            if (exception != null)
                throw exception;
        }

        public void OnError(Exception error)
        {
            exception = error;
            semaphore.Set();
        }

        public void OnCompleted()
        {
            semaphore.Set();
        }
    }
}