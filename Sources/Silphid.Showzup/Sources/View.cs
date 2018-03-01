using System;
using Silphid.Loadzup;
using Silphid.Injexit;
using Silphid.Loadzup.Http.Caching;
using Silphid.Requests;
using UniRx;
using UnityEngine;
// ReSharper disable ConditionIsAlwaysTrueOrFalse

namespace Silphid.Showzup
{
    public abstract class View : MonoBehaviour
    {
        protected virtual HttpCachePolicy? DefaultImageHttpCachePolicy => null;
    }

    public abstract class View<TViewModel> :
        View, IView<TViewModel>, IDisposable,
        ILoadable where TViewModel : IViewModel
    {
        protected bool IsDisposed { get; private set; }
        public bool DisposeViewModelOnDestroy = true;

        [Inject] protected ILoader Loader;

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
            get { return _viewModel; }
            set { _viewModel = value; }
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

        public override string ToString() => GetType().Name;

        #endregion

        #region Request helpers

        protected bool Send(IRequest request) =>
            gameObject.Send(request);

        protected bool Send(Exception exception) =>
            gameObject.Send(exception);

        protected bool Send<TRequest>() where TRequest : IRequest, new() =>
            gameObject.Send(new TRequest());

        #endregion

        #region Binding

        private IBinder _binder;

        protected IBinder Binder => _binder ?? (_binder = new Binder(this, Loader, DefaultImageHttpCachePolicy));

        #endregion
    }
}