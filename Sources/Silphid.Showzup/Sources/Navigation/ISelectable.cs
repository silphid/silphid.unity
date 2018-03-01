using UniRx;

namespace Silphid.Showzup.Navigation
{
    public interface ISelectable
    {
        ReactiveProperty<bool> IsSelected { get; }
        ReactiveProperty<bool> IsSelfOrDescendantSelected { get; }
    }
}