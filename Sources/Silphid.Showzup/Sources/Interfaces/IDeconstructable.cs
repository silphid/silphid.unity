using UniRx;

namespace Silphid.Showzup
{
    public interface IDeconstructable
    {
        IObservable<Unit> Deconstruct(Options options);
    }
}