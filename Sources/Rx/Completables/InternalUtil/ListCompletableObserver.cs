using System;
using UniRx.InternalUtil;

namespace UniRx.Completables.InternalUtil
{
    public class ListCompletableObserver : ICompletableObserver
    {
        private readonly ImmutableList<ICompletableObserver> _observers;

        public ListCompletableObserver(ImmutableList<ICompletableObserver> observers)
        {
            _observers = observers;
        }

        public void OnCompleted()
        {
            var targetObservers = _observers.Data;
            foreach (var t in targetObservers)
                t.OnCompleted();
        }

        public void OnError(Exception error)
        {
            var targetObservers = _observers.Data;
            foreach (var t in targetObservers)
                t.OnError(error);
        }

        internal ICompletableObserver Add(ICompletableObserver observer)
        {
            return new ListCompletableObserver(_observers.Add(observer));
        }

        internal ICompletableObserver Remove(ICompletableObserver observer)
        {
            var i = Array.IndexOf(_observers.Data, observer);
            if (i < 0)
                return this;

            if (_observers.Data.Length == 2)
                return _observers.Data[1 - i];

            return new ListCompletableObserver(_observers.Remove(observer));
        }
    }

    public class EmptyCompletableObserver : ICompletableObserver
    {
        public static readonly EmptyCompletableObserver Instance = new EmptyCompletableObserver();

        private EmptyCompletableObserver() {}

        public void OnCompleted() {}

        public void OnError(Exception error) {}
    }

    public class ThrowCompletableObserver : ICompletableObserver
    {
        public static readonly ThrowCompletableObserver Instance = new ThrowCompletableObserver();

        private ThrowCompletableObserver() {}

        public void OnCompleted() {}

        public void OnError(Exception error)
        {
            throw error;
        }
    }

    public class DisposedCompletableObserver : ICompletableObserver
    {
        public static readonly DisposedCompletableObserver Instance = new DisposedCompletableObserver();

        private DisposedCompletableObserver() {}

        public void OnCompleted()
        {
            throw new ObjectDisposedException("");
        }

        public void OnError(Exception error)
        {
            throw new ObjectDisposedException("");
        }
    }
}