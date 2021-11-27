using System;

namespace UniRx.Completables.Operators
{
    internal class CreateCompletable : OperatorCompletableBase
    {
        private readonly Func<ICompletableObserver, IDisposable> subscribe;

        public CreateCompletable(Func<ICompletableObserver, IDisposable> subscribe)
            : base(true)
        {
            this.subscribe = subscribe;
        }

        public CreateCompletable(Func<ICompletableObserver, IDisposable> subscribe,
                                 bool isRequiredSubscribeOnCurrentThread)
            : base(isRequiredSubscribeOnCurrentThread)
        {
            this.subscribe = subscribe;
        }

        protected override IDisposable SubscribeCore(ICompletableObserver observer, IDisposable cancel)
        {
            observer = new Create(observer, cancel);
            return subscribe(observer) ?? Disposable.Empty;
        }

        private class Create : OperatorCompletableObserverBase
        {
            public Create(ICompletableObserver observer, IDisposable cancel)
                : base(observer, cancel) {}

            public override void OnError(Exception error)
            {
                try
                {
                    observer.OnError(error);
                }
                finally
                {
                    Dispose();
                }
            }

            public override void OnCompleted()
            {
                try
                {
                    observer.OnCompleted();
                }
                finally
                {
                    Dispose();
                }
            }
        }
    }

    internal class CreateCompletable<TState> : OperatorCompletableBase
    {
        private readonly TState state;
        private readonly Func<TState, ICompletableObserver, IDisposable> subscribe;

        public CreateCompletable(TState state, Func<TState, ICompletableObserver, IDisposable> subscribe)
            : base(true)
        {
            this.state = state;
            this.subscribe = subscribe;
        }

        public CreateCompletable(TState state,
                                 Func<TState, ICompletableObserver, IDisposable> subscribe,
                                 bool isRequiredSubscribeOnCurrentThread)
            : base(isRequiredSubscribeOnCurrentThread)
        {
            this.state = state;
            this.subscribe = subscribe;
        }

        protected override IDisposable SubscribeCore(ICompletableObserver observer, IDisposable cancel)
        {
            observer = new Create(observer, cancel);
            return subscribe(state, observer) ?? Disposable.Empty;
        }

        private class Create : OperatorCompletableObserverBase
        {
            public Create(ICompletableObserver observer, IDisposable cancel)
                : base(observer, cancel) {}

            public override void OnError(Exception error)
            {
                try
                {
                    observer.OnError(error);
                }
                finally
                {
                    Dispose();
                }
            }

            public override void OnCompleted()
            {
                try
                {
                    observer.OnCompleted();
                }
                finally
                {
                    Dispose();
                }
            }
        }
    }
}