using UniRx;

namespace Silphid.Showzup.Navigation
{
    public interface ISelectable
    {
        IReactiveProperty<bool> IsSelected { get; }
        IReactiveProperty<bool> IsSelfOrDescendantSelected { get; }
    }
}