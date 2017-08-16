using System;
using UniRx;

namespace Silphid.Sequencit
{
    public interface ISequencer : IObservable<Unit>
    {
        IObservable<Unit> Add(IObservable<Unit> observable);
    }
}