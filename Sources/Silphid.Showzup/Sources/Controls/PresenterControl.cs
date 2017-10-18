using System;
using UniRx;
using UnityEngine;

namespace Silphid.Showzup
{
    public abstract class PresenterControl : Control, IPresenter
    {
        #region Private

        private Transform _instantiationContainer;

        #endregion
        
        #region Protected

        protected IReactiveProperty<PresenterState> MutableState { get; } = new ReactiveProperty<PresenterState>();
        protected IReactiveProperty<IView> MutableFirstView = new ReactiveProperty<IView>((IView) null);

        protected Transform GetInstantiationContainer()
        {
            if (_instantiationContainer == null)
            {
                var obj = new GameObject("InstantiationContainer");
                obj.SetActive(false);
                _instantiationContainer = obj.transform;
                _instantiationContainer.transform.parent = transform;
            }

            return _instantiationContainer;
        }

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