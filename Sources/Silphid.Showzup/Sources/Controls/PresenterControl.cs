using System;
using UniRx;

namespace Silphid.Showzup
{
    public abstract class PresenterControl : Control, IPresenter
    {
        #region Protected

        protected IReactiveProperty<PresenterState> MutableState { get; } = new ReactiveProperty<PresenterState>();
        protected IReactiveProperty<IView> MutableFirstView = new ReactiveProperty<IView>((IView) null);

        #endregion
        
        #region Public

        public IReadOnlyReactiveProperty<IView> FirstView => MutableFirstView;

        #endregion

        #region IPresenter members

        public abstract IObservable<IView> Present(object input, Options options = null);
        public virtual IReadOnlyReactiveProperty<PresenterState> State => MutableState;
        
        #endregion
    }
}