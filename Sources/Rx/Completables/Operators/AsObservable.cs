using System;
using UniRx.Operators;

namespace UniRx.Completables.Operators
{
    internal class AsObservableObservable<T> : OperatorObservableBase<T>
    {
        private readonly ICompletable source;

        public AsObservableObservable(ICompletable source)
            : base(source.IsRequiredSubscribeOnCurrentThread())
        {
            this.source = source;
        }

        protected override IDisposable SubscribeCore(IObserver<T> observer, IDisposable cancel)
        {
            return source.Subscribe(new PassthroughCompletableToObservableObserver<T>(observer, cancel));
        }
    }
}