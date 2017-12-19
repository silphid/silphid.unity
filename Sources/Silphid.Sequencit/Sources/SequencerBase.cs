using System;
using UniRx;

namespace Silphid.Sequencit
{
    public abstract class SequencerBase
    {
        protected ICompletable GetObservableFromItem(object item) =>
            item as ICompletable ??
            ((Func<ICompletable>) item)();
    }
}