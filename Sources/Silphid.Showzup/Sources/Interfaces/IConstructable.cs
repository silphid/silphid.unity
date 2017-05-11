using UniRx;
using Rx = UniRx;

namespace Silphid.Showzup
{
    public interface IConstructable
    {
        Rx.IObservable<Unit> Construct(Options options);
    }
}