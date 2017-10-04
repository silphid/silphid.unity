using UniRx;

namespace Silphid.Showzup.Navigation
{
    public interface IFocusable
    {
        ReactiveProperty<bool> IsFocused { get; }
    }
}