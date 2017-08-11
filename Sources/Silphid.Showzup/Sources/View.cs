using System;
using System.Collections;
using Silphid.Extensions;
using Silphid.Loadzup;
using Silphid.Injexit;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Silphid.Showzup
{
    public abstract class View<TViewModel> :
        MonoBehaviour, IView<TViewModel>, ILoadable where TViewModel : IViewModel
    {
        #region IView members

        private IViewModel _viewModel;

        public bool IsActive
        {
            get { return enabled && gameObject.activeSelf; }
            set
            {
                gameObject.SetActive(value);
                enabled = value;
            }
        }

        IViewModel IView.ViewModel
        {
            get { return _viewModel; }
            set { _viewModel = value; }
        }

        public GameObject GameObject => gameObject;
        public TViewModel ViewModel => (TViewModel) _viewModel;

        [Inject] protected ILoader Loader;

        protected void Bind(Text text, string value)
        {
            if (text != null)
                text.text = value;
        }

        protected void Bind(IPresenter presenter, object content)
        {
            presenter?
                .Present(content)
                .AutoDetach()
                .Subscribe()
                .AddTo(this);
        }

        protected void Bind(ListControl listControl, IEnumerable items)
        {
            if (listControl != null)
                BindAsync(listControl, items)
                    .Subscribe()
                    .AddTo(this);
        }

        protected IObservable<Unit> BindAsync(ListControl listControl, IEnumerable items) =>
            listControl
                ?.Present(items)
                .AsSingleUnitObservable()
                .AutoDetach()
            ?? Observable.ReturnUnit();

        protected void Bind(Image image, Uri uri, bool keepVisible = false)
        {
            if (image != null)
                BindAsync(image, uri, null, keepVisible)
                    .Subscribe()
                    .AddTo(this);
        }

        protected IObservable<Unit> BindAsync(Image image, Uri uri, Loadzup.Options options = null,
            bool keepVisible = false)
        {
            if (image == null)
                return Observable.ReturnUnit();

            image.enabled = keepVisible;
            return Loader
                .Load<Sprite>(uri, options)
                .Catch<Sprite, Exception>(x => Observable.Throw<Sprite>(new BindException($"Unable to resolve image {uri} in view {GetType().Name}", x)))
                .Do(x =>
                {
                    image.sprite = x;
                    image.enabled = true;
                })
                .AutoDetach()
                .AsSingleUnitObservable();
        }

        protected IObservable<Unit> BindAsyncOrDefault(Image image, Uri uri, Loadzup.Options options = null,
            bool keepVisible = false)
        {
            if (uri == null)
            {
                Debug.LogError($"Uri of image is null on {gameObject.name}");
                return Observable.ReturnUnit();
            }

            return BindAsync(image, uri, options, keepVisible);
        }

        #endregion

        #region ILoadable members

        public virtual IObservable<Unit> Load()
        {
            return null;
        }

        #endregion
    }
}