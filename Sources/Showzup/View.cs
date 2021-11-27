using System;
using JetBrains.Annotations;
using Silphid.Extensions;
using Silphid.Loadzup;
using Silphid.Injexit;
using Silphid.Requests;
using Silphid.Showzup.Navigation;
using UniRx;
using UnityEngine;
using GameObjectExtensions = Silphid.Injexit.GameObjectExtensions;

// ReSharper disable ConditionIsAlwaysTrueOrFalse

namespace Silphid.Showzup
{
    public abstract class View : MonoBehaviour
    {
        [Inject, Optional, UsedImplicitly] public IViewOptions ViewOptions;
    }

    public abstract class View<TViewModel> : View, IView<TViewModel>, IDisposable, ILoadable, IContainerProvider
        where TViewModel : IViewModel
    {
        public IReactiveProperty<bool> IsSelected { get; } = new ReactiveProperty<bool>(false);
        public IReactiveProperty<bool> IsSelfOrDescendantSelected { get; } = new ReactiveProperty<bool>(false);
        protected bool IsDisposed { get; private set; }
        public bool DisposeViewModelOnDestroy = true;

        [Inject, UsedImplicitly] protected ILoader Loader;

        #region MonoBehaviour members

        protected virtual void OnDestroy()
        {
            Dispose();
        }

        #endregion

        #region IDisposable members

        public void Dispose()
        {
            if (IsDisposed)
                return;

            OnDispose();

            IsDisposed = true;
        }

        protected virtual void OnDispose()
        {
            if (DisposeViewModelOnDestroy)
            {
                var disposable = ViewModel as IDisposable;
                disposable?.Dispose();
            }
        }

        #endregion

        #region IView members

        private IViewModel _viewModel;

        IViewModel IView.ViewModel
        {
            get => _viewModel;
            set
            {
                if (_viewModel != null)
                    throw new InvalidOperationException("ViewModel can only be set once on a given View.");

                _viewModel = value;

                if (_viewModel is IChooseable chooseable)
                {
                    IsSelected.BindTo(chooseable.IsChosen)
                              .AddTo(this);
                    IsSelfOrDescendantSelected.BindTo(chooseable.IsChosen)
                                              .AddTo(this);
                }
            }
        }

        public GameObject GameObject => gameObject;
        public TViewModel ViewModel => (TViewModel) _viewModel;

        #endregion

        #region ILoadable members

        public virtual ICompletable Load()
        {
            return null;
        }

        #endregion

        #region Object members

        public override string ToString() => GetType()
           .Name;

        #endregion

        #region Request helpers

        protected void Send(IRequest request) =>
            gameObject.Send(request);

        protected void Send(Exception exception) =>
            gameObject.Send(exception);

        protected void Send<TRequest>() where TRequest : IRequest, new() =>
            gameObject.Send(new TRequest());

        #endregion

        #region Binding

        private IBinder _binder;

        protected IBinder Binder => _binder ?? (_binder = new Binder(this, Loader));

        #endregion

        #region IContainerProvider members

        private IContainer _container;

        IContainer IContainerProvider.Container =>
            _container ?? (_container = CreateOwnContainer());

        protected virtual IContainer CreateOwnContainer() =>
            this.GetParentContainer();

        protected void OverrideContainer(Action<Silphid.Injexit.IBinder> action)
        {
            _container = ((IContainerProvider) this).Container.Using(action);
        }

        #endregion
    }
}