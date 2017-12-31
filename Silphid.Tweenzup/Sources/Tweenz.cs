using System;
using Silphid.Extensions;
using UniRx;
using UnityEngine;

namespace Silphid.Tweenzup
{
    public static class Tweenz
    {
        #region Range

        public static IObservable<float> Range(float duration, Func<float, float> ease = null) =>
            new TweenObservable(duration, ease);

        public static IObservable<float> Range(float from, float to, float duration, Func<float, float> ease = null) =>
            from.IsAlmostEqualTo(to)
                ? Observable.Return(to)
                : Range(duration, ease).Lerp(from, to);

        public static IObservable<Vector2> Range(Vector2 from, Vector2 to, float duration, Func<float, float> ease = null) =>
            from.IsAlmostEqualTo(to)
                ? Observable.Return(to)
                : Range(duration, ease).Lerp(from, to);

        public static IObservable<Vector3> Range(Vector3 from, Vector3 to, float duration, Func<float, float> ease = null) =>
            from.IsAlmostEqualTo(to)
                ? Observable.Return(to)
                : Range(duration, ease).Lerp(from, to);

        public static IObservable<Quaternion> Range(Quaternion from, Quaternion to, float duration, Func<float, float> ease = null) =>
            from.IsAlmostEqualTo(to)
                ? Observable.Return(to)
                : Range(duration, ease).Lerp(from, to);

        public static IObservable<Color> Range(Color from, Color to, float duration, Func<float, float> ease = null) =>
            from.IsAlmostEqualTo(to)
                ? Observable.Return(to)
                : Range(duration, ease).Lerp(from, to);
        
        #endregion

        #region ReactiveProperty TweenTo

        public static ICompletable TweenTo(this ReactiveProperty<float> This, float to, float duration, Func<float, float> ease = null) =>
            Range(This.Value, to, duration, ease)
                .Do(x => This.Value = x)
                .AsCompletable();

        public static ICompletable TweenTo(this ReactiveProperty<Vector2> This, Vector2 to, float duration, Func<float, float> ease = null) =>
            Range(This.Value, to, duration, ease)
                .Do(x => This.Value = x)
                .AsCompletable();

        public static ICompletable TweenTo(this ReactiveProperty<Vector3> This, Vector3 to, float duration, Func<float, float> ease = null) =>
            Range(This.Value, to, duration, ease)
                .Do(x => This.Value = x)
                .AsCompletable();

        public static ICompletable TweenTo(this ReactiveProperty<Quaternion> This, Quaternion to, float duration, Func<float, float> ease = null) =>
            Range(This.Value, to, duration, ease)
                .Do(x => This.Value = x)
                .AsCompletable();

        public static ICompletable TweenTo(this ReactiveProperty<Color> This, Color to, float duration, Func<float, float> ease = null) =>
            Range(This.Value, to, duration, ease)
                .Do(x => This.Value = x)
                .AsCompletable();

        #endregion

        #region Unity

        public static ICompletable FadeTo(this CanvasGroup This, float to, float duration, Func<float, float> ease = null) =>
            Range(This.alpha, to, duration, ease)
                .Do(x => This.alpha = x)
                .AsCompletable();

        public static ICompletable FadeIn(this CanvasGroup This, float duration, Func<float, float> ease = null) =>
            Range(This.alpha, 1f, duration, ease)
                .Do(x => This.alpha = x)
                .AsCompletable();

        public static ICompletable FadeOut(this CanvasGroup This, float duration, Func<float, float> ease = null) =>
            Range(This.alpha, 0f, duration, ease)
                .Do(x => This.alpha = x)
                .AsCompletable();

        public static ICompletable TweenAnchorPosTo(this RectTransform This, Vector2 to, float duration, Func<float, float> ease = null) =>
            Range(This.anchoredPosition, to, duration, ease)
                .Do(x => This.anchoredPosition = x)
                .AsCompletable();

        public static ICompletable TweenAnchorPosXTo(this RectTransform This, float to, float duration, Func<float, float> ease = null) =>
            Range(This.anchoredPosition.x, to, duration, ease)
                .Do(x => This.anchoredPosition = This.anchoredPosition.WithX(x))
                .AsCompletable();

        public static ICompletable TweenAnchorPosYTo(this RectTransform This, float to, float duration, Func<float, float> ease = null) =>
            Range(This.anchoredPosition.y, to, duration, ease)
                .Do(y => This.anchoredPosition = This.anchoredPosition.WithY(y))
                .AsCompletable();

        public static ICompletable TweenScaleTo(this RectTransform This, Vector3 to, float duration, Func<float, float> ease = null) =>
            Range(This.localScale, to, duration, ease)
                .Do(x => This.localScale = x)
                .AsCompletable();

        public static ICompletable TweenScaleTo(this RectTransform This, float to, float duration, Func<float, float> ease = null) =>
            Range(This.localScale, new Vector3(to, to, to), duration, ease)
                .Do(x => This.localScale = x)
                .AsCompletable();
        
        #endregion
    }
}