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

        public static IObservable<float> Range(float from, float to, float duration, Ease ease = Ease.Linear) =>
            from.IsAlmostEqualTo(to)
                ? Observable.Return(to)
                : Range(duration, ease).Lerp(from, to);

        public static IObservable<Vector2> Range(Vector2 from, Vector2 to, float duration, Ease ease = Ease.Linear) =>
            from.IsAlmostEqualTo(to)
                ? Observable.Return(to)
                : Range(duration, ease).Lerp(from, to);

        public static IObservable<Vector3> Range(Vector3 from, Vector3 to, float duration, Ease ease = Ease.Linear) =>
            from.IsAlmostEqualTo(to)
                ? Observable.Return(to)
                : Range(duration, ease).Lerp(from, to);

        public static IObservable<Quaternion> Range(Quaternion from, Quaternion to, float duration, Ease ease = Ease.Linear) =>
            from.IsAlmostEqualTo(to)
                ? Observable.Return(to)
                : Range(duration, ease).Lerp(from, to);

        public static IObservable<Color> Range(Color from, Color to, float duration, Ease ease = Ease.Linear) =>
            from.IsAlmostEqualTo(to)
                ? Observable.Return(to)
                : Range(duration, ease).Lerp(from, to);
        
        #endregion

        #region ReactiveProperty TweenTo

        public static ICompletable TweenTo(this ReactiveProperty<float> This, float to, float duration, Ease ease = Ease.Linear) =>
            Range(This.Value, to, duration, ease)
                .Do(x => This.Value = x)
                .AsCompletable();

        public static ICompletable TweenTo(this ReactiveProperty<Vector2> This, Vector2 to, float duration, Ease ease = Ease.Linear) =>
            Range(This.Value, to, duration, ease)
                .Do(x => This.Value = x)
                .AsCompletable();

        public static ICompletable TweenTo(this ReactiveProperty<Vector3> This, Vector3 to, float duration, Ease ease = Ease.Linear) =>
            Range(This.Value, to, duration, ease)
                .Do(x => This.Value = x)
                .AsCompletable();

        public static ICompletable TweenTo(this ReactiveProperty<Quaternion> This, Quaternion to, float duration, Ease ease = Ease.Linear) =>
            Range(This.Value, to, duration, ease)
                .Do(x => This.Value = x)
                .AsCompletable();

        public static ICompletable TweenTo(this ReactiveProperty<Color> This, Color to, float duration, Ease ease = Ease.Linear) =>
            Range(This.Value, to, duration, ease)
                .Do(x => This.Value = x)
                .AsCompletable();

        #endregion

        #region Unity

        public static ICompletable FadeTo(this CanvasGroup This, float to, float duration, Ease ease = Ease.Linear) =>
            Range(This.alpha, to, duration, ease)
                .Do(x => This.alpha = x)
                .AsCompletable();

        public static ICompletable FadeIn(this CanvasGroup This, float duration, Ease ease = Ease.Linear) =>
            Range(This.alpha, 1f, duration, ease)
                .Do(x => This.alpha = x)
                .AsCompletable();

        public static ICompletable FadeOut(this CanvasGroup This, float duration, Ease ease = Ease.Linear) =>
            Range(This.alpha, 0f, duration, ease)
                .Do(x => This.alpha = x)
                .AsCompletable();

        public static ICompletable ColorTo(this Graphic This, Color to, float duration, Ease ease = Ease.Linear) =>
            Range(This.color, to, duration, ease)
                .Do(x => This.color = x)
                .AsCompletable();

        public static ICompletable MoveLocallyTo(this Transform This, Vector3 to, float duration, Ease ease = Ease.Linear) =>
            Range(This.localPosition, to, duration, ease)
                .Do(x => This.localPosition = x)
                .AsCompletable();

        public static ICompletable MoveTo(this Transform This, Vector3 to, float duration, Ease ease = Ease.Linear) =>
            Range(This.position, to, duration, ease)
                .Do(x => This.position = x)
                .AsCompletable();

        public static ICompletable RotateLocallyTo(this Transform This, Quaternion to, float duration, Ease ease = Ease.Linear) =>
            Range(This.localRotation, to, duration, ease)
                .Do(x => This.localRotation = x)
                .AsCompletable();

        public static ICompletable RotateLocallyTo(this Transform This, Vector3 to, float duration, Ease ease = Ease.Linear) =>
            Range(This.localRotation, Quaternion.Euler(to), duration, ease)
                .Do(x => This.localRotation = x)
                .AsCompletable();

        public static ICompletable RotateTo(this Transform This, Quaternion to, float duration, Ease ease = Ease.Linear) =>
            Range(This.rotation, to, duration, ease)
                .Do(x => This.rotation = x)
                .AsCompletable();

        public static ICompletable RotateTo(this Transform This, Vector3 to, float duration, Ease ease = Ease.Linear) =>
            Range(This.rotation, Quaternion.Euler(to), duration, ease)
                .Do(x => This.rotation = x)
                .AsCompletable();

        public static ICompletable AnchorPosTo(this RectTransform This, Vector2 to, float duration, Ease ease = Ease.Linear) =>
            Range(This.anchoredPosition, to, duration, ease)
                .Do(x => This.anchoredPosition = x)
                .AsCompletable();

        public static ICompletable AnchorPosXTo(this RectTransform This, float to, float duration, Ease ease = Ease.Linear) =>
            Range(This.anchoredPosition.x, to, duration, ease)
                .Do(x => This.anchoredPosition = This.anchoredPosition.WithX(x))
                .AsCompletable();

        public static ICompletable AnchorPosYTo(this RectTransform This, float to, float duration, Ease ease = Ease.Linear) =>
            Range(This.anchoredPosition.y, to, duration, ease)
                .Do(y => This.anchoredPosition = This.anchoredPosition.WithY(y))
                .AsCompletable();

        public static ICompletable ScaleLocallyTo(this RectTransform This, Vector3 to, float duration, Ease ease = Ease.Linear) =>
            Range(This.localScale, to, duration, ease)
                .Do(x => This.localScale = x)
                .AsCompletable();

        public static ICompletable ScaleLocallyTo(this RectTransform This, float to, float duration, Ease ease = Ease.Linear) =>
            Range(This.localScale, new Vector3(to, to, to), duration, ease)
                .Do(x => This.localScale = x)
                .AsCompletable();
        
        #endregion
    }
}