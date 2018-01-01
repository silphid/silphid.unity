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

        public static IObservable<float> Range(float duration, Ease ease = Ease.Linear) =>
            new TweenObservable(duration, ease);

        public static IObservable<float> Range(Func<float> sourceSelector, float target, float duration, Ease ease = Ease.Linear) =>
            Observable.Defer(() =>
            {
                var source = sourceSelector();
                return source.IsAlmostEqualTo(target)
                    ? Observable.Return(target)
                    : Range(duration, ease).Lerp(source, target);
            });

        public static IObservable<Vector2> Range(Func<Vector2> sourceSelector, Vector2 target, float duration, Ease ease = Ease.Linear) =>
            Observable.Defer(() =>
            {
                var source = sourceSelector();
                return source.IsAlmostEqualTo(target)
                    ? Observable.Return(target)
                    : Range(duration, ease).Lerp(source, target);
            });

        public static IObservable<Vector3> Range(Func<Vector3> sourceSelector, Vector3 target, float duration, Ease ease = Ease.Linear) =>
            Observable.Defer(() =>
            {
                var source = sourceSelector();
                return source.IsAlmostEqualTo(target)
                    ? Observable.Return(target)
                    : Range(duration, ease).Lerp(source, target);
            });

        public static IObservable<Quaternion> Range(Func<Quaternion> sourceSelector, Quaternion target, float duration, Ease ease = Ease.Linear) =>
            Observable.Defer(() =>
            {
                var source = sourceSelector();
                return source.IsAlmostEqualTo(target)
                    ? Observable.Return(target)
                    : Range(duration, ease).Lerp(source, target);
            });

        public static IObservable<Color> Range(Func<Color> sourceSelector, Color target, float duration, Ease ease = Ease.Linear) =>
            Observable.Defer(() =>
            {
                var source = sourceSelector();
                return source.IsAlmostEqualTo(target)
                    ? Observable.Return(target)
                    : Range(duration, ease).Lerp(source, target);
            });
        
        #endregion

        #region ReactiveProperty TweenTo

        public static ICompletable TweenTo(this ReactiveProperty<float> This, float target, float duration, Ease ease = Ease.Linear) =>
            Range(() => This.Value, target, duration, ease)
                .Do(x => This.Value = x)
                .AsCompletable();

        public static ICompletable TweenTo(this ReactiveProperty<Vector2> This, Vector2 target, float duration, Ease ease = Ease.Linear) =>
            Range(() => This.Value, target, duration, ease)
                .Do(x => This.Value = x)
                .AsCompletable();

        public static ICompletable TweenTo(this ReactiveProperty<Vector3> This, Vector3 target, float duration, Ease ease = Ease.Linear) =>
            Range(() => This.Value, target, duration, ease)
                .Do(x => This.Value = x)
                .AsCompletable();

        public static ICompletable TweenTo(this ReactiveProperty<Quaternion> This, Quaternion target, float duration, Ease ease = Ease.Linear) =>
            Range(() => This.Value, target, duration, ease)
                .Do(x => This.Value = x)
                .AsCompletable();

        public static ICompletable TweenTo(this ReactiveProperty<Color> This, Color target, float duration, Ease ease = Ease.Linear) =>
            Range(() => This.Value, target, duration, ease)
                .Do(x => This.Value = x)
                .AsCompletable();

        #endregion

        #region Unity

        public static ICompletable FadeTo(this CanvasGroup This, float target, float duration, Ease ease = Ease.Linear) =>
            Range(() => This.alpha, target, duration, ease)
                .Do(x => This.alpha = x)
                .AsCompletable();

        public static ICompletable FadeIn(this CanvasGroup This, float duration, Ease ease = Ease.Linear) =>
            Range(() => This.alpha, 1f, duration, ease)
                .Do(x => This.alpha = x)
                .AsCompletable();

        public static ICompletable FadeOut(this CanvasGroup This, float duration, Ease ease = Ease.Linear) =>
            Range(() => This.alpha, 0f, duration, ease)
                .Do(x => This.alpha = x)
                .AsCompletable();

        public static ICompletable ColorTo(this Graphic This, Color target, float duration, Ease ease = Ease.Linear) =>
            Range(() => This.color, target, duration, ease)
                .Do(x => This.color = x)
                .AsCompletable();

        public static ICompletable MoveLocallyTo(this Transform This, Vector3 target, float duration, Ease ease = Ease.Linear) =>
            Range(() => This.localPosition, target, duration, ease)
                .Do(x => This.localPosition = x)
                .AsCompletable();

        public static ICompletable MoveTo(this Transform This, Vector3 target, float duration, Ease ease = Ease.Linear) =>
            Range(() => This.position, target, duration, ease)
                .Do(x => This.position = x)
                .AsCompletable();

        public static ICompletable RotateLocallyTo(this Transform This, Quaternion target, float duration, Ease ease = Ease.Linear) =>
            Range(() => This.localRotation, target, duration, ease)
                .Do(x => This.localRotation = x)
                .AsCompletable();

        public static ICompletable RotateLocallyTo(this Transform This, Vector3 target, float duration, Ease ease = Ease.Linear) =>
            Range(() => This.localRotation, Quaternion.Euler(target), duration, ease)
                .Do(x => This.localRotation = x)
                .AsCompletable();

        public static ICompletable RotateTo(this Transform This, Quaternion target, float duration, Ease ease = Ease.Linear) =>
            Range(() => This.rotation, target, duration, ease)
                .Do(x => This.rotation = x)
                .AsCompletable();

        public static ICompletable RotateTo(this Transform This, Vector3 target, float duration, Ease ease = Ease.Linear) =>
            Range(() => This.rotation, Quaternion.Euler(target), duration, ease)
                .Do(x => This.rotation = x)
                .AsCompletable();

        public static ICompletable ScaleLocallyTo(this Transform This, Vector3 target, float duration, Ease ease = Ease.Linear) =>
            Range(() => This.localScale, target, duration, ease)
                .Do(x => This.localScale = x)
                .AsCompletable();

        public static ICompletable ScaleLocallyTo(this Transform This, float target, float duration, Ease ease = Ease.Linear) =>
            Range(() => This.localScale, new Vector3(target, target, target), duration, ease)
                .Do(x => This.localScale = x)
                .AsCompletable();

        public static ICompletable AnchorPosTo(this RectTransform This, Vector2 target, float duration, Ease ease = Ease.Linear) =>
            Range(() => This.anchoredPosition, target, duration, ease)
                .Do(x => This.anchoredPosition = x)
                .AsCompletable();

        public static ICompletable AnchorPosXTo(this RectTransform This, float target, float duration, Ease ease = Ease.Linear) =>
            Range(() => This.anchoredPosition.x, target, duration, ease)
                .Do(x => This.anchoredPosition = This.anchoredPosition.WithX(x))
                .AsCompletable();

        public static ICompletable AnchorPosYTo(this RectTransform This, float target, float duration, Ease ease = Ease.Linear) =>
            Range(() => This.anchoredPosition.y, target, duration, ease)
                .Do(y => This.anchoredPosition = This.anchoredPosition.WithY(y))
                .AsCompletable();
        
        #endregion
    }
}