using System;

namespace Silphid.Showzup
{
    public abstract class PresenterControl: Control, IPresenter
    {
        public abstract bool CanPresent(object input, Options options = null);
        public abstract IObservable<IView> Present(object input, Options options = null);
    }
}