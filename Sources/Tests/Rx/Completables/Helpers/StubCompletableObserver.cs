using System;

namespace UniRx.Completables.Tests
{
    public class StubCompletableObserver : ICompletableObserver
    {
        public bool IsCompleted { get; private set; }
        public Exception Error { get; private set; }

        public void OnCompleted()
        {
            if (IsCompleted)
                throw new InvalidOperationException("CompletableObserver.OnCompleted() called more than once.");
                
            IsCompleted = true;
        }

        public void OnError(Exception error)
        {
            if (Error != null)
                throw new InvalidOperationException("CompletableObserver.OnError() called more than once.");
                
            Error = error;
        }
    }
}