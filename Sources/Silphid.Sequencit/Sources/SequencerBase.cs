using System;
using UniRx;

namespace Silphid.Sequencit
{
    public abstract class SequencerBase
    {
        protected IObservable<Unit> GetObservableFromItem(object item) =>
            item as IObservable<Unit> ??
            Observable.Defer((Func<IObservable<Unit>>) item);
    }
}