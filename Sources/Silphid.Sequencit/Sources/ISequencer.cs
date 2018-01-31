using System;
using UniRx;

namespace Silphid.Sequencit
{
    public interface ISequencer : ICompletable
    {
        object Add(ICompletable completable);
        object Add(Func<ICompletable> selector);
    }
}