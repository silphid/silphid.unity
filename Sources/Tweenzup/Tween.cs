using System;
using Silphid.Extensions;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Silphid.Tweenzup
{
    public static class Tween
    {
        #region Range

        public static IObservable<float> Range(float duration, IEaser easer = null) =>
            new TweenObservable(duration, easer);

        public static IObservable<float>
            Range(Func<float> selector, float target, float duration, IEaser easer = null) =>
            Observable.Defer(
                () =>
                {
                    var source = selector();
                    return source.IsAlmostEqualTo(target)
                               ? Observable.Return(target)
                               : Range(duration, easer)
                                  .Lerp(source, target);
                });

        public static IObservable<Vector2> Range(Func<Vector2> selector,
                                                 Vector2 target,
                                                 float duration,
                                                 IEaser easer = null) =>
            Observable.Defer(
                () =>
                {
                    var source = selector();
                    return source.IsAlmostEqualTo(target)
                               ? Observable.Return(target)
                               : Range(duration, easer)
                                  .Lerp(source, target);
                });

        public static IObservable<Vector3> Range(Func<Vector3> selector,
                                                 Vector3 target,
                                                 float duration,
                                                 IEaser easer = null) =>
            Observable.Defer(
                () =>
                {
                    var source = selector();
                    return source.IsAlmostEqualTo(target)
                               ? Observable.Return(target)
                               : Range(duration, easer)
                                  .Lerp(source, target);
                });

        public static IObservable<Quaternion> Range(Func<Quaternion> selector,
                                                    Quaternion target,
                                                    float duration,
                                                    IEaser easer = null) =>
            Observable.Defer(
                () =>
                {
                    var source = selector();
                    return source.IsAlmostEqualTo(target)
                               ? Observable.Return(target)
                               : Range(duration, easer)
                                  .Lerp(source, target);
                });

        public static IObservable<Color>
            Range(Func<Color> selector, Color target, float duration, IEaser easer = null) =>
            Observable.Defer(
                () =>
                {
                    var source = selector();
                    return source.IsAlmostEqualTo(target)
                               ? Observable.Return(target)
                               : Range(duration, easer)
                                  .Lerp(source, target);
                });

        public static IObservable<DateTime> Range(Func<DateTime> selector,
                                                  DateTime target,
                                                  float duration,
                                                  IEaser easer = null) =>
            Observable.Defer(
                () =>
                {
                    var source = selector();
                    return source == target
                               ? Observable.Return(target)
                               : Range(duration, easer)
                                  .Lerp(source, target);
                });

        public static IObservable<TimeSpan> Range(Func<TimeSpan> selector,
                                                  TimeSpan target,
                                                  float duration,
                                                  IEaser easer = null) =>
            Observable.Defer(
                () =>
                {
                    var source = selector();
                    return source == target
                               ? Observable.Return(target)
                               : Range(duration, easer)
                                  .Lerp(source, target);
                });

        #endregion

        #region ReactiveProperty TweenTo

        public static ICompletable TweenTo(this IReactiveProperty<float> This,
                                           float target,
                                           float duration,
                                           IEaser easer = null) =>
            Range(() => This.Value, target, duration, easer)
               .Do(x => This.Value = x)
               .AsCompletable();

        public static ICompletable TweenTo(this IReactiveProperty<float?> This,
                                           float target,
                                           float duration,
                                           IEaser easer = null) =>
            Range(() => This.Value.Value, target, duration, easer)
               .Do(x => This.Value = x)
               .AsCompletable();

        public static ICompletable TweenTo(this IReactiveProperty<float> This,
                                           float target,
                                           float duration,
                                           Func<float> velocitySelector,
                                           IEaser easer = null) =>
            new TweenFloatWithInitialVelocityObservable(() => This.Value, velocitySelector, target, duration, easer)
               .Do(x => This.Value = x)
               .AsCompletable();

        public static ICompletable TweenTo(this IReactiveProperty<float?> This,
                                           float target,
                                           float duration,
                                           Func<float> velocitySelector,
                                           IEaser easer = null) =>
            new TweenFloatWithInitialVelocityObservable(
                    () => This.Value.Value,
                    velocitySelector,
                    target,
                    duration,
                    easer).Do(x => This.Value = x)
                          .AsCompletable();

        public static ICompletable TweenToShiftingTarget(this IReactiveProperty<float> This,
                                                         IObservable<float> target,
                                                         float duration,
                                                         IEaser easer = null) =>
            new TweenFloatToShiftingTargetCompletable(This, target, duration, easer);

        public static ICompletable TweenTo(this IReactiveProperty<Vector2> This,
                                           Vector2 target,
                                           float duration,
                                           IEaser easer = null) =>
            Range(() => This.Value, target, duration, easer)
               .Do(x => This.Value = x)
               .AsCompletable();

        public static ICompletable TweenTo(this IReactiveProperty<Vector2?> This,
                                           Vector2 target,
                                           float duration,
                                           IEaser easer = null) =>
            Range(() => This.Value.Value, target, duration, easer)
               .Do(x => This.Value = x)
               .AsCompletable();

        public static ICompletable TweenTo(this IReactiveProperty<Vector2> This,
                                           Vector2 target,
                                           float duration,
                                           Func<Vector2> velocitySelector,
                                           IEaser easer = null) =>
            new TweenVector2WithInitialVelocityObservable(() => This.Value, velocitySelector, target, duration, easer)
               .Do(x => This.Value = x)
               .AsCompletable();

        public static ICompletable TweenTo(this IReactiveProperty<Vector2?> This,
                                           Vector2 target,
                                           float duration,
                                           Func<Vector2> velocitySelector,
                                           IEaser easer = null) =>
            new TweenVector2WithInitialVelocityObservable(
                    () => This.Value.Value,
                    velocitySelector,
                    target,
                    duration,
                    easer).Do(x => This.Value = x)
                          .AsCompletable();

        public static ICompletable TweenToShiftingTarget(this IReactiveProperty<Vector2> This,
                                                         IObservable<Vector2> target,
                                                         float duration,
                                                         IEaser easer = null) =>
            new TweenVector2ToShiftingTargetCompletable(This, target, duration, easer);

        public static ICompletable TweenTo(this IReactiveProperty<Vector3> This,
                                           Vector3 target,
                                           float duration,
                                           IEaser easer = null) =>
            Range(() => This.Value, target, duration, easer)
               .Do(x => This.Value = x)
               .AsCompletable();

        public static ICompletable TweenTo(this IReactiveProperty<Vector3?> This,
                                           Vector3 target,
                                           float duration,
                                           IEaser easer = null) =>
            Range(() => This.Value.Value, target, duration, easer)
               .Do(x => This.Value = x)
               .AsCompletable();

        public static ICompletable TweenTo(this IReactiveProperty<Vector3> This,
                                           Vector3 target,
                                           float duration,
                                           Func<Vector3> velocitySelector,
                                           IEaser easer = null) =>
            new TweenVector3WithInitialVelocityObservable(() => This.Value, velocitySelector, target, duration, easer)
               .Do(x => This.Value = x)
               .AsCompletable();

        public static ICompletable TweenTo(this IReactiveProperty<Vector3?> This,
                                           Vector3 target,
                                           float duration,
                                           Func<Vector3> velocitySelector,
                                           IEaser easer = null) =>
            new TweenVector3WithInitialVelocityObservable(
                    () => This.Value.Value,
                    velocitySelector,
                    target,
                    duration,
                    easer).Do(x => This.Value = x)
                          .AsCompletable();

        public static ICompletable TweenToShiftingTarget(this IReactiveProperty<Vector3> This,
                                                         IObservable<Vector3> target,
                                                         float duration,
                                                         IEaser easer = null) =>
            new TweenVector3ToShiftingTargetCompletable(This, target, duration, easer);

        public static ICompletable TweenTo(this IReactiveProperty<Quaternion> This,
                                           Quaternion target,
                                           float duration,
                                           IEaser easer = null) =>
            Range(() => This.Value, target, duration, easer)
               .Do(x => This.Value = x)
               .AsCompletable();

        public static ICompletable TweenTo(this IReactiveProperty<Quaternion?> This,
                                           Quaternion target,
                                           float duration,
                                           IEaser easer = null) =>
            Range(() => This.Value.Value, target, duration, easer)
               .Do(x => This.Value = x)
               .AsCompletable();

        public static ICompletable TweenTo(this IReactiveProperty<Color> This,
                                           Color target,
                                           float duration,
                                           IEaser easer = null) =>
            Range(() => This.Value, target, duration, easer)
               .Do(x => This.Value = x)
               .AsCompletable();

        public static ICompletable TweenTo(this IReactiveProperty<Color?> This,
                                           Color target,
                                           float duration,
                                           IEaser easer = null) =>
            Range(() => This.Value.Value, target, duration, easer)
               .Do(x => This.Value = x)
               .AsCompletable();

        public static ICompletable TweenTo(this IReactiveProperty<DateTime> This,
                                           DateTime target,
                                           float duration,
                                           IEaser easer = null) =>
            Range(() => This.Value, target, duration, easer)
               .Do(x => This.Value = x)
               .AsCompletable();

        public static ICompletable TweenTo(this IReactiveProperty<DateTime?> This,
                                           DateTime target,
                                           float duration,
                                           IEaser easer = null) =>
            Range(() => This.Value.Value, target, duration, easer)
               .Do(x => This.Value = x)
               .AsCompletable();

        public static ICompletable TweenTo(this IReactiveProperty<TimeSpan> This,
                                           TimeSpan target,
                                           float duration,
                                           IEaser easer = null) =>
            Range(() => This.Value, target, duration, easer)
               .Do(x => This.Value = x)
               .AsCompletable();

        public static ICompletable TweenTo(this IReactiveProperty<TimeSpan?> This,
                                           TimeSpan target,
                                           float duration,
                                           IEaser easer = null) =>
            Range(() => This.Value.Value, target, duration, easer)
               .Do(x => This.Value = x)
               .AsCompletable();

        #endregion

        #region Unity

        public static ICompletable FadeTo(this CanvasGroup This, float target, float duration, IEaser easer = null) =>
            Range(() => This.alpha, target, duration, easer)
               .Do(x => This.alpha = x)
               .AsCompletable();

        public static ICompletable FadeIn(this CanvasGroup This, float duration, IEaser easer = null) =>
            Range(() => This.alpha, 1f, duration, easer)
               .Do(x => This.alpha = x)
               .AsCompletable();

        public static ICompletable FadeOut(this CanvasGroup This, float duration, IEaser easer = null) =>
            Range(() => This.alpha, 0f, duration, easer)
               .Do(x => This.alpha = x)
               .AsCompletable();

        public static ICompletable TweenColorTo(this Graphic This, Color target, float duration, IEaser easer = null) =>
            Range(() => This.color, target, duration, easer)
               .Do(x => This.color = x)
               .AsCompletable();

        public static ICompletable MoveTo(this Transform This, Vector3 target, float duration, IEaser easer = null) =>
            Range(() => This.localPosition, target, duration, easer)
               .Do(x => This.localPosition = x)
               .AsCompletable();

        public static ICompletable MoveXBy(this Transform This, float delta, float duration, IEaser easer = null) =>
            This.MoveBy(delta * Vector3.right, duration, easer);

        public static ICompletable MoveYBy(this Transform This, float delta, float duration, IEaser easer = null) =>
            This.MoveBy(delta * Vector3.up, duration, easer);

        public static ICompletable MoveZBy(this Transform This, float delta, float duration, IEaser easer = null) =>
            This.MoveBy(delta * Vector3.forward, duration, easer);

        public static ICompletable MoveBy(this Transform This, Vector3 delta, float duration, IEaser easer = null) =>
            Completable.Defer(
                () =>
                {
                    var source = This.localPosition;
                    var target = source + delta;

                    return Range(duration, easer)
                          .Lerp(source, target)
                          .Do(x => This.localPosition = x)
                          .AsCompletable();
                });

        public static ICompletable WorldMoveTo(this Transform This,
                                               Vector3 target,
                                               float duration,
                                               IEaser easer = null) =>
            Range(() => This.position, target, duration, easer)
               .Do(x => This.position = x)
               .AsCompletable();

        public static ICompletable LocalMoveToWorld(this Transform This,
                                               Vector3 target,
                                               float duration,
                                               IEaser easer = null)
        {
            var localTarget = This.parent.InverseTransformPoint(target);
            return Range(() => This.localPosition, localTarget, duration, easer)
                .Do(x => This.localPosition = x)
                .AsCompletable();
        }

        public static ICompletable RotateTo(this Transform This,
                                            Quaternion target,
                                            float duration,
                                            IEaser easer = null) =>
            Range(() => This.localRotation, target, duration, easer)
               .Do(x => This.localRotation = x)
               .AsCompletable();

        public static ICompletable RotateTo(this Transform This, Vector3 target, float duration, IEaser easer = null) =>
            Range(() => This.localRotation, Quaternion.Euler(target), duration, easer)
               .Do(x => This.localRotation = x)
               .AsCompletable();

        public static ICompletable WorldRotateTo(this Transform This,
                                                 Quaternion target,
                                                 float duration,
                                                 IEaser easer = null) =>
            Range(() => This.rotation, target, duration, easer)
               .Do(x => This.rotation = x)
               .AsCompletable();

        public static ICompletable WorldRotateTo(this Transform This,
                                                 Vector3 target,
                                                 float duration,
                                                 IEaser easer = null) =>
            Range(() => This.rotation, Quaternion.Euler(target), duration, easer)
               .Do(x => This.rotation = x)
               .AsCompletable();

        public static ICompletable ScaleTo(this Transform This, Vector3 target, float duration, IEaser easer = null) =>
            Range(() => This.localScale, target, duration, easer)
               .Do(x => This.localScale = x)
               .AsCompletable();

        public static ICompletable ScaleTo(this Transform This, float target, float duration, IEaser easer = null) =>
            Range(() => This.localScale, new Vector3(target, target, target), duration, easer)
               .Do(x => This.localScale = x)
               .AsCompletable();

        public static ICompletable MoveAnchorTo(this RectTransform This,
                                                Vector2 target,
                                                float duration,
                                                IEaser easer = null) =>
            Range(() => This.anchoredPosition, target, duration, easer)
               .Do(x => This.anchoredPosition = x)
               .AsCompletable();

        public static ICompletable MoveAnchorXTo(this RectTransform This,
                                                 float target,
                                                 float duration,
                                                 IEaser easer = null) =>
            Range(() => This.anchoredPosition.x, target, duration, easer)
               .Do(x => This.anchoredPosition = This.anchoredPosition.WithX(x))
               .AsCompletable();

        public static ICompletable MoveAnchorYTo(this RectTransform This,
                                                 float target,
                                                 float duration,
                                                 IEaser easer = null) =>
            Range(() => This.anchoredPosition.y, target, duration, easer)
               .Do(y => This.anchoredPosition = This.anchoredPosition.WithY(y))
               .AsCompletable();

        public static ICompletable ChangeTopOffsetBy(this RectTransform This,
                                                     float target,
                                                     float duration,
                                                     IEaser easer = null) =>
            Range(() => This.offsetMax.y, This.offsetMax.y - target, duration, easer)
               .Do(y => This.offsetMax = new Vector2(This.offsetMax.x, y))
               .AsCompletable();

        #endregion
    }
}