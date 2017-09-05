using System;
using JetBrains.Annotations;

namespace Silphid.Showzup
{
    public interface IPresenter
    {
        [Pure] IObservable<IView> Present(object input, Options options = null);
    }
}