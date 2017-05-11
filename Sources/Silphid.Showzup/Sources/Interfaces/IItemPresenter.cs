using UniRx;

namespace Silphid.Showzup
{
    public interface IItemPresenter : IPresenter
    {
        ReadOnlyReactiveProperty<IView> View { get; }
    }
}