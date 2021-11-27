using UniRx;

namespace Silphid.Showzup.Navigation
{
    public interface IChooseable
    {
        IReactiveProperty<bool> IsChosen { get; }
    }
}