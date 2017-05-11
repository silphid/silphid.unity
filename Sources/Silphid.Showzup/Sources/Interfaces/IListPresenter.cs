using System.Collections.ObjectModel;
using UniRx;

namespace Silphid.Showzup
{
    public interface IListPresenter : IPresenter
    {
        ReadOnlyReactiveProperty<ReadOnlyCollection<IView>> Views { get; }
    }
}