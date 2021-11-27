using System;
using JetBrains.Annotations;
using log4net;
using Silphid.Extensions;
using Silphid.Injexit;
using Silphid.Requests;
using Silphid.Showzup.Navigation;
using Silphid.Showzup.Recipes;
using UniRx;
using UnityEngine;

namespace Silphid.Showzup
{
    public abstract class PresenterControl : Control, IPresenter, ISelectableContainer, IContainerProvider
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(PresenterControl));

        #region Private

        private Transform _instantiationContainer;
        private readonly Subject<Exception> _errorsSubject = new Subject<Exception>();

        public virtual GameObject SelectableContent => FirstView.Value?.GameObject;

        private void Awake()
        {
            if (Log.IsDebugEnabled)
                MutableState.Subscribe(x => Log.Debug($"State: {x}\r\nPresenter: {gameObject.ToHierarchyPath()}"))
                            .AddTo(this);
        }

        #endregion

        #region Protected

        protected IReactiveProperty<PresenterState> MutableState { get; } = new ReactiveProperty<PresenterState>();
        protected readonly IReactiveProperty<IView> MutableFirstView = new ReactiveProperty<IView>(null);
        [Inject, UsedImplicitly] protected IRecipeProvider RecipeProvider { get; set; }
        [Inject, UsedImplicitly] protected IVariantProvider VariantProvider { get; set; }

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

        public bool ForceUpdateLayoutRequest;

        #endregion

        #region IPresenter members

        public IObservable<IView> Present(object input, IOptions options = null) =>
            PresentView(input, options)
               .DoOnError(
                    ex =>
                    {
                        MutableState.Value = PresenterState.Ready;

                        try
                        {
                            _errorsSubject.OnNext(ex);
                            Log.Error(ex);
                        }
                        finally
                        {
                            if (SendExceptionRequest)
                                this.Send(ex);
                        }
                    })
               .DoOnCompleted(() =>
                {
                    if(ForceUpdateLayoutRequest)
                        this.Send<ForceUpdateLayoutRequest>();
                });

        protected abstract IObservable<IView> PresentView(object input, IOptions options = null);

        public virtual IReadOnlyReactiveProperty<PresenterState> State => MutableState;

        #endregion

        #region IContainerProvider members

        private IContainer _container;

        public IContainer Container =>
            _container ?? (_container = this.GetParentContainer());

        #endregion
    }
}