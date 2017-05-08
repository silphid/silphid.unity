using UniRx;

namespace Silphid.Sequencit
{
    public interface ISequenceable
    {
        void Add(IObservable<Unit> observable);
    }
}