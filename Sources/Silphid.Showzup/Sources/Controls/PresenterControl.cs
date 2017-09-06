using System;
using UniRx;

namespace Silphid.Showzup
{
    public abstract class PresenterControl: Control, IPresenter
    {
        public abstract IObservable<IView> Present(object input, Options options = null);
        
        public ReadOnlyReactiveProperty<bool> IsLoading { get; protected set; }

        protected ReactiveProperty<IView> MutableFirstView = new ReactiveProperty<IView>((IView) null);
        public ReadOnlyReactiveProperty<IView> FirstView => MutableFirstView.ToReadOnlyReactiveProperty();

    }
}