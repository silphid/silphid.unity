using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UniRx;

namespace Silphid.Showzup.Virtual
{
    public interface IListPresenter
    {
        [Pure]
        ICompletable Present(IReadOnlyList<object> models, IOptions options = null);

        IObservable<IView> LoadedViews { get; }
        IObservable<IView> UnloadedViews { get; }
    }
}