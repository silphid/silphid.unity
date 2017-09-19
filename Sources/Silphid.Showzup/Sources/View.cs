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

        public override string ToString() => GetType().Name;

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
                BindAsync(image, uri, false, null, keepVisible)
                    .Subscribe()
                    .AddTo(this);
        }

        protected IObservable<Unit> BindAsync(Image image, Uri uri, bool isOptional = false, Loadzup.Options options = null,
            bool keepVisible = false)
        {
            if (image == null)
                return Observable.ReturnUnit();
            
            if (uri == null)
            {
                if (isOptional)
                    return Observable.ReturnUnit();
                    
                return Observable.Throw<Unit>(
                    new BindException($"Cannot bind required image {image.gameObject.name} in view {gameObject.name} to null Uri."));
            }

            image.enabled = keepVisible;
            return Loader
                .Load<Sprite>(uri, options)
                .Catch<Sprite, Exception>(ex =>
                    Observable.Throw<Sprite>(
                        new BindException($"Failed to load image {image.gameObject.name} in view {GetType().Name} from {uri}", ex)))
                .Do(x =>
                {
                    image.sprite = x;
                    image.enabled = true;

                    if (uri.Scheme == Scheme.Http || uri.Scheme == Scheme.Https || uri.Scheme == Scheme.StreamingAsset)
                        Disposable.Create(() => Destroy(image.sprite.texture))
                            .AddTo(this);
                })
                .AutoDetach()
                .AsSingleUnitObservable();
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