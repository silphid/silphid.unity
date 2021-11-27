using System;

namespace UniRx.Completables.Operators
{
    internal class AsCompletableCompletable<T> : OperatorCompletableBase
    {
        private readonly IObservable<T> source;

        public AsCompletableCompletable(IObservable<T> source)
            : base(source.IsRequiredSubscribeOnCurrentThread())
        {
            this.source = source;
        }

        protected override IDisposable SubscribeCore(ICompletableObserver observer, IDisposable cancel)
        {
            return source.Subscribe(new PassthroughObservableToCompletableObserver<T>(observer, cancel));
        }
    }
}