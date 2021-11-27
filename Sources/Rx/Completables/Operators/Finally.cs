using System;

namespace UniRx.Completables.Operators
{
    internal class FinallyCompletable : OperatorCompletableBase
    {
        private readonly ICompletable source;
        private readonly Action finallyAction;

        public FinallyCompletable(ICompletable source, Action finallyAction)
            : base(source.IsRequiredSubscribeOnCurrentThread())
        {
            this.source = source;
            this.finallyAction = finallyAction;
        }

        protected override IDisposable SubscribeCore(ICompletableObserver observer, IDisposable cancel)
        {
            return new FinallyObserver(this, observer, cancel).Run();
        }

        private class FinallyObserver : OperatorCompletableObserverBase
        {
            private readonly FinallyCompletable parent;

            public FinallyObserver(FinallyCompletable parent, ICompletableObserver observer, IDisposable cancel)
                : base(observer, cancel)
            {
                this.parent = parent;
            }

            public IDisposable Run()
            {
                IDisposable subscription;
                try
                {
                    subscription = parent.source.Subscribe(this);
                }
                catch
                {
                    // This behaviour is not same as .NET Official Rx
                    parent.finallyAction();
                    throw;
                }

                return StableCompositeDisposable.Create(
                    subscription,
                    Disposable.Create(() => { parent.finallyAction(); }));
            }

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

                ;
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

                ;
            }
        }
    }
}