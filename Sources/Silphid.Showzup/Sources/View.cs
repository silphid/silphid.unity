using System;
using System.Collections;
using Silphid.Extensions;
using Silphid.Loadzup;
using Silphid.Injexit;
using Silphid.Loadzup.Http.Caching;
using Silphid.Requests;
using Silphid.Tweenzup;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

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

        #region Binding helpers

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

        protected ICompletable BindAsync(ListControl listControl, IEnumerable items) =>
            listControl
                ?.Present(items)
                .AsCompletable()
                .AutoDetach()
            ?? Completable.Empty();

        protected void Bind(Image image, Uri uri, bool keepVisible = false, float? fadeDuration = null)
        {
            if (image != null)
                BindAsync(image, uri, false, null, keepVisible, fadeDuration)
                    .Subscribe()
                    .AddTo(this);
        }

        protected ICompletable BindAsync(Image image, Uri uri, bool isOptional = false,
            Loadzup.Options options = null,
            bool keepVisible = false, float? fadeDuration = null)
        {
            if (image == null)
                return Completable.Empty();

            if (uri == null)
            {
                if (isOptional)
                    return Completable.Empty();

                return Completable.Throw(
                    new BindException(
                        $"Cannot bind required image {image.gameObject.name} in view {gameObject.name} to null Uri."));
            }

            if (fadeDuration != null)
                image.color = Color.clear;
            else
                image.enabled = keepVisible;

            return Loader
                .With(DefaultImageHttpCachePolicy)
                .Load<DisposableSprite>(uri, options)
                .Catch<DisposableSprite, Exception>(ex =>
                    Observable.Throw<DisposableSprite>(
                        new BindException(
                            $"Failed to load image {image.gameObject.name} in view {GetType().Name} from {uri}", ex)))
                .Do(x =>
                {
                    if (image == null)
                    {
                        if (uri.Scheme == Scheme.Http || uri.Scheme == Scheme.Https ||
                            uri.Scheme == Scheme.StreamingAsset || uri.Scheme == Scheme.StreamingFile)
                            x.Dispose();
                        return;
                    }

                    image.sprite = x.Sprite;
                    image.enabled = true;

                    if (fadeDuration != null)
                        Observable.NextFrame()
                            .Then(_ => image.TweenColorTo(Color.white, fadeDuration.Value))
                            .SubscribeAndForget()
                            .AddTo(this);

                    if (uri.Scheme == Scheme.Http || uri.Scheme == Scheme.Https || uri.Scheme == Scheme.StreamingAsset
                        || uri.Scheme == Scheme.StreamingFile)
                        x.AddTo(this);
                })
                .AutoDetach()
                .AsCompletable();
        }

        protected ICompletable BindAsync(RawImage image, Uri uri, bool isOptional = false,
            Loadzup.Options options = null,
            bool keepVisible = false, float? fadeDuration = null)
        {
            if (image == null)
                return Completable.Empty();

            if (uri == null)
            {
                if (isOptional)
                    return Completable.Empty();

                return Completable.Throw(
                    new BindException(
                        $"Cannot bind required image {image.gameObject.name} in view {gameObject.name} to null Uri."));
            }

            if (fadeDuration != null)
                image.color = Color.clear;
            else
                image.enabled = keepVisible;

            return Loader
                .With(DefaultImageHttpCachePolicy)
                .Load<Texture2D>(uri, options)
                .Catch<Texture2D, Exception>(ex => Observable
                    .Throw<Texture2D>(new BindException(
                        $"Failed to load image {image.gameObject.name} in view {GetType().Name} from {uri}", ex)))
                .Do(x =>
                {
                    if (image == null)
                    {
                        if (uri.Scheme == Scheme.Http || uri.Scheme == Scheme.Https ||
                            uri.Scheme == Scheme.StreamingAsset || uri.Scheme == Scheme.StreamingFile)
                            Destroy(x);
                        return;
                    }

                    image.texture = x;
                    image.enabled = true;

                    if (fadeDuration != null)
                        Observable.NextFrame()
                            .Then(_ => image.TweenColorTo(Color.white, fadeDuration.Value))
                            .SubscribeAndForget()
                            .AddTo(this);

                    if (uri.Scheme == Scheme.Http || uri.Scheme == Scheme.Https ||
                        uri.Scheme == Scheme.StreamingAsset || uri.Scheme == Scheme.StreamingFile)
                    {
                        Disposable
                            .Create(() => Destroy(x))
                            .AddTo(this);
                    }
                })
                .AutoDetach()
                .AsCompletable();
        }

        #endregion

        #region Request helpers

        protected bool Send(IRequest request) =>
            gameObject.Send(request);

        protected bool Send(Exception exception) =>
            gameObject.Send(exception);

        protected bool Send<TRequest>() where TRequest : IRequest, new() =>
            gameObject.Send(new TRequest());

        #endregion
    }
}