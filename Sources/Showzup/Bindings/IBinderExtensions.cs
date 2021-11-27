using System;
using System.Collections.Generic;
using Silphid.Extensions;
using Silphid.Loadzup;
using Silphid.Requests;
using Silphid.Tweenzup;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Silphid.Showzup
{
    public static class IBinderExtensions
    {
        public static IDisposable AddTo(this IDisposable This, IBinder binder) => binder.Add(This);

        public static IDisposable BindSetActive(this IBinder This, IObservable<bool> source, GameObject target)
        {
            if (target != null)
                return source.Subscribe(target.SetActive)
                             .AddTo(This);

            return Disposable.Empty;
        }

        public static IDisposable Bind<TSource, TTarget>(this IBinder This,
                                                         IObservable<TSource> source,
                                                         IReactiveProperty<TTarget> target) where TTarget : TSource =>
            source.BindTo(target)
                  .AddTo(This);

        public static IDisposable BindTwoWay<TSource, TTarget>(this IBinder This,
                                                               IReactiveProperty<TSource> source,
                                                               IReactiveProperty<TTarget> target) =>
            source.BindTwoWayTo(target)
                  .AddTo(This);

        public static IDisposable Bind<T>(this IBinder This, IObservable<T> source, IPresenter target)
        {
            if (target != null)
                return source.BindTo(target)
                             .AddTo(This);

            return Disposable.Empty;
        }

        public static IDisposable Bind(this IBinder This, Button source, IRequest target)
        {
            if (target == null)
                return Disposable.Empty;
            
            return source?.OnClickAsObservable()
                          .Subscribe(_ => source.Send(target))
                          .AddTo(This);
        }
        
        public static IDisposable Bind(this IBinder This, Button source, IEnumerable<IRequest> targets)
        {
            if (targets == null)
                return Disposable.Empty;
            
            return source?.OnClickAsObservable()
                          .Subscribe(_ =>
                           {
                               targets.ForEach(source.Send);
                           })
                          .AddTo(This);
        }

        public static IDisposable Bind<TRequest>(this IBinder This, Button source) where TRequest : IRequest, new() =>
            source?.OnClickAsObservable()
                   .Subscribe(_ => source.Send<TRequest>())
                   .AddTo(This);

        public static IDisposable Bind<TRequest>(this IBinder This, IObservable<Unit> source) where TRequest : IRequest, new() =>
            source?.Subscribe(_ => This.View.Send<TRequest>())
                   .AddTo(This);

        public static IDisposable BindAction(this IBinder This, Button source, Action action) =>
            source?.OnClickAsObservable()
                   .Subscribe(_ => action())
                   .AddTo(This);
        
        public static IDisposable BindAction(this IBinder This, Button source, Func<bool> condition, Action action) =>
            source?.OnClickAsObservable()
                   .Where(_ => condition())
                   .Subscribe(_ => action())
                   .AddTo(This);

        public static IDisposable Bind<T>(this IBinder This, IReadOnlyReactiveCollection<T> source, ListControl target)
        {
            if (target != null)
                return This.Bind(source.ObserveCurrentAddRemove(), target);

            return Disposable.Empty;
        }

        public static IDisposable Bind<T>(this IBinder This,
                                          IObservable<CollectionAddRemoveEvent<T>> source,
                                          ListControl target)
        {
            if (target != null)
                return source.Subscribe(
                                  x =>
                                  {
                                      if (x.IsAdded)
                                          target.Add(x.Value)
                                                .SubscribeAndForget();
                                      else
                                          target.Remove(x.Value);
                                  })
                             .AddTo(This);

            return Disposable.Empty;
        }

        // ReSharper disable once UnusedParameter.Global
        public static void Bind(this IBinder This, string source, Text target)
        {
            if (target != null)
                target.text = source;
        }

        // ReSharper disable once UnusedParameter.Global
        public static ICompletable BindAsCompletable(this IBinder This, object source, IPresenter target)
        {
            if (source == null || target == null)
                return Completable.Empty();

            return target.Present(source)
                         .AutoDetach()
                         .AsCompletable();
        }

        public static IDisposable Bind(this IBinder This, object source, IPresenter target)
        {
            if (source != null)
                return target?.Present(source)
                              .AutoDetach()
                              .Subscribe()
                              .AddTo(This);

            return Disposable.Empty;
        }

        public static IDisposable Bind(this IBinder This,
                                       string source,
                                       Image target,
                                       bool keepVisible = false,
                                       float? fadeDuration = null,
                                       QueuePriority queuePriority = QueuePriority.Normal) =>
            This.Bind(new Uri(source), target, keepVisible, fadeDuration, queuePriority);

        public static IDisposable Bind(this IBinder This,
                                       Uri source,
                                       Image target,
                                       bool keepVisible = false,
                                       float? fadeDuration = null,
                                       QueuePriority queuePriority = QueuePriority.Normal)
        {
            if (source != null && target != null)
                return This.BindAsCompletable(source, target, false, null, keepVisible, fadeDuration, queuePriority)
                           .Subscribe()
                           .AddTo(This);

            return Disposable.Empty;
        }

        public static ICompletable BindAsCompletable(this IBinder This,
                                                     string source,
                                                     Image target,
                                                     bool isOptional = false,
                                                     Loadzup.Options options = null,
                                                     bool keepVisible = false,
                                                     float? fadeDuration = null,
                                                     QueuePriority queuePriority = QueuePriority.Normal)
        {
            if (source == null || target == null)
                return Completable.Empty();

            return This.BindAsCompletable(
                new Uri(source),
                target,
                isOptional,
                options,
                keepVisible,
                fadeDuration,
                queuePriority);
        }

        public static ICompletable BindAsCompletable(this IBinder This,
                                                     Uri source,
                                                     Image target,
                                                     bool isOptional = false,
                                                     Loadzup.Options options = null,
                                                     bool keepVisible = false,
                                                     float? fadeDuration = null,
                                                     QueuePriority queuePriority = QueuePriority.Normal)
        {
            if (target == null)
                return Completable.Empty();

            if (source == null)
            {
                if (isOptional)
                    return Completable.Empty();

                return Completable.Throw(
                    new BindException(
                        $"Cannot bind required image {target.gameObject.name} in view {This.View.GameObject.name} to null Uri."));
            }

            if (fadeDuration != null)
                target.color = Color.clear;
            else
                target.enabled = keepVisible;

            return This.Loader.With(CacheGroup.Image)
                       .WithCancellationOnDestroy(This.View)
                       .With(queuePriority)
                       .Load<DisposableSprite>(source, options)
                       .Catch<DisposableSprite, Exception>(
                            ex => Observable.Throw<DisposableSprite>(
                                new BindException(
                                    $"Failed to load image {target.gameObject.name} in view {This.View.GetType().Name} from {source}",
                                    ex)))
                       .Do(
                            x =>
                            {
                                if (target.IsDestroyed())
                                {
                                    if (source.Scheme == Scheme.Http || source.Scheme == Scheme.Https ||
                                        source.Scheme == Scheme.StreamingAsset || source.Scheme == Scheme.StreamingFile)
                                        x.Dispose();
                                    return;
                                }

                                target.sprite = x.Sprite;
                                target.enabled = true;

                                if (fadeDuration != null)
                                    Observable.NextFrame()
                                              .Then(_ => target.TweenColorTo(Color.white, fadeDuration.Value))
                                              .SubscribeAndForget()
                                              .AddTo(This);

                                if (source.Scheme == Scheme.Http || source.Scheme == Scheme.Https ||
                                    source.Scheme == Scheme.StreamingAsset || source.Scheme == Scheme.StreamingFile)
                                    x.AddTo(This);
                            })
                       .AutoDetach()
                       .AsCompletable();
        }

        public static ICompletable BindAsCompletable(this IBinder This,
                                                     RawImage image,
                                                     Uri uri,
                                                     bool isOptional = false,
                                                     Loadzup.Options options = null,
                                                     bool keepVisible = false,
                                                     float? fadeDuration = null,
                                                     QueuePriority queuePriority = QueuePriority.Normal)
        {
            if (image == null)
                return Completable.Empty();

            if (uri == null)
            {
                if (isOptional)
                    return Completable.Empty();

                return Completable.Throw(
                    new BindException(
                        $"Cannot bind required image {image.gameObject.name} in view {This.View.GameObject.name} to null Uri."));
            }

            if (fadeDuration != null)
                image.color = Color.clear;
            else
                image.enabled = keepVisible;

            return This.Loader.With(CacheGroup.Image)
                       .WithCancellationOnDestroy(This.View)
                       .With(queuePriority)
                       .Load<Texture2D>(uri, options)
                       .Catch<Texture2D, Exception>(
                            ex => Observable.Throw<Texture2D>(
                                new BindException(
                                    $"Failed to load image {image.gameObject.name} in view {This.View.GetType().Name} from {uri}",
                                    ex)))
                       .Do(
                            x =>
                            {
                                if (image.IsDestroyed())
                                {
                                    if (uri.Scheme == Scheme.Http || uri.Scheme == Scheme.Https ||
                                        uri.Scheme == Scheme.StreamingAsset || uri.Scheme == Scheme.StreamingFile)
                                        Object.Destroy(x);
                                    return;
                                }

                                image.texture = x;
                                image.enabled = true;

                                if (fadeDuration != null)
                                    Observable.NextFrame()
                                              .Then(_ => image.TweenColorTo(Color.white, fadeDuration.Value))
                                              .SubscribeAndForget()
                                              .AddTo(This);

                                if (uri.Scheme == Scheme.Http || uri.Scheme == Scheme.Https ||
                                    uri.Scheme == Scheme.StreamingAsset || uri.Scheme == Scheme.StreamingFile)
                                {
                                    Disposable.Create(() => Object.Destroy(x))
                                              .AddTo(This);
                                }
                            })
                       .AutoDetach()
                       .AsCompletable();
        }
    }
}