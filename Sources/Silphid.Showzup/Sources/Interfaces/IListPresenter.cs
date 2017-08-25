using System;
using JetBrains.Annotations;
using UniRx;

namespace Silphid.Showzup
{
    public interface IListPresenter : IPresenter
    {
        ReadOnlyReactiveProperty<IView[]> Views { get; }
        [Pure] IObservable<IView> Add(object input, Options options = null);
        void Remove(object input);
    }
}