using System;
using UniRx;

namespace Silphid.Sequencit
{
    public abstract class SequencerBase
    {
        protected static ICompletable GetCompletableFromItem(object item) =>
            item as ICompletable ?? Completable.Defer((Func<ICompletable>) item);
    }
}