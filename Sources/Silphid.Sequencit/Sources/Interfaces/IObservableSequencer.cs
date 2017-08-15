using System;
using UniRx;

namespace Silphid.Sequencit
{
    public interface IObservableSequencer : ISequencer, IObservable<Unit>
    {
        
    }
}