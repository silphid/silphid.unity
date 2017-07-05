using UniRx;

namespace Silphid.Showzup
{
    public interface IListPresenter : IPresenter
    {
        ReadOnlyReactiveProperty<IView[]> Views { get; }
    }
}