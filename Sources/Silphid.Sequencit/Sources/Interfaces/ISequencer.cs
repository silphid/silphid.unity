using System;
using UniRx;

namespace Silphid.Sequencit
{
    public interface ISequencer : IObservable<Unit>
    {
        object Add(IObservable<Unit> observable);
        object Add(Func<IObservable<Unit>> selector);
    }
}