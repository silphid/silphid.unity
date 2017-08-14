using System;
using UniRx;

namespace Silphid.Showzup
{
    public abstract class PresenterControl: Control, IPresenter
    {
        public abstract bool CanPresent(object input, Options options = null);
        public abstract IObservable<IView> Present(object input, Options options = null);

        protected ReactiveProperty<IView> MutableFirstView = new ReactiveProperty<IView>((IView) null);
        public ReadOnlyReactiveProperty<IView> FirstView => MutableFirstView.ToReadOnlyReactiveProperty();

    }
}