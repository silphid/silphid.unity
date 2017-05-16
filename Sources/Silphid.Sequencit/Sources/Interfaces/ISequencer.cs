using UniRx;

namespace Silphid.Sequencit
{
    public interface ISequencer
    {
        void Add(IObservable<Unit> observable);
    }
}