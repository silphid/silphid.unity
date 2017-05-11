using JetBrains.Annotations;
using UniRx;

namespace Silphid.Showzup
{
    public interface IViewLoader
    {
        [Pure] IObservable<IView> Load(ViewInfo viewInfo, CancellationToken cancellationToken);
    }
}