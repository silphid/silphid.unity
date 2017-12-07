using System;
using log4net;
using Silphid.Extensions;
using Silphid.Requests;
using UniRx;
using UnityEngine;

namespace Silphid.Showzup
{
    public abstract class PresenterControl : Control, IPresenter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(PresenterControl));
        
        #region Private

        private Transform _instantiationContainer;
        private readonly Subject<Exception> _errorsSubject = new Subject<Exception>(); 

        private void Awake()
        {
            if (Log.IsDebugEnabled)
                MutableState.Subscribe(x => Log.Debug($"State: {x}\r\nPresenter: {gameObject.ToHierarchyPath()}")).AddTo(this);
        }

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

        public IObservable<IView> GetView() =>
            MutableFirstView.MergeErrors(_errorsSubject);

        [Tooltip("Whether control should send ExceptionRequests when errors occur.")]
        public bool SendExceptionRequest;

        #endregion

        #region IPresenter members

        public IObservable<IView> Present(object input, Options options = null) =>
            PresentView(input, options)
                .DoOnError(ex =>
                {
                    MutableState.Value = PresenterState.Ready;

                    try
                    {
                        _errorsSubject.OnNext(ex);
                    }
                    finally 
                    {
                        if (SendExceptionRequest)
                            this.Send(ex);
                    }
                });

        protected abstract IObservable<IView> PresentView(object input, Options options = null);
        
        public virtual IReadOnlyReactiveProperty<PresenterState> State => MutableState;
        
        #endregion
    }
}