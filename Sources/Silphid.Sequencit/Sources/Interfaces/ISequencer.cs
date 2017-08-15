using System;
using UniRx;

namespace Silphid.Sequencit
{
    public interface ISequencer : IObservable<Unit>
    {
        void Add(IObservable<Unit> observable);
    }
}