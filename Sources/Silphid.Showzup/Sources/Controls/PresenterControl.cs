using System;
using Silphid.Showzup.Navigation;
using UniRx;
using UnityEngine;

namespace Silphid.Showzup
{
    public abstract class PresenterControl : Control, IPresenter, IForwardSelectable
    {
        public abstract IObservable<IView> Present(object input, Options options = null);
        public abstract ReadOnlyReactiveProperty<bool> IsLoading { get; }
        public abstract ReadOnlyReactiveProperty<bool> IsPresenting { get; }
        
        protected ReactiveProperty<IView> MutableFirstView = new ReactiveProperty<IView>((IView) null);
        public ReadOnlyReactiveProperty<IView> FirstView => MutableFirstView.ToReadOnlyReactiveProperty();
        
        public virtual GameObject ForwardSelection() => FirstView.Value?.GameObject;
    }
}