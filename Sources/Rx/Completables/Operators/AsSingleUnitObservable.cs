using System;
using UniRx.Operators;

namespace UniRx.Completables.Operators
{
    internal class AsSingleUnitObservableCompletable : OperatorObservableBase<Unit>
    {
        private readonly ICompletable source;

        public AsSingleUnitObservableCompletable(ICompletable source)
            : base(source.IsRequiredSubscribeOnCurrentThread())
        {
            this.source = source;
        }

        protected override IDisposable SubscribeCore(IObserver<Unit> observer, IDisposable cancel)
        {
            return new AsSingleUnitObservableObserver(this, observer, cancel).Run();
        }

        private class AsSingleUnitObservableObserver : OperatorCompletableToObservableObserverBase<Unit>
        {
            private readonly AsSingleUnitObservableCompletable parent;

            public AsSingleUnitObservableObserver(AsSingleUnitObservableCompletable parent,
                                                  IObserver<Unit> observer,
                                                  IDisposable cancel)
                : base(observer, cancel)
            {
                this.parent = parent;
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
            }

            public override void OnCompleted()
            {
                observer.OnNext(Unit.Default);

                try
                {
                    observer.OnCompleted();
                }
                finally
                {
                    Dispose();
                }
            }

            public IDisposable Run()
            {
                return parent.source.Subscribe(this);
            }
        }
    }
}