using System;
using JetBrains.Annotations;
using UniRx;

namespace Silphid.Showzup
{
    public interface IPresenter
    {
        [Pure] bool CanPresent(object input, Options options = null);
        [Pure] IObservable<IView> Present(object input, Options options = null);
    }
}