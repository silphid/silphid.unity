using UniRx;

namespace Silphid.Showzup
{
    public interface IFocusable
    {
        BoolReactiveProperty IsFocused { get; }
    }
}