using JetBrains.Annotations;
using UniRx;

namespace Silphid.Showzup
{
    public interface IPresenter
    {
        [Pure] IObservable<IView> Present(object input, Options options = null);
    }
}