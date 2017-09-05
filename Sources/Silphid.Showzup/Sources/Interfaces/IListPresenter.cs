using System;
using System.Collections.ObjectModel;
using JetBrains.Annotations;
using UniRx;

namespace Silphid.Showzup
{
    public interface IListPresenter : IPresenter
    {
        ReadOnlyReactiveProperty<ReadOnlyCollection<IView>> Views { get; }
        [Pure] IObservable<IView> Add(object input, Options options = null);
        void Remove(object input);
    }
}